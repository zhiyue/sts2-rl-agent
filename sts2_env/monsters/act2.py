"""Act 2 (Hive) monsters: weak, normal, elite, boss.

All HP ranges, damage values, and state machines verified against decompiled C# source.
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from sts2_env.core.creature import Creature
from sts2_env.core.enums import CardId, CardRarity, CombatSide, MoveRepeatType, PowerId, ValueProp
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
    player_or_pet_owner,
)
from sts2_env.cards.status import (
    make_dazed,
    make_disintegration,
    make_frantic_escape,
    make_infection,
    make_mind_rot,
    make_sloth_status,
    make_toxic,
    make_void,
    make_waste_away,
)
from sts2_env.powers.remaining_c import SandpitPower, SwipePower

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


def _thieving_hopper_targets(combat: CombatState, creature: Creature) -> list[Creature]:
    return living_player_targets(combat)


def _contains_card_instance(cards: list, card) -> bool:
    return any(candidate is card for candidate in cards)


def _remove_card_instance(cards: list, card) -> None:
    for index, candidate in enumerate(cards):
        if candidate is card:
            cards.pop(index)
            return


def _thieving_hopper_steal_candidates(combat: CombatState, target: Creature) -> list:
    target_owner = player_or_pet_owner(target)
    state = combat.combat_player_state_for(target_owner)
    if state is None:
        return []
    return [
        card
        for card in list(state.draw) + list(state.discard)
        if _contains_card_instance(state.player_state.deck, card)
    ]


def _choose_thieving_hopper_card(combat: CombatState, cards: list):
    def not_imbued(card) -> bool:
        return not card.has_enchantment("Imbued")

    priorities = (
        lambda card: not_imbued(card) and card.rarity == CardRarity.UNCOMMON,
        lambda card: not_imbued(card) and card.rarity in {CardRarity.COMMON, CardRarity.RARE, CardRarity.EVENT},
        lambda card: not_imbued(card) and card.rarity in {CardRarity.BASIC, CardRarity.QUEST},
        lambda card: card.rarity == CardRarity.ANCIENT or card.has_enchantment("Imbued"),
    )
    for priority in priorities:
        matching = [card for card in cards if priority(card)]
        if matching:
            return combat.combat_card_generation_rng.choice(matching)
    return combat.combat_card_generation_rng.choice(cards) if cards else None


def _steal_card_with_swipe(combat: CombatState, creature: Creature, target: Creature) -> None:
    card = _choose_thieving_hopper_card(combat, _thieving_hopper_steal_candidates(combat, target))
    if card is None:
        return
    combat._remove_card_from_piles(card)
    target_owner = player_or_pet_owner(target)
    state = combat.combat_player_state_for(target_owner)
    if state is not None:
        _remove_card_instance(state.player_state.deck, card)
    swipe = creature.powers.get(PowerId.SWIPE)
    if not isinstance(swipe, SwipePower):
        swipe = SwipePower(0)
        creature.powers[PowerId.SWIPE] = swipe
    swipe.amount += 1
    swipe.steal(card, target_owner)


def _deal_damage_to_targets(
    combat: CombatState,
    creature: Creature,
    targets: list[Creature],
    base_dmg: int,
) -> None:
    for target in targets:
        if target.is_dead:
            continue
        dmg = calculate_damage(base_dmg, creature, target, ValueProp.MOVE, combat)
        apply_damage(target, dmg, ValueProp.MOVE, combat, creature)


# ========================================================================
# WEAK ENCOUNTERS
# ========================================================================

# ---- ThievingHopper (HP 79 / 84 asc) ----

THIEVING_HOPPER_MONSTER_ID = "THIEVING_HOPPER"
THIEVING_HOPPER_BASE_HP = 79
THIEVING_HOPPER_TOUGH_HP = 84
THIEVING_HOPPER_BASE_THEFT_DAMAGE = 17
THIEVING_HOPPER_DEADLY_THEFT_DAMAGE = 19
THIEVING_HOPPER_BASE_HAT_TRICK_DAMAGE = 21
THIEVING_HOPPER_DEADLY_HAT_TRICK_DAMAGE = 23
THIEVING_HOPPER_BASE_NAB_DAMAGE = 14
THIEVING_HOPPER_DEADLY_NAB_DAMAGE = 16
THIEVING_HOPPER_ESCAPE_ARTIST_AMOUNT = 5
THIEVING_HOPPER_FLUTTER_AMOUNT = 5
THIEVING_HOPPER_THIEVERY_MOVE = "THIEVERY_MOVE"
THIEVING_HOPPER_FLUTTER_MOVE = "FLUTTER_MOVE"
THIEVING_HOPPER_HAT_TRICK_MOVE = "HAT_TRICK_MOVE"
THIEVING_HOPPER_NAB_MOVE = "NAB_MOVE"
THIEVING_HOPPER_ESCAPE_MOVE = "ESCAPE_MOVE"


def create_thieving_hopper(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        THIEVING_HOPPER_TOUGH_HP,
        THIEVING_HOPPER_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=THIEVING_HOPPER_MONSTER_ID)

    def thievery(combat: CombatState) -> None:
        targets = _thieving_hopper_targets(combat, creature)
        for target in targets:
            _steal_card_with_swipe(combat, creature, target)
        theft_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            THIEVING_HOPPER_DEADLY_THEFT_DAMAGE,
            THIEVING_HOPPER_BASE_THEFT_DAMAGE,
        )
        _deal_damage_to_targets(combat, creature, targets, theft_dmg)

    def flutter(combat: CombatState) -> None:
        creature.apply_power(PowerId.FLUTTER, THIEVING_HOPPER_FLUTTER_AMOUNT)

    def hat_trick(combat: CombatState) -> None:
        hat_trick_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            THIEVING_HOPPER_DEADLY_HAT_TRICK_DAMAGE,
            THIEVING_HOPPER_BASE_HAT_TRICK_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, hat_trick_dmg)

    def nab(combat: CombatState) -> None:
        nab_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            THIEVING_HOPPER_DEADLY_NAB_DAMAGE,
            THIEVING_HOPPER_BASE_NAB_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, nab_dmg)

    def escape(combat: CombatState) -> None:
        combat.escape_creature(creature)

    theft_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        THIEVING_HOPPER_DEADLY_THEFT_DAMAGE,
        THIEVING_HOPPER_BASE_THEFT_DAMAGE,
    )
    hat_trick_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        THIEVING_HOPPER_DEADLY_HAT_TRICK_DAMAGE,
        THIEVING_HOPPER_BASE_HAT_TRICK_DAMAGE,
    )
    nab_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        THIEVING_HOPPER_DEADLY_NAB_DAMAGE,
        THIEVING_HOPPER_BASE_NAB_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        THIEVING_HOPPER_THIEVERY_MOVE: MoveState(
            THIEVING_HOPPER_THIEVERY_MOVE,
            thievery,
            [attack_intent(theft_intent_damage), Intent(IntentType.CARD_DEBUFF)],
            follow_up_id=THIEVING_HOPPER_FLUTTER_MOVE,
        ),
        THIEVING_HOPPER_FLUTTER_MOVE: MoveState(
            THIEVING_HOPPER_FLUTTER_MOVE,
            flutter,
            [buff_intent()],
            follow_up_id=THIEVING_HOPPER_HAT_TRICK_MOVE,
        ),
        THIEVING_HOPPER_HAT_TRICK_MOVE: MoveState(
            THIEVING_HOPPER_HAT_TRICK_MOVE,
            hat_trick,
            [attack_intent(hat_trick_intent_damage)],
            follow_up_id=THIEVING_HOPPER_NAB_MOVE,
        ),
        THIEVING_HOPPER_NAB_MOVE: MoveState(
            THIEVING_HOPPER_NAB_MOVE,
            nab,
            [attack_intent(nab_intent_damage)],
            follow_up_id=THIEVING_HOPPER_ESCAPE_MOVE,
        ),
        THIEVING_HOPPER_ESCAPE_MOVE: MoveState(
            THIEVING_HOPPER_ESCAPE_MOVE,
            escape,
            [Intent(IntentType.ESCAPE)],
            follow_up_id=THIEVING_HOPPER_ESCAPE_MOVE,
        ),
    }
    creature.apply_power(PowerId.ESCAPE_ARTIST, THIEVING_HOPPER_ESCAPE_ARTIST_AMOUNT)
    return creature, MonsterAI(states, THIEVING_HOPPER_THIEVERY_MOVE)


# ---- Tunneler (HP 87 / 92 asc) ----

TUNNELER_MONSTER_ID = "TUNNELER"
TUNNELER_BASE_HP = 87
TUNNELER_TOUGH_HP = 92
TUNNELER_BASE_BITE_DAMAGE = 13
TUNNELER_DEADLY_BITE_DAMAGE = 15
TUNNELER_BASE_BURROW_BLOCK = 32
TUNNELER_TOUGH_BURROW_BLOCK = 37
TUNNELER_BASE_BELOW_DAMAGE = 23
TUNNELER_DEADLY_BELOW_DAMAGE = 26
TUNNELER_BURROWED_AMOUNT = 1
TUNNELER_BITE_MOVE = "BITE_MOVE"
TUNNELER_BURROW_MOVE = "BURROW_MOVE"
TUNNELER_BELOW_MOVE = "BELOW_MOVE_1"
TUNNELER_DIZZY_MOVE = "DIZZY_MOVE"


def create_tunneler(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        TUNNELER_TOUGH_HP,
        TUNNELER_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=TUNNELER_MONSTER_ID)

    def burrow(combat: CombatState) -> None:
        burrow_block = _ascension_value(
            _combat_ascension_level(combat),
            TOUGH_ENEMIES_ASCENSION_LEVEL,
            TUNNELER_TOUGH_BURROW_BLOCK,
            TUNNELER_BASE_BURROW_BLOCK,
        )
        creature.apply_power(PowerId.BURROWED, TUNNELER_BURROWED_AMOUNT)
        _gain_block(creature, burrow_block, combat)

    def bite(combat: CombatState) -> None:
        bite_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            TUNNELER_DEADLY_BITE_DAMAGE,
            TUNNELER_BASE_BITE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, bite_dmg)

    def below(combat: CombatState) -> None:
        below_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            TUNNELER_DEADLY_BELOW_DAMAGE,
            TUNNELER_BASE_BELOW_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, below_dmg)

    def dizzy(combat: CombatState) -> None:
        return

    bite_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        TUNNELER_DEADLY_BITE_DAMAGE,
        TUNNELER_BASE_BITE_DAMAGE,
    )
    below_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        TUNNELER_DEADLY_BELOW_DAMAGE,
        TUNNELER_BASE_BELOW_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        TUNNELER_BITE_MOVE: MoveState(
            TUNNELER_BITE_MOVE,
            bite,
            [attack_intent(bite_intent_damage)],
            follow_up_id=TUNNELER_BURROW_MOVE,
        ),
        TUNNELER_BURROW_MOVE: MoveState(
            TUNNELER_BURROW_MOVE,
            burrow,
            [buff_intent(), defend_intent()],
            follow_up_id=TUNNELER_BELOW_MOVE,
        ),
        TUNNELER_BELOW_MOVE: MoveState(
            TUNNELER_BELOW_MOVE,
            below,
            [attack_intent(below_intent_damage)],
            follow_up_id=TUNNELER_BELOW_MOVE,
        ),
        TUNNELER_DIZZY_MOVE: MoveState(
            TUNNELER_DIZZY_MOVE,
            dizzy,
            [Intent(IntentType.STUN)],
            follow_up_id=TUNNELER_BITE_MOVE,
        ),
    }
    return creature, MonsterAI(states, TUNNELER_BITE_MOVE)


# ---- Bowlbugs ----

BOWLBUG_EGG_MONSTER_ID = "BOWLBUG_EGG"
BOWLBUG_EGG_BASE_MIN_HP = 21
BOWLBUG_EGG_BASE_MAX_HP = 22
BOWLBUG_EGG_TOUGH_MIN_HP = 23
BOWLBUG_EGG_TOUGH_MAX_HP = 24
BOWLBUG_EGG_BASE_BITE_DAMAGE = 7
BOWLBUG_EGG_DEADLY_BITE_DAMAGE = 8
BOWLBUG_EGG_BASE_PROTECT_BLOCK = 7
BOWLBUG_EGG_DEADLY_PROTECT_BLOCK = 8
BOWLBUG_EGG_BITE_MOVE = "BITE_MOVE"

BOWLBUG_NECTAR_MONSTER_ID = "BOWLBUG_NECTAR"
BOWLBUG_NECTAR_BASE_MIN_HP = 35
BOWLBUG_NECTAR_BASE_MAX_HP = 38
BOWLBUG_NECTAR_TOUGH_MIN_HP = 36
BOWLBUG_NECTAR_TOUGH_MAX_HP = 39
BOWLBUG_NECTAR_THRASH_DAMAGE = 3
BOWLBUG_NECTAR_BASE_BUFF_STRENGTH = 15
BOWLBUG_NECTAR_DEADLY_BUFF_STRENGTH = 16
BOWLBUG_NECTAR_THRASH_MOVE = "THRASH_MOVE"
BOWLBUG_NECTAR_BUFF_MOVE = "BUFF_MOVE"
BOWLBUG_NECTAR_THRASH2_MOVE = "THRASH2_MOVE"

BOWLBUG_ROCK_MONSTER_ID = "BOWLBUG_ROCK"
BOWLBUG_ROCK_BASE_MIN_HP = 45
BOWLBUG_ROCK_BASE_MAX_HP = 48
BOWLBUG_ROCK_TOUGH_MIN_HP = 46
BOWLBUG_ROCK_TOUGH_MAX_HP = 49
BOWLBUG_ROCK_BASE_HEADBUTT_DAMAGE = 15
BOWLBUG_ROCK_DEADLY_HEADBUTT_DAMAGE = 16
BOWLBUG_ROCK_IMBALANCED_AMOUNT = 1
BOWLBUG_ROCK_HEADBUTT_MOVE = "HEADBUTT_MOVE"
BOWLBUG_ROCK_POST_HEADBUTT = "POST_HEADBUTT"
BOWLBUG_ROCK_DIZZY_MOVE = "DIZZY_MOVE"

BOWLBUG_SILK_MONSTER_ID = "BOWLBUG_SILK"
BOWLBUG_SILK_BASE_MIN_HP = 40
BOWLBUG_SILK_BASE_MAX_HP = 43
BOWLBUG_SILK_TOUGH_MIN_HP = 41
BOWLBUG_SILK_TOUGH_MAX_HP = 44
BOWLBUG_SILK_BASE_THRASH_DAMAGE = 4
BOWLBUG_SILK_DEADLY_THRASH_DAMAGE = 5
BOWLBUG_SILK_THRASH_REPEAT = 2
BOWLBUG_SILK_TOXIC_SPIT_WEAK = 1
BOWLBUG_SILK_TRASH_MOVE = "TRASH_MOVE"
BOWLBUG_SILK_TOXIC_SPIT_MOVE = "TOXIC_SPIT_MOVE"


def create_bowlbug_egg(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        BOWLBUG_EGG_TOUGH_MIN_HP,
        BOWLBUG_EGG_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        BOWLBUG_EGG_TOUGH_MAX_HP,
        BOWLBUG_EGG_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id=BOWLBUG_EGG_MONSTER_ID)

    def bite(combat: CombatState) -> None:
        bite_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            BOWLBUG_EGG_DEADLY_BITE_DAMAGE,
            BOWLBUG_EGG_BASE_BITE_DAMAGE,
        )
        protect_block = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            BOWLBUG_EGG_DEADLY_PROTECT_BLOCK,
            BOWLBUG_EGG_BASE_PROTECT_BLOCK,
        )
        _deal_damage_to_player(combat, creature, bite_dmg)
        _gain_block(creature, protect_block, combat)

    bite_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        BOWLBUG_EGG_DEADLY_BITE_DAMAGE,
        BOWLBUG_EGG_BASE_BITE_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        BOWLBUG_EGG_BITE_MOVE: MoveState(
            BOWLBUG_EGG_BITE_MOVE,
            bite,
            [attack_intent(bite_intent_damage), defend_intent()],
            follow_up_id=BOWLBUG_EGG_BITE_MOVE,
        ),
    }
    return creature, MonsterAI(states, BOWLBUG_EGG_BITE_MOVE)


def create_bowlbug_nectar(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        BOWLBUG_NECTAR_TOUGH_MIN_HP,
        BOWLBUG_NECTAR_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        BOWLBUG_NECTAR_TOUGH_MAX_HP,
        BOWLBUG_NECTAR_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id=BOWLBUG_NECTAR_MONSTER_ID)

    def thrash(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, BOWLBUG_NECTAR_THRASH_DAMAGE)

    def buff_move(combat: CombatState) -> None:
        buff_strength = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            BOWLBUG_NECTAR_DEADLY_BUFF_STRENGTH,
            BOWLBUG_NECTAR_BASE_BUFF_STRENGTH,
        )
        creature.apply_power(PowerId.STRENGTH, buff_strength)

    def thrash2(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, BOWLBUG_NECTAR_THRASH_DAMAGE)

    states: dict[str, MonsterState] = {
        BOWLBUG_NECTAR_THRASH_MOVE: MoveState(
            BOWLBUG_NECTAR_THRASH_MOVE,
            thrash,
            [attack_intent(BOWLBUG_NECTAR_THRASH_DAMAGE)],
            follow_up_id=BOWLBUG_NECTAR_BUFF_MOVE,
        ),
        BOWLBUG_NECTAR_BUFF_MOVE: MoveState(
            BOWLBUG_NECTAR_BUFF_MOVE,
            buff_move,
            [buff_intent()],
            follow_up_id=BOWLBUG_NECTAR_THRASH2_MOVE,
        ),
        BOWLBUG_NECTAR_THRASH2_MOVE: MoveState(
            BOWLBUG_NECTAR_THRASH2_MOVE,
            thrash2,
            [attack_intent(BOWLBUG_NECTAR_THRASH_DAMAGE)],
            follow_up_id=BOWLBUG_NECTAR_THRASH2_MOVE,
        ),
    }
    return creature, MonsterAI(states, BOWLBUG_NECTAR_THRASH_MOVE)


def create_bowlbug_rock(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        BOWLBUG_ROCK_TOUGH_MIN_HP,
        BOWLBUG_ROCK_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        BOWLBUG_ROCK_TOUGH_MAX_HP,
        BOWLBUG_ROCK_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id=BOWLBUG_ROCK_MONSTER_ID)

    def headbutt(combat: CombatState) -> None:
        headbutt_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            BOWLBUG_ROCK_DEADLY_HEADBUTT_DAMAGE,
            BOWLBUG_ROCK_BASE_HEADBUTT_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, headbutt_dmg)

    def dizzy(combat: CombatState) -> None:
        power = creature.powers.get(PowerId.IMBALANCED)
        if power is not None and hasattr(power, "was_fully_blocked"):
            power.was_fully_blocked = False

    def is_off_balance() -> bool:
        power = creature.powers.get(PowerId.IMBALANCED)
        return bool(getattr(power, "was_fully_blocked", False))

    cond = ConditionalBranchState(BOWLBUG_ROCK_POST_HEADBUTT)
    cond.add_branch(is_off_balance, BOWLBUG_ROCK_DIZZY_MOVE)
    cond.add_branch(lambda: True, BOWLBUG_ROCK_HEADBUTT_MOVE)

    headbutt_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        BOWLBUG_ROCK_DEADLY_HEADBUTT_DAMAGE,
        BOWLBUG_ROCK_BASE_HEADBUTT_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        BOWLBUG_ROCK_HEADBUTT_MOVE: MoveState(
            BOWLBUG_ROCK_HEADBUTT_MOVE,
            headbutt,
            [attack_intent(headbutt_intent_damage)],
            follow_up_id=BOWLBUG_ROCK_POST_HEADBUTT,
        ),
        BOWLBUG_ROCK_POST_HEADBUTT: cond,
        BOWLBUG_ROCK_DIZZY_MOVE: MoveState(
            BOWLBUG_ROCK_DIZZY_MOVE,
            dizzy,
            [Intent(IntentType.STUN)],
            follow_up_id=BOWLBUG_ROCK_HEADBUTT_MOVE,
        ),
    }
    creature.apply_power(PowerId.IMBALANCED, BOWLBUG_ROCK_IMBALANCED_AMOUNT)
    return creature, MonsterAI(states, BOWLBUG_ROCK_HEADBUTT_MOVE)


def create_bowlbug_silk(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        BOWLBUG_SILK_TOUGH_MIN_HP,
        BOWLBUG_SILK_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        BOWLBUG_SILK_TOUGH_MAX_HP,
        BOWLBUG_SILK_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id=BOWLBUG_SILK_MONSTER_ID)

    def toxic_spit(combat: CombatState) -> None:
        apply_power_to_living_player_targets(combat, PowerId.WEAK, BOWLBUG_SILK_TOXIC_SPIT_WEAK, applier=creature)

    def thrash(combat: CombatState) -> None:
        thrash_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            BOWLBUG_SILK_DEADLY_THRASH_DAMAGE,
            BOWLBUG_SILK_BASE_THRASH_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, thrash_dmg, hits=BOWLBUG_SILK_THRASH_REPEAT)

    thrash_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        BOWLBUG_SILK_DEADLY_THRASH_DAMAGE,
        BOWLBUG_SILK_BASE_THRASH_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        BOWLBUG_SILK_TRASH_MOVE: MoveState(
            BOWLBUG_SILK_TRASH_MOVE,
            thrash,
            [multi_attack_intent(thrash_intent_damage, BOWLBUG_SILK_THRASH_REPEAT)],
            follow_up_id=BOWLBUG_SILK_TOXIC_SPIT_MOVE,
        ),
        BOWLBUG_SILK_TOXIC_SPIT_MOVE: MoveState(
            BOWLBUG_SILK_TOXIC_SPIT_MOVE,
            toxic_spit,
            [debuff_intent()],
            follow_up_id=BOWLBUG_SILK_TRASH_MOVE,
        ),
    }
    return creature, MonsterAI(states, BOWLBUG_SILK_TOXIC_SPIT_MOVE)


# ---- Exoskeleton (HP 24-28 / 26-30 asc) ----

EXOSKELETON_MONSTER_ID = "EXOSKELETON"
EXOSKELETON_BASE_MIN_HP = 24
EXOSKELETON_BASE_MAX_HP = 28
EXOSKELETON_TOUGH_MIN_HP = 26
EXOSKELETON_TOUGH_MAX_HP = 30
EXOSKELETON_SKITTER_DAMAGE = 1
EXOSKELETON_BASE_SKITTER_HITS = 3
EXOSKELETON_DEADLY_SKITTER_HITS = 4
EXOSKELETON_BASE_MANDIBLE_DAMAGE = 8
EXOSKELETON_DEADLY_MANDIBLE_DAMAGE = 9
EXOSKELETON_ENRAGE_STRENGTH = 2
EXOSKELETON_HARD_TO_KILL = 9
EXOSKELETON_FIRST_SLOT = "first"
EXOSKELETON_SECOND_SLOT = "second"
EXOSKELETON_THIRD_SLOT = "third"
EXOSKELETON_FOURTH_SLOT = "fourth"
EXOSKELETON_RANDOM_STATE = "RAND"
EXOSKELETON_SKITTER_MOVE = "SKITTER_MOVE"
EXOSKELETON_MANDIBLE_MOVE = "MANDIBLE_MOVE"
EXOSKELETON_ENRAGE_MOVE = "ENRAGE_MOVE"


def create_exoskeleton(rng: Rng, slot: str = EXOSKELETON_FIRST_SLOT, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        EXOSKELETON_TOUGH_MIN_HP,
        EXOSKELETON_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        EXOSKELETON_TOUGH_MAX_HP,
        EXOSKELETON_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id=EXOSKELETON_MONSTER_ID)

    def skitter(combat: CombatState) -> None:
        skitter_hits = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            EXOSKELETON_DEADLY_SKITTER_HITS,
            EXOSKELETON_BASE_SKITTER_HITS,
        )
        _deal_damage_to_player(combat, creature, EXOSKELETON_SKITTER_DAMAGE, hits=skitter_hits)

    def mandible(combat: CombatState) -> None:
        mandible_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            EXOSKELETON_DEADLY_MANDIBLE_DAMAGE,
            EXOSKELETON_BASE_MANDIBLE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, mandible_dmg)

    def enrage(combat: CombatState) -> None:
        creature.apply_power(PowerId.STRENGTH, EXOSKELETON_ENRAGE_STRENGTH)

    skitter_intent_hits = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        EXOSKELETON_DEADLY_SKITTER_HITS,
        EXOSKELETON_BASE_SKITTER_HITS,
    )
    mandible_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        EXOSKELETON_DEADLY_MANDIBLE_DAMAGE,
        EXOSKELETON_BASE_MANDIBLE_DAMAGE,
    )

    rand = RandomBranchState(EXOSKELETON_RANDOM_STATE)
    rand.add_branch(EXOSKELETON_SKITTER_MOVE, MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch(EXOSKELETON_MANDIBLE_MOVE, MoveRepeatType.CANNOT_REPEAT)

    states: dict[str, MonsterState] = {
        EXOSKELETON_RANDOM_STATE: rand,
        EXOSKELETON_SKITTER_MOVE: MoveState(
            EXOSKELETON_SKITTER_MOVE,
            skitter,
            [multi_attack_intent(EXOSKELETON_SKITTER_DAMAGE, skitter_intent_hits)],
            follow_up_id=EXOSKELETON_RANDOM_STATE,
        ),
        EXOSKELETON_MANDIBLE_MOVE: MoveState(
            EXOSKELETON_MANDIBLE_MOVE,
            mandible,
            [attack_intent(mandible_intent_damage)],
            follow_up_id=EXOSKELETON_ENRAGE_MOVE,
        ),
        EXOSKELETON_ENRAGE_MOVE: MoveState(
            EXOSKELETON_ENRAGE_MOVE,
            enrage,
            [buff_intent()],
            follow_up_id=EXOSKELETON_RANDOM_STATE,
        ),
    }

    slot_map = {
        EXOSKELETON_FIRST_SLOT: EXOSKELETON_SKITTER_MOVE,
        EXOSKELETON_SECOND_SLOT: EXOSKELETON_MANDIBLE_MOVE,
        EXOSKELETON_THIRD_SLOT: EXOSKELETON_ENRAGE_MOVE,
        EXOSKELETON_FOURTH_SLOT: EXOSKELETON_RANDOM_STATE,
    }
    initial = slot_map.get(slot, EXOSKELETON_RANDOM_STATE)

    creature.apply_power(PowerId.HARD_TO_KILL, EXOSKELETON_HARD_TO_KILL)
    return creature, MonsterAI(states, initial, rng)


# ========================================================================
# NORMAL ENCOUNTERS
# ========================================================================

# ---- Chomper (HP 60-64 / 63-67 asc) ----

CHOMPER_MONSTER_ID = "CHOMPER"
CHOMPER_BASE_MIN_HP = 60
CHOMPER_BASE_MAX_HP = 64
CHOMPER_TOUGH_MIN_HP = 63
CHOMPER_TOUGH_MAX_HP = 67
CHOMPER_BASE_CLAMP_DAMAGE = 8
CHOMPER_DEADLY_CLAMP_DAMAGE = 9
CHOMPER_CLAMP_REPEAT = 2
CHOMPER_SCREECH_DAZED = 3
CHOMPER_ARTIFACT_AMOUNT = 2
CHOMPER_CLAMP_MOVE = "CLAMP_MOVE"
CHOMPER_SCREECH_MOVE = "SCREECH_MOVE"


def create_chomper(rng: Rng, scream_first: bool = False, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        CHOMPER_TOUGH_MIN_HP,
        CHOMPER_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        CHOMPER_TOUGH_MAX_HP,
        CHOMPER_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id=CHOMPER_MONSTER_ID)

    def clamp(combat: CombatState) -> None:
        clamp_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            CHOMPER_DEADLY_CLAMP_DAMAGE,
            CHOMPER_BASE_CLAMP_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, clamp_dmg, hits=CHOMPER_CLAMP_REPEAT)

    def screech(combat: CombatState) -> None:
        add_generated_cards_to_living_player_discards(combat, make_dazed, CHOMPER_SCREECH_DAZED)

    clamp_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        CHOMPER_DEADLY_CLAMP_DAMAGE,
        CHOMPER_BASE_CLAMP_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        CHOMPER_CLAMP_MOVE: MoveState(
            CHOMPER_CLAMP_MOVE,
            clamp,
            [multi_attack_intent(clamp_intent_damage, CHOMPER_CLAMP_REPEAT)],
            follow_up_id=CHOMPER_SCREECH_MOVE,
        ),
        CHOMPER_SCREECH_MOVE: MoveState(
            CHOMPER_SCREECH_MOVE,
            screech,
            [status_intent()],
            follow_up_id=CHOMPER_CLAMP_MOVE,
        ),
    }

    creature.apply_power(PowerId.ARTIFACT, CHOMPER_ARTIFACT_AMOUNT)
    initial = CHOMPER_SCREECH_MOVE if scream_first else CHOMPER_CLAMP_MOVE
    return creature, MonsterAI(states, initial, rng)


# ---- HunterKiller (HP 121 / 126 asc) ----

HUNTER_KILLER_MONSTER_ID = "HUNTER_KILLER"
HUNTER_KILLER_BASE_HP = 121
HUNTER_KILLER_TOUGH_HP = 126
HUNTER_KILLER_BASE_BITE_DAMAGE = 17
HUNTER_KILLER_DEADLY_BITE_DAMAGE = 19
HUNTER_KILLER_BASE_PUNCTURE_DAMAGE = 7
HUNTER_KILLER_DEADLY_PUNCTURE_DAMAGE = 8
HUNTER_KILLER_PUNCTURE_REPEAT = 3
HUNTER_KILLER_TENDER_AMOUNT = 1
HUNTER_KILLER_PUNCTURE_MAX_REPEAT = 2
HUNTER_KILLER_TENDERIZING_GOOP_MOVE = "TENDERIZING_GOOP_MOVE"
HUNTER_KILLER_BITE_MOVE = "BITE_MOVE"
HUNTER_KILLER_PUNCTURE_MOVE = "PUNCTURE_MOVE"
HUNTER_KILLER_RANDOM_STATE = "RAND"


def create_hunter_killer(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        HUNTER_KILLER_TOUGH_HP,
        HUNTER_KILLER_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=HUNTER_KILLER_MONSTER_ID)

    def tenderizing_goop(combat: CombatState) -> None:
        apply_power_to_living_player_targets(combat, PowerId.TENDER, HUNTER_KILLER_TENDER_AMOUNT, applier=creature)

    def bite(combat: CombatState) -> None:
        bite_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            HUNTER_KILLER_DEADLY_BITE_DAMAGE,
            HUNTER_KILLER_BASE_BITE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, bite_dmg)

    def puncture(combat: CombatState) -> None:
        puncture_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            HUNTER_KILLER_DEADLY_PUNCTURE_DAMAGE,
            HUNTER_KILLER_BASE_PUNCTURE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, puncture_dmg, hits=HUNTER_KILLER_PUNCTURE_REPEAT)

    bite_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        HUNTER_KILLER_DEADLY_BITE_DAMAGE,
        HUNTER_KILLER_BASE_BITE_DAMAGE,
    )
    puncture_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        HUNTER_KILLER_DEADLY_PUNCTURE_DAMAGE,
        HUNTER_KILLER_BASE_PUNCTURE_DAMAGE,
    )

    rand = RandomBranchState(HUNTER_KILLER_RANDOM_STATE)
    rand.add_branch(HUNTER_KILLER_BITE_MOVE, MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch(
        HUNTER_KILLER_PUNCTURE_MOVE,
        MoveRepeatType.CAN_REPEAT_X_TIMES,
        max_times=HUNTER_KILLER_PUNCTURE_MAX_REPEAT,
    )

    states: dict[str, MonsterState] = {
        HUNTER_KILLER_TENDERIZING_GOOP_MOVE: MoveState(
            HUNTER_KILLER_TENDERIZING_GOOP_MOVE,
            tenderizing_goop,
            [debuff_intent()],
            follow_up_id=HUNTER_KILLER_RANDOM_STATE,
        ),
        HUNTER_KILLER_BITE_MOVE: MoveState(
            HUNTER_KILLER_BITE_MOVE,
            bite,
            [attack_intent(bite_intent_damage)],
            follow_up_id=HUNTER_KILLER_RANDOM_STATE,
        ),
        HUNTER_KILLER_PUNCTURE_MOVE: MoveState(
            HUNTER_KILLER_PUNCTURE_MOVE,
            puncture,
            [multi_attack_intent(puncture_intent_damage, HUNTER_KILLER_PUNCTURE_REPEAT)],
            follow_up_id=HUNTER_KILLER_RANDOM_STATE,
        ),
        HUNTER_KILLER_RANDOM_STATE: rand,
    }
    return creature, MonsterAI(states, HUNTER_KILLER_TENDERIZING_GOOP_MOVE)


# ---- Wriggler (HP 17-21 / 18-22 asc) ----

WRIGGLER_MONSTER_ID = "WRIGGLER"
WRIGGLER_BASE_MIN_HP = 17
WRIGGLER_BASE_MAX_HP = 21
WRIGGLER_TOUGH_MIN_HP = 18
WRIGGLER_TOUGH_MAX_HP = 22
WRIGGLER_BASE_BITE_DAMAGE = 6
WRIGGLER_DEADLY_BITE_DAMAGE = 7
WRIGGLER_INFECTION_COUNT = 1
WRIGGLER_STRENGTH = 2
WRIGGLER_SLOT_PREFIX = "wriggler"
WRIGGLER_SLOT_NUMBER_BASE = 1
WRIGGLER_SLOT_1 = "wriggler1"
WRIGGLER_SLOT_2 = "wriggler2"
WRIGGLER_SLOT_3 = "wriggler3"
WRIGGLER_SLOT_4 = "wriggler4"
WRIGGLER_BITE_SLOTS = (WRIGGLER_SLOT_1, WRIGGLER_SLOT_3)
WRIGGLER_WRIGGLE_SLOTS = (WRIGGLER_SLOT_2, WRIGGLER_SLOT_4)
WRIGGLER_INIT_MOVE = "INIT_MOVE"
WRIGGLER_SPAWNED_MOVE = "SPAWNED_MOVE"
WRIGGLER_NASTY_BITE_MOVE = "NASTY_BITE_MOVE"
WRIGGLER_WRIGGLE_MOVE = "WRIGGLE_MOVE"


def create_wriggler(
    rng: Rng,
    slot: str = WRIGGLER_SLOT_1,
    start_stunned: bool = False,
    ascension_level: int = 0,
) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        WRIGGLER_TOUGH_MIN_HP,
        WRIGGLER_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        WRIGGLER_TOUGH_MAX_HP,
        WRIGGLER_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id=WRIGGLER_MONSTER_ID)

    def bite(combat: CombatState) -> None:
        bite_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            WRIGGLER_DEADLY_BITE_DAMAGE,
            WRIGGLER_BASE_BITE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, bite_dmg)

    def wriggle(combat: CombatState) -> None:
        add_generated_cards_to_living_player_discards(combat, make_infection, WRIGGLER_INFECTION_COUNT)
        creature.apply_power(PowerId.STRENGTH, WRIGGLER_STRENGTH)

    def spawned(combat: CombatState) -> None:
        return

    bite_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        WRIGGLER_DEADLY_BITE_DAMAGE,
        WRIGGLER_BASE_BITE_DAMAGE,
    )

    init = ConditionalBranchState(WRIGGLER_INIT_MOVE)
    init.add_branch(lambda: slot in WRIGGLER_BITE_SLOTS, WRIGGLER_NASTY_BITE_MOVE)
    init.add_branch(lambda: slot in WRIGGLER_WRIGGLE_SLOTS, WRIGGLER_WRIGGLE_MOVE)
    init.add_branch(lambda: True, WRIGGLER_NASTY_BITE_MOVE)

    states: dict[str, MonsterState] = {
        WRIGGLER_INIT_MOVE: init,
        WRIGGLER_SPAWNED_MOVE: MoveState(
            WRIGGLER_SPAWNED_MOVE,
            spawned,
            [Intent(IntentType.STUN)],
            follow_up_id=WRIGGLER_INIT_MOVE,
        ),
        WRIGGLER_NASTY_BITE_MOVE: MoveState(
            WRIGGLER_NASTY_BITE_MOVE,
            bite,
            [attack_intent(bite_intent_damage)],
            follow_up_id=WRIGGLER_WRIGGLE_MOVE,
        ),
        WRIGGLER_WRIGGLE_MOVE: MoveState(
            WRIGGLER_WRIGGLE_MOVE,
            wriggle,
            [buff_intent(), status_intent()],
            follow_up_id=WRIGGLER_NASTY_BITE_MOVE,
        ),
    }
    initial = WRIGGLER_SPAWNED_MOVE if start_stunned else WRIGGLER_INIT_MOVE
    return creature, MonsterAI(states, initial, rng)


# ---- LouseProgenitor (HP 134-136 / 138-141 asc) ----

LOUSE_PROGENITOR_MONSTER_ID = "LOUSE_PROGENITOR"
LOUSE_PROGENITOR_BASE_MIN_HP = 134
LOUSE_PROGENITOR_BASE_MAX_HP = 136
LOUSE_PROGENITOR_TOUGH_MIN_HP = 138
LOUSE_PROGENITOR_TOUGH_MAX_HP = 141
LOUSE_PROGENITOR_BASE_WEB_DAMAGE = 9
LOUSE_PROGENITOR_DEADLY_WEB_DAMAGE = 10
LOUSE_PROGENITOR_WEB_FRAIL = 2
LOUSE_PROGENITOR_BASE_POUNCE_DAMAGE = 14
LOUSE_PROGENITOR_DEADLY_POUNCE_DAMAGE = 16
LOUSE_PROGENITOR_BASE_CURL_BLOCK = 14
LOUSE_PROGENITOR_TOUGH_CURL_BLOCK = 18
LOUSE_PROGENITOR_BASE_GROW_STRENGTH = 5
LOUSE_PROGENITOR_DEADLY_GROW_STRENGTH = 7
LOUSE_PROGENITOR_WEB_CANNON_MOVE = "WEB_CANNON_MOVE"
LOUSE_PROGENITOR_CURL_AND_GROW_MOVE = "CURL_AND_GROW_MOVE"
LOUSE_PROGENITOR_POUNCE_MOVE = "POUNCE_MOVE"


def create_louse_progenitor(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        LOUSE_PROGENITOR_TOUGH_MIN_HP,
        LOUSE_PROGENITOR_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        LOUSE_PROGENITOR_TOUGH_MAX_HP,
        LOUSE_PROGENITOR_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id=LOUSE_PROGENITOR_MONSTER_ID)

    def web(combat: CombatState) -> None:
        web_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            LOUSE_PROGENITOR_DEADLY_WEB_DAMAGE,
            LOUSE_PROGENITOR_BASE_WEB_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, web_dmg)
        apply_power_to_living_player_targets(combat, PowerId.FRAIL, LOUSE_PROGENITOR_WEB_FRAIL, applier=creature)

    def curl_and_grow(combat: CombatState) -> None:
        curl_block = _ascension_value(
            _combat_ascension_level(combat),
            TOUGH_ENEMIES_ASCENSION_LEVEL,
            LOUSE_PROGENITOR_TOUGH_CURL_BLOCK,
            LOUSE_PROGENITOR_BASE_CURL_BLOCK,
        )
        grow_strength = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            LOUSE_PROGENITOR_DEADLY_GROW_STRENGTH,
            LOUSE_PROGENITOR_BASE_GROW_STRENGTH,
        )
        _gain_block(creature, curl_block, combat)
        creature.apply_power(PowerId.STRENGTH, grow_strength)

    def pounce(combat: CombatState) -> None:
        pounce_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            LOUSE_PROGENITOR_DEADLY_POUNCE_DAMAGE,
            LOUSE_PROGENITOR_BASE_POUNCE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, pounce_dmg)

    web_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        LOUSE_PROGENITOR_DEADLY_WEB_DAMAGE,
        LOUSE_PROGENITOR_BASE_WEB_DAMAGE,
    )
    pounce_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        LOUSE_PROGENITOR_DEADLY_POUNCE_DAMAGE,
        LOUSE_PROGENITOR_BASE_POUNCE_DAMAGE,
    )
    curl_block = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        LOUSE_PROGENITOR_TOUGH_CURL_BLOCK,
        LOUSE_PROGENITOR_BASE_CURL_BLOCK,
    )

    states: dict[str, MonsterState] = {
        LOUSE_PROGENITOR_WEB_CANNON_MOVE: MoveState(
            LOUSE_PROGENITOR_WEB_CANNON_MOVE,
            web,
            [attack_intent(web_intent_damage), debuff_intent()],
            follow_up_id=LOUSE_PROGENITOR_CURL_AND_GROW_MOVE,
        ),
        LOUSE_PROGENITOR_CURL_AND_GROW_MOVE: MoveState(
            LOUSE_PROGENITOR_CURL_AND_GROW_MOVE,
            curl_and_grow,
            [defend_intent(), buff_intent()],
            follow_up_id=LOUSE_PROGENITOR_POUNCE_MOVE,
        ),
        LOUSE_PROGENITOR_POUNCE_MOVE: MoveState(
            LOUSE_PROGENITOR_POUNCE_MOVE,
            pounce,
            [attack_intent(pounce_intent_damage)],
            follow_up_id=LOUSE_PROGENITOR_WEB_CANNON_MOVE,
        ),
    }
    creature.apply_power(PowerId.CURL_UP, curl_block)
    return creature, MonsterAI(states, LOUSE_PROGENITOR_WEB_CANNON_MOVE)


# ---- Myte (HP 61-67 / 64-69 asc) ----

MYTE_MONSTER_ID = "MYTE"
MYTE_BASE_MIN_HP = 61
MYTE_BASE_MAX_HP = 67
MYTE_TOUGH_MIN_HP = 64
MYTE_TOUGH_MAX_HP = 69
MYTE_BASE_BITE_DAMAGE = 13
MYTE_DEADLY_BITE_DAMAGE = 15
MYTE_BASE_SUCK_DAMAGE = 4
MYTE_DEADLY_SUCK_DAMAGE = 6
MYTE_BASE_SUCK_STRENGTH = 2
MYTE_DEADLY_SUCK_STRENGTH = 3
MYTE_TOXIC_COUNT = 2
MYTE_FIRST_SLOT = "first"
MYTE_SECOND_SLOT = "second"
MYTE_TOXIC_MOVE = "TOXIC_MOVE"
MYTE_BITE_MOVE = "BITE_MOVE"
MYTE_SUCK_MOVE = "SUCK_MOVE"


def create_myte(rng: Rng, slot: str = MYTE_FIRST_SLOT, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        MYTE_TOUGH_MIN_HP,
        MYTE_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        MYTE_TOUGH_MAX_HP,
        MYTE_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id=MYTE_MONSTER_ID)

    def bite(combat: CombatState) -> None:
        bite_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            MYTE_DEADLY_BITE_DAMAGE,
            MYTE_BASE_BITE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, bite_dmg)

    def toxic(combat: CombatState) -> None:
        for target in living_player_targets(combat):
            for _ in range(MYTE_TOXIC_COUNT):
                combat.add_generated_card_to_creature_hand(
                    target,
                    make_toxic(),
                    added_by_player=False,
                )

    def suck(combat: CombatState) -> None:
        suck_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            MYTE_DEADLY_SUCK_DAMAGE,
            MYTE_BASE_SUCK_DAMAGE,
        )
        suck_strength = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            MYTE_DEADLY_SUCK_STRENGTH,
            MYTE_BASE_SUCK_STRENGTH,
        )
        _deal_damage_to_player(combat, creature, suck_dmg)
        combat.apply_power_to(creature, PowerId.STRENGTH, suck_strength, applier=creature)

    bite_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        MYTE_DEADLY_BITE_DAMAGE,
        MYTE_BASE_BITE_DAMAGE,
    )
    suck_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        MYTE_DEADLY_SUCK_DAMAGE,
        MYTE_BASE_SUCK_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        MYTE_TOXIC_MOVE: MoveState(
            MYTE_TOXIC_MOVE,
            toxic,
            [status_intent()],
            follow_up_id=MYTE_BITE_MOVE,
        ),
        MYTE_BITE_MOVE: MoveState(
            MYTE_BITE_MOVE,
            bite,
            [attack_intent(bite_intent_damage)],
            follow_up_id=MYTE_SUCK_MOVE,
        ),
        MYTE_SUCK_MOVE: MoveState(
            MYTE_SUCK_MOVE,
            suck,
            [attack_intent(suck_intent_damage), buff_intent()],
            follow_up_id=MYTE_TOXIC_MOVE,
        ),
    }

    initial = MYTE_TOXIC_MOVE if slot == MYTE_FIRST_SLOT else MYTE_SUCK_MOVE
    return creature, MonsterAI(states, initial, rng)


# ---- Ovicopter (HP 67-72 / 70-75 asc) + ToughEgg ----

TOUGH_EGG_MONSTER_ID = "TOUGH_EGG"
TOUGH_EGG_BASE_INITIAL_MIN_HP = 14
TOUGH_EGG_BASE_INITIAL_MAX_HP = 18
TOUGH_EGG_TOUGH_INITIAL_MIN_HP = 15
TOUGH_EGG_TOUGH_INITIAL_MAX_HP = 19
TOUGH_EGG_BASE_HATCHLING_MIN_HP = 19
TOUGH_EGG_BASE_HATCHLING_MAX_HP = 22
TOUGH_EGG_TOUGH_HATCHLING_MIN_HP = 20
TOUGH_EGG_TOUGH_HATCHLING_MAX_HP = 23
TOUGH_EGG_BASE_NIBBLE_DAMAGE = 4
TOUGH_EGG_DEADLY_NIBBLE_DAMAGE = 5
TOUGH_EGG_MINION_AMOUNT = 1
TOUGH_EGG_PLAYER_SIDE_HATCH_DURATION = 1
TOUGH_EGG_ENEMY_SIDE_HATCH_DURATION = 2
TOUGH_EGG_HATCH_MOVE = "HATCH_MOVE"
TOUGH_EGG_NIBBLE_MOVE = "NIBBLE_MOVE"


def create_tough_egg(
    rng: Rng,
    combat: CombatState | None = None,
    ascension_level: int = 0,
) -> tuple[Creature, MonsterAI]:
    initial_min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        TOUGH_EGG_TOUGH_INITIAL_MIN_HP,
        TOUGH_EGG_BASE_INITIAL_MIN_HP,
    )
    initial_max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        TOUGH_EGG_TOUGH_INITIAL_MAX_HP,
        TOUGH_EGG_BASE_INITIAL_MAX_HP,
    )
    hp = rng.next_int(initial_min_hp, initial_max_hp)
    creature = Creature(max_hp=hp, monster_id=TOUGH_EGG_MONSTER_ID)

    def hatch(combat: CombatState) -> None:
        from sts2_env.core.hooks import scaled_multiplayer_enemy_max_hp

        hatchling_min_hp = _ascension_value(
            _combat_ascension_level(combat),
            TOUGH_ENEMIES_ASCENSION_LEVEL,
            TOUGH_EGG_TOUGH_HATCHLING_MIN_HP,
            TOUGH_EGG_BASE_HATCHLING_MIN_HP,
        )
        hatchling_max_hp = _ascension_value(
            _combat_ascension_level(combat),
            TOUGH_ENEMIES_ASCENSION_LEVEL,
            TOUGH_EGG_TOUGH_HATCHLING_MAX_HP,
            TOUGH_EGG_BASE_HATCHLING_MAX_HP,
        )
        hatchling_hp = rng.next_int(hatchling_min_hp, hatchling_max_hp)
        for power_id in list(creature.powers):
            if power_id != PowerId.MINION:
                creature.powers.pop(power_id, None)
        creature.max_hp = hatchling_hp
        scaled_hp = scaled_multiplayer_enemy_max_hp(creature, combat)
        creature.max_hp = scaled_hp
        creature.current_hp = scaled_hp

    def nibble(combat: CombatState) -> None:
        nibble_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            TOUGH_EGG_DEADLY_NIBBLE_DAMAGE,
            TOUGH_EGG_BASE_NIBBLE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, nibble_dmg)

    nibble_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        TOUGH_EGG_DEADLY_NIBBLE_DAMAGE,
        TOUGH_EGG_BASE_NIBBLE_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        TOUGH_EGG_HATCH_MOVE: MoveState(
            TOUGH_EGG_HATCH_MOVE,
            hatch,
            [Intent(IntentType.SUMMON)],
            follow_up_id=TOUGH_EGG_NIBBLE_MOVE,
        ),
        TOUGH_EGG_NIBBLE_MOVE: MoveState(
            TOUGH_EGG_NIBBLE_MOVE,
            nibble,
            [attack_intent(nibble_intent_damage)],
            follow_up_id=TOUGH_EGG_NIBBLE_MOVE,
        ),
    }
    creature.apply_power(PowerId.MINION, TOUGH_EGG_MINION_AMOUNT)
    hatch_duration = (
        TOUGH_EGG_ENEMY_SIDE_HATCH_DURATION
        if combat is not None and combat.current_side == CombatSide.ENEMY
        else TOUGH_EGG_PLAYER_SIDE_HATCH_DURATION
    )
    creature.apply_power(PowerId.HATCH, hatch_duration)
    return creature, MonsterAI(states, TOUGH_EGG_HATCH_MOVE)


OVICOPTER_MONSTER_ID = "OVICOPTER"
OVICOPTER_BASE_MIN_HP = 124
OVICOPTER_BASE_MAX_HP = 130
OVICOPTER_TOUGH_MIN_HP = 126
OVICOPTER_TOUGH_MAX_HP = 132
OVICOPTER_BASE_SMASH_DAMAGE = 16
OVICOPTER_DEADLY_SMASH_DAMAGE = 17
OVICOPTER_BASE_TENDERIZER_DAMAGE = 7
OVICOPTER_DEADLY_TENDERIZER_DAMAGE = 8
OVICOPTER_TENDERIZER_VULNERABLE = 2
OVICOPTER_BASE_NUTRITIONAL_PASTE_STRENGTH = 3
OVICOPTER_DEADLY_NUTRITIONAL_PASTE_STRENGTH = 4
OVICOPTER_EGGS_TO_LAY = 3
OVICOPTER_MAX_LIVING_TEAMMATES_FOR_LAY = 3
OVICOPTER_LAY_EGGS_MOVE = "LAY_EGGS_MOVE"
OVICOPTER_SMASH_MOVE = "SMASH_MOVE"
OVICOPTER_TENDERIZER_MOVE = "TENDERIZER_MOVE"
OVICOPTER_NUTRITIONAL_PASTE_MOVE = "NUTRITIONAL_PASTE_MOVE"
OVICOPTER_SUMMON_BRANCH_STATE = "SUMMON_BRANCH_STATE"


def create_ovicopter(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        OVICOPTER_TOUGH_MIN_HP,
        OVICOPTER_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        OVICOPTER_TOUGH_MAX_HP,
        OVICOPTER_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id=OVICOPTER_MONSTER_ID)

    def can_lay(combat: CombatState | None) -> bool:
        if combat is None:
            return True
        return (
            sum(1 for teammate in combat.get_teammates_of(creature) if teammate.is_alive)
            <= OVICOPTER_MAX_LIVING_TEAMMATES_FOR_LAY
        )

    def lay_eggs(combat: CombatState) -> None:
        for _ in range(OVICOPTER_EGGS_TO_LAY):
            egg, egg_ai = create_tough_egg(rng, combat, ascension_level=_combat_ascension_level(combat))
            combat.add_enemy(egg, egg_ai)

    def smash(combat: CombatState) -> None:
        smash_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            OVICOPTER_DEADLY_SMASH_DAMAGE,
            OVICOPTER_BASE_SMASH_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, smash_dmg)

    def tenderizer(combat: CombatState) -> None:
        tenderizer_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            OVICOPTER_DEADLY_TENDERIZER_DAMAGE,
            OVICOPTER_BASE_TENDERIZER_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, tenderizer_dmg)
        apply_power_to_living_player_targets(
            combat,
            PowerId.VULNERABLE,
            OVICOPTER_TENDERIZER_VULNERABLE,
            applier=creature,
        )

    def nutritional_paste(combat: CombatState) -> None:
        paste_strength = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            OVICOPTER_DEADLY_NUTRITIONAL_PASTE_STRENGTH,
            OVICOPTER_BASE_NUTRITIONAL_PASTE_STRENGTH,
        )
        creature.apply_power(PowerId.STRENGTH, paste_strength)

    smash_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        OVICOPTER_DEADLY_SMASH_DAMAGE,
        OVICOPTER_BASE_SMASH_DAMAGE,
    )
    tenderizer_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        OVICOPTER_DEADLY_TENDERIZER_DAMAGE,
        OVICOPTER_BASE_TENDERIZER_DAMAGE,
    )

    summon_branch = ConditionalBranchState(OVICOPTER_SUMMON_BRANCH_STATE)
    summon_branch.add_branch(lambda: can_lay(creature.combat_state), OVICOPTER_LAY_EGGS_MOVE)
    summon_branch.add_branch(lambda: True, OVICOPTER_NUTRITIONAL_PASTE_MOVE)

    states: dict[str, MonsterState] = {
        OVICOPTER_LAY_EGGS_MOVE: MoveState(
            OVICOPTER_LAY_EGGS_MOVE,
            lay_eggs,
            [Intent(IntentType.SUMMON)],
            follow_up_id=OVICOPTER_SMASH_MOVE,
        ),
        OVICOPTER_SMASH_MOVE: MoveState(
            OVICOPTER_SMASH_MOVE,
            smash,
            [attack_intent(smash_intent_damage)],
            follow_up_id=OVICOPTER_TENDERIZER_MOVE,
        ),
        OVICOPTER_TENDERIZER_MOVE: MoveState(
            OVICOPTER_TENDERIZER_MOVE,
            tenderizer,
            [attack_intent(tenderizer_intent_damage), debuff_intent()],
            follow_up_id=OVICOPTER_SUMMON_BRANCH_STATE,
        ),
        OVICOPTER_NUTRITIONAL_PASTE_MOVE: MoveState(
            OVICOPTER_NUTRITIONAL_PASTE_MOVE,
            nutritional_paste,
            [buff_intent()],
            follow_up_id=OVICOPTER_SMASH_MOVE,
        ),
        OVICOPTER_SUMMON_BRANCH_STATE: summon_branch,
    }
    return creature, MonsterAI(states, OVICOPTER_LAY_EGGS_MOVE)


# ---- SlumberingBeetle (HP 86 / 89 asc) ----

SLUMBERING_BEETLE_MONSTER_ID = "SLUMBERING_BEETLE"
SLUMBERING_BEETLE_BASE_HP = 86
SLUMBERING_BEETLE_TOUGH_HP = 89
SLUMBERING_BEETLE_BASE_ROLLOUT_DAMAGE = 16
SLUMBERING_BEETLE_DEADLY_ROLLOUT_DAMAGE = 18
SLUMBERING_BEETLE_BASE_PLATING = 15
SLUMBERING_BEETLE_TOUGH_PLATING = 18
SLUMBERING_BEETLE_ROLLOUT_STRENGTH = 2
SLUMBERING_BEETLE_SLUMBER = 3
SLUMBERING_BEETLE_SNORE_MOVE = "SNORE_MOVE"
SLUMBERING_BEETLE_SNORE_NEXT = "SNORE_NEXT"
SLUMBERING_BEETLE_ROLL_OUT_MOVE = "ROLL_OUT_MOVE"


def create_slumbering_beetle(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        SLUMBERING_BEETLE_TOUGH_HP,
        SLUMBERING_BEETLE_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=SLUMBERING_BEETLE_MONSTER_ID)

    def snore(combat: CombatState) -> None:
        pass

    def rollout(combat: CombatState) -> None:
        rollout_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            SLUMBERING_BEETLE_DEADLY_ROLLOUT_DAMAGE,
            SLUMBERING_BEETLE_BASE_ROLLOUT_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, rollout_dmg)
        combat.apply_power_to(creature, PowerId.STRENGTH, SLUMBERING_BEETLE_ROLLOUT_STRENGTH, applier=creature)

    rollout_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        SLUMBERING_BEETLE_DEADLY_ROLLOUT_DAMAGE,
        SLUMBERING_BEETLE_BASE_ROLLOUT_DAMAGE,
    )
    plating_amount = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        SLUMBERING_BEETLE_TOUGH_PLATING,
        SLUMBERING_BEETLE_BASE_PLATING,
    )

    cond = ConditionalBranchState(SLUMBERING_BEETLE_SNORE_NEXT)
    cond.add_branch(lambda: creature.has_power(PowerId.SLUMBER), SLUMBERING_BEETLE_SNORE_MOVE)
    cond.add_branch(lambda: True, SLUMBERING_BEETLE_ROLL_OUT_MOVE)

    states: dict[str, MonsterState] = {
        SLUMBERING_BEETLE_SNORE_MOVE: MoveState(
            SLUMBERING_BEETLE_SNORE_MOVE,
            snore,
            [sleep_intent()],
            follow_up_id=SLUMBERING_BEETLE_SNORE_NEXT,
        ),
        SLUMBERING_BEETLE_SNORE_NEXT: cond,
        SLUMBERING_BEETLE_ROLL_OUT_MOVE: MoveState(
            SLUMBERING_BEETLE_ROLL_OUT_MOVE,
            rollout,
            [attack_intent(rollout_intent_damage), buff_intent()],
            follow_up_id=SLUMBERING_BEETLE_ROLL_OUT_MOVE,
        ),
    }
    creature.apply_power(PowerId.PLATING, plating_amount)
    creature.apply_power(PowerId.SLUMBER, SLUMBERING_BEETLE_SLUMBER)
    return creature, MonsterAI(states, SLUMBERING_BEETLE_SNORE_MOVE)


# ---- SpinyToad (HP 116-119 / 121-124 asc) ----

SPINY_TOAD_MONSTER_ID = "SPINY_TOAD"
SPINY_TOAD_BASE_MIN_HP = 116
SPINY_TOAD_BASE_MAX_HP = 119
SPINY_TOAD_TOUGH_MIN_HP = 121
SPINY_TOAD_TOUGH_MAX_HP = 124
SPINY_TOAD_BASE_LASH_DAMAGE = 17
SPINY_TOAD_DEADLY_LASH_DAMAGE = 19
SPINY_TOAD_BASE_EXPLOSION_DAMAGE = 23
SPINY_TOAD_DEADLY_EXPLOSION_DAMAGE = 25
SPINY_TOAD_THORNS = 5
SPINY_TOAD_PROTRUDING_SPIKES_MOVE = "PROTRUDING_SPIKES_MOVE"
SPINY_TOAD_SPIKE_EXPLOSION_MOVE = "SPIKE_EXPLOSION_MOVE"
SPINY_TOAD_TONGUE_LASH_MOVE = "TONGUE_LASH_MOVE"


def create_spiny_toad(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        SPINY_TOAD_TOUGH_MIN_HP,
        SPINY_TOAD_BASE_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        SPINY_TOAD_TOUGH_MAX_HP,
        SPINY_TOAD_BASE_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id=SPINY_TOAD_MONSTER_ID)

    def lash(combat: CombatState) -> None:
        lash_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            SPINY_TOAD_DEADLY_LASH_DAMAGE,
            SPINY_TOAD_BASE_LASH_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, lash_dmg)

    def spines(combat: CombatState) -> None:
        creature.apply_power(PowerId.THORNS, SPINY_TOAD_THORNS)

    def explosion(combat: CombatState) -> None:
        explosion_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            SPINY_TOAD_DEADLY_EXPLOSION_DAMAGE,
            SPINY_TOAD_BASE_EXPLOSION_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, explosion_dmg)
        combat.apply_power_to(creature, PowerId.THORNS, -SPINY_TOAD_THORNS, applier=creature)

    lash_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        SPINY_TOAD_DEADLY_LASH_DAMAGE,
        SPINY_TOAD_BASE_LASH_DAMAGE,
    )
    explosion_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        SPINY_TOAD_DEADLY_EXPLOSION_DAMAGE,
        SPINY_TOAD_BASE_EXPLOSION_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        SPINY_TOAD_PROTRUDING_SPIKES_MOVE: MoveState(
            SPINY_TOAD_PROTRUDING_SPIKES_MOVE,
            spines,
            [buff_intent()],
            follow_up_id=SPINY_TOAD_SPIKE_EXPLOSION_MOVE,
        ),
        SPINY_TOAD_SPIKE_EXPLOSION_MOVE: MoveState(
            SPINY_TOAD_SPIKE_EXPLOSION_MOVE,
            explosion,
            [attack_intent(explosion_intent_damage)],
            follow_up_id=SPINY_TOAD_TONGUE_LASH_MOVE,
        ),
        SPINY_TOAD_TONGUE_LASH_MOVE: MoveState(
            SPINY_TOAD_TONGUE_LASH_MOVE,
            lash,
            [attack_intent(lash_intent_damage)],
            follow_up_id=SPINY_TOAD_PROTRUDING_SPIKES_MOVE,
        ),
    }
    return creature, MonsterAI(states, SPINY_TOAD_PROTRUDING_SPIKES_MOVE)


# ---- TheObscura (HP 123 / 129 asc) ----

THE_OBSCURA_MONSTER_ID = "THE_OBSCURA"
THE_OBSCURA_BASE_HP = 123
THE_OBSCURA_TOUGH_HP = 129
THE_OBSCURA_BASE_PIERCING_GAZE_DAMAGE = 10
THE_OBSCURA_DEADLY_PIERCING_GAZE_DAMAGE = 11
THE_OBSCURA_BASE_HARDENING_STRIKE_DAMAGE = 6
THE_OBSCURA_DEADLY_HARDENING_STRIKE_DAMAGE = 7
THE_OBSCURA_BASE_HARDENING_STRIKE_BLOCK = 6
THE_OBSCURA_DEADLY_HARDENING_STRIKE_BLOCK = 7
THE_OBSCURA_SAIL_STRENGTH = 3
THE_OBSCURA_ILLUSION_MOVE = "ILLUSION_MOVE"
THE_OBSCURA_PIERCING_GAZE_MOVE = "PIERCING_GAZE_MOVE"
THE_OBSCURA_SAIL_MOVE = "SAIL_MOVE"
THE_OBSCURA_HARDENING_STRIKE_MOVE = "HARDENING_STRIKE_MOVE"
THE_OBSCURA_RANDOM_STATE = "RAND"


def create_the_obscura(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        THE_OBSCURA_TOUGH_HP,
        THE_OBSCURA_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=THE_OBSCURA_MONSTER_ID)

    def illusion(combat: CombatState) -> None:
        from sts2_env.monsters.act1 import create_parafright

        parafright, parafright_ai = create_parafright(rng, ascension_level=getattr(combat, "ascension_level", 0))
        combat.add_enemy(parafright, parafright_ai)

    def piercing_gaze(combat: CombatState) -> None:
        gaze_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            THE_OBSCURA_DEADLY_PIERCING_GAZE_DAMAGE,
            THE_OBSCURA_BASE_PIERCING_GAZE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, gaze_dmg)

    def sail(combat: CombatState) -> None:
        for teammate in combat.get_teammates_of(creature):
            if teammate.is_alive:
                teammate.apply_power(PowerId.STRENGTH, THE_OBSCURA_SAIL_STRENGTH)

    def hardening_strike(combat: CombatState) -> None:
        hardening_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            THE_OBSCURA_DEADLY_HARDENING_STRIKE_DAMAGE,
            THE_OBSCURA_BASE_HARDENING_STRIKE_DAMAGE,
        )
        hardening_block = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            THE_OBSCURA_DEADLY_HARDENING_STRIKE_BLOCK,
            THE_OBSCURA_BASE_HARDENING_STRIKE_BLOCK,
        )
        _deal_damage_to_player(combat, creature, hardening_dmg)
        _gain_block(creature, hardening_block, combat)

    piercing_gaze_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        THE_OBSCURA_DEADLY_PIERCING_GAZE_DAMAGE,
        THE_OBSCURA_BASE_PIERCING_GAZE_DAMAGE,
    )
    hardening_strike_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        THE_OBSCURA_DEADLY_HARDENING_STRIKE_DAMAGE,
        THE_OBSCURA_BASE_HARDENING_STRIKE_DAMAGE,
    )

    rand = RandomBranchState(THE_OBSCURA_RANDOM_STATE)
    rand.add_branch(THE_OBSCURA_PIERCING_GAZE_MOVE, MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch(THE_OBSCURA_SAIL_MOVE, MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch(THE_OBSCURA_HARDENING_STRIKE_MOVE, MoveRepeatType.CANNOT_REPEAT)

    states: dict[str, MonsterState] = {
        THE_OBSCURA_ILLUSION_MOVE: MoveState(
            THE_OBSCURA_ILLUSION_MOVE,
            illusion,
            [Intent(IntentType.SUMMON)],
            follow_up_id=THE_OBSCURA_RANDOM_STATE,
        ),
        THE_OBSCURA_RANDOM_STATE: rand,
        THE_OBSCURA_PIERCING_GAZE_MOVE: MoveState(
            THE_OBSCURA_PIERCING_GAZE_MOVE,
            piercing_gaze,
            [attack_intent(piercing_gaze_intent_damage)],
            follow_up_id=THE_OBSCURA_RANDOM_STATE,
        ),
        THE_OBSCURA_SAIL_MOVE: MoveState(
            THE_OBSCURA_SAIL_MOVE,
            sail,
            [buff_intent()],
            follow_up_id=THE_OBSCURA_RANDOM_STATE,
        ),
        THE_OBSCURA_HARDENING_STRIKE_MOVE: MoveState(
            THE_OBSCURA_HARDENING_STRIKE_MOVE,
            hardening_strike,
            [attack_intent(hardening_strike_intent_damage), defend_intent()],
            follow_up_id=THE_OBSCURA_RANDOM_STATE,
        ),
    }
    return creature, MonsterAI(states, THE_OBSCURA_ILLUSION_MOVE)


# ========================================================================
# ELITE ENCOUNTERS
# ========================================================================

# ---- Decimillipede (3 segments) (HP 40-46 / 46-52 asc) ----

DECIMILLIPEDE_SEGMENT_MONSTER_ID = "DECIMILLIPEDE_SEGMENT"
DECIMILLIPEDE_SEGMENT_FRONT_MONSTER_ID = "DECIMILLIPEDE_SEGMENT_FRONT"
DECIMILLIPEDE_SEGMENT_MIDDLE_MONSTER_ID = "DECIMILLIPEDE_SEGMENT_MIDDLE"
DECIMILLIPEDE_SEGMENT_BACK_MONSTER_ID = "DECIMILLIPEDE_SEGMENT_BACK"
DECIMILLIPEDE_SEGMENT_MIN_HP = 40
DECIMILLIPEDE_SEGMENT_MAX_HP = 46
DECIMILLIPEDE_SEGMENT_TOUGH_MIN_HP = 46
DECIMILLIPEDE_SEGMENT_TOUGH_MAX_HP = 52
DECIMILLIPEDE_HP_STEP = 2
DECIMILLIPEDE_REATTACH_HP = 25
DECIMILLIPEDE_BASE_WRITHE_DAMAGE = 5
DECIMILLIPEDE_DEADLY_WRITHE_DAMAGE = 6
DECIMILLIPEDE_WRITHE_REPEAT = 2
DECIMILLIPEDE_BASE_CONSTRICT_DAMAGE = 8
DECIMILLIPEDE_DEADLY_CONSTRICT_DAMAGE = 9
DECIMILLIPEDE_CONSTRICT_WEAK = 1
DECIMILLIPEDE_BASE_BULK_DAMAGE = 6
DECIMILLIPEDE_DEADLY_BULK_DAMAGE = 7
DECIMILLIPEDE_BULK_STRENGTH = 2
DECIMILLIPEDE_STARTER_MOVE_COUNT = 3
DECIMILLIPEDE_WRITHE_MOVE = "WRITHE_MOVE"
DECIMILLIPEDE_BULK_MOVE = "BULK_MOVE"
DECIMILLIPEDE_CONSTRICT_MOVE = "CONSTRICT_MOVE"
DECIMILLIPEDE_DEAD_MOVE = "DEAD_MOVE"
DECIMILLIPEDE_REATTACH_MOVE = "REATTACH_MOVE"
DECIMILLIPEDE_RANDOM_STATE = "RAND"


def apply_decimillipede_segment_room_setup(creature: Creature, combat: CombatState) -> None:
    from sts2_env.core.hooks import scaled_multiplayer_enemy_hp

    max_hp = creature.max_hp
    if max_hp % DECIMILLIPEDE_HP_STEP == 1:
        max_hp += 1
    ascension_level = _combat_ascension_level(combat)
    min_initial_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        DECIMILLIPEDE_SEGMENT_TOUGH_MIN_HP,
        DECIMILLIPEDE_SEGMENT_MIN_HP,
    )
    max_initial_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        DECIMILLIPEDE_SEGMENT_TOUGH_MAX_HP,
        DECIMILLIPEDE_SEGMENT_MAX_HP,
    )
    min_hp = scaled_multiplayer_enemy_hp(min_initial_hp, combat)
    max_hp_cap = scaled_multiplayer_enemy_hp(max_initial_hp, combat)
    teammates = combat.get_teammates_of(creature)
    while any(teammate.max_hp == max_hp for teammate in teammates):
        max_hp += DECIMILLIPEDE_HP_STEP
        if max_hp > max_hp_cap:
            max_hp = min_hp
    creature.max_hp = max_hp
    creature.current_hp = max_hp
    combat.apply_power_to(creature, PowerId.REATTACH, DECIMILLIPEDE_REATTACH_HP, applier=creature)


def create_decimillipede_segment(
    rng: Rng,
    starter_idx: int = 0,
    ascension_level: int = 0,
) -> tuple[Creature, MonsterAI]:
    min_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        DECIMILLIPEDE_SEGMENT_TOUGH_MIN_HP,
        DECIMILLIPEDE_SEGMENT_MIN_HP,
    )
    max_hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        DECIMILLIPEDE_SEGMENT_TOUGH_MAX_HP,
        DECIMILLIPEDE_SEGMENT_MAX_HP,
    )
    hp = rng.next_int(min_hp, max_hp)
    creature = Creature(max_hp=hp, monster_id=DECIMILLIPEDE_SEGMENT_MONSTER_ID)

    def writhe(combat: CombatState) -> None:
        writhe_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            DECIMILLIPEDE_DEADLY_WRITHE_DAMAGE,
            DECIMILLIPEDE_BASE_WRITHE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, writhe_dmg, hits=DECIMILLIPEDE_WRITHE_REPEAT)

    def constrict(combat: CombatState) -> None:
        constrict_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            DECIMILLIPEDE_DEADLY_CONSTRICT_DAMAGE,
            DECIMILLIPEDE_BASE_CONSTRICT_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, constrict_dmg)
        apply_power_to_living_player_targets(combat, PowerId.WEAK, DECIMILLIPEDE_CONSTRICT_WEAK, applier=creature)

    def bulk(combat: CombatState) -> None:
        bulk_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            DECIMILLIPEDE_DEADLY_BULK_DAMAGE,
            DECIMILLIPEDE_BASE_BULK_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, bulk_dmg)
        combat.apply_power_to(creature, PowerId.STRENGTH, DECIMILLIPEDE_BULK_STRENGTH, applier=creature)

    def dead_move(combat: CombatState) -> None:
        pass

    def reattach(combat: CombatState) -> None:
        power = creature.powers.get(PowerId.REATTACH)
        do_reattach = getattr(power, "do_reattach", None)
        if callable(do_reattach):
            do_reattach(creature)
        else:
            creature.heal(DECIMILLIPEDE_REATTACH_HP)

    writhe_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        DECIMILLIPEDE_DEADLY_WRITHE_DAMAGE,
        DECIMILLIPEDE_BASE_WRITHE_DAMAGE,
    )
    constrict_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        DECIMILLIPEDE_DEADLY_CONSTRICT_DAMAGE,
        DECIMILLIPEDE_BASE_CONSTRICT_DAMAGE,
    )
    bulk_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        DECIMILLIPEDE_DEADLY_BULK_DAMAGE,
        DECIMILLIPEDE_BASE_BULK_DAMAGE,
    )

    rand = RandomBranchState(DECIMILLIPEDE_RANDOM_STATE)
    rand.add_branch(DECIMILLIPEDE_WRITHE_MOVE, MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch(DECIMILLIPEDE_BULK_MOVE, MoveRepeatType.CANNOT_REPEAT)
    rand.add_branch(DECIMILLIPEDE_CONSTRICT_MOVE, MoveRepeatType.CANNOT_REPEAT)

    states: dict[str, MonsterState] = {
        DECIMILLIPEDE_WRITHE_MOVE: MoveState(
            DECIMILLIPEDE_WRITHE_MOVE,
            writhe,
            [multi_attack_intent(writhe_intent_damage, DECIMILLIPEDE_WRITHE_REPEAT)],
            follow_up_id=DECIMILLIPEDE_CONSTRICT_MOVE,
        ),
        DECIMILLIPEDE_CONSTRICT_MOVE: MoveState(
            DECIMILLIPEDE_CONSTRICT_MOVE,
            constrict,
            [attack_intent(constrict_intent_damage), debuff_intent()],
            follow_up_id=DECIMILLIPEDE_BULK_MOVE,
        ),
        DECIMILLIPEDE_BULK_MOVE: MoveState(
            DECIMILLIPEDE_BULK_MOVE,
            bulk,
            [attack_intent(bulk_intent_damage), buff_intent()],
            follow_up_id=DECIMILLIPEDE_WRITHE_MOVE,
        ),
        DECIMILLIPEDE_DEAD_MOVE: MoveState(
            DECIMILLIPEDE_DEAD_MOVE,
            dead_move,
            [Intent(IntentType.UNKNOWN)],
            follow_up_id=DECIMILLIPEDE_REATTACH_MOVE,
        ),
        DECIMILLIPEDE_REATTACH_MOVE: MoveState(
            DECIMILLIPEDE_REATTACH_MOVE,
            reattach,
            [Intent(IntentType.HEAL)],
            follow_up_id=DECIMILLIPEDE_RANDOM_STATE,
            must_perform_once=True,
        ),
        DECIMILLIPEDE_RANDOM_STATE: rand,
    }

    starter_moves = (
        DECIMILLIPEDE_WRITHE_MOVE,
        DECIMILLIPEDE_BULK_MOVE,
        DECIMILLIPEDE_CONSTRICT_MOVE,
    )
    initial = starter_moves[starter_idx % DECIMILLIPEDE_STARTER_MOVE_COUNT]
    return creature, MonsterAI(states, initial, rng)


# ---- DecimillipedeSegmentFront (HP 42-48 / 48-56 asc) ----
# Identical behavior to DecimillipedeSegment (same base class in C#).
# The only difference is visual (front segment animation).

def create_decimillipede_segment_front(
    rng: Rng,
    starter_idx: int = 0,
    ascension_level: int = 0,
) -> tuple[Creature, MonsterAI]:
    creature, ai = create_decimillipede_segment(rng, starter_idx, ascension_level=ascension_level)
    creature.monster_id = DECIMILLIPEDE_SEGMENT_FRONT_MONSTER_ID
    return creature, ai


# ---- DecimillipedeSegmentMiddle (HP 42-48 / 48-56 asc) ----
# Identical behavior to DecimillipedeSegment (same base class in C#).

def create_decimillipede_segment_middle(
    rng: Rng,
    starter_idx: int = 0,
    ascension_level: int = 0,
) -> tuple[Creature, MonsterAI]:
    creature, ai = create_decimillipede_segment(rng, starter_idx, ascension_level=ascension_level)
    creature.monster_id = DECIMILLIPEDE_SEGMENT_MIDDLE_MONSTER_ID
    return creature, ai


# ---- DecimillipedeSegmentBack (HP 42-48 / 48-56 asc) ----
# Identical behavior to DecimillipedeSegment (same base class in C#).

def create_decimillipede_segment_back(
    rng: Rng,
    starter_idx: int = 0,
    ascension_level: int = 0,
) -> tuple[Creature, MonsterAI]:
    creature, ai = create_decimillipede_segment(rng, starter_idx, ascension_level=ascension_level)
    creature.monster_id = DECIMILLIPEDE_SEGMENT_BACK_MONSTER_ID
    return creature, ai


# ---- Entomancer (HP 145 / 165 asc) ----

ENTOMANCER_MONSTER_ID = "ENTOMANCER"
ENTOMANCER_BASE_HP = 145
ENTOMANCER_TOUGH_HP = 165
ENTOMANCER_BASE_SPEAR_DAMAGE = 18
ENTOMANCER_DEADLY_SPEAR_DAMAGE = 20
ENTOMANCER_BASE_BEES_DAMAGE = 3
ENTOMANCER_DEADLY_BEES_DAMAGE = 3
ENTOMANCER_BASE_BEES_REPEAT = 7
ENTOMANCER_DEADLY_BEES_REPEAT = 8
ENTOMANCER_INITIAL_PERSONAL_HIVE = 1
ENTOMANCER_PERSONAL_HIVE_THRESHOLD = 3
ENTOMANCER_PHEROMONE_HIVE_GAIN = 1
ENTOMANCER_PHEROMONE_STRENGTH_GAIN = 1
ENTOMANCER_MAX_HIVE_STRENGTH_GAIN = 2
ENTOMANCER_BEES_MOVE = "BEES_MOVE"
ENTOMANCER_SPEAR_MOVE = "SPEAR_MOVE"
ENTOMANCER_PHEROMONE_SPIT_MOVE = "PHEROMONE_SPIT_MOVE"


def create_entomancer(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        ENTOMANCER_TOUGH_HP,
        ENTOMANCER_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=ENTOMANCER_MONSTER_ID)

    def bees(combat: CombatState) -> None:
        bees_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            ENTOMANCER_DEADLY_BEES_DAMAGE,
            ENTOMANCER_BASE_BEES_DAMAGE,
        )
        bees_hits = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            ENTOMANCER_DEADLY_BEES_REPEAT,
            ENTOMANCER_BASE_BEES_REPEAT,
        )
        _deal_damage_to_player(combat, creature, bees_dmg, hits=bees_hits)

    def spear(combat: CombatState) -> None:
        spear_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            ENTOMANCER_DEADLY_SPEAR_DAMAGE,
            ENTOMANCER_BASE_SPEAR_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, spear_dmg)

    def pheromone_spit(combat: CombatState) -> None:
        if creature.get_power_amount(PowerId.PERSONAL_HIVE) < ENTOMANCER_PERSONAL_HIVE_THRESHOLD:
            creature.apply_power(PowerId.PERSONAL_HIVE, ENTOMANCER_PHEROMONE_HIVE_GAIN)
            creature.apply_power(PowerId.STRENGTH, ENTOMANCER_PHEROMONE_STRENGTH_GAIN)
        else:
            creature.apply_power(PowerId.STRENGTH, ENTOMANCER_MAX_HIVE_STRENGTH_GAIN)

    spear_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        ENTOMANCER_DEADLY_SPEAR_DAMAGE,
        ENTOMANCER_BASE_SPEAR_DAMAGE,
    )
    bees_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        ENTOMANCER_DEADLY_BEES_DAMAGE,
        ENTOMANCER_BASE_BEES_DAMAGE,
    )
    bees_intent_hits = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        ENTOMANCER_DEADLY_BEES_REPEAT,
        ENTOMANCER_BASE_BEES_REPEAT,
    )

    states: dict[str, MonsterState] = {
        ENTOMANCER_BEES_MOVE: MoveState(
            ENTOMANCER_BEES_MOVE,
            bees,
            [multi_attack_intent(bees_intent_damage, bees_intent_hits)],
            follow_up_id=ENTOMANCER_SPEAR_MOVE,
        ),
        ENTOMANCER_SPEAR_MOVE: MoveState(
            ENTOMANCER_SPEAR_MOVE,
            spear,
            [attack_intent(spear_intent_damage)],
            follow_up_id=ENTOMANCER_PHEROMONE_SPIT_MOVE,
        ),
        ENTOMANCER_PHEROMONE_SPIT_MOVE: MoveState(
            ENTOMANCER_PHEROMONE_SPIT_MOVE,
            pheromone_spit,
            [buff_intent()],
            follow_up_id=ENTOMANCER_BEES_MOVE,
        ),
    }

    creature.apply_power(PowerId.PERSONAL_HIVE, ENTOMANCER_INITIAL_PERSONAL_HIVE)
    return creature, MonsterAI(states, ENTOMANCER_BEES_MOVE)


# ---- InfestedPrism (HP 161 / 171 asc) ----

INFESTED_PRISM_MONSTER_ID = "INFESTED_PRISM"
INFESTED_PRISM_BASE_HP = 161
INFESTED_PRISM_TOUGH_HP = 171
INFESTED_PRISM_BASE_JAB_DAMAGE = 15
INFESTED_PRISM_DEADLY_JAB_DAMAGE = 17
INFESTED_PRISM_BASE_RADIATE_DAMAGE = 11
INFESTED_PRISM_DEADLY_RADIATE_DAMAGE = 13
INFESTED_PRISM_BASE_RADIATE_BLOCK = 11
INFESTED_PRISM_DEADLY_RADIATE_BLOCK = 13
INFESTED_PRISM_BASE_WHIRLWIND_DAMAGE = 5
INFESTED_PRISM_DEADLY_WHIRLWIND_DAMAGE = 6
INFESTED_PRISM_WHIRLWIND_REPEAT = 3
INFESTED_PRISM_BASE_PULSATE_BLOCK = 20
INFESTED_PRISM_TOUGH_PULSATE_BLOCK = 22
INFESTED_PRISM_BASE_PULSATE_DAMAGE = 8
INFESTED_PRISM_DEADLY_PULSATE_DAMAGE = 10
INFESTED_PRISM_BASE_VITAL_SPARK = 2
INFESTED_PRISM_DEADLY_VITAL_SPARK = 3
INFESTED_PRISM_JAB_MOVE = "JAB_MOVE"
INFESTED_PRISM_RADIATE_MOVE = "RADIATE_MOVE"
INFESTED_PRISM_WHIRLWIND_MOVE = "WHIRLWIND_MOVE"
INFESTED_PRISM_PULSATE_MOVE = "PULSATE_MOVE"


def create_infested_prism(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        INFESTED_PRISM_TOUGH_HP,
        INFESTED_PRISM_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=INFESTED_PRISM_MONSTER_ID)

    def jab(combat: CombatState) -> None:
        jab_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            INFESTED_PRISM_DEADLY_JAB_DAMAGE,
            INFESTED_PRISM_BASE_JAB_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, jab_dmg)

    def radiate(combat: CombatState) -> None:
        radiate_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            INFESTED_PRISM_DEADLY_RADIATE_DAMAGE,
            INFESTED_PRISM_BASE_RADIATE_DAMAGE,
        )
        radiate_block = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            INFESTED_PRISM_DEADLY_RADIATE_BLOCK,
            INFESTED_PRISM_BASE_RADIATE_BLOCK,
        )
        _deal_damage_to_player(combat, creature, radiate_dmg)
        _gain_block(creature, radiate_block, combat)

    def whirlwind(combat: CombatState) -> None:
        whirlwind_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            INFESTED_PRISM_DEADLY_WHIRLWIND_DAMAGE,
            INFESTED_PRISM_BASE_WHIRLWIND_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, whirlwind_dmg, hits=INFESTED_PRISM_WHIRLWIND_REPEAT)

    def pulsate(combat: CombatState) -> None:
        pulsate_block = _ascension_value(
            _combat_ascension_level(combat),
            TOUGH_ENEMIES_ASCENSION_LEVEL,
            INFESTED_PRISM_TOUGH_PULSATE_BLOCK,
            INFESTED_PRISM_BASE_PULSATE_BLOCK,
        )
        pulsate_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            INFESTED_PRISM_DEADLY_PULSATE_DAMAGE,
            INFESTED_PRISM_BASE_PULSATE_DAMAGE,
        )
        vital_spark = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            INFESTED_PRISM_DEADLY_VITAL_SPARK,
            INFESTED_PRISM_BASE_VITAL_SPARK,
        )
        _deal_damage_to_player(combat, creature, pulsate_dmg)
        _gain_block(creature, pulsate_block, combat)
        creature.apply_power(PowerId.VITAL_SPARK, vital_spark)

    jab_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        INFESTED_PRISM_DEADLY_JAB_DAMAGE,
        INFESTED_PRISM_BASE_JAB_DAMAGE,
    )
    radiate_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        INFESTED_PRISM_DEADLY_RADIATE_DAMAGE,
        INFESTED_PRISM_BASE_RADIATE_DAMAGE,
    )
    whirlwind_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        INFESTED_PRISM_DEADLY_WHIRLWIND_DAMAGE,
        INFESTED_PRISM_BASE_WHIRLWIND_DAMAGE,
    )
    pulsate_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        INFESTED_PRISM_DEADLY_PULSATE_DAMAGE,
        INFESTED_PRISM_BASE_PULSATE_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        INFESTED_PRISM_JAB_MOVE: MoveState(
            INFESTED_PRISM_JAB_MOVE,
            jab,
            [attack_intent(jab_intent_damage)],
            follow_up_id=INFESTED_PRISM_RADIATE_MOVE,
        ),
        INFESTED_PRISM_RADIATE_MOVE: MoveState(
            INFESTED_PRISM_RADIATE_MOVE,
            radiate,
            [attack_intent(radiate_intent_damage), defend_intent()],
            follow_up_id=INFESTED_PRISM_WHIRLWIND_MOVE,
        ),
        INFESTED_PRISM_WHIRLWIND_MOVE: MoveState(
            INFESTED_PRISM_WHIRLWIND_MOVE,
            whirlwind,
            [multi_attack_intent(whirlwind_intent_damage, INFESTED_PRISM_WHIRLWIND_REPEAT)],
            follow_up_id=INFESTED_PRISM_PULSATE_MOVE,
        ),
        INFESTED_PRISM_PULSATE_MOVE: MoveState(
            INFESTED_PRISM_PULSATE_MOVE,
            pulsate,
            [attack_intent(pulsate_intent_damage), buff_intent(), defend_intent()],
            follow_up_id=INFESTED_PRISM_JAB_MOVE,
        ),
    }

    initial_vital_spark = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        INFESTED_PRISM_DEADLY_VITAL_SPARK,
        INFESTED_PRISM_BASE_VITAL_SPARK,
    )
    creature.apply_power(PowerId.VITAL_SPARK, initial_vital_spark)
    return creature, MonsterAI(states, INFESTED_PRISM_JAB_MOVE)


# ========================================================================
# BOSS ENCOUNTERS
# ========================================================================

# ---- TheInsatiable (HP 321 / 341 asc) ----

THE_INSATIABLE_MONSTER_ID = "THE_INSATIABLE"
THE_INSATIABLE_BASE_HP = 321
THE_INSATIABLE_TOUGH_HP = 341
THE_INSATIABLE_BASE_THRASH_DAMAGE = 8
THE_INSATIABLE_DEADLY_THRASH_DAMAGE = 9
THE_INSATIABLE_THRASH_REPEAT = 2
THE_INSATIABLE_BASE_BITE_DAMAGE = 28
THE_INSATIABLE_DEADLY_BITE_DAMAGE = 31
THE_INSATIABLE_BASE_SALIVATE_STRENGTH = 2
THE_INSATIABLE_DEADLY_SALIVATE_STRENGTH = 3
THE_INSATIABLE_SANDPIT_AMOUNT = 4
THE_INSATIABLE_LIQUIFY_DRAW_COUNT = 3
THE_INSATIABLE_LIQUIFY_DISCARD_COUNT = 3
THE_INSATIABLE_FRANTIC_ESCAPE_STATUS = "FRANTIC_ESCAPE"
THE_INSATIABLE_LIQUIFY_GROUND_MOVE = "LIQUIFY_GROUND_MOVE"
THE_INSATIABLE_THRASH_MOVE_1 = "THRASH_MOVE_1"
THE_INSATIABLE_THRASH_MOVE_2 = "THRASH_MOVE_2"
THE_INSATIABLE_LUNGING_BITE_MOVE = "LUNGING_BITE_MOVE"
THE_INSATIABLE_SALIVATE_MOVE = "SALIVATE_MOVE"


def create_the_insatiable(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        THE_INSATIABLE_TOUGH_HP,
        THE_INSATIABLE_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=THE_INSATIABLE_MONSTER_ID)

    def liquify_ground(combat: CombatState) -> None:
        for target in living_player_targets(combat):
            sandpit = SandpitPower(THE_INSATIABLE_SANDPIT_AMOUNT)
            sandpit.set_target(target)
            existing = creature.powers.get(PowerId.SANDPIT)
            if isinstance(existing, SandpitPower):
                existing.add_instance(THE_INSATIABLE_SANDPIT_AMOUNT, target)
            else:
                creature.powers[PowerId.SANDPIT] = sandpit
            combat.add_status_cards_to_draw(
                target,
                THE_INSATIABLE_FRANTIC_ESCAPE_STATUS,
                THE_INSATIABLE_LIQUIFY_DRAW_COUNT,
                random_position=True,
            )
            combat.add_status_cards_to_discard(
                target,
                THE_INSATIABLE_FRANTIC_ESCAPE_STATUS,
                THE_INSATIABLE_LIQUIFY_DISCARD_COUNT,
            )

    def thrash(combat: CombatState) -> None:
        thrash_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            THE_INSATIABLE_DEADLY_THRASH_DAMAGE,
            THE_INSATIABLE_BASE_THRASH_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, thrash_dmg, hits=THE_INSATIABLE_THRASH_REPEAT)

    def lunging_bite(combat: CombatState) -> None:
        bite_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            THE_INSATIABLE_DEADLY_BITE_DAMAGE,
            THE_INSATIABLE_BASE_BITE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, bite_dmg)

    def salivate(combat: CombatState) -> None:
        salivate_str = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            THE_INSATIABLE_DEADLY_SALIVATE_STRENGTH,
            THE_INSATIABLE_BASE_SALIVATE_STRENGTH,
        )
        creature.apply_power(PowerId.STRENGTH, salivate_str, applier=creature)

    thrash_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        THE_INSATIABLE_DEADLY_THRASH_DAMAGE,
        THE_INSATIABLE_BASE_THRASH_DAMAGE,
    )
    bite_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        THE_INSATIABLE_DEADLY_BITE_DAMAGE,
        THE_INSATIABLE_BASE_BITE_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        THE_INSATIABLE_LIQUIFY_GROUND_MOVE: MoveState(
            THE_INSATIABLE_LIQUIFY_GROUND_MOVE,
            liquify_ground,
            [buff_intent(), status_intent()],
            follow_up_id=THE_INSATIABLE_THRASH_MOVE_1,
        ),
        THE_INSATIABLE_THRASH_MOVE_1: MoveState(
            THE_INSATIABLE_THRASH_MOVE_1,
            thrash,
            [multi_attack_intent(thrash_intent_damage, THE_INSATIABLE_THRASH_REPEAT)],
            follow_up_id=THE_INSATIABLE_LUNGING_BITE_MOVE,
        ),
        THE_INSATIABLE_LUNGING_BITE_MOVE: MoveState(
            THE_INSATIABLE_LUNGING_BITE_MOVE,
            lunging_bite,
            [attack_intent(bite_intent_damage)],
            follow_up_id=THE_INSATIABLE_SALIVATE_MOVE,
        ),
        THE_INSATIABLE_SALIVATE_MOVE: MoveState(
            THE_INSATIABLE_SALIVATE_MOVE,
            salivate,
            [buff_intent()],
            follow_up_id=THE_INSATIABLE_THRASH_MOVE_2,
        ),
        THE_INSATIABLE_THRASH_MOVE_2: MoveState(
            THE_INSATIABLE_THRASH_MOVE_2,
            thrash,
            [multi_attack_intent(thrash_intent_damage, THE_INSATIABLE_THRASH_REPEAT)],
            follow_up_id=THE_INSATIABLE_THRASH_MOVE_1,
        ),
    }
    return creature, MonsterAI(states, THE_INSATIABLE_LIQUIFY_GROUND_MOVE)


# ---- KnowledgeDemon (HP 379 / 399 asc) ----
# C# cycle: CURSE_OF_KNOWLEDGE -> SLAP(17) -> KNOWLEDGE_OVERWHELMING(8x3) -> PONDER(11+heal30+str2) -> conditional

KNOWLEDGE_DEMON_MONSTER_ID = "KNOWLEDGE_DEMON"
KNOWLEDGE_DEMON_BASE_HP = 379
KNOWLEDGE_DEMON_TOUGH_HP = 399
KNOWLEDGE_DEMON_BASE_SLAP_DAMAGE = 17
KNOWLEDGE_DEMON_DEADLY_SLAP_DAMAGE = 18
KNOWLEDGE_DEMON_BASE_OVERWHELMING_DAMAGE = 8
KNOWLEDGE_DEMON_DEADLY_OVERWHELMING_DAMAGE = 9
KNOWLEDGE_DEMON_OVERWHELMING_REPEAT = 3
KNOWLEDGE_DEMON_BASE_PONDER_DAMAGE = 11
KNOWLEDGE_DEMON_DEADLY_PONDER_DAMAGE = 13
KNOWLEDGE_DEMON_PONDER_HEAL = 30
KNOWLEDGE_DEMON_BASE_PONDER_STRENGTH = 2
KNOWLEDGE_DEMON_DEADLY_PONDER_STRENGTH = 3
KNOWLEDGE_DEMON_CURSE_COUNTER_KEY = "curse_counter"
KNOWLEDGE_DEMON_INITIAL_CURSE_COUNTER = 0
KNOWLEDGE_DEMON_DISINTEGRATION_DAMAGE_VALUES = (6, 7, 8)
KNOWLEDGE_DEMON_CURSE_CHOICE_PROMPT = "Curse of Knowledge"
KNOWLEDGE_DEMON_CHOICE_SOURCE_PILE = "knowledge_demon"
KNOWLEDGE_DEMON_CURSE_OF_KNOWLEDGE_MOVE = "CURSE_OF_KNOWLEDGE_MOVE"
KNOWLEDGE_DEMON_SLAP_MOVE = "SLAP_MOVE"
KNOWLEDGE_DEMON_KNOWLEDGE_OVERWHELMING_MOVE = "KNOWLEDGE_OVERWHELMING_MOVE"
KNOWLEDGE_DEMON_PONDER_MOVE = "PONDER_MOVE"
KNOWLEDGE_DEMON_CURSE_OF_KNOWLEDGE_BRANCH = "CurseOfKnowledgeBranch"


def create_knowledge_demon(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        KNOWLEDGE_DEMON_TOUGH_HP,
        KNOWLEDGE_DEMON_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=KNOWLEDGE_DEMON_MONSTER_ID)

    _state = {KNOWLEDGE_DEMON_CURSE_COUNTER_KEY: KNOWLEDGE_DEMON_INITIAL_CURSE_COUNTER}
    curse_sets = (
        (make_disintegration, make_mind_rot),
        (make_disintegration, make_sloth_status),
        (make_disintegration, make_waste_away),
    )

    def request_knowledge_choice(combat: CombatState, targets: list[Creature], index: int) -> None:
        if index >= len(targets):
            _state[KNOWLEDGE_DEMON_CURSE_COUNTER_KEY] += 1
            return
        target = targets[index]
        counter = _state[KNOWLEDGE_DEMON_CURSE_COUNTER_KEY]
        cards = [factory() for factory in curse_sets[counter]]
        for card in cards:
            card.owner = target
            if card.card_id == CardId.DISINTEGRATION:
                card.effect_vars["disintegration_power"] = KNOWLEDGE_DEMON_DISINTEGRATION_DAMAGE_VALUES[counter]

        def resolver(selected) -> None:
            if selected is not None:
                selected.on_chosen(combat)
            request_knowledge_choice(combat, targets, index + 1)

        combat.request_card_choice(
            prompt=KNOWLEDGE_DEMON_CURSE_CHOICE_PROMPT,
            cards=cards,
            source_pile=KNOWLEDGE_DEMON_CHOICE_SOURCE_PILE,
            resolver=resolver,
            owner=target,
        )

    def curse_of_knowledge(combat: CombatState) -> None:
        counter = _state[KNOWLEDGE_DEMON_CURSE_COUNTER_KEY]
        if counter >= len(curse_sets):
            raise RuntimeError(f"No Curse of Knowledge set at index {counter}")
        targets = [state.creature for state in combat.combat_player_states if state.creature.is_alive]
        request_knowledge_choice(combat, targets, 0)

    def slap(combat: CombatState) -> None:
        slap_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            KNOWLEDGE_DEMON_DEADLY_SLAP_DAMAGE,
            KNOWLEDGE_DEMON_BASE_SLAP_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, slap_dmg)

    def knowledge_overwhelming(combat: CombatState) -> None:
        overwhelming_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            KNOWLEDGE_DEMON_DEADLY_OVERWHELMING_DAMAGE,
            KNOWLEDGE_DEMON_BASE_OVERWHELMING_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, overwhelming_dmg, hits=KNOWLEDGE_DEMON_OVERWHELMING_REPEAT)

    def ponder(combat: CombatState) -> None:
        ponder_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            KNOWLEDGE_DEMON_DEADLY_PONDER_DAMAGE,
            KNOWLEDGE_DEMON_BASE_PONDER_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, ponder_dmg)
        if combat.is_over:
            return
        creature.heal(KNOWLEDGE_DEMON_PONDER_HEAL * len(combat.combat_player_states))
        ponder_str = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            KNOWLEDGE_DEMON_DEADLY_PONDER_STRENGTH,
            KNOWLEDGE_DEMON_BASE_PONDER_STRENGTH,
        )
        combat.apply_power_to(creature, PowerId.STRENGTH, ponder_str, applier=creature)

    # After Ponder: if curse_counter < 3, go back to CURSE_OF_KNOWLEDGE; else SLAP
    curse_check = ConditionalBranchState(KNOWLEDGE_DEMON_CURSE_OF_KNOWLEDGE_BRANCH)
    curse_check.add_branch(
        lambda: _state[KNOWLEDGE_DEMON_CURSE_COUNTER_KEY] < len(curse_sets),
        KNOWLEDGE_DEMON_CURSE_OF_KNOWLEDGE_MOVE,
    )
    curse_check.add_branch(lambda: True, KNOWLEDGE_DEMON_SLAP_MOVE)

    slap_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        KNOWLEDGE_DEMON_DEADLY_SLAP_DAMAGE,
        KNOWLEDGE_DEMON_BASE_SLAP_DAMAGE,
    )
    overwhelming_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        KNOWLEDGE_DEMON_DEADLY_OVERWHELMING_DAMAGE,
        KNOWLEDGE_DEMON_BASE_OVERWHELMING_DAMAGE,
    )
    ponder_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        KNOWLEDGE_DEMON_DEADLY_PONDER_DAMAGE,
        KNOWLEDGE_DEMON_BASE_PONDER_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        KNOWLEDGE_DEMON_CURSE_OF_KNOWLEDGE_MOVE: MoveState(
            KNOWLEDGE_DEMON_CURSE_OF_KNOWLEDGE_MOVE,
            curse_of_knowledge,
            [debuff_intent()],
            follow_up_id=KNOWLEDGE_DEMON_SLAP_MOVE,
        ),
        KNOWLEDGE_DEMON_SLAP_MOVE: MoveState(
            KNOWLEDGE_DEMON_SLAP_MOVE,
            slap,
            [attack_intent(slap_intent_damage)],
            follow_up_id=KNOWLEDGE_DEMON_KNOWLEDGE_OVERWHELMING_MOVE,
        ),
        KNOWLEDGE_DEMON_KNOWLEDGE_OVERWHELMING_MOVE: MoveState(
            KNOWLEDGE_DEMON_KNOWLEDGE_OVERWHELMING_MOVE,
            knowledge_overwhelming,
            [multi_attack_intent(overwhelming_intent_damage, KNOWLEDGE_DEMON_OVERWHELMING_REPEAT)],
            follow_up_id=KNOWLEDGE_DEMON_PONDER_MOVE,
        ),
        KNOWLEDGE_DEMON_PONDER_MOVE: MoveState(
            KNOWLEDGE_DEMON_PONDER_MOVE,
            ponder,
            [attack_intent(ponder_intent_damage), buff_intent()],
            follow_up_id=KNOWLEDGE_DEMON_CURSE_OF_KNOWLEDGE_BRANCH,
        ),
        KNOWLEDGE_DEMON_CURSE_OF_KNOWLEDGE_BRANCH: curse_check,
    }
    return creature, MonsterAI(states, KNOWLEDGE_DEMON_CURSE_OF_KNOWLEDGE_MOVE)


# ---- KaiserCrab (Crusher + Rocket) ----

CRUSHER_MONSTER_ID = "CRUSHER"
CRUSHER_BASE_HP = 209
CRUSHER_TOUGH_HP = 219
CRUSHER_BASE_THRASH_DAMAGE = 12
CRUSHER_DEADLY_THRASH_DAMAGE = 14
CRUSHER_ENLARGING_STRIKE_DAMAGE = 4
CRUSHER_BASE_BUG_STING_DAMAGE = 6
CRUSHER_DEADLY_BUG_STING_DAMAGE = 7
CRUSHER_BUG_STING_REPEAT = 2
CRUSHER_BUG_STING_DEBUFF = 2
CRUSHER_BASE_ADAPT_STRENGTH = 2
CRUSHER_DEADLY_ADAPT_STRENGTH = 3
CRUSHER_BASE_GUARDED_STRIKE_DAMAGE = 12
CRUSHER_DEADLY_GUARDED_STRIKE_DAMAGE = 14
CRUSHER_GUARDED_STRIKE_BLOCK = 18
CRUSHER_BACK_ATTACK_LEFT_AMOUNT = 1
CRUSHER_CRAB_RAGE_AMOUNT = 1
CRUSHER_THRASH_MOVE = "THRASH_MOVE"
CRUSHER_ENLARGING_STRIKE_MOVE = "ENLARGING_STRIKE_MOVE"
CRUSHER_BUG_STING_MOVE = "BUG_STING_MOVE"
CRUSHER_ADAPT_MOVE = "ADAPT_MOVE"
CRUSHER_GUARDED_STRIKE_MOVE = "GUARDED_STRIKE_MOVE"


def create_crusher(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        CRUSHER_TOUGH_HP,
        CRUSHER_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=CRUSHER_MONSTER_ID)

    def thrash(combat: CombatState) -> None:
        thrash_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            CRUSHER_DEADLY_THRASH_DAMAGE,
            CRUSHER_BASE_THRASH_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, thrash_dmg)

    def enlarging_strike(combat: CombatState) -> None:
        _deal_damage_to_player(combat, creature, CRUSHER_ENLARGING_STRIKE_DAMAGE)

    def bug_sting(combat: CombatState) -> None:
        bug_sting_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            CRUSHER_DEADLY_BUG_STING_DAMAGE,
            CRUSHER_BASE_BUG_STING_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, bug_sting_dmg, hits=CRUSHER_BUG_STING_REPEAT)
        apply_power_to_living_player_targets(combat, PowerId.WEAK, CRUSHER_BUG_STING_DEBUFF, applier=creature)
        apply_power_to_living_player_targets(combat, PowerId.FRAIL, CRUSHER_BUG_STING_DEBUFF, applier=creature)

    def adapt(combat: CombatState) -> None:
        adapt_strength = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            CRUSHER_DEADLY_ADAPT_STRENGTH,
            CRUSHER_BASE_ADAPT_STRENGTH,
        )
        creature.apply_power(PowerId.STRENGTH, adapt_strength)

    def guarded_strike(combat: CombatState) -> None:
        guarded_strike_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            CRUSHER_DEADLY_GUARDED_STRIKE_DAMAGE,
            CRUSHER_BASE_GUARDED_STRIKE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, guarded_strike_dmg)
        _gain_block(creature, CRUSHER_GUARDED_STRIKE_BLOCK, combat)

    thrash_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        CRUSHER_DEADLY_THRASH_DAMAGE,
        CRUSHER_BASE_THRASH_DAMAGE,
    )
    bug_sting_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        CRUSHER_DEADLY_BUG_STING_DAMAGE,
        CRUSHER_BASE_BUG_STING_DAMAGE,
    )
    guarded_strike_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        CRUSHER_DEADLY_GUARDED_STRIKE_DAMAGE,
        CRUSHER_BASE_GUARDED_STRIKE_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        CRUSHER_THRASH_MOVE: MoveState(
            CRUSHER_THRASH_MOVE,
            thrash,
            [attack_intent(thrash_intent_damage)],
            follow_up_id=CRUSHER_ENLARGING_STRIKE_MOVE,
        ),
        CRUSHER_ENLARGING_STRIKE_MOVE: MoveState(
            CRUSHER_ENLARGING_STRIKE_MOVE,
            enlarging_strike,
            [attack_intent(CRUSHER_ENLARGING_STRIKE_DAMAGE)],
            follow_up_id=CRUSHER_BUG_STING_MOVE,
        ),
        CRUSHER_BUG_STING_MOVE: MoveState(
            CRUSHER_BUG_STING_MOVE,
            bug_sting,
            [multi_attack_intent(bug_sting_intent_damage, CRUSHER_BUG_STING_REPEAT), debuff_intent()],
            follow_up_id=CRUSHER_ADAPT_MOVE,
        ),
        CRUSHER_ADAPT_MOVE: MoveState(
            CRUSHER_ADAPT_MOVE,
            adapt,
            [buff_intent()],
            follow_up_id=CRUSHER_GUARDED_STRIKE_MOVE,
        ),
        CRUSHER_GUARDED_STRIKE_MOVE: MoveState(
            CRUSHER_GUARDED_STRIKE_MOVE,
            guarded_strike,
            [attack_intent(guarded_strike_intent_damage), defend_intent()],
            follow_up_id=CRUSHER_THRASH_MOVE,
        ),
    }

    creature.apply_power(PowerId.BACK_ATTACK_LEFT, CRUSHER_BACK_ATTACK_LEFT_AMOUNT)
    creature.apply_power(PowerId.CRAB_RAGE, CRUSHER_CRAB_RAGE_AMOUNT)
    return creature, MonsterAI(states, CRUSHER_THRASH_MOVE)


ROCKET_MONSTER_ID = "ROCKET"
ROCKET_BASE_HP = 199
ROCKET_TOUGH_HP = 209
ROCKET_BASE_TARGETING_RETICLE_DAMAGE = 3
ROCKET_DEADLY_TARGETING_RETICLE_DAMAGE = 4
ROCKET_BASE_PRECISION_BEAM_DAMAGE = 18
ROCKET_DEADLY_PRECISION_BEAM_DAMAGE = 20
ROCKET_BASE_LASER_DAMAGE = 31
ROCKET_DEADLY_LASER_DAMAGE = 35
ROCKET_BASE_CHARGE_UP_STRENGTH = 2
ROCKET_DEADLY_CHARGE_UP_STRENGTH = 3
ROCKET_SURROUNDED_AMOUNT = 1
ROCKET_BACK_ATTACK_RIGHT_AMOUNT = 1
ROCKET_CRAB_RAGE_AMOUNT = 1
ROCKET_TARGETING_RETICLE_MOVE = "TARGETING_RETICLE_MOVE"
ROCKET_PRECISION_BEAM_MOVE = "PRECISION_BEAM_MOVE"
ROCKET_CHARGE_UP_MOVE = "CHARGE_UP_MOVE"
ROCKET_LASER_MOVE = "LASER_MOVE"
ROCKET_RECHARGE_MOVE = "RECHARGE_MOVE"


def create_rocket(rng: Rng, ascension_level: int = 0) -> tuple[Creature, MonsterAI]:
    hp = _ascension_value(
        ascension_level,
        TOUGH_ENEMIES_ASCENSION_LEVEL,
        ROCKET_TOUGH_HP,
        ROCKET_BASE_HP,
    )
    creature = Creature(max_hp=hp, monster_id=ROCKET_MONSTER_ID)

    def targeting_reticle(combat: CombatState) -> None:
        targeting_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            ROCKET_DEADLY_TARGETING_RETICLE_DAMAGE,
            ROCKET_BASE_TARGETING_RETICLE_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, targeting_dmg)

    def precision_beam(combat: CombatState) -> None:
        precision_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            ROCKET_DEADLY_PRECISION_BEAM_DAMAGE,
            ROCKET_BASE_PRECISION_BEAM_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, precision_dmg)

    def charge_up(combat: CombatState) -> None:
        charge_up_strength = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            ROCKET_DEADLY_CHARGE_UP_STRENGTH,
            ROCKET_BASE_CHARGE_UP_STRENGTH,
        )
        creature.apply_power(PowerId.STRENGTH, charge_up_strength)

    def laser(combat: CombatState) -> None:
        laser_dmg = _ascension_value(
            _combat_ascension_level(combat),
            DEADLY_ENEMIES_ASCENSION_LEVEL,
            ROCKET_DEADLY_LASER_DAMAGE,
            ROCKET_BASE_LASER_DAMAGE,
        )
        _deal_damage_to_player(combat, creature, laser_dmg)

    def recharge(combat: CombatState) -> None:
        return

    targeting_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        ROCKET_DEADLY_TARGETING_RETICLE_DAMAGE,
        ROCKET_BASE_TARGETING_RETICLE_DAMAGE,
    )
    precision_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        ROCKET_DEADLY_PRECISION_BEAM_DAMAGE,
        ROCKET_BASE_PRECISION_BEAM_DAMAGE,
    )
    laser_intent_damage = _ascension_value(
        ascension_level,
        DEADLY_ENEMIES_ASCENSION_LEVEL,
        ROCKET_DEADLY_LASER_DAMAGE,
        ROCKET_BASE_LASER_DAMAGE,
    )

    states: dict[str, MonsterState] = {
        ROCKET_TARGETING_RETICLE_MOVE: MoveState(
            ROCKET_TARGETING_RETICLE_MOVE,
            targeting_reticle,
            [attack_intent(targeting_intent_damage)],
            follow_up_id=ROCKET_PRECISION_BEAM_MOVE,
        ),
        ROCKET_PRECISION_BEAM_MOVE: MoveState(
            ROCKET_PRECISION_BEAM_MOVE,
            precision_beam,
            [attack_intent(precision_intent_damage)],
            follow_up_id=ROCKET_CHARGE_UP_MOVE,
        ),
        ROCKET_CHARGE_UP_MOVE: MoveState(
            ROCKET_CHARGE_UP_MOVE,
            charge_up,
            [buff_intent()],
            follow_up_id=ROCKET_LASER_MOVE,
        ),
        ROCKET_LASER_MOVE: MoveState(
            ROCKET_LASER_MOVE,
            laser,
            [attack_intent(laser_intent_damage)],
            follow_up_id=ROCKET_RECHARGE_MOVE,
        ),
        ROCKET_RECHARGE_MOVE: MoveState(
            ROCKET_RECHARGE_MOVE,
            recharge,
            [sleep_intent()],
            follow_up_id=ROCKET_TARGETING_RETICLE_MOVE,
        ),
    }

    creature.apply_power(PowerId.BACK_ATTACK_RIGHT, ROCKET_BACK_ATTACK_RIGHT_AMOUNT)
    creature.apply_power(PowerId.CRAB_RAGE, ROCKET_CRAB_RAGE_AMOUNT)
    return creature, MonsterAI(states, ROCKET_TARGETING_RETICLE_MOVE)
