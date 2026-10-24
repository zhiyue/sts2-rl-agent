"""Remaining power implementations (batch C): 46 powers.

Covers: ReattachPower, ReboundPower, ReptileTrinketPower, RingingPower,
RollingBoulderPower, RoyaltiesPower, SandpitPower, SeekingEdgePower,
SentryModePower, ShadowStepPower, ShadowmeldPower, ShroudPower,
SicEmPower, SignalBoostPower, SleightOfFleshPower, SlowPower,
SlumberPower, SmokestackPower, SoarPower, SpectrumShiftPower,
SpeedsterPower, SpiritOfAshPower, StampedePower, StockPower,
StranglePower, StratagemPower, SubroutinePower, SwipePower,
SwordSagePower, SynchronizePower, TagTeamPower, TangledPower,
TankPower, TenderPower, TheGambitPower, TheHuntPower,
TheSealedThronePower, ToricToughnessPower, TrackingPower,
TrashToTreasurePower, TyrannyPower, UnmovablePower, VeilpiercerPower,
VitalSparkPower, VoidFormPower, WasteAwayPower.

All logic verified against decompiled C# source from
MegaCrit.Sts2.Core.Models.Powers.
"""

from __future__ import annotations

import math
from typing import TYPE_CHECKING

from sts2_env.core.creature import _power_type_for_amount, get_power_class
from sts2_env.core.enums import (
    CardPilePosition,
    CardId,
    CardKeyword,
    CardType,
    CombatSide,
    PileType,
    PowerId,
    PowerType,
    PowerStackType,
    ValueProp,
)
from sts2_env.powers.base import PowerInstance

if TYPE_CHECKING:
    from sts2_env.core.creature import Creature
    from sts2_env.core.combat import CombatState


_SLUMBERING_BEETLE_ID = "SLUMBERING_BEETLE"
_SLUMBERING_BEETLE_ROLL_OUT_MOVE_ID = "ROLL_OUT_MOVE"


def _gain_unpowered_block(owner: Creature, amount: int, combat: CombatState) -> int:
    before = owner.block
    owner.gain_block(amount, unpowered=True)
    gained = owner.block - before
    if gained > 0:
        from sts2_env.core.hooks import fire_after_block_gained

        fire_after_block_gained(owner, gained, combat)
    return gained


# ---------------------------------------------------------------------------
# ReattachPower
# ---------------------------------------------------------------------------
class ReattachPower(PowerInstance):
    """Decimillipede segment revival. On death, marks as reviving and heals
    Amount HP when reattached. Prevents creature removal from combat.
    Only triggers fatal death when all other segments with ReattachPower
    are also dead.

    C# ref: ReattachPower.cs
    - AfterDeath: if not all other segments dead, mark reviving.
    - ShouldAllowHitting: false while reviving.
    - ShouldCreatureBeRemovedFromCombatAfterDeath: false for owner.
    - ShouldPowerBeRemovedAfterOwnerDeath: false.
    - ShouldOwnerDeathTriggerFatal: only when all segments dead.
    StackType.Single.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.SINGLE
    should_scale_in_multiplayer = True

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.REATTACH, amount)
        self.is_reviving: bool = False

    def after_death(
        self,
        owner: Creature,
        creature: Creature,
        combat: CombatState,
        was_removal_prevented: bool = False,
    ) -> None:
        if was_removal_prevented or creature is not owner:
            return
        teammates = combat.get_teammates_of(owner) if hasattr(combat, "get_teammates_of") else []
        other_segments = [
            c for c in teammates
            if c is not owner and c.has_power(PowerId.REATTACH)
        ]
        all_others_dead = all(not c.is_alive for c in other_segments)
        if not all_others_dead:
            self.is_reviving = True
            combat.set_enemy_state(owner, "DEAD_MOVE")

    def should_owner_death_trigger_fatal(
        self,
        owner: Creature,
        combat: CombatState,
    ) -> bool:
        teammates = combat.get_teammates_of(owner) if hasattr(combat, "get_teammates_of") else []
        other_segments = [
            creature for creature in teammates
            if creature is not owner and creature.has_power(PowerId.REATTACH)
        ]
        return all(not creature.is_alive for creature in other_segments)

    def do_reattach(self, owner: Creature) -> None:
        """Called by encounter system to heal the segment."""
        self.is_reviving = False
        owner.current_hp = min(owner.max_hp, max(1, self.amount))
        owner.escaped = False
        owner._death_processed = False

    def should_stop_combat_ending(self) -> bool:
        return self.is_reviving

    def should_allow_hitting(self, owner: Creature, combat: CombatState) -> bool:
        return not self.is_reviving

    def should_power_be_removed_after_owner_death(
        self,
        owner: Creature,
        combat: CombatState,
    ) -> bool:
        return False

    def should_creature_be_removed_from_combat_after_death(
        self,
        owner: Creature,
        combat: CombatState,
    ) -> bool:
        return False


# ---------------------------------------------------------------------------
# ReboundPower
# ---------------------------------------------------------------------------
class ReboundPower(PowerInstance):
    """Cards that would go to discard go to top of draw pile instead.
    Decrements per card. Removed at end of owner's turn.

    C# ref: ReboundPower.cs
    - ModifyCardPlayResultPileTypeAndPosition: Discard -> Draw (Top).
    - AfterModifyingCardPlayResultPileOrPosition: decrement.
    - AfterTurnEnd (owner side): remove.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.REBOUND, amount)

    def modify_card_play_result_pile_type_and_position(
        self,
        owner: Creature,
        card: object,
        is_auto_play: bool,
        energy_value: int,
        pile_type: PileType,
        position: CardPilePosition,
    ) -> tuple[PileType, CardPilePosition]:
        if getattr(card, "owner", None) is not owner:
            return pile_type, position
        if pile_type != PileType.DISCARD:
            return pile_type, position
        return PileType.DRAW, CardPilePosition.TOP

    def after_modifying_card_play_result_pile_or_position(
        self,
        card: object,
        pile_type: PileType,
        position: CardPilePosition,
        combat: CombatState,
    ) -> None:
        owner = getattr(card, "owner", None)
        if owner is None:
            return
        self.amount -= 1
        if self.amount <= 0:
            combat._remove_power(owner, self.power_id)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            self.amount = 0


