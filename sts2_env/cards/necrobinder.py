"""Necrobinder card effects and factories (88 cards).

Covers all Necrobinder cards: Basic (4), Common (20), Uncommon (36),
Rare (26), Ancient (2).
"""

from __future__ import annotations

from sts2_env.cards.base import CardInstance, _get_next_id
from sts2_env.cards.registry import register_effect
from sts2_env.core.enums import (
    CardId, CardType, TargetType, CardRarity, ValueProp, PowerId,
)
from sts2_env.core.damage import calculate_damage, apply_damage, calculate_block
from sts2_env.core.creature import Creature
from sts2_env.core.combat import CombatState


# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

def _deal_damage_single(card: CardInstance, combat: CombatState, target: Creature) -> None:
    """Standard single-target damage."""
    damage = calculate_damage(card.base_damage, combat.player, target, ValueProp.MOVE, combat)
    apply_damage(target, damage, ValueProp.MOVE, combat, combat.player)


def _deal_damage_all(card: CardInstance, combat: CombatState) -> None:
    """Deal card.base_damage to all enemies."""
    for enemy in list(combat.alive_enemies):
        damage = calculate_damage(card.base_damage, combat.player, enemy, ValueProp.MOVE, combat)
        apply_damage(enemy, damage, ValueProp.MOVE, combat, combat.player)


def _gain_block(card: CardInstance, combat: CombatState) -> None:
    block = calculate_block(card.base_block, combat.player, ValueProp.MOVE, combat, card_source=card)
    combat.player.gain_block(block)


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
    # Summon minion — stub: minion summoning is handled by the minion system
    pass


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
    # Summon minion — stub
    pass


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
    # Upgrade random cards in draw pile — simplified stub
    pass


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
    _gain_block(card, combat)
    # Create Soul cards and add to draw pile — stub
    pass


