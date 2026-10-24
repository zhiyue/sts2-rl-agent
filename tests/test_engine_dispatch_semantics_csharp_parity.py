"""Engine-mechanism semantics verified against the decompiled C# (v0.107.1).

Covers five mechanism areas against decompiled/ sources:

1. Damage pipeline two-pass structure
   (Hook.ModifyDamageInternal, Hook.cs:2511 — one combined additive pass over
   every hook listener, then one combined multiplicative pass, then caps).
2. Block-clear timing
   (Creature.AfterTurnStart turn-1 exemption, Creature.cs:694; the
   Hook.AfterBlockCleared dispatch fires unconditionally afterwards,
   CombatManager.cs StartTurn).
3. Debuff duration decay
   (Vulnerable/Weak/Frail tick at end of the ENEMY side turn,
   VulnerablePower.cs AfterSideTurnEnd; PowerCmd.Apply sets
   SkipNextDurationTick only when a NEW debuff instance is created on a
   player-side creature, PowerCmd.cs:78-88 vs PowerCmd.cs:143-147).
4. Combined listener dispatch order
   (CombatState.IterateHookListeners, CombatState.cs:410 — per creature,
   powers then that creature's relics; player creature iterated first).
5. Player turn phase sequence
   (CombatManager.StartTurn / SetupPlayerTurn / EndPlayerTurnPhaseOneInternal
   / EndPlayerTurnPhaseTwoInternal ordering).
"""

import pytest

import sts2_env.powers  # noqa: F401  (ensure power registration)

from sts2_env.cards.ironclad_basic import make_strike_ironclad
from sts2_env.core.combat import CombatState
from sts2_env.core.damage import calculate_damage
from sts2_env.core.enums import CombatSide, PowerId, ValueProp
from sts2_env.core.hooks import (
    _iter_hook_listeners,
    fire_after_turn_end,
    fire_before_turn_end,
)
from sts2_env.powers.base import PowerInstance
from sts2_env.relics.registry import create_relic_by_name


# ─── helpers ────────────────────────────────────────────────────────────────


class _OrderProbePower(PowerInstance):
    """Records dispatch order; never modifies anything."""

    def __init__(self, events: list[str], tag: str):
        super().__init__(PowerId.BURROWED, 1)
        self._events = events
        self._tag = tag

    def after_turn_end(self, owner, side, combat) -> None:
        self._events.append(f"power:{self._tag}")

    def before_turn_end_very_early(self, owner, side, combat) -> None:
        self._events.append(f"power:{self._tag}:before_very_early")

    def before_turn_end_early(self, owner, side, combat) -> None:
        self._events.append(f"power:{self._tag}:before_early")

    def before_turn_end(self, owner, side, combat) -> None:
        self._events.append(f"power:{self._tag}:before")


class _OrderProbeRelic:
    """Standalone relic-shaped listener (duck-typed RelicInstance surface)."""

    def __init__(self, events: list[str], tag: str):
        self.relic_id = "PROBE_RELIC"
        self.enabled = True
        self._events = events
        self._tag = tag

    def should_clear_block(self, owner, creature) -> bool | None:
        return None

    def after_turn_end(self, owner, side, combat) -> None:
        self._events.append(f"relic:{self._tag}")

    def before_turn_end_very_early(self, owner, side, combat) -> None:
        self._events.append(f"relic:{self._tag}:before_very_early")

    def before_turn_end_early(self, owner, side, combat) -> None:
        self._events.append(f"relic:{self._tag}:before_early")

    def before_turn_end(self, owner, side, combat) -> None:
        self._events.append(f"relic:{self._tag}:before")

    def after_turn_end_late(self, owner, side, combat) -> None:
        pass


# ─── 1. Damage pipeline two-pass structure ─────────────────────────────────


