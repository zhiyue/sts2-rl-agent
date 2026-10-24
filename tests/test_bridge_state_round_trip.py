"""Round-trip tests for the live-trace replay builder.

combat_state_from_bridge_state rebuilds a simulator CombatState from the
bridge-shaped JSON that combat_state_to_bridge_state produces (and that the
real bridge mod emits). These tests pin the builder against the serializer so
live-trace replay comparisons start from a faithful state.
"""

import sys
from pathlib import Path

import pytest

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

import sts2_env.powers  # noqa: F401
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import PowerId
from sts2_env.core.rng import Rng
from sts2_env.cards.ironclad_basic import create_ironclad_starter_deck, make_bash
from sts2_env.monsters.act1_weak import create_shrinker_beetle
from sts2_env.parity.bridge_replay import (
    combat_state_from_bridge_state,
    combat_state_to_bridge_state,
)


def _make_combat() -> CombatState:
    combat = CombatState(
        player_hp=72,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=42,
        relics=[],
        gold=0,
        character_id="Ironclad",
    )
    creature, ai = create_shrinker_beetle(Rng(42))
    combat.add_enemy(creature, ai)
    return combat


def test_round_trip_preserves_every_bridge_field():
    combat = _make_combat()
    original = combat_state_to_bridge_state(combat)

    rebuilt = combat_state_from_bridge_state(original)
    round_trip = combat_state_to_bridge_state(rebuilt)

    diffs = {k: (original.get(k), round_trip.get(k)) for k in original if original.get(k) != round_trip.get(k)}
    assert diffs == {}


def test_builder_restores_player_powers_and_block():
    combat = _make_combat()
    combat.player.block = 12
    combat.player.apply_power(PowerId.STRENGTH, 3)
    combat.energy = 2
    state = combat_state_to_bridge_state(combat)

    rebuilt = combat_state_from_bridge_state(state)

    assert rebuilt.player.block == 12
    assert rebuilt.player.get_power_amount(PowerId.STRENGTH) == 3
    assert rebuilt.energy == 2


def test_builder_restores_hand_cards():
    combat = _make_combat()
    bash = make_bash()
    combat.hand = [bash]
    state = combat_state_to_bridge_state(combat)

    rebuilt = combat_state_from_bridge_state(state)

    assert [c.card_id.name for c in rebuilt.hand] == ["BASH"]


def test_builder_survives_unknown_ids_without_dropping_enemies():
    state = {
        "type": "combat_action",
        "player": {"hp": 50, "max_hp": 50, "block": 0, "energy": 3},
        "hand": [{"id": "NOT_A_REAL_CARD", "cost": 1}],
        "enemies": [
            {"id": "SHRINKER_BEETLE", "hp": 40, "max_hp": 40, "block": 0, "powers": []},
            {"id": "TOTALLY_FAKE_MONSTER", "hp": 30, "max_hp": 30, "block": 0, "powers": []},
        ],
        "draw_pile_count": 5,
        "discard_pile_count": 2,
        "round": 1,
    }

    rebuilt = combat_state_from_bridge_state(state)

    assert len(rebuilt.enemies) == 2
    assert rebuilt.enemies[0].monster_id == "SHRINKER_BEETLE"
    assert rebuilt.enemies[1].monster_id == "TOTALLY_FAKE_MONSTER"
    assert rebuilt.enemies[1].current_hp == 30
    assert rebuilt.hand == []
    assert len(rebuilt.draw_pile) == 5
    assert len(rebuilt.discard_pile) == 2