@register_effect(CardId.GRAVEBLAST)
def graveblast(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    # Choose from exhaust pile and add to hand — stub (requires card grid UI)
    pass


@register_effect(CardId.INVOKE)
def invoke(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    energy = card.effect_vars.get("energy", 2)
    combat.apply_power_to(combat.player, PowerId.SUMMON_NEXT_TURN, 1)
    combat.apply_power_to(combat.player, PowerId.ENERGY_NEXT_TURN, energy)


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
    # Summon minion — stub
    pass


@register_effect(CardId.REAP)
def reap(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)


@register_effect(CardId.REAVE)
def reave(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    # Create Soul cards and add to draw pile — stub
    pass


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
    # Select card from hand to modify — stub (requires UI)
    pass


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
    _deal_damage_all(card, combat)
    _gain_block(card, combat)


@register_effect(CardId.BORROWED_TIME)
def borrowed_time(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    doom = card.effect_vars.get("doom", 3)
    combat.apply_power_to(combat.player, PowerId.DOOM, doom)
    energy = card.effect_vars.get("energy", 1)
    combat.energy += energy


@register_effect(CardId.BURY)
def bury(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)


@register_effect(CardId.CALCIFY_CARD)
def calcify_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("calcify", 4)
    combat.apply_power_to(combat.player, PowerId.CALCIFY, amount)


@register_effect(CardId.CAPTURE_SPIRIT)
def capture_spirit(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Deal non-attack damage, create Souls — stub
    pass


@register_effect(CardId.CLEANSE)
def cleanse(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Exhaust from hand, choose from grid, summon — stub
    pass


@register_effect(CardId.COUNTDOWN_CARD)
def countdown_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("countdown", 6)
    combat.apply_power_to(combat.player, PowerId.COUNTDOWN, amount)


@register_effect(CardId.DANSE_MACABRE)
def danse_macabre(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("danse_macabre", 3)
    combat.apply_power_to(combat.player, PowerId.DANSE_MACABRE, amount)


@register_effect(CardId.DEATH_MARCH)
def death_march(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Damage scales with exhaust pile size."""
    assert target is not None
    _deal_damage_single(card, combat, target)


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
    combat.apply_power_to(combat.player, PowerId.ENERGY_NEXT_TURN, energy)


@register_effect(CardId.DIRGE)
def dirge(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """X-cost: creates Souls and summons minions based on energy spent."""
    # X-cost card — energy already spent by combat system
    pass


@register_effect(CardId.DREDGE)
def dredge(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Choose from exhaust pile and add to discard — stub
    pass


@register_effect(CardId.ENFEEBLING_TOUCH)
def enfeebling_touch(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    strength_loss = card.effect_vars.get("strength_loss", 8)
    combat.apply_power_to(target, PowerId.ENFEEBLING_TOUCH, strength_loss)


@register_effect(CardId.FETCH)
def fetch(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """OstyAttack — draw cards."""
    assert target is not None
    _deal_damage_single(card, combat, target)
    cards = card.effect_vars.get("cards", 1)
    combat._draw_cards(cards)


@register_effect(CardId.FRIENDSHIP)
def friendship(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    strength = card.effect_vars.get("strength", 2)
    combat.apply_power_to(combat.player, PowerId.STRENGTH, strength)
    combat.apply_power_to(combat.player, PowerId.FRIENDSHIP, 1)


@register_effect(CardId.HAUNT)
def haunt(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    hp_loss = card.effect_vars.get("hp_loss", 6)
    combat.apply_power_to(combat.player, PowerId.HAUNT, hp_loss)


@register_effect(CardId.HIGH_FIVE)
def high_five(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """OstyAttack — deal damage to all, apply vulnerable."""
    _deal_damage_all(card, combat)
    vuln = card.effect_vars.get("vulnerable", 2)
    for enemy in list(combat.alive_enemies):
        combat.apply_power_to(enemy, PowerId.VULNERABLE, vuln)


@register_effect(CardId.LEGION_OF_BONE)
def legion_of_bone(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Summon multiple minions — stub
    pass


@register_effect(CardId.LETHALITY_CARD)
def lethality_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("lethality", 50)
    combat.apply_power_to(combat.player, PowerId.LETHALITY, amount)


@register_effect(CardId.MELANCHOLY)
def melancholy(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)


@register_effect(CardId.NO_ESCAPE)
def no_escape(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Apply Doom to target — scales with existing Doom stacks."""
    assert target is not None
    base_doom = card.effect_vars.get("doom_base", 10)
    combat.apply_power_to(target, PowerId.DOOM, base_doom)


@register_effect(CardId.PAGESTORM)
def pagestorm(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(combat.player, PowerId.PAGESTORM, 1)


@register_effect(CardId.PARSE)
def parse(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    cards = card.effect_vars.get("cards", 3)
    combat._draw_cards(cards)


@register_effect(CardId.PULL_FROM_BELOW)
def pull_from_below(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Deal damage multiple times — scales with minions."""
    assert target is not None
    hits = card.effect_vars.get("hits", 1)
    for _ in range(hits):
        damage = calculate_damage(card.base_damage, combat.player, target, ValueProp.MOVE, combat)
        apply_damage(target, damage, ValueProp.MOVE, combat, combat.player)
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
    """OstyAttack — deal damage multiple times, scales with minions."""
    assert target is not None
    hits = card.effect_vars.get("hits", 1)
    for _ in range(hits):
        damage = calculate_damage(card.base_damage, combat.player, target, ValueProp.MOVE, combat)
        apply_damage(target, damage, ValueProp.MOVE, combat, combat.player)
        if target.is_dead:
            break


@register_effect(CardId.RIGHT_HAND_HAND)
def right_hand_hand(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """OstyAttack — deal damage, gain energy."""
    assert target is not None
    _deal_damage_single(card, combat, target)
    # Energy gained is based on Osty system — simplified
    pass


@register_effect(CardId.SEVERANCE)
def severance(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    # Create Soul cards and add to hand — stub
    pass


@register_effect(CardId.SHROUD)
def shroud(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    block = card.effect_vars.get("block", 2)
    combat.apply_power_to(combat.player, PowerId.SHROUD, block)


@register_effect(CardId.SIC_EM)
def sic_em(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """OstyAttack — apply SicEm debuff."""
    assert target is not None
    _deal_damage_single(card, combat, target)
    amount = card.effect_vars.get("sic_em", 2)
    combat.apply_power_to(target, PowerId.SIC_EM, amount)


@register_effect(CardId.SLEIGHT_OF_FLESH)
def sleight_of_flesh(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("sleight_of_flesh", 9)
    combat.apply_power_to(combat.player, PowerId.SLEIGHT_OF_FLESH, amount)


@register_effect(CardId.SPUR)
def spur(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    heal = card.effect_vars.get("heal", 5)
    combat.player.heal(heal)
    # Summon minion — stub
    pass


@register_effect(CardId.VEILPIERCER)
def veilpiercer(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    combat.apply_power_to(combat.player, PowerId.VEILPIERCER, 1)


# ---------------------------------------------------------------------------
# Rare cards (26)
# ---------------------------------------------------------------------------

@register_effect(CardId.BANSHEES_CRY)
def banshees_cry(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _deal_damage_all(card, combat)


@register_effect(CardId.CALL_OF_THE_VOID)
def call_of_the_void(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(combat.player, PowerId.CALL_OF_THE_VOID, 1)


@register_effect(CardId.DEMESNE)
def demesne(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(combat.player, PowerId.DEMESNE, 1)


@register_effect(CardId.DEVOUR_LIFE_CARD)
def devour_life_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("devour_life", 1)
    combat.apply_power_to(combat.player, PowerId.DEVOUR_LIFE, amount)


@register_effect(CardId.EIDOLON)
def eidolon(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(combat.player, PowerId.INTANGIBLE, 1)
    # Exhaust cards from hand — stub (requires UI)
    pass


@register_effect(CardId.END_OF_DAYS)
def end_of_days(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    doom = card.effect_vars.get("doom", 29)
    for enemy in list(combat.alive_enemies):
        combat.apply_power_to(enemy, PowerId.DOOM, doom)
    # Kill minion — stub
    pass


@register_effect(CardId.ERADICATE)
def eradicate(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """X-cost — deal damage X times."""
    assert target is not None
    # X is the energy spent; energy was already consumed
    x = card.effect_vars.get("x", 0)
    for _ in range(max(1, x)):
        damage = calculate_damage(card.base_damage, combat.player, target, ValueProp.MOVE, combat)
        apply_damage(target, damage, ValueProp.MOVE, combat, combat.player)
        if target.is_dead:
            break


@register_effect(CardId.GLIMPSE_BEYOND)
def glimpse_beyond(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Create Soul cards and add to draw pile — stub
    pass


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
    combat.apply_power_to(combat.player, PowerId.NECRO_MASTERY, 1)
    # Summon minion — stub
    pass


@register_effect(CardId.NEUROSURGE)
def neurosurge(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("neurosurge", 3)
    combat.apply_power_to(combat.player, PowerId.NEUROSURGE, amount)
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
    # Summon powerful minion — stub
    pass


@register_effect(CardId.REAPER_FORM)
def reaper_form(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(combat.player, PowerId.REAPER_FORM, 1)


@register_effect(CardId.SACRIFICE)
def sacrifice(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    # Kill a minion — stub
    pass


@register_effect(CardId.SEANCE)
def seance(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Transform card into Soul, choose from grid — stub
    pass


@register_effect(CardId.SENTRY_MODE)
def sentry_mode(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("sentry_mode", 1)
    combat.apply_power_to(combat.player, PowerId.SENTRY_MODE, amount)


@register_effect(CardId.SHARED_FATE)
def shared_fate(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    player_loss = card.effect_vars.get("player_strength_loss", 2)
    enemy_loss = card.effect_vars.get("enemy_strength_loss", 2)
    combat.apply_power_to(combat.player, PowerId.STRENGTH, -player_loss)
    combat.apply_power_to(target, PowerId.STRENGTH, -enemy_loss)


@register_effect(CardId.SOUL_STORM)
def soul_storm(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Damage scales with Souls in exhaust pile."""
    assert target is not None
    _deal_damage_single(card, combat, target)


@register_effect(CardId.SPIRIT_OF_ASH)
def spirit_of_ash(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    block_on_exhaust = card.effect_vars.get("block_on_exhaust", 4)
    combat.apply_power_to(combat.player, PowerId.SPIRIT_OF_ASH, block_on_exhaust)


@register_effect(CardId.SQUEEZE)
def squeeze(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """OstyAttack — scales with total cards."""
    assert target is not None
    _deal_damage_single(card, combat, target)


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
    _deal_damage_single(card, combat, target)


@register_effect(CardId.TRANSFIGURE)
def transfigure(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Grant Replay to a card selected from hand — stub
    pass


@register_effect(CardId.UNDEATH)
def undeath(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    # Add generated cards to discard — stub
    pass


# ---------------------------------------------------------------------------
# Ancient cards (2)
# ---------------------------------------------------------------------------

@register_effect(CardId.FORBIDDEN_GRIMOIRE)
def forbidden_grimoire(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(combat.player, PowerId.FORBIDDEN_GRIMOIRE, 1)


@register_effect(CardId.PROTECTOR)
def protector(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """OstyAttack — scales with total cards."""
    assert target is not None
    _deal_damage_single(card, combat, target)


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
        upgraded=upgraded, instance_id=_get_next_id(),
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
