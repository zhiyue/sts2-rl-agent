"""Act 1 (Overgrowth) encounter definitions: weak, normal, elite, boss."""

from __future__ import annotations

from typing import Callable, TYPE_CHECKING

from sts2_env.core.rng import Rng
from sts2_env.encounters.room_set import (
    ROOM_TYPE_BOSS,
    ROOM_TYPE_ELITE,
    EncounterSpec,
)

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
    apply_cubex_construct_room_setup,
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
    creature, ai = create_shrinker_beetle(rng, ascension_level=combat.ascension_level)
    combat.add_enemy(creature, ai)


def setup_fuzzy_wurm_crawler_weak(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_fuzzy_wurm_crawler(rng, ascension_level=combat.ascension_level)
    combat.add_enemy(creature, ai)


def setup_nibbits_weak(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_nibbit(rng, is_alone=True, ascension_level=combat.ascension_level)
    combat.add_enemy(creature, ai)


def setup_slimes_weak(combat: CombatState, rng: Rng) -> None:
    small_creators = [create_leaf_slime_s, create_twig_slime_s]
    medium_creators = [create_leaf_slime_m, create_twig_slime_m]
    chosen_small = rng.sample(small_creators, 2)
    for creator in chosen_small:
        creature, ai = creator(rng, ascension_level=combat.ascension_level)
        combat.add_enemy(creature, ai)
    creator = rng.choice(medium_creators)
    creature, ai = creator(rng, ascension_level=combat.ascension_level)
    combat.add_enemy(creature, ai)


WEAK_ENCOUNTERS: list[EncounterSetup] = [
    setup_shrinker_beetle_weak,
    setup_fuzzy_wurm_crawler_weak,
    setup_nibbits_weak,
    setup_slimes_weak,
]


# ---- Normal Encounters ----

def setup_cubex_construct_normal(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_cubex_construct(rng, ascension_level=combat.ascension_level)
    combat.add_enemy(creature, ai)
    apply_cubex_construct_room_setup(creature, combat)


def setup_flyconid_normal(combat: CombatState, rng: Rng) -> None:
    slime_creator = rng.choice([create_leaf_slime_m, create_twig_slime_m])
    slime, slime_ai = slime_creator(rng, ascension_level=combat.ascension_level)
    combat.add_enemy(slime, slime_ai)
    creature, ai = create_flyconid(rng, ascension_level=combat.ascension_level)
    combat.add_enemy(creature, ai)


def setup_fogmog_normal(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_fogmog(rng, ascension_level=combat.ascension_level)
    combat.add_enemy(creature, ai)


def setup_inklets_normal(combat: CombatState, rng: Rng) -> None:
    first, first_ai = create_inklet(rng, ascension_level=combat.ascension_level)
    combat.add_enemy(first, first_ai)
    middle, middle_ai = create_inklet(rng, middle_inklet=True, ascension_level=combat.ascension_level)
    combat.add_enemy(middle, middle_ai)
    last, last_ai = create_inklet(rng, ascension_level=combat.ascension_level)
    combat.add_enemy(last, last_ai)


def setup_mawler_normal(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_mawler(rng, ascension_level=combat.ascension_level)
    combat.add_enemy(creature, ai)


def setup_nibbits_normal(combat: CombatState, rng: Rng) -> None:
    c1, a1 = create_nibbit(rng, is_alone=False, is_front=True, ascension_level=combat.ascension_level)
    combat.add_enemy(c1, a1)
    c2, a2 = create_nibbit(rng, is_alone=False, is_front=False, ascension_level=combat.ascension_level)
    combat.add_enemy(c2, a2)


def setup_overgrowth_crawlers(combat: CombatState, rng: Rng) -> None:
    shrinker, shrinker_ai = create_shrinker_beetle(rng, ascension_level=combat.ascension_level)
    combat.add_enemy(shrinker, shrinker_ai)
    crawler, crawler_ai = create_fuzzy_wurm_crawler(rng, ascension_level=combat.ascension_level)
    combat.add_enemy(crawler, crawler_ai)


def setup_ruby_raiders_normal(combat: CombatState, rng: Rng) -> None:
    raider_creators = [
        create_axe_ruby_raider,
        create_assassin_ruby_raider,
        create_brute_ruby_raider,
        create_crossbow_ruby_raider,
        create_tracker_ruby_raider,
    ]
    chosen = rng.sample(raider_creators, 3)
    for creator in chosen:
        creature, ai = creator(rng, ascension_level=combat.ascension_level)
        combat.add_enemy(creature, ai)


def setup_slimes_normal(combat: CombatState, rng: Rng) -> None:
    first_small_creator, second_small_creator = (
        (create_leaf_slime_s, create_twig_slime_s)
        if rng.next_bool()
        else (create_twig_slime_s, create_leaf_slime_s)
    )
    slime_creators = [
        create_twig_slime_m,
        create_leaf_slime_m,
        first_small_creator,
        second_small_creator,
    ]
    for creator in slime_creators:
        creature, ai = creator(rng, ascension_level=combat.ascension_level)
        combat.add_enemy(creature, ai)


def setup_slithering_strangler_normal(combat: CombatState, rng: Rng) -> None:
    snapping_jaxfruit_branch = "snapping_jaxfruit"
    medium_slime_branch = "medium_slime"
    small_slimes_branch = "small_slimes"
    secondary_enemy_type = rng.choice([snapping_jaxfruit_branch, medium_slime_branch, small_slimes_branch])
    if secondary_enemy_type == snapping_jaxfruit_branch:
        secondary_creators = [create_snapping_jaxfruit]
    elif secondary_enemy_type == medium_slime_branch:
        secondary_creators = [rng.choice([create_leaf_slime_m, create_twig_slime_m])]
    else:
        assert secondary_enemy_type == small_slimes_branch
        secondary_creators = [
            rng.choice([create_leaf_slime_s, create_twig_slime_s]),
            rng.choice([create_leaf_slime_s, create_twig_slime_s]),
        ]

    for creator in secondary_creators:
        if creator is create_snapping_jaxfruit:
            creature, ai = creator(rng, ascension_level=combat.ascension_level)
        else:
            creature, ai = creator(rng, ascension_level=combat.ascension_level)
        combat.add_enemy(creature, ai)

    strangler, strangler_ai = create_slithering_strangler(rng, ascension_level=combat.ascension_level)
    combat.add_enemy(strangler, strangler_ai)


def setup_snapping_jaxfruit_normal(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_snapping_jaxfruit(rng, ascension_level=combat.ascension_level)
    combat.add_enemy(creature, ai)
    flyconid, flyconid_ai = create_flyconid(rng, ascension_level=combat.ascension_level)
    combat.add_enemy(flyconid, flyconid_ai)


def setup_vine_shambler_normal(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_vine_shambler(rng, ascension_level=combat.ascension_level)
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
    creature, ai = create_bygone_effigy(rng, ascension_level=combat.ascension_level)
    combat.add_enemy(creature, ai)


def setup_byrdonis_elite(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_byrdonis(rng, ascension_level=combat.ascension_level)
    combat.add_enemy(creature, ai)


def setup_phrog_parasite_elite(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_phrog_parasite(rng, ascension_level=combat.ascension_level)
    combat.add_enemy(creature, ai)


ELITE_ENCOUNTERS: list[EncounterSetup] = [
    setup_bygone_effigy_elite,
    setup_byrdonis_elite,
    setup_phrog_parasite_elite,
]


# ---- Boss Encounters ----

def setup_vantom_boss(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_vantom(rng, ascension_level=combat.ascension_level)
    combat.add_enemy(creature, ai)


def setup_ceremonial_beast_boss(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_ceremonial_beast(rng, ascension_level=combat.ascension_level)
    combat.add_enemy(creature, ai)


def setup_the_kin_boss(combat: CombatState, rng: Rng) -> None:
    first_follower, first_follower_ai = create_kin_follower(
        rng,
        starts_with_dance=True,
        ascension_level=combat.ascension_level,
    )
    combat.add_enemy(first_follower, first_follower_ai)
    second_follower, second_follower_ai = create_kin_follower(rng, ascension_level=combat.ascension_level)
    combat.add_enemy(second_follower, second_follower_ai)
    priest, priest_ai = create_kin_priest(rng, ascension_level=combat.ascension_level)
    combat.add_enemy(priest, priest_ai)


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


# ---- C#-ordered encounter table (v0.107.1) ----
# Overgrowth.GenerateAllEncounters array order, with each entry's
# EncounterModel.Tags / IsWeak / RoomType (ActModel.cs anti-repeat inputs).
ALL_ENCOUNTER_SPECS: list[EncounterSpec] = [
    EncounterSpec(setup_bygone_effigy_elite, room_type=ROOM_TYPE_ELITE),
    EncounterSpec(setup_byrdonis_elite, room_type=ROOM_TYPE_ELITE),
    EncounterSpec(setup_ceremonial_beast_boss, room_type=ROOM_TYPE_BOSS),
    EncounterSpec(setup_cubex_construct_normal),
    EncounterSpec(setup_flyconid_normal, tags=frozenset({"Mushroom", "Slimes"})),
    EncounterSpec(setup_fogmog_normal),
    EncounterSpec(setup_fuzzy_wurm_crawler_weak, tags=frozenset({"Crawler"}), is_weak=True),
    EncounterSpec(setup_inklets_normal),
    EncounterSpec(setup_mawler_normal),
    EncounterSpec(setup_nibbits_normal),
    EncounterSpec(setup_nibbits_weak, tags=frozenset({"Nibbit"}), is_weak=True),
    EncounterSpec(setup_overgrowth_crawlers, tags=frozenset({"Crawler", "Shrinker"})),
    EncounterSpec(setup_phrog_parasite_elite, room_type=ROOM_TYPE_ELITE),
    EncounterSpec(setup_ruby_raiders_normal),
    EncounterSpec(setup_shrinker_beetle_weak, tags=frozenset({"Shrinker"}), is_weak=True),
    EncounterSpec(setup_slimes_normal, tags=frozenset({"Slimes"})),
    EncounterSpec(setup_slimes_weak, tags=frozenset({"Slimes"}), is_weak=True),
    EncounterSpec(setup_slithering_strangler_normal),
    EncounterSpec(setup_snapping_jaxfruit_normal, tags=frozenset({"Mushroom"})),
    EncounterSpec(setup_the_kin_boss, room_type=ROOM_TYPE_BOSS),
    EncounterSpec(setup_vantom_boss, room_type=ROOM_TYPE_BOSS),
    EncounterSpec(setup_vine_shambler_normal),
]