class TestDamagePipelineTwoPass:
    """Hook.ModifyDamageInternal: additive pass completes before the
    multiplicative pass begins, for the combined listener list."""

    def test_additive_completes_before_multiplicative(self, player, enemy):
        """Strength first: (10 + 5) * 1.5 = 22, not 10 * 1.5 + 5 = 20.

        A multiplicative listener reading the intermediate total must observe
        the fully summed additive value — the C# pipeline guarantees this by
        finishing the Additive loop (Hook.cs:2522-2533) before the
        Multiplicative loop (Hook.cs:2534-2545).
        """
        player.apply_power(PowerId.STRENGTH, 5)
        enemy.apply_power(PowerId.VULNERABLE, 1)
        assert calculate_damage(10, player, enemy, ValueProp.MOVE, [player, enemy]) == 22

    def test_negative_strength_before_weak(self, player, enemy):
        """(10 - 3) * 0.75 = 5 (floor), not (10 * 0.75) - 3 = 4."""
        player.apply_power(PowerId.STRENGTH, -3)
        player.apply_power(PowerId.WEAK, 1)
        assert calculate_damage(10, player, enemy, ValueProp.MOVE, [player, enemy]) == 5

    def test_unpowered_attack_skips_both_passes(self, player, enemy):
        """ValueProp.Unpowered (e.g. Poison ticks) is immune to Strength,
        Weak and Vulnerable — IsPoweredAttack() requires Move without
        Unpowered (ValuePropExtensions.cs:5-16)."""
        player.apply_power(PowerId.STRENGTH, 5)
        enemy.apply_power(PowerId.VULNERABLE, 1)
        player.apply_power(PowerId.WEAK, 1)
        assert calculate_damage(10, player, enemy, ValueProp.UNPOWERED, [player, enemy]) == 10


# ─── 2. Block clear timing ─────────────────────────────────────────────────


class TestBlockClearTiming:
    """Creature.AfterTurnStart skips ClearBlock on the player's turn 1, but
    CombatManager.StartTurn still fires Hook.AfterBlockCleared afterwards."""

    def _make_anchor_combat(self):
        """Anchor + ShrinkerBeetle combat (same setup as the parity suite)."""
        from sts2_env.core.rng import Rng
        from sts2_env.monsters.act1_weak import create_shrinker_beetle

        combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=[make_strike_ironclad()],
            rng_seed=43,
            relics=["Anchor"],
            character_id="Ironclad",
        )
        enemy, ai = create_shrinker_beetle(Rng(43))
        combat.add_enemy(enemy, ai)
        combat.start_combat()
        return combat

    def test_round_1_block_persists_and_cleared_hook_still_fires(self):
        """Round 1: block granted at combat start (Anchor) survives the
        turn-1 setup, and block-on-clear powers (BlockNextTurn, whose C#
        AfterBlockCleared has no round guard, BlockNextTurnPower.cs:21-28)
        still trigger because CombatManager.StartTurn fires
        Hook.AfterBlockCleared unconditionally after AfterTurnStart."""
        combat = self._make_anchor_combat()
        assert combat.round_number == 1
        assert combat.player.block == 10  # Anchor block not cleared on turn 1

        combat.player.apply_power(PowerId.BLOCK_NEXT_TURN, 6)
        combat.end_player_turn()  # round 1 -> enemy turn -> round 2 setup
        assert combat.round_number == 2
        # Round 2 setup cleared the (Anchor) block first, then the
        # AfterBlockCleared hook granted BlockNextTurn's 6.
        assert combat.player.block == 6
        assert combat.player.get_power_amount(PowerId.BLOCK_NEXT_TURN) == 0

    def test_round_2_clears_leftover_block(self):
        """From round 2 on, player block clears at turn start
        (Creature.AfterTurnStart -> ClearBlock, Creature.cs:694-705)."""
        combat = self._make_anchor_combat()
        assert combat.player.block == 10
        combat.end_player_turn()
        # Entering round 2 cleared round-1 block; no BlockNextTurn power to
        # re-grant, so block is 0.
        assert combat.round_number == 2
        assert combat.player.block == 0


# ─── 3. Debuff duration decay ──────────────────────────────────────────────


