"""Tests for monster AI state machine."""

import pytest

from sts2_env.core.enums import MoveRepeatType
from sts2_env.core.rng import Rng
from sts2_env.monsters.intents import attack_intent, buff_intent, debuff_intent
from sts2_env.monsters.state_machine import (
    MonsterAI, MoveState, RandomBranchState, ConditionalBranchState,
)


# ---- Helpers ----

def _noop(combat):
    """Dummy effect for test moves."""
    pass


def _make_move(state_id: str, follow_up_id: str, must_perform_once: bool = False) -> MoveState:
    return MoveState(state_id, _noop, [attack_intent(1)], follow_up_id=follow_up_id,
                     must_perform_once=must_perform_once)


def _run_ai(ai: MonsterAI, rng: Rng, n: int) -> list[str]:
    """Perform n moves and return list of state_ids."""
    moves = [ai.current_move.state_id]
    ai.on_move_performed()
    for _ in range(n - 1):
        ai.roll_move(rng)
        moves.append(ai.current_move.state_id)
        ai.on_move_performed()
    return moves


# ========================================================================
# 1. Fixed rotation (MoveState follow-up chains)
# ========================================================================

class TestFixedRotation:
    def test_three_state_cycle(self):
        """A->B->C->A produces A,B,C,A,B,C."""
        rng = Rng(0)
        states = {
            "A": _make_move("A", "B"),
            "B": _make_move("B", "C"),
            "C": _make_move("C", "A"),
        }
        ai = MonsterAI(states, "A")
        moves = _run_ai(ai, rng, 6)
        assert moves == ["A", "B", "C", "A", "B", "C"]

    def test_two_state_cycle(self):
        """A->B->A produces A,B,A,B."""
        rng = Rng(0)
        states = {
            "A": _make_move("A", "B"),
            "B": _make_move("B", "A"),
        }
        ai = MonsterAI(states, "A")
        moves = _run_ai(ai, rng, 4)
        assert moves == ["A", "B", "A", "B"]

    def test_shrinker_beetle_rotation(self, rng):
        """ShrinkerBeetle: SHRINK -> CHOMP -> STOMP -> CHOMP -> STOMP..."""
        from sts2_env.monsters.act1_weak import create_shrinker_beetle
        _, ai = create_shrinker_beetle(rng)

        moves = _run_ai(ai, rng, 5)
        assert moves[0] == "SHRINK"
        assert moves[1] == "CHOMP"
        assert moves[2] == "STOMP"
        assert moves[3] == "CHOMP"
        assert moves[4] == "STOMP"

    def test_single_state_self_loop(self):
        """A->A stays on A forever."""
        rng = Rng(0)
        states = {
            "A": _make_move("A", "A"),
        }
        ai = MonsterAI(states, "A")
        moves = _run_ai(ai, rng, 5)
        assert moves == ["A", "A", "A", "A", "A"]


# ========================================================================
# 2. RandomBranchState with CannotRepeat
# ========================================================================

