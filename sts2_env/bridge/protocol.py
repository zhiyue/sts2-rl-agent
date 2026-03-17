"""Shared constants for the bridge protocol.

These constants define the message types, game phases, and action types
used in the newline-delimited JSON protocol between the C# bridge mod
and the Python client.

The protocol is simple:
  - Each message is a single JSON object terminated by a newline (\\n)
  - Game -> Agent: state messages with type="game_state"
  - Agent -> Game: action messages with type="PLAY"/"END_TURN"/etc.
"""

from __future__ import annotations

# ----------------------------------------------------------------
# Network defaults
# ----------------------------------------------------------------

DEFAULT_HOST = "127.0.0.1"
DEFAULT_PORT = 9002

# ----------------------------------------------------------------
# Message types (top-level "type" field)
# ----------------------------------------------------------------

MSG_TYPE_GAME_STATE = "game_state"
MSG_TYPE_ERROR = "error"
MSG_TYPE_PONG = "pong"

# ----------------------------------------------------------------
# Game phases (state["phase"] field)
# ----------------------------------------------------------------


class Phase:
    """Game phase constants used by the bridge protocol."""

    COMBAT_PLAY = "COMBAT_PLAY"
    COMBAT_WAITING = "COMBAT_WAITING"
    MAP_SELECT = "MAP_SELECT"
    CARD_REWARD = "CARD_REWARD"
    SHOP = "SHOP"
    REST = "REST"
    EVENT = "EVENT"
    UNKNOWN = "UNKNOWN"

    # All phases where the agent should act
    ACTIONABLE = frozenset({
        COMBAT_PLAY,
        MAP_SELECT,
        CARD_REWARD,
        SHOP,
        REST,
        EVENT,
    })

    # Phases where we use the trained combat model
    COMBAT_PHASES = frozenset({COMBAT_PLAY})

    # Phases where we use heuristic decision-making
    NON_COMBAT_PHASES = frozenset({MAP_SELECT, CARD_REWARD, SHOP, REST, EVENT})


# ----------------------------------------------------------------
# Action types (action["type"] field sent to game)
# ----------------------------------------------------------------


class ActionType:
    """Action type constants matching BridgeServer.cs ProcessActionMessage()."""

    PLAY = "PLAY"
    END_TURN = "END_TURN"
    CHOOSE = "CHOOSE"
    POTION = "POTION"
    PING = "PING"


# ----------------------------------------------------------------
# Intent type strings (from C# enum → string conversion)
# ----------------------------------------------------------------


class IntentName:
    """Intent type string constants as serialized by the bridge."""

    ATTACK = "ATTACK"
    MULTI_ATTACK = "MULTI_ATTACK"
    DEFEND = "DEFEND"
    BUFF = "BUFF"
    DEBUFF = "DEBUFF"
    DEBUFF_STRONG = "DEBUFF_STRONG"
    SLEEP = "SLEEP"
    SUMMON = "SUMMON"
    ESCAPE = "ESCAPE"
    UNKNOWN = "UNKNOWN"
    STATUS_CARD = "STATUS_CARD"
    STUN = "STUN"
    HEAL = "HEAL"
    DEATH_BLOW = "DEATH_BLOW"
    CARD_DEBUFF = "CARD_DEBUFF"


# ----------------------------------------------------------------
# Card type / target strings (from C# enum → string conversion)
# ----------------------------------------------------------------


class CardTypeName:
    ATTACK = "Attack"
    SKILL = "Skill"
    POWER = "Power"
    STATUS = "Status"
    CURSE = "Curse"


class TargetTypeName:
    SELF = "Self"
    NONE = "None"
    ANY_ENEMY = "AnyEnemy"
    ALL_ENEMIES = "AllEnemies"
    RANDOM_ENEMY = "RandomEnemy"
    ANY_ALLY = "AnyAlly"
    ALL_ALLIES = "AllAllies"


# ----------------------------------------------------------------
# Power ID strings (from C# enum → string conversion)
# These must match the PowerId enum names in the game.
# ----------------------------------------------------------------

POWER_STRENGTH = "STRENGTH"
POWER_DEXTERITY = "DEXTERITY"
POWER_VULNERABLE = "VULNERABLE"
POWER_WEAK = "WEAK"
POWER_FRAIL = "FRAIL"
POWER_ARTIFACT = "ARTIFACT"
