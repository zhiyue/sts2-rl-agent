"""RunState: persistent player state across combats.

Matches RunState.cs from MegaCrit.Sts2.Core.Runs.
"""

from __future__ import annotations

from dataclasses import dataclass, field
from typing import Any

from sts2_env.core.rng import Rng
from sts2_env.core.enums import MapPointType, RoomType
from sts2_env.map.map_point import MapCoord, MapPoint
from sts2_env.map.generator import ActMap, generate_act_map
from sts2_env.map.acts import ActConfig, get_act_config, ALL_ACTS
from sts2_env.potions.base import PotionInstance
from sts2_env.cards.base import CardInstance
from sts2_env.run.odds import UnknownMapPointOdds, CardRarityOdds, PotionRewardOdds


@dataclass
class PlayerState:
    """Persistent state for a single player across a run."""

    character_id: str = "Ironclad"
    max_hp: int = 80
    current_hp: int = 80
    gold: int = 99
    deck: list[CardInstance] = field(default_factory=list)
    potions: list[PotionInstance | None] = field(default_factory=list)
    max_potion_slots: int = 3
    card_shop_removals_used: int = 0

    def add_potion(self, potion: PotionInstance) -> bool:
        """Add a potion to first empty slot. Returns False if no room."""
        for i in range(self.max_potion_slots):
            if i >= len(self.potions):
                self.potions.append(None)
            if self.potions[i] is None:
                potion.slot_index = i
                self.potions[i] = potion
                return True
        return False

    def remove_potion(self, slot: int) -> PotionInstance | None:
        if 0 <= slot < len(self.potions):
            p = self.potions[slot]
            self.potions[slot] = None
            return p
        return None

    def held_potions(self) -> list[PotionInstance]:
        return [p for p in self.potions if p is not None]

    def heal(self, amount: int) -> int:
        before = self.current_hp
        self.current_hp = min(self.current_hp + amount, self.max_hp)
        return self.current_hp - before

    def lose_hp(self, amount: int) -> int:
        actual = min(amount, self.current_hp)
        self.current_hp = max(0, self.current_hp - amount)
        return actual

    def gain_gold(self, amount: int) -> None:
        self.gold += amount

    def lose_gold(self, amount: int) -> int:
        actual = min(amount, self.gold)
        self.gold -= actual
        return actual

    def gain_max_hp(self, amount: int) -> None:
        self.max_hp += amount
        self.current_hp += amount

    def lose_max_hp(self, amount: int) -> None:
        self.max_hp = max(1, self.max_hp - amount)
        self.current_hp = min(self.current_hp, self.max_hp)

    @property
    def is_dead(self) -> bool:
        return self.current_hp <= 0


class RunRngSet:
    """Separate seeded RNG streams for determinism."""

    def __init__(self, master_seed: int):
        master = Rng(master_seed)
        self.map_rngs: dict[int, Rng] = {}
        self.up_front = Rng(master.next_int(0, 2**31))
        self.unknown_map_point = Rng(master.next_int(0, 2**31))
        self.treasure_room = Rng(master.next_int(0, 2**31))
        self.niche = Rng(master.next_int(0, 2**31))
        self.combat_potion = Rng(master.next_int(0, 2**31))
        self.rewards = Rng(master.next_int(0, 2**31))
        self.shops = Rng(master.next_int(0, 2**31))
        self._master_seed = master_seed
        self._master = master

    def get_map_rng(self, act_index: int) -> Rng:
        if act_index not in self.map_rngs:
            self.map_rngs[act_index] = Rng(
                self._master_seed + act_index * 1000 + 7
            )
        return self.map_rngs[act_index]


