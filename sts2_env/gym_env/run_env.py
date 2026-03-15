"""STS2 Full-Run Gymnasium Environment.

Wraps :class:`RunManager` to expose a complete STS2 run (Acts 0-2) as a
single Gymnasium episode.  Each ``step()`` call corresponds to one player
decision; non-interactive transitions (treasure collection, post-combat
bookkeeping, act transitions) are handled internally by RunManager.

Action space
------------
``Discrete(100)`` with **action masking** so that only the actions valid
for the current game phase are unmasked:

===========  ========  ====================================================
 Slice        Phase     Meaning
===========  ========  ====================================================
 0            COMBAT    End turn / Continue / Skip
 1-10         COMBAT    Play card from hand slot 0-9 (self-target)
 11-60        COMBAT    Play card *i* targeting enemy *j* (i*5+j)
 61-65        MAP       Map choice 0-4
 66-69        CARD_RWD  Card reward pick 0-2, or skip (69)
 70-72        BOSS_REL  Boss relic choice 0-2
 73-82        SHOP      Shop action 0-9
 83-87        REST      Rest-site option 0-4
 88-91        EVENT     Event option 0-3
 92           TREASURE  Take and continue
 93-99        --        Reserved
===========  ========  ====================================================

Observation space
-----------------
Flat ``float32`` vector of size ``RUN_OBS_SIZE`` (151).

* Combat observation (131) -- reuses :func:`encode_observation`.
* Run-level state (20):
  - current_act / 3, total_floor / 50, act_floor / 20           (3)
  - player HP ratio, gold / 1000                                 (2)
  - deck_size / 40, num_relics / 30                              (2)
  - num_potions / max_potion_slots (or 0)                        (2)
  - phase one-hot (8 phases)                                     (8)
  - ascension_level / 20                                         (1)
  - is_elite flag, is_boss flag                                  (2)

Reward
------
Sparse: **+1** for winning the run, **-1** for death or timeout.

Compatibility
-------------
Implements ``action_masks()`` for *sb3-contrib* ``MaskablePPO``.
"""

from __future__ import annotations

from typing import Any

import gymnasium
import numpy as np
from gymnasium import spaces

from sts2_env.core.constants import (
    ACTION_SPACE_SIZE as COMBAT_ACTION_SPACE_SIZE,
    MAX_ENEMIES,
    MAX_HAND_SIZE,
)
from sts2_env.core.enums import RoomType
from sts2_env.gym_env.action_space import action_to_card_and_target, get_action_mask
from sts2_env.gym_env.observation import OBS_SIZE as COMBAT_OBS_SIZE, encode_observation
from sts2_env.run.run_manager import RunManager

# ---------------------------------------------------------------------------
# Unified action-space layout   (Discrete(100))
# ---------------------------------------------------------------------------

TOTAL_ACTIONS = 100

# Combat (mirrors existing 61-action combat env)
_COMBAT_START = 0       # 0 = end turn, 1-10 self-target, 11-60 targeted
_COMBAT_SIZE = COMBAT_ACTION_SPACE_SIZE  # 61

# Map choice
_MAP_START = 61
_MAP_SIZE = 5           # up to 5 paths

# Card reward
_CARD_RWD_START = 66
_CARD_RWD_SIZE = 4      # 66-68 = pick card 0-2, 69 = skip

# Boss relic
_BOSS_RELIC_START = 70
_BOSS_RELIC_SIZE = 3

# Shop
_SHOP_START = 73
_SHOP_SIZE = 10         # 0 = leave, 1-5 buy card, 6-7 buy relic,
#                         8 buy potion, 9 = card removal

# Rest site
_REST_START = 83
_REST_SIZE = 5

# Event
_EVENT_START = 88
_EVENT_SIZE = 4

# Treasure
_TREASURE_START = 92
_TREASURE_SIZE = 1

# ---------------------------------------------------------------------------
# Phase index for one-hot encoding (8 phases)
# ---------------------------------------------------------------------------

