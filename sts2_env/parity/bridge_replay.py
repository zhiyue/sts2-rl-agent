"""Bridge replay recording and simulator comparison harness.

This module provides a minimal golden-comparison loop for parity work:

1. Record bridge states/actions from a real game or a prerecorded source.
2. Save them as a deterministic replay JSON file.
3. Recreate the same combat or run phase in the simulator.
4. Replay the recorded actions and compare each resulting state snapshot.

Supported bridge message types:

- `combat_action`
- `card_select`
- `map_select`
- `reward_screen`
- `card_bundle`
- `crystal_sphere`
- `card_reward`
- `rest_site`
- `shop`
- `event`
- `treasure`
- `boss_relic`
- `game_over`
- `run_complete`
"""

from __future__ import annotations

from dataclasses import asdict, dataclass, field
import importlib
import json
from pathlib import Path
from typing import TYPE_CHECKING, Any, Callable

from sts2_env.bridge.protocol import BridgeAction, BridgeStateType
from sts2_env.core.enums import CardType, TargetType

if TYPE_CHECKING:
    from sts2_env.core.combat import CombatState
    from sts2_env.run.run_manager import RunManager

STATE_TYPE_COMBAT = BridgeStateType.COMBAT_ACTION
STATE_TYPE_CARD_SELECT = BridgeStateType.CARD_SELECT
STATE_TYPE_MAP_SELECT = BridgeStateType.MAP_SELECT
STATE_TYPE_REWARD_SCREEN = BridgeStateType.REWARD_SCREEN
STATE_TYPE_CARD_BUNDLE = BridgeStateType.CARD_BUNDLE
STATE_TYPE_CRYSTAL_SPHERE = BridgeStateType.CRYSTAL_SPHERE
STATE_TYPE_CARD_REWARD = BridgeStateType.CARD_REWARD
STATE_TYPE_REST_SITE = BridgeStateType.REST_SITE
STATE_TYPE_SHOP = BridgeStateType.SHOP
STATE_TYPE_EVENT = BridgeStateType.EVENT
STATE_TYPE_TREASURE = BridgeStateType.TREASURE
STATE_TYPE_BOSS_RELIC = BridgeStateType.BOSS_RELIC
STATE_TYPE_GAME_OVER = BridgeStateType.GAME_OVER
STATE_TYPE_RUN_COMPLETE = BridgeStateType.RUN_COMPLETE
SUPPORTED_STATE_TYPES = frozenset({
    STATE_TYPE_COMBAT,
    STATE_TYPE_CARD_SELECT,
    STATE_TYPE_MAP_SELECT,
    STATE_TYPE_REWARD_SCREEN,
    STATE_TYPE_CARD_BUNDLE,
    STATE_TYPE_CRYSTAL_SPHERE,
    STATE_TYPE_CARD_REWARD,
    STATE_TYPE_REST_SITE,
    STATE_TYPE_SHOP,
    STATE_TYPE_EVENT,
    STATE_TYPE_TREASURE,
    STATE_TYPE_BOSS_RELIC,
    STATE_TYPE_GAME_OVER,
    STATE_TYPE_RUN_COMPLETE,
})

_CARD_TYPE_NAMES = {
    CardType.ATTACK: "Attack",
    CardType.SKILL: "Skill",
    CardType.POWER: "Power",
    CardType.STATUS: "Status",
    CardType.CURSE: "Curse",
    CardType.QUEST: "Quest",
}

_TARGET_TYPE_NAMES = {
    TargetType.SELF: "Self",
    TargetType.NONE: "None",
    TargetType.ANY_ENEMY: "AnyEnemy",
    TargetType.ALL_ENEMIES: "AllEnemies",
    TargetType.RANDOM_ENEMY: "RandomEnemy",
    TargetType.ANY_ALLY: "AnyAlly",
    TargetType.ALL_ALLIES: "AllAllies",
}

REST_SITE_REPLAY_ACTIONS = frozenset({"rest_option"})
SHOP_REPLAY_ACTIONS = frozenset({"leave_shop", "buy_card", "buy_relic", "buy_potion", "remove_card"})
EVENT_REPLAY_ACTIONS = frozenset({"event_choice"})
TREASURE_REPLAY_ACTIONS = frozenset({"collect"})
BOSS_RELIC_REPLAY_ACTIONS = frozenset({"pick_relic"})
CARD_BUNDLE_REPLAY_ACTIONS = frozenset({"pick_card_bundle"})


@dataclass(slots=True)
class BridgeReplayStep:
    """One action followed by the resulting recorded state."""

    action: dict[str, Any]
    resulting_state: dict[str, Any]


