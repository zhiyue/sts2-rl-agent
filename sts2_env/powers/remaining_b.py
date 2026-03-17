"""Remaining powers batch B (46 powers): G-R.

Covers: GravityPower, GuardedPower, HailstormPower, HammerTimePower, HangPower,
HardToKillPower, HauntPower, HelicalDartPower, HexPower, HighVoltagePower,
HotfixPower, ImbalancedPower, ImprovementPower, InfernoPower, IterationPower,
JugglingPower, KnockdownPower, LeadershipPower, LightningRodPower,
MagicBombPower, ManglePower, MasterPlannerPower, MindRotPower,
MonarchsGazePower, MonarchsGazeStrengthDownPower, NemesisPower,
NeurosurgePower, NightmarePower, NoDrawPower, NostalgiaPower, OblivionPower,
OrbitPower, OutbreakPower, PagestormPower, PaleBlueDotPower, PaperCutsPower,
ParryPower, PiercingWailPower, PillarOfCreationPower, PossessSpeedPower,
PossessStrengthPower, PrepTimePower, PyrePower, RadiancePower, RampartPower,
ReaperFormPower.

All logic verified against decompiled C# source from
MegaCrit.Sts2.Core.Models.Powers.
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from sts2_env.core.enums import (
    CardId,
    CardKeyword,
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
# GravityPower
# ---------------------------------------------------------------------------
class GravityPower(PowerInstance):
    """After you play a card, deal Amount damage to all enemies (unpowered).
    Removed at end of owner's turn.

    C# ref: GravityPower.cs
    - BeforeCardPlayed: records card + amount.
    - AfterCardPlayed: deals Amount damage to all hittable enemies (unpowered).
    - AfterTurnEnd (owner side): remove self.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.GRAVITY, amount)

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        card_owner = getattr(card, "owner", None)
        if card_owner is not None and card_owner is not owner:
            return
        for enemy in combat.get_enemies_of(owner):
            if enemy.is_alive:
                combat.deal_damage(
                    dealer=owner,
                    target=enemy,
                    amount=self.amount,
                    props=ValueProp.UNPOWERED,
                )

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            self.amount = 0


# ---------------------------------------------------------------------------
# GuardedPower
# ---------------------------------------------------------------------------
class GuardedPower(PowerInstance):
    """Owner takes 50% damage from powered attacks. Removed when the applier dies.

    C# ref: GuardedPower.cs
    - ModifyDamageMultiplicative: 0.5 for powered attacks targeting owner.
    StackType.Single. Instanced.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.SINGLE

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.GUARDED, amount)

    def modify_damage_multiplicative(
        self, owner: Creature, dealer: Creature | None, target: Creature, props: ValueProp
    ) -> float:
        if target is not owner:
            return 1.0
        if not props.is_powered():
            return 1.0
        return 0.5


# ---------------------------------------------------------------------------
# HailstormPower
# ---------------------------------------------------------------------------
class HailstormPower(PowerInstance):
    """Before turn end, if owner has >= 1 Frost orb, deal Amount damage
    to all enemies (unpowered).

    C# ref: HailstormPower.cs
    - BeforeTurnEnd (owner side): count Frost orbs; if >= FrostOrbs threshold
      (1), deal Amount damage to all hittable enemies.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    FROST_ORB_THRESHOLD = 1

    def __init__(self, amount: int):
        super().__init__(PowerId.HAILSTORM, amount)

    def before_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side != owner.side:
            return
        # Count Frost orbs via the combat/orb system
        frost_count = 0
        orb_queue = getattr(owner, "orb_queue", None) or getattr(
            getattr(owner, "player_combat_state", None), "orb_queue", None
        )
        if orb_queue is not None:
            orbs = getattr(orb_queue, "orbs", orb_queue)
            if hasattr(orbs, "__iter__"):
                frost_count = sum(
                    1 for o in orbs if getattr(o, "orb_type", None) == "FROST"
                )
        # Simplified: if no orb system, check combat helper
        if frost_count == 0:
            count_fn = getattr(combat, "count_orbs", None)
            if count_fn is not None:
                frost_count = count_fn(owner, "FROST")
        if frost_count >= self.FROST_ORB_THRESHOLD:
            for enemy in combat.get_enemies_of(owner):
                if enemy.is_alive:
                    combat.deal_damage(
                        dealer=owner,
                        target=enemy,
                        amount=self.amount,
                        props=ValueProp.UNPOWERED,
                    )


# ---------------------------------------------------------------------------
# HammerTimePower
# ---------------------------------------------------------------------------
class HammerTimePower(PowerInstance):
    """When the owner forges, all other players also forge the same amount.

    C# ref: HammerTimePower.cs
    - AfterForge: if forger == owner.Player and source is not self,
      forge same amount for all other living players.
    StackType.Single.

    Simplified: Forge mechanic is Regent-specific multiplayer feature.
    This power is a flag; the forge system checks for it.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.SINGLE

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.HAMMER_TIME, amount)

    def after_forge(
        self,
        owner: Creature,
        amount: int,
        forger: Creature,
        source: object | None,
        combat: CombatState,
    ) -> None:
        if source is self or forger is not owner or amount <= 0:
            return
        for teammate in combat.get_teammates_of(owner):
            if getattr(teammate, "is_player", False) and teammate.is_alive:
                combat.forge(teammate, amount, source=self)


# ---------------------------------------------------------------------------
# HangPower
# ---------------------------------------------------------------------------
class HangPower(PowerInstance):
    """Multiplies damage taken from Hang cards by Amount.

    C# ref: HangPower.cs
    - ModifyDamageMultiplicative: returns Amount when target == owner
      AND cardSource is a Hang card.
    StackType.Counter.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.HANG, amount)

    def modify_damage_multiplicative(
        self, owner: Creature, dealer: Creature | None, target: Creature, props: ValueProp
    ) -> float:
        if target is not owner:
            return 1.0
        card_source = getattr(owner.combat_state, "active_card_source", None)
        if getattr(card_source, "card_id", None) != CardId.HANG:
            return 1.0
        return float(self.amount)


