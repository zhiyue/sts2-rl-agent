"""Colorless card effects and factories (64 cards).

Covers all Colorless cards: Uncommon (39), Rare (25).
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
# Uncommon cards (39)
# ---------------------------------------------------------------------------

@register_effect(CardId.AUTOMATION)
def automation(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(combat.player, PowerId.AUTOMATION, 1)


@register_effect(CardId.BELIEVE_IN_YOU)
def believe_in_you(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    energy = card.effect_vars.get("energy", 3)
    combat.energy += energy


@register_effect(CardId.CATASTROPHE)
def catastrophe(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Auto-play cards from draw pile — stub
    pass


@register_effect(CardId.COORDINATE_CARD)
def coordinate_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    strength = card.effect_vars.get("strength", 5)
    if target is not None:
        combat.apply_power_to(target, PowerId.COORDINATE, strength)
    else:
        combat.apply_power_to(combat.player, PowerId.COORDINATE, strength)


@register_effect(CardId.DARK_SHACKLES)
def dark_shackles(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    strength_loss = card.effect_vars.get("strength_loss", 9)
    combat.apply_power_to(target, PowerId.DARK_SHACKLES, strength_loss)


@register_effect(CardId.DISCOVERY)
def discovery(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Choose cards and add to hand with cost 0 — stub
    pass


@register_effect(CardId.DRAMATIC_ENTRANCE)
def dramatic_entrance(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _deal_damage_all(card, combat)


@register_effect(CardId.EQUILIBRIUM)
def equilibrium(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    combat.apply_power_to(combat.player, PowerId.RETAIN_HAND, 1)


@register_effect(CardId.FASTEN)
def fasten(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    extra_block = card.effect_vars.get("extra_block", 5)
    combat.apply_power_to(combat.player, PowerId.FASTEN, extra_block)


@register_effect(CardId.FINESSE)
def finesse(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    cards = card.effect_vars.get("cards", 1)
    combat._draw_cards(cards)


@register_effect(CardId.FISTICUFFS)
def fisticuffs(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    # Gain Block equal to unblocked damage — simplified
    _gain_block(card, combat)


@register_effect(CardId.FLASH_OF_STEEL)
def flash_of_steel(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    cards = card.effect_vars.get("cards", 1)
    combat._draw_cards(cards)


@register_effect(CardId.GANG_UP)
def gang_up(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Scales with allies in combat."""
    assert target is not None
    _deal_damage_single(card, combat, target)


