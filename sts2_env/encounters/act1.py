"""Act 1 (Overgrowth) encounter definitions: weak, normal, elite, boss."""

from __future__ import annotations

from typing import Callable, TYPE_CHECKING

from sts2_env.core.rng import Rng

if TYPE_CHECKING:
    from sts2_env.core.combat import CombatState
from sts2_env.monsters.act1_weak import (
    create_shrinker_beetle,
    create_fuzzy_wurm_crawler,
    create_nibbit,
    create_leaf_slime_s,
    create_twig_slime_s,
    create_leaf_slime_m,
    create_twig_slime_m,
)
from sts2_env.monsters.act1 import (
    create_cubex_construct,
    create_flyconid,
    create_fogmog,
    create_inklet,
    create_mawler,
    create_vine_shambler,
    create_slithering_strangler,
    create_snapping_jaxfruit,
    create_assassin_ruby_raider,
    create_axe_ruby_raider,
    create_brute_ruby_raider,
    create_crossbow_ruby_raider,
    create_tracker_ruby_raider,
    create_bygone_effigy,
    create_byrdonis,
    create_phrog_parasite,
    create_vantom,
    create_ceremonial_beast,
    create_kin_priest,
    create_kin_follower,
)

EncounterSetup = Callable[..., None]


# ---- Weak Encounters ----

def setup_shrinker_beetle_weak(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_shrinker_beetle(rng)
    combat.add_enemy(creature, ai)


def setup_fuzzy_wurm_crawler_weak(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_fuzzy_wurm_crawler(rng)
    combat.add_enemy(creature, ai)


def setup_nibbits_weak(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_nibbit(rng, is_alone=True)
    combat.add_enemy(creature, ai)


def setup_slimes_weak(combat: CombatState, rng: Rng) -> None:
    small_creators = [create_leaf_slime_s, create_twig_slime_s]
    medium_creators = [create_leaf_slime_m, create_twig_slime_m]
    chosen_small = rng.sample(small_creators, 2)
    for creator in chosen_small:
        creature, ai = creator(rng)
        combat.add_enemy(creature, ai)
    creator = rng.choice(medium_creators)
    creature, ai = creator(rng)
    combat.add_enemy(creature, ai)


WEAK_ENCOUNTERS: list[EncounterSetup] = [
    setup_shrinker_beetle_weak,
    setup_fuzzy_wurm_crawler_weak,
    setup_nibbits_weak,
    setup_slimes_weak,
]


# ---- Normal Encounters ----

def setup_cubex_construct_normal(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_cubex_construct(rng)
    combat.add_enemy(creature, ai)


def setup_flyconid_normal(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_flyconid(rng)
    combat.add_enemy(creature, ai)


def setup_fogmog_normal(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_fogmog(rng)
    combat.add_enemy(creature, ai)


def setup_inklets_normal(combat: CombatState, rng: Rng) -> None:
    c1, a1 = create_inklet(rng, slot="first")
    combat.add_enemy(c1, a1)
    c2, a2 = create_inklet(rng, slot="second")
    combat.add_enemy(c2, a2)


def setup_mawler_normal(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_mawler(rng)
    combat.add_enemy(creature, ai)


def setup_nibbits_normal(combat: CombatState, rng: Rng) -> None:
    c1, a1 = create_nibbit(rng, is_alone=False, is_front=True)
    combat.add_enemy(c1, a1)
    c2, a2 = create_nibbit(rng, is_alone=False, is_front=False)
    combat.add_enemy(c2, a2)


def setup_overgrowth_crawlers(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_fuzzy_wurm_crawler(rng)
    combat.add_enemy(creature, ai)
    c2, a2 = create_nibbit(rng, is_alone=False, is_front=True)
    combat.add_enemy(c2, a2)


def setup_ruby_raiders_normal(combat: CombatState, rng: Rng) -> None:
    raider_creators = [
        create_assassin_ruby_raider,
        create_axe_ruby_raider,
        create_brute_ruby_raider,
        create_crossbow_ruby_raider,
        create_tracker_ruby_raider,
    ]
    chosen = rng.sample(raider_creators, 3)
    for creator in chosen:
        creature, ai = creator(rng)
        combat.add_enemy(creature, ai)


def setup_slimes_normal(combat: CombatState, rng: Rng) -> None:
    medium_creators = [create_leaf_slime_m, create_twig_slime_m]
    for creator in medium_creators:
        creature, ai = creator(rng)
        combat.add_enemy(creature, ai)


def setup_slithering_strangler_normal(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_slithering_strangler(rng)
    combat.add_enemy(creature, ai)


def setup_snapping_jaxfruit_normal(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_snapping_jaxfruit(rng)
    combat.add_enemy(creature, ai)


def setup_vine_shambler_normal(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_vine_shambler(rng)
    combat.add_enemy(creature, ai)


NORMAL_ENCOUNTERS: list[EncounterSetup] = [
    setup_cubex_construct_normal,
    setup_flyconid_normal,
    setup_fogmog_normal,
    setup_inklets_normal,
    setup_mawler_normal,
    setup_nibbits_normal,
    setup_overgrowth_crawlers,
    setup_ruby_raiders_normal,
    setup_slimes_normal,
    setup_slithering_strangler_normal,
    setup_snapping_jaxfruit_normal,
    setup_vine_shambler_normal,
]


# ---- Elite Encounters ----

def setup_bygone_effigy_elite(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_bygone_effigy(rng)
    combat.add_enemy(creature, ai)


def setup_byrdonis_elite(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_byrdonis(rng)
    combat.add_enemy(creature, ai)


def setup_phrog_parasite_elite(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_phrog_parasite(rng)
    combat.add_enemy(creature, ai)


ELITE_ENCOUNTERS: list[EncounterSetup] = [
    setup_bygone_effigy_elite,
    setup_byrdonis_elite,
    setup_phrog_parasite_elite,
]


# ---- Boss Encounters ----

def setup_vantom_boss(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_vantom(rng)
    combat.add_enemy(creature, ai)


def setup_ceremonial_beast_boss(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_ceremonial_beast(rng)
    combat.add_enemy(creature, ai)


def setup_the_kin_boss(combat: CombatState, rng: Rng) -> None:
    priest, priest_ai = create_kin_priest(rng)
    combat.add_enemy(priest, priest_ai)
    f1, f1_ai = create_kin_follower(rng, slot="first")
    combat.add_enemy(f1, f1_ai)
    f2, f2_ai = create_kin_follower(rng, slot="third")
    combat.add_enemy(f2, f2_ai)


BOSS_ENCOUNTERS: list[EncounterSetup] = [
    setup_vantom_boss,
    setup_ceremonial_beast_boss,
    setup_the_kin_boss,
]


ALL_ACT1_ENCOUNTERS: list[EncounterSetup] = (
    list(WEAK_ENCOUNTERS) +
    list(NORMAL_ENCOUNTERS) +
    list(ELITE_ENCOUNTERS) +
    list(BOSS_ENCOUNTERS)
)
