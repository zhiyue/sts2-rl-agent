"""Creature: HP, Block, Powers management."""

from __future__ import annotations

from typing import TYPE_CHECKING

from sts2_env.core.enums import CombatSide, PowerId, PowerStackType, PowerType
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


def _power_type_for_amount(cls: type, amount: int) -> PowerType:
    power_type = getattr(cls, "power_type", PowerType.BUFF)
    allow_negative = getattr(cls, "allow_negative", False)
    stack_type = getattr(cls, "stack_type", None)
    if stack_type == PowerStackType.COUNTER and allow_negative and amount < 0:
        return PowerType.DEBUFF
    if not allow_negative and power_type == PowerType.DEBUFF and amount < 0:
        return PowerType.BUFF
    return power_type


class Creature:
    """A combat entity (player or monster) with HP, Block, and Powers."""

    __slots__ = (
        "max_hp", "current_hp", "block", "powers", "side",
        "is_player", "monster_id", "combat_id", "stars",
        "is_pet", "pet_owner", "is_osty", "owner", "combat_state", "escaped", "_death_processed",
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
        self._death_processed: bool = False

    @property
    def is_alive(self) -> bool:
        return self.current_hp > 0 and not self.escaped

    @property
    def is_dead(self) -> bool:
        return self.current_hp <= 0

    @property
    def is_secondary_enemy(self) -> bool:
        """C# Creature.IsSecondaryEnemy (Creature.cs:262): an enemy is secondary
        when any of its powers declares ``owner_is_secondary_enemy`` (e.g.
        MinionPower).  Secondary enemies automatically die unless a living
        primary enemy remains."""
        if self.side != CombatSide.ENEMY:
            return False
        return any(
            getattr(power, "owner_is_secondary_enemy", False)
            for power in self.powers.values()
        )

    @property
    def is_primary_enemy(self) -> bool:
        """C# Creature.IsPrimaryEnemy (Creature.cs:252): enemies that are not
        secondary.  Combat victory only requires every primary enemy to die
        (CombatManager.cs:192)."""
        return self.side == CombatSide.ENEMY and not self.is_secondary_enemy

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

    def lose_hp(self, amount: int, fire_hooks: bool = True) -> int:
        """Lose HP, returns actual HP lost."""
        if amount <= 0:
            return 0
        actual = min(amount, self.current_hp)
        self.current_hp = max(0, self.current_hp - amount)
        if fire_hooks and actual > 0 and self.combat_state is not None and not self.combat_state.is_over:
            from sts2_env.core.hooks import fire_after_current_hp_changed

            fire_after_current_hp_changed(self, -actual, self.combat_state)
        return actual

    def heal(self, amount: int) -> int:
        """Heal HP, capped at max. Returns actual healed."""
        if amount <= 0 or self.is_dead:
            return 0
        before = self.current_hp
        self.current_hp = min(self.current_hp + amount, self.max_hp)
        healed = self.current_hp - before
        if healed > 0 and self.combat_state is not None and not self.combat_state.is_over:
            from sts2_env.core.hooks import fire_after_current_hp_changed

            fire_after_current_hp_changed(self, healed, self.combat_state)
        return healed

    def clear_block(self, combat: object | None = None) -> None:
        """Clear block unless a power prevents it (Barricade).

        When combat is provided, uses the full hook dispatch (all hook listeners).
        Otherwise falls back to checking only own powers.
        """
        if combat is not None:
            from sts2_env.core.hooks import (
                block_clear_preventer,
                fire_after_preventing_block_clear,
            )

            preventer = block_clear_preventer(self, combat)
            if preventer is not None:
                fire_after_preventing_block_clear(preventer, self, combat)
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
        ignore_next_instance: bool = False,
    ) -> None:
        """Apply or stack a power. Handles Artifact blocking for debuffs."""
        combat = self.combat_state or getattr(applier, "combat_state", None)
        original_amount = amount
        if combat is not None:
            from sts2_env.core.hooks import (
                fire_after_modifying_power_amount_given,
                fire_after_modifying_power_amount_received,
                modify_power_amount_given,
                modify_power_amount_received,
            )

            if applier is not None:
                amount = int(modify_power_amount_given(
                    power_id,
                    amount,
                    applier,
                    self,
                    source,
                    combat,
                ))
                if amount != original_amount:
                    fire_after_modifying_power_amount_given(
                        power_id,
                        original_amount,
                        amount,
                        applier,
                        self,
                        source,
                        combat,
                    )
            received_original_amount = amount
            amount = int(modify_power_amount_received(
                power_id,
                amount,
                self,
                applier,
                source,
                combat,
            ))
            if amount != received_original_amount:
                fire_after_modifying_power_amount_received(
                    power_id,
                    received_original_amount,
                    amount,
                    self,
                    applier,
                    source,
                    combat,
                )
        if amount == 0:
            return

        cls = get_power_class(power_id)
        is_debuff = False
        if cls is not None:
            is_debuff = _power_type_for_amount(cls, amount) == PowerType.DEBUFF and getattr(cls, "is_visible", True)

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
            if getattr(existing, "applier", None) is None:
                existing.applier = applier
            if ignore_next_instance:
                existing.ignore_next_instance = True
            existing.amount += amount
            applied_delta = amount
            if existing.amount == 0 and not existing.allow_negative:
                del self.powers[power_id]
        else:
            if cls is not None:
                power = cls(amount)
                power.ignore_next_instance = ignore_next_instance
                power.applier = applier
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
        before = self.current_hp
        self.max_hp += amount
        self.current_hp = min(self.current_hp + amount, self.max_hp)
        delta = self.current_hp - before
        if delta != 0 and self.combat_state is not None and not self.combat_state.is_over:
            from sts2_env.core.hooks import fire_after_current_hp_changed

            fire_after_current_hp_changed(self, delta, self.combat_state)

    def lose_max_hp(self, amount: int) -> None:
        before = self.current_hp
        self.max_hp = max(1, self.max_hp - amount)
        self.current_hp = min(self.current_hp, self.max_hp)
        delta = self.current_hp - before
        if delta != 0 and self.combat_state is not None and not self.combat_state.is_over:
            from sts2_env.core.hooks import fire_after_current_hp_changed

            fire_after_current_hp_changed(self, delta, self.combat_state)

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

    def fill_empty_potion_slots(self) -> int:
        combat = self.combat_state
        fill_slots = getattr(combat, "fill_empty_potion_slots", None) if combat is not None else None
        if callable(fill_slots):
            return fill_slots(self)
        return 0

    def procure_potion(self, potion_id: str) -> bool:
        combat = self.combat_state
        procure_potion = getattr(combat, "procure_potion", None) if combat is not None else None
        if callable(procure_potion):
            return procure_potion(self, potion_id)
        run_state = getattr(self, "run_state", None)
        player_state = getattr(run_state, "player", None)
        if player_state is not None and hasattr(player_state, "procure_potion"):
            return player_state.procure_potion(potion_id)
        return False

    def upgrade_random_cards(self, card_type: object | None, count: int, rng: object | None = None) -> int:
        combat = self.combat_state
        state = getattr(combat, "combat_player_state_for", lambda *_: None)(self) if combat is not None else None
        if state is not None:
            candidates = [card for card in state.starting_deck if not card.upgraded and (card_type is None or card.card_type == card_type)]
            candidates.sort(key=lambda card: (card.card_id.name, card.upgraded))
            selected_rng = rng or getattr(combat, "combat_card_selection_rng", None) or getattr(combat, "rng", None)
            if selected_rng is not None:
                selected_rng.shuffle(candidates)
            upgraded = 0
            for card in candidates[:count]:
                if getattr(combat, "upgrade_card", None) is not None:
                    combat.upgrade_card(card)
                    upgraded += 1
            return upgraded
        run_state = getattr(self, "run_state", None)
        player_state = getattr(run_state, "player", None)
        if player_state is not None and hasattr(player_state, "upgrade_random_cards"):
            return player_state.upgrade_random_cards(card_type, count, rng=rng)
        return 0

    def transform_relic(self, current_relic: object, new_relic_id: object) -> bool:
        combat = self.combat_state
        transform_relic = getattr(combat, "transform_relic", None) if combat is not None else None
        if callable(transform_relic):
            return transform_relic(self, current_relic, new_relic_id)
        run_state = getattr(self, "run_state", None)
        player_state = getattr(run_state, "player", None)
        if player_state is not None and hasattr(player_state, "transform_relic"):
            return player_state.transform_relic(current_relic, new_relic_id)
        return False

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
