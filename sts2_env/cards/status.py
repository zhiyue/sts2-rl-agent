"""Status, Curse, Event, Token, and Quest card factories and effects.

Status (11), Curse (18), Event (27), Token (14), Quest (3).
"""

from __future__ import annotations

from sts2_env.cards.base import CardInstance, _get_next_id
from sts2_env.cards.factory import create_character_cards
from sts2_env.cards.registry import register_effect
from sts2_env.core.enums import (
    CardId, CardTag, CardType, TargetType, CardRarity, ValueProp, PowerId,
)
from sts2_env.core.damage import calculate_damage, apply_damage, calculate_block
from sts2_env.core.creature import Creature
from sts2_env.core.combat import CombatState


def _owner(card: CardInstance, combat: CombatState) -> Creature:
    return (
        getattr(card, "owner", None)
        or getattr(getattr(combat, "active_card_source", None), "owner", None)
        or combat.primary_player
    )


# ===========================================================================
# Helpers
# ===========================================================================

def _deal_damage_single(card: CardInstance, combat: CombatState, target: Creature) -> None:
    owner = _owner(card, combat)
    damage = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, damage, ValueProp.MOVE, combat, owner)


def _deal_damage_all(card: CardInstance, combat: CombatState) -> None:
    owner = _owner(card, combat)
    for enemy in list(combat.alive_enemies):
        damage = calculate_damage(card.base_damage, owner, enemy, ValueProp.MOVE, combat)
        apply_damage(enemy, damage, ValueProp.MOVE, combat, owner)


def _gain_block(card: CardInstance, combat: CombatState) -> None:
    owner = _owner(card, combat)
    block = calculate_block(card.base_block, owner, ValueProp.MOVE, combat, card_source=card)
    owner.gain_block(block)


# ===========================================================================
# STATUS CARDS (11)
# ===========================================================================

def make_wound() -> CardInstance:
    return CardInstance(
        card_id=CardId.WOUND, cost=-1, card_type=CardType.STATUS,
        target_type=TargetType.NONE, rarity=CardRarity.STATUS,
        keywords=frozenset({"unplayable"}), instance_id=_get_next_id(),
    )


def make_dazed() -> CardInstance:
    return CardInstance(
        card_id=CardId.DAZED, cost=-1, card_type=CardType.STATUS,
        target_type=TargetType.NONE, rarity=CardRarity.STATUS,
        keywords=frozenset({"unplayable", "ethereal"}), instance_id=_get_next_id(),
    )


@register_effect(CardId.SLIMED)
def slimed_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    cards = card.effect_vars.get("cards", 1)
    combat._draw_cards(cards)


def make_slimed() -> CardInstance:
    return CardInstance(
        card_id=CardId.SLIMED, cost=1, card_type=CardType.STATUS,
        target_type=TargetType.NONE, rarity=CardRarity.STATUS,
        keywords=frozenset({"exhaust"}), effect_vars={"cards": 1},
        instance_id=_get_next_id(),
    )


def make_burn() -> CardInstance:
    return CardInstance(
        card_id=CardId.BURN, cost=-1, card_type=CardType.STATUS,
        target_type=TargetType.NONE, rarity=CardRarity.STATUS,
        keywords=frozenset({"unplayable"}), instance_id=_get_next_id(),
    )


def make_void() -> CardInstance:
    return CardInstance(
        card_id=CardId.VOID, cost=-1, card_type=CardType.STATUS,
        target_type=TargetType.NONE, rarity=CardRarity.STATUS,
        keywords=frozenset({"unplayable", "ethereal"}),
        effect_vars={"energy": 1}, instance_id=_get_next_id(),
    )


