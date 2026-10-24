"""Tests for the STS2 Gymnasium environment.

Covers observation shape, action masking, step/reset contracts,
invalid action handling, and multi-episode stability.
"""

import pytest
import numpy as np

import gymnasium

from sts2_env.gym_env.combat_env import STS2CombatEnv
from sts2_env.gym_env.observation import (
    ENEMY_FEATURES,
    INTENT_TYPES,
    MAX_INTENTS_PER_MOVE,
    NUM_INTENT_TYPES,
    OBS_SIZE,
    _ENEMY_INTENT_SLOT_OFFSETS,
    encode_observation,
)
from sts2_env.core.constants import ACTION_SPACE_SIZE, MAX_ENEMIES
from sts2_env.core.combat import CombatState
from sts2_env.core.creature import Creature
from sts2_env.core.enums import CombatSide, IntentType
from sts2_env.core.rng import Rng
from sts2_env.cards.ironclad_basic import create_ironclad_starter_deck
from sts2_env.monsters.act4 import (
    WATERFALL_GIANT_MONSTER_ID,
    WATERFALL_GIANT_STOMP_MOVE,
    create_waterfall_giant,
)
from sts2_env.monsters.intents import Intent, attack_intent, buff_intent, debuff_intent, defend_intent
from sts2_env.monsters.state_machine import MonsterAI, MoveState


@pytest.fixture
def env():
    """Create a fresh environment for each test."""
    return STS2CombatEnv()


@pytest.fixture
def seeded_env(env):
    """Return an environment that has been reset with a fixed seed."""
    env.reset(seed=42)
    return env


class TestReset:
    """Test reset() returns valid observation shape and info dict."""

    def test_reset_returns_tuple_of_two(self, env):
        result = env.reset(seed=42)
        assert isinstance(result, tuple)
        assert len(result) == 2

    def test_reset_obs_shape(self, env):
        obs, info = env.reset(seed=42)
        assert isinstance(obs, np.ndarray)
        assert obs.shape == (OBS_SIZE,)
        assert obs.shape == (217,)

    def test_reset_obs_dtype(self, env):
        obs, info = env.reset(seed=42)
        assert obs.dtype == np.float32

    def test_reset_info_contains_action_mask(self, env):
        obs, info = env.reset(seed=42)
        assert isinstance(info, dict)
        assert "action_mask" in info

    def test_reset_info_action_mask_shape(self, env):
        obs, info = env.reset(seed=42)
        mask = info["action_mask"]
        assert mask.shape == (ACTION_SPACE_SIZE,)

    def test_reset_deterministic_with_same_seed(self, env):
        obs1, _ = env.reset(seed=123)
        obs2, _ = env.reset(seed=123)
        np.testing.assert_array_equal(obs1, obs2)


class TestStep:
    """Test step() with EndTurn action returns valid obs/reward/done."""

    def test_step_end_turn_returns_five_values(self, seeded_env):
        result = seeded_env.step(0)
        assert isinstance(result, tuple)
        assert len(result) == 5

    def test_step_end_turn_obs_shape(self, seeded_env):
        obs, reward, terminated, truncated, info = seeded_env.step(0)
        assert isinstance(obs, np.ndarray)
        assert obs.shape == (217,)
        assert obs.dtype == np.float32

    def test_step_end_turn_reward_is_float(self, seeded_env):
        obs, reward, terminated, truncated, info = seeded_env.step(0)
        assert isinstance(reward, float)

    def test_step_end_turn_terminated_is_bool(self, seeded_env):
        obs, reward, terminated, truncated, info = seeded_env.step(0)
        assert isinstance(terminated, bool)

    def test_step_end_turn_truncated_is_bool(self, seeded_env):
        obs, reward, terminated, truncated, info = seeded_env.step(0)
        assert isinstance(truncated, bool)

    def test_step_end_turn_info_has_action_mask(self, seeded_env):
        obs, reward, terminated, truncated, info = seeded_env.step(0)
        assert "action_mask" in info

    def test_step_reward_zero_when_not_over(self, seeded_env):
        """Mid-combat steps should have reward 0."""
        obs, reward, terminated, truncated, info = seeded_env.step(0)
        if not terminated:
            assert reward == 0.0

    def test_step_play_card_action(self, seeded_env):
        """Playing a valid card should return valid results."""
        mask = seeded_env.action_masks()
        valid_actions = np.where(mask == 1)[0]
        # There should be at least EndTurn + one card
        assert len(valid_actions) > 1
        action = int(valid_actions[1])
        obs, reward, terminated, truncated, info = seeded_env.step(action)
        assert isinstance(obs, np.ndarray)
        assert obs.shape == (217,)


