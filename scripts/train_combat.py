"""Train a MaskablePPO agent on STS2 combat.

Usage:
    pip install "sts2-rl-agent[train]"
    python scripts/train_combat.py

Requires: stable-baselines3, sb3-contrib, torch
"""

from __future__ import annotations

import argparse
import os
import sys
import time
from pathlib import Path

import numpy as np


def make_env(seed: int = 0):
    """Create a single STS2CombatEnv."""
    from sts2_env.gym_env.combat_env import STS2CombatEnv

    def _init():
        env = STS2CombatEnv()
        env.reset(seed=seed)
        return env

    return _init


def train(args):
    try:
        from sb3_contrib import MaskablePPO
        from sb3_contrib.common.wrappers import ActionMasker
        from sb3_contrib.common.maskable.utils import get_action_masks
        from stable_baselines3.common.vec_env import SubprocVecEnv, DummyVecEnv
        from stable_baselines3.common.callbacks import EvalCallback
    except ImportError:
        print("Training requires sb3-contrib and stable-baselines3.")
        print("Install with: pip install 'sts2-rl-agent[train]'")
        sys.exit(1)

    from sts2_env.gym_env.combat_env import STS2CombatEnv

    print(f"Training MaskablePPO on STS2 combat")
    print(f"  n_envs:          {args.n_envs}")
    print(f"  total_timesteps: {args.total_timesteps}")
    print(f"  learning_rate:   {args.lr}")
    print(f"  batch_size:      {args.batch_size}")
    print(f"  output_dir:      {args.output_dir}")
    print()

    output_dir = Path(args.output_dir)
    output_dir.mkdir(parents=True, exist_ok=True)

    # Wrap env with action masker
    def mask_fn(env):
        return env.action_masks()

    def make_masked_env(seed: int):
        def _init():
            env = STS2CombatEnv()
            env = ActionMasker(env, mask_fn)
            return env
        return _init

    # Create vectorized envs
    if args.n_envs > 1:
        train_env = SubprocVecEnv([make_masked_env(i) for i in range(args.n_envs)])
    else:
        train_env = DummyVecEnv([make_masked_env(0)])

    # Eval env (always single)
    eval_env = DummyVecEnv([make_masked_env(9999)])

    # Create model
    model = MaskablePPO(
        "MlpPolicy",
        train_env,
        learning_rate=args.lr,
        n_steps=args.n_steps,
        batch_size=args.batch_size,
        n_epochs=args.n_epochs,
        gamma=args.gamma,
        gae_lambda=0.95,
        clip_range=0.2,
        ent_coef=args.ent_coef,
        verbose=1,
        tensorboard_log=str(output_dir / "tb_logs"),
    )

    # Eval callback
    eval_callback = EvalCallback(
        eval_env,
        best_model_save_path=str(output_dir / "best_model"),
        log_path=str(output_dir / "eval_logs"),
        eval_freq=max(args.eval_freq // args.n_envs, 1),
        n_eval_episodes=args.eval_episodes,
        deterministic=False,
    )

    # Train
    start = time.perf_counter()
    model.learn(
        total_timesteps=args.total_timesteps,
        callback=eval_callback,
        progress_bar=True,
    )
    elapsed = time.perf_counter() - start

    # Save final model
    final_path = str(output_dir / "final_model")
    model.save(final_path)
    print(f"\nTraining complete in {elapsed:.1f}s")
    print(f"Final model saved to: {final_path}")
    print(f"Best model saved to: {output_dir / 'best_model'}")

    # Quick evaluation
    print("\n--- Final Evaluation ---")
    evaluate(model, n_episodes=100)

    train_env.close()
    eval_env.close()


def evaluate(model, n_episodes: int = 100):
    """Evaluate trained model."""
    from sb3_contrib.common.wrappers import ActionMasker
    from sts2_env.gym_env.combat_env import STS2CombatEnv

    def mask_fn(env):
        return env.action_masks()

    env = ActionMasker(STS2CombatEnv(), mask_fn)
    wins = 0
    total_rewards = []

    for ep in range(n_episodes):
        obs, info = env.reset(seed=ep + 10000)
        done = False
        ep_reward = 0.0
        while not done:
            masks = env.action_masks()
            action, _ = model.predict(obs, action_masks=masks, deterministic=True)
            obs, reward, terminated, truncated, info = env.step(int(action))
            ep_reward += reward
            done = terminated or truncated
            if terminated and reward > 0:
                wins += 1
        total_rewards.append(ep_reward)

    print(f"Episodes:    {n_episodes}")
    print(f"Win rate:    {wins / n_episodes:.1%}")
    print(f"Avg reward:  {np.mean(total_rewards):.3f}")


def main():
    parser = argparse.ArgumentParser(description="Train MaskablePPO on STS2 combat")
    parser.add_argument("--total-timesteps", type=int, default=500_000,
                        help="Total training timesteps (default: 500000)")
    parser.add_argument("--n-envs", type=int, default=4,
                        help="Number of parallel environments (default: 4)")
    parser.add_argument("--lr", type=float, default=3e-4,
                        help="Learning rate (default: 3e-4)")
    parser.add_argument("--batch-size", type=int, default=256,
                        help="Minibatch size (default: 256)")
    parser.add_argument("--n-steps", type=int, default=2048,
                        help="Steps per rollout per env (default: 2048)")
    parser.add_argument("--n-epochs", type=int, default=10,
                        help="PPO epochs per update (default: 10)")
    parser.add_argument("--gamma", type=float, default=0.99,
                        help="Discount factor (default: 0.99)")
    parser.add_argument("--ent-coef", type=float, default=0.01,
                        help="Entropy coefficient (default: 0.01)")
    parser.add_argument("--eval-freq", type=int, default=10_000,
                        help="Evaluate every N steps (default: 10000)")
    parser.add_argument("--eval-episodes", type=int, default=20,
                        help="Episodes per evaluation (default: 20)")
    parser.add_argument("--output-dir", type=str, default="output/combat_ppo",
                        help="Output directory (default: output/combat_ppo)")
    args = parser.parse_args()
    train(args)


if __name__ == "__main__":
    main()
