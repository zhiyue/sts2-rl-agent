"""Defect card effects and factories — all 88 cards."""

from __future__ import annotations

from sts2_env.cards.base import (
    CardInstance,
    _get_next_id,
    increase_base_block,
    increase_base_damage,
    new_card_instance_id,
)
from sts2_env.cards.factory import create_character_cards
from sts2_env.cards.registry import (
    register_after_card_generated_for_combat_hook,
    register_after_card_played_hook,
    register_effect,
    register_self_mutating_block,
    register_self_mutating_damage,
)
from sts2_env.core.enums import (
    CardId, CardType, TargetType, CardRarity, ValueProp, PowerId, OrbType,
)
from sts2_env.core.damage import calculate_damage, apply_damage, calculate_block
from sts2_env.core.hooks import fire_after_block_gained
from sts2_env.core.creature import Creature
from sts2_env.core.combat import CombatState


STRIKE_DEFECT_COST = 1
STRIKE_DEFECT_DAMAGE = 6
STRIKE_DEFECT_UPGRADED_DAMAGE = 9
DEFEND_DEFECT_COST = 1
DEFEND_DEFECT_BLOCK = 5
DEFEND_DEFECT_UPGRADED_BLOCK = 8
ZAP_COST = 1
ZAP_UPGRADED_COST = 0
CHARGE_BATTERY_BLOCK = 7
CHARGE_BATTERY_UPGRADED_BLOCK = 10
CHARGE_BATTERY_ENERGY_KEY = "energy"
CHARGE_BATTERY_ENERGY_NEXT_TURN = 1
CLAW_DAMAGE = 3
CLAW_UPGRADED_DAMAGE = 4
CLAW_INCREASE_KEY = "increase"
CLAW_INCREASE = 2
CLAW_UPGRADED_INCREASE = 3
BOOST_AWAY_BLOCK = 6
BOOST_AWAY_UPGRADED_BLOCK = 9
BOOT_SEQUENCE_BLOCK = 10
BOOT_SEQUENCE_UPGRADED_BLOCK = 13
BULK_UP_STRENGTH_KEY = "strength"
BULK_UP_DEXTERITY_KEY = "dexterity"
BULK_UP_ORB_SLOTS_KEY = "orb_slots"
BULK_UP_POWER = 2
BULK_UP_UPGRADED_POWER = 3
BULK_UP_ORB_SLOTS = 1
CHAOS_REPEAT_KEY = "repeat"
CHAOS_REPEAT = 1
CHAOS_UPGRADED_REPEAT = 2
ADAPTIVE_STRIKE_DAMAGE = 18
ADAPTIVE_STRIKE_UPGRADED_DAMAGE = 23
ALL_FOR_ONE_DAMAGE = 10
ALL_FOR_ONE_UPGRADED_DAMAGE = 14
BIASED_COGNITION_FOCUS_KEY = "focus_power"
BIASED_COGNITION_POWER_KEY = "biased_cognition_power"
BIASED_COGNITION_FOCUS = 4
BIASED_COGNITION_UPGRADED_FOCUS = 5
BIASED_COGNITION_POWER = 1
FOCUSED_STRIKE_DAMAGE = 9
FOCUSED_STRIKE_UPGRADED_DAMAGE = 11
FOCUSED_STRIKE_POWER_KEY = "focus_power"
FOCUSED_STRIKE_POWER = 1
FOCUSED_STRIKE_UPGRADED_POWER = 2
ENERGY_SURGE_ENERGY_KEY = "energy"
ENERGY_SURGE_ENERGY = 2
ENERGY_SURGE_UPGRADED_ENERGY = 3
FERAL_POWER_KEY = "feral_power"
FERAL_POWER = 1
FTL_DAMAGE = 5
FTL_UPGRADED_DAMAGE = 6
FTL_CARDS_KEY = "cards"
FTL_CARDS = 1
FTL_PLAY_MAX_KEY = "play_max"
FTL_PLAY_MAX = 3
FTL_UPGRADED_PLAY_MAX = 4
GLACIER_BLOCK = 6
GLACIER_UPGRADED_BLOCK = 9
GLASSWORK_BLOCK = 5
GLASSWORK_UPGRADED_BLOCK = 8
COOLANT_POWER_KEY = "coolant_power"
COOLANT_POWER = 2
COOLANT_UPGRADED_POWER = 3
CREATIVE_AI_POWER_KEY = "creative_ai"
CREATIVE_AI_POWER = 1
CREATIVE_AI_COST = 3
CREATIVE_AI_UPGRADED_COST = 2
DEFRAGMENT_FOCUS_KEY = "focus_power"
DEFRAGMENT_FOCUS = 1
DEFRAGMENT_UPGRADED_FOCUS = 2
GO_FOR_THE_EYES_DAMAGE = 3
GO_FOR_THE_EYES_UPGRADED_DAMAGE = 4
GO_FOR_THE_EYES_WEAK_KEY = "weak"
GO_FOR_THE_EYES_WEAK = 1
GO_FOR_THE_EYES_UPGRADED_WEAK = 2
GUNK_UP_DAMAGE = 4
GUNK_UP_UPGRADED_DAMAGE = 5
GUNK_UP_REPEAT_KEY = "repeat"
GUNK_UP_REPEAT = 3
HOTFIX_FOCUS_KEY = "focus_power"
# Hotfix.cs:18 CanonicalVars = PowerVar<FocusPower>(2); OnUpgrade (31-34) only
# removes Exhaust, focus stays 2.
HOTFIX_FOCUS = 2
HOTFIX_UPGRADED_FOCUS = 2
LEAP_BLOCK = 9
LEAP_UPGRADED_BLOCK = 12
HAILSTORM_POWER_KEY = "hailstorm_power"
HAILSTORM_POWER = 6
HAILSTORM_UPGRADED_POWER = 8
LIGHTNING_ROD_BLOCK = 4
LIGHTNING_ROD_UPGRADED_BLOCK = 7
LIGHTNING_ROD_POWER_KEY = "lightning_rod_power"
LIGHTNING_ROD_POWER = 2
MACHINE_LEARNING_CARDS_KEY = "cards"
MACHINE_LEARNING_CARDS = 1
MOMENTUM_STRIKE_DAMAGE = 10
MOMENTUM_STRIKE_UPGRADED_DAMAGE = 13
OVERCLOCK_CARDS_KEY = "cards"
OVERCLOCK_CARDS = 2
OVERCLOCK_UPGRADED_CARDS = 3
SKIM_CARDS_KEY = "cards"
SKIM_CARDS = 3
SKIM_UPGRADED_CARDS = 4
STORM_POWER_KEY = "storm_power"
STORM_POWER = 1
STORM_UPGRADED_POWER = 2
SWEEPING_BEAM_DAMAGE = 6
SWEEPING_BEAM_UPGRADED_DAMAGE = 9
SWEEPING_BEAM_CARDS_KEY = "cards"
SWEEPING_BEAM_CARDS = 1
NULL_DAMAGE = 10
NULL_UPGRADED_DAMAGE = 13
NULL_WEAK_KEY = "weak"
NULL_WEAK = 2
NULL_UPGRADED_WEAK = 3
THUNDER_POWER_KEY = "thunder_power"
THUNDER_POWER = 6
THUNDER_UPGRADED_POWER = 8
TURBO_ENERGY_KEY = "energy"
TURBO_ENERGY = 2
TURBO_UPGRADED_ENERGY = 3
# Uproar.cs:16 DamageVar(6), OnUpgrade (43-46) UpgradeValueBy(2); 2 hits (OnPlay WithHitCount(2)).
UPROAR_DAMAGE = 6
UPROAR_UPGRADED_DAMAGE = 8
UPROAR_HITS = 2
GENETIC_ALGORITHM_BLOCK_KEY = "block"
GENETIC_ALGORITHM_BLOCK = 1
GENETIC_ALGORITHM_INCREASE_KEY = "increase"
GENETIC_ALGORITHM_INCREASE = 3
GENETIC_ALGORITHM_UPGRADED_INCREASE = 4
HELIX_DRILL_DAMAGE = 3
HELIX_DRILL_UPGRADED_DAMAGE = 5
HELIX_DRILL_CALC_BASE_KEY = "calc_base"
HELIX_DRILL_CALC_BASE = 0
HELIX_DRILL_CALC_EXTRA_KEY = "calc_extra"
HELIX_DRILL_CALC_EXTRA = 1
# Hyperbeam.cs:20-24 DamageVar(28) + PowerVar<FocusPower>(3); OnUpgrade (59-62) +8 damage.
HYPERBEAM_DAMAGE = 28
HYPERBEAM_UPGRADED_DAMAGE = 36
HYPERBEAM_FOCUS_KEY = "focus_power"
HYPERBEAM_FOCUS = 3
METEOR_STRIKE_DAMAGE = 24
METEOR_STRIKE_UPGRADED_DAMAGE = 30
METEOR_STRIKE_PLASMA_ORBS = 3
MODDED_CARDS_KEY = "cards"
MODDED_CARDS = 1
MODDED_UPGRADED_CARDS = 2
MODDED_REPEAT_KEY = "repeat"
MODDED_REPEAT = 1
MODDED_COST_INCREASE = 1
REFRACT_REPEAT_KEY = "repeat"
REFRACT_REPEAT = 2
ICE_LANCE_REPEAT_KEY = "repeat"
ICE_LANCE_REPEAT = 3
SHADOW_SHIELD_BLOCK = 11
SHADOW_SHIELD_UPGRADED_BLOCK = 15
# Synthesis.cs:15 DamageVar(14), OnUpgrade (31-34) UpgradeValueBy(6).
SYNTHESIS_DAMAGE = 14
SYNTHESIS_UPGRADED_DAMAGE = 20
SYNTHESIS_FREE_POWER = 1
# TeslaCoil.cs:19 DamageVar(3), OnUpgrade (43-46) UpgradeValueBy(1); upgraded also
# triggers each Lightning orb passive a second time (OnPlay 36-39).
TESLA_COIL_DAMAGE = 3
TESLA_COIL_UPGRADED_DAMAGE = 4
VOLTAIC_CALC_BASE_KEY = "calc_base"
VOLTAIC_CALC_BASE = 0
VOLTAIC_CALC_EXTRA_KEY = "calc_extra"
VOLTAIC_CALC_EXTRA = 1
ROCKET_PUNCH_DAMAGE = 13
ROCKET_PUNCH_UPGRADED_DAMAGE = 14
ROCKET_PUNCH_CARDS_KEY = "cards"
ROCKET_PUNCH_CARDS = 1
ROCKET_PUNCH_UPGRADED_CARDS = 2
SCRAPE_DAMAGE = 7
SCRAPE_UPGRADED_DAMAGE = 10
SCRAPE_CARDS_KEY = "cards"
SCRAPE_CARDS = 4
SCRAPE_UPGRADED_CARDS = 5
SYNCHRONIZE_CALC_BASE_KEY = "calc_base"
SYNCHRONIZE_CALC_BASE = 0
SYNCHRONIZE_CALC_EXTRA_KEY = "calc_extra"
SYNCHRONIZE_FOCUS_PER_ORB_TYPE = 2
MULTI_CAST_UPGRADED_EXTRA_EVOKE = 1
QUADCAST_COST = 1
QUADCAST_UPGRADED_COST = 0
QUADCAST_REPEAT = 4
REBOOT_CARDS_KEY = "cards"
REBOOT_CARDS = 4
REBOOT_UPGRADED_CARDS = 6
# Shatter.cs:18 DamageVar(7), OnUpgrade (38-41) UpgradeValueBy(4).
SHATTER_DAMAGE = 7
SHATTER_UPGRADED_DAMAGE = 11
SIGNAL_BOOST_COST = 1
SIGNAL_BOOST_UPGRADED_COST = 0
SIGNAL_BOOST_POWER_KEY = "signal_boost_power"
SIGNAL_BOOST_POWER = 1
SMOKESTACK_POWER_KEY = "smokestack_power"
SMOKESTACK_POWER = 5
SMOKESTACK_UPGRADED_POWER = 7
SUBROUTINE_COST = 1
SUBROUTINE_UPGRADED_COST = 0
SUBROUTINE_POWER = 1
SUPERCRITICAL_ENERGY_KEY = "energy"
SUPERCRITICAL_ENERGY = 4
SUPERCRITICAL_UPGRADED_ENERGY = 6
TRASH_TO_TREASURE_POWER = 1


