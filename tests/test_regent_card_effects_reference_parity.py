"""Reference parity tests for Regent card effects."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.colorless import make_finesse
from sts2_env.cards.factory import create_card
from sts2_env.cards.regent import (
    make_bombardment,
    make_astral_pulse,
    make_celestial_might,
    create_regent_starter_deck,
    make_bundle_of_joy,
    make_child_of_the_stars,
    make_cloak_of_stars,
    make_comet,
    make_crescent_spear,
    make_crush_under,
    make_defend_regent,
    make_devastate,
    make_dying_star,
    make_gamma_blast,
    make_glitterstream,
    make_hammer_time,
    make_hegemony,
    make_heirloom_hammer,
    make_i_am_invincible,
    make_kingly_kick,
    make_kingly_punch,
    make_know_thy_place,
    make_lunar_blast,
    make_monarchs_gaze_card,
    make_monologue_card,
    make_neutron_aegis,
    make_patter,
    make_prophesize,
    make_quasar,
    make_resonance,
    make_royalties_card,
    make_shining_strike,
    make_spoils_of_battle,
    make_spectrum_shift,
    make_stardust,
    make_the_sealed_throne,
    make_venerate,
)
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import CardId, CombatSide, PowerId, ValueProp
from sts2_env.core.hooks import fire_after_turn_end
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle
from sts2_env.powers.base import PowerInstance
from sts2_env.run.run_state import PlayerState


PATTER_BLOCK = 8
PATTER_VIGOR = 2
PATTER_UPGRADED_BLOCK = 10
PATTER_UPGRADED_VIGOR = 3
MONARCHS_GAZE_STRENGTH_LOSS = 1


def _make_combat(*, extra_enemies: int = 0) -> CombatState:
    combat = CombatState(
        player_hp=60,
        player_max_hp=60,
        deck=create_regent_starter_deck(),
        rng_seed=5151,
        character_id="Regent",
    )
    creature, ai = create_shrinker_beetle(Rng(5151))
    combat.add_enemy(creature, ai)
    for i in range(extra_enemies):
        extra_creature, extra_ai = create_shrinker_beetle(Rng(5200 + i))
        combat.add_enemy(extra_creature, extra_ai)
    combat.start_combat()
    return combat


class _CannotHitPower(PowerInstance):
    def __init__(self):
        super().__init__(PowerId.COVERED, 1)

    def should_allow_hitting(self, owner, combat):
        return False


class TestRegentCardEffectsReferenceParity:
    def test_patter_gains_block_then_applies_vigor(self):
        combat = _make_combat()
        combat.hand = [make_patter()]
        combat.energy = 1

        assert combat.play_card(0)

        assert combat.player.block == PATTER_BLOCK
        assert combat.player.get_power_amount(PowerId.VIGOR) == PATTER_VIGOR

    def test_upgraded_patter_values_match_reference(self):
        combat = _make_combat()
        combat.hand = [make_patter(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0)

        assert combat.player.block == PATTER_UPGRADED_BLOCK
        assert combat.player.get_power_amount(PowerId.VIGOR) == PATTER_UPGRADED_VIGOR

    def test_celestial_might_hits_three_times(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        combat.hand = [make_celestial_might()]
        combat.energy = 2

        assert combat.play_card(0, 0)

        assert enemy.current_hp == 82

    def test_crescent_spear_scales_with_current_star_cost_cards(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        star_card = make_the_sealed_throne()
        combat.hand = [make_crescent_spear(), star_card]
        combat.draw_pile = [make_celestial_might()]
        combat.discard_pile = [make_devastate()]
        combat.energy = 1
        combat.gain_stars(combat.player, 1)

        assert combat.play_card(0, 0)

        # C# CrescentSpear.cs:20-22: CalculationBaseVar(8) + ExtraDamageVar(2) per
        # card with CanonicalStarCost >= 0 or HasStarCostX. Star cards here:
        # CrescentSpear (1) + TheSealedThrone (3) + Devastate (4) => 8 + 2*3 = 14.
        assert enemy.current_hp == 86
        assert combat.stars == 0

    def test_devastate_spends_stars_and_deals_large_damage(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        combat.hand = [make_devastate()]
        combat.energy = 1
        combat.gain_stars(combat.player, 4)

        assert combat.play_card(0, 0)

        assert enemy.current_hp == 70
        assert combat.stars == 0

    def test_gamma_blast_spends_stars_then_applies_weak_and_vulnerable(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        combat.hand = [make_gamma_blast()]
        combat.energy = 0
        combat.gain_stars(combat.player, 3)

        assert combat.play_card(0, 0)

        assert enemy.current_hp == 87
        assert combat.stars == 0
        assert enemy.get_power_amount(PowerId.WEAK) == 2
        assert enemy.get_power_amount(PowerId.VULNERABLE) == 2

    def test_glitterstream_next_turn_block_uses_block_modifiers(self):
        combat = _make_combat()
        combat.apply_power_to(combat.player, PowerId.DEXTERITY, 2)
        combat.hand = [make_glitterstream()]
        combat.energy = 2

        assert combat.play_card(0)

        assert combat.player.block == 13
        # C# Glitterstream.cs:22: BlockVar("BlockNextTurn", 5); Dexterity applies.
        assert combat.player.get_power_amount(PowerId.BLOCK_NEXT_TURN) == 7

    def test_hegemony_deals_damage_and_grants_energy_next_turn(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        combat.hand = [make_hegemony()]
        combat.energy = 2

        assert combat.play_card(0, 0)

        assert enemy.current_hp == starting_hp - 15
        assert combat.player.get_power_amount(PowerId.ENERGY_NEXT_TURN) == 2

    def test_know_thy_place_base_exhausts_and_upgrade_does_not(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        base = make_know_thy_place()
        upgraded = make_know_thy_place(upgraded=True)
        combat.hand = [base, upgraded]
        combat.energy = 0

        assert combat.play_card(0, 0)
        assert enemy.get_power_amount(PowerId.WEAK) == 1
        assert enemy.get_power_amount(PowerId.VULNERABLE) == 1
        assert base in combat.exhaust_pile

        assert combat.play_card(0, 0)
        assert enemy.get_power_amount(PowerId.WEAK) == 2
        assert enemy.get_power_amount(PowerId.VULNERABLE) == 2
        assert upgraded in combat.discard_pile

    def test_lunar_blast_hits_once_per_finished_skill_play_this_turn(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        combat.hand = [make_defend_regent(), make_know_thy_place(upgraded=True), make_lunar_blast()]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.play_card(0, 0)
        assert combat.play_card(0, 0)

        assert enemy.current_hp == 88

    def test_monarchs_gaze_applies_temporary_strength_down_after_powered_attack_only(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        combat.hand = [make_monarchs_gaze_card(), make_shining_strike()]
        combat.energy = 4

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.MONARCHS_GAZE) == MONARCHS_GAZE_STRENGTH_LOSS

        combat.deal_damage(combat.player, enemy, 4, ValueProp.UNPOWERED | ValueProp.MOVE)
        assert enemy.get_power_amount(PowerId.MONARCHS_GAZE_STRENGTH_DOWN) == 0
        assert enemy.get_power_amount(PowerId.STRENGTH) == 0

        assert combat.play_card(0, 0)
        assert enemy.get_power_amount(PowerId.MONARCHS_GAZE_STRENGTH_DOWN) == MONARCHS_GAZE_STRENGTH_LOSS
        assert enemy.get_power_amount(PowerId.STRENGTH) == -MONARCHS_GAZE_STRENGTH_LOSS

        fire_after_turn_end(CombatSide.ENEMY, combat)

        assert enemy.get_power_amount(PowerId.MONARCHS_GAZE_STRENGTH_DOWN) == 0
        assert enemy.get_power_amount(PowerId.STRENGTH) == 0

    def test_neutron_aegis_spends_stars_and_applies_plating(self):
        combat = _make_combat()
        combat.hand = [make_neutron_aegis()]
        combat.energy = 1
        combat.gain_stars(combat.player, 5)

        assert combat.play_card(0)

        assert combat.stars == 0
        assert combat.player.get_power_amount(PowerId.PLATING) == 8

    def test_prophesize_draws_six_cards(self):
        combat = _make_combat()
        drawn = [make_defend_regent() for _ in range(6)]
        combat.hand = [make_prophesize()]
        combat.draw_pile = list(drawn)
        combat.energy = 2

        assert combat.play_card(0)

        assert combat.hand == drawn

    def test_the_sealed_throne_spends_stars_and_applies_power(self):
        combat = _make_combat()
        combat.hand = [make_the_sealed_throne()]
        combat.energy = 1
        combat.gain_stars(combat.player, 3)

        assert combat.play_card(0)

        assert combat.stars == 0
        assert combat.player.get_power_amount(PowerId.THE_SEALED_THRONE) == 1

    def test_defend_regent_base_and_upgrade_block_values_match_reference(self):
        combat = _make_combat()
        combat.hand = [make_defend_regent(), make_defend_regent(upgraded=True)]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.player.block == 5

        assert combat.play_card(0)
        assert combat.player.block == 13

    def test_bundle_of_joy_adds_three_distinct_colorless_cards_to_hand(self):
        combat = _make_combat()
        combat.hand = [make_bundle_of_joy()]
        combat.energy = 2

        assert combat.play_card(0)
        assert len(combat.hand) == 3
        assert len({card.card_id for card in combat.hand}) == 3

    def test_royalties_applies_gold_value_as_power_amount(self):
        """Royalties.cs OnPlay: PowerCmd.Apply<RoyaltiesPower>(Gold.BaseValue) —
        the power amount is the Gold var (30, upgraded 40); RoyaltiesPower.cs
        AfterCombatEnd adds a GoldReward(Amount) for that much gold."""
        combat = _make_combat()
        combat.hand = [make_royalties_card()]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.ROYALTIES) == 30

        upgraded_combat = _make_combat()
        upgraded_combat.hand = [make_royalties_card(upgraded=True)]
        upgraded_combat.energy = 1

        assert upgraded_combat.play_card(0)
        assert upgraded_combat.player.get_power_amount(PowerId.ROYALTIES) == 40

    def test_heirloom_hammer_copies_colorless_visual_pool_hand_cards(self):
        """HeirloomHammer.cs OnPlay: hand filter is c.VisualCardPool.IsColorless —
        TOKEN-pool cards (Soul) count as colorless while STATUS-pool cards
        (Debris/Slimed) and curse cards do not (TokenCardPool.cs:15
        IsColorless => true, StatusCardPool.cs:16 IsColorless => false)."""
        from sts2_env.cards.status import make_slimed, make_soul

        combat = _make_combat()
        soul = make_soul()
        status = make_slimed()
        assert soul.visual_card_pool_is_colorless
        assert not status.visual_card_pool_is_colorless
        combat.hand = [make_heirloom_hammer(), soul, status]
        combat.energy = 2

        assert combat.play_card(0, 0)
        # Soul is the only IsColorless hand card, so the single-option
        # selection resolves immediately and copies it once (RepeatVar(1)).
        assert len([c for c in combat.hand if c.card_id == soul.card_id]) == 2
        assert status in combat.hand

    def test_arsenal_gains_strength_after_playing_colorless_card(self):
        combat = _make_combat()
        combat.hand = [make_royalties_card(), make_finesse()]
        # Royalties is not Arsenal, but uses same "apply power then later behavior" pattern? no.
        # Create actual Arsenal from reference factory via make function name injected in module.
        from sts2_env.cards.regent import make_arsenal

        combat.hand = [make_arsenal(), make_finesse()]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.ARSENAL) == 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.STRENGTH) == 1

        combat.hand = [create_card(CardId.ENTRENCH)]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.STRENGTH) == 1

    def test_child_of_the_stars_gains_block_when_stars_are_spent(self):
        combat = _make_combat()
        combat.hand = [make_child_of_the_stars(), make_quasar()]
        combat.energy = 2
        combat.gain_stars(combat.player, 2)

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.CHILD_OF_THE_STARS) == 2

        assert combat.play_card(0)
        assert combat.player.block == 4
        assert combat.pending_choice is not None
        assert combat.resolve_pending_choice(None)

    def test_cloak_of_stars_requires_and_spends_one_star(self):
        combat = _make_combat()
        card = make_cloak_of_stars()
        combat.hand = [card]
        combat.energy = 0

        assert card.star_cost == 1
        assert combat.can_play_card(card) is False

        combat.gain_stars(combat.player, 1)

        assert combat.play_card(0)
        assert combat.stars == 0
        assert combat.player.block == 7

    def test_pale_blue_dot_draws_extra_after_five_cards_played_last_round(self):
        combat = _make_combat()
        combat.player.apply_power(PowerId.PALE_BLUE_DOT, 1)
        combat.hand = [make_defend_regent() for _ in range(4)]
        combat.draw_pile = [make_defend_regent() for _ in range(8)]
        combat.energy = 4

        for _ in range(4):
            assert combat.play_card(0)

        combat.end_player_turn()
        assert len(combat.hand) == 5

        combat.hand = [make_defend_regent() for _ in range(5)]
        combat.draw_pile = [make_defend_regent() for _ in range(8)]
        combat.energy = 5

        for _ in range(5):
            assert combat.play_card(0)

        combat.end_player_turn()
        assert len(combat.hand) == 6

    def test_spectrum_shift_generates_colorless_card_before_next_hand_draw(self):
        combat = _make_combat()
        combat.hand = [make_spectrum_shift()]
        combat.draw_pile = []
        combat.discard_pile = []
        combat.energy = 2

        assert combat.play_card(0)
        combat.end_player_turn()

        assert len(combat.hand) == 1
        assert combat.hand[0].card_id not in {card.card_id for card in create_regent_starter_deck()}

    def test_black_hole_star_gain_damage_hits_only_hittable_enemies(self):
        combat = _make_combat(extra_enemies=1)
        blocked, hittable = combat.enemies
        blocked.current_hp = blocked.max_hp = 100
        hittable.current_hp = hittable.max_hp = 100
        blocked.powers[PowerId.COVERED] = _CannotHitPower()
        combat.apply_power_to(combat.player, PowerId.BLACK_HOLE, 3)

        combat.gain_stars(combat.player, 1)

        assert blocked.current_hp == 100
        assert hittable.current_hp == 97

    def test_astral_pulse_hits_only_hittable_enemies(self):
        combat = _make_combat(extra_enemies=1)
        blocked, hittable = combat.enemies
        blocked.current_hp = blocked.max_hp = 100
        hittable.current_hp = hittable.max_hp = 100
        blocked.powers[PowerId.COVERED] = _CannotHitPower()
        combat.hand = [make_astral_pulse()]
        combat.energy = 0
        combat.gain_stars(combat.player, 3)

        assert combat.play_card(0)
        assert blocked.current_hp == 100
        # C# AstralPulse.cs:24-28: Damage 6 with hit count 2 => 12 total damage.
        assert hittable.current_hp == 88

    def test_resonance_reduces_strength_only_on_hittable_enemies(self):
        combat = _make_combat(extra_enemies=1)
        blocked, hittable = combat.enemies
        blocked.powers[PowerId.COVERED] = _CannotHitPower()
        combat.hand = [make_resonance()]
        combat.energy = 1
        combat.gain_stars(combat.player, 3)

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.STRENGTH) == 1
        assert blocked.get_power_amount(PowerId.STRENGTH) == 0
        assert hittable.get_power_amount(PowerId.STRENGTH) == -1

    def test_stardust_random_hits_use_only_hittable_enemies(self):
        combat = _make_combat(extra_enemies=1)
        blocked, hittable = combat.enemies
        blocked.current_hp = blocked.max_hp = 100
        hittable.current_hp = hittable.max_hp = 100
        blocked.powers[PowerId.COVERED] = _CannotHitPower()
        combat.hand = [make_stardust()]
        combat.energy = 0
        combat.gain_stars(combat.player, 3)

        assert combat.play_card(0)
        assert blocked.current_hp == 100
        assert hittable.current_hp == 85
        assert combat.stars == 0

    def test_dying_star_hits_and_debuffs_only_hittable_enemies(self):
        combat = _make_combat(extra_enemies=1)
        blocked, hittable = combat.enemies
        blocked.current_hp = blocked.max_hp = 100
        hittable.current_hp = hittable.max_hp = 100
        blocked.powers[PowerId.COVERED] = _CannotHitPower()
        combat.hand = [make_dying_star()]
        combat.energy = 1
        combat.gain_stars(combat.player, 3)

        assert combat.play_card(0)
        assert blocked.current_hp == 100
        assert blocked.get_power_amount(PowerId.DYING_STAR) == 0
        assert hittable.current_hp == 91
        assert hittable.get_power_amount(PowerId.DYING_STAR) == 9
        assert hittable.get_power_amount(PowerId.STRENGTH) == -9

        fire_after_turn_end(CombatSide.ENEMY, combat)

        assert hittable.get_power_amount(PowerId.DYING_STAR) == 0
        assert hittable.get_power_amount(PowerId.STRENGTH) == 0

    def test_crush_under_applies_temporary_strength_loss_to_hittable_enemies(self):
        combat = _make_combat(extra_enemies=1)
        blocked, hittable = combat.enemies
        blocked.current_hp = blocked.max_hp = 100
        hittable.current_hp = hittable.max_hp = 100
        blocked.powers[PowerId.COVERED] = _CannotHitPower()
        combat.hand = [make_crush_under(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0)
        assert blocked.current_hp == 100
        assert blocked.get_power_amount(PowerId.CRUSH_UNDER) == 0
        assert hittable.current_hp == 92
        assert hittable.get_power_amount(PowerId.CRUSH_UNDER) == 2
        assert hittable.get_power_amount(PowerId.STRENGTH) == -2

        fire_after_turn_end(CombatSide.ENEMY, combat)

        assert hittable.get_power_amount(PowerId.CRUSH_UNDER) == 0
        assert hittable.get_power_amount(PowerId.STRENGTH) == 0

    def test_meteor_shower_hits_and_debuffs_only_hittable_enemies(self):
        combat = _make_combat(extra_enemies=1)
        blocked, hittable = combat.enemies
        blocked.current_hp = blocked.max_hp = 100
        hittable.current_hp = hittable.max_hp = 100
        blocked.powers[PowerId.COVERED] = _CannotHitPower()
        combat.hand = [create_card(CardId.METEOR_SHOWER)]
        combat.energy = 0
        combat.gain_stars(combat.player, 2)

        assert combat.play_card(0)
        assert blocked.current_hp == 100
        assert blocked.get_power_amount(PowerId.WEAK) == 0
        assert blocked.get_power_amount(PowerId.VULNERABLE) == 0
        assert hittable.current_hp == 86
        assert hittable.get_power_amount(PowerId.WEAK) == 2
        assert hittable.get_power_amount(PowerId.VULNERABLE) == 2

    def test_hammer_time_repeats_forge_for_other_living_players(self):
        combat = _make_combat()
        ally = combat.add_ally_player(
            PlayerState(player_id=2, character_id="Regent", max_hp=50, current_hp=50)
        )
        combat.hand = [make_hammer_time(), make_spoils_of_battle()]
        combat.energy = 3

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.HAMMER_TIME) == 1

        assert combat.play_card(0)
        owner_blade = next(card for card in combat.hand if card.card_id == CardId.SOVEREIGN_BLADE)
        ally_hand = combat._ally_player_zones[ally]["hand"]  # noqa: SLF001
        ally_blade = next(card for card in ally_hand if card.card_id == CardId.SOVEREIGN_BLADE)
        # C# SpoilsOfBattle.cs:15: ForgeVar(5) => blade 10 + 5.
        assert owner_blade.base_damage == 15
        assert ally_blade.base_damage == 15

    def test_monologue_gains_strength_per_card_played_then_resets_end_of_turn(self):
        combat = _make_combat()
        combat.hand = [make_monologue_card(), make_venerate(), make_defend_regent()]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.MONOLOGUE) == 1
        assert combat.player.get_power_amount(PowerId.STRENGTH) == 0

        assert combat.play_card(0)
        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.STRENGTH) == 2

        fire_after_turn_end(CombatSide.PLAYER, combat)
        assert combat.player.get_power_amount(PowerId.STRENGTH) == 0
        assert combat.player.get_power_amount(PowerId.MONOLOGUE) == 0

    def test_monologue_keeps_separate_instances_like_reference(self):
        combat = _make_combat()
        combat.hand = [make_monologue_card(), make_monologue_card(), make_venerate()]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.play_card(0)
        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.STRENGTH) == 3

        fire_after_turn_end(CombatSide.PLAYER, combat)
        assert combat.player.get_power_amount(PowerId.STRENGTH) == 0
        assert combat.player.get_power_amount(PowerId.MONOLOGUE) == 0

    def test_comet_deals_damage_and_applies_weak_and_vulnerable(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        combat.hand = [make_comet()]
        combat.energy = 0
        combat.gain_stars(combat.player, 5)

        assert combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - 33
        assert enemy.get_power_amount(PowerId.WEAK) == 3
        assert enemy.get_power_amount(PowerId.VULNERABLE) == 3

    def test_shining_strike_deals_damage_gains_stars_and_returns_to_draw_pile(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        card = make_shining_strike()
        combat.hand = [card]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - 8
        assert combat.stars == 2
        assert combat.draw_pile[0] is card

    def test_shining_strike_does_not_return_to_draw_pile_after_damage_ends_combat(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = 8
        card = make_shining_strike()
        existing_top = combat.draw_pile[0]
        combat.hand = [card]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert combat.is_over
        assert combat.draw_pile[0] is existing_top
        assert card not in combat.draw_pile

    def test_bombardment_autoplays_from_exhaust_before_hand_draw_and_stays_exhausted(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.max_hp = 100
        enemy.current_hp = 100
        card = make_bombardment()
        card.owner = combat.player
        combat.hand = []
        combat.draw_pile = []
        combat.exhaust_pile = [card]

        combat._apply_card_before_hand_draw(combat.player)  # noqa: SLF001

        assert enemy.current_hp == 82
        assert card in combat.exhaust_pile

    def test_i_am_invincible_autoplays_from_top_of_draw_pile_after_turn_end(self):
        combat = _make_combat()
        card = make_i_am_invincible()
        card.owner = combat.player
        combat.player.block = 0
        combat.hand = []
        combat.draw_pile = [card]

        combat._apply_card_after_turn_end(CombatSide.PLAYER)  # noqa: SLF001

        # IAmInvincible.cs CanonicalVars: BlockVar(10m, ValueProp.Move) in v0.107.1 (was 9).
        assert combat.player.block == 10
        assert card in combat.discard_pile

    def test_kingly_kick_gets_cheaper_each_time_it_is_drawn(self):
        combat = _make_combat()
        card = make_kingly_kick()
        card.owner = combat.player
        combat.hand = []
        combat.draw_pile = [card]

        combat.draw_cards(combat.player, 1)
        assert card.cost == 3

        combat.hand.remove(card)
        combat.draw_pile = [card]
        combat.draw_cards(combat.player, 1)
        assert card.cost == 2

    def test_kingly_punch_damage_increases_when_drawn_not_when_played(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.max_hp = 100
        enemy.current_hp = 100
        card = make_kingly_punch()
        card.owner = combat.player
        combat.hand = []
        combat.draw_pile = [card]

        combat.draw_cards(combat.player, 1)
        # KinglyPunch.cs: DamageVar(8m), DynamicVar("Increase", 4m) in v0.107.1
        # (Increase was 3 in the older decompile) -> 8 + 4 = 12 after draw.
        assert card.base_damage == 12

        combat.energy = 1
        assert combat.play_card(0, 0)
        assert enemy.current_hp == 88
        assert card.base_damage == 12

    def test_kingly_punch_upgrade_preserves_drawn_damage_growth(self):
        combat = _make_combat()
        card = make_kingly_punch()
        card.owner = combat.player
        combat.hand = []
        combat.draw_pile = [card]

        combat.draw_cards(combat.player, 1)
        # KinglyPunch.cs v0.107.1: draw adds Increase 4 (8 + 4 = 12); OnUpgrade
        # does Damage.UpgradeValueBy(2) and Increase.UpgradeValueBy(2) on the
        # current values -> 14 / 6 (drawn growth is preserved).
        assert card.base_damage == 12

        combat.upgrade_card(card)

        assert card.upgraded is True
        assert card.base_damage == 14
        assert card.effect_vars["increase"] == 6
