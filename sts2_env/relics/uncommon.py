"""Uncommon relics (~32 total).

All Uncommon-rarity relics from the reference doc.
"""

from __future__ import annotations

from functools import lru_cache
from typing import TYPE_CHECKING

from sts2_env.core.constants import VULNERABLE_MULTIPLIER
from sts2_env.core.enums import (
    RelicRarity, CombatSide, CardType, OrbType, PowerId, ValueProp,
)
from sts2_env.relics.base import RelicId, RelicPool, RelicInstance
from sts2_env.relics.registry import register_relic

if TYPE_CHECKING:
    from sts2_env.core.creature import Creature
    from sts2_env.core.combat import CombatState


@lru_cache(maxsize=1)
def _colorless_card_ids() -> frozenset[object]:
    from sts2_env.cards.factory import eligible_registered_cards

    return frozenset(
        eligible_registered_cards(
            module_name="sts2_env.cards.colorless",
            generation_context=None,
        )
    )


@register_relic
class Akabeko(RelicInstance):
    """Round 1: gain 8 Vigor."""
    relic_id = RelicId.AKABEKO
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.SHARED
    VIGOR = 8

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and combat.round_number == 1:
            owner.apply_power(PowerId.VIGOR, self.VIGOR)


@register_relic
class BagOfMarbles(RelicInstance):
    """Round 1: apply 1 Vulnerable to all enemies."""
    relic_id = RelicId.BAG_OF_MARBLES
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.SHARED
    VULNERABLE = 1

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and combat.round_number == 1:
            for enemy in combat.get_alive_enemies():
                enemy.apply_power(PowerId.VULNERABLE, self.VULNERABLE)


@register_relic
class Bellows(RelicInstance):
    """Round 1: upgrade all cards in hand."""
    relic_id = RelicId.BELLOWS
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.SHARED

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side != owner.side or combat.round_number > 1:
            return
        state = combat.combat_player_state_for(owner)
        if state is None:
            return
        for card in list(state.hand):
            combat.upgrade_card(card)


@register_relic
class BookRepairKnife(RelicInstance):
    """When enemies die to Doom, heal 3 per enemy."""
    relic_id = RelicId.BOOK_REPAIR_KNIFE
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.NECROBINDER
    HEAL_PER = 3

    def after_died_to_doom(self, owner: Creature, creatures: list[Creature], combat: CombatState | None = None) -> None:
        combat_state = combat or getattr(owner, "combat_state", None)
        doomed = 0
        for creature in creatures:
            if creature is owner or not creature.is_dead:
                continue
            if combat_state is not None:
                if any(not power.should_owner_death_trigger_fatal(creature, combat_state) for power in creature.powers.values()):
                    continue
            doomed += 1
        if doomed > 0:
            owner.heal(self.HEAL_PER * doomed)


@register_relic
class BowlerHat(RelicInstance):
    """Gain 20% bonus gold on gold gains."""
    relic_id = RelicId.BOWLER_HAT
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.SHARED
    MULTIPLIER = 0.2

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._pending_bonus_gold: int = 0
        self._is_applying_bonus: bool = False

    def should_gain_gold(self, owner: Creature, amount: int) -> bool | None:
        if self._is_applying_bonus:
            return True
        self._pending_bonus_gold = int(amount * self.MULTIPLIER)
        return True

    def on_gold_gained(self, owner: Creature, amount: int) -> None:
        if self._is_applying_bonus or self._pending_bonus_gold <= 0:
            return
        bonus = self._pending_bonus_gold
        self._pending_bonus_gold = 0
        self._is_applying_bonus = True
        try:
            owner.gain_gold(bonus)
        finally:
            self._is_applying_bonus = False


@register_relic
class Candelabra(RelicInstance):
    """Round 2: gain 2 energy."""
    relic_id = RelicId.CANDELABRA
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.SHARED
    ENERGY = 2

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and combat.round_number == 2:
            combat.gain_energy(owner, self.ENERGY)