@register_effect(CardId.BECKON)
def beckon_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Turn-end-in-hand HP loss is handled by combat hooks."""
    return


def make_beckon() -> CardInstance:
    return CardInstance(
        card_id=CardId.BECKON, cost=1, card_type=CardType.STATUS,
        target_type=TargetType.NONE, rarity=CardRarity.STATUS,
        effect_vars={"hp_loss": 6}, instance_id=_get_next_id(),
    )


def make_debris() -> CardInstance:
    return CardInstance(
        card_id=CardId.DEBRIS, cost=1, card_type=CardType.STATUS,
        target_type=TargetType.NONE, rarity=CardRarity.STATUS,
        keywords=frozenset({"exhaust"}), instance_id=_get_next_id(),
    )


def make_frantic_escape() -> CardInstance:
    return CardInstance(
        card_id=CardId.FRANTIC_ESCAPE, cost=1, card_type=CardType.STATUS,
        target_type=TargetType.SELF, rarity=CardRarity.STATUS,
        instance_id=_get_next_id(),
    )


def make_infection() -> CardInstance:
    return CardInstance(
        card_id=CardId.INFECTION, cost=-1, card_type=CardType.STATUS,
        target_type=TargetType.NONE, rarity=CardRarity.STATUS,
        keywords=frozenset({"unplayable"}), instance_id=_get_next_id(),
    )


def make_soot() -> CardInstance:
    return CardInstance(
        card_id=CardId.SOOT, cost=-1, card_type=CardType.STATUS,
        target_type=TargetType.NONE, rarity=CardRarity.STATUS,
        keywords=frozenset({"unplayable"}), instance_id=_get_next_id(),
    )


def make_toxic() -> CardInstance:
    return CardInstance(
        card_id=CardId.TOXIC, cost=1, card_type=CardType.STATUS,
        target_type=TargetType.NONE, rarity=CardRarity.STATUS,
        keywords=frozenset({"exhaust"}), instance_id=_get_next_id(),
    )


# ===========================================================================
# CURSE CARDS (18)
# ===========================================================================

def make_ascenders_bane() -> CardInstance:
    return CardInstance(
        card_id=CardId.ASCENDERS_BANE, cost=-1, card_type=CardType.CURSE,
        target_type=TargetType.NONE, rarity=CardRarity.CURSE,
        keywords=frozenset({"unplayable", "ethereal"}),
        instance_id=_get_next_id(),
    )


def make_bad_luck() -> CardInstance:
    return CardInstance(
        card_id=CardId.BAD_LUCK, cost=-1, card_type=CardType.CURSE,
        target_type=TargetType.NONE, rarity=CardRarity.CURSE,
        keywords=frozenset({"unplayable"}),
        effect_vars={"hp_loss": 13}, instance_id=_get_next_id(),
    )


def make_clumsy() -> CardInstance:
    return CardInstance(
        card_id=CardId.CLUMSY, cost=-1, card_type=CardType.CURSE,
        target_type=TargetType.NONE, rarity=CardRarity.CURSE,
        keywords=frozenset({"unplayable", "ethereal"}),
        instance_id=_get_next_id(),
    )


def make_curse_of_the_bell() -> CardInstance:
    return CardInstance(
        card_id=CardId.CURSE_OF_THE_BELL, cost=-1, card_type=CardType.CURSE,
        target_type=TargetType.NONE, rarity=CardRarity.CURSE,
        keywords=frozenset({"unplayable"}), instance_id=_get_next_id(),
    )


def make_debt() -> CardInstance:
    return CardInstance(
        card_id=CardId.DEBT, cost=-1, card_type=CardType.CURSE,
        target_type=TargetType.NONE, rarity=CardRarity.CURSE,
        keywords=frozenset({"unplayable"}),
        effect_vars={"gold": 10}, instance_id=_get_next_id(),
    )


def make_decay() -> CardInstance:
    return CardInstance(
        card_id=CardId.DECAY, cost=-1, card_type=CardType.CURSE,
        target_type=TargetType.NONE, rarity=CardRarity.CURSE,
        keywords=frozenset({"unplayable"}), instance_id=_get_next_id(),
    )


def make_doubt() -> CardInstance:
    return CardInstance(
        card_id=CardId.DOUBT, cost=-1, card_type=CardType.CURSE,
        target_type=TargetType.NONE, rarity=CardRarity.CURSE,
        keywords=frozenset({"unplayable"}),
        effect_vars={"weak": 1}, instance_id=_get_next_id(),
    )


def make_enthralled() -> CardInstance:
    return CardInstance(
        card_id=CardId.ENTHRALLED, cost=2, card_type=CardType.CURSE,
        target_type=TargetType.NONE, rarity=CardRarity.CURSE,
        instance_id=_get_next_id(),
    )


def make_folly() -> CardInstance:
    return CardInstance(
        card_id=CardId.FOLLY, cost=-1, card_type=CardType.CURSE,
        target_type=TargetType.NONE, rarity=CardRarity.CURSE,
        keywords=frozenset({"unplayable", "innate"}),
        instance_id=_get_next_id(),
    )


def make_greed() -> CardInstance:
    return CardInstance(
        card_id=CardId.GREED, cost=-1, card_type=CardType.CURSE,
        target_type=TargetType.NONE, rarity=CardRarity.CURSE,
        keywords=frozenset({"unplayable"}), instance_id=_get_next_id(),
    )


def make_guilty() -> CardInstance:
    return CardInstance(
        card_id=CardId.GUILTY, cost=-1, card_type=CardType.CURSE,
        target_type=TargetType.NONE, rarity=CardRarity.CURSE,
        keywords=frozenset({"unplayable"}),
        effect_vars={"combats": 5}, instance_id=_get_next_id(),
    )


def make_injury() -> CardInstance:
    return CardInstance(
        card_id=CardId.INJURY, cost=-1, card_type=CardType.CURSE,
        target_type=TargetType.NONE, rarity=CardRarity.CURSE,
        keywords=frozenset({"unplayable"}), instance_id=_get_next_id(),
    )


def make_normality() -> CardInstance:
    return CardInstance(
        card_id=CardId.NORMALITY, cost=-1, card_type=CardType.CURSE,
        target_type=TargetType.NONE, rarity=CardRarity.CURSE,
        keywords=frozenset({"unplayable"}),
        effect_vars={"card_limit": 3}, instance_id=_get_next_id(),
    )


def make_pain() -> CardInstance:
    return CardInstance(
        card_id=CardId.PAIN, cost=-1, card_type=CardType.CURSE,
        target_type=TargetType.NONE, rarity=CardRarity.CURSE,
        keywords=frozenset({"unplayable"}), instance_id=_get_next_id(),
    )


def make_parasite() -> CardInstance:
    return CardInstance(
        card_id=CardId.PARASITE, cost=-1, card_type=CardType.CURSE,
        target_type=TargetType.NONE, rarity=CardRarity.CURSE,
        keywords=frozenset({"unplayable"}), instance_id=_get_next_id(),
    )


def make_poor_sleep() -> CardInstance:
    return CardInstance(
        card_id=CardId.POOR_SLEEP, cost=-1, card_type=CardType.CURSE,
        target_type=TargetType.NONE, rarity=CardRarity.CURSE,
        keywords=frozenset({"unplayable", "retain"}),
        instance_id=_get_next_id(),
    )


def make_regret() -> CardInstance:
    return CardInstance(
        card_id=CardId.REGRET, cost=-1, card_type=CardType.CURSE,
        target_type=TargetType.NONE, rarity=CardRarity.CURSE,
        keywords=frozenset({"unplayable"}), instance_id=_get_next_id(),
    )


def make_shame() -> CardInstance:
    return CardInstance(
        card_id=CardId.SHAME, cost=-1, card_type=CardType.CURSE,
        target_type=TargetType.NONE, rarity=CardRarity.CURSE,
        keywords=frozenset({"unplayable"}),
        effect_vars={"frail": 1}, instance_id=_get_next_id(),
    )


def make_spore_mind() -> CardInstance:
    return CardInstance(
        card_id=CardId.SPORE_MIND, cost=1, card_type=CardType.CURSE,
        target_type=TargetType.NONE, rarity=CardRarity.CURSE,
        keywords=frozenset({"exhaust"}), instance_id=_get_next_id(),
    )


def make_writhe() -> CardInstance:
    return CardInstance(
        card_id=CardId.WRITHE, cost=-1, card_type=CardType.CURSE,
        target_type=TargetType.NONE, rarity=CardRarity.CURSE,
        keywords=frozenset({"innate", "unplayable"}),
        instance_id=_get_next_id(),
    )


# ===========================================================================
# EVENT CARDS (27)
# ===========================================================================

@register_effect(CardId.RIP_AND_TEAR)
def rip_and_tear_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    if target is None:
        return
    owner = _owner(card, combat)
    for _ in range(2):
        t = target if target.is_alive else None
        if t is None:
            alive = combat.alive_enemies
            if not alive:
                break
            t = combat.rng.choice(alive)
        damage = calculate_damage(card.base_damage, owner, t, ValueProp.MOVE, combat)
        apply_damage(t, damage, ValueProp.MOVE, combat, owner)


def make_rip_and_tear(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.RIP_AND_TEAR, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.RANDOM_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=9 if upgraded else 7, upgraded=upgraded,
        instance_id=_get_next_id(),
    )


@register_effect(CardId.APOTHEOSIS)
def apotheosis_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    for candidate in combat._all_cards_for_creature(owner):
        if candidate is not card:
            combat.upgrade_card(candidate)


def make_apotheosis(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.APOTHEOSIS, cost=1 if upgraded else 2,
        card_type=CardType.SKILL, target_type=TargetType.SELF,
        rarity=CardRarity.ANCIENT, upgraded=upgraded,
        keywords=frozenset({"exhaust", "innate"}),
        instance_id=_get_next_id(),
    )


@register_effect(CardId.APPARITION)
def apparition_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.INTANGIBLE, 1)


def make_apparition(upgraded: bool = False) -> CardInstance:
    kw = frozenset({"exhaust"}) if upgraded else frozenset({"ethereal", "exhaust"})
    return CardInstance(
        card_id=CardId.APPARITION, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.ANCIENT,
        upgraded=upgraded, keywords=kw, instance_id=_get_next_id(),
    )


@register_effect(CardId.BRIGHTEST_FLAME)
def brightest_flame_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    cards = card.effect_vars.get("cards", 2)
    energy = card.effect_vars.get("energy", 2)
    combat._draw_cards_for_creature(owner, cards)
    combat.gain_energy(owner, energy)
    combat.lose_max_hp(owner, card.effect_vars.get("max_hp_loss", 1))


def make_brightest_flame(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BRIGHTEST_FLAME, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.ANCIENT,
        upgraded=upgraded,
        effect_vars={
            "max_hp_loss": 1,
            "cards": 3 if upgraded else 2,
            "energy": 3 if upgraded else 2,
        },
        instance_id=_get_next_id(),
    )


@register_effect(CardId.MAUL)
def maul_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    for _ in range(2):
        _deal_damage_single(card, combat, target)
        if target.is_dead:
            break
    # All Maul copies gain +increase damage permanently
    increase = card.effect_vars.get("increase", 1)
    if card.base_damage is not None:
        card.base_damage += increase


def make_maul(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.MAUL, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.ANCIENT,
        base_damage=6 if upgraded else 5, upgraded=upgraded,
        effect_vars={"increase": 2 if upgraded else 1},
        instance_id=_get_next_id(),
    )


@register_effect(CardId.NEOWS_FURY)
def neows_fury_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    count = card.effect_vars.get("cards", 2)
    candidates = list(combat.discard_pile)
    if not candidates:
        return
    for selected in combat.rng.sample(candidates, min(count, len(candidates))):
        combat.move_card_to_hand(selected)


def make_neows_fury(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.NEOWS_FURY, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.ANCIENT,
        base_damage=14 if upgraded else 10, upgraded=upgraded,
        keywords=frozenset({"exhaust"}),
        effect_vars={"cards": 2}, instance_id=_get_next_id(),
    )


@register_effect(CardId.RELAX)
def relax_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    cards = card.effect_vars.get("cards", 2)
    energy = card.effect_vars.get("energy", 2)
    combat.apply_power_to(_owner(card, combat), PowerId.DRAW_CARDS_NEXT_TURN, cards)
    combat.apply_power_to(_owner(card, combat), PowerId.ENERGY_NEXT_TURN, energy)


def make_relax(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.RELAX, cost=3, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.ANCIENT,
        base_block=17 if upgraded else 15, upgraded=upgraded,
        keywords=frozenset({"exhaust"}),
        effect_vars={
            "cards": 3 if upgraded else 2,
            "energy": 3 if upgraded else 2,
        },
        instance_id=_get_next_id(),
    )


@register_effect(CardId.WHISTLE)
def whistle_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    combat.stun_enemy(target)


def make_whistle(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.WHISTLE, cost=3, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.ANCIENT,
        base_damage=44 if upgraded else 33, upgraded=upgraded,
        keywords=frozenset({"exhaust"}), instance_id=_get_next_id(),
    )


@register_effect(CardId.WISH)
def wish_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    ordered = sorted(combat.draw_pile, key=lambda c: (c.rarity.value, c.card_id.value))
    if not ordered:
        return
    combat.request_card_choice(
        prompt="Choose a card from draw pile",
        cards=ordered,
        source_pile="draw",
        resolver=combat.move_card_to_hand,
    )


def make_wish(upgraded: bool = False) -> CardInstance:
    kw = frozenset({"exhaust", "retain"}) if upgraded else frozenset({"exhaust"})
    return CardInstance(
        card_id=CardId.WISH, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.ANCIENT,
        upgraded=upgraded, keywords=kw, instance_id=_get_next_id(),
    )


@register_effect(CardId.BYRD_SWOOP)
def byrd_swoop_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)


def make_byrd_swoop(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BYRD_SWOOP, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.EVENT,
        base_damage=18 if upgraded else 14, upgraded=upgraded,
        instance_id=_get_next_id(),
    )


@register_effect(CardId.CALTROPS)
def caltrops_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    thorns = card.effect_vars.get("thorns", 3)
    combat.apply_power_to(_owner(card, combat), PowerId.THORNS, thorns)


def make_caltrops(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.CALTROPS, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.EVENT,
        upgraded=upgraded, effect_vars={"thorns": 5 if upgraded else 3},
        instance_id=_get_next_id(),
    )


@register_effect(CardId.CLASH)
def clash_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)


def make_clash(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.CLASH, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.EVENT,
        base_damage=18 if upgraded else 14, upgraded=upgraded,
        instance_id=_get_next_id(),
    )


@register_effect(CardId.DISTRACTION)
def distraction_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    generated = create_character_cards(
        combat.character_id,
        combat.rng,
        1,
        card_type=CardType.SKILL,
        generation_context="modifier",
    )
    if not generated:
        return
    generated[0].set_temporary_cost_for_turn(0)
    combat.move_card_to_hand(generated[0])


def make_distraction(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.DISTRACTION, cost=0 if upgraded else 1,
        card_type=CardType.SKILL, target_type=TargetType.SELF,
        rarity=CardRarity.EVENT, upgraded=upgraded,
        keywords=frozenset({"exhaust"}), instance_id=_get_next_id(),
    )


@register_effect(CardId.DUAL_WIELD)
def dual_wield_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    candidates = [c for c in combat.hand if c.card_type in (CardType.ATTACK, CardType.POWER)]
    if not candidates:
        return

    def _resolver(selected: CardInstance | None) -> None:
        if selected is None:
            return
        copies = card.effect_vars.get("cards", 1)
        clones = [selected.clone(combat.rng.next_int(1, 2**31 - 1)) for _ in range(max(0, copies))]
        combat._add_generated_cards_to_hand(clones)  # noqa: SLF001

    combat.request_card_choice(
        prompt="Choose an Attack or Power to copy",
        cards=candidates,
        source_pile="hand",
        resolver=_resolver,
    )


def make_dual_wield(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.DUAL_WIELD, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.EVENT,
        upgraded=upgraded,
        effect_vars={"cards": 2 if upgraded else 1},
        instance_id=_get_next_id(),
    )


@register_effect(CardId.ENLIGHTENMENT)
def enlightenment_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    state = combat.combat_player_state_for(owner)
    if state is None:
        return
    for held in state.hand:
        if card.upgraded:
            if not held.has_energy_cost_x:
                held.set_combat_cost(min(held.cost, 1))
        else:
            if not held.has_energy_cost_x:
                held.set_temporary_cost_for_turn(min(held.cost, 1))


def make_enlightenment(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.ENLIGHTENMENT, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.EVENT,
        upgraded=upgraded, keywords=frozenset({"exhaust"}),
        instance_id=_get_next_id(),
    )


@register_effect(CardId.ENTRENCH)
def entrench_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Double current block
    owner = _owner(card, combat)
    owner.gain_block(owner.block)


def make_entrench(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.ENTRENCH, cost=1 if upgraded else 2,
        card_type=CardType.SKILL, target_type=TargetType.SELF,
        rarity=CardRarity.EVENT, upgraded=upgraded,
        instance_id=_get_next_id(),
    )


@register_effect(CardId.EXTERMINATE)
def exterminate_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    hits = card.effect_vars.get("hits", 3)
    for _ in range(hits):
        _deal_damage_all(card, combat)


def make_exterminate(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.EXTERMINATE, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ALL_ENEMIES, rarity=CardRarity.EVENT,
        base_damage=4 if upgraded else 3, upgraded=upgraded,
        effect_vars={"hits": 3}, instance_id=_get_next_id(),
    )


@register_effect(CardId.FEEDING_FRENZY_CARD)
def feeding_frenzy_card_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    strength = card.effect_vars.get("strength", 5)
    combat.apply_power_to(_owner(card, combat), PowerId.FEEDING_FRENZY, strength)


def make_feeding_frenzy(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.FEEDING_FRENZY_CARD, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.EVENT,
        upgraded=upgraded,
        effect_vars={"strength": 7 if upgraded else 5},
        instance_id=_get_next_id(),
    )


@register_effect(CardId.HELLO_WORLD_CARD)
def hello_world_card_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.HELLO_WORLD, 1)


def make_hello_world(upgraded: bool = False) -> CardInstance:
    kw = frozenset({"innate"}) if upgraded else frozenset()
    return CardInstance(
        card_id=CardId.HELLO_WORLD_CARD, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.EVENT,
        upgraded=upgraded, keywords=kw, instance_id=_get_next_id(),
    )


@register_effect(CardId.MAD_SCIENCE)
def mad_science_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    rider = card.effect_vars.get("rider", 0)
    if card.card_type == CardType.ATTACK:
        assert target is not None
        hits = card.effect_vars.get("violence_hits", 1) if rider == 2 else 1
        for _ in range(hits):
            _deal_damage_single(card, combat, target)
        if rider == 1:
            target.apply_power(PowerId.WEAK, card.effect_vars.get("sapping_weak", 2))
            target.apply_power(PowerId.VULNERABLE, card.effect_vars.get("sapping_vulnerable", 2))
        elif rider == 3:
            target.apply_power(PowerId.STRANGLE, card.effect_vars.get("choking_damage", 6))
        return

    if card.card_type == CardType.SKILL:
        _gain_block(card, combat)
        if rider == 4:
            combat.gain_energy(owner, card.effect_vars.get("energized_energy", 2))
        elif rider == 5:
            combat.draw_cards(owner, card.effect_vars.get("wisdom_cards", 3))
        elif rider == 6:
            generated = create_character_cards(
                combat.character_id,
                combat.rng,
                1,
                distinct=True,
                generation_context="modifier",
            )
            if generated:
                generated[0].set_combat_cost(0)
                generated[0].owner = owner
                combat.hand.append(generated[0])
        return

    if rider == 7:
        owner.apply_power(PowerId.STRENGTH, card.effect_vars.get("expertise_strength", 2))
        owner.apply_power(PowerId.DEXTERITY, card.effect_vars.get("expertise_dexterity", 2))
    elif rider == 8:
        owner.apply_power(PowerId.CURIOUS, card.effect_vars.get("curious_reduction", 1))
    elif rider == 9:
        owner.apply_power(PowerId.IMPROVEMENT, 1)


def make_mad_science(upgraded: bool = False) -> CardInstance:
    kw = frozenset({"innate"}) if upgraded else frozenset()
    return CardInstance(
        card_id=CardId.MAD_SCIENCE, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.EVENT,
        base_damage=12, upgraded=upgraded, keywords=kw,
        effect_vars={
            "block": 8,
            "sapping_weak": 2,
            "sapping_vulnerable": 2,
            "violence_hits": 3,
            "choking_damage": 6,
            "energized_energy": 2,
            "wisdom_cards": 3,
            "expertise_strength": 2,
            "expertise_dexterity": 2,
            "curious_reduction": 1,
            "rider": 0,
        },
        instance_id=_get_next_id(),
    )


@register_effect(CardId.METAMORPHOSIS)
def metamorphosis_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    generated = create_character_cards(
        combat.character_id,
        combat.rng,
        card.effect_vars.get("cards", 3),
        card_type=CardType.ATTACK,
        distinct=False,
        generation_context="modifier",
    )
    for generated_card in generated:
        generated_card.set_combat_cost(0)
        combat.insert_card_into_draw_pile(generated_card, random_position=True)


def make_metamorphosis(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.METAMORPHOSIS, cost=2, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.EVENT,
        upgraded=upgraded, keywords=frozenset({"exhaust"}),
        effect_vars={"cards": 5 if upgraded else 3},
        instance_id=_get_next_id(),
    )


@register_effect(CardId.OUTMANEUVER)
def outmaneuver_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    energy = card.effect_vars.get("energy", 2)
    combat.apply_power_to(_owner(card, combat), PowerId.ENERGY_NEXT_TURN, energy)


def make_outmaneuver(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.OUTMANEUVER, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.EVENT,
        upgraded=upgraded,
        effect_vars={"energy": 3 if upgraded else 2},
        instance_id=_get_next_id(),
    )


@register_effect(CardId.PECK)
def peck_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    hits = card.effect_vars.get("hits", 3)
    for _ in range(hits):
        damage = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
        apply_damage(target, damage, ValueProp.MOVE, combat, owner)
        if target.is_dead:
            break


def make_peck(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.PECK, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.EVENT,
        base_damage=2, upgraded=upgraded,
        effect_vars={"hits": 4 if upgraded else 3},
        instance_id=_get_next_id(),
    )


@register_effect(CardId.REBOUND)
def rebound_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    combat.apply_power_to(_owner(card, combat), PowerId.REBOUND, 1)


def make_rebound(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.REBOUND, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.EVENT,
        base_damage=12 if upgraded else 9, upgraded=upgraded,
        instance_id=_get_next_id(),
    )


@register_effect(CardId.SQUASH)
def squash_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    vuln = card.effect_vars.get("vulnerable", 2)
    combat.apply_power_to(target, PowerId.VULNERABLE, vuln)


def make_squash(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SQUASH, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.EVENT,
        base_damage=12 if upgraded else 10, upgraded=upgraded,
        effect_vars={"vulnerable": 3 if upgraded else 2},
        instance_id=_get_next_id(),
    )


@register_effect(CardId.STACK)
def stack_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Block scales with discard pile size."""
    owner = _owner(card, combat)
    state = combat.combat_player_state_for(owner)
    discard_count = len(state.discard) if state is not None else 0
    base = card.base_block if card.base_block is not None else 0
    total_block = base + discard_count
    block = calculate_block(total_block, owner, ValueProp.MOVE, combat, card_source=card)
    owner.gain_block(block)


