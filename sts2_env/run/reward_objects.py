"""Reward objects and reward-set assembly."""

from __future__ import annotations

from dataclasses import dataclass, field
from enum import Enum, auto
from typing import TYPE_CHECKING

from sts2_env.cards.base import CardInstance
from sts2_env.core.enums import RoomType
from sts2_env.potions.base import create_potion, roll_random_potion_model
from sts2_env.relics.base import RelicPool, RelicRarity
from sts2_env.relics.registry import RELIC_REGISTRY, load_all_relics
from sts2_env.run.rewards import generate_combat_reward_cards

if TYPE_CHECKING:
    from sts2_env.run.rooms import CombatRoom, Room
    from sts2_env.run.run_manager import RunManager
    from sts2_env.run.run_state import RunState


class RewardType(Enum):
    NONE = auto()
    GOLD = auto()
    POTION = auto()
    RELIC = auto()
    CARD = auto()
    REMOVE_CARD = auto()


@dataclass
class Reward:
    player_id: int
    reward_type: RewardType
    rewards_set_index: int
    is_populated: bool = False
    skippable: bool = True

    def populate(self, run_state: RunState, room: Room | None) -> None:
        self.is_populated = True

    def select(self, run_manager: RunManager, **_: object) -> dict:
        return {"description": f"Collected {self.reward_type.name.lower()} reward."}

    def skip(self, run_manager: RunManager) -> dict:
        return {"description": f"Skipped {self.reward_type.name.lower()} reward."}


@dataclass
class GoldReward(Reward):
    min_gold: int = 0
    max_gold: int = 0
    amount: int = 0
    skippable: bool = False

    def __init__(self, player_id: int, min_gold: int, max_gold: int):
        super().__init__(player_id=player_id, reward_type=RewardType.GOLD, rewards_set_index=1, skippable=False)
        self.min_gold = min_gold
        self.max_gold = max_gold
        self.amount = 0

    def populate(self, run_state: RunState, room: Room | None) -> None:
        self.amount = run_state.rng.rewards.next_int(self.min_gold, self.max_gold)
        self.is_populated = True

    def select(self, run_manager: RunManager, **_: object) -> dict:
        player = run_manager.run_state.get_player(self.player_id)
        player.gain_gold(self.amount)
        return {"description": f"Gained {self.amount} gold.", "gold_earned": self.amount}


@dataclass
class PotionReward(Reward):
    potion_id: str | None = None

    def __init__(self, player_id: int, potion_id: str | None = None):
        super().__init__(player_id=player_id, reward_type=RewardType.POTION, rewards_set_index=2)
        self.potion_id = potion_id

    def populate(self, run_state: RunState, room: Room | None) -> None:
        if self.potion_id is None:
            player = run_state.get_player(self.player_id)
            potion = roll_random_potion_model(
                run_state.rng.rewards,
                character_id=player.character_id,
                in_combat=False,
            )
            self.potion_id = potion.potion_id if potion is not None else None
        self.is_populated = True

    def select(self, run_manager: RunManager, **_: object) -> dict:
        player = run_manager.run_state.get_player(self.player_id)
        if self.potion_id is None:
            return {"description": "No potion reward available.", "success": False}
        potion = create_potion(self.potion_id)
        if player.add_potion(potion):
            return {
                "description": f"Obtained potion {self.potion_id}.",
                "potion_id": self.potion_id,
                "success": True,
            }
        return {
            "description": f"No empty potion slot for {self.potion_id}.",
            "potion_id": self.potion_id,
            "success": False,
        }


