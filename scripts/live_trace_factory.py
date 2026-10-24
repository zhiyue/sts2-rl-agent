"""Scenario factory for live-recorded bridge traces.

Builds the simulator combat matching a recorded live-game state, so
compare_combat_replay can validate the simulator against the real game's
transitions (L5 in docs/PARITY_ASSURANCE_PLAN.md).
"""

from __future__ import annotations


def make_combat_from_trace(
    trace_path: str = "artifacts/live_trace.json",
    step: int = -1,
    state_type: str = "combat_action",
    rng_seed: int = 42,
):
    """Rebuild the sim combat from the last state of the given type in a trace."""
    from sts2_env.parity.bridge_replay import (
        combat_state_from_bridge_state,
        load_replay_trace,
    )

    trace = load_replay_trace(trace_path)
    candidates = [step.resulting_state for step in trace.steps
                  if step.resulting_state.get("type") == state_type]
    if state_type == trace.initial_state.get("type"):
        candidates.insert(0, trace.initial_state)
    if not candidates:
        raise ValueError(f"trace has no {state_type!r} state")
    chosen = candidates[step]
    return combat_state_from_bridge_state(chosen, rng_seed=rng_seed)