@dataclass(slots=True)
class BridgeReplayTrace:
    """A combat replay trace suitable for golden comparison."""

    version: int = 1
    mode: str = "combat"
    metadata: dict[str, Any] = field(default_factory=dict)
    initial_state: dict[str, Any] = field(default_factory=dict)
    steps: list[BridgeReplayStep] = field(default_factory=list)

    def to_dict(self) -> dict[str, Any]:
        return {
            "version": self.version,
            "mode": self.mode,
            "metadata": self.metadata,
            "initial_state": self.initial_state,
            "steps": [asdict(step) for step in self.steps],
        }

    @classmethod
    def from_dict(cls, data: dict[str, Any]) -> BridgeReplayTrace:
        return cls(
            version=int(data.get("version", 1)),
            mode=str(data.get("mode", "combat")),
            metadata=dict(data.get("metadata", {})),
            initial_state=dict(data.get("initial_state", {})),
            steps=[
                BridgeReplayStep(
                    action=dict(step.get("action", {})),
                    resulting_state=dict(step.get("resulting_state", {})),
                )
                for step in data.get("steps", [])
            ],
        )


@dataclass(slots=True)
class ReplayComparison:
    """Result of replaying a recorded trace against the simulator."""

    success: bool
    mismatches: list[str] = field(default_factory=list)


class BridgeReplayRecorder:
    """Record bridge states and outgoing actions into a replay trace."""

    def __init__(
        self,
        client: Any,
        *,
        metadata: dict[str, Any] | None = None,
        state_filter: Callable[[dict[str, Any]], bool] | None = None,
    ):
        self._client = client
        self._state_filter = state_filter or self._default_state_filter
        self._pending_action: dict[str, Any] | None = None
        self.trace = BridgeReplayTrace(metadata=dict(metadata or {}))

    @staticmethod
    def _default_state_filter(state: dict[str, Any]) -> bool:
        return state.get("type") in SUPPORTED_STATE_TYPES

    def receive_state(self) -> dict[str, Any]:
        state = self._client.receive_state()
        if not self._state_filter(state):
            return state
        normalized = normalize_bridge_state(state)
        if not self.trace.initial_state:
            self.trace.initial_state = normalized
        elif self._pending_action is not None:
            self.trace.steps.append(
                BridgeReplayStep(
                    action=dict(self._pending_action),
                    resulting_state=normalized,
                )
            )
            self._pending_action = None
        return state

    def send_action(self, action: dict[str, Any]) -> None:
        self._client.send_action(action)
        self._pending_action = dict(action)

    def play_card(self, card_index: int, target_index: int = -1) -> None:
        self.send_action({
            "action": BridgeAction.PLAY,
            "card_index": card_index,
            "target_index": target_index,
        })

    def end_turn(self) -> None:
        self.send_action({"action": BridgeAction.END_TURN})

    def choose(self, index: int) -> None:
        self.send_action({"action": BridgeAction.CHOOSE, "index": index})

    def choose_many(self, indexes: list[int]) -> None:
        self.send_action({"action": BridgeAction.CHOOSE, "indexes": indexes})

    def skip(self) -> None:
        self.send_action({"action": BridgeAction.SKIP})

    def use_potion(self, slot: int, target_index: int = -1) -> None:
        self.send_action({"action": BridgeAction.POTION, "slot": slot, "target_index": target_index})

    def save(self, path: str | Path) -> Path:
        return save_replay_trace(self.trace, path)

    def __getattr__(self, name: str) -> Any:
        return getattr(self._client, name)


def save_replay_trace(trace: BridgeReplayTrace, path: str | Path) -> Path:
    target = Path(path)
    target.write_text(json.dumps(trace.to_dict(), indent=2, sort_keys=True))
    return target


def load_replay_trace(path: str | Path) -> BridgeReplayTrace:
    trace = BridgeReplayTrace.from_dict(json.loads(Path(path).read_text()))
    # Normalize intent casing etc. so traces recorded by older recorder
    # versions compare cleanly against simulator serialization.
    trace.initial_state = normalize_bridge_state(trace.initial_state)
    for step in trace.steps:
        step.resulting_state = normalize_bridge_state(step.resulting_state)
    return trace


def _normalize_powers(powers: list[dict[str, Any]] | None) -> list[dict[str, Any]]:
    normalized: list[dict[str, Any]] = []
    for power in powers or []:
        normalized.append({
            "id": str(power.get("id", "UNKNOWN")),
            "amount": int(power.get("amount", 0)),
        })
    normalized.sort(key=lambda item: (item["id"], item["amount"]))
    return normalized


def _normalize_cards(cards: list[dict[str, Any]] | None) -> list[dict[str, Any]]:
    normalized: list[dict[str, Any]] = []
    for card in cards or []:
        item = {
            "id": str(card.get("id", "UNKNOWN")),
            "type": str(card.get("type", "UNKNOWN")),
        }
        if "cost" in card:
            item["cost"] = int(card.get("cost", 0))
        if "target" in card:
            item["target"] = str(card.get("target"))
        if "playable" in card:
            item["playable"] = bool(card.get("playable"))
        if card.get("upgraded"):
            item["upgraded"] = True
        if "base_damage" in card and card.get("base_damage") is not None:
            item["base_damage"] = int(card["base_damage"])
        if "base_block" in card and card.get("base_block") is not None:
            item["base_block"] = int(card["base_block"])
        normalized.append(item)
    return normalized


