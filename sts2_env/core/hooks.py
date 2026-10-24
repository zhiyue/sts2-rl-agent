"""Hook dispatch system.

Centralized hook dispatch matching CombatState.IterateHookListeners() from
the decompiled C# source. All hook-bearing objects (powers, relics, potions,
orbs, cards) participate in the dispatch.

Dispatch order per C#: Powers → Relics → Potions → Orbs → AllCards → Modifiers.
The simulator currently supports the two core listener classes that already
exist in this repo: powers and relics.

┌─ A7 note — structural difference vs C# (do not refactor blindly) ─────────
│ C# builds ONE global listener table per dispatch: CombatState.cs:410-501
│ (IterateHookListeners) walks creatures in order and appends, per creature,
│ its powers → relics → potions → orbs → all cards in every pile (plus card
│ afflictions/enchantments), then run Modifiers and Badges.  Every Hook.*
│ entry point iterates that same flat list, and most are centrally guarded
│ by CombatManager.IsOverOrEnding.
│
│ This module dispatches per listener CLASS: powers and relics are yielded
│ either interleaved per creature (`_iter_hook_listeners`, used by most
│ hooks) or in two segmented passes (`_iter_power_listeners` then
│ `_iter_relic_listeners`, used by a few hooks).  Potions, orbs, and card
│ models are NOT in the table; card models get their own fire_card_* calls
│ dispatched from sts2_env/cards/registry.py at specific call sites.
│
│ This is a faithful numeric equivalent today because no potion, orb, or
│ card implements a Modify* hook in v0.107.1 (the only card-adjacent
│ listener is the PerfectFit enchantment, handled explicitly).  It becomes
│ a real problem the moment the FIRST potion/orb/card implements a
│ Modify* hook: C# would interleave it into the global order (e.g. a card
│ listener between an enemy power and a relic), while this module would
│ run it out of order.  When that happens, extend `_iter_hook_listeners`
│ to include the new listener classes instead of adding more segmented
│ passes.
└────────────────────────────────────────────────────────────────────────────
"""

from __future__ import annotations

import math
from decimal import Decimal
from typing import TYPE_CHECKING, Iterator

from sts2_env.cards.enchantments import (
    enchant_block_additive,
    enchant_block_multiplicative,
    enchant_damage_additive,
    enchant_damage_multiplicative,
    modify_shuffle_order as enchantment_modify_shuffle_order,
)
from sts2_env.core.enums import CardPilePosition, CombatSide, PileType, PowerId, ValueProp

_MULTIPLAYER_CHARGE_SCALED_POWER_IDS = frozenset({
    PowerId.ARTIFACT,
    PowerId.BUFFER,
    PowerId.PLATING,
    PowerId.SLIPPERY,
})
_NO_MULTIPLAYER_BLOCK_SCALING = 1.0

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


def _relic_listeners_by_owner(combat: CombatState) -> dict[int, list[RelicInstance]]:
    """Group enabled relic listeners by the id of their owner creature."""
    grouped: dict[int, list[RelicInstance]] = {}
    player_states = getattr(combat, "combat_player_states", None)
    if player_states is None:
        grouped[id(combat.player)] = [
            relic
            for relic in getattr(combat, "relics", ())
            if getattr(relic, "enabled", True)
        ]
        return grouped
    for state in player_states:
        grouped[id(state.creature)] = [
            relic
            for relic in getattr(state, "relics", ())
            if getattr(relic, "enabled", True)
        ]
    return grouped


def _iter_hook_listeners(combat: CombatState) -> Iterator[tuple[Creature, object, bool]]:
    """Yield `(owner, listener, is_relic)` in the C# combined dispatch order.

    Matches `CombatState.IterateHookListeners` (CombatState.cs:410): for each
    creature (player first, then allies, then enemies) all of the creature's
    powers are yielded, followed by that same creature's relic listeners.
    C# interleaves per creature — the player's relics fire before any enemy
    power — so a flat "all powers then all relics" split would dispatch the
    two listener classes out of order.
    """
    relics_by_owner = _relic_listeners_by_owner(combat)
    for creature in combat.all_creatures:
        for power in list(creature.powers.values()):
            yield creature, power, False
        for relic in relics_by_owner.get(id(creature), ()):
            yield creature, relic, True


def is_relic_listener(listener: object) -> bool:
    from sts2_env.relics.base import RelicInstance

    return isinstance(listener, RelicInstance)


