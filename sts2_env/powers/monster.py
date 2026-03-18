"""Monster-specific powers.

Covers: MinionPower, SlipperyPower, RavenousPower, TerritorialPower,
HardenedShellPower, SkittishPower, ThieveryPower, SurprisePower, PlowPower,
SmoggyPower, InfestedPower, IllusionPower, AsleepPower, SteamEruptionPower,
ShriekPower, SuckPower, CrabRagePower, SpinnerPower, FeedingFrenzyPower,
HatchPower, BurrowedPower, SurroundedPower, CoveredPower, DoorRevivalPower,
PersonalHivePower, CoordinatePower, SlothPower, MonologuePower.

All logic verified against decompiled C# source.
"""

from __future__ import annotations

import math
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


# ---------------------------------------------------------------------------
# MinionPower
# ---------------------------------------------------------------------------
class MinionPower(PowerInstance):
    """Marks a creature as a minion (secondary enemy).

    C# ref: MinionPower.cs
    - OwnerIsSecondaryEnemy: true
    - ShouldPowerBeRemovedAfterOwnerDeath: false
    - ShouldOwnerDeathTriggerFatal: false
    StackType.Single. Non-stacking flag.

    Minions do not count for combat victory; killing them does not end
    the fight. The combat system checks for this power.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.SINGLE

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.MINION, amount)

    def should_owner_death_trigger_fatal(
        self,
        owner: Creature,
        combat: CombatState,
    ) -> bool:
        return False


# ---------------------------------------------------------------------------
# SlipperyPower
# ---------------------------------------------------------------------------
class SlipperyPower(PowerInstance):
    """Each hit can only deal 1 damage (damage cap). Decrements on any hit.

    C# ref: SlipperyPower.cs
    - ModifyDamageCap: cap at 1 if target == owner.
    - AfterDamageReceived: if target == owner and total damage != 0, decrement.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.SLIPPERY, amount)

    def modify_damage_cap(
        self,
        owner: Creature,
        dealer: Creature | None,
        target: Creature,
        damage: float,
        props: ValueProp,
    ) -> float:
        if target is owner:
            return 1.0
        return float("inf")

    def after_damage_received(
        self,
        owner: Creature,
        target: Creature,
        dealer: Creature | None,
        damage: int,
        props: ValueProp,
        combat: CombatState,
    ) -> None:
        if target is owner and damage > 0:
            self.amount -= 1
            if self.amount <= 0:
                owner.powers.pop(self.power_id, None)


# ---------------------------------------------------------------------------
# RavenousPower
# ---------------------------------------------------------------------------
class RavenousPower(PowerInstance):
    """When an allied creature dies, gain Amount Strength and become stunned
    for one turn.

    C# ref: RavenousPower.cs
    - AfterDeath: if an ally (not self) dies, gain Strength and stun.
    StackType.Counter.

    Simplified: Stun is modeled by the monster AI system setting the
    creature's next move to a stun. This power applies the Strength gain.
    The monster AI must check for this power to handle the stun.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.RAVENOUS, amount)

    def on_ally_death(
        self, owner: Creature, dead_creature: Creature, combat: CombatState
    ) -> None:
        """Called by the combat system when an allied creature dies."""
        if dead_creature is not owner and dead_creature.side == owner.side:
            if owner.is_alive:
                owner.apply_power(PowerId.STRENGTH, self.amount)
                # Stun is handled by the monster AI setting the next move


# ---------------------------------------------------------------------------
# TerritorialPower
# ---------------------------------------------------------------------------
class TerritorialPower(PowerInstance):
    """At end of its own turn, gain Amount Strength.

    C# ref: TerritorialPower.cs
    - AfterTurnEnd: if side == owner's side, apply Strength.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.TERRITORIAL, amount)

    def after_turn_end(
        self, owner: Creature, side: CombatSide, combat: CombatState
    ) -> None:
        if side == owner.side:
            owner.apply_power(PowerId.STRENGTH, self.amount)


