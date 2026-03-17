"""Deterministic RNG for reproducible combat simulations."""

from __future__ import annotations

import random


class Rng:
    """Seeded random number generator wrapping Python's random.Random."""

    def __init__(self, seed: int):
        self._rng = random.Random(seed)
        self._seed = seed

    @property
    def seed(self) -> int:
        return self._seed

    def next_int(self, low: int, high: int) -> int:
        """Return random int in [low, high] inclusive."""
        return self._rng.randint(low, high)

    def random_int(self, low: int, high: int) -> int:
        """Backward-compatible alias for ``next_int``."""
        return self.next_int(low, high)

    def next_float(self, upper: float = 1.0) -> float:
        """Return random float in [0, upper)."""
        return self._rng.random() * upper

    def shuffle(self, lst: list) -> None:
        """In-place shuffle."""
        self._rng.shuffle(lst)

    def choice(self, lst: list):
        """Pick a random element."""
        return self._rng.choice(lst)

    def sample(self, lst: list, k: int) -> list:
        """Pick k distinct elements."""
        return self._rng.sample(lst, k)

    def next_float_range(self, low: float, high: float) -> float:
        """Return random float in [low, high)."""
        return low + self._rng.random() * (high - low)

    def next_gaussian_int(self, mean: float, stddev: float, min_val: int, max_val: int) -> int:
        """Return a gaussian-distributed int in [min_val, max_val].

        Matches C# Rng.NextGaussianInt: uses rejection sampling (re-rolls
        until the result is within range) rather than clamping.
        """
        while True:
            u1 = 1.0 - self._rng.random()
            u2 = 1.0 - self._rng.random()
            import math
            z = math.sqrt(-2.0 * math.log(u1)) * math.sin(2.0 * math.pi * u2)
            val = round(mean + stddev * z)
            if min_val <= val <= max_val:
                return val

    def fork(self) -> Rng:
        """Create a child RNG with a derived seed."""
        return Rng(self._rng.randint(0, 2**31))