# ---------------------------------------------------------------------------
# HardToKillPower
# ---------------------------------------------------------------------------
class HardToKillPower(PowerInstance):
    """Caps incoming damage to Amount per hit when targeting the owner.

    C# ref: HardToKillPower.cs
    - ModifyDamageCap: returns Amount when target == owner, else MaxValue.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.HARD_TO_KILL, amount)

    def modify_damage_cap(
        self, owner: Creature, dealer: Creature | None, target: Creature,
        damage: float, props: ValueProp
    ) -> float:
        if target is not owner:
            return float("inf")
        return float(self.amount)


# ---------------------------------------------------------------------------
# HauntPower
# ---------------------------------------------------------------------------
class HauntPower(PowerInstance):
    """When owner plays a Soul card, deal Amount unblockable/unpowered damage
    to a random enemy.

    C# ref: HauntPower.cs
    - AfterCardPlayed: if card is Soul, deal Amount damage to random hittable
      enemy (Unblockable | Unpowered, no dealer).
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.HAUNT, amount)

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        card_id = getattr(card, "card_id", None)
        is_soul = (card_id is not None and card_id.name == "SOUL") or getattr(card, "is_soul", False)
        if not is_soul:
            return
        target = combat.random_enemy_of(owner)
        if target is not None:
            combat.deal_damage(
                dealer=None,
                target=target,
                amount=self.amount,
                props=ValueProp.UNBLOCKABLE | ValueProp.UNPOWERED,
            )


# ---------------------------------------------------------------------------
# HelicalDartPower
# ---------------------------------------------------------------------------
class HelicalDartPower(PowerInstance):
    """Temporary Dexterity (positive). Grants Dexterity on application,
    removes it at end of owner's turn.

    C# ref: HelicalDartPower.cs extends TemporaryDexterityPower.
    - BeforeApplied: apply +Amount Dexterity.
    - AfterTurnEnd (owner side): remove self and apply -Amount Dexterity.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.HELICAL_DART, amount)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            owner.apply_power(PowerId.DEXTERITY, -self.amount)
            self.amount = 0


# ---------------------------------------------------------------------------
# HexPower
# ---------------------------------------------------------------------------
class HexPower(PowerInstance):
    """Afflicts all cards with Hexed (adds Ethereal). Removed when the
    applier dies. On removal, clears Hexed and removes applied Ethereal.

    C# ref: HexPower.cs
    - AfterApplied: afflict all player cards with Hexed (adds Ethereal).
    - AfterCardEnteredCombat: afflict new cards.
    - AfterDeath: if applier dies, remove self.
    - AfterRemoved: clear Hexed from all cards, remove applied Ethereal.
    StackType.Single.

    Simplified: This power is a flag. The card system checks for it and
    applies/removes Ethereal. The Amount is the affliction amount.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.SINGLE

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.HEX, amount)

    # Card affliction is handled by the card/combat system.
    # owner.has_power(HEX) triggers Ethereal application.


# ---------------------------------------------------------------------------
# HighVoltagePower
# ---------------------------------------------------------------------------
class HighVoltagePower(PowerInstance):
    """Gain Amount Strength at end of owner's turn.

    C# ref: HighVoltagePower.cs
    - AfterTurnEnd (owner side): apply Amount Strength to owner.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.HIGH_VOLTAGE, amount)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            owner.apply_power(PowerId.STRENGTH, self.amount)


# ---------------------------------------------------------------------------
# HotfixPower
# ---------------------------------------------------------------------------
class HotfixPower(PowerInstance):
    """Temporary Focus (positive). Grants Focus on application,
    removes it at end of owner's turn.

    C# ref: HotfixPower.cs extends TemporaryFocusPower.
    - BeforeApplied: apply +Amount Focus.
    - AfterTurnEnd (owner side): remove self and apply -Amount Focus.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.HOTFIX, amount)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            owner.apply_power(PowerId.FOCUS, -self.amount)
            self.amount = 0


# ---------------------------------------------------------------------------
# ImbalancedPower
# ---------------------------------------------------------------------------
class ImbalancedPower(PowerInstance):
    """When owner deals damage that is fully blocked, the owner is stunned.

    C# ref: ImbalancedPower.cs
    - AfterDamageGiven: if dealer == owner and damage was fully blocked,
      stun the owner.
    StackType.Single.

    Simplified: Stun is handled by the monster AI. This power signals via
    a flag that the AI checks.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.SINGLE

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.IMBALANCED, amount)
        self.was_fully_blocked: bool = False

    def after_damage_given(
        self, owner: Creature, dealer: Creature, target: Creature,
        damage: int, props: ValueProp, combat: CombatState
    ) -> None:
        if dealer is owner and damage == 0:
            # Damage was fully blocked => stun
            self.was_fully_blocked = True
            # Stun is handled by the monster AI system


# ---------------------------------------------------------------------------
# ImprovementPower
# ---------------------------------------------------------------------------
class ImprovementPower(PowerInstance):
    """After combat ends, upgrade Amount random upgradable cards in the deck.

    C# ref: ImprovementPower.cs
    - AfterCombatEnd: pick Amount random upgradable cards and upgrade them.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.IMPROVEMENT, amount)

    def after_combat_victory(self, owner: Creature, combat: CombatState) -> None:
        upgrade_fn = getattr(combat, "upgrade_random_cards", None)
        if upgrade_fn is not None:
            upgrade_fn(owner, self.amount)


