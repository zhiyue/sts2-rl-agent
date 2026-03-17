"""Focused tests for combat potion actions in the fixed RL action space."""

from __future__ import annotations

import numpy as np

from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.core.combat import CombatState
from sts2_env.core.constants import POTION_ACTION_START, POTION_TARGET_OPTIONS
from sts2_env.core.rng import Rng
from sts2_env.gym_env.action_space import action_to_potion_and_target, get_action_mask
from sts2_env.gym_env.combat_env import STS2CombatEnv
from sts2_env.monsters.act1_weak import create_shrinker_beetle
from sts2_env.potions.base import create_potion


def _make_combat() -> CombatState:
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=42,
        character_id="Ironclad",
    )
    creature, ai = create_shrinker_beetle(Rng(42))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


def test_action_to_potion_and_target_decodes_slot_major_layout() -> None:
    assert action_to_potion_and_target(POTION_ACTION_START) == (0, None)
    assert action_to_potion_and_target(POTION_ACTION_START + 1) == (0, 0)
    assert action_to_potion_and_target(POTION_ACTION_START + POTION_TARGET_OPTIONS) == (1, None)


def test_get_action_mask_marks_self_and_enemy_targeted_potions() -> None:
    combat = _make_combat()
    combat.potions = [create_potion("BlockPotion"), create_potion("FirePotion"), None]
    combat.max_potion_slots = 3

    mask = get_action_mask(combat)

    block_action = POTION_ACTION_START
    fire_base = POTION_ACTION_START + POTION_TARGET_OPTIONS

    assert mask[block_action] == 1
    assert mask[fire_base] == 0
    assert mask[fire_base + 1] == 1


def test_combat_env_step_uses_potion_actions() -> None:
    env = STS2CombatEnv()
    env.reset(seed=7)
    assert env.combat is not None
    env.combat.potions = [create_potion("FirePotion"), None, None]
    env.combat.max_potion_slots = 3
    enemy = env.combat.enemies[0]
    starting_hp = enemy.current_hp

    action = POTION_ACTION_START + 1
    _, _, terminated, truncated, info = env.step(action)

    assert enemy.current_hp == starting_hp - 20
    assert env.combat.potions[0] is None
    assert not terminated
    assert not truncated
    assert isinstance(info["action_mask"], np.ndarray)