def _normalize_enemies(enemies: list[dict[str, Any]] | None) -> list[dict[str, Any]]:
    normalized: list[dict[str, Any]] = []
    for enemy in enemies or []:
        item = {
            "id": str(enemy.get("id", "UNKNOWN")),
            "hp": int(enemy.get("hp", 0)),
            "max_hp": int(enemy.get("max_hp", 0)),
            "block": int(enemy.get("block", 0)),
            "is_alive": bool(enemy.get("is_alive", False)),
            "powers": _normalize_powers(enemy.get("powers")),
        }
        if "intent" in enemy:
            item["intent"] = str(enemy.get("intent", "UNKNOWN"))
        if "intent_damage" in enemy:
            item["intent_damage"] = int(enemy.get("intent_damage", 0))
        if "intent_hits" in enemy:
            item["intent_hits"] = int(enemy.get("intent_hits", 1))
        normalized.append(item)
    return normalized


def _normalize_options(options: list[dict[str, Any]] | None) -> list[dict[str, Any]]:
    normalized: list[dict[str, Any]] = []
    for idx, option in enumerate(options or []):
        item = {
            "index": int(option.get("index", idx)),
            "label": str(option.get("label", "")),
            "enabled": bool(option.get("enabled", True)),
        }
        if option.get("id") is not None:
            item["id"] = str(option["id"])
        if option.get("action") is not None:
            item["action"] = str(option["action"])
        if option.get("description") is not None:
            item["description"] = str(option["description"])
        normalized.append(item)
    return normalized


def _normalize_action_options(options: list[dict[str, Any]] | None) -> list[dict[str, Any]]:
    normalized: list[dict[str, Any]] = []
    for idx, option in enumerate(options or []):
        normalized.append({
            "index": int(option.get("index", idx)),
            "action": str(option.get("action", "")),
            "enabled": bool(option.get("enabled", True)),
        })
    return normalized


def _normalize_card_bundles(bundles: list[dict[str, Any]] | None) -> list[dict[str, Any]]:
    normalized: list[dict[str, Any]] = []
    for idx, bundle in enumerate(bundles or []):
        normalized.append({
            "index": int(bundle.get("index", idx)),
            "action": str(bundle.get("action", "")),
            "cards": _normalize_cards(bundle.get("cards")),
            "enabled": bool(bundle.get("enabled", True)),
        })
    return normalized


def _canonical_intent_name(name: str) -> str:
    """Match the C# bridge's IntentType.ToString() (e.g. "StatusCard")
    case-insensitively against enum names ("STATUS_CARD") for comparison."""
    return str(name).replace("_", "").casefold()


def normalize_bridge_state(state: dict[str, Any]) -> dict[str, Any]:
    for enemy in (state.get("enemies") or []):
        if isinstance(enemy, dict) and enemy.get("intent"):
            enemy["intent"] = _canonical_intent_name(enemy["intent"])
    """Normalize a raw bridge message into a stable comparison shape."""
    state_type = state.get("type")
    if state_type == STATE_TYPE_COMBAT:
        player = state.get("player", {})
        return {
            "type": STATE_TYPE_COMBAT,
            "player": {
                "hp": int(player.get("hp", 0)),
                "max_hp": int(player.get("max_hp", 0)),
                "block": int(player.get("block", 0)),
                "energy": int(player.get("energy", 0)),
                "max_energy": int(player.get("max_energy", 0)),
                "powers": _normalize_powers(player.get("powers")),
            },
            "hand": _normalize_cards(state.get("hand")),
            "enemies": _normalize_enemies(state.get("enemies")),
            "draw_pile_count": int(state.get("draw_pile_count", 0)),
            "discard_pile_count": int(state.get("discard_pile_count", 0)),
            "exhaust_pile_count": int(state.get("exhaust_pile_count", 0)),
            "round": int(state.get("round", 0)),
        }
    if state_type == STATE_TYPE_CARD_SELECT:
        return {
            "type": STATE_TYPE_CARD_SELECT,
            "cards": _normalize_cards(state.get("cards")),
            "min_select": int(state.get("min_select", 1)),
            "max_select": int(state.get("max_select", 1)),
        }
    if state_type == STATE_TYPE_MAP_SELECT:
        return {
            "type": STATE_TYPE_MAP_SELECT,
            "nodes": [
                {
                    "index": int(node.get("index", idx)),
                    "type": str(node.get("type", "UNKNOWN")),
                    "row": int(node.get("row", 0)),
                    "col": int(node.get("col", 0)),
                }
                for idx, node in enumerate(state.get("nodes", []))
            ],
            "floor": int(state.get("floor", 0)),
            "act": int(state.get("act", 0)),
        }
    if state_type == STATE_TYPE_CARD_REWARD:
        cards = []
        for idx, card in enumerate(state.get("cards", [])):
            normalized = _normalize_cards([card])[0]
            normalized["index"] = int(card.get("index", idx))
            cards.append(normalized)
        return {
            "type": STATE_TYPE_CARD_REWARD,
            "cards": cards,
            "can_skip": bool(state.get("can_skip", False)),
        }
    if state_type == STATE_TYPE_CARD_BUNDLE:
        return {
            "type": STATE_TYPE_CARD_BUNDLE,
            "bundles": _normalize_card_bundles(state.get("bundles")),
            "floor": int(state.get("floor", 0)),
            "act": int(state.get("act", 0)),
        }
    if state_type == STATE_TYPE_REST_SITE:
        return {
            "type": state_type,
            "options": _normalize_options(state.get("options")),
            "floor": int(state.get("floor", 0)),
            "act": int(state.get("act", 0)),
        }
    if state_type in {
        STATE_TYPE_REWARD_SCREEN,
        STATE_TYPE_CRYSTAL_SPHERE,
        STATE_TYPE_SHOP,
        STATE_TYPE_EVENT,
        STATE_TYPE_TREASURE,
        STATE_TYPE_BOSS_RELIC,
    }:
        return {
            "type": state_type,
            "options": _normalize_action_options(state.get("options")),
            "floor": int(state.get("floor", 0)),
            "act": int(state.get("act", 0)),
        }
    if state_type in {STATE_TYPE_GAME_OVER, STATE_TYPE_RUN_COMPLETE}:
        normalized: dict[str, Any] = {"type": state_type}
        if "result" in state:
            normalized["result"] = str(state.get("result", ""))
        if "message" in state:
            normalized["message"] = str(state.get("message", ""))
        return normalized
    raise ValueError(f"Unsupported bridge state type for replay comparison: {state_type!r}")


