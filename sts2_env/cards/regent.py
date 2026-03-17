"""Regent card effects and factories (88 cards).

Covers all Regent cards: Basic (4), Common (20), Uncommon (36),
Rare (26), Ancient (2).
"""

from __future__ import annotations

from sts2_env.cards.base import CardInstance, _get_next_id
from sts2_env.cards.factory import create_cards_from_ids, eligible_registered_cards
from sts2_env.cards.registry import register_effect, register_late_effect
from sts2_env.core.enums import (
    CardId, CardType, TargetType, CardRarity, ValueProp, PowerId,
)
from sts2_env.core.damage import calculate_damage, apply_damage, calculate_block
from sts2_env.core.creature import Creature
from sts2_env.core.combat import CombatState


def _owner(card: CardInstance, combat: CombatState) -> Creature:
    return getattr(card, "owner", None) or combat.player


# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

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
    combat.gain_stars(_owner(card, combat), card.effect_vars.get("stars", 2))


# ---------------------------------------------------------------------------
# Common cards (20)
# ---------------------------------------------------------------------------

@register_effect(CardId.ASTRAL_PULSE)
def astral_pulse(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _deal_damage_all(card, combat)


@register_effect(CardId.BEGONE)
def begone(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    from sts2_env.cards.status import make_minion_dive_bomb

    assert target is not None
    _deal_damage_single(card, combat, target)
    if not combat.hand:
        return

    def _resolver(selected: CardInstance | None) -> None:
        if selected is None:
            return
        transformed = make_minion_dive_bomb(upgraded=card.upgraded)
        combat.transform_card(selected, transformed)

    combat.request_card_choice(
        prompt="Choose a hand card to transform",
        cards=list(combat.hand),
        source_pile="hand",
        resolver=_resolver,
    )


@register_effect(CardId.CELESTIAL_MIGHT)
def celestial_might(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    hits = card.effect_vars.get("hits", 3)
    for _ in range(hits):
        damage = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
        apply_damage(target, damage, ValueProp.MOVE, combat, owner)
        if target.is_dead:
            break


@register_effect(CardId.CLOAK_OF_STARS)
def cloak_of_stars(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)


@register_effect(CardId.COLLISION_COURSE)
def collision_course(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    from sts2_env.cards.status import make_debris

    combat.move_card_to_hand(make_debris())


@register_effect(CardId.COSMIC_INDIFFERENCE)
def cosmic_indifference(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    if not combat.discard_pile:
        return
    combat.request_card_choice(
        prompt="Choose a discard card to put on draw pile",
        cards=list(combat.discard_pile),
        source_pile="discard",
        resolver=lambda selected: combat.insert_card_into_draw_pile(selected, random_position=False),
    )


@register_effect(CardId.CRESCENT_SPEAR)
def crescent_spear(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Scales with total cards — StarCost: 1."""
    assert target is not None
    owner = _owner(card, combat)
    count = sum(
        1
        for candidate in combat._all_cards_for_creature(owner)
        if candidate.star_cost > 0 or candidate.card_id == CardId.STARDUST
    )
    base = card.effect_vars.get("calc_base", card.base_damage or 6)
    extra = card.effect_vars.get("extra_damage", 2)
    total_damage = base + extra * count
    damage = calculate_damage(total_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, damage, ValueProp.MOVE, combat, owner)


@register_effect(CardId.CRUSH_UNDER)
def crush_under(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _deal_damage_all(card, combat)
    strength_loss = card.effect_vars.get("strength_loss", 1)
    combat.apply_power_to(_owner(card, combat), PowerId.CRUSH_UNDER, strength_loss)


@register_effect(CardId.GATHER_LIGHT)
def gather_light(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    combat.gain_stars(_owner(card, combat), card.effect_vars.get("stars", 1))


@register_effect(CardId.GLITTERSTREAM)
def glitterstream(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    block_next = card.effect_vars.get("block_next", 0)
    if block_next:
        combat.apply_power_to(_owner(card, combat), PowerId.BLOCK_NEXT_TURN, block_next)


@register_effect(CardId.GLOW)
def glow(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.gain_stars(_owner(card, combat), card.effect_vars.get("stars", 1))
    cards = card.effect_vars.get("cards", 2)
    combat._draw_cards(cards)


@register_effect(CardId.GUIDING_STAR)
def guiding_star(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    cards = card.effect_vars.get("cards", 2)
    combat.apply_power_to(_owner(card, combat), PowerId.DRAW_CARDS_NEXT_TURN, cards)


@register_effect(CardId.HIDDEN_CACHE)
def hidden_cache(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    star_next = card.effect_vars.get("star_next", 3)
    combat.apply_power_to(_owner(card, combat), PowerId.STAR_NEXT_TURN, star_next)
    combat.gain_stars(_owner(card, combat), card.effect_vars.get("stars", 1))


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
    combat.apply_power_to(_owner(card, combat), PowerId.VIGOR, vigor)


@register_effect(CardId.PHOTON_CUT)
def photon_cut(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    cards = card.effect_vars.get("cards", 1)
    combat._draw_cards(cards)
    candidates = list(combat.hand)
    if not candidates:
        return
    combat.request_card_choice(
        prompt="Choose a hand card to put back on top of draw pile",
        cards=candidates,
        source_pile="hand",
        resolver=lambda selected: combat.insert_card_into_draw_pile(selected, random_position=False),
    )


@register_effect(CardId.REFINE_BLADE)
def refine_blade(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    energy = card.effect_vars.get("energy", 1)
    combat.apply_power_to(_owner(card, combat), PowerId.ENERGY_NEXT_TURN, energy)
    combat.forge(_owner(card, combat), card.effect_vars.get("forge", 6), source=card)


@register_effect(CardId.SOLAR_STRIKE)
def solar_strike(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    combat.gain_stars(_owner(card, combat), card.effect_vars.get("stars", 1))


@register_effect(CardId.SPOILS_OF_BATTLE)
def spoils_of_battle(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.forge(_owner(card, combat), card.effect_vars.get("forge", 10), source=card)


@register_effect(CardId.WROUGHT_IN_WAR)
def wrought_in_war(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    combat.forge(_owner(card, combat), card.effect_vars.get("forge", 5), source=card)


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
    combat.apply_power_to(_owner(card, combat), PowerId.BLACK_HOLE, amount)


@register_effect(CardId.BULWARK)
def bulwark(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    combat.forge(_owner(card, combat), card.effect_vars.get("forge", 10), source=card)


@register_effect(CardId.CHARGE)
def charge(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    from sts2_env.cards.status import make_minion_strike

    cards = sorted(combat.draw_pile, key=lambda c: (c.rarity.value, c.card_id.value))
    if not cards:
        return

    def _resolver(selected_cards: list[CardInstance]) -> None:
        for selected in selected_cards:
            transformed = make_minion_strike(upgraded=card.upgraded)
            combat.transform_card(selected, transformed)

    combat.request_multi_card_choice(
        prompt="Choose card(s) to transform into Minion Strike",
        cards=cards,
        source_pile="draw",
        resolver=_resolver,
        min_count=card.effect_vars.get("cards", 2),
    )


@register_effect(CardId.CHILD_OF_THE_STARS)
def child_of_the_stars(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("block_for_stars", 2)
    combat.apply_power_to(_owner(card, combat), PowerId.CHILD_OF_THE_STARS, amount)


@register_effect(CardId.CONQUEROR)
def conqueror(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    combat.apply_power_to(target, PowerId.CONQUEROR, 1)
    combat.forge(_owner(card, combat), card.effect_vars.get("forge", 3), source=card)


@register_effect(CardId.CONVERGENCE)
def convergence(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.RETAIN_HAND, 1)
    energy = card.effect_vars.get("energy", 1)
    combat.apply_power_to(_owner(card, combat), PowerId.ENERGY_NEXT_TURN, energy)
    combat.apply_power_to(_owner(card, combat), PowerId.STAR_NEXT_TURN, card.effect_vars.get("stars", 1))


@register_effect(CardId.DEVASTATE)
def devastate(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)


@register_effect(CardId.FURNACE)
def furnace(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    forge = card.effect_vars.get("forge", 4)
    combat.apply_power_to(_owner(card, combat), PowerId.FURNACE, forge)


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
    if not combat.hand:
        return
    combat.request_card_choice(
        prompt="Choose a hand card to put back on draw pile",
        cards=list(combat.hand),
        source_pile="hand",
        resolver=lambda selected: combat.insert_card_into_draw_pile(selected, random_position=False),
    )


@register_effect(CardId.HEGEMONY)
def hegemony(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    energy = card.effect_vars.get("energy", 2)
    combat.apply_power_to(_owner(card, combat), PowerId.ENERGY_NEXT_TURN, energy)


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
    if target.is_dead:
        combat.gain_stars(_owner(card, combat), card.effect_vars.get("stars", 5))


@register_effect(CardId.LARGESSE)
def largesse(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    if target is None:
        return
    colorless_ids = eligible_registered_cards(module_name="sts2_env.cards.colorless")
    generated = create_cards_from_ids(colorless_ids, combat.rng, 1, distinct=True)
    if not generated:
        return
    if card.upgraded:
        combat.upgrade_card(generated[0])
    combat.move_card_to_creature_hand(target, generated[0])


@register_effect(CardId.LUNAR_BLAST)
def lunar_blast(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Deal damage multiple times — scales with Skills played this turn."""
    assert target is not None
    owner = _owner(card, combat)
    hits = combat.count_cards_played_this_turn(owner, card_type=CardType.SKILL)
    for _ in range(hits):
        damage = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
        apply_damage(target, damage, ValueProp.MOVE, combat, owner)
        if target.is_dead:
            break


@register_effect(CardId.MANIFEST_AUTHORITY)
def manifest_authority(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    colorless_ids = eligible_registered_cards(module_name="sts2_env.cards.colorless")
    generated = create_cards_from_ids(colorless_ids, combat.rng, 1, distinct=True)
    if generated:
        if card.upgraded:
            combat.upgrade_card(generated[0])
        combat.move_card_to_hand(generated[0])


@register_effect(CardId.MONOLOGUE_CARD)
def monologue_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.MONOLOGUE, 1)


@register_effect(CardId.ORBIT)
def orbit(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.ORBIT, 1)


@register_effect(CardId.PALE_BLUE_DOT)
def pale_blue_dot(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    cards = card.effect_vars.get("cards", 1)
    combat.apply_power_to(_owner(card, combat), PowerId.PALE_BLUE_DOT, cards)


@register_effect(CardId.PARRY_CARD)
def parry_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("parry", 6)
    combat.apply_power_to(_owner(card, combat), PowerId.PARRY, amount)


@register_effect(CardId.PARTICLE_WALL)
def particle_wall(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)


@register_effect(CardId.PILLAR_OF_CREATION)
def pillar_of_creation(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    block = card.effect_vars.get("block", 3)
    combat.apply_power_to(_owner(card, combat), PowerId.PILLAR_OF_CREATION, block)


@register_effect(CardId.PROPHESIZE)
def prophesize(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    cards = card.effect_vars.get("cards", 6)
    combat._draw_cards(cards)


@register_effect(CardId.QUASAR)
def quasar(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    colorless_ids = eligible_registered_cards(module_name="sts2_env.cards.colorless")
    generated = create_cards_from_ids(colorless_ids, combat.rng, 3, distinct=True)
    for generated_card in generated:
        if card.upgraded:
            combat.upgrade_card(generated_card)
    if not generated:
        return
    combat.request_card_choice(
        prompt="Choose one colorless card",
        cards=generated,
        source_pile="generated",
        resolver=combat.move_card_to_hand,
        allow_skip=True,
    )


@register_effect(CardId.RADIATE)
def radiate(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Deal damage to all enemies multiple times — scales."""
    hits = combat.count_stars_gained_this_turn(_owner(card, combat))
    for _ in range(hits):
        _deal_damage_all(card, combat)


@register_effect(CardId.REFLECT_CARD)
def reflect_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    combat.apply_power_to(_owner(card, combat), PowerId.REFLECT, 1)


@register_effect(CardId.RESONANCE)
def resonance(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    strength = card.effect_vars.get("strength", 1)
    combat.apply_power_to(_owner(card, combat), PowerId.STRENGTH, strength)
    for enemy in list(combat.alive_enemies):
        combat.apply_power_to(enemy, PowerId.STRENGTH, -1)


@register_effect(CardId.ROYAL_GAMBLE)
def royal_gamble(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.gain_stars(_owner(card, combat), card.effect_vars.get("stars", 9))


@register_effect(CardId.SHINING_STRIKE)
def shining_strike(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    combat.gain_stars(_owner(card, combat), card.effect_vars.get("stars", 2))
    if not card.exhausts:
        combat.insert_card_into_draw_pile(card, random_position=False)


@register_effect(CardId.SPECTRUM_SHIFT)
def spectrum_shift(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.SPECTRUM_SHIFT, 1)


@register_effect(CardId.STARDUST)
def stardust(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Deal damage multiple times to random enemy using all current Stars."""
    owner = _owner(card, combat)
    owner_state = combat.combat_player_state_for(owner)
    hits = owner_state.stars if owner_state is not None else combat.stars
    if hits <= 0:
        return
    combat.spend_stars(owner, hits)
    for _ in range(hits):
        alive = combat.alive_enemies
        if not alive:
            break
        t = combat.rng.choice(alive)
        damage = calculate_damage(card.base_damage, owner, t, ValueProp.MOVE, combat)
        apply_damage(t, damage, ValueProp.MOVE, combat, owner)


@register_effect(CardId.SUMMON_FORTH)
def summon_forth(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    combat.forge(owner, card.effect_vars.get("forge", 8), source=card)
    for pile in (combat.draw_pile, combat.discard_pile, combat.exhaust_pile):
        for blade in [
            c for c in list(pile)
            if c.card_id == CardId.SOVEREIGN_BLADE
            and getattr(c, "owner", None) in (None, owner)
        ]:
            combat.move_card_to_creature_hand(owner, blade)


@register_effect(CardId.SUPERMASSIVE)
def supermassive(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Scales with forged cards."""
    assert target is not None
    owner = _owner(card, combat)
    base = card.effect_vars.get("calc_base", card.base_damage or 5)
    extra = card.effect_vars.get("extra_damage", 3)
    total_damage = base + extra * combat.count_generated_cards_this_combat(owner)
    damage = calculate_damage(total_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, damage, ValueProp.MOVE, combat, owner)


@register_effect(CardId.TERRAFORMING)
def terraforming(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    vigor = card.effect_vars.get("vigor", 6)
    combat.apply_power_to(_owner(card, combat), PowerId.VIGOR, vigor)


# ---------------------------------------------------------------------------
# Rare cards (26)
# ---------------------------------------------------------------------------

@register_effect(CardId.ARSENAL)
def arsenal(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("arsenal", 1)
    combat.apply_power_to(_owner(card, combat), PowerId.ARSENAL, amount)


@register_effect(CardId.BEAT_INTO_SHAPE)
def beat_into_shape(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    prior_hits = combat.count_powered_hits_this_turn(owner, target)
    _deal_damage_single(card, combat, target)
    forge_amount = card.effect_vars.get("forge_base", 5) + prior_hits * card.effect_vars.get("forge_per_hit", 5)
    combat.forge(owner, forge_amount, source=card)


@register_effect(CardId.BIG_BANG)
def big_bang(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    cards = card.effect_vars.get("cards", 1)
    combat._draw_cards(cards)
    energy = card.effect_vars.get("energy", 1)
    combat.energy += energy
    combat.gain_stars(_owner(card, combat), card.effect_vars.get("stars", 1))
    combat.forge(_owner(card, combat), card.effect_vars.get("forge", 5), source=card)


@register_effect(CardId.BOMBARDMENT)
def bombardment(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)


@register_effect(CardId.BUNDLE_OF_JOY)
def bundle_of_joy(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    colorless_ids = eligible_registered_cards(module_name="sts2_env.cards.colorless")
    generated = create_cards_from_ids(
        colorless_ids,
        combat.rng,
        card.effect_vars.get("cards", 3),
        distinct=True,
    )
    for generated_card in generated:
        combat.move_card_to_hand(generated_card)


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
    from sts2_env.cards.status import make_debris

    to_add = max(0, 10 - len(combat.hand))
    for _ in range(to_add):
        combat.move_card_to_hand(make_debris())


@register_effect(CardId.DECISIONS_DECISIONS)
def decisions_decisions(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    cards = card.effect_vars.get("cards", 3)
    combat._draw_cards(cards)
    candidates = [c for c in combat.hand if c.card_type == CardType.SKILL and not c.is_unplayable]
    if not candidates:
        return

    def _resolver(selected: CardInstance | None) -> None:
        if selected is None:
            return
        for _ in range(card.effect_vars.get("repeat", 3)):
            if combat.is_over:
                break
            combat.auto_play_card(selected)

    combat.request_card_choice(
        prompt="Choose a Skill to auto-play repeatedly",
        cards=candidates,
        source_pile="hand",
        resolver=_resolver,
    )


@register_effect(CardId.DYING_STAR)
def dying_star(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _deal_damage_all(card, combat)
    strength_loss = card.effect_vars.get("strength_loss", 9)
    for enemy in list(combat.alive_enemies):
        combat.apply_power_to(enemy, PowerId.DYING_STAR, strength_loss)


@register_effect(CardId.FOREGONE_CONCLUSION)
def foregone_conclusion(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    cards = card.effect_vars.get("cards", 2)
    combat.apply_power_to(_owner(card, combat), PowerId.FOREGONE_CONCLUSION, cards)


@register_effect(CardId.GENESIS)
def genesis(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    stars = card.effect_vars.get("stars_per_turn", 2)
    combat.apply_power_to(_owner(card, combat), PowerId.GENESIS, stars)


@register_effect(CardId.GUARDS)
def guards(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    from sts2_env.cards.status import make_minion_sacrifice

    if not combat.hand:
        return

    def _resolver(selected_cards: list[CardInstance]) -> None:
        for selected in selected_cards:
            transformed = make_minion_sacrifice(upgraded=card.upgraded)
            combat.transform_card(selected, transformed)

    combat.request_multi_card_choice(
        prompt="Choose any number of hand cards to transform",
        cards=list(combat.hand),
        source_pile="hand",
        resolver=_resolver,
        min_count=0,
        max_count=len(combat.hand),
        allow_skip=True,
    )


@register_effect(CardId.HAMMER_TIME)
def hammer_time(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.HAMMER_TIME, 1)


@register_effect(CardId.HEAVENLY_DRILL)
def heavenly_drill(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """X-cost — deal damage X times."""
    assert target is not None
    owner = _owner(card, combat)
    x = getattr(card, "energy_spent", 0)
    if x >= card.effect_vars.get("energy", 4):
        x *= 2
    for _ in range(x):
        damage = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
        apply_damage(target, damage, ValueProp.MOVE, combat, owner)
        if target.is_dead:
            break


@register_effect(CardId.HEIRLOOM_HAMMER)
def heirloom_hammer(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    colorless_ids = set(eligible_registered_cards(module_name="sts2_env.cards.colorless"))
    candidates = [c for c in combat.hand if c.card_id in colorless_ids]
    if not candidates:
        return

    def _resolver(selected: CardInstance | None) -> None:
        if selected is None:
            return
        copies = card.effect_vars.get("repeat", 1)
        clones = [selected.clone(combat.rng.next_int(1, 2**31 - 1)) for _ in range(copies)]
        combat._add_generated_cards_to_hand(clones)  # noqa: SLF001

    combat.request_card_choice(
        prompt="Choose a colorless card to copy",
        cards=candidates,
        source_pile="hand",
        resolver=_resolver,
    )


@register_effect(CardId.I_AM_INVINCIBLE)
def i_am_invincible(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)


@register_effect(CardId.MAKE_IT_SO)
def make_it_so(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)


@register_late_effect(CardId.MAKE_IT_SO)
def make_it_so_late(watched: CardInstance, played: CardInstance, combat: CombatState) -> None:
    watched_owner = getattr(watched, "owner", None) or combat.primary_player
    played_owner = getattr(played, "owner", None) or combat.primary_player
    if played_owner is not watched_owner:
        return
    if played.card_type != CardType.SKILL:
        return
    if watched in combat.hand:
        return
    interval = watched.effect_vars.get("cards", 3)
    if interval <= 0:
        return
    if combat.count_cards_played_this_turn(watched_owner, card_type=CardType.SKILL) % interval == 0:
        combat.move_card_to_hand(watched)


@register_effect(CardId.MONARCHS_GAZE_CARD)
def monarchs_gaze_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.MONARCHS_GAZE, 1)


@register_effect(CardId.NEUTRON_AEGIS)
def neutron_aegis(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    plating = card.effect_vars.get("plating", 8)
    combat.apply_power_to(_owner(card, combat), PowerId.PLATING, plating)


@register_effect(CardId.ROYALTIES_CARD)
def royalties_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.ROYALTIES, 1)


@register_effect(CardId.SEEKING_EDGE)
def seeking_edge(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    forge = card.effect_vars.get("forge", 7)
    combat.forge(_owner(card, combat), forge, source=card)
    combat.apply_power_to(_owner(card, combat), PowerId.SEEKING_EDGE, 1)


@register_effect(CardId.SEVEN_STARS)
def seven_stars(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Deal damage to all enemies 7 times."""
    for _ in range(7):
        _deal_damage_all(card, combat)


@register_effect(CardId.SWORD_SAGE)
def sword_sage(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.SWORD_SAGE, 1)


@register_effect(CardId.THE_SMITH)
def the_smith(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.forge(_owner(card, combat), card.effect_vars.get("forge", 30), source=card)


@register_effect(CardId.TYRANNY_CARD)
def tyranny_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.TYRANNY, 1)


@register_effect(CardId.VOID_FORM)
def void_form(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("void_form", 2)
    combat.apply_power_to(_owner(card, combat), PowerId.VOID_FORM, amount)
    combat.request_end_turn_after_current_play()


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
    combat.apply_power_to(_owner(card, combat), PowerId.THE_SEALED_THRONE, 1)


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


def make_beat_into_shape(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BEAT_INTO_SHAPE, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.RARE,
        upgraded=upgraded, base_damage=7 if upgraded else 5,
        effect_vars={"forge_base": 7 if upgraded else 5, "forge_per_hit": 7 if upgraded else 5},
        instance_id=_get_next_id(),
    )


def make_begone(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BEGONE, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=5 if upgraded else 4, upgraded=upgraded,
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


def make_photon_cut(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.PHOTON_CUT, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=13 if upgraded else 10, upgraded=upgraded,
        effect_vars={"cards": 2 if upgraded else 1},
        instance_id=_get_next_id(),
    )


def make_refine_blade(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.REFINE_BLADE, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        upgraded=upgraded,
        effect_vars={"forge": 10 if upgraded else 6, "energy": 1},
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


def make_spoils_of_battle(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SPOILS_OF_BATTLE, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        upgraded=upgraded,
        effect_vars={"forge": 15 if upgraded else 10},
        instance_id=_get_next_id(),
    )


def make_wrought_in_war(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.WROUGHT_IN_WAR, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=9 if upgraded else 7, upgraded=upgraded,
        effect_vars={"forge": 7 if upgraded else 5},
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


def make_bundle_of_joy(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BUNDLE_OF_JOY, cost=2, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        upgraded=upgraded, keywords=frozenset({"exhaust"}),
        effect_vars={"cards": 4 if upgraded else 3},
        instance_id=_get_next_id(),
    )


def make_crash_landing(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.CRASH_LANDING, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ALL_ENEMIES, rarity=CardRarity.RARE,
        base_damage=26 if upgraded else 21, upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_bulwark(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BULWARK, cost=2, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        base_block=16 if upgraded else 13, upgraded=upgraded,
        effect_vars={"forge": 13 if upgraded else 10},
        instance_id=_get_next_id(),
    )


def make_charge(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.CHARGE, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        upgraded=upgraded, effect_vars={"cards": 2},
        instance_id=_get_next_id(),
    )


def make_conqueror(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.CONQUEROR, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        upgraded=upgraded,
        effect_vars={"forge": 5 if upgraded else 3},
        instance_id=_get_next_id(),
    )


def make_manifest_authority(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.MANIFEST_AUTHORITY, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        base_block=8 if upgraded else 7, upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_guards(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.GUARDS, cost=2, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        upgraded=upgraded, keywords=frozenset({"exhaust"}),
        instance_id=_get_next_id(),
    )


def make_seeking_edge(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SEEKING_EDGE, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        upgraded=upgraded,
        effect_vars={"forge": 11 if upgraded else 7},
        instance_id=_get_next_id(),
    )


def make_the_smith(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.THE_SMITH, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        upgraded=upgraded, star_cost=4,
        effect_vars={"forge": 40 if upgraded else 30},
        instance_id=_get_next_id(),
    )


def make_convergence(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.CONVERGENCE, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        upgraded=upgraded,
        effect_vars={"energy": 1, "stars": 2 if upgraded else 1},
        instance_id=_get_next_id(),
    )


def make_hidden_cache(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.HIDDEN_CACHE, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        upgraded=upgraded,
        effect_vars={"stars": 1, "star_next": 4 if upgraded else 3},
        instance_id=_get_next_id(),
    )


def make_resonance(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.RESONANCE, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.ALL_ENEMIES, rarity=CardRarity.UNCOMMON,
        upgraded=upgraded, star_cost=3,
        effect_vars={"strength": 2 if upgraded else 1},
        instance_id=_get_next_id(),
    )


def make_summon_forth(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SUMMON_FORTH, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        upgraded=upgraded, keywords=frozenset({"retain"}),
        effect_vars={"forge": 11 if upgraded else 8},
        instance_id=_get_next_id(),
    )


def make_decisions_decisions(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.DECISIONS_DECISIONS, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        upgraded=upgraded, keywords=frozenset({"exhaust"}), star_cost=6,
        effect_vars={"cards": 5 if upgraded else 3, "repeat": 3},
        instance_id=_get_next_id(),
    )


def make_quasar(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.QUASAR, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        upgraded=upgraded, star_cost=2,
        instance_id=_get_next_id(),
    )


def make_make_it_so(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.MAKE_IT_SO, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.RARE,
        upgraded=upgraded, base_damage=9 if upgraded else 6,
        effect_vars={"cards": 3},
        instance_id=_get_next_id(),
    )


def make_heirloom_hammer(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.HEIRLOOM_HAMMER, cost=2, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.RARE,
        upgraded=upgraded, base_damage=22 if upgraded else 17,
        effect_vars={"repeat": 1},
        instance_id=_get_next_id(),
    )


def make_void_form(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.VOID_FORM, cost=3, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        upgraded=upgraded,
        effect_vars={"void_form": 3 if upgraded else 2},
        instance_id=_get_next_id(),
    )


def make_crescent_spear(upgraded: bool = False) -> CardInstance:
    from sts2_env.cards.factory import create_reference_card

    card = create_reference_card(CardId.CRESCENT_SPEAR, upgraded=upgraded, allow_generation=True)
    card.base_damage = 7 if upgraded else 6
    card.effect_vars["calc_base"] = 6
    card.effect_vars["extra_damage"] = 3 if upgraded else 2
    return card


def make_largesse(upgraded: bool = False) -> CardInstance:
    from sts2_env.cards.factory import create_reference_card

    return create_reference_card(CardId.LARGESSE, upgraded=upgraded, allow_generation=True)


def make_lunar_blast(upgraded: bool = False) -> CardInstance:
    from sts2_env.cards.factory import create_reference_card

    card = create_reference_card(CardId.LUNAR_BLAST, upgraded=upgraded, allow_generation=True)
    card.base_damage = 5 if upgraded else 4
    return card


def make_pale_blue_dot(upgraded: bool = False) -> CardInstance:
    from sts2_env.cards.factory import create_reference_card

    return create_reference_card(CardId.PALE_BLUE_DOT, upgraded=upgraded, allow_generation=True)


def make_radiate(upgraded: bool = False) -> CardInstance:
    from sts2_env.cards.factory import create_reference_card

    card = create_reference_card(CardId.RADIATE, upgraded=upgraded, allow_generation=True)
    card.base_damage = 4 if upgraded else 3
    card.effect_vars["stars"] = 1
    return card


def make_stardust(upgraded: bool = False) -> CardInstance:
    from sts2_env.cards.factory import create_reference_card

    card = create_reference_card(CardId.STARDUST, upgraded=upgraded, allow_generation=True)
    card.base_damage = 7 if upgraded else 5
    return card


def make_supermassive(upgraded: bool = False) -> CardInstance:
    from sts2_env.cards.factory import create_reference_card

    card = create_reference_card(CardId.SUPERMASSIVE, upgraded=upgraded, allow_generation=True)
    card.base_damage = 6 if upgraded else 5
    card.effect_vars["calc_base"] = 5
    card.effect_vars["extra_damage"] = 4 if upgraded else 3
    return card


def make_dying_star(upgraded: bool = False) -> CardInstance:
    from sts2_env.cards.factory import create_reference_card

    card = create_reference_card(CardId.DYING_STAR, upgraded=upgraded, allow_generation=True)
    card.base_damage = 11 if upgraded else 9
    card.effect_vars["strength_loss"] = 11 if upgraded else 9
    return card


def make_heavenly_drill(upgraded: bool = False) -> CardInstance:
    from sts2_env.cards.factory import create_reference_card

    card = create_reference_card(CardId.HEAVENLY_DRILL, upgraded=upgraded, allow_generation=True)
    card.base_damage = 10 if upgraded else 8
    return card


def make_i_am_invincible(upgraded: bool = False) -> CardInstance:
    from sts2_env.cards.factory import create_reference_card

    return create_reference_card(CardId.I_AM_INVINCIBLE, upgraded=upgraded, allow_generation=False)


def _make_reference_factory_card(card_id: CardId, upgraded: bool = False) -> CardInstance:
    from sts2_env.cards.factory import create_reference_card

    return create_reference_card(card_id, upgraded=upgraded, allow_generation=True)


_REFERENCE_FACTORY_CARD_IDS = (
    CardId.CRUSH_UNDER,
    CardId.GLITTERSTREAM,
    CardId.ALIGNMENT,
    CardId.BLACK_HOLE,
    CardId.CHILD_OF_THE_STARS,
    CardId.DEVASTATE,
    CardId.FURNACE,
    CardId.GAMMA_BLAST,
    CardId.GLIMMER,
    CardId.HEGEMONY,
    CardId.KINGLY_KICK,
    CardId.KINGLY_PUNCH,
    CardId.KNOCKOUT_BLOW,
    CardId.MONOLOGUE_CARD,
    CardId.ORBIT,
    CardId.PARRY_CARD,
    CardId.PARTICLE_WALL,
    CardId.PILLAR_OF_CREATION,
    CardId.PROPHESIZE,
    CardId.REFLECT_CARD,
    CardId.ROYAL_GAMBLE,
    CardId.SHINING_STRIKE,
    CardId.SPECTRUM_SHIFT,
    CardId.TERRAFORMING,
    CardId.ARSENAL,
    CardId.BOMBARDMENT,
    CardId.COMET,
    CardId.FOREGONE_CONCLUSION,
    CardId.GENESIS,
    CardId.HAMMER_TIME,
    CardId.MONARCHS_GAZE_CARD,
    CardId.NEUTRON_AEGIS,
    CardId.ROYALTIES_CARD,
    CardId.SEVEN_STARS,
    CardId.SWORD_SAGE,
    CardId.TYRANNY_CARD,
    CardId.METEOR_SHOWER,
    CardId.THE_SEALED_THRONE,
)

for _card_id in _REFERENCE_FACTORY_CARD_IDS:
    _factory_name = f"make_{_card_id.name.lower()}"
    if _factory_name in globals():
        continue

    def _factory(upgraded: bool = False, *, _cid: CardId = _card_id) -> CardInstance:
        return _make_reference_factory_card(_cid, upgraded=upgraded)

    _factory.__name__ = _factory_name
    globals()[_factory_name] = _factory
