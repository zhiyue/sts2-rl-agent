"""Remaining powers batch A (46 powers).

Each power is verified against the decompiled C# source in
decompiled/MegaCrit.Sts2.Core.Models.Powers/.
"""

from __future__ import annotations

import random
from typing import TYPE_CHECKING

from sts2_env.core.enums import (
    CardKeyword,
    CardTag,
    CardType,
    CombatSide,
    PowerId,
    PowerType,
    PowerStackType,
    ValueProp,
)
from sts2_env.powers.base import PowerInstance

if TYPE_CHECKING:
    from sts2_env.core.creature import Creature
    from sts2_env.core.combat import CombatState


# =====================================================================
#  1. AccelerantPower
# =====================================================================
class AccelerantPower(PowerInstance):
    """Buff/counter. Increases the number of times Poison ticks each turn.

    C# ref: AccelerantPower.cs
    StackType.Counter. The actual repeat-tick logic lives in PoisonPower;
    this power is a counter that PoisonPower reads.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.ACCELERANT, amount)

    # PoisonPower checks owner.get_power(ACCELERANT) to determine extra ticks.
    # No hook methods needed -- this is a passive counter.


# =====================================================================
#  2. AdaptablePower
# =====================================================================
class AdaptablePower(PowerInstance):
    """Buff/single. Monster revives after death (TestSubject boss mechanic).

    C# ref: AdaptablePower.cs
    - AfterDeath: triggers dead-state / revive on TestSubject.
    - ShouldAllowHitting: false while reviving.
    - ShouldStopCombatFromEnding: true.
    - ShouldCreatureBeRemovedFromCombatAfterDeath: false for owner.
    StackType.Single.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.SINGLE

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.ADAPTABLE, amount)
        self.is_reviving: bool = False

    def on_owner_death(self, owner: Creature, combat: CombatState) -> bool:
        """Returns True to prevent creature removal (triggers revive)."""
        self.is_reviving = True
        return True

    def do_revive(self) -> None:
        """Called by the encounter system when the revive completes."""
        self.is_reviving = False

    def should_stop_combat_ending(self) -> bool:
        return True


# =====================================================================
#  3. AnticipatePower  (TemporaryDexterity variant)
# =====================================================================
class AnticipatePower(PowerInstance):
    """Temporary Dexterity: grants Dexterity on application, removes it
    at end of owner's turn.

    C# ref: AnticipatePower.cs extends TemporaryDexterityPower (IsPositive=true).
    - BeforeApplied: applies +Amount Dexterity.
    - AfterTurnEnd (owner side): removes self and applies -Amount Dexterity.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.ANTICIPATE, amount)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            owner.apply_power(PowerId.DEXTERITY, -self.amount)
            self.amount = 0


# =====================================================================
#  4. ArsenalPower
# =====================================================================
class ArsenalPower(PowerInstance):
    """After playing a Colorless card, gain Amount Strength.

    C# ref: ArsenalPower.cs
    - AfterCardPlayed: if card is colorless and owned by owner's player,
      apply StrengthPower(Amount) to owner.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.ARSENAL, amount)

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        is_colorless = getattr(card, "is_colorless", False)
        card_owner = getattr(card, "owner", None)
        if is_colorless and (card_owner is owner or card_owner is None):
            owner.apply_power(PowerId.STRENGTH, self.amount)


# =====================================================================
#  5. AutomationPower
# =====================================================================
class AutomationPower(PowerInstance):
    """After every 10 cards drawn, gain Amount energy.

    C# ref: AutomationPower.cs
    - AfterCardDrawn: decrement internal counter; at 0, gain energy and reset.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    _BASE_CARDS = 10

    def __init__(self, amount: int):
        super().__init__(PowerId.AUTOMATION, amount)
        self.cards_left: int = self._BASE_CARDS

    def on_card_drawn(self, owner: Creature, card: object, combat: CombatState) -> None:
        """Called by the draw pipeline when a card is drawn."""
        card_owner = getattr(card, "owner", None)
        if card_owner is not owner and card_owner is not None:
            return
        self.cards_left -= 1
        if self.cards_left <= 0:
            combat.gain_energy(owner, self.amount)
            self.cards_left = self._BASE_CARDS


# =====================================================================
#  6. BattlewornDummyTimeLimitPower
# =====================================================================
class BattlewornDummyTimeLimitPower(PowerInstance):
    """Countdown timer for the Battleworn Dummy event. Decrements at end
    of owner's turn; at 1, the creature escapes.

    C# ref: BattlewornDummyTimeLimitPower.cs
    - AfterTurnEnd (owner side): decrement; at 1 -> creature escapes.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.BATTLEWORN_DUMMY_TIME_LIMIT, amount)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side != owner.side:
            return
        if self.amount > 1:
            self.amount -= 1
            return
        # Time's up -- creature escapes
        escape = getattr(combat, "creature_escape", None)
        if escape is not None:
            escape(owner)
        self.amount = 0


