"""RunState: persistent player state across combats.

Matches RunState.cs from MegaCrit.Sts2.Core.Runs.
"""

from __future__ import annotations

from dataclasses import dataclass, field
from typing import Any, Callable

from sts2_env.cards.enchantments import can_enchant_card
from sts2_env.cards.factory import (
    create_card,
    create_transform_card,
    eligible_character_cards,
    eligible_registered_cards,
    is_multiplayer_only_card,
)
from sts2_env.core.card_pools import CardPoolId
from sts2_env.core.selection import CardChoiceOption, PendingCardChoice
from sts2_env.core.enums import CardId, CardRarity, CardTag, CardType
from sts2_env.core.rng import Rng, deterministic_hash_code
from sts2_env.core.enums import MapPointType, RoomType
from sts2_env.characters.all import get_character
from sts2_env.map.map_point import MapCoord, MapPoint
from sts2_env.map.generator import ActMap, generate_act_map
from sts2_env.map.acts import ActConfig, ALL_ACTS
from sts2_env.potions.base import PotionInstance
from sts2_env.relics.base import RelicId, RelicRarity
from sts2_env.cards.base import (
    CardInstance,
    capture_self_mutating_card_progress,
    new_card_instance_id_after,
    restore_self_mutating_card_progress,
)
from sts2_env.run.odds import UnknownMapPointOdds, CardRarityOdds, PotionRewardOdds
from sts2_env.run.rewards import (
    CARD_CREATION_SOURCE_OTHER,
    CardRewardGenerationOptions,
)


UNLOCK_STATE_NUMBER_OF_RUNS_KEY = "number_of_runs"
UNLOCK_STATE_EPOCH_UNLOCK_COUNT_KEY = "epoch_unlock_count"
FIRST_RUN_COUNT = 0
SCROLL_BOXES_BUNDLE_COUNT = 2
SCROLL_BOXES_COMMON_CARDS_PER_BUNDLE = 2
SCROLL_BOXES_UNCOMMON_CARDS_PER_BUNDLE = 1
SCROLL_BOXES_CLAW_CARDS_PER_BUNDLE = 3
SCROLL_BOXES_CLAW_BUNDLE_CHANCE_PERCENT = 1
SCROLL_BOXES_CLAW_BUNDLE_ROLL_BOUND = 100
EVENT_PET_RELIC_IDS = frozenset({
    RelicId.BYRDPIP.name,
    RelicId.PAELS_LEGION.name,
})


@dataclass(frozen=True)
class MapPointHistoryEntry:
    map_point_type: MapPointType
    room_type: RoomType


