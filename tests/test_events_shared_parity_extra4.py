"""Focused parity tests for additional shared event behavior gaps."""

import sts2_env.events.shared  # noqa: F401

import pytest

from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.cards.status import make_decay, make_doubt
from sts2_env.core.enums import CardId
from sts2_env.events.shared import (
    AbyssalBaths,
    Amalgamator,
    AromaOfChaos,
    ByrdonisNest,
    Orobas,
    SapphireSeed,
    SelfHelpBook,
    WoodCarvings,
    ZenWeaver,
)
from sts2_env.run.run_state import RunState


def _make_run_state(seed: int = 501, character_id: str = "Ironclad") -> RunState:
    run_state = RunState(seed=seed, character_id=character_id)
    run_state.initialize_run()
    run_state.player.deck = create_ironclad_starter_deck()
    return run_state


def _count_card(deck, card_id: CardId) -> int:
    return sum(1 for card in deck if card.card_id == card_id)


def test_abyssal_baths_immerse_linger_and_exit_scale_hp_and_damage():
    run_state = _make_run_state(501)
    event = AbyssalBaths()
    event.generate_initial_options(run_state)

    start_hp = run_state.player.current_hp
    start_max_hp = run_state.player.max_hp

    first = event.choose(run_state, "immerse")
    assert first.finished is False
    assert {option.option_id for option in first.next_options} == {"linger", "exit_baths"}

    second = event.choose(run_state, "linger")
    assert second.finished is False

    exit_result = event.choose(run_state, "exit_baths")
    assert exit_result.finished
    assert run_state.player.max_hp == start_max_hp + 4
    assert run_state.player.current_hp == start_hp - 3


@pytest.mark.parametrize(
    ("option_id", "expected_card", "tag_fragment"),
    [
        ("combine_strikes", CardId.ULTIMATE_STRIKE, "STRIKE"),
        ("combine_defends", CardId.ULTIMATE_DEFEND, "DEFEND"),
    ],
)
def test_amalgamator_combines_two_basics_into_ultimate_card(option_id, expected_card, tag_fragment):
    run_state = _make_run_state(502)
    event = Amalgamator()

    before_size = len(run_state.player.deck)
    before_basics = sum(
        1
        for card in run_state.player.deck
        if tag_fragment in card.card_id.name and card.rarity.name == "BASIC"
    )

    first = event.choose(run_state, option_id)
    assert first.finished is False
    assert event.pending_choice is not None

    event.resolve_pending_choice(0)
    event.resolve_pending_choice(1)
    final = event.resolve_pending_choice(None)

    assert final.finished
    assert len(run_state.player.deck) == before_size - 1
    assert _count_card(run_state.player.deck, expected_card) == 1
    assert (
        sum(
            1
            for card in run_state.player.deck
            if tag_fragment in card.card_id.name and card.rarity.name == "BASIC"
        )
        == before_basics - 2
    )


def test_aroma_of_chaos_maintain_control_upgrades_selected_card():
    run_state = _make_run_state(503)
    event = AromaOfChaos()

    first = event.choose(run_state, "maintain_control")
    assert first.finished is False
    assert event.pending_choice is not None

    selected = event.pending_choice.options[0].card
    assert selected.upgraded is False
    final = event.resolve_pending_choice(0)

    assert final.finished
    assert selected.upgraded is True


def test_aroma_of_chaos_let_go_transforms_selected_card():
    run_state = _make_run_state(504)
    event = AromaOfChaos()

    first = event.choose(run_state, "let_go")
    assert first.finished is False
    assert event.pending_choice is not None

    selected = event.pending_choice.options[0].card
    original_id = selected.card_id
    final = event.resolve_pending_choice(0)

    assert final.finished
    assert selected.card_id != original_id


def test_byrdonis_nest_take_adds_egg_without_counting_as_event_pet():
    run_state = _make_run_state(505)
    event = ByrdonisNest()
    egg_before = _count_card(run_state.player.deck, CardId.BYRDONIS_EGG)

    assert event.is_allowed(run_state) is True
    take = event.choose(run_state, "take")
    assert take.finished
    assert _count_card(run_state.player.deck, CardId.BYRDONIS_EGG) == egg_before + 1
    assert event.is_allowed(run_state) is True


def test_sapphire_seed_eat_and_plant_apply_upgrade_and_sown():
    eat_state = _make_run_state(506)
    eat_state.player.current_hp = 60
    eat_event = SapphireSeed()
    hp_before = eat_state.player.current_hp

    eat = eat_event.choose(eat_state, "eat")
    assert eat.finished is False
    assert eat_state.player.current_hp == hp_before + 9
    assert eat_event.pending_choice is not None
    to_upgrade = eat_event.pending_choice.options[0].card
    eat_final = eat_event.resolve_pending_choice(0)

    assert eat_final.finished
    assert to_upgrade.upgraded is True

    plant_state = _make_run_state(507)
    plant_event = SapphireSeed()
    plant = plant_event.choose(plant_state, "plant")
    assert plant.finished is False
    assert plant_event.pending_choice is not None

    to_enchant = plant_event.pending_choice.options[0].card
    plant_final = plant_event.resolve_pending_choice(0)
    assert plant_final.finished
    assert to_enchant.enchantments.get("Sown") == 1


