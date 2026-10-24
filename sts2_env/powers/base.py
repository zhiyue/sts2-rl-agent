"""Power instance base class with all hook method stubs.

Every power, relic, and other hook-bearing object should inherit from this
and override only the hooks it needs. The Hook dispatcher iterates all
listeners and calls these methods.
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from sts2_env.core.enums import CardPilePosition, PileType, PowerId, PowerType, PowerStackType, CombatSide, ValueProp

if TYPE_CHECKING:
    from sts2_env.core.creature import Creature
    from sts2_env.core.combat import CombatState
    from sts2_env.potions.base import PotionInstance


class PowerInstance:
    """A single power (buff/debuff) instance on a creature.

    Subclass and override hook methods to implement specific power behavior.
    All hook methods are no-ops by default.
    """

    power_id: PowerId
    power_type: PowerType = PowerType.BUFF
    stack_type: PowerStackType = PowerStackType.COUNTER
    allow_negative: bool = False
    is_temporary: bool = False
    should_scale_in_multiplayer: bool = False

    def __init__(self, power_id: PowerId, amount: int):
        self.power_id = power_id
        self.amount = amount
        self.skip_next_tick: bool = False
        self.applier: Creature | None = None
        self.ignore_next_instance: bool = False

    def consume_ignore_next_instance(self) -> bool:
        if not self.ignore_next_instance:
            return False
        self.ignore_next_instance = False
        return True

    # ─── Damage Modification Hooks ──────────────────────────────────────

    def modify_damage_additive(
        self, owner: Creature, dealer: Creature | None, target: Creature, props: ValueProp
    ) -> int:
        """Called during damage additive pass. Return amount to ADD to damage."""
        return 0

    def modify_damage_multiplicative(
        self, owner: Creature, dealer: Creature | None, target: Creature, props: ValueProp
    ) -> float:
        """Called during damage multiplicative pass. Return MULTIPLIER (1.0 = no change)."""
        return 1.0

    def modify_damage_cap(
        self, owner: Creature, dealer: Creature | None, target: Creature, damage: float, props: ValueProp
    ) -> float:
        """Return a damage cap. float('inf') = no cap."""
        return float("inf")

    # ─── Block Modification Hooks ───────────────────────────────────────

    def modify_block_additive(
        self, owner: Creature, target: Creature, props: ValueProp,
        card_source: object | None = None, card_play: object | None = None,
    ) -> int:
        """Called during block additive pass. Return amount to ADD to block."""
        return 0

    def modify_block_multiplicative(
        self, owner: Creature, target: Creature, props: ValueProp,
        card_source: object | None = None, card_play: object | None = None,
        combat: CombatState | None = None,
    ) -> float:
        """Called during block multiplicative pass. Return MULTIPLIER."""
        return 1.0

    def after_modifying_block_amount(
        self,
        owner: Creature,
        modified_amount: int,
        card_source: object | None,
        card_play: object | None,
        combat: CombatState,
    ) -> None:
        pass

    # ─── HP Loss Modification ───────────────────────────────────────────

    def modify_hp_lost(
        self, owner: Creature, target: Creature, amount: float,
        dealer: Creature | None, props: ValueProp
    ) -> float:
        """Modify HP lost after block. Intangible caps at 1, TungstenRod -1, etc."""
        return amount

    def modify_hp_lost_before_osty_late(
        self, owner: Creature, target: Creature, amount: float,
        dealer: Creature | None, props: ValueProp
    ) -> float:
        return amount

    def modify_hp_lost_late(
        self, owner: Creature, target: Creature, amount: float,
        dealer: Creature | None, props: ValueProp
    ) -> float:
        return amount

    def modify_unblocked_damage_target(
        self,
        owner: Creature,
        target: Creature,
        amount: float,
        props: ValueProp,
        dealer: Creature | None,
    ) -> Creature:
        return target

    # ─── Block Clearing ─────────────────────────────────────────────────

    def should_clear_block(self, owner: Creature, creature: Creature) -> bool | None:
        """Return False to prevent block clearing (Barricade). None = no opinion."""
        return None

    def after_preventing_block_clear(
        self,
        owner: Creature,
        creature: Creature,
        combat: CombatState,
    ) -> None:
        pass

    def should_flush(self, owner: Creature, flushing_owner: Creature, combat: CombatState) -> bool | None:
        return None

    # ─── Power Application ──────────────────────────────────────────────

    def try_block_debuff(self, owner: Creature, power_id: PowerId) -> bool:
        """Return True to consume a charge and block a debuff (Artifact)."""
        return False

    # ─── Draw / Energy Modification ─────────────────────────────────────

    def modify_hand_draw(self, owner: Creature, draw: int) -> int:
        """Modify cards drawn at turn start."""
        return draw

    def modify_hand_draw_late(self, owner: Creature, draw: int) -> int:
        return draw

    def after_modifying_hand_draw(self, owner: Creature, combat: CombatState) -> None:
        pass

    def modify_max_energy(self, owner: Creature, energy: int) -> int:
        """Modify max energy."""
        return energy

    def after_energy_reset(self, owner: Creature, combat: CombatState) -> None:
        pass

    def after_energy_reset_late(self, owner: Creature, combat: CombatState) -> None:
        pass

    def after_energy_spent(self, owner: Creature, card: object, amount: int, combat: CombatState) -> None:
        pass

    def should_draw(self, owner: Creature, from_hand_draw: bool) -> bool | None:
        return None

    def after_preventing_draw(self, owner: Creature, combat: CombatState) -> None:
        pass

    # ─── Card Play Count ────────────────────────────────────────────────

    def modify_card_play_count(self, owner: Creature, count: int, card: object) -> int:
        """Modify how many times a card is played (EchoForm)."""
        return count

    def after_modifying_card_play_count(self, owner: Creature, card: object, combat: CombatState) -> None:
        pass

    def after_card_entered_combat(self, owner: Creature, card: object, combat: CombatState) -> None:
        pass

    def after_card_generated_for_combat(
        self,
        owner: Creature,
        card: object,
        added_by_player: bool,
        combat: CombatState,
    ) -> None:
        pass

    # ─── Turn Lifecycle Hooks ───────────────────────────────────────────

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        pass

    def before_hand_draw(self, owner: Creature, combat: CombatState) -> None:
        pass

    def before_hand_draw_late(self, owner: Creature, combat: CombatState) -> None:
        pass

    def after_player_turn_start_early(self, owner: Creature, combat: CombatState) -> None:
        pass

    def after_player_turn_start(self, owner: Creature, combat: CombatState) -> None:
        pass

    def after_player_turn_start_late(self, owner: Creature, combat: CombatState) -> None:
        pass

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        pass

    def before_play_phase_start(self, owner: Creature, player: Creature, combat: CombatState) -> None:
        pass

    def before_turn_end_very_early(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        pass

    def before_turn_end_early(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        pass

    def before_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        pass

    def before_flush(self, owner: Creature, flushing_owner: Creature, combat: CombatState) -> None:
        pass

    def before_flush_late(self, owner: Creature, flushing_owner: Creature, combat: CombatState) -> None:
        pass

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        pass

    def after_turn_end_late(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        pass

    # ─── Legacy turn hooks (for backward compat with existing powers) ───

    def on_turn_end_enemy_side(self, owner: Creature) -> None:
        """Called at AfterTurnEnd when side == ENEMY. Duration powers tick here."""
        pass

    def on_turn_start_own_side(self, owner: Creature, combat: object) -> None:
        """Called at start of owner's side turn. Poison/Ritual fire here."""
        pass

    # ─── Card Hooks ─────────────────────────────────────────────────────

    def before_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        pass

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        pass

    def modify_card_play_result_pile_type_and_position(
        self,
        owner: Creature,
        card: object,
        is_auto_play: bool,
        energy_value: int,
        pile_type: PileType,
        position: CardPilePosition,
    ) -> tuple[PileType, CardPilePosition]:
        return pile_type, position

    def after_modifying_card_play_result_pile_or_position(
        self,
        card: object,
        pile_type: PileType,
        position: CardPilePosition,
        combat: CombatState,
    ) -> None:
        pass

    def after_card_exhausted(self, owner: Creature, card: object, combat: CombatState) -> None:
        pass

    def on_card_drawn(
        self,
        owner: Creature,
        card: object,
        from_hand_draw: bool,
        combat: CombatState,
    ) -> None:
        pass

    def before_potion_used(
        self,
        owner: Creature,
        potion: PotionInstance,
        target: Creature | None,
        combat: CombatState,
    ) -> None:
        pass

    # ─── Damage Event Hooks ─────────────────────────────────────────────

    def before_damage_received(
        self, owner: Creature, target: Creature, dealer: Creature | None,
        damage: int, props: ValueProp, combat: CombatState
    ) -> None:
        """Thorns, FlameBarrier fire here (before block is applied)."""
        pass

    def after_damage_received(
        self, owner: Creature, target: Creature, dealer: Creature | None,
        damage: int, props: ValueProp, combat: CombatState
    ) -> None:
        pass

    def after_current_hp_changed(
        self,
        owner: Creature,
        creature: Creature,
        delta: int,
        combat: CombatState,
    ) -> None:
        pass

    def after_damage_given(
        self, owner: Creature, dealer: Creature, target: Creature,
        damage: int, props: ValueProp, combat: CombatState
    ) -> None:
        pass

    # ─── Attack Command Hooks ───────────────────────────────────────────

    def before_attack(self, owner: Creature, attack: object, combat: CombatState) -> None:
        pass

    def after_attack(self, owner: Creature, attack: object, combat: CombatState) -> None:
        pass

    # ─── Block Event Hooks ──────────────────────────────────────────────

    def after_block_gained(self, owner: Creature, creature: Creature, amount: int, combat: CombatState) -> None:
        pass

    def after_block_cleared(self, owner: Creature, creature: Creature, combat: CombatState) -> None:
        pass

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
        pass

    # ─── Combat Lifecycle ───────────────────────────────────────────────

    def after_combat_victory_early(self, owner: Creature, combat: CombatState) -> None:
        pass

    def after_combat_victory(self, owner: Creature, combat: CombatState) -> None:
        pass

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        pass

    def before_combat_start_late(self, owner: Creature, combat: CombatState) -> None:
        pass

    def after_creature_added_to_combat(
        self,
        owner: Creature,
        creature: Creature,
        combat: CombatState,
    ) -> None:
        pass

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        pass

    def after_forge(
        self,
        owner: Creature,
        amount: int,
        forger: Creature,
        source: object | None,
        combat: CombatState,
    ) -> None:
        pass

    def on_stars_gained(self, owner: Creature, stars: int, combat: CombatState) -> None:
        pass

    def on_stars_spent(self, owner: Creature, stars: int, combat: CombatState) -> None:
        pass

    def should_owner_death_trigger_fatal(
        self,
        owner: Creature,
        combat: CombatState,
    ) -> bool:
        return True

    def should_allow_hitting(self, owner: Creature, combat: CombatState) -> bool:
        return True

    def should_power_be_removed_after_owner_death(
        self,
        owner: Creature,
        combat: CombatState,
    ) -> bool:
        return True

    def should_other_power_be_removed_on_owner_death(
        self,
        owner: Creature,
        power: PowerInstance,
        combat: CombatState,
    ) -> bool | None:
        return None

    def should_creature_be_removed_from_combat_after_death(
        self,
        owner: Creature,
        combat: CombatState,
    ) -> bool:
        return True

    def modify_summon_amount(
        self,
        owner: Creature,
        summoner: Creature,
        amount: int,
        source: object | None,
        combat: CombatState,
    ) -> int:
        return amount

    def after_summon(
        self,
        owner: Creature,
        summoner: Creature,
        amount: int,
        combat: CombatState,
    ) -> None:
        pass

    def after_osty_revived(self, owner: Creature, osty: Creature, combat: CombatState) -> None:
        pass

    # ─── Display ────────────────────────────────────────────────────────

    def __repr__(self) -> str:
        # power_id is a PowerId for registered powers, but v0.107.1 powers
        # awaiting an enum member (BorrowedTimePower, TaintedPower,
        # WitheringPresencePower) use a stable string key instead; both must
        # render here.
        name = getattr(self.power_id, "name", self.power_id)
        return f"{name}({self.amount})"
