"""Shared test fixtures."""

import pytest

# Ensure power registration happens
import sts2_env.powers  # noqa: F401

from sts2_env.cards.base import reset_instance_counter
from sts2_env.cards.ironclad_basic import (
    create_ironclad_starter_deck,
    make_bash,
    make_defend_ironclad,
    make_strike_ironclad,
)
from sts2_env.core.combat import CombatState
from sts2_env.core.creature import Creature
from sts2_env.core.enums import CombatSide
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle


@pytest.fixture(autouse=True)
def reset_ids():
    """Reset card instance counter before each test."""
    reset_instance_counter()
    yield


@pytest.fixture
def player() -> Creature:
    return Creature(max_hp=80, current_hp=80, side=CombatSide.PLAYER, is_player=True)


@pytest.fixture
def enemy() -> Creature:
    return Creature(max_hp=50, current_hp=50, side=CombatSide.ENEMY, monster_id="TEST_ENEMY")


@pytest.fixture
def rng() -> Rng:
    return Rng(42)


@pytest.fixture
def simple_combat(rng) -> CombatState:
    """Combat with Ironclad starter deck vs ShrinkerBeetle."""
    deck = create_ironclad_starter_deck()
    combat = CombatState(player_hp=80, player_max_hp=80, deck=deck, rng_seed=42)
    creature, ai = create_shrinker_beetle(rng)
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat
