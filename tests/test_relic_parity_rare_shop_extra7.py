"""Focused direct parity coverage for additional rare/shop/event relic hooks."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.defect import create_defect_starter_deck
from sts2_env.cards.ironclad import create_ironclad_starter_deck, make_inflame
from sts2_env.cards.ironclad_basic import make_strike_ironclad
from sts2_env.cards.necrobinder import create_necrobinder_starter_deck
from sts2_env.cards.silent import create_silent_starter_deck
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import CombatSide, PowerId
from sts2_env.core.hooks import fire_before_turn_end
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle


def _with_owner(cards: list, owner):
    for card in cards:
        card.owner = owner
    return cards


def _starter_deck(character_id: str):
    if character_id == "Defect":
        return create_defect_starter_deck()
    if character_id == "Necrobinder":
        return create_necrobinder_starter_deck()
    if character_id == "Silent":
        return create_silent_starter_deck()
    return create_ironclad_starter_deck()


def _make_combat(
    *,
    character_id: str,
    relics: list[str] | None = None,
    seed: int = 1201,
    start: bool = True,
) -> CombatState:
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=_starter_deck(character_id),
        rng_seed=seed,
        character_id=character_id,
        relics=relics or [],
    )
    creature, ai = create_shrinker_beetle(Rng(seed))
    combat.add_enemy(creature, ai)
    if start:
        combat.start_combat()
    return combat


def test_big_hat_generates_two_ethereal_cards_on_round_one():
    combat = _make_combat(character_id="Necrobinder", relics=["BigHat"], seed=1201)
    player = combat.player

    assert combat.count_generated_cards_this_combat(player) == 2
    assert sum(1 for card in combat.hand if card.is_ethereal) >= 2


def test_bread_skips_energy_bonus_on_round_one_then_grants_it_after():
    combat = _make_combat(character_id="Ironclad", relics=["Bread"], seed=1202)

    assert combat.round_number == 1
    assert combat.max_energy == 3
    assert combat.energy == 1

    combat.end_player_turn()
    assert combat.round_number == 2
    assert combat.max_energy == 4
    assert combat.energy == 4


def test_chandelier_grants_three_energy_only_on_round_three():
    combat = _make_combat(character_id="Ironclad", relics=["Chandelier"], seed=1203)
    assert combat.energy == 3

    combat.end_player_turn()
    assert combat.round_number == 2
    assert combat.energy == 3

    combat.end_player_turn()
    assert combat.round_number == 3
    assert combat.energy == 6


def test_history_course_replays_last_attack_or_skill_from_previous_turn_only():
    combat = _make_combat(character_id="Ironclad", relics=["HistoryCourse"], seed=1204)
    player = combat.player
    enemy = combat.enemies[0]
    enemy.max_hp = 300
    enemy.current_hp = 300

    combat.hand = _with_owner([make_strike_ironclad()], player)
    combat.energy = 1
    assert combat.play_card(0, 0)
    assert combat.count_cards_played_this_turn(player) == 1

    combat.end_player_turn()
    assert combat.round_number == 2
    assert combat.count_cards_played_this_turn(player) == 1
    assert combat.count_cards_played_this_combat(player) == 2

    combat.end_player_turn()
    assert combat.round_number == 3
    assert combat.count_cards_played_this_turn(player) == 0


def test_ninja_scroll_adds_three_shivs_before_opening_draw():
    combat = _make_combat(character_id="Silent", relics=["NinjaScroll"], seed=1205)

    assert len(combat.hand) == 8
    assert sum(1 for card in combat.hand if card.is_shiv) == 3


def test_snecko_eye_applies_confused_and_draws_two_extra_cards():
    combat = _make_combat(character_id="Defect", relics=["SneckoEye"], seed=1206)

    assert combat.player.get_power_amount(PowerId.CONFUSED) == 1
    assert len(combat.hand) == 7


def test_spiked_gauntlets_adds_one_energy_and_charges_plus_one_for_power_cards():
    combat = _make_combat(character_id="Ironclad", relics=["SpikedGauntlets"], seed=1207)
    player = combat.player
    enemy = combat.enemies[0]
    enemy.max_hp = 300
    enemy.current_hp = 300

    assert combat.max_energy == 4
    assert combat.energy == 4

    combat.hand = _with_owner([make_inflame()], player)
    combat.energy = 1
    assert not combat.play_card(0)
    assert combat.energy == 1

    power_card = make_inflame()
    power_card.owner = player
    combat.hand = [power_card]
    combat.energy = 2
    assert combat.play_card(0)
    assert combat.energy == 0
    assert power_card.energy_spent == 2

    combat.hand = _with_owner([make_strike_ironclad()], player)
    combat.energy = 1
    assert combat.play_card(0, 0)
    assert combat.energy == 0


def test_stone_calendar_deals_damage_to_all_enemies_at_end_of_turn_seven_only():
    combat = _make_combat(character_id="Ironclad", relics=["StoneCalendar"], seed=1208)
    player = combat.player
    enemy = combat.enemies[0]
    enemy.max_hp = 200
    enemy.current_hp = 200

    combat.round_number = 6
    fire_before_turn_end(CombatSide.PLAYER, combat)
    assert enemy.current_hp == 200

    combat.round_number = 7
    fire_before_turn_end(CombatSide.PLAYER, combat)
    assert enemy.current_hp == 148