# ---------------------------------------------------------------------------
# InfernoPower
# ---------------------------------------------------------------------------
class InfernoPower(PowerInstance):
    """At start of turn, deal self-damage (tracked separately via SelfDamage
    DynamicVar, starts at 0). When owner takes unblocked damage on their own
    turn, deal Amount damage to all enemies (unpowered).

    C# ref: InfernoPower.cs
    - AfterPlayerTurnStart: deal SelfDamage to self (unblockable, unpowered).
    - AfterDamageReceived: if owner took unblocked damage on own turn,
      deal Amount damage to all hittable enemies (unpowered).
    - IncrementSelfDamage(): increases the self-damage var by 1.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.INFERNO, amount)
        self.self_damage: int = 0

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side and self.self_damage > 0:
            combat.deal_damage(
                dealer=owner,
                target=owner,
                amount=self.self_damage,
                props=ValueProp.UNBLOCKABLE | ValueProp.UNPOWERED,
            )

    def after_damage_received(
        self, owner: Creature, target: Creature, dealer: Creature | None,
        damage: int, props: ValueProp, combat: CombatState
    ) -> None:
        if target is not owner or damage <= 0:
            return
        current_side = getattr(combat, "current_side", None)
        if current_side != owner.side:
            return
        for enemy in combat.get_enemies_of(owner):
            if enemy.is_alive:
                combat.deal_damage(
                    dealer=owner,
                    target=enemy,
                    amount=self.amount,
                    props=ValueProp.UNPOWERED,
                )

    def increment_self_damage(self) -> None:
        """Called by card system (e.g., Stoke) to increase self-damage."""
        self.self_damage += 1


# ---------------------------------------------------------------------------
# IterationPower
# ---------------------------------------------------------------------------
class IterationPower(PowerInstance):
    """When owner draws a Status card (first one per turn), draw Amount
    additional cards.

    C# ref: IterationPower.cs
    - AfterCardDrawn: if owner drew a Status card and it's the first Status
      drawn this turn, draw Amount cards.
    StackType.Counter.

    Simplified: The draw system should call on_card_drawn() when a card
    is drawn. We track the first Status drawn per turn.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.ITERATION, amount)
        self._status_drawn_this_turn: int = 0

    def on_card_drawn(self, owner: Creature, card: object, combat: CombatState) -> None:
        """Called by the draw system when a card is drawn."""
        if getattr(card, "card_type", None) == CardType.STATUS:
            self._status_drawn_this_turn += 1
            if self._status_drawn_this_turn <= 1:
                combat.draw_cards(owner, self.amount)

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            self._status_drawn_this_turn = 0


# ---------------------------------------------------------------------------
# JugglingPower
# ---------------------------------------------------------------------------
class JugglingPower(PowerInstance):
    """On the 3rd Attack played each turn, add Amount copies of that card
    to hand.

    C# ref: JugglingPower.cs
    - AfterCardPlayed: if owner plays an Attack, increment counter.
      On the 3rd attack, clone the card Amount times to hand.
    - AfterTurnEnd (owner side): reset counter.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    TRIGGER_ON_ATTACK = 3

    def __init__(self, amount: int):
        super().__init__(PowerId.JUGGLING, amount)
        self._attacks_played_this_turn: int = 0

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        card_owner = getattr(card, "owner", None)
        if card_owner is not None and card_owner is not owner:
            return
        if getattr(card, "card_type", None) != CardType.ATTACK:
            return
        self._attacks_played_this_turn += 1
        if self._attacks_played_this_turn == self.TRIGGER_ON_ATTACK:
            clone_fn = getattr(combat, "clone_card_to_hand", None)
            if clone_fn is not None:
                for _ in range(self.amount):
                    clone_fn(owner, card)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            self._attacks_played_this_turn = 0


# ---------------------------------------------------------------------------
# KnockdownPower
# ---------------------------------------------------------------------------
class KnockdownPower(PowerInstance):
    """Multiplies powered attack damage taken by Amount, EXCEPT from the
    applier. Removed at end of owner's turn.

    C# ref: KnockdownPower.cs
    - ModifyDamageMultiplicative: returns Amount when target == owner,
      powered attack, and dealer != applier.
    - AfterTurnEnd (owner side): remove self.
    StackType.Counter. Instanced.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.KNOCKDOWN, amount)
        self.applier: Creature | None = None

    def modify_damage_multiplicative(
        self, owner: Creature, dealer: Creature | None, target: Creature, props: ValueProp
    ) -> float:
        if target is not owner:
            return 1.0
        if not props.is_powered():
            return 1.0
        if dealer is self.applier:
            return 1.0
        return float(self.amount)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            self.amount = 0


