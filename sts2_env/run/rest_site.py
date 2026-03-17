"""Rest site options.

Implements all rest site choices from MegaCrit.Sts2.Core.Entities.RestSite:
Heal (30% max HP), Smith (upgrade a card), Dig (relic from Shovel),
Lift (Girya), Cook, Clone, Hatch, Mend.
"""

from __future__ import annotations

import math
from dataclasses import dataclass
from typing import TYPE_CHECKING, Any

from sts2_env.relics.registry import coerce_relic_id

if TYPE_CHECKING:
    from sts2_env.run.reward_objects import Reward
    from sts2_env.run.run_state import PlayerState


@dataclass
class RestSiteOption:
    """A single rest site option."""

    option_id: str
    label: str
    enabled: bool = True
    description: str = ""

    def execute(self, player: PlayerState, **kwargs: Any) -> str:
        """Execute this option. Returns a result description."""
        raise NotImplementedError


class HealOption(RestSiteOption):
    """Heal floor(maxHp * 0.3) HP."""

    def __init__(self) -> None:
        super().__init__(option_id="HEAL", label="Rest", description="Heal 30% of max HP")

    def execute(self, player: PlayerState, **kwargs: Any) -> str:
        amount = math.floor(player.max_hp * 0.3)
        run_state = getattr(player, "run_state", None)
        rewards: list[Reward] = []
        if run_state is not None:
            for relic in player.get_relic_objects():
                amount = relic.modify_rest_site_heal_amount(player, amount, run_state)
            for relic in player.get_relic_objects():
                rewards = list(relic.modify_rest_site_heal_rewards(player, rewards, run_state))
        healed = player.heal(amount)
        if run_state is not None:
            run_state.pending_rewards.extend(rewards)
            for relic in player.get_relic_objects():
                relic.after_rest_site_heal(player, healed, run_state)
        return f"Healed {healed} HP"


class SmithOption(RestSiteOption):
    """Upgrade 1 card from deck."""

    def __init__(self, has_upgradable: bool = True) -> None:
        super().__init__(
            option_id="SMITH",
            label="Smith",
            description="Upgrade a card",
            enabled=has_upgradable,
        )

    def execute(self, player: PlayerState, **kwargs: Any) -> str:
        card_index = kwargs.get("card_index", -1)
        if 0 <= card_index < len(player.deck):
            card = player.deck[card_index]
            if not card.upgraded:
                player.upgrade_card_instance(card)
                if card.upgraded:
                    return f"Upgraded {card.card_id.name}"
        candidates = [card for card in player.deck if not card.upgraded]
        if candidates:
            if player.request_deck_choice(
                prompt="Choose a card to upgrade",
                cards=candidates,
                resolver=lambda selected: [player.upgrade_card_instance(card) for card in selected],
                allow_skip=False,
            ):
                return "Choose a card to upgrade"
        return "No card upgraded"


class DigOption(RestSiteOption):
    """Obtain a relic (requires Shovel relic)."""

    def __init__(self) -> None:
        super().__init__(option_id="DIG", label="Dig", description="Obtain a random relic")

    def execute(self, player: PlayerState, **kwargs: Any) -> str:
        player.offer_relic_rewards(1)
        return "Found a relic"


class LiftOption(RestSiteOption):
    """Increment Girya counter. Max 3 lifts -> +1 Strength each."""

    def __init__(self, lifts_done: int = 0) -> None:
        super().__init__(
            option_id="LIFT",
            label="Lift",
            description="Exercise with Girya",
            enabled=lifts_done < 3,
        )
        self.lifts_done = lifts_done

    def execute(self, player: PlayerState, **kwargs: Any) -> str:
        for relic in player.get_relic_objects():
            lift = getattr(relic, "lift", None)
            if callable(lift) and getattr(relic, "relic_id", None).name == "GIRYA":
                if lift():
                    self.lifts_done = getattr(relic, "_times_lifted", self.lifts_done + 1)
                    return f"Lifted! ({self.lifts_done}/3)"
                return "Cannot lift further"
        self.lifts_done += 1
        return f"Lifted! ({self.lifts_done}/3)"


