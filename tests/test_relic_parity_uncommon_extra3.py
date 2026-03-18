"""Focused uncommon relic parity tests for remaining high-signal hook behavior."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.cards.ironclad_basic import make_bash, make_defend_ironclad, make_strike_ironclad
from sts2_env.cards.status import make_void
from sts2_env.core.combat import CombatState
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle, create_twig_slime_s


def _with_owner(cards: list, owner):
    for card in cards:
        card.owner = owner
    return cards


def _make_ironclad_combat(
    relics: list[str] | None = None,
    *,
    seed: int = 801,
    enemies: int = 1,
) -> CombatState:
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=seed,
        character_id="Ironclad",
        relics=relics or [],
    )
    for i in range(enemies):
        if i == 0:
            creature, ai = create_shrinker_beetle(Rng(seed + i))
        else:
            creature, ai = create_twig_slime_s(Rng(seed + i))
        combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


class TestRelicParityUncommonExtra3:
    def test_horn_cleat_grants_block_only_on_round_two_block_clear(self):
        """Matches HornCleat.cs: trigger once when owner's block is cleared on round 2."""
        combat = _make_ironclad_combat(["HornCleat"], seed=801)
        assert combat.round_number == 1
        assert combat.player.block == 0

        combat.end_player_turn()

        assert combat.round_number == 2
        assert combat.player.block == 14

        combat.player.block = 0
        combat.end_player_turn()

        assert combat.round_number == 3
        assert combat.player.block == 0

    def test_kusarigama_counts_attacks_per_turn_and_resets_on_new_turn(self):
        """Matches Kusarigama.cs: every 3 attacks this turn deals 6 to a random enemy."""
        combat = _make_ironclad_combat(["Kusarigama"], seed=802)
        enemy = combat.enemies[0]
        enemy.max_hp = 400
        enemy.current_hp = 400

        combat.hand = _with_owner([make_strike_ironclad(), make_strike_ironclad()], combat.player)
        combat.energy = 2
        start_hp = enemy.current_hp

        assert combat.play_card(0, 0)
        assert combat.play_card(0, 0)
        assert enemy.current_hp == start_hp - 12

        combat.end_player_turn()
        assert combat.round_number == 2

        combat.hand = _with_owner([make_strike_ironclad()], combat.player)
        combat.energy = 1
        hp_before = enemy.current_hp
        assert combat.play_card(0, 0)
        first_hit = hp_before - enemy.current_hp

        combat.hand = _with_owner([make_strike_ironclad(), make_strike_ironclad()], combat.player)
        combat.energy = 2
        hp_before = enemy.current_hp
        assert combat.play_card(0, 0)
        second_hit = hp_before - enemy.current_hp
        hp_before = enemy.current_hp
        assert combat.play_card(0, 0)
        third_hit = hp_before - enemy.current_hp

        assert second_hit == first_hit
        assert third_hit == first_hit + 6

    def test_letter_opener_hits_all_enemies_on_every_third_skill(self):
        """Matches LetterOpener.cs: every 3 skills in a turn deals 5 to all enemies."""
        combat = _make_ironclad_combat(["LetterOpener"], seed=803, enemies=2)
        enemy_a, enemy_b = combat.enemies
        for enemy in combat.enemies:
            enemy.max_hp = 150
            enemy.current_hp = 150

        combat.hand = _with_owner(
            [make_defend_ironclad(), make_defend_ironclad(), make_defend_ironclad()],
            combat.player,
        )
        combat.energy = 3
        hp_a = enemy_a.current_hp
        hp_b = enemy_b.current_hp

        assert combat.play_card(0)
        assert combat.play_card(0)
        assert enemy_a.current_hp == hp_a
        assert enemy_b.current_hp == hp_b

        assert combat.play_card(0)
        assert enemy_a.current_hp == hp_a - 5
        assert enemy_b.current_hp == hp_b - 5

    def test_nunchaku_grants_energy_on_tenth_attack_across_turns(self):
        """Matches Nunchaku.cs: attack counter persists and grants +1 energy every 10 attacks."""
        combat = _make_ironclad_combat(["Nunchaku"], seed=804)
        enemy = combat.enemies[0]
        enemy.max_hp = 500
        enemy.current_hp = 500

        combat.hand = _with_owner([make_strike_ironclad() for _ in range(9)], combat.player)
        combat.energy = 9
        for _ in range(9):
            assert combat.play_card(0, 0)
        assert combat.energy == 0

        combat.end_player_turn()
        assert combat.round_number == 2

        combat.hand = _with_owner([make_strike_ironclad()], combat.player)
        combat.energy = 3
        assert combat.play_card(0, 0)
        assert combat.energy == 3

    def test_ornamental_fan_counts_attacks_per_turn_then_gains_block(self):
        """Matches OrnamentalFan.cs: every 3 attacks this turn grants 4 block."""
        combat = _make_ironclad_combat(["OrnamentalFan"], seed=805)

        combat.hand = _with_owner([make_strike_ironclad(), make_strike_ironclad()], combat.player)
        combat.energy = 2
        assert combat.play_card(0, 0)
        assert combat.play_card(0, 0)
        assert combat.player.block == 0

        combat.end_player_turn()
        assert combat.round_number == 2

        combat.player.block = 0
        combat.hand = _with_owner([make_strike_ironclad()], combat.player)
        combat.energy = 1
        assert combat.play_card(0, 0)
        assert combat.player.block == 0

        combat.hand = _with_owner([make_strike_ironclad(), make_strike_ironclad()], combat.player)
        combat.energy = 2
        assert combat.play_card(0, 0)
        assert combat.play_card(0, 0)
        assert combat.player.block == 4

    def test_joss_paper_draws_on_fifth_non_ethereal_exhaust(self):
        """Matches JossPaper.cs: every 5 exhausted cards draws 1."""
        combat = _make_ironclad_combat(["JossPaper"], seed=806)
        marker = make_bash()
        marker.owner = combat.player
        combat.draw_pile = [marker]
        combat.hand = _with_owner([make_strike_ironclad() for _ in range(5)], combat.player)

        for _ in range(4):
            combat.exhaust_card(combat.hand[0])
        assert marker not in combat.hand

        combat.exhaust_card(combat.hand[0])
        assert marker in combat.hand

    def test_joss_paper_defers_ethereal_exhaust_draw_until_after_turn_end(self):
        """Matches JossPaper.cs: ethereal exhausts count at turn end, then draw resolves."""
        combat = _make_ironclad_combat(["JossPaper"], seed=807)
        marker = make_bash()
        marker.owner = combat.player
        filler = _with_owner([make_strike_ironclad() for _ in range(5)], combat.player)
        combat.draw_pile = [marker, *filler]
        combat.hand = _with_owner([make_void() for _ in range(5)], combat.player)
        combat.discard_pile = []

        combat.end_player_turn()

        assert combat.round_number == 2
        assert marker in combat.hand
