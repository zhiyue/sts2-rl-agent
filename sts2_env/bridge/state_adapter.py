"""State adapter: converts game JSON state to observation vectors.

Translates the JSON state received from the C# bridge mod into the
same flat float32 observation vector format used by the gym_env. This
ensures the trained model receives inputs in the exact same encoding
it was trained on.

The observation format is defined in gym_env/observation.py:
  - Player state: hp/max_hp, block/50, energy/10, max_energy/10 (4)
  - Player powers: str, dex, vuln, weak, frail, artifact (6)
  - Hand cards: card_id_norm, cost, damage, block, is_attack (10 * 5 = 50)
  - Pile sizes: draw, discard, exhaust, reserved, reserved, reserved (6)
  - Enemies: alive, hp%, block, intent_onehot(5), intent_dmg,
             intent_hits, vuln, weak, str (5 * 13 = 65)
  Total: 131 dimensions
"""

from __future__ import annotations

from typing import Any

import numpy as np

from sts2_env.core.constants import (
    ACTION_END_TURN,
    ACTION_SPACE_SIZE,
    MAX_ENEMIES,
    MAX_HAND_SIZE,
    MAX_POTION_SLOTS,
    POTION_ACTION_START,
    POTION_TARGET_OPTIONS,
)
from sts2_env.core.enums import CardId
from sts2_env.gym_env.observation import (
    CARD_FEATURES,
    ENEMY_FEATURES,
    INTENT_TYPES,
    NUM_CARD_IDS,
    NUM_INTENT_TYPES,
    NUM_PLAYER_POWERS,
    OBS_SIZE,
    PILE_FEATURES,
    _CARD_ID_TO_IDX,
)
from sts2_env.bridge.protocol import (
    CardTypeName,
    IntentName,
    Phase,
    TargetTypeName,
)

# Mapping from JSON intent strings to our IntentType enum indices.
# Must match the INTENT_TYPES list in observation.py:
#   [ATTACK, MULTI_ATTACK, DEFEND, BUFF, DEBUFF]
_INTENT_STR_TO_IDX: dict[str, int] = {
    IntentName.ATTACK: 0,
    IntentName.MULTI_ATTACK: 1,
    IntentName.DEFEND: 2,
    IntentName.BUFF: 3,
    IntentName.DEBUFF: 4,
}

# Mapping from JSON card ID strings to CardId enum values,
# for card_id normalised encoding.
_CARD_STR_TO_ID: dict[str, CardId] = {cid.name: cid for cid in CardId}

# Power names to track (must match PLAYER_POWERS in observation.py)
_TRACKED_POWERS = ["STRENGTH", "DEXTERITY", "VULNERABLE", "WEAK", "FRAIL", "ARTIFACT"]

# Target types that need specific enemy targeting (for action masking)
_TARGETED_TYPES = {TargetTypeName.ANY_ENEMY, "ANY_ENEMY", "RANDOM_ENEMY", TargetTypeName.RANDOM_ENEMY}
_UNTARGETED_TYPES = {TargetTypeName.SELF, TargetTypeName.NONE, TargetTypeName.ALL_ENEMIES,
                     "SELF", "NONE", "ALL_ENEMIES", "Self", "None", "AllEnemies"}
_POTION_TARGETED_TYPES = {TargetTypeName.ANY_ENEMY, "ANY_ENEMY", "AnyEnemy"}
_POTION_UNTARGETED_TYPES = {
    TargetTypeName.SELF,
    TargetTypeName.ALL_ENEMIES,
    "SELF",
    "ANY_PLAYER",
    "ALL_ENEMIES",
    "Self",
    "AnyPlayer",
    "AllEnemies",
}


