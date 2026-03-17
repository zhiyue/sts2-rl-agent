"""Monster AI state machine.

Direct port of the C# MonsterMoveStateMachine, MoveState,
RandomBranchState, and ConditionalBranchState.
"""

from __future__ import annotations

from itertools import takewhile
from typing import Callable, TYPE_CHECKING

from sts2_env.core.enums import MoveRepeatType
from sts2_env.monsters.intents import Intent

if TYPE_CHECKING:
    from sts2_env.core.combat import CombatState
    from sts2_env.core.rng import Rng


class MonsterState:
    """Base class for all state machine states."""

    def __init__(self, state_id: str):
        self.state_id = state_id
        self.should_appear_in_logs: bool = True

    @property
    def is_move(self) -> bool:
        return False

    @property
    def can_transition_away(self) -> bool:
        return True

    def get_next_state(self, state_log: list[str], rng: Rng) -> str:
        raise NotImplementedError


class MoveState(MonsterState):
    """A concrete move that a monster performs."""

    def __init__(
        self,
        state_id: str,
        effect_fn: Callable[[CombatState], None],
        intents: list[Intent],
        follow_up_id: str | None = None,
        must_perform_once: bool = False,
    ):
        super().__init__(state_id)
        self.effect_fn = effect_fn
        self.intents = intents
        self.follow_up_id = follow_up_id
        self.must_perform_once = must_perform_once
        self._performed_at_least_once: bool = False

    @property
    def is_move(self) -> bool:
        return True

    @property
    def can_transition_away(self) -> bool:
        if self.must_perform_once:
            return self._performed_at_least_once
        return True

    def perform(self, combat: CombatState) -> None:
        self._performed_at_least_once = True
        flush_pending = getattr(combat, "flush_pending_attack_context", None)
        if callable(flush_pending):
            flush_pending()
        try:
            self.effect_fn(combat)
        finally:
            if callable(flush_pending):
                flush_pending()

    def on_exit_state(self) -> None:
        self._performed_at_least_once = False

    def get_next_state(self, state_log: list[str], rng: Rng) -> str:
        if self.follow_up_id is None:
            raise ValueError(f"MoveState '{self.state_id}' has no follow_up_id")
        return self.follow_up_id


class WeightedBranch:
    """A single branch option in a RandomBranchState."""

    __slots__ = ("state_id", "repeat_type", "max_times", "base_weight", "cooldown")

    def __init__(
        self,
        state_id: str,
        repeat_type: MoveRepeatType = MoveRepeatType.CAN_REPEAT_FOREVER,
        max_times: int = 1,
        weight: float = 1.0,
        cooldown: int = 0,
    ):
        self.state_id = state_id
        self.repeat_type = repeat_type
        self.max_times = max_times
        self.base_weight = weight
        self.cooldown = cooldown

    def get_weight(self, state_log: list[str]) -> float:
        """Calculate effective weight given the state history."""
        if self.repeat_type == MoveRepeatType.USE_ONLY_ONCE:
            if self.state_id in state_log:
                return 0.0

        if self.repeat_type == MoveRepeatType.CANNOT_REPEAT:
            if state_log and state_log[-1] == self.state_id:
                return 0.0

        if self.repeat_type == MoveRepeatType.CAN_REPEAT_X_TIMES:
            # Count consecutive occurrences at end of log
            consecutive = sum(
                1 for _ in takewhile(lambda x: x == self.state_id, reversed(state_log))
            )
            if consecutive >= self.max_times:
                return 0.0

        if self.cooldown > 0:
            # Check last N move entries
            move_entries = state_log[-self.cooldown:]
            if self.state_id in move_entries:
                return 0.0

        return self.base_weight


