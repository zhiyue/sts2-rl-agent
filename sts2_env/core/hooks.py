"""Hook dispatch system.

Centralized hook dispatch matching CombatState.IterateHookListeners() from
the decompiled C# source. All hook-bearing objects (powers, relics, potions,
orbs, cards) participate in the dispatch.

Dispatch order per C#: Powers → Relics → Potions → Orbs → AllCards → Modifiers.
The simulator currently supports the two core listener classes that already
exist in this repo: powers and relics.
"""

from __future__ import annotations

import math
from decimal import Decimal
from typing import TYPE_CHECKING, Iterator

from sts2_env.core.enums import CombatSide, PowerId, ValueProp

if TYPE_CHECKING:
    from sts2_env.core.creature import Creature
    from sts2_env.core.combat import CombatState
    from sts2_env.powers.base import PowerInstance
    from sts2_env.relics.base import RelicInstance


def _iter_power_listeners(combat: CombatState) -> Iterator[tuple[Creature, PowerInstance]]:
    """Yield `(owner_creature, power)` listeners in C# dispatch order."""
    for creature in combat.all_creatures:
        for power in list(creature.powers.values()):
            yield creature, power


def _iter_relic_listeners(combat: CombatState) -> Iterator[tuple[Creature, RelicInstance]]:
    """Yield `(owner_creature, relic)` listeners after powers."""
    player_states = getattr(combat, "combat_player_states", None)
    if player_states is None:
        owner = combat.player
        for relic in getattr(combat, "relics", ()):
            if getattr(relic, "enabled", True):
                yield owner, relic
        return

    for state in player_states:
        for relic in getattr(state, "relics", ()):
            if getattr(relic, "enabled", True):
                yield state.creature, relic


# ─── Damage Modification ───────────────────────────────────────────────

def modify_damage(
    base_damage: int,
    dealer: Creature | None,
    target: Creature,
    props: ValueProp,
    combat: CombatState,
    card_source: object | None = None,
) -> int:
    """Full damage pipeline matching Hook.ModifyDamageInternal (Hook.cs:1902).

    1. Additive pass (Strength, etc.)
    2. Multiplicative pass (Vulnerable=1.5x, Weak=0.75x)
    3. Cap pass
    4. Floor and clamp to 0
    """
    damage = float(base_damage)
    card_source = card_source if card_source is not None else getattr(combat, "active_card_source", None)

    # Step 1: Additive modifiers
    for owner, power in _iter_power_listeners(combat):
        damage += power.modify_damage_additive(owner, dealer, target, props)
    for owner, relic in _iter_relic_listeners(combat):
        damage += relic.modify_damage_additive(owner, dealer, target, props, card_source)

    # Step 2: Multiplicative modifiers
    for owner, power in _iter_power_listeners(combat):
        mult = power.modify_damage_multiplicative(owner, dealer, target, props)
        damage *= mult
    for owner, relic in _iter_relic_listeners(combat):
        damage *= relic.modify_damage_multiplicative(owner, dealer, target, props, card_source)

    # Step 3: Cap (usually no cap)
    cap = float("inf")
    for owner, power in _iter_power_listeners(combat):
        c = power.modify_damage_cap(owner, dealer, target, damage, props)
        if c < cap:
            cap = c
    for owner, relic in _iter_relic_listeners(combat):
        c = relic.modify_damage_cap(owner, dealer, target, damage, props)
        if c < cap:
            cap = c
    if damage > cap:
        damage = cap

    return max(0, math.floor(damage))


def fire_before_attack(attack: object, combat: CombatState) -> None:
    for owner, power in _iter_power_listeners(combat):
        power.before_attack(owner, attack, combat)
    for owner, relic in _iter_relic_listeners(combat):
        relic.before_attack(owner, attack, combat)


def fire_after_attack(attack: object, combat: CombatState) -> None:
    for owner, power in _iter_power_listeners(combat):
        power.after_attack(owner, attack, combat)
    for owner, relic in _iter_relic_listeners(combat):
        relic.after_attack(owner, attack, combat)


# ─── Block Modification ────────────────────────────────────────────────

