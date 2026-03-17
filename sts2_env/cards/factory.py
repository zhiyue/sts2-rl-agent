"""Helpers for instantiating cards from `CardId`.

This module builds a lazy `CardId -> make_*` registry by introspecting the
existing card modules. It is intentionally lightweight: the simulator already
has hundreds of per-card factories, but no central constructor registry.
"""

from __future__ import annotations

from dataclasses import dataclass
import importlib
import inspect
from functools import lru_cache
from pathlib import Path
import re
from typing import Callable, Literal

import sts2_env.cards  # noqa: F401  # ensure card modules are imported
from sts2_env.cards import base as card_base
from sts2_env.cards.base import CardInstance
from sts2_env.characters.all import ALL_CHARACTERS, get_character
from sts2_env.core.enums import CardId, CardRarity, CardTag, CardType, TargetType
from sts2_env.core.rng import Rng

CardFactory = Callable[..., CardInstance]
GenerationContext = Literal["combat", "modifier"]

_CARD_MODULES = (
    "sts2_env.cards.ironclad_basic",
    "sts2_env.cards.ironclad",
    "sts2_env.cards.silent",
    "sts2_env.cards.defect",
    "sts2_env.cards.necrobinder",
    "sts2_env.cards.regent",
    "sts2_env.cards.colorless",
    "sts2_env.cards.status",
)

_COMBAT_GENERATION_EXCLUDED = frozenset({
    CardId.ALCHEMIZE,
    CardId.DISINTEGRATION,
    CardId.FEED,
    CardId.FRANTIC_ESCAPE,
    CardId.HAND_OF_GREED,
    CardId.MIND_ROT,
    CardId.NEOWS_FURY,
    CardId.ROYALTIES_CARD,
    CardId.SLOTH_STATUS,
    CardId.SOOT,
    CardId.THE_HUNT,
    CardId.WASTE_AWAY,
})

_MODIFIER_GENERATION_EXCLUDED = frozenset({
    CardId.ASCENDERS_BANE,
    CardId.BAD_LUCK,
    CardId.CURSE_OF_THE_BELL,
    CardId.ENTHRALLED,
    CardId.FOLLY,
    CardId.GREED,
    CardId.POOR_SLEEP,
    CardId.SPORE_MIND,
})

_REFERENCE_VAR_ALIASES: dict[str, str] = {
    "vulnerable_power": "vulnerable",
    "weak_power": "weak",
    "strength_power": "strength",
    "dexterity_power": "dexterity",
    "plating_power": "plating",
    "doom_power": "doom",
    "doom_threshold": "doom_base",
    "calcify_power": "calcify",
    "countdown_power": "countdown",
    "danse_macabre_power": "danse_macabre",
    "debilitate_power": "debilitate",
    "lethality_power": "lethality",
    "sic_em_power": "sic_em",
    "sleight_of_flesh_power": "sleight_of_flesh",
    "devour_life_power": "devour_life",
    "vigor_power": "vigor",
    "arsenal_power": "arsenal",
    "black_hole_power": "black_hole",
    "parry_power": "parry",
    "furnace_power": "furnace",
    "stars_per_turn": "stars_per_turn",
    "block_for_stars": "block_for_stars",
}


@dataclass(frozen=True)
class CardMetadata:
    card_type: CardType
    rarity: CardRarity
    keywords: frozenset[str]
    can_be_generated_in_combat: bool
    can_be_generated_by_modifiers: bool


@dataclass(frozen=True)
class ReferenceCardDefinition:
    card_id_text: str
    color: str
    cost: str
    card_type: str
    rarity: str
    target: str
    keywords: tuple[str, ...]
    tags: tuple[CardTag, ...]
    vars_text: str
    upgrade_text: str


def _apply_generation_metadata(card: CardInstance) -> CardInstance:
    card.can_be_generated_in_combat = card.card_id not in _COMBAT_GENERATION_EXCLUDED
    card.can_be_generated_by_modifiers = card.card_id not in _MODIFIER_GENERATION_EXCLUDED
    return card


def _camel_to_snake(name: str) -> str:
    return re.sub(r"(?<!^)(?=[A-Z])", "_", name).lower()


