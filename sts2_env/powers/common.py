"""Common combat powers: Strength, Dexterity, Vulnerable, Weak, Frail."""

from __future__ import annotations

from typing import TYPE_CHECKING

from sts2_env.core.enums import PowerId, PowerType, PowerStackType, ValueProp
from sts2_env.core.constants import VULNERABLE_MULTIPLIER, WEAK_MULTIPLIER, FRAIL_MULTIPLIER
from sts2_env.powers.base import PowerInstance
from sts2_env.relics.base import RelicId

if TYPE_CHECKING:
    from sts2_env.core.creature import Creature

# C# folds these multipliers in `decimal` (exact: 0.75m - 0.15m = 0.60m,
# widened 0.20m).  Python floats can land one ulp below the true value
# (0.75 - 0.15 - (1 - 0.6) = 0.19999999999999996) and the pipeline's
# math.floor then drops a whole point of damage.  Round the folded
# multiplier to 10 digits — the same guard the retired ratio-compensation
# relic listeners used (PaperKrane.FLOAT_ROUND_DIGITS).
MULTIPLIER_ROUND_DIGITS = 10


def _get_relic_with_holder(creature: Creature | None, relic_id: RelicId) -> tuple[object, Creature] | None:
    """Mirror C# ``Player.GetRelic<T>()`` lookups done inside power hooks.

    C# VulnerablePower.cs:39 (``dealer.Player?.GetRelic<PaperPhrog>()``) and
    WeakPower.cs:34 (``target?.Player?.GetRelic<PaperKrane>()``) read the
    attacker's / victim's relic belt from inside the power's multiplicative
    hook.  Pets resolve to their owner's belt, matching C# ``Creature.Player``
    on pet creatures.  Returns ``(relic, holder_creature)`` or None; the
    holder is needed for the Phrog/Krane self-target guards.
    """
    if creature is None:
        return None
    combat = getattr(creature, "combat_state", None)
    if combat is None:
        return None
    holder = creature
    relics = combat.relics_for_creature(holder)
    if not relics:
        owner = getattr(holder, "pet_owner", None)
        if owner is not None:
            holder = owner
            relics = combat.relics_for_creature(owner)
    for relic in relics:
        if relic.relic_id == relic_id and getattr(relic, "enabled", True):
            return relic, holder
    return None


class StrengthPower(PowerInstance):
    """Adds Amount to all powered attack damage dealt by owner."""

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER
    allow_negative = True

    def __init__(self, amount: int):
        super().__init__(PowerId.STRENGTH, amount)

    def modify_damage_additive(
        self, owner: Creature, dealer: Creature | None, target: Creature, props: ValueProp
    ) -> int:
        if dealer is owner and props.is_powered_attack():
            return self.amount
        return 0


class DexterityPower(PowerInstance):
    """Adds Amount to all powered block gained by owner."""

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER
    allow_negative = True

    def __init__(self, amount: int):
        super().__init__(PowerId.DEXTERITY, amount)

    def modify_block_additive(
        self, owner: Creature, target: Creature, props: ValueProp,
        card_source: object | None = None, card_play: object | None = None,
    ) -> int:
        if card_source is not None:
            if getattr(card_source, "owner", None) is not owner:
                return 0
        elif target is not owner:
            return 0
        if not props.is_powered_card_or_monster_move_block():
            return 0
        return self.amount


class VulnerablePower(PowerInstance):
    """Target takes 50% more damage from powered attacks."""

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.VULNERABLE, amount)

    def modify_damage_multiplicative(
        self, owner: Creature, dealer: Creature | None, target: Creature, props: ValueProp
    ) -> float:
        if target is owner and props.is_powered_attack():
            # C# VulnerablePower.ModifyDamageMultiplicative folds every modifier
            # into a single multiplier value: the dealer's PaperPhrog (+0.25)
            # and Cruelty patch the base 1.5 BEFORE Debilitate doubles the
            # excess (VulnerablePower.cs:35-46; PaperPhrog.cs:17-27;
            # DebilitatePower.cs:27-35).  Order matters: with Vuln + Debilitate
            # + Phrog the C# result is (1.5 + 0.25) doubled = 2.5, not
            # 1.5 doubled (+0.25) = 2.25.
            multiplier = VULNERABLE_MULTIPLIER
            phrog = _get_relic_with_holder(dealer, RelicId.PAPER_PHROG)
            if phrog is not None and owner is not phrog[1]:
                multiplier += phrog[0].EXTRA_VULNERABLE
            if dealer is not None:
                cruelty = dealer.powers.get(PowerId.CRUELTY)
                if cruelty is not None:
                    multiplier += cruelty.amount / 100.0
            if owner.has_power(PowerId.DEBILITATE):
                multiplier += multiplier - 1.0
            return round(multiplier, MULTIPLIER_ROUND_DIGITS)
        return 1.0

    def on_turn_end_enemy_side(self, owner: Creature) -> None:
        if self.skip_next_tick:
            self.skip_next_tick = False
            return
        self.amount -= 1


