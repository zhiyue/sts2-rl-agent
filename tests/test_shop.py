"""Tests for shop (merchant) system.

Verifies inventory composition, pricing formulas, and card removal cost.
"""

import pytest

from sts2_env.core.enums import CardRarity, PotionRarity, RelicRarity
from sts2_env.core.rng import Rng
from sts2_env.run.shop import (
    card_base_cost,
    card_price,
    card_removal_cost,
    generate_shop_inventory,
    potion_price,
    relic_price,
    roll_potion_rarity,
    roll_relic_rarity,
    ShopInventory,
    SHOP_BLACKLISTED_RELICS,
)
from sts2_env.run.run_state import RunState


class TestShopInventoryComposition:
    def test_five_character_cards(self):
        rs = RunState(42)
        rs.initialize_run()
        inv = generate_shop_inventory(rs)
        assert len(inv.cards) == 5

    def test_character_card_types(self):
        rs = RunState(42)
        rs.initialize_run()
        inv = generate_shop_inventory(rs)
        types = [c.card_type for c in inv.cards]
        assert types.count("Attack") == 2
        assert types.count("Skill") == 2
        assert types.count("Power") == 1

    def test_two_colorless_cards(self):
        rs = RunState(42)
        rs.initialize_run()
        inv = generate_shop_inventory(rs)
        assert len(inv.colorless_cards) == 2

    def test_colorless_rarities(self):
        rs = RunState(42)
        rs.initialize_run()
        inv = generate_shop_inventory(rs)
        rarities = [c.rarity for c in inv.colorless_cards]
        assert CardRarity.UNCOMMON in rarities
        assert CardRarity.RARE in rarities

    def test_three_relics(self):
        rs = RunState(42)
        rs.initialize_run()
        inv = generate_shop_inventory(rs)
        assert len(inv.relics) == 3

    def test_one_shop_rarity_relic(self):
        rs = RunState(42)
        rs.initialize_run()
        inv = generate_shop_inventory(rs)
        shop_relics = [r for r in inv.relics if r.relic_rarity == RelicRarity.SHOP]
        assert len(shop_relics) == 1

    def test_three_potions(self):
        rs = RunState(42)
        rs.initialize_run()
        inv = generate_shop_inventory(rs)
        assert len(inv.potions) == 3

    def test_exactly_one_sale_card(self):
        rs = RunState(42)
        rs.initialize_run()
        inv = generate_shop_inventory(rs)
        on_sale = [c for c in inv.cards if c.on_sale]
        assert len(on_sale) == 1


class TestCardPricing:
    def test_rare_base_cost(self):
        assert card_base_cost(CardRarity.RARE) == 150

    def test_uncommon_base_cost(self):
        assert card_base_cost(CardRarity.UNCOMMON) == 75

    def test_common_base_cost(self):
        assert card_base_cost(CardRarity.COMMON) == 50

    def test_basic_base_cost(self):
        assert card_base_cost(CardRarity.BASIC) == 50

    def test_price_within_variance(self):
        """Price should be within 95%-105% of base."""
        rng = Rng(42)
        for _ in range(100):
            price = card_price(CardRarity.UNCOMMON, rng)
            # 75 * 0.95 = 71.25, 75 * 1.05 = 78.75
            assert 71 <= price <= 79, f"Price {price} outside expected range"

    def test_colorless_markup(self):
        """Colorless cards cost 15% more."""
        rng1 = Rng(42)
        rng2 = Rng(42)
        normal = card_price(CardRarity.UNCOMMON, rng1, is_colorless=False)
        colorless = card_price(CardRarity.UNCOMMON, rng2, is_colorless=True)
        # Colorless base = round(75 * 1.15) = 86
        # So colorless should be higher
        assert colorless > normal

    def test_sale_halves_price(self):
        rng1 = Rng(42)
        rng2 = Rng(42)
        full = card_price(CardRarity.RARE, rng1, on_sale=False)
        sale = card_price(CardRarity.RARE, rng2, on_sale=True)
        assert sale == full // 2

    def test_all_shop_cards_have_positive_price(self):
        rs = RunState(42)
        rs.initialize_run()
        inv = generate_shop_inventory(rs)
        for c in inv.cards + inv.colorless_cards:
            assert c.price > 0


