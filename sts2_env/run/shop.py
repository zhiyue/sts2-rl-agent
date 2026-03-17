"""Shop (Merchant) inventory generation and pricing.

Implements MerchantInventory.cs: 5 character cards, 2 colorless, 3 relics,
3 potions, card removal. Exact pricing formulas from decompiled source.
"""

from __future__ import annotations

from dataclasses import dataclass, field
from typing import TYPE_CHECKING

from sts2_env.cards.base import CardInstance
from sts2_env.cards.factory import create_card, eligible_character_cards, eligible_registered_cards
from sts2_env.core.enums import CardRarity, CardType, PotionRarity, RelicRarity
from sts2_env.core.rng import Rng
from sts2_env.potions.base import normal_pool_models
from sts2_env.relics.base import RelicPool
from sts2_env.relics.registry import RELIC_REGISTRY, load_all_relics

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

SHOP_BLACKLISTED_RELICS = {"TheCourier", "THE_COURIER", "OldCoin", "OLD_COIN"}


@dataclass
class ShopCardEntry:
    rarity: CardRarity
    card_type: str  # "Attack", "Skill", "Power"
    card_id: str = ""
    card: CardInstance | None = None
    is_colorless: bool = False
    on_sale: bool = False
    price: int = 0


@dataclass
class ShopRelicEntry:
    relic_rarity: RelicRarity
    relic_id: str = ""
    price: int = 0


@dataclass
class ShopPotionEntry:
    potion_rarity: PotionRarity
    potion_id: str = ""
    price: int = 0


@dataclass
class ShopInventory:
    """Generated shop inventory."""

    cards: list[ShopCardEntry] = field(default_factory=list)
    colorless_cards: list[ShopCardEntry] = field(default_factory=list)
    relics: list[ShopRelicEntry] = field(default_factory=list)
    potions: list[ShopPotionEntry] = field(default_factory=list)
    removal_cost: int = 75


def apply_merchant_price_modifiers(
    run_state: RunState,
    price: int,
    *,
    item_kind: str,
    item: object,
) -> int:
    player = run_state.player
    result = price
    for relic in player.get_relic_objects():
        result = relic.modify_merchant_price(
            player,
            result,
            item_kind=item_kind,
            item=item,
            run_state=run_state,
        )
    return max(0, result)


def apply_merchant_card_creation_results(
    run_state: RunState,
    card: CardInstance,
    *,
    is_colorless: bool,
) -> CardInstance:
    player = run_state.player
    result = card
    for relic in player.get_relic_objects():
        updated = relic.modify_merchant_card_creation_results(
            player,
            result,
            is_colorless=is_colorless,
            run_state=run_state,
        )
        if updated is not None:
            result = updated
    return result


def _create_character_shop_card(
    run_state: RunState,
    rng: Rng,
    card_type_name: str,
    rarity: CardRarity,
    *,
    on_sale: bool = False,
) -> ShopCardEntry:
    type_map = {"Attack": CardType.ATTACK, "Skill": CardType.SKILL, "Power": CardType.POWER}
    card_type = type_map[card_type_name]
    candidates = eligible_character_cards(
        run_state.player.character_id,
        card_type=card_type,
        rarity=rarity,
        generation_context=None,
    )
    if not candidates:
        candidates = eligible_character_cards(
            run_state.player.character_id,
            card_type=card_type,
            generation_context=None,
        )
    card_id = rng.choice(candidates) if candidates else None
    card = None
    if card_id is not None:
        card = apply_merchant_card_creation_results(
            run_state,
            create_card(card_id),
            is_colorless=False,
        )
    entry = ShopCardEntry(
        rarity=rarity,
        card_type=card_type_name,
        card_id=card.card_id.name if card is not None else "",
        card=card,
        is_colorless=False,
        on_sale=on_sale,
        price=0,
    )
    entry.price = apply_merchant_price_modifiers(
        run_state,
        card_price(rarity, rng, is_colorless=False, on_sale=on_sale),
        item_kind="card",
        item=entry,
    )
    return entry


def _create_colorless_shop_card(
    run_state: RunState,
    rng: Rng,
    rarity: CardRarity,
) -> ShopCardEntry:
    candidates = eligible_registered_cards(
        module_name="sts2_env.cards.colorless",
        rarity=rarity,
        generation_context=None,
    )
    card_id = rng.choice(candidates) if candidates else None
    card = None
    if card_id is not None:
        card = apply_merchant_card_creation_results(
            run_state,
            create_card(card_id),
            is_colorless=True,
        )
    entry = ShopCardEntry(
        rarity=rarity,
        card_type="Colorless",
        card_id=card.card_id.name if card is not None else "",
        card=card,
        is_colorless=True,
        on_sale=False,
        price=0,
    )
    entry.price = apply_merchant_price_modifiers(
        run_state,
        card_price(rarity, rng, is_colorless=True),
        item_kind="card",
        item=entry,
    )
    return entry


