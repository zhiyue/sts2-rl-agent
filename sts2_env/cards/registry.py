"""Card effect registry and dispatch."""

from __future__ import annotations

from typing import Callable, TYPE_CHECKING

from sts2_env.core.enums import CardId

if TYPE_CHECKING:
    from sts2_env.cards.base import CardInstance
    from sts2_env.core.combat import CombatState
    from sts2_env.core.creature import Creature
    from sts2_env.core.enums import CombatSide, RoomType, TargetType
    from sts2_env.map.generator import ActMap
    from sts2_env.run.events import EventModel
    from sts2_env.run.rest_site import RestSiteOption
    from sts2_env.run.run_state import PlayerState, RunState

# Type alias for card effect functions
CardEffect = Callable[["CardInstance", "CombatState", "Creature | None"], None]
CardLateEffect = Callable[["CardInstance", "CardInstance", "CombatState"], None]
CardChosenHook = Callable[
    ["CardInstance", "CombatState"],
    None,
]
CardPlayabilityHook = Callable[
    ["CardInstance", object, "CombatState", "Creature"],
    bool,
]
CardShouldPlayHook = Callable[
    ["CardInstance", "CardInstance", object, "CombatState", "Creature", bool],
    bool,
]
CardTargetTypeHook = Callable[
    ["CardInstance", "Creature", "TargetType"],
    "TargetType",
]
CardGainsBlockHook = Callable[
    ["CardInstance", bool],
    bool,
]
CardAfterCombatEndHook = Callable[
    ["CardInstance", "PlayerState"],
    bool,
]
CardNextEventHook = Callable[
    ["CardInstance", "RunState", "EventModel | None"],
    "EventModel | None",
]
CardUnknownRoomTypesHook = Callable[
    ["CardInstance", "RunState", set["RoomType"]],
    set["RoomType"],
]
CardGeneratedMapHook = Callable[
    ["CardInstance", "RunState", "ActMap", int],
    "ActMap",
]
CardAfterMapGeneratedHook = Callable[
    ["CardInstance", "RunState", "ActMap", int],
    None,
]
CardQuestCompleteHook = Callable[
    ["CardInstance", "RunState"],
    int,
]
CardRestSiteOptionsHook = Callable[
    ["CardInstance", "PlayerState", list["RestSiteOption"], "RunState | None"],
    list["RestSiteOption"],
]
CardBeforeHandDrawHook = Callable[
    ["CardInstance", "Creature", "CombatState"],
    None,
]
CardAfterTurnEndHook = Callable[
    ["CardInstance", "CombatSide", "CombatState"],
    None,
]
CardAfterCardDrawnHook = Callable[
    ["CardInstance", "CardInstance", bool, "CombatState"],
    None,
]
CardAfterCardExhaustedHook = Callable[
    ["CardInstance", "CardInstance", "CombatState"],
    None,
]
CardAfterAutoPostPlayHook = Callable[
    ["CardInstance", "Creature", "CombatState"],
    None,
]
CardTurnEndInHandHook = Callable[
    ["CardInstance", "CombatState", int],
    None,
]
CardAfterCardGeneratedForCombatHook = Callable[
    ["CardInstance", "CardInstance", bool, "CombatState"],
    None,
]
CardAfterCardEnteredCombatHook = Callable[
    ["CardInstance", "CardInstance", "Creature", "CombatState"],
    None,
]
CardCardPlayHook = Callable[
    ["CardInstance", "CardInstance", "Creature", "CombatState"],
    None,
]
CardAfterAttackHook = Callable[
    ["CardInstance", object, "CombatState"],
    None,
]
CardAfterDeathHook = Callable[
    ["CardInstance", "Creature", bool, "CombatState"],
    None,
]

