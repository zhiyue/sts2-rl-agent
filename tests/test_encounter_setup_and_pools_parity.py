"""Tests for encounter setup functions across all 4 acts.

Verifies each encounter factory:
- Returns the correct number of monsters
- HP values are within expected ranges
- Monsters have valid state machines (current_move is accessible)
- Each act has the expected number of encounters per tier
"""

from __future__ import annotations

import pytest

from sts2_env.cards.ironclad_basic import create_ironclad_starter_deck
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import PowerId
from sts2_env.core.rng import Rng
from sts2_env.encounters.events import (
    DEPRECATED_ENCOUNTER_ID,
    get_event_encounter_setup,
    setup_deprecated_encounter,
    setup_punch_off,
)

# Act 1
from sts2_env.encounters.act1 import (
    setup_shrinker_beetle_weak,
    setup_fuzzy_wurm_crawler_weak,
    setup_nibbits_weak,
    setup_slimes_weak,
    WEAK_ENCOUNTERS as ACT1_WEAK,
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
    NORMAL_ENCOUNTERS as ACT1_NORMAL,
    setup_bygone_effigy_elite,
    setup_byrdonis_elite,
    setup_phrog_parasite_elite,
    ELITE_ENCOUNTERS as ACT1_ELITE,
    setup_vantom_boss,
    setup_ceremonial_beast_boss,
    setup_the_kin_boss,
    BOSS_ENCOUNTERS as ACT1_BOSS,
    ALL_ACT1_ENCOUNTERS,
)

# Act 2
from sts2_env.encounters.act2 import (
    WEAK_ENCOUNTERS as ACT2_WEAK,
    NORMAL_ENCOUNTERS as ACT2_NORMAL,
    ELITE_ENCOUNTERS as ACT2_ELITE,
    BOSS_ENCOUNTERS as ACT2_BOSS,
    ALL_ACT2_ENCOUNTERS,
    setup_bowlbugs_normal,
    setup_bowlbugs_weak,
    setup_chompers_normal,
    setup_decimillipede_elite,
    setup_entomancer_elite,
    setup_exoskeletons_normal,
    setup_exoskeletons_weak,
    setup_hunter_killer_normal,
    setup_knowledge_demon_boss,
    setup_louse_progenitor_normal,
    setup_mytes_normal,
    setup_ovicopter_normal,
    setup_slumbering_beetle_normal,
    setup_spiny_toad_normal,
    setup_the_insatiable_boss,
    setup_the_obscura_normal,
    setup_thieving_hopper_weak,
    setup_tunneler_normal,
    setup_tunneler_weak,
)

# Act 3
from sts2_env.encounters.act3 import (
    WEAK_ENCOUNTERS as ACT3_WEAK,
    NORMAL_ENCOUNTERS as ACT3_NORMAL,
    ELITE_ENCOUNTERS as ACT3_ELITE,
    BOSS_ENCOUNTERS as ACT3_BOSS,
    ALL_ACT3_ENCOUNTERS,
)

# Act 4
from sts2_env.encounters.act4 import (
    WEAK_ENCOUNTERS as ACT4_WEAK,
    NORMAL_ENCOUNTERS as ACT4_NORMAL,
    ELITE_ENCOUNTERS as ACT4_ELITE,
    BOSS_ENCOUNTERS as ACT4_BOSS,
    ALL_ACT4_ENCOUNTERS,
    setup_cultists_normal,
    setup_fossil_stalker_normal,
    setup_gremlin_merc_normal,
    setup_haunted_ship_normal,
    setup_living_fog_normal,
    setup_punch_construct_normal,
    setup_sewer_clam_normal,
    setup_skulking_colony_elite,
    setup_terror_eel_elite,
)
from sts2_env.encounters.events import (
    setup_battleworn_dummy_v1,
    setup_dense_vegetation,
    setup_fake_merchant,
    setup_mysterious_knight,
    setup_the_architect,
)


def _make_combat(rng_seed: int = 42) -> CombatState:
    """Create a fresh CombatState for encounter testing."""
    deck = create_ironclad_starter_deck()
    return CombatState(player_hp=80, player_max_hp=80, deck=deck, rng_seed=rng_seed)


