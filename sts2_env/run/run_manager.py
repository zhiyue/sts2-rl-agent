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
    CardRarity,
    MapPointType,
    RoomType,
)
from sts2_env.core.rng import Rng
from sts2_env.map.map_point import MapCoord
from sts2_env.run.events import EventModel, EventOption, EventResult, pick_event
from sts2_env.run.rest_site import RestSiteOption, generate_rest_site_options
from sts2_env.run.rewards import generate_combat_card_rewards
from sts2_env.run.rooms import create_room
from sts2_env.run.run_state import RunState
from sts2_env.run.shop import ShopInventory, generate_shop_inventory


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
        self._run_state.relics.append(config["starter_relic"])
        self._heal_after_combat: int = config["heal_after_combat"]

        # Initialize the run (ascension effects + first map)
        self._run_state.initialize_run()

        # Phase tracking
        self._phase: str = self.PHASE_MAP_CHOICE
        self._combat: CombatState | None = None
        self._current_room_type: RoomType | None = None

        # Scratch state for each phase
        self._available_coords: list[MapCoord] = []
        self._offered_cards: list[tuple[CardRarity, bool]] = []
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
        if self._phase == self.PHASE_COMBAT and action_type == "end_turn":
            return self._do_combat_end_turn()
        if self._phase == self.PHASE_CARD_REWARD and action_type == "pick_card":
            return self._do_card_reward_pick(action)
        if self._phase == self.PHASE_CARD_REWARD and action_type == "skip":
            return self._do_card_reward_skip()
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
        if self._phase == self.PHASE_TREASURE and action_type == "collect":
            return self._do_treasure_collect()

        return {
            "phase": self.phase,
            "description": f"Invalid action '{action_type}' for phase '{self._phase}'.",
        }

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
        )

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

    def _enter_card_reward(self, context: str) -> None:
        """Transition to CARD_REWARD after combat."""
        self._phase = self.PHASE_CARD_REWARD
        self._offered_cards = generate_combat_card_rewards(
            self._run_state, context=context, num_cards=3,
        )

    def _enter_boss_relic(self) -> None:
        """Offer three boss relics after defeating a boss."""
        self._phase = self.PHASE_BOSS_RELIC
        self._boss_relics = [
            f"BossRelic_A{self._run_state.current_act_index}_{i}"
            for i in range(3)
        ]

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
        if room_type in (RoomType.MONSTER, RoomType.ELITE, RoomType.BOSS):
            self._enter_combat(room_type)
        elif room_type == RoomType.SHOP:
            self._enter_shop()
        elif room_type == RoomType.REST_SITE:
            self._enter_rest_site()
        elif room_type == RoomType.EVENT:
            self._enter_event()
        elif room_type == RoomType.TREASURE:
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

        # End turn is always available
        actions.append({"action": "end_turn"})

        # Playable cards in hand
        for i, card in enumerate(combat.hand):
            if combat.can_play_card(card):
                card_action: dict[str, Any] = {
                    "action": "play_card",
                    "hand_index": i,
                    "card_id": card.card_id.name,
                    "cost": card.cost,
                }
                if card.target_type.name == "ANY_ENEMY":
                    # One action per alive enemy target
                    for j, enemy in enumerate(combat.enemies):
                        if enemy.is_alive:
                            targeted = dict(card_action)
                            targeted["target_index"] = j
                            targeted["target_name"] = getattr(enemy, "name", f"Enemy_{j}")
                            actions.append(targeted)
                else:
                    # Self-targeted / all-enemies / none
                    card_action["target_index"] = None
                    actions.append(card_action)
        return actions

    def _actions_card_reward(self) -> list[dict]:
        actions: list[dict] = [{"action": "skip"}]
        for i, (rarity, upgraded) in enumerate(self._offered_cards):
            actions.append({
                "action": "pick_card",
                "index": i,
                "rarity": rarity.name,
                "upgraded": upgraded,
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
                    "rarity": entry.rarity.name,
                    "card_type": entry.card_type,
                    "on_sale": entry.on_sale,
                })

        for i, entry in enumerate(inv.colorless_cards):
            if gold >= entry.price:
                actions.append({
                    "action": "buy_card",
                    "index": len(inv.cards) + i,
                    "price": entry.price,
                    "rarity": entry.rarity.name,
                    "card_type": "Colorless",
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
        success = combat.play_card(hand_index, target_index)

        result: dict[str, Any] = {
            "phase": self.phase,
            "description": "Played card." if success else "Failed to play card.",
            "success": success,
        }

        # Check if combat ended
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
        player.current_hp = combat.player.current_hp

        # Post-combat heal (BurningBlood-style)
        healed = 0
        if self._heal_after_combat > 0 and player.current_hp > 0:
            healed = player.heal(self._heal_after_combat)

        # Gold reward
        low, high = _GOLD_REWARDS.get(room_type, (10, 20))
        gold_earned = self._rng.next_int(low, high)
        player.gain_gold(gold_earned)

        # Potion drop chance
        potion_dropped = False
        is_elite = room_type == RoomType.ELITE
        if self._run_state.potion_reward_odds.roll(
            self._rng, is_elite=is_elite,
        ):
            potion_dropped = True
            # Simplified: just note the drop; full implementation would
            # generate a specific potion and add it.

        info: dict[str, Any] = {
            "player_won": True,
            "gold_earned": gold_earned,
            "healed": healed,
            "potion_dropped": potion_dropped,
        }

        # Determine the reward context
        if room_type == RoomType.BOSS:
            context = "boss"
        elif room_type == RoomType.ELITE:
            context = "elite"
        else:
            context = "regular"

        # Transition: card reward first, then boss relic if applicable
        self._enter_card_reward(context)

        info["phase"] = self.phase
        info["description"] = (
            f"Victory! Gained {gold_earned} gold"
            + (f", healed {healed} HP" if healed else "")
            + "."
        )
        return info

    def _do_card_reward_pick(self, action: dict) -> dict:
        index = action.get("index", 0)
        if 0 <= index < len(self._offered_cards):
            rarity, upgraded = self._offered_cards[index]
            # For the real implementation this would look up a concrete card
            # from the character's pool. We record the pick metadata.
            info = {
                "description": f"Picked card (rarity={rarity.name}, upgraded={upgraded}).",
                "rarity": rarity.name,
                "upgraded": upgraded,
            }
        else:
            info = {"description": "Invalid card index."}

        self._after_card_reward()
        info["phase"] = self.phase
        return info

    def _do_card_reward_skip(self) -> dict:
        self._after_card_reward()
        return {"phase": self.phase, "description": "Skipped card reward."}

    def _after_card_reward(self) -> None:
        """Transition after the card reward screen."""
        if self._current_room_type == RoomType.BOSS:
            self._enter_boss_relic()
        else:
            self._enter_map_choice()

    def _do_boss_relic_pick(self, action: dict) -> dict:
        index = action.get("index", 0)
        relic_id = ""
        if 0 <= index < len(self._boss_relics):
            relic_id = self._boss_relics[index]
            self._run_state.relics.append(relic_id)

        description = f"Picked boss relic '{relic_id}'."
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
                    # Mark as sold
                    entry.price = 999999
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
                    self._run_state.relics.append(
                        f"ShopRelic_{entry.relic_rarity.name}_{index}"
                    )
                    entry.price = 999999
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
                    entry.price = 999999
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

        self._enter_map_choice()
        return {"phase": self.phase, "description": description}

    def _do_event_choice(self, action: dict) -> dict:
        option_id = action.get("option_id", "leave")
        event = self._event_model

        if event is not None and option_id != "leave":
            result = event.choose(self._run_state, option_id)
            if not result.finished and result.next_options:
                # Multi-page event -- stay in EVENT phase with new options
                self._event_options = result.next_options
                return {
                    "phase": self.phase,
                    "description": result.description,
                    "finished": False,
                }
            description = result.description
        else:
            description = "Left the event."

        # Check for death from event
        if self._run_state.player.is_dead:
            self._run_state.lose_run()
            self._phase = self.PHASE_RUN_OVER
            return {"phase": self.PHASE_RUN_OVER, "description": description}

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
