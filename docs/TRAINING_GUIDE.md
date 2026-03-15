# Training Guide

How to train RL agents on the STS2 headless simulator.

---

## Hardware Requirements

| Component | Minimum | Recommended |
|-----------|---------|-------------|
| CPU | 4 cores | 8+ cores (for parallel envs) |
| RAM | 8 GB | 16 GB |
| GPU | Not required (CPU training works) | NVIDIA RTX 4070 Ti SUPER or better |
| Disk | 2 GB free | 10 GB (for logs + checkpoints) |
| Python | 3.11+ | 3.12 |

GPU is recommended for faster training but not strictly required. The simulator itself is CPU-bound (pure Python); the GPU accelerates only the neural network forward/backward passes in PyTorch.

**Tested hardware:** RTX 4070 Ti SUPER, 32 GB RAM, 12-core CPU. Combat training completes in ~27 minutes for 2M steps.

---

## Installation

```bash
# Clone the repository
git clone <repo-url>
cd sts2-rl-agent

# Install with training dependencies (PyTorch, SB3, sb3-contrib)
pip install -e ".[train]"

# Verify installation
python scripts/benchmark.py
```

Expected benchmark output:
```
Episodes:       1000
Total steps:    28101
Time:           0.78s
Episodes/sec:   1276
Steps/sec:      28101
```

---

## Combat-Only Training

Train an agent to play single combat encounters (Act 1 Ironclad).

### Command

```bash
python scripts/train_combat.py \
    --total-timesteps 2000000 \
    --n-envs 8 \
    --lr 3e-4 \
    --batch-size 256 \
    --n-steps 2048 \
    --output-dir output/combat_ppo
```

### Flags

| Flag | Default | Description |
|------|---------|-------------|
| `--total-timesteps` | 500,000 | Total environment steps |
| `--n-envs` | 4 | Parallel environments (set to CPU core count) |
| `--lr` | 3e-4 | Learning rate |
| `--batch-size` | 256 | Minibatch size |
| `--n-steps` | 2048 | Steps per rollout per env |
| `--n-epochs` | 10 | PPO epochs per update |
| `--gamma` | 0.99 | Discount factor |
| `--ent-coef` | 0.01 | Entropy coefficient (exploration) |
| `--eval-freq` | 10,000 | Evaluate every N steps |
| `--eval-episodes` | 20 | Episodes per evaluation |
| `--output-dir` | output/combat_ppo | Where to save models and logs |

### Expected Results

| Timesteps | Win Rate | Training Time (8 envs, GPU) |
|-----------|----------|----------------------------|
| 100k | ~70% | ~1.5 min |
| 500k | ~85% | ~7 min |
| 2M | ~92% | ~27 min |

The random baseline win rate is approximately 63.4% for Act 1 Ironclad encounters. The agent significantly outperforms random within 500k steps.

### Evaluation

After training, evaluate the saved model:

```bash
python scripts/benchmark.py  # Quick throughput check

# Or use the built-in evaluation in train_combat.py:
# The script automatically runs 100 evaluation episodes after training.
```

The training script saves two model checkpoints:
- `output/combat_ppo/final_model.zip` -- model at the end of training
- `output/combat_ppo/best_model/best_model.zip` -- best model during training (based on eval callback)

### TensorBoard

Training logs are saved for TensorBoard:

```bash
tensorboard --logdir output/combat_ppo/tb_logs
```

Key metrics to watch:
- `rollout/ep_rew_mean` -- average episode reward (should trend toward +1.0)
- `rollout/ep_len_mean` -- average episode length (should decrease as agent wins faster)
- `train/entropy_loss` -- exploration (should decrease over time but not collapse to 0)
- `train/policy_gradient_loss` -- PPO policy loss
- `eval/mean_reward` -- evaluation reward (most reliable metric)

### Hyperparameter Tuning Tips

1. **Learning rate:** 3e-4 works well. Lower (1e-4) if training is unstable. Higher (1e-3) can speed up early learning but may diverge.

2. **Entropy coefficient:** 0.01 is a good default. Increase to 0.05 if the agent gets stuck in a local optimum (always ending turn). Decrease to 0.001 once the agent has learned basic play patterns.

3. **Batch size:** 256 is optimal for combat training. Larger batches (512, 1024) can stabilize gradients but slow down updates.

4. **n_steps:** 2048 per env per rollout. Since combat episodes average ~28 steps, this gives ~73 episodes per rollout per env. Shorter values (512, 1024) give more frequent updates but higher variance.

5. **n_envs:** Set to your CPU core count for maximum throughput. The simulator is CPU-bound, so more parallel envs = more samples per second.

