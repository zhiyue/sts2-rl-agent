"""Silent card effects and factories — all 88 cards."""

from __future__ import annotations

from sts2_env.cards.base import CardInstance, _get_next_id
from sts2_env.cards.registry import register_effect
from sts2_env.core.enums import (
    CardId, CardTag, CardType, TargetType, CardRarity, ValueProp, PowerId, RoomType,
)
from sts2_env.core.damage import calculate_damage, apply_damage, calculate_block
from sts2_env.core.creature import Creature
from sts2_env.core.combat import CombatState


def _owner(card: CardInstance, combat: CombatState) -> Creature:
    return getattr(card, "owner", None) or combat.player


# ---------------------------------------------------------------------------
#  BASIC (4)
# ---------------------------------------------------------------------------

@register_effect(CardId.STRIKE_SILENT)
def strike_silent(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    dmg = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, owner)


@register_effect(CardId.DEFEND_SILENT)
def defend_silent(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    blk = calculate_block(card.base_block, owner, ValueProp.MOVE, combat, card_source=card)
    owner.gain_block(blk)


@register_effect(CardId.NEUTRALIZE)
def neutralize(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    dmg = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, owner)
    combat.apply_power_to(target, PowerId.WEAK, card.effect_vars.get("weak", 1))


@register_effect(CardId.SURVIVOR)
def survivor(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    blk = calculate_block(card.base_block, owner, ValueProp.MOVE, combat, card_source=card)
    owner.gain_block(blk)
    if not combat.hand:
        return
    combat.request_card_choice(
        prompt="Choose a hand card to discard",
        cards=list(combat.hand),
        source_pile="hand",
        resolver=lambda selected: combat.discard_cards([selected] if selected is not None else []),
    )


# ---------------------------------------------------------------------------
#  COMMON (21)
# ---------------------------------------------------------------------------

@register_effect(CardId.ACROBATICS)
def acrobatics(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    draw = card.effect_vars.get("cards", 3)
    combat._draw_cards(draw)
    # Discard 1 card
    if combat.hand:
        c = combat.hand.pop()
        combat.discard_pile.append(c)


@register_effect(CardId.ANTICIPATE)
def anticipate(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Gain temporary Dexterity (Anticipate power — next turn only)
    amt = card.effect_vars.get("dexterity", 3)
    combat.apply_power_to(_owner(card, combat), PowerId.DEXTERITY, amt)


@register_effect(CardId.BACKFLIP)
def backflip(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    blk = calculate_block(card.base_block, owner, ValueProp.MOVE, combat, card_source=card)
    owner.gain_block(blk)
    combat._draw_cards(card.effect_vars.get("cards", 2))


@register_effect(CardId.BLADE_DANCE)
def blade_dance(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    count = card.effect_vars.get("cards", 3)
    for _ in range(count):
        shiv = _make_shiv()
        combat.hand.append(shiv)


@register_effect(CardId.CLOAK_AND_DAGGER)
def cloak_and_dagger(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    blk = calculate_block(card.base_block, owner, ValueProp.MOVE, combat, card_source=card)
    owner.gain_block(blk)
    count = card.effect_vars.get("cards", 1)
    for _ in range(count):
        combat.hand.append(_make_shiv())


@register_effect(CardId.DAGGER_SPRAY)
def dagger_spray(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    for _ in range(2):
        for enemy in combat.alive_enemies:
            dmg = calculate_damage(card.base_damage, owner, enemy, ValueProp.MOVE, combat)
            apply_damage(enemy, dmg, ValueProp.MOVE, combat, owner)


@register_effect(CardId.DAGGER_THROW)
def dagger_throw(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    dmg = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, owner)
    combat._draw_cards(1)
    if combat.hand:
        c = combat.hand.pop()
        combat.discard_pile.append(c)


@register_effect(CardId.DEADLY_POISON)
def deadly_poison(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    combat.apply_power_to(target, PowerId.POISON, card.effect_vars.get("poison", 5))


@register_effect(CardId.DEFLECT)
def deflect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    blk = calculate_block(card.base_block, owner, ValueProp.MOVE, combat, card_source=card)
    owner.gain_block(blk)


@register_effect(CardId.DODGE_AND_ROLL)
def dodge_and_roll(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    blk = calculate_block(card.base_block, owner, ValueProp.MOVE, combat, card_source=card)
    owner.gain_block(blk)
    combat.apply_power_to(owner, PowerId.BLOCK_NEXT_TURN, card.base_block)


@register_effect(CardId.FLICK_FLACK)
def flick_flack(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    for enemy in combat.alive_enemies:
        dmg = calculate_damage(card.base_damage, owner, enemy, ValueProp.MOVE, combat)
        apply_damage(enemy, dmg, ValueProp.MOVE, combat, owner)


@register_effect(CardId.LEADING_STRIKE)
def leading_strike(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    dmg = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, owner)
    combat.hand.append(_make_shiv())


@register_effect(CardId.PIERCING_WAIL)
def piercing_wail(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    loss = card.effect_vars.get("strength_loss", 6)
    for enemy in combat.alive_enemies:
        combat.apply_power_to(enemy, PowerId.STRENGTH, -loss)


@register_effect(CardId.POISONED_STAB)
def poisoned_stab(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    dmg = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, owner)
    combat.apply_power_to(target, PowerId.POISON, card.effect_vars.get("poison", 3))


@register_effect(CardId.PREPARED)
def prepared(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    draw = card.effect_vars.get("cards", 1)
    combat._draw_cards(draw)
    if combat.hand:
        c = combat.hand.pop()
        combat.discard_pile.append(c)


@register_effect(CardId.RICOCHET)
def ricochet(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    hits = card.effect_vars.get("hits", 3)
    for _ in range(hits):
        alive = combat.alive_enemies
        if not alive:
            break
        t = combat.rng.choice(alive)
        dmg = calculate_damage(card.base_damage, owner, t, ValueProp.MOVE, combat)
        apply_damage(t, dmg, ValueProp.MOVE, combat, owner)


@register_effect(CardId.SLICE)
def slice_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    dmg = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, owner)


@register_effect(CardId.SNAKEBITE)
def snakebite(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    combat.apply_power_to(target, PowerId.POISON, card.effect_vars.get("poison", 7))


@register_effect(CardId.SUCKER_PUNCH)
def sucker_punch(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    dmg = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, owner)
    combat.apply_power_to(target, PowerId.WEAK, card.effect_vars.get("weak", 1))


@register_effect(CardId.UNTOUCHABLE)
def untouchable(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    blk = calculate_block(card.base_block, owner, ValueProp.MOVE, combat, card_source=card)
    owner.gain_block(blk)


# ---------------------------------------------------------------------------
#  UNCOMMON (27)
# ---------------------------------------------------------------------------

@register_effect(CardId.ACCURACY_CARD)
def accuracy(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.ACCURACY, card.effect_vars.get("accuracy", 4))


@register_effect(CardId.BACKSTAB)
def backstab(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    dmg = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, owner)


@register_effect(CardId.BLUR_CARD)
def blur(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    blk = calculate_block(card.base_block, owner, ValueProp.MOVE, combat, card_source=card)
    owner.gain_block(blk)
    combat.apply_power_to(owner, PowerId.BLUR, card.effect_vars.get("blur", 1))


@register_effect(CardId.BOUNCING_FLASK)
def bouncing_flask(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    hits = card.effect_vars.get("hits", 3)
    poison_amt = card.effect_vars.get("poison", 3)
    for _ in range(hits):
        alive = combat.alive_enemies
        if not alive:
            break
        t = combat.rng.choice(alive)
        combat.apply_power_to(t, PowerId.POISON, poison_amt)


@register_effect(CardId.BUBBLE_BUBBLE)
def bubble_bubble(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    combat.apply_power_to(target, PowerId.POISON, card.effect_vars.get("poison", 9))


@register_effect(CardId.CALCULATED_GAMBLE)
def calculated_gamble(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    hand_size = len(combat.hand)
    combat.discard_pile.extend(combat.hand)
    combat.hand.clear()
    combat._draw_cards(hand_size)


@register_effect(CardId.DASH)
def dash(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    dmg = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, owner)
    blk = calculate_block(card.base_block, owner, ValueProp.MOVE, combat, card_source=card)
    owner.gain_block(blk)


@register_effect(CardId.ESCAPE_PLAN)
def escape_plan(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    blk = calculate_block(card.base_block, owner, ValueProp.MOVE, combat, card_source=card)
    owner.gain_block(blk)
    combat._draw_cards(1)


@register_effect(CardId.EXPERTISE)
def expertise(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    draw = card.effect_vars.get("cards", 6)
    draw_up_to = draw - len(combat.hand)
    if draw_up_to > 0:
        combat._draw_cards(draw_up_to)


@register_effect(CardId.EXPOSE)
def expose(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    # Remove Artifact stacks then apply Vulnerable
    amt = card.effect_vars.get("power", 2)
    target.block = 0
    combat.apply_power_to(target, PowerId.VULNERABLE, amt)


@register_effect(CardId.FINISHER)
def finisher(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    # Hits once per attack played this turn (tracked by combat.turn_attack_count or similar)
    hits = max(1, getattr(combat, 'attacks_played_this_turn', 1))
    owner = _owner(card, combat)
    for _ in range(hits):
        dmg = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
        apply_damage(target, dmg, ValueProp.MOVE, combat, owner)


@register_effect(CardId.FLANKING)
def flanking(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    # Apply Flanking debuff (prevents enemy block)
    combat.apply_power_to(target, PowerId.NO_BLOCK, 1)


@register_effect(CardId.FLECHETTES)
def flechettes(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    # Deals damage for each skill in hand
    skill_count = sum(1 for c in combat.hand if c.is_skill)
    hits = max(0, skill_count)
    for _ in range(hits):
        dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
        apply_damage(target, dmg, ValueProp.MOVE, combat, _owner(card, combat))


@register_effect(CardId.FOLLOW_THROUGH)
def follow_through(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    for enemy in combat.alive_enemies:
        dmg = calculate_damage(card.base_damage, owner, enemy, ValueProp.MOVE, combat)
        apply_damage(enemy, dmg, ValueProp.MOVE, combat, owner)
    weak = card.effect_vars.get("weak", 1)
    for enemy in combat.alive_enemies:
        combat.apply_power_to(enemy, PowerId.WEAK, weak)


@register_effect(CardId.FOOTWORK)
def footwork(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.DEXTERITY, card.effect_vars.get("dexterity", 2))


@register_effect(CardId.HAND_TRICK)
def hand_trick(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    blk = calculate_block(card.base_block, owner, ValueProp.MOVE, combat, card_source=card)
    owner.gain_block(blk)
    candidates = [c for c in combat.hand if c.card_type == CardType.SKILL and not c.combat_vars.get("sly_this_turn")]
    if not candidates:
        return

    def _resolver(selected: CardInstance | None) -> None:
        if selected is None:
            return
        selected.combat_vars["sly_this_turn"] = 1

    combat.request_card_choice(
        prompt="Choose a Skill to make Sly this turn",
        cards=candidates,
        source_pile="hand",
        resolver=_resolver,
    )


@register_effect(CardId.HAZE)
def haze(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    poison = card.effect_vars.get("poison", 4)
    for enemy in combat.alive_enemies:
        combat.apply_power_to(enemy, PowerId.POISON, poison)


@register_effect(CardId.HIDDEN_DAGGERS)
def hidden_daggers(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Discard 2, create 2 upgraded Shivs
    for _ in range(min(2, len(combat.hand))):
        c = combat.hand.pop()
        combat.discard_pile.append(c)
    for _ in range(card.effect_vars.get("shivs", 2)):
        combat.hand.append(_make_shiv())


@register_effect(CardId.INFINITE_BLADES_CARD)
def infinite_blades(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Power: gain 1 Shiv at start of each turn
    combat.apply_power_to(_owner(card, combat), PowerId.INFINITE_BLADES, 1)


@register_effect(CardId.LEG_SWEEP)
def leg_sweep(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    blk = calculate_block(card.base_block, owner, ValueProp.MOVE, combat, card_source=card)
    owner.gain_block(blk)
    combat.apply_power_to(target, PowerId.WEAK, card.effect_vars.get("weak", 2))


@register_effect(CardId.MEMENTO_MORI)
def memento_mori(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    # Damage scales with exhaust pile size
    owner = _owner(card, combat)
    base = card.effect_vars.get("calc_base", 8)
    extra = card.effect_vars.get("extra_damage", 4)
    exhaust_count = len(combat.exhaust_pile)
    total_dmg = base + extra * exhaust_count
    dmg = calculate_damage(total_dmg, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, owner)


@register_effect(CardId.MIRAGE)
def mirage(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Block scales with total Poison on all enemies
    total_poison = sum(
        e.get_power_amount(PowerId.POISON) for e in combat.alive_enemies
    )
    extra = card.effect_vars.get("calc_extra", 1)
    block_amt = total_poison * extra
    owner = _owner(card, combat)
    blk = calculate_block(block_amt, owner, ValueProp.MOVE, combat, card_source=card)
    owner.gain_block(blk)


@register_effect(CardId.NOXIOUS_FUMES_CARD)
def noxious_fumes(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.NOXIOUS_FUMES, card.effect_vars.get("poison_per_turn", 2))


@register_effect(CardId.OUTBREAK_CARD)
def outbreak(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.OUTBREAK, card.effect_vars.get("outbreak", 11))


@register_effect(CardId.PHANTOM_BLADES_CARD)
def phantom_blades(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.PHANTOM_BLADES, card.effect_vars.get("phantom_blades", 9))


@register_effect(CardId.PINPOINT)
def pinpoint(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    dmg = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, owner)


@register_effect(CardId.POUNCE)
def pounce(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    dmg = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, owner)
    combat.apply_power_to(owner, PowerId.FREE_SKILL, 1)


@register_effect(CardId.PRECISE_CUT)
def precise_cut(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    # Damage scales down with skills in hand
    owner = _owner(card, combat)
    base = card.effect_vars.get("calc_base", 13)
    extra = card.effect_vars.get("extra_damage", 2)
    skill_count = sum(1 for c in combat.hand if c.is_skill)
    total_dmg = base - extra * skill_count
    total_dmg = max(0, total_dmg)
    dmg = calculate_damage(total_dmg, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, owner)


@register_effect(CardId.PREDATOR)
def predator(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    dmg = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, owner)
    combat.apply_power_to(owner, PowerId.DRAW_CARDS_NEXT_TURN, 2)


@register_effect(CardId.REFLEX)
def reflex(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat._draw_cards(card.effect_vars.get("cards", 2))


@register_effect(CardId.SKEWER)
def skewer(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    # X-cost: hits X times
    owner = _owner(card, combat)
    hits = combat.energy + card.cost  # energy was already spent
    for _ in range(max(0, hits)):
        dmg = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
        apply_damage(target, dmg, ValueProp.MOVE, combat, owner)
    combat.energy = 0


@register_effect(CardId.SPEEDSTER_CARD)
def speedster(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.SPEEDSTER, card.effect_vars.get("speedster", 2))


@register_effect(CardId.STRANGLE)
def strangle(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    dmg = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, owner)
    combat.apply_power_to(target, PowerId.CONSTRICT, card.effect_vars.get("strangle", 2))


@register_effect(CardId.TACTICIAN)
def tactician(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.energy += card.effect_vars.get("energy", 1)


@register_effect(CardId.UP_MY_SLEEVE)
def up_my_sleeve(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    count = card.effect_vars.get("cards", 3)
    for _ in range(count):
        shiv = _make_shiv()
        # Reduce cost by 1 this combat
        shiv.cost = max(0, shiv.cost - 1)
        combat.hand.append(shiv)


@register_effect(CardId.WELL_LAID_PLANS)
def well_laid_plans(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.WELL_LAID_PLANS, card.effect_vars.get("retain", 1))


# ---------------------------------------------------------------------------
#  RARE (26)
# ---------------------------------------------------------------------------

@register_effect(CardId.ABRASIVE)
def abrasive(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    combat.apply_power_to(owner, PowerId.DEXTERITY, card.effect_vars.get("dexterity", 1))
    combat.apply_power_to(owner, PowerId.THORNS, card.effect_vars.get("thorns", 4))


@register_effect(CardId.ACCELERANT)
def accelerant(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.ACCELERANT, card.effect_vars.get("accelerant", 1))


@register_effect(CardId.ADRENALINE)
def adrenaline(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat._draw_cards(card.effect_vars.get("cards", 2))
    combat.energy += card.effect_vars.get("energy", 1)


@register_effect(CardId.AFTERIMAGE_CARD)
def afterimage(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.AFTERIMAGE, card.effect_vars.get("afterimage", 1))


@register_effect(CardId.ASSASSINATE)
def assassinate(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    dmg = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, owner)
    combat.apply_power_to(target, PowerId.VULNERABLE, card.effect_vars.get("vulnerable", 1))


@register_effect(CardId.BLADE_OF_INK)
def blade_of_ink(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.BLADE_OF_INK, card.effect_vars.get("strength", 2))


@register_effect(CardId.BULLET_TIME)
def bullet_time(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # NoDraw + all cards in hand cost 0 this turn
    combat.apply_power_to(_owner(card, combat), PowerId.NO_DRAW, 1)
    for c in combat.hand:
        c.set_temporary_cost_for_turn(0)


@register_effect(CardId.BURST)
def burst(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.BURST, card.effect_vars.get("skills", 1))


@register_effect(CardId.CORROSIVE_WAVE)
def corrosive_wave(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.CORROSIVE_WAVE, card.effect_vars.get("corrosive_wave", 3))


@register_effect(CardId.ECHOING_SLASH)
def echoing_slash(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Deal non-attack damage to all enemies
    dmg_val = card.effect_vars.get("damage", 10)
    owner = _owner(card, combat)
    for enemy in combat.alive_enemies:
        apply_damage(enemy, dmg_val, ValueProp.NONE, combat, owner)


@register_effect(CardId.ENVENOM_CARD)
def envenom(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.ENVENOM, card.effect_vars.get("envenom", 1))


@register_effect(CardId.FAN_OF_KNIVES_CARD)
def fan_of_knives(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Power: create Shivs + gain FanOfKnives power
    combat.apply_power_to(_owner(card, combat), PowerId.FAN_OF_KNIVES, 1)
    for _ in range(card.effect_vars.get("shivs", 2)):
        combat.hand.append(_make_shiv())


@register_effect(CardId.GRAND_FINALE)
def grand_finale(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Only playable when draw pile is empty
    if len(combat.draw_pile) == 0:
        owner = _owner(card, combat)
        for enemy in combat.alive_enemies:
            dmg = calculate_damage(card.base_damage, owner, enemy, ValueProp.MOVE, combat)
            apply_damage(enemy, dmg, ValueProp.MOVE, combat, owner)


@register_effect(CardId.KNIFE_TRAP)
def knife_trap(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Create Shivs based on exhaust pile size and auto-play them
    exhaust_count = len(combat.exhaust_pile)
    owner = _owner(card, combat)
    for _ in range(exhaust_count):
        shiv = _make_shiv()
        # Auto-play the shiv at target
        if target is not None and target.is_alive:
            dmg = calculate_damage(shiv.base_damage, owner, target, ValueProp.MOVE, combat)
            apply_damage(target, dmg, ValueProp.MOVE, combat, owner)


@register_effect(CardId.MALAISE)
def malaise(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    # X-cost: reduce Strength by X and apply X Weak
    x = combat.energy + card.cost
    combat.apply_power_to(target, PowerId.STRENGTH, -x)
    combat.apply_power_to(target, PowerId.WEAK, x)
    combat.energy = 0


@register_effect(CardId.MASTER_PLANNER)
def master_planner(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.MASTER_PLANNER, 1)


@register_effect(CardId.MURDER)
def murder(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    # Damage = 1 per card exhausted this combat
    owner = _owner(card, combat)
    exhaust_count = len(combat.exhaust_pile)
    base = card.effect_vars.get("calc_base", 1)
    extra = card.effect_vars.get("extra_damage", 1)
    total_dmg = base * exhaust_count * extra
    dmg = calculate_damage(total_dmg, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, owner)


@register_effect(CardId.NIGHTMARE)
def nightmare(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    if not combat.hand:
        return

    def _resolver(selected: CardInstance | None) -> None:
        if selected is None:
            return
        owner = _owner(card, combat)
        combat.apply_power_to(owner, PowerId.NIGHTMARE, 3)
        power = owner.powers.get(PowerId.NIGHTMARE)
        if power is not None and hasattr(power, "set_selected_card"):
            power.set_selected_card(selected)

    combat.request_card_choice(
        prompt="Choose a card for Nightmare",
        cards=list(combat.hand),
        source_pile="hand",
        resolver=_resolver,
    )


@register_effect(CardId.SERPENT_FORM_CARD)
def serpent_form(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.SERPENT_FORM, card.effect_vars.get("serpent_form", 4))


@register_effect(CardId.SHADOW_STEP)
def shadow_step(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    cards = list(combat.hand)
    combat.discard_cards(cards, 0)
    combat.apply_power_to(_owner(card, combat), PowerId.SHADOW_STEP, 1)


@register_effect(CardId.SHADOWMELD)
def shadowmeld(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.SHADOWMELD, card.effect_vars.get("power", 1))


@register_effect(CardId.SNEAKY_CARD)
def sneaky(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.SNEAKY, card.effect_vars.get("sneaky", 1))


@register_effect(CardId.STORM_OF_STEEL)
def storm_of_steel(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Discard hand, create a Shiv for each discarded
    count = len(combat.hand)
    combat.discard_pile.extend(combat.hand)
    combat.hand.clear()
    for _ in range(count):
        combat.hand.append(_make_shiv())


@register_effect(CardId.THE_HUNT)
def the_hunt(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    from sts2_env.run.reward_objects import CardReward

    owner = _owner(card, combat)
    should_trigger_fatal = combat.should_owner_death_trigger_fatal(target)
    dmg = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, owner)
    if should_trigger_fatal and target.is_dead:
        room = getattr(combat, "room", None)
        if room is not None and hasattr(room, "add_extra_reward"):
            context = "boss" if getattr(room, "room_type", None) == RoomType.BOSS else "elite" if getattr(room, "room_type", None) == RoomType.ELITE else "regular"
            room.add_extra_reward(
                combat.player_id,
                CardReward(combat.player_id, context=context),
            )
        else:
            combat.extra_card_rewards += 1
        combat.apply_power_to(owner, PowerId.THE_HUNT, 1)


@register_effect(CardId.TOOLS_OF_THE_TRADE)
def tools_of_the_trade(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.TOOLS_OF_THE_TRADE, 1)


@register_effect(CardId.TRACKING)
def tracking(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.TRACKING, 1)


# ---------------------------------------------------------------------------
#  ANCIENT (2)
# ---------------------------------------------------------------------------

@register_effect(CardId.SUPPRESS)
def suppress(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    dmg = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, owner)
    combat.apply_power_to(target, PowerId.WEAK, card.effect_vars.get("weak", 3))


@register_effect(CardId.WRAITH_FORM)
def wraith_form(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    combat.apply_power_to(owner, PowerId.INTANGIBLE, card.effect_vars.get("intangible", 2))
    combat.apply_power_to(owner, PowerId.WRAITH_FORM, card.effect_vars.get("wraith_form", 1))


# ---------------------------------------------------------------------------
#  Shiv helper
# ---------------------------------------------------------------------------

def _make_shiv() -> CardInstance:
    return CardInstance(
        card_id=CardId.SHIV,
        cost=0,
        card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.COMMON,
        base_damage=4,
        tags=frozenset({CardTag.SHIV}),
        keywords=frozenset({"exhaust"}),
        instance_id=_get_next_id(),
    )


# ---------------------------------------------------------------------------
#  Card factories
# ---------------------------------------------------------------------------

def make_strike_silent() -> CardInstance:
    return CardInstance(
        card_id=CardId.STRIKE_SILENT, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.BASIC,
        base_damage=6, instance_id=_get_next_id(),
    )


def make_defend_silent() -> CardInstance:
    return CardInstance(
        card_id=CardId.DEFEND_SILENT, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.BASIC,
        base_block=5, instance_id=_get_next_id(),
    )


def make_neutralize() -> CardInstance:
    return CardInstance(
        card_id=CardId.NEUTRALIZE, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.BASIC,
        base_damage=3, effect_vars={"weak": 1}, instance_id=_get_next_id(),
    )


def make_survivor() -> CardInstance:
    return CardInstance(
        card_id=CardId.SURVIVOR, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.BASIC,
        base_block=8, instance_id=_get_next_id(),
    )


def make_acrobatics() -> CardInstance:
    return CardInstance(
        card_id=CardId.ACROBATICS, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        effect_vars={"cards": 3}, instance_id=_get_next_id(),
    )


def make_anticipate() -> CardInstance:
    return CardInstance(
        card_id=CardId.ANTICIPATE, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        effect_vars={"dexterity": 3}, instance_id=_get_next_id(),
    )


def make_backflip() -> CardInstance:
    return CardInstance(
        card_id=CardId.BACKFLIP, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        base_block=5, effect_vars={"cards": 2}, instance_id=_get_next_id(),
    )


def make_blade_dance() -> CardInstance:
    return CardInstance(
        card_id=CardId.BLADE_DANCE, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        keywords=frozenset({"exhaust"}),
        effect_vars={"cards": 3}, instance_id=_get_next_id(),
    )


def make_cloak_and_dagger() -> CardInstance:
    return CardInstance(
        card_id=CardId.CLOAK_AND_DAGGER, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        base_block=6, effect_vars={"cards": 1}, instance_id=_get_next_id(),
    )


def make_dagger_spray() -> CardInstance:
    return CardInstance(
        card_id=CardId.DAGGER_SPRAY, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ALL_ENEMIES, rarity=CardRarity.COMMON,
        base_damage=4, instance_id=_get_next_id(),
    )


def make_dagger_throw() -> CardInstance:
    return CardInstance(
        card_id=CardId.DAGGER_THROW, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=9, instance_id=_get_next_id(),
    )


def make_deadly_poison() -> CardInstance:
    return CardInstance(
        card_id=CardId.DEADLY_POISON, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        effect_vars={"poison": 5}, instance_id=_get_next_id(),
    )


def make_deflect() -> CardInstance:
    return CardInstance(
        card_id=CardId.DEFLECT, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        base_block=4, instance_id=_get_next_id(),
    )


def make_dodge_and_roll() -> CardInstance:
    return CardInstance(
        card_id=CardId.DODGE_AND_ROLL, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        base_block=4, instance_id=_get_next_id(),
    )


def make_flick_flack() -> CardInstance:
    return CardInstance(
        card_id=CardId.FLICK_FLACK, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ALL_ENEMIES, rarity=CardRarity.COMMON,
        base_damage=7, keywords=frozenset({"sly"}), instance_id=_get_next_id(),
    )


def make_leading_strike() -> CardInstance:
    return CardInstance(
        card_id=CardId.LEADING_STRIKE, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=7, instance_id=_get_next_id(),
    )


def make_piercing_wail() -> CardInstance:
    return CardInstance(
        card_id=CardId.PIERCING_WAIL, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.ALL_ENEMIES, rarity=CardRarity.COMMON,
        keywords=frozenset({"exhaust"}),
        effect_vars={"strength_loss": 6}, instance_id=_get_next_id(),
    )


def make_poisoned_stab() -> CardInstance:
    return CardInstance(
        card_id=CardId.POISONED_STAB, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=6, effect_vars={"poison": 3}, instance_id=_get_next_id(),
    )


def make_prepared() -> CardInstance:
    return CardInstance(
        card_id=CardId.PREPARED, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        effect_vars={"cards": 1}, instance_id=_get_next_id(),
    )


def make_ricochet() -> CardInstance:
    return CardInstance(
        card_id=CardId.RICOCHET, cost=2, card_type=CardType.ATTACK,
        target_type=TargetType.RANDOM_ENEMY, rarity=CardRarity.COMMON,
        base_damage=3, keywords=frozenset({"sly"}),
        effect_vars={"hits": 3}, instance_id=_get_next_id(),
    )


def make_slice() -> CardInstance:
    return CardInstance(
        card_id=CardId.SLICE, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=6, instance_id=_get_next_id(),
    )


def make_snakebite() -> CardInstance:
    return CardInstance(
        card_id=CardId.SNAKEBITE, cost=2, card_type=CardType.SKILL,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        keywords=frozenset({"retain"}),
        effect_vars={"poison": 7}, instance_id=_get_next_id(),
    )


def make_sucker_punch() -> CardInstance:
    return CardInstance(
        card_id=CardId.SUCKER_PUNCH, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=8, effect_vars={"weak": 1}, instance_id=_get_next_id(),
    )


def make_untouchable() -> CardInstance:
    return CardInstance(
        card_id=CardId.UNTOUCHABLE, cost=2, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        base_block=9, keywords=frozenset({"sly"}), instance_id=_get_next_id(),
    )


def make_accuracy() -> CardInstance:
    return CardInstance(
        card_id=CardId.ACCURACY_CARD, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={"accuracy": 4}, instance_id=_get_next_id(),
    )


def make_backstab() -> CardInstance:
    return CardInstance(
        card_id=CardId.BACKSTAB, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=11, keywords=frozenset({"exhaust", "innate"}),
        instance_id=_get_next_id(),
    )


def make_blur() -> CardInstance:
    return CardInstance(
        card_id=CardId.BLUR_CARD, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        base_block=5, effect_vars={"blur": 1}, instance_id=_get_next_id(),
    )


def make_bouncing_flask() -> CardInstance:
    return CardInstance(
        card_id=CardId.BOUNCING_FLASK, cost=2, card_type=CardType.SKILL,
        target_type=TargetType.RANDOM_ENEMY, rarity=CardRarity.UNCOMMON,
        effect_vars={"poison": 3, "hits": 3}, instance_id=_get_next_id(),
    )


def make_bubble_bubble() -> CardInstance:
    return CardInstance(
        card_id=CardId.BUBBLE_BUBBLE, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        effect_vars={"poison": 9}, instance_id=_get_next_id(),
    )


def make_calculated_gamble() -> CardInstance:
    return CardInstance(
        card_id=CardId.CALCULATED_GAMBLE, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        keywords=frozenset({"exhaust"}), instance_id=_get_next_id(),
    )


def make_dash() -> CardInstance:
    return CardInstance(
        card_id=CardId.DASH, cost=2, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=10, base_block=10, instance_id=_get_next_id(),
    )


def make_escape_plan() -> CardInstance:
    return CardInstance(
        card_id=CardId.ESCAPE_PLAN, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        base_block=3, instance_id=_get_next_id(),
    )


def make_expertise() -> CardInstance:
    return CardInstance(
        card_id=CardId.EXPERTISE, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={"cards": 6}, instance_id=_get_next_id(),
    )


def make_expose() -> CardInstance:
    return CardInstance(
        card_id=CardId.EXPOSE, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        keywords=frozenset({"exhaust"}),
        effect_vars={"power": 2}, instance_id=_get_next_id(),
    )


def make_finisher() -> CardInstance:
    return CardInstance(
        card_id=CardId.FINISHER, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=6, effect_vars={"calc_base": 0, "calc_extra": 1},
        instance_id=_get_next_id(),
    )


def make_flanking() -> CardInstance:
    return CardInstance(
        card_id=CardId.FLANKING, cost=2, card_type=CardType.SKILL,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        instance_id=_get_next_id(),
    )


def make_flechettes() -> CardInstance:
    return CardInstance(
        card_id=CardId.FLECHETTES, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=5, effect_vars={"calc_base": 0, "calc_extra": 1},
        instance_id=_get_next_id(),
    )


def make_follow_through() -> CardInstance:
    return CardInstance(
        card_id=CardId.FOLLOW_THROUGH, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ALL_ENEMIES, rarity=CardRarity.UNCOMMON,
        base_damage=6, effect_vars={"weak": 1}, instance_id=_get_next_id(),
    )


def make_footwork() -> CardInstance:
    return CardInstance(
        card_id=CardId.FOOTWORK, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={"dexterity": 2}, instance_id=_get_next_id(),
    )


def make_hand_trick() -> CardInstance:
    return CardInstance(
        card_id=CardId.HAND_TRICK, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        base_block=7, instance_id=_get_next_id(),
    )


def make_haze() -> CardInstance:
    return CardInstance(
        card_id=CardId.HAZE, cost=3, card_type=CardType.SKILL,
        target_type=TargetType.ALL_ENEMIES, rarity=CardRarity.UNCOMMON,
        keywords=frozenset({"sly"}),
        effect_vars={"poison": 4}, instance_id=_get_next_id(),
    )


def make_hidden_daggers() -> CardInstance:
    return CardInstance(
        card_id=CardId.HIDDEN_DAGGERS, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={"cards": 2, "shivs": 2}, instance_id=_get_next_id(),
    )


def make_infinite_blades() -> CardInstance:
    return CardInstance(
        card_id=CardId.INFINITE_BLADES_CARD, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        instance_id=_get_next_id(),
    )


def make_leg_sweep() -> CardInstance:
    return CardInstance(
        card_id=CardId.LEG_SWEEP, cost=2, card_type=CardType.SKILL,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_block=11, effect_vars={"weak": 2}, instance_id=_get_next_id(),
    )


def make_memento_mori() -> CardInstance:
    return CardInstance(
        card_id=CardId.MEMENTO_MORI, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=8, effect_vars={"calc_base": 8, "extra_damage": 4},
        instance_id=_get_next_id(),
    )


def make_mirage() -> CardInstance:
    return CardInstance(
        card_id=CardId.MIRAGE, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        keywords=frozenset({"exhaust"}),
        effect_vars={"calc_base": 0, "calc_extra": 1},
        instance_id=_get_next_id(),
    )


def make_noxious_fumes() -> CardInstance:
    return CardInstance(
        card_id=CardId.NOXIOUS_FUMES_CARD, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={"poison_per_turn": 2}, instance_id=_get_next_id(),
    )


def make_outbreak() -> CardInstance:
    return CardInstance(
        card_id=CardId.OUTBREAK_CARD, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={"outbreak": 11}, instance_id=_get_next_id(),
    )


def make_phantom_blades() -> CardInstance:
    return CardInstance(
        card_id=CardId.PHANTOM_BLADES_CARD, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={"phantom_blades": 9}, instance_id=_get_next_id(),
    )


def make_pinpoint() -> CardInstance:
    return CardInstance(
        card_id=CardId.PINPOINT, cost=3, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=17, instance_id=_get_next_id(),
    )


def make_pounce() -> CardInstance:
    return CardInstance(
        card_id=CardId.POUNCE, cost=2, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=12, instance_id=_get_next_id(),
    )


def make_precise_cut() -> CardInstance:
    return CardInstance(
        card_id=CardId.PRECISE_CUT, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=13, effect_vars={"calc_base": 13, "extra_damage": 2},
        instance_id=_get_next_id(),
    )


def make_predator() -> CardInstance:
    return CardInstance(
        card_id=CardId.PREDATOR, cost=2, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=15, instance_id=_get_next_id(),
    )


def make_reflex() -> CardInstance:
    return CardInstance(
        card_id=CardId.REFLEX, cost=3, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        keywords=frozenset({"sly"}),
        effect_vars={"cards": 2}, instance_id=_get_next_id(),
    )


def make_skewer() -> CardInstance:
    return CardInstance(
        card_id=CardId.SKEWER, cost=-1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=7, instance_id=_get_next_id(),
    )


def make_speedster() -> CardInstance:
    return CardInstance(
        card_id=CardId.SPEEDSTER_CARD, cost=2, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={"speedster": 2}, instance_id=_get_next_id(),
    )


def make_strangle() -> CardInstance:
    return CardInstance(
        card_id=CardId.STRANGLE, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=8, effect_vars={"strangle": 2}, instance_id=_get_next_id(),
    )


def make_tactician() -> CardInstance:
    return CardInstance(
        card_id=CardId.TACTICIAN, cost=3, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        keywords=frozenset({"sly"}),
        effect_vars={"energy": 1}, instance_id=_get_next_id(),
    )


def make_up_my_sleeve() -> CardInstance:
    return CardInstance(
        card_id=CardId.UP_MY_SLEEVE, cost=2, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={"cards": 3}, instance_id=_get_next_id(),
    )


def make_well_laid_plans() -> CardInstance:
    return CardInstance(
        card_id=CardId.WELL_LAID_PLANS, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={"retain": 1}, instance_id=_get_next_id(),
    )


def make_abrasive() -> CardInstance:
    return CardInstance(
        card_id=CardId.ABRASIVE, cost=3, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        keywords=frozenset({"sly"}),
        effect_vars={"thorns": 4, "dexterity": 1}, instance_id=_get_next_id(),
    )


def make_accelerant() -> CardInstance:
    return CardInstance(
        card_id=CardId.ACCELERANT, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        effect_vars={"accelerant": 1}, instance_id=_get_next_id(),
    )


def make_adrenaline() -> CardInstance:
    return CardInstance(
        card_id=CardId.ADRENALINE, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        keywords=frozenset({"exhaust"}),
        effect_vars={"cards": 2, "energy": 1}, instance_id=_get_next_id(),
    )


def make_afterimage() -> CardInstance:
    return CardInstance(
        card_id=CardId.AFTERIMAGE_CARD, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        effect_vars={"afterimage": 1}, instance_id=_get_next_id(),
    )


def make_assassinate() -> CardInstance:
    return CardInstance(
        card_id=CardId.ASSASSINATE, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.RARE,
        base_damage=10, keywords=frozenset({"innate", "exhaust"}),
        effect_vars={"vulnerable": 1}, instance_id=_get_next_id(),
    )


def make_blade_of_ink() -> CardInstance:
    return CardInstance(
        card_id=CardId.BLADE_OF_INK, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        effect_vars={"strength": 2}, instance_id=_get_next_id(),
    )


def make_bullet_time() -> CardInstance:
    return CardInstance(
        card_id=CardId.BULLET_TIME, cost=3, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        instance_id=_get_next_id(),
    )


def make_burst() -> CardInstance:
    return CardInstance(
        card_id=CardId.BURST, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        effect_vars={"skills": 1}, instance_id=_get_next_id(),
    )


def make_corrosive_wave() -> CardInstance:
    return CardInstance(
        card_id=CardId.CORROSIVE_WAVE, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        effect_vars={"corrosive_wave": 3}, instance_id=_get_next_id(),
    )


def make_echoing_slash() -> CardInstance:
    return CardInstance(
        card_id=CardId.ECHOING_SLASH, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ALL_ENEMIES, rarity=CardRarity.RARE,
        base_damage=10, effect_vars={"damage": 10}, instance_id=_get_next_id(),
    )


def make_envenom() -> CardInstance:
    return CardInstance(
        card_id=CardId.ENVENOM_CARD, cost=2, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        effect_vars={"envenom": 1}, instance_id=_get_next_id(),
    )


def make_fan_of_knives() -> CardInstance:
    return CardInstance(
        card_id=CardId.FAN_OF_KNIVES_CARD, cost=2, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        effect_vars={"shivs": 2}, instance_id=_get_next_id(),
    )


def make_grand_finale() -> CardInstance:
    return CardInstance(
        card_id=CardId.GRAND_FINALE, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ALL_ENEMIES, rarity=CardRarity.RARE,
        base_damage=50, instance_id=_get_next_id(),
    )


def make_knife_trap() -> CardInstance:
    return CardInstance(
        card_id=CardId.KNIFE_TRAP, cost=2, card_type=CardType.SKILL,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.RARE,
        effect_vars={"calc_base": 0, "calc_extra": 1}, instance_id=_get_next_id(),
    )


def make_malaise() -> CardInstance:
    return CardInstance(
        card_id=CardId.MALAISE, cost=-1, card_type=CardType.SKILL,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.RARE,
        keywords=frozenset({"exhaust"}), instance_id=_get_next_id(),
    )


def make_master_planner() -> CardInstance:
    return CardInstance(
        card_id=CardId.MASTER_PLANNER, cost=2, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        instance_id=_get_next_id(),
    )


def make_murder() -> CardInstance:
    return CardInstance(
        card_id=CardId.MURDER, cost=3, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.RARE,
        base_damage=1, effect_vars={"calc_base": 1, "extra_damage": 1},
        instance_id=_get_next_id(),
    )


def make_nightmare() -> CardInstance:
    return CardInstance(
        card_id=CardId.NIGHTMARE, cost=3, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        keywords=frozenset({"exhaust"}), instance_id=_get_next_id(),
    )


def make_serpent_form() -> CardInstance:
    return CardInstance(
        card_id=CardId.SERPENT_FORM_CARD, cost=3, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        effect_vars={"serpent_form": 4}, instance_id=_get_next_id(),
    )


def make_shadow_step() -> CardInstance:
    return CardInstance(
        card_id=CardId.SHADOW_STEP, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        effect_vars={"cards": 3}, instance_id=_get_next_id(),
    )


def make_shadowmeld() -> CardInstance:
    return CardInstance(
        card_id=CardId.SHADOWMELD, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        effect_vars={"power": 1}, instance_id=_get_next_id(),
    )


def make_sneaky() -> CardInstance:
    return CardInstance(
        card_id=CardId.SNEAKY_CARD, cost=2, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        keywords=frozenset({"sly"}),
        effect_vars={"sneaky": 1}, instance_id=_get_next_id(),
    )


def make_storm_of_steel() -> CardInstance:
    return CardInstance(
        card_id=CardId.STORM_OF_STEEL, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        instance_id=_get_next_id(),
    )


def make_the_hunt() -> CardInstance:
    return CardInstance(
        card_id=CardId.THE_HUNT, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.RARE,
        base_damage=10, keywords=frozenset({"exhaust"}),
        instance_id=_get_next_id(),
    )


def make_tools_of_the_trade() -> CardInstance:
    return CardInstance(
        card_id=CardId.TOOLS_OF_THE_TRADE, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        instance_id=_get_next_id(),
    )


def make_tracking() -> CardInstance:
    return CardInstance(
        card_id=CardId.TRACKING, cost=2, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        instance_id=_get_next_id(),
    )


def make_suppress() -> CardInstance:
    return CardInstance(
        card_id=CardId.SUPPRESS, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.ANCIENT,
        base_damage=11, keywords=frozenset({"innate"}),
        effect_vars={"weak": 3}, instance_id=_get_next_id(),
    )


def make_wraith_form() -> CardInstance:
    return CardInstance(
        card_id=CardId.WRAITH_FORM, cost=3, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.ANCIENT,
        effect_vars={"intangible": 2, "wraith_form": 1},
        instance_id=_get_next_id(),
    )


def create_silent_starter_deck() -> list[CardInstance]:
    """Create the 12-card Silent starting deck: 5 Strike, 5 Defend, Neutralize, Survivor."""
    deck = []
    for _ in range(5):
        deck.append(make_strike_silent())
    for _ in range(5):
        deck.append(make_defend_silent())
    deck.append(make_neutralize())
    deck.append(make_survivor())
    return deck
