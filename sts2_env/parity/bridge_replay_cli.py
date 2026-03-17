"""CLI for bridge replay recording metadata inspection and comparison."""

from __future__ import annotations

import argparse
import json
import sys

from sts2_env.parity.bridge_replay import (
    compare_combat_replay,
    compare_run_replay,
    load_replay_trace,
)


def _cmd_show(args: argparse.Namespace) -> int:
    trace = load_replay_trace(args.trace)
    payload = {
        "version": trace.version,
        "mode": trace.mode,
        "metadata": trace.metadata,
        "initial_type": trace.initial_state.get("type"),
        "step_count": len(trace.steps),
    }
    print(json.dumps(payload, indent=2, sort_keys=True))
    return 0


def _cmd_compare(args: argparse.Namespace) -> int:
    mode = args.mode
    if mode is None:
        trace = load_replay_trace(args.trace)
        mode = trace.mode

    if mode == "combat":
        result = compare_combat_replay(
            args.trace,
            factory=args.factory,
            factory_kwargs=_parse_factory_kwargs(args.factory_kw),
        )
    elif mode == "run":
        result = compare_run_replay(
            args.trace,
            factory=args.factory,
            factory_kwargs=_parse_factory_kwargs(args.factory_kw),
        )
    else:
        raise ValueError(f"Unsupported replay mode: {mode!r}")

    if result.success:
        print("comparison: ok")
        return 0

    print("comparison: mismatch")
    for mismatch in result.mismatches:
        print(mismatch)
    return 1


def _parse_factory_kwargs(items: list[str]) -> dict[str, object]:
    parsed: dict[str, object] = {}
    for item in items:
        if "=" not in item:
            raise ValueError(f"Invalid --factory-kw value {item!r}; expected key=value")
        key, value = item.split("=", 1)
        lowered = value.lower()
        if lowered in {"true", "false"}:
            parsed[key] = lowered == "true"
            continue
        if lowered == "none":
            parsed[key] = None
            continue
        try:
            parsed[key] = int(value)
            continue
        except ValueError:
            pass
        parsed[key] = value
    return parsed


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description="Inspect and compare bridge replay traces.",
        formatter_class=argparse.ArgumentDefaultsHelpFormatter,
    )
    subparsers = parser.add_subparsers(dest="command", required=True)

    show_parser = subparsers.add_parser("show", help="Print replay metadata.")
    show_parser.add_argument("trace", help="Path to replay JSON.")
    show_parser.set_defaults(func=_cmd_show)

    compare_parser = subparsers.add_parser("compare", help="Compare replay against simulator.")
    compare_parser.add_argument("trace", help="Path to replay JSON.")
    compare_parser.add_argument(
        "--factory",
        default=None,
        help="module:function factory override. Defaults to replay metadata scenario_factory.",
    )
    compare_parser.add_argument(
        "--mode",
        choices=["combat", "run"],
        default=None,
        help="Replay mode override. Defaults to replay file mode.",
    )
    compare_parser.add_argument(
        "--factory-kw",
        action="append",
        default=[],
        help="Factory keyword argument in key=value form. Repeatable.",
    )
    compare_parser.set_defaults(func=_cmd_compare)

    return parser


def main() -> None:
    parser = build_parser()
    args = parser.parse_args()
    raise SystemExit(args.func(args))


if __name__ == "__main__":
    main()
