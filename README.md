# STS2 RL Agent

A reinforcement learning agent for **Slay the Spire 2**, built on a high-performance headless combat simulator and a Gymnasium training environment. Includes a C# bridge mod for connecting the trained agent to the real game.

## Architecture

```
+-----------------------------------------------------------------------+
|  Headless Python Simulator (sts2_env/)                                |
|                                                                       |
|  +----------------+  +----------------+  +---------------------------+|
|  | Core Engine    |  | Game Content   |  | Gym Environments          ||
|  | combat.py      |  | 577 cards      |  | combat_env.py  (single)  ||
|  | creature.py    |  | 260 powers     |  | run_env.py     (full run)||
|  | hooks.py       |  | 121 monsters   |  | observation.py (131-dim) ||
|  | damage.py      |  | 290 relics     |  | action_space.py(61/100)  ||
|  | rng.py         |  | 63 potions     |  | reward.py                ||
|  +-------+--------+  +-------+--------+  +-----------+--------------+|
|          |                    |                        |              |
|          +--------------------+------------------------+              |
+-----------------------------------------------------------------------+
           |                                             |
           v                                             v
+---------------------+                  +--------------------------+
| Training Pipeline   |                  | Bridge to Real Game      |
| MaskablePPO (SB3)   |                  | bridge_mod_v2/ (C#/Godot)|
| train_combat.py     |------model------>| agent_runner.py (Python) |
| train_full_run.py   |                  | TCP JSON protocol        |
+---------------------+                  +--------------------------+
```

## Project Stats

| Metric | Value |
|--------|-------|
| Source files | 133 Python + C# |
| Lines of code | ~50,000 |
| Test functions | 408 |
| Cards implemented | 577 |
| Powers implemented | 260 |
| Monsters implemented | 121 |
| Relics implemented | 290 |
| Potions implemented | 63 |
| Playable characters | 5 (Ironclad, Silent, Defect, Necrobinder, Regent) |
| Simulation speed | ~1,200 combats/sec, ~28,000 steps/sec |
| Combat win rate (trained PPO) | ~92% (Act 1 Ironclad) |

## Quick Start

### Prerequisites

- **Python 3.11+** (3.12 recommended)
- **pip** (included with Python)
- For training: a CUDA-capable GPU is recommended but not required
- For real-game bridge: .NET 9 SDK, Godot 4.5.1 Mono, Slay the Spire 2 (Steam)

### Install

```bash
git clone <repo-url>
cd sts2-rl-agent

# Core simulator only
pip install -e .

# With training dependencies (PyTorch, SB3, sb3-contrib)
pip install -e ".[train]"

# With dev dependencies (pytest)
pip install -e ".[dev]"
```

### Run Benchmark

Measure simulation throughput with random actions:

```bash
python scripts/benchmark.py
```

Expected output on a modern CPU:

```
Episodes:       1000
Total steps:    28101
Time:           0.78s
Episodes/sec:   1276
Steps/sec:      28101
```

### Train a Combat Agent

Train a MaskablePPO agent on single-combat encounters:

```bash
python scripts/train_combat.py \
    --total-timesteps 500000 \
    --n-envs 4 \
    --output-dir output/combat_ppo
```

Key flags:

| Flag | Default | Description |
|------|---------|-------------|
| `--total-timesteps` | 500,000 | Total environment steps |
| `--n-envs` | 4 | Parallel environments (use CPU cores) |
| `--lr` | 3e-4 | Learning rate |
| `--batch-size` | 256 | Minibatch size |
| `--n-steps` | 2048 | Steps per rollout per env |
| `--output-dir` | output/combat_ppo | Where to save models and logs |

### Train a Full-Run Agent

Train an agent that handles an entire run (combat + map + rewards + events):

```bash
python scripts/train_full_run.py \
    --total-timesteps 1000000 \
    --act-count 1 \
    --n-envs 4 \
    --output-dir output/run_ppo
```

The `--act-count` flag controls how many acts per episode (1 = Act 1 only, 3 = full game).

### Connect to Real Game

After training, run the agent against the actual game:

1. Build and install the bridge mod (see [docs/MOD_BUILD_GUIDE.md](docs/MOD_BUILD_GUIDE.md))
2. Start Slay the Spire 2
3. Run the agent:

```bash
python -m sts2_env.bridge.agent_runner \
    --model-path output/combat_ppo/best_model/best_model.zip \
    --verbose
```

See [docs/AGENT_USAGE_GUIDE.md](docs/AGENT_USAGE_GUIDE.md) for details.

## Project Structure