class RunState:
    """Complete run state, persisted across combats and rooms."""

    def __init__(
        self,
        seed: int = 0,
        ascension_level: int = 0,
        character_id: str = "Ironclad",
    ):
        self.seed = seed
        self.ascension_level = ascension_level
        self.rng = RunRngSet(seed)

        # Player
        self.player = PlayerState(character_id=character_id)

        # Act / map state
        self.acts: list[ActConfig] = [get_act_config(i) for i in range(len(ALL_ACTS))]
        self.current_act_index: int = 0
        self.map: ActMap | None = None
        self.visited_map_coords: list[MapCoord] = []
        self.act_floor: int = 0
        self.total_floor: int = 0

        # Event tracking
        self.visited_event_ids: set[str] = set()

        # Relics (list of relic IDs)
        self.relics: list[str] = []
        self.relic_grab_bag: list[str] = []

        # Odds systems
        self.unknown_odds = UnknownMapPointOdds()
        self.card_rarity_odds = CardRarityOdds(ascension_level)
        self.potion_reward_odds = PotionRewardOdds()

        # Run state flags
        self.is_over: bool = False
        self.player_won: bool = False
        self.has_double_boss: bool = False

    @property
    def current_act(self) -> ActConfig:
        return self.acts[self.current_act_index]

    def initialize_run(self) -> None:
        """Set up a new run: apply ascension, generate first map."""
        self._apply_ascension_effects()
        self.generate_map()

    def _apply_ascension_effects(self) -> None:
        """Apply all ascension effects matching AscensionManager.ApplyEffectsTo
        and the runtime checks throughout the C# source.

        Ascension levels (enum order):
          0 = None
          1 = SwarmingElites   — 1.6x elite map points (handled in map gen)
          2 = WearyTraveler    — Neow heals 80% of missing HP instead of 100%
          3 = Poverty          — Starting gold × 0.75
          4 = TightBelt        — -1 max potion slot
          5 = AscendersBane    — Add Ascender's Bane curse to deck
          6 = Gloom            — Map gen effects (handled in generator.py)
          7 = Scarcity         — Card upgrade rate halved (0.125 instead of 0.25)
          8 = ToughEnemies     — Higher monster HP (handled per-monster)
          9 = DeadlyEnemies    — Higher monster damage (handled per-monster)
         10 = DoubleBoss       — Two boss fights per act
        """
        asc = self.ascension_level

        # A1: SwarmingElites — 1.6x elites on map (handled in map generator)
        # A2: WearyTraveler — Neow heal reduction (handled in event)

        # A3: Poverty — gold multiplier 0.75x
        if asc >= 3:
            self.player.gold = round(self.player.gold * 0.75)

        # A4: TightBelt — 1 fewer potion slot
        if asc >= 4:
            self.player.max_potion_slots = max(0, self.player.max_potion_slots - 1)

        # A5: AscendersBane — add curse to starting deck
        if asc >= 5:
            from sts2_env.cards.base import CardInstance
            from sts2_env.core.enums import CardId, CardType, TargetType, CardRarity as CR
            curse = CardInstance(
                card_id=CardId.ASCENDERS_BANE,
                cost=0,
                card_type=CardType.CURSE,
                target_type=TargetType.NONE,
                rarity=CR.CURSE,
                keywords=frozenset({"unplayable", "ethereal"}),
            )
            self.player.deck.append(curse)

        # A7: Scarcity — card upgrade scaling halved (stored for rewards.py)
        # The actual check is: scaling = 0.125 if asc >= 7 else 0.25
        # Already handled in run/rewards.py roll_for_upgrade()

        # A8: ToughEnemies — higher monster HP (handled per-monster via
        #   AscensionHelper.GetValueIfAscension; our monsters use fixed values
        #   for simplicity since we don't have runtime ascension checks yet)

        # A9: DeadlyEnemies — higher monster damage (same as A8)

        # A10: DoubleBoss — two boss fights per act
        self.has_double_boss = asc >= 10

    def generate_map(self) -> None:
        """Generate the map for the current act."""
        act = self.current_act
        map_rng = self.rng.get_map_rng(self.current_act_index)
        self.map = generate_act_map(
            num_rooms=act.num_rooms,
            rng=map_rng,
            ascension_level=self.ascension_level,
            act_index=self.current_act_index,
        )

    def enter_act(self, act_index: int) -> None:
        """Transition to a new act."""
        self.current_act_index = act_index
        self.visited_map_coords.clear()
        self.act_floor = 0
        self.unknown_odds.reset_to_base()
        self.generate_map()

    def enter_next_act(self) -> bool:
        """Move to the next act. Returns False if run is over (won)."""
        if self.current_act_index >= len(self.acts) - 1:
            self.is_over = True
            self.player_won = True
            return False
        self.enter_act(self.current_act_index + 1)
        return True

    def add_visited_coord(self, coord: MapCoord) -> None:
        self.visited_map_coords.append(coord)
        self.act_floor = coord.row + 1
        self.total_floor += 1

    def get_available_next_coords(self) -> list[MapCoord]:
        """Get coordinates the player can move to from current position."""
        if self.map is None:
            return []

        if not self.visited_map_coords:
            # At start: can go to any first-row room
            if self.map.start_point:
                return [c.coord for c in self.map.start_point.children]
            return [p.coord for p in self.map.get_row(1)]

        last_coord = self.visited_map_coords[-1]
        last_point = self.map.get_point(last_coord)
        if last_point is None:
            return []
        return [c.coord for c in last_point.children]

    def resolve_room_type(self, point_type: MapPointType) -> RoomType:
        """Convert a MapPointType to a RoomType, rolling for Unknown rooms."""
        mapping = {
            MapPointType.SHOP: RoomType.SHOP,
            MapPointType.TREASURE: RoomType.TREASURE,
            MapPointType.REST_SITE: RoomType.REST_SITE,
            MapPointType.MONSTER: RoomType.MONSTER,
            MapPointType.ELITE: RoomType.ELITE,
            MapPointType.BOSS: RoomType.BOSS,
            MapPointType.ANCIENT: RoomType.EVENT,
        }
        if point_type == MapPointType.UNKNOWN:
            return self.unknown_odds.roll(self.rng.unknown_map_point, self)
        return mapping.get(point_type, RoomType.MONSTER)

    def win_run(self) -> None:
        self.is_over = True
        self.player_won = True

    def lose_run(self) -> None:
        self.is_over = True
        self.player_won = False