# ---------------------------------------------------------------------------
# ReptileTrinketPower
# ---------------------------------------------------------------------------
class ReptileTrinketPower(PowerInstance):
    """Temporary Strength from Reptile Trinket relic. Grants Strength on
    application, removes it at end of owner's turn.

    C# ref: ReptileTrinketPower.cs extends TemporaryStrengthPower.
    - BeforeApplied: apply Strength(Amount).
    - AfterTurnEnd (owner side): remove self, apply Strength(-Amount).
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER
    is_temporary = True

    def __init__(self, amount: int):
        super().__init__(PowerId.REPTILE_TRINKET, amount)

    def after_power_amount_changed(
        self,
        owner: Creature,
        target: Creature,
        power_id: PowerId,
        amount: int,
        applier: Creature | None,
        source: object | None,
        combat: CombatState,
    ) -> None:
        if owner is target and power_id == self.power_id and amount != 0 and not self.consume_ignore_next_instance():
            owner.apply_power(PowerId.STRENGTH, amount, applier=applier, source=source)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            owner.apply_power(PowerId.STRENGTH, -self.amount)
            self.amount = 0


# ---------------------------------------------------------------------------
# RingingPower
# ---------------------------------------------------------------------------
class RingingPower(PowerInstance):
    """Afflicts all cards with Ringing -- only 1 card can be played per turn
    while active. Removed at end of owner's turn.

    C# ref: RingingPower.cs
    - AfterApplied: afflict all cards with Ringing.
    - ShouldPlay: blocks play if any card already played this turn.
    - AfterTurnEnd (owner side): remove.
    - AfterRemoved: clear afflictions.
    StackType.Single.

    Uses the combat card-play-start history to match the original
    CardPlayStartedEntry check.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.SINGLE
    _AFFLICTION = "ringing"

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.RINGING, amount)

    def _afflict_card(self, owner: Creature, card: object) -> None:
        if (getattr(card, "owner", None) or owner) is not owner:
            return
        afflict = getattr(card, "afflict", None)
        if callable(afflict):
            afflict(self._AFFLICTION)
            return
        combat_vars = getattr(card, "combat_vars", None)
        if isinstance(combat_vars, dict) and combat_vars.get("_affliction") is None:
            combat_vars["_affliction"] = self._AFFLICTION

    def _afflict_all_cards(self, owner: Creature, combat: CombatState) -> None:
        state = combat.combat_player_state_for(owner)
        if state is None:
            return
        for pile in state.all_piles:
            for card in pile:
                self._afflict_card(owner, card)

    def _clear_afflictions(self, owner: Creature, combat: CombatState) -> None:
        state = combat.combat_player_state_for(owner)
        if state is None:
            return
        for pile in state.all_piles:
            for card in pile:
                clear_affliction = getattr(card, "clear_affliction", None)
                if callable(clear_affliction):
                    clear_affliction(self._AFFLICTION)
                elif getattr(card, "combat_vars", {}).get("_affliction") == self._AFFLICTION:
                    card.combat_vars.pop("_affliction", None)

    def after_power_amount_changed(
        self,
        owner: Creature,
        target: Creature,
        power_id: PowerId,
        amount: int,
        applier: Creature | None,
        source: object | None,
        combat: CombatState,
    ) -> None:
        if owner is target and power_id == PowerId.RINGING and amount > 0:
            self._afflict_all_cards(owner, combat)

    def after_card_entered_combat(self, owner: Creature, card: object, combat: CombatState) -> None:
        self._afflict_card(owner, card)

    def should_play(self, owner: Creature, card: object, combat: CombatState) -> bool:
        if self.amount <= 0:
            return True
        if (getattr(card, "owner", None) or owner) is not owner:
            return True
        has_affliction = getattr(card, "has_affliction", None)
        if callable(has_affliction):
            if not has_affliction(self._AFFLICTION):
                return True
        elif getattr(card, "combat_vars", {}).get("_affliction") != self._AFFLICTION:
            return True
        if combat.count_card_play_starts_this_turn(owner) > 0:
            return False
        return True

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            self.amount = 0
            self._clear_afflictions(owner, combat)

    def on_removed(self, owner: Creature, combat: CombatState) -> None:
        self._clear_afflictions(owner, combat)


# ---------------------------------------------------------------------------
# RollingBoulderPower
# ---------------------------------------------------------------------------
class RollingBoulderPower(PowerInstance):
    """At start of player turn, deal Amount damage (unpowered) to all hittable
    enemies, then increase Amount by 5.

    C# ref: RollingBoulderPower.cs
    - AfterPlayerTurnStart: deal Amount damage to all hittable enemies,
      then Amount += DynamicVars.Damage (5).
    - CanonicalVars: DamageVar(5, Unpowered).
    StackType.Counter. Instanced.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER
    DAMAGE_INCREMENT = 5

    def __init__(self, amount: int):
        super().__init__(PowerId.ROLLING_BOULDER, amount)
        self._instances: list[int] = [amount]

    def after_power_amount_changed(
        self,
        owner: Creature,
        target: Creature,
        power_id: PowerId,
        amount: int,
        applier: Creature | None,
        source: object | None,
        combat: CombatState,
    ) -> None:
        if owner is target and power_id == self.power_id and amount > 0 and self.amount != amount:
            self._instances.append(amount)
            self.amount = amount

    def after_player_turn_start(self, owner: Creature, combat: CombatState) -> None:
        if not owner.is_player:
            return
        next_instances: list[int] = []
        for amount in self._instances:
            for enemy in combat.hittable_enemies:
                combat.deal_damage(
                    dealer=owner,
                    target=enemy,
                    amount=amount,
                    props=ValueProp.UNPOWERED,
                )
            next_instances.append(amount + self.DAMAGE_INCREMENT)
        self._instances = next_instances
        if self._instances:
            self.amount = self._instances[-1]


# ---------------------------------------------------------------------------
# RoyaltiesPower
# ---------------------------------------------------------------------------
class RoyaltiesPower(PowerInstance):
    """After combat ends, gain Amount gold as an extra reward.

    C# ref: RoyaltiesPower.cs
    - AfterCombatEnd: add GoldReward(Amount) to the room.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.ROYALTIES, amount)

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        room = getattr(combat, "room", None)
        state = combat.combat_player_state_for(owner)
        if room is None or state is None or not hasattr(room, "add_extra_reward"):
            return
        from sts2_env.run.reward_objects import GoldReward

        player_id = state.player_state.player_id
        room.add_extra_reward(player_id, GoldReward(player_id, self.amount, self.amount))


# ---------------------------------------------------------------------------
# SandpitPower
# ---------------------------------------------------------------------------
class SandpitPower(PowerInstance):
    """Countdown: decrements at start of enemy turn. When it reaches 0 the
    power is removed, which triggers a kill on the target player and pets.
    (The Insatiable boss mechanic.)

    C# ref: SandpitPower.cs
    - AfterSideTurnStart (Enemy): decrement.
    - AfterRemoved: if target alive, kill target player and pets.
    StackType.Counter. Instanced.

    The power lives on the sandpit enemy and tracks a specific target player.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.SANDPIT, amount)
        self.target: Creature | None = None
        self._instances: list[tuple[int, Creature | None]] = [(amount, None)]

    def set_target(self, target: Creature | None) -> None:
        self.target = target
        amount, _ = self._instances[-1]
        self._instances[-1] = (amount, target)

    def add_instance(self, amount: int, target: Creature | None) -> None:
        self._instances.append((amount, target))
        self.amount = amount
        self.target = target

    def increment_target(self, target: Creature, amount: int = 1) -> bool:
        for index, (turns, instance_target) in enumerate(self._instances):
            if instance_target is target:
                self._instances[index] = (turns + amount, instance_target)
                self.amount = turns + amount
                self.target = instance_target
                return True
        return False

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side != CombatSide.ENEMY:
            return
        remaining_instances: list[tuple[int, Creature | None]] = []
        for turns, target in self._instances:
            turns -= 1
            if turns <= 0:
                self._kill_target(owner, target, combat)
            else:
                remaining_instances.append((turns, target))
        self._instances = remaining_instances
        if not self._instances:
            combat._remove_power(owner, self.power_id)
            return
        self.amount, self.target = self._instances[-1]

    def on_removed(self, owner: Creature, combat: CombatState) -> None:
        if not owner.is_dead:
            for _, target in list(self._instances):
                self._kill_target(owner, target, combat)
        self._instances = []
        self.target = None

    def _kill_target(self, owner: Creature, target: Creature | None, combat: CombatState) -> None:
        if owner.is_dead or target is None or not target.is_alive:
            return
        combat.kill_creature(target)
        for ally in combat.get_player_allies_of(target):
            combat.kill_creature(ally)
        osty = combat.get_osty(target)
        if osty is not None and osty.is_alive:
            combat.kill_creature(osty)


# ---------------------------------------------------------------------------
# SeekingEdgePower
# ---------------------------------------------------------------------------
class SeekingEdgePower(PowerInstance):
    """Flag power for the Forge mechanic. No hook logic in C#.

    C# ref: SeekingEdgePower.cs
    - No hooks. StackType.Single. ExtraHoverTips: Forge.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.SINGLE

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.SEEKING_EDGE, amount)


# ---------------------------------------------------------------------------
# SentryModePower
# ---------------------------------------------------------------------------
class SentryModePower(PowerInstance):
    """At start of turn, generate Amount SweepingGaze card(s) into hand.

    C# ref: SentryModePower.cs
    - BeforeHandDraw: create Amount SweepingGaze cards in hand.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.SENTRY_MODE, amount)

    def before_hand_draw(self, owner: Creature, combat: CombatState) -> None:
        if owner.is_player:
            from sts2_env.cards.status import make_sweeping_gaze

            for _ in range(self.amount):
                combat.add_generated_card_to_creature_hand(owner, make_sweeping_gaze())


# ---------------------------------------------------------------------------
# ShadowStepPower
# ---------------------------------------------------------------------------
class ShadowStepPower(PowerInstance):
    """At start of player turn, apply Amount DoubleDamage to self, then
    remove this power.

    C# ref: ShadowStepPower.cs
    - AfterSideTurnStart (Player): apply DoubleDamagePower(Amount),
      remove self.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.SHADOW_STEP, amount)

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER:
            owner.apply_power(PowerId.DOUBLE_DAMAGE, self.amount)
            self.amount = 0


