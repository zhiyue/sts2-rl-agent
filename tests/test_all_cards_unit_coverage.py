"""Exhaustive per-card unit coverage guards.

These tests guarantee that every ``CardId`` has at least one direct test case.
Factory-backed playable cards also get a generic smoke-play test to catch
missing registrations, broken target routing, and unresolved choice flow.
"""

from __future__ import annotations

from dataclasses import dataclass
from functools import lru_cache
from pathlib import Path
import re

import pytest

import sts2_env.potions  # noqa: F401
import sts2_env.powers  # noqa: F401

from sts2_env.cards.base import CardInstance
from sts2_env.cards.factory import (
    _factory_registry,
    create_card,
    eligible_character_cards,
    eligible_registered_cards,
)
from sts2_env.cards.ironclad_basic import create_ironclad_starter_deck, make_bash
from sts2_env.cards.ironclad_basic import make_defend_ironclad, make_strike_ironclad
from sts2_env.cards.necrobinder import create_necrobinder_starter_deck
from sts2_env.cards.registry import _CARD_EFFECTS
from sts2_env.cards.regent import create_regent_starter_deck
from sts2_env.cards.silent import create_silent_starter_deck
from sts2_env.cards.defect import create_defect_starter_deck
from sts2_env.characters.all import ALL_CHARACTERS
from sts2_env.core.combat import CombatState
from sts2_env.core.creature import Creature
from sts2_env.core.enums import CardId, CardRarity, CardTag, CardType, CombatSide, TargetType
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle


@dataclass(frozen=True, slots=True)
class ReferenceCardMeta:
    card_id_text: str
    cost: str
    card_type: str
    rarity: str
    target: str
    keywords: tuple[str, ...]
    vars_text: str


_RUNTIME_ONLY_CARD_IDS = {CardId.GENERIC}


def _camel_to_snake(name: str) -> str:
    return re.sub(r"(?<!^)(?=[A-Z])", "_", name).lower()


@lru_cache(maxsize=1)
def _reference_cards() -> dict[str, ReferenceCardMeta]:
    text = Path("docs/CARDS_REFERENCE.md").read_text()
    entries = re.split(r"^### ", text, flags=re.MULTILINE)[1:]
    result: dict[str, ReferenceCardMeta] = {}
    for entry in entries:
        lines = entry.splitlines()
        fields: dict[str, str] = {}
        for line in lines:
            match = re.match(r"- \*\*(.+?):\*\* (.+)", line)
            if match:
                fields[match.group(1)] = match.group(2)
        card_id_text = fields.get("ID")
        if not card_id_text:
            continue
        keywords_text = fields.get("Keywords", "None")
        keywords = tuple(
            keyword.strip().lower()
            for keyword in keywords_text.split(",")
            if keyword.strip() and keyword.strip() != "None"
        )
        result[card_id_text] = ReferenceCardMeta(
            card_id_text=card_id_text,
            cost=fields["Cost"],
            card_type=fields["Type"],
            rarity=fields["Rarity"],
            target=fields["Target"],
            keywords=keywords,
            vars_text=fields.get("Vars", "{}"),
        )
    return result


def _reference_meta_for(card_id: CardId) -> ReferenceCardMeta | None:
    refs = _reference_cards()
    candidates = [card_id.name]
    if card_id.name.endswith("_CARD"):
        candidates.append(card_id.name[:-5])
    if card_id.name.endswith("_STATUS"):
        candidates.append(card_id.name[:-7])
    if card_id.name == "NULL_CARD":
        candidates.append("NULL")
    for candidate in candidates:
        meta = refs.get(candidate)
        if meta is not None:
            return meta
    return None


