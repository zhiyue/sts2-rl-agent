"""Relic system for STS2 simulator."""
from sts2_env.relics.base import RelicInstance, RelicId, RelicPool
from sts2_env.relics.registry import RELIC_REGISTRY, create_relic

__all__ = [
    "RelicInstance",
    "RelicId",
    "RelicPool",
    "RELIC_REGISTRY",
    "create_relic",
]