def modify_block(
    base_block: int,
    target: Creature,
    props: ValueProp,
    combat: CombatState,
    card_source: object | None = None,
    card_play: object | None = None,
) -> int:
    """Full block pipeline matching Hook.ModifyBlock (Hook.cs:960).

    1. Enchantment additive/multiplicative (via card_source)
    2. Additive pass (Dexterity)
    3. Multiplicative pass (Frail=0.75x)
    4. Floor and clamp to 0
    """
    block = float(base_block)

    # Step 1: Additive
    for owner, power in _iter_power_listeners(combat):
        block += power.modify_block_additive(owner, target, props, card_source, card_play)
    for owner, relic in _iter_relic_listeners(combat):
        block += relic.modify_block_additive(owner, target, props, card_source, card_play)

    # Step 2: Multiplicative
    for owner, power in _iter_power_listeners(combat):
        block *= power.modify_block_multiplicative(owner, target, props, card_source, card_play)
    for owner, relic in _iter_relic_listeners(combat):
        block *= relic.modify_block_multiplicative(owner, target, props, card_source, card_play)

    return max(0, math.floor(block))


# ─── HP Loss Modification ──────────────────────────────────────────────

def modify_hp_lost(
    amount: int,
    target: Creature,
    dealer: Creature | None,
    props: ValueProp,
    combat: CombatState,
) -> int:
    """Modify HP loss after block (Intangible caps at 1, TungstenRod -1, etc.)."""
    result = float(amount)
    for owner, power in _iter_power_listeners(combat):
        result = power.modify_hp_lost(owner, target, result, dealer, props)
    for owner, relic in _iter_relic_listeners(combat):
        result = relic.modify_hp_lost(owner, target, result, dealer, props)
    return max(0, math.floor(result))


# ─── Power Amount Modification ──────────────────────────────────────────

def try_block_power_application(
    target: Creature,
    power_id: PowerId,
    combat: CombatState,
) -> bool:
    """Check if any listener blocks a debuff (Artifact). Returns True if blocked."""
    for owner, power in _iter_power_listeners(combat):
        if owner is target and power.try_block_debuff(owner, power_id):
            if power.amount <= 0:
                target.powers.pop(power.power_id, None)
            return True
    return False


# ─── Hand Draw Modification ────────────────────────────────────────────

def modify_hand_draw(
    base_draw: int,
    combat: CombatState,
) -> int:
    """Modify number of cards drawn at turn start."""
    draw = base_draw
    for owner, power in _iter_power_listeners(combat):
        draw = power.modify_hand_draw(owner, draw)
    for owner, relic in _iter_relic_listeners(combat):
        draw = relic.modify_hand_draw(owner, draw, combat)
    return max(0, draw)


# ─── Max Energy Modification ───────────────────────────────────────────

def modify_max_energy(
    base_energy: int,
    combat: CombatState,
) -> int:
    """Modify max energy (e.g. from relics)."""
    energy = base_energy
    for owner, power in _iter_power_listeners(combat):
        energy = power.modify_max_energy(owner, energy)
    for owner, relic in _iter_relic_listeners(combat):
        energy = relic.modify_max_energy(owner, energy)
    return max(0, energy)


def modify_summon_amount(
    summoner: Creature,
    amount: int,
    source: object | None,
    combat: CombatState,
) -> int:
    result = amount
    for owner, power in _iter_power_listeners(combat):
        result = power.modify_summon_amount(owner, summoner, result, source, combat)
    for owner, relic in _iter_relic_listeners(combat):
        result = relic.modify_summon_amount(owner, summoner, result, source, combat)
    return max(0, result)


def fire_after_summon(
    summoner: Creature,
    amount: int,
    combat: CombatState,
) -> None:
    for owner, power in _iter_power_listeners(combat):
        power.after_summon(owner, summoner, amount, combat)
    for owner, relic in _iter_relic_listeners(combat):
        relic.after_summon(owner, summoner, amount, combat)


def fire_after_osty_revived(osty: Creature, combat: CombatState) -> None:
    for owner, power in _iter_power_listeners(combat):
        power.after_osty_revived(owner, osty, combat)
    for owner, relic in _iter_relic_listeners(combat):
        relic.after_osty_revived(owner, osty, combat)


# ─── Card Play Count ───────────────────────────────────────────────────

def modify_card_play_count(
    base_count: int,
    card: object,
    combat: CombatState,
) -> int:
    """Modify how many times a card is played (EchoForm = 2x)."""
    count = base_count
    for owner, power in _iter_power_listeners(combat):
        count = power.modify_card_play_count(owner, count, card)
    for owner, relic in _iter_relic_listeners(combat):
        count = relic.modify_card_play_count(owner, count, card)
    return count


# ─── Should Clear Block ────────────────────────────────────────────────