class TestActionMasks:
    """Test action_masks() returns a binary vector sized to ACTION_SPACE_SIZE."""

    def test_action_mask_shape(self, seeded_env):
        mask = seeded_env.action_masks()
        assert mask.shape == (ACTION_SPACE_SIZE,)

    def test_action_mask_dtype(self, seeded_env):
        mask = seeded_env.action_masks()
        assert mask.dtype == np.int8

    def test_end_turn_always_valid(self, seeded_env):
        """action_masks()[0] is always True (EndTurn always valid)."""
        mask = seeded_env.action_masks()
        assert mask[0] == 1

    def test_end_turn_valid_after_playing_cards(self, seeded_env):
        """EndTurn should remain valid even after playing cards."""
        mask = seeded_env.action_masks()
        valid = np.where(mask == 1)[0]
        # Play a card if possible
        for action in valid:
            if action != 0:
                seeded_env.step(int(action))
                break
        mask_after = seeded_env.action_masks()
        assert mask_after[0] == 1

    def test_end_turn_valid_when_no_combat(self):
        """Before reset, EndTurn should be valid (fallback mask)."""
        env = STS2CombatEnv()
        mask = env.action_masks()
        assert mask[0] == 1

    def test_at_least_one_action_valid(self, seeded_env):
        """There should always be at least one valid action (EndTurn)."""
        mask = seeded_env.action_masks()
        assert np.sum(mask) >= 1

    def test_mask_values_are_binary(self, seeded_env):
        """All mask values should be 0 or 1."""
        mask = seeded_env.action_masks()
        assert np.all((mask == 0) | (mask == 1))

    def test_mask_matches_info_mask(self, env):
        """action_masks() should match the mask returned in info."""
        obs, info = env.reset(seed=42)
        mask_method = env.action_masks()
        mask_info = info["action_mask"]
        np.testing.assert_array_equal(mask_method, mask_info)


class TestInvalidActionMasking:
    """Test that invalid actions are masked (card with cost > energy)."""

    def test_expensive_card_masked_with_zero_energy(self, seeded_env):
        """Cards that cost more energy than available should be masked."""
        # Drain all energy by ending turns or setting directly
        seeded_env.combat.energy = 0
        mask = seeded_env.action_masks()
        # With 0 energy, no card actions should be valid (only EndTurn)
        # All cards in the starter deck cost >= 1
        assert mask[0] == 1  # EndTurn valid
        # No card actions should be valid
        assert np.sum(mask[1:]) == 0

    def test_card_masked_when_cost_exceeds_energy(self, seeded_env):
        """Verify each hand card's mask status respects cost vs energy."""
        combat = seeded_env.combat
        mask = seeded_env.action_masks()
        for i, card in enumerate(combat.hand):
            if card.cost > combat.energy:
                # This card should NOT be valid in any action slot
                # Self-target slot
                assert mask[1 + i] == 0, (
                    f"Card {card.card_id} at index {i} costs {card.cost} "
                    f"but energy is {combat.energy}, should be masked"
                )

    def test_energy_decreases_after_playing_card(self, seeded_env):
        """Playing a card should reduce energy, possibly masking other cards."""
        combat = seeded_env.combat
        initial_energy = combat.energy
        mask = seeded_env.action_masks()
        valid = np.where(mask == 1)[0]
        # Play a non-EndTurn action
        card_action = None
        for a in valid:
            if a != 0:
                card_action = int(a)
                break
        if card_action is not None:
            seeded_env.step(card_action)
            assert combat.energy <= initial_energy