@register_effect(CardId.HUDDLE_UP)
def huddle_up(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    cards = card.effect_vars.get("cards", 2)
    combat._draw_cards(cards)


@register_effect(CardId.IMPATIENCE)
def impatience(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Draw cards only if no attacks in hand
    cards = card.effect_vars.get("cards", 2)
    has_attack = any(c.is_attack for c in combat.hand)
    if not has_attack:
        combat._draw_cards(cards)


@register_effect(CardId.INTERCEPT_CARD)
def intercept_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    if target is not None:
        combat.apply_power_to(target, PowerId.COVERED, 1)


@register_effect(CardId.JACK_OF_ALL_TRADES)
def jack_of_all_trades(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Add generated colorless card to hand — stub
    pass


@register_effect(CardId.LIFT)
def lift(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)


@register_effect(CardId.MIND_BLAST)
def mind_blast(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Damage scales with draw pile size."""
    assert target is not None
    _deal_damage_single(card, combat, target)


@register_effect(CardId.OMNISLICE)
def omnislice(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    # Deal non-attack damage twice
    hp_loss = card.effect_vars.get("damage", 8)
    target.lose_hp(hp_loss)
    target.lose_hp(hp_loss)


@register_effect(CardId.PANACHE_CARD)
def panache_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    damage = card.effect_vars.get("panache_damage", 10)
    combat.apply_power_to(combat.player, PowerId.PANACHE, damage)


@register_effect(CardId.PANIC_BUTTON)
def panic_button(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    turns = card.effect_vars.get("turns", 2)
    combat.apply_power_to(combat.player, PowerId.NO_BLOCK, turns)


@register_effect(CardId.PREP_TIME)
def prep_time(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("prep_time", 4)
    combat.apply_power_to(combat.player, PowerId.PREP_TIME, amount)


@register_effect(CardId.PRODUCTION)
def production(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    energy = card.effect_vars.get("energy", 2)
    combat.energy += energy


@register_effect(CardId.PROLONG)
def prolong(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Apply BlockNextTurn — carry current block to next turn
    block = combat.player.block
    if block > 0:
        combat.apply_power_to(combat.player, PowerId.BLOCK_NEXT_TURN, block)


@register_effect(CardId.PROWESS)
def prowess(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    strength = card.effect_vars.get("strength", 1)
    dexterity = card.effect_vars.get("dexterity", 1)
    combat.apply_power_to(combat.player, PowerId.STRENGTH, strength)
    combat.apply_power_to(combat.player, PowerId.DEXTERITY, dexterity)


@register_effect(CardId.PURITY)
def purity(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Exhaust cards from hand — stub (requires UI selection)
    pass


@register_effect(CardId.RESTLESSNESS)
def restlessness(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    cards = card.effect_vars.get("cards", 2)
    energy = card.effect_vars.get("energy", 2)
    combat._draw_cards(cards)
    combat.energy += energy


@register_effect(CardId.SEEKER_STRIKE)
def seeker_strike(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    # Choose from draw pile and add to hand — stub
    pass


@register_effect(CardId.SHOCKWAVE)
def shockwave(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("power", 3)
    for enemy in list(combat.alive_enemies):
        combat.apply_power_to(enemy, PowerId.WEAK, amount)
        combat.apply_power_to(enemy, PowerId.VULNERABLE, amount)


@register_effect(CardId.SPLASH)
def splash(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Upgrade cards, set cost to 0, choose and add to hand — stub
    pass


@register_effect(CardId.STRATAGEM)
def stratagem(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(combat.player, PowerId.STRATAGEM, 1)


@register_effect(CardId.TAG_TEAM)
def tag_team(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    combat.apply_power_to(target, PowerId.TAG_TEAM, 1)


@register_effect(CardId.THE_BOMB_CARD)
def the_bomb_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    damage = card.effect_vars.get("bomb_damage", 40)
    combat.apply_power_to(combat.player, PowerId.THE_BOMB, damage)


@register_effect(CardId.THINKING_AHEAD)
def thinking_ahead(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    cards = card.effect_vars.get("cards", 2)
    combat._draw_cards(cards)
    # Put card back on draw pile — stub
    pass


@register_effect(CardId.THRUMMING_HATCHET)
def thrumming_hatchet(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)


@register_effect(CardId.ULTIMATE_DEFEND)
def ultimate_defend(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)


@register_effect(CardId.ULTIMATE_STRIKE)
def ultimate_strike(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)


@register_effect(CardId.VOLLEY)
def volley(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """X-cost — deal damage X times to random enemy."""
    if target is None:
        return
    x = card.effect_vars.get("x", 0)
    for _ in range(max(1, x)):
        t = target if target.is_alive else None
        if t is None:
            alive = combat.alive_enemies
            if not alive:
                break
            t = combat.rng.choice(alive)
        damage = calculate_damage(card.base_damage, combat.player, t, ValueProp.MOVE, combat)
        apply_damage(t, damage, ValueProp.MOVE, combat, combat.player)


# ---------------------------------------------------------------------------
# Rare cards (25)
# ---------------------------------------------------------------------------

@register_effect(CardId.ALCHEMIZE)
def alchemize(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Generate a random potion — stub
    pass


@register_effect(CardId.ANOINTED)
def anointed(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Add card from exhaust pile to hand — stub
    pass


@register_effect(CardId.BEACON_OF_HOPE)
def beacon_of_hope(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(combat.player, PowerId.BEACON_OF_HOPE, 1)


@register_effect(CardId.BEAT_DOWN)
def beat_down(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Auto-play cards from draw pile — stub
    cards = card.effect_vars.get("cards", 3)
    pass


@register_effect(CardId.BOLAS)
def bolas(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)


@register_effect(CardId.CALAMITY_CARD)
def calamity_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(combat.player, PowerId.CALAMITY, 1)


@register_effect(CardId.ENTROPY)
def entropy(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(combat.player, PowerId.ENTROPY, 1)


@register_effect(CardId.ETERNAL_ARMOR)
def eternal_armor(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    plating = card.effect_vars.get("plating", 7)
    combat.apply_power_to(combat.player, PowerId.PLATING, plating)


@register_effect(CardId.GOLD_AXE)
def gold_axe(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Damage scales with gold."""
    assert target is not None
    _deal_damage_single(card, combat, target)


@register_effect(CardId.HAND_OF_GREED)
def hand_of_greed(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    # Gain gold — stub (gold is a run-level resource)
    pass


@register_effect(CardId.HIDDEN_GEM)
def hidden_gem(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Grant Replay to a card — stub
    pass


@register_effect(CardId.JACKPOT)
def jackpot(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    # Upgrade and add generated cards to hand — stub
    pass


@register_effect(CardId.KNOCKDOWN)
def knockdown(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    amount = card.effect_vars.get("knockdown", 2)
    combat.apply_power_to(target, PowerId.KNOCKDOWN, amount)


@register_effect(CardId.MASTER_OF_STRATEGY)
def master_of_strategy(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    cards = card.effect_vars.get("cards", 3)
    combat._draw_cards(cards)


@register_effect(CardId.MAYHEM_CARD)
def mayhem_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(combat.player, PowerId.MAYHEM, 1)


@register_effect(CardId.MIMIC)
def mimic(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Gain block scaling with ally block."""
    _gain_block(card, combat)


@register_effect(CardId.NOSTALGIA_CARD)
def nostalgia_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(combat.player, PowerId.NOSTALGIA, 1)


@register_effect(CardId.RALLY)
def rally(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)


@register_effect(CardId.REND)
def rend(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Scales with upgraded cards in deck."""
    assert target is not None
    _deal_damage_single(card, combat, target)


@register_effect(CardId.ROLLING_BOULDER)
def rolling_boulder(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("rolling_boulder", 5)
    combat.apply_power_to(combat.player, PowerId.ROLLING_BOULDER, amount)


@register_effect(CardId.SALVO)
def salvo(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    combat.apply_power_to(combat.player, PowerId.RETAIN_HAND, 1)


@register_effect(CardId.SCRAWL)
def scrawl(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Draw until hand is full
    from sts2_env.core.constants import MAX_HAND_SIZE
    to_draw = MAX_HAND_SIZE - len(combat.hand)
    if to_draw > 0:
        combat._draw_cards(to_draw)


@register_effect(CardId.SECRET_TECHNIQUE)
def secret_technique(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Choose a Skill from draw pile and put in hand — stub
    pass


@register_effect(CardId.SECRET_WEAPON)
def secret_weapon(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Choose an Attack from draw pile and put in hand — stub
    pass


@register_effect(CardId.THE_GAMBIT)
def the_gambit(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    combat.apply_power_to(combat.player, PowerId.THE_GAMBIT, 1)


# ---------------------------------------------------------------------------
# Card factories (selected key cards)
# ---------------------------------------------------------------------------

def make_dramatic_entrance(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.DRAMATIC_ENTRANCE, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ALL_ENEMIES, rarity=CardRarity.UNCOMMON,
        base_damage=15 if upgraded else 11, upgraded=upgraded,
        keywords=frozenset({"exhaust", "innate"}),
        instance_id=_get_next_id(),
    )


def make_finesse(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.FINESSE, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        base_block=7 if upgraded else 4, upgraded=upgraded,
        effect_vars={"cards": 1}, instance_id=_get_next_id(),
    )


def make_flash_of_steel(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.FLASH_OF_STEEL, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=8 if upgraded else 5, upgraded=upgraded,
        effect_vars={"cards": 1}, instance_id=_get_next_id(),
    )


def make_shockwave(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SHOCKWAVE, cost=2, card_type=CardType.SKILL,
        target_type=TargetType.ALL_ENEMIES, rarity=CardRarity.UNCOMMON,
        upgraded=upgraded, keywords=frozenset({"exhaust"}),
        effect_vars={"power": 5 if upgraded else 3},
        instance_id=_get_next_id(),
    )


def make_master_of_strategy(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.MASTER_OF_STRATEGY, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        upgraded=upgraded, keywords=frozenset({"exhaust"}),
        effect_vars={"cards": 4 if upgraded else 3},
        instance_id=_get_next_id(),
    )


def make_hand_of_greed(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.HAND_OF_GREED, cost=2, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.RARE,
        base_damage=25 if upgraded else 20, upgraded=upgraded,
        effect_vars={"gold": 25 if upgraded else 20},
        instance_id=_get_next_id(),
    )


def make_panic_button(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.PANIC_BUTTON, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        base_block=40 if upgraded else 30, upgraded=upgraded,
        keywords=frozenset({"exhaust"}),
        effect_vars={"turns": 2}, instance_id=_get_next_id(),
    )


def make_ultimate_strike(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.ULTIMATE_STRIKE, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=20 if upgraded else 14, upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_ultimate_defend(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.ULTIMATE_DEFEND, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        base_block=15 if upgraded else 11, upgraded=upgraded,
        instance_id=_get_next_id(),
    )
