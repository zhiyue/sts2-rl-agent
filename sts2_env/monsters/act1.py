"""Act 1 (Overgrowth) monsters: weak, normal, elite, boss.

All HP ranges, damage values, and state machines verified against decompiled C# source.
Weak monsters are re-exported from act1_weak.py for convenience.
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
from sts2_env.cards.status import make_dazed, make_slimed, make_parasite

if TYPE_CHECKING:
    from sts2_env.core.combat import CombatState

# Re-export weak monsters
from sts2_env.monsters.act1_weak import (  # noqa: F401
    create_shrinker_beetle,
    create_fuzzy_wurm_crawler,
    create_nibbit,
    create_leaf_slime_s,
    create_twig_slime_s,
    create_leaf_slime_m,
    create_twig_slime_m,
)


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
# NORMAL ENCOUNTERS
# ========================================================================

# ---- CubexConstruct (HP 65 / 70 asc) ----
# CHARGE_UP -> REPEATER -> REPEATER_2 -> EXPEL_BLAST -> REPEATER (loop)

def create_cubex_construct(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 65
    creature = Creature(max_hp=hp, monster_id="CUBEX_CONSTRUCT")

    blast_dmg = 7
    expel_dmg = 5

    def charge_up(combat: CombatState) -> None:
        creature.apply_power(PowerId.STRENGTH, 2)

    def repeater(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, blast_dmg)
        creature.apply_power(PowerId.STRENGTH, 2)

    def expel_blast(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, expel_dmg, hits=2)

    def submerge(combat: CombatState) -> None:
        _gain_block(creature, 15)

    states: dict[str, MonsterState] = {
        "CHARGE_UP": MoveState("CHARGE_UP", charge_up, [buff_intent()], follow_up_id="REPEATER"),
        "REPEATER": MoveState("REPEATER", repeater, [attack_intent(blast_dmg), buff_intent()], follow_up_id="REPEATER_2"),
        "REPEATER_2": MoveState("REPEATER_2", repeater, [attack_intent(blast_dmg), buff_intent()], follow_up_id="EXPEL_BLAST"),
        "EXPEL_BLAST": MoveState("EXPEL_BLAST", expel_blast, [multi_attack_intent(expel_dmg, 2)], follow_up_id="REPEATER"),
        "SUBMERGE": MoveState("SUBMERGE", submerge, [defend_intent()], follow_up_id="CHARGE_UP"),
    }

    # Initial setup: gain 13 block + 1 artifact
    creature.gain_block(13)
    creature.apply_power(PowerId.ARTIFACT, 1)

    return creature, MonsterAI(states, "CHARGE_UP")


# ---- Flyconid (HP 47-49 / 51-53 asc) ----

def create_flyconid(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(47, 49)
    creature = Creature(max_hp=hp, monster_id="FLYCONID")

    smash_dmg = 11
    spore_dmg = 8

    def vulnerable_spores(combat: CombatState) -> None:
        combat.apply_power_to(combat.player, PowerId.VULNERABLE, 2)

    def frail_spores(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, spore_dmg)
        combat.apply_power_to(combat.player, PowerId.FRAIL, 2)

    def smash(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, smash_dmg)

    # Initial random: FrailSpores(2) or Smash(1)
    initial_rand = RandomBranchState("INITIAL_RAND")
    initial_rand.add_branch("FRAIL_SPORES", weight=2.0)
    initial_rand.add_branch("SMASH", weight=1.0)

    # Main random: all 3, cannot repeat
    main_rand = RandomBranchState("RAND")
    main_rand.add_branch("VULNERABLE_SPORES", MoveRepeatType.CANNOT_REPEAT, weight=3.0)
    main_rand.add_branch("FRAIL_SPORES", MoveRepeatType.CANNOT_REPEAT, weight=2.0)
    main_rand.add_branch("SMASH", MoveRepeatType.CANNOT_REPEAT, weight=1.0)

    states: dict[str, MonsterState] = {
        "INITIAL_RAND": initial_rand,
        "RAND": main_rand,
        "VULNERABLE_SPORES": MoveState("VULNERABLE_SPORES", vulnerable_spores, [debuff_intent()], follow_up_id="RAND"),
        "FRAIL_SPORES": MoveState("FRAIL_SPORES", frail_spores, [attack_intent(spore_dmg), debuff_intent()], follow_up_id="RAND"),
        "SMASH": MoveState("SMASH", smash, [attack_intent(smash_dmg)], follow_up_id="RAND"),
    }
    return creature, MonsterAI(states, "INITIAL_RAND")


# ---- Fogmog (HP 74 / 78 asc) ----

def create_eye_with_teeth(rng: Rng) -> tuple[Creature, MonsterAI]:
    creature = Creature(max_hp=6, monster_id="EYE_WITH_TEETH")

    def distract(combat: CombatState) -> None:
        for _ in range(3):
            combat.add_card_to_discard(make_dazed())

    states: dict[str, MonsterState] = {
        "DISTRACT": MoveState("DISTRACT", distract, [status_intent()], follow_up_id="DISTRACT"),
    }
    creature.apply_power(PowerId.MINION, 1)
    return creature, MonsterAI(states, "DISTRACT")


def create_fogmog(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 74
    creature = Creature(max_hp=hp, monster_id="FOGMOG")

    swipe_dmg = 8
    headbutt_dmg = 14

    def illusion(combat: CombatState) -> None:
        eye, eye_ai = create_eye_with_teeth(rng)
        combat.add_enemy(eye, eye_ai)

    def swipe(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, swipe_dmg)
        creature.apply_power(PowerId.STRENGTH, 1)

    def headbutt(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, headbutt_dmg)

    rand = RandomBranchState("RAND")
    rand.add_branch("SWIPE_RANDOM", MoveRepeatType.CANNOT_REPEAT, weight=0.4)
    rand.add_branch("HEADBUTT", MoveRepeatType.CANNOT_REPEAT, weight=0.6)

    states: dict[str, MonsterState] = {
        "ILLUSION": MoveState("ILLUSION", illusion, [Intent(IntentType.SUMMON)], follow_up_id="SWIPE"),
        "SWIPE": MoveState("SWIPE", swipe, [attack_intent(swipe_dmg), buff_intent()], follow_up_id="RAND"),
        "RAND": rand,
        "SWIPE_RANDOM": MoveState("SWIPE_RANDOM", swipe, [attack_intent(swipe_dmg), buff_intent()], follow_up_id="HEADBUTT"),
        "HEADBUTT": MoveState("HEADBUTT", headbutt, [attack_intent(headbutt_dmg)], follow_up_id="SWIPE"),
    }
    return creature, MonsterAI(states, "ILLUSION")


# ---- Inklet (HP 30-33 / 32-35 asc) ----

def create_inklet(rng: Rng, slot: str = "first") -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(30, 33)
    creature = Creature(max_hp=hp, monster_id="INKLET")

    splatter_dmg = 6
    sub_dmg = 4
    sub_block = 8

    def splatter(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, splatter_dmg)

    def submerge(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, sub_dmg)
        _gain_block(creature, sub_block)

    rand = RandomBranchState("RAND")
    rand.add_branch("SPLATTER", MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch("SUBMERGE", MoveRepeatType.CANNOT_REPEAT)

    states: dict[str, MonsterState] = {
        "RAND": rand,
        "SPLATTER": MoveState("SPLATTER", splatter, [attack_intent(splatter_dmg)], follow_up_id="SUBMERGE"),
        "SUBMERGE": MoveState("SUBMERGE", submerge, [attack_intent(sub_dmg), defend_intent()], follow_up_id="RAND"),
    }

    if slot == "first":
        initial = "SPLATTER"
    elif slot == "second":
        initial = "SUBMERGE"
    else:
        initial = "RAND"

    return creature, MonsterAI(states, initial)


# ---- Mawler (HP 72 / 76 asc) ----

def create_mawler(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 72
    creature = Creature(max_hp=hp, monster_id="MAWLER")

    rip_dmg = 14
    claw_dmg = 4

    def rip_and_tear(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, rip_dmg)

    def roar(combat: CombatState) -> None:
        combat.apply_power_to(combat.player, PowerId.VULNERABLE, 3)

    def claw(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, claw_dmg, hits=2)

    rand = RandomBranchState("RAND")
    rand.add_branch("RIP_AND_TEAR", MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch("ROAR", MoveRepeatType.USE_ONLY_ONCE)
    rand.add_branch("CLAW", MoveRepeatType.CANNOT_REPEAT)

    states: dict[str, MonsterState] = {
        "RAND": rand,
        "RIP_AND_TEAR": MoveState("RIP_AND_TEAR", rip_and_tear, [attack_intent(rip_dmg)], follow_up_id="RAND"),
        "ROAR": MoveState("ROAR", roar, [debuff_intent()], follow_up_id="RAND"),
        "CLAW": MoveState("CLAW", claw, [multi_attack_intent(claw_dmg, 2)], follow_up_id="RAND"),
    }
    return creature, MonsterAI(states, "CLAW")


# ---- VineShambler (HP 40-43 / 42-45 asc) ----

def create_vine_shambler(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(40, 43)
    creature = Creature(max_hp=hp, monster_id="VINE_SHAMBLER")

    vine_whip_dmg = 7
    tangle_dmg = 10

    def vine_whip(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, vine_whip_dmg)
        combat.apply_power_to(combat.player, PowerId.WEAK, 1)

    def tangle(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, tangle_dmg)

    rand = RandomBranchState("RAND")
    rand.add_branch("VINE_WHIP", MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch("TANGLE", MoveRepeatType.CANNOT_REPEAT)

    states: dict[str, MonsterState] = {
        "RAND": rand,
        "VINE_WHIP": MoveState("VINE_WHIP", vine_whip, [attack_intent(vine_whip_dmg), debuff_intent()], follow_up_id="RAND"),
        "TANGLE": MoveState("TANGLE", tangle, [attack_intent(tangle_dmg)], follow_up_id="RAND"),
    }

    # AfterAddedToRoom: Thorns(3)
    creature.apply_power(PowerId.THORNS, 3)

    return creature, MonsterAI(states, "RAND")


# ---- SlitheringStrangler (HP 45-49 / 47-51 asc) ----

def create_slithering_strangler(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(45, 49)
    creature = Creature(max_hp=hp, monster_id="SLITHERING_STRANGLER")

    squeeze_dmg = 5
    strangle_dmg = 12

    def squeeze(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, squeeze_dmg, hits=2)

    def strangle(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, strangle_dmg)
        combat.apply_power_to(combat.player, PowerId.WEAK, 1)

    states: dict[str, MonsterState] = {
        "SQUEEZE": MoveState("SQUEEZE", squeeze, [multi_attack_intent(squeeze_dmg, 2)], follow_up_id="STRANGLE"),
        "STRANGLE": MoveState("STRANGLE", strangle, [attack_intent(strangle_dmg), debuff_intent()], follow_up_id="SQUEEZE"),
    }

    # AfterAddedToRoom: Constrict(3)
    creature.apply_power(PowerId.CONSTRICT, 3)

    return creature, MonsterAI(states, "SQUEEZE")


# ---- SnappingJaxfruit (HP 53-56 / 56-59 asc) ----

def create_snapping_jaxfruit(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(53, 56)
    creature = Creature(max_hp=hp, monster_id="SNAPPING_JAXFRUIT")

    snap_dmg = 7
    seed_spit_dmg = 1
    seed_spit_hits = 4

    def snap(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, snap_dmg)

    def seed_spit(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, seed_spit_dmg, hits=seed_spit_hits)
        combat.apply_power_to(combat.player, PowerId.FRAIL, 1)

    def burrow(combat: CombatState) -> None:
        creature.apply_power(PowerId.STRENGTH, 2)

    rand = RandomBranchState("RAND")
    rand.add_branch("SNAP", MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch("SEED_SPIT", MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch("BURROW", MoveRepeatType.CANNOT_REPEAT)

    states: dict[str, MonsterState] = {
        "RAND": rand,
        "SNAP": MoveState("SNAP", snap, [attack_intent(snap_dmg)], follow_up_id="RAND"),
        "SEED_SPIT": MoveState("SEED_SPIT", seed_spit, [multi_attack_intent(seed_spit_dmg, seed_spit_hits), debuff_intent()], follow_up_id="RAND"),
        "BURROW": MoveState("BURROW", burrow, [buff_intent()], follow_up_id="RAND"),
    }

    # AfterAddedToRoom: Thorns(3)
    creature.apply_power(PowerId.THORNS, 3)

    return creature, MonsterAI(states, "RAND")


# ---- RubyRaiders ----

def create_assassin_ruby_raider(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(18, 23)
    creature = Creature(max_hp=hp, monster_id="ASSASSIN_RUBY_RAIDER")
    killshot_dmg = 11

    def killshot(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, killshot_dmg)

    states: dict[str, MonsterState] = {
        "KILLSHOT": MoveState("KILLSHOT", killshot, [attack_intent(killshot_dmg)], follow_up_id="KILLSHOT"),
    }
    return creature, MonsterAI(states, "KILLSHOT")


def create_axe_ruby_raider(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(20, 22)
    creature = Creature(max_hp=hp, monster_id="AXE_RUBY_RAIDER")
    swing_dmg = 5
    swing_block = 5
    big_swing_dmg = 12

    def swing(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, swing_dmg)
        _gain_block(creature, swing_block)

    def big_swing(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, big_swing_dmg)

    states: dict[str, MonsterState] = {
        "SWING_1": MoveState("SWING_1", swing, [attack_intent(swing_dmg), defend_intent()], follow_up_id="SWING_2"),
        "SWING_2": MoveState("SWING_2", swing, [attack_intent(swing_dmg), defend_intent()], follow_up_id="BIG_SWING"),
        "BIG_SWING": MoveState("BIG_SWING", big_swing, [attack_intent(big_swing_dmg)], follow_up_id="SWING_1"),
    }
    return creature, MonsterAI(states, "SWING_1")


def create_brute_ruby_raider(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(30, 33)
    creature = Creature(max_hp=hp, monster_id="BRUTE_RUBY_RAIDER")
    beat_dmg = 7

    def beat(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, beat_dmg)

    def roar(combat: CombatState) -> None:
        creature.apply_power(PowerId.STRENGTH, 3)

    states: dict[str, MonsterState] = {
        "BEAT": MoveState("BEAT", beat, [attack_intent(beat_dmg)], follow_up_id="ROAR"),
        "ROAR": MoveState("ROAR", roar, [buff_intent()], follow_up_id="BEAT"),
    }
    return creature, MonsterAI(states, "BEAT")


def create_crossbow_ruby_raider(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(18, 21)
    creature = Creature(max_hp=hp, monster_id="CROSSBOW_RUBY_RAIDER")
    fire_dmg = 14
    reload_block = 3

    def fire(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, fire_dmg)

    def reload(combat: CombatState) -> None:
        _gain_block(creature, reload_block)

    states: dict[str, MonsterState] = {
        "RELOAD": MoveState("RELOAD", reload, [defend_intent()], follow_up_id="FIRE"),
        "FIRE": MoveState("FIRE", fire, [attack_intent(fire_dmg)], follow_up_id="RELOAD"),
    }
    return creature, MonsterAI(states, "RELOAD")


def create_tracker_ruby_raider(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(21, 25)
    creature = Creature(max_hp=hp, monster_id="TRACKER_RUBY_RAIDER")
    hounds_dmg = 1
    hounds_hits = 8

    def track(combat: CombatState) -> None:
        combat.apply_power_to(combat.player, PowerId.FRAIL, 2)

    def hounds(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, hounds_dmg, hits=hounds_hits)

    states: dict[str, MonsterState] = {
        "TRACK": MoveState("TRACK", track, [debuff_intent()], follow_up_id="HOUNDS"),
        "HOUNDS": MoveState("HOUNDS", hounds, [multi_attack_intent(hounds_dmg, hounds_hits)], follow_up_id="HOUNDS"),
    }
    return creature, MonsterAI(states, "TRACK")


# ========================================================================
# ELITE ENCOUNTERS
# ========================================================================

# ---- BygoneEffigy (HP 127 / 132 asc) ----

def create_bygone_effigy(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 127
    creature = Creature(max_hp=hp, monster_id="BYGONE_EFFIGY")
    slash_dmg = 15

    def initial_sleep(combat: CombatState) -> None:
        pass  # Does nothing

    def wake(combat: CombatState) -> None:
        creature.apply_power(PowerId.STRENGTH, 10)

    def sleep_move(combat: CombatState) -> None:
        pass

    def slashes(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, slash_dmg)

    states: dict[str, MonsterState] = {
        "INITIAL_SLEEP": MoveState("INITIAL_SLEEP", initial_sleep, [sleep_intent()], follow_up_id="WAKE"),
        "WAKE": MoveState("WAKE", wake, [buff_intent()], follow_up_id="SLASHES"),
        "SLEEP": MoveState("SLEEP", sleep_move, [sleep_intent()], follow_up_id="SLASHES"),
        "SLASHES": MoveState("SLASHES", slashes, [attack_intent(slash_dmg)], follow_up_id="SLASHES"),
    }

    # AfterAddedToRoom: applies Slow power
    creature.apply_power(PowerId.SLOW, 1)

    return creature, MonsterAI(states, "INITIAL_SLEEP")


# ---- Byrdonis (HP 91-94 / 99 asc) ----

def create_byrdonis(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(91, 94)
    creature = Creature(max_hp=hp, monster_id="BYRDONIS")
    peck_dmg = 3
    peck_hits = 3
    swoop_dmg = 16

    def peck(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, peck_dmg, hits=peck_hits)

    def swoop(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, swoop_dmg)

    states: dict[str, MonsterState] = {
        "SWOOP": MoveState("SWOOP", swoop, [attack_intent(swoop_dmg)], follow_up_id="PECK"),
        "PECK": MoveState("PECK", peck, [multi_attack_intent(peck_dmg, peck_hits)], follow_up_id="SWOOP"),
    }

    # AfterAddedToRoom: applies Territorial power
    creature.apply_power(PowerId.TERRITORIAL, 1)

    return creature, MonsterAI(states, "SWOOP")


# ---- PhrogParasite (HP 80-83 / 83-86 asc) ----

def create_phrog_parasite(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(80, 83)
    creature = Creature(max_hp=hp, monster_id="PHROG_PARASITE")
    lunge_dmg = 12
    bite_dmg = 4

    def lunge(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, lunge_dmg)

    def infest(combat: CombatState) -> None:
        for _ in range(2):
            combat.add_card_to_discard(make_parasite())

    def bite(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, bite_dmg, hits=3)

    states: dict[str, MonsterState] = {
        "LUNGE": MoveState("LUNGE", lunge, [attack_intent(lunge_dmg)], follow_up_id="INFEST"),
        "INFEST": MoveState("INFEST", infest, [status_intent()], follow_up_id="BITE"),
        "BITE": MoveState("BITE", bite, [multi_attack_intent(bite_dmg, 3)], follow_up_id="LUNGE"),
    }

    # AfterAddedToRoom: CurlUp(10)
    creature.apply_power(PowerId.CURL_UP, 10)

    return creature, MonsterAI(states, "LUNGE")


# ========================================================================
# BOSS ENCOUNTERS
# ========================================================================

# ---- Vantom (HP 206 / 216 asc) ----

def create_parafright(rng: Rng) -> tuple[Creature, MonsterAI]:
    creature = Creature(max_hp=1, monster_id="PARAFRIGHT")
    haunt_dmg = 5

    def haunt(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, haunt_dmg)
        combat.apply_power_to(combat.player, PowerId.FRAIL, 1)

    states: dict[str, MonsterState] = {
        "HAUNT": MoveState("HAUNT", haunt, [attack_intent(haunt_dmg), debuff_intent()], follow_up_id="HAUNT"),
    }
    creature.apply_power(PowerId.MINION, 1)
    return creature, MonsterAI(states, "HAUNT")


def create_vantom(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 206
    creature = Creature(max_hp=hp, monster_id="VANTOM")
    chomp_dmg = 9
    ghastly_dmg = 22
    wail_dmg = 5

    def consume(combat: CombatState) -> None:
        for _ in range(2):
            pf, pf_ai = create_parafright(rng)
            combat.add_enemy(pf, pf_ai)

    def chomp(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, chomp_dmg, hits=2)

    def ghastly_smash(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, ghastly_dmg)

    def wail(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, wail_dmg, hits=3)
        combat.apply_power_to(combat.player, PowerId.WEAK, 1)
        combat.apply_power_to(combat.player, PowerId.FRAIL, 1)

    rand = RandomBranchState("RAND")
    rand.add_branch("CHOMP", MoveRepeatType.CAN_REPEAT_X_TIMES, max_times=2)
    rand.add_branch("GHASTLY_SMASH", MoveRepeatType.CAN_REPEAT_X_TIMES, max_times=2)
    rand.add_branch("WAIL", MoveRepeatType.CAN_REPEAT_X_TIMES, max_times=2)

    states: dict[str, MonsterState] = {
        "CONSUME": MoveState("CONSUME", consume, [Intent(IntentType.SUMMON), buff_intent()], follow_up_id="RAND"),
        "RAND": rand,
        "CHOMP": MoveState("CHOMP", chomp, [multi_attack_intent(chomp_dmg, 2)], follow_up_id="RAND"),
        "GHASTLY_SMASH": MoveState("GHASTLY_SMASH", ghastly_smash, [attack_intent(ghastly_dmg)], follow_up_id="RAND"),
        "WAIL": MoveState("WAIL", wail, [multi_attack_intent(wail_dmg, 3), debuff_intent()], follow_up_id="RAND"),
    }
    return creature, MonsterAI(states, "CONSUME")


# ---- CeremonialBeast (HP 252 / 262 asc) ----

def create_ceremonial_beast(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 252
    creature = Creature(max_hp=hp, monster_id="CEREMONIAL_BEAST")
    plow_dmg = 18
    stomp_dmg = 15
    crush_dmg = 17
    plow_amount = 150

    # Track phase
    _phase = {"stunned": False}

    def stamp(combat: CombatState) -> None:
        creature.apply_power(PowerId.PLOW, plow_amount)

    def plow_move(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, plow_dmg)
        creature.apply_power(PowerId.STRENGTH, 2)

    def stun(combat: CombatState) -> None:
        _phase["stunned"] = True

    def beast_cry(combat: CombatState) -> None:
        # Apply 1 debuff (Ringing/Weak equivalent)
        combat.apply_power_to(combat.player, PowerId.WEAK, 1)

    def stomp(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, stomp_dmg)

    def crush(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, crush_dmg)
        creature.apply_power(PowerId.STRENGTH, 3)

    # Phase 1 check: is plow broken?
    plow_check = ConditionalBranchState("PLOW_CHECK")
    plow_check.add_branch(lambda: creature.get_power_amount(PowerId.PLOW) <= 0, "STUN")
    plow_check.add_branch(lambda: True, "PLOW")

    states: dict[str, MonsterState] = {
        "STAMP": MoveState("STAMP", stamp, [buff_intent()], follow_up_id="PLOW"),
        "PLOW": MoveState("PLOW", plow_move, [attack_intent(plow_dmg), buff_intent()], follow_up_id="PLOW_CHECK"),
        "PLOW_CHECK": plow_check,
        "STUN": MoveState("STUN", stun, [Intent(IntentType.STUN)], follow_up_id="BEAST_CRY", must_perform_once=True),
        "BEAST_CRY": MoveState("BEAST_CRY", beast_cry, [debuff_intent()], follow_up_id="STOMP"),
        "STOMP": MoveState("STOMP", stomp, [attack_intent(stomp_dmg)], follow_up_id="CRUSH"),
        "CRUSH": MoveState("CRUSH", crush, [attack_intent(crush_dmg), buff_intent()], follow_up_id="BEAST_CRY"),
    }
    return creature, MonsterAI(states, "STAMP")


# ---- TheKin (KinPriest + 2 KinFollowers) ----

def create_kin_priest(rng: Rng) -> tuple[Creature, MonsterAI]:
    hp = 119
    creature = Creature(max_hp=hp, monster_id="KIN_PRIEST")
    smite_dmg = 22

    def conversion(combat: CombatState) -> None:
        creature.apply_power(PowerId.RITUAL, 4)

    def smite(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, smite_dmg)

    states: dict[str, MonsterState] = {
        "CONVERSION": MoveState("CONVERSION", conversion, [buff_intent()], follow_up_id="SMITE"),
        "SMITE": MoveState("SMITE", smite, [attack_intent(smite_dmg)], follow_up_id="SMITE"),
    }
    return creature, MonsterAI(states, "CONVERSION")


def create_kin_follower(rng: Rng, slot: str = "first") -> tuple[Creature, MonsterAI]:
    hp = rng.next_int(65, 71)
    creature = Creature(max_hp=hp, monster_id="KIN_FOLLOWER")
    bash_dmg = 10
    bite_dmg = 5

    def bash(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, bash_dmg)

    def bite(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, bite_dmg, hits=2)
        combat.apply_power_to(combat.player, PowerId.WEAK, 1)

    states: dict[str, MonsterState] = {
        "BASH": MoveState("BASH", bash, [attack_intent(bash_dmg)], follow_up_id="BITE"),
        "BITE": MoveState("BITE", bite, [multi_attack_intent(bite_dmg, 2), debuff_intent()], follow_up_id="BASH"),
    }

    initial = "BASH" if slot != "third" else "BITE"
    return creature, MonsterAI(states, initial)
