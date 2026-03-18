"""Rare relics (~40 total).

All Rare-rarity relics from the reference doc.
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from sts2_env.core.enums import (
    RelicRarity, CombatSide, CardType, PowerId, ValueProp,
)
from sts2_env.relics.base import RelicId, RelicPool, RelicInstance
from sts2_env.relics.registry import register_relic

if TYPE_CHECKING:
    from sts2_env.cards.base import CardInstance
    from sts2_env.core.creature import Creature
    from sts2_env.core.combat import CombatState
    from sts2_env.run.reward_objects import CardReward, Reward
    from sts2_env.run.rewards import CardRewardGenerationOptions
    from sts2_env.run.rooms import Room
    from sts2_env.run.run_state import RunState


def _upgrade_reward_cards(owner: Creature, cards: list[CardInstance], card_type: CardType) -> list[CardInstance]:
    for card in cards:
        if card.card_type == card_type:
            owner.upgrade_card_instance(card)
    return cards


@register_relic
class ArtOfWar(RelicInstance):
    """If no attacks played last turn, gain 1 energy."""
    relic_id = RelicId.ART_OF_WAR
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED
    ENERGY = 1

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._attacks_last_turn: bool = False
        self._attacks_this_turn: bool = False

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        if hasattr(card, "card_type") and card.card_type == CardType.ATTACK:
            self._attacks_this_turn = True

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER:
            self._attacks_last_turn = self._attacks_this_turn
            self._attacks_this_turn = False

    def after_energy_reset(self, owner: Creature, combat: CombatState) -> None:
        if combat.round_number > 1 and not self._attacks_last_turn:
            combat.gain_energy(owner, self.ENERGY)

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        self._attacks_last_turn = False
        self._attacks_this_turn = False


@register_relic
class BeatingRemnant(RelicInstance):
    """Cap damage taken per turn to 20."""
    relic_id = RelicId.BEATING_REMNANT
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED
    MAX_HP_LOSS = 20

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._damage_this_turn: float = 0

    def modify_hp_lost(
        self, owner: Creature, target: Creature, amount: float,
        dealer: Creature | None, props: ValueProp
    ) -> float:
        if target is owner:
            remaining = max(0, self.MAX_HP_LOSS - self._damage_this_turn)
            capped = min(amount, remaining)
            self._damage_this_turn += capped
            return capped
        return amount

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER:
            self._damage_this_turn = 0


@register_relic
class BigHat(RelicInstance):
    """Round 1: generate 2 random Ethereal cards in hand."""
    relic_id = RelicId.BIG_HAT
    rarity = RelicRarity.RARE
    pool = RelicPool.NECROBINDER
    CARDS = 2

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and combat.round_number == 1:
            combat.generate_ethereal_cards(owner, self.CARDS)


@register_relic
class Bookmark(RelicInstance):
    """End of turn: reduce cost of random Retained card by 1."""
    relic_id = RelicId.BOOKMARK
    rarity = RelicRarity.RARE
    pool = RelicPool.NECROBINDER

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER:
            combat.reduce_retained_card_cost(owner)


@register_relic
class CaptainsWheel(RelicInstance):
    """Round 3 block clear: gain 18 block."""
    relic_id = RelicId.CAPTAINS_WHEEL
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED
    BLOCK = 18

    def after_block_cleared(self, owner: Creature, creature: Creature, combat: CombatState) -> None:
        if creature is owner and combat.round_number == 3:
            owner.gain_block(self.BLOCK, unpowered=True)


@register_relic
class Chandelier(RelicInstance):
    """Round 3: gain 3 energy."""
    relic_id = RelicId.CHANDELIER
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED
    ENERGY = 3

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and combat.round_number == 3:
            combat.gain_energy(owner, self.ENERGY)


@register_relic
class CharonsAshes(RelicInstance):
    """On card exhaust: deal 3 damage to all enemies."""
    relic_id = RelicId.CHARONS_ASHES
    rarity = RelicRarity.RARE
    pool = RelicPool.IRONCLAD
    DAMAGE = 3

    def after_card_exhausted(self, owner: Creature, card: object, combat: CombatState) -> None:
        for enemy in combat.get_alive_enemies():
            combat.deal_damage(owner, enemy, self.DAMAGE, ValueProp.UNPOWERED)


@register_relic
class DemonTongue(RelicInstance):
    """First unblocked damage on owner's turn: heal that amount."""
    relic_id = RelicId.DEMON_TONGUE
    rarity = RelicRarity.RARE
    pool = RelicPool.IRONCLAD

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._triggered_this_turn: bool = False

    def after_damage_received(
        self, owner: Creature, target: Creature, dealer: Creature | None,
        damage: int, props: ValueProp, combat: CombatState
    ) -> None:
        if (target is owner and damage > 0
                and combat.current_side == CombatSide.PLAYER
                and not self._triggered_this_turn):
            self._triggered_this_turn = True
            owner.heal(damage)

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER:
            self._triggered_this_turn = False