# =====================================================================
#  7. BeaconOfHopePower
# =====================================================================
class BeaconOfHopePower(PowerInstance):
    """When owner gains block during their own turn, grant 50% * Amount of
    that block (unpowered) to all teammates.

    C# ref: BeaconOfHopePower.cs
    - AfterBlockGained: if creature == owner and on owner's side turn,
      give 50% * Amount of gained block to each teammate.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.BEACON_OF_HOPE, amount)

    def after_block_gained(self, owner: Creature, creature: Creature,
                           amount: int, combat: CombatState) -> None:
        if creature is not owner or amount < 1:
            return
        current_side = getattr(combat, "current_side", None)
        if current_side != owner.side:
            return
        share = int(amount * 0.5 * self.amount)
        if share < 1:
            return
        teammates = getattr(combat, "get_teammates_of", None)
        if teammates is not None:
            for ally in teammates(owner):
                if ally is not owner and getattr(ally, "is_alive", True):
                    ally.gain_block(share)


# =====================================================================
#  8. BlackHolePower
# =====================================================================
class BlackHolePower(PowerInstance):
    """Deal Amount damage (unpowered) to all enemies whenever stars are
    spent on a card or whenever stars are gained.

    C# ref: BlackHolePower.cs
    - AfterCardPlayed: if stars were spent, damage all enemies.
    - AfterStarsGained: if stars > 0 gained, damage all enemies.
    StackType.Counter.

    # Simplified: Stars system is Regent-specific. We expose a trigger
    # method the combat system calls when stars are spent or gained.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.BLACK_HOLE, amount)

    def _deal_damage_to_all_enemies(self, owner: Creature, combat: CombatState) -> None:
        for enemy in combat.get_enemies_of(owner):
            if getattr(enemy, "is_alive", True):
                combat.deal_damage(
                    dealer=owner,
                    target=enemy,
                    amount=self.amount,
                    props=ValueProp.UNPOWERED,
                )

    def on_stars_spent(self, owner: Creature, stars: int, combat: CombatState) -> None:
        """Called by the combat system when stars are spent on a card."""
        if stars > 0:
            self._deal_damage_to_all_enemies(owner, combat)

    def on_stars_gained(self, owner: Creature, stars: int, combat: CombatState) -> None:
        """Called by the combat system when stars are gained."""
        if stars > 0:
            self._deal_damage_to_all_enemies(owner, combat)


# =====================================================================
#  9. BladeOfInkPower
# =====================================================================
class BladeOfInkPower(PowerInstance):
    """Each Attack card played this turn grants +Amount Strength (temporary).
    At end of turn, remove the accumulated Strength and remove self.

    C# ref: BladeOfInkPower.cs
    - AfterCardPlayed: if Attack, apply StrengthPower(Amount) silently.
    - AfterTurnEnd (owner side): apply -strength_applied, remove self.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.BLADE_OF_INK, amount)
        self._strength_applied: int = 0

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        card_type = getattr(card, "card_type", None) or getattr(card, "type", None)
        card_owner = getattr(card, "owner", None)
        if card_owner is owner and card_type == CardType.ATTACK:
            owner.apply_power(PowerId.STRENGTH, self.amount)
            self._strength_applied += self.amount

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            if self._strength_applied != 0:
                owner.apply_power(PowerId.STRENGTH, -self._strength_applied)
            self._strength_applied = 0
            self.amount = 0  # signal removal


# =====================================================================
#  10. CallOfTheVoidPower
# =====================================================================
class CallOfTheVoidPower(PowerInstance):
    """At start of turn, generate Amount random (non-basic, non-ancient)
    cards with Ethereal into hand.

    C# ref: CallOfTheVoidPower.cs
    - BeforeHandDraw: generate Amount distinct cards from card pool,
      apply Ethereal, add to hand.
    StackType.Counter.

    # Simplified: delegates card generation to combat system.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.CALL_OF_THE_VOID, amount)

    def before_side_turn_start(self, owner: Creature, side: CombatSide,
                               combat: CombatState) -> None:
        if side != owner.side or not getattr(owner, "is_player", False):
            return
        gen = getattr(combat, "generate_random_cards_to_hand", None)
        if gen is not None:
            gen(owner, None, self.amount, ethereal=True)


# =====================================================================
#  11. ChainsOfBindingPower
# =====================================================================
class ChainsOfBindingPower(PowerInstance):
    """Debuff. When owner draws a card during their turn, the first Amount
    cards drawn are "Bound" (can only be played once this turn). Bound
    afflictions clear at end of turn.

    C# ref: ChainsOfBindingPower.cs
    - AfterCardDrawn: afflict drawn card with Bound (up to Amount per turn).
    - ShouldPlay: prevent playing a second Bound card.
    - BeforeTurnEnd: clear Bound afflictions.
    StackType.Counter.

    # Simplified: tracks bound state. Card system should check this power.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.CHAINS_OF_BINDING, amount)
        self._bound_cards_this_turn: int = 0
        self._bound_card_played: bool = False

    def on_card_drawn(self, owner: Creature, card: object, combat: CombatState) -> None:
        """Called by draw pipeline. Afflicts the card if limit not reached."""
        current_side = getattr(combat, "current_side", None)
        if current_side != owner.side:
            return
        if self._bound_cards_this_turn < self.amount:
            if hasattr(card, "bound"):
                card.bound = True
            self._bound_cards_this_turn += 1

    def before_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            self._bound_card_played = False
            self._bound_cards_this_turn = 0

    def before_side_turn_start(self, owner: Creature, side: CombatSide,
                               combat: CombatState) -> None:
        if side == owner.side:
            self._bound_cards_this_turn = 0
            self._bound_card_played = False


# =====================================================================
#  12. ChildOfTheStarsPower
# =====================================================================
class ChildOfTheStarsPower(PowerInstance):
    """Whenever stars are spent, gain Amount * stars_spent block (unpowered).

    C# ref: ChildOfTheStarsPower.cs
    - AfterStarsSpent: gain Amount * spent_amount block.
    StackType.Counter.

    # Simplified: Regent star system. Exposes trigger method.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.CHILD_OF_THE_STARS, amount)

    def on_stars_spent(self, owner: Creature, stars: int, combat: CombatState) -> None:
        """Called by the combat system when stars are spent."""
        if stars > 0:
            owner.gain_block(self.amount * stars)


