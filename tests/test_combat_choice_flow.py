"""Tests for reusable combat choice flow surfaced via RunManager."""

from sts2_env.cards.factory import create_card
from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.cards.ironclad_basic import make_bash, make_defend_ironclad, make_strike_ironclad
from sts2_env.cards.regent import make_gather_light
from sts2_env.cards.necrobinder import make_dredge
from sts2_env.cards.status import make_wish
from sts2_env.core.combat import CombatState
from sts2_env.run.run_state import PlayerState
from sts2_env.core.enums import CardId, RoomType
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle
from sts2_env.run.run_manager import RunManager


def test_run_manager_surfaces_choose_actions_for_pending_combat_choice():
    mgr = RunManager(seed=1, character_id="Ironclad")
    mgr._phase = RunManager.PHASE_COMBAT

    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=7,
        character_id="Ironclad",
    )
    creature, ai = create_shrinker_beetle(Rng(7))
    combat.add_enemy(creature, ai)
    combat.hand = [make_wish()]
    combat.draw_pile = [make_strike_ironclad()]
    combat.energy = 1
    mgr._combat = combat

    assert combat.play_card(0)
    actions = mgr.get_available_actions()

    assert all(action["action"] == "choose" for action in actions)
    assert actions[0]["card_id"] == "STRIKE_IRONCLAD"

    result = mgr.take_action({"action": "choose", "index": 0})
    assert result["success"] is True
    assert combat.pending_choice is None
    assert any(card.card_id.name == "STRIKE_IRONCLAD" for card in combat.hand)


def test_run_manager_requires_confirm_choice_for_exact_multi_select():
    mgr = RunManager(seed=1, character_id="Necrobinder")
    mgr._phase = RunManager.PHASE_COMBAT

    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=7,
        character_id="Necrobinder",
    )
    creature, ai = create_shrinker_beetle(Rng(7))
    combat.add_enemy(creature, ai)
    strike = make_strike_ironclad()
    defend = make_defend_ironclad()
    bash = make_bash()
    combat.hand = [make_dredge()]
    combat.discard_pile = [strike, defend, bash]
    combat.energy = 1
    mgr._combat = combat

    result = mgr.take_action({"action": "play_card", "hand_index": 0})
    assert result["success"] is True
    assert combat.pending_choice is not None

    actions = mgr.get_available_actions()
    assert all(action["action"] == "choose" for action in actions)

    assert mgr.take_action({"action": "choose", "index": 0})["success"] is True
    assert mgr.take_action({"action": "choose", "index": 1})["success"] is True
    actions = mgr.get_available_actions()
    assert all(action["action"] == "choose" for action in actions)

    assert mgr.take_action({"action": "choose", "index": 2})["success"] is True
    actions = mgr.get_available_actions()
    confirm_actions = [action for action in actions if action["action"] == "confirm_choice"]
    choose_actions = [action for action in actions if action["action"] == "choose"]

    assert len(confirm_actions) == 1
    assert confirm_actions[0]["selected_count"] == 3
    assert [action["selected"] for action in choose_actions] == [True, True, True]

    result = mgr.take_action({"action": "confirm_choice"})
    assert result["success"] is True
    assert combat.pending_choice is None
    assert combat.hand == [strike, defend, bash]


def test_run_manager_can_skip_allow_skip_choice_via_confirm_choice():
    mgr = RunManager(seed=1, character_id="Ironclad")
    mgr._phase = RunManager.PHASE_COMBAT

    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=7,
        character_id="Ironclad",
    )
    creature, ai = create_shrinker_beetle(Rng(7))
    combat.add_enemy(creature, ai)
    strike = make_strike_ironclad()
    combat.hand = [create_card(CardId.PURITY), strike]
    combat.energy = 1
    mgr._combat = combat

    result = mgr.take_action({"action": "play_card", "hand_index": 0})
    assert result["success"] is True
    assert combat.pending_choice is not None

    actions = mgr.get_available_actions()
    assert any(action["action"] == "confirm_choice" for action in actions)
    assert any(action["action"] == "choose" and action["card_id"] == "STRIKE_IRONCLAD" for action in actions)

    result = mgr.take_action({"action": "confirm_choice"})
    assert result["success"] is True
    assert combat.pending_choice is None
    assert strike in combat.hand
    assert strike not in combat.exhaust_pile


def test_run_manager_queues_extra_card_reward_screens_and_adds_picked_card_to_deck():
    mgr = RunManager(seed=2, character_id="Ironclad")
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=11,
        character_id="Ironclad",
    )
    combat.player_won = True
    combat.is_over = True
    combat.extra_card_rewards = 1
    mgr._combat = combat
    mgr._current_room_type = RoomType.MONSTER
    mgr.run_state.potion_reward_odds.current_value = -1.0

    deck_size_before = len(mgr.run_state.player.deck)
    result = mgr._resolve_combat_end()
    assert result["phase"] == RunManager.PHASE_CARD_REWARD

    actions = mgr.get_available_actions()
    reward_actions = [action for action in actions if action["action"] == "pick_card"]
    assert len(reward_actions) == 3
    assert "card_id" in reward_actions[0]

    pick = mgr.take_action({"action": "pick_card", "index": 0})
    assert pick["phase"] == RunManager.PHASE_CARD_REWARD
    assert len(mgr.run_state.player.deck) == deck_size_before + 1

    skip = mgr.take_action({"action": "skip"})
    assert skip["phase"] == RunManager.PHASE_MAP_CHOICE


def test_run_manager_offers_claimable_potion_reward_after_cards():
    mgr = RunManager(seed=3, character_id="Ironclad")
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=12,
        character_id="Ironclad",
    )
    combat.player_won = True
    combat.is_over = True
    mgr._combat = combat
    mgr._current_room_type = RoomType.MONSTER
    mgr.run_state.potion_reward_odds.current_value = 1.0

    result = mgr._resolve_combat_end()
    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    assert result["potion_reward"] is not None

    mgr.take_action({"action": "skip"})
    actions = mgr.get_available_actions()
    assert any(action["action"] == "pick_potion" for action in actions)

    take = mgr.take_action({"action": "pick_potion"})
    assert take["success"] is True
    assert any(p is not None and p.potion_id == take["potion_id"] for p in mgr.run_state.player.potions)


def test_run_manager_can_select_ally_player_and_play_from_their_hand():
    mgr = RunManager(seed=4, character_id="Ironclad")
    mgr._phase = RunManager.PHASE_COMBAT

    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=15,
        character_id="Ironclad",
    )
    creature, ai = create_shrinker_beetle(Rng(15))
    combat.add_enemy(creature, ai)
    ally_state = PlayerState(player_id=2, character_id="Regent", max_hp=60, current_hp=60)
    ally = combat.add_ally_player(ally_state)
    ally_combat_state = combat.combat_player_state_for(ally)
    assert ally_combat_state is not None
    card = make_gather_light()
    card.owner = ally
    ally_combat_state.hand = [card]
    ally_combat_state.zone_map["hand"] = ally_combat_state.hand
    ally_combat_state.energy = 1
    mgr._combat = combat

    actions = mgr.get_available_actions()
    select_actions = [action for action in actions if action["action"] == "select_player"]
    assert len(select_actions) >= 2

    selected = mgr.take_action({"action": "select_player", "player_id": 2})
    assert selected["success"] is True

    actions = mgr.get_available_actions()
    ally_play = next(
        action for action in actions
        if action["action"] == "play_card" and action.get("player_id") == 2
    )
    result = mgr.take_action(ally_play)

    assert result["success"] is True
    assert ally.block == 7
    assert ally.stars == 1
