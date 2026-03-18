"""Shared events -- events that can appear in multiple acts.

Each event is an EventModel subclass with is_allowed conditions and choices
matching the decompiled MegaCrit.Sts2.Core.Models.Events source.
"""

from __future__ import annotations

import math
from typing import TYPE_CHECKING

from sts2_env.cards.enchantments import can_enchant_card
from sts2_env.cards.factory import create_card, create_transform_card, eligible_character_cards
from sts2_env.core.enums import CardId, CardRarity, CardType, TargetType
from sts2_env.cards.status import (
    make_bad_luck,
    make_clumsy,
    make_decay,
    make_doubt,
    make_exterminate,
    make_greed,
    make_guilty,
    make_injury,
    make_lantern_key,
    make_metamorphosis,
    make_poor_sleep,
    make_regret,
    make_shame,
    make_spore_mind,
    make_squash,
)
from sts2_env.potions.base import create_potion
from sts2_env.relics.base import RelicId
from sts2_env.run.reward_objects import (
    AddCardsReward,
    CardReward,
    EnchantCardsReward,
    LoseGoldReward,
    LoseHpReward,
    PotionReward,
    RelicReward,
    RemoveCardReward,
    TransformCardsReward,
    UpgradeCardsReward,
)
from sts2_env.run.events import EventModel, EventOption, EventResult, register_event
from sts2_env.characters.all import ALL_CHARACTERS

if TYPE_CHECKING:
    from sts2_env.run.run_state import RunState


# ── Helpers ──────────────────────────────────────────────────────────

def _hp_ge(run_state: RunState, threshold: int) -> bool:
    return run_state.player.current_hp >= threshold


def _gold_ge(run_state: RunState, threshold: int) -> bool:
    return run_state.player.gold >= threshold


def _should_defer_event_rewards(run_state: RunState) -> bool:
    return getattr(run_state, "defer_followup_rewards", False)


def _event_result_with_rewards(description: str, rewards: list[object]) -> EventResult:
    return EventResult(finished=True, description=description, rewards={"reward_objects": rewards})


def _roll_random_relic_rewards(run_state: RunState, count: int) -> list[RelicReward]:
    rewards: list[RelicReward] = []
    rolled: set[str] = set()
    for _ in range(max(0, count)):
        relic_id = run_state.player.roll_relic_reward_id(excluded_relic_ids=rolled)
        if relic_id is None:
            continue
        rolled.add(relic_id)
        rewards.append(RelicReward(run_state.player.player_id, relic_id=relic_id))
    return rewards


def _obtain_random_relics(run_state: RunState, count: int) -> list[str]:
    if _should_defer_event_rewards(run_state):
        rewards = _roll_random_relic_rewards(run_state, count)
        run_state.pending_rewards.extend(rewards)
        return [reward.relic_id for reward in rewards if reward.relic_id is not None]
    obtained: list[str] = []
    for _ in range(max(0, count)):
        reward = RelicReward(run_state.player.player_id)
        reward.populate(run_state, None)
        if reward.relic_id is not None:
            run_state.player.obtain_relic(reward.relic_id)
            obtained.append(reward.relic_id)
    return obtained


def _obtain_relic_with_setup(run_state: RunState, relic_id: str, **attrs: object) -> bool:
    return run_state.player.obtain_relic_with_setup(relic_id, setup_attrs=attrs)


def _add_random_character_card(
    run_state: RunState,
    *,
    card_type: str | None = None,
    cost: int | None = None,
    costs_x: bool | None = None,
) -> bool:
    from sts2_env.core.enums import CardType

    type_filter = CardType[card_type] if isinstance(card_type, str) else None
    candidates = eligible_character_cards(
        run_state.player.character_id,
        card_type=type_filter,
        generation_context=None,
    )
    filtered = []
    for card_id in candidates:
        card = create_card(card_id)
        if cost is not None and card.cost != cost:
            continue
        if costs_x is not None and card.has_energy_cost_x != costs_x:
            continue
        filtered.append(card)
    if not filtered:
        return False
    card = run_state.rng.rewards.choice(filtered)
    if _should_defer_event_rewards(run_state):
        run_state.pending_rewards.append(AddCardsReward(run_state.player.player_id, [card]))
        return True
    run_state.player.add_card_instance_to_deck(card)
    return True


def _upgrade_n_cards(run_state: RunState, count: int) -> int:
    if _should_defer_event_rewards(run_state):
        run_state.pending_rewards.append(
            UpgradeCardsReward(
                run_state.player.player_id,
                count=count,
                cards=run_state.player.upgradable_deck_cards(),
            )
        )
        return 0
    return run_state.player.upgrade_random_cards(None, count)


def _transform_n_cards(run_state: RunState, count: int) -> int:
    if _should_defer_event_rewards(run_state):
        run_state.pending_rewards.append(
            TransformCardsReward(
                run_state.player.player_id,
                count=count,
                cards=run_state.player.transformable_deck_cards(),
            )
        )
        return 0
    transformed = 0
    candidates = run_state.player.transformable_deck_cards()
    run_state.rng.niche.shuffle(candidates)
    for old_card in candidates[:count]:
        new_card = create_transform_card(
            old_card,
            character_id=run_state.player.character_id,
            rng=run_state.rng.niche,
            generation_context=None,
        )
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
        if removed < count and card.rarity.name != "QUEST" and card.is_removable:
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


def _downgrade_selected_cards(cards: list, run_state: RunState) -> int:
    downgraded = 0
    for card in cards:
        if not card.upgraded:
            continue
        try:
            base_card = create_card(card.card_id, upgraded=False)
        except KeyError:
            continue
        card.cost = base_card.cost
        card.card_type = base_card.card_type
        card.target_type = base_card.target_type
        card.rarity = base_card.rarity
        card.base_damage = base_card.base_damage
        card.base_block = base_card.base_block
        card.upgraded = base_card.upgraded
        card.keywords = base_card.keywords
        card.tags = base_card.tags
        card.can_be_generated_in_combat = base_card.can_be_generated_in_combat
        card.can_be_generated_by_modifiers = base_card.can_be_generated_by_modifiers
        card.effect_vars = dict(base_card.effect_vars)
        card.original_cost = base_card.original_cost
        downgraded += 1
    return downgraded


def _transform_selected_cards(cards: list, run_state: RunState) -> int:
    transformed = 0
    for old_card in cards:
        new_card = create_transform_card(
            old_card,
            character_id=run_state.player.character_id,
            rng=run_state.rng.niche,
            generation_context=None,
        )
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
        if id(card) in selected_ids and card.is_removable:
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
        self._linger_count = 0
        self._damage = 3

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        self._linger_count = 0
        self._damage = 3
        return [
            EventOption("immerse", "Immerse", f"+2 Max HP, take {self._damage} damage"),
            EventOption("abstain", "Abstain", "Heal 10 HP"),
        ]

    def _on_immerse(self, run_state: RunState) -> int:
        damage = self._damage
        run_state.player.gain_max_hp(2)
        run_state.player.lose_hp(damage)
        self._damage += 1
        return damage

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "abstain":
            healed = run_state.player.heal(10)
            return EventResult(finished=True, description=f"Healed {healed} HP.")
        if option_id == "exit_baths":
            return EventResult(finished=True, description="Left the baths.")
        if option_id == "immerse":
            damage = self._on_immerse(run_state)
            return EventResult(
                finished=False,
                description=f"+2 Max HP, took {damage} damage.",
                next_options=[
                    EventOption("linger", "Linger", f"+2 Max HP, take {self._damage} damage"),
                    EventOption("exit_baths", "Exit Baths", "Leave the baths"),
                ],
            )
        if option_id == "linger":
            self._linger_count = min(self._linger_count + 1, 9)
            damage = self._on_immerse(run_state)
            return EventResult(
                finished=False,
                description=f"+2 Max HP, took {damage} damage.",
                next_options=[
                    EventOption("linger", "Linger", f"+2 Max HP, take {self._damage} damage"),
                    EventOption("exit_baths", "Exit Baths", "Leave the baths"),
                ],
            )
        return EventResult(finished=True, description="Nothing happened.")


