"""CombatState: the central combat simulation engine.

This module intentionally mirrors the decompiled `CombatManager` turn flow
closely enough for headless simulation: start-of-side hooks, energy reset,
hand draw modification, player end-turn phase separation, enemy end-turn
hooks, and hook-driven relic/power dispatch.
"""

from __future__ import annotations

from contextlib import contextmanager
from dataclasses import dataclass
from typing import TYPE_CHECKING, Callable, Sequence

from sts2_env.cards.base import (
    CardInstance,
    capture_self_mutating_card_progress,
    new_card_instance_id,
    restore_self_mutating_card_progress,
)
from sts2_env.cards.enchantments import (
    after_card_played as apply_enchantment_after_card_played,
    after_player_turn_start as apply_enchantment_after_player_turn_start,
    before_flush as apply_enchantment_before_flush,
    enchant_play_count_bonus,
    modify_shuffle_order as apply_enchantment_shuffle_order,
    on_card_drawn as apply_enchantment_on_card_drawn,
    on_card_played as apply_enchantment_on_card_played,
    reset_combat_enchantments,
)
from sts2_env.cards.factory import (
    create_card,
    create_cards_from_ids,
    create_character_cards,
    create_distinct_character_cards,
    create_transform_card,
    eligible_registered_cards,
    eligible_transform_cards,
)
from sts2_env.cards.registry import (
    fire_card_after_card_entered_combat,
    fire_card_after_card_played,
    fire_card_after_turn_end,
    fire_card_before_card_played,
    fire_card_before_hand_draw,
    fire_card_late_effects,
    fire_card_turn_end_in_hand,
    play_card_effect,
)
from sts2_env.characters.all import ALL_CHARACTERS, get_character
from sts2_env.core.attack import AttackContext
from sts2_env.core.card_pools import CardPoolId
from sts2_env.core.combat_player import CombatPlayerState
from sts2_env.core.constants import BASE_DRAW, BASE_ENERGY, MAX_HAND_SIZE
from sts2_env.core.creature import Creature
from sts2_env.core.enums import (
    CardPilePosition,
    CardId,
    CardTag,
    CardType,
    CombatSide,
    PileType,
    PotionTargetType,
    PowerId,
    PowerType,
    TargetType,
    ValueProp,
)
from sts2_env.core.rng import INT_MAX, Rng
from sts2_env.core.selection import CardChoiceOption, PendingCardChoice
from sts2_env.potions.base import PotionInstance
from sts2_env.relics.base import RelicId

if TYPE_CHECKING:
    from sts2_env.monsters.state_machine import MonsterAI
    from sts2_env.relics.base import RelicId, RelicInstance
    from sts2_env.run.run_state import PlayerState


_NO_CARD_PLAY_FILTER = object()
_NO_CARD_FILTER = object()

_STUNNED_MOVE_ID = "STUNNED"


@dataclass(frozen=True)
class CardPlayStartedEntry:
    card: CardInstance
    is_first_in_series: bool
    energy_value: int


@dataclass(frozen=True)
class CardPlayFinishedEntry:
    card: CardInstance
    was_ethereal: bool
    round_number: int


@dataclass(frozen=True)
class CardPlayResult:
    pile_type: PileType
    position: CardPilePosition


