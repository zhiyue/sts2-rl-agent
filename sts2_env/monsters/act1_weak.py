"""Act 1 weak monsters: Nibbit, ShrinkerBeetle, FuzzyWurmCrawler, Slimes.

All HP ranges and damage values verified against decompiled C# source.
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from sts2_env.core.creature import Creature
from sts2_env.core.enums import CombatSide, MoveRepeatType, PowerId, ValueProp
from sts2_env.core.damage import calculate_damage, apply_damage
from sts2_env.core.rng import Rng
from sts2_env.monsters.intents import (
    Intent, IntentType, attack_intent, buff_intent,
    debuff_intent, strong_debuff_intent, status_intent,
)
from sts2_env.monsters.state_machine import (
    ConditionalBranchState, MonsterAI, MonsterState, MoveState, RandomBranchState,
)
from sts2_env.cards.status import make_slimed

if TYPE_CHECKING:
    from sts2_env.core.combat import CombatState


# ---- Helpers ----

def _deal_damage_to_player(combat: CombatState, creature: Creature, base_dmg: int, hits: int = 1) -> None:
    """Monster deals powered damage to player."""
    for _ in range(hits):
        if combat.player.is_dead:
            break
        dmg = calculate_damage(base_dmg, creature, combat.player, ValueProp.MOVE, combat)
        apply_damage(combat.player, dmg, ValueProp.MOVE, combat, creature)


def _gain_block(creature: Creature, amount: int) -> None:
    creature.gain_block(amount)


# ---- ShrinkerBeetle (HP 38-40) ----
# Cycle: SHRINK → CHOMP → STOMP → CHOMP → STOMP...

def create_shrinker_beetle(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(38, 40)
    creature = Creature(max_hp=hp, monster_id="SHRINKER_BEETLE")

    def shrink(combat: CombatState) -> None:
        combat.apply_power_to(combat.player, PowerId.SHRINK, 1)

    def chomp(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, 7)

    def stomp(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, 13)

    states: dict[str, MonsterState] = {
        "SHRINK": MoveState("SHRINK", shrink, [strong_debuff_intent()], follow_up_id="CHOMP"),
        "CHOMP": MoveState("CHOMP", chomp, [attack_intent(7)], follow_up_id="STOMP"),
        "STOMP": MoveState("STOMP", stomp, [attack_intent(13)], follow_up_id="CHOMP"),
    }
    return creature, MonsterAI(states, "SHRINK")


# ---- FuzzyWurmCrawler (HP 55-57) ----
# Cycle: ACID_GOOP → INHALE → ACID_GOOP → ACID_GOOP → INHALE...

def create_fuzzy_wurm_crawler(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(55, 57)
    creature = Creature(max_hp=hp, monster_id="FUZZY_WURM_CRAWLER")

    def acid_goop(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, 4)

    def inhale(combat: CombatState) -> None:
        creature.apply_power(PowerId.STRENGTH, 7)

    states: dict[str, MonsterState] = {
        "FIRST_ACID_GOOP": MoveState("FIRST_ACID_GOOP", acid_goop, [attack_intent(4)], follow_up_id="INHALE"),
        "INHALE": MoveState("INHALE", inhale, [buff_intent()], follow_up_id="ACID_GOOP"),
        "ACID_GOOP": MoveState("ACID_GOOP", acid_goop, [attack_intent(4)], follow_up_id="FIRST_ACID_GOOP"),
    }
    return creature, MonsterAI(states, "FIRST_ACID_GOOP")


# ---- Nibbit (HP 42-46) ----
# Conditional start based on IsAlone/IsFront, then fixed rotation:
# BUTT(12) → SLICE(6+5blk) → HISS(Str+2) → BUTT...

def create_nibbit(rng: Rng, is_alone: bool = True, is_front: bool = False) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(42, 46)
    creature = Creature(max_hp=hp, monster_id="NIBBIT")

    def butt(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, 12)

    def slice_move(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, 6)
        _gain_block(creature, 5)

    def hiss(combat: CombatState) -> None:
        creature.apply_power(PowerId.STRENGTH, 2)

    states: dict[str, MonsterState] = {
        "BUTT": MoveState("BUTT", butt, [attack_intent(12)], follow_up_id="SLICE"),
        "SLICE": MoveState("SLICE", slice_move, [attack_intent(6)], follow_up_id="HISS"),
        "HISS": MoveState("HISS", hiss, [buff_intent()], follow_up_id="BUTT"),
    }

    # Determine starting state
    if is_alone:
        initial = "BUTT"
    elif is_front:
        initial = "SLICE"
    else:
        initial = "HISS"

    cond = ConditionalBranchState("INITIAL")
    cond.add_branch(lambda: is_alone, "BUTT")
    cond.add_branch(lambda: is_front, "SLICE")
    cond.add_branch(lambda: True, "HISS")
    states["INITIAL"] = cond

    return creature, MonsterAI(states, initial)


# ---- LeafSlimeS (HP 11-15) ----
# Random: BUTT(3) or GOOP(add Slimed), CannotRepeat

def create_leaf_slime_s(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(11, 15)
    creature = Creature(max_hp=hp, monster_id="LEAF_SLIME_S")

    def butt(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, 3)

    def goop(combat: CombatState) -> None:
        combat.add_card_to_discard(make_slimed())

    rand = RandomBranchState("RANDOM")
    rand.add_branch("BUTT", MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch("GOOP", MoveRepeatType.CANNOT_REPEAT)

    states: dict[str, MonsterState] = {
        "RANDOM": rand,
        "BUTT": MoveState("BUTT", butt, [attack_intent(3)], follow_up_id="RANDOM"),
        "GOOP": MoveState("GOOP", goop, [status_intent()], follow_up_id="RANDOM"),
    }
    return creature, MonsterAI(states, "RANDOM")


# ---- TwigSlimeS (HP 7-11) ----
# BUTT(4) → BUTT(4) (self loop)

def create_twig_slime_s(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(7, 11)
    creature = Creature(max_hp=hp, monster_id="TWIG_SLIME_S")

    def butt(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, 4)

    states: dict[str, MonsterState] = {
        "BUTT": MoveState("BUTT", butt, [attack_intent(4)], follow_up_id="BUTT"),
    }
    return creature, MonsterAI(states, "BUTT")


# ---- LeafSlimeM (HP 32-35) ----
# Strict alternation: STICKY_SHOT(add 2 Slimed) → CLUMP_SHOT(8) → STICKY_SHOT...

def create_leaf_slime_m(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(32, 35)
    creature = Creature(max_hp=hp, monster_id="LEAF_SLIME_M")

    def sticky_shot(combat: CombatState) -> None:
        for _ in range(2):
            combat.add_card_to_discard(make_slimed())

    def clump_shot(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, 8)

    states: dict[str, MonsterState] = {
        "STICKY_SHOT": MoveState("STICKY_SHOT", sticky_shot, [status_intent()], follow_up_id="CLUMP_SHOT"),
        "CLUMP_SHOT": MoveState("CLUMP_SHOT", clump_shot, [attack_intent(8)], follow_up_id="STICKY_SHOT"),
    }
    return creature, MonsterAI(states, "STICKY_SHOT")


# ---- TwigSlimeM (HP 26-28) ----
# STICKY_SHOT → Random(CLUMP_SHOT[max 2 consec] | STICKY_SHOT[CannotRepeat])

def create_twig_slime_m(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(26, 28)
    creature = Creature(max_hp=hp, monster_id="TWIG_SLIME_M")

    def sticky_shot(combat: CombatState) -> None:
        combat.add_card_to_discard(make_slimed())

    def clump_shot(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, 11)

    rand = RandomBranchState("RANDOM")
    rand.add_branch("CLUMP_SHOT", MoveRepeatType.CAN_REPEAT_X_TIMES, max_times=2)
    rand.add_branch("STICKY_SHOT", MoveRepeatType.CANNOT_REPEAT)

    states: dict[str, MonsterState] = {
        "RANDOM": rand,
        "STICKY_SHOT": MoveState("STICKY_SHOT", sticky_shot, [status_intent()], follow_up_id="RANDOM"),
        "CLUMP_SHOT": MoveState("CLUMP_SHOT", clump_shot, [attack_intent(11)], follow_up_id="RANDOM"),
    }
    return creature, MonsterAI(states, "STICKY_SHOT")
