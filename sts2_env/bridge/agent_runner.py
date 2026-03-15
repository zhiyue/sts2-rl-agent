"""Agent runner: connects a trained RL model to the real STS2 game.

Main loop:
  1. Connect to the game via TCP (bridge mod must be installed & running)
  2. Load a trained MaskablePPO model
  3. Receive game state -> encode observation -> model.predict -> send action
  4. Handle all game phases (combat, map, rewards, shop, rest, events)

Usage:
  python -m sts2_env.bridge.agent_runner --model-path models/combat_ppo.zip
  python -m sts2_env.bridge.agent_runner --model-path models/combat_ppo.zip --port 9002

The agent uses the trained model for combat decisions and simple heuristics
for non-combat decisions (map navigation, card rewards, etc.).
"""

from __future__ import annotations

import argparse
import logging
import sys
import time
from typing import Any

import numpy as np

from sts2_env.bridge.client import STS2GameClient
from sts2_env.bridge.protocol import Phase
from sts2_env.bridge.state_adapter import StateAdapter

logger = logging.getLogger(__name__)


def load_model(model_path: str) -> Any:
    """Load a trained MaskablePPO model.

    Args:
        model_path: Path to the saved model (.zip file).

    Returns:
        Loaded MaskablePPO model instance.
    """
    try:
        from sb3_contrib import MaskablePPO
    except ImportError:
        logger.error(
            "sb3-contrib is required. Install with: pip install sb3-contrib"
        )
        raise

    logger.info("Loading model from %s", model_path)
    model = MaskablePPO.load(model_path)
    logger.info("Model loaded successfully.")
    return model