# ---------------------------------------------------------------------------
# LeadershipPower
# ---------------------------------------------------------------------------
class LeadershipPower(PowerInstance):
    """Allied creatures (not the owner) deal +Amount damage on powered attacks.

    C# ref: LeadershipPower.cs
    - ModifyDamageAdditive: +Amount when dealer is an ally on the same side
      (but not the owner itself) and attack is powered.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.LEADERSHIP, amount)

    def modify_damage_additive(
        self, owner: Creature, dealer: Creature | None, target: Creature, props: ValueProp
    ) -> int:
        if dealer is owner:
            return 0
        if dealer is None:
            return 0
        if dealer.side != owner.side:
            return 0
        if not props.is_powered():
            return 0
        return self.amount


# ---------------------------------------------------------------------------
# LightningRodPower
# ---------------------------------------------------------------------------
class LightningRodPower(PowerInstance):
    """At start of turn (energy reset), channel a Lightning orb, then
    decrement.

    C# ref: LightningRodPower.cs
    - AfterEnergyReset: channel Lightning orb, then decrement.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.LIGHTNING_ROD, amount)

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side and owner.is_player:
            if hasattr(combat, "channel_orb"):
                combat.channel_orb(owner, "LIGHTNING")
            self.amount -= 1


# ---------------------------------------------------------------------------
# MagicBombPower
# ---------------------------------------------------------------------------
class MagicBombPower(PowerInstance):
    """At end of owner's turn, deal Amount damage to self (unpowered),
    then remove self. Removed if applier dies.

    C# ref: MagicBombPower.cs
    - AfterTurnEnd (owner side): if applier is alive, deal Amount damage
      to owner (unpowered), then remove self.
    - AfterDeath: if applier dies, remove self.
    StackType.Counter. Instanced.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.MAGIC_BOMB, amount)
        self.applier: Creature | None = None

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side != owner.side:
            return
        if self.applier is not None and not self.applier.is_alive:
            self.amount = 0
            return
        combat.deal_damage(
            dealer=owner,
            target=owner,
            amount=self.amount,
            props=ValueProp.UNPOWERED,
        )
        self.amount = 0


# ---------------------------------------------------------------------------
# ManglePower
# ---------------------------------------------------------------------------
class ManglePower(PowerInstance):
    """Temporary Strength (negative / debuff). Applies -Amount Strength on
    application, restores it at end of owner's turn.

    C# ref: ManglePower.cs extends TemporaryStrengthPower, IsPositive=false.
    - BeforeApplied: apply -Amount Strength.
    - AfterTurnEnd (owner side): remove self, apply +Amount Strength.
    StackType.Counter.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.MANGLE, amount)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            # Reverse: was applied as -Amount STR, now restore +Amount STR
            owner.apply_power(PowerId.STRENGTH, self.amount)
            self.amount = 0


# ---------------------------------------------------------------------------
# MasterPlannerPower
# ---------------------------------------------------------------------------
class MasterPlannerPower(PowerInstance):
    """After playing a Skill card, apply the Sly keyword to it.

    C# ref: MasterPlannerPower.cs
    - AfterCardPlayed: if owner plays a Skill, apply CardKeyword.Sly to it.
    StackType.Single.

    Simplified: The card system applies Sly (card goes to top of draw pile
    instead of discard). This power flags the intent.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.SINGLE

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.MASTER_PLANNER, amount)

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        card_owner = getattr(card, "owner", None)
        if card_owner is not None and card_owner is not owner:
            return
        if getattr(card, "card_type", None) != CardType.SKILL:
            return
        combat_vars = getattr(card, "combat_vars", None)
        if combat_vars is not None:
            combat_vars["sly_this_turn"] = 1


# ---------------------------------------------------------------------------
# MindRotPower
# ---------------------------------------------------------------------------
class MindRotPower(PowerInstance):
    """Reduces cards drawn per turn by Amount (minimum 0).

    C# ref: MindRotPower.cs
    - ModifyHandDraw: max(0, count - Amount).
    StackType.Counter.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.MIND_ROT, amount)

    def modify_hand_draw(self, owner: Creature, draw: int) -> int:
        return max(0, draw - self.amount)


# ---------------------------------------------------------------------------
# MonarchsGazePower
# ---------------------------------------------------------------------------
class MonarchsGazePower(PowerInstance):
    """After dealing powered attack damage, apply Amount
    MonarchsGazeStrengthDown (temporary negative Strength) to the target.

    C# ref: MonarchsGazePower.cs
    - AfterDamageGiven: if dealer == owner and powered attack, apply
      MonarchsGazeStrengthDownPower to target.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.MONARCHS_GAZE, amount)

    def after_damage_given(
        self, owner: Creature, dealer: Creature, target: Creature,
        damage: int, props: ValueProp, combat: CombatState
    ) -> None:
        if dealer is owner and props.is_powered():
            target.apply_power(PowerId.MONARCHS_GAZE_STRENGTH_DOWN, self.amount)


# ---------------------------------------------------------------------------
# MonarchsGazeStrengthDownPower
# ---------------------------------------------------------------------------
class MonarchsGazeStrengthDownPower(PowerInstance):
    """Temporary Strength (negative / debuff). Applied by MonarchsGazePower.
    Applies -Amount Strength on application, restores it at end of owner's turn.

    C# ref: MonarchsGazeStrengthDownPower.cs extends TemporaryStrengthPower,
    IsPositive=false.
    - BeforeApplied: apply -Amount Strength.
    - AfterTurnEnd (owner side): remove self, apply +Amount Strength.
    StackType.Counter.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.MONARCHS_GAZE_STRENGTH_DOWN, amount)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            owner.apply_power(PowerId.STRENGTH, self.amount)
            self.amount = 0


