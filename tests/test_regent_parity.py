"""Focused Regent parity tests backed by decompiled card models."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.colorless import make_hidden_gem
from sts2_env.cards.factory import create_card
from sts2_env.cards.ironclad_basic import make_defend_ironclad, make_strike_ironclad
from sts2_env.cards.regent import (
    make_big_bang,
    make_charge,
    make_bulwark,
    make_conqueror,
    make_convergence,
    create_regent_starter_deck,
    make_begone,
    make_crash_landing,
    make_decisions_decisions,
    make_guards,
    make_heirloom_hammer,
    make_hidden_cache,
    make_largesse,
    make_manifest_authority,
    make_pale_blue_dot,
    make_photon_cut,
    make_quasar,
    make_refine_blade,
    make_resonance,
    make_seeking_edge,
    make_solar_strike,
    make_spoils_of_battle,
    make_summon_forth,
    make_the_smith,
    make_void_form,
    make_wrought_in_war,
)
from sts2_env.cards.status import make_minion_dive_bomb, make_minion_sacrifice, make_minion_strike
from sts2_env.core.combat import CombatState
from sts2_env.core.creature import Creature
from sts2_env.core.enums import CardId, CombatSide, PowerId
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


class TestRegentParity:
    def test_begone_transforms_selected_hand_card_into_upgraded_minion_dive_bomb(self):
        """Matches Begone.cs: attack, then transform one selected hand card into MinionDiveBomb."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        target_card = make_strike_ironclad()
        combat.hand = [make_begone(upgraded=True), target_card]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - 5
        assert combat.pending_choice is not None
        assert [option.card for option in combat.pending_choice.options] == [target_card]

        assert combat.resolve_pending_choice(0)
        assert combat.pending_choice is None
        assert target_card not in combat.hand
        transformed = combat.hand[0]
        assert transformed.card_id == make_minion_dive_bomb(upgraded=True).card_id
        assert transformed.upgraded is True

    def test_photon_cut_draws_then_puts_selected_hand_card_back_on_top_of_draw(self):
        """Matches PhotonCut.cs: attack, draw, then choose a hand card to put on top of draw pile."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        kept = make_strike_ironclad()
        drawn = make_defend_ironclad()
        combat.hand = [make_photon_cut(), kept]
        combat.draw_pile = [drawn]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - 10
        assert combat.pending_choice is not None
        assert [option.card for option in combat.pending_choice.options] == [kept, drawn]

        assert combat.resolve_pending_choice(1)
        assert combat.pending_choice is None
        assert combat.draw_pile[0] is drawn
        assert drawn not in combat.hand
        assert kept in combat.hand

    def test_largesse_generates_colorless_card_in_target_ally_hand(self):
        """Matches Largesse.cs: generate one colorless card directly into the selected ally hand."""
        combat = _make_combat()
        ally = Creature(max_hp=40, current_hp=40, side=CombatSide.PLAYER, is_player=True)
        combat.add_ally_player(ally)
        combat.hand = [make_largesse(upgraded=True)]
        combat.energy = 0

        assert combat.can_play_card(combat.hand[0]) is True
        assert combat.play_card(0, 0)
        ally_hand = combat._ally_player_zones[ally]["hand"]  # noqa: SLF001
        assert len(ally_hand) == 1
        assert ally_hand[0].upgraded is True

    def test_manifest_authority_gains_block_and_adds_upgraded_colorless_card_to_hand(self):
        """Matches ManifestAuthority.cs: gain block, then add one colorless card to hand."""
        combat = _make_combat()
        combat.hand = [make_manifest_authority(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.block == 8
        assert len(combat.hand) == 1
        assert combat.hand[0].upgraded is True

    def test_void_form_applies_power_and_ends_turn_immediately(self):
        """Matches VoidForm.cs: apply VoidFormPower, then force end the turn."""
        combat = _make_combat()
        combat.hand = [make_void_form()]
        combat.energy = 3

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.VOID_FORM) == 2
        assert combat.round_number == 2
        assert combat.current_side == CombatSide.PLAYER

    def test_refine_blade_forges_and_applies_energy_next_turn(self):
        """Matches RefineBlade.cs: forge first, then apply EnergyNextTurn."""
        combat = _make_combat()
        combat.hand = [make_refine_blade()]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.ENERGY_NEXT_TURN) == 1
        blade = next(card for card in combat.hand if card.card_id.name == "SOVEREIGN_BLADE")
        assert blade.base_damage == 16

    def test_seeking_edge_forges_and_applies_seeking_edge_power(self):
        """Matches SeekingEdge.cs: forge immediately, then apply SeekingEdgePower."""
        combat = _make_combat()
        combat.hand = [make_seeking_edge()]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.SEEKING_EDGE) == 1
        blade = next(card for card in combat.hand if card.card_id.name == "SOVEREIGN_BLADE")
        assert blade.base_damage == 17

    def test_the_smith_spends_stars_and_forges_large_blade(self):
        """Matches TheSmith.cs: costs 4 Stars and forges by the card's forge amount."""
        combat = _make_combat()
        combat.hand = [make_the_smith()]
        combat.energy = 1
        combat.gain_stars(combat.player, 4)

        assert combat.play_card(0)
        assert combat.stars == 0
        blade = next(card for card in combat.hand if card.card_id.name == "SOVEREIGN_BLADE")
        assert blade.base_damage == 40

    def test_solar_strike_deals_damage_and_gains_star(self):
        """Matches SolarStrike.cs: deal damage, then gain the configured Stars."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        combat.hand = [make_solar_strike()]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - 8
        assert combat.stars == 1

    def test_spoils_of_battle_forges_without_other_side_effects(self):
        """Matches SpoilsOfBattle.cs: only forge by the card's forge amount."""
        combat = _make_combat()
        combat.hand = [make_spoils_of_battle()]
        combat.energy = 1

        assert combat.play_card(0)
        blade = next(card for card in combat.hand if card.card_id.name == "SOVEREIGN_BLADE")
        assert blade.base_damage == 20

    def test_wrought_in_war_deals_damage_then_forges(self):
        """Matches WroughtInWar.cs: attack the target, then forge."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        blade = create_card(CardId.SOVEREIGN_BLADE)
        combat.hand = [make_wrought_in_war(), blade]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - 7
        assert blade.base_damage == 15

    def test_knockout_blow_only_gains_stars_on_kill(self):
        """Matches KnockoutBlow.cs: stars are granted only if the target dies."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = 10
        enemy.max_hp = 10
        combat.hand = [create_card(CardId.KNOCKOUT_BLOW)]
        combat.energy = 3

        assert combat.play_card(0, 0)
        assert enemy.is_dead
        assert combat.stars == 5

    def test_charge_sorts_draw_pile_then_transforms_exact_selected_cards(self):
        """Matches Charge.cs: sorted draw-pile selection, then transform chosen cards into MinionStrike."""
        combat = _make_combat()
        card_a = make_strike_ironclad()
        card_b = make_defend_ironclad()
        card_c = create_card(CardId.VENERATE)
        combat.hand = [make_charge(upgraded=True)]
        combat.draw_pile = [card_a, card_b, card_c]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert combat.pending_choice.is_multi is True
        expected = sorted([card_a, card_b, card_c], key=lambda current: (current.rarity.value, current.card_id.value))
        assert [option.card for option in combat.pending_choice.options] == expected

        assert combat.resolve_pending_choice(0)
        assert combat.resolve_pending_choice(1)
        assert combat.resolve_pending_choice(None)
        assert combat.pending_choice is None

        minion_strikes = [card for card in combat.draw_pile if card.card_id == make_minion_strike().card_id]
        assert len(minion_strikes) == 2
        assert all(card.upgraded for card in minion_strikes)

    def test_guards_transforms_selected_hand_cards_into_minion_sacrifice(self):
        """Matches Guards.cs: choose any number of hand cards, then transform each into MinionSacrifice."""
        combat = _make_combat()
        a = make_strike_ironclad()
        b = make_defend_ironclad()
        combat.hand = [make_guards(upgraded=True), a, b]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert combat.pending_choice.is_multi is True

        assert combat.resolve_pending_choice(0)
        assert combat.resolve_pending_choice(1)
        assert combat.resolve_pending_choice(None)
        assert combat.pending_choice is None

        sacrifices = [card for card in combat.hand if card.card_id == make_minion_sacrifice().card_id]
        assert len(sacrifices) == 2
        assert all(card.upgraded for card in sacrifices)

    def test_big_bang_draws_then_gains_star_energy_and_forge(self):
        """Matches BigBang.cs: draw, gain stars, gain energy, then forge."""
        combat = _make_combat()
        drawn = make_strike_ironclad()
        combat.hand = [make_big_bang()]
        combat.draw_pile = [drawn]
        combat.energy = 0

        assert combat.play_card(0)
        assert combat.energy == 1
        assert combat.stars == 1
        assert drawn in combat.hand
        blade = next(card for card in combat.hand if card.card_id.name == "SOVEREIGN_BLADE")
        assert blade.base_damage == 15

    def test_hidden_cache_gains_star_and_star_next_turn_power(self):
        """Matches HiddenCache.cs: gain stars now and apply StarNextTurnPower."""
        combat = _make_combat()
        combat.hand = [make_hidden_cache(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.stars == 1
        assert combat.player.get_power_amount(PowerId.STAR_NEXT_TURN) == 4

    def test_resonance_buffs_owner_and_reduces_all_hittable_enemy_strength(self):
        """Matches Resonance.cs: gain Strength, then apply -1 Strength to all hittable enemies."""
        combat = _make_combat()
        enemy2, ai2 = create_shrinker_beetle(Rng(99))
        combat.add_enemy(enemy2, ai2)
        combat.hand = [make_resonance(upgraded=True)]
        combat.energy = 1
        combat.gain_stars(combat.player, 3)

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.STRENGTH) == 2
        assert combat.enemies[0].get_power_amount(PowerId.STRENGTH) == -1
        assert combat.enemies[1].get_power_amount(PowerId.STRENGTH) == -1

    def test_summon_forth_forges_and_returns_all_non_hand_sovereign_blades(self):
        """Matches SummonForth.cs: forge, then pull all non-hand SovereignBlade cards into hand."""
        combat = _make_combat()
        blade_draw = create_card(CardId.SOVEREIGN_BLADE)
        blade_discard = create_card(CardId.SOVEREIGN_BLADE)
        blade_exhaust = create_card(CardId.SOVEREIGN_BLADE)
        combat.hand = [make_summon_forth()]
        combat.draw_pile = [blade_draw]
        combat.discard_pile = [blade_discard]
        combat.exhaust_pile = [blade_exhaust]
        combat.energy = 1

        assert combat.play_card(0)
        blades = [card for card in combat.hand if card.card_id == CardId.SOVEREIGN_BLADE]
        assert len(blades) == 3
        assert all(card.base_damage == 18 for card in blades)

    def test_bulwark_gains_block_then_forges(self):
        """Matches Bulwark.cs: gain block, trigger cast animation, then forge."""
        combat = _make_combat()
        combat.hand = [make_bulwark()]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.player.block == 13
        blade = next(card for card in combat.hand if card.card_id == CardId.SOVEREIGN_BLADE)
        assert blade.base_damage == 20

    def test_conqueror_forges_then_applies_conqueror_power_to_target(self):
        """Matches Conqueror.cs: forge immediately, then apply ConquerorPower to the target."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        combat.hand = [make_conqueror()]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.get_power_amount(PowerId.CONQUEROR) == 1
        blade = next(card for card in combat.hand if card.card_id == CardId.SOVEREIGN_BLADE)
        assert blade.base_damage == 13

    def test_convergence_applies_retain_energy_and_star_next_turn_powers(self):
        """Matches Convergence.cs: apply RetainHand, EnergyNextTurn, and StarNextTurn."""
        combat = _make_combat()
        combat.hand = [make_convergence(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.RETAIN_HAND) == 1
        assert combat.player.get_power_amount(PowerId.ENERGY_NEXT_TURN) == 1
        assert combat.player.get_power_amount(PowerId.STAR_NEXT_TURN) == 2

    def test_glimmer_draws_then_puts_selected_hand_card_back_on_top_of_draw(self):
        """Matches Glimmer.cs: draw first, then choose one hand card to place on top of draw pile."""
        combat = _make_combat()
        kept = make_strike_ironclad()
        first_draw = make_defend_ironclad()
        second_draw = create_card(CardId.VENERATE)
        third_draw = create_card(CardId.GATHER_LIGHT)
        combat.hand = [create_card(CardId.GLIMMER), kept]
        combat.draw_pile = [first_draw, second_draw, third_draw]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert [option.card for option in combat.pending_choice.options] == [kept, first_draw, second_draw, third_draw]

        assert combat.resolve_pending_choice(2)
        assert combat.pending_choice is None
        assert combat.draw_pile[0] is second_draw
        assert second_draw not in combat.hand
        assert kept in combat.hand

    def test_decisions_decisions_draws_then_auto_plays_selected_skill_three_times(self):
        """Matches DecisionsDecisions.cs: draw first, then repeatedly auto-play the chosen Skill."""
        combat = _make_combat()
        repeated_skill = make_hidden_cache()
        combat.hand = [make_decisions_decisions(), repeated_skill]
        combat.draw_pile = [
            make_strike_ironclad(),
            make_strike_ironclad(),
            make_strike_ironclad(),
        ]
        combat.energy = 0
        combat.gain_stars(combat.player, 6)

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert [option.card for option in combat.pending_choice.options] == [repeated_skill]

        assert combat.resolve_pending_choice(0)
        assert combat.pending_choice is None
        assert combat.stars == 3
        assert combat.player.get_power_amount(PowerId.STAR_NEXT_TURN) == 9

    def test_quasar_offers_three_colorless_choices_and_adds_selected_card_to_hand(self):
        """Matches Quasar.cs: generate three distinct colorless options, then add the chosen card to hand."""
        combat = _make_combat()
        combat.hand = [make_quasar(upgraded=True)]
        combat.energy = 0
        combat.gain_stars(combat.player, 2)

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert len(combat.pending_choice.options) == 3

        assert combat.resolve_pending_choice(0)
        assert combat.pending_choice is None
        assert len(combat.hand) == 1
        assert combat.hand[0].upgraded is True

    def test_heirloom_hammer_damages_then_copies_selected_colorless_card(self):
        """Matches HeirloomHammer.cs: attack, then clone one selected colorless hand card into hand."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        gem = make_hidden_gem()
        combat.hand = [make_heirloom_hammer(), gem]
        combat.energy = 2

        assert combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - 17
        assert combat.pending_choice is not None
        assert [option.card for option in combat.pending_choice.options] == [gem]

        assert combat.resolve_pending_choice(0)
        assert combat.pending_choice is None
        copies = [card for card in combat.hand if card.card_id == gem.card_id]
        assert len(copies) == 2

    def test_crash_landing_fills_hand_to_ten_with_debris_after_attacking_all_enemies(self):
        """Matches CrashLanding.cs: attack all enemies, then add Debris until the hand reaches 10 cards."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        combat.hand = [make_crash_landing()]
        combat.energy = 1

        assert combat.play_card(0)
        assert enemy.current_hp == starting_hp - 21
        debris = [card for card in combat.hand if card.card_id == CardId.DEBRIS]
        assert len(combat.hand) == 10
        assert len(debris) == 10

    def test_black_hole_applies_black_hole_power(self):
        """Matches BlackHole.cs: apply BlackHolePower to the owner."""
        combat = _make_combat()
        card = create_card(CardId.BLACK_HOLE)
        combat.hand = [card]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.BLACK_HOLE) == card.effect_vars.get("black_hole", 3)

    def test_furnace_applies_furnace_power(self):
        """Matches Furnace.cs: apply FurnacePower with the configured forge amount."""
        combat = _make_combat()
        card = create_card(CardId.FURNACE)
        combat.hand = [card]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.FURNACE) == card.effect_vars.get("furnace", 4)

    def test_orbit_applies_orbit_power(self):
        """Matches Orbit.cs: apply OrbitPower with the configured energy value."""
        combat = _make_combat()
        card = create_card(CardId.ORBIT)
        combat.hand = [card]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.ORBIT) == card.effect_vars.get("energy", 1)

    def test_pale_blue_dot_applies_pale_blue_dot_power(self):
        """Matches PaleBlueDot.cs: apply PaleBlueDotPower with the configured card count."""
        combat = _make_combat()
        card = make_pale_blue_dot(upgraded=True)
        combat.hand = [card]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.PALE_BLUE_DOT) == card.effect_vars.get("cards", 2)