6. **gamma:** 0.99 for combat (short episodes). Use 0.995+ for full-run training (long episodes).

---

## Full-Run Training

Train an agent that handles an entire run: combat, map navigation, card rewards, events, shops, and rest sites.

### Command

```bash
python scripts/train_full_run.py \
    --total-timesteps 1000000 \
    --act-count 1 \
    --n-envs 4 \
    --lr 3e-4 \
    --batch-size 256 \
    --gamma 0.995 \
    --ent-coef 0.02 \
    --output-dir output/run_ppo
```

### Key Flags

| Flag | Default | Description |
|------|---------|-------------|
| `--act-count` | 1 | Acts per run (1 = Act 1 only, 3 = full game) |
| `--reward-shaping` | True | Enable floor/act completion bonuses |
| `--no-reward-shaping` | - | Sparse reward only (+1 win / -1 death) |
| `--gamma` | 0.995 | Higher discount for long episodes |
| `--ent-coef` | 0.02 | More exploration for complex decision space |
| `--baseline-only` | - | Only run random baseline (no training) |

The training script uses a larger network architecture (256x256 for both policy and value) compared to combat training (default MLP).

### Curriculum Learning

Start with Act 1 only, then expand:

```bash
# Phase 1: Act 1 only (shorter episodes, denser signal)
python scripts/train_full_run.py --act-count 1 --total-timesteps 2000000

# Phase 2: Full game (load Phase 1 model, fine-tune)
# TODO: model loading for curriculum not yet implemented
python scripts/train_full_run.py --act-count 3 --total-timesteps 5000000
```

### Expected Results

**Random baseline** (Act 1 only):
```
Win rate:         0%
Avg floors:       3.9
Max floors:       8
```

**Trained agent** (1M steps, Act 1 only):
```
Win rate:         0%
Avg floors:       8.9
Max floors:       15
```

The agent learns to progress further through Act 1 but does not yet achieve a positive win rate. Full-run training is significantly harder than combat-only due to:

1. **Sparse reward:** Only +1 for winning the entire run, -1 for death. No intermediate signal for floor progression (unless reward shaping is enabled).
2. **Long episodes:** A full Act 1 run involves 15+ floors, each with its own combat, map choice, and reward decisions. Episodes can span thousands of steps.
3. **Multi-phase action space:** The agent must learn to handle 8 different game phases with very different semantics under a single Discrete(100) action space.
4. **Compounding errors:** A bad card reward choice in floor 3 might not manifest as a loss until floor 12.

### Challenges and Mitigations

| Challenge | Current State | Future Direction |
|-----------|--------------|------------------|
| Sparse reward | Reward shaping (floor bonus) helps slightly | Better intermediate rewards, hindsight experience replay |
| Long episodes | Act-count curriculum | Hierarchical policy (meta-policy picks strategy, sub-policies execute) |
| Multi-phase | Single flat policy for all phases | Separate policies per phase, combined with a router |
| Simulation speed | ~1,200 combats/sec | Cython acceleration of core loop (target: 10k+/sec) |
| Card selection | Random/first heuristic | Dedicated card evaluation network |

---

## Training Results Summary

| Metric | Random | Combat-Trained (2M) | Run-Trained (1M) |
|--------|--------|---------------------|------------------|
| Combat win rate | 63.4% | 92% | N/A (not isolated) |
| Run win rate | 0% | N/A (combat only) | 0% |
| Avg floors (Act 1 run) | 3.9 | N/A | 8.9 |
| Training time | - | ~27 min (GPU) | ~2 hrs (GPU) |

---

## Future Improvements

1. **Hierarchical policy:** Separate the policy into a high-level strategy selector (which card to add, which path to take) and a low-level combat executor. Train them independently and combine.

2. **Cython acceleration:** Port `core/combat.py`, `core/hooks.py`, and `core/damage.py` to Cython for 5-10x speedup. The main bottleneck is Python interpreter overhead in the hot loop.

3. **Reward shaping:** Design intermediate rewards that correlate with run success:
   - HP preservation bonus after each combat
   - Deck quality score (attack/defense ratio, synergy metrics)
   - Gold efficiency metric

4. **Population-based training (PBT):** Use Ray RLlib to train multiple agents with different hyperparameters in parallel, automatically tuning learning rate, entropy, and gamma.

5. **Self-play against harder encounters:** Start with weak encounters, progressively introduce harder ones as the agent improves.

6. **Imitation learning:** If expert human replays become available, pre-train the policy with behavioral cloning before RL fine-tuning.

7. **Multi-character support:** Extend training to Silent, Defect, Necrobinder, and Regent. Each character has fundamentally different mechanics requiring adapted observation encodings.