def make_stack(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.STACK, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.EVENT,
        base_block=3 if upgraded else 0, upgraded=upgraded,
        instance_id=_get_next_id(),
    )


@register_effect(CardId.TORIC_TOUGHNESS)
def toric_toughness_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    turns = card.effect_vars.get("turns", 2)
    combat.apply_power_to(_owner(card, combat), PowerId.TORIC_TOUGHNESS, turns)


def make_toric_toughness(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.TORIC_TOUGHNESS, cost=2, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.EVENT,
        base_block=7 if upgraded else 5, upgraded=upgraded,
        effect_vars={"turns": 2}, instance_id=_get_next_id(),
    )


# ===========================================================================
# TOKEN CARDS (14)
# ===========================================================================

@register_effect(CardId.FUEL)
def fuel_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    cards = card.effect_vars.get("cards", 1)
    energy = card.effect_vars.get("energy", 1)
    combat._draw_cards(cards)
    combat.energy += energy


def make_fuel(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.FUEL, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.STATUS,
        upgraded=upgraded, keywords=frozenset({"exhaust"}),
        effect_vars={"cards": 2 if upgraded else 1, "energy": 1},
        instance_id=_get_next_id(),
    )


@register_effect(CardId.GIANT_ROCK)
def giant_rock_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)


