"""Event encounter definitions.

These encounters are triggered by in-game events rather than appearing
in any act's normal/weak/elite/boss pool.  They still follow the same
``EncounterSetup`` signature.

Covers:
- BattlewornDummyEventEncounter (Glory / BattlewornDummy event)
- DenseVegetationEventEncounter (Overgrowth / DenseVegetation event)
- FakeMerchantEventEncounter     (cross-act / FakeMerchant event)
- MysteriousKnightEventEncounter (Hive / TheLanternKey event)
- PunchOffEventEncounter         (Underdocks / PunchOff event)
- TheArchitectEventEncounter     (cross-act / TheArchitect event)
"""

from __future__ import annotations

from typing import Callable, TYPE_CHECKING

from sts2_env.core.rng import Rng

if TYPE_CHECKING:
    from sts2_env.core.combat import CombatState
from sts2_env.monsters.shared import (
    create_architect,
    create_battle_friend_v1,
    create_battle_friend_v2,
    create_battle_friend_v3,
    create_dense_vegetation_wriggler,
    create_fake_merchant_monster,
    create_mysterious_knight,
)
from sts2_env.monsters.act4 import create_punch_construct

EncounterSetup = Callable[..., None]


# ---- BattlewornDummyEventEncounter ----
# The event lets the player choose a setting (1/2/3) which selects the
# BattleFriend variant:  V1 (75 HP), V2 (150 HP), V3 (300 HP).
# We expose one setup per setting so the caller can pick the right one.

def setup_battleworn_dummy_v1(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_battle_friend_v1(rng)
    combat.add_enemy(creature, ai)


def setup_battleworn_dummy_v2(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_battle_friend_v2(rng)
    combat.add_enemy(creature, ai)


def setup_battleworn_dummy_v3(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_battle_friend_v3(rng)
    combat.add_enemy(creature, ai)


# ---- DenseVegetationEventEncounter ----
# Spawns 4 Wrigglers in named slots (wriggler1-4).
# All start unstunned; slot determines initial move alternation.

def setup_dense_vegetation(combat: CombatState, rng: Rng) -> None:
    for slot in ("wriggler1", "wriggler2", "wriggler3", "wriggler4"):
        creature, ai = create_dense_vegetation_wriggler(rng, slot=slot)
        combat.add_enemy(creature, ai)


# ---- FakeMerchantEventEncounter ----
# Single FakeMerchantMonster in the "merchant" slot.

def setup_fake_merchant(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_fake_merchant_monster(rng)
    combat.add_enemy(creature, ai)


# ---- MysteriousKnightEventEncounter ----
# Single MysteriousKnight (FlailKnight with +6 STR / +6 Plating).

def setup_mysterious_knight(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_mysterious_knight(rng)
    combat.add_enemy(creature, ai)


# ---- PunchOffEventEncounter ----
# Two PunchConstructs:
#   - First starts with strong punch, HP reduced by random 2-10
#   - Second starts normally (READY), HP reduced by random 2-10
# The existing create_punch_construct doesn't expose these params,
# so we apply the modifiers after creation.

def setup_punch_off(combat: CombatState, rng: Rng) -> None:
    c1, a1 = create_punch_construct(
        rng,
        starts_with_strong_punch=True,
        starting_hp_reduction=rng.next_int(2, 10),
    )
    combat.add_enemy(c1, a1)

    c2, a2 = create_punch_construct(
        rng,
        starts_with_strong_punch=False,
        starting_hp_reduction=rng.next_int(2, 10),
    )
    combat.add_enemy(c2, a2)


# ---- TheArchitectEventEncounter ----
# Single Architect (9999 HP, does nothing).

def setup_the_architect(combat: CombatState, rng: Rng) -> None:
    creature, ai = create_architect(rng)
    combat.add_enemy(creature, ai)


# ---- Aggregate lists ----

BATTLEWORN_DUMMY_ENCOUNTERS: list[EncounterSetup] = [
    setup_battleworn_dummy_v1,
    setup_battleworn_dummy_v2,
    setup_battleworn_dummy_v3,
]

EVENT_ENCOUNTERS: list[EncounterSetup] = [
    setup_battleworn_dummy_v1,
    setup_battleworn_dummy_v2,
    setup_battleworn_dummy_v3,
    setup_dense_vegetation,
    setup_fake_merchant,
    setup_mysterious_knight,
    setup_punch_off,
    setup_the_architect,
]