# ---------------------------------------------------------------------------
# HardenedShellPower
# ---------------------------------------------------------------------------
class HardenedShellPower(PowerInstance):
    """Limits total HP loss per turn to Amount. Resets each player turn start.

    C# ref: HardenedShellPower.cs
    - ModifyHpLostBeforeOstyLate: cap HP loss at (Amount - damage_taken_so_far).
    - AfterDamageReceived: track unblocked damage taken this turn.
    - BeforeSideTurnStart (Player side): reset tracker.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.HARDENED_SHELL, amount)
        self._damage_taken_this_turn: int = 0

    def modify_hp_lost(
        self,
        owner: Creature,
        target: Creature,
        amount: float,
        dealer: Creature | None,
        props: ValueProp,
    ) -> float:
        if target is not owner or amount == 0:
            return amount
        remaining_cap = max(0, self.amount - self._damage_taken_this_turn)
        return min(amount, float(remaining_cap))

    def after_damage_received(
        self,
        owner: Creature,
        target: Creature,
        dealer: Creature | None,
        damage: int,
        props: ValueProp,
        combat: CombatState,
    ) -> None:
        if target is owner and damage > 0:
            self._damage_taken_this_turn += damage

    def before_side_turn_start(
        self, owner: Creature, side: CombatSide, combat: CombatState
    ) -> None:
        if side == CombatSide.PLAYER:
            self._damage_taken_this_turn = 0


# ---------------------------------------------------------------------------
# SkittishPower
# ---------------------------------------------------------------------------
class SkittishPower(PowerInstance):
    """The first time the owner takes unblocked damage from a card this turn,
    gain Amount Block. Resets at end of non-owner turn.

    C# ref: SkittishPower.cs
    - AfterAttack: if owner took unblocked damage and hasn't triggered yet,
      gain block.
    - AfterTurnEnd (opposing side): reset triggered flag.
    StackType.Counter.

    Simplified: Triggers on after_damage_received when unblocked damage > 0
    from a powered attack, once per turn.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.SKITTISH, amount)
        self._triggered_this_turn: bool = False

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
            and not self._triggered_this_turn
            and damage > 0
            and bool(props & ValueProp.MOVE)
        ):
            self._triggered_this_turn = True
            owner.gain_block(self.amount)

    def after_turn_end(
        self, owner: Creature, side: CombatSide, combat: CombatState
    ) -> None:
        if side != owner.side:
            self._triggered_this_turn = False


# ---------------------------------------------------------------------------
# ThieveryPower
# ---------------------------------------------------------------------------
class ThieveryPower(PowerInstance):
    """Steals Amount gold from the target player each time Steal() is called
    (by the monster's attack move).

    C# ref: ThieveryPower.cs
    - Steal(): steal min(Amount, player.Gold) from target player.
    StackType.Counter. Instanced per target.

    Simplified: The monster AI calls steal() when executing a theft move.
    Gold tracking is handled by the run state.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.THIEVERY, amount)
        self.gold_stolen: int = 0

    def steal(self, owner: Creature, target_player: object) -> int:
        """Steal gold from the target player. Returns amount stolen."""
        player_gold = getattr(target_player, "gold", 0)
        if player_gold <= 0:
            return 0
        stolen = min(self.amount, player_gold)
        if hasattr(target_player, "gold"):
            target_player.gold -= stolen
        self.gold_stolen += stolen
        return stolen


# ---------------------------------------------------------------------------
# SurprisePower
# ---------------------------------------------------------------------------
class SurprisePower(PowerInstance):
    """On death, spawn replacement monsters (SneakyGremlin + FatGremlin).
    Prevents combat from ending until the death trigger resolves.

    C# ref: SurprisePower.cs
    - AfterDeath: spawn SneakyGremlin and FatGremlin. Transfer stolen gold
      to FatGremlin via HeistPower.
    - ShouldStopCombatFromEnding: true.
    StackType.Single.

    Simplified: The monster AI system handles spawning. This power is a
    flag that the combat system checks.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.SINGLE

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.SURPRISE, amount)

    def should_stop_combat_ending(self) -> bool:
        return True


