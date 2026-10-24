#!/usr/bin/env python3
"""Regenerate docs/CARDS_REFERENCE.md from the decompiled C# card models.

The decompiled source (decompiled/) is the ground truth. Run this after every
game update so the reference table, the factory fallback path, and the
pool-order tests all track the installed game version.

Usage: python scripts/generate_cards_reference.py [--decompiled decompiled] [--out docs/CARDS_REFERENCE.md]
"""

from __future__ import annotations

import argparse
import re
from pathlib import Path

POOL_COLORS = [
    ("IroncladCardPool", "ironclad"),
    ("SilentCardPool", "silent"),
    ("DefectCardPool", "defect"),
    ("NecrobinderCardPool", "necrobinder"),
    ("RegentCardPool", "regent"),
    ("ColorlessCardPool", "colorless"),
    ("StatusCardPool", "status"),
    ("CurseCardPool", "curse"),
    ("EventCardPool", "event"),
    ("QuestCardPool", "quest"),
    ("TokenCardPool", "token"),
    ("DeprecatedCardPool", "unknown"),
]

VAR_SHORT_NAMES = {
    "CalculationBase": "CalcBase",
    "ExtraDamage": "ExtraDamage",
    "CalculatedDamage": "CalculatedDamage",
}

BASE_RE = re.compile(r": base\((?P<cost>-?\d+),\s*CardType\.(?P<type>\w+),\s*CardRarity\.(?P<rarity>\w+),\s*TargetType\.(?P<target>\w+)")
CANON_STAR_COST_RE = re.compile(r"CanonicalStarCost => (\d+)")
KEYWORD_RE = re.compile(r"CardKeyword\.(?P<keyword>\w+)")
CANON_KEYWORDS_BLOCK_RE = re.compile(r"CanonicalKeywords\s*=>.*?(?:\n.*?)*?;\n", re.MULTILINE)
ON_UPGRADE_RE = re.compile(r"protected override void OnUpgrade\(\)\s*\{(?P<body>.*?)\n\t\}", re.DOTALL)
UPGRADE_VALUE_RE = re.compile(r"(?:base\.)?DynamicVars(?:\[\x22(?P<named>\w+)\x22\]|\.(?P<var>\w+))?\.UpgradeValueBy\((?P<amount>-?\d+(?:\.\d+)?)m?\)")
UPGRADE_COST_RE = re.compile(r"EnergyCost\.UpgradeBy\((?P<amount>-?\d+)m?\)")
ADD_KEYWORD_RE = re.compile(r"AddKeyword\(CardKeyword\.(\w+)\)")
REMOVE_KEYWORD_RE = re.compile(r"RemoveKeyword\(CardKeyword\.(\w+)\)")
VAR_ENTRY_RE = re.compile(r"new (?P<kind>\w+?)(?:<(?P<generic>\w+)>)?\((?P<args>[^()]*)\)")
DYNAMIC_VAR_NAME_RE = re.compile(r"new DynamicVar\(\x22(?P<name>\w+)\x22,\s*(?P<value>-?[\d.]+)m?\)")
TAG_RE = re.compile(r"CardTag\.(?P<tag>\w+)")
MAX_UPGRADE_ZERO_RE = re.compile(r"MaxUpgradeLevel => 0")


def canonical_vars_summary(source: str) -> str:
    match = re.search(r"CanonicalVars\s*=>(?P<body>.*?);\n", source, re.DOTALL)
    if not match:
        return "None"
    body = match.group("body")
    if "Mock" in body:
        return "None"
    parts: list[str] = []
    for dm in DYNAMIC_VAR_NAME_RE.finditer(body):
        parts.append(f"{dm.group('name')}: {_fmt(dm.group('value'))}")
    for vm in VAR_ENTRY_RE.finditer(body):
        kind, generic, args = vm.group("kind"), vm.group("generic"), vm.group("args")
        if kind == "DynamicVar":
            continue
        value = args.split(",")[0].strip().rstrip("m")
        if kind == "CalculatedDamageVar":
            parts.append("CalculatedDamage: dynamic")
            continue
        try:
            float(value)
        except ValueError:
            label = value.strip('"')
            parts.append(f"{label}: dynamic")
            continue
        if kind == "PowerVar" and generic:
            parts.append(f"{generic}: {_fmt(value)}")
            continue
        name = VAR_SHORT_NAMES.get(kind[:-3], kind[:-3]) if kind.endswith("Var") else kind
        parts.append(f"{name}: {_fmt(value)}")
    return ", ".join(parts) if parts else "None"


