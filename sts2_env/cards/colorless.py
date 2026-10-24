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
from sts2_env.core.card_pools import CardPoolId
from sts2_env.core.enums import (
    CardId, CardType, TargetType, CardRarity, CardTag, PowerStackType, PowerType, ValueProp, PowerId,
)
from sts2_env.core.damage import calculate_damage, apply_damage, calculate_block
from sts2_env.core.creature import Creature, _power_type_for_amount, get_power_class
from sts2_env.core.combat import CombatState


BELIEVE_IN_YOU_ENERGY_KEY = "energy"
# BelieveInYou.cs:18 EnergyVar(2); OnUpgrade (31-34) UpgradeValueBy(1).
BELIEVE_IN_YOU_ENERGY = 2
BELIEVE_IN_YOU_UPGRADED_ENERGY = 3
CATASTROPHE_CARDS_KEY = "cards"
CATASTROPHE_CARDS = 2
CATASTROPHE_UPGRADED_CARDS = 3
COORDINATE_STRENGTH_KEY = "strength"
COORDINATE_STRENGTH = 5
COORDINATE_UPGRADED_STRENGTH = 8
DARK_SHACKLES_STRENGTH_LOSS_KEY = "strength_loss"
DARK_SHACKLES_STRENGTH_LOSS = 9
DARK_SHACKLES_UPGRADED_STRENGTH_LOSS = 15
DRAMATIC_ENTRANCE_DAMAGE = 11
DRAMATIC_ENTRANCE_UPGRADED_DAMAGE = 15
EQUILIBRIUM_BLOCK = 13
EQUILIBRIUM_UPGRADED_BLOCK = 16
EQUILIBRIUM_POWER = 1
FASTEN_EXTRA_BLOCK_KEY = "extra_block"
# Fasten.cs:38 DynamicVar("ExtraBlock", 4); OnUpgrade (50-53) UpgradeValueBy(2).
FASTEN_EXTRA_BLOCK = 4
FASTEN_UPGRADED_EXTRA_BLOCK = 6
FINESSE_BLOCK = 4
FINESSE_UPGRADED_BLOCK = 7
FINESSE_CARDS_KEY = "cards"
FINESSE_CARDS = 1
FLASH_OF_STEEL_DAMAGE = 5
FLASH_OF_STEEL_UPGRADED_DAMAGE = 8
FLASH_OF_STEEL_CARDS_KEY = "cards"
FLASH_OF_STEEL_CARDS = 1
INTERCEPT_BLOCK = 9
INTERCEPT_UPGRADED_BLOCK = 13
INTERCEPT_COVERED = 1
JACK_OF_ALL_TRADES_CARDS_KEY = "cards"
JACK_OF_ALL_TRADES_CARDS = 1
JACK_OF_ALL_TRADES_UPGRADED_CARDS = 2
LIFT_BLOCK = 11
LIFT_UPGRADED_BLOCK = 16
MASTER_OF_STRATEGY_CARDS_KEY = "cards"
MASTER_OF_STRATEGY_CARDS = 3
MASTER_OF_STRATEGY_UPGRADED_CARDS = 4
MIMIC_CALC_BASE_KEY = "calc_base"
MIMIC_CALC_BASE = 0
MIMIC_CALC_EXTRA_KEY = "calc_extra"
MIMIC_CALC_EXTRA = 1
MIND_BLAST_CALC_BASE_KEY = "calc_base"
MIND_BLAST_CALC_BASE = 0
MIND_BLAST_EXTRA_DAMAGE_KEY = "extra_damage"
MIND_BLAST_EXTRA_DAMAGE = 1
OMNISLICE_DAMAGE_KEY = "damage"
OMNISLICE_DAMAGE = 8
OMNISLICE_UPGRADED_DAMAGE = 11
PANIC_BUTTON_BLOCK = 30
PANIC_BUTTON_UPGRADED_BLOCK = 40
PANIC_BUTTON_TURNS_KEY = "turns"
PANIC_BUTTON_TURNS = 2
PANACHE_DAMAGE_KEY = "panache_damage"
PANACHE_DAMAGE = 10
PANACHE_UPGRADED_DAMAGE = 14
PRODUCTION_ENERGY_KEY = "energy"
# Production.cs:13 EnergyVar(2); OnUpgrade (29-32) UpgradeValueBy(1) and does NOT
# remove the Exhaust keyword (CanonicalKeywords, line 15).
PRODUCTION_ENERGY = 2
PRODUCTION_UPGRADED_ENERGY = 3
PROWESS_STRENGTH_KEY = "strength"
PROWESS_DEXTERITY_KEY = "dexterity"
PROWESS_POWER = 1
PROWESS_UPGRADED_POWER = 2
RESTLESSNESS_CARDS_KEY = "cards"
RESTLESSNESS_CARDS = 2
RESTLESSNESS_UPGRADED_CARDS = 3
RESTLESSNESS_ENERGY_KEY = "energy"
RESTLESSNESS_ENERGY = 2
RESTLESSNESS_UPGRADED_ENERGY = 3
# SeekerStrike.cs:21 DamageVar(9); OnUpgrade (44-47) UpgradeValueBy(3).
SEEKER_STRIKE_DAMAGE = 9
SEEKER_STRIKE_UPGRADED_DAMAGE = 12
SEEKER_STRIKE_CARDS_KEY = "cards"
SEEKER_STRIKE_CARDS = 3
SHOCKWAVE_POWER_KEY = "power"
SHOCKWAVE_POWER = 3
SHOCKWAVE_UPGRADED_POWER = 5
SPLASH_ATTACK_CHOICES = 3
SPLASH_CHOICE_PROMPT = "Choose one upgraded free attack"
SPLASH_SOURCE_PILE = "generated"
STRATAGEM_COST = 1
STRATAGEM_UPGRADED_COST = 0
TAG_TEAM_DAMAGE = 11
TAG_TEAM_UPGRADED_DAMAGE = 15
TAG_TEAM_POWER = 1
THE_BOMB_COST = 2
THE_BOMB_TURNS_KEY = "turns"
THE_BOMB_TURNS = 3
THE_BOMB_DAMAGE_KEY = "bomb_damage"
THE_BOMB_DAMAGE = 40
THE_BOMB_UPGRADED_DAMAGE = 50
THRUMMING_HATCHET_DAMAGE = 11
THRUMMING_HATCHET_UPGRADED_DAMAGE = 14
ULTIMATE_DEFEND_BLOCK = 11
ULTIMATE_DEFEND_UPGRADED_BLOCK = 15
ULTIMATE_STRIKE_DAMAGE = 14
ULTIMATE_STRIKE_UPGRADED_DAMAGE = 20
VOLLEY_DAMAGE = 10
VOLLEY_UPGRADED_DAMAGE = 14
BOLAS_DAMAGE = 3
BOLAS_UPGRADED_DAMAGE = 4
CALAMITY_COST = 3
CALAMITY_UPGRADED_COST = 2
CALAMITY_POWER = 1
ENTROPY_CARDS_KEY = "cards"
ENTROPY_CARDS = 1
ETERNAL_ARMOR_PLATING_KEY = "plating"
# EternalArmor.cs:20 PowerVar<PlatingPower>(9); OnUpgrade (32-35) UpgradeValueBy(3).
ETERNAL_ARMOR_PLATING = 9
ETERNAL_ARMOR_UPGRADED_PLATING = 12
JACKPOT_DAMAGE = 25
JACKPOT_UPGRADED_DAMAGE = 30
JACKPOT_CARDS_KEY = "cards"
JACKPOT_CARDS = 3
MAYHEM_COST = 2
MAYHEM_UPGRADED_COST = 1
MAYHEM_POWER = 1
NOSTALGIA_COST = 1
NOSTALGIA_UPGRADED_COST = 0
NOSTALGIA_POWER = 1
RALLY_BLOCK = 12
RALLY_UPGRADED_BLOCK = 17
REND_CALC_BASE_KEY = "calc_base"
REND_DAMAGE = 15
REND_UPGRADED_DAMAGE = 18
REND_EXTRA_DAMAGE_KEY = "extra_damage"
REND_EXTRA_DAMAGE = 5
REND_UPGRADED_EXTRA_DAMAGE = 8
SALVO_DAMAGE = 12
SALVO_UPGRADED_DAMAGE = 16
THE_GAMBIT_BLOCK = 50
THE_GAMBIT_UPGRADED_BLOCK = 75