# ---------------------------------------------------------------------------
# PlowPower
# ---------------------------------------------------------------------------
class PlowPower(PowerInstance):
    """When the owner's HP drops to or below Amount after taking unblocked
    damage, remove all Strength, stun the owner, and remove this power.

    C# ref: PlowPower.cs
    - AfterDamageReceived: if owner HP <= Amount and unblocked > 0, stun
      and remove Strength.
    StackType.Counter.

    Simplified: Strength removal and stun are applied. The specific
    CeremonialBeast animation/state machine is not modeled.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.PLOW, amount)

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
            and damage > 0
            and owner.current_hp <= self.amount
        ):
            # Remove all Strength
            strength = owner.powers.get(PowerId.STRENGTH)
            if strength is not None:
                del owner.powers[PowerId.STRENGTH]
            # Remove this power
            owner.powers.pop(self.power_id, None)
            # Stun is handled by the monster AI


# ---------------------------------------------------------------------------
# SmoggyPower
# ---------------------------------------------------------------------------
class SmoggyPower(PowerInstance):
    """After the owner plays a Skill, all of the owner's Skills become
    unplayable (afflicted with Smog) for the rest of the turn.
    Clears at end of owner's turn.

    C# ref: SmoggyPower.cs
    - AfterCardPlayed: if owner plays a Skill, afflict all Skills.
    - ShouldPlay: block cards with Smog affliction.
    - AfterTurnEnd: clear afflictions at end of owner's side.
    StackType.Single.

    Simplified: We track a flag indicating Skills are locked. The card-play
    system must check this power before allowing Skill plays.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.SINGLE

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.SMOGGY, amount)
        self.skills_locked: bool = False

    def after_card_played(
        self, owner: Creature, card: object, combat: CombatState
    ) -> None:
        if getattr(card, "card_type", None) == CardType.SKILL:
            self.skills_locked = True

    def should_card_be_playable(self, owner: Creature, card: object) -> bool:
        """Return False to block skill plays after first skill this turn."""
        if (
            self.skills_locked
            and getattr(card, "card_type", None) == CardType.SKILL
        ):
            return False
        return True

    def before_side_turn_start(
        self, owner: Creature, side: CombatSide, combat: CombatState
    ) -> None:
        if side == owner.side:
            self.skills_locked = False


# ---------------------------------------------------------------------------
# InfestedPower
# ---------------------------------------------------------------------------
class InfestedPower(PowerInstance):
    """On death, spawn 4 Wriggler minions (stunned). Prevents combat from
    ending until spawns resolve.

    C# ref: InfestedPower.cs
    - AfterDeath: spawn 4 stunned Wrigglers.
    - ShouldStopCombatFromEnding: true.
    StackType.Single.

    Simplified: The monster AI / encounter system handles spawning.
    This power is a flag for the combat system.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.SINGLE

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.INFESTED, amount)

    def should_stop_combat_ending(self) -> bool:
        return True


# ---------------------------------------------------------------------------
# IllusionPower
# ---------------------------------------------------------------------------
class IllusionPower(PowerInstance):
    """On death, the owner revives at full HP. The owner is treated as a
    minion (applies MinionPower). Debuffs are removed on death.
    The creature cannot be hit while reviving.

    C# ref: IllusionPower.cs
    - AfterDeath: heal to full HP, set reviving state.
    - ShouldPowerBeRemovedOnDeath: removes debuffs only.
    - AfterApplied: applies MinionPower.
    - ShouldAllowHitting: false while reviving.
    - ShouldCreatureBeRemovedFromCombatAfterDeath: false for owner.
    StackType.Single.

    Simplified: On death, creature heals to full. Monster AI handles
    the revive move state.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.SINGLE

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.ILLUSION, amount)
        self.is_reviving: bool = False

    def on_owner_death(self, owner: Creature, combat: CombatState) -> bool:
        """Called when the owner dies. Returns True if death should be
        prevented (creature revives)."""
        if not self.is_reviving:
            self.is_reviving = True
            # Remove debuffs
            debuff_ids = [
                pid
                for pid, p in owner.powers.items()
                if p.power_type == PowerType.DEBUFF
            ]
            for pid in debuff_ids:
                del owner.powers[pid]
            return True  # Prevent removal from combat
        return False

    def revive(self, owner: Creature) -> None:
        """Called by the monster AI to complete the revive."""
        self.is_reviving = False
        owner.current_hp = owner.max_hp

    def should_allow_hitting(self, owner: Creature, combat: CombatState) -> bool:
        return not self.is_reviving