def _iter_modifier_listeners(combat: CombatState) -> Iterator[object]:
    primary_state = getattr(combat, "_primary_player_state", None)
    player_state = getattr(primary_state, "player_state", None)
    run_state = getattr(player_state, "run_state", None)
    yield from getattr(run_state, "modifiers", ())


def _combat_run_state(combat: CombatState) -> object | None:
    primary_state = getattr(combat, "_primary_player_state", None)
    player_state = getattr(primary_state, "player_state", None)
    return getattr(player_state, "run_state", None)


def _multiplayer_enemy_scaling_factor(combat: CombatState) -> float:
    from sts2_env.core.constants import MULTIPLAYER_ACT_3_BOSS_SCALING, MULTIPLAYER_ACT_SCALING
    from sts2_env.core.enums import RoomType

    run_state = _combat_run_state(combat)
    act_index = getattr(run_state, "current_act_index", 0)
    room_type = getattr(getattr(combat, "room", None), "room_type", None)
    if act_index == 2 and room_type == RoomType.BOSS:
        return MULTIPLAYER_ACT_3_BOSS_SCALING
    if 0 <= act_index < len(MULTIPLAYER_ACT_SCALING):
        return MULTIPLAYER_ACT_SCALING[act_index]
    return MULTIPLAYER_ACT_SCALING[-1]


def _is_multiplayer_scaled_enemy(creature: Creature) -> bool:
    return creature.side == CombatSide.ENEMY


def scaled_multiplayer_enemy_hp(base_hp: int, combat: CombatState) -> int:
    from sts2_env.core.constants import SOLO_PLAYER_COUNT

    player_count = len(combat.combat_player_states)
    if player_count == SOLO_PLAYER_COUNT:
        return base_hp
    scaled = base_hp * player_count * _multiplayer_enemy_scaling_factor(combat)
    return int(scaled)


def scaled_multiplayer_enemy_max_hp(creature: Creature, combat: CombatState) -> int:
    if not _is_multiplayer_scaled_enemy(creature):
        return creature.max_hp
    return scaled_multiplayer_enemy_hp(creature.max_hp, combat)


def _multiplayer_enemy_block_multiplier(target: Creature, props: ValueProp, combat: CombatState) -> float:
    from sts2_env.core.constants import SOLO_PLAYER_COUNT

    if not _is_multiplayer_scaled_enemy(target):
        return _NO_MULTIPLAYER_BLOCK_SCALING
    if not props.is_powered_card_or_monster_move_block():
        return _NO_MULTIPLAYER_BLOCK_SCALING
    player_count = len(combat.combat_player_states)
    if player_count == SOLO_PLAYER_COUNT:
        return _NO_MULTIPLAYER_BLOCK_SCALING
    return player_count * _multiplayer_enemy_scaling_factor(combat)


def _should_scale_power_in_multiplayer(power_id: PowerId) -> bool:
    from sts2_env.core.creature import get_power_class

    power_cls = get_power_class(power_id)
    return bool(getattr(power_cls, "should_scale_in_multiplayer", False))


def scaled_multiplayer_power_amount(
    power_id: PowerId,
    amount: int,
    target: Creature | None,
    combat: CombatState,
) -> int:
    from sts2_env.core.constants import (
        MULTIPLAYER_BASE_CHARGE_MULTIPLIER,
        MULTIPLAYER_CHARGE_EXTRA_PER_ADDITIONAL_PLAYER,
        SOLO_PLAYER_COUNT,
    )

    if target is None or not _is_multiplayer_scaled_enemy(target):
        return amount
    if not _should_scale_power_in_multiplayer(power_id):
        return amount
    player_count = len(combat.combat_player_states)
    if player_count == SOLO_PLAYER_COUNT:
        return amount
    if power_id in _MULTIPLAYER_CHARGE_SCALED_POWER_IDS:
        multiplier = (
            (player_count - SOLO_PLAYER_COUNT)
            * MULTIPLAYER_CHARGE_EXTRA_PER_ADDITIONAL_PLAYER
            + MULTIPLAYER_BASE_CHARGE_MULTIPLIER
        )
        return amount * multiplier
    scaled = amount * player_count * _multiplayer_enemy_scaling_factor(combat)
    return int(scaled)


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

    if card_source is not None and hasattr(card_source, "enchantments"):
        damage += enchant_damage_additive(card_source, props)

    # Step 1: Additive modifiers (combined C# listener order per pass)
    for owner, listener, is_relic in _iter_hook_listeners(combat):
        if is_relic:
            damage += listener.modify_damage_additive(owner, dealer, target, props, card_source)
        else:
            damage += listener.modify_damage_additive(owner, dealer, target, props)

    # Step 2: Multiplicative modifiers
    for owner, listener, is_relic in _iter_hook_listeners(combat):
        if is_relic:
            damage *= listener.modify_damage_multiplicative(owner, dealer, target, props, card_source)
        else:
            damage *= listener.modify_damage_multiplicative(owner, dealer, target, props)
    if card_source is not None and hasattr(card_source, "enchantments"):
        damage *= enchant_damage_multiplicative(card_source, props)

    # Step 3: Cap (usually no cap)
    cap = float("inf")
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        c = listener.modify_damage_cap(owner, dealer, target, damage, props)
        if c < cap:
            cap = c
    if damage > cap:
        damage = cap

    return max(0, math.floor(damage))