def make_giant_rock(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.GIANT_ROCK, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.STATUS,
        base_damage=20 if upgraded else 16, upgraded=upgraded,
        instance_id=_get_next_id(),
    )


@register_effect(CardId.LUMINESCE)
def luminesce_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    energy = card.effect_vars.get("energy", 2)
    combat.energy += energy


def make_luminesce(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.LUMINESCE, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.STATUS,
        upgraded=upgraded,
        keywords=frozenset({"exhaust", "retain"}),
        effect_vars={"energy": 3 if upgraded else 2},
        instance_id=_get_next_id(),
    )


@register_effect(CardId.MINION_DIVE_BOMB)
def minion_dive_bomb_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)


def make_minion_dive_bomb(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.MINION_DIVE_BOMB, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.STATUS,
        base_damage=16 if upgraded else 13, upgraded=upgraded,
        keywords=frozenset({"exhaust"}), instance_id=_get_next_id(),
    )


@register_effect(CardId.MINION_SACRIFICE)
def minion_sacrifice_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)


def make_minion_sacrifice(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.MINION_SACRIFICE, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.STATUS,
        base_block=12 if upgraded else 9, upgraded=upgraded,
        keywords=frozenset({"exhaust"}), instance_id=_get_next_id(),
    )


@register_effect(CardId.MINION_STRIKE)
def minion_strike_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    cards = card.effect_vars.get("cards", 1)
    combat._draw_cards(cards)


