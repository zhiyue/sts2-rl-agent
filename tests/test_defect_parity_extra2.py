"""Additional Defect parity tests for remaining high-signal orb cards."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.defect import (
    create_defect_starter_deck,
    make_ball_lightning,
    make_barrage,
    make_capacitor,
    make_chaos,
    make_compile_driver,
    make_defragment,
    make_glacier,
    make_multi_cast,
    make_strike_defect,
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


class TestDefectParityExtra2:
    def test_ball_lightning_deals_damage_and_channels_lightning(self):
        """Matches BallLightning.cs: attack target, then channel one Lightning orb."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        combat.hand = [make_ball_lightning()]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - 7
        assert len(combat.orb_queue.orbs) == 1
        assert combat.orb_queue.orbs[0].orb_type == OrbType.LIGHTNING

    def test_barrage_hits_once_per_current_orb(self):
        """Matches Barrage.cs: hit count equals current orb count."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        combat.channel_orb(combat.player, "LIGHTNING")
        combat.channel_orb(combat.player, "FROST")
        combat.hand = [make_barrage()]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - 10

    def test_capacitor_adds_orb_slots_up_to_cap(self):
        """Matches Capacitor.cs: add Repeat slots, clamped by queue max capacity."""
        combat = _make_combat()
        combat.orb_queue.capacity = 9
        combat.hand = [make_capacitor()]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.orb_queue.capacity == 10

    def test_chaos_channels_random_orb_for_each_repeat(self):
        """Matches Chaos.cs: channel random orb Repeat times."""
        combat = _make_combat()
        card = make_chaos()
        card.effect_vars["repeat"] = 2
        combat.hand = [card]
        combat.energy = 1

        assert combat.play_card(0)
        assert len(combat.orb_queue.orbs) == 2
        assert {orb.orb_type for orb in combat.orb_queue.orbs}.issubset(
            {OrbType.LIGHTNING, OrbType.FROST, OrbType.DARK, OrbType.PLASMA, OrbType.GLASS}
        )

    def test_compile_driver_draws_for_distinct_orb_types_only(self):
        """Matches CompileDriver.cs: draw count uses distinct orb ids, not total orb count."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        draw_a = make_strike_defect()
        draw_b = make_strike_defect()
        draw_c = make_strike_defect()
        combat.channel_orb(combat.player, "LIGHTNING")
        combat.channel_orb(combat.player, "LIGHTNING")
        combat.channel_orb(combat.player, "FROST")
        combat.hand = [make_compile_driver()]
        combat.draw_pile = [draw_a, draw_b, draw_c]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert combat.hand == [draw_a, draw_b]

    def test_defragment_applies_focus_that_scales_orb_passives(self):
        """Matches Defragment.cs: apply Focus to owner."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        combat.channel_orb(combat.player, "LIGHTNING")
        combat.hand = [make_defragment()]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.FOCUS) == 1

        combat.end_player_turn()
        assert enemy.current_hp == starting_hp - 4

    def test_glacier_grants_block_and_channels_two_frost_orbs(self):
        """Matches Glacier.cs: gain block, then channel Frost twice."""
        combat = _make_combat()
        combat.hand = [make_glacier()]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.player.block == 6
        assert len(combat.orb_queue.orbs) == 2
        assert all(orb.orb_type == OrbType.FROST for orb in combat.orb_queue.orbs)

    def test_multi_cast_uses_x_energy_for_repeated_front_evoke(self):
        """Matches MultiCast.cs: evoke front orb X times and spend all energy."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        combat.channel_orb(combat.player, "LIGHTNING")
        combat.hand = [make_multi_cast()]
        combat.energy = 3

        assert combat.play_card(0)
        assert combat.energy == 0
        assert enemy.current_hp == starting_hp - 24
        assert len(combat.orb_queue.orbs) == 0
