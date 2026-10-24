"""Reward objects and reward-set assembly."""

from __future__ import annotations

from dataclasses import dataclass, field
from enum import Enum, auto
from typing import TYPE_CHECKING, Any, Callable

from sts2_env.cards.base import CardInstance
from sts2_env.cards.factory import create_card
from sts2_env.characters.all import IRONCLAD
from sts2_env.core.enums import CardId, CardRarity, CardType, MapPointType, RoomType
from sts2_env.potions.base import create_potion, roll_random_potion_model
from sts2_env.relics.base import RelicId, RelicPool, RelicRarity
from sts2_env.relics.registry import RELIC_REGISTRY, load_all_relics
from sts2_env.run.rooms import CombatRoom, TreasureRoom
from sts2_env.run.rewards import (
    CARD_CREATION_SOURCE_ENCOUNTER,
    CARD_CREATION_SOURCE_OTHER,
    CARD_GENERATION_CONTEXT_COMBAT,
    CardRewardGenerationOptions,
    DEFAULT_CARD_REWARD_OPTION_COUNT,
    generate_combat_reward_cards,
    generate_noncombat_reward_cards,
    generate_uniform_noncombat_reward_cards_with_options,
)

if TYPE_CHECKING:
    from sts2_env.run.rooms import Room
    from sts2_env.run.run_manager import RunManager
    from sts2_env.run.run_state import RunState
from sts2_env.run.run_state import UNLOCK_STATE_EPOCH_UNLOCK_COUNT_KEY, UNLOCK_STATE_NUMBER_OF_RUNS_KEY


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
    CARD_BUNDLE = auto()


GOLD_REWARD_SET_INDEX = 1
POTION_REWARD_SET_INDEX = 2
RELIC_REWARD_SET_INDEX = 3
SPECIAL_REWARD_SET_INDEX = 4
CARD_REWARD_SET_INDEX = 5
CARD_REMOVAL_REWARD_SET_INDEX = 7
MAX_CARD_REWARD_ALTERNATIVES = 2
CARD_REWARD_ALTERNATIVE_LIMIT_MESSAGE = "More than 2 card reward alternatives are not supported."


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
        if not self.skippable:
            return {"description": f"Cannot skip {self.reward_type.name.lower()} reward.", "success": False}
        return {"description": f"Skipped {self.reward_type.name.lower()} reward."}