def make_minion_strike(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.MINION_STRIKE, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.STATUS,
        base_damage=10 if upgraded else 7, upgraded=upgraded,
        keywords=frozenset({"exhaust"}),
        effect_vars={"cards": 1}, instance_id=_get_next_id(),
    )


@register_effect(CardId.SHIV)
def shiv_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)


def make_shiv(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SHIV, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.STATUS,
        base_damage=6 if upgraded else 4, upgraded=upgraded,
        keywords=frozenset({"exhaust"}), tags=frozenset({CardTag.SHIV}), instance_id=_get_next_id(),
    )


@register_effect(CardId.SOUL)
def soul_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    cards = card.effect_vars.get("cards", 2)
    combat._draw_cards(cards)


def make_soul(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SOUL, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.STATUS,
        upgraded=upgraded, keywords=frozenset({"exhaust"}),
        effect_vars={"cards": 3 if upgraded else 2},
        instance_id=_get_next_id(),
    )


@register_effect(CardId.SOVEREIGN_BLADE)
def sovereign_blade_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Regent token attack; all-target only while Seeking Edge is active."""
    owner = _owner(card, combat)
    repeats = 1
    cost_increase = 0
    for power in owner.powers.values():
        get_repeats = getattr(power, "get_sovereign_blade_repeats", None)
        if callable(get_repeats):
            repeats = max(repeats, get_repeats())
        get_cost_increase = getattr(power, "get_sovereign_blade_cost_increase", None)
        if callable(get_cost_increase):
            cost_increase = max(cost_increase, get_cost_increase())

    if cost_increase:
        card.cost = (card.original_cost or card.cost) + cost_increase

    if owner.has_power(PowerId.SEEKING_EDGE):
        for _ in range(max(1, repeats)):
            _deal_damage_all(card, combat)
        return
    if target is not None:
        for _ in range(max(1, repeats)):
            _deal_damage_single(card, combat, target)


def make_sovereign_blade(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SOVEREIGN_BLADE, cost=1 if upgraded else 2,
        card_type=CardType.ATTACK, target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.STATUS, upgraded=upgraded,
        base_damage=10, keywords=frozenset({"retain"}),
        instance_id=_get_next_id(),
    )


@register_effect(CardId.SWEEPING_GAZE)
def sweeping_gaze_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """OstyAttack — deal damage to random enemy."""
    if target is not None:
        _deal_damage_single(card, combat, target)


def make_sweeping_gaze(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SWEEPING_GAZE, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.RANDOM_ENEMY, rarity=CardRarity.STATUS,
        base_damage=5 if upgraded else 0, upgraded=upgraded,
        keywords=frozenset({"ethereal", "exhaust"}),
        instance_id=_get_next_id(),
    )


# Token status cards (unplayable)
def make_disintegration() -> CardInstance:
    return CardInstance(
        card_id=CardId.DISINTEGRATION, cost=-1, card_type=CardType.STATUS,
        target_type=TargetType.NONE, rarity=CardRarity.STATUS,
        keywords=frozenset({"unplayable"}),
        effect_vars={"disintegration": 6}, instance_id=_get_next_id(),
    )


def make_mind_rot() -> CardInstance:
    return CardInstance(
        card_id=CardId.MIND_ROT, cost=-1, card_type=CardType.STATUS,
        target_type=TargetType.NONE, rarity=CardRarity.STATUS,
        keywords=frozenset({"unplayable"}),
        effect_vars={"mind_rot": 1}, instance_id=_get_next_id(),
    )


def make_sloth_status() -> CardInstance:
    return CardInstance(
        card_id=CardId.SLOTH_STATUS, cost=-1, card_type=CardType.STATUS,
        target_type=TargetType.NONE, rarity=CardRarity.STATUS,
        keywords=frozenset({"unplayable"}),
        effect_vars={"sloth": 3}, instance_id=_get_next_id(),
    )


def make_waste_away() -> CardInstance:
    return CardInstance(
        card_id=CardId.WASTE_AWAY, cost=-1, card_type=CardType.STATUS,
        target_type=TargetType.NONE, rarity=CardRarity.STATUS,
        keywords=frozenset({"unplayable"}),
        effect_vars={"waste_away": 1}, instance_id=_get_next_id(),
    )


# ===========================================================================
# QUEST CARDS (3)
# ===========================================================================

def make_byrdonis_egg() -> CardInstance:
    return CardInstance(
        card_id=CardId.BYRDONIS_EGG, cost=-1, card_type=CardType.QUEST,
        target_type=TargetType.NONE, rarity=CardRarity.QUEST,
        keywords=frozenset({"unplayable"}), instance_id=_get_next_id(),
    )


def make_lantern_key() -> CardInstance:
    return CardInstance(
        card_id=CardId.LANTERN_KEY, cost=-1, card_type=CardType.QUEST,
        target_type=TargetType.SELF, rarity=CardRarity.QUEST,
        keywords=frozenset({"unplayable"}), instance_id=_get_next_id(),
    )


def make_spoils_map() -> CardInstance:
    return CardInstance(
        card_id=CardId.SPOILS_MAP, cost=-1, card_type=CardType.QUEST,
        target_type=TargetType.SELF, rarity=CardRarity.QUEST,
        keywords=frozenset({"unplayable"}),
        effect_vars={"gold": 600}, instance_id=_get_next_id(),
    )


# ===========================================================================
# REGISTERED EFFECTS FOR ALL 36 MISSING CARDS
# ===========================================================================
# The registry (play_card_effect) already returns early for cards where
# card.is_unplayable or card.is_status is True.  We register explicit
# no-op effects here for:
#   (a) completeness / self-documentation,
#   (b) playable curse/status cards that would otherwise raise KeyError,
#   (c) cards whose actual gameplay-relevant triggers (end-of-turn, on-draw)
#       are handled by the combat system's hooks, not the play-effect path.
#
# Cards with end-of-turn or on-draw effects carry their parameters in
# effect_vars on the CardInstance (already set in the factory above).
# The combat loop inspects those to fire the appropriate trigger.
# ===========================================================================


# ---------------------------------------------------------------------------
# Status cards (8): BURN, DAZED, VOID, WOUND, DEBRIS, FRANTIC_ESCAPE,
#                   SOOT, TOXIC
# ---------------------------------------------------------------------------

@register_effect(CardId.BURN)
def burn_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Unplayable. End-of-turn: deals 2 damage to the player (unpowered).
    Triggered by combat end-of-turn hook, not this function."""
    pass


