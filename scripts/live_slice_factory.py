"""Factory rebuilding the sliced live combat (see PARITY_ASSURANCE_PLAN D4)."""

from __future__ import annotations


def make_combat_from_slice(trace_path: str = "artifacts/combat_slice_001.json", rng_seed: int = 42):
    import json

    from sts2_env.parity.bridge_replay import combat_state_from_bridge_state

    state = json.load(open(trace_path))["initial_state"]
    return combat_state_from_bridge_state(state, rng_seed=rng_seed)
