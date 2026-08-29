"""Act 3 (Glory) monsters: weak, normal, elite, boss.

All HP ranges, damage values, and state machines verified against decompiled C# source.
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from sts2_env.core.creature import Creature
from sts2_env.core.enums import CardId, CombatSide, MoveRepeatType, PowerId, ValueProp
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
from sts2_env.monsters.block import gain_move_block
from sts2_env.monsters.targets import (
    add_generated_cards_to_living_player_discards,
    apply_power_to_living_player_targets,
    living_player_targets,
)
from sts2_env.cards.status import make_burn, make_dazed, make_slimed, make_wither
from sts2_env.monsters.shared import TORCH_HEAD_AMALGAM_MONSTER_ID

if TYPE_CHECKING:
    from sts2_env.core.combat import CombatState


# ---- Helpers ----

TOUGH_ENEMIES_ASCENSION_LEVEL = 8
DEADLY_ENEMIES_ASCENSION_LEVEL = 9


def _ascension_value(ascension_level: int, threshold: int, ascension_value: int, base_value: int) -> int:
    return ascension_value if ascension_level >= threshold else base_value


def _combat_ascension_level(combat: CombatState) -> int:
    return getattr(combat, "ascension_level", 0)


def _deal_damage_to_player(combat: CombatState, creature: Creature, base_dmg: int, hits: int = 1) -> None:
    for _ in range(hits):
        targets = living_player_targets(combat)
        if not targets:
            break
        for target in targets:
            dmg = calculate_damage(base_dmg, creature, target, ValueProp.MOVE, combat)
            apply_damage(target, dmg, ValueProp.MOVE, combat, creature)
        combat._check_combat_end()  # noqa: SLF001
        if combat.is_over:
            break


def _gain_block(creature: Creature, amount: int, combat: CombatState) -> None:
    gain_move_block(creature, amount, combat)


def _gain_unpowered_block(creature: Creature, amount: int, combat: CombatState) -> None:
    if combat.is_over:
        return
    before = creature.block
    creature.gain_block(amount, unpowered=True)
    gained = creature.block - before
    if gained > 0:
        from sts2_env.core.hooks import fire_after_block_gained

        fire_after_block_gained(creature, gained, combat, ValueProp.UNPOWERED, None)


# ========================================================================
# WEAK ENCOUNTERS
# ========================================================================

# ---- DevotedSculptor (HP 162 / 172 asc) ----

DEVOTED_SCULPTOR_MONSTER_ID = "DEVOTED_SCULPTOR"
DEVOTED_SCULPTOR_BASE_HP = 162
DEVOTED_SCULPTOR_TOUGH_HP = 172
DEVOTED_SCULPTOR_RITUAL_GAIN = 9
DEVOTED_SCULPTOR_BASE_SAVAGE_DAMAGE = 12
DEVOTED_SCULPTOR_DEADLY_SAVAGE_DAMAGE = 15
DEVOTED_SCULPTOR_FORBIDDEN_INCANTATION_MOVE = "FORBIDDEN_INCANTATION_MOVE"
DEVOTED_SCULPTOR_SAVAGE_MOVE = "SAVAGE_MOVE"


def create_devoted_sculptor(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        DEVOTED_SCULPTOR_TOUGH_HP,
        DEVOTED_SCULPTOR_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=DEVOTED_SCULPTOR_MONSTER_ID)

    def forbidden_incantation(combat: CombatState) -> None:
        creature.apply_power(PowerId.RITUAL, DEVOTED_SCULPTOR_RITUAL_GAIN)

    def savage(combat: CombatState) -> None:
        savage_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            DEVOTED_SCULPTOR_DEADLY_SAVAGE_DAMAGE,
            DEVOTED_SCULPTOR_BASE_SAVAGE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, savage_dmg)

    savage_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        DEVOTED_SCULPTOR_DEADLY_SAVAGE_DAMAGE,
        DEVOTED_SCULPTOR_BASE_SAVAGE_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        DEVOTED_SCULPTOR_FORBIDDEN_INCANTATION_MOVE: MoveState(
            DEVOTED_SCULPTOR_FORBIDDEN_INCANTATION_MOVE,
            forbidden_incantation,
            [buff_intent()],
            follow_up_id=DEVOTED_SCULPTOR_SAVAGE_MOVE,
        ),
        DEVOTED_SCULPTOR_SAVAGE_MOVE: MoveState(
            DEVOTED_SCULPTOR_SAVAGE_MOVE,
            savage,
            [attack_intent(savage_intent_damage)],
            follow_up_id=DEVOTED_SCULPTOR_SAVAGE_MOVE,
        ),
    }
    return creature, MonsterAI(states, DEVOTED_SCULPTOR_FORBIDDEN_INCANTATION_MOVE)


# ---- ScrollOfBiting (HP 30-37 / 33-39 asc) ----

SCROLL_OF_BITING_BASE_MIN_HP = 30
SCROLL_OF_BITING_BASE_MAX_HP = 37
SCROLL_OF_BITING_TOUGH_MIN_HP = 33
SCROLL_OF_BITING_TOUGH_MAX_HP = 39
SCROLL_OF_BITING_BASE_CHOMP_DAMAGE = 14
SCROLL_OF_BITING_DEADLY_CHOMP_DAMAGE = 16
SCROLL_OF_BITING_BASE_CHEW_DAMAGE = 5
SCROLL_OF_BITING_DEADLY_CHEW_DAMAGE = 6


def create_scroll_of_biting(rng: Rng, starter_move_idx: int = 0, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        SCROLL_OF_BITING_TOUGH_MIN_HP,
        SCROLL_OF_BITING_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        SCROLL_OF_BITING_TOUGH_MAX_HP,
        SCROLL_OF_BITING_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id="SCROLL_OF_BITING")
    chomp_dmg = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        SCROLL_OF_BITING_DEADLY_CHOMP_DAMAGE,
        SCROLL_OF_BITING_BASE_CHOMP_DAMAGE,
    )
    chew_dmg = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        SCROLL_OF_BITING_DEADLY_CHEW_DAMAGE,
        SCROLL_OF_BITING_BASE_CHEW_DAMAGE,
    )

    def chomp(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, chomp_dmg)

    def chew(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, chew_dmg, hits=2)

    def more_teeth(combat: CombatState) -> None:
        creature.apply_power(PowerId.STRENGTH, 2, applier=creature)

    rand = RandomBranchState("rand")
    rand.add_branch("CHOMP", MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch("CHEW", weight=2.0)

    states: dict[str, MonsterState] = {
        "CHOMP": MoveState("CHOMP", chomp, [attack_intent(chomp_dmg)], follow_up_id="MORE_TEETH"),
        "CHEW": MoveState("CHEW", chew, [multi_attack_intent(chew_dmg, 2)], follow_up_id="rand"),
        "MORE_TEETH": MoveState("MORE_TEETH", more_teeth, [buff_intent()], follow_up_id="CHEW"),
        "rand": rand,
    }
    initial = ("CHOMP", "CHEW", "MORE_TEETH")[starter_move_idx % 3]
    creature.apply_power(PowerId.PAPER_CUTS, 2)
    return creature, MonsterAI(states, initial, rng)


# ---- TurretOperator (HP 28-30 / 30-32 asc) ----

TURRET_OPERATOR_MONSTER_ID = "TURRET_OPERATOR"
TURRET_OPERATOR_BASE_HP = 41
TURRET_OPERATOR_TOUGH_HP = 51
TURRET_OPERATOR_BASE_FIRE_DAMAGE = 3
TURRET_OPERATOR_DEADLY_FIRE_DAMAGE = 4
TURRET_OPERATOR_FIRE_REPEAT = 5
TURRET_OPERATOR_RELOAD_STRENGTH = 1
TURRET_OPERATOR_UNLOAD_MOVE = "UNLOAD_MOVE"
TURRET_OPERATOR_UNLOAD_MOVE_2 = "UNLOAD_MOVE_2"
TURRET_OPERATOR_RELOAD_MOVE = "RELOAD_MOVE"


def create_turret_operator(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        TURRET_OPERATOR_TOUGH_HP,
        TURRET_OPERATOR_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=TURRET_OPERATOR_MONSTER_ID)

    def unload(combat: CombatState) -> None:
        fire_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            TURRET_OPERATOR_DEADLY_FIRE_DAMAGE,
            TURRET_OPERATOR_BASE_FIRE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, fire_dmg, hits=TURRET_OPERATOR_FIRE_REPEAT)

    def reload(combat: CombatState) -> None:
        creature.apply_power(PowerId.STRENGTH, TURRET_OPERATOR_RELOAD_STRENGTH, applier=creature)

    fire_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        TURRET_OPERATOR_DEADLY_FIRE_DAMAGE,
        TURRET_OPERATOR_BASE_FIRE_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        TURRET_OPERATOR_UNLOAD_MOVE: MoveState(
            TURRET_OPERATOR_UNLOAD_MOVE,
            unload,
            [multi_attack_intent(fire_intent_damage, TURRET_OPERATOR_FIRE_REPEAT)],
            follow_up_id=TURRET_OPERATOR_UNLOAD_MOVE_2,
        ),
        TURRET_OPERATOR_UNLOAD_MOVE_2: MoveState(
            TURRET_OPERATOR_UNLOAD_MOVE_2,
            unload,
            [multi_attack_intent(fire_intent_damage, TURRET_OPERATOR_FIRE_REPEAT)],
            follow_up_id=TURRET_OPERATOR_RELOAD_MOVE,
        ),
        TURRET_OPERATOR_RELOAD_MOVE: MoveState(
            TURRET_OPERATOR_RELOAD_MOVE,
            reload,
            [buff_intent()],
            follow_up_id=TURRET_OPERATOR_UNLOAD_MOVE,
        ),
    }
    return creature, MonsterAI(states, TURRET_OPERATOR_UNLOAD_MOVE)


# ========================================================================
# NORMAL ENCOUNTERS
# ========================================================================

# ---- Axebot (HP 70-78 / 76-86 asc, +10 per used stock on respawn) ----

AXEBOT_MONSTER_ID = "AXEBOT"
AXEBOT_BASE_MIN_HP = 70
AXEBOT_BASE_MAX_HP = 78
AXEBOT_TOUGH_MIN_HP = 76
AXEBOT_TOUGH_MAX_HP = 86
AXEBOT_INITIAL_STOCK_AMOUNT = 2
AXEBOT_RESPAWN_MAX_HP_BONUS = 10
AXEBOT_BASE_BOOT_UP_BLOCK = 10
AXEBOT_DEADLY_BOOT_UP_BLOCK = 15
AXEBOT_BASE_BOOT_UP_STRENGTH = 3
AXEBOT_DEADLY_BOOT_UP_STRENGTH = 4
AXEBOT_BASE_ONE_TWO_DAMAGE = 10
AXEBOT_DEADLY_ONE_TWO_DAMAGE = 11
AXEBOT_ONE_TWO_REPEAT = 2
AXEBOT_BASE_HAMMER_UPPERCUT_DAMAGE = 14
AXEBOT_DEADLY_HAMMER_UPPERCUT_DAMAGE = 18
AXEBOT_HAMMER_UPPERCUT_DEBUFF = 2
AXEBOT_BOOT_UP_MOVE = "BOOT_UP_MOVE"
AXEBOT_ONE_TWO_MOVE = "ONE_TWO_MOVE"
AXEBOT_HAMMER_UPPERCUT_MOVE = "HAMMER_UPPERCUT_MOVE"


def create_axebot(
    rng: Rng,
    start_with_boot_up: bool = False,
    stock_amount: int | None = None,
    ascension_level: int = 0,
) -> tuple[Creature, MonsterAI]:
    # C#: RespawnCount = 2 - StockAmount; Min/MaxInitialHp gains
    # RespawnCount * 10, i.e. each consumed stock revives the Axebot with
    # +10 max HP.  The sim implements this on the StockPower respawn path,
    # which re-creates the Axebot with stock_amount = remaining stock.
    effective_stock = AXEBOT_INITIAL_STOCK_AMOUNT if stock_amount is None else stock_amount
    respawn_count = AXEBOT_INITIAL_STOCK_AMOUNT - effective_stock
    respawn_hp_bonus = respawn_count * AXEBOT_RESPAWN_MAX_HP_BONUS
    min_hp = (
        _ascension_value(
            ascension_level,
            TOUGH_ENEMIES_ASCENSION_LEVEL,
            AXEBOT_TOUGH_MIN_HP,
            AXEBOT_BASE_MIN_HP,
        )
        + respawn_hp_bonus
    )
    max_hp = (
        _ascension_value(
            ascension_level,
            TOUGH_ENEMIES_ASCENSION_LEVEL,
            AXEBOT_TOUGH_MAX_HP,
            AXEBOT_BASE_MAX_HP,
        )
        + respawn_hp_bonus
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id=AXEBOT_MONSTER_ID)

    def boot_up(combat: CombatState) -> None:
        boot_up_block = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            AXEBOT_DEADLY_BOOT_UP_BLOCK,
            AXEBOT_BASE_BOOT_UP_BLOCK,
        )
        boot_up_strength = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            AXEBOT_DEADLY_BOOT_UP_STRENGTH,
            AXEBOT_BASE_BOOT_UP_STRENGTH,
        )
        _gain_block(creature, boot_up_block, combat)
        creature.apply_power(PowerId.STRENGTH, boot_up_strength * respawn_count)

    def one_two(combat: CombatState) -> None:
        one_two_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            AXEBOT_DEADLY_ONE_TWO_DAMAGE,
            AXEBOT_BASE_ONE_TWO_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, one_two_dmg, hits=AXEBOT_ONE_TWO_REPEAT)

    def hammer_uppercut(combat: CombatState) -> None:
        hammer_uppercut_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            AXEBOT_DEADLY_HAMMER_UPPERCUT_DAMAGE,
            AXEBOT_BASE_HAMMER_UPPERCUT_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, hammer_uppercut_dmg)
        apply_power_to_living_player_targets(combat, PowerId.WEAK, AXEBOT_HAMMER_UPPERCUT_DEBUFF, applier=creature)
        apply_power_to_living_player_targets(combat, PowerId.FRAIL, AXEBOT_HAMMER_UPPERCUT_DEBUFF, applier=creature)

    one_two_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        AXEBOT_DEADLY_ONE_TWO_DAMAGE,
        AXEBOT_BASE_ONE_TWO_DAMAGE,
    )
    hammer_uppercut_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        AXEBOT_DEADLY_HAMMER_UPPERCUT_DAMAGE,
        AXEBOT_BASE_HAMMER_UPPERCUT_DAMAGE,
    )

    # C# chain: BOOT_UP -> HAMMER_UPPERCUT -> ONE_TWO -> HAMMER_UPPERCUT -> ...
    # Initial state is HAMMER_UPPERCUT unless a stock override was provided
    # (sim: stock_amount / start_with_boot_up), in which case it is BOOT_UP.
    states: dict[str, MonsterState] = {
        AXEBOT_BOOT_UP_MOVE: MoveState(
            AXEBOT_BOOT_UP_MOVE,
            boot_up,
            [defend_intent(), buff_intent()],
            follow_up_id=AXEBOT_HAMMER_UPPERCUT_MOVE,
        ),
        AXEBOT_ONE_TWO_MOVE: MoveState(
            AXEBOT_ONE_TWO_MOVE,
            one_two,
            [multi_attack_intent(one_two_intent_damage, AXEBOT_ONE_TWO_REPEAT)],
            follow_up_id=AXEBOT_HAMMER_UPPERCUT_MOVE,
        ),
        AXEBOT_HAMMER_UPPERCUT_MOVE: MoveState(
            AXEBOT_HAMMER_UPPERCUT_MOVE,
            hammer_uppercut,
            [attack_intent(hammer_uppercut_intent_damage), debuff_intent()],
            follow_up_id=AXEBOT_ONE_TWO_MOVE,
        ),
    }

    if stock_amount is None:
        creature.apply_power(PowerId.STOCK, AXEBOT_INITIAL_STOCK_AMOUNT)
    elif stock_amount > 0:
        creature.apply_power(PowerId.STOCK, stock_amount)

    if start_with_boot_up or stock_amount is not None:
        initial = AXEBOT_BOOT_UP_MOVE
    else:
        initial = AXEBOT_HAMMER_UPPERCUT_MOVE
    return creature, MonsterAI(states, initial, rng)


# ---- Fabricator (HP 150 / 155 asc) + bots ----

ZAPBOT_MONSTER_ID = "ZAPBOT"
ZAPBOT_BASE_MIN_HP = 18
ZAPBOT_BASE_MAX_HP = 23
ZAPBOT_TOUGH_MIN_HP = 19
ZAPBOT_TOUGH_MAX_HP = 24
ZAPBOT_BASE_ZAP_DAMAGE = 14
ZAPBOT_DEADLY_ZAP_DAMAGE = 15
ZAPBOT_HIGH_VOLTAGE_AMOUNT = 2
ZAPBOT_ZAP_MOVE = "ZAP"


def create_zapbot(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        ZAPBOT_TOUGH_MIN_HP,
        ZAPBOT_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        ZAPBOT_TOUGH_MAX_HP,
        ZAPBOT_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id=ZAPBOT_MONSTER_ID)

    def zap(combat: CombatState) -> None:
        zap_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            ZAPBOT_DEADLY_ZAP_DAMAGE,
            ZAPBOT_BASE_ZAP_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, zap_dmg)

    zap_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        ZAPBOT_DEADLY_ZAP_DAMAGE,
        ZAPBOT_BASE_ZAP_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        ZAPBOT_ZAP_MOVE: MoveState(
            ZAPBOT_ZAP_MOVE,
            zap,
            [attack_intent(zap_intent_damage)],
            follow_up_id=ZAPBOT_ZAP_MOVE,
        ),
    }
    return creature, MonsterAI(states, ZAPBOT_ZAP_MOVE)


STABBOT_MONSTER_ID = "STABBOT"
STABBOT_BASE_MIN_HP = 18
STABBOT_BASE_MAX_HP = 23
STABBOT_TOUGH_MIN_HP = 19
STABBOT_TOUGH_MAX_HP = 24
STABBOT_BASE_STAB_DAMAGE = 11
STABBOT_DEADLY_STAB_DAMAGE = 12
STABBOT_STAB_FRAIL = 1
STABBOT_STAB_MOVE = "STAB_MOVE"


def create_stabbot(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        STABBOT_TOUGH_MIN_HP,
        STABBOT_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        STABBOT_TOUGH_MAX_HP,
        STABBOT_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id=STABBOT_MONSTER_ID)

    def stab(combat: CombatState) -> None:
        stab_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            STABBOT_DEADLY_STAB_DAMAGE,
            STABBOT_BASE_STAB_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, stab_dmg)
        apply_power_to_living_player_targets(combat, PowerId.FRAIL, STABBOT_STAB_FRAIL, applier=creature)

    stab_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        STABBOT_DEADLY_STAB_DAMAGE,
        STABBOT_BASE_STAB_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        STABBOT_STAB_MOVE: MoveState(
            STABBOT_STAB_MOVE,
            stab,
            [attack_intent(stab_intent_damage), debuff_intent()],
            follow_up_id=STABBOT_STAB_MOVE,
        ),
    }
    return creature, MonsterAI(states, STABBOT_STAB_MOVE)


GUARDBOT_MONSTER_ID = "GUARDBOT"
GUARDBOT_BASE_MIN_HP = 16
GUARDBOT_BASE_MAX_HP = 20
GUARDBOT_TOUGH_MIN_HP = 17
GUARDBOT_TOUGH_MAX_HP = 21
GUARDBOT_GUARD_BLOCK = 15
GUARDBOT_GUARD_MOVE = "GUARD_MOVE"


def create_guardbot(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        GUARDBOT_TOUGH_MIN_HP,
        GUARDBOT_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        GUARDBOT_TOUGH_MAX_HP,
        GUARDBOT_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id=GUARDBOT_MONSTER_ID)

    def guard(combat: CombatState) -> None:
        for enemy in combat.alive_enemies:
            if enemy.monster_id == FABRICATOR_MONSTER_ID:
                _gain_unpowered_block(enemy, GUARDBOT_GUARD_BLOCK, combat)

    states: dict[str, MonsterState] = {
        GUARDBOT_GUARD_MOVE: MoveState(
            GUARDBOT_GUARD_MOVE,
            guard,
            [defend_intent()],
            follow_up_id=GUARDBOT_GUARD_MOVE,
        ),
    }
    return creature, MonsterAI(states, GUARDBOT_GUARD_MOVE)


NOISEBOT_MONSTER_ID = "NOISEBOT"
NOISEBOT_BASE_MIN_HP = 18
NOISEBOT_BASE_MAX_HP = 23
NOISEBOT_TOUGH_MIN_HP = 19
NOISEBOT_TOUGH_MAX_HP = 24
NOISEBOT_DAZED_TO_DISCARD = 1
NOISEBOT_DAZED_TO_DRAW = 1
NOISEBOT_NOISE_MOVE = "NOISE_MOVE"


def create_noisebot(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        NOISEBOT_TOUGH_MIN_HP,
        NOISEBOT_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        NOISEBOT_TOUGH_MAX_HP,
        NOISEBOT_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id=NOISEBOT_MONSTER_ID)

    def noise(combat: CombatState) -> None:
        for target in living_player_targets(combat):
            for _ in range(NOISEBOT_DAZED_TO_DISCARD):
                combat.add_generated_card_to_creature_discard(
                    target,
                    make_dazed(),
                    added_by_player=False,
                )
            for _ in range(NOISEBOT_DAZED_TO_DRAW):
                combat.add_generated_card_to_creature_draw_pile(
                    target,
                    make_dazed(),
                    added_by_player=False,
                    random_position=True,
                )

    states: dict[str, MonsterState] = {
        NOISEBOT_NOISE_MOVE: MoveState(
            NOISEBOT_NOISE_MOVE,
            noise,
            [status_intent()],
            follow_up_id=NOISEBOT_NOISE_MOVE,
        ),
    }
    return creature, MonsterAI(states, NOISEBOT_NOISE_MOVE)


FABRICATOR_MONSTER_ID = "FABRICATOR"
FABRICATOR_BASE_HP = 150
FABRICATOR_TOUGH_HP = 155
FABRICATOR_BASE_FABRICATING_STRIKE_DAMAGE = 18
FABRICATOR_DEADLY_FABRICATING_STRIKE_DAMAGE = 21
FABRICATOR_BASE_DISINTEGRATE_DAMAGE = 11
FABRICATOR_DEADLY_DISINTEGRATE_DAMAGE = 13
FABRICATOR_MAX_LIVING_TEAMMATES_FOR_FABRICATE = 4
FABRICATOR_MINION_AMOUNT = 1
FABRICATOR_LAST_SPAWNED_CREATOR_KEY = "last_spawned_creator"
FABRICATOR_FABRICATE_BRANCH = "fabricateBranch"
FABRICATOR_RAND_MOVE = "RAND"
FABRICATOR_FABRICATE_MOVE = "FABRICATE_MOVE"
FABRICATOR_FABRICATING_STRIKE_MOVE = "FABRICATING_STRIKE_MOVE"
FABRICATOR_DISINTEGRATE_MOVE = "DISINTEGRATE_MOVE"


def create_fabricator(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        FABRICATOR_TOUGH_HP,
        FABRICATOR_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=FABRICATOR_MONSTER_ID)

    _state = {FABRICATOR_LAST_SPAWNED_CREATOR_KEY: None}

    aggro_creators = [create_zapbot, create_stabbot]
    defense_creators = [create_guardbot, create_noisebot]

    def _spawn_bot(combat: CombatState, creators) -> None:
        options = [
            creator for creator in creators
            if creator is not _state[FABRICATOR_LAST_SPAWNED_CREATOR_KEY]
        ]
        creator = rng.choice(options)
        _state[FABRICATOR_LAST_SPAWNED_CREATOR_KEY] = creator
        bot, bot_ai = creator(rng, ascension_level=_combat_ascension_level(combat))
        combat.add_enemy(bot, bot_ai)
        combat.apply_power_to(bot, PowerId.MINION, FABRICATOR_MINION_AMOUNT, applier=creature)

    def _spawn_aggro(combat: CombatState) -> None:
        _spawn_bot(combat, aggro_creators)

    def _spawn_defense(combat: CombatState) -> None:
        _spawn_bot(combat, defense_creators)

    def fabricate(combat: CombatState) -> None:
        _spawn_defense(combat)
        _spawn_aggro(combat)

    def fabricating_strike(combat: CombatState) -> None:
        fabricating_strike_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            FABRICATOR_DEADLY_FABRICATING_STRIKE_DAMAGE,
            FABRICATOR_BASE_FABRICATING_STRIKE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, fabricating_strike_dmg)
        _spawn_aggro(combat)

    def disintegrate(combat: CombatState) -> None:
        disintegrate_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            FABRICATOR_DEADLY_DISINTEGRATE_DAMAGE,
            FABRICATOR_BASE_DISINTEGRATE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, disintegrate_dmg)

    def _can_fabricate() -> bool:
        combat = creature.combat_state
        if combat is None:
            return True
        alive_teammates = [
            enemy for enemy in combat.alive_enemies
            if enemy is not creature and enemy.side == creature.side
        ]
        return len(alive_teammates) < FABRICATOR_MAX_LIVING_TEAMMATES_FOR_FABRICATE

    fabricating_strike_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        FABRICATOR_DEADLY_FABRICATING_STRIKE_DAMAGE,
        FABRICATOR_BASE_FABRICATING_STRIKE_DAMAGE,
    )
    disintegrate_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        FABRICATOR_DEADLY_DISINTEGRATE_DAMAGE,
        FABRICATOR_BASE_DISINTEGRATE_DAMAGE,
    )

    fab_branch = ConditionalBranchState(FABRICATOR_FABRICATE_BRANCH)
    fab_branch.add_branch(_can_fabricate, FABRICATOR_RAND_MOVE)
    fab_branch.add_branch(lambda: True, FABRICATOR_DISINTEGRATE_MOVE)

    fab_rand = RandomBranchState(FABRICATOR_RAND_MOVE)
    fab_rand.add_branch(FABRICATOR_FABRICATE_MOVE)
    fab_rand.add_branch(FABRICATOR_FABRICATING_STRIKE_MOVE)

    states: dict[str, MonsterState] = {
        FABRICATOR_FABRICATE_BRANCH: fab_branch,
        FABRICATOR_RAND_MOVE: fab_rand,
        FABRICATOR_FABRICATE_MOVE: MoveState(
            FABRICATOR_FABRICATE_MOVE,
            fabricate,
            [Intent(IntentType.SUMMON)],
            follow_up_id=FABRICATOR_FABRICATE_BRANCH,
        ),
        FABRICATOR_FABRICATING_STRIKE_MOVE: MoveState(
            FABRICATOR_FABRICATING_STRIKE_MOVE,
            fabricating_strike,
            [attack_intent(fabricating_strike_intent_damage), Intent(IntentType.SUMMON)],
            follow_up_id=FABRICATOR_FABRICATE_BRANCH,
        ),
        FABRICATOR_DISINTEGRATE_MOVE: MoveState(
            FABRICATOR_DISINTEGRATE_MOVE,
            disintegrate,
            [attack_intent(disintegrate_intent_damage)],
            follow_up_id=FABRICATOR_FABRICATE_BRANCH,
        ),
    }
    return creature, MonsterAI(states, FABRICATOR_FABRICATE_BRANCH, rng)


# ---- FrogKnight (HP 191 / 199 asc) ----

FROG_KNIGHT_MONSTER_ID = "FROG_KNIGHT"
FROG_KNIGHT_BASE_HP = 191
FROG_KNIGHT_TOUGH_HP = 199
FROG_KNIGHT_BASE_STRIKE_DOWN_EVIL_DAMAGE = 21
FROG_KNIGHT_DEADLY_STRIKE_DOWN_EVIL_DAMAGE = 23
FROG_KNIGHT_BASE_TONGUE_LASH_DAMAGE = 13
FROG_KNIGHT_DEADLY_TONGUE_LASH_DAMAGE = 14
FROG_KNIGHT_TONGUE_LASH_FRAIL = 2
FROG_KNIGHT_BASE_BEETLE_CHARGE_DAMAGE = 35
FROG_KNIGHT_DEADLY_BEETLE_CHARGE_DAMAGE = 40
FROG_KNIGHT_FOR_THE_QUEEN_STRENGTH = 5
FROG_KNIGHT_BASE_PLATING = 15
FROG_KNIGHT_TOUGH_PLATING = 19
FROG_KNIGHT_BEETLE_CHARGED_KEY = "beetle_charged"
FROG_KNIGHT_TONGUE_LASH_MOVE = "TONGUE_LASH"
FROG_KNIGHT_STRIKE_DOWN_EVIL_MOVE = "STRIKE_DOWN_EVIL"
FROG_KNIGHT_FOR_THE_QUEEN_MOVE = "FOR_THE_QUEEN"
FROG_KNIGHT_HALF_HEALTH_BRANCH = "HALF_HEALTH"
FROG_KNIGHT_BEETLE_CHARGE_MOVE = "BEETLE_CHARGE"


def create_frog_knight(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        FROG_KNIGHT_TOUGH_HP,
        FROG_KNIGHT_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=FROG_KNIGHT_MONSTER_ID)

    _state = {FROG_KNIGHT_BEETLE_CHARGED_KEY: False}

    def tongue_lash(combat: CombatState) -> None:
        tongue_lash_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            FROG_KNIGHT_DEADLY_TONGUE_LASH_DAMAGE,
            FROG_KNIGHT_BASE_TONGUE_LASH_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, tongue_lash_dmg)
        apply_power_to_living_player_targets(combat, PowerId.FRAIL, FROG_KNIGHT_TONGUE_LASH_FRAIL, applier=creature)

    def strike_down_evil(combat: CombatState) -> None:
        strike_down_evil_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            FROG_KNIGHT_DEADLY_STRIKE_DOWN_EVIL_DAMAGE,
            FROG_KNIGHT_BASE_STRIKE_DOWN_EVIL_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, strike_down_evil_dmg)

    def for_the_queen(combat: CombatState) -> None:
        creature.apply_power(PowerId.STRENGTH, FROG_KNIGHT_FOR_THE_QUEEN_STRENGTH)

    def beetle_charge(combat: CombatState) -> None:
        beetle_charge_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            FROG_KNIGHT_DEADLY_BEETLE_CHARGE_DAMAGE,
            FROG_KNIGHT_BASE_BEETLE_CHARGE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, beetle_charge_dmg)
        _state[FROG_KNIGHT_BEETLE_CHARGED_KEY] = True

    # After for_the_queen, check HP for beetle charge
    charge_check = ConditionalBranchState(FROG_KNIGHT_HALF_HEALTH_BRANCH)
    charge_check.add_branch(
        lambda: not _state[FROG_KNIGHT_BEETLE_CHARGED_KEY] and creature.current_hp < creature.max_hp // 2,
        FROG_KNIGHT_BEETLE_CHARGE_MOVE,
    )
    charge_check.add_branch(lambda: True, FROG_KNIGHT_TONGUE_LASH_MOVE)

    tongue_lash_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        FROG_KNIGHT_DEADLY_TONGUE_LASH_DAMAGE,
        FROG_KNIGHT_BASE_TONGUE_LASH_DAMAGE,
    )
    strike_down_evil_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        FROG_KNIGHT_DEADLY_STRIKE_DOWN_EVIL_DAMAGE,
        FROG_KNIGHT_BASE_STRIKE_DOWN_EVIL_DAMAGE,
    )
    beetle_charge_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        FROG_KNIGHT_DEADLY_BEETLE_CHARGE_DAMAGE,
        FROG_KNIGHT_BASE_BEETLE_CHARGE_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        FROG_KNIGHT_TONGUE_LASH_MOVE: MoveState(
            FROG_KNIGHT_TONGUE_LASH_MOVE,
            tongue_lash,
            [attack_intent(tongue_lash_intent_damage), debuff_intent()],
            follow_up_id=FROG_KNIGHT_STRIKE_DOWN_EVIL_MOVE,
        ),
        FROG_KNIGHT_STRIKE_DOWN_EVIL_MOVE: MoveState(
            FROG_KNIGHT_STRIKE_DOWN_EVIL_MOVE,
            strike_down_evil,
            [attack_intent(strike_down_evil_intent_damage)],
            follow_up_id=FROG_KNIGHT_FOR_THE_QUEEN_MOVE,
        ),
        FROG_KNIGHT_FOR_THE_QUEEN_MOVE: MoveState(
            FROG_KNIGHT_FOR_THE_QUEEN_MOVE,
            for_the_queen,
            [buff_intent()],
            follow_up_id=FROG_KNIGHT_HALF_HEALTH_BRANCH,
        ),
        FROG_KNIGHT_HALF_HEALTH_BRANCH: charge_check,
        FROG_KNIGHT_BEETLE_CHARGE_MOVE: MoveState(
            FROG_KNIGHT_BEETLE_CHARGE_MOVE,
            beetle_charge,
            [attack_intent(beetle_charge_intent_damage)],
            follow_up_id=FROG_KNIGHT_TONGUE_LASH_MOVE,
        ),
    }

    plating_amount = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        FROG_KNIGHT_TOUGH_PLATING,
        FROG_KNIGHT_BASE_PLATING,
    )
    creature.apply_power(PowerId.PLATING, plating_amount)
    return creature, MonsterAI(states, FROG_KNIGHT_TONGUE_LASH_MOVE)


# ---- GlobeHead (HP 148 / 158 asc) ----

GLOBE_HEAD_MONSTER_ID = "GLOBE_HEAD"
GLOBE_HEAD_BASE_HP = 148
GLOBE_HEAD_TOUGH_HP = 158
GLOBE_HEAD_BASE_SHOCKING_SLAP_DAMAGE = 13
GLOBE_HEAD_DEADLY_SHOCKING_SLAP_DAMAGE = 14
GLOBE_HEAD_SHOCKING_SLAP_FRAIL = 2
GLOBE_HEAD_BASE_THUNDER_STRIKE_DAMAGE = 6
GLOBE_HEAD_DEADLY_THUNDER_STRIKE_DAMAGE = 7
GLOBE_HEAD_THUNDER_STRIKE_REPEAT = 3
GLOBE_HEAD_BASE_GALVANIC_BURST_DAMAGE = 16
GLOBE_HEAD_DEADLY_GALVANIC_BURST_DAMAGE = 17
GLOBE_HEAD_GALVANIC_BURST_STRENGTH = 2
GLOBE_HEAD_BASE_GALVANIC_AMOUNT = 6
GLOBE_HEAD_DEADLY_GALVANIC_AMOUNT = 8
GLOBE_HEAD_SHOCKING_SLAP_MOVE = "SHOCKING_SLAP"
GLOBE_HEAD_THUNDER_STRIKE_MOVE = "THUNDER_STRIKE"
GLOBE_HEAD_GALVANIC_BURST_MOVE = "GALVANIC_BURST"


def create_globe_head(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        GLOBE_HEAD_TOUGH_HP,
        GLOBE_HEAD_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=GLOBE_HEAD_MONSTER_ID)

    def shocking_slap(combat: CombatState) -> None:
        shocking_slap_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            GLOBE_HEAD_DEADLY_SHOCKING_SLAP_DAMAGE,
            GLOBE_HEAD_BASE_SHOCKING_SLAP_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, shocking_slap_dmg)
        apply_power_to_living_player_targets(combat, PowerId.FRAIL, GLOBE_HEAD_SHOCKING_SLAP_FRAIL, applier=creature)

    def thunder_strike(combat: CombatState) -> None:
        thunder_strike_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            GLOBE_HEAD_DEADLY_THUNDER_STRIKE_DAMAGE,
            GLOBE_HEAD_BASE_THUNDER_STRIKE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, thunder_strike_dmg, hits=GLOBE_HEAD_THUNDER_STRIKE_REPEAT)

    def galvanic_burst(combat: CombatState) -> None:
        galvanic_burst_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            GLOBE_HEAD_DEADLY_GALVANIC_BURST_DAMAGE,
            GLOBE_HEAD_BASE_GALVANIC_BURST_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, galvanic_burst_dmg)
        combat.apply_power_to(creature, PowerId.STRENGTH, GLOBE_HEAD_GALVANIC_BURST_STRENGTH, applier=creature)

    shocking_slap_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        GLOBE_HEAD_DEADLY_SHOCKING_SLAP_DAMAGE,
        GLOBE_HEAD_BASE_SHOCKING_SLAP_DAMAGE,
    )
    thunder_strike_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        GLOBE_HEAD_DEADLY_THUNDER_STRIKE_DAMAGE,
        GLOBE_HEAD_BASE_THUNDER_STRIKE_DAMAGE,
    )
    galvanic_burst_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        GLOBE_HEAD_DEADLY_GALVANIC_BURST_DAMAGE,
        GLOBE_HEAD_BASE_GALVANIC_BURST_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        GLOBE_HEAD_SHOCKING_SLAP_MOVE: MoveState(
            GLOBE_HEAD_SHOCKING_SLAP_MOVE,
            shocking_slap,
            [attack_intent(shocking_slap_intent_damage), debuff_intent()],
            follow_up_id=GLOBE_HEAD_THUNDER_STRIKE_MOVE,
        ),
        GLOBE_HEAD_THUNDER_STRIKE_MOVE: MoveState(
            GLOBE_HEAD_THUNDER_STRIKE_MOVE,
            thunder_strike,
            [multi_attack_intent(thunder_strike_intent_damage, GLOBE_HEAD_THUNDER_STRIKE_REPEAT)],
            follow_up_id=GLOBE_HEAD_GALVANIC_BURST_MOVE,
        ),
        GLOBE_HEAD_GALVANIC_BURST_MOVE: MoveState(
            GLOBE_HEAD_GALVANIC_BURST_MOVE,
            galvanic_burst,
            [attack_intent(galvanic_burst_intent_damage), buff_intent()],
            follow_up_id=GLOBE_HEAD_SHOCKING_SLAP_MOVE,
        ),
    }

    creature.apply_power(PowerId.GALVANIC, _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        GLOBE_HEAD_DEADLY_GALVANIC_AMOUNT,
        GLOBE_HEAD_BASE_GALVANIC_AMOUNT,
    ))
    return creature, MonsterAI(states, GLOBE_HEAD_SHOCKING_SLAP_MOVE)


# ---- OwlMagistrate (HP 231 / 247 asc) ----

OWL_MAGISTRATE_MONSTER_ID = "OWL_MAGISTRATE"
OWL_MAGISTRATE_BASE_HP = 231
OWL_MAGISTRATE_TOUGH_HP = 247
OWL_MAGISTRATE_BASE_SCRUTINY_DAMAGE = 16
OWL_MAGISTRATE_DEADLY_SCRUTINY_DAMAGE = 17
OWL_MAGISTRATE_PECK_ASSAULT_DAMAGE = 4
OWL_MAGISTRATE_PECK_ASSAULT_REPEAT = 6
OWL_MAGISTRATE_BASE_VERDICT_DAMAGE = 33
OWL_MAGISTRATE_DEADLY_VERDICT_DAMAGE = 36
OWL_MAGISTRATE_JUDICIAL_FLIGHT_SOAR = 1
OWL_MAGISTRATE_VERDICT_VULNERABLE = 4
OWL_MAGISTRATE_SCRUTINY_MOVE = "MAGISTRATE_SCRUTINY"
OWL_MAGISTRATE_PECK_ASSAULT_MOVE = "PECK_ASSAULT"
OWL_MAGISTRATE_JUDICIAL_FLIGHT_MOVE = "JUDICIAL_FLIGHT"
OWL_MAGISTRATE_VERDICT_MOVE = "VERDICT"


def create_owl_magistrate(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        OWL_MAGISTRATE_TOUGH_HP,
        OWL_MAGISTRATE_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=OWL_MAGISTRATE_MONSTER_ID)

    def magistrate_scrutiny(combat: CombatState) -> None:
        scrutiny_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            OWL_MAGISTRATE_DEADLY_SCRUTINY_DAMAGE,
            OWL_MAGISTRATE_BASE_SCRUTINY_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, scrutiny_dmg)

    def peck_assault(combat: CombatState) -> None:
        _deal_damage_to_player(
            combat,
            creature,
            OWL_MAGISTRATE_PECK_ASSAULT_DAMAGE,
            hits=OWL_MAGISTRATE_PECK_ASSAULT_REPEAT,
        )

    def judicial_flight(combat: CombatState) -> None:
        creature.apply_power(PowerId.SOAR, OWL_MAGISTRATE_JUDICIAL_FLIGHT_SOAR, applier=creature)

    def verdict(combat: CombatState) -> None:
        verdict_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            OWL_MAGISTRATE_DEADLY_VERDICT_DAMAGE,
            OWL_MAGISTRATE_BASE_VERDICT_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, verdict_dmg)
        apply_power_to_living_player_targets(
            combat,
            PowerId.VULNERABLE,
            OWL_MAGISTRATE_VERDICT_VULNERABLE,
            applier=creature,
        )
        creature.powers.pop(PowerId.SOAR, None)

    scrutiny_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        OWL_MAGISTRATE_DEADLY_SCRUTINY_DAMAGE,
        OWL_MAGISTRATE_BASE_SCRUTINY_DAMAGE,
    )
    verdict_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        OWL_MAGISTRATE_DEADLY_VERDICT_DAMAGE,
        OWL_MAGISTRATE_BASE_VERDICT_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        OWL_MAGISTRATE_SCRUTINY_MOVE: MoveState(
            OWL_MAGISTRATE_SCRUTINY_MOVE,
            magistrate_scrutiny,
            [attack_intent(scrutiny_intent_damage)],
            follow_up_id=OWL_MAGISTRATE_PECK_ASSAULT_MOVE,
        ),
        OWL_MAGISTRATE_PECK_ASSAULT_MOVE: MoveState(
            OWL_MAGISTRATE_PECK_ASSAULT_MOVE,
            peck_assault,
            [multi_attack_intent(OWL_MAGISTRATE_PECK_ASSAULT_DAMAGE, OWL_MAGISTRATE_PECK_ASSAULT_REPEAT)],
            follow_up_id=OWL_MAGISTRATE_JUDICIAL_FLIGHT_MOVE,
        ),
        OWL_MAGISTRATE_JUDICIAL_FLIGHT_MOVE: MoveState(
            OWL_MAGISTRATE_JUDICIAL_FLIGHT_MOVE,
            judicial_flight,
            [buff_intent()],
            follow_up_id=OWL_MAGISTRATE_VERDICT_MOVE,
        ),
        OWL_MAGISTRATE_VERDICT_MOVE: MoveState(
            OWL_MAGISTRATE_VERDICT_MOVE,
            verdict,
            [attack_intent(verdict_intent_damage), debuff_intent()],
            follow_up_id=OWL_MAGISTRATE_SCRUTINY_MOVE,
        ),
    }
    return creature, MonsterAI(states, OWL_MAGISTRATE_SCRUTINY_MOVE)


# ---- SlimedBerserker (HP 60-65 / 64-69 asc) ----

SLIMED_BERSERKER_MONSTER_ID = "SLIMED_BERSERKER"
SLIMED_BERSERKER_BASE_HP = 261
SLIMED_BERSERKER_TOUGH_HP = 281
SLIMED_BERSERKER_BASE_PUMMELING_DAMAGE = 4
SLIMED_BERSERKER_DEADLY_PUMMELING_DAMAGE = 5
SLIMED_BERSERKER_PUMMELING_REPEAT = 4
SLIMED_BERSERKER_VOMIT_ICHOR_SLIMED = 10
SLIMED_BERSERKER_LEECHING_HUG_WEAK = 3
SLIMED_BERSERKER_LEECHING_HUG_STRENGTH = 3
SLIMED_BERSERKER_BASE_SMOTHER_DAMAGE = 30
SLIMED_BERSERKER_DEADLY_SMOTHER_DAMAGE = 33
SLIMED_BERSERKER_VOMIT_ICHOR_MOVE = "VOMIT_ICHOR_MOVE"
SLIMED_BERSERKER_FURIOUS_PUMMELING_MOVE = "FURIOUS_PUMMELING_MOVE"
SLIMED_BERSERKER_LEECHING_HUG_MOVE = "LEECHING_HUG_MOVE"
SLIMED_BERSERKER_SMOTHER_MOVE = "SMOTHER_MOVE"


def create_slimed_berserker(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        SLIMED_BERSERKER_TOUGH_HP,
        SLIMED_BERSERKER_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=SLIMED_BERSERKER_MONSTER_ID)

    def vomit_ichor(combat: CombatState) -> None:
        for target in living_player_targets(combat):
            for _ in range(SLIMED_BERSERKER_VOMIT_ICHOR_SLIMED):
                combat.add_generated_card_to_creature_discard(
                    target,
                    make_slimed(),
                    added_by_player=False,
                )

    def furious_pummeling(combat: CombatState) -> None:
        pummeling_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            SLIMED_BERSERKER_DEADLY_PUMMELING_DAMAGE,
            SLIMED_BERSERKER_BASE_PUMMELING_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, pummeling_dmg, hits=SLIMED_BERSERKER_PUMMELING_REPEAT)

    def leeching_hug(combat: CombatState) -> None:
        apply_power_to_living_player_targets(
            combat,
            PowerId.WEAK,
            SLIMED_BERSERKER_LEECHING_HUG_WEAK,
            applier=creature,
        )
        creature.apply_power(PowerId.STRENGTH, SLIMED_BERSERKER_LEECHING_HUG_STRENGTH)

    def smother(combat: CombatState) -> None:
        smother_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            SLIMED_BERSERKER_DEADLY_SMOTHER_DAMAGE,
            SLIMED_BERSERKER_BASE_SMOTHER_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, smother_dmg)

    pummeling_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        SLIMED_BERSERKER_DEADLY_PUMMELING_DAMAGE,
        SLIMED_BERSERKER_BASE_PUMMELING_DAMAGE,
    )
    smother_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        SLIMED_BERSERKER_DEADLY_SMOTHER_DAMAGE,
        SLIMED_BERSERKER_BASE_SMOTHER_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        SLIMED_BERSERKER_VOMIT_ICHOR_MOVE: MoveState(
            SLIMED_BERSERKER_VOMIT_ICHOR_MOVE,
            vomit_ichor,
            [status_intent()],
            follow_up_id=SLIMED_BERSERKER_FURIOUS_PUMMELING_MOVE,
        ),
        SLIMED_BERSERKER_FURIOUS_PUMMELING_MOVE: MoveState(
            SLIMED_BERSERKER_FURIOUS_PUMMELING_MOVE,
            furious_pummeling,
            [multi_attack_intent(pummeling_intent_damage, SLIMED_BERSERKER_PUMMELING_REPEAT)],
            follow_up_id=SLIMED_BERSERKER_LEECHING_HUG_MOVE,
        ),
        SLIMED_BERSERKER_LEECHING_HUG_MOVE: MoveState(
            SLIMED_BERSERKER_LEECHING_HUG_MOVE,
            leeching_hug,
            [debuff_intent(), buff_intent()],
            follow_up_id=SLIMED_BERSERKER_SMOTHER_MOVE,
        ),
        SLIMED_BERSERKER_SMOTHER_MOVE: MoveState(
            SLIMED_BERSERKER_SMOTHER_MOVE,
            smother,
            [attack_intent(smother_intent_damage)],
            follow_up_id=SLIMED_BERSERKER_VOMIT_ICHOR_MOVE,
        ),
    }
    return creature, MonsterAI(states, SLIMED_BERSERKER_VOMIT_ICHOR_MOVE)


# ---- TheLost + TheForgotten ----

THE_LOST_MONSTER_ID = "THE_LOST"
THE_LOST_BASE_HP = 93
THE_LOST_TOUGH_HP = 99
THE_LOST_DEBILITATING_SMOG_STRENGTH = -2
THE_LOST_DEBILITATING_SMOG_SELF_STRENGTH = 2
THE_LOST_BASE_EYE_LASERS_DAMAGE = 4
THE_LOST_DEADLY_EYE_LASERS_DAMAGE = 5
THE_LOST_EYE_LASERS_REPEAT = 2
THE_LOST_POSSESS_STRENGTH = 1
THE_LOST_DEBILITATING_SMOG_MOVE = "DEBILITATING_SMOG"
THE_LOST_EYE_LASERS_MOVE = "EYE_LASERS"

THE_FORGOTTEN_MONSTER_ID = "THE_FORGOTTEN"
THE_FORGOTTEN_BASE_HP = 106
THE_FORGOTTEN_TOUGH_HP = 111
THE_FORGOTTEN_MIASMA_DEXTERITY = -2
THE_FORGOTTEN_MIASMA_BLOCK = 8
THE_FORGOTTEN_MIASMA_SELF_DEXTERITY = 2
THE_FORGOTTEN_BASE_DREAD_DAMAGE = 13
THE_FORGOTTEN_DEADLY_DREAD_DAMAGE = 15
THE_FORGOTTEN_POSSESS_SPEED = 1
THE_FORGOTTEN_MIASMA_MOVE = "MIASMA"
THE_FORGOTTEN_DREAD_MOVE = "DREAD"


def create_the_lost(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        THE_LOST_TOUGH_HP,
        THE_LOST_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=THE_LOST_MONSTER_ID)

    def debilitating_smog(combat: CombatState) -> None:
        apply_power_to_living_player_targets(
            combat,
            PowerId.STRENGTH,
            THE_LOST_DEBILITATING_SMOG_STRENGTH,
            applier=creature,
        )
        creature.apply_power(PowerId.STRENGTH, THE_LOST_DEBILITATING_SMOG_SELF_STRENGTH, applier=creature)

    def eye_lasers(combat: CombatState) -> None:
        eye_lasers_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            THE_LOST_DEADLY_EYE_LASERS_DAMAGE,
            THE_LOST_BASE_EYE_LASERS_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, eye_lasers_dmg, hits=THE_LOST_EYE_LASERS_REPEAT)

    eye_lasers_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        THE_LOST_DEADLY_EYE_LASERS_DAMAGE,
        THE_LOST_BASE_EYE_LASERS_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        THE_LOST_DEBILITATING_SMOG_MOVE: MoveState(
            THE_LOST_DEBILITATING_SMOG_MOVE,
            debilitating_smog,
            [debuff_intent(), buff_intent()],
            follow_up_id=THE_LOST_EYE_LASERS_MOVE,
        ),
        THE_LOST_EYE_LASERS_MOVE: MoveState(
            THE_LOST_EYE_LASERS_MOVE,
            eye_lasers,
            [multi_attack_intent(eye_lasers_intent_damage, THE_LOST_EYE_LASERS_REPEAT)],
            follow_up_id=THE_LOST_DEBILITATING_SMOG_MOVE,
        ),
    }
    creature.apply_power(PowerId.POSSESS_STRENGTH, THE_LOST_POSSESS_STRENGTH)
    return creature, MonsterAI(states, THE_LOST_DEBILITATING_SMOG_MOVE)


def create_the_forgotten(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        THE_FORGOTTEN_TOUGH_HP,
        THE_FORGOTTEN_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=THE_FORGOTTEN_MONSTER_ID)

    def _dread_damage(combat: CombatState | None) -> int:
        # C#: DreadDamage = ascension base + creature's Dexterity amount
        ascension = _combat_ascension_level(combat) if combat is not None else 0
        base = _ascension_value(
            ascension,
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            THE_FORGOTTEN_DEADLY_DREAD_DAMAGE,
            THE_FORGOTTEN_BASE_DREAD_DAMAGE,
        )
        return base + creature.get_power_amount(PowerId.DEXTERITY)

    def miasma(combat: CombatState) -> None:
        apply_power_to_living_player_targets(
            combat,
            PowerId.DEXTERITY,
            THE_FORGOTTEN_MIASMA_DEXTERITY,
            applier=creature,
        )
        _gain_block(creature, THE_FORGOTTEN_MIASMA_BLOCK, combat)
        creature.apply_power(PowerId.DEXTERITY, THE_FORGOTTEN_MIASMA_SELF_DEXTERITY, applier=creature)
        # C# SingleAttackIntent(() => DreadDamage) is dynamic: refresh the
        # displayed intent damage now that self-Dexterity is applied.
        states[THE_FORGOTTEN_DREAD_MOVE].intents[0].damage = _dread_damage(combat)

    def dread(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, _dread_damage(combat))

    dread_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        THE_FORGOTTEN_DEADLY_DREAD_DAMAGE,
        THE_FORGOTTEN_BASE_DREAD_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        THE_FORGOTTEN_MIASMA_MOVE: MoveState(
            THE_FORGOTTEN_MIASMA_MOVE,
            miasma,
            [debuff_intent(), defend_intent(), buff_intent()],
            follow_up_id=THE_FORGOTTEN_DREAD_MOVE,
        ),
        THE_FORGOTTEN_DREAD_MOVE: MoveState(
            THE_FORGOTTEN_DREAD_MOVE,
            dread,
            [attack_intent(dread_intent_damage)],
            follow_up_id=THE_FORGOTTEN_MIASMA_MOVE,
        ),
    }
    creature.apply_power(PowerId.POSSESS_SPEED, THE_FORGOTTEN_POSSESS_SPEED)
    return creature, MonsterAI(states, THE_FORGOTTEN_MIASMA_MOVE)


# ---- ConstructMenagerie ----


# ========================================================================
# ELITE ENCOUNTERS
# ========================================================================

# ---- Knights (FlailKnight, MagiKnight, SpectralKnight) ----

FLAIL_KNIGHT_MONSTER_ID = "FLAIL_KNIGHT"
FLAIL_KNIGHT_BASE_HP = 101
FLAIL_KNIGHT_TOUGH_HP = 108
FLAIL_KNIGHT_BASE_FLAIL_DAMAGE = 9
FLAIL_KNIGHT_DEADLY_FLAIL_DAMAGE = 10
FLAIL_KNIGHT_FLAIL_REPEAT = 2
FLAIL_KNIGHT_BASE_RAM_DAMAGE = 15
FLAIL_KNIGHT_DEADLY_RAM_DAMAGE = 17
FLAIL_KNIGHT_WAR_CHANT_STRENGTH = 3
FLAIL_KNIGHT_RANDOM_STATE = "RAND"
FLAIL_KNIGHT_WAR_CHANT_MOVE = "WAR_CHANT"
FLAIL_KNIGHT_FLAIL_MOVE = "FLAIL_MOVE"
FLAIL_KNIGHT_RAM_MOVE = "RAM_MOVE"

MAGI_KNIGHT_MONSTER_ID = "MAGI_KNIGHT"
MAGI_KNIGHT_BASE_HP = 82
MAGI_KNIGHT_TOUGH_HP = 89
MAGI_KNIGHT_BASE_POWER_SHIELD_DAMAGE = 6
MAGI_KNIGHT_DEADLY_POWER_SHIELD_DAMAGE = 7
MAGI_KNIGHT_BASE_POWER_SHIELD_BLOCK = 5
MAGI_KNIGHT_TOUGH_POWER_SHIELD_BLOCK = 9
MAGI_KNIGHT_DAMPEN_AMOUNT = 1
MAGI_KNIGHT_BASE_SPEAR_DAMAGE = 10
MAGI_KNIGHT_DEADLY_SPEAR_DAMAGE = 11
MAGI_KNIGHT_BASE_BOMB_DAMAGE = 35
MAGI_KNIGHT_DEADLY_BOMB_DAMAGE = 40
MAGI_KNIGHT_POWER_SHIELD_MOVE = "POWER_SHIELD_MOVE"
MAGI_KNIGHT_DAMPEN_MOVE = "DAMPEN_MOVE"
MAGI_KNIGHT_RAM_MOVE = "RAM_MOVE"
MAGI_KNIGHT_PREP_MOVE = "PREP_MOVE"
MAGI_KNIGHT_MAGIC_BOMB_MOVE = "MAGIC_BOMB"

SPECTRAL_KNIGHT_MONSTER_ID = "SPECTRAL_KNIGHT"
SPECTRAL_KNIGHT_BASE_HP = 93
SPECTRAL_KNIGHT_TOUGH_HP = 97
SPECTRAL_KNIGHT_HEX_AMOUNT = 2
SPECTRAL_KNIGHT_BASE_SOUL_SLASH_DAMAGE = 15
SPECTRAL_KNIGHT_DEADLY_SOUL_SLASH_DAMAGE = 17
SPECTRAL_KNIGHT_BASE_SOUL_FLAME_DAMAGE = 3
SPECTRAL_KNIGHT_DEADLY_SOUL_FLAME_DAMAGE = 4
SPECTRAL_KNIGHT_SOUL_FLAME_REPEAT = 3
SPECTRAL_KNIGHT_RANDOM_STATE = "RAND"
SPECTRAL_KNIGHT_HEX_MOVE = "HEX"
SPECTRAL_KNIGHT_SOUL_SLASH_MOVE = "SOUL_SLASH"
SPECTRAL_KNIGHT_SOUL_FLAME_MOVE = "SOUL_FLAME"

MECHA_KNIGHT_MONSTER_ID = "MECHA_KNIGHT"
MECHA_KNIGHT_BASE_HP = 300
MECHA_KNIGHT_TOUGH_HP = 320
MECHA_KNIGHT_BASE_CHARGE_DAMAGE = 25
MECHA_KNIGHT_DEADLY_CHARGE_DAMAGE = 30
MECHA_KNIGHT_BASE_HEAVY_CLEAVE_DAMAGE = 35
MECHA_KNIGHT_DEADLY_HEAVY_CLEAVE_DAMAGE = 40
MECHA_KNIGHT_WINDUP_BLOCK = 15
MECHA_KNIGHT_BASE_FLAMETHROWER_DAMAGE = 8
MECHA_KNIGHT_DEADLY_FLAMETHROWER_DAMAGE = 12
MECHA_KNIGHT_FLAMETHROWER_BURNS = 4
MECHA_KNIGHT_WINDUP_STRENGTH = 5
MECHA_KNIGHT_ARTIFACT = 3
MECHA_KNIGHT_CHARGE_MOVE = "CHARGE_MOVE"
MECHA_KNIGHT_FLAMETHROWER_MOVE = "FLAMETHROWER_MOVE"
MECHA_KNIGHT_WINDUP_MOVE = "WINDUP_MOVE"
MECHA_KNIGHT_HEAVY_CLEAVE_MOVE = "HEAVY_CLEAVE_MOVE"


def create_flail_knight(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        FLAIL_KNIGHT_TOUGH_HP,
        FLAIL_KNIGHT_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=FLAIL_KNIGHT_MONSTER_ID)

    def war_chant(combat: CombatState) -> None:
        creature.apply_power(PowerId.STRENGTH, FLAIL_KNIGHT_WAR_CHANT_STRENGTH)

    def flail(combat: CombatState) -> None:
        flail_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            FLAIL_KNIGHT_DEADLY_FLAIL_DAMAGE,
            FLAIL_KNIGHT_BASE_FLAIL_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, flail_dmg, hits=FLAIL_KNIGHT_FLAIL_REPEAT)

    def ram(combat: CombatState) -> None:
        ram_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            FLAIL_KNIGHT_DEADLY_RAM_DAMAGE,
            FLAIL_KNIGHT_BASE_RAM_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, ram_dmg)

    flail_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        FLAIL_KNIGHT_DEADLY_FLAIL_DAMAGE,
        FLAIL_KNIGHT_BASE_FLAIL_DAMAGE,
    )
    ram_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        FLAIL_KNIGHT_DEADLY_RAM_DAMAGE,
        FLAIL_KNIGHT_BASE_RAM_DAMAGE,
    )

    rand = RandomBranchState(FLAIL_KNIGHT_RANDOM_STATE)
    rand.add_branch(FLAIL_KNIGHT_WAR_CHANT_MOVE, MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch(FLAIL_KNIGHT_FLAIL_MOVE, MoveRepeatType.CAN_REPEAT_FOREVER, weight=2.0)
    rand.add_branch(FLAIL_KNIGHT_RAM_MOVE, MoveRepeatType.CAN_REPEAT_FOREVER, weight=2.0)

    states: dict[str, MonsterState] = {
        FLAIL_KNIGHT_RANDOM_STATE: rand,
        FLAIL_KNIGHT_WAR_CHANT_MOVE: MoveState(
            FLAIL_KNIGHT_WAR_CHANT_MOVE,
            war_chant,
            [buff_intent()],
            follow_up_id=FLAIL_KNIGHT_RANDOM_STATE,
        ),
        FLAIL_KNIGHT_FLAIL_MOVE: MoveState(
            FLAIL_KNIGHT_FLAIL_MOVE,
            flail,
            [multi_attack_intent(flail_intent_damage, FLAIL_KNIGHT_FLAIL_REPEAT)],
            follow_up_id=FLAIL_KNIGHT_RANDOM_STATE,
        ),
        FLAIL_KNIGHT_RAM_MOVE: MoveState(
            FLAIL_KNIGHT_RAM_MOVE,
            ram,
            [attack_intent(ram_intent_damage)],
            follow_up_id=FLAIL_KNIGHT_RANDOM_STATE,
        ),
    }
    return creature, MonsterAI(states, FLAIL_KNIGHT_RAM_MOVE)


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
    rand.add_branch("FLAIL_MOVE", MoveRepeatType.CAN_REPEAT_FOREVER, weight=2.0)
    rand.add_branch("RAM_MOVE", MoveRepeatType.CAN_REPEAT_FOREVER, weight=2.0)

    states: dict[str, MonsterState] = {
        "RAND": rand,
        "WAR_CHANT": MoveState("WAR_CHANT", war_chant, [buff_intent()], follow_up_id="RAND"),
        "FLAIL_MOVE": MoveState("FLAIL_MOVE", flail, [multi_attack_intent(flail_dmg, 2)], follow_up_id="RAND"),
        "RAM_MOVE": MoveState("RAM_MOVE", ram, [attack_intent(ram_dmg)], follow_up_id="RAND"),
    }

    # AfterAddedToRoom: Strength+6, Plating(6) on top of FlailKnight's base
    creature.apply_power(PowerId.STRENGTH, 6)
    creature.apply_power(PowerId.PLATING, 6)
    return creature, MonsterAI(states, "RAM_MOVE")


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
        combat.apply_power_to(creature, PowerId.STRENGTH, enrage_str, applier=creature)

    # Default ally count checker: count alive teammates excluding self
    def _default_ally_count() -> int:
        combat = creature.combat_state
        if combat is None:
            return 0
        return len(combat.get_teammates_of(creature))

    ally_count_fn = get_ally_count or _default_ally_count

    # Conditional: if allies alive -> SHIELD_SLAM, else -> SMASH (self loop)
    shield_slam_branch = ConditionalBranchState("SHIELD_SLAM_BRANCH")
    shield_slam_branch.add_branch(lambda: ally_count_fn() > 0, "SHIELD_SLAM_MOVE")
    shield_slam_branch.add_branch(lambda: ally_count_fn() == 0, "SMASH_MOVE")

    states: dict[str, MonsterState] = {
        "SHIELD_SLAM_BRANCH": shield_slam_branch,
        "SHIELD_SLAM_MOVE": MoveState("SHIELD_SLAM_MOVE", shield_slam, [attack_intent(shield_slam_dmg)], follow_up_id="SHIELD_SLAM_BRANCH"),
        "SMASH_MOVE": MoveState("SMASH_MOVE", smash, [attack_intent(smash_dmg), buff_intent()], follow_up_id="SMASH_MOVE"),
    }

    creature.apply_power(PowerId.RAMPART, 25)
    return creature, MonsterAI(states, "SHIELD_SLAM_MOVE")


def create_magi_knight(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        MAGI_KNIGHT_TOUGH_HP,
        MAGI_KNIGHT_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=MAGI_KNIGHT_MONSTER_ID)

    def power_shield(combat: CombatState) -> None:
        power_shield_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            MAGI_KNIGHT_DEADLY_POWER_SHIELD_DAMAGE,
            MAGI_KNIGHT_BASE_POWER_SHIELD_DAMAGE,
        )
        power_shield_block = _ascension_value(
            _combat_ascension_level(combat),
            TOUGH_ENEMIES_ASCENSION_LEVEL,
            MAGI_KNIGHT_TOUGH_POWER_SHIELD_BLOCK,
            MAGI_KNIGHT_BASE_POWER_SHIELD_BLOCK,
        )
        _deal_damage_to_player(combat, creature, power_shield_dmg)
        _gain_block(creature, power_shield_block, combat)

    def dampen(combat: CombatState) -> None:
        for target in living_player_targets(combat):
            power = target.powers.get(PowerId.DAMPEN)
            if power is not None:
                add_caster = getattr(power, "add_caster", None)
                if callable(add_caster):
                    add_caster(creature)
                continue
            combat.apply_power_to(target, PowerId.DAMPEN, MAGI_KNIGHT_DAMPEN_AMOUNT, applier=creature)
            power = target.powers.get(PowerId.DAMPEN)
            if power is not None:
                add_caster = getattr(power, "add_caster", None)
                if callable(add_caster):
                    add_caster(creature)

    def spear(combat: CombatState) -> None:
        spear_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            MAGI_KNIGHT_DEADLY_SPEAR_DAMAGE,
            MAGI_KNIGHT_BASE_SPEAR_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, spear_dmg)

    def prep(combat: CombatState) -> None:
        power_shield_block = _ascension_value(
            _combat_ascension_level(combat),
            TOUGH_ENEMIES_ASCENSION_LEVEL,
            MAGI_KNIGHT_TOUGH_POWER_SHIELD_BLOCK,
            MAGI_KNIGHT_BASE_POWER_SHIELD_BLOCK,
        )
        _gain_block(creature, power_shield_block, combat)

    def magic_bomb(combat: CombatState) -> None:
        bomb_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            MAGI_KNIGHT_DEADLY_BOMB_DAMAGE,
            MAGI_KNIGHT_BASE_BOMB_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, bomb_dmg)

    power_shield_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        MAGI_KNIGHT_DEADLY_POWER_SHIELD_DAMAGE,
        MAGI_KNIGHT_BASE_POWER_SHIELD_DAMAGE,
    )
    spear_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        MAGI_KNIGHT_DEADLY_SPEAR_DAMAGE,
        MAGI_KNIGHT_BASE_SPEAR_DAMAGE,
    )
    bomb_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        MAGI_KNIGHT_DEADLY_BOMB_DAMAGE,
        MAGI_KNIGHT_BASE_BOMB_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        MAGI_KNIGHT_POWER_SHIELD_MOVE: MoveState(
            MAGI_KNIGHT_POWER_SHIELD_MOVE,
            power_shield,
            [attack_intent(power_shield_intent_damage), defend_intent()],
            follow_up_id=MAGI_KNIGHT_DAMPEN_MOVE,
        ),
        MAGI_KNIGHT_DAMPEN_MOVE: MoveState(
            MAGI_KNIGHT_DAMPEN_MOVE,
            dampen,
            [debuff_intent()],
            follow_up_id=MAGI_KNIGHT_RAM_MOVE,
        ),
        MAGI_KNIGHT_RAM_MOVE: MoveState(
            MAGI_KNIGHT_RAM_MOVE,
            spear,
            [attack_intent(spear_intent_damage)],
            follow_up_id=MAGI_KNIGHT_PREP_MOVE,
        ),
        MAGI_KNIGHT_PREP_MOVE: MoveState(
            MAGI_KNIGHT_PREP_MOVE,
            prep,
            [defend_intent()],
            follow_up_id=MAGI_KNIGHT_MAGIC_BOMB_MOVE,
        ),
        MAGI_KNIGHT_MAGIC_BOMB_MOVE: MoveState(
            MAGI_KNIGHT_MAGIC_BOMB_MOVE,
            magic_bomb,
            [attack_intent(bomb_intent_damage)],
            follow_up_id=MAGI_KNIGHT_RAM_MOVE,
        ),
    }
    return creature, MonsterAI(states, MAGI_KNIGHT_POWER_SHIELD_MOVE)


def create_spectral_knight(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        SPECTRAL_KNIGHT_TOUGH_HP,
        SPECTRAL_KNIGHT_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=SPECTRAL_KNIGHT_MONSTER_ID)

    def hex_player(combat: CombatState) -> None:
        apply_power_to_living_player_targets(combat, PowerId.HEX, SPECTRAL_KNIGHT_HEX_AMOUNT, applier=creature)

    def soul_slash(combat: CombatState) -> None:
        soul_slash_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            SPECTRAL_KNIGHT_DEADLY_SOUL_SLASH_DAMAGE,
            SPECTRAL_KNIGHT_BASE_SOUL_SLASH_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, soul_slash_dmg)

    def soul_flame(combat: CombatState) -> None:
        soul_flame_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            SPECTRAL_KNIGHT_DEADLY_SOUL_FLAME_DAMAGE,
            SPECTRAL_KNIGHT_BASE_SOUL_FLAME_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, soul_flame_dmg, hits=SPECTRAL_KNIGHT_SOUL_FLAME_REPEAT)

    soul_slash_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        SPECTRAL_KNIGHT_DEADLY_SOUL_SLASH_DAMAGE,
        SPECTRAL_KNIGHT_BASE_SOUL_SLASH_DAMAGE,
    )
    soul_flame_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        SPECTRAL_KNIGHT_DEADLY_SOUL_FLAME_DAMAGE,
        SPECTRAL_KNIGHT_BASE_SOUL_FLAME_DAMAGE,
    )

    rand = RandomBranchState(SPECTRAL_KNIGHT_RANDOM_STATE)
    rand.add_branch(SPECTRAL_KNIGHT_SOUL_SLASH_MOVE, weight=2.0)
    rand.add_branch(SPECTRAL_KNIGHT_SOUL_FLAME_MOVE, MoveRepeatType.CANNOT_REPEAT)

    states: dict[str, MonsterState] = {
        SPECTRAL_KNIGHT_HEX_MOVE: MoveState(
            SPECTRAL_KNIGHT_HEX_MOVE,
            hex_player,
            [debuff_intent()],
            follow_up_id=SPECTRAL_KNIGHT_SOUL_SLASH_MOVE,
        ),
        SPECTRAL_KNIGHT_RANDOM_STATE: rand,
        SPECTRAL_KNIGHT_SOUL_SLASH_MOVE: MoveState(
            SPECTRAL_KNIGHT_SOUL_SLASH_MOVE,
            soul_slash,
            [attack_intent(soul_slash_intent_damage)],
            follow_up_id=SPECTRAL_KNIGHT_RANDOM_STATE,
        ),
        SPECTRAL_KNIGHT_SOUL_FLAME_MOVE: MoveState(
            SPECTRAL_KNIGHT_SOUL_FLAME_MOVE,
            soul_flame,
            [multi_attack_intent(soul_flame_intent_damage, SPECTRAL_KNIGHT_SOUL_FLAME_REPEAT)],
            follow_up_id=SPECTRAL_KNIGHT_RANDOM_STATE,
        ),
    }
    return creature, MonsterAI(states, SPECTRAL_KNIGHT_HEX_MOVE)


# ---- MechaKnight (HP 155 / 165 asc) ----

def create_mecha_knight(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        MECHA_KNIGHT_TOUGH_HP,
        MECHA_KNIGHT_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=MECHA_KNIGHT_MONSTER_ID)

    def charge(combat: CombatState) -> None:
        charge_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            MECHA_KNIGHT_DEADLY_CHARGE_DAMAGE,
            MECHA_KNIGHT_BASE_CHARGE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, charge_dmg)

    def flamethrower(combat: CombatState) -> None:
        flamethrower_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            MECHA_KNIGHT_DEADLY_FLAMETHROWER_DAMAGE,
            MECHA_KNIGHT_BASE_FLAMETHROWER_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, flamethrower_dmg)
        for target in living_player_targets(combat):
            for _ in range(MECHA_KNIGHT_FLAMETHROWER_BURNS):
                combat.add_generated_card_to_creature_hand(
                    target,
                    make_burn(),
                    added_by_player=False,
                )

    def windup(combat: CombatState) -> None:
        _gain_block(creature, MECHA_KNIGHT_WINDUP_BLOCK, combat)
        creature.apply_power(PowerId.STRENGTH, MECHA_KNIGHT_WINDUP_STRENGTH)

    def heavy_cleave(combat: CombatState) -> None:
        heavy_cleave_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            MECHA_KNIGHT_DEADLY_HEAVY_CLEAVE_DAMAGE,
            MECHA_KNIGHT_BASE_HEAVY_CLEAVE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, heavy_cleave_dmg)

    charge_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        MECHA_KNIGHT_DEADLY_CHARGE_DAMAGE,
        MECHA_KNIGHT_BASE_CHARGE_DAMAGE,
    )
    flamethrower_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        MECHA_KNIGHT_DEADLY_FLAMETHROWER_DAMAGE,
        MECHA_KNIGHT_BASE_FLAMETHROWER_DAMAGE,
    )
    heavy_cleave_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        MECHA_KNIGHT_DEADLY_HEAVY_CLEAVE_DAMAGE,
        MECHA_KNIGHT_BASE_HEAVY_CLEAVE_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        MECHA_KNIGHT_CHARGE_MOVE: MoveState(
            MECHA_KNIGHT_CHARGE_MOVE,
            charge,
            [attack_intent(charge_intent_damage)],
            follow_up_id=MECHA_KNIGHT_FLAMETHROWER_MOVE,
        ),
        MECHA_KNIGHT_FLAMETHROWER_MOVE: MoveState(
            MECHA_KNIGHT_FLAMETHROWER_MOVE,
            flamethrower,
            [attack_intent(flamethrower_intent_damage), status_intent()],
            follow_up_id=MECHA_KNIGHT_WINDUP_MOVE,
        ),
        MECHA_KNIGHT_WINDUP_MOVE: MoveState(
            MECHA_KNIGHT_WINDUP_MOVE,
            windup,
            [defend_intent(), buff_intent()],
            follow_up_id=MECHA_KNIGHT_HEAVY_CLEAVE_MOVE,
        ),
        MECHA_KNIGHT_HEAVY_CLEAVE_MOVE: MoveState(
            MECHA_KNIGHT_HEAVY_CLEAVE_MOVE,
            heavy_cleave,
            [attack_intent(heavy_cleave_intent_damage)],
            follow_up_id=MECHA_KNIGHT_FLAMETHROWER_MOVE,
        ),
    }
    creature.apply_power(PowerId.ARTIFACT, MECHA_KNIGHT_ARTIFACT)
    return creature, MonsterAI(states, MECHA_KNIGHT_CHARGE_MOVE)


# ---- SoulNexus + Osty ----

def create_osty(rng: Rng) -> tuple[Creature, MonsterAI]:
    creature = Creature(max_hp=1, monster_id="OSTY")

    def nothing(combat: CombatState) -> None:
        pass

    states: dict[str, MonsterState] = {
        "NOTHING_MOVE": MoveState("NOTHING_MOVE", nothing, [Intent(IntentType.UNKNOWN)], follow_up_id="NOTHING_MOVE"),
    }
    return creature, MonsterAI(states, "NOTHING_MOVE")


SOUL_NEXUS_MONSTER_ID = "SOUL_NEXUS"
SOUL_NEXUS_BASE_HP = 234
SOUL_NEXUS_TOUGH_HP = 254
SOUL_NEXUS_BASE_SOUL_BURN_DAMAGE = 29
SOUL_NEXUS_DEADLY_SOUL_BURN_DAMAGE = 31
SOUL_NEXUS_BASE_MAELSTROM_DAMAGE = 6
SOUL_NEXUS_DEADLY_MAELSTROM_DAMAGE = 7
SOUL_NEXUS_MAELSTROM_REPEAT = 4
SOUL_NEXUS_BASE_DRAIN_LIFE_DAMAGE = 18
SOUL_NEXUS_DEADLY_DRAIN_LIFE_DAMAGE = 19
SOUL_NEXUS_DRAIN_LIFE_DEBUFF = 2
SOUL_NEXUS_RANDOM_STATE = "RAND"
SOUL_NEXUS_SOUL_BURN_MOVE = "SOUL_BURN_MOVE"
SOUL_NEXUS_MAELSTROM_MOVE = "MAELSTROM_MOVE"
SOUL_NEXUS_DRAIN_LIFE_MOVE = "DRAIN_LIFE_MOVE"


def create_soul_nexus(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        SOUL_NEXUS_TOUGH_HP,
        SOUL_NEXUS_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=SOUL_NEXUS_MONSTER_ID)

    def soul_burn(combat: CombatState) -> None:
        soul_burn_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            SOUL_NEXUS_DEADLY_SOUL_BURN_DAMAGE,
            SOUL_NEXUS_BASE_SOUL_BURN_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, soul_burn_dmg)

    def maelstrom(combat: CombatState) -> None:
        maelstrom_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            SOUL_NEXUS_DEADLY_MAELSTROM_DAMAGE,
            SOUL_NEXUS_BASE_MAELSTROM_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, maelstrom_dmg, hits=SOUL_NEXUS_MAELSTROM_REPEAT)

    def drain_life(combat: CombatState) -> None:
        drain_life_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            SOUL_NEXUS_DEADLY_DRAIN_LIFE_DAMAGE,
            SOUL_NEXUS_BASE_DRAIN_LIFE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, drain_life_dmg)
        apply_power_to_living_player_targets(combat, PowerId.VULNERABLE, SOUL_NEXUS_DRAIN_LIFE_DEBUFF, applier=creature)
        apply_power_to_living_player_targets(combat, PowerId.WEAK, SOUL_NEXUS_DRAIN_LIFE_DEBUFF, applier=creature)

    soul_burn_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        SOUL_NEXUS_DEADLY_SOUL_BURN_DAMAGE,
        SOUL_NEXUS_BASE_SOUL_BURN_DAMAGE,
    )
    maelstrom_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        SOUL_NEXUS_DEADLY_MAELSTROM_DAMAGE,
        SOUL_NEXUS_BASE_MAELSTROM_DAMAGE,
    )
    drain_life_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        SOUL_NEXUS_DEADLY_DRAIN_LIFE_DAMAGE,
        SOUL_NEXUS_BASE_DRAIN_LIFE_DAMAGE,
    )

    rand = RandomBranchState(SOUL_NEXUS_RANDOM_STATE)
    rand.add_branch(SOUL_NEXUS_SOUL_BURN_MOVE, MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch(SOUL_NEXUS_MAELSTROM_MOVE, MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch(SOUL_NEXUS_DRAIN_LIFE_MOVE, MoveRepeatType.CANNOT_REPEAT)

    states: dict[str, MonsterState] = {
        SOUL_NEXUS_RANDOM_STATE: rand,
        SOUL_NEXUS_SOUL_BURN_MOVE: MoveState(
            SOUL_NEXUS_SOUL_BURN_MOVE,
            soul_burn,
            [attack_intent(soul_burn_intent_damage)],
            follow_up_id=SOUL_NEXUS_RANDOM_STATE,
        ),
        SOUL_NEXUS_MAELSTROM_MOVE: MoveState(
            SOUL_NEXUS_MAELSTROM_MOVE,
            maelstrom,
            [multi_attack_intent(maelstrom_intent_damage, SOUL_NEXUS_MAELSTROM_REPEAT)],
            follow_up_id=SOUL_NEXUS_RANDOM_STATE,
        ),
        SOUL_NEXUS_DRAIN_LIFE_MOVE: MoveState(
            SOUL_NEXUS_DRAIN_LIFE_MOVE,
            drain_life,
            [attack_intent(drain_life_intent_damage), strong_debuff_intent()],
            follow_up_id=SOUL_NEXUS_RANDOM_STATE,
        ),
    }
    return creature, MonsterAI(states, SOUL_NEXUS_SOUL_BURN_MOVE)


# ========================================================================
# BOSS ENCOUNTERS
# ========================================================================

# ---- Aeonglass (Act 3 boss; replaced Door + Doormaker as of v0.111.0) ----

AEONGLASS_MONSTER_ID = "AEONGLASS"
AEONGLASS_BASE_HP = 512
AEONGLASS_TOUGH_HP = 535
AEONGLASS_BASE_EBB_DAMAGE = 22
AEONGLASS_DEADLY_EBB_DAMAGE = 26
AEONGLASS_EBB_BLOCK = 33
AEONGLASS_BASE_EYE_LASERS_DAMAGE = 11
AEONGLASS_DEADLY_EYE_LASERS_DAMAGE = 12
AEONGLASS_EYE_LASERS_REPEAT = 2
AEONGLASS_BASE_INTENSITY_STRENGTH = 3
AEONGLASS_DEADLY_INTENSITY_STRENGTH = 4
AEONGLASS_BASE_WITHER_AMOUNT = 1
AEONGLASS_DEADLY_WITHER_AMOUNT = 2
AEONGLASS_WITHERING_PRESENCE_CARDS_LEFT = 6
AEONGLASS_EBB_MOVE = "EBB_MOVE"
AEONGLASS_EYE_LASERS_MOVE = "EYE_LASERS_MOVE"
AEONGLASS_INCREASING_INTENSITY_MOVE = "INCREASING_INTENSITY_MOVE"


def _fake_upgrade_all_withers(combat: CombatState) -> None:
    """Fake-upgrade every Wither in the players' piles (+3 damage each)."""
    from sts2_env.cards.status import WITHER_DAMAGE_PER_FAKE_UPGRADE

    for player_state in combat.combat_player_states:
        for pile in player_state.all_piles:
            for card in pile:
                if card.card_id == CardId.WITHER:
                    card.base_damage = (card.base_damage or 0) + WITHER_DAMAGE_PER_FAKE_UPGRADE


