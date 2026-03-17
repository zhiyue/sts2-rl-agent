"""Shared events -- events that can appear in multiple acts.

Each event is an EventModel subclass with is_allowed conditions and choices
matching the decompiled MegaCrit.Sts2.Core.Models.Events source.
"""

from __future__ import annotations

import math
from typing import TYPE_CHECKING

from sts2_env.cards.factory import create_character_cards
from sts2_env.cards.status import make_doubt, make_poor_sleep, make_regret, make_shame, make_spore_mind
from sts2_env.potions.base import create_potion
from sts2_env.relics.base import RelicId
from sts2_env.run.reward_objects import CardReward, PotionReward, RelicReward
from sts2_env.run.events import EventModel, EventOption, EventResult, register_event

if TYPE_CHECKING:
    from sts2_env.run.run_state import RunState


# ── Helpers ──────────────────────────────────────────────────────────

def _hp_ge(run_state: RunState, threshold: int) -> bool:
    return run_state.player.current_hp >= threshold


def _gold_ge(run_state: RunState, threshold: int) -> bool:
    return run_state.player.gold >= threshold


def _obtain_random_relics(run_state: RunState, count: int) -> list[str]:
    obtained: list[str] = []
    for _ in range(max(0, count)):
        reward = RelicReward(run_state.player.player_id)
        reward.populate(run_state, None)
        if reward.relic_id is not None:
            run_state.player.obtain_relic(reward.relic_id)
            obtained.append(reward.relic_id)
    return obtained


def _upgrade_n_cards(run_state: RunState, count: int) -> int:
    upgraded = 0
    for card in run_state.player.deck:
        if upgraded >= count:
            break
        if not card.upgraded:
            from sts2_env.cards.factory import create_card

            try:
                upgraded_card = create_card(card.card_id, upgraded=True)
            except KeyError:
                continue
            if not upgraded_card.upgraded:
                continue
            card.cost = upgraded_card.cost
            card.card_type = upgraded_card.card_type
            card.target_type = upgraded_card.target_type
            card.rarity = upgraded_card.rarity
            card.base_damage = upgraded_card.base_damage
            card.base_block = upgraded_card.base_block
            card.upgraded = upgraded_card.upgraded
            card.keywords = upgraded_card.keywords
            card.tags = upgraded_card.tags
            card.can_be_generated_in_combat = upgraded_card.can_be_generated_in_combat
            card.can_be_generated_by_modifiers = upgraded_card.can_be_generated_by_modifiers
            card.effect_vars = dict(upgraded_card.effect_vars)
            card.original_cost = upgraded_card.original_cost
            upgraded += 1
    return upgraded


def _transform_n_cards(run_state: RunState, count: int) -> int:
    transformed = 0
    candidates = [card for card in run_state.player.deck if card.rarity.name not in {"QUEST"}]
    replacements = create_character_cards(
        run_state.player.character_id,
        run_state.rng.niche,
        count,
        distinct=False,
        generation_context=None,
    )
    for old_card, new_card in zip(candidates, replacements):
        old_card.card_id = new_card.card_id
        old_card.cost = new_card.cost
        old_card.card_type = new_card.card_type
        old_card.target_type = new_card.target_type
        old_card.rarity = new_card.rarity
        old_card.base_damage = new_card.base_damage
        old_card.base_block = new_card.base_block
        old_card.upgraded = new_card.upgraded
        old_card.keywords = new_card.keywords
        old_card.tags = new_card.tags
        old_card.can_be_generated_in_combat = new_card.can_be_generated_in_combat
        old_card.can_be_generated_by_modifiers = new_card.can_be_generated_by_modifiers
        old_card.effect_vars = dict(new_card.effect_vars)
        old_card.original_cost = new_card.original_cost
        transformed += 1
        if transformed >= count:
            break
    return transformed


def _remove_n_cards(run_state: RunState, count: int) -> int:
    removed = 0
    remaining = []
    for card in run_state.player.deck:
        if removed < count and card.rarity.name != "QUEST":
            removed += 1
            continue
        remaining.append(card)
    run_state.player.deck = remaining
    return removed


def _upgrade_selected_cards(cards: list, run_state: RunState) -> int:
    upgraded = 0
    for card in cards:
        if card.upgraded:
            continue
        from sts2_env.cards.factory import create_card

        try:
            upgraded_card = create_card(card.card_id, upgraded=True)
        except KeyError:
            continue
        if not upgraded_card.upgraded:
            continue
        card.cost = upgraded_card.cost
        card.card_type = upgraded_card.card_type
        card.target_type = upgraded_card.target_type
        card.rarity = upgraded_card.rarity
        card.base_damage = upgraded_card.base_damage
        card.base_block = upgraded_card.base_block
        card.upgraded = upgraded_card.upgraded
        card.keywords = upgraded_card.keywords
        card.tags = upgraded_card.tags
        card.can_be_generated_in_combat = upgraded_card.can_be_generated_in_combat
        card.can_be_generated_by_modifiers = upgraded_card.can_be_generated_by_modifiers
        card.effect_vars = dict(upgraded_card.effect_vars)
        card.original_cost = upgraded_card.original_cost
        upgraded += 1
    return upgraded


def _transform_selected_cards(cards: list, run_state: RunState) -> int:
    replacements = create_character_cards(
        run_state.player.character_id,
        run_state.rng.niche,
        len(cards),
        distinct=False,
        generation_context=None,
    )
    transformed = 0
    for old_card, new_card in zip(cards, replacements):
        old_card.card_id = new_card.card_id
        old_card.cost = new_card.cost
        old_card.card_type = new_card.card_type
        old_card.target_type = new_card.target_type
        old_card.rarity = new_card.rarity
        old_card.base_damage = new_card.base_damage
        old_card.base_block = new_card.base_block
        old_card.upgraded = new_card.upgraded
        old_card.keywords = new_card.keywords
        old_card.tags = new_card.tags
        old_card.can_be_generated_in_combat = new_card.can_be_generated_in_combat
        old_card.can_be_generated_by_modifiers = new_card.can_be_generated_by_modifiers
        old_card.effect_vars = dict(new_card.effect_vars)
        old_card.original_cost = new_card.original_cost
        transformed += 1
    return transformed