@register_relic
class EmotionChip(RelicInstance):
    """If took HP loss previous turn, trigger all orb passives."""
    relic_id = RelicId.EMOTION_CHIP
    rarity = RelicRarity.RARE
    pool = RelicPool.DEFECT
    # AfterPlayerTurnStart: checks combat history for HP loss


@register_relic
class FrozenEgg(RelicInstance):
    """Auto-upgrade Power cards when obtained."""
    relic_id = RelicId.FROZEN_EGG
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED

    def modify_card_reward_options_late(
        self,
        owner: Creature,
        cards: list[CardInstance],
        reward: CardReward,
        room: Room | None,
        run_state: RunState,
    ) -> list[CardInstance]:
        return _upgrade_reward_cards(owner, cards, CardType.POWER)

    def modify_card_being_added_to_deck(self, owner: Creature, card: CardInstance) -> CardInstance:
        if card.card_type == CardType.POWER:
            owner.upgrade_card_instance(card)
        return card

    def modify_merchant_card_creation_results(
        self,
        owner: Creature,
        card: CardInstance,
        *,
        is_colorless: bool,
        run_state: RunState,
    ) -> CardInstance:
        if card.card_type == CardType.POWER:
            owner.upgrade_card_instance(card)
        return card


@register_relic
class GamblingChip(RelicInstance):
    """Round 1: discard any cards and draw that many."""
    relic_id = RelicId.GAMBLING_CHIP
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED
    # AfterPlayerTurnStart round 1 - interactive discard/draw


@register_relic
class GamePiece(RelicInstance):
    """When Power played, draw 1 card."""
    relic_id = RelicId.GAME_PIECE
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED
    CARDS = 1

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        if hasattr(card, "card_type") and card.card_type == CardType.POWER:
            combat.draw_cards(owner, self.CARDS)


@register_relic
class Girya(RelicInstance):
    """Combat start: gain Strength equal to times lifted (max 3)."""
    relic_id = RelicId.GIRYA
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED
    MAX_LIFTS = 3

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._times_lifted: int = 0

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        if self._times_lifted > 0:
            owner.apply_power(PowerId.STRENGTH, self._times_lifted)

    def lift(self) -> bool:
        if self._times_lifted < self.MAX_LIFTS:
            self._times_lifted += 1
            return True
        return False

    def modify_rest_site_options(self, owner: Creature, options: list[object], run_state: RunState) -> list[object]:
        from sts2_env.run.rest_site import LiftOption

        if not any(getattr(option, "option_id", "") == "LIFT" for option in options):
            options = [*options, LiftOption(self._times_lifted)]
        return options


@register_relic
class HelicalDart(RelicInstance):
    """When Shiv played, gain 1 Dexterity."""
    relic_id = RelicId.HELICAL_DART
    rarity = RelicRarity.RARE
    pool = RelicPool.SILENT
    DEXTERITY = 1

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        if hasattr(card, "is_shiv") and card.is_shiv:
            owner.apply_power(PowerId.DEXTERITY, self.DEXTERITY)


@register_relic
class IceCream(RelicInstance):
    """After round 1, conserve energy between turns."""
    relic_id = RelicId.ICE_CREAM
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED

    def should_reset_energy(self, owner: Creature, combat: CombatState) -> bool | None:
        if combat.round_number > 1:
            return False
        return None


@register_relic
class IntimidatingHelmet(RelicInstance):
    """Cards costing >= 2: gain 4 block."""
    relic_id = RelicId.INTIMIDATING_HELMET
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED
    BLOCK = 4
    ENERGY_THRESHOLD = 2

    def before_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        if hasattr(card, "energy_cost") and card.energy_cost >= self.ENERGY_THRESHOLD:
            owner.gain_block(self.BLOCK, unpowered=True)


@register_relic
class IvoryTile(RelicInstance):
    """When card spends >= 3 energy, gain 1 energy."""
    relic_id = RelicId.IVORY_TILE
    rarity = RelicRarity.RARE
    pool = RelicPool.NECROBINDER
    ENERGY = 1
    THRESHOLD = 3

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        if hasattr(card, "energy_spent") and card.energy_spent >= self.THRESHOLD:
            combat.gain_energy(owner, self.ENERGY)


