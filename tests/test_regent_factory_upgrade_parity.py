"""Regent factory upgrade parity tests backed by reference card models."""

import pytest

import sts2_env.powers  # noqa: F401

from sts2_env.cards.factory import create_card, create_reference_card
from sts2_env.core.enums import CardId


REGENT_FACTORY_UPGRADE_CARD_IDS = (
    CardId.ALIGNMENT,
    CardId.ARSENAL,
    CardId.ASTRAL_PULSE,
    CardId.BEAT_INTO_SHAPE,
    CardId.BOMBARDMENT,
    CardId.BUNDLE_OF_JOY,
    CardId.CELESTIAL_MIGHT,
    CardId.CHILD_OF_THE_STARS,
    CardId.CLOAK_OF_STARS,
    CardId.COLLISION_COURSE,
    CardId.COMET,
    CardId.COSMIC_INDIFFERENCE,
    CardId.CRESCENT_SPEAR,
    CardId.CRUSH_UNDER,
    CardId.DEFEND_REGENT,
    CardId.DEVASTATE,
    CardId.DYING_STAR,
    CardId.FALLING_STAR,
    CardId.FOREGONE_CONCLUSION,
    CardId.GAMMA_BLAST,
    CardId.GENESIS,
    CardId.GLITTERSTREAM,
    CardId.GUIDING_STAR,
    CardId.HAMMER_TIME,
    CardId.HEAVENLY_DRILL,
    CardId.HEGEMONY,
    CardId.I_AM_INVINCIBLE,
    CardId.KINGLY_KICK,
    CardId.KINGLY_PUNCH,
    CardId.KNOW_THY_PLACE,
    CardId.LUNAR_BLAST,
    CardId.METEOR_SHOWER,
    CardId.MONARCHS_GAZE_CARD,
    CardId.MONOLOGUE_CARD,
    CardId.NEUTRON_AEGIS,
    CardId.PARRY_CARD,
    CardId.PARTICLE_WALL,
    CardId.PATTER,
    CardId.PILLAR_OF_CREATION,
    CardId.PROPHESIZE,
    CardId.REFLECT_CARD,
    CardId.ROYALTIES_CARD,
    CardId.ROYAL_GAMBLE,
    CardId.SEVEN_STARS,
    CardId.SHINING_STRIKE,
    CardId.SPECTRUM_SHIFT,
    CardId.STRIKE_REGENT,
    CardId.SUPERMASSIVE,
    CardId.SWORD_SAGE,
    CardId.TERRAFORMING,
    CardId.THE_SEALED_THRONE,
    CardId.TYRANNY_CARD,
    CardId.VENERATE,
)

def _regent_card_id(card_id: CardId) -> str:
    return card_id.name


# CARDS_REFERENCE.md records these vars as "dynamic", so create_reference_card cannot
# derive them; the factories carry the canonical values from the decompiled C# instead
# (Glitterstream.cs:22: BlockVar("BlockNextTurn", 5), OnUpgrade +2 at Glitterstream.cs:43).
REFERENCE_DYNAMIC_VAR_EXPECTED_OVERRIDES: dict[tuple[CardId, str], int] = {
    (CardId.GLITTERSTREAM, "block_next_turn"): 7,
}


@pytest.mark.parametrize("card_id", REGENT_FACTORY_UPGRADE_CARD_IDS, ids=_regent_card_id)
def test_regent_factory_upgrade_core_metadata_matches_reference(card_id: CardId):
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


@pytest.mark.parametrize("card_id", REGENT_FACTORY_UPGRADE_CARD_IDS, ids=_regent_card_id)
def test_regent_factory_upgrade_dynamic_values_match_reference(card_id: CardId):
    actual = create_card(card_id, upgraded=True)
    expected = create_reference_card(card_id, upgraded=True, allow_generation=True)

    for key, expected_value in expected.effect_vars.items():
        expected_value = REFERENCE_DYNAMIC_VAR_EXPECTED_OVERRIDES.get(
            (card_id, key), expected_value
        )
        actual_value = actual.effect_vars.get(key)
        if key in {"damage", "calc_base"} and actual_value is None:
            assert actual.base_damage == expected_value
            continue
        if key == "block" and actual_value is None:
            assert actual.base_block == expected_value
            continue
        assert actual_value == expected_value