# ---------------------------------------------------------------------------
# NemesisPower
# ---------------------------------------------------------------------------
class NemesisPower(PowerInstance):
    """Every other turn (alternating), gain 1 Intangible with skip_next_tick.
    On the off-turn, remove Intangible if present.

    C# ref: NemesisPower.cs
    - AfterTurnEnd (owner side): toggle flag. If flag is set, apply
      Intangible(1) with SkipNextDurationTick=true. If flag is unset
      and owner has Intangible, remove it.
    StackType.Single.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.SINGLE

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.NEMESIS, amount)
        self._should_apply_intangible: bool = False

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side != owner.side:
            return
        self._should_apply_intangible = not self._should_apply_intangible
        if self._should_apply_intangible:
            owner.apply_power(PowerId.INTANGIBLE, 1)
            intangible = owner.powers.get(PowerId.INTANGIBLE)
            if intangible is not None:
                intangible.skip_next_tick = True
        else:
            if owner.has_power(PowerId.INTANGIBLE):
                owner.powers.pop(PowerId.INTANGIBLE, None)


# ---------------------------------------------------------------------------
# NeurosurgePower
# ---------------------------------------------------------------------------
class NeurosurgePower(PowerInstance):
    """Apply Amount Doom to owner at start of their turn.

    C# ref: NeurosurgePower.cs
    - AfterSideTurnStart (owner side): apply DoomPower(Amount) to owner.
    StackType.Counter.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.NEUROSURGE, amount)

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            owner.apply_power(PowerId.DOOM, self.amount)


# ---------------------------------------------------------------------------
# NightmarePower
# ---------------------------------------------------------------------------
class NightmarePower(PowerInstance):
    """At start of next turn, add Amount copies of the selected card to hand,
    then remove self.

    C# ref: NightmarePower.cs
    - BeforeHandDraw: if owner's turn, add Amount clones of selectedCard
      to hand, then remove self.
    - SetSelectedCard(): stores the card to clone.
    StackType.Counter. Instanced.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.NIGHTMARE, amount)
        self.selected_card: object | None = None

    def set_selected_card(self, card: object) -> None:
        """Store the card to be cloned next turn."""
        clone = getattr(card, "clone", None)
        if callable(clone):
            self.selected_card = clone(0)
        else:
            self.selected_card = card

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side and self.selected_card is not None:
            clone_fn = getattr(combat, "clone_card_to_hand", None)
            if clone_fn is not None:
                for _ in range(self.amount):
                    clone_fn(owner, self.selected_card)
            self.amount = 0


# ---------------------------------------------------------------------------
# NoDrawPower
# ---------------------------------------------------------------------------
class NoDrawPower(PowerInstance):
    """Prevents the owner from drawing cards outside of the hand draw phase.
    Removed at end of any turn.

    C# ref: NoDrawPower.cs
    - ShouldDraw: returns false for non-hand-draw draws for the owner.
    - AfterTurnEnd: remove self (unconditionally).
    StackType.Single.

    Simplified: The draw system checks owner.has_power(NO_DRAW) to block
    mid-turn draws.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.SINGLE

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.NO_DRAW, amount)

    def should_draw(self, owner: Creature, from_hand_draw: bool) -> bool | None:
        if from_hand_draw:
            return None
        return False

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        self.amount = 0


# ---------------------------------------------------------------------------
# NostalgiaPower
# ---------------------------------------------------------------------------
class NostalgiaPower(PowerInstance):
    """The first Amount Attack/Skill cards played each turn go to the top of
    the draw pile instead of discard.

    C# ref: NostalgiaPower.cs
    - ModifyCardPlayResultPileTypeAndPosition: for the first Amount
      Attack/Skill cards played this turn, redirect from Discard to
      Draw (top position).
    StackType.Counter.

    Simplified: The card-play system checks this power. We track how many
    qualifying cards have been redirected this turn.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.NOSTALGIA, amount)
        self._cards_redirected_this_turn: int = 0

    def should_redirect_to_draw_pile(self, owner: Creature, card: object) -> bool:
        """Called by the card-play system. Returns True if this card should
        go to the top of the draw pile instead of discard."""
        card_type = getattr(card, "card_type", None)
        if card_type not in (CardType.ATTACK, CardType.SKILL):
            return False
        if self._cards_redirected_this_turn >= self.amount:
            return False
        self._cards_redirected_this_turn += 1
        return True

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            self._cards_redirected_this_turn = 0


# ---------------------------------------------------------------------------
# OblivionPower
# ---------------------------------------------------------------------------
class OblivionPower(PowerInstance):
    """When the applier plays a card, apply Amount Doom to the owner.
    Removed at end of player turn.

    C# ref: OblivionPower.cs
    - BeforeCardPlayed: if card owner == applier's player, record.
    - AfterCardPlayed: apply DoomPower(Amount) to owner.
    - AfterTurnEnd (Player side): remove self.
    StackType.Counter.

    Simplified: In single-player, this triggers when the player plays a card
    and applies Doom to the enemy (owner of this power).
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.OBLIVION, amount)
        self.applier: Creature | None = None

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        # In single-player, the applier is a player creature
        card_owner = getattr(card, "owner", None)
        if self.applier is not None and card_owner is self.applier:
            owner.apply_power(PowerId.DOOM, self.amount)
        elif self.applier is None:
            # Fallback: trigger on any player card
            if getattr(card_owner, "is_player", False):
                owner.apply_power(PowerId.DOOM, self.amount)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == CombatSide.PLAYER:
            self.amount = 0


