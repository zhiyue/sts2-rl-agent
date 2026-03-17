"""CombatState: the central combat simulation engine.

This module intentionally mirrors the decompiled `CombatManager` turn flow
closely enough for headless simulation: start-of-side hooks, energy reset,
hand draw modification, player end-turn phase separation, enemy end-turn
hooks, and hook-driven relic/power dispatch.
"""

from __future__ import annotations

from contextlib import contextmanager
from typing import TYPE_CHECKING, Sequence

from sts2_env.cards.base import CardInstance
from sts2_env.cards.factory import (
    create_card,
    create_character_cards,
    create_distinct_character_cards,
)
from sts2_env.cards.registry import fire_card_late_effects, play_card_effect
from sts2_env.characters.all import ALL_CHARACTERS, get_character
from sts2_env.core.attack import AttackContext
from sts2_env.core.combat_player import CombatPlayerState
from sts2_env.core.constants import BASE_DRAW, BASE_ENERGY, MAX_HAND_SIZE
from sts2_env.core.creature import Creature
from sts2_env.core.enums import CardId, CardTag, CardType, CombatSide, PowerId, TargetType, ValueProp
from sts2_env.core.rng import Rng
from sts2_env.core.selection import CardChoiceOption, PendingCardChoice
from sts2_env.potions.base import PotionInstance