class TestRandomBranchCannotRepeat:
    def test_two_moves_must_alternate(self):
        """With 2 moves both CANNOT_REPEAT, verify no consecutive repeats."""
        rng = Rng(0)
        rand = RandomBranchState("RAND")
        rand.add_branch("A", MoveRepeatType.CANNOT_REPEAT)
        rand.add_branch("B", MoveRepeatType.CANNOT_REPEAT)

        states = {
            "RAND": rand,
            "A": _make_move("A", "RAND"),
            "B": _make_move("B", "RAND"),
        }
        ai = MonsterAI(states, "RAND")
        moves = _run_ai(ai, rng, 20)

        for i in range(1, len(moves)):
            assert moves[i] != moves[i - 1], (
                f"Consecutive repeat at index {i}: {moves[i-1:i+1]}"
            )

    def test_three_moves_cannot_repeat_no_consecutive(self):
        """With 3 moves all CANNOT_REPEAT, none should repeat consecutively."""
        rng = Rng(99)
        rand = RandomBranchState("RAND")
        rand.add_branch("A", MoveRepeatType.CANNOT_REPEAT)
        rand.add_branch("B", MoveRepeatType.CANNOT_REPEAT)
        rand.add_branch("C", MoveRepeatType.CANNOT_REPEAT)

        states = {
            "RAND": rand,
            "A": _make_move("A", "RAND"),
            "B": _make_move("B", "RAND"),
            "C": _make_move("C", "RAND"),
        }
        ai = MonsterAI(states, "RAND")
        moves = _run_ai(ai, rng, 30)

        for i in range(1, len(moves)):
            assert moves[i] != moves[i - 1]

    def test_uses_multiple_seeds(self):
        """CANNOT_REPEAT should work across different RNG seeds."""
        rand = RandomBranchState("RAND")
        rand.add_branch("X", MoveRepeatType.CANNOT_REPEAT)
        rand.add_branch("Y", MoveRepeatType.CANNOT_REPEAT)

        for seed in range(10):
            rng = Rng(seed)
            states = {
                "RAND": RandomBranchState("RAND"),
                "X": _make_move("X", "RAND"),
                "Y": _make_move("Y", "RAND"),
            }
            # Re-add branches since we cloned the state
            states["RAND"] = RandomBranchState("RAND")
            states["RAND"].add_branch("X", MoveRepeatType.CANNOT_REPEAT)
            states["RAND"].add_branch("Y", MoveRepeatType.CANNOT_REPEAT)

            ai = MonsterAI(states, "RAND")
            moves = _run_ai(ai, rng, 10)
            for i in range(1, len(moves)):
                assert moves[i] != moves[i - 1], f"seed={seed}"


# ========================================================================
# 3. UseOnlyOnce
# ========================================================================

class TestUseOnlyOnce:
    def test_appears_at_most_once(self):
        """USE_ONLY_ONCE move should appear at most 1 time."""
        rng = Rng(12345)
        rand = RandomBranchState("RAND")
        rand.add_branch("ONCE", MoveRepeatType.USE_ONLY_ONCE)
        rand.add_branch("ALWAYS", MoveRepeatType.CAN_REPEAT_FOREVER)

        states = {
            "RAND": rand,
            "ONCE": _make_move("ONCE", "RAND"),
            "ALWAYS": _make_move("ALWAYS", "RAND"),
        }
        ai = MonsterAI(states, "RAND")
        moves = _run_ai(ai, rng, 30)

        assert moves.count("ONCE") <= 1

    def test_use_only_once_across_seeds(self):
        """Verify USE_ONLY_ONCE across multiple seeds."""
        for seed in range(20):
            rng = Rng(seed)
            rand = RandomBranchState("RAND")
            rand.add_branch("SPECIAL", MoveRepeatType.USE_ONLY_ONCE)
            rand.add_branch("NORMAL", MoveRepeatType.CAN_REPEAT_FOREVER)

            states = {
                "RAND": rand,
                "SPECIAL": _make_move("SPECIAL", "RAND"),
                "NORMAL": _make_move("NORMAL", "RAND"),
            }
            ai = MonsterAI(states, "RAND")
            moves = _run_ai(ai, rng, 20)
            assert moves.count("SPECIAL") <= 1, f"seed={seed}, moves={moves}"


# ========================================================================
# 4. CAN_REPEAT_X_TIMES
# ========================================================================

class TestCanRepeatXTimes:
    def test_max_consecutive(self):
        """CAN_REPEAT_X_TIMES with max_times=2 allows at most 2 consecutive."""
        rng = Rng(0)
        rand = RandomBranchState("RAND")
        rand.add_branch("A", MoveRepeatType.CAN_REPEAT_X_TIMES, max_times=2)
        rand.add_branch("B", MoveRepeatType.CAN_REPEAT_FOREVER)

        states = {
            "RAND": rand,
            "A": _make_move("A", "RAND"),
            "B": _make_move("B", "RAND"),
        }
        ai = MonsterAI(states, "RAND")
        moves = _run_ai(ai, rng, 40)

        # Check: never more than 2 consecutive A's
        consecutive_a = 0
        max_consecutive_a = 0
        for m in moves:
            if m == "A":
                consecutive_a += 1
                max_consecutive_a = max(max_consecutive_a, consecutive_a)
            else:
                consecutive_a = 0
        assert max_consecutive_a <= 2, f"Got {max_consecutive_a} consecutive A's"


