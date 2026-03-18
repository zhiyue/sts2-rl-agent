"""Shop, Event, and Ancient relics.

All Shop-rarity, Event-rarity, and Ancient-rarity relics, plus special relics
(Circlet, DeprecatedRelic).
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from sts2_env.cards.enchantments import can_enchant_card
from sts2_env.characters.all import ALL_CHARACTERS
from sts2_env.core.enums import (
    CardId, CardRarity, RelicRarity, CombatSide, CardType, PowerId, RoomType, ValueProp,
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


def _build_named_cards(owner: Creature, *names: str) -> list[CardInstance]:
    from sts2_env.cards.factory import create_card

    cards: list[CardInstance] = []
    for name in names:
        card_id = owner._coerce_card_id(name)
        if card_id is not None:
            cards.append(create_card(card_id))
    return cards


def _queue_named_cards_reward(owner: Creature, *names: str) -> bool:
    cards = _build_named_cards(owner, *names)
    if not cards:
        return False
    owner.offer_add_cards_reward(cards)
    return True


# ═══════════════════════════════════════════════════════════════════════════
# SHOP RELICS
# ═══════════════════════════════════════════════════════════════════════════


@register_relic
class BeltBuckle(RelicInstance):
    """No potions: +2 Dex. Potion procured: -2 Dex."""
    relic_id = RelicId.BELT_BUCKLE
    rarity = RelicRarity.SHOP
    pool = RelicPool.SHARED
    DEXTERITY = 2

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._dex_applied: bool = False

    def _owner_has_potions(self, owner: Creature, combat: CombatState) -> bool:
        state = combat.combat_player_state_for(owner)
        if state is not None:
            return any(potion is not None for potion in state.potions)
        held_potions = getattr(owner, "held_potions", None)
        if callable(held_potions):
            return bool(held_potions())
        return bool(getattr(owner, "has_potions", False))

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        if not self._owner_has_potions(owner, combat) and not self._dex_applied:
            owner.apply_power(PowerId.DEXTERITY, self.DEXTERITY)
            self._dex_applied = True

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        self._dex_applied = False


@register_relic
class Bread(RelicInstance):
    """+1 max energy, but lose 2 energy round 1."""
    relic_id = RelicId.BREAD
    rarity = RelicRarity.SHOP
    pool = RelicPool.SHARED
    GAIN_ENERGY = 1
    LOSE_ENERGY = 2

    def modify_max_energy(self, owner: Creature, energy: int) -> int:
        combat = getattr(owner, "combat_state", None)
        if combat is not None and combat.round_number == 1:
            return energy
        return energy + self.GAIN_ENERGY

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and combat.round_number == 1:
            combat.lose_energy(owner, self.LOSE_ENERGY)


@register_relic
class Brimstone(RelicInstance):
    """Each turn: gain 2 Str, all enemies gain 1 Str."""
    relic_id = RelicId.BRIMSTONE
    rarity = RelicRarity.SHOP
    pool = RelicPool.IRONCLAD
    SELF_STRENGTH = 2
    ENEMY_STRENGTH = 1

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER:
            owner.apply_power(PowerId.STRENGTH, self.SELF_STRENGTH)
            for enemy in combat.get_alive_enemies():
                enemy.apply_power(PowerId.STRENGTH, self.ENEMY_STRENGTH)


@register_relic
class BurningSticks(RelicInstance):
    """First Skill exhausted per combat: clone it in hand."""
    relic_id = RelicId.BURNING_STICKS
    rarity = RelicRarity.SHOP
    pool = RelicPool.SHARED

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._used_this_combat: bool = False

    def after_card_exhausted(self, owner: Creature, card: object, combat: CombatState) -> None:
        if (not self._used_this_combat
                and hasattr(card, "card_type") and card.card_type == CardType.SKILL):
            self._used_this_combat = True
            combat.clone_card_to_hand(owner, card)

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        self._used_this_combat = False

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        self._used_this_combat = False


@register_relic
class Cauldron(RelicInstance):
    """On obtain: offer 5 random potions."""
    relic_id = RelicId.CAULDRON
    rarity = RelicRarity.SHOP
    pool = RelicPool.SHARED
    POTIONS = 5

    def after_obtained(self, owner: Creature) -> None:
        owner.offer_potions(self.POTIONS)


@register_relic
class ChemicalX(RelicInstance):
    """X-cost cards get +2 to X value."""
    relic_id = RelicId.CHEMICAL_X
    rarity = RelicRarity.SHOP
    pool = RelicPool.SHARED
    INCREASE = 2

    def modify_x_value(self, owner: Creature, x_value: int, card: object) -> int:
        return x_value + self.INCREASE

    def before_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        """Apply X-value boost in the current play pipeline.

        The combat engine currently resolves many X cards from `card.energy_spent`,
        so we mirror the C# `ModifyXValue` effect by adjusting that value once per
        play sequence for X-cost cards.
        """
        if not getattr(card, "has_energy_cost_x", False):
            return
        if not hasattr(card, "combat_vars"):
            return

        key = "_chemical_x_base_spent"
        current = int(getattr(card, "energy_spent", 0))
        base = card.combat_vars.get(key)
        if base is None or current != base + self.INCREASE:
            base = current
            card.combat_vars[key] = base

        card.energy_spent = max(0, int(base) + self.INCREASE)


@register_relic
class DingyRug(RelicInstance):
    """Add colorless cards to card reward pool."""
    relic_id = RelicId.DINGY_RUG
    rarity = RelicRarity.SHOP
    pool = RelicPool.SHARED

    def modify_card_reward_creation_options(
        self,
        owner: Creature,
        options: CardRewardGenerationOptions,
        reward: CardReward,
        room: Room | None,
        run_state: RunState,
    ) -> CardRewardGenerationOptions:
        from sts2_env.run.rewards import CardRewardGenerationOptions

        return CardRewardGenerationOptions(
            context=options.context,
            num_cards=options.num_cards,
            character_ids=options.character_ids,
            forced_rarities=options.forced_rarities,
            include_colorless=True,
            use_default_character_pool=options.use_default_character_pool,
        )


@register_relic
class DollysMirror(RelicInstance):
    """On obtain: duplicate a card from deck."""
    relic_id = RelicId.DOLLYS_MIRROR
    rarity = RelicRarity.SHOP
    pool = RelicPool.SHARED

    def after_obtained(self, owner: Creature) -> None:
        candidates = owner.duplicable_deck_cards()
        if getattr(owner.run_state, "defer_followup_rewards", False):
            owner.offer_duplicate_card_reward(1, cards=candidates)
            return
        owner.duplicate_card_from_deck(cards=candidates)


@register_relic
class DragonFruit(RelicInstance):
    """Gain 1 max HP every time gold is gained."""
    relic_id = RelicId.DRAGON_FRUIT
    rarity = RelicRarity.SHOP
    pool = RelicPool.SHARED
    MAX_HP = 1

    def on_gold_gained(self, owner: Creature, amount: int) -> None:
        if amount > 0:
            owner.gain_max_hp(self.MAX_HP)


@register_relic
class GhostSeed(RelicInstance):
    """Basic Strike/Defend become Ethereal in combat."""
    relic_id = RelicId.GHOST_SEED
    rarity = RelicRarity.SHOP
    pool = RelicPool.SHARED
    # AfterCardEnteredCombat hook


@register_relic
class GnarledHammer(RelicInstance):
    """On obtain: enchant up to 3 cards with Sharp(3)."""
    relic_id = RelicId.GNARLED_HAMMER
    rarity = RelicRarity.SHOP
    pool = RelicPool.SHARED
    CARDS = 3
    SHARP = 3

    def after_obtained(self, owner: Creature) -> None:
        candidates = [card for card in owner.deck if can_enchant_card(card, "Sharp")]
        if getattr(owner.run_state, "defer_followup_rewards", False):
            owner.offer_enchant_cards_reward("Sharp", self.SHARP, self.CARDS, cards=candidates)
            return
        owner.enchant_selected_cards("Sharp", self.SHARP, self.CARDS, cards=candidates)


@register_relic
class Kifuda(RelicInstance):
    """On obtain: enchant up to 3 cards with Adroit(3)."""
    relic_id = RelicId.KIFUDA
    rarity = RelicRarity.SHOP
    pool = RelicPool.SHARED
    CARDS = 3

    def after_obtained(self, owner: Creature) -> None:
        candidates = [card for card in owner.deck if can_enchant_card(card, "Adroit")]
        if getattr(owner.run_state, "defer_followup_rewards", False):
            owner.offer_enchant_cards_reward("Adroit", 3, self.CARDS, cards=candidates)
            return
        owner.enchant_selected_cards("Adroit", 3, self.CARDS, cards=candidates)


@register_relic
class LavaLamp(RelicInstance):
    """If no damage taken this combat, upgrade all card rewards."""
    relic_id = RelicId.LAVA_LAMP
    rarity = RelicRarity.SHOP
    pool = RelicPool.SHARED

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._took_damage: bool = False

    def after_damage_received(
        self, owner: Creature, target: Creature, dealer: Creature | None,
        damage: int, props: ValueProp, combat: CombatState
    ) -> None:
        if target is owner and damage > 0:
            self._took_damage = True

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        self._took_damage = False

    def after_room_entered(self, owner: Creature, room_type: object) -> None:
        if getattr(room_type, "is_combat", False):
            self._took_damage = False

    def modify_card_reward_options_late(
        self,
        owner: Creature,
        cards: list[CardInstance],
        reward: CardReward,
        room: Room | None,
        run_state: RunState,
    ) -> list[CardInstance]:
        if room is not None and getattr(room, "room_type", None) in {RoomType.MONSTER, RoomType.ELITE, RoomType.BOSS} and not self._took_damage:
            for card in cards:
                owner.upgrade_card_instance(card)
        return cards


@register_relic
class LeesWaffle(RelicInstance):
    """Gain 7 max HP and heal to full."""
    relic_id = RelicId.LEES_WAFFLE
    rarity = RelicRarity.SHOP
    pool = RelicPool.SHARED
    MAX_HP = 7

    def after_obtained(self, owner: Creature) -> None:
        owner.gain_max_hp(self.MAX_HP)
        owner.heal(owner.max_hp)


@register_relic
class MembershipCard(RelicInstance):
    """50% discount at merchants."""
    relic_id = RelicId.MEMBERSHIP_CARD
    rarity = RelicRarity.SHOP
    pool = RelicPool.SHARED
    DISCOUNT = 50

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


@register_relic
class MiniatureTent(RelicInstance):
    """Allow multiple rest site options."""
    relic_id = RelicId.MINIATURE_TENT
    rarity = RelicRarity.SHOP
    pool = RelicPool.SHARED

    def should_disable_remaining_rest_site_options(
        self,
        owner: Creature,
        chosen_option: object,
        run_state: RunState,
    ) -> bool | None:
        return False


@register_relic
class MysticLighter(RelicInstance):
    """Enchanted card attacks deal +9 damage."""
    relic_id = RelicId.MYSTIC_LIGHTER
    rarity = RelicRarity.SHOP
    pool = RelicPool.SHARED
    EXTRA_DAMAGE = 9

    def modify_damage_additive(
        self, owner: Creature, dealer: Creature | None, target: Creature,
        props: ValueProp, card: object | None = None
    ) -> int:
        if (dealer is owner
                and card is not None
                and hasattr(card, "card_type") and card.card_type == CardType.ATTACK
                and hasattr(card, "is_enchanted") and card.is_enchanted
                and bool(props & ValueProp.MOVE)):
            return self.EXTRA_DAMAGE
        return 0


@register_relic
class NinjaScroll(RelicInstance):
    """Round 1: generate 3 Shivs in hand."""
    relic_id = RelicId.NINJA_SCROLL
    rarity = RelicRarity.SHOP
    pool = RelicPool.SILENT
    SHIVS = 3

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and combat.round_number == 1:
            create_shivs_in_hand = getattr(combat, "create_shivs_in_hand", None)
            if callable(create_shivs_in_hand):
                create_shivs_in_hand(owner, self.SHIVS)
            else:
                combat.add_shivs_to_hand(owner, self.SHIVS)


@register_relic
class Orrery(RelicInstance):
    """On obtain: offer 5 card rewards."""
    relic_id = RelicId.ORRERY
    rarity = RelicRarity.SHOP
    pool = RelicPool.SHARED
    CARDS = 5

    def after_obtained(self, owner: Creature) -> None:
        for _ in range(self.CARDS):
            owner.offer_card_reward()


@register_relic
class PunchDagger(RelicInstance):
    """On obtain: enchant 1 card with Momentum(5)."""
    relic_id = RelicId.PUNCH_DAGGER
    rarity = RelicRarity.SHOP
    pool = RelicPool.SHARED
    MOMENTUM = 5

    def after_obtained(self, owner: Creature) -> None:
        candidates = [card for card in owner.deck if can_enchant_card(card, "Momentum")]
        if getattr(owner.run_state, "defer_followup_rewards", False):
            owner.offer_enchant_cards_reward("Momentum", self.MOMENTUM, 1, cards=candidates)
            return
        owner.enchant_selected_cards("Momentum", self.MOMENTUM, 1, cards=candidates)


@register_relic
class RingingTriangle(RelicInstance):
    """Prevent hand flush on turn 1 (retain all cards)."""
    relic_id = RelicId.RINGING_TRIANGLE
    rarity = RelicRarity.SHOP
    pool = RelicPool.SHARED

    def should_flush(self, owner: Creature, combat: CombatState) -> bool | None:
        if combat.round_number == 1:
            return False
        return None


@register_relic
class RoyalStamp(RelicInstance):
    """On obtain: enchant 1 card with RoyallyApproved."""
    relic_id = RelicId.ROYAL_STAMP
    rarity = RelicRarity.SHOP
    pool = RelicPool.SHARED

    def after_obtained(self, owner: Creature) -> None:
        candidates = [card for card in owner.deck if can_enchant_card(card, "RoyallyApproved")]
        if getattr(owner.run_state, "defer_followup_rewards", False):
            owner.offer_enchant_cards_reward("RoyallyApproved", 1, 1, cards=candidates)
            return
        owner.enchant_selected_cards("RoyallyApproved", 1, 1, cards=candidates)


@register_relic
class RunicCapacitor(RelicInstance):
    """Round 1: add 3 orb slots."""
    relic_id = RelicId.RUNIC_CAPACITOR
    rarity = RelicRarity.SHOP
    pool = RelicPool.DEFECT
    ORB_SLOTS = 3

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and combat.round_number == 1:
            add_orb_slots = getattr(combat, "add_orb_slots", None)
            if callable(add_orb_slots):
                add_orb_slots(owner, self.ORB_SLOTS)
                return
            state = combat.combat_player_state_for(owner)
            orb_queue = getattr(state, "orb_queue", None) if state is not None else None
            if orb_queue is not None:
                add_capacity = getattr(orb_queue, "add_capacity", None)
                if callable(add_capacity):
                    add_capacity(self.ORB_SLOTS)


@register_relic
class ScreamingFlagon(RelicInstance):
    """If hand empty at end of turn, deal 20 damage to all enemies."""
    relic_id = RelicId.SCREAMING_FLAGON
    rarity = RelicRarity.SHOP
    pool = RelicPool.SHARED
    DAMAGE = 20

    def before_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and len(combat.hand) == 0:
            for enemy in combat.get_alive_enemies():
                combat.deal_damage(owner, enemy, self.DAMAGE, ValueProp.UNPOWERED)


@register_relic
class SlingOfCourage(RelicInstance):
    """Elite rooms: gain 2 Strength."""
    relic_id = RelicId.SLING_OF_COURAGE
    rarity = RelicRarity.SHOP
    pool = RelicPool.SHARED
    STRENGTH = 2

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        if getattr(combat, "is_elite", False):
            owner.apply_power(PowerId.STRENGTH, self.STRENGTH)


@register_relic
class TheAbacus(RelicInstance):
    """On shuffle: gain 6 block."""
    relic_id = RelicId.THE_ABACUS
    rarity = RelicRarity.SHOP
    pool = RelicPool.SHARED
    BLOCK = 6

    def after_shuffle(self, owner: Creature, combat: CombatState) -> None:
        owner.gain_block(self.BLOCK, unpowered=True)


@register_relic
class Toolbox(RelicInstance):
    """Round 1: offer 3 colorless cards, chosen one goes to hand."""
    relic_id = RelicId.TOOLBOX
    rarity = RelicRarity.SHOP
    pool = RelicPool.SHARED
    CARDS = 3

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._used_this_combat: bool = False

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        from sts2_env.cards.factory import create_cards_from_ids, eligible_registered_cards

        if side != CombatSide.PLAYER or combat.round_number != 1 or self._used_this_combat:
            return
        ids = eligible_registered_cards(
            module_name="sts2_env.cards.colorless",
            generation_context="combat",
        )
        cards = create_cards_from_ids(ids, combat.rng, self.CARDS, distinct=True)
        if not cards:
            return
        self._used_this_combat = True
        combat.request_card_choice(
            prompt="Choose a colorless card to add to your hand.",
            cards=cards,
            source_pile="generated",
            resolver=lambda card: combat.move_card_to_creature_hand(owner, card),
            allow_skip=False,
        )

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        self._used_this_combat = False

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        self._used_this_combat = False


@register_relic
class UndyingSigil(RelicInstance):
    """If enemy HP <= Doom, halve damage to owner."""
    relic_id = RelicId.UNDYING_SIGIL
    rarity = RelicRarity.SHOP
    pool = RelicPool.NECROBINDER
    MULTIPLIER = 0.5

    def modify_damage_multiplicative(
        self, owner: Creature, dealer: Creature | None, target: Creature,
        props: ValueProp, card: object | None = None
    ) -> float:
        if (target is owner
                and dealer is not None
                and hasattr(dealer, "doom") and hasattr(dealer, "current_hp")
                and dealer.current_hp <= dealer.doom):
            return self.MULTIPLIER
        return 1.0


@register_relic
class VitruvianMinion(RelicInstance):
    """Minion-tagged cards deal 2x damage and grant 2x block."""
    relic_id = RelicId.VITRUVIAN_MINION
    rarity = RelicRarity.SHOP
    pool = RelicPool.REGENT
    MULTIPLIER = 2.0

    def modify_damage_multiplicative(
        self, owner: Creature, dealer: Creature | None, target: Creature,
        props: ValueProp, card: object | None = None
    ) -> float:
        if (dealer is owner and card is not None
                and hasattr(card, "tags") and hasattr(CardType, "MINION")):
            from sts2_env.core.enums import CardTag
            if CardTag.MINION in card.tags:
                return self.MULTIPLIER
        return 1.0

    def modify_block_multiplicative(
        self, owner: Creature, target: Creature, props: ValueProp,
        card_source: object | None = None, card_play: object | None = None,
    ) -> float:
        if (target is owner and card_source is not None
                and hasattr(card, "tags")):
            from sts2_env.core.enums import CardTag
            if CardTag.MINION in card.tags:
                return self.MULTIPLIER
        return 1.0


@register_relic
class WingCharm(RelicInstance):
    """One random card reward gets Swift(1) enchantment."""
    relic_id = RelicId.WING_CHARM
    rarity = RelicRarity.SHOP
    pool = RelicPool.SHARED
    SWIFT = 1

    def modify_card_reward_options_late(
        self,
        owner: Creature,
        cards: list[CardInstance],
        reward: CardReward,
        room: Room | None,
        run_state: RunState,
    ) -> list[CardInstance]:
        if not cards:
            return cards
        index = run_state.rng.rewards.next_int(0, len(cards) - 1)
        cards[index].add_enchantment("Swift", self.SWIFT)
        return cards


# ═══════════════════════════════════════════════════════════════════════════
# EVENT / ANCIENT RELICS
# ═══════════════════════════════════════════════════════════════════════════


@register_relic
class AlchemicalCoffer(RelicInstance):
    """Gain 4 potion slots and fill with random potions."""
    relic_id = RelicId.ALCHEMICAL_COFFER
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    POTION_SLOTS = 4

    def after_obtained(self, owner: Creature) -> None:
        owner.gain_potion_slots(self.POTION_SLOTS)
        for _ in range(self.POTION_SLOTS):
            owner.procure_potion("random")


@register_relic
class ArcaneScroll(RelicInstance):
    """Add 1 random Rare card to deck."""
    relic_id = RelicId.ARCANE_SCROLL
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    def after_obtained(self, owner: Creature) -> None:
        if getattr(owner.run_state, "defer_followup_rewards", False):
            from sts2_env.cards.factory import create_card, eligible_character_cards

            candidates = eligible_character_cards(owner.character_id, rarity="rare", generation_context="modifier")
            if candidates:
                card = create_card(owner.run_state.rng.rewards.choice(candidates))
                owner.offer_add_cards_reward([card])
                return
        owner.add_random_card_to_deck("rare")


@register_relic
class ArchaicTooth(RelicInstance):
    """Transform starter card into ancient version."""
    relic_id = RelicId.ARCHAIC_TOOTH
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._starter_card_id: str | None = None
        self._ancient_card_id: str | None = None

    def setup_for_player(self, owner: Creature) -> bool:
        mapping = {
            CardId.BASH: CardId.BREAK,
            CardId.NEUTRALIZE: CardId.SUPPRESS,
            CardId.UNLEASH: CardId.PROTECTOR,
            CardId.FALLING_STAR: CardId.METEOR_SHOWER,
            CardId.DUALCAST: CardId.QUADCAST,
        }
        for card in owner.deck:
            target_id = mapping.get(card.card_id)
            if target_id is None:
                continue
            self._starter_card_id = card.card_id.name
            self._ancient_card_id = target_id.name
            return True
        return False

    def after_obtained(self, owner: Creature) -> None:
        mapping = {
            CardId.BASH: CardId.BREAK,
            CardId.NEUTRALIZE: CardId.SUPPRESS,
            CardId.UNLEASH: CardId.PROTECTOR,
            CardId.FALLING_STAR: CardId.METEOR_SHOWER,
            CardId.DUALCAST: CardId.QUADCAST,
        }
        if self._starter_card_id is not None and self._ancient_card_id is not None:
            starter_id = owner._coerce_card_id(self._starter_card_id)
            ancient_id = owner._coerce_card_id(self._ancient_card_id)
            if starter_id is not None and ancient_id is not None:
                cards = [card for card in owner.deck if card.card_id == starter_id][:1]
                if getattr(owner.run_state, "defer_followup_rewards", False):
                    owner.offer_transform_cards_reward(
                        1,
                        cards=cards,
                        transform_to=ancient_id.name,
                        preserve_upgrades=True,
                        preserve_enchantments=True,
                    )
                    return
                if cards:
                    owner.transform_cards_to(
                        ancient_id.name,
                        1,
                        cards=cards,
                        preserve_upgrades=True,
                        preserve_enchantments=True,
                    )
                    return
        cards = [card for card in owner.deck if card.card_id in mapping][:1]
        if getattr(owner.run_state, "defer_followup_rewards", False):
            owner.offer_transform_cards_reward(1, cards=cards, mapping=mapping)
            return
        if cards:
            owner.transform_specific_cards_with_mapping(cards, mapping)


@register_relic
class Astrolabe(RelicInstance):
    """Select 3 cards from deck to transform + upgrade."""
    relic_id = RelicId.ASTROLABE
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    CARDS = 3

    def after_obtained(self, owner: Creature) -> None:
        candidates = owner.transformable_deck_cards()
        if getattr(owner.run_state, "enable_deck_choice_requests", False):
            owner.offer_transform_cards_reward(self.CARDS, upgrade=True, cards=candidates)
            return
        owner.transform_and_upgrade_cards(self.CARDS, cards=candidates)


@register_relic
class BeautifulBracelet(RelicInstance):
    """Enchant 3 cards with Swift(3)."""
    relic_id = RelicId.BEAUTIFUL_BRACELET
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    CARDS = 3
    SWIFT = 3

    def after_obtained(self, owner: Creature) -> None:
        candidates = [card for card in owner.deck if can_enchant_card(card, "Swift")]
        if getattr(owner.run_state, "defer_followup_rewards", False):
            owner.offer_enchant_cards_reward("Swift", self.SWIFT, self.CARDS, cards=candidates)
            return
        owner.enchant_selected_cards("Swift", self.SWIFT, self.CARDS, cards=candidates)


@register_relic
class BigMushroom(RelicInstance):
    """Gain 20 max HP, draw -2 on round 1."""
    relic_id = RelicId.BIG_MUSHROOM
    rarity = RelicRarity.EVENT
    pool = RelicPool.EVENT
    MAX_HP = 20
    DRAW_PENALTY = 2

    def after_obtained(self, owner: Creature) -> None:
        owner.gain_max_hp(self.MAX_HP)

    def modify_hand_draw(self, owner: Creature, draw: int, combat: CombatState) -> int:
        if combat.round_number == 1:
            return max(0, draw - self.DRAW_PENALTY)
        return draw


@register_relic
class BiiigHug(RelicInstance):
    """Remove 4 cards from deck. On shuffle, add Soot to draw pile."""
    relic_id = RelicId.BIIIG_HUG
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    CARDS = 4

    def after_obtained(self, owner: Creature) -> None:
        candidates = owner.removable_deck_cards()
        if getattr(owner.run_state, "enable_deck_choice_requests", False):
            owner.offer_remove_card_reward(self.CARDS, cards=candidates)
            return
        owner.remove_cards_from_deck(self.CARDS, cards=candidates)

    def after_shuffle(self, owner: Creature, combat: CombatState) -> None:
        combat.add_card_to_draw_pile(owner, "Soot")


@register_relic
class BingBong(RelicInstance):
    """When card added to deck, duplicate it."""
    relic_id = RelicId.BING_BONG
    rarity = RelicRarity.EVENT
    pool = RelicPool.EVENT

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._skip_next: bool = False

    def on_card_added_to_deck(self, owner: Creature) -> None:
        if not self._skip_next:
            self._skip_next = True
            owner.duplicate_last_added_card()
            self._skip_next = False


@register_relic
class BlackStar(RelicInstance):
    """Extra relic reward after elite combats."""
    relic_id = RelicId.BLACK_STAR
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    def modify_rewards(
        self,
        owner: Creature,
        rewards: list[Reward],
        room: Room | None,
        run_state: RunState,
    ) -> list[Reward]:
        from sts2_env.core.enums import RoomType
        from sts2_env.run.reward_objects import RelicReward

        if room is not None and room.room_type == RoomType.ELITE:
            return [*rewards, RelicReward(owner.player_id)]
        return rewards


@register_relic
class BlessedAntler(RelicInstance):
    """+1 max energy, round 1 shuffle 3 Dazed into draw pile."""
    relic_id = RelicId.BLESSED_ANTLER
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    ENERGY = 1
    DAZED_COUNT = 3

    def modify_max_energy(self, owner: Creature, energy: int) -> int:
        return energy + self.ENERGY

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and combat.round_number == 1:
            for _ in range(self.DAZED_COUNT):
                combat.add_card_to_draw_pile(owner, "Dazed")


@register_relic
class BloodSoakedRose(RelicInstance):
    """+1 max energy, add Enthralled curse to deck."""
    relic_id = RelicId.BLOOD_SOAKED_ROSE
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    ENERGY = 1

    def modify_max_energy(self, owner: Creature, energy: int) -> int:
        return energy + self.ENERGY

    def after_obtained(self, owner: Creature) -> None:
        if getattr(owner.run_state, "defer_followup_rewards", False):
            from sts2_env.cards.factory import create_card

            card_id = owner._coerce_card_id("Enthralled")
            if card_id is not None:
                owner.offer_add_cards_reward([create_card(card_id)])
                return
        owner.add_card_to_deck("Enthralled")


@register_relic
class BoneTea(RelicInstance):
    """For 1 combat, upgrade all cards in hand round 1."""
    relic_id = RelicId.BONE_TEA
    rarity = RelicRarity.EVENT
    pool = RelicPool.EVENT
    COMBATS = 1

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._combats_left: int = self.COMBATS

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if (side == CombatSide.PLAYER and combat.round_number == 1
                and self._combats_left > 0):
            self._combats_left -= 1
            for card in list(combat.hand):
                if hasattr(card, "upgrade"):
                    card.upgrade()


@register_relic
class BoomingConch(RelicInstance):
    """Round 1 in elite: draw +2."""
    relic_id = RelicId.BOOMING_CONCH
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    EXTRA_CARDS = 2

    def modify_hand_draw(self, owner: Creature, draw: int, combat: CombatState) -> int:
        if combat.round_number == 1 and getattr(combat, "is_elite", False):
            return draw + self.EXTRA_CARDS
        return draw


@register_relic
class BrilliantScarf(RelicInstance):
    """5th card played each turn costs 0."""
    relic_id = RelicId.BRILLIANT_SCARF
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    CARD_NUMBER = 5

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._cards_this_turn: int = 0

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER:
            self._cards_this_turn = 0

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        self._cards_this_turn += 1

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        self._cards_this_turn = 0


@register_relic
class Byrdpip(RelicInstance):
    """Summon Byrdpip pet."""
    relic_id = RelicId.BYRDPIP
    rarity = RelicRarity.EVENT
    pool = RelicPool.EVENT
    # Pet system hook


@register_relic
class CallingBell(RelicInstance):
    """Add CurseOfTheBell to deck, offer 3 relic rewards."""
    relic_id = RelicId.CALLING_BELL
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    RELICS = 3

    def after_obtained(self, owner: Creature) -> None:
        if getattr(owner.run_state, "defer_followup_rewards", False):
            if not _queue_named_cards_reward(owner, "CurseOfTheBell"):
                owner.add_card_to_deck("CurseOfTheBell")
            owner.offer_relic_rewards(
                self.RELICS,
                rarities=(
                    RelicRarity.COMMON,
                    RelicRarity.UNCOMMON,
                    RelicRarity.RARE,
                ),
            )
            return
        owner.add_card_to_deck("CurseOfTheBell")
        owner.offer_relic_rewards(
            self.RELICS,
            rarities=(
                RelicRarity.COMMON,
                RelicRarity.UNCOMMON,
                RelicRarity.RARE,
            ),
        )


@register_relic
class ChoicesParadox(RelicInstance):
    """Round 1: offer 5 random cards with Retain, pick 1."""
    relic_id = RelicId.CHOICES_PARADOX
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    CARDS = 5

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._used_this_combat: bool = False

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        from sts2_env.cards.factory import create_distinct_character_cards

        if side != CombatSide.PLAYER or combat.round_number != 1 or self._used_this_combat:
            return
        state = combat.combat_player_state_for(owner)
        if state is None:
            return
        cards = create_distinct_character_cards(
            state.character_id,
            combat.rng,
            self.CARDS,
            generation_context="combat",
        )
        if not cards:
            return
        for card in cards:
            card.keywords = frozenset(set(card.keywords) | {"retain"})
        self._used_this_combat = True
        combat.request_card_choice(
            prompt="Choose a retained card to add to your hand.",
            cards=cards,
            source_pile="generated",
            resolver=lambda card: combat.move_card_to_creature_hand(owner, card),
            allow_skip=False,
        )

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        self._used_this_combat = False

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        self._used_this_combat = False


@register_relic
class ChosenCheese(RelicInstance):
    """Gain 1 max HP after every combat."""
    relic_id = RelicId.CHOSEN_CHEESE
    rarity = RelicRarity.EVENT
    pool = RelicPool.EVENT
    MAX_HP = 1

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        owner.gain_max_hp(self.MAX_HP)


@register_relic
class Claws(RelicInstance):
    """On obtain: transform up to 6 cards into Maul."""
    relic_id = RelicId.CLAWS
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    CARDS = 6

    def after_obtained(self, owner: Creature) -> None:
        owner.transform_cards_to(
            "Maul",
            self.CARDS,
            cards=owner.transformable_deck_cards(),
            preserve_upgrades=True,
            preserve_enchantments=True,
            min_count=0,
        )


@register_relic
class Crossbow(RelicInstance):
    """Each turn: generate 1 random 0-cost Attack in hand."""
    relic_id = RelicId.CROSSBOW
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER:
            combat.generate_free_attack_in_hand(owner)


@register_relic
class CursedPearl(RelicInstance):
    """Add Greed curse, gain 333 gold."""
    relic_id = RelicId.CURSED_PEARL
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    GOLD = 333

    def after_obtained(self, owner: Creature) -> None:
        if getattr(owner.run_state, "defer_followup_rewards", False):
            if not _queue_named_cards_reward(owner, "Greed"):
                owner.add_card_to_deck("Greed")
            if self.GOLD > 0:
                for relic in owner._ensure_relic_objects():
                    if relic.should_gain_gold(owner, self.GOLD) is False:
                        return
                owner.gold += self.GOLD
                for relic in owner._ensure_relic_objects():
                    on_gold_gained = getattr(relic, "on_gold_gained", None)
                    if callable(on_gold_gained):
                        on_gold_gained(owner, self.GOLD)
            return
        owner.add_card_to_deck("Greed")
        owner.gain_gold(self.GOLD)


@register_relic
class DarkstonePeriapt(RelicInstance):
    """When curse added to deck, gain 6 max HP."""
    relic_id = RelicId.DARKSTONE_PERIAPT
    rarity = RelicRarity.EVENT
    pool = RelicPool.EVENT
    MAX_HP = 6

    def on_card_added_to_deck(self, owner: Creature) -> None:
        # Check if last added card is curse type
        pass  # Handled by card-added-to-deck system


@register_relic
class DaughterOfTheWind(RelicInstance):
    """When Attack played, gain 1 block."""
    relic_id = RelicId.DAUGHTER_OF_THE_WIND
    rarity = RelicRarity.EVENT
    pool = RelicPool.EVENT
    BLOCK = 1

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        if hasattr(card, "card_type") and card.card_type == CardType.ATTACK:
            owner.gain_block(self.BLOCK, unpowered=True)


@register_relic
class DelicateFrond(RelicInstance):
    """Combat start: fill empty potion slots with random potions."""
    relic_id = RelicId.DELICATE_FROND
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        owner.fill_empty_potion_slots()


@register_relic
class DiamondDiadem(RelicInstance):
    """If <= 2 cards played this turn, gain stacking buff."""
    relic_id = RelicId.DIAMOND_DIADEM
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    CARD_THRESHOLD = 2

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._cards_this_turn: int = 0

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        self._cards_this_turn += 1

    def before_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and self._cards_this_turn <= self.CARD_THRESHOLD:
            owner.apply_power(PowerId.DIAMOND_DIADEM, 1)

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER:
            self._cards_this_turn = 0

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        self._cards_this_turn = 0


@register_relic
class DistinguishedCape(RelicInstance):
    """Lose 9 max HP, add 3 Apparition cards."""
    relic_id = RelicId.DISTINGUISHED_CAPE
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    HP_LOSS = 9
    CARDS = 3

    def after_obtained(self, owner: Creature) -> None:
        owner.lose_max_hp(self.HP_LOSS)
        if getattr(owner.run_state, "defer_followup_rewards", False):
            from sts2_env.cards.factory import create_card

            card_id = owner._coerce_card_id("Apparition")
            if card_id is not None:
                owner.offer_add_cards_reward([create_card(card_id) for _ in range(self.CARDS)])
                return
        for _ in range(self.CARDS):
            owner.add_card_to_deck("Apparition")


@register_relic
class DreamCatcher(RelicInstance):
    """Add card reward at rest site."""
    relic_id = RelicId.DREAM_CATCHER
    rarity = RelicRarity.EVENT
    pool = RelicPool.EVENT

    def modify_rest_site_heal_rewards(self, owner: Creature, rewards: list[object], run_state: RunState) -> list[object]:
        from sts2_env.run.reward_objects import CardReward

        return [*rewards, CardReward(owner.player_id)]


@register_relic
class Driftwood(RelicInstance):
    """Allow rerolling card rewards."""
    relic_id = RelicId.DRIFTWOOD
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    def allow_card_reward_reroll(
        self,
        owner: Creature,
        reward: CardReward,
        room: Room | None,
        run_state: RunState,
    ) -> bool:
        return True


@register_relic
class DustyTome(RelicInstance):
    """Add 1 random Ancient-rarity upgraded card to deck."""
    relic_id = RelicId.DUSTY_TOME
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._ancient_card_id: str | None = None

    def setup_for_player(self, owner: Creature) -> bool:
        from sts2_env.cards.factory import eligible_character_cards

        transcendence_cards = {
            CardId.BREAK.name,
            CardId.SUPPRESS.name,
            CardId.PROTECTOR.name,
            CardId.METEOR_SHOWER.name,
            CardId.QUADCAST.name,
        }
        candidates = [
            card_id
            for card_id in eligible_character_cards(owner.character_id, rarity="ancient", generation_context="modifier")
            if card_id.name not in transcendence_cards
        ]
        if not candidates:
            return False
        self._ancient_card_id = owner.run_state.rng.rewards.choice(candidates).name
        return True

    def after_obtained(self, owner: Creature) -> None:
        from sts2_env.cards.factory import create_card

        card_id = owner._coerce_card_id(self._ancient_card_id) if self._ancient_card_id is not None else None
        if card_id is not None:
            card = create_card(card_id, upgraded=True)
            if getattr(owner.run_state, "defer_followup_rewards", False):
                owner.offer_add_cards_reward([card])
                return
            owner.add_card_instance_to_deck(card)
            return
        if getattr(owner.run_state, "defer_followup_rewards", False):
            from sts2_env.cards.factory import eligible_character_cards

            candidates = eligible_character_cards(owner.character_id, rarity="ancient", generation_context="modifier")
            if candidates:
                card = create_card(owner.run_state.rng.rewards.choice(candidates), upgraded=True)
                owner.offer_add_cards_reward([card])
                return
        owner.add_random_card_to_deck("ancient", upgraded=True)


@register_relic
class Ectoplasm(RelicInstance):
    """Prevent all gold gain, +1 max energy."""
    relic_id = RelicId.ECTOPLASM
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    ENERGY = 1

    def should_gain_gold(self, owner: Creature, amount: int) -> bool | None:
        return False

    def modify_max_energy(self, owner: Creature, energy: int) -> int:
        return energy + self.ENERGY


@register_relic
class ElectricShrymp(RelicInstance):
    """Enchant 1 card with Imbued."""
    relic_id = RelicId.ELECTRIC_SHRYMP
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    def after_obtained(self, owner: Creature) -> None:
        candidates = [card for card in owner.deck if can_enchant_card(card, "Imbued")]
        if getattr(owner.run_state, "defer_followup_rewards", False):
            owner.offer_enchant_cards_reward("Imbued", 1, 1, cards=candidates)
            return
        owner.enchant_selected_cards("Imbued", 1, 1, cards=candidates)


@register_relic
class EmberTea(RelicInstance):
    """For 5 combats, gain 2 Strength at start."""
    relic_id = RelicId.EMBER_TEA
    rarity = RelicRarity.EVENT
    pool = RelicPool.EVENT
    COMBATS = 5
    STRENGTH = 2

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._combats_left: int = self.COMBATS

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        if self._combats_left > 0:
            self._combats_left -= 1
            owner.apply_power(PowerId.STRENGTH, self.STRENGTH)


@register_relic
class EmptyCage(RelicInstance):
    """Remove 2 cards from deck."""
    relic_id = RelicId.EMPTY_CAGE
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    CARDS = 2

    def after_obtained(self, owner: Creature) -> None:
        candidates = owner.removable_deck_cards()
        if getattr(owner.run_state, "enable_deck_choice_requests", False):
            owner.offer_remove_card_reward(self.CARDS, cards=candidates)
            return
        owner.remove_cards_from_deck(self.CARDS, cards=candidates)


@register_relic
class FakeAnchor(RelicInstance):
    """Gain 4 block at combat start."""
    relic_id = RelicId.FAKE_ANCHOR
    rarity = RelicRarity.EVENT
    pool = RelicPool.EVENT
    BLOCK = 4

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        owner.gain_block(self.BLOCK, unpowered=True)


@register_relic
class FakeBloodVial(RelicInstance):
    """Heal 1 on round 1."""
    relic_id = RelicId.FAKE_BLOOD_VIAL
    rarity = RelicRarity.EVENT
    pool = RelicPool.EVENT
    HEAL = 1

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and combat.round_number == 1:
            owner.heal(self.HEAL)


@register_relic
class FakeHappyFlower(RelicInstance):
    """Every 5 turns, gain 1 energy."""
    relic_id = RelicId.FAKE_HAPPY_FLOWER
    rarity = RelicRarity.EVENT
    pool = RelicPool.EVENT
    ENERGY = 1
    TURNS = 5

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._turns_seen: int = 0

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER:
            self._turns_seen += 1
            if self._turns_seen % self.TURNS == 0:
                combat.gain_energy(owner, self.ENERGY)


@register_relic
class FakeLeesWaffle(RelicInstance):
    """Heal 10% max HP on obtain."""
    relic_id = RelicId.FAKE_LEES_WAFFLE
    rarity = RelicRarity.EVENT
    pool = RelicPool.EVENT

    def after_obtained(self, owner: Creature) -> None:
        owner.heal(owner.max_hp // 10)


@register_relic
class FakeMango(RelicInstance):
    """Gain 3 max HP."""
    relic_id = RelicId.FAKE_MANGO
    rarity = RelicRarity.EVENT
    pool = RelicPool.EVENT
    MAX_HP = 3

    def after_obtained(self, owner: Creature) -> None:
        owner.gain_max_hp(self.MAX_HP)


@register_relic
class FakeMerchantsRug(RelicInstance):
    """No effect placeholder."""
    relic_id = RelicId.FAKE_MERCHANTS_RUG
    rarity = RelicRarity.EVENT
    pool = RelicPool.EVENT


@register_relic
class FakeOrichalcum(RelicInstance):
    """If 0 block at end of turn, gain 3 block."""
    relic_id = RelicId.FAKE_ORICHALCUM
    rarity = RelicRarity.EVENT
    pool = RelicPool.EVENT
    BLOCK = 3

    def before_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and owner.block == 0:
            owner.gain_block(self.BLOCK, unpowered=True)


@register_relic
class FakeSneckoEye(RelicInstance):
    """Apply Confused power."""
    relic_id = RelicId.FAKE_SNECKO_EYE
    rarity = RelicRarity.EVENT
    pool = RelicPool.EVENT

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        owner.apply_power(PowerId.CONFUSED, 1)


@register_relic
class FakeStrikeDummy(RelicInstance):
    """Strike cards deal +1 damage."""
    relic_id = RelicId.FAKE_STRIKE_DUMMY
    rarity = RelicRarity.EVENT
    pool = RelicPool.EVENT
    EXTRA_DAMAGE = 1

    def modify_damage_additive(
        self, owner: Creature, dealer: Creature | None, target: Creature,
        props: ValueProp, card: object | None = None
    ) -> int:
        if (dealer is owner and card is not None
                and hasattr(card, "tags")):
            from sts2_env.core.enums import CardTag
            if CardTag.STRIKE in card.tags:
                return self.EXTRA_DAMAGE
        return 0


@register_relic
class FakeVenerableTeaSet(RelicInstance):
    """After rest site, gain 1 energy next combat."""
    relic_id = RelicId.FAKE_VENERABLE_TEA_SET
    rarity = RelicRarity.EVENT
    pool = RelicPool.EVENT
    ENERGY = 1

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._gain_energy: bool = False

    def after_room_entered(self, owner: Creature, room_type: object) -> None:
        if hasattr(room_type, "is_rest_site") and room_type.is_rest_site:
            self._gain_energy = True

    def after_energy_reset(self, owner: Creature, combat: CombatState) -> None:
        if self._gain_energy:
            combat.gain_energy(owner, self.ENERGY)
            self._gain_energy = False


@register_relic
class Fiddle(RelicInstance):
    """+2 hand draw, block non-hand-draw draws."""
    relic_id = RelicId.FIDDLE
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    EXTRA_DRAW = 2

    def modify_hand_draw(self, owner: Creature, draw: int, combat: CombatState) -> int:
        return draw + self.EXTRA_DRAW


@register_relic
class ForgottenSoul(RelicInstance):
    """On card exhaust: deal 1 damage to random enemy."""
    relic_id = RelicId.FORGOTTEN_SOUL
    rarity = RelicRarity.EVENT
    pool = RelicPool.EVENT
    DAMAGE = 1

    def after_card_exhausted(self, owner: Creature, card: object, combat: CombatState) -> None:
        target = combat.get_random_enemy()
        if target:
            combat.deal_damage(owner, target, self.DAMAGE, ValueProp.UNPOWERED)


@register_relic
class FragrantMushroom(RelicInstance):
    """Take 15 damage, upgrade 3 random cards."""
    relic_id = RelicId.FRAGRANT_MUSHROOM
    rarity = RelicRarity.EVENT
    pool = RelicPool.EVENT
    DAMAGE = 15
    CARDS = 3

    def after_obtained(self, owner: Creature) -> None:
        owner.take_damage(self.DAMAGE)
        if getattr(owner.run_state, "defer_followup_rewards", False):
            owner.offer_upgrade_cards_reward(self.CARDS, cards=owner.upgradable_deck_cards())
            return
        owner.upgrade_random_cards(None, self.CARDS)


@register_relic
class FresnelLens(RelicInstance):
    """All card rewards get Nimble(2)."""
    relic_id = RelicId.FRESNEL_LENS
    rarity = RelicRarity.EVENT
    pool = RelicPool.EVENT
    NIMBLE = 2

    def modify_card_reward_options_late(
        self,
        owner: Creature,
        cards: list[CardInstance],
        reward: CardReward,
        room: Room | None,
        run_state: RunState,
    ) -> list[CardInstance]:
        for card in cards:
            card.add_enchantment("Nimble", self.NIMBLE)
        return cards

    def modify_card_being_added_to_deck(self, owner: Creature, card: CardInstance) -> CardInstance:
        card.add_enchantment("Nimble", self.NIMBLE)
        return card

    def modify_merchant_card_creation_results(
        self,
        owner: Creature,
        card: CardInstance,
        *,
        is_colorless: bool,
        run_state: RunState,
    ) -> CardInstance:
        card.add_enchantment("Nimble", self.NIMBLE)
        return card


@register_relic
class FurCoat(RelicInstance):
    """Mark 7 combat rooms; enemies there start at 1 HP."""
    relic_id = RelicId.FUR_COAT
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    COMBATS = 7
    # Map modification + BeforeCombatStart hook


@register_relic
class GlassEye(RelicInstance):
    """Offer 5 card rewards (2 common, 2 uncommon, 1 rare)."""
    relic_id = RelicId.GLASS_EYE
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    def after_obtained(self, owner: Creature) -> None:
        from sts2_env.run.reward_objects import CardReward

        for rarity in (
            CardRarity.COMMON,
            CardRarity.COMMON,
            CardRarity.UNCOMMON,
            CardRarity.UNCOMMON,
            CardRarity.RARE,
        ):
            owner.run_state.pending_rewards.append(
                CardReward(
                    owner.player_id,
                    option_count=3,
                    forced_rarities=(rarity, rarity, rarity),
                )
            )


@register_relic
class Glitter(RelicInstance):
    """All card rewards get Glam enchantment."""
    relic_id = RelicId.GLITTER
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    def modify_card_reward_options_late(
        self,
        owner: Creature,
        cards: list[CardInstance],
        reward: CardReward,
        room: Room | None,
        run_state: RunState,
    ) -> list[CardInstance]:
        for card in cards:
            card.add_enchantment("Glam", 1)
        return cards


@register_relic
class GoldenCompass(RelicInstance):
    """Replace map with all-events map."""
    relic_id = RelicId.GOLDEN_COMPASS
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    # Map modification hooks


@register_relic
class GoldenPearl(RelicInstance):
    """Gain 150 gold."""
    relic_id = RelicId.GOLDEN_PEARL
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    GOLD = 150

    def after_obtained(self, owner: Creature) -> None:
        owner.gain_gold(self.GOLD)


@register_relic
class HandDrill(RelicInstance):
    """When owner breaks enemy block, apply 2 Vulnerable."""
    relic_id = RelicId.HAND_DRILL
    rarity = RelicRarity.EVENT
    pool = RelicPool.EVENT
    VULNERABLE = 2

    def after_damage_given(
        self, owner: Creature, dealer: Creature, target: Creature,
        damage: int, props: ValueProp, combat: CombatState
    ) -> None:
        if dealer is not owner:
            return
        attack = getattr(combat, "active_attack", None)
        if attack is None:
            return
        result = next((entry for entry in reversed(attack.results) if entry.target is target), None)
        if result is None:
            return
        if result.blocked > 0 and target.block == 0:
            target.apply_power(PowerId.VULNERABLE, self.VULNERABLE)


@register_relic
class HistoryCourse(RelicInstance):
    """After round 1, auto-play last Attack/Skill from previous turn."""
    relic_id = RelicId.HISTORY_COURSE
    rarity = RelicRarity.EVENT
    pool = RelicPool.EVENT
    _DUPE_MARKER = "_history_course_dupe"

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._pending_replay_card: CardInstance | None = None

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        self._pending_replay_card = None

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        self._pending_replay_card = None

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side != CombatSide.PLAYER:
            return
        played_this_turn = getattr(combat, "_played_cards_this_turn", ())
        self._pending_replay_card = next(
            (
                card
                for card in reversed(played_this_turn)
                if getattr(card, "owner", None) is owner
                and getattr(card, "card_type", None) in {CardType.ATTACK, CardType.SKILL}
                and not bool(getattr(card, "is_dupe", False))
                and not bool(getattr(card, "combat_vars", {}).get(self._DUPE_MARKER, 0))
            ),
            None,
        )

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side != CombatSide.PLAYER or combat.round_number <= 1:
            return
        pending = self._pending_replay_card
        self._pending_replay_card = None
        if pending is None:
            return
        replay = pending.clone(combat.rng.next_int(1, 2**31 - 1))
        replay.owner = owner
        replay.combat_vars[self._DUPE_MARKER] = 1
        combat.auto_play_card(replay)


@register_relic
class IronClub(RelicInstance):
    """Every 4 cards played (persistent), draw 1."""
    relic_id = RelicId.IRON_CLUB
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    CARD_THRESHOLD = 4

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._cards_played: int = 0

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        self._cards_played += 1
        if self._cards_played % self.CARD_THRESHOLD == 0:
            combat.draw_cards(owner, 1)


@register_relic
class JeweledMask(RelicInstance):
    """Round 1: pull random Power from draw pile, make free."""
    relic_id = RelicId.JEWELED_MASK
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side != CombatSide.PLAYER or combat.round_number != 1:
            return
        state = combat.combat_player_state_for(owner)
        if state is None:
            return
        candidates = [card for card in state.draw if card.card_type == CardType.POWER]
        if not candidates:
            return
        selected = combat.rng.choice(candidates)
        selected.set_temporary_cost_for_turn(0)
        combat.move_card_to_creature_hand(owner, selected)


@register_relic
class JewelryBox(RelicInstance):
    """Add Apotheosis card to deck."""
    relic_id = RelicId.JEWELRY_BOX
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    def after_obtained(self, owner: Creature) -> None:
        if getattr(owner.run_state, "defer_followup_rewards", False):
            from sts2_env.cards.factory import create_card

            card_id = owner._coerce_card_id("Apotheosis")
            if card_id is not None:
                owner.offer_add_cards_reward([create_card(card_id)])
                return
        owner.add_card_to_deck("Apotheosis")


@register_relic
class LargeCapsule(RelicInstance):
    """Obtain 2 random relics + add Strike + Defend to deck."""
    relic_id = RelicId.LARGE_CAPSULE
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    RELICS = 2

    def after_obtained(self, owner: Creature) -> None:
        if getattr(owner.run_state, "defer_followup_rewards", False):
            rolled_relics: list[str] = []
            excluded: set[str] = set()
            for _ in range(self.RELICS):
                relic_id = owner.roll_relic_reward_id(excluded_relic_ids=excluded)
                if relic_id is None:
                    continue
                excluded.add(relic_id)
                rolled_relics.append(relic_id)
            owner.offer_obtain_relics_reward(self.RELICS, relic_ids=rolled_relics)
            if _queue_named_cards_reward(owner, "Strike", "Defend"):
                return
            owner.add_card_to_deck("Strike")
            owner.add_card_to_deck("Defend")
            return
        owner.obtain_random_relics(self.RELICS)
        owner.add_card_to_deck("Strike")
        owner.add_card_to_deck("Defend")


@register_relic
class LavaRock(RelicInstance):
    """After Act 1 boss: add 2 relic rewards (once)."""
    relic_id = RelicId.LAVA_ROCK
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    RELICS = 2

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._triggered: bool = False

    def modify_rewards(
        self,
        owner: Creature,
        rewards: list[Reward],
        room: Room | None,
        run_state: RunState,
    ) -> list[Reward]:
        from sts2_env.core.enums import RoomType
        from sts2_env.run.reward_objects import RelicReward

        if not self.enabled or self._triggered:
            return rewards
        if room is None or room.room_type != RoomType.BOSS:
            return rewards
        if run_state.current_act_index != 0:
            return rewards
        self._triggered = True
        self.enabled = False
        return [*rewards, *(RelicReward(owner.player_id) for _ in range(self.RELICS))]


@register_relic
class LeadPaperweight(RelicInstance):
    """Offer 2 random Colorless cards, choose 1."""
    relic_id = RelicId.LEAD_PAPERWEIGHT
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    def after_obtained(self, owner: Creature) -> None:
        owner.offer_colorless_cards(2)


@register_relic
class LeafyPoultice(RelicInstance):
    """Lose 10 max HP, transform 1 Strike and 1 Defend."""
    relic_id = RelicId.LEAFY_POULTICE
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    MAX_HP_LOSS = 10

    def after_obtained(self, owner: Creature) -> None:
        owner.lose_max_hp(self.MAX_HP_LOSS)
        strike = next((card for card in owner.basic_strike_defend_cards() if "STRIKE" in card.card_id.name), None)
        defend = next((card for card in owner.basic_strike_defend_cards() if "DEFEND" in card.card_id.name), None)
        cards = [card for card in (strike, defend) if card is not None]
        if getattr(owner.run_state, "defer_followup_rewards", False):
            owner.offer_transform_cards_reward(len(cards), cards=cards)
            return
        if cards:
            owner.transform_specific_cards(cards)


@register_relic
class LoomingFruit(RelicInstance):
    """Gain 31 max HP."""
    relic_id = RelicId.LOOMING_FRUIT
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    MAX_HP = 31

    def after_obtained(self, owner: Creature) -> None:
        owner.gain_max_hp(self.MAX_HP)


@register_relic
class LordsParasol(RelicInstance):
    """At merchant: auto-purchase everything for free."""
    relic_id = RelicId.LORDS_PARASOL
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    # AfterRoomEntered hook


@register_relic
class LostCoffer(RelicInstance):
    """Offer 1 card reward + 1 potion reward."""
    relic_id = RelicId.LOST_COFFER
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    def after_obtained(self, owner: Creature) -> None:
        owner.offer_card_reward()
        owner.offer_potion_reward()


@register_relic
class LostWisp(RelicInstance):
    """When Power played, deal 8 damage to all enemies."""
    relic_id = RelicId.LOST_WISP
    rarity = RelicRarity.EVENT
    pool = RelicPool.EVENT
    DAMAGE = 8

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        if hasattr(card, "card_type") and card.card_type == CardType.POWER:
            for enemy in combat.get_alive_enemies():
                combat.deal_damage(owner, enemy, self.DAMAGE, ValueProp.UNPOWERED)


@register_relic
class MassiveScroll(RelicInstance):
    """Offer 3 multiplayer cards, choose 1."""
    relic_id = RelicId.MASSIVE_SCROLL
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    def after_obtained(self, owner: Creature) -> None:
        owner.offer_multiplayer_cards(3)


@register_relic
class MawBank(RelicInstance):
    """Gain 12 gold per room until first purchase."""
    relic_id = RelicId.MAW_BANK
    rarity = RelicRarity.EVENT
    pool = RelicPool.EVENT
    GOLD = 12

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._purchased: bool = False

    def after_room_entered(self, owner: Creature, room_type: object) -> None:
        if not self._purchased:
            owner.gain_gold(self.GOLD)

    def on_item_purchased(self, owner: Creature) -> None:
        self._purchased = True


@register_relic
class MeatCleaver(RelicInstance):
    """Add Cook rest site option."""
    relic_id = RelicId.MEAT_CLEAVER
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    def modify_rest_site_options(self, owner: Creature, options: list[object], run_state: RunState) -> list[object]:
        from sts2_env.run.rest_site import CookOption

        removable_count = sum(1 for card in getattr(owner, "deck", []) if card.rarity.name not in ("STATUS", "CURSE"))
        if removable_count >= 2 and not any(getattr(option, "option_id", "") == "COOK" for option in options):
            options = [*options, CookOption(has_enough_removable=True)]
        return options


@register_relic
class MrStruggles(RelicInstance):
    """Each turn: deal damage = round number to all enemies."""
    relic_id = RelicId.MR_STRUGGLES
    rarity = RelicRarity.EVENT
    pool = RelicPool.EVENT

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER:
            damage = combat.round_number
            for enemy in combat.get_alive_enemies():
                combat.deal_damage(owner, enemy, damage, ValueProp.UNPOWERED)


@register_relic
class MusicBox(RelicInstance):
    """First Attack each turn: create Ethereal clone in hand."""
    relic_id = RelicId.MUSIC_BOX
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._used_this_turn: bool = False

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER:
            self._used_this_turn = False

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        if (not self._used_this_turn
                and hasattr(card, "card_type") and card.card_type == CardType.ATTACK):
            self._used_this_turn = True
            combat.create_ethereal_clone_in_hand(owner, card)

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        self._used_this_turn = False


@register_relic
class NeowsTorment(RelicInstance):
    """Add NeowsFury card to deck."""
    relic_id = RelicId.NEOWS_TORMENT
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    def after_obtained(self, owner: Creature) -> None:
        if getattr(owner.run_state, "defer_followup_rewards", False):
            from sts2_env.cards.factory import create_card
            owner.offer_add_cards_reward([create_card(CardId.NEOWS_FURY)])
            return
        owner.add_card_to_deck("NeowsFury")


@register_relic
class NewLeaf(RelicInstance):
    """Select 1 card to transform."""
    relic_id = RelicId.NEW_LEAF
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    def after_obtained(self, owner: Creature) -> None:
        candidates = owner.transformable_deck_cards()
        if getattr(owner.run_state, "enable_deck_choice_requests", False):
            owner.offer_transform_cards_reward(1, cards=candidates)
            return
        owner.transform_cards(1, cards=candidates)


@register_relic
class NutritiousOyster(RelicInstance):
    """Gain 11 max HP."""
    relic_id = RelicId.NUTRITIOUS_OYSTER
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    MAX_HP = 11

    def after_obtained(self, owner: Creature) -> None:
        owner.gain_max_hp(self.MAX_HP)


@register_relic
class NutritiousSoup(RelicInstance):
    """Enchant all basic Strikes with TezcatarasEmber."""
    relic_id = RelicId.NUTRITIOUS_SOUP
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    def after_obtained(self, owner: Creature) -> None:
        owner.enchant_basic_strikes("TezcatarasEmber")


@register_relic
class PaelsBlood(RelicInstance):
    """Draw +1 card every turn."""
    relic_id = RelicId.PAELS_BLOOD
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    EXTRA_DRAW = 1

    def modify_hand_draw(self, owner: Creature, draw: int, combat: CombatState) -> int:
        return draw + self.EXTRA_DRAW


@register_relic
class PaelsClaw(RelicInstance):
    """Enchant all valid cards with Goopy."""
    relic_id = RelicId.PAELS_CLAW
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    def after_obtained(self, owner: Creature) -> None:
        owner.enchant_all_cards("Goopy")


@register_relic
class PaelsEye(RelicInstance):
    """If no cards played: exhaust hand, take extra turn (once per combat)."""
    relic_id = RelicId.PAELS_EYE
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._used_this_combat: bool = False
        self._any_cards_played: bool = False

    def before_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        self._any_cards_played = True

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER:
            self._any_cards_played = False

    def should_take_extra_turn(self, owner: Creature, combat: CombatState) -> bool | None:
        if not self._used_this_combat and not self._any_cards_played:
            return True
        return None

    def before_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if self._used_this_combat or self._any_cards_played or side != CombatSide.PLAYER:
            return
        from sts2_env.core.hooks import fire_after_card_exhausted
        for card in list(combat.hand):
            combat.hand.remove(card)
            combat.exhaust_pile.append(card)
            fire_after_card_exhausted(card, combat)

    def after_taking_extra_turn(self, owner: Creature, combat: CombatState) -> None:
        self._used_this_combat = True

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        self._used_this_combat = False
        self._any_cards_played = False


@register_relic
class PaelsFlesh(RelicInstance):
    """From round 3 onward, gain 1 energy each turn."""
    relic_id = RelicId.PAELS_FLESH
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    ENERGY = 1

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and combat.round_number >= 3:
            combat.gain_energy(owner, self.ENERGY)


@register_relic
class PaelsGrowth(RelicInstance):
    """Enchant 1 card with Clone(4), add Clone rest option."""
    relic_id = RelicId.PAELS_GROWTH
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    def after_obtained(self, owner: Creature) -> None:
        candidates = [card for card in owner.deck if can_enchant_card(card, "Clone")]
        if getattr(owner.run_state, "defer_followup_rewards", False):
            owner.offer_enchant_cards_reward("Clone", 4, 1, cards=candidates)
            return
        owner.enchant_selected_cards("Clone", 4, 1, cards=candidates)

    def modify_rest_site_options(self, owner: Creature, options: list[object], run_state: RunState) -> list[object]:
        from sts2_env.run.rest_site import CloneOption

        if not any(getattr(option, "option_id", "") == "CLONE" for option in options):
            options = [*options, CloneOption()]
        return options


@register_relic
class PaelsHorn(RelicInstance):
    """Add 2 Relax cards to deck."""
    relic_id = RelicId.PAELS_HORN
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    def after_obtained(self, owner: Creature) -> None:
        if getattr(owner.run_state, "defer_followup_rewards", False):
            from sts2_env.cards.factory import create_card
            owner.offer_add_cards_reward([create_card(CardId.RELAX), create_card(CardId.RELAX)])
            return
        for _ in range(2):
            owner.add_card_to_deck("Relax")


@register_relic
class PaelsLegion(RelicInstance):
    """Summon pet, double block from first block card (2 turn cooldown)."""
    relic_id = RelicId.PAELS_LEGION
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._cooldown: int = 0


@register_relic
class PaelsTears(RelicInstance):
    """If leftover energy at end of turn, gain 2 energy next turn."""
    relic_id = RelicId.PAELS_TEARS
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    ENERGY = 2

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._had_leftover: bool = False

    def before_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER:
            self._had_leftover = getattr(combat, "current_energy", 0) > 0

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and self._had_leftover:
            combat.gain_energy(owner, self.ENERGY)
            self._had_leftover = False

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        self._had_leftover = False


@register_relic
class PaelsTooth(RelicInstance):
    """Remove up to 5 cards, return 1 upgraded per combat."""
    relic_id = RelicId.PAELS_TOOTH
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    CARDS = 5

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._stored_cards: list = []


@register_relic
class PaelsWing(RelicInstance):
    """Sacrifice card rewards; every 2, obtain random relic."""
    relic_id = RelicId.PAELS_WING
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    SACRIFICES = 2

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._rewards_sacrificed: int = 0


@register_relic
class PandorasBox(RelicInstance):
    """Transform all basic Strikes/Defends to random cards."""
    relic_id = RelicId.PANDORAS_BOX
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    def after_obtained(self, owner: Creature) -> None:
        cards = owner.basic_strike_defend_cards()
        if getattr(owner.run_state, "defer_followup_rewards", False):
            owner.offer_transform_cards_reward(len(cards), cards=cards)
            return
        if cards:
            owner.transform_specific_cards(cards)


@register_relic
class PhilosophersStone(RelicInstance):
    """+1 max energy, all enemies gain 1 Strength."""
    relic_id = RelicId.PHILOSOPHERS_STONE
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    ENERGY = 1
    ENEMY_STRENGTH = 1

    def modify_max_energy(self, owner: Creature, energy: int) -> int:
        return energy + self.ENERGY

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        for enemy in combat.get_alive_enemies():
            enemy.apply_power(PowerId.STRENGTH, self.ENEMY_STRENGTH)

    def after_creature_added_to_combat(self, owner: Creature, creature: Creature, combat: CombatState) -> None:
        if creature.side == owner.side:
            return
        creature.apply_power(PowerId.STRENGTH, self.ENEMY_STRENGTH)


@register_relic
class PollinousCore(RelicInstance):
    """Every 4 turns, draw +2 extra cards."""
    relic_id = RelicId.POLLINOUS_CORE
    rarity = RelicRarity.EVENT
    pool = RelicPool.EVENT
    EXTRA_CARDS = 2
    TURNS = 4

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._turns_seen: int = 0

    def modify_hand_draw(self, owner: Creature, draw: int, combat: CombatState) -> int:
        if self._turns_seen > 0 and self._turns_seen % self.TURNS == 0:
            return draw + self.EXTRA_CARDS
        return draw

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER:
            self._turns_seen += 1


@register_relic
class Pomander(RelicInstance):
    """Upgrade 1 card from deck."""
    relic_id = RelicId.POMANDER
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    def after_obtained(self, owner: Creature) -> None:
        candidates = owner.upgradable_deck_cards()
        if getattr(owner.run_state, "enable_deck_choice_requests", False):
            owner.offer_upgrade_cards_reward(1, cards=candidates)
            return
        owner.upgrade_selected_cards(1, cards=candidates)


@register_relic
class PrecariousShears(RelicInstance):
    """Remove 2 cards, take 13 damage."""
    relic_id = RelicId.PRECARIOUS_SHEARS
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    CARDS = 2
    DAMAGE = 13

    def after_obtained(self, owner: Creature) -> None:
        candidates = owner.removable_deck_cards()
        if getattr(owner.run_state, "enable_deck_choice_requests", False):
            owner.offer_remove_card_reward(self.CARDS, cards=candidates)
        else:
            owner.remove_cards_from_deck(self.CARDS, cards=candidates)
        owner.take_damage(self.DAMAGE)


@register_relic
class PreciseScissors(RelicInstance):
    """Remove 1 card from deck."""
    relic_id = RelicId.PRECISE_SCISSORS
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    def after_obtained(self, owner: Creature) -> None:
        candidates = owner.removable_deck_cards()
        if getattr(owner.run_state, "enable_deck_choice_requests", False):
            owner.offer_remove_card_reward(1, cards=candidates)
            return
        owner.remove_cards_from_deck(1, cards=candidates)


@register_relic
class PreservedFog(RelicInstance):
    """Remove 5 cards, add Folly curse."""
    relic_id = RelicId.PRESERVED_FOG
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    CARDS = 5

    def after_obtained(self, owner: Creature) -> None:
        candidates = owner.removable_deck_cards()
        if getattr(owner.run_state, "defer_followup_rewards", False):
            owner.offer_remove_card_reward(self.CARDS, cards=candidates)
            if not _queue_named_cards_reward(owner, "Folly"):
                owner.add_card_to_deck("Folly")
            return
        if getattr(owner.run_state, "enable_deck_choice_requests", False):
            owner.offer_remove_card_reward(self.CARDS, cards=candidates)
        else:
            owner.remove_cards_from_deck(self.CARDS, cards=candidates)
        owner.add_card_to_deck("Folly")


@register_relic
class PrismaticGem(RelicInstance):
    """+1 max energy, all character pools in card rewards."""
    relic_id = RelicId.PRISMATIC_GEM
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    ENERGY = 1

    def modify_max_energy(self, owner: Creature, energy: int) -> int:
        return energy + self.ENERGY

    def modify_card_reward_creation_options(
        self,
        owner: Creature,
        options: CardRewardGenerationOptions,
        reward: CardReward,
        room: Room | None,
        run_state: RunState,
    ) -> CardRewardGenerationOptions:
        from sts2_env.run.rewards import CardRewardGenerationOptions

        return CardRewardGenerationOptions(
            context=options.context,
            num_cards=options.num_cards,
            character_ids=tuple(character.character_id for character in ALL_CHARACTERS),
            forced_rarities=options.forced_rarities,
            include_colorless=options.include_colorless,
            use_default_character_pool=options.use_default_character_pool,
        )


@register_relic
class PumpkinCandle(RelicInstance):
    """+1 max energy in act obtained."""
    relic_id = RelicId.PUMPKIN_CANDLE
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    ENERGY = 1

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._active_act: int = -1

    def after_obtained(self, owner: Creature) -> None:
        self._active_act = getattr(owner, "current_act", 0)

    def modify_max_energy(self, owner: Creature, energy: int) -> int:
        if getattr(owner, "current_act", -1) == self._active_act:
            return energy + self.ENERGY
        return energy


@register_relic
class RadiantPearl(RelicInstance):
    """Round 1: add 1 Luminesce to hand."""
    relic_id = RelicId.RADIANT_PEARL
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and combat.round_number == 1:
            combat.create_card_in_hand(owner, "Luminesce")


@register_relic
class RoyalPoison(RelicInstance):
    """Round 1: deal 4 unblockable damage to self."""
    relic_id = RelicId.ROYAL_POISON
    rarity = RelicRarity.EVENT
    pool = RelicPool.EVENT
    DAMAGE = 4

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and combat.round_number == 1:
            combat.deal_damage(None, owner, self.DAMAGE,
                               ValueProp.UNBLOCKABLE | ValueProp.UNPOWERED)


@register_relic
class RunicPyramid(RelicInstance):
    """Retain all cards every turn."""
    relic_id = RelicId.RUNIC_PYRAMID
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    def should_flush(self, owner: Creature, combat: CombatState) -> bool | None:
        return False


@register_relic
class Sai(RelicInstance):
    """Gain 7 block every turn."""
    relic_id = RelicId.SAI
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    BLOCK = 7

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER:
            owner.gain_block(self.BLOCK, unpowered=True)


@register_relic
class SandCastle(RelicInstance):
    """Upgrade 6 random cards."""
    relic_id = RelicId.SAND_CASTLE
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    CARDS = 6

    def after_obtained(self, owner: Creature) -> None:
        if getattr(owner.run_state, "defer_followup_rewards", False):
            owner.offer_upgrade_cards_reward(self.CARDS, cards=owner.upgradable_deck_cards())
            return
        owner.upgrade_random_cards(None, self.CARDS)


@register_relic
class ScrollBoxes(RelicInstance):
    """Lose all gold, choose from 2 bundles of cards."""
    relic_id = RelicId.SCROLL_BOXES
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    @staticmethod
    def can_generate_bundles(owner: Creature) -> bool:
        from sts2_env.cards.factory import eligible_character_cards

        common_cards = eligible_character_cards(owner.character_id, rarity="common", generation_context="combat")
        uncommon_cards = eligible_character_cards(owner.character_id, rarity="uncommon", generation_context="combat")
        return len(common_cards) >= 4 and len(uncommon_cards) >= 2

    def after_obtained(self, owner: Creature) -> None:
        owner.lose_all_gold()
        owner.offer_card_bundles()


@register_relic
class SeaGlass(RelicInstance):
    """Offer 15 cards from assigned character pool."""
    relic_id = RelicId.SEA_GLASS
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    CARDS = 15

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._character_id: str | None = None

    def after_obtained(self, owner: Creature) -> None:
        if self._character_id is None:
            self._character_id = owner.run_state.rng.rewards.choice(
                [character.character_id for character in ALL_CHARACTERS]
            )
        owner.offer_custom_card_reward(
            option_count=self.CARDS,
            character_ids=(self._character_id,),
            forced_rarities=(
                (CardRarity.COMMON,) * 5
                + (CardRarity.UNCOMMON,) * 5
                + (CardRarity.RARE,) * 5
            ),
        )


@register_relic
class SealOfGold(RelicInstance):
    """If >= 5 gold, gain 1 energy and lose 5 gold each turn."""
    relic_id = RelicId.SEAL_OF_GOLD
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    ENERGY = 1
    GOLD_COST = 5

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER:
            gold = getattr(owner, "gold", 0)
            if gold >= self.GOLD_COST:
                owner.lose_gold(self.GOLD_COST)
                combat.gain_energy(owner, self.ENERGY)


@register_relic
class SereTalon(RelicInstance):
    """Add 2 random curses and 3 Wish cards."""
    relic_id = RelicId.SERE_TALON
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    def after_obtained(self, owner: Creature) -> None:
        if getattr(owner.run_state, "defer_followup_rewards", False):
            from sts2_env.cards.factory import create_card

            curse_ids = eligible_registered_cards(card_type=CardType.CURSE, generation_context=None)
            generated = [
                create_card(owner.run_state.rng.rewards.choice(curse_ids))
                for _ in range(2)
                if curse_ids
            ]
            wish_id = owner._coerce_card_id("Wish")
            if wish_id is not None:
                generated.extend(create_card(wish_id) for _ in range(3))
            if generated:
                owner.offer_add_cards_reward(generated)
                return
        owner.add_random_curses(2)
        for _ in range(3):
            owner.add_card_to_deck("Wish")


@register_relic
class SignetRing(RelicInstance):
    """Gain 999 gold."""
    relic_id = RelicId.SIGNET_RING
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    GOLD = 999

    def after_obtained(self, owner: Creature) -> None:
        owner.gain_gold(self.GOLD)


@register_relic
class SilverCrucible(RelicInstance):
    """First 3 card rewards: all pre-upgraded. Skip first treasure."""
    relic_id = RelicId.SILVER_CRUCIBLE
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    CARD_REWARDS = 3

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._times_used: int = 0
        self._treasure_rooms_entered: int = 0

    def _refresh_enabled(self) -> None:
        if self._times_used >= self.CARD_REWARDS and self._treasure_rooms_entered > 0:
            self.enabled = False

    def after_room_entered(self, owner: Creature, room_type: object) -> None:
        if hasattr(room_type, "room_type") and room_type.room_type.name == "TREASURE":
            self._treasure_rooms_entered += 1
            self._refresh_enabled()

    def should_generate_treasure(self, owner: Creature) -> bool | None:
        return self._treasure_rooms_entered > 1

    def modify_card_reward_options_late(
        self,
        owner: Creature,
        cards: list[CardInstance],
        reward: CardReward,
        room: Room | None,
        run_state: RunState,
    ) -> list[CardInstance]:
        if not self.enabled:
            return cards
        if getattr(reward, "_silver_crucible_upgraded", False) or self._times_used < self.CARD_REWARDS:
            if not getattr(reward, "_silver_crucible_upgraded", False):
                reward._silver_crucible_upgraded = True
                self._times_used += 1
                self._refresh_enabled()
            for card in cards:
                owner.upgrade_card_instance(card)
        return cards


@register_relic
class SmallCapsule(RelicInstance):
    """Offer 1 random relic reward."""
    relic_id = RelicId.SMALL_CAPSULE
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    def after_obtained(self, owner: Creature) -> None:
        owner.offer_relic_rewards(1)


@register_relic
class SneckoEye(RelicInstance):
    """Confused power + draw +2 per turn."""
    relic_id = RelicId.SNECKO_EYE
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    EXTRA_DRAW = 2

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        owner.apply_power(PowerId.CONFUSED, 1)

    def modify_hand_draw(self, owner: Creature, draw: int, combat: CombatState) -> int:
        return draw + self.EXTRA_DRAW


@register_relic
class Sozu(RelicInstance):
    """Prevent potion procurement, +1 max energy."""
    relic_id = RelicId.SOZU
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    ENERGY = 1

    def should_procure_potion(self, owner: Creature) -> bool | None:
        return False

    def modify_max_energy(self, owner: Creature, energy: int) -> int:
        return energy + self.ENERGY


@register_relic
class SpikedGauntlets(RelicInstance):
    """+1 max energy, Power cards cost +1."""
    relic_id = RelicId.SPIKED_GAUNTLETS
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    ENERGY = 1
    POWER_COST_INCREASE = 1

    def modify_max_energy(self, owner: Creature, energy: int) -> int:
        return energy + self.ENERGY

    def _is_non_x_power(self, card: object) -> bool:
        return (
            getattr(card, "card_type", None) == CardType.POWER
            and not getattr(card, "has_energy_cost_x", False)
        )

    def should_play(self, owner: Creature, card: object, combat: CombatState) -> bool | None:
        if not self._is_non_x_power(card):
            return None
        state = combat.combat_player_state_for(owner)
        if state is None:
            return None
        required_energy = max(0, int(getattr(card, "cost", 0))) + self.POWER_COST_INCREASE
        if state.energy < required_energy:
            return False
        return None

    def before_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        if not self._is_non_x_power(card):
            return
        pending = getattr(combat, "_pending_play", None)
        if isinstance(pending, dict):
            if pending.get("_spiked_gauntlets_paid", False):
                return
            pending["_spiked_gauntlets_paid"] = True
        state = combat.combat_player_state_for(owner)
        if state is None:
            return
        state.energy = max(0, state.energy - self.POWER_COST_INCREASE)
        if hasattr(card, "energy_spent"):
            card.energy_spent = int(getattr(card, "energy_spent", 0)) + self.POWER_COST_INCREASE


@register_relic
class StoneHumidifier(RelicInstance):
    """After rest site heal, gain 5 max HP."""
    relic_id = RelicId.STONE_HUMIDIFIER
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    MAX_HP = 5

    def after_rest_site_heal(self, owner: Creature, healed: int, run_state: RunState) -> None:
        if healed > 0:
            owner.gain_max_hp(self.MAX_HP)


@register_relic
class Storybook(RelicInstance):
    """Add BrightestFlame card to deck."""
    relic_id = RelicId.STORYBOOK
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    def after_obtained(self, owner: Creature) -> None:
        if getattr(owner.run_state, "defer_followup_rewards", False):
            from sts2_env.cards.factory import create_card
            owner.offer_add_cards_reward([create_card(CardId.BRIGHTEST_FLAME)])
            return
        owner.add_card_to_deck("BrightestFlame")


@register_relic
class SwordOfStone(RelicInstance):
    """Count elite victories; at 5, transform into SwordOfJade."""
    relic_id = RelicId.SWORD_OF_STONE
    rarity = RelicRarity.EVENT
    pool = RelicPool.EVENT
    ELITES_NEEDED = 5

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._elites_defeated: int = 0

    def after_combat_victory(self, owner: Creature, combat: CombatState) -> None:
        if getattr(combat, "is_elite", False):
            self._elites_defeated += 1
            if self._elites_defeated >= self.ELITES_NEEDED:
                owner.transform_relic(self, RelicId.SWORD_OF_JADE)


@register_relic
class SwordOfJade(RelicInstance):
    """Combat rooms: gain 3 Strength."""
    relic_id = RelicId.SWORD_OF_JADE
    rarity = RelicRarity.EVENT
    pool = RelicPool.EVENT
    STRENGTH = 3

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        owner.apply_power(PowerId.STRENGTH, self.STRENGTH)


@register_relic
class TanxsWhistle(RelicInstance):
    """Add Whistle card to deck."""
    relic_id = RelicId.TANXS_WHISTLE
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    def after_obtained(self, owner: Creature) -> None:
        if getattr(owner.run_state, "defer_followup_rewards", False):
            from sts2_env.cards.factory import create_card

            card_id = owner._coerce_card_id("Whistle")
            if card_id is not None:
                owner.offer_add_cards_reward([create_card(card_id)])
                return
        owner.add_card_to_deck("Whistle")


@register_relic
class TeaOfDiscourtesy(RelicInstance):
    """For 1 combat, add 2 Dazed to draw pile."""
    relic_id = RelicId.TEA_OF_DISCOURTESY
    rarity = RelicRarity.EVENT
    pool = RelicPool.EVENT
    COMBATS = 1
    DAZED_COUNT = 2

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._combats_left: int = self.COMBATS

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        if self._combats_left > 0:
            self._combats_left -= 1
            for _ in range(self.DAZED_COUNT):
                combat.add_card_to_draw_pile(owner, "Dazed")


@register_relic
class TheBoot(RelicInstance):
    """Owner's powered attacks deal min 5 damage."""
    relic_id = RelicId.THE_BOOT
    rarity = RelicRarity.EVENT
    pool = RelicPool.EVENT
    DAMAGE_MINIMUM = 5

    def modify_hp_lost(
        self, owner: Creature, target: Creature, amount: float,
        dealer: Creature | None, props: ValueProp
    ) -> float:
        if (dealer is owner
                and target is not owner
                and bool(props & ValueProp.MOVE)
                and not bool(props & ValueProp.UNPOWERED)
                and 0 < amount < self.DAMAGE_MINIMUM):
            return float(self.DAMAGE_MINIMUM)
        return amount


