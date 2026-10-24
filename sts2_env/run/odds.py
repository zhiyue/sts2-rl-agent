"""Odds systems: Unknown room, card rarity, potion reward.

Implements the rolling/pity counter mechanics from:
- MegaCrit.Sts2.Core.Odds/UnknownMapPointOdds.cs
- MegaCrit.Sts2.Core.Odds/CardRarityOdds.cs
- MegaCrit.Sts2.Core.Odds/PotionRewardOdds.cs
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from sts2_env.core.enums import RoomType, CardRarity, MapPointType
from sts2_env.core.rng import Rng

if TYPE_CHECKING:
    from sts2_env.run.run_state import RunState


# ── Unknown Map Point Odds ────────────────────────────────────────────

FIRST_RUN_UNKNOWN_EVENT_HISTORY_LIMIT = 2
FIRST_RUN_UNKNOWN_MONSTER_HISTORY_COUNT = 2

C_SHARP_ROOM_TYPE_ORDER = {
    RoomType.MONSTER: 1,
    RoomType.ELITE: 2,
    RoomType.BOSS: 3,
    RoomType.TREASURE: 4,
    RoomType.SHOP: 5,
    RoomType.EVENT: 6,
    RoomType.REST_SITE: 7,
}


class UnknownMapPointOdds:
    """Rolling odds for Unknown ("?") rooms.

    Each non-event result that is NOT rolled gets its odds increased by
    its base value. When it IS rolled, it resets to base.
    """

    BASE_MONSTER = 0.10
    BASE_ELITE = -1.00   # negative = impossible until boosted
    BASE_TREASURE = 0.02
    BASE_SHOP = 0.03

    def __init__(self) -> None:
        self.reset_to_base()

    def reset_to_base(self) -> None:
        self._current: dict[RoomType, float] = {
            RoomType.MONSTER: self.BASE_MONSTER,
            RoomType.ELITE: self.BASE_ELITE,
            RoomType.TREASURE: self.BASE_TREASURE,
            RoomType.SHOP: self.BASE_SHOP,
        }
        self._base: dict[RoomType, float] = {
            RoomType.MONSTER: self.BASE_MONSTER,
            RoomType.ELITE: self.BASE_ELITE,
            RoomType.TREASURE: self.BASE_TREASURE,
            RoomType.SHOP: self.BASE_SHOP,
        }

    def roll(self, rng: Rng, run_state: RunState, blacklist: set[RoomType] | None = None) -> RoomType:
        from sts2_env.run.run_state import FIRST_RUN_COUNT, UNLOCK_STATE_NUMBER_OF_RUNS_KEY

        if blacklist is None:
            blacklist = set()

        if run_state.player.unlock_state.get(UNLOCK_STATE_NUMBER_OF_RUNS_KEY, FIRST_RUN_COUNT) == FIRST_RUN_COUNT:
            unknown_history_count = run_state.count_map_point_history_entries(map_point_type=MapPointType.UNKNOWN)
            if unknown_history_count < FIRST_RUN_UNKNOWN_EVENT_HISTORY_LIMIT:
                return RoomType.EVENT
            if unknown_history_count == FIRST_RUN_UNKNOWN_MONSTER_HISTORY_COUNT:
                return RoomType.MONSTER

        room_types = (set(self._current.keys()) | {RoomType.EVENT}) - blacklist
        for player in run_state.players:
            for card in player.deck:
                room_types = card.modify_unknown_map_point_room_types(run_state, room_types)
        for player in run_state.players:
            for relic in player.get_relic_objects():
                room_types = relic.modify_unknown_map_point_room_types(player, room_types)

        if RoomType.EVENT in room_types:
            result = RoomType.EVENT
        else:
            result = min(room_types, key=C_SHARP_ROOM_TYPE_ORDER.__getitem__)

        roll_val = rng.next_float()
        cumulative = 0.0

        for room_type, odds in self._current.items():
            if room_type not in room_types or odds < 0:
                continue
            cumulative += odds
            if roll_val <= cumulative:
                result = room_type
                break

        # Update odds for next roll
        for room_type, base_odds in self._base.items():
            if room_type == result:
                self._current[room_type] = base_odds  # reset rolled type
            elif room_type in room_types:
                odds_increase = base_odds
                for modifier in run_state.modifiers:
                    odds_increase = modifier.modify_odds_increase_for_unrolled_room_type(room_type, odds_increase)
                self._current[room_type] += odds_increase  # increase un-rolled

        return result


# ── Card Rarity Odds ──────────────────────────────────────────────────

class CardRarityOdds:
    """Card rarity pity counter system.

    Maintains a CurrentValue offset that shifts rare odds over time.
    Boss encounters ignore pity. Shop rolls don't change pity.
    """

    BASE_RARITY_OFFSET = -0.05
    MAX_RARITY_OFFSET = 0.40
    SCARCITY_ASCENSION_LEVEL = 7

    REGULAR_COMMON_ODDS = 0.60
    REGULAR_COMMON_ODDS_SCARCITY = 0.615
    REGULAR_UNCOMMON_ODDS = 0.37
    REGULAR_RARE_ODDS = 0.03
    REGULAR_RARE_ODDS_SCARCITY = 0.0149

    ELITE_COMMON_ODDS = 0.50
    ELITE_COMMON_ODDS_SCARCITY = 0.549
    ELITE_UNCOMMON_ODDS = 0.40
    ELITE_RARE_ODDS = 0.10
    ELITE_RARE_ODDS_SCARCITY = 0.05

    BOSS_COMMON_ODDS = 0.00
    BOSS_UNCOMMON_ODDS = 0.00
    BOSS_RARE_ODDS = 1.00

    SHOP_COMMON_ODDS = 0.54
    SHOP_COMMON_ODDS_SCARCITY = 0.585
    SHOP_UNCOMMON_ODDS = 0.37
    SHOP_RARE_ODDS = 0.09
    SHOP_RARE_ODDS_SCARCITY = 0.045

    UNIFORM_COMMON_ODDS = 0.33
    UNIFORM_UNCOMMON_ODDS = 0.33
    UNIFORM_RARE_ODDS = 0.33

    RARITY_GROWTH = 0.01
    RARITY_GROWTH_SCARCITY = 0.005

    def __init__(self, ascension_level: int = 0) -> None:
        self.ascension_level = ascension_level
        self.current_value: float = self.BASE_RARITY_OFFSET
        scarcity = ascension_level >= self.SCARCITY_ASCENSION_LEVEL

        # Base odds tables
        self.regular_odds = {
            "common": self.REGULAR_COMMON_ODDS_SCARCITY if scarcity else self.REGULAR_COMMON_ODDS,
            "uncommon": self.REGULAR_UNCOMMON_ODDS,
            "rare": self.REGULAR_RARE_ODDS_SCARCITY if scarcity else self.REGULAR_RARE_ODDS,
        }
        self.elite_odds = {
            "common": self.ELITE_COMMON_ODDS_SCARCITY if scarcity else self.ELITE_COMMON_ODDS,
            "uncommon": self.ELITE_UNCOMMON_ODDS,
            "rare": self.ELITE_RARE_ODDS_SCARCITY if scarcity else self.ELITE_RARE_ODDS,
        }
        self.boss_odds = {
            "common": self.BOSS_COMMON_ODDS,
            "uncommon": self.BOSS_UNCOMMON_ODDS,
            "rare": self.BOSS_RARE_ODDS,
        }
        self.shop_odds = {
            "common": self.SHOP_COMMON_ODDS_SCARCITY if scarcity else self.SHOP_COMMON_ODDS,
            "uncommon": self.SHOP_UNCOMMON_ODDS,
            "rare": self.SHOP_RARE_ODDS_SCARCITY if scarcity else self.SHOP_RARE_ODDS,
        }
        self.uniform_odds = {
            "common": self.UNIFORM_COMMON_ODDS,
            "uncommon": self.UNIFORM_UNCOMMON_ODDS,
            "rare": self.UNIFORM_RARE_ODDS,
        }

        self.rarity_growth = self.RARITY_GROWTH_SCARCITY if scarcity else self.RARITY_GROWTH

    def _get_odds(self, context: str) -> dict[str, float]:
        if context == "boss":
            return self.boss_odds
        if context == "elite":
            return self.elite_odds
        if context == "shop":
            return self.shop_odds
        if context == "uniform":
            return self.uniform_odds
        return self.regular_odds

    def roll(self, rng: Rng, context: str = "regular") -> CardRarity:
        """Roll for a card rarity.

        Args:
            rng: RNG stream.
            context: "regular", "elite", "boss", or "shop".

        Returns:
            CardRarity.COMMON, UNCOMMON, or RARE.
        """
        odds = self._get_odds(context)
        offset = 0.0 if context == "boss" else self.current_value
        roll_val = rng.next_float()

        rare_threshold = odds["rare"] + offset

        if roll_val < rare_threshold:
            result = CardRarity.RARE
            self.current_value = self.BASE_RARITY_OFFSET
        elif roll_val < odds["uncommon"] + rare_threshold:
            result = CardRarity.UNCOMMON
            self.current_value = min(self.current_value + self.rarity_growth, self.MAX_RARITY_OFFSET)
        else:
            result = CardRarity.COMMON
            self.current_value = min(self.current_value + self.rarity_growth, self.MAX_RARITY_OFFSET)

        return result

    def roll_without_changing_odds(self, rng: Rng, context: str = "shop") -> CardRarity:
        """Roll rarity for merchant cards -- consumes RNG but no pity change.

        Matches C# RollWithoutChangingFutureOdds: still applies current
        pity offset to the rare threshold.
        """
        odds = self._get_odds(context)
        roll_val = rng.next_float()
        rare_threshold = odds["rare"] + self.current_value
        if roll_val < rare_threshold:
            return CardRarity.RARE
        if roll_val < odds["uncommon"] + rare_threshold:
            return CardRarity.UNCOMMON
        return CardRarity.COMMON

    def roll_with_base_odds(self, rng: Rng, context: str = "regular") -> CardRarity:
        odds = self._get_odds(context)
        roll_val = rng.next_float()
        if roll_val < odds["rare"]:
            return CardRarity.RARE
        if roll_val < odds["uncommon"]:
            return CardRarity.UNCOMMON
        return CardRarity.COMMON


# ── Potion Reward Odds ────────────────────────────────────────────────

class PotionRewardOdds:
    """Potion drop odds oscillating around 40-50%.

    Each combat: if potion drops, reduce future odds by 10%.
    If no drop, increase by 10%. Elite fights get +12.5% bonus.
    """

    INITIAL_VALUE = 0.40
    TARGET = 0.50
    ELITE_BONUS = 0.25
    SWING = 0.10

    def __init__(self) -> None:
        self.current_value: float = self.INITIAL_VALUE

    def roll(self, rng: Rng, is_elite: bool = False, force: bool = False) -> bool:
        """Roll whether a potion drops. Returns True if yes.

        Matches C# PotionRewardOdds.Roll (PotionRewardOdds.cs L62-69): the
        elite bonus (25%, halved) is added to the comparison threshold, and
        that SAME comparison decides the +/-0.10 odds update.
        """
        # C#: num3 = currentValue + (elite ? 0.25f : 0f) * 0.5f
        elite_bonus = self.ELITE_BONUS if is_elite else 0.0
        threshold = self.current_value + elite_bonus * 0.50
        roll_val = rng.next_float()
        # C#: if (flag || num4 < num3) { CurrentValue -= 0.1f; return true; }
        if force or roll_val < threshold:
            self.current_value -= self.SWING
            return True
        self.current_value += self.SWING
        return False