def _owner(card: CardInstance, combat: CombatState) -> Creature:
    return (
        getattr(card, "owner", None)
        or getattr(getattr(combat, "active_card_source", None), "owner", None)
        or combat.primary_player
    )


def _gain_resolved_block(creature: Creature, block: int, combat: CombatState) -> int:
    before = creature.block
    creature.gain_block(block)
    gained = creature.block - before
    if gained > 0:
        fire_after_block_gained(creature, gained, combat, ValueProp.MOVE, combat.active_card_play_token)
    return gained


# ---------------------------------------------------------------------------
#  Orb helpers — delegates to combat orb queue when available
# ---------------------------------------------------------------------------

def _channel_orb(combat: CombatState, orb_type: OrbType) -> None:
    """Channel an orb through the combat orb queue, if present."""
    owner = (
        getattr(getattr(combat, "active_card_source", None), "owner", None)
        or combat.player
    )
    combat.channel_orb(owner, orb_type)


def _evoke_front(combat: CombatState) -> None:
    """Evoke the front orb."""
    queue = getattr(combat, 'orb_queue', None)
    if queue is not None and queue.orbs:
        queue.evoke_front(combat)


def _passive_front(combat: CombatState) -> None:
    queue = getattr(combat, 'orb_queue', None)
    if queue is not None and queue.orbs:
        queue.orbs[0].on_evoke(combat)


def _trigger_all_passives(combat: CombatState) -> None:
    """Trigger all orb passives once."""
    queue = getattr(combat, 'orb_queue', None)
    if queue is not None:
        for orb in list(queue.orbs):
            orb.on_passive(combat)


def _trigger_lightning_passives(
    combat: CombatState, target: Creature, times: int = 1
) -> None:
    """Trigger each Lightning orb's passive `times` times against `target`.

    TeslaCoil.cs OnPlay (32-40) triggers every Lightning orb passive once, and a
    second time per orb when the card is upgraded.
    """
    queue = getattr(combat, 'orb_queue', None)
    if queue is not None:
        for orb in list(queue.orbs):
            if orb.orb_type != OrbType.LIGHTNING:
                continue
            for _ in range(times):
                value = orb.get_passive_value(combat)
                if value > 0:
                    apply_damage(target, value, ValueProp.UNPOWERED, combat, combat.player)


def _get_orb_count(combat: CombatState) -> int:
    queue = getattr(combat, 'orb_queue', None)
    return len(queue.orbs) if queue is not None else 0


def _get_unique_orb_type_count(combat: CombatState) -> int:
    queue = getattr(combat, 'orb_queue', None)
    if queue is None:
        return 0
    return len({orb.orb_type for orb in queue.orbs})


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


def _target_intends_to_attack(combat: CombatState, target: Creature | None) -> bool:
    if target is None:
        return False
    ai = combat.enemy_ais.get(target.combat_id)
    if ai is None:
        return False
    move = getattr(ai, "current_move", None)
    intents = getattr(move, "intents", ())
    return any(getattr(intent, "is_attack", False) for intent in intents)


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
    _gain_resolved_block(owner, blk, combat)


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
    owner = _owner(card, combat)
    hits = _get_orb_count(combat)
    for _ in range(hits):
        if owner.is_dead or target.is_dead:
            break
        dmg = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
        apply_damage(target, dmg, ValueProp.MOVE, combat, owner)