_CARD_EFFECTS: dict[CardId, CardEffect] = {}
_CARD_LATE_EFFECTS: dict[CardId, CardLateEffect] = {}
_CARD_CHOSEN_HOOKS: dict[CardId, CardChosenHook] = {}
_CARD_PLAYABILITY_HOOKS: dict[CardId, CardPlayabilityHook] = {}
_CARD_SHOULD_PLAY_HOOKS: dict[CardId, CardShouldPlayHook] = {}
_CARD_TARGET_TYPE_HOOKS: dict[CardId, CardTargetTypeHook] = {}
_CARD_GAINS_BLOCK_HOOKS: dict[CardId, CardGainsBlockHook] = {}
_CARD_AFTER_COMBAT_END_HOOKS: dict[CardId, CardAfterCombatEndHook] = {}
_CARD_NEXT_EVENT_HOOKS: dict[CardId, CardNextEventHook] = {}
_CARD_UNKNOWN_ROOM_TYPES_HOOKS: dict[CardId, CardUnknownRoomTypesHook] = {}
_CARD_GENERATED_MAP_HOOKS: dict[CardId, CardGeneratedMapHook] = {}
_CARD_GENERATED_MAP_LATE_HOOKS: dict[CardId, CardGeneratedMapHook] = {}
_CARD_AFTER_MAP_GENERATED_HOOKS: dict[CardId, CardAfterMapGeneratedHook] = {}
_CARD_QUEST_COMPLETE_HOOKS: dict[CardId, CardQuestCompleteHook] = {}
_CARD_REST_SITE_OPTIONS_HOOKS: dict[CardId, CardRestSiteOptionsHook] = {}
_CARD_BEFORE_HAND_DRAW_HOOKS: dict[CardId, CardBeforeHandDrawHook] = {}
_CARD_AFTER_TURN_END_HOOKS: dict[CardId, CardAfterTurnEndHook] = {}
_CARD_AFTER_CARD_DRAWN_HOOKS: dict[CardId, CardAfterCardDrawnHook] = {}
_CARD_AFTER_CARD_EXHAUSTED_HOOKS: dict[CardId, CardAfterCardExhaustedHook] = {}
_CARD_AFTER_AUTO_POST_PLAY_HOOKS: dict[CardId, CardAfterAutoPostPlayHook] = {}
_CARD_TURN_END_IN_HAND_HOOKS: dict[CardId, CardTurnEndInHandHook] = {}
_CARD_AFTER_CARD_GENERATED_FOR_COMBAT_HOOKS: dict[CardId, CardAfterCardGeneratedForCombatHook] = {}
_CARD_AFTER_CARD_ENTERED_COMBAT_HOOKS: dict[CardId, CardAfterCardEnteredCombatHook] = {}
_CARD_BEFORE_CARD_PLAYED_HOOKS: dict[CardId, CardCardPlayHook] = {}
_CARD_AFTER_CARD_PLAYED_HOOKS: dict[CardId, CardCardPlayHook] = {}
_CARD_AFTER_ATTACK_HOOKS: dict[CardId, CardAfterAttackHook] = {}
_CARD_AFTER_DEATH_HOOKS: dict[CardId, CardAfterDeathHook] = {}
_SELF_MUTATING_DAMAGE_CARD_IDS: set[CardId] = set()
_SELF_MUTATING_BLOCK_CARD_IDS: set[CardId] = set()


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


def register_chosen_hook(card_id: CardId):
    def decorator(func: CardChosenHook) -> CardChosenHook:
        _CARD_CHOSEN_HOOKS[card_id] = func
        return func
    return decorator


def register_playability_hook(card_id: CardId):
    def decorator(func: CardPlayabilityHook) -> CardPlayabilityHook:
        _CARD_PLAYABILITY_HOOKS[card_id] = func
        return func
    return decorator


def register_should_play_hook(card_id: CardId):
    def decorator(func: CardShouldPlayHook) -> CardShouldPlayHook:
        _CARD_SHOULD_PLAY_HOOKS[card_id] = func
        return func
    return decorator


def register_target_type_hook(card_id: CardId):
    def decorator(func: CardTargetTypeHook) -> CardTargetTypeHook:
        _CARD_TARGET_TYPE_HOOKS[card_id] = func
        return func
    return decorator


def register_gains_block_hook(card_id: CardId):
    def decorator(func: CardGainsBlockHook) -> CardGainsBlockHook:
        _CARD_GAINS_BLOCK_HOOKS[card_id] = func
        return func
    return decorator


def register_after_combat_end_hook(card_id: CardId):
    def decorator(func: CardAfterCombatEndHook) -> CardAfterCombatEndHook:
        _CARD_AFTER_COMBAT_END_HOOKS[card_id] = func
        return func
    return decorator


def register_next_event_hook(card_id: CardId):
    def decorator(func: CardNextEventHook) -> CardNextEventHook:
        _CARD_NEXT_EVENT_HOOKS[card_id] = func
        return func
    return decorator


def register_unknown_room_types_hook(card_id: CardId):
    def decorator(func: CardUnknownRoomTypesHook) -> CardUnknownRoomTypesHook:
        _CARD_UNKNOWN_ROOM_TYPES_HOOKS[card_id] = func
        return func
    return decorator


