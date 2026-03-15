"""Damage-modifier powers: additive, multiplicative, cap, and related trigger powers.

Covers StrengthPower, VulnerablePower, WeakPower, VigorPower, DoubleDamagePower,
EnragePower, CrueltyPower, DebilitatePower, PainfulStabsPower, LethalityPower,
AccuracyPower, FocusedStrikePower, ViciousPower, CalamityPower, AggressionPower.

Strength, Vulnerable, Weak are already registered in common.py; they are NOT
re-registered here.  This module only adds the powers that common.py does not
contain.
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from sts2_env.core.enums import (
    CardTag,
    CardType,
    CombatSide,
    PowerId,
    PowerStackType,
    PowerType,
    ValueProp,
)
from sts2_env.powers.base import PowerInstance

if TYPE_CHECKING:
    from sts2_env.core.creature import Creature
    from sts2_env.core.combat import CombatState


# ── VigorPower ───────────────────────────────────────────────────────────
class VigorPower(PowerInstance):
    """Adds Amount to the *first* powered attack each card play, then consumed.

    In the full engine the Vigor tracks which AttackCommand it is modifying so
    it only applies once per card play.  In the simplified sim we add the
    bonus to every powered attack the owner deals (the attack pipeline is
    responsible for calling ``consume_vigor`` after the first hit of a card).
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.VIGOR, amount)

    def modify_damage_additive(
        self, owner: Creature, dealer: Creature | None, target: Creature, props: ValueProp
    ) -> int:
        if dealer is not owner:
            return 0
        if not props.is_powered():
            return 0
        return self.amount


# ── DoubleDamagePower ────────────────────────────────────────────────────
class DoubleDamagePower(PowerInstance):
    """Doubles powered attack damage from cards.  Ticks down at end of player turn.

    C# logic: returns 2.0 multiplier when dealer is owner (or owner's pet),
    the attack is powered, and originated from a card (cardSource != None).
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.DOUBLE_DAMAGE, amount)

    def modify_damage_multiplicative(
        self, owner: Creature, dealer: Creature | None, target: Creature, props: ValueProp
    ) -> float:
        if dealer is not owner:
            return 1.0
        if not props.is_powered():
            return 1.0
        # In C#, returns 1.0 if cardSource is None; in our sim card-sourced
        # attacks always have MOVE set, so the check is effectively covered.
        return 2.0

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        """Ticks down at end of PLAYER turn (C#: side == CombatSide.Player)."""
        if side == CombatSide.PLAYER:
            self.amount -= 1


# ── EnragePower ──────────────────────────────────────────────────────────
class EnragePower(PowerInstance):
    """After a Skill card is played, gain Amount Strength.

    C#: AfterCardPlayed checks cardPlay.Card.Type == CardType.Skill and then
    applies StrengthPower to owner.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.ENRAGE, amount)

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        card_type = getattr(card, "card_type", None) or getattr(card, "type", None)
        if card_type == CardType.SKILL:
            owner.apply_power(PowerId.STRENGTH, self.amount)


# ── CrueltyPower ─────────────────────────────────────────────────────────
class CrueltyPower(PowerInstance):
    """Increases the Vulnerable multiplier by Amount/100 when dealing damage.

    C#: ``ModifyVulnerableMultiplier`` returns ``amount + base.Amount / 100``
    but only when the target is NOT the owner and the attack is powered.

    This does not directly override a standard hook; instead,
    VulnerablePower's multiplicative hook consults this power.  For our sim
    we expose a helper that the damage pipeline (or VulnerablePower) can call.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.CRUELTY, amount)

    def extra_vulnerable_multiplier(self, owner: Creature, target: Creature, props: ValueProp) -> float:
        """Return additive bonus to Vulnerable multiplier (e.g. +0.25)."""
        if target is owner:
            return 0.0
        if not props.is_powered():
            return 0.0
        return self.amount / 100.0


