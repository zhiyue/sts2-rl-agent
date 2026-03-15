# STS2 Bridge Mod

C# mod for Slay the Spire 2 that exposes game state over TCP and accepts action commands from an RL agent.

## Architecture

```
STS2 Game Process (Godot + .NET 9)          Python Agent Process
+-----------------------------+   TCP   +------------------+
|  STS2BridgeMod.dll          |<------->|  bridge/client   |
|                             |  JSON   |                  |
|  TcpListener (port 9002)    | --state>|  state_adapter   |
|  Harmony Hooks              |         |  agent_runner    |
|  Speed Patches (5-10x)      | <-action|                  |
+-----------------------------+         +------------------+
```

## Building

### Prerequisites

- .NET 9.0 SDK
- Slay the Spire 2 installed (Steam)

### Build

Set the game path and build:

```bash
# Windows
dotnet build -p:STS2GamePath="C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2"

# Or set environment variable
set STS2GamePath=C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2
dotnet build
```

### Install

Copy the built DLL and manifest to the game's mods folder:

```
Slay the Spire 2/
  mods/
    STS2BridgeMod/
      STS2BridgeMod.dll        # From bin/Debug/net9.0/
      STS2BridgeMod.pck        # Minimal Godot resource pack (can be empty)
      mod_manifest.json         # From this directory
```

Create a minimal `.pck` file (Godot resource pack). The mod loader requires it but we don't use Godot resources:

```bash
# Create an empty .pck (just the header)
python -c "
with open('STS2BridgeMod.pck', 'wb') as f:
    f.write(b'GDPC')
    f.write((1).to_bytes(4, 'little'))  # version
    f.write((4).to_bytes(4, 'little'))  # major
    f.write((3).to_bytes(4, 'little'))  # minor
    f.write((0).to_bytes(4, 'little'))  # patch
    f.write(bytes(16 * 4))             # reserved
    f.write((0).to_bytes(4, 'little'))  # file count
"
```

## Protocol

Communication uses newline-delimited JSON over TCP on port 9002.

### Game -> Agent (State Messages)

Sent when the game is idle and waiting for player input:

```json
{
    "type": "game_state",
    "phase": "COMBAT_PLAY",
    "combat_state": {
        "player": {
            "hp": 70, "max_hp": 80, "block": 5,
            "energy": 3, "max_energy": 3,
            "powers": [{"id": "STRENGTH", "amount": 2}]
        },
        "hand": [
            {"id": "STRIKE_IRONCLAD", "cost": 1, "type": "Attack",
             "target": "AnyEnemy", "upgraded": false,
             "base_damage": 6, "base_block": 0}
        ],
        "draw_pile_count": 5,
        "discard_pile_count": 2,
        "exhaust_pile_count": 0,
        "enemies": [
            {"id": "NIBBIT", "hp": 35, "max_hp": 44, "block": 0,
             "is_alive": true, "intent": "ATTACK",
             "intent_damage": 12, "intent_hits": 1,
             "powers": []}
        ],
        "round": 2
    },
    "available_actions": ["PLAY", "END_TURN", "POTION"],
    "run_state": {
        "floor": 5, "act": 1, "gold": 120,
        "deck": ["STRIKE_IRONCLAD", "DEFEND_IRONCLAD", "BASH"],
        "relics": ["BURNING_BLOOD"],
        "potions": [{"id": "FIRE_POTION", "can_use": true, "requires_target": true}]
    }
}
```

### Agent -> Game (Action Messages)

```json
{"type": "PLAY", "card_index": 0, "target_index": 0}
{"type": "END_TURN"}
{"type": "CHOOSE", "choice_index": 1}
{"type": "POTION", "slot": 0, "target_index": 0}
{"type": "PING"}
```

### Game Phases

| Phase | Description | Valid Actions |
|-------|-------------|---------------|
| COMBAT_PLAY | Player's turn in combat | PLAY, END_TURN, POTION |
| COMBAT_WAITING | Enemy turn / animations | (wait) |
| MAP_SELECT | Selecting next map node | CHOOSE |
| CARD_REWARD | Picking a card reward | CHOOSE |
| SHOP | In the shop | CHOOSE |
| REST | At a rest site | CHOOSE |
| EVENT | In an event | CHOOSE |

## Speed Patches

The mod includes Harmony patches for game acceleration:

| Patch | Target | Effect |
|-------|--------|--------|
| WaitSpeedPatch | Cmd.CustomScaledWait | 10x faster delays |
| AnimationSpeedPatch | MegaAnimationState.SetTimeScale | 5x faster animations |
| CardAnimSpeedPatch | Card movement helpers | ~7x faster card draws |

Speed can be adjusted at runtime via the Python client.

## Files

| File | Purpose |
|------|---------|
| BridgeMod.cs | Entry point, Harmony init, TCP server start |
| BridgeServer.cs | TCP listener, JSON message handling |
| StateSerializer.cs | Game state to JSON conversion |
| ActionInjector.cs | JSON action to game action injection |
| SpeedPatches.cs | Harmony patches for acceleration |
| StabilityDetector.cs | Idle detection (ActionQueue empty) |
| mod_manifest.json | Mod metadata for STS2 mod loader |

## Troubleshooting

1. **Mod not loading**: Check that `mod_manifest.json` and `.pck` file are present alongside the DLL.
2. **TCP connection refused**: The mod only listens on localhost (127.0.0.1). Make sure port 9002 is free.
3. **Actions not executing**: Actions must run on the Godot main thread. Check game logs for `[ActionInjector]` messages.
4. **Game updated**: After STS2 updates, Harmony patches may need adjustment. Check for renamed/moved methods.
5. **Mac ARM64**: The bundled `0Harmony.dll` may have issues on Apple Silicon. Use a patched version.