class StateAdapter:
    """Converts game state JSON to observation vectors and action masks.

    This adapter bridges the gap between the C# serializer's JSON format
    and the gym environment's numpy observation encoding.

    Usage::

        adapter = StateAdapter()
        state = client.receive_state()
        obs = adapter.encode_observation(state)
        mask = adapter.compute_action_mask(state)
    """

    def encode_observation(self, state: dict[str, Any]) -> np.ndarray:
        """Convert a game state JSON dict to a flat float32 observation vector.

        Args:
            state: Full game state dict as received from the bridge.
                   Must contain 'combat_state' with player, hand, enemies.

        Returns:
            Float32 numpy array of shape (OBS_SIZE,) = (131,).
            Returns zeros if not in combat.
        """
        obs = np.zeros(OBS_SIZE, dtype=np.float32)

        # Support both formats:
        # Legacy nested payload: state["combat_state"]["player"]
        # Current v2 payload: state["player"]
        combat = state.get("combat_state") or state
        if "player" not in combat:
            return obs

        player = combat.get("player", {})
        # Debug: print first call to verify data
        if not hasattr(self, '_logged_first'):
            self._logged_first = True
            import logging
            logging.getLogger(__name__).warning(
                "First encode_observation: player=%s, hand_count=%d, enemies_count=%d",
                player, len(combat.get("hand", [])), len(combat.get("enemies", []))
            )

        idx = 0

        # --- Player state (4) ---
        player = combat.get("player", {})
        max_hp = player.get("max_hp", 1)
        obs[idx] = player.get("hp", 0) / max(max_hp, 1)
        obs[idx + 1] = player.get("block", 0) / 50.0
        obs[idx + 2] = player.get("energy", 0) / 10.0
        obs[idx + 3] = player.get("max_energy", 3) / 10.0
        idx += 4

        # --- Player powers (6) ---
        player_powers = _powers_to_dict(player.get("powers", []))
        for power_name in _TRACKED_POWERS:
            obs[idx] = player_powers.get(power_name, 0) / 20.0
            idx += 1

        # --- Hand cards (10 * 5 = 50) ---
        hand = combat.get("hand", [])
        for i in range(MAX_HAND_SIZE):
            if i < len(hand):
                card = hand[i]
                card_id_str = card.get("id", "UNKNOWN")
                card_enum = _CARD_STR_TO_ID.get(card_id_str)
                if card_enum is not None and card_enum in _CARD_ID_TO_IDX:
                    obs[idx] = (_CARD_ID_TO_IDX[card_enum] + 1) / (NUM_CARD_IDS + 1)
                else:
                    obs[idx] = 0.0

                obs[idx + 1] = max(0, card.get("cost", 0)) / 5.0
                obs[idx + 2] = card.get("base_damage", 0) / 50.0
                obs[idx + 3] = card.get("base_block", 0) / 50.0
                obs[idx + 4] = 1.0 if card.get("type", "") == CardTypeName.ATTACK else 0.0
            idx += CARD_FEATURES

        # --- Pile summaries (6) ---
        # Keep the last three pile-summary dimensions zeroed so the bridge
        # matches gym_env/observation.py exactly.
        draw_count = combat.get("draw_pile_count", 0)
        discard_count = combat.get("discard_pile_count", 0)
        exhaust_count = combat.get("exhaust_pile_count", 0)

        obs[idx] = draw_count / 20.0
        obs[idx + 1] = discard_count / 20.0
        obs[idx + 2] = exhaust_count / 20.0
        obs[idx + 3] = 0.0
        obs[idx + 4] = 0.0
        obs[idx + 5] = 0.0
        idx += PILE_FEATURES

        # --- Enemies (5 * 13 = 65) ---
        enemies = combat.get("enemies", [])
        for i in range(MAX_ENEMIES):
            if i < len(enemies):
                enemy = enemies[i]
                is_alive = enemy.get("is_alive", False)
                enemy_max_hp = max(enemy.get("max_hp", 1), 1)

                obs[idx] = 1.0 if is_alive else 0.0
                obs[idx + 1] = enemy.get("hp", 0) / enemy_max_hp
                obs[idx + 2] = enemy.get("block", 0) / 50.0

                # Intent encoding (one-hot + damage + hits)
                if is_alive:
                    intent_str = enemy.get("intent", "UNKNOWN")
                    intent_idx = _INTENT_STR_TO_IDX.get(intent_str, -1)
                    if 0 <= intent_idx < NUM_INTENT_TYPES:
                        obs[idx + 3 + intent_idx] = 1.0
                    obs[idx + 3 + NUM_INTENT_TYPES] = enemy.get("intent_damage", 0) / 30.0
                    obs[idx + 3 + NUM_INTENT_TYPES + 1] = enemy.get("intent_hits", 1) / 5.0

                # Enemy powers
                enemy_powers = _powers_to_dict(enemy.get("powers", []))
                obs[idx + 3 + NUM_INTENT_TYPES + 2] = enemy_powers.get("VULNERABLE", 0) / 10.0
                obs[idx + 3 + NUM_INTENT_TYPES + 3] = enemy_powers.get("WEAK", 0) / 10.0
                obs[idx + 3 + NUM_INTENT_TYPES + 4] = enemy_powers.get("STRENGTH", 0) / 10.0

            idx += ENEMY_FEATURES

        return obs

    def compute_action_mask(self, state: dict[str, Any]) -> np.ndarray:
        """Compute a boolean mask of valid actions from the game state.

        The combat action space is fixed-width and includes cards, end turn,
        and potion uses.

        Card actions:
          - 0: END_TURN
          - 1..10: Play card i (untargeted: self/none/all_enemies)
          - 11..60: Play card i targeting enemy j (i*5 + j offset)

        Potion actions:
          - POTION_ACTION_START..: slot-major layout
          - each slot gets 1 untargeted/self action + MAX_ENEMIES targeted actions

        This matches get_action_mask() in gym_env/action_space.py.

        Args:
            state: Full game state dict from the bridge.

        Returns:
            Int8 numpy array of shape (ACTION_SPACE_SIZE,).
        """
        mask = np.zeros(ACTION_SPACE_SIZE, dtype=np.int8)

        # Support both formats (combat_state wrapper or flat)
        combat = state.get("combat_state") or state
        if "player" not in combat:
            mask[ACTION_END_TURN] = 1
            return mask

        # Can always end turn during combat
        mask[ACTION_END_TURN] = 1

        player = combat.get("player", {})
        energy = player.get("energy", 0)
        hand = combat.get("hand", [])
        enemies = combat.get("enemies", [])
        available_actions = {
            str(item).upper()
            for item in (combat.get("available_actions") or state.get("available_actions") or [])
        }

        # Build list of alive enemy indices
        alive_enemies = []
        for j in range(min(len(enemies), MAX_ENEMIES)):
            if enemies[j].get("is_alive", False):
                alive_enemies.append(j)

        # For each card in hand, determine valid actions
        for i in range(min(len(hand), MAX_HAND_SIZE)):
            card = hand[i]
            cost = card.get("cost", 0)

            # Check if card is playable (enough energy, cost >= 0)
            if cost < 0:
                # X-cost cards (cost = -1) are always playable if energy > 0
                if energy <= 0:
                    continue
            elif cost > energy:
                continue

            target_type = card.get("target", "Self")

            if target_type in _UNTARGETED_TYPES:
                # Self-target / no-target / all-enemies: action index = 1 + i
                mask[1 + i] = 1
            elif target_type in _TARGETED_TYPES:
                # Needs specific enemy target: action index = 1 + MAX_HAND_SIZE + i * MAX_ENEMIES + j
                for j in alive_enemies:
                    mask[1 + MAX_HAND_SIZE + i * MAX_ENEMIES + j] = 1

        if not available_actions or "POTION" in available_actions:
            potions = combat.get("potions") or state.get("run_state", {}).get("potions", [])
            for list_index, potion in enumerate(potions[:MAX_POTION_SLOTS]):
                if not potion or not potion.get("can_use", True):
                    continue
                usage = str(potion.get("usage", "")).upper()
                if usage == "AUTOMATIC":
                    continue
                slot = int(potion.get("slot", list_index))
                if slot < 0 or slot >= MAX_POTION_SLOTS:
                    continue
                action_base = POTION_ACTION_START + slot * POTION_TARGET_OPTIONS
                target_type = potion.get("target") or potion.get("target_type", "Self")
                requires_target = potion.get("requires_target", False)
                if requires_target or target_type in _POTION_TARGETED_TYPES:
                    for j in alive_enemies:
                        mask[action_base + 1 + j] = 1
                elif target_type in _POTION_UNTARGETED_TYPES:
                    mask[action_base] = 1

        return mask

    def decode_action(
        self, action: int, state: dict[str, Any]
    ) -> dict[str, Any]:
        """Convert an action index to an action command dict.

        Args:
            action: Action index from model.predict().
            state: Current game state (for reference if needed).

        Returns:
            Action dict ready to send via client.send_action().
        """
        if action == ACTION_END_TURN:
            return {"type": "END_TURN"}

        if action >= POTION_ACTION_START:
            adjusted = action - POTION_ACTION_START
            slot = adjusted // POTION_TARGET_OPTIONS
            target_offset = adjusted % POTION_TARGET_OPTIONS
            target_index = target_offset - 1 if target_offset > 0 else -1
            return {
                "type": "PLAY",
                "out_of_hand": True,
                "potion_slot": slot,
                "target_index": target_index,
            }

        if action <= MAX_HAND_SIZE:
            # Untargeted card play
            card_index = action - 1
            return {"type": "PLAY", "card_index": card_index, "target_index": -1}

        # Targeted card play
        adjusted = action - 1 - MAX_HAND_SIZE
        card_index = adjusted // MAX_ENEMIES
        target_index = adjusted % MAX_ENEMIES
        return {
            "type": "PLAY",
            "card_index": card_index,
            "target_index": target_index,
        }


def _powers_to_dict(powers: list[dict[str, Any]]) -> dict[str, int]:
    """Convert a list of power dicts to a {id: amount} mapping."""
    result: dict[str, int] = {}
    for p in powers:
        pid = p.get("id", "")
        amount = p.get("amount", 0)
        # Normalise to uppercase for matching
        result[pid.upper()] = amount
    return result
