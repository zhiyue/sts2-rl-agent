"""Reward objects and reward-set assembly."""

from __future__ import annotations

from dataclasses import dataclass, field
from enum import Enum, auto
from typing import TYPE_CHECKING

from sts2_env.cards.base import CardInstance
from sts2_env.core.enums import CardId, CardRarity, RoomType
from sts2_env.potions.base import create_potion, roll_random_potion_model
from sts2_env.relics.base import RelicPool, RelicRarity
from sts2_env.relics.registry import RELIC_REGISTRY, load_all_relics
from sts2_env.run.rewards import CardRewardGenerationOptions, generate_combat_reward_cards

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
    ADD_CARD = auto()
    OBTAIN_RELIC = auto()
    LOSE_HP = auto()
    LOSE_GOLD = auto()
    REMOVE_CARD = auto()
    UPGRADE_CARD = auto()
    TRANSFORM_CARD = auto()
    DUPLICATE_CARD = auto()
    ENCHANT_CARD = auto()


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
    is_wax: bool = False
    rng_stream: str = "rewards"
    setup_attrs: dict[str, object] = field(default_factory=dict)

    def __init__(
        self,
        player_id: int,
        relic_id: str | None = None,
        rarity: RelicRarity | None = None,
        *,
        is_wax: bool = False,
        rng_stream: str = "rewards",
        setup_attrs: dict[str, object] | None = None,
    ):
        super().__init__(player_id=player_id, reward_type=RewardType.RELIC, rewards_set_index=3)
        self.relic_id = relic_id
        self.rarity = rarity
        self.is_wax = is_wax
        self.rng_stream = rng_stream
        self.setup_attrs = dict(setup_attrs or {})

    def populate(self, run_state: RunState, room: Room | None) -> None:
        if self.relic_id is None:
            player = run_state.get_player(self.player_id)
            load_all_relics()
            self.relic_id = player.pull_next_relic_reward_id(
                rarity=self.rarity,
                rng_stream=self.rng_stream,
            )
        self.is_populated = True

    def select(self, run_manager: RunManager, **_: object) -> dict:
        player = run_manager.run_state.get_player(self.player_id)
        if self.relic_id is None:
            return {"description": "No relic reward available."}
        previous = run_manager.run_state.defer_followup_rewards
        run_manager.run_state.defer_followup_rewards = True
        try:
            player.obtain_relic_with_setup(
                self.relic_id,
                setup_attrs=self.setup_attrs,
                is_wax=self.is_wax,
            )
        finally:
            run_manager.run_state.defer_followup_rewards = previous
        return {"description": f"Obtained relic {self.relic_id}.", "relic_id": self.relic_id}