def _run_choice_state(run: RunManager, state_type: str, actions: list[dict[str, Any]]) -> dict[str, Any]:
    return normalize_bridge_state({
        "type": state_type,
        "options": [
            {
                "index": index,
                "id": action.get("option_id", action.get("action", "")),
                "action": action.get("action", ""),
                "label": action.get("label", action.get("card_id", action.get("relic_id", action.get("potion_id", "")))),
                "description": action.get("description", ""),
                "enabled": action.get("enabled", True),
            }
            for index, action in enumerate(actions)
        ],
        "floor": run.run_state.total_floor,
        "act": run.run_state.current_act_index + 1,
    })


def combat_state_to_bridge_state(combat: CombatState) -> dict[str, Any]:
    """Serialize simulator combat into the bridge's comparison shape."""
    if combat.pending_choice is not None:
        return normalize_bridge_state({
            "type": STATE_TYPE_CARD_SELECT,
            "cards": [
                {
                    "id": option.card.card_id.name,
                    "type": _CARD_TYPE_NAMES[option.card.card_type],
                    "upgraded": option.card.upgraded or None,
                }
                for option in combat.pending_choice.options
            ],
            "min_select": combat.pending_choice.min_choices,
            "max_select": combat.pending_choice.max_choices,
        })

    enemies: list[dict[str, Any]] = []
    for enemy in combat.enemies:
        enemy_data: dict[str, Any] = {
            "id": enemy.monster_id or "UNKNOWN",
            "hp": enemy.current_hp,
            "max_hp": enemy.max_hp,
            "block": enemy.block,
            "is_alive": enemy.is_alive,
            "powers": [
                {"id": power_id.name, "amount": power.amount}
                for power_id, power in enemy.powers.items()
                if power.amount != 0
            ],
        }
        ai = combat.enemy_ais.get(enemy.combat_id)
        if ai is not None:
            intents = ai.current_move.intents
            if intents:
                first_intent = intents[0]
                enemy_data["intent"] = _canonical_intent_name(first_intent.intent_type.name)
                enemy_data["intent_damage"] = first_intent.damage
                enemy_data["intent_hits"] = first_intent.hits
        enemies.append(enemy_data)

    return normalize_bridge_state({
        "type": STATE_TYPE_COMBAT,
        "player": {
            "hp": combat.player.current_hp,
            "max_hp": combat.player.max_hp,
            "block": combat.player.block,
            "energy": combat.energy,
            "max_energy": combat.max_energy,
            "powers": [
                {"id": power_id.name, "amount": power.amount}
                for power_id, power in combat.player.powers.items()
                if power.amount != 0
            ],
        },
        "hand": [
            {
                "id": card.card_id.name,
                "cost": card.cost,
                "type": _CARD_TYPE_NAMES[card.card_type],
                "target": _TARGET_TYPE_NAMES[card.target_type],
                "playable": combat.can_play_card(card),
                "upgraded": card.upgraded or None,
                "base_damage": card.base_damage,
                "base_block": card.base_block,
            }
            for card in combat.hand
        ],
        "enemies": enemies,
        "draw_pile_count": len(combat.draw_pile),
        "discard_pile_count": len(combat.discard_pile),
        "exhaust_pile_count": len(combat.exhaust_pile),
        "round": combat.round_number,
    })


