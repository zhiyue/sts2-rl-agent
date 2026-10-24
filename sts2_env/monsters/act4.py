"""Act 4 (Underdocks) monsters: weak, normal, elite, boss.

All HP ranges, damage values, and state machines verified against decompiled C# source.
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from sts2_env.core.creature import Creature
from sts2_env.core.enums import CardId, CombatSide, MoveRepeatType, PowerId, ValueProp
from sts2_env.core.damage import calculate_damage, apply_damage
from sts2_env.core.rng import Rng
from sts2_env.cards.status import (
    WITHER_DAMAGE,
    make_dazed,
    make_wither,
    make_wound,
)
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

if TYPE_CHECKING:
    from sts2_env.core.combat import CombatState


# ---- Helpers ----

TOUGH_ENEMIES_ASCENSION_LEVEL = 8
DEADLY_ENEMIES_ASCENSION_LEVEL = 9


def _ascension_value(ascension_level: int, threshold: int, ascension_value: int, base_value: int) -> int:
    return ascension_value if ascension_level >= threshold else base_value


def _combat_ascension_level(combat: CombatState) -> int:
    return combat.ascension_level


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


# ========================================================================
# WEAK ENCOUNTERS
# ========================================================================

# ---- CorpseSlug (HP 25-27 / 27-29 asc) ----

CORPSE_SLUG_MONSTER_ID = "CORPSE_SLUG"
CORPSE_SLUG_BASE_MIN_HP = 25
CORPSE_SLUG_BASE_MAX_HP = 27
CORPSE_SLUG_TOUGH_MIN_HP = 27
CORPSE_SLUG_TOUGH_MAX_HP = 29
CORPSE_SLUG_WHIP_SLAP_DAMAGE = 3
CORPSE_SLUG_WHIP_SLAP_REPEAT = 2
CORPSE_SLUG_BASE_GLOMP_DAMAGE = 8
CORPSE_SLUG_DEADLY_GLOMP_DAMAGE = 9
CORPSE_SLUG_GOOP_FRAIL = 2
CORPSE_SLUG_BASE_RAVENOUS_STRENGTH = 4
CORPSE_SLUG_DEADLY_RAVENOUS_STRENGTH = 5
CORPSE_SLUG_MOVE_COUNT = 3
CORPSE_SLUG_WHIP_SLAP_MOVE = "WHIP_SLAP_MOVE"
CORPSE_SLUG_GLOMP_MOVE = "GLOMP_MOVE"
CORPSE_SLUG_GOOP_MOVE = "GOOP_MOVE"


def create_corpse_slug(
    rng: Rng,
    starter_idx: int = 0,
    ascension_level: int = 0,
) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        CORPSE_SLUG_TOUGH_MIN_HP,
        CORPSE_SLUG_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        CORPSE_SLUG_TOUGH_MAX_HP,
        CORPSE_SLUG_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id=CORPSE_SLUG_MONSTER_ID)

    def whip_slap(combat: CombatState) -> None:
        _deal_damage_to_player(
            combat,
            creature,
            CORPSE_SLUG_WHIP_SLAP_DAMAGE,
            hits=CORPSE_SLUG_WHIP_SLAP_REPEAT,
        )

    def glomp(combat: CombatState) -> None:
        glomp_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            CORPSE_SLUG_DEADLY_GLOMP_DAMAGE,
            CORPSE_SLUG_BASE_GLOMP_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, glomp_dmg)

    def goop(combat: CombatState) -> None:
        apply_power_to_living_player_targets(combat, PowerId.FRAIL, CORPSE_SLUG_GOOP_FRAIL, applier=creature)

    glomp_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        CORPSE_SLUG_DEADLY_GLOMP_DAMAGE,
        CORPSE_SLUG_BASE_GLOMP_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        CORPSE_SLUG_WHIP_SLAP_MOVE: MoveState(
            CORPSE_SLUG_WHIP_SLAP_MOVE,
            whip_slap,
            [multi_attack_intent(CORPSE_SLUG_WHIP_SLAP_DAMAGE, CORPSE_SLUG_WHIP_SLAP_REPEAT)],
            follow_up_id=CORPSE_SLUG_GLOMP_MOVE,
        ),
        CORPSE_SLUG_GLOMP_MOVE: MoveState(
            CORPSE_SLUG_GLOMP_MOVE,
            glomp,
            [attack_intent(glomp_intent_damage)],
            follow_up_id=CORPSE_SLUG_GOOP_MOVE,
        ),
        CORPSE_SLUG_GOOP_MOVE: MoveState(
            CORPSE_SLUG_GOOP_MOVE,
            goop,
            [debuff_intent()],
            follow_up_id=CORPSE_SLUG_WHIP_SLAP_MOVE,
        ),
    }

    starter_remainder = starter_idx - int(starter_idx / CORPSE_SLUG_MOVE_COUNT) * CORPSE_SLUG_MOVE_COUNT
    starter_map = {0: CORPSE_SLUG_WHIP_SLAP_MOVE, 1: CORPSE_SLUG_GLOMP_MOVE}
    initial = starter_map.get(starter_remainder, CORPSE_SLUG_GOOP_MOVE)

    ravenous_strength = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        CORPSE_SLUG_DEADLY_RAVENOUS_STRENGTH,
        CORPSE_SLUG_BASE_RAVENOUS_STRENGTH,
    )
    creature.apply_power(PowerId.RAVENOUS, ravenous_strength)
    return creature, MonsterAI(states, initial, rng)


# ---- Seapunk (HP 44-46 / 47-49 asc) ----

SEAPUNK_MONSTER_ID = "SEAPUNK"
SEAPUNK_BASE_MIN_HP = 44
SEAPUNK_BASE_MAX_HP = 46
SEAPUNK_TOUGH_MIN_HP = 47
SEAPUNK_TOUGH_MAX_HP = 49
SEAPUNK_BASE_SEA_KICK_DAMAGE = 11
SEAPUNK_DEADLY_SEA_KICK_DAMAGE = 13
SEAPUNK_SPINNING_KICK_DAMAGE = 2
SEAPUNK_SPINNING_KICK_REPEAT = 4
SEAPUNK_BASE_BUBBLE_BLOCK = 7
SEAPUNK_TOUGH_BUBBLE_BLOCK = 8
SEAPUNK_BASE_BUBBLE_STRENGTH = 1
SEAPUNK_DEADLY_BUBBLE_STRENGTH = 2
SEAPUNK_SEA_KICK_MOVE = "SEA_KICK_MOVE"
SEAPUNK_SPINNING_KICK_MOVE = "SPINNING_KICK_MOVE"
SEAPUNK_BUBBLE_BURP_MOVE = "BUBBLE_BURP_MOVE"


def create_seapunk(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        SEAPUNK_TOUGH_MIN_HP,
        SEAPUNK_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        SEAPUNK_TOUGH_MAX_HP,
        SEAPUNK_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id=SEAPUNK_MONSTER_ID)

    def sea_kick(combat: CombatState) -> None:
        sea_kick_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            SEAPUNK_DEADLY_SEA_KICK_DAMAGE,
            SEAPUNK_BASE_SEA_KICK_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, sea_kick_dmg)

    def spinning_kick(combat: CombatState) -> None:
        _deal_damage_to_player(
            combat,
            creature,
            SEAPUNK_SPINNING_KICK_DAMAGE,
            hits=SEAPUNK_SPINNING_KICK_REPEAT,
        )

    def bubble_burp(combat: CombatState) -> None:
        bubble_block = _ascension_value(
            _combat_ascension_level(combat),
            TOUGH_ENEMIES_ASCENSION_LEVEL,
            SEAPUNK_TOUGH_BUBBLE_BLOCK,
            SEAPUNK_BASE_BUBBLE_BLOCK,
        )
        bubble_strength = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            SEAPUNK_DEADLY_BUBBLE_STRENGTH,
            SEAPUNK_BASE_BUBBLE_STRENGTH,
        )
        _gain_block(creature, bubble_block, combat)
        creature.apply_power(PowerId.STRENGTH, bubble_strength, applier=creature)

    sea_kick_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        SEAPUNK_DEADLY_SEA_KICK_DAMAGE,
        SEAPUNK_BASE_SEA_KICK_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        SEAPUNK_SEA_KICK_MOVE: MoveState(
            SEAPUNK_SEA_KICK_MOVE,
            sea_kick,
            [attack_intent(sea_kick_intent_damage)],
            follow_up_id=SEAPUNK_SPINNING_KICK_MOVE,
        ),
        SEAPUNK_SPINNING_KICK_MOVE: MoveState(
            SEAPUNK_SPINNING_KICK_MOVE,
            spinning_kick,
            [multi_attack_intent(SEAPUNK_SPINNING_KICK_DAMAGE, SEAPUNK_SPINNING_KICK_REPEAT)],
            follow_up_id=SEAPUNK_BUBBLE_BURP_MOVE,
        ),
        SEAPUNK_BUBBLE_BURP_MOVE: MoveState(
            SEAPUNK_BUBBLE_BURP_MOVE,
            bubble_burp,
            [buff_intent(), defend_intent()],
            follow_up_id=SEAPUNK_SEA_KICK_MOVE,
        ),
    }
    return creature, MonsterAI(states, SEAPUNK_SEA_KICK_MOVE)


# ---- SludgeSpinner (HP 37-39 / 41-42 asc) ----

SLUDGE_SPINNER_MONSTER_ID = "SLUDGE_SPINNER"
SLUDGE_SPINNER_BASE_MIN_HP = 37
SLUDGE_SPINNER_BASE_MAX_HP = 39
SLUDGE_SPINNER_TOUGH_MIN_HP = 41
SLUDGE_SPINNER_TOUGH_MAX_HP = 42
SLUDGE_SPINNER_BASE_OIL_SPRAY_DAMAGE = 8
SLUDGE_SPINNER_DEADLY_OIL_SPRAY_DAMAGE = 9
SLUDGE_SPINNER_OIL_SPRAY_WEAK = 1
SLUDGE_SPINNER_BASE_SLAM_DAMAGE = 11
SLUDGE_SPINNER_DEADLY_SLAM_DAMAGE = 12
SLUDGE_SPINNER_BASE_RAGE_DAMAGE = 6
SLUDGE_SPINNER_DEADLY_RAGE_DAMAGE = 7
SLUDGE_SPINNER_RAGE_STRENGTH = 3
SLUDGE_SPINNER_RANDOM_STATE = "RAND"
SLUDGE_SPINNER_OIL_SPRAY_MOVE = "OIL_SPRAY_MOVE"
SLUDGE_SPINNER_SLAM_MOVE = "SLAM_MOVE"
SLUDGE_SPINNER_RAGE_MOVE = "RAGE_MOVE"


def create_sludge_spinner(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        SLUDGE_SPINNER_TOUGH_MIN_HP,
        SLUDGE_SPINNER_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        SLUDGE_SPINNER_TOUGH_MAX_HP,
        SLUDGE_SPINNER_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id=SLUDGE_SPINNER_MONSTER_ID)

    def oil_spray(combat: CombatState) -> None:
        oil_spray_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            SLUDGE_SPINNER_DEADLY_OIL_SPRAY_DAMAGE,
            SLUDGE_SPINNER_BASE_OIL_SPRAY_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, oil_spray_dmg)
        apply_power_to_living_player_targets(
            combat,
            PowerId.WEAK,
            SLUDGE_SPINNER_OIL_SPRAY_WEAK,
            applier=creature,
        )

    def slam(combat: CombatState) -> None:
        slam_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            SLUDGE_SPINNER_DEADLY_SLAM_DAMAGE,
            SLUDGE_SPINNER_BASE_SLAM_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, slam_dmg)

    def rage(combat: CombatState) -> None:
        rage_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            SLUDGE_SPINNER_DEADLY_RAGE_DAMAGE,
            SLUDGE_SPINNER_BASE_RAGE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, rage_dmg)
        combat.apply_power_to(creature, PowerId.STRENGTH, SLUDGE_SPINNER_RAGE_STRENGTH, applier=creature)

    oil_spray_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        SLUDGE_SPINNER_DEADLY_OIL_SPRAY_DAMAGE,
        SLUDGE_SPINNER_BASE_OIL_SPRAY_DAMAGE,
    )
    slam_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        SLUDGE_SPINNER_DEADLY_SLAM_DAMAGE,
        SLUDGE_SPINNER_BASE_SLAM_DAMAGE,
    )
    rage_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        SLUDGE_SPINNER_DEADLY_RAGE_DAMAGE,
        SLUDGE_SPINNER_BASE_RAGE_DAMAGE,
    )

    rand = RandomBranchState(SLUDGE_SPINNER_RANDOM_STATE)
    rand.add_branch(SLUDGE_SPINNER_OIL_SPRAY_MOVE, MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch(SLUDGE_SPINNER_SLAM_MOVE, MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch(SLUDGE_SPINNER_RAGE_MOVE, MoveRepeatType.CANNOT_REPEAT)

    states: dict[str, MonsterState] = {
        SLUDGE_SPINNER_RANDOM_STATE: rand,
        SLUDGE_SPINNER_OIL_SPRAY_MOVE: MoveState(
            SLUDGE_SPINNER_OIL_SPRAY_MOVE,
            oil_spray,
            [attack_intent(oil_spray_intent_damage), debuff_intent()],
            follow_up_id=SLUDGE_SPINNER_RANDOM_STATE,
        ),
        SLUDGE_SPINNER_SLAM_MOVE: MoveState(
            SLUDGE_SPINNER_SLAM_MOVE,
            slam,
            [attack_intent(slam_intent_damage)],
            follow_up_id=SLUDGE_SPINNER_RANDOM_STATE,
        ),
        SLUDGE_SPINNER_RAGE_MOVE: MoveState(
            SLUDGE_SPINNER_RAGE_MOVE,
            rage,
            [attack_intent(rage_intent_damage), buff_intent()],
            follow_up_id=SLUDGE_SPINNER_RANDOM_STATE,
        ),
    }
    return creature, MonsterAI(states, SLUDGE_SPINNER_OIL_SPRAY_MOVE)


# ---- Toadpole (HP 21-25 / 22-26 asc) ----

TOADPOLE_MONSTER_ID = "TOADPOLE"
TOADPOLE_BASE_MIN_HP = 21
TOADPOLE_BASE_MAX_HP = 25
TOADPOLE_TOUGH_MIN_HP = 22
TOADPOLE_TOUGH_MAX_HP = 26
TOADPOLE_BASE_SPIKE_SPIT_DAMAGE = 3
TOADPOLE_DEADLY_SPIKE_SPIT_DAMAGE = 4
TOADPOLE_SPIKE_SPIT_REPEAT = 3
TOADPOLE_BASE_WHIRL_DAMAGE = 7
TOADPOLE_DEADLY_WHIRL_DAMAGE = 8
TOADPOLE_SPIKEN_THORNS = 2
TOADPOLE_INIT_MOVE = "INIT_MOVE"
TOADPOLE_SPIKE_SPIT_MOVE = "SPIKE_SPIT_MOVE"
TOADPOLE_WHIRL_MOVE = "WHIRL_MOVE"
TOADPOLE_SPIKEN_MOVE = "SPIKEN_MOVE"


def create_toadpole(
    rng: Rng,
    slot: str = "first",
    ascension_level: int = 0,
) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        TOADPOLE_TOUGH_MIN_HP,
        TOADPOLE_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        TOADPOLE_TOUGH_MAX_HP,
        TOADPOLE_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id=TOADPOLE_MONSTER_ID)

    def spike_spit(combat: CombatState) -> None:
        if creature.has_power(PowerId.THORNS):
            creature.apply_power(PowerId.THORNS, -TOADPOLE_SPIKEN_THORNS, applier=creature)
        spike_spit_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            TOADPOLE_DEADLY_SPIKE_SPIT_DAMAGE,
            TOADPOLE_BASE_SPIKE_SPIT_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, spike_spit_dmg, hits=TOADPOLE_SPIKE_SPIT_REPEAT)

    def whirl(combat: CombatState) -> None:
        whirl_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            TOADPOLE_DEADLY_WHIRL_DAMAGE,
            TOADPOLE_BASE_WHIRL_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, whirl_dmg)

    def spiken(combat: CombatState) -> None:
        creature.apply_power(PowerId.THORNS, TOADPOLE_SPIKEN_THORNS, applier=creature)

    spike_spit_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        TOADPOLE_DEADLY_SPIKE_SPIT_DAMAGE,
        TOADPOLE_BASE_SPIKE_SPIT_DAMAGE,
    )
    whirl_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        TOADPOLE_DEADLY_WHIRL_DAMAGE,
        TOADPOLE_BASE_WHIRL_DAMAGE,
    )

    is_front = slot in {"first", "front"}
    init = ConditionalBranchState(TOADPOLE_INIT_MOVE)
    init.add_branch(lambda: not is_front, TOADPOLE_WHIRL_MOVE)
    init.add_branch(lambda: True, TOADPOLE_SPIKEN_MOVE)

    states: dict[str, MonsterState] = {
        TOADPOLE_INIT_MOVE: init,
        TOADPOLE_SPIKE_SPIT_MOVE: MoveState(
            TOADPOLE_SPIKE_SPIT_MOVE,
            spike_spit,
            [multi_attack_intent(spike_spit_intent_damage, TOADPOLE_SPIKE_SPIT_REPEAT)],
            follow_up_id=TOADPOLE_WHIRL_MOVE,
        ),
        TOADPOLE_WHIRL_MOVE: MoveState(
            TOADPOLE_WHIRL_MOVE,
            whirl,
            [attack_intent(whirl_intent_damage)],
            follow_up_id=TOADPOLE_SPIKEN_MOVE,
        ),
        TOADPOLE_SPIKEN_MOVE: MoveState(
            TOADPOLE_SPIKEN_MOVE,
            spiken,
            [buff_intent()],
            follow_up_id=TOADPOLE_SPIKE_SPIT_MOVE,
        ),
    }

    return creature, MonsterAI(states, TOADPOLE_INIT_MOVE, rng)


# ========================================================================
# NORMAL ENCOUNTERS
# ========================================================================

# ---- CalcifiedCultist (HP 38-41 / 39-42 asc) ----

CULTIST_INCANTATION_MOVE = "INCANTATION_MOVE"
CULTIST_DARK_STRIKE_MOVE = "DARK_STRIKE_MOVE"
CALCIFIED_CULTIST_MONSTER_ID = "CALCIFIED_CULTIST"
CALCIFIED_CULTIST_BASE_MIN_HP = 38
CALCIFIED_CULTIST_BASE_MAX_HP = 41
CALCIFIED_CULTIST_TOUGH_MIN_HP = 39
CALCIFIED_CULTIST_TOUGH_MAX_HP = 42
CALCIFIED_CULTIST_BASE_DARK_STRIKE_DAMAGE = 9
CALCIFIED_CULTIST_DEADLY_DARK_STRIKE_DAMAGE = 11
CALCIFIED_CULTIST_INCANTATION_RITUAL = 2


def create_calcified_cultist(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        CALCIFIED_CULTIST_TOUGH_MIN_HP,
        CALCIFIED_CULTIST_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        CALCIFIED_CULTIST_TOUGH_MAX_HP,
        CALCIFIED_CULTIST_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id=CALCIFIED_CULTIST_MONSTER_ID)

    def incantation(combat: CombatState) -> None:
        creature.apply_power(PowerId.RITUAL, CALCIFIED_CULTIST_INCANTATION_RITUAL)

    def dark_strike(combat: CombatState) -> None:
        dark_strike_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            CALCIFIED_CULTIST_DEADLY_DARK_STRIKE_DAMAGE,
            CALCIFIED_CULTIST_BASE_DARK_STRIKE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, dark_strike_dmg)

    dark_strike_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        CALCIFIED_CULTIST_DEADLY_DARK_STRIKE_DAMAGE,
        CALCIFIED_CULTIST_BASE_DARK_STRIKE_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        CULTIST_INCANTATION_MOVE: MoveState(
            CULTIST_INCANTATION_MOVE,
            incantation,
            [buff_intent()],
            follow_up_id=CULTIST_DARK_STRIKE_MOVE,
        ),
        CULTIST_DARK_STRIKE_MOVE: MoveState(
            CULTIST_DARK_STRIKE_MOVE,
            dark_strike,
            [attack_intent(dark_strike_intent_damage)],
            follow_up_id=CULTIST_DARK_STRIKE_MOVE,
        ),
    }
    return creature, MonsterAI(states, CULTIST_INCANTATION_MOVE)


# ---- DampCultist (HP 51-53 / 52-54 asc) ----

DAMP_CULTIST_MONSTER_ID = "DAMP_CULTIST"
DAMP_CULTIST_BASE_MIN_HP = 51
DAMP_CULTIST_BASE_MAX_HP = 53
DAMP_CULTIST_TOUGH_MIN_HP = 52
DAMP_CULTIST_TOUGH_MAX_HP = 54
DAMP_CULTIST_BASE_DARK_STRIKE_DAMAGE = 1
DAMP_CULTIST_DEADLY_DARK_STRIKE_DAMAGE = 3
DAMP_CULTIST_BASE_INCANTATION_RITUAL = 5
DAMP_CULTIST_DEADLY_INCANTATION_RITUAL = 6


def create_damp_cultist(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        DAMP_CULTIST_TOUGH_MIN_HP,
        DAMP_CULTIST_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        DAMP_CULTIST_TOUGH_MAX_HP,
        DAMP_CULTIST_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id=DAMP_CULTIST_MONSTER_ID)

    def incantation(combat: CombatState) -> None:
        ritual = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            DAMP_CULTIST_DEADLY_INCANTATION_RITUAL,
            DAMP_CULTIST_BASE_INCANTATION_RITUAL,
        )
        creature.apply_power(PowerId.RITUAL, ritual)

    def dark_strike(combat: CombatState) -> None:
        dark_strike_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            DAMP_CULTIST_DEADLY_DARK_STRIKE_DAMAGE,
            DAMP_CULTIST_BASE_DARK_STRIKE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, dark_strike_dmg)

    dark_strike_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        DAMP_CULTIST_DEADLY_DARK_STRIKE_DAMAGE,
        DAMP_CULTIST_BASE_DARK_STRIKE_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        CULTIST_INCANTATION_MOVE: MoveState(
            CULTIST_INCANTATION_MOVE,
            incantation,
            [buff_intent()],
            follow_up_id=CULTIST_DARK_STRIKE_MOVE,
        ),
        CULTIST_DARK_STRIKE_MOVE: MoveState(
            CULTIST_DARK_STRIKE_MOVE,
            dark_strike,
            [attack_intent(dark_strike_intent_damage)],
            follow_up_id=CULTIST_DARK_STRIKE_MOVE,
        ),
    }
    return creature, MonsterAI(states, CULTIST_INCANTATION_MOVE)


# ---- FossilStalker (HP 51-53 / 54-56 asc) ----

FOSSIL_STALKER_MONSTER_ID = "FOSSIL_STALKER"
FOSSIL_STALKER_BASE_MIN_HP = 51
FOSSIL_STALKER_BASE_MAX_HP = 53
FOSSIL_STALKER_TOUGH_MIN_HP = 54
FOSSIL_STALKER_TOUGH_MAX_HP = 56
FOSSIL_STALKER_BASE_TACKLE_DAMAGE = 9
FOSSIL_STALKER_DEADLY_TACKLE_DAMAGE = 11
FOSSIL_STALKER_TACKLE_FRAIL = 1
FOSSIL_STALKER_BASE_LATCH_DAMAGE = 12
FOSSIL_STALKER_DEADLY_LATCH_DAMAGE = 14
FOSSIL_STALKER_BASE_LASH_DAMAGE = 3
FOSSIL_STALKER_DEADLY_LASH_DAMAGE = 4
FOSSIL_STALKER_LASH_REPEAT = 2
FOSSIL_STALKER_SUCK = 3
FOSSIL_STALKER_RANDOM_STATE = "RAND"
FOSSIL_STALKER_TACKLE_MOVE = "TACKLE_MOVE"
FOSSIL_STALKER_LATCH_MOVE = "LATCH_MOVE"
FOSSIL_STALKER_LASH_MOVE = "LASH_MOVE"
FOSSIL_STALKER_MOVE_WEIGHT = 2.0


def create_fossil_stalker(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        FOSSIL_STALKER_TOUGH_MIN_HP,
        FOSSIL_STALKER_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        FOSSIL_STALKER_TOUGH_MAX_HP,
        FOSSIL_STALKER_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id=FOSSIL_STALKER_MONSTER_ID)

    def tackle(combat: CombatState) -> None:
        tackle_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            FOSSIL_STALKER_DEADLY_TACKLE_DAMAGE,
            FOSSIL_STALKER_BASE_TACKLE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, tackle_dmg)
        apply_power_to_living_player_targets(
            combat,
            PowerId.FRAIL,
            FOSSIL_STALKER_TACKLE_FRAIL,
            applier=creature,
        )

    def latch(combat: CombatState) -> None:
        latch_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            FOSSIL_STALKER_DEADLY_LATCH_DAMAGE,
            FOSSIL_STALKER_BASE_LATCH_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, latch_dmg)

    def lash(combat: CombatState) -> None:
        lash_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            FOSSIL_STALKER_DEADLY_LASH_DAMAGE,
            FOSSIL_STALKER_BASE_LASH_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, lash_dmg, hits=FOSSIL_STALKER_LASH_REPEAT)

    tackle_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        FOSSIL_STALKER_DEADLY_TACKLE_DAMAGE,
        FOSSIL_STALKER_BASE_TACKLE_DAMAGE,
    )
    latch_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        FOSSIL_STALKER_DEADLY_LATCH_DAMAGE,
        FOSSIL_STALKER_BASE_LATCH_DAMAGE,
    )
    lash_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        FOSSIL_STALKER_DEADLY_LASH_DAMAGE,
        FOSSIL_STALKER_BASE_LASH_DAMAGE,
    )

    rand = RandomBranchState(FOSSIL_STALKER_RANDOM_STATE)
    rand.add_branch(FOSSIL_STALKER_LATCH_MOVE, weight=FOSSIL_STALKER_MOVE_WEIGHT)
    rand.add_branch(FOSSIL_STALKER_TACKLE_MOVE, weight=FOSSIL_STALKER_MOVE_WEIGHT)
    rand.add_branch(FOSSIL_STALKER_LASH_MOVE, weight=FOSSIL_STALKER_MOVE_WEIGHT)

    states: dict[str, MonsterState] = {
        FOSSIL_STALKER_RANDOM_STATE: rand,
        FOSSIL_STALKER_TACKLE_MOVE: MoveState(
            FOSSIL_STALKER_TACKLE_MOVE,
            tackle,
            [attack_intent(tackle_intent_damage), debuff_intent()],
            follow_up_id=FOSSIL_STALKER_RANDOM_STATE,
        ),
        FOSSIL_STALKER_LATCH_MOVE: MoveState(
            FOSSIL_STALKER_LATCH_MOVE,
            latch,
            [attack_intent(latch_intent_damage)],
            follow_up_id=FOSSIL_STALKER_RANDOM_STATE,
        ),
        FOSSIL_STALKER_LASH_MOVE: MoveState(
            FOSSIL_STALKER_LASH_MOVE,
            lash,
            [multi_attack_intent(lash_intent_damage, FOSSIL_STALKER_LASH_REPEAT)],
            follow_up_id=FOSSIL_STALKER_RANDOM_STATE,
        ),
    }

    creature.apply_power(PowerId.SUCK, FOSSIL_STALKER_SUCK)
    return creature, MonsterAI(states, FOSSIL_STALKER_LATCH_MOVE)


# ---- GremlinMerc (HP 47-49 / 51-53 asc) + SneakyGremlin + FatGremlin ----

GREMLIN_SPAWNED_MOVE = "SPAWNED_MOVE"
GREMLIN_TACKLE_MOVE = "TACKLE_MOVE"
SNEAKY_GREMLIN_MONSTER_ID = "SNEAKY_GREMLIN"
SNEAKY_GREMLIN_BASE_MIN_HP = 10
SNEAKY_GREMLIN_BASE_MAX_HP = 14
SNEAKY_GREMLIN_TOUGH_MIN_HP = 11
SNEAKY_GREMLIN_TOUGH_MAX_HP = 15
SNEAKY_GREMLIN_BASE_TACKLE_DAMAGE = 9
SNEAKY_GREMLIN_DEADLY_TACKLE_DAMAGE = 10


def create_sneaky_gremlin(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        SNEAKY_GREMLIN_TOUGH_MIN_HP,
        SNEAKY_GREMLIN_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        SNEAKY_GREMLIN_TOUGH_MAX_HP,
        SNEAKY_GREMLIN_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id=SNEAKY_GREMLIN_MONSTER_ID)

    def spawned(combat: CombatState) -> None:
        pass

    def tackle(combat: CombatState) -> None:
        tackle_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            SNEAKY_GREMLIN_DEADLY_TACKLE_DAMAGE,
            SNEAKY_GREMLIN_BASE_TACKLE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, tackle_dmg)

    tackle_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        SNEAKY_GREMLIN_DEADLY_TACKLE_DAMAGE,
        SNEAKY_GREMLIN_BASE_TACKLE_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        GREMLIN_SPAWNED_MOVE: MoveState(
            GREMLIN_SPAWNED_MOVE,
            spawned,
            [Intent(IntentType.STUN)],
            follow_up_id=GREMLIN_TACKLE_MOVE,
        ),
        GREMLIN_TACKLE_MOVE: MoveState(
            GREMLIN_TACKLE_MOVE,
            tackle,
            [attack_intent(tackle_intent_damage)],
            follow_up_id=GREMLIN_TACKLE_MOVE,
        ),
    }
    return creature, MonsterAI(states, GREMLIN_SPAWNED_MOVE)


FAT_GREMLIN_MONSTER_ID = "FAT_GREMLIN"
FAT_GREMLIN_BASE_MIN_HP = 13
FAT_GREMLIN_BASE_MAX_HP = 17
FAT_GREMLIN_TOUGH_MIN_HP = 14
FAT_GREMLIN_TOUGH_MAX_HP = 18
FAT_GREMLIN_FLEE_MOVE = "FLEE_MOVE"


def create_fat_gremlin(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        FAT_GREMLIN_TOUGH_MIN_HP,
        FAT_GREMLIN_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        FAT_GREMLIN_TOUGH_MAX_HP,
        FAT_GREMLIN_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id=FAT_GREMLIN_MONSTER_ID)

    def spawned(combat: CombatState) -> None:
        pass

    def flee(combat: CombatState) -> None:
        combat.escape_creature(creature)

    states: dict[str, MonsterState] = {
        GREMLIN_SPAWNED_MOVE: MoveState(
            GREMLIN_SPAWNED_MOVE,
            spawned,
            [Intent(IntentType.STUN)],
            follow_up_id=FAT_GREMLIN_FLEE_MOVE,
        ),
        FAT_GREMLIN_FLEE_MOVE: MoveState(
            FAT_GREMLIN_FLEE_MOVE,
            flee,
            [Intent(IntentType.ESCAPE)],
            follow_up_id=FAT_GREMLIN_FLEE_MOVE,
        ),
    }
    return creature, MonsterAI(states, GREMLIN_SPAWNED_MOVE)


GREMLIN_MERC_MONSTER_ID = "GREMLIN_MERC"
GREMLIN_MERC_BASE_MIN_HP = 47
GREMLIN_MERC_BASE_MAX_HP = 49
GREMLIN_MERC_TOUGH_MIN_HP = 51
GREMLIN_MERC_TOUGH_MAX_HP = 53
GREMLIN_MERC_BASE_GIMME_DAMAGE = 7
GREMLIN_MERC_TOUGH_GIMME_DAMAGE = 8
GREMLIN_MERC_GIMME_REPEAT = 2
GREMLIN_MERC_BASE_DOUBLE_SMASH_DAMAGE = 6
GREMLIN_MERC_TOUGH_DOUBLE_SMASH_DAMAGE = 7
GREMLIN_MERC_DOUBLE_SMASH_REPEAT = 2
GREMLIN_MERC_DOUBLE_SMASH_WEAK = 2
GREMLIN_MERC_BASE_HEHE_DAMAGE = 8
GREMLIN_MERC_TOUGH_HEHE_DAMAGE = 9
GREMLIN_MERC_HEHE_STRENGTH = 2
GREMLIN_MERC_SURPRISE = 1
GREMLIN_MERC_THIEVERY = 20
GREMLIN_MERC_GIMME_MOVE = "GIMME_MOVE"
GREMLIN_MERC_DOUBLE_SMASH_MOVE = "DOUBLE_SMASH_MOVE"
GREMLIN_MERC_HEHE_MOVE = "HEHE_MOVE"


def create_gremlin_merc(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        GREMLIN_MERC_TOUGH_MIN_HP,
        GREMLIN_MERC_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        GREMLIN_MERC_TOUGH_MAX_HP,
        GREMLIN_MERC_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id=GREMLIN_MERC_MONSTER_ID)

    def gimme(combat: CombatState) -> None:
        gimme_dmg = _ascension_value(
            _combat_ascension_level(combat),
            TOUGH_ENEMIES_ASCENSION_LEVEL,
            GREMLIN_MERC_TOUGH_GIMME_DAMAGE,
            GREMLIN_MERC_BASE_GIMME_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, gimme_dmg, hits=GREMLIN_MERC_GIMME_REPEAT)

    def double_smash(combat: CombatState) -> None:
        double_smash_dmg = _ascension_value(
            _combat_ascension_level(combat),
            TOUGH_ENEMIES_ASCENSION_LEVEL,
            GREMLIN_MERC_TOUGH_DOUBLE_SMASH_DAMAGE,
            GREMLIN_MERC_BASE_DOUBLE_SMASH_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, double_smash_dmg, hits=GREMLIN_MERC_DOUBLE_SMASH_REPEAT)
        apply_power_to_living_player_targets(
            combat,
            PowerId.WEAK,
            GREMLIN_MERC_DOUBLE_SMASH_WEAK,
            applier=creature,
        )

    def hehe(combat: CombatState) -> None:
        hehe_dmg = _ascension_value(
            _combat_ascension_level(combat),
            TOUGH_ENEMIES_ASCENSION_LEVEL,
            GREMLIN_MERC_TOUGH_HEHE_DAMAGE,
            GREMLIN_MERC_BASE_HEHE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, hehe_dmg)
        combat.apply_power_to(creature, PowerId.STRENGTH, GREMLIN_MERC_HEHE_STRENGTH, applier=creature)

    gimme_intent_damage = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        GREMLIN_MERC_TOUGH_GIMME_DAMAGE,
        GREMLIN_MERC_BASE_GIMME_DAMAGE,
    )
    double_smash_intent_damage = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        GREMLIN_MERC_TOUGH_DOUBLE_SMASH_DAMAGE,
        GREMLIN_MERC_BASE_DOUBLE_SMASH_DAMAGE,
    )
    hehe_intent_damage = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        GREMLIN_MERC_TOUGH_HEHE_DAMAGE,
        GREMLIN_MERC_BASE_HEHE_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        GREMLIN_MERC_GIMME_MOVE: MoveState(
            GREMLIN_MERC_GIMME_MOVE,
            gimme,
            [multi_attack_intent(gimme_intent_damage, GREMLIN_MERC_GIMME_REPEAT)],
            follow_up_id=GREMLIN_MERC_DOUBLE_SMASH_MOVE,
        ),
        GREMLIN_MERC_DOUBLE_SMASH_MOVE: MoveState(
            GREMLIN_MERC_DOUBLE_SMASH_MOVE,
            double_smash,
            [multi_attack_intent(double_smash_intent_damage, GREMLIN_MERC_DOUBLE_SMASH_REPEAT), debuff_intent()],
            follow_up_id=GREMLIN_MERC_HEHE_MOVE,
        ),
        GREMLIN_MERC_HEHE_MOVE: MoveState(
            GREMLIN_MERC_HEHE_MOVE,
            hehe,
            [attack_intent(hehe_intent_damage), buff_intent()],
            follow_up_id=GREMLIN_MERC_GIMME_MOVE,
        ),
    }

    creature.apply_power(PowerId.SURPRISE, GREMLIN_MERC_SURPRISE)
    creature.apply_power(PowerId.THIEVERY, GREMLIN_MERC_THIEVERY)
    return creature, MonsterAI(states, GREMLIN_MERC_GIMME_MOVE)


# ---- HauntedShip (HP 63 / 67 asc) ----

HAUNTED_SHIP_MONSTER_ID = "HAUNTED_SHIP"
HAUNTED_SHIP_BASE_HP = 63
HAUNTED_SHIP_TOUGH_HP = 67
HAUNTED_SHIP_BASE_RAMMING_SPEED_DAMAGE = 10
HAUNTED_SHIP_DEADLY_RAMMING_SPEED_DAMAGE = 11
HAUNTED_SHIP_RAMMING_SPEED_STATUS_COUNT = 2
HAUNTED_SHIP_BASE_SWIPE_DAMAGE = 13
HAUNTED_SHIP_DEADLY_SWIPE_DAMAGE = 14
HAUNTED_SHIP_BASE_STOMP_DAMAGE = 4
HAUNTED_SHIP_DEADLY_STOMP_DAMAGE = 5
HAUNTED_SHIP_STOMP_REPEAT = 3
HAUNTED_SHIP_HAUNT_DEBUFF = 2
HAUNTED_SHIP_RANDOM_STATE = "RAND"
HAUNTED_SHIP_RAMMING_SPEED_MOVE = "RAMMING_SPEED_MOVE"
HAUNTED_SHIP_SWIPE_MOVE = "SWIPE_MOVE"
HAUNTED_SHIP_STOMP_MOVE = "STOMP_MOVE"
HAUNTED_SHIP_HAUNT_MOVE = "HAUNT_MOVE"


def create_haunted_ship(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        HAUNTED_SHIP_TOUGH_HP,
        HAUNTED_SHIP_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=HAUNTED_SHIP_MONSTER_ID)

    def _odd_round_weight() -> float:
        combat = creature.combat_state
        return 1.0 if combat is None or combat.round_number % 2 != 0 else 0.0

    def _even_round_weight() -> float:
        combat = creature.combat_state
        return 1.0 if combat is not None and combat.round_number % 2 == 0 else 0.0

    def ramming_speed(combat: CombatState) -> None:
        ramming_speed_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            HAUNTED_SHIP_DEADLY_RAMMING_SPEED_DAMAGE,
            HAUNTED_SHIP_BASE_RAMMING_SPEED_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, ramming_speed_dmg)
        if combat.is_over:
            return
        add_generated_cards_to_living_player_discards(
            combat,
            make_wound,
            HAUNTED_SHIP_RAMMING_SPEED_STATUS_COUNT,
        )

    def swipe(combat: CombatState) -> None:
        swipe_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            HAUNTED_SHIP_DEADLY_SWIPE_DAMAGE,
            HAUNTED_SHIP_BASE_SWIPE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, swipe_dmg)

    def stomp(combat: CombatState) -> None:
        stomp_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            HAUNTED_SHIP_DEADLY_STOMP_DAMAGE,
            HAUNTED_SHIP_BASE_STOMP_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, stomp_dmg, hits=HAUNTED_SHIP_STOMP_REPEAT)

    def haunt(combat: CombatState) -> None:
        apply_power_to_living_player_targets(combat, PowerId.WEAK, HAUNTED_SHIP_HAUNT_DEBUFF, applier=creature)
        apply_power_to_living_player_targets(combat, PowerId.FRAIL, HAUNTED_SHIP_HAUNT_DEBUFF, applier=creature)
        apply_power_to_living_player_targets(combat, PowerId.VULNERABLE, HAUNTED_SHIP_HAUNT_DEBUFF, applier=creature)

    ramming_speed_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        HAUNTED_SHIP_DEADLY_RAMMING_SPEED_DAMAGE,
        HAUNTED_SHIP_BASE_RAMMING_SPEED_DAMAGE,
    )
    swipe_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        HAUNTED_SHIP_DEADLY_SWIPE_DAMAGE,
        HAUNTED_SHIP_BASE_SWIPE_DAMAGE,
    )
    stomp_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        HAUNTED_SHIP_DEADLY_STOMP_DAMAGE,
        HAUNTED_SHIP_BASE_STOMP_DAMAGE,
    )

    rand = RandomBranchState(HAUNTED_SHIP_RANDOM_STATE)
    rand.add_branch(HAUNTED_SHIP_RAMMING_SPEED_MOVE, MoveRepeatType.CANNOT_REPEAT, weight=_odd_round_weight)
    rand.add_branch(HAUNTED_SHIP_SWIPE_MOVE, MoveRepeatType.CANNOT_REPEAT, weight=_odd_round_weight)
    rand.add_branch(HAUNTED_SHIP_STOMP_MOVE, MoveRepeatType.CANNOT_REPEAT, weight=_odd_round_weight)
    rand.add_branch(HAUNTED_SHIP_HAUNT_MOVE, MoveRepeatType.USE_ONLY_ONCE, weight=_even_round_weight)

    states: dict[str, MonsterState] = {
        HAUNTED_SHIP_RANDOM_STATE: rand,
        HAUNTED_SHIP_RAMMING_SPEED_MOVE: MoveState(
            HAUNTED_SHIP_RAMMING_SPEED_MOVE,
            ramming_speed,
            [attack_intent(ramming_speed_intent_damage), status_intent()],
            follow_up_id=HAUNTED_SHIP_RANDOM_STATE,
        ),
        HAUNTED_SHIP_SWIPE_MOVE: MoveState(
            HAUNTED_SHIP_SWIPE_MOVE,
            swipe,
            [attack_intent(swipe_intent_damage)],
            follow_up_id=HAUNTED_SHIP_RANDOM_STATE,
        ),
        HAUNTED_SHIP_STOMP_MOVE: MoveState(
            HAUNTED_SHIP_STOMP_MOVE,
            stomp,
            [multi_attack_intent(stomp_intent_damage, HAUNTED_SHIP_STOMP_REPEAT)],
            follow_up_id=HAUNTED_SHIP_RANDOM_STATE,
        ),
        HAUNTED_SHIP_HAUNT_MOVE: MoveState(
            HAUNTED_SHIP_HAUNT_MOVE,
            haunt,
            [debuff_intent()],
            follow_up_id=HAUNTED_SHIP_RANDOM_STATE,
        ),
    }
    states[HAUNTED_SHIP_RAMMING_SPEED_MOVE].intents[1].hits = HAUNTED_SHIP_RAMMING_SPEED_STATUS_COUNT
    return creature, MonsterAI(states, HAUNTED_SHIP_RAMMING_SPEED_MOVE)


# ---- LivingFog (HP 80 / 82 asc) + GasBomb ----

GAS_BOMB_MONSTER_ID = "GAS_BOMB"
GAS_BOMB_MINION_AMOUNT = 1
GAS_BOMB_BASE_HP = 7
GAS_BOMB_TOUGH_HP = 8
GAS_BOMB_BASE_EXPLODE_DAMAGE = 8
GAS_BOMB_DEADLY_EXPLODE_DAMAGE = 9
GAS_BOMB_EXPLODE_MOVE = "EXPLODE_MOVE"


def create_gas_bomb(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        GAS_BOMB_TOUGH_HP,
        GAS_BOMB_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=GAS_BOMB_MONSTER_ID)

    def explode(combat: CombatState) -> None:
        explode_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            GAS_BOMB_DEADLY_EXPLODE_DAMAGE,
            GAS_BOMB_BASE_EXPLODE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, explode_dmg)
        combat.kill_creature(creature)

    explode_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        GAS_BOMB_DEADLY_EXPLODE_DAMAGE,
        GAS_BOMB_BASE_EXPLODE_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        GAS_BOMB_EXPLODE_MOVE: MoveState(
            GAS_BOMB_EXPLODE_MOVE,
            explode,
            [Intent(IntentType.DEATH_BLOW, damage=explode_intent_damage)],
            follow_up_id=GAS_BOMB_EXPLODE_MOVE,
        ),
    }
    return creature, MonsterAI(states, GAS_BOMB_EXPLODE_MOVE)


LIVING_FOG_MONSTER_ID = "LIVING_FOG"
LIVING_FOG_BASE_HP = 80
LIVING_FOG_TOUGH_HP = 82
LIVING_FOG_BASE_ADVANCED_GAS_DAMAGE = 8
LIVING_FOG_DEADLY_ADVANCED_GAS_DAMAGE = 9
LIVING_FOG_ADVANCED_GAS_SMOGGY = 1
LIVING_FOG_BASE_BLOAT_DAMAGE = 5
LIVING_FOG_DEADLY_BLOAT_DAMAGE = 6
LIVING_FOG_BASE_SUPER_GAS_BLAST_DAMAGE = 8
LIVING_FOG_DEADLY_SUPER_GAS_BLAST_DAMAGE = 9
LIVING_FOG_INITIAL_BLOAT_AMOUNT = 1
LIVING_FOG_MAX_BLOAT_AMOUNT = 5
LIVING_FOG_BLOAT_AMOUNT_KEY = "bloat_amount"
LIVING_FOG_ADVANCED_GAS_MOVE = "ADVANCED_GAS_MOVE"
LIVING_FOG_BLOAT_MOVE = "BLOAT_MOVE"
LIVING_FOG_SUPER_GAS_BLAST_MOVE = "SUPER_GAS_BLAST_MOVE"


def create_living_fog(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        LIVING_FOG_TOUGH_HP,
        LIVING_FOG_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=LIVING_FOG_MONSTER_ID)
    state = {LIVING_FOG_BLOAT_AMOUNT_KEY: LIVING_FOG_INITIAL_BLOAT_AMOUNT}

    def advanced_gas(combat: CombatState) -> None:
        advanced_gas_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            LIVING_FOG_DEADLY_ADVANCED_GAS_DAMAGE,
            LIVING_FOG_BASE_ADVANCED_GAS_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, advanced_gas_dmg)
        apply_power_to_living_player_targets(
            combat,
            PowerId.SMOGGY,
            LIVING_FOG_ADVANCED_GAS_SMOGGY,
            applier=creature,
        )

    def bloat(combat: CombatState) -> None:
        bloat_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            LIVING_FOG_DEADLY_BLOAT_DAMAGE,
            LIVING_FOG_BASE_BLOAT_DAMAGE,
        )
        for _ in range(state[LIVING_FOG_BLOAT_AMOUNT_KEY]):
            bomb, bomb_ai = create_gas_bomb(rng, ascension_level=_combat_ascension_level(combat))
            combat.add_enemy(bomb, bomb_ai)
        state[LIVING_FOG_BLOAT_AMOUNT_KEY] = min(
            state[LIVING_FOG_BLOAT_AMOUNT_KEY] + 1,
            LIVING_FOG_MAX_BLOAT_AMOUNT,
        )
        _deal_damage_to_player(combat, creature, bloat_dmg)

    def super_gas_blast(combat: CombatState) -> None:
        super_gas_blast_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            LIVING_FOG_DEADLY_SUPER_GAS_BLAST_DAMAGE,
            LIVING_FOG_BASE_SUPER_GAS_BLAST_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, super_gas_blast_dmg)

    advanced_gas_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        LIVING_FOG_DEADLY_ADVANCED_GAS_DAMAGE,
        LIVING_FOG_BASE_ADVANCED_GAS_DAMAGE,
    )
    bloat_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        LIVING_FOG_DEADLY_BLOAT_DAMAGE,
        LIVING_FOG_BASE_BLOAT_DAMAGE,
    )
    super_gas_blast_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        LIVING_FOG_DEADLY_SUPER_GAS_BLAST_DAMAGE,
        LIVING_FOG_BASE_SUPER_GAS_BLAST_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        LIVING_FOG_ADVANCED_GAS_MOVE: MoveState(
            LIVING_FOG_ADVANCED_GAS_MOVE,
            advanced_gas,
            [attack_intent(advanced_gas_intent_damage), Intent(IntentType.CARD_DEBUFF)],
            follow_up_id=LIVING_FOG_BLOAT_MOVE,
        ),
        LIVING_FOG_BLOAT_MOVE: MoveState(
            LIVING_FOG_BLOAT_MOVE,
            bloat,
            [attack_intent(bloat_intent_damage), Intent(IntentType.SUMMON)],
            follow_up_id=LIVING_FOG_SUPER_GAS_BLAST_MOVE,
        ),
        LIVING_FOG_SUPER_GAS_BLAST_MOVE: MoveState(
            LIVING_FOG_SUPER_GAS_BLAST_MOVE,
            super_gas_blast,
            [attack_intent(super_gas_blast_intent_damage)],
            follow_up_id=LIVING_FOG_BLOAT_MOVE,
        ),
    }
    return creature, MonsterAI(states, LIVING_FOG_ADVANCED_GAS_MOVE)


# ---- PunchConstruct (HP 55 / 60 asc) ----

PUNCH_CONSTRUCT_MONSTER_ID = "PUNCH_CONSTRUCT"
PUNCH_CONSTRUCT_BASE_HP = 55
PUNCH_CONSTRUCT_TOUGH_HP = 60
PUNCH_CONSTRUCT_BASE_STRONG_PUNCH_DAMAGE = 14
PUNCH_CONSTRUCT_DEADLY_STRONG_PUNCH_DAMAGE = 16
PUNCH_CONSTRUCT_BASE_FAST_PUNCH_DAMAGE = 5
PUNCH_CONSTRUCT_DEADLY_FAST_PUNCH_DAMAGE = 6
PUNCH_CONSTRUCT_FAST_PUNCH_REPEAT = 2
PUNCH_CONSTRUCT_FAST_PUNCH_WEAK = 1
PUNCH_CONSTRUCT_READY_BLOCK = 10
PUNCH_CONSTRUCT_ARTIFACT = 1
PUNCH_CONSTRUCT_READY_MOVE = "READY_MOVE"
PUNCH_CONSTRUCT_STRONG_PUNCH_MOVE = "STRONG_PUNCH_MOVE"
PUNCH_CONSTRUCT_FAST_PUNCH_MOVE = "FAST_PUNCH_MOVE"


def create_punch_construct(
    rng: Rng,
    *,
    starts_with_strong_punch: bool = False,
    starting_hp_reduction: int = 0,
    ascension_level: int = 0,
) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        PUNCH_CONSTRUCT_TOUGH_HP,
        PUNCH_CONSTRUCT_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=PUNCH_CONSTRUCT_MONSTER_ID)

    if starting_hp_reduction > 0:
        creature.current_hp = max(1, creature.current_hp - starting_hp_reduction)
    creature.apply_power(PowerId.ARTIFACT, PUNCH_CONSTRUCT_ARTIFACT)

    def ready(combat: CombatState) -> None:
        _gain_block(creature, PUNCH_CONSTRUCT_READY_BLOCK, combat)

    def strong_punch(combat: CombatState) -> None:
        strong_punch_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            PUNCH_CONSTRUCT_DEADLY_STRONG_PUNCH_DAMAGE,
            PUNCH_CONSTRUCT_BASE_STRONG_PUNCH_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, strong_punch_dmg)

    def fast_punch(combat: CombatState) -> None:
        fast_punch_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            PUNCH_CONSTRUCT_DEADLY_FAST_PUNCH_DAMAGE,
            PUNCH_CONSTRUCT_BASE_FAST_PUNCH_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, fast_punch_dmg, hits=PUNCH_CONSTRUCT_FAST_PUNCH_REPEAT)
        apply_power_to_living_player_targets(
            combat,
            PowerId.WEAK,
            PUNCH_CONSTRUCT_FAST_PUNCH_WEAK,
            applier=creature,
        )

    strong_punch_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        PUNCH_CONSTRUCT_DEADLY_STRONG_PUNCH_DAMAGE,
        PUNCH_CONSTRUCT_BASE_STRONG_PUNCH_DAMAGE,
    )
    fast_punch_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        PUNCH_CONSTRUCT_DEADLY_FAST_PUNCH_DAMAGE,
        PUNCH_CONSTRUCT_BASE_FAST_PUNCH_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        PUNCH_CONSTRUCT_READY_MOVE: MoveState(
            PUNCH_CONSTRUCT_READY_MOVE,
            ready,
            [defend_intent()],
            follow_up_id=PUNCH_CONSTRUCT_STRONG_PUNCH_MOVE,
        ),
        PUNCH_CONSTRUCT_STRONG_PUNCH_MOVE: MoveState(
            PUNCH_CONSTRUCT_STRONG_PUNCH_MOVE,
            strong_punch,
            [attack_intent(strong_punch_intent_damage)],
            follow_up_id=PUNCH_CONSTRUCT_FAST_PUNCH_MOVE,
        ),
        PUNCH_CONSTRUCT_FAST_PUNCH_MOVE: MoveState(
            PUNCH_CONSTRUCT_FAST_PUNCH_MOVE,
            fast_punch,
            [multi_attack_intent(fast_punch_intent_damage, PUNCH_CONSTRUCT_FAST_PUNCH_REPEAT), debuff_intent()],
            follow_up_id=PUNCH_CONSTRUCT_READY_MOVE,
        ),
    }
    initial = PUNCH_CONSTRUCT_STRONG_PUNCH_MOVE if starts_with_strong_punch else PUNCH_CONSTRUCT_READY_MOVE
    return creature, MonsterAI(states, initial)


# ---- SewerClam (HP 56 / 58 asc) ----

SEWER_CLAM_MONSTER_ID = "SEWER_CLAM"
SEWER_CLAM_BASE_HP = 56
SEWER_CLAM_TOUGH_HP = 58
SEWER_CLAM_BASE_JET_DAMAGE = 10
SEWER_CLAM_DEADLY_JET_DAMAGE = 11
SEWER_CLAM_PRESSURIZE_STRENGTH = 4
SEWER_CLAM_BASE_PLATING = 8
SEWER_CLAM_TOUGH_PLATING = 9
SEWER_CLAM_PRESSURIZE_MOVE = "PRESSURIZE_MOVE"
SEWER_CLAM_JET_MOVE = "JET_MOVE"


def create_sewer_clam(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        SEWER_CLAM_TOUGH_HP,
        SEWER_CLAM_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=SEWER_CLAM_MONSTER_ID)

    def pressurize(combat: CombatState) -> None:
        creature.apply_power(PowerId.STRENGTH, SEWER_CLAM_PRESSURIZE_STRENGTH, applier=creature)

    def jet(combat: CombatState) -> None:
        jet_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            SEWER_CLAM_DEADLY_JET_DAMAGE,
            SEWER_CLAM_BASE_JET_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, jet_dmg)

    jet_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        SEWER_CLAM_DEADLY_JET_DAMAGE,
        SEWER_CLAM_BASE_JET_DAMAGE,
    )
    plating = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        SEWER_CLAM_TOUGH_PLATING,
        SEWER_CLAM_BASE_PLATING,
    )

    states: dict[str, MonsterState] = {
        SEWER_CLAM_PRESSURIZE_MOVE: MoveState(
            SEWER_CLAM_PRESSURIZE_MOVE,
            pressurize,
            [buff_intent()],
            follow_up_id=SEWER_CLAM_JET_MOVE,
        ),
        SEWER_CLAM_JET_MOVE: MoveState(
            SEWER_CLAM_JET_MOVE,
            jet,
            [attack_intent(jet_intent_damage)],
            follow_up_id=SEWER_CLAM_PRESSURIZE_MOVE,
        ),
    }
    creature.apply_power(PowerId.PLATING, plating)
    return creature, MonsterAI(states, SEWER_CLAM_JET_MOVE)


# ---- TwoTailedRat (HP 17-21 / 18-22 asc) ----

TWO_TAILED_RAT_MONSTER_ID = "TWO_TAILED_RAT"
TWO_TAILED_RAT_BASE_MIN_HP = 17
TWO_TAILED_RAT_BASE_MAX_HP = 21
TWO_TAILED_RAT_TOUGH_MIN_HP = 18
TWO_TAILED_RAT_TOUGH_MAX_HP = 22
TWO_TAILED_RAT_BASE_SCRATCH_DAMAGE = 8
TWO_TAILED_RAT_DEADLY_SCRATCH_DAMAGE = 9
TWO_TAILED_RAT_BASE_DISEASE_BITE_DAMAGE = 6
TWO_TAILED_RAT_DEADLY_DISEASE_BITE_DAMAGE = 7
TWO_TAILED_RAT_SCREECH_FRAIL = 1
TWO_TAILED_RAT_INITIAL_TURNS_UNTIL_SUMMONABLE = 2
TWO_TAILED_RAT_CALL_FOR_BACKUP_LIMIT = 3
TWO_TAILED_RAT_MAX_ALIVE_RATS = 5
TWO_TAILED_RAT_ATTACK_WEIGHT_WHEN_SUMMONABLE = 1.0 / 12.0
TWO_TAILED_RAT_ATTACK_WEIGHT = 1.0
TWO_TAILED_RAT_SCREECH_WEIGHT = 3.0
TWO_TAILED_RAT_CALL_FOR_BACKUP_WEIGHT = 0.75
TWO_TAILED_RAT_TURNS_UNTIL_SUMMONABLE_KEY = "turns_until_summonable"
TWO_TAILED_RAT_CALL_FOR_BACKUP_COUNT_KEY = "call_for_backup_count"
TWO_TAILED_RAT_RANDOM_STATE = "RAND"
TWO_TAILED_RAT_SCRATCH_MOVE = "SCRATCH_MOVE"
TWO_TAILED_RAT_DISEASE_BITE_MOVE = "DISEASE_BITE_MOVE"
TWO_TAILED_RAT_SCREECH_MOVE = "SCREECH_MOVE"
TWO_TAILED_RAT_CALL_FOR_BACKUP_MOVE = "CALL_FOR_BACKUP_MOVE"


def create_two_tailed_rat(
    rng: Rng,
    starter_move_idx: int = -1,
    ascension_level: int = 0,
) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        TWO_TAILED_RAT_TOUGH_MIN_HP,
        TWO_TAILED_RAT_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        TWO_TAILED_RAT_TOUGH_MAX_HP,
        TWO_TAILED_RAT_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id=TWO_TAILED_RAT_MONSTER_ID)
    state = {
        TWO_TAILED_RAT_TURNS_UNTIL_SUMMONABLE_KEY: TWO_TAILED_RAT_INITIAL_TURNS_UNTIL_SUMMONABLE,
        TWO_TAILED_RAT_CALL_FOR_BACKUP_COUNT_KEY: 0,
    }

    def can_summon(combat: CombatState | None = None) -> bool:
        combat = combat or creature.combat_state
        if combat is None:
            return False
        if state[TWO_TAILED_RAT_TURNS_UNTIL_SUMMONABLE_KEY] > 0:
            return False
        if state[TWO_TAILED_RAT_CALL_FOR_BACKUP_COUNT_KEY] >= TWO_TAILED_RAT_CALL_FOR_BACKUP_LIMIT:
            return False
        alive_rats = [
            enemy
            for enemy in combat.enemies
            if enemy.monster_id == TWO_TAILED_RAT_MONSTER_ID and enemy.is_alive
        ]
        if len(alive_rats) >= TWO_TAILED_RAT_MAX_ALIVE_RATS:
            return False
        for enemy in alive_rats:
            if enemy is creature:
                continue
            ai = combat.enemy_ais.get(enemy.combat_id)
            if ai is not None and ai.current_move.state_id == TWO_TAILED_RAT_CALL_FOR_BACKUP_MOVE:
                return False
        return True

    def _attack_performed() -> None:
        state[TWO_TAILED_RAT_TURNS_UNTIL_SUMMONABLE_KEY] -= 1

    def scratch(combat: CombatState) -> None:
        scratch_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            TWO_TAILED_RAT_DEADLY_SCRATCH_DAMAGE,
            TWO_TAILED_RAT_BASE_SCRATCH_DAMAGE,
        )
        _attack_performed()
        _deal_damage_to_player(combat, creature, scratch_dmg)

    def disease_bite(combat: CombatState) -> None:
        disease_bite_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            TWO_TAILED_RAT_DEADLY_DISEASE_BITE_DAMAGE,
            TWO_TAILED_RAT_BASE_DISEASE_BITE_DAMAGE,
        )
        _attack_performed()
        _deal_damage_to_player(combat, creature, disease_bite_dmg)

    def screech(combat: CombatState) -> None:
        _attack_performed()
        apply_power_to_living_player_targets(
            combat,
            PowerId.FRAIL,
            TWO_TAILED_RAT_SCREECH_FRAIL,
            applier=creature,
        )

    def call_for_backup(combat: CombatState) -> None:
        if can_summon(combat):
            backup, backup_ai = create_two_tailed_rat(rng, ascension_level=_combat_ascension_level(combat))
            combat.add_enemy(backup, backup_ai)
        rat_ais = [
            combat.enemy_ais[enemy.combat_id]
            for enemy in combat.enemies
            if enemy.monster_id == TWO_TAILED_RAT_MONSTER_ID and enemy.combat_id in combat.enemy_ais
        ]
        max_count = max(
            getattr(ai, "_two_tailed_rat_state", {}).get(TWO_TAILED_RAT_CALL_FOR_BACKUP_COUNT_KEY, 0) + 1
            for ai in rat_ais
        )
        for ai in rat_ais:
            rat_state = getattr(ai, "_two_tailed_rat_state", None)
            if rat_state is not None:
                rat_state[TWO_TAILED_RAT_CALL_FOR_BACKUP_COUNT_KEY] = max_count

    scratch_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        TWO_TAILED_RAT_DEADLY_SCRATCH_DAMAGE,
        TWO_TAILED_RAT_BASE_SCRATCH_DAMAGE,
    )
    disease_bite_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        TWO_TAILED_RAT_DEADLY_DISEASE_BITE_DAMAGE,
        TWO_TAILED_RAT_BASE_DISEASE_BITE_DAMAGE,
    )

    rand = RandomBranchState(TWO_TAILED_RAT_RANDOM_STATE)
    rand.add_branch(
        TWO_TAILED_RAT_SCRATCH_MOVE,
        MoveRepeatType.CANNOT_REPEAT,
        weight=lambda: TWO_TAILED_RAT_ATTACK_WEIGHT_WHEN_SUMMONABLE if can_summon() else TWO_TAILED_RAT_ATTACK_WEIGHT,
    )
    rand.add_branch(
        TWO_TAILED_RAT_DISEASE_BITE_MOVE,
        MoveRepeatType.CANNOT_REPEAT,
        weight=lambda: TWO_TAILED_RAT_ATTACK_WEIGHT_WHEN_SUMMONABLE if can_summon() else TWO_TAILED_RAT_ATTACK_WEIGHT,
    )
    rand.add_branch(
        TWO_TAILED_RAT_SCREECH_MOVE,
        MoveRepeatType.CANNOT_REPEAT,
        weight=lambda: TWO_TAILED_RAT_ATTACK_WEIGHT_WHEN_SUMMONABLE if can_summon() else TWO_TAILED_RAT_SCREECH_WEIGHT,
    )
    rand.add_branch(
        TWO_TAILED_RAT_CALL_FOR_BACKUP_MOVE,
        MoveRepeatType.USE_ONLY_ONCE,
        weight=lambda: TWO_TAILED_RAT_CALL_FOR_BACKUP_WEIGHT if can_summon() else 0.0,
    )

    states: dict[str, MonsterState] = {
        TWO_TAILED_RAT_RANDOM_STATE: rand,
        TWO_TAILED_RAT_SCRATCH_MOVE: MoveState(
            TWO_TAILED_RAT_SCRATCH_MOVE,
            scratch,
            [attack_intent(scratch_intent_damage)],
            follow_up_id=TWO_TAILED_RAT_RANDOM_STATE,
        ),
        TWO_TAILED_RAT_DISEASE_BITE_MOVE: MoveState(
            TWO_TAILED_RAT_DISEASE_BITE_MOVE,
            disease_bite,
            [attack_intent(disease_bite_intent_damage)],
            follow_up_id=TWO_TAILED_RAT_RANDOM_STATE,
        ),
        TWO_TAILED_RAT_SCREECH_MOVE: MoveState(
            TWO_TAILED_RAT_SCREECH_MOVE,
            screech,
            [debuff_intent()],
            follow_up_id=TWO_TAILED_RAT_RANDOM_STATE,
        ),
        TWO_TAILED_RAT_CALL_FOR_BACKUP_MOVE: MoveState(
            TWO_TAILED_RAT_CALL_FOR_BACKUP_MOVE,
            call_for_backup,
            [Intent(IntentType.SUMMON)],
            follow_up_id=TWO_TAILED_RAT_RANDOM_STATE,
        ),
    }

    starter_map = {
        0: TWO_TAILED_RAT_SCRATCH_MOVE,
        1: TWO_TAILED_RAT_DISEASE_BITE_MOVE,
        2: TWO_TAILED_RAT_SCREECH_MOVE,
    }
    initial = starter_map.get(starter_move_idx, TWO_TAILED_RAT_RANDOM_STATE)
    ai = MonsterAI(states, initial, rng)
    ai._two_tailed_rat_state = state  # noqa: SLF001
    return creature, ai


# ========================================================================
# ELITE ENCOUNTERS
# ========================================================================

# ---- PhantasmalGardener (HP 28-32 / 29-33 asc) ----

PHANTASMAL_GARDENER_MONSTER_ID = "PHANTASMAL_GARDENER"
PHANTASMAL_GARDENER_BASE_MIN_HP = 26
PHANTASMAL_GARDENER_BASE_MAX_HP = 31
PHANTASMAL_GARDENER_TOUGH_MIN_HP = 27
PHANTASMAL_GARDENER_TOUGH_MAX_HP = 32
PHANTASMAL_GARDENER_BITE_DAMAGE = 5
PHANTASMAL_GARDENER_LASH_DAMAGE = 7
PHANTASMAL_GARDENER_FLAIL_DAMAGE = 1
PHANTASMAL_GARDENER_FLAIL_REPEAT = 3
PHANTASMAL_GARDENER_BASE_ENLARGE_STRENGTH = 2
PHANTASMAL_GARDENER_DEADLY_ENLARGE_STRENGTH = 3
PHANTASMAL_GARDENER_BASE_SKITTISH = 6
PHANTASMAL_GARDENER_TOUGH_SKITTISH = 7
PHANTASMAL_GARDENER_INIT_MOVE = "INIT_MOVE"
PHANTASMAL_GARDENER_BITE_MOVE = "BITE_MOVE"
PHANTASMAL_GARDENER_LASH_MOVE = "LASH_MOVE"
PHANTASMAL_GARDENER_FLAIL_MOVE = "FLAIL_MOVE"
PHANTASMAL_GARDENER_ENLARGE_MOVE = "ENLARGE_MOVE"


def create_phantasmal_gardener(
    rng: Rng,
    slot: str = "first",
    ascension_level: int = 0,
) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        PHANTASMAL_GARDENER_TOUGH_MIN_HP,
        PHANTASMAL_GARDENER_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        PHANTASMAL_GARDENER_TOUGH_MAX_HP,
        PHANTASMAL_GARDENER_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id=PHANTASMAL_GARDENER_MONSTER_ID)

    def bite(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, PHANTASMAL_GARDENER_BITE_DAMAGE)

    def lash(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, PHANTASMAL_GARDENER_LASH_DAMAGE)

    def flail(combat: CombatState) -> None:
        _deal_damage_to_player(
            combat,
            creature,
            PHANTASMAL_GARDENER_FLAIL_DAMAGE,
            hits=PHANTASMAL_GARDENER_FLAIL_REPEAT,
        )

    def enlarge(combat: CombatState) -> None:
        enlarge_strength = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            PHANTASMAL_GARDENER_DEADLY_ENLARGE_STRENGTH,
            PHANTASMAL_GARDENER_BASE_ENLARGE_STRENGTH,
        )
        creature.apply_power(PowerId.STRENGTH, enlarge_strength, applier=creature)

    init = ConditionalBranchState(PHANTASMAL_GARDENER_INIT_MOVE)
    init.add_branch(lambda: slot == "first", PHANTASMAL_GARDENER_FLAIL_MOVE)
    init.add_branch(lambda: slot == "second", PHANTASMAL_GARDENER_BITE_MOVE)
    init.add_branch(lambda: slot == "third", PHANTASMAL_GARDENER_LASH_MOVE)
    init.add_branch(lambda: slot == "fourth", PHANTASMAL_GARDENER_ENLARGE_MOVE)

    states: dict[str, MonsterState] = {
        PHANTASMAL_GARDENER_INIT_MOVE: init,
        PHANTASMAL_GARDENER_BITE_MOVE: MoveState(
            PHANTASMAL_GARDENER_BITE_MOVE,
            bite,
            [attack_intent(PHANTASMAL_GARDENER_BITE_DAMAGE)],
            follow_up_id=PHANTASMAL_GARDENER_LASH_MOVE,
        ),
        PHANTASMAL_GARDENER_LASH_MOVE: MoveState(
            PHANTASMAL_GARDENER_LASH_MOVE,
            lash,
            [attack_intent(PHANTASMAL_GARDENER_LASH_DAMAGE)],
            follow_up_id=PHANTASMAL_GARDENER_FLAIL_MOVE,
        ),
        PHANTASMAL_GARDENER_FLAIL_MOVE: MoveState(
            PHANTASMAL_GARDENER_FLAIL_MOVE,
            flail,
            [multi_attack_intent(PHANTASMAL_GARDENER_FLAIL_DAMAGE, PHANTASMAL_GARDENER_FLAIL_REPEAT)],
            follow_up_id=PHANTASMAL_GARDENER_ENLARGE_MOVE,
        ),
        PHANTASMAL_GARDENER_ENLARGE_MOVE: MoveState(
            PHANTASMAL_GARDENER_ENLARGE_MOVE,
            enlarge,
            [buff_intent()],
            follow_up_id=PHANTASMAL_GARDENER_BITE_MOVE,
        ),
    }
    skittish = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        PHANTASMAL_GARDENER_TOUGH_SKITTISH,
        PHANTASMAL_GARDENER_BASE_SKITTISH,
    )
    creature.apply_power(PowerId.SKITTISH, skittish)
    return creature, MonsterAI(states, PHANTASMAL_GARDENER_INIT_MOVE, rng)


# ---- SkulkingColony (HP 79 / 84 asc) ----

SKULKING_COLONY_MONSTER_ID = "SKULKING_COLONY"
SKULKING_COLONY_BASE_HP = 75
SKULKING_COLONY_TOUGH_HP = 80
SKULKING_COLONY_BASE_SUPER_CRAB_DAMAGE = 6
SKULKING_COLONY_DEADLY_SUPER_CRAB_DAMAGE = 7
SKULKING_COLONY_SUPER_CRAB_REPEAT = 2
SKULKING_COLONY_BASE_ZOOM_DAMAGE = 14
SKULKING_COLONY_DEADLY_ZOOM_DAMAGE = 16
SKULKING_COLONY_BASE_SMASH_DAMAGE = 9
SKULKING_COLONY_DEADLY_SMASH_DAMAGE = 11
SKULKING_COLONY_SMASH_DAZED = 4
SKULKING_COLONY_BASE_INERTIA_BLOCK = 10
SKULKING_COLONY_TOUGH_INERTIA_BLOCK = 13
SKULKING_COLONY_HARDENED_SHELL = 20
SKULKING_COLONY_INERTIA_STRENGTH = 3
SKULKING_COLONY_INERTIA_MOVE = "INERTIA_MOVE"
SKULKING_COLONY_ZOOM_MOVE = "ZOOM_MOVE"
SKULKING_COLONY_SUPER_CRAB_MOVE = "SUPER_CRAB_MOVE"
SKULKING_COLONY_SMASH_MOVE = "SMASH_MOVE"


def create_skulking_colony(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        SKULKING_COLONY_TOUGH_HP,
        SKULKING_COLONY_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=SKULKING_COLONY_MONSTER_ID)

    def inertia(combat: CombatState) -> None:
        inertia_block = _ascension_value(
            _combat_ascension_level(combat),
            TOUGH_ENEMIES_ASCENSION_LEVEL,
            SKULKING_COLONY_TOUGH_INERTIA_BLOCK,
            SKULKING_COLONY_BASE_INERTIA_BLOCK,
        )
        _gain_block(creature, inertia_block, combat)
        creature.apply_power(PowerId.STRENGTH, SKULKING_COLONY_INERTIA_STRENGTH, applier=creature)

    def zoom(combat: CombatState) -> None:
        zoom_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            SKULKING_COLONY_DEADLY_ZOOM_DAMAGE,
            SKULKING_COLONY_BASE_ZOOM_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, zoom_dmg)

    def super_crab(combat: CombatState) -> None:
        super_crab_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            SKULKING_COLONY_DEADLY_SUPER_CRAB_DAMAGE,
            SKULKING_COLONY_BASE_SUPER_CRAB_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, super_crab_dmg, hits=SKULKING_COLONY_SUPER_CRAB_REPEAT)

    def smash(combat: CombatState) -> None:
        smash_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            SKULKING_COLONY_DEADLY_SMASH_DAMAGE,
            SKULKING_COLONY_BASE_SMASH_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, smash_dmg)
        if combat.is_over:
            return
        add_generated_cards_to_living_player_discards(combat, make_dazed, SKULKING_COLONY_SMASH_DAZED)

    super_crab_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        SKULKING_COLONY_DEADLY_SUPER_CRAB_DAMAGE,
        SKULKING_COLONY_BASE_SUPER_CRAB_DAMAGE,
    )
    zoom_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        SKULKING_COLONY_DEADLY_ZOOM_DAMAGE,
        SKULKING_COLONY_BASE_ZOOM_DAMAGE,
    )
    smash_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        SKULKING_COLONY_DEADLY_SMASH_DAMAGE,
        SKULKING_COLONY_BASE_SMASH_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        SKULKING_COLONY_INERTIA_MOVE: MoveState(
            SKULKING_COLONY_INERTIA_MOVE,
            inertia,
            [defend_intent(), buff_intent()],
            follow_up_id=SKULKING_COLONY_SUPER_CRAB_MOVE,
        ),
        SKULKING_COLONY_ZOOM_MOVE: MoveState(
            SKULKING_COLONY_ZOOM_MOVE,
            zoom,
            [attack_intent(zoom_intent_damage)],
            follow_up_id=SKULKING_COLONY_INERTIA_MOVE,
        ),
        SKULKING_COLONY_SUPER_CRAB_MOVE: MoveState(
            SKULKING_COLONY_SUPER_CRAB_MOVE,
            super_crab,
            [multi_attack_intent(super_crab_intent_damage, SKULKING_COLONY_SUPER_CRAB_REPEAT)],
            follow_up_id=SKULKING_COLONY_SMASH_MOVE,
        ),
        SKULKING_COLONY_SMASH_MOVE: MoveState(
            SKULKING_COLONY_SMASH_MOVE,
            smash,
            [attack_intent(smash_intent_damage), status_intent()],
            follow_up_id=SKULKING_COLONY_ZOOM_MOVE,
        ),
    }
    creature.apply_power(PowerId.HARDENED_SHELL, SKULKING_COLONY_HARDENED_SHELL)
    return creature, MonsterAI(states, SKULKING_COLONY_SMASH_MOVE)


# ---- TerrorEel (HP 140 / 150 asc) ----

TERROR_EEL_MONSTER_ID = "TERROR_EEL"
TERROR_EEL_BASE_HP = 140
TERROR_EEL_TOUGH_HP = 150
TERROR_EEL_BASE_SHRIEK = 70
TERROR_EEL_TOUGH_SHRIEK = 75
TERROR_EEL_BASE_CRASH_DAMAGE = 16
TERROR_EEL_DEADLY_CRASH_DAMAGE = 18
TERROR_EEL_BASE_THRASH_DAMAGE = 3
TERROR_EEL_DEADLY_THRASH_DAMAGE = 4
TERROR_EEL_THRASH_REPEAT = 3
TERROR_EEL_TERROR_VULNERABLE = 99
TERROR_EEL_THRASH_VIGOR = 7
TERROR_EEL_CRASH_MOVE = "CRASH_MOVE"
TERROR_EEL_THRASH_MOVE = "ThrashMove"
TERROR_EEL_STUN_MOVE = "STUN_MOVE"
TERROR_EEL_TERROR_MOVE = "TERROR_MOVE"


def create_terror_eel(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        TERROR_EEL_TOUGH_HP,
        TERROR_EEL_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=TERROR_EEL_MONSTER_ID)

    def crash(combat: CombatState) -> None:
        crash_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            TERROR_EEL_DEADLY_CRASH_DAMAGE,
            TERROR_EEL_BASE_CRASH_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, crash_dmg)

    def thrash(combat: CombatState) -> None:
        thrash_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            TERROR_EEL_DEADLY_THRASH_DAMAGE,
            TERROR_EEL_BASE_THRASH_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, thrash_dmg, hits=TERROR_EEL_THRASH_REPEAT)
        combat.apply_power_to(creature, PowerId.VIGOR, TERROR_EEL_THRASH_VIGOR, applier=creature)

    def stun(combat: CombatState) -> None:
        pass

    def terror(combat: CombatState) -> None:
        apply_power_to_living_player_targets(
            combat,
            PowerId.VULNERABLE,
            TERROR_EEL_TERROR_VULNERABLE,
            applier=creature,
        )

    crash_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        TERROR_EEL_DEADLY_CRASH_DAMAGE,
        TERROR_EEL_BASE_CRASH_DAMAGE,
    )
    thrash_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        TERROR_EEL_DEADLY_THRASH_DAMAGE,
        TERROR_EEL_BASE_THRASH_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        TERROR_EEL_CRASH_MOVE: MoveState(
            TERROR_EEL_CRASH_MOVE,
            crash,
            [attack_intent(crash_intent_damage)],
            follow_up_id=TERROR_EEL_THRASH_MOVE,
        ),
        TERROR_EEL_THRASH_MOVE: MoveState(
            TERROR_EEL_THRASH_MOVE,
            thrash,
            [multi_attack_intent(thrash_intent_damage, TERROR_EEL_THRASH_REPEAT), buff_intent()],
            follow_up_id=TERROR_EEL_CRASH_MOVE,
        ),
        TERROR_EEL_STUN_MOVE: MoveState(
            TERROR_EEL_STUN_MOVE,
            stun,
            [Intent(IntentType.STUN)],
            follow_up_id=TERROR_EEL_TERROR_MOVE,
        ),
        TERROR_EEL_TERROR_MOVE: MoveState(
            TERROR_EEL_TERROR_MOVE,
            terror,
            [debuff_intent()],
            follow_up_id=TERROR_EEL_CRASH_MOVE,
        ),
    }
    shriek = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        TERROR_EEL_TOUGH_SHRIEK,
        TERROR_EEL_BASE_SHRIEK,
    )
    creature.apply_power(PowerId.SHRIEK, shriek)
    return creature, MonsterAI(states, TERROR_EEL_CRASH_MOVE)


# ========================================================================
# BOSS ENCOUNTERS
# ========================================================================

# ---- WaterfallGiant ----

WATERFALL_GIANT_MONSTER_ID = "WATERFALL_GIANT"
WATERFALL_GIANT_BASE_HP = 240
WATERFALL_GIANT_TOUGH_HP = 250
WATERFALL_GIANT_BASE_PRESSURIZE = 15
WATERFALL_GIANT_DEADLY_PRESSURIZE = 20
WATERFALL_GIANT_BASE_STOMP_DAMAGE = 15
WATERFALL_GIANT_DEADLY_STOMP_DAMAGE = 16
WATERFALL_GIANT_STOMP_WEAK = 1
WATERFALL_GIANT_BASE_RAM_DAMAGE = 10
WATERFALL_GIANT_DEADLY_RAM_DAMAGE = 11
WATERFALL_GIANT_BASE_PRESSURE_UP_DAMAGE = 13
WATERFALL_GIANT_DEADLY_PRESSURE_UP_DAMAGE = 14
WATERFALL_GIANT_BASE_PRESSURE_GUN_DAMAGE = 20
WATERFALL_GIANT_DEADLY_PRESSURE_GUN_DAMAGE = 23
WATERFALL_GIANT_PRESSURE_GUN_INCREASE = 5
WATERFALL_GIANT_PRESSURE_BUILDUP = 3
WATERFALL_GIANT_BASE_SIPHON_HEAL = 10
WATERFALL_GIANT_TOUGH_SIPHON_HEAL = 15
WATERFALL_GIANT_CURRENT_PRESSURE_GUN_DAMAGE_KEY = "current_pressure_gun_damage"
WATERFALL_GIANT_STEAM_ERUPTION_DAMAGE_KEY = "steam_eruption_damage"
WATERFALL_GIANT_ABOUT_TO_BLOW_HP = 999_999_999
WATERFALL_GIANT_PRESSURIZE_MOVE = "PRESSURIZE_MOVE"
WATERFALL_GIANT_STOMP_MOVE = "STOMP_MOVE"
WATERFALL_GIANT_RAM_MOVE = "RAM_MOVE"
WATERFALL_GIANT_SIPHON_MOVE = "SIPHON_MOVE"
WATERFALL_GIANT_PRESSURE_GUN_MOVE = "PRESSURE_GUN_MOVE"
WATERFALL_GIANT_PRESSURE_UP_MOVE = "PRESSURE_UP_MOVE"
WATERFALL_GIANT_ABOUT_TO_BLOW_MOVE = "ABOUT_TO_BLOW_MOVE"
WATERFALL_GIANT_EXPLODE_MOVE = "EXPLODE_MOVE"


def create_waterfall_giant(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        WATERFALL_GIANT_TOUGH_HP,
        WATERFALL_GIANT_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=WATERFALL_GIANT_MONSTER_ID)

    base_pressure_gun_dmg = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        WATERFALL_GIANT_DEADLY_PRESSURE_GUN_DAMAGE,
        WATERFALL_GIANT_BASE_PRESSURE_GUN_DAMAGE,
    )
    _state = {
        WATERFALL_GIANT_CURRENT_PRESSURE_GUN_DAMAGE_KEY: base_pressure_gun_dmg,
        WATERFALL_GIANT_STEAM_ERUPTION_DAMAGE_KEY: 0,
    }

    def _gain_pressure(combat: CombatState, amount: int) -> None:
        combat.apply_power_to(creature, PowerId.STEAM_ERUPTION, amount, applier=creature)

    def pressurize(combat: CombatState) -> None:
        pressurize_amount = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            WATERFALL_GIANT_DEADLY_PRESSURIZE,
            WATERFALL_GIANT_BASE_PRESSURIZE,
        )
        _gain_pressure(combat, pressurize_amount)

    def stomp(combat: CombatState) -> None:
        stomp_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            WATERFALL_GIANT_DEADLY_STOMP_DAMAGE,
            WATERFALL_GIANT_BASE_STOMP_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, stomp_dmg)
        apply_power_to_living_player_targets(combat, PowerId.WEAK, WATERFALL_GIANT_STOMP_WEAK, applier=creature)
        _gain_pressure(combat, WATERFALL_GIANT_PRESSURE_BUILDUP)

    def ram(combat: CombatState) -> None:
        ram_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            WATERFALL_GIANT_DEADLY_RAM_DAMAGE,
            WATERFALL_GIANT_BASE_RAM_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, ram_dmg)
        _gain_pressure(combat, WATERFALL_GIANT_PRESSURE_BUILDUP)

    def siphon(combat: CombatState) -> None:
        siphon_heal = _ascension_value(
            _combat_ascension_level(combat),
            TOUGH_ENEMIES_ASCENSION_LEVEL,
            WATERFALL_GIANT_TOUGH_SIPHON_HEAL,
            WATERFALL_GIANT_BASE_SIPHON_HEAL,
        )
        creature.heal(siphon_heal * len(combat.combat_player_states))
        _gain_pressure(combat, WATERFALL_GIANT_PRESSURE_BUILDUP)

    def pressure_gun(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, _state[WATERFALL_GIANT_CURRENT_PRESSURE_GUN_DAMAGE_KEY])
        _state[WATERFALL_GIANT_CURRENT_PRESSURE_GUN_DAMAGE_KEY] += WATERFALL_GIANT_PRESSURE_GUN_INCREASE
        _gain_pressure(combat, WATERFALL_GIANT_PRESSURE_BUILDUP)

    def pressure_up(combat: CombatState) -> None:
        pressure_up_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            WATERFALL_GIANT_DEADLY_PRESSURE_UP_DAMAGE,
            WATERFALL_GIANT_BASE_PRESSURE_UP_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, pressure_up_dmg)
        _gain_pressure(combat, WATERFALL_GIANT_PRESSURE_BUILDUP)

    def about_to_blow(combat: CombatState) -> None:
        _state[WATERFALL_GIANT_STEAM_ERUPTION_DAMAGE_KEY] = creature.get_power_amount(PowerId.STEAM_ERUPTION)
        creature.powers.pop(PowerId.STEAM_ERUPTION, None)

    def explode(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, _state[WATERFALL_GIANT_STEAM_ERUPTION_DAMAGE_KEY])
        combat.kill_creature(creature)

    stomp_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        WATERFALL_GIANT_DEADLY_STOMP_DAMAGE,
        WATERFALL_GIANT_BASE_STOMP_DAMAGE,
    )
    ram_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        WATERFALL_GIANT_DEADLY_RAM_DAMAGE,
        WATERFALL_GIANT_BASE_RAM_DAMAGE,
    )
    pressure_up_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        WATERFALL_GIANT_DEADLY_PRESSURE_UP_DAMAGE,
        WATERFALL_GIANT_BASE_PRESSURE_UP_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        WATERFALL_GIANT_PRESSURIZE_MOVE: MoveState(
            WATERFALL_GIANT_PRESSURIZE_MOVE,
            pressurize,
            [buff_intent()],
            follow_up_id=WATERFALL_GIANT_STOMP_MOVE,
        ),
        WATERFALL_GIANT_STOMP_MOVE: MoveState(
            WATERFALL_GIANT_STOMP_MOVE,
            stomp,
            [attack_intent(stomp_intent_damage), debuff_intent(), buff_intent()],
            follow_up_id=WATERFALL_GIANT_RAM_MOVE,
        ),
        WATERFALL_GIANT_RAM_MOVE: MoveState(
            WATERFALL_GIANT_RAM_MOVE,
            ram,
            [attack_intent(ram_intent_damage), buff_intent()],
            follow_up_id=WATERFALL_GIANT_SIPHON_MOVE,
        ),
        WATERFALL_GIANT_SIPHON_MOVE: MoveState(
            WATERFALL_GIANT_SIPHON_MOVE,
            siphon,
            [Intent(IntentType.HEAL), buff_intent()],
            follow_up_id=WATERFALL_GIANT_PRESSURE_GUN_MOVE,
        ),
        WATERFALL_GIANT_PRESSURE_GUN_MOVE: MoveState(
            WATERFALL_GIANT_PRESSURE_GUN_MOVE,
            pressure_gun,
            [attack_intent(base_pressure_gun_dmg), buff_intent()],
            follow_up_id=WATERFALL_GIANT_PRESSURE_UP_MOVE,
        ),
        WATERFALL_GIANT_PRESSURE_UP_MOVE: MoveState(
            WATERFALL_GIANT_PRESSURE_UP_MOVE,
            pressure_up,
            [attack_intent(pressure_up_intent_damage), buff_intent()],
            follow_up_id=WATERFALL_GIANT_STOMP_MOVE,
        ),
        WATERFALL_GIANT_ABOUT_TO_BLOW_MOVE: MoveState(
            WATERFALL_GIANT_ABOUT_TO_BLOW_MOVE,
            about_to_blow,
            [Intent(IntentType.STUN)],
            follow_up_id=WATERFALL_GIANT_EXPLODE_MOVE,
            must_perform_once=True,
        ),
        WATERFALL_GIANT_EXPLODE_MOVE: MoveState(
            WATERFALL_GIANT_EXPLODE_MOVE,
            explode,
            [Intent(IntentType.DEATH_BLOW)],
            follow_up_id=WATERFALL_GIANT_EXPLODE_MOVE,
        ),
    }
    return creature, MonsterAI(states, WATERFALL_GIANT_PRESSURIZE_MOVE)


# ---- SoulFysh ----

SOUL_FYSH_MONSTER_ID = "SOUL_FYSH"
SOUL_FYSH_BASE_HP = 211
SOUL_FYSH_TOUGH_HP = 221
SOUL_FYSH_BASE_DE_GAS_DAMAGE = 16
SOUL_FYSH_DEADLY_DE_GAS_DAMAGE = 17
SOUL_FYSH_BASE_SCREAM_DAMAGE = 13
SOUL_FYSH_DEADLY_SCREAM_DAMAGE = 15
SOUL_FYSH_BASE_GAZE_DAMAGE = 7
SOUL_FYSH_DEADLY_GAZE_DAMAGE = 8
SOUL_FYSH_BECKON_STATUS_COUNT = 2
SOUL_FYSH_GAZE_STATUS_COUNT = 1
SOUL_FYSH_FADE_INTANGIBLE = 2
SOUL_FYSH_SCREAM_VULNERABLE = 3
SOUL_FYSH_BECKON_MOVE = "BECKON_MOVE"
SOUL_FYSH_DE_GAS_MOVE = "DE_GAS_MOVE"
SOUL_FYSH_GAZE_MOVE = "GAZE_MOVE"
SOUL_FYSH_FADE_MOVE = "FADE_MOVE"
SOUL_FYSH_SCREAM_MOVE = "SCREAM_MOVE"


def create_soul_fysh(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        SOUL_FYSH_TOUGH_HP,
        SOUL_FYSH_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=SOUL_FYSH_MONSTER_ID)

    def beckon(combat: CombatState) -> None:
        from sts2_env.cards.status import make_beckon

        for target in living_player_targets(combat):
            combat.add_generated_card_to_creature_draw_pile(
                target,
                make_beckon(),
                added_by_player=False,
                random_position=True,
            )
            combat.add_generated_card_to_creature_discard(target, make_beckon(), added_by_player=False)

    def de_gas(combat: CombatState) -> None:
        de_gas_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            SOUL_FYSH_DEADLY_DE_GAS_DAMAGE,
            SOUL_FYSH_BASE_DE_GAS_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, de_gas_dmg)

    def gaze(combat: CombatState) -> None:
        from sts2_env.cards.status import make_beckon

        gaze_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            SOUL_FYSH_DEADLY_GAZE_DAMAGE,
            SOUL_FYSH_BASE_GAZE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, gaze_dmg)
        if combat.is_over:
            return
        for target in living_player_targets(combat):
            combat.add_generated_card_to_creature_discard(target, make_beckon(), added_by_player=False)

    def fade(combat: CombatState) -> None:
        creature.apply_power(PowerId.INTANGIBLE, SOUL_FYSH_FADE_INTANGIBLE, applier=creature)

    def scream(combat: CombatState) -> None:
        scream_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            SOUL_FYSH_DEADLY_SCREAM_DAMAGE,
            SOUL_FYSH_BASE_SCREAM_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, scream_dmg)
        for target in living_player_targets(combat):
            combat.apply_power_to(target, PowerId.VULNERABLE, SOUL_FYSH_SCREAM_VULNERABLE, applier=creature)

    de_gas_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        SOUL_FYSH_DEADLY_DE_GAS_DAMAGE,
        SOUL_FYSH_BASE_DE_GAS_DAMAGE,
    )
    gaze_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        SOUL_FYSH_DEADLY_GAZE_DAMAGE,
        SOUL_FYSH_BASE_GAZE_DAMAGE,
    )
    scream_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        SOUL_FYSH_DEADLY_SCREAM_DAMAGE,
        SOUL_FYSH_BASE_SCREAM_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        SOUL_FYSH_BECKON_MOVE: MoveState(
            SOUL_FYSH_BECKON_MOVE,
            beckon,
            [status_intent()],
            follow_up_id=SOUL_FYSH_DE_GAS_MOVE,
        ),
        SOUL_FYSH_DE_GAS_MOVE: MoveState(
            SOUL_FYSH_DE_GAS_MOVE,
            de_gas,
            [attack_intent(de_gas_intent_damage)],
            follow_up_id=SOUL_FYSH_GAZE_MOVE,
        ),
        SOUL_FYSH_GAZE_MOVE: MoveState(
            SOUL_FYSH_GAZE_MOVE,
            gaze,
            [attack_intent(gaze_intent_damage), status_intent()],
            follow_up_id=SOUL_FYSH_FADE_MOVE,
        ),
        SOUL_FYSH_FADE_MOVE: MoveState(
            SOUL_FYSH_FADE_MOVE,
            fade,
            [buff_intent()],
            follow_up_id=SOUL_FYSH_SCREAM_MOVE,
        ),
        SOUL_FYSH_SCREAM_MOVE: MoveState(
            SOUL_FYSH_SCREAM_MOVE,
            scream,
            [attack_intent(scream_intent_damage), debuff_intent()],
            follow_up_id=SOUL_FYSH_BECKON_MOVE,
        ),
    }
    return creature, MonsterAI(states, SOUL_FYSH_BECKON_MOVE)


# ---- LagavulinMatriarch ----
# C# cycle: SLEEP -> branch -> SLASH -> DISEMBOWEL -> SLASH2 -> SOUL_SIPHON -> SLASH...

LAGAVULIN_MATRIARCH_MONSTER_ID = "LAGAVULIN_MATRIARCH"
LAGAVULIN_MATRIARCH_BASE_HP = 222
LAGAVULIN_MATRIARCH_TOUGH_HP = 233
LAGAVULIN_MATRIARCH_BASE_SLASH_DAMAGE = 19
LAGAVULIN_MATRIARCH_DEADLY_SLASH_DAMAGE = 21
LAGAVULIN_MATRIARCH_BASE_SLASH2_DAMAGE = 12
LAGAVULIN_MATRIARCH_DEADLY_SLASH2_DAMAGE = 14
LAGAVULIN_MATRIARCH_BASE_SLASH2_BLOCK = 12
LAGAVULIN_MATRIARCH_TOUGH_SLASH2_BLOCK = 14
LAGAVULIN_MATRIARCH_BASE_DISEMBOWEL_DAMAGE = 9
LAGAVULIN_MATRIARCH_DEADLY_DISEMBOWEL_DAMAGE = 10
LAGAVULIN_MATRIARCH_DISEMBOWEL_REPEAT = 2
LAGAVULIN_MATRIARCH_PLATING = 12
LAGAVULIN_MATRIARCH_ASLEEP = 3
LAGAVULIN_MATRIARCH_SOUL_SIPHON_DEBUFF = -2
LAGAVULIN_MATRIARCH_SOUL_SIPHON_STRENGTH = 2
LAGAVULIN_MATRIARCH_SLEEP_MOVE = "SLEEP_MOVE"
LAGAVULIN_MATRIARCH_SLEEP_BRANCH = "SLEEP_BRANCH"
LAGAVULIN_MATRIARCH_SLASH_MOVE = "SLASH_MOVE"
LAGAVULIN_MATRIARCH_DISEMBOWEL_MOVE = "DISEMBOWEL_MOVE"
LAGAVULIN_MATRIARCH_SLASH2_MOVE = "SLASH2_MOVE"
LAGAVULIN_MATRIARCH_SOUL_SIPHON_MOVE = "SOUL_SIPHON_MOVE"


def create_lagavulin_matriarch(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        LAGAVULIN_MATRIARCH_TOUGH_HP,
        LAGAVULIN_MATRIARCH_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=LAGAVULIN_MATRIARCH_MONSTER_ID)

    def sleep_move(combat: CombatState) -> None:
        pass

    def slash(combat: CombatState) -> None:
        slash_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            LAGAVULIN_MATRIARCH_DEADLY_SLASH_DAMAGE,
            LAGAVULIN_MATRIARCH_BASE_SLASH_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, slash_dmg)

    def disembowel(combat: CombatState) -> None:
        disembowel_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            LAGAVULIN_MATRIARCH_DEADLY_DISEMBOWEL_DAMAGE,
            LAGAVULIN_MATRIARCH_BASE_DISEMBOWEL_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, disembowel_dmg, hits=LAGAVULIN_MATRIARCH_DISEMBOWEL_REPEAT)

    def slash2(combat: CombatState) -> None:
        slash2_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            LAGAVULIN_MATRIARCH_DEADLY_SLASH2_DAMAGE,
            LAGAVULIN_MATRIARCH_BASE_SLASH2_DAMAGE,
        )
        slash2_block = _ascension_value(
            _combat_ascension_level(combat),
            TOUGH_ENEMIES_ASCENSION_LEVEL,
            LAGAVULIN_MATRIARCH_TOUGH_SLASH2_BLOCK,
            LAGAVULIN_MATRIARCH_BASE_SLASH2_BLOCK,
        )
        _deal_damage_to_player(combat, creature, slash2_dmg)
        _gain_block(creature, slash2_block, combat)

    def soul_siphon(combat: CombatState) -> None:
        apply_power_to_living_player_targets(
            combat,
            PowerId.STRENGTH,
            LAGAVULIN_MATRIARCH_SOUL_SIPHON_DEBUFF,
            applier=creature,
        )
        apply_power_to_living_player_targets(
            combat,
            PowerId.DEXTERITY,
            LAGAVULIN_MATRIARCH_SOUL_SIPHON_DEBUFF,
            applier=creature,
        )
        creature.apply_power(PowerId.STRENGTH, LAGAVULIN_MATRIARCH_SOUL_SIPHON_STRENGTH, applier=creature)

    slash_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        LAGAVULIN_MATRIARCH_DEADLY_SLASH_DAMAGE,
        LAGAVULIN_MATRIARCH_BASE_SLASH_DAMAGE,
    )
    disembowel_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        LAGAVULIN_MATRIARCH_DEADLY_DISEMBOWEL_DAMAGE,
        LAGAVULIN_MATRIARCH_BASE_DISEMBOWEL_DAMAGE,
    )
    slash2_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        LAGAVULIN_MATRIARCH_DEADLY_SLASH2_DAMAGE,
        LAGAVULIN_MATRIARCH_BASE_SLASH2_DAMAGE,
    )

    sleep_branch = ConditionalBranchState(LAGAVULIN_MATRIARCH_SLEEP_BRANCH)
    sleep_branch.add_branch(lambda: creature.has_power(PowerId.ASLEEP), LAGAVULIN_MATRIARCH_SLEEP_MOVE)
    sleep_branch.add_branch(lambda: True, LAGAVULIN_MATRIARCH_SLASH_MOVE)

    states: dict[str, MonsterState] = {
        LAGAVULIN_MATRIARCH_SLEEP_MOVE: MoveState(
            LAGAVULIN_MATRIARCH_SLEEP_MOVE,
            sleep_move,
            [sleep_intent()],
            follow_up_id=LAGAVULIN_MATRIARCH_SLEEP_BRANCH,
        ),
        LAGAVULIN_MATRIARCH_SLEEP_BRANCH: sleep_branch,
        LAGAVULIN_MATRIARCH_SLASH_MOVE: MoveState(
            LAGAVULIN_MATRIARCH_SLASH_MOVE,
            slash,
            [attack_intent(slash_intent_damage)],
            follow_up_id=LAGAVULIN_MATRIARCH_DISEMBOWEL_MOVE,
        ),
        LAGAVULIN_MATRIARCH_DISEMBOWEL_MOVE: MoveState(
            LAGAVULIN_MATRIARCH_DISEMBOWEL_MOVE,
            disembowel,
            [multi_attack_intent(disembowel_intent_damage, LAGAVULIN_MATRIARCH_DISEMBOWEL_REPEAT)],
            follow_up_id=LAGAVULIN_MATRIARCH_SLASH2_MOVE,
        ),
        LAGAVULIN_MATRIARCH_SLASH2_MOVE: MoveState(
            LAGAVULIN_MATRIARCH_SLASH2_MOVE,
            slash2,
            [attack_intent(slash2_intent_damage), defend_intent()],
            follow_up_id=LAGAVULIN_MATRIARCH_SOUL_SIPHON_MOVE,
        ),
        LAGAVULIN_MATRIARCH_SOUL_SIPHON_MOVE: MoveState(
            LAGAVULIN_MATRIARCH_SOUL_SIPHON_MOVE,
            soul_siphon,
            [debuff_intent(), buff_intent()],
            follow_up_id=LAGAVULIN_MATRIARCH_SLASH_MOVE,
        ),
    }

    creature.apply_power(PowerId.PLATING, LAGAVULIN_MATRIARCH_PLATING)
    creature.apply_power(PowerId.ASLEEP, LAGAVULIN_MATRIARCH_ASLEEP)
    return creature, MonsterAI(states, LAGAVULIN_MATRIARCH_SLEEP_MOVE)


# ---- Aeonglass (HP 512 / 535 asc) ----
# C# ref: decompiled/MegaCrit.Sts2.Core.Models.Monsters/Aeonglass.cs (v0.107.1).
# Queen-tier act 4 boss (CustomBgm "act3_boss_queen", music param "queen_progress").
# Fixed HP: MaxInitialHp => MinInitialHp.
# AfterAddedToRoom: WitheringPresencePower (Amount 6) targeting each opposing
# player (the sim tracks it as a single instance on the Aeonglass creature with
# one independent CardsLeft counter per opposing player — see
# WitheringPresencePower docstring) plus Artifact(3) on itself.
# Move cycle: EBB -> EYE_LASERS -> INCREASING_INTENSITY -> EBB ...
# INCREASING_INTENSITY fake-upgrades every Wither in the players' combat card
# lists (C# loops PlayerCombatState.AllCards calling Wither.FakeUpgrade, then
# increments WitherUpgradeCount, so newly generated Withers are matched to the
# same count via Aeonglass.AfterCardGeneratedForCombat /
# MatchWitherToUpgradeCount).

AEONGLASS_MONSTER_ID = "AEONGLASS"
AEONGLASS_BASE_HP = 512
AEONGLASS_TOUGH_HP = 535
AEONGLASS_BASE_EBB_DAMAGE = 26
AEONGLASS_DEADLY_EBB_DAMAGE = 32
AEONGLASS_EBB_BLOCK = 33
AEONGLASS_BASE_EYE_LASERS_DAMAGE = 11
AEONGLASS_DEADLY_EYE_LASERS_DAMAGE = 12
AEONGLASS_EYE_LASERS_REPEAT = 2
AEONGLASS_BASE_INTENSITY_STRENGTH = 3
AEONGLASS_DEADLY_INTENSITY_STRENGTH = 4
AEONGLASS_BASE_WITHER_AMOUNT = 1
AEONGLASS_DEADLY_WITHER_AMOUNT = 2
AEONGLASS_WITHERING_PRESENCE_AMOUNT = 6
AEONGLASS_ARTIFACT = 3
AEONGLASS_WITHER_UPGRADE_STEP = 3  # C# Wither.FakeUpgrade: Damage.UpgradeValueBy(3m)
AEONGLASS_WITHER_UPGRADE_COUNT_KEY = "wither_upgrade_count"
AEONGLASS_ADDITIONAL_STRENGTH_KEY = "additional_strength"
AEONGLASS_EBB_MOVE = "EBB_MOVE"
AEONGLASS_EYE_LASERS_MOVE = "EYE_LASERS_MOVE"
AEONGLASS_INCREASING_INTENSITY_MOVE = "INCREASING_INTENSITY_MOVE"


def _aeonglass_match_wither_to_upgrade_count(card: object, upgrade_count: int) -> None:
    """C# Aeonglass.MatchWitherToUpgradeCount: a Wither that received
    `upgrade_count` fake upgrades deals WITHER_DAMAGE + STEP * count damage."""
    card.effect_vars["damage"] = (  # type: ignore[union-attr]
        WITHER_DAMAGE + AEONGLASS_WITHER_UPGRADE_STEP * upgrade_count
    )


def _aeonglass_normalize_player_withers(combat: CombatState, upgrade_count: int) -> None:
    """Force every Wither in each player's combat card piles to the damage the
    C# model would have at `upgrade_count` fake upgrades (idempotent equivalent
    of looping AllCards and calling FakeUpgrade per count step)."""
    for state in combat.combat_player_states:
        for pile in state.all_piles:
            for card in pile:
                if card.card_id is CardId.WITHER:
                    _aeonglass_match_wither_to_upgrade_count(card, upgrade_count)


def _attach_aeonglass_wither_generation_matching(
    creature: Creature,
    ai_state: dict[str, int],
) -> None:
    """C# Aeonglass.AfterCardGeneratedForCombat fires MatchWitherToUpgradeCount
    for every Wither generated while Aeonglass is in combat. The sim dispatches
    generation through power listeners (fire_after_card_generated_for_combat),
    so attach the matching to the WitheringPresence instance this monster owns.
    """
    from sts2_env.powers.monster import WITHERING_PRESENCE_POWER_ID

    power = creature.powers.get(WITHERING_PRESENCE_POWER_ID)
    if power is None:
        return

    def after_card_generated_for_combat(
        owner: Creature,
        card: object,
        added_by_player: bool,
        combat: CombatState,
    ) -> None:
        if card is None or getattr(card, "card_id", None) is not CardId.WITHER:
            return
        _aeonglass_match_wither_to_upgrade_count(
            card,
            ai_state[AEONGLASS_WITHER_UPGRADE_COUNT_KEY],
        )

    power.after_card_generated_for_combat = after_card_generated_for_combat  # type: ignore[method-assign]


def create_aeonglass(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    from sts2_env.powers.monster import WITHERING_PRESENCE_POWER_ID

    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        AEONGLASS_TOUGH_HP,
        AEONGLASS_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=AEONGLASS_MONSTER_ID)
    ai_state = {
        AEONGLASS_WITHER_UPGRADE_COUNT_KEY: 0,
        AEONGLASS_ADDITIONAL_STRENGTH_KEY: 0,
    }

    def ebb(combat: CombatState) -> None:
        ebb_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            AEONGLASS_DEADLY_EBB_DAMAGE,
            AEONGLASS_BASE_EBB_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, ebb_dmg)
        if combat.is_over:
            return
        _gain_block(creature, AEONGLASS_EBB_BLOCK, combat)

    def eye_lasers(combat: CombatState) -> None:
        eye_lasers_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            AEONGLASS_DEADLY_EYE_LASERS_DAMAGE,
            AEONGLASS_BASE_EYE_LASERS_DAMAGE,
        )
        _deal_damage_to_player(
            combat,
            creature,
            eye_lasers_dmg,
            hits=AEONGLASS_EYE_LASERS_REPEAT,
        )

    def increasing_intensity(combat: CombatState) -> None:
        # C# order: FakeUpgrade every existing Wither, WitherUpgradeCount++,
        # add WitherAmount Withers to each target's discard, gain Strength,
        # AdditionalStrength++.
        new_count = ai_state[AEONGLASS_WITHER_UPGRADE_COUNT_KEY] + 1
        ai_state[AEONGLASS_WITHER_UPGRADE_COUNT_KEY] = new_count
        _aeonglass_normalize_player_withers(combat, new_count)
        wither_amount = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            AEONGLASS_DEADLY_WITHER_AMOUNT,
            AEONGLASS_BASE_WITHER_AMOUNT,
        )
        add_generated_cards_to_living_player_discards(combat, make_wither, wither_amount)
        # Re-normalize so the added Withers carry the new upgrade count even if
        # the generation listener is unavailable (e.g. power stripped).
        _aeonglass_normalize_player_withers(combat, new_count)
        intensity_strength = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            AEONGLASS_DEADLY_INTENSITY_STRENGTH,
            AEONGLASS_BASE_INTENSITY_STRENGTH,
        )
        combat.apply_power_to(
            creature,
            PowerId.STRENGTH,
            intensity_strength + ai_state[AEONGLASS_ADDITIONAL_STRENGTH_KEY],
            applier=creature,
        )
        ai_state[AEONGLASS_ADDITIONAL_STRENGTH_KEY] += 1

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
    initial_wither_amount = _ascension_value(
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
    # C# StatusIntent(WitherAmount) carries the status card count.
    states[AEONGLASS_INCREASING_INTENSITY_MOVE].intents[0].hits = initial_wither_amount

    # C# AfterAddedToRoom: WitheringPresence(6) on the opposing players (sim:
    # single instance on Aeonglass with one independent CardsLeft counter per
    # opposing player; see power docstring) and Artifact(3).
    creature.apply_power(
        WITHERING_PRESENCE_POWER_ID,
        AEONGLASS_WITHERING_PRESENCE_AMOUNT,
        applier=creature,
    )
    creature.apply_power(PowerId.ARTIFACT, AEONGLASS_ARTIFACT, applier=creature)
    _attach_aeonglass_wither_generation_matching(creature, ai_state)
    return creature, MonsterAI(states, AEONGLASS_EBB_MOVE)