def create_aeonglass(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        AEONGLASS_TOUGH_HP,
        AEONGLASS_BASE_HP,
    )
    # C#: MaxInitialHp == MinInitialHp -- a fixed HP pool, no roll.
    creature = Creature(max_hp=hp, monster_id=AEONGLASS_MONSTER_ID)

    def ebb(combat: CombatState) -> None:
        ebb_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            AEONGLASS_DEADLY_EBB_DAMAGE,
            AEONGLASS_BASE_EBB_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, ebb_dmg)
        _gain_block(creature, AEONGLASS_EBB_BLOCK, combat)

    def eye_lasers(combat: CombatState) -> None:
        laser_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            AEONGLASS_DEADLY_EYE_LASERS_DAMAGE,
            AEONGLASS_BASE_EYE_LASERS_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, laser_dmg, hits=AEONGLASS_EYE_LASERS_REPEAT)

    def increasing_intensity(combat: CombatState) -> None:
        _fake_upgrade_all_withers(combat)
        presence = creature.powers.get(PowerId.WITHERING_PRESENCE)
        level = presence.wither_upgrade_level + 1 if presence is not None else 1
        if presence is not None:
            presence.wither_upgrade_level = level
        wither_amount = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            AEONGLASS_DEADLY_WITHER_AMOUNT,
            AEONGLASS_BASE_WITHER_AMOUNT,
        )
        add_generated_cards_to_living_player_discards(
            combat,
            lambda: make_wither(level),
            wither_amount,
        )
        strength = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            AEONGLASS_DEADLY_INTENSITY_STRENGTH,
            AEONGLASS_BASE_INTENSITY_STRENGTH,
        )
        if presence is not None:
            strength += presence.additional_strength
            presence.additional_strength += 1
        combat.apply_power_to(creature, PowerId.STRENGTH, strength, applier=creature)

    ebb_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        AEONGLASS_DEADLY_EBB_DAMAGE,
        AEONGLASS_BASE_EBB_DAMAGE,
    )
    eye_lasers_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        AEONGLASS_DEADLY_EYE_LASERS_DAMAGE,
        AEONGLASS_BASE_EYE_LASERS_DAMAGE,
    )
    wither_intent_amount = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        AEONGLASS_DEADLY_WITHER_AMOUNT,
        AEONGLASS_BASE_WITHER_AMOUNT,
    )

    states: dict[str, MonsterState] = {
        AEONGLASS_EBB_MOVE: MoveState(
            AEONGLASS_EBB_MOVE,
            ebb,
            [attack_intent(ebb_intent_damage), defend_intent()],
            follow_up_id=AEONGLASS_EYE_LASERS_MOVE,
        ),
        AEONGLASS_EYE_LASERS_MOVE: MoveState(
            AEONGLASS_EYE_LASERS_MOVE,
            eye_lasers,
            [multi_attack_intent(eye_lasers_intent_damage, AEONGLASS_EYE_LASERS_REPEAT)],
            follow_up_id=AEONGLASS_INCREASING_INTENSITY_MOVE,
        ),
        AEONGLASS_INCREASING_INTENSITY_MOVE: MoveState(
            AEONGLASS_INCREASING_INTENSITY_MOVE,
            increasing_intensity,
            [status_intent(), buff_intent()],
            follow_up_id=AEONGLASS_EBB_MOVE,
        ),
    }
    creature.apply_power(PowerId.ARTIFACT, 3)
    creature.apply_power(PowerId.WITHERING_PRESENCE, AEONGLASS_WITHERING_PRESENCE_CARDS_LEFT)
    return creature, MonsterAI(states, AEONGLASS_EBB_MOVE)


