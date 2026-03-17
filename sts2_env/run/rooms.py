"""Room types for each map point kind.

Each room encapsulates the logic for what happens when the player enters.
"""

from __future__ import annotations

from dataclasses import dataclass, field
from typing import Any

from sts2_env.core.enums import RoomType


@dataclass(frozen=True)
class RoomVisitContext:
    room_type: RoomType

    @property
    def is_combat(self) -> bool:
        return self.room_type in {RoomType.MONSTER, RoomType.ELITE, RoomType.BOSS}

    @property
    def is_boss(self) -> bool:
        return self.room_type == RoomType.BOSS

    @property
    def is_rest_site(self) -> bool:
        return self.room_type == RoomType.REST_SITE

    @property
    def is_merchant(self) -> bool:
        return self.room_type == RoomType.SHOP

    @property
    def is_unknown(self) -> bool:
        return False


@dataclass
class Room:
    """Base room that the player enters on the map."""

    room_type: RoomType
    is_finished: bool = False

    def enter(self, run_state: Any) -> None:
        """Called when the player enters this room."""
        pass

    def get_choices(self, run_state: Any) -> list[str]:
        """Return available choices/actions in this room."""
        return []


@dataclass
class CombatRoom(Room):
    """A room with a combat encounter (Monster, Elite, or Boss)."""

    encounter_id: str = ""
    is_elite: bool = False
    is_boss: bool = False
    gold_proportion: float = 1.0
    extra_rewards: dict[int, list[Any]] = field(default_factory=dict)

    def __post_init__(self) -> None:
        if self.is_boss:
            self.room_type = RoomType.BOSS
        elif self.is_elite:
            self.room_type = RoomType.ELITE
        else:
            self.room_type = RoomType.MONSTER

    def add_extra_reward(self, player_id: int, reward: Any) -> None:
        self.extra_rewards.setdefault(player_id, []).append(reward)


@dataclass
class ShopRoom(Room):
    """Merchant shop room."""

    room_type: RoomType = field(default=RoomType.SHOP)


@dataclass
class RestSiteRoom(Room):
    """Rest site with heal/smith/etc. options."""

    room_type: RoomType = field(default=RoomType.REST_SITE)

    def get_choices(self, run_state: Any) -> list[str]:
        choices = ["heal", "smith"]
        # Relic-based options will be added via hooks
        return choices


@dataclass
class EventRoom(Room):
    """An event room (Unknown "?" rooms that resolve to events, or Ancients)."""

    event_id: str = ""
    room_type: RoomType = field(default=RoomType.EVENT)


@dataclass
class TreasureRoom(Room):
    """Treasure chest room -- player picks a relic."""

    room_type: RoomType = field(default=RoomType.TREASURE)


def create_room(room_type: RoomType, **kwargs: Any) -> Room:
    """Factory for creating room instances."""
    mapping = {
        RoomType.MONSTER: CombatRoom,
        RoomType.ELITE: lambda **kw: CombatRoom(is_elite=True, room_type=RoomType.ELITE, **kw),
        RoomType.BOSS: lambda **kw: CombatRoom(is_boss=True, room_type=RoomType.BOSS, **kw),
        RoomType.SHOP: ShopRoom,
        RoomType.REST_SITE: RestSiteRoom,
        RoomType.TREASURE: TreasureRoom,
        RoomType.EVENT: EventRoom,
    }
    factory = mapping.get(room_type, Room)
    if callable(factory):
        try:
            return factory(**kwargs)
        except TypeError:
            return factory(room_type=room_type, **kwargs)
    return Room(room_type=room_type)