@register_relic
class ThrowingAxe(RelicInstance):
    """First card each combat: played twice."""
    relic_id = RelicId.THROWING_AXE
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._used_this_combat: bool = False

    def modify_card_play_count(self, owner: Creature, count: int, card: object) -> int:
        if not self._used_this_combat:
            self._used_this_combat = True
            return count + 1
        return count

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        self._used_this_combat = False

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        self._used_this_combat = False


@register_relic
class ToastyMittens(RelicInstance):
    """Each turn: exhaust top of draw pile, gain 1 Strength."""
    relic_id = RelicId.TOASTY_MITTENS
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    STRENGTH = 1

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER:
            combat.exhaust_top_of_draw_pile(owner)
            owner.apply_power(PowerId.STRENGTH, self.STRENGTH)


@register_relic
class TouchOfOrobas(RelicInstance):
    """Replace starter relic with upgraded version."""
    relic_id = RelicId.TOUCH_OF_OROBAS
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._starter_relic_id: str | None = None
        self._upgraded_relic_id: str | None = None

    def setup_for_player(self, owner: Creature) -> bool:
        mapping = {
            RelicId.BURNING_BLOOD.name: RelicId.BLACK_BLOOD.name,
            RelicId.RING_OF_THE_SNAKE.name: RelicId.RING_OF_THE_DRAKE.name,
            RelicId.CRACKED_CORE.name: RelicId.INFUSED_CORE.name,
            RelicId.BOUND_PHYLACTERY.name: RelicId.PHYLACTERY_UNBOUND.name,
            RelicId.DIVINE_RIGHT.name: RelicId.DIVINE_DESTINY.name,
        }
        for relic_id in owner.relics:
            upgraded = mapping.get(relic_id)
            if upgraded is None:
                continue
            self._starter_relic_id = relic_id
            self._upgraded_relic_id = upgraded
            return True
        return False

    def after_obtained(self, owner: Creature) -> None:
        if self._starter_relic_id is not None or self._upgraded_relic_id is not None:
            if owner.upgrade_starter_relic(self._starter_relic_id, self._upgraded_relic_id):
                return
        owner.upgrade_starter_relic()


