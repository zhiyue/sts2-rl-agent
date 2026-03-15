"""Benchmark: measure combats per second."""

import time
import numpy as np
from sts2_env.gym_env.combat_env import STS2CombatEnv


def benchmark(n_episodes: int = 1000):
    env = STS2CombatEnv()
    rng = np.random.RandomState(42)

    total_steps = 0
    wins = 0
    losses = 0

    start = time.perf_counter()

    for seed in range(n_episodes):
        obs, info = env.reset(seed=seed)
        done = False
        while not done:
            mask = info["action_mask"]
            valid = np.where(mask == 1)[0]
            action = int(rng.choice(valid))
            obs, reward, terminated, truncated, info = env.step(action)
            total_steps += 1
            done = terminated or truncated
            if terminated:
                if reward > 0:
                    wins += 1
                else:
                    losses += 1

    elapsed = time.perf_counter() - start

    print(f"Episodes:       {n_episodes}")
    print(f"Total steps:    {total_steps}")
    print(f"Time:           {elapsed:.2f}s")
    print(f"Episodes/sec:   {n_episodes / elapsed:.0f}")
    print(f"Steps/sec:      {total_steps / elapsed:.0f}")
    print(f"Avg steps/ep:   {total_steps / n_episodes:.1f}")
    print(f"Win rate:       {wins / n_episodes:.1%}")
    print(f"Wins: {wins}, Losses: {losses}, Truncated: {n_episodes - wins - losses}")


if __name__ == "__main__":
    benchmark()