@register_effect(CardId.DAZED)
def dazed_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Unplayable, Ethereal. Pure dead-weight status card."""
    pass


@register_effect(CardId.VOID)
def void_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Unplayable, Ethereal. On draw: lose 1 energy.
    Triggered by the card-draw hook, not this function."""
    pass


@register_effect(CardId.WOUND)
def wound_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Unplayable. Pure dead-weight status card."""
    pass


@register_effect(CardId.DEBRIS)
def debris_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Playable (cost 1), Exhaust. Does nothing when played — just clogs hand."""
    pass


@register_effect(CardId.FRANTIC_ESCAPE)
def frantic_escape_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Increase the matching Sandpit countdown, then raise this card's cost."""
    owner = _owner(card, combat)
    for enemy in combat.enemies:
        sandpit = enemy.powers.get(PowerId.SANDPIT)
        if sandpit is not None and getattr(sandpit, "target", None) is owner:
            sandpit.amount += 1
            break
    increase = card.combat_vars.get("cost_increase", 0) + 1
    card.combat_vars["cost_increase"] = increase
    card.cost = (card.original_cost or 1) + increase


@register_effect(CardId.SOOT)
def soot_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Unplayable. Pure dead-weight status card."""
    pass


@register_effect(CardId.TOXIC)
def toxic_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Playable (cost 1), Exhaust. End-of-turn: deals 5 damage to player (unpowered).
    Triggered by combat end-of-turn hook, not this function.
    Playing it exhausts it, removing the end-of-turn penalty."""
    pass