RUBY_RAIDER_TOUGH_HP_RANGES = {
    "ASSASSIN_RUBY_RAIDER": (19, 24),
    "AXE_RUBY_RAIDER": (21, 23),
    "BRUTE_RUBY_RAIDER": (31, 34),
    "CROSSBOW_RUBY_RAIDER": (19, 22),
    "TRACKER_RUBY_RAIDER": (22, 26),
}
BYGONE_EFFIGY_TOUGH_HP = 132
BYRDONIS_TOUGH_HP = 90
PHROG_PARASITE_TOUGH_MIN_HP = 66
PHROG_PARASITE_TOUGH_MAX_HP = 68
VANTOM_TOUGH_HP = 183
CEREMONIAL_BEAST_TOUGH_HP = 262
KIN_FOLLOWER_TOUGH_MIN_HP = 62
KIN_FOLLOWER_TOUGH_MAX_HP = 63
KIN_PRIEST_TOUGH_HP = 199
THIEVING_HOPPER_MONSTER_ID = "THIEVING_HOPPER"
THIEVING_HOPPER_TOUGH_HP = 84
TUNNELER_MONSTER_ID = "TUNNELER"
TUNNELER_TOUGH_HP = 92
BOWLBUG_EGG_MONSTER_ID = "BOWLBUG_EGG"
BOWLBUG_EGG_TOUGH_MIN_HP = 23
BOWLBUG_EGG_TOUGH_MAX_HP = 24
BOWLBUG_NECTAR_MONSTER_ID = "BOWLBUG_NECTAR"
BOWLBUG_NECTAR_TOUGH_MIN_HP = 36
BOWLBUG_NECTAR_TOUGH_MAX_HP = 39
BOWLBUG_ROCK_MONSTER_ID = "BOWLBUG_ROCK"
BOWLBUG_ROCK_TOUGH_MIN_HP = 46
BOWLBUG_ROCK_TOUGH_MAX_HP = 49
BOWLBUG_SILK_MONSTER_ID = "BOWLBUG_SILK"
BOWLBUG_SILK_TOUGH_MIN_HP = 41
BOWLBUG_SILK_TOUGH_MAX_HP = 44
EXOSKELETON_MONSTER_ID = "EXOSKELETON"
EXOSKELETON_TOUGH_MIN_HP = 26
EXOSKELETON_TOUGH_MAX_HP = 30
CHOMPER_MONSTER_ID = "CHOMPER"
CHOMPER_TOUGH_MIN_HP = 63
CHOMPER_TOUGH_MAX_HP = 67
HUNTER_KILLER_MONSTER_ID = "HUNTER_KILLER"
HUNTER_KILLER_TOUGH_HP = 126
LOUSE_PROGENITOR_MONSTER_ID = "LOUSE_PROGENITOR"
LOUSE_PROGENITOR_TOUGH_MIN_HP = 138
LOUSE_PROGENITOR_TOUGH_MAX_HP = 141
MYTE_MONSTER_ID = "MYTE"
MYTE_TOUGH_MIN_HP = 64
MYTE_TOUGH_MAX_HP = 69
OVICOPTER_MONSTER_ID = "OVICOPTER"
OVICOPTER_TOUGH_MIN_HP = 126
OVICOPTER_TOUGH_MAX_HP = 132
SLUMBERING_BEETLE_MONSTER_ID = "SLUMBERING_BEETLE"
SLUMBERING_BEETLE_TOUGH_HP = 89
SLUMBERING_BEETLE_TOUGH_PLATING = 18
SPINY_TOAD_MONSTER_ID = "SPINY_TOAD"
SPINY_TOAD_TOUGH_MIN_HP = 121
SPINY_TOAD_TOUGH_MAX_HP = 124
THE_OBSCURA_MONSTER_ID = "THE_OBSCURA"
THE_OBSCURA_TOUGH_HP = 129


class _ExclusiveHighRng:
    def next_int_exclusive(self, low: int, high: int) -> int:
        return high - 1


def test_punch_off_hp_reduction_uses_exclusive_upper_bound():
    combat = _make_combat()

    setup_punch_off(combat, _ExclusiveHighRng())

    assert [enemy.current_hp for enemy in combat.enemies] == [46, 46]


CS_ENCOUNTER_PARITY_CASES = [
    ("BattlewornDummyEventEncounter", setup_battleworn_dummy_v1, ("BATTLE_FRIEND_V1",)),
    ("BowlbugsNormal", setup_bowlbugs_normal, ("BOWLBUG_EGG", "BOWLBUG_ROCK", "BOWLBUG_SILK")),
    ("BowlbugsWeak", setup_bowlbugs_weak, ("BOWLBUG_EGG", "BOWLBUG_NECTAR")),
    ("ChompersNormal", setup_chompers_normal, ("CHOMPER", "CHOMPER")),
    ("CultistsNormal", setup_cultists_normal, ("CALCIFIED_CULTIST", "DAMP_CULTIST")),
    (
        "DecimillipedeElite",
        setup_decimillipede_elite,
        ("DECIMILLIPEDE_SEGMENT", "DECIMILLIPEDE_SEGMENT", "DECIMILLIPEDE_SEGMENT"),
    ),
    ("DenseVegetationEventEncounter", setup_dense_vegetation, ("WRIGGLER", "WRIGGLER", "WRIGGLER", "WRIGGLER")),
    ("EntomancerElite", setup_entomancer_elite, ("ENTOMANCER",)),
    ("ExoskeletonsNormal", setup_exoskeletons_normal, ("EXOSKELETON", "EXOSKELETON", "EXOSKELETON")),
    ("ExoskeletonsWeak", setup_exoskeletons_weak, ("EXOSKELETON", "EXOSKELETON")),
    ("FakeMerchantEventEncounter", setup_fake_merchant, ("FAKE_MERCHANT_MONSTER",)),
    ("FossilStalkerNormal", setup_fossil_stalker_normal, ("FOSSIL_STALKER",)),
    ("GremlinMercNormal", setup_gremlin_merc_normal, ("GREMLIN_MERC", "SNEAKY_GREMLIN", "FAT_GREMLIN")),
    ("HauntedShipNormal", setup_haunted_ship_normal, ("HAUNTED_SHIP",)),
    ("HunterKillerNormal", setup_hunter_killer_normal, ("HUNTER_KILLER",)),
    ("KnowledgeDemonBoss", setup_knowledge_demon_boss, ("KNOWLEDGE_DEMON",)),
    ("LivingFogNormal", setup_living_fog_normal, ("LIVING_FOG",)),
    ("LouseProgenitorNormal", setup_louse_progenitor_normal, ("LOUSE_PROGENITOR",)),
    ("MysteriousKnightEventEncounter", setup_mysterious_knight, ("MYSTERIOUS_KNIGHT",)),
    ("OvicopterNormal", setup_ovicopter_normal, ("OVICOPTER",)),
    ("PunchConstructNormal", setup_punch_construct_normal, ("PUNCH_CONSTRUCT",)),
    ("PunchOffEventEncounter", setup_punch_off, ("PUNCH_CONSTRUCT", "PUNCH_CONSTRUCT")),
    ("SewerClamNormal", setup_sewer_clam_normal, ("SEWER_CLAM",)),
    ("SkulkingColonyElite", setup_skulking_colony_elite, ("SKULKING_COLONY",)),
    ("SlumberingBeetleNormal", setup_slumbering_beetle_normal, ("SLUMBERING_BEETLE",)),
    ("SpinyToadNormal", setup_spiny_toad_normal, ("SPINY_TOAD",)),
    ("TerrorEelElite", setup_terror_eel_elite, ("TERROR_EEL",)),
    ("TheArchitectEventEncounter", setup_the_architect, ("ARCHITECT",)),
    ("TheInsatiableBoss", setup_the_insatiable_boss, ("THE_INSATIABLE",)),
    ("TheObscuraNormal", setup_the_obscura_normal, ("THE_OBSCURA",)),
    ("ThievingHopperWeak", setup_thieving_hopper_weak, ("THIEVING_HOPPER",)),
    ("TunnelerWeak", setup_tunneler_weak, ("TUNNELER",)),
]


