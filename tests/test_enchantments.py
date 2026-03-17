"""Focused tests for persistent card enchantment support."""

from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.run.rest_site import CloneOption
from sts2_env.run.reward_objects import RelicReward
from sts2_env.run.run_manager import RunManager
from sts2_env.run.run_state import PlayerState


def test_player_state_enchant_helpers_mark_cards():
    player = PlayerState(character_id="Ironclad", deck=create_ironclad_starter_deck())

    enchanted = player.enchant_cards("Clone", 4, 1)
    assert enchanted == 1
    assert any(card.has_enchantment("Clone") for card in player.deck)

    basic_strikes = [card for card in player.deck if card.rarity.name == "BASIC" and "STRIKE" in card.card_id.name]
    player.enchant_basic_strikes("TezcatarasEmber")
    assert all(card.has_enchantment("TezcatarasEmber") for card in basic_strikes)


def test_clone_rest_option_duplicates_clone_enchanted_cards():
    player = PlayerState(character_id="Ironclad", deck=create_ironclad_starter_deck())
    player.enchant_cards("Clone", 4, 2)
    original_size = len(player.deck)

    result = CloneOption().execute(player)

    assert "Cloned" in result
    assert len(player.deck) == original_size + 2


def test_obtain_relic_triggers_after_obtained_enchantments():
    player = PlayerState(character_id="Ironclad", deck=create_ironclad_starter_deck())
    from sts2_env.run.run_state import RunState

    run_state = RunState(seed=53, character_id="Ironclad")
    run_state.player = player
    player.run_state = run_state
    run_state.players = [player]

    assert player.obtain_relic("PAELS_GROWTH")
    assert any(card.has_enchantment("Clone") for card in player.deck)


def test_relic_reward_can_enqueue_followup_rewards_via_after_obtained():
    mgr = RunManager(seed=59, character_id="Ironclad")
    reward = RelicReward(mgr.run_state.player.player_id, relic_id="CAULDRON")
    mgr._current_reward = reward
    mgr._phase = RunManager.PHASE_CARD_REWARD

    result = mgr._do_relic_reward_pick()

    assert result["phase"] == RunManager.PHASE_CARD_REWARD
    actions = mgr.get_available_actions()
    assert any(action["action"] == "pick_potion" for action in actions)
