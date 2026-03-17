"""Tests for the STS2 full-run Gymnasium environment.

Covers reset validity, random episode completion, action mask
correctness per phase, multi-episode stress testing, and
observation invariants.
"""

import pytest
import numpy as np

from sts2_env.cards.factory import create_card
from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.cards.ironclad_basic import make_strike_ironclad
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import CardId
from sts2_env.core.rng import Rng
from sts2_env.gym_env.run_env import (
    STS2RunEnv,
    RUN_OBS_SIZE,
    TOTAL_ACTIONS,
    NUM_PHASES,
    _COMBAT_START,
    _COMBAT_SIZE,
    _MAP_START,
    _MAP_SIZE,
    _CARD_RWD_START,
    _CARD_RWD_SIZE,
    _REST_START,
    _REST_SIZE,
    _SHOP_START,
    _SHOP_SIZE,
    _EVENT_START,
    _EVENT_SIZE,
    _TREASURE_START,
    _TREASURE_SIZE,
    _PLAYER_SELECT_START,
    _PLAYER_SELECT_SIZE,
    _BOSS_RELIC_START,
    _BOSS_RELIC_SIZE,
)
from sts2_env.cards.regent import make_gather_light
from sts2_env.monsters.act1_weak import create_shrinker_beetle
from sts2_env.run.run_manager import RunManager
from sts2_env.run.run_state import PlayerState


# ---------------------------------------------------------------------------
# Fixtures
# ---------------------------------------------------------------------------

@pytest.fixture
def env():
    """Fresh run environment."""
    return STS2RunEnv(character_id="Ironclad", ascension_level=0, max_steps=10000)


@pytest.fixture
def seeded_env(env):
    """Environment that has been reset with a fixed seed."""
    env.reset(seed=42)
    return env


# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

def _run_random_episode(env, seed, max_steps=10000):
    """Run a single episode with random valid actions.

    Returns (completed: bool, steps: int, cumulative_reward: float, info: dict).
    """
    obs, info = env.reset(seed=seed)
    rng = np.random.RandomState(seed)
    done = False
    steps = 0
    cumulative_reward = 0.0
    while not done and steps < max_steps:
        mask = info["action_mask"]
        valid = np.where(mask == 1)[0]
        assert len(valid) > 0, (
            f"No valid actions at seed={seed}, step={steps}, "
            f"phase={info.get('phase', '?')}"
        )
        action = int(rng.choice(valid))
        obs, reward, terminated, truncated, info = env.step(action)
        cumulative_reward += reward
        done = terminated or truncated
        steps += 1
    return done, steps, cumulative_reward, info


def _get_phase(env):
    """Return the current RunManager phase string."""
    if env._mgr is not None:
        return env._mgr.phase
    return None


# ---------------------------------------------------------------------------
# Test: reset returns valid observation
# ---------------------------------------------------------------------------

class TestResetReturnsValidObs:
    def test_reset_returns_tuple_of_two(self, env):
        result = env.reset(seed=42)
        assert isinstance(result, tuple)
        assert len(result) == 2

    def test_reset_obs_shape(self, env):
        obs, info = env.reset(seed=42)
        assert isinstance(obs, np.ndarray)
        assert obs.shape == (RUN_OBS_SIZE,), f"Expected {RUN_OBS_SIZE}, got {obs.shape}"

    def test_reset_obs_dtype(self, env):
        obs, _ = env.reset(seed=42)
        assert obs.dtype == np.float32

    def test_reset_obs_finite(self, env):
        obs, _ = env.reset(seed=42)
        assert np.all(np.isfinite(obs)), "Observation contains NaN or inf"

    def test_reset_obs_in_bounds(self, env):
        obs, _ = env.reset(seed=42)
        low = env.observation_space.low
        high = env.observation_space.high
        violations = np.where((obs < low) | (obs > high))[0]
        assert len(violations) == 0, (
            f"Obs out of range at indices {violations}: "
            f"values={obs[violations]}"
        )

    def test_reset_info_contains_action_mask(self, env):
        _, info = env.reset(seed=42)
        assert "action_mask" in info

    def test_reset_info_mask_shape(self, env):
        _, info = env.reset(seed=42)
        mask = info["action_mask"]
        assert mask.shape == (TOTAL_ACTIONS,)

    def test_reset_info_mask_dtype(self, env):
        _, info = env.reset(seed=42)
        assert info["action_mask"].dtype == np.int8

    def test_reset_starts_at_map_choice(self, env):
        env.reset(seed=42)
        assert _get_phase(env) == RunManager.PHASE_MAP_CHOICE

    def test_reset_deterministic_same_seed(self, env):
        obs1, _ = env.reset(seed=123)
        obs2, _ = env.reset(seed=123)
        np.testing.assert_array_equal(obs1, obs2)

    def test_reset_different_seed_differs(self, env):
        """After one step, observations from different seeds should diverge.

        At reset the obs may be identical because the initial game state
        (HP, gold, deck) is the same for the same character.  After one
        map step the combat setup differs, producing different obs.
        """
        obs1, info1 = env.reset(seed=1)
        m1 = info1["action_mask"]
        obs1b, _, _, _, _ = env.step(int(np.where(m1 == 1)[0][0]))

        obs2, info2 = env.reset(seed=999)
        m2 = info2["action_mask"]
        obs2b, _, _, _, _ = env.step(int(np.where(m2 == 1)[0][0]))

        assert not np.array_equal(obs1b, obs2b)

    def test_reset_info_has_debug_keys(self, env):
        _, info = env.reset(seed=42)
        for key in ("phase", "act", "floor", "hp", "max_hp", "gold",
                     "deck_size", "relics", "step"):
            assert key in info, f"Missing info key: {key}"