# ---------------------------------------------------------------------------
# ShadowmeldPower
# ---------------------------------------------------------------------------
class ShadowmeldPower(PowerInstance):
    """Multiplies block gained by 2^Amount. Removed at end of owner's turn.

    C# ref: ShadowmeldPower.cs
    - ModifyBlockMultiplicative: 2^Amount when target == owner.
    - AfterTurnEnd (owner side): remove.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.SHADOWMELD, amount)

    def modify_block_multiplicative(
        self, owner: Creature, target: Creature, props: ValueProp,
        card_source: object | None = None, card_play: object | None = None,
        combat: CombatState | None = None,
    ) -> float:
        if target is not owner:
            return 1.0
        return math.pow(2.0, self.amount)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            self.amount = 0


# ---------------------------------------------------------------------------
# ShroudPower
# ---------------------------------------------------------------------------
class ShroudPower(PowerInstance):
    """Whenever the owner applies Doom to an enemy, gain Amount block
    (unpowered).

    C# ref: ShroudPower.cs
    - AfterPowerAmountChanged: if applier == owner and power is DoomPower,
      gain block.
    StackType.Counter.

    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.SHROUD, amount)

    def after_power_amount_changed(
        self,
        owner: Creature,
        target: Creature,
        power_id: PowerId,
        amount: int,
        applier: Creature | None,
        source: object | None,
        combat: CombatState,
    ) -> None:
        if applier is owner and power_id == PowerId.DOOM and amount > 0:
            _gain_unpowered_block(owner, self.amount, combat)


# ---------------------------------------------------------------------------
# SicEmPower
# ---------------------------------------------------------------------------
class SicEmPower(PowerInstance):
    """When an Osty (pet) deals damage to this creature, summon Amount Osty(s)
    for the pet's owner. Removed at end of owner's turn.

    C# ref: SicEmPower.cs
    - AfterDamageGiven: if dealer is Osty and target is owner, summon.
    - AfterTurnEnd (owner side): remove.
    StackType.Counter.

    Simplified: The combat pipeline checks this power after pet damage.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.SIC_EM, amount)

    def after_damage_given(
        self, owner: Creature, dealer: Creature, target: Creature,
        damage: int, props: ValueProp, combat: CombatState
    ) -> None:
        if target is not owner:
            return
        if getattr(dealer, "is_pet", False):
            pet_owner = getattr(dealer, "pet_owner", None)
            if pet_owner is not None and pet_owner is self.applier:
                combat.summon_osty(pet_owner, self.amount)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            self.amount = 0


# ---------------------------------------------------------------------------
# SignalBoostPower
# ---------------------------------------------------------------------------
class SignalBoostPower(PowerInstance):
    """The next Amount Power card(s) played are played an extra time.
    Decrements per Power card played.

    C# ref: SignalBoostPower.cs
    - ModifyCardPlayCount: +1 for Power cards owned by this creature.
    - AfterModifyingCardPlayCount: decrement.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.SIGNAL_BOOST, amount)

    def modify_card_play_count(self, owner: Creature, count: int, card: object) -> int:
        card_owner = getattr(card, "owner", None)
        if card_owner is not owner:
            return count
        card_type = getattr(card, "card_type", None) or getattr(card, "type", None)
        if card_type != CardType.POWER:
            return count
        if self.amount <= 0:
            return count
        return count + 1

    def after_modifying_card_play_count(self, owner: Creature, card: object, combat: CombatState) -> None:
        card_owner = getattr(card, "owner", None)
        if card_owner is not owner:
            return
        card_type = getattr(card, "card_type", None) or getattr(card, "type", None)
        if card_type == CardType.POWER and self.amount > 0:
            self.amount -= 1


# ---------------------------------------------------------------------------
# SleightOfFleshPower
# ---------------------------------------------------------------------------
class SleightOfFleshPower(PowerInstance):
    """Whenever the owner applies a non-temporary debuff to an enemy, deal
    Amount damage (unpowered) to that enemy.

    C# ref: SleightOfFleshPower.cs
    - AfterPowerAmountChanged: if amount != 0, power type is Debuff,
      target is enemy, applier is owner, and power is not ITemporaryPower,
      deal damage.
    StackType.Counter.

    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.SLEIGHT_OF_FLESH, amount)

    def after_power_amount_changed(
        self,
        owner: Creature,
        target: Creature,
        power_id: PowerId,
        amount: int,
        applier: Creature | None,
        source: object | None,
        combat: CombatState,
    ) -> None:
        if applier is not owner or amount == 0:
            return
        if target.side == owner.side:
            return
        cls = get_power_class(power_id)
        if cls is None:
            return
        target_power = target.powers.get(power_id)
        if target_power is not None and _power_type_for_amount(cls, amount) == PowerType.DEBUFF and not target_power.is_temporary:
            combat.deal_damage(
                dealer=owner,
                target=target,
                amount=self.amount,
                props=ValueProp.UNPOWERED,
            )


# ---------------------------------------------------------------------------
# SlowPower
# ---------------------------------------------------------------------------
class SlowPower(PowerInstance):
    """Each card played increases damage taken by 10%. Resets at start of
    owner's turn.

    C# ref: SlowPower.cs
    - AfterCardPlayed: increment SlowAmount counter.
    - ModifyDamageMultiplicative: 1 + 0.1 * SlowAmount for powered attacks
      targeting owner.
    - AfterSideTurnStart (owner side): reset SlowAmount to 0.
    StackType.Counter. DisplayAmount = SlowAmount * 10.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.SLOW, amount)
        self._slow_amount: int = 0

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        self._slow_amount += 1

    def modify_damage_multiplicative(
        self, owner: Creature, dealer: Creature | None, target: Creature, props: ValueProp
    ) -> float:
        if target is not owner:
            return 1.0
        if not props.is_powered_attack():
            return 1.0
        return 1.0 + 0.1 * self._slow_amount

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            self._slow_amount = 0


# ---------------------------------------------------------------------------
# SlumberPower
# ---------------------------------------------------------------------------
class SlumberPower(PowerInstance):
    """Taking unblocked damage decrements. Ticks down at end of owner's turn.
    When amount reaches 0, the monster wakes up (stun into wake-up move).

    C# ref: SlumberPower.cs
    - AfterDamageReceived: if target == owner and unblocked > 0, decrement.
      If 0, stun into wake-up move.
    - AfterTurnEnd (owner side): decrement. If 0, wake up.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.SLUMBER, amount)
        self.is_awake: bool = False

    def after_damage_received(
        self, owner: Creature, target: Creature, dealer: Creature | None,
        damage: int, props: ValueProp, combat: CombatState
    ) -> None:
        if target is owner and damage > 0:
            self.amount -= 1
            if self.amount <= 0:
                self.is_awake = True
                owner.powers.pop(self.power_id, None)
                if owner.monster_id == _SLUMBERING_BEETLE_ID:
                    def wake_up(_: CombatState) -> None:
                        owner.powers.pop(PowerId.PLATING, None)
                        self.is_awake = True

                    combat.stun_enemy(owner, _SLUMBERING_BEETLE_ROLL_OUT_MOVE_ID, wake_up)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            self.amount -= 1
            if self.amount <= 0:
                self.is_awake = True
                owner.powers.pop(self.power_id, None)
                if owner.monster_id == _SLUMBERING_BEETLE_ID:
                    owner.powers.pop(PowerId.PLATING, None)
                    combat.set_enemy_state(owner, _SLUMBERING_BEETLE_ROLL_OUT_MOVE_ID)


# ---------------------------------------------------------------------------
# SmokestackPower
# ---------------------------------------------------------------------------
class SmokestackPower(PowerInstance):
    """When a Status card is generated for the player, deal Amount damage
    (unpowered) to all hittable enemies.

    C# ref: SmokestackPower.cs
    - AfterCardGeneratedForCombat: if addedByPlayer and Status, deal damage.
    StackType.Counter.

    The combat pipeline dispatches the generated-card hook when a status card
    enters combat through a generated-card path.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.SMOKESTACK, amount)

    def after_card_generated_for_combat(
        self,
        owner: Creature,
        card: object,
        added_by_player: bool,
        combat: CombatState,
    ) -> None:
        if not added_by_player or getattr(card, "card_type", None) != CardType.STATUS:
            return
        for enemy in combat.hittable_enemies:
            combat.deal_damage(
                dealer=owner,
                target=enemy,
                amount=self.amount,
                props=ValueProp.UNPOWERED,
            )