class TestFullEpisode:
    """Test full episode: random valid actions until done, verify terminated=True."""

    def test_episode_completes(self, env):
        """A full episode with random valid actions should terminate."""
        obs, info = env.reset(seed=42)
        done = False
        steps = 0
        max_steps = 500
        while not done and steps < max_steps:
            mask = info["action_mask"]
            valid = np.where(mask == 1)[0]
            action = int(np.random.choice(valid))
            obs, reward, terminated, truncated, info = env.step(action)
            done = terminated or truncated
            steps += 1
        assert done, f"Episode did not complete within {max_steps} steps"

    def test_terminal_reward_nonzero(self, env):
        """At termination, reward should be +1 (win) or -1 (loss)."""
        obs, info = env.reset(seed=42)
        done = False
        steps = 0
        final_reward = 0.0
        while not done and steps < 500:
            mask = info["action_mask"]
            valid = np.where(mask == 1)[0]
            action = int(np.random.choice(valid))
            obs, reward, terminated, truncated, info = env.step(action)
            done = terminated or truncated
            final_reward = reward
            steps += 1
        if done:
            assert final_reward == 1.0 or final_reward == -1.0

    def test_multiple_seeds_terminate(self, env):
        """Episodes with different seeds should all terminate."""
        for seed in range(10):
            obs, info = env.reset(seed=seed)
            done = False
            steps = 0
            while not done and steps < 500:
                mask = info["action_mask"]
                valid = np.where(mask == 1)[0]
                action = int(np.random.choice(valid))
                obs, reward, terminated, truncated, info = env.step(action)
                done = terminated or truncated
                steps += 1
            assert done, f"Episode with seed={seed} did not terminate"

    def test_terminated_flag_set(self, env):
        """At least some episodes should end with terminated=True (not truncated)."""
        natural_terminations = 0
        for seed in range(20):
            obs, info = env.reset(seed=seed)
            done = False
            steps = 0
            while not done and steps < 500:
                mask = info["action_mask"]
                valid = np.where(mask == 1)[0]
                action = int(np.random.choice(valid))
                obs, reward, terminated, truncated, info = env.step(action)
                done = terminated or truncated
                steps += 1
                if terminated:
                    natural_terminations += 1
        assert natural_terminations > 0, "No episodes ended with terminated=True"


class TestStressEpisodes:
    """Test 100 random episodes complete without errors."""

    def test_100_random_episodes(self, env):
        """Run 100 episodes with random valid actions -- none should crash."""
        completed = 0
        rng = np.random.RandomState(0)
        for seed in range(100):
            obs, info = env.reset(seed=seed)
            done = False
            steps = 0
            while not done and steps < 500:
                mask = info["action_mask"]
                valid = np.where(mask == 1)[0]
                assert len(valid) > 0, (
                    f"No valid actions at seed={seed}, step={steps}"
                )
                action = int(rng.choice(valid))
                obs, reward, terminated, truncated, info = env.step(action)
                # Verify step outputs are well-formed at every step
                assert obs.shape == (217,)
                assert obs.dtype == np.float32
                assert isinstance(reward, float)
                assert isinstance(terminated, bool)
                assert isinstance(truncated, bool)
                assert "action_mask" in info
                done = terminated or truncated
                steps += 1
            completed += 1
        assert completed == 100


class TestObservationRanges:
    """Test observation values are in reasonable ranges."""

    def test_obs_values_finite(self, seeded_env):
        """All observation values should be finite (no NaN or inf)."""
        obs = seeded_env.combat
        obs_vec, _ = seeded_env.reset(seed=42)
        assert np.all(np.isfinite(obs_vec)), "Observation contains NaN or inf"

    def test_obs_initial_in_range(self, env):
        """Initial observation values should be within observation space bounds."""
        obs, _ = env.reset(seed=42)
        low = env.observation_space.low
        high = env.observation_space.high
        violations = np.where((obs < low) | (obs > high))[0]
        assert len(violations) == 0, (
            f"Obs values out of range at indices {violations}: "
            f"values={obs[violations]}, low={low[violations]}, high={high[violations]}"
        )

    def test_obs_after_step_in_range(self, env):
        """Observation after a step should remain within observation space bounds."""
        obs, info = env.reset(seed=42)
        obs, reward, terminated, truncated, info = env.step(0)
        if not terminated:
            low = env.observation_space.low
            high = env.observation_space.high
            violations = np.where((obs < low) | (obs > high))[0]
            assert len(violations) == 0, (
                f"Post-step obs values out of range at indices {violations}: "
                f"values={obs[violations]}"
            )

    def test_obs_hp_ratio_between_0_and_1(self, env):
        """Player HP ratio (obs[0]) should be in [0, 1]."""
        obs, _ = env.reset(seed=42)
        assert 0.0 <= obs[0] <= 1.0, f"HP ratio = {obs[0]}"

    def test_obs_energy_nonnegative(self, env):
        """Energy (obs[2]) should be non-negative."""
        obs, _ = env.reset(seed=42)
        assert obs[2] >= 0.0, f"Energy obs = {obs[2]}"

    def test_obs_throughout_episode(self, env):
        """Observations should stay in valid range throughout an entire episode."""
        obs, info = env.reset(seed=42)
        low = env.observation_space.low
        high = env.observation_space.high
        steps = 0
        done = False
        while not done and steps < 300:
            assert np.all(np.isfinite(obs)), f"NaN/inf at step {steps}"
            violations = np.where((obs < low) | (obs > high))[0]
            assert len(violations) == 0, (
                f"Obs out of range at step {steps}, indices {violations}"
            )
            mask = info["action_mask"]
            valid = np.where(mask == 1)[0]
            action = int(np.random.choice(valid))
            obs, reward, terminated, truncated, info = env.step(action)
            done = terminated or truncated
            steps += 1

    def test_obs_across_multiple_seeds(self, env):
        """Observations across 20 seeds should all be finite and in range."""
        low = env.observation_space.low
        high = env.observation_space.high
        for seed in range(20):
            obs, _ = env.reset(seed=seed)
            assert np.all(np.isfinite(obs)), f"NaN/inf at seed {seed}"
            violations = np.where((obs < low) | (obs > high))[0]
            assert len(violations) == 0, (
                f"Obs out of range at seed {seed}, indices {violations}"
            )


