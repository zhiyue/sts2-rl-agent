"""Monster-specific powers.

Covers: MinionPower, SlipperyPower, RavenousPower, TerritorialPower,
HardenedShellPower, SkittishPower, ThieveryPower, SurprisePower, PlowPower,
SmoggyPower, InfestedPower, IllusionPower, AsleepPower, SteamEruptionPower,
ShriekPower, SuckPower, CrabRagePower, SpinnerPower, FeedingFrenzyPower,
HatchPower, BurrowedPower, SurroundedPower, CoveredPower,
PersonalHivePower, CoordinatePower, SlothPower, MonologuePower.

All logic verified against decompiled C# source.
"""

from __future__ import annotations

import math
from typing import TYPE_CHECKING

from sts2_env.core.enums import (
    CardType,
    CombatSide,
    OrbType,
    PowerId,
    PowerType,
    PowerStackType,
    ValueProp,
)
from sts2_env.powers.base import PowerInstance

if TYPE_CHECKING:
    from sts2_env.core.creature import Creature
    from sts2_env.core.combat import CombatState


_TERROR_EEL_ID = "TERROR_EEL"
_TERROR_EEL_TERROR_MOVE_ID = "TERROR_MOVE"
_TUNNELER_BITE_MOVE_ID = "BITE_MOVE"
_LAGAVULIN_MATRIARCH_ID = "LAGAVULIN_MATRIARCH"
_LAGAVULIN_MATRIARCH_SLASH_MOVE_ID = "SLASH_MOVE"