def run_manager_to_bridge_state(run: RunManager) -> dict[str, Any]:
    """Serialize supported RunManager phases into the bridge comparison shape."""
    from sts2_env.run.run_manager import RunManager

    phase = run.phase
    if phase == RunManager.PHASE_COMBAT:
        combat = run.get_combat_state()
        if combat is None:
            raise ValueError("RunManager reported COMBAT without an active CombatState")
        return combat_state_to_bridge_state(combat)

    if phase == RunManager.PHASE_MAP_CHOICE:
        move_actions = [action for action in run.get_available_actions() if action.get("action") == "move"]
        return normalize_bridge_state({
            "type": STATE_TYPE_MAP_SELECT,
            "nodes": [
                {
                    "index": idx,
                    "type": str(action.get("point_type", "UNKNOWN")).title().replace("_", ""),
                    "row": int(action["coord"][1]),
                    "col": int(action["coord"][0]),
                }
                for idx, action in enumerate(move_actions)
            ],
            "floor": run.run_state.total_floor,
            "act": run.run_state.current_act_index + 1,
        })

    if phase == RunManager.PHASE_CARD_REWARD:
        actions = run.get_available_actions()
        bundle_actions = [action for action in actions if action.get("action") in CARD_BUNDLE_REPLAY_ACTIONS]
        if bundle_actions:
            offered_bundles = getattr(run, "_offered_card_bundles", [])
            return normalize_bridge_state({
                "type": STATE_TYPE_CARD_BUNDLE,
                "bundles": [
                    {
                        "index": int(action["index"]),
                        "action": action["action"],
                        "cards": [
                            {
                                "id": card.card_id.name,
                                "type": _CARD_TYPE_NAMES[card.card_type],
                                "cost": card.cost,
                                "upgraded": card.upgraded or None,
                            }
                            for card in offered_bundles[action["index"]]
                        ],
                        "enabled": True,
                    }
                    for action in bundle_actions
                ],
                "floor": run.run_state.total_floor,
                "act": run.run_state.current_act_index + 1,
            })
        card_actions = [action for action in actions if action.get("action") == "pick_card"]
        offered_cards = getattr(run, "_offered_cards", [])
        return normalize_bridge_state({
            "type": STATE_TYPE_CARD_REWARD,
            "cards": [
                {
                    "index": int(action["index"]),
                    "id": action["card_id"],
                    "type": _CARD_TYPE_NAMES[offered_cards[action["index"]].card_type],
                    "cost": offered_cards[action["index"]].cost,
                    "upgraded": offered_cards[action["index"]].upgraded or None,
                }
                for action in card_actions
            ],
            "can_skip": any(action.get("action") == BridgeAction.SKIP for action in actions),
        })

    if phase == RunManager.PHASE_REST_SITE:
        actions = [action for action in run.get_available_actions() if action.get("action") in REST_SITE_REPLAY_ACTIONS]
        return _run_choice_state(run, STATE_TYPE_REST_SITE, actions)

    if phase == RunManager.PHASE_SHOP:
        actions = [
            action
            for action in run.get_available_actions()
            if action.get("action") in SHOP_REPLAY_ACTIONS
        ]
        return _run_choice_state(run, STATE_TYPE_SHOP, actions)

    if phase == RunManager.PHASE_EVENT:
        actions = [action for action in run.get_available_actions() if action.get("action") in EVENT_REPLAY_ACTIONS]
        return _run_choice_state(run, STATE_TYPE_EVENT, actions)

    if phase == RunManager.PHASE_TREASURE:
        actions = [action for action in run.get_available_actions() if action.get("action") in TREASURE_REPLAY_ACTIONS]
        return _run_choice_state(run, STATE_TYPE_TREASURE, actions)

    if phase == RunManager.PHASE_BOSS_RELIC:
        actions = [action for action in run.get_available_actions() if action.get("action") in BOSS_RELIC_REPLAY_ACTIONS]
        return _run_choice_state(run, STATE_TYPE_BOSS_RELIC, actions)

    raise ValueError(f"RunManager phase {phase!r} is not supported by the replay harness")


def _resolve_factory(factory: str | Callable[..., Any]) -> Callable[..., Any]:
    if callable(factory):
        return factory
    module_name, sep, attr_name = factory.partition(":")
    if not sep:
        raise ValueError("Factory path must use 'module:function' format")
    module = importlib.import_module(module_name)
    resolved = getattr(module, attr_name)
    if not callable(resolved):
        raise TypeError(f"Resolved factory {factory!r} is not callable")
    return resolved


def _diff_values(expected: Any, actual: Any, path: str, out: list[str]) -> None:
    if isinstance(expected, dict):
        if not isinstance(actual, dict):
            out.append(f"{path}: expected dict, got {type(actual).__name__}")
            return
        for key, value in expected.items():
            if key not in actual:
                out.append(f"{path}.{key}: missing in actual state")
                continue
            _diff_values(value, actual[key], f"{path}.{key}", out)
        return
    if isinstance(expected, list):
        if not isinstance(actual, list):
            out.append(f"{path}: expected list, got {type(actual).__name__}")
            return
        if len(expected) != len(actual):
            out.append(f"{path}: expected {len(expected)} items, got {len(actual)}")
            return
        for idx, (expected_item, actual_item) in enumerate(zip(expected, actual, strict=True)):
            _diff_values(expected_item, actual_item, f"{path}[{idx}]", out)
        return
    if expected != actual:
        out.append(f"{path}: expected {expected!r}, got {actual!r}")