# ---------------------------------------------------------------------------
# SoarPower
# ---------------------------------------------------------------------------
class SoarPower(PowerInstance):
    """Reduces powered attack damage taken by 50% (multiplier = 0.5).

    C# ref: SoarPower.cs
    - ModifyDamageMultiplicative: DamageDecrease/100 (default 50 => 0.5)
      for powered attacks targeting owner.
    - CanonicalVars: DamageDecrease = 50.
    StackType.Single.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.SINGLE
    DAMAGE_DECREASE_PERCENT = 50

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.SOAR, amount)

    def modify_damage_multiplicative(
        self, owner: Creature, dealer: Creature | None, target: Creature, props: ValueProp
    ) -> float:
        if target is not owner:
            return 1.0
        if not props.is_powered_attack():
            return 1.0
        return self.DAMAGE_DECREASE_PERCENT / 100.0


# ---------------------------------------------------------------------------
# SpectrumShiftPower
# ---------------------------------------------------------------------------
class SpectrumShiftPower(PowerInstance):
    """At start of turn, generate Amount random Colorless card(s) into hand.

    C# ref: SpectrumShiftPower.cs
    - BeforeHandDraw: generate Amount distinct Colorless cards into hand.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.SPECTRUM_SHIFT, amount)

    def before_hand_draw(self, owner: Creature, combat: CombatState) -> None:
        if owner.is_player:
            gen = getattr(combat, "generate_colorless_cards", None)
            if gen is not None:
                gen(owner, self.amount)


# ---------------------------------------------------------------------------
# SpeedsterPower
# ---------------------------------------------------------------------------
class SpeedsterPower(PowerInstance):
    """When a card is drawn (not from hand draw) during owner's turn, deal
    Amount damage (unpowered) to all hittable enemies.

    C# ref: SpeedsterPower.cs
    - AfterCardDrawn: if not fromHandDraw and card owner is this creature
      and it's the owner's turn, deal Amount damage to all enemies.
    StackType.Counter.

    Simplified: Triggers via on_card_drawn hook (non-hand-draw draws).
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.SPEEDSTER, amount)

    def on_card_drawn(
        self, owner: Creature, card: object, from_hand_draw: bool, combat: CombatState
    ) -> None:
        """Called by draw pipeline. Deals AoE damage on mid-turn draws."""
        if from_hand_draw:
            return
        card_owner = getattr(card, "owner", None)
        if card_owner is not owner:
            return
        if not combat.is_owner_side_turn(owner):
            return
        for enemy in combat.hittable_enemies:
            combat.deal_damage(
                dealer=owner,
                target=enemy,
                amount=self.amount,
                props=ValueProp.UNPOWERED,
            )


# ---------------------------------------------------------------------------
# SpiritOfAshPower
# ---------------------------------------------------------------------------
class SpiritOfAshPower(PowerInstance):
    """Before playing an Ethereal card, gain Amount block (unpowered).

    C# ref: SpiritOfAshPower.cs
    - BeforeCardPlayed: if card has Ethereal keyword and belongs to owner,
      gain block.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.SPIRIT_OF_ASH, amount)

    def before_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        card_owner = getattr(card, "owner", None)
        if card_owner is not owner:
            return
        keywords = getattr(card, "keywords", set())
        if CardKeyword.ETHEREAL in keywords or "ethereal" in keywords:
            _gain_unpowered_block(owner, self.amount, combat)


# ---------------------------------------------------------------------------
# StampedePower
# ---------------------------------------------------------------------------
class StampedePower(PowerInstance):
    """Before turn ends, auto-play Amount random playable Attack(s) from hand.

    C# ref: StampedePower.cs
    - BeforeTurnEnd (owner side): pick Amount random playable Attacks from
      hand and auto-play them.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.STAMPEDE, amount)

    def before_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side != owner.side:
            return
        auto_play = getattr(combat, "auto_play_random_attack_from_hand", None)
        if auto_play is not None:
            for _ in range(self.amount):
                auto_play(owner)


# ---------------------------------------------------------------------------
# StockPower
# ---------------------------------------------------------------------------
class StockPower(PowerInstance):
    """On death, spawn a replacement Axebot with Stock(Amount-1). Prevents
    combat from ending while stock remains.

    C# ref: StockPower.cs
    - AfterDeath: if Amount > 0, spawn Axebot with Stock(Amount-1).
    - ShouldStopCombatFromEnding: true.
    StackType.Counter.

    Simplified: The encounter system handles spawning. This power is a flag.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.STOCK, amount)

    def should_stop_combat_ending(self) -> bool:
        return True

    def after_death(
        self,
        owner: Creature,
        creature: Creature,
        combat: CombatState,
        was_removal_prevented: bool = False,
    ) -> None:
        if was_removal_prevented or creature is not owner:
            return
        if self.amount > 0:
            spawn = getattr(combat, "spawn_axebot", None)
            if spawn is not None:
                spawn(owner, self.amount - 1)
            else:
                from sts2_env.monsters.act3 import create_axebot

                axebot, axebot_ai = create_axebot(combat.rng, stock_amount=self.amount - 1)
                combat.add_enemy(axebot, axebot_ai)


# ---------------------------------------------------------------------------
# StranglePower
# ---------------------------------------------------------------------------
class StranglePower(PowerInstance):
    """Each card played deals Amount unblockable+unpowered damage to the
    owner. Removed at end of any turn.

    C# ref: StranglePower.cs
    - BeforeCardPlayed: record Amount for the card.
    - AfterCardPlayed: deal recorded damage to owner.
    - AfterTurnEnd: remove.
    StackType.Counter.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.STRANGLE, amount)
        self._amounts_for_played_cards: dict[int, int] = {}

    def before_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        self._amounts_for_played_cards[id(card)] = self.amount

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        amount = self._amounts_for_played_cards.pop(id(card), 0)
        if amount <= 0:
            return
        combat.deal_damage(
            dealer=None,
            target=owner,
            amount=amount,
            props=ValueProp.UNBLOCKABLE | ValueProp.UNPOWERED,
        )

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        self.amount = 0


# ---------------------------------------------------------------------------
# StratagemPower
# ---------------------------------------------------------------------------
class StratagemPower(PowerInstance):
    """After a shuffle, choose Amount card(s) from draw pile to move to hand.

    C# ref: StratagemPower.cs
    - AfterShuffle: show draw pile grid, pick Amount cards to move to hand.
    StackType.Counter.

    Simplified: The combat pipeline handles the selection. This power
    signals the draw pile search to the agent.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.STRATAGEM, amount)

    def on_shuffle(self, owner: Creature, combat: CombatState) -> None:
        """Called after draw pile is shuffled. Triggers card selection."""
        search = getattr(combat, "search_draw_pile_to_hand", None)
        if search is not None:
            search(owner, self.amount)


