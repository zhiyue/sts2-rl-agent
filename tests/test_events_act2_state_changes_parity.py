"""Focused tests for Act 2 event state changes."""

import sts2_env.events.act2  # noqa: F401

from sts2_env.cards.factory import create_card
from sts2_env.cards.factory import create_card
from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.cards.status import make_spore_mind
from sts2_env.core.enums import CardId
from sts2_env.events.act2 import (
    CrystalSphere,
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
from sts2_env.run.reward_objects import EnchantCardsReward, PotionReward, RemoveCardReward, TransformCardsReward
from sts2_env.potions.base import create_potion
from sts2_env.run.run_manager import RunManager
from sts2_env.run.run_state import PlayerState, RunState


class _ExclusiveHighRng:
    def next_int(self, low: int, high: int) -> int:
        raise AssertionError(f"expected exclusive RNG call, got inclusive {low}, {high}")

    def next_int_exclusive(self, low: int, high: int) -> int:
        return high - 1

    def next_float_range(self, low: float, high: float) -> float:
        return high - 0.5


class _LastChoiceRng:
    def choice(self, seq):
        return seq[-1]


class _FirstChoiceCountingRng:
    def __init__(self) -> None:
        self.choice_calls = 0

    def choice(self, seq):
        self.choice_calls += 1
        return seq[0]


class _DeckSizeOnAddModifier:
    def __init__(self) -> None:
        self.deck_sizes: list[int] = []

    def after_card_added_to_deck(self, player, card, source=None) -> None:
        self.deck_sizes.append(len(player.deck))


class _NoopShuffleRng(_LastChoiceRng):
    def shuffle(self, seq) -> None:
        pass


def test_act2_event_random_values_use_exclusive_upper_bounds():
    run_state = RunState(seed=28, character_id="Ironclad")
    run_state.initialize_run()
    run_state.rng.up_front = _ExclusiveHighRng()

    sphere = CrystalSphere()
    sphere.rng = _ExclusiveHighRng()
    sphere.calculate_vars(run_state)
    assert sphere._cost == 99

    maze = JungleMazeAdventure()
    maze.rng = _ExclusiveHighRng()
    maze.calculate_vars(run_state)
    assert maze._solo_gold == 164.5
    assert maze._join_gold == 64.5

    choir = LuminousChoir()
    choir.rng = _ExclusiveHighRng()
    choir.calculate_vars(run_state)
    assert choir._cost == 100


def test_luminous_choir_and_morphic_grove_apply_real_deck_changes():
    run_state = RunState(seed=29, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()
    run_state.player.gold = 200
    starting_deck = len(run_state.player.deck)

    choir = LuminousChoir()
    choir.ensure_vars_calculated(run_state)
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


def test_morphic_grove_requires_all_players_to_have_gold():
    run_state = RunState(seed=2901, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.gold = 100
    ally = run_state.add_player(PlayerState(player_id=2, character_id="Silent", gold=99))
    event = MorphicGrove()

    assert event.is_allowed(run_state) is False

    ally.gold = 100
    assert event.is_allowed(run_state) is True


def test_potion_courier_ranwid_and_whispering_hollow_change_inventory():
    run_state = RunState(seed=31, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()
    run_state.current_act_index = 1
    run_state.player.gold = 200
    run_state.player.add_potion(create_potion("FirePotion"))
    run_state.player.obtain_relic("BURNING_BLOOD")
    run_state.player.obtain_relic("ANCHOR")

    courier = PotionCourier()
    grab = courier.choose(run_state, "grab")
    assert grab.finished
    assert [reward.reward_type.name for reward in grab.rewards["reward_objects"]] == ["POTION", "POTION", "POTION"]

    ranwid = RanwidTheElder()
    ranwid.rng = _LastChoiceRng()
    ranwid_options = ranwid.generate_initial_options(run_state)
    assert [option.enabled for option in ranwid_options] == [True, True, True]
    chosen_potion_slot = ranwid._potion_slot
    starting_relics = len(run_state.player.relics)
    ranwid.choose(run_state, "gold")
    assert len(run_state.player.relics) == starting_relics + 1
    assert any(potion.slot_index == chosen_potion_slot for potion in run_state.player.held_potions())

    hollow = WhisperingHollow()
    starting_hp = run_state.player.current_hp
    before_ids = [card.card_id for card in run_state.player.deck]
    gold = hollow.choose(run_state, "gold")
    assert gold.finished
    assert [reward.reward_type.name for reward in gold.rewards["reward_objects"]] == ["POTION", "POTION"]
    result = hollow.choose(run_state, "hug")
    assert not result.finished
    assert run_state.player.current_hp == starting_hp
    hollow.resolve_pending_choice(0)
    after_ids = [card.card_id for card in run_state.player.deck]
    assert run_state.player.current_hp == starting_hp - 9
    assert before_ids != after_ids


def test_ranwid_requires_all_players_to_have_gold_potion_and_tradable_relic():
    run_state = RunState(seed=3106, character_id="Ironclad")
    run_state.initialize_run()
    run_state.current_act_index = 1
    run_state.player.gold = 200
    run_state.player.add_potion(create_potion("FirePotion"))
    run_state.player.obtain_relic("ANCHOR")
    ally = run_state.add_player(PlayerState(player_id=2, character_id="Silent", gold=99))
    ally.add_potion(create_potion("FlexPotion"))
    ally.obtain_relic("VAJRA")
    event = RanwidTheElder()

    assert event.is_allowed(run_state) is False

    ally.gold = 100
    assert event.is_allowed(run_state) is True

    ally.potions.clear()
    assert event.is_allowed(run_state) is False

    ally.add_potion(create_potion("FlexPotion"))
    ally.relics = ["RING_OF_THE_SNAKE"]
    ally.relic_objects = []
    assert event.is_allowed(run_state) is False

    ally.obtain_relic("VAJRA")
    assert event.is_allowed(run_state) is True


def test_whispering_hollow_requires_all_players_to_have_gold():
    run_state = RunState(seed=3101, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.gold = WhisperingHollow.GOLD_COST
    ally = run_state.add_player(PlayerState(player_id=2, character_id="Silent", gold=WhisperingHollow.GOLD_COST - 1))
    event = WhisperingHollow()

    assert event.is_allowed(run_state) is False

    ally.gold = WhisperingHollow.GOLD_COST
    assert event.is_allowed(run_state) is True


def test_whispering_hollow_options_and_effects_match_reference_vars():
    run_state = RunState(seed=3102, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()
    run_state.player.gold = 200
    event = WhisperingHollow()

    options = event.generate_initial_options(run_state)

    assert [option.option_id for option in options] == [
        WhisperingHollow.OPTION_GOLD,
        WhisperingHollow.OPTION_HUG,
    ]
    assert options[0].label == f"Pay Gold ({WhisperingHollow.GOLD_COST}g)"
    assert options[0].description == f"Gain {WhisperingHollow.GOLD_POTION_REWARD_COUNT} potions"
    assert options[1].description == f"Take {WhisperingHollow.HUG_DAMAGE} damage, transform 1 card"

    gold_before = run_state.player.gold
    gold_result = event.choose(run_state, WhisperingHollow.OPTION_GOLD)
    potion_rewards = gold_result.rewards["reward_objects"]

    assert gold_result.finished
    assert run_state.player.gold == gold_before - WhisperingHollow.GOLD_COST
    assert len(potion_rewards) == WhisperingHollow.GOLD_POTION_REWARD_COUNT
    assert all(isinstance(reward, PotionReward) for reward in potion_rewards)

    deferred_state = RunState(seed=3103, character_id="Ironclad")
    deferred_state.initialize_run()
    deferred_state.player.deck = create_ironclad_starter_deck()
    deferred_state.defer_followup_rewards = True
    hp_before = deferred_state.player.current_hp
    deferred_event = WhisperingHollow()

    hug_result = deferred_event.choose(deferred_state, WhisperingHollow.OPTION_HUG)
    transform_reward = hug_result.rewards["reward_objects"][0]

    assert hug_result.finished
    assert deferred_state.player.current_hp == hp_before
    assert isinstance(transform_reward, TransformCardsReward)
    assert transform_reward.count == 1


def test_potion_courier_ransack_uses_event_specific_uncommon_pool_order():
    run_state = RunState(seed=311, character_id="Ironclad")
    run_state.initialize_run()
    run_state.current_act_index = 1
    run_state.rng.rewards = _FirstChoiceCountingRng()
    courier = PotionCourier()

    result = courier.choose(run_state, "ransack")
    reward = result.rewards["reward_objects"][0]

    assert reward.potion_id == "Ashwater"
    assert run_state.rng.rewards.choice_calls == 1

    reward.populate(run_state, None)

    assert reward.potion_id == "Ashwater"
    assert run_state.rng.rewards.choice_calls == 1


def test_event_lifecycle_toggles_can_remove_potions_for_ranwid():
    mgr = RunManager(seed=3103, character_id="Ironclad")
    mgr.run_state.current_act_index = 1
    mgr.run_state.player.gold = 200
    mgr.run_state.player.add_potion(create_potion("FirePotion"))
    mgr.run_state.player.obtain_relic("ANCHOR")
    mgr.run_state.current_act.event_ids = ["RanwidTheElder"]

    mgr._enter_event()
    assert mgr.run_state.player.can_remove_potions is False

    result = mgr._do_event_choice({"option_id": "leave"})
    assert result["phase"] == RunManager.PHASE_MAP_CHOICE
    assert mgr.run_state.player.can_remove_potions is True


def test_ranwid_uses_the_randomly_selected_potion_and_relic_targets():
    potion_state = RunState(seed=3101, character_id="Ironclad")
    potion_state.initialize_run()
    potion_state.current_act_index = 1
    potion_state.player.gold = 200
    potion_state.player.add_potion(create_potion("FirePotion"))
    potion_state.player.add_potion(create_potion("FlexPotion"))
    potion_state.player.obtain_relic("ANCHOR")
    potion_state.player.obtain_relic("VAJRA")
    potion_event = RanwidTheElder()
    potion_event.rng = _LastChoiceRng()

    potion_event.generate_initial_options(potion_state)
    assert potion_event._potion_slot == 1
    potion_event.choose(potion_state, "potion")
    assert all(potion.potion_id != "FlexPotion" for potion in potion_state.player.held_potions())

    relic_state = RunState(seed=3102, character_id="Ironclad")
    relic_state.initialize_run()
    relic_state.current_act_index = 1
    relic_state.player.gold = 200
    relic_state.player.add_potion(create_potion("FirePotion"))
    relic_state.player.obtain_relic("ANCHOR")
    relic_state.player.obtain_relic("VAJRA")
    relic_event = RanwidTheElder()
    relic_event.rng = _LastChoiceRng()

    relic_event.generate_initial_options(relic_state)
    assert relic_event._relic_id == "VAJRA"
    relic_event.choose(relic_state, "relic")
    assert "VAJRA" not in relic_state.player.relics


def test_ranwid_uses_reference_tradable_relic_rules():
    blocked = RunState(seed=3107, character_id="Ironclad")
    blocked.initialize_run()
    blocked.current_act_index = 1
    blocked.player.gold = 200
    blocked.player.add_potion(create_potion("FirePotion"))
    blocked.player.obtain_relic("PEAR")
    blocked_event = RanwidTheElder()

    assert blocked_event.is_allowed(blocked) is False

    run_state = RunState(seed=3108, character_id="Ironclad")
    run_state.initialize_run()
    run_state.current_act_index = 1
    run_state.player.gold = 200
    run_state.player.add_potion(create_potion("FirePotion"))
    for relic_id in ("PEAR", "LEES_WAFFLE", "ANCHOR", "LIZARD_TAIL"):
        run_state.player.obtain_relic(relic_id)
    lizard_tail = next(relic for relic in run_state.player.relic_objects if relic.relic_id.name == "LIZARD_TAIL")
    lizard_tail._was_used = True  # noqa: SLF001
    event = RanwidTheElder()
    event.rng = _LastChoiceRng()

    options = event.generate_initial_options(run_state)
    result = event.choose(run_state, "relic")

    assert [option.enabled for option in options] == [True, True, True]
    assert event._relic_id == "ANCHOR"
    assert result.finished
    assert "ANCHOR" not in run_state.player.relics
    assert "PEAR" in run_state.player.relics
    assert "LEES_WAFFLE" in run_state.player.relics
    assert "LIZARD_TAIL" in run_state.player.relics


def test_ranwid_uses_event_rng_without_advancing_up_front_rng():
    run_state = RunState(seed=3104, character_id="Ironclad")
    run_state.initialize_run()
    run_state.current_act_index = 1
    run_state.player.gold = 200
    run_state.player.add_potion(create_potion("FirePotion"))
    run_state.player.obtain_relic("ANCHOR")
    run_state.player.obtain_relic("VAJRA")
    event = RanwidTheElder()
    up_front_counter = run_state.rng.up_front.counter

    options = event.generate_initial_options(run_state)

    assert [option.enabled for option in options] == [True, True, True]
    assert run_state.rng.up_front.counter == up_front_counter


def test_ranwid_allows_empty_reward_pool_and_uses_circlet_fallback():
    run_state = RunState(seed=3105, character_id="Ironclad")
    run_state.initialize_run()
    run_state.current_act_index = 1
    run_state.player.gold = 200
    run_state.player.add_potion(create_potion("FirePotion"))
    run_state.player.obtain_relic("ANCHOR")
    run_state.player.relic_grab_bag_by_rarity = {
        rarity: []
        for rarity in run_state.player.relic_grab_bag_by_rarity
    }
    run_state.player.relic_grab_bag = []
    run_state.player.relic_grab_bag_fallback = []
    event = RanwidTheElder()

    assert event.is_allowed(run_state) is True
    result = event.choose(run_state, "gold")

    assert result.finished
    assert run_state.player.gold == 100
    assert run_state.player.relics[-1] == "CIRCLET"


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

    exhausted = RunState(seed=2902, character_id="Ironclad")
    exhausted.initialize_run()
    exhausted.player.deck = create_ironclad_starter_deck()
    exhausted.player.gold = 200
    exhausted.player.relic_grab_bag_by_rarity = {
        rarity: []
        for rarity in exhausted.player.relic_grab_bag_by_rarity
    }
    exhausted.player.relic_grab_bag = []
    exhausted.player.relic_grab_bag_fallback = []
    exhausted_choir = LuminousChoir()
    assert exhausted_choir.is_allowed(exhausted) is False


def test_luminous_choir_requires_all_players_to_have_entry_gold():
    run_state = RunState(seed=2903, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()
    run_state.player.gold = 149
    ally = run_state.add_player(PlayerState(player_id=2, character_id="Silent", gold=148))
    choir = LuminousChoir()

    assert choir.is_allowed(run_state) is False

    ally.gold = 149
    assert choir.is_allowed(run_state) is True


def test_event_gold_gain_triggers_run_level_relic_hook():
    run_state = RunState(seed=34, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.obtain_relic("DRAGON_FRUIT")
    starting_max_hp = run_state.player.max_hp

    event = JungleMazeAdventure()
    event.calculate_vars(run_state)
    event.choose(run_state, "join")

    assert run_state.player.max_hp == starting_max_hp + 1


def test_jungle_maze_adventure_truncates_gold_gain_after_decimal_relic_hooks():
    run_state = RunState(seed=35, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.gold = 0
    run_state.player.obtain_relic("BOWLER_HAT")

    event = JungleMazeAdventure()
    event._join_gold = 64.5
    result = event.choose(run_state, "join")

    assert result.finished
    assert run_state.player.gold == 80


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


def test_whispering_hollow_hug_transforms_before_damage_can_end_run():
    mgr = RunManager(seed=4302, character_id="Ironclad")
    target = create_card(CardId.STRIKE_IRONCLAD)
    other = create_card(CardId.DEFEND_IRONCLAD)
    mgr.run_state.player.deck = [target, other]
    mgr.run_state.player.current_hp = WhisperingHollow.HUG_DAMAGE
    mgr.run_state.player.gold = WhisperingHollow.GOLD_COST
    mgr._phase = RunManager.PHASE_EVENT
    event = WhisperingHollow()
    mgr._event_model = event
    mgr._event_options = event.generate_initial_options(mgr.run_state)
    before_card_id = target.card_id

    result = mgr._do_event_choice({"option_id": WhisperingHollow.OPTION_HUG})

    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert isinstance(mgr._current_reward, TransformCardsReward)
    assert mgr.run_state.player.current_hp == WhisperingHollow.HUG_DAMAGE

    final = mgr.take_action({"action": "choose", "index": 0})

    assert final["phase"] == RunManager.PHASE_RUN_OVER
    assert mgr.run_state.player.current_hp == 0
    assert target.card_id is not before_card_id


def test_transform_events_only_offer_transformable_cards():
    run_state = RunState(seed=4301, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck = [create_card(CardId.SPOILS_MAP), create_card(CardId.SPOILS_MAP)]
    run_state.player.gold = 200
    run_state.current_act_index = 1

    grove = MorphicGrove()
    group = grove.choose(run_state, "group")
    assert group.finished is True
    assert group.description.startswith("Choose")
    assert grove.pending_choice is None

    hollow = WhisperingHollow()
    hug = hollow.choose(run_state, "hug")
    assert hug.finished is True
    assert hug.description.startswith("Choose")
    assert hollow.pending_choice is None


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


def test_luminous_choir_reach_adds_spore_mind_after_selected_cards_are_removed():
    mgr = RunManager(seed=4411, character_id="Ironclad")
    modifier = _DeckSizeOnAddModifier()
    mgr.run_state.modifiers = [modifier]
    mgr.run_state.player.deck = create_ironclad_starter_deck()
    mgr._phase = RunManager.PHASE_EVENT
    event = LuminousChoir()
    mgr._event_model = event
    mgr._event_options = event.generate_initial_options(mgr.run_state)
    starting_deck = len(mgr.run_state.player.deck)

    result = mgr._do_event_choice({"option_id": "reach"})
    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert mgr.run_state.pending_choice is not None

    mgr.take_action({"action": "choose", "index": 0})
    mgr.take_action({"action": "choose", "index": 1})
    final = mgr.take_action({"action": "confirm_choice"})

    assert final["phase"] == RunManager.PHASE_MAP_CHOICE
    assert modifier.deck_sizes[-1] == starting_deck - 1
    assert any(card.card_id == CardId.SPORE_MIND for card in mgr.run_state.player.deck)


def test_field_of_man_sized_holes_resist_adds_normality_after_selected_cards_are_removed():
    mgr = RunManager(seed=4412, character_id="Ironclad")
    modifier = _DeckSizeOnAddModifier()
    mgr.run_state.modifiers = [modifier]
    mgr.run_state.player.deck = create_ironclad_starter_deck()
    mgr._phase = RunManager.PHASE_EVENT
    event = FieldOfManSizedHoles()
    mgr._event_model = event
    mgr._event_options = event.generate_initial_options(mgr.run_state)
    starting_deck = len(mgr.run_state.player.deck)

    result = mgr._do_event_choice({"option_id": "resist"})
    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert mgr.run_state.pending_choice is not None

    mgr.take_action({"action": "choose", "index": 0})
    mgr.take_action({"action": "choose", "index": 1})
    final = mgr.take_action({"action": "confirm_choice"})

    assert final["phase"] == RunManager.PHASE_MAP_CHOICE
    assert modifier.deck_sizes[-1] == starting_deck - 1
    assert any(card.card_id == CardId.NORMALITY for card in mgr.run_state.player.deck)


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

    for relic_id in ("ANCHOR", "VAJRA", "BONE_FLUTE", "JUZU_BRACELET", "LANTERN"):
        run_state.player.obtain_relic(relic_id)
    trader = RelicTrader()
    trader.rng = _NoopShuffleRng()
    options = trader.generate_initial_options(run_state)
    result = trader.choose(run_state, options[0].option_id)
    assert result.finished
    assert len(run_state.player.relics) >= starting_relics + 5

    blocked = RunState(seed=5101, character_id="Ironclad")
    blocked.initialize_run()
    blocked.current_act_index = 1
    blocked.player.obtain_relic("BURNING_BLOOD")
    blocked.player.obtain_relic("BLACK_BLOOD")
    blocked.player.obtain_relic("RING_OF_THE_SNAKE")
    blocked.player.obtain_relic("RING_OF_THE_DRAKE")
    blocked.player.obtain_relic("CRACKED_CORE")
    blocked_trader = RelicTrader()
    assert blocked_trader.is_allowed(blocked) is False


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
