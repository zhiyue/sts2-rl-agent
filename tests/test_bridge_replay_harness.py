"""Tests for the bridge replay golden-comparison harness."""

from __future__ import annotations

from pathlib import Path

import sts2_env.powers  # noqa: F401

from sts2_env.cards.ironclad_basic import create_ironclad_starter_deck, make_strike_ironclad
from sts2_env.cards.silent import create_silent_starter_deck, make_strike_silent, make_survivor
from sts2_env.core.combat import CombatState
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle
from sts2_env.parity.bridge_replay import (
    BridgeReplayRecorder,
    BridgeReplayStep,
    BridgeReplayTrace,
    combat_state_to_bridge_state,
    compare_combat_replay,
    compare_run_replay,
    load_replay_trace,
    run_manager_to_bridge_state,
    save_replay_trace,
)
from sts2_env.parity.bridge_replay_cli import build_parser
from sts2_env.run.run_manager import RunManager


def make_basic_replay_combat() -> CombatState:
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=42,
        character_id="Ironclad",
    )
    creature, ai = create_shrinker_beetle(Rng(42))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    combat.hand = [make_strike_ironclad()]
    combat.energy = 1
    return combat


def make_choice_replay_combat() -> CombatState:
    combat = CombatState(
        player_hp=70,
        player_max_hp=70,
        deck=create_silent_starter_deck(),
        rng_seed=42,
        character_id="Silent",
    )
    creature, ai = create_shrinker_beetle(Rng(42))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    combat.hand = [make_survivor(), make_strike_silent(), make_strike_silent()]
    combat.energy = 1
    return combat


def make_basic_replay_run() -> RunManager:
    return RunManager(seed=42, character_id="Ironclad")


def make_card_reward_replay_run() -> RunManager:
    run = RunManager(seed=42, character_id="Ironclad")
    run._enter_card_reward(context="regular")
    return run


class FakeBridgeClient:
    def __init__(self, states: list[dict]):
        self._states = list(states)
        self.sent_actions: list[dict] = []
        self.connected = True

    def receive_state(self) -> dict:
        if not self._states:
            raise RuntimeError("No more states queued")
        return self._states.pop(0)

    def send_action(self, action: dict) -> None:
        self.sent_actions.append(dict(action))

    def ping(self) -> bool:
        return True


def test_bridge_replay_trace_round_trip(tmp_path: Path):
    combat = make_basic_replay_combat()
    initial_state = combat_state_to_bridge_state(combat)
    assert combat.play_card(0, 0)
    next_state = combat_state_to_bridge_state(combat)
    trace = BridgeReplayTrace(
        metadata={"scenario_factory": "tests.test_bridge_replay_harness:make_basic_replay_combat"},
        initial_state=initial_state,
        steps=[BridgeReplayStep(action={"action": "play", "card_index": 0, "target_index": 0}, resulting_state=next_state)],
    )

    path = save_replay_trace(trace, tmp_path / "basic_trace.json")
    loaded = load_replay_trace(path)

    assert loaded.metadata["scenario_factory"] == "tests.test_bridge_replay_harness:make_basic_replay_combat"
    assert loaded.initial_state == trace.initial_state
    assert loaded.steps[0].action == trace.steps[0].action
    assert loaded.steps[0].resulting_state == trace.steps[0].resulting_state


def test_bridge_replay_recorder_records_state_action_state_sequence():
    initial = {"type": "combat_action", "player": {"hp": 80, "max_hp": 80, "block": 0, "energy": 1, "max_energy": 3, "powers": []}, "hand": [], "enemies": [], "draw_pile_count": 0, "discard_pile_count": 0, "exhaust_pile_count": 0, "round": 1}
    next_state = {"type": "combat_action", "player": {"hp": 80, "max_hp": 80, "block": 0, "energy": 0, "max_energy": 3, "powers": []}, "hand": [], "enemies": [], "draw_pile_count": 0, "discard_pile_count": 1, "exhaust_pile_count": 0, "round": 1}
    client = FakeBridgeClient([initial, next_state])
    recorder = BridgeReplayRecorder(client)

    assert recorder.receive_state() == initial
    recorder.play_card(0, 0)
    assert recorder.receive_state() == next_state

    assert client.sent_actions == [{"action": "play", "card_index": 0, "target_index": 0}]
    assert recorder.trace.initial_state["type"] == "combat_action"
    assert recorder.trace.steps[0].action == client.sent_actions[0]
    assert recorder.trace.steps[0].resulting_state["player"]["energy"] == 0


def test_bridge_replay_recorder_delegates_unknown_attributes():
    client = FakeBridgeClient([])
    recorder = BridgeReplayRecorder(client)

    assert recorder.connected is True
    assert recorder.ping() is True