@lru_cache(maxsize=1)
def _reference_cards() -> dict[str, ReferenceCardDefinition]:
    text = Path("docs/CARDS_REFERENCE.md").read_text()
    entries = re.split(r"^### ", text, flags=re.MULTILINE)[1:]
    result: dict[str, ReferenceCardDefinition] = {}
    for entry in entries:
        fields: dict[str, str] = {}
        for line in entry.splitlines():
            match = re.match(r"- \*\*(.+?):\*\* (.+)", line)
            if match:
                fields[match.group(1)] = match.group(2)
        card_id_text = fields.get("ID")
        if not card_id_text:
            continue
        keywords = tuple(
            keyword.strip().lower()
            for keyword in fields.get("Keywords", "None").split(",")
            if keyword.strip() and keyword.strip() != "None"
        )
        tags = tuple(
            CardTag[_camel_to_snake(tag.strip()).upper()]
            for tag in fields.get("Tags", "None").split(",")
            if tag.strip() and tag.strip() != "None"
        )
        result[card_id_text] = ReferenceCardDefinition(
            card_id_text=card_id_text,
            color=fields.get("Color", "").lower(),
            cost=fields["Cost"],
            card_type=fields["Type"],
            rarity=fields["Rarity"],
            target=fields["Target"],
            keywords=keywords,
            tags=tags,
            vars_text=fields.get("Vars", "{}"),
            upgrade_text=fields.get("Upgrade", ""),
        )
    return result


def _reference_candidates(card_id: CardId) -> list[str]:
    candidates = [card_id.name]
    if card_id.name.endswith("_CARD"):
        candidates.append(card_id.name[:-5])
    if card_id.name.endswith("_STATUS"):
        candidates.append(card_id.name[:-7])
    if card_id.name == "NULL_CARD":
        candidates.append("NULL")
    return candidates


def _reference_definition(card_id: CardId) -> ReferenceCardDefinition | None:
    refs = _reference_cards()
    for candidate in _reference_candidates(card_id):
        definition = refs.get(candidate)
        if definition is not None:
            return definition
    return None


def _coerce_reference_rarity(rarity_name: str) -> CardRarity:
    normalized = rarity_name.upper()
    if normalized == "TOKEN":
        return CardRarity.STATUS
    return CardRarity[normalized]


def _build_reference_effect_vars(vars_text: str) -> dict[str, int]:
    effect_vars: dict[str, int] = {}
    for key, value_text in re.findall(r"([A-Za-z][A-Za-z0-9]*): ([^,}]+)", vars_text):
        value_text = value_text.strip()
        if re.fullmatch(r"-?\d+", value_text):
            normalized_key = _camel_to_snake(key)
            normalized_key = _REFERENCE_VAR_ALIASES.get(normalized_key, normalized_key)
            effect_vars[normalized_key] = int(value_text)
    return effect_vars


def _apply_upgrade_text(
    card: CardInstance,
    effect_vars: dict[str, int],
    upgrade_text: str,
) -> None:
    if not upgrade_text or upgrade_text == "No upgrade changes":
        return
    for part in [item.strip() for item in upgrade_text.split(";") if item.strip()]:
        if part.startswith("Add "):
            effect_vars_key = _camel_to_snake(part[4:])
            card.keywords = frozenset(set(card.keywords) | {effect_vars_key})
            continue
        if part.startswith("Remove "):
            effect_vars_key = _camel_to_snake(part[7:])
            card.keywords = frozenset(keyword for keyword in card.keywords if keyword != effect_vars_key)
            continue
        match = re.fullmatch(r"([A-Za-z][A-Za-z0-9]*)([+-]\d+)", part)
        if not match:
            continue
        field_name = _camel_to_snake(match.group(1))
        delta = int(match.group(2))
        if field_name == "damage":
            card.base_damage = (card.base_damage or 0) + delta
        elif field_name == "block":
            card.base_block = (card.base_block or 0) + delta
        elif field_name == "cost":
            card.cost += delta
            card.original_cost += delta
        elif field_name == "star_cost":
            card.star_cost += delta
        else:
            effect_vars[field_name] = effect_vars.get(field_name, 0) + delta