def _create_relic_shop_entry(
    run_state: RunState,
    rng: Rng,
    relic_rarity: RelicRarity,
    *,
    owned: set[str],
) -> ShopRelicEntry:
    desired_pool = getattr(RelicPool, run_state.player.character_id.upper(), None)
    candidates: list[str] = []
    for relic_id, relic_cls in RELIC_REGISTRY.items():
        if relic_id.name in owned or relic_id.name in SHOP_BLACKLISTED_RELICS:
            continue
        if relic_cls.pool in {RelicPool.EVENT, RelicPool.FALLBACK, RelicPool.DEPRECATED}:
            continue
        if relic_rarity == RelicRarity.SHOP:
            if relic_cls.rarity is not RelicRarity.SHOP:
                continue
        elif relic_cls.rarity is not relic_rarity:
            continue
        if desired_pool is not None and relic_cls.pool not in {RelicPool.SHARED, desired_pool} and relic_cls.rarity is not RelicRarity.SHOP:
            continue
        candidates.append(relic_id.name)
    chosen = rng.choice(candidates) if candidates else ""
    if chosen:
        owned.add(chosen)
    base_costs = {RelicRarity.COMMON: 150, RelicRarity.UNCOMMON: 250, RelicRarity.RARE: 300, RelicRarity.SHOP: 150}
    entry = ShopRelicEntry(
        relic_rarity=relic_rarity,
        relic_id=chosen,
        price=0,
    )
    entry.price = apply_merchant_price_modifiers(
        run_state,
        relic_price(base_costs.get(relic_rarity, 200), rng),
        item_kind="relic",
        item=entry,
    )
    return entry


def _create_potion_shop_entry(
    run_state: RunState,
    rng: Rng,
    potion_rarity: PotionRarity,
) -> ShopPotionEntry:
    models = [
        model for model in normal_pool_models(in_combat=False, character_id=run_state.player.character_id)
        if model.rarity == potion_rarity
    ]
    entry = ShopPotionEntry(
        potion_rarity=potion_rarity,
        potion_id=rng.choice(models).potion_id if models else "",
        price=0,
    )
    entry.price = apply_merchant_price_modifiers(
        run_state,
        potion_price(potion_rarity, rng),
        item_kind="potion",
        item=entry,
    )
    return entry


def refill_shop_entry(
    inventory: ShopInventory,
    item_kind: str,
    entry: object,
    run_state: RunState,
) -> None:
    rng = run_state.rng.shops
    if item_kind == "card":
        if entry in inventory.cards:
            index = inventory.cards.index(entry)
            inventory.cards[index] = _create_character_shop_card(
                run_state,
                rng,
                getattr(entry, "card_type", "Attack"),
                getattr(entry, "rarity", CardRarity.COMMON),
                on_sale=bool(getattr(entry, "on_sale", False)),
            )
            return
        if entry in inventory.colorless_cards:
            index = inventory.colorless_cards.index(entry)
            inventory.colorless_cards[index] = _create_colorless_shop_card(
                run_state,
                rng,
                getattr(entry, "rarity", CardRarity.UNCOMMON),
            )
            return
    elif item_kind == "relic" and entry in inventory.relics:
        owned = set(run_state.player.relics) | {e.relic_id for e in inventory.relics if e.relic_id}
        inventory.relics[inventory.relics.index(entry)] = _create_relic_shop_entry(
            run_state,
            rng,
            getattr(entry, "relic_rarity", RelicRarity.COMMON),
            owned=owned,
        )
        return
    elif item_kind == "potion" and entry in inventory.potions:
        inventory.potions[inventory.potions.index(entry)] = _create_potion_shop_entry(
            run_state,
            rng,
            getattr(entry, "potion_rarity", PotionRarity.COMMON),
        )


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
        inv.cards.append(_create_character_shop_card(
            run_state,
            rng,
            ct,
            rarity,
            on_sale=(i == sale_index),
        ))

    # ── Colorless cards ───────────────────────────────────────────────
    for rarity in [CardRarity.UNCOMMON, CardRarity.RARE]:
        inv.colorless_cards.append(_create_colorless_shop_card(run_state, rng, rarity))

    # ── Relics ────────────────────────────────────────────────────────
    load_all_relics()
    owned = set(run_state.player.relics)

    for _ in range(2):
        rr = roll_relic_rarity(rng)
        inv.relics.append(_create_relic_shop_entry(run_state, rng, rr, owned=owned))

    # Shop-rarity relic
    inv.relics.append(_create_relic_shop_entry(run_state, rng, RelicRarity.SHOP, owned=owned))

    # ── Potions ───────────────────────────────────────────────────────
    for _ in range(3):
        pr = roll_potion_rarity(rng)
        inv.potions.append(_create_potion_shop_entry(run_state, rng, pr))

    # ── Card removal ──────────────────────────────────────────────────
    inv.removal_cost = apply_merchant_price_modifiers(
        run_state,
        card_removal_cost(run_state.player.card_shop_removals_used),
        item_kind="remove_card",
        item=inv,
    )

    for relic in run_state.player.get_relic_objects():
        inv = relic.modify_merchant_inventory(run_state.player, inv, run_state)

    return inv
