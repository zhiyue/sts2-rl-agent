"""Tests for power mechanics: ticking, persistence, damage/block modification."""

import pytest

from sts2_env.core.creature import Creature
from sts2_env.core.enums import CardId, CombatSide, PowerId, ValueProp
from sts2_env.core.damage import calculate_block, calculate_damage
from sts2_env.powers.remaining_c import SandpitPower


class TestPowerApplication:
    """Stacking, negative amounts, Artifact blocking."""

    def test_strength_stacks(self, player):
        player.apply_power(PowerId.STRENGTH, 2)
        player.apply_power(PowerId.STRENGTH, 3)
        assert player.get_power_amount(PowerId.STRENGTH) == 5

    def test_vulnerable_stacks(self, enemy):
        enemy.apply_power(PowerId.VULNERABLE, 2)
        enemy.apply_power(PowerId.VULNERABLE, 3)
        assert enemy.get_power_amount(PowerId.VULNERABLE) == 5

    def test_strength_allows_negative(self, player):
        player.apply_power(PowerId.STRENGTH, 3)
        player.apply_power(PowerId.STRENGTH, -5)
        assert player.get_power_amount(PowerId.STRENGTH) == -2

    def test_artifact_blocks_debuff(self, player):
        player.apply_power(PowerId.ARTIFACT, 1)
        player.apply_power(PowerId.VULNERABLE, 3)
        assert not player.has_power(PowerId.VULNERABLE)
        assert not player.has_power(PowerId.ARTIFACT)  # consumed

    def test_artifact_blocks_multiple(self, player):
        player.apply_power(PowerId.ARTIFACT, 2)
        player.apply_power(PowerId.VULNERABLE, 1)
        player.apply_power(PowerId.WEAK, 1)
        assert not player.has_power(PowerId.VULNERABLE)
        assert not player.has_power(PowerId.WEAK)
        assert not player.has_power(PowerId.ARTIFACT)  # 2 consumed


class TestDebuffTicking:
    """Vulnerable/Weak/Frail tick down at enemy turn end only; Strength does not tick."""

    def test_vulnerable_ticks_down(self, enemy):
        enemy.apply_power(PowerId.VULNERABLE, 3)
        enemy.tick_down_power(PowerId.VULNERABLE)
        assert enemy.get_power_amount(PowerId.VULNERABLE) == 2

    def test_vulnerable_removed_at_zero(self, enemy):
        enemy.apply_power(PowerId.VULNERABLE, 1)
        enemy.tick_down_power(PowerId.VULNERABLE)
        assert not enemy.has_power(PowerId.VULNERABLE)

    def test_skip_first_tick(self, enemy):
        """Debuffs applied during player turn skip first tick."""
        enemy.apply_power(PowerId.VULNERABLE, 2)
        p = enemy.powers[PowerId.VULNERABLE]
        p.skip_next_tick = True
        enemy.tick_down_power(PowerId.VULNERABLE)
        assert enemy.get_power_amount(PowerId.VULNERABLE) == 2  # skipped
        enemy.tick_down_power(PowerId.VULNERABLE)
        assert enemy.get_power_amount(PowerId.VULNERABLE) == 1  # now ticked

    def test_weak_ticks_down(self, player):
        player.apply_power(PowerId.WEAK, 2)
        player.tick_down_power(PowerId.WEAK)
        assert player.get_power_amount(PowerId.WEAK) == 1

    def test_frail_ticks_down(self, player):
        player.apply_power(PowerId.FRAIL, 2)
        player.tick_down_power(PowerId.FRAIL)
        assert player.get_power_amount(PowerId.FRAIL) == 1

    def test_strength_does_not_tick(self, player):
        """Strength is permanent -- tick_down_power is a no-op."""
        player.apply_power(PowerId.STRENGTH, 3)
        player.tick_down_power(PowerId.STRENGTH)
        assert player.get_power_amount(PowerId.STRENGTH) == 3


