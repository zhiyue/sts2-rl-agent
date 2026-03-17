"""Potion instance base class and registry."""

from __future__ import annotations

from dataclasses import dataclass, field
from typing import Any, Callable, TYPE_CHECKING

from sts2_env.core.enums import PotionRarity, PotionUsageType, PotionTargetType
from sts2_env.core.rng import Rng

if TYPE_CHECKING:
    from sts2_env.core.creature import Creature
    from sts2_env.core.combat import CombatState

# Type alias for potion use-effect callbacks.
# Signature: (combat, user, target_or_none) -> None
PotionUseEffect = Callable[["CombatState", "Creature", "Creature | None"], None]


@dataclass
class PotionModel:
    """Static definition of a potion type."""

    potion_id: str
    rarity: PotionRarity
    usage_type: PotionUsageType
    target_type: PotionTargetType
    is_character_specific: bool = False
    character_pool: str | None = None
    can_be_generated_in_combat: bool = True

    def is_in_normal_pool(self, *, in_combat: bool = False) -> bool:
        if self.rarity in (
            PotionRarity.EVENT,
            PotionRarity.TOKEN,
            PotionRarity.NONE,
        ):
            return False
        if in_combat and not self.can_be_generated_in_combat:
            return False
        return True

    def is_available_for_character(self, character_id: str | None) -> bool:
        if character_id is None or not self.is_character_specific:
            return True
        return self.character_pool == character_id


@dataclass
class PotionInstance:
    """A concrete potion instance held by a player."""

    model: PotionModel
    slot_index: int = -1

    @property
    def potion_id(self) -> str:
        return self.model.potion_id

    @property
    def rarity(self) -> PotionRarity:
        return self.model.rarity

    @property
    def usage_type(self) -> PotionUsageType:
        return self.model.usage_type

    @property
    def target_type(self) -> PotionTargetType:
        return self.model.target_type

    def can_use_in_combat(self) -> bool:
        return self.usage_type in (PotionUsageType.COMBAT_ONLY, PotionUsageType.ANY_TIME)

    def can_use_out_of_combat(self) -> bool:
        return self.usage_type == PotionUsageType.ANY_TIME

    def use(self, combat: CombatState, user: Creature, target: Creature | None = None) -> None:
        """Execute this potion's effect in combat."""
        effect = get_potion_effect(self.potion_id)
        if effect is not None:
            effect(combat, user, target)

    def __repr__(self) -> str:
        return f"Potion({self.potion_id})"


# ── Registry ──────────────────────────────────────────────────────────

_POTION_MODELS: dict[str, PotionModel] = {}
_POTION_EFFECTS: dict[str, PotionUseEffect] = {}


def register_potion(model: PotionModel) -> PotionModel:
    _POTION_MODELS[model.potion_id] = model
    return model


def register_potion_effect(potion_id: str, effect: PotionUseEffect) -> None:
    """Register a use-effect callback for a potion."""
    _POTION_EFFECTS[potion_id] = effect


def get_potion_model(potion_id: str) -> PotionModel | None:
    return _POTION_MODELS.get(potion_id)


def get_potion_effect(potion_id: str) -> PotionUseEffect | None:
    return _POTION_EFFECTS.get(potion_id)


def all_potion_models() -> list[PotionModel]:
    return list(_POTION_MODELS.values())


def normal_pool_models(
    *,
    in_combat: bool = False,
    character_id: str | None = None,
) -> list[PotionModel]:
    """All potions eligible for random generation (excludes Event/Token/None)."""
    return [
        model
        for model in _POTION_MODELS.values()
        if model.is_in_normal_pool(in_combat=in_combat)
        and model.is_available_for_character(character_id)
    ]


def roll_random_potion_model(
    rng: Rng,
    *,
    character_id: str | None = None,
    in_combat: bool,
) -> PotionModel | None:
    models = normal_pool_models(in_combat=in_combat, character_id=character_id)
    if not models:
        return None

    roll = rng.next_float()
    if roll <= 0.10:
        rarity = PotionRarity.RARE
    elif roll <= 0.35:
        rarity = PotionRarity.UNCOMMON
    else:
        rarity = PotionRarity.COMMON

    rarity_models = [model for model in models if model.rarity == rarity]
    if not rarity_models:
        rarity_models = models
    return rng.choice(rarity_models)


def create_potion(potion_id: str, slot: int = -1) -> PotionInstance:
    model = _POTION_MODELS[potion_id]
    return PotionInstance(model=model, slot_index=slot)
