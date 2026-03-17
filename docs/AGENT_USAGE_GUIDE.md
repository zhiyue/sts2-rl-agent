# Agent Usage Guide

This guide covers training RL agents and running them against the real Slay the Spire 2 game.

---

## 1. Training a Combat Agent

The combat environment (`STS2CombatEnv`) trains an agent on single combat encounters. The agent learns to play cards, manage energy, and defeat monsters.

### Basic Training

```bash
pip install -e ".[train]"
python scripts/train_combat.py
```

### Advanced Training

```bash
python scripts/train_combat.py \
    --total-timesteps 2000000 \
    --n-envs 8 \
    --lr 3e-4 \
    --batch-size 512 \
    --n-steps 2048 \
    --ent-coef 0.01 \
    --eval-freq 10000 \
    --eval-episodes 20 \
    --output-dir output/combat_ppo_gpu
```

### What Happens During Training

1. The script creates `--n-envs` parallel combat environments.
2. Each environment randomly selects an Act 1 encounter and creates an Ironclad starter deck.
3. The MaskablePPO agent collects rollouts and updates its policy.
4. Every `--eval-freq` steps, the agent is evaluated on `--eval-episodes` episodes.
5. The best model (by evaluation reward) is saved to `output_dir/best_model/`.
6. The final model is saved to `output_dir/final_model/`.
7. After training, a 100-episode evaluation prints win rate and average reward.

### Training Output

```
output/combat_ppo/
  tb_logs/          # TensorBoard logs
  eval_logs/        # Evaluation results
  best_model/       # Best model checkpoint
    best_model.zip
  final_model.zip   # Final model
```

View training progress with TensorBoard:

```bash
tensorboard --logdir output/combat_ppo/tb_logs
```

### Hyperparameter Guidance

| Parameter | Recommended Range | Notes |
|-----------|------------------|-------|
| `--lr` | 1e-4 to 5e-4 | Lower for stability, higher for speed |
| `--batch-size` | 128 to 512 | Larger = more stable gradients |
| `--n-steps` | 1024 to 4096 | More steps = better advantage estimates |
| `--ent-coef` | 0.005 to 0.02 | Higher = more exploration |
| `--gamma` | 0.99 | Standard for short episodes |
| `--n-envs` | 4 to 16 | Match your CPU core count |
| `--total-timesteps` | 500K to 5M | More is generally better |

---

## 2. Training a Full-Run Agent

The full-run environment (`STS2RunEnv`) trains an agent on complete game runs, including combat, map navigation, card rewards, shop, rest sites, and events.

### Basic Training

```bash
python scripts/train_full_run.py
```

### Training with Options

```bash
python scripts/train_full_run.py \
    --total-timesteps 5000000 \
    --act-count 1 \
    --n-envs 8 \
    --lr 3e-4 \
    --gamma 0.995 \
    --ent-coef 0.02 \
    --reward-shaping \
    --output-dir output/run_ppo
```

### Key Differences from Combat Training

- **Action space is larger:** `Discrete(157)` vs combat-only `Discrete(115)`. Includes combat actions plus map choices, card rewards, boss relics, shop, rest, event, treasure, and acting-player selection.
- **Episodes are longer:** A full Act 1 run takes hundreds of steps.
- **Gamma is higher:** 0.995 (default) vs 0.99, because long-term planning matters more.
- **Entropy coefficient is higher:** 0.02 to encourage exploring different paths.
- **Reward shaping is available:** `--reward-shaping` adds small bonuses for floor progression and act completion, making the reward less sparse.

### Act Count

The `--act-count` flag controls episode length:

| Value | Description | Recommended for |
|-------|-------------|-----------------|
| 1 | Act 1 only (Overgrowth) | Initial training |
| 2 | Acts 1-2 | Intermediate |
| 3 | Full game (Acts 1-3) | Final training |

Start with `--act-count 1` and increase after the agent achieves >70% win rate.

### Random Baseline

Run a random-action baseline for comparison:

```bash
python scripts/train_full_run.py --baseline-only
```

The training script automatically runs a 50-episode random baseline before training starts.

---

## 3. Running the Benchmark

Measure simulator throughput:

```bash
python scripts/benchmark.py
```

This runs 1,000 episodes with random actions and reports:
- Episodes per second (~1,200)
- Steps per second (~28,000)
- Random-play win rate

---

## 4. Connecting to the Real Game

### Prerequisites

1. Trained model (from steps 1 or 2)
2. Bridge mod installed (see [MOD_BUILD_GUIDE.md](MOD_BUILD_GUIDE.md))
3. Slay the Spire 2 running with the mod loaded

### Starting the Agent

```bash
python -m sts2_env.bridge.agent_runner \
    --model-path output/combat_ppo/best_model/best_model.zip \
    --verbose
```

### Agent Runner Options

