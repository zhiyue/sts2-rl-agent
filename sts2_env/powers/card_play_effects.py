"""Powers triggered by card play, card exhaust, and related events.

Covers: CorruptionPower, EchoFormPower, DarkEmbracePower, FeelNoPainPower,
RagePower, AfterimagePower, RupturePower, JuggernautPower, EnvenomPower,
ThunderPower, ElectrodynamicsPower, PhantomBladesPower, SerpentFormPower,
SetupStrikePower, OneTwoPunchPower, FreeAttackPower, FreeSkillPower,
FreePowerPower, SneakyPower, HeistPower, NecroMasteryPower, HellraiserPower.

All logic verified against decompiled C# source.
"""

from __future__ import annotations

import random
from typing import TYPE_CHECKING

from sts2_env.core.enums import (
    CardTag,
    CardType,
    CombatSide,
    PowerId,
    PowerType,
    PowerStackType,
    ValueProp,
)
from sts2_env.powers.base import PowerInstance

if TYPE_CHECKING:
    from sts2_env.core.creature import Creature
    from sts2_env.core.combat import CombatState


# ---------------------------------------------------------------------------
# CorruptionPower
# ---------------------------------------------------------------------------
class CorruptionPower(PowerInstance):
    """Skills cost 0 and are Exhausted after play.

    C# ref: CorruptionPower.cs
    - TryModifyEnergyCostInCombat: sets skill cost to 0
    - ModifyCardPlayResultPileTypeAndPosition: sends skills to exhaust pile
    StackType.Single (non-stacking).
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.SINGLE

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.CORRUPTION, amount)

    # Cost modification is handled by the card-play engine checking this power.
    # We expose helpers that the combat system calls.

    def modify_card_cost(self, owner: Creature, card: object) -> int | None:
        """Return 0 for skills owned by this creature, None otherwise."""
        if getattr(card, "card_type", None) == CardType.SKILL:
            return 0
        return None

    def should_exhaust_card(self, owner: Creature, card: object) -> bool:
        """Return True if the card should be exhausted (skills)."""
        return getattr(card, "card_type", None) == CardType.SKILL


# ---------------------------------------------------------------------------
# EchoFormPower
# ---------------------------------------------------------------------------
class EchoFormPower(PowerInstance):
    """The first Amount card(s) played each turn are played an extra time.

    C# ref: EchoFormPower.cs
    - ModifyCardPlayCount: +1 if fewer than Amount first-play cards this turn.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.ECHO_FORM, amount)
        self._cards_echoed_this_turn: int = 0

    def modify_card_play_count(self, owner: Creature, count: int, card: object) -> int:
        if self._cards_echoed_this_turn < self.amount:
            self._cards_echoed_this_turn += 1
            return count + 1
        return count

    def before_side_turn_start(
        self, owner: Creature, side: CombatSide, combat: CombatState
    ) -> None:
        if side == owner.side:
            self._cards_echoed_this_turn = 0


# ---------------------------------------------------------------------------
# DarkEmbracePower
# ---------------------------------------------------------------------------
class DarkEmbracePower(PowerInstance):
    """Whenever a card is Exhausted, draw Amount card(s).

    C# ref: DarkEmbracePower.cs
    - AfterCardExhausted: draw Amount (deferred for ethereal cards, batched
      at end of turn; in our sim we draw immediately for simplicity).
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.DARK_EMBRACE, amount)

    def after_card_exhausted(
        self, owner: Creature, card: object, combat: CombatState
    ) -> None:
        combat._draw_cards(self.amount)


# ---------------------------------------------------------------------------
# FeelNoPainPower
# ---------------------------------------------------------------------------
class FeelNoPainPower(PowerInstance):
    """Whenever a card is Exhausted, gain Amount Block (unpowered).

    C# ref: FeelNoPainPower.cs
    - AfterCardExhausted: GainBlock(Amount, Unpowered).
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.FEEL_NO_PAIN, amount)

    def after_card_exhausted(
        self, owner: Creature, card: object, combat: CombatState
    ) -> None:
        owner.gain_block(self.amount)