# ========================================================================
# 5. ConditionalBranchState
# ========================================================================

class TestConditionalBranch:
    def test_first_matching_condition_wins(self):
        """ConditionalBranch picks the first true condition."""
        rng = Rng(0)
        cond = ConditionalBranchState("COND")
        cond.add_branch(lambda: False, "A")
        cond.add_branch(lambda: True, "B")
        cond.add_branch(lambda: True, "C")  # Also true but should not be picked

        states = {
            "COND": cond,
            "A": _make_move("A", "A"),
            "B": _make_move("B", "B"),
            "C": _make_move("C", "C"),
        }
        ai = MonsterAI(states, "COND")
        assert ai.current_move.state_id == "B"

    def test_condition_with_mutable_state(self):
        """ConditionalBranch can use mutable external state."""
        rng = Rng(0)
        flag = [False]

        cond = ConditionalBranchState("COND")
        cond.add_branch(lambda: flag[0], "WHEN_TRUE")
        cond.add_branch(lambda: True, "FALLBACK")

        states = {
            "COND": cond,
            "WHEN_TRUE": _make_move("WHEN_TRUE", "WHEN_TRUE"),
            "FALLBACK": _make_move("FALLBACK", "FALLBACK"),
        }

        ai = MonsterAI(states, "COND")
        assert ai.current_move.state_id == "FALLBACK"

    def test_no_condition_matches_raises(self):
        """ConditionalBranch with no matching condition raises ValueError."""
        rng = Rng(0)
        cond = ConditionalBranchState("COND")
        cond.add_branch(lambda: False, "A")
        cond.add_branch(lambda: False, "B")

        states = {
            "COND": cond,
            "A": _make_move("A", "A"),
            "B": _make_move("B", "B"),
        }

        with pytest.raises(ValueError, match="no condition matched"):
            MonsterAI(states, "COND")

    def test_nibbit_conditional_start(self, rng):
        """Nibbit uses ConditionalBranch for start state."""
        from sts2_env.monsters.act1_weak import create_nibbit

        _, ai_alone = create_nibbit(rng, is_alone=True)
        assert ai_alone.current_move.state_id == "BUTT"

        _, ai_front = create_nibbit(rng, is_alone=False, is_front=True)
        assert ai_front.current_move.state_id == "SLICE"

        _, ai_back = create_nibbit(rng, is_alone=False, is_front=False)
        assert ai_back.current_move.state_id == "HISS"


# ========================================================================
# 6. state_log only tracks MoveStates (not branch states)
# ========================================================================

class TestStateLog:
    def test_log_only_move_states(self):
        """state_log should only contain MoveState entries, not branch states."""
        rng = Rng(0)
        rand = RandomBranchState("RAND")
        rand.add_branch("A", MoveRepeatType.CANNOT_REPEAT)
        rand.add_branch("B", MoveRepeatType.CANNOT_REPEAT)

        states = {
            "RAND": rand,
            "A": _make_move("A", "RAND"),
            "B": _make_move("B", "RAND"),
        }
        ai = MonsterAI(states, "RAND")
        _run_ai(ai, rng, 6)

        assert "RAND" not in ai.state_log
        for entry in ai.state_log:
            assert entry in ("A", "B"), f"Unexpected log entry: {entry}"
        assert len(ai.state_log) == 6

    def test_fixed_rotation_log(self):
        """Fixed rotation should log each performed move."""
        rng = Rng(0)
        states = {
            "A": _make_move("A", "B"),
            "B": _make_move("B", "C"),
            "C": _make_move("C", "A"),
        }
        ai = MonsterAI(states, "A")
        _run_ai(ai, rng, 6)

        assert ai.state_log == ["A", "B", "C", "A", "B", "C"]

    def test_log_length_matches_moves(self):
        """Log length should match number of performed moves."""
        rng = Rng(42)
        rand = RandomBranchState("RAND")
        rand.add_branch("X", MoveRepeatType.CAN_REPEAT_FOREVER)
        rand.add_branch("Y", MoveRepeatType.CAN_REPEAT_FOREVER)

        states = {
            "RAND": rand,
            "X": _make_move("X", "RAND"),
            "Y": _make_move("Y", "RAND"),
        }
        ai = MonsterAI(states, "RAND")
        moves = _run_ai(ai, rng, 15)

        assert len(ai.state_log) == 15
        assert ai.state_log == moves


