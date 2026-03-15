# Troubleshooting Guide

This document catalogs problems encountered during development and their solutions. Organized by category.

---

## Mod Loading Issues

### Game hangs at "Loading assembly DLL"

**Symptom:** The game freezes during mod loading and never reaches the main menu.

**Cause:** The mod project was created with `Microsoft.NET.Sdk` instead of `Godot.NET.Sdk/4.5.1`. STS2 expects mods to be Godot-compatible assemblies.

**Solution:** Use the official mod template:

```bash
dotnet new install Alchyr.Sts2.Templates
dotnet new alchyrsts2mod -n STS2BridgeMod --ModAuthor "yourname"
```

The template generates a `.csproj` with the correct SDK:

```xml
<Project Sdk="Godot.NET.Sdk/4.5.1">
```

Using `Microsoft.NET.Sdk` produces a standard .NET assembly that the Godot mod loader cannot handle.

---

### "Pack version unsupported: 1"

**Symptom:** Game log shows `Pack version unsupported: 1` and the mod fails to load.

**Cause:** The `.pck` file was created with an incorrect version header. Godot 4.5.1 requires PCK format version 3. A manually crafted PCK with version 1 is rejected.

**Solution:** Use `dotnet publish` with the Godot export pipeline to generate the correct PCK:

```bash
dotnet publish
```

This invokes the `GodotPublish` target in the csproj, which calls Godot's `--export-pack` to produce a version-3 PCK. Do not create the PCK manually unless you match the exact binary format.

---

### "did not supply a mod manifest"

**Symptom:** Game loads but the mod is not recognized. Log says the mod did not supply a manifest.

**Cause:** `mod_manifest.json` exists alongside the DLL but is not inside the `.pck` file. The STS2 mod loader looks for the manifest inside the PCK, not as a separate file.

**Solution:** Place `mod_manifest.json` in the project root (next to `project.godot`) so that `dotnet publish` bundles it into the PCK automatically. The manifest should look like:

```json
{
  "pck_name": "STS2BridgeMod",
  "name": "STS2 RL Bridge",
  "author": "yourname",
  "version": "1.0.0"
}
```

After publishing, verify the manifest is in the PCK by checking the Godot export log.

---

### Game crashes after clicking "Load Mods"

**Symptom:** First time running with mods installed, clicking "Load Mods" in the consent dialog causes the game to crash or hang.

**Cause:** This is normal behavior. The first time you consent to mods, the game writes a settings flag and needs to restart to properly initialize the mod loading system.

**Solution:** Restart the game after the first consent. On subsequent launches, mods load automatically without the consent dialog.

---

## TCP Communication Issues

### Python agent cannot connect (connection refused)

**Symptom:** `ConnectionRefusedError` when the Python agent tries to connect to port 9002.

**Cause:** The game takes 30+ seconds to fully load and initialize the mod's TCP server. The Python client was attempting connection before the server was ready.

**Solution:** Increase the reconnection attempts and add a delay. The `STS2GameClient` class supports this:

```python
client = STS2GameClient(
    port=9002,
    reconnect_attempts=30,  # Try 30 times
    reconnect_delay=2.0,     # 2 seconds between attempts
)
```

This gives the game up to 60 seconds to start the TCP server.

---

### Agent receives data but all values are 0

**Symptom:** The agent connects and receives JSON, but player HP, energy, hand size, and all other values are 0.

**Cause:** The Python `state_adapter.py` was accessing `state.get("combat_state")` to find combat data, but the bridge mod (v2, AutoSlay-based) sends the combat data directly in the top-level message without a `combat_state` wrapper.

**Solution:** Fall back to the top-level state if `combat_state` is not present:

```python
combat = state.get("combat_state") or state
player = combat.get("player", {})
```

This handles both the v1 protocol (nested `combat_state`) and v2 protocol (flat structure).

---

### `receive_state()` skips all messages