def _owner(card: CardInstance, combat: CombatState) -> Creature:
    return (
        getattr(card, "owner", None)
        or getattr(getattr(combat, "active_card_source", None), "owner", None)
        or combat.primary_player
    )


# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

def _deal_damage_single(card: CardInstance, combat: CombatState, target: Creature) -> None:
    owner = _owner(card, combat)
    damage = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, damage, ValueProp.MOVE, combat, owner)


def _deal_damage_all(card: CardInstance, combat: CombatState) -> None:
    owner = _owner(card, combat)
    if owner.is_dead:
        return
    for enemy in list(combat.hittable_enemies):
        damage = calculate_damage(card.base_damage, owner, enemy, ValueProp.MOVE, combat)
        apply_damage(enemy, damage, ValueProp.MOVE, combat, owner)


def _gain_block(card: CardInstance, combat: CombatState) -> None:
    owner = _owner(card, combat)
    block = calculate_block(card.base_block, owner, ValueProp.MOVE, combat, card_source=card)
    _gain_resolved_block(owner, block, combat)


def _gain_resolved_block(creature: Creature, block: int, combat: CombatState) -> None:
    before = creature.block
    creature.gain_block(block)
    gained = creature.block - before
    if gained > 0:
        from sts2_env.core.hooks import fire_after_block_gained

        fire_after_block_gained(creature, gained, combat, ValueProp.MOVE, combat.active_card_play_token)


# ---------------------------------------------------------------------------
# Uncommon cards (39)
# ---------------------------------------------------------------------------

@register_effect(CardId.AUTOMATION)
def automation(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.AUTOMATION, 1)


