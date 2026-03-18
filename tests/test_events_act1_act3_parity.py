"""Focused parity coverage for selected Act 1 / Act 3 events."""

import sts2_env.events.act1  # noqa: F401
import sts2_env.events.act3  # noqa: F401

from sts2_env.cards.factory import create_card
from sts2_env.events.act1 import BrainLeech, RoomFullOfCheese, TeaMaster
from sts2_env.core.enums import CardId
from sts2_env.events.act3 import (
    DeprecatedAncientEvent,
    DeprecatedEvent,
    Neow,
    WarHistorianRepy,
)
from sts2_env.relics.base import RelicId
from sts2_env.run.reward_objects import PotionReward, RelicReward
from sts2_env.run.run_state import RunState


def test_brain_leech_rip_loses_hp_while_share_knowledge_is_safe():
    run_state = RunState(seed=801, character_id="Ironclad")
    run_state.initialize_run()
    event = BrainLeech()
    start_hp = run_state.player.current_hp

    share = event.choose(run_state, "share_knowledge")
    assert share.finished
    assert run_state.player.current_hp == start_hp

    rip = event.choose(run_state, "rip")
    assert rip.finished
    assert run_state.player.current_hp == start_hp - 5


def test_room_full_of_cheese_search_loses_hp_but_gorge_does_not():
    run_state = RunState(seed=802, character_id="Ironclad")
    run_state.initialize_run()
    event = RoomFullOfCheese()
    start_hp = run_state.player.current_hp

    gorge = event.choose(run_state, "gorge")
    assert gorge.finished
    assert run_state.player.current_hp == start_hp

    search = event.choose(run_state, "search")
    assert search.finished
    assert run_state.player.current_hp == start_hp - 14


def test_tea_master_options_and_gold_costs_match_thresholds():
    run_state = RunState(seed=803, character_id="Ironclad")
    run_state.initialize_run()
    event = TeaMaster()

    run_state.player.gold = 40
    options = event.generate_initial_options(run_state)
    assert [option.option_id for option in options] == ["bone_tea", "ember_tea", "discourtesy"]
    assert [option.enabled for option in options] == [False, False, True]

    run_state.player.gold = 60
    options = event.generate_initial_options(run_state)
    assert [option.option_id for option in options] == ["bone_tea", "ember_tea", "discourtesy"]
    assert [option.enabled for option in options] == [True, False, True]

    run_state.player.gold = 160
    options = event.generate_initial_options(run_state)
    assert [option.option_id for option in options] == ["bone_tea", "ember_tea", "discourtesy"]
    assert [option.enabled for option in options] == [True, True, True]

    before = run_state.player.gold
    relics_before = len(run_state.player.relics)
    bone = event.choose(run_state, "bone_tea")
    assert bone.finished
    assert run_state.player.gold == before - 50
    assert len(run_state.player.relics) == relics_before + 1

    before = run_state.player.gold
    relics_before = len(run_state.player.relics)
    ember = event.choose(run_state, "ember_tea")
    assert ember.finished
    assert run_state.player.gold == before - min(before, 150)
    assert len(run_state.player.relics) == relics_before + 1

    relics_before = len(run_state.player.relics)
    before = run_state.player.gold
    discourtesy = event.choose(run_state, "discourtesy")
    assert discourtesy.finished
    assert run_state.player.gold == before
    assert len(run_state.player.relics) == relics_before + 1


def test_neow_is_not_pool_allowed_but_exposes_three_choices():
    run_state = RunState(seed=804, character_id="Ironclad")
    run_state.initialize_run()
    event = Neow()

    assert event.is_allowed(run_state) is False
    options = event.generate_initial_options(run_state)
    assert [option.option_id for option in options] == ["positive_1", "positive_2", "cursed"]

    relics_before = len(run_state.player.relics)
    cursed = event.choose(run_state, "cursed")
    assert cursed.finished
    assert "cursed relic" in cursed.description.lower()
    assert len(run_state.player.relics) == relics_before + 1

    relics_before = len(run_state.player.relics)
    positive = event.choose(run_state, "positive_1")
    assert positive.finished
    assert "positive relic" in positive.description.lower()
    assert len(run_state.player.relics) == relics_before + 1