def run_agent(
    model_path: str,
    host: str = "127.0.0.1",
    port: int = 9002,
    deterministic: bool = True,
    verbose: bool = False,
) -> None:
    """Main agent loop.

    Connects to the game, loads the model, and plays indefinitely
    until disconnected or interrupted.

    Args:
        model_path: Path to saved MaskablePPO model.
        host: Bridge server host.
        port: Bridge server port.
        deterministic: Whether to use deterministic action selection.
        verbose: Whether to log every action taken.
    """
    model = load_model(model_path)
    adapter = StateAdapter()

    logger.info("Connecting to STS2 at %s:%d...", host, port)

    with STS2GameClient(host=host, port=port) as client:
        logger.info("Connected. Starting agent loop.")

        step_count = 0
        combat_count = 0

        while True:
            try:
                logger.info("Waiting for game state...")
                state = client.receive_state()
                logger.info("Received: type=%s", state.get("type", "?"))
            except TimeoutError:
                logger.warning("Timeout waiting for state. Sending ping...")
                if client.ping():
                    continue
                else:
                    logger.error("Lost connection. Attempting reconnect...")
                    _reconnect_with_retry(client)
                    continue
            except ConnectionError:
                logger.error("Connection lost. Attempting reconnect...")
                _reconnect_with_retry(client)
                continue

            # Mod sends "type" field (e.g. "combat_action", "map_select", "card_reward")
            # Map to our Phase constants
            msg_type = state.get("type", "")
            phase = {
                "combat_action": Phase.COMBAT_PLAY,
                "game_state": state.get("phase", "UNKNOWN"),
                "map_select": Phase.MAP_SELECT,
                "card_reward": Phase.CARD_REWARD,
                "card_select": Phase.CARD_REWARD,
                "rest_site": Phase.REST,
                "shop": Phase.SHOP,
                "event": Phase.EVENT,
                "game_over": "GAME_OVER",
                "pong": "PONG",
                "error": "ERROR",
            }.get(msg_type, state.get("phase", "UNKNOWN"))
            step_count += 1

            if verbose and step_count % 10 == 1:
                logger.info("Step %d: type=%s phase=%s", step_count, msg_type, phase)

            if verbose and msg_type:
                logger.debug("Received: type=%s keys=%s", msg_type, list(state.keys()))

            if phase == "PONG":
                continue
            if phase == "GAME_OVER":
                logger.info("Game over! Result: %s", state.get("result", "unknown"))
                break
            if phase == "ERROR":
                logger.warning("Game error: %s", state.get("message", ""))
                continue

            if phase in Phase.COMBAT_PHASES:
                # ---- Combat: use trained model ----
                obs = adapter.encode_observation(state)
                mask = adapter.compute_action_mask(state)

                # Ensure at least one action is valid
                if mask.sum() == 0:
                    logger.warning("No valid actions! Defaulting to END_TURN.")
                    client.end_turn()
                    continue

                action, _states = model.predict(
                    obs,
                    action_masks=mask,
                    deterministic=deterministic,
                )
                action_int = int(action)

                decoded = adapter.decode_action(action_int, state)

                if verbose:
                    _log_combat_action(state, action_int, decoded)

                if decoded["type"] == "END_TURN":
                    client.end_turn()
                else:
                    client.play_card(
                        decoded["card_index"],
                        decoded.get("target_index", -1),
                    )

            elif phase == Phase.MAP_SELECT:
                # ---- Map: pick path heuristically ----
                choice = _pick_map_node(state)
                if verbose:
                    logger.info("MAP: choosing node %d", choice)
                client.choose(choice)

            elif phase == Phase.CARD_REWARD:
                # ---- Card reward: pick best card ----
                choice = _pick_best_card(state)
                if verbose:
                    logger.info("CARD_REWARD: choosing option %d", choice)
                client.choose(choice)

            elif phase == Phase.REST:
                # ---- Rest: heal if low HP, otherwise upgrade ----
                choice = _pick_rest_option(state)
                if verbose:
                    logger.info("REST: choosing option %d", choice)
                client.choose(choice)

            elif phase == Phase.SHOP:
                # ---- Shop: skip for now ----
                if verbose:
                    logger.info("SHOP: skipping (choosing 0)")
                client.choose(0)

            elif phase == Phase.EVENT:
                # ---- Event: choose first option ----
                if verbose:
                    logger.info("EVENT: choosing option 0")
                client.choose(0)

            elif phase == Phase.COMBAT_WAITING:
                # Game is processing enemy turn / animations — just wait
                pass

            else:
                logger.debug("Unknown phase '%s', waiting...", phase)

            # Log progress periodically
            if step_count % 100 == 0:
                logger.info("Step %d, combats seen: %d", step_count, combat_count)


# ----------------------------------------------------------------
# Heuristic decision functions for non-combat phases
# ----------------------------------------------------------------


def _pick_map_node(state: dict[str, Any]) -> int:
    """Simple heuristic for map node selection.

    Strategy:
      - Prefer elite fights (for relics) if HP is high
      - Prefer rest sites if HP is low
      - Otherwise pick the first available option

    TODO: Replace with a trained map navigation model.
    """
    # For now, just pick the first available node
    return 0


def _pick_best_card(state: dict[str, Any]) -> int:
    """Simple heuristic for card reward selection.

    Strategy:
      - Prefer uncommon/rare cards
      - Prefer cards that synergize with current deck
      - Skip if the deck is already large

    TODO: Replace with a proper card evaluation model.
    """
    run_state = state.get("run_state", {})
    deck_size = len(run_state.get("deck", []))

    # If deck is getting large, skip more often
    if deck_size > 30:
        # Skip (index beyond the offered cards)
        return 99  # Large index = skip in most implementations

    # Otherwise pick the first card (index 0)
    return 0


def _pick_rest_option(state: dict[str, Any]) -> int:
    """Heuristic for rest site decisions.

    Strategy:
      - Rest (heal) if HP < 50%
      - Smith (upgrade a card) otherwise

    Index mapping: 0=Rest, 1=Smith, 2+=special
    """
    run_state = state.get("run_state", {})
    combat = state.get("combat_state")

    # Try to get HP from run state or last combat state
    hp = 0
    max_hp = 1
    if combat:
        player = combat.get("player", {})
        hp = player.get("hp", 0)
        max_hp = max(player.get("max_hp", 1), 1)

    hp_ratio = hp / max_hp if max_hp > 0 else 1.0

    if hp_ratio < 0.5:
        return 0  # Rest (heal)
    else:
        return 1  # Smith (upgrade)


