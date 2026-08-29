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
from sts2_env.relics.base import RelicId
from sts2_env.monsters.intents import (
    Intent, IntentType, attack_intent, multi_attack_intent,
    buff_intent, debuff_intent, strong_debuff_intent, status_intent,
    defend_intent, sleep_intent,
)
from sts2_env.monsters.state_machine import (
    ConditionalBranchState, MonsterAI, MonsterState, MoveState, RandomBranchState,
)
from sts2_env.monsters.targets import (
    add_generated_cards_to_living_player_discards,
    apply_power_to_living_player_targets,
    living_player_targets,
)
from sts2_env.cards.status import make_infection

if TYPE_CHECKING:
    from sts2_env.core.combat import CombatState


NOOP_MONSTER_HP = 9999
ATTACK_TEST_MONSTER_HP = 999


# ---- Helpers ----

TOUGH_ENEMIES_ASCENSION_LEVEL = 8
DEADLY_ENEMIES_ASCENSION_LEVEL = 9


def _ascension_value(ascension_level: int, threshold: int, ascension_value: int, base_value: int) -> int:
    return ascension_value if ascension_level >= threshold else base_value


def _combat_ascension_level(combat: CombatState) -> int:
    return getattr(combat, "ascension_level", 0)


def _deal_damage_to_player(combat: CombatState, creature: Creature, base_dmg: int, hits: int = 1) -> None:
    for _ in range(hits):
        targets = living_player_targets(combat)
        if not targets:
            break
        for target in targets:
            dmg = calculate_damage(base_dmg, creature, target, ValueProp.MOVE, combat)
            apply_damage(target, dmg, ValueProp.MOVE, combat, creature)
        combat._check_combat_end()  # noqa: SLF001
        if combat.is_over:
            break


def _gain_block(creature: Creature, amount: int) -> None:
    creature.gain_block(amount)


# ========================================================================
# PET / SPECIAL MONSTERS (no real moves)
# ========================================================================

# ---- Architect (HP 9999) ----
# No-op monster; exists as a structural element (e.g. for encounters).

def create_architect(rng: Rng) -> tuple[Creature, MonsterAI]:
    creature = Creature(max_hp=NOOP_MONSTER_HP, monster_id="ARCHITECT")

    def nothing(combat: CombatState) -> None:
        pass

    states: dict[str, MonsterState] = {
        "NOTHING": MoveState("NOTHING", nothing, [Intent(IntentType.UNKNOWN)], follow_up_id="NOTHING"),
    }
    return creature, MonsterAI(states, "NOTHING")


# ---- Byrdpip (HP 9999, no health bar) ----
# Pet companion; does nothing in combat.

def create_byrdpip(rng: Rng) -> tuple[Creature, MonsterAI]:
    creature = Creature(max_hp=NOOP_MONSTER_HP, monster_id=RelicId.BYRDPIP.name)

    def nothing(combat: CombatState) -> None:
        pass

    states: dict[str, MonsterState] = {
        "NOTHING_MOVE": MoveState("NOTHING_MOVE", nothing, [Intent(IntentType.UNKNOWN)], follow_up_id="NOTHING_MOVE"),
    }
    return creature, MonsterAI(states, "NOTHING_MOVE")


# ---- PaelsLegion (HP 9999, no health bar) ----
# Pet companion; does nothing in combat.

def create_paels_legion(rng: Rng) -> tuple[Creature, MonsterAI]:
    creature = Creature(max_hp=NOOP_MONSTER_HP, monster_id=RelicId.PAELS_LEGION.name)

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
    throw_relic_frail = 1
    enrage_str = 2

    def swipe(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, swipe_dmg)

    def spew_coins(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, spew_coins_dmg, hits=spew_coins_hits)

    def throw_relic(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, throw_relic_dmg)
        apply_power_to_living_player_targets(combat, PowerId.FRAIL, throw_relic_frail, applier=creature)

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

