"""Golden-sample tests for the parity audit parsers.

These tests exist because the audit parsers themselves can go blind: the
monster-stat parser matched only `private static int` and silently skipped
v0.107.1's dominant `private int X =>` form, masking 23 real mismatches.

Every expected value below was hand-derived by reading the decompiled C#
source directly (file cited per entry) — never copied from parser output.
If one of these fails, either the parser regex went blind or the C# changed:
investigate before touching anything else.
"""

import sys
from pathlib import Path

import pytest

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

from sts2_env.cards.reference_static_metadata import (
    reference_dynamic_vars_by_card_id,
    reference_metadata_by_card_id,
    upgraded_reference_dynamic_vars_by_card_id,
    upgraded_reference_metadata_by_card_id,
)
from sts2_env.core.enums import CardId, CardRarity, CardType, TargetType

REPO = Path(__file__).resolve().parents[1]


# ---------------------------------------------------------------------------
# Card static metadata golden samples
# ---------------------------------------------------------------------------
# (card, upgraded) -> {field: expected}; every entry hand-derived from the
# cited decompiled card model constructor / CanonicalKeywords / OnUpgrade.
CARD_STATIC_GOLDEN = [
    # Bash.cs: base(2, Attack, Basic, AnyEnemy), DamageVar(8)+PowerVar<Vuln>(2),
    # OnUpgrade: Damage+2, Vulnerable+1.
    (CardId.BASH, False, dict(cost=2, card_type=CardType.ATTACK, rarity=CardRarity.BASIC,
                              target_type=TargetType.ANY_ENEMY, keywords=frozenset())),
    (CardId.BASH, True, dict(cost=2, keywords=frozenset())),
    # NotYet.cs: base(2, Skill, Rare, Self), Exhaust, HealVar(10), upgrade Heal+3.
    (CardId.NOT_YET, False, dict(cost=2, card_type=CardType.SKILL, rarity=CardRarity.RARE,
                                 target_type=TargetType.SELF, keywords=frozenset({"exhaust"}))),
    (CardId.NOT_YET, True, dict(keywords=frozenset({"exhaust"}))),
    # Scare.cs: base(0, Skill, Uncommon, AllEnemies), Exhaust; upgrade removes Exhaust.
    (CardId.SCARE, False, {}),
    # Wither.cs: base(-1, Status, Status, None), Unplayable, MaxUpgradeLevel => 0.
    (CardId.WITHER, False, dict(cost=-1, card_type=CardType.STATUS, rarity=CardRarity.STATUS,
                                target_type=TargetType.NONE, keywords=frozenset({"unplayable"}),
                                max_upgrade_level=0)),
    # TheSealedThrone.cs: base(1, Power, Ancient, Self), CanonicalStarCost 3,
    # OnUpgrade: EnergyCost.UpgradeBy(-1).
    (CardId.THE_SEALED_THRONE, False, dict(cost=1, card_type=CardType.POWER,
                                           rarity=CardRarity.ANCIENT,
                                           target_type=TargetType.SELF, star_cost=3)),
    (CardId.THE_SEALED_THRONE, True, dict(cost=0)),
    # Alignment.cs: base(0, Skill, Uncommon, Self), CanonicalStarCost 3,
    # OnUpgrade: Energy +1 (cost unchanged).
    (CardId.ALIGNMENT, False, dict(cost=0, card_type=CardType.SKILL,
                                   rarity=CardRarity.UNCOMMON,
                                   target_type=TargetType.SELF, star_cost=3)),
    (CardId.ALIGNMENT, True, dict(cost=0)),
    # Arsenal.cs: base(1, Power, Rare, Self), PowerVar<ArsenalPower>(1);
    # OnUpgrade: AddKeyword(Innate).
    (CardId.ARSENAL, False, dict(cost=1, card_type=CardType.POWER, rarity=CardRarity.RARE,
                                 target_type=TargetType.SELF, keywords=frozenset())),
    (CardId.ARSENAL, True, dict(keywords=frozenset({"innate"}))),
    # Folly.cs: base(-1, Curse, Curse, None), CanonicalKeywords = Unplayable,
    # Eternal, Innate, Ethereal (four-entry array).
    (CardId.FOLLY, False, dict(cost=-1, card_type=CardType.CURSE, rarity=CardRarity.CURSE,
                               keywords=frozenset({"unplayable", "eternal", "innate", "ethereal"}))),
    # BansheesCry.cs: base(9, Attack, Rare, AllEnemies); OnUpgrade:
    # EnergyCost.UpgradeBy(-2) -> 7. (DamageVar(33), EnergyVar(2) in the vars test.)
    (CardId.BANSHEES_CRY, False, dict(cost=9, card_type=CardType.ATTACK, rarity=CardRarity.RARE,
                                      target_type=TargetType.ALL_ENEMIES)),
    (CardId.BANSHEES_CRY, True, dict(cost=7)),
]


