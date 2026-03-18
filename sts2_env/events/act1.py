"""Act 1 (Act 0) specific events.

Events that only appear in Act 1 (act_index 0 or < 2).
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from sts2_env.cards.factory import create_card
from sts2_env.core.enums import CardId
from sts2_env.potions.base import normal_pool_models
from sts2_env.run.events import EventModel, EventOption, EventResult, register_event
from sts2_env.events.shared import _event_result_with_rewards, _should_defer_event_rewards
from sts2_env.run.reward_objects import AddCardsReward, PotionReward, RelicReward

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
            if _should_defer_event_rewards(run_state):
                return _event_result_with_rewards(
                    "Gained Spoils Map card.",
                    [AddCardsReward(run_state.player.player_id, [create_card(CardId.SPOILS_MAP)])],
                )
            run_state.player.add_card_instance_to_deck(create_card(CardId.SPOILS_MAP))
            return EventResult(finished=True, description="Gained Spoils Map card.")
        run_state.player.lose_hp(8)
        reward_objects = []
        models = normal_pool_models(in_combat=False, character_id=run_state.player.character_id)
        if models:
            model = run_state.rng.rewards.choice(models)
            reward_objects.append(PotionReward(run_state.player.player_id, potion_id=model.potion_id))
        return EventResult(finished=True,
                           description="Took 8 damage, gained a potion.",
                           rewards={"reward_objects": reward_objects})


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
        return [
            EventOption(
                "bone_tea",
                "Bone Tea (50g)" if gold >= 50 else "Bone Tea",
                "Gain Bone Tea relic",
                enabled=gold >= 50,
            ),
            EventOption(
                "ember_tea",
                "Ember Tea (150g)" if gold >= 150 else "Ember Tea",
                "Gain Ember Tea relic",
                enabled=gold >= 150,
            ),
            EventOption("discourtesy", "Tea of Discourtesy", "Free: gain Tea of Discourtesy relic"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "bone_tea":
            run_state.player.lose_gold(50)
            if _should_defer_event_rewards(run_state):
                return _event_result_with_rewards(
                    "Paid 50g, gained Bone Tea relic.",
                    [RelicReward(run_state.player.player_id, relic_id="BONE_TEA")],
                )
            run_state.player.obtain_relic("BONE_TEA")
            return EventResult(finished=True, description="Paid 50g, gained Bone Tea relic.")
        if option_id == "ember_tea":
            run_state.player.lose_gold(150)
            if _should_defer_event_rewards(run_state):
                return _event_result_with_rewards(
                    "Paid 150g, gained Ember Tea relic.",
                    [RelicReward(run_state.player.player_id, relic_id="EMBER_TEA")],
                )
            run_state.player.obtain_relic("EMBER_TEA")
            return EventResult(finished=True, description="Paid 150g, gained Ember Tea relic.")
        if _should_defer_event_rewards(run_state):
            return _event_result_with_rewards(
                "Gained Tea of Discourtesy relic.",
                [RelicReward(run_state.player.player_id, relic_id="TEA_OF_DISCOURTESY")],
            )
        run_state.player.obtain_relic("TEA_OF_DISCOURTESY")
        return EventResult(finished=True, description="Gained Tea of Discourtesy relic.")


register_event(TeaMaster())