# ---- Queen ----

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


QUEEN_MONSTER_ID = "QUEEN"
QUEEN_BASE_HP = 400
QUEEN_TOUGH_HP = 419
QUEEN_BASE_OFF_WITH_YOUR_HEAD_DAMAGE = 3
QUEEN_DEADLY_OFF_WITH_YOUR_HEAD_DAMAGE = 4
QUEEN_OFF_WITH_YOUR_HEAD_REPEAT = 5
QUEEN_BASE_EXECUTION_DAMAGE = 15
QUEEN_DEADLY_EXECUTION_DAMAGE = 18
QUEEN_CHAINS_OF_BINDING = 3
QUEEN_YOURE_MINE_DEBUFF = 99
QUEEN_BURN_BRIGHT_STRENGTH = 1
QUEEN_BURN_BRIGHT_BLOCK = 20
QUEEN_ENRAGE_STRENGTH = 2
QUEEN_PUPPET_STRINGS_MOVE = "PUPPET_STRINGS_MOVE"
QUEEN_YOU_ARE_MINE_MOVE = "YOU_ARE_MINE_MOVE"
QUEEN_YOURE_MINE_NOW_BRANCH = "YOURE_MINE_NOW_BRANCH"
QUEEN_BURN_BRIGHT_FOR_ME_MOVE = "BURN_BRIGHT_FOR_ME_MOVE"
QUEEN_BURN_BRIGHT_FOR_ME_BRANCH = "BURN_BRIGHT_FOR_ME_BRANCH"
QUEEN_OFF_WITH_YOUR_HEAD_MOVE = "OFF_WITH_YOUR_HEAD_MOVE"
QUEEN_EXECUTION_MOVE = "EXECUTION_MOVE"
QUEEN_ENRAGE_MOVE = "ENRAGE_MOVE"


