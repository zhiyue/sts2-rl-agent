"""Tests for the damage and block calculation pipelines."""

import math
import pytest

from sts2_env.core.creature import Creature
from sts2_env.core.damage import calculate_damage, calculate_block, apply_damage
from sts2_env.core.enums import CombatSide, PowerId, ValueProp


class TestDamagePipeline:
    """Damage calculation: additive (Str) then multiplicative (Vuln/Weak)."""

    def test_base_damage_no_modifiers(self, player, enemy):
        """Base damage with no modifiers returns the base value unchanged."""
        dmg = calculate_damage(6, player, enemy, ValueProp.MOVE, [player, enemy])
        assert dmg == 6

    def test_base_damage_various_values(self, player, enemy):
        """Different base damage values pass through unchanged."""
        for base in (0, 1, 10, 50, 100):
            dmg = calculate_damage(base, player, enemy, ValueProp.MOVE, [player, enemy])
            assert dmg == base

    def test_strength_adds_to_damage(self, player, enemy):
        """Strength adds its amount to powered attack damage."""
        player.apply_power(PowerId.STRENGTH, 3)
        dmg = calculate_damage(6, player, enemy, ValueProp.MOVE, [player, enemy])
        assert dmg == 9  # 6 + 3

    def test_vulnerable_multiplies_damage(self, player, enemy):
        """Vulnerable on the target multiplies damage by 1.5."""
        enemy.apply_power(PowerId.VULNERABLE, 2)
        dmg = calculate_damage(10, player, enemy, ValueProp.MOVE, [player, enemy])
        assert dmg == 15  # 10 * 1.5

    def test_weak_reduces_damage(self, player, enemy):
        """Weak on the dealer multiplies damage by 0.75."""
        player.apply_power(PowerId.WEAK, 2)
        dmg = calculate_damage(10, player, enemy, ValueProp.MOVE, [player, enemy])
        assert dmg == 7  # floor(10 * 0.75) = 7

    def test_strength_then_vulnerable_combined(self, player, enemy):
        """10 base + 3 Str on Vulnerable target = floor((10+3)*1.5) = 19."""
        player.apply_power(PowerId.STRENGTH, 3)
        enemy.apply_power(PowerId.VULNERABLE, 2)
        dmg = calculate_damage(10, player, enemy, ValueProp.MOVE, [player, enemy])
        assert dmg == 19  # floor((10 + 3) * 1.5) = floor(19.5) = 19

    def test_weak_and_vulnerable_interaction(self, player, enemy):
        """10 base, dealer Weak (0.75x), target Vulnerable (1.5x)."""
        player.apply_power(PowerId.WEAK, 2)
        enemy.apply_power(PowerId.VULNERABLE, 2)
        dmg = calculate_damage(10, player, enemy, ValueProp.MOVE, [player, enemy])
        # 10 * 0.75 * 1.5 = 11.25 -> floor = 11
        assert dmg == 11

    def test_unpowered_ignores_strength_weak_vulnerable(self, player, enemy):
        """UNPOWERED props bypass Strength, Weak, and Vulnerable."""
        player.apply_power(PowerId.STRENGTH, 5)
        player.apply_power(PowerId.WEAK, 2)
        enemy.apply_power(PowerId.VULNERABLE, 3)
        props = ValueProp.MOVE | ValueProp.UNPOWERED
        dmg = calculate_damage(10, player, enemy, props, [player, enemy])
        assert dmg == 10  # No modifiers applied

    def test_damage_floors_to_zero(self, player, enemy):
        """Negative calculated damage is clamped to 0."""
        player.apply_power(PowerId.STRENGTH, -10)
        dmg = calculate_damage(5, player, enemy, ValueProp.MOVE, [player, enemy])
        assert dmg == 0  # 5 + (-10) = -5, clamped to 0


class TestApplyDamage:
    """Damage application: block absorption, HP loss, kill detection."""

    def test_block_absorbs_partial_damage(self, enemy):
        """10 dmg vs 7 block = 3 hp lost, 0 block remaining."""
        enemy.gain_block(7)
        result = apply_damage(enemy, 10, ValueProp.MOVE)
        assert result.blocked == 7
        assert result.hp_lost == 3
        assert enemy.block == 0
        assert enemy.current_hp == 47

    def test_block_absorbs_damage_exactly(self, enemy):
        """Damage equal to block = 0 hp lost, 0 block remaining."""
        enemy.gain_block(10)
        result = apply_damage(enemy, 10, ValueProp.MOVE)
        assert result.blocked == 10
        assert result.hp_lost == 0
        assert enemy.block == 0
        assert enemy.current_hp == 50

    def test_block_fully_absorbs_excess_block(self, enemy):
        """Block larger than damage leaves leftover block."""
        enemy.gain_block(10)
        result = apply_damage(enemy, 6, ValueProp.MOVE)
        assert result.blocked == 6
        assert result.hp_lost == 0
        assert enemy.block == 4
        assert enemy.current_hp == 50

    def test_unblockable_bypasses_block(self, enemy):
        """UNBLOCKABLE damage ignores block entirely."""
        enemy.gain_block(20)
        props = ValueProp.MOVE | ValueProp.UNBLOCKABLE
        result = apply_damage(enemy, 10, props)
        assert result.blocked == 0
        assert result.hp_lost == 10
        assert enemy.block == 20  # block unchanged
        assert enemy.current_hp == 40

    def test_lethal_damage_kills(self, enemy):
        """Damage exceeding HP kills the creature."""
        result = apply_damage(enemy, 100, ValueProp.MOVE)
        assert result.was_killed
        assert enemy.current_hp == 0
        assert enemy.is_dead

    def test_zero_damage_no_effect(self, enemy):
        """Zero damage does nothing."""
        result = apply_damage(enemy, 0, ValueProp.MOVE)
        assert result.blocked == 0
        assert result.hp_lost == 0
        assert enemy.current_hp == 50


class TestBlockPipeline:
    """Block calculation: additive (Dex) then multiplicative (Frail)."""

    def test_base_block_no_modifiers(self, player):
        block = calculate_block(5, player, ValueProp.MOVE, [player])
        assert block == 5

    def test_dexterity_adds_to_block(self, player):
        player.apply_power(PowerId.DEXTERITY, 2)
        block = calculate_block(5, player, ValueProp.MOVE, [player])
        assert block == 7  # 5 + 2

    def test_frail_reduces_block(self, player):
        """Frail multiplies block by 0.75."""
        player.apply_power(PowerId.FRAIL, 2)
        block = calculate_block(5, player, ValueProp.MOVE, [player])
        assert block == 3  # floor(5 * 0.75) = 3

    def test_dexterity_then_frail(self, player):
        """5 base + 2 dex = 7, * 0.75 frail = 5.25, floor = 5."""
        player.apply_power(PowerId.DEXTERITY, 2)
        player.apply_power(PowerId.FRAIL, 2)
        block = calculate_block(5, player, ValueProp.MOVE, [player])
        assert block == 5  # floor((5 + 2) * 0.75) = floor(5.25) = 5

    def test_block_floors_to_zero(self, player):
        """Negative block is clamped to 0."""
        player.apply_power(PowerId.DEXTERITY, -10)
        block = calculate_block(5, player, ValueProp.MOVE, [player])
        assert block == 0  # 5 + (-10) = -5, clamped to 0