def modify_power_amount_given(
    power_id: PowerId,
    amount: int,
    giver: Creature,
    target: Creature | None,
    source: object | None,
    combat: CombatState,
) -> int:
    """Apply giver-owned relic modifiers to a power amount before it is applied."""
    modified = scaled_multiplayer_power_amount(power_id, amount, target, combat)
    for owner, relic in _iter_relic_listeners(combat):
        if owner is giver:
            modified = relic.modify_power_amount_given(
                owner,
                power_id,
                modified,
                giver,
                target,
                source,
                combat,
            )
    return modified


def modify_power_amount_received(
    power_id: PowerId,
    amount: int,
    target: Creature,
    applier: Creature | None,
    source: object | None,
    combat: CombatState,
) -> int:
    modified = amount
    modifier_ids: set[int] = set()
    for owner, relic in _iter_relic_listeners(combat):
        if owner is target:
            before = modified
            modified = relic.modify_power_amount_received(
                owner,
                power_id,
                modified,
                target,
                applier,
                source,
                combat,
            )
            if modified != before:
                modifier_ids.add(id(relic))
    combat._power_amount_received_modifier_ids = modifier_ids
    return modified


def fire_after_modifying_power_amount_given(
    power_id: PowerId,
    original_amount: int,
    modified_amount: int,
    giver: Creature,
    target: Creature | None,
    source: object | None,
    combat: CombatState,
) -> None:
    for owner, relic in _iter_relic_listeners(combat):
        if owner is giver:
            relic.after_modifying_power_amount_given(
                owner,
                power_id,
                original_amount,
                modified_amount,
                giver,
                target,
                source,
                combat,
            )


def fire_after_modifying_power_amount_received(
    power_id: PowerId,
    original_amount: int,
    modified_amount: int,
    target: Creature,
    applier: Creature | None,
    source: object | None,
    combat: CombatState,
) -> None:
    modifier_ids = getattr(combat, "_power_amount_received_modifier_ids", set())
    for owner, relic in _iter_relic_listeners(combat):
        if owner is target and id(relic) in modifier_ids:
            relic.after_modifying_power_amount_received(
                owner,
                power_id,
                original_amount,
                modified_amount,
                target,
                applier,
                source,
                combat,
            )
    combat._power_amount_received_modifier_ids = set()


def fire_before_attack(attack: object, combat: CombatState) -> None:
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        listener.before_attack(owner, attack, combat)