def _gain_unpowered_block(owner: Creature, amount: int, combat: CombatState) -> int:
    before = owner.block
    owner.gain_block(amount, unpowered=True)
    gained = owner.block - before
    if gained > 0:
        from sts2_env.core.hooks import fire_after_block_gained

        fire_after_block_gained(owner, gained, combat)
    return gained


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

    def should_power_be_removed_after_owner_death(
        self,
        owner: Creature,
        combat: CombatState,
    ) -> bool:
        return False

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
    should_scale_in_multiplayer = True

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
        result = getattr(combat, "_active_damage_result", None)
        total_damage = getattr(result, "total_damage", damage)
        if target is owner and total_damage != 0:
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
        self,
        owner: Creature,
        dead_creature: Creature,
        combat: CombatState,
        was_removal_prevented: bool = False,
    ) -> None:
        """Called by the combat system when an allied creature dies."""
        if (
            not was_removal_prevented
            and dead_creature is not owner
            and dead_creature.side == owner.side
        ):
            if owner.is_alive:
                owner.apply_power(PowerId.STRENGTH, self.amount)
                combat.stun_enemy(owner)


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
    should_scale_in_multiplayer = True

    def __init__(self, amount: int):
        super().__init__(PowerId.HARDENED_SHELL, amount)
        self._damage_taken_this_turn: int = 0

    def modify_hp_lost_before_osty_late(
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

    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER
    should_scale_in_multiplayer = True

    def __init__(self, amount: int):
        super().__init__(PowerId.SKITTISH, amount)
        self._triggered_this_turn: bool = False

    def after_attack(self, owner: Creature, attack: object, combat: CombatState) -> None:
        if self._triggered_this_turn:
            return
        if not bool(getattr(attack, "damage_props", ValueProp.NONE) & ValueProp.MOVE):
            return
        if getattr(getattr(attack, "model_source", None), "card_id", None) is None:
            return
        for result in getattr(attack, "results", ()):
            if getattr(result, "target", None) is owner and getattr(result, "unblocked_damage", 0) != 0:
                self._triggered_this_turn = True
                _gain_unpowered_block(owner, self.amount, combat)
                return

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

    In the simulator, this is resolved from the finished attack context so
    every distinct player hit by the owner's move loses gold once.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.THIEVERY, amount)
        self.gold_stolen: int = 0
        self.gold_stolen_by_player: dict[Creature, int] = {}
        self._targets: list[Creature] = []

    def add_target(self, target_player: Creature) -> None:
        if target_player not in self._targets:
            self._targets.append(target_player)

    def _ensure_targets(self, combat: CombatState) -> None:
        if self._targets:
            return
        for state in getattr(combat, "combat_player_states", ()):
            self.add_target(state.creature)

    def _player_gold(self, target_player: object) -> int:
        if hasattr(target_player, "gold"):
            return int(getattr(target_player, "gold", 0))
        combat = getattr(target_player, "combat_state", None)
        state_for = getattr(combat, "combat_player_state_for", None) if combat is not None else None
        if callable(state_for):
            state = state_for(target_player)
            if state is not None:
                return int(state.player_state.gold)
        return 0

    def steal(self, owner: Creature, target_player: object) -> int:
        """Steal gold from the target player. Returns amount stolen."""
        player_gold = self._player_gold(target_player)
        if player_gold <= 0:
            return 0
        stolen = min(self.amount, player_gold)
        lose_gold = getattr(target_player, "lose_gold", None)
        if callable(lose_gold):
            stolen = lose_gold(stolen)
        elif hasattr(target_player, "gold"):
            target_player.gold -= stolen
        self.gold_stolen += stolen
        if getattr(target_player, "is_player", False):
            self.gold_stolen_by_player[target_player] = self.gold_stolen_by_player.get(target_player, 0) + stolen
        return stolen

    def after_attack(self, owner: Creature, attack: object, combat: CombatState) -> None:
        if getattr(attack, "attacker", None) is not owner:
            return
        self._ensure_targets(combat)
        if self._targets:
            for target in list(self._targets):
                self.steal(owner, target)
            return
        seen_targets: set[int] = set()
        for result in getattr(attack, "results", ()):
            target = getattr(result, "target", None)
            if target is None or not getattr(target, "is_player", False):
                continue
            target_key = id(target)
            if target_key in seen_targets:
                continue
            seen_targets.add(target_key)
            self.steal(owner, target)


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

    def after_death(
        self,
        owner: Creature,
        creature: Creature,
        combat: CombatState,
        was_removal_prevented: bool = False,
    ) -> None:
        if was_removal_prevented or creature is not owner:
            return
        combat.spawn_surprise_replacements(owner)


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
    should_scale_in_multiplayer = True

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
            owner.powers.pop(self.power_id, None)
            ai = combat.enemy_ais.get(owner.combat_id)
            next_state_id = "BEAST_CRY_MOVE" if ai is not None and "BEAST_CRY_MOVE" in ai.states else None
            combat.stun_enemy(owner, next_state_id)


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

    def _afflict_skill(self, owner: Creature, card: object) -> None:
        card_owner = getattr(card, "owner", None)
        if card_owner is None:
            card.owner = owner
            card_owner = owner
        if card_owner is owner and getattr(card, "card_type", None) == CardType.SKILL:
            afflict = getattr(card, "afflict", None)
            if callable(afflict) and not afflict("smog"):
                return
            card.combat_vars["_smoggy"] = True

    def _afflict_all_skills(self, owner: Creature, combat: CombatState) -> None:
        state = combat.combat_player_state_for(owner)
        if state is None:
            return
        for pile in state.all_piles:
            for card in pile:
                self._afflict_skill(owner, card)

    def after_card_played(
        self, owner: Creature, card: object, combat: CombatState
    ) -> None:
        if getattr(card, "owner", None) is owner and getattr(card, "card_type", None) == CardType.SKILL:
            self.skills_locked = True
            self._afflict_all_skills(owner, combat)

    def after_card_entered_combat(self, owner: Creature, card: object, combat: CombatState) -> None:
        if getattr(card, "owner", None) is not owner or getattr(card, "card_type", None) != CardType.SKILL:
            return
        skill_started_this_turn = combat.count_card_play_starts_this_turn(owner, card_type=CardType.SKILL) > 0
        if self.skills_locked or skill_started_this_turn:
            self._afflict_skill(owner, card)

    def should_card_be_playable(self, owner: Creature, card: object) -> bool:
        """Return False to block skill plays after first skill this turn."""
        if getattr(card, "owner", None) is not owner:
            return True
        has_affliction = getattr(card, "has_affliction", None)
        if callable(has_affliction):
            if has_affliction("smog"):
                return False
            if self.skills_locked and getattr(card, "card_type", None) == CardType.SKILL:
                return getattr(card, "affliction", None) is not None
            return True
        return not bool(getattr(card, "combat_vars", {}).get("_smoggy"))

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            self.skills_locked = False
            state = combat.combat_player_state_for(owner)
            if state is not None:
                for pile in state.all_piles:
                    for card in pile:
                        clear_affliction = getattr(card, "clear_affliction", None)
                        if callable(clear_affliction):
                            clear_affliction("smog")
                        card.combat_vars.pop("_smoggy", None)


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

    def after_death(
        self,
        owner: Creature,
        creature: Creature,
        combat: CombatState,
        was_removal_prevented: bool = False,
    ) -> None:
        if not was_removal_prevented and creature is owner:
            combat.spawn_infested_wrigglers(4)


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
        self.follow_up_state_id: str | None = None

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
        if owner is target and power_id == self.power_id and amount > 0 and not owner.has_power(PowerId.MINION):
            owner.apply_power(PowerId.MINION, 1, applier=applier, source=source)

    def after_death(
        self,
        owner: Creature,
        creature: Creature,
        combat: CombatState,
        was_removal_prevented: bool = False,
    ) -> None:
        if was_removal_prevented or creature is not owner:
            return
        from sts2_env.core.enums import IntentType
        from sts2_env.monsters.intents import Intent
        from sts2_env.monsters.state_machine import MoveState

        self.is_reviving = True
        debuff_ids = [
            pid
            for pid, p in owner.powers.items()
            if p.power_type == PowerType.DEBUFF
        ]
        for pid in debuff_ids:
            del owner.powers[pid]
        ai = combat.enemy_ais.get(owner.combat_id)
        if ai is None:
            return
        follow_up_id = self.follow_up_state_id or (ai.state_log[-1] if ai.state_log else ai.current_move.state_id)

        def _revive_move(_: CombatState) -> None:
            self.revive(owner)

        ai.states["REVIVE_MOVE"] = MoveState(
            "REVIVE_MOVE",
            _revive_move,
            [Intent(IntentType.HEAL)],
            follow_up_id=follow_up_id,
            must_perform_once=True,
        )
        ai._current_state_id = "REVIVE_MOVE"  # noqa: SLF001

    def revive(self, owner: Creature) -> None:
        """Called by the monster AI to complete the revive."""
        self.is_reviving = False
        owner.current_hp = owner.max_hp
        owner.escaped = False
        owner._death_processed = False

    def should_stop_combat_ending(
        self,
        owner: Creature | None = None,
        combat: CombatState | None = None,
    ) -> bool:
        return self.is_reviving

    def should_allow_hitting(self, owner: Creature, combat: CombatState) -> bool:
        return not self.is_reviving

    def should_creature_be_removed_from_combat_after_death(
        self,
        owner: Creature,
        combat: CombatState,
    ) -> bool:
        return False

    def should_power_be_removed_after_owner_death(
        self,
        owner: Creature,
        combat: CombatState,
    ) -> bool:
        return False

    def should_other_power_be_removed_on_owner_death(
        self,
        owner: Creature,
        power: PowerInstance,
        combat: CombatState,
    ) -> bool | None:
        return power.power_type == PowerType.DEBUFF


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

    LagavulinMatriarch damage wake inserts the original stun before SLASH_MOVE;
    natural wake transitions directly to SLASH_MOVE.
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
            owner.powers.pop(PowerId.PLATING, None)
            self.is_awake = True
            if owner.monster_id == _LAGAVULIN_MATRIARCH_ID:
                combat.stun_enemy(owner, _LAGAVULIN_MATRIARCH_SLASH_MOVE_ID)
            else:
                combat.stun_enemy(owner)
            owner.powers.pop(self.power_id, None)

    def after_turn_end(
        self, owner: Creature, side: CombatSide, combat: CombatState
    ) -> None:
        if side == owner.side:
            self.amount -= 1
            if self.amount <= 0:
                owner.powers.pop(PowerId.PLATING, None)
                self.is_awake = True
                owner.powers.pop(self.power_id, None)
                if owner.monster_id == _LAGAVULIN_MATRIARCH_ID:
                    combat.set_enemy_state(owner, _LAGAVULIN_MATRIARCH_SLASH_MOVE_ID)
                else:
                    combat.stun_enemy(owner)

    def before_turn_end_very_early(
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

    The simulator transitions the owner into the ABOUT_TO_BLOW_MOVE state and
    keeps it in combat until EXPLODE resolves.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.STEAM_ERUPTION, amount)

    def after_death(
        self,
        owner: Creature,
        creature: Creature,
        combat: CombatState,
        was_removal_prevented: bool = False,
    ) -> None:
        from sts2_env.monsters.act4 import (
            WATERFALL_GIANT_ABOUT_TO_BLOW_HP,
            WATERFALL_GIANT_ABOUT_TO_BLOW_MOVE,
            WATERFALL_GIANT_MONSTER_ID,
        )

        if was_removal_prevented or creature is not owner or owner.monster_id != WATERFALL_GIANT_MONSTER_ID:
            return
        owner.max_hp = WATERFALL_GIANT_ABOUT_TO_BLOW_HP
        owner.current_hp = WATERFALL_GIANT_ABOUT_TO_BLOW_HP
        owner.escaped = False
        owner._death_processed = False
        combat.set_enemy_state(owner, WATERFALL_GIANT_ABOUT_TO_BLOW_MOVE)

    def should_stop_combat_ending(self) -> bool:
        return True

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
    should_scale_in_multiplayer = True

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
            if owner.monster_id == _TERROR_EEL_ID:
                combat.stun_enemy(owner, _TERROR_EEL_TERROR_MOVE_ID)
            else:
                combat.stun_enemy(owner)
            owner.powers.pop(self.power_id, None)


# ---------------------------------------------------------------------------
# SuckPower
# ---------------------------------------------------------------------------
class SuckPower(PowerInstance):
    """After a powered attack resolves, gain Amount Strength per qualifying
    damage result.

    C# ref: SuckPower.cs
    - AfterAttack: count unblocked results, but if a pet was hit remove the
      corresponding pet-owner result from the count.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.SUCK, amount)

    def after_attack(self, owner: Creature, attack: object, combat: CombatState) -> None:
        if getattr(attack, "attacker", None) is not owner:
            return
        if not getattr(attack, "damage_props", ValueProp.NONE).is_powered_attack():
            return
        results = [
            result
            for result in getattr(attack, "results", ())
            if getattr(getattr(result, "target", None), "side", None) != owner.side
        ]
        if not results:
            return
        for pet_hit in [result for result in results if getattr(getattr(result, "target", None), "is_pet", False)]:
            pet_owner = getattr(getattr(pet_hit, "target", None), "pet_owner", None)
            if pet_owner is not None:
                results = [result for result in results if getattr(result, "target", None) is not pet_owner]
        results = [result for result in results if getattr(result, "unblocked_damage", 0) > 0]
        if results:
            owner.apply_power(PowerId.STRENGTH, self.amount * len(results))


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
        self,
        owner: Creature,
        dead_creature: Creature,
        combat: CombatState,
        was_removal_prevented: bool = False,
    ) -> None:
        """Called when an allied creature dies."""
        if dead_creature is not owner and dead_creature.side == owner.side:
            owner.apply_power(PowerId.STRENGTH, self.STRENGTH_GAIN)
            _gain_unpowered_block(owner, self.BLOCK_GAIN, combat)
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

    def after_energy_reset(self, owner: Creature, combat: CombatState) -> None:
        if hasattr(combat, "channel_orb"):
            for _ in range(self.amount):
                combat.channel_orb(owner, OrbType.GLASS)

    def after_side_turn_start(
        self, owner: Creature, side: CombatSide, combat: CombatState
    ) -> None:
        if side == owner.side and not combat.has_energy_reset_this_turn(owner):
            self.after_energy_reset(owner, combat)


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
    is_temporary = True

    def __init__(self, amount: int):
        super().__init__(PowerId.FEEDING_FRENZY, amount)

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
        combat.stun_enemy(owner, _TUNNELER_BITE_MOVE_ID)
        combat._remove_power(owner, self.power_id)

    def on_removed(self, owner: Creature, combat: CombatState) -> None:
        owner.block = 0


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

    Facing direction is tracked and updated from the exact target chosen by
    cards and potions. When only one attacking side remains, the player turns
    toward it automatically.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.SINGLE

    # Direction constants
    FACING_RIGHT = 0
    FACING_LEFT = 1

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.SURROUNDED, amount)
        self.facing: int = self.FACING_RIGHT

    def _update_facing_toward_target(self, target: Creature | None) -> None:
        if target is None:
            return
        if target.has_power(PowerId.BACK_ATTACK_LEFT):
            self.facing = self.FACING_LEFT
        elif target.has_power(PowerId.BACK_ATTACK_RIGHT):
            self.facing = self.FACING_RIGHT

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
        if getattr(card, "owner", None) is not owner:
            return
        self._update_facing_toward_target(getattr(combat, "active_card_target", None) or getattr(card, "target", None))

    def before_potion_used(
        self,
        owner: Creature,
        potion: object,
        target: Creature | None,
        combat: CombatState,
    ) -> None:
        self._update_facing_toward_target(target)

    def on_ally_death(
        self,
        owner: Creature,
        dead_creature: Creature,
        combat: CombatState,
        was_removal_prevented: bool = False,
    ) -> None:
        if was_removal_prevented or dead_creature.side == owner.side:
            return
        hittable_enemies = combat.hittable_enemies
        if not hittable_enemies:
            return
        if all(enemy.has_power(PowerId.BACK_ATTACK_LEFT) for enemy in hittable_enemies) or all(
            enemy.has_power(PowerId.BACK_ATTACK_RIGHT) for enemy in hittable_enemies
        ):
            self._update_facing_toward_target(hittable_enemies[0])


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
        self._covering_creatures: list[Creature] = []

    def _attach_cover(self, owner: Creature, coverer: Creature, source: object | None) -> None:
        if coverer in self._covering_creatures:
            return
        self._covering_creatures.append(coverer)
        self.covering_creature = coverer
        if not coverer.has_power(PowerId.INTERCEPT):
            coverer.apply_power(PowerId.INTERCEPT, 1, applier=coverer, source=source)
        intercept = coverer.powers.get(PowerId.INTERCEPT)
        add_covered_creature = getattr(intercept, "add_covered_creature", None)
        if callable(add_covered_creature):
            add_covered_creature(owner)

    def _detach_cover(self, owner: Creature, coverer: Creature) -> None:
        intercept = coverer.powers.get(PowerId.INTERCEPT)
        remove_covered_creature = getattr(intercept, "remove_covered_creature", None)
        if callable(remove_covered_creature):
            remove_covered_creature(owner)

    def _detach_all_covers(self, owner: Creature) -> None:
        for coverer in list(self._covering_creatures):
            self._detach_cover(owner, coverer)
        self._covering_creatures = []
        self.covering_creature = None

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
        if owner is not target or power_id != self.power_id or amount <= 0 or applier is None:
            return
        self._attach_cover(owner, applier, source)

    def modify_damage_multiplicative(
        self,
        owner: Creature,
        dealer: Creature | None,
        target: Creature,
        props: ValueProp,
    ) -> float:
        if target is owner and props.is_powered_attack():
            return 0.0
        return 1.0

    def after_turn_end(
        self, owner: Creature, side: CombatSide, combat: CombatState
    ) -> None:
        if side == CombatSide.ENEMY:
            self._detach_all_covers(owner)
            owner.powers.pop(self.power_id, None)

    def on_ally_death(
        self,
        owner: Creature,
        dead_creature: Creature,
        combat: CombatState,
        was_removal_prevented: bool = False,
    ) -> None:
        if was_removal_prevented or dead_creature not in self._covering_creatures:
            return
        self._detach_cover(owner, dead_creature)
        self._covering_creatures.remove(dead_creature)
        self.covering_creature = self._covering_creatures[-1] if self._covering_creatures else None
        if not self._covering_creatures:
            owner.powers.pop(self.power_id, None)

    def before_death(self, owner: Creature, creature: Creature, combat: CombatState) -> None:
        if creature is owner:
            self._detach_all_covers(owner)


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
        if target is owner and dealer is not None and props.is_powered_attack():
            from sts2_env.cards.status import make_dazed

            dazed_owner = dealer
            if getattr(dealer, "is_osty", False) and getattr(dealer, "pet_owner", None) is not None:
                dazed_owner = dealer.pet_owner
            for _ in range(self.amount):
                combat.add_generated_card_to_creature_draw_pile(
                    dazed_owner,
                    make_dazed(),
                    added_by_player=False,
                    random_position=True,
                )


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
    is_temporary = True

    def __init__(self, amount: int):
        super().__init__(PowerId.COORDINATE, amount)

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

    def should_play(self, owner: Creature, card: object, combat: CombatState) -> bool:
        if (getattr(card, "owner", None) or owner) is not owner:
            return True
        return self._cards_played_this_turn < self.amount

    def before_card_played(
        self, owner: Creature, card: object, combat: CombatState
    ) -> None:
        if getattr(card, "owner", None) is not owner:
            return
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
        self._instances: list[tuple[int, int]] = [(self.STRENGTH_PER_CARD, 0)]
        self.strength_per_card: int = self.STRENGTH_PER_CARD
        self._instances_for_played_cards: dict[int, list[tuple[int, int]]] = {}

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
            self._instances.append((self.strength_per_card, 0))

    def before_card_played(
        self, owner: Creature, card: object, combat: CombatState
    ) -> None:
        if getattr(card, "owner", None) is owner:
            self._instances_for_played_cards[id(card)] = [
                (index, strength_per_card)
                for index, (strength_per_card, _) in enumerate(self._instances)
            ]

    def after_card_played(
        self, owner: Creature, card: object, combat: CombatState
    ) -> None:
        instances = self._instances_for_played_cards.pop(id(card), [])
        amount = sum(strength_per_card for _, strength_per_card in instances)
        if amount <= 0:
            return
        owner.apply_power(PowerId.STRENGTH, amount)
        updated_instances = list(self._instances)
        for index, strength_per_card in instances:
            if index >= len(updated_instances):
                continue
            current_strength_per_card, strength_applied = updated_instances[index]
            updated_instances[index] = (current_strength_per_card, strength_applied + strength_per_card)
        self._instances = updated_instances
        self.amount = sum(strength_applied for _, strength_applied in self._instances)

    def after_turn_end(
        self, owner: Creature, side: CombatSide, combat: CombatState
    ) -> None:
        if side == owner.side:
            # Undo all Strength gained
            owner.apply_power(PowerId.STRENGTH, -sum(strength_applied for _, strength_applied in self._instances))
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
# WitheringPresencePower (Aeonglass boss)
# ---------------------------------------------------------------------------
WITHERING_PRESENCE_CARDS_LEFT = 6


class WitheringPresencePower(PowerInstance):
    """Aeonglass applies this to each player at combat start (amount 6).

    C# ref: WitheringPresencePower.cs (instanced, Target = player creature)
    - AfterCardPlayed: if the played card belongs to the target player,
      decrement CardsLeft; on reaching 0, add 1 Wither to that player's hand
      and reset the counter to 6.

    New Withers are fake-upgraded to the boss's current WitherUpgradeCount.
    ``target_player`` must be wired up by the encounter setup (the player
    creature does not exist at monster-creation time).
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(
        self,
        target_player: Creature | None = None,
        wither_upgrade_level: int = 0,
        cards_left: int = WITHERING_PRESENCE_CARDS_LEFT,
    ):
        super().__init__(PowerId.WITHERING_PRESENCE, cards_left)
        self.target_player = target_player
        self.wither_upgrade_level = wither_upgrade_level
        self.additional_strength = 0

    def after_card_played(
        self, owner: Creature, card: object, combat: CombatState
    ) -> None:
        if self.target_player is None or getattr(card, "owner", None) is not self.target_player:
            return
        self.amount -= 1
        if self.amount <= 0:
            from sts2_env.cards.status import make_wither

            wither = make_wither(self.wither_upgrade_level)
            combat._add_generated_cards_to_hand([wither], owner=self.target_player)
            self.amount = WITHERING_PRESENCE_CARDS_LEFT


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
    PowerId.PERSONAL_HIVE: PersonalHivePower,
    PowerId.COORDINATE: CoordinatePower,
    PowerId.SLOTH: SlothPower,
    PowerId.MONOLOGUE: MonologuePower,
    PowerId.BACK_ATTACK_LEFT: BackAttackLeftPower,
    PowerId.BACK_ATTACK_RIGHT: BackAttackRightPower,
    PowerId.WITHERING_PRESENCE: WitheringPresencePower,
}

for _pid, _cls in _ALL_POWERS.items():
    register_power_class(_pid, _cls)
