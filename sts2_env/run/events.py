"""Event engine base classes.

Provides the EventModel base, EventOption, and event registry,
matching MegaCrit.Sts2.Core.Models.Events/EventModel.cs patterns.
"""

from __future__ import annotations

from dataclasses import dataclass, field
from typing import TYPE_CHECKING, Any, Callable

from sts2_env.core.selection import CardChoiceOption, PendingCardChoice

if TYPE_CHECKING:
    from sts2_env.cards.base import CardInstance
    from sts2_env.run.run_state import RunState


@dataclass
class EventOption:
    """A single choice in an event."""

    option_id: str
    label: str
    description: str = ""
    enabled: bool = True

    def __repr__(self) -> str:
        return f"EventOption({self.option_id}: {self.label})"


class EventModel:
    """Base class for all events.

    Subclasses implement is_allowed(), generate_initial_options(),
    and choose() to define event behavior.
    """

    event_id: str = ""

    @property
    def pending_choice(self) -> PendingCardChoice | None:
        return getattr(self, "_pending_choice", None)

    @pending_choice.setter
    def pending_choice(self, value: PendingCardChoice | None) -> None:
        self._pending_choice = value

    def is_allowed(self, run_state: RunState) -> bool:
        """Whether this event can appear given current run state.

        Default = True (always allowed). Override for conditional events.
        """
        return True

    def calculate_vars(self, run_state: RunState) -> None:
        """Randomize dynamic variables (damage, gold, etc.) before display."""
        pass

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        """Return the initial set of choices for this event."""
        return []

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        """Execute a choice and return the result.

        May return a finished result or a new set of options (multi-page).
        """
        return EventResult(finished=True, description="Nothing happened.")

    def request_card_choice(
        self,
        *,
        prompt: str,
        cards: list[CardInstance],
        source_pile: str,
        resolver: Callable[[list[CardInstance]], EventResult | None],
        allow_skip: bool = False,
        min_count: int = 1,
        max_count: int = 1,
        description: str = "",
    ) -> EventResult:
        self.pending_choice = PendingCardChoice(
            prompt=prompt,
            options=[CardChoiceOption(card=card, source_pile=source_pile) for card in cards],
            resolver=resolver,
            allow_skip=allow_skip,
            min_choices=min_count,
            max_choices=max_count,
        )
        return EventResult(finished=False, description=description or prompt)

    def resolve_pending_choice(self, choice_index: int | None) -> EventResult:
        choice = self.pending_choice
        if choice is None:
            return EventResult(finished=False, description="No pending event choice.")

        if choice.is_multi:
            if choice_index is None:
                if not choice.can_confirm():
                    return EventResult(finished=False, description="Cannot confirm event choice.")
                selected_cards = choice.selected_cards
                self.pending_choice = None
                result = choice.resolver(selected_cards)
                return result if isinstance(result, EventResult) else EventResult(finished=True, description="Resolved event choice.")
            if not choice.toggle(choice_index):
                return EventResult(finished=False, description="Invalid event choice.")
            return EventResult(finished=False, description=choice.prompt)

        selected_cards: list[CardInstance] = []
        if choice_index is None:
            if not choice.allow_skip:
                return EventResult(finished=False, description="Cannot skip event choice.")
        else:
            if choice_index < 0 or choice_index >= len(choice.options):
                return EventResult(finished=False, description="Invalid event choice.")
            selected_cards = [choice.options[choice_index].card]
        self.pending_choice = None
        result = choice.resolver(selected_cards)
        return result if isinstance(result, EventResult) else EventResult(finished=True, description="Resolved event choice.")


@dataclass
class EventResult:
    """Result of choosing an event option."""

    finished: bool = True
    description: str = ""
    next_options: list[EventOption] = field(default_factory=list)
    rewards: dict[str, Any] = field(default_factory=dict)


# ── Event Registry ────────────────────────────────────────────────────

_EVENT_REGISTRY: dict[str, EventModel] = {}


def register_event(event: EventModel) -> EventModel:
    _EVENT_REGISTRY[event.event_id] = event
    return event


def get_event(event_id: str) -> EventModel | None:
    return _EVENT_REGISTRY.get(event_id)


def all_events() -> list[EventModel]:
    return list(_EVENT_REGISTRY.values())


def get_allowed_events(run_state: RunState, pool: list[str] | None = None) -> list[EventModel]:
    """Return events from pool that pass is_allowed and haven't been visited."""
    candidates = all_events() if pool is None else [
        _EVENT_REGISTRY[eid] for eid in pool if eid in _EVENT_REGISTRY
    ]
    return [
        e for e in candidates
        if e.event_id not in run_state.visited_event_ids and e.is_allowed(run_state)
    ]


def pick_event(run_state: RunState, pool: list[str] | None = None) -> EventModel | None:
    """Pick a random allowed event from the pool."""
    allowed = get_allowed_events(run_state, pool)
    if not allowed:
        return None
    event = run_state.rng.up_front.choice(allowed)
    run_state.visited_event_ids.add(event.event_id)
    return event