# ---------------------------------------------------------------------------
# Test: random agent can complete episodes
# ---------------------------------------------------------------------------

class TestRandomAgentCompletesEpisodes:
    def test_single_episode_completes(self, env):
        done, steps, reward, info = _run_random_episode(env, seed=42)
        assert done, "Episode did not complete within max steps"

    def test_episode_has_nonzero_terminal_reward(self, env):
        done, steps, reward, info = _run_random_episode(env, seed=42)
        assert done
        # Should be +1 (win) or -1 (death)
        assert reward != 0.0

    def test_multiple_seeds_complete(self, env):
        for seed in range(10):
            done, steps, reward, info = _run_random_episode(env, seed=seed)
            assert done, f"Episode with seed={seed} did not complete"

    def test_episode_step_outputs_valid(self, env):
        """Step outputs should have correct types/shapes throughout."""
        obs, info = env.reset(seed=42)
        rng = np.random.RandomState(42)
        done = False
        steps = 0
        while not done and steps < 10000:
            mask = info["action_mask"]
            valid = np.where(mask == 1)[0]
            action = int(rng.choice(valid))
            obs, reward, terminated, truncated, info = env.step(action)
            assert obs.shape == (RUN_OBS_SIZE,)
            assert obs.dtype == np.float32
            assert isinstance(reward, float)
            assert isinstance(terminated, bool)
            assert isinstance(truncated, bool)
            assert "action_mask" in info
            done = terminated or truncated
            steps += 1


# ---------------------------------------------------------------------------
# Test: action masks are correct per phase
# ---------------------------------------------------------------------------

