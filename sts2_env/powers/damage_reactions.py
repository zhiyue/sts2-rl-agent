"""Powers that react to damage events (taking or dealing damage).

Covers: ThornsPower, FlameBarrierPower, CurlUpPower, SelfFormingClayPower,
ReflectPower, GalvanicPower, InterceptPower.

All logic verified against decompiled C# source.
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from sts2_env.core.enums import (
    CardId,
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


# ---------------------------------------------------------------------------
# ThornsPower
# ---------------------------------------------------------------------------
class ThornsPower(PowerInstance):
    """Whenever you are hit by a powered attack, deal Amount damage back
    to the attacker (unpowered, skip-hurt-anim).

    C# ref: ThornsPower.cs
    - BeforeDamageReceived: if target == owner and dealer exists and
      powered attack, deal Amount damage to dealer.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.THORNS, amount)

    def before_damage_received(
        self,
        owner: Creature,
        target: Creature,
        dealer: Creature | None,
        damage: int,
        props: ValueProp,
        combat: CombatState,
    ) -> None:
        card_source = getattr(combat, "active_card_source", None)
        is_omnislice = getattr(card_source, "card_id", None) == CardId.OMNISLICE
        if target is owner and dealer is not None and (props.is_powered() or is_omnislice):
            combat.deal_damage(
                dealer=owner,
                target=dealer,
                amount=self.amount,
                props=ValueProp.UNPOWERED | ValueProp.SKIP_HURT_ANIM,
            )


# ---------------------------------------------------------------------------
# FlameBarrierPower
# ---------------------------------------------------------------------------
class FlameBarrierPower(PowerInstance):
    """Whenever you are hit by a powered attack, deal Amount damage back
    to the attacker (unpowered). Removed at end of the opposing side's turn.

    C# ref: FlameBarrierPower.cs
    - AfterDamageReceived: if target == owner and dealer exists and
      powered attack, deal Amount damage to dealer.
    - AfterTurnEnd: remove when the OPPOSITE side's turn ends
      (i.e., when enemy turn ends if you applied it on player turn).
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.FLAME_BARRIER, amount)

    def after_damage_received(
        self,
        owner: Creature,
        target: Creature,
        dealer: Creature | None,
        damage: int,
        props: ValueProp,
        combat: CombatState,
    ) -> None:
        if target is owner and dealer is not None and props.is_powered():
            combat.deal_damage(
                dealer=owner,
                target=dealer,
                amount=self.amount,
                props=ValueProp.UNPOWERED,
            )

    def after_turn_end(
        self, owner: Creature, side: CombatSide, combat: CombatState
    ) -> None:
        # Remove when the turn of the side OPPOSITE to owner ends.
        # C#: if (base.Owner.Side != side) -> remove.
        if owner.side != side:
            owner.powers.pop(self.power_id, None)


# ---------------------------------------------------------------------------
# CurlUpPower
# ---------------------------------------------------------------------------
class CurlUpPower(PowerInstance):
    """The first time you are hit by a powered attack (from a card), gain
    Amount Block and remove this power.

    C# ref: CurlUpPower.cs
    - AfterDamageReceived: if target == owner and powered attack from a card,
      mark the card source. After that card finishes playing, gain block
      and remove self.
    StackType.Counter. Single-use (removed after triggering).

    Simplified: We trigger immediately on the first qualifying hit instead
    of waiting for the card to finish playing.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.CURL_UP, amount)
        self._triggered: bool = False

    def after_damage_received(
        self,
        owner: Creature,
        target: Creature,
        dealer: Creature | None,
        damage: int,
        props: ValueProp,
        combat: CombatState,
    ) -> None:
        if (
            target is owner
            and not self._triggered
            and props.is_powered()
            and dealer is not None
        ):
            self._triggered = True
            owner.gain_block(self.amount)
            owner.powers.pop(self.power_id, None)


