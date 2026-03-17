"""Runtime helpers for persistent card enchantments."""

from __future__ import annotations

from typing import TYPE_CHECKING

from sts2_env.core.enums import CardRarity, CardTag, CardType, ValueProp
from sts2_env.core.damage import calculate_block

if TYPE_CHECKING:
    from sts2_env.cards.base import CardInstance
    from sts2_env.core.combat import CombatState


def _used_key(name: str) -> str:
    return f"_enchant_used_{name.lower()}"


def apply_static_enchantment(card: CardInstance, name: str, amount: int = 1) -> None:
    card.enchantments[name] = amount
    if name == "RoyallyApproved":
        card.keywords = frozenset(set(card.keywords) | {"innate", "retain"})
    elif name == "Steady":
        card.keywords = frozenset(set(card.keywords) | {"retain"})
    elif name == "TezcatarasEmber":
        card.keywords = frozenset(set(card.keywords) | {"eternal"})
        card.cost = 0
        card.original_cost = 0
    elif name == "SoulsPower":
        card.keywords = frozenset(keyword for keyword in card.keywords if keyword != "exhaust")
    elif name == "Instinct":
        if not card.has_energy_cost_x and card.cost > 0:
            card.cost = max(0, card.cost - 1)
            if card.original_cost is not None:
                card.original_cost = max(0, card.original_cost - 1)
    elif name == "Goopy":
        card.keywords = frozenset(set(card.keywords) | {"exhaust"})


def can_enchant_card(card: CardInstance, name: str) -> bool:
    if card.card_type in {CardType.STATUS, CardType.CURSE, CardType.QUEST}:
        return False
    if card.is_unplayable:
        return False
    if card.is_enchanted and name not in card.enchantments:
        return False

    if name in {"Sharp", "Momentum", "Corrupted", "Favored", "Vigorous", "Instinct"}:
        return card.card_type == CardType.ATTACK
    if name == "RoyallyApproved":
        return card.card_type in {CardType.ATTACK, CardType.SKILL}
    if name == "Imbued":
        return card.card_type == CardType.SKILL
    if name == "Spiral":
        return card.rarity == CardRarity.BASIC and (
            CardTag.STRIKE in card.tags
            or CardTag.DEFEND in card.tags
            or "STRIKE" in card.card_id.name
            or "DEFEND" in card.card_id.name
        )
    if name == "Nimble":
        return card.base_block is not None
    if name == "Goopy":
        return CardTag.DEFEND in card.tags or "DEFEND" in card.card_id.name
    if name == "SoulsPower":
        return card.exhausts
    if name == "Slither":
        return not card.has_energy_cost_x
    if name == "TezcatarasEmber":
        return card.rarity == CardRarity.BASIC and (
            CardTag.STRIKE in card.tags or "STRIKE" in card.card_id.name
        )
    return True


def reset_combat_enchantments(card: CardInstance) -> None:
    for key in list(card.combat_vars):
        if key.startswith("_enchant_"):
            card.combat_vars.pop(key, None)


def enchant_damage_additive(card: CardInstance, props: ValueProp) -> int:
    if not props.is_powered():
        return 0
    total = 0
    total += card.enchantments.get("Sharp", 0)
    total += card.combat_vars.get("_enchant_momentum_bonus", 0)
    if not card.combat_vars.get(_used_key("Vigorous")):
        total += card.enchantments.get("Vigorous", 0)
    return total


def enchant_damage_multiplicative(card: CardInstance, props: ValueProp) -> float:
    if not props.is_powered():
        return 1.0
    if card.has_enchantment("Favored"):
        return 2.0
    if card.has_enchantment("Corrupted"):
        return 1.5
    return 1.0


def enchant_block_additive(card: CardInstance, props: ValueProp) -> int:
    if not props.is_powered():
        return 0
    total = 0
    total += card.enchantments.get("Nimble", 0)
    if card.has_enchantment("Goopy"):
        total += max(0, card.enchantments.get("Goopy", 1) - 1)
    return total


def enchant_play_count_bonus(card: CardInstance) -> int:
    bonus = 0
    bonus += card.enchantments.get("Spiral", 0)
    if card.has_enchantment("Glam") and not card.combat_vars.get(_used_key("Glam")):
        bonus += card.enchantments.get("Glam", 0)
    return bonus


def on_card_drawn(card: CardInstance, combat: CombatState, from_hand_draw: bool) -> None:
    if card.has_enchantment("Slither"):
        card.set_combat_cost(combat.rng.next_int(0, 3))


def after_player_turn_start(owner, combat: CombatState) -> None:
    if combat.round_number != 1:
        return
    state = combat.combat_player_state_for(owner)
    if state is None:
        return
    imbued_cards = [card for card in list(state.draw) if card.has_enchantment("Imbued")]
    for card in imbued_cards:
        combat.auto_play_card(card)


def before_flush(owner, combat: CombatState) -> None:
    state = combat.combat_player_state_for(owner)
    if state is None:
        return
    for card in list(state.hand):
        if not card.has_enchantment("SlumberingEssence"):
            continue
        if card.has_energy_cost_x:
            continue
        card.set_combat_cost(max(0, card.cost - 1))


def modify_shuffle_order(cards: list[CardInstance], *, is_initial_shuffle: bool) -> None:
    if is_initial_shuffle:
        return
    perfect_fit = [card for card in cards if card.has_enchantment("PerfectFit")]
    if perfect_fit:
        remaining = [card for card in cards if not card.has_enchantment("PerfectFit")]
        cards[:] = perfect_fit + remaining


def on_card_played(card: CardInstance, combat: CombatState) -> None:
    owner = getattr(card, "owner", None) or combat.player
    if card.has_enchantment("Adroit"):
        block = calculate_block(card.enchantments["Adroit"], owner, ValueProp.MOVE, combat, card_source=card)
        owner.gain_block(block)
    if card.has_enchantment("Swift") and not card.combat_vars.get(_used_key("Swift")):
        combat.draw_cards(owner, card.enchantments["Swift"])
    if card.has_enchantment("Sown") and not card.combat_vars.get(_used_key("Sown")):
        combat.gain_energy(owner, card.enchantments["Sown"])
    if card.has_enchantment("Corrupted"):
        combat.deal_damage(
            dealer=None,
            target=owner,
            amount=2,
            props=ValueProp.UNBLOCKABLE | ValueProp.UNPOWERED | ValueProp.MOVE,
        )
    if card.has_enchantment("Momentum"):
        card.combat_vars["_enchant_momentum_bonus"] = (
            card.combat_vars.get("_enchant_momentum_bonus", 0) + card.enchantments["Momentum"]
        )


def after_card_played(card: CardInstance) -> None:
    for name in ("Swift", "Sown", "Vigorous", "Glam"):
        if card.has_enchantment(name):
            card.combat_vars[_used_key(name)] = 1
    if card.has_enchantment("Goopy"):
        card.enchantments["Goopy"] = card.enchantments.get("Goopy", 1) + 1