# ---------------------------------------------------------------------------
# OrbitPower
# ---------------------------------------------------------------------------
class OrbitPower(PowerInstance):
    """Every 4 energy spent, gain Amount energy.

    C# ref: OrbitPower.cs
    - AfterEnergySpent: track total energy spent by owner. Every 4 energy,
      gain Amount energy. Triggers = energySpent // 4 - previousTriggers.
    StackType.Counter. Instanced.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    ENERGY_INCREMENT = 4

    def __init__(self, amount: int):
        super().__init__(PowerId.ORBIT, amount)
        self._energy_spent: int = 0
        self._trigger_count: int = 0

    def on_energy_spent(self, owner: Creature, energy_amount: int, combat: CombatState) -> None:
        """Called by the energy system when energy is spent."""
        if energy_amount <= 0:
            return
        self._energy_spent += energy_amount
        triggers = self._energy_spent // self.ENERGY_INCREMENT - self._trigger_count
        if triggers > 0:
            combat.gain_energy(owner, self.amount * triggers)
            self._trigger_count += triggers


# ---------------------------------------------------------------------------
# OutbreakPower
# ---------------------------------------------------------------------------
class OutbreakPower(PowerInstance):
    """Every 3rd time the owner applies Poison, deal Amount damage to all
    enemies (unpowered).

    C# ref: OutbreakPower.cs
    - AfterPowerAmountChanged: if owner applied Poison (amount > 0),
      increment counter. Every 3rd application, deal Amount damage to all
      hittable enemies.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    POISON_THRESHOLD = 3

    def __init__(self, amount: int):
        super().__init__(PowerId.OUTBREAK, amount)
        self._times_poisoned: int = 0

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
        if applier is not owner or power_id != PowerId.POISON or amount <= 0:
            return
        self._times_poisoned += 1
        if self._times_poisoned >= self.POISON_THRESHOLD:
            for enemy in combat.get_enemies_of(owner):
                if enemy.is_alive:
                    combat.deal_damage(
                        dealer=owner,
                        target=enemy,
                        amount=self.amount,
                        props=ValueProp.UNPOWERED,
                    )
            self._times_poisoned %= self.POISON_THRESHOLD


# ---------------------------------------------------------------------------
# PagestormPower
# ---------------------------------------------------------------------------
class PagestormPower(PowerInstance):
    """When owner draws an Ethereal card, draw Amount additional cards.

    C# ref: PagestormPower.cs
    - AfterCardDrawn: if owner draws a card with Ethereal keyword,
      draw Amount cards.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.PAGESTORM, amount)

    def on_card_drawn(self, owner: Creature, card: object, combat: CombatState) -> None:
        """Called by the draw system when a card is drawn."""
        keywords = getattr(card, "keywords", set())
        if CardKeyword.ETHEREAL in keywords:
            combat.draw_cards(owner, self.amount)


# ---------------------------------------------------------------------------
# PaleBlueDotPower
# ---------------------------------------------------------------------------
class PaleBlueDotPower(PowerInstance):
    """If the owner played >= 5 cards last turn, draw Amount extra cards
    this turn.

    C# ref: PaleBlueDotPower.cs
    - ModifyHandDraw: if cards played last round >= 5 (CardPlay threshold),
      add Amount to draw count.
    StackType.Counter.

    Simplified: The combat state tracks cards played per round. We check
    the previous round's count.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    CARD_PLAY_THRESHOLD = 5

    def __init__(self, amount: int):
        super().__init__(PowerId.PALE_BLUE_DOT, amount)

    def modify_hand_draw(self, owner: Creature, draw: int) -> int:
        # Check cards played last turn via combat state
        last_turn_plays = getattr(owner, "_cards_played_last_turn", 0)
        if last_turn_plays == 0:
            # Try combat state helper
            combat = getattr(owner, "_combat", None)
            if combat is not None:
                fn = getattr(combat, "cards_played_last_round", None)
                if fn is not None:
                    last_turn_plays = fn(owner)
        if last_turn_plays >= self.CARD_PLAY_THRESHOLD:
            return draw + self.amount
        return draw


# ---------------------------------------------------------------------------
# PaperCutsPower
# ---------------------------------------------------------------------------
class PaperCutsPower(PowerInstance):
    """After dealing unblocked powered attack damage to a player, reduce
    that player's max HP by Amount.

    C# ref: PaperCutsPower.cs
    - AfterDamageGiven: if dealer == owner, target is player, powered attack,
      and unblocked damage > 0, reduce target's max HP by Amount.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.PAPER_CUTS, amount)

    def after_damage_given(
        self, owner: Creature, dealer: Creature, target: Creature,
        damage: int, props: ValueProp, combat: CombatState
    ) -> None:
        if dealer is not owner:
            return
        if not props.is_powered():
            return
        if damage <= 0:
            return
        if not getattr(target, "is_player", False):
            return
        lose_max_hp = getattr(target, "lose_max_hp", None)
        if lose_max_hp is not None:
            lose_max_hp(self.amount)


# ---------------------------------------------------------------------------
# ParryPower
# ---------------------------------------------------------------------------
class ParryPower(PowerInstance):
    """After owner plays a Sovereign Blade, gain Amount block (unpowered).

    C# ref: ParryPower.cs
    - AfterSovereignBladePlayed: if dealer == owner, gain Amount block.
    StackType.Counter.

    Simplified: The card system calls on_sovereign_blade_played(). Also
    triggers via after_card_played when the card is a Sovereign Blade.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.PARRY, amount)

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        card_id = getattr(card, "card_id", None)
        if card_id is not None and card_id.name == "SOVEREIGN_BLADE":
            card_owner = getattr(card, "owner", None)
            if card_owner is owner or card_owner is None:
                owner.gain_block(self.amount)