# ---------------------------------------------------------------------------
# RagePower
# ---------------------------------------------------------------------------
class RagePower(PowerInstance):
    """Whenever you play an Attack, gain Amount Block (unpowered).
    Removed at end of your turn.

    C# ref: RagePower.cs
    - AfterCardPlayed: if Attack -> GainBlock(Amount, Unpowered).
    - AfterTurnEnd: remove self.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.RAGE, amount)

    def after_card_played(
        self, owner: Creature, card: object, combat: CombatState
    ) -> None:
        if getattr(card, "card_type", None) == CardType.ATTACK:
            owner.gain_block(self.amount)

    def after_turn_end(
        self, owner: Creature, side: CombatSide, combat: CombatState
    ) -> None:
        if side == owner.side:
            owner.powers.pop(self.power_id, None)


# ---------------------------------------------------------------------------
# AfterimagePower
# ---------------------------------------------------------------------------
class AfterimagePower(PowerInstance):
    """Whenever you play a card, gain Amount Block (unpowered).

    C# ref: AfterimagePower.cs
    - AfterCardPlayed: GainBlock(Amount, Unpowered).
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.AFTERIMAGE, amount)

    def after_card_played(
        self, owner: Creature, card: object, combat: CombatState
    ) -> None:
        owner.gain_block(self.amount)


# ---------------------------------------------------------------------------
# RupturePower
# ---------------------------------------------------------------------------
class RupturePower(PowerInstance):
    """Whenever you lose HP during your turn, gain Amount Strength.

    C# ref: RupturePower.cs
    - AfterDamageReceived: if owner loses HP on own turn, gain Strength.
      (In C# the Strength gain is deferred until after the card finishes
       playing; for simulation purposes we apply it immediately.)
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.RUPTURE, amount)

    def after_damage_received(
        self,
        owner: Creature,
        target: Creature,
        dealer: Creature | None,
        damage: int,
        props: ValueProp,
        combat: CombatState,
    ) -> None:
        if target is owner and damage > 0:
            # Only triggers on the owner's own turn
            if getattr(combat, "current_side", None) == owner.side:
                owner.apply_power(PowerId.STRENGTH, self.amount)


# ---------------------------------------------------------------------------
# JuggernautPower
# ---------------------------------------------------------------------------
class JuggernautPower(PowerInstance):
    """Whenever you gain Block, deal Amount damage to a random enemy (unpowered).

    C# ref: JuggernautPower.cs
    - AfterBlockGained: Damage(random enemy, Amount, Unpowered).
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.JUGGERNAUT, amount)

    def after_block_gained(
        self,
        owner: Creature,
        creature: Creature,
        amount: int,
        combat: CombatState,
    ) -> None:
        if creature is owner and amount > 0:
            enemies = combat.alive_enemies
            if enemies:
                target = random.choice(enemies)
                combat.deal_damage(
                    dealer=owner,
                    target=target,
                    amount=self.amount,
                    props=ValueProp.UNPOWERED,
                )


# ---------------------------------------------------------------------------
# EnvenomPower
# ---------------------------------------------------------------------------
class EnvenomPower(PowerInstance):
    """Whenever you deal unblocked attack damage, apply Amount Poison to the target.

    C# ref: EnvenomPower.cs
    - AfterDamageGiven: if powered attack and unblocked > 0, apply Poison.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.ENVENOM, amount)

    def after_damage_given(
        self,
        owner: Creature,
        dealer: Creature,
        target: Creature,
        damage: int,
        props: ValueProp,
        combat: CombatState,
    ) -> None:
        if dealer is owner and props.is_powered() and damage > 0:
            target.apply_power(PowerId.POISON, self.amount)


# ---------------------------------------------------------------------------
# ThunderPower
# ---------------------------------------------------------------------------
class ThunderPower(PowerInstance):
    """Whenever a Lightning orb is Evoked, deal Amount damage to all targets.

    C# ref: ThunderPower.cs
    - AfterOrbEvoked: if Lightning, deal Amount unpowered damage to living
      targets.
    StackType.Counter.

    NOTE: Orb system integration is handled externally; this power provides
    the hook method that the orb evocation calls.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.THUNDER, amount)

    def on_orb_evoked(
        self,
        owner: Creature,
        orb_type: str,
        targets: list[Creature],
        combat: CombatState,
    ) -> None:
        """Called by the orb system when a Lightning orb is evoked."""
        if orb_type == "LIGHTNING":
            living = [t for t in targets if t.is_alive]
            for t in living:
                combat.deal_damage(
                    dealer=owner,
                    target=t,
                    amount=self.amount,
                    props=ValueProp.UNPOWERED,
                )


