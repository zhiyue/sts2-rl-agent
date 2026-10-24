#!/usr/bin/env python3
"""Slice a complete combat segment out of a recorded run trace.

Usage: python scripts/slice_combat_from_trace.py TRACE [COMBAT_INDEX] [OUT]
Writes a combat-mode replay trace for compare_combat_replay.
"""

from __future__ import annotations

import json
import sys

from sts2_env.parity.bridge_replay import load_replay_trace


def slice_combats(trace_path: str) -> list[dict]:
    trace = load_replay_trace(trace_path)
    segments: list[list] = []
    current: list = []
    for step in trace.steps:
        if step.resulting_state.get("type") == "combat_action":
            current.append(step)
        else:
            if len(current) >= 3:
                segments.append(current)
            current = []
    if len(current) >= 3:
        segments.append(current)
    return segments


def main() -> int:
    trace_path = sys.argv[1] if len(sys.argv) > 1 else "artifacts/live_trace_run005.json"
    combat_index = int(sys.argv[2]) if len(sys.argv) > 2 else 0
    out_path = sys.argv[3] if len(sys.argv) > 3 else "artifacts/combat_slice.json"

    segments = slice_combats(trace_path)
    print(f"{trace_path}: {len(segments)} combat segment(s)")
    if combat_index >= len(segments):
        print(f"combat index {combat_index} out of range")
        return 1
    segment = segments[combat_index]
    out = {
        "version": 1,
        "mode": "combat",
        "metadata": {
            "source": f"{trace_path} combat #{combat_index}",
            "transitions": len(segment) - 1,
        },
        "initial_state": segment[0].resulting_state,
        "steps": [
            {"action": step.action, "resulting_state": step.resulting_state}
            for step in segment[1:]
        ],
    }
    with open(out_path, "w", encoding="utf-8") as handle:
        json.dump(out, handle, ensure_ascii=False, indent=1)
    print(f"wrote {out_path}: initial round {out['initial_state'].get('round')}, "
          f"{len(out['steps'])} transitions")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
