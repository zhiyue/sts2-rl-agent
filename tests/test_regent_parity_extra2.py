"""Additional focused Regent parity tests for remaining high-signal cards."""

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


class TestRegentParityExtra2:
    def test_venerate_base_and_upgraded_gain_expected_stars(self):
        """Matches Venerate.cs: gain Stars using base and upgraded dynamic vars."""
        combat = _make_combat()
        combat.hand = [create_card(CardId.VENERATE), create_card(CardId.VENERATE, upgraded=True)]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.stars == 2
        assert combat.play_card(0)
        assert combat.stars == 5
        assert combat.count_stars_gained_this_turn(combat.player) == 5

    def test_collision_course_generates_debris_and_counts_as_generated_card(self):
        """Matches CollisionCourse.cs: attack then add a generated Debris card to hand."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        start_hp = enemy.current_hp
        start_generated = combat.count_generated_cards_this_combat(combat.player)
        combat.hand = [create_card(CardId.COLLISION_COURSE)]
        combat.energy = 0

        assert combat.play_card(0, 0)
        assert enemy.current_hp == start_hp - 9
        assert [card.card_id for card in combat.hand] == [CardId.DEBRIS]
        assert combat.count_generated_cards_this_combat(combat.player) == start_generated + 1

    def test_cosmic_indifference_gains_block_then_moves_selected_discard_to_top_of_draw(self):
        """Matches CosmicIndifference.cs: block first, then choose one discard card for draw-top."""
        combat = _make_combat()
        chosen = create_card(CardId.DEFEND_REGENT)
        other = create_card(CardId.STRIKE_REGENT)
        existing_top = create_card(CardId.GATHER_LIGHT)
        combat.hand = [create_card(CardId.COSMIC_INDIFFERENCE)]
        combat.discard_pile = [other, chosen]
        combat.draw_pile = [existing_top]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.block == 6
        assert combat.pending_choice is not None
        assert [option.card for option in combat.pending_choice.options] == [other, chosen]

        assert combat.resolve_pending_choice(1)
        assert combat.pending_choice is None
        assert combat.draw_pile[0] is chosen
        assert combat.draw_pile[1] is existing_top
        assert chosen not in combat.discard_pile
        assert other in combat.discard_pile

    def test_supermassive_scales_with_generated_cards_this_combat(self):
        """Matches Supermassive.cs: dynamic damage uses generated-card combat history."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = 100
        enemy.max_hp = 100
        combat.hand = [create_card(CardId.COLLISION_COURSE), create_card(CardId.SUPERMASSIVE)]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert combat.count_generated_cards_this_combat(combat.player) == 1
        assert combat.play_card(0, 0)
        assert enemy.current_hp == 100 - 9 - 8

    def test_royal_gamble_requires_star_cost_then_nets_stars_and_exhausts(self):
        """Matches RoyalGamble.cs: spend 5 Stars to play, then gain 9 Stars and Exhaust."""
        combat = _make_combat()
        gamble = create_card(CardId.ROYAL_GAMBLE)
        combat.hand = [gamble]
        combat.energy = 0
        combat.gain_stars(combat.player, 5)

        assert combat.can_play_card(gamble) is True
        assert combat.play_card(0)
        assert combat.stars == 9
        assert gamble in combat.exhaust_pile
        assert gamble not in combat.discard_pile

        blocked = _make_combat()
        blocked_gamble = create_card(CardId.ROYAL_GAMBLE)
        blocked.hand = [blocked_gamble]
        blocked.energy = 0
        blocked.gain_stars(blocked.player, 4)
        assert blocked.can_play_card(blocked_gamble) is False
        assert blocked.play_card(0) is False

    def test_terraforming_applies_vigor_that_is_consumed_by_next_powered_attack(self):
        """Matches Terraforming.cs + VigorPower: apply Vigor, then consume it on next attack."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        start_hp = enemy.current_hp
        combat.hand = [create_card(CardId.TERRAFORMING), create_card(CardId.STRIKE_REGENT)]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.VIGOR) == 6
        assert combat.play_card(0, 0)
        assert enemy.current_hp == start_hp - 12
        assert combat.player.get_power_amount(PowerId.VIGOR) == 0

    def test_genesis_applies_power_and_grants_stars_at_next_player_turn_start(self):
        """Matches Genesis.cs + GenesisPower.cs: gain Stars at next player-side turn start."""
        combat = _make_combat()
        combat.hand = [create_card(CardId.GENESIS)]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.GENESIS) == 2
        assert combat.stars == 0

        combat.end_player_turn()
        assert combat.round_number == 2
        assert combat.stars == 2

    def test_heavenly_drill_uses_x_energy_and_doubles_hits_when_x_reaches_threshold(self):
        """Matches HeavenlyDrill.cs: X hits, doubled when spent energy >= 4."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = 200
        enemy.max_hp = 200
        drill = create_card(CardId.HEAVENLY_DRILL)
        combat.hand = [drill]
        combat.energy = 4

        assert combat.play_card(0, 0)
        assert drill.energy_spent == 4
        assert combat.energy == 0
        assert enemy.current_hp == 136
