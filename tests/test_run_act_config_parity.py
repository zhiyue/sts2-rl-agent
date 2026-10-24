"""Run act configuration parity tests."""

from sts2_env.events.act2 import LuminousChoir, RanwidTheElder
from sts2_env.events.shared import TheLanternKey
from sts2_env.map.acts import ALL_ACTS
from sts2_env.potions.base import create_potion
from sts2_env.relics.base import RelicId
from sts2_env.run.events import pick_event
from sts2_env.run.run_state import RunState

RUN_SEED = 42
ACT_TWO_INDEX = 1
ENTRY_BLOCKED_GOLD = LuminousChoir.ENTRY_GOLD_COST - 1
FIRE_POTION_ID = "FirePotion"


def test_initialize_run_generates_shuffled_act_event_rooms_like_csharp_runmanager():
    run_state = RunState(seed=RUN_SEED)
    static_event_ids = [event_id for act in ALL_ACTS for event_id in act.event_ids]

    run_state.initialize_run()

    generated_event_ids = [event_id for act in run_state.acts for event_id in act.event_ids]
    assert sorted(generated_event_ids) == sorted(static_event_ids)
    assert generated_event_ids != static_event_ids
    # The 3 event shuffles each consume len(pool)-1 counter ticks on up_front;
    # encounter-room generation (C# ActModel.GenerateRooms also runs on
    # UpFront) consumes additional rolls after that.
    assert run_state.rng.up_front.counter >= len(static_event_ids) - len(run_state.acts)


def test_initialize_run_does_not_regenerate_event_rooms_after_first_initialization():
    run_state = RunState(seed=RUN_SEED)
    run_state.initialize_run()
    event_ids_by_act = [list(act.event_ids) for act in run_state.acts]
    up_front_counter = run_state.rng.up_front.counter

    run_state.initialize_run()

    assert [act.event_ids for act in run_state.acts] == event_ids_by_act
    assert run_state.rng.up_front.counter == up_front_counter


def test_run_state_uses_mutable_act_copies_like_csharp_runstate():
    first_run = RunState(seed=RUN_SEED)
    second_run = RunState(seed=RUN_SEED)

    first_run.acts[ACT_TWO_INDEX].event_ids = [RanwidTheElder.event_id]

    assert second_run.acts[ACT_TWO_INDEX].event_ids != [RanwidTheElder.event_id]
    # C# Hive.AllEvents includes TheLanternKey (LuminousChoir is Overgrowth).
    assert TheLanternKey.event_id in second_run.acts[ACT_TWO_INDEX].event_ids


def test_pick_event_advances_through_current_act_event_order_like_csharp_roomset():
    run_state = RunState(seed=RUN_SEED)
    run_state.current_act_index = ACT_TWO_INDEX
    run_state.current_act.event_ids = [
        LuminousChoir.event_id,
        RanwidTheElder.event_id,
    ]
    run_state.player.gold = ENTRY_BLOCKED_GOLD
    run_state.player.add_potion(create_potion(FIRE_POTION_ID))
    run_state.player.obtain_relic(RelicId.ANCHOR.name)

    event = pick_event(run_state)

    assert isinstance(event, RanwidTheElder)
    assert run_state.current_act.events_visited == 2
    assert run_state.rng.up_front.counter == 0


def test_pick_event_with_explicit_pool_does_not_mutate_act_event_cursor():
    run_state = RunState(seed=RUN_SEED)
    run_state.current_act_index = ACT_TWO_INDEX
    run_state.current_act.events_visited = 1
    run_state.player.gold = RanwidTheElder.ENTRY_GOLD_COST
    run_state.player.add_potion(create_potion(FIRE_POTION_ID))
    run_state.player.obtain_relic(RelicId.ANCHOR.name)

    event = pick_event(run_state, pool=[RanwidTheElder.event_id])

    assert isinstance(event, RanwidTheElder)
    assert run_state.current_act.events_visited == 1


def test_pick_event_repeats_current_act_event_after_unique_events_are_exhausted_like_csharp_roomset():
    run_state = RunState(seed=RUN_SEED)
    run_state.current_act_index = ACT_TWO_INDEX
    run_state.current_act.event_ids = [RanwidTheElder.event_id]
    run_state.current_act.events_visited = 1
    run_state.visited_event_ids.add(RanwidTheElder.event_id)
    run_state.player.gold = RanwidTheElder.ENTRY_GOLD_COST
    run_state.player.add_potion(create_potion(FIRE_POTION_ID))
    run_state.player.obtain_relic(RelicId.ANCHOR.name)

    event = pick_event(run_state)

    assert isinstance(event, RanwidTheElder)
    assert run_state.current_act.events_visited == 3
