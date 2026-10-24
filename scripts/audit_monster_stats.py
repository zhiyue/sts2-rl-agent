#!/usr/bin/env python3
"""Audit monster HP/damage constants against the decompiled C# monster models.

Ground truth: decompiled/MegaCrit.Sts2.Core.Models.Monsters/*.cs
  - MinInitialHp / MaxInitialHp => AscensionHelper.GetValueIfAscension(level, asc, base)
  - static int consts with the same pattern (damage/counts)
Python side: sts2_env/monsters/*.py constants named {MONSTER_ID}_*.

Usage: python scripts/audit_monster_stats.py [--show-missing]
"""

from __future__ import annotations

import argparse
import re
from pathlib import Path

ASCENSION_RE = re.compile(
    r"AscensionHelper\.GetValueIfAscension\(\s*AscensionLevel\.\w+,\s*(?P<asc>-?\d+),\s*(?P<base>-?\d+)\s*\)"
)
HP_PROP_RE = re.compile(
    r"public override int (?P<prop>MinInitialHp|MaxInitialHp)\s*=>\s*(?P<expr>.*?);",
    re.DOTALL,
)
STATIC_CONST_RE = re.compile(
    r"private static int (?P<name>\w+)\s*=>\s*(?P<expr>[^;]+);"
)
PYTHON_MONSTER_ID_RE = re.compile(r'(?P<prefix>\w+)_MONSTER_ID = "(?P<mid>[A-Z0-9_]+)"')


def camel_to_upper_snake(name: str) -> str:
    return re.sub(r"(?<=[a-z0-9])(?=[A-Z])", "_", name).upper()


def _parse_values(expr: str) -> tuple[int, int] | None:
    m = ASCENSION_RE.search(expr)
    if m:
        return int(m.group("base")), int(m.group("asc"))
    plain = re.fullmatch(r"\s*(\d+)\s*", expr)
    if plain:
        value = int(plain.group(1))
        return value, value
    return None


def parse_csharp_monsters(monsters_dir: Path) -> dict[str, dict]:
    result: dict[str, dict] = {}
    for path in sorted(monsters_dir.glob("*.cs")):
        source = path.read_text(encoding="utf-8")
        if "MonsterModel" not in source:
            continue
        info: dict = {"class": path.stem, "consts": {}}
        for prop in HP_PROP_RE.finditer(source):
            if re.search(rf"{prop.group('prop')}\s*=>\s*MinInitialHp\s*;", prop.group("expr")):
                # `MaxInitialHp => MinInitialHp` — fixed HP; max mirrors min.
                info[prop.group("prop")] = "ALIAS_MIN"
                continue
            values = _parse_values(prop.group("expr"))
            if values:
                info[prop.group("prop")] = values
        for const in STATIC_CONST_RE.finditer(source):
            values = _parse_values(const.group("expr"))
            if values:
                info["consts"][const.group("name")] = values
        result[path.stem] = info
    return result


