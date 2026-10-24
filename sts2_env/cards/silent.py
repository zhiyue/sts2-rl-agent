"""Silent card effects and factories — all 88 cards."""

from __future__ import annotations

from sts2_env.cards.base import CardInstance, _get_next_id
from sts2_env.cards.enchantments import apply_static_enchantment
from sts2_env.cards.registry import (
    register_after_card_entered_combat_hook,
    register_after_card_played_hook,
    register_effect,
    register_playability_hook,
)
from sts2_env.core.enums import (
    CardId, CardTag, CardType, TargetType, CardRarity, ValueProp, PowerId, RoomType,
)
from sts2_env.core.damage import calculate_damage, apply_damage, calculate_block
from sts2_env.core.hooks import fire_after_block_gained
from sts2_env.core.creature import Creature
from sts2_env.core.combat import CombatState


# LeadingStrike.cs: new CardsVar("Shivs", 2) — base and upgraded both create 2 Shivs.
LEADING_STRIKE_SHIVS = 2
OUTBREAK_REPEAT = 3


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
    _gain_resolved_block(owner, blk, combat)


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
    _gain_resolved_block(owner, blk, combat)
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
    if not combat.hand:
        return
    combat.request_card_choice(
        prompt="Choose a hand card to discard",
        cards=list(combat.hand),
        source_pile="hand",
        resolver=lambda selected: combat.discard_cards([selected] if selected is not None else []),
    )