register_event(AbyssalBaths())


# ── Amalgamator ──────────────────────────────────────────────────────

class Amalgamator(EventModel):
    """Remove 2 Strike/Defend cards and gain UltimateStrike/UltimateDefend."""

    event_id = "Amalgamator"

    @staticmethod
    def _is_valid(card, tag: str) -> bool:
        if card.rarity != CardRarity.BASIC or not card.is_removable:
            return False
        return tag in card.card_id.name

    def is_allowed(self, run_state: RunState) -> bool:
        deck = run_state.player.deck
        strike_count = sum(
            1 for c in deck
            if self._is_valid(c, "STRIKE")
        )
        defend_count = sum(
            1 for c in deck
            if self._is_valid(c, "DEFEND")
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
        if option_id == "combine_strikes":
            candidates = [card for card in run_state.player.deck if self._is_valid(card, "STRIKE")]
            if len(candidates) < 2:
                return EventResult(finished=True, description="No valid Strikes to combine.")
            if _should_defer_event_rewards(run_state):
                return _event_result_with_rewards(
                    "Combined 2 Strikes into Ultimate Strike.",
                    [
                        RemoveCardReward(run_state.player.player_id, count=2, cards=candidates),
                        AddCardsReward(run_state.player.player_id, [create_card(CardId.ULTIMATE_STRIKE)]),
                    ],
                )
            return self.request_card_choice(
                prompt="Choose 2 Strikes to combine",
                cards=candidates,
                source_pile="deck",
                resolver=lambda selected: (
                    _remove_selected_cards(selected, run_state),
                    run_state.player.add_card_instance_to_deck(create_card(CardId.ULTIMATE_STRIKE)),
                    EventResult(finished=True, description="Combined 2 Strikes into Ultimate Strike."),
                )[-1],
                allow_skip=False,
                min_count=2,
                max_count=2,
                description="Choose 2 Strikes to remove.",
            )
        if option_id == "combine_defends":
            candidates = [card for card in run_state.player.deck if self._is_valid(card, "DEFEND")]
            if len(candidates) < 2:
                return EventResult(finished=True, description="No valid Defends to combine.")
            if _should_defer_event_rewards(run_state):
                return _event_result_with_rewards(
                    "Combined 2 Defends into Ultimate Defend.",
                    [
                        RemoveCardReward(run_state.player.player_id, count=2, cards=candidates),
                        AddCardsReward(run_state.player.player_id, [create_card(CardId.ULTIMATE_DEFEND)]),
                    ],
                )
            return self.request_card_choice(
                prompt="Choose 2 Defends to combine",
                cards=candidates,
                source_pile="deck",
                resolver=lambda selected: (
                    _remove_selected_cards(selected, run_state),
                    run_state.player.add_card_instance_to_deck(create_card(CardId.ULTIMATE_DEFEND)),
                    EventResult(finished=True, description="Combined 2 Defends into Ultimate Defend."),
                )[-1],
                allow_skip=False,
                min_count=2,
                max_count=2,
                description="Choose 2 Defends to remove.",
            )
        return EventResult(finished=True, description="Nothing happened.")


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
            candidates = run_state.player.transformable_deck_cards()
            if not candidates:
                return EventResult(finished=True, description="Transformed a card.")
            if _should_defer_event_rewards(run_state):
                return _event_result_with_rewards(
                    "Transformed a card.",
                    [TransformCardsReward(run_state.player.player_id, count=1, cards=candidates)],
                )
            return self.request_card_choice(
                prompt="Choose a card to transform",
                cards=candidates,
                source_pile="deck",
                resolver=lambda selected: (
                    _transform_selected_cards(selected, run_state),
                    EventResult(finished=True, description="Transformed a card."),
                )[-1],
                allow_skip=False,
                min_count=1,
                max_count=1,
                description="Choose a card to transform.",
            )
        if option_id == "maintain_control":
            candidates = run_state.player.upgradable_deck_cards()
            if not candidates:
                return EventResult(finished=True, description="Upgraded a card.")
            if _should_defer_event_rewards(run_state):
                return _event_result_with_rewards(
                    "Upgraded a card.",
                    [UpgradeCardsReward(run_state.player.player_id, count=1, cards=candidates)],
                )
            return self.request_card_choice(
                prompt="Choose a card to upgrade",
                cards=candidates,
                source_pile="deck",
                resolver=lambda selected: (
                    _upgrade_selected_cards(selected, run_state),
                    EventResult(finished=True, description="Upgraded a card."),
                )[-1],
                allow_skip=False,
                min_count=1,
                max_count=1,
                description="Choose a card to upgrade.",
            )
        return EventResult(finished=True, description="Nothing happened.")


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
            if _should_defer_event_rewards(run_state):
                return _event_result_with_rewards(
                    "Gained Exterminate card.",
                    [AddCardsReward(run_state.player.player_id, [make_exterminate()])],
                )
            run_state.player.add_card_to_deck("Exterminate")
            return EventResult(finished=True, description="Gained Exterminate card.")
        if _should_defer_event_rewards(run_state):
            return _event_result_with_rewards(
                "Gained Squash card.",
                [AddCardsReward(run_state.player.player_id, [make_squash()])],
            )
        run_state.player.add_card_to_deck("Squash")
        return EventResult(finished=True, description="Gained Squash card.")


register_event(Bugslayer())


# ── ByrdonisNest ─────────────────────────────────────────────────────

class ByrdonisNest(EventModel):
    """Eat: +7 Max HP. Take: gain Byrdonis Egg card (event pet)."""

    event_id = "ByrdonisNest"

    def is_allowed(self, run_state: RunState) -> bool:
        return not run_state.player.has_event_pet()

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("eat", "Eat", "+7 Max HP"),
            EventOption("take", "Take the Egg", "Gain Byrdonis Egg card"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "eat":
            run_state.player.gain_max_hp(7)
            return EventResult(finished=True, description="Gained 7 Max HP.")
        if _should_defer_event_rewards(run_state):
            return _event_result_with_rewards(
                "Gained Byrdonis Egg card.",
                [AddCardsReward(run_state.player.player_id, [create_card(CardId.BYRDONIS_EGG)])],
            )
        run_state.player.add_card_to_deck("ByrdonisEgg")
        return EventResult(finished=True, description="Gained Byrdonis Egg card.")


register_event(ByrdonisNest())


# ── ColorfulPhilosophers ────────────────────────────────────────────

class ColorfulPhilosophers(EventModel):
    """Choose a philosopher color to get card rewards from that color's pool."""

    event_id = "ColorfulPhilosophers"

    _POOL_ORDER = ("Necrobinder", "Ironclad", "Regent", "Silent", "Defect")

    def __init__(self) -> None:
        self._choices: dict[str, str] = {}

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        available = [cid for cid in self._POOL_ORDER if cid != run_state.player.character_id]
        run_state.rng.up_front.shuffle(available)
        chosen = available[:3]
        self._choices = {f"pool_{i + 1}": cid for i, cid in enumerate(chosen)}
        return [
            EventOption(option_id, f"{cid} Philosopher", f"Card rewards from {cid} pool")
            for option_id, cid in self._choices.items()
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        character_id = self._choices.get(option_id)
        if character_id is None:
            return EventResult(finished=True, description="Nothing happened.")
        rewards = [
            CardReward(
                run_state.player.player_id,
                option_count=3,
                character_ids=(character_id,),
                forced_rarities=(rarity, rarity, rarity),
                use_default_character_pool=False,
            )
            for rarity in (CardRarity.COMMON, CardRarity.UNCOMMON, CardRarity.RARE)
        ]
        return EventResult(
            finished=True,
            description=f"Received card rewards from {character_id} pool.",
            rewards={"reward_objects": rewards},
        )


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
        if option_id == "pollinous_core":
            dmg = self._PRIZE_DAMAGE[self._digs]
            run_state.player.lose_hp(dmg)
            if _should_defer_event_rewards(run_state):
                return _event_result_with_rewards(
                    "Obtained Pollinous Core.",
                    [RelicReward(run_state.player.player_id, relic_id=RelicId.POLLINOUS_CORE.name)],
                )
            run_state.player.obtain_relic(RelicId.POLLINOUS_CORE.name)
            return EventResult(finished=True, description="Obtained Pollinous Core.")

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

    def __init__(self) -> None:
        self._choices: dict[str, tuple[str, dict[str, object]]] = {}

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        from sts2_env.relics.registry import create_relic_by_name

        source: list[str] = [
            RelicId.ASTROLABE.name,
            RelicId.BLACK_STAR.name,
            RelicId.CALLING_BELL.name,
            RelicId.EMPTY_CAGE.name,
            RelicId.RUNIC_PYRAMID.name,
            RelicId.SNECKO_EYE.name,
        ]
        modifiers = getattr(run_state, "modifiers", [])
        if not any(getattr(modifier, "clears_player_deck", False) for modifier in modifiers):
            source.append(RelicId.PANDORAS_BOX.name)
        if run_state.current_act_index == 1:
            source.append(run_state.rng.up_front.choice([RelicId.ECTOPLASM.name, RelicId.SOZU.name]))
        elif run_state.current_act_index == 2:
            source.append(run_state.rng.up_front.choice([RelicId.PHILOSOPHERS_STONE.name, RelicId.VELVET_CHOKER.name]))
        run_state.rng.up_front.shuffle(source)
        dusty_tome = create_relic_by_name(RelicId.DUSTY_TOME.name)
        dusty_tome_setup = getattr(dusty_tome, "setup_for_player", None)
        if callable(dusty_tome_setup):
            dusty_tome_setup(run_state.player)
        dusty_attrs: dict[str, object] = {}
        if getattr(dusty_tome, "_ancient_card_id", None) is not None:
            dusty_attrs["_ancient_card_id"] = dusty_tome._ancient_card_id
        if run_state.rng.up_front.next_int(0, 1) == 1:
            chosen = [(relic_id, {}) for relic_id in source[:2]]
            chosen.append((RelicId.DUSTY_TOME.name, dusty_attrs))
        else:
            chosen = [(relic_id, {}) for relic_id in source[:3]]
        self._choices = {f"relic_{i + 1}": choice for i, choice in enumerate(chosen)}
        return [
            EventOption(option_id, relic_id.replace("_", " ").title(), "Gain a boss relic")
            for option_id, (relic_id, _) in self._choices.items()
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        choice = self._choices.get(option_id)
        if choice is not None:
            relic_id, attrs = choice
            if _should_defer_event_rewards(run_state):
                return _event_result_with_rewards(
                    "Obtained a boss relic from Darv.",
                    [RelicReward(run_state.player.player_id, relic_id=relic_id, setup_attrs=attrs)],
                )
            _obtain_relic_with_setup(run_state, relic_id, **attrs)
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
            candidates = run_state.player.removable_deck_cards()
            if _should_defer_event_rewards(run_state):
                return _event_result_with_rewards(
                    "Removed a card, took 11 damage.",
                    [
                        RemoveCardReward(run_state.player.player_id, count=1, cards=candidates),
                        LoseHpReward(run_state.player.player_id, 11),
                    ],
                )
            return self.request_card_choice(
                prompt="Choose 1 card to remove",
                cards=candidates,
                source_pile="deck",
                resolver=lambda selected: (
                    _remove_selected_cards(selected, run_state),
                    run_state.player.lose_hp(11),
                    EventResult(finished=True, description="Removed a card, took 11 damage."),
                )[-1],
                allow_skip=False,
                min_count=1,
                max_count=1,
                description="Choose a card to remove.",
            )
        if option_id == "rest":
            heal_amount = int(run_state.player.max_hp * 0.3)
            run_state.player.heal(heal_amount)
            return EventResult(
                finished=False,
                description=f"Healed {heal_amount} HP and disturbed the growth.",
                next_options=[EventOption("fight", "Fight", "Fight through the vegetation")],
            )
        return EventResult(
            finished=True,
            description="Fought through the vegetation.",
            event_combat_setup="dense_vegetation",
        )


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
            _upgrade_n_cards(run_state, 2)
            return EventResult(finished=True, description="Upgraded 2 cards.")
        candidates = run_state.player.removable_deck_cards()
        if _should_defer_event_rewards(run_state):
            return _event_result_with_rewards(
                "Removed 1 card.",
                [RemoveCardReward(run_state.player.player_id, count=1, cards=candidates)],
            )
        return self.request_card_choice(
            prompt="Choose 1 card to remove",
            cards=candidates,
            source_pile="deck",
            resolver=lambda selected: EventResult(
                finished=True,
                description=f"Removed {_remove_selected_cards(selected, run_state)} card."
            ),
            allow_skip=False,
            min_count=1,
            max_count=1,
            description="Choose a card to remove.",
        )


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
            return EventResult(
                finished=True,
                description="Gained Glowwater Potion.",
                rewards={"reward_objects": [PotionReward(run_state.player.player_id, potion_id="GlowwaterPotion")]},
            )
        run_state.player.lose_max_hp(13)
        if _should_defer_event_rewards(run_state):
            return _event_result_with_rewards(
                "Lost 13 Max HP, gained Fresnel Lens relic.",
                [RelicReward(run_state.player.player_id, relic_id=RelicId.FRESNEL_LENS.name)],
            )
        run_state.player.obtain_relic(RelicId.FRESNEL_LENS.name)
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
        confront_enabled = any(can_enchant_card(card, "SoulsPower") for card in run_state.player.deck)
        return [
            EventOption("confront", "Confront", "Gain Decay curse, enchant a card", enabled=confront_enabled),
            EventOption("accept", "Accept", "Gain Forgotten Soul relic"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "confront":
            if _should_defer_event_rewards(run_state):
                candidates = [card for card in run_state.player.deck if can_enchant_card(card, "SoulsPower")]
                return _event_result_with_rewards(
                    "Gained Decay curse and Soul's Power enchantment.",
                    [
                        AddCardsReward(run_state.player.player_id, [make_decay()]),
                        EnchantCardsReward(
                            run_state.player.player_id,
                            enchantment="SoulsPower",
                            amount=1,
                            count=1,
                            cards=candidates,
                        ),
                    ],
                )
            run_state.player.add_card_instance_to_deck(make_decay())
            return self.request_card_choice(
                prompt="Choose a card to enchant with Soul's Power",
                cards=[card for card in run_state.player.deck if can_enchant_card(card, "SoulsPower")],
                source_pile="deck",
                resolver=lambda selected: (
                    selected and selected[0].add_enchantment("SoulsPower", 1),
                    EventResult(finished=True, description="Gained Decay curse and Soul's Power enchantment."),
                )[-1],
                description="Choose a card to enchant.",
            )
        if _should_defer_event_rewards(run_state):
            return _event_result_with_rewards(
                "Gained Forgotten Soul relic.",
                [RelicReward(run_state.player.player_id, relic_id=RelicId.FORGOTTEN_SOUL.name)],
            )
        run_state.player.obtain_relic(RelicId.FORGOTTEN_SOUL.name)
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
            if _should_defer_event_rewards(run_state):
                return _event_result_with_rewards(
                    "Gained Big Mushroom relic.",
                    [RelicReward(run_state.player.player_id, relic_id=RelicId.BIG_MUSHROOM.name)],
                )
            run_state.player.obtain_relic(RelicId.BIG_MUSHROOM.name)
            return EventResult(finished=True, description="Gained Big Mushroom relic.")
        if _should_defer_event_rewards(run_state):
            return _event_result_with_rewards(
                "Took 15 damage, gained Fragrant Mushroom relic.",
                [RelicReward(run_state.player.player_id, relic_id=RelicId.FRAGRANT_MUSHROOM.name)],
            )
        run_state.player.obtain_relic(RelicId.FRAGRANT_MUSHROOM.name)
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
            _add_random_character_card(run_state, card_type="POWER")
            return EventResult(finished=True, description="Gained a random Power card.")
        _add_random_character_card(run_state, cost=0, costs_x=False)
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
            if _should_defer_event_rewards(run_state):
                return _event_result_with_rewards(
                    "Gained Lost Wisp relic and Decay curse.",
                    [
                        RelicReward(run_state.player.player_id, relic_id=RelicId.LOST_WISP.name),
                        AddCardsReward(run_state.player.player_id, [make_decay()]),
                    ],
                )
            run_state.player.obtain_relic(RelicId.LOST_WISP.name)
            run_state.player.add_card_instance_to_deck(make_decay())
            return EventResult(finished=True,
                               description="Gained Lost Wisp relic and Decay curse.")
        run_state.player.gain_gold(self._gold)
        return EventResult(finished=True, description=f"Gained {self._gold} gold.")


register_event(LostWisp())


# ── Nonupeipe ────────────────────────────────────────────────────────

class Nonupeipe(EventModel):
    """Ancient event: choose from 3 randomized relic options."""

    event_id = "Nonupeipe"

    def __init__(self) -> None:
        self._choices: dict[str, str] = {}

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        pool = [
            RelicId.BLESSED_ANTLER.name,
            RelicId.BRILLIANT_SCARF.name,
            RelicId.DELICATE_FROND.name,
            RelicId.DIAMOND_DIADEM.name,
            RelicId.FUR_COAT.name,
            RelicId.GLITTER.name,
            RelicId.JEWELRY_BOX.name,
            RelicId.LOOMING_FRUIT.name,
            RelicId.SIGNET_RING.name,
        ]
        if sum(1 for card in run_state.player.deck if can_enchant_card(card, "Swift")) >= 4:
            pool.append(RelicId.BEAUTIFUL_BRACELET.name)
        run_state.rng.up_front.shuffle(pool)
        chosen = pool[:3]
        self._choices = {f"relic_{i + 1}": relic_id for i, relic_id in enumerate(chosen)}
        return [
            EventOption(option_id, relic_id.replace("_", " ").title(), "Gain a relic")
            for option_id, relic_id in self._choices.items()
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        relic_id = self._choices.get(option_id)
        if relic_id is not None:
            if _should_defer_event_rewards(run_state):
                return _event_result_with_rewards(
                    "Obtained a relic from Nonupeipe.",
                    [RelicReward(run_state.player.player_id, relic_id=relic_id)],
                )
            run_state.player.obtain_relic(relic_id)
        return EventResult(finished=True, description="Obtained a relic from Nonupeipe.")


register_event(Nonupeipe())


# ── Orobas ───────────────────────────────────────────────────────────

class Orobas(EventModel):
    """Ancient event: 3 relic offers from different pools."""

    event_id = "Orobas"

    def __init__(self) -> None:
        self._choices: dict[str, tuple[str, dict[str, object]]] = {}

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        from sts2_env.relics.registry import create_relic_by_name

        pool1: list[tuple[str, dict[str, object]]] = [
            (RelicId.ELECTRIC_SHRYMP.name, {}),
            (RelicId.GLASS_EYE.name, {}),
            (RelicId.SAND_CASTLE.name, {}),
        ]
        off_character_ids = [character.character_id for character in ALL_CHARACTERS if character.character_id != run_state.player.character_id]
        if run_state.rng.up_front.next_float() < 0.3333333:
            pool1.append((RelicId.PRISMATIC_GEM.name, {}))
        elif off_character_ids:
            pool1.append((RelicId.SEA_GLASS.name, {"_character_id": run_state.rng.up_front.choice(off_character_ids)}))
        pool2: list[tuple[str, dict[str, object]]] = [
            (RelicId.ALCHEMICAL_COFFER.name, {}),
            (RelicId.DRIFTWOOD.name, {}),
            (RelicId.RADIANT_PEARL.name, {}),
        ]
        pool3: list[tuple[str, dict[str, object]]] = []
        touch_of_orobas = create_relic_by_name(RelicId.TOUCH_OF_OROBAS.name)
        touch_setup = getattr(touch_of_orobas, "setup_for_player", None)
        if callable(touch_setup) and touch_setup(run_state.player):
            attrs: dict[str, object] = {}
            if getattr(touch_of_orobas, "_starter_relic_id", None) is not None:
                attrs["_starter_relic_id"] = touch_of_orobas._starter_relic_id
            if getattr(touch_of_orobas, "_upgraded_relic_id", None) is not None:
                attrs["_upgraded_relic_id"] = touch_of_orobas._upgraded_relic_id
            pool3.append((RelicId.TOUCH_OF_OROBAS.name, attrs))
        archaic_tooth = create_relic_by_name(RelicId.ARCHAIC_TOOTH.name)
        archaic_setup = getattr(archaic_tooth, "setup_for_player", None)
        if callable(archaic_setup) and archaic_setup(run_state.player):
            attrs = {}
            if getattr(archaic_tooth, "_starter_card_id", None) is not None:
                attrs["_starter_card_id"] = archaic_tooth._starter_card_id
            if getattr(archaic_tooth, "_ancient_card_id", None) is not None:
                attrs["_ancient_card_id"] = archaic_tooth._ancient_card_id
            pool3.append((RelicId.ARCHAIC_TOOTH.name, attrs))
        self._choices = {
            "relic_1": run_state.rng.up_front.choice(pool1),
            "relic_2": run_state.rng.up_front.choice(pool2),
        }
        options = [
            EventOption(option_id, relic_id.replace("_", " ").title(), "Gain a relic")
            for option_id, (relic_id, _) in self._choices.items()
        ]
        if pool3:
            self._choices["relic_3"] = run_state.rng.up_front.choice(pool3)
            relic_id, _ = self._choices["relic_3"]
            options.append(EventOption("relic_3", relic_id.replace("_", " ").title(), "Gain a relic"))
        else:
            options.append(EventOption("relic_3", "Locked", "No valid relic", enabled=False))
        return options

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        choice = self._choices.get(option_id)
        if choice is not None:
            relic_id, attrs = choice
            if _should_defer_event_rewards(run_state):
                return _event_result_with_rewards(
                    "Obtained a relic from Orobas.",
                    [RelicReward(run_state.player.player_id, relic_id=relic_id, setup_attrs=attrs)],
                )
            _obtain_relic_with_setup(run_state, relic_id, **attrs)
        return EventResult(finished=True, description="Obtained a relic from Orobas.")


register_event(Orobas())


# ── Pael ─────────────────────────────────────────────────────────────

class Pael(EventModel):
    """Ancient event: 3 relic offers from Pael's body parts."""

    event_id = "Pael"

    def __init__(self) -> None:
        self._choices: dict[str, str] = {}

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        pool1 = [RelicId.PAELS_FLESH.name, RelicId.PAELS_HORN.name, RelicId.PAELS_TEARS.name]
        pool2 = [RelicId.PAELS_WING.name, RelicId.PAELS_GROWTH.name]
        if sum(1 for card in run_state.player.deck if can_enchant_card(card, "Goopy")) >= 3:
            pool2.append(RelicId.PAELS_CLAW.name)
        if len(run_state.player.removable_deck_cards()) >= 5:
            pool2.append(RelicId.PAELS_TOOTH.name)
        pool2 = pool2 + pool2
        pool3 = [RelicId.PAELS_EYE.name, RelicId.PAELS_BLOOD.name]
        if not run_state.player.has_event_pet():
            pool3.append(RelicId.PAELS_LEGION.name)
        self._choices = {
            "relic_1": run_state.rng.up_front.choice(pool1),
            "relic_2": run_state.rng.up_front.choice(pool2),
            "relic_3": run_state.rng.up_front.choice(pool3),
        }
        return [
            EventOption(option_id, relic_id.replace("_", " ").title(), "Gain a Pael relic")
            for option_id, relic_id in self._choices.items()
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        relic_id = self._choices.get(option_id)
        if relic_id is not None:
            if _should_defer_event_rewards(run_state):
                return _event_result_with_rewards(
                    "Obtained a relic from Pael.",
                    [RelicReward(run_state.player.player_id, relic_id=relic_id)],
                )
            run_state.player.obtain_relic(relic_id)
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
            if _should_defer_event_rewards(run_state):
                return _event_result_with_rewards(
                    "Gained Injury curse and a relic.",
                    [
                        AddCardsReward(run_state.player.player_id, [make_injury()]),
                        RelicReward(run_state.player.player_id),
                    ],
                )
            run_state.player.add_card_instance_to_deck(make_injury())
            return EventResult(
                finished=True,
                description="Gained Injury curse and a relic.",
                rewards={"reward_objects": [RelicReward(run_state.player.player_id)]},
            )
        if option_id == "fight":
            return EventResult(
                finished=True,
                description="Fought and earned relic + potion.",
                rewards={"reward_objects": [RelicReward(run_state.player.player_id), PotionReward(run_state.player.player_id)]},
                event_combat_setup="punch_off",
            )
        return EventResult(
            finished=False,
            description="The constructs turn toward you.",
            next_options=[EventOption("fight", "Fight", "Fight for relic + potion")],
        )


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
        if option_id == "fight":
            return EventResult(
                finished=True,
                description="Fought and earned relic + potion.",
                rewards={"reward_objects": [RelicReward(run_state.player.player_id), PotionReward(run_state.player.player_id)]},
            )
        if option_id == "touch":
            upgraded_cards = [card for card in run_state.player.deck if card.upgraded]
            run_state.rng.niche.shuffle(upgraded_cards)
            downgraded_cards = upgraded_cards[:2]
            _downgrade_selected_cards(downgraded_cards, run_state)

            downgraded_ids = {id(card) for card in downgraded_cards}
            upgrade_candidates = [
                card for card in run_state.player.deck
                if not card.upgraded and id(card) not in downgraded_ids
            ]
            run_state.rng.niche.shuffle(upgrade_candidates)
            _upgrade_selected_cards(upgrade_candidates[:4], run_state)
            return EventResult(finished=True,
                               description="Downgraded 2 and upgraded 4 cards.")

        original_deck = list(run_state.player.deck)
        if _should_defer_event_rewards(run_state):
            clones = [
                card.clone(20_000_000 + len(original_deck) + index)
                for index, card in enumerate(original_deck)
            ]
            clones.append(make_bad_luck())
            return _event_result_with_rewards(
                "Duplicated deck, gained Bad Luck curse.",
                [AddCardsReward(run_state.player.player_id, clones)],
            )
        for card in original_deck:
            run_state.player.add_card_instance_to_deck(card.clone(20_000_000 + len(run_state.player.deck)))
        run_state.player.add_card_instance_to_deck(make_bad_luck())
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
            if _should_defer_event_rewards(run_state):
                return _event_result_with_rewards(
                    "Gained Royal Poison relic, healed to full.",
                    [RelicReward(run_state.player.player_id, relic_id=RelicId.ROYAL_POISON.name)],
                )
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
        if _should_defer_event_rewards(run_state):
            return _event_result_with_rewards(
                "Took 11 damage, gained a relic.",
                _roll_random_relic_rewards(run_state, 1),
            )
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
            if _should_defer_event_rewards(run_state):
                return _event_result_with_rewards(
                    "Healed 9 HP, upgraded a card.",
                    [UpgradeCardsReward(run_state.player.player_id, count=1, cards=candidates)],
                )
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
        candidates = [card for card in run_state.player.deck if can_enchant_card(card, "Sown")]
        if _should_defer_event_rewards(run_state):
            return _event_result_with_rewards(
                "Enchanted a card with Sown.",
                [EnchantCardsReward(run_state.player.player_id, enchantment="Sown", amount=1, count=1, cards=candidates)],
            )
        return self.request_card_choice(
            prompt="Choose a card to enchant with Sown",
            cards=candidates,
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
        attack_available = any(
            card.card_type == CardType.ATTACK and can_enchant_card(card, "Sharp")
            for card in run_state.player.deck
        )
        skill_available = any(
            card.card_type == CardType.SKILL and can_enchant_card(card, "Nimble")
            for card in run_state.player.deck
        )
        power_available = any(
            card.card_type == CardType.POWER and can_enchant_card(card, "Swift")
            for card in run_state.player.deck
        )
        if not any((attack_available, skill_available, power_available)):
            return [EventOption("skip_book", "No Options", "No valid cards")]
        return [
            EventOption("read_back", "Read the Back", "Enchant an Attack with Sharp +2", enabled=attack_available),
            EventOption("read_passage", "Read a Passage", "Enchant a Skill with Nimble +2", enabled=skill_available),
            EventOption("read_entire", "Read Entire Book", "Enchant a Power with Swift +2", enabled=power_available),
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
        if option_id == "skip_book":
            return EventResult(finished=True, description="Closed the book without learning anything.")
        candidates = [card for card in run_state.player.deck if card.card_type.name == card_type_name]
        candidates = [card for card in candidates if can_enchant_card(card, enchantment)]
        if _should_defer_event_rewards(run_state):
            return _event_result_with_rewards(
                description,
                [EnchantCardsReward(run_state.player.player_id, enchantment=enchantment, amount=amount, count=1, cards=candidates)],
            )
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
            if _should_defer_event_rewards(run_state):
                return _event_result_with_rewards(
                    "Healed 25 HP, gained Metamorphosis card.",
                    [AddCardsReward(run_state.player.player_id, [make_metamorphosis()])],
                )
            run_state.player.add_card_instance_to_deck(make_metamorphosis())
            return EventResult(finished=True,
                               description="Healed 25 HP, gained Metamorphosis card.")
        candidates = run_state.player.removable_deck_cards()
        if _should_defer_event_rewards(run_state):
            return _event_result_with_rewards(
                "Removed a card, took 9 damage.",
                [
                    RemoveCardReward(run_state.player.player_id, count=1, cards=candidates),
                    LoseHpReward(run_state.player.player_id, 9),
                ],
            )
        return self.request_card_choice(
            prompt="Choose 1 card to remove",
            cards=candidates,
            source_pile="deck",
            resolver=lambda selected: (
                _remove_selected_cards(selected, run_state),
                run_state.player.lose_hp(9),
                EventResult(finished=True, description="Removed a card, took 9 damage."),
            )[-1],
            allow_skip=False,
            min_count=1,
            max_count=1,
            description="Choose a card to remove.",
        )


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
            if _should_defer_event_rewards(run_state):
                return _event_result_with_rewards(
                    "Gained Sword of Stone relic.",
                    [RelicReward(run_state.player.player_id, relic_id=RelicId.SWORD_OF_STONE.name)],
                )
            run_state.player.obtain_relic(RelicId.SWORD_OF_STONE.name)
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
        if _should_defer_event_rewards(run_state):
            return _event_result_with_rewards(
                f"Gained {self._large_gold} gold and Greed curse.",
                [AddCardsReward(run_state.player.player_id, [make_greed()])],
            )
        run_state.player.add_card_instance_to_deck(make_greed())
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
        if option_id == "smash":
            run_state.player.heal(20)
            return EventResult(finished=True, description="Smashed tablet, healed 20 HP.")
        if option_id == "give_up":
            return EventResult(finished=True, description="Stopped deciphering the tablet.")

        # Decipher
        final_decipher = self._decipher_count >= 4
        if self._decipher_count < len(self._DECIPHER_COSTS):
            cost = self._DECIPHER_COSTS[self._decipher_count]
        else:
            cost = run_state.player.max_hp - 1
        if cost >= run_state.player.max_hp:
            run_state.player.lose_max_hp(run_state.player.max_hp - 1)
            run_state.player.current_hp = 0
            run_state.lose_run()
            return EventResult(finished=True, description=f"Lost {cost} Max HP and died deciphering.")
        run_state.player.lose_max_hp(cost)
        if final_decipher:
            _upgrade_n_cards(run_state, len(run_state.player.deck))
            run_state.player.current_hp = 0
            run_state.lose_run()
        else:
            _upgrade_n_cards(run_state, 1)
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

    def __init__(self) -> None:
        self._choices: dict[str, str] = {}

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        pool = [
            RelicId.CLAWS.name,
            RelicId.CROSSBOW.name,
            RelicId.IRON_CLUB.name,
            RelicId.MEAT_CLEAVER.name,
            RelicId.SAI.name,
            RelicId.SPIKED_GAUNTLETS.name,
            RelicId.TANXS_WHISTLE.name,
            RelicId.THROWING_AXE.name,
            RelicId.WAR_HAMMER.name,
        ]
        if sum(1 for card in run_state.player.deck if can_enchant_card(card, "Instinct")) >= 3:
            pool.append(RelicId.TRI_BOOMERANG.name)
        run_state.rng.up_front.shuffle(pool)
        chosen = pool[:3]
        self._choices = {f"weapon_{i + 1}": relic_id for i, relic_id in enumerate(chosen)}
        return [
            EventOption(option_id, relic_id.replace("_", " ").title(), "Gain a weapon relic")
            for option_id, relic_id in self._choices.items()
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        relic_id = self._choices.get(option_id)
        if relic_id is not None:
            if _should_defer_event_rewards(run_state):
                return _event_result_with_rewards(
                    "Obtained a weapon relic from Tanx.",
                    [RelicReward(run_state.player.player_id, relic_id=relic_id)],
                )
            run_state.player.obtain_relic(relic_id)
        return EventResult(finished=True, description="Obtained a weapon relic from Tanx.")


register_event(Tanx())


# ── Tezcatara ────────────────────────────────────────────────────────

class Tezcatara(EventModel):
    """Ancient event: choose from 3 comfort relics from 3 pools."""

    event_id = "Tezcatara"

    def __init__(self) -> None:
        self._choices: dict[str, str] = {}

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        pool1 = [RelicId.NUTRITIOUS_SOUP.name, RelicId.VERY_HOT_COCOA.name, RelicId.YUMMY_COOKIE.name]
        pool2 = [RelicId.BIIIG_HUG.name, RelicId.STORYBOOK.name, RelicId.SEAL_OF_GOLD.name, RelicId.TOASTY_MITTENS.name]
        pool3 = [RelicId.GOLDEN_COMPASS.name, RelicId.PUMPKIN_CANDLE.name, RelicId.TOY_BOX.name]
        self._choices = {
            "comfort_1": run_state.rng.up_front.choice(pool1),
            "comfort_2": run_state.rng.up_front.choice(pool2),
            "comfort_3": run_state.rng.up_front.choice(pool3),
        }
        return [
            EventOption(option_id, relic_id.replace("_", " ").title(), "Gain a comfort relic")
            for option_id, relic_id in self._choices.items()
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        relic_id = self._choices.get(option_id)
        if relic_id is not None:
            if _should_defer_event_rewards(run_state):
                return _event_result_with_rewards(
                    "Obtained a relic from Tezcatara.",
                    [RelicReward(run_state.player.player_id, relic_id=relic_id)],
                )
            run_state.player.obtain_relic(relic_id)
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
        if option_id == "keep_key":
            return EventResult(
                finished=False,
                description="The knight advances.",
                next_options=[EventOption("fight", "Fight", "Fight for Lantern Key")],
            )
        return EventResult(
            finished=True,
            description="Fought and gained Lantern Key card.",
            rewards={"reward_objects": [AddCardsReward(run_state.player.player_id, [make_lantern_key()])]},
            event_combat_setup="mysterious_knight",
        )


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
        if _should_defer_event_rewards(run_state):
            return _event_result_with_rewards(
                "Gained a relic and Clumsy curse.",
                [
                    *_roll_random_relic_rewards(run_state, 1),
                    AddCardsReward(run_state.player.player_id, [make_clumsy()]),
                ],
            )
        _obtain_random_relics(run_state, 1)
        run_state.player.add_card_instance_to_deck(make_clumsy())
        return EventResult(finished=True,
                           description="Gained a relic and Clumsy curse.")


register_event(ThisOrThat())


# ── TinkerTime ───────────────────────────────────────────────────────

class TinkerTime(EventModel):
    """Multi-page: Choose card type then rider effect to create a Mad Science card."""

    event_id = "TinkerTime"

    def __init__(self) -> None:
        self._card_type: str = ""
        self._rider_choices: list[str] = []

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("choose_card_type", "Choose Card Type", "Choose the base type for Mad Science"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "choose_card_type":
            card_types = ["attack", "skill", "power"]
            run_state.rng.up_front.shuffle(card_types)
            chosen = card_types[:2]
            return EventResult(
                finished=False,
                description="Choose a card type for Mad Science.",
                next_options=[
                    EventOption(card_type, card_type.title(), f"Create a {card_type.title()}-type Mad Science")
                    for card_type in chosen
                ],
            )
        if option_id in ("attack", "skill", "power"):
            self._card_type = option_id
            if option_id == "attack":
                riders = [("sapping", "Sapping"), ("violence", "Violence")]
            elif option_id == "skill":
                riders = [("energized", "Energized"), ("wisdom", "Wisdom")]
            else:
                riders = [("expertise", "Expertise"), ("curious", "Curious")]
            if option_id == "attack":
                riders.append(("choking", "Choking"))
            elif option_id == "skill":
                riders.append(("chaos", "Chaos"))
            else:
                riders.append(("improvement", "Improvement"))
            run_state.rng.up_front.shuffle(riders)
            riders = riders[:2]
            self._rider_choices = [rider_id for rider_id, _ in riders]
            return EventResult(
                finished=False,
                description=f"Chose {option_id} type. Pick a rider effect.",
                next_options=[
                    EventOption(rider_id, label, f"Add {label} rider")
                    for rider_id, label in riders
                ],
            )
        rider_map = {
            "sapping": 1,
            "violence": 2,
            "choking": 3,
            "energized": 4,
            "wisdom": 5,
            "chaos": 6,
            "expertise": 7,
            "curious": 8,
            "improvement": 9,
        }
        if option_id in rider_map:
            card = create_card(CardId.MAD_SCIENCE)
            if self._card_type == "attack":
                card.card_type = CardType.ATTACK
                card.target_type = TargetType.ANY_ENEMY
                card.base_damage = 12
                card.base_block = None
            elif self._card_type == "skill":
                card.card_type = CardType.SKILL
                card.target_type = TargetType.SELF
                card.base_damage = None
                card.base_block = 8
            else:
                card.card_type = CardType.POWER
                card.target_type = TargetType.SELF
                card.base_damage = None
                card.base_block = None
            card.effect_vars["rider"] = rider_map[option_id]
            if _should_defer_event_rewards(run_state):
                return _event_result_with_rewards(
                    f"Created Mad Science ({self._card_type} + {option_id}).",
                    [AddCardsReward(run_state.player.player_id, [card])],
                )
            run_state.player.add_card_instance_to_deck(card)
            return EventResult(
                finished=True,
                description=f"Created Mad Science ({self._card_type} + {option_id}).",
            )
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
            if _should_defer_event_rewards(run_state):
                return _event_result_with_rewards(
                    "Condemned the merchant, gained two relics and Regret.",
                    [
                        *_roll_random_relic_rewards(run_state, 2),
                        AddCardsReward(run_state.player.player_id, [make_regret()]),
                    ],
                )
            run_state.player.add_card_instance_to_deck(make_regret())
            _obtain_random_relics(run_state, 2)
            return EventResult(finished=True, description="Condemned the merchant, gained two relics and Regret.")
        if option_id == "merchant_innocent":
            if _should_defer_event_rewards(run_state):
                candidates = [card for card in run_state.player.deck if not card.upgraded]
                rewards: list[object] = [
                    AddCardsReward(run_state.player.player_id, [make_shame()]),
                ]
                if candidates:
                    rewards.append(
                        UpgradeCardsReward(
                            run_state.player.player_id,
                            count=min(2, len(candidates)),
                            cards=candidates,
                        )
                    )
                return _event_result_with_rewards(
                    "Spared the merchant, gained Shame and upgraded 2 cards.",
                    rewards,
                )
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
            if _should_defer_event_rewards(run_state):
                run_state.player.gain_gold(300)
                return _event_result_with_rewards(
                    "Freed the noble, gained 300 gold and Regret.",
                    [AddCardsReward(run_state.player.player_id, [make_regret()])],
                )
            run_state.player.add_card_instance_to_deck(make_regret())
            run_state.player.gain_gold(300)
            return EventResult(finished=True, description="Freed the noble, gained 300 gold and Regret.")
        if option_id == "nondescript_guilty":
            if _should_defer_event_rewards(run_state):
                return _event_result_with_rewards(
                    "Condemned the drifter, gained Doubt and two card rewards.",
                    [
                        AddCardsReward(run_state.player.player_id, [make_doubt()]),
                        CardReward(run_state.player.player_id),
                        CardReward(run_state.player.player_id),
                    ],
                )
            run_state.player.add_card_instance_to_deck(make_doubt())
            return EventResult(
                finished=True,
                description="Condemned the drifter, gained Doubt and two card rewards.",
                rewards={"reward_objects": [CardReward(run_state.player.player_id), CardReward(run_state.player.player_id)]},
            )
        if option_id == "nondescript_innocent":
            if _should_defer_event_rewards(run_state):
                return _event_result_with_rewards(
                    "Freed the drifter, gained Doubt and transformed 2 cards.",
                    [
                        AddCardsReward(run_state.player.player_id, [make_doubt()]),
                        TransformCardsReward(run_state.player.player_id, count=2),
                    ],
                )
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
            if _should_defer_event_rewards(run_state):
                return _event_result_with_rewards(
                    f"Healed {heal_amount} HP, gained Poor Sleep curse.",
                    [AddCardsReward(run_state.player.player_id, [make_poor_sleep()])],
                )
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

    def __init__(self) -> None:
        self._choices: dict[str, str] = {}

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        pool1 = [RelicId.BLOOD_SOAKED_ROSE.name, RelicId.WHISPERING_EARRING.name, RelicId.FIDDLE.name]
        pool2 = [RelicId.PRESERVED_FOG.name, RelicId.SERE_TALON.name, RelicId.DISTINGUISHED_CAPE.name]
        pool3 = [RelicId.CHOICES_PARADOX.name, RelicId.MUSIC_BOX.name, RelicId.LORDS_PARASOL.name, RelicId.JEWELED_MASK.name]
        run_state.rng.up_front.shuffle(pool1)
        run_state.rng.up_front.shuffle(pool2)
        run_state.rng.up_front.shuffle(pool3)
        self._choices = {
            "relic_1": pool1[0],
            "relic_2": pool2[0],
            "relic_3": pool3[0],
        }
        return [
            EventOption(option_id, relic_id.replace("_", " ").title(), "Gain a relic")
            for option_id, relic_id in self._choices.items()
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        relic_id = self._choices.get(option_id)
        if relic_id is not None:
            if _should_defer_event_rewards(run_state):
                return _event_result_with_rewards(
                    "Obtained a relic from Vakuu.",
                    [RelicReward(run_state.player.player_id, relic_id=relic_id)],
                )
            run_state.player.obtain_relic(relic_id)
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
            return EventResult(
                finished=True,
                description="Gained a random potion.",
                rewards={"reward_objects": [PotionReward(run_state.player.player_id)]},
            )
        candidates = run_state.player.removable_deck_cards()
        if _should_defer_event_rewards(run_state):
            return _event_result_with_rewards(
                "Removed a card, gained Guilty curse.",
                [
                    RemoveCardReward(run_state.player.player_id, count=1, cards=candidates),
                    AddCardsReward(run_state.player.player_id, [make_guilty()]),
                ],
            )
        return self.request_card_choice(
            prompt="Choose 1 card to remove",
            cards=candidates,
            source_pile="deck",
            resolver=lambda selected: (
                _remove_selected_cards(selected, run_state),
                EventResult(
                    finished=True,
                    description="Removed a card, gained Guilty curse.",
                    rewards={"reward_objects": [AddCardsReward(run_state.player.player_id, [make_guilty()])]},
                ) if _should_defer_event_rewards(run_state) else (
                    run_state.player.add_card_instance_to_deck(make_guilty()),
                    EventResult(finished=True, description="Removed a card, gained Guilty curse."),
                )[-1],
            )[-1],
            allow_skip=False,
            min_count=1,
            max_count=1,
            description="Choose a card to remove.",
        )


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
        snake_enabled = any(can_enchant_card(card, "Slither") for card in run_state.player.deck)
        return [
            EventOption("bird", "Bird", "Transform a Basic card into Peck"),
            EventOption("snake", "Snake", "Enchant a card with Slither", enabled=snake_enabled),
            EventOption("torus", "Torus",
                         "Transform a Basic card into Toric Toughness"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "bird":
            candidates = [card for card in run_state.player.deck if card.rarity == CardRarity.BASIC and card.is_removable]
            if not candidates:
                return EventResult(finished=True, description="Transformed a Basic card into Peck.")
            if _should_defer_event_rewards(run_state):
                return _event_result_with_rewards(
                    "Transformed a Basic card into Peck.",
                    [
                        TransformCardsReward(
                            run_state.player.player_id,
                            count=1,
                            cards=candidates,
                            mapping={card.card_id: CardId.PECK for card in candidates},
                        )
                    ],
                )
            return self.request_card_choice(
                prompt="Choose a Basic card to transform into Peck",
                cards=candidates,
                source_pile="deck",
                resolver=lambda selected: (
                    run_state.player.transform_specific_cards_with_mapping(
                        selected, {card.card_id: CardId.PECK for card in selected}
                    ),
                    EventResult(finished=True, description="Transformed a Basic card into Peck."),
                )[-1],
                allow_skip=False,
                min_count=1,
                max_count=1,
                description="Choose a Basic card to transform.",
            )
        if option_id == "snake":
            candidates = [card for card in run_state.player.deck if can_enchant_card(card, "Slither")]
            if not candidates:
                return EventResult(finished=True, description="Enchanted a card with Slither.")
            if _should_defer_event_rewards(run_state):
                return _event_result_with_rewards(
                    "Enchanted a card with Slither.",
                    [EnchantCardsReward(run_state.player.player_id, enchantment="Slither", amount=1, count=1, cards=candidates)],
                )
            return self.request_card_choice(
                prompt="Choose a card to enchant with Slither",
                cards=candidates,
                source_pile="deck",
                resolver=lambda selected: (
                    selected and selected[0].add_enchantment("Slither", 1),
                    EventResult(finished=True, description="Enchanted a card with Slither."),
                )[-1],
                allow_skip=False,
                min_count=1,
                max_count=1,
                description="Choose a card to enchant.",
            )
        if option_id == "torus":
            candidates = [card for card in run_state.player.deck if card.rarity == CardRarity.BASIC and card.is_removable]
            if not candidates:
                return EventResult(finished=True, description="Transformed a Basic card into Toric Toughness.")
            if _should_defer_event_rewards(run_state):
                return _event_result_with_rewards(
                    "Transformed a Basic card into Toric Toughness.",
                    [
                        TransformCardsReward(
                            run_state.player.player_id,
                            count=1,
                            cards=candidates,
                            mapping={card.card_id: CardId.TORIC_TOUGHNESS for card in candidates},
                        )
                    ],
                )
            return self.request_card_choice(
                prompt="Choose a Basic card to transform into Toric Toughness",
                cards=candidates,
                source_pile="deck",
                resolver=lambda selected: (
                    run_state.player.transform_specific_cards_with_mapping(
                        selected, {card.card_id: CardId.TORIC_TOUGHNESS for card in selected}
                    ),
                    EventResult(finished=True, description="Transformed a Basic card into Toric Toughness."),
                )[-1],
                allow_skip=False,
                min_count=1,
                max_count=1,
                description="Choose a Basic card to transform.",
            )
        return EventResult(finished=True, description="Nothing happened.")


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
        return [
            EventOption("breathing", "Breathing Techniques",
                        "Pay 50g, gain 2 Enlightenment cards"),
            EventOption("emotional", "Emotional Awareness",
                        "Pay 125g, remove 1 card", enabled=gold >= 125),
            EventOption("acupuncture", "Arachnid Acupuncture",
                        "Pay 250g, remove 2 cards", enabled=gold >= 250),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "breathing":
            run_state.player.lose_gold(50)
            if _should_defer_event_rewards(run_state):
                return _event_result_with_rewards(
                    "Paid 50g, gained 2 Enlightenment cards.",
                    [
                        AddCardsReward(
                            run_state.player.player_id,
                            [create_card(CardId.ENLIGHTENMENT), create_card(CardId.ENLIGHTENMENT)],
                        )
                    ],
                )
            run_state.player.add_card_instance_to_deck(create_card(CardId.ENLIGHTENMENT))
            run_state.player.add_card_instance_to_deck(create_card(CardId.ENLIGHTENMENT))
            return EventResult(finished=True,
                               description="Paid 50g, gained 2 Enlightenment cards.")
        if option_id == "emotional":
            if run_state.player.gold < 125:
                return EventResult(finished=True, description="Could not afford Emotional Awareness.")
            candidates = run_state.player.removable_deck_cards()
            if not candidates:
                run_state.player.lose_gold(125)
                return EventResult(finished=True, description="Paid 125g, removed 1 card.")
            if _should_defer_event_rewards(run_state):
                return _event_result_with_rewards(
                    "Paid 125g, removed 1 card.",
                    [
                        RemoveCardReward(run_state.player.player_id, count=1, cards=candidates),
                        LoseGoldReward(run_state.player.player_id, 125),
                    ],
                )
            return self.request_card_choice(
                prompt="Choose 1 card to remove",
                cards=candidates,
                source_pile="deck",
                resolver=lambda selected: (
                    _remove_selected_cards(selected, run_state),
                    run_state.player.lose_gold(125),
                    EventResult(finished=True, description="Paid 125g, removed 1 card."),
                )[-1],
                allow_skip=False,
                min_count=1,
                max_count=1,
                description="Choose a card to remove.",
            )
        if option_id == "acupuncture":
            if run_state.player.gold < 250:
                return EventResult(finished=True, description="Could not afford Arachnid Acupuncture.")
            candidates = run_state.player.removable_deck_cards()
            remove_count = min(2, len(candidates))
            if remove_count == 0:
                run_state.player.lose_gold(250)
                return EventResult(finished=True, description="Paid 250g, removed 2 cards.")
            if _should_defer_event_rewards(run_state):
                return _event_result_with_rewards(
                    "Paid 250g, removed 2 cards.",
                    [
                        RemoveCardReward(run_state.player.player_id, count=remove_count, cards=candidates),
                        LoseGoldReward(run_state.player.player_id, 250),
                    ],
                )
            return self.request_card_choice(
                prompt="Choose 2 cards to remove",
                cards=candidates,
                source_pile="deck",
                resolver=lambda selected: (
                    _remove_selected_cards(selected, run_state),
                    run_state.player.lose_gold(250),
                    EventResult(finished=True, description="Paid 250g, removed 2 cards."),
                )[-1],
                allow_skip=False,
                min_count=remove_count,
                max_count=remove_count,
                description="Choose cards to remove.",
            )
        return EventResult(finished=True, description="Nothing happened.")


register_event(ZenWeaver())