def _fmt(value: str) -> str:
    number = float(value)
    return str(int(number)) if number == int(number) else str(number)


def upgrade_summary(source: str) -> str:
    match = ON_UPGRADE_RE.search(source)
    if match is None:
        if MAX_UPGRADE_ZERO_RE.search(source):
            return "Cannot be upgraded"
        return "No upgrade changes"
    body = match.group("body")
    parts: list[str] = []
    for um in UPGRADE_VALUE_RE.finditer(body):
        var = um.group("named") or um.group("var")
        var = VAR_SHORT_NAMES.get(var, var)
        if var.endswith("Var"):
            var = var[:-3]
        amount = _fmt(um.group("amount"))
        parts.append(f"{var}+{amount}" if not amount.startswith("-") else f"{var}{amount}")
    for cm in UPGRADE_COST_RE.finditer(body):
        amount = int(cm.group("amount"))
        parts.append(f"Cost{amount}" if amount < 0 else f"Cost+{amount}")
    for rm in REMOVE_KEYWORD_RE.finditer(body):
        parts.append(f"Remove {rm.group(1)}")
    for am in ADD_KEYWORD_RE.finditer(body):
        parts.append(f"Add {am.group(1)}")
    return "; ".join(parts) if parts else "No upgrade changes"


def entry_for(path: Path, pool_color: str) -> str:
    source = path.read_text(encoding="utf-8")
    bm = BASE_RE.search(source)
    if bm is None:
        return ""
    cost = int(bm.group("cost"))
    cost_text = "Unplayable" if cost < 0 else str(cost)
    star = CANON_STAR_COST_RE.search(source)
    if star:
        cost_text = f"{cost_text} | StarCost: {star.group(1)}"
    kw_block = CANON_KEYWORDS_BLOCK_RE.search(source)
    keywords = []
    if kw_block:
        seen: set[str] = set()
        for km in KEYWORD_RE.finditer(kw_block.group(0)):
            if km.group("keyword") not in seen:
                seen.add(km.group("keyword"))
                keywords.append(km.group("keyword"))
    tags = sorted({m.group("tag") for m in TAG_RE.finditer(source)})
    from sts2_env.cards.reference_static_metadata import card_id_for_reference_class

    card_id = card_id_for_reference_class(path.stem)
    vars_summary = canonical_vars_summary(source)
    if vars_summary != "None":
        vars_summary = f"{{{vars_summary}}}"
    lines = [
        f"### {path.stem}",
        f"- **ID:** {card_id.name}",
        f"- **Color:** {pool_color}",
        f"- **Cost:** {cost_text}",
        f"- **Type:** {bm.group('type')}",
        f"- **Rarity:** {bm.group('rarity')}",
        f"- **Target:** {bm.group('target')}",
        f"- **Keywords:** {', '.join(keywords) if keywords else 'None'}",
        f"- **Tags:** {', '.join(tags) if tags else 'None'}",
        f"- **Vars:** {vars_summary}",
        f"- **Effect:** (see decompiled {path.stem}.cs)",
        f"- **Upgrade:** {upgrade_summary(source)}",
    ]
    return "\n".join(lines)


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--decompiled", default="decompiled")
    parser.add_argument("--out", default="docs/CARDS_REFERENCE.md")
    args = parser.parse_args()

    cards_dir = Path(args.decompiled) / "MegaCrit.Sts2.Core.Models.Cards"
    pools_dir = Path(args.decompiled) / "MegaCrit.Sts2.Core.Models.CardPools"
    color_by_class: dict[str, str] = {}
    for pool_file, color in POOL_COLORS:
        pool_path = pools_dir / f"{pool_file}.cs"
        if not pool_path.exists():
            continue
        for cm in re.finditer(r"ModelDb\.Card<(\w+)>\(\)", pool_path.read_text(encoding="utf-8")):
            color_by_class.setdefault(cm.group(1), color)

    entries = []
    for path in sorted(cards_dir.glob("*.cs"), key=lambda p: p.stem):
        entry = entry_for(path, color_by_class.get(path.stem, "unknown"))
        if entry:
            entries.append(entry)
    header = (
        "# Cards Reference\n\n"
        "Auto-generated from the decompiled game source by\n"
        "`scripts/generate_cards_reference.py`. Do not edit by hand;\n"
        "re-run the script after every game update.\n\n"
        f"Game version: see GAME_VERSIONS.md.\n\n"
    )
    Path(args.out).write_text(header + "\n\n".join(entries) + "\n", encoding="utf-8")
    print(f"wrote {len(entries)} entries to {args.out}")


if __name__ == "__main__":
    main()