@dataclass
class PlayerState:
    """Persistent state for a single player across a run."""

    player_id: int = 1
    character_id: str = "Ironclad"
    max_hp: int = 80
    current_hp: int = 80
    gold: int = 99
    deck: list[CardInstance] = field(default_factory=list)
    relics: list[str] = field(default_factory=list)
    relic_objects: list[Any] = field(default_factory=list)
    potions: list[PotionInstance | None] = field(default_factory=list)
    can_remove_potions: bool = True
    max_potion_slots: int = 3
    max_energy: int = 3
    base_orb_slot_count: int = 0
    relic_grab_bag: list[str] = field(default_factory=list)
    relic_grab_bag_by_rarity: dict[RelicRarity, list[str]] = field(default_factory=dict)
    relic_grab_bag_fallback: list[str] = field(default_factory=list)
    unlock_state: dict[str, Any] = field(default_factory=dict)
    discovered_cards: list[str] = field(default_factory=list)
    discovered_relics: list[str] = field(default_factory=list)
    discovered_potions: list[str] = field(default_factory=list)
    discovered_enemies: list[str] = field(default_factory=list)
    discovered_epochs: list[str] = field(default_factory=list)
    card_shop_removals_used: int = 0
    wongo_points: int = 0
    run_state: Any = None

    def add_potion(self, potion: PotionInstance) -> bool:
        """Add a potion to first empty slot. Returns False if no room."""
        for i in range(self.max_potion_slots):
            if i >= len(self.potions):
                self.potions.append(None)
            if self.potions[i] is None:
                potion.slot_index = i
                self.potions[i] = potion
                return True
        return False

    def remove_potion(self, slot: int) -> PotionInstance | None:
        if 0 <= slot < len(self.potions):
            p = self.potions[slot]
            self.potions[slot] = None
            return p
        return None

    def held_potions(self) -> list[PotionInstance]:
        return [p for p in self.potions if p is not None]

    def request_deck_choice(
        self,
        *,
        prompt: str,
        cards: list[CardInstance],
        resolver,
        allow_skip: bool = False,
        min_count: int = 1,
        max_count: int = 1,
        require_manual_confirmation: bool = False,
    ) -> bool:
        if (
            self.run_state is None
            or self.run_state.pending_choice is not None
            or not getattr(self.run_state, "enable_deck_choice_requests", False)
        ):
            return False
        if not cards:
            return False
        if self._can_auto_resolve_deck_choice(
            cards,
            allow_skip=allow_skip,
            min_count=min_count,
            max_count=max_count,
            require_manual_confirmation=require_manual_confirmation,
        ):
            resolver(list(cards))
            return True
        self.run_state.pending_choice = PendingCardChoice(
            prompt=prompt,
            options=[CardChoiceOption(card=card, source_pile="deck") for card in cards],
            resolver=resolver,
            allow_skip=allow_skip,
            min_choices=min_count,
            max_choices=max_count,
        )
        return True

    def _can_auto_resolve_deck_choice(
        self,
        cards: list[CardInstance],
        *,
        allow_skip: bool,
        min_count: int,
        max_count: int,
        require_manual_confirmation: bool = False,
    ) -> bool:
        return (
            self.run_state is not None
            and self.run_state.pending_choice is None
            and getattr(self.run_state, "enable_deck_choice_requests", False)
            and not require_manual_confirmation
            and not allow_skip
            and min_count == max_count
            and len(cards) <= min_count
        )

    def heal(self, amount: int) -> int:
        before = self.current_hp
        self.current_hp = min(self.current_hp + amount, self.max_hp)
        return self.current_hp - before

    def lose_hp(self, amount: int) -> int:
        actual = min(amount, self.current_hp)
        self.current_hp = max(0, self.current_hp - amount)
        return actual

    def gain_gold(self, amount: int) -> None:
        if amount <= 0:
            return
        for relic in self._ensure_relic_objects():
            if relic.should_gain_gold(self, amount) is False:
                return
        self.gold += int(amount)
        for relic in self._ensure_relic_objects():
            on_gold_gained = getattr(relic, "on_gold_gained", None)
            if callable(on_gold_gained):
                on_gold_gained(self, amount)

    def lose_gold(self, amount: int) -> int:
        actual = min(amount, self.gold)
        self.gold -= actual
        return actual

    def gain_potion_slots(self, amount: int) -> None:
        if amount > 0:
            self.max_potion_slots += amount

    def _ensure_relic_objects(self) -> list[Any]:
        from sts2_env.relics.registry import create_relic_by_name

        if len(self.relic_objects) == len(self.relics):
            return self.relic_objects
        self.relic_objects = []
        for relic_id in self.relics:
            try:
                self.relic_objects.append(create_relic_by_name(relic_id))
            except KeyError:
                continue
        return self.relic_objects

    def get_relic_objects(self) -> list[Any]:
        return self._ensure_relic_objects()

    def tradable_relics(self) -> list[str]:
        from sts2_env.relics.registry import create_relic_by_name

        if len(self.relic_objects) != len(self.relics):
            self._ensure_relic_objects()
        tradable: list[str] = []
        for index, relic_id in enumerate(self.relics):
            relic = self.relic_objects[index] if index < len(self.relic_objects) else None
            if getattr(getattr(relic, "relic_id", None), "name", None) != relic_id:
                try:
                    relic = create_relic_by_name(relic_id)
                except KeyError:
                    continue
            if getattr(relic, "is_tradable", False):
                tradable.append(relic_id)
        return tradable

    def has_event_pet(self) -> bool:
        return any(relic_id in EVENT_PET_RELIC_IDS for relic_id in self.relics)

    def _roll_relic_rarity(self, rng: Any) -> RelicRarity:
        roll = rng.next_float()
        if roll < 0.5:
            return RelicRarity.COMMON
        if roll < 0.83:
            return RelicRarity.UNCOMMON
        return RelicRarity.RARE

    def populate_relic_grab_bag(self) -> None:
        from sts2_env.relics.base import RelicPool
        from sts2_env.relics.registry import RELIC_REGISTRY, load_all_relics

        load_all_relics()
        desired_pool = getattr(RelicPool, self.character_id.upper(), None)
        allowed_rarities = (
            RelicRarity.COMMON,
            RelicRarity.UNCOMMON,
            RelicRarity.RARE,
            RelicRarity.SHOP,
        )
        self.relic_grab_bag_by_rarity = {rarity: [] for rarity in allowed_rarities}
        self.relic_grab_bag_fallback = []
        for relic_id, relic_cls in RELIC_REGISTRY.items():
            if relic_cls.rarity not in allowed_rarities:
                continue
            if relic_cls.pool in {RelicPool.EVENT, RelicPool.FALLBACK, RelicPool.DEPRECATED}:
                continue
            if desired_pool is not None and relic_cls.pool not in {RelicPool.SHARED, desired_pool}:
                continue
            self.relic_grab_bag_by_rarity[relic_cls.rarity].append(relic_id.name)
        for rarity, relic_ids in self.relic_grab_bag_by_rarity.items():
            self.run_state.rng.rewards.shuffle(relic_ids)
        self.relic_grab_bag = [
            relic_id
            for rarity in allowed_rarities
            for relic_id in self.relic_grab_bag_by_rarity.get(rarity, [])
        ]

    def has_available_relics(self) -> bool:
        from sts2_env.relics.registry import create_relic_by_name

        if not self.relic_grab_bag_by_rarity and not self.relic_grab_bag_fallback:
            self.populate_relic_grab_bag()
        for rarity in (RelicRarity.COMMON, RelicRarity.UNCOMMON, RelicRarity.RARE, RelicRarity.SHOP):
            for relic_id in self.relic_grab_bag_by_rarity.get(rarity, []):
                if create_relic_by_name(relic_id).is_allowed(self.run_state):
                    return True
        return any(create_relic_by_name(relic_id).is_allowed(self.run_state) for relic_id in self.relic_grab_bag_fallback)

    def _relic_grab_bag_order(self, rarity: RelicRarity) -> list[RelicRarity]:
        if rarity is RelicRarity.SHOP:
            return [RelicRarity.SHOP, RelicRarity.COMMON, RelicRarity.UNCOMMON, RelicRarity.RARE]
        if rarity is RelicRarity.COMMON:
            return [RelicRarity.COMMON, RelicRarity.UNCOMMON, RelicRarity.RARE]
        if rarity is RelicRarity.UNCOMMON:
            return [RelicRarity.UNCOMMON, RelicRarity.RARE]
        if rarity is RelicRarity.RARE:
            return [RelicRarity.RARE]
        return []

    def remove_relic_from_grab_bag(self, relic_id: str) -> None:
        while relic_id in self.relic_grab_bag:
            self.relic_grab_bag.remove(relic_id)
        for deque in self.relic_grab_bag_by_rarity.values():
            while relic_id in deque:
                deque.remove(relic_id)
        while relic_id in self.relic_grab_bag_fallback:
            self.relic_grab_bag_fallback.remove(relic_id)

    def pull_next_relic_reward_id(
        self,
        *,
        rarity: RelicRarity | None = None,
        rng_stream: str = "rewards",
        excluded_relic_ids: set[str] | None = None,
    ) -> str | None:
        from sts2_env.relics.registry import create_relic_by_name

        if not self.relic_grab_bag_by_rarity and not self.relic_grab_bag_fallback:
            self.populate_relic_grab_bag()
        relic_rng = getattr(self.run_state.rng, rng_stream, self.run_state.rng.rewards)
        target_rarity = rarity or self._roll_relic_rarity(relic_rng)
        excluded = set(excluded_relic_ids or ())
        excluded.update(self.relics)
        for candidate_rarity in self._relic_grab_bag_order(target_rarity):
            deque = self.relic_grab_bag_by_rarity.get(candidate_rarity, [])
            for idx, relic_id in enumerate(list(deque)):
                if relic_id in excluded:
                    continue
                if not create_relic_by_name(relic_id).is_allowed(self.run_state):
                    continue
                deque.pop(idx)
                if relic_id in self.relic_grab_bag:
                    self.relic_grab_bag.remove(relic_id)
                return relic_id
        for idx, relic_id in enumerate(list(self.relic_grab_bag_fallback)):
            if relic_id in excluded:
                continue
            if not create_relic_by_name(relic_id).is_allowed(self.run_state):
                continue
            self.relic_grab_bag_fallback.pop(idx)
            if relic_id in self.relic_grab_bag:
                self.relic_grab_bag.remove(relic_id)
            return relic_id
        return "CIRCLET"

    def upgrade_card_instance(self, card: CardInstance | None) -> CardInstance | None:
        if card is None or card.upgraded:
            return card
        progress = capture_self_mutating_card_progress(card)
        try:
            upgraded_card = create_card(card.card_id, upgraded=True)
        except KeyError:
            return card
        if not upgraded_card.upgraded:
            return card

        card.cost = upgraded_card.cost
        card.card_type = upgraded_card.card_type
        card.target_type = upgraded_card.target_type
        card.rarity = upgraded_card.rarity
        card.base_damage = upgraded_card.base_damage
        card.base_block = upgraded_card.base_block
        card.upgraded = upgraded_card.upgraded
        card.keywords = upgraded_card.keywords
        card.tags = upgraded_card.tags
        card.can_be_generated_in_combat = upgraded_card.can_be_generated_in_combat
        card.can_be_generated_by_modifiers = upgraded_card.can_be_generated_by_modifiers
        card.has_turn_end_in_hand_effect = upgraded_card.has_turn_end_in_hand_effect
        card.effect_vars = dict(upgraded_card.effect_vars)
        card.original_cost = upgraded_card.original_cost
        card.has_energy_cost_x = upgraded_card.has_energy_cost_x
        card.star_cost = upgraded_card.star_cost
        restore_self_mutating_card_progress(card, progress)
        return card

    def modify_card_being_added_to_deck(self, card: CardInstance) -> CardInstance:
        modified = card
        for relic in self.get_relic_objects():
            modifier = getattr(relic, "modify_card_being_added_to_deck", None)
            if not callable(modifier):
                continue
            updated = modifier(self, modified)
            if updated is not None:
                modified = updated
        return modified

    def add_card_instance_to_deck(self, card: CardInstance, source: object | None = None) -> None:
        card = self.modify_card_being_added_to_deck(card)
        self.deck.append(card)
        for relic in self.get_relic_objects():
            on_card_added = getattr(relic, "on_card_added_to_deck", None)
            if callable(on_card_added):
                on_card_added(self, card, source)
        if self.run_state is not None:
            for modifier in self.run_state.modifiers:
                modifier.after_card_added_to_deck(self, card, source)

    def gain_max_hp(self, amount: int) -> None:
        self.max_hp += amount
        self.current_hp += amount

    def lose_max_hp(self, amount: int) -> None:
        self.max_hp = max(1, self.max_hp - amount)
        self.current_hp = min(self.current_hp, self.max_hp)

    def gain_potion_slots(self, amount: int) -> None:
        self.max_potion_slots = max(0, self.max_potion_slots + amount)

    def lose_all_gold(self) -> int:
        lost = self.gold
        self.gold = 0
        return lost

    def take_damage(self, amount: int) -> int:
        return self.lose_hp(amount)

    def enchant_cards(self, enchantment: str, amount: int, count: int) -> int:
        candidates = [card for card in self.deck if can_enchant_card(card, enchantment)]
        return self.enchant_selected_cards(enchantment, amount, count, cards=candidates)

    def enchant_selected_cards(
        self,
        enchantment: str,
        amount: int,
        count: int,
        *,
        cards: list[CardInstance] | None = None,
        min_count: int | None = None,
        after_selected: Callable[[], None] | None = None,
    ) -> int:
        candidates = list(cards) if cards is not None else [card for card in self.deck if can_enchant_card(card, enchantment)]
        max_count = min(count, len(candidates))
        required = min_count if min_count is not None else max_count
        allow_skip = required == 0

        def apply_enchantment(selected: list[CardInstance]) -> int:
            for card in selected:
                card.add_enchantment(enchantment, amount)
            if after_selected is not None:
                after_selected()
            return len(selected)

        if count <= 0:
            return 0
        if count > 0 and candidates and self._can_auto_resolve_deck_choice(
            candidates,
            allow_skip=allow_skip,
            min_count=required,
            max_count=max_count,
        ):
            return apply_enchantment(candidates[:max_count])
        if count > 0 and candidates and self.request_deck_choice(
            prompt=f"Choose {count} cards to enchant with {enchantment}",
            cards=candidates,
            resolver=apply_enchantment,
            allow_skip=allow_skip,
            min_count=required,
            max_count=max_count,
        ):
            return 0
        return apply_enchantment(candidates[:count])

    def enchant_all_cards(self, enchantment: str, amount: int = 1) -> int:
        enchanted = 0
        for card in self.deck:
            if can_enchant_card(card, enchantment):
                card.add_enchantment(enchantment, amount)
                enchanted += 1
        return enchanted

    def enchant_basic_strikes(self, enchantment: str, amount: int = 1) -> int:
        enchanted = 0
        for card in self.deck:
            if (
                card.rarity == CardRarity.BASIC
                and CardTag.STRIKE in card.tags
                and can_enchant_card(card, enchantment)
            ):
                card.add_enchantment(enchantment, amount)
                enchanted += 1
        return enchanted

    def _coerce_card_id(self, name: str) -> CardId | None:
        if name == "Strike":
            return self._basic_card_id_with_tag(CardTag.STRIKE)
        if name == "Defend":
            return self._basic_card_id_with_tag(CardTag.DEFEND)
        candidates = {
            name,
            name.upper(),
            "".join(("_" + ch if ch.isupper() else ch) for ch in name).upper().lstrip("_"),
        }
        for candidate in candidates:
            if candidate in CardId.__members__:
                return CardId[candidate]
        return None

    def _basic_card_id_with_tag(self, tag: CardTag) -> CardId | None:
        from sts2_env.cards.reference_static_metadata import reference_metadata_by_card_id

        metadata_by_card_id = reference_metadata_by_card_id()
        return next(
            (
                card_id
                for card_id in get_character(self.character_id).card_pool
                if (
                    (metadata := metadata_by_card_id.get(card_id)) is not None
                    and metadata.rarity == CardRarity.BASIC
                    and tag in metadata.tags
                )
            ),
            None,
        )

    def add_card_to_deck(self, name: str, upgraded: bool = False) -> bool:
        card_id = self._coerce_card_id(name)
        if card_id is None:
            return False
        try:
            card = create_card(card_id, upgraded=upgraded)
        except KeyError:
            return False
        self.add_card_instance_to_deck(card)
        return True

    def clone_card_for_deck(self, card: CardInstance) -> CardInstance:
        return card.clone(new_card_instance_id_after(self.deck))

    def add_random_card_to_deck(self, rarity: str, upgraded: bool = False) -> bool:
        card_rarity = CardRarity[rarity.upper()]
        candidates = eligible_character_cards(
            self.character_id,
            rarity=card_rarity,
            generation_context="modifier",
            is_multiplayer=len(self.run_state.players) > 1,
        )
        if not candidates:
            return False
        card_id = self.run_state.rng.rewards.choice(candidates)
        self.add_card_instance_to_deck(create_card(card_id, upgraded=upgraded))
        return True

    def add_random_curses(self, count: int, rng: Rng | None = None) -> int:
        curse_ids = eligible_registered_cards(card_pool=CardPoolId.CURSE, generation_context="modifier")
        selected_rng = rng or self.run_state.rng.rewards
        chosen_ids = selected_rng.sample(curse_ids, min(max(0, count), len(curse_ids)))
        added = 0
        for card_id in chosen_ids:
            self.add_card_instance_to_deck(create_card(card_id))
            added += 1
        return added

    def duplicable_deck_cards(self) -> list[CardInstance]:
        return [card for card in self.deck if card.card_type != CardType.QUEST]

    def removable_deck_cards(self) -> list[CardInstance]:
        curses = [card for card in self.deck if card.card_type == CardType.CURSE and card.is_removable]
        others = [card for card in self.deck if card.card_type != CardType.CURSE and card.is_removable]
        return curses + others

    def transformable_deck_cards(self) -> list[CardInstance]:
        return [
            card for card in self.deck
            if card.card_type != CardType.QUEST and card.is_removable
        ]

    def basic_strike_defend_cards(self) -> list[CardInstance]:
        return [
            card for card in self.removable_deck_cards()
            if card.rarity == CardRarity.BASIC and (CardTag.STRIKE in card.tags or CardTag.DEFEND in card.tags)
        ]

    def upgradable_deck_cards(self, card_type: CardType | None = None) -> list[CardInstance]:
        candidates: list[CardInstance] = []
        for card in self.deck:
            if card.upgraded:
                continue
            if card_type is not None and card.card_type != card_type:
                continue
            try:
                upgraded_card = create_card(card.card_id, upgraded=True)
            except KeyError:
                continue
            if upgraded_card.upgraded:
                candidates.append(card)
        return candidates

    def duplicate_card_from_deck(self, *, cards: list[CardInstance] | None = None) -> bool:
        candidates = list(cards) if cards is not None else self.duplicable_deck_cards()
        if candidates and self._can_auto_resolve_deck_choice(
            candidates,
            allow_skip=False,
            min_count=1,
            max_count=1,
        ):
            self.add_card_instance_to_deck(self.clone_card_for_deck(candidates[0]))
            return True
        if candidates and self.request_deck_choice(
            prompt="Choose a card to duplicate",
            cards=candidates,
            resolver=lambda selected: selected and self.add_card_instance_to_deck(self.clone_card_for_deck(selected[0])),
            allow_skip=False,
        ):
            return True
        if not candidates:
            return False
        self.add_card_instance_to_deck(self.clone_card_for_deck(candidates[0]))
        return True

    def duplicate_last_added_card(self, source: object | None = None) -> bool:
        if not self.deck:
            return False
        self.add_card_instance_to_deck(self.clone_card_for_deck(self.deck[-1]), source=source)
        return True

    def upgrade_selected_cards(
        self,
        count: int,
        *,
        cards: list[CardInstance] | None = None,
        require_manual_confirmation: bool = False,
    ) -> int:
        candidates = list(cards) if cards is not None else self.upgradable_deck_cards()
        required = min(count, len(candidates))
        if required > 0 and self._can_auto_resolve_deck_choice(
            candidates,
            allow_skip=False,
            min_count=required,
            max_count=required,
            require_manual_confirmation=require_manual_confirmation,
        ):
            upgraded = 0
            for card in candidates:
                if self.upgrade_card_instance(card) is not None and card.upgraded:
                    upgraded += 1
            return upgraded
        if required > 0 and self.request_deck_choice(
            prompt=f"Choose {min(count, len(candidates))} cards to upgrade",
            cards=candidates,
            resolver=lambda selected: [self.upgrade_card_instance(card) for card in selected],
            allow_skip=False,
            min_count=required,
            max_count=required,
        ):
            return 0
        upgraded = 0
        for card in candidates:
            if upgraded >= count:
                break
            if self.upgrade_card_instance(card) is not None and card.upgraded:
                upgraded += 1
        return upgraded

    def clone_enchanted_cards(self, enchantment: str) -> int:
        clones = []
        for card in self.deck:
            if card.has_enchantment(enchantment):
                clones.append(card.clone(new_card_instance_id_after([*self.deck, *clones])))
        self.deck.extend(clones)
        return len(clones)

    def remove_cards_from_deck(
        self,
        count: int,
        *,
        cards: list[CardInstance] | None = None,
        require_manual_confirmation: bool = False,
        after_selected: Callable[[], None] | None = None,
    ) -> int:
        candidates = list(cards) if cards is not None else self.removable_deck_cards()
        required = min(count, len(candidates))

        def apply_removal(selected: list[CardInstance]) -> int:
            selected_ids = {id(card) for card in selected}
            before = len(self.deck)
            self.deck = [card for card in self.deck if id(card) not in selected_ids]
            removed = before - len(self.deck)
            if after_selected is not None:
                after_selected()
            return removed

        if required > 0 and self._can_auto_resolve_deck_choice(
            candidates,
            allow_skip=False,
            min_count=required,
            max_count=required,
            require_manual_confirmation=require_manual_confirmation,
        ):
            return apply_removal(candidates[:required])
        if required > 0 and self.request_deck_choice(
            prompt=f"Choose {min(count, len(candidates))} cards to remove",
            cards=candidates,
            resolver=apply_removal,
            allow_skip=False,
            min_count=required,
            max_count=required,
        ):
            return 0
        return apply_removal(candidates[:count])

    def transform_cards(
        self,
        count: int,
        *,
        cards: list[CardInstance] | None = None,
        rng: Rng | None = None,
        after_selected: Callable[[], None] | None = None,
    ) -> int:
        candidates = list(cards) if cards is not None else self.transformable_deck_cards()
        required = min(count, len(candidates))

        def apply_transform(selected: list[CardInstance]) -> int:
            transformed = self._transform_selected_cards(selected, rng=rng)
            if after_selected is not None:
                after_selected()
            return transformed

        if required > 0 and self._can_auto_resolve_deck_choice(
            candidates,
            allow_skip=False,
            min_count=required,
            max_count=required,
        ):
            return apply_transform(candidates[:required])
        if required > 0 and self.request_deck_choice(
            prompt=f"Choose {min(count, len(candidates))} cards to transform",
            cards=candidates,
            resolver=apply_transform,
            allow_skip=False,
            min_count=required,
            max_count=required,
        ):
            return 0
        return apply_transform(candidates[:count])

    def transform_basic_cards(self, count: int, upgrade: int = 0) -> int:
        basics = self.basic_strike_defend_cards()[:count]
        transformed = self.transform_cards(len(basics), cards=basics)
        if upgrade:
            self.upgrade_random_cards(None, upgrade)
        return transformed

    def transform_all_basic_cards(self) -> int:
        basics = self.basic_strike_defend_cards()
        return self.transform_cards(len(basics), cards=basics)

    def transform_specific_cards(self, cards: list[CardInstance], rng: Rng | None = None) -> int:
        return self._transform_selected_cards(list(cards), rng=rng)

    def transform_specific_cards_with_mapping(
        self,
        cards: list[CardInstance],
        mapping: dict[CardId, CardId],
    ) -> int:
        return self._transform_selected_cards_with_mapping(list(cards), mapping)

    def transform_starter_card(self, *, mapping: dict[CardId, CardId] | None = None) -> bool:
        basics = [
            card for card in self.deck
            if (card.rarity == CardRarity.BASIC if mapping is None else card.card_id in mapping)
        ]
        if basics and self.request_deck_choice(
            prompt="Choose a starter card to transform",
            cards=basics,
            resolver=lambda selected: self._transform_selected_cards(selected) if mapping is None else self._transform_selected_cards_with_mapping(selected, mapping),
            allow_skip=True,
        ):
            return True
        if not basics:
            return False
        if mapping is not None:
            return self._transform_selected_cards_with_mapping(basics[:1], mapping) > 0
        return self.transform_cards(1, cards=basics) > 0

    def transform_cards_to(
        self,
        name: str,
        count: int,
        *,
        cards: list[CardInstance] | None = None,
        preserve_upgrades: bool = False,
        preserve_enchantments: bool = False,
        min_count: int | None = None,
    ) -> int:
        card_id = self._coerce_card_id(name)
        if card_id is None:
            return 0
        candidates = list(cards) if cards is not None else list(self.deck)
        required = min_count if min_count is not None else min(count, len(candidates))
        max_count = min(count, len(candidates))
        allow_skip = min_count == 0
        if count > 0 and candidates and self._can_auto_resolve_deck_choice(
            candidates,
            allow_skip=allow_skip,
            min_count=required,
            max_count=max_count,
        ):
            return self._transform_selected_cards_to(
                candidates[:max_count],
                card_id,
                preserve_upgrades=preserve_upgrades,
                preserve_enchantments=preserve_enchantments,
            )
        if count > 0 and candidates and self.request_deck_choice(
            prompt=f"Choose {min(count, len(candidates))} cards to transform into {name}",
            cards=candidates,
            resolver=lambda selected: self._transform_selected_cards_to(
                selected,
                card_id,
                preserve_upgrades=preserve_upgrades,
                preserve_enchantments=preserve_enchantments,
            ),
            allow_skip=allow_skip,
            min_count=required,
            max_count=max_count,
        ):
            return 0
        transformed = 0
        for card in candidates[:count]:
            replacement = create_card(card_id, upgraded=preserve_upgrades and card.upgraded)
            old_enchantments = self._apply_card_replacement(card, replacement)
            if preserve_enchantments:
                for name, amount in old_enchantments.items():
                    if can_enchant_card(card, name):
                        card.add_enchantment(name, amount)
            transformed += 1
        return transformed

    def transform_and_upgrade_cards(
        self,
        count: int,
        *,
        cards: list[CardInstance] | None = None,
        rng: Rng | None = None,
        after_selected: Callable[[], None] | None = None,
    ) -> int:
        candidates = list(cards) if cards is not None else self.transformable_deck_cards()
        required = min(count, len(candidates))

        def apply_transform(selected: list[CardInstance]) -> int:
            transformed = self._transform_and_upgrade_selected(selected, rng=rng)
            if after_selected is not None:
                after_selected()
            return transformed

        if required > 0 and self._can_auto_resolve_deck_choice(
            candidates,
            allow_skip=False,
            min_count=required,
            max_count=required,
        ):
            return apply_transform(candidates[:required])
        if required > 0 and self.request_deck_choice(
            prompt=f"Choose {min(count, len(candidates))} cards to transform and upgrade",
            cards=candidates,
            resolver=apply_transform,
            allow_skip=False,
            min_count=required,
            max_count=required,
        ):
            return 0
        return apply_transform(candidates[:count])

    def _apply_card_replacement(self, card: CardInstance, replacement: CardInstance) -> None:
        original_enchantments = dict(card.enchantments)
        card.card_id = replacement.card_id
        card.cost = replacement.cost
        card.card_type = replacement.card_type
        card.target_type = replacement.target_type
        card.rarity = replacement.rarity
        card.base_damage = replacement.base_damage
        card.base_block = replacement.base_block
        card.upgraded = replacement.upgraded
        card.keywords = replacement.keywords
        card.tags = replacement.tags
        card.can_be_generated_in_combat = replacement.can_be_generated_in_combat
        card.can_be_generated_by_modifiers = replacement.can_be_generated_by_modifiers
        card.has_turn_end_in_hand_effect = replacement.has_turn_end_in_hand_effect
        card.enchantments = dict(replacement.enchantments)
        card.effect_vars = dict(replacement.effect_vars)
        card.original_cost = replacement.original_cost
        return original_enchantments

    def _transform_selected_cards(self, cards: list[CardInstance], rng: Rng | None = None) -> int:
        transformed = 0
        for card in cards:
            replacement = create_transform_card(
                card,
                character_id=self.character_id,
                rng=rng or self.run_state.rng.niche,
                generation_context=None,
                is_multiplayer=len(self.run_state.players) > 1,
            )
            self._apply_card_replacement(card, replacement)
            transformed += 1
        return transformed

    def _transform_selected_cards_to(
        self,
        cards: list[CardInstance],
        card_id: CardId,
        *,
        preserve_upgrades: bool = False,
        preserve_enchantments: bool = False,
    ) -> int:
        transformed = 0
        for card in cards:
            replacement = create_card(card_id, upgraded=preserve_upgrades and card.upgraded)
            old_enchantments = self._apply_card_replacement(card, replacement)
            if preserve_enchantments:
                for name, amount in old_enchantments.items():
                    if can_enchant_card(card, name):
                        card.add_enchantment(name, amount)
            transformed += 1
        return transformed

    def _transform_and_upgrade_selected(self, cards: list[CardInstance], rng: Rng | None = None) -> int:
        transformed = self._transform_selected_cards(cards, rng=rng)
        for card in cards:
            self.upgrade_card_instance(card)
        return transformed

    def _transform_selected_cards_with_mapping(self, cards: list[CardInstance], mapping: dict[CardId, CardId]) -> int:
        transformed = 0
        for card in cards:
            target_id = mapping.get(card.card_id)
            if target_id is None:
                continue
            replacement = create_card(target_id, upgraded=card.upgraded)
            old_enchantments = self._apply_card_replacement(card, replacement)
            for name, amount in old_enchantments.items():
                if can_enchant_card(card, name):
                    card.add_enchantment(name, amount)
            transformed += 1
        return transformed

    def upgrade_random_cards(self, card_type: CardType | None, count: int, rng: Rng | None = None) -> int:
        candidates = [card for card in self.deck if not card.upgraded and (card_type is None or card.card_type == card_type)]
        if not candidates or count <= 0:
            return 0
        selected_rng = rng or self.run_state.rng.rewards
        candidates.sort(key=lambda card: (card.card_id.name, card.upgraded))
        selected_rng.shuffle(candidates)
        upgraded = 0
        for card in candidates[:count]:
            if self.upgrade_card_instance(card) is not None and card.upgraded:
                upgraded += 1
        return upgraded

    def procure_potion(self, potion_id: str) -> bool:
        from sts2_env.potions.base import create_potion, roll_random_potion_model

        for relic in self.get_relic_objects():
            should_procure = getattr(relic, "should_procure_potion", None)
            if callable(should_procure) and should_procure(self) is False:
                return False
        if potion_id == "random":
            model = roll_random_potion_model(
                self.run_state.rng.combat_potion_generation,
                character_id=self.character_id,
                in_combat=False,
            )
            if model is None:
                return False
            return self.add_potion(create_potion(model.potion_id))
        return self.add_potion(create_potion(potion_id))

    def fill_empty_potion_slots(self) -> int:
        filled = 0
        while len(self.held_potions()) < self.max_potion_slots:
            if not self.procure_potion("random"):
                break
            filled += 1
        return filled

    def offer_card_reward(self) -> None:
        from sts2_env.run.reward_objects import CardReward

        self.run_state.pending_rewards.append(
            CardReward(self.player_id, generation_context=None, card_creation_source="other")
        )

    def offer_custom_card_reward(
        self,
        *,
        context: str = "regular",
        option_count: int | None = None,
        cards_to_pick: int = 1,
        skippable: bool = True,
        character_ids: tuple[str, ...] | None = None,
        forced_rarities: tuple[CardRarity, ...] | None = None,
        include_colorless: bool = False,
        use_default_character_pool: bool = True,
        generation_context: str | None = "combat",
        roll_upgrade: bool = True,
        card_creation_source: str | None = None,
        allow_card_pool_modifications: bool = True,
        allow_rarity_modifications: bool = True,
        has_custom_card_pool: bool = False,
        custom_card_ids: tuple[CardId, ...] | None = None,
        card_pool_rarity_filter: CardRarity | None = None,
        use_uniform_noncombat_odds: bool = False,
        cards: list[CardInstance] | None = None,
    ) -> None:
        from sts2_env.run.reward_objects import CardReward

        reward = CardReward(
            self.player_id,
            context=context,
            option_count=option_count,
            cards_to_pick=cards_to_pick,
            skippable=skippable,
            character_ids=character_ids,
            forced_rarities=forced_rarities,
            include_colorless=include_colorless,
            use_default_character_pool=use_default_character_pool,
            generation_context=generation_context,
            roll_upgrade=roll_upgrade,
            card_creation_source=card_creation_source,
            allow_card_pool_modifications=allow_card_pool_modifications,
            allow_rarity_modifications=allow_rarity_modifications,
            has_custom_card_pool=has_custom_card_pool,
            custom_card_ids=custom_card_ids,
            card_pool_rarity_filter=card_pool_rarity_filter,
            use_uniform_noncombat_odds=use_uniform_noncombat_odds,
            cards=cards,
        )
        self.run_state.pending_rewards.append(reward)

    def offer_colorless_cards(self, count: int) -> None:
        self.offer_custom_card_reward(
            option_count=count,
            include_colorless=True,
            use_default_character_pool=False,
            generation_context=None,
            card_creation_source="other",
            has_custom_card_pool=True,
        )

    def offer_multiplayer_cards(self, count: int) -> None:
        custom_card_ids = []
        seen_ids = set()
        for card_id in eligible_registered_cards(card_pool=CardPoolId.COLORLESS, generation_context=None):
            if is_multiplayer_only_card(card_id):
                custom_card_ids.append(card_id)
                seen_ids.add(card_id)
        for card_id in get_character(self.character_id).card_pool:
            if is_multiplayer_only_card(card_id) and card_id not in seen_ids:
                custom_card_ids.append(card_id)
                seen_ids.add(card_id)
        self.offer_custom_card_reward(
            option_count=count,
            character_ids=(),
            include_colorless=False,
            use_default_character_pool=False,
            generation_context=None,
            card_creation_source="other",
            has_custom_card_pool=True,
            custom_card_ids=tuple(custom_card_ids),
        )

    def offer_relic_rewards(
        self,
        count: int,
        *,
        rarities: tuple[Any, ...] | None = None,
        rng_stream: str = "rewards",
    ) -> None:
        from sts2_env.run.reward_objects import RelicReward

        rarity_sequence = tuple(rarities or ())
        for index in range(count):
            rarity = rarity_sequence[index] if index < len(rarity_sequence) else None
            self.run_state.pending_rewards.append(
                RelicReward(self.player_id, rarity=rarity, rng_stream=rng_stream)
            )

    def offer_specific_relic_rewards(
        self,
        relic_ids: list[str],
        *,
        is_wax: bool = False,
    ) -> None:
        from sts2_env.run.reward_objects import RelicReward

        for relic_id in relic_ids:
            self.run_state.pending_rewards.append(
                RelicReward(self.player_id, relic_id=relic_id, is_wax=is_wax)
            )

    def offer_add_cards_reward(self, cards: list[CardInstance]) -> None:
        from sts2_env.run.reward_objects import AddCardsReward

        self.run_state.pending_rewards.append(AddCardsReward(self.player_id, cards))

    def offer_obtain_relics_reward(
        self,
        count: int,
        *,
        rarities: tuple[RelicRarity | None, ...] | None = None,
        relic_ids: tuple[str, ...] | list[str] | None = None,
        rng_stream: str = "rewards",
    ) -> None:
        from sts2_env.run.reward_objects import ObtainRelicsReward

        self.run_state.pending_rewards.append(
            ObtainRelicsReward(
                self.player_id,
                count=count,
                rarities=rarities,
                relic_ids=relic_ids,
                rng_stream=rng_stream,
            )
        )

    def offer_remove_card_reward(self, count: int = 1, *, cards: list[CardInstance] | None = None) -> None:
        from sts2_env.run.reward_objects import RemoveCardReward

        self.run_state.pending_rewards.append(RemoveCardReward(self.player_id, count=count, cards=cards))

    def offer_upgrade_cards_reward(self, count: int = 1, *, cards: list[CardInstance] | None = None) -> None:
        from sts2_env.run.reward_objects import UpgradeCardsReward

        self.run_state.pending_rewards.append(UpgradeCardsReward(self.player_id, count=count, cards=cards))

    def offer_transform_cards_reward(
        self,
        count: int = 1,
        *,
        upgrade: bool = False,
        cards: list[CardInstance] | None = None,
        mapping: dict[CardId, CardId] | None = None,
        rng_stream: str = "niche",
    ) -> None:
        from sts2_env.run.reward_objects import TransformCardsReward

        self.run_state.pending_rewards.append(
            TransformCardsReward(
                self.player_id,
                count=count,
                upgrade=upgrade,
                cards=cards,
                mapping=mapping,
                rng_stream=rng_stream,
            )
        )

    def offer_duplicate_card_reward(self, count: int = 1, *, cards: list[CardInstance] | None = None) -> None:
        from sts2_env.run.reward_objects import DuplicateCardReward

        self.run_state.pending_rewards.append(DuplicateCardReward(self.player_id, count=count, cards=cards))

    def offer_enchant_cards_reward(
        self,
        enchantment: str,
        amount: int = 1,
        count: int = 1,
        *,
        cards: list[CardInstance] | None = None,
        min_count: int | None = None,
    ) -> None:
        from sts2_env.run.reward_objects import EnchantCardsReward

        self.run_state.pending_rewards.append(
            EnchantCardsReward(
                self.player_id,
                enchantment=enchantment,
                amount=amount,
                count=count,
                cards=cards,
                min_count=min_count,
            )
        )

    def offer_potion_reward(self) -> None:
        from sts2_env.run.reward_objects import PotionReward

        self.run_state.pending_rewards.append(PotionReward(self.player_id))

    def offer_potions(self, count: int) -> None:
        for _ in range(count):
            self.offer_potion_reward()

    def offer_card_bundles(self) -> None:
        from sts2_env.run.reward_objects import CardBundlesReward

        common_ids = self._card_bundle_candidate_ids(CardRarity.COMMON)
        uncommon_ids = self._card_bundle_candidate_ids(CardRarity.UNCOMMON)
        used_ids: set[CardId] = set()
        bundles: list[list[CardInstance]] = []
        for _ in range(SCROLL_BOXES_BUNDLE_COUNT):
            if (
                self.character_id == CardPoolId.DEFECT.value
                and self.run_state.rng.rewards.next_int_exclusive(0, SCROLL_BOXES_CLAW_BUNDLE_ROLL_BOUND)
                < SCROLL_BOXES_CLAW_BUNDLE_CHANCE_PERCENT
            ):
                bundles.append([create_card(CardId.CLAW) for _ in range(SCROLL_BOXES_CLAW_CARDS_PER_BUNDLE)])
                continue
            bundle: list[CardInstance] = []
            available_common = [card_id for card_id in common_ids if card_id not in used_ids]
            for _ in range(SCROLL_BOXES_COMMON_CARDS_PER_BUNDLE):
                card_id = self.run_state.rng.rewards.choice(available_common)
                bundle.append(create_card(card_id))
                used_ids.add(card_id)
                available_common.remove(card_id)
            available_uncommon = [card_id for card_id in uncommon_ids if card_id not in used_ids]
            for _ in range(SCROLL_BOXES_UNCOMMON_CARDS_PER_BUNDLE):
                card_id = self.run_state.rng.rewards.choice(available_uncommon)
                bundle.append(create_card(card_id))
                used_ids.add(card_id)
                available_uncommon.remove(card_id)
            bundles.append(bundle)
        self.run_state.pending_rewards.append(CardBundlesReward(self.player_id, bundles))

    def _card_bundle_candidate_ids(self, rarity: CardRarity) -> list[CardId]:
        options = CardRewardGenerationOptions(
            forced_rarities=(rarity,),
            card_creation_source=CARD_CREATION_SOURCE_OTHER,
            generation_context=None,
            roll_upgrade=False,
            allow_rarity_modifications=False,
            card_pool_rarity_filter=rarity,
        )
        for relic in self.get_relic_objects():
            options = relic.modify_card_reward_creation_options(
                self,
                options,
                None,
                None,
                self.run_state,
            )
        for modifier in self.run_state.modifiers:
            options = modifier.modify_card_reward_creation_options(
                self,
                options,
                None,
                None,
                self.run_state,
            )
        from sts2_env.run.rewards import card_reward_candidate_ids

        return list(
            card_reward_candidate_ids(
                self.run_state,
                options,
                default_character_id=self.character_id,
                generation_context=None,
            )
        )

    def roll_relic_reward_id(
        self,
        *,
        rarity: Any | None = None,
        rng_stream: str = "rewards",
        excluded_relic_ids: set[str] | None = None,
    ) -> str | None:
        return self.pull_next_relic_reward_id(
            rarity=rarity,
            rng_stream=rng_stream,
            excluded_relic_ids=excluded_relic_ids,
        )

    def obtain_random_relics(self, count: int) -> int:
        obtained = 0
        rolled: set[str] = set()
        for _ in range(count):
            relic_id = self.roll_relic_reward_id(excluded_relic_ids=rolled)
            if relic_id is None:
                continue
            rolled.add(relic_id)
            if self.obtain_relic(relic_id):
                obtained += 1
        return obtained

    def upgrade_starter_relic(
        self,
        starter_relic_id: str | None = None,
        upgraded_relic_id: str | None = None,
    ) -> bool:
        mapping = {
            "BURNING_BLOOD": "BLACK_BLOOD",
            "RING_OF_THE_SNAKE": "RING_OF_THE_DRAKE",
            "CRACKED_CORE": "INFUSED_CORE",
            "BOUND_PHYLACTERY": "PHYLACTERY_UNBOUND",
            "DIVINE_RIGHT": "DIVINE_DESTINY",
        }
        for i, relic_id in enumerate(self.relics):
            if starter_relic_id is not None and relic_id != starter_relic_id:
                continue
            upgraded = upgraded_relic_id or mapping.get(relic_id)
            if upgraded is None:
                continue
            self.relics[i] = upgraded
            if i < len(self.relic_objects):
                from sts2_env.relics.registry import create_relic_by_name

                self.relic_objects[i] = create_relic_by_name(upgraded)
            return True
        return False

    def obtain_relic_with_setup(
        self,
        relic_id: str,
        *,
        setup_attrs: dict[str, object] | None = None,
        is_wax: bool = False,
    ) -> bool:
        from sts2_env.relics.registry import create_relic_by_name

        try:
            relic = create_relic_by_name(relic_id)
        except KeyError:
            relic = None
        canonical_relic_id = relic.relic_id.name if relic is not None else relic_id
        if relic is None and relic_id in self.relics:
            return False
        if relic is not None and not relic.is_stackable:
            for owned_relic_id in self.relics:
                try:
                    if create_relic_by_name(owned_relic_id).relic_id == relic.relic_id:
                        return False
                except KeyError:
                    if owned_relic_id == relic_id:
                        return False
        self.remove_relic_from_grab_bag(canonical_relic_id)
        if canonical_relic_id != relic_id:
            self.remove_relic_from_grab_bag(relic_id)
        self.relics.append(canonical_relic_id)
        if relic is None:
            return True
        for key, value in (setup_attrs or {}).items():
            setattr(relic, key, value)
        if is_wax:
            setattr(relic, "is_wax", True)
            setattr(relic, "is_melted", False)
            relic.enabled = True
        self.relic_objects.append(relic)
        if self.run_state is not None:
            for reward in list(self.run_state.active_card_rewards):
                if reward.player_id != self.player_id:
                    continue
                reward.on_relic_obtained(relic, self.run_state)
        relic.after_obtained(self)
        return True

    def obtain_relic(self, relic_id: str) -> bool:
        return self.obtain_relic_with_setup(relic_id)

    def transform_relic(self, current_relic: Any, new_relic_id: Any) -> bool:
        from sts2_env.relics.registry import create_relic_by_name

        current_name = getattr(getattr(current_relic, "relic_id", None), "name", str(current_relic))
        new_name = getattr(new_relic_id, "name", str(new_relic_id))
        try:
            index = self.relics.index(current_name)
        except ValueError:
            return False
        self.relics[index] = new_name
        if index < len(self.relic_objects):
            self.relic_objects[index] = create_relic_by_name(new_name)
        else:
            self._ensure_relic_objects()
        return True

    @property
    def is_dead(self) -> bool:
        return self.current_hp <= 0