def _build_reference_card(
    card_id: CardId,
    upgraded: bool = False,
    *,
    allow_generation: bool = False,
) -> CardInstance:
    definition = _reference_definition(card_id)
    if definition is None:
        raise KeyError(f"No reference card definition for {card_id!r}")

    cost_text = definition.cost.split("|", 1)[0].strip()
    star_cost = 0
    star_cost_match = re.search(r"StarCost:\s*(-?\d+)", definition.cost)
    if star_cost_match is not None:
        star_cost = int(star_cost_match.group(1))

    has_energy_cost_x = cost_text == "X"
    cost = 0 if has_energy_cost_x else int(cost_text)
    effect_vars = _build_reference_effect_vars(definition.vars_text)
    card_type = CardType[definition.card_type.upper()]
    base_damage = effect_vars.get("damage", effect_vars.get("calc_base"))
    base_block = effect_vars.get("block")
    if base_damage is None and card_type == CardType.ATTACK:
        base_damage = 0
    if base_block is None and card_type in {CardType.SKILL, CardType.POWER}:
        base_block = 0

    card = CardInstance(
        card_id=card_id,
        cost=cost,
        card_type=card_type,
        target_type=TargetType[_camel_to_snake(definition.target).upper()],
        rarity=_coerce_reference_rarity(definition.rarity),
        base_damage=base_damage,
        base_block=base_block,
        upgraded=upgraded,
        keywords=frozenset(definition.keywords),
        tags=frozenset(definition.tags),
        effect_vars=effect_vars,
        has_energy_cost_x=has_energy_cost_x,
        star_cost=star_cost,
    )
    if upgraded:
        _apply_upgrade_text(card, effect_vars, definition.upgrade_text)
    card = _apply_generation_metadata(card)
    if not allow_generation:
        card.can_be_generated_in_combat = False
        card.can_be_generated_by_modifiers = False
    return card


def create_reference_card(
    card_id: CardId,
    upgraded: bool = False,
    *,
    allow_generation: bool = False,
) -> CardInstance:
    """Instantiate a card from reference metadata when no handwritten factory exists."""
    return _build_reference_card(card_id, upgraded=upgraded, allow_generation=allow_generation)


def _reference_source_module(card_id: CardId) -> str | None:
    definition = _reference_definition(card_id)
    if definition is None:
        return None
    color = definition.color
    if color in {"colorless"}:
        return "sts2_env.cards.colorless"
    if color in {"event", "token", "status", "curse", "quest"}:
        return "sts2_env.cards.status"
    if color == "silent":
        return "sts2_env.cards.silent"
    if color == "defect":
        return "sts2_env.cards.defect"
    if color == "necrobinder":
        return "sts2_env.cards.necrobinder"
    if color == "regent":
        return "sts2_env.cards.regent"
    if color == "ironclad":
        return "sts2_env.cards.ironclad"
    return None


def _probe_factory(factory: CardFactory) -> CardInstance:
    """Call a card factory without consuming the global instance counter."""
    saved_counter = card_base._next_instance_id
    try:
        return _apply_generation_metadata(factory())
    finally:
        card_base._next_instance_id = saved_counter


def _supports_upgraded_arg(factory: CardFactory) -> bool:
    try:
        sig = inspect.signature(factory)
    except (TypeError, ValueError):
        return False
    return "upgraded" in sig.parameters


@lru_cache(maxsize=1)
def _factory_registry() -> dict[CardId, tuple[CardFactory, bool, str]]:
    registry: dict[CardId, tuple[CardFactory, bool, str]] = {}

    for module_name in _CARD_MODULES:
        module = importlib.import_module(module_name)
        for name, obj in vars(module).items():
            if not callable(obj) or not name.startswith("make_"):
                continue
            try:
                card = _probe_factory(obj)
            except TypeError:
                continue
            if not isinstance(card, CardInstance):
                continue
            registry[card.card_id] = (obj, _supports_upgraded_arg(obj), module_name)

    return registry


def create_card(card_id: CardId, upgraded: bool = False) -> CardInstance:
    """Instantiate a card by id using the existing `make_*` factories."""
    registry = _factory_registry()
    entry = registry.get(card_id)
    if entry is None:
        return create_reference_card(card_id, upgraded=upgraded, allow_generation=False)
    factory, supports_upgraded, _ = entry

    if supports_upgraded:
        return _apply_generation_metadata(factory(upgraded=upgraded))
    return _apply_generation_metadata(factory())


@lru_cache(maxsize=None)
def card_metadata(card_id: CardId) -> CardMetadata:
    registry = _factory_registry()
    if card_id in registry:
        factory, _, _ = registry[card_id]
        card = _probe_factory(factory)
    else:
        card = create_reference_card(card_id, allow_generation=False)
    return CardMetadata(
        card_type=card.card_type,
        rarity=card.rarity,
        keywords=card.keywords,
        can_be_generated_in_combat=card.can_be_generated_in_combat,
        can_be_generated_by_modifiers=card.can_be_generated_by_modifiers,
    )


@lru_cache(maxsize=None)
def card_preview(card_id: CardId) -> CardInstance:
    """Return a probed card instance without advancing the global id counter."""
    registry = _factory_registry()
    if card_id in registry:
        factory, _, _ = registry[card_id]
        return _probe_factory(factory)
    return create_reference_card(card_id, allow_generation=False)


