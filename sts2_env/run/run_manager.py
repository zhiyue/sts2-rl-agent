"""RunManager: central orchestrator for a complete STS2 run.

Provides a step-based API for navigating through an entire run from
start to finish, suitable for RL agents and scripted play-throughs.

The run follows this lifecycle:
  1. Initialize: starter deck, starter relic, first act map
  2. MAP_CHOICE: player selects the next map node
  3. Enter room -> phase depends on room type:
     - Monster/Elite/Boss -> COMBAT (agent plays through CombatState)
     - After combat victory -> CARD_REWARD (pick or skip)
     - After boss victory -> BOSS_RELIC (pick one of three)
     - Shop -> SHOP (buy/sell/leave)
     - Rest Site -> REST_SITE (heal/smith/etc.)
     - Event -> EVENT (choose an option)
     - Treasure -> TREASURE (collect relic)
  4. After room resolution -> back to MAP_CHOICE (or next act / run over)
"""

from __future__ import annotations

import math
from contextlib import contextmanager
from typing import Any, TYPE_CHECKING

from sts2_env.cards.base import CardInstance, reset_instance_counter
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import (
    CardId,
    MapPointType,
    PotionTargetType,
    RoomType,
    TargetType,
)
from sts2_env.core.rng import INT_MAX, Rng
from sts2_env.map.map_point import MapCoord
from sts2_env.core.constants import PERCENT_DENOMINATOR
from sts2_env.potions.all import (
    BLOOD_POTION_HEAL_PERCENT,
    BLOOD_POTION_ID,
    ENTROPIC_BREW_ID,
    FOUL_POTION_GOLD,
    FOUL_POTION_ID,
    FRUIT_JUICE_ID,
    FRUIT_JUICE_MAX_HP,
)
from sts2_env.potions.base import PotionInstance, create_potion, roll_random_potion_model
from sts2_env.encounters.events import get_event_encounter_setup
from sts2_env.run.events import EventModel, EventOption, EventResult, get_event, pick_event
from sts2_env.run.reward_objects import (
    AddCardsReward,
    CardBundlesReward,
    CardReward,
    DuplicateCardReward,
    EnchantCardsReward,
    GoldReward,
    LoseGoldReward,
    LoseHpReward,
    ObtainRelicsReward,
    PotionReward,
    RelicReward,
    RemoveCardReward,
    Reward,
    RecoveredCardReward,
    RewardsSet,
    TransformCardsReward,
    UpgradeCardsReward,
    calculate_gold_proportion,
    CARD_REWARD_ALTERNATIVE_LIMIT_MESSAGE,
    MAX_CARD_REWARD_ALTERNATIVES,
)
from sts2_env.run.rest_site import MendOption, RestSiteOption, generate_rest_site_options
from sts2_env.run.rewards import generate_combat_reward_cards
from sts2_env.run.rooms import CombatRoom, Room, RoomVisitContext, create_room
from sts2_env.run.run_state import RunState
from sts2_env.run.shop import (
    SHOP_ENTRY_SOLD_OUT_PRICE,
    ShopInventory,
    card_removal_cost,
    generate_shop_inventory,
    is_shop_entry_available,
    refill_shop_entry,
)


DEFAULT_CHARACTER_ID = "Ironclad"
NEOW_EVENT_ID = "Neow"

CARD_REWARD_ACTION_PICK_CARD = "pick_card"
CARD_REWARD_ACTION_REROLL = "reroll_card_reward"
CARD_REWARD_ACTION_SKIP = "skip"
CARD_REWARD_ACTION_SACRIFICE = "sacrifice_card_reward"


# ---------------------------------------------------------------------------
# Character configuration
# ---------------------------------------------------------------------------

_CHARACTER_CONFIG: dict[str, dict[str, Any]] = {
    DEFAULT_CHARACTER_ID: {
        "hp": 80,
        "gold": 99,
        "starter_relic": "BurningBlood",
        "heal_after_combat": 6,
    },
    "Silent": {
        "hp": 70,
        "gold": 99,
        "starter_relic": "RingOfTheSnake",
        "heal_after_combat": 0,
    },
    "Defect": {
        "hp": 75,
        "gold": 99,
        "starter_relic": "CrackedCore",
        "heal_after_combat": 0,
    },
    "Necrobinder": {
        "hp": 75,
        "gold": 99,
        "starter_relic": "BoundPhylactery",
        "heal_after_combat": 0,
    },
    "Regent": {
        "hp": 75,
        "gold": 99,
        "starter_relic": "DivineRight",
        "heal_after_combat": 0,
    },
}
SUPPORTED_CHARACTER_IDS = tuple(_CHARACTER_CONFIG)

RUN_MANAGER_RNG_SEED_OFFSET = 9999


def _get_starter_deck(character_id: str) -> list[CardInstance]:
    """Import and call the correct starter deck factory for the character."""
    if character_id == "Ironclad":
        from sts2_env.cards.ironclad import create_ironclad_starter_deck
        return create_ironclad_starter_deck()
    if character_id == "Silent":
        from sts2_env.cards.silent import create_silent_starter_deck
        return create_silent_starter_deck()
    if character_id == "Defect":
        from sts2_env.cards.defect import create_defect_starter_deck
        return create_defect_starter_deck()
    if character_id == "Necrobinder":
        from sts2_env.cards.necrobinder import create_necrobinder_starter_deck
        return create_necrobinder_starter_deck()
    if character_id == "Regent":
        from sts2_env.cards.regent import create_regent_starter_deck
        return create_regent_starter_deck()
    # Fallback to Ironclad
    from sts2_env.cards.ironclad import create_ironclad_starter_deck
    return create_ironclad_starter_deck()


# ---------------------------------------------------------------------------
# Encounter pool accessor per act
# ---------------------------------------------------------------------------

def _get_encounter_pools(act_index: int) -> dict[str, list]:
    """Return {weak, normal, elite, boss} encounter setup lists for an act."""
    from sts2_env.encounters import get_act_encounter_pools

    return get_act_encounter_pools(act_index)


_BOSS_RELIC_POOL = [
    "ASTROLABE",
    "BLACK_STAR",
    "CALLING_BELL",
    "ECTOPLASM",
    "PANDORAS_BOX",
    "PHILOSOPHERS_STONE",
    "RUNIC_PYRAMID",
    "SNECKO_EYE",
    "SOZU",
    "TOUCH_OF_OROBAS",
    "VELVET_CHOKER",
]


# ---------------------------------------------------------------------------
# RunManager
# ---------------------------------------------------------------------------

