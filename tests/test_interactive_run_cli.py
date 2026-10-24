"""Tests for the interactive full-run CLI support surface."""

import sts2_env.events  # noqa: F401

from sts2_env.cli.play_run import (
    describe_card,
    describe_action,
    describe_intent,
    display_event,
    display_map,
    display_name,
    display_boss_relics,
    display_combat,
    display_reward,
    display_rest_site,
    display_shop,
    display_text,
    display_treasure,
)
from sts2_env.core.enums import RoomType
from sts2_env.monsters.intents import attack_intent, multi_attack_intent
from sts2_env.potions.base import create_potion
from sts2_env.run.reward_objects import RelicReward
from sts2_env.run.run_manager import RunManager


NEOW_TEST_SEED = 123
COMBAT_TEST_SEED = 456


def test_run_manager_can_start_at_neow_and_force_relic_pick() -> None:
    mgr = RunManager(seed=NEOW_TEST_SEED, character_id="Ironclad", start_with_neow=True)

    assert mgr.phase == RunManager.PHASE_EVENT
    assert [action["action"] for action in mgr.get_available_actions()] == [
        "event_choice",
        "event_choice",
        "event_choice",
    ]

    result = mgr.take_action(mgr.get_available_actions()[0])
    assert result["phase"] == RunManager.PHASE_CARD_REWARD

    actions = mgr.get_available_actions()
    assert [action["action"] for action in actions] == ["pick_relic_reward"]
    assert mgr.take_action({"action": "skip_relic"})["success"] is False

    relic = actions[0]["relic_id"]
    result = mgr.take_action(actions[0])

    assert result["phase"] == RunManager.PHASE_MAP_CHOICE
    assert relic in mgr.run_state.player.relics


def test_run_manager_combat_actions_include_potions() -> None:
    mgr = RunManager(seed=COMBAT_TEST_SEED, character_id="Ironclad")
    mgr._enter_combat(RoomType.MONSTER)
    combat = mgr.get_combat_state()
    assert combat is not None

    combat.potions = [create_potion("FirePotion")]
    actions = mgr.get_available_actions()

    potion_actions = [action for action in actions if action["action"] == "use_potion"]
    assert potion_actions
    assert potion_actions[0]["potion_id"] == "FirePotion"


def test_interactive_cli_uses_player_readable_names() -> None:
    mgr = RunManager(seed=COMBAT_TEST_SEED, character_id="Ironclad")
    card = mgr.run_state.player.deck[0]

    assert display_name("BOOMING_CONCH") == "Booming Conch"
    assert display_name("TheLegendsWereTrue") == "The Legends Were True"
    assert display_text("Obtained relic BOOMING_CONCH.") == "Obtained relic Booming Conch."
    assert display_text("BASH(2E 8dmg)") == "Bash(2E 8dmg)"
    assert "STRIKE_IRONCLAD" not in describe_card(card)
    assert describe_action({
        "action": "play_card",
        "card_id": "BASH",
        "hand_index": 0,
        "target_name": "LEAF_SLIME_S",
    }) == "Play Bash from hand[0] -> Leaf Slime S"


def test_interactive_cli_describes_enemy_intents(capsys) -> None:
    mgr = RunManager(seed=COMBAT_TEST_SEED, character_id="Ironclad")
    mgr._enter_combat(RoomType.MONSTER)
    combat = mgr.get_combat_state()
    assert combat is not None

    display_combat(combat)
    output = capsys.readouterr().out

    assert "Intent:" in output
    assert describe_intent(attack_intent(7)) == "Attack 7"
    assert describe_intent(multi_attack_intent(3, 2)) == "Attack 3x2"


def test_interactive_cli_displays_map_position_and_next_paths(capsys) -> None:
    mgr = RunManager(seed=COMBAT_TEST_SEED, character_id="Ironclad")
    actions = mgr.get_available_actions()

    display_map(mgr, actions)
    output = capsys.readouterr().out

    assert "MAP:" in output
    assert "Current: start" in output
    assert "* reachable" in output
    assert "NEXT PATHS:" in output
    assert "->" in output


def test_interactive_cli_shows_event_option_descriptions(capsys) -> None:
    mgr = RunManager(seed=NEOW_TEST_SEED, character_id="Ironclad", start_with_neow=True)

    actions = mgr.get_available_actions()
    display_event(mgr)
    output = capsys.readouterr().out

    assert "EVENT: Neow" in output
    assert any(action["description"] == "Gain a positive relic" for action in actions)
    assert any(action["description"] == "Gain a cursed relic" for action in actions)


def test_interactive_cli_displays_reward_contents(capsys) -> None:
    mgr = RunManager(seed=NEOW_TEST_SEED, character_id="Ironclad", start_with_neow=True)
    mgr.take_action(mgr.get_available_actions()[0])

    display_reward(mgr)
    output = capsys.readouterr().out

    assert "REWARD:" in output
    assert "Relic:" in output
    assert "Fishing Rod" in output


def test_interactive_cli_displays_shop_inventory(capsys) -> None:
    mgr = RunManager(seed=COMBAT_TEST_SEED, character_id="Ironclad")
    mgr._enter_shop()

    display_shop(mgr)
    output = capsys.readouterr().out

    assert "SHOP:" in output
    assert "Cards:" in output
    assert "Relics:" in output
    assert "Potions:" in output
    assert "Remove card:" in output


def test_interactive_cli_displays_rest_site_options(capsys) -> None:
    mgr = RunManager(seed=COMBAT_TEST_SEED, character_id="Ironclad")
    mgr._enter_rest_site()

    display_rest_site(mgr)
    output = capsys.readouterr().out

    assert "REST SITE:" in output
    assert "Rest" in output
    assert "Smith" in output


def test_interactive_cli_displays_boss_relic_choices(capsys) -> None:
    mgr = RunManager(seed=COMBAT_TEST_SEED, character_id="Ironclad")
    mgr._phase = RunManager.PHASE_BOSS_RELIC
    mgr._boss_relics = ["BLACK_STAR", "SOZU", "BEAUTIFUL_BRACELET"]

    display_boss_relics(mgr)
    output = capsys.readouterr().out

    assert "BOSS RELICS:" in output
    assert "Black Star" in output
    assert "Beautiful Bracelet" in output


def test_interactive_cli_displays_treasure_relic(capsys) -> None:
    mgr = RunManager(seed=COMBAT_TEST_SEED, character_id="Ironclad")
    mgr._phase = RunManager.PHASE_TREASURE
    mgr._current_reward = RelicReward(mgr.run_state.player.player_id, relic_id="LANTERN")

    display_treasure(mgr)
    output = capsys.readouterr().out

    assert "TREASURE:" in output
    assert "Lantern" in output
