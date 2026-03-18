"""Additional focused Regent parity tests for high-signal remaining cards."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.factory import create_card
from sts2_env.cards.regent import create_regent_starter_deck
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import CardId, PowerId
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


class TestRegentParityExtra3:
    def test_astral_pulse_requires_stars_then_spends_them_for_aoe(self):
        """Matches AstralPulse.cs: StarCost 3, then damage all enemies."""
        combat = _make_combat()
        enemy_a = combat.enemies[0]
        enemy_b, ai_b = create_shrinker_beetle(Rng(99))
        combat.add_enemy(enemy_b, ai_b)
        enemy_a.current_hp = enemy_a.max_hp = 100
        enemy_b.current_hp = enemy_b.max_hp = 100
        card = create_card(CardId.ASTRAL_PULSE)
        combat.hand = [card]
        combat.energy = 0

        assert card.star_cost == 3
        assert combat.can_play_card(card) is False
        combat.gain_stars(combat.player, 3)
        assert combat.can_play_card(card) is True
        assert combat.play_card(0)
        assert combat.stars == 0
        assert enemy_a.current_hp == 86
        assert enemy_b.current_hp == 86

    def test_guiding_star_spends_star_cost_and_applies_draw_next_turn(self):
        """Matches GuidingStar.cs: StarCost 2, damage target, apply DrawCardsNextTurn."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        card = create_card(CardId.GUIDING_STAR, upgraded=True)
        combat.hand = [card]
        combat.energy = 1

        assert card.star_cost == 2
        assert combat.can_play_card(card) is False
        combat.gain_stars(combat.player, 2)
        assert combat.play_card(0, 0)
        assert combat.stars == 0
        assert enemy.current_hp == 87
        assert combat.player.get_power_amount(PowerId.DRAW_CARDS_NEXT_TURN) == 3

    def test_alignment_spends_stars_and_grants_energy(self):
        """Matches Alignment.cs: StarCost 2 and gain upgraded Energy."""
        combat = _make_combat()
        card = create_card(CardId.ALIGNMENT, upgraded=True)
        combat.hand = [card]
        combat.energy = 0

        assert card.star_cost == 2
        assert combat.can_play_card(card) is False
        combat.gain_stars(combat.player, 2)
        assert combat.play_card(0)
        assert combat.stars == 0
        assert combat.energy == 3

    def test_particle_wall_returns_to_hand_after_play(self):
        """Matches ParticleWall.cs: gain block and return to hand instead of discarding."""
        combat = _make_combat()
        card = create_card(CardId.PARTICLE_WALL, upgraded=True)
        combat.hand = [card]
        combat.energy = 0
        combat.gain_stars(combat.player, 2)

        assert combat.play_card(0)
        assert combat.player.block == 12
        assert combat.stars == 0
        assert card in combat.hand
        assert card not in combat.discard_pile

    def test_comet_spends_stars_and_applies_both_debuffs(self):
        """Matches Comet.cs: StarCost 5, damage, then Weak and Vulnerable."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        card = create_card(CardId.COMET)
        combat.hand = [card]
        combat.energy = 0
        combat.gain_stars(combat.player, 5)

        assert combat.play_card(0, 0)
        assert combat.stars == 0
        assert enemy.current_hp == 67
        assert enemy.get_power_amount(PowerId.WEAK) == 3
        assert enemy.get_power_amount(PowerId.VULNERABLE) == 3

    def test_seven_stars_hits_all_enemies_seven_times(self):
        """Matches SevenStars.cs: repeat-7 AoE and upgraded cost reduction."""
        combat = _make_combat()
        enemy_a = combat.enemies[0]
        enemy_b, ai_b = create_shrinker_beetle(Rng(123))
        combat.add_enemy(enemy_b, ai_b)
        enemy_a.current_hp = enemy_a.max_hp = 200
        enemy_b.current_hp = enemy_b.max_hp = 200
        card = create_card(CardId.SEVEN_STARS, upgraded=True)
        combat.hand = [card]
        combat.energy = 1
        combat.gain_stars(combat.player, 7)

        assert card.cost == 1
        assert combat.play_card(0)
        assert combat.stars == 0
        assert enemy_a.current_hp == 151
        assert enemy_b.current_hp == 151

    def test_sword_sage_upgrade_reduces_cost_and_applies_power(self):
        """Matches SwordSage.cs: upgraded card costs 1 and applies SwordSage power."""
        combat = _make_combat()
        card = create_card(CardId.SWORD_SAGE, upgraded=True)
        combat.hand = [card]
        combat.energy = 1

        assert card.cost == 1
        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.SWORD_SAGE) == 1

    def test_tyranny_upgrade_adds_innate_and_power_increases_turn_draw(self):
        """Matches Tyranny.cs + TyrannyPower.cs: upgraded Innate and +draw from power."""
        upgraded = create_card(CardId.TYRANNY_CARD, upgraded=True)
        assert upgraded.is_innate is True

        combat = _make_combat()
        card = create_card(CardId.TYRANNY_CARD)
        combat.hand = [card]
        combat.draw_pile = [create_card(CardId.STRIKE_REGENT) for _ in range(10)]
        combat.discard_pile = []
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.TYRANNY) == 1

        combat.end_player_turn()
        assert combat.round_number == 2
        assert len(combat.hand) == 6
