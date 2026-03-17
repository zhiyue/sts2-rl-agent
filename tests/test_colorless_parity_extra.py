"""Additional parity tests for Colorless cards backed by decompiled semantics."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.colorless import (
    make_master_of_strategy,
    make_mimic,
    make_mind_blast,
    make_panache_card,
    make_volley,
)
from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.cards.ironclad_basic import make_strike_ironclad
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import PowerId
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle
from sts2_env.run.run_state import PlayerState


def _make_combat() -> CombatState:
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=123,
        character_id="Ironclad",
    )
    creature, ai = create_shrinker_beetle(Rng(123))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


class TestColorlessParityExtra:
    def test_master_of_strategy_draws_exact_number_of_cards(self):
        """Matches MasterOfStrategy.cs: draw N cards immediately."""
        combat = _make_combat()
        combat.hand = [make_master_of_strategy()]
        combat.draw_pile = [make_strike_ironclad() for _ in range(4)]

        assert combat.play_card(0)
        assert len(combat.hand) == 3
        assert len(combat.draw_pile) == 1

    def test_mind_blast_scales_damage_with_draw_pile_size(self):
        """Matches MindBlast.cs: deal damage equal to draw-pile-count scaling."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        combat.hand = [make_mind_blast()]
        combat.draw_pile = [make_strike_ironclad() for _ in range(7)]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - 7

    def test_panache_applies_panache_power_with_configured_amount(self):
        """Matches Panache.cs: apply PanachePower using configured damage amount."""
        combat = _make_combat()
        card = make_panache_card()
        combat.hand = [card]

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.PANACHE) == card.effect_vars["panache_damage"]

    def test_mimic_gains_block_from_target_ally_block(self):
        """Matches Mimic.cs: gain block based on selected ally's current block."""
        combat = _make_combat()
        ally_state = PlayerState(player_id=2, character_id="Ironclad", max_hp=60, current_hp=60)
        ally = combat.add_ally_player(ally_state)
        ally.gain_block(9)
        combat.hand = [make_mimic()]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert combat.player.block == 9

    def test_volley_spends_all_energy_and_hits_once_per_energy(self):
        """Matches Volley.cs: X-cost random-enemy attack repeated X times."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        combat.hand = [make_volley()]
        combat.energy = 3

        assert combat.play_card(0)
        assert combat.energy == 0
        assert enemy.current_hp == starting_hp - 30
