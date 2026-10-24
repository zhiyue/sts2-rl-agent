"""Tests for the local full-run web UI state surface."""

import sts2_env.events  # noqa: F401

from sts2_env.core.enums import RoomType
from sts2_env.potions.base import create_potion
from sts2_env.run.events import get_event
from sts2_env.run.run_manager import RunManager
from sts2_env.web.play_run import RunSession, serialize_run


FULL_RUN_FLOW_SEED = 123
ROOM_SCREEN_TEST_SEED = 321
PENDING_CHOICE_TEST_SEED = 75
COMBAT_TEST_SEED = 456
LEGENDS_WERE_TRUE_EVENT_ID = "TheLegendsWereTrue"
LEGENDS_WERE_TRUE_TITLE = "The Legends Were True"
LEGENDS_NAB_MAP_LABEL = "Nab the Map"
LEGENDS_FIND_EXIT_LABEL = "Slowly Find an Exit"
BOSS_RELIC_WITHOUT_FOLLOWUP_CHOICE = "SOZU"
REST_TEST_STARTING_HP = 40
REST_TEST_EXPECTED_HP = "64/80"
LEGENDS_EXIT_EXPECTED_HP = "72/80"
MAX_COMBAT_ACTIONS_TO_REACH_REWARD = 200
MAX_REWARD_SCREENS_TO_SKIP = 10


def _first_damage_or_end_turn_action_index(state: dict) -> int:
    play_action = next(
        (
            action
            for action in state["actions"]
            if action["kind"] == "play_card"
            and ("Strike" in action["label"] or "Bash" in action["label"])
        ),
        None,
    )
    if play_action is None:
        play_action = next(
            (action for action in state["actions"] if action["kind"] == "play_card"),
            None,
        )
    if play_action is not None:
        return play_action["index"]
    return next(action["index"] for action in state["actions"] if action["kind"] == "end_turn")


def _skip_current_reward_screen(state: dict, session: RunSession) -> dict:
    item = next(
        (
            item
            for item in state["screen"]["items"]
            if item["name"].startswith("Skip")
        ),
        state["screen"]["items"][0],
    )
    return session.take_action(item["action_index"])


def _skip_reward_screens_until_map(state: dict, session: RunSession) -> dict:
    reward_steps = 0
    while state["phase"] == RunManager.PHASE_CARD_REWARD and reward_steps < MAX_REWARD_SCREENS_TO_SKIP:
        state = _skip_current_reward_screen(state, session)
        reward_steps += 1
    return state


def _take_first_map_node(state: dict, session: RunSession) -> dict:
    move_action = next(action for action in state["actions"] if action["kind"] == "move")
    return session.take_action(move_action["index"])


def _enter_legends_were_true_event(session: RunSession) -> dict:
    state = session.start(character="Ironclad", seed=ROOM_SCREEN_TEST_SEED)
    assert session.mgr is not None
    event = get_event(LEGENDS_WERE_TRUE_EVENT_ID)
    assert event is not None
    event.reset_rng_for_run(session.mgr.run_state)
    event.ensure_vars_calculated(session.mgr.run_state)
    session.mgr._phase = RunManager.PHASE_EVENT
    session.mgr._event_model = event
    session.mgr._event_started = True
    session.mgr._event_options = event.generate_initial_options(session.mgr.run_state)
    state = session.state()
    assert state["screen"]["title"] == LEGENDS_WERE_TRUE_TITLE
    return state


def _reach_first_combat_reward(session: RunSession) -> dict:
    state = session.start(character="Ironclad", seed=FULL_RUN_FLOW_SEED)
    assert state["screen"]["title"] == "Neow"
    state = session.take_action(0)
    assert state["screen"]["title"] == "Relic Reward"
    state = session.take_action(0)
    assert state["screen"]["title"] == "Map"
    state = _take_first_map_node(state, session)
    assert state["screen"]["title"] == "Combat"

    steps = 0
    while state["phase"] == RunManager.PHASE_COMBAT and steps < MAX_COMBAT_ACTIONS_TO_REACH_REWARD:
        state = session.take_action(_first_damage_or_end_turn_action_index(state))
        steps += 1

    assert state["phase"] == RunManager.PHASE_CARD_REWARD
    assert state["screen"]["title"] == "Card Reward"
    return state


def test_web_session_waits_for_new_run_before_neow() -> None:
    session = RunSession()

    state = session.state()

    assert state["phase"] == "START"
    assert state["screen"]["type"] == "start"
    assert state["screen"]["title"] == "Start Run"
    assert state["actions"] == []
    assert state["last_description"] == "Ready to start."


