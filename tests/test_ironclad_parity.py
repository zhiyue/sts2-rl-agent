"""Focused Ironclad parity tests backed by decompiled card models."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.ironclad import (
    create_ironclad_starter_deck,
    make_armaments,
    make_burning_pact,
    make_headbutt,
    make_true_grit,
    make_whirlwind,
)
from sts2_env.cards.ironclad_basic import make_bash, make_defend_ironclad, make_strike_ironclad
from sts2_env.core.combat import CombatState
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle


def _make_combat(*, enemies: int = 1) -> CombatState:
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=42,
        character_id="Ironclad",
    )
    for i in range(enemies):
        creature, ai = create_shrinker_beetle(Rng(42 + i))
        combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


class TestIroncladParity:
    def test_armaments_plus_upgrades_all_unupgraded_hand_cards(self):
        """Matches Armaments.cs: gain block and, when upgraded, upgrade every non-upgraded hand card."""
        combat = _make_combat()
        strike = make_strike_ironclad()
        defend = make_defend_ironclad()
        combat.hand = [make_armaments(upgraded=True), strike, defend]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.block == 5
        assert combat.pending_choice is None
        assert strike.upgraded is True
        assert defend.upgraded is True

    def test_burning_pact_exhausts_selected_card_then_draws(self):
        """Matches BurningPact.cs: select one hand card to exhaust, then draw the configured amount."""
        combat = _make_combat()
        strike = make_strike_ironclad()
        defend = make_defend_ironclad()
        draw_a = make_bash()
        draw_b = make_strike_ironclad()
        combat.hand = [make_burning_pact(), strike, defend]
        combat.draw_pile = [draw_a, draw_b]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert [option.card for option in combat.pending_choice.options] == [strike, defend]

        assert combat.resolve_pending_choice(1)
        assert combat.pending_choice is None
        assert defend in combat.exhaust_pile
        assert draw_a in combat.hand
        assert draw_b in combat.hand
        assert len(combat.hand) == 3

    def test_headbutt_puts_selected_discard_card_on_top_of_draw(self):
        """Matches Headbutt.cs: attack, then choose one discard card and place it on top of draw pile."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        strike = make_strike_ironclad()
        defend = make_defend_ironclad()
        existing_top = make_bash()
        combat.hand = [make_headbutt()]
        combat.discard_pile = [strike, defend]
        combat.draw_pile = [existing_top]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - 9
        assert combat.pending_choice is not None
        assert [option.card for option in combat.pending_choice.options] == [strike, defend]

        assert combat.resolve_pending_choice(1)
        assert combat.pending_choice is None
        assert combat.draw_pile[0] is defend
        assert combat.draw_pile[1] is existing_top
        assert defend not in combat.discard_pile
        assert strike in combat.discard_pile

    def test_true_grit_plus_exhausts_the_selected_hand_card(self):
        """Matches TrueGrit.cs: gain block, then upgraded version exhausts a selected hand card."""
        combat = _make_combat()
        strike = make_strike_ironclad()
        defend = make_defend_ironclad()
        combat.hand = [make_true_grit(upgraded=True), strike, defend]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.block == 9
        assert combat.pending_choice is not None
        assert [option.card for option in combat.pending_choice.options] == [strike, defend]

        assert combat.resolve_pending_choice(1)
        assert combat.pending_choice is None
        assert defend in combat.exhaust_pile
        assert strike in combat.hand

    def test_whirlwind_uses_all_energy_and_hits_all_enemies_per_energy(self):
        """Matches Whirlwind.cs: consume all current energy and deal repeated all-enemy hits."""
        combat = _make_combat(enemies=2)
        enemy_a, enemy_b = combat.enemies
        start_a = enemy_a.current_hp
        start_b = enemy_b.current_hp
        combat.hand = [make_whirlwind()]
        combat.energy = 3

        assert combat.play_card(0)
        assert combat.energy == 0
        assert enemy_a.current_hp == start_a - 15
        assert enemy_b.current_hp == start_b - 15
