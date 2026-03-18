"""Focused parity tests for additional uncovered shared events."""

import sts2_env.events.shared  # noqa: F401

from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.core.enums import CardId, CardRarity
from sts2_env.events.shared import (
    ColorfulPhilosophers,
    SunkenTreasury,
    TabletOfTruth,
    TinkerTime,
    ThisOrThat,
    Wellspring,
)
from sts2_env.run.reward_objects import CardReward
from sts2_env.run.run_state import RunState


def _make_run_state(seed: int = 401, character_id: str = "Ironclad") -> RunState:
    run_state = RunState(seed=seed, character_id=character_id)
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()
    return run_state


def _count_card(deck, card_id: CardId) -> int:
    return sum(1 for card in deck if card.card_id == card_id)


def test_colorful_philosophers_choice_surfaces_three_rarity_tiered_card_rewards():
    run_state = _make_run_state(401, character_id="Defect")
    event = ColorfulPhilosophers()

    options = event.generate_initial_options(run_state)
    assert len(options) == 3
    chosen_option_id = options[0].option_id
    chosen_pool = event._choices[chosen_option_id]

    result = event.choose(run_state, chosen_option_id)
    assert result.finished
    rewards = result.rewards["reward_objects"]
    assert len(rewards) == 3
    assert all(isinstance(reward, CardReward) for reward in rewards)
    assert all(reward.character_ids == (chosen_pool,) for reward in rewards)
    assert all(reward.use_default_character_pool is False for reward in rewards)
    assert [reward.forced_rarities for reward in rewards] == [
        (CardRarity.COMMON, CardRarity.COMMON, CardRarity.COMMON),
        (CardRarity.UNCOMMON, CardRarity.UNCOMMON, CardRarity.UNCOMMON),
        (CardRarity.RARE, CardRarity.RARE, CardRarity.RARE),
    ]


def test_sunken_treasury_first_and_second_chests_apply_gold_and_greed_curse():
    first_state = _make_run_state(402)
    first_event = SunkenTreasury()
    first_state.rng.up_front.next_int = lambda low, high: 0
    first_event.generate_initial_options(first_state)
    first_start_gold = first_state.player.gold

    first = first_event.choose(first_state, "first_chest")
    assert first.finished
    assert first_state.player.gold == first_start_gold + 60

    second_state = _make_run_state(403)
    second_event = SunkenTreasury()
    second_state.rng.up_front.next_int = lambda low, high: 0
    second_event.generate_initial_options(second_state)
    second_start_gold = second_state.player.gold
    greed_before = _count_card(second_state.player.deck, CardId.GREED)

    second = second_event.choose(second_state, "second_chest")
    assert second.finished
    assert second_state.player.gold == second_start_gold + 333
    assert _count_card(second_state.player.deck, CardId.GREED) == greed_before + 1


def test_tablet_of_truth_decipher_chain_costs_max_hp_and_ends_with_no_upgradable_cards():
    run_state = _make_run_state(404)
    event = TabletOfTruth()
    event.generate_initial_options(run_state)

    assert run_state.player.upgradable_deck_cards()
    for _ in range(4):
        result = event.choose(run_state, "decipher")
        assert result.finished is False

    assert run_state.player.max_hp == 35
    fifth = event.choose(run_state, "decipher")
    assert fifth.finished
    assert run_state.player.max_hp == 1
    assert run_state.player.current_hp == 0
    assert run_state.is_over
    assert run_state.player.upgradable_deck_cards() == []


def test_tablet_of_truth_give_up_does_not_heal():
    run_state = _make_run_state(405)
    run_state.player.current_hp = 20
    event = TabletOfTruth()
    event.generate_initial_options(run_state)

    first = event.choose(run_state, "decipher")
    assert first.finished is False
    hp_before_give_up = run_state.player.current_hp
    give_up = event.choose(run_state, "give_up")
    assert give_up.finished
    assert run_state.player.current_hp == hp_before_give_up


def test_this_or_that_plain_and_ornate_apply_hp_gold_relic_and_clumsy():
    plain_state = _make_run_state(406)
    plain_event = ThisOrThat()
    plain_state.rng.up_front.next_int = lambda low, high: 55
    plain_event.generate_initial_options(plain_state)
    plain_start_hp = plain_state.player.current_hp
    plain_start_gold = plain_state.player.gold

    plain = plain_event.choose(plain_state, "plain")
    assert plain.finished
    assert plain_state.player.current_hp == plain_start_hp - 6
    assert plain_state.player.gold == plain_start_gold + 55

    ornate_state = _make_run_state(407)
    ornate_event = ThisOrThat()
    ornate_event.generate_initial_options(ornate_state)
    relics_before = len(ornate_state.player.relics)
    clumsy_before = _count_card(ornate_state.player.deck, CardId.CLUMSY)

    ornate = ornate_event.choose(ornate_state, "ornate")
    assert ornate.finished
    assert len(ornate_state.player.relics) == relics_before + 1
    assert _count_card(ornate_state.player.deck, CardId.CLUMSY) == clumsy_before + 1


def test_wellspring_bathe_pending_choice_removes_selected_card_and_adds_guilty():
    run_state = _make_run_state(408)
    event = Wellspring()
    guilty_before = _count_card(run_state.player.deck, CardId.GUILTY)
    deck_size_before = len(run_state.player.deck)

    bathe = event.choose(run_state, "bathe")
    assert bathe.finished is False
    assert event.pending_choice is not None
    assert event.pending_choice.allow_skip is False

    removed = event.pending_choice.options[0].card
    resolved = event.resolve_pending_choice(0)
    assert resolved.finished
    assert event.pending_choice is None
    assert len(run_state.player.deck) == deck_size_before
    assert all(card.instance_id != removed.instance_id for card in run_state.player.deck)
    assert _count_card(run_state.player.deck, CardId.GUILTY) == guilty_before + 1


def test_tinker_time_creates_configured_mad_science_card():
    run_state = _make_run_state(409)
    event = TinkerTime()

    initial = event.generate_initial_options(run_state)
    assert [option.option_id for option in initial] == ["choose_card_type"]

    first = event.choose(run_state, "choose_card_type")
    assert first.finished is False
    assert {option.option_id for option in first.next_options}
    chosen_type = first.next_options[0].option_id
    second = event.choose(run_state, chosen_type)
    assert second.finished is False
    rider = second.next_options[0].option_id
    final = event.choose(run_state, rider)
    assert final.finished
    mad_science = run_state.player.deck[-1]
    assert mad_science.card_id == CardId.MAD_SCIENCE
    assert mad_science.card_type.name == chosen_type.upper()
    assert mad_science.effect_vars["rider"] > 0