def test_web_session_starts_at_neow_and_advances_to_map() -> None:
    session = RunSession()

    state = session.start(character="Ironclad", seed=FULL_RUN_FLOW_SEED)
    assert state["phase"] == RunManager.PHASE_EVENT
    assert state["screen"]["type"] == "event"
    assert state["screen"]["title"] == "Neow"
    assert state["actions"][0]["label"] == "Booming Conch - Gain a positive relic"

    reward = session.take_action(0)
    assert reward["phase"] == RunManager.PHASE_CARD_REWARD
    assert reward["screen"]["type"] == "reward"
    assert reward["screen"]["items"] == [
        {"name": "Booming Conch", "action_index": 0},
    ]

    map_state = session.take_action(0)
    assert map_state["phase"] == RunManager.PHASE_MAP_CHOICE
    assert map_state["screen"]["type"] == "map"
    assert map_state["screen"]["columns"]
    assert map_state["screen"]["paths"]
    reachable_nodes = [
        node
        for row in map_state["screen"]["rows"]
        for node in row
        if node["reachable"]
    ]
    assert reachable_nodes
    assert all(node["action_index"] is not None for node in reachable_nodes)


def test_web_treasure_item_links_to_collect_action() -> None:
    mgr = RunManager(seed=ROOM_SCREEN_TEST_SEED, character_id="Ironclad")
    mgr._enter_treasure()
    actions = mgr.get_available_actions()

    state = serialize_run(
        mgr,
        actions,
        seed=ROOM_SCREEN_TEST_SEED,
        character="Ironclad",
        ascension=0,
        last_description="",
    )

    assert state["screen"]["type"] == "treasure"
    assert state["screen"]["items"][0]["action_index"] == 0


def test_web_session_can_collect_treasure_and_return_to_map() -> None:
    session = RunSession()
    session.start(character="Ironclad", seed=ROOM_SCREEN_TEST_SEED)
    assert session.mgr is not None
    session.mgr._enter_treasure()

    state = session.state()
    collect_action = state["screen"]["items"][0]["action_index"]
    state = session.take_action(collect_action)

    assert state["phase"] == RunManager.PHASE_MAP_CHOICE
    assert state["screen"]["title"] == "Map"


def test_web_state_serializes_combat_for_browser_display() -> None:
    mgr = RunManager(seed=COMBAT_TEST_SEED, character_id="Ironclad")
    mgr._enter_combat(RoomType.MONSTER)
    actions = mgr.get_available_actions()

    state = serialize_run(
        mgr,
        actions,
        seed=COMBAT_TEST_SEED,
        character="Ironclad",
        ascension=0,
        last_description="",
    )

    assert state["screen"]["type"] == "combat"
    assert state["screen"]["enemies"]
    assert "intent" in state["screen"]["enemies"][0]
    assert state["screen"]["end_turn_action_index"] == 0
    assert any(card["actions"] for card in state["screen"]["hand"])
    assert any(action["kind"] == "play_card" for action in state["actions"])


def test_web_state_serializes_combat_potion_actions() -> None:
    mgr = RunManager(seed=COMBAT_TEST_SEED, character_id="Ironclad")
    assert mgr.run_state.player.add_potion(create_potion("FirePotion"))
    mgr._enter_combat(RoomType.MONSTER)
    actions = mgr.get_available_actions()

    state = serialize_run(
        mgr,
        actions,
        seed=COMBAT_TEST_SEED,
        character="Ironclad",
        ascension=0,
        last_description="",
    )

    assert state["screen"]["potions"]
    assert state["screen"]["potions"][0]["actions"]
    assert any(action["kind"] == "use_potion" for action in state["actions"])


def test_web_state_serializes_pending_card_choice() -> None:
    mgr = RunManager(seed=PENDING_CHOICE_TEST_SEED, character_id="Ironclad")
    mgr._enter_rest_site()
    result = mgr._do_rest_site({"option_id": "SMITH"})
    actions = mgr.get_available_actions()

    state = serialize_run(
        mgr,
        actions,
        seed=PENDING_CHOICE_TEST_SEED,
        character="Ironclad",
        ascension=0,
        last_description=result["description"],
    )

    assert state["screen"]["type"] == "choice"
    assert state["screen"]["items"]
    assert all(item["action_index"] is not None for item in state["screen"]["items"])


def test_web_session_can_rest_and_return_to_map() -> None:
    session = RunSession()
    session.start(character="Ironclad", seed=ROOM_SCREEN_TEST_SEED)
    assert session.mgr is not None
    session.mgr.run_state.player.current_hp = REST_TEST_STARTING_HP
    session.mgr._enter_rest_site()

    state = session.state()
    rest_action = next(item["action_index"] for item in state["screen"]["items"] if item["name"] == "Rest")
    state = session.take_action(rest_action)

    assert state["phase"] == RunManager.PHASE_MAP_CHOICE
    assert state["screen"]["title"] == "Map"
    assert state["hp"] == REST_TEST_EXPECTED_HP


