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
    """Adds Amount to the tracked powered attack command, then consumes it."""

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.VIGOR, amount)
        self._command_to_modify: object | None = None
        self._amount_when_attack_started: int = 0

    def before_attack(self, owner: Creature, attack: object, combat: CombatState) -> None:
        if getattr(attack, "attacker", None) is not owner:
            return
        if not getattr(attack, "damage_props", ValueProp.NONE).is_powered():
            return
        if self._command_to_modify is not None:
            return
        model_source = getattr(attack, "model_source", None)
        if model_source is not None and not hasattr(model_source, "card_id"):
            return
        self._command_to_modify = attack
        self._amount_when_attack_started = self.amount

    def modify_damage_additive(
        self, owner: Creature, dealer: Creature | None, target: Creature, props: ValueProp
    ) -> int:
        if dealer is not owner:
            return 0
        if not props.is_powered():
            return 0
        active_attack = getattr(owner.combat_state, "active_attack", None)
        active_card_source = getattr(owner.combat_state, "active_card_source", None)
        if self._command_to_modify is not None:
            tracked_source = getattr(self._command_to_modify, "model_source", None)
            if active_card_source is not None and tracked_source is not active_card_source:
                return 0
            if active_attack is not None and active_attack is not self._command_to_modify:
                return 0
        return self.amount

    def after_attack(self, owner: Creature, attack: object, combat: CombatState) -> None:
        if attack is not self._command_to_modify:
            return
        self.amount -= self._amount_when_attack_started
        self._command_to_modify = None
        self._amount_when_attack_started = 0
        if self.amount <= 0 and not self.allow_negative:
            owner.powers.pop(self.power_id, None)


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
        if dealer is not owner and getattr(dealer, "pet_owner", None) is not owner:
            return 1.0
        if not props.is_powered():
            return 1.0
        if getattr(owner.combat_state, "active_card_source", None) is None:
            return 1.0
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
        if getattr(target, "is_player", False):
            combat.add_status_cards_to_discard(target, "WOUND", self.amount)


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
        card = getattr(owner.combat_state, "active_card_source", None)
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

    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.VICIOUS, amount)

    def after_power_amount_changed(
        self,
        owner: Creature,
        target: Creature,
        power_id: PowerId,
        amount: int,
        applier: Creature | None,
        source: object | None,
        combat: CombatState,
    ) -> None:
        if amount > 0 and applier is owner and power_id == PowerId.VULNERABLE:
            combat.draw_cards(owner, self.amount)


# ── CalamityPower ────────────────────────────────────────────────────────
class CalamityPower(PowerInstance):
    """After playing an Attack card, generate Amount random Attack cards into hand.

    C#: BeforeCardPlayed records the card; AfterCardPlayed generates random
    Attack cards from the character pool into Hand.

    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.CALAMITY, amount)
        self._played_attack_ids: set[int] = set()

    def before_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        card_owner = getattr(card, "owner", None)
        card_type = getattr(card, "card_type", None) or getattr(card, "type", None)
        if card_owner is not owner:
            return
        if card_type != CardType.ATTACK:
            return
        instance_id = getattr(card, "instance_id", None)
        if instance_id is not None:
            self._played_attack_ids.add(instance_id)

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        instance_id = getattr(card, "instance_id", None)
        if instance_id is None or instance_id not in self._played_attack_ids:
            return
        self._played_attack_ids.discard(instance_id)
        combat.generate_random_cards_to_hand(owner, CardType.ATTACK, count=self.amount)


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
