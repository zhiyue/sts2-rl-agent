"""Turn-start and turn-end effect powers.

Implements powers that trigger effects at the start or end of a turn,
verified against the decompiled C# source from MegaCrit.Sts2.Core.Models.Powers.
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from sts2_env.core.enums import (
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
#  Turn-start powers
# =====================================================================


class DemonFormPower(PowerInstance):
    """Gain Amount Strength at the start of each turn.

    C# hook: AfterSideTurnStart (side == Owner.Side)
    Applies StrengthPower(Amount) to owner.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.DEMON_FORM, amount)

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            owner.apply_power(PowerId.STRENGTH, self.amount)


class RitualPower(PowerInstance):
    """Gain Amount Strength at end of owner's turn.

    C# hook: AfterTurnEnd (side == Owner.Side).
    Enemy-applied Ritual skips its first tick (WasJustAppliedByEnemy flag).
    In our simulator this is modeled with skip_next_tick.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.RITUAL, amount)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            if self.skip_next_tick:
                self.skip_next_tick = False
                return
            owner.apply_power(PowerId.STRENGTH, self.amount)


class RegenPower(PowerInstance):
    """Heal Amount HP at end of turn, then decrement.

    C# hook: AfterTurnEnd (side == Owner.Side).
    Heals owner, then decrements by 1. Removes at 0.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.REGEN, amount)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side and owner.is_alive:
            owner.heal(self.amount)
            self.amount -= 1


class NoxiousFumesPower(PowerInstance):
    """Apply Amount Poison to all enemies at the start of owner's turn.

    C# hook: AfterSideTurnStart (side == Owner.Side).
    Applies PoisonPower(Amount) to all hittable enemies.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.NOXIOUS_FUMES, amount)

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side != owner.side:
            return
        for enemy in combat.get_enemies_of(owner):
            if enemy.is_alive:
                enemy.apply_power(PowerId.POISON, self.amount)


class CreativeAiPower(PowerInstance):
    """Generate Amount random Power card(s) into hand at start of turn.

    C# hook: BeforeHandDraw (player == Owner.Player).
    In the simulator we add a random power card to hand.
    The combat state handles the actual card generation.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.CREATIVE_AI, amount)

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side and owner.is_player:
            # Card generation is handled by the combat state
            for _ in range(self.amount):
                combat.generate_card_to_hand(owner, card_type=CardType.POWER)


class StormPower(PowerInstance):
    """Channel Amount Lightning orb(s) when a Power card is played.

    C# hook: AfterCardPlayed — checks card.Type == Power.
    Channels Lightning orb for each stack.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.STORM, amount)

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        card_type = getattr(card, "card_type", None) or getattr(card, "type", None)
        if card_type == CardType.POWER:
            card_owner = getattr(card, "owner", None)
            if card_owner is owner or card_owner is None:
                for _ in range(self.amount):
                    combat.channel_orb(owner, "LIGHTNING")


class DrawCardsNextTurnPower(PowerInstance):
    """Draw Amount extra cards next turn, then remove self.

    C# hooks: ModifyHandDraw (adds Amount) + AfterSideTurnStart (removes self).
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.DRAW_CARDS_NEXT_TURN, amount)

    def modify_hand_draw(self, owner: Creature, draw: int) -> int:
        if self.amount != 0:
            return draw + self.amount
        return draw

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side and self.amount != 0:
            # Remove self after the extra draw has been applied
            self.amount = 0


class EnergyNextTurnPower(PowerInstance):
    """Gain Amount extra energy next turn, then remove self.

    C# hook: AfterEnergyReset (player == Owner.Player).
    Mapped to after_side_turn_start for the simulator.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.ENERGY_NEXT_TURN, amount)

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side and owner.is_player:
            combat.gain_energy(owner, self.amount)
            self.amount = 0


class StarNextTurnPower(PowerInstance):
    """Gain Amount stars next turn, then remove self.

    C# hook: AfterEnergyReset (player == Owner.Player).
    Stars are Regent-specific resource; mapped to combat.gain_stars.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.STAR_NEXT_TURN, amount)

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side and owner.is_player:
            combat.gain_stars(owner, self.amount)
            self.amount = 0