class TestActionMasksPerPhase:
    def test_map_choice_mask_in_range(self, seeded_env):
        """After reset, phase is MAP_CHOICE; mask should only have
        bits set in the MAP range."""
        mask = seeded_env.action_masks()
        # Bits outside MAP range should all be 0
        before = mask[:_MAP_START]
        after = mask[_MAP_START + _MAP_SIZE:]
        assert np.sum(before) == 0, "Non-zero mask bits before MAP range"
        assert np.sum(after) == 0, "Non-zero mask bits after MAP range"
        # At least one MAP action should be valid
        assert np.sum(mask[_MAP_START: _MAP_START + _MAP_SIZE]) >= 1

    def test_combat_mask_in_range(self, env):
        """When in combat, mask should be within COMBAT range."""
        obs, info = env.reset(seed=42)
        mask = info["action_mask"]
        valid = np.where(mask == 1)[0]
        obs, reward, terminated, truncated, info = env.step(int(valid[0]))

        if _get_phase(env) == RunManager.PHASE_COMBAT:
            mask = info["action_mask"]
            before = mask[:_COMBAT_START]
            after = mask[_COMBAT_START + _COMBAT_SIZE:]
            assert np.sum(before) == 0
            assert np.sum(after) == 0
            assert np.sum(mask[_COMBAT_START: _COMBAT_START + _COMBAT_SIZE]) >= 1

    def test_mask_always_has_valid_action(self, env):
        """At every step, the mask should have at least one valid action."""
        obs, info = env.reset(seed=42)
        rng = np.random.RandomState(42)
        for _ in range(200):
            mask = info["action_mask"]
            assert np.sum(mask) >= 1, (
                f"No valid actions in phase {info.get('phase', '?')}"
            )
            valid = np.where(mask == 1)[0]
            action = int(rng.choice(valid))
            obs, reward, terminated, truncated, info = env.step(action)
            if terminated or truncated:
                break

    def test_mask_values_are_binary(self, seeded_env):
        mask = seeded_env.action_masks()
        assert np.all((mask == 0) | (mask == 1))

    def test_mask_matches_info_mask(self, env):
        obs, info = env.reset(seed=42)
        mask_method = env.action_masks()
        mask_info = info["action_mask"]
        np.testing.assert_array_equal(mask_method, mask_info)

    def test_pending_choice_action_zero_confirms_choice(self, env):
        env.reset(seed=42)

        mgr = RunManager(seed=1, character_id="Ironclad")
        mgr._phase = RunManager.PHASE_COMBAT
        combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=create_ironclad_starter_deck(),
            rng_seed=7,
            character_id="Ironclad",
        )
        creature, ai = create_shrinker_beetle(Rng(7))
        combat.add_enemy(creature, ai)
        strike = make_strike_ironclad()
        combat.hand = [create_card(CardId.PURITY), strike]
        combat.energy = 1
        mgr._combat = combat
        env._mgr = mgr

        assert combat.play_card(0)
        assert combat.pending_choice is not None

        env._step_combat(_COMBAT_START)

        assert combat.pending_choice is None
        assert strike in combat.hand

    def test_card_reward_mask(self, env):
        """Force into CARD_REWARD phase and verify mask."""
        obs, info = env.reset(seed=42)
        rng = np.random.RandomState(42)
        for _ in range(500):
            mask = info["action_mask"]
            valid = np.where(mask == 1)[0]
            action = int(rng.choice(valid))
            obs, reward, terminated, truncated, info = env.step(action)
            if terminated or truncated:
                break
            if _get_phase(env) == RunManager.PHASE_CARD_REWARD:
                mask = info["action_mask"]
                # Skip action (index 69) should be valid
                assert mask[_CARD_RWD_START + 3] == 1, "Skip should be valid in CARD_REWARD"
                # At least skip + 1 card
                assert np.sum(mask[_CARD_RWD_START: _CARD_RWD_START + _CARD_RWD_SIZE]) >= 2
                return
        pytest.skip("CARD_REWARD phase not reached with this seed")

    def test_multiplayer_combat_mask_exposes_player_select_actions(self, env):
        env.reset(seed=42)
        mgr = RunManager(seed=4, character_id="Ironclad")
        mgr._phase = RunManager.PHASE_COMBAT
        combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=create_ironclad_starter_deck(),
            rng_seed=21,
            character_id="Ironclad",
        )
        creature, ai = create_shrinker_beetle(Rng(21))
        combat.add_enemy(creature, ai)
        ally_state = PlayerState(player_id=2, character_id="Regent", max_hp=60, current_hp=60)
        ally = combat.add_ally_player(ally_state)
        ally_combat_state = combat.combat_player_state_for(ally)
        assert ally_combat_state is not None
        card = make_gather_light()
        card.owner = ally
        ally_combat_state.hand = [card]
        ally_combat_state.zone_map["hand"] = ally_combat_state.hand
        ally_combat_state.energy = 1
        mgr._combat = combat
        env._mgr = mgr

        mask = env.action_masks()
        assert np.sum(mask[_PLAYER_SELECT_START:_PLAYER_SELECT_START + _PLAYER_SELECT_SIZE]) >= 2

    def test_rest_site_mask(self, env):
        """Force into REST_SITE phase and verify mask."""
        for seed in range(50):
            obs, info = env.reset(seed=seed)
            rng = np.random.RandomState(seed)
            for _ in range(500):
                mask = info["action_mask"]
                valid = np.where(mask == 1)[0]
                if len(valid) == 0:
                    break
                action = int(rng.choice(valid))
                obs, reward, terminated, truncated, info = env.step(action)
                if terminated or truncated:
                    break
                if _get_phase(env) == RunManager.PHASE_REST_SITE:
                    mask = info["action_mask"]
                    assert np.sum(mask[_REST_START: _REST_START + _REST_SIZE]) >= 1
                    return
        pytest.skip("No rest site encountered in tested seeds")

    def test_shop_mask(self, env):
        """Force into SHOP phase and verify leave is always valid."""
        for seed in range(50):
            obs, info = env.reset(seed=seed)
            rng = np.random.RandomState(seed)
            for _ in range(500):
                mask = info["action_mask"]
                valid = np.where(mask == 1)[0]
                if len(valid) == 0:
                    break
                action = int(rng.choice(valid))
                obs, reward, terminated, truncated, info = env.step(action)
                if terminated or truncated:
                    break
                if _get_phase(env) == RunManager.PHASE_SHOP:
                    mask = info["action_mask"]
                    assert mask[_SHOP_START] == 1, "Leave should be valid in SHOP"
                    return
        pytest.skip("No shop encountered in tested seeds")

    def test_event_mask(self, env):
        """Force into EVENT phase and verify at least one action valid."""
        for seed in range(50):
            obs, info = env.reset(seed=seed)
            rng = np.random.RandomState(seed)
            for _ in range(500):
                mask = info["action_mask"]
                valid = np.where(mask == 1)[0]
                if len(valid) == 0:
                    break
                action = int(rng.choice(valid))
                obs, reward, terminated, truncated, info = env.step(action)
                if terminated or truncated:
                    break
                if _get_phase(env) == RunManager.PHASE_EVENT:
                    mask = info["action_mask"]
                    assert np.sum(mask[_EVENT_START: _EVENT_START + _EVENT_SIZE]) >= 1
                    return
        pytest.skip("No event encountered in tested seeds")

    def test_treasure_mask(self, env):
        """Force into TREASURE phase and verify mask."""
        for seed in range(50):
            obs, info = env.reset(seed=seed)
            rng = np.random.RandomState(seed)
            for _ in range(500):
                mask = info["action_mask"]
                valid = np.where(mask == 1)[0]
                if len(valid) == 0:
                    break
                action = int(rng.choice(valid))
                obs, reward, terminated, truncated, info = env.step(action)
                if terminated or truncated:
                    break
                if _get_phase(env) == RunManager.PHASE_TREASURE:
                    mask = info["action_mask"]
                    assert mask[_TREASURE_START] == 1
                    return
        pytest.skip("No treasure encountered in tested seeds")