# ========================================================================
# 7. First move hold
# ========================================================================

class TestFirstMoveHold:
    def test_cannot_advance_before_perform(self):
        """Initial MoveState can't transition away until performed."""
        rng = Rng(42)
        states = {
            "A": _make_move("A", "B"),
            "B": _make_move("B", "A"),
        }
        ai = MonsterAI(states, "A")

        # Roll multiple times without performing -- should stay on A
        ai.roll_move(rng)
        assert ai.current_move.state_id == "A"
        ai.roll_move(rng)
        assert ai.current_move.state_id == "A"
        ai.roll_move(rng)
        assert ai.current_move.state_id == "A"

        # Now perform, then roll should advance
        ai.on_move_performed()
        ai.roll_move(rng)
        assert ai.current_move.state_id == "B"

    def test_must_perform_once_blocks_transition(self):
        """must_perform_once prevents transition until the move is performed."""
        rng = Rng(0)
        a_state = MoveState("A", _noop, [attack_intent(1)], follow_up_id="B",
                            must_perform_once=True)
        states = {
            "A": a_state,
            "B": _make_move("B", "A"),
        }
        ai = MonsterAI(states, "A")

        # First move hold: can't advance until on_move_performed
        ai.roll_move(rng)
        assert ai.current_move.state_id == "A"

        # on_move_performed clears both first-move hold AND marks performed
        ai.on_move_performed()
        assert a_state._performed_at_least_once is True

        # Now can transition to B
        ai.roll_move(rng)
        assert ai.current_move.state_id == "B"

        # Perform B, roll back to A
        ai.on_move_performed()
        ai.roll_move(rng)
        assert ai.current_move.state_id == "A"

        # A was exited (on_exit_state resets _performed_at_least_once),
        # so must_perform_once holds it again until on_move_performed
        assert a_state._performed_at_least_once is False
        result = ai.roll_move(rng)
        assert result.state_id == "A"  # Held because must_perform_once not yet done

        # After on_move_performed, A can transition again
        ai.on_move_performed()
        assert a_state._performed_at_least_once is True
        ai.roll_move(rng)
        assert ai.current_move.state_id == "B"


# ========================================================================
# 8. Cooldown
# ========================================================================

class TestCooldown:
    def test_cooldown_prevents_recent_use(self):
        """Cooldown=2 prevents a move from appearing within the last 2 log entries."""
        rng = Rng(0)
        rand = RandomBranchState("RAND")
        rand.add_branch("A", MoveRepeatType.CAN_REPEAT_FOREVER, cooldown=2)
        rand.add_branch("B", MoveRepeatType.CAN_REPEAT_FOREVER)

        states = {
            "RAND": rand,
            "A": _make_move("A", "RAND"),
            "B": _make_move("B", "RAND"),
        }
        ai = MonsterAI(states, "RAND")
        moves = _run_ai(ai, rng, 20)

        for i in range(len(moves)):
            if moves[i] == "A":
                # Next 2 entries should not be A
                window = moves[i + 1: i + 3]
                assert "A" not in window, (
                    f"A appeared within cooldown at index {i}: {moves[i:i+4]}"
                )