class CombatState:
    """Full state of a single combat encounter."""

    def __init__(
        self,
        player_hp: int,
        player_max_hp: int,
        deck: list[CardInstance],
        rng_seed: int,
        relics: Sequence[str | RelicId | RelicInstance] | None = None,
        gold: int = 0,
        character_id: str | None = None,
        potions: Sequence[PotionInstance | None] | None = None,
        max_potion_slots: int = 3,
        player_state: PlayerState | None = None,
        ally_players: Sequence[PlayerState | Creature] | None = None,
        room: object | None = None,
        ascension_level: int = 0,
    ):
        self.rng = Rng(rng_seed)
        self.room = room
        self.ascension_level = ascension_level

        resolved_character_id = character_id or self._infer_character_id(
            deck,
            self._coerce_relics(relics or []),
        )
        persistent_player = self._build_player_state(
            player_state=player_state,
            character_id=resolved_character_id,
            player_hp=player_hp,
            player_max_hp=player_max_hp,
            deck=deck,
            relics=relics or (),
            gold=gold,
            potions=potions,
            max_potion_slots=max_potion_slots,
        )

        # Player
        self._root_player = Creature(
            max_hp=persistent_player.max_hp,
            current_hp=persistent_player.current_hp,
            side=CombatSide.PLAYER,
            is_player=True,
        )
        self._root_player.combat_state = self
        self._acting_player: Creature | None = None
        self._acting_player_state: CombatPlayerState | None = None
        self._primary_player_state = self._create_combat_player_state(persistent_player, self._root_player)
        self._ally_player_states: list[CombatPlayerState] = []
        self._combat_player_state_by_creature: dict[Creature, CombatPlayerState] = {
            self._root_player: self._primary_player_state,
        }

        # Enemies
        self.enemies: list[Creature] = []
        self.enemy_ais: dict[int, MonsterAI] = {}
        self.allies: list[Creature] = []
        self.osty: Creature | None = None
        self._ally_player_zones: dict[Creature, dict[str, list[CardInstance]]] = {}

        # Combat state
        self.round_number: int = 1
        self.current_side: CombatSide = CombatSide.PLAYER
        self.is_over: bool = False
        self.player_won: bool = False
        self.turn_count: int = 0
        self._pending_retain_count: dict[int, int] = {}
        self.pending_choice: PendingCardChoice | None = None
        self._pending_play: dict[str, object] | None = None
        self._pending_draw: dict[str, object] | None = None
        self._pending_turn_setup: Callable[[], None] | None = None
        self._end_turn_after_play: bool = False
        self.in_play_phase: bool = False
        self._damage_events_this_turn: list[tuple[Creature | None, Creature, ValueProp]] = []
        self._damage_events_combat: list[tuple[Creature | None, Creature, ValueProp, int]] = []
        self._block_events_this_turn: list[tuple[Creature, ValueProp, object | None]] = []
        self._draw_events_this_turn: list[tuple[Creature, CardInstance, bool]] = []
        self._draw_events_combat: list[Creature] = []
        self._exhaust_events_this_turn: list[CardInstance] = []
        self._discard_events_this_turn: list[CardInstance] = []
        self._stars_gained_this_turn: list[tuple[Creature, int]] = []
        self._power_events_this_turn: list[tuple[Creature, PowerId, int, Creature | None]] = []
        self._generated_cards_combat: list[tuple[Creature, bool]] = []
        self._energy_spent_this_turn: dict[Creature, int] = {}
        self._after_energy_reset_owners_this_turn: set[Creature] = set()
        self._orb_channel_events_combat: list[tuple[Creature, object]] = []
        self._active_card_source: object | None = None
        self._active_card_target: Creature | None = None
        self._card_being_played_for_cost: CardInstance | None = None
        self._attack_context_stack: list[AttackContext] = []
        self._pending_auto_attack: AttackContext | None = None
        self._combat_started: bool = False
        self.extra_card_rewards: int = 0
        self._legacy_extra_card_rewards: int = 0
        self._played_cards_this_turn: list[CardInstance] = []
        self._played_cards_combat: list[CardInstance] = []
        self._card_play_finished_entries_combat: list[CardPlayFinishedEntry] = []
        self._card_play_starts_this_turn: list[CardPlayStartedEntry] = []
        self._card_play_round_counts: dict[tuple[int, Creature], int] = {}

        for ally in ally_players or ():
            self.add_ally_player(ally)

    @property
    def max_energy(self) -> int:
        from sts2_env.core.hooks import modify_max_energy

        return modify_max_energy(self.base_max_energy, self)

    @property
    def current_energy(self) -> int:
        return self.energy

    @property
    def player(self) -> Creature:
        return self._acting_player or self._root_player

    @property
    def primary_player(self) -> Creature:
        return self._root_player

    @property
    def is_multiplayer(self) -> bool:
        return len(self.combat_player_states) > 1

    @property
    def current_player_state(self) -> CombatPlayerState:
        return self._acting_player_state or self._primary_player_state

    @contextmanager
    def acting_player_view(self, owner: Creature):
        state = self.combat_player_state_for(owner)
        if state is None:
            yield
            return
        previous_player = self._acting_player
        previous_state = self._acting_player_state
        self._acting_player = owner
        self._acting_player_state = state
        try:
            yield
        finally:
            self._acting_player = previous_player
            self._acting_player_state = previous_state

    @property
    def combat_player_states(self) -> list[CombatPlayerState]:
        return [self._primary_player_state] + self._ally_player_states

    @property
    def player_id(self) -> int:
        return self._primary_player_state.player_state.player_id

    @property
    def hand(self) -> list[CardInstance]:
        return self.current_player_state.hand

    @hand.setter
    def hand(self, value: list[CardInstance]) -> None:
        self.current_player_state.hand = value
        self.current_player_state.zone_map["hand"] = value

    @property
    def draw_pile(self) -> list[CardInstance]:
        return self.current_player_state.draw

    @draw_pile.setter
    def draw_pile(self, value: list[CardInstance]) -> None:
        self.current_player_state.draw = value
        self.current_player_state.zone_map["draw"] = value

    @property
    def discard_pile(self) -> list[CardInstance]:
        return self.current_player_state.discard

    @discard_pile.setter
    def discard_pile(self, value: list[CardInstance]) -> None:
        self.current_player_state.discard = value
        self.current_player_state.zone_map["discard"] = value

    @property
    def exhaust_pile(self) -> list[CardInstance]:
        return self.current_player_state.exhaust

    @exhaust_pile.setter
    def exhaust_pile(self, value: list[CardInstance]) -> None:
        self.current_player_state.exhaust = value
        self.current_player_state.zone_map["exhaust"] = value

    @property
    def play_pile(self) -> list[CardInstance]:
        return self.current_player_state.play

    @play_pile.setter
    def play_pile(self, value: list[CardInstance]) -> None:
        self.current_player_state.play = value

    @property
    def energy(self) -> int:
        return self.current_player_state.energy

    @energy.setter
    def energy(self, value: int) -> None:
        self.current_player_state.energy = value

    @property
    def base_max_energy(self) -> int:
        return self.current_player_state.base_max_energy

    @base_max_energy.setter
    def base_max_energy(self, value: int) -> None:
        self.current_player_state.base_max_energy = value

    @property
    def stars(self) -> int:
        return self.current_player_state.stars

    @stars.setter
    def stars(self, value: int) -> None:
        self.current_player_state.stars = value

    @property
    def relics(self) -> list[RelicInstance]:
        return self.current_player_state.relics

    @relics.setter
    def relics(self, value: list[RelicInstance]) -> None:
        self.current_player_state.relics = value

    @property
    def potions(self) -> list[PotionInstance | None]:
        return self.current_player_state.potions

    @potions.setter
    def potions(self, value: list[PotionInstance | None]) -> None:
        self.current_player_state.potions = value

    @property
    def max_potion_slots(self) -> int:
        return self.current_player_state.max_potion_slots

    @max_potion_slots.setter
    def max_potion_slots(self, value: int) -> None:
        self.current_player_state.max_potion_slots = value

    @property
    def orb_queue(self) -> object | None:
        return self.current_player_state.orb_queue

    @orb_queue.setter
    def orb_queue(self, value: object | None) -> None:
        self.current_player_state.orb_queue = value

    @property
    def character_id(self) -> str:
        return self.current_player_state.character_id

    @character_id.setter
    def character_id(self, value: str) -> None:
        self.current_player_state.player_state.character_id = value

    @property
    def gold(self) -> int:
        return self.current_player_state.player_state.gold

    @gold.setter
    def gold(self, value: int) -> None:
        self.current_player_state.player_state.gold = value

    @property
    def active_card_source(self) -> object | None:
        return self._active_card_source

    @property
    def active_card_target(self) -> Creature | None:
        if self._active_card_target is not None:
            return self._active_card_target
        if self._pending_play is None:
            return None
        target = self._pending_play.get("target")
        return target if isinstance(target, Creature) else None

    @property
    def active_card_play_token(self) -> object | None:
        return self._pending_play

    @property
    def active_card_play_is_last_in_series(self) -> bool:
        if self._pending_play is None:
            return True
        return int(self._pending_play.get("remaining_plays", 0)) <= 0

    @property
    def active_card_play_is_auto(self) -> bool:
        if self._pending_play is None:
            return False
        return bool(self._pending_play.get("is_auto_play", False))

    @property
    def active_attack(self) -> AttackContext | None:
        if not self._attack_context_stack:
            return None
        return self._attack_context_stack[-1]

    @property
    def pending_auto_attack(self) -> AttackContext | None:
        return self._pending_auto_attack

    @property
    def all_creatures(self) -> list[Creature]:
        return [self.primary_player] + self.allies + self.enemies

    @property
    def all_piles(self) -> tuple[list[CardInstance], ...]:
        return (
            self.hand,
            self.draw_pile,
            self.discard_pile,
            self.exhaust_pile,
            self.play_pile,
        )

    @property
    def alive_enemies(self) -> list[Creature]:
        return [e for e in self.enemies if e.is_alive]

    @property
    def enemies_with_turns(self) -> list[Creature]:
        return [enemy for enemy in self.enemies if not enemy.escaped]

    @property
    def alive_allies(self) -> list[Creature]:
        return [ally for ally in self.allies if ally.is_alive]

    @property
    def hittable_enemies(self) -> list[Creature]:
        return [enemy for enemy in self.alive_enemies if self.can_hit_creature(enemy)]

    def can_hit_creature(self, creature: Creature) -> bool:
        if not creature.is_alive:
            return False
        return all(
            getattr(power, "should_allow_hitting", lambda owner, combat: True)(creature, self)
            for power in creature.powers.values()
        )

    def _run_rng(self, stream_name: str) -> Rng:
        state = getattr(self, "_primary_player_state", None)
        player_state = getattr(state, "player_state", None)
        run_state = getattr(player_state, "run_state", None)
        rng_set = getattr(run_state, "rng", None)
        return getattr(rng_set, stream_name, self.rng)

    @property
    def shuffle_rng(self) -> Rng:
        return self._run_rng("shuffle")

    @property
    def combat_targets_rng(self) -> Rng:
        return self._run_rng("combat_targets")

    @property
    def combat_card_selection_rng(self) -> Rng:
        return self._run_rng("combat_card_selection")

    @property
    def combat_card_generation_rng(self) -> Rng:
        return self._run_rng("combat_card_generation")

    @property
    def combat_energy_costs_rng(self) -> Rng:
        return self._run_rng("combat_energy_costs")

    @property
    def combat_potion_generation_rng(self) -> Rng:
        return self._run_rng("combat_potion_generation")

    @property
    def combat_orbs_rng(self) -> Rng:
        return self._run_rng("combat_orbs")

    @property
    def monster_ai_rng(self) -> Rng:
        return self._run_rng("monster_ai")

    def _push_attack_context(self, attack: AttackContext) -> None:
        from sts2_env.core.hooks import fire_before_attack

        self._attack_context_stack.append(attack)
        fire_before_attack(attack, self)

    def _pop_attack_context(self, attack: AttackContext) -> None:
        from sts2_env.core.hooks import fire_after_attack

        if self._attack_context_stack and self._attack_context_stack[-1] is attack:
            self._attack_context_stack.pop()
        else:
            for idx, candidate in enumerate(self._attack_context_stack):
                if candidate is attack:
                    self._attack_context_stack.pop(idx)
                    break
        fire_after_attack(attack, self)

    @contextmanager
    def attack_context(
        self,
        attacker: Creature | None,
        target: Creature | None,
        props: ValueProp,
        *,
        model_source: object | None = None,
    ):
        attack = AttackContext(
            attacker=attacker,
            target=target,
            damage_props=props,
            model_source=model_source if model_source is not None else self.active_card_source,
        )
        self._push_attack_context(attack)
        try:
            yield attack
        finally:
            self._pop_attack_context(attack)

    def _ensure_pending_attack_context(
        self,
        attacker: Creature | None,
        target: Creature | None,
        props: ValueProp,
    ) -> AttackContext:
        if self.active_attack is not None:
            return self.active_attack
        if self._pending_auto_attack is None:
            self._pending_auto_attack = AttackContext(
                attacker=attacker,
                target=target,
                damage_props=props,
                model_source=self.active_card_source,
            )
            self._push_attack_context(self._pending_auto_attack)
        return self._pending_auto_attack

    def flush_pending_attack_context(self) -> None:
        attack = self._pending_auto_attack
        if attack is None:
            return
        self._pending_auto_attack = None
        self._pop_attack_context(attack)

    # ---- Setup ----

    def _coerce_relics(
        self,
        relics: Sequence[str | RelicId | RelicInstance],
    ) -> list[RelicInstance]:
        if not relics:
            return []

        from sts2_env.relics.base import RelicInstance
        from sts2_env.relics.registry import create_relic_by_name

        resolved: list[RelicInstance] = []
        for relic in relics:
            if isinstance(relic, RelicInstance):
                resolved.append(relic)
            else:
                resolved.append(create_relic_by_name(relic))
        return resolved

    def _infer_character_id(
        self,
        deck: Sequence[CardInstance],
        relics: Sequence[RelicInstance],
    ) -> str:
        if relics:
            starter_relic_map = {
                config.starting_relic: config.character_id
                for config in ALL_CHARACTERS
            }
            for relic in relics:
                relic_id = getattr(relic, "relic_id", None)
                if relic_id is not None:
                    inferred = starter_relic_map.get(relic_id)
                    if inferred is not None:
                        return inferred

        deck_ids = {card.card_id for card in deck}
        best_match = "Ironclad"
        best_score = -1
        for config in ALL_CHARACTERS:
            score = sum(1 for card_id in deck_ids if card_id in config.card_pool)
            if score > best_score:
                best_match = config.character_id
                best_score = score
        return best_match

    def _normalize_relic_ids(
        self,
        relics: Sequence[str | RelicId | RelicInstance],
    ) -> list[str]:
        normalized: list[str] = []
        for relic in relics:
            relic_id = getattr(relic, "relic_id", relic)
            normalized.append(getattr(relic_id, "name", str(relic_id)))
        return normalized

    def _build_player_state(
        self,
        *,
        player_state: PlayerState | None,
        character_id: str,
        player_hp: int,
        player_max_hp: int,
        deck: Sequence[CardInstance],
        relics: Sequence[str | RelicId | RelicInstance],
        gold: int,
        potions: Sequence[PotionInstance | None] | None,
        max_potion_slots: int,
    ) -> PlayerState:
        from sts2_env.run.run_state import PlayerState

        if player_state is not None:
            if not player_state.deck:
                player_state.deck = list(deck)
            if relics:
                player_state.relics = self._normalize_relic_ids(relics)
            if potions is not None:
                player_state.potions = list(potions)
            player_state.max_hp = player_max_hp
            player_state.current_hp = player_hp
            player_state.gold = gold
            player_state.max_potion_slots = max_potion_slots
            player_state.character_id = character_id
            return player_state

        char_cfg = get_character(character_id)
        return PlayerState(
            character_id=character_id,
            max_hp=player_max_hp,
            current_hp=player_hp,
            gold=gold,
            deck=list(deck),
            relics=self._normalize_relic_ids(relics),
            potions=list(potions or []),
            max_potion_slots=max_potion_slots,
            max_energy=BASE_ENERGY,
            base_orb_slot_count=char_cfg.base_orb_slots,
        )

    def _create_combat_player_state(
        self,
        player_state: PlayerState,
        creature: Creature,
    ) -> CombatPlayerState:
        setattr(player_state, "combat_state", self)
        state = CombatPlayerState(
            player_state=player_state,
            creature=creature,
            starting_deck=list(player_state.deck),
            max_potion_slots=player_state.max_potion_slots,
            base_max_energy=player_state.max_energy or BASE_ENERGY,
        )
        char_cfg = get_character(player_state.character_id)
        if char_cfg.base_orb_slots > 0:
            from sts2_env.orbs.base import OrbQueue

            state.orb_queue = OrbQueue(char_cfg.base_orb_slots)
        return state

    def combat_player_state_for(self, creature: Creature) -> CombatPlayerState | None:
        return self._combat_player_state_by_creature.get(creature)

    def relics_for_creature(self, creature: Creature) -> list[RelicInstance]:
        state = self.combat_player_state_for(creature)
        return state.relics if state is not None else []

    def add_enemy(self, creature: Creature, ai: MonsterAI) -> None:
        """Add an enemy to this combat."""
        from sts2_env.core.hooks import fire_after_creature_added_to_combat
        from sts2_env.core.hooks import scaled_multiplayer_enemy_max_hp
        from sts2_env.core.hooks import scaled_multiplayer_power_amount

        creature.combat_id = len(self.enemies)
        creature.side = CombatSide.ENEMY
        creature.combat_state = self
        scaled_max_hp = scaled_multiplayer_enemy_max_hp(creature, self)
        if scaled_max_hp != creature.max_hp:
            creature.max_hp = scaled_max_hp
            creature.current_hp = scaled_max_hp
        for power in creature.powers.values():
            power.amount = scaled_multiplayer_power_amount(
                power.power_id,
                power.amount,
                creature,
                self,
            )
        self.enemies.append(creature)
        self.enemy_ais[creature.combat_id] = ai
        from sts2_env.monsters.act3 import ZAPBOT_HIGH_VOLTAGE_AMOUNT, ZAPBOT_MONSTER_ID
        from sts2_env.monsters.act4 import GAS_BOMB_MINION_AMOUNT, GAS_BOMB_MONSTER_ID

        if creature.monster_id == GAS_BOMB_MONSTER_ID:
            self.apply_power_to(creature, PowerId.MINION, GAS_BOMB_MINION_AMOUNT, applier=creature)
        if creature.monster_id == ZAPBOT_MONSTER_ID:
            self.apply_power_to(creature, PowerId.HIGH_VOLTAGE, ZAPBOT_HIGH_VOLTAGE_AMOUNT, applier=creature)
        if self._combat_started:
            fire_after_creature_added_to_combat(creature, self)

    def add_ally_player(self, creature: PlayerState | Creature) -> Creature:
        """Register another living player with full combat state."""
        from sts2_env.core.hooks import fire_after_creature_added_to_combat
        from sts2_env.run.run_state import PlayerState

        if isinstance(creature, PlayerState):
            player_state = creature
            existing_ids = {state.player_state.player_id for state in self.combat_player_states}
            if player_state.player_id in existing_ids:
                player_state.player_id = max(existing_ids) + 1
            ally_creature = Creature(
                max_hp=player_state.max_hp,
                current_hp=player_state.current_hp,
                side=CombatSide.PLAYER,
                is_player=True,
            )
        else:
            ally_creature = creature
            existing_ids = {state.player_state.player_id for state in self.combat_player_states}
            next_id = (max(existing_ids) + 1) if existing_ids else 2
            player_state = PlayerState(
                player_id=next_id,
                character_id=self.character_id,
                max_hp=ally_creature.max_hp,
                current_hp=ally_creature.current_hp,
                max_energy=BASE_ENERGY,
            )

        if ally_creature in self._combat_player_state_by_creature:
            return ally_creature

        state = self._create_combat_player_state(player_state, ally_creature)
        creature = ally_creature
        creature.side = CombatSide.PLAYER
        creature.is_player = True
        creature.combat_state = self
        self.allies.append(creature)
        self._ally_player_states.append(state)
        self._combat_player_state_by_creature[creature] = state
        self._ally_player_zones[creature] = state.zone_map
        if self._combat_started:
            fire_after_creature_added_to_combat(creature, self)
        return creature

    def _reset_player_combat_state(self, state: CombatPlayerState) -> None:
        state.energy = 0
        state.stars = 0
        state.hand.clear()
        for card in state.starting_deck:
            reset_combat_enchantments(card)
        state.draw[:] = list(state.starting_deck)
        state.discard.clear()
        state.exhaust.clear()
        state.play.clear()
        self.shuffle_rng.shuffle(state.draw)
        imbued_cards = [card for card in state.draw if card.has_enchantment("Imbued")]
        if imbued_cards:
            state.draw[:] = [card for card in state.draw if not card.has_enchantment("Imbued")] + imbued_cards

    def _draw_opening_hand_for_state(self, state: CombatPlayerState, count: int = BASE_DRAW) -> None:
        innate_cards = [card for card in state.draw if card.is_innate]
        non_innate_cards = [card for card in state.draw if not card.is_innate]
        state.draw[:] = innate_cards + non_innate_cards
        draw_count = min(MAX_HAND_SIZE, max(count, len(innate_cards)))
        for _ in range(draw_count):
            if not state.draw or len(state.hand) >= MAX_HAND_SIZE:
                break
            card = state.draw.pop(0)
            card.owner = state.creature
            state.hand.append(card)

    def _reset_side_turn_history(self) -> None:
        self._damage_events_this_turn = []
        self._block_events_this_turn = []
        self._draw_events_this_turn = []
        self._exhaust_events_this_turn = []
        self._discard_events_this_turn = []
        self._stars_gained_this_turn = []
        self._power_events_this_turn = []
        self._energy_spent_this_turn = {}
        self._played_cards_this_turn = []
        self._card_play_starts_this_turn = []
        self._after_energy_reset_owners_this_turn = set()

    def start_combat(self) -> None:
        """Initialize combat and enter the first player turn."""
        from sts2_env.core.hooks import fire_before_combat_start

        self._combat_started = True
        self.current_side = CombatSide.PLAYER
        self.round_number = 1
        self.energy = 0
        self.stars = 0
        self.primary_player.stars = 0
        self._pending_retain_count = {}
        self._damage_events_combat = []
        self._draw_events_combat = []
        self._generated_cards_combat = []
        self._orb_channel_events_combat = []
        self._played_cards_combat = []
        self._card_play_finished_entries_combat = []
        self._card_play_round_counts = {}
        self.extra_card_rewards = 0
        self._reset_side_turn_history()

        for state in self.combat_player_states:
            state.creature.combat_state = self
            self._reset_player_combat_state(state)

        fire_before_combat_start(self)

        for enemy in self.alive_enemies:
            ai = self.enemy_ais[enemy.combat_id]
            ai.roll_move(self.monster_ai_rng)

        self._start_player_turn()

    # ---- Player Turn ----

    def _start_player_turn(self) -> None:
        """Start-of-player-side lifecycle."""
        from sts2_env.core.hooks import (
            fire_before_side_turn_start,
        )

        self.current_side = CombatSide.PLAYER
        self.turn_count += 1
        self._pending_retain_count = {}
        self._reset_side_turn_history()

        fire_before_side_turn_start(CombatSide.PLAYER, self)
        if self.pending_choice is not None:
            self._pending_turn_setup = self._continue_player_turn_setup
            return
        self._continue_player_turn_setup()

    def _continue_player_turn_setup(self, player_index: int = 0, stage: str = "block") -> None:
        from sts2_env.core.hooks import (
            fire_after_block_cleared,
            fire_after_energy_reset,
            fire_after_player_turn_start,
            fire_after_side_turn_start,
            fire_before_play_phase_start,
            fire_before_hand_draw,
            fire_before_hand_draw_late,
            modify_hand_draw,
            should_reset_energy,
        )

        states = self.combat_player_states
        while player_index < len(states):
            state = states[player_index]
            owner = state.creature
            with self.acting_player_view(owner):
                if stage == "block":
                    if self.round_number > 1:
                        owner.clear_block(self)
                    fire_after_block_cleared(owner, self)

                    if should_reset_energy(self):
                        self.energy = self.max_energy
                    else:
                        self.energy += self.max_energy

                    fire_after_energy_reset(self, owner)
                    if self.pending_choice is not None:
                        self._pending_turn_setup = lambda idx=player_index: self._continue_player_turn_setup(idx, "before_hand_draw")
                        return
                    stage = "before_hand_draw"

                if stage == "before_hand_draw":
                    fire_before_hand_draw(owner, self)
                    if self.pending_choice is not None:
                        self._pending_turn_setup = lambda idx=player_index: self._continue_player_turn_setup(idx, "card_before_hand_draw")
                        return
                    stage = "card_before_hand_draw"

                if stage == "card_before_hand_draw":
                    self._apply_card_before_hand_draw(owner)
                    if self.is_over:
                        return
                    if self.pending_choice is not None:
                        self._pending_turn_setup = lambda idx=player_index: self._continue_player_turn_setup(idx, "before_hand_draw_late")
                        return
                    stage = "before_hand_draw_late"

                if stage == "before_hand_draw_late":
                    fire_before_hand_draw_late(owner, self)
                    if self.is_over:
                        return
                    if self.pending_choice is not None:
                        self._pending_turn_setup = lambda idx=player_index: self._continue_player_turn_setup(idx, "draw")
                        return
                    stage = "draw"

                if stage == "draw":
                    draw_count = modify_hand_draw(BASE_DRAW, self, owner)
                    draw_count = self._prepare_opening_draw_for_owner(owner, draw_count)
                    self._draw_cards_for_creature(owner, draw_count, from_hand_draw=True)
                    if self.pending_choice is not None:
                        self._pending_turn_setup = lambda idx=player_index: self._continue_player_turn_setup(idx, "after_player_turn_start")
                        return
                    stage = "after_player_turn_start"

                if stage == "after_player_turn_start":
                    fire_after_player_turn_start(owner, self)
                    if self.is_over:
                        return
                    if self.pending_choice is not None:
                        self._pending_turn_setup = lambda idx=player_index: self._continue_player_turn_setup(idx, "post_after_player_turn_start")
                        return
                    stage = "post_after_player_turn_start"

                if stage == "post_after_player_turn_start":
                    apply_enchantment_after_player_turn_start(owner, self)

            player_index += 1
            stage = "block"

        fire_after_side_turn_start(CombatSide.PLAYER, self)
        if self.pending_choice is not None or self.is_over:
            return
        for state in self.combat_player_states:
            if state.orb_queue is not None:
                with self.acting_player_view(state.creature):
                    state.orb_queue.trigger_after_turn_start(self)
                self._check_combat_end()
                if self.pending_choice is not None or self.is_over:
                    return
        for state in self.combat_player_states:
            fire_before_play_phase_start(state.creature, self)
            if self.pending_choice is not None or self.is_over:
                return
        self._check_combat_end()

    def _prepare_opening_draw_for_owner(self, owner: Creature, draw_count: int) -> int:
        """Round-1 innate handling from `SetupPlayerTurn()`."""
        state = self.combat_player_state_for(owner)
        if state is None:
            return max(0, draw_count)
        if self.round_number != 1:
            return max(0, draw_count)

        innate_cards = [card for card in state.draw if card.is_innate]
        non_innate_cards = [card for card in state.draw if not card.is_innate]
        state.draw[:] = innate_cards + non_innate_cards

        adjusted = max(draw_count, len(innate_cards))
        return min(MAX_HAND_SIZE, max(0, adjusted))

    def _apply_card_before_hand_draw(self, owner: Creature) -> None:
        state = self.combat_player_state_for(owner)
        if state is None:
            return
        for card in list(state.draw) + list(state.discard) + list(state.exhaust) + list(state.play):
            if card.combat_vars.get("_return_before_hand_draw_round") == self.round_number:
                self.move_card_to_creature_hand(owner, card)
        for card in self.unique_cards_in_piles(state.all_piles):
            fire_card_before_hand_draw(card, owner, self)
            if self.is_over:
                return

    def _apply_card_after_turn_end(self, side: CombatSide) -> None:
        for state in self.combat_player_states:
            for card in self.unique_cards_in_piles(state.all_piles):
                fire_card_after_turn_end(card, side, self)
                if self.is_over:
                    return

    def unique_cards_in_piles(self, piles: tuple[list[CardInstance], ...]) -> list[CardInstance]:
        cards: list[CardInstance] = []
        seen_ids: set[int] = set()
        for pile in piles:
            for card in list(pile):
                instance_id = getattr(card, "instance_id", 0) or id(card)
                if instance_id in seen_ids:
                    continue
                seen_ids.add(instance_id)
                cards.append(card)
        return cards

    def _is_card_in_combat(self, card: CardInstance) -> bool:
        for state in self.combat_player_states:
            for pile in state.all_piles:
                if any(existing is card for existing in pile):
                    return True
        return False

    def _apply_card_after_card_entered_combat(self, card: CardInstance, owner: Creature) -> None:
        for listener in list(self.all_creatures):
            for power in list(listener.powers.values()):
                after_card_entered_combat = getattr(power, "after_card_entered_combat", None)
                if callable(after_card_entered_combat):
                    after_card_entered_combat(listener, card, self)
        for state in self.combat_player_states:
            for relic in list(state.relics):
                if getattr(relic, "enabled", True):
                    relic.after_card_entered_combat(state.creature, card, self)
        if card.combat_vars.get("_is_clone"):
            return
        if card.is_shiv and owner.get_power_amount(PowerId.PHANTOM_BLADES) > 0:
            card.keywords = frozenset(set(card.keywords) | {"retain"})
        state = self.combat_player_state_for(owner)
        if state is not None:
            for listener_card in self.unique_cards_in_piles(state.all_piles):
                fire_card_after_card_entered_combat(listener_card, card, owner, self)

    def _apply_card_after_card_generated_for_combat(
        self,
        card: CardInstance,
        owner: Creature,
        added_by_player: bool,
    ) -> None:
        from sts2_env.core.hooks import fire_after_card_generated_for_combat

        fire_after_card_generated_for_combat(card, added_by_player, self)

    def _add_generated_card_to_combat(
        self,
        card: CardInstance | None,
        owner: Creature,
        pile_name: str,
        *,
        added_by_player: bool = True,
        random_position: bool = False,
        bottom_position: bool = False,
    ) -> None:
        if card is None or (self._combat_started and self.is_over):
            return
        self._remove_card_from_piles(card)
        card.owner = owner
        self._generated_cards_combat.append((owner, added_by_player))
        zones = self._zones_for_creature(owner)
        if pile_name == "hand":
            if len(zones["hand"]) < MAX_HAND_SIZE:
                zones["hand"].append(card)
            else:
                zones["discard"].append(card)
        elif pile_name == "draw":
            if random_position:
                insert_at = self.shuffle_rng.next_int(0, len(zones["draw"]))
                zones["draw"].insert(insert_at, card)
            elif bottom_position:
                zones["draw"].append(card)
            else:
                zones["draw"].insert(0, card)
        elif pile_name == "discard":
            zones["discard"].append(card)
        else:
            raise ValueError(f"Unsupported generated card pile: {pile_name}")
        self._apply_card_after_card_entered_combat(card, owner)
        self._apply_card_after_card_generated_for_combat(card, owner, added_by_player)

    def add_generated_card_to_creature_hand(
        self,
        owner: Creature,
        card: CardInstance | None,
        *,
        added_by_player: bool = True,
    ) -> None:
        self._add_generated_card_to_combat(
            card,
            owner,
            "hand",
            added_by_player=added_by_player,
        )

    def add_generated_card_to_creature_draw_pile(
        self,
        owner: Creature,
        card: CardInstance | None,
        *,
        added_by_player: bool = True,
        random_position: bool = False,
        bottom_position: bool = False,
    ) -> None:
        self._add_generated_card_to_combat(
            card,
            owner,
            "draw",
            added_by_player=added_by_player,
            random_position=random_position,
            bottom_position=bottom_position,
        )

    def add_generated_card_to_creature_discard(
        self,
        owner: Creature,
        card: CardInstance | None,
        *,
        added_by_player: bool = True,
    ) -> None:
        self._add_generated_card_to_combat(
            card,
            owner,
            "discard",
            added_by_player=added_by_player,
        )

    def _apply_card_before_card_played(self, played_card: CardInstance, owner: Creature) -> None:
        state = self.combat_player_state_for(owner)
        if state is None:
            return
        for card in self.unique_cards_in_piles(state.all_piles):
            fire_card_before_card_played(card, played_card, owner, self)

    def _apply_card_after_card_played(self, played_card: CardInstance, owner: Creature) -> None:
        state = self.combat_player_state_for(owner)
        if state is None:
            return
        for card in self.unique_cards_in_piles(state.all_piles):
            fire_card_after_card_played(card, played_card, owner, self)

    def can_play_card(self, card: CardInstance) -> bool:
        """Check whether a card can be played right now."""
        from sts2_env.core.hooks import should_play

        owner = getattr(card, "owner", None) or self.player
        if getattr(card, "owner", None) is None:
            card.owner = owner
        owner_state = self.combat_player_state_for(owner) or self.current_player_state
        if self.is_over:
            return False
        if self.pending_choice is not None:
            return False
        if card.is_unplayable:
            return False
        if not should_play(card, self):
            return False

        if not card.is_playable_by_card_logic(owner_state, self, owner):
            return False
        if any(not hand_card.allows_hand_card_play(card, owner_state, self, owner) for hand_card in owner_state.hand):
            return False
        target_type = card.target_type_for(owner)
        if target_type == TargetType.ANY_ALLY and not self.get_player_allies_of(owner):
            return False
        if self.modified_star_cost(owner, card) > owner_state.stars:
            return False
        if not card.has_energy_cost_x and self.modified_card_cost(owner, card) > owner_state.energy:
            return False
        if card.card_type == CardType.STATUS:
            return not card.is_unplayable
        return True

    def can_auto_play_card(self, card: CardInstance) -> bool:
        """Check whether a card can be auto-played without spending resources."""
        from sts2_env.core.hooks import should_play

        owner = getattr(card, "owner", None) or self.player
        if getattr(card, "owner", None) is None:
            card.owner = owner
        owner_state = self.combat_player_state_for(owner) or self.current_player_state
        if self.is_over:
            return False
        if card.is_unplayable:
            return False
        if not should_play(card, self):
            return False
        return all(
            hand_card.allows_hand_card_play(
                card,
                owner_state,
                self,
                owner,
                is_auto_play=True,
            )
            for hand_card in owner_state.hand
        )

    def modified_card_cost(self, owner: Creature, card: CardInstance) -> int:
        if getattr(card, "owner", None) is None:
            card.owner = owner
        cost = max(0, card.cost)
        if card.has_energy_cost_x:
            return cost
        for power in owner.powers.values():
            modify_card_cost = getattr(power, "modify_card_cost", None)
            if not callable(modify_card_cost):
                continue
            modified = modify_card_cost(owner, card)
            if modified is not None:
                cost = max(0, int(modified))
        for relic in self.relics_for_creature(owner):
            modify_card_cost = getattr(relic, "modify_card_cost", None)
            if not callable(modify_card_cost):
                continue
            modified = modify_card_cost(owner, card, self)
            if modified is not None:
                cost = max(0, int(modified))
        return cost

    def modified_star_cost(self, owner: Creature, card: CardInstance) -> int:
        if getattr(card, "owner", None) is None:
            card.owner = owner
        cost = max(
            0,
            int(
                card.combat_vars.get(
                    "_turn_star_cost_override",
                    card.combat_vars.get("_combat_star_cost_override", card.star_cost),
                )
            ),
        )
        for power in owner.powers.values():
            modify_star_cost = getattr(power, "modify_star_cost", None)
            if not callable(modify_star_cost):
                continue
            modified = modify_star_cost(owner, card)
            if modified is not None:
                cost = max(0, int(modified))
        for relic in self.relics_for_creature(owner):
            modify_star_cost = getattr(relic, "modify_star_cost", None)
            if not callable(modify_star_cost):
                continue
            modified = modify_star_cost(owner, card, self)
            if modified is not None:
                cost = max(0, int(modified))
        return cost

    def _default_card_play_result(
        self,
        owner: Creature,
        card: CardInstance,
        *,
        force_exhaust: bool,
    ) -> CardPlayResult:
        if card.combat_vars.get("_is_dupe") or card.card_type == CardType.POWER:
            return CardPlayResult(PileType.NONE, CardPilePosition.NONE)
        if force_exhaust or card.exhausts:
            return CardPlayResult(PileType.EXHAUST, CardPilePosition.BOTTOM)
        return CardPlayResult(PileType.DISCARD, CardPilePosition.BOTTOM)

    def _modified_card_play_result(
        self,
        owner: Creature,
        card: CardInstance,
        *,
        force_exhaust: bool,
        is_auto_play: bool,
        energy_value: int,
    ) -> CardPlayResult:
        from sts2_env.core.hooks import modify_card_play_result_pile_type_and_position

        result = self._default_card_play_result(owner, card, force_exhaust=force_exhaust)
        pile_type, position = modify_card_play_result_pile_type_and_position(
            card,
            self,
            is_auto_play,
            energy_value,
            result.pile_type,
            result.position,
        )
        return CardPlayResult(pile_type, position)

    def _move_card_to_play_result(
        self,
        owner: Creature,
        card: CardInstance,
        result: CardPlayResult,
    ) -> None:
        owner_state = self.combat_player_state_for(owner) or self.current_player_state
        if result.pile_type == PileType.NONE:
            return
        if result.pile_type == PileType.EXHAUST:
            owner_state.exhaust.append(card)
            from sts2_env.core.hooks import fire_after_card_exhausted

            fire_after_card_exhausted(card, self)
            return
        if result.pile_type == PileType.HAND:
            target_pile = owner_state.hand
        elif result.pile_type == PileType.DRAW:
            target_pile = owner_state.draw
        elif result.pile_type == PileType.DISCARD:
            target_pile = owner_state.discard
        else:
            return
        if result.position == CardPilePosition.TOP:
            target_pile.insert(0, card)
        elif result.position == CardPilePosition.RANDOM:
            target_pile.insert(self.shuffle_rng.next_int(0, len(target_pile)), card)
        else:
            target_pile.append(card)

    def play_card(self, hand_index: int, target_index: int | None = None) -> bool:
        return self.play_card_from_creature(self.primary_player, hand_index, target_index)

    def play_card_from_creature(
        self,
        owner: Creature,
        hand_index: int,
        target_index: int | None = None,
    ) -> bool:
        """Play a card from hand. Returns True if successful."""
        from sts2_env.core.hooks import fire_after_hand_emptied

        owner_state = self.combat_player_state_for(owner)
        if owner_state is None:
            return False
        if self.pending_choice is not None:
            return False
        if hand_index < 0 or hand_index >= len(owner_state.hand):
            return False

        card = owner_state.hand[hand_index]
        card.owner = owner
        if not self.can_play_card(card):
            return False

        target_type = card.target_type_for(owner)
        target = self._resolve_target(card, target_index)
        if target_type == TargetType.ANY_ENEMY and target is None:
            return False

        owner_state.hand.pop(hand_index)
        previous_in_play_phase = self.in_play_phase
        self.in_play_phase = True
        try:
            self._execute_card_play(card, target, spend_energy=True)

            if not owner_state.hand:
                fire_after_hand_emptied(self)

            self._check_combat_end()
            return True
        finally:
            self.in_play_phase = previous_in_play_phase

    def _execute_card_play(
        self,
        card: CardInstance,
        target: Creature | None,
        *,
        spend_energy: bool,
        force_exhaust: bool = False,
        is_auto_play: bool | None = None,
    ) -> None:
        """Shared play pipeline for normal play and auto-play sources."""
        from sts2_env.core.hooks import (
            fire_after_energy_spent,
            fire_after_modifying_card_play_count,
            modify_card_play_count,
        )

        self.flush_pending_attack_context()
        owner = getattr(card, "owner", None) or self.player
        owner_state = self.combat_player_state_for(owner) or self.current_player_state
        energy_spent = 0
        previous_cost_card = self._card_being_played_for_cost
        self._card_being_played_for_cost = card if spend_energy else None
        try:
            if spend_energy:
                energy_spent = owner_state.energy if card.has_energy_cost_x else self.modified_card_cost(owner, card)
            card.owner = owner
            card.energy_spent = energy_spent
            card.combat_vars["_stars_spent_for_play"] = 0

            if spend_energy:
                owner_state.energy = max(0, owner_state.energy - energy_spent)
                if energy_spent > 0:
                    self._energy_spent_this_turn[owner] = self._energy_spent_this_turn.get(owner, 0) + energy_spent
                fire_after_energy_spent(owner, card, energy_spent, self)
                star_cost = self.modified_star_cost(owner, card)
                if star_cost > 0:
                    card.combat_vars["_stars_spent_for_play"] = self.spend_stars(owner, star_cost)
        finally:
            self._card_being_played_for_cost = previous_cost_card
        if card not in owner_state.play:
            owner_state.play.append(card)

        previous_card_target = self._active_card_target
        self._active_card_target = target
        try:
            play_count = 1 + getattr(card, "base_replay_count", 0) + enchant_play_count_bonus(card)
            play_count = modify_card_play_count(play_count, card, self)
            fire_after_modifying_card_play_count(card, self)
        finally:
            self._active_card_target = previous_card_target
        energy_value = energy_spent
        if not spend_energy:
            energy_value = owner_state.energy if card.has_energy_cost_x else self.modified_card_cost(owner, card)

        self._pending_play = {
            "card": card,
            "target": target,
            "owner": owner,
            "remaining_plays": play_count,
            "play_count": play_count,
            "energy_spent": energy_spent,
            "energy_value": energy_value,
            "force_exhaust": force_exhaust,
            "is_auto_play": not spend_energy if is_auto_play is None else is_auto_play,
            "awaiting_after_hook": False,
        }
        self._resume_pending_play()

    def _resume_pending_play(self) -> None:
        from sts2_env.core.hooks import fire_after_card_played, fire_before_card_played

        while self._pending_play is not None:
            ctx = self._pending_play
            card = ctx["card"]
            target = ctx["target"]
            owner = ctx["owner"]
            owner_state = self.combat_player_state_for(owner) or self.current_player_state

            if ctx["awaiting_after_hook"]:
                if self.pending_choice is not None:
                    return
                self._finish_card_play(card, owner)
                with self.acting_player_view(owner):
                    fire_after_card_played(card, self)
                    self._apply_card_after_card_played(card, owner)
                    apply_enchantment_on_card_played(card, self)
                    apply_enchantment_after_card_played(card)
                    self._fire_after_card_played_late(card)
                ctx["awaiting_after_hook"] = False
                if self.is_over:
                    self._pending_play = None
                    return

            if ctx["remaining_plays"] <= 0:
                if card in owner_state.play:
                    owner_state.play.remove(card)

                if any(card in pile for pile in owner_state.all_piles):
                    self._pending_play = None
                    return

                result = self._modified_card_play_result(
                    owner,
                    card,
                    force_exhaust=ctx["force_exhaust"],
                    is_auto_play=ctx["is_auto_play"],
                    energy_value=ctx["energy_value"],
                )
                self._move_card_to_play_result(owner, card, result)
                self._pending_play = None
                if self._end_turn_after_play and self.current_side == CombatSide.PLAYER and not self.is_over:
                    self._end_turn_after_play = False
                    self.end_player_turn()
                return

            with self.acting_player_view(owner):
                self._apply_card_before_card_played(card, owner)
                fire_before_card_played(card, self)
            is_first_in_series = ctx["remaining_plays"] == ctx["play_count"]
            self._card_play_starts_this_turn.append(
                CardPlayStartedEntry(card, is_first_in_series, ctx["energy_value"])
            )
            ctx["remaining_plays"] -= 1
            previous_card_source = self._active_card_source
            self._active_card_source = card
            try:
                with self.acting_player_view(owner):
                    play_card_effect(card, self, target)
            finally:
                self._active_card_source = previous_card_source
            self.flush_pending_attack_context()
            if self.pending_choice is not None:
                ctx["awaiting_after_hook"] = True
                return
            self._finish_card_play(card, owner)
            with self.acting_player_view(owner):
                fire_after_card_played(card, self)
                self._apply_card_after_card_played(card, owner)
                apply_enchantment_on_card_played(card, self)
                apply_enchantment_after_card_played(card)
                self._fire_after_card_played_late(card)
            if self.is_over:
                self._pending_play = None
                return

    def _finish_card_play(self, card: CardInstance, owner: Creature) -> None:
        self._played_cards_this_turn.append(card)
        self._played_cards_combat.append(card)
        self._card_play_finished_entries_combat.append(
            CardPlayFinishedEntry(
                card,
                card.is_ethereal,
                self.round_number,
            )
        )
        self._record_finished_card_play(owner)

    def _record_finished_card_play(self, owner: Creature) -> None:
        key = (self.round_number, owner)
        self._card_play_round_counts[key] = self._card_play_round_counts.get(key, 0) + 1

    def end_player_turn(self) -> None:
        """End player turn, execute enemy turn, then start the next player turn."""
        from sts2_env.core.hooks import fire_after_turn_end, fire_before_turn_end

        if self.is_over:
            return

        fire_before_turn_end(CombatSide.PLAYER, self)
        self._check_combat_end()
        if self.is_over:
            return

        for state in self.combat_player_states:
            if state.orb_queue is not None:
                with self.acting_player_view(state.creature):
                    state.orb_queue.trigger_before_turn_end(self)
                self._check_combat_end()
                if self.is_over:
                    return

        self._resolve_end_of_turn_hand()
        self._check_combat_end()
        if self.is_over:
            return

        self._cleanup_cards_end_of_turn()
        fire_after_turn_end(CombatSide.PLAYER, self)
        self._apply_card_after_turn_end(CombatSide.PLAYER)
        self._check_combat_end()
        if self.is_over:
            return

        from sts2_env.core.hooks import fire_after_taking_extra_turn, should_take_extra_turn
        if should_take_extra_turn(self):
            self.round_number += 1
            fire_after_taking_extra_turn(self)
            self._start_player_turn()
            return

        self._execute_enemy_turn()
        if self.is_over or self.pending_choice is not None:
            return

        self.round_number += 1
        self._start_player_turn()

    def _resolve_end_of_turn_hand(self) -> None:
        from sts2_env.core.hooks import (
            fire_after_card_discarded,
            fire_after_card_exhausted,
            fire_before_flush,
            fire_before_flush_late,
            should_flush,
        )

        for state in self.combat_player_states:
            owner = state.creature
            with self.acting_player_view(owner):
                cards_in_hand_at_turn_end = len(self.hand)
                turn_end_cards = [card for card in list(self.hand) if self._has_turn_end_in_hand_effect(card)]
                turn_end_ids = {id(card) for card in turn_end_cards}

                ethereal_cards = [
                    card for card in list(self.hand)
                    if id(card) not in turn_end_ids and card.is_ethereal
                ]
                for card in ethereal_cards:
                    self.hand.remove(card)
                    self.exhaust_pile.append(card)
                    card.combat_vars["_exhausted_by_ethereal"] = True
                    try:
                        fire_after_card_exhausted(card, self)
                    finally:
                        card.combat_vars.pop("_exhausted_by_ethereal", None)

                for card in turn_end_cards:
                    if card not in self.hand:
                        continue
                    self.hand.remove(card)
                    self.play_pile.append(card)
                    self._execute_turn_end_in_hand_effect(card, cards_in_hand_at_turn_end)
                    if card in self.play_pile:
                        self.play_pile.remove(card)
                    if card.is_ethereal:
                        self.exhaust_pile.append(card)
                        card.combat_vars["_exhausted_by_ethereal"] = True
                        try:
                            fire_after_card_exhausted(card, self)
                        finally:
                            card.combat_vars.pop("_exhausted_by_ethereal", None)
                    else:
                        self.discard_pile.append(card)
                        fire_after_card_discarded(card, self)

                retained: list[CardInstance] = []
                retained_ids: set[int] = set()

                for card in self.hand:
                    if card.should_retain_this_turn:
                        retained.append(card)
                        retained_ids.add(id(card))

                fire_before_flush(owner, self)
                apply_enchantment_before_flush(owner, self)
                fire_before_flush_late(owner, self)

                retain_budget = self._pending_retain_count.get(state.player_state.player_id, 0)
                if retain_budget > 0:
                    retained_by_budget = 0
                    for card in self.hand:
                        if id(card) in retained_ids:
                            continue
                        retained.append(card)
                        retained_ids.add(id(card))
                        retained_by_budget += 1
                        if retained_by_budget >= retain_budget:
                            break

                remaining = [card for card in self.hand if id(card) not in retained_ids]
                flush_hand = should_flush(self, owner)

                if flush_hand:
                    for card in remaining:
                        self.discard_pile.append(card)
                        fire_after_card_discarded(card, self)
                    self.hand = list(retained)
                else:
                    self.hand = list(retained) + remaining

                self._pending_retain_count[state.player_state.player_id] = 0

    # ---- Enemy Turn ----

    def _execute_enemy_turn(self) -> None:
        """Execute the enemy-side lifecycle and all enemy moves."""
        from sts2_env.core.hooks import (
            fire_after_block_cleared,
            fire_after_side_turn_start,
            fire_after_turn_end,
            fire_before_side_turn_start,
            fire_before_turn_end,
        )

        self.current_side = CombatSide.ENEMY
        self._reset_side_turn_history()

        fire_before_side_turn_start(CombatSide.ENEMY, self)

        for enemy in list(self.alive_enemies):
            enemy.clear_block(self)
            fire_after_block_cleared(enemy, self)

        fire_after_side_turn_start(CombatSide.ENEMY, self)
        self._check_combat_end()
        if self.is_over:
            return

        self._continue_enemy_moves(0)

    def _continue_enemy_moves(self, start_index: int, *, resume_player_turn: bool = False) -> None:
        enemies = list(self.enemies_with_turns)
        for index in range(start_index, len(enemies)):
            enemy = enemies[index]
            ai = self.enemy_ais[enemy.combat_id]
            move = ai.current_move
            move.perform(self)
            if self.pending_choice is not None:
                self._pending_turn_setup = lambda enemy=enemy, index=index: self._finish_enemy_move_after_choice(enemy, index)
                return
            ai.on_move_performed()

            self._check_combat_end()
            if self.is_over:
                return

        self._finish_enemy_turn(resume_player_turn=resume_player_turn)

    def _finish_enemy_move_after_choice(self, enemy: Creature, index: int) -> None:
        if enemy.combat_id in self.enemy_ais:
            self.enemy_ais[enemy.combat_id].on_move_performed()
        self._check_combat_end()
        if self.is_over:
            return
        self._continue_enemy_moves(index + 1, resume_player_turn=True)

    def _finish_enemy_turn(self, *, resume_player_turn: bool = False) -> None:
        from sts2_env.core.hooks import (
            fire_after_turn_end,
            fire_before_turn_end,
        )

        for enemy in list(self.enemies_with_turns):
            ai = self.enemy_ais[enemy.combat_id]
            ai.roll_move(self.monster_ai_rng)

        fire_before_turn_end(CombatSide.ENEMY, self)
        self._check_combat_end()
        if self.is_over:
            return

        self._cleanup_cards_end_of_turn()
        fire_after_turn_end(CombatSide.ENEMY, self)
        self._apply_card_after_turn_end(CombatSide.ENEMY)
        self._check_combat_end()
        if self.is_over or self.pending_choice is not None or not resume_player_turn:
            return

        self.round_number += 1
        self._start_player_turn()

    # ---- Card pile operations ----

    def _draw_cards(self, count: int, from_hand_draw: bool = False) -> list[CardInstance]:
        return self._draw_cards_for_creature(self.player, count, from_hand_draw=from_hand_draw)

    def _draw_cards_for_creature(
        self,
        owner: Creature,
        count: int,
        *,
        from_hand_draw: bool = False,
    ) -> list[CardInstance]:
        """Draw cards one at a time, reshuffling if needed."""
        from sts2_env.core.hooks import fire_after_card_drawn, should_draw

        if self.is_over or self.pending_choice is not None:
            return []
        state = self.combat_player_state_for(owner)
        if state is None:
            return []
        if not should_draw(self, owner, from_hand_draw):
            return []

        drawn_cards: list[CardInstance] = []
        remaining = max(0, count)
        while remaining > 0:
            if len(state.hand) >= MAX_HAND_SIZE:
                break
            self._shuffle_if_needed(owner)
            if self.pending_choice is not None:
                self._pending_draw = {
                    "owner": owner,
                    "remaining": remaining,
                    "from_hand_draw": from_hand_draw,
                }
                return drawn_cards
            if not state.draw:
                break
            remaining -= 1
            card = state.draw.pop(0)
            setattr(card, "owner", owner)
            state.hand.append(card)
            drawn_cards.append(card)
            self._draw_events_this_turn.append((owner, card, from_hand_draw))
            self._draw_events_combat.append(owner)
            self._apply_card_after_card_drawn_early(card, owner)
            fire_after_card_drawn(card, from_hand_draw, self)
            apply_enchantment_on_card_drawn(card, self, from_hand_draw)
        return drawn_cards

    def _resume_pending_draw(self) -> None:
        if self._pending_draw is None:
            return
        pending = self._pending_draw
        self._pending_draw = None
        self._draw_cards_for_creature(
            pending["owner"],
            pending["remaining"],
            from_hand_draw=pending["from_hand_draw"],
        )

    def _shuffle_if_needed(self, owner: Creature | None = None) -> None:
        """If draw pile is empty and discard has cards, shuffle discard into draw."""
        state = self.combat_player_state_for(owner or self.player) or self._primary_player_state
        if not state.draw and state.discard:
            from sts2_env.core.hooks import fire_after_shuffle

            state.draw[:] = list(state.discard)
            state.discard.clear()
            self.shuffle_rng.shuffle(state.draw)
            apply_enchantment_shuffle_order(state.draw, is_initial_shuffle=False)
            fire_after_shuffle(self, state.creature)

    def add_card_to_discard(
        self,
        card: CardInstance,
        *,
        owner: Creature | None = None,
        added_by_player: bool = False,
    ) -> None:
        """Add a generated card to discard pile."""
        self.add_generated_card_to_creature_discard(
            owner or self.player,
            card,
            added_by_player=added_by_player,
        )

    def held_potions(self, owner: Creature | None = None) -> list[PotionInstance]:
        state = self.combat_player_state_for(owner or self.player) or self._primary_player_state
        return [p for p in state.potions if p is not None]

    def add_potion(self, potion: PotionInstance, owner: Creature | None = None) -> bool:
        from sts2_env.core.hooks import fire_after_potion_procured

        state = self.combat_player_state_for(owner or self.player) or self._primary_player_state
        for i in range(state.max_potion_slots):
            if i >= len(state.potions):
                state.potions.append(None)
            if state.potions[i] is None:
                potion.slot_index = i
                potion.owner = state.creature
                state.potions[i] = potion
                fire_after_potion_procured(potion, self)
                return True
        return False

    def _can_procure_potion(self, owner: Creature) -> bool:
        for relic in self.relics_for_creature(owner):
            should_procure = getattr(relic, "should_procure_potion", None)
            if callable(should_procure) and should_procure(owner) is False:
                return False
        return True

    def procure_random_potion(
        self,
        owner: Creature | None = None,
        *,
        in_combat: bool = True,
    ) -> PotionInstance | None:
        from sts2_env.potions.base import create_potion, roll_random_potion_model

        state = self.combat_player_state_for(owner or self.player) or self._primary_player_state
        potion_model = roll_random_potion_model(
            self.combat_potion_generation_rng,
            character_id=state.character_id,
            in_combat=in_combat,
        )
        if potion_model is None:
            return None
        potion = create_potion(potion_model.potion_id)
        if not self._can_procure_potion(state.creature):
            return None
        if self.add_potion(potion, owner=state.creature):
            return potion
        return None

    def procure_potion(
        self,
        owner: Creature | None,
        potion_id: str,
    ) -> bool:
        from sts2_env.potions.base import create_potion

        state = self.combat_player_state_for(owner or self.player) or self._primary_player_state
        if potion_id == "random":
            return self.procure_random_potion(state.creature, in_combat=False) is not None
        potion = create_potion(potion_id)
        if not self._can_procure_potion(state.creature):
            return False
        return self.add_potion(potion, owner=state.creature)

    def fill_empty_potion_slots(
        self,
        owner: Creature | None = None,
        *,
        in_combat: bool = True,
    ) -> int:
        state = self.combat_player_state_for(owner or self.player) or self._primary_player_state
        filled = 0
        while len(self.held_potions(state.creature)) < state.max_potion_slots:
            if self.procure_random_potion(state.creature, in_combat=in_combat) is None:
                break
            filled += 1
        return filled

    def _resolve_potion_target(
        self,
        potion: PotionInstance,
        user: Creature,
        target_index: int | None,
    ) -> Creature | None:
        if potion.target_type == PotionTargetType.SELF:
            return user
        if potion.target_type == PotionTargetType.ANY_PLAYER:
            if target_index is None or target_index < 0:
                return user
            players = [user] + self.get_player_allies_of(user)
            if target_index < len(players):
                return players[target_index]
            return None
        if potion.target_type == PotionTargetType.ALL_ENEMIES:
            return None
        if potion.target_type != PotionTargetType.ANY_ENEMY:
            return None
        if target_index is not None and 0 <= target_index < len(self.enemies):
            enemy = self.enemies[target_index]
            if self.can_hit_creature(enemy):
                return enemy
            return None
        hittable = self.hittable_enemies
        return hittable[0] if hittable else None

    def can_use_potion(
        self,
        slot: int,
        *,
        target_index: int | None = None,
        owner: Creature | None = None,
    ) -> bool:
        state = self.combat_player_state_for(owner or self.player) or self._primary_player_state
        if self.is_over or self.pending_choice is not None or self.current_side != CombatSide.PLAYER:
            return False
        if slot < 0 or slot >= len(state.potions):
            return False
        potion = state.potions[slot]
        if potion is None or not potion.can_use_in_combat():
            return False
        target = self._resolve_potion_target(potion, state.creature, target_index)
        if potion.target_type == PotionTargetType.ANY_PLAYER:
            return target is not None
        if potion.target_type == PotionTargetType.ANY_ENEMY:
            return target is not None
        return True

    def use_potion(
        self,
        slot: int,
        *,
        target_index: int | None = None,
        owner: Creature | None = None,
    ) -> bool:
        from sts2_env.core.hooks import fire_after_potion_used, fire_before_potion_used

        state = self.combat_player_state_for(owner or self.player) or self._primary_player_state
        if not self.can_use_potion(slot, target_index=target_index, owner=state.creature):
            return False
        potion = state.potions[slot]
        assert potion is not None
        target = self._resolve_potion_target(potion, state.creature, target_index)
        fire_before_potion_used(potion, target, self)
        potion.use(self, state.creature, target)
        state.potions[slot] = None
        potion.slot_index = -1
        fire_after_potion_used(potion, target, self)
        self._check_combat_end()
        return True

    def add_card_to_draw_pile(self, owner: Creature, card_name: str) -> None:
        card = self._make_named_card(card_name)
        if card is not None:
            self._zones_for_creature(owner)["draw"].insert(0, card)

    def exhaust_top_of_draw_pile(self, owner: Creature) -> None:
        self.exhaust_from_draw_pile(owner, 1)

    def exhaust_from_draw_pile(self, owner: Creature, count: int) -> None:
        from sts2_env.core.hooks import fire_after_card_exhausted

        zones = self._zones_for_creature(owner)
        for _ in range(max(0, count)):
            self._shuffle_if_needed(owner)
            if not zones["draw"]:
                break
            card = zones["draw"].pop(0)
            zones["exhaust"].append(card)
            fire_after_card_exhausted(card, self)

    # ---- Target resolution ----

    def _resolve_target(self, card: CardInstance, target_index: int | None) -> Creature | None:
        owner = getattr(card, "owner", None) or self.player
        target_type = card.target_type_for(owner)
        if target_type in (TargetType.SELF, TargetType.NONE):
            return owner
        if target_type == TargetType.ALL_ENEMIES:
            return None
        if target_type == TargetType.ALL_ALLIES:
            return None
        if target_type == TargetType.RANDOM_ENEMY:
            alive = self.hittable_enemies
            return self.combat_targets_rng.choice(alive) if alive else None
        if target_type == TargetType.ANY_ENEMY:
            if target_index is not None and 0 <= target_index < len(self.enemies):
                enemy = self.enemies[target_index]
                if self.can_hit_creature(enemy):
                    return enemy
            alive = self.hittable_enemies
            return alive[0] if alive else None
        if target_type == TargetType.ANY_ALLY:
            allies = self.get_player_allies_of(owner)
            if target_index is not None and 0 <= target_index < len(allies):
                return allies[target_index]
            return allies[0] if allies else None
        return None

    # ---- Power / relic helpers ----

    def apply_power_to(
        self,
        target: Creature,
        power_id: PowerId,
        amount: int,
        *,
        applier: Creature | None = None,
        source: object | None = None,
        ignore_next_instance: bool = False,
    ) -> None:
        """Apply a power to a creature. Player-side debuffs skip first tick."""
        if self.is_over:
            return
        if target.combat_state is not self or target.escaped:
            return
        if not all(
            getattr(power, "should_allow_hitting", lambda owner, combat: True)(target, self)
            for power in target.powers.values()
        ):
            return
        if source is None:
            source = self.active_card_source
        if applier is None:
            applier = getattr(source, "owner", None)
            if applier is None and self.current_side == CombatSide.PLAYER:
                applier = self.primary_player
        target.apply_power(
            power_id,
            amount,
            applier=applier,
            source=source,
            ignore_next_instance=ignore_next_instance,
        )

        if target.side == CombatSide.PLAYER:
            power = target.powers.get(power_id)
            if power is not None and power.power_type == PowerType.DEBUFF:
                power.skip_next_tick = True

    def after_power_amount_changed(
        self,
        target: Creature,
        power_id: PowerId,
        amount: int,
        *,
        applier: Creature | None = None,
        source: object | None = None,
    ) -> None:
        for creature in self.all_creatures:
            for power in list(creature.powers.values()):
                power.after_power_amount_changed(
                    creature,
                    target,
                    power_id,
                    amount,
                    applier,
                    source,
                    self,
                )
        self._power_events_this_turn.append((target, power_id, amount, applier))

    def request_retain(self, owner: Creature, count: int) -> None:
        if self.combat_player_state_for(owner) is not None and count > 0:
            player_id = self.combat_player_state_for(owner).player_state.player_id
            self._pending_retain_count[player_id] = self._pending_retain_count.get(player_id, 0) + count

    def request_discard(self, owner: Creature, count: int) -> None:
        from sts2_env.core.hooks import fire_after_card_discarded

        state = self.combat_player_state_for(owner)
        if state is None or count <= 0:
            return
        discard_count = min(count, len(state.hand))
        if discard_count <= 0:
            return

        def _discard_selected(selected_cards: list[CardInstance]) -> None:
            for card in selected_cards:
                if card in state.hand:
                    state.hand.remove(card)
                    state.discard.append(card)
                    fire_after_card_discarded(card, self)

        self.request_multi_card_choice(
            prompt="Choose hand cards to discard",
            cards=list(state.hand),
            source_pile="hand",
            resolver=_discard_selected,
            min_count=discard_count,
            max_count=discard_count,
            owner=owner,
        )

    def request_exhaust(self, owner: Creature, count: int) -> None:
        state = self.combat_player_state_for(owner)
        if state is None or count <= 0:
            return
        exhaust_count = min(count, len(state.hand))
        if exhaust_count <= 0:
            return

        def _exhaust_selected(selected_cards: list[CardInstance]) -> None:
            for card in selected_cards:
                if card in state.hand:
                    self.exhaust_card(card)

        self.request_multi_card_choice(
            prompt="Choose hand cards to exhaust",
            cards=list(state.hand),
            source_pile="hand",
            resolver=_exhaust_selected,
            min_count=exhaust_count,
            max_count=exhaust_count,
            owner=owner,
        )

    def reduce_retained_card_cost(self, owner: Creature) -> None:
        state = self.combat_player_state_for(owner)
        if state is None:
            return
        candidates = [
            card for card in state.hand
            if card.should_retain_this_turn and not card.has_energy_cost_x and card.cost > 0
        ]
        if not candidates:
            return
        card = self.combat_card_selection_rng.choice(candidates)
        card.cost -= 1

    def gain_energy(self, owner: Creature, amount: int) -> None:
        state = self.combat_player_state_for(owner)
        if not self.is_over and state is not None and amount > 0:
            state.energy += amount

    def lose_energy(self, owner: Creature, amount: int) -> None:
        state = self.combat_player_state_for(owner)
        if not self.is_over and state is not None and amount > 0:
            state.energy = max(0, state.energy - amount)

    def gain_gold(self, owner: Creature, amount: int) -> int:
        state = self.combat_player_state_for(owner)
        if state is None or amount <= 0:
            return 0
        for relic in self.relics_for_creature(owner):
            if relic.should_gain_gold(owner, amount) is False:
                return 0
        state.player_state.gold += int(amount)
        for relic in self.relics_for_creature(owner):
            on_gold_gained = getattr(relic, "on_gold_gained", None)
            if callable(on_gold_gained):
                on_gold_gained(owner, amount)
        return amount

    def lose_gold(self, owner: Creature, amount: int) -> int:
        state = self.combat_player_state_for(owner)
        if state is None or amount <= 0:
            return 0
        return state.player_state.lose_gold(amount)

    def transform_relic(self, owner: Creature, current_relic: object, new_relic_id: object) -> bool:
        state = self.combat_player_state_for(owner)
        if state is None:
            return False
        current_name = getattr(getattr(current_relic, "relic_id", None), "name", str(current_relic))
        new_name = getattr(new_relic_id, "name", str(new_relic_id))
        try:
            index = state.player_state.relics.index(current_name)
        except ValueError:
            return False
        from sts2_env.relics.registry import create_relic_by_name

        state.player_state.relics[index] = new_name
        if index < len(state.player_state.relic_objects):
            state.player_state.relic_objects[index] = create_relic_by_name(new_name)
        if index < len(state.relics):
            state.relics[index] = create_relic_by_name(new_name)
        return True

    def gain_max_hp(self, owner: Creature, amount: int) -> None:
        state = self.combat_player_state_for(owner)
        if state is None or amount <= 0:
            return
        owner.gain_max_hp(amount)
        state.player_state.gain_max_hp(amount)

    def lose_max_hp(self, owner: Creature, amount: int) -> None:
        state = self.combat_player_state_for(owner)
        if state is None or amount <= 0:
            return
        owner.lose_max_hp(amount)
        state.player_state.lose_max_hp(amount)

    def gain_stars(self, owner: Creature, amount: int) -> None:
        state = self.combat_player_state_for(owner)
        if not self.is_over and state is not None and amount > 0:
            state.stars += amount
            owner.gain_stars(amount)
            self._stars_gained_this_turn.append((owner, amount))
            for power in owner.powers.values():
                power.on_stars_gained(owner, amount, self)
            for relic in self.relics_for_creature(owner):
                relic.on_stars_gained(owner, amount, self)

    def spend_stars(self, owner: Creature, amount: int) -> int:
        state = self.combat_player_state_for(owner)
        if state is None or amount <= 0:
            return 0
        spent = owner.lose_stars(amount)
        if spent <= 0:
            return 0
        state.stars = max(0, state.stars - spent)
        for power in owner.powers.values():
            power.on_stars_spent(owner, spent, self)
        for relic in self.relics_for_creature(owner):
            relic.on_stars_spent(owner, spent, self)
        return spent

    def draw_cards(self, owner: Creature, count: int) -> list[CardInstance]:
        if self.combat_player_state_for(owner) is not None and count > 0:
            return self._draw_cards_for_creature(owner, count, from_hand_draw=False)
        return []

    def deal_damage(
        self,
        dealer: Creature | None = None,
        target: Creature | None = None,
        amount: int = 0,
        props: ValueProp = ValueProp.MOVE,
        targets: list[Creature] | None = None,
    ) -> list:
        """Convenience method for powers/relics to deal damage."""
        from sts2_env.core.damage import apply_damage, calculate_damage

        results = []
        target_list = targets if targets is not None else ([target] if target is not None else [])
        if dealer is None:
            for creature in target_list:
                if creature is None or creature.is_dead:
                    continue
                results.append(apply_damage(creature, amount, props, self, dealer))
            self._check_combat_end()
            return results
        if dealer.is_dead:
            return results

        with self.attack_context(dealer, target, props):
            for creature in target_list:
                if creature is None or creature.is_dead:
                    continue
                damage = calculate_damage(amount, dealer, creature, props, self)
                results.append(apply_damage(creature, damage, props, self, dealer))
        self._check_combat_end()
        return results

    def record_damage_event(
        self,
        dealer: Creature | None,
        target: Creature,
        props: ValueProp,
        unblocked_damage: int = 0,
    ) -> None:
        self._damage_events_this_turn.append((dealer, target, props))
        self._damage_events_combat.append((dealer, target, props, unblocked_damage))

    def count_powered_hits_this_turn(self, dealer: Creature, target: Creature) -> int:
        return sum(
            1
            for logged_dealer, logged_target, props in self._damage_events_this_turn
            if logged_dealer is dealer and logged_target is target and props.is_powered_attack()
        )

    def count_powered_hits_by_dealer_this_turn(self, dealer: Creature) -> int:
        return sum(
            1
            for logged_dealer, _, props in self._damage_events_this_turn
            if logged_dealer is dealer and props.is_powered_attack()
        )

    def count_allied_powered_hits_on_target_this_turn(self, owner: Creature, target: Creature) -> int:
        return sum(
            1
            for logged_dealer, logged_target, props in self._damage_events_this_turn
            if logged_target is target
            and logged_dealer is not None
            and logged_dealer is not owner
            and logged_dealer.side == owner.side
            and props.is_powered_attack()
        )

    def count_unblocked_hits_received_this_combat(self, target: Creature) -> int:
        return sum(
            1
            for _, logged_target, _, unblocked in self._damage_events_combat
            if logged_target is target and unblocked > 0
        )

    def record_block_gained_event(
        self,
        target: Creature,
        props: ValueProp,
        card_play: object | None,
    ) -> None:
        self._block_events_this_turn.append((target, props, card_play))

    def count_block_gained_events_this_turn(
        self,
        target: Creature,
        *,
        props_mask: ValueProp | None = None,
        exclude_card_play: object | None = _NO_CARD_PLAY_FILTER,
    ) -> int:
        return sum(
            1
            for logged_target, props, card_play in self._block_events_this_turn
            if logged_target is target
            and (props_mask is None or bool(props & props_mask))
            and (exclude_card_play is _NO_CARD_PLAY_FILTER or card_play is not exclude_card_play)
        )

    def count_card_or_monster_move_block_gained_events_this_turn(
        self,
        target: Creature,
        *,
        exclude_card_play: object | None = _NO_CARD_PLAY_FILTER,
    ) -> int:
        return sum(
            1
            for logged_target, props, card_play in self._block_events_this_turn
            if logged_target is target
            and props.is_card_or_monster_move()
            and (exclude_card_play is _NO_CARD_PLAY_FILTER or card_play is not exclude_card_play)
        )

    def count_non_hand_draws_this_turn(self, owner: Creature) -> int:
        return sum(
            1
            for logged_owner, _, from_hand_draw in self._draw_events_this_turn
            if logged_owner is owner and not from_hand_draw
        )

    def count_drawn_cards_this_turn(self, owner: Creature, card_type: CardType | None = None) -> int:
        return sum(
            1
            for logged_owner, card, _ in self._draw_events_this_turn
            if logged_owner is owner and (card_type is None or card.card_type == card_type)
        )

    def energy_spent_this_turn(self, owner: Creature) -> int:
        return self._energy_spent_this_turn.get(owner, 0)

    def has_energy_reset_this_turn(self, owner: Creature) -> bool:
        return owner in self._after_energy_reset_owners_this_turn

    def mark_energy_reset_this_turn(self, owner: Creature) -> None:
        self._after_energy_reset_owners_this_turn.add(owner)

    def count_cards_drawn_this_combat(self, owner: Creature) -> int:
        return sum(1 for logged_owner in self._draw_events_combat if logged_owner is owner)

    def record_card_exhausted(self, card: CardInstance) -> None:
        self._exhaust_events_this_turn.append(card)

    def was_card_exhausted_this_turn(self, owner: Creature) -> bool:
        return any(getattr(card, "owner", None) is owner for card in self._exhaust_events_this_turn)

    def record_card_discarded(self, card: CardInstance) -> None:
        self._discard_events_this_turn.append(card)

    def count_cards_discarded_this_turn(self, owner: Creature) -> int:
        return sum(1 for card in self._discard_events_this_turn if getattr(card, "owner", None) is owner)

    def count_generated_cards_this_combat(self, owner: Creature) -> int:
        return sum(
            1
            for logged_owner, added_by_player in self._generated_cards_combat
            if logged_owner is owner and added_by_player
        )

    def count_stars_gained_this_turn(self, owner: Creature) -> int:
        return sum(amount for logged_owner, amount in self._stars_gained_this_turn if logged_owner is owner)

    def was_power_applied_this_turn(
        self,
        power_id: PowerId,
        *,
        applier: Creature | None = None,
        target: Creature | None = None,
    ) -> bool:
        return any(
            logged_power_id == power_id
            and (applier is None or logged_applier is applier)
            and (target is None or logged_target is target)
            for logged_target, logged_power_id, _, logged_applier in self._power_events_this_turn
        )

    def should_owner_death_trigger_fatal(self, target: Creature) -> bool:
        return all(
            power.should_owner_death_trigger_fatal(target, self)
            for power in target.powers.values()
        )

    def get_alive_enemies(self) -> list[Creature]:
        return self.alive_enemies

    def get_enemies_of(self, owner: Creature) -> list[Creature]:
        if owner.side == CombatSide.PLAYER or getattr(owner, "is_pet", False):
            return self.alive_enemies
        targets: list[Creature] = [self.primary_player] if self.primary_player.is_alive else []
        targets.extend(self.alive_allies)
        return targets

    def get_teammates_of(self, owner: Creature) -> list[Creature]:
        if owner is self.primary_player:
            return list(self.alive_allies)
        if owner in self.allies:
            return ([self.primary_player] if self.primary_player.is_alive else []) + [
                ally for ally in self.alive_allies if ally is not owner
            ]
        return [enemy for enemy in self.alive_enemies if enemy is not owner]

    def random_enemy_of(self, owner: Creature) -> Creature | None:
        enemies = [enemy for enemy in self.get_enemies_of(owner) if self.can_hit_creature(enemy)]
        return self.combat_targets_rng.choice(enemies) if enemies else None

    def get_player_allies_of(self, owner: Creature) -> list[Creature]:
        """Living allied player-creatures excluding the owner itself."""
        if owner.side != CombatSide.PLAYER or not getattr(owner, "is_player", False):
            return []
        return [
            ally for ally in self.alive_allies
            if getattr(ally, "is_player", False) and ally is not owner
        ] + ([self.primary_player] if self.primary_player.is_alive and self.primary_player is not owner else [])

    def request_card_choice(
        self,
        *,
        prompt: str,
        cards: Sequence[CardInstance],
        source_pile: str,
        resolver,
        allow_skip: bool = False,
        owner: Creature | None = None,
    ) -> None:
        """Pause combat resolution until a card selection is made."""
        if self.is_over:
            return
        owner = owner or self.player

        if not allow_skip and len(cards) <= 1:
            with self.acting_player_view(owner):
                resolver(cards[0] if cards else None)
            return

        def _single_resolver(selected: list[CardInstance]) -> None:
            with self.acting_player_view(owner):
                resolver(selected[0] if selected else None)

        self.pending_choice = PendingCardChoice(
            prompt=prompt,
            options=[CardChoiceOption(card=card, source_pile=source_pile) for card in cards],
            resolver=_single_resolver,
            allow_skip=allow_skip,
        )

    def request_multi_card_choice(
        self,
        *,
        prompt: str,
        cards: Sequence[CardInstance],
        source_pile: str,
        resolver,
        min_count: int,
        max_count: int | None = None,
        allow_skip: bool = False,
        owner: Creature | None = None,
    ) -> None:
        """Pause combat resolution until multiple cards are selected and confirmed."""
        if self.is_over:
            return
        if max_count is None:
            max_count = min_count
        owner = owner or self.player

        if not allow_skip and min_count == max_count and len(cards) <= min_count:
            with self.acting_player_view(owner):
                resolver(list(cards))
            return

        def _wrapped_resolver(selected_cards: list[CardInstance]) -> None:
            with self.acting_player_view(owner):
                resolver(selected_cards)

        self.pending_choice = PendingCardChoice(
            prompt=prompt,
            options=[CardChoiceOption(card=card, source_pile=source_pile) for card in cards],
            resolver=_wrapped_resolver,
            allow_skip=allow_skip,
            min_choices=min_count,
            max_choices=max_count,
        )

    def resolve_pending_choice(self, choice_index: int | None) -> bool:
        """Resolve the current pending combat choice."""
        if self.pending_choice is None:
            return False

        choice = self.pending_choice
        if choice.is_multi:
            if choice_index is None:
                if not choice.can_confirm():
                    return False
                selected_cards = choice.selected_cards
                self.pending_choice = None
                choice.resolver(selected_cards)
            else:
                return choice.toggle(choice_index)
        else:
            selected_cards: list[CardInstance] = []
            if choice_index is None:
                if not choice.allow_skip:
                    return False
            else:
                if choice_index < 0 or choice_index >= len(choice.options):
                    return False
                selected_cards = [choice.options[choice_index].card]
            self.pending_choice = None
            choice.resolver(selected_cards)

        if self.pending_choice is None and self._pending_draw is not None:
            self._resume_pending_draw()
        if self.pending_choice is None and self._pending_play is not None:
            self._resume_pending_play()
        if self.pending_choice is None and self._pending_turn_setup is not None:
            pending_setup = self._pending_turn_setup
            self._pending_turn_setup = None
            pending_setup()
        self._check_combat_end()
        return True

    def move_card_to_hand(self, card: CardInstance | None) -> None:
        self.move_card_to_creature_hand(self.player, card)

    def move_card_to_creature_hand(self, creature: Creature, card: CardInstance | None) -> None:
        if card is None or (self._combat_started and self.is_over):
            return
        was_in_combat = self._is_card_in_combat(card)
        self._remove_card_from_piles(card)
        card.owner = creature
        zones = self._zones_for_creature(creature)
        if len(zones["hand"]) < MAX_HAND_SIZE:
            zones["hand"].append(card)
        else:
            zones["discard"].append(card)
        if not was_in_combat:
            self._apply_card_after_card_entered_combat(card, creature)

    def search_draw_pile_to_hand(self, owner: Creature, count: int) -> None:
        state = self.combat_player_state_for(owner)
        if state is None or count <= 0 or not state.draw:
            return
        candidates = sorted(state.draw, key=lambda card: (card.rarity.value, card.card_id.name))
        required = min(count, len(candidates))

        def _move_selected(selected_cards: list[CardInstance]) -> None:
            for selected in selected_cards:
                self.move_card_to_creature_hand(owner, selected)

        self.request_multi_card_choice(
            prompt="Choose card(s) to move to hand",
            cards=candidates,
            source_pile="draw",
            resolver=_move_selected,
            min_count=required,
            max_count=required,
            owner=owner,
        )

    def stable_shuffle_cards(self, cards: list[CardInstance], rng: Rng) -> None:
        cards.sort(key=lambda card: (card.card_id.name, card.upgraded))
        rng.shuffle(cards)

    def move_card_to_discard(self, card: CardInstance | None) -> None:
        self.move_card_to_creature_discard(self.player, card)

    def move_card_to_creature_discard(self, creature: Creature, card: CardInstance | None) -> None:
        if card is None:
            return
        was_in_combat = self._is_card_in_combat(card)
        self._remove_card_from_piles(card)
        card.owner = creature
        zones = self._zones_for_creature(creature)
        zones["discard"].append(card)
        if not was_in_combat:
            self._apply_card_after_card_entered_combat(card, creature)

    def discard_cards(self, cards: Sequence[CardInstance], draw_count: int = 0) -> None:
        """Discard cards, then draw, then auto-play any Sly cards."""
        from sts2_env.core.hooks import fire_after_card_discarded

        sly_cards: list[CardInstance] = []
        for card in list(cards):
            if card in self.hand:
                if card.is_sly:
                    sly_cards.append(card)
                self.move_card_to_discard(card)
                fire_after_card_discarded(card, self)
        if draw_count > 0:
            self._draw_cards(draw_count)
        for sly_card in sly_cards:
            self.auto_play_card(sly_card)

    def exhaust_card(self, card: CardInstance | None) -> None:
        from sts2_env.core.hooks import fire_after_card_exhausted

        if card is None or (self._combat_started and self.is_over):
            return
        was_in_combat = self._is_card_in_combat(card)
        self._remove_card_from_piles(card)
        owner = getattr(card, "owner", None) or self.player
        card.owner = owner
        self._zones_for_creature(owner)["exhaust"].append(card)
        if not was_in_combat:
            self._apply_card_after_card_entered_combat(card, owner)
        fire_after_card_exhausted(card, self)

    def clone_card_to_hand(self, owner: Creature, card: CardInstance | None) -> None:
        if self.combat_player_state_for(owner) is None or card is None:
            return
        clone = card.clone(new_card_instance_id())
        self.add_generated_card_to_creature_hand(owner, clone)

    def insert_card_into_draw_pile(self, card: CardInstance | None, *, random_position: bool = False) -> None:
        self.insert_card_into_creature_draw_pile(self.player, card, random_position=random_position)

    def insert_card_into_creature_draw_pile(
        self,
        creature: Creature,
        card: CardInstance | None,
        *,
        random_position: bool = False,
    ) -> None:
        if card is None or (self._combat_started and self.is_over):
            return
        was_in_combat = self._is_card_in_combat(card)
        self._remove_card_from_piles(card)
        card.owner = creature
        zones = self._zones_for_creature(creature)
        if random_position:
            insert_at = self.shuffle_rng.next_int(0, len(zones["draw"]))
            zones["draw"].insert(insert_at, card)
        else:
            zones["draw"].insert(0, card)
        if not was_in_combat:
            self._apply_card_after_card_entered_combat(card, creature)

    def _remove_card_from_piles(self, card: CardInstance) -> None:
        for state in self.combat_player_states:
            for pile in state.all_piles:
                if card in pile:
                    pile.remove(card)
                    return

    def return_stolen_card(self, card: CardInstance, target: Creature | None = None) -> None:
        state = self.combat_player_state_for(target) if target is not None else None
        if state is None:
            owner = getattr(card, "owner", None)
            state = self.combat_player_state_for(owner) if owner is not None else None
        if state is None:
            return
        if not any(existing is card for existing in state.player_state.deck):
            state.player_state.add_card_instance_to_deck(card)
        room = self.room
        if room is not None and hasattr(room, "add_extra_reward"):
            from sts2_env.run.reward_objects import RecoveredCardReward

            room.add_extra_reward(
                state.player_state.player_id,
                RecoveredCardReward(state.player_state.player_id, card, encounter_source="THIEVING_HOPPER"),
            )

    def _zones_for_creature(self, creature: Creature) -> dict[str, list[CardInstance]]:
        state = self.combat_player_state_for(creature)
        if state is not None:
            return state.zone_map
        return {"hand": [], "draw": [], "discard": [], "exhaust": []}

    def upgrade_card(self, card: CardInstance | None) -> CardInstance | None:
        if card is None or card.upgraded:
            return card
        if self._combat_started and self.is_over and self._is_card_in_combat(card):
            return card
        progress = capture_self_mutating_card_progress(card)
        try:
            upgraded = create_card(card.card_id, upgraded=True)
        except KeyError:
            return card
        if not upgraded.upgraded:
            return card

        current_cost = card.cost
        had_turn_override = "_turn_cost_override" in card.combat_vars
        card.card_type = upgraded.card_type
        card.target_type = upgraded.target_type
        card.rarity = upgraded.rarity
        card.base_damage = upgraded.base_damage
        card.base_block = upgraded.base_block
        card.upgraded = upgraded.upgraded
        card.keywords = upgraded.keywords
        card.tags = upgraded.tags
        card.can_be_generated_in_combat = upgraded.can_be_generated_in_combat
        card.can_be_generated_by_modifiers = upgraded.can_be_generated_by_modifiers
        card.has_turn_end_in_hand_effect = upgraded.has_turn_end_in_hand_effect
        card.enchantments = dict(card.enchantments)
        card.effect_vars = dict(upgraded.effect_vars)
        card.has_energy_cost_x = upgraded.has_energy_cost_x
        card.star_cost = upgraded.star_cost
        card.original_cost = upgraded.original_cost
        if had_turn_override:
            card.cost = current_cost
        else:
            card.cost = upgraded.cost
        restore_self_mutating_card_progress(card, progress)
        return card

    def transform_card(self, old_card: CardInstance | None, new_card: CardInstance | None) -> CardInstance | None:
        if old_card is None or new_card is None or (self._combat_started and self.is_over):
            return None

        target_pile = None
        target_index = None
        for state in self.combat_player_states:
            for pile in state.all_piles:
                if old_card in pile:
                    target_pile = pile
                    target_index = pile.index(old_card)
                    break
            if target_pile is not None:
                break
        if target_pile is None or target_index is None:
            return None

        owner = getattr(old_card, "owner", None) or self.primary_player
        new_card.owner = owner
        target_pile[target_index] = new_card
        self._generated_cards_combat.append((owner, True))
        self._apply_card_after_card_entered_combat(new_card, owner)
        self._apply_card_after_card_generated_for_combat(new_card, owner, True)
        return new_card

    def transform_cards_from_hand(self, owner: Creature, count: int) -> None:
        state = self.combat_player_state_for(owner)
        if state is None or count <= 0:
            return
        candidates = [
            card for card in state.hand
            if eligible_transform_cards(
                card,
                character_id=state.character_id,
                generation_context="combat",
                is_multiplayer=self.is_multiplayer,
            )
        ]
        required = min(count, len(candidates))
        if required <= 0:
            return

        def _transform_selected(selected_cards: list[CardInstance]) -> None:
            for selected in selected_cards:
                replacement = create_transform_card(
                    selected,
                    character_id=state.character_id,
                    rng=self.combat_card_selection_rng,
                    generation_context="combat",
                    is_multiplayer=self.is_multiplayer,
                )
                self.transform_card(selected, replacement)

        self.request_multi_card_choice(
            prompt=f"Choose {required} card(s) to transform",
            cards=candidates,
            source_pile="hand",
            resolver=_transform_selected,
            min_count=required,
            max_count=required,
            owner=owner,
        )

    def upgrade_random_cards(self, pile: list[CardInstance], count: int) -> list[CardInstance]:
        """Upgrade up to `count` upgradable cards chosen uniformly from a pile."""
        candidates = [card for card in pile if not card.upgraded]
        if not candidates or count <= 0:
            return []
        self.combat_card_selection_rng.shuffle(candidates)
        chosen = candidates[:count]
        for card in chosen:
            self.upgrade_card(card)
        return chosen

    def channel_orb(self, owner: Creature, orb_type: str) -> None:
        from sts2_env.core.enums import OrbType

        if self._combat_started and self.is_over:
            return
        state = self.combat_player_state_for(owner)
        orb_queue = getattr(state, "orb_queue", None)
        if orb_queue is not None:
            if isinstance(orb_type, str):
                orb_type = OrbType[orb_type.upper()]
            if orb_queue.capacity <= 0:
                return
            with self.acting_player_view(owner):
                orb_queue.channel(orb_type, self)
                self._orb_channel_events_combat.append((owner, orb_type))
                for relic in self.relics_for_creature(owner):
                    on_orb_channeled = getattr(relic, "on_orb_channeled", None)
                    if callable(on_orb_channeled):
                        on_orb_channeled(owner, self)

    def channel_random_orb(self, owner: Creature) -> None:
        from sts2_env.core.enums import OrbType

        self.channel_orb(owner, self.combat_orbs_rng.choice(list(OrbType)))

    def add_orb_slots(self, owner: Creature, amount: int) -> None:
        state = self.combat_player_state_for(owner)
        orb_queue = getattr(state, "orb_queue", None)
        if orb_queue is not None and amount > 0:
            orb_queue.capacity = min(orb_queue.capacity + amount, orb_queue.MAX_CAPACITY)

    def count_distinct_orb_types(self, owner: Creature) -> int:
        state = self.combat_player_state_for(owner)
        orb_queue = getattr(state, "orb_queue", None)
        if orb_queue is None:
            return 0
        return len({orb.orb_type for orb in getattr(orb_queue, "orbs", [])})

    def count_orbs(self, owner: Creature, orb_type: str | object) -> int:
        from sts2_env.core.enums import OrbType

        state = self.combat_player_state_for(owner)
        orb_queue = getattr(state, "orb_queue", None)
        if orb_queue is None:
            return 0
        if isinstance(orb_type, str):
            orb_type = OrbType[orb_type.upper()]
        return sum(1 for orb in getattr(orb_queue, "orbs", []) if orb.orb_type == orb_type)

    def trigger_first_orb_passive(self, owner: Creature) -> None:
        state = self.combat_player_state_for(owner)
        orb_queue = getattr(state, "orb_queue", None)
        if orb_queue is not None:
            with self.acting_player_view(owner):
                orb_queue.trigger_first_passive(self)

    def evoke_last_orb(self, owner: Creature) -> None:
        state = self.combat_player_state_for(owner)
        orb_queue = getattr(state, "orb_queue", None)
        if orb_queue is not None:
            with self.acting_player_view(owner):
                orb_queue.evoke_last(self)

    def get_osty(self, owner: Creature | None = None) -> Creature | None:
        target_owner = owner or self.primary_player
        if target_owner is self.primary_player:
            return self.osty
        return next(
            (
                ally
                for ally in self.allies
                if getattr(ally, "is_osty", False) and getattr(ally, "pet_owner", None) is target_owner
            ),
            None,
        )

    def summon_event_pet(self, owner: Creature, monster_id: str | RelicId) -> Creature | None:
        from sts2_env.core.hooks import fire_after_creature_added_to_combat
        from sts2_env.monsters.shared import create_byrdpip, create_paels_legion

        if isinstance(monster_id, RelicId):
            pet_id = monster_id.name
        else:
            pet_id = monster_id
        state = self.combat_player_state_for(owner)
        if state is None or owner.side != CombatSide.PLAYER or not getattr(owner, "is_player", False):
            return None
        existing = next(
            (
                ally
                for ally in self.allies
                if getattr(ally, "is_pet", False)
                and getattr(ally, "pet_owner", None) is owner
                and getattr(ally, "monster_id", None) == pet_id
            ),
            None,
        )
        if existing is not None:
            return existing

        factories = {
            RelicId.BYRDPIP.name: create_byrdpip,
            RelicId.PAELS_LEGION.name: create_paels_legion,
        }
        factory = factories.get(pet_id)
        if factory is None:
            return None
        pet, _ = factory(self.rng)
        pet.side = CombatSide.PLAYER
        pet.is_pet = True
        pet.pet_owner = owner
        pet.owner = owner
        pet.combat_state = self
        self.allies.append(pet)
        fire_after_creature_added_to_combat(pet, self)
        return pet

    def summon_osty(self, owner: Creature, amount: int, source: object | None = None) -> Creature | None:
        from sts2_env.core.hooks import (
            fire_after_creature_added_to_combat,
            fire_after_osty_revived,
            fire_after_summon,
            modify_summon_amount,
        )

        state = self.combat_player_state_for(owner)
        if state is None or owner.side != CombatSide.PLAYER or not getattr(owner, "is_player", False):
            return None
        amount = modify_summon_amount(owner, amount, source, self)
        osty = self.get_osty(owner)
        if amount <= 0:
            return osty
        is_new = osty is None
        is_reviving = osty is not None and not osty.is_alive

        if osty is None:
            osty = Creature(
                max_hp=1,
                current_hp=1,
                side=CombatSide.PLAYER,
                is_player=False,
                monster_id="OSTY",
            )
            osty.is_pet = True
            osty.is_osty = True
            osty.pet_owner = owner
            osty.owner = owner
            osty.combat_state = self
            if owner is self.primary_player:
                self.osty = osty
            self.allies.append(osty)
            fire_after_creature_added_to_combat(osty, self)
            osty.apply_power(PowerId.DIE_FOR_YOU, 1)
        elif owner is self.primary_player:
            self.osty = osty

        if osty.is_alive and not is_new:
            osty.gain_max_hp(amount)
            fire_after_summon(owner, amount, self)
            return osty

        osty.max_hp = amount
        osty.current_hp = amount
        osty.block = 0
        osty.escaped = False
        osty._death_processed = False
        osty.pet_owner = owner
        osty.owner = owner
        if not osty.has_power(PowerId.DIE_FOR_YOU):
            osty.apply_power(PowerId.DIE_FOR_YOU, 1)

        if is_reviving:
            fire_after_osty_revived(osty, self)
        fire_after_summon(owner, amount, self)
        return osty

    def kill_osty(self, owner: Creature) -> bool:
        """Kill the player's Osty pet if it is alive."""
        osty = self.get_osty(owner)
        if osty is None or not osty.is_alive:
            return False
        self.kill_creature(osty)
        return True

    def kill_creature(self, creature: Creature | None) -> bool:
        """Immediately kill a creature."""
        if creature is None or creature.escaped:
            return False
        if creature.is_dead and getattr(creature, "_death_processed", False):
            return False
        current_hp = creature.current_hp
        creature.current_hp = 0
        creature.block = 0
        if current_hp > 0:
            from sts2_env.core.hooks import fire_after_current_hp_changed

            fire_after_current_hp_changed(creature, -current_hp, self)
        for listener in list(self.all_creatures):
            for power in list(listener.powers.values()):
                before_death = getattr(power, "before_death", None)
                if callable(before_death):
                    before_death(listener, creature, self)
        was_removal_prevented = self._prevent_death_if_needed(creature)
        if was_removal_prevented:
            for listener in list(self.all_creatures):
                for power in list(listener.powers.values()):
                    after_death = getattr(power, "after_death", None)
                    if callable(after_death):
                        after_death(listener, creature, self, was_removal_prevented)
                    on_ally_death = getattr(power, "on_ally_death", None)
                    if callable(on_ally_death):
                        on_ally_death(listener, creature, self, was_removal_prevented)
            for state in self.combat_player_states:
                for relic in list(state.relics):
                    after_death = getattr(relic, "after_death", None)
                    if callable(after_death):
                        after_death(state.creature, creature, self)
            self._fire_card_after_death(creature, was_removal_prevented)
            creature._death_processed = False
            self._check_combat_end()
            return True
        creature._death_processed = True
        should_remove_power: dict[PowerId, bool] = {}
        should_remove_creature = True
        original_powers = list(creature.powers.values())
        for power in original_powers:
            should_remove = getattr(
                power,
                "should_power_be_removed_after_owner_death",
                lambda owner, combat: True,
            )(creature, self)
            override_remove: bool | None = None
            for other_power in list(creature.powers.values()):
                decide_remove = getattr(other_power, "should_other_power_be_removed_on_owner_death", None)
                if not callable(decide_remove):
                    continue
                result = decide_remove(creature, power, self)
                if result is False:
                    override_remove = False
                    break
                if result is True:
                    override_remove = True
            if override_remove is not None:
                should_remove = override_remove
            should_remove_power[power.power_id] = should_remove
            should_remove_creature = should_remove_creature and getattr(
                power,
                "should_creature_be_removed_from_combat_after_death",
                lambda owner, combat: True,
            )(creature, self)
        was_removal_prevented = False
        for listener in list(self.all_creatures):
            for power in list(listener.powers.values()):
                after_death = getattr(power, "after_death", None)
                if callable(after_death):
                    after_death(listener, creature, self, was_removal_prevented)
                on_ally_death = getattr(power, "on_ally_death", None)
                if callable(on_ally_death):
                    on_ally_death(listener, creature, self, was_removal_prevented)
        for power_id, remove_power in should_remove_power.items():
            if remove_power and power_id in creature.powers:
                self._remove_power(creature, power_id)
        if should_remove_creature:
            creature.escaped = True
        for state in self.combat_player_states:
            for relic in list(state.relics):
                after_death = getattr(relic, "after_death", None)
                if callable(after_death):
                    after_death(state.creature, creature, self)
        self._fire_card_after_death(creature, was_removal_prevented)
        self._sync_monster_death_move_responses(creature)
        self._check_combat_end()
        return True

    def _fire_card_after_death(self, creature: Creature, was_removal_prevented: bool) -> None:
        from sts2_env.cards.registry import fire_card_after_death

        for state in self.combat_player_states:
            for card in self.unique_cards_in_piles(state.all_piles):
                fire_card_after_death(card, creature, was_removal_prevented, self)

    def _sync_monster_death_move_responses(self, creature: Creature) -> None:
        from sts2_env.monsters.act3 import QUEEN_BURN_BRIGHT_FOR_ME_MOVE, QUEEN_ENRAGE_MOVE, QUEEN_MONSTER_ID
        from sts2_env.monsters.shared import TORCH_HEAD_AMALGAM_MONSTER_ID

        if creature.monster_id != TORCH_HEAD_AMALGAM_MONSTER_ID:
            return
        queen = next(
            (
                enemy for enemy in self.enemies
                if enemy.monster_id == QUEEN_MONSTER_ID
                and enemy.side == creature.side
                and enemy.is_alive
            ),
            None,
        )
        if queen is None:
            return
        queen_ai = self.enemy_ais.get(queen.combat_id)
        if queen_ai is None or queen_ai.current_move.state_id != QUEEN_BURN_BRIGHT_FOR_ME_MOVE:
            return
        self.set_enemy_state(queen, QUEEN_ENRAGE_MOVE)

    def _remove_power(self, owner: Creature, power_id: PowerId) -> None:
        power = owner.powers.pop(power_id, None)
        if power is None:
            return
        on_removed = getattr(power, "on_removed", None)
        if callable(on_removed):
            on_removed(owner, self)

    def _prevent_death_if_needed(self, creature: Creature) -> bool:
        state = self.combat_player_state_for(creature)
        if state is None:
            return False
        for index, potion in enumerate(list(state.potions)):
            if potion is None or potion.potion_id != "FairyInABottle":
                continue
            from sts2_env.core.hooks import fire_after_potion_used

            potion.use(self, creature, creature)
            state.potions[index] = None
            potion.slot_index = -1
            fire_after_potion_used(potion, creature, self)
            return True
        for relic in list(state.relics):
            should_die_late = getattr(relic, "should_die_late", None)
            if callable(should_die_late) and should_die_late(creature, self) is False:
                return True
        return False

    def escape_creature(self, creature: Creature | None) -> bool:
        """Remove a creature from combat without killing it."""
        if creature is None or not creature.is_alive:
            return False
        creature.escaped = True
        creature.block = 0
        self._check_combat_end()
        return True

    def kill_doomed_enemies(self) -> int:
        """Immediately kill enemies whose HP is within their Doom threshold."""
        doomed = [
            enemy for enemy in list(self.alive_enemies)
            if enemy.get_power_amount(PowerId.DOOM) > 0
            and enemy.current_hp <= enemy.get_power_amount(PowerId.DOOM)
        ]
        return self.kill_doomed_creatures(doomed)

    def kill_doomed_creatures(self, creatures: list[Creature]) -> int:
        killed = 0
        doomed_kills: list[Creature] = []
        for creature in creatures:
            if self.kill_creature(creature):
                killed += 1
                doomed_kills.append(creature)
        if doomed_kills:
            for listener_state in self.combat_player_states:
                owner = listener_state.creature
                for relic in listener_state.relics:
                    after_died_to_doom = getattr(relic, "after_died_to_doom", None)
                    if callable(after_died_to_doom):
                        after_died_to_doom(owner, list(doomed_kills), self)
        return killed

    def _is_player_side_player(self, creature: Creature) -> bool:
        return creature.side == CombatSide.PLAYER and getattr(creature, "is_player", False)

    def is_owner_side_turn(self, owner: Creature) -> bool:
        return self.current_side == owner.side

    def _all_cards_for_creature(
        self,
        creature: Creature,
        *,
        include_exhausted: bool = True,
    ) -> list[CardInstance]:
        state = self.combat_player_state_for(creature)
        if state is None:
            return []
        cards = list(state.hand) + list(state.draw) + list(state.discard) + list(state.play)
        if include_exhausted:
            cards.extend(state.exhaust)
        return cards

    def count_card_plays_finished_this_turn(
        self,
        owner: Creature,
        *,
        card_type: CardType | None = None,
    ) -> int:
        return sum(
            1
            for card in self._played_cards_this_turn
            if getattr(card, "owner", None) is owner
            and (card_type is None or card.card_type == card_type)
        )

    def count_cards_played_this_turn(
        self,
        owner: Creature,
        *,
        card_type: CardType | None = None,
    ) -> int:
        return self.count_card_plays_finished_this_turn(owner, card_type=card_type)

    def has_card_play_finished_this_turn(self, card: CardInstance) -> bool:
        return any(played is card for played in self._played_cards_this_turn)

    def has_card_with_tag_finished_this_turn(self, owner: Creature, tag: CardTag) -> bool:
        return any(
            getattr(card, "owner", None) is owner and tag in getattr(card, "tags", ())
            for card in self._played_cards_this_turn
        )

    def count_card_play_starts_this_turn(
        self,
        owner: Creature,
        *,
        card_type: CardType | None = None,
        first_in_series_only: bool = False,
        exclude_card: object | None = _NO_CARD_FILTER,
        energy_value: int | None = None,
    ) -> int:
        return sum(
            1
            for entry in self._card_play_starts_this_turn
            if getattr(entry.card, "owner", None) is owner
            and (card_type is None or entry.card.card_type == card_type)
            and (not first_in_series_only or entry.is_first_in_series)
            and (exclude_card is _NO_CARD_FILTER or entry.card is not exclude_card)
            and (energy_value is None or entry.energy_value == energy_value)
        )

    def last_card_play_started_this_turn(
        self,
        owner: Creature,
        *,
        exclude_card: object | None = _NO_CARD_FILTER,
    ) -> CardInstance | None:
        for entry in reversed(self._card_play_starts_this_turn):
            card = entry.card
            if getattr(card, "owner", None) is not owner:
                continue
            if exclude_card is not _NO_CARD_FILTER and card is exclude_card:
                continue
            return card
        return None

    def count_cards_played_this_combat(
        self,
        owner: Creature,
        *,
        card_type: CardType | None = None,
    ) -> int:
        return sum(
            1
            for card in self._played_cards_combat
            if getattr(card, "owner", None) is owner
            and (card_type is None or card.card_type == card_type)
        )

    def count_ethereal_cards_played_this_combat(self, owner: Creature) -> int:
        return sum(
            1
            for entry in self._card_play_finished_entries_combat
            if entry.was_ethereal and getattr(entry.card, "owner", None) is owner
        )

    def has_unblocked_damage_received_this_turn(
        self,
        target: Creature,
        *,
        side: CombatSide | None = None,
    ) -> bool:
        if side is not None and self.current_side != side:
            return False
        event_count = len(self._damage_events_this_turn)
        current_turn_events = self._damage_events_combat[-event_count:] if event_count else ()
        return any(
            logged_target is target and unblocked > 0
            for _, logged_target, _, unblocked in current_turn_events
        )

    def last_finished_attack_or_skill_from_previous_round(self, owner: Creature) -> CardInstance | None:
        for entry in reversed(self._card_play_finished_entries_combat):
            card = entry.card
            if entry.round_number != self.round_number - 1:
                continue
            if getattr(card, "owner", None) is not owner:
                continue
            if card.card_type not in {CardType.ATTACK, CardType.SKILL}:
                continue
            if bool(card.combat_vars.get("_is_dupe")):
                continue
            return card
        return None

    def count_cards_played_last_round(self, owner: Creature) -> int:
        return self._card_play_round_counts.get((self.round_number - 1, owner), 0)

    def _sovereign_blades_for_creature(
        self,
        creature: Creature,
        *,
        include_exhausted: bool,
    ) -> list[CardInstance]:
        from sts2_env.cards.status import is_forgeable_sovereign_blade

        return [
            card
            for card in self._all_cards_for_creature(creature, include_exhausted=include_exhausted)
            if is_forgeable_sovereign_blade(card)
        ]

    def forge(self, owner: Creature, amount: int, source: object | None = None) -> None:
        """Regent forge mechanic mirroring ForgeCmd: ensure blade, buff all, then fire hooks."""
        from sts2_env.cards.status import add_sovereign_blade_damage, make_sovereign_blade
        from sts2_env.core.hooks import fire_after_forge

        if self.is_over or amount <= 0 or not self._is_player_side_player(owner) or not owner.is_alive:
            return

        active_blades = self._sovereign_blades_for_creature(owner, include_exhausted=False)
        if not active_blades:
            blade = make_sovereign_blade()
            blade.owner = owner
            blade.combat_vars["created_through_forge"] = 1
            self.add_generated_card_to_creature_hand(owner, blade)

        all_blades = self._sovereign_blades_for_creature(owner, include_exhausted=True)
        for blade in all_blades:
            add_sovereign_blade_damage(blade, amount)
        fire_after_forge(self, amount, owner, source)

    def stun_enemy(
        self,
        creature: Creature,
        next_state_id: str | None = None,
        stun_effect: Callable[[CombatState], None] | None = None,
    ) -> bool:
        """Replace an enemy's next move with a one-turn stun."""
        if creature is None or creature.is_player or creature.is_dead:
            return False
        ai = self.enemy_ais.get(creature.combat_id)
        if ai is None:
            return False

        from sts2_env.core.enums import IntentType
        from sts2_env.monsters.intents import Intent
        from sts2_env.monsters.state_machine import MoveState

        next_state_id = next_state_id or ai.current_move.state_id

        def _stunned(combat: CombatState) -> None:
            if stun_effect is not None:
                stun_effect(combat)
            return

        stunned = MoveState(_STUNNED_MOVE_ID, _stunned, [Intent(IntentType.STUN)], follow_up_id=next_state_id, must_perform_once=True)
        ai.states[_STUNNED_MOVE_ID] = stunned
        ai._current_state_id = _STUNNED_MOVE_ID  # noqa: SLF001
        ai._performed_first_move = True  # noqa: SLF001
        return True

    def set_enemy_state(self, creature: Creature, state_id: str) -> bool:
        if creature is None or creature.is_player:
            return False
        ai = self.enemy_ais.get(creature.combat_id)
        if ai is None or state_id not in ai.states:
            return False
        ai._current_state_id = state_id  # noqa: SLF001
        return True

    def spawn_surprise_replacements(self, owner: Creature) -> list[Creature]:
        from sts2_env.monsters.act4 import create_fat_gremlin, create_sneaky_gremlin

        spawned: list[Creature] = []
        sneaky, sneaky_ai = create_sneaky_gremlin(Rng(self.rng.next_int(0, INT_MAX)))
        self.add_enemy(sneaky, sneaky_ai)
        spawned.append(sneaky)

        fat, fat_ai = create_fat_gremlin(Rng(self.rng.next_int(0, INT_MAX)))
        thievery = owner.powers.get(PowerId.THIEVERY)
        gold_stolen = getattr(thievery, "gold_stolen", 0)
        if gold_stolen > 0:
            fat.apply_power(PowerId.HEIST, gold_stolen)
            heist = fat.powers.get(PowerId.HEIST)
            add_stolen_gold = getattr(heist, "add_stolen_gold", None)
            if callable(add_stolen_gold):
                for player, amount in getattr(thievery, "gold_stolen_by_player", {}).items():
                    add_stolen_gold(player, amount)
        self.add_enemy(fat, fat_ai)
        spawned.append(fat)
        return spawned

    def spawn_infested_wrigglers(self, count: int = 4) -> list[Creature]:
        from sts2_env.monsters.act2 import WRIGGLER_SLOT_NUMBER_BASE, WRIGGLER_SLOT_PREFIX, create_wriggler

        spawned: list[Creature] = []
        for index in range(max(0, count)):
            wriggler, wriggler_ai = create_wriggler(
                Rng(self.rng.next_int(0, INT_MAX)),
                slot=f"{WRIGGLER_SLOT_PREFIX}{index + WRIGGLER_SLOT_NUMBER_BASE}",
                start_stunned=True,
                ascension_level=self.ascension_level,
            )
            self.add_enemy(wriggler, wriggler_ai)
            spawned.append(wriggler)
        return spawned


    def auto_play_card(
        self,
        card: CardInstance | None,
        *,
        target: Creature | None = None,
        force_exhaust: bool = False,
    ) -> bool:
        """Auto-play a specific card already present in one of the combat piles."""
        if card is None or self.is_over:
            return False

        owner = getattr(card, "owner", None) or self.player
        if getattr(card, "owner", None) is None:
            card.owner = owner
        if not self.can_auto_play_card(card):
            self._remove_card_from_piles(card)
            self._zones_for_creature(owner)["discard"].append(card)
            return False
        self._remove_card_from_piles(card)
        target_type = card.target_type_for(owner)
        resolved_target = target if target is not None else self._resolve_target(card, None)
        if target_type == TargetType.ANY_ENEMY and resolved_target is None:
            self._zones_for_creature(owner)["discard"].append(card)
            return False
        if target_type == TargetType.ANY_ALLY and resolved_target is None:
            self._zones_for_creature(owner)["discard"].append(card)
            return False
        self._execute_card_play(card, resolved_target, spend_energy=False, force_exhaust=force_exhaust)
        self._check_combat_end()
        return True

    def request_end_turn_after_current_play(self) -> None:
        """End the player's turn immediately after the current play resolves."""
        self._end_turn_after_play = True

    def auto_play_from_draw(self, owner: Creature, count: int, *, force_exhaust: bool = False) -> None:
        state = self.combat_player_state_for(owner)
        if state is None or count <= 0 or self.is_over:
            return

        cards: list[CardInstance] = []
        for _ in range(max(0, count)):
            self._shuffle_if_needed(owner)
            if not state.draw:
                break
            card = state.draw.pop(0)
            card.owner = owner
            state.play.append(card)
            cards.append(card)

        for card in cards:
            if self.is_over:
                break
            self.auto_play_card(card, force_exhaust=force_exhaust)
            if self.pending_choice is not None:
                break

    def auto_play_random_attack_from_hand(self, owner: Creature) -> bool:
        state = self.combat_player_state_for(owner)
        if state is None or self.is_over:
            return False
        candidates = [card for card in state.hand if card.card_type == CardType.ATTACK and not card.is_unplayable]
        if not candidates:
            return False
        return self.auto_play_card(self.shuffle_rng.choice(candidates))

    def generate_card_to_hand(
        self,
        owner: Creature,
        card_name: str | None = None,
        card_type: CardType | None = None,
        rarity: str | None = None,
        *,
        generation_context: str = "combat",
    ) -> None:
        state = self.combat_player_state_for(owner)
        if state is None:
            return
        if card_name is not None:
            card_id = owner._coerce_card_id(card_name)
            if card_id is None:
                return
            self._add_generated_cards_to_hand([create_card(card_id)], owner=owner)
            return
        generated = create_distinct_character_cards(
            state.character_id,
            self.combat_card_generation_rng,
            1,
            card_type=card_type,
            rarity=rarity,
            generation_context=generation_context,
            is_multiplayer=self.is_multiplayer,
        )
        if generated:
            self._add_generated_cards_to_hand(generated, owner=owner)

    def create_card_in_hand(self, owner: Creature, card_name: str, *, upgraded: bool = False) -> None:
        card_id = owner._coerce_card_id(card_name)
        if card_id is None:
            return
        self._add_generated_cards_to_hand([create_card(card_id, upgraded=upgraded)], owner=owner)

    def generate_random_cards_to_hand(
        self,
        owner: Creature,
        card_type: CardType | None = None,
        count: int = 1,
        *,
        generation_context: str = "combat",
        ethereal: bool = False,
    ) -> None:
        state = self.combat_player_state_for(owner)
        if state is None or count <= 0:
            return
        generated = create_character_cards(
            state.character_id,
            self.combat_card_generation_rng,
            count,
            card_type=card_type,
            distinct=False,
            generation_context=generation_context,
            is_multiplayer=self.is_multiplayer,
        )
        if ethereal:
            for card in generated:
                card.keywords = frozenset(set(card.keywords) | {"ethereal"})
        self._add_generated_cards_to_hand(generated, owner=owner)

    def retrieve_attacks_from_discard(self, owner: Creature, count: int, *, upgrade: bool = False) -> None:
        state = self.combat_player_state_for(owner)
        if state is None or count <= 0:
            return
        candidates = [card for card in state.discard if card.card_type == CardType.ATTACK]
        if not candidates:
            return
        self.combat_card_selection_rng.shuffle(candidates)
        for card in candidates[:count]:
            self.move_card_to_creature_hand(owner, card)
            if upgrade and card.upgraded is False:
                self.upgrade_card(card)

    def generate_ethereal_cards(
        self,
        owner: Creature,
        count: int,
        *,
        generation_context: str = "combat",
    ) -> None:
        state = self.combat_player_state_for(owner)
        if state is None or count <= 0:
            return
        generated = create_distinct_character_cards(
            state.character_id,
            self.combat_card_generation_rng,
            count,
            require_keyword="ethereal",
            generation_context=generation_context,
            is_multiplayer=self.is_multiplayer,
        )
        self._add_generated_cards_to_hand(generated, owner=owner)

    def generate_colorless_cards(self, owner: Creature, count: int) -> None:
        state = self.combat_player_state_for(owner)
        if state is None or count <= 0:
            return
        colorless_ids = eligible_registered_cards(
            card_pool=CardPoolId.COLORLESS,
            generation_context="combat",
            is_multiplayer=self.is_multiplayer,
        )
        generated = create_cards_from_ids(
            colorless_ids,
            self.combat_card_generation_rng,
            count,
            distinct=True,
        )
        self._add_generated_cards_to_hand(generated, owner=owner)

    def generate_free_card_in_hand(self, owner: Creature) -> None:
        state = self.combat_player_state_for(owner)
        if state is None:
            return
        generated = create_distinct_character_cards(
            state.character_id,
            self.combat_card_generation_rng,
            1,
            generation_context="combat",
            is_multiplayer=self.is_multiplayer,
        )
        for card in generated:
            card.set_combat_cost(0)
        self._add_generated_cards_to_hand(generated, owner=owner)

    def generate_free_attack_in_hand(self, owner: Creature) -> None:
        state = self.combat_player_state_for(owner)
        if state is None:
            return
        generated = create_character_cards(
            state.character_id,
            self.combat_card_generation_rng,
            1,
            card_type=CardType.ATTACK,
            distinct=True,
            generation_context="combat",
            is_multiplayer=self.is_multiplayer,
        )
        for card in generated:
            card.set_temporary_cost_for_turn(0)
        self._add_generated_cards_to_hand(generated, owner=owner)

    def move_zero_cost_cards_to_hand(self, owner: Creature, count: int) -> None:
        state = self.combat_player_state_for(owner)
        if state is None or count <= 0:
            return
        candidates = [
            card for card in state.draw
            if not getattr(card, "has_energy_cost_x", False) and getattr(card, "cost", 0) == 0
        ]
        if not candidates:
            return
        self.combat_card_selection_rng.shuffle(candidates)
        for card in candidates[:count]:
            self.move_card_to_creature_hand(owner, card)

    def add_status_cards_to_discard(self, owner: Creature, card_name: str, count: int) -> None:
        if self.combat_player_state_for(owner) is None or count <= 0:
            return
        for _ in range(count):
            card = self._make_named_card(card_name)
            if card is not None:
                self.add_generated_card_to_creature_discard(owner, card, added_by_player=False)

    def add_status_cards_to_draw(
        self,
        owner: Creature,
        card_name: str,
        count: int,
        *,
        random_position: bool = True,
    ) -> None:
        if self.combat_player_state_for(owner) is None or count <= 0:
            return
        for _ in range(count):
            card = self._make_named_card(card_name)
            if card is not None:
                self.add_generated_card_to_creature_draw_pile(
                    owner,
                    card,
                    added_by_player=False,
                    random_position=random_position,
                )

    def _add_generated_cards_to_hand(
        self,
        cards: Sequence[CardInstance],
        *,
        owner: Creature | None = None,
    ) -> None:
        target = owner or self.player
        for card in cards:
            self.add_generated_card_to_creature_hand(target, card)

    def _fire_after_card_played_late(self, played_card: CardInstance) -> None:
        seen_ids: set[int] = set()
        for state in self.combat_player_states:
            for pile in state.all_piles:
                for card in list(pile):
                    instance_id = getattr(card, "instance_id", id(card))
                    if instance_id in seen_ids:
                        continue
                    seen_ids.add(instance_id)
                    fire_card_late_effects(card, played_card, self)

    def create_ethereal_clone_in_hand(self, owner: Creature, card: CardInstance) -> None:
        if self.combat_player_state_for(owner) is None:
            return
        clone = card.clone(new_card_instance_id())
        clone.keywords = frozenset(set(clone.keywords) | {"ethereal"})
        self.add_generated_card_to_creature_hand(owner, clone)

    def reduce_random_card_cost_to_zero(self, owner: Creature) -> None:
        state = self.combat_player_state_for(owner)
        if state is None:
            return
        candidates = [card for card in state.hand if not getattr(card, "has_energy_cost_x", False)]
        if not candidates:
            return
        self.combat_card_selection_rng.shuffle(candidates)
        candidates[0].set_temporary_cost_for_turn(0)

    def add_shivs_to_hand(self, owner: Creature, count: int) -> None:
        from sts2_env.cards.status import make_shiv

        if self.combat_player_state_for(owner) is None:
            return
        for _ in range(max(0, count)):
            self.add_generated_card_to_creature_hand(owner, make_shiv())

    def _make_named_card(self, card_name: str) -> CardInstance | None:
        from sts2_env.cards.status import (
            make_apotheosis,
            make_burn,
            make_dazed,
            make_folly,
            make_frantic_escape,
            make_greed,
            make_luminesce,
            make_soot,
            make_wish,
            make_wound,
        )

        factories = {
            "APOTHEOSIS": make_apotheosis,
            "BURN": make_burn,
            "DAZED": make_dazed,
            "FOLLY": make_folly,
            "SOOT": make_soot,
            "WOUND": make_wound,
            "FRANTIC_ESCAPE": make_frantic_escape,
            "GREED": make_greed,
            "LUMINESCE": make_luminesce,
            "WISH": make_wish,
        }
        factory = factories.get(card_name.upper())
        return factory() if factory is not None else None

    def _apply_card_after_card_drawn_early(self, card: CardInstance, owner: Creature) -> None:
        if owner.get_power_amount(PowerId.HELLRAISER) <= 0:
            return
        if CardTag.STRIKE not in card.tags:
            return
        self.auto_play_card(card)

    def _cleanup_cards_end_of_turn(self) -> None:
        for state in self.combat_player_states:
            for pile in state.all_piles:
                for card in pile:
                    card.end_of_turn_cleanup()

    def _has_turn_end_in_hand_effect(self, card: CardInstance) -> bool:
        return card.has_turn_end_in_hand_effect

    def _execute_turn_end_in_hand_effect(self, card: CardInstance, cards_in_hand_at_turn_end: int) -> None:
        if not card.has_turn_end_in_hand_effect:
            return
        fire_card_turn_end_in_hand(card, self, cards_in_hand_at_turn_end)

    # ---- Combat end ----

    def _check_combat_end(self) -> None:
        def _blocks_combat_end(enemy: Creature, power: object) -> bool:
            should_stop = getattr(power, "should_stop_combat_ending", None)
            if not callable(should_stop):
                return False
            try:
                return bool(should_stop(enemy, self))
            except TypeError:
                return bool(should_stop())

        blocking_dead_enemies = [
            enemy
            for enemy in self.enemies
            if not enemy.is_alive and any(
                _blocks_combat_end(enemy, power)
                for power in enemy.powers.values()
            )
        ]
        if not self.alive_enemies and not blocking_dead_enemies:
            self._end_combat(player_won=True)
        elif self.primary_player.is_dead:
            self._end_combat(player_won=False)

    def _end_combat(self, player_won: bool) -> None:
        from sts2_env.core.hooks import fire_after_combat_end, fire_after_combat_victory

        if self.is_over:
            return

        self.is_over = True
        self.player_won = player_won
        fire_after_combat_end(self)
        if player_won:
            fire_after_combat_victory(self)

    def __repr__(self) -> str:
        return (
            f"CombatState(round={self.round_number}, energy={self.energy}, "
            f"hand={len(self.hand)}, draw={len(self.draw_pile)}, "
            f"discard={len(self.discard_pile)}, "
            f"player={self.primary_player}, allies={self.alive_allies}, enemies={self.alive_enemies})"
        )