def create_queen(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        QUEEN_TOUGH_HP,
        QUEEN_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=QUEEN_MONSTER_ID)

    def _has_amalgam_alive() -> bool:
        combat = creature.combat_state
        if combat is None:
            return True
        return any(enemy.monster_id == TORCH_HEAD_AMALGAM_MONSTER_ID and enemy.is_alive for enemy in combat.enemies)

    def puppet_strings(combat: CombatState) -> None:
        apply_power_to_living_player_targets(combat, PowerId.CHAINS_OF_BINDING, QUEEN_CHAINS_OF_BINDING, applier=creature)

    def youre_mine(combat: CombatState) -> None:
        apply_power_to_living_player_targets(combat, PowerId.FRAIL, QUEEN_YOURE_MINE_DEBUFF, applier=creature)
        apply_power_to_living_player_targets(combat, PowerId.WEAK, QUEEN_YOURE_MINE_DEBUFF, applier=creature)
        apply_power_to_living_player_targets(combat, PowerId.VULNERABLE, QUEEN_YOURE_MINE_DEBUFF, applier=creature)

    def burn_bright_for_me(combat: CombatState) -> None:
        for enemy in combat.alive_enemies:
            if enemy is not creature and enemy.side == creature.side:
                enemy.apply_power(PowerId.STRENGTH, QUEEN_BURN_BRIGHT_STRENGTH, applier=creature)
        _gain_block(creature, QUEEN_BURN_BRIGHT_BLOCK, combat)

    def off_with_your_head(combat: CombatState) -> None:
        off_with_your_head_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            QUEEN_DEADLY_OFF_WITH_YOUR_HEAD_DAMAGE,
            QUEEN_BASE_OFF_WITH_YOUR_HEAD_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, off_with_your_head_dmg, hits=QUEEN_OFF_WITH_YOUR_HEAD_REPEAT)

    def execution(combat: CombatState) -> None:
        execution_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            QUEEN_DEADLY_EXECUTION_DAMAGE,
            QUEEN_BASE_EXECUTION_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, execution_dmg)

    def enrage(combat: CombatState) -> None:
        creature.apply_power(PowerId.STRENGTH, QUEEN_ENRAGE_STRENGTH, applier=creature)

    off_with_your_head_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        QUEEN_DEADLY_OFF_WITH_YOUR_HEAD_DAMAGE,
        QUEEN_BASE_OFF_WITH_YOUR_HEAD_DAMAGE,
    )
    execution_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        QUEEN_DEADLY_EXECUTION_DAMAGE,
        QUEEN_BASE_EXECUTION_DAMAGE,
    )

    youre_mine_now_branch = ConditionalBranchState(QUEEN_YOURE_MINE_NOW_BRANCH)
    youre_mine_now_branch.add_branch(_has_amalgam_alive, QUEEN_BURN_BRIGHT_FOR_ME_MOVE)
    youre_mine_now_branch.add_branch(lambda: True, QUEEN_OFF_WITH_YOUR_HEAD_MOVE)

    burn_bright_branch = ConditionalBranchState(QUEEN_BURN_BRIGHT_FOR_ME_BRANCH)
    burn_bright_branch.add_branch(_has_amalgam_alive, QUEEN_BURN_BRIGHT_FOR_ME_MOVE)
    burn_bright_branch.add_branch(lambda: True, QUEEN_OFF_WITH_YOUR_HEAD_MOVE)

    states: dict[str, MonsterState] = {
        QUEEN_PUPPET_STRINGS_MOVE: MoveState(
            QUEEN_PUPPET_STRINGS_MOVE,
            puppet_strings,
            [strong_debuff_intent()],
            follow_up_id=QUEEN_YOU_ARE_MINE_MOVE,
        ),
        QUEEN_YOU_ARE_MINE_MOVE: MoveState(
            QUEEN_YOU_ARE_MINE_MOVE,
            youre_mine,
            [debuff_intent()],
            follow_up_id=QUEEN_YOURE_MINE_NOW_BRANCH,
        ),
        QUEEN_YOURE_MINE_NOW_BRANCH: youre_mine_now_branch,
        QUEEN_BURN_BRIGHT_FOR_ME_MOVE: MoveState(
            QUEEN_BURN_BRIGHT_FOR_ME_MOVE,
            burn_bright_for_me,
            [buff_intent(), defend_intent()],
            follow_up_id=QUEEN_BURN_BRIGHT_FOR_ME_BRANCH,
        ),
        QUEEN_BURN_BRIGHT_FOR_ME_BRANCH: burn_bright_branch,
        QUEEN_OFF_WITH_YOUR_HEAD_MOVE: MoveState(
            QUEEN_OFF_WITH_YOUR_HEAD_MOVE,
            off_with_your_head,
            [multi_attack_intent(off_with_your_head_intent_damage, QUEEN_OFF_WITH_YOUR_HEAD_REPEAT)],
            follow_up_id=QUEEN_EXECUTION_MOVE,
        ),
        QUEEN_EXECUTION_MOVE: MoveState(
            QUEEN_EXECUTION_MOVE,
            execution,
            [attack_intent(execution_intent_damage)],
            follow_up_id=QUEEN_ENRAGE_MOVE,
        ),
        QUEEN_ENRAGE_MOVE: MoveState(
            QUEEN_ENRAGE_MOVE,
            enrage,
            [buff_intent()],
            follow_up_id=QUEEN_OFF_WITH_YOUR_HEAD_MOVE,
        ),
    }
    return creature, MonsterAI(states, QUEEN_PUPPET_STRINGS_MOVE)