def _compare_state(expected: dict[str, Any], actual: dict[str, Any], label: str) -> list[str]:
    normalized_expected = normalize_bridge_state(expected)
    normalized_actual = normalize_bridge_state(actual)
    diffs: list[str] = []
    _diff_values(normalized_expected, normalized_actual, label, diffs)
    return diffs


def _apply_replay_action(combat: CombatState, action: dict[str, Any]) -> None:
    action_type = action.get("action")
    if action_type == BridgeAction.PLAY:
        success = combat.play_card(int(action["card_index"]), action.get("target_index"))
        if not success:
            raise AssertionError(f"Simulator failed to apply play action: {action}")
        return
    if action_type == BridgeAction.END_TURN:
        combat.end_player_turn()
        return
    if action_type == BridgeAction.CHOOSE:
        if "indexes" in action:
            for index in action["indexes"]:
                success = combat.resolve_pending_choice(int(index))
                if not success:
                    raise AssertionError(f"Simulator failed to apply choose-many action: {action}")
            success = combat.resolve_pending_choice(None)
            if not success:
                raise AssertionError(f"Simulator failed to confirm choose-many action: {action}")
            return
        success = combat.resolve_pending_choice(int(action["index"]))
        if not success:
            raise AssertionError(f"Simulator failed to apply choose action: {action}")
        return
    if action_type == BridgeAction.SKIP:
        success = combat.resolve_pending_choice(None)
        if not success:
            raise AssertionError(f"Simulator failed to apply skip action: {action}")
        return
    if action_type == BridgeAction.POTION:
        success = combat.use_potion(int(action["slot"]), target_index=action.get("target_index"))
        if not success:
            raise AssertionError(f"Simulator failed to apply potion action: {action}")
        return
    raise ValueError(f"Unsupported replay action type: {action_type!r}")


def compare_combat_replay(
    trace: BridgeReplayTrace | str | Path,
    factory: str | Callable[..., Any] | None = None,
    *,
    factory_kwargs: dict[str, Any] | None = None,
) -> ReplayComparison:
    """Replay a recorded combat trace against the simulator."""
    resolved_trace = load_replay_trace(trace) if isinstance(trace, (str, Path)) else trace
    factory_kwargs = dict(factory_kwargs or {})

    if factory is None:
        factory = resolved_trace.metadata.get("scenario_factory")
    if factory is None:
        raise ValueError("No combat factory provided for replay comparison")
    factory_fn = _resolve_factory(factory)

    combat = factory_fn(**factory_kwargs)
    mismatches = _compare_state(resolved_trace.initial_state, combat_state_to_bridge_state(combat), "initial_state")
    if mismatches:
        return ReplayComparison(success=False, mismatches=mismatches)

    for idx, step in enumerate(resolved_trace.steps):
        try:
            _apply_replay_action(combat, step.action)
        except Exception as exc:  # noqa: BLE001
            mismatches.append(f"step[{idx}] action error: {exc}")
            return ReplayComparison(success=False, mismatches=mismatches)

        expected_type = step.resulting_state.get("type")
        actual_state = combat_state_to_bridge_state(combat)
        actual_type = actual_state.get("type")
        if expected_type != actual_type:
            mismatches.append(
                f"step[{idx}] state type mismatch: expected {expected_type!r}, got {actual_type!r}"
            )
            return ReplayComparison(success=False, mismatches=mismatches)

        diffs = _compare_state(step.resulting_state, actual_state, f"step[{idx}]")
        if diffs:
            mismatches.extend(diffs)
            return ReplayComparison(success=False, mismatches=mismatches)

    return ReplayComparison(success=True)


