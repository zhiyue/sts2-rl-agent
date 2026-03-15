"""Act 2 (Hive) encounter definitions: weak, normal, elite, boss."""

from __future__ import annotations

from typing import Callable, TYPE_CHECKING

from sts2_env.core.rng import Rng

if TYPE_CHECKING:
    from sts2_env.core.combat import CombatState
from sts2_env.monsters.act2 import (
    create_thieving_hopper,
    create_tunneler,
    create_bowlbug_egg,
    create_bowlbug_nectar,
    create_bowlbug_rock,
    create_bowlbug_silk,
    create_exoskeleton,
    create_chomper,
    create_hunter_killer,
    create_louse_progenitor,
    create_myte,
    create_ovicopter,
    create_slumbering_beetle,
    create_spiny_toad,
    create_the_obscura,
    create_decimillipede_segment,
    create_entomancer,
    create_infested_prism,
    create_the_insatiable,
    create_knowledge_demon,
    create_crusher,
    create_rocket,
)

EncounterSetup = Callable[..., None]


# ---- Weak Encounters ----

def setup_thieving_hopper_weak(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_thieving_hopper(rng)
    combat.add_enemy(creature, ai)


def setup_tunneler_weak(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_tunneler(rng)
    combat.add_enemy(creature, ai)


def setup_bowlbugs_weak(combat: CombatState, rng: Rng) -> None:
    creators = [create_bowlbug_egg, create_bowlbug_nectar]
    for creator in creators:
        creature, ai = creator(rng)
        combat.add_enemy(creature, ai)


def setup_exoskeletons_weak(combat: CombatState, rng: Rng) -> None:
    c1, a1 = create_exoskeleton(rng, slot="first")
    combat.add_enemy(c1, a1)
    c2, a2 = create_exoskeleton(rng, slot="second")
    combat.add_enemy(c2, a2)


WEAK_ENCOUNTERS: list[EncounterSetup] = [
    setup_thieving_hopper_weak,
    setup_tunneler_weak,
    setup_bowlbugs_weak,
    setup_exoskeletons_weak,
]


# ---- Normal Encounters ----

def setup_bowlbugs_normal(combat: CombatState, rng: Rng) -> None:
    creators = [create_bowlbug_egg, create_bowlbug_rock, create_bowlbug_silk]
    for creator in creators:
        creature, ai = creator(rng)
        combat.add_enemy(creature, ai)


def setup_chompers_normal(combat: CombatState, rng: Rng) -> None:
    c1, a1 = create_chomper(rng, scream_first=False)
    combat.add_enemy(c1, a1)
    c2, a2 = create_chomper(rng, scream_first=True)
    combat.add_enemy(c2, a2)


def setup_exoskeletons_normal(combat: CombatState, rng: Rng) -> None:
    for i, slot in enumerate(["first", "second", "third"]):
        creature, ai = create_exoskeleton(rng, slot=slot)
        combat.add_enemy(creature, ai)


def setup_hunter_killer_normal(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_hunter_killer(rng)
    combat.add_enemy(creature, ai)


def setup_louse_progenitor_normal(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_louse_progenitor(rng)
    combat.add_enemy(creature, ai)


def setup_mytes_normal(combat: CombatState, rng: Rng) -> None:
    for _ in range(3):
        creature, ai = create_myte(rng)
        combat.add_enemy(creature, ai)


def setup_ovicopter_normal(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_ovicopter(rng)
    combat.add_enemy(creature, ai)


def setup_slumbering_beetle_normal(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_slumbering_beetle(rng)
    combat.add_enemy(creature, ai)


def setup_spiny_toad_normal(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_spiny_toad(rng)
    combat.add_enemy(creature, ai)


def setup_the_obscura_normal(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_the_obscura(rng)
    combat.add_enemy(creature, ai)


def setup_tunneler_normal(combat: CombatState, rng: Rng) -> None:
    for _ in range(2):
        creature, ai = create_tunneler(rng)
        combat.add_enemy(creature, ai)


NORMAL_ENCOUNTERS: list[EncounterSetup] = [
    setup_bowlbugs_normal,
    setup_chompers_normal,
    setup_exoskeletons_normal,
    setup_hunter_killer_normal,
    setup_louse_progenitor_normal,
    setup_mytes_normal,
    setup_ovicopter_normal,
    setup_slumbering_beetle_normal,
    setup_spiny_toad_normal,
    setup_the_obscura_normal,
    setup_tunneler_normal,
]


# ---- Elite Encounters ----

def setup_decimillipede_elite(combat: CombatState, rng: Rng) -> None:
    for i in range(3):
        creature, ai = create_decimillipede_segment(rng, starter_idx=i)
        combat.add_enemy(creature, ai)


def setup_entomancer_elite(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_entomancer(rng)
    combat.add_enemy(creature, ai)


def setup_infested_prisms_elite(combat: CombatState, rng: Rng) -> None:
    c1, a1 = create_infested_prism(rng, slot="first")
    combat.add_enemy(c1, a1)
    c2, a2 = create_infested_prism(rng, slot="second")
    combat.add_enemy(c2, a2)


ELITE_ENCOUNTERS: list[EncounterSetup] = [
    setup_decimillipede_elite,
    setup_entomancer_elite,
    setup_infested_prisms_elite,
]


# ---- Boss Encounters ----

def setup_the_insatiable_boss(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_the_insatiable(rng)
    combat.add_enemy(creature, ai)


def setup_knowledge_demon_boss(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_knowledge_demon(rng)
    combat.add_enemy(creature, ai)


def setup_kaiser_crab_boss(combat: CombatState, rng: Rng) -> None:
    crusher, crusher_ai = create_crusher(rng)
    combat.add_enemy(crusher, crusher_ai)
    rocket, rocket_ai = create_rocket(rng)
    combat.add_enemy(rocket, rocket_ai)


BOSS_ENCOUNTERS: list[EncounterSetup] = [
    setup_the_insatiable_boss,
    setup_knowledge_demon_boss,
    setup_kaiser_crab_boss,
]


ALL_ACT2_ENCOUNTERS: list[EncounterSetup] = (
    list(WEAK_ENCOUNTERS) +
    list(NORMAL_ENCOUNTERS) +
    list(ELITE_ENCOUNTERS) +
    list(BOSS_ENCOUNTERS)
)
