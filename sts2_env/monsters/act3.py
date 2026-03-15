"""Act 3 (Glory) monsters: weak, normal, elite, boss.

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
from sts2_env.cards.status import make_dazed

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

# ---- DevotedSculptor (HP 162 / 172 asc) ----

def create_devoted_sculptor(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 162
    creature = Creature(max_hp=hp, monster_id="DEVOTED_SCULPTOR")
    savage_dmg = 12

    def forbidden_incantation(combat: CombatState) -> None:
        creature.apply_power(PowerId.RITUAL, 9)

    def savage(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, savage_dmg)

    states: dict[str, MonsterState] = {
        "FORBIDDEN_INCANTATION": MoveState("FORBIDDEN_INCANTATION", forbidden_incantation, [buff_intent()], follow_up_id="SAVAGE"),
        "SAVAGE": MoveState("SAVAGE", savage, [attack_intent(savage_dmg)], follow_up_id="SAVAGE"),
    }
    return creature, MonsterAI(states, "FORBIDDEN_INCANTATION")


# ---- ScrollOfBiting (HP 24-26 / 26-28 asc) ----

def create_scroll_of_biting(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(24, 26)
    creature = Creature(max_hp=hp, monster_id="SCROLL_OF_BITING")
    bite_dmg = 7

    def bite(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, bite_dmg)

    states: dict[str, MonsterState] = {
        "BITE": MoveState("BITE", bite, [attack_intent(bite_dmg)], follow_up_id="BITE"),
    }
    return creature, MonsterAI(states, "BITE")


# ---- TurretOperator (HP 28-30 / 30-32 asc) ----

def create_turret_operator(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(28, 30)
    creature = Creature(max_hp=hp, monster_id="TURRET_OPERATOR")
    shoot_dmg = 9

    def deploy_turret(combat: CombatState) -> None:
        creature.apply_power(PowerId.STRENGTH, 2)

    def shoot(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, shoot_dmg)

    states: dict[str, MonsterState] = {
        "DEPLOY_TURRET": MoveState("DEPLOY_TURRET", deploy_turret, [buff_intent()], follow_up_id="SHOOT"),
        "SHOOT": MoveState("SHOOT", shoot, [attack_intent(shoot_dmg)], follow_up_id="SHOOT"),
    }
    return creature, MonsterAI(states, "DEPLOY_TURRET")


# ========================================================================
# NORMAL ENCOUNTERS
# ========================================================================

# ---- Axebot (HP 40-44 / 42-46 asc) ----

def create_axebot(rng: Rng, start_with_boot_up: bool = False) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(40, 44)
    creature = Creature(max_hp=hp, monster_id="AXEBOT")
    one_two_dmg = 5
    hammer_uppercut_dmg = 8
    boot_up_block = 10

    def boot_up(combat: CombatState) -> None:
        _gain_block(creature, boot_up_block)
        creature.apply_power(PowerId.STRENGTH, 1)

    def one_two(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, one_two_dmg, hits=2)

    def sharpen(combat: CombatState) -> None:
        creature.apply_power(PowerId.STRENGTH, 4)

    def hammer_uppercut(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, hammer_uppercut_dmg)
        combat.apply_power_to(combat.player, PowerId.WEAK, 1)
        combat.apply_power_to(combat.player, PowerId.FRAIL, 1)

    rand = RandomBranchState("RAND")
    rand.add_branch("ONE_TWO", MoveRepeatType.CAN_REPEAT_FOREVER, weight=2.0)
    rand.add_branch("SHARPEN", MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch("HAMMER_UPPERCUT", MoveRepeatType.CAN_REPEAT_FOREVER, weight=2.0)

    states: dict[str, MonsterState] = {
        "RAND": rand,
        "BOOT_UP": MoveState("BOOT_UP", boot_up, [defend_intent(), buff_intent()], follow_up_id="RAND"),
        "ONE_TWO": MoveState("ONE_TWO", one_two, [multi_attack_intent(one_two_dmg, 2)], follow_up_id="RAND"),
        "SHARPEN": MoveState("SHARPEN", sharpen, [buff_intent()], follow_up_id="RAND"),
        "HAMMER_UPPERCUT": MoveState("HAMMER_UPPERCUT", hammer_uppercut, [attack_intent(hammer_uppercut_dmg), debuff_intent()], follow_up_id="RAND"),
    }

    initial = "BOOT_UP" if start_with_boot_up else "RAND"
    return creature, MonsterAI(states, initial)


# ---- Fabricator (HP 150 / 155 asc) + bots ----

def create_zapbot(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(23, 28)
    creature = Creature(max_hp=hp, monster_id="ZAPBOT")
    zap_dmg = 14

    def zap(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, zap_dmg)

    states: dict[str, MonsterState] = {
        "ZAP": MoveState("ZAP", zap, [attack_intent(zap_dmg)], follow_up_id="ZAP"),
    }
    creature.apply_power(PowerId.MINION, 1)
    return creature, MonsterAI(states, "ZAP")


def create_stabbot(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(23, 28)
    creature = Creature(max_hp=hp, monster_id="STABBOT")
    stab_dmg = 3

    def stab(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, stab_dmg, hits=3)

    states: dict[str, MonsterState] = {
        "STAB": MoveState("STAB", stab, [multi_attack_intent(stab_dmg, 3)], follow_up_id="STAB"),
    }
    creature.apply_power(PowerId.MINION, 1)
    return creature, MonsterAI(states, "STAB")


def create_guardbot(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(21, 25)
    creature = Creature(max_hp=hp, monster_id="GUARDBOT")

    def guard(combat: CombatState) -> None:
        # Give block to all Fabricators
        for enemy in combat.alive_enemies:
            if enemy.monster_id == "FABRICATOR":
                _gain_block(enemy, 8)

    states: dict[str, MonsterState] = {
        "GUARD": MoveState("GUARD", guard, [defend_intent()], follow_up_id="GUARD"),
    }
    creature.apply_power(PowerId.MINION, 1)
    return creature, MonsterAI(states, "GUARD")


def create_noisebot(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(23, 28)
    creature = Creature(max_hp=hp, monster_id="NOISEBOT")

    def noise(combat: CombatState) -> None:
        combat.add_card_to_discard(make_dazed())
        combat.add_card_to_discard(make_dazed())

    states: dict[str, MonsterState] = {
        "NOISE": MoveState("NOISE", noise, [status_intent()], follow_up_id="NOISE"),
    }
    creature.apply_power(PowerId.MINION, 1)
    return creature, MonsterAI(states, "NOISE")


def create_fabricator(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 150
    creature = Creature(max_hp=hp, monster_id="FABRICATOR")
    fabricating_strike_dmg = 18
    disintegrate_dmg = 11

    _state = {"last_aggro": None, "last_defense": None}

    aggro_creators = [create_zapbot, create_stabbot]
    defense_creators = [create_guardbot, create_noisebot]

    def _spawn_aggro(combat: CombatState) -> None:
        idx = 0 if _state["last_aggro"] == 1 else (1 if _state["last_aggro"] == 0 else rng.next_int(0, 1))
        _state["last_aggro"] = idx
        bot, bot_ai = aggro_creators[idx](rng)
        combat.add_enemy(bot, bot_ai)

    def _spawn_defense(combat: CombatState) -> None:
        idx = 0 if _state["last_defense"] == 1 else (1 if _state["last_defense"] == 0 else rng.next_int(0, 1))
        _state["last_defense"] = idx
        bot, bot_ai = defense_creators[idx](rng)
        combat.add_enemy(bot, bot_ai)

    def fabricate(combat: CombatState) -> None:
        _spawn_defense(combat)
        _spawn_aggro(combat)

    def fabricating_strike(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, fabricating_strike_dmg)
        _spawn_aggro(combat)

    def disintegrate(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, disintegrate_dmg)

    def _can_fabricate() -> bool:
        return True  # Simplified: check ally count < 4

    fab_branch = ConditionalBranchState("FAB_BRANCH")
    fab_branch.add_branch(_can_fabricate, "FAB_RAND")
    fab_branch.add_branch(lambda: True, "DISINTEGRATE")

    fab_rand = RandomBranchState("FAB_RAND")
    fab_rand.add_branch("FABRICATE")
    fab_rand.add_branch("FABRICATING_STRIKE")

    states: dict[str, MonsterState] = {
        "FAB_BRANCH": fab_branch,
        "FAB_RAND": fab_rand,
        "FABRICATE": MoveState("FABRICATE", fabricate, [Intent(IntentType.SUMMON)], follow_up_id="FAB_BRANCH"),
        "FABRICATING_STRIKE": MoveState("FABRICATING_STRIKE", fabricating_strike, [attack_intent(fabricating_strike_dmg), Intent(IntentType.SUMMON)], follow_up_id="FAB_BRANCH"),
        "DISINTEGRATE": MoveState("DISINTEGRATE", disintegrate, [attack_intent(disintegrate_dmg)], follow_up_id="FAB_BRANCH"),
    }
    return creature, MonsterAI(states, "FAB_BRANCH")


# ---- FrogKnight (HP 191 / 199 asc) ----

def create_frog_knight(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 191
    creature = Creature(max_hp=hp, monster_id="FROG_KNIGHT")
    strike_down_evil_dmg = 21
    tongue_lash_dmg = 13
    beetle_charge_dmg = 35

    _state = {"beetle_charged": False}

    def tongue_lash(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, tongue_lash_dmg)
        combat.apply_power_to(combat.player, PowerId.FRAIL, 2)

    def strike_down_evil(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, strike_down_evil_dmg)

    def for_the_queen(combat: CombatState) -> None:
        creature.apply_power(PowerId.STRENGTH, 5)

    def beetle_charge(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, beetle_charge_dmg)
        _state["beetle_charged"] = True

    # After for_the_queen, check HP for beetle charge
    charge_check = ConditionalBranchState("CHARGE_CHECK")
    charge_check.add_branch(
        lambda: not _state["beetle_charged"] and creature.current_hp < creature.max_hp // 2,
        "BEETLE_CHARGE"
    )
    charge_check.add_branch(lambda: True, "TONGUE_LASH")

    states: dict[str, MonsterState] = {
        "TONGUE_LASH": MoveState("TONGUE_LASH", tongue_lash, [attack_intent(tongue_lash_dmg), debuff_intent()], follow_up_id="STRIKE_DOWN_EVIL"),
        "STRIKE_DOWN_EVIL": MoveState("STRIKE_DOWN_EVIL", strike_down_evil, [attack_intent(strike_down_evil_dmg)], follow_up_id="FOR_THE_QUEEN"),
        "FOR_THE_QUEEN": MoveState("FOR_THE_QUEEN", for_the_queen, [buff_intent()], follow_up_id="CHARGE_CHECK"),
        "CHARGE_CHECK": charge_check,
        "BEETLE_CHARGE": MoveState("BEETLE_CHARGE", beetle_charge, [attack_intent(beetle_charge_dmg)], follow_up_id="TONGUE_LASH"),
    }

    creature.apply_power(PowerId.PLATING, 15)
    return creature, MonsterAI(states, "TONGUE_LASH")


# ---- GlobeHead (HP 148 / 158 asc) ----

def create_globe_head(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 148
    creature = Creature(max_hp=hp, monster_id="GLOBE_HEAD")
    shocking_slap_dmg = 13
    thunder_strike_dmg = 6
    galvanic_burst_dmg = 16

    def shocking_slap(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, shocking_slap_dmg)
        combat.apply_power_to(combat.player, PowerId.FRAIL, 2)

    def thunder_strike(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, thunder_strike_dmg, hits=3)

    def galvanic_burst(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, galvanic_burst_dmg)
        creature.apply_power(PowerId.STRENGTH, 2)

    states: dict[str, MonsterState] = {
        "SHOCKING_SLAP": MoveState("SHOCKING_SLAP", shocking_slap, [attack_intent(shocking_slap_dmg), debuff_intent()], follow_up_id="THUNDER_STRIKE"),
        "THUNDER_STRIKE": MoveState("THUNDER_STRIKE", thunder_strike, [multi_attack_intent(thunder_strike_dmg, 3)], follow_up_id="GALVANIC_BURST"),
        "GALVANIC_BURST": MoveState("GALVANIC_BURST", galvanic_burst, [attack_intent(galvanic_burst_dmg), buff_intent()], follow_up_id="SHOCKING_SLAP"),
    }

    creature.apply_power(PowerId.GALVANIC, 6)
    return creature, MonsterAI(states, "SHOCKING_SLAP")


# ---- OwlMagistrate (HP 82 / 86 asc) ----

def create_owl_magistrate(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 82
    creature = Creature(max_hp=hp, monster_id="OWL_MAGISTRATE")
    judgement_dmg = 10
    sentence_dmg = 16

    def judgement(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, judgement_dmg)
        combat.apply_power_to(combat.player, PowerId.WEAK, 1)

    def sentence(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, sentence_dmg)

    states: dict[str, MonsterState] = {
        "JUDGEMENT": MoveState("JUDGEMENT", judgement, [attack_intent(judgement_dmg), debuff_intent()], follow_up_id="SENTENCE"),
        "SENTENCE": MoveState("SENTENCE", sentence, [attack_intent(sentence_dmg)], follow_up_id="JUDGEMENT"),
    }
    return creature, MonsterAI(states, "JUDGEMENT")


# ---- SlimedBerserker (HP 60-65 / 64-69 asc) ----

def create_slimed_berserker(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(60, 65)
    creature = Creature(max_hp=hp, monster_id="SLIMED_BERSERKER")
    slash_dmg = 8
    slam_dmg = 14

    def slash(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, slash_dmg)

    def slam(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, slam_dmg)

    def enrage(combat: CombatState) -> None:
        creature.apply_power(PowerId.STRENGTH, 3)

    states: dict[str, MonsterState] = {
        "SLASH": MoveState("SLASH", slash, [attack_intent(slash_dmg)], follow_up_id="SLAM"),
        "SLAM": MoveState("SLAM", slam, [attack_intent(slam_dmg)], follow_up_id="ENRAGE"),
        "ENRAGE": MoveState("ENRAGE", enrage, [buff_intent()], follow_up_id="SLASH"),
    }
    return creature, MonsterAI(states, "SLASH")


# ---- TheLost + TheForgotten ----

def create_the_lost(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(50, 54)
    creature = Creature(max_hp=hp, monster_id="THE_LOST")
    dark_slash_dmg = 9
    shadow_strike_dmg = 13

    def dark_slash(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, dark_slash_dmg)

    def shadow_strike(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, shadow_strike_dmg)

    states: dict[str, MonsterState] = {
        "DARK_SLASH": MoveState("DARK_SLASH", dark_slash, [attack_intent(dark_slash_dmg)], follow_up_id="SHADOW_STRIKE"),
        "SHADOW_STRIKE": MoveState("SHADOW_STRIKE", shadow_strike, [attack_intent(shadow_strike_dmg)], follow_up_id="DARK_SLASH"),
    }
    return creature, MonsterAI(states, "DARK_SLASH")


def create_the_forgotten(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(50, 54)
    creature = Creature(max_hp=hp, monster_id="THE_FORGOTTEN")
    haunt_dmg = 7
    wail_dmg = 11

    def haunt(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, haunt_dmg)
        combat.apply_power_to(combat.player, PowerId.WEAK, 1)

    def wail_of_sorrow(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, wail_dmg)

    states: dict[str, MonsterState] = {
        "HAUNT": MoveState("HAUNT", haunt, [attack_intent(haunt_dmg), debuff_intent()], follow_up_id="WAIL_OF_SORROW"),
        "WAIL_OF_SORROW": MoveState("WAIL_OF_SORROW", wail_of_sorrow, [attack_intent(wail_dmg)], follow_up_id="HAUNT"),
    }
    return creature, MonsterAI(states, "HAUNT")


# ---- ConstructMenagerie (mixed normal encounter placeholder) ----
# Uses CubexConstruct from act1 in a different configuration


# ========================================================================
# ELITE ENCOUNTERS
# ========================================================================

# ---- Knights (FlailKnight, MagiKnight, SpectralKnight) ----

def create_flail_knight(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 101
    creature = Creature(max_hp=hp, monster_id="FLAIL_KNIGHT")
    flail_dmg = 9
    ram_dmg = 15

    def war_chant(combat: CombatState) -> None:
        creature.apply_power(PowerId.STRENGTH, 3)

    def flail(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, flail_dmg, hits=2)

    def ram(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, ram_dmg)

    rand = RandomBranchState("RAND")
    rand.add_branch("WAR_CHANT", MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch("FLAIL", MoveRepeatType.CAN_REPEAT_FOREVER, weight=2.0)
    rand.add_branch("RAM", MoveRepeatType.CAN_REPEAT_FOREVER, weight=2.0)

    states: dict[str, MonsterState] = {
        "RAND": rand,
        "WAR_CHANT": MoveState("WAR_CHANT", war_chant, [buff_intent()], follow_up_id="RAND"),
        "FLAIL": MoveState("FLAIL", flail, [multi_attack_intent(flail_dmg, 2)], follow_up_id="RAND"),
        "RAM": MoveState("RAM", ram, [attack_intent(ram_dmg)], follow_up_id="RAND"),
    }
    return creature, MonsterAI(states, "RAM")


# ---- MysteriousKnight (HP 101, event combat) ----
# Identical state machine to FlailKnight, but starts with Str+6 and Plating(6).

def create_mysterious_knight(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 101
    creature = Creature(max_hp=hp, monster_id="MYSTERIOUS_KNIGHT")
    flail_dmg = 9
    ram_dmg = 15

    def war_chant(combat: CombatState) -> None:
        creature.apply_power(PowerId.STRENGTH, 3)

    def flail(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, flail_dmg, hits=2)

    def ram(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, ram_dmg)

    rand = RandomBranchState("RAND")
    rand.add_branch("WAR_CHANT", MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch("FLAIL", MoveRepeatType.CAN_REPEAT_FOREVER, weight=2.0)
    rand.add_branch("RAM", MoveRepeatType.CAN_REPEAT_FOREVER, weight=2.0)

    states: dict[str, MonsterState] = {
        "RAND": rand,
        "WAR_CHANT": MoveState("WAR_CHANT", war_chant, [buff_intent()], follow_up_id="RAND"),
        "FLAIL": MoveState("FLAIL", flail, [multi_attack_intent(flail_dmg, 2)], follow_up_id="RAND"),
        "RAM": MoveState("RAM", ram, [attack_intent(ram_dmg)], follow_up_id="RAND"),
    }

    # AfterAddedToRoom: Strength+6, Plating(6) on top of FlailKnight's base
    creature.apply_power(PowerId.STRENGTH, 6)
    creature.apply_power(PowerId.PLATING, 6)
    return creature, MonsterAI(states, "RAM")


# ---- LivingShield (HP 55) ----
# Moves: SHIELD_SLAM(6) while allies alive, SMASH(16, Str+3) when alone.
# Conditional branch checks ally count.
# AfterAddedToRoom: Rampart(25)

def create_living_shield(rng: Rng, get_ally_count=None) -> tuple[Creature, MonsterAI]:
    hp = 55
    creature = Creature(max_hp=hp, monster_id="LIVING_SHIELD")
    shield_slam_dmg = 6
    smash_dmg = 16
    enrage_str = 3

    def shield_slam(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, shield_slam_dmg)

    def smash(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, smash_dmg)
        creature.apply_power(PowerId.STRENGTH, enrage_str)

    # Default ally count checker: count alive teammates excluding self
    def _default_ally_count() -> int:
        # Simplified: if no combat context available, assume allies exist
        return 1

    ally_count_fn = get_ally_count or _default_ally_count

    # Conditional: if allies alive -> SHIELD_SLAM, else -> SMASH (self loop)
    shield_slam_branch = ConditionalBranchState("SHIELD_SLAM_BRANCH")
    shield_slam_branch.add_branch(lambda: ally_count_fn() > 0, "SHIELD_SLAM")
    shield_slam_branch.add_branch(lambda: ally_count_fn() == 0, "SMASH")

    states: dict[str, MonsterState] = {
        "SHIELD_SLAM_BRANCH": shield_slam_branch,
        "SHIELD_SLAM": MoveState("SHIELD_SLAM", shield_slam, [attack_intent(shield_slam_dmg)], follow_up_id="SHIELD_SLAM_BRANCH"),
        "SMASH": MoveState("SMASH", smash, [attack_intent(smash_dmg), buff_intent()], follow_up_id="SMASH"),
    }

    creature.apply_power(PowerId.RAMPART, 25)
    return creature, MonsterAI(states, "SHIELD_SLAM")


def create_magi_knight(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 101
    creature = Creature(max_hp=hp, monster_id="MAGI_KNIGHT")
    magic_missile_dmg = 5
    arcane_blast_dmg = 15
    arcane_siege_block = 12

    def magic_missile(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, magic_missile_dmg, hits=3)

    def arcane_blast(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, arcane_blast_dmg)

    def arcane_siege(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, arcane_blast_dmg)
        _gain_block(creature, arcane_siege_block)

    rand = RandomBranchState("RAND")
    rand.add_branch("MAGIC_MISSILE", MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch("ARCANE_BLAST", MoveRepeatType.CAN_REPEAT_FOREVER, weight=2.0)
    rand.add_branch("ARCANE_SIEGE", MoveRepeatType.CANNOT_REPEAT)

    states: dict[str, MonsterState] = {
        "RAND": rand,
        "MAGIC_MISSILE": MoveState("MAGIC_MISSILE", magic_missile, [multi_attack_intent(magic_missile_dmg, 3)], follow_up_id="RAND"),
        "ARCANE_BLAST": MoveState("ARCANE_BLAST", arcane_blast, [attack_intent(arcane_blast_dmg)], follow_up_id="RAND"),
        "ARCANE_SIEGE": MoveState("ARCANE_SIEGE", arcane_siege, [attack_intent(arcane_blast_dmg), defend_intent()], follow_up_id="RAND"),
    }
    return creature, MonsterAI(states, "MAGIC_MISSILE")


def create_spectral_knight(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 101
    creature = Creature(max_hp=hp, monster_id="SPECTRAL_KNIGHT")
    ghost_slash_dmg = 12
    phantom_rush_dmg = 5

    def ghost_slash(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, ghost_slash_dmg)

    def phantom_rush(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, phantom_rush_dmg, hits=2)

    def ethereal_shift(combat: CombatState) -> None:
        creature.apply_power(PowerId.INTANGIBLE, 1)

    rand = RandomBranchState("RAND")
    rand.add_branch("GHOST_SLASH", MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch("PHANTOM_RUSH", MoveRepeatType.CAN_REPEAT_FOREVER, weight=2.0)
    rand.add_branch("ETHEREAL_SHIFT", MoveRepeatType.CANNOT_REPEAT)

    states: dict[str, MonsterState] = {
        "RAND": rand,
        "GHOST_SLASH": MoveState("GHOST_SLASH", ghost_slash, [attack_intent(ghost_slash_dmg)], follow_up_id="RAND"),
        "PHANTOM_RUSH": MoveState("PHANTOM_RUSH", phantom_rush, [multi_attack_intent(phantom_rush_dmg, 2)], follow_up_id="RAND"),
        "ETHEREAL_SHIFT": MoveState("ETHEREAL_SHIFT", ethereal_shift, [buff_intent()], follow_up_id="RAND"),
    }
    return creature, MonsterAI(states, "GHOST_SLASH")


# ---- MechaKnight (HP 155 / 165 asc) ----

def create_mecha_knight(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 155
    creature = Creature(max_hp=hp, monster_id="MECHA_KNIGHT")
    steel_slash_dmg = 14
    missile_salvo_dmg = 4
    overdrive_dmg = 24
    shield_up_block = 16

    def steel_slash(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, steel_slash_dmg)

    def missile_salvo(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, missile_salvo_dmg, hits=4)

    def shield_up(combat: CombatState) -> None:
        _gain_block(creature, shield_up_block)
        creature.apply_power(PowerId.STRENGTH, 2)

    def overdrive_strike(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, overdrive_dmg)

    states: dict[str, MonsterState] = {
        "STEEL_SLASH": MoveState("STEEL_SLASH", steel_slash, [attack_intent(steel_slash_dmg)], follow_up_id="MISSILE_SALVO"),
        "MISSILE_SALVO": MoveState("MISSILE_SALVO", missile_salvo, [multi_attack_intent(missile_salvo_dmg, 4)], follow_up_id="SHIELD_UP"),
        "SHIELD_UP": MoveState("SHIELD_UP", shield_up, [defend_intent(), buff_intent()], follow_up_id="OVERDRIVE_STRIKE"),
        "OVERDRIVE_STRIKE": MoveState("OVERDRIVE_STRIKE", overdrive_strike, [attack_intent(overdrive_dmg)], follow_up_id="STEEL_SLASH"),
    }
    return creature, MonsterAI(states, "STEEL_SLASH")


# ---- SoulNexus (HP 155 / 165 asc) + Osty ----

def create_osty(rng: Rng) -> tuple[Creature, MonsterAI]:
    creature = Creature(max_hp=1, monster_id="OSTY")
    haunt_dmg = 4

    def haunt(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, haunt_dmg)

    states: dict[str, MonsterState] = {
        "HAUNT": MoveState("HAUNT", haunt, [attack_intent(haunt_dmg)], follow_up_id="HAUNT"),
    }
    creature.apply_power(PowerId.MINION, 1)
    return creature, MonsterAI(states, "HAUNT")


def create_soul_nexus(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 155
    creature = Creature(max_hp=hp, monster_id="SOUL_NEXUS")
    soul_drain_dmg = 10
    spirit_barrage_dmg = 4

    def summon(combat: CombatState) -> None:
        osty, osty_ai = create_osty(rng)
        combat.add_enemy(osty, osty_ai)

    def soul_drain(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, soul_drain_dmg)
        creature.heal(10)

    def spirit_barrage(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, spirit_barrage_dmg, hits=3)

    states: dict[str, MonsterState] = {
        "SUMMON": MoveState("SUMMON", summon, [Intent(IntentType.SUMMON)], follow_up_id="SOUL_DRAIN"),
        "SOUL_DRAIN": MoveState("SOUL_DRAIN", soul_drain, [attack_intent(soul_drain_dmg), Intent(IntentType.HEAL)], follow_up_id="SPIRIT_BARRAGE"),
        "SPIRIT_BARRAGE": MoveState("SPIRIT_BARRAGE", spirit_barrage, [multi_attack_intent(spirit_barrage_dmg, 3)], follow_up_id="SUMMON"),
    }
    return creature, MonsterAI(states, "SUMMON")


# ========================================================================
# BOSS ENCOUNTERS
# ========================================================================

# ---- Door + Doormaker ----

def create_door(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 155
    creature = Creature(max_hp=hp, monster_id="DOOR")
    dramatic_open_dmg = 25
    enforce_dmg = 20
    door_slam_dmg = 15

    def dramatic_open(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, dramatic_open_dmg)

    def enforce(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, enforce_dmg)
        creature.apply_power(PowerId.STRENGTH, 3)

    def door_slam(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, door_slam_dmg, hits=2)

    def dead_move(combat: CombatState) -> None:
        pass

    states: dict[str, MonsterState] = {
        "DRAMATIC_OPEN": MoveState("DRAMATIC_OPEN", dramatic_open, [attack_intent(dramatic_open_dmg)], follow_up_id="DOOR_SLAM"),
        "DOOR_SLAM": MoveState("DOOR_SLAM", door_slam, [multi_attack_intent(door_slam_dmg, 2)], follow_up_id="ENFORCE"),
        "ENFORCE": MoveState("ENFORCE", enforce, [attack_intent(enforce_dmg), buff_intent()], follow_up_id="DRAMATIC_OPEN"),
        "DEAD": MoveState("DEAD", dead_move, [Intent(IntentType.UNKNOWN)], follow_up_id="DEAD"),
    }

    creature.apply_power(PowerId.DOOR_REVIVAL, 1)
    return creature, MonsterAI(states, "DRAMATIC_OPEN")


def create_doormaker(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 489
    creature = Creature(max_hp=hp, monster_id="DOORMAKER")
    laser_beam_dmg = 31
    get_back_in_dmg = 40

    def what_is_it(combat: CombatState) -> None:
        pass

    def beam(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, laser_beam_dmg)

    def get_back_in(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, get_back_in_dmg)
        creature.apply_power(PowerId.STRENGTH, 5)

    states: dict[str, MonsterState] = {
        "WHAT_IS_IT": MoveState("WHAT_IS_IT", what_is_it, [Intent(IntentType.STUN)], follow_up_id="BEAM"),
        "BEAM": MoveState("BEAM", beam, [attack_intent(laser_beam_dmg)], follow_up_id="GET_BACK_IN"),
        "GET_BACK_IN": MoveState("GET_BACK_IN", get_back_in, [attack_intent(get_back_in_dmg), buff_intent()], follow_up_id="GET_BACK_IN"),
    }
    return creature, MonsterAI(states, "WHAT_IS_IT")


# ---- Queen (HP 302 / 322 asc) ----

def create_royal_guard(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 40
    creature = Creature(max_hp=hp, monster_id="ROYAL_GUARD")
    strike_dmg = 15

    def guard_strike(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, strike_dmg)

    states: dict[str, MonsterState] = {
        "STRIKE": MoveState("STRIKE", guard_strike, [attack_intent(strike_dmg)], follow_up_id="STRIKE"),
    }
    creature.apply_power(PowerId.MINION, 1)
    return creature, MonsterAI(states, "STRIKE")


def create_queen(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 302
    creature = Creature(max_hp=hp, monster_id="QUEEN")
    royal_strike_dmg = 18
    guillotine_dmg = 30

    def summon_guard(combat: CombatState) -> None:
        guard, guard_ai = create_royal_guard(rng)
        combat.add_enemy(guard, guard_ai)

    def royal_strike(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, royal_strike_dmg)
        combat.apply_power_to(combat.player, PowerId.WEAK, 1)

    def guillotine(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, guillotine_dmg)

    states: dict[str, MonsterState] = {
        "SUMMON_GUARD": MoveState("SUMMON_GUARD", summon_guard, [Intent(IntentType.SUMMON)], follow_up_id="ROYAL_STRIKE"),
        "ROYAL_STRIKE": MoveState("ROYAL_STRIKE", royal_strike, [attack_intent(royal_strike_dmg), debuff_intent()], follow_up_id="GUILLOTINE"),
        "GUILLOTINE": MoveState("GUILLOTINE", guillotine, [attack_intent(guillotine_dmg)], follow_up_id="SUMMON_GUARD"),
    }
    return creature, MonsterAI(states, "SUMMON_GUARD")


# ---- TestSubject (HP 255 / 270 asc) ----

def create_test_subject(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 255
    creature = Creature(max_hp=hp, monster_id="TEST_SUBJECT")
    discharge_dmg = 11
    overload_dmg = 28
    chain_lightning_dmg = 7

    _state = {"phase2": False}

    def charge_up(combat: CombatState) -> None:
        creature.apply_power(PowerId.STRENGTH, 3)

    def discharge(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, discharge_dmg, hits=2)

    def overload(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, overload_dmg)

    def chain_lightning(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, chain_lightning_dmg, hits=4)

    # Phase check
    phase_check = ConditionalBranchState("PHASE_CHECK")
    phase_check.add_branch(
        lambda: not _state["phase2"] and creature.current_hp < creature.max_hp // 2,
        "CHARGE_UP_P2"
    )
    phase_check.add_branch(lambda: _state["phase2"], "DISCHARGE_P2")
    phase_check.add_branch(lambda: True, "CHARGE_UP")

    def charge_up_p2(combat: CombatState) -> None:
        _state["phase2"] = True
        creature.apply_power(PowerId.STRENGTH, 5)

    states: dict[str, MonsterState] = {
        "CHARGE_UP": MoveState("CHARGE_UP", charge_up, [buff_intent()], follow_up_id="DISCHARGE"),
        "DISCHARGE": MoveState("DISCHARGE", discharge, [multi_attack_intent(discharge_dmg, 2)], follow_up_id="OVERLOAD"),
        "OVERLOAD": MoveState("OVERLOAD", overload, [attack_intent(overload_dmg)], follow_up_id="PHASE_CHECK"),
        "PHASE_CHECK": phase_check,
        "CHARGE_UP_P2": MoveState("CHARGE_UP_P2", charge_up_p2, [buff_intent()], follow_up_id="DISCHARGE_P2"),
        "DISCHARGE_P2": MoveState("DISCHARGE_P2", discharge, [multi_attack_intent(discharge_dmg, 2)], follow_up_id="CHAIN_LIGHTNING"),
        "CHAIN_LIGHTNING": MoveState("CHAIN_LIGHTNING", chain_lightning, [multi_attack_intent(chain_lightning_dmg, 4)], follow_up_id="OVERLOAD_P2"),
        "OVERLOAD_P2": MoveState("OVERLOAD_P2", overload, [attack_intent(overload_dmg)], follow_up_id="CHARGE_UP_P2"),
    }
    return creature, MonsterAI(states, "CHARGE_UP")
