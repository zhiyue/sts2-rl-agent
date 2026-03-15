"""Regent card effects and factories (88 cards).

Covers all Regent cards: Basic (4), Common (20), Uncommon (36),
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
    damage = calculate_damage(card.base_damage, combat.player, target, ValueProp.MOVE, combat)
    apply_damage(target, damage, ValueProp.MOVE, combat, combat.player)


def _deal_damage_all(card: CardInstance, combat: CombatState) -> None:
    for enemy in list(combat.alive_enemies):
        damage = calculate_damage(card.base_damage, combat.player, enemy, ValueProp.MOVE, combat)
        apply_damage(enemy, damage, ValueProp.MOVE, combat, combat.player)


def _gain_block(card: CardInstance, combat: CombatState) -> None:
    block = calculate_block(card.base_block, combat.player, ValueProp.MOVE, combat, card_source=card)
    combat.player.gain_block(block)


# ---------------------------------------------------------------------------
# Basic cards (4)
# ---------------------------------------------------------------------------

@register_effect(CardId.STRIKE_REGENT)
def strike_regent(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)


@register_effect(CardId.DEFEND_REGENT)
def defend_regent(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)


@register_effect(CardId.FALLING_STAR)
def falling_star(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    weak = card.effect_vars.get("weak", 1)
    vuln = card.effect_vars.get("vulnerable", 1)
    combat.apply_power_to(target, PowerId.WEAK, weak)
    combat.apply_power_to(target, PowerId.VULNERABLE, vuln)


@register_effect(CardId.VENERATE)
def venerate(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Gain Stars — stub (Stars are a Regent-specific resource)
    pass


# ---------------------------------------------------------------------------
# Common cards (20)
# ---------------------------------------------------------------------------

@register_effect(CardId.ASTRAL_PULSE)
def astral_pulse(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _deal_damage_all(card, combat)


@register_effect(CardId.BEGONE)
def begone(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    # Transform/create MinionDiveBomb — stub
    pass


@register_effect(CardId.CELESTIAL_MIGHT)
def celestial_might(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    hits = card.effect_vars.get("hits", 3)
    for _ in range(hits):
        damage = calculate_damage(card.base_damage, combat.player, target, ValueProp.MOVE, combat)
        apply_damage(target, damage, ValueProp.MOVE, combat, combat.player)
        if target.is_dead:
            break


@register_effect(CardId.CLOAK_OF_STARS)
def cloak_of_stars(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)


@register_effect(CardId.COLLISION_COURSE)
def collision_course(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    # Create Debris card and add to hand — stub
    pass


@register_effect(CardId.COSMIC_INDIFFERENCE)
def cosmic_indifference(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    # Choose from discard and add to draw pile — stub
    pass


@register_effect(CardId.CRESCENT_SPEAR)
def crescent_spear(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Scales with total cards — StarCost: 1."""
    assert target is not None
    _deal_damage_single(card, combat, target)