```
sts2-rl-agent/
|-- pyproject.toml                 # Package config, dependencies
|-- scripts/
|   |-- benchmark.py               # Throughput benchmark
|   |-- train_combat.py            # Combat-only training
|   +-- train_full_run.py          # Full-run training
|
|-- sts2_env/                      # Python package (headless simulator)
|   |-- core/                      # Combat engine
|   |   |-- combat.py              # CombatState (turn flow, card play)
|   |   |-- creature.py            # Creature (HP, block, powers)
|   |   |-- hooks.py               # Central hook dispatch (~25 hooks)
|   |   |-- damage.py              # Damage/block calculation pipelines
|   |   |-- enums.py               # CardId, PowerId, IntentType, etc.
|   |   |-- constants.py           # Game constants from decompiled source
|   |   +-- rng.py                 # Seeded RNG
|   |
|   |-- cards/                     # Card definitions (577 cards)
|   |   |-- base.py                # CardInstance class
|   |   |-- effects.py             # 12 composable effect primitives
|   |   |-- registry.py            # Card ID -> effect dispatch
|   |   |-- ironclad.py            # Ironclad cards
|   |   |-- silent.py              # Silent cards
|   |   |-- defect.py              # Defect cards
|   |   |-- necrobinder.py         # Necrobinder cards
|   |   |-- regent.py              # Regent cards
|   |   |-- colorless.py           # Colorless cards
|   |   +-- status.py              # Status/Curse cards
|   |
|   |-- powers/                    # Status effects (260 powers)
|   |   |-- base.py                # PowerInstance base class
|   |   |-- common.py              # Core powers (Strength, Vulnerable, etc.)
|   |   |-- damage_modifiers.py    # Damage pipeline hooks
|   |   |-- block_modifiers.py     # Block pipeline hooks
|   |   |-- card_play_effects.py   # On-card-play triggers
|   |   |-- damage_reactions.py    # Thorns, reactive powers
|   |   |-- duration.py            # Tick-down / duration powers
|   |   |-- turn_effects.py        # Start/end of turn triggers
|   |   +-- monster.py             # Monster-specific powers
|   |
|   |-- monsters/                  # Monster AI (121 monsters)
|   |   |-- state_machine.py       # MoveState, RandomBranch, ConditionalBranch
|   |   |-- intents.py             # Intent types
|   |   |-- act1_weak.py           # Act 1 weak encounters
|   |   |-- act1.py                # Act 1 monsters
|   |   |-- act2.py                # Act 2 monsters
|   |   |-- act3.py                # Act 3 monsters
|   |   +-- act4.py                # Act 4 monsters
|   |
|   |-- relics/                    # Relic effects (290 relics)
|   |-- potions/                   # Potion effects (63 potions)
|   |-- orbs/                      # Orb mechanics (Defect)
|   |-- characters/                # Character starting states
|   |-- encounters/                # Encounter definitions (88 encounters)
|   |-- events/                    # Event decision trees (68 events)
|   |-- map/                       # Map generation algorithm
|   |-- run/                       # Full-run state management
|   |   |-- run_manager.py         # Run loop (map -> room -> rewards)
|   |   |-- run_state.py           # Persistent run state
|   |   |-- rewards.py             # Card/gold/potion rewards
|   |   |-- shop.py                # Shop system
|   |   |-- rest_site.py           # Rest site (heal/upgrade)
|   |   +-- events.py              # Event handler
|   |
|   |-- gym_env/                   # Gymnasium environments
|   |   |-- combat_env.py          # Single-combat env (Discrete(61))
|   |   |-- run_env.py             # Full-run env (Discrete(100))
|   |   |-- observation.py         # State -> 131-dim float32 vector
|   |   |-- action_space.py        # Action encoding + masking
|   |   +-- reward.py              # Reward shaping
|   |
|   +-- bridge/                    # Real-game connection
|       |-- client.py              # TCP client
|       |-- protocol.py            # Message types, phases
|       |-- state_adapter.py       # Game JSON -> observation vector
|       +-- agent_runner.py        # Main agent loop
|
|-- bridge_mod_v2/                 # C# Bridge Mod (Godot project)
|   |-- STS2BridgeMod.csproj       # Build config (Godot.NET.Sdk/4.5.1)
|   |-- MainFile.cs                # Entry point, Harmony patches
|   |-- BridgeServer.cs            # TCP server
|   |-- RlAutoSlayer.cs            # AutoSlay-based automation
|   |-- RlCombatHandler.cs         # Combat decision handler
|   |-- RlMapHandler.cs            # Map navigation handler
|   |-- RlCardSelector.cs          # Card selection handler
|   +-- RlCardRewardScreenHandler.cs
|
|-- bridge_mod/                    # Legacy bridge mod (v1, TCP + hooks)
|-- tests/                         # 14 test files, 408 test functions
|-- docs/                          # Documentation
|   |-- GAME_BRIDGE_REFERENCE.md   # Bridge architecture and protocol
|   |-- AUTOSLAY_BRIDGE.md         # AutoSlay-based bridge design
|   |-- GAME_SYSTEMS_REFERENCE.md  # Game mechanics reference
|   |-- CARDS_REFERENCE.md         # All 577 cards
|   |-- POWERS_REFERENCE.md        # All 260 powers
|   |-- MONSTERS_REFERENCE.md      # All 121 monsters
|   +-- RELICS_REFERENCE.md        # All 290 relics
|
|-- RESEARCH.md                    # Research notes and prior work
+-- DECOMPILED_ARCHITECTURE.md     # Decompiled C# architecture analysis
```

