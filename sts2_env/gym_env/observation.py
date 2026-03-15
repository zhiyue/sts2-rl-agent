"""Observation space encoding.

Compact flat float32 vector (~131 dimensions):
  Player state:       hp/max_hp, block/50, energy, max_energy       (4)
  Player powers:      str, dex, vuln, weak, frail, artifact         (6)
  Hand (10 cards):    card_id_norm, cost, damage, block, is_attack  (50)
  Pile sizes:         draw, discard, exhaust, draw_attacks,
                      draw_skills, discard_attacks                  (6)
  Enemies (5 slots):  alive, hp%, block, intent_onehot(5),
                      intent_dmg, intent_hits, vuln, weak, str      (13 * 5 = 65)
Total: 4 + 6 + 50 + 6 + 65 = 131
"""

from __future__ import annotations

import numpy as np

from sts2_env.core.combat import CombatState
from sts2_env.core.enums import CardId, IntentType, PowerId
from sts2_env.core.constants import MAX_HAND_SIZE, MAX_ENEMIES

# Card IDs list for normalised encoding
CARD_IDS = list(CardId)
NUM_CARD_IDS = len(CARD_IDS)
_CARD_ID_TO_IDX = {cid: i for i, cid in enumerate(CARD_IDS)}

# Player powers to track (6)
PLAYER_POWERS = [
    PowerId.STRENGTH, PowerId.DEXTERITY, PowerId.VULNERABLE,
    PowerId.WEAK, PowerId.FRAIL, PowerId.ARTIFACT,
]
NUM_PLAYER_POWERS = len(PLAYER_POWERS)

# Intent types for one-hot (5)
INTENT_TYPES = [
    IntentType.ATTACK, IntentType.MULTI_ATTACK, IntentType.DEFEND,
    IntentType.BUFF, IntentType.DEBUFF,
]
NUM_INTENT_TYPES = len(INTENT_TYPES)

# Per-card features in hand
CARD_FEATURES = 5  # card_id_norm, cost, damage, block, is_attack

# Per-enemy features
# alive(1) + hp%(1) + block(1) + intent_onehot(5) + intent_dmg(1) + intent_hits(1) + vuln(1) + weak(1) + str(1)
ENEMY_FEATURES = 1 + 1 + 1 + NUM_INTENT_TYPES + 1 + 1 + 1 + 1 + 1  # = 13

# Pile summary features
PILE_FEATURES = 6  # draw_size, discard_size, exhaust_size, draw_attacks, draw_skills, discard_attacks

# Observation size
OBS_SIZE = (
    4                                  # player state
    + NUM_PLAYER_POWERS                # player powers (6)
    + MAX_HAND_SIZE * CARD_FEATURES    # hand cards (50)
    + PILE_FEATURES                    # pile summaries (6)
    + MAX_ENEMIES * ENEMY_FEATURES     # enemies (65)
)  # = 131


def encode_observation(combat: CombatState) -> np.ndarray:
    """Encode combat state as a compact flat float32 vector."""
    obs = np.zeros(OBS_SIZE, dtype=np.float32)
    idx = 0

    # --- Player state (4) ---
    obs[idx] = combat.player.current_hp / combat.player.max_hp if combat.player.max_hp > 0 else 0.0
    obs[idx + 1] = combat.player.block / 50.0
    obs[idx + 2] = combat.energy / 10.0
    obs[idx + 3] = combat.max_energy / 10.0
    idx += 4

    # --- Player powers (6) ---
    for pid in PLAYER_POWERS:
        obs[idx] = combat.player.get_power_amount(pid) / 20.0
        idx += 1

    # --- Hand cards (10 * 5 = 50) ---
    for i in range(MAX_HAND_SIZE):
        if i < len(combat.hand):
            card = combat.hand[i]
            obs[idx] = (_CARD_ID_TO_IDX.get(card.card_id, 0) + 1) / (NUM_CARD_IDS + 1)
            obs[idx + 1] = max(0, card.cost) / 5.0
            obs[idx + 2] = (card.base_damage or 0) / 50.0
            obs[idx + 3] = (card.base_block or 0) / 50.0
            obs[idx + 4] = 1.0 if card.is_attack else 0.0
        idx += CARD_FEATURES

    # --- Pile summaries (6) ---
    draw_attacks = sum(1 for c in combat.draw_pile if c.is_attack)
    draw_skills = sum(1 for c in combat.draw_pile if c.is_skill)
    discard_attacks = sum(1 for c in combat.discard_pile if c.is_attack)

    obs[idx] = len(combat.draw_pile) / 20.0
    obs[idx + 1] = len(combat.discard_pile) / 20.0
    obs[idx + 2] = len(combat.exhaust_pile) / 20.0
    obs[idx + 3] = draw_attacks / 10.0
    obs[idx + 4] = draw_skills / 10.0
    obs[idx + 5] = discard_attacks / 10.0
    idx += PILE_FEATURES

    # --- Enemies (5 * 13 = 65) ---
    for i in range(MAX_ENEMIES):
        if i < len(combat.enemies):
            enemy = combat.enemies[i]
            obs[idx] = 1.0 if enemy.is_alive else 0.0
            obs[idx + 1] = enemy.current_hp / enemy.max_hp if enemy.max_hp > 0 else 0.0
            obs[idx + 2] = enemy.block / 50.0

            # Intent encoding (one-hot + damage + hits)
            ai = combat.enemy_ais.get(enemy.combat_id)
            if ai is not None and enemy.is_alive:
                move = ai.current_move
                if move.intents:
                    intent = move.intents[0]
                    for j, it in enumerate(INTENT_TYPES):
                        if intent.intent_type == it:
                            obs[idx + 3 + j] = 1.0
                    obs[idx + 3 + NUM_INTENT_TYPES] = intent.damage / 30.0
                    obs[idx + 3 + NUM_INTENT_TYPES + 1] = intent.hits / 5.0

            # Enemy powers
            obs[idx + 3 + NUM_INTENT_TYPES + 2] = enemy.get_power_amount(PowerId.VULNERABLE) / 10.0
            obs[idx + 3 + NUM_INTENT_TYPES + 3] = enemy.get_power_amount(PowerId.WEAK) / 10.0
            obs[idx + 3 + NUM_INTENT_TYPES + 4] = enemy.get_power_amount(PowerId.STRENGTH) / 10.0
        idx += ENEMY_FEATURES

    return obs