@dataclass
class RelicReward(Reward):
    relic_id: str | None = None
    rarity: RelicRarity | None = None

    def __init__(self, player_id: int, relic_id: str | None = None, rarity: RelicRarity | None = None):
        super().__init__(player_id=player_id, reward_type=RewardType.RELIC, rewards_set_index=3)
        self.relic_id = relic_id
        self.rarity = rarity

    def populate(self, run_state: RunState, room: Room | None) -> None:
        if self.relic_id is None:
            player = run_state.get_player(self.player_id)
            load_all_relics()
            desired_pool = getattr(RelicPool, player.character_id.upper(), None)
            owned = set(player.relics)
            candidates: list[str] = []
            for relic_id, relic_cls in RELIC_REGISTRY.items():
                if relic_id.name in owned:
                    continue
                if relic_cls.pool in {RelicPool.EVENT, RelicPool.FALLBACK, RelicPool.DEPRECATED}:
                    continue
                if desired_pool is not None and relic_cls.pool not in {RelicPool.SHARED, desired_pool}:
                    continue
                if self.rarity is not None and relic_cls.rarity is not self.rarity:
                    continue
                candidates.append(relic_id.name)
            if candidates:
                self.relic_id = run_state.rng.rewards.choice(candidates)
        self.is_populated = True

    def select(self, run_manager: RunManager, **_: object) -> dict:
        player = run_manager.run_state.get_player(self.player_id)
        if self.relic_id is None:
            return {"description": "No relic reward available."}
        player.obtain_relic(self.relic_id)
        return {"description": f"Obtained relic {self.relic_id}.", "relic_id": self.relic_id}


@dataclass
class CardReward(Reward):
    context: str = "regular"
    option_count: int = 3
    cards: list[CardInstance] = field(default_factory=list)

    def __init__(
        self,
        player_id: int,
        context: str = "regular",
        option_count: int = 3,
        cards: list[CardInstance] | None = None,
    ):
        super().__init__(player_id=player_id, reward_type=RewardType.CARD, rewards_set_index=5)
        self.context = context
        self.option_count = option_count
        self.cards = list(cards or [])

    def populate(self, run_state: RunState, room: Room | None) -> None:
        if not self.cards:
            self.cards = generate_combat_reward_cards(
                run_state,
                context=self.context,
                num_cards=self.option_count,
            )
        self.is_populated = True

    def select(self, run_manager: RunManager, **kwargs: object) -> dict:
        index = int(kwargs.get("index", 0))
        if 0 <= index < len(self.cards):
            card = self.cards[index]
            run_manager.run_state.get_player(self.player_id).add_card_instance_to_deck(card)
            return {
                "description": f"Picked {card.card_id.name}.",
                "card_id": card.card_id.name,
                "rarity": card.rarity.name,
                "upgraded": card.upgraded,
            }
        return {"description": "Invalid card index."}


_GOLD_REWARD_RANGES: dict[RoomType, tuple[int, int]] = {
    RoomType.MONSTER: (10, 20),
    RoomType.ELITE: (25, 35),
    RoomType.BOSS: (95, 105),
}


@dataclass
class RewardsSet:
    player_id: int
    room: Room | None = None
    rewards: list[Reward] = field(default_factory=list)
    allow_empty_rewards: bool = False

    def empty_for_room(self, room: Room) -> RewardsSet:
        self.room = room
        self.allow_empty_rewards = True
        return self

    def with_rewards_from_room(self, room: Room, run_state: RunState) -> RewardsSet:
        self.room = room
        if room.room_type == RoomType.BOSS and run_state.current_act_index >= len(run_state.acts) - 1:
            if hasattr(room, "extra_rewards"):
                self.rewards.extend(room.extra_rewards.get(self.player_id, []))
            return self
        if room.room_type in _GOLD_REWARD_RANGES:
            low, high = _GOLD_REWARD_RANGES[room.room_type]
            self.rewards.append(GoldReward(self.player_id, low, high))
            if run_state.potion_reward_odds.roll(run_state.rng.rewards, is_elite=room.room_type == RoomType.ELITE):
                self.rewards.append(PotionReward(self.player_id))
            self.rewards.append(CardReward(self.player_id, context=self._card_context(room.room_type)))
            if room.room_type == RoomType.ELITE:
                self.rewards.append(RelicReward(self.player_id))

        if hasattr(room, "extra_rewards"):
            self.rewards.extend(room.extra_rewards.get(self.player_id, []))
        return self

    def with_custom_rewards(self, rewards: list[Reward]) -> RewardsSet:
        self.rewards.extend(rewards)
        return self

    def generate_without_offering(self, run_state: RunState) -> list[Reward]:
        for reward in self.rewards:
            if not reward.is_populated:
                reward.populate(run_state, self.room)
        self.rewards.sort(key=lambda reward: reward.rewards_set_index)
        return self.rewards

    @staticmethod
    def _card_context(room_type: RoomType) -> str:
        if room_type == RoomType.BOSS:
            return "boss"
        if room_type == RoomType.ELITE:
            return "elite"
        return "regular"
