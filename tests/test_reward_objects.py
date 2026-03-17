"""Tests for reward objects and rewards-set assembly."""

from sts2_env.cards.factory import create_card
from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.cards.status import make_curse_of_the_bell
from sts2_env.core.enums import CardId, RoomType
from sts2_env.run.reward_objects import CardReward, PotionReward, RelicReward, RewardsSet
from sts2_env.run.rooms import CombatRoom
from sts2_env.run.run_state import RunState


def test_rewards_set_merges_combat_room_extra_rewards_for_player():
    run_state = RunState(seed=42, character_id="Ironclad")
    room = CombatRoom(room_type=RoomType.MONSTER)
    extra = CardReward(run_state.player.player_id, context="regular")
    room.add_extra_reward(run_state.player.player_id, extra)

    rewards = RewardsSet(run_state.player.player_id).with_rewards_from_room(room, run_state)
    generated = rewards.generate_without_offering(run_state)

    assert any(reward is extra for reward in generated)


def test_cauldron_after_obtained_queues_five_potion_rewards():
    run_state = RunState(seed=42, character_id="Ironclad")
    run_state.initialize_run()

    assert run_state.player.obtain_relic("CAULDRON")

    assert len(run_state.pending_rewards) == 5
    assert all(isinstance(reward, PotionReward) for reward in run_state.pending_rewards)


def test_orrery_after_obtained_queues_five_card_rewards():
    run_state = RunState(seed=42, character_id="Ironclad")
    run_state.initialize_run()

    assert run_state.player.obtain_relic("ORRERY")

    assert len(run_state.pending_rewards) == 5
    assert all(isinstance(reward, CardReward) for reward in run_state.pending_rewards)


def test_calling_bell_adds_curse_and_queues_three_relic_rewards():
    run_state = RunState(seed=42, character_id="Ironclad")
    run_state.initialize_run()

    assert run_state.player.obtain_relic("CALLING_BELL")

    assert any(card.card_id == make_curse_of_the_bell().card_id for card in run_state.player.deck)
    assert len(run_state.pending_rewards) == 3
    assert all(isinstance(reward, RelicReward) for reward in run_state.pending_rewards)


def test_war_paint_upgrades_two_skills_on_obtain():
    run_state = RunState(seed=42, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()

    assert run_state.player.obtain_relic("WAR_PAINT")

    upgraded_skills = [card for card in run_state.player.deck if card.upgraded and card.card_type.name == "SKILL"]
    assert len(upgraded_skills) >= 2


def test_whetstone_upgrades_two_attacks_on_obtain():
    run_state = RunState(seed=42, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()

    assert run_state.player.obtain_relic("WHETSTONE")

    upgraded_attacks = [card for card in run_state.player.deck if card.upgraded and card.card_type.name == "ATTACK"]
    assert len(upgraded_attacks) >= 2


def test_archaic_tooth_transforms_a_basic_card():
    run_state = RunState(seed=42, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()
    before = [card.card_id for card in run_state.player.deck]

    assert run_state.player.obtain_relic("ARCHAIC_TOOTH")

    after = [card.card_id for card in run_state.player.deck]
    assert before != after


def test_astrolabe_transforms_and_upgrades_three_cards():
    run_state = RunState(seed=42, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()
    before = [card.card_id for card in run_state.player.deck[:3]]

    assert run_state.player.obtain_relic("ASTROLABE")

    after = [card.card_id for card in run_state.player.deck[:3]]
    assert before != after
    assert sum(1 for card in run_state.player.deck if card.upgraded) >= 3


def test_precise_scissors_removes_one_card_from_deck():
    run_state = RunState(seed=42, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()
    starting_deck = len(run_state.player.deck)

    assert run_state.player.obtain_relic("PRECISE_SCISSORS")

    assert len(run_state.player.deck) == starting_deck - 1


def test_pandoras_box_only_transforms_basic_strike_defend_cards():
    run_state = RunState(seed=42, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()
    bash_count_before = sum(1 for card in run_state.player.deck if card.card_id == CardId.BASH)

    assert run_state.player.obtain_relic("PANDORAS_BOX")

    assert sum(1 for card in run_state.player.deck if card.card_id == CardId.BASH) == bash_count_before
    assert sum(
        1
        for card in run_state.player.deck
        if card.rarity.name == "BASIC" and ("STRIKE" in card.card_id.name or "DEFEND" in card.card_id.name)
    ) == 0


def test_duplicate_card_helper_excludes_quest_cards_from_candidates():
    run_state = RunState(seed=52, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()
    run_state.player.deck.append(create_card(CardId.BYRDONIS_EGG))

    assert run_state.player.duplicate_card_from_deck(cards=run_state.player.duplicable_deck_cards())

    assert sum(1 for card in run_state.player.deck if card.card_id == CardId.BYRDONIS_EGG) == 1
