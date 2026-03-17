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
        "is_pet", "pet_owner", "is_osty", "owner", "combat_state", "escaped",
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
        self.is_pet: bool = False
        self.pet_owner: Creature | None = None
        self.is_osty: bool = False
        self.owner: Creature | None = None
        self.combat_state = None
        self.escaped: bool = False

    @property
    def is_alive(self) -> bool:
        return self.current_hp > 0 and not self.escaped

    @property
    def is_dead(self) -> bool:
        return self.current_hp <= 0

    @property
    def creature(self) -> Creature:
        """Compatibility shim for code paths that expect owner.creature."""
        return self

    @property
    def is_pet_of(self) -> Creature | None:
        return self.pet_owner

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

    def apply_power(
        self,
        power_id: PowerId,
        amount: int,
        *,
        applier: Creature | None = None,
        source: object | None = None,
    ) -> None:
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
        applied_delta = 0
        if existing is not None:
            existing.amount += amount
            applied_delta = amount
            if existing.amount == 0 and not existing.allow_negative:
                del self.powers[power_id]
        else:
            if cls is not None:
                power = cls(amount)
                self.powers[power_id] = power
                applied_delta = amount

        combat = self.combat_state
        if applied_delta != 0 and combat is not None:
            after_change = getattr(combat, "after_power_amount_changed", None)
            if callable(after_change):
                after_change(self, power_id, applied_delta, applier=applier, source=source)

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

    def gain_gold(self, amount: int) -> int:
        combat = self.combat_state
        gain_gold = getattr(combat, "gain_gold", None) if combat is not None else None
        if callable(gain_gold):
            return gain_gold(self, amount)
        return 0

    def lose_gold(self, amount: int) -> int:
        combat = self.combat_state
        lose_gold = getattr(combat, "lose_gold", None) if combat is not None else None
        if callable(lose_gold):
            return lose_gold(self, amount)
        return 0

    def gain_potion_slots(self, amount: int) -> None:
        combat = self.combat_state
        state = getattr(combat, "combat_player_state_for", lambda *_: None)(self) if combat is not None else None
        if state is not None:
            state.max_potion_slots += max(0, amount)

    def upgrade_random_cards(self, card_type: object | None, count: int) -> int:
        combat = self.combat_state
        state = getattr(combat, "combat_player_state_for", lambda *_: None)(self) if combat is not None else None
        if state is not None:
            candidates = [card for card in state.starting_deck if not card.upgraded and (card_type is None or card.card_type == card_type)]
            getattr(combat, "rng", None).shuffle(candidates)
            upgraded = 0
            for card in candidates[:count]:
                if getattr(combat, "upgrade_card", None) is not None:
                    combat.upgrade_card(card)
                    upgraded += 1
            return upgraded
        run_state = getattr(self, "run_state", None)
        player_state = getattr(run_state, "player", None)
        if player_state is not None and hasattr(player_state, "upgrade_random_cards"):
            return player_state.upgrade_random_cards(card_type, count)
        return 0

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

    def gain_forge(self, amount: int, source: object | None = None) -> None:
        combat = self.combat_state
        forge = getattr(combat, "forge", None) if combat is not None else None
        if callable(forge):
            forge(self, amount, source=source)

    def __repr__(self) -> str:
        powers_str = ", ".join(str(p) for p in self.powers.values()) if self.powers else ""
        name = self.monster_id or ("Player" if self.is_player else "Creature")
        return f"{name}(HP={self.current_hp}/{self.max_hp} Block={self.block}{' ' + powers_str if powers_str else ''})"
