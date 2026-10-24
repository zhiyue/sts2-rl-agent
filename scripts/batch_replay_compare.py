#!/usr/bin/env python3
"""Batch slice + transition-compare all recorded run traces.

Classifies each combat segment:
  ok                — simulator matches the game transition-for-transition
  initial-mismatch  — rebuilt state already differs (builder gap)
  transition-mismatch — diverged mid-combat (parity bug or RNG limit)
  empty             — fewer than 3 steps
"""

from __future__ import annotations

import sys
from collections import Counter
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

from sts2_env.parity.bridge_replay import compare_combat_replay, load_replay_trace
from sts2_env.parity.bridge_replay import _diff_values, normalize_bridge_state


def slice_segments(trace) -> list[list]:
    segments, current = [], []
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
    artifacts = Path("artifacts")
    traces = sorted(artifacts.glob("live_trace_run*.json"))
    outcomes: Counter = Counter()
    mismatch_details: list[str] = []
    total_segments = 0

    for trace_path in traces:
        try:
            trace = load_replay_trace(trace_path)
        except Exception as exc:
            print(f"{trace_path.name}: LOAD FAIL {exc}")
            continue
        for index, segment in enumerate(slice_segments(trace)):
            total_segments += 1
            initial = segment[0].resulting_state
            steps = [
                {"action": step.action, "resulting_state": step.resulting_state}
                for step in segment[1:]
            ]
            synthetic = {
                "version": 1,
                "mode": "combat",
                "metadata": {"source": f"{trace_path.name}#{index}"},
                "initial_state": initial,
                "steps": steps,
            }

            # Rebuild check first (isolates builder gaps from transition gaps)
            import sts2_env.powers  # noqa: F401
            from sts2_env.parity.bridge_replay import (
                combat_state_from_bridge_state,
                combat_state_to_bridge_state,
            )

            try:
                rebuilt = combat_state_from_bridge_state(initial)
            except Exception as exc:
                outcomes["rebuild-error"] += 1
                mismatch_details.append(f"{trace_path.name}#{index}: rebuild error {exc}")
                continue
            rebuild_diffs = _diff_values(
                normalize_bridge_state(initial),
                normalize_bridge_state(combat_state_to_bridge_state(rebuilt)),
                "initial",
                [],
            )
            if rebuild_diffs:
                outcomes["initial-mismatch"] += 1
                mismatch_details.append(
                    f"{trace_path.name}#{index} initial: {rebuild_diffs[0]}"
                )
                continue

            out_path = artifacts / f"_slice_tmp.json"
            import json

            out_path.write_text(json.dumps(synthetic, ensure_ascii=False), encoding="utf-8")
            result = compare_combat_replay(out_path, factory="scripts.live_slice_factory:make_combat_from_slice",
                                           factory_kwargs={"trace_path": str(out_path)})
            if result.success:
                outcomes["ok"] += 1
            else:
                first = result.mismatches[0]
                if "step[" in first:
                    outcomes["transition-mismatch"] += 1
                else:
                    outcomes["initial-mismatch"] += 1
                mismatch_details.append(f"{trace_path.name}#{index} ({len(steps)}t): {first[:160]}")

    print(f"\n{len(traces)} traces, {total_segments} combat segments:")
    for key, count in sorted(outcomes.items()):
        print(f"  {key}: {count}")
    print("\nFirst mismatch details:")
    for detail in mismatch_details[:15]:
        print(" -", detail)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