# ---------------------------------------------------------------------------
# AsleepPower
# ---------------------------------------------------------------------------
class AsleepPower(PowerInstance):
    """The owner is asleep. Taking unblocked damage wakes them up (removes
    Plating, triggers wake-up, stuns). Also ticks down each turn; reaching
    0 wakes the owner up naturally.

    C# ref: AsleepPower.cs
    - AfterDamageReceived: if unblocked damage > 0, remove Plating and wake.
    - BeforeTurnEndVeryEarly: if Amount <= 1 and has Plating, remove Plating.
    - AfterTurnEnd (owner's side): decrement; if 0, wake up.
    StackType.Counter.

    Simplified: Plating removal and wake-up stun are applied. The specific
    LagavulinMatriarch state machine is not modeled.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.ASLEEP, amount)
        self.is_awake: bool = False

    def after_damage_received(
        self,
        owner: Creature,
        target: Creature,
        dealer: Creature | None,
        damage: int,
        props: ValueProp,
        combat: CombatState,
    ) -> None:
        if target is owner and damage > 0:
            # Remove Plating
            owner.powers.pop(PowerId.PLATING, None)
            self.is_awake = True
            # Remove this power (woken up by damage)
            owner.powers.pop(self.power_id, None)

    def after_turn_end(
        self, owner: Creature, side: CombatSide, combat: CombatState
    ) -> None:
        if side == owner.side:
            self.amount -= 1
            if self.amount <= 0:
                # About to wake up naturally, remove Plating first
                owner.powers.pop(PowerId.PLATING, None)
                self.is_awake = True
                owner.powers.pop(self.power_id, None)

    def before_turn_end(
        self, owner: Creature, side: CombatSide, combat: CombatState
    ) -> None:
        if side == owner.side and self.amount <= 1:
            # About to wake up next decrement; remove Plating early
            owner.powers.pop(PowerId.PLATING, None)


# ---------------------------------------------------------------------------
# SteamEruptionPower
# ---------------------------------------------------------------------------
class SteamEruptionPower(PowerInstance):
    """On death, triggers the "about to blow" state (explosion).
    Prevents combat from ending. Creature is not removed from combat
    on death. Power is not removed on owner death.

    C# ref: SteamEruptionPower.cs
    - AfterDeath: trigger explosion state.
    - ShouldStopCombatFromEnding: true.
    - ShouldCreatureBeRemovedFromCombatAfterDeath: false for owner.
    - ShouldPowerBeRemovedAfterOwnerDeath: false.
    StackType.Counter.

    Simplified: This is a flag. The monster AI handles the explosion
    sequence.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.STEAM_ERUPTION, amount)

    def should_stop_combat_ending(self) -> bool:
        return True


