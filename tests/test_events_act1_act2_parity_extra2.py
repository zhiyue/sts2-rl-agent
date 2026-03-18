"""Focused parity coverage for additional Act 1 / Act 2 events."""

import sts2_env.events.act1  # noqa: F401
import sts2_env.events.act2  # noqa: F401

from sts2_env.cards.factory import create_card
from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.cards.status import make_spore_mind
from sts2_env.core.enums import CardId, CardRarity, CardType
from sts2_env.events.act1 import TheLegendsWereTrue
from sts2_env.events.act2 import (
    SpiralingWhirlpool,
    StoneOfAllTime,
    TheFutureOfPotions,
    WaterloggedScriptorium,
)
from sts2_env.potions.base import create_potion
from sts2_env.run.reward_objects import CardReward, PotionReward
from sts2_env.run.run_state import RunState


def _make_run_state(seed: int = 401) -> RunState:
    run_state = RunState(seed=seed, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()
    return run_state


def test_the_legends_were_true_adds_spoils_map_and_returns_a_potion_reward():
    run_state = _make_run_state(401)
    event = TheLegendsWereTrue()

    assert event.is_allowed(run_state)
    run_state.player.current_hp = 9
    assert event.is_allowed(run_state) is False
    run_state.player.current_hp = 20

    deck_before = len(run_state.player.deck)
    nab = event.choose(run_state, "nab_map")
    assert nab.finished
    assert len(run_state.player.deck) == deck_before + 1
    assert run_state.player.deck[-1].card_id == CardId.SPOILS_MAP

    hp_before = run_state.player.current_hp
    potions_before = len(run_state.player.held_potions())
    exit_result = event.choose(run_state, "find_exit")
    assert exit_result.finished
    assert run_state.player.current_hp == hp_before - 8
    assert len(run_state.player.held_potions()) == potions_before

    reward_objects = exit_result.rewards["reward_objects"]
    assert len(reward_objects) == 1
    assert isinstance(reward_objects[0], PotionReward)
    assert reward_objects[0].potion_id is not None


def test_spiraling_whirlpool_requires_spiral_targets_and_applies_observe_drink():
    blocked_state = _make_run_state(402)
    blocked_state.player.deck = [create_card(CardId.BASH)]
    blocked_event = SpiralingWhirlpool()
    assert blocked_event.is_allowed(blocked_state) is False

    run_state = _make_run_state(403)
    event = SpiralingWhirlpool()
    assert event.is_allowed(run_state)

    observe = event.choose(run_state, "observe")
    assert not observe.finished
    assert event.pending_choice is not None
    target = event.pending_choice.options[0].card

    resolved = event.resolve_pending_choice(0)
    assert resolved.finished
    assert target.enchantments.get("Spiral") == 1

    run_state.player.current_hp = run_state.player.max_hp - 20
    heal_before = run_state.player.current_hp
    heal_amount = int(run_state.player.max_hp * 0.33)
    drink = event.choose(run_state, "drink")
    assert drink.finished
    assert run_state.player.current_hp == min(run_state.player.max_hp, heal_before + heal_amount)


def test_stone_of_all_time_lift_discards_potion_and_push_enchants_attack():
    run_state = _make_run_state(404)
    run_state.current_act_index = 1
    run_state.player.add_potion(create_potion("FirePotion"))
    run_state.player.add_potion(create_potion("FlexPotion"))
    event = StoneOfAllTime()

    assert event.is_allowed(run_state)
    options = event.generate_initial_options(run_state)
    assert [option.enabled for option in options] == [True, True]
    potions_before = len(run_state.player.held_potions())
    max_hp_before = run_state.player.max_hp
    lift = event.choose(run_state, "lift")
    assert lift.finished
    assert run_state.player.max_hp == max_hp_before + 10
    assert len(run_state.player.held_potions()) == potions_before - 1

    hp_before = run_state.player.current_hp
    push = event.choose(run_state, "push")
    assert not push.finished
    assert event.pending_choice is not None
    target = event.pending_choice.options[0].card

    resolved = event.resolve_pending_choice(0)
    assert resolved.finished
    assert run_state.player.current_hp == hp_before - 6
    assert target.enchantments.get("Vigorous") == 8

    no_push_state = _make_run_state(4041)
    no_push_state.current_act_index = 1
    no_push_state.player.add_potion(create_potion("FirePotion"))
    no_push_state.player.deck = [make_spore_mind(), make_spore_mind()]
    no_push_event = StoneOfAllTime()
    no_push_options = no_push_event.generate_initial_options(no_push_state)
    assert [option.enabled for option in no_push_options] == [True, False]


def test_future_of_potions_trade_discards_potion_and_builds_upgraded_rewards():
    run_state = _make_run_state(405)
    run_state.player.add_potion(create_potion("FirePotion"))
    run_state.player.add_potion(create_potion("Clarity"))
    run_state.player.add_potion(create_potion("FairyInABottle"))
    event = TheFutureOfPotions()

    assert event.is_allowed(run_state)
    options = event.generate_initial_options(run_state)
    assert [opt.option_id for opt in options] == ["trade_0", "trade_1", "trade_2"]

    potions_before = len(run_state.player.held_potions())
    result = event.choose(run_state, "trade_0")
    assert result.finished
    assert len(run_state.player.held_potions()) == potions_before - 1

    reward_objects = result.rewards["reward_objects"]
    assert len(reward_objects) == 1
    assert isinstance(reward_objects[0], CardReward)
    reward = reward_objects[0]
    assert len(reward.cards) == 3
    assert all(card.upgraded for card in reward.cards)
    assert all(card.rarity == CardRarity.COMMON for card in reward.cards)
    assert len({card.card_type for card in reward.cards}) == 1
    assert reward.cards[0].card_type in {CardType.ATTACK, CardType.SKILL}


def test_waterlogged_scriptorium_tentacle_and_prickly_apply_steady_enchantments():
    run_state = _make_run_state(406)
    run_state.player.gold = 320
    event = WaterloggedScriptorium()

    locked_state = _make_run_state(4051)
    locked_state.player.gold = 64
    locked_options = event.generate_initial_options(locked_state)
    assert [option.option_id for option in locked_options] == ["bloody_ink", "tentacle_quill", "prickly_sponge"]
    assert [option.enabled for option in locked_options] == [True, False, False]

    max_hp_before = run_state.player.max_hp
    bloody = event.choose(run_state, "bloody_ink")
    assert bloody.finished
    assert run_state.player.max_hp == max_hp_before + 6

    gold_before = run_state.player.gold
    tentacle = event.choose(run_state, "tentacle_quill")
    assert not tentacle.finished
    assert event.pending_choice is not None
    assert run_state.player.gold == gold_before - 65
    first = event.pending_choice.options[0].card
    tentacle_done = event.resolve_pending_choice(0)
    assert tentacle_done.finished
    assert first.enchantments.get("Steady") == 1

    gold_before = run_state.player.gold
    prickly = event.choose(run_state, "prickly_sponge")
    assert not prickly.finished
    assert event.pending_choice is not None
    assert run_state.player.gold == gold_before - 155
    selected_cards = [event.pending_choice.options[0].card, event.pending_choice.options[1].card]
    event.resolve_pending_choice(0)
    event.resolve_pending_choice(1)
    prickly_done = event.resolve_pending_choice(None)
    assert prickly_done.finished
    assert all(card.enchantments.get("Steady") == 1 for card in selected_cards)


def test_waterlogged_scriptorium_prickly_handles_sparse_or_empty_candidates():
    one_candidate_state = _make_run_state(407)
    one_candidate_state.player.gold = 200
    one_candidate_state.player.deck = [
        create_card(CardId.STRIKE_IRONCLAD),
        make_spore_mind(),
        make_spore_mind(),
    ]
    event = WaterloggedScriptorium()
    result = event.choose(one_candidate_state, "prickly_sponge")
    assert not result.finished
    assert event.pending_choice is not None
    assert len(event.pending_choice.options) == 1
    done = event.resolve_pending_choice(0)
    assert done.finished
    assert one_candidate_state.player.deck[0].enchantments.get("Steady") == 1

    no_candidate_state = _make_run_state(408)
    no_candidate_state.player.gold = 200
    no_candidate_state.player.deck = [make_spore_mind(), make_spore_mind()]
    no_card_event = WaterloggedScriptorium()
    gold_before = no_candidate_state.player.gold
    finished = no_card_event.choose(no_candidate_state, "prickly_sponge")
    assert finished.finished
    assert no_card_event.pending_choice is None
    assert no_candidate_state.player.gold == gold_before - 155