**Symptom:** The agent connects but `receive_state()` blocks indefinitely or returns nothing useful.

**Cause:** The `receive_state()` method was filtering for `type: "game_state"` messages, but the v2 bridge mod sends messages with `type: "combat_action"`, `type: "map_select"`, `type: "card_reward"`, etc. These were all being dropped.

**Solution:** Accept all message types except system messages (pong, error):

```python
def receive_state(self):
    while True:
        msg = self._receive_line()
        data = json.loads(msg)
        msg_type = data.get("type", "")
        if msg_type in ("pong",):
            continue
        elif msg_type == "error":
            logger.warning("Server error: %s", data)
            continue
        else:
            return data  # Accept all game messages
```

---

## Action Injection Issues

### "The given key was not present in the dictionary"

**Symptom:** C# exception in the game when the agent sends an action.

**Cause:** Protocol mismatch. The Python agent was sending `{"type": "PLAY"}` but the v2 bridge mod expects `{"action": "play"}`. The key name and casing differ.

**Solution:** Match the mod's expected protocol exactly:

```python
# Correct (v2 protocol)
client.send_action({"action": "play", "card_index": 0, "target_index": 0})

# Wrong (v1 protocol)
client.send_action({"type": "PLAY", "card_index": 0, "target_index": 0})
```

The `STS2GameClient` helper methods (`play_card()`, `end_turn()`, `choose()`) use the correct format.

---

### Agent only sends END_TURN

**Symptom:** The agent is connected and receiving state, but every action it takes is END_TURN.

**Cause:** `compute_action_mask()` was checking `state.get("phase")` to determine valid actions, but the v2 mod's messages do not include a `phase` field -- the message `type` itself indicates the phase. All card-play actions were being masked as invalid.

**Solution:** Use the same `combat_state or state` pattern in the action mask computation:

```python
def compute_action_mask(self, state):
    combat = state.get("combat_state") or state
    # Now read hand, energy, enemies from combat dict
```

---

### Energy always shows 3 after playing cards

**Symptom:** When connected to the real game, the agent plays cards but energy never decreases. It can play unlimited cards per turn.

**Cause:** The initial implementation used `CardCmd.AutoPlay()` to play cards. This API is a test/debug function that does not spend energy -- it just executes the card effect.

**Solution:** Use the proper play card pathway via the action queue:

```csharp
// Wrong: does not spend energy
CardCmd.AutoPlay(ctx, card, target);

// Correct: full play card pipeline (validates, spends energy, triggers hooks)
ActionQueueSynchronizer.RequestEnqueue(
    new PlayCardAction(card, target, playerState)
);
```

The `RlCombatHandler` in bridge_mod_v2 uses `PlayCardAction` through the AutoSlay combat handler interface, which correctly processes energy costs.

---

## Build Issues

### .NET 8 SDK cannot build net9.0 target

**Symptom:** `dotnet build` fails with error `The framework 'net9.0' is not available`.