MYSTERIOUS_KNIGHT_MONSTER_ID = "MYSTERIOUS_KNIGHT"
MYSTERIOUS_KNIGHT_BASE_HP = 101
MYSTERIOUS_KNIGHT_TOUGH_HP = 108
MYSTERIOUS_KNIGHT_BASE_FLAIL_DAMAGE = 9
MYSTERIOUS_KNIGHT_DEADLY_FLAIL_DAMAGE = 10
MYSTERIOUS_KNIGHT_FLAIL_REPEAT = 2
MYSTERIOUS_KNIGHT_BASE_RAM_DAMAGE = 15
MYSTERIOUS_KNIGHT_DEADLY_RAM_DAMAGE = 17
MYSTERIOUS_KNIGHT_WAR_CHANT_STRENGTH = 3
MYSTERIOUS_KNIGHT_STRENGTH = 6
MYSTERIOUS_KNIGHT_PLATING = 6
MYSTERIOUS_KNIGHT_RANDOM_STATE = "RAND"
MYSTERIOUS_KNIGHT_WAR_CHANT_MOVE = "WAR_CHANT"
MYSTERIOUS_KNIGHT_FLAIL_MOVE = "FLAIL_MOVE"
MYSTERIOUS_KNIGHT_RAM_MOVE = "RAM_MOVE"