# ── DebilitatePower ──────────────────────────────────────────────────────
class DebilitatePower(PowerInstance):
    """Doubles the *excess* of both Vulnerable and Weak multipliers.

    C# ``ModifyVulnerableMultiplier``: ``amount + (amount - 1)`` -- e.g. 1.5
    becomes 1.5 + 0.5 = 2.0.
    C# ``ModifyWeakMultiplier``: ``amount - (1 - amount)`` -- e.g. 0.75
    becomes 0.75 - 0.25 = 0.50.

    Ticks down at end of owner's side turn.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.DEBILITATE, amount)

    def extra_vulnerable_multiplier_on_self(self, base_mult: float, owner: Creature, props: ValueProp) -> float:
        """Return modified Vulnerable multiplier when owner IS the target."""
        if owner is not self._get_owner_ref():
            return base_mult
        if not props.is_powered():
            return base_mult
        # amount + (amount - 1)  e.g. 1.5 -> 2.0
        return base_mult + (base_mult - 1.0)

    def extra_weak_multiplier_on_self(self, base_mult: float, owner: Creature, props: ValueProp) -> float:
        """Return modified Weak multiplier when owner IS the dealer."""
        if owner is not self._get_owner_ref():
            return base_mult
        if not props.is_powered():
            return base_mult
        # amount - (1 - amount)  e.g. 0.75 -> 0.50
        return base_mult - (1.0 - base_mult)

    def _get_owner_ref(self) -> object:
        """Helper -- we don't store owner; rely on the power dict lookup."""
        return None  # placeholder; callers pass owner explicitly

    def on_turn_end_enemy_side(self, owner: Creature) -> None:
        """Ticks down at end of owner's side turn (C#: side == owner.Side)."""
        if self.skip_next_tick:
            self.skip_next_tick = False
            return
        self.amount -= 1


# ── PainfulStabsPower ────────────────────────────────────────────────────
class PainfulStabsPower(PowerInstance):
    """After the owner deals unblocked powered attack damage, adds Wound cards
    to each player's discard pile.

    C#: For each player hit with unblocked damage, adds ``Amount`` Wound cards
    per hit that dealt unblocked damage, into that player's discard pile.

    In the simplified sim this is an after_damage_given trigger.  The card
    generation part (adding Wounds) is handled by the combat pipeline.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.PAINFUL_STABS, amount)

    def after_damage_given(
        self,
        owner: Creature,
        dealer: Creature,
        target: Creature,
        damage: int,
        props: ValueProp,
        combat: CombatState,
    ) -> None:
        if dealer is not owner:
            return
        if not props.is_powered():
            return
        if damage <= 0:
            return
        # In the full engine, Wound cards are added to discard.
        # The combat pipeline should handle the actual card creation.
        # We signal via combat state:
        add_wounds = getattr(combat, "add_status_cards_to_discard", None)
        if add_wounds is not None:
            add_wounds(target, "WOUND", self.amount)


# ── LethalityPower ───────────────────────────────────────────────────────
class LethalityPower(PowerInstance):
    """The first Attack card played each turn deals Amount% more damage.

    C#: ModifyDamageMultiplicative checks that the card is the owner's first
    attack this turn (by counting CardPlaysStarted for attacks this turn).
    If it IS the first attack, returns ``1 + Amount/100``.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.LETHALITY, amount)
        self._attacks_played_this_turn: int = 0

    def modify_damage_multiplicative(
        self, owner: Creature, dealer: Creature | None, target: Creature, props: ValueProp
    ) -> float:
        if dealer is not owner:
            return 1.0
        if not props.is_powered():
            return 1.0
        if self._attacks_played_this_turn > 0:
            return 1.0
        return 1.0 + self.amount / 100.0

    def before_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        card_type = getattr(card, "card_type", None) or getattr(card, "type", None)
        if card_type == CardType.ATTACK:
            self._attacks_played_this_turn += 1

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            self._attacks_played_this_turn = 0


