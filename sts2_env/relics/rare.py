"""Rare relics (~40 total).

All Rare-rarity relics from the reference doc.
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from sts2_env.core.constants import PERCENT_DENOMINATOR, WEAK_MULTIPLIER
from sts2_env.core.creature import _power_type_for_amount, get_power_class
from sts2_env.core.enums import (
    RelicRarity, CombatSide, CardType, PowerId, PowerType, RoomType, ValueProp,
)
from sts2_env.cards.factory import (
    card_metadata,
    create_distinct_character_cards,
    eligible_character_cards,
    eligible_registered_cards,
)
from sts2_env.core.card_pools import CardPoolId
from sts2_env.run.rewards import (
    CARD_CREATION_SOURCE_ENCOUNTER,
    CARD_GENERATION_CONTEXT_COMBAT,
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


def _gain_unpowered_block(owner: Creature, amount: int, combat: CombatState) -> int:
    before = owner.block
    owner.gain_block(amount, unpowered=True)
    gained = owner.block - before
    if gained > 0:
        from sts2_env.core.hooks import fire_after_block_gained

        fire_after_block_gained(owner, gained, combat)
    return gained


def _upgrade_reward_cards(owner: Creature, cards: list[CardInstance], card_type: CardType) -> list[CardInstance]:
    for card in cards:
        if card.card_type == card_type:
            owner.upgrade_card_instance(card)
    return cards


def _can_upgrade_reward_cards(reward: CardReward) -> bool:
    return getattr(reward, "allow_hook_upgrades", True)


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
        if getattr(card, "owner", None) is owner and hasattr(card, "card_type") and card.card_type == CardType.ATTACK:
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

    def modify_hp_lost_after_osty(
        self, owner: Creature, target: Creature, amount: float,
        dealer: Creature | None, props: ValueProp
    ) -> float:
        if target is owner:
            remaining = max(0, self.MAX_HP_LOSS - self._damage_this_turn)
            return min(amount, remaining)
        return amount

    def after_damage_received(
        self,
        owner: Creature,
        target: Creature,
        dealer: Creature | None,
        damage: int,
        props: ValueProp,
        combat: CombatState,
    ) -> None:
        if target is owner:
            self._damage_this_turn += damage

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
    TRIGGER_ROUND = 3

    def after_block_cleared(self, owner: Creature, creature: Creature, combat: CombatState) -> None:
        if creature is owner and combat.round_number == self.TRIGGER_ROUND:
            _gain_unpowered_block(owner, self.BLOCK, combat)


@register_relic
class Chandelier(RelicInstance):
    """Round 3: gain 3 energy."""
    relic_id = RelicId.CHANDELIER
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED
    ENERGY = 3
    TRIGGER_ROUND = 3

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and combat.round_number == self.TRIGGER_ROUND:
            combat.gain_energy(owner, self.ENERGY)


@register_relic
class CharonsAshes(RelicInstance):
    """On card exhaust: deal 3 damage to all enemies."""
    relic_id = RelicId.CHARONS_ASHES
    rarity = RelicRarity.RARE
    pool = RelicPool.IRONCLAD
    DAMAGE = 3

    def after_card_exhausted(self, owner: Creature, card: object, combat: CombatState) -> None:
        if getattr(card, "owner", None) is not owner:
            return
        combat.deal_damage(
            dealer=owner,
            amount=self.DAMAGE,
            props=ValueProp.UNPOWERED,
            targets=list(combat.hittable_enemies),
        )


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
                and combat.is_owner_side_turn(owner)
                and not self._triggered_this_turn):
            self._triggered_this_turn = True
            owner.heal(damage)

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            self._triggered_this_turn = False


@register_relic
class EmotionChip(RelicInstance):
    """If took HP loss previous turn, trigger all orb passives."""
    relic_id = RelicId.EMOTION_CHIP
    rarity = RelicRarity.RARE
    pool = RelicPool.DEFECT

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._last_hp_loss_round: int | None = None

    def after_damage_received(
        self,
        owner: Creature,
        target: Creature,
        dealer: Creature | None,
        damage: int,
        props: ValueProp,
        combat: CombatState,
    ) -> None:
        result = getattr(combat, "_active_damage_result", None)
        if target is owner and not getattr(result, "was_fully_blocked", False):
            self._last_hp_loss_round = combat.round_number

    def after_player_turn_start(self, owner: Creature, combat: CombatState) -> None:
        if self._last_hp_loss_round != combat.round_number - 1:
            return

        state = combat.combat_player_state_for(owner)
        orb_queue = getattr(state, "orb_queue", None) if state is not None else None
        if orb_queue is None:
            return

        for orb in list(orb_queue.orbs):
            orb.on_passive(combat)

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        self._last_hp_loss_round = None


@register_relic
class FrozenEgg(RelicInstance):
    """Auto-upgrade Power cards when obtained."""
    relic_id = RelicId.FROZEN_EGG
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED

    def is_allowed(self, run_state: RunState) -> bool:
        return self.is_before_act3_treasure_chest(run_state)

    def modify_card_reward_options_late(
        self,
        owner: Creature,
        cards: list[CardInstance],
        reward: CardReward,
        room: Room | None,
        run_state: RunState,
    ) -> list[CardInstance]:
        if not _can_upgrade_reward_cards(reward):
            return cards
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

    def after_player_turn_start(self, owner: Creature, combat: CombatState) -> None:
        from sts2_env.core.hooks import fire_after_card_discarded

        if combat.round_number > 1 or combat.pending_choice is not None:
            return

        state = combat.combat_player_state_for(owner)
        if state is None or not state.hand:
            return

        hand_cards = list(state.hand)

        def _resolver(selected_cards: list[CardInstance]) -> None:
            draw_count = 0
            for card in selected_cards:
                if card in state.hand:
                    state.hand.remove(card)
                    state.discard.append(card)
                    fire_after_card_discarded(card, combat)
                    draw_count += 1
            if draw_count > 0:
                combat.draw_cards(owner, draw_count)

        combat.request_multi_card_choice(
            prompt="Choose cards to discard",
            cards=hand_cards,
            source_pile="hand",
            resolver=_resolver,
            min_count=0,
            max_count=len(hand_cards),
            allow_skip=True,
        )


@register_relic
class GamePiece(RelicInstance):
    """When Power played, draw 1 card."""
    relic_id = RelicId.GAME_PIECE
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED
    CARDS = 1

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        if getattr(card, "owner", None) is owner and hasattr(card, "card_type") and card.card_type == CardType.POWER:
            combat.draw_cards(owner, self.CARDS)


@register_relic
class Girya(RelicInstance):
    """Combat start: gain Strength equal to times lifted (max 3)."""
    relic_id = RelicId.GIRYA
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED
    MAX_LIFTS = 3

    def is_allowed(self, run_state: RunState) -> bool:
        return self.is_before_act3_treasure_chest(run_state)

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

        if self._times_lifted >= self.MAX_LIFTS:
            return options
        if not any(getattr(option, "option_id", "") == LiftOption.OPTION_ID for option in options):
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
        if getattr(card, "owner", None) is owner and hasattr(card, "is_shiv") and card.is_shiv:
            combat.apply_power_to(owner, PowerId.HELICAL_DART, self.DEXTERITY, applier=owner)


@register_relic
class IceCream(RelicInstance):
    """After round 1, conserve energy between turns."""
    relic_id = RelicId.ICE_CREAM
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED

    def should_reset_energy(self, owner: Creature, combat: CombatState) -> bool | None:
        if combat.round_number > 1 and combat.player is owner:
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
        if getattr(card, "owner", None) is owner and getattr(card, "energy_spent", 0) >= self.ENERGY_THRESHOLD:
            _gain_unpowered_block(owner, self.BLOCK, combat)


@register_relic
class IvoryTile(RelicInstance):
    """When card spends >= 3 energy, gain 1 energy."""
    relic_id = RelicId.IVORY_TILE
    rarity = RelicRarity.RARE
    pool = RelicPool.NECROBINDER
    ENERGY = 1
    THRESHOLD = 3

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        if getattr(card, "owner", None) is owner and hasattr(card, "energy_spent") and card.energy_spent >= self.THRESHOLD:
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
        if getattr(card, "owner", None) is owner and hasattr(card, "card_type") and card.card_type == CardType.ATTACK:
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
    COMBATS = 2
    CARDS = 1

    def is_allowed(self, run_state: RunState) -> bool:
        return self.is_before_act3_treasure_chest(run_state)

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._combats_seen: int = 0

    def modify_card_reward_options(
        self,
        owner: Creature,
        cards: list[CardInstance],
        reward: CardReward,
        room: Room | None,
        run_state: RunState,
    ) -> list[CardInstance]:
        from sts2_env.run.rewards import generate_combat_reward_cards

        if self._combats_seen <= 0 or self._combats_seen % self.COMBATS != 0:
            return cards
        if getattr(reward, "card_creation_source", CARD_CREATION_SOURCE_ENCOUNTER) != CARD_CREATION_SOURCE_ENCOUNTER:
            return cards
        if getattr(reward, "_lasting_candy_added", False):
            return cards
        existing_ids = {card.card_id for card in cards}
        custom_card_ids = tuple(
            card_id
            for card_id in getattr(reward, "custom_card_ids", ())
            if card_id not in existing_ids and card_metadata(card_id).card_type == CardType.POWER
        )
        if custom_card_ids:
            character_ids: tuple[str, ...] = ()
        elif getattr(reward, "has_custom_card_pool", False):
            return cards
        elif not getattr(reward, "_can_regenerate", True):
            return cards
        else:
            character_ids = getattr(reward, "character_ids", ())
            if getattr(reward, "use_default_character_pool", True) and not character_ids:
                character_ids = (owner.character_id,)
            custom_pool = []
            seen_ids = set(existing_ids)
            for character_id in character_ids:
                for card_id in eligible_character_cards(
                    character_id,
                    card_type=CardType.POWER,
                    generation_context=getattr(reward, "generation_context", CARD_GENERATION_CONTEXT_COMBAT),
                    is_multiplayer=len(run_state.players) > 1,
                ):
                    if card_id in seen_ids:
                        continue
                    seen_ids.add(card_id)
                    custom_pool.append(card_id)
            if getattr(reward, "include_colorless", False):
                for card_id in eligible_registered_cards(
                    card_pool=CardPoolId.COLORLESS,
                    card_type=CardType.POWER,
                    generation_context=getattr(reward, "generation_context", CARD_GENERATION_CONTEXT_COMBAT),
                    is_multiplayer=len(run_state.players) > 1,
                ):
                    if card_id in seen_ids:
                        continue
                    seen_ids.add(card_id)
                    custom_pool.append(card_id)
            custom_card_ids = tuple(custom_pool)
        generated = generate_combat_reward_cards(
            run_state,
            context=getattr(reward, "context", "regular"),
            num_cards=self.CARDS,
            character_ids=character_ids,
            generation_context=getattr(reward, "generation_context", CARD_GENERATION_CONTEXT_COMBAT),
            roll_upgrade=True,
            update_rarity_odds=False,
            card_type=CardType.POWER,
            custom_card_ids=custom_card_ids,
        )
        if generated:
            reward._lasting_candy_added = True
            return [*cards, generated[0]]
        return cards

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        self._combats_seen += 1


@register_relic
class LizardTail(RelicInstance):
    """Prevent death once, heal 50% max HP (at least 1)."""
    relic_id = RelicId.LIZARD_TAIL
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED
    HEAL_PCT = 50

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._was_used: bool = False

    @property
    def is_used_up(self) -> bool:
        return self._was_used

    def should_die_late(self, owner: Creature, combat: CombatState) -> bool | None:
        if not self._was_used:
            self._was_used = True
            # v0.111.0: heal amount is at least 1 (Math.Max(1m, ...)).
            heal_amount = max(1, owner.max_hp * self.HEAL_PCT // PERCENT_DENOMINATOR)
            if owner.is_dead:
                owner.current_hp = min(owner.max_hp, heal_amount)
            else:
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
            combat.gain_stars(owner, self.STARS)


@register_relic
class Mango(RelicInstance):
    """Gain 14 max HP on obtain."""
    relic_id = RelicId.MANGO
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED
    has_upon_pickup_effect = True
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

    def after_combat_victory_early(self, owner: Creature, combat: CombatState) -> None:
        if owner.current_hp <= (owner.max_hp * self.HP_THRESHOLD_PCT // PERCENT_DENOMINATOR):
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
        if self._orbs_channeled == self.ORB_THRESHOLD:
            combat.deal_damage(
                dealer=owner,
                amount=self.DAMAGE,
                props=ValueProp.UNPOWERED,
                targets=list(combat.hittable_enemies),
            )

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

    def is_allowed(self, run_state: RunState) -> bool:
        return self.is_before_act3_treasure_chest(run_state)

    def modify_card_reward_options_late(
        self,
        owner: Creature,
        cards: list[CardInstance],
        reward: CardReward,
        room: Room | None,
        run_state: RunState,
    ) -> list[CardInstance]:
        if not _can_upgrade_reward_cards(reward):
            return cards
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
        if getattr(card, "owner", None) is owner and hasattr(card, "card_type") and card.card_type == CardType.POWER:
            state = combat.combat_player_state_for(owner)
            if state is None:
                return
            candidates = [
                hand_card
                for hand_card in state.hand
                if not getattr(hand_card, "has_energy_cost_x", False)
                and (
                    getattr(hand_card, "cost", 0) > 0
                    or int(getattr(hand_card, "combat_vars", {}).get("_turn_star_cost_override", getattr(hand_card, "star_cost", 0))) > 0
                )
            ]
            if not candidates:
                return
            chosen = combat.combat_card_selection_rng.choice(candidates)
            if hasattr(chosen, "set_temporary_cost_for_turn"):
                chosen.set_temporary_free_this_turn()
            else:
                chosen.cost = 0


@register_relic
class OldCoin(RelicInstance):
    """Gain 300 gold on obtain."""
    relic_id = RelicId.OLD_COIN
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED
    has_upon_pickup_effect = True
    GOLD = 300

    def is_allowed(self, run_state: RunState) -> bool:
        return self.is_before_act3_treasure_chest(run_state)

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
        if getattr(card, "owner", None) is owner:
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
        if getattr(card, "owner", None) is not owner or not hasattr(card, "card_type"):
            return
        if card.card_type == CardType.ATTACK:
            self._attacks += 1
        elif card.card_type == CardType.SKILL:
            self._skills += 1
        elif card.card_type == CardType.POWER:
            self._powers += 1

        if (self._activated_this_turn < 1
                and self._attacks > 0
                and self._skills > 0
                and self._powers > 0):
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
        if (getattr(card, "owner", None) is owner
                and hasattr(card, "card_type")
                and card.card_type in (CardType.ATTACK, CardType.SKILL)
                and getattr(card, "upgraded", False) is False):
            combat.upgrade_card(card)


@register_relic
class Shovel(RelicInstance):
    """Add Dig rest site option."""
    relic_id = RelicId.SHOVEL
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED

    def is_allowed(self, run_state: RunState) -> bool:
        return self.is_before_act3_treasure_chest(run_state)

    def modify_rest_site_options(self, owner: Creature, options: list[object], run_state: RunState) -> list[object]:
        from sts2_env.run.rest_site import DigOption

        if not any(getattr(option, "option_id", "") == DigOption.OPTION_ID for option in options):
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
        if getattr(card, "owner", None) is owner and hasattr(card, "card_type") and card.card_type == CardType.ATTACK:
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
            combat.deal_damage(
                dealer=owner,
                amount=self.DAMAGE,
                props=ValueProp.UNPOWERED,
                targets=list(combat.hittable_enemies),
            )


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

    def after_preventing_block_clear(self, owner: Creature, creature: Creature, combat: CombatState) -> None:
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
        return max(0, int(round(price * (PERCENT_DENOMINATOR - self.DISCOUNT) / PERCENT_DENOMINATOR)))

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
        if getattr(card, "owner", None) is owner and combat.is_owner_side_turn(owner):
            _gain_unpowered_block(owner, self.BLOCK, combat)


@register_relic
class ToxicEgg(RelicInstance):
    """Auto-upgrade Skill cards when obtained."""
    relic_id = RelicId.TOXIC_EGG
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED

    def is_allowed(self, run_state: RunState) -> bool:
        return self.is_before_act3_treasure_chest(run_state)

    def modify_card_reward_options_late(
        self,
        owner: Creature,
        cards: list[CardInstance],
        reward: CardReward,
        room: Room | None,
        run_state: RunState,
    ) -> list[CardInstance]:
        if not _can_upgrade_reward_cards(reward):
            return cards
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

    def modify_hp_lost_after_osty(
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
    """First debuff card: double all debuff amounts (once per combat, skips Artifact targets)."""
    relic_id = RelicId.UNSETTLING_LAMP
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED
    MULTIPLIER = 2

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._triggering_card: object | None = None
        self._is_finished_triggering: bool = False
        self._doubled_temporary_internal_power_ids: set[PowerId] = set()

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        self._triggering_card = None
        self._is_finished_triggering = False
        self._doubled_temporary_internal_power_ids = set()

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
        if amount == 0 or self._is_finished_triggering:
            return amount
        card_source = source if source is not None else getattr(combat, "active_card_source", None)
        if card_source is None:
            return amount
        if target is None or target.side == owner.side:
            return amount
        power_cls = get_power_class(power_id)
        if power_cls is None or not getattr(power_cls, "is_visible", True):
            return amount
        if _power_type_for_amount(power_cls, amount) != PowerType.DEBUFF:
            return amount
        if target.has_power(PowerId.ARTIFACT):
            # v0.111.0: a debuff absorbed by Artifact no longer triggers the relic.
            return amount
        if self._triggering_card is None:
            self._triggering_card = card_source
        if card_source is not self._triggering_card:
            return amount
        if power_id in self._doubled_temporary_internal_power_ids:
            return amount
        temporary_internal_power_ids = {
            PowerId.FLEX_POTION: PowerId.STRENGTH,
            PowerId.SHACKLING_POTION: PowerId.STRENGTH,
            PowerId.SPEED_POTION: PowerId.DEXTERITY,
            PowerId.TEMPORARY_DEXTERITY: PowerId.DEXTERITY,
            PowerId.TEMPORARY_FOCUS: PowerId.FOCUS,
            PowerId.TEMPORARY_STRENGTH: PowerId.STRENGTH,
        }
        internal_power_id = temporary_internal_power_ids.get(power_id)
        if internal_power_id is not None:
            self._doubled_temporary_internal_power_ids.add(internal_power_id)
        return amount * self.MULTIPLIER

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        if card is self._triggering_card:
            self._is_finished_triggering = True

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        self._triggering_card = None
        self._is_finished_triggering = False
        self._doubled_temporary_internal_power_ids = set()


@register_relic
class RuinedHelmet(RelicInstance):
    """First positive Strength gain in combat is doubled."""
    relic_id = RelicId.RUINED_HELMET
    rarity = RelicRarity.RARE
    pool = RelicPool.IRONCLAD
    MULTIPLIER = 2

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._used_this_combat: bool = False

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        self._used_this_combat = False

    def modify_power_amount_received(
        self,
        owner: Creature,
        power_id: PowerId,
        amount: int,
        target: Creature,
        applier: Creature | None,
        source: object | None,
        combat: CombatState,
    ) -> int:
        if target is owner and power_id == PowerId.STRENGTH and amount > 0 and not self._used_this_combat:
            return amount * self.MULTIPLIER
        return amount

    def after_modifying_power_amount_received(
        self,
        owner: Creature,
        power_id: PowerId,
        original_amount: int,
        modified_amount: int,
        target: Creature,
        applier: Creature | None,
        source: object | None,
        combat: CombatState,
    ) -> None:
        if target is owner and power_id == PowerId.STRENGTH and modified_amount != original_amount:
            self._used_this_combat = True

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        self._used_this_combat = False


@register_relic
class VexingPuzzlebox(RelicInstance):
    """Round 1: generate a random card in hand, free this turn."""
    relic_id = RelicId.VEXING_PUZZLEBOX
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED

    def after_player_turn_start(self, owner: Creature, combat: CombatState) -> None:
        if combat.round_number != 1:
            return
        state = combat.combat_player_state_for(owner)
        if state is None:
            return
        generated = create_distinct_character_cards(
            state.character_id,
            combat.combat_card_generation_rng,
            1,
            generation_context="combat",
            is_multiplayer=combat.is_multiplayer,
        )
        for card in generated:
            # v0.111.0: SetToFreeThisTurn instead of SetThisCombat(0).
            card.set_temporary_free_this_turn()
            combat.add_generated_card_to_creature_hand(owner, card)


@register_relic
class WhiteBeastStatue(RelicInstance):
    """Force potion reward after all combats."""
    relic_id = RelicId.WHITE_BEAST_STATUE
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED

    def is_allowed(self, run_state: RunState) -> bool:
        return self.is_before_act3_treasure_chest(run_state)

    def should_force_potion_reward(self, owner: Creature, room: Room | None = None) -> bool | None:
        if room is not None and getattr(room, "room_type", None) not in {
            RoomType.MONSTER,
            RoomType.ELITE,
            RoomType.BOSS,
        }:
            return False
        return True


@register_relic
class WhiteStar(RelicInstance):
    """Add boss-rarity card reward after elite combats."""
    relic_id = RelicId.WHITE_STAR
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED

    def is_allowed(self, run_state: RunState) -> bool:
        return self.is_before_act3_treasure_chest(run_state)

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
    EXTRA_WEAK = 0.15
    FLOAT_ROUND_DIGITS = 10

    def modify_damage_multiplicative(
        self,
        owner: Creature,
        dealer: Creature | None,
        target: Creature,
        props: ValueProp,
        card: object | None = None,
    ) -> float:
        if target is not owner or dealer is None or not props.is_powered_attack():
            return 1.0
        if dealer.get_power_amount(PowerId.WEAK) <= 0:
            return 1.0

        weak_multiplier = WEAK_MULTIPLIER
        if dealer.has_power(PowerId.DEBILITATE):
            weak_multiplier -= 1.0 - weak_multiplier

        return max(0.0, round((weak_multiplier - self.EXTRA_WEAK) / weak_multiplier, self.FLOAT_ROUND_DIGITS))


@register_relic
class CloakClasp(RelicInstance):
    """Before turn end, gain block equal to cards in hand."""
    relic_id = RelicId.CLOAK_CLASP
    rarity = RelicRarity.RARE
    pool = RelicPool.SHARED

    def before_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER:
            state = combat.combat_player_state_for(owner)
            hand_size = len(state.hand) if state is not None else 0
            if hand_size > 0:
                _gain_unpowered_block(owner, hand_size, combat)