def test_compare_combat_replay_passes_for_simple_combat_trace():
    combat = make_basic_replay_combat()
    initial_state = combat_state_to_bridge_state(combat)
    assert combat.play_card(0, 0)
    resulting_state = combat_state_to_bridge_state(combat)

    trace = BridgeReplayTrace(
        initial_state=initial_state,
        steps=[BridgeReplayStep(action={"action": "play", "card_index": 0, "target_index": 0}, resulting_state=resulting_state)],
    )
    result = compare_combat_replay(trace, factory=make_basic_replay_combat)

    assert result.success is True
    assert result.mismatches == []


def test_compare_combat_replay_handles_card_select_round_trip():
    combat = make_choice_replay_combat()
    initial_state = combat_state_to_bridge_state(combat)

    assert combat.play_card(0)
    choice_state = combat_state_to_bridge_state(combat)
    assert combat.resolve_pending_choice(0)
    resulting_state = combat_state_to_bridge_state(combat)

    trace = BridgeReplayTrace(
        initial_state=initial_state,
        steps=[
            BridgeReplayStep(action={"action": "play", "card_index": 0, "target_index": -1}, resulting_state=choice_state),
            BridgeReplayStep(action={"action": "choose", "index": 0}, resulting_state=resulting_state),
        ],
    )
    result = compare_combat_replay(trace, factory=make_choice_replay_combat)

    assert result.success is True
    assert result.mismatches == []


def test_compare_combat_replay_reports_state_mismatch():
    combat = make_basic_replay_combat()
    initial_state = combat_state_to_bridge_state(combat)
    assert combat.play_card(0, 0)
    resulting_state = combat_state_to_bridge_state(combat)
    resulting_state["player"]["hp"] = 999

    trace = BridgeReplayTrace(
        initial_state=initial_state,
        steps=[BridgeReplayStep(action={"action": "play", "card_index": 0, "target_index": 0}, resulting_state=resulting_state)],
    )
    result = compare_combat_replay(trace, factory=make_basic_replay_combat)

    assert result.success is False
    assert any("player.hp" in mismatch for mismatch in result.mismatches)


def test_run_manager_to_bridge_state_serializes_map_select():
    run = make_basic_replay_run()
    state = run_manager_to_bridge_state(run)

    assert state["type"] == "map_select"
    assert state["nodes"]
    assert all("row" in node and "col" in node and "type" in node for node in state["nodes"])


def test_compare_run_replay_handles_map_select_to_combat_transition():
    run = make_basic_replay_run()
    initial_state = run_manager_to_bridge_state(run)
    actions = [action for action in run.get_available_actions() if action.get("action") == "move"]
    run.take_action(actions[0])
    resulting_state = run_manager_to_bridge_state(run)

    trace = BridgeReplayTrace(
        mode="run",
        initial_state=initial_state,
        steps=[BridgeReplayStep(action={"action": "choose", "index": 0}, resulting_state=resulting_state)],
    )
    result = compare_run_replay(trace, factory=make_basic_replay_run)

    assert result.success is True
    assert result.mismatches == []


def test_compare_run_replay_handles_card_reward_pick_to_map_transition():
    run = make_card_reward_replay_run()
    initial_state = run_manager_to_bridge_state(run)
    run.take_action({"action": "pick_card", "index": 0})
    resulting_state = run_manager_to_bridge_state(run)

    trace = BridgeReplayTrace(
        mode="run",
        initial_state=initial_state,
        steps=[BridgeReplayStep(action={"action": "choose", "index": 0}, resulting_state=resulting_state)],
    )
    result = compare_run_replay(trace, factory=make_card_reward_replay_run)

    assert result.success is True
    assert result.mismatches == []


def test_bridge_replay_cli_show_and_compare(tmp_path: Path, capsys):
    trace = BridgeReplayTrace(
        mode="combat",
        metadata={"scenario_factory": "tests.test_bridge_replay_harness:make_basic_replay_combat"},
        initial_state=combat_state_to_bridge_state(make_basic_replay_combat()),
        steps=[],
    )
    trace_path = save_replay_trace(trace, tmp_path / "trace.json")

    parser = build_parser()

    show_args = parser.parse_args(["show", str(trace_path)])
    assert show_args.func(show_args) == 0
    show_output = capsys.readouterr().out
    assert '"step_count": 0' in show_output

    compare_args = parser.parse_args([
        "compare",
        str(trace_path),
        "--factory",
        "tests.test_bridge_replay_harness:make_basic_replay_combat",
    ])
    assert compare_args.func(compare_args) == 0
    compare_output = capsys.readouterr().out
    assert "comparison: ok" in compare_output
