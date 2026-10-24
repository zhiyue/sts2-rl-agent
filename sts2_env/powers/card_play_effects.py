"""Powers triggered by card play, card exhaust, and related events.

Covers: CorruptionPower, EchoFormPower, DarkEmbracePower, FeelNoPainPower,
RagePower, AfterimagePower, RupturePower, JuggernautPower, EnvenomPower,
ThunderPower, ElectrodynamicsPower, PhantomBladesPower, SerpentFormPower,
SetupStrikePower, OneTwoPunchPower, FreeAttackPower, FreeSkillPower,
FreePowerPower, SneakyPower, HeistPower, NecroMasteryPower, HellraiserPower,
NoEnergyGainPower, BorrowedTimePower.

All logic verified against decompiled C# source.
"""

from __future__ import annotations
from typing import TYPE_CHECKING

from sts2_env.core.enums import (
    CardPilePosition,
    CardTag,
    CardType,
    CombatSide,
    OrbType,
    PileType,
    PowerId,
    PowerType,
    PowerStackType,
    ValueProp,
)
from sts2_env.powers.base import PowerInstance

if TYPE_CHECKING:
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
        if getattr(card, "owner", None) is owner and getattr(card, "card_type", None) == CardType.SKILL:
            return 0
        return None

    def modify_card_play_result_pile_type_and_position(
        self,
        owner: Creature,
        card: object,
        is_auto_play: bool,
        energy_value: int,
        pile_type: PileType,
        position: CardPilePosition,
    ) -> tuple[PileType, CardPilePosition]:
        if getattr(card, "owner", None) is not owner:
            return pile_type, position
        if getattr(card, "card_type", None) != CardType.SKILL:
            return pile_type, position
        return PileType.EXHAUST, position


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

    def modify_card_play_count(self, owner: Creature, count: int, card: object) -> int:
        if getattr(card, "owner", None) is not owner:
            return count
        combat = owner.combat_state
        started_this_turn = combat.count_card_play_starts_this_turn(owner, first_in_series_only=True)
        if started_this_turn < self.amount:
            return count + 1
        return count


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
        self._ethereal_exhaust_count: int = 0

    def after_card_exhausted(
        self, owner: Creature, card: object, combat: CombatState
    ) -> None:
        card_owner = getattr(card, "owner", None)
        if card_owner is None:
            for state in combat.combat_player_states:
                if any(card in pile for pile in state.all_piles):
                    card_owner = state.creature
                    break
        if card_owner is not owner:
            return
        if getattr(card, "combat_vars", {}).get("_exhausted_by_ethereal"):
            self._ethereal_exhaust_count += 1
            return
        combat.draw_cards(owner, self.amount)

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        if side != CombatSide.PLAYER:
            return
        if self._ethereal_exhaust_count > 0:
            combat.draw_cards(owner, self.amount * self._ethereal_exhaust_count)
            self._ethereal_exhaust_count = 0