@pytest.mark.parametrize(
    "cs_name, setup, expected_monster_ids",
    CS_ENCOUNTER_PARITY_CASES,
    ids=[case[0] for case in CS_ENCOUNTER_PARITY_CASES],
)
def test_cs_named_encounter_setup_maps_to_expected_monsters(cs_name, setup, expected_monster_ids):
    combat = _make_combat()

    setup(combat, Rng(42))

    assert cs_name
    assert tuple(enemy.monster_id for enemy in combat.enemies) == expected_monster_ids
    assert all(combat.enemy_ais[enemy.combat_id].current_move.is_move for enemy in combat.enemies)


# ========================================================================
# Act 1: Weak Encounters
# ========================================================================

class TestAct1WeakEncounters:
    def test_shrinker_beetle_count_and_hp(self):
        for seed in range(5):
            combat = _make_combat(seed)
            rng = Rng(seed)
            setup_shrinker_beetle_weak(combat, rng)
            assert len(combat.enemies) == 1
            e = combat.enemies[0]
            assert 38 <= e.max_hp <= 40
            assert e.monster_id == "SHRINKER_BEETLE"

    def test_shrinker_beetle_tough_ascension_hp_matches_csharp(self):
        for seed in range(5):
            combat = _make_combat(seed)
            combat.ascension_level = 8
            rng = Rng(seed)
            setup_shrinker_beetle_weak(combat, rng)
            e = combat.enemies[0]
            assert 40 <= e.max_hp <= 42
            assert e.monster_id == "SHRINKER_BEETLE"

    def test_fuzzy_wurm_crawler_count_and_hp(self):
        for seed in range(5):
            combat = _make_combat(seed)
            rng = Rng(seed)
            setup_fuzzy_wurm_crawler_weak(combat, rng)
            assert len(combat.enemies) == 1
            e = combat.enemies[0]
            assert 55 <= e.max_hp <= 57
            assert e.monster_id == "FUZZY_WURM_CRAWLER"

    def test_fuzzy_wurm_crawler_tough_ascension_hp_matches_csharp(self):
        for seed in range(5):
            combat = _make_combat(seed)
            combat.ascension_level = 8
            rng = Rng(seed)
            setup_fuzzy_wurm_crawler_weak(combat, rng)
            e = combat.enemies[0]
            assert 58 <= e.max_hp <= 59
            assert e.monster_id == "FUZZY_WURM_CRAWLER"

    def test_nibbits_weak_count_and_hp(self):
        for seed in range(5):
            combat = _make_combat(seed)
            rng = Rng(seed)
            setup_nibbits_weak(combat, rng)
            assert len(combat.enemies) == 1
            e = combat.enemies[0]
            assert 42 <= e.max_hp <= 46
            assert e.monster_id == "NIBBIT"

    def test_nibbits_tough_ascension_hp_matches_csharp(self):
        for seed in range(5):
            combat = _make_combat(seed)
            combat.ascension_level = 8
            rng = Rng(seed)
            setup_nibbits_weak(combat, rng)
            e = combat.enemies[0]
            assert 44 <= e.max_hp <= 48
            assert e.monster_id == "NIBBIT"

    def test_slimes_weak_count(self):
        for seed in range(5):
            combat = _make_combat(seed)
            rng = Rng(seed)
            setup_slimes_weak(combat, rng)
            assert len(combat.enemies) == 3  # 2 small + 1 medium
            for e in combat.enemies:
                assert e.max_hp > 0

    def test_slimes_weak_tough_ascension_hp_matches_csharp(self):
        for seed in range(5):
            combat = _make_combat(seed)
            combat.ascension_level = 8
            rng = Rng(seed)
            setup_slimes_weak(combat, rng)
            ranges_by_id = {
                "LEAF_SLIME_S": (12, 16),
                "TWIG_SLIME_S": (8, 12),
                "LEAF_SLIME_M": (33, 36),
                "TWIG_SLIME_M": (27, 29),
            }
            for enemy in combat.enemies:
                min_hp, max_hp = ranges_by_id[enemy.monster_id]
                assert min_hp <= enemy.max_hp <= max_hp


# ========================================================================
# Act 1: Normal Encounters
# ========================================================================

