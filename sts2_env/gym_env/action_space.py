"""Action space and masking logic."""

from __future__ import annotations

import numpy as np

from sts2_env.core.combat import CombatState
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


def get_action_mask(combat: CombatState) -> np.ndarray:
    """Return boolean mask of valid actions."""
    mask = np.zeros(ACTION_SPACE_SIZE, dtype=np.int8)

    if combat.is_over:
        mask[ACTION_END_TURN] = 1
        return mask

    # Can always end turn
    mask[ACTION_END_TURN] = 1

    for i in range(min(len(combat.hand), MAX_HAND_SIZE)):
        card = combat.hand[i]
        if not combat.can_play_card(card):
            continue

        if card.target_type in (TargetType.SELF, TargetType.NONE, TargetType.ALL_ENEMIES):
            mask[1 + i] = 1
        elif card.target_type in (TargetType.ANY_ENEMY, TargetType.RANDOM_ENEMY):
            for j in range(min(len(combat.enemies), MAX_ENEMIES)):
                if combat.enemies[j].is_alive:
                    mask[1 + MAX_HAND_SIZE + i * MAX_ENEMIES + j] = 1

    return mask