# ---------------------------------------------------------------------------
# Test: 50 random episodes complete without errors
# ---------------------------------------------------------------------------

class TestStress50Episodes:
    def test_50_random_episodes_no_errors(self, env):
        """Run 50 episodes with random valid actions; none should crash."""
        completed = 0
        for seed in range(50):
            done, steps, reward, info = _run_random_episode(env, seed=seed)
            assert done, f"Episode seed={seed} did not terminate"
            completed += 1
        assert completed == 50

    def test_50_episodes_obs_always_valid(self):
        """Observations remain in-bounds throughout all 50 episodes."""
        env = STS2RunEnv(character_id="Ironclad", ascension_level=0)
        low = env.observation_space.low
        high = env.observation_space.high

        for seed in range(50):
            obs, info = env.reset(seed=seed)
            rng = np.random.RandomState(seed + 1000)
            done = False
            steps = 0
            while not done and steps < 10000:
                assert np.all(np.isfinite(obs)), (
                    f"NaN/inf at seed={seed}, step={steps}"
                )
                violations = np.where((obs < low) | (obs > high))[0]
                assert len(violations) == 0, (
                    f"Out of range at seed={seed}, step={steps}, "
                    f"indices={violations}"
                )

                mask = info["action_mask"]
                valid = np.where(mask == 1)[0]
                action = int(rng.choice(valid))
                obs, reward, terminated, truncated, info = env.step(action)
                done = terminated or truncated
                steps += 1

    def test_50_episodes_return_float_rewards(self):
        """Every reward returned should be a float."""
        env = STS2RunEnv(character_id="Ironclad", ascension_level=0)
        for seed in range(50):
            obs, info = env.reset(seed=seed)
            rng = np.random.RandomState(seed + 2000)
            done = False
            steps = 0
            while not done and steps < 10000:
                mask = info["action_mask"]
                valid = np.where(mask == 1)[0]
                action = int(rng.choice(valid))
                obs, reward, terminated, truncated, info = env.step(action)
                assert isinstance(reward, float), (
                    f"Reward is {type(reward)} at seed={seed}, step={steps}"
                )
                done = terminated or truncated
                steps += 1


# ---------------------------------------------------------------------------
# Test: phase transitions
# ---------------------------------------------------------------------------