@dataclass
class CardReward(Reward):
    context: str = "regular"
    option_count: int = 3
    character_ids: tuple[str, ...] = field(default_factory=tuple)
    forced_rarities: tuple[CardRarity, ...] = field(default_factory=tuple)
    include_colorless: bool = False
    use_default_character_pool: bool = True
    cards: list[CardInstance] = field(default_factory=list)
    max_rerolls: int = 0
    rerolls_used: int = 0
    _can_regenerate: bool = True

    def __init__(
        self,
        player_id: int,
        context: str = "regular",
        option_count: int | None = None,
        character_ids: tuple[str, ...] | None = None,
        forced_rarities: tuple[CardRarity, ...] | None = None,
        include_colorless: bool = False,
        use_default_character_pool: bool = True,
        cards: list[CardInstance] | None = None,
    ):
        super().__init__(player_id=player_id, reward_type=RewardType.CARD, rewards_set_index=5)
        resolved_cards = list(cards or [])
        resolved_rarities = tuple(forced_rarities or ())
        self.context = context
        self.option_count = option_count or len(resolved_rarities) or len(resolved_cards) or 3
        self.character_ids = tuple(character_ids or ())
        self.forced_rarities = resolved_rarities
        self.include_colorless = include_colorless
        self.use_default_character_pool = use_default_character_pool
        self.cards = resolved_cards
        self.max_rerolls = 0
        self.rerolls_used = 0
        self._can_regenerate = cards is None

    def populate(self, run_state: RunState, room: Room | None) -> None:
        player = run_state.get_player(self.player_id)
        options = CardRewardGenerationOptions(
            context=self.context,
            num_cards=self.option_count,
            character_ids=self.character_ids,
            forced_rarities=self.forced_rarities,
            include_colorless=self.include_colorless,
            use_default_character_pool=self.use_default_character_pool,
        )
        for relic in player.get_relic_objects():
            options = relic.modify_card_reward_creation_options(
                player,
                options,
                self,
                room,
                run_state,
            )
        if not self.cards:
            self.cards = generate_combat_reward_cards(
                run_state,
                context=options.context,
                num_cards=options.num_cards,
                character_ids=None if options.use_default_character_pool and not options.character_ids else options.character_ids,
                forced_rarities=options.forced_rarities,
                include_colorless=options.include_colorless,
            )
        self.include_colorless = options.include_colorless
        self.use_default_character_pool = options.use_default_character_pool
        for relic in player.get_relic_objects():
            self.cards = relic.modify_card_reward_options_late(
                player,
                self.cards,
                self,
                room,
                run_state,
            )
        if self.max_rerolls == 0 and self._can_regenerate:
            for relic in player.get_relic_objects():
                if relic.allow_card_reward_reroll(player, self, room, run_state):
                    self.max_rerolls = max(self.max_rerolls, 1)
        self.option_count = len(self.cards)
        self.is_populated = True

    @property
    def rerolls_remaining(self) -> int:
        return max(0, self.max_rerolls - self.rerolls_used)

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

    def reroll(self, run_manager: RunManager) -> dict:
        if self.rerolls_remaining <= 0 or not self._can_regenerate:
            return {"description": "Cannot reroll card reward.", "success": False}
        self.rerolls_used += 1
        self.cards = []
        self.is_populated = False
        self.populate(run_manager.run_state, run_manager.current_room)
        return {
            "description": "Rerolled card reward.",
            "success": True,
            "rerolls_remaining": self.rerolls_remaining,
        }


@dataclass
class AddCardsReward(Reward):
    cards: list[CardInstance] = field(default_factory=list)

    def __init__(self, player_id: int, cards: list[CardInstance]):
        super().__init__(player_id=player_id, reward_type=RewardType.ADD_CARD, rewards_set_index=4, skippable=False)
        self.cards = list(cards)

    def select(self, run_manager: RunManager, **_: object) -> dict:
        player = run_manager.run_state.get_player(self.player_id)
        for card in self.cards:
            player.add_card_instance_to_deck(card)
        return {
            "description": f"Added {len(self.cards)} card(s) to deck.",
            "added": len(self.cards),
        }


@dataclass
class ObtainRelicsReward(Reward):
    count: int = 1
    rarities: tuple[RelicRarity | None, ...] = field(default_factory=tuple)
    relic_ids: tuple[str, ...] = field(default_factory=tuple)
    rng_stream: str = "rewards"

    def __init__(
        self,
        player_id: int,
        count: int = 1,
        *,
        rarities: tuple[RelicRarity | None, ...] | None = None,
        relic_ids: tuple[str, ...] | list[str] | None = None,
        rng_stream: str = "rewards",
    ):
        super().__init__(player_id=player_id, reward_type=RewardType.OBTAIN_RELIC, rewards_set_index=4, skippable=False)
        self.relic_ids = tuple(relic_ids or ())
        self.count = len(self.relic_ids) if self.relic_ids else count
        self.rarities = tuple(rarities or ())
        self.rng_stream = rng_stream

    def select(self, run_manager: RunManager, **_: object) -> dict:
        obtained: list[str] = []
        for index in range(self.count):
            if index < len(self.relic_ids):
                reward = RelicReward(self.player_id, relic_id=self.relic_ids[index], rng_stream=self.rng_stream)
            else:
                rarity = self.rarities[index] if index < len(self.rarities) else None
                reward = RelicReward(self.player_id, rarity=rarity, rng_stream=self.rng_stream)
                reward.populate(run_manager.run_state, run_manager.current_room)
            if reward.relic_id is None:
                continue
            reward.select(run_manager)
            obtained.append(reward.relic_id)
        return {
            "description": f"Obtained {len(obtained)} relic(s).",
            "obtained": obtained,
        }