# ---------------------------------------------------------------------------
# SubroutinePower
# ---------------------------------------------------------------------------
class SubroutinePower(PowerInstance):
    """Whenever you play a Power card, gain Amount energy.

    C# ref: SubroutinePower.cs
    - BeforeCardPlayed: record Amount for Power cards.
    - AfterCardPlayed: if Power card, gain Amount energy.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.SUBROUTINE, amount)
        self._amounts_for_played_cards: dict[int, int] = {}

    def before_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        if getattr(card, "owner", None) is not owner:
            return
        card_type = getattr(card, "card_type", None) or getattr(card, "type", None)
        if card_type == CardType.POWER:
            self._amounts_for_played_cards[id(card)] = self.amount

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        amount = self._amounts_for_played_cards.pop(id(card), 0)
        if amount <= 0:
            return
        gain_energy = getattr(combat, "gain_energy", None)
        if gain_energy is not None:
            gain_energy(owner, amount)


# ---------------------------------------------------------------------------
# SwipePower
# ---------------------------------------------------------------------------
class SwipePower(PowerInstance):
    """Tracks a stolen card. On the owner's death, the card is returned to
    the target player's deck.

    C# ref: SwipePower.cs
    - BeforeDeath: return stolen card to target player's deck.
    - Steal(): remove card from target's deck and store reference.
    StackType.Single. Instanced.

    Simplified: The monster AI calls steal/return. This power is a flag
    and data container.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.SINGLE

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.SWIPE, amount)
        self.stolen_card: object | None = None
        self.stolen_cards: list[tuple[object, Creature | None]] = []

    def steal(self, card: object, target: Creature | None = None) -> None:
        self.stolen_card = card
        self.stolen_cards.append((card, target))

    def before_death(self, owner: Creature, creature: Creature, combat: CombatState) -> None:
        if creature is not owner:
            return
        if self.stolen_cards:
            return_card = getattr(combat, "return_stolen_card", None)
            if return_card is not None:
                for card, target in list(self.stolen_cards):
                    return_card(card, target)
            self.stolen_cards.clear()
            self.stolen_card = None
            return
        if self.stolen_card is not None:
            return_card = getattr(combat, "return_stolen_card", None)
            if return_card is not None:
                return_card(self.stolen_card)
            self.stolen_card = None


# ---------------------------------------------------------------------------
# SwordSagePower
# ---------------------------------------------------------------------------
class SwordSagePower(PowerInstance):
    """Sovereign Blade cards repeat (Amount+1) times. Their energy cost
    increases by Amount.

    C# ref: SwordSagePower.cs
    - AfterPowerAmountChanged: update SovereignBlade repeats to Amount+1.
    - AfterCardEnteredCombat: same for new SovereignBlade cards.
    - AfterRemoved: reset SovereignBlade repeats to 1.
    - TryModifyEnergyCostInCombat: SovereignBlade cost += Amount.
    StackType.Counter.

    Simplified: Tracked as a flag. Card system checks for this power.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.SWORD_SAGE, amount)

    def get_sovereign_blade_repeats(self) -> int:
        """Returns the number of times Sovereign Blade should repeat."""
        return self.amount + 1

    def get_sovereign_blade_cost_increase(self) -> int:
        """Returns the extra cost for Sovereign Blade."""
        return self.amount

    def modify_card_cost(self, owner: Creature, card: object) -> int | None:
        from sts2_env.cards.status import is_sovereign_blade

        if not is_sovereign_blade(card):
            return None
        if getattr(card, "owner", None) is not owner:
            return None
        return getattr(card, "cost", 0) + self.amount


# ---------------------------------------------------------------------------
# SynchronizePower
# ---------------------------------------------------------------------------
class SynchronizePower(PowerInstance):
    """Temporary Focus from Synchronize card. Grants Focus on application,
    removes it at end of owner's turn.

    C# ref: SynchronizePower.cs extends TemporaryFocusPower.
    - BeforeApplied: apply Focus(Amount).
    - AfterTurnEnd (owner side): remove self, apply Focus(-Amount).
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER
    is_temporary = True

    def __init__(self, amount: int):
        super().__init__(PowerId.SYNCHRONIZE, amount)

    def after_power_amount_changed(
        self,
        owner: Creature,
        target: Creature,
        power_id: PowerId,
        amount: int,
        applier: Creature | None,
        source: object | None,
        combat: CombatState,
    ) -> None:
        if owner is target and power_id == self.power_id and amount != 0 and not self.consume_ignore_next_instance():
            owner.apply_power(PowerId.FOCUS, amount, applier=applier, source=source)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            owner.apply_power(PowerId.FOCUS, -self.amount)
            self.amount = 0


# ---------------------------------------------------------------------------
# TagTeamPower
# ---------------------------------------------------------------------------
class TagTeamPower(PowerInstance):
    """The next Attack card targeting this creature (from a different player)
    is played an extra Amount time(s). Then this power is removed.

    C# ref: TagTeamPower.cs
    - ModifyCardPlayCount: +Amount for Attack cards targeting owner,
      played by someone other than the applier.
    - AfterModifyingCardPlayCount: remove self.
    StackType.Counter. Instanced.

    Simplified: Triggers once for the next qualifying Attack.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.TAG_TEAM, amount)
        self._instances: list[tuple[int, Creature | None]] = [(amount, None)]
        self._triggered_instances_by_card: dict[int, list[int]] = {}

    def after_power_amount_changed(
        self,
        owner: Creature,
        target: Creature,
        power_id: PowerId,
        amount: int,
        applier: Creature | None,
        source: object | None,
        combat: CombatState,
    ) -> None:
        if owner is not target or power_id != self.power_id or amount <= 0:
            return
        if len(self._instances) == 1 and self._instances[0][1] is None:
            self._instances[0] = (self._instances[0][0], applier)
            return
        self._instances.append((amount, applier))

    def modify_card_play_count(self, owner: Creature, count: int, card: object) -> int:
        card_type = getattr(card, "card_type", None) or getattr(card, "type", None)
        if card_type != CardType.ATTACK:
            return count
        combat = getattr(owner, "combat_state", None)
        target = getattr(combat, "active_card_target", None) or getattr(card, "target", None)
        if target is not owner:
            return count
        card_owner = getattr(card, "owner", None)
        triggered_indices: list[int] = []
        for index, (amount, applier) in enumerate(self._instances):
            if card_owner is (applier or self.applier):
                continue
            count += amount
            triggered_indices.append(index)
        if triggered_indices:
            self._triggered_instances_by_card[id(card)] = triggered_indices
        return count

    def after_modifying_card_play_count(self, owner: Creature, card: object, combat: CombatState) -> None:
        triggered_indices = self._triggered_instances_by_card.pop(id(card), None)
        if triggered_indices is None:
            return
        triggered = set(triggered_indices)
        self._instances = [
            instance
            for index, instance in enumerate(self._instances)
            if index not in triggered
        ]
        if not self._instances:
            combat._remove_power(owner, self.power_id)
            return
        self.amount = sum(amount for amount, _ in self._instances)


# ---------------------------------------------------------------------------
# TangledPower
# ---------------------------------------------------------------------------
class TangledPower(PowerInstance):
    """All Attack cards cost Amount more energy. Removed at end of owner's turn.

    C# ref: TangledPower.cs
    - AfterApplied: afflict all Attack cards with Entangled.
    - TryModifyEnergyCostInCombat: Attack cost += Amount.
    - AfterTurnEnd (owner side): remove, clear afflictions.
    StackType.Counter.

    Simplified: The card-play system checks this power for cost modification.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER
    _AFFLICTION = "entangled"

    def __init__(self, amount: int):
        super().__init__(PowerId.TANGLED, amount)
        self._afflicted_initial_cards = False

    def _afflict_card(self, owner: Creature, card: object) -> None:
        card_owner = getattr(card, "owner", None)
        if card_owner is None:
            card.owner = owner
            card_owner = owner
        if card_owner is not owner:
            return
        card_type = getattr(card, "card_type", None) or getattr(card, "type", None)
        if card_type != CardType.ATTACK:
            return
        afflict = getattr(card, "afflict", None)
        if callable(afflict):
            afflict(self._AFFLICTION)
            return
        combat_vars = getattr(card, "combat_vars", None)
        if isinstance(combat_vars, dict) and combat_vars.get("_affliction") is None:
            combat_vars["_affliction"] = self._AFFLICTION

    def _afflict_all_attacks(self, owner: Creature, combat: CombatState) -> None:
        state = combat.combat_player_state_for(owner)
        if state is None:
            return
        for pile in state.all_piles:
            for card in pile:
                self._afflict_card(owner, card)

    def _clear_afflictions(self, owner: Creature, combat: CombatState) -> None:
        state = combat.combat_player_state_for(owner)
        if state is None:
            return
        for pile in state.all_piles:
            for card in pile:
                clear_affliction = getattr(card, "clear_affliction", None)
                if callable(clear_affliction):
                    clear_affliction(self._AFFLICTION)
                elif getattr(card, "combat_vars", {}).get("_affliction") == self._AFFLICTION:
                    card.combat_vars.pop("_affliction", None)

    def after_power_amount_changed(
        self,
        owner: Creature,
        target: Creature,
        power_id: PowerId,
        amount: int,
        applier: Creature | None,
        source: object | None,
        combat: CombatState,
    ) -> None:
        if owner is target and power_id == PowerId.TANGLED and amount > 0 and not self._afflicted_initial_cards:
            self._afflict_all_attacks(owner, combat)
            self._afflicted_initial_cards = True

    def after_card_entered_combat(self, owner: Creature, card: object, combat: CombatState) -> None:
        self._afflict_card(owner, card)

    def modify_card_cost(self, owner: Creature, card: object) -> int | None:
        """Increase Attack card cost by Amount."""
        card_owner = getattr(card, "owner", None)
        if card_owner is not owner:
            return None
        has_affliction = getattr(card, "has_affliction", None)
        if callable(has_affliction):
            if not has_affliction(self._AFFLICTION):
                return None
        elif getattr(card, "combat_vars", {}).get("_affliction") != self._AFFLICTION:
            return None
        card_type = getattr(card, "card_type", None) or getattr(card, "type", None)
        if card_type != CardType.ATTACK:
            return None
        base_cost = getattr(card, "cost", 0)
        return base_cost + self.amount

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            self.amount = 0
            self._clear_afflictions(owner, combat)

    def on_removed(self, owner: Creature, combat: CombatState) -> None:
        self._clear_afflictions(owner, combat)


