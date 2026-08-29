"""Starter relics (10 total).

BurningBlood, BlackBlood, RingOfTheSnake, RingOfTheDrake,
CrackedCore, InfusedCore, BoundPhylactery, PhylacteryUnbound,
DivineRight, DivineDestiny.
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from sts2_env.core.enums import RelicRarity, CombatSide, OrbType
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
            combat.channel_orb(owner, OrbType.LIGHTNING)


@register_relic
class InfusedCore(RelicInstance):
    """Upgraded CrackedCore: 3 Lightning on turn 1; your Lightning orbs deal +1 damage."""
    relic_id = RelicId.INFUSED_CORE
    rarity = RelicRarity.STARTER
    pool = RelicPool.EVENT
    LIGHTNING_COUNT = 3
    EXTRA_DAMAGE = 1

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and combat.round_number == 1:
            for _ in range(self.LIGHTNING_COUNT):
                combat.channel_orb(owner, OrbType.LIGHTNING)

    def on_orb_channeled(self, owner: Creature, combat: CombatState) -> None:
        # v0.111.0 InfusedCore.cs ModifyOrbValue: owner's Lightning orbs gain +ExtraDamage.
        # The orb value pipeline has no relic hook, so boost each freshly
        # channeled Lightning orb (channel_orb is the only creation funnel).
        state = combat.combat_player_state_for(owner)
        orbs = getattr(getattr(state, "orb_queue", None), "orbs", [])
        if not orbs:
            return
        orb = orbs[-1]
        if getattr(orb, "orb_type", None) is not OrbType.LIGHTNING:
            return
        bonus = self.EXTRA_DAMAGE
        base_passive = orb.get_passive_value
        base_evoke = orb.get_evoke_value
        orb.get_passive_value = lambda combat_ref: base_passive(combat_ref) + bonus
        orb.get_evoke_value = lambda combat_ref: base_evoke(combat_ref) + bonus


# ─── Necrobinder Starter ────────────────────────────────────────────────


@register_relic
class BoundPhylactery(RelicInstance):
    relic_id = RelicId.BOUND_PHYLACTERY
    rarity = RelicRarity.STARTER
    pool = RelicPool.NECROBINDER
    SUMMON_COUNT = 1

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        combat.summon_osty(owner, self.SUMMON_COUNT)

    def after_energy_reset_late(self, owner: Creature, combat: CombatState) -> None:
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

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        combat.gain_stars(owner, self.STARS)


@register_relic
class DivineDestiny(RelicInstance):
    """Upgraded DivineRight."""
    relic_id = RelicId.DIVINE_DESTINY
    rarity = RelicRarity.STARTER
    pool = RelicPool.EVENT
    STARS = 6

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and combat.round_number == 1:
            combat.gain_stars(owner, self.STARS)
