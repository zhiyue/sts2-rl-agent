"""Behavior tests for the run-layer C# parity fixes (v0.107.1).

Each test notes its decompiled C# source:
- A25 encounter anti-repeat: MegaCrit.Sts2.Core.Models/ActModel.cs
  GenerateRooms / AddWithoutRepeatingTags + Helpers/GrabBag.cs; pool order
  comes from each act's GenerateAllEncounters array.
- A12 escaped-gold proportion: MegaCrit.Sts2.Core.Models/EncounterModel.cs
  L373-376 CalculateGoldProportion = 1 - EscapedCreatures/SpawnedEnemies.
- A16 potion odds: MegaCrit.Sts2.Core.Odds/PotionRewardOdds.cs L62-69: the
  elite bonus (+25%, halved) is part of the comparison threshold that also
  decides the +/-0.10 odds update.
- A20 relic prices: MegaCrit.Sts2.Core.Models/RelicModel.cs L304-312
  MerchantCost 175/225/275/200 by rarity (rolled x[0.85,1.15] in
  MerchantRelicEntry.CalcCost).
- A22 card removal: MegaCrit.Sts2.Core.Entities.Merchant/
  MerchantCardRemovalEntry.cs L20-32: base 100 + 50/removal at
  AscensionLevel.Inflation (6+), else 75 + 25.
"""

import math
from unittest.mock import MagicMock

import pytest

from sts2_env.core.enums import MapPointType, RoomType
from sts2_env.core.rng import Rng
from sts2_env.encounters import get_act_encounter_pools
from sts2_env.map.acts import ALL_ACTS
from sts2_env.run.odds import PotionRewardOdds
from sts2_env.run.reward_objects import calculate_gold_proportion
from sts2_env.run.run_state import RunState
from sts2_env.run.shop import (
    INFLATION_ASCENSION_LEVEL,
    MERCHANT_RELIC_PRICE_VARIANCE,
    RELIC_BASE_COSTS,
    card_removal_cost,
    relic_price,
)


# ── A25: anti-repeat pool semantics ──────────────────────────────────

class TestEncounterPoolOrderMatchesCSharp:
    def test_pool_composition_and_order(self):
        """C# AllWeak/AllRegular/AllElite/AllBossEncounters filter
        GenerateAllEncounters array order; Hive has no TunnelerNormal."""
        pools1 = get_act_encounter_pools(0)
        assert [f.__name__ for f in pools1["weak"]] == [
            "setup_fuzzy_wurm_crawler_weak",
            "setup_nibbits_weak",
            "setup_shrinker_beetle_weak",
            "setup_slimes_weak",
        ]
        assert [f.__name__ for f in pools1["boss"]] == [
            "setup_ceremonial_beast_boss",
            "setup_the_kin_boss",
            "setup_vantom_boss",
        ]
        pools2 = get_act_encounter_pools(1)
        normal_names = [f.__name__ for f in pools2["normal"]]
        assert "setup_tunneler_normal" not in normal_names  # not in Hive pool
        assert len(pools2["normal"]) == 10
        assert [f.__name__ for f in pools2["weak"]] == [
            "setup_bowlbugs_weak",
            "setup_exoskeletons_weak",
            "setup_thieving_hopper_weak",
            "setup_tunneler_weak",
        ]

    def test_anti_repeat_predicate_and_fallback(self):
        """C# AddWithoutRepeatingTags semantics: the draw skips entries that
        share a tag with (or equal) the previous pick; when NO bag entry can
        satisfy that (bag drained to conflicting entries), the fallback grabs
        any entry — so tag-sharing adjacency is legal near pool exhaustion."""
        from sts2_env.encounters import shares_tags_with
        from sts2_env.run.run_state import _EncounterGrabBag

        def a(name):
            fn = MagicMock()
            fn.__name__ = name
            return fn

        from sts2_env.encounters import ENCOUNTER_TAGS

        def register(name, tags):
            ENCOUNTER_TAGS[name] = frozenset(tags)

        register("t_a", {"mushroom"})
        register("t_b", {"mushroom"})
        register("t_c", set())
        try:
            prev = a("t_a")
            # Bag still holds a non-conflicting entry -> it must be picked.
            bag = _EncounterGrabBag([a("t_b"), a("t_c")])
            chosen = bag.grab_and_remove(
                Rng(0), predicate=lambda e: not shares_tags_with(e, prev) and e != prev
            )
            assert chosen.__name__ == "t_c"
            # Bag drained to conflicting entries only -> predicate fails
            # (no RNG consumed), fallback picks one of them.
            bag = _EncounterGrabBag([a("t_b")])
            empty = bag.grab_and_remove(
                Rng(0), predicate=lambda e: not shares_tags_with(e, prev) and e != prev
            )
            assert empty is None
            fallback = bag.grab_and_remove(Rng(0))
            assert fallback.__name__ == "t_b"
        finally:
            for name in ("t_a", "t_b", "t_c"):
                ENCOUNTER_TAGS.pop(name, None)


# ── A12: escaped-creature gold proportion ────────────────────────────