@register_relic
class ToyBox(RelicInstance):
    """Offer 4 wax relics. Every 3 combats, melt one."""
    relic_id = RelicId.TOY_BOX
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    RELICS = 4
    COMBATS_PER_MELT = 3

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._combats_seen: int = 0

    def after_obtained(self, owner: Creature) -> None:
        from sts2_env.run.reward_objects import RelicReward

        for _ in range(self.RELICS):
            owner.run_state.pending_rewards.append(RelicReward(owner.player_id, is_wax=True))

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        if not self.enabled:
            return
        self._combats_seen += 1
        if self._combats_seen % self.COMBATS_PER_MELT != 0:
            return
        player_state_ref = combat.combat_player_state_for(owner)
        if player_state_ref is None:
            return
        for relic in player_state_ref.player_state.get_relic_objects():
            if relic is self:
                continue
            if getattr(relic, "is_wax", False) and not getattr(relic, "is_melted", False):
                relic.is_melted = True
                relic.enabled = False
                break
        if self._combats_seen >= self.COMBATS_PER_MELT * self.RELICS:
            self.enabled = False


@register_relic
class TriBoomerang(RelicInstance):
    """Enchant 3 cards with Instinct."""
    relic_id = RelicId.TRI_BOOMERANG
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    CARDS = 3

    def after_obtained(self, owner: Creature) -> None:
        candidates = [card for card in owner.deck if can_enchant_card(card, "Instinct")]
        if getattr(owner.run_state, "defer_followup_rewards", False):
            owner.offer_enchant_cards_reward("Instinct", 1, self.CARDS, cards=candidates)
            return
        owner.enchant_selected_cards("Instinct", 1, self.CARDS, cards=candidates)


