"""Potion system for STS2."""

from sts2_env.potions.base import (
    PotionModel,
    PotionInstance,
    PotionUseEffect,
    register_potion,
    register_potion_effect,
    get_potion_model,
    get_potion_effect,
    all_potion_models,
    normal_pool_models,
    create_potion,
)

# Import all potions to register them
import sts2_env.potions.all  # noqa: F401

# Import effects to register use-effect callbacks
import sts2_env.potions.effects  # noqa: F401