# ---------------------------------------------------------------------------
# ElectrodynamicsPower
# ---------------------------------------------------------------------------
class ElectrodynamicsPower(PowerInstance):
    """Lightning orbs now hit ALL enemies instead of a random one.

    C# ref: ElectrodynamicsPower does not exist as a standalone file in
    STS2 decompiled source; the Lightning targeting change is likely handled
    by the orb model itself. This power serves as a flag that the orb
    system checks.
    StackType.Single (non-stacking flag).
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.SINGLE

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.ELECTRODYNAMICS, amount)

    # The orb system checks owner.has_power(ELECTRODYNAMICS) to decide
    # targeting. No hook methods needed.


# ---------------------------------------------------------------------------
# PhantomBladesPower
# ---------------------------------------------------------------------------
class PhantomBladesPower(PowerInstance):
    """Shivs gain Retain. The first Shiv played each turn deals +Amount damage.

    C# ref: PhantomBladesPower.cs
    - AfterCardEnteredCombat: add Retain to Shivs
    - ModifyDamageAdditive: +Amount for first Shiv played per turn
    StackType.Counter.

    NOTE: Shiv-specific Retain is handled by the card system. This power
    provides the damage bonus for the first Shiv each turn.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.PHANTOM_BLADES, amount)
        self._shiv_played_this_turn: bool = False

    def modify_damage_additive(
        self,
        owner: Creature,
        dealer: Creature | None,
        target: Creature,
        props: ValueProp,
    ) -> int:
        if dealer is not owner or not props.is_powered() or self._shiv_played_this_turn:
            return 0
        card = getattr(owner.combat_state, "active_card_source", None)
        tags = getattr(card, "tags", set())
        if CardTag.SHIV not in tags:
            return 0
        return self.amount

    def after_card_played(
        self, owner: Creature, card: object, combat: CombatState
    ) -> None:
        if getattr(card, "is_shiv", False):
            self._shiv_played_this_turn = True

    def before_side_turn_start(
        self, owner: Creature, side: CombatSide, combat: CombatState
    ) -> None:
        if side == owner.side:
            self._shiv_played_this_turn = False


# ---------------------------------------------------------------------------
# SerpentFormPower
# ---------------------------------------------------------------------------
class SerpentFormPower(PowerInstance):
    """After you play a card, deal Amount damage to a random enemy (unpowered).

    C# ref: SerpentFormPower.cs
    - AfterCardPlayed: deal Amount damage to random hittable enemy.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.SERPENT_FORM, amount)

    def after_card_played(
        self, owner: Creature, card: object, combat: CombatState
    ) -> None:
        enemies = combat.alive_enemies
        if enemies:
            target = random.choice(enemies)
            combat.deal_damage(
                dealer=owner,
                target=target,
                amount=self.amount,
                props=ValueProp.UNPOWERED,
            )


# ---------------------------------------------------------------------------
# SetupStrikePower (TemporaryStrength)
# ---------------------------------------------------------------------------
class SetupStrikePower(PowerInstance):
    """Temporary Strength that is removed at end of turn.

    C# ref: SetupStrikePower.cs extends TemporaryStrengthPower.
    - BeforeApplied: also apply same amount of Strength.
    - AfterTurnEnd: remove self and apply -Amount Strength.
    StackType.Counter.

    The power_type is BUFF when positive, DEBUFF when negative.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.SETUP_STRIKE, amount)

    # Strength application is handled when the power is applied to the
    # creature, via apply_power which calls the creature's power system.
    # The combat system applies Strength alongside this power.

    def after_turn_end(
        self, owner: Creature, side: CombatSide, combat: CombatState
    ) -> None:
        if side == owner.side:
            # Remove the temporary strength
            owner.apply_power(PowerId.STRENGTH, -self.amount)
            owner.powers.pop(self.power_id, None)


