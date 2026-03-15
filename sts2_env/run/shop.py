"""Shop (Merchant) inventory generation and pricing.

Implements MerchantInventory.cs: 5 character cards, 2 colorless, 3 relics,
3 potions, card removal. Exact pricing formulas from decompiled source.
"""

from __future__ import annotations

import math
from dataclasses import dataclass, field
from typing import TYPE_CHECKING

from sts2_env.core.enums import CardRarity, PotionRarity, RelicRarity
from sts2_env.core.rng import Rng

if TYPE_CHECKING:
    from sts2_env.run.run_state import RunState


# ── Pricing ───────────────────────────────────────────────────────────

def card_base_cost(rarity: CardRarity) -> int:
    if rarity == CardRarity.RARE:
        return 150
    if rarity == CardRarity.UNCOMMON:
        return 75
    return 50  # Common/Basic


def card_price(rarity: CardRarity, rng: Rng, is_colorless: bool = False, on_sale: bool = False) -> int:
    """Calculate the price of a card in the shop."""
    base = card_base_cost(rarity)
    if is_colorless:
        base = round(base * 1.15)
    cost = round(base * rng.next_float_range(0.95, 1.05))
    if on_sale:
        cost //= 2
    return cost


def potion_price(rarity: PotionRarity, rng: Rng) -> int:
    """Calculate the price of a potion in the shop."""
    if rarity == PotionRarity.RARE:
        base = 100
    elif rarity == PotionRarity.UNCOMMON:
        base = 75
    else:
        base = 50
    return round(base * rng.next_float_range(0.95, 1.05))


def relic_price(merchant_cost: int, rng: Rng) -> int:
    """Calculate relic price (wider variance than cards: 0.85-1.15)."""
    return round(merchant_cost * rng.next_float_range(0.85, 1.15))


def card_removal_cost(removals_used: int) -> int:
    """75 + 25 * number of removals already used."""
    return 75 + 25 * removals_used


# ── Relic Rarity Roll ─────────────────────────────────────────────────

def roll_relic_rarity(rng: Rng) -> RelicRarity:
    """Roll a relic rarity: 50% Common, 33% Uncommon, 17% Rare."""
    roll = rng.next_float()
    if roll < 0.50:
        return RelicRarity.COMMON
    if roll < 0.83:
        return RelicRarity.UNCOMMON
    return RelicRarity.RARE


# ── Potion Rarity Roll ────────────────────────────────────────────────

def roll_potion_rarity(rng: Rng) -> PotionRarity:
    """Roll potion rarity: 65% Common, 25% Uncommon, 10% Rare."""
    roll = rng.next_float()
    if roll <= 0.10:
        return PotionRarity.RARE
    if roll <= 0.35:
        return PotionRarity.UNCOMMON
    return PotionRarity.COMMON


# ── Shop Inventory ────────────────────────────────────────────────────

SHOP_BLACKLISTED_RELICS = {"TheCourier", "OldCoin"}


@dataclass
class ShopCardEntry:
    rarity: CardRarity
    card_type: str  # "Attack", "Skill", "Power"
    is_colorless: bool = False
    on_sale: bool = False
    price: int = 0


@dataclass
class ShopRelicEntry:
    relic_rarity: RelicRarity
    price: int = 0


@dataclass
class ShopPotionEntry:
    potion_rarity: PotionRarity
    price: int = 0


@dataclass
class ShopInventory:
    """Generated shop inventory."""

    cards: list[ShopCardEntry] = field(default_factory=list)
    colorless_cards: list[ShopCardEntry] = field(default_factory=list)
    relics: list[ShopRelicEntry] = field(default_factory=list)
    potions: list[ShopPotionEntry] = field(default_factory=list)
    removal_cost: int = 75


def generate_shop_inventory(run_state: RunState) -> ShopInventory:
    """Generate a full merchant shop inventory.

    5 character cards (Attack, Attack, Skill, Skill, Power) -- 1 on sale.
    2 colorless cards (Uncommon, Rare).
    3 relics (2x rolled rarity + 1x Shop rarity).
    3 potions.
    Card removal.
    """
    rng = run_state.rng.shops
    inv = ShopInventory()

    # ── Character cards ───────────────────────────────────────────────
    card_types = ["Attack", "Attack", "Skill", "Skill", "Power"]
    sale_index = rng.next_int(0, 4)

    for i, ct in enumerate(card_types):
        rarity = run_state.card_rarity_odds.roll_without_changing_odds(rng, context="shop")
        on_sale = (i == sale_index)
        price = card_price(rarity, rng, is_colorless=False, on_sale=on_sale)
        inv.cards.append(ShopCardEntry(
            rarity=rarity,
            card_type=ct,
            is_colorless=False,
            on_sale=on_sale,
            price=price,
        ))

    # ── Colorless cards ───────────────────────────────────────────────
    for rarity in [CardRarity.UNCOMMON, CardRarity.RARE]:
        price = card_price(rarity, rng, is_colorless=True)
        inv.colorless_cards.append(ShopCardEntry(
            rarity=rarity,
            card_type="Colorless",
            is_colorless=True,
            price=price,
        ))

    # ── Relics ────────────────────────────────────────────────────────
    for _ in range(2):
        rr = roll_relic_rarity(rng)
        # Price would come from the relic model's MerchantCost
        # For now use a base cost by rarity
        base_costs = {RelicRarity.COMMON: 150, RelicRarity.UNCOMMON: 250, RelicRarity.RARE: 300}
        price = relic_price(base_costs.get(rr, 200), rng)
        inv.relics.append(ShopRelicEntry(relic_rarity=rr, price=price))

    # Shop-rarity relic
    inv.relics.append(ShopRelicEntry(
        relic_rarity=RelicRarity.SHOP,
        price=relic_price(150, rng),
    ))

    # ── Potions ───────────────────────────────────────────────────────
    for _ in range(3):
        pr = roll_potion_rarity(rng)
        price = potion_price(pr, rng)
        inv.potions.append(ShopPotionEntry(potion_rarity=pr, price=price))

    # ── Card removal ──────────────────────────────────────────────────
    inv.removal_cost = card_removal_cost(run_state.player.card_shop_removals_used)

    return inv