def fire_after_attack(attack: object, combat: CombatState) -> None:
    from sts2_env.cards.registry import fire_card_after_attack

    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        listener.after_attack(owner, attack, combat)
    for state in getattr(combat, "combat_player_states", ()):
        for card in combat.unique_cards_in_piles(state.all_piles):
            fire_card_after_attack(card, attack, combat)


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
    # C# CreatureCmd.GainBlock fires Hook.BeforeBlockGained(combatState,
    # creature, amount, props, cardPlay?.Card) with the RAW amount right
    # before Hook.ModifyBlock (CreatureCmd.cs:634-642).  Every real block
    # application in this sim flows through calculate_block -> modify_block,
    # so firing here reproduces the C# ordering; pure out-of-combat/unpowered
    # gains that skip the pipeline do not fire it.  Skeleton dispatch: no
    # v0.107.1 power or relic implements BeforeBlockGained yet.
    fire_before_block_gained(target, base_block, combat, props=props, card_source=card_source)

    block = float(base_block)
    modifier_ids: list[tuple[Creature, object]] = []

    if card_source is not None and hasattr(card_source, "enchantments"):
        # C# Hook.ModifyBlock applies the card enchantment's additive AND
        # multiplicative corrections up front, before BOTH listener passes
        # (Hook.cs:1316-1322) — the enchantment segments are not repeated
        # after the listeners.
        block += enchant_block_additive(card_source, props)
        block *= enchant_block_multiplicative(card_source, props)

    # Step 1: Additive
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        delta = listener.modify_block_additive(owner, target, props, card_source, card_play)
        block += delta
        if delta != 0:
            modifier_ids.append((owner, listener))

    # Step 2: Multiplicative
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        multiplier = listener.modify_block_multiplicative(owner, target, props, card_source, card_play, combat)
        block *= multiplier
        if multiplier != 1.0:
            modifier_ids.append((owner, listener))
    block *= _multiplayer_enemy_block_multiplier(target, props, combat)

    modified_block = max(0, math.floor(block))
    if modifier_ids:
        for owner, listener in modifier_ids:
            listener.after_modifying_block_amount(owner, modified_block, card_source, card_play, combat)
    return modified_block


# ─── HP Loss Modification ──────────────────────────────────────────────

def modify_hp_lost_before_osty(
    amount: int,
    target: Creature,
    dealer: Creature | None,
    props: ValueProp,
    combat: CombatState,
) -> int:
    result = float(amount)
    for owner, relic in _iter_relic_listeners(combat):
        result = relic.modify_hp_lost_before_osty(owner, target, result, dealer, props)
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        result = listener.modify_hp_lost_before_osty_late(owner, target, result, dealer, props)
    return max(0, math.floor(result))


def modify_hp_lost_after_osty(
    amount: int,
    target: Creature,
    dealer: Creature | None,
    props: ValueProp,
    combat: CombatState,
) -> int:
    result = float(amount)
    # C# Hook.ModifyHpLost AfterOsty phase: one combined pass over all
    # listeners (powers override ModifyHpLost, relics ModifyHpLostAfterOsty),
    # then a second pass for the Late variant (implemented by powers).
    for owner, listener, is_relic in _iter_hook_listeners(combat):
        if is_relic:
            result = listener.modify_hp_lost_after_osty(owner, target, result, dealer, props)
        else:
            result = listener.modify_hp_lost(owner, target, result, dealer, props)
    for owner, power in _iter_power_listeners(combat):
        result = power.modify_hp_lost_late(owner, target, result, dealer, props)
    return max(0, math.floor(result))


def modify_hp_lost(
    amount: int,
    target: Creature,
    dealer: Creature | None,
    props: ValueProp,
    combat: CombatState,
) -> int:
    """Modify HP loss after block for legacy call sites without Osty redirect."""
    result = modify_hp_lost_before_osty(amount, target, dealer, props, combat)
    return modify_hp_lost_after_osty(result, target, dealer, props, combat)


def modify_unblocked_damage_target(
    target: Creature,
    amount: int,
    props: ValueProp,
    dealer: Creature | None,
    combat: CombatState,
) -> Creature:
    result = target
    for owner, power in _iter_power_listeners(combat):
        result = power.modify_unblocked_damage_target(owner, result, amount, props, dealer)
    return result


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
    drawing_owner: Creature | None = None,
) -> int:
    """Modify number of cards drawn at turn start."""
    draw = base_draw
    modifiers: list[tuple[Creature, object]] = []
    for owner, listener, is_relic in _iter_hook_listeners(combat):
        if drawing_owner is not None and owner is not drawing_owner:
            continue
        before = draw
        draw = listener.modify_hand_draw(owner, draw, combat) if is_relic else listener.modify_hand_draw(owner, draw)
        if int(before) != int(draw):
            modifiers.append((owner, listener))
    for owner, listener, is_relic in _iter_hook_listeners(combat):
        if drawing_owner is not None and owner is not drawing_owner:
            continue
        before = draw
        draw = listener.modify_hand_draw_late(owner, draw, combat) if is_relic else listener.modify_hand_draw_late(owner, draw)
        if int(before) != int(draw):
            modifiers.append((owner, listener))
    for owner, modifier in modifiers:
        modifier.after_modifying_hand_draw(owner, combat)
    return max(0, draw)


# ─── Max Energy Modification ───────────────────────────────────────────