class TestAct1NormalEncounters:
    def test_cubex_construct_count(self):
        combat = _make_combat()
        setup_cubex_construct_normal(combat, Rng(42))
        assert len(combat.enemies) == 1
        assert combat.enemies[0].max_hp == 65

    def test_cubex_construct_tough_ascension_hp_matches_csharp(self):
        combat = _make_combat()
        combat.ascension_level = 8
        setup_cubex_construct_normal(combat, Rng(42))
        assert len(combat.enemies) == 1
        assert combat.enemies[0].max_hp == 70

    def test_flyconid_count_and_hp(self):
        for seed in range(5):
            combat = _make_combat(seed)
            setup_flyconid_normal(combat, Rng(seed))
            assert combat.enemies[0].monster_id in {"LEAF_SLIME_M", "TWIG_SLIME_M"}
            assert combat.enemies[1].monster_id == "FLYCONID"
            assert 47 <= combat.enemies[1].max_hp <= 49
            assert combat.enemy_ais[combat.enemies[1].combat_id].current_move.state_id in {
                "FRAIL_SPORES_MOVE",
                "SMASH_MOVE",
            }

    def test_flyconid_tough_ascension_hp_matches_csharp(self):
        for seed in range(5):
            combat = _make_combat(seed)
            combat.ascension_level = 8
            setup_flyconid_normal(combat, Rng(seed))
            assert combat.enemies[1].monster_id == "FLYCONID"
            assert 51 <= combat.enemies[1].max_hp <= 53

    def test_fogmog_count(self):
        combat = _make_combat()
        setup_fogmog_normal(combat, Rng(42))
        assert len(combat.enemies) == 1

    def test_fogmog_tough_ascension_hp_matches_csharp(self):
        combat = _make_combat()
        combat.ascension_level = 8
        setup_fogmog_normal(combat, Rng(42))
        assert len(combat.enemies) == 1
        assert combat.enemies[0].max_hp == 78

    def test_inklets_count(self):
        combat = _make_combat()
        setup_inklets_normal(combat, Rng(42))
        assert [enemy.monster_id for enemy in combat.enemies] == ["INKLET", "INKLET", "INKLET"]
        assert [combat.enemy_ais[enemy.combat_id].current_move.state_id for enemy in combat.enemies] == [
            "JAB_MOVE",
            "WHIRLWIND_MOVE",
            "JAB_MOVE",
        ]
        assert all(11 <= enemy.max_hp <= 17 for enemy in combat.enemies)
        assert all(enemy.get_power_amount(PowerId.SLIPPERY) == 1 for enemy in combat.enemies)

    def test_inklets_tough_ascension_hp_matches_csharp(self):
        combat = _make_combat()
        combat.ascension_level = 8
        setup_inklets_normal(combat, Rng(42))
        assert [enemy.monster_id for enemy in combat.enemies] == ["INKLET", "INKLET", "INKLET"]
        assert all(12 <= enemy.max_hp <= 18 for enemy in combat.enemies)
        assert all(enemy.get_power_amount(PowerId.SLIPPERY) == 1 for enemy in combat.enemies)

    def test_mawler_count(self):
        combat = _make_combat()
        setup_mawler_normal(combat, Rng(42))
        assert len(combat.enemies) == 1

    def test_mawler_tough_ascension_hp_matches_csharp(self):
        combat = _make_combat()
        combat.ascension_level = 8
        setup_mawler_normal(combat, Rng(42))
        assert len(combat.enemies) == 1
        assert combat.enemies[0].monster_id == "MAWLER"
        assert combat.enemies[0].max_hp == 76

    def test_nibbits_normal_count(self):
        combat = _make_combat()
        setup_nibbits_normal(combat, Rng(42))
        assert len(combat.enemies) == 2
        for e in combat.enemies:
            assert e.monster_id == "NIBBIT"

    def test_overgrowth_crawlers_count(self):
        combat = _make_combat()
        setup_overgrowth_crawlers(combat, Rng(42))
        assert [enemy.monster_id for enemy in combat.enemies] == [
            "SHRINKER_BEETLE",
            "FUZZY_WURM_CRAWLER",
        ]
        assert [combat.enemy_ais[enemy.combat_id].current_move.state_id for enemy in combat.enemies] == [
            "SHRINKER_MOVE",
            "FIRST_ACID_GOOP",
        ]

    def test_ruby_raiders_normal_samples_three_unique_raiders_in_original_pool_order(self):
        encounter_seed = 42
        expected_raiders_for_seed = [
            "CROSSBOW_RUBY_RAIDER",
            "AXE_RUBY_RAIDER",
            "ASSASSIN_RUBY_RAIDER",
        ]
        combat = _make_combat()

        setup_ruby_raiders_normal(combat, Rng(encounter_seed))

        assert [enemy.monster_id for enemy in combat.enemies] == expected_raiders_for_seed
        assert len(set(enemy.monster_id for enemy in combat.enemies)) == len(expected_raiders_for_seed)

    def test_ruby_raiders_tough_ascension_hp_matches_csharp(self):
        seen_raiders = set()
        for seed in range(30):
            combat = _make_combat(seed)
            combat.ascension_level = 8

            setup_ruby_raiders_normal(combat, Rng(seed))

            assert len(combat.enemies) == 3
            for enemy in combat.enemies:
                min_hp, max_hp = RUBY_RAIDER_TOUGH_HP_RANGES[enemy.monster_id]
                assert min_hp <= enemy.max_hp <= max_hp
                seen_raiders.add(enemy.monster_id)

        assert seen_raiders == set(RUBY_RAIDER_TOUGH_HP_RANGES)

    def test_slimes_normal_count(self):
        seen_small_orders = set()
        for seed in range(5):
            combat = _make_combat(seed)

            setup_slimes_normal(combat, Rng(seed))

            monster_ids = [enemy.monster_id for enemy in combat.enemies]
            assert monster_ids[:2] == ["TWIG_SLIME_M", "LEAF_SLIME_M"]
            assert set(monster_ids[2:]) == {"LEAF_SLIME_S", "TWIG_SLIME_S"}
            assert [combat.enemy_ais[enemy.combat_id].current_move.state_id for enemy in combat.enemies[:2]] == [
                "STICKY_SHOT_MOVE",
                "STICKY_SHOT",
            ]
            seen_small_orders.add(tuple(monster_ids[2:]))

        assert seen_small_orders == {
            ("LEAF_SLIME_S", "TWIG_SLIME_S"),
            ("TWIG_SLIME_S", "LEAF_SLIME_S"),
        }

    def test_slithering_strangler_normal_uses_original_secondary_enemy_branches(self):
        expected_secondary_ids = {
            ("SNAPPING_JAXFRUIT",),
            ("LEAF_SLIME_M",),
            ("TWIG_SLIME_M",),
            ("LEAF_SLIME_S", "LEAF_SLIME_S"),
            ("LEAF_SLIME_S", "TWIG_SLIME_S"),
            ("TWIG_SLIME_S", "LEAF_SLIME_S"),
            ("TWIG_SLIME_S", "TWIG_SLIME_S"),
        }
        expected_branch_markers = {"SNAPPING_JAXFRUIT", "LEAF_SLIME_M", "TWIG_SLIME_M", "SMALL_SLIME"}
        seen_secondary_shapes: set[tuple[str, ...]] = set()
        for seed in range(30):
            combat = _make_combat(seed)

            setup_slithering_strangler_normal(combat, Rng(seed))

            assert combat.enemies[-1].monster_id == "SLITHERING_STRANGLER"
            secondary_ids = tuple(enemy.monster_id for enemy in combat.enemies[:-1])
            assert secondary_ids in expected_secondary_ids
            seen_secondary_shapes.add(tuple("SMALL_SLIME" if monster_id.endswith("_SLIME_S") else monster_id
                                            for monster_id in secondary_ids))

        assert expected_branch_markers <= {
            monster_id
            for secondary_shape in seen_secondary_shapes
            for monster_id in secondary_shape
        }

    def test_slithering_strangler_tough_ascension_hp_matches_csharp(self):
        for seed in range(30):
            combat = _make_combat(seed)
            combat.ascension_level = 8

            setup_slithering_strangler_normal(combat, Rng(seed))

            strangler = combat.enemies[-1]
            assert strangler.monster_id == "SLITHERING_STRANGLER"
            assert 54 <= strangler.max_hp <= 56

    def test_snapping_jaxfruit_count(self):
        combat = _make_combat()
        setup_snapping_jaxfruit_normal(combat, Rng(42))
        assert [enemy.monster_id for enemy in combat.enemies] == [
            "SNAPPING_JAXFRUIT",
            "FLYCONID",
        ]
        assert 31 <= combat.enemies[0].max_hp <= 33
        assert combat.enemy_ais[combat.enemies[0].combat_id].current_move.state_id == "ENERGY_ORB_MOVE"

    def test_snapping_jaxfruit_tough_ascension_hp_matches_csharp(self):
        combat = _make_combat()
        combat.ascension_level = 8
        setup_snapping_jaxfruit_normal(combat, Rng(42))
        assert [enemy.monster_id for enemy in combat.enemies] == [
            "SNAPPING_JAXFRUIT",
            "FLYCONID",
        ]
        assert 34 <= combat.enemies[0].max_hp <= 36
        assert combat.enemy_ais[combat.enemies[0].combat_id].current_move.state_id == "ENERGY_ORB_MOVE"

    def test_vine_shambler_count(self):
        combat = _make_combat()
        setup_vine_shambler_normal(combat, Rng(42))
        assert len(combat.enemies) == 1
        assert combat.enemies[0].monster_id == "VINE_SHAMBLER"
        assert combat.enemies[0].max_hp == 61
        assert combat.enemy_ais[combat.enemies[0].combat_id].current_move.state_id == "SWIPE_MOVE"

    def test_vine_shambler_tough_ascension_hp_matches_csharp(self):
        combat = _make_combat()
        combat.ascension_level = 8
        setup_vine_shambler_normal(combat, Rng(42))
        assert len(combat.enemies) == 1
        assert combat.enemies[0].monster_id == "VINE_SHAMBLER"
        assert combat.enemies[0].max_hp == 64
        assert combat.enemy_ais[combat.enemies[0].combat_id].current_move.state_id == "SWIPE_MOVE"