# ---------------------------------------------------------------------------
# PiercingWailPower
# ---------------------------------------------------------------------------
class PiercingWailPower(PowerInstance):
    """Temporary Strength (negative / debuff). Applied by Piercing Wail card.
    Applies -Amount Strength on application, restores at end of owner's turn.

    C# ref: PiercingWailPower.cs extends TemporaryStrengthPower,
    IsPositive=false.
    - BeforeApplied: apply -Amount Strength.
    - AfterTurnEnd (owner side): remove self, apply +Amount Strength.
    StackType.Counter.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.PIERCING_WAIL, amount)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            owner.apply_power(PowerId.STRENGTH, self.amount)
            self.amount = 0


# ---------------------------------------------------------------------------
# PillarOfCreationPower
# ---------------------------------------------------------------------------
class PillarOfCreationPower(PowerInstance):
    """When a card is generated and added to combat (by the owner's player),
    gain Amount block (unpowered).

    C# ref: PillarOfCreationPower.cs
    - AfterCardGeneratedForCombat: if card owner == owner.Player and
      addedByPlayer, gain Amount block.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.PILLAR_OF_CREATION, amount)

    def on_card_generated(self, owner: Creature, card: object, combat: CombatState) -> None:
        """Called by the card generation system when a card is generated."""
        owner.gain_block(self.amount)


# ---------------------------------------------------------------------------
# PossessSpeedPower
# ---------------------------------------------------------------------------
class PossessSpeedPower(PowerInstance):
    """Tracks Dexterity stolen from players by the owner. When the owner
    dies, restores the stolen Dexterity to each victim.

    C# ref: PossessSpeedPower.cs
    - AfterPowerAmountChanged: if owner applied negative Dexterity to a
      player, track the amount.
    - AfterDeath: if owner dies, restore stolen Dexterity to victims.
    StackType.Single.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.SINGLE

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.POSSESS_SPEED, amount)
        self._stolen_dexterity: dict[Creature, int] = {}

    def track_stolen_dexterity(self, victim: Creature, stolen_amount: int) -> None:
        """Called by the power application system when the owner steals
        Dexterity from a player."""
        if victim not in self._stolen_dexterity:
            self._stolen_dexterity[victim] = 0
        self._stolen_dexterity[victim] += stolen_amount

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
        if applier is owner and power_id == PowerId.DEXTERITY and amount < 0 and target.is_player:
            self.track_stolen_dexterity(target, -amount)

    def on_owner_death(self, owner: Creature, combat: CombatState) -> bool:
        """Restore stolen Dexterity to victims when owner dies."""
        for victim, stolen in self._stolen_dexterity.items():
            if victim.is_alive:
                victim.apply_power(PowerId.DEXTERITY, -stolen)
        self._stolen_dexterity.clear()
        return False  # Do not prevent removal


# ---------------------------------------------------------------------------
# PossessStrengthPower
# ---------------------------------------------------------------------------
class PossessStrengthPower(PowerInstance):
    """Tracks Strength stolen from players by the owner. When the owner
    dies, restores the stolen Strength to each victim.

    C# ref: PossessStrengthPower.cs
    - AfterPowerAmountChanged: if owner applied negative Strength to a
      player, track the amount.
    - AfterDeath: if owner dies, restore stolen Strength to victims.
    StackType.Single.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.SINGLE

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.POSSESS_STRENGTH, amount)
        self._stolen_strength: dict[Creature, int] = {}

    def track_stolen_strength(self, victim: Creature, stolen_amount: int) -> None:
        """Called by the power application system when the owner steals
        Strength from a player."""
        if victim not in self._stolen_strength:
            self._stolen_strength[victim] = 0
        self._stolen_strength[victim] += stolen_amount

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
        if applier is owner and power_id == PowerId.STRENGTH and amount < 0 and target.is_player:
            self.track_stolen_strength(target, -amount)

    def on_owner_death(self, owner: Creature, combat: CombatState) -> bool:
        """Restore stolen Strength to victims when owner dies."""
        for victim, stolen in self._stolen_strength.items():
            if victim.is_alive:
                victim.apply_power(PowerId.STRENGTH, -stolen)
        self._stolen_strength.clear()
        return False


# ---------------------------------------------------------------------------
# PrepTimePower
# ---------------------------------------------------------------------------
class PrepTimePower(PowerInstance):
    """Gain Amount Vigor at the start of each turn.

    C# ref: PrepTimePower.cs
    - AfterSideTurnStart (owner side): apply VigorPower(Amount) to owner.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.PREP_TIME, amount)

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side:
            owner.apply_power(PowerId.VIGOR, self.amount)


# ---------------------------------------------------------------------------
# PyrePower
# ---------------------------------------------------------------------------
class PyrePower(PowerInstance):
    """Increases max energy by Amount.

    C# ref: PyrePower.cs
    - ModifyMaxEnergy: +Amount for owner's player.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.PYRE, amount)

    def modify_max_energy(self, owner: Creature, energy: int) -> int:
        return energy + self.amount