def test_web_session_can_leave_shop_from_shop_screen() -> None:
    session = RunSession()
    session.start(character="Ironclad", seed=ROOM_SCREEN_TEST_SEED)
    assert session.mgr is not None
    session.mgr._enter_shop()

    state = session.state()
    leave_action = next(
        item["action_index"]
        for section in state["screen"]["sections"]
        for item in section["items"]
        if item["name"] == "Leave shop"
    )
    state = session.take_action(leave_action)

    assert state["phase"] == RunManager.PHASE_MAP_CHOICE
    assert state["screen"]["title"] == "Map"


def test_web_session_can_take_boss_relic_and_enter_next_act() -> None:
    session = RunSession()
    session.start(character="Ironclad", seed=ROOM_SCREEN_TEST_SEED)
    assert session.mgr is not None
    session.mgr._phase = RunManager.PHASE_BOSS_RELIC
    session.mgr._boss_relics = [BOSS_RELIC_WITHOUT_FOLLOWUP_CHOICE]

    state = session.state()
    relic_action = state["screen"]["items"][0]["action_index"]
    state = session.take_action(relic_action)

    assert state["phase"] == RunManager.PHASE_MAP_CHOICE
    assert state["screen"]["title"] == "Map"
    assert state["act"] == 2
    assert "Sozu" in state["relics"]


def test_web_session_can_finish_direct_event_and_return_to_map() -> None:
    session = RunSession()

    state = _enter_legends_were_true_event(session)
    nab_action = next(
        item["action_index"]
        for item in state["screen"]["items"]
        if item["name"] == LEGENDS_NAB_MAP_LABEL
    )
    starting_deck_size = state["deck_size"]
    state = session.take_action(nab_action)

    assert state["phase"] == RunManager.PHASE_MAP_CHOICE
    assert state["screen"]["title"] == "Map"
    assert state["deck_size"] == starting_deck_size + 1


def test_web_session_can_finish_event_reward_and_return_to_map() -> None:
    session = RunSession()

    state = _enter_legends_were_true_event(session)
    exit_action = next(
        item["action_index"]
        for item in state["screen"]["items"]
        if item["name"] == LEGENDS_FIND_EXIT_LABEL
    )
    state = session.take_action(exit_action)

    assert state["phase"] == RunManager.PHASE_CARD_REWARD
    assert state["screen"]["title"] == "Potion Reward"

    state = _skip_current_reward_screen(state, session)

    assert state["phase"] == RunManager.PHASE_MAP_CHOICE
    assert state["screen"]["title"] == "Map"
    assert state["hp"] == LEGENDS_EXIT_EXPECTED_HP


def test_web_session_can_reach_first_combat_reward() -> None:
    session = RunSession()

    state = _reach_first_combat_reward(session)
    state = _skip_reward_screens_until_map(state, session)

    assert state["phase"] == RunManager.PHASE_MAP_CHOICE
    assert state["screen"]["title"] == "Map"


def test_web_session_can_take_card_reward_and_return_to_map() -> None:
    session = RunSession()

    state = _reach_first_combat_reward(session)
    card_reward = next(
        item
        for item in state["screen"]["items"]
        if not item["name"].startswith("Skip")
    )
    starting_deck_size = state["deck_size"]
    state = session.take_action(card_reward["action_index"])

    while state["phase"] == RunManager.PHASE_CARD_REWARD:
        state = _skip_current_reward_screen(state, session)

    assert state["phase"] == RunManager.PHASE_MAP_CHOICE
    assert state["screen"]["title"] == "Map"
    assert state["deck_size"] == starting_deck_size + 1


def test_web_session_can_continue_to_second_map_node_after_rewards() -> None:
    session = RunSession()

    state = _reach_first_combat_reward(session)
    state = _skip_reward_screens_until_map(state, session)
    # The exact column of the visited row-1 node depends on the generated
    # map; the flow invariant is being back on the map at row 1.
    assert state["screen"]["current"][1] == 1

    state = _take_first_map_node(state, session)

    # The room type at the second node depends on the generated map; the flow
    # invariant is that we leave the map and enter that room at floor 2.
    room_phases = {
        RunManager.PHASE_COMBAT,
        RunManager.PHASE_SHOP,
        RunManager.PHASE_REST_SITE,
        RunManager.PHASE_EVENT,
        RunManager.PHASE_TREASURE,
    }
    assert state["phase"] in room_phases
    assert state["floor"] == 2