def register_generated_map_hook(card_id: CardId):
    def decorator(func: CardGeneratedMapHook) -> CardGeneratedMapHook:
        _CARD_GENERATED_MAP_HOOKS[card_id] = func
        return func
    return decorator


def register_generated_map_late_hook(card_id: CardId):
    def decorator(func: CardGeneratedMapHook) -> CardGeneratedMapHook:
        _CARD_GENERATED_MAP_LATE_HOOKS[card_id] = func
        return func
    return decorator


def register_after_map_generated_hook(card_id: CardId):
    def decorator(func: CardAfterMapGeneratedHook) -> CardAfterMapGeneratedHook:
        _CARD_AFTER_MAP_GENERATED_HOOKS[card_id] = func
        return func
    return decorator


def register_quest_complete_hook(card_id: CardId):
    def decorator(func: CardQuestCompleteHook) -> CardQuestCompleteHook:
        _CARD_QUEST_COMPLETE_HOOKS[card_id] = func
        return func
    return decorator


def register_rest_site_options_hook(card_id: CardId):
    def decorator(func: CardRestSiteOptionsHook) -> CardRestSiteOptionsHook:
        _CARD_REST_SITE_OPTIONS_HOOKS[card_id] = func
        return func
    return decorator


def register_before_hand_draw_hook(card_id: CardId):
    def decorator(func: CardBeforeHandDrawHook) -> CardBeforeHandDrawHook:
        _CARD_BEFORE_HAND_DRAW_HOOKS[card_id] = func
        return func
    return decorator


def register_after_turn_end_hook(card_id: CardId):
    def decorator(func: CardAfterTurnEndHook) -> CardAfterTurnEndHook:
        _CARD_AFTER_TURN_END_HOOKS[card_id] = func
        return func
    return decorator


def register_after_card_drawn_hook(card_id: CardId):
    def decorator(func: CardAfterCardDrawnHook) -> CardAfterCardDrawnHook:
        _CARD_AFTER_CARD_DRAWN_HOOKS[card_id] = func
        return func
    return decorator


def register_after_card_exhausted_hook(card_id: CardId):
    """Register a per-card AfterCardExhausted hook (C# CardModel.AfterCardExhausted).

    The hook receives (listener_card, exhausted_card, combat), mirroring the C#
    signature AfterCardExhausted(choiceContext, card, causedByEthereal) where
    `card` is the exhausted model; listeners compare identity against `this`.
    """
    def decorator(func: CardAfterCardExhaustedHook) -> CardAfterCardExhaustedHook:
        _CARD_AFTER_CARD_EXHAUSTED_HOOKS[card_id] = func
        return func
    return decorator


def register_after_auto_post_play_hook(card_id: CardId):
    """Register a per-card AfterAutoPostPlayPhaseEntered hook.

    C# CardModel.AfterAutoPostPlayPhaseEntered(choiceContext, player) fires at
    the start of the end-of-turn phase, before BeforeTurnEnd, for the player
    whose turn is ending.
    """
    def decorator(func: CardAfterAutoPostPlayHook) -> CardAfterAutoPostPlayHook:
        _CARD_AFTER_AUTO_POST_PLAY_HOOKS[card_id] = func
        return func
    return decorator


def register_turn_end_in_hand_hook(card_id: CardId):
    def decorator(func: CardTurnEndInHandHook) -> CardTurnEndInHandHook:
        _CARD_TURN_END_IN_HAND_HOOKS[card_id] = func
        return func
    return decorator


def register_after_card_generated_for_combat_hook(card_id: CardId):
    def decorator(
        func: CardAfterCardGeneratedForCombatHook,
    ) -> CardAfterCardGeneratedForCombatHook:
        _CARD_AFTER_CARD_GENERATED_FOR_COMBAT_HOOKS[card_id] = func
        return func
    return decorator


def register_after_card_entered_combat_hook(card_id: CardId):
    def decorator(func: CardAfterCardEnteredCombatHook) -> CardAfterCardEnteredCombatHook:
        _CARD_AFTER_CARD_ENTERED_COMBAT_HOOKS[card_id] = func
        return func
    return decorator


def register_before_card_played_hook(card_id: CardId):
    def decorator(func: CardCardPlayHook) -> CardCardPlayHook:
        _CARD_BEFORE_CARD_PLAYED_HOOKS[card_id] = func
        return func
    return decorator


def register_after_card_played_hook(card_id: CardId):
    def decorator(func: CardCardPlayHook) -> CardCardPlayHook:
        _CARD_AFTER_CARD_PLAYED_HOOKS[card_id] = func
        return func
    return decorator