@dataclass
class LoseHpReward(Reward):
    amount: int = 0

    def __init__(self, player_id: int, amount: int):
        super().__init__(player_id=player_id, reward_type=RewardType.LOSE_HP, rewards_set_index=4, skippable=False)
        self.amount = amount

    def select(self, run_manager: RunManager, **_: object) -> dict:
        player = run_manager.run_state.get_player(self.player_id)
        lost = player.lose_hp(self.amount)
        if player.current_hp <= 0:
            run_manager.run_state.lose_run()
        return {
            "description": f"Lost {lost} HP.",
            "hp_lost": lost,
        }


@dataclass
class LoseGoldReward(Reward):
    amount: int = 0

    def __init__(self, player_id: int, amount: int):
        super().__init__(player_id=player_id, reward_type=RewardType.LOSE_GOLD, rewards_set_index=4, skippable=False)
        self.amount = amount

    def select(self, run_manager: RunManager, **_: object) -> dict:
        player = run_manager.run_state.get_player(self.player_id)
        lost = player.lose_gold(self.amount)
        return {
            "description": f"Lost {lost} gold.",
            "gold_lost": lost,
        }


@dataclass
class RemoveCardReward(Reward):
    count: int = 1
    cards: list[CardInstance] | None = None

    def __init__(self, player_id: int, count: int = 1, cards: list[CardInstance] | None = None):
        super().__init__(player_id=player_id, reward_type=RewardType.REMOVE_CARD, rewards_set_index=4, skippable=False)
        self.count = count
        self.cards = cards

    def select(self, run_manager: RunManager, **_: object) -> dict:
        player = run_manager.run_state.get_player(self.player_id)
        removed = player.remove_cards_from_deck(self.count, cards=self.cards)
        if run_manager.run_state.pending_choice is not None:
            return {
                "description": f"Choose {self.count} card(s) to remove.",
                "pending_choice": True,
            }
        return {
            "description": f"Removed {removed} card(s).",
            "removed": removed,
        }


@dataclass
class UpgradeCardsReward(Reward):
    count: int = 1
    cards: list[CardInstance] | None = None

    def __init__(self, player_id: int, count: int = 1, cards: list[CardInstance] | None = None):
        super().__init__(player_id=player_id, reward_type=RewardType.UPGRADE_CARD, rewards_set_index=4, skippable=False)
        self.count = count
        self.cards = cards

    def select(self, run_manager: RunManager, **_: object) -> dict:
        player = run_manager.run_state.get_player(self.player_id)
        upgraded = player.upgrade_selected_cards(self.count, cards=self.cards)
        if run_manager.run_state.pending_choice is not None:
            return {
                "description": f"Choose {self.count} card(s) to upgrade.",
                "pending_choice": True,
            }
        return {
            "description": f"Upgraded {upgraded} card(s).",
            "upgraded": upgraded,
        }


@dataclass
class TransformCardsReward(Reward):
    count: int = 1
    upgrade: bool = False
    cards: list[CardInstance] | None = None
    mapping: dict[CardId, CardId] | None = None

    def __init__(
        self,
        player_id: int,
        count: int = 1,
        *,
        upgrade: bool = False,
        cards: list[CardInstance] | None = None,
        mapping: dict[CardId, CardId] | None = None,
    ):
        super().__init__(player_id=player_id, reward_type=RewardType.TRANSFORM_CARD, rewards_set_index=4, skippable=False)
        self.count = count
        self.upgrade = upgrade
        self.cards = cards
        self.mapping = mapping

    def select(self, run_manager: RunManager, **_: object) -> dict:
        player = run_manager.run_state.get_player(self.player_id)
        if self.mapping is not None:
            candidates = list(self.cards or [])
            required = min(self.count, len(candidates))
            if required > 0 and player.request_deck_choice(
                prompt=f"Choose {required} card(s) to transform",
                cards=candidates,
                resolver=lambda selected: player.transform_specific_cards_with_mapping(selected, self.mapping or {}),
                allow_skip=False,
                min_count=required,
                max_count=required,
            ):
                return {
                    "description": f"Choose {required} card(s) to transform.",
                    "pending_choice": True,
                }
            transformed = player.transform_specific_cards_with_mapping(candidates[:required], self.mapping)
        else:
            transformed = (
                player.transform_and_upgrade_cards(self.count, cards=self.cards)
                if self.upgrade
                else player.transform_cards(self.count, cards=self.cards)
            )
        if run_manager.run_state.pending_choice is not None:
            verb = "transform and upgrade" if self.upgrade else "transform"
            return {
                "description": f"Choose {self.count} card(s) to {verb}.",
                "pending_choice": True,
            }
        return {
            "description": f"Transformed {transformed} card(s).",
            "transformed": transformed,
        }


