"""Potion instance base class and registry."""

from __future__ import annotations

from dataclasses import dataclass, field
from typing import Any, Callable, TYPE_CHECKING

from sts2_env.core.enums import PotionRarity, PotionUsageType, PotionTargetType

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

    def is_in_normal_pool(self) -> bool:
        return self.rarity not in (
            PotionRarity.EVENT,
            PotionRarity.TOKEN,
            PotionRarity.NONE,
        )


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


def normal_pool_models() -> list[PotionModel]:
    """All potions eligible for random generation (excludes Event/Token/None)."""
    return [m for m in _POTION_MODELS.values() if m.is_in_normal_pool()]


def create_potion(potion_id: str, slot: int = -1) -> PotionInstance:
    model = _POTION_MODELS[potion_id]
    return PotionInstance(model=model, slot_index=slot)