class RunRngSet:
    """Separate seeded RNG streams for determinism."""

    def __init__(self, master_seed: int):
        self.seed = deterministic_hash_code(str(master_seed))
        player_seed = self.seed + 1
        self.map_rngs: dict[int, Rng] = {}
        self.up_front = Rng(self.seed, "up_front")
        self.shuffle = Rng(self.seed, "shuffle")
        self.unknown_map_point = Rng(self.seed, "unknown_map_point")
        self.combat_card_generation = Rng(self.seed, "combat_card_generation")
        self.combat_potion_generation = Rng(self.seed, "combat_potion_generation")
        self.combat_card_selection = Rng(self.seed, "combat_card_selection")
        self.combat_energy_costs = Rng(self.seed, "combat_energy_costs")
        self.combat_targets = Rng(self.seed, "combat_targets")
        self.monster_ai = Rng(self.seed, "monster_ai")
        self.niche = Rng(self.seed, "niche")
        self.combat_orbs = Rng(self.seed, "combat_orbs")
        self.treasure_room = Rng(self.seed, "treasure_room_relics")
        self.combat_potion = self.combat_potion_generation
        self.rewards = Rng(player_seed, "rewards")
        self.shops = Rng(player_seed, "shops")
        self.transformations = Rng(player_seed, "transformations")

    def get_map_rng(self, act_index: int) -> Rng:
        if act_index not in self.map_rngs:
            self.map_rngs[act_index] = Rng(self.seed, f"act_{act_index + 1}_map")
        return self.map_rngs[act_index]