# ---- TestSubject ----

TEST_SUBJECT_MONSTER_ID = "TEST_SUBJECT"
TEST_SUBJECT_BASE_FIRST_FORM_HP = 100
TEST_SUBJECT_TOUGH_FIRST_FORM_HP = 111
TEST_SUBJECT_BASE_SECOND_FORM_HP = 200
TEST_SUBJECT_TOUGH_SECOND_FORM_HP = 212
TEST_SUBJECT_BASE_THIRD_FORM_HP = 300
TEST_SUBJECT_TOUGH_THIRD_FORM_HP = 313
TEST_SUBJECT_BASE_ENRAGE = 2
TEST_SUBJECT_DEADLY_ENRAGE = 3
TEST_SUBJECT_BASE_BITE_DAMAGE = 20
TEST_SUBJECT_DEADLY_BITE_DAMAGE = 22
TEST_SUBJECT_BASE_SKULL_BASH_DAMAGE = 14
TEST_SUBJECT_DEADLY_SKULL_BASH_DAMAGE = 16
TEST_SUBJECT_SKULL_BASH_VULNERABLE = 1
TEST_SUBJECT_BASE_MULTI_CLAW_DAMAGE = 10
TEST_SUBJECT_DEADLY_MULTI_CLAW_DAMAGE = 11
TEST_SUBJECT_BASE_MULTI_CLAW_COUNT = 3
TEST_SUBJECT_PHASE3_LACERATE_REPEAT = 3
TEST_SUBJECT_BIG_POUNCE_DAMAGE = 45
TEST_SUBJECT_BASE_BURNING_GROWL_BURNS = 3
TEST_SUBJECT_DEADLY_BURNING_GROWL_BURNS = 5
TEST_SUBJECT_BASE_BURNING_GROWL_STRENGTH = 2
TEST_SUBJECT_DEADLY_BURNING_GROWL_STRENGTH = 3
TEST_SUBJECT_BURNING_GROWL_STATUS = "BURN"
TEST_SUBJECT_ADAPTABLE = 1
TEST_SUBJECT_PAINFUL_STABS = 1
TEST_SUBJECT_NEMESIS = 1
TEST_SUBJECT_RESPAWNS_KEY = "respawns"
TEST_SUBJECT_EXTRA_MULTI_CLAW_COUNT_KEY = "extra_multi_claw_count"
TEST_SUBJECT_RESPAWN_MOVE = "RESPAWN_MOVE"
TEST_SUBJECT_REVIVE_BRANCH = "REVIVE_BRANCH"
TEST_SUBJECT_BITE_MOVE = "BITE_MOVE"
TEST_SUBJECT_SKULL_BASH_MOVE = "SKULL_BASH_MOVE"
TEST_SUBJECT_MULTI_CLAW_MOVE = "MULTI_CLAW_MOVE"
TEST_SUBJECT_PHASE3_LACERATE_MOVE = "PHASE3_LACERATE_MOVE"
TEST_SUBJECT_BIG_POUNCE_MOVE = "BIG_POUNCE"
TEST_SUBJECT_BURNING_GROWL_MOVE = "BURNING_GROWL_MOVE"