class RunManager:
    """Central orchestrator for a full STS2 run.

    Exposes a step-based API:
      - ``phase``: current game phase string
      - ``get_available_actions()``: actions valid right now
      - ``take_action(action)``: execute one action, return result dict
      - ``get_combat_state()``: the live CombatState during COMBAT phase
    """

    # Phase constants
    PHASE_MAP_CHOICE = "MAP_CHOICE"
    PHASE_COMBAT = "COMBAT"
    PHASE_CARD_REWARD = "CARD_REWARD"
    PHASE_BOSS_RELIC = "BOSS_RELIC"
    PHASE_SHOP = "SHOP"
    PHASE_REST_SITE = "REST_SITE"
    PHASE_EVENT = "EVENT"
    PHASE_TREASURE = "TREASURE"
    PHASE_RUN_OVER = "RUN_OVER"

    def __init__(
        self,
        seed: int = 0,
        character_id: str = DEFAULT_CHARACTER_ID,
        ascension_level: int = 0,
        start_with_neow: bool = False,
    ):
        self._seed = seed
        self._character_id = character_id
        self._ascension_level = ascension_level

        # Master RNG (for encounter selection, gold rolls, etc.)
        self._rng = Rng(seed + RUN_MANAGER_RNG_SEED_OFFSET)

        # Build RunState
        config = _CHARACTER_CONFIG.get(character_id, _CHARACTER_CONFIG[DEFAULT_CHARACTER_ID])
        self._run_state = RunState(
            seed=seed,
            ascension_level=ascension_level,
            character_id=character_id,
        )
        self._run_state.enable_deck_choice_requests = True
        self._run_state.player.max_hp = config["hp"]
        self._run_state.player.current_hp = config["hp"]
        self._run_state.player.gold = config["gold"]

        # Starter deck and relic
        reset_instance_counter()
        self._run_state.player.deck = _get_starter_deck(character_id)
        self._run_state.player.obtain_relic(config["starter_relic"])
        self._heal_after_combat: int = config["heal_after_combat"]

        # Initialize the run (ascension effects + first map)
        self._run_state.initialize_run()

        # Phase tracking
        self._phase: str = self.PHASE_MAP_CHOICE
        self._combat: CombatState | None = None
        self._current_room: Room | None = None
        self._current_room_type: RoomType | None = None

        # Scratch state for each phase
        self._available_coords: list[MapCoord] = []
        self._offered_cards: list[CardInstance] = []
        self._offered_card_bundles: list[list[CardInstance]] = []
        self._pending_card_reward_screens: list[list[CardInstance]] = []
        self._offered_potion: PotionInstance | None = None
        self._pending_potion_reward: PotionInstance | None = None
        self._offered_relic: str | None = None
        self._pending_relic_reward: str | None = None
        self._current_rewards: RewardsSet | None = None
        self._pending_rewards: list[Reward] = []
        self._current_reward: Reward | None = None
        self._return_phase_after_rewards: str | None = None
        self._resume_after_reward_chain = None
        self._selected_combat_player_id: int | None = None
        self._rest_options: list[RestSiteOption] = []
        self._shop_inventory: ShopInventory | None = None
        self._event_model: EventModel | None = None
        self._event_options: list[EventOption] = []
        self._event_started: bool = False
        self._boss_relics: list[str] = []
        self._resume_after_run_choice = None

        # Kick off the run
        if start_with_neow:
            self._enter_neow()
        else:
            self._enter_map_choice()

    # ------------------------------------------------------------------
    # Public properties
    # ------------------------------------------------------------------

    @property
    def phase(self) -> str:
        """Current game phase."""
        if self._run_state.is_over:
            return self.PHASE_RUN_OVER
        return self._phase

    @property
    def run_state(self) -> RunState:
        return self._run_state

    @property
    def is_over(self) -> bool:
        return self._run_state.is_over

    @property
    def player_won(self) -> bool:
        return self._run_state.player_won

    @property
    def current_room(self) -> Room | None:
        return self._current_room

    # ------------------------------------------------------------------
    # Step-based API
    # ------------------------------------------------------------------

    def get_available_actions(self) -> list[dict]:
        """Return the list of valid actions for the current phase.

        Each action is a ``dict`` with at least an ``"action"`` key describing
        the action type and any parameters (e.g. ``"coord"``, ``"index"``).
        """
        if self._run_state.is_over:
            return []
        if self._phase != self.PHASE_COMBAT and self._run_state.pending_choice is not None:
            return self._actions_run_choice()

        if self._phase == self.PHASE_MAP_CHOICE:
            return self._actions_map_choice()
        if self._phase == self.PHASE_COMBAT:
            return self._actions_combat()
        if self._phase == self.PHASE_CARD_REWARD:
            return self._actions_card_reward()
        if self._phase == self.PHASE_BOSS_RELIC:
            return self._actions_boss_relic()
        if self._phase == self.PHASE_SHOP:
            return self._actions_shop()
        if self._phase == self.PHASE_REST_SITE:
            return self._actions_rest_site()
        if self._phase == self.PHASE_EVENT:
            return self._actions_event()
        if self._phase == self.PHASE_TREASURE:
            return self._actions_treasure()
        return []

    def take_action(self, action: dict) -> dict:
        """Execute *action* and return a result dict describing what happened.

        The result always contains:
          - ``"phase"``: the phase after the action
          - ``"description"``: human-readable summary
        Additional keys depend on the action type.
        """
        if self._run_state.is_over:
            return {"phase": self.PHASE_RUN_OVER, "description": "Run is over."}

        action_type = action.get("action", "")

        if self._phase != self.PHASE_COMBAT and self._run_state.pending_choice is not None:
            if action_type == "choose":
                return self._do_run_choose(action)
            if action_type == "confirm_choice":
                return self._do_run_confirm_choice()

        if self._phase == self.PHASE_MAP_CHOICE and action_type == "move":
            return self._do_map_move(action)
        if self._phase == self.PHASE_COMBAT and action_type == "play_card":
            return self._do_combat_play_card(action)
        if self._phase == self.PHASE_COMBAT and action_type == "select_player":
            return self._do_combat_select_player(action)
        if self._phase == self.PHASE_COMBAT and action_type == "choose":
            return self._do_combat_choose(action)
        if self._phase == self.PHASE_COMBAT and action_type == "confirm_choice":
            return self._do_combat_confirm_choice()
        if self._phase == self.PHASE_COMBAT and action_type == "use_potion":
            return self._do_combat_use_potion(action)
        if self._phase == self.PHASE_COMBAT and action_type == "end_turn":
            return self._do_combat_end_turn()
        if self._phase == self.PHASE_CARD_REWARD and action_type == CARD_REWARD_ACTION_PICK_CARD:
            return self._do_card_reward_pick(action)
        if self._phase == self.PHASE_CARD_REWARD and action_type == "pick_card_bundle":
            return self._do_card_bundle_pick(action)
        if self._phase == self.PHASE_CARD_REWARD and action_type == CARD_REWARD_ACTION_REROLL:
            return self._do_card_reward_reroll()
        if self._phase == self.PHASE_CARD_REWARD and action_type == CARD_REWARD_ACTION_SACRIFICE:
            return self._do_card_reward_sacrifice()
        if self._phase == self.PHASE_CARD_REWARD and action_type == CARD_REWARD_ACTION_SKIP:
            return self._do_card_reward_skip()
        if self._phase == self.PHASE_CARD_REWARD and action_type == "pick_potion":
            return self._do_potion_reward_pick()
        if self._phase == self.PHASE_CARD_REWARD and action_type == "skip_potion":
            return self._do_potion_reward_skip()
        if self._phase == self.PHASE_CARD_REWARD and action_type == "pick_relic_reward":
            return self._do_relic_reward_pick()
        if self._phase == self.PHASE_CARD_REWARD and action_type == "skip_relic":
            return self._do_relic_reward_skip()
        if self._phase == self.PHASE_BOSS_RELIC and action_type == "pick_relic":
            return self._do_boss_relic_pick(action)
        if self._phase != self.PHASE_COMBAT and action_type == "use_potion":
            return self._do_out_of_combat_potion(action)
        if self._phase == self.PHASE_SHOP and action_type in (
            "buy_card", "buy_relic", "buy_potion", "remove_card", "leave_shop",
        ):
            return self._do_shop_action(action)
        if self._phase == self.PHASE_REST_SITE and action_type in ("rest_option",):
            return self._do_rest_site(action)
        if self._phase == self.PHASE_EVENT and action_type == "event_choice":
            return self._do_event_choice(action)
        if self._phase == self.PHASE_EVENT and action_type == "choose":
            return self._do_event_choose(action)
        if self._phase == self.PHASE_EVENT and action_type == "confirm_choice":
            return self._do_event_confirm_choice()
        if self._phase == self.PHASE_TREASURE and action_type == "collect":
            return self._do_treasure_collect()

        return {
            "phase": self.phase,
            "description": f"Invalid action '{action_type}' for phase '{self._phase}'.",
        }

    def _consume_run_pending_rewards(self) -> None:
        pending = list(self._run_state.pending_rewards)
        if not pending:
            return
        self._run_state.pending_rewards.clear()
        rewards_set = RewardsSet(self._run_state.player.player_id).with_custom_rewards(pending)
        generated = rewards_set.generate_without_offering(self._run_state)
        if self._phase == self.PHASE_CARD_REWARD and (self._current_reward is not None or self._pending_rewards):
            self._pending_rewards.extend(generated)
            return
        self._current_rewards = rewards_set
        self._pending_rewards = list(generated)
        self._phase = self.PHASE_CARD_REWARD
        self._advance_post_combat_rewards()

    @contextmanager
    def _deferred_followup_rewards(self):
        previous = self._run_state.defer_followup_rewards
        self._run_state.defer_followup_rewards = True
        try:
            yield
        finally:
            self._run_state.defer_followup_rewards = previous

    def _do_run_choose(self, action: dict) -> dict:
        success = self._run_state.resolve_pending_choice(action.get("index"))
        if not success:
            return {"phase": self.phase, "description": "Failed to resolve pending choice.", "success": False}
        return self._finalize_after_run_choice()

    def _do_run_confirm_choice(self) -> dict:
        success = self._run_state.resolve_pending_choice(None)
        if not success:
            return {"phase": self.phase, "description": "Failed to confirm pending choice.", "success": False}
        return self._finalize_after_run_choice()

    def _finalize_after_run_choice(self) -> dict:
        if self._run_state.pending_choice is not None:
            return {"phase": self.phase, "description": "Pending choice updated.", "finished": False, "success": True}
        callback = self._resume_after_run_choice
        self._resume_after_run_choice = None
        if self._run_state.is_over or self._run_state.player.is_dead:
            if self._run_state.player.is_dead:
                self._run_state.lose_run()
            self._phase = self.PHASE_RUN_OVER
            return {"phase": self.phase, "description": "Resolved pending choice.", "finished": True, "success": True}
        if callback is not None:
            callback()
        return {"phase": self.phase, "description": "Resolved pending choice.", "finished": True, "success": True}

    def get_combat_state(self) -> CombatState | None:
        """Return the active CombatState, or None if not in combat."""
        if self._phase == self.PHASE_COMBAT:
            return self._combat
        return None

    # ------------------------------------------------------------------
    # Phase entry helpers
    # ------------------------------------------------------------------

    def _fire_modifiers_after_room_entered(self, context: RoomVisitContext, combat: CombatState | None = None) -> None:
        for modifier in self._run_state.modifiers:
            modifier.after_room_entered(self._run_state, context, combat=combat)

    def _enter_map_choice(self) -> None:
        """Transition to MAP_CHOICE."""
        self._phase = self.PHASE_MAP_CHOICE
        self._combat = None
        self._current_room = None
        self._current_rewards = None
        self._pending_rewards = []
        self._current_reward = None
        self._offered_cards = []
        self._offered_card_bundles = []
        self._offered_potion = None
        self._pending_potion_reward = None
        self._offered_relic = None
        self._pending_relic_reward = None
        self._return_phase_after_rewards = None
        self._selected_combat_player_id = None
        self._available_coords = self._run_state.get_available_next_coords()

        # If no nodes are reachable check for boss
        if not self._available_coords:
            act_map = self._run_state.map
            if act_map is not None and act_map.boss_point is not None:
                self._available_coords = [act_map.boss_point.coord]

        # Still empty means the act is done (shouldn't normally happen)
        if not self._available_coords:
            self._transition_next_act()

    def _enter_combat(self, room_type: RoomType) -> None:
        """Set up a combat encounter for the given room type."""
        self._phase = self.PHASE_COMBAT
        self._current_room_type = room_type
        self._current_room = create_room(room_type)
        reset_instance_counter()

        player = self._run_state.player
        combat_seed = self._rng.next_int(0, INT_MAX)
        self._combat = CombatState(
            player_hp=player.current_hp,
            player_max_hp=player.max_hp,
            deck=list(player.deck),
            rng_seed=combat_seed,
            relics=list(self._run_state.relics),
            gold=player.gold,
            character_id=self._run_state.player.character_id,
            potions=list(player.potions),
            max_potion_slots=player.max_potion_slots,
            player_state=player,
            room=self._current_room,
            ascension_level=self._run_state.ascension_level,
        )
        self._selected_combat_player_id = player.player_id

        # Select encounter from the act's pre-generated room set
        # (C# RoomSet: NextNormalEncounter / NextEliteEncounter /
        # NextBossEncounter read list[visited % len], then MarkVisited
        # increments the counter when the room is entered).
        room_set = self._run_state.get_act_room_set(self._run_state.current_act_index)
        if room_type == RoomType.BOSS:
            setup_fn = room_set.next_boss_encounter
        elif room_type == RoomType.ELITE:
            setup_fn = room_set.next_elite_encounter
        else:
            setup_fn = room_set.next_normal_encounter
        room_set.mark_visited(room_type)

        if setup_fn is not None:
            encounter_rng = Rng(self._rng.next_int(0, INT_MAX))
            setup_fn(self._combat, encounter_rng)

        self._fire_modifiers_after_room_entered(RoomVisitContext(room_type), combat=self._combat)
        self._combat.start_combat()

    def _enter_event_combat(
        self,
        encounter_id: str,
        reward_objects: list[Reward] | None = None,
        *,
        post_combat_phase: str | None = None,
    ) -> None:
        self._phase = self.PHASE_COMBAT
        self._current_room_type = RoomType.MONSTER
        self._current_room = CombatRoom(
            room_type=RoomType.MONSTER,
            encounter_id=encounter_id,
            suppress_default_rewards=True,
            post_combat_phase=post_combat_phase,
        )
        if reward_objects:
            for reward in reward_objects:
                self._current_room.add_extra_reward(self._run_state.player.player_id, reward)
        reset_instance_counter()

        player = self._run_state.player
        combat_seed = self._rng.next_int(0, INT_MAX)
        self._combat = CombatState(
            player_hp=player.current_hp,
            player_max_hp=player.max_hp,
            deck=list(player.deck),
            rng_seed=combat_seed,
            relics=list(self._run_state.relics),
            gold=player.gold,
            character_id=self._run_state.player.character_id,
            potions=list(player.potions),
            max_potion_slots=player.max_potion_slots,
            player_state=player,
            room=self._current_room,
            ascension_level=self._run_state.ascension_level,
        )
        self._selected_combat_player_id = player.player_id
        setup_fn = get_event_encounter_setup(encounter_id)
        if setup_fn is not None:
            encounter_rng = Rng(self._rng.next_int(0, INT_MAX))
            setup_fn(self._combat, encounter_rng)
        # NOTE: event combats do NOT consume a slot of the act's encounter
        # sequences. C# enters them via EnterRoomWithoutExitingCurrentRoom
        # (room stack count becomes 2), and MarkRoomVisited only fires when
        # CurrentRoomCount == 1 (RunManager.cs L1127); the event's own
        # EncounterModel is used instead of PullNextEncounter (EventModel.cs
        # L625 builds the CombatRoom from the event's encounter).
        self._fire_modifiers_after_room_entered(RoomVisitContext(self._current_room.room_type), combat=self._combat)
        self._combat.start_combat()

    def _prime_next_card_reward(self) -> None:
        reward = self._current_reward
        self._offered_cards = reward.cards if isinstance(reward, CardReward) else []
        self._offered_card_bundles = []
        self._offered_potion = None
        self._offered_relic = None
        if isinstance(reward, CardReward):
            self._card_reward_alternatives(reward)

    def _prime_next_card_bundles_reward(self) -> None:
        reward = self._current_reward
        self._offered_cards = []
        self._offered_card_bundles = reward.bundles if isinstance(reward, CardBundlesReward) else []
        self._offered_potion = None
        self._offered_relic = None

    def _prime_next_potion_reward(self) -> None:
        self._offered_cards = []
        self._offered_card_bundles = []
        reward = self._current_reward
        self._offered_potion = (
            create_potion(reward.potion_id)
            if isinstance(reward, PotionReward) and reward.potion_id is not None
            else None
        )
        self._offered_relic = None

    def _prime_next_relic_reward(self) -> None:
        reward = self._current_reward
        self._offered_cards = []
        self._offered_card_bundles = []
        self._offered_potion = None
        self._offered_relic = reward.relic_id if isinstance(reward, RelicReward) else None

    def _advance_post_combat_rewards(self) -> None:
        self._offered_cards = []
        self._offered_card_bundles = []
        self._offered_potion = None
        self._offered_relic = None
        self._current_reward = None

        while self._pending_rewards:
            reward = self._pending_rewards[0]
            if isinstance(reward, (
                AddCardsReward,
                GoldReward,
                LoseGoldReward,
                LoseHpReward,
                ObtainRelicsReward,
                RemoveCardReward,
                RecoveredCardReward,
                UpgradeCardsReward,
                TransformCardsReward,
                DuplicateCardReward,
                EnchantCardsReward,
            )):
                self._current_reward = self._pending_rewards.pop(0)
                reward.select(self)
                if self._run_state.pending_choice is not None:
                    self._resume_after_run_choice = self._advance_post_combat_rewards
                    return
                if self._run_state.is_over or self._run_state.player.is_dead:
                    self._run_state.lose_run()
                    self._phase = self.PHASE_RUN_OVER
                    self._current_reward = None
                    return
                self._current_reward = None
                continue
            break

        for reward_type in (CardReward, CardBundlesReward, PotionReward, RelicReward):
            for index, reward in enumerate(self._pending_rewards):
                if isinstance(reward, reward_type):
                    self._current_reward = self._pending_rewards.pop(index)
                    self._phase = self.PHASE_CARD_REWARD
                    if isinstance(self._current_reward, CardReward):
                        self._prime_next_card_reward()
                    elif isinstance(self._current_reward, CardBundlesReward):
                        self._prime_next_card_bundles_reward()
                    elif isinstance(self._current_reward, PotionReward):
                        self._prime_next_potion_reward()
                    elif isinstance(self._current_reward, RelicReward):
                        self._prime_next_relic_reward()
                    return
        self._after_card_reward()

    def _enter_card_reward(
        self,
        context: str,
        reward_count: int = 1,
        potion_reward: PotionInstance | None = None,
    ) -> None:
        """Transition to CARD_REWARD after combat."""
        rewards: list[Reward] = [
            CardReward(self._run_state.player.player_id, context=context)
            for _ in range(max(1, reward_count))
        ]
        if potion_reward is not None:
            rewards.append(PotionReward(self._run_state.player.player_id, potion_id=potion_reward.potion_id))
        self._current_rewards = RewardsSet(self._run_state.player.player_id, room=self._current_room)
        self._current_rewards.with_custom_rewards(rewards)
        self._pending_rewards = self._current_rewards.generate_without_offering(self._run_state)
        self._phase = self.PHASE_CARD_REWARD
        self._advance_post_combat_rewards()

    def _enter_boss_relic(self) -> None:
        """Offer three boss relics after defeating a boss."""
        self._phase = self.PHASE_BOSS_RELIC
        owned = set(self._run_state.player.relics)
        candidates = [relic_id for relic_id in _BOSS_RELIC_POOL if relic_id not in owned]
        self._rng.shuffle(candidates)
        self._boss_relics = candidates[:3]

    def _enter_shop(self) -> None:
        self._phase = self.PHASE_SHOP
        self._shop_inventory = generate_shop_inventory(self._run_state)

    def _auto_purchase_shop_inventory(self) -> None:
        inv = self._shop_inventory
        if inv is None:
            return
        player = self._run_state.player
        for entry in list(inv.cards) + list(inv.colorless_cards):
            if not is_shop_entry_available(entry):
                continue
            if entry.card is not None:
                player.add_card_instance_to_deck(player.clone_card_for_deck(entry.card))
            elif entry.card_id:
                player.add_card_to_deck(entry.card_id)
            self._handle_post_shop_purchase("card", entry, gold_spent=0)
        for entry in list(inv.relics):
            if not is_shop_entry_available(entry):
                continue
            if entry.relic_id:
                with self._deferred_followup_rewards():
                    player.obtain_relic(entry.relic_id)
            self._handle_post_shop_purchase("relic", entry, gold_spent=0)
        for entry in list(inv.potions):
            if not is_shop_entry_available(entry):
                continue
            if entry.potion_id and player.procure_potion(entry.potion_id):
                self._handle_post_shop_purchase("potion", entry, gold_spent=0)
        if (
            not inv.removal_used
            and self._run_state.pending_choice is None
            and self._should_allow_merchant_card_removal()
            and player.removable_deck_cards()
        ):
            reward = RemoveCardReward(player.player_id, count=1)
            info = reward.select(self)

            def _finalize_free_removal() -> None:
                self._finalize_shop_card_removal(gold_spent=0)

            if self._run_state.pending_choice is not None:
                self._resume_after_run_choice = _finalize_free_removal
            elif info.get("removed", 0) > 0:
                _finalize_free_removal()

    def _enter_rest_site(self) -> None:
        self._phase = self.PHASE_REST_SITE
        self._rest_options = generate_rest_site_options(self._run_state.player)

    def _enter_event(self) -> None:
        self._phase = self.PHASE_EVENT
        act_cfg = self._run_state.current_act
        event = pick_event(self._run_state, pool=act_cfg.event_ids)
        self._event_model = event
        if event is not None:
            event.reset_rng_for_run(self._run_state)
            event.before_event_started(self._run_state)
            self._event_started = True
            event.ensure_vars_calculated(self._run_state)
            self._event_options = event.generate_initial_options(self._run_state)
        else:
            # No event available -- provide a simple leave option
            self._event_options = [
                EventOption(option_id="leave", label="Leave"),
            ]

    def _enter_neow(self) -> None:
        self._phase = self.PHASE_EVENT
        event = get_event(NEOW_EVENT_ID)
        self._event_model = event
        self._event_started = False
        if event is None:
            self._event_options = [EventOption(option_id="leave", label="Leave")]
            return
        event.reset_rng_for_run(self._run_state)
        event.ensure_vars_calculated(self._run_state)
        self._event_options = event.generate_initial_options(self._run_state)

    def _enter_treasure(self) -> None:
        self._phase = self.PHASE_TREASURE
        self._current_rewards = RewardsSet(self._run_state.player.player_id, room=self._current_room)
        self._pending_rewards = []
        generated = self._current_rewards.with_custom_rewards([
            RelicReward(self._run_state.player.player_id, rng_stream="treasure_room")
        ]).generate_without_offering(self._run_state)
        self._current_reward = generated[0] if generated else None

    def _enter_room(self, room_type: RoomType) -> None:
        """Dispatch to the correct phase based on room type."""
        self._current_room_type = room_type
        context = RoomVisitContext(room_type)
        if room_type == RoomType.SHOP:
            self._enter_shop()
            context = RoomVisitContext(room_type, shop_inventory=self._shop_inventory, run_manager=self)
            for relic in list(self._run_state.player.get_relic_objects()):
                relic.after_room_entered(self._run_state.player, context)
            self._fire_modifiers_after_room_entered(context)
            return

        for relic in self._run_state.player.get_relic_objects():
            relic.after_room_entered(self._run_state.player, context)
        if room_type in (RoomType.MONSTER, RoomType.ELITE, RoomType.BOSS):
            self._enter_combat(room_type)
        elif room_type == RoomType.REST_SITE:
            self._fire_modifiers_after_room_entered(context)
            self._enter_rest_site()
        elif room_type == RoomType.EVENT:
            self._fire_modifiers_after_room_entered(context)
            self._enter_event()
        elif room_type == RoomType.TREASURE:
            self._fire_modifiers_after_room_entered(context)
            should_skip_treasure = any(
                relic.should_generate_treasure(self._run_state.player) is False
                for relic in self._run_state.player.get_relic_objects()
            )
            if should_skip_treasure:
                self._enter_map_choice()
            else:
                self._enter_treasure()
        else:
            # Unknown room type -- just go back to map
            self._enter_map_choice()

    # ------------------------------------------------------------------
    # Action builders
    # ------------------------------------------------------------------

    def _actions_map_choice(self) -> list[dict]:
        actions: list[dict] = []
        act_map = self._run_state.map
        for coord in self._available_coords:
            point = act_map.get_point(coord) if act_map else None
            point_type = point.point_type.name if point else "UNKNOWN"
            actions.append({
                "action": "move",
                "coord": (coord.col, coord.row),
                "point_type": point_type,
            })
        return actions

    def _actions_run_choice(self) -> list[dict]:
        choice = self._run_state.pending_choice
        if choice is None:
            return []
        actions: list[dict] = []
        if choice.can_confirm():
            actions.append({
                "action": "confirm_choice",
                "prompt": choice.prompt,
                "selected_count": len(choice.selected_indices),
            })
        for i, option in enumerate(choice.options):
            actions.append({
                "action": "choose",
                "index": i,
                "card_id": option.card.card_id.name,
                "source_pile": option.source_pile,
                "selected": i in choice.selected_indices,
            })
        return actions

    def _actions_combat(self) -> list[dict]:
        actions: list[dict] = []
        combat = self._combat
        if combat is None or combat.is_over:
            return actions

        if combat.pending_choice is not None:
            if combat.pending_choice.can_confirm():
                actions.append({
                    "action": "confirm_choice",
                    "prompt": combat.pending_choice.prompt,
                    "selected_count": len(combat.pending_choice.selected_indices),
                })
            for i, option in enumerate(combat.pending_choice.options):
                actions.append({
                    "action": "choose",
                    "index": i,
                    "card_id": option.card.card_id.name,
                    "cost": option.card.cost,
                    "source_pile": option.source_pile,
                    "prompt": combat.pending_choice.prompt,
                    "selected": i in combat.pending_choice.selected_indices,
                })
            return actions

        actions.append({"action": "end_turn"})

        controllable_states = [
            state for state in combat.combat_player_states
            if state.creature.is_alive and getattr(state.creature, "is_player", False)
        ]
        if not controllable_states:
            return actions
        selected_state = next(
            (
                state for state in controllable_states
                if state.player_state.player_id == self._selected_combat_player_id
            ),
            controllable_states[0],
        )
        self._selected_combat_player_id = selected_state.player_state.player_id

        if len(controllable_states) > 1:
            for idx, state in enumerate(controllable_states):
                actions.append({
                    "action": "select_player",
                    "player_id": state.player_state.player_id,
                    "player_index": idx,
                    "character_id": state.player_state.character_id,
                    "selected": state is selected_state,
                })

        for i, potion in enumerate(selected_state.potions):
            if potion is None:
                continue
            potion_action: dict[str, Any] = {
                "action": "use_potion",
                "player_id": selected_state.player_state.player_id,
                "slot_index": i,
                "potion_id": potion.potion_id,
            }
            if potion.target_type == PotionTargetType.ANY_ENEMY:
                for j, creature in enumerate(combat.enemies):
                    if creature.is_alive and combat.can_use_potion(i, target_index=j, owner=selected_state.creature):
                        targeted = dict(potion_action)
                        targeted["target_index"] = j
                        targeted["target_name"] = getattr(creature, "monster_id", f"Target_{j}")
                        actions.append(targeted)
            else:
                if combat.can_use_potion(i, owner=selected_state.creature):
                    actions.append(potion_action)

        # Playable cards in hand
        for i, card in enumerate(selected_state.hand):
            if combat.can_play_card(card):
                card_action: dict[str, Any] = {
                    "action": "play_card",
                    "player_id": selected_state.player_state.player_id,
                    "hand_index": i,
                    "card_id": card.card_id.name,
                    "cost": card.cost,
                }
                target_type = card.target_type_for(selected_state.creature)
                if target_type in {TargetType.ANY_ENEMY, TargetType.ANY_ALLY}:
                    targets = (
                        combat.enemies
                        if target_type == TargetType.ANY_ENEMY
                        else combat.get_player_allies_of(selected_state.creature)
                    )
                    for j, creature in enumerate(targets):
                        if creature.is_alive:
                            targeted = dict(card_action)
                            targeted["target_index"] = j
                            targeted["target_name"] = getattr(creature, "monster_id", f"Target_{j}")
                            actions.append(targeted)
                else:
                    # Self-targeted / all-enemies / none
                    card_action["target_index"] = None
                    actions.append(card_action)
        return actions

    def _actions_card_reward(self) -> list[dict]:
        if self._offered_potion is not None:
            return [
                {"action": "skip_potion", "potion_id": self._offered_potion.potion_id},
                {"action": "pick_potion", "potion_id": self._offered_potion.potion_id},
            ]
        if self._offered_relic is not None:
            actions = []
            if self._current_reward is None or self._current_reward.skippable:
                actions.append({"action": "skip_relic", "relic_id": self._offered_relic})
            actions.append({"action": "pick_relic_reward", "relic_id": self._offered_relic})
            return actions

        reward = self._current_reward
        if isinstance(reward, CardReward):
            actions: list[dict] = self._card_reward_alternatives(reward)
        else:
            actions = [{"action": CARD_REWARD_ACTION_SKIP}] if reward is None or reward.skippable else []
        for i, bundle in enumerate(self._offered_card_bundles):
            actions.append({
                "action": "pick_card_bundle",
                "index": i,
                "card_ids": [card.card_id.name for card in bundle],
                "rarities": [card.rarity.name for card in bundle],
                "upgraded": [card.upgraded for card in bundle],
            })
        for i, card in enumerate(self._offered_cards):
            actions.append({
                "action": CARD_REWARD_ACTION_PICK_CARD,
                "index": i,
                "card_id": card.card_id.name,
                "rarity": card.rarity.name,
                "upgraded": card.upgraded,
                "enchantments": dict(card.enchantments),
            })
        return actions

    def _card_reward_alternatives(self, reward: CardReward) -> list[dict]:
        actions: list[dict] = []
        if reward.skippable:
            actions.append({"action": CARD_REWARD_ACTION_SKIP})
        if reward.rerolls_remaining > 0:
            actions.append({
                "action": CARD_REWARD_ACTION_REROLL,
                "rerolls_remaining": reward.rerolls_remaining,
            })
        player = self._run_state.get_player(reward.player_id)
        if any(callable(getattr(relic, "sacrifice_card_reward", None)) for relic in player.get_relic_objects()):
            actions.append({"action": CARD_REWARD_ACTION_SACRIFICE})
        if len(actions) > MAX_CARD_REWARD_ALTERNATIVES:
            raise ValueError(CARD_REWARD_ALTERNATIVE_LIMIT_MESSAGE)
        return actions

    def _actions_boss_relic(self) -> list[dict]:
        return [
            {"action": "pick_relic", "index": i, "relic_id": rid}
            for i, rid in enumerate(self._boss_relics)
        ]

    def _actions_shop(self) -> list[dict]:
        actions: list[dict] = [{"action": "leave_shop"}]
        inv = self._shop_inventory
        if inv is None:
            return actions

        gold = self._run_state.player.gold

        for i, entry in enumerate(inv.cards):
            if gold >= entry.price:
                actions.append({
                    "action": "buy_card",
                    "index": i,
                    "price": entry.price,
                    "card_id": entry.card.card_id.name if entry.card is not None else entry.card_id,
                    "rarity": entry.rarity.name,
                    "card_type": entry.card_type,
                    "upgraded": entry.card.upgraded if entry.card is not None else False,
                    "enchantments": dict(entry.card.enchantments) if entry.card is not None else {},
                    "on_sale": entry.on_sale,
                })

        for i, entry in enumerate(inv.colorless_cards):
            if gold >= entry.price:
                actions.append({
                    "action": "buy_card",
                    "index": len(inv.cards) + i,
                    "price": entry.price,
                    "card_id": entry.card.card_id.name if entry.card is not None else entry.card_id,
                    "rarity": entry.rarity.name,
                    "card_type": "Colorless",
                    "upgraded": entry.card.upgraded if entry.card is not None else False,
                    "enchantments": dict(entry.card.enchantments) if entry.card is not None else {},
                    "on_sale": entry.on_sale,
                })

        for i, entry in enumerate(inv.relics):
            if gold >= entry.price:
                actions.append({
                    "action": "buy_relic",
                    "index": i,
                    "price": entry.price,
                    "relic_id": entry.relic_id,
                    "rarity": entry.relic_rarity.name,
                })

        for i, entry in enumerate(inv.potions):
            if gold >= entry.price:
                actions.append({
                    "action": "buy_potion",
                    "index": i,
                    "price": entry.price,
                    "potion_id": entry.potion_id,
                    "rarity": entry.potion_rarity.name,
                })

        for potion in self._run_state.player.held_potions():
            if potion.potion_id == FOUL_POTION_ID:
                actions.append({
                    "action": "use_potion",
                    "slot_index": potion.slot_index,
                    "potion_id": potion.potion_id,
                })

        if (
            not inv.removal_used
            and gold >= inv.removal_cost
            and self._should_allow_merchant_card_removal()
            and self._run_state.player.removable_deck_cards()
        ):
            actions.append({
                "action": "remove_card",
                "price": inv.removal_cost,
            })

        return actions

    def _actions_rest_site(self) -> list[dict]:
        actions: list[dict] = []
        for opt in self._rest_options:
            if not opt.enabled:
                continue
            if opt.option_id == MendOption.OPTION_ID:
                for target in self._run_state.players:
                    if target is self._run_state.player:
                        continue
                    actions.append({
                        "action": "rest_option",
                        "option_id": opt.option_id,
                        "target_player_id": target.player_id,
                        "label": opt.label,
                        "enabled": opt.enabled,
                        "description": opt.description,
                    })
                continue
            actions.append({
                "action": "rest_option",
                "option_id": opt.option_id,
                "label": opt.label,
                "enabled": opt.enabled,
                "description": opt.description,
            })
        return actions

    def _actions_event(self) -> list[dict]:
        if self._event_model is not None and self._event_model.pending_choice is not None:
            choice = self._event_model.pending_choice
            actions: list[dict] = []
            if choice.can_confirm():
                actions.append({
                    "action": "confirm_choice",
                    "prompt": choice.prompt,
                    "selected_count": len(choice.selected_indices),
                })
            for i, option in enumerate(choice.options):
                actions.append({
                    "action": "choose",
                    "index": i,
                    "card_id": option.card.card_id.name,
                    "source_pile": option.source_pile,
                    "selected": i in choice.selected_indices,
                })
            return actions
        if not self._event_options:
            return [{"action": "event_choice", "option_id": "leave", "label": "Leave"}]
        return [
            {
                "action": "event_choice",
                "option_id": opt.option_id,
                "label": opt.label,
                "description": opt.description,
                "enabled": opt.enabled,
            }
            for opt in self._event_options
            if opt.enabled
        ]

    def _actions_treasure(self) -> list[dict]:
        action: dict[str, Any] = {"action": "collect"}
        if isinstance(self._current_reward, RelicReward):
            action["relic_id"] = self._current_reward.relic_id
        return [action]

    # ------------------------------------------------------------------
    # Action executors
    # ------------------------------------------------------------------

    def _do_map_move(self, action: dict) -> dict:
        col, row = action["coord"]
        coord = MapCoord(col, row)

        # Validate coord is reachable
        if coord not in self._available_coords:
            return {
                "phase": self.phase,
                "description": f"Coordinate ({col},{row}) is not reachable.",
            }

        # Resolve room type
        act_map = self._run_state.map
        point = act_map.get_point(coord) if act_map else None
        if point is None:
            room_type = RoomType.MONSTER
        else:
            room_type = self._run_state.resolve_room_type(
                point.point_type,
                blacklist=self._run_state.build_room_type_blacklist(point.children),
            )
        if not self._run_state.add_visited_coord(coord, room_type=room_type):
            return {
                "phase": self.phase,
                "description": f"Coordinate ({col},{row}) was already visited.",
            }

        self._enter_room(room_type)

        return {
            "phase": self.phase,
            "description": f"Moved to ({col},{row}), entered {room_type.name} room.",
            "room_type": room_type.name,
            "floor": self._run_state.total_floor,
        }

    def _do_combat_play_card(self, action: dict) -> dict:
        combat = self._combat
        if combat is None or combat.is_over:
            return {"phase": self.phase, "description": "No active combat."}

        hand_index = action.get("hand_index", 0)
        target_index = action.get("target_index")
        player_id = action.get("player_id", self._selected_combat_player_id)
        owner = combat.primary_player
        if player_id is not None:
            for state in combat.combat_player_states:
                if state.player_state.player_id == player_id:
                    owner = state.creature
                    self._selected_combat_player_id = player_id
                    break
        success = combat.play_card_from_creature(owner, hand_index, target_index)

        result: dict[str, Any] = {
            "phase": self.phase,
            "description": "Played card." if success else "Failed to play card.",
            "success": success,
        }

        # Check if combat ended
        if combat.is_over:
            result.update(self._resolve_combat_end())

        return result

    def _do_combat_choose(self, action: dict) -> dict:
        combat = self._combat
        if combat is None or combat.is_over:
            return {"phase": self.phase, "description": "No active combat."}

        success = combat.resolve_pending_choice(action.get("index"))
        result: dict[str, Any] = {
            "phase": self.phase,
            "description": "Resolved combat choice." if success else "Failed to resolve combat choice.",
            "success": success,
        }
        if combat.is_over:
            result.update(self._resolve_combat_end())
        return result

    def _do_combat_confirm_choice(self) -> dict:
        combat = self._combat
        if combat is None or combat.is_over:
            return {"phase": self.phase, "description": "No active combat."}

        success = combat.resolve_pending_choice(None)
        result: dict[str, Any] = {
            "phase": self.phase,
            "description": "Confirmed combat choice." if success else "Failed to confirm combat choice.",
            "success": success,
        }
        if combat.is_over:
            result.update(self._resolve_combat_end())
        return result

    def _do_combat_use_potion(self, action: dict) -> dict:
        combat = self._combat
        if combat is None or combat.is_over:
            return {"phase": self.phase, "description": "No active combat.", "success": False}

        slot_index = action.get("slot_index", -1)
        target_index = action.get("target_index")
        player_id = action.get("player_id", self._selected_combat_player_id)
        owner = combat.primary_player
        if player_id is not None:
            for state in combat.combat_player_states:
                if state.player_state.player_id == player_id:
                    owner = state.creature
                    self._selected_combat_player_id = player_id
                    break
        success = combat.use_potion(slot_index, target_index=target_index, owner=owner)
        result: dict[str, Any] = {
            "phase": self.phase,
            "description": "Used potion." if success else "Failed to use potion.",
            "success": success,
        }
        if combat.is_over:
            result.update(self._resolve_combat_end())
        return result

    def _do_combat_end_turn(self) -> dict:
        combat = self._combat
        if combat is None or combat.is_over:
            return {"phase": self.phase, "description": "No active combat."}

        combat.end_player_turn()

        result: dict[str, Any] = {
            "phase": self.phase,
            "description": "Ended player turn.",
        }

        if combat.is_over:
            result.update(self._resolve_combat_end())

        return result

    def _do_combat_select_player(self, action: dict) -> dict:
        combat = self._combat
        if combat is None or combat.is_over:
            return {"phase": self.phase, "description": "No active combat.", "success": False}

        player_id = action.get("player_id")
        valid = {
            state.player_state.player_id
            for state in combat.combat_player_states
            if state.creature.is_alive and getattr(state.creature, "is_player", False)
        }
        if player_id not in valid:
            return {"phase": self.phase, "description": "Invalid combat player.", "success": False}
        self._selected_combat_player_id = player_id
        return {
            "phase": self.phase,
            "description": f"Selected player {player_id} for combat actions.",
            "success": True,
            "player_id": player_id,
        }

    def _resolve_combat_end(self) -> dict:
        """Handle post-combat bookkeeping and transition to the next phase."""
        combat = self._combat
        assert combat is not None

        player = self._run_state.player
        room_type = self._current_room_type or RoomType.MONSTER

        if not combat.player_won:
            # Player died
            player.current_hp = 0
            self._run_state.lose_run()
            self._phase = self.PHASE_RUN_OVER
            return {
                "phase": self.PHASE_RUN_OVER,
                "description": "Player was defeated.",
                "player_won": False,
            }

        # --- Victory path ---
        # Sync HP from combat back to run state
        player.max_hp = combat.player.max_hp
        player.current_hp = combat.player.current_hp
        player.potions = list(combat.potions)
        player.max_potion_slots = combat.max_potion_slots
        self._apply_deck_cards_after_combat_end()

        # Post-combat heal (BurningBlood-style)
        healed = 0
        if self._heal_after_combat > 0 and player.current_hp > 0:
            healed = player.heal(self._heal_after_combat)

        if self._current_room is None:
            self._current_room = create_room(room_type)

        context = "boss" if room_type == RoomType.BOSS else "elite" if room_type == RoomType.ELITE else "regular"
        if isinstance(self._current_room, CombatRoom):
            existing_extra_cards = sum(
                1
                for reward in self._current_room.extra_rewards.get(player.player_id, [])
                if isinstance(reward, CardReward)
            )
            for _ in range(max(0, combat.extra_card_rewards - existing_extra_cards)):
                self._current_room.add_extra_reward(
                    player.player_id,
                    CardReward(player.player_id, context=context),
                )

        # C# CombatRoom.OnCombatEnded: GoldProportion = Encounter.CalculateGoldProportion(CombatState)
        # (1 - escaped / spawned); Monsters flee -> less gold (EncounterModel.cs L373-376).
        if isinstance(self._current_room, CombatRoom):
            self._current_room.gold_proportion = calculate_gold_proportion(combat)

        if isinstance(self._current_room, CombatRoom) and getattr(self._current_room, "suppress_default_rewards", False):
            self._current_rewards = RewardsSet(player.player_id).empty_for_room(self._current_room)
            for reward in self._current_room.extra_rewards.get(player.player_id, []):
                self._current_rewards.rewards.append(reward)
        else:
            self._current_rewards = RewardsSet(player.player_id).with_rewards_from_room(self._current_room, self._run_state)
        generated_rewards = self._current_rewards.generate_without_offering(self._run_state)
        gold_reward = next((reward for reward in generated_rewards if isinstance(reward, GoldReward)), None)
        potion_reward = next((reward for reward in generated_rewards if isinstance(reward, PotionReward)), None)
        room_ref = self._current_room
        post_combat_phase = (
            room_ref.post_combat_phase
            if isinstance(room_ref, CombatRoom)
            and getattr(room_ref, "suppress_default_rewards", False)
            and not generated_rewards
            and getattr(room_ref, "post_combat_phase", None)
            else None
        )
        self._pending_rewards = list(generated_rewards)
        self._phase = self.PHASE_CARD_REWARD
        self._advance_post_combat_rewards()
        if post_combat_phase is not None:
            self._phase = post_combat_phase
            if post_combat_phase == self.PHASE_RUN_OVER:
                self._run_state.is_over = True
                self._run_state.player_won = True

        info: dict[str, Any] = {
            "player_won": True,
            "gold_earned": gold_reward.amount if gold_reward is not None else 0,
            "healed": healed,
            "potion_dropped": potion_reward is not None,
            "potion_reward": potion_reward.potion_id if potion_reward is not None else None,
        }

        info["phase"] = self.phase
        info["description"] = (
            f"Victory! Gained {info['gold_earned']} gold"
            + (f", healed {healed} HP" if healed else "")
            + "."
        )
        return info

    def _apply_deck_cards_after_combat_end(self) -> None:
        player = self._run_state.player
        remaining_deck: list[CardInstance] = []
        for card in player.deck:
            if card.after_combat_end_in_deck(player):
                remaining_deck.append(card)
        player.deck = remaining_deck

    def _do_card_reward_pick(self, action: dict) -> dict:
        reward = self._current_reward
        if isinstance(reward, CardReward):
            info = reward.select(self, index=action.get("index", 0))
        else:
            info = {"description": "No card reward."}

        if not info.get("pending_more_picks"):
            self._advance_post_combat_rewards()
        info["phase"] = self.phase
        return info

    def _do_card_bundle_pick(self, action: dict) -> dict:
        reward = self._current_reward
        if isinstance(reward, CardBundlesReward):
            info = reward.select(self, index=action.get("index", 0))
        else:
            info = {"description": "No card bundle reward.", "success": False}
        self._advance_post_combat_rewards()
        info["phase"] = self.phase
        return info

    def _do_card_reward_reroll(self) -> dict:
        reward = self._current_reward
        if not isinstance(reward, CardReward):
            return {"phase": self.phase, "description": "No card reward.", "success": False}
        info = reward.reroll(self)
        self._prime_next_card_reward()
        info["phase"] = self.phase
        return info

    def _do_card_reward_sacrifice(self) -> dict:
        reward = self._current_reward
        if not isinstance(reward, CardReward):
            return {"phase": self.phase, "description": "No card reward.", "success": False}
        player = self._run_state.get_player(reward.player_id)
        for relic in player.get_relic_objects():
            sacrifice = getattr(relic, "sacrifice_card_reward", None)
            if callable(sacrifice):
                reward.skip(self)
                relic_id = sacrifice(player)
                self._advance_post_combat_rewards()
                return {
                    "phase": self.phase,
                    "description": "Sacrificed card reward.",
                    "success": True,
                    "obtained_relic_id": relic_id,
                }
        return {"phase": self.phase, "description": "No sacrifice available.", "success": False}

    def _do_card_reward_skip(self) -> dict:
        if self._current_reward is not None:
            self._current_reward.skip(self)
        self._advance_post_combat_rewards()
        return {"phase": self.phase, "description": "Skipped card reward."}

    def _do_potion_reward_pick(self) -> dict:
        reward = self._current_reward
        if not isinstance(reward, PotionReward):
            return {"phase": self.phase, "description": "No potion reward."}
        info = reward.select(self)
        self._offered_potion = None
        self._advance_post_combat_rewards()
        info["phase"] = self.phase
        return info

    def _do_potion_reward_skip(self) -> dict:
        if self._current_reward is not None:
            self._current_reward.skip(self)
        self._offered_potion = None
        self._advance_post_combat_rewards()
        return {"phase": self.phase, "description": "Skipped potion reward."}

    def _do_relic_reward_pick(self) -> dict:
        reward = self._current_reward
        if not isinstance(reward, RelicReward):
            return {"phase": self.phase, "description": "No relic reward."}
        info = reward.select(self)
        self._offered_relic = None
        if self._run_state.pending_choice is not None:
            self._resume_after_run_choice = lambda: (self._consume_run_pending_rewards(), self._advance_post_combat_rewards())
            info["phase"] = self.phase
            return info
        self._consume_run_pending_rewards()
        self._advance_post_combat_rewards()
        info["phase"] = self.phase
        return info

    def _do_relic_reward_skip(self) -> dict:
        info = {"description": "Skipped relic reward.", "success": True}
        if self._current_reward is not None:
            info = self._current_reward.skip(self)
            if info.get("success") is False:
                info["phase"] = self.phase
                return info
        self._offered_relic = None
        self._advance_post_combat_rewards()
        info["phase"] = self.phase
        return info

    def _after_card_reward(self) -> None:
        """Transition after the card reward screen."""
        callback = self._resume_after_reward_chain
        if callback is not None:
            self._resume_after_reward_chain = None
            callback()
            return
        if self._return_phase_after_rewards is not None:
            self._phase = self._return_phase_after_rewards
            self._return_phase_after_rewards = None
            return
        if self._run_state.pending_rewards:
            self._consume_run_pending_rewards()
            return
        if self._current_room_type == RoomType.BOSS:
            self._enter_boss_relic()
        else:
            self._enter_map_choice()

    def _do_boss_relic_pick(self, action: dict) -> dict:
        index = action.get("index", 0)
        relic_id = ""
        if 0 <= index < len(self._boss_relics):
            relic_id = self._boss_relics[index]
            with self._deferred_followup_rewards():
                self._run_state.player.obtain_relic(relic_id)

        description = f"Picked boss relic '{relic_id}'."
        if self._run_state.pending_choice is not None:
            self._resume_after_run_choice = lambda: (
                self._consume_run_pending_rewards(),
                self._transition_next_act() if self.phase != self.PHASE_CARD_REWARD else None,
            )
            return {"phase": self.phase, "description": description, "relic_id": relic_id}
        if self._run_state.pending_rewards:
            self._resume_after_reward_chain = self._transition_next_act
            self._consume_run_pending_rewards()
            return {"phase": self.phase, "description": description, "relic_id": relic_id}
        self._transition_next_act()
        return {"phase": self.phase, "description": description, "relic_id": relic_id}

    def _do_shop_action(self, action: dict) -> dict:
        action_type = action["action"]
        player = self._run_state.player
        inv = self._shop_inventory

        if action_type == "leave_shop":
            self._enter_map_choice()
            return {"phase": self.phase, "description": "Left the shop."}

        if inv is None:
            self._enter_map_choice()
            return {"phase": self.phase, "description": "No shop inventory."}

        if action_type == "buy_card":
            index = action.get("index", 0)
            all_cards = list(inv.cards) + list(inv.colorless_cards)
            if 0 <= index < len(all_cards):
                entry = all_cards[index]
                if player.gold >= entry.price:
                    gold_spent = player.lose_gold(entry.price)
                    if entry.card is not None:
                        player.add_card_instance_to_deck(player.clone_card_for_deck(entry.card))
                    elif entry.card_id:
                        player.add_card_to_deck(entry.card_id)
                    self._handle_post_shop_purchase("card", entry, gold_spent=gold_spent)
                    return {
                        "phase": self.phase,
                        "description": f"Bought card ({entry.rarity.name}).",
                    }
            return {"phase": self.phase, "description": "Cannot afford card."}

        if action_type == "buy_relic":
            index = action.get("index", 0)
            if 0 <= index < len(inv.relics):
                entry = inv.relics[index]
                if player.gold >= entry.price:
                    gold_spent = player.lose_gold(entry.price)
                    if entry.relic_id:
                        with self._deferred_followup_rewards():
                            player.obtain_relic(entry.relic_id)
                    self._handle_post_shop_purchase("relic", entry, gold_spent=gold_spent)
                    if self._run_state.pending_choice is not None:
                        self._resume_after_run_choice = lambda: (
                            self._consume_run_pending_rewards(),
                            setattr(self, "_phase", self.PHASE_SHOP) if self.phase != self.PHASE_CARD_REWARD else None,
                        )
                        return {
                            "phase": self.phase,
                            "description": f"Bought relic ({entry.relic_rarity.name}).",
                        }
                    if self._run_state.pending_rewards:
                        self._resume_after_reward_chain = lambda: setattr(self, "_phase", self.PHASE_SHOP)
                    self._consume_run_pending_rewards()
                    return {
                        "phase": self.phase,
                        "description": f"Bought relic ({entry.relic_rarity.name}).",
                    }
            return {"phase": self.phase, "description": "Cannot afford relic."}

        if action_type == "buy_potion":
            index = action.get("index", 0)
            if 0 <= index < len(inv.potions):
                entry = inv.potions[index]
                if player.gold >= entry.price:
                    gold_spent = player.lose_gold(entry.price)
                    if entry.potion_id:
                        player.procure_potion(entry.potion_id)
                    self._handle_post_shop_purchase("potion", entry, gold_spent=gold_spent)
                    return {
                        "phase": self.phase,
                        "description": f"Bought potion ({entry.potion_rarity.name}).",
                    }
            return {"phase": self.phase, "description": "Cannot afford potion."}

        if action_type == "remove_card":
            removable = player.removable_deck_cards()
            if (
                not inv.removal_used
                and player.gold >= inv.removal_cost
                and self._should_allow_merchant_card_removal()
                and removable
            ):
                gold_spent = player.lose_gold(inv.removal_cost)
                reward = RemoveCardReward(player.player_id, count=1, cards=removable, require_manual_confirmation=True)
                info = reward.select(self)

                def _finalize_removal_purchase() -> None:
                    self._finalize_shop_card_removal(gold_spent=gold_spent)

                if self._run_state.pending_choice is not None:
                    self._resume_after_run_choice = _finalize_removal_purchase
                    return {
                        "phase": self.phase,
                        "description": info["description"],
                    }

                if info.get("removed", 0) > 0:
                    _finalize_removal_purchase()
                    return {
                        "phase": self.phase,
                        "description": info["description"],
                    }
            return {"phase": self.phase, "description": "Cannot afford removal."}

        return {"phase": self.phase, "description": "Unknown shop action."}

    def _do_rest_site(self, action: dict) -> dict:
        option_id = action.get("option_id", "")
        player = self._run_state.player

        chosen = None
        for opt in self._rest_options:
            if opt.option_id == option_id and opt.enabled:
                chosen = opt
                break

        if chosen is None:
            # Fallback: try first enabled
            for opt in self._rest_options:
                if opt.enabled:
                    chosen = opt
                    break

        description = "Rested."
        if chosen is not None:
            result_str = chosen.execute(player, **action)
            description = result_str

        disable_remaining = True
        if chosen is not None:
            chosen.enabled = False
            for relic in player.get_relic_objects():
                decision = relic.should_disable_remaining_rest_site_options(player, chosen, self._run_state)
                if decision is False:
                    disable_remaining = False
        has_remaining_rest_options = any(opt.enabled for opt in self._rest_options)
        if not disable_remaining and has_remaining_rest_options:
            self._return_phase_after_rewards = self.PHASE_REST_SITE

        if self._run_state.pending_choice is not None:
            self._resume_after_run_choice = lambda: (
                self._consume_run_pending_rewards(),
                setattr(self, "_phase", self.PHASE_REST_SITE) if self._return_phase_after_rewards == self.PHASE_REST_SITE and self.phase != self.PHASE_CARD_REWARD else None,
                self._enter_map_choice() if self._return_phase_after_rewards != self.PHASE_REST_SITE and self.phase != self.PHASE_CARD_REWARD else None,
            )
            return {"phase": self.phase, "description": description}

        self._consume_run_pending_rewards()
        if self._phase == self.PHASE_CARD_REWARD:
            return {"phase": self.phase, "description": description}
        if self._return_phase_after_rewards == self.PHASE_REST_SITE:
            self._phase = self.PHASE_REST_SITE
            self._return_phase_after_rewards = None
        else:
            self._enter_map_choice()
        return {"phase": self.phase, "description": description}

    def _do_out_of_combat_potion(self, action: dict) -> dict:
        player = self._run_state.player
        slot_index = action.get("slot_index", -1)
        try:
            slot = int(slot_index)
        except (TypeError, ValueError):
            slot = -1
        if slot < 0 or slot >= len(player.potions):
            return {"phase": self.phase, "description": "Cannot use potion."}
        potion = player.potions[slot]
        if potion is None:
            return {"phase": self.phase, "description": "Cannot use potion."}

        if potion.potion_id == FOUL_POTION_ID:
            if self._phase != self.PHASE_SHOP:
                return {"phase": self.phase, "description": "Cannot use potion."}
            player.remove_potion(slot)
            player.gain_gold(FOUL_POTION_GOLD)
            return {"phase": self.phase, "description": f"Used Foul Potion for {FOUL_POTION_GOLD} gold."}

        if potion.potion_id == ENTROPIC_BREW_ID:
            player.remove_potion(slot)
            filled = player.fill_empty_potion_slots()
            return {"phase": self.phase, "description": f"Filled {filled} potion slot(s)."}

        if potion.potion_id in {BLOOD_POTION_ID, FRUIT_JUICE_ID}:
            target_player_id = action.get("target_player_id")
            if target_player_id is None:
                return {"phase": self.phase, "description": "Cannot use potion."}
            try:
                target = self._run_state.get_player(int(target_player_id))
            except (KeyError, TypeError, ValueError):
                return {"phase": self.phase, "description": "Cannot use potion."}
            player.remove_potion(slot)
            if potion.potion_id == BLOOD_POTION_ID:
                healed = target.heal(target.max_hp * BLOOD_POTION_HEAL_PERCENT // PERCENT_DENOMINATOR)
                return {"phase": self.phase, "description": f"Healed {healed} HP."}
            target.gain_max_hp(FRUIT_JUICE_MAX_HP)
            return {"phase": self.phase, "description": f"Gained {FRUIT_JUICE_MAX_HP} Max HP."}

        return {"phase": self.phase, "description": "Cannot use potion."}

    def _should_allow_merchant_card_removal(self) -> bool:
        player = self._run_state.player
        return all(modifier.should_allow_merchant_card_removal(player) for modifier in self._run_state.modifiers)

    def _finalize_shop_card_removal(self, *, gold_spent: int) -> None:
        inv = self._shop_inventory
        if inv is None or inv.removal_used:
            return
        player = self._run_state.player
        player.card_shop_removals_used += 1
        inv.removal_used = True
        inv.removal_cost = card_removal_cost(
            player.card_shop_removals_used,
            ascension_level=self._run_state.ascension_level,
        )
        self._handle_post_shop_purchase("remove_card", inv, gold_spent=gold_spent)

    def _handle_post_shop_purchase(self, item_kind: str, entry: object, *, gold_spent: int) -> None:
        player = self._run_state.player
        should_refill = False
        for relic in player.get_relic_objects():
            relic.on_item_purchased(
                player,
                item_kind=item_kind,
                item=entry,
                run_state=self._run_state,
                gold_spent=gold_spent,
            )
            if relic.should_refill_merchant_entry(
                player,
                item_kind=item_kind,
                item=entry,
                run_state=self._run_state,
            ) is True:
                should_refill = True

        if should_refill and self._shop_inventory is not None:
            refill_shop_entry(self._shop_inventory, item_kind, entry, self._run_state)
            return

        if hasattr(entry, "price"):
            entry.price = SHOP_ENTRY_SOLD_OUT_PRICE

    def _do_event_choice(self, action: dict) -> dict:
        option_id = action.get("option_id", "leave")
        event = self._event_model

        if event is not None and option_id != "leave":
            with self._deferred_followup_rewards():
                result = event.choose(self._run_state, option_id)
            return self._apply_event_result(event, result)
        if event is not None and self._event_started:
            event.on_event_finished(self._run_state)
            self._event_started = False
        self._enter_map_choice()
        return {"phase": self.phase, "description": "Left the event."}

    def _do_event_choose(self, action: dict) -> dict:
        event = self._event_model
        if event is None or event.pending_choice is None:
            return {"phase": self.phase, "description": "No pending event choice.", "success": False}
        with self._deferred_followup_rewards():
            result = event.resolve_pending_choice(action.get("index"))
        return self._apply_event_result(event, result)

    def _do_event_confirm_choice(self) -> dict:
        event = self._event_model
        if event is None or event.pending_choice is None:
            return {"phase": self.phase, "description": "No pending event choice.", "success": False}
        with self._deferred_followup_rewards():
            result = event.resolve_pending_choice(None)
        return self._apply_event_result(event, result)

    def _apply_event_result(self, event: EventModel, result: EventResult) -> dict:
        if event.pending_choice is not None:
            self._event_options = []
            return {
                "phase": self.phase,
                "description": result.description,
                "finished": False,
            }

        description = result.description
        reward_objects = result.rewards.get("reward_objects", [])
        if result.finished and self._event_started:
            event.on_event_finished(self._run_state)
            self._event_started = False

        if self._run_state.is_over:
            self._phase = self.PHASE_RUN_OVER
            return {"phase": self.PHASE_RUN_OVER, "description": description}
        if self._run_state.player.is_dead:
            self._run_state.lose_run()
            self._phase = self.PHASE_RUN_OVER
            return {"phase": self.PHASE_RUN_OVER, "description": description}

        if not result.finished and result.next_options:
            if reward_objects:
                def _resume_event_options_after_rewards() -> None:
                    if self._run_state.is_over:
                        self._phase = self.PHASE_RUN_OVER
                        return
                    if self._run_state.player.is_dead:
                        self._run_state.lose_run()
                        self._phase = self.PHASE_RUN_OVER
                        return
                    self._event_options = result.next_options
                    self._phase = self.PHASE_EVENT

                self._resume_after_reward_chain = _resume_event_options_after_rewards
                self._current_rewards = RewardsSet(self._run_state.player.player_id).with_custom_rewards(
                    reward_objects,
                    preserve_order=result.preserve_reward_order,
                )
                self._pending_rewards = self._current_rewards.generate_without_offering(self._run_state)
                self._phase = self.PHASE_CARD_REWARD
                self._advance_post_combat_rewards()
                return {
                    "phase": self.phase,
                    "description": description,
                    "finished": False,
                }
            if self._run_state.pending_rewards:
                def _resume_event_options_after_pending_rewards() -> None:
                    if self._run_state.is_over:
                        self._phase = self.PHASE_RUN_OVER
                        return
                    if self._run_state.player.is_dead:
                        self._run_state.lose_run()
                        self._phase = self.PHASE_RUN_OVER
                        return
                    self._event_options = result.next_options
                    self._phase = self.PHASE_EVENT

                self._resume_after_reward_chain = _resume_event_options_after_pending_rewards
                self._consume_run_pending_rewards()
                return {
                    "phase": self.phase,
                    "description": description,
                    "finished": False,
                }
            self._event_options = result.next_options
            return {
                "phase": self.phase,
                "description": result.description,
                "finished": False,
            }

        event_combat_setup = result.event_combat_setup or result.rewards.get("event_combat_setup")
        post_combat_phase = result.post_combat_phase or result.rewards.get("post_combat_phase")
        if self._run_state.pending_choice is not None:
            def _resume_after_event_choice() -> None:
                if self._run_state.is_over:
                    self._phase = self.PHASE_RUN_OVER
                    return
                if self._run_state.player.is_dead:
                    self._run_state.lose_run()
                    self._phase = self.PHASE_RUN_OVER
                    return
                if event_combat_setup:
                    self._event_model = None
                    self._event_options = []
                    self._enter_event_combat(
                        event_combat_setup,
                        list(reward_objects),
                        post_combat_phase=post_combat_phase,
                    )
                    return
                if reward_objects:
                    if not result.finished and result.next_options:
                        def _resume_event_options_after_rewards() -> None:
                            self._event_options = result.next_options
                            self._phase = self.PHASE_EVENT

                        self._resume_after_reward_chain = _resume_event_options_after_rewards
                    self._current_rewards = RewardsSet(self._run_state.player.player_id).with_custom_rewards(
                        reward_objects,
                        preserve_order=result.preserve_reward_order,
                    )
                    self._pending_rewards = self._current_rewards.generate_without_offering(self._run_state)
                    self._phase = self.PHASE_CARD_REWARD
                    self._advance_post_combat_rewards()
                    return
                if self._run_state.pending_rewards:
                    if not result.finished and result.next_options:
                        def _resume_event_options_after_pending_rewards() -> None:
                            self._event_options = result.next_options
                            self._phase = self.PHASE_EVENT

                        self._resume_after_reward_chain = _resume_event_options_after_pending_rewards
                    self._consume_run_pending_rewards()
                    return
                if not result.finished and result.next_options:
                    self._event_options = result.next_options
                    self._phase = self.PHASE_EVENT
                    return
                self._enter_map_choice()

            self._resume_after_run_choice = _resume_after_event_choice
            return {
                "phase": self.phase,
                "description": description,
                "finished": False,
            }
        if event_combat_setup:
            self._event_model = None
            self._event_options = []
            self._enter_event_combat(
                event_combat_setup,
                list(reward_objects),
                post_combat_phase=post_combat_phase,
            )
            return {
                "phase": self.phase,
                "description": description,
                "finished": True,
            }

        if reward_objects:
            self._resume_after_reward_chain = self._enter_map_choice
            self._current_rewards = RewardsSet(self._run_state.player.player_id).with_custom_rewards(
                reward_objects,
                preserve_order=result.preserve_reward_order,
            )
            self._pending_rewards = self._current_rewards.generate_without_offering(self._run_state)
            self._phase = self.PHASE_CARD_REWARD
            self._advance_post_combat_rewards()
            return {
                "phase": self.phase,
                "description": description,
                "finished": True,
            }

        if self._run_state.pending_rewards:
            self._resume_after_reward_chain = self._enter_map_choice
            self._consume_run_pending_rewards()
            return {
                "phase": self.phase,
                "description": description,
                "finished": True,
            }

        self._enter_map_choice()
        return {"phase": self.phase, "description": description}

    def _do_treasure_collect(self) -> dict:
        reward = self._current_reward
        if not isinstance(reward, RelicReward):
            self._enter_map_choice()
            return {
                "phase": self.phase,
                "description": "Collected treasure.",
            }

        info = reward.select(self)
        spoils_gold = self._complete_spoils_map_quest()
        self._current_reward = None
        if self._run_state.pending_choice is not None:
            self._resume_after_run_choice = self._enter_map_choice
            info["phase"] = self.phase
            info["description"] = f"Collected treasure relic: {info.get('relic_id', reward.relic_id)}."
            if spoils_gold:
                info["spoils_gold"] = spoils_gold
            return info
        self._consume_run_pending_rewards()
        if self._phase != self.PHASE_CARD_REWARD:
            self._enter_map_choice()
        info["phase"] = self.phase
        info["description"] = f"Collected treasure relic: {info.get('relic_id', reward.relic_id)}."
        if spoils_gold:
            info["spoils_gold"] = spoils_gold
        return info

    def _complete_spoils_map_quest(self) -> int:
        run_state = self._run_state
        if run_state.map is None or not run_state.visited_map_coords:
            return 0
        point = run_state.map.get_point(run_state.visited_map_coords[-1])
        if point is None:
            return 0
        for quest in list(point.quests):
            on_quest_complete = getattr(quest, "on_quest_complete", None)
            if not callable(on_quest_complete):
                continue
            gold = on_quest_complete(run_state)
            if gold:
                return gold
        return 0

    # ------------------------------------------------------------------
    # Act transitions
    # ------------------------------------------------------------------

    def _transition_next_act(self) -> None:
        """Move to the next act, or win the run if there are no more acts."""
        success = self._run_state.enter_next_act()
        if not success:
            # Run completed (all acts cleared)
            self._phase = self.PHASE_RUN_OVER
        else:
            self._enter_map_choice()

    # ------------------------------------------------------------------
    # Summary / debug
    # ------------------------------------------------------------------

    def summary(self) -> dict:
        """Return a human-readable summary of the current run state."""
        rs = self._run_state
        p = rs.player
        return {
            "phase": self.phase,
            "act": rs.current_act_index,
            "floor": rs.total_floor,
            "hp": f"{p.current_hp}/{p.max_hp}",
            "gold": p.gold,
            "deck_size": len(p.deck),
            "relics": list(rs.relics),
            "is_over": rs.is_over,
            "player_won": rs.player_won,
        }