@register_relic
class EternalFeather(RelicInstance):
    """At rest sites, heal 3 * (deck_size / 5)."""
    relic_id = RelicId.ETERNAL_FEATHER
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.SHARED
    CARDS_PER = 5
    HEAL_PER = 3

    def after_room_entered(self, owner: Creature, room_type: object) -> None:
        if hasattr(room_type, "is_rest_site") and room_type.is_rest_site:
            deck_size = len(getattr(owner, "deck", []))
            heal = self.HEAL_PER * (deck_size // self.CARDS_PER)
            if heal > 0 and owner.current_hp > 0:
                owner.heal(heal)


@register_relic
class FuneraryMask(RelicInstance):
    """Round 1: create 3 Soul cards in draw pile."""
    relic_id = RelicId.FUNERARY_MASK
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.NECROBINDER
    CARDS = 3

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and combat.round_number == 1:
            from sts2_env.cards.status import make_soul

            for _ in range(self.CARDS):
                combat.insert_card_into_creature_draw_pile(owner, make_soul(), random_position=True)


@register_relic
class GalacticDust(RelicInstance):
    """Every 10 stars spent, gain 10 block."""
    relic_id = RelicId.GALACTIC_DUST
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.REGENT
    STARS_THRESHOLD = 10
    BLOCK = 10

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._stars_spent: int = 0

    def on_stars_spent(self, owner: Creature, amount: int, combat: CombatState) -> None:
        self._stars_spent += amount
        while self._stars_spent >= self.STARS_THRESHOLD:
            self._stars_spent -= self.STARS_THRESHOLD
            owner.gain_block(self.BLOCK, unpowered=True)


@register_relic
class GoldPlatedCables(RelicInstance):
    """First orb gets +1 passive trigger."""
    relic_id = RelicId.GOLD_PLATED_CABLES
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.DEFECT

    def _trigger_first_passive_if(self, owner: Creature, combat: CombatState, *, plasma: bool) -> None:
        state = combat.combat_player_state_for(owner)
        orb_queue = getattr(state, "orb_queue", None) if state is not None else None
        if orb_queue is None or not orb_queue.orbs:
            return
        first = orb_queue.orbs[0]
        if plasma and first.orb_type == OrbType.PLASMA:
            orb_queue.trigger_first_passive(combat)
        if not plasma and first.orb_type != OrbType.PLASMA:
            orb_queue.trigger_first_passive(combat)

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            self._trigger_first_passive_if(owner, combat, plasma=True)

    def before_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            self._trigger_first_passive_if(owner, combat, plasma=False)


@register_relic
class GremlinHorn(RelicInstance):
    """When enemy dies, gain 1 energy and draw 1."""
    relic_id = RelicId.GREMLIN_HORN
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.SHARED
    ENERGY = 1
    CARDS = 1

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
            return
        if target.side != CombatSide.ENEMY:
            return
        if not target.is_dead:
            return
        combat.gain_energy(owner, self.ENERGY)
        combat.draw_cards(owner, self.CARDS)


@register_relic
class HornCleat(RelicInstance):
    """Round 2: gain 14 block after block cleared."""
    relic_id = RelicId.HORN_CLEAT
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.SHARED
    BLOCK = 14

    def after_block_cleared(self, owner: Creature, creature: Creature, combat: CombatState) -> None:
        if creature is owner and combat.round_number == 2:
            owner.gain_block(self.BLOCK, unpowered=True)


@register_relic
class JossPaper(RelicInstance):
    """Every 5 cards exhausted, draw 1."""
    relic_id = RelicId.JOSS_PAPER
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.SHARED
    EXHAUST_THRESHOLD = 5
    CARDS = 1

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._cards_exhausted: int = 0
        self._ethereal_exhausted_this_turn: int = 0

    def _draw_if_threshold_met(self, owner: Creature, combat: CombatState) -> None:
        if self._cards_exhausted < self.EXHAUST_THRESHOLD:
            return
        draws = (self._cards_exhausted // self.EXHAUST_THRESHOLD) * self.CARDS
        self._cards_exhausted %= self.EXHAUST_THRESHOLD
        combat.draw_cards(owner, draws)

    def after_card_exhausted(self, owner: Creature, card: object, combat: CombatState) -> None:
        card_owner = getattr(card, "owner", None)
        if card_owner is not None and card_owner is not owner:
            return
        if getattr(card, "is_ethereal", False) and combat.current_side == CombatSide.PLAYER:
            self._ethereal_exhausted_this_turn += 1
            return
        self._cards_exhausted += 1
        self._draw_if_threshold_met(owner, combat)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side != CombatSide.PLAYER or self._ethereal_exhausted_this_turn <= 0:
            return
        self._cards_exhausted += self._ethereal_exhausted_this_turn
        self._ethereal_exhausted_this_turn = 0
        self._draw_if_threshold_met(owner, combat)

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        self._ethereal_exhausted_this_turn = 0


@register_relic
class Kusarigama(RelicInstance):
    """Every 3 attacks this turn, deal 6 damage to random enemy."""
    relic_id = RelicId.KUSARIGAMA
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.SHARED
    ATTACK_THRESHOLD = 3
    DAMAGE = 6

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
                target = combat.random_enemy_of(owner)
                if target:
                    combat.deal_damage(owner, target, self.DAMAGE, ValueProp.UNPOWERED)

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        self._attacks_this_turn = 0


@register_relic
class LetterOpener(RelicInstance):
    """Every 3 skills this turn, deal 5 damage to all enemies."""
    relic_id = RelicId.LETTER_OPENER
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.SHARED
    SKILL_THRESHOLD = 3
    DAMAGE = 5

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._skills_this_turn: int = 0

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER:
            self._skills_this_turn = 0

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        if hasattr(card, "card_type") and card.card_type == CardType.SKILL:
            self._skills_this_turn += 1
            if self._skills_this_turn % self.SKILL_THRESHOLD == 0:
                for enemy in combat.get_alive_enemies():
                    combat.deal_damage(owner, enemy, self.DAMAGE, ValueProp.UNPOWERED)

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        self._skills_this_turn = 0


@register_relic
class LuckyFysh(RelicInstance):
    """Gain 15 gold when card added to deck."""
    relic_id = RelicId.LUCKY_FYSH
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.SHARED
    GOLD = 15

    def on_card_added_to_deck(self, owner: Creature) -> None:
        owner.gain_gold(self.GOLD)


@register_relic
class MercuryHourglass(RelicInstance):
    """Every turn: deal 3 damage to all enemies."""
    relic_id = RelicId.MERCURY_HOURGLASS
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.SHARED
    DAMAGE = 3

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER:
            for enemy in combat.get_alive_enemies():
                combat.deal_damage(owner, enemy, self.DAMAGE, ValueProp.UNPOWERED)


@register_relic
class MiniatureCannon(RelicInstance):
    """Upgraded card attacks by owner deal +3 damage."""
    relic_id = RelicId.MINIATURE_CANNON
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.SHARED
    EXTRA_DAMAGE = 3

    def modify_damage_additive(
        self, owner: Creature, dealer: Creature | None, target: Creature,
        props: ValueProp, card: object | None = None
    ) -> int:
        if (dealer is owner
                and card is not None
                and hasattr(card, "card_type") and card.card_type == CardType.ATTACK
                and hasattr(card, "upgraded") and card.upgraded
                and bool(props & ValueProp.MOVE) and not bool(props & ValueProp.UNPOWERED)):
            return self.EXTRA_DAMAGE
        return 0


@register_relic
class Nunchaku(RelicInstance):
    """Every 10 attacks played, gain 1 energy."""
    relic_id = RelicId.NUNCHAKU
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.SHARED
    ATTACK_THRESHOLD = 10
    ENERGY = 1

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._attacks_played: int = 0

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        if hasattr(card, "card_type") and card.card_type == CardType.ATTACK:
            self._attacks_played += 1
            if self._attacks_played % self.ATTACK_THRESHOLD == 0:
                combat.gain_energy(owner, self.ENERGY)


@register_relic
class Orichalcum(RelicInstance):
    """If 0 block at end of turn, gain 6 block."""
    relic_id = RelicId.ORICHALCUM
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.SHARED
    BLOCK = 6

    def before_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and owner.block == 0:
            owner.gain_block(self.BLOCK, unpowered=True)


@register_relic
class OrnamentalFan(RelicInstance):
    """Every 3 attacks this turn, gain 4 block."""
    relic_id = RelicId.ORNAMENTAL_FAN
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.SHARED
    ATTACK_THRESHOLD = 3
    BLOCK = 4

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
                owner.gain_block(self.BLOCK, unpowered=True)

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        self._attacks_this_turn = 0


@register_relic
class Pantograph(RelicInstance):
    """Heal 25 at boss rooms."""
    relic_id = RelicId.PANTOGRAPH
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.SHARED
    HEAL = 25

    def after_room_entered(self, owner: Creature, room_type: object) -> None:
        if hasattr(room_type, "is_boss") and room_type.is_boss:
            if owner.current_hp > 0:
                owner.heal(self.HEAL)


@register_relic
class PaperPhrog(RelicInstance):
    """Enemies take +25% more damage from Vulnerable."""
    relic_id = RelicId.PAPER_PHROG
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.IRONCLAD
    EXTRA_VULNERABLE = 0.25

    def modify_damage_multiplicative(
        self,
        owner: Creature,
        dealer: Creature | None,
        target: Creature,
        props: ValueProp,
        card: object | None = None,
    ) -> float:
        if dealer is not owner or target is owner or not props.is_powered():
            return 1.0
        if target.get_power_amount(PowerId.VULNERABLE) <= 0:
            return 1.0

        vulnerable_multiplier = VULNERABLE_MULTIPLIER
        cruelty = dealer.powers.get(PowerId.CRUELTY)
        if cruelty is not None:
            vulnerable_multiplier += cruelty.amount / 100.0
        if target.has_power(PowerId.DEBILITATE):
            vulnerable_multiplier += vulnerable_multiplier - 1.0

        return (vulnerable_multiplier + self.EXTRA_VULNERABLE) / vulnerable_multiplier


@register_relic
class ParryingShield(RelicInstance):
    """If >= 10 block at end of turn, deal 6 damage to random enemy."""
    relic_id = RelicId.PARRYING_SHIELD
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.SHARED
    BLOCK_THRESHOLD = 10
    DAMAGE = 6

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and owner.block >= self.BLOCK_THRESHOLD:
            target = combat.random_enemy_of(owner)
            if target:
                combat.deal_damage(owner, target, self.DAMAGE, ValueProp.UNPOWERED)


@register_relic
class Pear(RelicInstance):
    """Gain 10 max HP on obtain."""
    relic_id = RelicId.PEAR
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.SHARED
    MAX_HP = 10

    def after_obtained(self, owner: Creature) -> None:
        owner.gain_max_hp(self.MAX_HP)


@register_relic
class PenNib(RelicInstance):
    """Every 10th attack: double damage."""
    relic_id = RelicId.PEN_NIB
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.SHARED
    THRESHOLD = 10

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._attacks_played: int = 0
        self._is_active: bool = False

    def before_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        if hasattr(card, "card_type") and card.card_type == CardType.ATTACK:
            self._attacks_played += 1
            self._is_active = (self._attacks_played % self.THRESHOLD == 0)

    def modify_damage_multiplicative(
        self, owner: Creature, dealer: Creature | None, target: Creature,
        props: ValueProp, card: object | None = None
    ) -> float:
        if dealer is owner and self._is_active:
            return 2.0
        return 1.0

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        self._is_active = False


@register_relic
class PetrifiedToad(RelicInstance):
    """At combat start, procure a PotionShapedRock."""
    relic_id = RelicId.PETRIFIED_TOAD
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.SHARED

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        if hasattr(owner, "procure_potion"):
            owner.procure_potion("PotionShapedRock")
            return
        import sts2_env.potions  # noqa: F401
        from sts2_env.potions.base import create_potion

        combat.add_potion(create_potion("PotionShapedRock"), owner=owner)


@register_relic
class Planisphere(RelicInstance):
    """Heal 4 at unknown rooms."""
    relic_id = RelicId.PLANISPHERE
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.SHARED
    HEAL = 4

    def after_room_entered(self, owner: Creature, room_type: object) -> None:
        if owner.current_hp <= 0:
            return
        if hasattr(room_type, "is_unknown") and room_type.is_unknown:
            owner.heal(self.HEAL)
            return
        run_state = getattr(owner, "run_state", None)
        visited = getattr(run_state, "visited_map_coords", None)
        act_map = getattr(run_state, "map", None)
        if act_map is None or not visited:
            return
        current = act_map.get_point(visited[-1])
        if current is not None and getattr(getattr(current, "point_type", None), "name", None) == "UNKNOWN":
            owner.heal(self.HEAL)


@register_relic
class RedMask(RelicInstance):
    """Round 1: apply 1 Weak to all enemies."""
    relic_id = RelicId.RED_MASK
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.SHARED
    WEAK = 1

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and combat.round_number == 1:
            for enemy in combat.get_alive_enemies():
                enemy.apply_power(PowerId.WEAK, self.WEAK)


@register_relic
class Regalite(RelicInstance):
    """When colorless card enters combat, gain 2 block."""
    relic_id = RelicId.REGALITE
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.REGENT
    BLOCK = 2

    def after_card_entered_combat(self, owner: Creature, card: object, combat: CombatState) -> None:
        if getattr(card, "owner", None) is not owner:
            return
        visual_card_pool = getattr(card, "visual_card_pool", None)
        is_colorless = bool(
            getattr(card, "is_colorless", False)
            or getattr(visual_card_pool, "is_colorless", False)
            or getattr(card, "card_id", None) in _colorless_card_ids()
        )
        if is_colorless:
            owner.gain_block(self.BLOCK, unpowered=True)


@register_relic
class ReptileTrinket(RelicInstance):
    """After potion used, gain 3 temporary Strength."""
    relic_id = RelicId.REPTILE_TRINKET
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.SHARED
    STRENGTH = 3

    def after_potion_used(
        self,
        owner: Creature,
        potion: object,
        target: Creature | None,
        combat: CombatState,
    ) -> None:
        if getattr(potion, "owner", owner) is not owner or combat.is_over:
            return
        owner.apply_power(PowerId.STRENGTH, self.STRENGTH)
        owner.apply_power(PowerId.REPTILE_TRINKET, self.STRENGTH)


@register_relic
class RippleBasin(RelicInstance):
    """If no attacks played this turn, gain 4 block at end of turn."""
    relic_id = RelicId.RIPPLE_BASIN
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.SHARED
    BLOCK = 4

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._attacks_played_this_turn: int = 0

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER:
            self._attacks_played_this_turn = 0

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        if hasattr(card, "card_type") and card.card_type == CardType.ATTACK:
            self._attacks_played_this_turn += 1

    def before_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and self._attacks_played_this_turn == 0:
            owner.gain_block(self.BLOCK, unpowered=True)

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        self._attacks_played_this_turn = 0


@register_relic
class SelfFormingClay(RelicInstance):
    """When taking unblocked damage, gain 3 block next turn."""
    relic_id = RelicId.SELF_FORMING_CLAY
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.IRONCLAD
    BLOCK_NEXT_TURN = 3

    def after_damage_received(
        self, owner: Creature, target: Creature, dealer: Creature | None,
        damage: int, props: ValueProp, combat: CombatState
    ) -> None:
        if target is owner and damage > 0:
            owner.apply_power(PowerId.BLOCK_NEXT_TURN, self.BLOCK_NEXT_TURN)


@register_relic
class SparklingRouge(RelicInstance):
    """Round 3 block clear: gain 1 Str + 1 Dex."""
    relic_id = RelicId.SPARKLING_ROUGE
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.SHARED
    STRENGTH = 1
    DEXTERITY = 1

    def after_block_cleared(self, owner: Creature, creature: Creature, combat: CombatState) -> None:
        if creature is owner and combat.round_number == 3:
            owner.apply_power(PowerId.STRENGTH, self.STRENGTH)
            owner.apply_power(PowerId.DEXTERITY, self.DEXTERITY)


@register_relic
class StoneCracker(RelicInstance):
    """Boss room: upgrade 3 random cards in draw pile."""
    relic_id = RelicId.STONE_CRACKER
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.SHARED
    CARDS = 3

    def after_room_entered(self, owner: Creature, room_type: object) -> None:
        if hasattr(room_type, "is_boss") and room_type.is_boss and hasattr(owner, "upgrade_random_cards"):
            owner.upgrade_random_cards(None, self.CARDS)


@register_relic
class TuningFork(RelicInstance):
    """Every 10 skills played, gain 7 block."""
    relic_id = RelicId.TUNING_FORK
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.SHARED
    SKILL_THRESHOLD = 10
    BLOCK = 7

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._skills_played: int = 0

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        if hasattr(card, "card_type") and card.card_type == CardType.SKILL:
            self._skills_played += 1
            if self._skills_played % self.SKILL_THRESHOLD == 0:
                owner.gain_block(self.BLOCK, unpowered=True)


@register_relic
class Tingsha(RelicInstance):
    """When card discarded on player turn, deal 3 damage to random enemy."""
    relic_id = RelicId.TINGSHA
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.SILENT
    DAMAGE = 3

    def after_card_discarded(self, owner: Creature, card: object, combat: CombatState) -> None:
        if combat.current_side == CombatSide.PLAYER:
            target = combat.random_enemy_of(owner)
            if target:
                combat.deal_damage(owner, target, self.DAMAGE, ValueProp.UNPOWERED)


@register_relic
class Vambrace(RelicInstance):
    """First block-granting card in combat: double block (once per combat)."""
    relic_id = RelicId.VAMBRACE
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.SHARED

    def __init__(self, relic_id: RelicId):
        super().__init__(relic_id)
        self._block_gained_this_combat: bool = False

    def modify_block_multiplicative(
        self, owner: Creature, target: Creature, props: ValueProp,
        card_source: object | None = None, card_play: object | None = None,
    ) -> float:
        if (target is owner and not self._block_gained_this_combat
                and card_source is not None):
            self._block_gained_this_combat = True
            return 2.0
        return 1.0

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        self._block_gained_this_combat = False

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        self._block_gained_this_combat = False


@register_relic
class SymbioticVirus(RelicInstance):
    """Round 1: channel 1 Dark orb."""
    relic_id = RelicId.SYMBIOTIC_VIRUS
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.DEFECT
    DARK_ORBS = 1

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and combat.round_number == 1:
            for _ in range(self.DARK_ORBS):
                combat.channel_orb(owner, "Dark")


@register_relic
class TwistedFunnel(RelicInstance):
    """Round 1: apply 4 Poison to all enemies."""
    relic_id = RelicId.TWISTED_FUNNEL
    rarity = RelicRarity.UNCOMMON
    pool = RelicPool.SHARED
    POISON = 4

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER and combat.round_number == 1:
            for enemy in combat.get_alive_enemies():
                enemy.apply_power(PowerId.POISON, self.POISON)