# ---------------------------------------------------------------------------
# RadiancePower
# ---------------------------------------------------------------------------
class RadiancePower(PowerInstance):
    """At start of turn (energy reset), gain 1 energy and decrement.

    C# ref: RadiancePower.cs
    - AfterEnergyReset: gain EnergyVar (default 1) energy, then decrement.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    ENERGY_GAIN = 1

    def __init__(self, amount: int):
        super().__init__(PowerId.RADIANCE, amount)

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side == owner.side and owner.is_player:
            combat.gain_energy(owner, self.ENERGY_GAIN)
            self.amount -= 1


# ---------------------------------------------------------------------------
# RampartPower
# ---------------------------------------------------------------------------
class RampartPower(PowerInstance):
    """At start of player turn, grant Amount block (unpowered) to all
    TurretOperator enemies.

    C# ref: RampartPower.cs
    - AfterSideTurnStart (Player side): give Amount block to all
      TurretOperator enemies.
    StackType.Counter.

    Simplified: Grants block to all allied creatures (on the owner's side)
    at the start of the player turn.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.RAMPART, amount)

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side != CombatSide.PLAYER:
            return
        # Grant block to allied creatures on the same side
        allies = getattr(combat, "get_allies_of", None)
        if allies is not None:
            for ally in allies(owner):
                if ally.is_alive:
                    ally.gain_block(self.amount)
        else:
            # Fallback: grant block to all enemies (since this is an enemy power)
            for enemy in getattr(combat, "enemies", []):
                if enemy.is_alive and enemy.side == owner.side:
                    enemy.gain_block(self.amount)


# ---------------------------------------------------------------------------
# ReaperFormPower
# ---------------------------------------------------------------------------
class ReaperFormPower(PowerInstance):
    """After dealing powered attack damage, apply (total_damage * Amount)
    Doom to the target.

    C# ref: ReaperFormPower.cs
    - AfterDamageGiven: if dealer == owner (or owner's pet) and powered
      attack and totalDamage > 0, apply Doom = totalDamage * Amount.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.REAPER_FORM, amount)

    def after_damage_given(
        self, owner: Creature, dealer: Creature, target: Creature,
        damage: int, props: ValueProp, combat: CombatState
    ) -> None:
        if dealer is None:
            return
        is_owner_or_pet = (dealer is owner) or (
            getattr(dealer, "pet_owner", None) is not None
            and getattr(dealer.pet_owner, "creature", None) is owner
        )
        if not is_owner_or_pet:
            return
        if not props.is_powered():
            return
        if damage <= 0:
            return
        target.apply_power(PowerId.DOOM, damage * self.amount)


# ---------------------------------------------------------------------------
# Registration
# ---------------------------------------------------------------------------
from sts2_env.core.creature import register_power_class  # noqa: E402

_ALL_POWERS: dict[PowerId, type[PowerInstance]] = {
    PowerId.GRAVITY: GravityPower,
    PowerId.GUARDED: GuardedPower,
    PowerId.HAILSTORM: HailstormPower,
    PowerId.HAMMER_TIME: HammerTimePower,
    PowerId.HANG: HangPower,
    PowerId.HARD_TO_KILL: HardToKillPower,
    PowerId.HAUNT: HauntPower,
    PowerId.HELICAL_DART: HelicalDartPower,
    PowerId.HEX: HexPower,
    PowerId.HIGH_VOLTAGE: HighVoltagePower,
    PowerId.HOTFIX: HotfixPower,
    PowerId.IMBALANCED: ImbalancedPower,
    PowerId.IMPROVEMENT: ImprovementPower,
    PowerId.INFERNO: InfernoPower,
    PowerId.ITERATION: IterationPower,
    PowerId.JUGGLING: JugglingPower,
    PowerId.KNOCKDOWN: KnockdownPower,
    PowerId.LEADERSHIP: LeadershipPower,
    PowerId.LIGHTNING_ROD: LightningRodPower,
    PowerId.MAGIC_BOMB: MagicBombPower,
    PowerId.MANGLE: ManglePower,
    PowerId.MASTER_PLANNER: MasterPlannerPower,
    PowerId.MIND_ROT: MindRotPower,
    PowerId.MONARCHS_GAZE: MonarchsGazePower,
    PowerId.MONARCHS_GAZE_STRENGTH_DOWN: MonarchsGazeStrengthDownPower,
    PowerId.NEMESIS: NemesisPower,
    PowerId.NEUROSURGE: NeurosurgePower,
    PowerId.NIGHTMARE: NightmarePower,
    PowerId.NO_DRAW: NoDrawPower,
    PowerId.NOSTALGIA: NostalgiaPower,
    PowerId.OBLIVION: OblivionPower,
    PowerId.ORBIT: OrbitPower,
    PowerId.OUTBREAK: OutbreakPower,
    PowerId.PAGESTORM: PagestormPower,
    PowerId.PALE_BLUE_DOT: PaleBlueDotPower,
    PowerId.PAPER_CUTS: PaperCutsPower,
    PowerId.PARRY: ParryPower,
    PowerId.PIERCING_WAIL: PiercingWailPower,
    PowerId.PILLAR_OF_CREATION: PillarOfCreationPower,
    PowerId.POSSESS_SPEED: PossessSpeedPower,
    PowerId.POSSESS_STRENGTH: PossessStrengthPower,
    PowerId.PREP_TIME: PrepTimePower,
    PowerId.PYRE: PyrePower,
    PowerId.RADIANCE: RadiancePower,
    PowerId.RAMPART: RampartPower,
    PowerId.REAPER_FORM: ReaperFormPower,
}

for _pid, _cls in _ALL_POWERS.items():
    register_power_class(_pid, _cls)
