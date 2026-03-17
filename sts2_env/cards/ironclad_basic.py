"""Ironclad basic card effects: Strike, Defend, Bash."""

from __future__ import annotations

from sts2_env.cards.base import CardInstance, _get_next_id
from sts2_env.cards.registry import register_effect
from sts2_env.core.enums import CardId, CardType, TargetType, CardRarity, ValueProp, PowerId
from sts2_env.core.damage import calculate_damage, apply_damage, calculate_block
from sts2_env.core.creature import Creature
from sts2_env.core.combat import CombatState


def _owner(card: CardInstance, combat: CombatState) -> Creature:
    return getattr(card, "owner", None) or combat.player


@register_effect(CardId.STRIKE_IRONCLAD)
def strike_ironclad(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    damage = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, damage, ValueProp.MOVE, combat, owner)


@register_effect(CardId.DEFEND_IRONCLAD)
def defend_ironclad(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    block = calculate_block(card.base_block, owner, ValueProp.MOVE, combat, card_source=card)
    owner.gain_block(block)


@register_effect(CardId.BASH)
def bash(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    damage = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, damage, ValueProp.MOVE, combat, owner)
    vuln_amount = card.effect_vars.get("vulnerable", 2)
    combat.apply_power_to(target, PowerId.VULNERABLE, vuln_amount)


# --- Card factories ---

def make_strike_ironclad() -> CardInstance:
    return CardInstance(
        card_id=CardId.STRIKE_IRONCLAD,
        cost=1,
        card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.BASIC,
        base_damage=6,
        instance_id=_get_next_id(),
    )


def make_defend_ironclad() -> CardInstance:
    return CardInstance(
        card_id=CardId.DEFEND_IRONCLAD,
        cost=1,
        card_type=CardType.SKILL,
        target_type=TargetType.SELF,
        rarity=CardRarity.BASIC,
        base_block=5,
        instance_id=_get_next_id(),
    )


def make_bash() -> CardInstance:
    return CardInstance(
        card_id=CardId.BASH,
        cost=2,
        card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.BASIC,
        base_damage=8,
        effect_vars={"vulnerable": 2},
        instance_id=_get_next_id(),
    )


def create_ironclad_starter_deck() -> list[CardInstance]:
    """Create the 10-card Ironclad starting deck."""
    deck = []
    for _ in range(5):
        deck.append(make_strike_ironclad())
    for _ in range(4):
        deck.append(make_defend_ironclad())
    deck.append(make_bash())
    return deck