# ---------------------------------------------------------------------------
# Multi-intent move encoding
# ---------------------------------------------------------------------------

# Offset of enemy slot 0 inside the flat observation.
_ENEMY_BLOCK_START = OBS_SIZE - MAX_ENEMIES * ENEMY_FEATURES


def _intent_slot_vec(obs: np.ndarray, slot: int) -> np.ndarray:
    """Slice of the first enemy's intent *slot* (onehot + damage + hits)."""
    base = _ENEMY_BLOCK_START + _ENEMY_INTENT_SLOT_OFFSETS[slot]
    return obs[base:base + NUM_INTENT_TYPES + 2]


class TestMultiIntentEncoding:
    """Enemy blocks must encode every intent of a multi-intent move.

    C# ``MoveState`` takes ``params AbstractIntent[] intents``; the decompiled
    game attaches up to 3 intents to one move (e.g. WaterfallGiant.cs:195
    STOMP: SingleAttackIntent + DebuffIntent + BuffIntent).
    """

    def test_waterfall_giant_stomp_encodes_all_three_slots(self):
        """WaterfallGiant STOMP (3 intents) fills all 3 intent slots."""
        _, factory_ai = create_waterfall_giant(Rng(42))
        deck = create_ironclad_starter_deck()
        combat = CombatState(player_hp=80, player_max_hp=80, deck=deck, rng_seed=42)
        creature = Creature(
            max_hp=300, current_hp=300, side=CombatSide.ENEMY,
            monster_id=WATERFALL_GIANT_MONSTER_ID,
        )
        ai = MonsterAI(factory_ai.states, WATERFALL_GIANT_STOMP_MOVE)
        combat.add_enemy(creature, ai)
        combat.start_combat()

        assert ai.current_move.state_id == WATERFALL_GIANT_STOMP_MOVE
        assert len(ai.current_move.intents) == MAX_INTENTS_PER_MOVE

        obs = encode_observation(combat)
        assert obs.shape == (OBS_SIZE,)

        # C# WaterfallGiant.cs:195: SingleAttackIntent(15) + Debuff + Buff
        expected = [
            (IntentType.ATTACK, 15),
            (IntentType.DEBUFF, 0),
            (IntentType.BUFF, 0),
        ]
        for slot, (intent_type, damage) in enumerate(expected):
            vec = _intent_slot_vec(obs, slot)
            expected_onehot = [1.0 if t == intent_type else 0.0 for t in INTENT_TYPES]
            assert list(vec[:NUM_INTENT_TYPES]) == expected_onehot, (
                f"slot {slot}: expected {intent_type} one-hot, got {vec[:NUM_INTENT_TYPES]}"
            )
            assert vec[NUM_INTENT_TYPES] == pytest.approx(damage / 30.0)
            assert vec[NUM_INTENT_TYPES + 1] == pytest.approx(1 / 5.0)

        # Enemy block head stays alive/hp/block, powers sit between slots.
        assert obs[_ENEMY_BLOCK_START] == 1.0

    def test_single_intent_move_leaves_overflow_slots_zero(self):
        """A 1-intent move fills slot 0 only; slots 1-2 stay zero."""
        move = MoveState(
            "SOLO_MOVE", lambda combat: None, [attack_intent(6)],
        )
        combat, _, _ = self._combat_with_move(move)
        obs = encode_observation(combat)

        slot0 = _intent_slot_vec(obs, 0)
        assert slot0[INTENT_TYPES.index(IntentType.ATTACK)] == 1.0
        assert slot0[NUM_INTENT_TYPES] == pytest.approx(6 / 30.0)
        for slot in range(1, MAX_INTENTS_PER_MOVE):
            assert not _intent_slot_vec(obs, slot).any(), (
                f"overflow slot {slot} should be all zeros"
            )

    def test_intents_beyond_max_slots_are_truncated(self):
        """Intents past MAX_INTENTS_PER_MOVE are dropped without error.

        Note: the decompiled C# never exceeds 3 intents per move, so in
        practice this only guards against future Python monster definitions
        drifting past the cap.
        """
        move = MoveState(
            "OVERFLOW_MOVE",
            lambda combat: None,
            [attack_intent(9), defend_intent(), buff_intent(), debuff_intent()],
        )
        combat, _, ai = self._combat_with_move(move)
        obs = encode_observation(combat)

        # The move itself keeps all 4 intents; the encoding keeps the first 3.
        assert len(ai.current_move.intents) == 4
        assert _intent_slot_vec(obs, 0)[INTENT_TYPES.index(IntentType.ATTACK)] == 1.0
        assert _intent_slot_vec(obs, 1)[INTENT_TYPES.index(IntentType.DEFEND)] == 1.0
        assert _intent_slot_vec(obs, 2)[INTENT_TYPES.index(IntentType.BUFF)] == 1.0
        for slot in range(MAX_INTENTS_PER_MOVE):
            assert _intent_slot_vec(obs, slot)[INTENT_TYPES.index(IntentType.DEBUFF)] == 0.0

    @staticmethod
    def _combat_with_move(move: MoveState):
        """Single-enemy combat whose AI starts at *move*."""
        deck = create_ironclad_starter_deck()
        combat = CombatState(player_hp=80, player_max_hp=80, deck=deck, rng_seed=42)
        creature = Creature(
            max_hp=50, current_hp=50, side=CombatSide.ENEMY, monster_id="TEST_ENEMY",
        )
        ai = MonsterAI({move.state_id: move}, move.state_id)
        combat.add_enemy(creature, ai)
        combat.start_combat()
        return combat, creature, ai