class TestPhaseTransitions:
    def test_map_choice_to_room(self, env):
        """Choosing a map node should transition to some room phase."""
        obs, info = env.reset(seed=42)
        mask = info["action_mask"]
        valid = np.where(mask == 1)[0]
        obs, reward, terminated, truncated, info = env.step(int(valid[0]))
        phase = _get_phase(env)
        assert phase in (
            RunManager.PHASE_COMBAT,
            RunManager.PHASE_REST_SITE,
            RunManager.PHASE_SHOP,
            RunManager.PHASE_EVENT,
            RunManager.PHASE_MAP_CHOICE,  # treasure auto-resolves
            RunManager.PHASE_TREASURE,
        )

    def test_phases_cycle_through_run(self, env):
        """Run an episode and check that multiple phases are visited."""
        obs, info = env.reset(seed=42)
        rng = np.random.RandomState(42)
        visited_phases = set()
        done = False
        steps = 0
        while not done and steps < 10000:
            visited_phases.add(info.get("phase", "?"))
            mask = info["action_mask"]
            valid = np.where(mask == 1)[0]
            action = int(rng.choice(valid))
            obs, reward, terminated, truncated, info = env.step(action)
            done = terminated or truncated
            steps += 1
        # Should visit at least MAP_CHOICE and COMBAT
        assert RunManager.PHASE_MAP_CHOICE in visited_phases
        assert RunManager.PHASE_COMBAT in visited_phases


# ---------------------------------------------------------------------------
# Test: observation encoding
# ---------------------------------------------------------------------------

class TestObservationEncoding:
    def test_obs_size_matches_constant(self, seeded_env):
        obs = seeded_env._encode_obs()
        assert obs.shape == (RUN_OBS_SIZE,)

    def test_obs_changes_after_step(self, env):
        """Obs should change after taking a step."""
        obs1, info = env.reset(seed=42)
        mask = info["action_mask"]
        valid = np.where(mask == 1)[0]
        obs2, _, _, _, _ = env.step(int(valid[0]))
        assert not np.array_equal(obs1, obs2)

    def test_combat_obs_populated_during_combat(self, env):
        """The first 131 dims (combat obs) should be non-zero during combat."""
        obs, info = env.reset(seed=42)
        rng = np.random.RandomState(42)
        for _ in range(200):
            mask = info["action_mask"]
            valid = np.where(mask == 1)[0]
            action = int(rng.choice(valid))
            obs, reward, terminated, truncated, info = env.step(action)
            if terminated or truncated:
                break
            if info.get("phase") == RunManager.PHASE_COMBAT:
                # Combat portion of obs should have non-zero values
                combat_obs = obs[:131]
                assert np.any(combat_obs != 0), "Combat obs is all zeros during combat"
                return
        pytest.skip("No combat phase reached")

    def test_run_state_obs_always_present(self, seeded_env):
        """Run-state portion (last 20 dims) should have some non-zero values."""
        obs = seeded_env._encode_obs()
        run_state_obs = obs[131:]
        assert np.any(run_state_obs != 0), "Run-state obs is all zeros"


# ---------------------------------------------------------------------------
# Test: reward structure
# ---------------------------------------------------------------------------

class TestRewardStructure:
    def test_sparse_reward_at_death(self, env):
        """At death, cumulative reward should be -1."""
        done, steps, reward, info = _run_random_episode(env, seed=42)
        assert done
        # Random agent almost always dies -> reward should be -1
        assert reward == pytest.approx(-1.0) or reward == pytest.approx(1.0)

    def test_mid_run_reward_is_zero(self, env):
        """Before episode ends, individual step rewards should be 0."""
        obs, info = env.reset(seed=42)
        rng = np.random.RandomState(42)
        for _ in range(50):
            mask = info["action_mask"]
            valid = np.where(mask == 1)[0]
            action = int(rng.choice(valid))
            obs, reward, terminated, truncated, info = env.step(action)
            if terminated or truncated:
                break
            assert reward == 0.0, f"Non-zero mid-run reward: {reward}"


# ---------------------------------------------------------------------------
# Test: render
# ---------------------------------------------------------------------------

class TestRender:
    def test_ansi_render(self):
        env = STS2RunEnv(character_id="Ironclad", render_mode="ansi")
        env.reset(seed=42)
        rendered = env.render()
        assert isinstance(rendered, str)
        assert "STS2 Run" in rendered

    def test_no_render_mode(self):
        env = STS2RunEnv(character_id="Ironclad", render_mode=None)
        env.reset(seed=42)
        rendered = env.render()
        assert rendered is None


# ---------------------------------------------------------------------------
# Test: multi-character
# ---------------------------------------------------------------------------

class TestMultiCharacter:
    @pytest.mark.parametrize("char_id", ["Ironclad", "Silent", "Defect"])
    def test_character_completes_episode(self, char_id):
        env = STS2RunEnv(character_id=char_id, ascension_level=0, max_steps=10000)
        done, steps, reward, info = _run_random_episode(env, seed=42)
        assert done, f"{char_id} episode did not complete"
