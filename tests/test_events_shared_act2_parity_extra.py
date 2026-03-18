"""Additional focused parity tests for Shared / Act 2 events."""

import sts2_env.events.act2  # noqa: F401
import sts2_env.events.shared  # noqa: F401

from sts2_env.cards.factory import create_card
from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.cards.status import make_bad_luck, make_spore_mind
from sts2_env.core.enums import CardId
from sts2_env.events.act2 import RelicTrader, SlipperyBridge, Symbiote
from sts2_env.events.shared import LostWisp, Reflections
from sts2_env.run.run_state import RunState


def _make_run_state(seed: int = 91) -> RunState:
    run_state = RunState(seed=seed, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()
    return run_state


def test_relic_trader_excludes_starter_relics_and_swaps_selected_relic():
    run_state = _make_run_state(91)
    run_state.current_act_index = 1
    for relic_id in ("ANCHOR", "VAJRA", "PEAR", "JUZU_BRACELET", "LANTERN"):
        run_state.player.obtain_relic(relic_id)

    event = RelicTrader()
    options = event.generate_initial_options(run_state)

    assert len(options) == 3
    assert "BURNING_BLOOD" not in event._owned_relic_choices  # noqa: SLF001
    old_relic = event._owned_relic_choices[0]  # noqa: SLF001
    new_relic = event._new_relic_choices[0]  # noqa: SLF001
    starting_relics = len(run_state.player.relics)

    result = event.choose(run_state, "trade_0")

    assert result.finished
    assert len(run_state.player.relics) == starting_relics
    assert old_relic not in run_state.player.relics
    assert new_relic in run_state.player.relics


def test_slippery_bridge_hold_on_escalates_damage_and_overcome_removes_card():
    run_state = _make_run_state(92)
    run_state.total_floor = 7
    event = SlipperyBridge()

    options = event.generate_initial_options(run_state)
    assert options[1].description == "Take 3 damage, reroll card"

    starting_hp = run_state.player.current_hp
    first = event.choose(run_state, "hold_on")
    assert not first.finished
    assert run_state.player.current_hp == starting_hp - 3
    assert first.next_options[1].description == "Take 4 damage, reroll card"

    second = event.choose(run_state, "hold_on")
    assert not second.finished
    assert run_state.player.current_hp == starting_hp - 7
    assert second.next_options[1].description == "Take 5 damage, reroll card"

    deck_before = len(run_state.player.deck)
    overcome = event.choose(run_state, "overcome")
    assert overcome.finished
    assert len(run_state.player.deck) == deck_before - 1


def test_symbiote_kill_fire_transforms_selected_card_via_pending_choice():
    run_state = _make_run_state(93)
    target_card = run_state.player.deck[0]
    original_id = target_card.card_id
    event = Symbiote()

    locked_state = _make_run_state(931)
    locked_state.player.deck = [make_spore_mind(), make_spore_mind()]
    locked_options = event.generate_initial_options(locked_state)
    assert [option.enabled for option in locked_options] == [False, True]

    result = event.choose(run_state, "kill_fire")

    assert not result.finished
    assert event.pending_choice is not None
    assert event.pending_choice.options[0].card is target_card

    resolved = event.resolve_pending_choice(0)

    assert resolved.finished
    assert event.pending_choice is None
    assert target_card.card_id != original_id


def test_lost_wisp_search_grants_the_rolled_gold_amount():
    run_state = _make_run_state(94)
    event = LostWisp()

    options = event.generate_initial_options(run_state)
    gold_option = next(option for option in options if option.option_id == "search")
    expected_gold = event._gold  # noqa: SLF001
    starting_gold = run_state.player.gold

    result = event.choose(run_state, "search")

    assert gold_option.description == f"Gain {expected_gold} gold"
    assert result.finished
    assert run_state.player.gold == starting_gold + expected_gold


def test_reflections_touch_downgrades_two_and_upgrades_four_cards():
    run_state = _make_run_state(95)
    run_state.player.deck = [
        create_card(CardId.STRIKE_IRONCLAD, upgraded=True),
        create_card(CardId.DEFEND_IRONCLAD, upgraded=True),
        create_card(CardId.BASH),
        create_card(CardId.ANGER),
        create_card(CardId.SHRUG_IT_OFF),
        create_card(CardId.POMMEL_STRIKE),
    ]
    event = Reflections()

    result = event.choose(run_state, "touch")

    assert result.finished
    assert len(run_state.player.deck) == 6
    assert sum(1 for card in run_state.player.deck if card.upgraded) == 4


def test_reflections_shatter_duplicates_deck_and_adds_bad_luck():
    run_state = _make_run_state(96)
    run_state.player.deck = [
        create_card(CardId.BASH),
        create_card(CardId.ANGER),
    ]
    event = Reflections()

    result = event.choose(run_state, "shatter")

    assert result.finished
    assert len(run_state.player.deck) == 5
    assert sum(1 for card in run_state.player.deck if card.card_id == CardId.BASH) == 2
    assert sum(1 for card in run_state.player.deck if card.card_id == CardId.ANGER) == 2
    assert any(card.card_id == make_bad_luck().card_id for card in run_state.player.deck)