@register_relic
class Kunai(RelicInstance):
    """Every 3 attacks this turn, gain 1 Dexterity."""
    relic_id = RelicId.KUNAI
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED
    ATTACK_THRESHOLD = 3
    DEXTERITY = 1

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._attacks_this_turn: int = 0

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER:
            self._attacks_this_turn = 0

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        if hasattr(card, "card_type") and card.card_type == CardType.ATTACK:
            self._attacks_this_turn += 1
            if self._attacks_this_turn % self.ATTACK_THRESHOLD == 0:
                owner.apply_power(PowerId.DEXTERITY, self.DEXTERITY)

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        self._attacks_this_turn = 0


@register_relic
class LastingCandy(RelicInstance):
    """Every 2nd combat: extra Power in card rewards."""
    relic_id = RelicId.LASTING_CANDY
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._combats_seen: int = 0

    def modify_card_reward_options_late(
        self,
        owner: Creature,
        cards: list[CardInstance],
        reward: CardReward,
        room: Room | None,
        run_state: RunState,
    ) -> list[CardInstance]:
        from sts2_env.cards.factory import create_character_cards

        if self._combats_seen <= 0 or self._combats_seen % 2 != 0:
            return cards
        if getattr(reward, "_lasting_candy_added", False):
            return cards
        generated = create_character_cards(
            owner.character_id,
            run_state.rng.rewards,
            1,
            card_type=CardType.POWER,
            generation_context="modifier",
            distinct=True,
        )
        if generated:
            reward._lasting_candy_added = True
            return [*cards, generated[0]]
        return cards

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        self._combats_seen += 1


@register_relic
class LizardTail(RelicInstance):
    """Prevent death once, heal 50% max HP."""
    relic_id = RelicId.LIZARD_TAIL
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED
    HEAL_PCT = 50

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._was_used: bool = False

    def should_die_late(self, owner: Creature, combat: CombatState) -> bool | None:
        if not self._was_used:
            self._was_used = True
            heal_amount = owner.max_hp * self.HEAL_PCT // 100
            owner.heal(heal_amount)
            return False
        return None


@register_relic
class LunarPastry(RelicInstance):
    """End of each turn: gain 1 star."""
    relic_id = RelicId.LUNAR_PASTRY
    rarity = RelicRarity.RARE
    pool = RelicPool.REGENT
    STARS = 1

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER:
            owner.gain_stars(self.STARS)


@register_relic
class Mango(RelicInstance):
    """Gain 14 max HP on obtain."""
    relic_id = RelicId.MANGO
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED
    MAX_HP = 14

    def after_obtained(self, owner: Creature) -> None:
        owner.gain_max_hp(self.MAX_HP)