class CookOption(RestSiteOption):
    """Remove 2 cards, gain +9 Max HP."""

    def __init__(self, has_enough_removable: bool = True) -> None:
        super().__init__(
            option_id="COOK",
            label="Cook",
            description="Remove 2 cards, gain 9 Max HP",
            enabled=has_enough_removable,
        )

    def execute(self, player: PlayerState, **kwargs: Any) -> str:
        card_indices = kwargs.get("card_indices", [])
        removed = 0
        for idx in sorted(card_indices, reverse=True):
            if 0 <= idx < len(player.deck) and removed < 2 and player.deck[idx].is_removable:
                player.deck.pop(idx)
                removed += 1
        if removed == 2:
            player.gain_max_hp(9)
            return "Cooked: removed 2 cards, gained 9 Max HP"
        candidates = [card for card in player.deck if card.is_removable and card.rarity.name not in ("STATUS", "CURSE", "QUEST")]
        if len(candidates) >= 2:
            if player.request_deck_choice(
                prompt="Choose 2 cards to cook away",
                cards=candidates,
                resolver=lambda selected: (
                    [player.deck.remove(card) for card in selected if card in player.deck],
                    player.gain_max_hp(9),
                ),
                allow_skip=False,
                min_count=2,
                max_count=2,
            ):
                return "Choose 2 cards to remove"
        return "Cook cancelled"


class CloneOption(RestSiteOption):
    """Duplicate all cards with Clone enchantment (Pael's Growth relic)."""

    def __init__(self) -> None:
        super().__init__(
            option_id="CLONE",
            label="Clone",
            description="Duplicate all Clone-enchanted cards",
        )

    def execute(self, player: PlayerState, **kwargs: Any) -> str:
        cloned = player.clone_enchanted_cards("Clone")
        return f"Cloned {cloned} card(s)"


class HatchOption(RestSiteOption):
    """Obtain the Byrdpip relic (from Byrdoni's Nest event pet)."""

    def __init__(self) -> None:
        super().__init__(
            option_id="HATCH",
            label="Hatch",
            description="Hatch the egg",
        )

    def execute(self, player: PlayerState, **kwargs: Any) -> str:
        player.obtain_relic("BYRDPIP")
        return "Hatched Byrdpip"


class MendOption(RestSiteOption):
    """Heal another player (multiplayer only)."""

    def __init__(self) -> None:
        super().__init__(
            option_id="MEND",
            label="Mend",
            description="Heal another player for 30% of their max HP",
        )

    def execute(self, player: PlayerState, **kwargs: Any) -> str:
        return "Mended ally"


def generate_rest_site_options(
    player: PlayerState,
    relic_ids: list[str] | None = None,
) -> list[RestSiteOption]:
    """Generate available rest site options for a player.

    Default: Heal + Smith (always available, Smith disabled if nothing to upgrade).
    Additional options based on relics.
    """
    if relic_ids is None:
        relic_ids = []

    normalized_relic_ids = set()
    for relic_id in relic_ids:
        try:
            normalized_relic_ids.add(coerce_relic_id(relic_id).name)
        except KeyError:
            normalized_relic_ids.add(str(relic_id).upper())

    options: list[RestSiteOption] = []

    # Heal is always available
    options.append(HealOption())

    # Smith: always available, disabled if no upgradable cards
    has_upgradable = any(not c.upgraded for c in player.deck)
    options.append(SmithOption(has_upgradable=has_upgradable))

    # Relic-based options
    if "SHOVEL" in normalized_relic_ids:
        options.append(DigOption())

    if "GIRYA" in normalized_relic_ids:
        lifts_done = 0
        for relic in player.get_relic_objects():
            if getattr(relic, "relic_id", None).name == "GIRYA":
                lifts_done = getattr(relic, "_times_lifted", 0)
                break
        options.append(LiftOption(lifts_done=lifts_done))

    removable_count = sum(1 for c in player.deck if c.rarity.name not in ("STATUS", "CURSE"))
    if "MEAT_CLEAVER" in normalized_relic_ids and removable_count >= 2:
        options.append(CookOption(has_enough_removable=True))

    if "PAELS_GROWTH" in normalized_relic_ids:
        options.append(CloneOption())

    if any(card.card_id.name == "BYRDONIS_EGG" for card in player.deck):
        options.append(HatchOption())

    run_state = getattr(player, "run_state", None)
    if run_state is not None:
        for relic in player.get_relic_objects():
            options = list(relic.modify_rest_site_options(player, options, run_state))

    return options
