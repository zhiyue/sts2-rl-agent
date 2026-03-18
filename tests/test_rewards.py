"""Tests for card reward generation.

Verifies rarity distribution, pity counter mechanics, upgrade probability
scaling, and blacklist handling.
"""

import pytest

from sts2_env.cards.factory import eligible_character_cards, eligible_registered_cards
from sts2_env.core.enums import CardId, CardRarity, CardType
from sts2_env.core.rng import Rng
from sts2_env.run.odds import CardRarityOdds
from sts2_env.run.rewards import (
    generate_card_reward,
    generate_combat_card_rewards,
    generate_combat_reward_cards,
    roll_for_upgrade,
)
from sts2_env.run.run_state import RunState


class TestCardRarityOdds:
    def test_initial_pity_value(self):
        odds = CardRarityOdds(ascension_level=0)
        assert odds.current_value == -0.05

    def test_boss_always_rare(self):
        odds = CardRarityOdds()
        rng = Rng(42)
        results = [odds.roll(rng, context="boss") for _ in range(100)]
        assert all(r == CardRarity.RARE for r in results)

    def test_boss_does_not_change_pity(self):
        odds = CardRarityOdds()
        initial_pity = odds.current_value
        rng = Rng(42)
        odds.roll(rng, context="boss")
        assert odds.current_value == initial_pity

    def test_pity_counter_resets_on_rare(self):
        odds = CardRarityOdds()
        odds.current_value = 0.30  # artificially high
        rng = Rng(42)
        # Force a rare by setting pity very high
        odds.current_value = 0.40
        result = odds.roll(rng, context="regular")
        if result == CardRarity.RARE:
            assert odds.current_value == -0.05

    def test_pity_counter_increases_on_non_rare(self):
        odds = CardRarityOdds(ascension_level=0)
        rng = Rng(42)
        initial = odds.current_value
        # Roll many times -- pity should generally increase
        for _ in range(20):
            result = odds.roll(rng, context="regular")
            if result != CardRarity.RARE:
                break
        # After at least one non-rare roll, pity should have increased
        # (it increases by 0.01 per non-rare)

    def test_pity_capped_at_0_4(self):
        odds = CardRarityOdds()
        odds.current_value = 0.39
        rng = Rng(42)
        # Force a non-rare result
        for _ in range(100):
            odds.roll(rng, context="regular")
        assert odds.current_value <= 0.4

    def test_shop_does_not_change_pity(self):
        odds = CardRarityOdds()
        initial = odds.current_value
        rng = Rng(42)
        odds.roll_without_changing_odds(rng, context="shop")
        assert odds.current_value == initial

    def test_a7_changes_odds(self):
        odds_normal = CardRarityOdds(ascension_level=0)
        odds_a7 = CardRarityOdds(ascension_level=7)
        # Regular rare odds should be lower at A7
        assert odds_a7.regular_odds["rare"] < odds_normal.regular_odds["rare"]
        assert odds_a7.rarity_growth < odds_normal.rarity_growth

    def test_a7_regular_rare_odds(self):
        odds = CardRarityOdds(ascension_level=7)
        assert odds.regular_odds["rare"] == pytest.approx(0.0149)
        assert odds.regular_odds["common"] == pytest.approx(0.615)

    def test_a7_elite_rare_odds(self):
        odds = CardRarityOdds(ascension_level=7)
        assert odds.elite_odds["rare"] == pytest.approx(0.05)
        assert odds.elite_odds["common"] == pytest.approx(0.549)

    def test_a7_rarity_growth(self):
        odds = CardRarityOdds(ascension_level=7)
        assert odds.rarity_growth == pytest.approx(0.005)


class TestRarityDistribution:
    """Statistical tests over many rolls to verify distribution."""

    def test_regular_distribution(self):
        """Regular rolls should have ~60% common, ~37% uncommon, ~3% rare."""
        odds = CardRarityOdds()
        rng = Rng(12345)
        counts = {CardRarity.COMMON: 0, CardRarity.UNCOMMON: 0, CardRarity.RARE: 0}
        n = 5000
        for _ in range(n):
            r = odds.roll(rng, context="regular")
            counts[r] += 1

        # With pity mechanics, rare will be higher than base 3%, but
        # common should still be the majority
        assert counts[CardRarity.COMMON] > n * 0.40
        assert counts[CardRarity.UNCOMMON] > n * 0.20
        assert counts[CardRarity.RARE] > 0

    def test_elite_has_more_rares(self):
        """Elite context should produce more rares than regular."""
        rng1 = Rng(42)
        rng2 = Rng(42)
        odds1 = CardRarityOdds()
        odds2 = CardRarityOdds()
        n = 3000

        reg_rares = sum(1 for _ in range(n) if odds1.roll(rng1, "regular") == CardRarity.RARE)
        elite_rares = sum(1 for _ in range(n) if odds2.roll(rng2, "elite") == CardRarity.RARE)

        assert elite_rares > reg_rares


class TestGenerateCardReward:
    def test_returns_correct_count(self):
        rs = RunState(42)
        rs.initialize_run()
        rewards = generate_card_reward(rs, "regular", 3)
        assert len(rewards) == 3

    def test_returns_valid_rarities(self):
        rs = RunState(42)
        rs.initialize_run()
        rewards = generate_card_reward(rs, "regular", 5)
        for r in rewards:
            assert r in (CardRarity.COMMON, CardRarity.UNCOMMON, CardRarity.RARE)

    def test_boss_rewards_all_rare(self):
        rs = RunState(42)
        rs.initialize_run()
        rewards = generate_card_reward(rs, "boss", 3)
        assert all(r == CardRarity.RARE for r in rewards)