def _remove_selected_cards(cards: list, run_state: RunState) -> int:
    removed = 0
    selected_ids = {id(card) for card in cards}
    new_deck = []
    for card in run_state.player.deck:
        if id(card) in selected_ids:
            removed += 1
            continue
        new_deck.append(card)
    run_state.player.deck = new_deck
    return removed


# ── AbyssalBaths ─────────────────────────────────────────────────────

class AbyssalBaths(EventModel):
    """Multi-page: Immerse +2 Max HP, take damage (scaling). Abstain: Heal 10."""

    event_id = "AbyssalBaths"

    def __init__(self) -> None:
        self._immerse_count = 0
        self._damage = 3

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        self._immerse_count = 0
        self._damage = 3
        return [
            EventOption("immerse", "Immerse", f"+2 Max HP, take {self._damage} damage"),
            EventOption("abstain", "Abstain", "Heal 10 HP"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "abstain":
            healed = run_state.player.heal(10)
            return EventResult(finished=True, description=f"Healed {healed} HP.")

        # Immerse
        run_state.player.gain_max_hp(2)
        run_state.player.lose_hp(self._damage)
        self._immerse_count += 1
        self._damage += 1

        if self._immerse_count >= 9:
            return EventResult(finished=True, description=f"Immersed {self._immerse_count} times.")

        return EventResult(
            finished=False,
            description=f"+2 Max HP, took {self._damage - 1} damage.",
            next_options=[
                EventOption("immerse", "Linger", f"+2 Max HP, take {self._damage} damage"),
                EventOption("abstain", "Abstain", "Heal 10 HP"),
            ],
        )


register_event(AbyssalBaths())


# ── Amalgamator ──────────────────────────────────────────────────────

class Amalgamator(EventModel):
    """Remove 2 Strike/Defend cards and gain UltimateStrike/UltimateDefend."""

    event_id = "Amalgamator"

    def is_allowed(self, run_state: RunState) -> bool:
        deck = run_state.player.deck
        from sts2_env.core.enums import CardRarity
        strike_count = sum(
            1 for c in deck
            if "STRIKE" in c.card_id.name
            and c.rarity == CardRarity.BASIC
        )
        defend_count = sum(
            1 for c in deck
            if "DEFEND" in c.card_id.name
            and c.rarity == CardRarity.BASIC
        )
        return strike_count >= 2 and defend_count >= 2

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("combine_strikes", "Combine Strikes",
                         "Remove 2 Strikes, gain Ultimate Strike"),
            EventOption("combine_defends", "Combine Defends",
                         "Remove 2 Defends, gain Ultimate Defend"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        # Simplified: we record the decision; actual card manipulation
        # would require card factory infrastructure
        if option_id == "combine_strikes":
            return EventResult(finished=True, description="Combined 2 Strikes into Ultimate Strike.")
        return EventResult(finished=True, description="Combined 2 Defends into Ultimate Defend.")


register_event(Amalgamator())


# ── AromaOfChaos ─────────────────────────────────────────────────────

class AromaOfChaos(EventModel):
    """Let Go: Transform 1 card. Maintain Control: Upgrade 1 card."""

    event_id = "AromaOfChaos"

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("let_go", "Let Go", "Transform 1 card"),
            EventOption("maintain_control", "Maintain Control", "Upgrade 1 card"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "let_go":
            return EventResult(finished=True, description="Transformed a card.")
        return EventResult(finished=True, description="Upgraded a card.")


register_event(AromaOfChaos())


# ── BattlewornDummy ──────────────────────────────────────────────────

class BattlewornDummy(EventModel):
    """Combat event: 3 difficulty settings with different rewards.

    Setting 1 (easy) -> Potion reward
    Setting 2 (medium) -> Upgrade 2 random cards
    Setting 3 (hard) -> Relic reward
    """

    event_id = "BattlewornDummy"

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("setting_1", "Setting 1 (Easy)", "Win: gain a potion"),
            EventOption("setting_2", "Setting 2 (Medium)", "Win: upgrade 2 cards"),
            EventOption("setting_3", "Setting 3 (Hard)", "Win: gain a relic"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "setting_1":
            return EventResult(
                finished=True,
                description="Fought dummy (easy), gained a potion.",
                rewards={"reward_objects": [PotionReward(run_state.player.player_id)]},
            )
        if option_id == "setting_2":
            _upgrade_n_cards(run_state, 2)
            return EventResult(finished=True, description="Fought dummy (medium), upgraded 2 cards.")
        return EventResult(
            finished=True,
            description="Fought dummy (hard), gained a relic.",
            rewards={"reward_objects": [RelicReward(run_state.player.player_id)]},
        )


register_event(BattlewornDummy())


# ── Bugslayer ────────────────────────────────────────────────────────

class Bugslayer(EventModel):
    """Choose Exterminate or Squash card to add to deck."""

    event_id = "Bugslayer"

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("exterminate", "Extermination", "Gain Exterminate card"),
            EventOption("squash", "Squash", "Gain Squash card"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "exterminate":
            return EventResult(finished=True, description="Gained Exterminate card.")
        return EventResult(finished=True, description="Gained Squash card.")


register_event(Bugslayer())


# ── ByrdonisNest ─────────────────────────────────────────────────────

class ByrdonisNest(EventModel):
    """Eat: +7 Max HP. Take: gain Byrdonis Egg card (event pet)."""

    event_id = "ByrdonisNest"

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("eat", "Eat", "+7 Max HP"),
            EventOption("take", "Take the Egg", "Gain Byrdonis Egg card"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "eat":
            run_state.player.gain_max_hp(7)
            return EventResult(finished=True, description="Gained 7 Max HP.")
        return EventResult(finished=True, description="Gained Byrdonis Egg card.")


register_event(ByrdonisNest())


# ── ColorfulPhilosophers ────────────────────────────────────────────

class ColorfulPhilosophers(EventModel):
    """Choose a philosopher color to get card rewards from that color's pool."""

    event_id = "ColorfulPhilosophers"

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("red", "Red Philosopher", "Card rewards from Red pool"),
            EventOption("blue", "Blue Philosopher", "Card rewards from Blue pool"),
            EventOption("green", "Green Philosopher", "Card rewards from Green pool"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        return EventResult(finished=True, description=f"Received card rewards from {option_id} pool.")


register_event(ColorfulPhilosophers())


# ── ColossalFlower ───────────────────────────────────────────────────

class ColossalFlower(EventModel):
    """Multi-page: Extract gold or reach deeper (take damage for more gold/relic).

    Prize tiers: 35g, 75g, 135g. Damage: 5, 6, 7.
    Final option: Pollinous Core relic (take 7 damage).
    """

    event_id = "ColossalFlower"

    _PRIZE_GOLD = [35, 75, 135]
    _PRIZE_DAMAGE = [5, 6, 7]

    def __init__(self) -> None:
        self._digs = 0

    def is_allowed(self, run_state: RunState) -> bool:
        return _hp_ge(run_state, 19)

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        self._digs = 0
        return [
            EventOption("extract", "Extract Prize", f"Gain {self._PRIZE_GOLD[0]} gold"),
            EventOption("reach_deeper", "Reach Deeper",
                         f"Take {self._PRIZE_DAMAGE[0]} damage for better reward"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "extract":
            gold = self._PRIZE_GOLD[self._digs]
            run_state.player.gain_gold(gold)
            return EventResult(finished=True, description=f"Extracted {gold} gold.")

        # reach_deeper
        dmg = self._PRIZE_DAMAGE[self._digs]
        run_state.player.lose_hp(dmg)
        self._digs += 1

        if self._digs < 2:
            return EventResult(
                finished=False,
                description=f"Took {dmg} damage, dug deeper.",
                next_options=[
                    EventOption("extract", "Extract Prize",
                                 f"Gain {self._PRIZE_GOLD[self._digs]} gold"),
                    EventOption("reach_deeper", "Reach Deeper",
                                 f"Take {self._PRIZE_DAMAGE[self._digs]} damage"),
                ],
            )
        # Final level: choose extract or pollinous core
        return EventResult(
            finished=False,
            description=f"Took {dmg} damage, reached the bottom.",
            next_options=[
                EventOption("extract", "Extract Prize",
                             f"Gain {self._PRIZE_GOLD[self._digs]} gold"),
                EventOption("pollinous_core", "Pollinous Core",
                             f"Take {self._PRIZE_DAMAGE[self._digs]} damage, gain Pollinous Core relic"),
            ],
        )


register_event(ColossalFlower())


# ── Darv ─────────────────────────────────────────────────────────────

class Darv(EventModel):
    """Ancient event: choose from 3 boss relics (randomly selected)."""

    event_id = "Darv"

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("relic_1", "Relic Offer 1", "Gain a boss relic"),
            EventOption("relic_2", "Relic Offer 2", "Gain a boss relic"),
            EventOption("relic_3", "Relic Offer 3", "Gain a boss relic"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        return EventResult(finished=True, description="Obtained a boss relic from Darv.")


register_event(Darv())


# ── DenseVegetation ──────────────────────────────────────────────────

class DenseVegetation(EventModel):
    """Trudge On: Remove 1 card, take 11 damage. Rest: Heal (rest-site amount), then fight."""

    event_id = "DenseVegetation"

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        heal_amount = int(run_state.player.max_hp * 0.3)
        return [
            EventOption("trudge_on", "Trudge On",
                         "Remove 1 card, take 11 damage"),
            EventOption("rest", "Rest",
                         f"Heal {heal_amount} HP, then fight"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "trudge_on":
            run_state.player.lose_hp(11)
            return EventResult(finished=True,
                               description="Removed a card, took 11 damage.")
        # Rest -> heal then fight
        heal_amount = int(run_state.player.max_hp * 0.3)
        run_state.player.heal(heal_amount)
        return EventResult(finished=True,
                           description=f"Healed {heal_amount} HP, entered combat.")


register_event(DenseVegetation())


# ── DoorsOfLightAndDark ─────────────────────────────────────────────

class DoorsOfLightAndDark(EventModel):
    """Light: Upgrade 2 random cards. Dark: Remove 1 card."""

    event_id = "DoorsOfLightAndDark"

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("light", "Door of Light", "Upgrade 2 random cards"),
            EventOption("dark", "Door of Dark", "Remove 1 card"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "light":
            return EventResult(finished=True, description="Upgraded 2 cards.")
        return EventResult(finished=True, description="Removed 1 card.")


register_event(DoorsOfLightAndDark())


# ── DrowningBeacon ───────────────────────────────────────────────────

class DrowningBeacon(EventModel):
    """Bottle: Gain Glowwater Potion. Climb: Lose 13 Max HP, gain Fresnel Lens relic."""

    event_id = "DrowningBeacon"

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("bottle", "Bottle the Water", "Gain Glowwater Potion"),
            EventOption("climb", "Climb", "Lose 13 Max HP, gain Fresnel Lens relic"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "bottle":
            return EventResult(finished=True, description="Gained Glowwater Potion.")
        run_state.player.lose_max_hp(13)
        return EventResult(finished=True,
                           description="Lost 13 Max HP, gained Fresnel Lens relic.")


register_event(DrowningBeacon())


# ── GraveOfTheForgotten ─────────────────────────────────────────────

class GraveOfTheForgotten(EventModel):
    """Confront: Gain Decay curse + Soul's Power enchantment on a card.
    Accept: Gain Forgotten Soul relic.
    """

    event_id = "GraveOfTheForgotten"

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("confront", "Confront", "Gain Decay curse, enchant a card"),
            EventOption("accept", "Accept", "Gain Forgotten Soul relic"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "confront":
            return EventResult(finished=True,
                               description="Gained Decay curse and Soul's Power enchantment.")
        return EventResult(finished=True, description="Gained Forgotten Soul relic.")


register_event(GraveOfTheForgotten())


# ── HungryForMushrooms ──────────────────────────────────────────────

class HungryForMushrooms(EventModel):
    """Big Mushroom: Gain Big Mushroom relic. Fragrant Mushroom: Gain Fragrant Mushroom relic."""

    event_id = "HungryForMushrooms"

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("big", "Big Mushroom", "Gain Big Mushroom relic"),
            EventOption("fragrant", "Fragrant Mushroom",
                         "Take 15 damage, gain Fragrant Mushroom relic"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "big":
            return EventResult(finished=True, description="Gained Big Mushroom relic.")
        run_state.player.lose_hp(15)
        return EventResult(finished=True,
                           description="Took 15 damage, gained Fragrant Mushroom relic.")


register_event(HungryForMushrooms())


# ── InfestedAutomaton ────────────────────────────────────────────────

class InfestedAutomaton(EventModel):
    """Study: Gain a random Power card. Touch Core: Gain a random 0-cost card."""

    event_id = "InfestedAutomaton"

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("study", "Study", "Gain a random Power card"),
            EventOption("touch_core", "Touch Core", "Gain a random 0-cost card"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "study":
            return EventResult(finished=True, description="Gained a random Power card.")
        return EventResult(finished=True, description="Gained a random 0-cost card.")


register_event(InfestedAutomaton())


# ── LostWisp ────────────────────────────────────────────────────────

class LostWisp(EventModel):
    """Claim: Gain Decay curse + Lost Wisp relic. Search: Gain ~60 gold."""

    event_id = "LostWisp"

    def __init__(self) -> None:
        self._gold = 60

    def calculate_vars(self, run_state: RunState) -> None:
        variance = run_state.rng.up_front.next_int(-15, 16)
        self._gold = 60 + variance

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        self.calculate_vars(run_state)
        return [
            EventOption("claim", "Claim",
                         "Gain Lost Wisp relic + Decay curse"),
            EventOption("search", "Search", f"Gain {self._gold} gold"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "claim":
            return EventResult(finished=True,
                               description="Gained Lost Wisp relic and Decay curse.")
        run_state.player.gain_gold(self._gold)
        return EventResult(finished=True, description=f"Gained {self._gold} gold.")


register_event(LostWisp())


# ── Nonupeipe ────────────────────────────────────────────────────────

class Nonupeipe(EventModel):
    """Ancient event: choose from 3 randomized relic options."""

    event_id = "Nonupeipe"

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("relic_1", "Relic Option 1", "Gain a relic"),
            EventOption("relic_2", "Relic Option 2", "Gain a relic"),
            EventOption("relic_3", "Relic Option 3", "Gain a relic"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        return EventResult(finished=True, description="Obtained a relic from Nonupeipe.")


register_event(Nonupeipe())


# ── Orobas ───────────────────────────────────────────────────────────

class Orobas(EventModel):
    """Ancient event: 3 relic offers from different pools."""

    event_id = "Orobas"

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("relic_1", "Orobas Offer 1", "Gain a relic"),
            EventOption("relic_2", "Orobas Offer 2", "Gain a relic"),
            EventOption("relic_3", "Orobas Offer 3", "Gain a relic"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        return EventResult(finished=True, description="Obtained a relic from Orobas.")


register_event(Orobas())


# ── Pael ─────────────────────────────────────────────────────────────

class Pael(EventModel):
    """Ancient event: 3 relic offers from Pael's body parts."""

    event_id = "Pael"

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("relic_1", "Pael Offer 1", "Gain a Pael relic"),
            EventOption("relic_2", "Pael Offer 2", "Gain a Pael relic"),
            EventOption("relic_3", "Pael Offer 3", "Gain a Pael relic"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        return EventResult(finished=True, description="Obtained a relic from Pael.")


register_event(Pael())


# ── PunchOff ─────────────────────────────────────────────────────────

class PunchOff(EventModel):
    """Nab: Gain Injury curse + relic reward. Take Them: Fight for relic + potion."""

    event_id = "PunchOff"

    def __init__(self) -> None:
        self._gold = 0

    def is_allowed(self, run_state: RunState) -> bool:
        return run_state.total_floor >= 6

    def calculate_vars(self, run_state: RunState) -> None:
        self._gold = run_state.rng.up_front.next_int(91, 99)

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("nab", "Nab", "Gain Injury curse + relic reward"),
            EventOption("take_them", "I Can Take Them", "Fight for relic + potion"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "nab":
            return EventResult(finished=True,
                               description="Gained Injury curse and a relic.")
        # take_them -> fight
        return EventResult(finished=True,
                           description="Fought and earned relic + potion.")


register_event(PunchOff())


# ── Reflections ──────────────────────────────────────────────────────

class Reflections(EventModel):
    """Touch Mirror: Downgrade 2 upgraded cards, upgrade 4 upgradable cards.
    Shatter: Duplicate entire deck, gain Bad Luck curse.
    """

    event_id = "Reflections"

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("touch", "Touch a Mirror",
                         "Downgrade 2, upgrade 4 cards"),
            EventOption("shatter", "Shatter",
                         "Duplicate entire deck, gain Bad Luck curse"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "touch":
            return EventResult(finished=True,
                               description="Downgraded 2 and upgraded 4 cards.")
        return EventResult(finished=True,
                           description="Duplicated deck, gained Bad Luck curse.")


register_event(Reflections())


# ── RoundTeaParty ────────────────────────────────────────────────────

class RoundTeaParty(EventModel):
    """Enjoy Tea: Gain Royal Poison relic, heal to full.
    Pick Fight: Take 11 damage, gain a relic.
    """

    event_id = "RoundTeaParty"

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("enjoy_tea", "Enjoy Tea",
                         "Gain Royal Poison relic, heal to full HP"),
            EventOption("pick_fight", "Pick a Fight",
                         "Take 11 damage, gain a relic"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "enjoy_tea":
            missing = run_state.player.max_hp - run_state.player.current_hp
            run_state.player.heal(missing)
            run_state.player.obtain_relic(RelicId.ROYAL_POISON.name)
            return EventResult(finished=True,
                               description="Gained Royal Poison relic, healed to full.")
        if option_id == "pick_fight":
            return EventResult(
                finished=False,
                description="The table overturns and the room turns hostile.",
                next_options=[EventOption("continue_fight", "Continue Fight", "Take 11 damage, gain a relic")],
            )
        run_state.player.lose_hp(11)
        _obtain_random_relics(run_state, 1)
        return EventResult(finished=True,
                           description="Took 11 damage, gained a relic.")


register_event(RoundTeaParty())


# ── SapphireSeed ─────────────────────────────────────────────────────

class SapphireSeed(EventModel):
    """Eat: Heal 9 HP, upgrade 1 card. Plant: Enchant a card with Sown."""

    event_id = "SapphireSeed"

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("eat", "Eat", "Heal 9 HP, upgrade 1 card"),
            EventOption("plant", "Plant the Seed", "Enchant a card with Sown"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "eat":
            run_state.player.heal(9)
            candidates = [card for card in run_state.player.deck if not card.upgraded]
            if not candidates:
                return EventResult(finished=True, description="Healed 9 HP.")
            return self.request_card_choice(
                prompt="Choose a card to upgrade",
                cards=candidates,
                source_pile="deck",
                resolver=lambda selected: (
                    _upgrade_selected_cards(selected, run_state),
                    EventResult(finished=True, description="Healed 9 HP, upgraded a card."),
                )[1],
                description="Choose a card to upgrade.",
            )
        return self.request_card_choice(
            prompt="Choose a card to enchant with Sown",
            cards=list(run_state.player.deck),
            source_pile="deck",
            resolver=lambda selected: (
                selected and selected[0].add_enchantment("Sown", 1),
                EventResult(finished=True, description="Enchanted a card with Sown."),
            )[-1],
            description="Choose a card to enchant.",
        )


register_event(SapphireSeed())


# ── SelfHelpBook ─────────────────────────────────────────────────────

class SelfHelpBook(EventModel):
    """Enchant a card: Sharp +2 (Attack), Nimble +2 (Skill), or Swift +2 (Power)."""

    event_id = "SelfHelpBook"

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("read_back", "Read the Back",
                         "Enchant an Attack with Sharp +2"),
            EventOption("read_passage", "Read a Passage",
                         "Enchant a Skill with Nimble +2"),
            EventOption("read_entire", "Read Entire Book",
                         "Enchant a Power with Swift +2"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        mapping = {
            "read_back": ("Sharp", 2, "ATTACK", "Enchanted an Attack with Sharp +2."),
            "read_passage": ("Nimble", 2, "SKILL", "Enchanted a Skill with Nimble +2."),
            "read_entire": ("Swift", 2, "POWER", "Enchanted a Power with Swift +2."),
        }
        enchantment, amount, card_type_name, description = mapping.get(
            option_id,
            ("Swift", 2, "POWER", "Enchanted a Power with Swift +2."),
        )
        candidates = [card for card in run_state.player.deck if card.card_type.name == card_type_name]
        return self.request_card_choice(
            prompt=f"Choose a {card_type_name.title()} to enchant",
            cards=candidates,
            source_pile="deck",
            resolver=lambda selected: (
                selected and selected[0].add_enchantment(enchantment, amount),
                EventResult(finished=True, description=description),
            )[-1],
            description="Choose a card to enchant.",
        )


register_event(SelfHelpBook())


# ── SpiritGrafter ────────────────────────────────────────────────────

class SpiritGrafter(EventModel):
    """Let It In: Heal 25, gain Metamorphosis curse card.
    Rejection: Remove 1 card, take 9 damage.
    """

    event_id = "SpiritGrafter"

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("let_it_in", "Let It In",
                         "Heal 25 HP, gain Metamorphosis card"),
            EventOption("rejection", "Rejection",
                         "Remove 1 card, take 9 damage"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "let_it_in":
            run_state.player.heal(25)
            return EventResult(finished=True,
                               description="Healed 25 HP, gained Metamorphosis card.")
        run_state.player.lose_hp(9)
        return EventResult(finished=True,
                           description="Removed a card, took 9 damage.")


register_event(SpiritGrafter())


# ── SunkenStatue ─────────────────────────────────────────────────────

class SunkenStatue(EventModel):
    """Grab Sword: Gain Sword of Stone relic.
    Dive: Gain ~111 gold, take 7 damage.
    """

    event_id = "SunkenStatue"

    def __init__(self) -> None:
        self._gold = 111

    def calculate_vars(self, run_state: RunState) -> None:
        self._gold = 111 + run_state.rng.up_front.next_int(-10, 11)

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        self.calculate_vars(run_state)
        return [
            EventOption("grab_sword", "Grab the Sword",
                         "Gain Sword of Stone relic"),
            EventOption("dive", "Dive Into Water",
                         f"Gain {self._gold} gold, take 7 damage"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "grab_sword":
            return EventResult(finished=True, description="Gained Sword of Stone relic.")
        run_state.player.gain_gold(self._gold)
        run_state.player.lose_hp(7)
        return EventResult(finished=True,
                           description=f"Gained {self._gold} gold, took 7 damage.")


register_event(SunkenStatue())


# ── SunkenTreasury ───────────────────────────────────────────────────

class SunkenTreasury(EventModel):
    """First Chest: Gain ~60 gold. Second Chest: Gain ~333 gold + Greed curse."""

    event_id = "SunkenTreasury"

    def __init__(self) -> None:
        self._small_gold = 60
        self._large_gold = 333

    def calculate_vars(self, run_state: RunState) -> None:
        self._small_gold = 60 + run_state.rng.up_front.next_int(-8, 9)
        self._large_gold = 333 + run_state.rng.up_front.next_int(-30, 31)

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        self.calculate_vars(run_state)
        return [
            EventOption("first_chest", "Small Chest", f"Gain {self._small_gold} gold"),
            EventOption("second_chest", "Large Chest",
                         f"Gain {self._large_gold} gold + Greed curse"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "first_chest":
            run_state.player.gain_gold(self._small_gold)
            return EventResult(finished=True,
                               description=f"Gained {self._small_gold} gold.")
        run_state.player.gain_gold(self._large_gold)
        return EventResult(finished=True,
                           description=f"Gained {self._large_gold} gold and Greed curse.")


register_event(SunkenTreasury())


# ── TabletOfTruth ────────────────────────────────────────────────────

class TabletOfTruth(EventModel):
    """Multi-page: Decipher (lose Max HP, upgrade cards) or Smash (heal 20).

    Decipher costs escalate: 3, 6, 12, 24, MaxHP-1.
    Each decipher upgrades a card (final decipher upgrades ALL).
    """

    event_id = "TabletOfTruth"

    _DECIPHER_COSTS = [3, 6, 12, 24]

    def __init__(self) -> None:
        self._decipher_count = 0

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        self._decipher_count = 0
        cost = self._DECIPHER_COSTS[0]
        return [
            EventOption("decipher", "Decipher",
                         f"Lose {cost} Max HP, upgrade a card"),
            EventOption("smash", "Smash", "Heal 20 HP"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "smash" or option_id == "give_up":
            run_state.player.heal(20)
            return EventResult(finished=True, description="Smashed tablet, healed 20 HP.")

        # Decipher
        if self._decipher_count < len(self._DECIPHER_COSTS):
            cost = self._DECIPHER_COSTS[self._decipher_count]
        else:
            cost = run_state.player.max_hp - 1
        run_state.player.lose_max_hp(cost)
        self._decipher_count += 1

        if self._decipher_count >= 5:
            return EventResult(finished=True,
                               description=f"Lost {cost} Max HP, upgraded ALL cards.")

        if self._decipher_count < len(self._DECIPHER_COSTS):
            next_cost = self._DECIPHER_COSTS[self._decipher_count]
        else:
            next_cost = run_state.player.max_hp - 1

        return EventResult(
            finished=False,
            description=f"Lost {cost} Max HP, upgraded a card.",
            next_options=[
                EventOption("decipher", "Decipher",
                             f"Lose {next_cost} Max HP, upgrade a card"),
                EventOption("give_up", "Give Up", "Stop deciphering"),
            ],
        )


register_event(TabletOfTruth())


# ── Tanx ─────────────────────────────────────────────────────────────

class Tanx(EventModel):
    """Ancient event: choose from 3 weapon relics."""

    event_id = "Tanx"

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("weapon_1", "Weapon 1", "Gain a weapon relic"),
            EventOption("weapon_2", "Weapon 2", "Gain a weapon relic"),
            EventOption("weapon_3", "Weapon 3", "Gain a weapon relic"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        return EventResult(finished=True, description="Obtained a weapon relic from Tanx.")


register_event(Tanx())


# ── Tezcatara ────────────────────────────────────────────────────────

class Tezcatara(EventModel):
    """Ancient event: choose from 3 comfort relics from 3 pools."""

    event_id = "Tezcatara"

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("comfort_1", "Comfort 1", "Gain a comfort relic"),
            EventOption("comfort_2", "Comfort 2", "Gain a comfort relic"),
            EventOption("comfort_3", "Comfort 3", "Gain a comfort relic"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        return EventResult(finished=True, description="Obtained a relic from Tezcatara.")


register_event(Tezcatara())


# ── TheLanternKey ────────────────────────────────────────────────────

class TheLanternKey(EventModel):
    """Return the Key: Gain 100 gold. Keep the Key: Fight for Lantern Key card."""

    event_id = "TheLanternKey"

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("return_key", "Return the Key", "Gain 100 gold"),
            EventOption("keep_key", "Keep the Key",
                         "Fight for Lantern Key card"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "return_key":
            run_state.player.gain_gold(100)
            return EventResult(finished=True, description="Gained 100 gold.")
        return EventResult(finished=True,
                           description="Fought and gained Lantern Key card.")


register_event(TheLanternKey())


# ── ThisOrThat ───────────────────────────────────────────────────────

class ThisOrThat(EventModel):
    """Plain: Take 6 damage, gain ~55 gold. Ornate: Gain relic + Clumsy curse."""

    event_id = "ThisOrThat"

    def __init__(self) -> None:
        self._gold = 55

    def calculate_vars(self, run_state: RunState) -> None:
        self._gold = run_state.rng.up_front.next_int(41, 69)

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        self.calculate_vars(run_state)
        return [
            EventOption("plain", "Plain", f"Take 6 damage, gain {self._gold} gold"),
            EventOption("ornate", "Ornate", "Gain a relic + Clumsy curse"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "plain":
            run_state.player.lose_hp(6)
            run_state.player.gain_gold(self._gold)
            return EventResult(finished=True,
                               description=f"Took 6 damage, gained {self._gold} gold.")
        return EventResult(finished=True,
                           description="Gained a relic and Clumsy curse.")


register_event(ThisOrThat())


# ── TinkerTime ───────────────────────────────────────────────────────

class TinkerTime(EventModel):
    """Multi-page: Choose card type then rider effect to create a Mad Science card."""

    event_id = "TinkerTime"

    def __init__(self) -> None:
        self._card_type: str = ""

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("attack", "Attack", "Create an Attack-type Mad Science"),
            EventOption("skill", "Skill", "Create a Skill-type Mad Science"),
            EventOption("power", "Power", "Create a Power-type Mad Science"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id in ("attack", "skill", "power"):
            self._card_type = option_id
            # Show rider options based on type
            if option_id == "attack":
                riders = [("sapping", "Sapping"), ("violence", "Violence")]
            elif option_id == "skill":
                riders = [("energized", "Energized"), ("wisdom", "Wisdom")]
            else:
                riders = [("expertise", "Expertise"), ("curious", "Curious")]
            return EventResult(
                finished=False,
                description=f"Chose {option_id} type. Pick a rider effect.",
                next_options=[
                    EventOption(r[0], r[1], f"Add {r[1]} rider")
                    for r in riders
                ],
            )
        # Rider chosen
        return EventResult(finished=True,
                           description=f"Created Mad Science ({self._card_type} + {option_id}).")


register_event(TinkerTime())


# ── TrashHeap ────────────────────────────────────────────────────────

class TrashHeap(EventModel):
    """Dive In: Take 8 damage, gain a random relic.
    Grab: Gain 100 gold + a random card.
    """

    event_id = "TrashHeap"

    def is_allowed(self, run_state: RunState) -> bool:
        return run_state.player.current_hp > 5

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("dive_in", "Dive In", "Take 8 damage, gain a relic"),
            EventOption("grab", "Grab", "Gain 100 gold + a card"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "dive_in":
            run_state.player.lose_hp(8)
            return EventResult(
                finished=True,
                description="Took 8 damage, gained a relic.",
                rewards={"reward_objects": [RelicReward(run_state.player.player_id)]},
            )
        run_state.player.gain_gold(100)
        return EventResult(
            finished=True,
            description="Gained 100 gold and a card.",
            rewards={"reward_objects": [CardReward(run_state.player.player_id, option_count=1)]},
        )


register_event(TrashHeap())


# ── Trial ────────────────────────────────────────────────────────────

class Trial(EventModel):
    """Multi-page: Accept trial, judge a random defendant.

    Merchant Guilty: Regret curse + 2 relics
    Merchant Innocent: Shame curse + upgrade 2 cards
    Noble Guilty: Heal 10
    Noble Innocent: Regret curse + 300 gold
    Nondescript Guilty: Doubt curse + 2 card rewards
    Nondescript Innocent: Doubt curse + transform 2 cards
    """

    event_id = "Trial"

    def __init__(self) -> None:
        self._trial_variant: str | None = None

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        self._trial_variant = None
        return [
            EventOption("accept", "Accept the Trial", "Judge a defendant"),
            EventOption("reject", "Reject", "Reconsider (or abandon run)"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "reject":
            return EventResult(
                finished=False,
                description="You hesitate.",
                next_options=[
                    EventOption("accept", "Accept", "Fine, accept the trial"),
                    EventOption("double_down", "Double Down", "Abandon the run"),
                ],
            )
        if option_id == "double_down":
            run_state.lose_run()
            return EventResult(finished=True, description="Abandoned the run.")

        if option_id == "accept":
            variant_roll = run_state.rng.niche.next_int(0, 2)
            self._trial_variant = ("merchant", "noble", "nondescript")[variant_roll]
            if self._trial_variant == "merchant":
                return EventResult(
                    finished=False,
                    description="A merchant stands accused before you.",
                    next_options=[
                        EventOption("merchant_guilty", "Guilty", "Regret curse + 2 relics"),
                        EventOption("merchant_innocent", "Innocent", "Shame curse + upgrade 2 cards"),
                    ],
                )
            if self._trial_variant == "noble":
                return EventResult(
                    finished=False,
                    description="A noble presents a polished defense.",
                    next_options=[
                        EventOption("noble_guilty", "Guilty", "Heal 10 HP"),
                        EventOption("noble_innocent", "Innocent", "Regret curse + 300 gold"),
                    ],
                )
            return EventResult(
                finished=False,
                description="A nondescript drifter waits for your verdict.",
                next_options=[
                    EventOption("nondescript_guilty", "Guilty", "Doubt curse + 2 card rewards"),
                    EventOption("nondescript_innocent", "Innocent", "Doubt curse + transform 2 cards"),
                ],
            )

        if option_id == "merchant_guilty":
            run_state.player.add_card_instance_to_deck(make_regret())
            _obtain_random_relics(run_state, 2)
            return EventResult(finished=True, description="Condemned the merchant, gained two relics and Regret.")
        if option_id == "merchant_innocent":
            run_state.player.add_card_instance_to_deck(make_shame())
            candidates = [card for card in run_state.player.deck if not card.upgraded]
            if not candidates:
                return EventResult(finished=True, description="Spared the merchant and gained Shame.")
            return self.request_card_choice(
                prompt="Choose 2 cards to upgrade",
                cards=candidates,
                source_pile="deck",
                resolver=lambda selected: (
                    _upgrade_selected_cards(selected, run_state),
                    EventResult(finished=True, description="Spared the merchant, gained Shame and upgraded 2 cards."),
                )[1],
                min_count=min(2, len(candidates)),
                max_count=min(2, len(candidates)),
                description="Choose 2 cards to upgrade.",
            )
        if option_id == "noble_guilty":
            run_state.player.heal(10)
            return EventResult(finished=True, description="Condemned the noble and healed 10 HP.")
        if option_id == "noble_innocent":
            run_state.player.add_card_instance_to_deck(make_regret())
            run_state.player.gain_gold(300)
            return EventResult(finished=True, description="Freed the noble, gained 300 gold and Regret.")
        if option_id == "nondescript_guilty":
            run_state.player.add_card_instance_to_deck(make_doubt())
            return EventResult(
                finished=True,
                description="Condemned the drifter, gained Doubt and two card rewards.",
                rewards={"reward_objects": [CardReward(run_state.player.player_id), CardReward(run_state.player.player_id)]},
            )
        if option_id == "nondescript_innocent":
            run_state.player.add_card_instance_to_deck(make_doubt())
            candidates = list(run_state.player.deck)
            return self.request_card_choice(
                prompt="Choose 2 cards to transform",
                cards=candidates,
                source_pile="deck",
                resolver=lambda selected: (
                    _transform_selected_cards(selected, run_state),
                    EventResult(finished=True, description="Freed the drifter, gained Doubt and transformed 2 cards."),
                )[1],
                min_count=min(2, len(candidates)),
                max_count=min(2, len(candidates)),
                description="Choose 2 cards to transform.",
            )
        return EventResult(finished=True, description="The trial ends.")


register_event(Trial())


# ── UnrestSite ───────────────────────────────────────────────────────

class UnrestSite(EventModel):
    """Rest: Heal to full, gain Poor Sleep curse.
    Kill: Lose 8 Max HP, gain a relic.
    """

    event_id = "UnrestSite"

    def is_allowed(self, run_state: RunState) -> bool:
        return run_state.player.current_hp <= run_state.player.max_hp * 0.70

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        heal_amount = run_state.player.max_hp - run_state.player.current_hp
        return [
            EventOption("rest", "Rest",
                         f"Heal {heal_amount} HP, gain Poor Sleep curse"),
            EventOption("kill", "Kill", "Lose 8 Max HP, gain a relic"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "rest":
            heal_amount = run_state.player.max_hp - run_state.player.current_hp
            run_state.player.heal(heal_amount)
            run_state.player.add_card_instance_to_deck(make_poor_sleep())
            return EventResult(finished=True,
                               description=f"Healed {heal_amount} HP, gained Poor Sleep curse.")
        run_state.player.lose_max_hp(8)
        return EventResult(
            finished=True,
            description="Lost 8 Max HP, gained a relic.",
            rewards={"reward_objects": [RelicReward(run_state.player.player_id)]},
        )


register_event(UnrestSite())


# ── Vakuu ────────────────────────────────────────────────────────────

class Vakuu(EventModel):
    """Ancient event: choose from 3 relic offers (from 3 themed pools)."""

    event_id = "Vakuu"

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("relic_1", "Vakuu Offer 1", "Gain a relic"),
            EventOption("relic_2", "Vakuu Offer 2", "Gain a relic"),
            EventOption("relic_3", "Vakuu Offer 3", "Gain a relic"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        return EventResult(finished=True, description="Obtained a relic from Vakuu.")


register_event(Vakuu())


# ── Wellspring ───────────────────────────────────────────────────────

class Wellspring(EventModel):
    """Bottle: Gain a random potion. Bathe: Remove 1 card, gain Guilty curse."""

    event_id = "Wellspring"

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("bottle", "Bottle", "Gain a random potion"),
            EventOption("bathe", "Bathe", "Remove 1 card, gain Guilty curse"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "bottle":
            return EventResult(finished=True, description="Gained a random potion.")
        return EventResult(finished=True,
                           description="Removed a card, gained Guilty curse.")


register_event(Wellspring())


# ── WoodCarvings ─────────────────────────────────────────────────────

class WoodCarvings(EventModel):
    """Bird: Transform a Basic into Peck.
    Snake: Enchant a card with Slither.
    Torus: Transform a Basic into Toric Toughness.
    """

    event_id = "WoodCarvings"

    def is_allowed(self, run_state: RunState) -> bool:
        from sts2_env.core.enums import CardRarity
        return any(c.rarity == CardRarity.BASIC for c in run_state.player.deck)

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("bird", "Bird", "Transform a Basic card into Peck"),
            EventOption("snake", "Snake", "Enchant a card with Slither"),
            EventOption("torus", "Torus",
                         "Transform a Basic card into Toric Toughness"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "bird":
            return EventResult(finished=True,
                               description="Transformed a Basic card into Peck.")
        if option_id == "snake":
            return EventResult(finished=True,
                               description="Enchanted a card with Slither.")
        return EventResult(finished=True,
                           description="Transformed a Basic card into Toric Toughness.")


register_event(WoodCarvings())


# ── ZenWeaver ────────────────────────────────────────────────────────

class ZenWeaver(EventModel):
    """Breathing Techniques: Pay 50g, gain 2 Enlightenment cards.
    Emotional Awareness: Pay 125g, remove 1 card.
    Arachnid Acupuncture: Pay 250g, remove 2 cards.
    """

    event_id = "ZenWeaver"

    def is_allowed(self, run_state: RunState) -> bool:
        return run_state.player.gold >= 125

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        gold = run_state.player.gold
        options: list[EventOption] = [
            EventOption("breathing", "Breathing Techniques",
                         "Pay 50g, gain 2 Enlightenment cards"),
        ]
        if gold >= 125:
            options.append(EventOption("emotional", "Emotional Awareness",
                                        "Pay 125g, remove 1 card"))
        if gold >= 250:
            options.append(EventOption("acupuncture", "Arachnid Acupuncture",
                                        "Pay 250g, remove 2 cards"))
        return options

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "breathing":
            run_state.player.lose_gold(50)
            return EventResult(finished=True,
                               description="Paid 50g, gained 2 Enlightenment cards.")
        if option_id == "emotional":
            run_state.player.lose_gold(125)
            return EventResult(finished=True,
                               description="Paid 125g, removed 1 card.")
        run_state.player.lose_gold(250)
        return EventResult(finished=True,
                           description="Paid 250g, removed 2 cards.")


register_event(ZenWeaver())