if TYPE_CHECKING:
    from sts2_env.monsters.state_machine import MonsterAI
    from sts2_env.relics.base import RelicId, RelicInstance
    from sts2_env.run.run_state import PlayerState


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
    ):
        self.rng = Rng(rng_seed)
        self.room = room

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
        self._end_turn_after_play: bool = False
        self._damage_events_this_turn: list[tuple[Creature | None, Creature, ValueProp]] = []
        self._damage_events_combat: list[tuple[Creature | None, Creature, ValueProp, int]] = []
        self._draw_events_this_turn: list[tuple[Creature, bool]] = []
        self._stars_gained_this_turn: list[tuple[Creature, int]] = []
        self._generated_cards_combat: list[Creature] = []
        self._active_card_source: object | None = None
        self._attack_context_stack: list[AttackContext] = []
        self._pending_auto_attack: AttackContext | None = None
        self._combat_started: bool = False
        self.extra_card_rewards: int = 0
        self._legacy_extra_card_rewards: int = 0
        self._played_cards_this_turn: list[CardInstance] = []
        self._played_cards_combat: list[CardInstance] = []

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
    def alive_allies(self) -> list[Creature]:
        return [ally for ally in self.allies if ally.is_alive]

    @property
    def hittable_enemies(self) -> list[Creature]:
        return self.alive_enemies

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
                "BURNING_BLOOD": "Ironclad",
                "RING_OF_THE_SNAKE": "Silent",
                "CRACKED_CORE": "Defect",
                "BOUND_PHYLACTERY": "Necrobinder",
                "DIVINE_RIGHT": "Regent",
            }
            for relic in relics:
                relic_id = getattr(relic, "relic_id", None)
                if relic_id is not None:
                    inferred = starter_relic_map.get(relic_id.name)
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

        creature.combat_id = len(self.enemies)
        creature.side = CombatSide.ENEMY
        creature.combat_state = self
        self.enemies.append(creature)
        self.enemy_ais[creature.combat_id] = ai
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
        state.draw[:] = list(state.starting_deck)
        state.discard.clear()
        state.exhaust.clear()
        state.play.clear()
        self.rng.shuffle(state.draw)

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
        self._damage_events_this_turn = []
        self._damage_events_combat = []
        self._draw_events_this_turn = []
        self._stars_gained_this_turn = []
        self._generated_cards_combat = []
        self._played_cards_this_turn = []
        self._played_cards_combat = []
        self.extra_card_rewards = 0

        for state in self.combat_player_states:
            state.creature.combat_state = self
            self._reset_player_combat_state(state)

        fire_before_combat_start(self)

        for enemy in self.alive_enemies:
            ai = self.enemy_ais[enemy.combat_id]
            ai.roll_move(self.rng)

        self._start_player_turn()

    # ---- Player Turn ----

    def _start_player_turn(self) -> None:
        """Start-of-player-side lifecycle."""
        from sts2_env.core.hooks import (
            fire_after_block_cleared,
            fire_after_energy_reset,
            fire_after_side_turn_start,
            fire_before_side_turn_start,
            modify_hand_draw,
            should_reset_energy,
        )

        self.current_side = CombatSide.PLAYER
        self.turn_count += 1
        self._pending_retain_count = {}
        self._damage_events_this_turn = []
        self._draw_events_this_turn = []
        self._stars_gained_this_turn = []
        self._played_cards_this_turn = []

        fire_before_side_turn_start(CombatSide.PLAYER, self)

        for state in self.combat_player_states:
            owner = state.creature
            with self.acting_player_view(owner):
                if self.round_number > 1:
                    owner.clear_block(self)
                fire_after_block_cleared(owner, self)

                if owner is self.primary_player:
                    if should_reset_energy(self):
                        self.energy = self.max_energy
                    else:
                        self.energy += self.max_energy
                else:
                    state.energy = state.base_max_energy

                draw_count = modify_hand_draw(BASE_DRAW, self) if owner is self.primary_player else BASE_DRAW
                draw_count = self._prepare_opening_draw_for_owner(owner, draw_count)
                self._draw_cards_for_creature(owner, draw_count, from_hand_draw=True)

                if state.orb_queue is not None:
                    state.orb_queue.trigger_after_turn_start(self)

        fire_after_energy_reset(self)

        fire_after_side_turn_start(CombatSide.PLAYER, self)
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

    def can_play_card(self, card: CardInstance) -> bool:
        """Check whether a card can be played right now."""
        from sts2_env.core.hooks import should_play

        owner = getattr(card, "owner", None) or self.player
        owner_state = self.combat_player_state_for(owner) or self.current_player_state
        if self.is_over:
            return False
        if self.pending_choice is not None:
            return False
        if card.is_unplayable:
            return False
        if not should_play(card, self):
            return False

        if card.card_id == CardId.CLASH and any(hand_card.card_type != CardType.ATTACK for hand_card in owner_state.hand):
            return False
        if (
            card.card_id != CardId.ENTHRALLED
            and any(hand_card.card_id == CardId.ENTHRALLED for hand_card in owner_state.hand)
        ):
            return False
        if (
            CardTag.OSTY_ATTACK in getattr(card, "tags", ())
            or "osty_attack" in getattr(card, "tags", ())
        ) and (self.osty is None or not self.osty.is_alive):
            return False
        if card.target_type == TargetType.ANY_ALLY and not self.get_player_allies_of(owner):
            return False
        if card.card_id == CardId.GRAND_FINALE and owner_state.draw:
            return False
        if card.star_cost > owner_state.stars:
            return False
        if card.has_energy_cost_x:
            return owner_state.energy > 0
        if card.cost > owner_state.energy:
            return False
        if card.card_type == CardType.STATUS:
            return not card.is_unplayable
        return True

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

        target = self._resolve_target(card, target_index)
        if card.target_type == TargetType.ANY_ENEMY and target is None:
            return False

        owner_state.hand.pop(hand_index)
        self._execute_card_play(card, target, spend_energy=True)

        if not owner_state.hand:
            fire_after_hand_emptied(self)

        self._check_combat_end()
        return True

    def _execute_card_play(
        self,
        card: CardInstance,
        target: Creature | None,
        *,
        spend_energy: bool,
        force_exhaust: bool = False,
    ) -> None:
        """Shared play pipeline for normal play and auto-play sources."""
        from sts2_env.core.hooks import fire_after_modifying_card_play_count, modify_card_play_count

        self.flush_pending_attack_context()
        owner = getattr(card, "owner", None) or self.player
        owner_state = self.combat_player_state_for(owner) or self.current_player_state
        energy_spent = 0
        if spend_energy:
            energy_spent = owner_state.energy if card.has_energy_cost_x else max(0, card.cost)
        card.owner = owner
        card.energy_spent = energy_spent

        if spend_energy:
            owner_state.energy = max(0, owner_state.energy - energy_spent)
            if card.star_cost > 0:
                self.spend_stars(owner, card.star_cost)
        if card not in owner_state.play:
            owner_state.play.append(card)

        play_count = modify_card_play_count(1 + getattr(card, "base_replay_count", 0), card, self)
        fire_after_modifying_card_play_count(card, self)
        self._pending_play = {
            "card": card,
            "target": target,
            "owner": owner,
            "remaining_plays": play_count,
            "force_exhaust": force_exhaust,
            "awaiting_after_hook": False,
        }
        self._resume_pending_play()

    def _resume_pending_play(self) -> None:
        from sts2_env.core.hooks import fire_after_card_exhausted, fire_after_card_played, fire_before_card_played

        while self._pending_play is not None:
            ctx = self._pending_play
            card = ctx["card"]
            target = ctx["target"]
            owner = ctx["owner"]
            owner_state = self.combat_player_state_for(owner) or self.current_player_state

            if ctx["awaiting_after_hook"]:
                if self.pending_choice is not None:
                    return
                with self.acting_player_view(owner):
                    fire_after_card_played(card, self)
                self._played_cards_this_turn.append(card)
                self._played_cards_combat.append(card)
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

                if card.card_type != CardType.POWER:
                    if ctx["force_exhaust"] or card.exhausts:
                        owner_state.exhaust.append(card)
                        fire_after_card_exhausted(card, self)
                    else:
                        owner_state.discard.append(card)
                self._fire_after_card_played_late(card)
                self._pending_play = None
                if self._end_turn_after_play and self.current_side == CombatSide.PLAYER and not self.is_over:
                    self._end_turn_after_play = False
                    self.end_player_turn()
                return

            with self.acting_player_view(owner):
                fire_before_card_played(card, self)
            ctx["remaining_plays"] -= 1
            previous_card_source = self._active_card_source
            self._active_card_source = card
            try:
                with self.acting_player_view(owner):
                    play_card_effect(card, self, target)
            finally:
                self._active_card_source = previous_card_source
            if self.pending_choice is not None:
                ctx["awaiting_after_hook"] = True
                return
            with self.acting_player_view(owner):
                fire_after_card_played(card, self)
            self._played_cards_this_turn.append(card)
            self._played_cards_combat.append(card)
            if self.is_over:
                self._pending_play = None
                return

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
        if self.is_over:
            return

        self.round_number += 1
        self._start_player_turn()

    def _resolve_end_of_turn_hand(self) -> None:
        from sts2_env.core.hooks import fire_after_card_discarded, fire_after_card_exhausted, should_flush

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
                    fire_after_card_exhausted(card, self)

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
                        fire_after_card_exhausted(card, self)
                    else:
                        self.discard_pile.append(card)
                        fire_after_card_discarded(card, self)

                retained: list[CardInstance] = []
                retained_ids: set[int] = set()

                for card in self.hand:
                    if card.should_retain_this_turn:
                        retained.append(card)
                        retained_ids.add(id(card))

                retain_budget = self._pending_retain_count.get(state.player_state.player_id, 0)
                if retain_budget > 0:
                    for card in self.hand:
                        if id(card) in retained_ids:
                            continue
                        retained.append(card)
                        retained_ids.add(id(card))
                        if len(retained_ids) >= len([c for c in self.hand if c.is_retain]) + retain_budget:
                            break

                remaining = [card for card in self.hand if id(card) not in retained_ids]
                flush_hand = should_flush(self)

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

        fire_before_side_turn_start(CombatSide.ENEMY, self)

        for enemy in list(self.alive_enemies):
            enemy.clear_block(self)
            fire_after_block_cleared(enemy, self)

        fire_after_side_turn_start(CombatSide.ENEMY, self)
        self._check_combat_end()
        if self.is_over:
            return

        for enemy in list(self.alive_enemies):
            ai = self.enemy_ais[enemy.combat_id]
            move = ai.current_move
            move.perform(self)
            ai.on_move_performed()

            self._check_combat_end()
            if self.is_over:
                return

        for enemy in list(self.alive_enemies):
            ai = self.enemy_ais[enemy.combat_id]
            ai.roll_move(self.rng)

        fire_before_turn_end(CombatSide.ENEMY, self)
        self._check_combat_end()
        if self.is_over:
            return

        self._cleanup_cards_end_of_turn()
        fire_after_turn_end(CombatSide.ENEMY, self)
        self._check_combat_end()

    # ---- Card pile operations ----

    def _draw_cards(self, count: int, from_hand_draw: bool = False) -> None:
        self._draw_cards_for_creature(self.player, count, from_hand_draw=from_hand_draw)

    def _draw_cards_for_creature(
        self,
        owner: Creature,
        count: int,
        *,
        from_hand_draw: bool = False,
    ) -> None:
        """Draw cards one at a time, reshuffling if needed."""
        from sts2_env.core.hooks import fire_after_card_drawn, should_draw

        state = self.combat_player_state_for(owner)
        if state is None:
            return
        if not should_draw(self, from_hand_draw):
            return

        for _ in range(max(0, count)):
            if len(state.hand) >= MAX_HAND_SIZE:
                break
            self._shuffle_if_needed(owner)
            if not state.draw:
                break
            card = state.draw.pop(0)
            setattr(card, "owner", owner)
            state.hand.append(card)
            self._draw_events_this_turn.append((owner, from_hand_draw))
            fire_after_card_drawn(card, from_hand_draw, self)
            self._invoke_card_drawn(card, from_hand_draw, owner)

    def _shuffle_if_needed(self, owner: Creature | None = None) -> None:
        """If draw pile is empty and discard has cards, shuffle discard into draw."""
        state = self.combat_player_state_for(owner or self.player) or self._primary_player_state
        if not state.draw and state.discard:
            from sts2_env.core.hooks import fire_after_shuffle

            state.draw[:] = list(state.discard)
            state.discard.clear()
            self.rng.shuffle(state.draw)
            fire_after_shuffle(self)

    def add_card_to_discard(self, card: CardInstance) -> None:
        """Add a generated card to discard pile."""
        self.discard_pile.append(card)

    def held_potions(self, owner: Creature | None = None) -> list[PotionInstance]:
        state = self.combat_player_state_for(owner or self.player) or self._primary_player_state
        return [p for p in state.potions if p is not None]

    def add_potion(self, potion: PotionInstance, owner: Creature | None = None) -> bool:
        state = self.combat_player_state_for(owner or self.player) or self._primary_player_state
        for i in range(state.max_potion_slots):
            if i >= len(state.potions):
                state.potions.append(None)
            if state.potions[i] is None:
                potion.slot_index = i
                state.potions[i] = potion
                return True
        return False

    def procure_random_potion(
        self,
        owner: Creature | None = None,
        *,
        in_combat: bool = True,
    ) -> PotionInstance | None:
        from sts2_env.potions.base import create_potion, roll_random_potion_model

        state = self.combat_player_state_for(owner or self.player) or self._primary_player_state
        potion_model = roll_random_potion_model(
            self.rng,
            character_id=state.character_id,
            in_combat=in_combat,
        )
        if potion_model is None:
            return None
        potion = create_potion(potion_model.potion_id)
        if self.add_potion(potion, owner=state.creature):
            return potion
        return None

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
        if card.target_type in (TargetType.SELF, TargetType.NONE):
            return owner
        if card.target_type == TargetType.ALL_ENEMIES:
            return None
        if card.target_type == TargetType.ALL_ALLIES:
            return None
        if card.target_type == TargetType.RANDOM_ENEMY:
            alive = self.alive_enemies
            return self.rng.choice(alive) if alive else None
        if card.target_type == TargetType.ANY_ENEMY:
            if target_index is not None and 0 <= target_index < len(self.enemies):
                enemy = self.enemies[target_index]
                if enemy.is_alive:
                    return enemy
            alive = self.alive_enemies
            return alive[0] if alive else None
        if card.target_type == TargetType.ANY_ALLY:
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
    ) -> None:
        """Apply a power to a creature. Player-side debuffs skip first tick."""
        if applier is None:
            applier = getattr(self.active_card_source, "owner", None)
            if applier is None and self.current_side == CombatSide.PLAYER:
                applier = self.primary_player
        target.apply_power(power_id, amount, applier=applier, source=source)

        if target.side == CombatSide.PLAYER:
            power = target.powers.get(power_id)
            if power is not None and power.power_type.name == "DEBUFF":
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
            for power in creature.powers.values():
                power.after_power_amount_changed(
                    creature,
                    target,
                    power_id,
                    amount,
                    applier,
                    source,
                    self,
                )

    def request_retain(self, owner: Creature, count: int) -> None:
        if self.combat_player_state_for(owner) is not None and count > 0:
            player_id = self.combat_player_state_for(owner).player_state.player_id
            self._pending_retain_count[player_id] = self._pending_retain_count.get(player_id, 0) + count

    def request_discard(self, owner: Creature, count: int) -> None:
        from sts2_env.core.hooks import fire_after_card_discarded

        state = self.combat_player_state_for(owner)
        if state is None or count <= 0:
            return
        hand = state.hand
        discard = state.discard
        for _ in range(min(count, len(hand))):
            card = hand.pop()
            discard.append(card)
            fire_after_card_discarded(card, self)

    def reduce_retained_card_cost(self, owner: Creature) -> None:
        state = self.combat_player_state_for(owner)
        if state is None:
            return
        for card in state.hand:
            if not card.has_energy_cost_x and card.cost > 0:
                card.cost -= 1
                break  # Only reduce one card per turn
                break

    def gain_energy(self, owner: Creature, amount: int) -> None:
        state = self.combat_player_state_for(owner)
        if state is not None and amount > 0:
            state.energy += amount

    def lose_energy(self, owner: Creature, amount: int) -> None:
        state = self.combat_player_state_for(owner)
        if state is not None and amount > 0:
            state.energy = max(0, state.energy - amount)

    def gain_gold(self, owner: Creature, amount: int) -> int:
        state = self.combat_player_state_for(owner)
        if state is None or amount <= 0:
            return 0
        for relic in self.relics_for_creature(owner):
            if relic.should_gain_gold(owner, amount) is False:
                return 0
        state.player_state.gain_gold(amount)
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
        if state is not None and amount > 0:
            state.stars += amount
            owner.gain_stars(amount)
            self._stars_gained_this_turn.append((owner, amount))
            for creature in self.all_creatures:
                for power in creature.powers.values():
                    power.on_stars_gained(creature, amount, self)
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
        for creature in self.all_creatures:
            for power in creature.powers.values():
                power.on_stars_spent(creature, spent, self)
        for relic in self.relics_for_creature(owner):
            relic.on_stars_spent(owner, spent, self)
        return spent

    def draw_cards(self, owner: Creature, count: int) -> None:
        if self.combat_player_state_for(owner) is not None and count > 0:
            self._draw_cards_for_creature(owner, count, from_hand_draw=False)

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
            if logged_dealer is dealer and logged_target is target and props.is_powered()
        )

    def count_powered_hits_by_dealer_this_turn(self, dealer: Creature) -> int:
        return sum(
            1
            for logged_dealer, _, props in self._damage_events_this_turn
            if logged_dealer is dealer and props.is_powered()
        )

    def count_unblocked_hits_received_this_combat(self, target: Creature) -> int:
        return sum(
            1
            for _, logged_target, _, unblocked in self._damage_events_combat
            if logged_target is target and unblocked > 0
        )

    def count_non_hand_draws_this_turn(self, owner: Creature) -> int:
        return sum(
            1
            for logged_owner, from_hand_draw in self._draw_events_this_turn
            if logged_owner is owner and not from_hand_draw
        )

    def count_generated_cards_this_combat(self, owner: Creature) -> int:
        return sum(1 for logged_owner in self._generated_cards_combat if logged_owner is owner)

    def count_stars_gained_this_turn(self, owner: Creature) -> int:
        return sum(amount for logged_owner, amount in self._stars_gained_this_turn if logged_owner is owner)

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
        enemies = self.get_enemies_of(owner)
        return self.rng.choice(enemies) if enemies else None

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
    ) -> None:
        """Pause combat resolution until a card selection is made."""
        owner = self.player

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
    ) -> None:
        """Pause combat resolution until multiple cards are selected and confirmed."""
        if max_count is None:
            max_count = min_count
        owner = self.player

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

        if self.pending_choice is None and self._pending_play is not None:
            self._resume_pending_play()
        self._check_combat_end()
        return True

    def move_card_to_hand(self, card: CardInstance | None) -> None:
        self.move_card_to_creature_hand(self.player, card)

    def move_card_to_creature_hand(self, creature: Creature, card: CardInstance | None) -> None:
        if card is None:
            return
        self._remove_card_from_piles(card)
        card.owner = creature
        zones = self._zones_for_creature(creature)
        if len(zones["hand"]) < MAX_HAND_SIZE:
            zones["hand"].append(card)
        else:
            zones["discard"].append(card)

    def move_card_to_discard(self, card: CardInstance | None) -> None:
        self.move_card_to_creature_discard(self.player, card)

    def move_card_to_creature_discard(self, creature: Creature, card: CardInstance | None) -> None:
        if card is None:
            return
        self._remove_card_from_piles(card)
        card.owner = creature
        zones = self._zones_for_creature(creature)
        zones["discard"].append(card)

    def discard_cards(self, cards: Sequence[CardInstance], draw_count: int = 0) -> None:
        """Discard cards, then draw, then auto-play any Sly cards."""
        sly_cards: list[CardInstance] = []
        for card in list(cards):
            if card in self.hand:
                if card.is_sly:
                    sly_cards.append(card)
                self.move_card_to_discard(card)
        if draw_count > 0:
            self._draw_cards(draw_count)
        for sly_card in sly_cards:
            self.auto_play_card(sly_card)

    def exhaust_card(self, card: CardInstance | None) -> None:
        from sts2_env.core.hooks import fire_after_card_exhausted

        if card is None:
            return
        self._remove_card_from_piles(card)
        owner = getattr(card, "owner", None) or self.player
        card.owner = owner
        self._zones_for_creature(owner)["exhaust"].append(card)
        fire_after_card_exhausted(card, self)

    def clone_card_to_hand(self, owner: Creature, card: CardInstance | None) -> None:
        if self.combat_player_state_for(owner) is None or card is None:
            return
        clone = card.clone(self.rng.next_int(1, 2**31 - 1))
        self.move_card_to_creature_hand(owner, clone)

    def insert_card_into_draw_pile(self, card: CardInstance | None, *, random_position: bool = False) -> None:
        self.insert_card_into_creature_draw_pile(self.player, card, random_position=random_position)

    def insert_card_into_creature_draw_pile(
        self,
        creature: Creature,
        card: CardInstance | None,
        *,
        random_position: bool = False,
    ) -> None:
        if card is None:
            return
        self._remove_card_from_piles(card)
        card.owner = creature
        zones = self._zones_for_creature(creature)
        if random_position:
            insert_at = self.rng.next_int(0, len(zones["draw"]))
            zones["draw"].insert(insert_at, card)
        else:
            zones["draw"].insert(0, card)

    def _remove_card_from_piles(self, card: CardInstance) -> None:
        for state in self.combat_player_states:
            for pile in state.all_piles:
                if card in pile:
                    pile.remove(card)
                    return

    def _zones_for_creature(self, creature: Creature) -> dict[str, list[CardInstance]]:
        state = self.combat_player_state_for(creature)
        if state is not None:
            return state.zone_map
        return {"hand": [], "draw": [], "discard": [], "exhaust": []}

    def upgrade_card(self, card: CardInstance | None) -> CardInstance | None:
        if card is None or card.upgraded:
            return card
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
        card.enchantments = dict(card.enchantments)
        card.effect_vars = dict(upgraded.effect_vars)
        card.has_energy_cost_x = upgraded.has_energy_cost_x
        card.star_cost = upgraded.star_cost
        card.original_cost = upgraded.original_cost
        if had_turn_override:
            card.cost = current_cost
        else:
            card.cost = upgraded.cost
        return card

    def transform_card(self, old_card: CardInstance | None, new_card: CardInstance | None) -> CardInstance | None:
        if old_card is None or new_card is None:
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

        new_card.owner = getattr(old_card, "owner", None) or self.primary_player
        target_pile[target_index] = new_card
        return new_card

    def upgrade_random_cards(self, pile: list[CardInstance], count: int) -> list[CardInstance]:
        """Upgrade up to `count` upgradable cards chosen uniformly from a pile."""
        candidates = [card for card in pile if not card.upgraded]
        if not candidates or count <= 0:
            return []
        chosen = self.rng.sample(candidates, min(count, len(candidates)))
        for card in chosen:
            self.upgrade_card(card)
        return chosen

    def channel_orb(self, owner: Creature, orb_type: str) -> None:
        from sts2_env.core.enums import OrbType

        state = self.combat_player_state_for(owner)
        orb_queue = getattr(state, "orb_queue", None)
        if orb_queue is not None:
            if isinstance(orb_type, str):
                orb_type = OrbType[orb_type]
            orb_queue.channel(orb_type, self)

    def trigger_first_orb_passive(self, owner: Creature) -> None:
        state = self.combat_player_state_for(owner)
        orb_queue = getattr(state, "orb_queue", None)
        if orb_queue is not None:
            orb_queue.trigger_first_passive(self)

    def summon_osty(self, owner: Creature, amount: int, source: object | None = None) -> Creature | None:
        from sts2_env.core.hooks import (
            fire_after_creature_added_to_combat,
            fire_after_osty_revived,
            fire_after_summon,
            modify_summon_amount,
        )

        if owner is not self.primary_player:
            return
        amount = modify_summon_amount(owner, amount, source, self)
        if amount <= 0:
            return self.osty

        osty = self.osty or next(
            (
                ally
                for ally in self.allies
                if getattr(ally, "is_osty", False) and getattr(ally, "pet_owner", None) is owner
            ),
            None,
        )
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
            self.osty = osty
            self.allies.append(osty)
            fire_after_creature_added_to_combat(osty, self)
            osty.apply_power(PowerId.DIE_FOR_YOU, 1)
        else:
            self.osty = osty

        if osty.is_alive and not is_new:
            osty.gain_max_hp(amount)
            fire_after_summon(owner, amount, self)
            return osty

        osty.max_hp = amount
        osty.current_hp = amount
        osty.block = 0
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
        if owner is not self.primary_player or self.osty is None or not self.osty.is_alive:
            return False
        self.kill_creature(self.osty)
        return True

    def kill_creature(self, creature: Creature | None) -> bool:
        """Immediately kill a creature."""
        if creature is None or not creature.is_alive:
            return False
        creature.current_hp = 0
        creature.block = 0
        self._check_combat_end()
        return True

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
        killed = 0
        for enemy in list(self.alive_enemies):
            doom = enemy.get_power_amount(PowerId.DOOM)
            if doom > 0 and enemy.current_hp <= doom:
                if self.kill_creature(enemy):
                    killed += 1
        return killed

    def _is_player_side_player(self, creature: Creature) -> bool:
        return creature.side == CombatSide.PLAYER and getattr(creature, "is_player", False)

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

    def count_cards_played_this_turn(
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

    def _sovereign_blades_for_creature(
        self,
        creature: Creature,
        *,
        include_exhausted: bool,
    ) -> list[CardInstance]:
        return [
            card
            for card in self._all_cards_for_creature(creature, include_exhausted=include_exhausted)
            if card.card_id == CardId.SOVEREIGN_BLADE
        ]

    def forge(self, owner: Creature, amount: int, source: object | None = None) -> None:
        """Regent forge mechanic mirroring ForgeCmd: ensure blade, buff all, then fire hooks."""
        from sts2_env.cards.status import make_sovereign_blade
        from sts2_env.core.hooks import fire_after_forge

        if self.is_over or amount <= 0 or not self._is_player_side_player(owner) or not owner.is_alive:
            return

        active_blades = self._sovereign_blades_for_creature(owner, include_exhausted=False)
        if not active_blades:
            blade = make_sovereign_blade()
            blade.owner = owner
            blade.combat_vars["created_through_forge"] = 1
            self.move_card_to_creature_hand(owner, blade)

        all_blades = self._sovereign_blades_for_creature(owner, include_exhausted=True)
        for blade in all_blades:
            blade.base_damage = (blade.base_damage or 10) + amount
            blade.after_forged()
        fire_after_forge(self, amount, owner, source)

    def stun_enemy(self, creature: Creature) -> bool:
        """Replace an enemy's next move with a one-turn stun."""
        if creature is None or creature.is_player or creature.is_dead:
            return False
        ai = self.enemy_ais.get(creature.combat_id)
        if ai is None:
            return False

        from sts2_env.core.enums import IntentType
        from sts2_env.monsters.intents import Intent
        from sts2_env.monsters.state_machine import MoveState

        next_state_id = ai.current_move.state_id

        def _stunned(_: CombatState) -> None:
            return

        stunned = MoveState("STUNNED", _stunned, [Intent(IntentType.STUN)], follow_up_id=next_state_id, must_perform_once=True)
        ai.states["STUNNED"] = stunned
        ai._current_state_id = "STUNNED"  # noqa: SLF001
        ai._performed_first_move = True  # noqa: SLF001
        return True

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
        self._remove_card_from_piles(card)
        resolved_target = target if target is not None else self._resolve_target(card, None)
        if card.target_type == TargetType.ANY_ENEMY and resolved_target is None:
            self._zones_for_creature(owner)["discard"].append(card)
            return False
        if card.target_type == TargetType.ANY_ALLY and resolved_target is None:
            self._zones_for_creature(owner)["discard"].append(card)
            return False
        self._execute_card_play(card, resolved_target, spend_energy=False, force_exhaust=force_exhaust)
        self._check_combat_end()
        return True

    def request_end_turn_after_current_play(self) -> None:
        """End the player's turn immediately after the current play resolves."""
        self._end_turn_after_play = True

    def auto_play_from_draw(self, owner: Creature, count: int) -> None:
        state = self.combat_player_state_for(owner)
        if state is None or count <= 0 or self.is_over:
            return

        for _ in range(max(0, count)):
            self._shuffle_if_needed(owner)
            if not state.draw:
                break
            card = state.draw.pop(0)
            self.auto_play_card(card)

    def generate_card_to_hand(
        self,
        owner: Creature,
        card_type: CardType | None = None,
        rarity: str | None = None,
    ) -> None:
        state = self.combat_player_state_for(owner)
        if state is None:
            return
        generated = create_distinct_character_cards(
            state.character_id,
            self.rng,
            1,
            card_type=card_type,
            rarity=rarity,
        )
        if generated:
            self._add_generated_cards_to_hand(generated, owner=owner)

    def generate_random_cards_to_hand(
        self,
        owner: Creature,
        card_type: CardType | None = None,
        count: int = 1,
    ) -> None:
        state = self.combat_player_state_for(owner)
        if state is None or count <= 0:
            return
        generated = create_character_cards(
            state.character_id,
            self.rng,
            count,
            card_type=card_type,
            distinct=False,
            generation_context="combat",
        )
        self._add_generated_cards_to_hand(generated, owner=owner)

    def retrieve_attacks_from_discard(self, owner: Creature, count: int, *, upgrade: bool = False) -> None:
        state = self.combat_player_state_for(owner)
        if state is None or count <= 0:
            return
        candidates = [card for card in state.discard if card.card_type == CardType.ATTACK]
        if not candidates:
            return
        self.rng.shuffle(candidates)
        for card in candidates[:count]:
            self.move_card_to_creature_hand(owner, card)
            if upgrade and card.upgraded is False:
                self.upgrade_card(card)

    def generate_ethereal_cards(self, owner: Creature, count: int) -> None:
        state = self.combat_player_state_for(owner)
        if state is None or count <= 0:
            return
        generated = create_distinct_character_cards(
            state.character_id,
            self.rng,
            count,
            require_keyword="ethereal",
        )
        self._add_generated_cards_to_hand(generated, owner=owner)

    def add_status_cards_to_discard(self, owner: Creature, card_name: str, count: int) -> None:
        if self.combat_player_state_for(owner) is None or count <= 0:
            return
        for _ in range(count):
            card = self._make_named_card(card_name)
            if card is not None:
                self.move_card_to_creature_discard(owner, card)

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
                self.insert_card_into_creature_draw_pile(
                    owner,
                    card,
                    random_position=random_position,
                )

    def _add_generated_cards_to_hand(
        self,
        cards: Sequence[CardInstance],
        *,
        owner: Creature | None = None,
    ) -> None:
        target = owner or self.player
        zones = self._zones_for_creature(target)
        for card in cards:
            setattr(card, "owner", target)
            self._generated_cards_combat.append(target)
            if len(zones["hand"]) < MAX_HAND_SIZE:
                zones["hand"].append(card)
            else:
                zones["discard"].append(card)

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
        clone = card.clone(self.rng.next_int(1, 2**31 - 1))
        clone.keywords = frozenset(set(clone.keywords) | {"ethereal"})
        self.move_card_to_creature_hand(owner, clone)

    def add_shivs_to_hand(self, owner: Creature, count: int) -> None:
        from sts2_env.cards.status import make_shiv

        zones = self._zones_for_creature(owner)
        if self.combat_player_state_for(owner) is None:
            return
        for _ in range(max(0, count)):
            if len(zones["hand"]) >= MAX_HAND_SIZE:
                break
            zones["hand"].append(make_shiv())

    def _make_named_card(self, card_name: str) -> CardInstance | None:
        from sts2_env.cards.status import (
            make_dazed,
            make_frantic_escape,
            make_soot,
            make_wound,
        )

        factories = {
            "DAZED": make_dazed,
            "SOOT": make_soot,
            "WOUND": make_wound,
            "FRANTIC_ESCAPE": make_frantic_escape,
        }
        factory = factories.get(card_name.upper())
        return factory() if factory is not None else None

    def _invoke_card_drawn(self, card: CardInstance, from_hand_draw: bool, owner: Creature) -> None:
        from sts2_env.core.enums import CardId
        from sts2_env.core.damage import apply_damage

        if card.card_id == CardId.VOID:
            state = self.combat_player_state_for(owner)
            if state is not None:
                state.energy = max(0, state.energy - card.effect_vars.get("energy", 1))
            return

    def _cleanup_cards_end_of_turn(self) -> None:
        for state in self.combat_player_states:
            for pile in state.all_piles:
                for card in pile:
                    card.end_of_turn_cleanup()

    def _has_turn_end_in_hand_effect(self, card: CardInstance) -> bool:
        from sts2_env.core.enums import CardId

        return card.card_id in {
            CardId.BECKON,
            CardId.BURN,
            CardId.BAD_LUCK,
            CardId.DEBT,
            CardId.DECAY,
            CardId.DOUBT,
            CardId.REGRET,
            CardId.SHAME,
        }

    def _execute_turn_end_in_hand_effect(self, card: CardInstance, cards_in_hand_at_turn_end: int) -> None:
        from sts2_env.core.damage import apply_damage
        from sts2_env.core.enums import CardId

        turn_end_ids = {
            CardId.BECKON,
            CardId.BURN,
            CardId.BAD_LUCK,
            CardId.DEBT,
            CardId.DECAY,
            CardId.DOUBT,
            CardId.REGRET,
            CardId.SHAME,
        }
        if card.card_id not in turn_end_ids:
            return

        if card.card_id == CardId.BECKON:
            owner = getattr(card, "owner", None) or self.primary_player
            apply_damage(
                owner,
                card.effect_vars.get("hp_loss", 6),
                ValueProp.UNBLOCKABLE | ValueProp.UNPOWERED | ValueProp.MOVE,
                self,
                None,
            )
            self._check_combat_end()
            return

        if card.card_id == CardId.BURN:
            owner = getattr(card, "owner", None) or self.primary_player
            self.deal_damage(
                dealer=owner,
                target=owner,
                amount=2,
                props=ValueProp.UNPOWERED | ValueProp.MOVE,
            )
            return

        if card.card_id == CardId.BAD_LUCK:
            owner = getattr(card, "owner", None) or self.primary_player
            apply_damage(
                owner,
                13,
                ValueProp.UNBLOCKABLE | ValueProp.UNPOWERED | ValueProp.MOVE,
                self,
                None,
            )
            self._check_combat_end()
            return

        if card.card_id == CardId.DEBT:
            self.gold = max(0, self.gold - min(card.effect_vars.get("gold", 10), self.gold))
            return

        if card.card_id == CardId.DECAY:
            owner = getattr(card, "owner", None) or self.primary_player
            self.deal_damage(
                dealer=owner,
                target=owner,
                amount=2,
                props=ValueProp.UNPOWERED | ValueProp.MOVE,
            )
            return

        if card.card_id == CardId.DOUBT:
            owner = getattr(card, "owner", None) or self.primary_player
            already_had = owner.has_power(PowerId.WEAK)
            owner.apply_power(PowerId.WEAK, card.effect_vars.get("weak", 1))
            if not already_had and owner.has_power(PowerId.WEAK):
                owner.powers[PowerId.WEAK].skip_next_tick = True
            return

        if card.card_id == CardId.REGRET:
            owner = getattr(card, "owner", None) or self.primary_player
            apply_damage(
                owner,
                cards_in_hand_at_turn_end,
                ValueProp.UNBLOCKABLE | ValueProp.UNPOWERED | ValueProp.MOVE,
                self,
                None,
            )
            self._check_combat_end()
            return

        if card.card_id == CardId.SHAME:
            owner = getattr(card, "owner", None) or self.primary_player
            already_had = owner.has_power(PowerId.FRAIL)
            owner.apply_power(PowerId.FRAIL, card.effect_vars.get("frail", 1))
            if not already_had and owner.has_power(PowerId.FRAIL):
                owner.powers[PowerId.FRAIL].skip_next_tick = True

    # ---- Combat end ----

    def _check_combat_end(self) -> None:
        if not self.alive_enemies:
            self._end_combat(player_won=True)
        elif self.primary_player.is_dead:
            self._end_combat(player_won=False)

    def _end_combat(self, player_won: bool) -> None:
        from sts2_env.core.hooks import fire_after_combat_end, fire_after_combat_victory

        if self.is_over:
            return

        self.is_over = True
        self.player_won = player_won
        if player_won:
            fire_after_combat_victory(self)
        fire_after_combat_end(self)

    def __repr__(self) -> str:
        return (
            f"CombatState(round={self.round_number}, energy={self.energy}, "
            f"hand={len(self.hand)}, draw={len(self.draw_pile)}, "
            f"discard={len(self.discard_pile)}, "
            f"player={self.primary_player}, allies={self.alive_allies}, enemies={self.alive_enemies})"
        )
