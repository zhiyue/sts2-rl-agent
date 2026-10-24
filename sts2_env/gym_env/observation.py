"""Observation space encoding.

Compact flat float32 vector (~217 dimensions):
  Player state:       hp/max_hp, block/50, energy, max_energy, stars/5 (5)
  Player powers:      str, dex, vuln, weak, frail, artifact         (6)
  Hand (10 cards):    card_id_norm, cost, damage, block, is_attack  (50)
  Pile sizes:         draw, discard, exhaust, reserved, reserved,
                      reserved                                     (6)
  Enemies (5 slots):  alive, hp%, block,
                      intent slot 0: onehot(6) + dmg + hits         (8)
                      vuln, weak, str                              (3)
                      intent slots 1-2: onehot(6) + dmg + hits each (2 * 8)
                                                                (30 * 5 = 150)
Total: 5 + 6 + 50 + 6 + 150 = 217

Multi-intent encoding
---------------------
A single C# ``MoveState`` can carry several intents (e.g. attack + buff,
attack + heal + buff).  The decompiled game defines at most 3 intents per
move (KnowledgeDemon.cs:133, InfestedPrism.cs:68, TheForgotten.cs:45,
WaterfallGiant.cs:195), so each enemy block exposes
``MAX_INTENTS_PER_MOVE = 3`` intent slots of 8 features each
(one-hot(6) + damage/30 + hits/5).  Unused slots stay zero.  Slot 0 sits
right after ``block`` (matching the pre-multi-intent layout, positions
3-9 of the enemy block); the enemy power features follow at positions
10-12; overflow slots 1..N-1 come last so that the bridge-side encoder
(``bridge/state_adapter.py``), which only receives the primary intent
from the C# serializer, keeps writing every feature at the same offset.
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

# Intent types for one-hot (6).  Tactically relevant subset of C# IntentType:
# attack/multi-attack/defend/buff/debuff cover most moves; Heal is encoded so
# "enemy will heal" (KnowledgeDemon PONDER_MOVE) is distinguishable from "no
# intent data" — untracked types (Sleep/Summon/Stun/...) stay all-zero.
INTENT_TYPES = [
    IntentType.ATTACK, IntentType.MULTI_ATTACK, IntentType.DEFEND,
    IntentType.BUFF, IntentType.DEBUFF, IntentType.HEAL,
]
NUM_INTENT_TYPES = len(INTENT_TYPES)

# Per-card features in hand
CARD_FEATURES = 5  # card_id_norm, cost, damage, block, is_attack

# Intents per enemy move slot.  The decompiled C# never attaches more than
# 3 intents to a single MoveState (params AbstractIntent[] intents):
#   - 3 intents: KnowledgeDemon "PONDER_MOVE" (SingleAttackIntent + HealIntent
#     + BuffIntent, KnowledgeDemon.cs:133), InfestedPrism (InfestedPrism.cs:68),
#     TheForgotten (TheForgotten.cs:45), WaterfallGiant (WaterfallGiant.cs:195)
#   - 2 intents: 76 moves (e.g. Aeonglass "EBB_MOVE", VineShambler)
#   - 1 intent: all remaining moves
MAX_INTENTS_PER_MOVE = 3

# Per-intent-slot features: onehot(NUM_INTENT_TYPES) + damage + hits
INTENT_SLOT_FEATURES = NUM_INTENT_TYPES + 2  # = 8

# Per-enemy features
# alive(1) + hp%(1) + block(1)
# + intent slot 0 (onehot(6) + dmg + hits = 8)
# + vuln(1) + weak(1) + str(1)
# + intent slots 1..MAX_INTENTS_PER_MOVE-1 (onehot(6) + dmg + hits = 8 each)
ENEMY_FEATURES = (
    3 + INTENT_SLOT_FEATURES + 3 + (MAX_INTENTS_PER_MOVE - 1) * INTENT_SLOT_FEATURES
)  # = 30

# Offsets inside one enemy block.  Slot 0 is the "primary" intent at the
# legacy position; enemy powers follow; overflow slots come last.
_ENEMY_INTENT_SLOT0_OFFSET = 3
_ENEMY_POWERS_OFFSET = _ENEMY_INTENT_SLOT0_OFFSET + INTENT_SLOT_FEATURES  # = 10
_ENEMY_INTENT_SLOT_OFFSETS = [_ENEMY_INTENT_SLOT0_OFFSET] + [
    _ENEMY_POWERS_OFFSET + 3 + k * INTENT_SLOT_FEATURES
    for k in range(MAX_INTENTS_PER_MOVE - 1)
]  # = [3, 13, 20]

# Pile summary features
PILE_FEATURES = 6  # draw_size, discard_size, exhaust_size, reserved x3

# Observation size
OBS_SIZE = (
    5                                  # player state (incl. Stars, see
                                       # PlayerCombatState.cs:190-208 — Stars
                                       # gate card playability like energy)
    + NUM_PLAYER_POWERS                # player powers (6)
    + MAX_HAND_SIZE * CARD_FEATURES    # hand cards (50)
    + PILE_FEATURES                    # pile summaries (6)
    + MAX_ENEMIES * ENEMY_FEATURES     # enemies (5 * 30 = 150)
)  # = 217


def encode_observation(combat: CombatState) -> np.ndarray:
    """Encode combat state as a compact flat float32 vector."""
    obs = np.zeros(OBS_SIZE, dtype=np.float32)
    idx = 0

    # --- Player state (5: hp, block, energy, max_energy, stars) ---
    obs[idx] = combat.player.current_hp / combat.player.max_hp if combat.player.max_hp > 0 else 0.0
    obs[idx + 1] = combat.player.block / 50.0
    obs[idx + 2] = combat.energy / 10.0
    obs[idx + 3] = combat.max_energy / 10.0
    obs[idx + 4] = combat.stars / 5.0
    idx += 5

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
    obs[idx] = len(combat.draw_pile) / 20.0
    obs[idx + 1] = len(combat.discard_pile) / 20.0
    obs[idx + 2] = len(combat.exhaust_pile) / 20.0
    # Keep the last three pile-summary dimensions zeroed so simulator and
    # bridge observations stay aligned even though the bridge only exposes
    # aggregate pile counts.
    obs[idx + 3] = 0.0
    obs[idx + 4] = 0.0
    obs[idx + 5] = 0.0
    idx += PILE_FEATURES

    # --- Enemies (5 * 27 = 135) ---
    for i in range(MAX_ENEMIES):
        if i < len(combat.enemies):
            enemy = combat.enemies[i]
            obs[idx] = 1.0 if enemy.is_alive else 0.0
            obs[idx + 1] = enemy.current_hp / enemy.max_hp if enemy.max_hp > 0 else 0.0
            obs[idx + 2] = enemy.block / 50.0

            # Intent encoding: one slot per intent, each slot =
            # onehot(NUM_INTENT_TYPES) + damage + hits.  A single move can
            # carry several intents (e.g. attack + buff); unused slots stay
            # zero.  Extra intents beyond MAX_INTENTS_PER_MOVE are dropped.
            ai = combat.enemy_ais.get(enemy.combat_id)
            if ai is not None and enemy.is_alive:
                move = ai.current_move
                for k, intent in enumerate(move.intents[:MAX_INTENTS_PER_MOVE]):
                    base = idx + _ENEMY_INTENT_SLOT_OFFSETS[k]
                    for j, it in enumerate(INTENT_TYPES):
                        if intent.intent_type == it:
                            obs[base + j] = 1.0
                    obs[base + NUM_INTENT_TYPES] = intent.damage / 30.0
                    obs[base + NUM_INTENT_TYPES + 1] = intent.hits / 5.0

            # Enemy powers
            obs[idx + _ENEMY_POWERS_OFFSET] = enemy.get_power_amount(PowerId.VULNERABLE) / 10.0
            obs[idx + _ENEMY_POWERS_OFFSET + 1] = enemy.get_power_amount(PowerId.WEAK) / 10.0
            obs[idx + _ENEMY_POWERS_OFFSET + 2] = enemy.get_power_amount(PowerId.STRENGTH) / 10.0
        idx += ENEMY_FEATURES

    return obs