def should_clear_block(
    creature: Creature,
    combat: CombatState,
) -> bool:
    """Return False if any listener prevents block clearing (Barricade)."""
    for owner, power in _iter_power_listeners(combat):
        result = power.should_clear_block(owner, creature)
        if result is False:
            return False
    for owner, relic in _iter_relic_listeners(combat):
        result = relic.should_clear_block(owner, creature)
        if result is False:
            return False
    return True


def should_reset_energy(combat: CombatState) -> bool:
    """Return whether the player should be reset to max energy this turn."""
    for owner, relic in _iter_relic_listeners(combat):
        result = relic.should_reset_energy(owner, combat)
        if result is False:
            return False
    return True


def should_flush(combat: CombatState) -> bool:
    """Return whether the player's hand should flush at end of turn."""
    for owner, relic in _iter_relic_listeners(combat):
        result = relic.should_flush(owner, combat)
        if result is False:
            return False
    return True


def should_play(card: object, combat: CombatState) -> bool:
    """Return whether the current card play is allowed by all listeners."""
    for owner, relic in _iter_relic_listeners(combat):
        result = relic.should_play(owner, card, combat)
        if result is False:
            return False
    return True


def should_draw(combat: CombatState, from_hand_draw: bool) -> bool:
    """Return whether a draw should proceed."""
    for owner, power in _iter_power_listeners(combat):
        result = power.should_draw(owner, from_hand_draw)
        if result is False:
            return False
    return True


def should_take_extra_turn(combat: CombatState) -> bool:
    """Return whether the player should immediately take another turn."""
    for owner, relic in _iter_relic_listeners(combat):
        result = relic.should_take_extra_turn(owner, combat)
        if result is True:
            return True
    return False


# ─── Event Hooks (fire-and-forget) ─────────────────────────────────────

def fire_before_card_played(card: object, combat: CombatState) -> None:
    for owner, power in _iter_power_listeners(combat):
        power.before_card_played(owner, card, combat)
    for owner, relic in _iter_relic_listeners(combat):
        relic.before_card_played(owner, card, combat)


def fire_after_card_played(card: object, combat: CombatState) -> None:
    for owner, power in _iter_power_listeners(combat):
        power.after_card_played(owner, card, combat)
    for owner, relic in _iter_relic_listeners(combat):
        relic.after_card_played(owner, card, combat)


def fire_after_card_exhausted(card: object, combat: CombatState) -> None:
    for owner, power in _iter_power_listeners(combat):
        power.after_card_exhausted(owner, card, combat)
    for owner, relic in _iter_relic_listeners(combat):
        relic.after_card_exhausted(owner, card, combat)


def fire_after_card_discarded(card: object, combat: CombatState) -> None:
    for owner, relic in _iter_relic_listeners(combat):
        relic.after_card_discarded(owner, card, combat)


def fire_after_card_drawn(card: object, from_hand_draw: bool, combat: CombatState) -> None:
    import inspect

    for owner, power in _iter_power_listeners(combat):
        method = getattr(power, "on_card_drawn", None)
        if method is None:
            continue
        param_count = len(inspect.signature(method).parameters)
        if param_count >= 4:
            method(owner, card, from_hand_draw, combat)
        else:
            method(owner, card, combat)


def fire_after_modifying_card_play_count(card: object, combat: CombatState) -> None:
    for owner, power in _iter_power_listeners(combat):
        power.after_modifying_card_play_count(owner, card, combat)
    for owner, relic in _iter_relic_listeners(combat):
        relic.after_modifying_card_play_count(owner, card, combat)


def fire_before_turn_end(side: CombatSide, combat: CombatState) -> None:
    for owner, power in _iter_power_listeners(combat):
        power.before_turn_end(owner, side, combat)
    for owner, relic in _iter_relic_listeners(combat):
        relic.before_turn_end(owner, side, combat)


def fire_after_turn_end(side: CombatSide, combat: CombatState) -> None:
    from sts2_env.powers.base import PowerInstance

    for owner, power in _iter_power_listeners(combat):
        power.after_turn_end(owner, side, combat)
        if (
            side == CombatSide.ENEMY
            and type(power).after_turn_end is PowerInstance.after_turn_end
            and type(power).on_turn_end_enemy_side is not PowerInstance.on_turn_end_enemy_side
        ):
            power.on_turn_end_enemy_side(owner)
    for owner, relic in _iter_relic_listeners(combat):
        relic.after_turn_end(owner, side, combat)


def fire_before_side_turn_start(side: CombatSide, combat: CombatState) -> None:
    for owner, power in _iter_power_listeners(combat):
        power.before_side_turn_start(owner, side, combat)
    for owner, relic in _iter_relic_listeners(combat):
        relic.before_side_turn_start(owner, side, combat)