class _EncounterGrabBag:
    """C# MegaCrit.Sts2.Core.Helpers.GrabBag with the uniform weights used by
    ActModel.GenerateRooms (every Add uses weight 1.0): draws without
    replacement until empty, then the caller refills it."""

    def __init__(self, entries: list):
        self._entries = list(entries)

    def any(self) -> bool:
        return bool(self._entries)

    def grab_and_remove(self, rng: Rng, predicate=None):
        """C# GrabBag.GrabAndRemove: rejection-samples a weighted index until
        the predicate matches; returns None when no entry can match."""
        if predicate is not None and not any(predicate(e) for e in self._entries):
            return None
        if not self._entries:
            return None
        while True:
            index = rng.next_int(0, len(self._entries) - 1)
            entry = self._entries[index]
            if predicate is None or predicate(entry):
                self._entries.pop(index)
                return entry


class ActRoomSet:
    """Per-act encounter queues: the encounter half of C#
    MegaCrit.Sts2.Core.Rooms.RoomSet.

    normal_encounters has one entry per potential normal fight (the first
    ``num_weak`` are from the act's weak pool, the rest from its regular
    pool); elite_encounters holds 15 entries; boss is rolled once per act.
    Rooms pull ``list[visited % len]`` and visiting increments the counter
    (C# RoomSet.NextNormalEncounter / NextEliteEncounter / NextBossEncounter /
    MarkVisited).
    """

    def __init__(
        self,
        normal_encounters: list,
        elite_encounters: list,
        boss=None,
        second_boss=None,
    ):
        self.normal_encounters = normal_encounters
        self.elite_encounters = elite_encounters
        self.boss = boss
        self.second_boss = second_boss
        self.normal_encounters_visited = 0
        self.elite_encounters_visited = 0
        self.boss_encounters_visited = 0

    @property
    def next_normal_encounter(self):
        return self.normal_encounters[
            self.normal_encounters_visited % len(self.normal_encounters)
        ]

    @property
    def next_elite_encounter(self):
        return self.elite_encounters[
            self.elite_encounters_visited % len(self.elite_encounters)
        ]

    @property
    def next_boss_encounter(self):
        if self.boss_encounters_visited != 0 and self.second_boss is not None:
            return self.second_boss
        return self.boss

    def mark_visited(self, room_type) -> None:
        """C# RoomSet.MarkVisited."""
        from sts2_env.core.enums import RoomType

        if room_type == RoomType.MONSTER:
            self.normal_encounters_visited += 1
        elif room_type == RoomType.ELITE:
            self.elite_encounters_visited += 1
        elif room_type == RoomType.BOSS:
            self.boss_encounters_visited += 1