class TestEpisodeEndContract:
    """Gymnasium termination convention: an episode is frozen once step()
    reports terminated=True or truncated=True -- the internal combat stops
    advancing and any further step() raises
    gymnasium.error.ClosedEnvironmentError until reset() is called.
    """

    def test_truncation_reports_truncated_and_freezes_env(self):
        """Gymnasium TimeLimit convention: hitting max_turns ends the episode
        through truncated=True (terminated stays False -- the two signals stay
        separate) and the internal combat no longer advances afterwards."""
        env = STS2CombatEnv(max_turns=1)
        env.reset(seed=0)
        obs, reward, terminated, truncated, info = env.step(0)
        assert terminated is False
        assert truncated is True

        frozen_turn_count = env.combat.turn_count
        with pytest.raises(gymnasium.error.ClosedEnvironmentError):
            env.step(0)
        assert env.combat.turn_count == frozen_turn_count

    def test_action_masks_after_truncation_returns_fallback(self):
        """action_masks() after the episode ended must not crash (training
        scaffolds may query it after the final step)."""
        env = STS2CombatEnv(max_turns=1)
        env.reset(seed=0)
        env.step(0)
        mask = env.action_masks()
        assert mask.shape == (ACTION_SPACE_SIZE,)
        assert mask.sum() >= 1

    def test_step_after_termination_raises_closed_env_error(self):
        """Once combat.is_over fires, the episode is over: a further step()
        raises ClosedEnvironmentError (Gymnasium step-after-done guard)."""
        env = STS2CombatEnv()
        obs, info = env.reset(seed=3)
        rng = np.random.RandomState(0)
        terminated = truncated = False
        action = 0
        for _ in range(1000):
            action = int(rng.choice(np.where(info["action_mask"] == 1)[0]))
            obs, reward, terminated, truncated, info = env.step(action)
            if terminated or truncated:
                break
        assert terminated or truncated
        with pytest.raises(gymnasium.error.ClosedEnvironmentError):
            env.step(action)

    def test_reset_clears_ended_state(self):
        """reset() starts a fresh episode and clears the end-of-episode
        guard, so stepping after reset is legal again (with max_turns=1 the
        new episode truncates on its own schedule instead of raising)."""
        env = STS2CombatEnv(max_turns=1)
        env.reset(seed=0)
        env.step(0)  # truncated
        with pytest.raises(gymnasium.error.ClosedEnvironmentError):
            env.step(0)
        env.reset(seed=1)
        obs, reward, terminated, truncated, info = env.step(0)
        assert isinstance(truncated, bool)  # stepped again without raising
        assert env.combat.turn_count == 2