def fire_after_side_turn_start(side: CombatSide, combat: CombatState) -> None:
    from sts2_env.powers.base import PowerInstance

    for owner, power in _iter_power_listeners(combat):
        power.after_side_turn_start(owner, side, combat)
        if (
            owner.side == side
            and type(power).after_side_turn_start is PowerInstance.after_side_turn_start
            and type(power).on_turn_start_own_side is not PowerInstance.on_turn_start_own_side
        ):
            power.on_turn_start_own_side(owner, combat)
    for owner, relic in _iter_relic_listeners(combat):
        relic.after_side_turn_start(owner, side, combat)


def fire_after_block_cleared(creature: Creature, combat: CombatState) -> None:
    for owner, relic in _iter_relic_listeners(combat):
        relic.after_block_cleared(owner, creature, combat)


def fire_after_creature_added_to_combat(creature: Creature, combat: CombatState) -> None:
    for owner, power in _iter_power_listeners(combat):
        power.after_creature_added_to_combat(owner, creature, combat)
    for owner, relic in _iter_relic_listeners(combat):
        relic.after_creature_added_to_combat(owner, creature, combat)


def fire_before_combat_start(combat: CombatState) -> None:
    for owner, relic in _iter_relic_listeners(combat):
        relic.before_combat_start(owner, combat)


def fire_after_energy_reset(combat: CombatState) -> None:
    for owner, relic in _iter_relic_listeners(combat):
        relic.after_energy_reset(owner, combat)


def fire_after_shuffle(combat: CombatState) -> None:
    for owner, relic in _iter_relic_listeners(combat):
        relic.after_shuffle(owner, combat)


def fire_after_hand_emptied(combat: CombatState) -> None:
    for owner, relic in _iter_relic_listeners(combat):
        relic.after_hand_emptied(owner, combat)


def fire_before_damage_received(
    target: Creature, dealer: Creature | None, damage: int, props: ValueProp, combat: CombatState
) -> None:
    """Thorns, FlameBarrier fire here."""
    for owner, power in _iter_power_listeners(combat):
        power.before_damage_received(owner, target, dealer, damage, props, combat)
    for owner, relic in _iter_relic_listeners(combat):
        relic.before_damage_received(owner, target, dealer, damage, props, combat)


def fire_after_damage_received(
    target: Creature, dealer: Creature | None, damage: int, props: ValueProp, combat: CombatState
) -> None:
    for owner, power in _iter_power_listeners(combat):
        power.after_damage_received(owner, target, dealer, damage, props, combat)
    for owner, relic in _iter_relic_listeners(combat):
        relic.after_damage_received(owner, target, dealer, damage, props, combat)


def fire_after_damage_given(
    dealer: Creature, target: Creature, damage: int, props: ValueProp, combat: CombatState
) -> None:
    for owner, power in _iter_power_listeners(combat):
        power.after_damage_given(owner, dealer, target, damage, props, combat)
    for owner, relic in _iter_relic_listeners(combat):
        relic.after_damage_given(owner, dealer, target, damage, props, combat)


def fire_after_block_gained(
    creature: Creature, amount: int, combat: CombatState
) -> None:
    for owner, power in _iter_power_listeners(combat):
        power.after_block_gained(owner, creature, amount, combat)
    for owner, relic in _iter_relic_listeners(combat):
        relic.after_block_gained(owner, creature, amount, combat)


def fire_after_combat_victory(combat: CombatState) -> None:
    for owner, power in _iter_power_listeners(combat):
        power.after_combat_victory(owner, combat)
    for owner, relic in _iter_relic_listeners(combat):
        relic.after_combat_victory(owner, combat)


def fire_after_forge(
    combat: CombatState,
    amount: int,
    forger: Creature,
    source: object | None,
) -> None:
    for owner, power in _iter_power_listeners(combat):
        power.after_forge(owner, amount, forger, source, combat)
    for owner, relic in _iter_relic_listeners(combat):
        relic.after_forge(owner, amount, forger, source, combat)


def fire_after_combat_end(combat: CombatState) -> None:
    for owner, relic in _iter_relic_listeners(combat):
        relic.after_combat_end(owner, combat)


def fire_after_taking_extra_turn(combat: CombatState) -> None:
    for owner, relic in _iter_relic_listeners(combat):
        if relic.should_take_extra_turn(owner, combat):
            relic.after_taking_extra_turn(owner, combat)