def modify_max_energy(
    base_energy: int,
    combat: CombatState,
    energy_owner: Creature | None = None,
) -> int:
    """Modify max energy (e.g. from relics)."""
    energy = base_energy
    target = energy_owner if energy_owner is not None else combat.player
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        if owner is not target:
            continue
        energy = listener.modify_max_energy(owner, energy)
    return max(0, energy)


def modify_summon_amount(
    summoner: Creature,
    amount: int,
    source: object | None,
    combat: CombatState,
) -> int:
    result = amount
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        result = listener.modify_summon_amount(owner, summoner, result, source, combat)
    return max(0, result)


def fire_after_summon(
    summoner: Creature,
    amount: int,
    combat: CombatState,
) -> None:
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        listener.after_summon(owner, summoner, amount, combat)


def fire_after_osty_revived(osty: Creature, combat: CombatState) -> None:
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        listener.after_osty_revived(owner, osty, combat)


# ─── Card Play Count ───────────────────────────────────────────────────

def modify_card_play_count(
    base_count: int,
    card: object,
    combat: CombatState,
) -> int:
    """Modify how many times a card is played (EchoForm = 2x)."""
    count = base_count
    modifier_ids: list[tuple[Creature, object]] = []
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        before = count
        count = listener.modify_card_play_count(owner, count, card)
        if count != before:
            modifier_ids.append((owner, listener))
    combat._card_play_count_modifier_ids = modifier_ids
    return count


# ─── Should Clear Block ────────────────────────────────────────────────

def should_clear_block(
    creature: Creature,
    combat: CombatState,
) -> bool:
    """Return False if any listener prevents block clearing (Barricade)."""
    return block_clear_preventer(creature, combat) is None


def block_clear_preventer(
    creature: Creature,
    combat: CombatState,
) -> tuple[Creature, object] | None:
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        if listener.should_clear_block(owner, creature) is False:
            return owner, listener
    return None


def fire_after_preventing_block_clear(
    preventer: tuple[Creature, object],
    creature: Creature,
    combat: CombatState,
) -> None:
    owner, listener = preventer
    listener.after_preventing_block_clear(owner, creature, combat)


def should_reset_energy(combat: CombatState) -> bool:
    """Return whether the player should be reset to max energy this turn."""
    for owner, relic in _iter_relic_listeners(combat):
        result = relic.should_reset_energy(owner, combat)
        if result is False:
            return False
    return True


def should_flush(combat: CombatState, flushing_owner: Creature | None = None) -> bool:
    """Return whether the player's hand should flush at end of turn."""
    if flushing_owner is None:
        flushing_owner = combat.player
    for owner, listener, is_relic in _iter_hook_listeners(combat):
        if is_relic:
            if owner is not flushing_owner:
                continue
            result = listener.should_flush(owner, combat)
        else:
            result = listener.should_flush(owner, flushing_owner, combat)
        if result is False:
            return False
    return True


def fire_before_flush(flushing_owner: Creature, combat: CombatState) -> None:
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        listener.before_flush(owner, flushing_owner, combat)


def fire_before_flush_late(flushing_owner: Creature, combat: CombatState) -> None:
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        listener.before_flush_late(owner, flushing_owner, combat)


def should_play(card: object, combat: CombatState) -> bool:
    """Return whether the current card play is allowed by all listeners."""
    for owner, listener, is_relic in _iter_hook_listeners(combat):
        if is_relic:
            result = listener.should_play(owner, card, combat)
            if result is False:
                return False
        else:
            result = getattr(listener, "should_play", lambda *_: None)(owner, card, combat)
            if result is False:
                return False
            should_card_be_playable = getattr(listener, "should_card_be_playable", None)
            if callable(should_card_be_playable) and should_card_be_playable(owner, card) is False:
                return False
    return True


def modify_card_play_result_pile_type_and_position(
    card: object,
    combat: CombatState,
    is_auto_play: bool,
    energy_value: int,
    pile_type: PileType,
    position: CardPilePosition,
) -> tuple[PileType, CardPilePosition]:
    modifiers: list[object] = []
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        modify_result = getattr(listener, "modify_card_play_result_pile_type_and_position", None)
        if not callable(modify_result):
            continue
        previous = (pile_type, position)
        pile_type, position = modify_result(owner, card, is_auto_play, energy_value, pile_type, position)
        if (pile_type, position) != previous:
            modifiers.append(listener)
    for modifier in modifiers:
        after_modify = getattr(modifier, "after_modifying_card_play_result_pile_or_position", None)
        if callable(after_modify):
            after_modify(card, pile_type, position, combat)
    return pile_type, position