class TestDebuffDecayTiming:
    """Vulnerable/Weak/Frail tick at end of ENEMY side (VulnerablePower.cs
    AfterSideTurnEnd: side==Enemy -> PowerCmd.TickDownDuration)."""

    def test_vulnerable_applied_on_player_turn_survives_player_turn_end(self, simple_combat):
        """1 stack applied during the player turn covers the whole player
        turn; it decrements only when the enemy side's turn ends."""
        player = simple_combat.player
        enemy = simple_combat.enemies[0]
        combat = simple_combat
        combat.apply_power_to(enemy, PowerId.VULNERABLE, 1)

        fire_after_turn_end(CombatSide.PLAYER, combat)
        assert enemy.get_power_amount(PowerId.VULNERABLE) == 1  # no tick on player side end

        fire_after_turn_end(CombatSide.ENEMY, combat)
        assert enemy.get_power_amount(PowerId.VULNERABLE) == 0  # ticked and removed

    def test_debuff_on_player_skips_first_tick_only_for_new_instance(self, simple_combat):
        """PowerCmd.Apply sets SkipNextDurationTick only for a NEW instance
        (PowerCmd.cs:78-88 routes stacking through ModifyAmount first, which
        never sets the flag).  Stacking an existing player-side debuff must
        not re-arm the skip."""
        combat = simple_combat
        player = combat.player

        # New instance on the player during the enemy turn -> first tick skipped.
        combat.apply_power_to(player, PowerId.VULNERABLE, 2)
        assert player.powers[PowerId.VULNERABLE].skip_next_tick is True
        fire_after_turn_end(CombatSide.ENEMY, combat)
        assert player.get_power_amount(PowerId.VULNERABLE) == 2  # skipped

        # Stack onto the existing instance -> C# does NOT re-arm the skip.
        combat.apply_power_to(player, PowerId.VULNERABLE, 1)
        assert player.powers[PowerId.VULNERABLE].skip_next_tick is False
        assert player.get_power_amount(PowerId.VULNERABLE) == 3

        fire_after_turn_end(CombatSide.ENEMY, combat)
        assert player.get_power_amount(PowerId.VULNERABLE) == 2

    def test_debuff_on_enemy_never_skips_first_tick(self, simple_combat):
        """The skip flag is player-side only (PowerCmd.cs:143-147 gates on
        target.Side == CombatSide.Player)."""
        combat = simple_combat
        enemy = combat.enemies[0]
        combat.apply_power_to(enemy, PowerId.VULNERABLE, 1)
        assert enemy.powers[PowerId.VULNERABLE].skip_next_tick is False


# ─── 4. Combined listener dispatch order ───────────────────────────────────


class TestListenerDispatchOrder:
    """CombatState.IterateHookListeners (CombatState.cs:410-446): for each
    creature — allies first, then enemies — all powers come before that same
    creature's relics; the player creature is iterated before enemies, so the
    player's relics dispatch before any enemy power."""

    def test_dispatch_order_player_powers_relics_then_enemy_powers(self, simple_combat):
        combat = simple_combat
        player = combat.player
        enemy = combat.enemies[0]
        events: list[str] = []

        player.powers[PowerId.BURROWED] = _OrderProbePower(events, "player")
        player.powers[PowerId.VIGOR] = _OrderProbePower(events, "player2")
        enemy.powers[PowerId.VIGOR] = _OrderProbePower(events, "enemy")
        combat.current_player_state.relics.append(_OrderProbeRelic(events, "player_relic"))

        order = [
            getattr(l, "_tag")
            for _o, l, _r in _iter_hook_listeners(combat)
            if isinstance(l, (_OrderProbePower, _OrderProbeRelic))
        ]
        # CombatState.cs:410 — player powers, then the player's relics, then
        # enemy powers (enemy creatures have no relic belt).
        assert order == ["player", "player2", "player_relic", "enemy"]

    def test_fire_after_turn_end_interleaves_powers_and_relics(self, simple_combat):
        """Hook.AfterTurnEnd runs one combined pass over all listeners
        (Hook.cs:1267-1283), so each creature's powers run before its relics
        and the player's relics run before any enemy power."""
        combat = simple_combat
        player = combat.player
        enemy = combat.enemies[0]
        events: list[str] = []

        player.powers[PowerId.VIGOR] = _OrderProbePower(events, "player")
        enemy.powers[PowerId.VIGOR] = _OrderProbePower(events, "enemy")
        combat.current_player_state.relics.append(_OrderProbeRelic(events, "player_relic"))

        fire_after_turn_end(CombatSide.PLAYER, combat)
        assert events == ["power:player", "relic:player_relic", "power:enemy"]

    def test_player_relics_before_enemy_powers_in_turn_end(self, simple_combat):
        """Solo C# order: playerPowers -> playerRelics -> enemyPowers.  The
        old flat 'all powers then all relics' split inverted the last two."""
        combat = simple_combat
        player = combat.player
        enemy = combat.enemies[0]
        events: list[str] = []

        enemy.powers[PowerId.VIGOR] = _OrderProbePower(events, "enemy")
        combat.current_player_state.relics.append(_OrderProbeRelic(events, "player_relic"))

        fire_before_turn_end(CombatSide.PLAYER, combat)
        # Three separate passes (VeryEarly -> Early -> plain, Hook.cs
        # BeforeTurnEnd); within each pass the combined listener order holds:
        # player relic before enemy power.
        assert events == [
            "relic:player_relic:before_very_early",
            "power:enemy:before_very_early",
            "relic:player_relic:before_early",
            "power:enemy:before_early",
            "relic:player_relic:before",
            "power:enemy:before",
        ]


