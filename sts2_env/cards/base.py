"""Card instance dataclass and factory."""

from __future__ import annotations

from collections.abc import Iterable
from dataclasses import dataclass, field
from functools import lru_cache

from sts2_env.core.card_pools import (
    CHARACTER_CARD_POOLS_BY_ID,
    SHARED_CARD_POOLS_BY_ID,
    CardPoolId,
)
from sts2_env.core.enums import CardId, CardTag, CardType, TargetType, CardRarity, OrbEvokeType


_TAG_ALIASES = {
    "strike": CardTag.STRIKE,
    "defend": CardTag.DEFEND,
    "minion": CardTag.MINION,
    "osty_attack": CardTag.OSTY_ATTACK,
    "shiv": CardTag.SHIV,
}

_DEFAULT_VISUAL_CARD_POOL_BY_RARITY = {
    CardRarity.CURSE: CardPoolId.CURSE,
    CardRarity.EVENT: CardPoolId.EVENT,
    CardRarity.QUEST: CardPoolId.QUEST,
    CardRarity.STATUS: CardPoolId.STATUS,
}

_DEFAULT_VISUAL_CARD_POOL_BY_CARD_ID = {
    card_id: pool_id
    for pool_id, card_ids in {
        **CHARACTER_CARD_POOLS_BY_ID,
        **SHARED_CARD_POOLS_BY_ID,
    }.items()
    for card_id in card_ids
}

@lru_cache(maxsize=None)
def _reference_card_base_values(card_id: CardId, upgraded: bool) -> tuple[int | None, int | None]:
    from sts2_env.cards.factory import create_card

    reference = create_card(card_id, upgraded=upgraded)
    return reference.base_damage, reference.effect_vars.get("block", reference.base_block)


def increase_base_damage(card: CardInstance, amount: int) -> None:
    card.base_damage = (card.base_damage or 0) + amount
    if "damage" in card.effect_vars:
        card.effect_vars["damage"] += amount


def increase_base_block(card: CardInstance, amount: int) -> None:
    card.base_block = (card.base_block or 0) + amount
    if "block" in card.effect_vars:
        card.effect_vars["block"] += amount


def capture_self_mutating_card_progress(card: CardInstance) -> dict[str, int]:
    from sts2_env.cards.registry import card_preserves_self_mutating_block, card_preserves_self_mutating_damage

    progress: dict[str, int] = {}
    reference_damage, reference_block = _reference_card_base_values(card.card_id, card.upgraded)
    if card_preserves_self_mutating_damage(card.card_id) and card.base_damage is not None and reference_damage is not None:
        progress["damage"] = card.base_damage - reference_damage
    if card_preserves_self_mutating_block(card.card_id) and reference_block is not None:
        current_block = card.effect_vars.get("block", card.base_block or 0)
        progress["block"] = current_block - reference_block
    return progress


def restore_self_mutating_card_progress(card: CardInstance, progress: dict[str, int]) -> None:
    damage_bonus = progress.get("damage", 0)
    if damage_bonus:
        increase_base_damage(card, damage_bonus)
    block_bonus = progress.get("block", 0)
    if block_bonus:
        increase_base_block(card, block_bonus)


def reference_has_turn_end_in_hand_effect(card_id: CardId) -> bool:
    from sts2_env.cards.reference_static_metadata import reference_metadata_by_card_id

    metadata = reference_metadata_by_card_id().get(card_id)
    return metadata.has_turn_end_in_hand_effect if metadata is not None else False


def reference_gains_block(card_id: CardId) -> bool:
    from sts2_env.cards.reference_static_metadata import reference_metadata_by_card_id

    metadata = reference_metadata_by_card_id().get(card_id)
    return metadata.gains_block if metadata is not None else False


def reference_orb_evoke_type(card_id: CardId) -> OrbEvokeType:
    from sts2_env.cards.reference_static_metadata import reference_metadata_by_card_id

    metadata = reference_metadata_by_card_id().get(card_id)
    return metadata.orb_evoke_type if metadata is not None else OrbEvokeType.NONE