# ---------------------------------------------------------------------------
# Debuff status cards (5): DISINTEGRATION, INFECTION, MIND_ROT,
#                          SLOTH_STATUS, WASTE_AWAY
# ---------------------------------------------------------------------------

@register_effect(CardId.DISINTEGRATION)
def disintegration_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Unplayable. When chosen by Knowledge Demon, applies DisintegrationPower(6).
    The OnChosen trigger is handled outside the play-effect path."""
    pass


@register_effect(CardId.INFECTION)
def infection_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Unplayable. End-of-turn: deals 3 damage to player (unpowered).
    Triggered by combat end-of-turn hook, not this function."""
    pass


@register_effect(CardId.MIND_ROT)
def mind_rot_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Unplayable. When chosen by Knowledge Demon, applies MindRotPower(1).
    The OnChosen trigger is handled outside the play-effect path."""
    pass


@register_effect(CardId.SLOTH_STATUS)
def sloth_status_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Unplayable. When chosen by Knowledge Demon, applies SlothPower(3).
    The OnChosen trigger is handled outside the play-effect path."""
    pass


@register_effect(CardId.WASTE_AWAY)
def waste_away_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Unplayable. When chosen by Knowledge Demon, applies WasteAwayPower(1).
    The OnChosen trigger is handled outside the play-effect path."""
    pass


# ---------------------------------------------------------------------------
# Curse cards (20): ASCENDERS_BANE, BAD_LUCK, CLUMSY, CURSE_OF_THE_BELL,
#   DEBT, DECAY, DOUBT, ENTHRALLED, FOLLY, GREED, GUILTY, INJURY,
#   NORMALITY, PAIN, PARASITE, POOR_SLEEP, REGRET, SHAME, SPORE_MIND,
#   WRITHE
# ---------------------------------------------------------------------------

@register_effect(CardId.ASCENDERS_BANE)
def ascenders_bane_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Unplayable, Ethereal, Eternal. Pure dead-weight curse."""
    pass