@register_effect(CardId.CRUSH_UNDER)
def crush_under(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _deal_damage_all(card, combat)
    strength_loss = card.effect_vars.get("strength_loss", 1)
    combat.apply_power_to(combat.player, PowerId.CRUSH_UNDER, strength_loss)


@register_effect(CardId.GATHER_LIGHT)
def gather_light(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    # Gain Stars — stub
    pass


@register_effect(CardId.GLITTERSTREAM)
def glitterstream(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    block_next = card.effect_vars.get("block_next", 0)
    if block_next:
        combat.apply_power_to(combat.player, PowerId.BLOCK_NEXT_TURN, block_next)


@register_effect(CardId.GLOW)
def glow(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    cards = card.effect_vars.get("cards", 2)
    combat._draw_cards(cards)
    # Gain Stars — stub
    pass


@register_effect(CardId.GUIDING_STAR)
def guiding_star(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    cards = card.effect_vars.get("cards", 2)
    combat.apply_power_to(combat.player, PowerId.DRAW_CARDS_NEXT_TURN, cards)


@register_effect(CardId.HIDDEN_CACHE)
def hidden_cache(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    star_next = card.effect_vars.get("star_next", 3)
    combat.apply_power_to(combat.player, PowerId.STAR_NEXT_TURN, star_next)
    # Gain Stars — stub
    pass


@register_effect(CardId.KNOW_THY_PLACE)
def know_thy_place(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    weak = card.effect_vars.get("weak", 1)
    vuln = card.effect_vars.get("vulnerable", 1)
    combat.apply_power_to(target, PowerId.WEAK, weak)
    combat.apply_power_to(target, PowerId.VULNERABLE, vuln)


@register_effect(CardId.PATTER)
def patter(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    vigor = card.effect_vars.get("vigor", 2)
    combat.apply_power_to(combat.player, PowerId.VIGOR, vigor)


@register_effect(CardId.PHOTON_CUT)
def photon_cut(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    cards = card.effect_vars.get("cards", 1)
    combat._draw_cards(cards)
    # Put card back on draw pile — stub
    pass


@register_effect(CardId.REFINE_BLADE)
def refine_blade(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    energy = card.effect_vars.get("energy", 1)
    combat.apply_power_to(combat.player, PowerId.ENERGY_NEXT_TURN, energy)
    # Forge — upgrade random card — stub
    pass


@register_effect(CardId.SOLAR_STRIKE)
def solar_strike(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    # Gain Stars — stub
    pass


@register_effect(CardId.SPOILS_OF_BATTLE)
def spoils_of_battle(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Forge — upgrade random card — stub
    pass


@register_effect(CardId.WROUGHT_IN_WAR)
def wrought_in_war(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    # Forge — stub
    pass


# ---------------------------------------------------------------------------
# Uncommon cards (36)
# ---------------------------------------------------------------------------

@register_effect(CardId.ALIGNMENT)
def alignment(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    energy = card.effect_vars.get("energy", 2)
    combat.energy += energy


@register_effect(CardId.BLACK_HOLE)
def black_hole(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("black_hole", 3)
    combat.apply_power_to(combat.player, PowerId.BLACK_HOLE, amount)


@register_effect(CardId.BULWARK)
def bulwark(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    # Forge — stub
    pass


@register_effect(CardId.CHARGE)
def charge(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Transform cards into MinionStrike — stub
    pass


@register_effect(CardId.CHILD_OF_THE_STARS)
def child_of_the_stars(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("block_for_stars", 2)
    combat.apply_power_to(combat.player, PowerId.CHILD_OF_THE_STARS, amount)


@register_effect(CardId.CONQUEROR)
def conqueror(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    combat.apply_power_to(target, PowerId.CONQUEROR, 1)
    # Forge — stub
    pass


@register_effect(CardId.CONVERGENCE)
def convergence(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(combat.player, PowerId.RETAIN_HAND, 1)
    energy = card.effect_vars.get("energy", 1)
    combat.apply_power_to(combat.player, PowerId.ENERGY_NEXT_TURN, energy)
    # StarNextTurn — stub
    pass


@register_effect(CardId.DEVASTATE)
def devastate(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)


@register_effect(CardId.FURNACE)
def furnace(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    forge = card.effect_vars.get("forge", 4)
    combat.apply_power_to(combat.player, PowerId.FURNACE_POWER, forge)


@register_effect(CardId.GAMMA_BLAST)
def gamma_blast(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    vuln = card.effect_vars.get("vulnerable", 2)
    weak = card.effect_vars.get("weak", 2)
    combat.apply_power_to(target, PowerId.WEAK, weak)
    combat.apply_power_to(target, PowerId.VULNERABLE, vuln)


@register_effect(CardId.GLIMMER)
def glimmer(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    cards = card.effect_vars.get("cards", 3)
    combat._draw_cards(cards)
    # Put card back on draw pile — stub
    pass


@register_effect(CardId.HEGEMONY)
def hegemony(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    energy = card.effect_vars.get("energy", 2)
    combat.apply_power_to(combat.player, PowerId.ENERGY_NEXT_TURN, energy)


@register_effect(CardId.KINGLY_KICK)
def kingly_kick(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)


@register_effect(CardId.KINGLY_PUNCH)
def kingly_punch(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    # Damage permanently increases — persistent state
    increase = card.effect_vars.get("increase", 3)
    if card.base_damage is not None:
        card.base_damage += increase


@register_effect(CardId.KNOCKOUT_BLOW)
def knockout_blow(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    # Gain Stars — stub
    pass


@register_effect(CardId.LARGESSE)
def largesse(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Upgrade and add generated cards to hand — stub
    pass


@register_effect(CardId.LUNAR_BLAST)
def lunar_blast(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Deal damage multiple times — scales with Stars spent."""
    assert target is not None
    hits = card.effect_vars.get("hits", 1)
    for _ in range(max(1, hits)):
        damage = calculate_damage(card.base_damage, combat.player, target, ValueProp.MOVE, combat)
        apply_damage(target, damage, ValueProp.MOVE, combat, combat.player)
        if target.is_dead:
            break


@register_effect(CardId.MANIFEST_AUTHORITY)
def manifest_authority(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    # Upgrade and add generated cards to hand — stub
    pass


@register_effect(CardId.MONOLOGUE_CARD)
def monologue_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(combat.player, PowerId.MONOLOGUE, 1)


@register_effect(CardId.ORBIT)
def orbit(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(combat.player, PowerId.ORBIT, 1)


@register_effect(CardId.PALE_BLUE_DOT)
def pale_blue_dot(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    cards = card.effect_vars.get("cards", 1)
    combat.apply_power_to(combat.player, PowerId.PALE_BLUE_DOT, cards)


@register_effect(CardId.PARRY_CARD)
def parry_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("parry", 6)
    combat.apply_power_to(combat.player, PowerId.PARRY, amount)


@register_effect(CardId.PARTICLE_WALL)
def particle_wall(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)


@register_effect(CardId.PILLAR_OF_CREATION)
def pillar_of_creation(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    block = card.effect_vars.get("block", 3)
    combat.apply_power_to(combat.player, PowerId.PILLAR_OF_CREATION, block)


@register_effect(CardId.PROPHESIZE)
def prophesize(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    cards = card.effect_vars.get("cards", 6)
    combat._draw_cards(cards)


@register_effect(CardId.QUASAR)
def quasar(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Upgrade, choose cards and add to hand — stub
    pass


@register_effect(CardId.RADIATE)
def radiate(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Deal damage to all enemies multiple times — scales."""
    hits = card.effect_vars.get("hits", 1)
    for _ in range(max(1, hits)):
        _deal_damage_all(card, combat)


@register_effect(CardId.REFLECT_CARD)
def reflect_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    combat.apply_power_to(combat.player, PowerId.REFLECT, 1)


@register_effect(CardId.RESONANCE)
def resonance(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    strength = card.effect_vars.get("strength", 1)
    combat.apply_power_to(combat.player, PowerId.STRENGTH, strength)
    # Additional strength application via StarCost — simplified
    combat.apply_power_to(combat.player, PowerId.STRENGTH, strength)


@register_effect(CardId.ROYAL_GAMBLE)
def royal_gamble(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Gain Stars — stub
    pass


@register_effect(CardId.SHINING_STRIKE)
def shining_strike(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    # Add card to draw pile, gain Stars — stub
    pass


@register_effect(CardId.SPECTRUM_SHIFT)
def spectrum_shift(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(combat.player, PowerId.SPECTRUM_SHIFT, 1)


@register_effect(CardId.STARDUST)
def stardust(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Deal damage multiple times to random enemy."""
    if target is None:
        return
    hits = card.effect_vars.get("hits", 3)
    for _ in range(hits):
        t = target if target.is_alive else None
        if t is None:
            alive = combat.alive_enemies
            if not alive:
                break
            t = combat.rng.choice(alive)
        damage = calculate_damage(card.base_damage, combat.player, t, ValueProp.MOVE, combat)
        apply_damage(t, damage, ValueProp.MOVE, combat, combat.player)


@register_effect(CardId.SUMMON_FORTH)
def summon_forth(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Add card to hand, Forge — stub
    pass


@register_effect(CardId.SUPERMASSIVE)
def supermassive(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Scales with forged cards."""
    assert target is not None
    _deal_damage_single(card, combat, target)


@register_effect(CardId.TERRAFORMING)
def terraforming(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    vigor = card.effect_vars.get("vigor", 6)
    combat.apply_power_to(combat.player, PowerId.VIGOR, vigor)


# ---------------------------------------------------------------------------
# Rare cards (26)
# ---------------------------------------------------------------------------

@register_effect(CardId.ARSENAL)
def arsenal(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("arsenal", 1)
    combat.apply_power_to(combat.player, PowerId.ARSENAL, amount)


@register_effect(CardId.BEAT_INTO_SHAPE)
def beat_into_shape(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    # Forge — stub
    pass


@register_effect(CardId.BIG_BANG)
def big_bang(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    cards = card.effect_vars.get("cards", 1)
    combat._draw_cards(cards)
    energy = card.effect_vars.get("energy", 1)
    combat.energy += energy
    # Gain Stars, Forge — stub
    pass


@register_effect(CardId.BOMBARDMENT)
def bombardment(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)


@register_effect(CardId.BUNDLE_OF_JOY)
def bundle_of_joy(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Add generated cards to hand — stub
    pass


@register_effect(CardId.COMET)
def comet(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    vuln = card.effect_vars.get("vulnerable", 3)
    weak = card.effect_vars.get("weak", 3)
    combat.apply_power_to(target, PowerId.WEAK, weak)
    combat.apply_power_to(target, PowerId.VULNERABLE, vuln)


@register_effect(CardId.CRASH_LANDING)
def crash_landing(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _deal_damage_all(card, combat)
    # Create Debris and add to hand — stub
    pass


@register_effect(CardId.DECISIONS_DECISIONS)
def decisions_decisions(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    cards = card.effect_vars.get("cards", 3)
    combat._draw_cards(cards)
    # Auto-play, select from hand — stub
    pass


@register_effect(CardId.DYING_STAR)
def dying_star(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _deal_damage_all(card, combat)
    strength_loss = card.effect_vars.get("strength_loss", 9)
    combat.apply_power_to(combat.player, PowerId.DYING_STAR, strength_loss)


@register_effect(CardId.FOREGONE_CONCLUSION)
def foregone_conclusion(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    cards = card.effect_vars.get("cards", 2)
    combat.apply_power_to(combat.player, PowerId.FOREGONE_CONCLUSION, cards)


@register_effect(CardId.GENESIS)
def genesis(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    stars = card.effect_vars.get("stars_per_turn", 2)
    combat.apply_power_to(combat.player, PowerId.GENESIS, stars)


@register_effect(CardId.GUARDS)
def guards(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Transform hand cards into MinionSacrifice — stub
    pass


@register_effect(CardId.HAMMER_TIME)
def hammer_time(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(combat.player, PowerId.HAMMER_TIME, 1)


@register_effect(CardId.HEAVENLY_DRILL)
def heavenly_drill(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """X-cost — deal damage X times."""
    assert target is not None
    x = card.effect_vars.get("x", 0)
    for _ in range(max(1, x)):
        damage = calculate_damage(card.base_damage, combat.player, target, ValueProp.MOVE, combat)
        apply_damage(target, damage, ValueProp.MOVE, combat, combat.player)
        if target.is_dead:
            break


@register_effect(CardId.HEIRLOOM_HAMMER)
def heirloom_hammer(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    # Add generated cards to hand, select from hand — stub
    pass


@register_effect(CardId.I_AM_INVINCIBLE)
def i_am_invincible(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)


@register_effect(CardId.MAKE_IT_SO)
def make_it_so(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)


@register_effect(CardId.MONARCHS_GAZE_CARD)
def monarchs_gaze_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(combat.player, PowerId.MONARCHS_GAZE, 1)


@register_effect(CardId.NEUTRON_AEGIS)
def neutron_aegis(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    plating = card.effect_vars.get("plating", 8)
    combat.apply_power_to(combat.player, PowerId.PLATING, plating)


@register_effect(CardId.ROYALTIES_CARD)
def royalties_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(combat.player, PowerId.ROYALTIES, 1)


@register_effect(CardId.SEEKING_EDGE)
def seeking_edge(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    forge = card.effect_vars.get("forge", 7)
    combat.apply_power_to(combat.player, PowerId.SEEKING_EDGE, forge)
    # Also forge immediately — stub
    pass


@register_effect(CardId.SEVEN_STARS)
def seven_stars(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Deal damage to all enemies 7 times."""
    for _ in range(7):
        _deal_damage_all(card, combat)


@register_effect(CardId.SWORD_SAGE)
def sword_sage(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(combat.player, PowerId.SWORD_SAGE, 1)


@register_effect(CardId.THE_SMITH)
def the_smith(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Forge — stub
    pass


@register_effect(CardId.TYRANNY_CARD)
def tyranny_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(combat.player, PowerId.TYRANNY, 1)


@register_effect(CardId.VOID_FORM)
def void_form(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("void_form", 2)
    combat.apply_power_to(combat.player, PowerId.VOID_FORM, amount)
    # End turn effect — stub
    pass


# ---------------------------------------------------------------------------
# Ancient cards (2)
# ---------------------------------------------------------------------------

@register_effect(CardId.METEOR_SHOWER)
def meteor_shower(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _deal_damage_all(card, combat)
    vuln = card.effect_vars.get("vulnerable", 2)
    weak = card.effect_vars.get("weak", 2)
    for enemy in list(combat.alive_enemies):
        combat.apply_power_to(enemy, PowerId.WEAK, weak)
        combat.apply_power_to(enemy, PowerId.VULNERABLE, vuln)


@register_effect(CardId.THE_SEALED_THRONE)
def the_sealed_throne(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(combat.player, PowerId.THE_SEALED_THRONE, 1)


# ---------------------------------------------------------------------------
# Card factories
# ---------------------------------------------------------------------------

def make_strike_regent(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.STRIKE_REGENT, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.BASIC,
        base_damage=9 if upgraded else 6, upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_defend_regent(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.DEFEND_REGENT, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.BASIC,
        base_block=8 if upgraded else 5, upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_falling_star(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.FALLING_STAR, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.BASIC,
        base_damage=11 if upgraded else 7, upgraded=upgraded,
        effect_vars={"weak": 1, "vulnerable": 1},
        star_cost=2,
        instance_id=_get_next_id(),
    )


def make_venerate(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.VENERATE, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.BASIC,
        upgraded=upgraded,
        effect_vars={"stars": 3 if upgraded else 2},
        instance_id=_get_next_id(),
    )


def make_astral_pulse(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.ASTRAL_PULSE, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ALL_ENEMIES, rarity=CardRarity.COMMON,
        base_damage=18 if upgraded else 14, upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_celestial_might(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.CELESTIAL_MIGHT, cost=2, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=8 if upgraded else 6, upgraded=upgraded,
        effect_vars={"hits": 3},
        instance_id=_get_next_id(),
    )


def make_cloak_of_stars(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.CLOAK_OF_STARS, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        base_block=10 if upgraded else 7, upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_collision_course(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.COLLISION_COURSE, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=12 if upgraded else 9, upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_cosmic_indifference(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.COSMIC_INDIFFERENCE, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        base_block=9 if upgraded else 6, upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_gather_light(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.GATHER_LIGHT, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        base_block=10 if upgraded else 7, upgraded=upgraded,
        effect_vars={"stars": 1},
        instance_id=_get_next_id(),
    )


def make_glow(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.GLOW, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        upgraded=upgraded,
        effect_vars={"cards": 2, "stars": 2 if upgraded else 1},
        instance_id=_get_next_id(),
    )


def make_guiding_star(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.GUIDING_STAR, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=13 if upgraded else 12, upgraded=upgraded,
        effect_vars={"cards": 3 if upgraded else 2},
        instance_id=_get_next_id(),
    )


def make_know_thy_place(upgraded: bool = False) -> CardInstance:
    kw = frozenset() if upgraded else frozenset({"exhaust"})
    return CardInstance(
        card_id=CardId.KNOW_THY_PLACE, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        upgraded=upgraded, keywords=kw,
        effect_vars={"weak": 1, "vulnerable": 1},
        instance_id=_get_next_id(),
    )


def make_patter(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.PATTER, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        base_block=10 if upgraded else 8, upgraded=upgraded,
        effect_vars={"vigor": 3 if upgraded else 2},
        instance_id=_get_next_id(),
    )


def make_solar_strike(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SOLAR_STRIKE, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=9 if upgraded else 8, upgraded=upgraded,
        effect_vars={"stars": 2 if upgraded else 1},
        instance_id=_get_next_id(),
    )


def create_regent_starter_deck() -> list[CardInstance]:
    """Create the Regent starting deck."""
    deck = []
    for _ in range(4):
        deck.append(make_strike_regent())
    for _ in range(4):
        deck.append(make_defend_regent())
    deck.append(make_falling_star())
    deck.append(make_venerate())
    return deck


def make_big_bang(upgraded: bool = False) -> CardInstance:
    kw = frozenset({"exhaust", "innate"}) if upgraded else frozenset({"exhaust"})
    return CardInstance(
        card_id=CardId.BIG_BANG, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        upgraded=upgraded, keywords=kw,
        effect_vars={"cards": 1, "energy": 1, "stars": 1, "forge": 5},
        instance_id=_get_next_id(),
    )