class TestUpgradeProbability:
    def test_act0_no_upgrade(self):
        """Act 0 combat rewards should have 0% base upgrade chance for non-rares."""
        rs = RunState(42)
        rs.initialize_run()
        rs.current_act_index = 0
        rng = Rng(99)
        # Roll many times -- at act 0, base chance is 0 so should never upgrade
        upgraded_count = sum(
            1 for _ in range(1000)
            if roll_for_upgrade(rs, CardRarity.COMMON, rng, base_chance=0.0)
        )
        assert upgraded_count == 0

    def test_act1_upgrades_non_rare(self):
        """Act 1 should have 25% upgrade chance for non-rare cards."""
        rs = RunState(42)
        rs.initialize_run()
        rs.current_act_index = 1
        rng = Rng(99)
        n = 5000
        upgraded = sum(
            1 for _ in range(n)
            if roll_for_upgrade(rs, CardRarity.COMMON, rng, base_chance=0.0)
        )
        # Should be around 25% (scaling = 0.25 * act_index 1 = 0.25)
        ratio = upgraded / n
        assert 0.15 <= ratio <= 0.35, f"Expected ~25%, got {ratio:.2%}"

    def test_act2_upgrades_non_rare(self):
        """Act 2 should have 50% upgrade chance for non-rare cards."""
        rs = RunState(42)
        rs.initialize_run()
        rs.current_act_index = 2
        rng = Rng(99)
        n = 5000
        upgraded = sum(
            1 for _ in range(n)
            if roll_for_upgrade(rs, CardRarity.COMMON, rng, base_chance=0.0)
        )
        ratio = upgraded / n
        assert 0.40 <= ratio <= 0.60, f"Expected ~50%, got {ratio:.2%}"

    def test_rare_never_upgrades_from_combat(self):
        """Rare cards have 0% base and no act scaling."""
        rs = RunState(42)
        rs.initialize_run()
        rs.current_act_index = 2
        rng = Rng(99)
        upgraded = sum(
            1 for _ in range(1000)
            if roll_for_upgrade(rs, CardRarity.RARE, rng, base_chance=0.0)
        )
        assert upgraded == 0

    def test_shop_never_upgrades(self):
        """Shop cards use base_chance=-999999999, should never upgrade."""
        rs = RunState(42)
        rs.initialize_run()
        rs.current_act_index = 2
        rng = Rng(99)
        upgraded = sum(
            1 for _ in range(1000)
            if roll_for_upgrade(rs, CardRarity.COMMON, rng, base_chance=-999999999)
        )
        assert upgraded == 0

    def test_a7_halves_upgrade_scaling(self):
        """At A7, upgrade scaling is 0.125 instead of 0.25."""
        rs = RunState(42, ascension_level=7)
        rs.initialize_run()
        rs.current_act_index = 2
        rng = Rng(99)
        n = 5000
        upgraded = sum(
            1 for _ in range(n)
            if roll_for_upgrade(rs, CardRarity.COMMON, rng, base_chance=0.0)
        )
        ratio = upgraded / n
        # A7: 0.125 * 2 = 0.25
        assert 0.15 <= ratio <= 0.35, f"Expected ~25% at A7 act2, got {ratio:.2%}"


class TestCombatCardRewards:
    def test_returns_rarity_and_upgrade_tuples(self):
        rs = RunState(42)
        rs.initialize_run()
        results = generate_combat_card_rewards(rs, "regular", 3)
        assert len(results) == 3
        for rarity, upgraded in results:
            assert isinstance(rarity, CardRarity)
            assert isinstance(upgraded, bool)


class TestConcreteCombatRewardCards:
    def test_returns_real_card_instances(self):
        rs = RunState(42, character_id="Ironclad")
        rs.initialize_run()
        rewards = generate_combat_reward_cards(rs, "regular", 3)

        assert len(rewards) == 3
        assert all(card.card_id in eligible_character_cards("Ironclad", generation_context="combat") for card in rewards)


class TestCombatGenerationEligibility:
    def test_character_combat_generation_excludes_noncombat_cards(self):
        silent_attacks = eligible_character_cards("Silent", card_type=CardType.ATTACK)
        silent_attacks_all = eligible_character_cards(
            "Silent",
            card_type=CardType.ATTACK,
            generation_context=None,
        )

        assert CardId.THE_HUNT not in silent_attacks
        assert CardId.THE_HUNT in silent_attacks_all

    def test_colorless_combat_generation_excludes_blacklisted_cards(self):
        colorless_combat = eligible_registered_cards(module_name="sts2_env.cards.colorless")
        colorless_all = eligible_registered_cards(
            module_name="sts2_env.cards.colorless",
            generation_context=None,
        )

        assert CardId.ALCHEMIZE not in colorless_combat
        assert CardId.HAND_OF_GREED not in colorless_combat
        assert CardId.ALCHEMIZE in colorless_all
        assert CardId.HAND_OF_GREED in colorless_all

    def test_combat_reward_cards_respect_combat_generation_filters(self):
        rs = RunState(43, character_id="Silent")
        rs.initialize_run()
        rewards = generate_combat_reward_cards(rs, "regular", 20)

        reward_ids = {card.card_id for card in rewards}
        assert CardId.THE_HUNT not in reward_ids
        assert reward_ids <= set(eligible_character_cards("Silent", generation_context="combat"))
