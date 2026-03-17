"""Action space and masking logic."""

from __future__ import annotations

import numpy as np

from sts2_env.core.combat import CombatState
from sts2_env.core.creature import Creature
from sts2_env.core.enums import TargetType
from sts2_env.core.constants import ACTION_END_TURN, ACTION_SPACE_SIZE, MAX_HAND_SIZE, MAX_ENEMIES


def action_to_card_and_target(action: int) -> tuple[int | None, int | None]:
    """Convert action index to (hand_index, target_index).

    Returns (None, None) for end turn.
    Returns (hand_idx, None) for self/all-target cards.
    Returns (hand_idx, enemy_idx) for targeted cards.
    """
    if action == ACTION_END_TURN:
        return None, None
    if action <= MAX_HAND_SIZE:
        return action - 1, None
    adjusted = action - 1 - MAX_HAND_SIZE
    hand_idx = adjusted // MAX_ENEMIES
    enemy_idx = adjusted % MAX_ENEMIES
    return hand_idx, enemy_idx


def get_action_mask(combat: CombatState, owner: Creature | None = None) -> np.ndarray:
    """Return boolean mask of valid actions."""
    mask = np.zeros(ACTION_SPACE_SIZE, dtype=np.int8)
    acting_owner = owner or combat.primary_player
    owner_state = combat.combat_player_state_for(acting_owner)

    if combat.is_over:
        mask[ACTION_END_TURN] = 1
        return mask

    if combat.pending_choice is not None:
        if combat.pending_choice.can_confirm():
            mask[ACTION_END_TURN] = 1
        for i in range(min(combat.pending_choice.num_options, ACTION_SPACE_SIZE - 1)):
            if combat.pending_choice.is_multi:
                if combat.pending_choice.can_toggle(i):
                    mask[1 + i] = 1
            else:
                mask[1 + i] = 1
        return mask

    # Can always end turn
    mask[ACTION_END_TURN] = 1

    hand = owner_state.hand if owner_state is not None else combat.hand
    for i in range(min(len(hand), MAX_HAND_SIZE)):
        card = hand[i]
        if not combat.can_play_card(card):
            continue

        if card.target_type in (TargetType.SELF, TargetType.NONE, TargetType.ALL_ENEMIES, TargetType.ALL_ALLIES):
            mask[1 + i] = 1
        elif card.target_type in (TargetType.ANY_ENEMY, TargetType.RANDOM_ENEMY):
            for j in range(min(len(combat.enemies), MAX_ENEMIES)):
                if combat.enemies[j].is_alive:
                    mask[1 + MAX_HAND_SIZE + i * MAX_ENEMIES + j] = 1
        elif card.target_type == TargetType.ANY_ALLY:
            allies = combat.get_player_allies_of(acting_owner)
            for j in range(min(len(allies), MAX_ENEMIES)):
                mask[1 + MAX_HAND_SIZE + i * MAX_ENEMIES + j] = 1

    return mask