# ---------------------------------------------------------------------------
# FeelNoPainPower
# ---------------------------------------------------------------------------
class FeelNoPainPower(PowerInstance):
    """Whenever one of the owner's cards is Exhausted, gain Amount Block
    (unpowered).

    C# ref: FeelNoPainPower.cs:19-25
    - AfterCardExhausted: only when card.Owner.Creature == base.Owner, gain
      Block(Amount, Unpowered).
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.FEEL_NO_PAIN, amount)

    def after_card_exhausted(
        self, owner: Creature, card: object, combat: CombatState
    ) -> None:
        # C# FeelNoPainPower.cs:21: card.Owner.Creature == base.Owner.
        if getattr(card, "owner", None) is not owner:
            return
        _gain_unpowered_block(owner, self.amount, combat)


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
        if getattr(card, "owner", None) is owner and getattr(card, "card_type", None) == CardType.ATTACK:
            _gain_unpowered_block(owner, self.amount, combat)

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
        self._amounts_for_played_cards: dict[int, int] = {}

    def before_card_played(
        self, owner: Creature, card: object, combat: CombatState
    ) -> None:
        if getattr(card, "owner", None) is owner:
            self._amounts_for_played_cards[id(card)] = self.amount

    def after_card_played(
        self, owner: Creature, card: object, combat: CombatState
    ) -> None:
        amount = self._amounts_for_played_cards.pop(id(card), 0)
        if amount > 0:
            _gain_unpowered_block(owner, amount, combat)


# ---------------------------------------------------------------------------
# RupturePower
# ---------------------------------------------------------------------------
class RupturePower(PowerInstance):
    """Whenever you lose HP during your turn, gain Amount Strength.

    C# ref: RupturePower.cs
    - BeforeCardPlayed: track owner-played cards.
    - AfterDamageReceived: owner HP loss during own turn grants Strength.
      HP loss caused by the card being played is paid out after that card.
    - AfterCardPlayed: apply tracked Strength for that card.
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.RUPTURE, amount)
        self._pending_by_card: dict[int, int] = {}

    def before_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        if getattr(card, "owner", None) is owner and combat.is_owner_side_turn(owner):
            self._pending_by_card[id(card)] = 0

    def after_damage_received(
        self,
        owner: Creature,
        target: Creature,
        dealer: Creature | None,
        damage: int,
        props: ValueProp,
        combat: CombatState,
    ) -> None:
        if target is not owner or damage <= 0 or not combat.is_owner_side_turn(owner):
            return
        card = getattr(combat, "active_card_source", None)
        if card is not None and id(card) in self._pending_by_card:
            self._pending_by_card[id(card)] += self.amount
            return
        owner.apply_power(PowerId.STRENGTH, self.amount)

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        if getattr(card, "owner", None) is not owner:
            return
        amount = self._pending_by_card.pop(id(card), 0)
        if amount > 0:
            owner.apply_power(PowerId.STRENGTH, amount)


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
            enemies = combat.hittable_enemies
            if enemies:
                target = combat.combat_targets_rng.choice(enemies)
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
        if dealer is owner and props.is_powered_attack() and damage > 0:
            combat.apply_power_to(target, PowerId.POISON, self.amount, applier=owner)


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
        orb_type: OrbType,
        targets: list[Creature],
        combat: CombatState,
    ) -> None:
        """Called by the orb system when a Lightning orb is evoked."""
        if orb_type == OrbType.LIGHTNING:
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
    - AfterApplied (PhantomBladesPower.cs:42-49): add Retain to all Shivs the
      owner's player already owns.
    - AfterCardEnteredCombat (PhantomBladesPower.cs:28-40): Shivs owned by the
      power holder's player gain Retain as they enter combat (covers Shivs
      generated after this power was applied).
    - ModifyDamageAdditive: +Amount for first Shiv played per turn
    StackType.Counter.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.PHANTOM_BLADES, amount)
        self._shiv_played_this_turn: bool = False

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
        if owner is not target or power_id != PowerId.PHANTOM_BLADES or amount <= 0:
            return
        state = combat.combat_player_state_for(owner)
        if state is None:
            return
        for pile in state.all_piles:
            for card in pile:
                if getattr(card, "is_shiv", False):
                    card.keywords = frozenset(set(card.keywords) | {"retain"})

    def after_card_entered_combat(self, owner: Creature, card: object, combat: CombatState) -> None:
        # C# PhantomBladesPower.cs:28-40 AfterCardEnteredCombat: Shivs owned
        # by the holder's player get Retain when they enter combat, so Shivs
        # generated after application keep the buff.
        if not getattr(card, "is_shiv", False):
            return
        if getattr(card, "owner", None) is not owner:
            return
        card.keywords = frozenset(set(card.keywords) | {"retain"})

    def modify_damage_additive(
        self,
        owner: Creature,
        dealer: Creature | None,
        target: Creature,
        props: ValueProp,
    ) -> int:
        if dealer is not owner or not props.is_powered_attack() or self._shiv_played_this_turn:
            return 0
        combat = getattr(owner, "combat_state", None)
        if combat is not None and combat.has_card_with_tag_finished_this_turn(owner, CardTag.SHIV):
            return 0
        card = getattr(owner.combat_state, "active_card_source", None)
        tags = getattr(card, "tags", set())
        if CardTag.SHIV not in tags:
            return 0
        return self.amount

    def after_card_played(
        self, owner: Creature, card: object, combat: CombatState
    ) -> None:
        if getattr(card, "owner", None) is owner and getattr(card, "is_shiv", False):
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
        self._amounts_for_played_cards: dict[int, int] = {}

    def before_card_played(
        self, owner: Creature, card: object, combat: CombatState
    ) -> None:
        if getattr(card, "owner", None) is owner:
            self._amounts_for_played_cards[id(card)] = self.amount

    def after_card_played(
        self, owner: Creature, card: object, combat: CombatState
    ) -> None:
        amount = self._amounts_for_played_cards.pop(id(card), 0)
        if amount > 0:
            enemies = combat.hittable_enemies
            target = combat.combat_targets_rng.choice(enemies) if enemies else None
            if target is None:
                return
            combat.deal_damage(
                dealer=owner,
                target=target,
                amount=amount,
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
    is_temporary = True

    def __init__(self, amount: int):
        super().__init__(PowerId.SETUP_STRIKE, amount)

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
        if owner is target and power_id == self.power_id and amount != 0 and not self.consume_ignore_next_instance():
            owner.apply_power(PowerId.STRENGTH, amount, applier=applier, source=source)

    def after_turn_end(
        self, owner: Creature, side: CombatSide, combat: CombatState
    ) -> None:
        if side == owner.side:
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
        if (
            self.amount > 0
            and getattr(card, "owner", None) is owner
            and getattr(card, "card_type", None) == CardType.ATTACK
        ):
            return count + 1
        return count

    def after_modifying_card_play_count(self, owner: Creature, card: object, combat: CombatState) -> None:
        if (
            self.amount > 0
            and getattr(card, "owner", None) is owner
            and getattr(card, "card_type", None) == CardType.ATTACK
        ):
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
            and getattr(card, "owner", None) is owner
            and getattr(card, "card_type", None) == CardType.ATTACK
        ):
            return 0
        return None

    def before_card_played(
        self, owner: Creature, card: object, combat: CombatState
    ) -> None:
        if (
            getattr(card, "owner", None) is owner
            and getattr(card, "card_type", None) == CardType.ATTACK
            and self.amount > 0
        ):
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
            and getattr(card, "owner", None) is owner
            and getattr(card, "card_type", None) == CardType.SKILL
        ):
            return 0
        return None

    def before_card_played(
        self, owner: Creature, card: object, combat: CombatState
    ) -> None:
        if (
            getattr(card, "owner", None) is owner
            and getattr(card, "card_type", None) == CardType.SKILL
            and self.amount > 0
        ):
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
            and getattr(card, "owner", None) is owner
            and getattr(card, "card_type", None) == CardType.POWER
        ):
            return 0
        return None

    def before_card_played(
        self, owner: Creature, card: object, combat: CombatState
    ) -> None:
        if (
            getattr(card, "owner", None) is owner
            and getattr(card, "card_type", None) == CardType.POWER
            and self.amount > 0
        ):
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
            _gain_unpowered_block(owner, self.amount, combat)