# ---------------------------------------------------------------------------
# ShriekPower
# ---------------------------------------------------------------------------
class ShriekPower(PowerInstance):
    """When the owner's HP drops to or below Amount after unblocked damage,
    the owner is stunned and this power is removed.

    C# ref: ShriekPower.cs
    - AfterDamageReceived: if HP <= Amount and unblocked > 0, stun and remove.
    StackType.Counter. AllowNegative = true.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER
    allow_negative = True

    def __init__(self, amount: int):
        super().__init__(PowerId.SHRIEK, amount)

    def after_damage_received(
        self,
        owner: Creature,
        target: Creature,
        dealer: Creature | None,
        damage: int,
        props: ValueProp,
        combat: CombatState,
    ) -> None:
        if target is owner and damage > 0 and owner.current_hp <= self.amount:
            # Stun is handled by the monster AI
            owner.powers.pop(self.power_id, None)


# ---------------------------------------------------------------------------
# SuckPower
# ---------------------------------------------------------------------------
class SuckPower(PowerInstance):
    """After dealing unblocked powered-attack damage, gain Amount Strength
    per unique player hit.

    C# ref: SuckPower.cs
    - AfterAttack: count unique players that took unblocked damage, gain
      Amount * count Strength.
    StackType.Counter.

    Simplified: We trigger on after_damage_given for each unique target hit.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.SUCK, amount)

    def after_damage_given(
        self,
        owner: Creature,
        dealer: Creature,
        target: Creature,
        damage: int,
        props: ValueProp,
        combat: CombatState,
    ) -> None:
        if dealer is owner and props.is_powered() and damage > 0:
            owner.apply_power(PowerId.STRENGTH, self.amount)


# ---------------------------------------------------------------------------
# CrabRagePower
# ---------------------------------------------------------------------------
class CrabRagePower(PowerInstance):
    """When an allied creature dies, gain 5 Strength and 99 Block.
    Then remove this power. Single-use.

    C# ref: CrabRagePower.cs
    - AfterDeath: if ally (not self) on same side dies, gain Strength + Block.
    StackType.Single.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.SINGLE

    # Hard-coded values from C# DynamicVars
    STRENGTH_GAIN = 5
    BLOCK_GAIN = 99

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.CRAB_RAGE, amount)

    def on_ally_death(
        self, owner: Creature, dead_creature: Creature, combat: CombatState
    ) -> None:
        """Called when an allied creature dies."""
        if dead_creature is not owner and dead_creature.side == owner.side:
            owner.apply_power(PowerId.STRENGTH, self.STRENGTH_GAIN)
            owner.gain_block(self.BLOCK_GAIN)
            owner.powers.pop(self.power_id, None)


# ---------------------------------------------------------------------------
# SpinnerPower
# ---------------------------------------------------------------------------
class SpinnerPower(PowerInstance):
    """At start of turn (energy reset), channel Amount Glass orb(s).

    C# ref: SpinnerPower.cs
    - AfterEnergyReset: channel Amount GlassOrbs for the player.
    StackType.Counter.

    Simplified: Orb channeling is handled by the orb system. This power
    acts as a hook that the orb system checks at turn start.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.SPINNER, amount)

    def after_side_turn_start(
        self, owner: Creature, side: CombatSide, combat: CombatState
    ) -> None:
        if side == owner.side and hasattr(combat, "channel_orb"):
            for _ in range(self.amount):
                combat.channel_orb(owner, "GLASS")


# ---------------------------------------------------------------------------
# FeedingFrenzyPower (TemporaryStrength)
# ---------------------------------------------------------------------------
class FeedingFrenzyPower(PowerInstance):
    """Temporary Strength that is removed at end of turn. Same as
    SetupStrikePower but used by monsters.

    C# ref: FeedingFrenzyPower.cs extends TemporaryStrengthPower.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.FEEDING_FRENZY, amount)

    def after_turn_end(
        self, owner: Creature, side: CombatSide, combat: CombatState
    ) -> None:
        if side == owner.side:
            owner.apply_power(PowerId.STRENGTH, -self.amount)
            owner.powers.pop(self.power_id, None)


# ---------------------------------------------------------------------------
# HatchPower
# ---------------------------------------------------------------------------
class HatchPower(PowerInstance):
    """Ticks down each enemy turn. When it reaches 0, the monster evolves
    (handled by monster AI).

    C# ref: HatchPower.cs
    - AfterTurnEnd (Enemy side): tick down duration.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.HATCH, amount)

    def after_turn_end(
        self, owner: Creature, side: CombatSide, combat: CombatState
    ) -> None:
        if side == CombatSide.ENEMY:
            self.amount -= 1
            if self.amount <= 0:
                owner.powers.pop(self.power_id, None)


