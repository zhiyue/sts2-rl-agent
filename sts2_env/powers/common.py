"""Common combat powers: Strength, Dexterity, Vulnerable, Weak, Frail."""

from __future__ import annotations

from typing import TYPE_CHECKING

from sts2_env.core.enums import PowerId, PowerType, PowerStackType, ValueProp
from sts2_env.core.constants import VULNERABLE_MULTIPLIER, WEAK_MULTIPLIER, FRAIL_MULTIPLIER
from sts2_env.powers.base import PowerInstance

if TYPE_CHECKING:
    from sts2_env.core.creature import Creature


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
        if dealer is owner and props.is_powered():
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
        if target is owner and props.is_powered():
            return self.amount
        return 0


class VulnerablePower(PowerInstance):
    """Target takes 50% more damage from powered attacks."""

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.VULNERABLE, amount)

    def modify_damage_multiplicative(
        self, owner: Creature, dealer: Creature | None, target: Creature, props: ValueProp
    ) -> float:
        if target is owner and props.is_powered():
            multiplier = VULNERABLE_MULTIPLIER
            if dealer is not None:
                cruelty = dealer.powers.get(PowerId.CRUELTY)
                if cruelty is not None:
                    multiplier += cruelty.amount / 100.0
            if owner.has_power(PowerId.DEBILITATE):
                multiplier += multiplier - 1.0
            return multiplier
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
        if dealer is owner and props.is_powered():
            multiplier = WEAK_MULTIPLIER
            if owner.has_power(PowerId.DEBILITATE):
                multiplier -= 1.0 - multiplier
            return multiplier
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
    ) -> float:
        if target is owner and props.is_powered():
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

    def modify_damage_additive(
        self, owner: Creature, dealer: Creature | None, target: Creature, props: ValueProp
    ) -> int:
        # Shrink reduces the target's damage output (applied to the creature being shrunk)
        if dealer is owner and props.is_powered():
            return -self.amount
        return 0


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
