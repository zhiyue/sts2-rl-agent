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
from typing import Any, TYPE_CHECKING

from sts2_env.cards.base import CardInstance, reset_instance_counter
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import (
    MapPointType,
    RoomType,
)
from sts2_env.core.rng import Rng
from sts2_env.map.map_point import MapCoord
from sts2_env.potions.base import PotionInstance, create_potion, roll_random_potion_model
from sts2_env.relics.base import RelicPool, RelicRarity
from sts2_env.relics.registry import RELIC_REGISTRY, load_all_relics
from sts2_env.run.events import EventModel, EventOption, EventResult, pick_event
from sts2_env.run.reward_objects import CardReward, GoldReward, PotionReward, RelicReward, Reward, RewardsSet
from sts2_env.run.rest_site import RestSiteOption, generate_rest_site_options
from sts2_env.run.rewards import generate_combat_reward_cards
from sts2_env.run.rooms import CombatRoom, Room, RoomVisitContext, create_room
from sts2_env.run.run_state import RunState
from sts2_env.run.shop import ShopInventory, generate_shop_inventory, refill_shop_entry


# ---------------------------------------------------------------------------
# Character configuration
# ---------------------------------------------------------------------------

_CHARACTER_CONFIG: dict[str, dict[str, Any]] = {
    "Ironclad": {
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
    if act_index == 0:
        from sts2_env.encounters.act1 import (
            WEAK_ENCOUNTERS, NORMAL_ENCOUNTERS, ELITE_ENCOUNTERS, BOSS_ENCOUNTERS,
        )
    elif act_index == 1:
        from sts2_env.encounters.act2 import (
            WEAK_ENCOUNTERS, NORMAL_ENCOUNTERS, ELITE_ENCOUNTERS, BOSS_ENCOUNTERS,
        )
    elif act_index == 2:
        from sts2_env.encounters.act3 import (
            WEAK_ENCOUNTERS, NORMAL_ENCOUNTERS, ELITE_ENCOUNTERS, BOSS_ENCOUNTERS,
        )
    else:
        from sts2_env.encounters.act4 import (
            WEAK_ENCOUNTERS, NORMAL_ENCOUNTERS, ELITE_ENCOUNTERS, BOSS_ENCOUNTERS,
        )
    return {
        "weak": list(WEAK_ENCOUNTERS),
        "normal": list(NORMAL_ENCOUNTERS),
        "elite": list(ELITE_ENCOUNTERS),
        "boss": list(BOSS_ENCOUNTERS),
    }


# ---------------------------------------------------------------------------
# Gold reward ranges by room type
# ---------------------------------------------------------------------------

_GOLD_REWARDS: dict[RoomType, tuple[int, int]] = {
    RoomType.MONSTER: (10, 20),
    RoomType.ELITE: (25, 35),
    RoomType.BOSS: (50, 75),
}


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
        character_id: str = "Ironclad",
        ascension_level: int = 0,
    ):
        self._seed = seed
        self._character_id = character_id
        self._ascension_level = ascension_level

        # Master RNG (for encounter selection, gold rolls, etc.)
        self._rng = Rng(seed + 9999)

        # Build RunState
        config = _CHARACTER_CONFIG.get(character_id, _CHARACTER_CONFIG["Ironclad"])
        self._run_state = RunState(
            seed=seed,
            ascension_level=ascension_level,
            character_id=character_id,
        )
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
        self._pending_card_reward_screens: list[list[CardInstance]] = []
        self._offered_potion: PotionInstance | None = None
        self._pending_potion_reward: PotionInstance | None = None
        self._offered_relic: str | None = None
        self._pending_relic_reward: str | None = None
        self._current_rewards: RewardsSet | None = None
        self._pending_rewards: list[Reward] = []
        self._current_reward: Reward | None = None
        self._return_phase_after_rewards: str | None = None
        self._selected_combat_player_id: int | None = None
        self._rest_options: list[RestSiteOption] = []
        self._shop_inventory: ShopInventory | None = None
        self._event_model: EventModel | None = None
        self._event_options: list[EventOption] = []
        self._boss_relics: list[str] = []

        # Kick off the map
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
        if self._phase == self.PHASE_COMBAT and action_type == "end_turn":
            return self._do_combat_end_turn()
        if self._phase == self.PHASE_CARD_REWARD and action_type == "pick_card":
            return self._do_card_reward_pick(action)
        if self._phase == self.PHASE_CARD_REWARD and action_type == "reroll_card_reward":
            return self._do_card_reward_reroll()
        if self._phase == self.PHASE_CARD_REWARD and action_type == "skip":
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

    def get_combat_state(self) -> CombatState | None:
        """Return the active CombatState, or None if not in combat."""
        if self._phase == self.PHASE_COMBAT:
            return self._combat
        return None

    # ------------------------------------------------------------------
    # Phase entry helpers
    # ------------------------------------------------------------------

    def _enter_map_choice(self) -> None:
        """Transition to MAP_CHOICE."""
        self._phase = self.PHASE_MAP_CHOICE
        self._combat = None
        self._current_room = None
        self._current_rewards = None
        self._pending_rewards = []
        self._current_reward = None
        self._offered_cards = []
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
        combat_seed = self._rng.next_int(0, 2**31 - 1)
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
        )
        self._selected_combat_player_id = player.player_id

        # Select encounter from appropriate pool
        pools = _get_encounter_pools(self._run_state.current_act_index)
        if room_type == RoomType.BOSS:
            pool = pools["boss"]
        elif room_type == RoomType.ELITE:
            pool = pools["elite"]
        elif self._run_state.act_floor <= self._run_state.current_act.num_weak_encounters:
            pool = pools["weak"]
        else:
            pool = pools["normal"]

        if pool:
            setup_fn = self._rng.choice(pool)
            encounter_rng = Rng(self._rng.next_int(0, 2**31 - 1))
            setup_fn(self._combat, encounter_rng)

        self._combat.start_combat()

    def _prime_next_card_reward(self) -> None:
        reward = self._current_reward
        self._offered_cards = reward.cards if isinstance(reward, CardReward) else []
        self._offered_potion = None
        self._offered_relic = None

    def _prime_next_potion_reward(self) -> None:
        self._offered_cards = []
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
        self._offered_potion = None
        self._offered_relic = reward.relic_id if isinstance(reward, RelicReward) else None

    def _advance_post_combat_rewards(self) -> None:
        self._offered_cards = []
        self._offered_potion = None
        self._offered_relic = None
        self._current_reward = None

        remaining: list[Reward] = []
        for reward in self._pending_rewards:
            if isinstance(reward, GoldReward):
                reward.select(self)
            else:
                remaining.append(reward)
        self._pending_rewards = remaining

        for reward_type in (CardReward, PotionReward, RelicReward):
            for index, reward in enumerate(self._pending_rewards):
                if isinstance(reward, reward_type):
                    self._current_reward = self._pending_rewards.pop(index)
                    self._phase = self.PHASE_CARD_REWARD
                    if isinstance(self._current_reward, CardReward):
                        self._prime_next_card_reward()
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
        load_all_relics()
        desired_pool = getattr(RelicPool, self._run_state.player.character_id.upper(), None)
        owned = set(self._run_state.player.relics)
        candidates = [
            relic_id.name
            for relic_id, relic_cls in RELIC_REGISTRY.items()
            if relic_cls.rarity is RelicRarity.BOSS
            and relic_id.name not in owned
            and (desired_pool is None or relic_cls.pool in {RelicPool.SHARED, desired_pool})
        ]
        self._rng.shuffle(candidates)
        self._boss_relics = candidates[:3]

    def _enter_shop(self) -> None:
        self._phase = self.PHASE_SHOP
        self._shop_inventory = generate_shop_inventory(self._run_state)

    def _enter_rest_site(self) -> None:
        self._phase = self.PHASE_REST_SITE
        self._rest_options = generate_rest_site_options(
            self._run_state.player,
            relic_ids=self._run_state.relics,
        )

    def _enter_event(self) -> None:
        self._phase = self.PHASE_EVENT
        act_cfg = self._run_state.current_act
        event = pick_event(self._run_state, pool=act_cfg.event_ids)
        self._event_model = event
        if event is not None:
            event.calculate_vars(self._run_state)
            self._event_options = event.generate_initial_options(self._run_state)
        else:
            # No event available -- provide a simple leave option
            self._event_options = [
                EventOption(option_id="leave", label="Leave"),
            ]

    def _enter_treasure(self) -> None:
        self._phase = self.PHASE_TREASURE

    def _enter_room(self, room_type: RoomType) -> None:
        """Dispatch to the correct phase based on room type."""
        self._current_room_type = room_type
        context = RoomVisitContext(room_type)
        for relic in self._run_state.player.get_relic_objects():
            relic.after_room_entered(self._run_state.player, context)
        if room_type in (RoomType.MONSTER, RoomType.ELITE, RoomType.BOSS):
            self._enter_combat(room_type)
        elif room_type == RoomType.SHOP:
            self._enter_shop()
        elif room_type == RoomType.REST_SITE:
            self._enter_rest_site()
        elif room_type == RoomType.EVENT:
            self._enter_event()
        elif room_type == RoomType.TREASURE:
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
                if card.target_type.name in {"ANY_ENEMY", "ANY_ALLY"}:
                    targets = (
                        combat.enemies
                        if card.target_type.name == "ANY_ENEMY"
                        else combat.get_player_allies_of(selected_state.creature)
                    )
                    for j, creature in enumerate(targets):
                        if creature.is_alive:
                            targeted = dict(card_action)
                            targeted["target_index"] = j
                            targeted["target_name"] = getattr(creature, "name", f"Target_{j}")
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
            return [
                {"action": "skip_relic", "relic_id": self._offered_relic},
                {"action": "pick_relic_reward", "relic_id": self._offered_relic},
            ]

        actions: list[dict] = [{"action": "skip"}]
        reward = self._current_reward
        if isinstance(reward, CardReward) and reward.rerolls_remaining > 0:
            actions.append({
                "action": "reroll_card_reward",
                "rerolls_remaining": reward.rerolls_remaining,
            })
        for i, card in enumerate(self._offered_cards):
            actions.append({
                "action": "pick_card",
                "index": i,
                "card_id": card.card_id.name,
                "rarity": card.rarity.name,
                "upgraded": card.upgraded,
                "enchantments": dict(card.enchantments),
            })
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
                    "rarity": entry.relic_rarity.name,
                })

        for i, entry in enumerate(inv.potions):
            if gold >= entry.price:
                actions.append({
                    "action": "buy_potion",
                    "index": i,
                    "price": entry.price,
                    "rarity": entry.potion_rarity.name,
                })

        if gold >= inv.removal_cost and len(self._run_state.player.deck) > 0:
            actions.append({
                "action": "remove_card",
                "price": inv.removal_cost,
            })

        return actions

    def _actions_rest_site(self) -> list[dict]:
        return [
            {
                "action": "rest_option",
                "option_id": opt.option_id,
                "label": opt.label,
                "enabled": opt.enabled,
                "description": opt.description,
            }
            for opt in self._rest_options
            if opt.enabled
        ]

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
                "enabled": opt.enabled,
            }
            for opt in self._event_options
            if opt.enabled
        ]

    def _actions_treasure(self) -> list[dict]:
        return [{"action": "collect"}]

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

        self._run_state.add_visited_coord(coord)

        # Resolve room type
        act_map = self._run_state.map
        point = act_map.get_point(coord) if act_map else None
        if point is None:
            room_type = RoomType.MONSTER
        else:
            room_type = self._run_state.resolve_room_type(point.point_type)

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

        self._current_rewards = RewardsSet(player.player_id).with_rewards_from_room(self._current_room, self._run_state)
        generated_rewards = self._current_rewards.generate_without_offering(self._run_state)
        gold_reward = next((reward for reward in generated_rewards if isinstance(reward, GoldReward)), None)
        potion_reward = next((reward for reward in generated_rewards if isinstance(reward, PotionReward)), None)
        self._pending_rewards = list(generated_rewards)
        self._phase = self.PHASE_CARD_REWARD
        self._advance_post_combat_rewards()

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

    def _do_card_reward_pick(self, action: dict) -> dict:
        reward = self._current_reward
        if isinstance(reward, CardReward):
            info = reward.select(self, index=action.get("index", 0))
        else:
            info = {"description": "No card reward."}

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
        self._consume_run_pending_rewards()
        self._advance_post_combat_rewards()
        info["phase"] = self.phase
        return info

    def _do_relic_reward_skip(self) -> dict:
        if self._current_reward is not None:
            self._current_reward.skip(self)
        self._offered_relic = None
        self._advance_post_combat_rewards()
        return {"phase": self.phase, "description": "Skipped relic reward."}

    def _after_card_reward(self) -> None:
        """Transition after the card reward screen."""
        if self._return_phase_after_rewards is not None:
            self._phase = self._return_phase_after_rewards
            self._return_phase_after_rewards = None
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
            self._run_state.player.obtain_relic(relic_id)

        description = f"Picked boss relic '{relic_id}'."
        if self._run_state.pending_rewards:
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
                    player.lose_gold(entry.price)
                    if entry.card is not None:
                        player.add_card_instance_to_deck(entry.card.clone(30_000_000 + len(player.deck)))
                    elif entry.card_id:
                        player.add_card_to_deck(entry.card_id)
                    self._handle_post_shop_purchase("card", entry)
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
                    player.lose_gold(entry.price)
                    if entry.relic_id:
                        player.obtain_relic(entry.relic_id)
                    self._handle_post_shop_purchase("relic", entry)
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
                    player.lose_gold(entry.price)
                    if entry.potion_id:
                        player.procure_potion(entry.potion_id)
                    self._handle_post_shop_purchase("potion", entry)
                    return {
                        "phase": self.phase,
                        "description": f"Bought potion ({entry.potion_rarity.name}).",
                    }
            return {"phase": self.phase, "description": "Cannot afford potion."}

        if action_type == "remove_card":
            if player.gold >= inv.removal_cost and len(player.deck) > 0:
                player.lose_gold(inv.removal_cost)
                player.card_shop_removals_used += 1
                inv.removal_cost += 25
                for relic in player.get_relic_objects():
                    relic.on_item_purchased(player)
                return {
                    "phase": self.phase,
                    "description": "Card removal purchased.",
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
            if chosen.option_id == "SMITH":
                # Auto-select first upgradable card
                for i, card in enumerate(player.deck):
                    if not card.upgraded:
                        result_str = chosen.execute(player, card_index=i)
                        description = result_str
                        break
                else:
                    description = "No card to upgrade."
            else:
                result_str = chosen.execute(player)
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

        self._consume_run_pending_rewards()
        if self._phase == self.PHASE_CARD_REWARD:
            return {"phase": self.phase, "description": description}
        if self._return_phase_after_rewards == self.PHASE_REST_SITE:
            self._phase = self.PHASE_REST_SITE
            self._return_phase_after_rewards = None
        else:
            self._enter_map_choice()
        return {"phase": self.phase, "description": description}

    def _handle_post_shop_purchase(self, item_kind: str, entry: object) -> None:
        player = self._run_state.player
        should_refill = False
        for relic in player.get_relic_objects():
            relic.on_item_purchased(player)
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
            entry.price = 999999

    def _do_event_choice(self, action: dict) -> dict:
        option_id = action.get("option_id", "leave")
        event = self._event_model

        if event is not None and option_id != "leave":
            result = event.choose(self._run_state, option_id)
            return self._apply_event_result(event, result)
        self._enter_map_choice()
        return {"phase": self.phase, "description": "Left the event."}

    def _do_event_choose(self, action: dict) -> dict:
        event = self._event_model
        if event is None or event.pending_choice is None:
            return {"phase": self.phase, "description": "No pending event choice.", "success": False}
        result = event.resolve_pending_choice(action.get("index"))
        return self._apply_event_result(event, result)

    def _do_event_confirm_choice(self) -> dict:
        event = self._event_model
        if event is None or event.pending_choice is None:
            return {"phase": self.phase, "description": "No pending event choice.", "success": False}
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
        if not result.finished and result.next_options:
            self._event_options = result.next_options
            return {
                "phase": self.phase,
                "description": result.description,
                "finished": False,
            }

        description = result.description

        if self._run_state.is_over:
            self._phase = self.PHASE_RUN_OVER
            return {"phase": self.PHASE_RUN_OVER, "description": description}
        if self._run_state.player.is_dead:
            self._run_state.lose_run()
            self._phase = self.PHASE_RUN_OVER
            return {"phase": self.PHASE_RUN_OVER, "description": description}

        reward_objects = result.rewards.get("reward_objects", [])
        if reward_objects:
            self._current_rewards = RewardsSet(self._run_state.player.player_id).with_custom_rewards(reward_objects)
            self._pending_rewards = self._current_rewards.generate_without_offering(self._run_state)
            self._phase = self.PHASE_CARD_REWARD
            self._advance_post_combat_rewards()
            return {
                "phase": self.phase,
                "description": description,
                "finished": True,
            }

        if self._run_state.pending_rewards:
            self._consume_run_pending_rewards()
            return {
                "phase": self.phase,
                "description": description,
                "finished": True,
            }

        self._enter_map_choice()
        return {"phase": self.phase, "description": description}

    def _do_treasure_collect(self) -> dict:
        # Simple treasure: gain gold
        gold = self._rng.next_int(30, 60)
        self._run_state.player.gain_gold(gold)

        self._enter_map_choice()
        return {
            "phase": self.phase,
            "description": f"Collected treasure: {gold} gold.",
            "gold": gold,
        }

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
