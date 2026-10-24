"""Tests for the damage and block calculation pipelines."""

import math
import pytest

from sts2_env.core.constants import MULTIPLAYER_ACT_SCALING
from sts2_env.core.creature import Creature
from sts2_env.core.damage import calculate_damage, calculate_block, apply_damage
from sts2_env.core.enums import CombatSide, PowerId, ValueProp
from sts2_env.powers.base import PowerInstance
from sts2_env.run.run_state import PlayerState


STARTING_ACT_INDEX = 0
MULTIPLAYER_BLOCK_BASE = 10
MULTIPLAYER_ALLY_CHARACTER_ID = "Ironclad"
MULTIPLAYER_ALLY_PLAYER_ID = 2
MULTIPLAYER_ALLY_HP = 70


def _add_multiplayer_ally(combat):
    combat.add_ally_player(
        PlayerState(
            player_id=MULTIPLAYER_ALLY_PLAYER_ID,
            character_id=MULTIPLAYER_ALLY_CHARACTER_ID,
            max_hp=MULTIPLAYER_ALLY_HP,
            current_hp=MULTIPLAYER_ALLY_HP,
        )
    )


def _starting_act_enemy_block(combat, base_block: int) -> int:
    return math.floor(
        base_block
        * len(combat.combat_player_states)
        * MULTIPLAYER_ACT_SCALING[STARTING_ACT_INDEX]
    )


class _RecordingDamagePower(PowerInstance):
    def __init__(self, events: list[str]):
        super().__init__(PowerId.BURROWED, 1)
        self.events = events

    def on_block_broken(self, owner, combat) -> None:
        self.events.append("block")

    def after_current_hp_changed(self, owner, creature, delta, combat) -> None:
        if creature is owner:
            self.events.append("hp")

    def after_damage_given(self, owner, dealer, target, damage, props, combat) -> None:
        if target is owner:
            self.events.append("given")

    def after_damage_received(self, owner, target, dealer, damage, props, combat) -> None:
        if target is owner:
            self.events.append("received")


class _RecordingGivenDamagePower(PowerInstance):
    def __init__(self, damage_seen: list[int]):
        super().__init__(PowerId.VIGOR, 1)
        self.damage_seen = damage_seen

    def after_damage_given(self, owner, dealer, target, damage, props, combat) -> None:
        if dealer is owner:
            self.damage_seen.append(damage)


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

    def test_combat_damage_hooks_follow_original_order(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        events = []
        enemy.block = 3
        enemy.powers[PowerId.BURROWED] = _RecordingDamagePower(events)

        simple_combat.deal_damage(
            dealer=player,
            target=enemy,
            amount=5,
            props=ValueProp.MOVE,
        )

        assert events == ["block", "hp", "given", "received"]

    def test_lethal_damage_hooks_use_actual_hp_lost_not_overkill(self, simple_combat):
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        damage_seen = []
        player.powers[PowerId.VIGOR] = _RecordingGivenDamagePower(damage_seen)
        enemy.current_hp = 2

        simple_combat.deal_damage(
            dealer=player,
            target=enemy,
            amount=5,
            props=ValueProp.MOVE,
        )

        assert damage_seen == [2]


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

    def test_multiplayer_enemy_powered_block_scales_like_original(self, simple_combat):
        _add_multiplayer_ally(simple_combat)
        enemy = simple_combat.enemies[0]

        block = calculate_block(MULTIPLAYER_BLOCK_BASE, enemy, ValueProp.MOVE, simple_combat)

        assert block == _starting_act_enemy_block(simple_combat, MULTIPLAYER_BLOCK_BASE)

    def test_multiplayer_block_scaling_skips_players_and_unpowered_block(self, simple_combat):
        _add_multiplayer_ally(simple_combat)
        enemy = simple_combat.enemies[0]

        player_block = calculate_block(MULTIPLAYER_BLOCK_BASE, simple_combat.player, ValueProp.MOVE, simple_combat)
        unpowered_enemy_block = calculate_block(
            MULTIPLAYER_BLOCK_BASE,
            enemy,
            ValueProp.MOVE | ValueProp.UNPOWERED,
            simple_combat,
        )

        assert player_block == MULTIPLAYER_BLOCK_BASE
        assert unpowered_enemy_block == MULTIPLAYER_BLOCK_BASE

    def test_before_block_gained_fires_with_raw_amount_before_modify(self, simple_combat):
        """C# CreatureCmd.cs:634-642: Hook.BeforeBlockGained (Hook.cs:131-137)
        fires with the RAW amount right before Hook.ModifyBlock runs.  Skeleton
        dispatch — no v0.107.1 listener implements it yet."""
        from sts2_env.core.hooks import modify_block

        events: list[tuple[Creature, int, ValueProp, object]] = []

        class _BeforeBlockProbe(PowerInstance):
            def __init__(self):
                super().__init__(PowerId.BURROWED, 1)

            def before_block_gained(self, owner, creature, amount, props, card_source):
                events.append((creature, amount, props, card_source))

        simple_combat.player.powers[PowerId.BURROWED] = _BeforeBlockProbe()

        modify_block(8, simple_combat.player, ValueProp.MOVE, simple_combat, card_source=None)

        assert events == [(simple_combat.player, 8, ValueProp.MOVE, None)]

    def test_block_cap_constant_matches_csharp_gain_block_internal(self):
        """C# Creature.cs:465 GainBlockInternal caps block at 999999999, not 999."""
        from sts2_env.core.constants import BLOCK_CAP

        assert BLOCK_CAP == 999999999


class TestHookListenerOrder:
    """A7 guard: C# dispatches one global listener table built per creature
    (powers -> relics -> potions -> orbs -> cards, CombatState.cs:410-501).
    The sim only carries powers and relics; pin their relative order so a
    future listener class cannot silently jump the queue."""

    def test_hook_listeners_dispatch_powers_before_relics_per_creature(self, simple_combat):
        from sts2_env.core.hooks import _iter_hook_listeners  # noqa: SLF001
        from sts2_env.relics.base import RelicId, RelicInstance

        class _OrderProbeRelic(RelicInstance):
            def __init__(self):
                super().__init__(RelicId.BURNING_BLOOD)

        simple_combat.player.apply_power(PowerId.DEXTERITY, 1)
        simple_combat.current_player_state.relics.append(_OrderProbeRelic())

        kinds = [
            "relic" if is_relic else "power"
            for owner, listener, is_relic in _iter_hook_listeners(simple_combat)
            if owner is simple_combat.player
        ]

        assert "power" in kinds and "relic" in kinds
        assert kinds.index("relic") > max(
            index for index, kind in enumerate(kinds) if kind == "power"
        )