@dataclass
class DuplicateCardReward(Reward):
    count: int = 1
    cards: list[CardInstance] | None = None

    def __init__(self, player_id: int, count: int = 1, cards: list[CardInstance] | None = None):
        super().__init__(player_id=player_id, reward_type=RewardType.DUPLICATE_CARD, rewards_set_index=4, skippable=False)
        self.count = count
        self.cards = cards

    def select(self, run_manager: RunManager, **_: object) -> dict:
        player = run_manager.run_state.get_player(self.player_id)
        duplicated = 0
        for _ in range(self.count):
            if player.duplicate_card_from_deck(cards=self.cards):
                duplicated += 1
            if run_manager.run_state.pending_choice is not None:
                break
        if run_manager.run_state.pending_choice is not None:
            return {
                "description": f"Choose {self.count} card(s) to duplicate.",
                "pending_choice": True,
            }
        return {
            "description": f"Duplicated {duplicated} card(s).",
            "duplicated": duplicated,
        }


@dataclass
class EnchantCardsReward(Reward):
    enchantment: str = ""
    amount: int = 1
    count: int = 1
    cards: list[CardInstance] | None = None

    def __init__(
        self,
        player_id: int,
        enchantment: str,
        amount: int = 1,
        count: int = 1,
        cards: list[CardInstance] | None = None,
    ):
        super().__init__(player_id=player_id, reward_type=RewardType.ENCHANT_CARD, rewards_set_index=4, skippable=False)
        self.enchantment = enchantment
        self.amount = amount
        self.count = count
        self.cards = cards

    def select(self, run_manager: RunManager, **_: object) -> dict:
        player = run_manager.run_state.get_player(self.player_id)
        enchanted = player.enchant_selected_cards(self.enchantment, self.amount, self.count, cards=self.cards)
        if run_manager.run_state.pending_choice is not None:
            return {
                "description": f"Choose up to {self.count} card(s) to enchant with {self.enchantment}.",
                "pending_choice": True,
            }
        return {
            "description": f"Enchanted {enchanted} card(s) with {self.enchantment}.",
            "enchanted": enchanted,
        }


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
    _reward_modifiers_applied: bool = False

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
        if getattr(room, "suppress_default_rewards", False):
            if hasattr(room, "extra_rewards"):
                self.rewards.extend(room.extra_rewards.get(self.player_id, []))
            return self
        if room.room_type in _GOLD_REWARD_RANGES:
            low, high = _GOLD_REWARD_RANGES[room.room_type]
            self.rewards.append(GoldReward(self.player_id, low, high))
            player = run_state.get_player(self.player_id)
            forced_potion = any(
                relic.should_force_potion_reward(player) is True
                for relic in player.get_relic_objects()
            )
            if forced_potion or run_state.potion_reward_odds.roll(
                run_state.rng.rewards,
                is_elite=room.room_type == RoomType.ELITE,
            ):
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
        if not self._reward_modifiers_applied:
            player = run_state.get_player(self.player_id)
            rewards = list(self.rewards)
            for relic in player.get_relic_objects():
                rewards = list(relic.modify_rewards(player, rewards, self.room, run_state))
            self.rewards = rewards
            self._reward_modifiers_applied = True
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