# ========================================================================
# Act 1: Elite Encounters
# ========================================================================

class TestAct1EliteEncounters:
    def test_bygone_effigy_count(self):
        combat = _make_combat()
        setup_bygone_effigy_elite(combat, Rng(42))
        assert len(combat.enemies) == 1

    def test_bygone_effigy_tough_ascension_hp_matches_csharp(self):
        combat = _make_combat()
        combat.ascension_level = 8

        setup_bygone_effigy_elite(combat, Rng(42))

        assert len(combat.enemies) == 1
        assert combat.enemies[0].max_hp == BYGONE_EFFIGY_TOUGH_HP

    def test_byrdonis_count(self):
        combat = _make_combat()
        setup_byrdonis_elite(combat, Rng(42))
        assert len(combat.enemies) == 1

    def test_byrdonis_tough_ascension_hp_matches_csharp(self):
        combat = _make_combat()
        combat.ascension_level = 8

        setup_byrdonis_elite(combat, Rng(42))

        assert len(combat.enemies) == 1
        assert combat.enemies[0].max_hp == BYRDONIS_TOUGH_HP

    def test_phrog_parasite_count(self):
        combat = _make_combat()
        setup_phrog_parasite_elite(combat, Rng(42))
        assert len(combat.enemies) == 1

    def test_phrog_parasite_tough_ascension_hp_matches_csharp(self):
        for seed in range(5):
            combat = _make_combat(seed)
            combat.ascension_level = 8

            setup_phrog_parasite_elite(combat, Rng(seed))

            assert len(combat.enemies) == 1
            assert PHROG_PARASITE_TOUGH_MIN_HP <= combat.enemies[0].max_hp <= PHROG_PARASITE_TOUGH_MAX_HP


# ========================================================================
# Act 1: Boss Encounters
# ========================================================================

