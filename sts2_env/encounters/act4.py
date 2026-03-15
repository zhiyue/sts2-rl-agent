"""Act 4 (Underdocks) encounter definitions: weak, normal, elite, boss."""

from __future__ import annotations

from typing import Callable, TYPE_CHECKING

from sts2_env.core.rng import Rng

if TYPE_CHECKING:
    from sts2_env.core.combat import CombatState
from sts2_env.monsters.act4 import (
    create_corpse_slug,
    create_seapunk,
    create_sludge_spinner,
    create_toadpole,
    create_calcified_cultist,
    create_damp_cultist,
    create_fossil_stalker,
    create_gremlin_merc,
    create_sneaky_gremlin,
    create_fat_gremlin,
    create_haunted_ship,
    create_living_fog,
    create_punch_construct,
    create_sewer_clam,
    create_two_tailed_rat,
    create_phantasmal_gardener,
    create_skulking_colony,
    create_terror_eel,
    create_waterfall_giant,
    create_soul_fysh,
    create_lagavulin_matriarch,
)

EncounterSetup = Callable[..., None]


# ---- Weak Encounters ----

def setup_corpse_slugs_weak(combat: CombatState, rng: Rng) -> None:
    c1, a1 = create_corpse_slug(rng, starter_idx=0)
    combat.add_enemy(c1, a1)
    c2, a2 = create_corpse_slug(rng, starter_idx=1)
    combat.add_enemy(c2, a2)


def setup_seapunk_weak(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_seapunk(rng)
    combat.add_enemy(creature, ai)


def setup_sludge_spinner_weak(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_sludge_spinner(rng)
    combat.add_enemy(creature, ai)


def setup_toadpoles_weak(combat: CombatState, rng: Rng) -> None:
    c1, a1 = create_toadpole(rng, slot="first")
    combat.add_enemy(c1, a1)
    c2, a2 = create_toadpole(rng, slot="second")
    combat.add_enemy(c2, a2)


WEAK_ENCOUNTERS: list[EncounterSetup] = [
    setup_corpse_slugs_weak,
    setup_seapunk_weak,
    setup_sludge_spinner_weak,
    setup_toadpoles_weak,
]


# ---- Normal Encounters ----

def setup_corpse_slugs_normal(combat: CombatState, rng: Rng) -> None:
    for i in range(3):
        creature, ai = create_corpse_slug(rng, starter_idx=i)
        combat.add_enemy(creature, ai)


def setup_cultists_normal(combat: CombatState, rng: Rng) -> None:
    c1, a1 = create_calcified_cultist(rng)
    combat.add_enemy(c1, a1)
    c2, a2 = create_damp_cultist(rng)
    combat.add_enemy(c2, a2)


def setup_fossil_stalker_normal(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_fossil_stalker(rng)
    combat.add_enemy(creature, ai)


def setup_gremlin_merc_normal(combat: CombatState, rng: Rng) -> None:
    merc, merc_ai = create_gremlin_merc(rng)
    combat.add_enemy(merc, merc_ai)
    # Spawn minions
    sg, sg_ai = create_sneaky_gremlin(rng)
    combat.add_enemy(sg, sg_ai)
    fg, fg_ai = create_fat_gremlin(rng)
    combat.add_enemy(fg, fg_ai)


def setup_haunted_ship_normal(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_haunted_ship(rng)
    combat.add_enemy(creature, ai)


def setup_living_fog_normal(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_living_fog(rng)
    combat.add_enemy(creature, ai)


def setup_punch_construct_normal(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_punch_construct(rng)
    combat.add_enemy(creature, ai)


def setup_sewer_clam_normal(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_sewer_clam(rng)
    combat.add_enemy(creature, ai)


def setup_toadpoles_normal(combat: CombatState, rng: Rng) -> None:
    for slot in ["first", "second"]:
        creature, ai = create_toadpole(rng, slot=slot)
        combat.add_enemy(creature, ai)
    c3, a3 = create_toadpole(rng, slot="first")
    combat.add_enemy(c3, a3)


def setup_two_tailed_rats_normal(combat: CombatState, rng: Rng) -> None:
    c1, a1 = create_two_tailed_rat(rng, slot="first")
    combat.add_enemy(c1, a1)
    c2, a2 = create_two_tailed_rat(rng, slot="second")
    combat.add_enemy(c2, a2)


NORMAL_ENCOUNTERS: list[EncounterSetup] = [
    setup_corpse_slugs_normal,
    setup_cultists_normal,
    setup_fossil_stalker_normal,
    setup_gremlin_merc_normal,
    setup_haunted_ship_normal,
    setup_living_fog_normal,
    setup_punch_construct_normal,
    setup_sewer_clam_normal,
    setup_toadpoles_normal,
    setup_two_tailed_rats_normal,
]


# ---- Elite Encounters ----

def setup_phantasmal_gardeners_elite(combat: CombatState, rng: Rng) -> None:
    for _ in range(2):
        creature, ai = create_phantasmal_gardener(rng)
        combat.add_enemy(creature, ai)


def setup_skulking_colony_elite(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_skulking_colony(rng)
    combat.add_enemy(creature, ai)


def setup_terror_eel_elite(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_terror_eel(rng)
    combat.add_enemy(creature, ai)


ELITE_ENCOUNTERS: list[EncounterSetup] = [
    setup_phantasmal_gardeners_elite,
    setup_skulking_colony_elite,
    setup_terror_eel_elite,
]


# ---- Boss Encounters ----

def setup_waterfall_giant_boss(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_waterfall_giant(rng)
    combat.add_enemy(creature, ai)


def setup_soul_fysh_boss(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_soul_fysh(rng)
    combat.add_enemy(creature, ai)


def setup_lagavulin_matriarch_boss(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_lagavulin_matriarch(rng)
    combat.add_enemy(creature, ai)


BOSS_ENCOUNTERS: list[EncounterSetup] = [
    setup_waterfall_giant_boss,
    setup_soul_fysh_boss,
    setup_lagavulin_matriarch_boss,
]


ALL_ACT4_ENCOUNTERS: list[EncounterSetup] = (
    list(WEAK_ENCOUNTERS) +
    list(NORMAL_ENCOUNTERS) +
    list(ELITE_ENCOUNTERS) +
    list(BOSS_ENCOUNTERS)
)
