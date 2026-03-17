"""Regression tests for rest-site relic hook plumbing."""

from sts2_env.core.enums import RoomType
from sts2_env.run.run_manager import RunManager
from sts2_env.run.run_state import RunState
from sts2_env.run.rest_site import generate_rest_site_options


def test_regal_pillow_increases_rest_heal_amount():
    run_state = RunState(seed=201, character_id="Ironclad")
    assert run_state.player.obtain_relic("REGAL_PILLOW")
    run_state.player.max_hp = 100
    run_state.player.current_hp = 40

    heal = next(option for option in generate_rest_site_options(run_state.player) if option.option_id == "HEAL")
    heal.execute(run_state.player)

    assert run_state.player.current_hp == 85


def test_dream_catcher_surfaces_card_reward_after_rest():
    mgr = RunManager(seed=202, character_id="Ironclad")
    assert mgr.run_state.player.obtain_relic("DREAM_CATCHER")
    mgr.run_state.player.current_hp = 30
    mgr._enter_rest_site()

    result = mgr._do_rest_site({"option_id": "HEAL"})

    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert any(action["action"] == "pick_card" for action in mgr.get_available_actions())


def test_tiny_mailbox_surfaces_potion_reward_after_rest():
    mgr = RunManager(seed=203, character_id="Ironclad")
    assert mgr.run_state.player.obtain_relic("TINY_MAILBOX")
    mgr._enter_rest_site()

    result = mgr._do_rest_site({"option_id": "HEAL"})

    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert any(action["action"] == "pick_potion" for action in mgr.get_available_actions())


def test_stone_humidifier_grants_max_hp_after_rest_heal():
    run_state = RunState(seed=204, character_id="Ironclad")
    assert run_state.player.obtain_relic("STONE_HUMIDIFIER")
    run_state.player.max_hp = 80
    run_state.player.current_hp = 20

    heal = next(option for option in generate_rest_site_options(run_state.player) if option.option_id == "HEAL")
    heal.execute(run_state.player)

    assert run_state.player.max_hp == 85


def test_entering_shop_triggers_meal_ticket_heal():
    mgr = RunManager(seed=205, character_id="Ironclad")
    assert mgr.run_state.player.obtain_relic("MEAL_TICKET")
    mgr.run_state.player.current_hp = 40
    mgr.run_state.player.max_hp = 80

    mgr._enter_room(RoomType.SHOP)

    assert mgr.run_state.player.current_hp == 55


def test_miniature_tent_keeps_rest_site_open_for_second_option():
    mgr = RunManager(seed=206, character_id="Ironclad")
    assert mgr.run_state.player.obtain_relic("MINIATURE_TENT")
    mgr.run_state.player.current_hp = 20
    mgr._enter_rest_site()

    result = mgr._do_rest_site({"option_id": "HEAL"})

    assert result["phase"] == RunManager.PHASE_REST_SITE
    actions = mgr.get_available_actions()
    assert all(action["option_id"] != "HEAL" for action in actions)
    assert any(action["option_id"] == "SMITH" for action in actions)


def test_miniature_tent_returns_to_rest_site_after_dream_catcher_reward():
    mgr = RunManager(seed=207, character_id="Ironclad")
    assert mgr.run_state.player.obtain_relic("MINIATURE_TENT")
    assert mgr.run_state.player.obtain_relic("DREAM_CATCHER")
    mgr.run_state.player.current_hp = 20
    mgr._enter_rest_site()

    first = mgr._do_rest_site({"option_id": "HEAL"})
    assert first["phase"] == RunManager.PHASE_CARD_REWARD

    second = mgr.take_action({"action": "pick_card", "index": 0})
    assert second["phase"] == RunManager.PHASE_REST_SITE
    actions = mgr.get_available_actions()
    assert all(action["option_id"] != "HEAL" for action in actions)
    assert any(action["option_id"] == "SMITH" for action in actions)
