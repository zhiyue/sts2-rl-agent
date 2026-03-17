"""Additional Defect parity tests backed by decompiled card models."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.defect import (
    create_defect_starter_deck,
    make_charge_battery,
    make_coolheaded,
    make_dualcast,
    make_loop,
    make_strike_defect,
    make_sunder,
)
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import OrbType, PowerId
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle


def _make_combat() -> CombatState:
    combat = CombatState(
        player_hp=75,
        player_max_hp=75,
        deck=create_defect_starter_deck(),
        rng_seed=42,
        character_id="Defect",
    )
    creature, ai = create_shrinker_beetle(Rng(42))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


class TestDefectParityExtra:
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

    def test_sunder_refunds_energy_only_when_target_is_killed(self):
        """Matches Sunder.cs: gain 3 energy only if damage result includes a kill."""
        kill_combat = _make_combat()
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