class TestGoldProportion:
    @staticmethod
    def _creature(hp=10, escaped=False):
        creature = MagicMock()
        creature.current_hp = hp
        creature.escaped = escaped
        return creature

    def test_no_escapes_full_gold(self):
        combat = MagicMock()
        combat.enemies = [self._creature(escaped=False), self._creature(escaped=True, hp=-1)]
        # Dead creatures (hp <= 0, removal-flagged escaped) are NOT escaped
        # per C# (CreatureEscaped only records live fleers).
        assert calculate_gold_proportion(combat) == 1.0

    def test_half_escaped_half_gold(self):
        combat = MagicMock()
        combat.enemies = [
            self._creature(escaped=True),   # fled (alive, flagged)
            self._creature(escaped=False),
        ]
        assert calculate_gold_proportion(combat) == 0.5

    def test_all_escaped_zero_gold(self):
        combat = MagicMock()
        combat.enemies = [self._creature(escaped=True)]
        assert calculate_gold_proportion(combat) == 0.0

    def test_no_enemies_full_gold(self):
        combat = MagicMock()
        combat.enemies = []
        assert calculate_gold_proportion(combat) == 1.0


# ── A16: potion odds elite bonus in the swing threshold ──────────────

class TestPotionRewardOddsEliteThreshold:
    def test_elite_bonus_drives_the_downswing(self):
        """C# L62-69: threshold = CurrentValue + (elite ? 0.25 : 0) * 0.5;
        roll < threshold => CurrentValue -= 0.10 AND return True."""
        odds = PotionRewardOdds()
        odds.current_value = 0.40
        # Non-elite: threshold 0.40; roll 0.45 misses -> odds INCREASE.
        rng = MagicMock()
        rng.next_float.return_value = 0.45
        assert odds.roll(rng, is_elite=False) is False
        assert odds.current_value == 0.50
        # Elite: threshold 0.40 + 0.125 = 0.525; same roll 0.45 hits ->
        # odds DECREASE and reward is granted.
        rng.next_float.return_value = 0.45
        assert odds.roll(rng, is_elite=True) is True
        assert odds.current_value == 0.40

    def test_non_elite_threshold_unchanged(self):
        odds = PotionRewardOdds()
        odds.current_value = 0.40
        rng = MagicMock()
        rng.next_float.return_value = 0.39
        assert odds.roll(rng, is_elite=False) is True
        assert odds.current_value == pytest.approx(0.30)


# ── A20: relic base prices ───────────────────────────────────────────

class TestRelicPrices:
    def test_base_costs_match_relic_model(self):
        """C# RelicModel.MerchantCost: Common 175 / Uncommon 225 / Rare 275
        / Shop 200."""
        from sts2_env.relics.base import RelicRarity

        assert RELIC_BASE_COSTS[RelicRarity.COMMON] == 175
        assert RELIC_BASE_COSTS[RelicRarity.UNCOMMON] == 225
        assert RELIC_BASE_COSTS[RelicRarity.RARE] == 275
        assert RELIC_BASE_COSTS[RelicRarity.SHOP] == 200

    def test_variance_bounds_applied(self):
        """C# MerchantRelicEntry.CalcCost: round(MerchantCost *
        Shops.NextFloat(0.85, 1.15))."""
        from sts2_env.relics.base import RelicRarity

        rng = Rng(123)
        for _ in range(50):
            price = relic_price(175, rng)
            assert math.floor(175 * MERCHANT_RELIC_PRICE_VARIANCE[0]) <= price <= math.ceil(
                175 * MERCHANT_RELIC_PRICE_VARIANCE[1]
            )


# ── A22: card removal inflation tier ─────────────────────────────────

class TestCardRemovalCost:
    def test_below_inflation_75_plus_25(self):
        """C# MerchantCardRemovalEntry: BaseCost 75, +25 per prior removal
        when below AscensionLevel.Inflation (6)."""
        for i in range(5):
            assert card_removal_cost(i) == 75 + 25 * i

    def test_at_inflation_100_plus_50(self):
        """C# GetValueIfAscension(Inflation, 100/50, 75/25): at A6+ the cost
        is 100 + 50 per prior removal."""
        assert INFLATION_ASCENSION_LEVEL == 6  # C# AscensionLevel.Inflation
        for i in range(5):
            assert card_removal_cost(i, ascension_level=6) == 100 + 50 * i
            assert card_removal_cost(i, ascension_level=10) == 100 + 50 * i

    def test_run_state_shop_inventory_uses_ascension(self):
        for asc, first, second in ((0, 75, 100), (6, 100, 150)):
            run_state = RunState(seed=11, ascension_level=asc)
            run_state.initialize_run()
            assert run_state.ascension_level == asc
            # Costs derive from player.card_shop_removals_used at generation.
            assert card_removal_cost(
                run_state.player.card_shop_removals_used,
                ascension_level=run_state.ascension_level,
            ) == first
            run_state.player.card_shop_removals_used = 1
            assert card_removal_cost(
                run_state.player.card_shop_removals_used,
                ascension_level=run_state.ascension_level,
            ) == second
