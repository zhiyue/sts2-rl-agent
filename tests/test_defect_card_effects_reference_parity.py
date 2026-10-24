"""Reference parity tests for Defect card effects."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.defect import (
    DEFEND_DEFECT_BLOCK,
    DEFEND_DEFECT_COST,
    DEFEND_DEFECT_UPGRADED_BLOCK,
    make_adaptive_strike,
    make_buffer,
    create_defect_starter_deck,
    make_all_for_one,
    make_biased_cognition,
    make_boost_away,
    make_boot_sequence,
    make_bulk_up,
    make_claw,
    make_cold_snap,
    make_consuming_shadow,
    make_creative_ai,
    make_darkness,
    make_defend_defect,
    make_double_energy,
    make_echo_form,
    make_fight_through,
    make_flak_cannon,
    make_ftl,
    make_fusion,
    make_genetic_algorithm,
    make_glasswork,
    make_go_for_the_eyes,
    make_gunk_up,
    make_helix_drill,
    make_hyperbeam,
    make_ice_lance,
    make_iteration,
    make_leap,
    make_lightning_rod,
    make_machine_learning,
    make_meteor_strike,
    make_momentum_strike,
    make_modded,
    make_null,
    make_overclock,
    make_quadcast,
    make_rainbow,
    make_refract,
    make_reboot,
    make_rocket_punch,
    make_shatter,
    make_shadow_shield,
    make_signal_boost,
    make_skim,
    make_spinner,
    make_strike_defect,
    make_synthesis,
    make_sweeping_beam,
    make_tempest,
    make_tesla_coil,
    make_thunder,
    make_uproar,
    make_voltaic,
    STRIKE_DEFECT_COST,
    STRIKE_DEFECT_DAMAGE,
    STRIKE_DEFECT_UPGRADED_DAMAGE,
    ZAP_COST,
    ZAP_UPGRADED_COST,
    make_zap,
)
from sts2_env.cards.factory import create_card
from sts2_env.cards.status import make_burn, make_dazed, make_slimed
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import CardId, CardTag, OrbType, PowerId, ValueProp
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle, create_twig_slime_s
from sts2_env.powers.base import PowerInstance


BUFFER_POWER_AMOUNT = 1
BUFFER_UPGRADED_POWER_AMOUNT = 2
ECHO_FORM_POWER_AMOUNT = 1
ITERATION_POWER_AMOUNT = 2
ITERATION_UPGRADED_POWER_AMOUNT = 3
SPINNER_POWER_AMOUNT = 1
NULL_DAMAGE = 10
NULL_UPGRADED_DAMAGE = 13
NULL_WEAK = 2
NULL_UPGRADED_WEAK = 3
RAINBOW_ORBS = (OrbType.LIGHTNING, OrbType.FROST, OrbType.DARK)
TEST_ENEMY_HP = 100
EXHAUST_KEYWORD = "exhaust"
COLD_SNAP_DAMAGE = 6
COLD_SNAP_UPGRADED_DAMAGE = 9
FIGHT_THROUGH_BLOCK = 13
FIGHT_THROUGH_UPGRADED_BLOCK = 17
FIGHT_THROUGH_WOUND_COUNT = 2
REFRACT_DAMAGE = 9
REFRACT_UPGRADED_DAMAGE = 12
REFRACT_HITS = 2
REFRACT_GLASS_ORBS = 2
ICE_LANCE_DAMAGE = 19
ICE_LANCE_UPGRADED_DAMAGE = 24
ICE_LANCE_FROST_ORBS = 3


class _CannotHitPower(PowerInstance):
    def __init__(self):
        super().__init__(PowerId.COVERED, 1)

    def should_allow_hitting(self, owner, combat):
        return False


def _make_combat(monster_factory=create_shrinker_beetle, *, extra_enemies: int = 0) -> CombatState:
    combat = CombatState(
        player_hp=75,
        player_max_hp=75,
        deck=create_defect_starter_deck(),
        rng_seed=42,
        character_id="Defect",
    )
    creature, ai = monster_factory(Rng(42))
    combat.add_enemy(creature, ai)
    for i in range(extra_enemies):
        extra_creature, extra_ai = create_shrinker_beetle(Rng(100 + i))
        combat.add_enemy(extra_creature, extra_ai)
    combat.start_combat()
    return combat


class TestDefectCardEffectsReferenceParity:
    def test_strike_defect_matches_reference_damage_and_upgrade(self):
        combat = _make_combat(monster_factory=create_twig_slime_s)
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        card = make_strike_defect()
        combat.hand = [card]
        combat.energy = STRIKE_DEFECT_COST

        assert card.cost == STRIKE_DEFECT_COST
        assert card.original_cost == STRIKE_DEFECT_COST
        assert card.base_damage == STRIKE_DEFECT_DAMAGE
        assert CardTag.STRIKE in card.tags
        assert combat.play_card(0, 0)

        assert enemy.current_hp == starting_hp - STRIKE_DEFECT_DAMAGE
        assert make_strike_defect(upgraded=True).base_damage == STRIKE_DEFECT_UPGRADED_DAMAGE

    def test_defend_defect_matches_reference_block_and_upgrade(self):
        combat = _make_combat()
        card = make_defend_defect()
        combat.hand = [card]
        combat.energy = DEFEND_DEFECT_COST

        assert card.cost == DEFEND_DEFECT_COST
        assert card.original_cost == DEFEND_DEFECT_COST
        assert card.base_block == DEFEND_DEFECT_BLOCK
        assert CardTag.DEFEND in card.tags
        assert combat.play_card(0)

        assert combat.player.block == DEFEND_DEFECT_BLOCK
        assert make_defend_defect(upgraded=True).base_block == DEFEND_DEFECT_UPGRADED_BLOCK

    def test_zap_matches_reference_channel_and_upgrade_cost(self):
        combat = _make_combat()
        card = make_zap()
        combat.hand = [card]
        combat.energy = ZAP_COST

        assert card.cost == ZAP_COST
        assert card.original_cost == ZAP_COST
        assert combat.play_card(0)

        assert [orb.orb_type for orb in combat.orb_queue.orbs] == [OrbType.LIGHTNING]
        upgraded = make_zap(upgraded=True)
        assert upgraded.cost == ZAP_UPGRADED_COST
        assert upgraded.original_cost == ZAP_UPGRADED_COST

    def test_leap_gains_block(self):
        combat = _make_combat()
        combat.hand = [make_leap()]
        combat.energy = 1

        assert combat.play_card(0)

        assert combat.player.block == 9

    def test_boot_sequence_gains_block_and_exhausts(self):
        combat = _make_combat()
        card = make_boot_sequence()
        combat.hand = [card]
        combat.energy = 0

        assert combat.play_card(0)

        assert combat.player.block == 10
        assert card in combat.exhaust_pile

    def test_bulk_up_removes_slot_and_grants_strength_dexterity(self):
        combat = _make_combat()
        combat.orb_queue.capacity = 3
        combat.hand = [make_bulk_up()]
        combat.energy = 2

        assert combat.play_card(0)

        assert combat.orb_queue.capacity == 2
        assert combat.player.get_power_amount(PowerId.STRENGTH) == 2
        assert combat.player.get_power_amount(PowerId.DEXTERITY) == 2

    def test_double_energy_gains_energy_equal_to_remaining_energy_after_cost(self):
        combat = _make_combat()
        combat.hand = [make_double_energy()]
        combat.energy = 3

        assert combat.play_card(0)

        assert combat.energy == 4

    def test_fusion_channels_plasma(self):
        combat = _make_combat()
        combat.hand = [make_fusion()]
        combat.energy = 2

        assert combat.play_card(0)

        assert len(combat.orb_queue.orbs) == 1
        assert combat.orb_queue.orbs[0].orb_type == OrbType.PLASMA

    def test_glasswork_gains_block_and_channels_glass(self):
        combat = _make_combat()
        combat.hand = [make_glasswork()]
        combat.energy = 1

        assert combat.play_card(0)

        assert combat.player.block == 5
        assert len(combat.orb_queue.orbs) == 1
        assert combat.orb_queue.orbs[0].orb_type == OrbType.GLASS

    def test_null_deals_damage_applies_weak_and_channels_dark(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.max_hp = TEST_ENEMY_HP
        enemy.current_hp = TEST_ENEMY_HP
        combat.hand = [make_null()]
        combat.energy = 2

        assert combat.play_card(0, 0)
        assert enemy.current_hp == TEST_ENEMY_HP - NULL_DAMAGE
        assert enemy.get_power_amount(PowerId.WEAK) == NULL_WEAK
        assert [orb.orb_type for orb in combat.orb_queue.orbs] == [OrbType.DARK]

        upgraded_combat = _make_combat()
        upgraded_enemy = upgraded_combat.enemies[0]
        upgraded_enemy.max_hp = TEST_ENEMY_HP
        upgraded_enemy.current_hp = TEST_ENEMY_HP
        upgraded_combat.hand = [make_null(upgraded=True)]
        upgraded_combat.energy = 2

        assert upgraded_combat.play_card(0, 0)
        assert upgraded_enemy.current_hp == TEST_ENEMY_HP - NULL_UPGRADED_DAMAGE
        assert upgraded_enemy.get_power_amount(PowerId.WEAK) == NULL_UPGRADED_WEAK
        assert [orb.orb_type for orb in upgraded_combat.orb_queue.orbs] == [OrbType.DARK]

    def test_rainbow_channels_lightning_frost_dark_and_upgrade_removes_exhaust(self):
        combat = _make_combat()
        card = make_rainbow()
        combat.hand = [card]
        combat.energy = 2

        assert combat.play_card(0)
        assert [orb.orb_type for orb in combat.orb_queue.orbs] == list(RAINBOW_ORBS)
        assert card in combat.exhaust_pile

        upgraded = make_rainbow(upgraded=True)
        assert EXHAUST_KEYWORD not in upgraded.keywords

    def test_cold_snap_deals_damage_and_channels_frost(self):
        """Matches ColdSnap.cs: attack target, then channel one Frost orb."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        combat.hand = [make_cold_snap()]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - COLD_SNAP_DAMAGE
        assert len(combat.orb_queue.orbs) == 1
        assert combat.orb_queue.orbs[0].orb_type == OrbType.FROST

    def test_upgraded_cold_snap_deals_nine_damage_and_still_channels_frost(self):
        """Matches ColdSnap.cs: upgrade changes Damage only."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        combat.hand = [make_cold_snap(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - COLD_SNAP_UPGRADED_DAMAGE
        assert [orb.orb_type for orb in combat.orb_queue.orbs] == [OrbType.FROST]

    def test_sweeping_beam_hits_only_hittable_enemies_then_draws(self):
        combat = _make_combat(extra_enemies=1)
        blocked, hittable = combat.enemies
        blocked.current_hp = blocked.max_hp = 100
        hittable.current_hp = hittable.max_hp = 100
        blocked.powers[PowerId.COVERED] = _CannotHitPower()
        drawn = make_strike_defect()
        combat.hand = [make_sweeping_beam()]
        combat.draw_pile = [drawn]
        combat.energy = 1

        assert combat.play_card(0)

        assert blocked.current_hp == 100
        assert hittable.current_hp == 94
        assert drawn in combat.hand

    def test_flak_cannon_random_hits_use_only_hittable_enemies(self):
        combat = _make_combat(extra_enemies=1)
        blocked, hittable = combat.enemies
        blocked.current_hp = blocked.max_hp = 100
        hittable.current_hp = hittable.max_hp = 100
        blocked.powers[PowerId.COVERED] = _CannotHitPower()
        combat.hand = [make_flak_cannon(), make_dazed(), make_strike_defect()]
        combat.draw_pile = [make_burn()]
        combat.energy = 2

        assert combat.play_card(0)

        assert blocked.current_hp == 100
        assert hittable.current_hp == 84

    def test_genetic_algorithm_starts_at_one_block_and_grows_after_play(self):
        """Matches GeneticAlgorithm.cs: CurrentBlock starts at 1 and grows by Increase after play."""
        combat = _make_combat()
        card = make_genetic_algorithm()
        combat.hand = [card]
        combat.energy = 1

        assert card.base_block == 1
        assert card.effect_vars["block"] == 1

        assert combat.play_card(0)

        assert combat.player.block == 1
        assert card.base_block == 4
        assert card.effect_vars["block"] == 4

    def test_upgraded_genetic_algorithm_grows_by_four(self):
        combat = _make_combat()
        card = create_card(CardId.GENETIC_ALGORITHM, upgraded=True)
        combat.hand = [card]
        combat.energy = 1

        assert card.effect_vars["increase"] == 4

        assert combat.play_card(0)

        assert combat.player.block == 1
        assert card.base_block == 5
        assert card.effect_vars["block"] == 5

    def test_genetic_algorithm_upgrade_preserves_grown_block(self):
        combat = _make_combat()
        card = make_genetic_algorithm()
        combat.hand = [card]
        combat.energy = 1

        assert combat.play_card(0)
        assert card.base_block == 4

        combat.upgrade_card(card)

        assert card.upgraded is True
        assert card.effect_vars["increase"] == 4
        assert card.base_block == 4
        assert card.effect_vars["block"] == 4

    def test_claw_upgrade_preserves_shared_growth_for_all_owner_copies(self):
        combat = _make_combat()
        played = make_claw()
        in_draw = make_claw()
        in_discard = make_claw()
        combat.hand = [played]
        combat.draw_pile = [in_draw]
        combat.discard_pile = [in_discard]

        assert combat.play_card(0, 0)

        assert played.base_damage == 5
        assert in_draw.base_damage == 5
        assert in_discard.base_damage == 5

        combat.upgrade_card(in_draw)

        assert in_draw.upgraded is True
        assert in_draw.base_damage == 6
        assert in_draw.effect_vars["increase"] == 3

    def test_hyperbeam_hits_only_hittable_enemies_and_loses_focus(self):
        combat = _make_combat(extra_enemies=1)
        blocked, hittable = combat.enemies
        blocked.current_hp = blocked.max_hp = 100
        hittable.current_hp = hittable.max_hp = 100
        blocked.powers[PowerId.COVERED] = _CannotHitPower()
        combat.apply_power_to(combat.player, PowerId.FOCUS, 4)
        combat.hand = [make_hyperbeam()]
        combat.energy = 2

        assert combat.play_card(0)

        # Hyperbeam.cs CanonicalVars: DamageVar(28m, ValueProp.Move) in v0.107.1
        # (was 26 in the older decompile); no damage modifiers present, so flat 28.
        assert blocked.current_hp == 100
        assert hittable.current_hp == 72
        assert combat.player.get_power_amount(PowerId.FOCUS) == 1

    def test_shatter_hits_only_hittable_enemies(self):
        combat = _make_combat(extra_enemies=1)
        blocked, hittable = combat.enemies
        blocked.current_hp = blocked.max_hp = 100
        hittable.current_hp = hittable.max_hp = 100
        blocked.powers[PowerId.COVERED] = _CannotHitPower()
        combat.hand = [make_shatter()]
        combat.energy = 1

        assert combat.play_card(0)

        # Shatter.cs CanonicalVars: DamageVar(7m, ValueProp.Move) in v0.107.1 (was 11).
        assert blocked.current_hp == 100
        assert hittable.current_hp == 93

    def test_go_for_the_eyes_applies_weak_only_if_target_intends_attack(self):
        """Matches GoForTheEyes.cs: Weak is conditional on target attack intent."""
        no_attack_combat = _make_combat(create_shrinker_beetle)
        no_attack_enemy = no_attack_combat.enemies[0]
        no_attack_hp = no_attack_enemy.current_hp
        no_attack_combat.hand = [make_go_for_the_eyes()]
        no_attack_combat.energy = 0

        assert no_attack_combat.play_card(0, 0)
        assert no_attack_enemy.current_hp == no_attack_hp - 3
        assert no_attack_enemy.get_power_amount(PowerId.WEAK) == 0

        attack_combat = _make_combat(create_twig_slime_s)
        attack_enemy = attack_combat.enemies[0]
        attack_hp = attack_enemy.current_hp
        attack_combat.hand = [make_go_for_the_eyes()]
        attack_combat.energy = 0

        assert attack_combat.play_card(0, 0)
        assert attack_enemy.current_hp == attack_hp - 3
        assert attack_enemy.get_power_amount(PowerId.WEAK) == 1

    def test_reboot_shuffles_current_hand_into_draw_and_redraws(self):
        """Matches Reboot.cs: move hand to draw, shuffle, then draw configured cards."""
        combat = _make_combat()
        reboot = make_reboot()
        held_a = make_strike_defect()
        held_b = make_defend_defect()
        draw_a = make_strike_defect()
        draw_b = make_defend_defect()
        combat.hand = [reboot, held_a, held_b]
        combat.draw_pile = [draw_a, draw_b]
        combat.discard_pile = []
        combat.energy = 0

        assert combat.play_card(0)
        assert reboot in combat.exhaust_pile
        assert len(combat.hand) == 4
        assert {id(card) for card in combat.hand} == {id(held_a), id(held_b), id(draw_a), id(draw_b)}
        assert combat.draw_pile == []

    def test_skim_draws_three_cards(self):
        combat = _make_combat()
        drawn = [make_strike_defect(), make_defend_defect(), make_strike_defect()]
        combat.hand = [make_skim()]
        combat.draw_pile = list(drawn)
        combat.energy = 1

        assert combat.play_card(0)

        assert combat.hand == drawn

    def test_all_for_one_returns_only_zero_cost_non_x_attack_skill_power_cards(self):
        """Matches AllForOne.cs: discard filter is zero energy, non-X, Attack/Skill/Power only."""
        combat = _make_combat()
        zero_attack = make_strike_defect()
        zero_attack.cost = 0
        zero_skill = make_defend_defect()
        zero_skill.cost = 0
        x_attack = make_tempest()
        zero_status = make_slimed()
        zero_status.cost = 0
        costly_attack = make_strike_defect()
        for discarded in [zero_attack, zero_skill, x_attack, zero_status, costly_attack]:
            discarded.owner = combat.player
        combat.hand = [make_all_for_one()]
        combat.discard_pile = [zero_attack, zero_skill, x_attack, zero_status, costly_attack]
        combat.energy = 2

        assert combat.play_card(0, 0)
        assert zero_attack in combat.hand
        assert zero_skill in combat.hand
        assert x_attack in combat.discard_pile
        assert zero_status in combat.discard_pile
        assert costly_attack in combat.discard_pile

    def test_all_for_one_does_not_return_discard_cards_after_damage_ends_combat(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = 10
        zero_attack = make_strike_defect()
        zero_attack.cost = 0
        zero_skill = make_defend_defect()
        zero_skill.cost = 0
        for discarded in [zero_attack, zero_skill]:
            discarded.owner = combat.player
        combat.hand = [make_all_for_one()]
        combat.discard_pile = [zero_attack, zero_skill]
        combat.energy = 2

        assert combat.play_card(0, 0)
        assert combat.is_over
        assert zero_attack in combat.discard_pile
        assert zero_skill in combat.discard_pile
        assert zero_attack not in combat.hand
        assert zero_skill not in combat.hand

    def test_boost_away_adds_owned_generated_dazed_to_discard(self):
        """Matches BoostAway.cs: generated Dazed is added to the owner's discard pile."""
        combat = _make_combat()
        combat.hand = [make_boost_away()]
        combat.energy = 0

        assert combat.play_card(0)
        dazed = [card for card in combat.discard_pile if card.card_id.name == "DAZED"]
        assert len(dazed) == 1
        assert dazed[0].owner is combat.player

    def test_gunk_up_hits_three_times_and_adds_owned_slimed_to_discard(self):
        """Matches GunkUp.cs: 3 hits, then generated Slimed to discard."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        combat.hand = [make_gunk_up()]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - 12
        slimed = [card for card in combat.discard_pile if card.card_id.name == "SLIMED"]
        assert len(slimed) == 1
        assert slimed[0].owner is combat.player

    def test_fight_through_adds_two_owned_wounds_to_discard(self):
        """Matches FightThrough.cs: gain block, then add 2 generated Wounds."""
        combat = _make_combat()
        combat.hand = [make_fight_through()]
        combat.energy = 1

        assert combat.play_card(0)
        wounds = [card for card in combat.discard_pile if card.card_id.name == "WOUND"]
        assert combat.player.block == FIGHT_THROUGH_BLOCK
        assert len(wounds) == FIGHT_THROUGH_WOUND_COUNT
        assert all(card.owner is combat.player for card in wounds)

    def test_upgraded_fight_through_gains_seventeen_block_and_still_adds_two_wounds(self):
        """Matches FightThrough.cs: upgrade changes Block only."""
        combat = _make_combat()
        combat.hand = [make_fight_through(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0)
        wounds = [card for card in combat.discard_pile if card.card_id.name == "WOUND"]
        assert combat.player.block == FIGHT_THROUGH_UPGRADED_BLOCK
        assert len(wounds) == FIGHT_THROUGH_WOUND_COUNT

    def test_overclock_draws_then_adds_owned_burn_to_discard(self):
        """Matches Overclock.cs: draw cards, then add generated Burn to discard."""
        combat = _make_combat()
        drawn_a = make_strike_defect()
        drawn_b = make_defend_defect()
        combat.hand = [make_overclock()]
        combat.draw_pile = [drawn_a, drawn_b]
        combat.energy = 0

        assert combat.play_card(0)
        assert drawn_a in combat.hand
        assert drawn_b in combat.hand
        burns = [card for card in combat.discard_pile if card.card_id.name == "BURN"]
        assert len(burns) == 1
        assert burns[0].owner is combat.player

    def test_darkness_triggers_only_dark_orb_passives(self):
        """Matches Darkness.cs: channel Dark, then trigger Dark orbs only."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        combat.channel_orb(combat.player, "LIGHTNING")
        combat.channel_orb(combat.player, "DARK")
        existing_dark = combat.orb_queue.orbs[1]
        combat.hand = [make_darkness()]
        combat.energy = 1

        assert combat.play_card(0)
        dark_orbs = [orb for orb in combat.orb_queue.orbs if orb.orb_type == OrbType.DARK]
        assert enemy.current_hp == starting_hp
        assert existing_dark.get_evoke_value(combat) == 12
        assert len(dark_orbs) == 2
        assert all(orb.get_evoke_value(combat) == 12 for orb in dark_orbs)

    def test_upgraded_darkness_triggers_dark_orb_passives_twice(self):
        """Matches Darkness.cs: upgraded Darkness passives Dark orbs twice."""
        combat = _make_combat()
        combat.channel_orb(combat.player, "DARK")
        existing_dark = combat.orb_queue.orbs[0]
        combat.hand = [make_darkness(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0)
        dark_orbs = [orb for orb in combat.orb_queue.orbs if orb.orb_type == OrbType.DARK]
        assert existing_dark.get_evoke_value(combat) == 18
        assert len(dark_orbs) == 2
        assert all(orb.get_evoke_value(combat) == 18 for orb in dark_orbs)

    def test_refract_hits_twice_and_channels_two_glass_orbs(self):
        """Matches Refract.cs: 2 damage hits, then channel 2 Glass."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        combat.hand = [make_refract()]
        combat.energy = 3

        assert combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - REFRACT_DAMAGE * REFRACT_HITS
        assert len(combat.orb_queue.orbs) == REFRACT_GLASS_ORBS
        assert all(orb.orb_type == OrbType.GLASS for orb in combat.orb_queue.orbs)

    def test_upgraded_refract_hits_twice_with_twelve_damage(self):
        """Matches Refract.cs: upgrade changes Damage only."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        combat.hand = [make_refract(upgraded=True)]
        combat.energy = 3

        assert combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - REFRACT_UPGRADED_DAMAGE * REFRACT_HITS
        assert len(combat.orb_queue.orbs) == REFRACT_GLASS_ORBS

    def test_lightning_orb_damage_is_unpowered(self):
        """Matches LightningOrb.cs: orb damage uses ValueProp.Unpowered."""
        combat = _make_combat()
        combat.channel_orb(combat.player, "LIGHTNING")

        combat.orb_queue.trigger_before_turn_end(combat)

        assert combat._damage_events_combat[-1][2] == ValueProp.UNPOWERED

    def test_dark_orb_evoke_targets_lowest_hittable_enemy(self):
        """Matches DarkOrb.cs: evoke picks the lowest-HP hittable enemy."""
        combat = _make_combat(extra_enemies=1)
        blocked, hittable = combat.enemies
        blocked.current_hp = blocked.max_hp = 10
        hittable.current_hp = hittable.max_hp = 100
        blocked.powers[PowerId.COVERED] = _CannotHitPower()
        combat.channel_orb(combat.player, "DARK")

        combat.orb_queue.evoke_front(combat)

        assert blocked.current_hp == 10
        assert hittable.current_hp == 94
        assert combat._damage_events_combat[-1][2] == ValueProp.UNPOWERED

    def test_glass_orb_passive_hits_only_hittable_enemies(self):
        """Matches GlassOrb.cs: passive damages HittableEnemies."""
        combat = _make_combat(extra_enemies=1)
        blocked, hittable = combat.enemies
        blocked.current_hp = blocked.max_hp = 100
        hittable.current_hp = hittable.max_hp = 100
        blocked.powers[PowerId.COVERED] = _CannotHitPower()
        combat.channel_orb(combat.player, "GLASS")

        combat.orb_queue.trigger_before_turn_end(combat)

        assert blocked.current_hp == 100
        assert hittable.current_hp == 96
        assert combat._damage_events_combat[-1][2] == ValueProp.UNPOWERED

    def test_ice_lance_channels_three_frost_orbs_after_damage(self):
        """Matches IceLance.cs: damage target, then channel 3 Frost."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        combat.hand = [make_ice_lance()]
        combat.energy = 3

        assert combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - ICE_LANCE_DAMAGE
        assert len(combat.orb_queue.orbs) == ICE_LANCE_FROST_ORBS
        assert all(orb.orb_type == OrbType.FROST for orb in combat.orb_queue.orbs)

    def test_upgraded_ice_lance_deals_twenty_four_damage_and_still_channels_three_frost(self):
        """Matches IceLance.cs: upgrade changes Damage only."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        combat.hand = [make_ice_lance(upgraded=True)]
        combat.energy = 3

        assert combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - ICE_LANCE_UPGRADED_DAMAGE
        assert len(combat.orb_queue.orbs) == ICE_LANCE_FROST_ORBS
        assert all(orb.orb_type == OrbType.FROST for orb in combat.orb_queue.orbs)

    def test_helix_drill_hits_once_per_prior_energy_spent_this_turn(self):
        """Matches HelixDrill.cs: hit count uses owner energy spent this turn, excluding this card."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        combat.hand = [make_defend_defect(), make_strike_defect(), make_helix_drill()]
        combat.energy = 3

        assert combat.play_card(0)
        assert combat.play_card(0, 0)
        assert combat.play_card(0, 0)

        assert enemy.current_hp == 100 - 6 - 6

    def test_helix_drill_can_hit_zero_times(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        combat.hand = [make_helix_drill()]
        combat.energy = 0

        assert combat.play_card(0, 0)

        assert enemy.current_hp == 100

    def test_helix_drill_excludes_its_own_modified_cost(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        drill = make_helix_drill()
        drill.cost = 2
        combat.hand = [drill]
        combat.energy = 2

        assert combat.play_card(0, 0)

        assert enemy.current_hp == 100

    def test_voltaic_channels_one_lightning_per_owner_lightning_channel_history(self):
        """Matches Voltaic.cs: count all owner Lightning channels in combat history."""
        combat = _make_combat()
        combat.channel_orb(combat.player, "LIGHTNING")
        combat.channel_orb(combat.player, "FROST")
        combat.channel_orb(combat.player, "LIGHTNING")
        combat.channel_orb(combat.player, "LIGHTNING")
        combat.hand = [make_voltaic()]
        combat.energy = 3  # Voltaic.cs: base(3, ...) energy cost in v0.107.1 (was 2)

        assert combat.play_card(0)

        assert len(combat.orb_queue.orbs) == 3
        assert all(orb.orb_type == OrbType.LIGHTNING for orb in combat.orb_queue.orbs)

    def test_voltaic_can_channel_zero_lightning(self):
        combat = _make_combat()
        combat.channel_orb(combat.player, "FROST")
        combat.hand = [make_voltaic()]
        combat.energy = 3  # Voltaic.cs: base(3, ...) energy cost in v0.107.1 (was 2)

        assert combat.play_card(0)

        assert len(combat.orb_queue.orbs) == 1
        assert combat.orb_queue.orbs[0].orb_type == OrbType.FROST

    def test_ftl_draws_only_before_owner_has_finished_three_card_plays(self):
        """Matches Ftl.cs: draw is gated by owner card plays finished this turn."""
        combat = _make_combat()
        drawn = make_strike_defect()
        combat.hand = [make_claw(), make_go_for_the_eyes(), make_boost_away(), make_ftl()]
        combat.draw_pile = [drawn]
        combat.energy = 0

        assert combat.play_card(0, 0)
        assert combat.play_card(0, 0)
        assert combat.play_card(0)
        assert combat.play_card(0, 0)
        assert drawn in combat.draw_pile
        assert drawn not in combat.hand

    def test_momentum_strike_sets_its_own_cost_to_zero_this_combat(self):
        """Matches MomentumStrike.cs: after damage, this card costs 0 for the combat."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        card = make_momentum_strike()
        combat.hand = [card]
        combat.energy = 1

        assert combat.play_card(0, 0)

        assert enemy.current_hp == starting_hp - 10
        assert card.cost == 0

    def test_tesla_coil_triggers_only_lightning_passives_against_target(self):
        """Matches TeslaCoil.cs: only Lightning orbs passive, and they hit the card target."""
        combat = _make_combat(extra_enemies=1)
        target, other = combat.enemies
        target.current_hp = target.max_hp = 100
        other.current_hp = other.max_hp = 100
        combat.channel_orb(combat.player, "LIGHTNING")
        combat.channel_orb(combat.player, "FROST")
        combat.channel_orb(combat.player, "DARK")
        combat.hand = [make_tesla_coil()]
        combat.energy = 0

        assert combat.play_card(0, 0)

        assert target.current_hp == 100 - 3 - 3
        assert other.current_hp == 100
        assert combat.player.block == 0
        assert combat.orb_queue.orbs[2].get_evoke_value(combat) == 6

    def test_adaptive_strike_adds_zero_cost_copy_to_discard(self):
        """Matches AdaptiveStrike.cs: after damage, add a 0-cost copy to discard."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        card = make_adaptive_strike()
        combat.hand = [card]
        combat.energy = 2

        assert combat.play_card(0, 0)

        copies = [
            discarded
            for discarded in combat.discard_pile
            if discarded.card_id == CardId.ADAPTIVE_STRIKE and discarded is not card
        ]
        assert enemy.current_hp == starting_hp - 18
        assert len(copies) == 1
        assert copies[0].cost == 0
        assert copies[0].owner is combat.player

    def test_adaptive_strike_clone_does_not_consume_combat_rng(self):
        """Matches AdaptiveStrike.cs: CreateClone does not advance the combat RNG."""
        combat = _make_combat()
        card = make_adaptive_strike()
        combat.hand = [card]
        combat.energy = 2
        counter = combat.rng.counter

        assert combat.play_card(0, 0)

        assert combat.rng.counter == counter

    def test_shadow_shield_gains_block_and_channels_dark(self):
        combat = _make_combat()
        combat.hand = [make_shadow_shield()]
        combat.energy = 2

        assert combat.play_card(0)

        assert combat.player.block == 11
        assert len(combat.orb_queue.orbs) == 1
        assert combat.orb_queue.orbs[0].orb_type == OrbType.DARK

    def test_synthesis_damages_and_grants_next_power_free(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        combat.hand = [make_synthesis()]
        combat.energy = 2

        assert combat.play_card(0, 0)

        # Synthesis.cs CanonicalVars: DamageVar(14m, ValueProp.Move) in v0.107.1 (was 12).
        assert enemy.current_hp == starting_hp - 14
        assert combat.player.get_power_amount(PowerId.FREE_POWER) == 1

    def test_meteor_strike_deals_damage_and_channels_three_plasma(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        combat.hand = [make_meteor_strike()]
        combat.energy = 5

        assert combat.play_card(0, 0)

        assert enemy.current_hp == 76
        assert len(combat.orb_queue.orbs) == 3
        assert all(orb.orb_type == OrbType.PLASMA for orb in combat.orb_queue.orbs)

    def test_modded_adds_slot_draws_and_raises_own_cost(self):
        combat = _make_combat()
        card = make_modded()
        drawn = make_strike_defect()
        combat.orb_queue.capacity = 3
        combat.hand = [card]
        combat.draw_pile = [drawn]
        combat.energy = 0

        assert combat.play_card(0)

        assert combat.orb_queue.capacity == 4
        assert drawn in combat.hand
        assert card.cost == 1

    def test_quadcast_triggers_front_orb_four_times_and_removes_once(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        combat.channel_orb(combat.player, "LIGHTNING")
        combat.hand = [make_quadcast()]
        combat.energy = 1

        assert combat.play_card(0)

        assert enemy.current_hp == 68
        assert not combat.orb_queue.orbs

    def test_uproar_hits_twice_then_autoplays_draw_pile_attack(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        attack = make_strike_defect()
        combat.hand = [make_uproar()]
        combat.draw_pile = [attack]
        combat.energy = 2

        assert combat.play_card(0, 0)

        # Uproar.cs CanonicalVars: DamageVar(6m, ValueProp.Move) in v0.107.1 (was 5),
        # WithHitCount(2) => 2 x 6, then autoplay Strike (6).
        assert enemy.current_hp == 100 - 12 - 6
        assert attack in combat.discard_pile

    def test_tempest_uses_x_energy_and_upgrade_adds_one_channel(self):
        """Matches Tempest.cs: channel X Lightning, and X+1 when upgraded."""
        base_combat = _make_combat()
        base_combat.hand = [make_tempest()]
        base_combat.energy = 3

        assert base_combat.play_card(0)
        assert base_combat.energy == 0
        assert len(base_combat.orb_queue.orbs) == 3
        assert all(orb.orb_type == OrbType.LIGHTNING for orb in base_combat.orb_queue.orbs)

        upgraded_combat = _make_combat()
        upgraded_combat.hand = [make_tempest(upgraded=True)]
        upgraded_combat.energy = 2

        assert upgraded_combat.play_card(0)
        assert upgraded_combat.energy == 0
        assert len(upgraded_combat.orb_queue.orbs) == 3
        assert all(orb.orb_type == OrbType.LIGHTNING for orb in upgraded_combat.orb_queue.orbs)

    def test_thunder_applies_thunder_power(self):
        """Matches Thunder.cs: apply ThunderPower to owner."""
        combat = _make_combat()
        combat.hand = [make_thunder()]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.THUNDER) == 6

    def test_thunder_adds_damage_to_lightning_orb_evoke_targets(self):
        """Matches ThunderPower.cs: after Lightning evoke, damage the living evoke targets."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        combat.player.apply_power(PowerId.THUNDER, 6)
        combat.channel_orb(combat.player, "LIGHTNING")

        combat.orb_queue.evoke_front(combat)

        assert enemy.current_hp == 86

    def test_thunder_does_not_damage_dark_orb_evoke_targets(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        combat.player.apply_power(PowerId.THUNDER, 6)
        combat.channel_orb(combat.player, "DARK")

        combat.orb_queue.evoke_front(combat)

        assert enemy.current_hp == 94

    def test_lightning_rod_grants_block_and_channels_lightning_each_turn_while_decrementing(self):
        """Matches LightningRod.cs + LightningRodPower: block now, channel Lightning at turn start."""
        combat = _make_combat()
        combat.hand = [make_lightning_rod()]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.block == 4
        assert combat.player.get_power_amount(PowerId.LIGHTNING_ROD) == 2

        combat.end_player_turn()

        assert len(combat.orb_queue.orbs) == 1
        assert combat.orb_queue.orbs[0].orb_type == OrbType.LIGHTNING
        assert combat.player.get_power_amount(PowerId.LIGHTNING_ROD) == 1

    def test_machine_learning_increases_next_turn_hand_draw(self):
        """Matches MachineLearning.cs + MachineLearningPower: ModifyHandDraw by +Cards each turn."""
        combat = _make_combat()
        draw_cards = [make_strike_defect() for _ in range(8)]
        combat.hand = [make_machine_learning()]
        combat.draw_pile = list(draw_cards)
        combat.discard_pile = []
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.MACHINE_LEARNING) == 1

        combat.end_player_turn()

        assert len(combat.hand) == 6
        assert combat.hand == draw_cards[:6]

    def test_signal_boost_replays_next_power_card_once(self):
        """Matches SignalBoost.cs + SignalBoostPower: next Power card gets +1 play count."""
        combat = _make_combat()
        combat.hand = [make_signal_boost(), make_machine_learning()]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.SIGNAL_BOOST) == 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.MACHINE_LEARNING) == 2
        assert combat.player.get_power_amount(PowerId.SIGNAL_BOOST) == 0

    def test_buffer_card_applies_reference_power_amounts(self):
        combat = _make_combat()
        combat.hand = [make_buffer()]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.BUFFER) == BUFFER_POWER_AMOUNT

        upgraded_combat = _make_combat()
        upgraded_combat.hand = [make_buffer(upgraded=True)]
        upgraded_combat.energy = 2

        assert upgraded_combat.play_card(0)
        assert upgraded_combat.player.get_power_amount(PowerId.BUFFER) == BUFFER_UPGRADED_POWER_AMOUNT

    def test_buffer_prevents_next_hp_loss_and_then_decrements(self):
        combat = _make_combat()
        combat.hand = [make_buffer()]
        combat.energy = 2

        assert combat.play_card(0)
        combat.deal_damage(combat.enemies[0], combat.player, 7)

        assert combat.player.current_hp == combat.player.max_hp
        assert combat.player.get_power_amount(PowerId.BUFFER) == 0

    def test_echo_form_card_applies_reference_power_and_upgrade_removes_ethereal(self):
        card = make_echo_form()
        assert card.is_ethereal
        combat = _make_combat()
        combat.hand = [card]
        combat.energy = 3

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.ECHO_FORM) == ECHO_FORM_POWER_AMOUNT

        upgraded = make_echo_form(upgraded=True)
        assert upgraded.is_ethereal is False

    def test_iteration_card_applies_reference_power_amounts(self):
        combat = _make_combat()
        combat.hand = [make_iteration()]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.ITERATION) == ITERATION_POWER_AMOUNT

        upgraded_combat = _make_combat()
        upgraded_combat.hand = [make_iteration(upgraded=True)]
        upgraded_combat.energy = 1

        assert upgraded_combat.play_card(0)
        assert upgraded_combat.player.get_power_amount(PowerId.ITERATION) == ITERATION_UPGRADED_POWER_AMOUNT

    def test_iteration_draws_only_after_first_status_drawn_this_turn(self):
        combat = _make_combat()
        first_status = make_dazed()
        second_status = make_burn()
        drawn_from_iteration = make_strike_defect()
        undrawn = make_defend_defect()
        combat.hand = [make_iteration()]
        combat.draw_pile = [first_status, drawn_from_iteration, second_status, undrawn]
        combat.energy = 1

        assert combat.play_card(0)
        combat.draw_cards(combat.player, 1)

        assert combat.hand == [first_status, drawn_from_iteration, second_status]
        assert combat.draw_pile == [undrawn]

        combat.draw_cards(combat.player, 1)

        assert combat.hand == [first_status, drawn_from_iteration, second_status, undrawn]
        assert combat.draw_pile == []

    def test_rocket_punch_costs_zero_until_played_after_status_generation(self):
        """RocketPunch.cs AfterCardGeneratedForCombat (v0.107.1): each friendly
        Status generation calls base.EnergyCost.SetUntilPlayed(0) — a WhenPlayed
        LocalCostModifier on RocketPunch itself (CardEnergyCost.cs:175), so it
        costs 0 until it is next played, then reverts to its printed cost."""
        combat = _make_combat()
        card = make_rocket_punch()
        combat.hand = [card]
        combat.energy = 1

        combat.add_generated_card_to_creature_hand(combat.player, make_burn())
        assert card.cost == 0

        combat.energy = 5
        assert combat.play_card(0, 0)
        assert card.cost == 2
        assert "_until_played_cost" not in card.combat_vars

    def test_rocket_punch_zero_cost_survives_end_of_turn_cleanup(self):
        """RocketPunch.cs uses SetUntilPlayed (WhenPlayed), not a this-turn
        modifier — the 0 cost persists through end-of-turn cleanup until the
        card is played (RocketPunch.cs:2736-2752, CardEnergyCost.cs:175)."""
        combat = _make_combat()
        card = make_rocket_punch()
        combat.hand = [card]
        combat.energy = 1

        combat.add_generated_card_to_creature_hand(combat.player, make_slimed())
        assert card.cost == 0

        combat._cleanup_cards_end_of_turn()
        assert card.cost == 0

    def test_spinner_base_and_upgraded_immediate_channel_match_reference(self):
        combat = _make_combat()
        combat.hand = [make_spinner()]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.SPINNER) == SPINNER_POWER_AMOUNT
        assert combat.orb_queue.orbs == []

        upgraded_combat = _make_combat()
        upgraded_combat.hand = [make_spinner(upgraded=True)]
        upgraded_combat.energy = 1

        assert upgraded_combat.play_card(0)
        assert upgraded_combat.player.get_power_amount(PowerId.SPINNER) == SPINNER_POWER_AMOUNT
        assert [orb.orb_type for orb in upgraded_combat.orb_queue.orbs] == [OrbType.GLASS]

    def test_spinner_channels_glass_on_next_energy_reset(self):
        combat = _make_combat()
        combat.hand = [make_spinner()]
        combat.energy = 1

        assert combat.play_card(0)
        combat.end_player_turn()

        assert [orb.orb_type for orb in combat.orb_queue.orbs] == [OrbType.GLASS]

    def test_biased_cognition_applies_focus_then_delayed_focus_loss(self):
        """Matches BiasedCognition.cs: apply Focus(4), then BiasedCognition(1)."""
        combat = _make_combat()
        combat.hand = [make_biased_cognition()]
        combat.energy = 1

        assert combat.play_card(0)

        assert combat.player.get_power_amount(PowerId.FOCUS) == 4
        assert combat.player.get_power_amount(PowerId.BIASED_COGNITION) == 1

        combat.end_player_turn()

        assert combat.player.get_power_amount(PowerId.FOCUS) == 3

    def test_creative_ai_applies_power(self):
        """Matches CreativeAI.cs: apply CreativeAiPower(1)."""
        combat = _make_combat()
        combat.hand = [make_creative_ai()]
        combat.energy = 3

        assert combat.play_card(0)

        assert combat.player.get_power_amount(PowerId.CREATIVE_AI) == 1

    def test_consuming_shadow_channels_dark_orbs_then_applies_power(self):
        """Matches ConsumingShadow.cs: channel Repeat Dark orbs, then apply its power."""
        combat = _make_combat()
        combat.hand = [make_consuming_shadow()]
        combat.energy = 2

        assert combat.play_card(0)

        assert [orb.orb_type for orb in combat.orb_queue.orbs] == [OrbType.DARK, OrbType.DARK]
        assert combat.player.get_power_amount(PowerId.CONSUMING_SHADOW) == 1

    def test_upgraded_consuming_shadow_channels_three_dark_orbs(self):
        """Matches ConsumingShadow.cs OnUpgrade: Repeat increases from 2 to 3."""
        combat = _make_combat()
        combat.hand = [create_card(CardId.CONSUMING_SHADOW, upgraded=True)]
        combat.energy = 2

        assert combat.play_card(0)

        assert [orb.orb_type for orb in combat.orb_queue.orbs] == [
            OrbType.DARK,
            OrbType.DARK,
            OrbType.DARK,
        ]
        assert combat.player.get_power_amount(PowerId.CONSUMING_SHADOW) == 1

    def test_consuming_shadow_evokes_last_orb_at_turn_end(self):
        """Matches ConsumingShadowPower.cs: evoke the last orb Amount times on owner turn end."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        combat.hand = [make_consuming_shadow()]
        combat.energy = 2

        assert combat.play_card(0)
        front, last = combat.orb_queue.orbs
        front._accumulated_evoke = 20
        last._accumulated_evoke = 6

        combat.end_player_turn()

        assert enemy.current_hp == 88
        assert combat.orb_queue.orbs == [front]

    def test_defect_power_card_upgrades_match_original(self):
        """Matches BiasedCognition, CreativeAI, and MachineLearning OnUpgrade methods."""
        biased = create_card(CardId.BIASED_COGNITION_CARD, upgraded=True)
        creative_ai = create_card(CardId.CREATIVE_AI_CARD, upgraded=True)
        machine_learning = create_card(CardId.MACHINE_LEARNING_CARD, upgraded=True)

        assert biased.effect_vars["focus_power"] == 5
        assert biased.cost == 1
        assert creative_ai.cost == 2
        assert creative_ai.effect_vars["creative_ai"] == 1
        assert machine_learning.cost == 1
        assert machine_learning.is_innate is True
