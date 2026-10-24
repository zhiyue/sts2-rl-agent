"""Necrobinder factory upgrade parity tests backed by reference card models."""

import pytest

import sts2_env.powers  # noqa: F401

from sts2_env.cards.factory import create_card, create_reference_card
from sts2_env.core.enums import CardId


NECROBINDER_FACTORY_UPGRADE_CARD_IDS = (
    CardId.BANSHEES_CRY,
    CardId.BLIGHT_STRIKE,
    CardId.BURY,
    CardId.CALCIFY_CARD,
    CardId.CALL_OF_THE_VOID,
    CardId.DEATHBRINGER,
    CardId.DEBILITATE_CARD,
    CardId.DEFEND_NECROBINDER,
    CardId.DEFILE,
    CardId.DEFY,
    CardId.DELAY,
    CardId.DEMESNE,
    CardId.DEVOUR_LIFE_CARD,
    CardId.ENFEEBLING_TOUCH,
    CardId.ERADICATE,
    CardId.FEAR,
    CardId.FETCH,
    CardId.FLATTEN,
    CardId.FORBIDDEN_GRIMOIRE,
    CardId.FRIENDSHIP,
    CardId.GRAVEBLAST,
    CardId.HANG,
    CardId.HAUNT,
    CardId.INVOKE,
    CardId.LETHALITY_CARD,
    CardId.MELANCHOLY,
    CardId.MISERY,
    CardId.NEGATIVE_PULSE,
    CardId.NO_ESCAPE,
    CardId.OBLIVION,
    CardId.PAGESTORM,
    CardId.PARSE,
    CardId.POKE,
    CardId.PULL_FROM_BELOW,
    CardId.PUTREFY,
    CardId.REAP,
    CardId.REAPER_FORM,
    CardId.REAVE,
    CardId.RIGHT_HAND_HAND,
    CardId.SCOURGE,
    CardId.SCULPTING_STRIKE,
    CardId.SHARED_FATE,
    CardId.SHROUD,
    CardId.SIC_EM,
    CardId.SLEIGHT_OF_FLESH,
    CardId.SNAP,
    CardId.SOW,
    CardId.SPIRIT_OF_ASH,
    CardId.SQUEEZE,
    CardId.STRIKE_NECROBINDER,
    CardId.THE_SCYTHE,
    CardId.TIMES_UP,
    CardId.UNLEASH,
    CardId.VEILPIERCER,
    CardId.WISP,
)


def _necrobinder_card_id(card_id: CardId) -> str:
    return card_id.name


@pytest.mark.parametrize("card_id", NECROBINDER_FACTORY_UPGRADE_CARD_IDS, ids=_necrobinder_card_id)
def test_necrobinder_factory_upgrade_core_metadata_matches_reference(card_id: CardId):
    actual = create_card(card_id, upgraded=True)
    expected = create_reference_card(card_id, upgraded=True, allow_generation=True)

    assert actual.upgraded is True
    assert actual.cost == expected.cost
    assert actual.has_energy_cost_x == expected.has_energy_cost_x
    assert actual.card_type == expected.card_type
    assert actual.target_type == expected.target_type
    assert actual.rarity == expected.rarity
    assert actual.star_cost == expected.star_cost
    assert actual.keywords == expected.keywords
    assert actual.tags == expected.tags


@pytest.mark.parametrize("card_id", NECROBINDER_FACTORY_UPGRADE_CARD_IDS, ids=_necrobinder_card_id)
def test_necrobinder_factory_upgrade_dynamic_values_match_reference(card_id: CardId):
    actual = create_card(card_id, upgraded=True)
    expected = create_reference_card(card_id, upgraded=True, allow_generation=True)

    if card_id == CardId.THE_SCYTHE:
        # docs/CARDS_REFERENCE.md marks TheScythe's Increase var "dynamic", so the
        # doc-derived reference model yields a bogus increase=1 for the upgraded
        # state. Assert the decompiled values instead: TheScythe.cs CanonicalVars
        # has IntVar("Increase", 4) with OnUpgrade Increase.UpgradeValueBy(1m) -> 5,
        # and base damage 13 is unchanged by the upgrade.
        assert actual.effect_vars.get("increase") == 5
        assert actual.base_damage == 13
        return

    for key, expected_value in expected.effect_vars.items():
        actual_value = actual.effect_vars.get(key)
        if key in {"damage", "calc_base"} and actual_value is None:
            assert actual.base_damage == expected_value
            continue
        if key == "block" and actual_value is None:
            assert actual.base_block == expected_value
            continue
        assert actual_value == expected_value