class TestRelicPricing:
    def test_relic_price_variance(self):
        """Relic price should be within 85%-115% of merchant cost."""
        rng = Rng(42)
        base = 200
        for _ in range(100):
            price = relic_price(base, rng)
            assert 170 <= price <= 230, f"Relic price {price} outside expected range"

    def test_all_shop_relics_have_positive_price(self):
        rs = RunState(42)
        rs.initialize_run()
        inv = generate_shop_inventory(rs)
        for r in inv.relics:
            assert r.price > 0


class TestPotionPricing:
    def test_rare_potion_base(self):
        rng = Rng(42)
        for _ in range(50):
            price = potion_price(PotionRarity.RARE, rng)
            assert 95 <= price <= 105

    def test_uncommon_potion_base(self):
        rng = Rng(42)
        for _ in range(50):
            price = potion_price(PotionRarity.UNCOMMON, rng)
            assert 71 <= price <= 79

    def test_common_potion_base(self):
        rng = Rng(42)
        for _ in range(50):
            price = potion_price(PotionRarity.COMMON, rng)
            assert 47 <= price <= 53


class TestCardRemovalCost:
    def test_first_removal(self):
        assert card_removal_cost(0) == 75

    def test_second_removal(self):
        assert card_removal_cost(1) == 100

    def test_third_removal(self):
        assert card_removal_cost(2) == 125

    def test_escalation_formula(self):
        for i in range(10):
            assert card_removal_cost(i) == 75 + 25 * i

    def test_shop_tracks_removal_cost(self):
        rs = RunState(42)
        rs.initialize_run()
        inv = generate_shop_inventory(rs)
        assert inv.removal_cost == 75

        rs.player.card_shop_removals_used = 3
        inv2 = generate_shop_inventory(rs)
        assert inv2.removal_cost == 150  # 75 + 25*3


class TestRelicRarityRoll:
    def test_distribution(self):
        """50% Common, 33% Uncommon, 17% Rare."""
        rng = Rng(42)
        n = 10000
        counts = {RelicRarity.COMMON: 0, RelicRarity.UNCOMMON: 0, RelicRarity.RARE: 0}
        for _ in range(n):
            counts[roll_relic_rarity(rng)] += 1

        assert 0.42 <= counts[RelicRarity.COMMON] / n <= 0.58
        assert 0.25 <= counts[RelicRarity.UNCOMMON] / n <= 0.41
        assert 0.10 <= counts[RelicRarity.RARE] / n <= 0.24


class TestPotionRarityRoll:
    def test_distribution(self):
        """65% Common, 25% Uncommon, 10% Rare."""
        rng = Rng(42)
        n = 10000
        counts = {PotionRarity.COMMON: 0, PotionRarity.UNCOMMON: 0, PotionRarity.RARE: 0}
        for _ in range(n):
            counts[roll_potion_rarity(rng)] += 1

        assert 0.57 <= counts[PotionRarity.COMMON] / n <= 0.73
        assert 0.18 <= counts[PotionRarity.UNCOMMON] / n <= 0.32
        assert 0.04 <= counts[PotionRarity.RARE] / n <= 0.16


class TestShopBlacklist:
    def test_blacklisted_relics(self):
        assert "TheCourier" in SHOP_BLACKLISTED_RELICS
        assert "OldCoin" in SHOP_BLACKLISTED_RELICS


class TestShopDeterminism:
    def test_same_seed_same_shop(self):
        rs1 = RunState(42)
        rs1.initialize_run()
        rs2 = RunState(42)
        rs2.initialize_run()

        inv1 = generate_shop_inventory(rs1)
        inv2 = generate_shop_inventory(rs2)

        assert len(inv1.cards) == len(inv2.cards)
        for c1, c2 in zip(inv1.cards, inv2.cards):
            assert c1.rarity == c2.rarity
            assert c1.card_type == c2.card_type
            assert c1.on_sale == c2.on_sale
            assert c1.price == c2.price


class TestShopConcreteItems:
    def test_shop_entries_have_real_ids(self):
        rs = RunState(42)
        rs.initialize_run()
        inv = generate_shop_inventory(rs)

        assert all(entry.card_id for entry in inv.cards + inv.colorless_cards)
        assert all(entry.relic_id for entry in inv.relics)
        assert all(entry.potion_id for entry in inv.potions)
