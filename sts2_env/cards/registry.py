"""Card effect registry and dispatch."""

from __future__ import annotations

from typing import Callable, TYPE_CHECKING

from sts2_env.core.enums import CardId

if TYPE_CHECKING:
    from sts2_env.cards.base import CardInstance
    from sts2_env.core.combat import CombatState
    from sts2_env.core.creature import Creature

# Type alias for card effect functions
CardEffect = Callable[["CardInstance", "CombatState", "Creature | None"], None]
CardLateEffect = Callable[["CardInstance", "CardInstance", "CombatState"], None]

_CARD_EFFECTS: dict[CardId, CardEffect] = {}
_CARD_LATE_EFFECTS: dict[CardId, CardLateEffect] = {}


def register_effect(card_id: CardId):
    """Decorator to register a card's play effect function."""
    def decorator(func: CardEffect) -> CardEffect:
        _CARD_EFFECTS[card_id] = func
        return func
    return decorator


def register_late_effect(card_id: CardId):
    """Decorator to register a card's post-play late effect function."""
    def decorator(func: CardLateEffect) -> CardLateEffect:
        _CARD_LATE_EFFECTS[card_id] = func
        return func
    return decorator


def play_card_effect(
    card: "CardInstance",
    combat: "CombatState",
    target: "Creature | None",
) -> None:
    """Execute a card's effect. Raises KeyError if card has no registered effect."""
    effect = _CARD_EFFECTS.get(card.card_id)
    if effect is None:
        if card.is_unplayable or card.is_status:
            return  # Status/unplayable cards have no effect
        raise KeyError(f"No effect registered for {card.card_id}")
    try:
        effect(card, combat, target)
    finally:
        combat.flush_pending_attack_context()


def fire_card_late_effects(
    watched_card: "CardInstance",
    played_card: "CardInstance",
    combat: "CombatState",
) -> None:
    effect = _CARD_LATE_EFFECTS.get(watched_card.card_id)
    if effect is not None:
        effect(watched_card, played_card, combat)
