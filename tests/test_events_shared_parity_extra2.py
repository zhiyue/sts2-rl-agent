"""Focused parity tests for additional uncovered shared events."""

import sts2_env.events.shared  # noqa: F401

from sts2_env.cards.factory import create_card
from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.cards.silent import make_backstab
from sts2_env.cards.status import make_decay, make_doubt
from sts2_env.core.enums import CardId, CardType
from sts2_env.events.shared import (
    DrowningBeacon,
    GraveOfTheForgotten,
    HungryForMushrooms,
    InfestedAutomaton,
    SunkenStatue,
)
from sts2_env.run.reward_objects import PotionReward
from sts2_env.run.run_state import RunState


def _make_run_state(seed: int = 301) -> RunState:
    run_state = RunState(seed=seed, character_id="Ironclad")
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()
    return run_state


def test_drowning_beacon_bottle_returns_glowwater_reward_and_climb_costs_max_hp():
    run_state = _make_run_state(301)
    event = DrowningBeacon()
    start_max_hp = run_state.player.max_hp
    start_relics = len(run_state.player.relics)

    bottle = event.choose(run_state, "bottle")
    rewards = bottle.rewards["reward_objects"]
    assert len(rewards) == 1
    assert isinstance(rewards[0], PotionReward)
    assert rewards[0].potion_id == "GlowwaterPotion"

    climb = event.choose(run_state, "climb")
    assert climb.finished
    assert run_state.player.max_hp == start_max_hp - 13
    assert len(run_state.player.relics) == start_relics + 1
    assert run_state.player.relics[-1] == "FRESNEL_LENS"


def test_grave_of_the_forgotten_confront_adds_decay_and_souls_power_enchant():
    run_state = _make_run_state(302)
    card = make_backstab()
    run_state.player.deck.append(card)
    event = GraveOfTheForgotten()
    options = event.generate_initial_options(run_state)
    assert [option.enabled for option in options] == [True, True]
    decay_count_before = sum(1 for c in run_state.player.deck if c.card_id == CardId.DECAY)

    result = event.choose(run_state, "confront")
    assert result.finished is False
    assert event.pending_choice is not None
    assert card in [option.card for option in event.pending_choice.options]

    resolved = event.resolve_pending_choice(0)
    assert resolved.finished
    assert event.pending_choice is None
    assert card.enchantments.get("SoulsPower") == 1
    assert sum(1 for c in run_state.player.deck if c.card_id == CardId.DECAY) == decay_count_before + 1

    accept = event.choose(run_state, "accept")
    assert accept.finished
    assert run_state.player.relics[-1] == "FORGOTTEN_SOUL"

    locked_state = _make_run_state(3021)
    locked_state.player.deck = [make_decay(), make_doubt()]
    locked_event = GraveOfTheForgotten()
    locked_options = locked_event.generate_initial_options(locked_state)
    assert [option.enabled for option in locked_options] == [False, True]


def test_hungry_for_mushrooms_big_and_fragrant_apply_expected_hp_and_relics():
    run_state = _make_run_state(303)
    event = HungryForMushrooms()
    start_hp = run_state.player.current_hp
    start_relics = len(run_state.player.relics)

    big = event.choose(run_state, "big")
    assert big.finished
    assert run_state.player.current_hp == start_hp + 20
    assert run_state.player.relics[-1] == "BIG_MUSHROOM"
    hp_after_big = run_state.player.current_hp

    fragrant = event.choose(run_state, "fragrant")
    assert fragrant.finished
    # Fragrant Mushroom relic applies the 15 HP loss on obtain.
    assert run_state.player.current_hp == hp_after_big - 15
    assert run_state.player.relics[-1] == "FRAGRANT_MUSHROOM"
    assert len(run_state.player.relics) == start_relics + 2


def test_infested_automaton_study_adds_power_and_touch_core_adds_zero_cost_non_x():
    run_state = _make_run_state(304)
    event = InfestedAutomaton()
    deck_before = len(run_state.player.deck)

    study = event.choose(run_state, "study")
    assert study.finished
    assert len(run_state.player.deck) == deck_before + 1
    assert run_state.player.deck[-1].card_type == CardType.POWER

    touch_core = event.choose(run_state, "touch_core")
    assert touch_core.finished
    assert len(run_state.player.deck) == deck_before + 2
    added = run_state.player.deck[-1]
    assert added.cost == 0
    assert added.has_energy_cost_x is False


def test_sunken_statue_grab_sword_and_dive_apply_relic_gold_and_hp_changes():
    run_state = _make_run_state(305)
    event = SunkenStatue()
    options = event.generate_initial_options(run_state)
    dive_option = next(option for option in options if option.option_id == "dive")
    expected_gold = int(dive_option.description.split()[1])
    start_gold = run_state.player.gold
    start_hp = run_state.player.current_hp
    start_relics = len(run_state.player.relics)

    grab = event.choose(run_state, "grab_sword")
    assert grab.finished
    assert len(run_state.player.relics) == start_relics + 1
    assert run_state.player.relics[-1] == "SWORD_OF_STONE"

    dive = event.choose(run_state, "dive")
    assert dive.finished
    assert run_state.player.gold == start_gold + expected_gold
    assert run_state.player.current_hp == start_hp - 7
