"""Focused tests for shared event parity improvements."""

import sts2_env.events.shared  # noqa: F401

from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.cards.status import make_doubt, make_poor_sleep, make_regret
from sts2_env.run.run_manager import RunManager
from sts2_env.run.run_state import RunState
from sts2_env.events.shared import BattlewornDummy, RoundTeaParty, Trial, TrashHeap, UnrestSite


def test_round_tea_party_pick_fight_is_multi_page_and_grants_relic():
    run_state = RunState(seed=7, character_id="Ironclad")
    run_state.initialize_run()
    event = RoundTeaParty()

    result = event.choose(run_state, "pick_fight")
    assert not result.finished
    assert result.next_options[0].option_id == "continue_fight"

    starting_hp = run_state.player.current_hp
    starting_relics = len(run_state.player.relics)
    result = event.choose(run_state, "continue_fight")
    assert result.finished
    assert run_state.player.current_hp == starting_hp - 11
    assert len(run_state.player.relics) == starting_relics + 1


def test_trial_accept_randomizes_variant_and_merchant_guilty_adds_rewards():
    run_state = RunState(seed=11, character_id="Ironclad")
    run_state.initialize_run()
    run_state.rng.niche.next_int = lambda low, high: 0  # merchant branch
    event = Trial()
    event.generate_initial_options(run_state)

    result = event.choose(run_state, "accept")
    assert not result.finished
    option_ids = {option.option_id for option in result.next_options}
    assert option_ids == {"merchant_guilty", "merchant_innocent"}

    starting_relics = len(run_state.player.relics)
    result = event.choose(run_state, "merchant_guilty")
    assert result.finished
    assert any(card.card_id == make_regret().card_id for card in run_state.player.deck)
    assert len(run_state.player.relics) == starting_relics + 2


def test_trial_nondescript_guilty_surfaces_card_rewards_through_run_manager():
    mgr = RunManager(seed=13, character_id="Ironclad")
    mgr._phase = RunManager.PHASE_EVENT
    event = Trial()
    mgr._event_model = event
    mgr._event_options = event.generate_initial_options(mgr.run_state)
    mgr.run_state.rng.niche.next_int = lambda low, high: 2  # nondescript branch

    first = mgr._do_event_choice({"option_id": "accept"})
    assert first["phase"] == RunManager.PHASE_EVENT

    second = mgr._do_event_choice({"option_id": "nondescript_guilty"})
    assert second["phase"] == RunManager.PHASE_CARD_REWARD
    assert any(card.card_id == make_doubt().card_id for card in mgr.run_state.player.deck)

    actions = mgr.get_available_actions()
    reward_actions = [action for action in actions if action["action"] == "pick_card"]
    assert len(reward_actions) == 3


def test_trial_merchant_innocent_uses_pending_event_card_choice():
    mgr = RunManager(seed=41, character_id="Ironclad")
    mgr.run_state.player.deck = create_ironclad_starter_deck()
    mgr._phase = RunManager.PHASE_EVENT
    event = Trial()
    mgr._event_model = event
    mgr._event_options = event.generate_initial_options(mgr.run_state)
    mgr.run_state.rng.niche.next_int = lambda low, high: 0

    mgr._do_event_choice({"option_id": "accept"})
    result = mgr._do_event_choice({"option_id": "merchant_innocent"})
    assert result["phase"] == RunManager.PHASE_EVENT

    actions = mgr.get_available_actions()
    choose_actions = [action for action in actions if action["action"] == "choose"]
    assert choose_actions

    mgr.take_action({"action": "choose", "index": 0})
    mgr.take_action({"action": "choose", "index": 1})
    final = mgr.take_action({"action": "confirm_choice"})
    assert final["phase"] == RunManager.PHASE_MAP_CHOICE


def test_trial_double_down_ends_run():
    mgr = RunManager(seed=17, character_id="Ironclad")
    mgr._phase = RunManager.PHASE_EVENT
    event = Trial()
    mgr._event_model = event
    mgr._event_options = event.generate_initial_options(mgr.run_state)

    first = mgr._do_event_choice({"option_id": "reject"})
    assert first["phase"] == RunManager.PHASE_EVENT

    second = mgr._do_event_choice({"option_id": "double_down"})
    assert second["phase"] == RunManager.PHASE_RUN_OVER


def test_battleworn_dummy_and_trash_heap_surface_real_rewards():
    run_state = RunState(seed=19, character_id="Ironclad")
    run_state.initialize_run()

    dummy = BattlewornDummy()
    result = dummy.choose(run_state, "setting_1")
    rewards = result.rewards["reward_objects"]
    assert len(rewards) == 1
    assert rewards[0].reward_type.name == "POTION"

    heap = TrashHeap()
    result = heap.choose(run_state, "dive_in")
    rewards = result.rewards["reward_objects"]
    assert len(rewards) == 1
    assert rewards[0].reward_type.name == "RELIC"


def test_unrest_site_rest_adds_poor_sleep_curse():
    run_state = RunState(seed=23, character_id="Ironclad")
    run_state.initialize_run()
    event = UnrestSite()

    result = event.choose(run_state, "rest")
    assert result.finished
    assert any(card.card_id == make_poor_sleep().card_id for card in run_state.player.deck)
