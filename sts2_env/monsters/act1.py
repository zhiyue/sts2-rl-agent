"""Act 1 (Overgrowth) monsters: weak, normal, elite, boss.

All HP ranges, damage values, and state machines verified against decompiled C# source.
Weak monsters are re-exported from act1_weak.py for convenience.
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
from sts2_env.monsters.state_machine import MonsterAI, MonsterState, MoveState, RandomBranchState
from sts2_env.monsters.block import gain_move_block
from sts2_env.monsters.targets import (
    add_generated_cards_to_living_player_discards,
    apply_power_to_living_player_targets,
    living_player_targets,
)
from sts2_env.cards.status import make_dazed, make_infection, make_wound

if TYPE_CHECKING:
    from sts2_env.core.combat import CombatState

# Re-export weak monsters
from sts2_env.monsters.act1_weak import (  # noqa: F401
    create_shrinker_beetle,
    create_fuzzy_wurm_crawler,
    create_nibbit,
    create_leaf_slime_s,
    create_twig_slime_s,
    create_leaf_slime_m,
    create_twig_slime_m,
)


# ---- Helpers ----

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


def _gain_block(creature: Creature, amount: int, combat: CombatState) -> None:
    gain_move_block(creature, amount, combat)


# ========================================================================
# NORMAL ENCOUNTERS
# ========================================================================

# ---- CubexConstruct (HP 65 / 70 asc) ----
# CHARGE_UP_MOVE -> REPEATER_MOVE -> REPEATER_MOVE_2 -> EXPEL_BLAST -> REPEATER_MOVE (loop)

TOUGH_ENEMIES_ASCENSION_LEVEL = 8
DEADLY_ENEMIES_ASCENSION_LEVEL = 9


def _ascension_value(ascension_level: int, threshold: int, ascension_value: int, base_value: int) -> int:
    return ascension_value if ascension_level >= threshold else base_value


def _combat_ascension_level(combat: CombatState) -> int:
    return getattr(combat, "ascension_level", 0)


CUBEX_CONSTRUCT_BASE_HP = 65
CUBEX_CONSTRUCT_TOUGH_HP = 70
CUBEX_CONSTRUCT_BASE_BLAST_DAMAGE = 7
CUBEX_CONSTRUCT_DEADLY_BLAST_DAMAGE = 8
CUBEX_CONSTRUCT_BASE_EXPEL_DAMAGE = 5
CUBEX_CONSTRUCT_DEADLY_EXPEL_DAMAGE = 6
CUBEX_CONSTRUCT_EXPEL_HITS = 2
CUBEX_CONSTRUCT_STRENGTH_GAIN = 2
CUBEX_CONSTRUCT_INITIAL_BLOCK = 13
CUBEX_CONSTRUCT_ARTIFACT_AMOUNT = 1
CUBEX_CONSTRUCT_CHARGE_UP_MOVE = "CHARGE_UP_MOVE"
CUBEX_CONSTRUCT_REPEATER_MOVE = "REPEATER_MOVE"
CUBEX_CONSTRUCT_REPEATER_MOVE_2 = "REPEATER_MOVE_2"
CUBEX_CONSTRUCT_EXPEL_BLAST_MOVE = "EXPEL_BLAST"


def create_cubex_construct(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        CUBEX_CONSTRUCT_TOUGH_HP,
        CUBEX_CONSTRUCT_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id="CUBEX_CONSTRUCT")

    def charge_up(combat: CombatState) -> None:
        combat.apply_power_to(creature, PowerId.STRENGTH, CUBEX_CONSTRUCT_STRENGTH_GAIN, applier=creature)

    def repeater(combat: CombatState) -> None:
        blast_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            CUBEX_CONSTRUCT_DEADLY_BLAST_DAMAGE,
            CUBEX_CONSTRUCT_BASE_BLAST_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, blast_dmg)
        combat.apply_power_to(creature, PowerId.STRENGTH, CUBEX_CONSTRUCT_STRENGTH_GAIN, applier=creature)

    def expel_blast(combat: CombatState) -> None:
        expel_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            CUBEX_CONSTRUCT_DEADLY_EXPEL_DAMAGE,
            CUBEX_CONSTRUCT_BASE_EXPEL_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, expel_dmg, hits=CUBEX_CONSTRUCT_EXPEL_HITS)

    blast_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        CUBEX_CONSTRUCT_DEADLY_BLAST_DAMAGE,
        CUBEX_CONSTRUCT_BASE_BLAST_DAMAGE,
    )
    expel_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        CUBEX_CONSTRUCT_DEADLY_EXPEL_DAMAGE,
        CUBEX_CONSTRUCT_BASE_EXPEL_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        CUBEX_CONSTRUCT_CHARGE_UP_MOVE: MoveState(
            CUBEX_CONSTRUCT_CHARGE_UP_MOVE,
            charge_up,
            [buff_intent()],
            follow_up_id=CUBEX_CONSTRUCT_REPEATER_MOVE,
        ),
        CUBEX_CONSTRUCT_REPEATER_MOVE: MoveState(
            CUBEX_CONSTRUCT_REPEATER_MOVE,
            repeater,
            [attack_intent(blast_intent_damage), buff_intent()],
            follow_up_id=CUBEX_CONSTRUCT_REPEATER_MOVE_2,
        ),
        CUBEX_CONSTRUCT_REPEATER_MOVE_2: MoveState(
            CUBEX_CONSTRUCT_REPEATER_MOVE_2,
            repeater,
            [attack_intent(blast_intent_damage), buff_intent()],
            follow_up_id=CUBEX_CONSTRUCT_EXPEL_BLAST_MOVE,
        ),
        CUBEX_CONSTRUCT_EXPEL_BLAST_MOVE: MoveState(
            CUBEX_CONSTRUCT_EXPEL_BLAST_MOVE,
            expel_blast,
            [multi_attack_intent(expel_intent_damage, CUBEX_CONSTRUCT_EXPEL_HITS)],
            follow_up_id=CUBEX_CONSTRUCT_REPEATER_MOVE,
        ),
    }

    return creature, MonsterAI(states, CUBEX_CONSTRUCT_CHARGE_UP_MOVE)


def apply_cubex_construct_room_setup(creature: Creature, combat: CombatState) -> None:
    _gain_block(creature, CUBEX_CONSTRUCT_INITIAL_BLOCK, combat)
    creature.apply_power(PowerId.ARTIFACT, CUBEX_CONSTRUCT_ARTIFACT_AMOUNT)


# ---- Flyconid (HP 47-49 / 51-53 asc) ----

FLYCONID_BASE_MIN_HP = 47
FLYCONID_BASE_MAX_HP = 49
FLYCONID_TOUGH_MIN_HP = 51
FLYCONID_TOUGH_MAX_HP = 53
FLYCONID_BASE_SMASH_DAMAGE = 11
FLYCONID_DEADLY_SMASH_DAMAGE = 12
FLYCONID_BASE_SPORE_DAMAGE = 8
FLYCONID_DEADLY_SPORE_DAMAGE = 9
FLYCONID_VULNERABLE_SPORES_VULNERABLE = 2
FLYCONID_FRAIL_SPORES_FRAIL = 2
FLYCONID_INITIAL_MOVE = "INITIAL"
FLYCONID_RANDOM_MOVE = "RAND"
FLYCONID_VULNERABLE_SPORES_MOVE = "VULNERABLE_SPORES_MOVE"
FLYCONID_FRAIL_SPORES_MOVE = "FRAIL_SPORES_MOVE"
FLYCONID_SMASH_MOVE = "SMASH_MOVE"
FLYCONID_VULNERABLE_SPORES_WEIGHT = 3.0
FLYCONID_FRAIL_SPORES_WEIGHT = 2.0
FLYCONID_SMASH_WEIGHT = 1.0


def create_flyconid(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        FLYCONID_TOUGH_MIN_HP,
        FLYCONID_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        FLYCONID_TOUGH_MAX_HP,
        FLYCONID_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id="FLYCONID")

    def vulnerable_spores(combat: CombatState) -> None:
        apply_power_to_living_player_targets(
            combat,
            PowerId.VULNERABLE,
            FLYCONID_VULNERABLE_SPORES_VULNERABLE,
            applier=creature,
        )

    def frail_spores(combat: CombatState) -> None:
        spore_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            FLYCONID_DEADLY_SPORE_DAMAGE,
            FLYCONID_BASE_SPORE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, spore_dmg)
        apply_power_to_living_player_targets(combat, PowerId.FRAIL, FLYCONID_FRAIL_SPORES_FRAIL, applier=creature)

    def smash(combat: CombatState) -> None:
        smash_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            FLYCONID_DEADLY_SMASH_DAMAGE,
            FLYCONID_BASE_SMASH_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, smash_dmg)

    spore_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        FLYCONID_DEADLY_SPORE_DAMAGE,
        FLYCONID_BASE_SPORE_DAMAGE,
    )
    smash_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        FLYCONID_DEADLY_SMASH_DAMAGE,
        FLYCONID_BASE_SMASH_DAMAGE,
    )

    # Initial random: FrailSpores(2) or Smash(1)
    initial_rand = RandomBranchState(FLYCONID_INITIAL_MOVE)
    initial_rand.add_branch(FLYCONID_FRAIL_SPORES_MOVE, weight=FLYCONID_FRAIL_SPORES_WEIGHT)
    initial_rand.add_branch(FLYCONID_SMASH_MOVE, weight=FLYCONID_SMASH_WEIGHT)

    # Main random: all 3, cannot repeat
    main_rand = RandomBranchState(FLYCONID_RANDOM_MOVE)
    main_rand.add_branch(
        FLYCONID_VULNERABLE_SPORES_MOVE,
        MoveRepeatType.CANNOT_REPEAT,
        weight=FLYCONID_VULNERABLE_SPORES_WEIGHT,
    )
    main_rand.add_branch(
        FLYCONID_FRAIL_SPORES_MOVE,
        MoveRepeatType.CANNOT_REPEAT,
        weight=FLYCONID_FRAIL_SPORES_WEIGHT,
    )
    main_rand.add_branch(FLYCONID_SMASH_MOVE, MoveRepeatType.CANNOT_REPEAT, weight=FLYCONID_SMASH_WEIGHT)

    states: dict[str, MonsterState] = {
        FLYCONID_INITIAL_MOVE: initial_rand,
        FLYCONID_RANDOM_MOVE: main_rand,
        FLYCONID_VULNERABLE_SPORES_MOVE: MoveState(
            FLYCONID_VULNERABLE_SPORES_MOVE,
            vulnerable_spores,
            [debuff_intent()],
            follow_up_id=FLYCONID_RANDOM_MOVE,
        ),
        FLYCONID_FRAIL_SPORES_MOVE: MoveState(
            FLYCONID_FRAIL_SPORES_MOVE,
            frail_spores,
            [attack_intent(spore_intent_damage), debuff_intent()],
            follow_up_id=FLYCONID_RANDOM_MOVE,
        ),
        FLYCONID_SMASH_MOVE: MoveState(
            FLYCONID_SMASH_MOVE,
            smash,
            [attack_intent(smash_intent_damage)],
            follow_up_id=FLYCONID_RANDOM_MOVE,
        ),
    }
    return creature, MonsterAI(states, FLYCONID_INITIAL_MOVE, rng)


# ---- Fogmog (HP 74 / 78 asc) ----

def create_eye_with_teeth(rng: Rng) -> tuple[Creature, MonsterAI]:
    creature = Creature(max_hp=6, monster_id="EYE_WITH_TEETH")
    distract_dazed = 3

    def distract(combat: CombatState) -> None:
        add_generated_cards_to_living_player_discards(combat, make_dazed, distract_dazed)

    states: dict[str, MonsterState] = {
        "DISTRACT_MOVE": MoveState("DISTRACT_MOVE", distract, [status_intent()], follow_up_id="DISTRACT_MOVE"),
    }
    creature.apply_power(PowerId.ILLUSION, 1)
    creature.apply_power(PowerId.MINION, 1)
    return creature, MonsterAI(states, "DISTRACT_MOVE")


FOGMOG_BASE_HP = 74
FOGMOG_TOUGH_HP = 78
FOGMOG_BASE_SWIPE_DAMAGE = 8
FOGMOG_DEADLY_SWIPE_DAMAGE = 9
FOGMOG_BASE_HEADBUTT_DAMAGE = 14
FOGMOG_DEADLY_HEADBUTT_DAMAGE = 16
FOGMOG_SWIPE_STRENGTH_GAIN = 1
FOGMOG_BRANCH_MOVE = "BRANCH"
FOGMOG_ILLUSION_MOVE = "ILLUSION_MOVE"
FOGMOG_SWIPE_MOVE = "SWIPE_MOVE"
FOGMOG_SWIPE_RANDOM_MOVE = "SWIPE_RANDOM_MOVE"
FOGMOG_HEADBUTT_MOVE = "HEADBUTT_MOVE"
FOGMOG_SWIPE_RANDOM_WEIGHT = 0.4
FOGMOG_HEADBUTT_WEIGHT = 0.6


def create_fogmog(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        FOGMOG_TOUGH_HP,
        FOGMOG_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id="FOGMOG")

    def illusion(combat: CombatState) -> None:
        eye, eye_ai = create_eye_with_teeth(rng)
        combat.add_enemy(eye, eye_ai)

    def swipe(combat: CombatState) -> None:
        swipe_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            FOGMOG_DEADLY_SWIPE_DAMAGE,
            FOGMOG_BASE_SWIPE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, swipe_dmg)
        combat.apply_power_to(creature, PowerId.STRENGTH, FOGMOG_SWIPE_STRENGTH_GAIN, applier=creature)

    def headbutt(combat: CombatState) -> None:
        headbutt_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            FOGMOG_DEADLY_HEADBUTT_DAMAGE,
            FOGMOG_BASE_HEADBUTT_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, headbutt_dmg)

    swipe_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        FOGMOG_DEADLY_SWIPE_DAMAGE,
        FOGMOG_BASE_SWIPE_DAMAGE,
    )
    headbutt_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        FOGMOG_DEADLY_HEADBUTT_DAMAGE,
        FOGMOG_BASE_HEADBUTT_DAMAGE,
    )

    rand = RandomBranchState(FOGMOG_BRANCH_MOVE)
    rand.add_branch(FOGMOG_SWIPE_RANDOM_MOVE, MoveRepeatType.CANNOT_REPEAT, weight=FOGMOG_SWIPE_RANDOM_WEIGHT)
    rand.add_branch(FOGMOG_HEADBUTT_MOVE, MoveRepeatType.CANNOT_REPEAT, weight=FOGMOG_HEADBUTT_WEIGHT)

    states: dict[str, MonsterState] = {
        FOGMOG_ILLUSION_MOVE: MoveState(
            FOGMOG_ILLUSION_MOVE,
            illusion,
            [Intent(IntentType.SUMMON)],
            follow_up_id=FOGMOG_SWIPE_MOVE,
        ),
        FOGMOG_SWIPE_MOVE: MoveState(
            FOGMOG_SWIPE_MOVE,
            swipe,
            [attack_intent(swipe_intent_damage), buff_intent()],
            follow_up_id=FOGMOG_BRANCH_MOVE,
        ),
        FOGMOG_BRANCH_MOVE: rand,
        FOGMOG_SWIPE_RANDOM_MOVE: MoveState(
            FOGMOG_SWIPE_RANDOM_MOVE,
            swipe,
            [attack_intent(swipe_intent_damage), buff_intent()],
            follow_up_id=FOGMOG_HEADBUTT_MOVE,
        ),
        FOGMOG_HEADBUTT_MOVE: MoveState(
            FOGMOG_HEADBUTT_MOVE,
            headbutt,
            [attack_intent(headbutt_intent_damage)],
            follow_up_id=FOGMOG_SWIPE_MOVE,
        ),
    }
    return creature, MonsterAI(states, FOGMOG_ILLUSION_MOVE)


# ---- Inklet (HP 11-17 / 12-18 asc) ----

INKLET_BASE_MIN_HP = 11
INKLET_BASE_MAX_HP = 17
INKLET_TOUGH_MIN_HP = 12
INKLET_TOUGH_MAX_HP = 18
INKLET_BASE_JAB_DAMAGE = 3
INKLET_DEADLY_JAB_DAMAGE = 4
INKLET_BASE_WHIRLWIND_DAMAGE = 2
INKLET_DEADLY_WHIRLWIND_DAMAGE = 3
INKLET_WHIRLWIND_HITS = 3
INKLET_BASE_PIERCING_GAZE_DAMAGE = 10
INKLET_DEADLY_PIERCING_GAZE_DAMAGE = 11
INKLET_SLIPPERY_AMOUNT = 1
INKLET_JAB_MOVE = "JAB_MOVE"
INKLET_WHIRLWIND_MOVE = "WHIRLWIND_MOVE"
INKLET_PIERCING_GAZE_MOVE = "PIERCING_GAZE_MOVE"
INKLET_RANDOM_MOVE = "RAND"


def create_inklet(
    rng: Rng,
    *,
    middle_inklet: bool = False,
    ascension_level: int = 0,
) -> tuple[Creature, MonsterAI]:
    min_initial_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        INKLET_TOUGH_MIN_HP,
        INKLET_BASE_MIN_HP,
    )
    max_initial_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        INKLET_TOUGH_MAX_HP,
        INKLET_BASE_MAX_HP,
    )
    hp = rng.next_int(min_initial_hp, max_initial_hp)
    creature = Creature(max_hp=hp, monster_id="INKLET")

    def jab(combat: CombatState) -> None:
        jab_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            INKLET_DEADLY_JAB_DAMAGE,
            INKLET_BASE_JAB_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, jab_dmg)

    def whirlwind(combat: CombatState) -> None:
        whirlwind_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            INKLET_DEADLY_WHIRLWIND_DAMAGE,
            INKLET_BASE_WHIRLWIND_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, whirlwind_dmg, hits=INKLET_WHIRLWIND_HITS)

    def piercing_gaze(combat: CombatState) -> None:
        piercing_gaze_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            INKLET_DEADLY_PIERCING_GAZE_DAMAGE,
            INKLET_BASE_PIERCING_GAZE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, piercing_gaze_dmg)

    jab_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        INKLET_DEADLY_JAB_DAMAGE,
        INKLET_BASE_JAB_DAMAGE,
    )
    whirlwind_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        INKLET_DEADLY_WHIRLWIND_DAMAGE,
        INKLET_BASE_WHIRLWIND_DAMAGE,
    )
    piercing_gaze_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        INKLET_DEADLY_PIERCING_GAZE_DAMAGE,
        INKLET_BASE_PIERCING_GAZE_DAMAGE,
    )

    rand = RandomBranchState(INKLET_RANDOM_MOVE)
    rand.add_branch(INKLET_PIERCING_GAZE_MOVE, MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch(INKLET_WHIRLWIND_MOVE, MoveRepeatType.CANNOT_REPEAT)

    states: dict[str, MonsterState] = {
        INKLET_JAB_MOVE: MoveState(
            INKLET_JAB_MOVE,
            jab,
            [attack_intent(jab_intent_damage)],
            follow_up_id=INKLET_RANDOM_MOVE,
        ),
        INKLET_WHIRLWIND_MOVE: MoveState(
            INKLET_WHIRLWIND_MOVE,
            whirlwind,
            [multi_attack_intent(whirlwind_intent_damage, INKLET_WHIRLWIND_HITS)],
            follow_up_id=INKLET_JAB_MOVE,
        ),
        INKLET_PIERCING_GAZE_MOVE: MoveState(
            INKLET_PIERCING_GAZE_MOVE,
            piercing_gaze,
            [attack_intent(piercing_gaze_intent_damage)],
            follow_up_id=INKLET_JAB_MOVE,
        ),
        INKLET_RANDOM_MOVE: rand,
    }

    creature.apply_power(PowerId.SLIPPERY, INKLET_SLIPPERY_AMOUNT)
    initial = INKLET_WHIRLWIND_MOVE if middle_inklet else INKLET_JAB_MOVE
    return creature, MonsterAI(states, initial, rng)


# ---- Mawler (HP 72 / 76 asc) ----

MAWLER_BASE_HP = 72
MAWLER_TOUGH_HP = 76
MAWLER_BASE_RIP_AND_TEAR_DAMAGE = 14
MAWLER_DEADLY_RIP_AND_TEAR_DAMAGE = 16
MAWLER_BASE_CLAW_DAMAGE = 4
MAWLER_DEADLY_CLAW_DAMAGE = 5
MAWLER_CLAW_HITS = 2
MAWLER_ROAR_VULNERABLE = 3
MAWLER_RANDOM_MOVE = "RAND"
MAWLER_RIP_AND_TEAR_MOVE = "RIP_AND_TEAR_MOVE"
MAWLER_ROAR_MOVE = "ROAR_MOVE"
MAWLER_CLAW_MOVE = "CLAW_MOVE"


def create_mawler(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        MAWLER_TOUGH_HP,
        MAWLER_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id="MAWLER")

    def rip_and_tear(combat: CombatState) -> None:
        rip_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            MAWLER_DEADLY_RIP_AND_TEAR_DAMAGE,
            MAWLER_BASE_RIP_AND_TEAR_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, rip_dmg)

    def roar(combat: CombatState) -> None:
        apply_power_to_living_player_targets(combat, PowerId.VULNERABLE, MAWLER_ROAR_VULNERABLE, applier=creature)

    def claw(combat: CombatState) -> None:
        claw_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            MAWLER_DEADLY_CLAW_DAMAGE,
            MAWLER_BASE_CLAW_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, claw_dmg, hits=MAWLER_CLAW_HITS)

    rip_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        MAWLER_DEADLY_RIP_AND_TEAR_DAMAGE,
        MAWLER_BASE_RIP_AND_TEAR_DAMAGE,
    )
    claw_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        MAWLER_DEADLY_CLAW_DAMAGE,
        MAWLER_BASE_CLAW_DAMAGE,
    )

    rand = RandomBranchState(MAWLER_RANDOM_MOVE)
    rand.add_branch(MAWLER_RIP_AND_TEAR_MOVE, MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch(MAWLER_ROAR_MOVE, MoveRepeatType.USE_ONLY_ONCE)
    rand.add_branch(MAWLER_CLAW_MOVE, MoveRepeatType.CANNOT_REPEAT)

    states: dict[str, MonsterState] = {
        MAWLER_RANDOM_MOVE: rand,
        MAWLER_RIP_AND_TEAR_MOVE: MoveState(
            MAWLER_RIP_AND_TEAR_MOVE,
            rip_and_tear,
            [attack_intent(rip_intent_damage)],
            follow_up_id=MAWLER_RANDOM_MOVE,
        ),
        MAWLER_ROAR_MOVE: MoveState(
            MAWLER_ROAR_MOVE,
            roar,
            [debuff_intent()],
            follow_up_id=MAWLER_RANDOM_MOVE,
        ),
        MAWLER_CLAW_MOVE: MoveState(
            MAWLER_CLAW_MOVE,
            claw,
            [multi_attack_intent(claw_intent_damage, MAWLER_CLAW_HITS)],
            follow_up_id=MAWLER_RANDOM_MOVE,
        ),
    }
    return creature, MonsterAI(states, MAWLER_CLAW_MOVE)


# ---- VineShambler (HP 61 / 64 asc) ----

VINE_SHAMBLER_BASE_HP = 61
VINE_SHAMBLER_TOUGH_HP = 64
VINE_SHAMBLER_BASE_GRASPING_VINES_DAMAGE = 8
VINE_SHAMBLER_DEADLY_GRASPING_VINES_DAMAGE = 9
VINE_SHAMBLER_BASE_SWIPE_DAMAGE = 6
VINE_SHAMBLER_DEADLY_SWIPE_DAMAGE = 7
VINE_SHAMBLER_SWIPE_HITS = 2
VINE_SHAMBLER_BASE_CHOMP_DAMAGE = 16
VINE_SHAMBLER_DEADLY_CHOMP_DAMAGE = 18
VINE_SHAMBLER_TANGLED_AMOUNT = 1
VINE_SHAMBLER_GRASPING_VINES_MOVE = "GRASPING_VINES_MOVE"
VINE_SHAMBLER_SWIPE_MOVE = "SWIPE_MOVE"
VINE_SHAMBLER_CHOMP_MOVE = "CHOMP_MOVE"


def create_vine_shambler(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        VINE_SHAMBLER_TOUGH_HP,
        VINE_SHAMBLER_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id="VINE_SHAMBLER")

    def grasping_vines(combat: CombatState) -> None:
        grasping_vines_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            VINE_SHAMBLER_DEADLY_GRASPING_VINES_DAMAGE,
            VINE_SHAMBLER_BASE_GRASPING_VINES_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, grasping_vines_dmg)
        apply_power_to_living_player_targets(combat, PowerId.TANGLED, VINE_SHAMBLER_TANGLED_AMOUNT, applier=creature)

    def swipe(combat: CombatState) -> None:
        swipe_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            VINE_SHAMBLER_DEADLY_SWIPE_DAMAGE,
            VINE_SHAMBLER_BASE_SWIPE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, swipe_dmg, hits=VINE_SHAMBLER_SWIPE_HITS)

    def chomp(combat: CombatState) -> None:
        chomp_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            VINE_SHAMBLER_DEADLY_CHOMP_DAMAGE,
            VINE_SHAMBLER_BASE_CHOMP_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, chomp_dmg)

    grasping_vines_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        VINE_SHAMBLER_DEADLY_GRASPING_VINES_DAMAGE,
        VINE_SHAMBLER_BASE_GRASPING_VINES_DAMAGE,
    )
    swipe_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        VINE_SHAMBLER_DEADLY_SWIPE_DAMAGE,
        VINE_SHAMBLER_BASE_SWIPE_DAMAGE,
    )
    chomp_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        VINE_SHAMBLER_DEADLY_CHOMP_DAMAGE,
        VINE_SHAMBLER_BASE_CHOMP_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        VINE_SHAMBLER_SWIPE_MOVE: MoveState(
            VINE_SHAMBLER_SWIPE_MOVE,
            swipe,
            [multi_attack_intent(swipe_intent_damage, VINE_SHAMBLER_SWIPE_HITS)],
            follow_up_id=VINE_SHAMBLER_GRASPING_VINES_MOVE,
        ),
        VINE_SHAMBLER_GRASPING_VINES_MOVE: MoveState(
            VINE_SHAMBLER_GRASPING_VINES_MOVE,
            grasping_vines,
            [attack_intent(grasping_vines_intent_damage), Intent(IntentType.CARD_DEBUFF)],
            follow_up_id=VINE_SHAMBLER_CHOMP_MOVE,
        ),
        VINE_SHAMBLER_CHOMP_MOVE: MoveState(
            VINE_SHAMBLER_CHOMP_MOVE,
            chomp,
            [attack_intent(chomp_intent_damage)],
            follow_up_id=VINE_SHAMBLER_SWIPE_MOVE,
        ),
    }

    return creature, MonsterAI(states, VINE_SHAMBLER_SWIPE_MOVE)


# ---- SlitheringStrangler (HP 53-55 / 54-56 asc) ----

SLITHERING_STRANGLER_BASE_MIN_HP = 53
SLITHERING_STRANGLER_BASE_MAX_HP = 55
SLITHERING_STRANGLER_TOUGH_MIN_HP = 54
SLITHERING_STRANGLER_TOUGH_MAX_HP = 56
SLITHERING_STRANGLER_BASE_TWACK_DAMAGE = 7
SLITHERING_STRANGLER_DEADLY_TWACK_DAMAGE = 8
SLITHERING_STRANGLER_BASE_LASH_DAMAGE = 12
SLITHERING_STRANGLER_DEADLY_LASH_DAMAGE = 13
SLITHERING_STRANGLER_TWACK_BLOCK = 5
SLITHERING_STRANGLER_CONSTRICT_AMOUNT = 3
SLITHERING_STRANGLER_RANDOM_MOVE = "rand"
SLITHERING_STRANGLER_CONSTRICT_MOVE = "CONSTRICT"
SLITHERING_STRANGLER_TWACK_MOVE = "TWACK"
SLITHERING_STRANGLER_LASH_MOVE = "LASH"


def create_slithering_strangler(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        SLITHERING_STRANGLER_TOUGH_MIN_HP,
        SLITHERING_STRANGLER_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        SLITHERING_STRANGLER_TOUGH_MAX_HP,
        SLITHERING_STRANGLER_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id="SLITHERING_STRANGLER")

    def constrict(combat: CombatState) -> None:
        apply_power_to_living_player_targets(
            combat,
            PowerId.CONSTRICT,
            SLITHERING_STRANGLER_CONSTRICT_AMOUNT,
            applier=creature,
        )

    def twack(combat: CombatState) -> None:
        twack_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            SLITHERING_STRANGLER_DEADLY_TWACK_DAMAGE,
            SLITHERING_STRANGLER_BASE_TWACK_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, twack_dmg)
        _gain_block(creature, SLITHERING_STRANGLER_TWACK_BLOCK, combat)

    def lash(combat: CombatState) -> None:
        lash_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            SLITHERING_STRANGLER_DEADLY_LASH_DAMAGE,
            SLITHERING_STRANGLER_BASE_LASH_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, lash_dmg)

    twack_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        SLITHERING_STRANGLER_DEADLY_TWACK_DAMAGE,
        SLITHERING_STRANGLER_BASE_TWACK_DAMAGE,
    )
    lash_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        SLITHERING_STRANGLER_DEADLY_LASH_DAMAGE,
        SLITHERING_STRANGLER_BASE_LASH_DAMAGE,
    )

    rand = RandomBranchState(SLITHERING_STRANGLER_RANDOM_MOVE)
    rand.add_branch(SLITHERING_STRANGLER_TWACK_MOVE, MoveRepeatType.CAN_REPEAT_FOREVER)
    rand.add_branch(SLITHERING_STRANGLER_LASH_MOVE, MoveRepeatType.CAN_REPEAT_FOREVER)

    states: dict[str, MonsterState] = {
        SLITHERING_STRANGLER_CONSTRICT_MOVE: MoveState(
            SLITHERING_STRANGLER_CONSTRICT_MOVE,
            constrict,
            [debuff_intent()],
            follow_up_id=SLITHERING_STRANGLER_RANDOM_MOVE,
        ),
        SLITHERING_STRANGLER_TWACK_MOVE: MoveState(
            SLITHERING_STRANGLER_TWACK_MOVE,
            twack,
            [attack_intent(twack_intent_damage), defend_intent()],
            follow_up_id=SLITHERING_STRANGLER_CONSTRICT_MOVE,
        ),
        SLITHERING_STRANGLER_LASH_MOVE: MoveState(
            SLITHERING_STRANGLER_LASH_MOVE,
            lash,
            [attack_intent(lash_intent_damage)],
            follow_up_id=SLITHERING_STRANGLER_CONSTRICT_MOVE,
        ),
        SLITHERING_STRANGLER_RANDOM_MOVE: rand,
    }

    return creature, MonsterAI(states, SLITHERING_STRANGLER_CONSTRICT_MOVE)


# ---- SnappingJaxfruit (HP 31-33 / 34-36 asc) ----

SNAPPING_JAXFRUIT_BASE_MIN_HP = 31
SNAPPING_JAXFRUIT_BASE_MAX_HP = 33
SNAPPING_JAXFRUIT_TOUGH_MIN_HP = 34
SNAPPING_JAXFRUIT_TOUGH_MAX_HP = 36
SNAPPING_JAXFRUIT_BASE_ENERGY_DAMAGE = 3
SNAPPING_JAXFRUIT_DEADLY_ENERGY_DAMAGE = 4
SNAPPING_JAXFRUIT_ENERGY_STRENGTH = 2
SNAPPING_JAXFRUIT_ENERGY_ORB_MOVE = "ENERGY_ORB_MOVE"


def create_snapping_jaxfruit(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_initial_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        SNAPPING_JAXFRUIT_TOUGH_MIN_HP,
        SNAPPING_JAXFRUIT_BASE_MIN_HP,
    )
    max_initial_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        SNAPPING_JAXFRUIT_TOUGH_MAX_HP,
        SNAPPING_JAXFRUIT_BASE_MAX_HP,
    )
    hp = rng.next_int(min_initial_hp, max_initial_hp)
    creature = Creature(max_hp=hp, monster_id="SNAPPING_JAXFRUIT")

    def energy_orb(combat: CombatState) -> None:
        energy_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            SNAPPING_JAXFRUIT_DEADLY_ENERGY_DAMAGE,
            SNAPPING_JAXFRUIT_BASE_ENERGY_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, energy_dmg)
        combat.apply_power_to(creature, PowerId.STRENGTH, SNAPPING_JAXFRUIT_ENERGY_STRENGTH, applier=creature)

    energy_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        SNAPPING_JAXFRUIT_DEADLY_ENERGY_DAMAGE,
        SNAPPING_JAXFRUIT_BASE_ENERGY_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        SNAPPING_JAXFRUIT_ENERGY_ORB_MOVE: MoveState(
            SNAPPING_JAXFRUIT_ENERGY_ORB_MOVE,
            energy_orb,
            [attack_intent(energy_intent_damage), buff_intent()],
            follow_up_id=SNAPPING_JAXFRUIT_ENERGY_ORB_MOVE,
        ),
    }

    return creature, MonsterAI(states, SNAPPING_JAXFRUIT_ENERGY_ORB_MOVE)


# ---- RubyRaiders ----

ASSASSIN_RUBY_RAIDER_ID = "ASSASSIN_RUBY_RAIDER"
ASSASSIN_RUBY_RAIDER_BASE_MIN_HP = 18
ASSASSIN_RUBY_RAIDER_BASE_MAX_HP = 23
ASSASSIN_RUBY_RAIDER_TOUGH_MIN_HP = 19
ASSASSIN_RUBY_RAIDER_TOUGH_MAX_HP = 24
ASSASSIN_RUBY_RAIDER_BASE_KILLSHOT_DAMAGE = 10
ASSASSIN_RUBY_RAIDER_DEADLY_KILLSHOT_DAMAGE = 11
ASSASSIN_RUBY_RAIDER_KILLSHOT_MOVE = "KILLSHOT_MOVE"

AXE_RUBY_RAIDER_ID = "AXE_RUBY_RAIDER"
AXE_RUBY_RAIDER_BASE_MIN_HP = 20
AXE_RUBY_RAIDER_BASE_MAX_HP = 22
AXE_RUBY_RAIDER_TOUGH_MIN_HP = 21
AXE_RUBY_RAIDER_TOUGH_MAX_HP = 23
AXE_RUBY_RAIDER_BASE_SWING_DAMAGE = 5
AXE_RUBY_RAIDER_DEADLY_SWING_DAMAGE = 6
AXE_RUBY_RAIDER_BASE_SWING_BLOCK = 5
AXE_RUBY_RAIDER_DEADLY_SWING_BLOCK = 6
AXE_RUBY_RAIDER_BASE_BIG_SWING_DAMAGE = 12
AXE_RUBY_RAIDER_DEADLY_BIG_SWING_DAMAGE = 13
AXE_RUBY_RAIDER_SWING_1_MOVE = "SWING_1"
AXE_RUBY_RAIDER_SWING_2_MOVE = "SWING_2"
AXE_RUBY_RAIDER_BIG_SWING_MOVE = "BIG_SWING"

BRUTE_RUBY_RAIDER_ID = "BRUTE_RUBY_RAIDER"
BRUTE_RUBY_RAIDER_BASE_MIN_HP = 30
BRUTE_RUBY_RAIDER_BASE_MAX_HP = 33
BRUTE_RUBY_RAIDER_TOUGH_MIN_HP = 31
BRUTE_RUBY_RAIDER_TOUGH_MAX_HP = 34
BRUTE_RUBY_RAIDER_BASE_BEAT_DAMAGE = 7
BRUTE_RUBY_RAIDER_DEADLY_BEAT_DAMAGE = 8
BRUTE_RUBY_RAIDER_ROAR_STRENGTH = 3
BRUTE_RUBY_RAIDER_BEAT_MOVE = "BEAT_MOVE"
BRUTE_RUBY_RAIDER_ROAR_MOVE = "ROAR_MOVE"

CROSSBOW_RUBY_RAIDER_ID = "CROSSBOW_RUBY_RAIDER"
CROSSBOW_RUBY_RAIDER_BASE_MIN_HP = 18
CROSSBOW_RUBY_RAIDER_BASE_MAX_HP = 21
CROSSBOW_RUBY_RAIDER_TOUGH_MIN_HP = 19
CROSSBOW_RUBY_RAIDER_TOUGH_MAX_HP = 22
CROSSBOW_RUBY_RAIDER_BASE_FIRE_DAMAGE = 14
CROSSBOW_RUBY_RAIDER_DEADLY_FIRE_DAMAGE = 16
CROSSBOW_RUBY_RAIDER_RELOAD_BLOCK = 3
CROSSBOW_RUBY_RAIDER_RELOAD_MOVE = "RELOAD_MOVE"
CROSSBOW_RUBY_RAIDER_FIRE_MOVE = "FIRE_MOVE"

TRACKER_RUBY_RAIDER_ID = "TRACKER_RUBY_RAIDER"
TRACKER_RUBY_RAIDER_BASE_MIN_HP = 21
TRACKER_RUBY_RAIDER_BASE_MAX_HP = 25
TRACKER_RUBY_RAIDER_TOUGH_MIN_HP = 22
TRACKER_RUBY_RAIDER_TOUGH_MAX_HP = 26
TRACKER_RUBY_RAIDER_HOUNDS_DAMAGE = 1
TRACKER_RUBY_RAIDER_BASE_HOUNDS_HITS = 8
TRACKER_RUBY_RAIDER_DEADLY_HOUNDS_HITS = 9
TRACKER_RUBY_RAIDER_TRACK_FRAIL = 2
TRACKER_RUBY_RAIDER_TRACK_MOVE = "TRACK_MOVE"
TRACKER_RUBY_RAIDER_HOUNDS_MOVE = "HOUNDS_MOVE"


def create_assassin_ruby_raider(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        ASSASSIN_RUBY_RAIDER_TOUGH_MIN_HP,
        ASSASSIN_RUBY_RAIDER_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        ASSASSIN_RUBY_RAIDER_TOUGH_MAX_HP,
        ASSASSIN_RUBY_RAIDER_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id=ASSASSIN_RUBY_RAIDER_ID)

    def killshot(combat: CombatState) -> None:
        killshot_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            ASSASSIN_RUBY_RAIDER_DEADLY_KILLSHOT_DAMAGE,
            ASSASSIN_RUBY_RAIDER_BASE_KILLSHOT_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, killshot_dmg)

    killshot_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        ASSASSIN_RUBY_RAIDER_DEADLY_KILLSHOT_DAMAGE,
        ASSASSIN_RUBY_RAIDER_BASE_KILLSHOT_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        ASSASSIN_RUBY_RAIDER_KILLSHOT_MOVE: MoveState(
            ASSASSIN_RUBY_RAIDER_KILLSHOT_MOVE,
            killshot,
            [attack_intent(killshot_intent_damage)],
            follow_up_id=ASSASSIN_RUBY_RAIDER_KILLSHOT_MOVE,
        ),
    }
    return creature, MonsterAI(states, ASSASSIN_RUBY_RAIDER_KILLSHOT_MOVE)


def create_axe_ruby_raider(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        AXE_RUBY_RAIDER_TOUGH_MIN_HP,
        AXE_RUBY_RAIDER_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        AXE_RUBY_RAIDER_TOUGH_MAX_HP,
        AXE_RUBY_RAIDER_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id=AXE_RUBY_RAIDER_ID)

    def swing(combat: CombatState) -> None:
        swing_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            AXE_RUBY_RAIDER_DEADLY_SWING_DAMAGE,
            AXE_RUBY_RAIDER_BASE_SWING_DAMAGE,
        )
        swing_block = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            AXE_RUBY_RAIDER_DEADLY_SWING_BLOCK,
            AXE_RUBY_RAIDER_BASE_SWING_BLOCK,
        )
        _deal_damage_to_player(combat, creature, swing_dmg)
        _gain_block(creature, swing_block, combat)

    def big_swing(combat: CombatState) -> None:
        big_swing_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            AXE_RUBY_RAIDER_DEADLY_BIG_SWING_DAMAGE,
            AXE_RUBY_RAIDER_BASE_BIG_SWING_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, big_swing_dmg)

    swing_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        AXE_RUBY_RAIDER_DEADLY_SWING_DAMAGE,
        AXE_RUBY_RAIDER_BASE_SWING_DAMAGE,
    )
    big_swing_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        AXE_RUBY_RAIDER_DEADLY_BIG_SWING_DAMAGE,
        AXE_RUBY_RAIDER_BASE_BIG_SWING_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        AXE_RUBY_RAIDER_SWING_1_MOVE: MoveState(
            AXE_RUBY_RAIDER_SWING_1_MOVE,
            swing,
            [attack_intent(swing_intent_damage), defend_intent()],
            follow_up_id=AXE_RUBY_RAIDER_SWING_2_MOVE,
        ),
        AXE_RUBY_RAIDER_SWING_2_MOVE: MoveState(
            AXE_RUBY_RAIDER_SWING_2_MOVE,
            swing,
            [attack_intent(swing_intent_damage), defend_intent()],
            follow_up_id=AXE_RUBY_RAIDER_BIG_SWING_MOVE,
        ),
        AXE_RUBY_RAIDER_BIG_SWING_MOVE: MoveState(
            AXE_RUBY_RAIDER_BIG_SWING_MOVE,
            big_swing,
            [attack_intent(big_swing_intent_damage)],
            follow_up_id=AXE_RUBY_RAIDER_SWING_1_MOVE,
        ),
    }
    return creature, MonsterAI(states, AXE_RUBY_RAIDER_SWING_1_MOVE)


def create_brute_ruby_raider(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        BRUTE_RUBY_RAIDER_TOUGH_MIN_HP,
        BRUTE_RUBY_RAIDER_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        BRUTE_RUBY_RAIDER_TOUGH_MAX_HP,
        BRUTE_RUBY_RAIDER_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id=BRUTE_RUBY_RAIDER_ID)

    def beat(combat: CombatState) -> None:
        beat_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            BRUTE_RUBY_RAIDER_DEADLY_BEAT_DAMAGE,
            BRUTE_RUBY_RAIDER_BASE_BEAT_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, beat_dmg)

    def roar(combat: CombatState) -> None:
        creature.apply_power(PowerId.STRENGTH, BRUTE_RUBY_RAIDER_ROAR_STRENGTH)

    beat_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        BRUTE_RUBY_RAIDER_DEADLY_BEAT_DAMAGE,
        BRUTE_RUBY_RAIDER_BASE_BEAT_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        BRUTE_RUBY_RAIDER_BEAT_MOVE: MoveState(
            BRUTE_RUBY_RAIDER_BEAT_MOVE,
            beat,
            [attack_intent(beat_intent_damage)],
            follow_up_id=BRUTE_RUBY_RAIDER_ROAR_MOVE,
        ),
        BRUTE_RUBY_RAIDER_ROAR_MOVE: MoveState(
            BRUTE_RUBY_RAIDER_ROAR_MOVE,
            roar,
            [buff_intent()],
            follow_up_id=BRUTE_RUBY_RAIDER_BEAT_MOVE,
        ),
    }
    return creature, MonsterAI(states, BRUTE_RUBY_RAIDER_BEAT_MOVE)


def create_crossbow_ruby_raider(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        CROSSBOW_RUBY_RAIDER_TOUGH_MIN_HP,
        CROSSBOW_RUBY_RAIDER_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        CROSSBOW_RUBY_RAIDER_TOUGH_MAX_HP,
        CROSSBOW_RUBY_RAIDER_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id=CROSSBOW_RUBY_RAIDER_ID)

    def fire(combat: CombatState) -> None:
        fire_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            CROSSBOW_RUBY_RAIDER_DEADLY_FIRE_DAMAGE,
            CROSSBOW_RUBY_RAIDER_BASE_FIRE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, fire_dmg)

    def reload(combat: CombatState) -> None:
        _gain_block(creature, CROSSBOW_RUBY_RAIDER_RELOAD_BLOCK, combat)

    fire_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        CROSSBOW_RUBY_RAIDER_DEADLY_FIRE_DAMAGE,
        CROSSBOW_RUBY_RAIDER_BASE_FIRE_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        CROSSBOW_RUBY_RAIDER_RELOAD_MOVE: MoveState(
            CROSSBOW_RUBY_RAIDER_RELOAD_MOVE,
            reload,
            [defend_intent()],
            follow_up_id=CROSSBOW_RUBY_RAIDER_FIRE_MOVE,
        ),
        CROSSBOW_RUBY_RAIDER_FIRE_MOVE: MoveState(
            CROSSBOW_RUBY_RAIDER_FIRE_MOVE,
            fire,
            [attack_intent(fire_intent_damage)],
            follow_up_id=CROSSBOW_RUBY_RAIDER_RELOAD_MOVE,
        ),
    }
    return creature, MonsterAI(states, CROSSBOW_RUBY_RAIDER_RELOAD_MOVE)


def create_tracker_ruby_raider(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        TRACKER_RUBY_RAIDER_TOUGH_MIN_HP,
        TRACKER_RUBY_RAIDER_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        TRACKER_RUBY_RAIDER_TOUGH_MAX_HP,
        TRACKER_RUBY_RAIDER_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id=TRACKER_RUBY_RAIDER_ID)

    def track(combat: CombatState) -> None:
        apply_power_to_living_player_targets(combat, PowerId.FRAIL, TRACKER_RUBY_RAIDER_TRACK_FRAIL, applier=creature)

    def hounds(combat: CombatState) -> None:
        hounds_hits = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            TRACKER_RUBY_RAIDER_DEADLY_HOUNDS_HITS,
            TRACKER_RUBY_RAIDER_BASE_HOUNDS_HITS,
        )
        _deal_damage_to_player(combat, creature, hounds_dmg, hits=hounds_hits)

    hounds_dmg = TRACKER_RUBY_RAIDER_HOUNDS_DAMAGE
    hounds_intent_hits = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        TRACKER_RUBY_RAIDER_DEADLY_HOUNDS_HITS,
        TRACKER_RUBY_RAIDER_BASE_HOUNDS_HITS,
    )

    states: dict[str, MonsterState] = {
        TRACKER_RUBY_RAIDER_TRACK_MOVE: MoveState(
            TRACKER_RUBY_RAIDER_TRACK_MOVE,
            track,
            [debuff_intent()],
            follow_up_id=TRACKER_RUBY_RAIDER_HOUNDS_MOVE,
        ),
        TRACKER_RUBY_RAIDER_HOUNDS_MOVE: MoveState(
            TRACKER_RUBY_RAIDER_HOUNDS_MOVE,
            hounds,
            [multi_attack_intent(hounds_dmg, hounds_intent_hits)],
            follow_up_id=TRACKER_RUBY_RAIDER_HOUNDS_MOVE,
        ),
    }
    return creature, MonsterAI(states, TRACKER_RUBY_RAIDER_TRACK_MOVE)


# ========================================================================
# ELITE ENCOUNTERS
# ========================================================================

# ---- BygoneEffigy (HP 127 / 132 asc) ----

BYGONE_EFFIGY_ID = "BYGONE_EFFIGY"
BYGONE_EFFIGY_BASE_HP = 127
BYGONE_EFFIGY_TOUGH_HP = 132
BYGONE_EFFIGY_BASE_SLASH_DAMAGE = 13
BYGONE_EFFIGY_DEADLY_SLASH_DAMAGE = 15
BYGONE_EFFIGY_WAKE_STRENGTH = 10
BYGONE_EFFIGY_SLOW = 1
BYGONE_EFFIGY_INITIAL_SLEEP_MOVE = "INITIAL_SLEEP_MOVE"
BYGONE_EFFIGY_WAKE_MOVE = "WAKE_MOVE"
BYGONE_EFFIGY_SLEEP_MOVE = "SLEEP_MOVE"
BYGONE_EFFIGY_SLASHES_MOVE = "SLASHES_MOVE"


def create_bygone_effigy(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        BYGONE_EFFIGY_TOUGH_HP,
        BYGONE_EFFIGY_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=BYGONE_EFFIGY_ID)

    def initial_sleep(combat: CombatState) -> None:
        pass  # Does nothing

    def wake(combat: CombatState) -> None:
        creature.apply_power(PowerId.STRENGTH, BYGONE_EFFIGY_WAKE_STRENGTH)

    def sleep_move(combat: CombatState) -> None:
        pass

    def slashes(combat: CombatState) -> None:
        slash_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            BYGONE_EFFIGY_DEADLY_SLASH_DAMAGE,
            BYGONE_EFFIGY_BASE_SLASH_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, slash_dmg)

    slash_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        BYGONE_EFFIGY_DEADLY_SLASH_DAMAGE,
        BYGONE_EFFIGY_BASE_SLASH_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        BYGONE_EFFIGY_INITIAL_SLEEP_MOVE: MoveState(
            BYGONE_EFFIGY_INITIAL_SLEEP_MOVE,
            initial_sleep,
            [sleep_intent()],
            follow_up_id=BYGONE_EFFIGY_WAKE_MOVE,
        ),
        BYGONE_EFFIGY_WAKE_MOVE: MoveState(
            BYGONE_EFFIGY_WAKE_MOVE,
            wake,
            [buff_intent()],
            follow_up_id=BYGONE_EFFIGY_SLASHES_MOVE,
        ),
        BYGONE_EFFIGY_SLEEP_MOVE: MoveState(
            BYGONE_EFFIGY_SLEEP_MOVE,
            sleep_move,
            [sleep_intent()],
            follow_up_id=BYGONE_EFFIGY_SLASHES_MOVE,
        ),
        BYGONE_EFFIGY_SLASHES_MOVE: MoveState(
            BYGONE_EFFIGY_SLASHES_MOVE,
            slashes,
            [attack_intent(slash_intent_damage)],
            follow_up_id=BYGONE_EFFIGY_SLASHES_MOVE,
        ),
    }

    # AfterAddedToRoom: applies Slow power
    creature.apply_power(PowerId.SLOW, BYGONE_EFFIGY_SLOW)

    return creature, MonsterAI(states, BYGONE_EFFIGY_INITIAL_SLEEP_MOVE)


# ---- Byrdonis (HP 81-84 / 90 asc) ----

BYRDONIS_ID = "BYRDONIS"
BYRDONIS_BASE_MIN_HP = 81
BYRDONIS_BASE_MAX_HP = 84
BYRDONIS_TOUGH_HP = 90
BYRDONIS_BASE_PECK_DAMAGE = 3
BYRDONIS_DEADLY_PECK_DAMAGE = 4
BYRDONIS_PECK_HITS = 3
BYRDONIS_BASE_SWOOP_DAMAGE = 17
BYRDONIS_DEADLY_SWOOP_DAMAGE = 19
BYRDONIS_TERRITORIAL = 1
BYRDONIS_SWOOP_MOVE = "SWOOP_MOVE"
BYRDONIS_PECK_MOVE = "PECK_MOVE"


def create_byrdonis(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        BYRDONIS_TOUGH_HP,
        BYRDONIS_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        BYRDONIS_TOUGH_HP,
        BYRDONIS_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id=BYRDONIS_ID)

    def peck(combat: CombatState) -> None:
        peck_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            BYRDONIS_DEADLY_PECK_DAMAGE,
            BYRDONIS_BASE_PECK_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, peck_dmg, hits=BYRDONIS_PECK_HITS)

    def swoop(combat: CombatState) -> None:
        swoop_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            BYRDONIS_DEADLY_SWOOP_DAMAGE,
            BYRDONIS_BASE_SWOOP_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, swoop_dmg)

    peck_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        BYRDONIS_DEADLY_PECK_DAMAGE,
        BYRDONIS_BASE_PECK_DAMAGE,
    )
    swoop_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        BYRDONIS_DEADLY_SWOOP_DAMAGE,
        BYRDONIS_BASE_SWOOP_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        BYRDONIS_SWOOP_MOVE: MoveState(
            BYRDONIS_SWOOP_MOVE,
            swoop,
            [attack_intent(swoop_intent_damage)],
            follow_up_id=BYRDONIS_PECK_MOVE,
        ),
        BYRDONIS_PECK_MOVE: MoveState(
            BYRDONIS_PECK_MOVE,
            peck,
            [multi_attack_intent(peck_intent_damage, BYRDONIS_PECK_HITS)],
            follow_up_id=BYRDONIS_SWOOP_MOVE,
        ),
    }

    # AfterAddedToRoom: applies Territorial power
    creature.apply_power(PowerId.TERRITORIAL, BYRDONIS_TERRITORIAL)

    return creature, MonsterAI(states, BYRDONIS_SWOOP_MOVE)


# ---- PhrogParasite (HP 61-64 / 66-68 asc) ----

PHROG_PARASITE_ID = "PHROG_PARASITE"
PHROG_PARASITE_BASE_MIN_HP = 61
PHROG_PARASITE_BASE_MAX_HP = 64
PHROG_PARASITE_TOUGH_MIN_HP = 66
PHROG_PARASITE_TOUGH_MAX_HP = 68
PHROG_PARASITE_BASE_LASH_DAMAGE = 4
PHROG_PARASITE_DEADLY_LASH_DAMAGE = 5
PHROG_PARASITE_LASH_HITS = 4
PHROG_PARASITE_INFECT_INFECTIONS = 3
PHROG_PARASITE_INFESTED = 4
PHROG_PARASITE_INFECT_MOVE = "INFECT_MOVE"
PHROG_PARASITE_LASH_MOVE = "LASH_MOVE"


def create_phrog_parasite(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        PHROG_PARASITE_TOUGH_MIN_HP,
        PHROG_PARASITE_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        PHROG_PARASITE_TOUGH_MAX_HP,
        PHROG_PARASITE_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id=PHROG_PARASITE_ID)

    def infest(combat: CombatState) -> None:
        add_generated_cards_to_living_player_discards(
            combat,
            make_infection,
            PHROG_PARASITE_INFECT_INFECTIONS,
        )

    def bite(combat: CombatState) -> None:
        lash_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            PHROG_PARASITE_DEADLY_LASH_DAMAGE,
            PHROG_PARASITE_BASE_LASH_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, lash_dmg, hits=PHROG_PARASITE_LASH_HITS)

    lash_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        PHROG_PARASITE_DEADLY_LASH_DAMAGE,
        PHROG_PARASITE_BASE_LASH_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        PHROG_PARASITE_INFECT_MOVE: MoveState(
            PHROG_PARASITE_INFECT_MOVE,
            infest,
            [status_intent()],
            follow_up_id=PHROG_PARASITE_LASH_MOVE,
        ),
        PHROG_PARASITE_LASH_MOVE: MoveState(
            PHROG_PARASITE_LASH_MOVE,
            bite,
            [multi_attack_intent(lash_intent_damage, PHROG_PARASITE_LASH_HITS)],
            follow_up_id=PHROG_PARASITE_INFECT_MOVE,
        ),
    }

    # AfterAddedToRoom: Infested(4)
    creature.apply_power(PowerId.INFESTED, PHROG_PARASITE_INFESTED)

    return creature, MonsterAI(states, PHROG_PARASITE_INFECT_MOVE)


# ========================================================================
# BOSS ENCOUNTERS
# ========================================================================

# ---- Vantom (HP 173 / 183 asc) ----

PARAFRIGHT_HP = 21
PARAFRIGHT_BASE_SLAM_DAMAGE = 16
PARAFRIGHT_DEADLY_SLAM_DAMAGE = 17
PARAFRIGHT_ILLUSION = 1
PARAFRIGHT_MINION = 1
PARAFRIGHT_ID = "PARAFRIGHT"
PARAFRIGHT_SLAM_MOVE = "SLAM_MOVE"

VANTOM_ID = "VANTOM"
VANTOM_BASE_HP = 173
VANTOM_TOUGH_HP = 183
VANTOM_BASE_INK_BLOT_DAMAGE = 7
VANTOM_DEADLY_INK_BLOT_DAMAGE = 8
VANTOM_BASE_INKY_LANCE_DAMAGE = 6
VANTOM_DEADLY_INKY_LANCE_DAMAGE = 7
VANTOM_INKY_LANCE_HITS = 2
VANTOM_BASE_DISMEMBER_DAMAGE = 26
VANTOM_DEADLY_DISMEMBER_DAMAGE = 30
VANTOM_DISMEMBER_WOUNDS = 3
VANTOM_PREPARE_STRENGTH = 2
VANTOM_BASE_SLIPPERY = 8
VANTOM_TOUGH_SLIPPERY = 9
VANTOM_INK_BLOT_MOVE = "INK_BLOT_MOVE"
VANTOM_INKY_LANCE_MOVE = "INKY_LANCE_MOVE"
VANTOM_DISMEMBER_MOVE = "DISMEMBER_MOVE"
VANTOM_PREPARE_MOVE = "PREPARE_MOVE"


def create_parafright(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    creature = Creature(max_hp=PARAFRIGHT_HP, monster_id=PARAFRIGHT_ID)

    def slam(combat: CombatState) -> None:
        slam_damage = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            PARAFRIGHT_DEADLY_SLAM_DAMAGE,
            PARAFRIGHT_BASE_SLAM_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, slam_damage)

    slam_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        PARAFRIGHT_DEADLY_SLAM_DAMAGE,
        PARAFRIGHT_BASE_SLAM_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        PARAFRIGHT_SLAM_MOVE: MoveState(
            PARAFRIGHT_SLAM_MOVE,
            slam,
            [attack_intent(slam_intent_damage)],
            follow_up_id=PARAFRIGHT_SLAM_MOVE,
        ),
    }
    creature.apply_power(PowerId.ILLUSION, PARAFRIGHT_ILLUSION)
    creature.apply_power(PowerId.MINION, PARAFRIGHT_MINION)
    return creature, MonsterAI(states, PARAFRIGHT_SLAM_MOVE)


def create_vantom(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        VANTOM_TOUGH_HP,
        VANTOM_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=VANTOM_ID)
    slippery = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        VANTOM_TOUGH_SLIPPERY,
        VANTOM_BASE_SLIPPERY,
    )

    def ink_blot(combat: CombatState) -> None:
        ink_blot_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            VANTOM_DEADLY_INK_BLOT_DAMAGE,
            VANTOM_BASE_INK_BLOT_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, ink_blot_dmg)

    def inky_lance(combat: CombatState) -> None:
        inky_lance_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            VANTOM_DEADLY_INKY_LANCE_DAMAGE,
            VANTOM_BASE_INKY_LANCE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, inky_lance_dmg, hits=VANTOM_INKY_LANCE_HITS)

    def dismember(combat: CombatState) -> None:
        dismember_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            VANTOM_DEADLY_DISMEMBER_DAMAGE,
            VANTOM_BASE_DISMEMBER_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, dismember_dmg)
        if combat.is_over:
            return
        add_generated_cards_to_living_player_discards(combat, make_wound, VANTOM_DISMEMBER_WOUNDS)

    def prepare(combat: CombatState) -> None:
        creature.apply_power(PowerId.STRENGTH, VANTOM_PREPARE_STRENGTH, applier=creature)

    ink_blot_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        VANTOM_DEADLY_INK_BLOT_DAMAGE,
        VANTOM_BASE_INK_BLOT_DAMAGE,
    )
    inky_lance_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        VANTOM_DEADLY_INKY_LANCE_DAMAGE,
        VANTOM_BASE_INKY_LANCE_DAMAGE,
    )
    dismember_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        VANTOM_DEADLY_DISMEMBER_DAMAGE,
        VANTOM_BASE_DISMEMBER_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        VANTOM_INK_BLOT_MOVE: MoveState(
            VANTOM_INK_BLOT_MOVE,
            ink_blot,
            [attack_intent(ink_blot_intent_damage)],
            follow_up_id=VANTOM_INKY_LANCE_MOVE,
        ),
        VANTOM_INKY_LANCE_MOVE: MoveState(
            VANTOM_INKY_LANCE_MOVE,
            inky_lance,
            [multi_attack_intent(inky_lance_intent_damage, VANTOM_INKY_LANCE_HITS)],
            follow_up_id=VANTOM_DISMEMBER_MOVE,
        ),
        VANTOM_DISMEMBER_MOVE: MoveState(
            VANTOM_DISMEMBER_MOVE,
            dismember,
            [attack_intent(dismember_intent_damage), status_intent()],
            follow_up_id=VANTOM_PREPARE_MOVE,
        ),
        VANTOM_PREPARE_MOVE: MoveState(
            VANTOM_PREPARE_MOVE,
            prepare,
            [buff_intent()],
            follow_up_id=VANTOM_INK_BLOT_MOVE,
        ),
    }
    creature.apply_power(PowerId.SLIPPERY, slippery)
    return creature, MonsterAI(states, VANTOM_INK_BLOT_MOVE)


# ---- CeremonialBeast (HP 252 / 262 asc) ----

CEREMONIAL_BEAST_ID = "CEREMONIAL_BEAST"
CEREMONIAL_BEAST_BASE_HP = 252
CEREMONIAL_BEAST_TOUGH_HP = 262
CEREMONIAL_BEAST_BASE_PLOW_AMOUNT = 150
CEREMONIAL_BEAST_DEADLY_PLOW_AMOUNT = 160
CEREMONIAL_BEAST_BASE_PLOW_DAMAGE = 18
CEREMONIAL_BEAST_DEADLY_PLOW_DAMAGE = 20
CEREMONIAL_BEAST_PLOW_STRENGTH = 2
CEREMONIAL_BEAST_BASE_STOMP_DAMAGE = 15
CEREMONIAL_BEAST_DEADLY_STOMP_DAMAGE = 17
CEREMONIAL_BEAST_BASE_CRUSH_DAMAGE = 17
CEREMONIAL_BEAST_DEADLY_CRUSH_DAMAGE = 19
CEREMONIAL_BEAST_BASE_CRUSH_STRENGTH = 3
CEREMONIAL_BEAST_DEADLY_CRUSH_STRENGTH = 4
CEREMONIAL_BEAST_BEAST_CRY_RINGING = 1
CEREMONIAL_BEAST_STAMP_MOVE = "STAMP_MOVE"
CEREMONIAL_BEAST_PLOW_MOVE = "PLOW_MOVE"
CEREMONIAL_BEAST_STUN_MOVE = "STUN_MOVE"
CEREMONIAL_BEAST_BEAST_CRY_MOVE = "BEAST_CRY_MOVE"
CEREMONIAL_BEAST_STOMP_MOVE = "STOMP_MOVE"
CEREMONIAL_BEAST_CRUSH_MOVE = "CRUSH_MOVE"


def create_ceremonial_beast(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        CEREMONIAL_BEAST_TOUGH_HP,
        CEREMONIAL_BEAST_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=CEREMONIAL_BEAST_ID)

    def stamp(combat: CombatState) -> None:
        plow_amount = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            CEREMONIAL_BEAST_DEADLY_PLOW_AMOUNT,
            CEREMONIAL_BEAST_BASE_PLOW_AMOUNT,
        )
        creature.apply_power(PowerId.PLOW, plow_amount)

    def plow_move(combat: CombatState) -> None:
        plow_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            CEREMONIAL_BEAST_DEADLY_PLOW_DAMAGE,
            CEREMONIAL_BEAST_BASE_PLOW_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, plow_dmg)
        if combat.is_over:
            return
        combat.apply_power_to(creature, PowerId.STRENGTH, CEREMONIAL_BEAST_PLOW_STRENGTH, applier=creature)

    def stun(combat: CombatState) -> None:
        pass

    def beast_cry(combat: CombatState) -> None:
        apply_power_to_living_player_targets(
            combat,
            PowerId.RINGING,
            CEREMONIAL_BEAST_BEAST_CRY_RINGING,
            applier=creature,
        )

    def stomp(combat: CombatState) -> None:
        stomp_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            CEREMONIAL_BEAST_DEADLY_STOMP_DAMAGE,
            CEREMONIAL_BEAST_BASE_STOMP_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, stomp_dmg)

    def crush(combat: CombatState) -> None:
        crush_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            CEREMONIAL_BEAST_DEADLY_CRUSH_DAMAGE,
            CEREMONIAL_BEAST_BASE_CRUSH_DAMAGE,
        )
        crush_strength = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            CEREMONIAL_BEAST_DEADLY_CRUSH_STRENGTH,
            CEREMONIAL_BEAST_BASE_CRUSH_STRENGTH,
        )
        _deal_damage_to_player(combat, creature, crush_dmg)
        if combat.is_over:
            return
        combat.apply_power_to(creature, PowerId.STRENGTH, crush_strength, applier=creature)

    plow_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        CEREMONIAL_BEAST_DEADLY_PLOW_DAMAGE,
        CEREMONIAL_BEAST_BASE_PLOW_DAMAGE,
    )
    stomp_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        CEREMONIAL_BEAST_DEADLY_STOMP_DAMAGE,
        CEREMONIAL_BEAST_BASE_STOMP_DAMAGE,
    )
    crush_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        CEREMONIAL_BEAST_DEADLY_CRUSH_DAMAGE,
        CEREMONIAL_BEAST_BASE_CRUSH_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        CEREMONIAL_BEAST_STAMP_MOVE: MoveState(
            CEREMONIAL_BEAST_STAMP_MOVE,
            stamp,
            [buff_intent()],
            follow_up_id=CEREMONIAL_BEAST_PLOW_MOVE,
        ),
        CEREMONIAL_BEAST_PLOW_MOVE: MoveState(
            CEREMONIAL_BEAST_PLOW_MOVE,
            plow_move,
            [attack_intent(plow_intent_damage), buff_intent()],
            follow_up_id=CEREMONIAL_BEAST_PLOW_MOVE,
        ),
        CEREMONIAL_BEAST_STUN_MOVE: MoveState(
            CEREMONIAL_BEAST_STUN_MOVE,
            stun,
            [Intent(IntentType.STUN)],
            follow_up_id=CEREMONIAL_BEAST_BEAST_CRY_MOVE,
            must_perform_once=True,
        ),
        CEREMONIAL_BEAST_BEAST_CRY_MOVE: MoveState(
            CEREMONIAL_BEAST_BEAST_CRY_MOVE,
            beast_cry,
            [debuff_intent()],
            follow_up_id=CEREMONIAL_BEAST_STOMP_MOVE,
        ),
        CEREMONIAL_BEAST_STOMP_MOVE: MoveState(
            CEREMONIAL_BEAST_STOMP_MOVE,
            stomp,
            [attack_intent(stomp_intent_damage)],
            follow_up_id=CEREMONIAL_BEAST_CRUSH_MOVE,
        ),
        CEREMONIAL_BEAST_CRUSH_MOVE: MoveState(
            CEREMONIAL_BEAST_CRUSH_MOVE,
            crush,
            [attack_intent(crush_intent_damage), buff_intent()],
            follow_up_id=CEREMONIAL_BEAST_BEAST_CRY_MOVE,
        ),
    }
    return creature, MonsterAI(states, CEREMONIAL_BEAST_STAMP_MOVE)


# ---- TheKin (KinPriest + 2 KinFollowers) ----

KIN_PRIEST_ID = "KIN_PRIEST"
KIN_PRIEST_BASE_HP = 190
KIN_PRIEST_TOUGH_HP = 199
KIN_PRIEST_BASE_ORB_OF_FRAILTY_DAMAGE = 8
KIN_PRIEST_DEADLY_ORB_OF_FRAILTY_DAMAGE = 9
KIN_PRIEST_BASE_ORB_OF_WEAKNESS_DAMAGE = 8
KIN_PRIEST_DEADLY_ORB_OF_WEAKNESS_DAMAGE = 9
KIN_PRIEST_BEAM_DAMAGE = 3
KIN_PRIEST_BEAM_HITS = 3
KIN_PRIEST_BASE_RITUAL_STRENGTH = 2
KIN_PRIEST_DEADLY_RITUAL_STRENGTH = 3
KIN_PRIEST_ORB_DEBUFF = 1
KIN_PRIEST_ORB_OF_FRAILTY_MOVE = "ORB_OF_FRAILTY_MOVE"
KIN_PRIEST_ORB_OF_WEAKNESS_MOVE = "ORB_OF_WEAKNESS_MOVE"
KIN_PRIEST_BEAM_MOVE = "BEAM_MOVE"
KIN_PRIEST_RITUAL_MOVE = "RITUAL_MOVE"

KIN_FOLLOWER_ID = "KIN_FOLLOWER"
KIN_FOLLOWER_BASE_MIN_HP = 58
KIN_FOLLOWER_BASE_MAX_HP = 59
KIN_FOLLOWER_TOUGH_MIN_HP = 62
KIN_FOLLOWER_TOUGH_MAX_HP = 63
KIN_FOLLOWER_QUICK_SLASH_DAMAGE = 5
KIN_FOLLOWER_BOOMERANG_DAMAGE = 2
KIN_FOLLOWER_BOOMERANG_HITS = 2
KIN_FOLLOWER_BASE_DANCE_STRENGTH = 2
KIN_FOLLOWER_DEADLY_DANCE_STRENGTH = 3
KIN_FOLLOWER_MINION = 1
KIN_FOLLOWER_QUICK_SLASH_MOVE = "QUICK_SLASH_MOVE"
KIN_FOLLOWER_BOOMERANG_MOVE = "BOOMERANG_MOVE"
KIN_FOLLOWER_POWER_DANCE_MOVE = "POWER_DANCE_MOVE"


def create_kin_priest(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        KIN_PRIEST_TOUGH_HP,
        KIN_PRIEST_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=KIN_PRIEST_ID)

    def orb_of_frailty(combat: CombatState) -> None:
        orb_of_frailty_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            KIN_PRIEST_DEADLY_ORB_OF_FRAILTY_DAMAGE,
            KIN_PRIEST_BASE_ORB_OF_FRAILTY_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, orb_of_frailty_dmg)
        if combat.is_over:
            return
        apply_power_to_living_player_targets(combat, PowerId.FRAIL, KIN_PRIEST_ORB_DEBUFF, applier=creature)

    def orb_of_weakness(combat: CombatState) -> None:
        orb_of_weakness_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            KIN_PRIEST_DEADLY_ORB_OF_WEAKNESS_DAMAGE,
            KIN_PRIEST_BASE_ORB_OF_WEAKNESS_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, orb_of_weakness_dmg)
        if combat.is_over:
            return
        apply_power_to_living_player_targets(combat, PowerId.WEAK, KIN_PRIEST_ORB_DEBUFF, applier=creature)

    def beam(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, KIN_PRIEST_BEAM_DAMAGE, hits=KIN_PRIEST_BEAM_HITS)

    def ritual(combat: CombatState) -> None:
        ritual_strength = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            KIN_PRIEST_DEADLY_RITUAL_STRENGTH,
            KIN_PRIEST_BASE_RITUAL_STRENGTH,
        )
        combat.apply_power_to(creature, PowerId.STRENGTH, ritual_strength, applier=creature)

    orb_of_frailty_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        KIN_PRIEST_DEADLY_ORB_OF_FRAILTY_DAMAGE,
        KIN_PRIEST_BASE_ORB_OF_FRAILTY_DAMAGE,
    )
    orb_of_weakness_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        KIN_PRIEST_DEADLY_ORB_OF_WEAKNESS_DAMAGE,
        KIN_PRIEST_BASE_ORB_OF_WEAKNESS_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        KIN_PRIEST_ORB_OF_FRAILTY_MOVE: MoveState(
            KIN_PRIEST_ORB_OF_FRAILTY_MOVE,
            orb_of_frailty,
            [attack_intent(orb_of_frailty_intent_damage), debuff_intent()],
            follow_up_id=KIN_PRIEST_ORB_OF_WEAKNESS_MOVE,
        ),
        KIN_PRIEST_ORB_OF_WEAKNESS_MOVE: MoveState(
            KIN_PRIEST_ORB_OF_WEAKNESS_MOVE,
            orb_of_weakness,
            [attack_intent(orb_of_weakness_intent_damage), debuff_intent()],
            follow_up_id=KIN_PRIEST_BEAM_MOVE,
        ),
        KIN_PRIEST_BEAM_MOVE: MoveState(
            KIN_PRIEST_BEAM_MOVE,
            beam,
            [multi_attack_intent(KIN_PRIEST_BEAM_DAMAGE, KIN_PRIEST_BEAM_HITS)],
            follow_up_id=KIN_PRIEST_RITUAL_MOVE,
        ),
        KIN_PRIEST_RITUAL_MOVE: MoveState(
            KIN_PRIEST_RITUAL_MOVE,
            ritual,
            [buff_intent()],
            follow_up_id=KIN_PRIEST_ORB_OF_FRAILTY_MOVE,
        ),
    }
    return creature, MonsterAI(states, KIN_PRIEST_ORB_OF_FRAILTY_MOVE)


def create_kin_follower(
    rng: Rng,
    *,
    starts_with_dance: bool = False,
    ascension_level: int = 0,
) -> tuple[Creature, MonsterAI]:
    min_initial_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        KIN_FOLLOWER_TOUGH_MIN_HP,
        KIN_FOLLOWER_BASE_MIN_HP,
    )
    max_initial_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        KIN_FOLLOWER_TOUGH_MAX_HP,
        KIN_FOLLOWER_BASE_MAX_HP,
    )
    hp = rng.next_int(min_initial_hp, max_initial_hp)
    creature = Creature(max_hp=hp, monster_id=KIN_FOLLOWER_ID)

    def quick_slash(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, KIN_FOLLOWER_QUICK_SLASH_DAMAGE)

    def boomerang(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, KIN_FOLLOWER_BOOMERANG_DAMAGE, hits=KIN_FOLLOWER_BOOMERANG_HITS)

    def power_dance(combat: CombatState) -> None:
        dance_strength = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            KIN_FOLLOWER_DEADLY_DANCE_STRENGTH,
            KIN_FOLLOWER_BASE_DANCE_STRENGTH,
        )
        combat.apply_power_to(creature, PowerId.STRENGTH, dance_strength, applier=creature)

    states: dict[str, MonsterState] = {
        KIN_FOLLOWER_QUICK_SLASH_MOVE: MoveState(
            KIN_FOLLOWER_QUICK_SLASH_MOVE,
            quick_slash,
            [attack_intent(KIN_FOLLOWER_QUICK_SLASH_DAMAGE)],
            follow_up_id=KIN_FOLLOWER_BOOMERANG_MOVE,
        ),
        KIN_FOLLOWER_BOOMERANG_MOVE: MoveState(
            KIN_FOLLOWER_BOOMERANG_MOVE,
            boomerang,
            [multi_attack_intent(KIN_FOLLOWER_BOOMERANG_DAMAGE, KIN_FOLLOWER_BOOMERANG_HITS)],
            follow_up_id=KIN_FOLLOWER_POWER_DANCE_MOVE,
        ),
        KIN_FOLLOWER_POWER_DANCE_MOVE: MoveState(
            KIN_FOLLOWER_POWER_DANCE_MOVE,
            power_dance,
            [buff_intent()],
            follow_up_id=KIN_FOLLOWER_QUICK_SLASH_MOVE,
        ),
    }

    creature.apply_power(PowerId.MINION, KIN_FOLLOWER_MINION)
    initial = KIN_FOLLOWER_POWER_DANCE_MOVE if starts_with_dance else KIN_FOLLOWER_QUICK_SLASH_MOVE
    return creature, MonsterAI(states, initial, rng)