# =====================================================================
#  13. ClarityPower
# =====================================================================
class ClarityPower(PowerInstance):
    """Draw 1 extra card per turn. Decrements each turn.

    C# ref: ClarityPower.cs
    - ModifyHandDraw: +1 card.
    - AfterSideTurnStart: decrement (removes at 0).
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.CLARITY, amount)

    def modify_hand_draw(self, owner: Creature, draw: int) -> int:
        return draw + 1

    def after_side_turn_start(self, owner: Creature, side: CombatSide,
                              combat: CombatState) -> None:
        if side == owner.side:
            self.amount -= 1


# =====================================================================
#  14. ConfusedPower
# =====================================================================
class ConfusedPower(PowerInstance):
    """Debuff/single. When a card is drawn, randomize its energy cost (0-3).

    C# ref: ConfusedPower.cs
    - AfterCardDrawn: set card energy cost to random 0-3.
    StackType.Single.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.SINGLE

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.CONFUSED, amount)

    def on_card_drawn(self, owner: Creature, card: object, combat: CombatState) -> None:
        """Called by draw pipeline. Randomizes card cost."""
        card_owner = getattr(card, "owner", None)
        if card_owner is not owner and card_owner is not None:
            return
        canonical_cost = getattr(card, "energy_cost", -1)
        if canonical_cost is not None and canonical_cost < 0:
            return
        new_cost = random.randint(0, 3)
        if hasattr(card, "energy_cost"):
            card.energy_cost = new_cost
        elif hasattr(card, "set_combat_cost"):
            card.set_combat_cost(new_cost)


# =====================================================================
#  15. ConquerorPower
# =====================================================================
class ConquerorPower(PowerInstance):
    """Debuff/duration. Sovereign Blade cards deal 2x damage to the owner.
    Ticks down at end of owner's turn.

    C# ref: ConquerorPower.cs
    - ModifyDamageMultiplicative: 2x for Sovereign Blade against owner.
    - AfterTurnEnd (owner side): tick down duration.
    StackType.Counter (used as duration).
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.CONQUEROR, amount)

    def modify_damage_multiplicative(
        self, owner: Creature, dealer: Creature | None, target: Creature, props: ValueProp
    ) -> float:
        if target is not owner:
            return 1.0
        if not props.is_powered():
            return 1.0
        # In the full engine, only SovereignBlade cards trigger this.
        # The card system should set a flag. We expose a helper.
        return 1.0  # Default; see modify_damage_for_card below

    def modify_damage_for_sovereign_blade(
        self, owner: Creature, target: Creature, props: ValueProp
    ) -> float:
        """Called by the damage pipeline when the card source is Sovereign Blade."""
        if target is owner and props.is_powered():
            return 2.0
        return 1.0

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            self.amount -= 1


# =====================================================================
#  16. ConsumingShadowPower
# =====================================================================
class ConsumingShadowPower(PowerInstance):
    """At end of owner's turn, evoke the last orb Amount times.

    C# ref: ConsumingShadowPower.cs
    - AfterTurnEnd (owner side): evoke last orb Amount times if orbs exist.
    StackType.Counter.

    # Simplified: delegates orb evocation to combat system.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.CONSUMING_SHADOW, amount)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side != owner.side:
            return
        evoke = getattr(combat, "evoke_last_orb", None)
        if evoke is not None:
            for _ in range(self.amount):
                evoke(owner)


# =====================================================================
#  17. CoolantPower
# =====================================================================
class CoolantPower(PowerInstance):
    """At start of owner's turn, gain block equal to (distinct orb types) * Amount.

    C# ref: CoolantPower.cs
    - AfterSideTurnStart (owner side): count distinct orb types, gain
      block = count * Amount (unpowered).
    StackType.Counter.

    # Simplified: delegates orb counting to combat system.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.COOLANT, amount)

    def after_side_turn_start(self, owner: Creature, side: CombatSide,
                              combat: CombatState) -> None:
        if side != owner.side:
            return
        distinct_orbs = getattr(combat, "count_distinct_orb_types", None)
        if distinct_orbs is not None:
            count = distinct_orbs(owner)
            if count > 0:
                owner.gain_block(count * self.amount)


# =====================================================================
#  18. CorrosiveWavePower
# =====================================================================
class CorrosiveWavePower(PowerInstance):
    """Whenever the owner draws a card, apply Amount Poison to all enemies.
    Removed at end of owner's turn.

    C# ref: CorrosiveWavePower.cs
    - AfterCardDrawn: apply PoisonPower(Amount) to all hittable enemies.
    - AfterTurnEnd (owner side): remove self.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.CORROSIVE_WAVE, amount)

    def on_card_drawn(self, owner: Creature, card: object, combat: CombatState) -> None:
        """Called by draw pipeline. Applies Poison to all enemies."""
        card_owner = getattr(card, "owner", None)
        if card_owner is not None and card_owner is not owner:
            return
        for enemy in combat.get_enemies_of(owner):
            if getattr(enemy, "is_alive", True):
                enemy.apply_power(PowerId.POISON, self.amount)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            self.amount = 0  # signal removal


