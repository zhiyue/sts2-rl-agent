"""Card reward generation.

Implements CardFactory logic: rarity odds, pity counter, upgrade probability,
blacklist handling, and GetNextHighestRarity fallback.
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from sts2_env.cards.base import CardInstance
from sts2_env.cards.factory import create_card, eligible_character_cards
from sts2_env.core.enums import CardRarity
from sts2_env.core.rng import Rng

if TYPE_CHECKING:
    from sts2_env.run.run_state import RunState
    from sts2_env.cards.base import CardInstance


def _get_next_highest_rarity(rarity: CardRarity) -> CardRarity:
    """If a rarity pool is exhausted, bump up."""
    if rarity == CardRarity.COMMON:
        return CardRarity.UNCOMMON
    if rarity == CardRarity.UNCOMMON:
        return CardRarity.RARE
    return CardRarity.RARE


def generate_card_reward(
    run_state: RunState,
    context: str = "regular",
    num_cards: int = 3,
) -> list[CardRarity]:
    """Generate card rarities for a reward screen.

    Args:
        run_state: Current run state (contains pity counter).
        context: "regular", "elite", or "boss".
        num_cards: Number of cards to generate (default 3).

    Returns:
        List of CardRarity values for each offered card.
    """
    result: list[CardRarity] = []
    for _ in range(num_cards):
        rarity = run_state.card_rarity_odds.roll(
            run_state.rng.rewards, context=context
        )
        result.append(rarity)
    return result


def roll_for_upgrade(
    run_state: RunState,
    rarity: CardRarity,
    rng: Rng,
    base_chance: float = 0.0,
) -> bool:
    """Roll whether a reward card should be upgraded.

    Formula: odds = base_chance + (act_index * scaling) for non-rare cards.
    Scaling = 0.25 normally, 0.125 at A7+.

    Args:
        run_state: Current run state.
        rarity: The card's rarity.
        rng: RNG stream for the roll.
        base_chance: Base upgrade chance (0.0 for combat, -999999999 for shop).

    Returns:
        True if the card should be upgraded.
    """
    roll = rng.next_float()
    odds = base_chance

    if rarity != CardRarity.RARE:
        scaling = 0.125 if run_state.ascension_level >= 7 else 0.25
        odds += run_state.current_act_index * scaling

    return roll <= odds


def generate_combat_card_rewards(
    run_state: RunState,
    context: str = "regular",
    num_cards: int = 3,
) -> list[tuple[CardRarity, bool]]:
    """Generate card rewards with upgrade rolls.

    Returns list of (rarity, should_upgrade) tuples.
    """
    rarities = generate_card_reward(run_state, context, num_cards)
    result: list[tuple[CardRarity, bool]] = []
    for rarity in rarities:
        upgraded = roll_for_upgrade(
            run_state, rarity, run_state.rng.rewards, base_chance=0.0,
        )
        result.append((rarity, upgraded))
    return result


def _pick_reward_card(
    run_state: RunState,
    rarity: CardRarity,
    chosen_ids: set,
) -> CardInstance | None:
    current_rarity = rarity
    candidate_ids = []

    while True:
        candidate_ids = [
            card_id
            for card_id in eligible_character_cards(
                run_state.player.character_id,
                rarity=current_rarity,
                generation_context=None,
            )
            if card_id not in chosen_ids
        ]
        if candidate_ids or current_rarity == CardRarity.RARE:
            break
        current_rarity = _get_next_highest_rarity(current_rarity)

    if not candidate_ids:
        candidate_ids = eligible_character_cards(
            run_state.player.character_id,
            rarity=current_rarity,
            generation_context=None,
        )
    if not candidate_ids:
        return None

    chosen_id = run_state.rng.rewards.choice(candidate_ids)
    chosen_ids.add(chosen_id)
    upgraded = roll_for_upgrade(
        run_state,
        current_rarity,
        run_state.rng.rewards,
        base_chance=0.0,
    )
    return create_card(chosen_id, upgraded=upgraded)


def generate_combat_reward_cards(
    run_state: RunState,
    context: str = "regular",
    num_cards: int = 3,
) -> list[CardInstance]:
    """Generate concrete card reward options for a post-combat reward screen."""
    rarities = generate_card_reward(run_state, context=context, num_cards=num_cards)
    chosen_ids: set = set()
    cards: list[CardInstance] = []
    for rarity in rarities:
        card = _pick_reward_card(run_state, rarity, chosen_ids)
        if card is not None:
            cards.append(card)
    return cards