class SummonNextTurnPower(PowerInstance):
    """Summon Amount Osty(s) next turn, then remove self.

    C# hook: AfterPlayerTurnStart.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.SUMMON_NEXT_TURN, amount)

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side and owner.is_player and self.amount != 0:
            combat.summon_osty(owner, self.amount)
            self.amount = 0


class ToolsOfTheTradePower(PowerInstance):
    """Draw Amount extra cards at turn start, then discard Amount cards.

    C# hooks: ModifyHandDraw (adds Amount) + AfterPlayerTurnStart (discard Amount).
    The discard selection is handled by the combat/agent layer.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.TOOLS_OF_THE_TRADE, amount)

    def modify_hand_draw(self, owner: Creature, draw: int) -> int:
        return draw + self.amount

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side and owner.is_player:
            combat.request_discard(owner, self.amount)


class BurstPower(PowerInstance):
    """Next Skill is played an additional time. Decrements per use, removed at end of turn.

    C# hooks: ModifyCardPlayCount (card.Type == Skill => +1),
              AfterModifyingCardPlayCount (decrement),
              AfterTurnEnd (remove).
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.BURST, amount)

    def modify_card_play_count(self, owner: Creature, count: int, card: object) -> int:
        card_type = getattr(card, "card_type", None) or getattr(card, "type", None)
        card_owner = getattr(card, "owner", None)
        if card_owner is not owner:
            return count
        if card_type != CardType.SKILL:
            return count
        return count + 1

    def after_modifying_card_play_count(self, owner: Creature, card: object, combat: CombatState) -> None:
        card_type = getattr(card, "card_type", None) or getattr(card, "type", None)
        if card_type == CardType.SKILL:
            self.amount -= 1

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            self.amount = 0


class DuplicationPower(PowerInstance):
    """Next card is played an additional time. Decrements per use, removed at end of turn.

    C# hooks: ModifyCardPlayCount (any card => +1),
              AfterModifyingCardPlayCount (decrement),
              AfterTurnEnd (remove).
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.DUPLICATION, amount)

    def modify_card_play_count(self, owner: Creature, count: int, card: object) -> int:
        card_owner = getattr(card, "owner", None)
        if card_owner is not owner:
            return count
        return count + 1

    def after_modifying_card_play_count(self, owner: Creature, card: object, combat: CombatState) -> None:
        self.amount -= 1

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            self.amount = 0


class MayhemPower(PowerInstance):
    """Auto-play Amount card(s) from top of draw pile at start of turn.

    C# hook: BeforeHandDrawLate (player == Owner.Player).
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.MAYHEM, amount)

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side and owner.is_player:
            combat.auto_play_from_draw(owner, self.amount)


class HelloWorldPower(PowerInstance):
    """Generate Amount random Common card(s) into hand at start of turn.

    C# hook: BeforeHandDraw (player == Owner.Player).
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.HELLO_WORLD, amount)

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side and owner.is_player:
            for _ in range(self.amount):
                combat.generate_card_to_hand(owner, rarity="COMMON")


class MachineLearningPower(PowerInstance):
    """Draw Amount extra cards each turn (permanent).

    C# hook: ModifyHandDraw (player == Owner.Player).
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.MACHINE_LEARNING, amount)

    def modify_hand_draw(self, owner: Creature, draw: int) -> int:
        return draw + self.amount


class LoopPower(PowerInstance):
    """Trigger the passive of the first orb Amount time(s) at start of turn.

    C# hook: AfterPlayerTurnStart.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.LOOP, amount)

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side and owner.is_player:
            for _ in range(self.amount):
                combat.trigger_first_orb_passive(owner)