# =====================================================================
#  19. CrimsonMantlePower
# =====================================================================
class CrimsonMantlePower(PowerInstance):
    """At start of owner's player turn, deal self-damage (unblockable,
    unpowered) then gain Amount block (unpowered). Self-damage increments
    over time via IncrementSelfDamage().

    C# ref: CrimsonMantlePower.cs
    - AfterPlayerTurnStart: deal SelfDamage to self, then gain Amount block.
    - IncrementSelfDamage(): increases the self-damage each call.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.CRIMSON_MANTLE, amount)
        self.self_damage: int = 0

    def after_side_turn_start(self, owner: Creature, side: CombatSide,
                              combat: CombatState) -> None:
        if side != owner.side:
            return
        if not getattr(owner, "is_player", False):
            return
        if self.self_damage > 0:
            combat.deal_damage(
                dealer=owner,
                target=owner,
                amount=self.self_damage,
                props=ValueProp.UNBLOCKABLE | ValueProp.UNPOWERED,
            )
        owner.gain_block(self.amount)

    def increment_self_damage(self) -> None:
        """Called by the card system when Crimson Mantle card is played."""
        self.self_damage += 1


# =====================================================================
#  20. CrushUnderPower  (TemporaryStrength variant -- negative)
# =====================================================================
class CrushUnderPower(PowerInstance):
    """Negative temporary Strength (debuff). Applies -Amount Strength on
    application. At end of turn, removes self and restores the Strength.

    C# ref: CrushUnderPower.cs extends TemporaryStrengthPower (IsPositive=false).
    StackType.Counter.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.CRUSH_UNDER, amount)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            # Reverse: the initial application reduced Strength by Amount,
            # so now we restore it.
            owner.apply_power(PowerId.STRENGTH, self.amount)
            self.amount = 0  # signal removal


# =====================================================================
#  21. CuriousPower
# =====================================================================
class CuriousPower(PowerInstance):
    """Reduces the energy cost of Power-type cards by Amount.

    C# ref: CuriousPower.cs
    - TryModifyEnergyCostInCombat: if card is Power type with cost > 0,
      reduce by Amount (min 0).
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.CURIOUS, amount)

    def modify_card_cost(self, owner: Creature, card: object) -> int | None:
        """Return modified cost for Power cards, None otherwise."""
        card_type = getattr(card, "card_type", None) or getattr(card, "type", None)
        if card_type != CardType.POWER:
            return None
        original_cost = getattr(card, "energy_cost", 0)
        if original_cost is None or original_cost <= 0:
            return None
        return max(0, original_cost - self.amount)


# =====================================================================
#  22. DampenPower
# =====================================================================
class DampenPower(PowerInstance):
    """Debuff. On application, downgrades all upgraded cards. When all
    casters die (or power is removed), re-upgrades them.

    C# ref: DampenPower.cs
    - AfterApplied: downgrade all upgraded cards.
    - AfterDeath: if all casters dead, remove self.
    - AfterRemoved: re-upgrade all downgraded cards.
    StackType.None (does not stack).

    # Simplified: tracks downgraded state. Card upgrade/downgrade is
    # delegated to the combat system.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.DAMPEN, amount)
        self._active: bool = False

    def on_applied(self, owner: Creature, combat: CombatState) -> None:
        """Called when the power is first applied. Downgrades cards."""
        self._active = True
        downgrade = getattr(combat, "downgrade_all_cards", None)
        if downgrade is not None:
            downgrade(owner)

    def on_removed(self, owner: Creature, combat: CombatState) -> None:
        """Called when the power is removed. Re-upgrades cards."""
        if self._active:
            upgrade = getattr(combat, "restore_downgraded_cards", None)
            if upgrade is not None:
                upgrade(owner)
            self._active = False


# =====================================================================
#  23. DanseMacabrePower
# =====================================================================
class DanseMacabrePower(PowerInstance):
    """Before playing a card that costs >= 2 energy, gain Amount block (unpowered).

    C# ref: DanseMacabrePower.cs
    - BeforeCardPlayed: if card cost >= Energy DynamicVar (default 2),
      gain Amount block.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    _ENERGY_THRESHOLD = 2

    def __init__(self, amount: int):
        super().__init__(PowerId.DANSE_MACABRE, amount)

    def before_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        card_owner = getattr(card, "owner", None)
        if card_owner is not None and card_owner is not owner:
            return
        cost = getattr(card, "energy_cost", 0) or 0
        if cost >= self._ENERGY_THRESHOLD:
            owner.gain_block(self.amount)


# =====================================================================
#  24. DarkShacklesPower  (TemporaryStrength variant -- negative)
# =====================================================================
class DarkShacklesPower(PowerInstance):
    """Negative temporary Strength (debuff). Same as CrushUnder.

    C# ref: DarkShacklesPower.cs extends TemporaryStrengthPower (IsPositive=false).
    StackType.Counter.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.DARK_SHACKLES, amount)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            owner.apply_power(PowerId.STRENGTH, self.amount)
            self.amount = 0  # signal removal


