"""RunState: persistent player state across combats.

Matches RunState.cs from MegaCrit.Sts2.Core.Runs.
"""

from __future__ import annotations

from dataclasses import dataclass, field
from typing import Any

from sts2_env.cards.factory import create_card, create_character_cards, eligible_character_cards, eligible_registered_cards
from sts2_env.core.enums import CardId, CardRarity, CardType
from sts2_env.core.rng import Rng
from sts2_env.core.enums import MapPointType, RoomType
from sts2_env.characters.all import get_character
from sts2_env.map.map_point import MapCoord, MapPoint
from sts2_env.map.generator import ActMap, generate_act_map
from sts2_env.map.acts import ActConfig, get_act_config, ALL_ACTS
from sts2_env.potions.base import PotionInstance
from sts2_env.cards.base import CardInstance
from sts2_env.run.odds import UnknownMapPointOdds, CardRarityOdds, PotionRewardOdds


@dataclass
class PlayerState:
    """Persistent state for a single player across a run."""

    player_id: int = 1
    character_id: str = "Ironclad"
    max_hp: int = 80
    current_hp: int = 80
    gold: int = 99
    deck: list[CardInstance] = field(default_factory=list)
    relics: list[str] = field(default_factory=list)
    relic_objects: list[Any] = field(default_factory=list)
    potions: list[PotionInstance | None] = field(default_factory=list)
    max_potion_slots: int = 3
    max_energy: int = 3
    base_orb_slot_count: int = 0
    relic_grab_bag: list[str] = field(default_factory=list)
    unlock_state: dict[str, Any] = field(default_factory=dict)
    discovered_cards: list[str] = field(default_factory=list)
    discovered_relics: list[str] = field(default_factory=list)
    discovered_potions: list[str] = field(default_factory=list)
    discovered_enemies: list[str] = field(default_factory=list)
    discovered_epochs: list[str] = field(default_factory=list)
    card_shop_removals_used: int = 0
    wongo_points: int = 0
    run_state: Any = None

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

    def gain_potion_slots(self, amount: int) -> None:
        if amount > 0:
            self.max_potion_slots += amount

    def _ensure_relic_objects(self) -> list[Any]:
        from sts2_env.relics.registry import create_relic_by_name

        if len(self.relic_objects) == len(self.relics):
            return self.relic_objects
        self.relic_objects = []
        for relic_id in self.relics:
            try:
                self.relic_objects.append(create_relic_by_name(relic_id))
            except KeyError:
                continue
        return self.relic_objects

    def add_card_instance_to_deck(self, card: CardInstance) -> None:
        self.deck.append(card)
        for relic in self._ensure_relic_objects():
            on_card_added = getattr(relic, "on_card_added_to_deck", None)
            if callable(on_card_added):
                on_card_added(self)

    def gain_max_hp(self, amount: int) -> None:
        self.max_hp += amount
        self.current_hp += amount

    def lose_max_hp(self, amount: int) -> None:
        self.max_hp = max(1, self.max_hp - amount)
        self.current_hp = min(self.current_hp, self.max_hp)

    def gain_potion_slots(self, amount: int) -> None:
        self.max_potion_slots = max(0, self.max_potion_slots + amount)

    def lose_all_gold(self) -> int:
        lost = self.gold
        self.gold = 0
        return lost

    def take_damage(self, amount: int) -> int:
        return self.lose_hp(amount)

    def enchant_cards(self, enchantment: str, amount: int, count: int) -> int:
        enchanted = 0
        for card in self.deck:
            if enchanted >= count:
                break
            if card.rarity.name == "QUEST":
                continue
            card.add_enchantment(enchantment, amount)
            enchanted += 1
        return enchanted

    def enchant_all_cards(self, enchantment: str, amount: int = 1) -> int:
        for card in self.deck:
            card.add_enchantment(enchantment, amount)
        return len(self.deck)

    def enchant_basic_strikes(self, enchantment: str, amount: int = 1) -> int:
        enchanted = 0
        for card in self.deck:
            if card.rarity.name == "BASIC" and "STRIKE" in card.card_id.name:
                card.add_enchantment(enchantment, amount)
                enchanted += 1
        return enchanted

    def _coerce_card_id(self, name: str) -> CardId | None:
        if name == "Strike":
            return next((card_id for card_id in get_character(self.character_id).card_pool if "STRIKE" in card_id.name), None)
        if name == "Defend":
            return next((card_id for card_id in get_character(self.character_id).card_pool if "DEFEND" in card_id.name), None)
        candidates = {
            name,
            name.upper(),
            "".join(("_" + ch if ch.isupper() else ch) for ch in name).upper().lstrip("_"),
        }
        for candidate in candidates:
            if candidate in CardId.__members__:
                return CardId[candidate]
        return None

    def add_card_to_deck(self, name: str, upgraded: bool = False) -> bool:
        card_id = self._coerce_card_id(name)
        if card_id is None:
            return False
        try:
            card = create_card(card_id, upgraded=upgraded)
        except KeyError:
            return False
        self.add_card_instance_to_deck(card)
        return True

    def add_random_card_to_deck(self, rarity: str, upgraded: bool = False) -> bool:
        card_rarity = CardRarity[rarity.upper()]
        candidates = eligible_character_cards(self.character_id, rarity=card_rarity, generation_context=None)
        if not candidates:
            return False
        card_id = self.run_state.rng.rewards.choice(candidates)
        self.add_card_instance_to_deck(create_card(card_id, upgraded=upgraded))
        return True

    def add_random_curses(self, count: int) -> int:
        curse_ids = eligible_registered_cards(card_type=CardType.CURSE, generation_context=None)
        added = 0
        for _ in range(max(0, count)):
            if not curse_ids:
                break
            card_id = self.run_state.rng.rewards.choice(curse_ids)
            self.add_card_instance_to_deck(create_card(card_id))
            added += 1
        return added

    def duplicate_card_from_deck(self) -> bool:
        if not self.deck:
            return False
        self.add_card_instance_to_deck(self.deck[0].clone(20_000_000 + len(self.deck)))
        return True

    def upgrade_selected_cards(self, count: int) -> int:
        from sts2_env.cards.factory import create_card

        upgraded = 0
        for card in self.deck:
            if upgraded >= count or card.upgraded:
                continue
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

    def clone_enchanted_cards(self, enchantment: str) -> int:
        clones = []
        for card in self.deck:
            if card.has_enchantment(enchantment):
                clones.append(card.clone(10_000_000 + len(clones)))
        self.deck.extend(clones)
        return len(clones)

    def remove_cards_from_deck(self, count: int) -> int:
        removed = 0
        remaining = []
        for card in self.deck:
            if removed < count and card.rarity.name != "QUEST":
                removed += 1
                continue
            remaining.append(card)
        self.deck = remaining
        return removed

    def transform_cards(self, count: int) -> int:
        candidates = self.deck[:count]
        replacements = create_character_cards(self.character_id, self.run_state.rng.niche, len(candidates), distinct=False, generation_context=None)
        transformed = 0
        for card, replacement in zip(candidates, replacements):
            card.card_id = replacement.card_id
            card.cost = replacement.cost
            card.card_type = replacement.card_type
            card.target_type = replacement.target_type
            card.rarity = replacement.rarity
            card.base_damage = replacement.base_damage
            card.base_block = replacement.base_block
            card.upgraded = replacement.upgraded
            card.keywords = replacement.keywords
            card.tags = replacement.tags
            card.can_be_generated_in_combat = replacement.can_be_generated_in_combat
            card.can_be_generated_by_modifiers = replacement.can_be_generated_by_modifiers
            card.enchantments = dict(replacement.enchantments)
            card.effect_vars = dict(replacement.effect_vars)
            card.original_cost = replacement.original_cost
            transformed += 1
        return transformed

    def transform_basic_cards(self, count: int, upgrade: int = 0) -> int:
        basics = [card for card in self.deck if card.rarity == CardRarity.BASIC][:count]
        transformed = self.transform_cards(len(basics))
        if upgrade:
            self.upgrade_random_cards(None, upgrade)
        return transformed

    def transform_all_basic_cards(self) -> int:
        basics = [card for card in self.deck if card.rarity == CardRarity.BASIC]
        return self.transform_cards(len(basics))

    def transform_starter_card(self) -> bool:
        basics = [card for card in self.deck if card.rarity == CardRarity.BASIC]
        return self.transform_cards(1) > 0 if basics else False

    def transform_cards_to(self, name: str, count: int) -> int:
        card_id = self._coerce_card_id(name)
        if card_id is None:
            return 0
        transformed = 0
        for card in self.deck[:count]:
            replacement = create_card(card_id)
            card.card_id = replacement.card_id
            card.cost = replacement.cost
            card.card_type = replacement.card_type
            card.target_type = replacement.target_type
            card.rarity = replacement.rarity
            card.base_damage = replacement.base_damage
            card.base_block = replacement.base_block
            card.upgraded = replacement.upgraded
            card.keywords = replacement.keywords
            card.tags = replacement.tags
            card.can_be_generated_in_combat = replacement.can_be_generated_in_combat
            card.can_be_generated_by_modifiers = replacement.can_be_generated_by_modifiers
            card.enchantments = dict(replacement.enchantments)
            card.effect_vars = dict(replacement.effect_vars)
            card.original_cost = replacement.original_cost
            transformed += 1
        return transformed

    def transform_and_upgrade_cards(self, count: int) -> int:
        transformed = self.transform_cards(count)
        self.upgrade_random_cards(None, transformed)
        return transformed

    def upgrade_random_cards(self, card_type: CardType | None, count: int) -> int:
        from sts2_env.cards.factory import create_card

        candidates = [card for card in self.deck if not card.upgraded and (card_type is None or card.card_type == card_type)]
        if not candidates:
            return 0
        self.run_state.rng.rewards.shuffle(candidates)
        upgraded = 0
        for card in candidates[:count]:
            try:
                replacement = create_card(card.card_id, upgraded=True)
            except KeyError:
                continue
            if not replacement.upgraded:
                continue
            card.cost = replacement.cost
            card.card_type = replacement.card_type
            card.target_type = replacement.target_type
            card.rarity = replacement.rarity
            card.base_damage = replacement.base_damage
            card.base_block = replacement.base_block
            card.upgraded = replacement.upgraded
            card.keywords = replacement.keywords
            card.tags = replacement.tags
            card.can_be_generated_in_combat = replacement.can_be_generated_in_combat
            card.can_be_generated_by_modifiers = replacement.can_be_generated_by_modifiers
            card.effect_vars = dict(replacement.effect_vars)
            card.original_cost = replacement.original_cost
            upgraded += 1
        return upgraded

    def procure_potion(self, potion_id: str) -> bool:
        from sts2_env.potions.base import create_potion, roll_random_potion_model

        if potion_id == "random":
            model = roll_random_potion_model(self.run_state.rng.rewards, character_id=self.character_id, in_combat=False)
            if model is None:
                return False
            return self.add_potion(create_potion(model.potion_id))
        return self.add_potion(create_potion(potion_id))

    def offer_card_reward(self) -> None:
        from sts2_env.run.reward_objects import CardReward

        self.run_state.pending_rewards.append(CardReward(self.player_id))

    def offer_colorless_cards(self, count: int) -> None:
        from sts2_env.cards.factory import create_cards_from_ids, eligible_registered_cards
        from sts2_env.run.reward_objects import CardReward

        ids = eligible_registered_cards(module_name="sts2_env.cards.colorless", generation_context=None)
        cards = create_cards_from_ids(ids, self.run_state.rng.rewards, count, distinct=True)
        self.run_state.pending_rewards.append(CardReward(self.player_id, cards=cards, option_count=len(cards)))

    def offer_multiplayer_cards(self, count: int) -> None:
        self.offer_colorless_cards(count)

    def offer_relic_rewards(self, count: int) -> None:
        from sts2_env.run.reward_objects import RelicReward

        for _ in range(count):
            self.run_state.pending_rewards.append(RelicReward(self.player_id))

    def offer_potion_reward(self) -> None:
        from sts2_env.run.reward_objects import PotionReward

        self.run_state.pending_rewards.append(PotionReward(self.player_id))

    def offer_potions(self, count: int) -> None:
        for _ in range(count):
            self.offer_potion_reward()

    def offer_card_bundles(self) -> None:
        self.offer_card_reward()
        self.offer_card_reward()

    def obtain_random_relics(self, count: int) -> int:
        obtained = 0
        for _ in range(count):
            from sts2_env.run.reward_objects import RelicReward

            reward = RelicReward(self.player_id)
            reward.populate(self.run_state, None)
            if reward.relic_id is None:
                continue
            self.obtain_relic(reward.relic_id)
            obtained += 1
        return obtained

    def upgrade_starter_relic(self) -> bool:
        mapping = {
            "BURNING_BLOOD": "BLACK_BLOOD",
            "RING_OF_THE_SNAKE": "RING_OF_THE_DRAKE",
            "CRACKED_CORE": "INFUSED_CORE",
            "BOUND_PHYLACTERY": "PHYLACTERY_UNBOUND",
            "DIVINE_RIGHT": "DIVINE_DESTINY",
        }
        for i, relic_id in enumerate(self.relics):
            upgraded = mapping.get(relic_id)
            if upgraded is not None:
                self.relics[i] = upgraded
                return True
        return False

    def obtain_relic(self, relic_id: str) -> bool:
        from sts2_env.relics.registry import create_relic_by_name

        if relic_id in self.relics:
            return False
        self.relics.append(relic_id)
        try:
            relic = create_relic_by_name(relic_id)
        except KeyError:
            return True
        self.relic_objects.append(relic)
        relic.after_obtained(self)
        return True

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
        self.player.run_state = self
        self.player.base_orb_slot_count = get_character(character_id).base_orb_slots
        self.players: list[PlayerState] = [self.player]

        # Act / map state
        self.acts: list[ActConfig] = [get_act_config(i) for i in range(len(ALL_ACTS))]
        self.current_act_index: int = 0
        self.map: ActMap | None = None
        self.visited_map_coords: list[MapCoord] = []
        self.act_floor: int = 0
        self.total_floor: int = 0

        # Event tracking
        self.visited_event_ids: set[str] = set()

        # Primary-player compatibility aliases.
        self.relics = self.player.relics
        self.relic_grab_bag = self.player.relic_grab_bag

        # Odds systems
        self.unknown_odds = UnknownMapPointOdds()
        self.card_rarity_odds = CardRarityOdds(ascension_level)
        self.potion_reward_odds = PotionRewardOdds()
        self.pending_rewards: list[Any] = []

        # Run state flags
        self.is_over: bool = False
        self.player_won: bool = False
        self.has_double_boss: bool = False

    @property
    def current_act(self) -> ActConfig:
        return self.acts[self.current_act_index]

    def add_player(self, player: PlayerState) -> PlayerState:
        if any(existing.player_id == player.player_id for existing in self.players):
            raise ValueError(f"Duplicate player_id {player.player_id}")
        player.run_state = self
        self.players.append(player)
        return player

    def get_player(self, player_id: int) -> PlayerState:
        for player in self.players:
            if player.player_id == player_id:
                return player
        raise KeyError(f"Unknown player_id {player_id}")

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
                can_be_generated_by_modifiers=False,
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