class PanachePower(PowerInstance):
    """Every 5th card played deals Amount damage to all enemies.

    C# hooks: AfterCardPlayed (track cards, deal damage every 5th),
              AfterTurnEnd (reset counter).
    Uses internal counter; display amount shows cards remaining.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    _TRIGGER_EVERY = 5

    def __init__(self, amount: int):
        super().__init__(PowerId.PANACHE, amount)
        self._cards_left: int = self._TRIGGER_EVERY
        self._started: bool = False

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        card_owner = getattr(card, "owner", None)
        if card_owner is not None and card_owner is not owner:
            return
        if self._started:
            self._cards_left -= 1
            if self._cards_left <= 0:
                for enemy in combat.get_enemies_of(owner):
                    if enemy.is_alive:
                        combat.deal_damage(
                            dealer=owner,
                            target=enemy,
                            amount=self.amount,
                            props=ValueProp.UNPOWERED,
                        )
                self._cards_left = self._TRIGGER_EVERY
        self._started = True

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            self._cards_left = self._TRIGGER_EVERY


class InfiniteBladesPower(PowerInstance):
    """Add Amount Shiv(s) to hand at the start of each turn.

    C# hook: BeforeHandDraw (player == Owner.Player).
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.INFINITE_BLADES, amount)

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side and owner.is_player:
            combat.add_shivs_to_hand(owner, self.amount)


class FanOfKnivesPower(PowerInstance):
    """Marker power (Single stack). No hook logic in C# — a flag checked by FanOfKnives card.

    C# source: StackType = Single, no hooks.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.SINGLE

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.FAN_OF_KNIVES, amount)


class DrumOfBattlePower(PowerInstance):
    """Exhaust Amount card(s) from the top of the draw pile at start of turn.

    C# hook: BeforeHandDrawLate (shuffles if needed, exhausts top card).
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.DRUM_OF_BATTLE, amount)

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side and owner.is_player:
            combat.exhaust_from_draw_pile(owner, self.amount)


# =====================================================================
#  Turn-end powers
# =====================================================================


class PoisonPower(PowerInstance):
    """Deal Amount unblockable damage at start of owner's turn, then decrement.

    C# hook: AfterSideTurnStart (side == Owner.Side).
    Deals damage = Amount, then decrements by 1. Repeats once per turn
    (Accelerant can increase trigger count, but simplified here to 1).
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.POISON, amount)

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side != owner.side:
            return
        # Deal poison damage (unblockable, unpowered)
        if owner.is_alive and self.amount > 0:
            combat.deal_damage(
                dealer=None,
                target=owner,
                amount=self.amount,
                props=ValueProp.UNBLOCKABLE | ValueProp.UNPOWERED,
            )
            if owner.is_alive:
                self.amount -= 1


class ConstrictPower(PowerInstance):
    """Deal Amount damage to owner at end of their turn.

    C# hook: AfterTurnEnd (side == Owner.Side).
    Damage is unpowered. Removed if the applier dies.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.CONSTRICT, amount)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            combat.deal_damage(
                dealer=owner,
                target=owner,
                amount=self.amount,
                props=ValueProp.UNPOWERED,
            )


class TheBombPower(PowerInstance):
    """Countdown timer: decrement each turn, deal 40 damage to all enemies when it hits 0.

    C# hook: BeforeTurnEnd (side == Owner.Side).
    Damage default is 40 (unpowered). Amount is the countdown turns.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER
    DEFAULT_DAMAGE = 40

    def __init__(self, amount: int):
        super().__init__(PowerId.THE_BOMB, amount)
        self.damage: int = self.DEFAULT_DAMAGE

    def before_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side != owner.side:
            return
        if self.amount > 1:
            self.amount -= 1
            return
        # Explode: deal damage to all enemies
        for enemy in combat.get_enemies_of(owner):
            if enemy.is_alive:
                combat.deal_damage(
                    dealer=owner,
                    target=enemy,
                    amount=self.damage,
                    props=ValueProp.UNPOWERED,
                )
        self.amount = 0


class TemporaryStrengthPower(PowerInstance):
    """Temporary Strength: removed at end of turn, reversing the Strength gain.

    C# hook: AfterTurnEnd (side == Owner.Side).
    On removal, applies -Amount Strength to owner.
    The initial Strength was granted when this power was first applied
    (BeforeApplied applies StrengthPower of same amount).
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.TEMPORARY_STRENGTH, amount)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            owner.apply_power(PowerId.STRENGTH, -self.amount)
            self.amount = 0