# =====================================================================
#  25. DemesnePower
# =====================================================================
class DemesnePower(PowerInstance):
    """Draw Amount extra cards each turn AND gain Amount extra max energy.

    C# ref: DemesnePower.cs
    - ModifyHandDraw: +Amount.
    - ModifyMaxEnergy: +Amount.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.DEMESNE, amount)

    def modify_hand_draw(self, owner: Creature, draw: int) -> int:
        return draw + self.amount

    def modify_max_energy(self, owner: Creature, energy: int) -> int:
        return energy + self.amount


# =====================================================================
#  26. DemisePower
# =====================================================================
class DemisePower(PowerInstance):
    """Debuff. At end of owner's turn, deal Amount unblockable/unpowered
    damage to self.

    C# ref: DemisePower.cs
    - AfterTurnEnd (owner side): deal Amount damage (unblockable+unpowered)
      to owner with no dealer.
    StackType.Counter.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.DEMISE, amount)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            combat.deal_damage(
                dealer=None,
                target=owner,
                amount=self.amount,
                props=ValueProp.UNBLOCKABLE | ValueProp.UNPOWERED,
            )


# =====================================================================
#  27. DevourLifePower
# =====================================================================
class DevourLifePower(PowerInstance):
    """When the owner plays a Soul card, summon Amount Osty minion(s).

    C# ref: DevourLifePower.cs
    - AfterCardPlayed: if card is Soul and owned by owner, summon Amount osties.
    StackType.Counter.

    # Simplified: Necrobinder-specific. Soul card + Osty summon system.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.DEVOUR_LIFE, amount)

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        card_id = getattr(card, "card_id", None)
        card_owner = getattr(card, "owner", None)
        is_soul = (card_id is not None and str(card_id) == "SOUL") or \
                  getattr(card, "is_soul", False)
        if is_soul and (card_owner is owner or card_owner is None):
            summon = getattr(combat, "summon_osty", None)
            if summon is not None:
                summon(owner, self.amount)


# =====================================================================
#  28. DiamondDiademPower
# =====================================================================
class DiamondDiademPower(PowerInstance):
    """Buff/single. Owner takes 50% damage from powered attacks.
    Removed at end of enemy turn.

    C# ref: DiamondDiademPower.cs
    - ModifyDamageMultiplicative: 0.5 for powered attacks targeting owner.
    - AfterTurnEnd (Enemy side): remove self.
    StackType.Single.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.SINGLE

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.DIAMOND_DIADEM, amount)

    def modify_damage_multiplicative(
        self, owner: Creature, dealer: Creature | None, target: Creature, props: ValueProp
    ) -> float:
        if target is owner and props.is_powered():
            return 0.5
        return 1.0

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.ENEMY:
            self.amount = 0  # signal removal


# =====================================================================
#  29. DieForYouPower
# =====================================================================
class DieForYouPower(PowerInstance):
    """Buff/single. Redirects powered attack damage targeting the pet owner
    to this creature (the pet) instead.

    C# ref: DieForYouPower.cs
    - ModifyUnblockedDamageTarget: redirect damage from pet owner to self.
    - ShouldCreatureBeRemovedFromCombatAfterDeath: false for owner.
    - ShouldPowerBeRemovedAfterOwnerDeath: false.
    StackType.Single.

    # Simplified: The pet/owner relationship is model-specific. This power
    # exposes a redirect method the damage pipeline can call.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.SINGLE

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.DIE_FOR_YOU, amount)

    def should_redirect_damage(
        self, owner: Creature, target: Creature, props: ValueProp
    ) -> Creature | None:
        """If target is the pet's owner and owner (pet) is alive, return
        owner (pet) as the new target. Otherwise return None."""
        pet_owner = getattr(owner, "pet_owner", None)
        if pet_owner is None:
            return None
        pet_owner_creature = getattr(pet_owner, "creature", pet_owner)
        if target is not pet_owner_creature:
            return None
        if not getattr(owner, "is_alive", True):
            return None
        if not props.is_powered():
            return None
        return owner


# =====================================================================
#  30. DisintegrationPower
# =====================================================================
class DisintegrationPower(PowerInstance):
    """Debuff. At the end of owner's turn (late phase), deal Amount damage
    (unpowered) to self.

    C# ref: DisintegrationPower.cs
    - AfterTurnEndLate (owner side): deal Amount unpowered damage to self.
    StackType.Counter.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.DISINTEGRATION, amount)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            combat.deal_damage(
                dealer=owner,
                target=owner,
                amount=self.amount,
                props=ValueProp.UNPOWERED,
            )