def should_draw(combat: CombatState, drawing_owner: Creature, from_hand_draw: bool) -> bool:
    """Return whether a draw should proceed."""
    for owner, listener, is_relic in _iter_hook_listeners(combat):
        if owner is not drawing_owner:
            continue
        result = listener.should_draw(owner, from_hand_draw, combat) if is_relic else listener.should_draw(owner, from_hand_draw)
        if result is False:
            listener.after_preventing_draw(owner, combat)
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
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        listener.before_card_played(owner, card, combat)


def fire_after_card_played(card: object, combat: CombatState) -> None:
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        listener.after_card_played(owner, card, combat)


def fire_after_card_generated_for_combat(
    card: object,
    added_by_player: bool,
    combat: CombatState,
) -> None:
    from sts2_env.cards.registry import fire_card_after_card_generated_for_combat

    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        listener.after_card_generated_for_combat(owner, card, added_by_player, combat)
    for state in getattr(combat, "combat_player_states", ()):
        for listener_card in combat.unique_cards_in_piles(state.all_piles):
            fire_card_after_card_generated_for_combat(listener_card, card, added_by_player, combat)


def fire_after_card_exhausted(card: object, combat: CombatState) -> None:
    record_card_exhausted = getattr(combat, "record_card_exhausted", None)
    if callable(record_card_exhausted):
        record_card_exhausted(card)
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        listener.after_card_exhausted(owner, card, combat)
    # C# Hook.AfterCardExhausted also dispatches to all card models (AllCards
    # listener class, after powers/relics) — e.g. DrumOfBattle.
    from sts2_env.cards.registry import fire_card_after_card_exhausted

    for state in getattr(combat, "combat_player_states", ()):
        for listener_card in combat.unique_cards_in_piles(state.all_piles):
            fire_card_after_card_exhausted(listener_card, card, combat)


def fire_after_card_discarded(card: object, combat: CombatState) -> None:
    record_card_discarded = getattr(combat, "record_card_discarded", None)
    if callable(record_card_discarded):
        record_card_discarded(card)
    for owner, relic in _iter_relic_listeners(combat):
        relic.after_card_discarded(owner, card, combat)


def fire_before_potion_used(
    potion: object,
    target: Creature | None,
    combat: CombatState,
) -> None:
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        listener.before_potion_used(owner, potion, target, combat)


def fire_after_potion_procured(potion: object, combat: CombatState) -> None:
    for owner, relic in _iter_relic_listeners(combat):
        relic.after_potion_procured(owner, potion, combat)


def fire_after_potion_discarded(potion: object, combat: CombatState) -> None:
    for owner, relic in _iter_relic_listeners(combat):
        relic.after_potion_discarded(owner, potion, combat)


def fire_after_potion_used(
    potion: object,
    target: Creature | None,
    combat: CombatState,
) -> None:
    for owner, relic in _iter_relic_listeners(combat):
        relic.after_potion_used(owner, potion, target, combat)


def fire_after_card_drawn(card: object, from_hand_draw: bool, combat: CombatState) -> None:
    from sts2_env.cards.registry import fire_card_after_card_drawn

    for owner, power in _iter_power_listeners(combat):
        power.on_card_drawn(owner, card, from_hand_draw, combat)
    for state in getattr(combat, "combat_player_states", ()):
        for listener_card in combat.unique_cards_in_piles(state.all_piles):
            fire_card_after_card_drawn(listener_card, card, from_hand_draw, combat)


def fire_after_modifying_card_play_count(card: object, combat: CombatState) -> None:
    modifier_ids = getattr(combat, "_card_play_count_modifier_ids", [])
    for owner, modifier in modifier_ids:
        modifier.after_modifying_card_play_count(owner, card, combat)
    combat._card_play_count_modifier_ids = []


def fire_before_turn_end(side: CombatSide, combat: CombatState) -> None:
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        listener.before_turn_end_very_early(owner, side, combat)
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        listener.before_turn_end_early(owner, side, combat)
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        listener.before_turn_end(owner, side, combat)