def test_self_help_book_enchants_attack_and_skill_cards():
    run_state = _make_run_state(508)
    event = SelfHelpBook()

    locked_state = _make_run_state(5071)
    locked_state.player.deck = [make_decay(), make_doubt()]
    locked_options = event.generate_initial_options(locked_state)
    assert [option.option_id for option in locked_options] == ["skip_book"]
    assert locked_options[0].enabled is False

    read_back = event.choose(run_state, "read_back")
    assert read_back.finished is False
    assert event.pending_choice is not None
    attack_card = event.pending_choice.options[0].card
    attack_final = event.resolve_pending_choice(0)
    assert attack_final.finished
    assert attack_card.enchantments.get("Sharp") == 2

    read_passage = event.choose(run_state, "read_passage")
    assert read_passage.finished is False
    assert event.pending_choice is not None
    skill_card = event.pending_choice.options[0].card
    skill_final = event.resolve_pending_choice(0)
    assert skill_final.finished
    assert skill_card.enchantments.get("Nimble") == 2


def test_wood_carvings_bird_snake_and_torus_mutate_deck_state():
    bird_state = _make_run_state(509)
    bird_event = WoodCarvings()
    bird = bird_event.choose(bird_state, "bird")
    assert bird.finished is False
    assert bird_event.pending_choice is not None
    bird_target = bird_event.pending_choice.options[0].card
    bird_final = bird_event.resolve_pending_choice(0)
    assert bird_final.finished
    assert bird_target.card_id == CardId.PECK

    snake_state = _make_run_state(510)
    snake_event = WoodCarvings()
    snake_options = snake_event.generate_initial_options(snake_state)
    snake_option = next(option for option in snake_options if option.option_id == "snake")
    assert snake_option.enabled is True
    snake = snake_event.choose(snake_state, "snake")
    assert snake.finished is False
    assert snake_event.pending_choice is not None
    snake_target = snake_event.pending_choice.options[0].card
    snake_final = snake_event.resolve_pending_choice(0)
    assert snake_final.finished
    assert snake_target.enchantments.get("Slither") == 1

    torus_state = _make_run_state(511)
    torus_event = WoodCarvings()
    torus = torus_event.choose(torus_state, "torus")
    assert torus.finished is False
    assert torus_event.pending_choice is not None
    torus_target = torus_event.pending_choice.options[0].card
    torus_final = torus_event.resolve_pending_choice(0)
    assert torus_final.finished
    assert torus_target.card_id == CardId.TORIC_TOUGHNESS


def test_zen_weaver_applies_gold_card_gain_and_card_removal_costs():
    options_state = _make_run_state(512)
    options_state.player.gold = 200
    options_event = ZenWeaver()
    options = options_event.generate_initial_options(options_state)
    enabled_by_id = {option.option_id: option.enabled for option in options}
    assert enabled_by_id == {
        "breathing": True,
        "emotional": True,
        "acupuncture": False,
    }

    breathing_state = _make_run_state(513)
    breathing_state.player.gold = 200
    breathing_event = ZenWeaver()
    breathing_gold = breathing_state.player.gold
    enlightenment_before = _count_card(breathing_state.player.deck, CardId.ENLIGHTENMENT)
    breathing = breathing_event.choose(breathing_state, "breathing")
    assert breathing.finished
    assert breathing_state.player.gold == breathing_gold - 50
    assert _count_card(breathing_state.player.deck, CardId.ENLIGHTENMENT) == enlightenment_before + 2

    emotional_state = _make_run_state(514)
    emotional_state.player.gold = 200
    emotional_event = ZenWeaver()
    emotional_gold = emotional_state.player.gold
    emotional_size = len(emotional_state.player.deck)
    emotional = emotional_event.choose(emotional_state, "emotional")
    assert emotional.finished is False
    emotional_final = emotional_event.resolve_pending_choice(0)
    assert emotional_final.finished
    assert emotional_state.player.gold == emotional_gold - 125
    assert len(emotional_state.player.deck) == emotional_size - 1

    acupuncture_state = _make_run_state(515)
    acupuncture_state.player.gold = 300
    acupuncture_event = ZenWeaver()
    acupuncture_gold = acupuncture_state.player.gold
    acupuncture_size = len(acupuncture_state.player.deck)
    acupuncture = acupuncture_event.choose(acupuncture_state, "acupuncture")
    assert acupuncture.finished is False
    acupuncture_event.resolve_pending_choice(0)
    acupuncture_event.resolve_pending_choice(1)
    acupuncture_final = acupuncture_event.resolve_pending_choice(None)
    assert acupuncture_final.finished
    assert acupuncture_state.player.gold == acupuncture_gold - 250
    assert len(acupuncture_state.player.deck) == acupuncture_size - 2


def test_orobas_assigns_off_character_for_sea_glass_when_selected():
    run_state = _make_run_state(516)
    event = Orobas()

    run_state.rng.up_front.next_float = lambda: 0.9

    def choose(seq):
        first = seq[0]
        if isinstance(first, str):
            return seq[0]
        if isinstance(first, tuple):
            for entry in seq:
                if entry[0] == "SEA_GLASS":
                    return entry
            return seq[0]
        return seq[0]

    run_state.rng.up_front.choice = choose
    options = event.generate_initial_options(run_state)
    assert options

    result = event.choose(run_state, "relic_1")
    assert result.finished
    sea_glass = run_state.player.relic_objects[-1]
    assert sea_glass.relic_id.name == "SEA_GLASS"
    assert sea_glass._character_id != run_state.player.character_id


def test_orobas_locks_third_option_when_no_setup_relics_apply():
    run_state = _make_run_state(517, character_id="Ironclad")
    event = Orobas()
    run_state.player.relics = []
    run_state.player.deck = [card for card in run_state.player.deck if card.card_id != CardId.BASH]

    options = event.generate_initial_options(run_state)
    third = next(option for option in options if option.option_id == "relic_3")
    assert third.enabled is False
