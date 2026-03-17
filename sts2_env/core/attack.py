"""Attack command context used by the combat hook pipeline."""

from __future__ import annotations

from dataclasses import dataclass, field
from typing import TYPE_CHECKING

from sts2_env.core.enums import ValueProp

if TYPE_CHECKING:
    from sts2_env.core.creature import Creature
    from sts2_env.core.damage import DamageResult


@dataclass
class AttackContext:
    """Minimal attack-command analogue for hook parity."""

    attacker: Creature | None
    target: Creature | None
    damage_props: ValueProp
    model_source: object | None = None
    results: list[DamageResult] = field(default_factory=list)
