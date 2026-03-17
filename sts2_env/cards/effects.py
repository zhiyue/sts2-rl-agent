"""Composable card effect primitives.

These are the building blocks used by card effect functions. Each primitive
maps to a C# command (DamageCmd, CreatureCmd, PowerCmd, CardPileCmd, etc.)
but operates synchronously on the CombatState.
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from sts2_env.core.enums import CardType, PowerId, PileType, TargetType, ValueProp
from sts2_env.core.damage import calculate_damage, calculate_block, apply_damage, DamageResult

if TYPE_CHECKING:
    from sts2_env.cards.base import CardInstance
    from sts2_env.core.combat import CombatState
    from sts2_env.core.creature import Creature


def current_owner(combat: CombatState, card: CardInstance | None = None) -> Creature:
    owner = getattr(card, "owner", None) if card is not None else None
    if owner is None:
        owner = getattr(getattr(combat, "active_card_source", None), "owner", None)
    return owner or combat.player


# ─── Damage ─────────────────────────────────────────────────────────────

def deal_damage(
    combat: CombatState,
    dealer: Creature,
    target: Creature,
    base_damage: int,
    hits: int = 1,
    props: ValueProp = ValueProp.MOVE,
) -> list[DamageResult]:
    """Deal powered damage to a single target, potentially multiple hits."""
    results = []
    with combat.attack_context(dealer, target, props):
        for _ in range(hits):
            if target.is_dead:
                break
            dmg = calculate_damage(base_damage, dealer, target, props, combat)
            result = apply_damage(target, dmg, props, combat, dealer)
            results.append(result)
    combat._check_combat_end()
    return results


def deal_damage_to_all_enemies(
    combat: CombatState,
    dealer: Creature,
    base_damage: int,
    hits: int = 1,
    props: ValueProp = ValueProp.MOVE,
) -> list[DamageResult]:
    """Deal powered damage to all alive enemies."""
    results = []
    with combat.attack_context(dealer, None, props):
        for _ in range(hits):
            for enemy in list(combat.alive_enemies):
                if enemy.is_dead:
                    continue
                dmg = calculate_damage(base_damage, dealer, enemy, props, combat)
                result = apply_damage(enemy, dmg, props, combat, dealer)
                results.append(result)
    combat._check_combat_end()
    return results


def deal_damage_to_random_enemy(
    combat: CombatState,
    dealer: Creature,
    base_damage: int,
    hits: int = 1,
    props: ValueProp = ValueProp.MOVE,
) -> list[DamageResult]:
    """Deal powered damage to random enemies (re-rolls target each hit)."""
    results = []
    with combat.attack_context(dealer, None, props):
        for _ in range(hits):
            alive = combat.alive_enemies
            if not alive:
                break
            target = combat.rng.choice(alive)
            dmg = calculate_damage(base_damage, dealer, target, props, combat)
            result = apply_damage(target, dmg, props, combat, dealer)
            results.append(result)
    combat._check_combat_end()
    return results


# ─── Block ───────────────────────────────────────────────────────────────

def gain_block(
    combat: CombatState,
    target: Creature,
    base_block: int,
    props: ValueProp = ValueProp.MOVE,
    card_source: object | None = None,
    card_play: object | None = None,
) -> int:
    """Gain block with full modifier pipeline. Returns actual block gained."""
    block = calculate_block(base_block, target, props, combat,
                            card_source=card_source, card_play=card_play)
    before = target.block
    target.gain_block(block)
    gained = target.block - before
    from sts2_env.core.hooks import fire_after_block_gained
    fire_after_block_gained(target, gained, combat)
    return gained


def gain_block_unpowered(
    target: Creature,
    amount: int,
) -> int:
    """Gain flat block without modifier pipeline (Plating, Thorns-style)."""
    before = target.block
    target.gain_block(amount)
    return target.block - before


# ─── Powers ──────────────────────────────────────────────────────────────

def apply_power(
    combat: CombatState,
    target: Creature,
    power_id: PowerId,
    amount: int,
) -> None:
    """Apply a power to a creature, respecting Artifact and skip-first-tick."""
    combat.apply_power_to(target, power_id, amount)


def apply_power_to_all_enemies(
    combat: CombatState,
    power_id: PowerId,
    amount: int,
) -> None:
    """Apply a power to all alive enemies."""
    for enemy in list(combat.alive_enemies):
        combat.apply_power_to(enemy, power_id, amount)


# ─── Cards / Piles ──────────────────────────────────────────────────────

def draw_cards(combat: CombatState, count: int) -> None:
    """Draw cards from draw pile."""
    combat._draw_cards_for_creature(current_owner(combat), count)


def discard_card(combat: CombatState, card: CardInstance) -> None:
    """Move a card to discard pile."""
    owner = getattr(card, "owner", None) or current_owner(combat, card)
    zones = combat._zones_for_creature(owner)  # noqa: SLF001
    if card in zones["hand"]:
        zones["hand"].remove(card)
    zones["discard"].append(card)


def exhaust_card(combat: CombatState, card: CardInstance) -> None:
    """Exhaust a card (move to exhaust pile)."""
    owner = getattr(card, "owner", None) or current_owner(combat, card)
    zones = combat._zones_for_creature(owner)  # noqa: SLF001
    if card in zones["hand"]:
        zones["hand"].remove(card)
    elif card in zones["discard"]:
        zones["discard"].remove(card)
    elif card in zones["draw"]:
        zones["draw"].remove(card)
    zones["exhaust"].append(card)
    from sts2_env.core.hooks import fire_after_card_exhausted
    fire_after_card_exhausted(card, combat)


def add_card_to_pile(
    combat: CombatState,
    card: CardInstance,
    pile_type: PileType,
    position: str = "random",
) -> None:
    """Add a generated card to a specific pile.

    position: "top", "bottom", "random" (for draw pile only)
    """
    owner = getattr(card, "owner", None) or current_owner(combat, card)
    zones = combat._zones_for_creature(owner)  # noqa: SLF001
    if pile_type == PileType.HAND:
        from sts2_env.core.constants import MAX_HAND_SIZE
        if len(zones["hand"]) < MAX_HAND_SIZE:
            zones["hand"].append(card)
    elif pile_type == PileType.DISCARD:
        zones["discard"].append(card)
    elif pile_type == PileType.DRAW:
        if position == "top":
            zones["draw"].insert(0, card)
        elif position == "bottom":
            zones["draw"].append(card)
        else:
            idx = combat.rng.next_int(0, max(0, len(zones["draw"])))
            zones["draw"].insert(idx, card)
    elif pile_type == PileType.EXHAUST:
        zones["exhaust"].append(card)


def add_card_to_hand(combat: CombatState, card: CardInstance) -> None:
    """Add a generated card to hand (if space)."""
    add_card_to_pile(combat, card, PileType.HAND)


def move_card_to_top_of_draw(combat: CombatState, card: CardInstance) -> None:
    """Move a card from discard to top of draw pile (Headbutt)."""
    if card in combat.discard_pile:
        combat.discard_pile.remove(card)
        combat.draw_pile.insert(0, card)


# ─── Energy ──────────────────────────────────────────────────────────────

def gain_energy(combat: CombatState, amount: int) -> None:
    """Gain energy this turn."""
    combat.gain_energy(current_owner(combat), amount)


def lose_energy(combat: CombatState, amount: int) -> None:
    """Lose energy."""
    combat.lose_energy(current_owner(combat), amount)


# ─── HP ──────────────────────────────────────────────────────────────────

def lose_hp(
    combat: CombatState,
    target: Creature,
    amount: int,
    props: ValueProp = ValueProp.UNBLOCKABLE | ValueProp.UNPOWERED,
) -> DamageResult:
    """Self-damage / HP loss (unblockable, unpowered by default)."""
    result = apply_damage(target, amount, props, combat, None)
    combat._check_combat_end()
    return result


def heal(target: Creature, amount: int) -> int:
    """Heal a creature. Returns actual HP healed."""
    return target.heal(amount)


def gain_max_hp(target: Creature, amount: int) -> None:
    """Permanently gain max HP."""
    target.gain_max_hp(amount)


# ─── Stars (Regent) ─────────────────────────────────────────────────────

def gain_stars(combat: CombatState, amount: int) -> None:
    """Gain stars (Regent resource)."""
    gain = getattr(combat, "gain_stars", None)
    if callable(gain):
        gain(current_owner(combat), amount)


# ─── Orbs (Defect) ───────────────────────────────────────────────────────

def channel_orb(combat: CombatState, orb_type: str) -> None:
    """Channel an orb through CombatState so evoke-on-overflow resolves correctly."""
    channel = getattr(combat, "channel_orb", None)
    if callable(channel):
        channel(current_owner(combat), orb_type)


# ─── Osty (Necrobinder) ─────────────────────────────────────────────────

def summon_osty(combat: CombatState, amount: int) -> None:
    """Summon or revive Osty through CombatState."""
    summon = getattr(combat, "summon_osty", None)
    if callable(summon):
        summon(current_owner(combat), amount)


# ─── Utility ─────────────────────────────────────────────────────────────

def resolve_x_cost(combat: CombatState) -> int:
    """Resolve X-cost: spend all energy, return amount spent."""
    owner = current_owner(combat)
    state = combat.combat_player_state_for(owner)
    x = state.energy if state is not None else combat.energy
    combat.lose_energy(owner, x)
    return x


def random_card_from_hand(combat: CombatState) -> CardInstance | None:
    """Pick a random card from hand (for TrueGrit-style effects)."""
    hand = combat._zones_for_creature(current_owner(combat))["hand"]  # noqa: SLF001
    if not hand:
        return None
    return combat.rng.choice(hand)


def get_playable_cards(combat: CombatState) -> list[CardInstance]:
    """Get all playable cards in hand."""
    hand = combat._zones_for_creature(current_owner(combat))["hand"]  # noqa: SLF001
    return [c for c in hand if combat.can_play_card(c)]


def any_target_killed(results: list[DamageResult]) -> bool:
    """Check if any DamageResult resulted in a kill (Feed-style check)."""
    return any(r.was_killed for r in results)
