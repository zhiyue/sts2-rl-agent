"""Act 1 (Act 0) specific events.

Events that only appear in Act 1 (act_index 0 or < 2).
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from sts2_env.run.events import EventModel, EventOption, EventResult, register_event

if TYPE_CHECKING:
    from sts2_env.run.run_state import RunState


# ── BrainLeech ────────────────────────────────────────────────────────

class BrainLeech(EventModel):
    """Share Knowledge: Pick 1 of 5 character cards to gain.
    Rip: Take 5 damage, gain a colorless card reward.
    """

    event_id = "BrainLeech"

    def is_allowed(self, run_state: RunState) -> bool:
        return run_state.current_act_index < 2

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("share_knowledge", "Share Knowledge",
                         "Pick 1 of 5 character cards to gain"),
            EventOption("rip", "Rip", "Take 5 damage, gain a colorless card"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "rip":
            run_state.player.lose_hp(5)
            return EventResult(finished=True,
                               description="Took 5 damage, gained a colorless card.")
        return EventResult(finished=True, description="Gained a character card.")


register_event(BrainLeech())


# ── RoomFullOfCheese ──────────────────────────────────────────────────

class RoomFullOfCheese(EventModel):
    """Gorge: Pick 2 of 8 common character cards.
    Search: Take 14 damage, gain Chosen Cheese relic.
    """

    event_id = "RoomFullOfCheese"

    def is_allowed(self, run_state: RunState) -> bool:
        return run_state.current_act_index < 2

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("gorge", "Gorge",
                         "Pick 2 of 8 common cards"),
            EventOption("search", "Search",
                         "Take 14 damage, gain Chosen Cheese relic"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "gorge":
            return EventResult(finished=True,
                               description="Picked 2 common character cards.")
        run_state.player.lose_hp(14)
        return EventResult(finished=True,
                           description="Took 14 damage, gained Chosen Cheese relic.")


register_event(RoomFullOfCheese())


# ── TheLegendsWereTrue ────────────────────────────────────────────────

class TheLegendsWereTrue(EventModel):
    """Nab the Map: Gain Spoils Map card.
    Slowly Find an Exit: Take 8 damage, gain a potion.
    """

    event_id = "TheLegendsWereTrue"

    def is_allowed(self, run_state: RunState) -> bool:
        return (
            run_state.current_act_index == 0
            and len(run_state.player.deck) > 0
            and run_state.player.current_hp >= 10
        )

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("nab_map", "Nab the Map", "Gain Spoils Map card"),
            EventOption("find_exit", "Slowly Find an Exit",
                         "Take 8 damage, gain a potion"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "nab_map":
            return EventResult(finished=True, description="Gained Spoils Map card.")
        run_state.player.lose_hp(8)
        return EventResult(finished=True,
                           description="Took 8 damage, gained a potion.")


register_event(TheLegendsWereTrue())


# ── TeaMaster ────────────────────────────────────────────────────────

class TeaMaster(EventModel):
    """Bone Tea: Pay 50g, gain Bone Tea relic.
    Ember Tea: Pay 150g, gain Ember Tea relic.
    Tea of Discourtesy: Free, gain Tea of Discourtesy relic.
    """

    event_id = "TeaMaster"

    def is_allowed(self, run_state: RunState) -> bool:
        return run_state.current_act_index < 2 and run_state.player.gold >= 150

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        gold = run_state.player.gold
        options: list[EventOption] = []
        if gold >= 50:
            options.append(EventOption("bone_tea", "Bone Tea (50g)",
                                        "Gain Bone Tea relic"))
        if gold >= 150:
            options.append(EventOption("ember_tea", "Ember Tea (150g)",
                                        "Gain Ember Tea relic"))
        options.append(EventOption("discourtesy", "Tea of Discourtesy",
                                    "Free: gain Tea of Discourtesy relic"))
        return options

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "bone_tea":
            run_state.player.lose_gold(50)
            return EventResult(finished=True,
                               description="Paid 50g, gained Bone Tea relic.")
        if option_id == "ember_tea":
            run_state.player.lose_gold(150)
            return EventResult(finished=True,
                               description="Paid 150g, gained Ember Tea relic.")
        return EventResult(finished=True,
                           description="Gained Tea of Discourtesy relic.")


register_event(TeaMaster())
