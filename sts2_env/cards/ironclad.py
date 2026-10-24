"""All 87 Ironclad card effects and factories.

Covers: 3 Basic, 21 Common, 32 Uncommon, 29 Rare, 2 Ancient cards.
Each card has a registered effect function and a make_*() factory.
"""

from __future__ import annotations

from sts2_env.cards.base import CardInstance, _get_next_id, increase_base_damage
from sts2_env.cards.registry import (
    register_after_card_entered_combat_hook,
    register_after_auto_post_play_hook,
    register_after_card_exhausted_hook,
    register_before_card_played_hook,
    register_effect,
    register_self_mutating_damage,
)
from sts2_env.core.enums import (
    CardId, CardTag, CardType, TargetType, CardRarity, ValueProp, PowerId, CombatSide,
)
from sts2_env.core.damage import calculate_damage, apply_damage, calculate_block
from sts2_env.core.hooks import modify_damage
from sts2_env.core.creature import Creature
from sts2_env.core.combat import CombatState
from sts2_env.core.constants import MAX_HAND_SIZE


# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

def _owner(card: CardInstance, combat: CombatState) -> Creature:
    return (
        getattr(card, "owner", None)
        or getattr(getattr(combat, "active_card_source", None), "owner", None)
        or combat.primary_player
    )


def _deal_damage_to_target(
    card: CardInstance, combat: CombatState, target: Creature,
) -> None:
    """Standard single-target damage."""
    owner = _owner(card, combat)
    if owner.is_dead:
        return
    damage = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, damage, ValueProp.MOVE, combat, owner)


def _deal_damage_all_enemies(
    card: CardInstance, combat: CombatState,
) -> None:
    """Deal card.base_damage to every hittable enemy."""
    owner = _owner(card, combat)
    if owner.is_dead:
        return
    for enemy in list(combat.hittable_enemies):
        damage = calculate_damage(card.base_damage, owner, enemy, ValueProp.MOVE, combat)
        apply_damage(enemy, damage, ValueProp.MOVE, combat, owner)


def _calculate_untargeted_card_damage(
    card: CardInstance,
    owner: Creature,
    combat: CombatState,
) -> int:
    previous_card_source = combat._active_card_source
    combat._active_card_source = card
    try:
        return modify_damage(card.base_damage or 0, owner, None, ValueProp.MOVE, combat, card_source=card)
    finally:
        combat._active_card_source = previous_card_source


def _gain_block(card: CardInstance, combat: CombatState) -> None:
    """Gain block equal to card.base_block."""
    owner = _owner(card, combat)
    block = calculate_block(card.base_block, owner, ValueProp.MOVE, combat, card_source=card)
    before = owner.block
    owner.gain_block(block)
    gained = owner.block - before
    if gained > 0:
        from sts2_env.core.hooks import fire_after_block_gained

        fire_after_block_gained(owner, gained, combat, ValueProp.MOVE, combat.active_card_play_token)

def _self_hp_loss(card: CardInstance, combat: CombatState, amount: int) -> None:
    """Non-attack (unblockable, unpowered) self-damage."""
    owner = _owner(card, combat)
    apply_damage(
        owner, amount,
        ValueProp.UNBLOCKABLE | ValueProp.UNPOWERED | ValueProp.MOVE,
        combat, owner,
    )


def _draw_cards(combat: CombatState, count: int, owner: Creature | None = None) -> None:
    combat.draw_cards(owner or combat.player, count)


# ========================================================================
# BASIC (3)
# ========================================================================