@register_effect(CardId.ANTICIPATE)
def anticipate(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Anticipate.cs: PowerCmd.Apply<AnticipatePower>(DynamicVars.Dexterity.BaseValue)
    amt = card.effect_vars.get("dexterity", 2)
    combat.apply_power_to(_owner(card, combat), PowerId.ANTICIPATE, amt)


@register_effect(CardId.BACKFLIP)
def backflip(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    blk = calculate_block(card.base_block, owner, ValueProp.MOVE, combat, card_source=card)
    _gain_resolved_block(owner, blk, combat)
    combat._draw_cards(card.effect_vars.get("cards", 2))


@register_effect(CardId.BLADE_DANCE)
def blade_dance(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    count = card.effect_vars.get("cards", 3)
    for _ in range(count):
        combat.add_generated_card_to_creature_hand(owner, _make_shiv())


@register_effect(CardId.CLOAK_AND_DAGGER)
def cloak_and_dagger(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    blk = calculate_block(card.base_block, owner, ValueProp.MOVE, combat, card_source=card)
    _gain_resolved_block(owner, blk, combat)
    count = card.effect_vars.get("cards", 1)
    for _ in range(count):
        combat.add_generated_card_to_creature_hand(owner, _make_shiv())


@register_effect(CardId.DAGGER_SPRAY)
def dagger_spray(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    for _ in range(2):
        if owner.is_dead:
            break
        for enemy in combat.hittable_enemies:
            dmg = calculate_damage(card.base_damage, owner, enemy, ValueProp.MOVE, combat)
            apply_damage(enemy, dmg, ValueProp.MOVE, combat, owner)


@register_effect(CardId.DAGGER_THROW)
def dagger_throw(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    dmg = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, owner)
    combat._draw_cards(1)
    if not combat.hand:
        return
    combat.request_card_choice(
        prompt="Choose a hand card to discard",
        cards=list(combat.hand),
        source_pile="hand",
        resolver=lambda selected: combat.discard_cards([selected] if selected is not None else []),
    )


@register_effect(CardId.DEADLY_POISON)
def deadly_poison(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    combat.apply_power_to(target, PowerId.POISON, card.effect_vars.get("poison_power", 5))


@register_effect(CardId.DEFLECT)
def deflect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    blk = calculate_block(card.base_block, owner, ValueProp.MOVE, combat, card_source=card)
    _gain_resolved_block(owner, blk, combat)


@register_effect(CardId.DODGE_AND_ROLL)
def dodge_and_roll(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    blk = calculate_block(card.base_block, owner, ValueProp.MOVE, combat, card_source=card)
    gained = _gain_resolved_block(owner, blk, combat)
    combat.apply_power_to(owner, PowerId.BLOCK_NEXT_TURN, gained)


@register_effect(CardId.FLICK_FLACK)
def flick_flack(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    for enemy in combat.hittable_enemies:
        dmg = calculate_damage(card.base_damage, owner, enemy, ValueProp.MOVE, combat)
        apply_damage(enemy, dmg, ValueProp.MOVE, combat, owner)


@register_effect(CardId.LEADING_STRIKE)
def leading_strike(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    dmg = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, owner)
    for _ in range(card.effect_vars.get("shivs", LEADING_STRIKE_SHIVS)):
        combat.add_generated_card_to_creature_hand(owner, _make_shiv())


@register_effect(CardId.PIERCING_WAIL)
def piercing_wail(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    loss = card.effect_vars.get("strength_loss", 6)
    for enemy in combat.hittable_enemies:
        combat.apply_power_to(enemy, PowerId.PIERCING_WAIL, loss)


@register_effect(CardId.POISONED_STAB)
def poisoned_stab(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    dmg = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, owner)
    combat.apply_power_to(target, PowerId.POISON, card.effect_vars.get("poison_power", 3))


@register_effect(CardId.PREPARED)
def prepared(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    draw = card.effect_vars.get("cards", 1)
    combat._draw_cards(draw)
    discard_count = min(draw, len(combat.hand))
    if discard_count <= 0:
        return
    if discard_count == 1:
        combat.request_card_choice(
            prompt="Choose a hand card to discard",
            cards=list(combat.hand),
            source_pile="hand",
            resolver=lambda selected: combat.discard_cards([selected] if selected is not None else []),
        )
        return
    combat.request_multi_card_choice(
        prompt="Choose hand cards to discard",
        cards=list(combat.hand),
        source_pile="hand",
        resolver=lambda selected_cards: combat.discard_cards(selected_cards),
        min_count=discard_count,
        max_count=discard_count,
    )


@register_effect(CardId.RICOCHET)
def ricochet(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    repeat = card.effect_vars.get("repeat", 4)
    for _ in range(repeat):
        if owner.is_dead:
            break
        alive = combat.hittable_enemies
        if not alive:
            break
        t = combat.combat_targets_rng.choice(alive)
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
    combat.apply_power_to(target, PowerId.POISON, card.effect_vars.get("poison_power", 7))


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
    _gain_resolved_block(owner, blk, combat)


# ---------------------------------------------------------------------------
#  UNCOMMON (27)
# ---------------------------------------------------------------------------

@register_effect(CardId.ACCURACY_CARD)
def accuracy(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.ACCURACY, card.effect_vars.get("accuracy_power", 4))


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
    _gain_resolved_block(owner, blk, combat)
    combat.apply_power_to(owner, PowerId.BLUR, card.effect_vars.get("blur", 1))


@register_effect(CardId.BOUNCING_FLASK)
def bouncing_flask(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    repeat = card.effect_vars.get("repeat", 3)
    poison_amt = card.effect_vars.get("poison_power", 3)
    for _ in range(repeat):
        alive = combat.hittable_enemies
        if not alive:
            break
        t = combat.combat_targets_rng.choice(alive)
        combat.apply_power_to(t, PowerId.POISON, poison_amt)


@register_effect(CardId.BUBBLE_BUBBLE)
def bubble_bubble(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    if target.get_power_amount(PowerId.POISON) > 0:
        combat.apply_power_to(target, PowerId.POISON, card.effect_vars.get("poison_power", 9))


@register_effect(CardId.CALCULATED_GAMBLE)
def calculated_gamble(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    cards_to_discard = list(combat.hand)
    combat.discard_cards(cards_to_discard, draw_count=len(cards_to_discard))


@register_effect(CardId.DASH)
def dash(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    blk = calculate_block(card.base_block, owner, ValueProp.MOVE, combat, card_source=card)
    _gain_resolved_block(owner, blk, combat)
    dmg = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, owner)


@register_effect(CardId.ESCAPE_PLAN)
def escape_plan(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    pre_draw_hand_size = len(combat.hand)
    combat._draw_cards(1)
    if len(combat.hand) <= pre_draw_hand_size:
        return
    drawn_card = combat.hand[pre_draw_hand_size]
    if not drawn_card.is_skill:
        return
    owner = _owner(card, combat)
    blk = calculate_block(card.base_block, owner, ValueProp.MOVE, combat, card_source=card)
    _gain_resolved_block(owner, blk, combat)


@register_effect(CardId.EXPERTISE)
def expertise(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    draw = card.effect_vars.get("cards", 6)
    draw_up_to = draw - len(combat.hand)
    if draw_up_to > 0:
        combat._draw_cards(draw_up_to)


@register_effect(CardId.EXPOSE)
def expose(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    amt = card.effect_vars.get("power", 2)
    target.block = 0
    target.powers.pop(PowerId.ARTIFACT, None)
    combat.apply_power_to(target, PowerId.VULNERABLE, amt)


@register_effect(CardId.FINISHER)
def finisher(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    hits = combat.count_card_plays_finished_this_turn(owner, card_type=CardType.ATTACK)
    for _ in range(hits):
        if owner.is_dead or target.is_dead:
            break
        dmg = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
        apply_damage(target, dmg, ValueProp.MOVE, combat, owner)


@register_effect(CardId.FLANKING)
def flanking(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    combat.apply_power_to(target, PowerId.FLANKING, card.effect_vars.get("flanking_power", 2), applier=owner)
    power = target.powers.get(PowerId.FLANKING)
    if power is not None:
        power.applier = owner


@register_effect(CardId.FLECHETTES)
def flechettes(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    # Deals damage for each skill in hand
    owner = _owner(card, combat)
    skill_count = sum(1 for c in combat.hand if c.is_skill)
    hits = max(0, skill_count)
    for _ in range(hits):
        if owner.is_dead or target.is_dead:
            break
        dmg = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
        apply_damage(target, dmg, ValueProp.MOVE, combat, owner)


@register_effect(CardId.FOLLOW_THROUGH)
def follow_through(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    for enemy in combat.hittable_enemies:
        dmg = calculate_damage(card.base_damage, owner, enemy, ValueProp.MOVE, combat)
        apply_damage(enemy, dmg, ValueProp.MOVE, combat, owner)
    previous = combat.last_card_play_started_this_turn(owner, exclude_card=card)
    if previous is None or previous.card_type != CardType.SKILL:
        return
    weak = card.effect_vars.get("weak", 1)
    for enemy in combat.hittable_enemies:
        combat.apply_power_to(enemy, PowerId.WEAK, weak)


@register_effect(CardId.FOOTWORK)
def footwork(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.DEXTERITY, card.effect_vars.get("dexterity", 2))


@register_effect(CardId.HAND_TRICK)
def hand_trick(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    blk = calculate_block(card.base_block, owner, ValueProp.MOVE, combat, card_source=card)
    _gain_resolved_block(owner, blk, combat)
    candidates = [c for c in combat.hand if c.card_type == CardType.SKILL and not c.is_sly]
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
    poison = card.effect_vars.get("poison_power", 4)
    for enemy in combat.hittable_enemies:
        combat.apply_power_to(enemy, PowerId.POISON, poison)


@register_effect(CardId.HIDDEN_DAGGERS)
def hidden_daggers(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    discard_count = min(card.effect_vars.get("cards", 2), len(combat.hand))

    def _finish() -> None:
        for _ in range(card.effect_vars.get("shivs", 2)):
            combat.add_generated_card_to_creature_hand(owner, _make_shiv(upgraded=card.upgraded))

    if discard_count <= 0:
        _finish()
        return
    if discard_count == 1:
        combat.request_card_choice(
            prompt="Choose a hand card to discard",
            cards=list(combat.hand),
            source_pile="hand",
            resolver=lambda selected: (
                combat.discard_cards([selected] if selected is not None else []),
                _finish(),
            ),
        )
        return
    combat.request_multi_card_choice(
        prompt="Choose hand cards to discard",
        cards=list(combat.hand),
        source_pile="hand",
        resolver=lambda selected_cards: (
            combat.discard_cards(selected_cards),
            _finish(),
        ),
        min_count=discard_count,
        max_count=discard_count,
    )


@register_effect(CardId.INFINITE_BLADES_CARD)
def infinite_blades(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Power: gain 1 Shiv at start of each turn
    combat.apply_power_to(_owner(card, combat), PowerId.INFINITE_BLADES, 1)


@register_effect(CardId.LEG_SWEEP)
def leg_sweep(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    blk = calculate_block(card.base_block, owner, ValueProp.MOVE, combat, card_source=card)
    _gain_resolved_block(owner, blk, combat)
    combat.apply_power_to(target, PowerId.WEAK, card.effect_vars.get("weak", 2))


@register_effect(CardId.MEMENTO_MORI)
def memento_mori(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    base = card.effect_vars.get("calc_base", 8)
    extra = card.effect_vars.get("extra_damage", 4)
    discard_count = combat.count_cards_discarded_this_turn(owner)
    total_dmg = base + extra * discard_count
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
    _gain_resolved_block(owner, blk, combat)


@register_effect(CardId.NOXIOUS_FUMES_CARD)
def noxious_fumes(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.NOXIOUS_FUMES, card.effect_vars.get("poison_per_turn", 2))


@register_effect(CardId.OUTBREAK_CARD)
def outbreak(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    combat.apply_power_to(owner, PowerId.OUTBREAK, card.effect_vars.get("outbreak_power", 11))
    outbreak_power = owner.powers.get(PowerId.OUTBREAK)
    if outbreak_power is not None:
        outbreak_power.repeat = card.effect_vars.get("repeat", OUTBREAK_REPEAT)


@register_effect(CardId.PHANTOM_BLADES_CARD)
def phantom_blades(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    combat.apply_power_to(owner, PowerId.PHANTOM_BLADES, card.effect_vars.get("phantom_blades_power", 9))
    state = combat.combat_player_state_for(owner)
    if state is None:
        return
    for pile in state.all_piles:
        for existing in pile:
            if existing.is_shiv:
                existing.keywords = frozenset(set(existing.keywords) | {"retain"})


@register_effect(CardId.PINPOINT)
def pinpoint(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    dmg = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, owner)


@register_after_card_entered_combat_hook(CardId.PINPOINT)
def pinpoint_after_card_entered_combat(
    card: CardInstance,
    entered_card: CardInstance,
    owner: Creature,
    combat: CombatState,
) -> None:
    if entered_card is not card or card.combat_vars.get("_is_clone"):
        return
    amount = combat.count_card_plays_finished_this_turn(owner, card_type=CardType.SKILL)
    if amount > 0:
        card.set_temporary_cost_for_turn(max(0, card.cost - amount))


@register_after_card_played_hook(CardId.PINPOINT)
def pinpoint_after_card_played(
    card: CardInstance,
    played_card: CardInstance,
    owner: Creature,
    combat: CombatState,
) -> None:
    if played_card.card_type == CardType.SKILL:
        card.set_temporary_cost_for_turn(max(0, card.cost - 1))


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
    owner = _owner(card, combat)
    base = card.effect_vars.get("calc_base", 13)
    extra = card.effect_vars.get("extra_damage", 2)
    total_dmg = base - extra * len(combat.hand)
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
    owner = _owner(card, combat)
    hits = getattr(card, "energy_spent", 0)
    for _ in range(max(0, hits)):
        if owner.is_dead or target.is_dead:
            break
        dmg = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
        apply_damage(target, dmg, ValueProp.MOVE, combat, owner)


@register_effect(CardId.SPEEDSTER_CARD)
def speedster(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.SPEEDSTER, card.effect_vars.get("speedster_power", 2))


@register_effect(CardId.STRANGLE)
def strangle(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Strangle.cs OnPlay: DamageCmd.Attack(...) then
    # PowerCmd.Apply<StranglePower>(target, StranglePower base value, applier=owner).
    # StranglePower (powers/remaining_c.py) = enemy loses Amount HP (unblockable +
    # unpowered) for each card its owner plays that turn; removed at end of turn.
    assert target is not None
    owner = _owner(card, combat)
    dmg = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, owner)
    combat.apply_power_to(target, PowerId.STRANGLE, card.effect_vars.get("strangle_power", 2))


@register_effect(CardId.TACTICIAN)
def tactician(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.gain_energy(_owner(card, combat), card.effect_vars.get("energy", 1))


@register_effect(CardId.UP_MY_SLEEVE)
def up_my_sleeve(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    count = card.effect_vars.get("cards", 3)
    for _ in range(count):
        combat.add_generated_card_to_creature_hand(owner, _make_shiv())
    card.set_combat_cost(max(0, card.cost - 1))


@register_effect(CardId.WELL_LAID_PLANS)
def well_laid_plans(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.WELL_LAID_PLANS, card.effect_vars.get("retain_amount", 1))


# ---------------------------------------------------------------------------
#  RARE (26)
# ---------------------------------------------------------------------------

@register_effect(CardId.ABRASIVE)
def abrasive(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    combat.apply_power_to(owner, PowerId.DEXTERITY, card.effect_vars.get("dexterity", 1))
    combat.apply_power_to(owner, PowerId.THORNS, card.effect_vars.get("thorns_power", 4))


@register_effect(CardId.ACCELERANT)
def accelerant(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.ACCELERANT, card.effect_vars.get("accelerant", 1))


@register_effect(CardId.ADRENALINE)
def adrenaline(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    combat.gain_energy(owner, card.effect_vars.get("energy", 1))
    combat._draw_cards(card.effect_vars.get("cards", 2))


@register_effect(CardId.AFTERIMAGE_CARD)
def afterimage(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.AFTERIMAGE, card.effect_vars.get("afterimage_power", 1))


@register_effect(CardId.ASSASSINATE)
def assassinate(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    dmg = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, dmg, ValueProp.MOVE, combat, owner)
    combat.apply_power_to(target, PowerId.VULNERABLE, card.effect_vars.get("vulnerable", 1))


@register_effect(CardId.BLADE_OF_INK)
def blade_of_ink(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # BladeOfInk.cs OnPlay: Shiv.CreateInHand(owner, DynamicVars.Cards.IntValue),
    # then CardCmd.Enchant<Inky>(item, 1m) on every created Shiv.
    owner = _owner(card, combat)
    count = card.effect_vars.get("cards", 2)
    for _ in range(count):
        shiv = _make_shiv()
        apply_static_enchantment(shiv, "Inky", 1)
        combat.add_generated_card_to_creature_hand(owner, shiv)


@register_effect(CardId.BULLET_TIME)
def bullet_time(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # NoDraw + all cards in hand cost 0 this turn
    combat.apply_power_to(_owner(card, combat), PowerId.NO_DRAW, 1)
    for c in combat.hand:
        if not c.has_energy_cost_x:
            c.set_temporary_free_this_turn()


@register_effect(CardId.BURST)
def burst(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.BURST, card.effect_vars.get("skills", 1))


@register_effect(CardId.CORROSIVE_WAVE)
def corrosive_wave(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # CorrosiveWave.cs: PowerCmd.Apply<CorrosiveWavePower>(DynamicVars["CorrosiveWave"].BaseValue)
    combat.apply_power_to(_owner(card, combat), PowerId.CORROSIVE_WAVE, card.effect_vars.get("corrosive_wave", 2))


@register_effect(CardId.ECHOING_SLASH)
def echoing_slash(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    attacks = 1
    while attacks > 0:
        if owner.is_dead:
            break
        attacks -= 1
        killed = 0
        for enemy in list(combat.hittable_enemies):
            dmg = calculate_damage(card.base_damage, owner, enemy, ValueProp.MOVE, combat)
            apply_damage(enemy, dmg, ValueProp.MOVE, combat, owner)
            if enemy.is_dead:
                killed += 1
        attacks += killed


@register_effect(CardId.ENVENOM_CARD)
def envenom(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.ENVENOM, card.effect_vars.get("envenom_power", 1))


@register_effect(CardId.FAN_OF_KNIVES_CARD)
def fan_of_knives(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    combat.apply_power_to(owner, PowerId.FAN_OF_KNIVES, 1)
    for _ in range(card.effect_vars.get("shivs", 2)):
        combat.add_generated_card_to_creature_hand(owner, _make_shiv())


@register_effect(CardId.GRAND_FINALE)
def grand_finale(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    for enemy in combat.hittable_enemies:
        dmg = calculate_damage(card.base_damage, owner, enemy, ValueProp.MOVE, combat)
        apply_damage(enemy, dmg, ValueProp.MOVE, combat, owner)


@register_playability_hook(CardId.GRAND_FINALE)
def grand_finale_is_playable(card: CardInstance, owner_state, combat: CombatState, owner: Creature) -> bool:
    return not owner_state.draw


@register_effect(CardId.KNIFE_TRAP)
def knife_trap(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # KnifeTrap.cs OnPlay: for every Shiv in the exhaust pile, first
    # CardCmd.Upgrade(item) when KnifeTrap is upgraded, then
    # CardCmd.AutoPlay(...) — a full play that triggers AfterCardPlayed hooks.
    owner = _owner(card, combat)
    for shiv in [c for c in combat.exhaust_pile if c.is_shiv]:
        if card.upgraded:
            combat.upgrade_card(shiv)
        if combat.is_over:
            break
        if target is not None and not target.is_alive:
            break
        if getattr(owner, "is_dead", False):
            break
        combat.auto_play_card(shiv, target=target)


@register_effect(CardId.MALAISE)
def malaise(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    x = getattr(card, "energy_spent", 0)
    if card.upgraded:
        x += 1
    combat.apply_power_to(target, PowerId.STRENGTH, -x)
    combat.apply_power_to(target, PowerId.WEAK, x)


@register_effect(CardId.MASTER_PLANNER)
def master_planner(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.MASTER_PLANNER, 1)


@register_effect(CardId.MURDER)
def murder(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Murder.cs CanonicalVars: CalculationBaseVar(1) + ExtraDamageVar(1) with
    # CalculatedDamageVar multiplier = number of CardDrawnEntry entries whose
    # Actor is the owner this combat. CalculatedVar.Calculate = base + extra *
    # multiplier, i.e. 1 + 1 * cards drawn (not a product).
    assert target is not None
    owner = _owner(card, combat)
    base = card.effect_vars.get("calc_base", 1)
    extra = card.effect_vars.get("extra_damage", 1)
    total_dmg = base + extra * combat.count_cards_drawn_this_combat(owner)
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
        before_amount = owner.get_power_amount(PowerId.NIGHTMARE)
        combat.apply_power_to(owner, PowerId.NIGHTMARE, 3)
        power = owner.powers.get(PowerId.NIGHTMARE)
        if power is not None and hasattr(power, "set_selected_card"):
            power.amount = max(0, power.amount - before_amount)
            power.set_selected_card(selected)

    combat.request_card_choice(
        prompt="Choose a card for Nightmare",
        cards=list(combat.hand),
        source_pile="hand",
        resolver=_resolver,
    )


@register_effect(CardId.SERPENT_FORM_CARD)
def serpent_form(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.SERPENT_FORM, card.effect_vars.get("serpent_form_power", 4))


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
    combat.apply_power_to(_owner(card, combat), PowerId.SNEAKY, card.effect_vars.get("sneaky_power", 1))


@register_effect(CardId.STORM_OF_STEEL)
def storm_of_steel(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    cards = list(combat.hand)
    count = len(cards)
    combat.discard_cards(cards, 0)
    for _ in range(count):
        combat.add_generated_card_to_creature_hand(owner, _make_shiv(upgraded=card.upgraded))


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
    owner = _owner(card, combat)
    amount = 1 if owner.get_power_amount(PowerId.TRACKING) > 0 else 2
    combat.apply_power_to(owner, PowerId.TRACKING, amount)


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
    combat.apply_power_to(owner, PowerId.INTANGIBLE, card.effect_vars.get("intangible_power", 2))
    combat.apply_power_to(owner, PowerId.WRAITH_FORM, card.effect_vars.get("wraith_form_power", 1))


# ---------------------------------------------------------------------------
#  Shiv helper
# ---------------------------------------------------------------------------

def _make_shiv(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SHIV,
        cost=0,
        card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.COMMON,
        base_damage=6 if upgraded else 4,
        upgraded=upgraded,
        tags=frozenset({CardTag.SHIV}),
        keywords=frozenset({"exhaust"}),
        instance_id=_get_next_id(),
    )


# ---------------------------------------------------------------------------
#  Card factories
# ---------------------------------------------------------------------------

def make_strike_silent(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.STRIKE_SILENT, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.BASIC,
        base_damage=9 if upgraded else 6, upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_defend_silent(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.DEFEND_SILENT, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.BASIC,
        base_block=8 if upgraded else 5, upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_neutralize(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.NEUTRALIZE, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.BASIC,
        base_damage=4 if upgraded else 3,
        effect_vars={"weak": 2 if upgraded else 1},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_survivor(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SURVIVOR, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.BASIC,
        base_block=11 if upgraded else 8, upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_acrobatics(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.ACROBATICS, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={"cards": 4 if upgraded else 3},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_anticipate(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.ANTICIPATE, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        # Anticipate.cs: new PowerVar<DexterityPower>(2m); OnUpgrade: Dexterity.UpgradeValueBy(1m)
        effect_vars={"dexterity": 3 if upgraded else 2},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_backflip(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BACKFLIP, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        base_block=8 if upgraded else 5, effect_vars={"cards": 2},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_blade_dance(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BLADE_DANCE, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        keywords=frozenset({"exhaust"}),
        effect_vars={"cards": 4 if upgraded else 3},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_cloak_and_dagger(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.CLOAK_AND_DAGGER, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        base_block=6, effect_vars={"cards": 2 if upgraded else 1},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_dagger_spray(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.DAGGER_SPRAY, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ALL_ENEMIES, rarity=CardRarity.COMMON,
        base_damage=6 if upgraded else 4, upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_dagger_throw(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.DAGGER_THROW, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=12 if upgraded else 9,
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_deadly_poison(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.DEADLY_POISON, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        effect_vars={"poison_power": 7 if upgraded else 5},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_deflect(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.DEFLECT, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        base_block=7 if upgraded else 4, upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_dodge_and_roll(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.DODGE_AND_ROLL, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        base_block=6 if upgraded else 4,
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_flick_flack(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.FLICK_FLACK, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ALL_ENEMIES, rarity=CardRarity.COMMON,
        # FlickFlack.cs: new DamageVar(6m, ValueProp.Move); OnUpgrade: Damage.UpgradeValueBy(2m)
        base_damage=8 if upgraded else 6, keywords=frozenset({"sly"}),
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_leading_strike(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.LEADING_STRIKE, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        # LeadingStrike.cs: new CardsVar("Shivs", 2), new DamageVar(3m);
        # OnUpgrade: Damage.UpgradeValueBy(3m) (Shivs unchanged).
        base_damage=6 if upgraded else 3,
        effect_vars={"shivs": LEADING_STRIKE_SHIVS},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_piercing_wail(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.PIERCING_WAIL, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.ALL_ENEMIES, rarity=CardRarity.COMMON,
        keywords=frozenset({"exhaust"}),
        effect_vars={"strength_loss": 8 if upgraded else 6},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_poisoned_stab(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.POISONED_STAB, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=8 if upgraded else 6,
        effect_vars={"poison_power": 4 if upgraded else 3},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_prepared(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.PREPARED, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        effect_vars={"cards": 2 if upgraded else 1},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_ricochet(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.RICOCHET, cost=2, card_type=CardType.ATTACK,
        target_type=TargetType.RANDOM_ENEMY, rarity=CardRarity.COMMON,
        base_damage=3, keywords=frozenset({"sly"}),
        effect_vars={"repeat": 5 if upgraded else 4},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_slice(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SLICE, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=9 if upgraded else 6, upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_snakebite(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SNAKEBITE, cost=2, card_type=CardType.SKILL,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        keywords=frozenset({"retain"}),
        effect_vars={"poison_power": 10 if upgraded else 7},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_sucker_punch(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SUCKER_PUNCH, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=10 if upgraded else 8,
        effect_vars={"weak": 2 if upgraded else 1},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_untouchable(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.UNTOUCHABLE, cost=2, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.COMMON,
        # Untouchable.cs: new BlockVar(6m, ValueProp.Move); OnUpgrade: Block.UpgradeValueBy(3m)
        base_block=9 if upgraded else 6, keywords=frozenset({"sly"}),
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_accuracy(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.ACCURACY_CARD, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={"accuracy_power": 6 if upgraded else 4},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_backstab(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BACKSTAB, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=15 if upgraded else 11, keywords=frozenset({"exhaust", "innate"}),
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_blur(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BLUR_CARD, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        base_block=8 if upgraded else 5, effect_vars={"blur": 1},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_bouncing_flask(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BOUNCING_FLASK, cost=2, card_type=CardType.SKILL,
        target_type=TargetType.RANDOM_ENEMY, rarity=CardRarity.UNCOMMON,
        effect_vars={"poison_power": 3, "repeat": 4 if upgraded else 3},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_bubble_bubble(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BUBBLE_BUBBLE, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        effect_vars={"poison_power": 12 if upgraded else 9},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_calculated_gamble(upgraded: bool = False) -> CardInstance:
    keywords = {"exhaust"}
    if upgraded:
        keywords.add("retain")
    return CardInstance(
        card_id=CardId.CALCULATED_GAMBLE, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        keywords=frozenset(keywords), upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_dash(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.DASH, cost=2, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=13 if upgraded else 10, base_block=13 if upgraded else 10,
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_escape_plan(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.ESCAPE_PLAN, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        base_block=5 if upgraded else 3,
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_expertise(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.EXPERTISE, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={"cards": 7 if upgraded else 6},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_expose(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.EXPOSE, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        keywords=frozenset({"exhaust"}),
        effect_vars={"power": 3 if upgraded else 2},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_finisher(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.FINISHER, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=8 if upgraded else 6,
        effect_vars={"calc_base": 0, "calc_extra": 1},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_flanking(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.FLANKING, cost=1 if upgraded else 2, card_type=CardType.SKILL,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        effect_vars={"flanking_power": 2},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_flechettes(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.FLECHETTES, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=7 if upgraded else 5, effect_vars={"calc_base": 0, "calc_extra": 1},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_follow_through(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.FOLLOW_THROUGH, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ALL_ENEMIES, rarity=CardRarity.UNCOMMON,
        base_damage=8 if upgraded else 6,
        effect_vars={"weak": 2 if upgraded else 1},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_footwork(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.FOOTWORK, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={"dexterity": 3 if upgraded else 2},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_hand_trick(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.HAND_TRICK, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        base_block=10 if upgraded else 7,
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_haze(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.HAZE, cost=3, card_type=CardType.SKILL,
        target_type=TargetType.ALL_ENEMIES, rarity=CardRarity.UNCOMMON,
        keywords=frozenset({"sly"}),
        effect_vars={"poison_power": 6 if upgraded else 4},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_hidden_daggers(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.HIDDEN_DAGGERS, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        upgraded=upgraded,
        effect_vars={"cards": 2, "shivs": 2}, instance_id=_get_next_id(),
    )


def make_infinite_blades(upgraded: bool = False) -> CardInstance:
    keywords = frozenset({"innate"}) if upgraded else frozenset()
    return CardInstance(
        card_id=CardId.INFINITE_BLADES_CARD, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        upgraded=upgraded, keywords=keywords,
        instance_id=_get_next_id(),
    )


def make_leg_sweep(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.LEG_SWEEP, cost=2, card_type=CardType.SKILL,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_block=14 if upgraded else 11, effect_vars={"weak": 3 if upgraded else 2},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_memento_mori(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.MEMENTO_MORI, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        # MementoMori.cs: new CalculationBaseVar(9m), new ExtraDamageVar(4m);
        # OnUpgrade: CalculationBase.UpgradeValueBy(2m), ExtraDamage.UpgradeValueBy(1m).
        # CalculatedDamageVar = CalculationBase + ExtraDamage * discardsThisTurn.
        base_damage=11 if upgraded else 9,
        effect_vars={"calc_base": 11 if upgraded else 9, "extra_damage": 5 if upgraded else 4},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_mirage(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.MIRAGE, cost=0 if upgraded else 1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        keywords=frozenset({"exhaust"}),
        effect_vars={"calc_base": 0, "calc_extra": 1},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_noxious_fumes(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.NOXIOUS_FUMES_CARD, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={"poison_per_turn": 3 if upgraded else 2},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_outbreak(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.OUTBREAK_CARD, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={"outbreak_power": 15 if upgraded else 11, "repeat": OUTBREAK_REPEAT},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_phantom_blades(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.PHANTOM_BLADES_CARD, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={"phantom_blades_power": 12 if upgraded else 9},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_pinpoint(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.PINPOINT, cost=3, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        # Pinpoint.cs: new DamageVar(15m, ValueProp.Move); OnUpgrade: Damage.UpgradeValueBy(4m)
        base_damage=19 if upgraded else 15,
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_pounce(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.POUNCE, cost=2, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        # Pounce.cs: new DamageVar(14m, ValueProp.Move); OnUpgrade: Damage.UpgradeValueBy(6m)
        base_damage=20 if upgraded else 14,
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_precise_cut(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.PRECISE_CUT, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=16 if upgraded else 13,
        effect_vars={"calc_base": 16 if upgraded else 13, "extra_damage": 2},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_predator(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.PREDATOR, cost=2, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.COMMON,
        base_damage=20 if upgraded else 15,
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_reflex(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.REFLEX, cost=3, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        keywords=frozenset({"sly"}),
        effect_vars={"cards": 3 if upgraded else 2},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_skewer(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SKEWER, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        # Skewer.cs: new DamageVar(8m, ValueProp.Move); OnUpgrade: Damage.UpgradeValueBy(3m)
        base_damage=11 if upgraded else 8, upgraded=upgraded,
        has_energy_cost_x=True, instance_id=_get_next_id(),
    )


def make_speedster(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SPEEDSTER_CARD, cost=2, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        # Speedster.cs: new PowerVar<SpeedsterPower>(2m); OnUpgrade only AddKeyword(Innate)
        # — the power amount does not change on upgrade.
        keywords=frozenset({"innate"}) if upgraded else frozenset(),
        effect_vars={"speedster_power": 2},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_strangle(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.STRANGLE, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=10 if upgraded else 8, effect_vars={"strangle_power": 3 if upgraded else 2},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_tactician(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.TACTICIAN, cost=3, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        keywords=frozenset({"sly"}),
        effect_vars={"energy": 2 if upgraded else 1},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_up_my_sleeve(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.UP_MY_SLEEVE, cost=2, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={"cards": 4 if upgraded else 3},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_well_laid_plans(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.WELL_LAID_PLANS, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={"retain_amount": 2 if upgraded else 1},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_abrasive(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.ABRASIVE, cost=3, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        keywords=frozenset({"sly"}),
        effect_vars={"thorns_power": 6 if upgraded else 4, "dexterity": 1},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_accelerant(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.ACCELERANT, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        effect_vars={"accelerant": 2 if upgraded else 1},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_adrenaline(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.ADRENALINE, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        keywords=frozenset({"exhaust"}),
        effect_vars={"cards": 2, "energy": 2 if upgraded else 1},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_afterimage(upgraded: bool = False) -> CardInstance:
    keywords = frozenset({"innate"}) if upgraded else frozenset()
    return CardInstance(
        card_id=CardId.AFTERIMAGE_CARD, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        effect_vars={"afterimage_power": 1},
        upgraded=upgraded, keywords=keywords, instance_id=_get_next_id(),
    )


def make_assassinate(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.ASSASSINATE, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.RARE,
        base_damage=13 if upgraded else 10, keywords=frozenset({"innate", "exhaust"}),
        effect_vars={"vulnerable": 2 if upgraded else 1},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_blade_of_ink(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BLADE_OF_INK, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        # BladeOfInk.cs: new CardsVar(2); OnUpgrade: Cards.UpgradeValueBy(1m).
        # OnPlay creates that many Shivs in hand, each enchanted with Inky.
        effect_vars={"cards": 3 if upgraded else 2},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_bullet_time(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BULLET_TIME, cost=2 if upgraded else 3, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_burst(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BURST, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        effect_vars={"skills": 2 if upgraded else 1},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_corrosive_wave(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.CORROSIVE_WAVE, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        # CorrosiveWave.cs: new DynamicVar("CorrosiveWave", 2m);
        # OnUpgrade: DynamicVars["CorrosiveWave"].UpgradeValueBy(1m)
        effect_vars={"corrosive_wave": 3 if upgraded else 2},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_echoing_slash(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.ECHOING_SLASH, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ALL_ENEMIES, rarity=CardRarity.RARE,
        base_damage=13 if upgraded else 10,
        effect_vars={"damage": 13 if upgraded else 10},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_envenom(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.ENVENOM_CARD, cost=2, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        effect_vars={"envenom_power": 2 if upgraded else 1},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_fan_of_knives(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.FAN_OF_KNIVES_CARD, cost=2, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        effect_vars={"shivs": 5 if upgraded else 4},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_grand_finale(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.GRAND_FINALE, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ALL_ENEMIES, rarity=CardRarity.RARE,
        # GrandFinale.cs: new DamageVar(60m, ValueProp.Move); OnUpgrade: Damage.UpgradeValueBy(15m)
        base_damage=75 if upgraded else 60,
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_knife_trap(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.KNIFE_TRAP, cost=2, card_type=CardType.SKILL,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.RARE,
        effect_vars={"calc_base": 0, "calc_extra": 1},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_malaise(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.MALAISE, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.RARE,
        keywords=frozenset({"exhaust"}), has_energy_cost_x=True,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_master_planner(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.MASTER_PLANNER, cost=1 if upgraded else 2, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_murder(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.MURDER, cost=2 if upgraded else 3, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.RARE,
        upgraded=upgraded,
        base_damage=1, effect_vars={"calc_base": 1, "extra_damage": 1},
        instance_id=_get_next_id(),
    )


def make_nightmare(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.NIGHTMARE, cost=2 if upgraded else 3, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        keywords=frozenset({"exhaust"}), upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_serpent_form(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SERPENT_FORM_CARD, cost=3, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        # SerpentForm.cs: new PowerVar<SerpentFormPower>(4m);
        # OnUpgrade: DynamicVars["SerpentFormPower"].UpgradeValueBy(2m)
        effect_vars={"serpent_form_power": 6 if upgraded else 4},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_shadow_step(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SHADOW_STEP, cost=0 if upgraded else 1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        effect_vars={"cards": 3}, upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_shadowmeld(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SHADOWMELD, cost=0 if upgraded else 1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        effect_vars={"power": 1}, upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_sneaky(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SNEAKY_CARD, cost=2, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        keywords=frozenset({"sly"}),
        effect_vars={"sneaky_power": 2 if upgraded else 1},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_storm_of_steel(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.STORM_OF_STEEL, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_the_hunt(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.THE_HUNT, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.RARE,
        base_damage=15 if upgraded else 10, keywords=frozenset({"exhaust"}),
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_tools_of_the_trade(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.TOOLS_OF_THE_TRADE, cost=0 if upgraded else 1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_tracking(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.TRACKING, cost=1 if upgraded else 2, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_suppress(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SUPPRESS, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.ANCIENT,
        base_damage=17 if upgraded else 11, keywords=frozenset({"innate"}),
        effect_vars={"weak": 5 if upgraded else 3},
        upgraded=upgraded, instance_id=_get_next_id(),
    )


def make_wraith_form(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.WRAITH_FORM, cost=3, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.ANCIENT,
        effect_vars={"intangible_power": 3 if upgraded else 2, "wraith_form_power": 1},
        upgraded=upgraded, instance_id=_get_next_id(),
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


# --- Scare (game v0.107.1, Silent uncommon) ---


def make_scare(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SCARE,
        cost=0,
        card_type=CardType.SKILL,
        target_type=TargetType.ALL_ENEMIES,
        rarity=CardRarity.UNCOMMON,
        keywords=frozenset() if upgraded else frozenset({"exhaust"}),
        effect_vars={"weak": 1},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


@register_effect(CardId.SCARE)
def scare_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Apply 1 Weak to ALL enemies (C# Scare.OnPlay -> PowerCmd.Apply<WeakPower>)."""
    for enemy in combat.hittable_enemies:
        combat.apply_power_to(enemy, PowerId.WEAK, card.effect_vars.get("weak", 1))