class RunState:
    """Complete run state, persisted across combats and rooms."""

    def __init__(
        self,
        seed: int = 0,
        ascension_level: int = 0,
        character_id: str = "Ironclad",
    ):
        self.seed = seed
        self.ascension_level = ascension_level
        self.rng = RunRngSet(seed)

        # Player
        self.player = PlayerState(character_id=character_id)
        self.player.run_state = self
        self.player.base_orb_slot_count = get_character(character_id).base_orb_slots
        self.players: list[PlayerState] = [self.player]

        # Act / map state
        self.acts: list[ActConfig] = [act.to_mutable() for act in ALL_ACTS]
        self.current_act_index: int = 0
        self.map: ActMap | None = None
        self.visited_map_coords: list[MapCoord] = []
        self.map_point_history: list[MapPointHistoryEntry] = []
        self.act_floor: int = 0
        self.total_floor: int = 0
        self._rooms_generated = False
        self.act_rooms: dict[int, ActRoomSet] = {}

        # Event tracking
        self.visited_event_ids: set[str] = set()
        self.extra_fields: dict[str, Any] = {}
        self.modifiers: list[Any] = []

        # Primary-player compatibility aliases.
        self.relics = self.player.relics
        self.relic_grab_bag = self.player.relic_grab_bag

        # Odds systems
        self.unknown_odds = UnknownMapPointOdds()
        self.card_rarity_odds = CardRarityOdds(ascension_level)
        self.potion_reward_odds = PotionRewardOdds()
        self.pending_rewards: list[Any] = []
        self.active_card_rewards: list[Any] = []
        self.pending_choice: PendingCardChoice | None = None
        self.enable_deck_choice_requests: bool = False
        self.defer_followup_rewards: bool = False

        # Run state flags
        self.is_over: bool = False
        self.player_won: bool = False
        self.has_double_boss: bool = False

    @property
    def current_act(self) -> ActConfig:
        return self.acts[self.current_act_index]

    def add_player(self, player: PlayerState) -> PlayerState:
        if any(existing.player_id == player.player_id for existing in self.players):
            raise ValueError(f"Duplicate player_id {player.player_id}")
        player.run_state = self
        self.players.append(player)
        return player

    def get_player(self, player_id: int) -> PlayerState:
        for player in self.players:
            if player.player_id == player_id:
                return player
        raise KeyError(f"Unknown player_id {player_id}")

    def resolve_pending_choice(self, choice_index: int | None) -> bool:
        choice = self.pending_choice
        if choice is None:
            return False

        if choice.is_multi:
            if choice_index is None:
                if not choice.can_confirm():
                    return False
                selected_cards = choice.selected_cards
                self.pending_choice = None
                choice.resolver(selected_cards)
                return True
            return choice.toggle(choice_index)

        selected_cards: list[CardInstance] = []
        if choice_index is None:
            if not choice.allow_skip:
                return False
        else:
            if choice_index < 0 or choice_index >= len(choice.options):
                return False
            selected_cards = [choice.options[choice_index].card]
        self.pending_choice = None
        choice.resolver(selected_cards)
        return True

    def initialize_run(self) -> None:
        """Set up a new run: apply ascension, generate first map."""
        self._apply_ascension_effects()
        for modifier in self.modifiers:
            on_run_created = getattr(modifier, "on_run_created", None)
            if callable(on_run_created):
                on_run_created(self)
        for player in self.players:
            player.populate_relic_grab_bag()
        for modifier in self.modifiers:
            modifier.after_relic_grab_bags_populated(self)
        self._generate_rooms()
        self.generate_map()
        self._fire_after_act_entered()

    def _generate_rooms(self) -> None:
        if self._rooms_generated:
            return
        for act in self.acts:
            self.rng.up_front.shuffle(act.event_ids)
            act.events_visited = 0
        self._generate_encounter_rooms()
        self._rooms_generated = True

    def _generate_encounter_rooms(self) -> None:
        """Build each act's encounter queues (C# ActModel.GenerateRooms driven
        by RunManager.GenerateRooms with State.Rng.UpFront).

        - Normal queue: the first ``NumberOfWeakEncounters`` fights come from
          the weak pool, the rest from the regular pool; both via a refill
          shuffle-bag (C# GrabBag, all weights 1.0) with the
          AddWithoutRepeatingTags rule (no shared tags with / equality to the
          previous pick, falling back to any entry).
        - Elite queue: 15 entries from the elite pool via the same bag.
        - Boss: one rng.NextItem pick per act.
        - Double boss (A10): the last act also rolls a distinct second boss.
        """
        from sts2_env.encounters import get_act_encounter_pools, shares_tags_with

        def add_without_repeating_tags(
            chosen: list, bag: _EncounterGrabBag, rng: Rng
        ) -> None:
            previous = chosen[-1] if chosen else None
            candidate = bag.grab_and_remove(
                rng,
                predicate=lambda e: not shares_tags_with(e, previous) and e != previous,
            )
            if candidate is None:
                candidate = bag.grab_and_remove(rng)
            if candidate is not None:
                chosen.append(candidate)

        for act_index, act in enumerate(self.acts):
            pools = get_act_encounter_pools(act_index)
            rng = self.rng.up_front
            # C#: _mapLength = actModel.GetNumberOfRooms(isMultiplayer) + 1;
            # GenerateRooms fills one normal-queue entry per room
            # (multiplayer removes one room).
            num_rooms = act.num_rooms - (1 if len(self.players) > 1 else 0)
            num_weak = act.num_weak_encounters

            normal: list = []
            weak_bag = _EncounterGrabBag(pools["weak"])
            for _ in range(num_weak):
                if not weak_bag.any():
                    weak_bag = _EncounterGrabBag(pools["weak"])
                add_without_repeating_tags(normal, weak_bag, rng)
            regular_bag = _EncounterGrabBag(pools["normal"])
            for _ in range(num_weak, num_rooms):
                if not regular_bag.any():
                    regular_bag = _EncounterGrabBag(pools["normal"])
                add_without_repeating_tags(normal, regular_bag, rng)

            elite: list = []
            elite_bag = _EncounterGrabBag(pools["elite"])
            for _ in range(15):
                if not elite_bag.any():
                    elite_bag = _EncounterGrabBag(pools["elite"])
                add_without_repeating_tags(elite, elite_bag, rng)

            boss = rng.choice(pools["boss"]) if pools["boss"] else None

            second_boss = None
            if act_index == len(self.acts) - 1 and self.has_double_boss and pools["boss"]:
                others = [e for e in pools["boss"] if e != boss]
                if others:
                    second_boss = rng.choice(others)

            self.act_rooms[act_index] = ActRoomSet(
                normal_encounters=normal,
                elite_encounters=elite,
                boss=boss,
                second_boss=second_boss,
            )

    def get_act_room_set(self, act_index: int) -> ActRoomSet:
        room_set = self.act_rooms.get(act_index)
        if room_set is None:
            self._generate_encounter_rooms()
            room_set = self.act_rooms[act_index]
        return room_set

    def _apply_ascension_effects(self) -> None:
        """Apply all ascension effects matching AscensionManager.ApplyEffectsTo
        and the runtime checks throughout the C# source.

        Ascension levels (enum order):
          0 = None
          1 = SwarmingElites   — 1.6x elite map points (handled in map gen)
          2 = WearyTraveler    — Neow heals 80% of missing HP instead of 100%
          3 = Poverty          — Starting gold × 0.75
          4 = TightBelt        — -1 max potion slot
          5 = AscendersBane    — Add Ascender's Bane curse to deck
          6 = Inflation         — shop card-removal base cost 100 / +50 per
                                  removal (handled in run/shop.py; C#
                                  MerchantCardRemovalEntry)
          7 = Scarcity         — Card upgrade rate halved (0.125 instead of 0.25)
          8 = ToughEnemies     — Higher monster HP (handled per-monster)
          9 = DeadlyEnemies    — Higher monster damage (handled per-monster)
         10 = DoubleBoss       — Two boss fights per act
        """
        asc = self.ascension_level

        # A1: SwarmingElites — 1.6x elites on map (handled in map generator)
        # A2: WearyTraveler — Neow heal reduction (handled in event)

        # A3: Poverty — gold multiplier 0.75x
        if asc >= 3:
            self.player.gold = round(self.player.gold * 0.75)

        # A4: TightBelt — 1 fewer potion slot
        if asc >= 4:
            self.player.max_potion_slots = max(0, self.player.max_potion_slots - 1)

        # A5: AscendersBane — add curse to starting deck
        if asc >= 5:
            from sts2_env.cards.base import CardInstance
            from sts2_env.core.enums import CardId, CardType, TargetType, CardRarity as CR
            curse = CardInstance(
                card_id=CardId.ASCENDERS_BANE,
                cost=0,
                card_type=CardType.CURSE,
                target_type=TargetType.NONE,
                rarity=CR.CURSE,
                keywords=frozenset({"unplayable", "ethereal"}),
                can_be_generated_by_modifiers=False,
            )
            self.player.add_card_instance_to_deck(curse)

        # A7: Scarcity — card upgrade scaling halved (stored for rewards.py)
        # The actual check is: scaling = 0.125 if asc >= 7 else 0.25
        # Already handled in run/rewards.py roll_for_upgrade()

        # A8: ToughEnemies — higher monster HP (handled per-monster via
        #   AscensionHelper.GetValueIfAscension; our monsters use fixed values
        #   for simplicity since we don't have runtime ascension checks yet)

        # A9: DeadlyEnemies — higher monster damage (same as A8)

        # A10: DoubleBoss — two boss fights per act
        self.has_double_boss = asc >= 10

    def generate_map(self) -> None:
        """Generate the map for the current act."""
        act = self.current_act
        map_rng = self.rng.get_map_rng(self.current_act_index)
        act_map = generate_act_map(
            num_rooms=act.num_rooms,
            rng=map_rng,
            ascension_level=self.ascension_level,
            act_index=self.current_act_index,
        )
        for player in self.players:
            for card in player.deck:
                act_map = card.modify_generated_map(self, act_map, self.current_act_index)
        for player in self.players:
            for relic in player.get_relic_objects():
                act_map = relic.modify_generated_map(player, self, act_map, self.current_act_index)
        for player in self.players:
            for modifier in self.modifiers:
                act_map = modifier.modify_generated_map(player, self, act_map, self.current_act_index)
        for player in self.players:
            for card in player.deck:
                act_map = card.modify_generated_map_late(self, act_map, self.current_act_index)
        for player in self.players:
            for relic in player.get_relic_objects():
                act_map = relic.modify_generated_map_late(player, self, act_map, self.current_act_index)
        for player in self.players:
            for card in player.deck:
                card.after_map_generated(self, act_map, self.current_act_index)
        self.map = act_map

    def enter_act(self, act_index: int) -> None:
        """Transition to a new act."""
        self.current_act_index = act_index
        self.visited_map_coords.clear()
        self.act_floor = 0
        self.unknown_odds.reset_to_base()
        self.generate_map()
        self._fire_after_act_entered()

    def _fire_after_act_entered(self) -> None:
        for modifier in self.modifiers:
            modifier.after_act_entered(self)

    def enter_next_act(self) -> bool:
        """Move to the next act. Returns False if run is over (won)."""
        if self.current_act_index >= len(self.acts) - 1:
            self.is_over = True
            self.player_won = True
            return False
        self.enter_act(self.current_act_index + 1)
        return True

    def add_visited_coord(self, coord: MapCoord, room_type: RoomType | None = None) -> bool:
        if coord in self.visited_map_coords:
            return False
        self.visited_map_coords.append(coord)
        self.act_floor = coord.row + 1
        self.total_floor += 1
        if room_type is not None:
            map_point_type = self._map_point_type_for_room(room_type)
            if self.map is not None:
                point = self.map.get_point(coord)
                if point is not None:
                    map_point_type = point.point_type
            self.append_to_map_point_history(map_point_type, room_type)
        return True

    def append_to_map_point_history(self, map_point_type: MapPointType, room_type: RoomType) -> None:
        self.map_point_history.append(MapPointHistoryEntry(map_point_type=map_point_type, room_type=room_type))

    def count_map_point_history_entries(
        self,
        *,
        room_type: RoomType | None = None,
        map_point_type: MapPointType | None = None,
    ) -> int:
        return sum(
            1
            for entry in self.map_point_history
            if (room_type is None or entry.room_type == room_type)
            and (map_point_type is None or entry.map_point_type == map_point_type)
        )

    @staticmethod
    def _map_point_type_for_room(room_type: RoomType) -> MapPointType:
        return {
            RoomType.MONSTER: MapPointType.MONSTER,
            RoomType.ELITE: MapPointType.ELITE,
            RoomType.BOSS: MapPointType.BOSS,
            RoomType.SHOP: MapPointType.SHOP,
            RoomType.REST_SITE: MapPointType.REST_SITE,
            RoomType.TREASURE: MapPointType.TREASURE,
            RoomType.EVENT: MapPointType.ANCIENT,
        }[room_type]

    def get_available_next_coords(self) -> list[MapCoord]:
        """Get coordinates the player can move to from current position."""
        if self.map is None:
            return []

        if not self.visited_map_coords:
            # At start: can go to any first-row room
            if self.map.start_point:
                return [c.coord for c in self.map.start_point.children]
            return [p.coord for p in self.map.get_row(1)]

        last_coord = self.visited_map_coords[-1]
        last_point = self.map.get_point(last_coord)
        if last_point is None:
            return []
        if any(getattr(modifier, "modifier_id", None) == "flight" for modifier in self.modifiers):
            return [point.coord for point in self.map.get_row(last_coord.row + 1)]
        return [c.coord for c in last_point.children]

    def resolve_room_type(self, point_type: MapPointType, blacklist: set[RoomType] | None = None) -> RoomType:
        """Convert a MapPointType to a RoomType, rolling for Unknown rooms."""
        blacklist = set(blacklist or ())
        tutorial_room_type = self._tutorial_room_type_for(point_type)
        if tutorial_room_type is not None:
            return tutorial_room_type
        mapping = {
            MapPointType.SHOP: RoomType.SHOP,
            MapPointType.TREASURE: RoomType.TREASURE,
            MapPointType.REST_SITE: RoomType.REST_SITE,
            MapPointType.MONSTER: RoomType.MONSTER,
            MapPointType.ELITE: RoomType.ELITE,
            MapPointType.BOSS: RoomType.BOSS,
            MapPointType.ANCIENT: RoomType.EVENT,
        }
        if point_type == MapPointType.UNKNOWN:
            return self.unknown_odds.roll(self.rng.unknown_map_point, self, blacklist=blacklist)
        return mapping.get(point_type, RoomType.MONSTER)

    def build_room_type_blacklist(self, next_map_points: list[MapPoint] | None = None) -> set[RoomType]:
        blacklist: set[RoomType] = set()
        if self.map_point_history and self.map_point_history[-1].room_type == RoomType.SHOP:
            blacklist.add(RoomType.SHOP)
        if next_map_points and all(point.point_type == MapPointType.SHOP for point in next_map_points):
            blacklist.add(RoomType.SHOP)
        return blacklist

    def _tutorial_room_type_for(self, point_type: MapPointType) -> RoomType | None:
        if len(self.players) > 1:
            return None
        if point_type is not MapPointType.UNASSIGNED:
            return None
        if self.player.unlock_state.get(UNLOCK_STATE_NUMBER_OF_RUNS_KEY, FIRST_RUN_COUNT) > FIRST_RUN_COUNT:
            return None
        if self.count_map_point_history_entries(map_point_type=MapPointType.UNASSIGNED) > 0:
            return None
        return RoomType.EVENT

    def win_run(self) -> None:
        self.is_over = True
        self.player_won = True

    def lose_run(self) -> None:
        self.is_over = True
        self.player_won = False
