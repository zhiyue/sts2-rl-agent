"""Starter relics (10 total).

BurningBlood, BlackBlood, RingOfTheSnake, RingOfTheDrake,
CrackedCore, InfusedCore, BoundPhylactery, PhylacteryUnbound,
DivineRight, DivineDestiny.
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from sts2_env.core.enums import RelicRarity, CombatSide
from sts2_env.relics.base import RelicId, RelicPool, RelicInstance
from sts2_env.relics.registry import register_relic

if TYPE_CHECKING:
    from sts2_env.core.creature import Creature
    from sts2_env.core.combat import CombatState


# ─── Ironclad Starter ───────────────────────────────────────────────────


@register_relic
class BurningBlood(RelicInstance):
    relic_id = RelicId.BURNING_BLOOD
    rarity = RelicRarity.STARTER
    pool = RelicPool.IRONCLAD
    HEAL = 6

    def after_combat_victory(self, owner: Creature, combat: CombatState) -> None:
        if owner.current_hp > 0:
            owner.heal(self.HEAL)


@register_relic
class BlackBlood(RelicInstance):
    """Upgraded BurningBlood."""
    relic_id = RelicId.BLACK_BLOOD
    rarity = RelicRarity.STARTER
    pool = RelicPool.EVENT
    HEAL = 12

    def after_combat_victory(self, owner: Creature, combat: CombatState) -> None:
        if owner.current_hp > 0:
            owner.heal(self.HEAL)


# ─── Silent Starter ─────────────────────────────────────────────────────


@register_relic
class RingOfTheSnake(RelicInstance):
    relic_id = RelicId.RING_OF_THE_SNAKE
    rarity = RelicRarity.STARTER
    pool = RelicPool.SILENT
    EXTRA_CARDS = 2

    def modify_hand_draw(self, owner: Creature, draw: int, combat: CombatState) -> int:
        if combat.round_number == 1:
            return draw + self.EXTRA_CARDS
        return draw


@register_relic
class RingOfTheDrake(RelicInstance):
    """Upgraded RingOfTheSnake."""
    relic_id = RelicId.RING_OF_THE_DRAKE
    rarity = RelicRarity.STARTER
    pool = RelicPool.EVENT
    EXTRA_CARDS = 2
    TURNS = 3

    def modify_hand_draw(self, owner: Creature, draw: int, combat: CombatState) -> int:
        if combat.round_number <= self.TURNS:
            return draw + self.EXTRA_CARDS
        return draw


# ─── Defect Starter ─────────────────────────────────────────────────────


@register_relic
class CrackedCore(RelicInstance):
    relic_id = RelicId.CRACKED_CORE
    rarity = RelicRarity.STARTER
    pool = RelicPool.DEFECT
    LIGHTNING_COUNT = 1

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and combat.round_number == 1:
            # Channel 1 Lightning orb
            combat.channel_orb(owner, "LIGHTNING")


@register_relic
class InfusedCore(RelicInstance):
    """Upgraded CrackedCore."""
    relic_id = RelicId.INFUSED_CORE
    rarity = RelicRarity.STARTER
    pool = RelicPool.EVENT
    LIGHTNING_COUNT = 3

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and combat.round_number == 1:
            for _ in range(self.LIGHTNING_COUNT):
                combat.channel_orb(owner, "LIGHTNING")


# ─── Necrobinder Starter ────────────────────────────────────────────────


@register_relic
class BoundPhylactery(RelicInstance):
    relic_id = RelicId.BOUND_PHYLACTERY
    rarity = RelicRarity.STARTER
    pool = RelicPool.NECROBINDER
    SUMMON_COUNT = 1

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        combat.summon_osty(owner, self.SUMMON_COUNT)

    def after_energy_reset(self, owner: Creature, combat: CombatState) -> None:
        if combat.round_number > 1:
            combat.summon_osty(owner, self.SUMMON_COUNT)


@register_relic
class PhylacteryUnbound(RelicInstance):
    """Upgraded BoundPhylactery."""
    relic_id = RelicId.PHYLACTERY_UNBOUND
    rarity = RelicRarity.STARTER
    pool = RelicPool.EVENT
    START_COMBAT_COUNT = 5
    START_TURN_COUNT = 2

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        combat.summon_osty(owner, self.START_COMBAT_COUNT)

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER:
            combat.summon_osty(owner, self.START_TURN_COUNT)


# ─── Regent Starter ─────────────────────────────────────────────────────


@register_relic
class DivineRight(RelicInstance):
    relic_id = RelicId.DIVINE_RIGHT
    rarity = RelicRarity.STARTER
    pool = RelicPool.REGENT
    STARS = 3

    def after_room_entered(self, owner: Creature, room_type: object) -> None:
        # Gain 3 stars when entering combat room
        if hasattr(room_type, "is_combat") and room_type.is_combat:
            owner.gain_stars(self.STARS)


@register_relic
class DivineDestiny(RelicInstance):
    """Upgraded DivineRight."""
    relic_id = RelicId.DIVINE_DESTINY
    rarity = RelicRarity.STARTER
    pool = RelicPool.EVENT
    STARS = 6

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and combat.round_number == 1:
            owner.gain_stars(self.STARS)