# =====================================================================
#  31. DyingStarPower  (TemporaryStrength variant -- negative)
# =====================================================================
class DyingStarPower(PowerInstance):
    """Negative temporary Strength (debuff). Same as CrushUnder/DarkShackles.

    C# ref: DyingStarPower.cs extends TemporaryStrengthPower (IsPositive=false).
    StackType.Counter.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.DYING_STAR, amount)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            owner.apply_power(PowerId.STRENGTH, self.amount)
            self.amount = 0  # signal removal


# =====================================================================
#  32. EnfeeblingTouchPower  (TemporaryStrength variant -- negative)
# =====================================================================
class EnfeeblingTouchPower(PowerInstance):
    """Negative temporary Strength (debuff). Same as CrushUnder/DarkShackles.

    C# ref: EnfeeblingTouchPower.cs extends TemporaryStrengthPower (IsPositive=false).
    StackType.Counter.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.ENFEEBLING_TOUCH, amount)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            owner.apply_power(PowerId.STRENGTH, self.amount)
            self.amount = 0  # signal removal


# =====================================================================
#  33. EntropyPower
# =====================================================================
class EntropyPower(PowerInstance):
    """At start of owner's turn, transform Amount cards from hand into
    random cards.

    C# ref: EntropyPower.cs
    - AfterPlayerTurnStart: select Amount cards from hand, transform each
      to a random card.
    StackType.Counter.

    # Simplified: Card selection/transform delegated to combat system.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.ENTROPY, amount)

    def after_side_turn_start(self, owner: Creature, side: CombatSide,
                              combat: CombatState) -> None:
        if side != owner.side:
            return
        if not getattr(owner, "is_player", False):
            return
        transform = getattr(combat, "transform_cards_from_hand", None)
        if transform is not None:
            transform(owner, self.amount)


# =====================================================================
#  34. EscapeArtistPower
# =====================================================================
class EscapeArtistPower(PowerInstance):
    """Countdown buff. Decrements at end of owner's turn. Starts pulsing
    at 1 (visual indicator that the creature will escape next turn).

    C# ref: EscapeArtistPower.cs
    - AfterTurnEnd (owner side): decrement if > 1. At 1, start pulsing.
    StackType.Counter.

    The actual escape logic is in the monster's move state machine.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.ESCAPE_ARTIST, amount)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            if self.amount > 1:
                self.amount -= 1


# =====================================================================
#  35. FastenPower
# =====================================================================
class FastenPower(PowerInstance):
    """Adds Amount block to Defend-tagged cards (powered block from cards
    with the Defend tag).

    C# ref: FastenPower.cs
    - ModifyBlockAdditive: +Amount if target is owner, powered card block,
      and card has Defend tag.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.FASTEN, amount)

    def modify_block_additive(
        self, owner: Creature, target: Creature, props: ValueProp,
        card_source: object | None = None, card_play: object | None = None,
    ) -> int:
        if target is not owner:
            return 0
        if not props.is_powered():
            return 0
        # The card context is not directly available in this hook.
        # The combat system should call modify_block_for_card instead.
        return 0

    def modify_block_for_defend(
        self, owner: Creature, target: Creature, props: ValueProp, card: object
    ) -> int:
        """Called by the block pipeline when the card source is known."""
        if target is not owner:
            return 0
        if not props.is_powered():
            return 0
        tags = getattr(card, "tags", set())
        if CardTag.DEFEND not in tags:
            return 0
        return self.amount


# =====================================================================
#  36. FeralPower
# =====================================================================
class FeralPower(PowerInstance):
    """The first Amount 0-cost Attack cards played each turn return to hand
    instead of going to discard.

    C# ref: FeralPower.cs
    - ModifyCardPlayResultPileTypeAndPosition: return to hand if 0-cost
      Attack and limit not reached.
    - AfterSideTurnStart: reset counter.
    StackType.Counter.

    # Simplified: exposes a check the card-play system calls.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.FERAL, amount)
        self._zero_cost_attacks_played: int = 0

    def should_return_to_hand(self, owner: Creature, card: object, energy_spent: int) -> bool:
        """Return True if this 0-cost attack should return to hand."""
        card_type = getattr(card, "card_type", None) or getattr(card, "type", None)
        if card_type != CardType.ATTACK:
            return False
        if energy_spent > 0:
            return False
        if self._zero_cost_attacks_played >= self.amount:
            return False
        self._zero_cost_attacks_played += 1
        return True

    def before_side_turn_start(self, owner: Creature, side: CombatSide,
                               combat: CombatState) -> None:
        if side == owner.side:
            self._zero_cost_attacks_played = 0


