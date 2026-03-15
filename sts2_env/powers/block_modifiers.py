"""Block-modifier, HP-loss-modifier, block-clearing, and debuff-blocking powers.

Block modifiers:  DexterityPower, FrailPower, BlurPower, NoBlockPower, ColossusPower
HP loss modifiers: IntangiblePower, BufferPower
Block clearing:   BarricadePower
Debuff blocking:  ArtifactPower

Dexterity, Frail, Artifact are already registered in common.py; they are NOT
re-registered here.  This module adds the powers that common.py does not
contain.
"""

from __future__ import annotations

import math
from typing import TYPE_CHECKING

from sts2_env.core.enums import (
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


# ── BlurPower ────────────────────────────────────────────────────────────
class BlurPower(PowerInstance):
    """Block is not removed at the start of turn.  Decrements each player turn.

    C#:
      - ShouldClearBlock: returns False when creature is owner, preventing
        block removal.
      - AfterSideTurnStart (Player side): decrements by 1.

    In our hook system, ``should_clear_block`` returns False to prevent
    clearing, and we tick down on the player's turn start.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.BLUR, amount)

    def should_clear_block(self, owner: Creature, creature: Creature) -> bool | None:
        if owner is not creature:
            return None  # no opinion on other creatures
        return False  # prevent block clearing for owner

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        """Decrement at start of player turn (C#: AfterSideTurnStart, Player)."""
        if side == CombatSide.PLAYER:
            self.amount -= 1


# ── NoBlockPower ─────────────────────────────────────────────────────────
class NoBlockPower(PowerInstance):
    """Prevents the owner from gaining block from card sources.

    C#: ModifyBlockMultiplicative returns 0.0 when:
      - target is owner
      - props does NOT have Unpowered flag
      - cardSource is not None (i.e. block came from a card)

    Ticks down at end of enemy turn.

    In our simplified sim, cards always set the MOVE prop, so checking
    ``is_powered()`` covers the "from a card source" requirement.
    """

    power_type = PowerType.DEBUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.NO_BLOCK, amount)

    def modify_block_multiplicative(
        self, owner: Creature, target: Creature, props: ValueProp,
        card_source: object | None = None, card_play: object | None = None,
    ) -> float:
        if target is not owner:
            return 1.0
        if bool(props & ValueProp.UNPOWERED):
            return 1.0
        # C# checks cardSource != null; in our sim, MOVE flag indicates
        # card/monster-move origin.
        if not bool(props & ValueProp.MOVE):
            return 1.0
        return 0.0

    def on_turn_end_enemy_side(self, owner: Creature) -> None:
        if self.skip_next_tick:
            self.skip_next_tick = False
            return
        self.amount -= 1


# ── ColossusPower ────────────────────────────────────────────────────────
class ColossusPower(PowerInstance):
    """Halves powered attack damage from Vulnerable attackers.

    C#: ModifyDamageMultiplicative returns 0.5 when:
      - target is owner
      - attack is powered
      - dealer is not None
      - dealer has VulnerablePower

    Ticks down at end of enemy turn.

    This is a defensive multiplier: if the attacker is Vulnerable, the
    damage this creature receives is halved.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.COLOSSUS, amount)

    def modify_damage_multiplicative(
        self, owner: Creature, dealer: Creature | None, target: Creature, props: ValueProp
    ) -> float:
        if target is not owner:
            return 1.0
        if not props.is_powered():
            return 1.0
        if dealer is None:
            return 1.0
        if not dealer.has_power(PowerId.VULNERABLE):
            return 1.0
        return 0.5

    def on_turn_end_enemy_side(self, owner: Creature) -> None:
        if self.skip_next_tick:
            self.skip_next_tick = False
            return
        self.amount -= 1


# ── IntangiblePower ──────────────────────────────────────────────────────
class IntangiblePower(PowerInstance):
    """All damage and HP loss is reduced to 1 (or 5 if attacker has The Boot relic).

    C#:
      - ModifyHpLostAfterOsty: caps HP loss at min(cap, amount).
      - ModifyDamageCap: returns 1 (or 5 with The Boot) for the owner.
      - AfterTurnEnd (Enemy side): ticks down.

    In our hook system we use ``modify_hp_lost`` to cap all incoming damage,
    and ``modify_damage_cap`` for the raw damage cap.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    # Base cap is 1; The Boot relic raises it to 5.
    _DEFAULT_CAP = 1

    def __init__(self, amount: int):
        super().__init__(PowerId.INTANGIBLE, amount)

    def _get_damage_cap(self, dealer: Creature | None) -> int:
        """Return damage cap, accounting for The Boot relic on dealer."""
        # In the full engine, The Boot relic on the attacker's player
        # raises the cap from 1 to 5.  We check for a relic flag.
        if dealer is not None:
            has_boot = getattr(dealer, "_has_the_boot", False)
            if has_boot:
                return 5
        return self._DEFAULT_CAP

    def modify_hp_lost(
        self, owner: Creature, target: Creature, amount: float,
        dealer: Creature | None, props: ValueProp
    ) -> float:
        if target is not owner:
            return amount
        cap = self._get_damage_cap(dealer)
        return min(float(cap), amount)

    def modify_damage_cap(
        self, owner: Creature, dealer: Creature | None, target: Creature,
        damage: float, props: ValueProp
    ) -> float:
        if target is not owner:
            return float("inf")
        return float(self._get_damage_cap(dealer))

    def on_turn_end_enemy_side(self, owner: Creature) -> None:
        if self.skip_next_tick:
            self.skip_next_tick = False
            return
        self.amount -= 1


# ── BufferPower ──────────────────────────────────────────────────────────
class BufferPower(PowerInstance):
    """Prevents all HP loss once, then decrements.

    C#: ModifyHpLostAfterOstyLate returns 0 when target is owner.
    AfterModifyingHpLostAfterOsty decrements by 1.

    The "Late" modifier runs after IntangiblePower's modifier.  In our
    pipeline, modify_hp_lost is called in power-iteration order; Buffer
    should be registered after Intangible or the pipeline should call it
    in a second pass.  For simplicity, we apply it in the standard
    modify_hp_lost hook.
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.COUNTER

    def __init__(self, amount: int):
        super().__init__(PowerId.BUFFER, amount)

    def modify_hp_lost(
        self, owner: Creature, target: Creature, amount: float,
        dealer: Creature | None, props: ValueProp
    ) -> float:
        if target is not owner:
            return amount
        if amount <= 0:
            return amount
        # Absorb ALL HP loss, then decrement self.
        self.amount -= 1
        return 0.0


# ── BarricadePower ───────────────────────────────────────────────────────
class BarricadePower(PowerInstance):
    """Block is not removed at the start of turn.

    C#: ShouldClearBlock returns False when creature == owner.
    PowerStackType is Single (only one instance, amount is always 1).
    """

    power_type = PowerType.BUFF
    stack_type = PowerStackType.SINGLE

    def __init__(self, amount: int = 1):
        super().__init__(PowerId.BARRICADE, amount)

    def should_clear_block(self, owner: Creature, creature: Creature) -> bool | None:
        if owner is not creature:
            return None
        return False


# ─── Registration ────────────────────────────────────────────────────────
from sts2_env.core.creature import register_power_class  # noqa: E402

_BLOCK_HP_POWERS: dict[PowerId, type[PowerInstance]] = {
    PowerId.BLUR: BlurPower,
    PowerId.NO_BLOCK: NoBlockPower,
    PowerId.COLOSSUS: ColossusPower,
    PowerId.INTANGIBLE: IntangiblePower,
    PowerId.BUFFER: BufferPower,
    PowerId.BARRICADE: BarricadePower,
}

for _pid, _cls in _BLOCK_HP_POWERS.items():
    register_power_class(_pid, _cls)
