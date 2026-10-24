"""OrbInstance base class for the Defect orb system.

Based on MegaCrit.Sts2.Core.Models.Orbs and MegaCrit.Sts2.Core.Entities.Orbs.
"""

from __future__ import annotations

from abc import ABC, abstractmethod
from typing import TYPE_CHECKING

from sts2_env.core.enums import OrbType, PowerId

if TYPE_CHECKING:
    from sts2_env.core.creature import Creature
    from sts2_env.core.combat import CombatState


class OrbInstance(ABC):
    """A single channeled orb in the orb queue.

    Subclasses must implement:
      - orb_type: the OrbType
      - base_passive: base passive trigger value
      - base_evoke: base evoke value
      - on_passive: called each turn (BeforeTurnEnd or AfterTurnStart)
      - on_evoke: called when the orb is evoked
    """

    orb_type: OrbType

    def __init__(self) -> None:
        # Mutable evoke value for Dark orb stacking
        self._evoke_override: int | None = None

    @property
    @abstractmethod
    def base_passive(self) -> int:
        """Base passive value before Focus modifier."""

    @property
    @abstractmethod
    def base_evoke(self) -> int:
        """Base evoke value before Focus modifier."""

    def get_passive_value(self, combat: CombatState) -> int:
        """Get Focus-modified passive value. Override for Plasma (no focus)."""
        focus = combat.player.get_power_amount(PowerId.FOCUS)
        return self._apply_modify_orb_value(combat, max(0, self.base_passive + focus))

    def get_evoke_value(self, combat: CombatState) -> int:
        """Get Focus-modified evoke value. Override for Plasma/Dark."""
        if self._evoke_override is not None:
            return self._evoke_override
        focus = combat.player.get_power_amount(PowerId.FOCUS)
        return self._apply_modify_orb_value(combat, max(0, self.base_evoke + focus))

    def _apply_modify_orb_value(self, combat: CombatState, value: int) -> int:
        # C# Hook.ModifyOrbValue (Hook.cs:1874-1881): every relic listener may
        # adjust an orb's value; RelicModel.ModifyOrbValue (AbstractModel.cs:1796)
        # defaults to no-op (InfusedCore.cs:46-55 adds +1 to the owner's
        # LightningOrb passive/evoke values). Dispatched to the relics of the
        # player whose orb queue contains this orb, which is where C# relics
        # get their `orb.Owner == base.Owner` check from.
        for state in getattr(combat, "combat_player_states", ()):
            queue = getattr(state, "orb_queue", None)
            if queue is None or self not in getattr(queue, "orbs", ()):
                continue
            for relic in getattr(state, "relics", ()):
                modify = getattr(relic, "modify_orb_value", None)
                if callable(modify):
                    value = modify(state.creature, self, value)
        return value

    @abstractmethod
    def on_passive(self, combat: CombatState) -> None:
        """Trigger the orb's passive effect (once per turn per trigger count)."""

    @abstractmethod
    def on_evoke(self, combat: CombatState) -> list[Creature]:
        """Trigger the orb's evoke effect and return affected creatures."""

    def __repr__(self) -> str:
        return f"{self.orb_type.name}Orb"


class OrbQueue:
    """Manages the Defect player's orb slots.

    Based on MegaCrit.Sts2.Core.Entities.Orbs/OrbQueue.cs.

    Capacity starts at 0 and must be granted by cards/relics.
    Max capacity is 10.
    """

    MAX_CAPACITY = 10

    def __init__(self, initial_capacity: int = 3) -> None:
        self.capacity = min(initial_capacity, self.MAX_CAPACITY)
        self.orbs: list[OrbInstance] = []

    def channel(self, orb_type: OrbType, combat: CombatState | None = None) -> None:
        """Channel a new orb. If full, evoke front first."""
        from sts2_env.orbs.all import create_orb

        if self.capacity <= 0:
            return

        # If at capacity, evoke the front orb first
        if len(self.orbs) >= self.capacity:
            self.evoke_front(combat)

        orb = create_orb(orb_type)
        self.orbs.append(orb)

    def evoke_front(self, combat: CombatState | None = None) -> list[Creature]:
        """Evoke the front (first) orb and remove it."""
        if combat is not None and getattr(combat, "is_over", False):
            return []
        if not self.orbs:
            return []
        orb = self.orbs.pop(0)
        return self._evoke_orb(orb, combat)

    def evoke_last(self, combat: CombatState | None = None) -> list[Creature]:
        """Evoke the last orb and remove it."""
        if combat is not None and getattr(combat, "is_over", False):
            return []
        if not self.orbs:
            return []
        orb = self.orbs.pop()
        return self._evoke_orb(orb, combat)

    def _evoke_orb(self, orb: OrbInstance, combat: CombatState | None = None) -> list[Creature]:
        if combat is None:
            return []
        targets = orb.on_evoke(combat)
        for owner in combat.all_creatures:
            for power in list(owner.powers.values()):
                on_orb_evoked = getattr(power, "on_orb_evoked", None)
                if callable(on_orb_evoked):
                    on_orb_evoked(owner, orb.orb_type, targets, combat)
        return targets

    def evoke_all(self, combat: CombatState | None = None) -> None:
        """Evoke all orbs (used by some effects)."""
        while self.orbs:
            self.evoke_front(combat)

    def trigger_before_turn_end(self, combat: CombatState) -> None:
        """Trigger passives for all orbs at end of turn (BeforeTurnEnd).

        Lightning, Frost, Dark, Glass trigger here.
        """
        for orb in list(self.orbs):
            if orb.orb_type != OrbType.PLASMA:
                orb.on_passive(combat)

    def trigger_after_turn_start(self, combat: CombatState) -> None:
        """Trigger passives for all orbs at start of turn (AfterTurnStart).

        Plasma triggers here.
        """
        for orb in list(self.orbs):
            if orb.orb_type == OrbType.PLASMA:
                orb.on_passive(combat)

    def trigger_first_passive(self, combat: CombatState) -> None:
        """Trigger the passive of the first orb only."""
        if getattr(combat, "is_over", False):
            return
        if not self.orbs:
            return
        orb = self.orbs[0]
        orb.on_passive(combat)

    def add_capacity(self, amount: int) -> None:
        self.capacity = min(self.capacity + amount, self.MAX_CAPACITY)

    def remove_capacity(self, amount: int) -> None:
        self.capacity = max(0, self.capacity - amount)
        # If over capacity, evoke excess orbs
        while len(self.orbs) > self.capacity and self.orbs:
            self.orbs.pop(0)

    def __repr__(self) -> str:
        orb_names = ", ".join(str(o) for o in self.orbs)
        return f"OrbQueue({len(self.orbs)}/{self.capacity}: [{orb_names}])"