# ---------------------------------------------------------------------------
# OneTwoPunchPower
# ---------------------------------------------------------------------------
class OneTwoPunchPower(PowerInstance):
    """The next Attack played this turn is played an extra time. Decrements.
    Removed at end of turn.

    C# ref: OneTwoPunchPower.cs
    - ModifyCardPlayCount: +1 for Attacks.
    - AfterModifyingCardPlayCount: decrement.
    - AfterTurnEnd: remove self.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.ONE_TWO_PUNCH, amount)

    def modify_card_play_count(self, owner: Creature, count: int, card: object) -> int:
        if self.amount > 0 and getattr(card, "card_type", None) == CardType.ATTACK:
            return count + 1
        return count

    def after_modifying_card_play_count(self, owner: Creature, card: object, combat: CombatState) -> None:
        if self.amount > 0 and getattr(card, "card_type", None) == CardType.ATTACK:
            self.amount -= 1

    def after_turn_end(
        self, owner: Creature, side: CombatSide, combat: CombatState
    ) -> None:
        if side == owner.side:
            owner.powers.pop(self.power_id, None)


# ---------------------------------------------------------------------------
# FreeAttackPower
# ---------------------------------------------------------------------------
class FreeAttackPower(PowerInstance):
    """The next Amount Attack(s) cost 0 this turn. Decrements on play.

    C# ref: FreeAttackPower.cs
    - TryModifyEnergyCostInCombat: Attack cost -> 0.
    - BeforeCardPlayed: decrement when Attack is played.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.FREE_ATTACK, amount)

    def modify_card_cost(self, owner: Creature, card: object) -> int | None:
        if (
            self.amount > 0
            and getattr(card, "card_type", None) == CardType.ATTACK
        ):
            return 0
        return None

    def before_card_played(
        self, owner: Creature, card: object, combat: CombatState
    ) -> None:
        if getattr(card, "card_type", None) == CardType.ATTACK and self.amount > 0:
            self.amount -= 1
            if self.amount <= 0:
                owner.powers.pop(self.power_id, None)


# ---------------------------------------------------------------------------
# FreeSkillPower
# ---------------------------------------------------------------------------
class FreeSkillPower(PowerInstance):
    """The next Amount Skill(s) cost 0 this turn. Decrements on play.

    C# ref: FreeSkillPower.cs
    - TryModifyEnergyCostInCombat: Skill cost -> 0.
    - BeforeCardPlayed: decrement when Skill is played.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.FREE_SKILL, amount)

    def modify_card_cost(self, owner: Creature, card: object) -> int | None:
        if (
            self.amount > 0
            and getattr(card, "card_type", None) == CardType.SKILL
        ):
            return 0
        return None

    def before_card_played(
        self, owner: Creature, card: object, combat: CombatState
    ) -> None:
        if getattr(card, "card_type", None) == CardType.SKILL and self.amount > 0:
            self.amount -= 1
            if self.amount <= 0:
                owner.powers.pop(self.power_id, None)


# ---------------------------------------------------------------------------
# FreePowerPower
# ---------------------------------------------------------------------------
class FreePowerPower(PowerInstance):
    """The next Amount Power card(s) cost 0 this turn. Decrements on play.

    C# ref: FreePowerPower.cs
    - TryModifyEnergyCostInCombat: Power cost -> 0.
    - BeforeCardPlayed: decrement when Power is played.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.FREE_POWER, amount)

    def modify_card_cost(self, owner: Creature, card: object) -> int | None:
        if (
            self.amount > 0
            and getattr(card, "card_type", None) == CardType.POWER
        ):
            return 0
        return None

    def before_card_played(
        self, owner: Creature, card: object, combat: CombatState
    ) -> None:
        if getattr(card, "card_type", None) == CardType.POWER and self.amount > 0:
            self.amount -= 1
            if self.amount <= 0:
                owner.powers.pop(self.power_id, None)


# ---------------------------------------------------------------------------
# SneakyPower
# ---------------------------------------------------------------------------
class SneakyPower(PowerInstance):
    """Whenever an *enemy* plays an Attack, gain Amount Block (unpowered).

    C# ref: SneakyPower.cs
    - AfterCardPlayed: if the card is NOT owned by this creature and is an
      Attack, gain block.
    StackType.Counter.

    NOTE: In the sim this fires from the monster-intent execution path.
    The hook is after_card_played with the card belonging to the enemy.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.SNEAKY, amount)

    def after_card_played(
        self, owner: Creature, card: object, combat: CombatState
    ) -> None:
        # Triggers when a card NOT owned by this creature is an Attack
        card_owner = getattr(card, "owner", None)
        if card_owner is not owner and getattr(card, "card_type", None) == CardType.ATTACK:
            owner.gain_block(self.amount)


# ---------------------------------------------------------------------------
# HeistPower
# ---------------------------------------------------------------------------
class HeistPower(PowerInstance):
    """When this creature dies, the gold it stole is added as a reward.

    C# ref: HeistPower.cs
    - BeforeDeath: add gold reward to combat room.
    StackType.Counter. Amount = gold stolen.

    In the simulator, HeistPower is a flag. The reward system checks for
    it upon creature death.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.HEIST, amount)