@dataclass
class GoldReward(Reward):
    min_gold: int = 0
    max_gold: int = 0
    amount: int = 0
    skippable: bool = False

    def __init__(self, player_id: int, min_gold: int, max_gold: int):
        super().__init__(
            player_id=player_id,
            reward_type=RewardType.GOLD,
            rewards_set_index=GOLD_REWARD_SET_INDEX,
            skippable=False,
        )
        self.min_gold = min_gold
        self.max_gold = max_gold
        self.amount = 0

    @classmethod
    def fixed(cls, player_id: int, amount: int) -> GoldReward:
        reward = cls(player_id, amount, amount)
        reward.amount = amount
        reward.is_populated = True
        return reward

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
        super().__init__(player_id=player_id, reward_type=RewardType.POTION, rewards_set_index=POTION_REWARD_SET_INDEX)
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
        skippable: bool = True,
    ):
        super().__init__(
            player_id=player_id,
            reward_type=RewardType.RELIC,
            rewards_set_index=RELIC_REWARD_SET_INDEX,
            skippable=skippable,
        )
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
    cards_to_pick: int = 1
    cards_picked: int = 0
    generation_context: str | None = CARD_GENERATION_CONTEXT_COMBAT
    roll_upgrade: bool = True
    card_type: CardType | None = None
    character_ids: tuple[str, ...] = field(default_factory=tuple)
    forced_rarities: tuple[CardRarity, ...] = field(default_factory=tuple)
    include_colorless: bool = False
    use_default_character_pool: bool = True
    card_creation_source: str = CARD_CREATION_SOURCE_ENCOUNTER
    allow_card_pool_modifications: bool = True
    allow_rarity_modifications: bool = True
    allow_hook_upgrades: bool = True
    has_custom_card_pool: bool = False
    custom_card_ids: tuple[CardId, ...] = field(default_factory=tuple)
    card_pool_rarity_filter: CardRarity | None = None
    use_uniform_noncombat_odds: bool = False
    upgrade_after_generation: bool = False
    cards: list[CardInstance] = field(default_factory=list)
    max_rerolls: int = 0
    rerolls_used: int = 0
    _can_regenerate: bool = True

    def __init__(
        self,
        player_id: int,
        context: str = "regular",
        option_count: int | None = None,
        cards_to_pick: int = 1,
        *,
        skippable: bool = True,
        generation_context: str | None = CARD_GENERATION_CONTEXT_COMBAT,
        roll_upgrade: bool = True,
        card_type: CardType | None = None,
        character_ids: tuple[str, ...] | None = None,
        forced_rarities: tuple[CardRarity, ...] | None = None,
        include_colorless: bool = False,
        use_default_character_pool: bool = True,
        card_creation_source: str | None = None,
        allow_card_pool_modifications: bool = True,
        allow_rarity_modifications: bool = True,
        allow_hook_upgrades: bool = True,
        has_custom_card_pool: bool = False,
        custom_card_ids: tuple[CardId, ...] | None = None,
        card_pool_rarity_filter: CardRarity | None = None,
        use_uniform_noncombat_odds: bool = False,
        upgrade_after_generation: bool = False,
        cards: list[CardInstance] | None = None,
    ):
        super().__init__(
            player_id=player_id,
            reward_type=RewardType.CARD,
            rewards_set_index=CARD_REWARD_SET_INDEX,
            skippable=skippable,
        )
        cards_were_manually_set = cards is not None
        resolved_cards = list(cards or [])
        resolved_rarities = tuple(forced_rarities or ())
        self.context = context
        if option_count is not None:
            self.option_count = option_count
        elif cards_were_manually_set:
            self.option_count = len(resolved_cards)
        else:
            self.option_count = len(resolved_rarities) or DEFAULT_CARD_REWARD_OPTION_COUNT
        self.cards_to_pick = max(1, cards_to_pick)
        self.cards_picked = 0
        self.generation_context = generation_context
        self.roll_upgrade = roll_upgrade
        self.card_type = card_type
        self.character_ids = tuple(character_ids or ())
        self.forced_rarities = resolved_rarities
        self.include_colorless = include_colorless
        self.use_default_character_pool = use_default_character_pool
        self.card_creation_source = card_creation_source or (
            CARD_CREATION_SOURCE_OTHER if generation_context is None else CARD_CREATION_SOURCE_ENCOUNTER
        )
        self.allow_card_pool_modifications = allow_card_pool_modifications and not cards_were_manually_set
        self.allow_rarity_modifications = allow_rarity_modifications
        self.allow_hook_upgrades = allow_hook_upgrades
        self.has_custom_card_pool = has_custom_card_pool
        self.custom_card_ids = tuple(custom_card_ids or ())
        self.card_pool_rarity_filter = card_pool_rarity_filter
        self.use_uniform_noncombat_odds = use_uniform_noncombat_odds
        self.upgrade_after_generation = upgrade_after_generation
        self.cards = resolved_cards
        self.max_rerolls = 0
        self.rerolls_used = 0
        self._can_regenerate = not cards_were_manually_set
        self._populated_room: Room | None = None

    def _register_active_reward(self, run_state: RunState) -> None:
        if self not in run_state.active_card_rewards:
            run_state.active_card_rewards.append(self)

    def _unregister_active_reward(self, run_state: RunState) -> None:
        if self in run_state.active_card_rewards:
            run_state.active_card_rewards.remove(self)

    def populate(self, run_state: RunState, room: Room | None) -> None:
        player = run_state.get_player(self.player_id)
        self._populated_room = room
        options = CardRewardGenerationOptions(
            context=self.context,
            num_cards=self.option_count,
            character_ids=self.character_ids,
            forced_rarities=self.forced_rarities,
            include_colorless=self.include_colorless,
            use_default_character_pool=self.use_default_character_pool,
            generation_context=self.generation_context,
            roll_upgrade=self.roll_upgrade,
            card_type=self.card_type,
            card_creation_source=self.card_creation_source,
            allow_card_pool_modifications=self.allow_card_pool_modifications,
            allow_rarity_modifications=self.allow_rarity_modifications,
            allow_hook_upgrades=self.allow_hook_upgrades,
            has_custom_card_pool=self.has_custom_card_pool,
            custom_card_ids=self.custom_card_ids,
            card_pool_rarity_filter=self.card_pool_rarity_filter,
        )
        if self._can_regenerate:
            for relic in player.get_relic_objects():
                options = relic.modify_card_reward_creation_options(
                    player,
                    options,
                    self,
                    room,
                    run_state,
                )
            for modifier in run_state.modifiers:
                options = modifier.modify_card_reward_creation_options(
                    player,
                    options,
                    self,
                    room,
                    run_state,
                )
        if self._can_regenerate and not self.cards:
            character_ids = None if options.use_default_character_pool and not options.character_ids else options.character_ids
            if options.card_creation_source == CARD_CREATION_SOURCE_OTHER and self.use_uniform_noncombat_odds:
                self.cards = generate_uniform_noncombat_reward_cards_with_options(
                    run_state,
                    options,
                    default_character_id=player.character_id,
                )
            elif options.card_creation_source == CARD_CREATION_SOURCE_OTHER:
                self.cards = generate_noncombat_reward_cards(
                    run_state,
                    num_cards=options.num_cards,
                    character_ids=character_ids,
                    forced_rarities=options.forced_rarities,
                    include_colorless=options.include_colorless,
                    card_type=options.card_type,
                    custom_card_ids=options.custom_card_ids,
                    roll_upgrade=options.roll_upgrade,
                )
            else:
                self.cards = generate_combat_reward_cards(
                    run_state,
                    context=options.context,
                    num_cards=options.num_cards,
                    character_ids=character_ids,
                    forced_rarities=options.forced_rarities,
                    include_colorless=options.include_colorless,
                    generation_context=options.generation_context,
                    roll_upgrade=options.roll_upgrade,
                    update_rarity_odds=options.card_creation_source == CARD_CREATION_SOURCE_ENCOUNTER,
                    card_type=options.card_type,
                    custom_card_ids=options.custom_card_ids,
                )
        self.include_colorless = options.include_colorless
        self.use_default_character_pool = options.use_default_character_pool
        self.generation_context = options.generation_context
        self.roll_upgrade = options.roll_upgrade
        self.card_type = options.card_type
        self.card_creation_source = options.card_creation_source
        self.allow_card_pool_modifications = options.allow_card_pool_modifications
        self.allow_rarity_modifications = options.allow_rarity_modifications
        self.allow_hook_upgrades = options.allow_hook_upgrades
        self.has_custom_card_pool = options.has_custom_card_pool
        self.custom_card_ids = options.custom_card_ids
        self.card_pool_rarity_filter = options.card_pool_rarity_filter
        listeners = [*player.get_relic_objects(), *run_state.modifiers]
        for listener in listeners:
            modify_card_reward_options = getattr(listener, "modify_card_reward_options", None)
            if not callable(modify_card_reward_options):
                continue
            self.cards = modify_card_reward_options(
                player,
                self.cards,
                self,
                room,
                run_state,
            )
        for listener in listeners:
            modify_card_reward_options_late = getattr(listener, "modify_card_reward_options_late", None)
            if not callable(modify_card_reward_options_late):
                continue
            self.cards = modify_card_reward_options_late(
                player,
                self.cards,
                self,
                room,
                run_state,
            )
        if self.upgrade_after_generation:
            for card in self.cards:
                player.upgrade_card_instance(card)
        alternatives = int(self.skippable) + int(self.rerolls_remaining > 0)
        if any(callable(getattr(relic, "sacrifice_card_reward", None)) for relic in player.get_relic_objects()):
            alternatives += 1
        if alternatives > MAX_CARD_REWARD_ALTERNATIVES:
            raise ValueError(CARD_REWARD_ALTERNATIVE_LIMIT_MESSAGE)
        self.option_count = len(self.cards)
        self.is_populated = True
        if self._can_regenerate:
            self._register_active_reward(run_state)

    def on_relic_obtained(self, relic: object, run_state: RunState) -> None:
        player = run_state.get_player(self.player_id)
        modify_card_reward_options = getattr(relic, "modify_card_reward_options", None)
        if callable(modify_card_reward_options):
            self.cards = modify_card_reward_options(
                player,
                self.cards,
                self,
                self._populated_room,
                run_state,
            )
        modify_card_reward_options_late = getattr(relic, "modify_card_reward_options_late", None)
        if callable(modify_card_reward_options_late):
            self.cards = modify_card_reward_options_late(
                player,
                self.cards,
                self,
                self._populated_room,
                run_state,
            )
        self.option_count = len(self.cards)

    @property
    def rerolls_remaining(self) -> int:
        return max(0, self.max_rerolls - self.rerolls_used)

    def select(self, run_manager: RunManager, **kwargs: object) -> dict:
        index = int(kwargs.get("index", 0))
        if 0 <= index < len(self.cards):
            card = self.cards[index]
            run_manager.run_state.get_player(self.player_id).add_card_instance_to_deck(card)
            self.cards.pop(index)
            self.cards_picked += 1
            info = {
                "description": f"Picked {card.card_id.name}.",
                "card_id": card.card_id.name,
                "rarity": card.rarity.name,
                "upgraded": card.upgraded,
            }
            if self.cards_picked < self.cards_to_pick and self.cards:
                info["pending_more_picks"] = True
                info["remaining_picks"] = self.cards_to_pick - self.cards_picked
            else:
                self._unregister_active_reward(run_manager.run_state)
            return info
        return {"description": "Invalid card index."}

    def skip(self, run_manager: RunManager) -> dict:
        self._unregister_active_reward(run_manager.run_state)
        return super().skip(run_manager)

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
        super().__init__(
            player_id=player_id,
            reward_type=RewardType.ADD_CARD,
            rewards_set_index=SPECIAL_REWARD_SET_INDEX,
            skippable=False,
        )
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
class RecoveredCardReward(Reward):
    card: CardInstance | None = None
    encounter_source: str = ""

    def __init__(self, player_id: int, card: CardInstance, encounter_source: str = ""):
        super().__init__(
            player_id=player_id,
            reward_type=RewardType.ADD_CARD,
            rewards_set_index=SPECIAL_REWARD_SET_INDEX,
            skippable=False,
        )
        self.card = card
        self.encounter_source = encounter_source

    def select(self, run_manager: RunManager, **_: object) -> dict:
        return {
            "description": f"Recovered {self.card.card_id.name if self.card is not None else 'card'}.",
            "card_id": self.card.card_id.name if self.card is not None else None,
            "encounter_source": self.encounter_source,
        }


