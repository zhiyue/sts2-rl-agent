"""Relic registry for STS2 simulator.

Maps RelicId to relic class for instantiation. All relic modules register
themselves by importing and calling register_relic().
"""

from __future__ import annotations

import re
from typing import TYPE_CHECKING

from sts2_env.relics.base import RelicId, RelicInstance

if TYPE_CHECKING:
    pass

# Maps RelicId -> RelicInstance subclass
RELIC_REGISTRY: dict[RelicId, type[RelicInstance]] = {}
_RELICS_LOADED = False


def register_relic(relic_class: type[RelicInstance]) -> type[RelicInstance]:
    """Decorator to register a relic class in the global registry."""
    RELIC_REGISTRY[relic_class.relic_id] = relic_class
    return relic_class


def create_relic(relic_id: RelicId) -> RelicInstance:
    """Create a relic instance by id."""
    load_all_relics()
    cls = RELIC_REGISTRY.get(relic_id)
    if cls is None:
        raise KeyError(f"No relic registered for {relic_id}")
    return cls(relic_id)


def _camel_to_enum_name(name: str) -> str:
    normalized = re.sub(r"[^A-Za-z0-9]", "", name)
    if not normalized:
        return name
    return re.sub(r"(?<!^)(?=[A-Z])", "_", normalized).upper()


def coerce_relic_id(value: str | RelicId) -> RelicId:
    """Resolve either a `RelicId` or a CamelCase / enum-style string."""
    load_all_relics()
    if isinstance(value, RelicId):
        return value

    candidates = {
        value,
        value.upper(),
        _camel_to_enum_name(value),
    }
    for candidate in candidates:
        if candidate in RelicId.__members__:
            return RelicId[candidate]

    for relic_id, relic_cls in RELIC_REGISTRY.items():
        if value == relic_cls.__name__ or value.upper() == relic_cls.__name__.upper():
            return relic_id

    raise KeyError(f"Unknown relic id '{value}'")


def create_relic_by_name(value: str | RelicId) -> RelicInstance:
    """Create a relic instance from either enum id or serialized string id."""
    return create_relic(coerce_relic_id(value))


def load_all_relics() -> None:
    """Import all relic modules to trigger registration."""
    global _RELICS_LOADED
    if _RELICS_LOADED:
        return
    import sts2_env.relics.starter  # noqa: F401
    import sts2_env.relics.common  # noqa: F401
    import sts2_env.relics.uncommon  # noqa: F401
    import sts2_env.relics.rare  # noqa: F401
    import sts2_env.relics.shop_event  # noqa: F401
    _RELICS_LOADED = True
