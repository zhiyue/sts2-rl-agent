"""Tests for potions system.

Verifies potion registry, rarity counts, pool filtering, and instance behavior.
"""

import pytest

from sts2_env.core.enums import PotionRarity, PotionUsageType, PotionTargetType
from sts2_env.potions.base import (
    PotionModel,
    PotionInstance,
    all_potion_models,
    create_potion,
    get_potion_model,
    normal_pool_models,
    roll_random_potion_model,
)
import sts2_env.potions.all  # noqa: F401 -- register all potions


class TestPotionRegistry:
    def test_total_count(self):
        assert len(all_potion_models()) == 63

    def test_normal_pool_excludes_event_token(self):
        pool = normal_pool_models()
        for m in pool:
            assert m.rarity not in (PotionRarity.EVENT, PotionRarity.TOKEN, PotionRarity.NONE)

    def test_normal_pool_count(self):
        """63 total - 2 Event (FoulPotion, GlowwaterPotion) - 1 Token (PotionShapedRock) = 60."""
        assert len(normal_pool_models()) == 60

    def test_character_filtered_pool_excludes_other_character_potions(self):
        ironclad_pool = {model.potion_id for model in normal_pool_models(character_id="Ironclad")}
        assert "BloodPotion" in ironclad_pool
        assert "SoldiersStew" in ironclad_pool
        assert "FocusPotion" not in ironclad_pool
        assert "StarPotion" not in ironclad_pool

    def test_in_combat_pool_excludes_out_of_combat_only_potions(self):
        in_combat_pool = {model.potion_id for model in normal_pool_models(in_combat=True)}
        assert "FairyInABottle" not in in_combat_pool
        assert "FruitJuice" not in in_combat_pool
        assert "RegenPotion" not in in_combat_pool

    def test_combat_pool_excludes_noncombat_generation_potions(self):
        combat_pool_ids = {m.potion_id for m in normal_pool_models(in_combat=True)}
        assert len(combat_pool_ids) == 57
        assert "FairyInABottle" not in combat_pool_ids
        assert "FruitJuice" not in combat_pool_ids
        assert "RegenPotion" not in combat_pool_ids

    def test_lookup_by_id(self):
        model = get_potion_model("FirePotion")
        assert model is not None
        assert model.potion_id == "FirePotion"

    def test_nonexistent_returns_none(self):
        assert get_potion_model("NonexistentPotion") is None


class TestRarityCounts:
    def test_common_count(self):
        common = [m for m in all_potion_models() if m.rarity == PotionRarity.COMMON]
        assert len(common) == 20

    def test_uncommon_count(self):
        uncommon = [m for m in all_potion_models() if m.rarity == PotionRarity.UNCOMMON]
        assert len(uncommon) == 20

    def test_rare_count(self):
        rare = [m for m in all_potion_models() if m.rarity == PotionRarity.RARE]
        assert len(rare) == 20

    def test_event_count(self):
        event = [m for m in all_potion_models() if m.rarity == PotionRarity.EVENT]
        assert len(event) == 2

    def test_token_count(self):
        token = [m for m in all_potion_models() if m.rarity == PotionRarity.TOKEN]
        assert len(token) == 1


class TestPotionUsageTypes:
    def test_combat_only_potions(self):
        combat = [m for m in all_potion_models() if m.usage_type == PotionUsageType.COMBAT_ONLY]
        assert len(combat) >= 50  # majority are combat only

    def test_any_time_potions(self):
        """BloodPotion, EntropicBrew, FruitJuice, FoulPotion."""
        any_time = [m for m in all_potion_models() if m.usage_type == PotionUsageType.ANY_TIME]
        any_time_ids = {m.potion_id for m in any_time}
        assert "BloodPotion" in any_time_ids
        assert "EntropicBrew" in any_time_ids
        assert "FruitJuice" in any_time_ids
        assert "FoulPotion" in any_time_ids

    def test_automatic_potions(self):
        """FairyInABottle is the only automatic potion."""
        auto = [m for m in all_potion_models() if m.usage_type == PotionUsageType.AUTOMATIC]
        assert len(auto) == 1
        assert auto[0].potion_id == "FairyInABottle"


class TestPotionTargetTypes:
    def test_self_targeting(self):
        m = get_potion_model("AttackPotion")
        assert m.target_type == PotionTargetType.SELF

    def test_any_enemy_targeting(self):
        m = get_potion_model("FirePotion")
        assert m.target_type == PotionTargetType.ANY_ENEMY

    def test_all_enemies_targeting(self):
        m = get_potion_model("ExplosiveAmpoule")
        assert m.target_type == PotionTargetType.ALL_ENEMIES

    def test_any_player_targeting(self):
        m = get_potion_model("BlockPotion")
        assert m.target_type == PotionTargetType.ANY_PLAYER