@register_effect(CardId.BELIEVE_IN_YOU)
def believe_in_you(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    energy = card.effect_vars.get(BELIEVE_IN_YOU_ENERGY_KEY, BELIEVE_IN_YOU_ENERGY)
    combat.gain_energy(target if target is not None else _owner(card, combat), energy)


@register_effect(CardId.CATASTROPHE)
def catastrophe(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    for _ in range(card.effect_vars.get(CATASTROPHE_CARDS_KEY, CATASTROPHE_CARDS)):
        candidates = [c for c in combat.draw_pile if not c.is_unplayable]
        if not candidates:
            candidates = list(combat.draw_pile)
        if not candidates:
            break
        combat.stable_shuffle_cards(candidates, combat.shuffle_rng)
        combat.auto_play_card(candidates[0])


@register_effect(CardId.COORDINATE_CARD)
def coordinate_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    strength = card.effect_vars.get(COORDINATE_STRENGTH_KEY, COORDINATE_STRENGTH)
    resolved_target = target if target is not None else _owner(card, combat)
    if target is not None:
        combat.apply_power_to(target, PowerId.COORDINATE, strength)
    else:
        combat.apply_power_to(resolved_target, PowerId.COORDINATE, strength)


@register_effect(CardId.DARK_SHACKLES)
def dark_shackles(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    strength_loss = card.effect_vars.get(
        DARK_SHACKLES_STRENGTH_LOSS_KEY,
        DARK_SHACKLES_STRENGTH_LOSS,
    )
    combat.apply_power_to(target, PowerId.DARK_SHACKLES, strength_loss)


@register_effect(CardId.DISCOVERY)
def discovery(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    candidates = create_distinct_character_cards(
        combat.character_id,
        combat.combat_card_generation_rng,
        3,
        generation_context="combat",
        is_multiplayer=combat.is_multiplayer,
    )
    if not candidates:
        return

    def _resolver(selected: CardInstance | None) -> None:
        if selected is None:
            return
        selected.set_temporary_cost_for_turn(0)
        combat.add_generated_card_to_creature_hand(_owner(card, combat), selected)

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
    combat.apply_power_to(_owner(card, combat), PowerId.RETAIN_HAND, EQUILIBRIUM_POWER)


@register_effect(CardId.FASTEN)
def fasten(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    extra_block = card.effect_vars.get(FASTEN_EXTRA_BLOCK_KEY, FASTEN_EXTRA_BLOCK)
    combat.apply_power_to(_owner(card, combat), PowerId.FASTEN, extra_block)


@register_effect(CardId.FINESSE)
def finesse(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    cards = card.effect_vars.get(FINESSE_CARDS_KEY, FINESSE_CARDS)
    combat._draw_cards(cards)


@register_effect(CardId.FISTICUFFS)
def fisticuffs(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    damage = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
    result = apply_damage(target, damage, ValueProp.MOVE, combat, owner)
    if combat.is_over:
        return
    block = calculate_block(
        result.total_damage + result.overkill_damage,
        owner,
        ValueProp.MOVE,
        combat,
        card_source=card,
    )
    _gain_resolved_block(owner, block, combat)


@register_effect(CardId.FLASH_OF_STEEL)
def flash_of_steel(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    cards = card.effect_vars.get(FLASH_OF_STEEL_CARDS_KEY, FLASH_OF_STEEL_CARDS)
    combat._draw_cards(cards)


@register_effect(CardId.GANG_UP)
def gang_up(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Scales with allies in combat."""
    assert target is not None
    owner = _owner(card, combat)
    count = combat.count_allied_powered_hits_on_target_this_turn(owner, target)
    base = card.effect_vars.get("calc_base", card.base_damage or 5)
    extra = card.effect_vars.get("extra_damage", 5)
    total_damage = base + extra * count
    damage = calculate_damage(total_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, damage, ValueProp.MOVE, combat, owner)


@register_effect(CardId.HUDDLE_UP)
def huddle_up(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    cards = card.effect_vars.get("cards", 2)
    # HuddleUp.cs OnPlay (lines 26-34): iterates CombatState.GetTeammatesOf,
    # which per CombatState.cs:397 "includes the creature itself" — so the
    # caster draws too. get_teammates_of excludes the owner, so add it here.
    owner = _owner(card, combat)
    teammates = [owner]
    for creature in combat.get_teammates_of(owner):
        if creature is not owner:
            teammates.append(creature)
    for teammate in [
        creature
        for creature in teammates
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
    owner = _owner(card, combat)
    resolved_target = target if target is not None else owner
    block = calculate_block(card.base_block, owner, ValueProp.MOVE, combat, card_source=card)
    _gain_resolved_block(owner, block, combat)
    combat.apply_power_to(resolved_target, PowerId.COVERED, INTERCEPT_COVERED, applier=owner, source=card)


@register_effect(CardId.JACK_OF_ALL_TRADES)
def jack_of_all_trades(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    colorless_ids = eligible_registered_cards(
        card_pool=CardPoolId.COLORLESS,
        exclude_ids={CardId.JACK_OF_ALL_TRADES},
        generation_context="combat",
        is_multiplayer=combat.is_multiplayer,
    )
    generated = create_cards_from_ids(
        colorless_ids,
        combat.combat_card_generation_rng,
        card.effect_vars.get(JACK_OF_ALL_TRADES_CARDS_KEY, JACK_OF_ALL_TRADES_CARDS),
        distinct=True,
    )
    for generated_card in generated:
        combat.add_generated_card_to_creature_hand(_owner(card, combat), generated_card)


@register_effect(CardId.LIFT)
def lift(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    if target is None:
        return
    block = calculate_block(card.base_block, target, ValueProp.MOVE, combat, card_source=card)
    _gain_resolved_block(target, block, combat)


@register_effect(CardId.MIND_BLAST)
def mind_blast(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Damage scales with draw pile size."""
    assert target is not None
    owner = _owner(card, combat)
    total_damage = (
        card.effect_vars.get(MIND_BLAST_CALC_BASE_KEY, MIND_BLAST_CALC_BASE)
        + card.effect_vars.get(MIND_BLAST_EXTRA_DAMAGE_KEY, MIND_BLAST_EXTRA_DAMAGE)
        * len(combat.draw_pile)
    )
    damage = calculate_damage(total_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, damage, ValueProp.MOVE, combat, owner)


@register_effect(CardId.OMNISLICE)
def omnislice(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    base_damage = (
        card.base_damage
        if card.base_damage is not None
        else card.effect_vars.get(OMNISLICE_DAMAGE_KEY, OMNISLICE_DAMAGE)
    )
    damage = calculate_damage(base_damage, owner, target, ValueProp.MOVE, combat)
    result = apply_damage(target, damage, ValueProp.MOVE, combat, owner)

    splash_damage = result.total_damage + result.overkill_damage
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


def is_omnislice(card: object) -> bool:
    return getattr(card, "card_id", None) == CardId.OMNISLICE


@register_effect(CardId.PANACHE_CARD)
def panache_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    damage = card.effect_vars.get(PANACHE_DAMAGE_KEY, PANACHE_DAMAGE)
    combat.apply_power_to(_owner(card, combat), PowerId.PANACHE, damage)


@register_effect(CardId.PANIC_BUTTON)
def panic_button(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    turns = card.effect_vars.get(PANIC_BUTTON_TURNS_KEY, PANIC_BUTTON_TURNS)
    combat.apply_power_to(_owner(card, combat), PowerId.NO_BLOCK, turns)


@register_effect(CardId.PREP_TIME)
def prep_time(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("prep_time", 4)
    combat.apply_power_to(_owner(card, combat), PowerId.PREP_TIME, amount)


@register_effect(CardId.PRODUCTION)
def production(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    energy = card.effect_vars.get(PRODUCTION_ENERGY_KEY, PRODUCTION_ENERGY)
    combat.gain_energy(_owner(card, combat), energy)


@register_effect(CardId.PROLONG)
def prolong(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Apply BlockNextTurn — carry current block to next turn
    owner = _owner(card, combat)
    block = owner.block
    if block > 0:
        combat.apply_power_to(owner, PowerId.BLOCK_NEXT_TURN, block)


@register_effect(CardId.PROWESS)
def prowess(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    strength = card.effect_vars.get(PROWESS_STRENGTH_KEY, PROWESS_POWER)
    dexterity = card.effect_vars.get(PROWESS_DEXTERITY_KEY, PROWESS_POWER)
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
    cards = card.effect_vars.get(RESTLESSNESS_CARDS_KEY, RESTLESSNESS_CARDS)
    energy = card.effect_vars.get(RESTLESSNESS_ENERGY_KEY, RESTLESSNESS_ENERGY)
    combat._draw_cards(cards)
    combat.gain_energy(_owner(card, combat), energy)


@register_effect(CardId.SEEKER_STRIKE)
def seeker_strike(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    candidates = list(combat.draw_pile)
    if not candidates:
        return
    combat.stable_shuffle_cards(candidates, combat.combat_card_selection_rng)
    candidates = candidates[:card.effect_vars.get(SEEKER_STRIKE_CARDS_KEY, SEEKER_STRIKE_CARDS)]
    combat.request_card_choice(
        prompt="Choose one of the revealed cards",
        cards=candidates,
        source_pile="draw",
        resolver=combat.move_card_to_hand,
    )


@register_effect(CardId.SHOCKWAVE)
def shockwave(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get(SHOCKWAVE_POWER_KEY, SHOCKWAVE_POWER)
    for enemy in list(combat.hittable_enemies):
        combat.apply_power_to(enemy, PowerId.WEAK, amount)
        combat.apply_power_to(enemy, PowerId.VULNERABLE, amount)


@register_effect(CardId.SPLASH)
def splash(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    owner_state = combat.combat_player_state_for(owner)
    own_character = (owner_state.character_id if owner_state is not None else combat.character_id).lower()
    candidate_ids: list[CardId] = []
    for cfg in ALL_CHARACTERS:
        if cfg.character_id.lower() == own_character:
            continue
        candidate_ids.extend(
            eligible_character_cards(
                cfg.character_id,
                card_type=CardType.ATTACK,
                generation_context="combat",
                is_multiplayer=combat.is_multiplayer,
            )
        )

    if not candidate_ids:
        return

    generated = create_cards_from_ids(
        candidate_ids,
        combat.combat_card_generation_rng,
        SPLASH_ATTACK_CHOICES,
        distinct=True,
    )
    for generated_card in generated:
        if card.upgraded:
            combat.upgrade_card(generated_card)

    def _resolver(selected: CardInstance | None) -> None:
        if selected is None:
            return
        selected.set_temporary_free_this_turn()
        combat.add_generated_card_to_creature_hand(owner, selected)

    combat.request_card_choice(
        prompt=SPLASH_CHOICE_PROMPT,
        cards=generated,
        source_pile=SPLASH_SOURCE_PILE,
        resolver=_resolver,
        allow_skip=True,
        owner=owner,
    )


@register_effect(CardId.STRATAGEM)
def stratagem(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.STRATAGEM, 1)


@register_effect(CardId.TAG_TEAM)
def tag_team(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    combat.apply_power_to(target, PowerId.TAG_TEAM, TAG_TEAM_POWER)


@register_effect(CardId.THE_BOMB_CARD)
def the_bomb_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    turns = card.effect_vars.get(THE_BOMB_TURNS_KEY, THE_BOMB_TURNS)
    damage = card.effect_vars.get(THE_BOMB_DAMAGE_KEY, THE_BOMB_DAMAGE)
    owner = _owner(card, combat)
    existing = owner.powers.get(PowerId.THE_BOMB)
    if existing is not None and hasattr(existing, "add_instance"):
        existing.add_instance(turns, damage)
    else:
        combat.apply_power_to(owner, PowerId.THE_BOMB, turns)
        bomb_power = owner.powers.get(PowerId.THE_BOMB)
        if bomb_power is not None and hasattr(bomb_power, "set_latest_damage"):
            bomb_power.set_latest_damage(damage)


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
    card.combat_vars["_return_before_hand_draw_round"] = combat.round_number + 1


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
        if owner.is_dead:
            break
        hittable = combat.hittable_enemies
        if not hittable:
            break
        t = combat.combat_targets_rng.choice(hittable)
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
    # Anointed.cs OnPlay (lines 22-24): count = MaxCardsInHand - current hand
    # size; take that many RANDOM rare cards (TakeRandom, no replacement) from
    # the draw pile and add them to hand — only enough to fill the free slots.
    from sts2_env.core.constants import MAX_HAND_SIZE

    count = MAX_HAND_SIZE - len(combat.hand)
    if count <= 0:
        return
    rares = [c for c in list(combat.draw_pile) if c.rarity == CardRarity.RARE]
    if not rares:
        return
    combat.stable_shuffle_cards(rares, combat.combat_card_selection_rng)
    for rare in rares[:count]:
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
    combat.stable_shuffle_cards(candidates, combat.shuffle_rng)
    for selected in candidates[:cards]:
        if combat.is_over:
            break
        resolved_target = combat.random_enemy_of(owner) if selected.target_type == TargetType.ANY_ENEMY else None
        combat.auto_play_card(selected, target=resolved_target)


@register_effect(CardId.BOLAS)
def bolas(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    card.combat_vars["_return_before_hand_draw_round"] = combat.round_number + 1


@register_effect(CardId.CALAMITY_CARD)
def calamity_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.CALAMITY, CALAMITY_POWER)


@register_effect(CardId.ENTROPY)
def entropy(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.ENTROPY, 1)


@register_effect(CardId.ETERNAL_ARMOR)
def eternal_armor(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    plating = card.effect_vars.get(ETERNAL_ARMOR_PLATING_KEY, ETERNAL_ARMOR_PLATING)
    combat.apply_power_to(_owner(card, combat), PowerId.PLATING, plating)


@register_effect(CardId.GOLD_AXE)
def gold_axe(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Damage scales with card plays finished this combat across all players."""
    assert target is not None
    owner = _owner(card, combat)
    # GoldAxe.cs CanonicalVars (lines 20-21): the multiplier is
    # CombatManager.Instance.History.CardPlaysFinished.Count() — the combat-wide
    # history, i.e. every player's finished card plays, not just the owner's.
    # The in-flight play is not yet in History when the damage resolves, matching
    # C# (entries are appended after the effect resolves). In single-player the
    # only player is the owner, so behavior there is unchanged.
    finished_plays = sum(
        combat.count_cards_played_this_combat(state.creature)
        for state in combat.combat_player_states
    )
    total_damage = card.effect_vars.get("calc_base", 0) + card.effect_vars.get("extra_damage", 1) * finished_plays
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
    # HiddenGem.cs OnPlay (lines 35-45): candidates from the draw pile exclude
    # Unplayable cards, Curse/Quest (CardType 5/6), and any card whose
    # GetEnchantedReplayCount() >= 1 (already has replay). Attack/Skill/Power
    # (CardType 1-3) are preferred when any exist.
    from sts2_env.cards.enchantments import enchant_play_count_bonus

    def effective_replay_count(c: CardInstance) -> int:
        return c.base_replay_count + enchant_play_count_bonus(c)

    valid_draw_cards = [
        c for c in combat.draw_pile
        if (
            not c.is_unplayable
            and c.card_type not in (CardType.CURSE, CardType.QUEST)
            and effective_replay_count(c) < 1
        )
    ]
    candidates = [
        c for c in valid_draw_cards
        if c.card_type in (CardType.ATTACK, CardType.SKILL, CardType.POWER)
    ]
    if not candidates:
        candidates = valid_draw_cards
    if not candidates:
        return
    selected = combat.combat_card_selection_rng.choice(candidates)
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
            generation_context="combat",
            is_multiplayer=combat.is_multiplayer,
        )
        if card_preview(card_id).cost == 0 and not card_preview(card_id).has_energy_cost_x
    ]
    generated = create_cards_from_ids(
        [card_id for card_id in zero_cost_ids if card_id in set(get_character(combat.character_id).card_pool)],
        combat.combat_card_generation_rng,
        card.effect_vars.get(JACKPOT_CARDS_KEY, JACKPOT_CARDS),
        distinct=False,
    )
    for generated_card in generated:
        if card.upgraded:
            combat.upgrade_card(generated_card)
        combat.add_generated_card_to_creature_hand(_owner(card, combat), generated_card)


@register_effect(CardId.KNOCKDOWN)
def knockdown(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_single(card, combat, target)
    amount = card.effect_vars.get("knockdown", 2)
    combat.apply_power_to(target, PowerId.KNOCKDOWN, amount)


@register_effect(CardId.MASTER_OF_STRATEGY)
def master_of_strategy(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    cards = card.effect_vars.get(MASTER_OF_STRATEGY_CARDS_KEY, MASTER_OF_STRATEGY_CARDS)
    combat._draw_cards(cards)


@register_effect(CardId.MAYHEM_CARD)
def mayhem_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.MAYHEM, MAYHEM_POWER)


@register_effect(CardId.MIMIC)
def mimic(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Gain block scaling with ally block."""
    if target is None:
        return
    owner = _owner(card, combat)
    total_block = (
        card.effect_vars.get(MIMIC_CALC_BASE_KEY, MIMIC_CALC_BASE)
        + card.effect_vars.get(MIMIC_CALC_EXTRA_KEY, MIMIC_CALC_EXTRA) * target.block
    )
    block = calculate_block(total_block, owner, ValueProp.MOVE, combat, card_source=card)
    _gain_resolved_block(owner, block, combat)


@register_effect(CardId.NOSTALGIA_CARD)
def nostalgia_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.NOSTALGIA, NOSTALGIA_POWER)


@register_effect(CardId.RALLY)
def rally(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Rally.cs OnPlay (lines 26-35): iterates CombatState.GetTeammatesOf, which
    # per CombatState.cs:397 "includes the creature itself" — so the caster
    # gains block too. get_teammates_of excludes the owner, so add it here.
    owner = _owner(card, combat)
    teammates = [owner]
    for creature in combat.get_teammates_of(owner):
        if creature is not owner:
            teammates.append(creature)
    for teammate in [
        creature
        for creature in teammates
        if creature is not None and creature.is_alive and getattr(creature, "is_player", False)
    ]:
        block = calculate_block(card.base_block, teammate, ValueProp.MOVE, combat, card_source=card)
        _gain_resolved_block(teammate, block, combat)


@register_effect(CardId.REND)
def rend(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Scales with non-temporary debuffs on the target."""
    assert target is not None
    debuffs = sum(
        1
        for power_id, power in target.powers.items()
        if (cls := get_power_class(power_id)) is not None
        and _power_type_for_amount(cls, power.amount) == PowerType.DEBUFF
        and not getattr(power, "is_temporary", False)
    )
    base = card.effect_vars.get(REND_CALC_BASE_KEY, card.base_damage or REND_DAMAGE)
    extra = card.effect_vars.get(REND_EXTRA_DAMAGE_KEY, REND_EXTRA_DAMAGE)
    total_damage = base + extra * debuffs
    owner = _owner(card, combat)
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
        base_damage=DRAMATIC_ENTRANCE_UPGRADED_DAMAGE if upgraded else DRAMATIC_ENTRANCE_DAMAGE,
        upgraded=upgraded,
        keywords=frozenset({"exhaust", "innate"}),
        instance_id=_get_next_id(),
    )


def make_finesse(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.FINESSE, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        base_block=FINESSE_UPGRADED_BLOCK if upgraded else FINESSE_BLOCK,
        upgraded=upgraded,
        effect_vars={FINESSE_CARDS_KEY: FINESSE_CARDS},
        instance_id=_get_next_id(),
    )


def make_flash_of_steel(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.FLASH_OF_STEEL, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=FLASH_OF_STEEL_UPGRADED_DAMAGE if upgraded else FLASH_OF_STEEL_DAMAGE,
        upgraded=upgraded,
        effect_vars={FLASH_OF_STEEL_CARDS_KEY: FLASH_OF_STEEL_CARDS},
        instance_id=_get_next_id(),
    )


def make_shockwave(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SHOCKWAVE, cost=2, card_type=CardType.SKILL,
        target_type=TargetType.ALL_ENEMIES, rarity=CardRarity.UNCOMMON,
        upgraded=upgraded, keywords=frozenset({"exhaust"}),
        effect_vars={SHOCKWAVE_POWER_KEY: SHOCKWAVE_UPGRADED_POWER if upgraded else SHOCKWAVE_POWER},
        instance_id=_get_next_id(),
    )


def make_master_of_strategy(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.MASTER_OF_STRATEGY, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        upgraded=upgraded, keywords=frozenset({"exhaust"}),
        effect_vars={
            MASTER_OF_STRATEGY_CARDS_KEY: (
                MASTER_OF_STRATEGY_UPGRADED_CARDS
                if upgraded
                else MASTER_OF_STRATEGY_CARDS
            )
        },
        instance_id=_get_next_id(),
    )


def make_omnislice(upgraded: bool = False) -> CardInstance:
    damage = OMNISLICE_UPGRADED_DAMAGE if upgraded else OMNISLICE_DAMAGE
    return CardInstance(
        card_id=CardId.OMNISLICE, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=damage, upgraded=upgraded,
        effect_vars={OMNISLICE_DAMAGE_KEY: damage},
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
        base_block=PANIC_BUTTON_UPGRADED_BLOCK if upgraded else PANIC_BUTTON_BLOCK,
        upgraded=upgraded,
        keywords=frozenset({"exhaust"}),
        effect_vars={PANIC_BUTTON_TURNS_KEY: PANIC_BUTTON_TURNS},
        instance_id=_get_next_id(),
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
        base_damage=ULTIMATE_STRIKE_UPGRADED_DAMAGE if upgraded else ULTIMATE_STRIKE_DAMAGE,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_ultimate_defend(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.ULTIMATE_DEFEND, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        base_block=ULTIMATE_DEFEND_UPGRADED_BLOCK if upgraded else ULTIMATE_DEFEND_BLOCK,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_believe_in_you(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BELIEVE_IN_YOU, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.ANY_ALLY, rarity=CardRarity.UNCOMMON,
        effect_vars={
            BELIEVE_IN_YOU_ENERGY_KEY: (
                BELIEVE_IN_YOU_UPGRADED_ENERGY if upgraded else BELIEVE_IN_YOU_ENERGY
            )
        },
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_catastrophe(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.CATASTROPHE, cost=2, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={
            CATASTROPHE_CARDS_KEY: (
                CATASTROPHE_UPGRADED_CARDS if upgraded else CATASTROPHE_CARDS
            )
        },
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_dark_shackles(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.DARK_SHACKLES, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        keywords=frozenset({"exhaust"}),
        effect_vars={
            DARK_SHACKLES_STRENGTH_LOSS_KEY: (
                DARK_SHACKLES_UPGRADED_STRENGTH_LOSS
                if upgraded
                else DARK_SHACKLES_STRENGTH_LOSS
            )
        },
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_equilibrium(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.EQUILIBRIUM, cost=2, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        base_block=EQUILIBRIUM_UPGRADED_BLOCK if upgraded else EQUILIBRIUM_BLOCK,
        effect_vars={"equilibrium": EQUILIBRIUM_POWER},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_fasten(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.FASTEN, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={
            FASTEN_EXTRA_BLOCK_KEY: (
                FASTEN_UPGRADED_EXTRA_BLOCK if upgraded else FASTEN_EXTRA_BLOCK
            )
        },
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_intercept_card(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.INTERCEPT_CARD, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.ANY_ALLY, rarity=CardRarity.UNCOMMON,
        base_block=INTERCEPT_UPGRADED_BLOCK if upgraded else INTERCEPT_BLOCK,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_jack_of_all_trades(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.JACK_OF_ALL_TRADES, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        keywords=frozenset({"exhaust"}),
        effect_vars={
            JACK_OF_ALL_TRADES_CARDS_KEY: (
                JACK_OF_ALL_TRADES_UPGRADED_CARDS if upgraded else JACK_OF_ALL_TRADES_CARDS
            )
        },
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_lift(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.LIFT, cost=1, card_type=CardType.SKILL,
        target_type=TargetType.ANY_ALLY, rarity=CardRarity.UNCOMMON,
        base_block=LIFT_UPGRADED_BLOCK if upgraded else LIFT_BLOCK,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_production(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.PRODUCTION, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        keywords=frozenset({"exhaust"}),
        effect_vars={
            PRODUCTION_ENERGY_KEY: (
                PRODUCTION_UPGRADED_ENERGY if upgraded else PRODUCTION_ENERGY
            )
        },
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_prolong(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.PROLONG, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        keywords=frozenset() if upgraded else frozenset({"exhaust"}),
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_prowess(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.PROWESS, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={
            PROWESS_STRENGTH_KEY: PROWESS_UPGRADED_POWER if upgraded else PROWESS_POWER,
            PROWESS_DEXTERITY_KEY: PROWESS_UPGRADED_POWER if upgraded else PROWESS_POWER,
        },
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_restlessness(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.RESTLESSNESS, cost=0, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        keywords=frozenset({"retain"}),
        effect_vars={
            RESTLESSNESS_CARDS_KEY: (
                RESTLESSNESS_UPGRADED_CARDS if upgraded else RESTLESSNESS_CARDS
            ),
            RESTLESSNESS_ENERGY_KEY: (
                RESTLESSNESS_UPGRADED_ENERGY if upgraded else RESTLESSNESS_ENERGY
            ),
        },
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_seeker_strike(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SEEKER_STRIKE, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=SEEKER_STRIKE_UPGRADED_DAMAGE if upgraded else SEEKER_STRIKE_DAMAGE,
        effect_vars={SEEKER_STRIKE_CARDS_KEY: SEEKER_STRIKE_CARDS},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_stratagem(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.STRATAGEM,
        cost=STRATAGEM_UPGRADED_COST if upgraded else STRATAGEM_COST,
        card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_tag_team(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.TAG_TEAM, cost=2, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=TAG_TEAM_UPGRADED_DAMAGE if upgraded else TAG_TEAM_DAMAGE,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_the_bomb_card(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.THE_BOMB_CARD, cost=THE_BOMB_COST, card_type=CardType.SKILL,
        target_type=TargetType.SELF, rarity=CardRarity.UNCOMMON,
        effect_vars={
            THE_BOMB_TURNS_KEY: THE_BOMB_TURNS,
            THE_BOMB_DAMAGE_KEY: THE_BOMB_UPGRADED_DAMAGE if upgraded else THE_BOMB_DAMAGE,
        },
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_thrumming_hatchet(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.THRUMMING_HATCHET, cost=1, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.UNCOMMON,
        base_damage=THRUMMING_HATCHET_UPGRADED_DAMAGE if upgraded else THRUMMING_HATCHET_DAMAGE,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_bolas(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BOLAS, cost=0, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.RARE,
        base_damage=BOLAS_UPGRADED_DAMAGE if upgraded else BOLAS_DAMAGE,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_calamity_card(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.CALAMITY_CARD,
        cost=CALAMITY_UPGRADED_COST if upgraded else CALAMITY_COST,
        card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_entropy(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.ENTROPY, cost=1, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        keywords=frozenset({"innate"}) if upgraded else frozenset(),
        effect_vars={ENTROPY_CARDS_KEY: ENTROPY_CARDS},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_eternal_armor(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.ETERNAL_ARMOR, cost=3, card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        effect_vars={
            ETERNAL_ARMOR_PLATING_KEY: (
                ETERNAL_ARMOR_UPGRADED_PLATING if upgraded else ETERNAL_ARMOR_PLATING
            )
        },
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_jackpot(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.JACKPOT, cost=3, card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY, rarity=CardRarity.RARE,
        base_damage=JACKPOT_UPGRADED_DAMAGE if upgraded else JACKPOT_DAMAGE,
        effect_vars={JACKPOT_CARDS_KEY: JACKPOT_CARDS},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_mayhem_card(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.MAYHEM_CARD,
        cost=MAYHEM_UPGRADED_COST if upgraded else MAYHEM_COST,
        card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


def make_nostalgia_card(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.NOSTALGIA_CARD,
        cost=NOSTALGIA_UPGRADED_COST if upgraded else NOSTALGIA_COST,
        card_type=CardType.POWER,
        target_type=TargetType.SELF, rarity=CardRarity.RARE,
        upgraded=upgraded,
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
    card.effect_vars[COORDINATE_STRENGTH_KEY] = (
        COORDINATE_UPGRADED_STRENGTH if upgraded else COORDINATE_STRENGTH
    )
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
    card.base_damage = MIND_BLAST_CALC_BASE
    card.effect_vars[MIND_BLAST_CALC_BASE_KEY] = MIND_BLAST_CALC_BASE
    card.effect_vars[MIND_BLAST_EXTRA_DAMAGE_KEY] = MIND_BLAST_EXTRA_DAMAGE
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
    card.base_block = MIMIC_CALC_BASE
    card.effect_vars[MIMIC_CALC_BASE_KEY] = MIMIC_CALC_BASE
    card.effect_vars[MIMIC_CALC_EXTRA_KEY] = MIMIC_CALC_EXTRA
    return card


def make_rally(upgraded: bool = False) -> CardInstance:
    card = _make_reference_factory_card(CardId.RALLY, upgraded=upgraded)
    card.base_block = RALLY_UPGRADED_BLOCK if upgraded else RALLY_BLOCK
    return card


def make_rend(upgraded: bool = False) -> CardInstance:
    card = _make_reference_factory_card(CardId.REND, upgraded=upgraded)
    card.base_damage = REND_UPGRADED_DAMAGE if upgraded else REND_DAMAGE
    card.effect_vars[REND_CALC_BASE_KEY] = REND_UPGRADED_DAMAGE if upgraded else REND_DAMAGE
    card.effect_vars[REND_EXTRA_DAMAGE_KEY] = (
        REND_UPGRADED_EXTRA_DAMAGE if upgraded else REND_EXTRA_DAMAGE
    )
    return card


def make_salvo(upgraded: bool = False) -> CardInstance:
    card = _make_reference_factory_card(CardId.SALVO, upgraded=upgraded)
    card.base_damage = SALVO_UPGRADED_DAMAGE if upgraded else SALVO_DAMAGE
    return card


def make_the_gambit(upgraded: bool = False) -> CardInstance:
    card = _make_reference_factory_card(CardId.THE_GAMBIT, upgraded=upgraded)
    card.base_block = THE_GAMBIT_UPGRADED_BLOCK if upgraded else THE_GAMBIT_BLOCK
    return card


def make_volley(upgraded: bool = False) -> CardInstance:
    card = _make_reference_factory_card(CardId.VOLLEY, upgraded=upgraded)
    card.base_damage = VOLLEY_UPGRADED_DAMAGE if upgraded else VOLLEY_DAMAGE
    return card
