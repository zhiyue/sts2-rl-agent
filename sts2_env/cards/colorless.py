"""Colorless card effects and factories (64 cards).

Covers all Colorless cards: Uncommon (39), Rare (25).
"""

from __future__ import annotations

from sts2_env.cards.base import CardInstance, _get_next_id
from sts2_env.cards.factory import (
    card_preview,
    create_cards_from_ids,
    create_character_cards,
    create_distinct_character_cards,
    eligible_character_cards,
    eligible_registered_cards,
)
from sts2_env.cards.registry import register_effect
from sts2_env.characters.all import ALL_CHARACTERS, get_character
from sts2_env.core.enums import (
    CardId, CardType, TargetType, CardRarity, PowerStackType, PowerType, ValueProp, PowerId,
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
# Uncommon cards (39)
# ---------------------------------------------------------------------------

@register_effect(CardId.AUTOMATION)
def automation(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.AUTOMATION, 1)


@register_effect(CardId.BELIEVE_IN_YOU)
def believe_in_you(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    energy = card.effect_vars.get("energy", 3)
    combat.gain_energy(target if target is not None else _owner(card, combat), energy)


@register_effect(CardId.CATASTROPHE)
def catastrophe(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    for _ in range(card.effect_vars.get("cards", 2)):
        candidates = [c for c in combat.draw_pile if not c.is_unplayable]
        if not candidates:
            candidates = list(combat.draw_pile)
        if not candidates:
            break
        combat.auto_play_card(combat.rng.choice(candidates))


@register_effect(CardId.COORDINATE_CARD)
def coordinate_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    strength = card.effect_vars.get("strength", 5)
    if target is not None:
        combat.apply_power_to(target, PowerId.COORDINATE, strength)
    else:
        combat.apply_power_to(_owner(card, combat), PowerId.COORDINATE, strength)


@register_effect(CardId.DARK_SHACKLES)
def dark_shackles(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    strength_loss = card.effect_vars.get("strength_loss", 9)
    combat.apply_power_to(target, PowerId.DARK_SHACKLES, strength_loss)


@register_effect(CardId.DISCOVERY)
def discovery(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    candidates = create_distinct_character_cards(combat.character_id, combat.rng, 3)
    if not candidates:
        return

    def _resolver(selected: CardInstance | None) -> None:
        if selected is None:
            return
        selected.set_temporary_cost_for_turn(0)
        combat.move_card_to_hand(selected)

    combat.request_card_choice(
        prompt="Choose one of three cards",
        cards=candidates,
        source_pile="generated",
        resolver=_resolver,
        allow_skip=True,
    )


@register_effect(CardId.DRAMATIC_ENTRANCE)
def dramatic_entrance(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _deal_damage_all(card, combat)


@register_effect(CardId.EQUILIBRIUM)
def equilibrium(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    combat.apply_power_to(_owner(card, combat), PowerId.RETAIN_HAND, 1)


@register_effect(CardId.FASTEN)
def fasten(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    extra_block = card.effect_vars.get("extra_block", 5)
    combat.apply_power_to(_owner(card, combat), PowerId.FASTEN, extra_block)


@register_effect(CardId.FINESSE)
def finesse(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    cards = card.effect_vars.get("cards", 1)
    combat._draw_cards(cards)


@register_effect(CardId.FISTICUFFS)
def fisticuffs(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    damage = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
    result = apply_damage(target, damage, ValueProp.MOVE, combat, owner)
    owner.gain_block(result.unblocked_damage)


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
    owner = _owner(card, combat)
    count = sum(
        1
        for dealer, damaged_target, props in combat._damage_events_this_turn
        if damaged_target is target
        and dealer is not None
        and dealer is not owner
        and dealer.side == owner.side
        and props.is_powered()
    )
    base = card.effect_vars.get("calc_base", card.base_damage or 5)
    extra = card.effect_vars.get("extra_damage", 5)
    total_damage = base + extra * count
    damage = calculate_damage(total_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, damage, ValueProp.MOVE, combat, owner)


@register_effect(CardId.HUDDLE_UP)
def huddle_up(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    cards = card.effect_vars.get("cards", 2)
    for teammate in [
        creature
        for creature in combat.get_teammates_of(_owner(card, combat))
        if creature is not None and creature.is_alive and getattr(creature, "is_player", False)
    ]:
        combat._draw_cards_for_creature(teammate, cards)


@register_effect(CardId.IMPATIENCE)
def impatience(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Draw cards only if no attacks in hand
    cards = card.effect_vars.get("cards", 2)
    has_attack = any(c.is_attack for c in combat.hand)
    if not has_attack:
        combat._draw_cards(cards)


@register_effect(CardId.INTERCEPT_CARD)
def intercept_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    resolved_target = target if target is not None else _owner(card, combat)
    block = calculate_block(card.base_block, resolved_target, ValueProp.MOVE, combat, card_source=card)
    resolved_target.gain_block(block)
    if target is not None:
        combat.apply_power_to(target, PowerId.COVERED, 1)


@register_effect(CardId.JACK_OF_ALL_TRADES)
def jack_of_all_trades(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    colorless_ids = eligible_registered_cards(
        module_name=__name__,
        exclude_ids={CardId.JACK_OF_ALL_TRADES},
    )
    generated = create_cards_from_ids(
        colorless_ids,
        combat.rng,
        card.effect_vars.get("cards", 1),
        distinct=True,
    )
    for generated_card in generated:
        combat.move_card_to_hand(generated_card)


@register_effect(CardId.LIFT)
def lift(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    if target is None:
        return
    block = calculate_block(card.base_block, target, ValueProp.MOVE, combat, card_source=card)
    target.gain_block(block)


@register_effect(CardId.MIND_BLAST)
def mind_blast(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Damage scales with draw pile size."""
    assert target is not None
    owner = _owner(card, combat)
    total_damage = card.effect_vars.get("calc_base", 0) + card.effect_vars.get("extra_damage", 1) * len(combat.draw_pile)
    damage = calculate_damage(total_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, damage, ValueProp.MOVE, combat, owner)


@register_effect(CardId.OMNISLICE)
def omnislice(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    base_damage = card.base_damage if card.base_damage is not None else card.effect_vars.get("damage", 8)
    damage = calculate_damage(base_damage, owner, target, ValueProp.MOVE, combat)
    result = apply_damage(target, damage, ValueProp.MOVE, combat, owner)

    splash_damage = result.blocked + result.unblocked_damage
    if splash_damage <= 0:
        return

    for teammate in [enemy for enemy in combat.get_teammates_of(target) if enemy.is_alive]:
        teammate_damage = calculate_damage(
            splash_damage,
            owner,
            teammate,
            ValueProp.UNPOWERED | ValueProp.MOVE,
            combat,
        )
        apply_damage(
            teammate,
            teammate_damage,
            ValueProp.UNPOWERED | ValueProp.MOVE,
            combat,
            owner,
        )


@register_effect(CardId.PANACHE_CARD)
def panache_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    damage = card.effect_vars.get("panache_damage", 10)
    combat.apply_power_to(_owner(card, combat), PowerId.PANACHE, damage)


@register_effect(CardId.PANIC_BUTTON)
def panic_button(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    turns = card.effect_vars.get("turns", 2)
    combat.apply_power_to(_owner(card, combat), PowerId.NO_BLOCK, turns)


@register_effect(CardId.PREP_TIME)
def prep_time(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("prep_time", 4)
    combat.apply_power_to(_owner(card, combat), PowerId.PREP_TIME, amount)


@register_effect(CardId.PRODUCTION)
def production(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    energy = card.effect_vars.get("energy", 2)
    combat.energy += energy


@register_effect(CardId.PROLONG)
def prolong(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Apply BlockNextTurn — carry current block to next turn
    owner = _owner(card, combat)
    block = owner.block
    if block > 0:
        combat.apply_power_to(owner, PowerId.BLOCK_NEXT_TURN, block)


@register_effect(CardId.PROWESS)
def prowess(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    strength = card.effect_vars.get("strength", 1)
    dexterity = card.effect_vars.get("dexterity", 1)
    combat.apply_power_to(_owner(card, combat), PowerId.STRENGTH, strength)
    combat.apply_power_to(_owner(card, combat), PowerId.DEXTERITY, dexterity)


@register_effect(CardId.PURITY)
def purity(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    cards = [c for c in combat.hand]
    if not cards:
        return
    combat.request_multi_card_choice(
        prompt="Choose any number of hand cards to exhaust",
        cards=cards,
        source_pile="hand",
        resolver=lambda selected_cards: [combat.exhaust_card(selected) for selected in selected_cards],
        min_count=0,
        max_count=card.effect_vars.get("cards", 3),
        allow_skip=True,
    )


@register_effect(CardId.RESTLESSNESS)
def restlessness(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    if combat.hand:
        return
    cards = card.effect_vars.get("cards", 2)
    energy = card.effect_vars.get("energy", 2)
    combat._draw_cards(cards)
    combat.energy += energy


@register_effect(CardId.SEEKER_STRIKE)
def seeker_strike(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    candidates = list(combat.draw_pile)
    if not candidates:
        return
    combat.rng.shuffle(candidates)
    candidates = candidates[:card.effect_vars.get("cards", 2)]
    combat.request_card_choice(
        prompt="Choose one of the revealed cards",
        cards=candidates,
        source_pile="draw",
        resolver=combat.move_card_to_hand,
    )


@register_effect(CardId.SHOCKWAVE)
def shockwave(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("power", 3)
    for enemy in list(combat.alive_enemies):
        combat.apply_power_to(enemy, PowerId.WEAK, amount)
        combat.apply_power_to(enemy, PowerId.VULNERABLE, amount)


@register_effect(CardId.SPLASH)
def splash(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    own_character = combat.character_id.lower()
    candidate_ids: list[CardId] = []
    for cfg in ALL_CHARACTERS:
        if cfg.character_id.lower() == own_character:
            continue
        candidate_ids.extend(
            eligible_character_cards(cfg.character_id, card_type=CardType.ATTACK)
        )

    if not candidate_ids:
        return

    generated = create_cards_from_ids(candidate_ids, combat.rng, 3, distinct=True)
    for generated_card in generated:
        if card.upgraded:
            combat.upgrade_card(generated_card)
        generated_card.set_temporary_cost_for_turn(0)

    combat.request_card_choice(
        prompt="Choose one upgraded free attack",
        cards=generated,
        source_pile="generated",
        resolver=combat.move_card_to_hand,
        allow_skip=True,
    )


@register_effect(CardId.STRATAGEM)
def stratagem(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.STRATAGEM, 1)


@register_effect(CardId.TAG_TEAM)
def tag_team(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    combat.apply_power_to(target, PowerId.TAG_TEAM, 1)


@register_effect(CardId.THE_BOMB_CARD)
def the_bomb_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    damage = card.effect_vars.get("bomb_damage", 40)
    combat.apply_power_to(_owner(card, combat), PowerId.THE_BOMB, damage)


@register_effect(CardId.THINKING_AHEAD)
def thinking_ahead(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    cards = card.effect_vars.get("cards", 2)
    combat._draw_cards(cards)
    if not combat.hand:
        return
    combat.request_card_choice(
        prompt="Choose a hand card to put back on draw pile",
        cards=list(combat.hand),
        source_pile="hand",
        resolver=lambda selected: combat.insert_card_into_draw_pile(selected, random_position=False),
    )


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
    hits = getattr(card, "energy_spent", 0)
    if hits <= 0:
        return
    owner = _owner(card, combat)
    for _ in range(hits):
        alive = combat.alive_enemies
        if not alive:
            break
        t = combat.rng.choice(alive)
        damage = calculate_damage(card.base_damage, owner, t, ValueProp.MOVE, combat)
        apply_damage(t, damage, ValueProp.MOVE, combat, owner)


# ---------------------------------------------------------------------------
# Rare cards (25)
# ---------------------------------------------------------------------------

@register_effect(CardId.ALCHEMIZE)
def alchemize(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.procure_random_potion(_owner(card, combat), in_combat=True)


@register_effect(CardId.ANOINTED)
def anointed(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    rares = [c for c in list(combat.draw_pile) if c.rarity == CardRarity.RARE]
    for rare in rares:
        combat.move_card_to_hand(rare)


@register_effect(CardId.BEACON_OF_HOPE)
def beacon_of_hope(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.BEACON_OF_HOPE, 1)


@register_effect(CardId.BEAT_DOWN)
def beat_down(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    cards = card.effect_vars.get("cards", 3)
    discard = combat._zones_for_creature(owner)["discard"]  # noqa: SLF001
    candidates = [c for c in discard if c.card_type == CardType.ATTACK and not c.is_unplayable]
    combat.rng.shuffle(candidates)
    for selected in candidates[:cards]:
        if combat.is_over:
            break
        resolved_target = combat.random_enemy_of(owner) if selected.target_type == TargetType.ANY_ENEMY else None
        combat.auto_play_card(selected, target=resolved_target)


@register_effect(CardId.BOLAS)
def bolas(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)


@register_effect(CardId.CALAMITY_CARD)
def calamity_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.CALAMITY, 1)


@register_effect(CardId.ENTROPY)
def entropy(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.ENTROPY, 1)


@register_effect(CardId.ETERNAL_ARMOR)
def eternal_armor(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    plating = card.effect_vars.get("plating", 7)
    combat.apply_power_to(_owner(card, combat), PowerId.PLATING, plating)


@register_effect(CardId.GOLD_AXE)
def gold_axe(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Damage scales with gold."""
    assert target is not None
    owner = _owner(card, combat)
    total_damage = card.effect_vars.get("calc_base", 0) + card.effect_vars.get("extra_damage", 1) * combat.count_cards_played_this_combat(owner)
    damage = calculate_damage(total_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, damage, ValueProp.MOVE, combat, owner)


@register_effect(CardId.HAND_OF_GREED)
def hand_of_greed(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    should_trigger_fatal = combat.should_owner_death_trigger_fatal(target)
    _deal_damage_single(card, combat, target)
    if should_trigger_fatal and target.is_dead:
        owner.gain_gold(card.effect_vars.get("gold", 20))


@register_effect(CardId.HIDDEN_GEM)
def hidden_gem(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    candidates = [
        c for c in combat.draw_pile
        if not c.is_unplayable and c.card_type in (CardType.ATTACK, CardType.SKILL, CardType.POWER)
    ]
    if not candidates:
        candidates = [c for c in combat.draw_pile if not c.is_unplayable]
    if not candidates:
        return
    selected = combat.rng.choice(candidates)
    selected.base_replay_count += card.effect_vars.get("replay", 2)


@register_effect(CardId.JACKPOT)
def jackpot(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    zero_cost_ids = [
        card_id
        for card_id in eligible_registered_cards(
            module_name=None,
            card_type=None,
        )
        if card_preview(card_id).cost == 0 and not card_preview(card_id).has_energy_cost_x
    ]
    generated = create_cards_from_ids(
        [card_id for card_id in zero_cost_ids if card_id in set(get_character(combat.character_id).card_pool)],
        combat.rng,
        card.effect_vars.get("cards", 3),
        distinct=False,
    )
    for generated_card in generated:
        if card.upgraded:
            combat.upgrade_card(generated_card)
        combat.move_card_to_hand(generated_card)


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
    combat.apply_power_to(_owner(card, combat), PowerId.MAYHEM, 1)


@register_effect(CardId.MIMIC)
def mimic(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Gain block scaling with ally block."""
    if target is None:
        return
    owner = _owner(card, combat)
    total_block = card.effect_vars.get("calc_base", 0) + card.effect_vars.get("calc_extra", 1) * target.block
    block = calculate_block(total_block, owner, ValueProp.MOVE, combat, card_source=card)
    owner.gain_block(block)


@register_effect(CardId.NOSTALGIA_CARD)
def nostalgia_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.NOSTALGIA, 1)


@register_effect(CardId.RALLY)
def rally(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    for teammate in [
        creature
        for creature in combat.get_teammates_of(_owner(card, combat))
        if creature is not None and creature.is_alive and getattr(creature, "is_player", False)
    ]:
        block = calculate_block(card.base_block, teammate, ValueProp.MOVE, combat, card_source=card)
        teammate.gain_block(block)


@register_effect(CardId.REND)
def rend(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Scales with non-temporary debuffs on the target."""
    assert target is not None
    owner = _owner(card, combat)
    debuffs = sum(
        1
        for power in target.powers.values()
        if power.power_type == PowerType.DEBUFF and power.stack_type != PowerStackType.DURATION
    )
    base = card.effect_vars.get("calc_base", card.base_damage or 15)
    extra = card.effect_vars.get("extra_damage", 5)
    total_damage = base + extra * debuffs
    damage = calculate_damage(total_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, damage, ValueProp.MOVE, combat, owner)


@register_effect(CardId.ROLLING_BOULDER)
def rolling_boulder(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("rolling_boulder", 5)
    combat.apply_power_to(_owner(card, combat), PowerId.ROLLING_BOULDER, amount)


@register_effect(CardId.SALVO)
def salvo(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    combat.apply_power_to(_owner(card, combat), PowerId.RETAIN_HAND, 1)


@register_effect(CardId.SCRAWL)
def scrawl(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Draw until hand is full
    from sts2_env.core.constants import MAX_HAND_SIZE
    to_draw = MAX_HAND_SIZE - len(combat.hand)
    if to_draw > 0:
        combat._draw_cards(to_draw)


@register_effect(CardId.SECRET_TECHNIQUE)
def secret_technique(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    candidates = [c for c in combat.draw_pile if c.card_type == CardType.SKILL]
    if not candidates:
        return
    combat.request_card_choice(
        prompt="Choose a Skill from draw pile",
        cards=candidates,
        source_pile="draw",
        resolver=combat.move_card_to_hand,
    )


@register_effect(CardId.SECRET_WEAPON)
def secret_weapon(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    candidates = [c for c in combat.draw_pile if c.card_type == CardType.ATTACK]
    if not candidates:
        return
    combat.request_card_choice(
        prompt="Choose an Attack from draw pile",
        cards=candidates,
        source_pile="draw",
        resolver=combat.move_card_to_hand,
    )


@register_effect(CardId.THE_GAMBIT)
def the_gambit(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    combat.apply_power_to(_owner(card, combat), PowerId.THE_GAMBIT, 1)


# ---------------------------------------------------------------------------
# Card factories (selected key cards)
# ---------------------------------------------------------------------------

def make_discovery(upgraded: bool = False) -> CardInstance:
    keywords = frozenset() if upgraded else frozenset({"exhaust"})
    return CardInstance(
        card_id=CardId.DISCOVERY, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        upgraded=upgraded, keywords=keywords,
        instance_id=_get_next_id(),
    )


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


def make_omnislice(upgraded: bool = False) -> CardInstance:
    damage = 11 if upgraded else 8
    return CardInstance(
        card_id=CardId.OMNISLICE, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=damage, upgraded=upgraded,
        effect_vars={"damage": damage},
        instance_id=_get_next_id(),
    )


def make_secret_technique(upgraded: bool = False) -> CardInstance:
    keywords = frozenset() if upgraded else frozenset({"exhaust"})
    return CardInstance(
        card_id=CardId.SECRET_TECHNIQUE, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        upgraded=upgraded, keywords=keywords,
        instance_id=_get_next_id(),
    )


def make_secret_weapon(upgraded: bool = False) -> CardInstance:
    keywords = frozenset() if upgraded else frozenset({"exhaust"})
    return CardInstance(
        card_id=CardId.SECRET_WEAPON, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        upgraded=upgraded, keywords=keywords,
        instance_id=_get_next_id(),
    )


def make_thinking_ahead(upgraded: bool = False) -> CardInstance:
    keywords = frozenset() if upgraded else frozenset({"exhaust"})
    return CardInstance(
        card_id=CardId.THINKING_AHEAD, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        upgraded=upgraded, keywords=keywords,
        effect_vars={"cards": 2},
        instance_id=_get_next_id(),
    )


def make_alchemize(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.ALCHEMIZE, cost=0 if upgraded else 1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        upgraded=upgraded, keywords=frozenset({"exhaust"}),
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


def make_hidden_gem(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.HIDDEN_GEM, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        upgraded=upgraded,
        effect_vars={"replay": 3 if upgraded else 2},
        instance_id=_get_next_id(),
    )


def make_fisticuffs(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.FISTICUFFS, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        upgraded=upgraded, base_damage=9 if upgraded else 7,
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


def make_purity(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.PURITY, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        upgraded=upgraded, keywords=frozenset({"retain", "exhaust"}),
        effect_vars={"cards": 5 if upgraded else 3},
        instance_id=_get_next_id(),
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


def _make_reference_factory_card(card_id: CardId, upgraded: bool = False) -> CardInstance:
    from sts2_env.cards.factory import create_reference_card

    return create_reference_card(card_id, upgraded=upgraded, allow_generation=True)


_REFERENCE_FACTORY_CARD_IDS = (
    CardId.AUTOMATION,
    CardId.BELIEVE_IN_YOU,
    CardId.CATASTROPHE,
    CardId.DARK_SHACKLES,
    CardId.EQUILIBRIUM,
    CardId.FASTEN,
    CardId.HUDDLE_UP,
    CardId.IMPATIENCE,
    CardId.INTERCEPT_CARD,
    CardId.JACK_OF_ALL_TRADES,
    CardId.LIFT,
    CardId.PANACHE_CARD,
    CardId.PREP_TIME,
    CardId.PRODUCTION,
    CardId.PROLONG,
    CardId.PROWESS,
    CardId.RESTLESSNESS,
    CardId.SEEKER_STRIKE,
    CardId.SPLASH,
    CardId.STRATAGEM,
    CardId.TAG_TEAM,
    CardId.THE_BOMB_CARD,
    CardId.THRUMMING_HATCHET,
    CardId.ANOINTED,
    CardId.BEACON_OF_HOPE,
    CardId.BEAT_DOWN,
    CardId.BOLAS,
    CardId.CALAMITY_CARD,
    CardId.ENTROPY,
    CardId.ETERNAL_ARMOR,
    CardId.JACKPOT,
    CardId.KNOCKDOWN,
    CardId.MAYHEM_CARD,
    CardId.NOSTALGIA_CARD,
    CardId.ROLLING_BOULDER,
    CardId.SCRAWL,
)

for _card_id in _REFERENCE_FACTORY_CARD_IDS:
    _factory_name = f"make_{_card_id.name.lower()}"
    if _factory_name in globals():
        continue

    def _factory(upgraded: bool = False, *, _cid: CardId = _card_id) -> CardInstance:
        return _make_reference_factory_card(_cid, upgraded=upgraded)

    _factory.__name__ = _factory_name
    globals()[_factory_name] = _factory


def make_coordinate_card(upgraded: bool = False) -> CardInstance:
    card = _make_reference_factory_card(CardId.COORDINATE_CARD, upgraded=upgraded)
    card.effect_vars["strength"] = 8 if upgraded else 5
    return card


def make_gang_up(upgraded: bool = False) -> CardInstance:
    card = _make_reference_factory_card(CardId.GANG_UP, upgraded=upgraded)
    card.base_damage = 7 if upgraded else 5
    card.effect_vars["calc_base"] = 5
    card.effect_vars["extra_damage"] = 7 if upgraded else 5
    return card


def make_mind_blast(upgraded: bool = False) -> CardInstance:
    card = _make_reference_factory_card(CardId.MIND_BLAST, upgraded=upgraded)
    card.cost = 0 if upgraded else 1
    card.original_cost = card.cost
    card.base_damage = 0
    card.effect_vars["calc_base"] = 0
    card.effect_vars["extra_damage"] = 1
    return card


def make_gold_axe(upgraded: bool = False) -> CardInstance:
    card = _make_reference_factory_card(CardId.GOLD_AXE, upgraded=upgraded)
    if upgraded:
        card.keywords = frozenset(set(card.keywords) | {"retain"})
    card.base_damage = 0
    card.effect_vars["calc_base"] = 0
    card.effect_vars["extra_damage"] = 1
    return card


def make_mimic(upgraded: bool = False) -> CardInstance:
    card = _make_reference_factory_card(CardId.MIMIC, upgraded=upgraded)
    if upgraded:
        card.keywords = frozenset(keyword for keyword in card.keywords if keyword != "exhaust")
    card.base_block = 0
    card.effect_vars["calc_base"] = 0
    card.effect_vars["calc_extra"] = 1
    return card


def make_rally(upgraded: bool = False) -> CardInstance:
    card = _make_reference_factory_card(CardId.RALLY, upgraded=upgraded)
    card.base_block = 17 if upgraded else 12
    return card


def make_rend(upgraded: bool = False) -> CardInstance:
    card = _make_reference_factory_card(CardId.REND, upgraded=upgraded)
    card.base_damage = 18 if upgraded else 15
    card.effect_vars["calc_base"] = 18 if upgraded else 15
    card.effect_vars["extra_damage"] = 8 if upgraded else 5
    return card


def make_salvo(upgraded: bool = False) -> CardInstance:
    card = _make_reference_factory_card(CardId.SALVO, upgraded=upgraded)
    card.base_damage = 16 if upgraded else 12
    return card


def make_the_gambit(upgraded: bool = False) -> CardInstance:
    card = _make_reference_factory_card(CardId.THE_GAMBIT, upgraded=upgraded)
    card.base_block = 75 if upgraded else 50
    return card


def make_volley(upgraded: bool = False) -> CardInstance:
    card = _make_reference_factory_card(CardId.VOLLEY, upgraded=upgraded)
    card.base_damage = 14 if upgraded else 10
    return card
