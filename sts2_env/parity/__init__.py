"""Parity helpers and bridge replay comparison harness.

This package intentionally avoids eager imports so its CLI modules can be
invoked directly without tripping simulator import cycles.
"""

from __future__ import annotations

from importlib import import_module

__all__ = [
    "BridgeReplayRecorder",
    "BridgeReplayStep",
    "BridgeReplayTrace",
    "ReplayComparison",
    "combat_state_to_bridge_state",
    "run_manager_to_bridge_state",
    "compare_combat_replay",
    "compare_run_replay",
    "load_replay_trace",
    "save_replay_trace",
]


def __getattr__(name: str):
    if name not in __all__:
        raise AttributeError(name)
    module = import_module("sts2_env.parity.bridge_replay")
    return getattr(module, name)