@dataclass
class CardBundlesReward(Reward):
    bundles: list[list[CardInstance]] = field(default_factory=list)

    def __init__(self, player_id: int, bundles: list[list[CardInstance]]):
        super().__init__(
            player_id=player_id,
            reward_type=RewardType.CARD_BUNDLE,
            rewards_set_index=CARD_REWARD_SET_INDEX,
            skippable=False,
        )
        self.bundles = [list(bundle) for bundle in bundles]

    def populate(self, run_state: RunState, room: Room | None) -> None:
        self.is_populated = True

    def select(self, run_manager: RunManager, **kwargs: object) -> dict:
        index = int(kwargs.get("index", 0))
        if index < 0 or index >= len(self.bundles):
            return {"description": "Invalid card bundle index.", "success": False}
        player = run_manager.run_state.get_player(self.player_id)
        bundle = self.bundles[index]
        for card in bundle:
            player.add_card_instance_to_deck(card)
        return {
            "description": f"Picked card bundle with {len(bundle)} card(s).",
            "added": len(bundle),
            "card_ids": [card.card_id.name for card in bundle],
            "success": True,
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
        super().__init__(
            player_id=player_id,
            reward_type=RewardType.OBTAIN_RELIC,
            rewards_set_index=SPECIAL_REWARD_SET_INDEX,
            skippable=False,
        )
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
        super().__init__(
            player_id=player_id,
            reward_type=RewardType.LOSE_HP,
            rewards_set_index=SPECIAL_REWARD_SET_INDEX,
            skippable=False,
        )
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
        super().__init__(
            player_id=player_id,
            reward_type=RewardType.LOSE_GOLD,
            rewards_set_index=SPECIAL_REWARD_SET_INDEX,
            skippable=False,
        )
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
    require_manual_confirmation: bool = False
    after_remove_card_names: tuple[str, ...] = field(default_factory=tuple)
    after_selected: Callable[[], None] | None = None

    def __init__(
        self,
        player_id: int,
        count: int = 1,
        cards: list[CardInstance] | None = None,
        *,
        require_manual_confirmation: bool = False,
        after_remove_card_names: tuple[str, ...] | None = None,
        after_selected: Callable[[], None] | None = None,
    ):
        super().__init__(
            player_id=player_id,
            reward_type=RewardType.REMOVE_CARD,
            rewards_set_index=CARD_REMOVAL_REWARD_SET_INDEX,
            skippable=False,
        )
        self.count = count
        self.cards = cards
        self.require_manual_confirmation = require_manual_confirmation
        self.after_remove_card_names = tuple(after_remove_card_names or ())
        self.after_selected = after_selected

    def select(self, run_manager: RunManager, **_: object) -> dict:
        player = run_manager.run_state.get_player(self.player_id)
        previous_choice = run_manager.run_state.pending_choice

        def after_selected() -> None:
            for card_name in self.after_remove_card_names:
                player.add_card_to_deck(card_name)
            if self.after_selected is not None:
                self.after_selected()

        removed = player.remove_cards_from_deck(
            self.count,
            cards=self.cards,
            require_manual_confirmation=self.require_manual_confirmation,
            after_selected=after_selected,
        )
        if run_manager.run_state.pending_choice is not None and previous_choice is None:
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
    require_manual_confirmation: bool = False

    def __init__(
        self,
        player_id: int,
        count: int = 1,
        cards: list[CardInstance] | None = None,
        *,
        require_manual_confirmation: bool = False,
    ):
        super().__init__(
            player_id=player_id,
            reward_type=RewardType.UPGRADE_CARD,
            rewards_set_index=SPECIAL_REWARD_SET_INDEX,
            skippable=False,
        )
        self.count = count
        self.cards = cards
        self.require_manual_confirmation = require_manual_confirmation

    def select(self, run_manager: RunManager, **_: object) -> dict:
        player = run_manager.run_state.get_player(self.player_id)
        upgraded = player.upgrade_selected_cards(
            self.count,
            cards=self.cards,
            require_manual_confirmation=self.require_manual_confirmation,
        )
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
    rng_stream: str = "niche"
    rng_override: object | None = None
    after_selected: Callable[[], None] | None = None

    def __init__(
        self,
        player_id: int,
        count: int = 1,
        *,
        upgrade: bool = False,
        cards: list[CardInstance] | None = None,
        mapping: dict[CardId, CardId] | None = None,
        rng_stream: str = "niche",
        rng_override: object | None = None,
        after_selected: Callable[[], None] | None = None,
    ):
        super().__init__(
            player_id=player_id,
            reward_type=RewardType.TRANSFORM_CARD,
            rewards_set_index=SPECIAL_REWARD_SET_INDEX,
            skippable=False,
        )
        self.count = count
        self.upgrade = upgrade
        self.cards = cards
        self.mapping = mapping
        self.rng_stream = rng_stream
        self.rng_override = rng_override
        self.after_selected = after_selected

    def select(self, run_manager: RunManager, **_: object) -> dict:
        player = run_manager.run_state.get_player(self.player_id)
        transform_rng = self.rng_override or getattr(
            run_manager.run_state.rng,
            self.rng_stream,
            run_manager.run_state.rng.niche,
        )
        if self.mapping is not None:
            candidates = list(self.cards or [])
            required = min(self.count, len(candidates))

            def transform_with_mapping(selected: list[CardInstance]) -> int:
                transformed = player.transform_specific_cards_with_mapping(selected, self.mapping or {})
                if self.after_selected is not None:
                    self.after_selected()
                return transformed

            if required > 0 and player._can_auto_resolve_deck_choice(
                candidates,
                allow_skip=False,
                min_count=required,
                max_count=required,
            ):
                transformed = transform_with_mapping(candidates[:required])
            elif required > 0 and player.request_deck_choice(
                prompt=f"Choose {required} card(s) to transform",
                cards=candidates,
                resolver=transform_with_mapping,
                allow_skip=False,
                min_count=required,
                max_count=required,
            ):
                return {
                    "description": f"Choose {required} card(s) to transform.",
                    "pending_choice": True,
                }
            else:
                transformed = transform_with_mapping(candidates[:required])
        else:
            transformed = (
                player.transform_and_upgrade_cards(
                    self.count,
                    cards=self.cards,
                    rng=transform_rng,
                    after_selected=self.after_selected,
                )
                if self.upgrade
                else player.transform_cards(
                    self.count,
                    cards=self.cards,
                    rng=transform_rng,
                    after_selected=self.after_selected,
                )
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
        super().__init__(
            player_id=player_id,
            reward_type=RewardType.DUPLICATE_CARD,
            rewards_set_index=SPECIAL_REWARD_SET_INDEX,
            skippable=False,
        )
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
    min_count: int | None = None
    after_selected: Callable[[], None] | None = None

    def __init__(
        self,
        player_id: int,
        enchantment: str,
        amount: int = 1,
        count: int = 1,
        cards: list[CardInstance] | None = None,
        *,
        min_count: int | None = None,
        after_selected: Callable[[], None] | None = None,
    ):
        super().__init__(
            player_id=player_id,
            reward_type=RewardType.ENCHANT_CARD,
            rewards_set_index=SPECIAL_REWARD_SET_INDEX,
            skippable=False,
        )
        self.enchantment = enchantment
        self.amount = amount
        self.count = count
        self.cards = cards
        self.min_count = min_count
        self.after_selected = after_selected

    def select(self, run_manager: RunManager, **_: object) -> dict:
        player = run_manager.run_state.get_player(self.player_id)
        enchanted = player.enchant_selected_cards(
            self.enchantment,
            self.amount,
            self.count,
            cards=self.cards,
            min_count=self.min_count,
            after_selected=self.after_selected,
        )
        if run_manager.run_state.pending_choice is not None:
            return {
                "description": f"Choose {self.count} card(s) to enchant with {self.enchantment}.",
                "pending_choice": True,
            }
        return {
            "description": f"Enchanted {enchanted} card(s) with {self.enchantment}.",
            "enchanted": enchanted,
        }


ENCOUNTER_GOLD_REWARD_RANGES: dict[RoomType, tuple[int, int]] = {
    RoomType.MONSTER: (10, 20),
    RoomType.ELITE: (35, 45),
    RoomType.BOSS: (100, 100),
}


def calculate_gold_proportion(combat_state: Any) -> float:
    """Proportion of gold granted when a combat ends.

    Matches C# EncounterModel.CalculateGoldProportion (EncounterModel.cs
    L373-376): ``1f - EscapedCreatures.Count / SpawnedEnemies.Count``.
    Escaped creatures are ones that fled combat (removed while still alive);
    creatures that died are not counted as escaped (C# only adds to
    EscapedCreatures via CreatureEscaped). The Python engine flags both
    deaths and flees with ``escaped`` on the Creature, so a fled creature is
    identified by ``escaped`` with HP still above zero.
    """
    spawned = list(getattr(combat_state, "enemies", None) or [])
    if not spawned:
        return 1.0
    escaped = sum(
        1
        for creature in spawned
        if getattr(creature, "escaped", False) and creature.current_hp > 0
    )
    return 1.0 - escaped / len(spawned)
POVERTY_ASCENSION_LEVEL = 3
POVERTY_ASCENSION_GOLD_MULTIPLIER = 0.75
TUTORIAL_FIRST_RUN_COUNT = 0
TUTORIAL_UNLOCKED_EPOCH_COUNT = 0
TUTORIAL_FIRST_MONSTER_COUNT = 1
TUTORIAL_THIRD_MONSTER_COUNT = 3
TUTORIAL_FIFTH_MONSTER_COUNT = 5
TUTORIAL_SEVENTH_MONSTER_COUNT = 7
TUTORIAL_FIRST_ELITE_COUNT = 1
TUTORIAL_SECOND_ELITE_COUNT = 2
TUTORIAL_FIRST_BOSS_COUNT = 1
TUTORIAL_FIRE_POTION_ID = "FirePotion"
TUTORIAL_STRENGTH_POTION_ID = "StrengthPotion"
TUTORIAL_ENERGY_POTION_ID = "EnergyPotion"
TUTORIAL_BLOCK_POTION_ID = "BlockPotion"
TUTORIAL_MONSTER_CARD_REWARDS: tuple[tuple[CardId, CardId, CardId], ...] = (
    (CardId.SETUP_STRIKE_CARD, CardId.TREMBLE, CardId.BLOOD_WALL),
    (CardId.BREAKTHROUGH, CardId.INFLAME, CardId.ANGER),
    (CardId.IRON_WAVE, CardId.DISMANTLE, CardId.CINDER),
    (CardId.STOMP, CardId.SHRUG_IT_OFF, CardId.ARMAMENTS),
    (CardId.THUNDERCLAP, CardId.SETUP_STRIKE_CARD, CardId.RAGE_CARD),
    (CardId.BATTLE_TRANCE, CardId.TRUE_GRIT, CardId.UPPERCUT),
    (CardId.BLOODLETTING, CardId.WHIRLWIND, CardId.TREMBLE),
)
TUTORIAL_MONSTER_POTION_REWARDS = {
    TUTORIAL_THIRD_MONSTER_COUNT: TUTORIAL_FIRE_POTION_ID,
    TUTORIAL_FIFTH_MONSTER_COUNT: TUTORIAL_STRENGTH_POTION_ID,
    TUTORIAL_SEVENTH_MONSTER_COUNT: TUTORIAL_ENERGY_POTION_ID,
}
TUTORIAL_ELITE_REWARDS = {
    TUTORIAL_FIRST_ELITE_COUNT: (
        (CardId.BLUDGEON, CardId.PYRE, CardId.EVIL_EYE),
        TUTORIAL_BLOCK_POTION_ID,
        RelicId.VAJRA.name,
    ),
    TUTORIAL_SECOND_ELITE_COUNT: (
        (CardId.PILLAGE, CardId.RAMPAGE, CardId.FLAME_BARRIER_CARD),
        None,
        RelicId.ORNAMENTAL_FAN.name,
    ),
}
TUTORIAL_BOSS_CARD_REWARD = (CardId.PRIMAL_FORCE, CardId.DEMON_FORM_CARD, CardId.THRASH)


@dataclass
class RewardsSet:
    player_id: int
    room: Room | None = None
    rewards: list[Reward] = field(default_factory=list)
    allow_empty_rewards: bool = False
    preserve_order: bool = False
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
        if self._try_generate_tutorial_rewards(room, run_state):
            if hasattr(room, "extra_rewards"):
                self.rewards.extend(room.extra_rewards.get(self.player_id, []))
            return self
        if not isinstance(room, CombatRoom):
            if not isinstance(room, TreasureRoom):
                raise ValueError(f"Tried to generate a reward for invalid room type: {type(room).__name__}")
        elif room.room_type in ENCOUNTER_GOLD_REWARD_RANGES:
            low, high = ENCOUNTER_GOLD_REWARD_RANGES[room.room_type]
            if run_state.ascension_level >= POVERTY_ASCENSION_LEVEL:
                # C# EncounterModel.Min/MaxGoldReward: num *= 0.75 then
                # `return (int)num` -- truncation, not rounding.
                low = int(low * POVERTY_ASCENSION_GOLD_MULTIPLIER)
                high = int(high * POVERTY_ASCENSION_GOLD_MULTIPLIER)
            gold_proportion = getattr(room, "gold_proportion", 1.0) if room.room_type == RoomType.MONSTER else 1.0
            if gold_proportion > 0:
                self.rewards.append(
                    GoldReward(
                        self.player_id,
                        round(low * gold_proportion),
                        round(high * gold_proportion),
                    )
                )
            player = run_state.get_player(self.player_id)
            forced_potion = any(
                relic.should_force_potion_reward(player, room) is True
                for relic in player.get_relic_objects()
            )
            if run_state.potion_reward_odds.roll(
                run_state.rng.rewards,
                is_elite=room.room_type == RoomType.ELITE,
                force=forced_potion,
            ):
                self.rewards.append(PotionReward(self.player_id))
            self.rewards.append(CardReward(self.player_id, context=self._card_context(room.room_type)))
            if room.room_type == RoomType.ELITE:
                self.rewards.append(RelicReward(self.player_id))

        if hasattr(room, "extra_rewards"):
            self.rewards.extend(room.extra_rewards.get(self.player_id, []))
        return self

    def with_custom_rewards(self, rewards: list[Reward], *, preserve_order: bool = False) -> RewardsSet:
        self.rewards.extend(rewards)
        self.preserve_order = self.preserve_order or preserve_order
        return self

    def generate_without_offering(self, run_state: RunState) -> list[Reward]:
        if not self._reward_modifiers_applied:
            player = run_state.get_player(self.player_id)
            for reward in self.rewards:
                if not reward.is_populated:
                    reward.populate(run_state, self.room)
            rewards = list(self.rewards)
            listeners = [*player.get_relic_objects(), *run_state.modifiers]
            modified_by: list[object] = []
            for hook_name in ("modify_rewards", "modify_rewards_late"):
                for listener in listeners:
                    method = getattr(listener, hook_name, None)
                    if not callable(method):
                        continue
                    updated = method(player, rewards, self.room, run_state)
                    if updated is None:
                        continue
                    if updated is not rewards and not any(listener is modifier for modifier in modified_by):
                        modified_by.append(listener)
                    rewards = list(updated)
            self.rewards = rewards
            for reward in self.rewards:
                if not reward.is_populated:
                    reward.populate(run_state, self.room)
            for listener in listeners:
                if any(listener is modifier for modifier in modified_by):
                    after_modifying_rewards = getattr(listener, "after_modifying_rewards", None)
                    if callable(after_modifying_rewards):
                        after_modifying_rewards(player, run_state)
            self._reward_modifiers_applied = True
        for reward in self.rewards:
            if not reward.is_populated:
                reward.populate(run_state, self.room)
        if not self.preserve_order:
            self.rewards.sort(key=lambda reward: reward.rewards_set_index)
        return self.rewards

    @staticmethod
    def _card_context(room_type: RoomType) -> str:
        if room_type == RoomType.BOSS:
            return "boss"
        if room_type == RoomType.ELITE:
            return "elite"
        return "regular"

    def _try_generate_tutorial_rewards(self, room: Room, run_state: RunState) -> bool:
        player = run_state.get_player(self.player_id)
        if not self._should_use_tutorial_rewards(player, room, run_state):
            return False
        if room.room_type == RoomType.MONSTER:
            monster_count = run_state.count_map_point_history_entries(room_type=RoomType.MONSTER)
            if not TUTORIAL_FIRST_MONSTER_COUNT <= monster_count <= len(TUTORIAL_MONSTER_CARD_REWARDS):
                return False
            self.rewards.append(GoldReward(self.player_id, *ENCOUNTER_GOLD_REWARD_RANGES[RoomType.MONSTER]))
            potion_id = TUTORIAL_MONSTER_POTION_REWARDS.get(monster_count)
            if potion_id is not None:
                self.rewards.append(PotionReward(self.player_id, potion_id=potion_id))
            self.rewards.append(self._fixed_card_reward(TUTORIAL_MONSTER_CARD_REWARDS[monster_count - TUTORIAL_FIRST_MONSTER_COUNT]))
            return True
        if room.room_type == RoomType.ELITE:
            elite_count = run_state.count_map_point_history_entries(map_point_type=MapPointType.ELITE)
            reward = TUTORIAL_ELITE_REWARDS.get(elite_count)
            if reward is None:
                return False
            card_ids, potion_id, relic_id = reward
            self.rewards.append(GoldReward(self.player_id, *ENCOUNTER_GOLD_REWARD_RANGES[RoomType.ELITE]))
            if potion_id is not None:
                self.rewards.append(PotionReward(self.player_id, potion_id=potion_id))
            self.rewards.append(RelicReward(self.player_id, relic_id=relic_id))
            self.rewards.append(self._fixed_card_reward(card_ids))
            return True
        if (
            room.room_type == RoomType.BOSS
            and run_state.count_map_point_history_entries(map_point_type=MapPointType.BOSS) == TUTORIAL_FIRST_BOSS_COUNT
        ):
            self.rewards.append(GoldReward(self.player_id, *ENCOUNTER_GOLD_REWARD_RANGES[RoomType.BOSS]))
            self.rewards.append(self._fixed_card_reward(TUTORIAL_BOSS_CARD_REWARD))
            return True
        return False

    def _should_use_tutorial_rewards(self, player: object, room: Room, run_state: RunState) -> bool:
        if not isinstance(room, CombatRoom):
            return False
        if player.character_id != IRONCLAD.character_id:
            return False
        unlock_state = getattr(player, "unlock_state", {})
        if unlock_state.get(UNLOCK_STATE_NUMBER_OF_RUNS_KEY) != TUTORIAL_FIRST_RUN_COUNT:
            return False
        if (
            unlock_state.get(UNLOCK_STATE_EPOCH_UNLOCK_COUNT_KEY, len(getattr(player, "discovered_epochs", [])))
            != TUTORIAL_UNLOCKED_EPOCH_COUNT
        ):
            return False
        if not run_state.map_point_history:
            return False
        return room.room_type in {RoomType.MONSTER, RoomType.ELITE, RoomType.BOSS}

    def _fixed_card_reward(self, card_ids: tuple[CardId, ...]) -> CardReward:
        return CardReward(
            self.player_id,
            cards=[create_card(card_id) for card_id in card_ids],
            generation_context=CARD_GENERATION_CONTEXT_COMBAT,
            card_creation_source=CARD_CREATION_SOURCE_ENCOUNTER,
        )