def test_neow_positive_pool_respects_cursed_choice_conflicts():
    run_state = RunState(seed=807, character_id="Ironclad")
    run_state.initialize_run()
    event = Neow()

    run_state.rng.up_front.choice = lambda seq: RelicId.LEAFY_POULTICE.name if RelicId.LEAFY_POULTICE.name in seq else seq[0]
    run_state.rng.up_front.shuffle = lambda seq: None
    options = event.generate_initial_options(run_state)

    labels = {option.label for option in options}
    assert any("LEAFY POULTICE" in label.upper() for label in labels)
    assert not any("NEW LEAF" in option.label.upper() for option in options if option.option_id != "cursed")


def test_neow_cursed_pool_adds_scroll_boxes_when_bundles_are_possible_and_skips_silver_crucible_in_multiplayer():
    run_state = RunState(seed=809, character_id="Ironclad")
    run_state.initialize_run()
    event = Neow()

    run_state.rng.up_front.choice = lambda seq: RelicId.SCROLL_BOXES.name if RelicId.SCROLL_BOXES.name in seq else seq[0]
    run_state.rng.up_front.shuffle = lambda seq: None
    options = event.generate_initial_options(run_state)
    assert any("SCROLL BOXES" in option.label.upper() for option in options)

    multiplayer = RunState(seed=810, character_id="Ironclad")
    multiplayer.initialize_run()
    multiplayer.players.append(multiplayer.player)
    multiplayer_event = Neow()
    multiplayer.rng.up_front.choice = lambda seq: RelicId.SILVER_CRUCIBLE.name if RelicId.SILVER_CRUCIBLE.name in seq else seq[0]
    multiplayer.rng.up_front.shuffle = lambda seq: None
    multiplayer_options = multiplayer_event.generate_initial_options(multiplayer)
    assert not any("SILVER CRUCIBLE" in option.label.upper() for option in multiplayer_options)


def test_war_historian_repy_is_disabled_but_choice_results_are_stable():
    run_state = RunState(seed=805, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck.append(create_card(CardId.LANTERN_KEY))
    event = WarHistorianRepy()

    assert event.is_allowed(run_state) is False
    options = event.generate_initial_options(run_state)
    assert [option.option_id for option in options] == ["unlock_cage", "unlock_chest"]

    unlock_cage = event.choose(run_state, "unlock_cage")
    assert unlock_cage.finished
    assert "history course" in unlock_cage.description.lower()
    assert run_state.extra_fields.get("freed_repy") is True
    assert CardId.LANTERN_KEY not in [card.card_id for card in run_state.player.deck]
    assert "HISTORY_COURSE" in run_state.player.relics

    run_state.player.deck.append(create_card(CardId.LANTERN_KEY))
    unlock_chest = event.choose(run_state, "unlock_chest")
    assert unlock_chest.finished
    assert "2 potions" in unlock_chest.description.lower()
    rewards = unlock_chest.rewards["reward_objects"]
    assert len([reward for reward in rewards if isinstance(reward, PotionReward)]) == 2
    assert len([reward for reward in rewards if isinstance(reward, RelicReward)]) == 2


def test_war_historian_repy_removes_lantern_key_on_both_paths():
    run_state = RunState(seed=808, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck.extend([create_card(CardId.LANTERN_KEY), create_card(CardId.LANTERN_KEY)])
    event = WarHistorianRepy()

    event.choose(run_state, "unlock_cage")
    assert CardId.LANTERN_KEY not in [card.card_id for card in run_state.player.deck]


def test_deprecated_act3_events_are_unreachable_and_optionless():
    run_state = RunState(seed=806, character_id="Ironclad")
    run_state.initialize_run()

    for event in (DeprecatedEvent(), DeprecatedAncientEvent()):
        assert event.is_allowed(run_state) is False
        assert event.generate_initial_options(run_state) == []
        result = event.choose(run_state, "anything")
        assert result.finished
        assert result.next_options == []
