"""Defect card effects and factories — all 88 cards."""

from __future__ import annotations

from sts2_env.cards.base import CardInstance, _get_next_id
from sts2_env.cards.factory import create_character_cards
from sts2_env.cards.registry import register_effect
from sts2_env.core.enums import (
    CardId, CardType, TargetType, CardRarity, ValueProp, PowerId, OrbType,
)
from sts2_env.core.damage import calculate_damage, apply_damage, calculate_block
from sts2_env.core.creature import Creature
from sts2_env.core.combat import CombatState


def _owner(card: CardInstance, combat: CombatState) -> Creature:
    return getattr(card, "owner", None) or combat.player


# ---------------------------------------------------------------------------
#  Orb helpers — delegates to combat orb queue when available
# ---------------------------------------------------------------------------

def _channel_orb(combat: CombatState, orb_type: OrbType) -> None:
    """Channel an orb through the combat orb queue, if present."""
    queue = getattr(combat, 'orb_queue', None)
    if queue is not None:
        queue.channel(orb_type, combat)


def _evoke_front(combat: CombatState) -> None:
    """Evoke the front orb."""
    queue = getattr(combat, 'orb_queue', None)
    if queue is not None and queue.orbs:
        queue.evoke_front(combat)


def _trigger_all_passives(combat: CombatState) -> None:
    """Trigger all orb passives once."""
    queue = getattr(combat, 'orb_queue', None)
    if queue is not None:
        for orb in list(queue.orbs):
            orb.on_passive(combat)


def _get_orb_count(combat: CombatState) -> int:
    queue = getattr(combat, 'orb_queue', None)
    return len(queue.orbs) if queue is not None else 0


def _add_orb_slot(combat: CombatState, count: int = 1) -> None:
    queue = getattr(combat, 'orb_queue', None)
    if queue is not None:
        queue.capacity = min(queue.capacity + count, 10)


def _remove_orb_slot(combat: CombatState, count: int = 1) -> None:
    queue = getattr(combat, 'orb_queue', None)
    if queue is not None:
        queue.capacity = max(0, queue.capacity - count)


def _non_exhausted_status_cards(combat: CombatState, owner: Creature) -> list[CardInstance]:
    return [
        card
        for card in combat._all_cards_for_creature(owner, include_exhausted=False)  # noqa: SLF001
        if card.card_type == CardType.STATUS
    ]


# ---------------------------------------------------------------------------
#  Status card creators used by Defect effects
# ---------------------------------------------------------------------------

def _make_dazed() -> CardInstance:
    from sts2_env.cards.status import make_dazed
    return make_dazed()


def _make_wound() -> CardInstance:
    from sts2_env.cards.status import make_wound
    return make_wound()


def _make_slimed() -> CardInstance:
    from sts2_env.cards.status import make_slimed
    return make_slimed()


def _make_burn() -> CardInstance:
    return CardInstance(
        card_id=CardId.BURN, cost=-1, card_type=CardType.STATUS,
        target_type=TargetType.NONE, rarity=CardRarity.STATUS,
        keywords=frozenset({"unplayable"}), instance_id=_get_next_id(),
    )


def _make_void() -> CardInstance:
    return CardInstance(
        card_id=CardId.VOID, cost=-1, card_type=CardType.STATUS,
        target_type=TargetType.NONE, rarity=CardRarity.STATUS,
        keywords=frozenset({"unplayable", "ethereal"}), instance_id=_get_next_id(),
    )


# ---------------------------------------------------------------------------
#  BASIC (4)
# ---------------------------------------------------------------------------

@register_effect(CardId.STRIKE_DEFECT)
def strike_defect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    dmg = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, owner)


@register_effect(CardId.DEFEND_DEFECT)
def defend_defect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    blk = calculate_block(card.base_block, owner, ValueProp.MOVE, combat, card_source=card)
    owner.gain_block(blk)