def register_after_attack_hook(card_id: CardId):
    def decorator(func: CardAfterAttackHook) -> CardAfterAttackHook:
        _CARD_AFTER_ATTACK_HOOKS[card_id] = func
        return func
    return decorator


def register_after_death_hook(card_id: CardId):
    def decorator(func: CardAfterDeathHook) -> CardAfterDeathHook:
        _CARD_AFTER_DEATH_HOOKS[card_id] = func
        return func
    return decorator


def register_self_mutating_damage(card_id: CardId) -> None:
    _SELF_MUTATING_DAMAGE_CARD_IDS.add(card_id)


def register_self_mutating_block(card_id: CardId) -> None:
    _SELF_MUTATING_BLOCK_CARD_IDS.add(card_id)


def card_preserves_self_mutating_damage(card_id: CardId) -> bool:
    return card_id in _SELF_MUTATING_DAMAGE_CARD_IDS


def card_preserves_self_mutating_block(card_id: CardId) -> bool:
    return card_id in _SELF_MUTATING_BLOCK_CARD_IDS


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


def fire_card_chosen(
    card: "CardInstance",
    combat: "CombatState",
) -> None:
    hook = _CARD_CHOSEN_HOOKS.get(card.card_id)
    if hook is not None:
        hook(card, combat)


def is_card_playable(
    card: "CardInstance",
    owner_state: object,
    combat: "CombatState",
    owner: "Creature",
) -> bool:
    hook = _CARD_PLAYABILITY_HOOKS.get(card.card_id)
    if hook is None:
        return True
    return hook(card, owner_state, combat, owner)


def should_play_card_from_hand(
    listener_card: "CardInstance",
    played_card: "CardInstance",
    owner_state: object,
    combat: "CombatState",
    owner: "Creature",
    is_auto_play: bool,
) -> bool:
    hook = _CARD_SHOULD_PLAY_HOOKS.get(listener_card.card_id)
    if hook is None:
        return True
    return hook(listener_card, played_card, owner_state, combat, owner, is_auto_play)


def card_target_type_for(
    card: "CardInstance",
    owner: "Creature",
    target_type: "TargetType",
) -> "TargetType":
    hook = _CARD_TARGET_TYPE_HOOKS.get(card.card_id)
    if hook is None:
        return target_type
    return hook(card, owner, target_type)


def card_gains_block(card: "CardInstance", default: bool) -> bool:
    hook = _CARD_GAINS_BLOCK_HOOKS.get(card.card_id)
    if hook is None:
        return default
    return hook(card, default)


def apply_card_after_combat_end(card: "CardInstance", owner: "PlayerState") -> bool:
    hook = _CARD_AFTER_COMBAT_END_HOOKS.get(card.card_id)
    if hook is None:
        return True
    return hook(card, owner)


def modify_card_next_event(
    card: "CardInstance",
    run_state: "RunState",
    event: "EventModel | None",
) -> "EventModel | None":
    hook = _CARD_NEXT_EVENT_HOOKS.get(card.card_id)
    if hook is None:
        return event
    return hook(card, run_state, event)


def modify_card_unknown_room_types(
    card: "CardInstance",
    run_state: "RunState",
    room_types: set["RoomType"],
) -> set["RoomType"]:
    hook = _CARD_UNKNOWN_ROOM_TYPES_HOOKS.get(card.card_id)
    if hook is None:
        return room_types
    return hook(card, run_state, room_types)


def modify_card_generated_map(
    card: "CardInstance",
    run_state: "RunState",
    act_map: "ActMap",
    act_index: int,
) -> "ActMap":
    hook = _CARD_GENERATED_MAP_HOOKS.get(card.card_id)
    if hook is None:
        return act_map
    return hook(card, run_state, act_map, act_index)


def modify_card_generated_map_late(
    card: "CardInstance",
    run_state: "RunState",
    act_map: "ActMap",
    act_index: int,
) -> "ActMap":
    hook = _CARD_GENERATED_MAP_LATE_HOOKS.get(card.card_id)
    if hook is None:
        return act_map
    return hook(card, run_state, act_map, act_index)


def fire_card_after_map_generated(
    card: "CardInstance",
    run_state: "RunState",
    act_map: "ActMap",
    act_index: int,
) -> None:
    hook = _CARD_AFTER_MAP_GENERATED_HOOKS.get(card.card_id)
    if hook is not None:
        hook(card, run_state, act_map, act_index)


def complete_card_quest(card: "CardInstance", run_state: "RunState") -> int:
    hook = _CARD_QUEST_COMPLETE_HOOKS.get(card.card_id)
    if hook is None:
        return 0
    return hook(card, run_state)