def reference_visual_card_pool(card_id: CardId) -> CardPoolId | None:
    from sts2_env.cards.reference_static_metadata import reference_metadata_by_card_id

    metadata = reference_metadata_by_card_id().get(card_id)
    return metadata.visual_card_pool if metadata is not None else None


def reference_should_show_in_card_library(card_id: CardId) -> bool:
    from sts2_env.cards.reference_static_metadata import reference_metadata_by_card_id

    metadata = reference_metadata_by_card_id().get(card_id)
    return metadata.should_show_in_card_library if metadata is not None else True


def reference_has_custom_playability(card_id: CardId) -> bool:
    from sts2_env.cards.reference_static_metadata import reference_metadata_by_card_id

    metadata = reference_metadata_by_card_id().get(card_id)
    return metadata.has_custom_playability if metadata is not None else False


def reference_has_custom_should_play(card_id: CardId) -> bool:
    from sts2_env.cards.reference_static_metadata import reference_metadata_by_card_id

    metadata = reference_metadata_by_card_id().get(card_id)
    return metadata.has_custom_should_play if metadata is not None else False


def reference_has_custom_card_type(card_id: CardId) -> bool:
    from sts2_env.cards.reference_static_metadata import reference_metadata_by_card_id

    metadata = reference_metadata_by_card_id().get(card_id)
    return metadata.has_custom_card_type if metadata is not None else False


def reference_has_custom_target_type(card_id: CardId) -> bool:
    from sts2_env.cards.reference_static_metadata import reference_metadata_by_card_id

    metadata = reference_metadata_by_card_id().get(card_id)
    return metadata.has_custom_target_type if metadata is not None else False


def reference_canonical_tags(card_id: CardId) -> frozenset[CardTag]:
    from sts2_env.cards.reference_static_metadata import reference_metadata_by_card_id

    metadata = reference_metadata_by_card_id().get(card_id)
    return metadata.tags if metadata is not None else frozenset()


