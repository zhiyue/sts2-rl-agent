"""Parity tests for Regent star-cost, Void Form, and forge card effects."""

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


class TestRegentStarCostVoidFormForgeParity:
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
        # C# AstralPulse.cs:24-28: Damage 6 with hit count 2 => 12 total damage.
        assert enemy_a.current_hp == 88
        assert enemy_b.current_hp == 88

    def test_guiding_star_spends_star_cost_and_draws_immediately(self):
        """Matches GuidingStar.cs: StarCost 2, damage target, then
        CardPileCmd.Draw(Cards.BaseValue) — an immediate draw of 2 (upgraded 3),
        not a DrawCardsNextTurn power (GuidingStar.cs:49)."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        card = create_card(CardId.GUIDING_STAR, upgraded=True)
        combat.hand = [card]
        combat.energy = 1

        assert card.star_cost == 2
        assert combat.can_play_card(card) is False
        combat.gain_stars(combat.player, 2)
        draw_before = combat.count_cards_drawn_this_combat(combat.player)
        assert combat.play_card(0, 0)
        assert combat.stars == 0
        assert enemy.current_hp == 87
        assert combat.count_cards_drawn_this_combat(combat.player) == draw_before + 3
        assert combat.player.get_power_amount(PowerId.DRAW_CARDS_NEXT_TURN) == 0

    def test_alignment_spends_stars_and_grants_energy(self):
        """Matches Alignment.cs (v0.107.1): CanonicalStarCost 3, EnergyVar(2),
        OnUpgrade Energy.UpgradeValueBy(1) -> upgraded grants 3 energy."""
        combat = _make_combat()
        card = create_card(CardId.ALIGNMENT, upgraded=True)
        combat.hand = [card]
        combat.energy = 0

        assert card.star_cost == 3
        assert combat.can_play_card(card) is False
        combat.gain_stars(combat.player, 3)
        assert combat.play_card(0)
        assert combat.stars == 0
        assert combat.energy == 3

    def test_void_form_waives_star_cost_for_covered_card_play(self):
        """Matches VoidFormPower.cs: TryModifyStarCost returns 0 for covered cards."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        card = create_card(CardId.GUIDING_STAR)
        combat.hand = [card]
        combat.energy = 1
        combat.apply_power_to(combat.player, PowerId.VOID_FORM, 1)

        assert card.star_cost == 2
        assert combat.modified_star_cost(combat.player, card) == 0
        assert combat.can_play_card(card) is True
        assert combat.play_card(0, 0)
        assert combat.stars == 0
        assert enemy.current_hp == 88

    def test_void_form_only_waives_hand_or_play_pile_cards_like_reference(self):
        """Matches VoidFormPower.cs: ShouldSkip only covers cards in Hand or Play."""
        combat = _make_combat()
        hand_card = create_card(CardId.GUIDING_STAR)
        draw_card = create_card(CardId.GUIDING_STAR)
        discard_card = create_card(CardId.GUIDING_STAR)
        combat.hand = [hand_card]
        combat.draw_pile = [draw_card]
        combat.discard_pile = [discard_card]
        combat.apply_power_to(combat.player, PowerId.VOID_FORM, 2)

        assert combat.modified_card_cost(combat.player, hand_card) == 0
        assert combat.modified_star_cost(combat.player, hand_card) == 0
        assert combat.modified_card_cost(combat.player, draw_card) == draw_card.cost
        assert combat.modified_star_cost(combat.player, draw_card) == draw_card.star_cost
        assert combat.modified_card_cost(combat.player, discard_card) == discard_card.cost
        assert combat.modified_star_cost(combat.player, discard_card) == discard_card.star_cost

    def test_void_form_counts_replayed_card_once_for_coverage(self):
        """Matches VoidFormPower.cs: AfterCardPlayed counts only the last play in a series."""
        combat = _make_combat()
        first = create_card(CardId.GUIDING_STAR)
        first.base_replay_count = 1
        second = create_card(CardId.ALIGNMENT)
        combat.hand = [first, second]
        combat.energy = 2
        combat.apply_power_to(combat.player, PowerId.VOID_FORM, 2)

        assert combat.play_card(0, 0)
        assert combat.modified_star_cost(combat.player, second) == 0
        assert combat.modified_card_cost(combat.player, second) == 0

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

    def test_sword_sage_increases_sovereign_blade_cost_and_repeats(self):
        """Matches SwordSagePower.cs: SovereignBlade costs +Amount and repeats Amount+1 times."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        blade = create_card(CardId.SOVEREIGN_BLADE)
        combat.hand = [blade]
        combat.energy = 3
        combat.apply_power_to(combat.player, PowerId.SWORD_SAGE, 1)

        assert combat.modified_card_cost(combat.player, blade) == 3
        assert combat.play_card(0, 0)
        assert enemy.current_hp == 80

    def test_sovereign_blade_upgrade_preserves_forged_damage(self):
        combat = _make_combat()
        blade = create_card(CardId.SOVEREIGN_BLADE)
        combat.hand = [blade]

        combat.forge(combat.player, 7)
        assert blade.base_damage == 17

        combat.upgrade_card(blade)

        assert blade.upgraded is True
        assert blade.cost == 1
        assert blade.base_damage == 17

    def test_forge_ignores_sovereign_blade_dupes(self):
        """Matches ForgeCmd.cs: GetSovereignBlades excludes IsDupe cards."""
        combat = _make_combat()
        blade = create_card(CardId.SOVEREIGN_BLADE)
        dupe = blade.create_dupe(123456)
        combat.hand = [blade, dupe]

        combat.forge(combat.player, 7)

        assert blade.base_damage == 17
        assert dupe.base_damage == 10

    def test_tyranny_upgrade_adds_innate_and_power_increases_turn_draw(self):
        """Matches Tyranny.cs + TyrannyPower.cs: upgraded Innate and +draw from power."""
        upgraded = create_card(CardId.TYRANNY_CARD, upgraded=True)
        assert upgraded.is_innate is True

        combat = _make_combat()
        card = create_card(CardId.TYRANNY_CARD)
        first_drawn = create_card(CardId.STRIKE_REGENT)
        combat.hand = [card]
        combat.draw_pile = [first_drawn] + [create_card(CardId.STRIKE_REGENT) for _ in range(9)]
        combat.discard_pile = []
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.TYRANNY) == 1

        combat.end_player_turn()
        assert combat.round_number == 2
        assert len(combat.hand) == 6
        assert combat.pending_choice is not None
        assert combat.pending_choice.min_choices == 1
        assert combat.pending_choice.max_choices == 1

        assert combat.resolve_pending_choice(0)
        assert combat.pending_choice is None

        assert first_drawn in combat.exhaust_pile
        assert first_drawn not in combat.hand

    def test_foregone_conclusion_selects_draw_pile_cards_before_hand_draw(self):
        """Matches ForegoneConclusionPower.cs: choose draw-pile cards into hand before drawing."""
        combat = _make_combat()
        skill = create_card(CardId.FOREGONE_CONCLUSION)
        chosen_a = create_card(CardId.DEFEND_REGENT)
        chosen_b = create_card(CardId.FALLING_STAR)
        later_cards = [create_card(CardId.STRIKE_REGENT) for _ in range(5)]
        combat.hand = [skill]
        combat.draw_pile = [chosen_a, chosen_b, *later_cards]
        combat.discard_pile = []
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.FOREGONE_CONCLUSION) == 2

        combat.end_player_turn()

        assert combat.pending_choice is not None
        assert combat.pending_choice.min_choices == 2
        assert combat.pending_choice.max_choices == 2

        assert combat.resolve_pending_choice(0)
        assert combat.resolve_pending_choice(1)
        assert combat.resolve_pending_choice(None)

        assert chosen_a in combat.hand
        assert chosen_b in combat.hand
        assert combat.player.get_power_amount(PowerId.FOREGONE_CONCLUSION) == 0

    def test_foregone_conclusion_shuffles_discard_before_selection_like_reference(self):
        """Matches ForegoneConclusionPower.cs: ShuffleIfNecessary runs before selection."""
        combat = _make_combat()
        skill = create_card(CardId.FOREGONE_CONCLUSION)
        shuffled_choice = create_card(CardId.DEFEND_REGENT)
        combat.hand = [skill]
        combat.draw_pile = []
        combat.discard_pile = [shuffled_choice]
        combat.energy = 1

        assert combat.play_card(0)
        combat.end_player_turn()

        assert combat.pending_choice is None
        assert shuffled_choice in combat.hand
        assert shuffled_choice not in combat.discard_pile
        assert combat.player.get_power_amount(PowerId.FOREGONE_CONCLUSION) == 0