CARD_DYNAMIC_GOLDEN = [
    # Bash.cs: DamageVar(8), PowerVar<VulnerablePower>(2); +2 / +1.
    (CardId.BASH, {"damage": 8, "vulnerable": 2}, {"damage": 10, "vulnerable": 3}),
    # NotYet.cs: HealVar(10); OnUpgrade Heal.UpgradeValueBy(3).
    (CardId.NOT_YET, {"heal": 10}, {"heal": 13}),
    # Scare.cs: no CanonicalVars at all.
    (CardId.SCARE if "SCARE" in CardId.__members__ else CardId.SCAR_E, {}, {}),
    # Wither.cs: DamageVar(3, Unpowered|Move); FakeUpgrade +3 is NOT OnUpgrade,
    # so the parsed upgrade vars stay at 3 (MaxUpgradeLevel 0).
    (CardId.WITHER, {"damage": 3}, {"damage": 3}),
    # TheSealedThrone.cs: no CanonicalVars.
    (CardId.THE_SEALED_THRONE, {}, {}),
    # Alignment.cs: EnergyVar(2); OnUpgrade Energy +1.
    (CardId.ALIGNMENT, {"energy": 2}, {"energy": 3}),
    # Arsenal.cs: PowerVar<ArsenalPower>(1); upgrade only adds a keyword.
    (CardId.ARSENAL, {"arsenal": 1}, {"arsenal": 1}),
    # BansheesCry.cs: DamageVar(33) + EnergyVar(2); upgrade only cuts cost.
    (CardId.BANSHEES_CRY, {"damage": 33, "energy": 2}, {"damage": 33, "energy": 2}),
    # GrandFinale.cs: base(0, Attack, Rare, AllEnemies), DamageVar(60); +15.
    (CardId.GRAND_FINALE, {"damage": 60}, {"damage": 75}),
    # Hemokinesis.cs: HpLossVar(2) + DamageVar(15); OnUpgrade Damage +5 only.
    (CardId.HEMOKINESIS, {"hp_loss": 2, "damage": 15}, {"hp_loss": 2, "damage": 20}),
    # Conflagration.cs: DamageVar(2) + RepeatVar(4); OnUpgrade Repeat +1 only.
    (CardId.CONFLAGRATION, {"damage": 2, "repeat": 4}, {"damage": 2, "repeat": 5}),
    # BorrowedTime.cs: EnergyVar(4) + EnergyVar("ExtraCost", 1);
    # OnUpgrade Energy +2 only (extra cost unchanged).
    (CardId.BORROWED_TIME, {"energy": 4, "extra_cost": 1},
     {"energy": 6, "extra_cost": 1}),
    # Loop.cs: DynamicVar("Loop", 1) — named var; OnUpgrade ["Loop"] +1.
    (CardId.LOOP_CARD, {"loop": 1}, {"loop": 2}),
]


def _mask(meta, expected: dict) -> dict:
    return {field: getattr(meta, field) for field in expected}