@register_relic
class MeatOnTheBone(RelicInstance):
    """After combat, if HP <= 50%, heal 12."""
    relic_id = RelicId.MEAT_ON_THE_BONE
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED
    HP_THRESHOLD_PCT = 50
    HEAL = 12

    def after_combat_victory(self, owner: Creature, combat: CombatState) -> None:
        if owner.current_hp <= (owner.max_hp * self.HP_THRESHOLD_PCT // 100):
            owner.heal(self.HEAL)


@register_relic
class Metronome(RelicInstance):
    """After 7 orbs channeled, deal 30 damage to all enemies."""
    relic_id = RelicId.METRONOME
    rarity = RelicRarity.RARE
    pool = RelicPool.DEFECT
    ORB_THRESHOLD = 7
    DAMAGE = 30

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._orbs_channeled: int = 0

    def on_orb_channeled(self, owner: Creature, combat: CombatState) -> None:
        self._orbs_channeled += 1
        if self._orbs_channeled >= self.ORB_THRESHOLD:
            self._orbs_channeled = 0
            for enemy in combat.get_alive_enemies():
                combat.deal_damage(owner, enemy, self.DAMAGE, ValueProp.UNPOWERED)

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        self._orbs_channeled = 0


@register_relic
class MiniRegent(RelicInstance):
    """First time stars spent each turn: gain 1 Strength."""
    relic_id = RelicId.MINI_REGENT
    rarity = RelicRarity.RARE
    pool = RelicPool.REGENT
    STRENGTH = 1

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._used_this_turn: bool = False

    def on_stars_spent(self, owner: Creature, amount: int, combat: CombatState) -> None:
        if not self._used_this_turn:
            self._used_this_turn = True
            owner.apply_power(PowerId.STRENGTH, self.STRENGTH)

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER:
            self._used_this_turn = False

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        self._used_this_turn = False


@register_relic
class MoltenEgg(RelicInstance):
    """Auto-upgrade Attack cards when obtained."""
    relic_id = RelicId.MOLTEN_EGG
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED

    def modify_card_reward_options_late(
        self,
        owner: Creature,
        cards: list[CardInstance],
        reward: CardReward,
        room: Room | None,
        run_state: RunState,
    ) -> list[CardInstance]:
        return _upgrade_reward_cards(owner, cards, CardType.ATTACK)

    def modify_card_being_added_to_deck(self, owner: Creature, card: CardInstance) -> CardInstance:
        if card.card_type == CardType.ATTACK:
            owner.upgrade_card_instance(card)
        return card

    def modify_merchant_card_creation_results(
        self,
        owner: Creature,
        card: CardInstance,
        *,
        is_colorless: bool,
        run_state: RunState,
    ) -> CardInstance:
        if card.card_type == CardType.ATTACK:
            owner.upgrade_card_instance(card)
        return card


@register_relic
class MummifiedHand(RelicInstance):
    """When Power played, reduce cost of random card in hand to 0."""
    relic_id = RelicId.MUMMIFIED_HAND
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        if hasattr(card, "card_type") and card.card_type == CardType.POWER:
            candidates = [
                hand_card
                for hand_card in combat.hand
                if getattr(hand_card, "cost", 0) > 0 and not getattr(hand_card, "has_energy_cost_x", False)
            ]
            if not candidates:
                return
            chosen = combat.rng.choice(candidates)
            if hasattr(chosen, "set_temporary_cost_for_turn"):
                chosen.set_temporary_cost_for_turn(0)
            else:
                chosen.cost = 0


@register_relic
class OldCoin(RelicInstance):
    """Gain 300 gold on obtain."""
    relic_id = RelicId.OLD_COIN
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED
    GOLD = 300

    def after_obtained(self, owner: Creature) -> None:
        owner.gain_gold(self.GOLD)


@register_relic
class OrangeDough(RelicInstance):
    """Round 1: generate 2 random colorless cards in hand."""
    relic_id = RelicId.ORANGE_DOUGH
    rarity = RelicRarity.RARE
    pool = RelicPool.REGENT
    CARDS = 2

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and combat.round_number == 1:
            combat.generate_colorless_cards(owner, self.CARDS)


@register_relic
class Pocketwatch(RelicInstance):
    """If <= 3 cards played last turn, draw +3 this turn."""
    relic_id = RelicId.POCKETWATCH
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED
    CARD_THRESHOLD = 3
    EXTRA_DRAW = 3

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._cards_this_turn: int = 0
        self._cards_last_turn: int = 0

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        self._cards_this_turn += 1

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER:
            self._cards_last_turn = self._cards_this_turn
            self._cards_this_turn = 0

    def modify_hand_draw(self, owner: Creature, draw: int, combat: CombatState) -> int:
        if combat.round_number > 1 and self._cards_last_turn <= self.CARD_THRESHOLD:
            return draw + self.EXTRA_DRAW
        return draw

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        self._cards_this_turn = 0
        self._cards_last_turn = 0


@register_relic
class PowerCell(RelicInstance):
    """Round 1: move 2 random 0-cost cards from draw pile to hand."""
    relic_id = RelicId.POWER_CELL
    rarity = RelicRarity.RARE
    pool = RelicPool.DEFECT
    CARDS = 2

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and combat.round_number == 1:
            combat.move_zero_cost_cards_to_hand(owner, self.CARDS)


@register_relic
class PrayerWheel(RelicInstance):
    """Extra card reward after monster combats."""
    relic_id = RelicId.PRAYER_WHEEL
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED

    def modify_rewards(
        self,
        owner: Creature,
        rewards: list[Reward],
        room: Room | None,
        run_state: RunState,
    ) -> list[Reward]:
        from sts2_env.core.enums import RoomType
        from sts2_env.run.reward_objects import CardReward

        if room is not None and room.room_type == RoomType.MONSTER:
            return [*rewards, CardReward(owner.player_id, context="regular")]
        return rewards


@register_relic
class RainbowRing(RelicInstance):
    """If Attack+Skill+Power played this turn, gain 1 Str + 1 Dex."""
    relic_id = RelicId.RAINBOW_RING
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED
    STRENGTH = 1
    DEXTERITY = 1

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._attacks: int = 0
        self._skills: int = 0
        self._powers: int = 0
        self._activated_this_turn: int = 0

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER:
            self._attacks = 0
            self._skills = 0
            self._powers = 0
            self._activated_this_turn = 0

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        if not hasattr(card, "card_type"):
            return
        if card.card_type == CardType.ATTACK:
            self._attacks += 1
        elif card.card_type == CardType.SKILL:
            self._skills += 1
        elif card.card_type == CardType.POWER:
            self._powers += 1

        if (self._attacks > self._activated_this_turn
                and self._skills > self._activated_this_turn
                and self._powers > self._activated_this_turn):
            self._activated_this_turn += 1
            owner.apply_power(PowerId.STRENGTH, self.STRENGTH)
            owner.apply_power(PowerId.DEXTERITY, self.DEXTERITY)

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        self._attacks = 0
        self._skills = 0
        self._powers = 0
        self._activated_this_turn = 0


@register_relic
class RazorTooth(RelicInstance):
    """When Attack/Skill played that is upgradable, upgrade it in combat."""
    relic_id = RelicId.RAZOR_TOOTH
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        if (hasattr(card, "card_type")
                and card.card_type in (CardType.ATTACK, CardType.SKILL)
                and hasattr(card, "can_upgrade") and card.can_upgrade()
                and hasattr(card, "upgrade")):
            card.upgrade()


@register_relic
class Shovel(RelicInstance):
    """Add Dig rest site option."""
    relic_id = RelicId.SHOVEL
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED

    def modify_rest_site_options(self, owner: Creature, options: list[object], run_state: RunState) -> list[object]:
        from sts2_env.run.rest_site import DigOption

        if not any(getattr(option, "option_id", "") == "DIG" for option in options):
            options = [*options, DigOption()]
        return options


@register_relic
class Shuriken(RelicInstance):
    """Every 3 attacks this turn, gain 1 Strength."""
    relic_id = RelicId.SHURIKEN
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED
    ATTACK_THRESHOLD = 3
    STRENGTH = 1

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._attacks_this_turn: int = 0

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER:
            self._attacks_this_turn = 0

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        if hasattr(card, "card_type") and card.card_type == CardType.ATTACK:
            self._attacks_this_turn += 1
            if self._attacks_this_turn % self.ATTACK_THRESHOLD == 0:
                owner.apply_power(PowerId.STRENGTH, self.STRENGTH)

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        self._attacks_this_turn = 0


@register_relic
class StoneCalendar(RelicInstance):
    """On turn 7: deal 52 damage to all enemies."""
    relic_id = RelicId.STONE_CALENDAR
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED
    DAMAGE = 52
    DAMAGE_TURN = 7

    def before_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and combat.round_number == self.DAMAGE_TURN:
            for enemy in combat.get_alive_enemies():
                combat.deal_damage(owner, enemy, self.DAMAGE, ValueProp.UNPOWERED)


@register_relic
class SturdyClamp(RelicInstance):
    """Retain up to 10 block between turns."""
    relic_id = RelicId.STURDY_CLAMP
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED
    MAX_RETAINED_BLOCK = 10

    def should_clear_block(self, owner: Creature, creature: Creature) -> bool | None:
        if creature is owner:
            # Prevent full clear, but we cap to MAX_RETAINED_BLOCK
            return False
        return None

    def after_block_cleared(self, owner: Creature, creature: Creature, combat: CombatState) -> None:
        if creature is owner and owner.block > self.MAX_RETAINED_BLOCK:
            owner.block = self.MAX_RETAINED_BLOCK


@register_relic
class TheCourier(RelicInstance):
    """20% discount at merchants, merchant entries refill."""
    relic_id = RelicId.THE_COURIER
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED
    DISCOUNT = 20

    def modify_merchant_price(
        self,
        owner: Creature,
        price: int,
        *,
        item_kind: str,
        item: object,
        run_state: RunState,
    ) -> int:
        return max(0, int(round(price * (100 - self.DISCOUNT) / 100)))

    def should_refill_merchant_entry(
        self,
        owner: Creature,
        *,
        item_kind: str,
        item: object,
        run_state: RunState,
    ) -> bool | None:
        return item_kind in {"card", "relic", "potion"}


@register_relic
class ToughBandages(RelicInstance):
    """When card discarded on player turn, gain 3 block."""
    relic_id = RelicId.TOUGH_BANDAGES
    rarity = RelicRarity.RARE
    pool = RelicPool.SILENT
    BLOCK = 3

    def after_card_discarded(self, owner: Creature, card: object, combat: CombatState) -> None:
        if combat.current_side == CombatSide.PLAYER:
            owner.gain_block(self.BLOCK, unpowered=True)


@register_relic
class ToxicEgg(RelicInstance):
    """Auto-upgrade Skill cards when obtained."""
    relic_id = RelicId.TOXIC_EGG
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED

    def modify_card_reward_options_late(
        self,
        owner: Creature,
        cards: list[CardInstance],
        reward: CardReward,
        room: Room | None,
        run_state: RunState,
    ) -> list[CardInstance]:
        return _upgrade_reward_cards(owner, cards, CardType.SKILL)

    def modify_card_being_added_to_deck(self, owner: Creature, card: CardInstance) -> CardInstance:
        if card.card_type == CardType.SKILL:
            owner.upgrade_card_instance(card)
        return card

    def modify_merchant_card_creation_results(
        self,
        owner: Creature,
        card: CardInstance,
        *,
        is_colorless: bool,
        run_state: RunState,
    ) -> CardInstance:
        if card.card_type == CardType.SKILL:
            owner.upgrade_card_instance(card)
        return card


@register_relic
class TungstenRod(RelicInstance):
    """Reduce all HP loss by 1."""
    relic_id = RelicId.TUNGSTEN_ROD
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED
    REDUCTION = 1

    def modify_hp_lost(
        self, owner: Creature, target: Creature, amount: float,
        dealer: Creature | None, props: ValueProp
    ) -> float:
        if target is owner and amount > 0:
            return max(0, amount - self.REDUCTION)
        return amount


@register_relic
class UnceasingTop(RelicInstance):
    """When hand is empty in play phase, draw 1."""
    relic_id = RelicId.UNCEASING_TOP
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED

    def after_hand_emptied(self, owner: Creature, combat: CombatState) -> None:
        in_play_phase = getattr(combat, "in_play_phase", combat.current_side == CombatSide.PLAYER)
        if in_play_phase:
            combat.draw_cards(owner, 1)


@register_relic
class UnsettlingLamp(RelicInstance):
    """First debuff card: double all debuff amounts (once per combat)."""
    relic_id = RelicId.UNSETTLING_LAMP
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._triggered: bool = False

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        self._triggered = False

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        self._triggered = False


@register_relic
class RuinedHelmet(RelicInstance):
    """First positive Strength gain in combat is doubled."""
    relic_id = RelicId.RUINED_HELMET
    rarity = RelicRarity.RARE
    pool = RelicPool.IRONCLAD

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._used_this_combat: bool = False

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        self._used_this_combat = False

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        self._used_this_combat = False


@register_relic
class VexingPuzzlebox(RelicInstance):
    """Round 1: generate random card with 0 cost in hand."""
    relic_id = RelicId.VEXING_PUZZLEBOX
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and combat.round_number == 1:
            combat.generate_free_card_in_hand(owner)


@register_relic
class WhiteBeastStatue(RelicInstance):
    """Force potion reward after all combats."""
    relic_id = RelicId.WHITE_BEAST_STATUE
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED

    def should_force_potion_reward(self, owner: Creature) -> bool | None:
        return True


@register_relic
class WhiteStar(RelicInstance):
    """Add boss-rarity card reward after elite combats."""
    relic_id = RelicId.WHITE_STAR
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED

    def modify_rewards(
        self,
        owner: Creature,
        rewards: list[Reward],
        room: Room | None,
        run_state: RunState,
    ) -> list[Reward]:
        from sts2_env.core.enums import RoomType
        from sts2_env.run.reward_objects import CardReward

        if room is not None and room.room_type == RoomType.ELITE:
            return [*rewards, CardReward(owner.player_id, context="boss")]
        return rewards


@register_relic
class PaperKrane(RelicInstance):
    """Reduce Weak damage multiplier by 15%."""
    relic_id = RelicId.PAPER_KRANE
    rarity = RelicRarity.RARE
    pool = RelicPool.SILENT
    # ModifyWeakMultiplier hook - handled in damage pipeline


@register_relic
class CloakClasp(RelicInstance):
    """Before turn end, gain block equal to cards in hand."""
    relic_id = RelicId.CLOAK_CLASP
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED

    def before_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER:
            hand_size = len(combat.hand)
            if hand_size > 0:
                owner.gain_block(hand_size, unpowered=True)