class TestDebuffTickTiming:
    """Debuffs tick at the end of the ENEMY turn, not the player turn.

    When a debuff is applied via combat.apply_power_to() during the player
    turn, skip_next_tick is set, so the first enemy-turn-end tick is skipped.
    """

    def test_debuffs_tick_after_enemy_turn(self, simple_combat):
        """Vulnerable applied to ENEMY during player turn.

        Per C# PowerCmd.Apply: skip_next_tick is only set when
        target.Side == CombatSide.Player. Since the target is an enemy,
        skip is NOT set, so the debuff ticks normally.
        """
        enemy = simple_combat.enemies[0]
        simple_combat.apply_power_to(enemy, PowerId.VULNERABLE, 2)
        assert enemy.get_power_amount(PowerId.VULNERABLE) == 2

        # End turn 1: enemy is not player-side, no skip -> ticks 2->1
        simple_combat.end_player_turn()
        assert enemy.get_power_amount(PowerId.VULNERABLE) == 1

        # End turn 2: ticks 1->0 (removed)
        simple_combat.end_player_turn()
        assert not enemy.has_power(PowerId.VULNERABLE)

    def test_strength_persists_across_turns(self, simple_combat):
        """Strength never ticks down across multiple turns."""
        simple_combat.player.apply_power(PowerId.STRENGTH, 5)
        assert simple_combat.player.get_power_amount(PowerId.STRENGTH) == 5
        simple_combat.end_player_turn()
        assert simple_combat.player.get_power_amount(PowerId.STRENGTH) == 5
        simple_combat.end_player_turn()
        assert simple_combat.player.get_power_amount(PowerId.STRENGTH) == 5

    def test_debuff_on_player_skips_first_tick(self, simple_combat):
        """Debuffs applied TO the player skip first tick (C# checks target.Side == Player)."""
        player = simple_combat.player
        simple_combat.apply_power_to(player, PowerId.VULNERABLE, 2)
        assert player.get_power_amount(PowerId.VULNERABLE) == 2
        # End turn 1: player is player-side, skip IS set -> stays at 2
        simple_combat.end_player_turn()
        assert player.get_power_amount(PowerId.VULNERABLE) == 2
        # End turn 2: now ticks 2->1
        simple_combat.end_player_turn()
        assert player.get_power_amount(PowerId.VULNERABLE) == 1


