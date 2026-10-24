"""Silent factory upgrade parity tests backed by reference card models."""

import pytest

import sts2_env.powers  # noqa: F401

from sts2_env.cards.factory import create_card, create_reference_card
from sts2_env.core.enums import CardId


SILENT_FACTORY_UPGRADE_CARD_IDS = (
    CardId.ABRASIVE,
    CardId.ACCELERANT,
    CardId.ACCURACY_CARD,
    CardId.ADRENALINE,
    CardId.AFTERIMAGE_CARD,
    CardId.ANTICIPATE,
    CardId.ASSASSINATE,
    CardId.BACKFLIP,
    CardId.BLADE_OF_INK,
    CardId.BLUR_CARD,
    CardId.BOUNCING_FLASK,
    CardId.BUBBLE_BUBBLE,
    CardId.BULLET_TIME,
    CardId.CALCULATED_GAMBLE,
    CardId.CLOAK_AND_DAGGER,
    CardId.CORROSIVE_WAVE,
    CardId.DAGGER_SPRAY,
    CardId.DASH,
    CardId.DEADLY_POISON,
    CardId.DEFEND_SILENT,
    CardId.DEFLECT,
    CardId.DODGE_AND_ROLL,
    CardId.ECHOING_SLASH,
    CardId.ENVENOM_CARD,
    CardId.EXPERTISE,
    CardId.EXPOSE,
    CardId.FAN_OF_KNIVES_CARD,
    CardId.FINISHER,
    CardId.FLANKING,
    CardId.FLECHETTES,
    CardId.FLICK_FLACK,
    # FOLLOW_THROUGH removed: the card no longer exists in v0.107.1 (there is
    # no FollowThrough.cs under decompiled/; it only survives in the
    # 2026-03-15 backup decompile), so it has no reference definition. The
    # Python enum/factory entry is legacy save-compat only.
    CardId.FOOTWORK,
    CardId.HAZE,
    CardId.INFINITE_BLADES_CARD,
    CardId.KNIFE_TRAP,
    CardId.LEADING_STRIKE,
    CardId.LEG_SWEEP,
    CardId.MALAISE,
    CardId.MASTER_PLANNER,
    CardId.MEMENTO_MORI,
    CardId.MIRAGE,
    CardId.MURDER,
    CardId.NEUTRALIZE,
    CardId.NOXIOUS_FUMES_CARD,
    CardId.OUTBREAK_CARD,
    CardId.PHANTOM_BLADES_CARD,
    CardId.PIERCING_WAIL,
    CardId.PINPOINT,
    CardId.POISONED_STAB,
    CardId.POUNCE,
    CardId.PRECISE_CUT,
    CardId.PREDATOR,
    CardId.REFLEX,
    CardId.RICOCHET,
    CardId.SERPENT_FORM_CARD,
    CardId.SHADOWMELD,
    CardId.SHADOW_STEP,
    CardId.SKEWER,
    CardId.SLICE,
    CardId.SNAKEBITE,
    CardId.SNEAKY_CARD,
    CardId.SPEEDSTER_CARD,
    CardId.STORM_OF_STEEL,
    CardId.STRANGLE,
    CardId.STRIKE_SILENT,
    CardId.SUCKER_PUNCH,
    CardId.SUPPRESS,
    CardId.TACTICIAN,
    CardId.TOOLS_OF_THE_TRADE,
    CardId.TRACKING,
    CardId.UNTOUCHABLE,
    CardId.UP_MY_SLEEVE,
    CardId.WELL_LAID_PLANS,
)


def _silent_card_id(card_id: CardId) -> str:
    return card_id.name


@pytest.mark.parametrize("card_id", SILENT_FACTORY_UPGRADE_CARD_IDS, ids=_silent_card_id)
def test_silent_factory_upgrade_core_metadata_matches_reference(card_id: CardId):
    actual = create_card(card_id, upgraded=True)
    expected = create_reference_card(card_id, upgraded=True, allow_generation=True)

    assert actual.upgraded is expected.upgraded
    assert actual.cost == expected.cost
    assert actual.has_energy_cost_x == expected.has_energy_cost_x
    assert actual.card_type == expected.card_type
    assert actual.target_type == expected.target_type
    assert actual.rarity == expected.rarity
    assert actual.star_cost == expected.star_cost
    assert actual.keywords == expected.keywords
    assert actual.tags == expected.tags


@pytest.mark.parametrize("card_id", SILENT_FACTORY_UPGRADE_CARD_IDS, ids=_silent_card_id)
def test_silent_factory_upgrade_dynamic_values_match_reference(card_id: CardId):
    actual = create_card(card_id, upgraded=True)
    expected = create_reference_card(card_id, upgraded=True, allow_generation=True)

    if card_id is CardId.FAN_OF_KNIVES_CARD:
        # docs/CARDS_REFERENCE.md emits `{Shivs: dynamic}` because the doc
        # generator cannot resolve named CardsVar values, so the doc-backed
        # reference degrades to shivs=0+1. FanOfKnives.cs:9 defines
        # CardsVar("Shivs", 4) and OnUpgrade (lines 32-34) adds 1 -> 5,
        # which the factory encodes directly.
        assert actual.effect_vars["shivs"] == 5
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