_PHASE_INDEX: dict[str, int] = {
    RunManager.PHASE_MAP_CHOICE: 0,
    RunManager.PHASE_COMBAT:     1,
    RunManager.PHASE_CARD_REWARD: 2,
    RunManager.PHASE_BOSS_RELIC:  3,
    RunManager.PHASE_SHOP:        4,
    RunManager.PHASE_REST_SITE:   5,
    RunManager.PHASE_EVENT:       6,
    RunManager.PHASE_TREASURE:    7,
}
NUM_PHASES = len(_PHASE_INDEX)

# ---------------------------------------------------------------------------
# Observation size
# ---------------------------------------------------------------------------

_RUN_STATE_SIZE = 20   # see module docstring
RUN_OBS_SIZE = COMBAT_OBS_SIZE + _RUN_STATE_SIZE  # 131 + 20 = 151

# ---------------------------------------------------------------------------
# Reward constants
# ---------------------------------------------------------------------------

REWARD_WIN = 1.0
REWARD_DEATH = -1.0


class STS2RunEnv(gymnasium.Env):
    """Gymnasium environment for a complete STS2 run.

    Wraps :class:`RunManager` with a ``Discrete(100)`` action space and
    action masking suitable for ``MaskablePPO``.

    Parameters
    ----------
    character_id : str
        Character to play (``"Ironclad"``, ``"Silent"``, etc.).
    ascension_level : int
        Ascension level (0-20).
    max_steps : int
        Maximum environment steps before the episode is truncated.
    max_combat_turns : int
        Maximum turns within a single combat before it is force-ended
        as a loss.
    render_mode : str or None
        ``"ansi"`` for text rendering.
    """

    metadata = {"render_modes": ["ansi"]}

    def __init__(
        self,
        character_id: str = "Ironclad",
        ascension_level: int = 0,
        max_steps: int = 10000,
        max_combat_turns: int = 200,
        render_mode: str | None = None,
    ):
        super().__init__()

        self.observation_space = spaces.Box(
            low=-1.0, high=10.0, shape=(RUN_OBS_SIZE,), dtype=np.float32,
        )
        self.action_space = spaces.Discrete(TOTAL_ACTIONS)

        self._character_id = character_id
        self._ascension_level = ascension_level
        self.max_steps = max_steps
        self.max_combat_turns = max_combat_turns
        self.render_mode = render_mode

        # Mutable state -- set during reset()
        self._mgr: RunManager | None = None
        self._step_count: int = 0

    # ------------------------------------------------------------------
    # Gymnasium API
    # ------------------------------------------------------------------

    def reset(
        self,
        seed: int | None = None,
        options: dict[str, Any] | None = None,
    ) -> tuple[np.ndarray, dict[str, Any]]:
        super().reset(seed=seed)

        run_seed = int(self.np_random.integers(0, 2**31))
        self._mgr = RunManager(
            seed=run_seed,
            character_id=self._character_id,
            ascension_level=self._ascension_level,
        )
        self._step_count = 0

        obs = self._encode_obs()
        info = self._build_info()
        return obs, info

    def step(
        self, action: int,
    ) -> tuple[np.ndarray, float, bool, bool, dict[str, Any]]:
        assert self._mgr is not None, "Must call reset() before step()"
        self._step_count += 1

        reward = 0.0
        phase = self._mgr.phase

        # ---- dispatch action to RunManager ----
        try:
            if phase == RunManager.PHASE_COMBAT:
                self._step_combat(action)
            elif phase == RunManager.PHASE_MAP_CHOICE:
                self._step_map_choice(action)
            elif phase == RunManager.PHASE_CARD_REWARD:
                self._step_card_reward(action)
            elif phase == RunManager.PHASE_BOSS_RELIC:
                self._step_boss_relic(action)
            elif phase == RunManager.PHASE_SHOP:
                self._step_shop(action)
            elif phase == RunManager.PHASE_REST_SITE:
                self._step_rest_site(action)
            elif phase == RunManager.PHASE_EVENT:
                self._step_event(action)
            elif phase == RunManager.PHASE_TREASURE:
                self._step_treasure()
        except Exception:
            # Guard against simulation bugs so the episode can finish.
            # Force-end as a loss if the run is not already over.
            if not self._mgr.is_over:
                self._mgr.run_state.lose_run()

        # ---- terminal conditions ----
        terminated = self._mgr.is_over
        truncated = False

        if not terminated and self._step_count >= self.max_steps:
            truncated = True

        if terminated:
            reward = REWARD_WIN if self._mgr.player_won else REWARD_DEATH
        elif truncated:
            reward = REWARD_DEATH

        obs = self._encode_obs()
        info = self._build_info()
        return obs, float(reward), terminated, truncated, info

    def action_masks(self) -> np.ndarray:
        """Return a boolean mask over ``Discrete(100)`` actions.

        Required by *sb3-contrib* ``MaskablePPO``.
        """
        mask = np.zeros(TOTAL_ACTIONS, dtype=np.int8)

        if self._mgr is None or self._mgr.is_over:
            # Fallback: unmask a single action so sampling never fails.
            mask[0] = 1
            return mask

        phase = self._mgr.phase
        actions = self._mgr.get_available_actions()

        if phase == RunManager.PHASE_COMBAT:
            combat = self._mgr.get_combat_state()
            if combat is not None:
                combat_mask = get_action_mask(combat)
                n = min(len(combat_mask), _COMBAT_SIZE)
                mask[_COMBAT_START: _COMBAT_START + n] = combat_mask[:n]
            else:
                mask[_COMBAT_START] = 1  # end turn fallback

        elif phase == RunManager.PHASE_MAP_CHOICE:
            n = min(len(actions), _MAP_SIZE)
            for i in range(n):
                mask[_MAP_START + i] = 1

        elif phase == RunManager.PHASE_CARD_REWARD:
            # skip is always available
            mask[_CARD_RWD_START + 3] = 1
            n_cards = sum(
                1 for a in actions if a.get("action") == "pick_card"
            )
            for i in range(min(n_cards, 3)):
                mask[_CARD_RWD_START + i] = 1

        elif phase == RunManager.PHASE_BOSS_RELIC:
            n = min(
                sum(1 for a in actions if a.get("action") == "pick_relic"),
                _BOSS_RELIC_SIZE,
            )
            for i in range(n):
                mask[_BOSS_RELIC_START + i] = 1

        elif phase == RunManager.PHASE_SHOP:
            # Action 0 = leave (always valid when in shop)
            mask[_SHOP_START] = 1
            self._mask_shop(actions, mask)

        elif phase == RunManager.PHASE_REST_SITE:
            rest_actions = [
                a for a in actions if a.get("action") == "rest_option"
            ]
            for i in range(min(len(rest_actions), _REST_SIZE)):
                mask[_REST_START + i] = 1

        elif phase == RunManager.PHASE_EVENT:
            event_actions = [
                a for a in actions if a.get("action") == "event_choice"
            ]
            for i in range(min(len(event_actions), _EVENT_SIZE)):
                mask[_EVENT_START + i] = 1

        elif phase == RunManager.PHASE_TREASURE:
            mask[_TREASURE_START] = 1

        # Safety: guarantee at least one action is unmasked.
        if mask.sum() == 0:
            mask[0] = 1

        return mask

    # ------------------------------------------------------------------
    # Action dispatch helpers
    # ------------------------------------------------------------------

    def _step_combat(self, action: int) -> None:
        """Translate a unified action index into a RunManager combat action."""
        mgr = self._mgr
        assert mgr is not None
        combat = mgr.get_combat_state()
        if combat is None or combat.is_over:
            return

        local = max(0, min(action - _COMBAT_START, _COMBAT_SIZE - 1))
        hand_idx, target_idx = action_to_card_and_target(local)

        if hand_idx is None:
            mgr.take_action({"action": "end_turn"})
        else:
            act: dict[str, Any] = {
                "action": "play_card",
                "hand_index": hand_idx,
            }
            if target_idx is not None:
                act["target_index"] = target_idx
            result = mgr.take_action(act)
            # If the card play was invalid (e.g. bad index), fall back to
            # ending the turn so the episode keeps progressing.
            if not result.get("success", True) and not mgr.is_over:
                if mgr.phase == RunManager.PHASE_COMBAT:
                    combat2 = mgr.get_combat_state()
                    if combat2 is not None and not combat2.is_over:
                        mgr.take_action({"action": "end_turn"})

        # Force-end combat if it exceeds the turn limit.
        if mgr.phase == RunManager.PHASE_COMBAT:
            combat_now = mgr.get_combat_state()
            if (
                combat_now is not None
                and not combat_now.is_over
                and combat_now.turn_count > self.max_combat_turns
            ):
                mgr.run_state.player.current_hp = 0
                mgr.run_state.lose_run()

    def _step_map_choice(self, action: int) -> None:
        mgr = self._mgr
        assert mgr is not None
        actions = mgr.get_available_actions()
        if not actions:
            return
        local = max(0, min(action - _MAP_START, len(actions) - 1))
        mgr.take_action(actions[local])

    def _step_card_reward(self, action: int) -> None:
        mgr = self._mgr
        assert mgr is not None
        local = action - _CARD_RWD_START

        if local == 3 or local < 0 or local > 3:
            # Skip
            mgr.take_action({"action": "skip"})
        else:
            mgr.take_action({"action": "pick_card", "index": local})

    def _step_boss_relic(self, action: int) -> None:
        mgr = self._mgr
        assert mgr is not None
        local = max(0, min(action - _BOSS_RELIC_START, _BOSS_RELIC_SIZE - 1))
        mgr.take_action({"action": "pick_relic", "index": local})

    def _step_shop(self, action: int) -> None:
        mgr = self._mgr
        assert mgr is not None
        local = action - _SHOP_START

        if local <= 0 or local >= _SHOP_SIZE:
            mgr.take_action({"action": "leave_shop"})
            return

        # Map local index to a concrete shop action from RunManager.
        # We build a canonical ordered list of buyable actions and pick by
        # index.  Action 0 = leave (handled above), 1-N = buy items.
        buyable = self._get_shop_buy_actions()
        idx = local - 1
        if 0 <= idx < len(buyable):
            mgr.take_action(buyable[idx])
        else:
            # Index out of range -- just leave the shop.
            mgr.take_action({"action": "leave_shop"})

    def _step_rest_site(self, action: int) -> None:
        mgr = self._mgr
        assert mgr is not None
        actions = [
            a for a in mgr.get_available_actions()
            if a.get("action") == "rest_option"
        ]
        if not actions:
            return
        local = max(0, min(action - _REST_START, len(actions) - 1))
        mgr.take_action(actions[local])

    def _step_event(self, action: int) -> None:
        mgr = self._mgr
        assert mgr is not None
        actions = [
            a for a in mgr.get_available_actions()
            if a.get("action") == "event_choice"
        ]
        if not actions:
            return
        local = max(0, min(action - _EVENT_START, len(actions) - 1))
        mgr.take_action(actions[local])

    def _step_treasure(self) -> None:
        mgr = self._mgr
        assert mgr is not None
        mgr.take_action({"action": "collect"})

    # ------------------------------------------------------------------
    # Shop helpers
    # ------------------------------------------------------------------

    def _get_shop_buy_actions(self) -> list[dict]:
        """Return the ordered list of buyable shop actions (excludes leave)."""
        mgr = self._mgr
        assert mgr is not None
        return [
            a for a in mgr.get_available_actions()
            if a.get("action") not in ("leave_shop",)
        ]

    def _mask_shop(self, actions: list[dict], mask: np.ndarray) -> None:
        """Populate *mask* for shop buy-actions at indices 1..N."""
        buyable = [
            a for a in actions if a.get("action") != "leave_shop"
        ]
        for i in range(min(len(buyable), _SHOP_SIZE - 1)):
            mask[_SHOP_START + 1 + i] = 1

    # ------------------------------------------------------------------
    # Observation encoding
    # ------------------------------------------------------------------

    def _encode_obs(self) -> np.ndarray:
        """Encode the full run state as a flat float32 vector."""
        obs = np.zeros(RUN_OBS_SIZE, dtype=np.float32)

        mgr = self._mgr
        if mgr is None:
            return obs

        # ---- Combat observation (131 dims) ----
        combat = mgr.get_combat_state()
        if combat is not None:
            combat_obs = encode_observation(combat)
            n = min(len(combat_obs), COMBAT_OBS_SIZE)
            obs[:n] = combat_obs[:n]

        # ---- Run-level state (20 dims) ----
        rs = mgr.run_state
        player = rs.player
        idx = COMBAT_OBS_SIZE

        # Act / floor (3)
        obs[idx + 0] = rs.current_act_index / 3.0
        obs[idx + 1] = rs.total_floor / 50.0
        obs[idx + 2] = rs.act_floor / 20.0

        # HP ratio, gold (2)
        obs[idx + 3] = (
            player.current_hp / max(player.max_hp, 1)
        )
        obs[idx + 4] = player.gold / 1000.0

        # Deck size, relic count (2)
        obs[idx + 5] = len(player.deck) / 40.0
        obs[idx + 6] = len(rs.relics) / 30.0

        # Potions (2)
        num_potions = sum(1 for p in player.potions if p is not None)
        max_slots = max(player.max_potion_slots, 1)
        obs[idx + 7] = num_potions / max_slots
        obs[idx + 8] = player.max_potion_slots / 5.0

        # Phase one-hot (8)
        phase_idx = _PHASE_INDEX.get(mgr.phase, 0)
        obs[idx + 9 + phase_idx] = 1.0

        # Ascension (1)
        obs[idx + 17] = rs.ascension_level / 20.0

        # is_elite, is_boss flags (2)
        room = mgr._current_room_type
        obs[idx + 18] = 1.0 if room == RoomType.ELITE else 0.0
        obs[idx + 19] = 1.0 if room == RoomType.BOSS else 0.0

        np.clip(obs, -1.0, 10.0, out=obs)
        return obs

    # ------------------------------------------------------------------
    # Info dict
    # ------------------------------------------------------------------

    def _build_info(self) -> dict[str, Any]:
        """Build the ``info`` dict returned by ``reset()`` and ``step()``."""
        info: dict[str, Any] = {"action_mask": self.action_masks()}
        if self._mgr is not None:
            rs = self._mgr.run_state
            info.update({
                "phase": self._mgr.phase,
                "act": rs.current_act_index,
                "floor": rs.total_floor,
                "hp": rs.player.current_hp,
                "max_hp": rs.player.max_hp,
                "gold": rs.player.gold,
                "deck_size": len(rs.player.deck),
                "relics": len(rs.relics),
                "step": self._step_count,
            })
        return info

    # ------------------------------------------------------------------
    # Render
    # ------------------------------------------------------------------

    def render(self) -> str | None:
        if self.render_mode != "ansi" or self._mgr is None:
            return None
        s = self._mgr.summary()
        lines = [
            "=== STS2 Run ===",
            f"Phase: {s['phase']}  |  Act {s['act']}  Floor {s['floor']}",
            f"HP: {s['hp']}  Gold: {s['gold']}  Deck: {s['deck_size']}",
            f"Relics: {s['relics']}",
            f"Step: {self._step_count}",
        ]
        combat = self._mgr.get_combat_state()
        if combat is not None:
            lines.append(str(combat))
        return "\n".join(lines)