@dataclass
class CardInstance:
    """A single card instance in combat."""

    card_id: CardId
    cost: int
    card_type: CardType
    target_type: TargetType
    rarity: CardRarity = CardRarity.BASIC
    base_damage: int | None = None
    base_block: int | None = None
    upgraded: bool = False
    keywords: frozenset[str] = frozenset()
    tags: frozenset[str] = frozenset()
    can_be_generated_in_combat: bool = True
    can_be_generated_by_modifiers: bool = True
    enchantments: dict[str, int] = field(default_factory=dict)
    effect_vars: dict[str, int] = field(default_factory=dict)
    instance_id: int = 0
    # X-cost and Star-cost support
    has_energy_cost_x: bool = False
    star_cost: int = 0
    has_star_cost_x: bool = False
    has_turn_end_in_hand_effect: bool = False
    # Persistent per-combat state (e.g. Rampage extra damage, Claw buff)
    combat_vars: dict[str, object] = field(default_factory=dict)
    # Original cost for cost-modification tracking
    original_cost: int | None = None
    single_turn_retain: bool = False
    bound: bool = False
    base_replay_count: int = 0

    def __post_init__(self):
        if self.original_cost is None:
            self.original_cost = self.cost
        tags = {
            _TAG_ALIASES.get(tag, tag)
            for tag in self.tags
        }
        tags.update(reference_canonical_tags(self.card_id))
        self.tags = frozenset(tags)
        self.has_turn_end_in_hand_effect = reference_has_turn_end_in_hand_effect(self.card_id)

    @property
    def is_attack(self) -> bool:
        return self.card_type == CardType.ATTACK

    @property
    def is_skill(self) -> bool:
        return self.card_type == CardType.SKILL

    @property
    def is_power(self) -> bool:
        return self.card_type == CardType.POWER

    @property
    def gains_block(self) -> bool:
        from sts2_env.cards.registry import card_gains_block

        return card_gains_block(self, reference_gains_block(self.card_id))

    @property
    def orb_evoke_type(self) -> OrbEvokeType:
        return reference_orb_evoke_type(self.card_id)

    @property
    def visual_card_pool(self) -> CardPoolId | None:
        reference_pool = reference_visual_card_pool(self.card_id)
        if reference_pool is not None:
            return reference_pool
        return _DEFAULT_VISUAL_CARD_POOL_BY_CARD_ID.get(
            self.card_id,
            _DEFAULT_VISUAL_CARD_POOL_BY_RARITY.get(self.rarity),
        )

    @property
    def visual_card_pool_is_colorless(self) -> bool:
        return self.visual_card_pool in {CardPoolId.COLORLESS, CardPoolId.EVENT, CardPoolId.TOKEN}

    def is_visual_colorless(self) -> bool:
        return self.visual_card_pool_is_colorless

    @property
    def should_show_in_card_library(self) -> bool:
        return reference_should_show_in_card_library(self.card_id)

    @property
    def has_custom_playability(self) -> bool:
        return reference_has_custom_playability(self.card_id)

    @property
    def has_custom_should_play(self) -> bool:
        return reference_has_custom_should_play(self.card_id)

    @property
    def has_custom_card_type(self) -> bool:
        return reference_has_custom_card_type(self.card_id)

    @property
    def has_custom_target_type(self) -> bool:
        return reference_has_custom_target_type(self.card_id)

    def is_playable_by_card_logic(self, owner_state: object, combat: object, owner: object) -> bool:
        from sts2_env.cards.registry import is_card_playable

        return is_card_playable(self, owner_state, combat, owner)

    def allows_hand_card_play(
        self,
        card: CardInstance,
        owner_state: object,
        combat: object,
        owner: object,
        *,
        is_auto_play: bool = False,
    ) -> bool:
        from sts2_env.cards.registry import should_play_card_from_hand

        return should_play_card_from_hand(self, card, owner_state, combat, owner, is_auto_play)

    def target_type_for(self, owner: object) -> TargetType:
        from sts2_env.cards.registry import card_target_type_for

        return card_target_type_for(self, owner, self.target_type)

    @property
    def is_status(self) -> bool:
        return self.card_type == CardType.STATUS

    @property
    def is_curse(self) -> bool:
        return self.card_type == CardType.CURSE

    @property
    def exhausts(self) -> bool:
        return "exhaust" in self.keywords

    @property
    def is_unplayable(self) -> bool:
        return (self.cost < 0 and not self.has_energy_cost_x) or "unplayable" in self.keywords

    @property
    def is_ethereal(self) -> bool:
        return "ethereal" in self.keywords

    @property
    def is_innate(self) -> bool:
        return "innate" in self.keywords

    @property
    def is_retain(self) -> bool:
        return "retain" in self.keywords

    @property
    def is_sly(self) -> bool:
        return "sly" in self.keywords or bool(self.combat_vars.get("sly_this_turn"))

    @property
    def has_tag(self) -> bool:
        return len(self.tags) > 0

    @property
    def is_enchanted(self) -> bool:
        return bool(self.enchantments)

    def has_card_tag(self, tag: str) -> bool:
        return tag in self.tags

    def has_enchantment(self, name: str) -> bool:
        return name in self.enchantments

    def add_enchantment(self, name: str, amount: int = 1) -> None:
        from sts2_env.cards.enchantments import apply_static_enchantment

        apply_static_enchantment(self, name, amount)

    @property
    def is_removable(self) -> bool:
        return "eternal" not in self.keywords

    @property
    def is_shiv(self) -> bool:
        return CardTag.SHIV in self.tags

    @property
    def affliction(self) -> str | None:
        affliction = self.combat_vars.get("_affliction")
        if isinstance(affliction, str):
            return affliction
        if self.bound:
            return "bound"
        return None

    def has_affliction(self, name: str) -> bool:
        return self.affliction == name

    @property
    def affliction_amount(self) -> int:
        """Stacked amount of this card's affliction (C# AfflictionModel.Amount).

        Afflictions stamped before amounts existed (bare combat_vars writes)
        carry the default 1; unafflicted cards report 0.
        """
        if self.affliction is None:
            return 0
        amount = self.combat_vars.get("_affliction_amount")
        return amount if isinstance(amount, int) else 1

    def can_afflict(self, name: str, *, stackable: bool = False) -> bool:
        current = self.affliction
        return current is None or (stackable and current == name)

    def afflict(self, name: str, *, stackable: bool = False, amount: int = 1) -> bool:
        """Attach (or stack) an affliction.

        Mirrors C# CardCmd.Afflict (CardCmd.cs:625-659): afflicting a card
        that already holds a different affliction fails; re-afflicting the
        same stackable affliction adds ``amount`` to its Amount (line 656).
        """
        if not self.can_afflict(name, stackable=stackable):
            return False
        if self.affliction == name and stackable:
            self.combat_vars["_affliction_amount"] = self.affliction_amount + amount
            return True
        self.combat_vars["_affliction"] = name
        self.combat_vars["_affliction_amount"] = amount
        if name == "bound":
            self.bound = True
        return True

    def clear_affliction(self, name: str | None = None) -> None:
        current = self.affliction
        if name is not None and current != name:
            return
        if current == "bound" or name == "bound":
            self.bound = False
        self.combat_vars.pop("_affliction", None)
        self.combat_vars.pop("_affliction_amount", None)

    def clone(self, new_id: int) -> CardInstance:
        """Create a copy with a new instance_id."""
        return CardInstance(
            card_id=self.card_id,
            cost=self.cost,
            card_type=self.card_type,
            target_type=self.target_type,
            rarity=self.rarity,
            base_damage=self.base_damage,
            base_block=self.base_block,
            upgraded=self.upgraded,
            keywords=self.keywords,
            tags=self.tags,
            can_be_generated_in_combat=self.can_be_generated_in_combat,
            can_be_generated_by_modifiers=self.can_be_generated_by_modifiers,
            enchantments=dict(self.enchantments),
            effect_vars=dict(self.effect_vars),
            instance_id=new_id,
            has_energy_cost_x=self.has_energy_cost_x,
            star_cost=self.star_cost,
            has_star_cost_x=self.has_star_cost_x,
            has_turn_end_in_hand_effect=self.has_turn_end_in_hand_effect,
            combat_vars={**self.combat_vars, "_is_clone": 1},
            original_cost=self.original_cost,
            single_turn_retain=self.single_turn_retain,
            bound=self.bound,
            base_replay_count=self.base_replay_count,
        )

    def create_dupe(self, new_id: int) -> CardInstance:
        dupe = self.clone(new_id)
        dupe.combat_vars["_is_dupe"] = 1
        dupe.keywords = frozenset(keyword for keyword in dupe.keywords if keyword != "exhaust")
        return dupe

    @property
    def should_retain_this_turn(self) -> bool:
        return self.is_retain or self.single_turn_retain

    @property
    def energy_cost(self) -> int:
        return self.cost

    @energy_cost.setter
    def energy_cost(self, value: int) -> None:
        self.cost = value

    def set_temporary_cost_for_turn(self, cost: int) -> None:
        self.combat_vars["_turn_cost_override"] = cost
        self.cost = cost

    def set_temporary_star_cost_for_turn(self, cost: int) -> None:
        self.combat_vars["_turn_star_cost_override"] = cost

    def set_temporary_free_this_turn(self) -> None:
        self.set_temporary_cost_for_turn(0)
        self.set_temporary_star_cost_for_turn(0)

    def set_combat_cost(self, cost: int) -> None:
        self.cost = cost

    def set_combat_star_cost(self, cost: int) -> None:
        self.combat_vars["_combat_star_cost_override"] = cost

    def set_free_this_combat(self) -> None:
        self.set_combat_cost(0)
        self.set_combat_star_cost(0)

    def after_forged(self) -> None:
        """Card lifecycle hook fired after a forge increases this card's damage."""
        return

    def on_chosen(self, combat: object) -> None:
        from sts2_env.cards.registry import fire_card_chosen

        fire_card_chosen(self, combat)

    def after_combat_end_in_deck(self, owner: object) -> bool:
        from sts2_env.cards.registry import apply_card_after_combat_end

        return apply_card_after_combat_end(self, owner)

    def modify_next_event(self, run_state: object, event: object | None) -> object | None:
        from sts2_env.cards.registry import modify_card_next_event

        return modify_card_next_event(self, run_state, event)

    def modify_unknown_map_point_room_types(self, run_state: object, room_types: set[object]) -> set[object]:
        from sts2_env.cards.registry import modify_card_unknown_room_types

        return modify_card_unknown_room_types(self, run_state, room_types)

    def modify_generated_map(self, run_state: object, act_map: object, act_index: int) -> object:
        from sts2_env.cards.registry import modify_card_generated_map

        return modify_card_generated_map(self, run_state, act_map, act_index)

    def modify_generated_map_late(self, run_state: object, act_map: object, act_index: int) -> object:
        from sts2_env.cards.registry import modify_card_generated_map_late

        return modify_card_generated_map_late(self, run_state, act_map, act_index)

    def after_map_generated(self, run_state: object, act_map: object, act_index: int) -> None:
        from sts2_env.cards.registry import fire_card_after_map_generated

        fire_card_after_map_generated(self, run_state, act_map, act_index)

    def on_quest_complete(self, run_state: object) -> int:
        from sts2_env.cards.registry import complete_card_quest

        return complete_card_quest(self, run_state)

    def modify_rest_site_options(
        self,
        owner: object,
        options: list[object],
        run_state: object | None,
    ) -> list[object]:
        from sts2_env.cards.registry import modify_card_rest_site_options

        return modify_card_rest_site_options(self, owner, options, run_state)

    def end_of_turn_cleanup(self) -> None:
        self.single_turn_retain = False
        self.bound = False
        if self.combat_vars.get("_affliction") == "bound":
            self.combat_vars.pop("_affliction", None)
            self.combat_vars.pop("_affliction_amount", None)
        self.combat_vars.pop("sly_this_turn", None)
        if "_turn_cost_override" in self.combat_vars:
            self.combat_vars.pop("_turn_cost_override", None)
            self.cost = self.original_cost if self.original_cost is not None else self.cost
        self.combat_vars.pop("_turn_star_cost_override", None)

    def __repr__(self) -> str:
        name = self.card_id.name
        cost_str = "X" if self.has_energy_cost_x else str(self.cost)
        parts = [f"{name}({cost_str}E"]
        if self.base_damage is not None:
            parts.append(f" {self.base_damage}dmg")
        if self.base_block is not None:
            parts.append(f" {self.base_block}blk")
        if self.upgraded:
            parts.append("+")
        return "".join(parts) + ")"


# Global instance counter for unique IDs
_next_instance_id = 0


def _get_next_id() -> int:
    global _next_instance_id
    _next_instance_id += 1
    return _next_instance_id


def new_card_instance_id() -> int:
    return _get_next_id()


def new_card_instance_id_after(cards: Iterable[object]) -> int:
    global _next_instance_id
    for card in cards:
        instance_id = getattr(card, "instance_id", 0)
        if isinstance(instance_id, int):
            _next_instance_id = max(_next_instance_id, instance_id)
    return _get_next_id()


def reset_instance_counter() -> None:
    global _next_instance_id
    _next_instance_id = 0
