"""Combat-scoped player state.

This mirrors the original separation between persistent player state and
combat-only state without forcing the simulator to thread a full multiplayer
entity graph through every call site at once.
"""

from __future__ import annotations

from dataclasses import dataclass, field
from typing import TYPE_CHECKING

from sts2_env.core.constants import BASE_ENERGY
from sts2_env.relics.registry import create_relic_by_name

if TYPE_CHECKING:
    from sts2_env.cards.base import CardInstance
    from sts2_env.core.creature import Creature
    from sts2_env.potions.base import PotionInstance
    from sts2_env.relics.base import RelicInstance
    from sts2_env.run.run_state import PlayerState


@dataclass
class CombatPlayerState:
    """Combat-local state for a player creature."""

    player_state: PlayerState
    creature: Creature
    starting_deck: list[CardInstance] = field(default_factory=list)
    hand: list[CardInstance] = field(default_factory=list)
    draw: list[CardInstance] = field(default_factory=list)
    discard: list[CardInstance] = field(default_factory=list)
    exhaust: list[CardInstance] = field(default_factory=list)
    play: list[CardInstance] = field(default_factory=list)
    relics: list[RelicInstance] = field(default_factory=list)
    potions: list[PotionInstance | None] = field(default_factory=list)
    max_potion_slots: int = 3
    energy: int = 0
    stars: int = 0
    base_max_energy: int = BASE_ENERGY
    orb_queue: object | None = None

    def __post_init__(self) -> None:
        if not self.relics:
            get_relic_objects = getattr(self.player_state, "get_relic_objects", None)
            if callable(get_relic_objects):
                self.relics = get_relic_objects()
            else:
                self.relics = [create_relic_by_name(relic_id) for relic_id in self.player_state.relics]
        if not self.potions:
            self.potions = list(self.player_state.potions)
        if not self.starting_deck:
            self.starting_deck = list(self.player_state.deck)
        if self.max_potion_slots <= 0:
            self.max_potion_slots = self.player_state.max_potion_slots
        if self.base_max_energy <= 0:
            self.base_max_energy = self.player_state.max_energy

        self.zone_map: dict[str, list[CardInstance]] = {
            "hand": self.hand,
            "draw": self.draw,
            "discard": self.discard,
            "exhaust": self.exhaust,
        }

    @property
    def character_id(self) -> str:
        return self.player_state.character_id

    @property
    def all_piles(self) -> tuple[list[CardInstance], ...]:
        return (self.hand, self.draw, self.discard, self.exhaust, self.play)

    def sync_back_to_player_state(self) -> None:
        self.player_state.current_hp = self.creature.current_hp
        self.player_state.max_hp = self.creature.max_hp
        self.player_state.potions = list(self.potions)
        self.player_state.max_potion_slots = self.max_potion_slots
        self.player_state.max_energy = self.base_max_energy
