"""Focused tests for Act 2 event state changes."""

import sts2_env.events.act2  # noqa: F401

from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.cards.status import make_spore_mind
from sts2_env.events.act2 import (
    DollRoom,
    EndlessConveyor,
    FieldOfManSizedHoles,
    JungleMazeAdventure,
    LuminousChoir,
    MorphicGrove,
    PotionCourier,
    RanwidTheElder,
    RelicTrader,
    Symbiote,
    WhisperingHollow,
)
from sts2_env.run.reward_objects import EnchantCardsReward, RemoveCardReward, TransformCardsReward
from sts2_env.potions.base import create_potion
from sts2_env.run.run_manager import RunManager
from sts2_env.run.run_state import RunState


def test_luminous_choir_and_morphic_grove_apply_real_deck_changes():
    run_state = RunState(seed=29, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()
    run_state.player.gold = 200
    starting_deck = len(run_state.player.deck)

    choir = LuminousChoir()
    choir.calculate_vars(run_state)
    assert choir.is_allowed(run_state) is True
    choir_options = choir.generate_initial_options(run_state)
    assert [option.option_id for option in choir_options] == ["reach", "tribute"]
    assert all(option.enabled for option in choir_options)
    result = choir.choose(run_state, "reach")
    assert not result.finished
    choir.resolve_pending_choice(0)
    choir.resolve_pending_choice(1)
    result = choir.resolve_pending_choice(None)
    assert result.finished
    assert len(run_state.player.deck) == starting_deck - 1
    assert any(card.card_id == make_spore_mind().card_id for card in run_state.player.deck)

    grove = MorphicGrove()
    before_ids = [card.card_id for card in run_state.player.deck]
    result = grove.choose(run_state, "group")
    assert not result.finished
    grove.resolve_pending_choice(0)
    grove.resolve_pending_choice(1)
    result = grove.resolve_pending_choice(None)
    assert result.finished
    after_ids = [card.card_id for card in run_state.player.deck]
    assert before_ids != after_ids


def test_potion_courier_ranwid_and_whispering_hollow_change_inventory():
    run_state = RunState(seed=31, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()
    run_state.current_act_index = 1
    run_state.player.gold = 200
    run_state.player.add_potion(create_potion("FirePotion"))
    run_state.player.obtain_relic("BURNING_BLOOD")

    courier = PotionCourier()
    courier.choose(run_state, "grab")
    assert len(run_state.player.held_potions()) >= 3

    ranwid = RanwidTheElder()
    starting_relics = len(run_state.player.relics)
    ranwid.choose(run_state, "gold")
    assert len(run_state.player.relics) == starting_relics + 1

    hollow = WhisperingHollow()
    starting_hp = run_state.player.current_hp
    before_ids = [card.card_id for card in run_state.player.deck]
    result = hollow.choose(run_state, "hug")
    assert not result.finished
    hollow.resolve_pending_choice(0)
    after_ids = [card.card_id for card in run_state.player.deck]
    assert run_state.player.current_hp == starting_hp - 9
    assert before_ids != after_ids


def test_event_added_card_triggers_run_level_relic_hook():
    run_state = RunState(seed=33, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()
    run_state.player.obtain_relic("LUCKY_FYSH")
    starting_gold = run_state.player.gold

    choir = LuminousChoir()
    choir.calculate_vars(run_state)
    result = choir.choose(run_state, "reach")
    assert not result.finished
    choir.resolve_pending_choice(0)
    choir.resolve_pending_choice(1)
    result = choir.resolve_pending_choice(None)

    assert result.finished
    assert run_state.player.gold == starting_gold + 15


def test_luminous_choir_blocks_event_entry_and_tribute_when_gold_is_too_low():
    blocked = RunState(seed=2901, character_id="Ironclad")
    blocked.initialize_run()
    blocked.player.deck = create_ironclad_starter_deck()
    blocked.player.gold = 148
    choir = LuminousChoir()

    assert choir.is_allowed(blocked) is False

    blocked.player.gold = 149
    blocked.rng.up_front.next_int = lambda low, high: 0
    options = choir.generate_initial_options(blocked)
    assert [option.option_id for option in options] == ["reach", "tribute"]
    assert [option.enabled for option in options] == [True, True]


def test_event_gold_gain_triggers_run_level_relic_hook():
    run_state = RunState(seed=34, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.obtain_relic("DRAGON_FRUIT")
    starting_max_hp = run_state.player.max_hp

    event = JungleMazeAdventure()
    event.calculate_vars(run_state)
    event.choose(run_state, "join")

    assert run_state.player.max_hp == starting_max_hp + 1


def test_whispering_hollow_hug_uses_run_level_transform_reward_in_run_manager():
    mgr = RunManager(seed=43, character_id="Ironclad")
    mgr.run_state.player.deck = create_ironclad_starter_deck()
    mgr._phase = RunManager.PHASE_EVENT
    event = WhisperingHollow()
    mgr._event_model = event
    mgr._event_options = event.generate_initial_options(mgr.run_state)
    mgr.run_state.player.gold = 200

    result = mgr._do_event_choice({"option_id": "hug"})
    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert isinstance(mgr._current_reward, TransformCardsReward)

    actions = mgr.get_available_actions()
    assert any(action["action"] == "choose" for action in actions)

    final = mgr.take_action({"action": "choose", "index": 0})
    assert final["phase"] == RunManager.PHASE_MAP_CHOICE


def test_luminous_choir_reach_uses_run_level_remove_reward_in_run_manager():
    mgr = RunManager(seed=44, character_id="Ironclad")
    mgr.run_state.player.deck = create_ironclad_starter_deck()
    mgr._phase = RunManager.PHASE_EVENT
    event = LuminousChoir()
    mgr._event_model = event
    mgr._event_options = event.generate_initial_options(mgr.run_state)
    starting_deck = len(mgr.run_state.player.deck)

    result = mgr._do_event_choice({"option_id": "reach"})
    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert isinstance(mgr._current_reward, RemoveCardReward)
    assert any(action["action"] == "choose" for action in mgr.get_available_actions())

    mgr.take_action({"action": "choose", "index": 0})
    mgr.take_action({"action": "choose", "index": 1})
    final = mgr.take_action({"action": "confirm_choice"})
    assert final["phase"] == RunManager.PHASE_MAP_CHOICE
    assert len(mgr.run_state.player.deck) == starting_deck - 1
    assert any(card.card_id == make_spore_mind().card_id for card in mgr.run_state.player.deck)


def test_act2_enchant_events_use_run_level_enchant_rewards_in_run_manager():
    mgr = RunManager(seed=45, character_id="Ironclad")
    mgr.run_state.player.deck = create_ironclad_starter_deck()
    mgr._phase = RunManager.PHASE_EVENT

    field = FieldOfManSizedHoles()
    mgr._event_model = field
    mgr._event_options = field.generate_initial_options(mgr.run_state)
    result = mgr._do_event_choice({"option_id": "enter"})
    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert isinstance(mgr._current_reward, EnchantCardsReward)
    mgr.take_action({"action": "choose", "index": 0})
    final = mgr.take_action({"action": "confirm_choice"})
    assert final["phase"] == RunManager.PHASE_MAP_CHOICE

    mgr._phase = RunManager.PHASE_EVENT
    symbiote = Symbiote()
    mgr._event_model = symbiote
    mgr._event_options = symbiote.generate_initial_options(mgr.run_state)
    result = mgr._do_event_choice({"option_id": "approach"})
    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert isinstance(mgr._current_reward, EnchantCardsReward)


def test_doll_room_and_relic_trader_apply_real_relic_changes():
    run_state = RunState(seed=51, character_id="Ironclad")
    run_state.initialize_run()
    run_state.current_act_index = 1
    starting_relics = len(run_state.player.relics)

    doll = DollRoom()
    result = doll.choose(run_state, "random")
    assert result.finished
    assert len(run_state.player.relics) == starting_relics + 1

    for relic_id in ("ANCHOR", "VAJRA", "PEAR", "JUZU_BRACELET", "LANTERN"):
        run_state.player.obtain_relic(relic_id)
    trader = RelicTrader()
    options = trader.generate_initial_options(run_state)
    result = trader.choose(run_state, options[0].option_id)
    assert result.finished
    assert len(run_state.player.relics) >= starting_relics + 5


def test_endless_conveyor_observe_and_grab_apply_real_state_changes():
    run_state = RunState(seed=52, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()
    run_state.player.gold = 200

    conveyor = EndlessConveyor()
    conveyor.generate_initial_options(run_state)
    observe = conveyor.choose(run_state, "observe")
    assert observe.finished
    assert any(card.upgraded for card in run_state.player.deck)

    before_gold = run_state.player.gold
    grab = conveyor.choose(run_state, "grab")
    assert not grab.finished
    assert run_state.player.gold <= before_gold
