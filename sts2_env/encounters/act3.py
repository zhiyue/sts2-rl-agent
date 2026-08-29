"""Act 3 (Glory) encounter definitions: weak, normal, elite, boss."""

from __future__ import annotations

from typing import Callable, TYPE_CHECKING

from sts2_env.core.enums import PowerId
from sts2_env.core.rng import Rng

if TYPE_CHECKING:
    from sts2_env.core.combat import CombatState
from sts2_env.monsters.act3 import (
    create_devoted_sculptor,
    create_living_shield,
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
    create_aeonglass,
    create_queen,
    create_test_subject,
)
from sts2_env.monsters.act1 import apply_cubex_construct_room_setup, create_cubex_construct
from sts2_env.monsters.act4 import create_punch_construct
from sts2_env.monsters.shared import create_torch_head_amalgam

EncounterSetup = Callable[..., None]


# ---- Weak Encounters ----

def setup_devoted_sculptor_weak(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_devoted_sculptor(rng, ascension_level=getattr(combat, "ascension_level", 0))
    combat.add_enemy(creature, ai)


def setup_scrolls_of_biting_weak(combat: CombatState, rng: Rng) -> None:
    starter_move_idx = rng.next_int(0, 2)
    for offset in range(3):
        creature, ai = create_scroll_of_biting(rng, (starter_move_idx + offset) % 3)
        combat.add_enemy(creature, ai)


def setup_turret_operator_weak(combat: CombatState, rng: Rng) -> None:
    shield, shield_ai = create_living_shield(rng)
    combat.add_enemy(shield, shield_ai)
    creature, ai = create_turret_operator(rng, ascension_level=getattr(combat, "ascension_level", 0))
    combat.add_enemy(creature, ai)


WEAK_ENCOUNTERS: list[EncounterSetup] = [
    setup_devoted_sculptor_weak,
    setup_scrolls_of_biting_weak,
    setup_turret_operator_weak,
]


# ---- Normal Encounters ----

def setup_axebots_normal(combat: CombatState, rng: Rng) -> None:
    for _ in range(2):
        creature, ai = create_axebot(rng, ascension_level=getattr(combat, "ascension_level", 0))
        combat.add_enemy(creature, ai)


def setup_construct_menagerie_normal(combat: CombatState, rng: Rng) -> None:
    c1, a1 = create_punch_construct(rng)
    combat.add_enemy(c1, a1)
    c2, a2 = create_cubex_construct(rng, ascension_level=combat.ascension_level)
    combat.add_enemy(c2, a2)
    apply_cubex_construct_room_setup(c2, combat)
    c3, a3 = create_cubex_construct(rng, ascension_level=combat.ascension_level)
    combat.add_enemy(c3, a3)
    apply_cubex_construct_room_setup(c3, combat)


def setup_fabricator_normal(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_fabricator(rng, ascension_level=getattr(combat, "ascension_level", 0))
    combat.add_enemy(creature, ai)


def setup_frog_knight_normal(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_frog_knight(rng, ascension_level=getattr(combat, "ascension_level", 0))
    combat.add_enemy(creature, ai)


def setup_globe_head_normal(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_globe_head(rng, ascension_level=getattr(combat, "ascension_level", 0))
    combat.add_enemy(creature, ai)


def setup_owl_magistrate_normal(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_owl_magistrate(rng, ascension_level=getattr(combat, "ascension_level", 0))
    combat.add_enemy(creature, ai)


def setup_scrolls_of_biting_normal(combat: CombatState, rng: Rng) -> None:
    starter_move_idx = rng.next_int(0, 2)
    for offset in range(3):
        creature, ai = create_scroll_of_biting(rng, (starter_move_idx + offset) % 3)
        combat.add_enemy(creature, ai)
    creature, ai = create_scroll_of_biting(rng, 2)
    combat.add_enemy(creature, ai)


def setup_slimed_berserker_normal(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_slimed_berserker(rng, ascension_level=getattr(combat, "ascension_level", 0))
    combat.add_enemy(creature, ai)


def setup_the_lost_and_forgotten_normal(combat: CombatState, rng: Rng) -> None:
    ascension_level = getattr(combat, "ascension_level", 0)
    c1, a1 = create_the_lost(rng, ascension_level=ascension_level)
    combat.add_enemy(c1, a1)
    c2, a2 = create_the_forgotten(rng, ascension_level=ascension_level)
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
    ascension_level = getattr(combat, "ascension_level", 0)
    for creator in (create_flail_knight, create_spectral_knight, create_magi_knight):
        creature, ai = creator(rng, ascension_level=ascension_level)
        combat.add_enemy(creature, ai)


def setup_mecha_knight_elite(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_mecha_knight(rng, ascension_level=getattr(combat, "ascension_level", 0))
    combat.add_enemy(creature, ai)


def setup_soul_nexus_elite(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_soul_nexus(rng, ascension_level=getattr(combat, "ascension_level", 0))
    combat.add_enemy(creature, ai)


ELITE_ENCOUNTERS: list[EncounterSetup] = [
    setup_knights_elite,
    setup_mecha_knight_elite,
    setup_soul_nexus_elite,
]


# ---- Boss Encounters ----

def setup_aeonglass_boss(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_aeonglass(rng, ascension_level=getattr(combat, "ascension_level", 0))
    combat.add_enemy(creature, ai)
    presence = creature.powers.get(PowerId.WITHERING_PRESENCE)
    if presence is not None:
        presence.target_player = combat.primary_player


def setup_queen_boss(combat: CombatState, rng: Rng) -> None:
    ascension_level = getattr(combat, "ascension_level", 0)
    amalgam, amalgam_ai = create_torch_head_amalgam(rng, ascension_level=ascension_level)
    combat.add_enemy(amalgam, amalgam_ai)
    creature, ai = create_queen(rng, ascension_level=ascension_level)
    combat.add_enemy(creature, ai)


def setup_test_subject_boss(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_test_subject(rng, ascension_level=getattr(combat, "ascension_level", 0))
    combat.add_enemy(creature, ai)


BOSS_ENCOUNTERS: list[EncounterSetup] = [
    setup_queen_boss,
    setup_test_subject_boss,
    setup_aeonglass_boss,
]


ALL_ACT3_ENCOUNTERS: list[EncounterSetup] = [
    setup_axebots_normal,
    setup_construct_menagerie_normal,
    setup_devoted_sculptor_weak,
    setup_aeonglass_boss,
    setup_fabricator_normal,
    setup_frog_knight_normal,
    setup_globe_head_normal,
    setup_knights_elite,
    setup_mecha_knight_elite,
    setup_owl_magistrate_normal,
    setup_queen_boss,
    setup_scrolls_of_biting_normal,
    setup_scrolls_of_biting_weak,
    setup_slimed_berserker_normal,
    setup_soul_nexus_elite,
    setup_test_subject_boss,
    setup_the_lost_and_forgotten_normal,
    setup_turret_operator_weak,
]
