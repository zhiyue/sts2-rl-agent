"""Damage and block calculation pipelines.

Uses the centralized Hook dispatch system from hooks.py.
Faithfully reproduces Hook.ModifyDamageInternal (Hook.cs:1902) and
Hook.ModifyBlock (Hook.cs:960).
"""

from __future__ import annotations

import math
from dataclasses import dataclass
from typing import TYPE_CHECKING

from sts2_env.core.enums import ValueProp

if TYPE_CHECKING:
    from sts2_env.core.creature import Creature
    from sts2_env.core.combat import CombatState


@dataclass
class DamageResult:
    """Result of applying damage to a single target."""

    target: Creature
    blocked: int = 0
    hp_lost: int = 0
    was_killed: bool = False
    unblocked_damage: int = 0


def calculate_damage(
    base_damage: int,
    dealer: Creature | None,
    target: Creature,
    props: ValueProp,
    combat_or_creatures: CombatState | list[Creature],
) -> int:
    """Calculate final damage after all power modifiers.

    Accepts either a CombatState (uses hooks.modify_damage) or a list of
    Creature (legacy path, iterates powers directly).
    """
    from sts2_env.core.hooks import modify_damage

    # If given a CombatState, use the hook system
    if hasattr(combat_or_creatures, 'all_creatures'):
        combat = combat_or_creatures
        if dealer is not None:
            combat._ensure_pending_attack_context(dealer, target, props)
        return modify_damage(
            base_damage,
            dealer,
            target,
            props,
            combat,
            card_source=getattr(combat, "active_card_source", None),
        )

    # Legacy path: iterate creature list directly
    all_creatures: list[Creature] = combat_or_creatures
    damage = float(base_damage)

    for creature in all_creatures:
        for power in creature.powers.values():
            damage += power.modify_damage_additive(creature, dealer, target, props)

    for creature in all_creatures:
        for power in creature.powers.values():
            damage *= power.modify_damage_multiplicative(creature, dealer, target, props)

    return max(0, math.floor(damage))


def calculate_block(
    base_block: int,
    target: Creature,
    props: ValueProp,
    combat_or_creatures: CombatState | list[Creature],
    card_source: object | None = None,
    card_play: object | None = None,
) -> int:
    """Calculate final block after all power modifiers."""
    from sts2_env.core.hooks import modify_block

    if hasattr(combat_or_creatures, 'all_creatures'):
        return modify_block(base_block, target, props, combat_or_creatures,
                            card_source=card_source, card_play=card_play)

    all_creatures: list[Creature] = combat_or_creatures
    block = float(base_block)

    for creature in all_creatures:
        for power in creature.powers.values():
            block += power.modify_block_additive(creature, target, props,
                                                  card_source, card_play)

    for creature in all_creatures:
        for power in creature.powers.values():
            block *= power.modify_block_multiplicative(creature, target, props,
                                                        card_source, card_play)

    return max(0, math.floor(block))


def apply_damage(
    target: Creature,
    damage: int,
    props: ValueProp,
    combat: CombatState | None = None,
    dealer: Creature | None = None,
) -> DamageResult:
    """Apply calculated damage to a creature.

    When combat is provided, fires damage hooks and applies HP loss modification.
    Returns DamageResult with details.
    """
    from sts2_env.core.hooks import (
        fire_before_damage_received, fire_after_damage_received,
        fire_after_damage_given, modify_hp_lost,
    )

    attack = None
    if combat is not None:
        attack = combat.active_attack or combat.pending_auto_attack
        can_hit = getattr(combat, "can_hit_creature", None)
        if callable(can_hit) and not can_hit(target):
            return DamageResult(target=target, blocked=0, hp_lost=0, was_killed=False, unblocked_damage=0)

    # Fire before-damage hooks (Thorns)
    if combat is not None:
        fire_before_damage_received(target, dealer, damage, props, combat)

    # Block absorption
    unblockable = bool(props & ValueProp.UNBLOCKABLE)
    blocked = target.damage_block(damage, unblockable)
    remaining = damage - blocked

    # HP loss modification (Intangible, TungstenRod)
    if combat is not None and remaining > 0:
        remaining = modify_hp_lost(remaining, target, dealer, props, combat)

    # Apply HP loss
    was_alive = target.is_alive
    hp_lost = target.lose_hp(remaining)
    was_killed = was_alive and target.is_dead

    result = DamageResult(
        target=target,
        blocked=blocked,
        hp_lost=hp_lost,
        was_killed=was_killed,
        unblocked_damage=remaining,
    )

    if attack is not None:
        attack.results.append(result)

    # Fire after-damage hooks
    if combat is not None:
        combat.record_damage_event(dealer, target, props, remaining)
        fire_after_damage_received(target, dealer, remaining, props, combat)
        if blocked > 0:
            for owner in combat.all_creatures:
                for power in owner.powers.values():
                    on_damage_blocked = getattr(power, "on_damage_blocked", None)
                    if callable(on_damage_blocked):
                        on_damage_blocked(owner, blocked, dealer, props, combat)
        if dealer is not None:
            fire_after_damage_given(dealer, target, remaining, props, combat)

    return result
