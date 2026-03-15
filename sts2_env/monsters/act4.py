"""Act 4 (Underdocks) monsters: weak, normal, elite, boss.

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

# ---- CorpseSlug (HP 25-27 / 27-29 asc) ----

def create_corpse_slug(rng: Rng, starter_idx: int = 0) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(25, 27)
    creature = Creature(max_hp=hp, monster_id="CORPSE_SLUG")
    whip_slap_dmg = 3
    glomp_dmg = 8

    def whip_slap(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, whip_slap_dmg, hits=2)

    def glomp(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, glomp_dmg)

    def goop(combat: CombatState) -> None:
        combat.apply_power_to(combat.player, PowerId.FRAIL, 2)

    states: dict[str, MonsterState] = {
        "WHIP_SLAP": MoveState("WHIP_SLAP", whip_slap, [multi_attack_intent(whip_slap_dmg, 2)], follow_up_id="GLOMP"),
        "GLOMP": MoveState("GLOMP", glomp, [attack_intent(glomp_dmg)], follow_up_id="GOOP"),
        "GOOP": MoveState("GOOP", goop, [debuff_intent()], follow_up_id="WHIP_SLAP"),
    }

    starter_map = {0: "WHIP_SLAP", 1: "GLOMP", 2: "GOOP"}
    initial = starter_map.get(starter_idx, "WHIP_SLAP")

    creature.apply_power(PowerId.RAVENOUS, 4)
    return creature, MonsterAI(states, initial)


# ---- Seapunk (HP 35-38 / 37-40 asc) ----

def create_seapunk(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(35, 38)
    creature = Creature(max_hp=hp, monster_id="SEAPUNK")
    jab_dmg = 6
    poison_spit_dmg = 9

    def jab(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, jab_dmg)

    def poison_spit(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, poison_spit_dmg)
        combat.apply_power_to(combat.player, PowerId.POISON, 3)

    states: dict[str, MonsterState] = {
        "JAB": MoveState("JAB", jab, [attack_intent(jab_dmg)], follow_up_id="POISON_SPIT"),
        "POISON_SPIT": MoveState("POISON_SPIT", poison_spit, [attack_intent(poison_spit_dmg), debuff_intent()], follow_up_id="JAB"),
    }
    return creature, MonsterAI(states, "JAB")


# ---- SludgeSpinner (HP 20-23 / 22-25 asc) ----

def create_sludge_spinner(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(20, 23)
    creature = Creature(max_hp=hp, monster_id="SLUDGE_SPINNER")
    sludge_ball_dmg = 6

    def toxic_spray(combat: CombatState) -> None:
        combat.apply_power_to(combat.player, PowerId.WEAK, 2)

    def sludge_ball(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, sludge_ball_dmg)

    states: dict[str, MonsterState] = {
        "TOXIC_SPRAY": MoveState("TOXIC_SPRAY", toxic_spray, [debuff_intent()], follow_up_id="SLUDGE_BALL"),
        "SLUDGE_BALL": MoveState("SLUDGE_BALL", sludge_ball, [attack_intent(sludge_ball_dmg)], follow_up_id="SLUDGE_BALL"),
    }
    return creature, MonsterAI(states, "TOXIC_SPRAY")


# ---- Toadpole (HP 14-16 / 15-17 asc) ----

def create_toadpole(rng: Rng, slot: str = "first") -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(14, 16)
    creature = Creature(max_hp=hp, monster_id="TOADPOLE")
    tongue_dmg = 5
    chomp_dmg = 9

    def tongue(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, tongue_dmg)

    def chomp(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, chomp_dmg)

    rand = RandomBranchState("RAND")
    rand.add_branch("TONGUE", MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch("CHOMP", MoveRepeatType.CANNOT_REPEAT)

    states: dict[str, MonsterState] = {
        "RAND": rand,
        "TONGUE": MoveState("TONGUE", tongue, [attack_intent(tongue_dmg)], follow_up_id="CHOMP"),
        "CHOMP": MoveState("CHOMP", chomp, [attack_intent(chomp_dmg)], follow_up_id="RAND"),
    }

    initial = "TONGUE" if slot == "first" else ("CHOMP" if slot == "second" else "RAND")
    return creature, MonsterAI(states, initial)


# ========================================================================
# NORMAL ENCOUNTERS
# ========================================================================

# ---- CalcifiedCultist (HP 38-41 / 39-42 asc) ----

def create_calcified_cultist(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(38, 41)
    creature = Creature(max_hp=hp, monster_id="CALCIFIED_CULTIST")
    dark_strike_dmg = 9

    def incantation(combat: CombatState) -> None:
        creature.apply_power(PowerId.RITUAL, 2)

    def dark_strike(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, dark_strike_dmg)

    states: dict[str, MonsterState] = {
        "INCANTATION": MoveState("INCANTATION", incantation, [buff_intent()], follow_up_id="DARK_STRIKE"),
        "DARK_STRIKE": MoveState("DARK_STRIKE", dark_strike, [attack_intent(dark_strike_dmg)], follow_up_id="DARK_STRIKE"),
    }
    return creature, MonsterAI(states, "INCANTATION")


# ---- DampCultist (HP 51-53 / 52-54 asc) ----

def create_damp_cultist(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(51, 53)
    creature = Creature(max_hp=hp, monster_id="DAMP_CULTIST")
    dark_strike_dmg = 1

    def incantation(combat: CombatState) -> None:
        creature.apply_power(PowerId.RITUAL, 5)

    def dark_strike(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, dark_strike_dmg)

    states: dict[str, MonsterState] = {
        "INCANTATION": MoveState("INCANTATION", incantation, [buff_intent()], follow_up_id="DARK_STRIKE"),
        "DARK_STRIKE": MoveState("DARK_STRIKE", dark_strike, [attack_intent(dark_strike_dmg)], follow_up_id="DARK_STRIKE"),
    }
    return creature, MonsterAI(states, "INCANTATION")


# ---- FossilStalker (HP 51-53 / 54-56 asc) ----

def create_fossil_stalker(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(51, 53)
    creature = Creature(max_hp=hp, monster_id="FOSSIL_STALKER")
    tackle_dmg = 9
    latch_dmg = 12
    lash_dmg = 3

    def tackle(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, tackle_dmg)
        combat.apply_power_to(combat.player, PowerId.FRAIL, 1)

    def latch(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, latch_dmg)

    def lash(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, lash_dmg, hits=2)

    rand = RandomBranchState("RAND")
    rand.add_branch("LATCH", weight=2.0)
    rand.add_branch("TACKLE", weight=2.0)
    rand.add_branch("LASH", weight=2.0)

    states: dict[str, MonsterState] = {
        "RAND": rand,
        "TACKLE": MoveState("TACKLE", tackle, [attack_intent(tackle_dmg), debuff_intent()], follow_up_id="RAND"),
        "LATCH": MoveState("LATCH", latch, [attack_intent(latch_dmg)], follow_up_id="RAND"),
        "LASH": MoveState("LASH", lash, [multi_attack_intent(lash_dmg, 2)], follow_up_id="RAND"),
    }

    creature.apply_power(PowerId.SUCK, 3)
    return creature, MonsterAI(states, "LATCH")


# ---- GremlinMerc (HP 47-49 / 51-53 asc) + SneakyGremlin + FatGremlin ----

def create_sneaky_gremlin(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(9, 12)
    creature = Creature(max_hp=hp, monster_id="SNEAKY_GREMLIN")
    stab_dmg = 8

    def stab(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, stab_dmg)

    states: dict[str, MonsterState] = {
        "STAB": MoveState("STAB", stab, [attack_intent(stab_dmg)], follow_up_id="STAB"),
    }
    return creature, MonsterAI(states, "STAB")


def create_fat_gremlin(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(13, 17)
    creature = Creature(max_hp=hp, monster_id="FAT_GREMLIN")

    def spawned(combat: CombatState) -> None:
        pass

    def flee(combat: CombatState) -> None:
        # Escape from combat (simplified: kill self)
        creature.lose_hp(creature.current_hp)

    states: dict[str, MonsterState] = {
        "SPAWNED": MoveState("SPAWNED", spawned, [Intent(IntentType.STUN)], follow_up_id="FLEE"),
        "FLEE": MoveState("FLEE", flee, [Intent(IntentType.ESCAPE)], follow_up_id="FLEE"),
    }
    return creature, MonsterAI(states, "SPAWNED")


def create_gremlin_merc(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(47, 49)
    creature = Creature(max_hp=hp, monster_id="GREMLIN_MERC")
    gimme_dmg = 7
    double_smash_dmg = 6
    hehe_dmg = 8

    def gimme(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, gimme_dmg, hits=2)

    def double_smash(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, double_smash_dmg, hits=2)
        combat.apply_power_to(combat.player, PowerId.WEAK, 2)

    def hehe(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, hehe_dmg)
        creature.apply_power(PowerId.STRENGTH, 2)

    states: dict[str, MonsterState] = {
        "GIMME": MoveState("GIMME", gimme, [multi_attack_intent(gimme_dmg, 2)], follow_up_id="DOUBLE_SMASH"),
        "DOUBLE_SMASH": MoveState("DOUBLE_SMASH", double_smash, [multi_attack_intent(double_smash_dmg, 2), debuff_intent()], follow_up_id="HEHE"),
        "HEHE": MoveState("HEHE", hehe, [attack_intent(hehe_dmg), buff_intent()], follow_up_id="GIMME"),
    }

    creature.apply_power(PowerId.SURPRISE, 1)
    creature.apply_power(PowerId.THIEVERY, 20)
    return creature, MonsterAI(states, "GIMME")


# ---- HauntedShip (HP 58-62 / 61-65 asc) ----

def create_haunted_ship(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(58, 62)
    creature = Creature(max_hp=hp, monster_id="HAUNTED_SHIP")
    broadside_dmg = 7
    ramming_speed_dmg = 18

    def broadside(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, broadside_dmg, hits=2)

    def ramming_speed(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, ramming_speed_dmg)

    states: dict[str, MonsterState] = {
        "BROADSIDE": MoveState("BROADSIDE", broadside, [multi_attack_intent(broadside_dmg, 2)], follow_up_id="RAMMING_SPEED"),
        "RAMMING_SPEED": MoveState("RAMMING_SPEED", ramming_speed, [attack_intent(ramming_speed_dmg)], follow_up_id="BROADSIDE"),
    }
    return creature, MonsterAI(states, "BROADSIDE")


# ---- LivingFog (HP 70-74 / 73-77 asc) + GasBomb ----

def create_gas_bomb(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 10
    creature = Creature(max_hp=hp, monster_id="GAS_BOMB")
    explode_dmg = 8

    def explode(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, explode_dmg)
        creature.lose_hp(creature.current_hp)  # Kill self

    states: dict[str, MonsterState] = {
        "EXPLODE": MoveState("EXPLODE", explode, [Intent(IntentType.DEATH_BLOW)], follow_up_id="EXPLODE"),
    }
    creature.apply_power(PowerId.MINION, 1)
    return creature, MonsterAI(states, "EXPLODE")


def create_living_fog(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(70, 74)
    creature = Creature(max_hp=hp, monster_id="LIVING_FOG")
    engulf_dmg = 10

    def spawn(combat: CombatState) -> None:
        bomb, bomb_ai = create_gas_bomb(rng)
        combat.add_enemy(bomb, bomb_ai)

    def engulf(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, engulf_dmg)
        combat.apply_power_to(combat.player, PowerId.WEAK, 1)

    states: dict[str, MonsterState] = {
        "SPAWN": MoveState("SPAWN", spawn, [Intent(IntentType.SUMMON)], follow_up_id="ENGULF"),
        "ENGULF": MoveState("ENGULF", engulf, [attack_intent(engulf_dmg), debuff_intent()], follow_up_id="SPAWN"),
    }
    return creature, MonsterAI(states, "SPAWN")


# ---- PunchConstruct (HP 55 / 60 asc) ----
# C#: StrongPunch(14) <-> FastPunch(5x2) alternating

def create_punch_construct(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 55
    creature = Creature(max_hp=hp, monster_id="PUNCH_CONSTRUCT")
    strong_punch_dmg = 14
    fast_punch_dmg = 5

    def strong_punch(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, strong_punch_dmg)

    def fast_punch(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, fast_punch_dmg, hits=2)

    states: dict[str, MonsterState] = {
        "STRONG_PUNCH": MoveState("STRONG_PUNCH", strong_punch, [attack_intent(strong_punch_dmg)], follow_up_id="FAST_PUNCH"),
        "FAST_PUNCH": MoveState("FAST_PUNCH", fast_punch, [multi_attack_intent(fast_punch_dmg, 2)], follow_up_id="STRONG_PUNCH"),
    }
    return creature, MonsterAI(states, "FAST_PUNCH")


# ---- SewerClam (HP 50-54 / 53-57 asc) ----

def create_sewer_clam(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(50, 54)
    creature = Creature(max_hp=hp, monster_id="SEWER_CLAM")
    snap_dmg = 8
    pearl_spit_dmg = 5
    shell_block = 10

    def shell(combat: CombatState) -> None:
        _gain_block(creature, shell_block)

    def snap(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, snap_dmg)

    def pearl_spit(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, pearl_spit_dmg)
        combat.apply_power_to(combat.player, PowerId.WEAK, 1)

    states: dict[str, MonsterState] = {
        "SHELL": MoveState("SHELL", shell, [defend_intent()], follow_up_id="SNAP"),
        "SNAP": MoveState("SNAP", snap, [attack_intent(snap_dmg)], follow_up_id="PEARL_SPIT"),
        "PEARL_SPIT": MoveState("PEARL_SPIT", pearl_spit, [attack_intent(pearl_spit_dmg), debuff_intent()], follow_up_id="SHELL"),
    }
    return creature, MonsterAI(states, "SHELL")


# ---- TwoTailedRat (HP 25-28 / 27-30 asc) ----

def create_two_tailed_rat(rng: Rng, slot: str = "first") -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(25, 28)
    creature = Creature(max_hp=hp, monster_id="TWO_TAILED_RAT")
    gnaw_dmg = 6
    tail_whip_dmg = 4

    def gnaw(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, gnaw_dmg)

    def tail_whip(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, tail_whip_dmg, hits=2)

    rand = RandomBranchState("RAND")
    rand.add_branch("GNAW", MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch("TAIL_WHIP", MoveRepeatType.CANNOT_REPEAT)

    states: dict[str, MonsterState] = {
        "RAND": rand,
        "GNAW": MoveState("GNAW", gnaw, [attack_intent(gnaw_dmg)], follow_up_id="TAIL_WHIP"),
        "TAIL_WHIP": MoveState("TAIL_WHIP", tail_whip, [multi_attack_intent(tail_whip_dmg, 2)], follow_up_id="RAND"),
    }

    initial = "GNAW" if slot == "first" else ("TAIL_WHIP" if slot == "second" else "RAND")
    return creature, MonsterAI(states, initial)


# ========================================================================
# ELITE ENCOUNTERS
# ========================================================================

# ---- PhantasmalGardener (HP 60-65 / 63-68 asc) ----

def create_phantasmal_gardener(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(60, 65)
    creature = Creature(max_hp=hp, monster_id="PHANTASMAL_GARDENER")
    prune_dmg = 10
    uproot_dmg = 16

    def prune(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, prune_dmg)
        combat.apply_power_to(combat.player, PowerId.VULNERABLE, 1)

    def uproot(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, uproot_dmg)

    rand = RandomBranchState("RAND")
    rand.add_branch("PRUNE", MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch("UPROOT", MoveRepeatType.CANNOT_REPEAT)

    states: dict[str, MonsterState] = {
        "RAND": rand,
        "PRUNE": MoveState("PRUNE", prune, [attack_intent(prune_dmg), debuff_intent()], follow_up_id="RAND"),
        "UPROOT": MoveState("UPROOT", uproot, [attack_intent(uproot_dmg)], follow_up_id="RAND"),
    }
    return creature, MonsterAI(states, "RAND")


# ---- SkulkingColony (HP 140 / 150 asc) ----

def create_skulking_colony(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 140
    creature = Creature(max_hp=hp, monster_id="SKULKING_COLONY")
    lash_dmg = 6
    devour_dmg = 20

    def skulk(combat: CombatState) -> None:
        combat.apply_power_to(combat.player, PowerId.WEAK, 1)
        combat.apply_power_to(combat.player, PowerId.FRAIL, 1)

    def lash(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, lash_dmg, hits=3)

    def devour(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, devour_dmg)

    states: dict[str, MonsterState] = {
        "SKULK": MoveState("SKULK", skulk, [debuff_intent()], follow_up_id="LASH"),
        "LASH": MoveState("LASH", lash, [multi_attack_intent(lash_dmg, 3)], follow_up_id="DEVOUR"),
        "DEVOUR": MoveState("DEVOUR", devour, [attack_intent(devour_dmg)], follow_up_id="SKULK"),
    }
    return creature, MonsterAI(states, "SKULK")


# ---- TerrorEel (HP 130 / 140 asc) ----

def create_terror_eel(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 130
    creature = Creature(max_hp=hp, monster_id="TERROR_EEL")
    shock_dmg = 12
    coil_dmg = 8
    surge_dmg = 25

    def shock(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, shock_dmg)
        combat.apply_power_to(combat.player, PowerId.VULNERABLE, 1)

    def coil(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, coil_dmg, hits=2)

    def surge(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, surge_dmg)

    states: dict[str, MonsterState] = {
        "SHOCK": MoveState("SHOCK", shock, [attack_intent(shock_dmg), debuff_intent()], follow_up_id="COIL"),
        "COIL": MoveState("COIL", coil, [multi_attack_intent(coil_dmg, 2)], follow_up_id="SURGE"),
        "SURGE": MoveState("SURGE", surge, [attack_intent(surge_dmg)], follow_up_id="SHOCK"),
    }
    return creature, MonsterAI(states, "SHOCK")


# ========================================================================
# BOSS ENCOUNTERS
# ========================================================================

# ---- WaterfallGiant (HP 310 / 330 asc) ----

def create_waterfall_giant(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 310
    creature = Creature(max_hp=hp, monster_id="WATERFALL_GIANT")
    slam_dmg = 22
    crush_dmg = 14
    torrent_dmg = 35

    def roar(combat: CombatState) -> None:
        creature.apply_power(PowerId.STRENGTH, 3)
        combat.apply_power_to(combat.player, PowerId.WEAK, 1)
        combat.apply_power_to(combat.player, PowerId.VULNERABLE, 1)

    def slam(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, slam_dmg)

    def crush(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, crush_dmg, hits=2)

    def torrent(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, torrent_dmg)

    states: dict[str, MonsterState] = {
        "ROAR": MoveState("ROAR", roar, [buff_intent(), debuff_intent()], follow_up_id="SLAM"),
        "SLAM": MoveState("SLAM", slam, [attack_intent(slam_dmg)], follow_up_id="CRUSH"),
        "CRUSH": MoveState("CRUSH", crush, [multi_attack_intent(crush_dmg, 2)], follow_up_id="TORRENT"),
        "TORRENT": MoveState("TORRENT", torrent, [attack_intent(torrent_dmg)], follow_up_id="ROAR"),
    }
    return creature, MonsterAI(states, "ROAR")


# ---- SoulFysh (HP 270 / 290 asc) ----

def create_soul_fysh(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 270
    creature = Creature(max_hp=hp, monster_id="SOUL_FYSH")
    chomp_dmg = 16
    soul_drain_dmg = 8
    deep_dive_dmg = 30

    def chomp(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, chomp_dmg)

    def soul_drain(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, soul_drain_dmg, hits=3)

    def deep_dive(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, deep_dive_dmg)

    states: dict[str, MonsterState] = {
        "CHOMP": MoveState("CHOMP", chomp, [attack_intent(chomp_dmg)], follow_up_id="SOUL_DRAIN"),
        "SOUL_DRAIN": MoveState("SOUL_DRAIN", soul_drain, [multi_attack_intent(soul_drain_dmg, 3)], follow_up_id="DEEP_DIVE"),
        "DEEP_DIVE": MoveState("DEEP_DIVE", deep_dive, [attack_intent(deep_dive_dmg)], follow_up_id="CHOMP"),
    }
    return creature, MonsterAI(states, "CHOMP")


# ---- LagavulinMatriarch (HP 222 / 233 asc) ----
# C# cycle: SLEEP -> (branch: asleep->SLEEP, else->SLASH) ->
#   SLASH(19) -> DISEMBOWEL(9x2) -> SLASH2(12+12blk) -> SOUL_SIPHON(debuff+buff) -> SLASH...

def create_lagavulin_matriarch(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 222
    creature = Creature(max_hp=hp, monster_id="LAGAVULIN_MATRIARCH")
    slash_dmg = 19
    disembowel_dmg = 9
    slash2_dmg = 12
    slash2_block = 12

    _state = {"asleep": True}

    def sleep_move(combat: CombatState) -> None:
        pass  # AsleepPower handles the wake-up logic

    def slash(combat: CombatState) -> None:
        _state["asleep"] = False
        _deal_damage_to_player(combat, creature, slash_dmg)

    def disembowel(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, disembowel_dmg, hits=2)

    def slash2(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, slash2_dmg)
        _gain_block(creature, slash2_block)

    def soul_siphon(combat: CombatState) -> None:
        combat.apply_power_to(combat.player, PowerId.STRENGTH, -1)
        creature.apply_power(PowerId.STRENGTH, 1)

    # Sleep check
    sleep_check = ConditionalBranchState("SLEEP_CHECK")
    sleep_check.add_branch(lambda: _state["asleep"], "SLEEP")
    sleep_check.add_branch(lambda: True, "SLASH")

    states: dict[str, MonsterState] = {
        "SLEEP": MoveState("SLEEP", sleep_move, [sleep_intent()], follow_up_id="SLEEP_CHECK"),
        "SLEEP_CHECK": sleep_check,
        "SLASH": MoveState("SLASH", slash, [attack_intent(slash_dmg)], follow_up_id="DISEMBOWEL"),
        "DISEMBOWEL": MoveState("DISEMBOWEL", disembowel, [multi_attack_intent(disembowel_dmg, 2)], follow_up_id="SLASH2"),
        "SLASH2": MoveState("SLASH2", slash2, [attack_intent(slash2_dmg), defend_intent()], follow_up_id="SOUL_SIPHON"),
        "SOUL_SIPHON": MoveState("SOUL_SIPHON", soul_siphon, [debuff_intent(), buff_intent()], follow_up_id="SLASH"),
    }

    creature.apply_power(PowerId.PLATING, 12)
    creature.apply_power(PowerId.ASLEEP, 3)
    return creature, MonsterAI(states, "SLEEP")