class WeakPower(PowerInstance):
    """Owner deals 25% less damage from powered attacks."""

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.WEAK, amount)

    def modify_damage_multiplicative(
        self, owner: Creature, dealer: Creature | None, target: Creature, props: ValueProp
    ) -> float:
        if dealer is owner and props.is_powered_attack():
            # C# WeakPower.ModifyDamageMultiplicative folds every modifier into
            # a single multiplier value: the victim's PaperKrane (-0.15) patches
            # the base 0.75 BEFORE Debilitate widens the shortfall
            # (WeakPower.cs:33-40; PaperKrane.cs:17-27; DebilitatePower.cs:37-45).
            # Order matters: with Weak + Debilitate + Krane the C# result is
            # (0.75 - 0.15) widened = 0.20, not 0.75 widened (-0.15) = 0.35.
            multiplier = WEAK_MULTIPLIER
            krane = _get_relic_with_holder(target, RelicId.PAPER_KRANE)
            if krane is not None and target is krane[1]:
                multiplier -= krane[0].EXTRA_WEAK
            if owner.has_power(PowerId.DEBILITATE):
                multiplier -= 1.0 - multiplier
            return round(multiplier, MULTIPLIER_ROUND_DIGITS)
        return 1.0

    def on_turn_end_enemy_side(self, owner: Creature) -> None:
        if self.skip_next_tick:
            self.skip_next_tick = False
            return
        self.amount -= 1


class FrailPower(PowerInstance):
    """Owner gains 25% less block from powered sources."""

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.FRAIL, amount)

    def modify_block_multiplicative(
        self, owner: Creature, target: Creature, props: ValueProp,
        card_source: object | None = None, card_play: object | None = None,
        combat: CombatState | None = None,
    ) -> float:
        if target is owner and props.is_powered_card_or_monster_move_block():
            return FRAIL_MULTIPLIER
        return 1.0

    def on_turn_end_enemy_side(self, owner: Creature) -> None:
        if self.skip_next_tick:
            self.skip_next_tick = False
            return
        self.amount -= 1


class ArtifactPower(PowerInstance):
    """Blocks one debuff application per stack."""

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER
    should_scale_in_multiplayer = True

    def __init__(self, amount: int):
        super().__init__(PowerId.ARTIFACT, amount)

    def try_block_debuff(self, owner: Creature, power_id: PowerId) -> bool:
        if self.amount > 0:
            self.amount -= 1
            return True
        return False


class PlatingPower(PowerInstance):
    """Gain Amount block at end of turn, ticks down."""

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER
    should_scale_in_multiplayer = True

    def __init__(self, amount: int):
        super().__init__(PowerId.PLATING, amount)

    def on_turn_end_enemy_side(self, owner: Creature) -> None:
        if self.skip_next_tick:
            self.skip_next_tick = False
            return
        self.amount -= 1


class RitualPower(PowerInstance):
    """Gain Amount Strength at start of turn."""

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.RITUAL, amount)

    def on_turn_start_own_side(self, owner: Creature, combat: object) -> None:
        owner.apply_power(PowerId.STRENGTH, self.amount)


class ShrinkPower(PowerInstance):
    """Reduces Strength (applied as negative Strength-like debuff via custom power)."""

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER
    allow_negative = True

    def __init__(self, amount: int):
        super().__init__(PowerId.SHRINK, amount)

    def modify_damage_multiplicative(
        self, owner: Creature, dealer: Creature | None, target: Creature, props: ValueProp
    ) -> float:
        if dealer is owner and props.is_powered_attack():
            return 0.7
        return 1.0

    def after_turn_end(self, owner: Creature, side: object, combat: object) -> None:
        if self.amount >= 0 and side == owner.side:
            self.amount -= 1

    def after_death(
        self,
        owner: Creature,
        creature: Creature,
        combat: object,
        was_removal_prevented: bool = False,
    ) -> None:
        if not was_removal_prevented and creature is self.applier:
            owner.powers.pop(self.power_id, None)


# Register all powers with the creature registry
from sts2_env.core.creature import register_power_class

_ALL_POWERS: dict[PowerId, type[PowerInstance]] = {
    PowerId.STRENGTH: StrengthPower,
    PowerId.DEXTERITY: DexterityPower,
    PowerId.VULNERABLE: VulnerablePower,
    PowerId.WEAK: WeakPower,
    PowerId.FRAIL: FrailPower,
    PowerId.ARTIFACT: ArtifactPower,
    PowerId.PLATING: PlatingPower,
    PowerId.RITUAL: RitualPower,
    PowerId.SHRINK: ShrinkPower,
}

for _pid, _cls in _ALL_POWERS.items():
    register_power_class(_pid, _cls)

# Legacy alias for backward compatibility
POWER_CLASSES = _ALL_POWERS