@register_relic
class VelvetChoker(RelicInstance):
    """+1 max energy, max 6 cards per turn."""
    relic_id = RelicId.VELVET_CHOKER
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    ENERGY = 1
    MAX_CARDS = 6

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._cards_this_turn: int = 0

    def modify_max_energy(self, owner: Creature, energy: int) -> int:
        return energy + self.ENERGY

    def should_play(self, owner: Creature, card: object, combat: CombatState) -> bool | None:
        if self._cards_this_turn >= self.MAX_CARDS:
            return False
        return None

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        self._cards_this_turn += 1

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER:
            self._cards_this_turn = 0

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        self._cards_this_turn = 0


@register_relic
class VeryHotCocoa(RelicInstance):
    """Round 1: gain 4 energy."""
    relic_id = RelicId.VERY_HOT_COCOA
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    ENERGY = 4

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and combat.round_number == 1:
            combat.gain_energy(owner, self.ENERGY)


@register_relic
class WarHammer(RelicInstance):
    """After elite: upgrade 4 random cards."""
    relic_id = RelicId.WAR_HAMMER
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    CARDS = 4

    def after_combat_victory(self, owner: Creature, combat: CombatState) -> None:
        if not getattr(combat, "is_elite", False):
            return
        state = combat.combat_player_state_for(owner)
        if state is None:
            return
        player_state = state.player_state
        player_state.offer_upgrade_cards_reward(self.CARDS, cards=player_state.upgradable_deck_cards())