class TestAct1BossEncounters:
    def test_vantom_count(self):
        combat = _make_combat()
        setup_vantom_boss(combat, Rng(42))
        assert len(combat.enemies) == 1

    def test_vantom_tough_ascension_hp_matches_csharp(self):
        combat = _make_combat()
        combat.ascension_level = 8

        setup_vantom_boss(combat, Rng(42))

        assert len(combat.enemies) == 1
        assert combat.enemies[0].max_hp == VANTOM_TOUGH_HP

    def test_ceremonial_beast_count(self):
        combat = _make_combat()
        setup_ceremonial_beast_boss(combat, Rng(42))
        assert len(combat.enemies) == 1

    def test_ceremonial_beast_tough_ascension_hp_matches_csharp(self):
        combat = _make_combat()
        combat.ascension_level = 8

        setup_ceremonial_beast_boss(combat, Rng(42))

        assert len(combat.enemies) == 1
        assert combat.enemies[0].max_hp == CEREMONIAL_BEAST_TOUGH_HP

    def test_the_kin_count(self):
        combat = _make_combat()
        setup_the_kin_boss(combat, Rng(42))
        assert len(combat.enemies) == 3  # priest + 2 followers

    def test_the_kin_tough_ascension_hp_matches_csharp(self):
        for seed in range(5):
            combat = _make_combat(seed)
            combat.ascension_level = 8

            setup_the_kin_boss(combat, Rng(seed))

            assert [enemy.monster_id for enemy in combat.enemies] == [
                "KIN_FOLLOWER",
                "KIN_FOLLOWER",
                "KIN_PRIEST",
            ]
            for follower in combat.enemies[:2]:
                assert KIN_FOLLOWER_TOUGH_MIN_HP <= follower.max_hp <= KIN_FOLLOWER_TOUGH_MAX_HP
            assert combat.enemies[2].max_hp == KIN_PRIEST_TOUGH_HP

    def test_the_kin_followers_start_on_original_moves(self):
        combat = _make_combat()

        setup_the_kin_boss(combat, Rng(42))

        assert [enemy.monster_id for enemy in combat.enemies] == [
            "KIN_FOLLOWER",
            "KIN_FOLLOWER",
            "KIN_PRIEST",
        ]
        assert [combat.enemy_ais[enemy.combat_id].current_move.state_id for enemy in combat.enemies] == [
            "POWER_DANCE_MOVE",
            "QUICK_SLASH_MOVE",
            "ORB_OF_FRAILTY_MOVE",
        ]


# ========================================================================
# Act 1: Pool Counts
# ========================================================================

class TestAct1Pools:
    def test_weak_encounter_count(self):
        assert len(ACT1_WEAK) == 4

    def test_normal_encounter_count(self):
        assert len(ACT1_NORMAL) == 12

    def test_elite_encounter_count(self):
        assert len(ACT1_ELITE) == 3

    def test_boss_encounter_count(self):
        assert len(ACT1_BOSS) == 3

    def test_all_act1_total(self):
        assert len(ALL_ACT1_ENCOUNTERS) == 4 + 12 + 3 + 3  # 22


# ========================================================================
# Act 2: Pool Counts
# ========================================================================

