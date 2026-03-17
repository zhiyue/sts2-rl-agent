"""Additional focused Regent parity tests for currently uncovered cards."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.regent import (
    create_regent_starter_deck,
    make_gather_light,
    make_glow,
    make_make_it_so,
    make_radiate,
    make_stardust,
)
from sts2_env.cards.ironclad_basic import make_strike_ironclad
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import CardId
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle


def _make_combat() -> CombatState:
    combat = CombatState(
        player_hp=60,
        player_max_hp=60,
        deck=create_regent_starter_deck(),
        rng_seed=42,
        character_id="Regent",
    )
    creature, ai = create_shrinker_beetle(Rng(42))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


def _play_card_by_id(combat: CombatState, card_id: CardId, target_index: int | None = None) -> bool:
    for index, card in enumerate(combat.hand):
        if card.card_id == card_id:
            return combat.play_card(index, target_index)
    return False


class TestRegentParityExtra:
    def test_gather_light_gains_block_and_stars(self):
        """Matches GatherLight.cs: gain block, then gain stars."""
        combat = _make_combat()
        combat.hand = [make_gather_light()]
        combat.energy = 1
        starting_stars = combat.stars

        assert combat.play_card(0)
        assert combat.player.block == 7
        assert combat.stars == starting_stars + 1

    def test_glow_gains_stars_and_draws_cards(self):
        """Matches Glow.cs: gain stars, then draw the configured number of cards."""
        combat = _make_combat()
        first_draw = make_strike_ironclad()
        second_draw = make_strike_ironclad()
        combat.hand = [make_glow(upgraded=True)]
        combat.draw_pile = [first_draw, second_draw]
        combat.energy = 1
        starting_stars = combat.stars

        assert combat.play_card(0)
        assert combat.stars == starting_stars + 2
        assert first_draw in combat.hand
        assert second_draw in combat.hand

    def test_make_it_so_returns_from_discard_after_every_three_skills_played(self):
        """Matches MakeItSo.cs: return this card from discard each third Skill played."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        combat.hand = [
            make_make_it_so(),
            make_gather_light(),
            make_glow(),
            make_gather_light(),
        ]
        combat.draw_pile = [make_strike_ironclad(), make_strike_ironclad()]
        combat.energy = 10

        assert _play_card_by_id(combat, CardId.MAKE_IT_SO, 0)
        assert combat.count_cards_played_this_turn(combat.player) == 1
        assert any(card.card_id == CardId.MAKE_IT_SO for card in combat.discard_pile)
        assert enemy.current_hp < enemy.max_hp

        assert _play_card_by_id(combat, CardId.GATHER_LIGHT)
        assert _play_card_by_id(combat, CardId.GLOW)
        assert _play_card_by_id(combat, CardId.GATHER_LIGHT)

        assert combat.count_cards_played_this_turn(combat.player, card_type=make_glow().card_type) >= 3
        assert any(card.card_id == CardId.MAKE_IT_SO for card in combat.hand)

    def test_radiate_hits_all_enemies_once_per_star_gained_this_turn(self):
        """Matches Radiate.cs: AoE hit count is stars gained this turn."""
        combat = _make_combat()
        enemy_a = combat.enemies[0]
        enemy_b, ai_b = create_shrinker_beetle(Rng(99))
        combat.add_enemy(enemy_b, ai_b)
        starting_hp_a = enemy_a.current_hp
        starting_hp_b = enemy_b.current_hp
        combat.hand = [make_gather_light(), make_glow(upgraded=True), make_radiate(upgraded=True)]
        combat.energy = 5
        combat.draw_pile = [make_strike_ironclad(), make_strike_ironclad()]

        assert _play_card_by_id(combat, CardId.GATHER_LIGHT)
        assert _play_card_by_id(combat, CardId.GLOW)
        assert _play_card_by_id(combat, CardId.RADIATE)

        # Gather Light (1) + upgraded Glow (2) => 3 Radiate hits; upgraded Radiate damage is 4.
        assert enemy_a.current_hp == starting_hp_a - 12
        assert enemy_b.current_hp == starting_hp_b - 12

    def test_stardust_spends_all_stars_and_hits_that_many_times(self):
        """Matches Stardust.cs: spend all stars, then perform that many random hits."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        combat.hand = [make_stardust()]
        combat.energy = 3
        combat.gain_stars(combat.player, 3)

        assert combat.play_card(0, 0)
        assert combat.stars == 0
        assert enemy.current_hp == starting_hp - 15