# ---------------------------------------------------------------------------
# TankPower
# ---------------------------------------------------------------------------
class TankPower(PowerInstance):
    """Owner takes 2x damage from powered attacks but applies Guarded to
    all teammates (Amount block absorption).

    C# ref: TankPower.cs
    - AfterApplied: apply GuardedPower(Amount) to all player teammates.
    - ModifyDamageMultiplicative: 2x for powered attacks targeting owner.
    StackType.Single.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.SINGLE

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.TANK, amount)
        self._guarded_applied: bool = False

    def after_power_amount_changed(
        self,
        owner: Creature,
        target: Creature,
        power_id: PowerId,
        amount: int,
        applier: Creature | None,
        source: object | None,
        combat: CombatState,
    ) -> None:
        if owner is not target or power_id != PowerId.TANK or amount <= 0 or self._guarded_applied:
            return
        self._guarded_applied = True
        for teammate in combat.get_teammates_of(owner):
            if teammate.is_alive and teammate.is_player and teammate is not owner:
                combat.apply_power_to(teammate, PowerId.GUARDED, amount, applier=owner)

    def modify_damage_multiplicative(
        self, owner: Creature, dealer: Creature | None, target: Creature, props: ValueProp
    ) -> float:
        if target is not owner:
            return 1.0
        if not props.is_powered_attack():
            return 1.0
        return 2.0


# ---------------------------------------------------------------------------
# TenderPower
# ---------------------------------------------------------------------------
class TenderPower(PowerInstance):
    """Each card played by the owner reduces Strength and Dexterity by 1.
    At end of player turn, restore the lost Strength and Dexterity.

    C# ref: TenderPower.cs
    - AfterCardPlayed: if card owner matches, apply -1 Str and -1 Dex.
    - AfterTurnEnd (Player side): restore Str and Dex, reset counter.
    StackType.Counter. DisplayAmount = cards played this turn.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.TENDER, amount)
        self._cards_played_this_turn: int = 0

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        card_owner = getattr(card, "owner", None)
        if card_owner is owner and getattr(combat, "active_card_play_is_last_in_series", True):
            self._cards_played_this_turn += 1
            owner.apply_power(PowerId.STRENGTH, -1)
            owner.apply_power(PowerId.DEXTERITY, -1)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER:
            owner.apply_power(PowerId.STRENGTH, self._cards_played_this_turn)
            owner.apply_power(PowerId.DEXTERITY, self._cards_played_this_turn)
            self._cards_played_this_turn = 0


# ---------------------------------------------------------------------------
# TheGambitPower
# ---------------------------------------------------------------------------
class TheGambitPower(PowerInstance):
    """If the owner takes any unblocked damage from a powered attack,
    the owner is killed instantly.

    C# ref: TheGambitPower.cs
    - AfterDamageReceived: if target == owner and powered attack and
      unblocked > 0, remove self and kill owner.
    StackType.Single.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.SINGLE

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.THE_GAMBIT, amount)

    def after_damage_received(
        self, owner: Creature, target: Creature, dealer: Creature | None,
        damage: int, props: ValueProp, combat: CombatState
    ) -> None:
        if target is owner and props.is_powered_attack() and damage > 0:
            owner.powers.pop(self.power_id, None)
            combat.kill_creature(owner)


# ---------------------------------------------------------------------------
# TheHuntPower
# ---------------------------------------------------------------------------
class TheHuntPower(PowerInstance):
    """Flag power with no hooks. Counter that tracks hunt progress.

    C# ref: TheHuntPower.cs
    - No hooks. StackType.Counter.

    Used by The Hunt card as a counter for additional effects.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.THE_HUNT, amount)


# ---------------------------------------------------------------------------
# TheSealedThronePower
# ---------------------------------------------------------------------------
class TheSealedThronePower(PowerInstance):
    """Before playing any card, gain Amount star(s).

    C# ref: TheSealedThronePower.cs
    - BeforeCardPlayed: if card belongs to owner, gain Amount stars.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.THE_SEALED_THRONE, amount)

    def before_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        card_owner = getattr(card, "owner", None)
        if card_owner is owner:
            gain_stars = getattr(combat, "gain_stars", None)
            if gain_stars is not None:
                gain_stars(owner, self.amount)


# ---------------------------------------------------------------------------
# ToricToughnessPower
# ---------------------------------------------------------------------------
class ToricToughnessPower(PowerInstance):
    """When the owner's block is cleared (drops to 0 at turn start),
    gain block equal to a stored value. Decrements per trigger.

    C# ref: ToricToughnessPower.cs
    - AfterBlockCleared: if creature == owner, gain stored block, decrement.
    - SetBlock(block): stores the block value in DynamicVars.
    StackType.Counter. Instanced.

    Simplified: Block value is stored in _block_value. Triggers when block
    is cleared.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.TORIC_TOUGHNESS, amount)
        self._block_value: int = 0
        self._instances: list[tuple[int, int]] = [(amount, 0)]

    def set_block(self, block: int) -> None:
        """Set the block value to grant when block is cleared."""
        self._block_value = block
        amount, _ = self._instances[-1]
        self._instances[-1] = (amount, block)

    def after_power_amount_changed(
        self,
        owner: Creature,
        target: Creature,
        power_id: PowerId,
        amount: int,
        applier: Creature | None,
        source: object | None,
        combat: CombatState,
    ) -> None:
        if owner is target and power_id == self.power_id and amount > 0 and self.amount != amount:
            self._instances.append((amount, 0))

    def on_block_cleared(self, owner: Creature, combat: CombatState) -> None:
        """Called when owner's block is cleared at turn start."""
        remaining_instances: list[tuple[int, int]] = []
        for amount, block_value in self._instances:
            if block_value > 0:
                _gain_unpowered_block(owner, block_value, combat)
            amount -= 1
            if amount > 0:
                remaining_instances.append((amount, block_value))
        self._instances = remaining_instances
        if not self._instances:
            combat._remove_power(owner, self.power_id)
            return
        self.amount, self._block_value = self._instances[-1]

    def after_block_cleared(self, owner: Creature, creature: Creature, combat: CombatState) -> None:
        if creature is owner:
            self.on_block_cleared(owner, combat)