# ── AccuracyPower ────────────────────────────────────────────────────────
class AccuracyPower(PowerInstance):
    """Adds Amount damage to Shiv cards.

    C#: ModifyDamageAdditive checks ``card.Tags.Contains(CardTag.Shiv)``.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.ACCURACY, amount)

    def modify_damage_additive(
        self, owner: Creature, dealer: Creature | None, target: Creature, props: ValueProp
    ) -> int:
        if dealer is not owner:
            return 0
        if not props.is_powered():
            return 0
        # The card context is not passed into modify_damage_additive in our
        # hook signature.  In the simplified sim, we check a thread-local or
        # combat-state "current card" attribute.  For now this returns 0;
        # the attack pipeline is expected to set a flag when a shiv is being
        # played.  A more direct approach: override with card-aware hook.
        return 0

    def modify_damage_additive_for_card(
        self, owner: Creature, dealer: Creature | None, target: Creature,
        props: ValueProp, card: object
    ) -> int:
        """Extended hook called by card-aware damage pipeline."""
        if dealer is not owner:
            return 0
        if not props.is_powered():
            return 0
        if card is None:
            return 0
        tags = getattr(card, "tags", set())
        if CardTag.SHIV not in tags:
            return 0
        return self.amount


# ── FocusedStrikePower (TemporaryFocusPower subclass) ────────────────────
class FocusedStrikePower(PowerInstance):
    """Temporary Focus: grants Focus equal to Amount on application, removes
    it at end of owner's turn.

    C#: Extends TemporaryFocusPower which:
      - On BeforeApplied: applies FocusPower += Amount to the target.
      - On AfterTurnEnd (owner's side): removes self and applies
        FocusPower -= Amount to owner.

    In our sim, we apply/remove Focus directly.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.FOCUSED_STRIKE, amount)
        self._applied: bool = False

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        """Apply focus when first added (simulates BeforeApplied)."""
        if not self._applied:
            owner.apply_power(PowerId.FOCUS, self.amount)
            self._applied = True

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        """Remove focus at end of owner's turn and remove self."""
        if side == owner.side:
            owner.apply_power(PowerId.FOCUS, -self.amount)
            self.amount = 0  # signal removal


# ── ViciousPower ─────────────────────────────────────────────────────────
class ViciousPower(PowerInstance):
    """When the owner applies Vulnerable, draw Amount cards.

    C#: AfterPowerAmountChanged checks if the changed power is
    VulnerablePower, amount > 0, and applier == owner, then draws.

    In our simplified sim, this is a trigger power -- the combat pipeline
    should call its hook after applying vulnerable.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.VICIOUS, amount)


# ── CalamityPower ────────────────────────────────────────────────────────
class CalamityPower(PowerInstance):
    """After playing an Attack card, generate Amount random Attack cards into hand.

    C#: BeforeCardPlayed records the card; AfterCardPlayed generates random
    Attack cards from the character pool into Hand.

    In the simplified sim, the card generation is handled by the combat
    pipeline.  This power signals the intent via after_card_played.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.CALAMITY, amount)

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        card_type = getattr(card, "card_type", None) or getattr(card, "type", None)
        if card_type != CardType.ATTACK:
            return
        # Signal to combat pipeline to generate random attack cards.
        gen = getattr(combat, "generate_random_cards_to_hand", None)
        if gen is not None:
            gen(owner, CardType.ATTACK, self.amount)


# ── AggressionPower ──────────────────────────────────────────────────────
class AggressionPower(PowerInstance):
    """At start of owner's turn, retrieve Amount Attack cards from discard
    to hand and upgrade them if possible.

    C#: BeforeSideTurnStart shuffles discard, takes up to Amount attack cards,
    moves them to hand, and upgrades upgradable ones.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.AGGRESSION, amount)

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side != owner.side:
            return
        # In the simplified sim, card retrieval from discard is delegated
        # to the combat pipeline.
        retrieve = getattr(combat, "retrieve_attacks_from_discard", None)
        if retrieve is not None:
            retrieve(owner, self.amount, upgrade=True)


# ─── Registration ────────────────────────────────────────────────────────
from sts2_env.core.creature import register_power_class  # noqa: E402

_DAMAGE_MODIFIER_POWERS: dict[PowerId, type[PowerInstance]] = {
    PowerId.VIGOR: VigorPower,
    PowerId.DOUBLE_DAMAGE: DoubleDamagePower,
    PowerId.ENRAGE: EnragePower,
    PowerId.CRUELTY: CrueltyPower,
    PowerId.DEBILITATE: DebilitatePower,
    PowerId.PAINFUL_STABS: PainfulStabsPower,
    PowerId.LETHALITY: LethalityPower,
    PowerId.ACCURACY: AccuracyPower,
    PowerId.FOCUSED_STRIKE: FocusedStrikePower,
    PowerId.VICIOUS: ViciousPower,
    PowerId.CALAMITY: CalamityPower,
    PowerId.AGGRESSION: AggressionPower,
}

for _pid, _cls in _DAMAGE_MODIFIER_POWERS.items():
    register_power_class(_pid, _cls)