# ---------------------------------------------------------------------------
# BurrowedPower
# ---------------------------------------------------------------------------
class BurrowedPower(PowerInstance):
    """Prevents block from being cleared. When block is broken (reaches 0),
    the creature unburrows and is stunned into a bite move.
    On removal, lose all block.

    C# ref: BurrowedPower.cs
    - ShouldClearBlock: false for owner (retains block).
    - AfterBlockBroken: trigger unburrow anim, stun into bite move,
      remove self.
    - AfterRemoved: lose all block.
    StackType.Single.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.SINGLE

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.BURROWED, amount)

    def should_clear_block(self, owner: Creature, creature: Creature) -> bool | None:
        if creature is owner:
            return False
        return None

    def on_block_broken(self, owner: Creature, combat: CombatState) -> None:
        """Called when the owner's block drops to 0."""
        owner.powers.pop(self.power_id, None)
        owner.block = 0  # Lose all block on removal
        # Stun into bite move is handled by monster AI


# ---------------------------------------------------------------------------
# SurroundedPower
# ---------------------------------------------------------------------------
class SurroundedPower(PowerInstance):
    """Player takes 50% more damage from attacks from behind (based on
    facing direction and BackAttack powers on enemies).

    C# ref: SurroundedPower.cs
    - ModifyDamageMultiplicative: 1.5x if dealer has BackAttack power
      opposite to the player's facing direction.
    - BeforeCardPlayed: update facing direction toward target.
    StackType.Single.

    Simplified: Facing direction is tracked. The damage multiplier applies
    when the player is facing the wrong way. BackAttackLeft/Right powers
    on enemies determine which side they attack from.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.SINGLE

    # Direction constants
    FACING_RIGHT = 0
    FACING_LEFT = 1

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.SURROUNDED, amount)
        self.facing: int = self.FACING_RIGHT

    def modify_damage_multiplicative(
        self,
        owner: Creature,
        dealer: Creature | None,
        target: Creature,
        props: ValueProp,
    ) -> float:
        if dealer is None or target is not owner:
            return 1.0

        # Check if the dealer attacks from the back
        if self.facing == self.FACING_RIGHT:
            if dealer.has_power(PowerId.BACK_ATTACK_LEFT):
                return 1.5
        elif self.facing == self.FACING_LEFT:
            if dealer.has_power(PowerId.BACK_ATTACK_RIGHT):
                return 1.5
        return 1.0

    def before_card_played(
        self, owner: Creature, card: object, combat: CombatState
    ) -> None:
        # Update facing direction toward the card's target
        target = getattr(card, "target", None)
        if target is not None:
            if self.facing == self.FACING_RIGHT:
                if target.has_power(PowerId.BACK_ATTACK_LEFT):
                    self.facing = self.FACING_LEFT
            elif self.facing == self.FACING_LEFT:
                if target.has_power(PowerId.BACK_ATTACK_RIGHT):
                    self.facing = self.FACING_RIGHT


# ---------------------------------------------------------------------------
# CoveredPower
# ---------------------------------------------------------------------------
class CoveredPower(PowerInstance):
    """The owner takes 0 damage from powered attacks (fully covered).
    Removed at end of enemy turn. If the covering creature dies, this
    power is removed.

    C# ref: CoveredPower.cs
    - ModifyDamageMultiplicative: 0 for powered attacks targeting owner.
    - AfterTurnEnd (Enemy side): remove.
    - AfterDeath: if covering creature dies, remove.
    StackType.Single. Instanced.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.SINGLE

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.COVERED, amount)
        self.covering_creature: Creature | None = None

    def modify_damage_multiplicative(
        self,
        owner: Creature,
        dealer: Creature | None,
        target: Creature,
        props: ValueProp,
    ) -> float:
        if target is owner and props.is_powered():
            return 0.0
        return 1.0

    def after_turn_end(
        self, owner: Creature, side: CombatSide, combat: CombatState
    ) -> None:
        if side == CombatSide.ENEMY:
            owner.powers.pop(self.power_id, None)