def modify_card_rest_site_options(
    card: "CardInstance",
    owner: "PlayerState",
    options: list["RestSiteOption"],
    run_state: "RunState | None",
) -> list["RestSiteOption"]:
    hook = _CARD_REST_SITE_OPTIONS_HOOKS.get(card.card_id)
    if hook is None:
        return options
    return hook(card, owner, options, run_state)


def fire_card_before_hand_draw(
    card: "CardInstance",
    drawing_owner: "Creature",
    combat: "CombatState",
) -> None:
    hook = _CARD_BEFORE_HAND_DRAW_HOOKS.get(card.card_id)
    if hook is not None:
        hook(card, drawing_owner, combat)


def fire_card_after_turn_end(
    card: "CardInstance",
    side: "CombatSide",
    combat: "CombatState",
) -> None:
    hook = _CARD_AFTER_TURN_END_HOOKS.get(card.card_id)
    if hook is not None:
        hook(card, side, combat)


def fire_card_after_card_drawn(
    listener_card: "CardInstance",
    drawn_card: "CardInstance",
    from_hand_draw: bool,
    combat: "CombatState",
) -> None:
    hook = _CARD_AFTER_CARD_DRAWN_HOOKS.get(listener_card.card_id)
    if hook is not None:
        hook(listener_card, drawn_card, from_hand_draw, combat)


def fire_card_after_card_exhausted(
    listener_card: "CardInstance",
    exhausted_card: "CardInstance",
    combat: "CombatState",
) -> None:
    hook = _CARD_AFTER_CARD_EXHAUSTED_HOOKS.get(listener_card.card_id)
    if hook is not None:
        hook(listener_card, exhausted_card, combat)


def fire_card_after_auto_post_play(
    card: "CardInstance",
    player: "Creature",
    combat: "CombatState",
) -> None:
    hook = _CARD_AFTER_AUTO_POST_PLAY_HOOKS.get(card.card_id)
    if hook is not None:
        hook(card, player, combat)


def fire_card_turn_end_in_hand(
    card: "CardInstance",
    combat: "CombatState",
    cards_in_hand_at_turn_end: int,
) -> None:
    hook = _CARD_TURN_END_IN_HAND_HOOKS.get(card.card_id)
    if hook is not None:
        hook(card, combat, cards_in_hand_at_turn_end)


def fire_card_after_card_generated_for_combat(
    listener_card: "CardInstance",
    generated_card: "CardInstance",
    added_by_player: bool,
    combat: "CombatState",
) -> None:
    hook = _CARD_AFTER_CARD_GENERATED_FOR_COMBAT_HOOKS.get(listener_card.card_id)
    if hook is not None:
        hook(listener_card, generated_card, added_by_player, combat)


def fire_card_after_card_entered_combat(
    listener_card: "CardInstance",
    entered_card: "CardInstance",
    owner: "Creature",
    combat: "CombatState",
) -> None:
    hook = _CARD_AFTER_CARD_ENTERED_COMBAT_HOOKS.get(listener_card.card_id)
    if hook is not None:
        hook(listener_card, entered_card, owner, combat)


def fire_card_before_card_played(
    listener_card: "CardInstance",
    played_card: "CardInstance",
    owner: "Creature",
    combat: "CombatState",
) -> None:
    hook = _CARD_BEFORE_CARD_PLAYED_HOOKS.get(listener_card.card_id)
    if hook is not None:
        hook(listener_card, played_card, owner, combat)


def fire_card_after_card_played(
    listener_card: "CardInstance",
    played_card: "CardInstance",
    owner: "Creature",
    combat: "CombatState",
) -> None:
    hook = _CARD_AFTER_CARD_PLAYED_HOOKS.get(listener_card.card_id)
    if hook is not None:
        hook(listener_card, played_card, owner, combat)


def fire_card_after_attack(
    listener_card: "CardInstance",
    attack: object,
    combat: "CombatState",
) -> None:
    hook = _CARD_AFTER_ATTACK_HOOKS.get(listener_card.card_id)
    if hook is not None:
        hook(listener_card, attack, combat)


def fire_card_after_death(
    listener_card: "CardInstance",
    creature: "Creature",
    was_removal_prevented: bool,
    combat: "CombatState",
) -> None:
    hook = _CARD_AFTER_DEATH_HOOKS.get(listener_card.card_id)
    if hook is not None:
        hook(listener_card, creature, was_removal_prevented, combat)
