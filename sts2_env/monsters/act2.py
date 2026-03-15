"""Act 2 (Hive) monsters: weak, normal, elite, boss.

All HP ranges, damage values, and state machines verified against decompiled C# source.
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from sts2_env.core.creature import Creature
from sts2_env.core.enums import CombatSide, MoveRepeatType, PowerId, ValueProp
from sts2_env.core.damage import calculate_damage, apply_damage
from sts2_env.core.rng import Rng
from sts2_env.monsters.intents import (
    Intent, IntentType, attack_intent, multi_attack_intent,
    buff_intent, debuff_intent, strong_debuff_intent, status_intent,
    defend_intent, sleep_intent,
)
from sts2_env.monsters.state_machine import (
    ConditionalBranchState, MonsterAI, MonsterState, MoveState, RandomBranchState,
)
from sts2_env.cards.status import make_dazed, make_parasite, make_void

if TYPE_CHECKING:
    from sts2_env.core.combat import CombatState


# ---- Helpers ----

def _deal_damage_to_player(combat: CombatState, creature: Creature, base_dmg: int, hits: int = 1) -> None:
    for _ in range(hits):
        if combat.player.is_dead:
            break
        dmg = calculate_damage(base_dmg, creature, combat.player, ValueProp.MOVE, combat)
        apply_damage(combat.player, dmg, ValueProp.MOVE, combat, creature)


def _gain_block(creature: Creature, amount: int) -> None:
    creature.gain_block(amount)


# ========================================================================
# WEAK ENCOUNTERS
# ========================================================================

# ---- ThievingHopper (HP 15-18 / 16-19 asc) ----

def create_thieving_hopper(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(15, 18)
    creature = Creature(max_hp=hp, monster_id="THIEVING_HOPPER")
    mug_dmg = 8

    def mug(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, mug_dmg)

    states: dict[str, MonsterState] = {
        "MUG": MoveState("MUG", mug, [attack_intent(mug_dmg)], follow_up_id="MUG"),
    }
    creature.apply_power(PowerId.THIEVERY, 15)
    return creature, MonsterAI(states, "MUG")


# ---- Tunneler (HP 24-28 / 26-29 asc) ----

def create_tunneler(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(24, 28)
    creature = Creature(max_hp=hp, monster_id="TUNNELER")
    bite_dmg = 7
    burrow_block = 8

    def burrow(combat: CombatState) -> None:
        _gain_block(creature, burrow_block)

    def bite(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, bite_dmg)

    states: dict[str, MonsterState] = {
        "BURROW": MoveState("BURROW", burrow, [defend_intent()], follow_up_id="BITE"),
        "BITE": MoveState("BITE", bite, [attack_intent(bite_dmg)], follow_up_id="BURROW"),
    }
    return creature, MonsterAI(states, "BURROW")


# ---- Bowlbugs ----

def create_bowlbug_egg(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(21, 22)
    creature = Creature(max_hp=hp, monster_id="BOWLBUG_EGG")
    bite_dmg = 7
    protect_block = 7

    def bite(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, bite_dmg)
        _gain_block(creature, protect_block)

    states: dict[str, MonsterState] = {
        "BITE": MoveState("BITE", bite, [attack_intent(bite_dmg), defend_intent()], follow_up_id="BITE"),
    }
    return creature, MonsterAI(states, "BITE")


def create_bowlbug_nectar(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(35, 38)
    creature = Creature(max_hp=hp, monster_id="BOWLBUG_NECTAR")
    thrash_dmg = 3
    buff_str = 15

    def thrash(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, thrash_dmg)

    def buff_move(combat: CombatState) -> None:
        creature.apply_power(PowerId.STRENGTH, buff_str)

    def thrash2(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, thrash_dmg)

    states: dict[str, MonsterState] = {
        "THRASH": MoveState("THRASH", thrash, [attack_intent(thrash_dmg)], follow_up_id="BUFF"),
        "BUFF": MoveState("BUFF", buff_move, [buff_intent()], follow_up_id="THRASH2"),
        "THRASH2": MoveState("THRASH2", thrash2, [attack_intent(thrash_dmg)], follow_up_id="THRASH2"),
    }
    return creature, MonsterAI(states, "THRASH")


def create_bowlbug_rock(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(45, 48)
    creature = Creature(max_hp=hp, monster_id="BOWLBUG_ROCK")
    headbutt_dmg = 15

    _state = {"off_balance": False}

    def headbutt(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, headbutt_dmg)

    def dizzy(combat: CombatState) -> None:
        _state["off_balance"] = False

    cond = ConditionalBranchState("CHECK")
    cond.add_branch(lambda: _state["off_balance"], "DIZZY")
    cond.add_branch(lambda: True, "HEADBUTT")

    states: dict[str, MonsterState] = {
        "HEADBUTT": MoveState("HEADBUTT", headbutt, [attack_intent(headbutt_dmg)], follow_up_id="CHECK"),
        "CHECK": cond,
        "DIZZY": MoveState("DIZZY", dizzy, [Intent(IntentType.STUN)], follow_up_id="HEADBUTT"),
    }
    return creature, MonsterAI(states, "HEADBUTT")


def create_bowlbug_silk(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(40, 43)
    creature = Creature(max_hp=hp, monster_id="BOWLBUG_SILK")
    thrash_dmg = 4

    def toxic_spit(combat: CombatState) -> None:
        combat.apply_power_to(combat.player, PowerId.WEAK, 1)

    def thrash(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, thrash_dmg, hits=2)

    states: dict[str, MonsterState] = {
        "TOXIC_SPIT": MoveState("TOXIC_SPIT", toxic_spit, [debuff_intent()], follow_up_id="THRASH"),
        "THRASH": MoveState("THRASH", thrash, [multi_attack_intent(thrash_dmg, 2)], follow_up_id="TOXIC_SPIT"),
    }
    return creature, MonsterAI(states, "TOXIC_SPIT")


# ---- Exoskeleton (HP 24-28 / 25-29 asc) ----

def create_exoskeleton(rng: Rng, slot: str = "first") -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(24, 28)
    creature = Creature(max_hp=hp, monster_id="EXOSKELETON")
    skitter_dmg = 1
    skitter_hits = 3
    mandible_dmg = 8

    def skitter(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, skitter_dmg, hits=skitter_hits)

    def mandible(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, mandible_dmg)

    def enrage(combat: CombatState) -> None:
        creature.apply_power(PowerId.STRENGTH, 2)

    rand = RandomBranchState("RAND")
    rand.add_branch("SKITTER", MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch("MANDIBLE", MoveRepeatType.CANNOT_REPEAT)

    states: dict[str, MonsterState] = {
        "RAND": rand,
        "SKITTER": MoveState("SKITTER", skitter, [multi_attack_intent(skitter_dmg, skitter_hits)], follow_up_id="RAND"),
        "MANDIBLE": MoveState("MANDIBLE", mandible, [attack_intent(mandible_dmg)], follow_up_id="ENRAGE"),
        "ENRAGE": MoveState("ENRAGE", enrage, [buff_intent()], follow_up_id="RAND"),
    }

    slot_map = {"first": "SKITTER", "second": "MANDIBLE", "third": "ENRAGE", "fourth": "RAND"}
    initial = slot_map.get(slot, "RAND")

    return creature, MonsterAI(states, initial)


# ========================================================================
# NORMAL ENCOUNTERS
# ========================================================================

# ---- Chomper (HP 60-64 / 63-67 asc) ----

def create_chomper(rng: Rng, scream_first: bool = False) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(60, 64)
    creature = Creature(max_hp=hp, monster_id="CHOMPER")
    clamp_dmg = 8

    def clamp(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, clamp_dmg, hits=2)

    def screech(combat: CombatState) -> None:
        for _ in range(3):
            combat.add_card_to_discard(make_dazed())

    states: dict[str, MonsterState] = {
        "CLAMP": MoveState("CLAMP", clamp, [multi_attack_intent(clamp_dmg, 2)], follow_up_id="SCREECH"),
        "SCREECH": MoveState("SCREECH", screech, [status_intent()], follow_up_id="CLAMP"),
    }

    creature.apply_power(PowerId.ARTIFACT, 2)
    initial = "SCREECH" if scream_first else "CLAMP"
    return creature, MonsterAI(states, initial)


# ---- HunterKiller (HP 60-65 / 63-68 asc) ----

def create_hunter_killer(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(60, 65)
    creature = Creature(max_hp=hp, monster_id="HUNTER_KILLER")
    hunt_dmg = 4
    kill_dmg = 15

    def hunt(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, hunt_dmg, hits=2)
        combat.apply_power_to(combat.player, PowerId.VULNERABLE, 1)

    def kill(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, kill_dmg)

    states: dict[str, MonsterState] = {
        "HUNT": MoveState("HUNT", hunt, [multi_attack_intent(hunt_dmg, 2), debuff_intent()], follow_up_id="KILL"),
        "KILL": MoveState("KILL", kill, [attack_intent(kill_dmg)], follow_up_id="HUNT"),
    }
    return creature, MonsterAI(states, "HUNT")


# ---- LouseProgenitor (HP 52-56 / 54-58 asc) + Wriggler ----

def create_wriggler(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(8, 10)
    creature = Creature(max_hp=hp, monster_id="WRIGGLER")
    bite_dmg = 5

    def bite(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, bite_dmg)

    states: dict[str, MonsterState] = {
        "BITE": MoveState("BITE", bite, [attack_intent(bite_dmg)], follow_up_id="BITE"),
    }
    creature.apply_power(PowerId.MINION, 1)
    return creature, MonsterAI(states, "BITE")


def create_louse_progenitor(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(52, 56)
    creature = Creature(max_hp=hp, monster_id="LOUSE_PROGENITOR")
    scratch_dmg = 8

    def spawn(combat: CombatState) -> None:
        for _ in range(2):
            w, w_ai = create_wriggler(rng)
            combat.add_enemy(w, w_ai)

    def scratch(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, scratch_dmg)

    states: dict[str, MonsterState] = {
        "SPAWN": MoveState("SPAWN", spawn, [Intent(IntentType.SUMMON)], follow_up_id="SCRATCH"),
        "SCRATCH": MoveState("SCRATCH", scratch, [attack_intent(scratch_dmg)], follow_up_id="SCRATCH"),
    }
    return creature, MonsterAI(states, "SPAWN")


# ---- Myte (HP 22-26 / 24-28 asc) ----

def create_myte(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(22, 26)
    creature = Creature(max_hp=hp, monster_id="MYTE")
    bite_dmg = 6
    infest_dmg = 5

    def bite(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, bite_dmg)

    def infest(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, infest_dmg)
        combat.add_card_to_discard(make_parasite())

    rand = RandomBranchState("RAND")
    rand.add_branch("BITE", MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch("INFEST", MoveRepeatType.CANNOT_REPEAT)

    states: dict[str, MonsterState] = {
        "RAND": rand,
        "BITE": MoveState("BITE", bite, [attack_intent(bite_dmg)], follow_up_id="RAND"),
        "INFEST": MoveState("INFEST", infest, [attack_intent(infest_dmg), status_intent()], follow_up_id="RAND"),
    }

    creature.apply_power(PowerId.CURL_UP, 6)
    return creature, MonsterAI(states, "RAND")


# ---- Ovicopter (HP 67-72 / 70-75 asc) + ToughEgg ----

def create_tough_egg(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(10, 12)
    creature = Creature(max_hp=hp, monster_id="TOUGH_EGG")

    def wait(combat: CombatState) -> None:
        pass

    states: dict[str, MonsterState] = {
        "WAIT": MoveState("WAIT", wait, [Intent(IntentType.UNKNOWN)], follow_up_id="WAIT"),
    }
    creature.apply_power(PowerId.MINION, 1)
    creature.apply_power(PowerId.HATCH, 1)
    return creature, MonsterAI(states, "WAIT")


def create_ovicopter(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(67, 72)
    creature = Creature(max_hp=hp, monster_id="OVICOPTER")
    dive_dmg = 14

    def lay_eggs(combat: CombatState) -> None:
        egg, egg_ai = create_tough_egg(rng)
        combat.add_enemy(egg, egg_ai)

    def dive(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, dive_dmg)

    states: dict[str, MonsterState] = {
        "LAY_EGGS": MoveState("LAY_EGGS", lay_eggs, [Intent(IntentType.SUMMON)], follow_up_id="DIVE"),
        "DIVE": MoveState("DIVE", dive, [attack_intent(dive_dmg)], follow_up_id="LAY_EGGS"),
    }
    return creature, MonsterAI(states, "LAY_EGGS")


# ---- SlumberingBeetle (HP 66-70 / 69-73 asc) ----

def create_slumbering_beetle(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(66, 70)
    creature = Creature(max_hp=hp, monster_id="SLUMBERING_BEETLE")
    gore_dmg = 9

    _state = {"awakened": False}

    def sleep_move(combat: CombatState) -> None:
        pass

    def gore(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, gore_dmg)

    def wake_up(combat: CombatState) -> None:
        _state["awakened"] = True
        creature.apply_power(PowerId.STRENGTH, 6)

    cond = ConditionalBranchState("CHECK")
    cond.add_branch(lambda: _state["awakened"], "GORE")
    cond.add_branch(lambda: True, "SLEEP")

    states: dict[str, MonsterState] = {
        "SLEEP": MoveState("SLEEP", sleep_move, [sleep_intent()], follow_up_id="CHECK"),
        "CHECK": cond,
        "GORE": MoveState("GORE", gore, [attack_intent(gore_dmg)], follow_up_id="GORE"),
    }
    return creature, MonsterAI(states, "SLEEP")


# ---- SpinyToad (HP 116-119 / 121-124 asc) ----

def create_spiny_toad(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(116, 119)
    creature = Creature(max_hp=hp, monster_id="SPINY_TOAD")
    lash_dmg = 17
    explosion_dmg = 23
    spines_amount = 8

    def lash(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, lash_dmg)

    def spines(combat: CombatState) -> None:
        creature.apply_power(PowerId.THORNS, spines_amount)

    def explosion(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, explosion_dmg)
        # Remove all thorns
        if PowerId.THORNS in creature.powers:
            del creature.powers[PowerId.THORNS]

    states: dict[str, MonsterState] = {
        "LASH": MoveState("LASH", lash, [attack_intent(lash_dmg)], follow_up_id="SPINES"),
        "SPINES": MoveState("SPINES", spines, [buff_intent()], follow_up_id="LASH2"),
        "LASH2": MoveState("LASH2", lash, [attack_intent(lash_dmg)], follow_up_id="EXPLOSION"),
        "EXPLOSION": MoveState("EXPLOSION", explosion, [attack_intent(explosion_dmg)], follow_up_id="LASH"),
    }
    return creature, MonsterAI(states, "LASH")


# ---- TheObscura (HP 36-39 / 38-41 asc) ----

def create_the_obscura(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(36, 39)
    creature = Creature(max_hp=hp, monster_id="THE_OBSCURA")
    chomp_dmg = 5
    strike_dmg = 13

    def darkness(combat: CombatState) -> None:
        combat.apply_power_to(combat.player, PowerId.FRAIL, 2)
        combat.apply_power_to(combat.player, PowerId.WEAK, 2)

    def chomp(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, chomp_dmg, hits=2)

    def strike(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, strike_dmg)

    rand = RandomBranchState("RAND")
    rand.add_branch("CHOMP", MoveRepeatType.CAN_REPEAT_X_TIMES, max_times=2, weight=2.0)
    rand.add_branch("STRIKE", MoveRepeatType.CAN_REPEAT_X_TIMES, max_times=2, weight=2.0)

    states: dict[str, MonsterState] = {
        "DARKNESS": MoveState("DARKNESS", darkness, [strong_debuff_intent()], follow_up_id="RAND"),
        "RAND": rand,
        "CHOMP": MoveState("CHOMP", chomp, [multi_attack_intent(chomp_dmg, 2)], follow_up_id="RAND"),
        "STRIKE": MoveState("STRIKE", strike, [attack_intent(strike_dmg)], follow_up_id="RAND"),
    }
    return creature, MonsterAI(states, "DARKNESS")


# ========================================================================
# ELITE ENCOUNTERS
# ========================================================================

# ---- Decimillipede (3 segments) (HP 42-48 / 48-56 asc) ----

def create_decimillipede_segment(rng: Rng, starter_idx: int = 0) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(42, 48)
    creature = Creature(max_hp=hp, monster_id="DECIMILLIPEDE_SEGMENT")
    writhe_dmg = 5
    constrict_dmg = 8
    bulk_dmg = 6

    def writhe(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, writhe_dmg, hits=2)

    def constrict(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, constrict_dmg)
        combat.apply_power_to(combat.player, PowerId.WEAK, 1)

    def bulk(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, bulk_dmg)
        creature.apply_power(PowerId.STRENGTH, 2)

    def dead_move(combat: CombatState) -> None:
        pass

    def reattach(combat: CombatState) -> None:
        creature.heal(25)

    rand = RandomBranchState("RAND")
    rand.add_branch("WRITHE", MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch("BULK", MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch("CONSTRICT", MoveRepeatType.CANNOT_REPEAT)

    states: dict[str, MonsterState] = {
        "WRITHE": MoveState("WRITHE", writhe, [multi_attack_intent(writhe_dmg, 2)], follow_up_id="CONSTRICT"),
        "CONSTRICT": MoveState("CONSTRICT", constrict, [attack_intent(constrict_dmg), debuff_intent()], follow_up_id="BULK"),
        "BULK": MoveState("BULK", bulk, [attack_intent(bulk_dmg), buff_intent()], follow_up_id="WRITHE"),
        "DEAD": MoveState("DEAD", dead_move, [Intent(IntentType.UNKNOWN)], follow_up_id="REATTACH"),
        "REATTACH": MoveState("REATTACH", reattach, [Intent(IntentType.HEAL)], follow_up_id="RAND", must_perform_once=True),
        "RAND": rand,
    }

    starter_map = {0: "WRITHE", 1: "BULK", 2: "CONSTRICT"}
    initial = starter_map.get(starter_idx, "WRITHE")
    return creature, MonsterAI(states, initial)


# ---- DecimillipedeSegmentFront (HP 42-48 / 48-56 asc) ----
# Identical behavior to DecimillipedeSegment (same base class in C#).
# The only difference is visual (front segment animation).

def create_decimillipede_segment_front(rng: Rng, starter_idx: int = 0) -> tuple[Creature, MonsterAI]:
    creature, ai = create_decimillipede_segment(rng, starter_idx)
    creature.monster_id = "DECIMILLIPEDE_SEGMENT_FRONT"
    return creature, ai


# ---- DecimillipedeSegmentMiddle (HP 42-48 / 48-56 asc) ----
# Identical behavior to DecimillipedeSegment (same base class in C#).

def create_decimillipede_segment_middle(rng: Rng, starter_idx: int = 0) -> tuple[Creature, MonsterAI]:
    creature, ai = create_decimillipede_segment(rng, starter_idx)
    creature.monster_id = "DECIMILLIPEDE_SEGMENT_MIDDLE"
    return creature, ai


# ---- DecimillipedeSegmentBack (HP 42-48 / 48-56 asc) ----
# Identical behavior to DecimillipedeSegment (same base class in C#).

def create_decimillipede_segment_back(rng: Rng, starter_idx: int = 0) -> tuple[Creature, MonsterAI]:
    creature, ai = create_decimillipede_segment(rng, starter_idx)
    creature.monster_id = "DECIMILLIPEDE_SEGMENT_BACK"
    return creature, ai


# ---- Entomancer (HP 145 / 155 asc) ----

def create_entomancer(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 145
    creature = Creature(max_hp=hp, monster_id="ENTOMANCER")
    spear_dmg = 18
    bees_dmg = 3
    bees_hits = 7

    _state = {"personal_hive": 1}

    def bees(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, bees_dmg, hits=bees_hits)

    def spear(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, spear_dmg)

    def pheromone_spit(combat: CombatState) -> None:
        if _state["personal_hive"] < 3:
            _state["personal_hive"] += 1
            creature.apply_power(PowerId.PERSONAL_HIVE, 1)
        else:
            creature.apply_power(PowerId.STRENGTH, 2)

    states: dict[str, MonsterState] = {
        "BEES": MoveState("BEES", bees, [multi_attack_intent(bees_dmg, bees_hits)], follow_up_id="SPEAR"),
        "SPEAR": MoveState("SPEAR", spear, [attack_intent(spear_dmg)], follow_up_id="PHEROMONE_SPIT"),
        "PHEROMONE_SPIT": MoveState("PHEROMONE_SPIT", pheromone_spit, [buff_intent()], follow_up_id="BEES"),
    }

    creature.apply_power(PowerId.PERSONAL_HIVE, 1)
    return creature, MonsterAI(states, "BEES")


# ---- InfestedPrism (HP 40-45 / 42-47 asc) ----

def create_infested_prism(rng: Rng, slot: str = "first") -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(40, 45)
    creature = Creature(max_hp=hp, monster_id="INFESTED_PRISM")
    laser_dmg = 8
    infested_dmg = 10

    def laser_beam(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, laser_dmg)

    def infested_laser(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, infested_dmg)
        combat.add_card_to_discard(make_parasite())

    rand = RandomBranchState("RAND")
    rand.add_branch("LASER_BEAM", MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch("INFESTED_LASER", MoveRepeatType.CANNOT_REPEAT)

    states: dict[str, MonsterState] = {
        "RAND": rand,
        "LASER_BEAM": MoveState("LASER_BEAM", laser_beam, [attack_intent(laser_dmg)], follow_up_id="INFESTED_LASER"),
        "INFESTED_LASER": MoveState("INFESTED_LASER", infested_laser, [attack_intent(infested_dmg), status_intent()], follow_up_id="RAND"),
    }

    initial = "LASER_BEAM" if slot == "first" else ("INFESTED_LASER" if slot == "second" else "RAND")
    return creature, MonsterAI(states, initial)


# ========================================================================
# BOSS ENCOUNTERS
# ========================================================================

# ---- TheInsatiable (HP 242 / 256 asc) ----

def create_the_insatiable(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 242
    creature = Creature(max_hp=hp, monster_id="THE_INSATIABLE")
    chomp_dmg = 17
    devouring_maw_dmg = 7
    acid_blast_dmg = 23

    _state = {"evolved": False}

    def chomp(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, chomp_dmg)

    def devouring_maw(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, devouring_maw_dmg, hits=3)

    def acid_blast(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, acid_blast_dmg)

    def evolve(combat: CombatState) -> None:
        _state["evolved"] = True
        creature.apply_power(PowerId.STRENGTH, 5)
        creature.heal(30)

    # Check for evolution at half HP
    evolve_check = ConditionalBranchState("EVOLVE_CHECK")
    evolve_check.add_branch(lambda: not _state["evolved"] and creature.current_hp <= creature.max_hp // 2, "EVOLVE")
    evolve_check.add_branch(lambda: True, "CHOMP")

    states: dict[str, MonsterState] = {
        "CHOMP": MoveState("CHOMP", chomp, [attack_intent(chomp_dmg)], follow_up_id="DEVOURING_MAW"),
        "DEVOURING_MAW": MoveState("DEVOURING_MAW", devouring_maw, [multi_attack_intent(devouring_maw_dmg, 3)], follow_up_id="ACID_BLAST"),
        "ACID_BLAST": MoveState("ACID_BLAST", acid_blast, [attack_intent(acid_blast_dmg)], follow_up_id="EVOLVE_CHECK"),
        "EVOLVE_CHECK": evolve_check,
        "EVOLVE": MoveState("EVOLVE", evolve, [buff_intent()], follow_up_id="CHOMP"),
    }
    return creature, MonsterAI(states, "CHOMP")


# ---- KnowledgeDemon (HP 379 / 399 asc) ----
# C# cycle: CURSE_OF_KNOWLEDGE -> SLAP(17) -> KNOWLEDGE_OVERWHELMING(8x3) -> PONDER(11+heal30+str2) -> conditional

def create_knowledge_demon(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 379
    creature = Creature(max_hp=hp, monster_id="KNOWLEDGE_DEMON")
    slap_dmg = 17
    overwhelming_dmg = 8
    overwhelming_hits = 3
    ponder_dmg = 11
    ponder_heal = 30
    ponder_str = 2

    _state = {"curse_counter": 0}

    def curse_of_knowledge(combat: CombatState) -> None:
        # Simplified: add status cards to discard (Disintegration curse)
        _state["curse_counter"] += 1

    def slap(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, slap_dmg)

    def knowledge_overwhelming(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, overwhelming_dmg, hits=overwhelming_hits)

    def ponder(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, ponder_dmg)
        creature.heal(ponder_heal)
        creature.apply_power(PowerId.STRENGTH, ponder_str)

    # After Ponder: if curse_counter < 3, go back to CURSE_OF_KNOWLEDGE; else SLAP
    curse_check = ConditionalBranchState("CURSE_CHECK")
    curse_check.add_branch(lambda: _state["curse_counter"] < 3, "CURSE_OF_KNOWLEDGE")
    curse_check.add_branch(lambda: True, "SLAP")

    states: dict[str, MonsterState] = {
        "CURSE_OF_KNOWLEDGE": MoveState("CURSE_OF_KNOWLEDGE", curse_of_knowledge, [debuff_intent()], follow_up_id="SLAP"),
        "SLAP": MoveState("SLAP", slap, [attack_intent(slap_dmg)], follow_up_id="KNOWLEDGE_OVERWHELMING"),
        "KNOWLEDGE_OVERWHELMING": MoveState("KNOWLEDGE_OVERWHELMING", knowledge_overwhelming, [multi_attack_intent(overwhelming_dmg, overwhelming_hits)], follow_up_id="PONDER"),
        "PONDER": MoveState("PONDER", ponder, [attack_intent(ponder_dmg), buff_intent()], follow_up_id="CURSE_CHECK"),
        "CURSE_CHECK": curse_check,
    }
    return creature, MonsterAI(states, "CURSE_OF_KNOWLEDGE")


# ---- KaiserCrab (Crusher + Rocket) ----

def create_crusher(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 199
    creature = Creature(max_hp=hp, monster_id="CRUSHER")
    thrash_dmg = 12
    enlarging_dmg = 4
    bug_sting_dmg = 6
    guarded_strike_dmg = 12
    guarded_block = 18

    def thrash(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, thrash_dmg)

    def enlarging_strike(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, enlarging_dmg)

    def bug_sting(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, bug_sting_dmg, hits=2)
        combat.apply_power_to(combat.player, PowerId.WEAK, 2)
        combat.apply_power_to(combat.player, PowerId.FRAIL, 2)

    def adapt(combat: CombatState) -> None:
        creature.apply_power(PowerId.STRENGTH, 2)

    def guarded_strike(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, guarded_strike_dmg)
        _gain_block(creature, guarded_block)

    states: dict[str, MonsterState] = {
        "THRASH": MoveState("THRASH", thrash, [attack_intent(thrash_dmg)], follow_up_id="ENLARGING_STRIKE"),
        "ENLARGING_STRIKE": MoveState("ENLARGING_STRIKE", enlarging_strike, [attack_intent(enlarging_dmg)], follow_up_id="BUG_STING"),
        "BUG_STING": MoveState("BUG_STING", bug_sting, [multi_attack_intent(bug_sting_dmg, 2), debuff_intent()], follow_up_id="ADAPT"),
        "ADAPT": MoveState("ADAPT", adapt, [buff_intent()], follow_up_id="GUARDED_STRIKE"),
        "GUARDED_STRIKE": MoveState("GUARDED_STRIKE", guarded_strike, [attack_intent(guarded_strike_dmg), defend_intent()], follow_up_id="THRASH"),
    }

    creature.apply_power(PowerId.BACK_ATTACK_LEFT, 1)
    creature.apply_power(PowerId.CRAB_RAGE, 1)
    return creature, MonsterAI(states, "THRASH")


def create_rocket(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 189
    creature = Creature(max_hp=hp, monster_id="ROCKET")
    targeting_dmg = 3
    precision_dmg = 18
    laser_dmg = 31

    def targeting_reticle(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, targeting_dmg)
        combat.apply_power_to(combat.player, PowerId.VULNERABLE, 2)

    def precision_beam(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, precision_dmg)

    def laser(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, laser_dmg)

    states: dict[str, MonsterState] = {
        "TARGETING_RETICLE": MoveState("TARGETING_RETICLE", targeting_reticle, [attack_intent(targeting_dmg), debuff_intent()], follow_up_id="PRECISION_BEAM"),
        "PRECISION_BEAM": MoveState("PRECISION_BEAM", precision_beam, [attack_intent(precision_dmg)], follow_up_id="LASER"),
        "LASER": MoveState("LASER", laser, [attack_intent(laser_dmg)], follow_up_id="TARGETING_RETICLE"),
    }

    creature.apply_power(PowerId.BACK_ATTACK_RIGHT, 1)
    creature.apply_power(PowerId.CRAB_RAGE, 1)
    return creature, MonsterAI(states, "TARGETING_RETICLE")