class TestDamageModifierInteractions:
    def test_cruelty_increases_vulnerable_multiplier(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        player.apply_power(PowerId.CRUELTY, 25)
        enemy.apply_power(PowerId.VULNERABLE, 1)

        damage = calculate_damage(10, player, enemy, ValueProp.MOVE, simple_combat)
        assert damage == 17

    def test_debilitate_modifies_weak_and_vulnerable_excess(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]

        enemy.apply_power(PowerId.DEBILITATE, 1)
        enemy.apply_power(PowerId.VULNERABLE, 1)
        assert calculate_damage(10, player, enemy, ValueProp.MOVE, simple_combat) == 20

        enemy.powers.pop(PowerId.DEBILITATE, None)
        enemy.powers.pop(PowerId.VULNERABLE, None)
        player.apply_power(PowerId.DEBILITATE, 1)
        player.apply_power(PowerId.WEAK, 1)
        assert calculate_damage(10, player, enemy, ValueProp.MOVE, simple_combat) == 5


class TestSandpitPower:
    def test_sandpit_kills_target_player_when_count_reaches_zero(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        sandpit = SandpitPower(1)
        sandpit.target = player
        enemy.powers[PowerId.SANDPIT] = sandpit

        sandpit.after_side_turn_start(enemy, CombatSide.ENEMY, simple_combat)

        assert player.is_dead


class TestPowerAmountChangedHooks:
    def test_shroud_gains_block_when_owner_applies_doom(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        player.apply_power(PowerId.SHROUD, 3)

        simple_combat.apply_power_to(enemy, PowerId.DOOM, 2)

        assert player.block == 3

    def test_vicious_draws_when_owner_applies_vulnerable(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        player.apply_power(PowerId.VICIOUS, 2)
        simple_combat.hand.clear()

        simple_combat.apply_power_to(enemy, PowerId.VULNERABLE, 1)

        assert len(simple_combat.hand) == 2

    def test_outbreak_triggers_every_third_poison_application(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        player.apply_power(PowerId.OUTBREAK, 4)
        starting_hp = enemy.current_hp

        simple_combat.apply_power_to(enemy, PowerId.POISON, 1)
        simple_combat.apply_power_to(enemy, PowerId.POISON, 1)
        assert enemy.current_hp == starting_hp

        simple_combat.apply_power_to(enemy, PowerId.POISON, 1)
        assert enemy.current_hp == starting_hp - 4

    def test_painful_stabs_adds_wounds_to_player_discard_on_unblocked_hit(self, simple_combat):
        enemy = simple_combat.enemies[0]
        player = simple_combat.player
        enemy.apply_power(PowerId.PAINFUL_STABS, 2)

        simple_combat.deal_damage(
            dealer=enemy,
            target=player,
            amount=5,
            props=ValueProp.MOVE,
        )

        wounds = [card for card in simple_combat.discard_pile if card.card_id == CardId.WOUND]
        assert len(wounds) == 2

    def test_poison_triggers_multiple_times_with_opponent_accelerant(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        player.apply_power(PowerId.ACCELERANT, 2)
        enemy.apply_power(PowerId.POISON, 3)

        poison = enemy.powers[PowerId.POISON]
        starting_hp = enemy.current_hp
        poison.after_side_turn_start(enemy, CombatSide.ENEMY, simple_combat)

        assert enemy.current_hp == starting_hp - 6
        assert not enemy.has_power(PowerId.POISON)

    def test_reflect_deals_blocked_damage_back_to_attacker(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        player.apply_power(PowerId.REFLECT, 1)
        player.block = 3
        starting_enemy_hp = enemy.current_hp

        simple_combat.deal_damage(
            dealer=enemy,
            target=player,
            amount=5,
            props=ValueProp.MOVE,
        )

        assert enemy.current_hp == starting_enemy_hp - 3

    def test_no_block_only_blocks_card_sourced_block(self, simple_combat):
        player = simple_combat.player
        player.apply_power(PowerId.NO_BLOCK, 1)

        card_block = calculate_block(8, player, ValueProp.MOVE, simple_combat, card_source=object())
        non_card_block = calculate_block(8, player, ValueProp.MOVE, simple_combat, card_source=None)

        assert card_block == 0
        assert non_card_block == 8


class TestDexterityBlock:
    """Dexterity adds to block; Frail multiplies block by 0.75."""

    def test_dexterity_adds_to_block(self, player):
        player.apply_power(PowerId.DEXTERITY, 3)
        block = calculate_block(5, player, ValueProp.MOVE, [player])
        assert block == 8  # 5 + 3

    def test_frail_reduces_block(self, player):
        """Frail x0.75 block."""
        player.apply_power(PowerId.FRAIL, 2)
        block = calculate_block(8, player, ValueProp.MOVE, [player])
        assert block == 6  # floor(8 * 0.75) = 6

    def test_dexterity_and_frail_combined(self, player):
        """Dexterity additive first, then Frail multiplicative."""
        player.apply_power(PowerId.DEXTERITY, 3)
        player.apply_power(PowerId.FRAIL, 2)
        block = calculate_block(5, player, ValueProp.MOVE, [player])
        # (5 + 3) * 0.75 = 6.0 -> 6
        assert block == 6


class TestCreatureBlock:
    """Block cap, clear, and heal cap."""

    def test_clear_block(self, player):
        player.gain_block(10)
        player.clear_block()
        assert player.block == 0

    def test_block_capped_at_999(self, player):
        player.gain_block(1000)
        assert player.block == 999

    def test_heal_capped_at_max(self, player):
        player.current_hp = 50
        healed = player.heal(100)
        assert player.current_hp == 80  # max_hp
        assert healed == 30