class TestPotionInstance:
    def test_create_potion(self):
        p = create_potion("FirePotion")
        assert p.potion_id == "FirePotion"
        assert p.rarity == PotionRarity.COMMON
        assert p.usage_type == PotionUsageType.COMBAT_ONLY
        assert p.target_type == PotionTargetType.ANY_ENEMY

    def test_can_use_in_combat(self):
        combat_only = create_potion("FirePotion")
        assert combat_only.can_use_in_combat()
        assert not combat_only.can_use_out_of_combat()

        any_time = create_potion("BloodPotion")
        assert any_time.can_use_in_combat()
        assert any_time.can_use_out_of_combat()

    def test_automatic_usage(self):
        fairy = create_potion("FairyInABottle")
        assert fairy.usage_type == PotionUsageType.AUTOMATIC

    def test_repr(self):
        p = create_potion("StrengthPotion")
        assert "StrengthPotion" in repr(p)

    def test_create_nonexistent_raises(self):
        with pytest.raises(KeyError):
            create_potion("NonexistentPotion")


class TestSpecificPotions:
    """Verify specific notable potions have correct attributes."""

    def test_fairy_in_a_bottle(self):
        m = get_potion_model("FairyInABottle")
        assert m.rarity == PotionRarity.RARE
        assert m.usage_type == PotionUsageType.AUTOMATIC
        assert m.target_type == PotionTargetType.SELF

    def test_foul_potion(self):
        m = get_potion_model("FoulPotion")
        assert m.rarity == PotionRarity.EVENT
        assert m.usage_type == PotionUsageType.ANY_TIME

    def test_glowwater_potion(self):
        m = get_potion_model("GlowwaterPotion")
        assert m.rarity == PotionRarity.EVENT
        assert m.usage_type == PotionUsageType.COMBAT_ONLY

    def test_potion_shaped_rock(self):
        m = get_potion_model("PotionShapedRock")
        assert m.rarity == PotionRarity.TOKEN
        assert m.target_type == PotionTargetType.ANY_ENEMY

    def test_entropic_brew(self):
        m = get_potion_model("EntropicBrew")
        assert m.rarity == PotionRarity.RARE
        assert m.usage_type == PotionUsageType.ANY_TIME

    def test_fruit_juice(self):
        m = get_potion_model("FruitJuice")
        assert m.rarity == PotionRarity.RARE
        assert m.usage_type == PotionUsageType.ANY_TIME
        assert m.target_type == PotionTargetType.ANY_PLAYER

    def test_energy_potion(self):
        m = get_potion_model("EnergyPotion")
        assert m.rarity == PotionRarity.COMMON
        assert m.usage_type == PotionUsageType.COMBAT_ONLY
        assert m.target_type == PotionTargetType.ANY_PLAYER

    def test_distilled_chaos(self):
        m = get_potion_model("DistilledChaos")
        assert m.rarity == PotionRarity.RARE
        assert m.usage_type == PotionUsageType.COMBAT_ONLY
        assert m.target_type == PotionTargetType.SELF


class TestPotionModelBehavior:
    def test_is_in_normal_pool_common(self):
        m = get_potion_model("FirePotion")
        assert m.is_in_normal_pool()

    def test_is_in_normal_pool_event(self):
        m = get_potion_model("FoulPotion")
        assert not m.is_in_normal_pool()

    def test_is_in_normal_pool_token(self):
        m = get_potion_model("PotionShapedRock")
        assert not m.is_in_normal_pool()

    def test_slot_index_default(self):
        p = create_potion("FirePotion")
        assert p.slot_index == -1

    def test_slot_index_assigned(self):
        p = create_potion("FirePotion", slot=2)
        assert p.slot_index == 2


class _StubPotionRng:
    def __init__(self, roll: float):
        self._roll = roll

    def next_float(self) -> float:
        return self._roll

    def choice(self, items):
        return items[0]


class TestPotionGenerationRolls:
    def test_roll_random_potion_model_uses_rarity_bands(self):
        rare = roll_random_potion_model(_StubPotionRng(0.05), character_id="Ironclad", in_combat=True)
        uncommon = roll_random_potion_model(_StubPotionRng(0.20), character_id="Ironclad", in_combat=True)
        common = roll_random_potion_model(_StubPotionRng(0.90), character_id="Ironclad", in_combat=True)

        assert rare is not None and rare.rarity == PotionRarity.RARE
        assert uncommon is not None and uncommon.rarity == PotionRarity.UNCOMMON
        assert common is not None and common.rarity == PotionRarity.COMMON