# ---------------------------------------------------------------------------
# SelfFormingClayPower
# ---------------------------------------------------------------------------
class SelfFormingClayPower(PowerInstance):
    """When your block is fully broken, gain Amount Block and remove this power.

    C# ref: SelfFormingClayPower.cs
    - AfterBlockCleared: if creature == owner, gain block and remove self.
    StackType.Counter. Single-use.

    NOTE: The C# hook is AfterBlockCleared, which fires when block drops
    to 0. In the simulator, we check after damage if block was broken.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.SELF_FORMING_CLAY, amount)

    def after_damage_received(
        self,
        owner: Creature,
        target: Creature,
        dealer: Creature | None,
        damage: int,
        props: ValueProp,
        combat: CombatState,
    ) -> None:
        # Trigger when owner's block was broken (block is now 0 after taking
        # a hit that consumed block).
        if target is owner and target.block == 0 and damage > 0:
            owner.gain_block(self.amount)
            owner.powers.pop(self.power_id, None)


# ---------------------------------------------------------------------------
# ReflectPower
# ---------------------------------------------------------------------------
class ReflectPower(PowerInstance):
    """Whenever you are hit by a powered attack and block some damage, deal
    the blocked amount back to the attacker. Decrements at start of your turn.

    C# ref: ReflectPower.cs
    - AfterDamageReceived: if target == owner, result.BlockedDamage > 0,
      powered attack, and dealer exists, deal BlockedDamage to dealer.
    - AfterSideTurnStart: decrement on owner's side.
    StackType.Counter.

    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.REFLECT, amount)

    def on_damage_blocked(
        self,
        owner: Creature,
        blocked_amount: int,
        dealer: Creature | None,
        props: ValueProp,
        combat: CombatState,
    ) -> None:
        """Called by the damage pipeline with the amount of damage that was
        blocked. Reflects the blocked damage back at the attacker."""
        if blocked_amount > 0 and dealer is not None and props.is_powered():
            combat.deal_damage(
                dealer=owner,
                target=dealer,
                amount=blocked_amount,
                props=ValueProp.UNPOWERED,
            )

    def after_side_turn_start(
        self, owner: Creature, side: CombatSide, combat: CombatState
    ) -> None:
        if side == owner.side:
            self.amount -= 1
            if self.amount <= 0:
                owner.powers.pop(self.power_id, None)


# ---------------------------------------------------------------------------
# GalvanicPower
# ---------------------------------------------------------------------------
class GalvanicPower(PowerInstance):
    """Power cards are afflicted with "Galvanized" -- when played, they deal
    Amount damage to the player who played them.

    C# ref: GalvanicPower.cs
    - BeforeCombatStart: afflict all Power cards with Galvanized.
    - AfterCardEnteredCombat: afflict new Power cards.
    - AfterCardPlayed: if card has Galvanized, deal Amount damage to card
      owner (unpowered, as a MOVE).
    StackType.Counter.

    Simplified: In the simulator, whenever a Power card is played, the
    owner takes Amount self-damage. The affliction mechanic is not modeled
    separately.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.GALVANIC, amount)

    def after_card_played(
        self, owner: Creature, card: object, combat: CombatState
    ) -> None:
        from sts2_env.core.enums import CardType
        if getattr(card, "card_type", None) == CardType.POWER:
            # Deal self-damage to the card's owner (the player who played it)
            card_owner_creature = getattr(card, "owner_creature", None)
            if card_owner_creature is not None:
                combat.deal_damage(
                    dealer=None,
                    target=card_owner_creature,
                    amount=self.amount,
                    props=ValueProp.UNPOWERED | ValueProp.MOVE,
                )


# ---------------------------------------------------------------------------
# InterceptPower
# ---------------------------------------------------------------------------
class InterceptPower(PowerInstance):
    """Takes increased damage from powered attacks proportional to the number
    of creatures being covered (+1 multiplier per covered creature).
    Removed at end of enemy turn.

    C# ref: InterceptPower.cs
    - ModifyDamageMultiplicative: (coveredCreatures.Count + 1) multiplier
      for powered attacks targeting owner.
    - AfterTurnEnd: remove at end of ENEMY side turn.
    StackType.Single.

    Simplified: The covered-creature list is tracked as a simple count
    stored in amount. The multiplier is (amount) since amount = count + 1
    from how CoveredPower sets it up.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.SINGLE

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.INTERCEPT, amount)
        self._covered_count: int = 0

    def add_covered_creature(self) -> None:
        """Called by CoveredPower when a creature is covered."""
        self._covered_count += 1

    def modify_damage_multiplicative(
        self,
        owner: Creature,
        dealer: Creature | None,
        target: Creature,
        props: ValueProp,
    ) -> float:
        if target is owner and props.is_powered():
            return float(self._covered_count + 1)
        return 1.0

    def after_turn_end(
        self, owner: Creature, side: CombatSide, combat: CombatState
    ) -> None:
        if side == CombatSide.ENEMY:
            owner.powers.pop(self.power_id, None)


# ---------------------------------------------------------------------------
# Registration
# ---------------------------------------------------------------------------
from sts2_env.core.creature import register_power_class  # noqa: E402

_ALL_POWERS: dict[PowerId, type[PowerInstance]] = {
    PowerId.THORNS: ThornsPower,
    PowerId.FLAME_BARRIER: FlameBarrierPower,
    PowerId.CURL_UP: CurlUpPower,
    PowerId.SELF_FORMING_CLAY: SelfFormingClayPower,
    PowerId.REFLECT: ReflectPower,
    PowerId.GALVANIC: GalvanicPower,
    PowerId.INTERCEPT: InterceptPower,
}

for _pid, _cls in _ALL_POWERS.items():
    register_power_class(_pid, _cls)