# =====================================================================
#  37. FlankingPower
# =====================================================================
class FlankingPower(PowerInstance):
    """Debuff. Damage from powered attacks by players OTHER than the
    applier is multiplied by Amount. Removed at end of owner's turn.

    C# ref: FlankingPower.cs
    - ModifyDamageMultiplicative: Amount multiplier for attacks on owner
      from dealers that are NOT the applier.
    - AfterTurnEnd (owner side): remove self.
    StackType.Counter. Instanced.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.FLANKING, amount)
        self.applier: Creature | None = None

    def modify_damage_multiplicative(
        self, owner: Creature, dealer: Creature | None, target: Creature, props: ValueProp
    ) -> float:
        if target is not owner:
            return 1.0
        if not props.is_powered():
            return 1.0
        if dealer is self.applier:
            return 1.0
        return float(self.amount)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            self.amount = 0  # signal removal


# =====================================================================
#  38. FlutterPower
# =====================================================================
class FlutterPower(PowerInstance):
    """Owner takes 50% damage from powered attacks. When hit with unblocked
    powered damage, decrement. At 0, the owner is stunned.

    C# ref: FlutterPower.cs
    - ModifyDamageMultiplicative: 0.5 (DamageDecrease/100) for powered
      attacks targeting owner.
    - AfterDamageReceived: decrement on unblocked powered hit; stun at 0.
    StackType.Counter.

    # Simplified: stun is handled by monster AI.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    _DAMAGE_DECREASE_PCT = 50

    def __init__(self, amount: int):
        super().__init__(PowerId.FLUTTER, amount)

    def modify_damage_multiplicative(
        self, owner: Creature, dealer: Creature | None, target: Creature, props: ValueProp
    ) -> float:
        if target is owner and props.is_powered():
            return self._DAMAGE_DECREASE_PCT / 100.0
        return 1.0

    def after_damage_received(
        self, owner: Creature, target: Creature, dealer: Creature | None,
        damage: int, props: ValueProp, combat: CombatState
    ) -> None:
        if target is owner and damage > 0 and props.is_powered():
            self.amount -= 1
            # At 0, stun is handled by the monster AI


# =====================================================================
#  39. FocusPower
# =====================================================================
class FocusPower(PowerInstance):
    """Modifies orb passive/evoke values by Amount. AllowNegative.

    C# ref: FocusPower.cs
    - ModifyOrbValue: add Amount to orb value (min 0).
    StackType.Counter. AllowNegative = true.

    # The orb system reads this power's amount when calculating orb values.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER
    allow_negative = True

    def __init__(self, amount: int):
        super().__init__(PowerId.FOCUS, amount)


# =====================================================================
#  40. ForbiddenGrimoirePower
# =====================================================================
class ForbiddenGrimoirePower(PowerInstance):
    """After combat ends, offer Amount card removal(s) as a reward.

    C# ref: ForbiddenGrimoirePower.cs
    - AfterCombatEnd: add Amount CardRemovalReward(s).
    StackType.Counter.

    # Simplified: the reward system reads this power post-combat.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.FORBIDDEN_GRIMOIRE, amount)

    def after_combat_victory(self, owner: Creature, combat: CombatState) -> None:
        add_reward = getattr(combat, "add_card_removal_reward", None)
        if add_reward is not None:
            for _ in range(self.amount):
                add_reward(owner)


# =====================================================================
#  41. ForegoneConclusionPower
# =====================================================================
class ForegoneConclusionPower(PowerInstance):
    """Before hand draw, choose Amount cards from the draw pile to put
    into hand. Then remove self.

    C# ref: ForegoneConclusionPower.cs
    - BeforeHandDraw: shuffle if necessary, let player pick Amount cards
      from draw pile -> hand. Remove self.
    StackType.Counter.

    # Simplified: card selection delegated to combat system.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.FOREGONE_CONCLUSION, amount)

    def before_side_turn_start(self, owner: Creature, side: CombatSide,
                               combat: CombatState) -> None:
        if side != owner.side or not getattr(owner, "is_player", False):
            return
        select = getattr(combat, "select_cards_from_draw_to_hand", None)
        if select is not None:
            select(owner, self.amount)
        self.amount = 0  # signal removal


# =====================================================================
#  42. FriendshipPower
# =====================================================================
class FriendshipPower(PowerInstance):
    """Gain Amount extra max energy each turn.

    C# ref: FriendshipPower.cs
    - ModifyMaxEnergy: +Amount.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.FRIENDSHIP, amount)

    def modify_max_energy(self, owner: Creature, energy: int) -> int:
        return energy + self.amount


# =====================================================================
#  43. FurnacePower
# =====================================================================
class FurnacePower(PowerInstance):
    """At start of owner's turn, forge Amount times (upgrade a card in hand).

    C# ref: FurnacePower.cs
    - AfterSideTurnStart (owner side): forge Amount times.
    StackType.Counter.

    # Simplified: Regent forge mechanic. Delegates to combat system.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.FURNACE, amount)

    def after_side_turn_start(self, owner: Creature, side: CombatSide,
                              combat: CombatState) -> None:
        if side != owner.side:
            return
        forge = getattr(combat, "forge", None)
        if forge is not None:
            forge(owner, self.amount)


# =====================================================================
#  44. GenesisPower
# =====================================================================
class GenesisPower(PowerInstance):
    """At energy reset (start of turn), gain Amount stars.

    C# ref: GenesisPower.cs
    - AfterEnergyReset: gain Amount stars for owner's player.
    StackType.Counter.

    # Simplified: Regent star mechanic. Delegates to combat system.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.GENESIS, amount)

    def after_side_turn_start(self, owner: Creature, side: CombatSide,
                              combat: CombatState) -> None:
        if side != owner.side or not getattr(owner, "is_player", False):
            return
        gain_stars = getattr(combat, "gain_stars", None)
        if gain_stars is not None:
            gain_stars(owner, self.amount)


