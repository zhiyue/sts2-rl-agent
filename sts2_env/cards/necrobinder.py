"""Necrobinder card effects and factories (88 cards).

Covers all Necrobinder cards: Basic (4), Common (20), Uncommon (36),
Rare (26), Ancient (2).
"""

from __future__ import annotations

from sts2_env.cards.base import CardInstance, _get_next_id
from sts2_env.cards.registry import register_effect, register_late_effect
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


def _osty(card: CardInstance, combat: CombatState) -> Creature | None:
    return combat.get_osty(_owner(card, combat))


# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

def _deal_damage_single(card: CardInstance, combat: CombatState, target: Creature) -> None:
    """Standard single-target damage."""
    owner = _owner(card, combat)
    damage = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, damage, ValueProp.MOVE, combat, owner)


def _deal_damage_all(card: CardInstance, combat: CombatState) -> None:
    """Deal card.base_damage to all enemies."""
    owner = _owner(card, combat)
    for enemy in list(combat.alive_enemies):
        damage = calculate_damage(card.base_damage, owner, enemy, ValueProp.MOVE, combat)
        apply_damage(enemy, damage, ValueProp.MOVE, combat, owner)


def _gain_block(card: CardInstance, combat: CombatState) -> None:
    owner = _owner(card, combat)
    block = calculate_block(card.base_block, owner, ValueProp.MOVE, combat, card_source=card)
    owner.gain_block(block)


# ---------------------------------------------------------------------------
# Basic cards (4)
# ---------------------------------------------------------------------------

@register_effect(CardId.STRIKE_NECROBINDER)
def strike_necrobinder(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)


@register_effect(CardId.DEFEND_NECROBINDER)
def defend_necrobinder(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)


