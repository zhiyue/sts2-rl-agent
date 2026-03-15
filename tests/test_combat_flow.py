"""Tests for combat flow: turns, draw, discard, energy, block clearing."""

import pytest

from sts2_env.cards.base import CardInstance
from sts2_env.cards.ironclad_basic import make_strike_ironclad, make_defend_ironclad
from sts2_env.core.enums import CardId, CardType, PowerId, TargetType, CardRarity
from sts2_env.core.constants import BASE_DRAW, BASE_ENERGY, MAX_HAND_SIZE


class TestBlockClearing:
    """Block is NOT cleared on round 1 player turn start, IS cleared on round 2+."""

    def test_round_1_block_not_cleared(self, simple_combat):
        """Block gained on round 1 persists during round 1."""
        for i, card in enumerate(simple_combat.hand):
            if card.card_id.name == "DEFEND_IRONCLAD":
                simple_combat.play_card(i)
                break
        assert simple_combat.player.block > 0
        assert simple_combat.round_number == 1

    def test_round_2_block_cleared(self, simple_combat):
        """Block is cleared at the start of round 2."""
        for i, card in enumerate(simple_combat.hand):
            if card.card_id.name == "DEFEND_IRONCLAD":
                simple_combat.play_card(i)
                break
        assert simple_combat.player.block > 0
        simple_combat.end_player_turn()
        assert simple_combat.round_number == 2
        assert simple_combat.player.block == 0


class TestEnergyReset:
    """Energy resets to BASE_ENERGY (3) each turn."""

    def test_energy_starts_at_base(self, simple_combat):
        assert simple_combat.energy == BASE_ENERGY

    def test_energy_resets_after_end_turn(self, simple_combat):
        simple_combat.energy = 0
        simple_combat.end_player_turn()
        assert simple_combat.energy == BASE_ENERGY

    def test_energy_resets_even_if_partial(self, simple_combat):
        simple_combat.energy = 1
        simple_combat.end_player_turn()
        assert simple_combat.energy == BASE_ENERGY


class TestDrawCards:
    """Draw 5 cards per turn; empty draw pile triggers shuffle from discard."""

    def test_initial_hand_is_5(self, simple_combat):
        assert len(simple_combat.hand) == BASE_DRAW

    def test_draw_5_each_turn(self, simple_combat):
        simple_combat.end_player_turn()
        assert len(simple_combat.hand) == BASE_DRAW

    def test_shuffle_from_discard_when_draw_empty(self, simple_combat):
        """When draw pile empties, discard pile shuffles into draw pile."""
        simple_combat.end_player_turn()  # round 2
        simple_combat.end_player_turn()  # round 3
        assert len(simple_combat.hand) == BASE_DRAW

    def test_hand_limit_10(self, simple_combat):
        """Hand cannot exceed MAX_HAND_SIZE (10) cards."""
        while len(simple_combat.hand) < MAX_HAND_SIZE:
            simple_combat.hand.append(make_strike_ironclad())
        assert len(simple_combat.hand) == MAX_HAND_SIZE
        simple_combat._draw_cards(5)
        assert len(simple_combat.hand) == MAX_HAND_SIZE


class TestEndTurn:
    """End-of-turn: hand discard, round increment."""

    def test_hand_discarded_and_redrawn(self, simple_combat):
        simple_combat.end_player_turn()
        assert len(simple_combat.hand) == BASE_DRAW

    def test_round_increments(self, simple_combat):
        assert simple_combat.round_number == 1
        simple_combat.end_player_turn()
        assert simple_combat.round_number == 2

    def test_ethereal_cards_exhausted(self, simple_combat):
        """Ethereal cards left in hand at end of turn are exhausted, not discarded."""
        ethereal = CardInstance(
            card_id=CardId.STRIKE_IRONCLAD,
            cost=1,
            card_type=CardType.ATTACK,
            target_type=TargetType.ANY_ENEMY,
            rarity=CardRarity.BASIC,
            base_damage=6,
            keywords=frozenset({"ethereal"}),
        )
        simple_combat.hand.clear()
        simple_combat.hand.append(ethereal)
        simple_combat.end_player_turn()
        assert ethereal in simple_combat.exhaust_pile
        assert ethereal not in simple_combat.discard_pile

    def test_non_ethereal_not_exhausted(self, simple_combat):
        """Normal cards are discarded, not exhausted, at end of turn."""
        normal = make_strike_ironclad()
        simple_combat.hand.clear()
        simple_combat.hand.append(normal)
        simple_combat.end_player_turn()
        assert normal not in simple_combat.exhaust_pile


class TestPlayCard:
    """Playing cards: energy cost, target resolution, pile movement."""

    def test_play_strike_costs_energy(self, simple_combat):
        for i, card in enumerate(simple_combat.hand):
            if card.card_id.name == "STRIKE_IRONCLAD":
                initial_energy = simple_combat.energy
                simple_combat.play_card(i, 0)
                assert simple_combat.energy == initial_energy - 1
                break

    def test_cannot_play_with_insufficient_energy(self, simple_combat):
        simple_combat.energy = 0
        result = simple_combat.play_card(0, 0)
        assert not result

    def test_play_card_damages_enemy(self, simple_combat):
        enemy = simple_combat.enemies[0]
        initial_hp = enemy.current_hp
        for i, card in enumerate(simple_combat.hand):
            if card.is_attack:
                simple_combat.play_card(i, 0)
                assert enemy.current_hp < initial_hp
                break


class TestBashEffect:
    """Bash deals damage and applies Vulnerable via combat."""

    def test_bash_applies_vulnerable(self, simple_combat):
        enemy = simple_combat.enemies[0]
        for i, card in enumerate(simple_combat.hand):
            if card.card_id.name == "BASH":
                simple_combat.play_card(i, 0)
                assert enemy.has_power(PowerId.VULNERABLE)
                assert enemy.get_power_amount(PowerId.VULNERABLE) == 2
                break