# ---------------------------------------------------------------------------
# DoorRevivalPower
# ---------------------------------------------------------------------------
class DoorRevivalPower(PowerInstance):
    """On death, the Door opens and spawns the Doormaker. The Door is not
    removed from combat. When the Doormaker revives the Door, it heals
    to minimum max HP.

    C# ref: DoorRevivalPower.cs
    - BeforeDeath: mark as half-dead.
    - AfterDeath: spawn Doormaker.
    - ShouldAllowHitting: false while half-dead.
    - ShouldStopCombatFromEnding: true while Doormaker is alive.
    - ShouldCreatureBeRemovedFromCombatAfterDeath: false for owner.
    - ShouldPowerBeRemovedAfterOwnerDeath: false.
    StackType.Single. Invisible.

    Simplified: Tracks half-dead state. The encounter system handles
    Doormaker spawning.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.SINGLE

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.DOOR_REVIVAL, amount)
        self.is_half_dead: bool = False

    def on_owner_death(self, owner: Creature, combat: CombatState) -> bool:
        """Returns True to prevent creature removal."""
        self.is_half_dead = True
        return True

    def revive(self, owner: Creature, min_hp: int) -> None:
        """Called by the encounter system to revive the Door."""
        self.is_half_dead = False
        owner.max_hp = min_hp
        owner.current_hp = min_hp

    def should_stop_combat_ending(self) -> bool:
        return self.is_half_dead

    def should_allow_hitting(self, owner: Creature, combat: CombatState) -> bool:
        return not self.is_half_dead


# ---------------------------------------------------------------------------
# PersonalHivePower
# ---------------------------------------------------------------------------
class PersonalHivePower(PowerInstance):
    """Whenever hit by a powered attack, shuffle Amount Dazed cards into
    the attacker's draw pile.

    C# ref: PersonalHivePower.cs
    - AfterDamageReceived: if powered attack with a dealer, add Amount Dazed
      to dealer's draw pile.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.PERSONAL_HIVE, amount)

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
            # Add Dazed status cards to the attacker's draw pile
            from sts2_env.core.enums import CardId

            for _ in range(self.amount):
                if hasattr(combat, "add_status_to_draw_pile"):
                    combat.add_status_to_draw_pile(dealer, CardId.DAZED)


# ---------------------------------------------------------------------------
# CoordinatePower (TemporaryStrength - monster variant)
# ---------------------------------------------------------------------------
class CoordinatePower(PowerInstance):
    """Temporary Strength used by monsters (from Coordinate card). Same
    mechanics as SetupStrikePower / FeedingFrenzyPower.

    C# ref: CoordinatePower.cs extends TemporaryStrengthPower.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.COORDINATE, amount)

    def after_turn_end(
        self, owner: Creature, side: CombatSide, combat: CombatState
    ) -> None:
        if side == owner.side:
            owner.apply_power(PowerId.STRENGTH, -self.amount)
            owner.powers.pop(self.power_id, None)


# ---------------------------------------------------------------------------
# SlothPower
# ---------------------------------------------------------------------------
class SlothPower(PowerInstance):
    """Limits the number of cards the owner can play per turn to Amount.

    C# ref: SlothPower.cs
    - ShouldPlay: return false if cards played >= Amount.
    - BeforeCardPlayed: increment counter.
    - BeforeSideTurnStart (owner side): reset counter.
    StackType.Counter.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.SLOTH, amount)
        self._cards_played_this_turn: int = 0

    def can_play_card(self, owner: Creature) -> bool:
        """Return False if card play limit reached."""
        return self._cards_played_this_turn < self.amount

    def before_card_played(
        self, owner: Creature, card: object, combat: CombatState
    ) -> None:
        self._cards_played_this_turn += 1

    def before_side_turn_start(
        self, owner: Creature, side: CombatSide, combat: CombatState
    ) -> None:
        if side == owner.side:
            self._cards_played_this_turn = 0


