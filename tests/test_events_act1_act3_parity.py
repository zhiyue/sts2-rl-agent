"""Focused parity coverage for selected Act 1 / Act 3 events."""

import sts2_env.events.act1  # noqa: F401
import sts2_env.events.act3  # noqa: F401

from sts2_env.events.act1 import BrainLeech, RoomFullOfCheese, TeaMaster
from sts2_env.events.act3 import (
    DeprecatedAncientEvent,
    DeprecatedEvent,
    Neow,
    WarHistorianRepy,
)
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
    option_ids = [option.option_id for option in options]
    assert option_ids == ["discourtesy"]

    run_state.player.gold = 60
    options = event.generate_initial_options(run_state)
    option_ids = [option.option_id for option in options]
    assert option_ids == ["bone_tea", "discourtesy"]

    run_state.player.gold = 160
    options = event.generate_initial_options(run_state)
    option_ids = [option.option_id for option in options]
    assert option_ids == ["bone_tea", "ember_tea", "discourtesy"]

    before = run_state.player.gold
    bone = event.choose(run_state, "bone_tea")
    assert bone.finished
    assert run_state.player.gold == before - 50

    before = run_state.player.gold
    ember = event.choose(run_state, "ember_tea")
    assert ember.finished
    assert run_state.player.gold == before - min(before, 150)

    before = run_state.player.gold
    discourtesy = event.choose(run_state, "discourtesy")
    assert discourtesy.finished
    assert run_state.player.gold == before


def test_neow_is_not_pool_allowed_but_exposes_three_choices():
    run_state = RunState(seed=804, character_id="Ironclad")
    run_state.initialize_run()
    event = Neow()

    assert event.is_allowed(run_state) is False
    options = event.generate_initial_options(run_state)
    assert [option.option_id for option in options] == ["positive_1", "positive_2", "cursed"]

    cursed = event.choose(run_state, "cursed")
    assert cursed.finished
    assert "cursed relic" in cursed.description.lower()

    positive = event.choose(run_state, "positive_1")
    assert positive.finished
    assert "positive relic" in positive.description.lower()


def test_war_historian_repy_is_disabled_but_choice_results_are_stable():
    run_state = RunState(seed=805, character_id="Ironclad")
    run_state.initialize_run()
    event = WarHistorianRepy()

    assert event.is_allowed(run_state) is False
    options = event.generate_initial_options(run_state)
    assert [option.option_id for option in options] == ["unlock_cage", "unlock_chest"]

    unlock_cage = event.choose(run_state, "unlock_cage")
    assert unlock_cage.finished
    assert "history course" in unlock_cage.description.lower()

    unlock_chest = event.choose(run_state, "unlock_chest")
    assert unlock_chest.finished
    assert "2 potions" in unlock_chest.description.lower()


def test_deprecated_act3_events_are_unreachable_and_optionless():
    run_state = RunState(seed=806, character_id="Ironclad")
    run_state.initialize_run()

    for event in (DeprecatedEvent(), DeprecatedAncientEvent()):
        assert event.is_allowed(run_state) is False
        assert event.generate_initial_options(run_state) == []
        result = event.choose(run_state, "anything")
        assert result.finished
        assert result.next_options == []