class TestAct2Pools:
    def test_decimillipede_elite_offsets_segment_openers_from_original_random_starter(self):
        encounter_seed = 0
        expected_openers = [
            "CONSTRICT_MOVE",
            "WRITHE_MOVE",
            "BULK_MOVE",
        ]
        combat = _make_combat(encounter_seed)

        setup_decimillipede_elite(combat, Rng(encounter_seed))

        assert [enemy.monster_id for enemy in combat.enemies] == [
            "DECIMILLIPEDE_SEGMENT",
            "DECIMILLIPEDE_SEGMENT",
            "DECIMILLIPEDE_SEGMENT",
        ]
        assert [combat.enemy_ais[enemy.combat_id].current_move.state_id for enemy in combat.enemies] == expected_openers

    def test_act2_weak_tough_ascension_hp_matches_csharp(self):
        for seed in range(5):
            combat = _make_combat(seed)
            combat.ascension_level = 8

            setup_thieving_hopper_weak(combat, Rng(seed))

            assert len(combat.enemies) == 1
            assert combat.enemies[0].monster_id == THIEVING_HOPPER_MONSTER_ID
            assert combat.enemies[0].max_hp == THIEVING_HOPPER_TOUGH_HP

            combat = _make_combat(seed)
            combat.ascension_level = 8

            setup_tunneler_weak(combat, Rng(seed))

            assert len(combat.enemies) == 1
            assert combat.enemies[0].monster_id == TUNNELER_MONSTER_ID
            assert combat.enemies[0].max_hp == TUNNELER_TOUGH_HP

    def test_exoskeletons_tough_ascension_hp_matches_csharp(self):
        for seed in range(5):
            combat = _make_combat(seed)
            combat.ascension_level = 8

            setup_exoskeletons_weak(combat, Rng(seed))

            assert [enemy.monster_id for enemy in combat.enemies] == [
                EXOSKELETON_MONSTER_ID,
                EXOSKELETON_MONSTER_ID,
            ]
            assert all(EXOSKELETON_TOUGH_MIN_HP <= enemy.max_hp <= EXOSKELETON_TOUGH_MAX_HP for enemy in combat.enemies)

            combat = _make_combat(seed)
            combat.ascension_level = 8

            setup_exoskeletons_normal(combat, Rng(seed))

            assert [enemy.monster_id for enemy in combat.enemies] == [
                EXOSKELETON_MONSTER_ID,
                EXOSKELETON_MONSTER_ID,
                EXOSKELETON_MONSTER_ID,
            ]
            assert all(EXOSKELETON_TOUGH_MIN_HP <= enemy.max_hp <= EXOSKELETON_TOUGH_MAX_HP for enemy in combat.enemies)

    def test_bowlbugs_weak_tough_ascension_hp_matches_csharp(self):
        for seed in range(5):
            combat = _make_combat(seed)
            combat.ascension_level = 8

            setup_bowlbugs_weak(combat, Rng(seed))

            assert [enemy.monster_id for enemy in combat.enemies] == [
                BOWLBUG_EGG_MONSTER_ID,
                BOWLBUG_NECTAR_MONSTER_ID,
            ]
            egg, nectar = combat.enemies
            assert BOWLBUG_EGG_TOUGH_MIN_HP <= egg.max_hp <= BOWLBUG_EGG_TOUGH_MAX_HP
            assert BOWLBUG_NECTAR_TOUGH_MIN_HP <= nectar.max_hp <= BOWLBUG_NECTAR_TOUGH_MAX_HP

    def test_bowlbugs_normal_tough_ascension_hp_matches_csharp(self):
        for seed in range(5):
            combat = _make_combat(seed)
            combat.ascension_level = 8

            setup_bowlbugs_normal(combat, Rng(seed))

            assert [enemy.monster_id for enemy in combat.enemies] == [
                BOWLBUG_EGG_MONSTER_ID,
                BOWLBUG_ROCK_MONSTER_ID,
                BOWLBUG_SILK_MONSTER_ID,
            ]
            egg, rock, silk = combat.enemies
            assert BOWLBUG_EGG_TOUGH_MIN_HP <= egg.max_hp <= BOWLBUG_EGG_TOUGH_MAX_HP
            assert BOWLBUG_ROCK_TOUGH_MIN_HP <= rock.max_hp <= BOWLBUG_ROCK_TOUGH_MAX_HP
            assert BOWLBUG_SILK_TOUGH_MIN_HP <= silk.max_hp <= BOWLBUG_SILK_TOUGH_MAX_HP

    def test_tunneler_normal_tough_ascension_hp_matches_csharp(self):
        for seed in range(5):
            combat = _make_combat(seed)
            combat.ascension_level = 8

            setup_tunneler_normal(combat, Rng(seed))

            assert len(combat.enemies) == 2
            assert combat.enemies[1].monster_id == TUNNELER_MONSTER_ID
            assert combat.enemies[1].max_hp == TUNNELER_TOUGH_HP

    def test_act2_normal_tough_ascension_hp_matches_csharp(self):
        for seed in range(5):
            combat = _make_combat(seed)
            combat.ascension_level = 8

            setup_chompers_normal(combat, Rng(seed))

            assert [enemy.monster_id for enemy in combat.enemies] == [CHOMPER_MONSTER_ID, CHOMPER_MONSTER_ID]
            assert all(CHOMPER_TOUGH_MIN_HP <= enemy.max_hp <= CHOMPER_TOUGH_MAX_HP for enemy in combat.enemies)

            combat = _make_combat(seed)
            combat.ascension_level = 8

            setup_hunter_killer_normal(combat, Rng(seed))

            assert len(combat.enemies) == 1
            assert combat.enemies[0].monster_id == HUNTER_KILLER_MONSTER_ID
            assert combat.enemies[0].max_hp == HUNTER_KILLER_TOUGH_HP

            combat = _make_combat(seed)
            combat.ascension_level = 8

            setup_louse_progenitor_normal(combat, Rng(seed))

            assert len(combat.enemies) == 1
            assert combat.enemies[0].monster_id == LOUSE_PROGENITOR_MONSTER_ID
            assert LOUSE_PROGENITOR_TOUGH_MIN_HP <= combat.enemies[0].max_hp <= LOUSE_PROGENITOR_TOUGH_MAX_HP

            combat = _make_combat(seed)
            combat.ascension_level = 8

            setup_mytes_normal(combat, Rng(seed))

            assert [enemy.monster_id for enemy in combat.enemies] == [MYTE_MONSTER_ID, MYTE_MONSTER_ID]
            assert all(MYTE_TOUGH_MIN_HP <= enemy.max_hp <= MYTE_TOUGH_MAX_HP for enemy in combat.enemies)

            combat = _make_combat(seed)
            combat.ascension_level = 8

            setup_ovicopter_normal(combat, Rng(seed))

            assert len(combat.enemies) == 1
            assert combat.enemies[0].monster_id == OVICOPTER_MONSTER_ID
            assert OVICOPTER_TOUGH_MIN_HP <= combat.enemies[0].max_hp <= OVICOPTER_TOUGH_MAX_HP

            combat = _make_combat(seed)
            combat.ascension_level = 8

            setup_slumbering_beetle_normal(combat, Rng(seed))

            assert len(combat.enemies) == 1
            assert combat.enemies[0].monster_id == SLUMBERING_BEETLE_MONSTER_ID
            assert combat.enemies[0].max_hp == SLUMBERING_BEETLE_TOUGH_HP
            assert combat.enemies[0].get_power_amount(PowerId.PLATING) == SLUMBERING_BEETLE_TOUGH_PLATING

            combat = _make_combat(seed)
            combat.ascension_level = 8

            setup_spiny_toad_normal(combat, Rng(seed))

            assert len(combat.enemies) == 1
            assert combat.enemies[0].monster_id == SPINY_TOAD_MONSTER_ID
            assert SPINY_TOAD_TOUGH_MIN_HP <= combat.enemies[0].max_hp <= SPINY_TOAD_TOUGH_MAX_HP

            combat = _make_combat(seed)
            combat.ascension_level = 8

            setup_the_obscura_normal(combat, Rng(seed))

            assert len(combat.enemies) == 1
            assert combat.enemies[0].monster_id == THE_OBSCURA_MONSTER_ID
            assert combat.enemies[0].max_hp == THE_OBSCURA_TOUGH_HP

    def test_weak_encounter_count(self):
        assert len(ACT2_WEAK) == 4

    def test_normal_encounter_count(self):
        assert len(ACT2_NORMAL) == 11

    def test_elite_encounter_count(self):
        assert len(ACT2_ELITE) == 3

    def test_boss_encounter_count(self):
        assert len(ACT2_BOSS) == 3

    def test_all_act2_total(self):
        assert len(ALL_ACT2_ENCOUNTERS) == 4 + 11 + 3 + 3  # 21


# ========================================================================
# Act 3: Pool Counts
# ========================================================================