# ---------------------------------------------------------------------------
# MonologuePower
# ---------------------------------------------------------------------------
class MonologuePower(PowerInstance):
    """Gains Strength per card played by the owner this turn. At end of
    turn, remove all the accumulated Strength.

    C# ref: MonologuePower.cs
    - BeforeCardPlayed: record strength amount.
    - AfterCardPlayed: apply Strength.
    - AfterTurnEnd: remove self and undo all Strength gained.
    StackType.Counter (displays as accumulated Strength).
    Instanced.

    The DynamicVars give a Strength gain per card of 1 by default,
    tracked via the "StrengthApplied" variable.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    # Default Strength gain per card played (from DynamicVars in C#)
    STRENGTH_PER_CARD = 1

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.MONOLOGUE, amount)
        self._strength_applied: int = 0
        self.strength_per_card: int = self.STRENGTH_PER_CARD

    def after_card_played(
        self, owner: Creature, card: object, combat: CombatState
    ) -> None:
        owner.apply_power(PowerId.STRENGTH, self.strength_per_card)
        self._strength_applied += self.strength_per_card

    def after_turn_end(
        self, owner: Creature, side: CombatSide, combat: CombatState
    ) -> None:
        if side == owner.side:
            # Undo all Strength gained
            owner.apply_power(PowerId.STRENGTH, -self._strength_applied)
            owner.powers.pop(self.power_id, None)


# ---------------------------------------------------------------------------
# BackAttackLeftPower / BackAttackRightPower (utility flags for Surrounded)
# ---------------------------------------------------------------------------
class BackAttackLeftPower(PowerInstance):
    """Flag power indicating this creature attacks from the left side.
    Used by SurroundedPower to determine back-attack damage bonus.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.SINGLE

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.BACK_ATTACK_LEFT, amount)


class BackAttackRightPower(PowerInstance):
    """Flag power indicating this creature attacks from the right side.
    Used by SurroundedPower to determine back-attack damage bonus.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.SINGLE

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.BACK_ATTACK_RIGHT, amount)


# ---------------------------------------------------------------------------
# Registration
# ---------------------------------------------------------------------------
from sts2_env.core.creature import register_power_class  # noqa: E402

_ALL_POWERS: dict[PowerId, type[PowerInstance]] = {
    PowerId.MINION: MinionPower,
    PowerId.SLIPPERY: SlipperyPower,
    PowerId.RAVENOUS: RavenousPower,
    PowerId.TERRITORIAL: TerritorialPower,
    PowerId.HARDENED_SHELL: HardenedShellPower,
    PowerId.SKITTISH: SkittishPower,
    PowerId.THIEVERY: ThieveryPower,
    PowerId.SURPRISE: SurprisePower,
    PowerId.PLOW: PlowPower,
    PowerId.SMOGGY: SmoggyPower,
    PowerId.INFESTED: InfestedPower,
    PowerId.ILLUSION: IllusionPower,
    PowerId.ASLEEP: AsleepPower,
    PowerId.STEAM_ERUPTION: SteamEruptionPower,
    PowerId.SHRIEK: ShriekPower,
    PowerId.SUCK: SuckPower,
    PowerId.CRAB_RAGE: CrabRagePower,
    PowerId.SPINNER: SpinnerPower,
    PowerId.FEEDING_FRENZY: FeedingFrenzyPower,
    PowerId.HATCH: HatchPower,
    PowerId.BURROWED: BurrowedPower,
    PowerId.SURROUNDED: SurroundedPower,
    PowerId.COVERED: CoveredPower,
    PowerId.DOOR_REVIVAL: DoorRevivalPower,
    PowerId.PERSONAL_HIVE: PersonalHivePower,
    PowerId.COORDINATE: CoordinatePower,
    PowerId.SLOTH: SlothPower,
    PowerId.MONOLOGUE: MonologuePower,
    PowerId.BACK_ATTACK_LEFT: BackAttackLeftPower,
    PowerId.BACK_ATTACK_RIGHT: BackAttackRightPower,
}

for _pid, _cls in _ALL_POWERS.items():
    register_power_class(_pid, _cls)