@register_effect(CardId.BAD_LUCK)
def bad_luck_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Unplayable, Eternal. End-of-turn: deal 13 HP loss to player.
    Triggered by combat end-of-turn hook, not this function."""
    pass


@register_effect(CardId.CLUMSY)
def clumsy_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Unplayable, Ethereal. Pure dead-weight curse."""
    pass


@register_effect(CardId.CURSE_OF_THE_BELL)
def curse_of_the_bell_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Unplayable, Eternal. Pure dead-weight curse."""
    pass


@register_effect(CardId.DEBT)
def debt_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Unplayable. End-of-turn: lose up to 10 gold.
    Triggered by combat end-of-turn hook, not this function."""
    pass


@register_effect(CardId.DECAY)
def decay_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Unplayable. End-of-turn: deal 2 damage to player (unpowered).
    Triggered by combat end-of-turn hook, not this function."""
    pass


@register_effect(CardId.DOUBT)
def doubt_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Unplayable. End-of-turn: apply 1 Weak to player (skip first tick).
    Triggered by combat end-of-turn hook, not this function."""
    pass


@register_effect(CardId.ENTHRALLED)
def enthralled_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Playable (cost 2), Eternal. When in hand, prevents the player from
    playing other cards (ShouldPlay check). Playing Enthralled itself is
    allowed and does nothing — it just wastes 2 energy to clear it from hand."""
    pass


@register_effect(CardId.FOLLY)
def folly_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Unplayable, Innate, Eternal. Always drawn first turn. Dead weight."""
    pass


@register_effect(CardId.GREED)
def greed_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Unplayable, Eternal. Pure dead-weight curse."""
    pass


@register_effect(CardId.GUILTY)
def guilty_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Unplayable. After 5 combats, removes itself from deck.
    Combat counter is tracked outside the play-effect path."""
    pass


@register_effect(CardId.INJURY)
def injury_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Unplayable. Pure dead-weight curse."""
    pass


@register_effect(CardId.NORMALITY)
def normality_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Unplayable. While in hand, limits player to 3 card plays per turn.
    Enforced by the combat system's ShouldPlay check, not this function."""
    pass


@register_effect(CardId.PAIN)
def pain_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Unplayable. When any other card is played, lose 1 HP.
    Triggered by the on-card-played hook, not this function."""
    pass


@register_effect(CardId.PARASITE)
def parasite_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Unplayable. When removed from deck, lose 3 max HP.
    Triggered by the card-removal hook, not this function."""
    pass


@register_effect(CardId.POOR_SLEEP)
def poor_sleep_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Unplayable, Retain. Dead-weight curse that stays in hand every turn."""
    pass


@register_effect(CardId.REGRET)
def regret_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Unplayable. End-of-turn: lose HP equal to number of cards in hand.
    Triggered by combat end-of-turn hook, not this function."""
    pass


@register_effect(CardId.SHAME)
def shame_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Unplayable. End-of-turn: apply 1 Frail to player (skip first tick).
    Triggered by combat end-of-turn hook, not this function."""
    pass


@register_effect(CardId.SPORE_MIND)
def spore_mind_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Playable (cost 1), Exhaust. Does nothing when played — just wastes
    1 energy to clear it from hand via exhaust."""
    pass


@register_effect(CardId.WRITHE)
def writhe_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Unplayable, Innate. Always drawn first turn. Dead weight."""
    pass


# ---------------------------------------------------------------------------
# Quest cards (3): BYRDONIS_EGG, LANTERN_KEY, SPOILS_MAP
# ---------------------------------------------------------------------------

@register_effect(CardId.BYRDONIS_EGG)
def byrdonis_egg_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Unplayable quest card. Adds 'Hatch' option at rest sites.
    Handled outside the play-effect path."""
    pass


@register_effect(CardId.LANTERN_KEY)
def lantern_key_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Unplayable quest card. Modifies map in Act 3 to force War Historian event.
    Handled outside the play-effect path."""
    pass


@register_effect(CardId.SPOILS_MAP)
def spoils_map_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Unplayable quest card. Modifies Act 2 map to add treasure room (600 gold).
    Handled outside the play-effect path."""
    pass
