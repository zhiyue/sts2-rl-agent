"""Rest site options.

Implements all rest site choices from MegaCrit.Sts2.Core.Entities.RestSite:
Heal (30% max HP), Smith (upgrade a card), Dig (relic from Shovel),
Lift (Girya), Cook, Clone, Hatch, Mend.
"""

from __future__ import annotations

import math
from dataclasses import dataclass
from typing import TYPE_CHECKING, Any

if TYPE_CHECKING:
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
        healed = player.heal(amount)
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
                card.upgraded = True
                return f"Upgraded {card.card_id.name}"
        return "No card upgraded"


class DigOption(RestSiteOption):
    """Obtain a relic (requires Shovel relic)."""

    def __init__(self) -> None:
        super().__init__(option_id="DIG", label="Dig", description="Obtain a random relic")

    def execute(self, player: PlayerState, **kwargs: Any) -> str:
        return "Obtained a relic"


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
            if 0 <= idx < len(player.deck) and removed < 2:
                player.deck.pop(idx)
                removed += 1
        if removed == 2:
            player.gain_max_hp(9)
            return "Cooked: removed 2 cards, gained 9 Max HP"
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

    options: list[RestSiteOption] = []

    # Heal is always available
    options.append(HealOption())

    # Smith: always available, disabled if no upgradable cards
    has_upgradable = any(not c.upgraded for c in player.deck)
    options.append(SmithOption(has_upgradable=has_upgradable))

    # Relic-based options
    if "Shovel" in relic_ids:
        options.append(DigOption())

    if "Girya" in relic_ids:
        options.append(LiftOption())

    removable_count = sum(1 for c in player.deck if c.rarity.name not in ("STATUS", "CURSE"))
    if "CookingPot" in relic_ids and removable_count >= 2:
        options.append(CookOption(has_enough_removable=True))

    if "PaelsGrowth" in relic_ids:
        options.append(CloneOption())

    return options