def _coerce_rarity(rarity: str | CardRarity | None) -> CardRarity | None:
    if rarity is None or isinstance(rarity, CardRarity):
        return rarity
    return CardRarity[rarity.upper()]


def _matches_generation_context(
    metadata: CardMetadata,
    generation_context: GenerationContext | None,
) -> bool:
    if generation_context == "combat":
        return (
            metadata.can_be_generated_in_combat
            and metadata.rarity not in (CardRarity.BASIC, CardRarity.ANCIENT, CardRarity.EVENT)
        )
    if generation_context == "modifier":
        return metadata.can_be_generated_by_modifiers
    return True


def eligible_character_cards(
    character_id: str,
    *,
    card_type: CardType | None = None,
    rarity: str | CardRarity | None = None,
    require_keyword: str | None = None,
    generation_context: GenerationContext | None = "combat",
) -> list[CardId]:
    """Return eligible class cards from the owning character's card pool."""
    rarity_filter = _coerce_rarity(rarity)
    config = get_character(character_id)
    eligible: list[CardId] = []

    for card_id in config.card_pool:
        try:
            metadata = card_metadata(card_id)
        except KeyError:
            continue
        if not _matches_generation_context(metadata, generation_context):
            continue
        if card_type is not None and metadata.card_type is not card_type:
            continue
        if rarity_filter is not None and metadata.rarity is not rarity_filter:
            continue
        if require_keyword is not None and require_keyword not in metadata.keywords:
            continue
        eligible.append(card_id)

    return eligible


def eligible_registered_cards(
    *,
    module_name: str | None = None,
    card_type: CardType | None = None,
    rarity: str | CardRarity | None = None,
    exclude_ids: set[CardId] | None = None,
    generation_context: GenerationContext | None = "combat",
) -> list[CardId]:
    """Return registered cards filtered by source module and metadata."""
    rarity_filter = _coerce_rarity(rarity)
    exclude_ids = exclude_ids or set()
    eligible: list[CardId] = []

    registry = _factory_registry()
    for card_id in CardId:
        if card_id in exclude_ids:
            continue
        if card_id.name == "GENERIC":
            continue
        source_module = registry.get(card_id, (None, None, _reference_source_module(card_id)))[2]
        if source_module is None:
            continue
        if module_name is not None and source_module != module_name:
            continue
        try:
            metadata = card_metadata(card_id)
        except KeyError:
            continue
        if not _matches_generation_context(metadata, generation_context):
            continue
        if card_type is not None and metadata.card_type is not card_type:
            continue
        if rarity_filter is not None and metadata.rarity is not rarity_filter:
            continue
        eligible.append(card_id)

    return eligible


def create_cards_from_ids(
    card_ids: list[CardId],
    rng: Rng,
    count: int,
    *,
    distinct: bool = True,
) -> list[CardInstance]:
    """Create cards from a provided id pool."""
    if not card_ids or count <= 0:
        return []

    if distinct:
        chosen_ids = rng.sample(card_ids, min(count, len(card_ids)))
    else:
        chosen_ids = [rng.choice(card_ids) for _ in range(count)]
    return [create_card(card_id) for card_id in chosen_ids]


def create_character_cards(
    character_id: str,
    rng: Rng,
    count: int,
    *,
    card_type: CardType | None = None,
    rarity: str | CardRarity | None = None,
    require_keyword: str | None = None,
    distinct: bool = True,
    generation_context: GenerationContext | None = "combat",
) -> list[CardInstance]:
    """Create cards from the owning character pool with optional filtering."""
    eligible = eligible_character_cards(
        character_id,
        card_type=card_type,
        rarity=rarity,
        require_keyword=require_keyword,
        generation_context=generation_context,
    )
    return create_cards_from_ids(eligible, rng, count, distinct=distinct)


def create_distinct_character_cards(
    character_id: str,
    rng: Rng,
    count: int,
    *,
    card_type: CardType | None = None,
    rarity: str | CardRarity | None = None,
    require_keyword: str | None = None,
    generation_context: GenerationContext | None = "combat",
) -> list[CardInstance]:
    """Create up to `count` distinct cards from a character pool."""
    return create_character_cards(
        character_id,
        rng,
        count,
        card_type=card_type,
        rarity=rarity,
        require_keyword=require_keyword,
        generation_context=generation_context,
        distinct=True,
    )
