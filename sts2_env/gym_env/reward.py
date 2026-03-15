"""Reward calculation."""

from __future__ import annotations

from sts2_env.core.combat import CombatState


def compute_reward(combat: CombatState, prev_hp: int) -> float:
    """Compute step reward.

    Sparse reward: +1 for win, -1 for loss, 0 otherwise.
    """
    if combat.is_over:
        return 1.0 if combat.player_won else -1.0
    return 0.0