@register_effect(CardId.ZAP)
def zap(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _channel_orb(combat, OrbType.LIGHTNING)


@register_effect(CardId.DUALCAST)
def dualcast(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    queue = getattr(combat, 'orb_queue', None)
    if queue is None or not queue.orbs:
        return
    # Dualcast evokes the front orb twice: once without dequeue, then once with dequeue.
    queue.orbs[0].on_evoke(combat)
    _evoke_front(combat)


# ---------------------------------------------------------------------------
#  COMMON (20)
# ---------------------------------------------------------------------------

@register_effect(CardId.BALL_LIGHTNING)
def ball_lightning(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    _channel_orb(combat, OrbType.LIGHTNING)


@register_effect(CardId.BARRAGE)
def barrage(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    hits = _get_orb_count(combat)
    for _ in range(hits):
        dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
        apply_damage(target, dmg, ValueProp.MOVE, combat, _owner(card, combat))


@register_effect(CardId.BEAM_CELL)
def beam_cell(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    combat.apply_power_to(target, PowerId.VULNERABLE, card.effect_vars.get("vulnerable", 1))


@register_effect(CardId.BOOST_AWAY)
def boost_away(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    blk = calculate_block(card.base_block, _owner(card, combat), ValueProp.MOVE, combat, card_source=card)
    _owner(card, combat).gain_block(blk)
    combat.discard_pile.append(_make_dazed())


@register_effect(CardId.CHARGE_BATTERY)
def charge_battery(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    blk = calculate_block(card.base_block, _owner(card, combat), ValueProp.MOVE, combat, card_source=card)
    _owner(card, combat).gain_block(blk)
    combat.apply_power_to(_owner(card, combat), PowerId.ENERGY_NEXT_TURN, card.effect_vars.get("energy", 1))


@register_effect(CardId.CLAW)
def claw(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    # All Claw copies gain +Increase damage permanently
    increase = card.effect_vars.get("increase", 2)
    for pile in [combat.hand, combat.draw_pile, combat.discard_pile, combat.exhaust_pile]:
        for c in pile:
            if c.card_id == CardId.CLAW:
                c.base_damage = (c.base_damage or 0) + increase


@register_effect(CardId.COLD_SNAP)
def cold_snap(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    _channel_orb(combat, OrbType.FROST)


@register_effect(CardId.COMPILE_DRIVER)
def compile_driver(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    draw = _get_orb_count(combat)
    if draw > 0:
        combat._draw_cards(draw)


@register_effect(CardId.COOLHEADED)
def coolheaded(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _channel_orb(combat, OrbType.FROST)
    combat._draw_cards(card.effect_vars.get("cards", 1))


@register_effect(CardId.FOCUSED_STRIKE_CARD)
def focused_strike(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    combat.apply_power_to(_owner(card, combat), PowerId.FOCUSED_STRIKE, card.effect_vars.get("focus", 1))


@register_effect(CardId.GO_FOR_THE_EYES)
def go_for_the_eyes(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    combat.apply_power_to(target, PowerId.WEAK, card.effect_vars.get("weak", 1))


@register_effect(CardId.GUNK_UP)
def gunk_up(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    # Deal damage 2 times; add Slimed to discard
    for _ in range(2):
        dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
        apply_damage(target, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    combat.discard_pile.append(_make_slimed())


@register_effect(CardId.HOLOGRAM)
def hologram(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    blk = calculate_block(card.base_block, _owner(card, combat), ValueProp.MOVE, combat, card_source=card)
    _owner(card, combat).gain_block(blk)
    if not combat.discard_pile:
        return
    combat.request_card_choice(
        prompt="Choose a discard card to return to hand",
        cards=list(combat.discard_pile),
        source_pile="discard",
        resolver=combat.move_card_to_hand,
    )


@register_effect(CardId.HOTFIX)
def hotfix(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Apply temporary Focus (Hotfix power)
    combat.apply_power_to(_owner(card, combat), PowerId.FOCUS, card.effect_vars.get("focus", 2))


@register_effect(CardId.LEAP)
def leap(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    blk = calculate_block(card.base_block, _owner(card, combat), ValueProp.MOVE, combat, card_source=card)
    _owner(card, combat).gain_block(blk)


@register_effect(CardId.LIGHTNING_ROD)
def lightning_rod(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    blk = calculate_block(card.base_block, _owner(card, combat), ValueProp.MOVE, combat, card_source=card)
    _owner(card, combat).gain_block(blk)
    combat.apply_power_to(_owner(card, combat), PowerId.LIGHTNING_ROD, card.effect_vars.get("lightning_rod", 2))


@register_effect(CardId.MOMENTUM_STRIKE)
def momentum_strike(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, _owner(card, combat))


@register_effect(CardId.SWEEPING_BEAM)
def sweeping_beam(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    for enemy in combat.alive_enemies:
        dmg = calculate_damage(card.base_damage, _owner(card, combat), enemy, ValueProp.MOVE, combat)
        apply_damage(enemy, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    combat._draw_cards(card.effect_vars.get("cards", 1))


@register_effect(CardId.TURBO)
def turbo(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.energy += card.effect_vars.get("energy", 2)
    combat.discard_pile.append(_make_void())


@register_effect(CardId.UPROAR)
def uproar(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    for _ in range(2):
        dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
        apply_damage(target, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    candidates = [c for c in combat.draw_pile if c.card_type == CardType.ATTACK and not c.is_unplayable]
    if not candidates:
        candidates = [c for c in combat.draw_pile if c.card_type == CardType.ATTACK]
    if candidates:
        combat.auto_play_card(combat.rng.choice(candidates))


# ---------------------------------------------------------------------------
#  UNCOMMON (32)
# ---------------------------------------------------------------------------

@register_effect(CardId.BOOT_SEQUENCE)
def boot_sequence(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    blk = calculate_block(card.base_block, _owner(card, combat), ValueProp.MOVE, combat, card_source=card)
    _owner(card, combat).gain_block(blk)


@register_effect(CardId.BULK_UP)
def bulk_up(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.STRENGTH, card.effect_vars.get("strength", 2))
    combat.apply_power_to(_owner(card, combat), PowerId.DEXTERITY, card.effect_vars.get("dexterity", 2))
    _remove_orb_slot(combat, card.effect_vars.get("orb_slots", 1))


@register_effect(CardId.CAPACITOR)
def capacitor(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    slots = card.effect_vars.get("slots", 2)
    _add_orb_slot(combat, slots)


@register_effect(CardId.CHAOS)
def chaos(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    import random
    orb_types = [OrbType.LIGHTNING, OrbType.FROST, OrbType.DARK, OrbType.PLASMA, OrbType.GLASS]
    chosen = combat.rng.choice(orb_types)
    _channel_orb(combat, chosen)


@register_effect(CardId.CHILL)
def chill(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _channel_orb(combat, OrbType.FROST)


@register_effect(CardId.COMPACT)
def compact(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    from sts2_env.cards.status import make_fuel

    blk = calculate_block(card.base_block, _owner(card, combat), ValueProp.MOVE, combat, card_source=card)
    _owner(card, combat).gain_block(blk)
    status_cards = [c for c in list(combat.hand) if c.card_type == CardType.STATUS]
    for status_card in status_cards:
        combat.transform_card(status_card, make_fuel(upgraded=card.upgraded))


@register_effect(CardId.DARKNESS_CARD)
def darkness(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _channel_orb(combat, OrbType.DARK)
    _trigger_all_passives(combat)


@register_effect(CardId.DOUBLE_ENERGY)
def double_energy(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.energy *= 2


@register_effect(CardId.ENERGY_SURGE)
def energy_surge(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.energy += card.effect_vars.get("energy", 2)


@register_effect(CardId.FERAL)
def feral(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.FERAL, card.effect_vars.get("feral", 1))


@register_effect(CardId.FIGHT_THROUGH)
def fight_through(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    blk = calculate_block(card.base_block, _owner(card, combat), ValueProp.MOVE, combat, card_source=card)
    _owner(card, combat).gain_block(blk)
    combat.discard_pile.append(_make_wound())


@register_effect(CardId.FTL)
def ftl(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    combat._draw_cards(card.effect_vars.get("cards", 1))


@register_effect(CardId.FUSION)
def fusion(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _channel_orb(combat, OrbType.PLASMA)


@register_effect(CardId.GLACIER)
def glacier(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    blk = calculate_block(card.base_block, _owner(card, combat), ValueProp.MOVE, combat, card_source=card)
    _owner(card, combat).gain_block(blk)
    _channel_orb(combat, OrbType.FROST)
    _channel_orb(combat, OrbType.FROST)


@register_effect(CardId.GLASSWORK)
def glasswork(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    blk = calculate_block(card.base_block, _owner(card, combat), ValueProp.MOVE, combat, card_source=card)
    _owner(card, combat).gain_block(blk)
    _channel_orb(combat, OrbType.GLASS)


@register_effect(CardId.HAILSTORM)
def hailstorm(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.HAILSTORM, card.effect_vars.get("hailstorm", 6))


@register_effect(CardId.ITERATION_CARD)
def iteration(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.ITERATION, card.effect_vars.get("iteration", 2))


@register_effect(CardId.LOOP_CARD)
def loop(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.LOOP, card.effect_vars.get("loop", 1))


@register_effect(CardId.NULL_CARD)
def null_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    combat.apply_power_to(target, PowerId.WEAK, card.effect_vars.get("weak", 2))
    _channel_orb(combat, OrbType.DARK)


@register_effect(CardId.OVERCLOCK)
def overclock(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat._draw_cards(card.effect_vars.get("cards", 2))
    combat.discard_pile.append(_make_burn())


@register_effect(CardId.REFRACT)
def refract(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    for _ in range(2):
        dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
        apply_damage(target, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    _channel_orb(combat, OrbType.GLASS)


@register_effect(CardId.ROCKET_PUNCH)
def rocket_punch(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    combat._draw_cards(card.effect_vars.get("cards", 1))


@register_effect(CardId.SCAVENGE)
def scavenge(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    energy = card.effect_vars.get("energy", 2)
    candidates = list(combat.hand)
    if not candidates:
        combat.apply_power_to(owner, PowerId.ENERGY_NEXT_TURN, energy)
        return

    def _resolver(selected: CardInstance | None) -> None:
        if selected is not None:
            combat.exhaust_card(selected)
        combat.apply_power_to(owner, PowerId.ENERGY_NEXT_TURN, energy)

    combat.request_card_choice(
        prompt="Choose a hand card to exhaust",
        cards=candidates,
        source_pile="hand",
        resolver=_resolver,
    )


@register_effect(CardId.SCRAPE)
def scrape(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    draw = card.effect_vars.get("cards", 4)
    combat._draw_cards(draw)
    # Discard non-0-cost cards drawn
    to_discard = [c for c in combat.hand[-draw:] if c.cost != 0]
    for c in to_discard:
        combat.hand.remove(c)
        combat.discard_pile.append(c)


@register_effect(CardId.SHADOW_SHIELD)
def shadow_shield(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    blk = calculate_block(card.base_block, _owner(card, combat), ValueProp.MOVE, combat, card_source=card)
    _owner(card, combat).gain_block(blk)
    _channel_orb(combat, OrbType.DARK)


@register_effect(CardId.SKIM)
def skim(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat._draw_cards(card.effect_vars.get("cards", 3))


@register_effect(CardId.SMOKESTACK)
def smokestack(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.SMOKESTACK, card.effect_vars.get("smokestack", 5))


@register_effect(CardId.STORM_CARD)
def storm(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.STORM, card.effect_vars.get("storm", 1))


@register_effect(CardId.SUBROUTINE)
def subroutine(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.SUBROUTINE, 1)


@register_effect(CardId.SUNDER)
def sunder(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
    result = apply_damage(target, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    # Gain energy if enemy was killed
    if result.was_killed:
        combat.energy += card.effect_vars.get("energy", 3)


@register_effect(CardId.SYNCHRONIZE)
def synchronize(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Gain Focus based on orb count
    orb_count = _get_orb_count(combat)
    extra = card.effect_vars.get("calc_extra", 2)
    focus = orb_count * extra
    if focus > 0:
        combat.apply_power_to(_owner(card, combat), PowerId.FOCUS, focus)


@register_effect(CardId.SYNTHESIS)
def synthesis(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    combat.apply_power_to(_owner(card, combat), PowerId.FREE_POWER, 1)


@register_effect(CardId.TEMPEST)
def tempest(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # X-cost: channel X Lightning orbs
    x = combat.energy + card.cost
    for _ in range(max(0, x)):
        _channel_orb(combat, OrbType.LIGHTNING)
    combat.energy = 0


@register_effect(CardId.TESLA_COIL)
def tesla_coil(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    _trigger_all_passives(combat)


@register_effect(CardId.THUNDER_CARD)
def thunder_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.THUNDER, card.effect_vars.get("thunder", 6))


@register_effect(CardId.WHITE_NOISE)
def white_noise(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    generated = create_character_cards(
        combat.character_id,
        combat.rng,
        1,
        card_type=CardType.POWER,
    )
    if not generated:
        return
    generated[0].set_temporary_cost_for_turn(0)
    combat.move_card_to_hand(generated[0])


# ---------------------------------------------------------------------------
#  RARE (26)
# ---------------------------------------------------------------------------

@register_effect(CardId.ADAPTIVE_STRIKE)
def adaptive_strike(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, _owner(card, combat))


@register_effect(CardId.ALL_FOR_ONE)
def all_for_one(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    # Return all 0-cost cards from discard to hand
    zero_cost = [c for c in combat.discard_pile if c.cost == 0]
    for c in zero_cost:
        combat.discard_pile.remove(c)
        combat.hand.append(c)


@register_effect(CardId.BUFFER_CARD)
def buffer(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.BUFFER, card.effect_vars.get("buffer", 1))


@register_effect(CardId.CONSUMING_SHADOW)
def consuming_shadow(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.CONSUMING_SHADOW, card.effect_vars.get("consuming_shadow", 1))
    _channel_orb(combat, OrbType.DARK)


@register_effect(CardId.COOLANT)
def coolant(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.COOLANT, card.effect_vars.get("coolant", 2))


@register_effect(CardId.CREATIVE_AI_CARD)
def creative_ai(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.CREATIVE_AI, card.effect_vars.get("creative_ai", 1))


@register_effect(CardId.DEFRAGMENT)
def defragment(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.FOCUS, card.effect_vars.get("focus", 1))


@register_effect(CardId.ECHO_FORM_CARD)
def echo_form(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.ECHO_FORM, card.effect_vars.get("echo_form", 1))


@register_effect(CardId.FLAK_CANNON)
def flak_cannon(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    statuses = _non_exhausted_status_cards(combat, owner)
    hits = len(statuses)
    for status in statuses:
        combat.exhaust_card(status)
    for _ in range(hits):
        alive = combat.alive_enemies
        if not alive:
            break
        t = combat.rng.choice(alive)
        dmg = calculate_damage(card.base_damage, owner, t, ValueProp.MOVE, combat)
        apply_damage(t, dmg, ValueProp.MOVE, combat, owner)


@register_effect(CardId.GENETIC_ALGORITHM)
def genetic_algorithm(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Gain Block (self-mutating: gains +3 block permanently each play)
    block_amt = card.effect_vars.get("block", 0)
    owner = _owner(card, combat)
    blk = calculate_block(block_amt, owner, ValueProp.MOVE, combat, card_source=card)
    owner.gain_block(blk)
    increase = card.effect_vars.get("increase", 3)
    card.effect_vars["block"] = block_amt + increase
    if card.base_block is not None:
        card.base_block += increase
    else:
        card.base_block = increase


@register_effect(CardId.HELIX_DRILL)
def helix_drill(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    # Hits scale with orb count
    hits = _get_orb_count(combat)
    for _ in range(max(1, hits)):
        dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
        apply_damage(target, dmg, ValueProp.MOVE, combat, _owner(card, combat))


@register_effect(CardId.HYPERBEAM)
def hyperbeam(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    for enemy in combat.alive_enemies:
        dmg = calculate_damage(card.base_damage, _owner(card, combat), enemy, ValueProp.MOVE, combat)
        apply_damage(enemy, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    # Lose Focus
    combat.apply_power_to(_owner(card, combat), PowerId.FOCUS, -card.effect_vars.get("focus_loss", 3))


@register_effect(CardId.ICE_LANCE)
def ice_lance(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    _channel_orb(combat, OrbType.FROST)
    _channel_orb(combat, OrbType.FROST)


@register_effect(CardId.IGNITION)
def ignition(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _channel_orb(combat, OrbType.PLASMA)


@register_effect(CardId.MACHINE_LEARNING_CARD)
def machine_learning(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.MACHINE_LEARNING, card.effect_vars.get("cards", 1))


@register_effect(CardId.METEOR_STRIKE)
def meteor_strike(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    _channel_orb(combat, OrbType.PLASMA)
    _channel_orb(combat, OrbType.PLASMA)
    _channel_orb(combat, OrbType.PLASMA)


@register_effect(CardId.MODDED)
def modded(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat._draw_cards(card.effect_vars.get("cards", 1))
    _add_orb_slot(combat, 1)
    # Increase cost by 1 this combat
    card.cost += 1


@register_effect(CardId.MULTI_CAST)
def multi_cast(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # X-cost: evoke front orb X times
    x = combat.energy + card.cost
    for _ in range(max(0, x)):
        _evoke_front(combat)
    combat.energy = 0


@register_effect(CardId.RAINBOW)
def rainbow(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _channel_orb(combat, OrbType.LIGHTNING)
    _channel_orb(combat, OrbType.FROST)
    _channel_orb(combat, OrbType.DARK)


@register_effect(CardId.REBOOT)
def reboot(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Shuffle hand into draw pile, then draw
    combat.draw_pile.extend(combat.hand)
    combat.hand.clear()
    combat.rng.shuffle(combat.draw_pile)
    combat._draw_cards(card.effect_vars.get("cards", 4))


@register_effect(CardId.SHATTER)
def shatter(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    for enemy in combat.alive_enemies:
        dmg = calculate_damage(card.base_damage, _owner(card, combat), enemy, ValueProp.MOVE, combat)
        apply_damage(enemy, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    _evoke_front(combat)


@register_effect(CardId.SIGNAL_BOOST)
def signal_boost(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.SIGNAL_BOOST, card.effect_vars.get("signal_boost", 1))


@register_effect(CardId.SPINNER_CARD)
def spinner_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.SPINNER, card.effect_vars.get("spinner", 1))
    _channel_orb(combat, OrbType.GLASS)


@register_effect(CardId.SUPERCRITICAL)
def supercritical(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.energy += card.effect_vars.get("energy", 4)


@register_effect(CardId.TRASH_TO_TREASURE)
def trash_to_treasure(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.TRASH_TO_TREASURE, 1)


@register_effect(CardId.VOLTAIC)
def voltaic(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Channel Lightning orbs based on orb count
    orb_count = _get_orb_count(combat)
    for _ in range(max(1, orb_count)):
        _channel_orb(combat, OrbType.LIGHTNING)


# ---------------------------------------------------------------------------
#  ANCIENT (2)
# ---------------------------------------------------------------------------

@register_effect(CardId.BIASED_COGNITION_CARD)
def biased_cognition(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.FOCUS, card.effect_vars.get("focus", 4))
    combat.apply_power_to(_owner(card, combat), PowerId.BIASED_COGNITION, card.effect_vars.get("biased_cognition", 1))


@register_effect(CardId.QUADCAST)
def quadcast(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    for _ in range(4):
        _evoke_front(combat)


# ---------------------------------------------------------------------------
#  Card factories
# ---------------------------------------------------------------------------

def make_strike_defect() -> CardInstance:
    return CardInstance(
        card_id=CardId.STRIKE_DEFECT, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.BASIC,
        base_damage=6, instance_id=_get_next_id(),
    )


def make_defend_defect() -> CardInstance:
    return CardInstance(
        card_id=CardId.DEFEND_DEFECT, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.BASIC,
        base_block=5, instance_id=_get_next_id(),
    )


def make_zap() -> CardInstance:
    return CardInstance(
        card_id=CardId.ZAP, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.BASIC,
        instance_id=_get_next_id(),
    )


def make_dualcast() -> CardInstance:
    return CardInstance(
        card_id=CardId.DUALCAST, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.BASIC,
        instance_id=_get_next_id(),
    )


def make_ball_lightning() -> CardInstance:
    return CardInstance(
        card_id=CardId.BALL_LIGHTNING, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=7, instance_id=_get_next_id(),
    )


def make_barrage() -> CardInstance:
    return CardInstance(
        card_id=CardId.BARRAGE, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=5, effect_vars={"calc_base": 0, "calc_extra": 1},
        instance_id=_get_next_id(),
    )


def make_beam_cell() -> CardInstance:
    return CardInstance(
        card_id=CardId.BEAM_CELL, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=3, effect_vars={"vulnerable": 1}, instance_id=_get_next_id(),
    )


def make_boost_away() -> CardInstance:
    return CardInstance(
        card_id=CardId.BOOST_AWAY, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        base_block=6, instance_id=_get_next_id(),
    )


def make_charge_battery() -> CardInstance:
    return CardInstance(
        card_id=CardId.CHARGE_BATTERY, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        base_block=7, effect_vars={"energy": 1}, instance_id=_get_next_id(),
    )


def make_claw() -> CardInstance:
    return CardInstance(
        card_id=CardId.CLAW, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=3, effect_vars={"increase": 2}, instance_id=_get_next_id(),
    )


def make_cold_snap() -> CardInstance:
    return CardInstance(
        card_id=CardId.COLD_SNAP, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=6, instance_id=_get_next_id(),
    )


def make_compile_driver() -> CardInstance:
    return CardInstance(
        card_id=CardId.COMPILE_DRIVER, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=7, effect_vars={"calc_base": 0, "calc_extra": 1},
        instance_id=_get_next_id(),
    )


def make_coolheaded() -> CardInstance:
    return CardInstance(
        card_id=CardId.COOLHEADED, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        effect_vars={"cards": 1}, instance_id=_get_next_id(),
    )


def make_focused_strike() -> CardInstance:
    return CardInstance(
        card_id=CardId.FOCUSED_STRIKE_CARD, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=9, effect_vars={"focus": 1}, instance_id=_get_next_id(),
    )


def make_go_for_the_eyes() -> CardInstance:
    return CardInstance(
        card_id=CardId.GO_FOR_THE_EYES, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=3, effect_vars={"weak": 1}, instance_id=_get_next_id(),
    )


def make_gunk_up() -> CardInstance:
    return CardInstance(
        card_id=CardId.GUNK_UP, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=4, instance_id=_get_next_id(),
    )


def make_hologram() -> CardInstance:
    return CardInstance(
        card_id=CardId.HOLOGRAM, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        base_block=3, keywords=frozenset({"exhaust"}), instance_id=_get_next_id(),
    )


def make_hotfix() -> CardInstance:
    return CardInstance(
        card_id=CardId.HOTFIX, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        effect_vars={"focus": 2}, instance_id=_get_next_id(),
    )


def make_leap() -> CardInstance:
    return CardInstance(
        card_id=CardId.LEAP, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        base_block=9, instance_id=_get_next_id(),
    )


def make_lightning_rod() -> CardInstance:
    return CardInstance(
        card_id=CardId.LIGHTNING_ROD, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        base_block=4, effect_vars={"lightning_rod": 2}, instance_id=_get_next_id(),
    )


def make_momentum_strike() -> CardInstance:
    return CardInstance(
        card_id=CardId.MOMENTUM_STRIKE, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=10, instance_id=_get_next_id(),
    )


def make_sweeping_beam() -> CardInstance:
    return CardInstance(
        card_id=CardId.SWEEPING_BEAM, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ALL_ENEMIES, rarity=CardRarity.COMMON,
        base_damage=6, effect_vars={"cards": 1}, instance_id=_get_next_id(),
    )


def make_turbo() -> CardInstance:
    return CardInstance(
        card_id=CardId.TURBO, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        effect_vars={"energy": 2}, instance_id=_get_next_id(),
    )


def make_uproar() -> CardInstance:
    return CardInstance(
        card_id=CardId.UPROAR, cost=2, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=5, instance_id=_get_next_id(),
    )


def make_boot_sequence() -> CardInstance:
    return CardInstance(
        card_id=CardId.BOOT_SEQUENCE, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        base_block=10, keywords=frozenset({"innate", "exhaust"}),
        instance_id=_get_next_id(),
    )


def make_bulk_up() -> CardInstance:
    return CardInstance(
        card_id=CardId.BULK_UP, cost=2, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={"strength": 2, "dexterity": 2, "orb_slots": 1},
        instance_id=_get_next_id(),
    )


def make_capacitor() -> CardInstance:
    return CardInstance(
        card_id=CardId.CAPACITOR, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={"slots": 2}, instance_id=_get_next_id(),
    )


def make_chaos() -> CardInstance:
    return CardInstance(
        card_id=CardId.CHAOS, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        instance_id=_get_next_id(),
    )


def make_chill() -> CardInstance:
    return CardInstance(
        card_id=CardId.CHILL, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        keywords=frozenset({"exhaust"}), instance_id=_get_next_id(),
    )


def make_compact() -> CardInstance:
    return CardInstance(
        card_id=CardId.COMPACT, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        base_block=6, instance_id=_get_next_id(),
    )


def make_darkness() -> CardInstance:
    return CardInstance(
        card_id=CardId.DARKNESS_CARD, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        instance_id=_get_next_id(),
    )


def make_double_energy() -> CardInstance:
    return CardInstance(
        card_id=CardId.DOUBLE_ENERGY, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        keywords=frozenset({"exhaust"}), instance_id=_get_next_id(),
    )


def make_energy_surge() -> CardInstance:
    return CardInstance(
        card_id=CardId.ENERGY_SURGE, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        keywords=frozenset({"exhaust"}),
        effect_vars={"energy": 2}, instance_id=_get_next_id(),
    )


def make_feral() -> CardInstance:
    return CardInstance(
        card_id=CardId.FERAL, cost=2, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={"feral": 1}, instance_id=_get_next_id(),
    )


def make_fight_through() -> CardInstance:
    return CardInstance(
        card_id=CardId.FIGHT_THROUGH, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        base_block=13, instance_id=_get_next_id(),
    )


def make_ftl() -> CardInstance:
    return CardInstance(
        card_id=CardId.FTL, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=5, effect_vars={"cards": 1, "play_max": 3},
        instance_id=_get_next_id(),
    )


def make_fusion() -> CardInstance:
    return CardInstance(
        card_id=CardId.FUSION, cost=2, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        instance_id=_get_next_id(),
    )


def make_glacier() -> CardInstance:
    return CardInstance(
        card_id=CardId.GLACIER, cost=2, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        base_block=6, instance_id=_get_next_id(),
    )


def make_glasswork() -> CardInstance:
    return CardInstance(
        card_id=CardId.GLASSWORK, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        base_block=5, instance_id=_get_next_id(),
    )


def make_hailstorm() -> CardInstance:
    return CardInstance(
        card_id=CardId.HAILSTORM, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={"hailstorm": 6}, instance_id=_get_next_id(),
    )


def make_iteration() -> CardInstance:
    return CardInstance(
        card_id=CardId.ITERATION_CARD, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={"iteration": 2}, instance_id=_get_next_id(),
    )


def make_loop() -> CardInstance:
    return CardInstance(
        card_id=CardId.LOOP_CARD, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={"loop": 1}, instance_id=_get_next_id(),
    )


def make_null() -> CardInstance:
    return CardInstance(
        card_id=CardId.NULL_CARD, cost=2, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=10, effect_vars={"weak": 2}, instance_id=_get_next_id(),
    )


def make_overclock() -> CardInstance:
    return CardInstance(
        card_id=CardId.OVERCLOCK, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={"cards": 2}, instance_id=_get_next_id(),
    )


def make_refract() -> CardInstance:
    return CardInstance(
        card_id=CardId.REFRACT, cost=3, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=9, instance_id=_get_next_id(),
    )


def make_rocket_punch() -> CardInstance:
    return CardInstance(
        card_id=CardId.ROCKET_PUNCH, cost=2, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=13, effect_vars={"cards": 1}, instance_id=_get_next_id(),
    )


def make_scavenge() -> CardInstance:
    return CardInstance(
        card_id=CardId.SCAVENGE, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={"energy": 2}, instance_id=_get_next_id(),
    )


def make_scrape() -> CardInstance:
    return CardInstance(
        card_id=CardId.SCRAPE, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=7, effect_vars={"cards": 4}, instance_id=_get_next_id(),
    )


def make_shadow_shield() -> CardInstance:
    return CardInstance(
        card_id=CardId.SHADOW_SHIELD, cost=2, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        base_block=11, instance_id=_get_next_id(),
    )


def make_skim() -> CardInstance:
    return CardInstance(
        card_id=CardId.SKIM, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={"cards": 3}, instance_id=_get_next_id(),
    )


def make_smokestack() -> CardInstance:
    return CardInstance(
        card_id=CardId.SMOKESTACK, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={"smokestack": 5}, instance_id=_get_next_id(),
    )


def make_storm() -> CardInstance:
    return CardInstance(
        card_id=CardId.STORM_CARD, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={"storm": 1}, instance_id=_get_next_id(),
    )


def make_subroutine() -> CardInstance:
    return CardInstance(
        card_id=CardId.SUBROUTINE, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        instance_id=_get_next_id(),
    )


def make_sunder() -> CardInstance:
    return CardInstance(
        card_id=CardId.SUNDER, cost=3, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=24, effect_vars={"energy": 3}, instance_id=_get_next_id(),
    )


def make_synchronize() -> CardInstance:
    return CardInstance(
        card_id=CardId.SYNCHRONIZE, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        keywords=frozenset({"exhaust"}),
        effect_vars={"calc_base": 0, "calc_extra": 2}, instance_id=_get_next_id(),
    )


def make_synthesis() -> CardInstance:
    return CardInstance(
        card_id=CardId.SYNTHESIS, cost=2, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=12, instance_id=_get_next_id(),
    )


def make_tempest() -> CardInstance:
    return CardInstance(
        card_id=CardId.TEMPEST, cost=-1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        instance_id=_get_next_id(),
    )


def make_tesla_coil() -> CardInstance:
    return CardInstance(
        card_id=CardId.TESLA_COIL, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=3, instance_id=_get_next_id(),
    )


def make_thunder() -> CardInstance:
    return CardInstance(
        card_id=CardId.THUNDER_CARD, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={"thunder": 6}, instance_id=_get_next_id(),
    )


def make_white_noise() -> CardInstance:
    return CardInstance(
        card_id=CardId.WHITE_NOISE, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        keywords=frozenset({"exhaust"}), instance_id=_get_next_id(),
    )


def make_adaptive_strike() -> CardInstance:
    return CardInstance(
        card_id=CardId.ADAPTIVE_STRIKE, cost=2, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.RARE,
        base_damage=18, instance_id=_get_next_id(),
    )


def make_all_for_one() -> CardInstance:
    return CardInstance(
        card_id=CardId.ALL_FOR_ONE, cost=2, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.RARE,
        base_damage=10, instance_id=_get_next_id(),
    )


def make_buffer() -> CardInstance:
    return CardInstance(
        card_id=CardId.BUFFER_CARD, cost=2, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        effect_vars={"buffer": 1}, instance_id=_get_next_id(),
    )


def make_consuming_shadow() -> CardInstance:
    return CardInstance(
        card_id=CardId.CONSUMING_SHADOW, cost=2, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        effect_vars={"consuming_shadow": 1}, instance_id=_get_next_id(),
    )


def make_coolant() -> CardInstance:
    return CardInstance(
        card_id=CardId.COOLANT, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        effect_vars={"coolant": 2}, instance_id=_get_next_id(),
    )


def make_creative_ai() -> CardInstance:
    return CardInstance(
        card_id=CardId.CREATIVE_AI_CARD, cost=3, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        effect_vars={"creative_ai": 1}, instance_id=_get_next_id(),
    )


def make_defragment() -> CardInstance:
    return CardInstance(
        card_id=CardId.DEFRAGMENT, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        effect_vars={"focus": 1}, instance_id=_get_next_id(),
    )


def make_echo_form() -> CardInstance:
    return CardInstance(
        card_id=CardId.ECHO_FORM_CARD, cost=3, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        keywords=frozenset({"ethereal"}),
        effect_vars={"echo_form": 1}, instance_id=_get_next_id(),
    )


def make_flak_cannon() -> CardInstance:
    return CardInstance(
        card_id=CardId.FLAK_CANNON, cost=2, card_type=CardType.ATTACK,
        target_type=TargetType.RANDOM_ENEMY, rarity=CardRarity.RARE,
        base_damage=8, effect_vars={"calc_base": 0, "calc_extra": 1},
        instance_id=_get_next_id(),
    )


def make_genetic_algorithm() -> CardInstance:
    return CardInstance(
        card_id=CardId.GENETIC_ALGORITHM, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        base_block=0, keywords=frozenset({"exhaust"}),
        effect_vars={"block": 0, "increase": 3}, instance_id=_get_next_id(),
    )


def make_helix_drill() -> CardInstance:
    return CardInstance(
        card_id=CardId.HELIX_DRILL, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.RARE,
        base_damage=3, effect_vars={"calc_base": 0, "calc_extra": 1},
        instance_id=_get_next_id(),
    )


def make_hyperbeam() -> CardInstance:
    return CardInstance(
        card_id=CardId.HYPERBEAM, cost=2, card_type=CardType.ATTACK,
        target_type=TargetType.ALL_ENEMIES, rarity=CardRarity.RARE,
        base_damage=26, effect_vars={"focus_loss": 3}, instance_id=_get_next_id(),
    )


def make_ice_lance() -> CardInstance:
    return CardInstance(
        card_id=CardId.ICE_LANCE, cost=3, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.RARE,
        base_damage=19, instance_id=_get_next_id(),
    )


def make_ignition() -> CardInstance:
    return CardInstance(
        card_id=CardId.IGNITION, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        keywords=frozenset({"exhaust"}), instance_id=_get_next_id(),
    )


def make_machine_learning() -> CardInstance:
    return CardInstance(
        card_id=CardId.MACHINE_LEARNING_CARD, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        effect_vars={"cards": 1}, instance_id=_get_next_id(),
    )


def make_meteor_strike() -> CardInstance:
    return CardInstance(
        card_id=CardId.METEOR_STRIKE, cost=5, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.RARE,
        base_damage=24, instance_id=_get_next_id(),
    )


def make_modded() -> CardInstance:
    return CardInstance(
        card_id=CardId.MODDED, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        effect_vars={"cards": 1}, instance_id=_get_next_id(),
    )


def make_multi_cast() -> CardInstance:
    return CardInstance(
        card_id=CardId.MULTI_CAST, cost=-1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        instance_id=_get_next_id(),
    )


def make_rainbow() -> CardInstance:
    return CardInstance(
        card_id=CardId.RAINBOW, cost=2, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        keywords=frozenset({"exhaust"}), instance_id=_get_next_id(),
    )


def make_reboot() -> CardInstance:
    return CardInstance(
        card_id=CardId.REBOOT, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        keywords=frozenset({"exhaust"}),
        effect_vars={"cards": 4}, instance_id=_get_next_id(),
    )


def make_shatter() -> CardInstance:
    return CardInstance(
        card_id=CardId.SHATTER, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ALL_ENEMIES, rarity=CardRarity.RARE,
        base_damage=11, instance_id=_get_next_id(),
    )


def make_signal_boost() -> CardInstance:
    return CardInstance(
        card_id=CardId.SIGNAL_BOOST, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        keywords=frozenset({"exhaust"}),
        effect_vars={"signal_boost": 1}, instance_id=_get_next_id(),
    )


def make_spinner() -> CardInstance:
    return CardInstance(
        card_id=CardId.SPINNER_CARD, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        effect_vars={"spinner": 1}, instance_id=_get_next_id(),
    )


def make_supercritical() -> CardInstance:
    return CardInstance(
        card_id=CardId.SUPERCRITICAL, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        keywords=frozenset({"exhaust"}),
        effect_vars={"energy": 4}, instance_id=_get_next_id(),
    )


def make_trash_to_treasure() -> CardInstance:
    return CardInstance(
        card_id=CardId.TRASH_TO_TREASURE, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        instance_id=_get_next_id(),
    )


def make_voltaic() -> CardInstance:
    return CardInstance(
        card_id=CardId.VOLTAIC, cost=2, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        keywords=frozenset({"exhaust"}),
        effect_vars={"calc_base": 0, "calc_extra": 1}, instance_id=_get_next_id(),
    )


def make_biased_cognition() -> CardInstance:
    return CardInstance(
        card_id=CardId.BIASED_COGNITION_CARD, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.ANCIENT,
        effect_vars={"focus": 4, "biased_cognition": 1}, instance_id=_get_next_id(),
    )


def make_quadcast() -> CardInstance:
    return CardInstance(
        card_id=CardId.QUADCAST, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.ANCIENT,
        instance_id=_get_next_id(),
    )


def create_defect_starter_deck() -> list[CardInstance]:
    """Create the 12-card Defect starting deck: 4 Strike, 4 Defend, Zap, Dualcast."""
    deck = []
    for _ in range(4):
        deck.append(make_strike_defect())
    for _ in range(4):
        deck.append(make_defend_defect())
    deck.append(make_zap())
    deck.append(make_dualcast())
    return deck