def _build_reference_card(card_id: CardId, meta: ReferenceCardMeta) -> CardInstance:
    card_type = CardType[meta.card_type.upper()]
    rarity_name = meta.rarity.upper()
    if rarity_name == "TOKEN":
        rarity_name = "STATUS"

    cost_text = meta.cost.split("|", 1)[0].strip()
    star_cost = 0
    star_cost_match = re.search(r"StarCost:\s*(-?\d+)", meta.cost)
    if star_cost_match is not None:
        star_cost = int(star_cost_match.group(1))

    cost = 0
    has_energy_cost_x = False
    if cost_text == "X":
        has_energy_cost_x = True
    else:
        cost = int(cost_text)

    effect_vars: dict[str, int] = {}
    for key, value_text in re.findall(r"([A-Za-z][A-Za-z0-9]*): ([^,}]+)", meta.vars_text):
        value_text = value_text.strip()
        if re.fullmatch(r"-?\d+", value_text):
            effect_vars[_camel_to_snake(key)] = int(value_text)

    base_damage = effect_vars.get("damage", effect_vars.get("calc_base"))
    base_block = effect_vars.get("block", effect_vars.get("calculated_block"))
    if base_damage is None and card_type == CardType.ATTACK:
        base_damage = 0
    if base_block is None and card_type in {CardType.SKILL, CardType.POWER}:
        base_block = 0

    return CardInstance(
        card_id=card_id,
        cost=cost,
        card_type=card_type,
        target_type=TargetType[_camel_to_snake(meta.target).upper()],
        rarity=CardRarity[rarity_name],
        base_damage=base_damage,
        base_block=base_block,
        keywords=frozenset(meta.keywords),
        effect_vars=effect_vars,
        has_energy_cost_x=has_energy_cost_x,
        star_cost=star_cost,
    )


def _card_for_test(card_id: CardId) -> CardInstance:
    registry = _factory_registry()
    if card_id in registry:
        return create_card(card_id)
    if card_id in _RUNTIME_ONLY_CARD_IDS:
        return CardInstance(
            card_id=card_id,
            cost=0,
            card_type=CardType.SKILL,
            target_type=TargetType.SELF,
            rarity=CardRarity.STATUS,
        )
    meta = _reference_meta_for(card_id)
    assert meta is not None, f"{card_id.name} is missing both factory and reference metadata"
    return _build_reference_card(card_id, meta)


def _character_id_for(card_id: CardId) -> str:
    for config in ALL_CHARACTERS:
        if card_id in config.card_pool:
            return config.character_id
    return "Ironclad"


def _starter_deck_for(character_id: str) -> list[CardInstance]:
    if character_id == "Silent":
        return create_silent_starter_deck()
    if character_id == "Defect":
        return create_defect_starter_deck()
    if character_id == "Necrobinder":
        return create_necrobinder_starter_deck()
    if character_id == "Regent":
        return create_regent_starter_deck()
    return create_ironclad_starter_deck()


def _make_smoke_combat(card: CardInstance) -> CombatState:
    character_id = _character_id_for(card.card_id)
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=_starter_deck_for(character_id),
        rng_seed=123,
        character_id=character_id,
    )
    creature, ai = create_shrinker_beetle(Rng(123))
    combat.add_enemy(creature, ai)
    combat.start_combat()

    ally = Creature(max_hp=30, current_hp=30, side=CombatSide.PLAYER, is_player=True)
    combat.add_ally_player(ally)

    combat.hand = [card, make_strike_ironclad(), make_defend_ironclad()]
    combat.draw_pile = [make_bash(), make_strike_ironclad(), make_defend_ironclad()]
    combat.discard_pile = [make_bash(), make_strike_ironclad(), make_defend_ironclad()]
    combat.exhaust_pile = [make_bash()]
    combat.energy = 10
    combat.stars = 10
    combat.player.stars = 10
    if CardTag.OSTY_ATTACK in getattr(card, "tags", ()) or "osty_attack" in getattr(card, "tags", ()):
        combat.summon_osty(combat.player, 10)
    if card.card_id == CardId.GRAND_FINALE:
        combat.draw_pile = []
    if card.card_id == CardId.CLASH:
        combat.hand = [card, make_strike_ironclad(), make_bash()]
    return combat


