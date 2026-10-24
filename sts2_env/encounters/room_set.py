"""Shared metadata for act encounter pools.

The selection machinery (GrabBag, anti-repeat tags, pre-generated per-act
queues) lives in ``sts2_env.run.run_state`` (``_EncounterGrabBag`` /
``ActRoomSet`` / ``RunState._generate_encounter_rooms``), porting C#
``ActModel.GenerateRooms`` + ``RoomSet`` (ActModel.cs L349-416,
MegaCrit.Sts2.Core.Rooms/RoomSet.cs). This module only defines the per-act
``EncounterSpec`` table type: each act module lists its encounters in the
exact C# ``GenerateAllEncounters`` array order with their ``EncounterModel``
Tags / IsWeak / RoomType, which drives GrabBag scan order for RNG parity.
"""

from __future__ import annotations

from dataclasses import dataclass
from typing import Callable

# C# EncounterModel.RoomType values relevant to encounter pools.
ROOM_TYPE_MONSTER = "monster"
ROOM_TYPE_ELITE = "elite"
ROOM_TYPE_BOSS = "boss"


@dataclass(frozen=True)
class EncounterSpec:
    """One entry of a C# act's GenerateAllEncounters array.

    ``setup`` is the Python combat setup callable; ``tags`` mirror the C#
    ``EncounterModel.Tags`` override (used by anti-repeat); ``is_weak`` mirrors
    ``EncounterModel.IsWeak``; ``room_type`` mirrors ``EncounterModel.RoomType``.
    """

    setup: Callable
    tags: frozenset[str] = frozenset()
    is_weak: bool = False
    room_type: str = ROOM_TYPE_MONSTER