# ---------------------------------------------------------------------------
# TrackingPower
# ---------------------------------------------------------------------------
class TrackingPower(PowerInstance):
    """Powered attacks from owner (or owner's pets) deal Amount multiplier
    damage against Weak targets.

    C# ref: TrackingPower.cs
    - ModifyDamageMultiplicative: Amount multiplier when dealer is owner
      or pet, target has Weak, and it's a powered card attack.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.TRACKING, amount)

    def modify_damage_multiplicative(
        self, owner: Creature, dealer: Creature | None, target: Creature, props: ValueProp
    ) -> float:
        if not props.is_powered_attack():
            return 1.0
        if dealer is not owner:
            if not (dealer is not None and getattr(dealer, "pet_owner", None) is owner):
                return 1.0
        if target is None or not target.has_power(PowerId.WEAK):
            return 1.0
        return float(self.amount)


# ---------------------------------------------------------------------------
# TrashToTreasurePower
# ---------------------------------------------------------------------------
class TrashToTreasurePower(PowerInstance):
    """When a Status card is generated for the player, channel Amount random
    orb(s).

    C# ref: TrashToTreasurePower.cs
    - AfterCardGeneratedForCombat: if addedByPlayer and Status, channel
      Amount random orbs.
    StackType.Counter.

    Triggers through the generated-card combat hook.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.TRASH_TO_TREASURE, amount)

    def after_card_generated_for_combat(
        self,
        owner: Creature,
        card: object,
        added_by_player: bool,
        combat: CombatState,
    ) -> None:
        if not added_by_player or getattr(card, "card_type", None) != CardType.STATUS:
            return
        channel = getattr(combat, "channel_random_orb", None)
        if channel is None:
            return
        for _ in range(self.amount):
            channel(owner)


# ---------------------------------------------------------------------------
# TyrannyPower
# ---------------------------------------------------------------------------
class TyrannyPower(PowerInstance):
    """Draw Amount extra cards at turn start, then exhaust Amount cards
    from hand (player chooses).

    C# ref: TyrannyPower.cs
    - ModifyHandDraw: +Amount for owner's player.
    - AfterPlayerTurnStart: player selects Amount cards to exhaust.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.TYRANNY, amount)

    def modify_hand_draw(self, owner: Creature, draw: int) -> int:
        return draw + self.amount

    def after_player_turn_start(self, owner: Creature, combat: CombatState) -> None:
        if owner.is_player:
            exhaust = getattr(combat, "request_exhaust", None)
            if exhaust is not None:
                exhaust(owner, self.amount)


# ---------------------------------------------------------------------------
# UnmovablePower
# ---------------------------------------------------------------------------
class UnmovablePower(PowerInstance):
    """The first Amount block gain(s) per turn from card/move sources are
    doubled (2x multiplier). Subsequent block gains are normal.

    C# ref: UnmovablePower.cs
    - ModifyBlockMultiplicative: 2x if target is player, source is
      card/move, and fewer than Amount block gains this turn (per card play).
    StackType.Counter.

    Uses the combat block history so repeated block gains from the same card
    play are counted like the original BlockGainedEntry check.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.UNMOVABLE, amount)

    def modify_block_multiplicative(
        self, owner: Creature, target: Creature, props: ValueProp,
        card_source: object | None, card_play: object | None,
        combat: CombatState,
    ) -> float:
        if target.side == CombatSide.ENEMY:
            return 1.0
        if not props.is_card_or_monster_move():
            return 1.0
        if card_source is not None and getattr(card_source, "owner", None) is not owner:
            return 1.0
        count = combat.count_card_or_monster_move_block_gained_events_this_turn(
            target,
            exclude_card_play=card_play,
        )
        if count >= self.amount:
            return 1.0
        return 2.0


# ---------------------------------------------------------------------------
# VeilpiercerPower
# ---------------------------------------------------------------------------
class VeilpiercerPower(PowerInstance):
    """Ethereal cards in hand cost 0 energy. Decrements when an Ethereal
    card is played from hand.

    C# ref: VeilpiercerPower.cs
    - TryModifyEnergyCostInCombat: Ethereal cards in hand/play cost 0.
    - BeforeCardPlayed: if Ethereal card from hand, decrement.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.VEILPIERCER, amount)

    def modify_card_cost(self, owner: Creature, card: object) -> int | None:
        """Make Ethereal cards cost 0."""
        if self.amount <= 0:
            return None
        card_owner = getattr(card, "owner", None)
        if card_owner is not owner:
            return None
        keywords = getattr(card, "keywords", set())
        if CardKeyword.ETHEREAL not in keywords and "ethereal" not in keywords:
            return None
        return 0

    def before_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        card_owner = getattr(card, "owner", None)
        if card_owner is not owner:
            return
        keywords = getattr(card, "keywords", set())
        if CardKeyword.ETHEREAL in keywords or "ethereal" in keywords:
            self.amount -= 1


# ---------------------------------------------------------------------------
# VitalSparkPower
# ---------------------------------------------------------------------------
class VitalSparkPower(PowerInstance):
    """Afflicts every player ally's Skill cards with the Tainted affliction;
    playing an afflicted Skill inflicts TaintedPower on its owner.

    C# ref: VitalSparkPower.cs (v0.107.1)
    - PowerType.Buff, PowerStackType.Counter.
    - BeforeCombatStart (lines 25-39): for each player ally
      (CombatState.Allies filtered to IsPlayer, i.e. the player side),
      Afflict<Tainted>(Amount) every Skill card in PlayerCombatState.AllCards.
    - AfterCardEnteredCombat (lines 41-47): a card entering combat with
      Affliction == null and Type == Skill gets Afflict<Tainted>(Amount).
    - AfterCardPlayed (lines 49-56): if the played card's Affliction is
      Tainted, Apply<TaintedPower>(cardPlay.Card.Owner.Creature, Amount) —
      the card's OWNER takes the debuff.
    - AfterRemoved (lines 58-77): when the power is removed, clear Tainted
      from player allies' cards.
    - AfterPowerAmountChanged (lines 79-98): re-sync existing Tainted card
      affliction Amounts to the power's new Amount.

    Applier: InfestedPrism.AfterAddedToRoom (InfestedPrism.cs:56-60,
    VitalSparkAmount 2, 3 on DeadlyEnemies) and PulsateMove
    (InfestedPrism.cs:114).

    C# CardCmd.Afflict (CardCmd.cs:625-659): Tainted.IsStackable is true
    (Tainted.cs:13) and CanAfflictCardType is Skill-only (Tainted.cs:19-22);
    re-afflicting a card that already holds Tainted stacks Amount (line 656)
    while a different existing affliction makes the Afflict fail. The sim
    carries the affliction on CardInstance.combat_vars ("_affliction" +
    "_affliction_amount"); the runtime debuff is TaintedPower
    (damage_modifiers.py), which removes itself at the end of the enemy
    side's turn (TaintedPower.cs:31-38).
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER
    _AFFLICTION = "tainted"

    def __init__(self, amount: int):
        super().__init__(PowerId.VITAL_SPARK, amount)

    def _player_allies(self, combat: CombatState) -> list[Creature]:
        # C# iterates Owner.CombatState.Allies filtered to IsPlayer
        # (VitalSparkPower.cs:27-32); Allies is the player side
        # (CombatState.cs:58-60) regardless of the power owner's own side.
        return [state.creature for state in combat.combat_player_states]

    def _all_cards(self, creature: Creature, combat: CombatState) -> list:
        state = combat.combat_player_state_for(creature)
        if state is None:
            return []
        return list(combat.unique_cards_in_piles(state.all_piles))

    def _afflict_card(self, card: object) -> None:
        # Skill-only gate mirrors the .Where(c => c.Type == CardType.Skill)
        # at the C# call sites plus Tainted.CanAfflictCardType; the
        # stackable=True afflict mirrors CardCmd.Afflict's Amount stacking
        # (CardCmd.cs:656) and its no-op on a different existing affliction.
        if not getattr(card, "is_skill", False):
            return
        afflict = getattr(card, "afflict", None)
        if callable(afflict):
            afflict(self._AFFLICTION, stackable=True, amount=self.amount)

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        # C# BeforeCombatStart (VitalSparkPower.cs:25-39).
        for ally in self._player_allies(combat):
            for card in self._all_cards(ally, combat):
                self._afflict_card(card)

    def after_card_entered_combat(self, owner: Creature, card: object, combat: CombatState) -> None:
        # C# AfterCardEnteredCombat (VitalSparkPower.cs:41-47): only cards
        # with no affliction yet; Skills only.
        if getattr(card, "affliction", None) is not None:
            return
        self._afflict_card(card)

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        # C# AfterCardPlayed (VitalSparkPower.cs:49-56): the played card's
        # owner creature receives TaintedPower(Amount) — the player debuffs
        # themselves by playing afflicted Skills.
        if getattr(card, "affliction", None) != self._AFFLICTION:
            return
        card_owner = getattr(card, "owner", None)
        if card_owner is None:
            return
        card_owner.apply_power(PowerId.TAINTED, self.amount)

    def _clear_afflictions(self, combat: CombatState) -> None:
        # C# AfterRemoved (VitalSparkPower.cs:58-77): strip Tainted from
        # player allies' cards.
        for ally in self._player_allies(combat):
            for card in self._all_cards(ally, combat):
                clear_affliction = getattr(card, "clear_affliction", None)
                if callable(clear_affliction):
                    clear_affliction(self._AFFLICTION)

    def on_removed(self, owner: Creature, combat: CombatState) -> None:
        # C# AfterRemoved, dispatched via CombatState._remove_power
        # (creature death removes all powers through the same path).
        self._clear_afflictions(combat)

    def after_power_amount_changed(
        self,
        owner: Creature,
        target: Creature,
        power_id: PowerId,
        amount: int,
        applier: Creature | None,
        source: object | None,
        combat: CombatState,
    ) -> None:
        # C# AfterPowerAmountChanged (VitalSparkPower.cs:79-98): keep existing
        # Tainted card affliction Amounts at the power's current Amount.
        if power_id != PowerId.VITAL_SPARK or owner is not target:
            return
        for ally in self._player_allies(combat):
            for card in self._all_cards(ally, combat):
                if getattr(card, "affliction", None) == self._AFFLICTION:
                    card.combat_vars["_affliction_amount"] = self.amount