# --- Strike Ironclad ---
@register_effect(CardId.STRIKE_IRONCLAD)
def strike_ironclad(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_to_target(card, combat, target)


def make_strike_ironclad(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.STRIKE_IRONCLAD,
        cost=1,
        card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.BASIC,
        base_damage=9 if upgraded else 6,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Defend Ironclad ---
@register_effect(CardId.DEFEND_IRONCLAD)
def defend_ironclad(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)


def make_defend_ironclad(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.DEFEND_IRONCLAD,
        cost=1,
        card_type=CardType.SKILL,
        target_type=TargetType.SELF,
        rarity=CardRarity.BASIC,
        base_block=8 if upgraded else 5,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Bash ---
@register_effect(CardId.BASH)
def bash(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_to_target(card, combat, target)
    vuln = card.effect_vars.get("vulnerable", 2)
    combat.apply_power_to(target, PowerId.VULNERABLE, vuln)


def make_bash(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BASH,
        cost=2,
        card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.BASIC,
        base_damage=10 if upgraded else 8,
        effect_vars={"vulnerable": 3 if upgraded else 2},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# ========================================================================
# COMMON (21)
# ========================================================================

# --- Anger ---
@register_effect(CardId.ANGER)
def anger(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_to_target(card, combat, target)
    # Add a copy of this card to discard pile
    copy = CardInstance(
        card_id=CardId.ANGER,
        cost=card.cost,
        card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.COMMON,
        base_damage=card.base_damage,
        upgraded=card.upgraded,
        instance_id=_get_next_id(),
    )
    combat.add_card_to_discard(copy, owner=_owner(card, combat), added_by_player=True)


def make_anger(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.ANGER,
        cost=0,
        card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.COMMON,
        base_damage=8 if upgraded else 6,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Armaments ---
@register_effect(CardId.ARMAMENTS)
def armaments(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    candidates = [c for c in combat.hand if not c.upgraded]
    if not candidates:
        return
    if card.upgraded:
        for candidate in candidates:
            combat.upgrade_card(candidate)
        return
    combat.request_card_choice(
        prompt="Choose a hand card to upgrade",
        cards=candidates,
        source_pile="hand",
        resolver=combat.upgrade_card,
    )


def make_armaments(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.ARMAMENTS,
        cost=1,
        card_type=CardType.SKILL,
        target_type=TargetType.SELF,
        rarity=CardRarity.COMMON,
        base_block=5,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Blood Wall ---
@register_effect(CardId.BLOOD_WALL)
def blood_wall(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    hp_loss = card.effect_vars.get("hp_loss", 2)
    _self_hp_loss(card, combat, hp_loss)
    if combat.is_over:
        return
    _gain_block(card, combat)


def make_blood_wall(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BLOOD_WALL,
        cost=2,
        card_type=CardType.SKILL,
        target_type=TargetType.SELF,
        rarity=CardRarity.COMMON,
        base_block=20 if upgraded else 16,
        effect_vars={"hp_loss": 2},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Bloodletting ---
@register_effect(CardId.BLOODLETTING)
def bloodletting(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    hp_loss = card.effect_vars.get("hp_loss", 3)
    _self_hp_loss(card, combat, hp_loss)
    energy = card.effect_vars.get("energy", 2)
    combat.gain_energy(owner, energy)


def make_bloodletting(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BLOODLETTING,
        cost=0,
        card_type=CardType.SKILL,
        target_type=TargetType.SELF,
        rarity=CardRarity.COMMON,
        effect_vars={"hp_loss": 3, "energy": 3 if upgraded else 2},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Body Slam ---
@register_effect(CardId.BODY_SLAM)
def body_slam(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    # Damage = player's current block
    owner = _owner(card, combat)
    base = owner.block
    damage = calculate_damage(base, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, damage, ValueProp.MOVE, combat, owner)


def make_body_slam(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BODY_SLAM,
        cost=0 if upgraded else 1,
        card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.COMMON,
        base_damage=0,  # Dynamic: uses current block
        effect_vars={"calc_base": 0, "extra_damage": 1},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Breakthrough ---
@register_effect(CardId.BREAKTHROUGH)
def breakthrough(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    hp_loss = card.effect_vars.get("hp_loss", 1)
    _self_hp_loss(card, combat, hp_loss)
    _deal_damage_all_enemies(card, combat)


def make_breakthrough(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BREAKTHROUGH,
        cost=1,
        card_type=CardType.ATTACK,
        target_type=TargetType.ALL_ENEMIES,
        rarity=CardRarity.COMMON,
        base_damage=13 if upgraded else 9,
        effect_vars={"hp_loss": 1},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Cinder ---
@register_effect(CardId.CINDER)
def cinder(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_to_target(card, combat, target)
    # Cinder.cs OnPlay: exhaust one random card from the HAND
    # (PileType.Hand + Rng.CombatCardSelection.NextItem; no-op if hand is empty).
    if not combat.hand:
        return
    to_exhaust = combat.combat_card_selection_rng.choice(list(combat.hand))
    combat.exhaust_card(to_exhaust)


def make_cinder(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.CINDER,
        cost=2,
        card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.COMMON,
        # Cinder.cs: DamageVar(18); OnUpgrade: damage +6.
        base_damage=24 if upgraded else 18,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Havoc ---
@register_effect(CardId.HAVOC)
def havoc(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.auto_play_from_draw(_owner(card, combat), 1, force_exhaust=True)


def make_havoc(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.HAVOC,
        cost=0 if upgraded else 1,
        card_type=CardType.SKILL,
        target_type=TargetType.SELF,
        rarity=CardRarity.COMMON,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Headbutt ---
@register_effect(CardId.HEADBUTT)
def headbutt(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_to_target(card, combat, target)
    if not combat.discard_pile:
        return
    combat.request_card_choice(
        prompt="Choose a discard card to put on top of draw pile",
        cards=list(combat.discard_pile),
        source_pile="discard",
        resolver=lambda selected: combat.insert_card_into_draw_pile(selected, random_position=False),
    )


def make_headbutt(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.HEADBUTT,
        cost=1,
        card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.COMMON,
        base_damage=12 if upgraded else 9,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Iron Wave ---
@register_effect(CardId.IRON_WAVE)
def iron_wave(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _gain_block(card, combat)
    _deal_damage_to_target(card, combat, target)


def make_iron_wave(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.IRON_WAVE,
        cost=1,
        card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.COMMON,
        base_damage=7 if upgraded else 5,
        base_block=7 if upgraded else 5,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Molten Fist ---
@register_effect(CardId.MOLTEN_FIST)
def molten_fist(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_to_target(card, combat, target)
    if target.is_alive:
        vulnerable = target.get_power_amount(PowerId.VULNERABLE)
        if vulnerable > 0:
            combat.apply_power_to(target, PowerId.VULNERABLE, vulnerable)


def make_molten_fist(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.MOLTEN_FIST,
        cost=1,
        card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.COMMON,
        base_damage=14 if upgraded else 10,
        keywords=frozenset({"exhaust"}),
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Perfected Strike ---
@register_effect(CardId.PERFECTED_STRIKE)
def perfected_strike(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    calc_base = card.effect_vars.get("calc_base", 6)
    extra_per = card.effect_vars.get("extra_damage", 2)
    # Count Strike cards in all owner piles, including play pile.
    owner = _owner(card, combat)
    owner_state = combat.combat_player_state_for(owner)
    if owner_state is None:
        piles = [combat.hand, combat.draw_pile, combat.discard_pile, combat.exhaust_pile, combat.play_pile]
    else:
        piles = [owner_state.hand, owner_state.draw, owner_state.discard, owner_state.exhaust, owner_state.play]
    strike_count = 0
    for pile in piles:
        for c in pile:
            if CardTag.STRIKE in c.tags:
                strike_count += 1
    base = calc_base + extra_per * strike_count
    damage = calculate_damage(base, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, damage, ValueProp.MOVE, combat, owner)


def make_perfected_strike(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.PERFECTED_STRIKE,
        cost=2,
        card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.COMMON,
        base_damage=6,  # Dynamic
        effect_vars={
            "calc_base": 6,
            "extra_damage": 3 if upgraded else 2,
        },
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- NotYet (game v0.107.1, Ironclad rare) ---

NOT_YET_HEAL = 10


def make_not_yet(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.NOT_YET,
        cost=2,
        card_type=CardType.SKILL,
        target_type=TargetType.SELF,
        rarity=CardRarity.RARE,
        keywords=frozenset({"exhaust"}),
        effect_vars={"heal": NOT_YET_HEAL + (3 if upgraded else 0)},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


@register_effect(CardId.NOT_YET)
def not_yet_effect(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    """Heal the owner (C# NotYet.OnPlay -> CreatureCmd.Heal)."""
    owner = _owner(card, combat)
    owner.heal(card.effect_vars.get("heal", NOT_YET_HEAL))


# --- Pommel Strike ---
@register_effect(CardId.POMMEL_STRIKE)
def pommel_strike(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_to_target(card, combat, target)
    draw = card.effect_vars.get("cards", 1)
    _draw_cards(combat, draw)


def make_pommel_strike(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.POMMEL_STRIKE,
        cost=1,
        card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.COMMON,
        base_damage=10 if upgraded else 9,
        effect_vars={"cards": 2 if upgraded else 1},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Setup Strike ---
@register_effect(CardId.SETUP_STRIKE_CARD)
def setup_strike(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_to_target(card, combat, target)
    strength = card.effect_vars.get("strength", 2)
    combat.apply_power_to(_owner(card, combat), PowerId.SETUP_STRIKE, strength)


def make_setup_strike(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SETUP_STRIKE_CARD,
        cost=1,
        card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.COMMON,
        base_damage=9 if upgraded else 7,
        effect_vars={"strength": 3 if upgraded else 2},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Shrug It Off ---
@register_effect(CardId.SHRUG_IT_OFF)
def shrug_it_off(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    _draw_cards(combat, card.effect_vars.get("cards", 1))


def make_shrug_it_off(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SHRUG_IT_OFF,
        cost=1,
        card_type=CardType.SKILL,
        target_type=TargetType.SELF,
        rarity=CardRarity.COMMON,
        base_block=11 if upgraded else 8,
        effect_vars={"cards": 1},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Sword Boomerang ---
@register_effect(CardId.SWORD_BOOMERANG)
def sword_boomerang(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    hits = card.effect_vars.get("repeat", 3)
    for _ in range(hits):
        if owner.is_dead:
            break
        alive = combat.hittable_enemies
        if not alive:
            break
        rand_target = combat.combat_targets_rng.choice(alive)
        damage = calculate_damage(card.base_damage, owner, rand_target, ValueProp.MOVE, combat)
        apply_damage(rand_target, damage, ValueProp.MOVE, combat, owner)


def make_sword_boomerang(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SWORD_BOOMERANG,
        cost=1,
        card_type=CardType.ATTACK,
        target_type=TargetType.RANDOM_ENEMY,
        rarity=CardRarity.COMMON,
        base_damage=3,
        effect_vars={"repeat": 4 if upgraded else 3},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Thunderclap ---
@register_effect(CardId.THUNDERCLAP)
def thunderclap(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    vuln = card.effect_vars.get("vulnerable", 1)
    owner = _owner(card, combat)
    for enemy in list(combat.hittable_enemies):
        damage = calculate_damage(card.base_damage, owner, enemy, ValueProp.MOVE, combat)
        apply_damage(enemy, damage, ValueProp.MOVE, combat, owner)
    for enemy in list(combat.hittable_enemies):
        combat.apply_power_to(enemy, PowerId.VULNERABLE, vuln)


def make_thunderclap(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.THUNDERCLAP,
        cost=1,
        card_type=CardType.ATTACK,
        target_type=TargetType.ALL_ENEMIES,
        rarity=CardRarity.COMMON,
        base_damage=7 if upgraded else 4,
        effect_vars={"vulnerable": 1},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Tremble ---
@register_effect(CardId.TREMBLE)
def tremble(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    vuln = card.effect_vars.get("vulnerable", 2)
    combat.apply_power_to(target, PowerId.VULNERABLE, vuln)


def make_tremble(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.TREMBLE,
        cost=1,
        card_type=CardType.SKILL,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.COMMON,
        # Tremble.cs: PowerVar<VulnerablePower>(3); OnUpgrade: +1.
        # CanonicalKeywords = {Exhaust}; OnUpgrade does not remove it, so Exhaust stays.
        keywords=frozenset({"exhaust"}),
        effect_vars={"vulnerable": 4 if upgraded else 3},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- True Grit ---
@register_effect(CardId.TRUE_GRIT)
def true_grit(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    if not combat.hand:
        return
    if card.upgraded:
        combat.request_card_choice(
            prompt="Choose a hand card to exhaust",
            cards=list(combat.hand),
            source_pile="hand",
            resolver=combat.exhaust_card,
        )
        return
    combat.exhaust_card(combat.combat_card_selection_rng.choice(list(combat.hand)))


def make_true_grit(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.TRUE_GRIT,
        cost=1,
        card_type=CardType.SKILL,
        target_type=TargetType.SELF,
        rarity=CardRarity.COMMON,
        base_block=9 if upgraded else 7,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Twin Strike ---
@register_effect(CardId.TWIN_STRIKE)
def twin_strike(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    for _ in range(2):
        if owner.is_dead or target.is_dead:
            break
        damage = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
        apply_damage(target, damage, ValueProp.MOVE, combat, owner)


def make_twin_strike(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.TWIN_STRIKE,
        cost=1,
        card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.COMMON,
        base_damage=7 if upgraded else 5,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# ========================================================================
# UNCOMMON (32)
# ========================================================================

# --- Ashen Strike ---
@register_effect(CardId.ASHEN_STRIKE)
def ashen_strike(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    calc_base = card.effect_vars.get("calc_base", 6)
    extra = card.effect_vars.get("extra_damage", 3)
    exhaust_count = len(combat.exhaust_pile)
    base = calc_base + extra * exhaust_count
    owner = _owner(card, combat)
    damage = calculate_damage(base, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, damage, ValueProp.MOVE, combat, owner)


def make_ashen_strike(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.ASHEN_STRIKE,
        cost=1,
        card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.UNCOMMON,
        base_damage=6,  # Dynamic
        effect_vars={
            "calc_base": 6,
            "extra_damage": 4 if upgraded else 3,
        },
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Battle Trance ---
@register_effect(CardId.BATTLE_TRANCE)
def battle_trance(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    draw = card.effect_vars.get("cards", 3)
    _draw_cards(combat, draw)
    combat.apply_power_to(_owner(card, combat), PowerId.NO_DRAW, 1)


def make_battle_trance(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BATTLE_TRANCE,
        cost=0,
        card_type=CardType.SKILL,
        target_type=TargetType.SELF,
        rarity=CardRarity.UNCOMMON,
        effect_vars={"cards": 4 if upgraded else 3},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Bludgeon ---
@register_effect(CardId.BLUDGEON)
def bludgeon(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_to_target(card, combat, target)


def make_bludgeon(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BLUDGEON,
        cost=3,
        card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.UNCOMMON,
        base_damage=42 if upgraded else 32,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Bully ---
@register_effect(CardId.BULLY)
def bully(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    calc_base = card.effect_vars.get("calc_base", 4)
    extra = card.effect_vars.get("extra_damage", 2)
    vuln_stacks = target.get_power_amount(PowerId.VULNERABLE)
    base = calc_base + extra * vuln_stacks
    owner = _owner(card, combat)
    damage = calculate_damage(base, owner, target, ValueProp.MOVE, combat)
    apply_damage(target, damage, ValueProp.MOVE, combat, owner)


def make_bully(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BULLY,
        cost=0,
        card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.UNCOMMON,
        base_damage=4,  # Dynamic
        effect_vars={
            "calc_base": 4,
            "extra_damage": 3 if upgraded else 2,
        },
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Burning Pact ---
@register_effect(CardId.BURNING_PACT)
def burning_pact(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    if not combat.hand:
        _draw_cards(combat, card.effect_vars.get("cards", 2), owner)
        return

    def _resolver(selected: CardInstance | None) -> None:
        if selected is not None:
            combat.exhaust_card(selected)
        _draw_cards(combat, card.effect_vars.get("cards", 2), owner)

    combat.request_card_choice(
        prompt="Choose a hand card to exhaust",
        cards=list(combat.hand),
        source_pile="hand",
        resolver=_resolver,
    )


def make_burning_pact(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BURNING_PACT,
        cost=1,
        card_type=CardType.SKILL,
        target_type=TargetType.SELF,
        rarity=CardRarity.UNCOMMON,
        effect_vars={"cards": 3 if upgraded else 2},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Demonic Shield ---
@register_effect(CardId.DEMONIC_SHIELD)
def demonic_shield(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    hp_loss = card.effect_vars.get("hp_loss", 1)
    _self_hp_loss(card, combat, hp_loss)
    if combat.is_over:
        return
    block = calculate_block(_owner(card, combat).block, target, ValueProp.MOVE, combat, card_source=card)
    before = target.block
    target.gain_block(block)
    gained = target.block - before
    if gained > 0:
        from sts2_env.core.hooks import fire_after_block_gained

        fire_after_block_gained(target, gained, combat, ValueProp.MOVE, combat.active_card_play_token)


def make_demonic_shield(upgraded: bool = False) -> CardInstance:
    kw = frozenset() if upgraded else frozenset({"exhaust"})
    return CardInstance(
        card_id=CardId.DEMONIC_SHIELD,
        cost=0,
        card_type=CardType.SKILL,
        target_type=TargetType.ANY_ALLY,
        rarity=CardRarity.UNCOMMON,
        effect_vars={"hp_loss": 1, "calc_base": 0, "calc_extra": 1},
        keywords=kw,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Dismantle ---
@register_effect(CardId.DISMANTLE)
def dismantle(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    hits = 2 if target.has_power(PowerId.VULNERABLE) else 1
    for _ in range(hits):
        if owner.is_dead or target.is_dead:
            break
        damage = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
        apply_damage(target, damage, ValueProp.MOVE, combat, owner)


def make_dismantle(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.DISMANTLE,
        cost=1,
        card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.UNCOMMON,
        base_damage=10 if upgraded else 8,
        effect_vars={"hits": 2},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Dominate ---
@register_effect(CardId.DOMINATE)
def dominate(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    # Dominate.cs OnPlay: apply Vulnerable (PowerVar, 1 / 2 upgraded) to the target,
    # then gain Strength equal to the target's total Vulnerable AFTER the application.
    vulnerable = card.effect_vars.get("vulnerable", 1)
    combat.apply_power_to(target, PowerId.VULNERABLE, vulnerable)
    total_vulnerable = target.get_power_amount(PowerId.VULNERABLE)
    if total_vulnerable > 0:
        combat.apply_power_to(_owner(card, combat), PowerId.STRENGTH, total_vulnerable)


def make_dominate(upgraded: bool = False) -> CardInstance:
    # Dominate.cs: PowerVar<VulnerablePower>(1), DynamicVar("StrengthPerVulnerable", 1);
    # CanonicalKeywords = {Exhaust} and OnUpgrade only upgrades Vulnerable (+1),
    # so the card keeps Exhaust when upgraded.
    kw = frozenset({"exhaust"})
    return CardInstance(
        card_id=CardId.DOMINATE,
        cost=1,
        card_type=CardType.SKILL,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.UNCOMMON,
        effect_vars={"vulnerable": 2 if upgraded else 1, "strength_per_vulnerable": 1},
        keywords=kw,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Drum of Battle ---
@register_effect(CardId.DRUM_OF_BATTLE_CARD)
def drum_of_battle(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # DrumOfBattle.cs OnPlay: draw Cards (2 in both upgrade states).
    # The C# card's energy gain happens in AfterCardExhausted (see the
    # register_after_card_exhausted_hook below), not OnPlay. No power is
    # applied (no C# DrumOfBattlePower exists).
    draw = card.effect_vars.get("cards", 2)
    _draw_cards(combat, draw)


@register_after_card_exhausted_hook(CardId.DRUM_OF_BATTLE_CARD)
def drum_of_battle_after_card_exhausted(
    card: CardInstance, exhausted_card: CardInstance, combat: CombatState
) -> None:
    # DrumOfBattle.cs AfterCardExhausted: when THIS card is exhausted, gain its
    # Energy var (2, or 3 upgraded) per GeneratePlayCount (enchanted replay
    # count + 1, modified by ModifyCardPlayCount hooks such as Echo Form).
    if exhausted_card is not card:
        return
    from sts2_env.core.hooks import modify_card_play_count
    from sts2_env.cards.enchantments import enchant_play_count_bonus

    play_count = 1 + enchant_play_count_bonus(card)
    play_count = modify_card_play_count(play_count, card, combat)
    energy = card.effect_vars.get("energy", 2)
    for _ in range(play_count):
        combat.gain_energy(_owner(card, combat), energy)


def make_drum_of_battle(upgraded: bool = False) -> CardInstance:
    # DrumOfBattle.cs: CardsVar(2), EnergyVar(2); OnUpgrade: energy +1 (cards stay 2).
    return CardInstance(
        card_id=CardId.DRUM_OF_BATTLE_CARD,
        cost=1,
        card_type=CardType.SKILL,
        target_type=TargetType.SELF,
        rarity=CardRarity.UNCOMMON,
        effect_vars={"cards": 2, "energy": 3 if upgraded else 2},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Evil Eye ---
@register_effect(CardId.EVIL_EYE)
def evil_eye(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    gains = 2 if combat.was_card_exhausted_this_turn(_owner(card, combat)) else 1
    for _ in range(gains):
        if combat.is_over:
            return
        _gain_block(card, combat)


def make_evil_eye(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.EVIL_EYE,
        cost=1,
        card_type=CardType.SKILL,
        target_type=TargetType.SELF,
        rarity=CardRarity.UNCOMMON,
        base_block=11 if upgraded else 8,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Expect a Fight ---
@register_effect(CardId.EXPECT_A_FIGHT)
def expect_a_fight(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # ExpectAFight.cs OnPlay: gain energy equal to the CalculatedVar multiplier
    # (count of Attack cards in hand), then apply NoEnergyGainPower(1) to the
    # owner's own creature — no more energy gain this side's turn.
    owner = _owner(card, combat)
    state = combat.combat_player_state_for(owner)
    attack_count = sum(1 for c in state.hand if c.is_attack) if state is not None else 0
    combat.gain_energy(owner, attack_count)
    combat.apply_power_to(owner, PowerId.NO_ENERGY_GAIN, 1)


def make_expect_a_fight(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.EXPECT_A_FIGHT,
        cost=1 if upgraded else 2,
        card_type=CardType.SKILL,
        target_type=TargetType.SELF,
        rarity=CardRarity.UNCOMMON,
        effect_vars={"energy": 0, "calc_base": 0, "calc_extra": 1},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Feel No Pain ---
@register_effect(CardId.FEEL_NO_PAIN_CARD)
def feel_no_pain(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("power", 3)
    combat.apply_power_to(_owner(card, combat), PowerId.FEEL_NO_PAIN, amount)


def make_feel_no_pain(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.FEEL_NO_PAIN_CARD,
        cost=1,
        card_type=CardType.POWER,
        target_type=TargetType.SELF,
        rarity=CardRarity.UNCOMMON,
        effect_vars={"power": 4 if upgraded else 3},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Fight Me ---
@register_effect(CardId.FIGHT_ME)
def fight_me(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    hits = card.effect_vars.get("repeat", 2)
    for _ in range(hits):
        if owner.is_dead or target.is_dead:
            break
        damage = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
        apply_damage(target, damage, ValueProp.MOVE, combat, owner)
    # Gain strength
    str_amount = card.effect_vars.get("strength", 2)
    combat.apply_power_to(_owner(card, combat), PowerId.STRENGTH, str_amount)
    # Give enemy strength
    enemy_str = card.effect_vars.get("enemy_strength", 1)
    if target.is_alive:
        combat.apply_power_to(target, PowerId.STRENGTH, enemy_str)


def make_fight_me(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.FIGHT_ME,
        cost=2,
        card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.UNCOMMON,
        base_damage=6 if upgraded else 5,
        # FightMe.cs: DamageVar(5), RepeatVar(2), PowerVar<StrengthPower>(3), EnemyStrength 1;
        # OnUpgrade: damage +1, Strength +1.
        effect_vars={
            "repeat": 2,
            "strength": 4 if upgraded else 3,
            "enemy_strength": 1,
        },
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Flame Barrier ---
@register_effect(CardId.FLAME_BARRIER_CARD)
def flame_barrier(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    dmg_back = card.effect_vars.get("damage_back", 4)
    combat.apply_power_to(_owner(card, combat), PowerId.FLAME_BARRIER, dmg_back)


def make_flame_barrier(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.FLAME_BARRIER_CARD,
        cost=2,
        card_type=CardType.SKILL,
        target_type=TargetType.SELF,
        rarity=CardRarity.UNCOMMON,
        base_block=16 if upgraded else 12,
        effect_vars={"damage_back": 6 if upgraded else 4},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Forgotten Ritual ---
@register_effect(CardId.FORGOTTEN_RITUAL)
def forgotten_ritual(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    if not combat.was_card_exhausted_this_turn(owner):
        return
    energy = card.effect_vars.get("energy", 3)
    combat.gain_energy(owner, energy)


def make_forgotten_ritual(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.FORGOTTEN_RITUAL,
        cost=1,
        card_type=CardType.SKILL,
        target_type=TargetType.SELF,
        rarity=CardRarity.UNCOMMON,
        keywords=frozenset({"exhaust"}),
        effect_vars={"energy": 4 if upgraded else 3},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Grapple ---
@register_effect(CardId.GRAPPLE)
def grapple(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_to_target(card, combat, target)
    grapple_amount = card.effect_vars.get("grapple_power", 5)
    combat.apply_power_to(target, PowerId.GRAPPLE, grapple_amount)


def make_grapple(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.GRAPPLE,
        cost=1,
        card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.UNCOMMON,
        base_damage=9 if upgraded else 7,
        effect_vars={"grapple_power": 7 if upgraded else 5},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Hemokinesis ---
@register_effect(CardId.HEMOKINESIS)
def hemokinesis(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    hp_loss = card.effect_vars.get("hp_loss", 2)
    _self_hp_loss(card, combat, hp_loss)
    _deal_damage_to_target(card, combat, target)


def make_hemokinesis(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.HEMOKINESIS,
        cost=1,
        card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.UNCOMMON,
        # Hemokinesis.cs: HpLossVar(2), DamageVar(15); OnUpgrade: damage +5.
        base_damage=20 if upgraded else 15,
        effect_vars={"hp_loss": 2},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Howl From Beyond ---
@register_effect(CardId.HOWL_FROM_BEYOND)
def howl_from_beyond(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _deal_damage_all_enemies(card, combat)


@register_after_auto_post_play_hook(CardId.HOWL_FROM_BEYOND)
def howl_from_beyond_after_auto_post_play(
    card: CardInstance, player: Creature, combat: CombatState
) -> None:
    # HowlFromBeyond.cs AfterAutoPostPlayPhaseEntered: at the start of the
    # end-of-turn phase, when the card sits in the owner's exhaust pile, it
    # replays itself via CardCmd.AutoPlay (full play pipeline).
    if player is not _owner(card, combat):
        return
    state = combat.combat_player_state_for(player)
    if state is None or card not in state.exhaust:
        return
    combat.auto_play_card(card)


def make_howl_from_beyond(upgraded: bool = False) -> CardInstance:
    # HowlFromBeyond.cs defines NO CanonicalKeywords (docs/CARDS_REFERENCE.md:
    # "Keywords: None"); its ExtraHoverTips only advertise the Exhaust tooltip,
    # and the card text is conditional — "At the end of your turn, if this is in
    # your Exhaust Pile, play it." The card only reaches the exhaust pile when
    # another effect exhausts it (e.g. an added Exhaust keyword); it does not
    # exhaust on play by itself.
    return CardInstance(
        card_id=CardId.HOWL_FROM_BEYOND,
        cost=3,
        card_type=CardType.ATTACK,
        target_type=TargetType.ALL_ENEMIES,
        rarity=CardRarity.UNCOMMON,
        base_damage=21 if upgraded else 16,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Infernal Blade ---
@register_effect(CardId.INFERNAL_BLADE)
def infernal_blade(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    from sts2_env.cards.factory import create_character_cards

    generated = create_character_cards(
        combat.character_id,
        combat.combat_card_generation_rng,
        1,
        card_type=CardType.ATTACK,
        is_multiplayer=combat.is_multiplayer,
    )
    if not generated:
        return
    generated_card = generated[0]
    generated_card.set_temporary_free_this_turn()
    combat.add_generated_card_to_creature_hand(_owner(card, combat), generated_card)


def make_infernal_blade(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.INFERNAL_BLADE,
        cost=0 if upgraded else 1,
        card_type=CardType.SKILL,
        target_type=TargetType.SELF,
        rarity=CardRarity.UNCOMMON,
        keywords=frozenset({"exhaust"}),
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Inferno ---
@register_effect(CardId.INFERNO_CARD)
def inferno(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("inferno_power", 6)
    owner = _owner(card, combat)
    combat.apply_power_to(owner, PowerId.INFERNO, amount)
    power = owner.powers.get(PowerId.INFERNO)
    increment_self_damage = getattr(power, "increment_self_damage", None)
    if callable(increment_self_damage):
        increment_self_damage()


def make_inferno(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.INFERNO_CARD,
        cost=1,
        card_type=CardType.POWER,
        target_type=TargetType.SELF,
        rarity=CardRarity.UNCOMMON,
        effect_vars={"inferno_power": 9 if upgraded else 6},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Inflame ---
@register_effect(CardId.INFLAME)
def inflame(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    strength = card.effect_vars.get("strength", 2)
    combat.apply_power_to(_owner(card, combat), PowerId.STRENGTH, strength)


def make_inflame(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.INFLAME,
        cost=1,
        card_type=CardType.POWER,
        target_type=TargetType.SELF,
        rarity=CardRarity.UNCOMMON,
        effect_vars={"strength": 3 if upgraded else 2},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Juggling ---
@register_effect(CardId.JUGGLING_CARD)
def juggling(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.JUGGLING, 1)


def make_juggling(upgraded: bool = False) -> CardInstance:
    kw = frozenset({"innate"}) if upgraded else frozenset()
    return CardInstance(
        card_id=CardId.JUGGLING_CARD,
        cost=1,
        card_type=CardType.POWER,
        target_type=TargetType.SELF,
        rarity=CardRarity.UNCOMMON,
        keywords=kw,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Pillage ---
@register_effect(CardId.PILLAGE)
def pillage(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_to_target(card, combat, target)
    owner = _owner(card, combat)
    state = combat.combat_player_state_for(owner)
    if state is None:
        return
    while len(state.hand) < MAX_HAND_SIZE:
        drawn_cards = combat.draw_cards(owner, 1)
        if not drawn_cards or drawn_cards[0].card_type != CardType.ATTACK:
            break


def make_pillage(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.PILLAGE,
        cost=1,
        card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.UNCOMMON,
        base_damage=9 if upgraded else 6,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Rage ---
@register_effect(CardId.RAGE_CARD)
def rage_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("power", 3)
    combat.apply_power_to(_owner(card, combat), PowerId.RAGE, amount)


def make_rage(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.RAGE_CARD,
        cost=0,
        card_type=CardType.SKILL,
        target_type=TargetType.SELF,
        rarity=CardRarity.UNCOMMON,
        effect_vars={"power": 5 if upgraded else 3},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Rampage ---
@register_effect(CardId.RAMPAGE)
def rampage(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_to_target(card, combat, target)
    # Self-mutating: increase base damage for future plays
    increase = card.effect_vars.get("increase", 5)
    increase_base_damage(card, increase)


register_self_mutating_damage(CardId.RAMPAGE)


def make_rampage(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.RAMPAGE,
        cost=1,
        card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.UNCOMMON,
        base_damage=9,
        effect_vars={"increase": 9 if upgraded else 5},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Rupture ---
@register_effect(CardId.RUPTURE_CARD)
def rupture(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    strength = card.effect_vars.get("strength", 1)
    combat.apply_power_to(_owner(card, combat), PowerId.RUPTURE, strength)


def make_rupture(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.RUPTURE_CARD,
        cost=1,
        card_type=CardType.POWER,
        target_type=TargetType.SELF,
        rarity=CardRarity.UNCOMMON,
        effect_vars={"strength": 2 if upgraded else 1},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Second Wind ---
@register_effect(CardId.SECOND_WIND)
def second_wind(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    block_per = card.effect_vars.get("block", 5)
    # Exhaust all non-attack cards from hand, gain block for each
    to_exhaust = [c for c in combat.hand if not c.is_attack]
    owner = _owner(card, combat)
    for c in to_exhaust:
        combat.exhaust_card(c)
        if combat.is_over:
            return
        block = calculate_block(block_per, owner, ValueProp.MOVE, combat, card_source=card)
        before = owner.block
        owner.gain_block(block)
        gained = owner.block - before
        if gained > 0:
            from sts2_env.core.hooks import fire_after_block_gained

            fire_after_block_gained(owner, gained, combat, ValueProp.MOVE, combat.active_card_play_token)


def make_second_wind(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.SECOND_WIND,
        cost=1,
        card_type=CardType.SKILL,
        target_type=TargetType.SELF,
        rarity=CardRarity.UNCOMMON,
        effect_vars={"block": 7 if upgraded else 5},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Spite ---
@register_effect(CardId.SPITE)
def spite(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    # Spite.cs OnPlay: 1 hit normally; if the owner lost unblocked HP this turn
    # (LostHpThisTurn), hit Repeat times instead (RepeatVar 2, upgraded 3). No draw.
    repeat = card.effect_vars.get("repeat", 2)
    hit_count = (
        repeat
        if combat.has_unblocked_damage_received_this_turn(owner, side=CombatSide.PLAYER)
        else 1
    )
    for _ in range(hit_count):
        if owner.is_dead or target.is_dead:
            break
        damage = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
        apply_damage(target, damage, ValueProp.MOVE, combat, owner)


def make_spite(upgraded: bool = False) -> CardInstance:
    # Spite.cs: DamageVar(5), RepeatVar(2); OnUpgrade: repeat +1 (damage stays 5).
    return CardInstance(
        card_id=CardId.SPITE,
        cost=0,
        card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.UNCOMMON,
        base_damage=5,
        effect_vars={"repeat": 3 if upgraded else 2},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Stampede ---
@register_effect(CardId.STAMPEDE_CARD)
def stampede(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.STAMPEDE, card.effect_vars.get("power", 1))


def make_stampede(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.STAMPEDE_CARD,
        cost=1 if upgraded else 2,
        card_type=CardType.POWER,
        target_type=TargetType.SELF,
        rarity=CardRarity.UNCOMMON,
        effect_vars={"power": 1},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Stomp ---
@register_effect(CardId.STOMP)
def stomp(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _deal_damage_all_enemies(card, combat)


@register_after_card_entered_combat_hook(CardId.STOMP)
def stomp_after_card_entered_combat(
    card: CardInstance,
    entered_card: CardInstance,
    owner: Creature,
    combat: CombatState,
) -> None:
    if entered_card is not card or card.combat_vars.get("_is_clone"):
        return
    amount = combat.count_card_plays_finished_this_turn(owner, card_type=CardType.ATTACK)
    if amount > 0:
        card.set_temporary_cost_for_turn(max(0, card.cost - amount))


@register_before_card_played_hook(CardId.STOMP)
def stomp_before_card_played(
    card: CardInstance,
    played_card: CardInstance,
    owner: Creature,
    combat: CombatState,
) -> None:
    if played_card.card_type == CardType.ATTACK:
        card.set_temporary_cost_for_turn(max(0, card.cost - 1))


def make_stomp(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.STOMP,
        cost=3,
        card_type=CardType.ATTACK,
        target_type=TargetType.ALL_ENEMIES,
        rarity=CardRarity.UNCOMMON,
        base_damage=15 if upgraded else 12,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Stone Armor ---
@register_effect(CardId.STONE_ARMOR)
def stone_armor(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("plating", 4)
    combat.apply_power_to(_owner(card, combat), PowerId.PLATING, amount)


def make_stone_armor(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.STONE_ARMOR,
        cost=1,
        card_type=CardType.POWER,
        target_type=TargetType.SELF,
        rarity=CardRarity.UNCOMMON,
        effect_vars={"plating": 6 if upgraded else 4},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Taunt ---
@register_effect(CardId.TAUNT)
def taunt(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _gain_block(card, combat)
    vuln = card.effect_vars.get("vulnerable", 1)
    combat.apply_power_to(target, PowerId.VULNERABLE, vuln)


def make_taunt(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.TAUNT,
        cost=1,
        card_type=CardType.SKILL,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.UNCOMMON,
        base_block=8 if upgraded else 7,
        effect_vars={"vulnerable": 2 if upgraded else 1},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Unrelenting ---
@register_effect(CardId.UNRELENTING)
def unrelenting(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_to_target(card, combat, target)
    combat.apply_power_to(_owner(card, combat), PowerId.FREE_ATTACK, 1)


def make_unrelenting(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.UNRELENTING,
        cost=2,
        card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.UNCOMMON,
        # Unrelenting.cs: DamageVar(14); OnUpgrade: damage +6. Also applies
        # FreeAttackPower(1) hardcoded in OnPlay.
        base_damage=20 if upgraded else 14,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Uppercut ---
@register_effect(CardId.UPPERCUT)
def uppercut(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_to_target(card, combat, target)
    amount = card.effect_vars.get("power", 1)
    combat.apply_power_to(target, PowerId.WEAK, amount)
    combat.apply_power_to(target, PowerId.VULNERABLE, amount)


def make_uppercut(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.UPPERCUT,
        cost=2,
        card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.UNCOMMON,
        base_damage=13,
        effect_vars={"power": 2 if upgraded else 1},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Vicious ---
@register_effect(CardId.VICIOUS_CARD)
def vicious(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("cards", 1)
    combat.apply_power_to(_owner(card, combat), PowerId.VICIOUS, amount)


def make_vicious(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.VICIOUS_CARD,
        cost=1,
        card_type=CardType.POWER,
        target_type=TargetType.SELF,
        rarity=CardRarity.UNCOMMON,
        effect_vars={"cards": 2 if upgraded else 1},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Whirlwind ---
@register_effect(CardId.WHIRLWIND)
def whirlwind(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    x = getattr(card, "energy_spent", 0)
    for _ in range(x):
        if owner.is_dead:
            break
        for enemy in list(combat.hittable_enemies):
            damage = calculate_damage(card.base_damage, owner, enemy, ValueProp.MOVE, combat)
            apply_damage(enemy, damage, ValueProp.MOVE, combat, owner)


def make_whirlwind(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.WHIRLWIND,
        cost=0,
        card_type=CardType.ATTACK,
        target_type=TargetType.ALL_ENEMIES,
        rarity=CardRarity.UNCOMMON,
        base_damage=8 if upgraded else 5,
        upgraded=upgraded,
        has_energy_cost_x=True,
        instance_id=_get_next_id(),
    )


# ========================================================================
# RARE (29)
# ========================================================================

# --- Aggression ---
@register_effect(CardId.AGGRESSION_CARD)
def aggression(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.AGGRESSION, 1)


def make_aggression(upgraded: bool = False) -> CardInstance:
    kw = frozenset({"innate"}) if upgraded else frozenset()
    return CardInstance(
        card_id=CardId.AGGRESSION_CARD,
        cost=1,
        card_type=CardType.POWER,
        target_type=TargetType.SELF,
        rarity=CardRarity.RARE,
        keywords=kw,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Barricade ---
@register_effect(CardId.BARRICADE_CARD)
def barricade(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.BARRICADE, 1)


def make_barricade(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BARRICADE_CARD,
        cost=2 if upgraded else 3,
        card_type=CardType.POWER,
        target_type=TargetType.SELF,
        rarity=CardRarity.RARE,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Brand ---
@register_effect(CardId.BRAND)
def brand(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    hp_loss = card.effect_vars.get("hp_loss", 1)
    _self_hp_loss(card, combat, hp_loss)
    if combat.is_over:
        return
    if not combat.hand:
        combat.apply_power_to(_owner(card, combat), PowerId.STRENGTH, card.effect_vars.get("strength", 1))
        return

    def _resolver(selected):
        if selected is not None:
            combat.exhaust_card(selected)
        combat.apply_power_to(_owner(card, combat), PowerId.STRENGTH, card.effect_vars.get("strength", 1))

    combat.request_card_choice(
        prompt="Choose a hand card to exhaust",
        cards=list(combat.hand),
        source_pile="hand",
        resolver=_resolver,
    )


def make_brand(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BRAND,
        cost=0,
        card_type=CardType.SKILL,
        target_type=TargetType.SELF,
        rarity=CardRarity.RARE,
        effect_vars={
            "hp_loss": 1,
            "strength": 2 if upgraded else 1,
        },
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Cascade ---
@register_effect(CardId.CASCADE)
def cascade(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    count = getattr(card, "energy_spent", 0)
    if card.upgraded:
        count += 1
    combat.auto_play_from_draw(_owner(card, combat), count)


def make_cascade(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.CASCADE,
        cost=-1,
        card_type=CardType.SKILL,
        target_type=TargetType.SELF,
        rarity=CardRarity.RARE,
        upgraded=upgraded,
        has_energy_cost_x=True,
        instance_id=_get_next_id(),
    )


# --- Colossus ---
@register_effect(CardId.COLOSSUS_CARD)
def colossus(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)
    combat.apply_power_to(_owner(card, combat), PowerId.COLOSSUS, card.effect_vars.get("colossus", 1))


def make_colossus(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.COLOSSUS_CARD,
        cost=1,
        card_type=CardType.SKILL,
        target_type=TargetType.SELF,
        rarity=CardRarity.UNCOMMON,
        base_block=8 if upgraded else 5,
        effect_vars={"colossus": 1},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Conflagration ---
@register_effect(CardId.CONFLAGRATION)
def conflagration(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Conflagration.cs OnPlay: attack ALL opponents for DamageVar(2) with hit
    # count RepeatVar(4) (5 upgraded). Damage does not scale with attacks played.
    owner = _owner(card, combat)
    repeat = card.effect_vars.get("repeat", 4)
    for _ in range(repeat):
        if owner.is_dead:
            return
        for enemy in list(combat.hittable_enemies):
            damage = calculate_damage(card.base_damage, owner, enemy, ValueProp.MOVE, combat)
            apply_damage(enemy, damage, ValueProp.MOVE, combat, owner)


def make_conflagration(upgraded: bool = False) -> CardInstance:
    # Conflagration.cs: DamageVar(2), RepeatVar(4); OnUpgrade: repeat +1.
    return CardInstance(
        card_id=CardId.CONFLAGRATION,
        cost=1,
        card_type=CardType.ATTACK,
        target_type=TargetType.ALL_ENEMIES,
        rarity=CardRarity.RARE,
        base_damage=2,
        effect_vars={"repeat": 5 if upgraded else 4},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Crimson Mantle ---
@register_effect(CardId.CRIMSON_MANTLE)
def crimson_mantle(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    amount = card.effect_vars.get("crimson_mantle_power", 8)
    combat.apply_power_to(owner, PowerId.CRIMSON_MANTLE, amount)
    power = owner.powers.get(PowerId.CRIMSON_MANTLE)
    increment = getattr(power, "increment_self_damage", None)
    if callable(increment):
        increment()


def make_crimson_mantle(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.CRIMSON_MANTLE,
        cost=1,
        card_type=CardType.POWER,
        target_type=TargetType.SELF,
        rarity=CardRarity.RARE,
        effect_vars={"crimson_mantle_power": 10 if upgraded else 8},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Cruelty ---
@register_effect(CardId.CRUELTY_CARD)
def cruelty(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("cruelty_power", 25)
    combat.apply_power_to(_owner(card, combat), PowerId.CRUELTY, amount)


def make_cruelty(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.CRUELTY_CARD,
        cost=1,
        card_type=CardType.POWER,
        target_type=TargetType.SELF,
        rarity=CardRarity.RARE,
        effect_vars={"cruelty_power": 50 if upgraded else 25},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Dark Embrace ---
@register_effect(CardId.DARK_EMBRACE_CARD)
def dark_embrace(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.DARK_EMBRACE, 1)


def make_dark_embrace(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.DARK_EMBRACE_CARD,
        cost=1 if upgraded else 2,
        card_type=CardType.POWER,
        target_type=TargetType.SELF,
        rarity=CardRarity.RARE,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Demon Form ---
@register_effect(CardId.DEMON_FORM_CARD)
def demon_form(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    strength = card.effect_vars.get("strength", 2)
    combat.apply_power_to(_owner(card, combat), PowerId.DEMON_FORM, strength)


def make_demon_form(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.DEMON_FORM_CARD,
        cost=3,
        card_type=CardType.POWER,
        target_type=TargetType.SELF,
        rarity=CardRarity.RARE,
        effect_vars={"strength": 3 if upgraded else 2},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Feed ---
@register_effect(CardId.FEED)
def feed(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    should_trigger_fatal = combat.should_owner_death_trigger_fatal(target)
    _deal_damage_to_target(card, combat, target)
    # If kill, gain max HP
    if should_trigger_fatal and target.is_dead:
        max_hp_gain = card.effect_vars.get("max_hp", 3)
        combat.gain_max_hp(_owner(card, combat), max_hp_gain)


def make_feed(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.FEED,
        cost=1,
        card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.RARE,
        base_damage=12 if upgraded else 10,
        keywords=frozenset({"exhaust"}),
        can_be_generated_in_combat=False,
        effect_vars={"max_hp": 4 if upgraded else 3},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Fiend Fire ---
@register_effect(CardId.FIEND_FIRE)
def fiend_fire(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    cards_to_exhaust = list(combat.hand)
    for c in cards_to_exhaust:
        combat.exhaust_card(c)
    for _ in range(len(cards_to_exhaust)):
        if owner.is_dead or target.is_dead:
            break
        damage = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
        apply_damage(target, damage, ValueProp.MOVE, combat, owner)


def make_fiend_fire(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.FIEND_FIRE,
        cost=2,
        card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.RARE,
        base_damage=10 if upgraded else 7,
        keywords=frozenset({"exhaust"}),
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Hellraiser ---
@register_effect(CardId.HELLRAISER_CARD)
def hellraiser(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.HELLRAISER, 1)


def make_hellraiser(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.HELLRAISER_CARD,
        cost=1 if upgraded else 2,
        card_type=CardType.POWER,
        target_type=TargetType.SELF,
        rarity=CardRarity.RARE,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Impervious ---
@register_effect(CardId.IMPERVIOUS)
def impervious(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    _gain_block(card, combat)


def make_impervious(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.IMPERVIOUS,
        cost=2,
        card_type=CardType.SKILL,
        target_type=TargetType.SELF,
        rarity=CardRarity.RARE,
        base_block=40 if upgraded else 30,
        keywords=frozenset({"exhaust"}),
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Juggernaut ---
@register_effect(CardId.JUGGERNAUT_CARD)
def juggernaut(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("juggernaut_power", 6)
    combat.apply_power_to(_owner(card, combat), PowerId.JUGGERNAUT, amount)


def make_juggernaut(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.JUGGERNAUT_CARD,
        cost=2,
        card_type=CardType.POWER,
        target_type=TargetType.SELF,
        rarity=CardRarity.RARE,
        # Juggernaut.cs: PowerVar<JuggernautPower>(6); OnUpgrade: +2.
        effect_vars={"juggernaut_power": 8 if upgraded else 6},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Mangle ---
@register_effect(CardId.MANGLE)
def mangle(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_to_target(card, combat, target)
    str_loss = card.effect_vars.get("strength_loss", 10)
    combat.apply_power_to(target, PowerId.MANGLE, str_loss)


def make_mangle(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.MANGLE,
        cost=3,
        card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.RARE,
        base_damage=20 if upgraded else 15,
        effect_vars={"strength_loss": 15 if upgraded else 10},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Offering ---
@register_effect(CardId.OFFERING)
def offering(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    owner = _owner(card, combat)
    hp_loss = card.effect_vars.get("hp_loss", 6)
    _self_hp_loss(card, combat, hp_loss)
    energy = card.effect_vars.get("energy", 2)
    combat.gain_energy(owner, energy)
    draw = card.effect_vars.get("cards", 3)
    _draw_cards(combat, draw, owner)


def make_offering(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.OFFERING,
        cost=0,
        card_type=CardType.SKILL,
        target_type=TargetType.SELF,
        rarity=CardRarity.RARE,
        keywords=frozenset({"exhaust"}),
        effect_vars={
            "hp_loss": 6,
            "cards": 5 if upgraded else 3,
            "energy": 2,
        },
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- One Two Punch ---
@register_effect(CardId.ONE_TWO_PUNCH_CARD)
def one_two_punch(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    amount = card.effect_vars.get("attacks", 1)
    combat.apply_power_to(_owner(card, combat), PowerId.ONE_TWO_PUNCH, amount)


def make_one_two_punch(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.ONE_TWO_PUNCH_CARD,
        cost=1,
        card_type=CardType.SKILL,
        target_type=TargetType.SELF,
        rarity=CardRarity.RARE,
        effect_vars={"attacks": 2 if upgraded else 1},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Pact's End ---
@register_effect(CardId.PACTS_END)
def pacts_end(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # PactsEnd.cs: the card is always playable. OnPlay only deals damage when
    # CanDealDamage holds (owner's exhaust pile size >= CardsVar(3)); with
    # fewer exhausted cards playing it is legal but has no effect.
    state = combat.combat_player_state_for(_owner(card, combat))
    exhaust_size = len(state.exhaust) if state is not None else len(combat.exhaust_pile)
    if exhaust_size >= card.effect_vars.get("cards", 3):
        _deal_damage_all_enemies(card, combat)


def make_pacts_end(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.PACTS_END,
        cost=0,
        card_type=CardType.ATTACK,
        target_type=TargetType.ALL_ENEMIES,
        rarity=CardRarity.RARE,
        base_damage=23 if upgraded else 17,
        effect_vars={"cards": 3},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Primal Force ---
@register_effect(CardId.PRIMAL_FORCE)
def primal_force(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    from sts2_env.cards.status import make_giant_rock

    attacks = [c for c in list(combat.hand) if c.card_type == CardType.ATTACK]
    for attack in attacks:
        transformed = make_giant_rock(upgraded=card.upgraded)
        combat.transform_card(attack, transformed)


def make_primal_force(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.PRIMAL_FORCE,
        cost=0,
        card_type=CardType.SKILL,
        target_type=TargetType.SELF,
        rarity=CardRarity.RARE,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Pyre ---
@register_effect(CardId.PYRE)
def pyre(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    energy = card.effect_vars.get("energy", 1)
    combat.apply_power_to(_owner(card, combat), PowerId.PYRE, energy)


def make_pyre(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.PYRE,
        cost=2,
        card_type=CardType.POWER,
        target_type=TargetType.SELF,
        rarity=CardRarity.RARE,
        effect_vars={"energy": 2 if upgraded else 1},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Stoke ---
@register_effect(CardId.STOKE)
def stoke(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    # Stoke.cs OnPlay: exhaust every card in hand, then generate one random card
    # per exhausted card from the owner's character card pool into hand
    # (CardFactory.GetForCombat: random with replacement); when upgraded,
    # CardCmd.Upgrade upgrades every generated card.
    from sts2_env.cards.factory import create_character_cards

    owner = _owner(card, combat)
    count = len(combat.hand)
    for hand_card in list(combat.hand):
        combat.exhaust_card(hand_card)
    if count <= 0 or combat.is_over:
        return
    state = combat.combat_player_state_for(owner)
    if state is None:
        return
    generated = create_character_cards(
        state.character_id,
        combat.combat_card_generation_rng,
        count,
        distinct=False,
        generation_context="combat",
        is_multiplayer=combat.is_multiplayer,
    )
    if card.upgraded:
        for generated_card in generated:
            combat.upgrade_card(generated_card)
    for generated_card in generated:
        combat.add_generated_card_to_creature_hand(owner, generated_card)


def make_stoke(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.STOKE,
        cost=1,
        card_type=CardType.SKILL,
        target_type=TargetType.SELF,
        rarity=CardRarity.RARE,
        keywords=frozenset(),
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Tank ---
@register_effect(CardId.TANK_CARD)
def tank(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.TANK, 1)


def make_tank(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.TANK_CARD,
        cost=0 if upgraded else 1,
        card_type=CardType.POWER,
        target_type=TargetType.SELF,
        rarity=CardRarity.RARE,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Tear Asunder ---
@register_effect(CardId.TEAR_ASUNDER)
def tear_asunder(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    hits = 1 + combat.count_unblocked_hits_received_this_combat(owner)
    for _ in range(hits):
        if owner.is_dead or target.is_dead:
            break
        damage = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
        apply_damage(target, damage, ValueProp.MOVE, combat, owner)


def make_tear_asunder(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.TEAR_ASUNDER,
        cost=2,
        card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.RARE,
        base_damage=7 if upgraded else 5,
        effect_vars={"repeat": 1, "calc_base": 0, "calc_extra": 1},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Thrash ---
@register_effect(CardId.THRASH)
def thrash(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    owner = _owner(card, combat)
    for _ in range(2):
        if owner.is_dead or target.is_dead:
            break
        damage = calculate_damage(card.base_damage, owner, target, ValueProp.MOVE, combat)
        apply_damage(target, damage, ValueProp.MOVE, combat, owner)
    attacks = [hand_card for hand_card in combat.hand if hand_card.card_type == CardType.ATTACK]
    if attacks:
        exhausted = combat.combat_card_selection_rng.choice(attacks)
        bonus = _calculate_untargeted_card_damage(exhausted, owner, combat)
        increase_base_damage(card, bonus)
        combat.exhaust_card(exhausted)


register_self_mutating_damage(CardId.THRASH)


def make_thrash(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.THRASH,
        cost=1,
        card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.RARE,
        base_damage=6 if upgraded else 4,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Unmovable ---
@register_effect(CardId.UNMOVABLE)
def unmovable(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.UNMOVABLE, 1)


def make_unmovable(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.UNMOVABLE,
        cost=1 if upgraded else 2,
        card_type=CardType.POWER,
        target_type=TargetType.SELF,
        rarity=CardRarity.RARE,
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# ========================================================================
# ANCIENT (2)
# ========================================================================

# --- Break ---
@register_effect(CardId.BREAK)
def break_card(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    assert target is not None
    _deal_damage_to_target(card, combat, target)
    vuln = card.effect_vars.get("vulnerable", 5)
    combat.apply_power_to(target, PowerId.VULNERABLE, vuln)


def make_break(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.BREAK,
        cost=1,
        card_type=CardType.ATTACK,
        target_type=TargetType.ANY_ENEMY,
        rarity=CardRarity.ANCIENT,
        # Break.cs: DamageVar(20), PowerVar<VulnerablePower>(5); OnUpgrade: damage +10, vulnerable +2.
        base_damage=30 if upgraded else 20,
        effect_vars={"vulnerable": 7 if upgraded else 5},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# --- Corruption ---
@register_effect(CardId.CORRUPTION_CARD)
def corruption(card: CardInstance, combat: CombatState, target: Creature | None) -> None:
    combat.apply_power_to(_owner(card, combat), PowerId.CORRUPTION, 1)


def make_corruption(upgraded: bool = False) -> CardInstance:
    return CardInstance(
        card_id=CardId.CORRUPTION_CARD,
        cost=2 if upgraded else 3,
        card_type=CardType.POWER,
        target_type=TargetType.SELF,
        rarity=CardRarity.ANCIENT,
        effect_vars={"power": 1},
        upgraded=upgraded,
        instance_id=_get_next_id(),
    )


# ========================================================================
# Starter Deck Factory
# ========================================================================

def create_ironclad_starter_deck() -> list[CardInstance]:
    """Create the 10-card Ironclad starting deck."""
    deck: list[CardInstance] = []
    for _ in range(5):
        deck.append(make_strike_ironclad())
    for _ in range(4):
        deck.append(make_defend_ironclad())
    deck.append(make_bash())
    return deck


# ========================================================================
# Card Pool (all 87 Ironclad cards by rarity)
# ========================================================================

IRONCLAD_CARD_POOL = {
    CardRarity.BASIC: [
        make_strike_ironclad,
        make_defend_ironclad,
        make_bash,
    ],
    CardRarity.COMMON: [
        make_anger,
        make_armaments,
        make_blood_wall,
        make_bloodletting,
        make_body_slam,
        make_breakthrough,
        make_cinder,
        make_havoc,
        make_headbutt,
        make_iron_wave,
        make_molten_fist,
        make_perfected_strike,
        make_pommel_strike,
        make_setup_strike,
        make_shrug_it_off,
        make_sword_boomerang,
        make_thunderclap,
        make_tremble,
        make_true_grit,
        make_twin_strike,
    ],
    CardRarity.UNCOMMON: [
        make_ashen_strike,
        make_battle_trance,
        make_bludgeon,
        make_bully,
        make_burning_pact,
        make_demonic_shield,
        make_dismantle,
        make_dominate,
        make_drum_of_battle,
        make_evil_eye,
        make_expect_a_fight,
        make_feel_no_pain,
        make_fight_me,
        make_flame_barrier,
        make_forgotten_ritual,
        make_grapple,
        make_hemokinesis,
        make_howl_from_beyond,
        make_infernal_blade,
        make_inferno,
        make_inflame,
        make_juggling,
        make_pillage,
        make_rage,
        make_rampage,
        make_rupture,
        make_second_wind,
        make_spite,
        make_stampede,
        make_stomp,
        make_stone_armor,
        make_taunt,
        make_unrelenting,
        make_uppercut,
        make_vicious,
        make_whirlwind,
    ],
    CardRarity.RARE: [
        make_aggression,
        make_barricade,
        make_brand,
        make_cascade,
        make_colossus,
        make_conflagration,
        make_crimson_mantle,
        make_cruelty,
        make_dark_embrace,
        make_demon_form,
        make_feed,
        make_fiend_fire,
        make_hellraiser,
        make_impervious,
        make_juggernaut,
        make_mangle,
        make_not_yet,
        make_offering,
        make_one_two_punch,
        make_pacts_end,
        make_primal_force,
        make_pyre,
        make_stoke,
        make_tank,
        make_tear_asunder,
        make_thrash,
        make_unmovable,
    ],
    CardRarity.ANCIENT: [
        make_break,
        make_corruption,
    ],
}