def fire_after_turn_end(side: CombatSide, combat: CombatState) -> None:
    from sts2_env.powers.base import PowerInstance

    for owner, listener, is_relic in _iter_hook_listeners(combat):
        if is_relic:
            listener.after_turn_end(owner, side, combat)
            continue
        power = listener
        used_turn_hook = type(power).after_turn_end is not PowerInstance.after_turn_end
        used_legacy_tick = False
        power.after_turn_end(owner, side, combat)
        if (
            side == CombatSide.ENEMY
            and type(power).after_turn_end is PowerInstance.after_turn_end
            and type(power).on_turn_end_enemy_side is not PowerInstance.on_turn_end_enemy_side
        ):
            power.on_turn_end_enemy_side(owner)
            used_legacy_tick = True
        should_remove = power.amount == 0 if power.allow_negative else power.amount <= 0
        if (used_turn_hook or used_legacy_tick) and should_remove and owner.powers.get(power.power_id) is power:
            combat._remove_power(owner, power.power_id)
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        listener.after_turn_end_late(owner, side, combat)


def fire_before_side_turn_start(side: CombatSide, combat: CombatState) -> None:
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        listener.before_side_turn_start(owner, side, combat)


def fire_before_hand_draw(drawing_owner: Creature, combat: CombatState) -> None:
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        if owner is drawing_owner:
            listener.before_hand_draw(owner, combat)


def fire_before_hand_draw_late(drawing_owner: Creature, combat: CombatState) -> None:
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        if owner is drawing_owner:
            listener.before_hand_draw_late(owner, combat)


def fire_after_player_turn_start(drawing_owner: Creature, combat: CombatState) -> None:
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        if owner is drawing_owner:
            listener.after_player_turn_start_early(owner, combat)
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        if owner is drawing_owner:
            listener.after_player_turn_start(owner, combat)
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        if owner is drawing_owner:
            listener.after_player_turn_start_late(owner, combat)


def fire_after_side_turn_start(side: CombatSide, combat: CombatState) -> None:
    from sts2_env.powers.base import PowerInstance

    for owner, listener, is_relic in _iter_hook_listeners(combat):
        if is_relic:
            listener.after_side_turn_start(owner, side, combat)
            continue
        power = listener
        used_turn_hook = type(power).after_side_turn_start is not PowerInstance.after_side_turn_start
        used_legacy_tick = False
        power.after_side_turn_start(owner, side, combat)
        if (
            owner.side == side
            and type(power).after_side_turn_start is PowerInstance.after_side_turn_start
            and type(power).on_turn_start_own_side is not PowerInstance.on_turn_start_own_side
        ):
            power.on_turn_start_own_side(owner, combat)
            used_legacy_tick = True
        should_remove = power.amount == 0 if power.allow_negative else power.amount <= 0
        if (used_turn_hook or used_legacy_tick) and should_remove and owner.powers.get(power.power_id) is power:
            combat._remove_power(owner, power.power_id)


def fire_before_play_phase_start(player: Creature, combat: CombatState) -> None:
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        listener.before_play_phase_start(owner, player, combat)


def fire_after_block_cleared(creature: Creature, combat: CombatState) -> None:
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        listener.after_block_cleared(owner, creature, combat)


def fire_after_creature_added_to_combat(creature: Creature, combat: CombatState) -> None:
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        listener.after_creature_added_to_combat(owner, creature, combat)
    for modifier in _iter_modifier_listeners(combat):
        modifier.after_creature_added_to_combat(creature, combat)


def fire_before_combat_start(combat: CombatState) -> None:
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        listener.before_combat_start(owner, combat)
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        listener.before_combat_start_late(owner, combat)


def fire_after_energy_reset(combat: CombatState, reset_owner: Creature | None = None) -> None:
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        if reset_owner is not None and owner is not reset_owner:
            continue
        listener.after_energy_reset(owner, combat)
        if not is_relic_listener(listener):
            should_remove = listener.amount == 0 if listener.allow_negative else listener.amount <= 0
            if should_remove and owner.powers.get(listener.power_id) is listener:
                combat._remove_power(owner, listener.power_id)
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        if reset_owner is not None and owner is not reset_owner:
            continue
        listener.after_energy_reset_late(owner, combat)
    if reset_owner is not None:
        combat.mark_energy_reset_this_turn(reset_owner)


def fire_after_energy_spent(owner: Creature, card: object, amount: int, combat: CombatState) -> None:
    for listener_owner, power in _iter_power_listeners(combat):
        power.after_energy_spent(listener_owner, card, amount, combat)