def _apply_run_replay_action(run: RunManager, current_state_type: str, action: dict[str, Any]) -> None:
    if current_state_type == STATE_TYPE_MAP_SELECT:
        move_actions = [candidate for candidate in run.get_available_actions() if candidate.get("action") == "move"]
        index = int(action.get("index", -1))
        if not (0 <= index < len(move_actions)):
            raise AssertionError(f"Invalid map_select index {index} for {len(move_actions)} available moves")
        run.take_action(move_actions[index])
        return

    if current_state_type == STATE_TYPE_CARD_REWARD:
        if action.get("action") == BridgeAction.SKIP:
            run.take_action({"action": BridgeAction.SKIP})
            return
        index = int(action.get("index", -1))
        run.take_action({"action": "pick_card", "index": index})
        return

    if current_state_type == STATE_TYPE_CARD_BUNDLE:
        if action.get("action") != BridgeAction.CHOOSE:
            raise ValueError(f"Unsupported card_bundle action type: {action.get('action')!r}")
        bundle_actions = [
            candidate
            for candidate in run.get_available_actions()
            if candidate.get("action") in CARD_BUNDLE_REPLAY_ACTIONS
        ]
        index = int(action.get("index", -1))
        if not (0 <= index < len(bundle_actions)):
            raise AssertionError(f"Invalid card_bundle index {index} for {len(bundle_actions)} available bundles")
        run.take_action({"action": "pick_card_bundle", "index": index})
        return

    if current_state_type == STATE_TYPE_COMBAT:
        combat = run.get_combat_state()
        if combat is None:
            raise AssertionError("Run replay expected a live combat state")
        _apply_replay_action(combat, action)
        return

    if current_state_type == STATE_TYPE_CARD_SELECT:
        combat = run.get_combat_state()
        if combat is None:
            raise AssertionError("Card-select replay currently requires a live combat state")
        _apply_replay_action(combat, action)
        return

    if current_state_type in {
        STATE_TYPE_REST_SITE,
        STATE_TYPE_SHOP,
        STATE_TYPE_EVENT,
        STATE_TYPE_TREASURE,
        STATE_TYPE_BOSS_RELIC,
    }:
        if action.get("action") != BridgeAction.CHOOSE:
            raise ValueError(f"Unsupported {current_state_type} action type: {action.get('action')!r}")
        actions_by_state = {
            STATE_TYPE_REST_SITE: REST_SITE_REPLAY_ACTIONS,
            STATE_TYPE_SHOP: SHOP_REPLAY_ACTIONS,
            STATE_TYPE_EVENT: EVENT_REPLAY_ACTIONS,
            STATE_TYPE_TREASURE: TREASURE_REPLAY_ACTIONS,
            STATE_TYPE_BOSS_RELIC: BOSS_RELIC_REPLAY_ACTIONS,
        }
        selectable_actions = [
            candidate
            for candidate in run.get_available_actions()
            if candidate.get("action") in actions_by_state[current_state_type]
        ]
        index = int(action.get("index", -1))
        if not (0 <= index < len(selectable_actions)):
            raise AssertionError(
                f"Invalid {current_state_type} index {index} for {len(selectable_actions)} available options"
            )
        run.take_action(selectable_actions[index])
        return

    raise ValueError(f"Unsupported replay state type: {current_state_type!r}")


def compare_run_replay(
    trace: BridgeReplayTrace | str | Path,
    factory: str | Callable[..., Any] | None = None,
    *,
    factory_kwargs: dict[str, Any] | None = None,
) -> ReplayComparison:
    """Replay a recorded run trace against a simulator RunManager."""
    resolved_trace = load_replay_trace(trace) if isinstance(trace, (str, Path)) else trace
    factory_kwargs = dict(factory_kwargs or {})

    if factory is None:
        factory = resolved_trace.metadata.get("scenario_factory")
    if factory is None:
        raise ValueError("No run factory provided for replay comparison")
    factory_fn = _resolve_factory(factory)

    run = factory_fn(**factory_kwargs)
    mismatches = _compare_state(resolved_trace.initial_state, run_manager_to_bridge_state(run), "initial_state")
    if mismatches:
        return ReplayComparison(success=False, mismatches=mismatches)

    current_state_type = resolved_trace.initial_state.get("type")
    for idx, step in enumerate(resolved_trace.steps):
        try:
            _apply_run_replay_action(run, current_state_type, step.action)
        except Exception as exc:  # noqa: BLE001
            mismatches.append(f"step[{idx}] action error: {exc}")
            return ReplayComparison(success=False, mismatches=mismatches)

        actual_state = run_manager_to_bridge_state(run)
        expected_type = step.resulting_state.get("type")
        actual_type = actual_state.get("type")
        if expected_type != actual_type:
            mismatches.append(
                f"step[{idx}] state type mismatch: expected {expected_type!r}, got {actual_type!r}"
            )
            return ReplayComparison(success=False, mismatches=mismatches)

        diffs = _compare_state(step.resulting_state, actual_state, f"step[{idx}]")
        if diffs:
            mismatches.extend(diffs)
            return ReplayComparison(success=False, mismatches=mismatches)
        current_state_type = actual_type

    return ReplayComparison(success=True)