def _resolve_all_pending_choices(combat: CombatState) -> None:
    for _ in range(20):
        choice = combat.pending_choice
        if choice is None:
            return
        if choice.is_multi:
            if not choice.can_confirm() and choice.options:
                next_index = next(
                    (i for i in range(len(choice.options)) if i not in choice.selected_indices),
                    None,
                )
                assert next_index is not None
                assert combat.resolve_pending_choice(next_index)
                continue
            assert combat.resolve_pending_choice(None)
            continue
        if choice.options:
            assert combat.resolve_pending_choice(0)
        else:
            assert combat.resolve_pending_choice(None)
    raise AssertionError("Pending card choice did not resolve within 20 steps")


def _target_for(card: CardInstance, combat: CombatState) -> Creature | None:
    if card.target_type == TargetType.ANY_ENEMY:
        return combat.enemies[0]
    if card.target_type == TargetType.ANY_ALLY:
        return combat.get_player_allies_of(combat.player)[0]
    return None


@pytest.mark.parametrize("card_id", list(CardId), ids=lambda current: current.name)
def test_every_card_has_direct_unit_case(card_id: CardId):
    """Every CardId must have a direct, per-card unit-test case."""
    card = _card_for_test(card_id)
    assert card.card_id is card_id
    assert card.original_cost == card.cost

    if card_id in _RUNTIME_ONLY_CARD_IDS:
        assert card_id not in _CARD_EFFECTS
        return

    if card_id in _factory_registry():
        assert isinstance(card, CardInstance)
    else:
        assert _reference_meta_for(card_id) is not None

    if not card.is_unplayable and card.card_type not in {CardType.CURSE, CardType.QUEST}:
        combat = _make_smoke_combat(card)
        target_index = None
        if card.target_type == TargetType.ANY_ENEMY:
            target_index = 0
        elif card.target_type == TargetType.ANY_ALLY:
            target_index = 0
        played = combat.play_card(0, target_index)
        assert played, f"{card_id.name} should play successfully in the direct per-card case"
        _resolve_all_pending_choices(combat)

    if not card.is_unplayable and card.card_type not in {CardType.STATUS, CardType.CURSE, CardType.QUEST}:
        assert card_id in _CARD_EFFECTS


def _playable_factory_backed_cards() -> list[CardId]:
    result: list[CardId] = []
    for card_id in sorted(_factory_registry(), key=lambda current: current.value):
        card = create_card(card_id)
        if card.is_unplayable:
            continue
        if card.card_type in {CardType.CURSE, CardType.QUEST}:
            continue
        result.append(card_id)
    return result


@pytest.mark.parametrize("card_id", _playable_factory_backed_cards(), ids=lambda current: current.name)
def test_factory_backed_playable_cards_have_smoke_execution(card_id: CardId):
    """Factory-backed playable cards should survive a generic play/choice smoke test."""
    card = create_card(card_id)
    combat = _make_smoke_combat(card)

    target_index = None
    if card.target_type == TargetType.ANY_ENEMY:
        target_index = 0
    elif card.target_type == TargetType.ANY_ALLY:
        target_index = 0

    played = combat.play_card(0, target_index)
    assert played, f"{card_id.name} should play successfully in the smoke harness"
    _resolve_all_pending_choices(combat)

    assert combat.pending_choice is None


def test_docs_backed_character_cards_still_participate_in_generation_pools():
    assert CardId.BONE_SHARDS in eligible_character_cards("Necrobinder")


def test_docs_backed_colorless_cards_still_participate_in_registered_colorless_pool():
    assert CardId.GANG_UP in eligible_registered_cards(module_name="sts2_env.cards.colorless")


def test_docs_backed_cards_preserve_reference_tags_for_explicit_instantiation():
    assert CardTag.OSTY_ATTACK in create_card(CardId.BONE_SHARDS).tags


def test_new_real_necrobinder_factory_cards_participate_in_generation_pools():
    assert CardId.BORROWED_TIME in eligible_character_cards("Necrobinder", generation_context=None)


def test_new_real_regent_factory_cards_participate_in_generation_pools():
    assert CardId.ALIGNMENT in eligible_character_cards("Regent", generation_context=None)