def modify_shuffle_order(
    combat: CombatState,
    owner: Creature,
    cards: list,
    *,
    is_initial_shuffle: bool,
) -> None:
    """C# Hook.ModifyShuffleOrder (Hook.cs:2004-2009).

    Called with ``is_initial_shuffle=True`` right after the combat-start
    randomization (C# CardPile.cs:79) and with False on every standard
    shuffle (C# CardPileCmd.cs:877).  Per Hook.cs:47 this hook intentionally
    has no IsOverOrEnding guard — it must still reach listeners at combat
    end.  v0.107.1 implementations: the PerfectFit enchantment (non-initial
    shuffles only, PerfectFit.cs:10-16); powers/relics are probed for the
    same-named hook for structural parity.
    """
    for creature in combat.all_creatures:
        for power in list(creature.powers.values()):
            modify_order = getattr(power, "modify_shuffle_order", None)
            if callable(modify_order):
                modify_order(creature, cards, is_initial_shuffle)
    for relic_owner, relic in _iter_relic_listeners(combat):
        modify_order = getattr(relic, "modify_shuffle_order", None)
        if callable(modify_order):
            modify_order(relic_owner, cards, is_initial_shuffle)
    enchantment_modify_shuffle_order(cards, is_initial_shuffle=is_initial_shuffle)


def fire_after_shuffle(combat: CombatState, shuffler: Creature | None = None) -> None:
    for owner, listener, is_relic in _iter_hook_listeners(combat):
        if shuffler is not None and owner is not shuffler:
            continue
        if is_relic:
            listener.after_shuffle(owner, combat)
            continue
        on_shuffle = getattr(listener, "on_shuffle", None)
        if callable(on_shuffle):
            on_shuffle(owner, combat)


def fire_after_hand_emptied(combat: CombatState) -> None:
    for owner, relic in _iter_relic_listeners(combat):
        relic.after_hand_emptied(owner, combat)


def fire_before_damage_received(
    target: Creature, dealer: Creature | None, damage: int, props: ValueProp, combat: CombatState
) -> None:
    """Thorns, FlameBarrier fire here."""
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        listener.before_damage_received(owner, target, dealer, damage, props, combat)


def fire_after_damage_received(
    target: Creature, dealer: Creature | None, damage: int, props: ValueProp, combat: CombatState
) -> None:
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        listener.after_damage_received(owner, target, dealer, damage, props, combat)


def fire_after_current_hp_changed(
    creature: Creature,
    delta: int,
    combat: CombatState,
) -> None:
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        listener.after_current_hp_changed(owner, creature, delta, combat)


def fire_after_damage_given(
    dealer: Creature, target: Creature, damage: int, props: ValueProp, combat: CombatState
) -> None:
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        listener.after_damage_given(owner, dealer, target, damage, props, combat)


def fire_before_block_gained(
    creature: Creature,
    amount: int,
    combat: CombatState,
    props: ValueProp | None = None,
    card_source: object | None = None,
) -> None:
    """C# Hook.BeforeBlockGained (Hook.cs:131-137), fired from
    CreatureCmd.GainBlock before Hook.ModifyBlock (CreatureCmd.cs:642).

    Skeleton dispatch: no v0.107.1 listener implements BeforeBlockGained, so
    listeners are probed with getattr instead of hard requirements on the
    base classes.
    """
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        before_block_gained = getattr(listener, "before_block_gained", None)
        if callable(before_block_gained):
            before_block_gained(owner, creature, amount, props, card_source)


def fire_after_block_gained(
    creature: Creature,
    amount: int,
    combat: CombatState,
    props: ValueProp | None = None,
    card_play: object | None = None,
) -> None:
    if amount > 0 and props is not None:
        combat.record_block_gained_event(creature, props, card_play)
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        listener.after_block_gained(owner, creature, amount, combat)


def fire_after_combat_victory(combat: CombatState) -> None:
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        listener.after_combat_victory_early(owner, combat)
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        listener.after_combat_victory(owner, combat)


def fire_after_forge(
    combat: CombatState,
    amount: int,
    forger: Creature,
    source: object | None,
) -> None:
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        listener.after_forge(owner, amount, forger, source, combat)


def fire_after_combat_end(combat: CombatState) -> None:
    for owner, listener, _is_relic in _iter_hook_listeners(combat):
        listener.after_combat_end(owner, combat)


def fire_after_taking_extra_turn(combat: CombatState) -> None:
    for owner, relic in _iter_relic_listeners(combat):
        if relic.should_take_extra_turn(owner, combat):
            relic.after_taking_extra_turn(owner, combat)
