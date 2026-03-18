"""Additional Defect parity tests for remaining high-signal card behaviors."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.defect import (
    create_defect_starter_deck,
    make_cold_snap,
    make_defend_defect,
    make_go_for_the_eyes,
    make_lightning_rod,
    make_machine_learning,
    make_reboot,
    make_signal_boost,
    make_strike_defect,
    make_tempest,
    make_thunder,
)
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import OrbType, PowerId
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle, create_twig_slime_s


def _make_combat(monster_factory=create_shrinker_beetle) -> CombatState:
    combat = CombatState(
        player_hp=75,
        player_max_hp=75,
        deck=create_defect_starter_deck(),
        rng_seed=42,
        character_id="Defect",
    )
    creature, ai = monster_factory(Rng(42))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


class TestDefectParityExtra3:
    def test_cold_snap_deals_damage_and_channels_frost(self):
        """Matches ColdSnap.cs: attack target, then channel one Frost orb."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        combat.hand = [make_cold_snap()]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == starting_hp - 6
        assert len(combat.orb_queue.orbs) == 1
        assert combat.orb_queue.orbs[0].orb_type == OrbType.FROST

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
        upgraded = make_tempest()
        upgraded.upgraded = True
        upgraded_combat.hand = [upgraded]
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
