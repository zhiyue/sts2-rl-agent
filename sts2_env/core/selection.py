"""Reusable combat selection abstractions."""

from __future__ import annotations

from dataclasses import dataclass, field
from typing import TYPE_CHECKING, Callable

if TYPE_CHECKING:
    from sts2_env.cards.base import CardInstance


CardSelectionResolver = Callable[[list["CardInstance"]], None]


@dataclass(slots=True)
class CardChoiceOption:
    """One selectable card option in a pending combat choice."""

    card: CardInstance
    source_pile: str


@dataclass(slots=True)
class PendingCardChoice:
    """Pending card selection that pauses combat execution."""

    prompt: str
    options: list[CardChoiceOption]
    resolver: CardSelectionResolver
    allow_skip: bool = False
    min_choices: int = 1
    max_choices: int = 1
    selected_indices: set[int] = field(default_factory=set)

    @property
    def num_options(self) -> int:
        return len(self.options)

    @property
    def is_multi(self) -> bool:
        return self.max_choices != 1 or self.min_choices != 1

    @property
    def selected_cards(self) -> list[CardInstance]:
        return [
            option.card
            for i, option in enumerate(self.options)
            if i in self.selected_indices
        ]

    def can_confirm(self) -> bool:
        if len(self.selected_indices) >= self.min_choices:
            return True
        return self.allow_skip and not self.selected_indices

    def can_toggle(self, index: int) -> bool:
        if index in self.selected_indices:
            return True
        return len(self.selected_indices) < self.max_choices

    def toggle(self, index: int) -> bool:
        if index < 0 or index >= len(self.options):
            return False
        if index in self.selected_indices:
            self.selected_indices.remove(index)
            return True
        if len(self.selected_indices) >= self.max_choices:
            return False
        self.selected_indices.add(index)
        return True