def create_test_subject(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        TEST_SUBJECT_TOUGH_FIRST_FORM_HP,
        TEST_SUBJECT_BASE_FIRST_FORM_HP,
    )
    creature = Creature(max_hp=hp, monster_id=TEST_SUBJECT_MONSTER_ID)

    _state = {
        TEST_SUBJECT_RESPAWNS_KEY: 0,
        TEST_SUBJECT_EXTRA_MULTI_CLAW_COUNT_KEY: 0,
    }

    def _multi_claw_total_count() -> int:
        return TEST_SUBJECT_BASE_MULTI_CLAW_COUNT + _state[TEST_SUBJECT_EXTRA_MULTI_CLAW_COUNT_KEY]

    def respawn(combat: CombatState) -> None:
        _state[TEST_SUBJECT_RESPAWNS_KEY] += 1
        adaptable = creature.powers.get(PowerId.ADAPTABLE)
        do_revive = getattr(adaptable, "do_revive", None)
        if callable(do_revive):
            do_revive()
        second_form_hp = _ascension_value(
            _combat_ascension_level(combat),
            TOUGH_ENEMIES_ASCENSION_LEVEL,
            TEST_SUBJECT_TOUGH_SECOND_FORM_HP,
            TEST_SUBJECT_BASE_SECOND_FORM_HP,
        )
        third_form_hp = _ascension_value(
            _combat_ascension_level(combat),
            TOUGH_ENEMIES_ASCENSION_LEVEL,
            TEST_SUBJECT_TOUGH_THIRD_FORM_HP,
            TEST_SUBJECT_BASE_THIRD_FORM_HP,
        )
        scaled_hp = (
            second_form_hp
            if _state[TEST_SUBJECT_RESPAWNS_KEY] == 1
            else third_form_hp
        ) * len(combat.combat_player_states)
        creature.max_hp = scaled_hp
        creature.current_hp = scaled_hp
        if _state[TEST_SUBJECT_RESPAWNS_KEY] == 1:
            creature.apply_power(PowerId.PAINFUL_STABS, TEST_SUBJECT_PAINFUL_STABS)
        else:
            creature.apply_power(PowerId.NEMESIS, TEST_SUBJECT_NEMESIS)
            creature.powers.pop(PowerId.ADAPTABLE, None)
            creature.powers.pop(PowerId.PAINFUL_STABS, None)

    def bite(combat: CombatState) -> None:
        bite_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            TEST_SUBJECT_DEADLY_BITE_DAMAGE,
            TEST_SUBJECT_BASE_BITE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, bite_dmg)

    def skull_bash(combat: CombatState) -> None:
        skull_bash_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            TEST_SUBJECT_DEADLY_SKULL_BASH_DAMAGE,
            TEST_SUBJECT_BASE_SKULL_BASH_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, skull_bash_dmg)
        apply_power_to_living_player_targets(
            combat,
            PowerId.VULNERABLE,
            TEST_SUBJECT_SKULL_BASH_VULNERABLE,
            applier=creature,
        )

    def multi_claw(combat: CombatState) -> None:
        multi_claw_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            TEST_SUBJECT_DEADLY_MULTI_CLAW_DAMAGE,
            TEST_SUBJECT_BASE_MULTI_CLAW_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, multi_claw_dmg, hits=_multi_claw_total_count())
        _state[TEST_SUBJECT_EXTRA_MULTI_CLAW_COUNT_KEY] += 1
        states[TEST_SUBJECT_MULTI_CLAW_MOVE].intents = [multi_attack_intent(multi_claw_dmg, _multi_claw_total_count())]

    def phase3_lacerate(combat: CombatState) -> None:
        phase3_lacerate_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            TEST_SUBJECT_DEADLY_MULTI_CLAW_DAMAGE,
            TEST_SUBJECT_BASE_MULTI_CLAW_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, phase3_lacerate_dmg, hits=TEST_SUBJECT_PHASE3_LACERATE_REPEAT)

    def big_pounce(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, TEST_SUBJECT_BIG_POUNCE_DAMAGE)

    def burning_growl(combat: CombatState) -> None:
        burning_growl_burns = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            TEST_SUBJECT_DEADLY_BURNING_GROWL_BURNS,
            TEST_SUBJECT_BASE_BURNING_GROWL_BURNS,
        )
        burning_growl_strength = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            TEST_SUBJECT_DEADLY_BURNING_GROWL_STRENGTH,
            TEST_SUBJECT_BASE_BURNING_GROWL_STRENGTH,
        )
        for target in living_player_targets(combat):
            combat.add_status_cards_to_discard(target, TEST_SUBJECT_BURNING_GROWL_STATUS, burning_growl_burns)
        creature.apply_power(PowerId.STRENGTH, burning_growl_strength)

    bite_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        TEST_SUBJECT_DEADLY_BITE_DAMAGE,
        TEST_SUBJECT_BASE_BITE_DAMAGE,
    )
    skull_bash_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        TEST_SUBJECT_DEADLY_SKULL_BASH_DAMAGE,
        TEST_SUBJECT_BASE_SKULL_BASH_DAMAGE,
    )
    multi_claw_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        TEST_SUBJECT_DEADLY_MULTI_CLAW_DAMAGE,
        TEST_SUBJECT_BASE_MULTI_CLAW_DAMAGE,
    )
    burning_growl_intent_burns = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        TEST_SUBJECT_DEADLY_BURNING_GROWL_BURNS,
        TEST_SUBJECT_BASE_BURNING_GROWL_BURNS,
    )
    enrage_amount = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        TEST_SUBJECT_DEADLY_ENRAGE,
        TEST_SUBJECT_BASE_ENRAGE,
    )

    revive_branch = ConditionalBranchState(TEST_SUBJECT_REVIVE_BRANCH)
    revive_branch.add_branch(lambda: _state[TEST_SUBJECT_RESPAWNS_KEY] < 2, TEST_SUBJECT_MULTI_CLAW_MOVE)
    revive_branch.add_branch(lambda: True, TEST_SUBJECT_PHASE3_LACERATE_MOVE)

    states: dict[str, MonsterState] = {
        TEST_SUBJECT_RESPAWN_MOVE: MoveState(
            TEST_SUBJECT_RESPAWN_MOVE,
            respawn,
            [Intent(IntentType.HEAL), buff_intent()],
            follow_up_id=TEST_SUBJECT_REVIVE_BRANCH,
            must_perform_once=True,
        ),
        TEST_SUBJECT_REVIVE_BRANCH: revive_branch,
        TEST_SUBJECT_BITE_MOVE: MoveState(
            TEST_SUBJECT_BITE_MOVE,
            bite,
            [attack_intent(bite_intent_damage)],
            follow_up_id=TEST_SUBJECT_SKULL_BASH_MOVE,
        ),
        TEST_SUBJECT_SKULL_BASH_MOVE: MoveState(
            TEST_SUBJECT_SKULL_BASH_MOVE,
            skull_bash,
            [attack_intent(skull_bash_intent_damage), debuff_intent()],
            follow_up_id=TEST_SUBJECT_BITE_MOVE,
        ),
        TEST_SUBJECT_MULTI_CLAW_MOVE: MoveState(
            TEST_SUBJECT_MULTI_CLAW_MOVE,
            multi_claw,
            [multi_attack_intent(multi_claw_intent_damage, _multi_claw_total_count())],
            follow_up_id=TEST_SUBJECT_MULTI_CLAW_MOVE,
        ),
        TEST_SUBJECT_PHASE3_LACERATE_MOVE: MoveState(
            TEST_SUBJECT_PHASE3_LACERATE_MOVE,
            phase3_lacerate,
            [multi_attack_intent(multi_claw_intent_damage, TEST_SUBJECT_PHASE3_LACERATE_REPEAT)],
            follow_up_id=TEST_SUBJECT_BIG_POUNCE_MOVE,
        ),
        TEST_SUBJECT_BIG_POUNCE_MOVE: MoveState(
            TEST_SUBJECT_BIG_POUNCE_MOVE,
            big_pounce,
            [attack_intent(TEST_SUBJECT_BIG_POUNCE_DAMAGE)],
            follow_up_id=TEST_SUBJECT_BURNING_GROWL_MOVE,
        ),
        TEST_SUBJECT_BURNING_GROWL_MOVE: MoveState(
            TEST_SUBJECT_BURNING_GROWL_MOVE,
            burning_growl,
            [status_intent(), buff_intent()],
            follow_up_id=TEST_SUBJECT_PHASE3_LACERATE_MOVE,
        ),
    }
    states[TEST_SUBJECT_BURNING_GROWL_MOVE].intents[0].hits = burning_growl_intent_burns
    creature.apply_power(PowerId.ADAPTABLE, TEST_SUBJECT_ADAPTABLE)
    creature.apply_power(PowerId.ENRAGE, enrage_amount)
    return creature, MonsterAI(states, TEST_SUBJECT_BITE_MOVE)