class FlexPotionPower(TemporaryStrengthPower):
    """Flex Potion: identical to TemporaryStrengthPower (positive temporary strength)."""

    def __init__(self, amount: int):
        # Call PowerInstance.__init__ directly to set the correct PowerId
        PowerInstance.__init__(self, PowerId.FLEX_POTION, amount)


class ShacklingPotionPower(PowerInstance):
    """Shackling Potion: negative temporary Strength (debuff version).

    Applies -Amount Strength on application. At end of turn, removes self
    and reverses the debuff by applying +Amount Strength.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.SHACKLING_POTION, amount)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            # Reverse: was applied as -Amount STR, now restore +Amount STR
            owner.apply_power(PowerId.STRENGTH, self.amount)
            self.amount = 0


class TemporaryDexterityPower(PowerInstance):
    """Temporary Dexterity: removed at end of turn, reversing the Dexterity gain.

    C# hook: AfterTurnEnd (side == Owner.Side).
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.TEMPORARY_DEXTERITY, amount)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            owner.apply_power(PowerId.DEXTERITY, -self.amount)
            self.amount = 0


class SpeedPotionPower(TemporaryDexterityPower):
    """Speed Potion: identical to TemporaryDexterityPower (positive temporary dexterity)."""

    def __init__(self, amount: int):
        PowerInstance.__init__(self, PowerId.SPEED_POTION, amount)


class TemporaryFocusPower(PowerInstance):
    """Temporary Focus: removed at end of turn, reversing the Focus gain.

    C# hook: AfterTurnEnd (side == Owner.Side).
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.TEMPORARY_FOCUS, amount)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            owner.apply_power(PowerId.FOCUS, -self.amount)
            self.amount = 0


class BlockNextTurnPower(PowerInstance):
    """Gain Amount block when block is cleared (next turn start), then remove self.

    C# hook: AfterBlockCleared (creature == Owner).
    Gains unpowered block then removes self.
    We map this to after_side_turn_start since block clearing happens
    at turn start in the sim pipeline.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.BLOCK_NEXT_TURN, amount)

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            owner.gain_block(self.amount)
            self.amount = 0


class RetainHandPower(PowerInstance):
    """Retain entire hand for Amount turn(s).

    C# hooks: ShouldFlush (returns false for owner's player),
              AfterTurnEnd (decrement).
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.RETAIN_HAND, amount)

    def before_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side and owner.is_player and self.amount > 0:
            combat.request_retain(owner, len(combat.hand))

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            self.amount -= 1


class WellLaidPlansPower(PowerInstance):
    """Retain up to Amount card(s) at end of turn.

    C# hook: BeforeFlushLate — lets the player choose cards to retain.
    In the simulator, this is handled by the combat layer; this power
    just exposes the retain count.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.WELL_LAID_PLANS, amount)

    def before_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side and owner.is_player:
            combat.request_retain(owner, self.amount)


class WraithFormPower(PowerInstance):
    """Lose Amount Dexterity at the start of each turn.

    C# hook: AfterSideTurnStart (side == Owner.Side).
    Applies DexterityPower(-Amount).
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.WRAITH_FORM, amount)

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            owner.apply_power(PowerId.DEXTERITY, -self.amount)


class BiasedCognitionPower(PowerInstance):
    """Lose Amount Focus at the start of each turn.

    C# hook: AfterSideTurnStart (side == Owner.Side).
    Applies FocusPower(-Amount).
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.BIASED_COGNITION, amount)

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            owner.apply_power(PowerId.FOCUS, -self.amount)