| Flag | Default | Description |
|------|---------|-------------|
| `--model-path` | (required) | Path to trained MaskablePPO model (.zip) |
| `--host` | 127.0.0.1 | Bridge server hostname |
| `--port` | 9002 | Bridge server port |
| `--deterministic` | True | Use greedy action selection (no randomness) |
| `--stochastic` | False | Use stochastic selection (for diversity) |
| `--verbose` / `-v` | False | Log every action taken |
| `--log-level` | INFO | Logging verbosity (DEBUG/INFO/WARNING/ERROR) |

### Step-by-Step Walkthrough

1. **Start the game.** Launch STS2 via Steam. Wait for the main menu. The mod will show "Running Modded" and start the TCP server on port 9002.

2. **Wait for AutoSlayer.** The mod automatically creates an AutoSlayer instance and waits at the main menu. You should see log messages like:
   ```
   [STS2Bridge] [RlAutoSlay] Main menu visible. Creating RL AutoSlayer...
   ```

3. **Start the agent.** In a separate terminal:
   ```bash
   python -m sts2_env.bridge.agent_runner \
       --model-path output/combat_ppo/best_model/best_model.zip \
       --verbose
   ```

4. **Watch the game play.** The agent will:
   - Start a new run with a random seed
   - Navigate the map (using simple heuristics for now)
   - Fight combats (using the trained model)
   - Pick card rewards (heuristic: skip if deck > 30 cards)
   - Use rest sites (heuristic: rest if HP < 50%, otherwise upgrade)
   - Handle events (heuristic: pick first option)

5. **Monitor output.** With `--verbose`, the agent logs every action:
   ```
   COMBAT [HP:72/80 E:3] -> PLAY BASH (idx=4) -> NIBBIT (idx=0)
   COMBAT [HP:72/80 E:1] -> PLAY STRIKE_IRONCLAD (idx=2) -> NIBBIT (idx=0)
   COMBAT [HP:72/80 E:0] -> END_TURN (round 1)
   MAP: choosing node 0
   CARD_REWARD: choosing option 0
   ```

### How the Agent Handles Different Phases

| Phase | Strategy | Source |
|-------|----------|--------|
| Combat | Trained MaskablePPO model | Model prediction |
| Map navigation | Pick first available node | Heuristic (TODO: train) |
| Card rewards | Pick first card; skip if deck > 30 | Heuristic (TODO: train) |
| Rest sites | Rest if HP < 50%, otherwise upgrade | Heuristic (TODO: train) |
| Shop | Skip | Heuristic (TODO: train) |
| Events | Pick first option | Heuristic (TODO: train) |

For a fully trained agent, use the full-run model instead of the combat-only model. The full-run model handles all phases via the trained policy.

---

## 5. Interpreting Agent Output

### Training Metrics (TensorBoard)

Key metrics to watch:

| Metric | What It Means |
|--------|---------------|
| `rollout/ep_rew_mean` | Average episode reward (should increase) |
| `rollout/ep_len_mean` | Average episode length (shorter = faster wins or faster deaths) |
| `train/loss` | Total PPO loss (should decrease then stabilize) |
| `train/entropy_loss` | Policy entropy (should decrease slowly; too fast = collapsed policy) |
| `train/approx_kl` | KL divergence between old and new policy (should stay < 0.02) |

### Evaluation Output

After training, the script prints:

```
--- Final Evaluation ---
Episodes:    100
Win rate:    92.0%
Avg reward:  0.847
```

For the full-run agent:

```
Episodes:         100
Win rate:         45.0%
Avg reward:       0.123
Avg ep length:    342.5
Avg floors:       12.3
Max floors:       17
```

### Real-Game Logs

When running against the real game, look for:

- **Connection messages:** `Connected to STS2 bridge at 127.0.0.1:9002`
- **Phase transitions:** `Step 42: type=combat_action phase=COMBAT_PLAY`
- **Combat actions:** `COMBAT [HP:65/80 E:2] -> PLAY INFLAME -> N/A`
- **Warnings:** `No valid actions! Defaulting to END_TURN.` (should be rare)
- **Game over:** `Game over! Result: win` or `Game over! Result: death`

---

## 6. Known Limitations

### Simulator vs Real Game Differences

The headless simulator closely mirrors the decompiled game logic, but some edge cases may differ:

- **Floating-point rounding:** The simulator uses Python `int(floor())` for damage; the game uses C# `(int)`. Results should be identical for positive values.
- **Hook execution order:** When multiple relics/powers modify the same value, the iteration order matters. The simulator processes them in list order, which should match the game.
- **Rare card interactions:** Some complex multi-step card effects (e.g., cards that draw cards that trigger effects) may have subtle ordering differences.

### Non-Combat Phases

The combat model only handles combat. Non-combat decisions (map, rewards, etc.) use simple heuristics. For better non-combat play, train a full-run agent.

### Characters

The default combat environment uses Ironclad. All 5 characters are implemented in the simulator, but training configurations for other characters require specifying the appropriate starter deck and encounter pool.

### Ascension

Ascension (difficulty levels) is supported by the simulator but not enabled by default in the training scripts. Pass ascension-level parameters when creating encounters to train at higher difficulties.