@register_effect(CardId.BODYGUARD)
def bodyguard(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.summon_osty(_owner(card, combat), card.effect_vars.get("summon", 5))


@register_effect(CardId.UNLEASH)
def unleash(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """OstyAttack — damage scales with Osty stacks."""
    assert target is not None
    _deal_damage_single(card, combat, target)


# ---------------------------------------------------------------------------
# Common cards (20)
# ---------------------------------------------------------------------------

@register_effect(CardId.AFTERLIFE)
def afterlife(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.summon_osty(_owner(card, combat), card.effect_vars.get("summon", 6))


@register_effect(CardId.BLIGHT_STRIKE)
def blight_strike(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    doom = card.effect_vars.get("doom", 0)
    if doom:
        combat.apply_power_to(target, PowerId.DOOM, doom)


@register_effect(CardId.DEFILE)
def defile(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)


@register_effect(CardId.DEFY)
def defy(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _gain_block(card, combat)
    weak = card.effect_vars.get("weak", 1)
    combat.apply_power_to(target, PowerId.WEAK, weak)


@register_effect(CardId.DRAIN_POWER)
def drain_power(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    combat.upgrade_random_cards(combat.discard_pile, card.effect_vars.get("cards", 2))


@register_effect(CardId.FEAR)
def fear(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    vuln = card.effect_vars.get("vulnerable", 1)
    combat.apply_power_to(target, PowerId.VULNERABLE, vuln)


@register_effect(CardId.FLATTEN)
def flatten(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """OstyAttack — damage dealt by both player and osty."""
    assert target is not None
    _deal_damage_single(card, combat, target)


@register_effect(CardId.GRAVE_WARDEN)
def grave_warden(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    from sts2_env.cards.status import make_soul

    owner = _owner(card, combat)
    _gain_block(card, combat)
    for _ in range(max(0, card.effect_vars.get("cards", 1))):
        soul = make_soul(upgraded=card.upgraded)
        soul.owner = owner
        insert_at = combat.rng.next_int(0, len(combat.draw_pile))
        combat.draw_pile.insert(insert_at, soul)


@register_effect(CardId.GRAVEBLAST)
def graveblast(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    if not combat.discard_pile:
        return
    combat.request_card_choice(
        prompt="Choose a discard card to return to hand",
        cards=list(combat.discard_pile),
        source_pile="discard",
        resolver=combat.move_card_to_hand,
    )


@register_effect(CardId.INVOKE)
def invoke(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    energy = card.effect_vars.get("energy", 2)
    combat.apply_power_to(_owner(card, combat), PowerId.SUMMON_NEXT_TURN, 1)
    combat.apply_power_to(_owner(card, combat), PowerId.ENERGY_NEXT_TURN, energy)


@register_effect(CardId.NEGATIVE_PULSE)
def negative_pulse(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    doom = card.effect_vars.get("doom", 7)
    for enemy in list(combat.alive_enemies):
        combat.apply_power_to(enemy, PowerId.DOOM, doom)


@register_effect(CardId.POKE)
def poke(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """OstyAttack."""
    assert target is not None
    _deal_damage_single(card, combat, target)


@register_effect(CardId.PULL_AGGRO)
def pull_aggro(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    combat.summon_osty(_owner(card, combat), card.effect_vars.get("summon", 4))


@register_effect(CardId.REAP)
def reap(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)


@register_effect(CardId.REAVE)
def reave(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    from sts2_env.cards.status import make_soul

    assert target is not None
    _deal_damage_single(card, combat, target)
    for _ in range(max(0, card.effect_vars.get("cards", 1))):
        combat.insert_card_into_draw_pile(make_soul(upgraded=card.upgraded), random_position=True)


@register_effect(CardId.SCOURGE)
def scourge(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    doom = card.effect_vars.get("doom", 13)
    combat.apply_power_to(target, PowerId.DOOM, doom)
    cards = card.effect_vars.get("cards", 1)
    combat._draw_cards(cards)


@register_effect(CardId.SCULPTING_STRIKE)
def sculpting_strike(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    candidates = [c for c in combat.hand if not c.is_ethereal]
    if not candidates:
        return

    def _resolver(selected: CardInstance | None) -> None:
        if selected is None:
            return
        selected.keywords = frozenset(set(selected.keywords) | {"ethereal"})

    combat.request_card_choice(
        prompt="Choose a hand card to make Ethereal",
        cards=candidates,
        source_pile="hand",
        resolver=_resolver,
    )


@register_effect(CardId.SNAP)
def snap(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """OstyAttack — select card from hand."""
    assert target is not None
    _deal_damage_single(card, combat, target)


@register_effect(CardId.SOW)
def sow(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _deal_damage_all(card, combat)


@register_effect(CardId.WISP)
def wisp(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    energy = card.effect_vars.get("energy", 1)
    combat.energy += energy


# ---------------------------------------------------------------------------
# Uncommon cards (36)
# ---------------------------------------------------------------------------

@register_effect(CardId.BONE_SHARDS)
def bone_shards(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """OstyAttack — deal damage to all enemies, gain block, kill minion."""
    osty = _osty(card, combat)
    if osty is None or not osty.is_alive:
        return
    _deal_damage_all(card, combat)
    _gain_block(card, combat)
    combat.kill_osty(_owner(card, combat))


@register_effect(CardId.BORROWED_TIME)
def borrowed_time(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    doom = card.effect_vars.get("doom", 3)
    combat.apply_power_to(_owner(card, combat), PowerId.DOOM, doom)
    energy = card.effect_vars.get("energy", 1)
    combat.energy += energy


@register_effect(CardId.BURY)
def bury(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)


@register_effect(CardId.CALCIFY_CARD)
def calcify_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("calcify", 4)
    combat.apply_power_to(_owner(card, combat), PowerId.CALCIFY, amount)


@register_effect(CardId.CAPTURE_SPIRIT)
def capture_spirit(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    from sts2_env.cards.status import make_soul

    assert target is not None
    owner = _owner(card, combat)
    combat.deal_damage(
        dealer=owner,
        target=target,
        amount=card.effect_vars.get("damage", 3),
        props=ValueProp.UNBLOCKABLE | ValueProp.UNPOWERED | ValueProp.MOVE,
    )
    for _ in range(max(0, card.effect_vars.get("cards", 3))):
        combat.insert_card_into_draw_pile(make_soul(), random_position=True)


@register_effect(CardId.CLEANSE)
def cleanse(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.summon_osty(_owner(card, combat), card.effect_vars.get("summon", 3))
    cards = sorted(combat.draw_pile, key=lambda c: (c.rarity.value, c.card_id.value))
    if not cards:
        return
    combat.request_card_choice(
        prompt="Choose a draw-pile card to exhaust",
        cards=cards,
        source_pile="draw",
        resolver=combat.exhaust_card,
    )


@register_effect(CardId.COUNTDOWN_CARD)
def countdown_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("countdown", 6)
    combat.apply_power_to(_owner(card, combat), PowerId.COUNTDOWN, amount)


@register_effect(CardId.DANSE_MACABRE)
def danse_macabre(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("danse_macabre", 3)
    combat.apply_power_to(_owner(card, combat), PowerId.DANSE_MACABRE, amount)


@register_effect(CardId.DEATH_MARCH)
def death_march(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Damage scales with non-opening draws this turn."""
    assert target is not None
    owner = _owner(card, combat)
    base = card.effect_vars.get("calc_base", card.base_damage or 8)
    extra = card.effect_vars.get("extra_damage", 3)
    total_damage = base + extra * combat.count_non_hand_draws_this_turn(owner)
    damage = calculate_damage(total_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, damage, ValueProp.MOVE, combat, owner)


@register_effect(CardId.DEATHBRINGER)
def deathbringer(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    doom = card.effect_vars.get("doom", 21)
    for enemy in list(combat.alive_enemies):
        combat.apply_power_to(enemy, PowerId.DOOM, doom)
    weak = card.effect_vars.get("weak", 1)
    for enemy in list(combat.alive_enemies):
        combat.apply_power_to(enemy, PowerId.WEAK, weak)


@register_effect(CardId.DEATHS_DOOR)
def deaths_door(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)


@register_effect(CardId.DEBILITATE_CARD)
def debilitate_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    amount = card.effect_vars.get("debilitate", 3)
    combat.apply_power_to(target, PowerId.DEBILITATE, amount)


@register_effect(CardId.DELAY)
def delay(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    energy = card.effect_vars.get("energy", 1)
    combat.apply_power_to(_owner(card, combat), PowerId.ENERGY_NEXT_TURN, energy)


@register_effect(CardId.DIRGE)
def dirge(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """X-cost: creates Souls and summons minions based on energy spent."""
    from sts2_env.cards.status import make_soul

    x_value = getattr(card, "energy_spent", 0)
    if x_value <= 0:
        return
    summon_amount = card.effect_vars.get("summon", 3)
    for _ in range(x_value):
        combat.summon_osty(_owner(card, combat), summon_amount)
    for _ in range(x_value):
        soul = make_soul(upgraded=card.upgraded)
        combat.insert_card_into_draw_pile(soul, random_position=True)


@register_effect(CardId.DREDGE)
def dredge(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    max_cards = min(card.effect_vars.get("cards", 3), max(0, 10 - len(combat.hand)), len(combat.discard_pile))
    if max_cards <= 0:
        return
    combat.request_multi_card_choice(
        prompt="Choose discard cards to move to hand",
        cards=list(combat.discard_pile),
        source_pile="discard",
        resolver=lambda selected_cards: [combat.move_card_to_hand(selected) for selected in selected_cards],
        min_count=max_cards,
    )


@register_effect(CardId.ENFEEBLING_TOUCH)
def enfeebling_touch(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    strength_loss = card.effect_vars.get("strength_loss", 8)
    combat.apply_power_to(target, PowerId.ENFEEBLING_TOUCH, strength_loss)


@register_effect(CardId.FETCH)
def fetch(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """OstyAttack — draw cards."""
    osty = _osty(card, combat)
    if osty is None or not osty.is_alive:
        return
    assert target is not None
    _deal_damage_single(card, combat, target)
    if all(played is not card for played in combat._played_cards_this_turn):
        cards = card.effect_vars.get("cards", 1)
        combat._draw_cards(cards)


@register_effect(CardId.FRIENDSHIP)
def friendship(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    strength = card.effect_vars.get("strength", 2)
    combat.apply_power_to(_owner(card, combat), PowerId.STRENGTH, strength)
    combat.apply_power_to(_owner(card, combat), PowerId.FRIENDSHIP, 1)


@register_effect(CardId.HAUNT)
def haunt(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    hp_loss = card.effect_vars.get("hp_loss", 6)
    combat.apply_power_to(_owner(card, combat), PowerId.HAUNT, hp_loss)


@register_effect(CardId.HIGH_FIVE)
def high_five(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """OstyAttack — deal damage to all, apply vulnerable."""
    osty = _osty(card, combat)
    if osty is None or not osty.is_alive:
        return
    _deal_damage_all(card, combat)
    vuln = card.effect_vars.get("vulnerable", 2)
    for enemy in list(combat.alive_enemies):
        combat.apply_power_to(enemy, PowerId.VULNERABLE, vuln)


@register_effect(CardId.LEGION_OF_BONE)
def legion_of_bone(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    summon_amount = card.effect_vars.get("summon", 6)
    for state in combat.combat_player_states:
        owner = state.creature
        if owner.is_alive and getattr(owner, "is_player", False):
            combat.summon_osty(owner, summon_amount, source=card)


@register_effect(CardId.LETHALITY_CARD)
def lethality_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("lethality", 50)
    combat.apply_power_to(_owner(card, combat), PowerId.LETHALITY, amount)


@register_effect(CardId.MELANCHOLY)
def melancholy(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)


@register_effect(CardId.NO_ESCAPE)
def no_escape(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Apply Doom to target — scales with existing Doom stacks."""
    assert target is not None
    threshold = card.effect_vars.get("doom_threshold", 10)
    base_doom = card.effect_vars.get("calc_base", 10)
    extra = card.effect_vars.get("calc_extra", 5)
    multiplier = target.get_power_amount(PowerId.DOOM) // max(1, threshold)
    combat.apply_power_to(target, PowerId.DOOM, base_doom + extra * multiplier)


@register_effect(CardId.PAGESTORM)
def pagestorm(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.PAGESTORM, 1)


@register_effect(CardId.PARSE)
def parse(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    cards = card.effect_vars.get("cards", 3)
    combat._draw_cards(cards)


@register_effect(CardId.PULL_FROM_BELOW)
def pull_from_below(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Deal damage multiple times — scales with ethereal cards played this turn."""
    assert target is not None
    owner = _owner(card, combat)
    hits = sum(
        1
        for played in combat._played_cards_this_turn
        if getattr(played, "owner", None) is owner and played.is_ethereal
    )
    for _ in range(hits):
        damage = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
        apply_damage(target, damage, ValueProp.MOVE, combat, owner)
        if target.is_dead:
            break


@register_effect(CardId.PUTREFY)
def putrefy(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    amount = card.effect_vars.get("power", 2)
    combat.apply_power_to(target, PowerId.WEAK, amount)
    combat.apply_power_to(target, PowerId.VULNERABLE, amount)


@register_effect(CardId.RATTLE)
def rattle(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """OstyAttack — deal damage multiple times based on Osty's prior hits this turn."""
    osty = _osty(card, combat)
    if osty is None or not osty.is_alive:
        return
    assert target is not None
    owner = _owner(card, combat)
    hits = 1 + combat.count_powered_hits_by_dealer_this_turn(osty)
    for _ in range(hits):
        damage = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
        apply_damage(target, damage, ValueProp.MOVE, combat, owner)
        if target.is_dead:
            break


@register_effect(CardId.RIGHT_HAND_HAND)
def right_hand_hand(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """OstyAttack — deal damage; may return to hand after play."""
    assert target is not None
    _deal_damage_single(card, combat, target)


@register_late_effect(CardId.RIGHT_HAND_HAND)
def right_hand_hand_late(watched: CardInstance, played: CardInstance, combat: CombatState) -> None:
    if watched is not played:
        return
    owner = getattr(watched, "owner", None) or combat.primary_player
    owner_state = combat.combat_player_state_for(owner)
    if owner_state is None:
        return
    if owner_state.energy < watched.effect_vars.get("energy", 2):
        return
    if watched not in owner_state.discard:
        return
    combat.move_card_to_creature_hand(owner, watched)


@register_effect(CardId.SEVERANCE)
def severance(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    from sts2_env.cards.status import make_soul

    assert target is not None
    _deal_damage_single(card, combat, target)
    souls = [make_soul() for _ in range(3)]
    combat.insert_card_into_draw_pile(souls[0], random_position=True)
    combat.move_card_to_discard(souls[1])
    combat.move_card_to_hand(souls[2])


@register_effect(CardId.SHROUD)
def shroud(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    block = card.effect_vars.get("block", 2)
    combat.apply_power_to(_owner(card, combat), PowerId.SHROUD, block)


@register_effect(CardId.SIC_EM)
def sic_em(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """OstyAttack — apply SicEm debuff."""
    osty = _osty(card, combat)
    if osty is None or not osty.is_alive:
        return
    assert target is not None
    _deal_damage_single(card, combat, target)
    amount = card.effect_vars.get("sic_em", 2)
    combat.apply_power_to(target, PowerId.SIC_EM, amount)


@register_effect(CardId.SLEIGHT_OF_FLESH)
def sleight_of_flesh(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("sleight_of_flesh", 9)
    combat.apply_power_to(_owner(card, combat), PowerId.SLEIGHT_OF_FLESH, amount)


@register_effect(CardId.SPUR)
def spur(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    heal = card.effect_vars.get("heal", 5)
    combat.summon_osty(_owner(card, combat), card.effect_vars.get("summon", 3))
    osty = _osty(card, combat)
    if osty is not None:
        osty.heal(heal)


@register_effect(CardId.VEILPIERCER)
def veilpiercer(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    combat.apply_power_to(_owner(card, combat), PowerId.VEILPIERCER, 1)


# ---------------------------------------------------------------------------
# Rare cards (26)
# ---------------------------------------------------------------------------

@register_effect(CardId.BANSHEES_CRY)
def banshees_cry(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _deal_damage_all(card, combat)


@register_effect(CardId.CALL_OF_THE_VOID)
def call_of_the_void(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.CALL_OF_THE_VOID, 1)


@register_effect(CardId.DEMESNE)
def demesne(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.DEMESNE, 1)


@register_effect(CardId.DEVOUR_LIFE_CARD)
def devour_life_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("devour_life", 1)
    combat.apply_power_to(_owner(card, combat), PowerId.DEVOUR_LIFE, amount)


@register_effect(CardId.EIDOLON)
def eidolon(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    exhausted_count = len(combat.hand)
    for hand_card in list(combat.hand):
        combat.exhaust_card(hand_card)
    if exhausted_count >= 9:
        combat.apply_power_to(_owner(card, combat), PowerId.INTANGIBLE, 1)


@register_effect(CardId.END_OF_DAYS)
def end_of_days(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    doom = card.effect_vars.get("doom", 29)
    for enemy in list(combat.alive_enemies):
        combat.apply_power_to(enemy, PowerId.DOOM, doom)
    combat.kill_doomed_enemies()


@register_effect(CardId.ERADICATE)
def eradicate(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """X-cost — deal damage X times."""
    assert target is not None
    owner = _owner(card, combat)
    x = getattr(card, "energy_spent", 0)
    for _ in range(x):
        damage = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
        apply_damage(target, damage, ValueProp.MOVE, combat, owner)
        if target.is_dead:
            break


@register_effect(CardId.GLIMPSE_BEYOND)
def glimpse_beyond(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    from sts2_env.cards.status import make_soul

    for ally in combat.get_player_allies_of(_owner(card, combat)):
        for _ in range(max(0, card.effect_vars.get("cards", 3))):
            soul = make_soul()
            combat.insert_card_into_creature_draw_pile(ally, soul, random_position=True)


@register_effect(CardId.HANG)
def hang(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    combat.apply_power_to(target, PowerId.HANG, 1)


@register_effect(CardId.MISERY)
def misery(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)


@register_effect(CardId.NECRO_MASTERY_CARD)
def necro_mastery_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.NECRO_MASTERY, 1)
    combat.summon_osty(_owner(card, combat), card.effect_vars.get("summon", 5))


@register_effect(CardId.NEUROSURGE)
def neurosurge(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("neurosurge", 3)
    combat.apply_power_to(_owner(card, combat), PowerId.NEUROSURGE, amount)
    cards = card.effect_vars.get("cards", 2)
    combat._draw_cards(cards)
    energy = card.effect_vars.get("energy", 3)
    combat.energy += energy


@register_effect(CardId.OBLIVION)
def oblivion(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    doom = card.effect_vars.get("doom", 3)
    combat.apply_power_to(target, PowerId.OBLIVION, doom)


@register_effect(CardId.REANIMATE)
def reanimate(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.summon_osty(_owner(card, combat), card.effect_vars.get("summon", 20))


@register_effect(CardId.REAPER_FORM)
def reaper_form(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.REAPER_FORM, 1)


@register_effect(CardId.SACRIFICE)
def sacrifice(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    osty = combat.get_osty(owner)
    if osty is None or not osty.is_alive:
        return
    block_gain = osty.max_hp * 2
    combat.kill_osty(_owner(card, combat))
    owner.gain_block(block_gain)


@register_effect(CardId.SEANCE)
def seance(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    from sts2_env.cards.status import make_soul

    cards = sorted(combat.draw_pile, key=lambda c: (c.rarity.value, c.card_id.value))
    if not cards:
        return

    def _resolver(selected_cards: list[CardInstance]) -> None:
        for selected in selected_cards:
            transformed = make_soul(upgraded=card.upgraded)
            combat.transform_card(selected, transformed)

    combat.request_multi_card_choice(
        prompt="Choose card(s) to transform into Soul",
        cards=cards,
        source_pile="draw",
        resolver=_resolver,
        min_count=card.effect_vars.get("cards", 1),
    )


@register_effect(CardId.SENTRY_MODE)
def sentry_mode(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("sentry_mode", 1)
    combat.apply_power_to(_owner(card, combat), PowerId.SENTRY_MODE, amount)


@register_effect(CardId.SHARED_FATE)
def shared_fate(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    player_loss = card.effect_vars.get("player_strength_loss", 2)
    enemy_loss = card.effect_vars.get("enemy_strength_loss", 2)
    combat.apply_power_to(_owner(card, combat), PowerId.STRENGTH, -player_loss)
    combat.apply_power_to(target, PowerId.STRENGTH, -enemy_loss)


@register_effect(CardId.SOUL_STORM)
def soul_storm(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Damage scales with Souls in exhaust pile."""
    assert target is not None
    owner = _owner(card, combat)
    souls = sum(1 for exhausted in combat.exhaust_pile if exhausted.card_id == CardId.SOUL)
    base = card.effect_vars.get("calc_base", card.base_damage or 9)
    extra = card.effect_vars.get("extra_damage", 2)
    total_damage = base + extra * souls
    damage = calculate_damage(total_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, damage, ValueProp.MOVE, combat, owner)


@register_effect(CardId.SPIRIT_OF_ASH)
def spirit_of_ash(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    block_on_exhaust = card.effect_vars.get("block_on_exhaust", 4)
    combat.apply_power_to(_owner(card, combat), PowerId.SPIRIT_OF_ASH, block_on_exhaust)


@register_effect(CardId.SQUEEZE)
def squeeze(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """OstyAttack — scales with total cards."""
    osty = _osty(card, combat)
    if osty is None or not osty.is_alive:
        return
    assert target is not None
    owner = _owner(card, combat)
    count = sum(
        1
        for candidate in combat._all_cards_for_creature(owner)
        if candidate is not card and (
            CardTag.OSTY_ATTACK in getattr(candidate, "tags", ())
            or "osty_attack" in getattr(candidate, "tags", ())
        )
    )
    base = card.effect_vars.get("calc_base", card.base_damage or 25)
    extra = card.effect_vars.get("extra_damage", 5)
    total_damage = base + extra * count
    damage = calculate_damage(total_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, damage, ValueProp.MOVE, combat, owner)


@register_effect(CardId.THE_SCYTHE)
def the_scythe(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Damage permanently increases each play."""
    assert target is not None
    _deal_damage_single(card, combat, target)
    increase = card.effect_vars.get("increase", 3)
    if card.base_damage is not None:
        card.base_damage += increase


@register_effect(CardId.TIMES_UP)
def times_up(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Scales with DoomPower stacks."""
    assert target is not None
    owner = _owner(card, combat)
    doom = target.get_power_amount(PowerId.DOOM)
    base = card.effect_vars.get("calc_base", 0)
    extra = card.effect_vars.get("extra_damage", 1)
    total_damage = base + extra * doom
    damage = calculate_damage(total_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, damage, ValueProp.MOVE, combat, owner)


@register_effect(CardId.TRANSFIGURE)
def transfigure(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    if not combat.hand:
        return

    def _resolver(selected: CardInstance | None) -> None:
        if selected is None:
            return
        if not selected.has_energy_cost_x and selected.cost >= 0:
            selected.set_combat_cost(selected.cost + 1)
        selected.base_replay_count += 1

    combat.request_card_choice(
        prompt="Choose a hand card to grant Replay",
        cards=list(combat.hand),
        source_pile="hand",
        resolver=_resolver,
    )


@register_effect(CardId.UNDEATH)
def undeath(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    clone = card.clone(combat.rng.next_int(1, 2**31 - 1))
    combat.move_card_to_discard(clone)


# ---------------------------------------------------------------------------
# Ancient cards (2)
# ---------------------------------------------------------------------------

@register_effect(CardId.FORBIDDEN_GRIMOIRE)
def forbidden_grimoire(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.FORBIDDEN_GRIMOIRE, 1)


@register_effect(CardId.PROTECTOR)
def protector(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """OstyAttack — scales with total cards."""
    osty = _osty(card, combat)
    if osty is None or not osty.is_alive:
        return
    assert target is not None
    owner = _owner(card, combat)
    base = card.effect_vars.get("calc_base", card.base_damage or 10)
    extra = card.effect_vars.get("extra_damage", 1)
    total_damage = base + extra * osty.max_hp
    damage = calculate_damage(total_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, damage, ValueProp.MOVE, combat, owner)


# ---------------------------------------------------------------------------
# Card factories
# ---------------------------------------------------------------------------

def make_strike_necrobinder(upgraded: bool = False) -> CardInstance:
    dmg = 9 if upgraded else 6
    return CardInstance(
        card_id=CardId.STRIKE_NECROBINDER, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.BASIC,
        base_damage=dmg, upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_defend_necrobinder(upgraded: bool = False) -> CardInstance:
    blk = 8 if upgraded else 5
    return CardInstance(
        card_id=CardId.DEFEND_NECROBINDER, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.BASIC,
        base_block=blk, upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_bodyguard(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BODYGUARD, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.BASIC,
        upgraded=upgraded, effect_vars={"summon": 7 if upgraded else 5},
        instance_id=_get_next_id(),
    )


def make_unleash(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.UNLEASH, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.BASIC,
        base_damage=6 if not upgraded else 9,
        upgraded=upgraded, instance_id=_get_next_id(),
        effect_vars={"calc_base": 6 if not upgraded else 9, "extra_damage": 1},
    )


def make_afterlife(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.AFTERLIFE, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        keywords=frozenset({"exhaust"}), upgraded=upgraded,
        effect_vars={"summon": 9 if upgraded else 6},
        instance_id=_get_next_id(),
    )


def make_blight_strike(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BLIGHT_STRIKE, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=10 if upgraded else 8, upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_defile(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.DEFILE, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=17 if upgraded else 13, upgraded=upgraded,
        keywords=frozenset({"ethereal"}), instance_id=_get_next_id(),
    )


def make_defy(upgraded: bool = False) -> CardInstance:
    blk = 7 if upgraded else 6
    weak = 2 if upgraded else 1
    return CardInstance(
        card_id=CardId.DEFY, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_block=blk, upgraded=upgraded,
        keywords=frozenset({"ethereal"}),
        effect_vars={"weak": weak}, instance_id=_get_next_id(),
    )


def make_drain_power(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.DRAIN_POWER, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=12 if upgraded else 10, upgraded=upgraded,
        effect_vars={"cards": 3 if upgraded else 2},
        instance_id=_get_next_id(),
    )


def make_fear(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.FEAR, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=8 if upgraded else 7, upgraded=upgraded,
        keywords=frozenset({"ethereal"}),
        effect_vars={"vulnerable": 2 if upgraded else 1},
        instance_id=_get_next_id(),
    )


def make_flatten(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.FLATTEN, cost=2, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=0, upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_grave_warden(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.GRAVE_WARDEN, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        base_block=10 if upgraded else 8, upgraded=upgraded,
        effect_vars={"cards": 1}, instance_id=_get_next_id(),
    )


def make_graveblast(upgraded: bool = False) -> CardInstance:
    kw = frozenset() if upgraded else frozenset({"exhaust"})
    return CardInstance(
        card_id=CardId.GRAVEBLAST, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=6 if upgraded else 4, upgraded=upgraded,
        keywords=kw, instance_id=_get_next_id(),
    )


def make_invoke(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.INVOKE, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        upgraded=upgraded,
        effect_vars={"energy": 3 if upgraded else 2},
        instance_id=_get_next_id(),
    )


def make_negative_pulse(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.NEGATIVE_PULSE, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.ALL_ENEMIES, rarity=CardRarity.COMMON,
        base_block=6 if upgraded else 5, upgraded=upgraded,
        effect_vars={"doom": 11 if upgraded else 7},
        instance_id=_get_next_id(),
    )


def make_poke(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.POKE, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=3 if upgraded else 0, upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_pull_aggro(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.PULL_AGGRO, cost=2, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        base_block=9 if upgraded else 7, upgraded=upgraded,
        effect_vars={"summon": 5 if upgraded else 4},
        instance_id=_get_next_id(),
    )


def make_reap(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.REAP, cost=3, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=33 if upgraded else 27, upgraded=upgraded,
        keywords=frozenset({"retain"}), instance_id=_get_next_id(),
    )


def make_reave(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.REAVE, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=11 if upgraded else 9, upgraded=upgraded,
        effect_vars={"cards": 1}, instance_id=_get_next_id(),
    )


def make_scourge(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SCOURGE, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        upgraded=upgraded,
        effect_vars={"doom": 16 if upgraded else 13, "cards": 2 if upgraded else 1},
        instance_id=_get_next_id(),
    )


def make_sculpting_strike(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SCULPTING_STRIKE, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=11 if upgraded else 8, upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_snap(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SNAP, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=3 if upgraded else 0, upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_sow(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SOW, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ALL_ENEMIES, rarity=CardRarity.COMMON,
        base_damage=11 if upgraded else 8, upgraded=upgraded,
        keywords=frozenset({"retain"}), instance_id=_get_next_id(),
    )


def make_wisp(upgraded: bool = False) -> CardInstance:
    kw = frozenset({"exhaust", "retain"}) if upgraded else frozenset({"exhaust"})
    return CardInstance(
        card_id=CardId.WISP, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        upgraded=upgraded, keywords=kw,
        effect_vars={"energy": 1}, instance_id=_get_next_id(),
    )


def make_seance(upgraded: bool = False) -> CardInstance:
    kw = frozenset({"ethereal"})
    return CardInstance(
        card_id=CardId.SEANCE, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        upgraded=upgraded, keywords=kw,
        effect_vars={"cards": 1},
        instance_id=_get_next_id(),
    )


def make_capture_spirit(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.CAPTURE_SPIRIT, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        upgraded=upgraded,
        effect_vars={"damage": 4 if upgraded else 3, "cards": 4 if upgraded else 3},
        instance_id=_get_next_id(),
    )


def make_cleanse(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.CLEANSE, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        upgraded=upgraded,
        effect_vars={"summon": 5 if upgraded else 3},
        instance_id=_get_next_id(),
    )


def make_dredge(upgraded: bool = False) -> CardInstance:
    kw = frozenset({"exhaust", "retain"}) if upgraded else frozenset({"exhaust"})
    return CardInstance(
        card_id=CardId.DREDGE, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        upgraded=upgraded, keywords=kw,
        effect_vars={"cards": 3},
        instance_id=_get_next_id(),
    )


def make_dirge(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.DIRGE, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        upgraded=upgraded, has_energy_cost_x=True,
        effect_vars={"summon": 4 if upgraded else 3},
        instance_id=_get_next_id(),
    )


def make_end_of_days(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.END_OF_DAYS, cost=3, card_type=CardType.SKILL,
        target_type=TargetType.ALL_ENEMIES, rarity=CardRarity.RARE,
        upgraded=upgraded,
        effect_vars={"doom": 37 if upgraded else 29},
        instance_id=_get_next_id(),
    )


def make_undeath(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.UNDEATH, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        upgraded=upgraded, base_block=9 if upgraded else 7,
        instance_id=_get_next_id(),
    )


def make_reanimate(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.REANIMATE, cost=3, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        upgraded=upgraded, keywords=frozenset({"exhaust"}),
        effect_vars={"summon": 25 if upgraded else 20},
        instance_id=_get_next_id(),
    )


def make_necro_mastery(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.NECRO_MASTERY_CARD, cost=2, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        upgraded=upgraded,
        effect_vars={"summon": 8 if upgraded else 5},
        instance_id=_get_next_id(),
    )


def make_sacrifice(upgraded: bool = False) -> CardInstance:
    kw = frozenset({"retain"})
    return CardInstance(
        card_id=CardId.SACRIFICE, cost=0 if upgraded else 1,
        card_type=CardType.SKILL, target_type=TargetType.SELF,
        rarity=CardRarity.RARE, upgraded=upgraded, keywords=kw,
        instance_id=_get_next_id(),
    )


def make_legion_of_bone(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.LEGION_OF_BONE, cost=2, card_type=CardType.SKILL,
        target_type=TargetType.ALL_ALLIES, rarity=CardRarity.UNCOMMON,
        upgraded=upgraded, keywords=frozenset({"exhaust"}),
        effect_vars={"summon": 8 if upgraded else 6},
        instance_id=_get_next_id(),
    )


def make_severance(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SEVERANCE, cost=2, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        upgraded=upgraded, base_damage=18 if upgraded else 13,
        instance_id=_get_next_id(),
    )


def make_spur(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SPUR, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        upgraded=upgraded, keywords=frozenset({"retain"}),
        effect_vars={"summon": 5 if upgraded else 3, "heal": 7 if upgraded else 5},
        instance_id=_get_next_id(),
    )


def make_right_hand_hand(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.RIGHT_HAND_HAND, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        upgraded=upgraded, base_damage=6 if upgraded else 4,
        tags=frozenset({"osty_attack"}),
        effect_vars={"energy": 2},
        instance_id=_get_next_id(),
    )


def make_bone_shards(upgraded: bool = False) -> CardInstance:
    from sts2_env.cards.factory import create_reference_card

    card = create_reference_card(CardId.BONE_SHARDS, upgraded=upgraded, allow_generation=True)
    card.base_damage = 12 if upgraded else 9
    card.base_block = 12 if upgraded else 9
    return card


def make_death_march(upgraded: bool = False) -> CardInstance:
    from sts2_env.cards.factory import create_reference_card

    card = create_reference_card(CardId.DEATH_MARCH, upgraded=upgraded, allow_generation=True)
    card.base_damage = 9 if upgraded else 8
    card.effect_vars["calc_base"] = 9 if upgraded else 8
    card.effect_vars["extra_damage"] = 4 if upgraded else 3
    return card


def make_fetch(upgraded: bool = False) -> CardInstance:
    from sts2_env.cards.factory import create_reference_card

    card = create_reference_card(CardId.FETCH, upgraded=upgraded, allow_generation=True)
    card.base_damage = 6 if upgraded else 3
    return card


def make_high_five(upgraded: bool = False) -> CardInstance:
    from sts2_env.cards.factory import create_reference_card

    card = create_reference_card(CardId.HIGH_FIVE, upgraded=upgraded, allow_generation=True)
    card.base_damage = 13 if upgraded else 11
    return card


def make_no_escape(upgraded: bool = False) -> CardInstance:
    from sts2_env.cards.factory import create_reference_card

    card = create_reference_card(CardId.NO_ESCAPE, upgraded=upgraded, allow_generation=True)
    card.effect_vars["doom_threshold"] = 10
    card.effect_vars["calc_base"] = 15 if upgraded else 10
    card.effect_vars["calc_extra"] = 5
    return card


def make_pull_from_below(upgraded: bool = False) -> CardInstance:
    from sts2_env.cards.factory import create_reference_card

    card = create_reference_card(CardId.PULL_FROM_BELOW, upgraded=upgraded, allow_generation=True)
    card.base_damage = 7 if upgraded else 5
    return card


def make_rattle(upgraded: bool = False) -> CardInstance:
    from sts2_env.cards.factory import create_reference_card

    card = create_reference_card(CardId.RATTLE, upgraded=upgraded, allow_generation=True)
    card.base_damage = 9 if upgraded else 7
    return card


def make_sic_em(upgraded: bool = False) -> CardInstance:
    from sts2_env.cards.factory import create_reference_card

    card = create_reference_card(CardId.SIC_EM, upgraded=upgraded, allow_generation=True)
    card.base_damage = 6 if upgraded else 5
    card.effect_vars["sic_em"] = 3 if upgraded else 2
    return card


def make_eidolon(upgraded: bool = False) -> CardInstance:
    from sts2_env.cards.factory import create_reference_card

    return create_reference_card(CardId.EIDOLON, upgraded=upgraded, allow_generation=True)


def make_eradicate(upgraded: bool = False) -> CardInstance:
    from sts2_env.cards.factory import create_reference_card

    card = create_reference_card(CardId.ERADICATE, upgraded=upgraded, allow_generation=True)
    card.base_damage = 14 if upgraded else 11
    return card


def make_glimpse_beyond(upgraded: bool = False) -> CardInstance:
    from sts2_env.cards.factory import create_reference_card

    return create_reference_card(CardId.GLIMPSE_BEYOND, upgraded=upgraded, allow_generation=True)


def make_soul_storm(upgraded: bool = False) -> CardInstance:
    from sts2_env.cards.factory import create_reference_card

    card = create_reference_card(CardId.SOUL_STORM, upgraded=upgraded, allow_generation=True)
    card.base_damage = 10 if upgraded else 9
    card.effect_vars["calc_base"] = 9
    card.effect_vars["extra_damage"] = 3 if upgraded else 2
    return card


def make_squeeze(upgraded: bool = False) -> CardInstance:
    from sts2_env.cards.factory import create_reference_card

    card = create_reference_card(CardId.SQUEEZE, upgraded=upgraded, allow_generation=True)
    card.base_damage = 30 if upgraded else 25
    card.effect_vars["calc_base"] = 30 if upgraded else 25
    card.effect_vars["extra_damage"] = 6 if upgraded else 5
    return card


def make_the_scythe(upgraded: bool = False) -> CardInstance:
    from sts2_env.cards.factory import create_reference_card

    card = create_reference_card(CardId.THE_SCYTHE, upgraded=upgraded, allow_generation=True)
    card.base_damage = 13
    card.effect_vars["increase"] = 4 if upgraded else 3
    return card


def make_times_up(upgraded: bool = False) -> CardInstance:
    from sts2_env.cards.factory import create_reference_card

    card = create_reference_card(CardId.TIMES_UP, upgraded=upgraded, allow_generation=True)
    card.base_damage = 0
    card.effect_vars["calc_base"] = 0
    card.effect_vars["extra_damage"] = 1
    return card


def make_transfigure(upgraded: bool = False) -> CardInstance:
    from sts2_env.cards.factory import create_reference_card

    return create_reference_card(CardId.TRANSFIGURE, upgraded=upgraded, allow_generation=True)


def make_protector(upgraded: bool = False) -> CardInstance:
    from sts2_env.cards.factory import create_reference_card

    card = create_reference_card(CardId.PROTECTOR, upgraded=upgraded, allow_generation=True)
    card.base_damage = 15 if upgraded else 10
    card.effect_vars["calc_base"] = 15 if upgraded else 10
    card.effect_vars["extra_damage"] = 1
    return card


def _make_reference_factory_card(card_id: CardId, upgraded: bool = False) -> CardInstance:
    from sts2_env.cards.factory import create_reference_card

    return create_reference_card(card_id, upgraded=upgraded, allow_generation=True)


_REFERENCE_FACTORY_CARD_IDS = (
    CardId.BORROWED_TIME,
    CardId.BURY,
    CardId.CALCIFY_CARD,
    CardId.COUNTDOWN_CARD,
    CardId.DANSE_MACABRE,
    CardId.DEATHBRINGER,
    CardId.DEATHS_DOOR,
    CardId.DEBILITATE_CARD,
    CardId.DELAY,
    CardId.ENFEEBLING_TOUCH,
    CardId.FRIENDSHIP,
    CardId.HAUNT,
    CardId.LETHALITY_CARD,
    CardId.MELANCHOLY,
    CardId.PAGESTORM,
    CardId.PARSE,
    CardId.PUTREFY,
    CardId.SHROUD,
    CardId.SLEIGHT_OF_FLESH,
    CardId.VEILPIERCER,
    CardId.BANSHEES_CRY,
    CardId.CALL_OF_THE_VOID,
    CardId.DEMESNE,
    CardId.DEVOUR_LIFE_CARD,
    CardId.HANG,
    CardId.MISERY,
    CardId.NEUROSURGE,
    CardId.OBLIVION,
    CardId.REAPER_FORM,
    CardId.SENTRY_MODE,
    CardId.SHARED_FATE,
    CardId.SPIRIT_OF_ASH,
    CardId.FORBIDDEN_GRIMOIRE,
)

for _card_id in _REFERENCE_FACTORY_CARD_IDS:
    _factory_name = f"make_{_card_id.name.lower()}"
    if _factory_name in globals():
        continue

    def _factory(upgraded: bool = False, *, _cid: CardId = _card_id) -> CardInstance:
        return _make_reference_factory_card(_cid, upgraded=upgraded)

    _factory.__name__ = _factory_name
    globals()[_factory_name] = _factory


def create_necrobinder_starter_deck() -> list[CardInstance]:
    """Create the Necrobinder starting deck."""
    deck = []
    for _ in range(4):
        deck.append(make_strike_necrobinder())
    for _ in range(4):
        deck.append(make_defend_necrobinder())
    deck.append(make_bodyguard())
    deck.append(make_unleash())
    return deck
