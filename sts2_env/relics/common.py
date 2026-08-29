"""Common relics (~32 total).

All Common-rarity relics from the reference doc.
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from sts2_env.core.constants import PERCENT_DENOMINATOR
from sts2_env.core.enums import (
    RelicRarity, CombatSide, CardType, PowerId, ValueProp, CardTag,
)
from sts2_env.relics.base import RelicId, RelicPool, RelicInstance
from sts2_env.relics.registry import register_relic

if TYPE_CHECKING:
    from sts2_env.cards.base import CardInstance
    from sts2_env.core.creature import Creature
    from sts2_env.core.combat import CombatState


def _gain_unpowered_block(owner: Creature, amount: int, combat: CombatState) -> int:
    before = owner.block
    owner.gain_block(amount, unpowered=True)
    gained = owner.block - before
    if gained > 0:
        from sts2_env.core.hooks import fire_after_block_gained

        fire_after_block_gained(owner, gained, combat)
    return gained


@register_relic
class AmethystAubergine(RelicInstance):
    """Gain 15 gold after non-boss combat rooms."""
    relic_id = RelicId.AMETHYST_AUBERGINE
    rarity = RelicRarity.COMMON
    pool = RelicPool.SHARED
    # IsAllowedInShops => false (v0.111.0); shop relic rolls do not consult this yet.
    is_allowed_in_shops = False

    def is_allowed(self, run_state: RunState) -> bool:
        return self.is_before_act3_treasure_chest(run_state)
    GOLD = 15

    def modify_rewards(
        self,
        owner: Creature,
        rewards: list[object],
        room: object | None,
        run_state: object,
    ) -> list[object]:
        from sts2_env.core.enums import RoomType
        from sts2_env.run.reward_objects import GoldReward

        room_type = getattr(room, "room_type", None)
        is_final_boss = (
            room_type == RoomType.BOSS
            and run_state.current_act_index >= len(getattr(run_state, "acts", ())) - 1
        )
        if room is not None and room_type in {RoomType.MONSTER, RoomType.ELITE, RoomType.BOSS} and not is_final_boss:
            return [*rewards, GoldReward(owner.player_id, self.GOLD, self.GOLD)]
        return rewards


@register_relic
class Anchor(RelicInstance):
    """Gain 10 block at combat start."""
    relic_id = RelicId.ANCHOR
    rarity = RelicRarity.COMMON
    pool = RelicPool.SHARED
    BLOCK = 10

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        _gain_unpowered_block(owner, self.BLOCK, combat)


@register_relic
class BagOfPreparation(RelicInstance):
    """Draw +2 cards on round 1."""
    relic_id = RelicId.BAG_OF_PREPARATION
    rarity = RelicRarity.COMMON
    pool = RelicPool.SHARED
    EXTRA_CARDS = 2

    def modify_hand_draw(self, owner: Creature, draw: int, combat: CombatState) -> int:
        if combat.round_number == 1:
            return draw + self.EXTRA_CARDS
        return draw


@register_relic
class BloodVial(RelicInstance):
    """Heal 2 on round 1."""
    relic_id = RelicId.BLOOD_VIAL
    rarity = RelicRarity.COMMON
    pool = RelicPool.SHARED
    HEAL = 2

    def after_player_turn_start_late(self, owner: Creature, combat: CombatState) -> None:
        if combat.round_number == 1:
            owner.heal(self.HEAL)


@register_relic
class BoneFlute(RelicInstance):
    """When Osty attacks, gain 2 block."""
    relic_id = RelicId.BONE_FLUTE
    rarity = RelicRarity.COMMON
    pool = RelicPool.NECROBINDER
    BLOCK = 2

    def after_attack(self, owner: Creature, attack: object, combat: CombatState) -> None:
        dealer = getattr(attack, "attacker", None)
        if getattr(dealer, "is_osty", False) and getattr(dealer, "pet_owner", None) is owner:
            _gain_unpowered_block(owner, self.BLOCK, combat)


@register_relic
class BronzeScales(RelicInstance):
    """Gain 3 Thorns at combat start."""
    relic_id = RelicId.BRONZE_SCALES
    rarity = RelicRarity.COMMON
    pool = RelicPool.SHARED
    THORNS = 3

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        owner.apply_power(PowerId.THORNS, self.THORNS)


@register_relic
class CentennialPuzzle(RelicInstance):
    """First time taking unblocked damage in combat, draw 3."""
    relic_id = RelicId.CENTENNIAL_PUZZLE
    rarity = RelicRarity.COMMON
    pool = RelicPool.SHARED
    CARDS = 3

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._used_this_combat: bool = False

    def after_damage_received(
        self, owner: Creature, target: Creature, dealer: Creature | None,
        damage: int, props: ValueProp, combat: CombatState
    ) -> None:
        if target is owner and damage > 0 and not self._used_this_combat:
            self._used_this_combat = True
            combat.draw_cards(owner, self.CARDS)

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        self._used_this_combat = False


@register_relic
class DataDisk(RelicInstance):
    """Gain 1 Focus at combat start."""
    relic_id = RelicId.DATA_DISK
    rarity = RelicRarity.COMMON
    pool = RelicPool.DEFECT
    FOCUS = 1

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        owner.apply_power(PowerId.FOCUS, self.FOCUS)


@register_relic
class FencingManual(RelicInstance):
    """Round 1: forge 10."""
    relic_id = RelicId.FENCING_MANUAL
    rarity = RelicRarity.COMMON
    pool = RelicPool.REGENT
    FORGE = 10

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and combat.round_number == 1:
            owner.gain_forge(self.FORGE, source=self)


@register_relic
class FestivePopper(RelicInstance):
    """Round 1: deal 9 unpowered damage to all enemies."""
    relic_id = RelicId.FESTIVE_POPPER
    rarity = RelicRarity.COMMON
    pool = RelicPool.SHARED
    DAMAGE = 9

    def after_player_turn_start(self, owner: Creature, combat: CombatState) -> None:
        if combat.round_number == 1:
            combat.deal_damage(
                dealer=owner,
                amount=self.DAMAGE,
                props=ValueProp.UNPOWERED,
                targets=list(combat.hittable_enemies),
            )


@register_relic
class Gorget(RelicInstance):
    """Gain 4 Plating at combat start."""
    relic_id = RelicId.GORGET
    rarity = RelicRarity.COMMON
    pool = RelicPool.SHARED
    PLATING = 4

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        owner.apply_power(PowerId.PLATING, self.PLATING)


@register_relic
class HappyFlower(RelicInstance):
    """Every 3 turns gain 1 energy."""
    relic_id = RelicId.HAPPY_FLOWER
    rarity = RelicRarity.COMMON
    pool = RelicPool.SHARED
    ENERGY = 1
    TURNS = 3

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._turns_seen: int = 0

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            self._turns_seen = (self._turns_seen + 1) % self.TURNS
            if self._turns_seen == 0:
                combat.gain_energy(owner, self.ENERGY)

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        pass  # _turns_seen persists across combats (SavedProperty)


@register_relic
class JuzuBracelet(RelicInstance):
    """Removes Monster from unknown room types."""
    relic_id = RelicId.JUZU_BRACELET
    rarity = RelicRarity.COMMON
    pool = RelicPool.SHARED

    def is_allowed(self, run_state: RunState) -> bool:
        return self.is_before_act3_treasure_chest(run_state)

    def modify_unknown_map_point_room_types(self, owner: Creature, room_types: set[object]) -> set[object]:
        from sts2_env.core.enums import RoomType

        return {room_type for room_type in room_types if room_type is not RoomType.MONSTER}


@register_relic
class Lantern(RelicInstance):
    """Round 1: gain 1 energy."""
    relic_id = RelicId.LANTERN
    rarity = RelicRarity.COMMON
    pool = RelicPool.SHARED
    ENERGY = 1

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and combat.round_number == 1:
            combat.gain_energy(owner, self.ENERGY)


@register_relic
class MealTicket(RelicInstance):
    """Heal 15 at merchant rooms."""
    relic_id = RelicId.MEAL_TICKET
    rarity = RelicRarity.COMMON
    pool = RelicPool.SHARED

    def is_allowed(self, run_state: RunState) -> bool:
        return self.is_before_act3_treasure_chest(run_state)
    HEAL = 15

    def after_room_entered(self, owner: Creature, room_type: object) -> None:
        if hasattr(room_type, "is_merchant") and room_type.is_merchant:
            if owner.current_hp > 0:
                owner.heal(self.HEAL)


@register_relic
class OddlySmoothStone(RelicInstance):
    """Gain 1 Dexterity at combat start."""
    relic_id = RelicId.ODDLY_SMOOTH_STONE
    rarity = RelicRarity.COMMON
    pool = RelicPool.SHARED
    DEXTERITY = 1

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        owner.apply_power(PowerId.DEXTERITY, self.DEXTERITY)


@register_relic
class Pendulum(RelicInstance):
    """Every 3rd turn, draw 1 additional card."""
    relic_id = RelicId.PENDULUM
    rarity = RelicRarity.COMMON
    pool = RelicPool.SHARED
    CARDS = 1
    TURNS = 3

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._turns_seen: int = 0

    def before_hand_draw(self, owner: Creature, combat: CombatState) -> None:
        # v0.111.0 Pendulum.cs BeforeHandDraw: TurnsSeen = (TurnsSeen + 1) % Turns.
        self._turns_seen = (self._turns_seen + 1) % self.TURNS

    def modify_hand_draw(self, owner: Creature, draw: int, combat: CombatState) -> int:
        if self._turns_seen == 0:
            return draw + self.CARDS
        return draw

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        pass  # _turns_seen persists across combats (SavedProperty)


@register_relic
class Permafrost(RelicInstance):
    """First Power card played per combat: gain 7 block."""
    relic_id = RelicId.PERMAFROST
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.SHARED
    BLOCK = 7

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._activated_this_combat: bool = False

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        if (not self._activated_this_combat
                and getattr(card, "owner", None) is owner
                and hasattr(card, "card_type") and card.card_type == CardType.POWER):
            self._activated_this_combat = True
            _gain_unpowered_block(owner, self.BLOCK, combat)

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        self._activated_this_combat = False


@register_relic
class PotionBelt(RelicInstance):
    """Gain 2 potion slots on obtain."""
    relic_id = RelicId.POTION_BELT
    rarity = RelicRarity.COMMON
    pool = RelicPool.SHARED
    has_upon_pickup_effect = True
    POTION_SLOTS = 2

    def after_obtained(self, owner: Creature) -> None:
        owner.gain_potion_slots(self.POTION_SLOTS)


@register_relic
class RedSkull(RelicInstance):
    """When HP <= 50%: gain 3 Strength. When HP > 50%: remove it."""
    relic_id = RelicId.RED_SKULL
    rarity = RelicRarity.COMMON
    pool = RelicPool.IRONCLAD
    HP_THRESHOLD_PCT = 50
    STRENGTH = 3

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._strength_applied: bool = False

    def _check_hp(self, owner: Creature) -> None:
        low_hp = owner.current_hp <= (owner.max_hp * self.HP_THRESHOLD_PCT // PERCENT_DENOMINATOR)
        if low_hp and not self._strength_applied:
            owner.apply_power(PowerId.STRENGTH, self.STRENGTH)
            self._strength_applied = True
        elif not low_hp and self._strength_applied:
            owner.apply_power(PowerId.STRENGTH, -self.STRENGTH)
            self._strength_applied = False

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        self._strength_applied = False
        self._check_hp(owner)

    def after_current_hp_changed(
        self,
        owner: Creature,
        creature: Creature,
        delta: int,
        combat: CombatState,
    ) -> None:
        if creature is owner:
            self._check_hp(owner)

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        self._strength_applied = False


@register_relic
class RegalPillow(RelicInstance):
    """Add 15 to rest site heal amount."""
    relic_id = RelicId.REGAL_PILLOW
    rarity = RelicRarity.COMMON
    pool = RelicPool.SHARED
    HEAL = 15

    def modify_rest_site_heal_amount(self, owner: Creature, amount: int, run_state: object) -> int:
        return amount + self.HEAL


@register_relic
class SneckoSkull(RelicInstance):
    """When applying Poison, add +1."""
    relic_id = RelicId.SNECKO_SKULL
    rarity = RelicRarity.COMMON
    pool = RelicPool.SILENT
    EXTRA_POISON = 1

    def modify_power_amount_given(
        self,
        owner: Creature,
        power_id: PowerId,
        amount: int,
        giver: Creature,
        target: Creature | None,
        source: object | None,
        combat: CombatState,
    ) -> int:
        if giver is owner and power_id == PowerId.POISON and amount > 0:
            return amount + self.EXTRA_POISON
        return amount


@register_relic
class Strawberry(RelicInstance):
    """Gain 7 max HP on obtain."""
    relic_id = RelicId.STRAWBERRY
    rarity = RelicRarity.COMMON
    pool = RelicPool.SHARED
    has_upon_pickup_effect = True
    MAX_HP = 7

    def after_obtained(self, owner: Creature) -> None:
        owner.gain_max_hp(self.MAX_HP)


@register_relic
class StrikeDummy(RelicInstance):
    """Strike-tagged attacks deal +3 damage."""
    relic_id = RelicId.STRIKE_DUMMY
    rarity = RelicRarity.COMMON
    pool = RelicPool.SHARED
    EXTRA_DAMAGE = 3

    def modify_damage_additive(
        self, owner: Creature, dealer: Creature | None, target: Creature,
        props: ValueProp, card: object | None = None
    ) -> int:
        is_strike = card is not None and hasattr(card, "tags") and CardTag.STRIKE in card.tags
        if ((dealer is owner or getattr(card, "owner", None) is owner)
                and is_strike
                and hasattr(card, "card_type") and card.card_type == CardType.ATTACK
                and props.is_powered_attack()):
            return self.EXTRA_DAMAGE
        return 0


@register_relic
class TinyMailbox(RelicInstance):
    """Add potion reward at rest site."""
    relic_id = RelicId.TINY_MAILBOX
    rarity = RelicRarity.COMMON
    pool = RelicPool.SHARED

    def modify_rest_site_heal_rewards(
        self,
        owner: Creature,
        rewards: list[object],
        run_state: object,
        *,
        is_mimicked: bool = False,
    ) -> list[object]:
        from sts2_env.run.reward_objects import PotionReward

        return [*rewards, PotionReward(owner.player_id)]


@register_relic
class Vajra(RelicInstance):
    """Gain 1 Strength at combat start."""
    relic_id = RelicId.VAJRA
    rarity = RelicRarity.COMMON
    pool = RelicPool.SHARED
    STRENGTH = 1

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        owner.apply_power(PowerId.STRENGTH, self.STRENGTH)


@register_relic
class VenerableTeaSet(RelicInstance):
    """After rest site, gain 2 energy next combat."""
    relic_id = RelicId.VENERABLE_TEA_SET
    rarity = RelicRarity.COMMON
    pool = RelicPool.SHARED
    ENERGY = 2

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._gain_energy_next_combat: bool = False

    def after_room_entered(self, owner: Creature, room_type: object) -> None:
        if hasattr(room_type, "is_rest_site") and room_type.is_rest_site:
            self._gain_energy_next_combat = True

    def after_energy_reset(self, owner: Creature, combat: CombatState) -> None:
        if self._gain_energy_next_combat:
            combat.gain_energy(owner, self.ENERGY)
            self._gain_energy_next_combat = False


@register_relic
class WarPaint(RelicInstance):
    """Upgrade 2 random Skill cards on obtain."""
    relic_id = RelicId.WAR_PAINT
    rarity = RelicRarity.COMMON
    pool = RelicPool.SHARED
    has_upon_pickup_effect = True
    CARDS = 2

    def after_obtained(self, owner: Creature) -> None:
        if getattr(owner.run_state, "defer_followup_rewards", False):
            owner.offer_upgrade_cards_reward(self.CARDS, cards=owner.upgradable_deck_cards(CardType.SKILL))
            return
        owner.upgrade_random_cards(CardType.SKILL, self.CARDS, rng=owner.run_state.rng.niche)


@register_relic
class Whetstone(RelicInstance):
    """Upgrade 2 random Attack cards on obtain."""
    relic_id = RelicId.WHETSTONE
    rarity = RelicRarity.COMMON
    pool = RelicPool.SHARED
    has_upon_pickup_effect = True
    CARDS = 2

    def after_obtained(self, owner: Creature) -> None:
        if getattr(owner.run_state, "defer_followup_rewards", False):
            owner.offer_upgrade_cards_reward(self.CARDS, cards=owner.upgradable_deck_cards(CardType.ATTACK))
            return
        owner.upgrade_random_cards(CardType.ATTACK, self.CARDS, rng=owner.run_state.rng.niche)


@register_relic
class BookOfFiveRings(RelicInstance):
    """Every 5 cards added to deck, heal 20."""
    relic_id = RelicId.BOOK_OF_FIVE_RINGS
    rarity = RelicRarity.COMMON
    pool = RelicPool.SHARED

    def is_allowed(self, run_state: RunState) -> bool:
        return self.is_before_act3_treasure_chest(run_state)
    CARDS_THRESHOLD = 5
    HEAL = 20

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._cards_added: int = 0

    def on_card_added_to_deck(
        self,
        owner: Creature,
        card: CardInstance,
        source: object | None = None,
    ) -> None:
        self._cards_added += 1
        if self._cards_added % self.CARDS_THRESHOLD == 0:
            if owner.current_hp > 0:
                owner.heal(self.HEAL)