class CalcifyPower(PowerInstance):
    """Osty (pet) attacks deal Amount extra damage.

    C# hook: ModifyDamageAdditive — adds Amount when dealer is an Osty
    owned by the power's owner. Regent-specific power.
    In the simulator, we model this as a simple damage boost for pet attacks.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.CALCIFY, amount)

    def modify_damage_additive(
        self, owner: Creature, dealer: Creature | None, target: Creature, props: ValueProp
    ) -> int:
        # In the sim, if the dealer is the owner's pet and attack is powered
        if dealer is not None and props.is_powered():
            if getattr(dealer, "is_pet_of", None) is owner:
                return self.amount
        return 0


class CountdownPower(PowerInstance):
    """Apply Amount Doom to a random enemy at start of turn.

    C# hook: AfterSideTurnStart (side == Owner.Side).
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.COUNTDOWN, amount)

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            target = combat.random_enemy_of(owner)
            if target is not None:
                target.apply_power(PowerId.DOOM, self.amount)


class DoomPower(PowerInstance):
    """At end of turn, if owner's HP <= Doom amount, kill the owner.

    C# hook: BeforeTurnEnd (side == Owner.Side).
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.DOOM, amount)

    def before_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side and owner.is_alive:
            if owner.current_hp <= self.amount:
                owner.lose_hp(owner.current_hp)


# =====================================================================
#  Registration
# =====================================================================

from sts2_env.core.creature import register_power_class

_ALL_POWERS: dict[PowerId, type[PowerInstance]] = {
    PowerId.DEMON_FORM: DemonFormPower,
    PowerId.RITUAL: RitualPower,
    PowerId.REGEN: RegenPower,
    PowerId.NOXIOUS_FUMES: NoxiousFumesPower,
    PowerId.CREATIVE_AI: CreativeAiPower,
    PowerId.STORM: StormPower,
    PowerId.DRAW_CARDS_NEXT_TURN: DrawCardsNextTurnPower,
    PowerId.ENERGY_NEXT_TURN: EnergyNextTurnPower,
    PowerId.STAR_NEXT_TURN: StarNextTurnPower,
    PowerId.SUMMON_NEXT_TURN: SummonNextTurnPower,
    PowerId.TOOLS_OF_THE_TRADE: ToolsOfTheTradePower,
    PowerId.BURST: BurstPower,
    PowerId.DUPLICATION: DuplicationPower,
    PowerId.MAYHEM: MayhemPower,
    PowerId.HELLO_WORLD: HelloWorldPower,
    PowerId.MACHINE_LEARNING: MachineLearningPower,
    PowerId.LOOP: LoopPower,
    PowerId.PANACHE: PanachePower,
    PowerId.INFINITE_BLADES: InfiniteBladesPower,
    PowerId.FAN_OF_KNIVES: FanOfKnivesPower,
    PowerId.DRUM_OF_BATTLE: DrumOfBattlePower,
    PowerId.POISON: PoisonPower,
    PowerId.CONSTRICT: ConstrictPower,
    PowerId.THE_BOMB: TheBombPower,
    PowerId.TEMPORARY_STRENGTH: TemporaryStrengthPower,
    PowerId.FLEX_POTION: FlexPotionPower,
    PowerId.SHACKLING_POTION: ShacklingPotionPower,
    PowerId.TEMPORARY_DEXTERITY: TemporaryDexterityPower,
    PowerId.SPEED_POTION: SpeedPotionPower,
    PowerId.TEMPORARY_FOCUS: TemporaryFocusPower,
    PowerId.BLOCK_NEXT_TURN: BlockNextTurnPower,
    PowerId.RETAIN_HAND: RetainHandPower,
    PowerId.WELL_LAID_PLANS: WellLaidPlansPower,
    PowerId.WRAITH_FORM: WraithFormPower,
    PowerId.BIASED_COGNITION: BiasedCognitionPower,
    PowerId.CALCIFY: CalcifyPower,
    PowerId.COUNTDOWN: CountdownPower,
    PowerId.DOOM: DoomPower,
}

for _pid, _cls in _ALL_POWERS.items():
    register_power_class(_pid, _cls)