class TestAct3Pools:
    def test_weak_encounter_count(self):
        assert len(ACT3_WEAK) == 3

    def test_normal_encounter_count(self):
        assert len(ACT3_NORMAL) == 9

    def test_elite_encounter_count(self):
        assert len(ACT3_ELITE) == 3

    def test_boss_encounter_count(self):
        assert len(ACT3_BOSS) == 3

    def test_all_act3_total(self):
        assert len(ALL_ACT3_ENCOUNTERS) == 3 + 9 + 3 + 3  # 18

    def test_act3_order_matches_original_glory_lists(self):
        assert [encounter.__name__ for encounter in ACT3_BOSS] == [
            "setup_queen_boss",
            "setup_test_subject_boss",
            "setup_aeonglass_boss",
        ]
        assert [encounter.__name__ for encounter in ALL_ACT3_ENCOUNTERS] == [
            "setup_axebots_normal",
            "setup_construct_menagerie_normal",
            "setup_devoted_sculptor_weak",
            "setup_aeonglass_boss",
            "setup_fabricator_normal",
            "setup_frog_knight_normal",
            "setup_globe_head_normal",
            "setup_knights_elite",
            "setup_mecha_knight_elite",
            "setup_owl_magistrate_normal",
            "setup_queen_boss",
            "setup_scrolls_of_biting_normal",
            "setup_scrolls_of_biting_weak",
            "setup_slimed_berserker_normal",
            "setup_soul_nexus_elite",
            "setup_test_subject_boss",
            "setup_the_lost_and_forgotten_normal",
            "setup_turret_operator_weak",
        ]


# ========================================================================
# Act 4: Pool Counts
# ========================================================================

class TestAct4Pools:
    def test_weak_encounter_count(self):
        assert len(ACT4_WEAK) == 4

    def test_normal_encounter_count(self):
        assert len(ACT4_NORMAL) == 10

    def test_elite_encounter_count(self):
        assert len(ACT4_ELITE) == 3

    def test_boss_encounter_count(self):
        assert len(ACT4_BOSS) == 3

    def test_all_act4_total(self):
        assert len(ALL_ACT4_ENCOUNTERS) == 4 + 10 + 3 + 3  # 20


# ========================================================================
# Cross-act: All encounters can be set up and produce valid AI
# ========================================================================

ALL_ENCOUNTERS_BY_ACT = [
    ("act1", ALL_ACT1_ENCOUNTERS),
    ("act2", ALL_ACT2_ENCOUNTERS),
    ("act3", ALL_ACT3_ENCOUNTERS),
    ("act4", ALL_ACT4_ENCOUNTERS),
]


class TestAllEncountersSetup:
    @pytest.mark.parametrize("act_name, encounters", ALL_ENCOUNTERS_BY_ACT)
    def test_every_encounter_creates_enemies_with_valid_ai(self, act_name, encounters):
        """Every encounter across all 4 acts should set up enemies with valid AI."""
        for idx, encounter in enumerate(encounters):
            rng = Rng(42)
            combat = _make_combat(42)
            encounter(combat, rng)

            assert len(combat.enemies) >= 1, (
                f"{act_name} encounter {idx} added no enemies"
            )
            for enemy in combat.enemies:
                assert enemy.max_hp > 0
                assert enemy.is_alive
                ai = combat.enemy_ais[enemy.combat_id]
                move = ai.current_move
                assert move.is_move
                assert len(move.intents) >= 1

    @pytest.mark.parametrize("act_name, encounters", ALL_ENCOUNTERS_BY_ACT)
    def test_hp_values_within_bounds(self, act_name, encounters):
        """All monster HP values should be between 1 and 600."""
        for seed in range(10):
            rng = Rng(seed)
            for encounter in encounters:
                combat = _make_combat(seed)
                encounter(combat, rng)
                for enemy in combat.enemies:
                    # v0.111.0: Act3 boss Aeonglass is a fixed 512 (A8 535) HP.
                    assert 1 <= enemy.max_hp <= 600, (
                        f"{act_name}: unreasonable HP {enemy.max_hp} for {enemy.monster_id}"
                    )
                    assert enemy.current_hp == enemy.max_hp

    @pytest.mark.parametrize("act_name, encounters", ALL_ENCOUNTERS_BY_ACT)
    def test_multiple_seeds_all_succeed(self, act_name, encounters):
        """All encounters should set up successfully across different seeds."""
        for seed in range(10):
            rng = Rng(seed)
            for encounter in encounters:
                combat = _make_combat(seed)
                encounter(combat, rng)
                assert len(combat.enemies) >= 1


class TestAllActsHaveEncounters:
    def test_all_4_acts_present(self):
        """Verify all 4 acts have encounter lists defined."""
        assert len(ALL_ACT1_ENCOUNTERS) > 0
        assert len(ALL_ACT2_ENCOUNTERS) > 0
        assert len(ALL_ACT3_ENCOUNTERS) > 0
        assert len(ALL_ACT4_ENCOUNTERS) > 0

    def test_total_encounter_count(self):
        """Verify total encounters across all acts."""
        total = (
            len(ALL_ACT1_ENCOUNTERS) +
            len(ALL_ACT2_ENCOUNTERS) +
            len(ALL_ACT3_ENCOUNTERS) +
            len(ALL_ACT4_ENCOUNTERS)
        )
        # 22 + 21 + 18 + 20 = 81
        assert total == 81

    def test_each_act_has_all_tiers(self):
        """Each act should have weak, normal, elite, and boss encounters."""
        for name, weak, normal, elite, boss in [
            ("act1", ACT1_WEAK, ACT1_NORMAL, ACT1_ELITE, ACT1_BOSS),
            ("act2", ACT2_WEAK, ACT2_NORMAL, ACT2_ELITE, ACT2_BOSS),
            ("act3", ACT3_WEAK, ACT3_NORMAL, ACT3_ELITE, ACT3_BOSS),
            ("act4", ACT4_WEAK, ACT4_NORMAL, ACT4_ELITE, ACT4_BOSS),
        ]:
            assert len(weak) >= 2, f"{name} has too few weak encounters"
            assert len(normal) >= 5, f"{name} has too few normal encounters"
            assert len(elite) >= 2, f"{name} has too few elite encounters"
            assert len(boss) >= 2, f"{name} has too few boss encounters"


def test_deprecated_encounter_is_debug_placeholder_with_no_monsters():
    rng = Rng(42)
    combat = _make_combat(42)

    setup_deprecated_encounter(combat, rng)

    assert combat.enemies == []
    assert get_event_encounter_setup(DEPRECATED_ENCOUNTER_ID) is setup_deprecated_encounter