# ─── 5. Player turn phase sequence ─────────────────────────────────────────


class TestPlayerTurnPhaseSequence:
    """CombatManager.StartTurn: BeforeSideTurnStart -> per-creature
    AfterTurnStart (block clear) -> AfterBlockCleared -> SetupPlayerTurn
    (energy reset -> AfterEnergyReset -> BeforeHandDraw -> draw ->
    AfterPlayerTurnStart) -> AfterSideTurnStart -> orbs -> BeforePlayPhaseStart."""

    def test_full_round_phase_order(self):
        from sts2_env.core.hooks import (
            fire_after_block_cleared,
            fire_after_energy_reset,
            fire_after_player_turn_start,
            fire_after_side_turn_start,
            fire_before_hand_draw,
            fire_before_play_phase_start,
            fire_before_side_turn_start,
        )

        combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=[make_strike_ironclad()],
            rng_seed=43,
            character_id="Ironclad",
        )
        events: list[str] = []

        player = combat.player
        player.powers[PowerId.VIGOR] = _PhaseProbe(events)
        from sts2_env.monsters.act1_weak import create_shrinker_beetle
        from sts2_env.core.rng import Rng

        enemy, ai = create_shrinker_beetle(Rng(43))
        combat.add_enemy(enemy, ai)

        fire_before_side_turn_start(CombatSide.PLAYER, combat)
        if combat.round_number > 1:
            player.clear_block(combat)
        fire_after_block_cleared(player, combat)
        combat.energy = combat.max_energy
        fire_after_energy_reset(combat, player)
        fire_before_hand_draw(player, combat)
        fire_after_player_turn_start(player, combat)
        fire_after_side_turn_start(CombatSide.PLAYER, combat)
        fire_before_play_phase_start(player, combat)

        assert events == [
            "before_side_turn_start",
            "after_block_cleared",
            "after_energy_reset",
            "before_hand_draw",
            "after_player_turn_start_early",
            "after_player_turn_start",
            "after_player_turn_start_late",
            "after_side_turn_start",
            "before_play_phase_start",
        ]

    def test_end_of_turn_phase_order(self):
        combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=[make_strike_ironclad()],
            rng_seed=43,
            character_id="Ironclad",
        )
        events: list[str] = []
        combat.player.powers[PowerId.VIGOR] = _PhaseProbe(events)

        fire_before_turn_end(CombatSide.PLAYER, combat)
        fire_after_turn_end(CombatSide.PLAYER, combat)

        assert events == [
            "before_turn_end_very_early",
            "before_turn_end_early",
            "before_turn_end",
            "after_turn_end",
            "after_turn_end_late",
        ]


class _PhaseProbe(PowerInstance):
    """Records turn-phase hook invocation order on a power."""

    def __init__(self, events: list[str]):
        super().__init__(PowerId.VIGOR, 1)
        self._events = events

    def before_side_turn_start(self, owner, side, combat) -> None:
        self._events.append("before_side_turn_start")

    def after_block_cleared(self, owner, creature, combat) -> None:
        self._events.append("after_block_cleared")

    def after_energy_reset(self, owner, combat) -> None:
        self._events.append("after_energy_reset")

    def before_hand_draw(self, owner, combat) -> None:
        self._events.append("before_hand_draw")

    def after_player_turn_start_early(self, owner, combat) -> None:
        self._events.append("after_player_turn_start_early")

    def after_player_turn_start(self, owner, combat) -> None:
        self._events.append("after_player_turn_start")

    def after_player_turn_start_late(self, owner, combat) -> None:
        self._events.append("after_player_turn_start_late")

    def after_side_turn_start(self, owner, side, combat) -> None:
        self._events.append("after_side_turn_start")

    def before_play_phase_start(self, owner, player, combat) -> None:
        self._events.append("before_play_phase_start")

    def before_turn_end_very_early(self, owner, side, combat) -> None:
        self._events.append("before_turn_end_very_early")

    def before_turn_end_early(self, owner, side, combat) -> None:
        self._events.append("before_turn_end_early")

    def before_turn_end(self, owner, side, combat) -> None:
        self._events.append("before_turn_end")

    def after_turn_end(self, owner, side, combat) -> None:
        self._events.append("after_turn_end")

    def after_turn_end_late(self, owner, side, combat) -> None:
        self._events.append("after_turn_end_late")
