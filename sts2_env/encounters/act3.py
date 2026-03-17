"""Act 3 (Glory) encounter definitions: weak, normal, elite, boss."""

from __future__ import annotations

from typing import Callable, TYPE_CHECKING

from sts2_env.core.rng import Rng

if TYPE_CHECKING:
    from sts2_env.core.combat import CombatState
from sts2_env.monsters.act3 import (
    create_devoted_sculptor,
    create_scroll_of_biting,
    create_turret_operator,
    create_axebot,
    create_fabricator,
    create_frog_knight,
    create_globe_head,
    create_owl_magistrate,
    create_slimed_berserker,
    create_the_lost,
    create_the_forgotten,
    create_flail_knight,
    create_magi_knight,
    create_spectral_knight,
    create_mecha_knight,
    create_soul_nexus,
    create_door,
    create_doormaker,
    create_queen,
    create_test_subject,
)
from sts2_env.monsters.act1 import create_cubex_construct
from sts2_env.monsters.act4 import create_punch_construct

EncounterSetup = Callable[..., None]


# ---- Weak Encounters ----

def setup_devoted_sculptor_weak(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_devoted_sculptor(rng)
    combat.add_enemy(creature, ai)


def setup_scrolls_of_biting_weak(combat: CombatState, rng: Rng) -> None:
    for _ in range(3):
        creature, ai = create_scroll_of_biting(rng)
        combat.add_enemy(creature, ai)


def setup_turret_operator_weak(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_turret_operator(rng)
    combat.add_enemy(creature, ai)


WEAK_ENCOUNTERS: list[EncounterSetup] = [
    setup_devoted_sculptor_weak,
    setup_scrolls_of_biting_weak,
    setup_turret_operator_weak,
]


# ---- Normal Encounters ----

def setup_axebots_normal(combat: CombatState, rng: Rng) -> None:
    for _ in range(2):
        creature, ai = create_axebot(rng)
        combat.add_enemy(creature, ai)


def setup_construct_menagerie_normal(combat: CombatState, rng: Rng) -> None:
    c1, a1 = create_punch_construct(rng)
    combat.add_enemy(c1, a1)
    c2, a2 = create_cubex_construct(rng)
    combat.add_enemy(c2, a2)
    c3, a3 = create_cubex_construct(rng)
    combat.add_enemy(c3, a3)


def setup_fabricator_normal(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_fabricator(rng)
    combat.add_enemy(creature, ai)


def setup_frog_knight_normal(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_frog_knight(rng)
    combat.add_enemy(creature, ai)


def setup_globe_head_normal(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_globe_head(rng)
    combat.add_enemy(creature, ai)


def setup_owl_magistrate_normal(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_owl_magistrate(rng)
    combat.add_enemy(creature, ai)


def setup_scrolls_of_biting_normal(combat: CombatState, rng: Rng) -> None:
    for _ in range(4):
        creature, ai = create_scroll_of_biting(rng)
        combat.add_enemy(creature, ai)


def setup_slimed_berserker_normal(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_slimed_berserker(rng)
    combat.add_enemy(creature, ai)


def setup_the_lost_and_forgotten_normal(combat: CombatState, rng: Rng) -> None:
    c1, a1 = create_the_lost(rng)
    combat.add_enemy(c1, a1)
    c2, a2 = create_the_forgotten(rng)
    combat.add_enemy(c2, a2)


NORMAL_ENCOUNTERS: list[EncounterSetup] = [
    setup_axebots_normal,
    setup_construct_menagerie_normal,
    setup_fabricator_normal,
    setup_frog_knight_normal,
    setup_globe_head_normal,
    setup_owl_magistrate_normal,
    setup_scrolls_of_biting_normal,
    setup_slimed_berserker_normal,
    setup_the_lost_and_forgotten_normal,
]


# ---- Elite Encounters ----

def setup_knights_elite(combat: CombatState, rng: Rng) -> None:
    knight_creators = [create_flail_knight, create_magi_knight, create_spectral_knight]
    chosen = rng.sample(knight_creators, 2)
    for creator in chosen:
        creature, ai = creator(rng)
        combat.add_enemy(creature, ai)


def setup_mecha_knight_elite(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_mecha_knight(rng)
    combat.add_enemy(creature, ai)


def setup_soul_nexus_elite(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_soul_nexus(rng)
    combat.add_enemy(creature, ai)


ELITE_ENCOUNTERS: list[EncounterSetup] = [
    setup_knights_elite,
    setup_mecha_knight_elite,
    setup_soul_nexus_elite,
]


# ---- Boss Encounters ----

def setup_doormaker_boss(combat: CombatState, rng: Rng) -> None:
    door, door_ai = create_door(rng)
    combat.add_enemy(door, door_ai)
    doormaker, doormaker_ai = create_doormaker(rng)
    combat.add_enemy(doormaker, doormaker_ai)


def setup_queen_boss(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_queen(rng)
    combat.add_enemy(creature, ai)


def setup_test_subject_boss(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_test_subject(rng)
    combat.add_enemy(creature, ai)


BOSS_ENCOUNTERS: list[EncounterSetup] = [
    setup_doormaker_boss,
    setup_queen_boss,
    setup_test_subject_boss,
]


ALL_ACT3_ENCOUNTERS: list[EncounterSetup] = (
    list(WEAK_ENCOUNTERS) +
    list(NORMAL_ENCOUNTERS) +
    list(ELITE_ENCOUNTERS) +
    list(BOSS_ENCOUNTERS)
)
