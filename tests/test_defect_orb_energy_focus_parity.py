"""Additional Defect parity tests backed by decompiled card models."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.defect import (
    create_defect_starter_deck,
    make_charge_battery,
    make_coolheaded,
    make_dualcast,
    make_focused_strike,
    make_hotfix,
    make_loop,
    make_multi_cast,
    make_scrape,
    make_storm,
    make_strike_defect,
    make_synchronize,
    make_sunder,
)
from sts2_env.cards.regent import make_stardust
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import CombatSide, OrbType, PowerId
from sts2_env.core.hooks import fire_after_turn_end
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle


def _make_combat(*, extra_enemies: int = 0) -> CombatState:
    combat = CombatState(
        player_hp=75,
        player_max_hp=75,
        deck=create_defect_starter_deck(),
        rng_seed=42,
        character_id="Defect",
    )
    creature, ai = create_shrinker_beetle(Rng(42))
    combat.add_enemy(creature, ai)
    for i in range(extra_enemies):
        extra_creature, extra_ai = create_shrinker_beetle(Rng(43 + i))
        combat.add_enemy(extra_creature, extra_ai)
    combat.start_combat()
    return combat


class TestDefectOrbEnergyFocusParity:
    def test_charge_battery_grants_block_and_energy_next_turn_power(self):
        """Matches ChargeBattery.cs: gain block, then apply EnergyNextTurn."""
        combat = _make_combat()
        combat.hand = [make_charge_battery()]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.block == 7
        assert combat.player.get_power_amount(PowerId.ENERGY_NEXT_TURN) == 1

    def test_coolheaded_channels_frost_and_draws_cards(self):
        """Matches Coolheaded.cs: channel Frost, then draw configured cards."""
        combat = _make_combat()
        drawn = make_strike_defect()
        combat.hand = [make_coolheaded()]
        combat.draw_pile = [drawn]
        combat.energy = 1

        assert combat.play_card(0)
        assert len(combat.hand) == 1
        assert combat.hand[0] is drawn
        assert len(combat.orb_queue.orbs) == 1
        assert combat.orb_queue.orbs[0].orb_type == OrbType.FROST

    def test_dualcast_evokes_front_orb_twice_and_removes_it(self):
        """Matches Dualcast.cs: evoke front orb once without dequeue, then once with dequeue."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        combat.channel_orb(combat.player, "LIGHTNING")
        combat.hand = [make_dualcast()]
        combat.energy = 1

        assert combat.play_card(0)
        assert enemy.current_hp == starting_hp - 16
        assert not combat.orb_queue.orbs

    def test_dualcast_stops_before_second_evoke_after_combat_ends(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        combat.channel_orb(combat.player, "LIGHTNING")
        enemy.current_hp = 8
        combat.hand = [make_dualcast()]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.is_over
        assert len(combat.orb_queue.orbs) == 1
        assert combat.orb_queue.orbs[0].orb_type == OrbType.LIGHTNING

    def test_scrape_discards_only_nonzero_energy_cost_or_costs_x_cards(self):
        """Matches Scrape.cs: drawn cards are discarded when
        c.EnergyCost.GetWithModifiers(CostModifiers.Local) != 0 ||
        c.EnergyCost.CostsX. Star costs are StarCost, not EnergyCost, so
        0-energy-cost star and star-X cards stay in hand (Scrape.cs:25-28)."""
        combat = _make_combat()
        zero_cost = make_strike_defect()
        zero_cost.cost = 0
        star_cost = make_strike_defect()
        star_cost.cost = 0
        star_cost.star_cost = 1
        star_x = make_stardust()
        assert star_x.has_star_cost_x is True
        assert star_x.cost == 0
        energy_x = make_multi_cast()
        assert energy_x.has_energy_cost_x is True
        combat.hand = [make_scrape()]
        # Scrape draws 4 cards (Scrape.cs CardsVar(4)).
        combat.draw_pile = [zero_cost, star_cost, star_x, energy_x]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert zero_cost in combat.hand
        assert star_cost in combat.hand
        assert star_x in combat.hand
        assert energy_x in combat.discard_pile

    def test_scrape_does_not_discard_existing_hand_cards_after_damage_ends_combat(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = 7
        kept = make_strike_defect()
        kept.cost = 1
        combat.hand = [make_scrape(), kept]
        combat.draw_pile = []
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert combat.is_over
        assert kept in combat.hand
        assert kept not in combat.discard_pile

    def test_loop_triggers_first_orb_passive_again_on_next_turn_start(self):
        """Matches Loop.cs + LoopPower: first orb passive triggers extra times each turn."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        combat.channel_orb(combat.player, "LIGHTNING")
        combat.hand = [make_loop()]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.LOOP) == 1

        combat.end_player_turn()

        assert enemy.current_hp == starting_hp - 6

    def test_storm_does_not_trigger_from_first_storm_play(self):
        combat = _make_combat()
        combat.hand = [make_storm()]
        combat.energy = 1

        assert combat.play_card(0)

        assert combat.player.get_power_amount(PowerId.STORM) == 1
        assert combat.orb_queue.orbs == []

    def test_storm_uses_amount_from_before_card_played(self):
        combat = _make_combat()
        combat.apply_power_to(combat.player, PowerId.STORM, 2)
        combat.hand = [make_storm()]
        combat.energy = 1

        assert combat.play_card(0)

        assert combat.player.get_power_amount(PowerId.STORM) == 3
        assert len(combat.orb_queue.orbs) == 2
        assert all(orb.orb_type == OrbType.LIGHTNING for orb in combat.orb_queue.orbs)

    def test_sunder_refunds_energy_only_when_target_is_killed(self):
        """Matches Sunder.cs: gain 3 energy only if damage result includes a kill."""
        kill_combat = _make_combat(extra_enemies=1)
        kill_enemy = kill_combat.enemies[0]
        kill_enemy.current_hp = 20
        kill_enemy.max_hp = 20
        kill_combat.hand = [make_sunder()]
        kill_combat.energy = 3

        assert kill_combat.play_card(0, 0)
        assert kill_enemy.is_dead
        assert kill_combat.energy == 3

        no_kill_combat = _make_combat()
        no_kill_enemy = no_kill_combat.enemies[0]
        no_kill_enemy.current_hp = 30
        no_kill_enemy.max_hp = 30
        no_kill_combat.hand = [make_sunder()]
        no_kill_combat.energy = 3

        assert no_kill_combat.play_card(0, 0)
        assert not no_kill_enemy.is_dead
        assert no_kill_combat.energy == 0

    def test_sunder_does_not_refund_after_ending_combat(self):
        kill_combat = _make_combat()
        kill_enemy = kill_combat.enemies[0]
        kill_enemy.current_hp = 20
        kill_enemy.max_hp = 20
        kill_combat.hand = [make_sunder()]
        kill_combat.energy = 3

        assert kill_combat.play_card(0, 0)

        assert kill_combat.is_over
        assert kill_combat.energy == 0

    def test_hotfix_grants_temporary_focus_until_turn_end(self):
        combat = _make_combat()
        combat.hand = [make_hotfix(), make_hotfix()]
        combat.energy = 0

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.HOTFIX) == 2
        assert combat.player.get_power_amount(PowerId.FOCUS) == 2

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.HOTFIX) == 4
        assert combat.player.get_power_amount(PowerId.FOCUS) == 4

        fire_after_turn_end(CombatSide.PLAYER, combat)

        assert combat.player.get_power_amount(PowerId.HOTFIX) == 0
        assert combat.player.get_power_amount(PowerId.FOCUS) == 0

    def test_synchronize_grants_temporary_focus_from_distinct_orb_types(self):
        combat = _make_combat()
        combat.channel_orb(combat.player, "LIGHTNING")
        combat.channel_orb(combat.player, "LIGHTNING")
        combat.channel_orb(combat.player, "FROST")
        combat.hand = [make_synchronize(), make_synchronize()]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.SYNCHRONIZE) == 4
        assert combat.player.get_power_amount(PowerId.FOCUS) == 4

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.SYNCHRONIZE) == 8
        assert combat.player.get_power_amount(PowerId.FOCUS) == 8

        fire_after_turn_end(CombatSide.PLAYER, combat)

        assert combat.player.get_power_amount(PowerId.SYNCHRONIZE) == 0
        assert combat.player.get_power_amount(PowerId.FOCUS) == 0

    def test_focused_strike_grants_focus_immediately_until_turn_end(self):
        combat = _make_combat(extra_enemies=1)
        enemy = combat.enemies[0]
        combat.hand = [make_focused_strike(), make_focused_strike()]
        combat.energy = 2

        assert combat.play_card(0, 0)
        assert enemy.current_hp == enemy.max_hp - 9
        assert combat.player.get_power_amount(PowerId.FOCUSED_STRIKE) == 1
        assert combat.player.get_power_amount(PowerId.FOCUS) == 1

        assert combat.play_card(0, 0)
        assert combat.player.get_power_amount(PowerId.FOCUSED_STRIKE) == 2
        assert combat.player.get_power_amount(PowerId.FOCUS) == 2

        fire_after_turn_end(CombatSide.PLAYER, combat)

        assert combat.player.get_power_amount(PowerId.FOCUSED_STRIKE) == 0
        assert combat.player.get_power_amount(PowerId.FOCUS) == 0
