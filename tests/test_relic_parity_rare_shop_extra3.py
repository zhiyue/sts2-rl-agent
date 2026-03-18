"""Focused direct parity coverage for uncovered rare/shop/event relic hooks."""

from sts2_env.cards.factory import create_card
from sts2_env.core.enums import CardId, CombatSide, RoomType
from sts2_env.run.reward_objects import CardReward
from sts2_env.run.rooms import create_room
from sts2_env.run.run_state import RunState


def _get_relic(run_state: RunState, relic_name: str):
    return next(relic for relic in run_state.player.get_relic_objects() if relic.relic_id.name == relic_name)


def test_frozen_egg_upgrades_only_power_cards_for_reward_deck_and_merchant_paths():
    run_state = RunState(seed=801, character_id="Ironclad")
    assert run_state.player.obtain_relic("FROZEN_EGG")
    relic = _get_relic(run_state, "FROZEN_EGG")

    reward = CardReward(
        run_state.player.player_id,
        cards=[
            create_card(CardId.INFLAME),
            create_card(CardId.SHRUG_IT_OFF),
            create_card(CardId.ANGER),
        ],
    )
    reward.populate(run_state, create_room(RoomType.MONSTER))
    by_id = {card.card_id: card for card in reward.cards}
    assert by_id[CardId.INFLAME].upgraded
    assert not by_id[CardId.SHRUG_IT_OFF].upgraded
    assert not by_id[CardId.ANGER].upgraded

    run_state.player.add_card_instance_to_deck(create_card(CardId.INFLAME))
    assert run_state.player.deck[-1].upgraded
    run_state.player.add_card_instance_to_deck(create_card(CardId.ANGER))
    assert not run_state.player.deck[-1].upgraded

    merchant_power = relic.modify_merchant_card_creation_results(
        run_state.player,
        create_card(CardId.INFLAME),
        is_colorless=False,
        run_state=run_state,
    )
    merchant_skill = relic.modify_merchant_card_creation_results(
        run_state.player,
        create_card(CardId.SHRUG_IT_OFF),
        is_colorless=False,
        run_state=run_state,
    )
    assert merchant_power.upgraded
    assert not merchant_skill.upgraded


def test_toxic_egg_upgrades_only_skill_cards_for_reward_deck_and_merchant_paths():
    run_state = RunState(seed=802, character_id="Ironclad")
    assert run_state.player.obtain_relic("TOXIC_EGG")
    relic = _get_relic(run_state, "TOXIC_EGG")

    reward = CardReward(
        run_state.player.player_id,
        cards=[
            create_card(CardId.SHRUG_IT_OFF),
            create_card(CardId.INFLAME),
            create_card(CardId.ANGER),
        ],
    )
    reward.populate(run_state, create_room(RoomType.MONSTER))
    by_id = {card.card_id: card for card in reward.cards}
    assert by_id[CardId.SHRUG_IT_OFF].upgraded
    assert not by_id[CardId.INFLAME].upgraded
    assert not by_id[CardId.ANGER].upgraded

    run_state.player.add_card_instance_to_deck(create_card(CardId.SHRUG_IT_OFF))
    assert run_state.player.deck[-1].upgraded
    run_state.player.add_card_instance_to_deck(create_card(CardId.ANGER))
    assert not run_state.player.deck[-1].upgraded

    merchant_skill = relic.modify_merchant_card_creation_results(
        run_state.player,
        create_card(CardId.SHRUG_IT_OFF),
        is_colorless=False,
        run_state=run_state,
    )
    merchant_power = relic.modify_merchant_card_creation_results(
        run_state.player,
        create_card(CardId.INFLAME),
        is_colorless=False,
        run_state=run_state,
    )
    assert merchant_skill.upgraded
    assert not merchant_power.upgraded


def test_lizard_tail_prevents_death_once_and_heals_half_max_hp():
    run_state = RunState(seed=803, character_id="Ironclad")
    assert run_state.player.obtain_relic("LIZARD_TAIL")
    relic = _get_relic(run_state, "LIZARD_TAIL")

    run_state.player.max_hp = 81
    run_state.player.current_hp = 1

    assert relic.should_die_late(run_state.player, None) is False
    assert run_state.player.current_hp == 41
    assert relic.should_die_late(run_state.player, None) is None
    assert run_state.player.current_hp == 41


def test_maw_bank_grants_gold_per_room_until_item_purchase():
    run_state = RunState(seed=804, character_id="Ironclad")
    assert run_state.player.obtain_relic("MAW_BANK")
    relic = _get_relic(run_state, "MAW_BANK")
    start_gold = run_state.player.gold

    relic.after_room_entered(run_state.player, RoomType.MONSTER)
    relic.after_room_entered(run_state.player, RoomType.ELITE)
    assert run_state.player.gold == start_gold + 24

    relic.on_item_purchased(run_state.player)
    relic.after_room_entered(run_state.player, RoomType.SHOP)
    assert run_state.player.gold == start_gold + 24


def test_sozu_blocks_potion_procurement_and_adds_one_max_energy():
    run_state = RunState(seed=805, character_id="Ironclad")
    assert run_state.player.obtain_relic("SOZU")
    relic = _get_relic(run_state, "SOZU")

    assert relic.should_procure_potion(run_state.player) is False
    assert relic.modify_max_energy(run_state.player, 3) == 4


def test_velvet_choker_caps_card_plays_each_turn_and_resets_next_turn():
    run_state = RunState(seed=806, character_id="Ironclad")
    assert run_state.player.obtain_relic("VELVET_CHOKER")
    relic = _get_relic(run_state, "VELVET_CHOKER")

    assert relic.modify_max_energy(run_state.player, 3) == 4
    relic.before_side_turn_start(run_state.player, CombatSide.PLAYER, None)
    for _ in range(6):
        assert relic.should_play(run_state.player, object(), None) is None
        relic.after_card_played(run_state.player, object(), None)
    assert relic.should_play(run_state.player, object(), None) is False

    relic.before_side_turn_start(run_state.player, CombatSide.PLAYER, None)
    assert relic.should_play(run_state.player, object(), None) is None