# ----------------------------------------------------------------
# Utility functions
# ----------------------------------------------------------------


def _reconnect_with_retry(
    client: STS2GameClient, max_retries: int = 10, delay: float = 3.0
) -> None:
    """Attempt to reconnect to the game server with retries.

    Args:
        client: The game client to reconnect.
        max_retries: Maximum reconnection attempts.
        delay: Seconds between attempts.
    """
    for attempt in range(1, max_retries + 1):
        try:
            logger.info("Reconnect attempt %d/%d...", attempt, max_retries)
            client.reconnect()
            logger.info("Reconnected successfully.")
            return
        except ConnectionError:
            if attempt < max_retries:
                time.sleep(delay)
            else:
                logger.error("Failed to reconnect after %d attempts. Exiting.", max_retries)
                sys.exit(1)


def _log_combat_action(
    state: dict[str, Any], action_int: int, decoded: dict[str, Any]
) -> None:
    """Log a combat action with context for debugging."""
    combat = state.get("combat_state") or state
    player = combat.get("player", {})
    hand = combat.get("hand", [])
    enemies = combat.get("enemies", [])

    if decoded["type"] == "END_TURN":
        logger.info(
            "COMBAT [HP:%d/%d E:%d] -> END_TURN (round %d)",
            player.get("hp", 0),
            player.get("max_hp", 0),
            player.get("energy", 0),
            combat.get("round", 0),
        )
    else:
        ci = decoded.get("card_index", -1)
        ti = decoded.get("target_index", -1)
        card_name = hand[ci].get("id", "?") if ci < len(hand) else "?"
        target_name = enemies[ti].get("id", "?") if 0 <= ti < len(enemies) else "N/A"
        logger.info(
            "COMBAT [HP:%d/%d E:%d] -> PLAY %s (idx=%d) -> %s (idx=%d)",
            player.get("hp", 0),
            player.get("max_hp", 0),
            player.get("energy", 0),
            card_name, ci,
            target_name, ti,
        )


# ----------------------------------------------------------------
# CLI entry point
# ----------------------------------------------------------------


def main() -> None:
    """CLI entry point for the agent runner."""
    parser = argparse.ArgumentParser(
        description="Run a trained RL agent on the real STS2 game.",
        formatter_class=argparse.ArgumentDefaultsHelpFormatter,
    )
    parser.add_argument(
        "--model-path",
        required=True,
        help="Path to the trained MaskablePPO model (.zip file).",
    )
    parser.add_argument(
        "--host",
        default="127.0.0.1",
        help="Bridge server hostname.",
    )
    parser.add_argument(
        "--port",
        type=int,
        default=9002,
        help="Bridge server port.",
    )
    parser.add_argument(
        "--deterministic",
        action="store_true",
        default=True,
        help="Use deterministic action selection (no exploration).",
    )
    parser.add_argument(
        "--stochastic",
        action="store_true",
        default=False,
        help="Use stochastic action selection (for diversity).",
    )
    parser.add_argument(
        "--verbose", "-v",
        action="store_true",
        default=False,
        help="Log every action taken.",
    )
    parser.add_argument(
        "--log-level",
        default="INFO",
        choices=["DEBUG", "INFO", "WARNING", "ERROR"],
        help="Logging level.",
    )

    args = parser.parse_args()

    logging.basicConfig(
        level=getattr(logging, args.log_level),
        format="%(asctime)s [%(name)s] %(levelname)s: %(message)s",
        datefmt="%H:%M:%S",
    )

    deterministic = not args.stochastic

    run_agent(
        model_path=args.model_path,
        host=args.host,
        port=args.port,
        deterministic=deterministic,
        verbose=args.verbose,
    )


if __name__ == "__main__":
    main()
