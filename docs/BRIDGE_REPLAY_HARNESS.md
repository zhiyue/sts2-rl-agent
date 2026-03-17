# Bridge Replay Harness

This harness adds a lightweight golden-comparison loop for parity work.

It is intended for this workflow:

1. Run the real game through the TCP bridge.
2. Record bridge states and chosen actions into a replay JSON file.
3. Recreate the same combat in the simulator.
4. Replay the recorded actions and compare each resulting state snapshot.

The current comparator supports:

- combat-only traces through `CombatState`
- partial run traces through `RunManager`

Supported bridge message types:

- `combat_action`
- `card_select`
- `map_select`
- `card_reward`

It does not yet compare map, shop, event, or post-combat reward flows.

## Location

The harness lives in:

- [`sts2_env/parity/bridge_replay.py`](../sts2_env/parity/bridge_replay.py)

Key exports:

- `BridgeReplayRecorder`
- `BridgeReplayTrace`
- `save_replay_trace()`
- `load_replay_trace()`
- `combat_state_to_bridge_state()`
- `run_manager_to_bridge_state()`
- `compare_combat_replay()`
- `compare_run_replay()`

## Replay Format

Replay files are JSON with this structure:

```json
{
  "version": 1,
  "mode": "combat",
  "metadata": {
    "scenario_factory": "my_module:make_combat"
  },
  "initial_state": { "...": "bridge-like state" },
  "steps": [
    {
      "action": { "action": "play", "card_index": 0, "target_index": 0 },
      "resulting_state": { "...": "bridge-like state" }
    }
  ]
}
```

`initial_state` is the first actionable state seen from the real game.

Each step stores:

- the action sent to the game
- the next state observed after that action resolves

## Recording From a Real Bridge Session

Wrap the existing bridge client:

```python
from sts2_env.bridge.client import STS2GameClient
from sts2_env.parity.bridge_replay import BridgeReplayRecorder

client = STS2GameClient()
client.connect()
recorder = BridgeReplayRecorder(
    client,
    metadata={"scenario_factory": "my_module:make_combat"},
)

state = recorder.receive_state()
recorder.play_card(0, 0)
state = recorder.receive_state()

recorder.save("artifacts/my_trace.json")
```

The recorder only persists states relevant to the comparator:

- `combat_action`
- `card_select`
- `map_select`
- `card_reward`

You can also record directly from the existing bridge runner:

```bash
python -m sts2_env.bridge.agent_runner \
  --model-path models/combat_ppo.zip \
  --record-replay artifacts/run_trace.json \
  --replay-factory my_module:make_run_manager
```

The recorded trace will be written when the run ends or when the runner exits.

## Comparing Against the Simulator

Build a deterministic factory that recreates the same combat setup:

```python
from sts2_env.parity.bridge_replay import compare_combat_replay

result = compare_combat_replay(
    "artifacts/my_trace.json",
    factory="my_module:make_combat",
)

if not result.success:
    for mismatch in result.mismatches:
        print(mismatch)
```

For run-level traces:

```python
from sts2_env.parity.bridge_replay import compare_run_replay

result = compare_run_replay(
    "artifacts/map_or_reward_trace.json",
    factory="my_module:make_run_manager",
)
```

There is also a CLI helper:

```bash
python -m sts2_env.parity.bridge_replay_cli show artifacts/run_trace.json

python -m sts2_env.parity.bridge_replay_cli compare \
  artifacts/run_trace.json \
  --mode run \
  --factory my_module:make_run_manager
```

`compare_combat_replay()`:

- compares the simulator's initial state to the recorded initial state
- replays each recorded action in order
- compares each resulting simulator state to the recorded state
- stops at the first mismatch and returns a diff list

## What Gets Compared

For `combat_action`, the comparator checks:

- player HP / max HP / block / energy / max energy / powers
- hand order and card info (`id`, `cost`, `type`, `target`, `playable`, `upgraded`, `base_damage`, `base_block`)
- enemy order and combat info (`id`, `hp`, `max_hp`, `block`, `is_alive`, `intent`, `intent_damage`, `intent_hits`, `powers`)
- draw / discard / exhaust counts
- round number

For `card_select`, it checks:

- card list
- `min_select`
- `max_select`

For `map_select`, it checks:

- node order
- node type
- row / col
- floor
- act

For `card_reward`, it checks:

- offered card list
- card order and metadata
- `can_skip`

## Current Limitations

- No full-run replay comparison yet.
- Run comparison currently targets `map_select` and `card_reward` only.
- No passive comparison for shop, rest, treasure, or event screens yet.
- The trace must be paired with a deterministic simulator factory that recreates the same combat or run setup.

This is deliberate: the goal is to give parity work a reusable combat golden harness now, without blocking on a full run-state serializer.
