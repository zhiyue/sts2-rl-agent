"""Additional high-signal parity tests for remaining Colorless cards."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.colorless import (
    make_flash_of_steel,
    make_jack_of_all_trades,
    make_mayhem_card,
    make_omnislice,
    make_panache_card,
    make_rally,
    make_scrawl,
    make_the_bomb_card,
    make_ultimate_strike,
)
from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.cards.ironclad_basic import make_strike_ironclad
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import CardId, PowerId
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle
from sts2_env.run.run_state import PlayerState


def _make_combat(*, extra_enemies: int = 0) -> CombatState:
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=777,
        character_id="Ironclad",
    )
    creature, ai = create_shrinker_beetle(Rng(777))
    combat.add_enemy(creature, ai)
    for i in range(extra_enemies):
        extra_creature, extra_ai = create_shrinker_beetle(Rng(900 + i))
        combat.add_enemy(extra_creature, extra_ai)
    combat.start_combat()
    return combat


class TestColorlessParityExtra2:
    def test_flash_of_steel_deals_damage_then_draws_one(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        drawn = make_strike_ironclad()
        combat.hand = [make_flash_of_steel()]
        combat.draw_pile = [drawn]

        assert combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - 5
        assert combat.hand == [drawn]

    def test_jack_of_all_trades_upgraded_adds_two_distinct_non_self_colorless_cards(self):
        combat = _make_combat()
        combat.hand = [make_jack_of_all_trades(upgraded=True)]
        combat.energy = 0

        assert combat.play_card(0)
        assert len(combat.hand) == 2
        assert all(card.card_id != CardId.JACK_OF_ALL_TRADES for card in combat.hand)
        assert len({card.card_id for card in combat.hand}) == 2

    def test_mayhem_autoplays_top_draw_card_at_next_turn_start(self):
        combat = _make_combat()
        top_power = make_panache_card()
        fillers = [make_strike_ironclad() for _ in range(6)]
        combat.hand = [make_mayhem_card()]
        combat.draw_pile = [top_power, *fillers]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.MAYHEM) == 1

        combat.end_player_turn()

        assert combat.player.get_power_amount(PowerId.PANACHE) == top_power.effect_vars["panache_damage"]
        assert len(combat.hand) == 5

    def test_omnislice_splashes_total_hit_damage_to_other_enemies(self):
        combat = _make_combat(extra_enemies=1)
        primary = combat.enemies[0]
        secondary = combat.enemies[1]
        primary_start = primary.current_hp
        secondary_start = secondary.current_hp
        primary.block = 3
        combat.hand = [make_omnislice()]
        combat.energy = 0

        assert combat.play_card(0, 0)
        assert primary.current_hp == primary_start - 5
        assert secondary.current_hp == secondary_start - 8

    def test_rally_grants_block_to_other_allied_players_not_owner(self):
        combat = _make_combat()
        ally_one = combat.add_ally_player(
            PlayerState(player_id=2, character_id="Ironclad", max_hp=60, current_hp=60)
        )
        ally_two = combat.add_ally_player(
            PlayerState(player_id=3, character_id="Ironclad", max_hp=60, current_hp=60)
        )
        combat.hand = [make_rally()]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.player.block == 0
        assert ally_one.block == 12
        assert ally_two.block == 12

    def test_scrawl_draws_until_hand_is_full(self):
        combat = _make_combat()
        keep_a = make_strike_ironclad()
        keep_b = make_strike_ironclad()
        keep_c = make_strike_ironclad()
        draws = [make_strike_ironclad() for _ in range(8)]
        combat.hand = [make_scrawl(), keep_a, keep_b, keep_c]
        combat.draw_pile = list(draws)
        combat.energy = 1

        assert combat.play_card(0)
        assert len(combat.hand) == 10
        assert len(combat.draw_pile) == 1

    def test_the_bomb_counts_down_then_deals_configured_damage_to_all_enemies(self):
        combat = _make_combat(extra_enemies=1)
        first_enemy = combat.enemies[0]
        second_enemy = combat.enemies[1]
        first_enemy.max_hp = 120
        first_enemy.current_hp = 120
        second_enemy.max_hp = 120
        second_enemy.current_hp = 120
        combat.hand = [make_the_bomb_card()]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.THE_BOMB) == 3

        first_enemy.block = 0
        second_enemy.block = 0
        combat.end_player_turn()
        assert combat.player.get_power_amount(PowerId.THE_BOMB) == 2
        assert first_enemy.current_hp == 120
        assert second_enemy.current_hp == 120

        first_enemy.block = 0
        second_enemy.block = 0
        combat.end_player_turn()
        assert combat.player.get_power_amount(PowerId.THE_BOMB) == 1
        assert first_enemy.current_hp == 120
        assert second_enemy.current_hp == 120

        first_enemy.block = 0
        second_enemy.block = 0
        combat.end_player_turn()
        assert combat.player.get_power_amount(PowerId.THE_BOMB) == 0
        assert first_enemy.current_hp == 80
        assert second_enemy.current_hp == 80

    def test_ultimate_strike_and_upgrade_damage_values_match_reference(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.max_hp = 200
        enemy.current_hp = 200
        combat.hand = [make_ultimate_strike(), make_ultimate_strike(upgraded=True)]
        combat.energy = 2

        assert combat.play_card(0, 0)
        assert enemy.current_hp == 186

        assert combat.play_card(0, 0)
        assert enemy.current_hp == 166