## Game Content Coverage

| Content Type | Game Total | Implemented | Coverage |
|-------------|-----------|-------------|----------|
| Cards | 577 | 577 | 100% |
| Powers (status effects) | 260 | 260 | 100% |
| Monsters | 121 | 121 | 100% |
| Relics | 290 | 290 | 100% |
| Potions | 63 | 63 | 100% |
| Encounters | 88 | 88 | 100% |
| Events | 68 | 68 | 100% |
| Characters | 5 + 2 | 5 | 100% (playable) |
| Acts | 4 | 4 | 100% |

## How It Works

### Two-Phase Approach

Following lessons from the STS1 RL community, this project uses a two-phase strategy:

1. **Headless Simulator** (for training): A pure-Python reimplementation of STS2 combat and run mechanics, verified against the decompiled C# source. Runs at ~1,200 combats/second -- fast enough for millions of training episodes.

2. **Bridge Mod** (for validation): A C# mod that hooks into the real game via Harmony, exposes state over TCP, and injects agent decisions. Includes 5-10x speed patches for faster real-game evaluation.

### RL Algorithm

- **MaskablePPO** from sb3-contrib (Stable Baselines 3)
- **Invalid action masking**: Each step, the environment provides a boolean mask indicating which actions are legal (playable cards, valid targets). Illegal actions are zeroed out before policy sampling.
- **Observation**: 131-dimensional float32 vector encoding player state, hand cards, pile summaries, and enemy state
- **Action space**: Discrete(61) for combat (end turn + 10 self-target + 50 targeted), Discrete(100) for full run

### Reward Design

**Combat environment:**
- Win: +1.0
- Loss: -1.0
- HP loss: small negative penalty (encourages efficient play)

**Full-run environment:**
- Win the run: +1.0
- Death: -1.0
- Optional shaping: small bonuses for floor progression and act completion

## Documentation

| Document | Description |
|----------|-------------|
| [README.md](README.md) | This file |
| [RESEARCH.md](RESEARCH.md) | Research notes, prior work, algorithm selection |
| [DECOMPILED_ARCHITECTURE.md](DECOMPILED_ARCHITECTURE.md) | Decompiled C# analysis for simulator |
| [CONTRIBUTING.md](CONTRIBUTING.md) | Contribution guide, dev setup, adding content |
| [docs/SIMULATOR_ARCHITECTURE.md](docs/SIMULATOR_ARCHITECTURE.md) | Python simulator internal architecture |
| [docs/TRAINING_GUIDE.md](docs/TRAINING_GUIDE.md) | Comprehensive RL training guide |
| [docs/PROTOCOL.md](docs/PROTOCOL.md) | TCP bridge communication protocol |
| [docs/KNOWN_ISSUES.md](docs/KNOWN_ISSUES.md) | Current known issues and limitations |
| [docs/MOD_BUILD_GUIDE.md](docs/MOD_BUILD_GUIDE.md) | How to build and install the bridge mod |
| [docs/TROUBLESHOOTING.md](docs/TROUBLESHOOTING.md) | Common problems and solutions |
| [docs/GAME_BRIDGE_REFERENCE.md](docs/GAME_BRIDGE_REFERENCE.md) | Bridge architecture and design notes |
| [docs/AUTOSLAY_BRIDGE.md](docs/AUTOSLAY_BRIDGE.md) | AutoSlay-based bridge design |
| [docs/GAME_SYSTEMS_REFERENCE.md](docs/GAME_SYSTEMS_REFERENCE.md) | Game mechanics reference |

## License

This project is for research and educational purposes. Slay the Spire 2 is the property of Mega Crit Games.

## Acknowledgments

- [decapitate-the-spire](https://github.com/jahabrewer/decapitate-the-spire) -- STS1 headless simulator, architectural inspiration
- [spire-codex](https://github.com/ptrlrd/spire-codex) -- STS2 data extraction pipeline
- [CommunicationMod](https://github.com/ForgottenArbiter/CommunicationMod) -- STS1 game bridge protocol design
- [BaseLib-StS2](https://github.com/Alchyr/BaseLib-StS2) -- STS2 mod framework
- [Stable Baselines 3](https://github.com/DLR-RM/stable-baselines3) -- RL training framework
