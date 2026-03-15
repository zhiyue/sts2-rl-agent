"""Power instance base class with all hook method stubs.

Every power, relic, and other hook-bearing object should inherit from this
and override only the hooks it needs. The Hook dispatcher iterates all
listeners and calls these methods.
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from sts2_env.core.enums import PowerId, PowerType, PowerStackType, CombatSide, ValueProp

if TYPE_CHECKING:
    from sts2_env.core.creature import Creature
    from sts2_env.core.combat import CombatState


class PowerInstance:
    """A single power (buff/debuff) instance on a creature.

    Subclass and override hook methods to implement specific power behavior.
    All hook methods are no-ops by default.
    """

    power_id: PowerId
    power_type: PowerType = PowerType.BUFF
    stack_type: PowerStackType = PowerStackType.COUNTER
    allow_negative: bool = False

    def __init__(self, power_id: PowerId, amount: int):
        self.power_id = power_id
        self.amount = amount
        self.skip_next_tick: bool = False

    # ─── Damage Modification Hooks ──────────────────────────────────────

    def modify_damage_additive(
        self, owner: Creature, dealer: Creature | None, target: Creature, props: ValueProp
    ) -> int:
        """Called during damage additive pass. Return amount to ADD to damage."""
        return 0

    def modify_damage_multiplicative(
        self, owner: Creature, dealer: Creature | None, target: Creature, props: ValueProp
    ) -> float:
        """Called during damage multiplicative pass. Return MULTIPLIER (1.0 = no change)."""
        return 1.0

    def modify_damage_cap(
        self, owner: Creature, dealer: Creature | None, target: Creature, damage: float, props: ValueProp
    ) -> float:
        """Return a damage cap. float('inf') = no cap."""
        return float("inf")

    # ─── Block Modification Hooks ───────────────────────────────────────

    def modify_block_additive(
        self, owner: Creature, target: Creature, props: ValueProp,
        card_source: object | None = None, card_play: object | None = None,
    ) -> int:
        """Called during block additive pass. Return amount to ADD to block."""
        return 0

    def modify_block_multiplicative(
        self, owner: Creature, target: Creature, props: ValueProp,
        card_source: object | None = None, card_play: object | None = None,
    ) -> float:
        """Called during block multiplicative pass. Return MULTIPLIER."""
        return 1.0

    # ─── HP Loss Modification ───────────────────────────────────────────

    def modify_hp_lost(
        self, owner: Creature, target: Creature, amount: float,
        dealer: Creature | None, props: ValueProp
    ) -> float:
        """Modify HP lost after block. Intangible caps at 1, TungstenRod -1, etc."""
        return amount

    # ─── Block Clearing ─────────────────────────────────────────────────

    def should_clear_block(self, owner: Creature, creature: Creature) -> bool | None:
        """Return False to prevent block clearing (Barricade). None = no opinion."""
        return None

    # ─── Power Application ──────────────────────────────────────────────

    def try_block_debuff(self, owner: Creature, power_id: PowerId) -> bool:
        """Return True to consume a charge and block a debuff (Artifact)."""
        return False

    # ─── Draw / Energy Modification ─────────────────────────────────────

    def modify_hand_draw(self, owner: Creature, draw: int) -> int:
        """Modify cards drawn at turn start."""
        return draw

    def modify_max_energy(self, owner: Creature, energy: int) -> int:
        """Modify max energy."""
        return energy

    def should_draw(self, owner: Creature, from_hand_draw: bool) -> bool | None:
        return None

    # ─── Card Play Count ────────────────────────────────────────────────

    def modify_card_play_count(self, owner: Creature, count: int, card: object) -> int:
        """Modify how many times a card is played (EchoForm)."""
        return count

    def after_modifying_card_play_count(self, owner: Creature, card: object, combat: CombatState) -> None:
        pass

    # ─── Turn Lifecycle Hooks ───────────────────────────────────────────

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        pass

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        pass

    def before_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        pass

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        pass

    # ─── Legacy turn hooks (for backward compat with existing powers) ───

    def on_turn_end_enemy_side(self, owner: Creature) -> None:
        """Called at AfterTurnEnd when side == ENEMY. Duration powers tick here."""
        pass

    def on_turn_start_own_side(self, owner: Creature, combat: object) -> None:
        """Called at start of owner's side turn. Poison/Ritual fire here."""
        pass

    # ─── Card Hooks ─────────────────────────────────────────────────────

    def before_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        pass

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        pass

    def after_card_exhausted(self, owner: Creature, card: object, combat: CombatState) -> None:
        pass

    def on_card_drawn(
        self,
        owner: Creature,
        card: object,
        from_hand_draw: bool,
        combat: CombatState,
    ) -> None:
        pass

    # ─── Damage Event Hooks ─────────────────────────────────────────────

    def before_damage_received(
        self, owner: Creature, target: Creature, dealer: Creature | None,
        damage: int, props: ValueProp, combat: CombatState
    ) -> None:
        """Thorns, FlameBarrier fire here (before block is applied)."""
        pass

    def after_damage_received(
        self, owner: Creature, target: Creature, dealer: Creature | None,
        damage: int, props: ValueProp, combat: CombatState
    ) -> None:
        pass

    def after_damage_given(
        self, owner: Creature, dealer: Creature, target: Creature,
        damage: int, props: ValueProp, combat: CombatState
    ) -> None:
        pass

    # ─── Block Event Hooks ──────────────────────────────────────────────

    def after_block_gained(self, owner: Creature, creature: Creature, amount: int, combat: CombatState) -> None:
        pass

    # ─── Combat Lifecycle ───────────────────────────────────────────────

    def after_combat_victory(self, owner: Creature, combat: CombatState) -> None:
        pass

    # ─── Display ────────────────────────────────────────────────────────

    def __repr__(self) -> str:
        return f"{self.power_id.name}({self.amount})"