# =====================================================================
#  45. GigantificationPower
# =====================================================================
class GigantificationPower(PowerInstance):
    """The first powered attack card each turn deals 3x damage. Decrements
    after the first modified attack resolves.

    C# ref: GigantificationPower.cs
    - BeforeAttack: record first attack command from a card.
    - ModifyDamageMultiplicative: 3x for the recorded attack.
    - AfterAttack: clear recorded command, decrement.
    StackType.Counter.

    # Simplified: tracks whether the first attack this turn has been
    # boosted. The damage pipeline should check this power.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.GIGANTIFICATION, amount)
        self._boosting_attack: bool = False
        self._used_this_turn: bool = False

    def modify_damage_multiplicative(
        self, owner: Creature, dealer: Creature | None, target: Creature, props: ValueProp
    ) -> float:
        if dealer is not owner:
            return 1.0
        if not props.is_powered():
            return 1.0
        if self._boosting_attack:
            return 3.0
        return 1.0

    def before_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        card_type = getattr(card, "card_type", None) or getattr(card, "type", None)
        if card_type == CardType.ATTACK and not self._used_this_turn:
            self._boosting_attack = True

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        if self._boosting_attack:
            self._boosting_attack = False
            self._used_this_turn = True
            self.amount -= 1

    def before_side_turn_start(self, owner: Creature, side: CombatSide,
                               combat: CombatState) -> None:
        if side == owner.side:
            self._used_this_turn = False
            self._boosting_attack = False


# =====================================================================
#  46. GrapplePower
# =====================================================================
class GrapplePower(PowerInstance):
    """Debuff. When the applier gains block, deal Amount damage (unpowered)
    to the owner. Removed at end of any turn.

    C# ref: GrapplePower.cs
    - AfterBlockGained: if creature == applier and amount > 0, deal Amount
      damage to owner.
    - AfterTurnEnd: remove self (any side).
    StackType.Counter. Instanced.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.GRAPPLE, amount)
        self.applier: Creature | None = None

    def after_block_gained(self, owner: Creature, creature: Creature,
                           amount: int, combat: CombatState) -> None:
        if amount <= 0:
            return
        if creature is not self.applier:
            return
        combat.deal_damage(
            dealer=owner,
            target=owner,
            amount=self.amount,
            props=ValueProp.UNPOWERED,
        )

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        self.amount = 0  # signal removal


# =====================================================================
#  Registration
# =====================================================================
from sts2_env.core.creature import register_power_class  # noqa: E402

_ALL_POWERS: dict[PowerId, type[PowerInstance]] = {
    PowerId.ACCELERANT: AccelerantPower,
    PowerId.ADAPTABLE: AdaptablePower,
    PowerId.ANTICIPATE: AnticipatePower,
    PowerId.ARSENAL: ArsenalPower,
    PowerId.AUTOMATION: AutomationPower,
    PowerId.BATTLEWORN_DUMMY_TIME_LIMIT: BattlewornDummyTimeLimitPower,
    PowerId.BEACON_OF_HOPE: BeaconOfHopePower,
    PowerId.BLACK_HOLE: BlackHolePower,
    PowerId.BLADE_OF_INK: BladeOfInkPower,
    PowerId.CALL_OF_THE_VOID: CallOfTheVoidPower,
    PowerId.CHAINS_OF_BINDING: ChainsOfBindingPower,
    PowerId.CHILD_OF_THE_STARS: ChildOfTheStarsPower,
    PowerId.CLARITY: ClarityPower,
    PowerId.CONFUSED: ConfusedPower,
    PowerId.CONQUEROR: ConquerorPower,
    PowerId.CONSUMING_SHADOW: ConsumingShadowPower,
    PowerId.COOLANT: CoolantPower,
    PowerId.CORROSIVE_WAVE: CorrosiveWavePower,
    PowerId.CRIMSON_MANTLE: CrimsonMantlePower,
    PowerId.CRUSH_UNDER: CrushUnderPower,
    PowerId.CURIOUS: CuriousPower,
    PowerId.DAMPEN: DampenPower,
    PowerId.DANSE_MACABRE: DanseMacabrePower,
    PowerId.DARK_SHACKLES: DarkShacklesPower,
    PowerId.DEMESNE: DemesnePower,
    PowerId.DEMISE: DemisePower,
    PowerId.DEVOUR_LIFE: DevourLifePower,
    PowerId.DIAMOND_DIADEM: DiamondDiademPower,
    PowerId.DIE_FOR_YOU: DieForYouPower,
    PowerId.DISINTEGRATION: DisintegrationPower,
    PowerId.DYING_STAR: DyingStarPower,
    PowerId.ENFEEBLING_TOUCH: EnfeeblingTouchPower,
    PowerId.ENTROPY: EntropyPower,
    PowerId.ESCAPE_ARTIST: EscapeArtistPower,
    PowerId.FASTEN: FastenPower,
    PowerId.FERAL: FeralPower,
    PowerId.FLANKING: FlankingPower,
    PowerId.FLUTTER: FlutterPower,
    PowerId.FOCUS: FocusPower,
    PowerId.FORBIDDEN_GRIMOIRE: ForbiddenGrimoirePower,
    PowerId.FOREGONE_CONCLUSION: ForegoneConclusionPower,
    PowerId.FRIENDSHIP: FriendshipPower,
    PowerId.FURNACE: FurnacePower,
    PowerId.GENESIS: GenesisPower,
    PowerId.GIGANTIFICATION: GigantificationPower,
    PowerId.GRAPPLE: GrapplePower,
}

for _pid, _cls in _ALL_POWERS.items():
    register_power_class(_pid, _cls)
