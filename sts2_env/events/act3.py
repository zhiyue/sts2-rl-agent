"""Act 3 specific events and special events.

This file covers:
- TheArchitect: special end-of-run victory event
- WarHistorianRepy: disabled event (never spawns)
- Neow: special starting event (relic selection)
- DeprecatedEvent / DeprecatedAncientEvent: disabled legacy events
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from sts2_env.run.events import EventModel, EventOption, EventResult, register_event

if TYPE_CHECKING:
    from sts2_env.run.run_state import RunState


# ── TheArchitect (special final-act transition) ──────────────────────

class TheArchitect(EventModel):
    """Special event that triggers at the end of the final act before winning."""

    event_id = "TheArchitect"

    def is_allowed(self, run_state: RunState) -> bool:
        # Not in normal pool -- only triggered by act transition logic
        return False

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("proceed", "Proceed"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        return EventResult(finished=True, description="The Architect vanishes.")


register_event(TheArchitect())


# ── WarHistorianRepy (disabled) ───────────────────────────────────────

class WarHistorianRepy(EventModel):
    """Disabled event -- requires Lantern Key card, never appears in normal pools.

    Unlock Cage: Free Repy, gain History Course relic.
    Unlock Chest: Lose Lantern Key, gain 2 potions + 2 relics.
    """

    event_id = "WarHistorianRepy"

    def is_allowed(self, run_state: RunState) -> bool:
        return False

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("unlock_cage", "Unlock Cage",
                         "Free Repy, gain History Course relic"),
            EventOption("unlock_chest", "Unlock Chest",
                         "Gain 2 potions + 2 relics"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "unlock_cage":
            return EventResult(finished=True,
                               description="Freed Repy, gained History Course relic.")
        return EventResult(finished=True,
                           description="Unlocked chest, gained 2 potions and 2 relics.")


register_event(WarHistorianRepy())


# ── Neow (special starting event) ───────────────────────────────────

class Neow(EventModel):
    """Starting event: choose from 3 relic options (2 positive, 1 cursed).

    Positive relics: ArcaneScroll, BoomingConch, Pomander, GoldenPearl,
    LeadPaperweight, NewLeaf, NeowsTorment, PreciseScissors, LostCoffer,
    NutritiousOyster, StoneHumidifier, LavaRock, SmallCapsule.
    Cursed relics: CursedPearl, LargeCapsule, LeafyPoultice, PrecariousShears,
    SilverCrucible, ScrollBoxes.
    """

    event_id = "Neow"

    def is_allowed(self, run_state: RunState) -> bool:
        # Only triggered at run start by game logic, not from random pool
        return False

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("positive_1", "Positive Relic 1", "Gain a positive relic"),
            EventOption("positive_2", "Positive Relic 2", "Gain a positive relic"),
            EventOption("cursed", "Cursed Relic", "Gain a powerful relic with a curse"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "cursed":
            return EventResult(finished=True,
                               description="Gained a cursed relic from Neow.")
        return EventResult(finished=True,
                           description="Gained a positive relic from Neow.")


register_event(Neow())


# ── DeprecatedEvent ──────────────────────────────────────────────────

class DeprecatedEvent(EventModel):
    """Deprecated event stub -- never appears. Has no options."""

    event_id = "DeprecatedEvent"

    def is_allowed(self, run_state: RunState) -> bool:
        return False

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return []

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        return EventResult(finished=True)


register_event(DeprecatedEvent())


# ── DeprecatedAncientEvent ───────────────────────────────────────────

class DeprecatedAncientEvent(EventModel):
    """Deprecated ancient event stub -- never appears. Has no options."""

    event_id = "DeprecatedAncientEvent"

    def is_allowed(self, run_state: RunState) -> bool:
        return False

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return []

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        return EventResult(finished=True)


register_event(DeprecatedAncientEvent())
