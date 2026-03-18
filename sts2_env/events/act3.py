"""Act 3 specific events and special events.

This file covers:
- TheArchitect: special end-of-run victory event
- WarHistorianRepy: disabled event (never spawns)
- Neow: special starting event (relic selection)
- DeprecatedEvent / DeprecatedAncientEvent: disabled legacy events
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from sts2_env.cards.factory import create_card
from sts2_env.core.enums import CardId
from sts2_env.events.shared import _event_result_with_rewards, _should_defer_event_rewards
from sts2_env.relics.base import RelicId
from sts2_env.run.reward_objects import AddCardsReward, PotionReward, RelicReward
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
        return EventResult(
            finished=True,
            description="The Architect emerges.",
            event_combat_setup="the_architect",
            post_combat_phase="RUN_OVER",
        )


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

    @staticmethod
    def _remove_lantern_key(run_state: RunState) -> int:
        removed = 0
        remaining = []
        for card in run_state.player.deck:
            if card.card_id == CardId.LANTERN_KEY:
                removed += 1
                continue
            remaining.append(card)
        run_state.player.deck = remaining
        return removed

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "unlock_cage":
            run_state.extra_fields["freed_repy"] = True
            self._remove_lantern_key(run_state)
            if _should_defer_event_rewards(run_state):
                return _event_result_with_rewards(
                    "Freed Repy, gained History Course relic.",
                    [RelicReward(run_state.player.player_id, relic_id=RelicId.HISTORY_COURSE.name)],
                )
            run_state.player.obtain_relic(RelicId.HISTORY_COURSE.name)
            return EventResult(finished=True,
                               description="Freed Repy, gained History Course relic.")
        self._remove_lantern_key(run_state)
        return EventResult(
            finished=True,
            description="Unlocked chest, gained 2 potions and 2 relics.",
            rewards={
                "reward_objects": [
                    PotionReward(run_state.player.player_id),
                    PotionReward(run_state.player.player_id),
                    RelicReward(run_state.player.player_id),
                    RelicReward(run_state.player.player_id),
                ]
            },
        )


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

    _POSITIVE_POOL = (
        RelicId.ARCANE_SCROLL.name,
        RelicId.BOOMING_CONCH.name,
        RelicId.POMANDER.name,
        RelicId.GOLDEN_PEARL.name,
        RelicId.LEAD_PAPERWEIGHT.name,
        RelicId.NEW_LEAF.name,
        RelicId.NEOWS_TORMENT.name,
        RelicId.PRECISE_SCISSORS.name,
        RelicId.LOST_COFFER.name,
    )
    _CURSED_POOL = (
        RelicId.CURSED_PEARL.name,
        RelicId.LARGE_CAPSULE.name,
        RelicId.LEAFY_POULTICE.name,
        RelicId.PRECARIOUS_SHEARS.name,
    )

    def __init__(self) -> None:
        self._choices: dict[str, str] = {}

    def is_allowed(self, run_state: RunState) -> bool:
        # Only triggered at run start by game logic, not from random pool
        return False

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        from sts2_env.relics.shop_event import ScrollBoxes

        cursed_pool = list(self._CURSED_POOL)
        if ScrollBoxes.can_generate_bundles(run_state.player):
            cursed_pool.append(RelicId.SCROLL_BOXES.name)
        if len(run_state.players) == 1:
            cursed_pool.append(RelicId.SILVER_CRUCIBLE.name)
        cursed_choice = run_state.rng.up_front.choice(cursed_pool)

        positive_pool = list(self._POSITIVE_POOL)
        if cursed_choice == RelicId.CURSED_PEARL.name and RelicId.GOLDEN_PEARL.name in positive_pool:
            positive_pool.remove(RelicId.GOLDEN_PEARL.name)
        if cursed_choice == RelicId.PRECARIOUS_SHEARS.name and RelicId.PRECISE_SCISSORS.name in positive_pool:
            positive_pool.remove(RelicId.PRECISE_SCISSORS.name)
        if cursed_choice == RelicId.LEAFY_POULTICE.name and RelicId.NEW_LEAF.name in positive_pool:
            positive_pool.remove(RelicId.NEW_LEAF.name)
        if len(run_state.players) > 1:
            positive_pool.append(RelicId.MASSIVE_SCROLL.name)
        positive_pool.append(run_state.rng.up_front.choice([RelicId.NUTRITIOUS_OYSTER.name, RelicId.STONE_HUMIDIFIER.name]))
        if cursed_choice != RelicId.LARGE_CAPSULE.name:
            positive_pool.append(run_state.rng.up_front.choice([RelicId.LAVA_ROCK.name, RelicId.SMALL_CAPSULE.name]))
        run_state.rng.up_front.shuffle(positive_pool)
        positives = positive_pool[:2]
        self._choices = {
            "positive_1": positives[0],
            "positive_2": positives[1],
            "cursed": cursed_choice,
        }
        return [
            EventOption("positive_1", self._choices["positive_1"].replace("_", " ").title(), "Gain a positive relic"),
            EventOption("positive_2", self._choices["positive_2"].replace("_", " ").title(), "Gain a positive relic"),
            EventOption("cursed", self._choices["cursed"].replace("_", " ").title(), "Gain a cursed relic"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        relic_id = self._choices.get(option_id)
        if relic_id is not None:
            if _should_defer_event_rewards(run_state):
                if option_id == "cursed":
                    return _event_result_with_rewards(
                        "Gained a cursed relic from Neow.",
                        [RelicReward(run_state.player.player_id, relic_id=relic_id)],
                    )
                return _event_result_with_rewards(
                    "Gained a positive relic from Neow.",
                    [RelicReward(run_state.player.player_id, relic_id=relic_id)],
                )
            run_state.player.obtain_relic(relic_id)
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