@register_relic
class WhisperingEarring(RelicInstance):
    """+1 max energy, round 1 auto-play all cards."""
    relic_id = RelicId.WHISPERING_EARRING
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    ENERGY = 1

    def modify_max_energy(self, owner: Creature, energy: int) -> int:
        return energy + self.ENERGY


@register_relic
class WongoCustomerAppreciationBadge(RelicInstance):
    """No effect."""
    relic_id = RelicId.WONGO_CUSTOMER_APPRECIATION_BADGE
    rarity = RelicRarity.EVENT
    pool = RelicPool.EVENT


@register_relic
class WongosMysteryTicket(RelicInstance):
    """After 5 combats, add 3 relic rewards next combat."""
    relic_id = RelicId.WONGOS_MYSTERY_TICKET
    rarity = RelicRarity.EVENT
    pool = RelicPool.EVENT
    COMBATS_NEEDED = 5
    RELICS = 3

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._combats_finished: int = 0
        self._gave_relic: bool = False

    def modify_rewards(
        self,
        owner: Creature,
        rewards: list[Reward],
        room: Room | None,
        run_state: RunState,
    ) -> list[Reward]:
        from sts2_env.core.enums import RoomType
        from sts2_env.run.reward_objects import RelicReward

        if self._gave_relic or self._combats_finished < self.COMBATS_NEEDED:
            return rewards
        if room is None or room.room_type not in {RoomType.MONSTER, RoomType.ELITE, RoomType.BOSS}:
            return rewards
        self._gave_relic = True
        return [*rewards, *(RelicReward(owner.player_id) for _ in range(self.RELICS))]

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        if not self._gave_relic:
            self._combats_finished += 1


@register_relic
class YummyCookie(RelicInstance):
    """Select 4 cards to upgrade."""
    relic_id = RelicId.YUMMY_COOKIE
    rarity = RelicRarity.ANCIENT
    pool = RelicPool.EVENT
    CARDS = 4

    def after_obtained(self, owner: Creature) -> None:
        candidates = owner.upgradable_deck_cards()
        if getattr(owner.run_state, "enable_deck_choice_requests", False):
            owner.offer_upgrade_cards_reward(self.CARDS, cards=candidates)
            return
        owner.upgrade_selected_cards(self.CARDS, cards=candidates)


# ═══════════════════════════════════════════════════════════════════════════
# SPECIAL RELICS
# ═══════════════════════════════════════════════════════════════════════════


@register_relic
class Circlet(RelicInstance):
    """Stackable placeholder when no other relics available."""
    relic_id = RelicId.CIRCLET
    rarity = RelicRarity.COMMON  # No specific rarity
    pool = RelicPool.FALLBACK


@register_relic
class DeprecatedRelic(RelicInstance):
    """Stackable placeholder for deprecated relics."""
    relic_id = RelicId.DEPRECATED_RELIC
    rarity = RelicRarity.COMMON
    pool = RelicPool.DEPRECATED
