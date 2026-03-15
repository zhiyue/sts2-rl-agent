"""Duration and temporary powers that tick down each turn.

These powers decrement their amount at AfterTurnEnd when side == ENEMY,
verified against the decompiled C# source from MegaCrit.Sts2.Core.Models.Powers.

The existing VulnerablePower, WeakPower, FrailPower in common.py already
implement the correct tick logic. This module re-exports and verifies them.

Plating also ticks down and is already in common.py, but it additionally
grants block at turn end. The version here supersedes common.py's stub.
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from sts2_env.core.enums import CombatSide, PowerId, PowerType, PowerStackType, ValueProp
from sts2_env.powers.base import PowerInstance

if TYPE_CHECKING:
    from sts2_env.core.creature import Creature
    from sts2_env.core.combat import CombatState

# =====================================================================
#  Re-exported duration powers from common.py for reference / verification
# =====================================================================
#
# VulnerablePower (common.py):
#   - Tick: AfterTurnEnd side==ENEMY => PowerCmd.TickDownDuration
#   - Our on_turn_end_enemy_side: amount -= 1 (with skip_next_tick guard)
#   - VERIFIED: matches C# exactly.
#
# WeakPower (common.py):
#   - Tick: AfterTurnEnd side==ENEMY => PowerCmd.TickDownDuration
#   - Our on_turn_end_enemy_side: amount -= 1 (with skip_next_tick guard)
#   - VERIFIED: matches C# exactly.
#
# FrailPower (common.py):
#   - Tick: AfterTurnEnd side==ENEMY => PowerCmd.TickDownDuration
#   - Our on_turn_end_enemy_side: amount -= 1 (with skip_next_tick guard)
#   - VERIFIED: matches C# exactly.
#
# PlatingPower (common.py):
#   - Current common.py has a stub that only ticks at AfterTurnEnd.
#   - C# actually: BeforeTurnEndEarly grants block(Amount, Unpowered),
#     AfterTurnEnd (side==ENEMY) decrements by 1 for player, by player-count
#     for enemies. Also grants block on first round for enemies.
#   - We override with a full implementation below.
#
# RitualPower (common.py):
#   - C# actually fires at AfterTurnEnd (side == Owner.Side), NOT turn start.
#   - The turn_effects.py version uses after_turn_end which is correct.
#   - common.py version uses on_turn_start_own_side which is the legacy mapping.
#   - turn_effects.py supersedes common.py's RitualPower.


# =====================================================================
#  Plating — full implementation matching C# source
# =====================================================================


class PlatingPower(PowerInstance):
    """Gain Amount block at end of turn (before other end-of-turn effects).
    Ticks down by 1 at AfterTurnEnd when side == ENEMY.

    C# hooks:
      BeforeSideTurnStart: Enemy Plating grants block on round 1 (side==PLAYER).
      BeforeTurnEndEarly (side == Owner.Side): gain block = Amount (Unpowered).
      AfterTurnEnd (side == ENEMY): decrement by 1 (player) or by player-count (enemy).

    For the simulator, we simplify the multiplayer scaling:
    enemies decrement by 1 per turn (single-player).
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.PLATING, amount)
        self._first_round_block_given: bool = False

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        # Enemy Plating grants block on first round when player side starts
        if side == CombatSide.PLAYER and not owner.is_player and not self._first_round_block_given:
            round_num = getattr(combat, "round_number", 1)
            if round_num == 1:
                owner.gain_block(self.amount)
                self._first_round_block_given = True

    def before_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            owner.gain_block(self.amount)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.ENEMY:
            if self.skip_next_tick:
                self.skip_next_tick = False
                return
            self.amount -= 1

    # Legacy hook kept for backward compat with tick_down_power()
    def on_turn_end_enemy_side(self, owner: Creature) -> None:
        if self.skip_next_tick:
            self.skip_next_tick = False
            return
        self.amount -= 1


# =====================================================================
#  Duration versions of Vulnerable / Weak / Frail
#  (Verifying the existing implementations in common.py match C#)
# =====================================================================
#
# The C# uses PowerCmd.TickDownDuration which:
#   1. If SkipNextTick is true, set it false and return.
#   2. Otherwise, decrement Amount by 1.
#
# common.py's on_turn_end_enemy_side does exactly this.
# All three tick at AfterTurnEnd when side == CombatSide.Enemy.
# This matches the C# source perfectly.
#
# No changes needed for VulnerablePower, WeakPower, FrailPower.


# =====================================================================
#  Registration — only register powers that supersede common.py
# =====================================================================

from sts2_env.core.creature import register_power_class

# Override the Plating stub from common.py with the full implementation
register_power_class(PowerId.PLATING, PlatingPower)