@pytest.mark.parametrize("card_id,upgraded,expected", CARD_STATIC_GOLDEN)
def test_static_metadata_parser_golden(card_id, upgraded, expected):
    source = (upgraded_reference_metadata_by_card_id() if upgraded
              else reference_metadata_by_card_id())
    meta = source[card_id]
    actual = _mask(meta, expected)
    assert actual == expected, f"parser output for {card_id.name} drifted from C#: {actual}"


@pytest.mark.parametrize("card_id,base_expected,upgraded_expected", CARD_DYNAMIC_GOLDEN)
def test_dynamic_var_parser_golden(card_id, base_expected, upgraded_expected):
    base_vars = reference_dynamic_vars_by_card_id().get(card_id, {})
    upgraded_vars = upgraded_reference_dynamic_vars_by_card_id().get(card_id, {})
    assert base_vars == base_expected, f"{card_id.name} base vars drifted: {base_vars}"
    assert upgraded_vars == upgraded_expected, f"{card_id.name} upgraded vars drifted: {upgraded_vars}"


# ---------------------------------------------------------------------------
# Monster stats parser golden samples
# ---------------------------------------------------------------------------
# parse_csharp_monsters returns {class: {prop: (base, asc) | "ALIAS_MIN",
# "consts": {name: (base, asc)}}}. Samples deliberately cover both constant
# styles (private static int / private int), AscensionHelper scaling, plain
# constants, and the `MaxInitialHp => MinInitialHp` alias.
MONSTER_GOLDEN = {
    # Guardbot.cs (lowercase b — /mnt/c mounts are case-insensitive, the
    # parser key is the file stem): Min => (Tough,17,16), Max => (Tough,21,20).
    "Guardbot": {
        "MinInitialHp": (16, 17),
        "MaxInitialHp": (20, 21),
    },
    # WaterfallGiant.cs: Min => (Tough,250,240); Max => MinInitialHp (fixed HP).
    "WaterfallGiant": {
        "MinInitialHp": (240, 250),
        "MaxInitialHp": "ALIAS_MIN",
    },
    # Byrdonis.cs (`private static int` style): Min (90,81), Max (90,84),
    # PeckDamage (3,4), PeckRepeat (3,3), SwoopDamage (17,19).
    "Byrdonis": {
        "MinInitialHp": (81, 90),
        "MaxInitialHp": (84, 90),
        "consts": {
            "PeckDamage": (3, 4),
            "PeckRepeat": (3, 3),
            "SwoopDamage": (17, 19),
        },
    },
    # Aeonglass.cs (`private int` instance style — the style the parser used
    # to miss entirely): Min (512,535), Max alias; EbbDamage (26,32),
    # EbbBlock 33 plain, EyeLasersDamage (11,12), EyeLasersRepeat 2 plain,
    # IncreasingIntensityBaseStrength (3,4), WitherAmount (1,2).
    "Aeonglass": {
        "MinInitialHp": (512, 535),
        "MaxInitialHp": "ALIAS_MIN",
        "consts": {
            "EbbDamage": (26, 32),
            "EbbBlock": (33, 33),
            "EyeLasersDamage": (11, 12),
            "EyeLasersRepeat": (2, 2),
            "IncreasingIntensityBaseStrength": (3, 4),
            "WitherAmount": (1, 2),
        },
    },
}


def test_monster_stats_parser_golden():
    sys.path.insert(0, str(REPO / "scripts"))
    from audit_monster_stats import parse_csharp_monsters

    parsed = parse_csharp_monsters(REPO / "decompiled" / "MegaCrit.Sts2.Core.Models.Monsters")
    for class_name, expected in MONSTER_GOLDEN.items():
        assert class_name in parsed, f"{class_name} missing from parser output entirely"
        info = parsed[class_name]
        for field, expected_value in expected.items():
            if field == "consts":
                for const_name, const_value in expected_value.items():
                    actual = info["consts"].get(const_name)
                    assert actual == const_value, (
                        f"{class_name}.{const_name}: parser sees {actual}, "
                        f"C# says {const_value}"
                    )
            else:
                actual = info.get(field)
                assert actual == expected_value, (
                    f"{class_name}.{field}: parser sees {actual}, C# says {expected_value}"
                )
