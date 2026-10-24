"""Runtime helpers for persistent card enchantments."""

from __future__ import annotations

from typing import TYPE_CHECKING

from sts2_env.core.enums import CardRarity, CardTag, CardType, ValueProp
from sts2_env.core.damage import calculate_block

if TYPE_CHECKING:
    from sts2_env.cards.base import CardInstance
    from sts2_env.core.creature import Creature
    from sts2_env.core.combat import CombatState


ATTACK_ONLY_ENCHANTMENTS = frozenset({
    "Sharp",
    "Momentum",
    "Corrupted",
    "Favored",
    "Vigorous",
    "Instinct",
})


def _used_key(name: str) -> str:
    return f"_enchant_used_{name.lower()}"


def _gain_block(owner: Creature, amount: int, combat: CombatState) -> int:
    before = owner.block
    owner.gain_block(amount)
    gained = owner.block - before
    if gained > 0:
        from sts2_env.core.hooks import fire_after_block_gained

        fire_after_block_gained(owner, gained, combat)
    return gained


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


def should_start_at_bottom_of_draw_pile(card: CardInstance) -> bool:
    """C# EnchantmentModel.ShouldStartAtBottomOfDrawPile (EnchantmentModel.cs:125).

    On turn 1 the draw pile is ordered so enchanted cards with this flag sit at
    the bottom before Innate cards are moved to the top (CombatManager.cs:659-664).
    Only Imbued overrides it to true in v0.107.1 (Imbued.cs:11).
    """
    return card.has_enchantment("Imbued")


def can_enchant_card(card: CardInstance, name: str) -> bool:
    if card.card_type in {CardType.STATUS, CardType.CURSE, CardType.QUEST}:
        return False
    if card.is_unplayable:
        return False
    if card.is_enchanted and name not in card.enchantments:
        return False

    if name in ATTACK_ONLY_ENCHANTMENTS:
        return card.card_type == CardType.ATTACK
    if name == "RoyallyApproved":
        return card.card_type in {CardType.ATTACK, CardType.SKILL}
    if name == "Imbued":
        return card.card_type == CardType.SKILL
    if name == "Spiral":
        return card.rarity == CardRarity.BASIC and (
            CardTag.STRIKE in card.tags
            or CardTag.DEFEND in card.tags
        )
    if name == "Nimble":
        return card.gains_block
    if name == "Goopy":
        return CardTag.DEFEND in card.tags
    if name == "SoulsPower":
        return card.exhausts
    if name == "Slither":
        return not card.has_energy_cost_x
    if name == "TezcatarasEmber":
        return card.rarity == CardRarity.BASIC and CardTag.STRIKE in card.tags
    return True


def reset_combat_enchantments(card: CardInstance) -> None:
    for key in list(card.combat_vars):
        if key.startswith("_enchant_"):
            card.combat_vars.pop(key, None)


def enchant_damage_additive(card: CardInstance, props: ValueProp) -> int:
    if not props.is_powered_attack():
        return 0
    total = 0
    total += card.enchantments.get("Sharp", 0)
    total += card.combat_vars.get("_enchant_momentum_bonus", 0)
    if not card.combat_vars.get(_used_key("Vigorous")):
        total += card.enchantments.get("Vigorous", 0)
    return total


def enchant_damage_multiplicative(card: CardInstance, props: ValueProp) -> float:
    if not props.is_powered_attack():
        return 1.0
    if card.has_enchantment("Favored"):
        return 2.0
    if card.has_enchantment("Corrupted"):
        return 1.5
    return 1.0


def enchant_block_additive(card: CardInstance, props: ValueProp) -> int:
    if not props.is_powered_card_or_monster_move_block():
        return 0
    total = 0
    total += card.enchantments.get("Nimble", 0)
    if card.has_enchantment("Goopy"):
        total += max(0, card.enchantments.get("Goopy", 1) - 1)
    return total


def enchant_block_multiplicative(card: CardInstance, props: ValueProp) -> float:
    """C# EnchantmentModel.EnchantBlockMultiplicative (EnchantmentModel.cs:418).

    No v0.107.1 enchantment overrides it (all return the 1m default), so this
    stays 1.0 until one is added; it must be applied together with the
    additive enchantment pass BEFORE the listener passes (Hook.cs:1316-1322).
    """
    if not props.is_powered_card_or_monster_move_block():
        return 1.0
    return 1.0


def enchant_play_count_bonus(card: CardInstance) -> int:
    bonus = 0
    bonus += card.enchantments.get("Spiral", 0)
    if card.has_enchantment("Glam") and not card.combat_vars.get(_used_key("Glam")):
        bonus += card.enchantments.get("Glam", 0)
    return bonus


def on_card_drawn(card: CardInstance, combat: CombatState, from_hand_draw: bool) -> None:
    if card.has_enchantment("Slither"):
        card.set_combat_cost(combat.combat_energy_costs_rng.next_int(0, 3))


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
    owner = (
        getattr(card, "owner", None)
        or getattr(getattr(combat, "active_card_source", None), "owner", None)
        or combat.primary_player
    )
    if card.has_enchantment("Adroit"):
        block = calculate_block(card.enchantments["Adroit"], owner, ValueProp.MOVE, combat, card_source=card)
        _gain_block(owner, block, combat)
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