# ---------------------------------------------------------------------------
# VoidFormPower
# ---------------------------------------------------------------------------
class VoidFormPower(PowerInstance):
    """The first Amount card(s) played each turn cost 0 energy (and 0 stars).
    Tracks cards played this turn; resets at turn start.

    C# ref: VoidFormPower.cs
    - TryModifyEnergyCostInCombat: cost = 0 if cardsPlayedThisTurn < Amount
      and card is in hand/play and belongs to owner.
    - TryModifyStarCost: same logic.
    - AfterCardPlayed: increment counter (only non-autoplay, last in series).
    - BeforeSideTurnStart (owner side): reset counter.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.VOID_FORM, amount)
        self._cards_played_this_turn: int = 0

    def _covers_card(self, owner: Creature, card: object) -> bool:
        card_owner = getattr(card, "owner", None)
        if card_owner is not owner:
            return False
        combat = getattr(owner, "combat_state", None)
        if combat is None:
            return False
        state = combat.combat_player_state_for(owner)
        if state is None:
            return False
        return any(card is existing for existing in state.hand) or any(card is existing for existing in state.play)

    def modify_card_cost(self, owner: Creature, card: object) -> int | None:
        """Make the first Amount cards cost 0."""
        if not self._covers_card(owner, card):
            return None
        if self._cards_played_this_turn >= self.amount:
            return None
        return 0

    def modify_star_cost(self, owner: Creature, card: object) -> int | None:
        if not self._covers_card(owner, card):
            return None
        if self._cards_played_this_turn >= self.amount:
            return None
        return 0

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        card_owner = getattr(card, "owner", None)
        if card_owner is owner and getattr(combat, "active_card_play_is_last_in_series", True):
            self._cards_played_this_turn += 1

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            self._cards_played_this_turn = 0


# ---------------------------------------------------------------------------
# WasteAwayPower
# ---------------------------------------------------------------------------
class WasteAwayPower(PowerInstance):
    """Reduces max energy by Amount.

    C# ref: WasteAwayPower.cs
    - ModifyMaxEnergy: amount - Amount for owner's player.
    StackType.Counter.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.WASTE_AWAY, amount)

    def modify_max_energy(self, owner: Creature, energy: int) -> int:
        return energy - self.amount


# ---------------------------------------------------------------------------
# EntangledPower
# ---------------------------------------------------------------------------
class EntangledPower(PowerInstance):
    """Prevents the owner from playing Attack cards. Removed at end of turn.

    C# ref: EntangledPower.cs
    - ShouldPlay: return false for Attack cards.
    - AfterTurnEnd: remove self.
    StackType.Single.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.SINGLE

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.ENTANGLED, amount)

    def can_play_card(self, owner: Creature, card: object) -> bool:
        """Return False for Attack cards to prevent playing them."""
        if getattr(card, "card_type", None) == CardType.ATTACK:
            return False
        return True

    def after_turn_end(
        self, owner: Creature, side: CombatSide, combat: CombatState
    ) -> None:
        if side == owner.side:
            self.amount = 0


# ---------------------------------------------------------------------------
# MetallicizePower
# ---------------------------------------------------------------------------
class MetallicizePower(PowerInstance):
    """Gain Amount block at end of turn.

    C# ref: MetallicizePower.cs
    - BeforeTurnEndEarly (side == Owner.Side): gain Amount block (unpowered).
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.METALLICIZE, amount)

    def before_turn_end(
        self, owner: Creature, side: CombatSide, combat: CombatState
    ) -> None:
        if side == owner.side:
            owner.gain_block(self.amount)


# ---------------------------------------------------------------------------
# Registration
# ---------------------------------------------------------------------------
from sts2_env.core.creature import register_power_class  # noqa: E402

_ALL_POWERS: dict[PowerId, type[PowerInstance]] = {
    PowerId.REATTACH: ReattachPower,
    PowerId.REBOUND: ReboundPower,
    PowerId.REPTILE_TRINKET: ReptileTrinketPower,
    PowerId.RINGING: RingingPower,
    PowerId.ROLLING_BOULDER: RollingBoulderPower,
    PowerId.ROYALTIES: RoyaltiesPower,
    PowerId.SANDPIT: SandpitPower,
    PowerId.SEEKING_EDGE: SeekingEdgePower,
    PowerId.SENTRY_MODE: SentryModePower,
    PowerId.SHADOW_STEP: ShadowStepPower,
    PowerId.SHADOWMELD: ShadowmeldPower,
    PowerId.SHROUD: ShroudPower,
    PowerId.SIC_EM: SicEmPower,
    PowerId.SIGNAL_BOOST: SignalBoostPower,
    PowerId.SLEIGHT_OF_FLESH: SleightOfFleshPower,
    PowerId.SLOW: SlowPower,
    PowerId.SLUMBER: SlumberPower,
    PowerId.SMOKESTACK: SmokestackPower,
    PowerId.SOAR: SoarPower,
    PowerId.SPECTRUM_SHIFT: SpectrumShiftPower,
    PowerId.SPEEDSTER: SpeedsterPower,
    PowerId.SPIRIT_OF_ASH: SpiritOfAshPower,
    PowerId.STAMPEDE: StampedePower,
    PowerId.STOCK: StockPower,
    PowerId.STRANGLE: StranglePower,
    PowerId.STRATAGEM: StratagemPower,
    PowerId.SUBROUTINE: SubroutinePower,
    PowerId.SWIPE: SwipePower,
    PowerId.SWORD_SAGE: SwordSagePower,
    PowerId.SYNCHRONIZE: SynchronizePower,
    PowerId.TAG_TEAM: TagTeamPower,
    PowerId.TANGLED: TangledPower,
    PowerId.TANK: TankPower,
    PowerId.TENDER: TenderPower,
    PowerId.THE_GAMBIT: TheGambitPower,
    PowerId.THE_HUNT: TheHuntPower,
    PowerId.THE_SEALED_THRONE: TheSealedThronePower,
    PowerId.TORIC_TOUGHNESS: ToricToughnessPower,
    PowerId.TRACKING: TrackingPower,
    PowerId.TRASH_TO_TREASURE: TrashToTreasurePower,
    PowerId.TYRANNY: TyrannyPower,
    PowerId.UNMOVABLE: UnmovablePower,
    PowerId.VEILPIERCER: VeilpiercerPower,
    PowerId.VITAL_SPARK: VitalSparkPower,
    PowerId.VOID_FORM: VoidFormPower,
    PowerId.WASTE_AWAY: WasteAwayPower,
    PowerId.ENTANGLED: EntangledPower,
    PowerId.METALLICIZE: MetallicizePower,
}

for _pid, _cls in _ALL_POWERS.items():
    register_power_class(_pid, _cls)

# Register FREE as an alias for FreePowerPower (from card_play_effects)
from sts2_env.powers.card_play_effects import FreePowerPower as _FreePowerPower  # noqa: E402
register_power_class(PowerId.FREE, _FreePowerPower)
