"""TCP client for communicating with the STS2 Bridge Mod.

Connects to the game's TCP server (default: localhost:9002) and provides
methods for receiving game state and sending actions. Uses newline-delimited
JSON protocol matching BridgeServer.cs.

Features:
  - Automatic reconnection with configurable retry count
  - Error recovery (partial reads, broken connections)
  - Type-safe helper methods for each action type
  - Configurable timeout for state reception
"""

from __future__ import annotations

import json
import logging
import socket
import time
from typing import Any

from sts2_env.bridge.protocol import (
    DEFAULT_HOST,
    DEFAULT_PORT,
    ActionType,
    MSG_TYPE_GAME_STATE,
)

logger = logging.getLogger(__name__)


class STS2GameClient:
    """TCP client for the STS2 Bridge Mod.

    Usage::

        client = STS2GameClient()
        client.connect()

        while True:
            state = client.receive_state()
            if state["phase"] == "COMBAT_PLAY":
                client.play_card(0, 0)
            else:
                client.end_turn()
    """

    def __init__(
        self,
        host: str = DEFAULT_HOST,
        port: int = DEFAULT_PORT,
        timeout: float = 60.0,
        reconnect_attempts: int = 30,
        reconnect_delay: float = 2.0,
    ):
        """Initialize the client.

        Args:
            host: Game server hostname (default: 127.0.0.1).
            port: Game server port (default: 9002).
            timeout: Socket timeout in seconds for receive operations.
            reconnect_attempts: Number of reconnection attempts before giving up.
            reconnect_delay: Seconds to wait between reconnection attempts.
        """
        self.host = host
        self.port = port
        self.timeout = timeout
        self.reconnect_attempts = reconnect_attempts
        self.reconnect_delay = reconnect_delay

        self._sock: socket.socket | None = None
        self._buffer: bytes = b""
        self._connected: bool = False
        self._last_request_id: str | None = None

    # ----------------------------------------------------------------
    # Connection management
    # ----------------------------------------------------------------

    def connect(self) -> None:
        """Connect to the game's bridge server.

        Retries up to ``reconnect_attempts`` times if the initial
        connection fails (the game may not have started the server yet).

        Raises:
            ConnectionError: If all connection attempts fail.
        """
        for attempt in range(1, self.reconnect_attempts + 1):
            try:
                self._sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
                self._sock.settimeout(self.timeout)
                self._sock.connect((self.host, self.port))
                self._buffer = b""
                self._connected = True
                logger.info(
                    "Connected to STS2 bridge at %s:%d (attempt %d)",
                    self.host, self.port, attempt,
                )
                return
            except (ConnectionRefusedError, OSError) as e:
                logger.warning(
                    "Connection attempt %d/%d failed: %s",
                    attempt, self.reconnect_attempts, e,
                )
                self._cleanup_socket()
                if attempt < self.reconnect_attempts:
                    time.sleep(self.reconnect_delay)

        raise ConnectionError(
            f"Could not connect to STS2 bridge at {self.host}:{self.port} "
            f"after {self.reconnect_attempts} attempts"
        )

    def disconnect(self) -> None:
        """Disconnect from the game server."""
        self._cleanup_socket()
        self._connected = False
        logger.info("Disconnected from STS2 bridge.")

    def reconnect(self) -> None:
        """Disconnect and reconnect."""
        self.disconnect()
        self.connect()

    @property
    def connected(self) -> bool:
        """Whether the client is currently connected."""
        return self._connected

    # ----------------------------------------------------------------
    # Message I/O
    # ----------------------------------------------------------------

    def receive_state(self) -> dict[str, Any]:
        """Block until a game state message is received.

        Reads newline-delimited JSON from the socket. Skips non-state
        messages (e.g., pong responses).

        Returns:
            Parsed game state dictionary with keys: type, phase,
            combat_state, run_state, available_actions.

        Raises:
            ConnectionError: If the connection is lost.
            TimeoutError: If no message is received within the timeout.
        """
        while True:
            msg = self._receive_line()
            if msg is None:
                raise ConnectionError("Connection lost while waiting for state")

            try:
                data = json.loads(msg)
            except json.JSONDecodeError as e:
                logger.warning("Invalid JSON from server: %s (raw: %s)", e, msg[:200])
                continue

            msg_type = data.get("type", "")
            if msg_type in ("pong",):
                logger.debug("Received pong")
                continue
            elif msg_type == "error":
                logger.warning("Server error: %s", data)
                continue
            else:
                self._last_request_id = data.get("request_id")
                # Return any game message (combat_action, map_select, card_reward, etc.)
                return data

    def send_action(self, action: dict[str, Any]) -> None:
        """Send an action command to the game.

        Args:
            action: Action dictionary with at least a "type" key.

        Raises:
            ConnectionError: If the connection is lost.
        """
        if not self._connected or self._sock is None:
            raise ConnectionError("Not connected to STS2 bridge")

        try:
            payload = dict(action)
            if "request_id" not in payload and self._last_request_id is not None:
                payload["request_id"] = self._last_request_id
            data = json.dumps(payload, separators=(",", ":")).encode("utf-8") + b"\n"
            self._sock.sendall(data)
            logger.debug("Sent action: %s", payload)
            self._last_request_id = None
        except (BrokenPipeError, OSError) as e:
            self._connected = False
            raise ConnectionError(f"Lost connection while sending action: {e}") from e

    # ----------------------------------------------------------------
    # Convenience methods for each action type
    # ----------------------------------------------------------------

    def play_card(self, card_index: int, target_index: int = -1) -> None:
        """Play a card from the hand."""
        self.send_action({
            "action": "play",
            "card_index": card_index,
            "target_index": target_index,
        })

    def end_turn(self) -> None:
        """End the current turn."""
        self.send_action({"action": "end_turn"})

    def choose(self, choice_index: int) -> None:
        """Make a non-combat choice (map node, card reward, event, etc)."""
        self.send_action({
            "action": "choose",
            "index": choice_index,
        })

    def choose_many(self, indexes: list[int]) -> None:
        """Make a multi-card selection choice."""
        self.send_action({
            "action": "choose",
            "indexes": indexes,
        })

    def skip(self) -> None:
        """Skip the current choice screen when the bridge supports it."""
        self.send_action({"action": "skip"})

    def use_potion(self, slot: int, target_index: int = -1) -> None:
        """Use a potion.

        Args:
            slot: Potion slot index (0-based).
            target_index: Enemy index for targeted potions (-1 for untargeted).
        """
        self.send_action({
            "action": "potion",
            "slot": slot,
            "target_index": target_index,
        })

    def ping(self) -> bool:
        """Send a health-check ping. Returns True if pong received.

        Non-blocking check — sends PING and waits briefly for PONG.
        """
        try:
            self.send_action({"type": ActionType.PING})
            # The pong will be received and skipped by receive_state()
            return True
        except ConnectionError:
            return False

    # ----------------------------------------------------------------
    # Internal helpers
    # ----------------------------------------------------------------

    def _receive_line(self) -> str | None:
        """Read one newline-terminated line from the socket.

        Buffers partial reads and returns complete lines. Returns None
        if the connection is closed.
        """
        if self._sock is None:
            return None

        while b"\n" not in self._buffer:
            try:
                chunk = self._sock.recv(8192)
            except socket.timeout:
                raise TimeoutError(
                    f"No data received within {self.timeout}s timeout"
                )
            except OSError as e:
                self._connected = False
                logger.error("Socket error during receive: %s", e)
                return None

            if not chunk:
                # Connection closed by server
                self._connected = False
                return None

            self._buffer += chunk

        line, self._buffer = self._buffer.split(b"\n", 1)
        return line.decode("utf-8", errors="replace").strip()

    def _cleanup_socket(self) -> None:
        """Close and clean up the socket."""
        if self._sock is not None:
            try:
                self._sock.close()
            except OSError:
                pass
            self._sock = None
        self._buffer = b""

    # ----------------------------------------------------------------
    # Context manager support
    # ----------------------------------------------------------------

    def __enter__(self) -> STS2GameClient:
        self.connect()
        return self

    def __exit__(self, *args: Any) -> None:
        self.disconnect()
