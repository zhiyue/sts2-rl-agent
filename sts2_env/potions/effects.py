"""Potion use-effect implementations for all 63 potions.

Each effect function has the signature:
    (combat: CombatState, user: Creature, target: Creature | None) -> None

Matches the OnUse methods from decompiled MegaCrit.Sts2.Core.Models.Potions.
"""

from __future__ import annotations

import random
from typing import TYPE_CHECKING

from sts2_env.core.enums import (
    CardType, PowerId, ValueProp, PotionTargetType,
)
from sts2_env.core.damage import calculate_damage, apply_damage
from sts2_env.potions.base import register_potion_effect

if TYPE_CHECKING:
    from sts2_env.core.creature import Creature
    from sts2_env.core.combat import CombatState


# ─── Helpers ──────────────────────────────────────────────────────────

def _deal_unpowered_damage(
    combat: CombatState, dealer: Creature, target: Creature, base_damage: int,
) -> None:
    """Deal unpowered damage (no Str/Dex scaling) to a single target."""
    final = calculate_damage(base_damage, dealer, target, ValueProp.UNPOWERED, combat)
    apply_damage(target, final, ValueProp.UNPOWERED, combat=combat, dealer=dealer)


def _deal_unpowered_damage_all(
    combat: CombatState, dealer: Creature, base_damage: int,
) -> None:
    """Deal unpowered damage to all alive enemies."""
    for enemy in list(combat.alive_enemies):
        _deal_unpowered_damage(combat, dealer, enemy, base_damage)


# =====================================================================
#  COMMON POTIONS (20)
# =====================================================================

