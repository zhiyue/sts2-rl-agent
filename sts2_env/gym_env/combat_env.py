"""STS2 Combat Gymnasium Environment."""

from __future__ import annotations

import logging

import gymnasium
import numpy as np
from gymnasium import spaces

from sts2_env.cards.base import reset_instance_counter
from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.core.combat import CombatState
from sts2_env.core.constants import ACTION_END_TURN, ACTION_SPACE_SIZE, IRONCLAD_STARTING_HP
from sts2_env.encounters.act1 import ALL_ACT1_ENCOUNTERS, EncounterSetup
from sts2_env.core.rng import Rng
from sts2_env.gym_env.action_space import (
    action_to_card_and_target,
    action_to_potion_and_target,
    get_action_mask,
    is_potion_action,
)
from sts2_env.gym_env.observation import OBS_SIZE, encode_observation
from sts2_env.gym_env.reward import compute_reward

logger = logging.getLogger(__name__)


class STS2CombatEnv(gymnasium.Env):
    """Gymnasium environment for a single STS2 combat encounter.

    Observation: flat float32 vector encoding player state, hand, piles, enemies.
    Action: fixed discrete combat action space including cards, end turn, and potions.
    """

    metadata = {"render_modes": ["ansi"]}

    def __init__(
        self,
        encounter_pool: list[EncounterSetup] | None = None,
        player_hp: int = IRONCLAD_STARTING_HP,
        player_max_hp: int = IRONCLAD_STARTING_HP,
        max_turns: int = 200,
        render_mode: str | None = None,
    ):
        super().__init__()
        self.observation_space = spaces.Box(
            low=-1.0, high=10.0, shape=(OBS_SIZE,), dtype=np.float32
        )
        self.action_space = spaces.Discrete(ACTION_SPACE_SIZE)
        self.encounter_pool = encounter_pool or ALL_ACT1_ENCOUNTERS
        self.player_hp = player_hp
        self.player_max_hp = player_max_hp
        self.max_turns = max_turns
        self.render_mode = render_mode

        self.combat: CombatState | None = None

    def reset(self, seed=None, options=None):
        super().reset(seed=seed)
        reset_instance_counter()

        rng_seed = int(self.np_random.integers(0, 2**31))
        rng = Rng(rng_seed)

        # Create deck
        deck = create_ironclad_starter_deck()

        # Create combat
        self.combat = CombatState(
            player_hp=self.player_hp,
            player_max_hp=self.player_max_hp,
            deck=deck,
            rng_seed=rng_seed,
            character_id="Ironclad",
        )

        # Setup encounter
        encounter_idx = int(self.np_random.integers(0, len(self.encounter_pool)))
        encounter_setup = self.encounter_pool[encounter_idx]
        encounter_setup(self.combat, rng)

        # Start combat
        self.combat.start_combat()

        obs = encode_observation(self.combat)
        info = {"action_mask": get_action_mask(self.combat)}
        return obs, info

    def step(self, action: int):
        assert self.combat is not None, "Must call reset() first"

        prev_hp = self.combat.player.current_hp
        if self.combat.pending_choice is not None:
            if action == ACTION_END_TURN:
                self.combat.resolve_pending_choice(None)
            else:
                self.combat.resolve_pending_choice(action - 1)
        else:
            if action == ACTION_END_TURN:
                self.combat.end_player_turn()
            elif is_potion_action(action):
                slot_idx, target_idx = action_to_potion_and_target(action)
                success = (
                    slot_idx is not None
                    and self.combat.use_potion(slot_idx, target_index=target_idx)
                )
                if not success:
                    logger.debug("Ignored invalid potion action %d", action)
            else:
                hand_idx, target_idx = action_to_card_and_target(action)
                success = hand_idx is not None and self.combat.play_card(hand_idx, target_idx)
                if not success:
                    logger.debug("Ignored invalid card action %d", action)

        obs = encode_observation(self.combat)
        reward = compute_reward(self.combat, prev_hp)
        terminated = self.combat.is_over
        truncated = self.combat.turn_count > self.max_turns
        info = {"action_mask": get_action_mask(self.combat)}

        return obs, reward, terminated, truncated, info

    def action_masks(self) -> np.ndarray:
        """Return the current action mask (for sb3-contrib MaskablePPO)."""
        if self.combat is None:
            mask = np.zeros(ACTION_SPACE_SIZE, dtype=np.int8)
            mask[0] = 1
            return mask
        return get_action_mask(self.combat)

    def render(self):
        if self.render_mode == "ansi" and self.combat is not None:
            return str(self.combat)
        return None