**Cause:** The mod template targets .NET 9.0 (matching the game's runtime), but only .NET 8 SDK was installed.

**Solution:** Install .NET 9 SDK:

```powershell
# Windows
Invoke-WebRequest -Uri https://dot.net/v1/dotnet-install.ps1 -OutFile dotnet-install.ps1
./dotnet-install.ps1 -Channel 9.0

# Verify
dotnet --list-sdks
```

---

### "MegaDot not found" error during publish

**Symptom:** `dotnet publish` fails because it cannot find the Godot editor binary needed to export the PCK.

**Cause:** The `.csproj` has a `GodotPublish` target that calls Godot's `--export-pack` command. The `GodotPath` property must point to a valid Godot 4.5.1 Mono editor binary.

**Solution:** Download Godot 4.5.1 Mono from the official releases and configure the path:

1. Download from: https://github.com/godotengine/godot/releases/tag/4.5.1-stable
2. Extract to a known location (e.g., `C:/megadot/`)
3. The `.csproj` already defaults to `C:/megadot/Godot_v4.5.1-stable_mono_win64/`
4. Adjust the `GodotPath` in the csproj if your path differs

If you only need the DLL (not the PCK), `dotnet build` works without Godot installed. The PCK is only needed for mod resources.

---

## Thread Safety Issues

### "Room type not assigned" timeout after character select

**Symptom:** After the AutoSlayer selects a character and starts a run, the game times out with "Room type not assigned" and never enters the first map node.

**Cause:** `Task.Run()` was used to launch the AutoSlayer logic. `Task.Run()` executes on the .NET thread pool, not the Godot main thread. Game operations like `NGame.StartRun()` must execute on the Godot synchronization context.

**Solution:** Use `TaskHelper.RunSafely()` instead of `Task.Run()`:

```csharp
// Wrong: runs on thread pool
Task.Run(async () => {
    await NGame.Instance.StartRun(seed);
});

// Correct: runs on Godot main thread
TaskHelper.RunSafely(LaunchRunAsync());
```

`TaskHelper.RunSafely()` is a game utility that schedules async tasks on the Godot synchronization context, ensuring all game API calls happen on the main thread.

---

### NGame.StartRun throws exception

**Symptom:** `NGame.Instance.StartRun()` throws a null reference or invalid state exception.

**Cause:** Same root cause as above -- the call was executing on a background thread. Many game subsystems are only accessible from the main thread.

**Solution:** Ensure all game API calls go through `TaskHelper.RunSafely()` or `Callable.From(...).CallDeferred()`:

```csharp
// For fire-and-forget game operations
Godot.Callable.From(() => {
    // This runs on the main thread next frame
    SomeGameOperation();
}).CallDeferred();

// For async operations that need to await
TaskHelper.RunSafely(async () => {
    await WaitHelper.Until(() => NGame.Instance != null, ct);
    // Now safe to use game APIs
});
```

---

## Training Issues

### Training loss diverges or NaN

**Symptom:** PPO training shows NaN losses or reward diverges to extreme values.

**Cause:** Usually caused by too-large learning rate, too-small batch size, or observation values outside expected range.

**Solution:** Use conservative hyperparameters to start:

```bash
python scripts/train_combat.py \
    --lr 3e-4 \
    --batch-size 256 \
    --n-steps 2048 \
    --ent-coef 0.01
```

Check that observation values are properly normalized (the observation encoder divides by fixed constants like HP/max_hp, block/50, energy/10).

---

### Agent learns to always END_TURN

**Symptom:** After training, the agent ends every turn immediately without playing any cards.

**Cause:** The action mask might not be working correctly, or the reward signal is too sparse (only win/loss at episode end). With sparse reward, the agent finds END_TURN to be the "safest" action.

**Solution:**
1. Verify action masking: the mask should have card-play actions enabled when the player has energy and playable cards.
2. Add reward shaping: small negative penalty for HP loss encourages the agent to play defensively rather than passively.
3. Use entropy coefficient (`--ent-coef 0.01` or higher) to encourage exploration during training.

---

## Environment Issues

### "Must call reset() first" assertion

**Symptom:** Calling `env.step()` raises an assertion error.

**Cause:** The environment was not reset before stepping, or a previous episode ended and `reset()` was not called.

**Solution:** Always call `reset()` before the first `step()`, and call it again after each episode ends:

```python
obs, info = env.reset(seed=42)
done = False
while not done:
    action = ...
    obs, reward, terminated, truncated, info = env.step(action)
    done = terminated or truncated
obs, info = env.reset()  # Start next episode
```

---

### Observation shape mismatch

**Symptom:** Model loading fails with `observation space shape mismatch`.

**Cause:** The observation size changed between training and inference (e.g., new features were added to the observation encoder).

**Solution:** Ensure the same version of `observation.py` is used for both training and inference. The observation size is `OBS_SIZE = 131` dimensions. If you modify the observation encoder, retrain the model.