def parse_python_monsters(env_dir: Path) -> dict[str, dict[str, int]]:
    constants: dict[str, dict[str, int]] = {}
    for path in sorted(env_dir.glob("*.py")):
        for line in path.read_text(encoding="utf-8").splitlines():
            m = re.match(r"([A-Z0-9_]+) = (-?\d+)\s*(?:#.*)?$", line.strip())
            if m:
                constants.setdefault(path.name, {})[m.group(1)] = int(m.group(2))
    return constants


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--decompiled", default="decompiled")
    parser.add_argument("--env", default="sts2_env/monsters")
    parser.add_argument("--show-missing", action="store_true")
    args = parser.parse_args()

    csharp = parse_csharp_monsters(Path(args.decompiled) / "MegaCrit.Sts2.Core.Models.Monsters")
    module_constants = parse_python_monsters(Path(args.env))

    # Map Python monster ids -> prefix + constants (merged across modules).
    py_by_id: dict[str, dict[str, int]] = {}
    for path, constants in module_constants.items():
        text = (Path(args.env) / path).read_text(encoding="utf-8")
        for m in PYTHON_MONSTER_ID_RE.finditer(text):
            prefix = m.group("prefix")
            mid = m.group("mid")
            for name, value in constants.items():
                if name.startswith(f"{prefix}_"):
                    py_by_id.setdefault(mid, {})[name] = value

    mismatches: list[str] = []
    hp_checked = 0
    missing_python: list[str] = []
    const_checked = 0
    const_mismatches: list[str] = []

    for class_name, info in sorted(csharp.items()):
        monster_id = camel_to_upper_snake(class_name)
        py = py_by_id.get(monster_id)
        if py is None:
            missing_python.append(class_name)
            continue
        min_values = info.get("MinInitialHp")
        max_values = info.get("MaxInitialHp")
        if max_values == "ALIAS_MIN":
            max_values = min_values
        if min_values is None and max_values is None:
            continue
        hp_checked += 1
        base_min, tough_min = min_values if min_values else max_values
        base_max, tough_max = max_values if max_values else min_values
        # Naming variants seen in the Python modules: the standard
        # {ID}_BASE_MIN_HP family, an _INITIAL_ infix, and a prefix-less
        # {ID}_MIN_HP form used by multi-segment monsters.
        candidates_min = (
            f"{monster_id}_BASE_MIN_HP",
            f"{monster_id}_BASE_INITIAL_MIN_HP",
            f"{monster_id}_MIN_HP",
        )
        candidates_max = (
            f"{monster_id}_BASE_MAX_HP",
            f"{monster_id}_BASE_INITIAL_MAX_HP",
            f"{monster_id}_MAX_HP",
        )
        candidates_tough_min = (
            f"{monster_id}_TOUGH_MIN_HP",
            f"{monster_id}_TOUGH_INITIAL_MIN_HP",
        )
        candidates_tough_max = (
            f"{monster_id}_TOUGH_MAX_HP",
            f"{monster_id}_TOUGH_INITIAL_MAX_HP",
        )
        actual_min = next((py[name] for name in candidates_min if name in py), None)
        actual_max = next((py[name] for name in candidates_max if name in py), None)
        actual_tough_min = next((py[name] for name in candidates_tough_min if name in py), None)
        actual_tough_max = next((py[name] for name in candidates_tough_max if name in py), None)
        single_base = py.get(f"{monster_id}_BASE_HP")
        single_tough = py.get(f"{monster_id}_TOUGH_HP")
        if actual_min is None and single_base is not None:
            # Single-value naming is only valid when the C# HP is fixed.
            if base_min == base_max and tough_min == tough_max:
                actual_min, actual_max = single_base, single_base
                actual_tough_min = actual_tough_max = single_tough
            else:
                mismatches.append(
                    f"{monster_id}: fixed-HP constants {single_base}/{single_tough} "
                    f"but C# now rolls a range {base_min}-{base_max} (tough {tough_min}-{tough_max})"
                )
                continue
        if actual_min is None and actual_max is None:
            mismatches.append(f"{monster_id}: missing HP constants (C# {base_min}-{base_max})")
            continue
        if actual_min is not None and actual_min != base_min:
            mismatches.append(f"{monster_id}.BASE_MIN_HP: {actual_min} != {base_min}")
        if actual_max is not None and actual_max != base_max:
            mismatches.append(f"{monster_id}.BASE_MAX_HP: {actual_max} != {base_max}")
        if actual_tough_min is not None and actual_tough_min != tough_min:
            mismatches.append(f"{monster_id}.TOUGH_MIN_HP: {actual_tough_min} != {tough_min}")
        if actual_tough_max is not None and actual_tough_max != tough_max:
            mismatches.append(f"{monster_id}.TOUGH_MAX_HP: {actual_tough_max} != {tough_max}")
        for const_name, (base, asc) in sorted(info["consts"].items()):
            snake = camel_to_upper_snake(const_name)
            candidates = [
                f"{monster_id}_BASE_{snake}",
                f"{monster_id}_TOUGH_{snake}",
                f"{monster_id}_{snake}",
            ]
            actual = {name: py[name] for name in candidates if name in py}
            if not actual:
                continue
            for name, value in actual.items():
                const_checked += 1
                if value == base or value == asc:
                    continue
                const_mismatches.append(
                    f"{monster_id}.{name}: {value} != C# {const_name} (base {base} / asc {asc})"
                )

    print(f"C# monsters: {len(csharp)}; mapped to Python: {len(csharp) - len(missing_python)}")
    print(f"HP values compared: {hp_checked}; damage/count consts compared: {const_checked}")
    print(f"HP mismatches: {len(mismatches)}; const mismatches: {len(const_mismatches)}; "
          f"unmapped C# monsters: {len(missing_python)}")
    for row in mismatches + const_mismatches:
        print(" ", row)
    if args.show_missing and missing_python:
        print("Unmapped C# monster classes:")
        for name in missing_python:
            print("  ", name)


if __name__ == "__main__":
    main()