# ---------------------------------------------------------------------------
# HeistPower
# ---------------------------------------------------------------------------
class HeistPower(PowerInstance):
    """When this creature dies, the gold it stole is added as a reward.

    C# ref: HeistPower.cs
    - BeforeDeath: add gold reward to combat room.
    StackType.Counter. Amount = gold stolen.

    The simulator adds the reward from the BeforeDeath hook.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.HEIST, amount)
        self.gold_by_player: dict[Creature, int] = {}

    def add_stolen_gold(self, player: Creature, amount: int) -> None:
        if amount <= 0:
            return
        self.gold_by_player[player] = self.gold_by_player.get(player, 0) + amount

    def before_death(self, owner: Creature, creature: Creature, combat: CombatState) -> None:
        if creature is not owner or not hasattr(combat.room, "add_extra_reward"):
            return
        from sts2_env.run.reward_objects import GoldReward

        rewards = self.gold_by_player or {combat.primary_player: self.amount}
        for player, amount in rewards.items():
            if amount <= 0:
                continue
            state = combat.combat_player_state_for(player)
            player_id = state.player_state.player_id if state is not None else combat.player_id
            combat.room.add_extra_reward(player_id, GoldReward(player_id, amount, amount))


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

    def after_current_hp_changed(
        self,
        owner: Creature,
        creature: Creature,
        delta: int,
        combat: CombatState,
    ) -> None:
        if (
            getattr(creature, "is_osty", False)
            and getattr(creature, "pet_owner", None) is owner
            and delta < 0
        ):
            for enemy in combat.hittable_enemies:
                combat.deal_damage(
                    dealer=owner,
                    target=enemy,
                    amount=-delta * self.amount,
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
# NoEnergyGainPower
# ---------------------------------------------------------------------------
class NoEnergyGainPower(PowerInstance):
    """The owner cannot gain energy for the rest of the side's turn.

    C# ref: NoEnergyGainPower.cs
    - PowerType.Debuff, PowerStackType.Single.
    - ModifyEnergyGain: returns 0 for the owner's player, passes other players
      through (multiplayer semantics).
    - AfterSideTurnEnd: removed when the owner is among the participants, i.e.
      at the end of the owner's side turn.
    Applied by ExpectAFight.cs OnPlay (after gaining its own CalculatedEnergy).
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.SINGLE

    def __init__(self, amount: int):
        super().__init__(PowerId.NO_ENERGY_GAIN, amount)

    def modify_energy_gain(self, owner: Creature, player: Creature, amount: int) -> int:
        if player is not owner:
            return amount
        return 0

    def after_turn_end(
        self, owner: Creature, side: CombatSide, combat: CombatState
    ) -> None:
        # C# AfterSideTurnEnd removes the power when the owner participates in
        # the side that just ended; in the sim, fire_after_turn_end is invoked
        # once per side, so remove when the owner's side ended.
        if side == owner.side:
            owner.powers.pop(self.power_id, None)