def _attack_potion(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Generate 3 random Attack cards; add one with cost 0 this turn to hand.

    RL simplification: generate 1 random Attack card (cost 0) into hand.
    """
    # Card generation requires the card factory subsystem; approximate
    # by drawing 1 card as a stand-in.
    combat._draw_cards(1)  # noqa: SLF001  # approximate card generation


def _block_potion(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Target gains 12 block (unpowered)."""
    t = target if target is not None else user
    t.gain_block(12)


def _blood_potion(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Heal target for 20% of max HP."""
    t = target if target is not None else user
    amount = t.max_hp * 20 // 100
    t.heal(amount)


def _colorless_potion(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Generate a random Colorless card (cost 0) in hand.

    RL simplification: draw 1 card.
    """
    combat._draw_cards(1)  # noqa: SLF001


def _dexterity_potion(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Target gains 2 Dexterity."""
    t = target if target is not None else user
    t.apply_power(PowerId.DEXTERITY, 2)


def _energy_potion(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Target gains 2 energy."""
    combat.energy += 2


def _explosive_ampoule(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Deal 10 unpowered damage to ALL enemies."""
    _deal_unpowered_damage_all(combat, user, 10)


def _fire_potion(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Deal 20 unpowered damage to target enemy."""
    if target is not None:
        _deal_unpowered_damage(combat, user, target, 20)


def _flex_potion(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Target gains 5 temporary Strength (FlexPotion power removes at end of turn)."""
    t = target if target is not None else user
    t.apply_power(PowerId.FLEX_POTION, 5)


def _focus_potion(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """User gains 2 Focus."""
    user.apply_power(PowerId.FOCUS, 2)


def _poison_potion(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Apply 6 Poison to target enemy."""
    if target is not None:
        target.apply_power(PowerId.POISON, 6)


def _potion_of_doom(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Apply 33 Doom to target enemy."""
    if target is not None:
        target.apply_power(PowerId.DOOM, 33)


def _power_potion(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Generate a random Power card (cost 0) in hand.

    RL simplification: draw 1 card.
    """
    combat._draw_cards(1)  # noqa: SLF001


def _skill_potion(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Generate a random Skill card (cost 0) in hand.

    RL simplification: draw 1 card.
    """
    combat._draw_cards(1)  # noqa: SLF001


def _speed_potion(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Target gains 5 temporary Dexterity (SpeedPotion power removes at end of turn)."""
    t = target if target is not None else user
    t.apply_power(PowerId.SPEED_POTION, 5)


def _star_potion(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """User gains 3 stars.

    Stars are a Regent mechanic; calls gain_stars if available.
    """
    if hasattr(user, "gain_stars"):
        user.gain_stars(3)


def _strength_potion(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Target gains 2 Strength."""
    t = target if target is not None else user
    t.apply_power(PowerId.STRENGTH, 2)


def _swift_potion(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Target draws 3 cards."""
    combat._draw_cards(3)  # noqa: SLF001


def _vulnerable_potion(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Apply 3 Vulnerable to target enemy."""
    if target is not None:
        combat.apply_power_to(target, PowerId.VULNERABLE, 3)


def _weak_potion(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Apply 3 Weak to target enemy."""
    if target is not None:
        combat.apply_power_to(target, PowerId.WEAK, 3)


# =====================================================================
#  UNCOMMON POTIONS (22)
# =====================================================================

def _ashwater(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Exhaust any number of cards from hand.

    RL simplification: exhaust all cards currently in hand.
    """
    exhausted = list(combat.hand)
    for card in exhausted:
        combat.hand.remove(card)
        combat.exhaust_pile.append(card)


def _blessing_of_the_forge(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Upgrade all upgradable cards in hand."""
    for card in list(combat.hand):
        if hasattr(card, "can_upgrade") and card.can_upgrade():
            card.upgrade()


def _bone_brew(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Summon Osty with 15 HP.

    Necrobinder mechanic; calls summon_osty if available.
    """
    if hasattr(combat, "summon_osty"):
        combat.summon_osty(user, 15)


def _clarity(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Target draws 1 card and gains 3 Clarity (retain cards)."""
    t = target if target is not None else user
    combat._draw_cards(1)  # noqa: SLF001
    t.apply_power(PowerId.CLARITY, 3)


def _cunning_potion(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Add 3 upgraded Shivs to hand.

    Uses card generation if available; otherwise adds 3 draws as approximation.
    """
    if hasattr(combat, "create_cards_in_hand"):
        combat.create_cards_in_hand(user, "ShivUpgraded", 3)
    else:
        combat._draw_cards(3)  # noqa: SLF001


def _cure_all(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Target gains 1 energy and draws 2 cards."""
    combat.energy += 1
    combat._draw_cards(2)  # noqa: SLF001


def _duplicator(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Gain 1 Duplication (next card is played twice)."""
    user.apply_power(PowerId.DUPLICATION, 1)


def _fortifier(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Double target's current block."""
    t = target if target is not None else user
    t.gain_block(t.block)


def _fysh_oil(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Target gains 1 Strength and 1 Dexterity."""
    t = target if target is not None else user
    t.apply_power(PowerId.STRENGTH, 1)
    t.apply_power(PowerId.DEXTERITY, 1)


def _gamblers_brew(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Discard any number of cards and draw that many.

    RL simplification: discard entire hand and redraw the same count.
    """
    count = len(combat.hand)
    for card in list(combat.hand):
        combat.hand.remove(card)
        combat.discard_pile.append(card)
    combat._draw_cards(count)  # noqa: SLF001


def _heart_of_iron(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Target gains 7 Plating."""
    t = target if target is not None else user
    t.apply_power(PowerId.PLATING, 7)


def _kings_courage(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Target gains 15 Forge.

    Regent mechanic; calls gain_forge if available.
    """
    t = target if target is not None else user
    if hasattr(t, "gain_forge"):
        t.gain_forge(15)


def _liquid_bronze(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Target gains 3 Thorns."""
    t = target if target is not None else user
    t.apply_power(PowerId.THORNS, 3)


def _potion_of_binding(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Apply 1 Weak and 1 Vulnerable to all enemies."""
    for enemy in list(combat.alive_enemies):
        combat.apply_power_to(enemy, PowerId.WEAK, 1)
        combat.apply_power_to(enemy, PowerId.VULNERABLE, 1)


def _potion_of_capacity(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Add 2 orb slots.

    Defect mechanic; calls add_orb_slots if available.
    """
    if hasattr(combat, "add_orb_slots"):
        combat.add_orb_slots(user, 2)


def _powdered_demise(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Apply 9 Demise to target enemy."""
    if target is not None:
        target.apply_power(PowerId.DEMISE, 9)


def _radiant_tincture(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Target gains 1 energy and 3 Radiance."""
    t = target if target is not None else user
    combat.energy += 1
    t.apply_power(PowerId.RADIANCE, 3)


def _regen_potion(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Target gains 5 Regen."""
    t = target if target is not None else user
    t.apply_power(PowerId.REGEN, 5)


def _stable_serum(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Target gains 2 RetainHand (retain entire hand for N turns)."""
    t = target if target is not None else user
    t.apply_power(PowerId.RETAIN_HAND, 2)


def _touch_of_insanity(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Choose a card from hand; it costs 0 for the rest of combat.

    RL simplification: set first energy-costing card in hand to 0 cost.
    """
    for card in combat.hand:
        if hasattr(card, "cost") and card.cost > 0:
            card.cost = 0
            break


# =====================================================================
#  RARE POTIONS (20)
# =====================================================================

def _beetle_juice(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Apply 4 Shrink to target enemy (reduces damage dealt by 30% per stack)."""
    if target is not None:
        target.apply_power(PowerId.SHRINK, 4)


def _bottled_potential(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Put hand into draw pile, shuffle, then draw 5."""
    cards_in_hand = list(combat.hand)
    combat.hand.clear()
    combat.draw_pile.extend(cards_in_hand)
    combat.rng.shuffle(combat.draw_pile)
    combat._draw_cards(5)  # noqa: SLF001


def _cosmic_concoction(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Generate 3 upgraded Colorless cards in hand.

    RL simplification: draw 3 cards and upgrade them.
    """
    hand_before = len(combat.hand)
    combat._draw_cards(3)  # noqa: SLF001
    for card in combat.hand[hand_before:]:
        if hasattr(card, "can_upgrade") and card.can_upgrade():
            card.upgrade()


def _distilled_chaos(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Auto-play top 3 cards from draw pile.

    RL simplification: draw 3 cards and play each automatically.
    """
    for _ in range(3):
        combat._shuffle_if_needed()  # noqa: SLF001
        if not combat.draw_pile:
            break
        card = combat.draw_pile.pop(0)
        # Auto-play: apply effect then exhaust
        from sts2_env.cards.registry import play_card_effect
        auto_target = None
        if hasattr(card, "target_type"):
            from sts2_env.core.enums import TargetType
            if card.target_type == TargetType.ANY_ENEMY:
                alive = combat.alive_enemies
                auto_target = alive[0] if alive else None
            elif card.target_type in (TargetType.SELF, TargetType.NONE):
                auto_target = user
        play_card_effect(card, combat, auto_target)
        combat.discard_pile.append(card)


def _droplet_of_precognition(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Choose a card from draw pile and add to hand.

    RL simplification: move top card of draw pile to hand.
    """
    combat._shuffle_if_needed()  # noqa: SLF001
    if combat.draw_pile:
        card = combat.draw_pile.pop(0)
        combat.hand.append(card)


def _entropic_brew(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Fill all empty potion slots with random potions.

    Requires potion-slot subsystem not yet available; this is a no-op placeholder.
    """
    # Potion slot management requires RunState.potion_slots; not yet modeled
    pass


def _essence_of_darkness(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Channel Dark orbs equal to orb slot count.

    Defect mechanic; calls channel_orb if available.
    """
    orb_capacity = getattr(user, "orb_capacity", 3)
    if hasattr(combat, "channel_orb"):
        for _ in range(orb_capacity):
            combat.channel_orb(user, "Dark")


def _fairy_in_a_bottle(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """On death: heal 30% max HP and prevent death.

    Automatic usage; this effect fires when triggered by should_die check.
    """
    t = target if target is not None else user
    heal_amount = t.max_hp * 30 // 100
    t.heal(heal_amount)


def _fruit_juice(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Target gains 5 max HP."""
    t = target if target is not None else user
    t.gain_max_hp(5)


def _ghost_in_a_jar(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Target gains 1 Intangible."""
    t = target if target is not None else user
    t.apply_power(PowerId.INTANGIBLE, 1)


def _gigantification_potion(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Target gains 1 Gigantification (double damage for 1 turn)."""
    t = target if target is not None else user
    t.apply_power(PowerId.GIGANTIFICATION, 1)


def _liquid_memories(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Choose a card from discard pile, set cost to 0, add to hand.

    RL simplification: move first discard pile card to hand with cost 0.
    """
    if combat.discard_pile:
        card = combat.discard_pile.pop(0)
        if hasattr(card, "cost"):
            card.cost = 0
        combat.hand.append(card)


def _lucky_tonic(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Target gains 1 Buffer (negate next HP loss)."""
    t = target if target is not None else user
    t.apply_power(PowerId.BUFFER, 1)


def _mazaleths_gift(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Target gains 1 Ritual (gain Strength each turn)."""
    t = target if target is not None else user
    t.apply_power(PowerId.RITUAL, 1)


def _orobic_acid(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Generate 1 random Attack, 1 Skill, 1 Power, each with cost 0, in hand.

    RL simplification: draw 3 cards and set them to cost 0.
    """
    hand_before = len(combat.hand)
    combat._draw_cards(3)  # noqa: SLF001
    for card in combat.hand[hand_before:]:
        if hasattr(card, "cost"):
            card.cost = 0


def _pot_of_ghouls(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Add 2 Soul cards to hand.

    Necrobinder mechanic; calls create_cards_in_hand if available.
    """
    if hasattr(combat, "create_cards_in_hand"):
        combat.create_cards_in_hand(user, "Soul", 2)


def _shackling_potion(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Apply 7 ShacklingPotion power to all enemies (temporary Strength loss)."""
    for enemy in list(combat.alive_enemies):
        enemy.apply_power(PowerId.SHACKLING_POTION, 7)


def _ship_in_a_bottle(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Target gains 10 block now and 10 block next turn."""
    t = target if target is not None else user
    t.gain_block(10)
    t.apply_power(PowerId.BLOCK_NEXT_TURN, 10)


def _snecko_oil(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Draw 7 cards and randomize costs of all cards in hand (0-3)."""
    combat._draw_cards(7)  # noqa: SLF001
    for card in combat.hand:
        if hasattr(card, "cost") and card.cost >= 0:
            card.cost = random.randint(0, 3)


def _soldiers_stew(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """All Strike-tagged cards gain +1 base replay count.

    Adds replay_count attribute if available.
    """
    from sts2_env.core.enums import CardTag
    for pile in (combat.hand, combat.draw_pile, combat.discard_pile):
        for card in pile:
            if hasattr(card, "tags") and CardTag.STRIKE in card.tags:
                if hasattr(card, "base_replay_count"):
                    card.base_replay_count += 1


# =====================================================================
#  EVENT / TOKEN POTIONS (3)
# =====================================================================

def _foul_potion(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """In combat: deal 12 unpowered damage to ALL creatures (enemies + self).
    Out of combat at merchant: gain 100 gold (handled outside combat).
    """
    _deal_unpowered_damage_all(combat, user, 12)


def _glowwater_potion(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Exhaust entire hand, then draw 10."""
    for card in list(combat.hand):
        combat.hand.remove(card)
        combat.exhaust_pile.append(card)
    combat._draw_cards(10)  # noqa: SLF001


def _potion_shaped_rock(combat: CombatState, user: Creature, target: Creature | None) -> None:
    """Deal 15 unpowered damage to target enemy."""
    if target is not None:
        _deal_unpowered_damage(combat, user, target, 15)


# =====================================================================
#  Registration
# =====================================================================

_ALL_EFFECTS: dict[str, object] = {
    # Common (20)
    "AttackPotion":        _attack_potion,
    "BlockPotion":         _block_potion,
    "BloodPotion":         _blood_potion,
    "ColorlessPotion":     _colorless_potion,
    "DexterityPotion":     _dexterity_potion,
    "EnergyPotion":        _energy_potion,
    "ExplosiveAmpoule":    _explosive_ampoule,
    "FirePotion":          _fire_potion,
    "FlexPotion":          _flex_potion,
    "FocusPotion":         _focus_potion,
    "PoisonPotion":        _poison_potion,
    "PotionOfDoom":        _potion_of_doom,
    "PowerPotion":         _power_potion,
    "SkillPotion":         _skill_potion,
    "SpeedPotion":         _speed_potion,
    "StarPotion":          _star_potion,
    "StrengthPotion":      _strength_potion,
    "SwiftPotion":         _swift_potion,
    "VulnerablePotion":    _vulnerable_potion,
    "WeakPotion":          _weak_potion,
    # Uncommon (22 -- 20 in pool + BoneBrew & CunningPotion use char-specific subsystems)
    "Ashwater":            _ashwater,
    "BlessingOfTheForge":  _blessing_of_the_forge,
    "BoneBrew":            _bone_brew,
    "Clarity":             _clarity,
    "CunningPotion":       _cunning_potion,
    "CureAll":             _cure_all,
    "Duplicator":          _duplicator,
    "Fortifier":           _fortifier,
    "FyshOil":             _fysh_oil,
    "GamblersBrew":        _gamblers_brew,
    "HeartOfIron":         _heart_of_iron,
    "KingsCourage":        _kings_courage,
    "LiquidBronze":        _liquid_bronze,
    "PotionOfBinding":     _potion_of_binding,
    "PotionOfCapacity":    _potion_of_capacity,
    "PowderedDemise":      _powdered_demise,
    "RadiantTincture":     _radiant_tincture,
    "RegenPotion":         _regen_potion,
    "StableSerum":         _stable_serum,
    "TouchOfInsanity":     _touch_of_insanity,
    # Rare (20)
    "BeetleJuice":         _beetle_juice,
    "BottledPotential":    _bottled_potential,
    "CosmicConcoction":    _cosmic_concoction,
    "DistilledChaos":      _distilled_chaos,
    "DropletOfPrecognition": _droplet_of_precognition,
    "EntropicBrew":        _entropic_brew,
    "EssenceOfDarkness":   _essence_of_darkness,
    "FairyInABottle":      _fairy_in_a_bottle,
    "FruitJuice":          _fruit_juice,
    "GhostInAJar":         _ghost_in_a_jar,
    "GigantificationPotion": _gigantification_potion,
    "LiquidMemories":      _liquid_memories,
    "LuckyTonic":          _lucky_tonic,
    "MazalethsGift":       _mazaleths_gift,
    "OrobicAcid":          _orobic_acid,
    "PotOfGhouls":         _pot_of_ghouls,
    "ShacklingPotion":     _shackling_potion,
    "ShipInABottle":       _ship_in_a_bottle,
    "SneckoOil":           _snecko_oil,
    "SoldiersStew":        _soldiers_stew,
    # Event / Token (3)
    "FoulPotion":          _foul_potion,
    "GlowwaterPotion":     _glowwater_potion,
    "PotionShapedRock":    _potion_shaped_rock,
}

for _pid, _fn in _ALL_EFFECTS.items():
    register_potion_effect(_pid, _fn)