def combat_state_from_bridge_state(
    state: dict[str, Any],
    *,
    rng_seed: int = 42,
    default_character: str = "Ironclad",
) -> "CombatState":
    """Rebuild a simulator CombatState from a bridge-shaped combat state.

    This is the inverse of :func:`combat_state_to_bridge_state` restricted to
    the fields a live bridge trace actually carries. It enables replaying a
    live-game trace through the simulator: the recorded ``initial_state``
    becomes the scenario factory argument, and the comparator then checks the
    *transitions* (game's actual next state vs simulator's next state after
    the same action).

    Known v1 limitations (refine against real traces):
    - pile contents are not in the bridge state; the draw pile is filled with
      the recorded deck order when available, otherwise Strike placeholders,
      so only ``*_pile_count`` fields compare exactly;
    - enemy AIs are rebuilt through the create_<monster> factory index so
      intents match; the AI's internal RNG starts at ``rng_seed + slot``
      rather than the game's live stream, so intent *choices* can diverge
      after the first random event by design;
    - relics/potions are not carried over (the live combat state does not
      list them per combat snapshot).
    """
    from sts2_env.cards.factory import create_card
    from sts2_env.core.combat import CombatState
    from sts2_env.core.creature import Creature
    from sts2_env.core.enums import CardId, CombatSide, PowerId
    from sts2_env.core.rng import Rng

    import sts2_env.powers  # noqa: F401  (register power classes)

    player = state.get("player", {})
    max_hp = int(player.get("max_hp") or player.get("hp") or 80)
    hp = int(player.get("hp", max_hp))

    deck_entries = state.get("deck") or []
    deck = []
    for entry in deck_entries:
        try:
            deck.append(create_card(CardId[str(entry["id"]).upper()]))
        except (KeyError, TypeError, ValueError):
            continue
    if not deck:
        # Placeholder fill so pile counts compare; replace from trace metadata
        # once the live state exposes the full deck.
        deck = [create_card(CardId.STRIKE_IRONCLAD) for _ in range(10)]

    combat = CombatState(
        player_hp=hp,
        player_max_hp=max_hp,
        deck=deck,
        rng_seed=rng_seed,
        relics=[],
        gold=0,
        character_id=default_character,
    )
    combat.player.current_hp = hp
    combat.player.block = int(player.get("block") or 0)
    combat.energy = int(player.get("energy") or 0)
    if player.get("max_energy") is not None:
        combat.base_max_energy = int(player["max_energy"])
    for power in player.get("powers") or []:
        try:
            combat.player.apply_power(PowerId[str(power["id"]).upper()], int(power.get("amount", 1)))
        except (KeyError, ValueError):
            continue

    for entry in state.get("hand") or []:
        try:
            card = create_card(CardId[str(entry["id"]).upper()], upgraded=bool(entry.get("upgraded")))
        except (KeyError, TypeError, ValueError):
            continue
        combat.hand.append(card)

    for index, enemy_data in enumerate(state.get("enemies") or []):
        e_max = int(enemy_data.get("max_hp") or enemy_data.get("hp") or 50)
        e_hp = int(enemy_data.get("hp", e_max))
        monster_id = str(enemy_data.get("id", "UNKNOWN"))
        enemy = None
        ai = None
        factory = _monster_factory_by_id(monster_id)
        if factory is not None:
            try:
                enemy, ai = factory(Rng(rng_seed + index))
            except Exception:
                enemy, ai = None, None
        if enemy is None:
            enemy = Creature(
                max_hp=e_max,
                current_hp=e_hp,
                side=CombatSide.ENEMY,
                monster_id=monster_id,
            )
        else:
            enemy.max_hp = e_max
            enemy.current_hp = e_hp
        enemy.block = int(enemy_data.get("block") or 0)
        for power in enemy_data.get("powers") or []:
            try:
                enemy.apply_power(PowerId[str(power["id"]).upper()], int(power.get("amount", 1)))
            except (KeyError, ValueError):
                continue
        combat.add_enemy(enemy, ai)

    draw_count = int(state.get("draw_pile_count") or 0)
    combat.draw_pile = [create_card(CardId.STRIKE_IRONCLAD) for _ in range(draw_count)]
    combat.discard_pile = [create_card(CardId.STRIKE_IRONCLAD) for _ in range(int(state.get("discard_pile_count") or 0))]
    combat.exhaust_pile = []
    combat.round_number = int(state.get("round") or 1)
    return combat

_MONSTER_FACTORY_INDEX: dict[str, Callable[..., Any]] | None = None


def _monster_factory_by_id(monster_id: str) -> Callable[..., Any] | None:
    """Index create_<monster>(rng) factories by the monster_id they produce."""
    global _MONSTER_FACTORY_INDEX
    if _MONSTER_FACTORY_INDEX is not None:
        return _MONSTER_FACTORY_INDEX.get(monster_id)
    import importlib

    from sts2_env.core.rng import Rng

    index: dict[str, Callable[..., Any]] = {}
    monsters_pkg = importlib.import_module("sts2_env.monsters")
    seen: set[str] = set()
    for module_name in list(monsters_pkg.__path__):
        from pathlib import Path as _Path

        for file in _Path(module_name).glob("*.py"):
            module = importlib.import_module(f"sts2_env.monsters.{file.stem}")
            for name, obj in vars(module).items():
                if not callable(obj) or not name.startswith("create_"):
                    continue
                try:
                    creature, _ai = obj(Rng(1))
                except Exception:
                    continue
                mid = getattr(creature, "monster_id", None)
                if mid and mid not in seen:
                    seen.add(mid)
                    index[mid] = obj
    _MONSTER_FACTORY_INDEX = index
    return index.get(monster_id)
