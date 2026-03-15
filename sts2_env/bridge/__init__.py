"""Bridge module for connecting RL agents to the real STS2 game via TCP."""

from sts2_env.bridge.client import STS2GameClient
from sts2_env.bridge.state_adapter import StateAdapter
from sts2_env.bridge.protocol import (
    Phase,
    ActionType,
    MSG_TYPE_GAME_STATE,
    DEFAULT_HOST,
    DEFAULT_PORT,
)

__all__ = [
    "STS2GameClient",
    "StateAdapter",
    "Phase",
    "ActionType",
    "MSG_TYPE_GAME_STATE",
    "DEFAULT_HOST",
    "DEFAULT_PORT",
]