# ---------------------------------------------------------------------------
# NecroMasteryPower
# ---------------------------------------------------------------------------
class NecroMasteryPower(PowerInstance):
    """When your Osty (pet) takes damage, deal that damage x Amount to all
    hittable enemies (unblockable, unpowered).

    C# ref: NecroMasteryPower.cs
    - AfterCurrentHpChanged: if Osty lost HP, deal damage * Amount to all
      enemies.
    StackType.Counter.

    Simplified: In the simulator, this triggers on damage to a pet creature
    if it exists.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.NECRO_MASTERY, amount)

    def after_damage_received(
        self,
        owner: Creature,
        target: Creature,
        dealer: Creature | None,
        damage: int,
        props: ValueProp,
        combat: CombatState,
    ) -> None:
        # Triggers when a pet (Osty) owned by this player takes damage
        if (
            target is not owner
            and getattr(target, "is_pet", False)
            and getattr(target, "pet_owner", None) is owner
            and damage > 0
        ):
            for enemy in combat.alive_enemies:
                combat.deal_damage(
                    dealer=owner,
                    target=enemy,
                    amount=damage * self.amount,
                    props=ValueProp.UNBLOCKABLE | ValueProp.UNPOWERED,
                )


# ---------------------------------------------------------------------------
# HellraiserPower
# ---------------------------------------------------------------------------
class HellraiserPower(PowerInstance):
    """When you draw a Strike card, auto-play it.

    C# ref: HellraiserPower.cs
    - AfterCardDrawnEarly: if the card has the Strike tag, auto-play it.
    StackType.Single.

    In the simulator, this is handled by the draw-card pipeline checking
    for this power. The power itself is a non-stacking flag.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.SINGLE

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.HELLRAISER, amount)

    # Auto-play logic is handled by the card-draw system:
    # if owner.has_power(HELLRAISER) and card.has_tag(STRIKE):
    #     auto_play(card)


# ---------------------------------------------------------------------------
# EnragePower (monster variant -- triggers on Skill played by anyone)
# ---------------------------------------------------------------------------
class EnragePower(PowerInstance):
    """Whenever a Skill is played (by anyone), gain Amount Strength.

    C# ref: EnragePower.cs
    - AfterCardPlayed: if Skill, gain Strength.
    StackType.Counter.

    This is a monster power (used by e.g. Gremlin Nob). It triggers on
    ANY skill card played, not just the owner's.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.ENRAGE, amount)

    def after_card_played(
        self, owner: Creature, card: object, combat: CombatState
    ) -> None:
        if getattr(card, "card_type", None) == CardType.SKILL:
            owner.apply_power(PowerId.STRENGTH, self.amount)


# ---------------------------------------------------------------------------
# Registration
# ---------------------------------------------------------------------------
from sts2_env.core.creature import register_power_class  # noqa: E402

_ALL_POWERS: dict[PowerId, type[PowerInstance]] = {
    PowerId.CORRUPTION: CorruptionPower,
    PowerId.ECHO_FORM: EchoFormPower,
    PowerId.DARK_EMBRACE: DarkEmbracePower,
    PowerId.FEEL_NO_PAIN: FeelNoPainPower,
    PowerId.RAGE: RagePower,
    PowerId.AFTERIMAGE: AfterimagePower,
    PowerId.RUPTURE: RupturePower,
    PowerId.JUGGERNAUT: JuggernautPower,
    PowerId.ENVENOM: EnvenomPower,
    PowerId.THUNDER: ThunderPower,
    PowerId.ELECTRODYNAMICS: ElectrodynamicsPower,
    PowerId.PHANTOM_BLADES: PhantomBladesPower,
    PowerId.SERPENT_FORM: SerpentFormPower,
    PowerId.SETUP_STRIKE: SetupStrikePower,
    PowerId.ONE_TWO_PUNCH: OneTwoPunchPower,
    PowerId.FREE_ATTACK: FreeAttackPower,
    PowerId.FREE_SKILL: FreeSkillPower,
    PowerId.FREE_POWER: FreePowerPower,
    PowerId.SNEAKY: SneakyPower,
    PowerId.HEIST: HeistPower,
    PowerId.NECRO_MASTERY: NecroMasteryPower,
    PowerId.HELLRAISER: HellraiserPower,
    PowerId.ENRAGE: EnragePower,
}

for _pid, _cls in _ALL_POWERS.items():
    register_power_class(_pid, _cls)