@register_effect(CardId.BEAM_CELL)
def beam_cell(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    combat.apply_power_to(target, PowerId.VULNERABLE, card.effect_vars.get("vulnerable", 1))


@register_effect(CardId.BOOST_AWAY)
def boost_away(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    blk = calculate_block(card.base_block, owner, ValueProp.MOVE, combat, card_source=card)
    _gain_resolved_block(owner, blk, combat)
    combat.add_generated_card_to_creature_discard(owner, _make_dazed())


@register_effect(CardId.CHARGE_BATTERY)
def charge_battery(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    blk = calculate_block(card.base_block, _owner(card, combat), ValueProp.MOVE, combat, card_source=card)
    _gain_resolved_block(_owner(card, combat), blk, combat)
    combat.apply_power_to(
        _owner(card, combat),
        PowerId.ENERGY_NEXT_TURN,
        card.effect_vars.get(CHARGE_BATTERY_ENERGY_KEY, CHARGE_BATTERY_ENERGY_NEXT_TURN),
    )


@register_effect(CardId.CLAW)
def claw(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    # All Claw copies gain +Increase damage permanently
    increase = card.effect_vars.get(CLAW_INCREASE_KEY, CLAW_INCREASE)
    for claw_card in combat._all_cards_for_creature(_owner(card, combat)):
        if claw_card.card_id == CardId.CLAW:
            increase_base_damage(claw_card, increase)


register_self_mutating_damage(CardId.CLAW)


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
    draw = _get_unique_orb_type_count(combat)
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
    combat.apply_power_to(
        _owner(card, combat),
        PowerId.FOCUSED_STRIKE,
        card.effect_vars.get(FOCUSED_STRIKE_POWER_KEY, FOCUSED_STRIKE_POWER),
    )


@register_effect(CardId.GO_FOR_THE_EYES)
def go_for_the_eyes(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    if _target_intends_to_attack(combat, target):
        combat.apply_power_to(target, PowerId.WEAK, card.effect_vars.get(GO_FOR_THE_EYES_WEAK_KEY, GO_FOR_THE_EYES_WEAK))


@register_effect(CardId.GUNK_UP)
def gunk_up(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    for _ in range(card.effect_vars.get(GUNK_UP_REPEAT_KEY, GUNK_UP_REPEAT)):
        if owner.is_dead or target.is_dead:
            break
        dmg = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
        apply_damage(target, dmg, ValueProp.MOVE, combat, owner)
    combat.add_generated_card_to_creature_discard(owner, _make_slimed())


@register_effect(CardId.HOLOGRAM)
def hologram(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    blk = calculate_block(card.base_block, _owner(card, combat), ValueProp.MOVE, combat, card_source=card)
    _gain_resolved_block(_owner(card, combat), blk, combat)
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
    combat.apply_power_to(_owner(card, combat), PowerId.HOTFIX, card.effect_vars.get(HOTFIX_FOCUS_KEY, HOTFIX_FOCUS))


@register_effect(CardId.LEAP)
def leap(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    blk = calculate_block(card.base_block, _owner(card, combat), ValueProp.MOVE, combat, card_source=card)
    _gain_resolved_block(_owner(card, combat), blk, combat)


@register_effect(CardId.LIGHTNING_ROD)
def lightning_rod(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    blk = calculate_block(card.base_block, _owner(card, combat), ValueProp.MOVE, combat, card_source=card)
    _gain_resolved_block(_owner(card, combat), blk, combat)
    combat.apply_power_to(
        _owner(card, combat),
        PowerId.LIGHTNING_ROD,
        card.effect_vars.get(LIGHTNING_ROD_POWER_KEY, LIGHTNING_ROD_POWER),
    )


@register_effect(CardId.MOMENTUM_STRIKE)
def momentum_strike(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    card.set_combat_cost(0)


@register_effect(CardId.SWEEPING_BEAM)
def sweeping_beam(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    for enemy in combat.hittable_enemies:
        dmg = calculate_damage(card.base_damage, _owner(card, combat), enemy, ValueProp.MOVE, combat)
        apply_damage(enemy, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    combat._draw_cards(card.effect_vars.get(SWEEPING_BEAM_CARDS_KEY, SWEEPING_BEAM_CARDS))


@register_effect(CardId.TURBO)
def turbo(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    combat.gain_energy(owner, card.effect_vars.get(TURBO_ENERGY_KEY, TURBO_ENERGY))
    combat.add_generated_card_to_creature_discard(owner, _make_void())


@register_effect(CardId.UPROAR)
def uproar(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    for _ in range(UPROAR_HITS):
        if owner.is_dead or target.is_dead:
            break
        dmg = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
        apply_damage(target, dmg, ValueProp.MOVE, combat, owner)
    candidates = [c for c in combat.draw_pile if c.card_type == CardType.ATTACK and not c.is_unplayable]
    if not candidates:
        candidates = [c for c in combat.draw_pile if c.card_type == CardType.ATTACK]
    if candidates:
        combat.stable_shuffle_cards(candidates, combat.shuffle_rng)
        combat.auto_play_card(candidates[0])


# ---------------------------------------------------------------------------
#  UNCOMMON (32)
# ---------------------------------------------------------------------------

@register_effect(CardId.BOOT_SEQUENCE)
def boot_sequence(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    blk = calculate_block(card.base_block, _owner(card, combat), ValueProp.MOVE, combat, card_source=card)
    _gain_resolved_block(_owner(card, combat), blk, combat)


@register_effect(CardId.BULK_UP)
def bulk_up(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    combat.apply_power_to(
        owner,
        PowerId.STRENGTH,
        card.effect_vars.get(BULK_UP_STRENGTH_KEY, BULK_UP_POWER),
    )
    combat.apply_power_to(
        owner,
        PowerId.DEXTERITY,
        card.effect_vars.get(BULK_UP_DEXTERITY_KEY, BULK_UP_POWER),
    )
    _remove_orb_slot(combat, card.effect_vars.get(BULK_UP_ORB_SLOTS_KEY, BULK_UP_ORB_SLOTS))


@register_effect(CardId.CAPACITOR)
def capacitor(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    slots = card.effect_vars.get("repeat", 2)
    _add_orb_slot(combat, slots)


@register_effect(CardId.CHAOS)
def chaos(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    orb_types = [OrbType.LIGHTNING, OrbType.FROST, OrbType.DARK, OrbType.PLASMA, OrbType.GLASS]
    repeat = card.effect_vars.get(CHAOS_REPEAT_KEY, CHAOS_REPEAT)
    for _ in range(max(0, repeat)):
        chosen = combat.combat_orbs_rng.choice(orb_types)
        _channel_orb(combat, chosen)


@register_effect(CardId.CHILL)
def chill(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    for _ in combat.hittable_enemies:
        _channel_orb(combat, OrbType.FROST)


@register_effect(CardId.COMPACT)
def compact(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    from sts2_env.cards.status import make_fuel

    blk = calculate_block(card.base_block, _owner(card, combat), ValueProp.MOVE, combat, card_source=card)
    _gain_resolved_block(_owner(card, combat), blk, combat)
    status_cards = [c for c in list(combat.hand) if c.card_type == CardType.STATUS]
    for status_card in status_cards:
        combat.transform_card(status_card, make_fuel(upgraded=card.upgraded))


@register_effect(CardId.DARKNESS_CARD)
def darkness(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    _channel_orb(combat, OrbType.DARK)
    state = combat.combat_player_state_for(owner)
    queue = getattr(state, "orb_queue", None) if state is not None else None
    if queue is None:
        return
    trigger_count = card.effect_vars.get("passives", 2 if card.upgraded else 1)
    for orb in list(queue.orbs):
        if orb.orb_type != OrbType.DARK:
            continue
        for _ in range(trigger_count):
            orb.on_passive(combat)


@register_effect(CardId.DOUBLE_ENERGY)
def double_energy(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.energy *= 2


@register_effect(CardId.ENERGY_SURGE)
def energy_surge(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # EnergySurge.cs OnPlay: for c in CombatState.GetTeammatesOf(Owner.Creature)
    # where c.IsAlive && c.IsPlayer — GetTeammatesOf returns the whole side
    # (CombatState.cs:397), so the caster itself is included in single-player.
    owner = _owner(card, combat)
    energy = card.effect_vars.get(ENERGY_SURGE_ENERGY_KEY, ENERGY_SURGE_ENERGY)
    teammates = [owner] + combat.get_player_allies_of(owner)
    for teammate in teammates:
        if not getattr(teammate, "is_alive", True):
            continue
        combat.gain_energy(teammate, energy)


@register_effect(CardId.FERAL)
def feral(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(
        _owner(card, combat),
        PowerId.FERAL,
        card.effect_vars.get(FERAL_POWER_KEY, FERAL_POWER),
    )


@register_effect(CardId.FIGHT_THROUGH)
def fight_through(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    blk = calculate_block(card.base_block, owner, ValueProp.MOVE, combat, card_source=card)
    _gain_resolved_block(owner, blk, combat)
    for _ in range(card.effect_vars.get("wounds", 2)):
        combat.add_generated_card_to_creature_discard(owner, _make_wound())


@register_effect(CardId.FTL)
def ftl(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    dmg = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, owner)
    previous_plays = combat.count_card_plays_finished_this_turn(owner)
    if previous_plays < card.effect_vars.get(
        FTL_PLAY_MAX_KEY,
        FTL_UPGRADED_PLAY_MAX if card.upgraded else FTL_PLAY_MAX,
    ):
        combat.draw_cards(owner, card.effect_vars.get(FTL_CARDS_KEY, FTL_CARDS))


@register_effect(CardId.FUSION)
def fusion(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _channel_orb(combat, OrbType.PLASMA)


@register_effect(CardId.GLACIER)
def glacier(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    blk = calculate_block(card.base_block, _owner(card, combat), ValueProp.MOVE, combat, card_source=card)
    _gain_resolved_block(_owner(card, combat), blk, combat)
    _channel_orb(combat, OrbType.FROST)
    _channel_orb(combat, OrbType.FROST)


@register_effect(CardId.GLASSWORK)
def glasswork(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    blk = calculate_block(card.base_block, _owner(card, combat), ValueProp.MOVE, combat, card_source=card)
    _gain_resolved_block(_owner(card, combat), blk, combat)
    _channel_orb(combat, OrbType.GLASS)


@register_effect(CardId.HAILSTORM)
def hailstorm(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(
        _owner(card, combat),
        PowerId.HAILSTORM,
        card.effect_vars.get(HAILSTORM_POWER_KEY, HAILSTORM_POWER),
    )


@register_effect(CardId.ITERATION_CARD)
def iteration(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.ITERATION, card.effect_vars.get("iteration_power", 2))


@register_effect(CardId.LOOP_CARD)
def loop(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.LOOP, card.effect_vars.get("loop", 1))


@register_effect(CardId.NULL_CARD)
def null_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    combat.apply_power_to(target, PowerId.WEAK, card.effect_vars.get(NULL_WEAK_KEY, NULL_WEAK))
    _channel_orb(combat, OrbType.DARK)


@register_effect(CardId.OVERCLOCK)
def overclock(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    combat.draw_cards(owner, card.effect_vars.get(OVERCLOCK_CARDS_KEY, OVERCLOCK_CARDS))
    combat.add_generated_card_to_creature_discard(owner, _make_burn())


@register_effect(CardId.REFRACT)
def refract(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    for _ in range(card.effect_vars.get(REFRACT_REPEAT_KEY, REFRACT_REPEAT)):
        if owner.is_dead or target.is_dead:
            break
        dmg = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
        apply_damage(target, dmg, ValueProp.MOVE, combat, owner)
    for _ in range(card.effect_vars.get(REFRACT_REPEAT_KEY, REFRACT_REPEAT)):
        _channel_orb(combat, OrbType.GLASS)


@register_effect(CardId.ROCKET_PUNCH)
def rocket_punch(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    combat._draw_cards(card.effect_vars.get(ROCKET_PUNCH_CARDS_KEY, ROCKET_PUNCH_CARDS))


@register_after_card_generated_for_combat_hook(CardId.ROCKET_PUNCH)
def rocket_punch_after_card_generated_for_combat(
    card: CardInstance,
    generated_card: CardInstance,
    added_by_player: bool,
    combat: CombatState,
) -> None:
    if generated_card.card_type is not CardType.STATUS:
        return
    owner = getattr(card, "owner", None)
    generated_owner = getattr(generated_card, "owner", None)
    if owner is not None and generated_owner is not owner:
        return
    # RocketPunch.cs AfterCardGeneratedForCombat: base.EnergyCost.SetUntilPlayed(0)
    # — a LocalCostModifier(LocalCostType.Absolute, WhenPlayed) on RocketPunch
    # itself (CardEnergyCost.cs:175), so RocketPunch costs 0 until it is next
    # played, persisting across end-of-turn cleanup. Tracked with combat_vars
    # markers (not _turn_cost_override, which end_of_turn_cleanup pops) and
    # restored by rocket_punch_after_card_played below.
    if "_until_played_cost" in card.combat_vars:
        return
    card.combat_vars["_until_played_cost"] = 0
    card.combat_vars["_until_played_prev_cost"] = card.cost
    card.cost = 0


@register_after_card_played_hook(CardId.ROCKET_PUNCH)
def rocket_punch_after_card_played(
    card: CardInstance,
    played_card: CardInstance,
    owner: Creature,
    combat: CombatState,
) -> None:
    # C# LocalCostModifierExpiration.WhenPlayed: the 0-cost modifier expires when
    # RocketPunch is played, restoring its printed cost.
    if played_card is not card:
        return
    if card.combat_vars.pop("_until_played_cost", None) is None:
        return
    prev_cost = card.combat_vars.pop("_until_played_prev_cost", None)
    if prev_cost is not None:
        card.cost = prev_cost


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
    if combat.is_over:
        return
    draw = card.effect_vars.get(SCRAPE_CARDS_KEY, SCRAPE_CARDS)
    combat._draw_cards(draw)
    # Scrape.cs OnPlay: discard drawn cards where
    # c.EnergyCost.GetWithModifiers(CostModifiers.Local) != 0 || c.EnergyCost.CostsX.
    # Local cost = printed cost plus the card's own local modifiers (Python:
    # card.cost, which set_temporary_cost_for_turn / set_combat_cost mutate);
    # global power/relic cost modifiers and star costs are NOT part of
    # EnergyCost, so 0-cost star cards are not discarded.
    to_discard = [c for c in combat.hand[-draw:] if c.cost != 0 or c.has_energy_cost_x]
    for c in to_discard:
        combat.hand.remove(c)
        combat.discard_pile.append(c)


@register_effect(CardId.SHADOW_SHIELD)
def shadow_shield(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    blk = calculate_block(card.base_block, _owner(card, combat), ValueProp.MOVE, combat, card_source=card)
    _gain_resolved_block(_owner(card, combat), blk, combat)
    _channel_orb(combat, OrbType.DARK)


@register_effect(CardId.SKIM)
def skim(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat._draw_cards(card.effect_vars.get(SKIM_CARDS_KEY, SKIM_CARDS))


@register_effect(CardId.SMOKESTACK)
def smokestack(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.SMOKESTACK, card.effect_vars.get(SMOKESTACK_POWER_KEY, SMOKESTACK_POWER))


@register_effect(CardId.STORM_CARD)
def storm(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.STORM, card.effect_vars.get(STORM_POWER_KEY, STORM_POWER))


@register_effect(CardId.SUBROUTINE)
def subroutine(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.SUBROUTINE, SUBROUTINE_POWER)


@register_effect(CardId.SUNDER)
def sunder(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
    result = apply_damage(target, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    # Gain energy if enemy was killed
    if result.was_killed:
        combat.gain_energy(_owner(card, combat), card.effect_vars.get("energy", 3))


@register_effect(CardId.SYNCHRONIZE)
def synchronize(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Gain Focus based on orb count
    orb_count = _get_unique_orb_type_count(combat)
    extra = card.effect_vars.get(SYNCHRONIZE_CALC_EXTRA_KEY, SYNCHRONIZE_FOCUS_PER_ORB_TYPE)
    focus = orb_count * extra
    if focus > 0:
        combat.apply_power_to(_owner(card, combat), PowerId.SYNCHRONIZE, focus)


@register_effect(CardId.SYNTHESIS)
def synthesis(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    combat.apply_power_to(_owner(card, combat), PowerId.FREE_POWER, SYNTHESIS_FREE_POWER)


@register_effect(CardId.TEMPEST)
def tempest(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # X-cost: channel X Lightning orbs (+1 when upgraded).
    x = getattr(card, "energy_spent", 0)
    if card.upgraded:
        x += 1
    for _ in range(max(0, x)):
        _channel_orb(combat, OrbType.LIGHTNING)


@register_effect(CardId.TESLA_COIL)
def tesla_coil(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    # TeslaCoil.cs OnPlay (32-40): upgraded triggers each Lightning orb passive twice.
    _trigger_lightning_passives(combat, target, times=2 if card.upgraded else 1)


@register_effect(CardId.THUNDER_CARD)
def thunder_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(
        _owner(card, combat),
        PowerId.THUNDER,
        card.effect_vars.get(THUNDER_POWER_KEY, THUNDER_POWER),
    )


@register_effect(CardId.WHITE_NOISE)
def white_noise(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # WhiteNoise.cs OnPlay: CardFactory.GetDistinctForCombat(base.Owner,
    # base.Owner.Character.CardPool.GetUnlockedCards(...).Where(Type == Power), 1,
    # rng) — the pool is the CARD OWNER's character (not combat.character_id,
    # which in multiplayer is the primary player). GetDistinctForCombat picks
    # count distinct cards via TakeRandom(count) (no replacement within the
    # call); with count=1 there is no cross-combat dedup state in C#.
    owner = _owner(card, combat)
    owner_state = combat.combat_player_state_for(owner)
    character_id = getattr(owner_state, "character_id", None) or combat.character_id
    generated = create_character_cards(
        character_id,
        combat.combat_card_generation_rng,
        1,
        card_type=CardType.POWER,
        generation_context="combat",
        is_multiplayer=combat.is_multiplayer,
    )
    if not generated:
        return
    generated[0].set_temporary_free_this_turn()
    combat.add_generated_card_to_creature_hand(owner, generated[0])


# ---------------------------------------------------------------------------
#  RARE (26)
# ---------------------------------------------------------------------------

@register_effect(CardId.ADAPTIVE_STRIKE)
def adaptive_strike(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
    owner = _owner(card, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, owner)
    clone = card.clone(new_card_instance_id())
    clone.set_combat_cost(0)
    combat.add_generated_card_to_creature_discard(owner, clone)


@register_effect(CardId.ALL_FOR_ONE)
def all_for_one(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    owner = _owner(card, combat)
    state = combat.combat_player_state_for(owner)
    if state is None:
        return
    for discarded in list(state.discard):
        if discarded.card_type not in (CardType.ATTACK, CardType.SKILL, CardType.POWER):
            continue
        if discarded.has_energy_cost_x:
            continue
        if combat.modified_card_cost(owner, discarded) == 0:
            combat.move_card_to_creature_hand(owner, discarded)


@register_effect(CardId.BUFFER_CARD)
def buffer(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.BUFFER, card.effect_vars.get("buffer_power", 1))


@register_effect(CardId.CONSUMING_SHADOW)
def consuming_shadow(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    for _ in range(card.effect_vars.get("repeat", 2)):
        _channel_orb(combat, OrbType.DARK)
    combat.apply_power_to(_owner(card, combat), PowerId.CONSUMING_SHADOW, card.effect_vars.get("consuming_shadow_power", 1))


@register_effect(CardId.COOLANT)
def coolant(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(
        _owner(card, combat),
        PowerId.COOLANT,
        card.effect_vars.get(COOLANT_POWER_KEY, COOLANT_POWER),
    )


@register_effect(CardId.CREATIVE_AI_CARD)
def creative_ai(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(
        _owner(card, combat),
        PowerId.CREATIVE_AI,
        card.effect_vars.get(CREATIVE_AI_POWER_KEY, CREATIVE_AI_POWER),
    )


@register_effect(CardId.DEFRAGMENT)
def defragment(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(
        _owner(card, combat),
        PowerId.FOCUS,
        card.effect_vars.get(DEFRAGMENT_FOCUS_KEY, DEFRAGMENT_FOCUS),
    )


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
        if owner.is_dead:
            break
        alive = combat.hittable_enemies
        if not alive:
            break
        t = combat.combat_targets_rng.choice(alive)
        dmg = calculate_damage(card.base_damage, owner, t, ValueProp.MOVE, combat)
        apply_damage(t, dmg, ValueProp.MOVE, combat, owner)


@register_effect(CardId.GENETIC_ALGORITHM)
def genetic_algorithm(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Gain Block (self-mutating: gains +3 block permanently each play)
    block_amt = card.effect_vars.get(GENETIC_ALGORITHM_BLOCK_KEY, 0)
    owner = _owner(card, combat)
    blk = calculate_block(block_amt, owner, ValueProp.MOVE, combat, card_source=card)
    _gain_resolved_block(owner, blk, combat)
    increase = card.effect_vars.get(GENETIC_ALGORITHM_INCREASE_KEY, GENETIC_ALGORITHM_INCREASE)
    increase_base_block(card, increase)


register_self_mutating_block(CardId.GENETIC_ALGORITHM)


@register_effect(CardId.HELIX_DRILL)
def helix_drill(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    hits = combat.energy_spent_this_turn(owner)
    state = combat.combat_player_state_for(owner)
    if state is not None and card in state.play:
        hits -= combat.modified_card_cost(owner, card)
    for _ in range(max(0, hits)):
        if owner.is_dead or target.is_dead:
            break
        dmg = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
        apply_damage(target, dmg, ValueProp.MOVE, combat, owner)


@register_effect(CardId.HYPERBEAM)
def hyperbeam(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    for enemy in combat.hittable_enemies:
        dmg = calculate_damage(card.base_damage, _owner(card, combat), enemy, ValueProp.MOVE, combat)
        apply_damage(enemy, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    # Lose Focus
    combat.apply_power_to(
        _owner(card, combat),
        PowerId.FOCUS,
        -card.effect_vars.get(HYPERBEAM_FOCUS_KEY, HYPERBEAM_FOCUS),
    )


@register_effect(CardId.ICE_LANCE)
def ice_lance(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    dmg = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, owner)
    for _ in range(card.effect_vars.get(ICE_LANCE_REPEAT_KEY, ICE_LANCE_REPEAT)):
        _channel_orb(combat, OrbType.FROST)


@register_effect(CardId.IGNITION)
def ignition(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    combat.channel_orb(target, OrbType.PLASMA)


@register_effect(CardId.MACHINE_LEARNING_CARD)
def machine_learning(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(
        _owner(card, combat),
        PowerId.MACHINE_LEARNING,
        card.effect_vars.get(MACHINE_LEARNING_CARDS_KEY, MACHINE_LEARNING_CARDS),
    )


@register_effect(CardId.METEOR_STRIKE)
def meteor_strike(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    dmg = calculate_damage(card.base_damage, _owner(card, combat), target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    for _ in range(METEOR_STRIKE_PLASMA_ORBS):
        _channel_orb(combat, OrbType.PLASMA)


@register_effect(CardId.MODDED)
def modded(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat._draw_cards(card.effect_vars.get(MODDED_CARDS_KEY, MODDED_CARDS))
    _add_orb_slot(combat, card.effect_vars.get(MODDED_REPEAT_KEY, MODDED_REPEAT))
    card.cost += MODDED_COST_INCREASE


@register_effect(CardId.MULTI_CAST)
def multi_cast(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # X-cost: evoke front orb X times, removing the orb only on the last evoke.
    queue = getattr(combat, 'orb_queue', None)
    if queue is None or not queue.orbs:
        return
    evokes = getattr(card, "energy_spent", 0)
    if card.upgraded:
        evokes += 1
    for i in range(max(0, evokes)):
        if not queue.orbs:
            break
        if i == evokes - 1:
            _evoke_front(combat)
        else:
            queue.orbs[0].on_evoke(combat)


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
    combat.shuffle_rng.shuffle(combat.draw_pile)
    combat._draw_cards(card.effect_vars.get(REBOOT_CARDS_KEY, REBOOT_CARDS))


@register_effect(CardId.SHATTER)
def shatter(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    for enemy in combat.hittable_enemies:
        dmg = calculate_damage(card.base_damage, _owner(card, combat), enemy, ValueProp.MOVE, combat)
        apply_damage(enemy, dmg, ValueProp.MOVE, combat, _owner(card, combat))
    # Shatter.cs OnPlay (30-35): loops once per orb queued at cast time; each
    # iteration evokes the front orb twice — first with dequeue:false (evoke
    # effect and AfterOrbEvoked hooks fire, orb stays) then normally (orb removed).
    queue = getattr(combat, 'orb_queue', None)
    if queue is None:
        return
    for _ in range(len(queue.orbs)):
        if not queue.orbs:
            break
        queue._evoke_orb(queue.orbs[0], combat)
        queue.evoke_front(combat)


@register_effect(CardId.SIGNAL_BOOST)
def signal_boost(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.SIGNAL_BOOST, card.effect_vars.get(SIGNAL_BOOST_POWER_KEY, SIGNAL_BOOST_POWER))


@register_effect(CardId.SPINNER_CARD)
def spinner_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    if card.upgraded:
        _channel_orb(combat, OrbType.GLASS)
    combat.apply_power_to(_owner(card, combat), PowerId.SPINNER, card.effect_vars.get("spinner_power", 1))


@register_effect(CardId.SUPERCRITICAL)
def supercritical(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.gain_energy(_owner(card, combat), card.effect_vars.get(SUPERCRITICAL_ENERGY_KEY, SUPERCRITICAL_ENERGY))


@register_effect(CardId.TRASH_TO_TREASURE)
def trash_to_treasure(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.TRASH_TO_TREASURE, TRASH_TO_TREASURE_POWER)


@register_effect(CardId.VOLTAIC)
def voltaic(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    channel_count = sum(
        1
        for channel_owner, orb_type in getattr(combat, "_orb_channel_events_combat", ())
        if channel_owner is owner and orb_type == OrbType.LIGHTNING
    )
    for _ in range(channel_count):
        _channel_orb(combat, OrbType.LIGHTNING)


# ---------------------------------------------------------------------------
#  ANCIENT (2)
# ---------------------------------------------------------------------------

@register_effect(CardId.BIASED_COGNITION_CARD)
def biased_cognition(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(
        _owner(card, combat),
        PowerId.FOCUS,
        card.effect_vars.get(BIASED_COGNITION_FOCUS_KEY, BIASED_COGNITION_FOCUS),
    )
    combat.apply_power_to(
        _owner(card, combat),
        PowerId.BIASED_COGNITION,
        card.effect_vars.get(BIASED_COGNITION_POWER_KEY, BIASED_COGNITION_POWER),
    )


@register_effect(CardId.QUADCAST)
def quadcast(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    repeat = card.effect_vars.get("repeat", QUADCAST_REPEAT)
    for index in range(repeat):
        if index == repeat - 1:
            _evoke_front(combat)
        else:
            _passive_front(combat)


# ---------------------------------------------------------------------------
#  Card factories
# ---------------------------------------------------------------------------

def make_strike_defect(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.STRIKE_DEFECT, cost=STRIKE_DEFECT_COST, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.BASIC,
        base_damage=STRIKE_DEFECT_UPGRADED_DAMAGE if upgraded else STRIKE_DEFECT_DAMAGE,
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_defend_defect(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.DEFEND_DEFECT, cost=DEFEND_DEFECT_COST, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.BASIC,
        base_block=DEFEND_DEFECT_UPGRADED_BLOCK if upgraded else DEFEND_DEFECT_BLOCK,
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_zap(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.ZAP,
        cost=ZAP_UPGRADED_COST if upgraded else ZAP_COST,
        card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.BASIC,
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_dualcast(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.DUALCAST, cost=0 if upgraded else 1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.BASIC,
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_ball_lightning(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BALL_LIGHTNING, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=10 if upgraded else 7,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_barrage(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BARRAGE, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=7 if upgraded else 5,
        effect_vars={"calc_base": 0, "calc_extra": 1},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_beam_cell(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BEAM_CELL, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=4 if upgraded else 3,
        effect_vars={"vulnerable": 2 if upgraded else 1},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_boost_away(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BOOST_AWAY, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        base_block=BOOST_AWAY_UPGRADED_BLOCK if upgraded else BOOST_AWAY_BLOCK,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_charge_battery(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.CHARGE_BATTERY, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        base_block=CHARGE_BATTERY_UPGRADED_BLOCK if upgraded else CHARGE_BATTERY_BLOCK,
        effect_vars={CHARGE_BATTERY_ENERGY_KEY: CHARGE_BATTERY_ENERGY_NEXT_TURN},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_claw(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.CLAW, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=CLAW_UPGRADED_DAMAGE if upgraded else CLAW_DAMAGE,
        effect_vars={CLAW_INCREASE_KEY: CLAW_UPGRADED_INCREASE if upgraded else CLAW_INCREASE},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_cold_snap(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.COLD_SNAP, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=9 if upgraded else 6,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_compile_driver(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.COMPILE_DRIVER, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=10 if upgraded else 7,
        effect_vars={"calc_base": 0, "calc_extra": 1},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_coolheaded() -> CardInstance:
    return CardInstance(
        card_id=CardId.COOLHEADED, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        effect_vars={"cards": 1}, instance_id=_get_next_id(),
    )


def make_focused_strike(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.FOCUSED_STRIKE_CARD, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=FOCUSED_STRIKE_UPGRADED_DAMAGE if upgraded else FOCUSED_STRIKE_DAMAGE,
        effect_vars={
            FOCUSED_STRIKE_POWER_KEY: (
                FOCUSED_STRIKE_UPGRADED_POWER if upgraded else FOCUSED_STRIKE_POWER
            ),
        },
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_go_for_the_eyes(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.GO_FOR_THE_EYES, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=GO_FOR_THE_EYES_UPGRADED_DAMAGE if upgraded else GO_FOR_THE_EYES_DAMAGE,
        effect_vars={
            GO_FOR_THE_EYES_WEAK_KEY: (
                GO_FOR_THE_EYES_UPGRADED_WEAK if upgraded else GO_FOR_THE_EYES_WEAK
            ),
        },
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_gunk_up(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.GUNK_UP, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=GUNK_UP_UPGRADED_DAMAGE if upgraded else GUNK_UP_DAMAGE,
        effect_vars={GUNK_UP_REPEAT_KEY: GUNK_UP_REPEAT},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_hologram() -> CardInstance:
    return CardInstance(
        card_id=CardId.HOLOGRAM, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        base_block=3, keywords=frozenset({"exhaust"}), instance_id=_get_next_id(),
    )


def make_hotfix(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.HOTFIX, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        keywords=frozenset() if upgraded else frozenset({"exhaust"}),
        effect_vars={HOTFIX_FOCUS_KEY: HOTFIX_UPGRADED_FOCUS if upgraded else HOTFIX_FOCUS},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_leap(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.LEAP, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        base_block=LEAP_UPGRADED_BLOCK if upgraded else LEAP_BLOCK,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_lightning_rod(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.LIGHTNING_ROD, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        base_block=LIGHTNING_ROD_UPGRADED_BLOCK if upgraded else LIGHTNING_ROD_BLOCK,
        effect_vars={LIGHTNING_ROD_POWER_KEY: LIGHTNING_ROD_POWER},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_momentum_strike(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.MOMENTUM_STRIKE, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=MOMENTUM_STRIKE_UPGRADED_DAMAGE if upgraded else MOMENTUM_STRIKE_DAMAGE,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_sweeping_beam(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SWEEPING_BEAM, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ALL_ENEMIES, rarity=CardRarity.COMMON,
        base_damage=SWEEPING_BEAM_UPGRADED_DAMAGE if upgraded else SWEEPING_BEAM_DAMAGE,
        effect_vars={SWEEPING_BEAM_CARDS_KEY: SWEEPING_BEAM_CARDS},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_turbo(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.TURBO, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        effect_vars={TURBO_ENERGY_KEY: TURBO_UPGRADED_ENERGY if upgraded else TURBO_ENERGY},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_uproar(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.UPROAR, cost=2, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=UPROAR_UPGRADED_DAMAGE if upgraded else UPROAR_DAMAGE,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_boot_sequence(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BOOT_SEQUENCE, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        base_block=BOOT_SEQUENCE_UPGRADED_BLOCK if upgraded else BOOT_SEQUENCE_BLOCK,
        keywords=frozenset({"innate", "exhaust"}),
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_bulk_up(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BULK_UP, cost=2, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={
            BULK_UP_STRENGTH_KEY: BULK_UP_UPGRADED_POWER if upgraded else BULK_UP_POWER,
            BULK_UP_DEXTERITY_KEY: BULK_UP_UPGRADED_POWER if upgraded else BULK_UP_POWER,
            BULK_UP_ORB_SLOTS_KEY: BULK_UP_ORB_SLOTS,
        },
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_capacitor(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.CAPACITOR, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        upgraded=upgraded,
        effect_vars={"repeat": 3 if upgraded else 2}, instance_id=_get_next_id(),
    )


def make_chaos(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.CHAOS, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={CHAOS_REPEAT_KEY: CHAOS_UPGRADED_REPEAT if upgraded else CHAOS_REPEAT},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_chill(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.CHILL, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        keywords=frozenset() if upgraded else frozenset({"exhaust"}),
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_compact() -> CardInstance:
    return CardInstance(
        card_id=CardId.COMPACT, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        base_block=6, instance_id=_get_next_id(),
    )


def make_darkness(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.DARKNESS_CARD, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        upgraded=upgraded, effect_vars={"passives": 2 if upgraded else 1},
        instance_id=_get_next_id(),
    )


def make_double_energy(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.DOUBLE_ENERGY, cost=0 if upgraded else 1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        keywords=frozenset({"exhaust"}),
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_energy_surge(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.ENERGY_SURGE, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.ALL_ALLIES, rarity=CardRarity.UNCOMMON,
        keywords=frozenset({"exhaust"}),
        effect_vars={
            ENERGY_SURGE_ENERGY_KEY: (
                ENERGY_SURGE_UPGRADED_ENERGY if upgraded else ENERGY_SURGE_ENERGY
            ),
        },
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_feral(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.FERAL, cost=1 if upgraded else 2, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={FERAL_POWER_KEY: FERAL_POWER},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_fight_through(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.FIGHT_THROUGH, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        base_block=17 if upgraded else 13,
        effect_vars={"wounds": 2},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_ftl(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.FTL, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=FTL_UPGRADED_DAMAGE if upgraded else FTL_DAMAGE,
        effect_vars={
            FTL_CARDS_KEY: FTL_CARDS,
            FTL_PLAY_MAX_KEY: FTL_UPGRADED_PLAY_MAX if upgraded else FTL_PLAY_MAX,
        },
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_fusion(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.FUSION, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        keywords=frozenset() if upgraded else frozenset({"exhaust"}),
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_glacier(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.GLACIER, cost=2, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        base_block=GLACIER_UPGRADED_BLOCK if upgraded else GLACIER_BLOCK,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_glasswork(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.GLASSWORK, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        base_block=GLASSWORK_UPGRADED_BLOCK if upgraded else GLASSWORK_BLOCK,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_hailstorm(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.HAILSTORM, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={
            HAILSTORM_POWER_KEY: HAILSTORM_UPGRADED_POWER if upgraded else HAILSTORM_POWER
        },
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_iteration(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.ITERATION_CARD, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={"iteration_power": 3 if upgraded else 2},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_loop() -> CardInstance:
    return CardInstance(
        card_id=CardId.LOOP_CARD, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={"loop": 1}, instance_id=_get_next_id(),
    )


def make_null(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.NULL_CARD, cost=2, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=NULL_UPGRADED_DAMAGE if upgraded else NULL_DAMAGE,
        effect_vars={NULL_WEAK_KEY: NULL_UPGRADED_WEAK if upgraded else NULL_WEAK},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_overclock(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.OVERCLOCK, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={
            OVERCLOCK_CARDS_KEY: OVERCLOCK_UPGRADED_CARDS if upgraded else OVERCLOCK_CARDS
        },
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_refract(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.REFRACT, cost=3, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=12 if upgraded else 9,
        effect_vars={REFRACT_REPEAT_KEY: REFRACT_REPEAT},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_rocket_punch(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.ROCKET_PUNCH, cost=2, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=ROCKET_PUNCH_UPGRADED_DAMAGE if upgraded else ROCKET_PUNCH_DAMAGE,
        effect_vars={
            ROCKET_PUNCH_CARDS_KEY: (
                ROCKET_PUNCH_UPGRADED_CARDS if upgraded else ROCKET_PUNCH_CARDS
            )
        },
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_scavenge() -> CardInstance:
    return CardInstance(
        card_id=CardId.SCAVENGE, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={"energy": 2}, instance_id=_get_next_id(),
    )


def make_scrape(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SCRAPE, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=SCRAPE_UPGRADED_DAMAGE if upgraded else SCRAPE_DAMAGE,
        effect_vars={SCRAPE_CARDS_KEY: SCRAPE_UPGRADED_CARDS if upgraded else SCRAPE_CARDS},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_shadow_shield(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SHADOW_SHIELD, cost=2, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        base_block=SHADOW_SHIELD_UPGRADED_BLOCK if upgraded else SHADOW_SHIELD_BLOCK,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_skim(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SKIM, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={SKIM_CARDS_KEY: SKIM_UPGRADED_CARDS if upgraded else SKIM_CARDS},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_smokestack(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SMOKESTACK, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={
            SMOKESTACK_POWER_KEY: SMOKESTACK_UPGRADED_POWER if upgraded else SMOKESTACK_POWER
        },
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_storm(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.STORM_CARD, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={STORM_POWER_KEY: STORM_UPGRADED_POWER if upgraded else STORM_POWER},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_subroutine(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SUBROUTINE,
        cost=SUBROUTINE_UPGRADED_COST if upgraded else SUBROUTINE_COST,
        card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_sunder() -> CardInstance:
    return CardInstance(
        card_id=CardId.SUNDER, cost=3, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=24, effect_vars={"energy": 3}, instance_id=_get_next_id(),
    )


def make_synchronize(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SYNCHRONIZE, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        keywords=frozenset() if upgraded else frozenset({"exhaust"}),
        effect_vars={
            SYNCHRONIZE_CALC_BASE_KEY: SYNCHRONIZE_CALC_BASE,
            SYNCHRONIZE_CALC_EXTRA_KEY: SYNCHRONIZE_FOCUS_PER_ORB_TYPE,
        },
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_synthesis(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SYNTHESIS, cost=2, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=SYNTHESIS_UPGRADED_DAMAGE if upgraded else SYNTHESIS_DAMAGE,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_tempest(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.TEMPEST, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        has_energy_cost_x=True,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_tesla_coil(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.TESLA_COIL, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=TESLA_COIL_UPGRADED_DAMAGE if upgraded else TESLA_COIL_DAMAGE,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_thunder(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.THUNDER_CARD, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={
            THUNDER_POWER_KEY: THUNDER_UPGRADED_POWER if upgraded else THUNDER_POWER
        },
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_white_noise() -> CardInstance:
    return CardInstance(
        card_id=CardId.WHITE_NOISE, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        keywords=frozenset({"exhaust"}), instance_id=_get_next_id(),
    )


def make_adaptive_strike(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.ADAPTIVE_STRIKE, cost=2, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.RARE,
        base_damage=ADAPTIVE_STRIKE_UPGRADED_DAMAGE if upgraded else ADAPTIVE_STRIKE_DAMAGE,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_all_for_one(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.ALL_FOR_ONE, cost=2, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.RARE,
        base_damage=ALL_FOR_ONE_UPGRADED_DAMAGE if upgraded else ALL_FOR_ONE_DAMAGE,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_buffer(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BUFFER_CARD, cost=2, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        effect_vars={"buffer_power": 2 if upgraded else 1},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_consuming_shadow(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.CONSUMING_SHADOW, cost=2, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        upgraded=upgraded,
        effect_vars={"repeat": 3 if upgraded else 2, "consuming_shadow_power": 1},
        instance_id=_get_next_id(),
    )


def make_coolant(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.COOLANT, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        effect_vars={COOLANT_POWER_KEY: COOLANT_UPGRADED_POWER if upgraded else COOLANT_POWER},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_creative_ai(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.CREATIVE_AI_CARD,
        cost=CREATIVE_AI_UPGRADED_COST if upgraded else CREATIVE_AI_COST,
        card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        effect_vars={CREATIVE_AI_POWER_KEY: CREATIVE_AI_POWER},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_defragment(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.DEFRAGMENT, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        effect_vars={
            DEFRAGMENT_FOCUS_KEY: DEFRAGMENT_UPGRADED_FOCUS if upgraded else DEFRAGMENT_FOCUS,
        },
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_echo_form(upgraded: bool = False) -> CardInstance:
    keywords = frozenset() if upgraded else frozenset({"ethereal"})
    return CardInstance(
        card_id=CardId.ECHO_FORM_CARD, cost=3, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        keywords=keywords,
        effect_vars={"echo_form": 1}, upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_flak_cannon() -> CardInstance:
    return CardInstance(
        card_id=CardId.FLAK_CANNON, cost=2, card_type=CardType.ATTACK,
        target_type=TargetType.RANDOM_ENEMY, rarity=CardRarity.RARE,
        base_damage=8, effect_vars={"calc_base": 0, "calc_extra": 1},
        instance_id=_get_next_id(),
    )


def make_genetic_algorithm(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.GENETIC_ALGORITHM, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        base_block=GENETIC_ALGORITHM_BLOCK, keywords=frozenset({"exhaust"}),
        effect_vars={
            GENETIC_ALGORITHM_BLOCK_KEY: GENETIC_ALGORITHM_BLOCK,
            GENETIC_ALGORITHM_INCREASE_KEY: (
                GENETIC_ALGORITHM_UPGRADED_INCREASE if upgraded else GENETIC_ALGORITHM_INCREASE
            ),
        },
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_helix_drill(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.HELIX_DRILL, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.RARE,
        base_damage=HELIX_DRILL_UPGRADED_DAMAGE if upgraded else HELIX_DRILL_DAMAGE,
        effect_vars={
            HELIX_DRILL_CALC_BASE_KEY: HELIX_DRILL_CALC_BASE,
            HELIX_DRILL_CALC_EXTRA_KEY: HELIX_DRILL_CALC_EXTRA,
        },
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_hyperbeam(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.HYPERBEAM, cost=2, card_type=CardType.ATTACK,
        target_type=TargetType.ALL_ENEMIES, rarity=CardRarity.RARE,
        base_damage=HYPERBEAM_UPGRADED_DAMAGE if upgraded else HYPERBEAM_DAMAGE,
        effect_vars={HYPERBEAM_FOCUS_KEY: HYPERBEAM_FOCUS},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_ice_lance(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.ICE_LANCE, cost=3, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.RARE,
        base_damage=24 if upgraded else 19,
        effect_vars={ICE_LANCE_REPEAT_KEY: ICE_LANCE_REPEAT},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_ignition(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.IGNITION, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.ANY_ALLY, rarity=CardRarity.RARE,
        keywords=frozenset() if upgraded else frozenset({"exhaust"}),
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_machine_learning(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.MACHINE_LEARNING_CARD, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        effect_vars={MACHINE_LEARNING_CARDS_KEY: MACHINE_LEARNING_CARDS},
        keywords=frozenset({"innate"}) if upgraded else frozenset(),
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_meteor_strike(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.METEOR_STRIKE, cost=5, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.RARE,
        base_damage=METEOR_STRIKE_UPGRADED_DAMAGE if upgraded else METEOR_STRIKE_DAMAGE,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_modded(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.MODDED, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        effect_vars={
            MODDED_CARDS_KEY: MODDED_UPGRADED_CARDS if upgraded else MODDED_CARDS,
            MODDED_REPEAT_KEY: MODDED_REPEAT,
        },
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_multi_cast(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.MULTI_CAST, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        has_energy_cost_x=True,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_rainbow(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.RAINBOW, cost=2, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        keywords=frozenset() if upgraded else frozenset({"exhaust"}),
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_reboot(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.REBOOT, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        keywords=frozenset({"exhaust"}),
        effect_vars={REBOOT_CARDS_KEY: REBOOT_UPGRADED_CARDS if upgraded else REBOOT_CARDS},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_shatter(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SHATTER, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ALL_ENEMIES, rarity=CardRarity.RARE,
        base_damage=SHATTER_UPGRADED_DAMAGE if upgraded else SHATTER_DAMAGE,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_signal_boost(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SIGNAL_BOOST,
        cost=SIGNAL_BOOST_UPGRADED_COST if upgraded else SIGNAL_BOOST_COST,
        card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        keywords=frozenset({"exhaust"}),
        effect_vars={SIGNAL_BOOST_POWER_KEY: SIGNAL_BOOST_POWER},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_spinner(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SPINNER_CARD, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        effect_vars={"spinner_power": 1}, upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_supercritical(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SUPERCRITICAL, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        keywords=frozenset({"exhaust"}),
        effect_vars={
            SUPERCRITICAL_ENERGY_KEY: (
                SUPERCRITICAL_UPGRADED_ENERGY if upgraded else SUPERCRITICAL_ENERGY
            )
        },
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_trash_to_treasure(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.TRASH_TO_TREASURE, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        keywords=frozenset({"innate"}) if upgraded else frozenset(),
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_voltaic(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.VOLTAIC, cost=3, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        keywords=frozenset() if upgraded else frozenset({"exhaust"}),
        effect_vars={
            VOLTAIC_CALC_BASE_KEY: VOLTAIC_CALC_BASE,
            VOLTAIC_CALC_EXTRA_KEY: VOLTAIC_CALC_EXTRA,
        },
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_biased_cognition(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BIASED_COGNITION_CARD, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.ANCIENT,
        effect_vars={
            BIASED_COGNITION_FOCUS_KEY: (
                BIASED_COGNITION_UPGRADED_FOCUS if upgraded else BIASED_COGNITION_FOCUS
            ),
            BIASED_COGNITION_POWER_KEY: BIASED_COGNITION_POWER,
        },
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_quadcast(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.QUADCAST,
        cost=QUADCAST_UPGRADED_COST if upgraded else QUADCAST_COST,
        card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.ANCIENT,
        effect_vars={"repeat": QUADCAST_REPEAT},
        upgraded=upgraded,
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