# ---------------------------------------------------------------------------
# BorrowedTimePower
# ---------------------------------------------------------------------------
# PowerId.BORROWED_TIME does not exist yet in sts2_env/core/enums.py
# (BORROWED_TIME is currently only a CardId — v0.107.1 gap, reported). Until
# the enum member lands, fall back to a stable string key; the key is
# registered either way so apply_power can create instances and the
# BorrowedTime card can wire the real power (import BORROWED_TIME_POWER_ID,
# never hardcode the raw key).
BORROWED_TIME_POWER_ID = (
    PowerId.BORROWED_TIME
    if "BORROWED_TIME" in PowerId.__members__
    else "BORROWED_TIME"
)


class BorrowedTimePower(PowerInstance):
    """Cards played by the owner cost Amount extra energy until the end of the
    owner's side turn.

    C# ref: BorrowedTimePower.cs (v0.107.1)
    - PowerType.Debuff, PowerStackType.Counter (one stack per ExtraCost).
    - TryModifyEnergyCostInCombat: `if (card.Owner.Creature != base.Owner)
      return false;` otherwise `modifiedCost = originalCost + Amount`.
    - ExtraHoverTips: HoverTipFactory.ForEnergy (cost bump is shown as energy).
    - AfterSideTurnEnd: removed when the owner is among the participants of
      the side whose turn just ended, i.e. at the end of the owner's own side
      turn (differs from TaintedPower, which keys on side == Enemy).
    Applied by BorrowedTime.cs OnPlay: gain Energy (4, upgraded 6) then
    Apply<BorrowedTimePower>(ExtraCost = 1).

    Sim wiring: the engine surfaces C# TryModifyEnergyCostInCombat as
    CombatState.modified_card_cost -> PowerInstance.modify_card_cost, which is
    consulted for both the can-play check and the energy actually spent.
    sts2_env/cards/necrobinder.py's borrowed_time effect applies this power
    (BorrowedTime.cs OnPlay).
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(BORROWED_TIME_POWER_ID, amount)

    def modify_card_cost(self, owner: Creature, card: object) -> int | None:
        # CombatState.modified_card_cost only consults the card owner's own
        # powers, which mirrors the C# guard (card.Owner.Creature != Owner ->
        # no modification). X-cost cards are skipped by the dispatcher, and
        # C# BorrowedTimePower does not override TryModifyStarCost, so star
        # costs are intentionally untouched.
        card_cost = getattr(card, "cost", None)
        if not isinstance(card_cost, int):
            return None
        return max(0, card_cost) + self.amount

    def after_turn_end(
        self, owner: Creature, side: CombatSide, combat: CombatState
    ) -> None:
        # C# AfterSideTurnEnd removes when the owner participates in the side
        # that just ended; dropping to 0 lets fire_after_turn_end remove the
        # whole (possibly multi-stack) power (EntangledPower pattern).
        if side == owner.side:
            self.amount = 0


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
    PowerId.NO_ENERGY_GAIN: NoEnergyGainPower,
}

_ALL_POWERS[BORROWED_TIME_POWER_ID] = BorrowedTimePower

for _pid, _cls in _ALL_POWERS.items():
    register_power_class(_pid, _cls)