def create_mysterious_knight(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        MYSTERIOUS_KNIGHT_TOUGH_HP,
        MYSTERIOUS_KNIGHT_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=MYSTERIOUS_KNIGHT_MONSTER_ID)

    def war_chant(combat: CombatState) -> None:
        creature.apply_power(PowerId.STRENGTH, MYSTERIOUS_KNIGHT_WAR_CHANT_STRENGTH)

    def flail(combat: CombatState) -> None:
        flail_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            MYSTERIOUS_KNIGHT_DEADLY_FLAIL_DAMAGE,
            MYSTERIOUS_KNIGHT_BASE_FLAIL_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, flail_dmg, hits=MYSTERIOUS_KNIGHT_FLAIL_REPEAT)

    def ram(combat: CombatState) -> None:
        ram_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            MYSTERIOUS_KNIGHT_DEADLY_RAM_DAMAGE,
            MYSTERIOUS_KNIGHT_BASE_RAM_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, ram_dmg)

    flail_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        MYSTERIOUS_KNIGHT_DEADLY_FLAIL_DAMAGE,
        MYSTERIOUS_KNIGHT_BASE_FLAIL_DAMAGE,
    )
    ram_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        MYSTERIOUS_KNIGHT_DEADLY_RAM_DAMAGE,
        MYSTERIOUS_KNIGHT_BASE_RAM_DAMAGE,
    )

    rand = RandomBranchState(MYSTERIOUS_KNIGHT_RANDOM_STATE)
    rand.add_branch(MYSTERIOUS_KNIGHT_WAR_CHANT_MOVE, MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch(MYSTERIOUS_KNIGHT_FLAIL_MOVE, MoveRepeatType.CAN_REPEAT_FOREVER, weight=2.0)
    rand.add_branch(MYSTERIOUS_KNIGHT_RAM_MOVE, MoveRepeatType.CAN_REPEAT_FOREVER, weight=2.0)

    states: dict[str, MonsterState] = {
        MYSTERIOUS_KNIGHT_RANDOM_STATE: rand,
        MYSTERIOUS_KNIGHT_WAR_CHANT_MOVE: MoveState(
            MYSTERIOUS_KNIGHT_WAR_CHANT_MOVE,
            war_chant,
            [buff_intent()],
            follow_up_id=MYSTERIOUS_KNIGHT_RANDOM_STATE,
        ),
        MYSTERIOUS_KNIGHT_FLAIL_MOVE: MoveState(
            MYSTERIOUS_KNIGHT_FLAIL_MOVE,
            flail,
            [multi_attack_intent(flail_intent_damage, MYSTERIOUS_KNIGHT_FLAIL_REPEAT)],
            follow_up_id=MYSTERIOUS_KNIGHT_RANDOM_STATE,
        ),
        MYSTERIOUS_KNIGHT_RAM_MOVE: MoveState(
            MYSTERIOUS_KNIGHT_RAM_MOVE,
            ram,
            [attack_intent(ram_intent_damage)],
            follow_up_id=MYSTERIOUS_KNIGHT_RANDOM_STATE,
        ),
    }

    # AfterAddedToRoom: +6 Strength, +6 Plating
    creature.apply_power(PowerId.STRENGTH, MYSTERIOUS_KNIGHT_STRENGTH)
    creature.apply_power(PowerId.PLATING, MYSTERIOUS_KNIGHT_PLATING)
    return creature, MonsterAI(states, MYSTERIOUS_KNIGHT_RAM_MOVE)


# ---- DenseVegetationWriggler (event combat, Dense Vegetation) ----
# Full Wriggler for the Dense Vegetation event.
# HP 17-21, BiteDamage=6, Wriggle=+2STR + Infection.
# Slot-based initial move: wriggler1/wriggler3 start with BITE, wriggler2/wriggler4 with WRIGGLE.
# StartStunned=false in the event.

def create_dense_vegetation_wriggler(
    rng: Rng, slot: str = "wriggler1",
) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(17, 21)
    creature = Creature(max_hp=hp, monster_id="WRIGGLER")
    bite_dmg = 6
    wriggle_str = 2
    wriggle_infections = 1

    def nasty_bite(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, bite_dmg)

    def wriggle(combat: CombatState) -> None:
        add_generated_cards_to_living_player_discards(combat, make_infection, wriggle_infections)
        creature.apply_power(PowerId.STRENGTH, wriggle_str)

    init = ConditionalBranchState("INIT_MOVE")
    init.add_branch(lambda: slot in ("wriggler1", "wriggler3"), "NASTY_BITE_MOVE")
    init.add_branch(lambda: slot in ("wriggler2", "wriggler4"), "WRIGGLE_MOVE")
    init.add_branch(lambda: True, "NASTY_BITE_MOVE")

    states: dict[str, MonsterState] = {
        "INIT_MOVE": init,
        "NASTY_BITE_MOVE": MoveState(
            "NASTY_BITE_MOVE",
            nasty_bite,
            [attack_intent(bite_dmg)],
            follow_up_id="WRIGGLE_MOVE",
        ),
        "WRIGGLE_MOVE": MoveState(
            "WRIGGLE_MOVE",
            wriggle,
            [buff_intent(), status_intent()],
            follow_up_id="NASTY_BITE_MOVE",
        ),
    }

    return creature, MonsterAI(states, "INIT_MOVE")


# ========================================================================
# BOSS MINIONS
# ========================================================================

# ---- TorchHeadAmalgam ----

TORCH_HEAD_AMALGAM_MONSTER_ID = "TORCH_HEAD_AMALGAM"
TORCH_HEAD_AMALGAM_BASE_HP = 199
TORCH_HEAD_AMALGAM_TOUGH_HP = 211
TORCH_HEAD_AMALGAM_BASE_STRONG_TACKLE_DAMAGE = 26
TORCH_HEAD_AMALGAM_DEADLY_STRONG_TACKLE_DAMAGE = 32
TORCH_HEAD_AMALGAM_BASE_TACKLE_DAMAGE = 18
TORCH_HEAD_AMALGAM_DEADLY_TACKLE_DAMAGE = 22
TORCH_HEAD_AMALGAM_BASE_WEAK_TACKLE_DAMAGE = 14
TORCH_HEAD_AMALGAM_DEADLY_WEAK_TACKLE_DAMAGE = 16
TORCH_HEAD_AMALGAM_SOUL_BEAM_DAMAGE = 8
TORCH_HEAD_AMALGAM_SOUL_BEAM_REPEAT = 3
TORCH_HEAD_AMALGAM_MINION = 1
TORCH_HEAD_AMALGAM_STRONG_TACKLE_MOVE = "STRONG_TACKLE_MOVE"
TORCH_HEAD_AMALGAM_TACKLE_2_MOVE = "TACKLE_2_MOVE"
TORCH_HEAD_AMALGAM_BEAM_MOVE = "BEAM_MOVE"
TORCH_HEAD_AMALGAM_TACKLE_3_MOVE = "TACKLE_3_MOVE"
TORCH_HEAD_AMALGAM_TACKLE_4_MOVE = "TACKLE_4_MOVE"


def create_torch_head_amalgam(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        TORCH_HEAD_AMALGAM_TOUGH_HP,
        TORCH_HEAD_AMALGAM_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=TORCH_HEAD_AMALGAM_MONSTER_ID)

    def strong_tackle(combat: CombatState) -> None:
        strong_tackle_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            TORCH_HEAD_AMALGAM_DEADLY_STRONG_TACKLE_DAMAGE,
            TORCH_HEAD_AMALGAM_BASE_STRONG_TACKLE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, strong_tackle_dmg)

    def tackle(combat: CombatState) -> None:
        tackle_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            TORCH_HEAD_AMALGAM_DEADLY_TACKLE_DAMAGE,
            TORCH_HEAD_AMALGAM_BASE_TACKLE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, tackle_dmg)

    def weak_tackle(combat: CombatState) -> None:
        weak_tackle_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            TORCH_HEAD_AMALGAM_DEADLY_WEAK_TACKLE_DAMAGE,
            TORCH_HEAD_AMALGAM_BASE_WEAK_TACKLE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, weak_tackle_dmg)

    def soul_beam(combat: CombatState) -> None:
        _deal_damage_to_player(
            combat,
            creature,
            TORCH_HEAD_AMALGAM_SOUL_BEAM_DAMAGE,
            hits=TORCH_HEAD_AMALGAM_SOUL_BEAM_REPEAT,
        )

    strong_tackle_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        TORCH_HEAD_AMALGAM_DEADLY_STRONG_TACKLE_DAMAGE,
        TORCH_HEAD_AMALGAM_BASE_STRONG_TACKLE_DAMAGE,
    )
    tackle_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        TORCH_HEAD_AMALGAM_DEADLY_TACKLE_DAMAGE,
        TORCH_HEAD_AMALGAM_BASE_TACKLE_DAMAGE,
    )
    weak_tackle_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        TORCH_HEAD_AMALGAM_DEADLY_WEAK_TACKLE_DAMAGE,
        TORCH_HEAD_AMALGAM_BASE_WEAK_TACKLE_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        TORCH_HEAD_AMALGAM_STRONG_TACKLE_MOVE: MoveState(
            TORCH_HEAD_AMALGAM_STRONG_TACKLE_MOVE,
            strong_tackle,
            [attack_intent(strong_tackle_intent_damage)],
            follow_up_id=TORCH_HEAD_AMALGAM_TACKLE_2_MOVE,
        ),
        TORCH_HEAD_AMALGAM_TACKLE_2_MOVE: MoveState(
            TORCH_HEAD_AMALGAM_TACKLE_2_MOVE,
            tackle,
            [attack_intent(tackle_intent_damage)],
            follow_up_id=TORCH_HEAD_AMALGAM_BEAM_MOVE,
        ),
        TORCH_HEAD_AMALGAM_BEAM_MOVE: MoveState(
            TORCH_HEAD_AMALGAM_BEAM_MOVE,
            soul_beam,
            [multi_attack_intent(TORCH_HEAD_AMALGAM_SOUL_BEAM_DAMAGE, TORCH_HEAD_AMALGAM_SOUL_BEAM_REPEAT)],
            follow_up_id=TORCH_HEAD_AMALGAM_TACKLE_3_MOVE,
        ),
        TORCH_HEAD_AMALGAM_TACKLE_3_MOVE: MoveState(
            TORCH_HEAD_AMALGAM_TACKLE_3_MOVE,
            weak_tackle,
            [attack_intent(weak_tackle_intent_damage)],
            follow_up_id=TORCH_HEAD_AMALGAM_TACKLE_4_MOVE,
        ),
        TORCH_HEAD_AMALGAM_TACKLE_4_MOVE: MoveState(
            TORCH_HEAD_AMALGAM_TACKLE_4_MOVE,
            weak_tackle,
            [attack_intent(weak_tackle_intent_damage)],
            follow_up_id=TORCH_HEAD_AMALGAM_BEAM_MOVE,
        ),
    }

    creature.apply_power(PowerId.MINION, TORCH_HEAD_AMALGAM_MINION)
    return creature, MonsterAI(states, TORCH_HEAD_AMALGAM_STRONG_TACKLE_MOVE)


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
        combat.apply_power_to(creature, PowerId.STRENGTH, 2, applier=creature)

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
        combat.apply_power_to(creature, PowerId.STRENGTH, 3, applier=creature)

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
        combat.apply_power_to(creature, PowerId.STRENGTH, 4, applier=creature)

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
    creature = Creature(max_hp=NOOP_MONSTER_HP, monster_id="BIG_DUMMY")

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
    creature = Creature(max_hp=ATTACK_TEST_MONSTER_HP, monster_id="SINGLE_ATTACK_MOVE_MONSTER")
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
    creature = Creature(max_hp=ATTACK_TEST_MONSTER_HP, monster_id="MULTI_ATTACK_MOVE_MONSTER")
    poke_dmg = 1
    poke_hits = 5

    def poke(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, poke_dmg, hits=poke_hits)

    states: dict[str, MonsterState] = {
        "POKE": MoveState("POKE", poke, [multi_attack_intent(poke_dmg, poke_hits)], follow_up_id="POKE"),
    }
    return creature, MonsterAI(states, "POKE")