class RandomBranchState(MonsterState):
    """Randomly selects among branches with weights and repeat constraints."""

    def __init__(self, state_id: str):
        super().__init__(state_id)
        self.branches: list[WeightedBranch] = []
        self.should_appear_in_logs = False

    def add_branch(
        self,
        state_id: str,
        repeat_type: MoveRepeatType = MoveRepeatType.CAN_REPEAT_FOREVER,
        max_times: int = 1,
        weight: float = 1.0,
        cooldown: int = 0,
    ) -> RandomBranchState:
        self.branches.append(WeightedBranch(state_id, repeat_type, max_times, weight, cooldown))
        return self

    def get_next_state(self, state_log: list[str], rng: Rng) -> str:
        weights = [b.get_weight(state_log) for b in self.branches]
        total_weight = sum(weights)
        if total_weight <= 0:
            # Fallback: all branches exhausted, pick first available
            for b in self.branches:
                return b.state_id
            raise ValueError(f"RandomBranchState '{self.state_id}' has no branches")

        roll = rng.next_float(total_weight)
        cumulative = 0.0
        for branch, w in zip(self.branches, weights):
            if w <= 0:
                continue
            cumulative += w
            if roll < cumulative:
                return branch.state_id

        # Floating point edge case: return last valid branch
        for branch, w in zip(reversed(self.branches), reversed(weights)):
            if w > 0:
                return branch.state_id
        raise ValueError("unreachable")


class ConditionalBranchState(MonsterState):
    """Selects the first branch whose condition is true."""

    def __init__(self, state_id: str):
        super().__init__(state_id)
        self.branches: list[tuple[Callable[[], bool], str]] = []
        self.should_appear_in_logs = False

    def add_branch(self, condition: Callable[[], bool], state_id: str) -> ConditionalBranchState:
        self.branches.append((condition, state_id))
        return self

    def get_next_state(self, state_log: list[str], rng: Rng) -> str:
        for condition, state_id in self.branches:
            if condition():
                return state_id
        raise ValueError(f"ConditionalBranchState '{self.state_id}': no condition matched")


class MonsterAI:
    """Container for a monster's state machine."""

    def __init__(self, states: dict[str, MonsterState], initial_state_id: str):
        self.states = states
        self.state_log: list[str] = []
        self._current_state_id = initial_state_id
        self._performed_first_move: bool = False

        # Resolve initial state to a MoveState (walk through branches)
        self._resolve_to_move(None)

    @property
    def current_move(self) -> MoveState:
        state = self.states[self._current_state_id]
        assert state.is_move, f"Current state {self._current_state_id} is not a MoveState"
        return state

    def _resolve_to_move(self, rng: Rng | None) -> None:
        """Walk through branch states until we reach a MoveState."""
        safety = 100
        while not self.states[self._current_state_id].is_move:
            if safety <= 0:
                raise RuntimeError("Infinite loop in state machine resolution")
            safety -= 1
            state = self.states[self._current_state_id]
            # Use a temporary rng for initial resolution if none provided
            if rng is None:
                from sts2_env.core.rng import Rng as RngClass
                rng = RngClass(0)
            self._current_state_id = state.get_next_state(self.state_log, rng)

    def roll_move(self, rng: Rng) -> MoveState:
        """Advance the state machine to the next move.

        Per C#: first move is held until performed. After that,
        each call advances to the next MoveState.
        """
        current = self.states[self._current_state_id]

        # Don't advance if first move hasn't been performed yet
        if not self._performed_first_move and current.is_move:
            return current

        # Don't advance if current state must be performed first
        if current.is_move and not current.can_transition_away:
            return current

        # Get next state ID from current state
        next_id = current.get_next_state(self.state_log, rng)
        if current.is_move:
            current.on_exit_state()
        self._current_state_id = next_id

        # Resolve through branch states until we reach a MoveState
        self._resolve_to_move(rng)

        return self.current_move

    def on_move_performed(self) -> None:
        """Called after the current move has been executed."""
        self._performed_first_move = True
        state = self.states[self._current_state_id]
        if isinstance(state, MoveState):
            state._performed_at_least_once = True
        if state.should_appear_in_logs:
            self.state_log.append(self._current_state_id)
