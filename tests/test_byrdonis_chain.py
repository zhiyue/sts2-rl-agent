"""Focused regressions for Byrdonis Nest egg -> Hatch rest-site flow."""

import sts2_env.events.shared  # noqa: F401

from sts2_env.cards.factory import create_card
from sts2_env.core.enums import CardId
from sts2_env.events.shared import ByrdonisNest
from sts2_env.run.run_manager import RunManager
from sts2_env.run.run_state import RunState
from sts2_env.run.rest_site import generate_rest_site_options


def test_byrdonis_nest_take_adds_byrdonis_egg_to_deck():
    run_state = RunState(seed=1101, character_id="Ironclad")
    run_state.initialize_run()
    event = ByrdonisNest()

    result = event.choose(run_state, "take")

    assert result.finished
    assert any(card.card_id == CardId.BYRDONIS_EGG for card in run_state.player.deck)


def test_byrdonis_nest_is_only_disallowed_when_player_already_has_event_pet():
    run_state = RunState(seed=1102, character_id="Ironclad")
    run_state.initialize_run()
    event = ByrdonisNest()

    assert event.is_allowed(run_state) is True

    run_state.player.deck.append(create_card(CardId.BYRDONIS_EGG))
    assert event.is_allowed(run_state) is True

    run_state.player.deck = [card for card in run_state.player.deck if card.card_id != CardId.BYRDONIS_EGG]
    run_state.player.obtain_relic("BYRDPIP")
    assert event.is_allowed(run_state) is False


def test_rest_site_injects_hatch_option_when_byrdonis_egg_in_deck():
    run_state = RunState(seed=1103, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck.append(create_card(CardId.BYRDONIS_EGG))

    options = generate_rest_site_options(run_state.player)

    assert any(option.option_id == "HATCH" for option in options)


def test_hatch_option_obtains_byrdpip_and_returns_to_map():
    mgr = RunManager(seed=1104, character_id="Ironclad")
    mgr.run_state.player.deck.append(create_card(CardId.BYRDONIS_EGG))
    mgr._enter_rest_site()

    result = mgr._do_rest_site({"option_id": "HATCH"})

    assert result["phase"] == RunManager.PHASE_MAP_CHOICE
    assert "BYRDPIP" in mgr.run_state.player.relics
