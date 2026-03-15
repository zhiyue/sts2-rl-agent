"""Creature: HP, Block, Powers management."""

from __future__ import annotations

from typing import TYPE_CHECKING

from sts2_env.core.enums import CombatSide, PowerId, PowerType
from sts2_env.core.constants import BLOCK_CAP

if TYPE_CHECKING:
    from sts2_env.powers.base import PowerInstance


# Power class registry — populated by powers/common.py and other power modules
_POWER_CLASSES: dict[PowerId, type] = {}


def register_power_class(power_id: PowerId, cls: type) -> None:
    """Register a power class for a given PowerId."""
    _POWER_CLASSES[power_id] = cls


def get_power_class(power_id: PowerId) -> type | None:
    return _POWER_CLASSES.get(power_id)


class Creature:
    """A combat entity (player or monster) with HP, Block, and Powers."""

    __slots__ = (
        "max_hp", "current_hp", "block", "powers", "side",
        "is_player", "monster_id", "combat_id", "stars",
    )

    def __init__(
        self,
        max_hp: int,
        current_hp: int | None = None,
        side: CombatSide = CombatSide.PLAYER,
        is_player: bool = False,
        monster_id: str | None = None,
        combat_id: int = 0,
    ):
        self.max_hp = max_hp
        self.current_hp = current_hp if current_hp is not None else max_hp
        self.block: int = 0
        self.powers: dict[PowerId, PowerInstance] = {}
        self.side = side
        self.is_player = is_player
        self.monster_id = monster_id
        self.combat_id = combat_id
        self.stars: int = 0

    @property
    def is_alive(self) -> bool:
        return self.current_hp > 0

    @property
    def is_dead(self) -> bool:
        return self.current_hp <= 0

    def gain_block(self, amount: int, unpowered: bool = False) -> None:
        if amount <= 0:
            return
        self.block = min(self.block + amount, BLOCK_CAP)

    def damage_block(self, amount: int, unblockable: bool = False) -> int:
        """Block absorbs damage. Returns amount blocked."""
        if unblockable or self.block <= 0:
            return 0
        blocked = min(self.block, amount)
        self.block -= blocked
        return blocked

    def lose_hp(self, amount: int) -> int:
        """Lose HP, returns actual HP lost."""
        if amount <= 0:
            return 0
        actual = min(amount, self.current_hp)
        self.current_hp = max(0, self.current_hp - amount)
        return actual

    def heal(self, amount: int) -> int:
        """Heal HP, capped at max. Returns actual healed."""
        if amount <= 0 or self.is_dead:
            return 0
        before = self.current_hp
        self.current_hp = min(self.current_hp + amount, self.max_hp)
        return self.current_hp - before

    def clear_block(self, combat: object | None = None) -> None:
        """Clear block unless a power prevents it (Barricade).

        When combat is provided, uses the full hook dispatch (all hook listeners).
        Otherwise falls back to checking only own powers.
        """
        if combat is not None:
            from sts2_env.core.hooks import should_clear_block
            if not should_clear_block(self, combat):
                return
        else:
            for p in self.powers.values():
                result = p.should_clear_block(self, self)
                if result is False:
                    return
        self.block = 0

    def get_power_amount(self, power_id: PowerId) -> int:
        p = self.powers.get(power_id)
        return p.amount if p else 0

    def has_power(self, power_id: PowerId) -> bool:
        return power_id in self.powers and self.powers[power_id].amount != 0

    def apply_power(self, power_id: PowerId, amount: int) -> None:
        """Apply or stack a power. Handles Artifact blocking for debuffs."""
        if amount == 0:
            return

        cls = get_power_class(power_id)
        is_debuff = False
        if cls is not None:
            is_debuff = getattr(cls, 'power_type', PowerType.BUFF) == PowerType.DEBUFF

        # Artifact blocks debuffs
        if is_debuff:
            artifact = self.powers.get(PowerId.ARTIFACT)
            if artifact is not None and artifact.try_block_debuff(self, power_id):
                if artifact.amount <= 0:
                    del self.powers[PowerId.ARTIFACT]
                return

        existing = self.powers.get(power_id)
        if existing is not None:
            existing.amount += amount
            if existing.amount == 0 and not existing.allow_negative:
                del self.powers[power_id]
        else:
            if cls is not None:
                power = cls(amount)
                self.powers[power_id] = power

    def tick_down_power(self, power_id: PowerId) -> None:
        """Decrement a duration power by 1, remove if <= 0."""
        p = self.powers.get(power_id)
        if p is None:
            return
        p.on_turn_end_enemy_side(self)
        if p.amount <= 0:
            del self.powers[power_id]

    def gain_max_hp(self, amount: int) -> None:
        self.max_hp += amount
        self.current_hp = min(self.current_hp + amount, self.max_hp)

    def lose_max_hp(self, amount: int) -> None:
        self.max_hp = max(1, self.max_hp - amount)
        self.current_hp = min(self.current_hp, self.max_hp)

    def gain_stars(self, amount: int) -> int:
        if amount <= 0:
            return 0
        self.stars += amount
        return amount

    def lose_stars(self, amount: int) -> int:
        if amount <= 0:
            return 0
        lost = min(self.stars, amount)
        self.stars -= lost
        return lost

    def __repr__(self) -> str:
        powers_str = ", ".join(str(p) for p in self.powers.values()) if self.powers else ""
        name = self.monster_id or ("Player" if self.is_player else "Creature")
        return f"{name}(HP={self.current_hp}/{self.max_hp} Block={self.block}{' ' + powers_str if powers_str else ''})"
