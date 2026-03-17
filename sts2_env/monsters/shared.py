"""Shared / cross-act / event / test monsters.

Includes:
- Architect (pet, no moves)
- Byrdpip (pet, no moves)
- PaelsLegion (pet, no moves)
- FakeMerchantMonster (event combat)
- MysteriousKnight (event combat, Lantern Key)
- DenseVegetationWriggler (event combat, Dense Vegetation)
- TorchHeadAmalgam (boss minion)
- TheAdversaryMkOne/Two/Three (boss encounter)
- BattleFriendV1/V2/V3 (battleworn dummies)
- BigDummy, OneHpMonster, TenHpMonster (test dummies)
- SingleAttackMoveMonster, MultiAttackMoveMonster (test monsters)

All HP ranges, damage values, and state machines verified against decompiled C# source.
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from sts2_env.core.creature import Creature
from sts2_env.core.enums import CombatSide, MoveRepeatType, PowerId, ValueProp
from sts2_env.core.damage import calculate_damage, apply_damage
from sts2_env.core.rng import Rng
from sts2_env.monsters.intents import (
    Intent, IntentType, attack_intent, multi_attack_intent,
    buff_intent, debuff_intent, strong_debuff_intent, status_intent,
    defend_intent, sleep_intent,
)
from sts2_env.monsters.state_machine import (
    ConditionalBranchState, MonsterAI, MonsterState, MoveState, RandomBranchState,
)

if TYPE_CHECKING:
    from sts2_env.core.combat import CombatState


# ---- Helpers ----

def _deal_damage_to_player(combat: CombatState, creature: Creature, base_dmg: int, hits: int = 1) -> None:
    for _ in range(hits):
        if combat.primary_player.is_dead:
            break
        dmg = calculate_damage(base_dmg, creature, combat.primary_player, ValueProp.MOVE, combat)
        apply_damage(combat.primary_player, dmg, ValueProp.MOVE, combat, creature)


def _gain_block(creature: Creature, amount: int) -> None:
    creature.gain_block(amount)


# ========================================================================
# PET / SPECIAL MONSTERS (no real moves)
# ========================================================================

# ---- Architect (HP 9999) ----
# No-op monster; exists as a structural element (e.g. for encounters).

def create_architect(rng: Rng) -> tuple[Creature, MonsterAI]:
    creature = Creature(max_hp=9999, monster_id="ARCHITECT")

    def nothing(combat: CombatState) -> None:
        pass

    states: dict[str, MonsterState] = {
        "NOTHING": MoveState("NOTHING", nothing, [Intent(IntentType.UNKNOWN)], follow_up_id="NOTHING"),
    }
    return creature, MonsterAI(states, "NOTHING")


# ---- Byrdpip (HP 9999, no health bar) ----
# Pet companion; does nothing in combat.

def create_byrdpip(rng: Rng) -> tuple[Creature, MonsterAI]:
    creature = Creature(max_hp=9999, monster_id="BYRDPIP")

    def nothing(combat: CombatState) -> None:
        pass

    states: dict[str, MonsterState] = {
        "NOTHING_MOVE": MoveState("NOTHING_MOVE", nothing, [Intent(IntentType.UNKNOWN)], follow_up_id="NOTHING_MOVE"),
    }
    return creature, MonsterAI(states, "NOTHING_MOVE")


# ---- PaelsLegion (HP 9999, no health bar) ----
# Pet companion; does nothing in combat.

def create_paels_legion(rng: Rng) -> tuple[Creature, MonsterAI]:
    creature = Creature(max_hp=9999, monster_id="PAELS_LEGION")

    def nothing(combat: CombatState) -> None:
        pass

    states: dict[str, MonsterState] = {
        "NOTHING_MOVE": MoveState("NOTHING_MOVE", nothing, [Intent(IntentType.UNKNOWN)], follow_up_id="NOTHING_MOVE"),
    }
    return creature, MonsterAI(states, "NOTHING_MOVE")


# ========================================================================
# EVENT COMBAT MONSTERS
# ========================================================================

# ---- FakeMerchantMonster (HP 165) ----
# Moves: SWIPE(13), SPEW_COINS(2x8), THROW_RELIC(13+Frail), ENRAGE(Str+2)
# Initial move: SWIPE
# After SWIPE/SPEW_COINS/ENRAGE -> RAND(all 4, CannotRepeat, Enrage cooldown 3)
# After THROW_RELIC -> RAND_ATTACK(SWIPE/SPEW_COINS/THROW_RELIC only, CannotRepeat)

def create_fake_merchant_monster(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 165
    creature = Creature(max_hp=hp, monster_id="FAKE_MERCHANT_MONSTER")
    swipe_dmg = 13
    spew_coins_dmg = 2
    spew_coins_hits = 8
    throw_relic_dmg = 13  # C# uses SwipeDamage for the actual DamageCmd
    enrage_str = 2

    def swipe(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, swipe_dmg)

    def spew_coins(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, spew_coins_dmg, hits=spew_coins_hits)

    def throw_relic(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, throw_relic_dmg)
        combat.apply_power_to(combat.primary_player, PowerId.FRAIL, 1)

    def enrage(combat: CombatState) -> None:
        creature.apply_power(PowerId.STRENGTH, enrage_str)

    # Main random: all 4 moves, enrage has cooldown 3
    rand = RandomBranchState("RAND")
    rand.add_branch("SWIPE", MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch("SPEW_COINS", MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch("THROW_RELIC", MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch("ENRAGE", MoveRepeatType.CANNOT_REPEAT, cooldown=3)

    # Attack-only random (after throw_relic): 3 attack moves, CannotRepeat
    rand_attack = RandomBranchState("RAND_ATTACK")
    rand_attack.add_branch("SWIPE", MoveRepeatType.CANNOT_REPEAT)
    rand_attack.add_branch("SPEW_COINS", MoveRepeatType.CANNOT_REPEAT)
    rand_attack.add_branch("THROW_RELIC", MoveRepeatType.CANNOT_REPEAT)

    states: dict[str, MonsterState] = {
        "RAND": rand,
        "RAND_ATTACK": rand_attack,
        "SWIPE": MoveState("SWIPE", swipe, [attack_intent(swipe_dmg)], follow_up_id="RAND"),
        "SPEW_COINS": MoveState("SPEW_COINS", spew_coins, [multi_attack_intent(spew_coins_dmg, spew_coins_hits)], follow_up_id="RAND"),
        "THROW_RELIC": MoveState("THROW_RELIC", throw_relic, [attack_intent(throw_relic_dmg), debuff_intent()], follow_up_id="RAND_ATTACK"),
        "ENRAGE": MoveState("ENRAGE", enrage, [buff_intent()], follow_up_id="RAND"),
    }
    return creature, MonsterAI(states, "SWIPE")


# ---- MysteriousKnight (event combat, The Lantern Key) ----
# FlailKnight with +6 Strength and +6 Plating applied after added to room.
# HP 101 (same as FlailKnight base), moves identical to FlailKnight.
# C# class MysteriousKnight : FlailKnight

def create_mysterious_knight(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 101
    creature = Creature(max_hp=hp, monster_id="MYSTERIOUS_KNIGHT")
    flail_dmg = 9
    ram_dmg = 15

    def war_chant(combat: CombatState) -> None:
        creature.apply_power(PowerId.STRENGTH, 3)

    def flail(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, flail_dmg, hits=2)

    def ram(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, ram_dmg)

    rand = RandomBranchState("RAND")
    rand.add_branch("WAR_CHANT", MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch("FLAIL", MoveRepeatType.CAN_REPEAT_FOREVER, weight=2.0)
    rand.add_branch("RAM", MoveRepeatType.CAN_REPEAT_FOREVER, weight=2.0)

    states: dict[str, MonsterState] = {
        "RAND": rand,
        "WAR_CHANT": MoveState("WAR_CHANT", war_chant, [buff_intent()], follow_up_id="RAND"),
        "FLAIL": MoveState("FLAIL", flail, [multi_attack_intent(flail_dmg, 2)], follow_up_id="RAND"),
        "RAM": MoveState("RAM", ram, [attack_intent(ram_dmg)], follow_up_id="RAND"),
    }

    # AfterAddedToRoom: +6 Strength, +6 Plating
    creature.apply_power(PowerId.STRENGTH, 6)
    creature.apply_power(PowerId.PLATING, 6)
    return creature, MonsterAI(states, "RAM")


# ---- DenseVegetationWriggler (event combat, Dense Vegetation) ----
# Full Wriggler for the Dense Vegetation event (not the minion spawned by LouseProgenitor).
# HP 17-21, BiteDamage=6, Wriggle=+2STR + status card.
# Slot-based initial move: wriggler1/wriggler3 start with BITE, wriggler2/wriggler4 with WRIGGLE.
# StartStunned=false in the event.

def create_dense_vegetation_wriggler(
    rng: Rng, slot: str = "wriggler1",
) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(17, 21)
    creature = Creature(max_hp=hp, monster_id="WRIGGLER")
    bite_dmg = 6
    wriggle_str = 2

    def nasty_bite(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, bite_dmg)

    def wriggle(combat: CombatState) -> None:
        creature.apply_power(PowerId.STRENGTH, wriggle_str)

    states: dict[str, MonsterState] = {
        "NASTY_BITE": MoveState(
            "NASTY_BITE", nasty_bite, [attack_intent(bite_dmg)], follow_up_id="WRIGGLE",
        ),
        "WRIGGLE": MoveState(
            "WRIGGLE", wriggle, [buff_intent(), status_intent()], follow_up_id="NASTY_BITE",
        ),
    }

    # Slot determines starting move: wriggler1/3 -> bite, wriggler2/4 -> wriggle
    if slot in ("wriggler2", "wriggler4"):
        initial_state = "WRIGGLE"
    else:
        initial_state = "NASTY_BITE"

    return creature, MonsterAI(states, initial_state)


# ========================================================================
# BOSS MINIONS
# ========================================================================

# ---- TorchHeadAmalgam (HP 199, minion) ----
# Cycle: TACKLE_1(18) -> TACKLE_2(18) -> BEAM(8x3) -> TACKLE_3(14) -> TACKLE_4(14) -> BEAM -> ...
# AfterAddedToRoom: Minion power

def create_torch_head_amalgam(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 199
    creature = Creature(max_hp=hp, monster_id="TORCH_HEAD_AMALGAM")
    tackle_dmg = 18
    weak_tackle_dmg = 14
    soul_beam_dmg = 8
    soul_beam_hits = 3

    def tackle(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, tackle_dmg)

    def weak_tackle(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, weak_tackle_dmg)

    def soul_beam(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, soul_beam_dmg, hits=soul_beam_hits)

    states: dict[str, MonsterState] = {
        "TACKLE_1": MoveState("TACKLE_1", tackle, [attack_intent(tackle_dmg)], follow_up_id="TACKLE_2"),
        "TACKLE_2": MoveState("TACKLE_2", tackle, [attack_intent(tackle_dmg)], follow_up_id="BEAM"),
        "BEAM": MoveState("BEAM", soul_beam, [multi_attack_intent(soul_beam_dmg, soul_beam_hits)], follow_up_id="TACKLE_3"),
        "TACKLE_3": MoveState("TACKLE_3", weak_tackle, [attack_intent(weak_tackle_dmg)], follow_up_id="TACKLE_4"),
        "TACKLE_4": MoveState("TACKLE_4", weak_tackle, [attack_intent(weak_tackle_dmg)], follow_up_id="BEAM"),
    }

    creature.apply_power(PowerId.MINION, 1)
    return creature, MonsterAI(states, "TACKLE_1")


# ========================================================================
# THE ADVERSARY (3-phase boss encounter)
# ========================================================================

# ---- TheAdversaryMkOne (HP 100) ----
# Cycle: SMASH(12) -> BEAM(15) -> BARRAGE(8x2, Str+2) -> SMASH -> ...
# AfterAddedToRoom: Artifact(0) -- effectively no artifact stacks

def create_the_adversary_mk_one(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 100
    creature = Creature(max_hp=hp, monster_id="THE_ADVERSARY_MK_ONE")
    smash_dmg = 12
    beam_dmg = 15
    barrage_dmg = 8
    barrage_hits = 2

    def smash(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, smash_dmg)

    def beam(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, beam_dmg)

    def barrage(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, barrage_dmg, hits=barrage_hits)
        creature.apply_power(PowerId.STRENGTH, 2)

    states: dict[str, MonsterState] = {
        "SMASH": MoveState("SMASH", smash, [attack_intent(smash_dmg)], follow_up_id="BEAM"),
        "BEAM": MoveState("BEAM", beam, [attack_intent(beam_dmg)], follow_up_id="BARRAGE"),
        "BARRAGE": MoveState("BARRAGE", barrage, [multi_attack_intent(barrage_dmg, barrage_hits), buff_intent()], follow_up_id="SMASH"),
    }

    # C# applies Artifact with amount 0 (no stacks, but power is present)
    return creature, MonsterAI(states, "SMASH")


# ---- TheAdversaryMkTwo (HP 200) ----
# Cycle: BASH(13) -> FLAME_BEAM(16) -> BARRAGE(9x2, Str+3) -> BASH -> ...
# AfterAddedToRoom: Artifact(1)

def create_the_adversary_mk_two(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 200
    creature = Creature(max_hp=hp, monster_id="THE_ADVERSARY_MK_TWO")
    bash_dmg = 13
    flame_beam_dmg = 16
    barrage_dmg = 9
    barrage_hits = 2

    def bash(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, bash_dmg)

    def flame_beam(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, flame_beam_dmg)

    def barrage(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, barrage_dmg, hits=barrage_hits)
        creature.apply_power(PowerId.STRENGTH, 3)

    states: dict[str, MonsterState] = {
        "BASH": MoveState("BASH", bash, [attack_intent(bash_dmg)], follow_up_id="FLAME_BEAM"),
        "FLAME_BEAM": MoveState("FLAME_BEAM", flame_beam, [attack_intent(flame_beam_dmg)], follow_up_id="BARRAGE"),
        "BARRAGE": MoveState("BARRAGE", barrage, [multi_attack_intent(barrage_dmg, barrage_hits), buff_intent()], follow_up_id="BASH"),
    }

    creature.apply_power(PowerId.ARTIFACT, 1)
    return creature, MonsterAI(states, "BASH")


# ---- TheAdversaryMkThree (HP 300) ----
# Cycle: CRASH(15) -> FLAME_BEAM(18) -> BARRAGE(10x2, Str+4) -> CRASH -> ...
# AfterAddedToRoom: Artifact(2)

def create_the_adversary_mk_three(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 300
    creature = Creature(max_hp=hp, monster_id="THE_ADVERSARY_MK_THREE")
    crash_dmg = 15
    flame_beam_dmg = 18
    barrage_dmg = 10
    barrage_hits = 2

    def crash(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, crash_dmg)

    def flame_beam(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, flame_beam_dmg)

    def barrage(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, barrage_dmg, hits=barrage_hits)
        creature.apply_power(PowerId.STRENGTH, 4)

    states: dict[str, MonsterState] = {
        "CRASH": MoveState("CRASH", crash, [attack_intent(crash_dmg)], follow_up_id="FLAME_BEAM"),
        "FLAME_BEAM": MoveState("FLAME_BEAM", flame_beam, [attack_intent(flame_beam_dmg)], follow_up_id="BARRAGE"),
        "BARRAGE": MoveState("BARRAGE", barrage, [multi_attack_intent(barrage_dmg, barrage_hits), buff_intent()], follow_up_id="CRASH"),
    }

    creature.apply_power(PowerId.ARTIFACT, 2)
    return creature, MonsterAI(states, "CRASH")


# ========================================================================
# BATTLEWORN DUMMIES (event combat training)
# ========================================================================

# ---- BattleFriendV1 (HP 75) ----
# No moves; has BattlewornDummyTimeLimit(3) power.

def create_battle_friend_v1(rng: Rng) -> tuple[Creature, MonsterAI]:
    creature = Creature(max_hp=75, monster_id="BATTLE_FRIEND_V1")

    def nothing(combat: CombatState) -> None:
        pass

    states: dict[str, MonsterState] = {
        "NOTHING_MOVE": MoveState("NOTHING_MOVE", nothing, [Intent(IntentType.UNKNOWN)], follow_up_id="NOTHING_MOVE"),
    }

    creature.apply_power(PowerId.BATTLEWORN_DUMMY_TIME_LIMIT, 3)
    return creature, MonsterAI(states, "NOTHING_MOVE")


# ---- BattleFriendV2 (HP 150) ----
# No moves; has BattlewornDummyTimeLimit(3) power.

def create_battle_friend_v2(rng: Rng) -> tuple[Creature, MonsterAI]:
    creature = Creature(max_hp=150, monster_id="BATTLE_FRIEND_V2")

    def nothing(combat: CombatState) -> None:
        pass

    states: dict[str, MonsterState] = {
        "NOTHING_MOVE": MoveState("NOTHING_MOVE", nothing, [Intent(IntentType.UNKNOWN)], follow_up_id="NOTHING_MOVE"),
    }

    creature.apply_power(PowerId.BATTLEWORN_DUMMY_TIME_LIMIT, 3)
    return creature, MonsterAI(states, "NOTHING_MOVE")


# ---- BattleFriendV3 (HP 300) ----
# No moves; has BattlewornDummyTimeLimit(3) power.

def create_battle_friend_v3(rng: Rng) -> tuple[Creature, MonsterAI]:
    creature = Creature(max_hp=300, monster_id="BATTLE_FRIEND_V3")

    def nothing(combat: CombatState) -> None:
        pass

    states: dict[str, MonsterState] = {
        "NOTHING_MOVE": MoveState("NOTHING_MOVE", nothing, [Intent(IntentType.UNKNOWN)], follow_up_id="NOTHING_MOVE"),
    }

    creature.apply_power(PowerId.BATTLEWORN_DUMMY_TIME_LIMIT, 3)
    return creature, MonsterAI(states, "NOTHING_MOVE")


# ========================================================================
# TEST MONSTERS
# ========================================================================

# ---- BigDummy (HP 9999) ----
# No-op test target.

def create_big_dummy(rng: Rng) -> tuple[Creature, MonsterAI]:
    creature = Creature(max_hp=9999, monster_id="BIG_DUMMY")

    def nothing(combat: CombatState) -> None:
        pass

    states: dict[str, MonsterState] = {
        "NOTHING": MoveState("NOTHING", nothing, [Intent(IntentType.UNKNOWN)], follow_up_id="NOTHING"),
    }
    return creature, MonsterAI(states, "NOTHING")


# ---- OneHpMonster (HP 1) ----
# Minimal test target.

def create_one_hp_monster(rng: Rng) -> tuple[Creature, MonsterAI]:
    creature = Creature(max_hp=1, monster_id="ONE_HP_MONSTER")

    def nothing(combat: CombatState) -> None:
        pass

    states: dict[str, MonsterState] = {
        "NOTHING": MoveState("NOTHING", nothing, [Intent(IntentType.UNKNOWN)], follow_up_id="NOTHING"),
    }
    return creature, MonsterAI(states, "NOTHING")


# ---- TenHpMonster (HP 10) ----
# Small test target.

def create_ten_hp_monster(rng: Rng) -> tuple[Creature, MonsterAI]:
    creature = Creature(max_hp=10, monster_id="TEN_HP_MONSTER")

    def nothing(combat: CombatState) -> None:
        pass

    states: dict[str, MonsterState] = {
        "NOTHING": MoveState("NOTHING", nothing, [Intent(IntentType.UNKNOWN)], follow_up_id="NOTHING"),
    }
    return creature, MonsterAI(states, "NOTHING")


# ---- SingleAttackMoveMonster (HP 999) ----
# Repeats POKE (1 damage single attack) every turn.

def create_single_attack_move_monster(rng: Rng) -> tuple[Creature, MonsterAI]:
    creature = Creature(max_hp=999, monster_id="SINGLE_ATTACK_MOVE_MONSTER")
    poke_dmg = 1

    def poke(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, poke_dmg)

    states: dict[str, MonsterState] = {
        "POKE": MoveState("POKE", poke, [attack_intent(poke_dmg)], follow_up_id="POKE"),
    }
    return creature, MonsterAI(states, "POKE")


# ---- MultiAttackMoveMonster (HP 999) ----
# Repeats POKE (1 damage x 5 hits) every turn.

def create_multi_attack_move_monster(rng: Rng) -> tuple[Creature, MonsterAI]:
    creature = Creature(max_hp=999, monster_id="MULTI_ATTACK_MOVE_MONSTER")
    poke_dmg = 1
    poke_hits = 5

    def poke(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, poke_dmg, hits=poke_hits)

    states: dict[str, MonsterState] = {
        "POKE": MoveState("POKE", poke, [multi_attack_intent(poke_dmg, poke_hits)], follow_up_id="POKE"),
    }
    return creature, MonsterAI(states, "POKE")
