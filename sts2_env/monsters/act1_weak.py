"""Act 1 weak monsters: Nibbit, ShrinkerBeetle, FuzzyWurmCrawler, Slimes.

All HP ranges and damage values verified against decompiled C# source.
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from sts2_env.core.creature import Creature
from sts2_env.core.enums import CombatSide, MoveRepeatType, PowerId, ValueProp
from sts2_env.core.damage import calculate_damage, apply_damage
from sts2_env.core.rng import Rng
from sts2_env.monsters.intents import (
    Intent, IntentType, attack_intent, buff_intent,
    debuff_intent, strong_debuff_intent, status_intent,
)
from sts2_env.monsters.state_machine import (
    ConditionalBranchState, MonsterAI, MonsterState, MoveState, RandomBranchState,
)
from sts2_env.monsters.block import gain_move_block
from sts2_env.monsters.targets import (
    add_generated_cards_to_living_player_discards,
    apply_power_to_living_player_targets,
    living_player_targets,
)
from sts2_env.cards.status import make_slimed

if TYPE_CHECKING:
    from sts2_env.core.combat import CombatState

TOUGH_ENEMIES_ASCENSION_LEVEL = 8
DEADLY_ENEMIES_ASCENSION_LEVEL = 9


# ---- Helpers ----

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


def _gain_block(creature: Creature, amount: int, combat: CombatState) -> None:
    gain_move_block(creature, amount, combat)


SHRINKER_BEETLE_BASE_MIN_HP = 38
SHRINKER_BEETLE_BASE_MAX_HP = 40
SHRINKER_BEETLE_TOUGH_MIN_HP = 40
SHRINKER_BEETLE_TOUGH_MAX_HP = 42
SHRINKER_BEETLE_BASE_CHOMP_DAMAGE = 7
SHRINKER_BEETLE_DEADLY_CHOMP_DAMAGE = 8
SHRINKER_BEETLE_BASE_STOMP_DAMAGE = 13
SHRINKER_BEETLE_DEADLY_STOMP_DAMAGE = 14
SHRINKER_BEETLE_SHRINK_AMOUNT = -1


# ---- ShrinkerBeetle (HP 38-40) ----
# Cycle: SHRINKER_MOVE → CHOMP_MOVE → STOMP_MOVE → CHOMP_MOVE → STOMP_MOVE...

def create_shrinker_beetle(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        SHRINKER_BEETLE_TOUGH_MIN_HP,
        SHRINKER_BEETLE_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        SHRINKER_BEETLE_TOUGH_MAX_HP,
        SHRINKER_BEETLE_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id="SHRINKER_BEETLE")

    def shrink(combat: CombatState) -> None:
        apply_power_to_living_player_targets(
            combat,
            PowerId.SHRINK,
            SHRINKER_BEETLE_SHRINK_AMOUNT,
            applier=creature,
        )

    def chomp(combat: CombatState) -> None:
        chomp_damage = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            SHRINKER_BEETLE_DEADLY_CHOMP_DAMAGE,
            SHRINKER_BEETLE_BASE_CHOMP_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, chomp_damage)

    def stomp(combat: CombatState) -> None:
        stomp_damage = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            SHRINKER_BEETLE_DEADLY_STOMP_DAMAGE,
            SHRINKER_BEETLE_BASE_STOMP_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, stomp_damage)

    chomp_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        SHRINKER_BEETLE_DEADLY_CHOMP_DAMAGE,
        SHRINKER_BEETLE_BASE_CHOMP_DAMAGE,
    )
    stomp_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        SHRINKER_BEETLE_DEADLY_STOMP_DAMAGE,
        SHRINKER_BEETLE_BASE_STOMP_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        "SHRINKER_MOVE": MoveState(
            "SHRINKER_MOVE",
            shrink,
            [strong_debuff_intent()],
            follow_up_id="CHOMP_MOVE",
        ),
        "CHOMP_MOVE": MoveState(
            "CHOMP_MOVE",
            chomp,
            [attack_intent(chomp_intent_damage)],
            follow_up_id="STOMP_MOVE",
        ),
        "STOMP_MOVE": MoveState(
            "STOMP_MOVE",
            stomp,
            [attack_intent(stomp_intent_damage)],
            follow_up_id="CHOMP_MOVE",
        ),
    }
    return creature, MonsterAI(states, "SHRINKER_MOVE")


# ---- FuzzyWurmCrawler (HP 55-57) ----
# Cycle: ACID_GOOP → INHALE → ACID_GOOP → ACID_GOOP → INHALE...

FUZZY_WURM_CRAWLER_BASE_MIN_HP = 55
FUZZY_WURM_CRAWLER_BASE_MAX_HP = 57
FUZZY_WURM_CRAWLER_TOUGH_MIN_HP = 58
FUZZY_WURM_CRAWLER_TOUGH_MAX_HP = 59
FUZZY_WURM_CRAWLER_BASE_ACID_GOOP_DAMAGE = 4
FUZZY_WURM_CRAWLER_DEADLY_ACID_GOOP_DAMAGE = 6
FUZZY_WURM_CRAWLER_INHALE_STRENGTH = 7
FUZZY_WURM_CRAWLER_FIRST_ACID_GOOP_MOVE = "FIRST_ACID_GOOP"
FUZZY_WURM_CRAWLER_ACID_GOOP_MOVE = "ACID_GOOP"
FUZZY_WURM_CRAWLER_INHALE_MOVE = "INHALE"


def create_fuzzy_wurm_crawler(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        FUZZY_WURM_CRAWLER_TOUGH_MIN_HP,
        FUZZY_WURM_CRAWLER_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        FUZZY_WURM_CRAWLER_TOUGH_MAX_HP,
        FUZZY_WURM_CRAWLER_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id="FUZZY_WURM_CRAWLER")

    def acid_goop(combat: CombatState) -> None:
        acid_goop_damage = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            FUZZY_WURM_CRAWLER_DEADLY_ACID_GOOP_DAMAGE,
            FUZZY_WURM_CRAWLER_BASE_ACID_GOOP_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, acid_goop_damage)

    def inhale(combat: CombatState) -> None:
        creature.apply_power(PowerId.STRENGTH, FUZZY_WURM_CRAWLER_INHALE_STRENGTH)

    acid_goop_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        FUZZY_WURM_CRAWLER_DEADLY_ACID_GOOP_DAMAGE,
        FUZZY_WURM_CRAWLER_BASE_ACID_GOOP_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        FUZZY_WURM_CRAWLER_FIRST_ACID_GOOP_MOVE: MoveState(
            FUZZY_WURM_CRAWLER_FIRST_ACID_GOOP_MOVE,
            acid_goop,
            [attack_intent(acid_goop_intent_damage)],
            follow_up_id=FUZZY_WURM_CRAWLER_INHALE_MOVE,
        ),
        FUZZY_WURM_CRAWLER_INHALE_MOVE: MoveState(
            FUZZY_WURM_CRAWLER_INHALE_MOVE,
            inhale,
            [buff_intent()],
            follow_up_id=FUZZY_WURM_CRAWLER_ACID_GOOP_MOVE,
        ),
        FUZZY_WURM_CRAWLER_ACID_GOOP_MOVE: MoveState(
            FUZZY_WURM_CRAWLER_ACID_GOOP_MOVE,
            acid_goop,
            [attack_intent(acid_goop_intent_damage)],
            follow_up_id=FUZZY_WURM_CRAWLER_FIRST_ACID_GOOP_MOVE,
        ),
    }
    return creature, MonsterAI(states, FUZZY_WURM_CRAWLER_FIRST_ACID_GOOP_MOVE)


# ---- Nibbit (HP 42-46) ----
# Conditional start based on IsAlone/IsFront, then fixed rotation:
# BUTT_MOVE(12/13) → SLICE_MOVE(6/7 + 5/6 blk) → HISS_MOVE(Str+2/3) → BUTT_MOVE...

NIBBIT_BASE_MIN_HP = 42
NIBBIT_BASE_MAX_HP = 46
NIBBIT_TOUGH_MIN_HP = 44
NIBBIT_TOUGH_MAX_HP = 48
NIBBIT_BASE_BUTT_DAMAGE = 12
NIBBIT_DEADLY_BUTT_DAMAGE = 13
NIBBIT_BASE_SLICE_DAMAGE = 6
NIBBIT_DEADLY_SLICE_DAMAGE = 7
NIBBIT_BASE_SLICE_BLOCK = 5
NIBBIT_TOUGH_SLICE_BLOCK = 6
NIBBIT_BASE_HISS_STRENGTH = 2
NIBBIT_DEADLY_HISS_STRENGTH = 3
NIBBIT_BUTT_MOVE = "BUTT_MOVE"
NIBBIT_SLICE_MOVE = "SLICE_MOVE"
NIBBIT_HISS_MOVE = "HISS_MOVE"
NIBBIT_INITIAL_MOVE = "INITIAL"


def create_nibbit(
    rng: Rng,
    is_alone: bool = True,
    is_front: bool = False,
    ascension_level: int = 0,
) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        NIBBIT_TOUGH_MIN_HP,
        NIBBIT_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        NIBBIT_TOUGH_MAX_HP,
        NIBBIT_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id="NIBBIT")

    def butt(combat: CombatState) -> None:
        butt_damage = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            NIBBIT_DEADLY_BUTT_DAMAGE,
            NIBBIT_BASE_BUTT_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, butt_damage)

    def slice_move(combat: CombatState) -> None:
        slice_damage = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            NIBBIT_DEADLY_SLICE_DAMAGE,
            NIBBIT_BASE_SLICE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, slice_damage)
        slice_block = _ascension_value(
            _combat_ascension_level(combat),
            TOUGH_ENEMIES_ASCENSION_LEVEL,
            NIBBIT_TOUGH_SLICE_BLOCK,
            NIBBIT_BASE_SLICE_BLOCK,
        )
        _gain_block(creature, slice_block, combat)

    def hiss(combat: CombatState) -> None:
        hiss_strength = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            NIBBIT_DEADLY_HISS_STRENGTH,
            NIBBIT_BASE_HISS_STRENGTH,
        )
        creature.apply_power(PowerId.STRENGTH, hiss_strength)

    butt_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        NIBBIT_DEADLY_BUTT_DAMAGE,
        NIBBIT_BASE_BUTT_DAMAGE,
    )
    slice_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        NIBBIT_DEADLY_SLICE_DAMAGE,
        NIBBIT_BASE_SLICE_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        NIBBIT_BUTT_MOVE: MoveState(
            NIBBIT_BUTT_MOVE,
            butt,
            [attack_intent(butt_intent_damage)],
            follow_up_id=NIBBIT_SLICE_MOVE,
        ),
        NIBBIT_SLICE_MOVE: MoveState(
            NIBBIT_SLICE_MOVE,
            slice_move,
            [attack_intent(slice_intent_damage)],
            follow_up_id=NIBBIT_HISS_MOVE,
        ),
        NIBBIT_HISS_MOVE: MoveState(
            NIBBIT_HISS_MOVE,
            hiss,
            [buff_intent()],
            follow_up_id=NIBBIT_BUTT_MOVE,
        ),
    }

    # Determine starting state
    if is_alone:
        initial = NIBBIT_BUTT_MOVE
    elif is_front:
        initial = NIBBIT_SLICE_MOVE
    else:
        initial = NIBBIT_HISS_MOVE

    cond = ConditionalBranchState(NIBBIT_INITIAL_MOVE)
    cond.add_branch(lambda: is_alone, NIBBIT_BUTT_MOVE)
    cond.add_branch(lambda: is_front, NIBBIT_SLICE_MOVE)
    cond.add_branch(lambda: True, NIBBIT_HISS_MOVE)
    states[NIBBIT_INITIAL_MOVE] = cond

    return creature, MonsterAI(states, initial, rng)


# ---- LeafSlimeS (HP 11-15) ----
# Random: BUTT_MOVE(3) or GOOP_MOVE(add Slimed), CannotRepeat

LEAF_SLIME_S_BASE_MIN_HP = 11
LEAF_SLIME_S_BASE_MAX_HP = 15
LEAF_SLIME_S_TOUGH_MIN_HP = 12
LEAF_SLIME_S_TOUGH_MAX_HP = 16
LEAF_SLIME_S_BASE_TACKLE_DAMAGE = 3
LEAF_SLIME_S_DEADLY_TACKLE_DAMAGE = 4
LEAF_SLIME_S_GOOP_AMOUNT = 1
LEAF_SLIME_S_RANDOM_MOVE = "RANDOM"
LEAF_SLIME_S_BUTT_MOVE = "BUTT_MOVE"
LEAF_SLIME_S_GOOP_MOVE = "GOOP_MOVE"


def create_leaf_slime_s(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        LEAF_SLIME_S_TOUGH_MIN_HP,
        LEAF_SLIME_S_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        LEAF_SLIME_S_TOUGH_MAX_HP,
        LEAF_SLIME_S_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id="LEAF_SLIME_S")

    def butt(combat: CombatState) -> None:
        tackle_damage = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            LEAF_SLIME_S_DEADLY_TACKLE_DAMAGE,
            LEAF_SLIME_S_BASE_TACKLE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, tackle_damage)

    def goop(combat: CombatState) -> None:
        add_generated_cards_to_living_player_discards(combat, make_slimed, LEAF_SLIME_S_GOOP_AMOUNT)

    tackle_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        LEAF_SLIME_S_DEADLY_TACKLE_DAMAGE,
        LEAF_SLIME_S_BASE_TACKLE_DAMAGE,
    )

    rand = RandomBranchState(LEAF_SLIME_S_RANDOM_MOVE)
    rand.add_branch(LEAF_SLIME_S_BUTT_MOVE, MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch(LEAF_SLIME_S_GOOP_MOVE, MoveRepeatType.CANNOT_REPEAT)

    states: dict[str, MonsterState] = {
        LEAF_SLIME_S_RANDOM_MOVE: rand,
        LEAF_SLIME_S_BUTT_MOVE: MoveState(
            LEAF_SLIME_S_BUTT_MOVE,
            butt,
            [attack_intent(tackle_intent_damage)],
            follow_up_id=LEAF_SLIME_S_RANDOM_MOVE,
        ),
        LEAF_SLIME_S_GOOP_MOVE: MoveState(
            LEAF_SLIME_S_GOOP_MOVE,
            goop,
            [status_intent()],
            follow_up_id=LEAF_SLIME_S_RANDOM_MOVE,
        ),
    }
    return creature, MonsterAI(states, LEAF_SLIME_S_RANDOM_MOVE, rng)


# ---- TwigSlimeS (HP 7-11) ----
# BUTT_MOVE(4) → BUTT_MOVE(4) (self loop)

TWIG_SLIME_S_BASE_MIN_HP = 7
TWIG_SLIME_S_BASE_MAX_HP = 11
TWIG_SLIME_S_TOUGH_MIN_HP = 8
TWIG_SLIME_S_TOUGH_MAX_HP = 12
TWIG_SLIME_S_BASE_TACKLE_DAMAGE = 4
TWIG_SLIME_S_DEADLY_TACKLE_DAMAGE = 5
TWIG_SLIME_S_BUTT_MOVE = "BUTT_MOVE"


def create_twig_slime_s(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        TWIG_SLIME_S_TOUGH_MIN_HP,
        TWIG_SLIME_S_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        TWIG_SLIME_S_TOUGH_MAX_HP,
        TWIG_SLIME_S_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id="TWIG_SLIME_S")

    def butt(combat: CombatState) -> None:
        tackle_damage = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            TWIG_SLIME_S_DEADLY_TACKLE_DAMAGE,
            TWIG_SLIME_S_BASE_TACKLE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, tackle_damage)

    tackle_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        TWIG_SLIME_S_DEADLY_TACKLE_DAMAGE,
        TWIG_SLIME_S_BASE_TACKLE_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        TWIG_SLIME_S_BUTT_MOVE: MoveState(
            TWIG_SLIME_S_BUTT_MOVE,
            butt,
            [attack_intent(tackle_intent_damage)],
            follow_up_id=TWIG_SLIME_S_BUTT_MOVE,
        ),
    }
    return creature, MonsterAI(states, TWIG_SLIME_S_BUTT_MOVE)


# ---- LeafSlimeM (HP 32-35) ----
# Strict alternation: STICKY_SHOT(add 2 Slimed) → CLUMP_SHOT(8) → STICKY_SHOT...

LEAF_SLIME_M_BASE_MIN_HP = 32
LEAF_SLIME_M_BASE_MAX_HP = 35
LEAF_SLIME_M_TOUGH_MIN_HP = 33
LEAF_SLIME_M_TOUGH_MAX_HP = 36
LEAF_SLIME_M_BASE_CLUMP_DAMAGE = 8
LEAF_SLIME_M_DEADLY_CLUMP_DAMAGE = 9
LEAF_SLIME_M_STICKY_AMOUNT = 2
LEAF_SLIME_M_STICKY_SHOT_MOVE = "STICKY_SHOT"
LEAF_SLIME_M_CLUMP_SHOT_MOVE = "CLUMP_SHOT"


def create_leaf_slime_m(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        LEAF_SLIME_M_TOUGH_MIN_HP,
        LEAF_SLIME_M_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        LEAF_SLIME_M_TOUGH_MAX_HP,
        LEAF_SLIME_M_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id="LEAF_SLIME_M")

    def sticky_shot(combat: CombatState) -> None:
        add_generated_cards_to_living_player_discards(combat, make_slimed, LEAF_SLIME_M_STICKY_AMOUNT)

    def clump_shot(combat: CombatState) -> None:
        clump_damage = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            LEAF_SLIME_M_DEADLY_CLUMP_DAMAGE,
            LEAF_SLIME_M_BASE_CLUMP_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, clump_damage)

    clump_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        LEAF_SLIME_M_DEADLY_CLUMP_DAMAGE,
        LEAF_SLIME_M_BASE_CLUMP_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        LEAF_SLIME_M_STICKY_SHOT_MOVE: MoveState(
            LEAF_SLIME_M_STICKY_SHOT_MOVE,
            sticky_shot,
            [status_intent()],
            follow_up_id=LEAF_SLIME_M_CLUMP_SHOT_MOVE,
        ),
        LEAF_SLIME_M_CLUMP_SHOT_MOVE: MoveState(
            LEAF_SLIME_M_CLUMP_SHOT_MOVE,
            clump_shot,
            [attack_intent(clump_intent_damage)],
            follow_up_id=LEAF_SLIME_M_STICKY_SHOT_MOVE,
        ),
    }
    return creature, MonsterAI(states, LEAF_SLIME_M_STICKY_SHOT_MOVE)


# ---- TwigSlimeM (HP 26-28) ----
# STICKY_SHOT_MOVE → Random(CLUMP_SHOT_MOVE[max 2 consec] | STICKY_SHOT_MOVE[CannotRepeat])

TWIG_SLIME_M_BASE_MIN_HP = 26
TWIG_SLIME_M_BASE_MAX_HP = 28
TWIG_SLIME_M_TOUGH_MIN_HP = 27
TWIG_SLIME_M_TOUGH_MAX_HP = 29
TWIG_SLIME_M_BASE_CLUMP_DAMAGE = 11
TWIG_SLIME_M_DEADLY_CLUMP_DAMAGE = 12
TWIG_SLIME_M_STICKY_AMOUNT = 1
TWIG_SLIME_M_RANDOM_MOVE = "RANDOM"
TWIG_SLIME_M_STICKY_SHOT_MOVE = "STICKY_SHOT_MOVE"
TWIG_SLIME_M_CLUMP_SHOT_MOVE = "CLUMP_SHOT_MOVE"
TWIG_SLIME_M_MAX_CONSECUTIVE_CLUMP_SHOTS = 2


def create_twig_slime_m(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        TWIG_SLIME_M_TOUGH_MIN_HP,
        TWIG_SLIME_M_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        TWIG_SLIME_M_TOUGH_MAX_HP,
        TWIG_SLIME_M_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id="TWIG_SLIME_M")

    def sticky_shot(combat: CombatState) -> None:
        add_generated_cards_to_living_player_discards(combat, make_slimed, TWIG_SLIME_M_STICKY_AMOUNT)

    def clump_shot(combat: CombatState) -> None:
        clump_damage = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            TWIG_SLIME_M_DEADLY_CLUMP_DAMAGE,
            TWIG_SLIME_M_BASE_CLUMP_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, clump_damage)

    clump_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        TWIG_SLIME_M_DEADLY_CLUMP_DAMAGE,
        TWIG_SLIME_M_BASE_CLUMP_DAMAGE,
    )

    rand = RandomBranchState(TWIG_SLIME_M_RANDOM_MOVE)
    rand.add_branch(
        TWIG_SLIME_M_CLUMP_SHOT_MOVE,
        MoveRepeatType.CAN_REPEAT_X_TIMES,
        max_times=TWIG_SLIME_M_MAX_CONSECUTIVE_CLUMP_SHOTS,
    )
    rand.add_branch(TWIG_SLIME_M_STICKY_SHOT_MOVE, MoveRepeatType.CANNOT_REPEAT)

    states: dict[str, MonsterState] = {
        TWIG_SLIME_M_RANDOM_MOVE: rand,
        TWIG_SLIME_M_STICKY_SHOT_MOVE: MoveState(
            TWIG_SLIME_M_STICKY_SHOT_MOVE,
            sticky_shot,
            [status_intent()],
            follow_up_id=TWIG_SLIME_M_RANDOM_MOVE,
        ),
        TWIG_SLIME_M_CLUMP_SHOT_MOVE: MoveState(
            TWIG_SLIME_M_CLUMP_SHOT_MOVE,
            clump_shot,
            [attack_intent(clump_intent_damage)],
            follow_up_id=TWIG_SLIME_M_RANDOM_MOVE,
        ),
    }
    return creature, MonsterAI(states, TWIG_SLIME_M_STICKY_SHOT_MOVE)
