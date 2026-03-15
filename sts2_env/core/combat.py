"""CombatState: the central combat simulation engine.

This module intentionally mirrors the decompiled `CombatManager` turn flow
closely enough for headless simulation: start-of-side hooks, energy reset,
hand draw modification, player end-turn phase separation, enemy end-turn
hooks, and hook-driven relic/power dispatch.
"""

from __future__ import annotations

from typing import TYPE_CHECKING, Sequence

from sts2_env.cards.base import CardInstance
from sts2_env.cards.registry import play_card_effect
from sts2_env.core.constants import BASE_DRAW, BASE_ENERGY, MAX_HAND_SIZE
from sts2_env.core.creature import Creature
from sts2_env.core.enums import CardType, CombatSide, PowerId, TargetType, ValueProp
from sts2_env.core.rng import Rng

if TYPE_CHECKING:
    from sts2_env.monsters.state_machine import MonsterAI
    from sts2_env.relics.base import RelicId, RelicInstance


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
    ):
        self.rng = Rng(rng_seed)

        # Player
        self.player = Creature(
            max_hp=player_max_hp,
            current_hp=player_hp,
            side=CombatSide.PLAYER,
            is_player=True,
        )
        self.energy: int = 0
        self.base_max_energy: int = BASE_ENERGY
        self.stars: int = 0
        self.gold: int = gold

        # Card piles
        self.hand: list[CardInstance] = []
        self.draw_pile: list[CardInstance] = []
        self.discard_pile: list[CardInstance] = []
        self.exhaust_pile: list[CardInstance] = []
        self.play_pile: list[CardInstance] = []
        self._deck = list(deck)

        # Relics
        self.relics: list[RelicInstance] = self._coerce_relics(relics or [])

        # Enemies
        self.enemies: list[Creature] = []
        self.enemy_ais: dict[int, MonsterAI] = {}

        # Combat state
        self.round_number: int = 1
        self.current_side: CombatSide = CombatSide.PLAYER
        self.is_over: bool = False
        self.player_won: bool = False
        self.turn_count: int = 0
        self._pending_retain_count: int = 0

    @property
    def max_energy(self) -> int:
        from sts2_env.core.hooks import modify_max_energy

        return modify_max_energy(self.base_max_energy, self)

    @property
    def current_energy(self) -> int:
        return self.energy

    @property
    def all_creatures(self) -> list[Creature]:
        return [self.player] + self.enemies

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
    def hittable_enemies(self) -> list[Creature]:
        return self.alive_enemies

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

    def add_enemy(self, creature: Creature, ai: MonsterAI) -> None:
        """Add an enemy to this combat."""
        creature.combat_id = len(self.enemies)
        creature.side = CombatSide.ENEMY
        self.enemies.append(creature)
        self.enemy_ais[creature.combat_id] = ai

    def start_combat(self) -> None:
        """Initialize combat and enter the first player turn."""
        from sts2_env.core.hooks import fire_before_combat_start

        self.current_side = CombatSide.PLAYER
        self.round_number = 1
        self.energy = 0
        self.stars = 0
        self._pending_retain_count = 0

        self.draw_pile = list(self._deck)
        self.rng.shuffle(self.draw_pile)
        self.hand.clear()
        self.discard_pile.clear()
        self.exhaust_pile.clear()
        self.play_pile.clear()

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
        self._pending_retain_count = 0

        fire_before_side_turn_start(CombatSide.PLAYER, self)

        if self.round_number > 1:
            self.player.clear_block(self)
        fire_after_block_cleared(self.player, self)

        if should_reset_energy(self):
            self.energy = self.max_energy
        else:
            self.energy += self.max_energy
        fire_after_energy_reset(self)

        draw_count = modify_hand_draw(BASE_DRAW, self)
        draw_count = self._prepare_opening_draw(draw_count)
        self._draw_cards(draw_count, from_hand_draw=True)

        fire_after_side_turn_start(CombatSide.PLAYER, self)
        self._check_combat_end()

    def _prepare_opening_draw(self, draw_count: int) -> int:
        """Round-1 innate handling from `SetupPlayerTurn()`."""
        if self.round_number != 1:
            return max(0, draw_count)

        innate_cards = [card for card in self.draw_pile if card.is_innate]
        non_innate_cards = [card for card in self.draw_pile if not card.is_innate]
        self.draw_pile = innate_cards + non_innate_cards

        adjusted = max(draw_count, len(innate_cards))
        return min(MAX_HAND_SIZE, max(0, adjusted))

    def can_play_card(self, card: CardInstance) -> bool:
        """Check whether a card can be played right now."""
        from sts2_env.core.hooks import should_play

        if self.is_over:
            return False
        if card.is_unplayable:
            return False
        if not should_play(card, self):
            return False

        if card.has_energy_cost_x:
            return self.energy > 0
        if card.cost > self.energy:
            return False
        if card.card_type == CardType.STATUS:
            return not card.is_unplayable
        return True

    def play_card(self, hand_index: int, target_index: int | None = None) -> bool:
        """Play a card from hand. Returns True if successful."""
        from sts2_env.core.hooks import (
            fire_after_card_exhausted,
            fire_after_card_played,
            fire_after_hand_emptied,
            fire_after_modifying_card_play_count,
            fire_before_card_played,
            modify_card_play_count,
        )

        if hand_index < 0 or hand_index >= len(self.hand):
            return False

        card = self.hand[hand_index]
        if not self.can_play_card(card):
            return False

        target = self._resolve_target(card, target_index)
        if card.target_type == TargetType.ANY_ENEMY and target is None:
            return False

        energy_spent = self.energy if card.has_energy_cost_x else max(0, card.cost)
        card.owner = self.player
        card.energy_spent = energy_spent

        self.energy = max(0, self.energy - energy_spent)
        self.hand.pop(hand_index)
        self.play_pile.append(card)

        play_count = modify_card_play_count(1, card, self)
        fire_after_modifying_card_play_count(card, self)
        for _ in range(play_count):
            fire_before_card_played(card, self)
            play_card_effect(card, self, target)
            fire_after_card_played(card, self)

        if card in self.play_pile:
            self.play_pile.remove(card)

        if card.card_type == CardType.POWER:
            pass
        elif card.exhausts:
            self.exhaust_pile.append(card)
            fire_after_card_exhausted(card, self)
        else:
            self.discard_pile.append(card)

        if not self.hand:
            fire_after_hand_emptied(self)

        self._check_combat_end()
        return True

    def end_player_turn(self) -> None:
        """End player turn, execute enemy turn, then start the next player turn."""
        from sts2_env.core.hooks import fire_after_turn_end, fire_before_turn_end

        if self.is_over:
            return

        fire_before_turn_end(CombatSide.PLAYER, self)
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

        cards_in_hand_at_turn_end = len(self.hand)
        turn_end_cards = [card for card in list(self.hand) if self._has_turn_end_in_hand_effect(card)]
        turn_end_ids = {id(card) for card in turn_end_cards}

        # Phase 1: ethereal cards without explicit hand-end effects exhaust first.
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

        if self._pending_retain_count > 0:
            for card in self.hand:
                if id(card) in retained_ids:
                    continue
                retained.append(card)
                retained_ids.add(id(card))
                if len(retained_ids) >= len([c for c in self.hand if c.is_retain]) + self._pending_retain_count:
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

        self._pending_retain_count = 0

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
        """Draw cards one at a time, reshuffling if needed."""
        from sts2_env.core.hooks import fire_after_card_drawn, should_draw

        if not should_draw(self, from_hand_draw):
            return

        for _ in range(max(0, count)):
            if len(self.hand) >= MAX_HAND_SIZE:
                break
            self._shuffle_if_needed()
            if not self.draw_pile:
                break
            card = self.draw_pile.pop(0)
            setattr(card, "owner", self.player)
            self.hand.append(card)
            fire_after_card_drawn(card, from_hand_draw, self)
            self._invoke_card_drawn(card, from_hand_draw)

    def _shuffle_if_needed(self) -> None:
        """If draw pile is empty and discard has cards, shuffle discard into draw."""
        if not self.draw_pile and self.discard_pile:
            from sts2_env.core.hooks import fire_after_shuffle

            self.draw_pile = list(self.discard_pile)
            self.discard_pile.clear()
            self.rng.shuffle(self.draw_pile)
            fire_after_shuffle(self)

    def add_card_to_discard(self, card: CardInstance) -> None:
        """Add a generated card to discard pile."""
        self.discard_pile.append(card)

    def add_card_to_draw_pile(self, owner: Creature, card_name: str) -> None:
        card = self._make_named_card(card_name)
        if card is not None:
            self.draw_pile.insert(0, card)

    def exhaust_top_of_draw_pile(self, owner: Creature) -> None:
        self.exhaust_from_draw_pile(owner, 1)

    def exhaust_from_draw_pile(self, owner: Creature, count: int) -> None:
        from sts2_env.core.hooks import fire_after_card_exhausted

        for _ in range(max(0, count)):
            self._shuffle_if_needed()
            if not self.draw_pile:
                break
            card = self.draw_pile.pop(0)
            self.exhaust_pile.append(card)
            fire_after_card_exhausted(card, self)

    # ---- Target resolution ----

    def _resolve_target(self, card: CardInstance, target_index: int | None) -> Creature | None:
        if card.target_type in (TargetType.SELF, TargetType.NONE):
            return self.player
        if card.target_type == TargetType.ALL_ENEMIES:
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
        return None

    # ---- Power / relic helpers ----

    def apply_power_to(self, target: Creature, power_id: PowerId, amount: int) -> None:
        """Apply a power to a creature. Player-side debuffs skip first tick."""
        target.apply_power(power_id, amount)

        if target.side == CombatSide.PLAYER:
            power = target.powers.get(power_id)
            if power is not None and power.power_type.name == "DEBUFF":
                power.skip_next_tick = True

    def request_retain(self, owner: Creature, count: int) -> None:
        if owner is self.player and count > 0:
            self._pending_retain_count += count

    def request_discard(self, owner: Creature, count: int) -> None:
        from sts2_env.core.hooks import fire_after_card_discarded

        if owner is not self.player or count <= 0:
            return
        for _ in range(min(count, len(self.hand))):
            card = self.hand.pop()
            self.discard_pile.append(card)
            fire_after_card_discarded(card, self)

    def reduce_retained_card_cost(self, owner: Creature) -> None:
        if owner is not self.player:
            return
        for card in self.hand:
            if not card.has_energy_cost_x and card.cost > 0:
                card.cost -= 1
                break  # Only reduce one card per turn
                break

    def gain_energy(self, owner: Creature, amount: int) -> None:
        if owner is self.player and amount > 0:
            self.energy += amount

    def lose_energy(self, owner: Creature, amount: int) -> None:
        if owner is self.player and amount > 0:
            self.energy = max(0, self.energy - amount)

    def gain_stars(self, owner: Creature, amount: int) -> None:
        if owner is self.player and amount > 0:
            self.stars += amount
            owner.gain_stars(amount)

    def draw_cards(self, owner: Creature, count: int) -> None:
        if owner is self.player and count > 0:
            self._draw_cards(count, from_hand_draw=False)

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
        for creature in target_list:
            if creature is None or creature.is_dead:
                continue
            damage = calculate_damage(amount, dealer, creature, props, self) if dealer is not None else amount
            results.append(apply_damage(creature, damage, props, self, dealer))
        self._check_combat_end()
        return results

    def get_alive_enemies(self) -> list[Creature]:
        return self.alive_enemies

    def get_enemies_of(self, owner: Creature) -> list[Creature]:
        if owner.side == CombatSide.PLAYER:
            return self.alive_enemies
        return [self.player] if self.player.is_alive else []

    def channel_orb(self, owner: Creature, orb_type: str) -> None:
        orb_queue = getattr(self, "orb_queue", None)
        if orb_queue is not None:
            orb_queue.channel(orb_type)

    def trigger_first_orb_passive(self, owner: Creature) -> None:
        orb_queue = getattr(self, "orb_queue", None)
        if orb_queue is not None:
            orb_queue.trigger_first_passive()

    def summon_osty(self, owner: Creature, amount: int) -> None:
        # Pet simulation is not implemented yet in the headless combat core.
        return

    def auto_play_from_draw(self, owner: Creature, count: int) -> None:
        return

    def generate_card_to_hand(
        self,
        owner: Creature,
        card_type: CardType | None = None,
        rarity: str | None = None,
    ) -> None:
        return

    def generate_ethereal_cards(self, owner: Creature, count: int) -> None:
        return

    def create_ethereal_clone_in_hand(self, owner: Creature, card: CardInstance) -> None:
        clone = card.clone(self.rng.next_int(1, 2**31 - 1))
        clone.keywords = frozenset(set(clone.keywords) | {"ethereal"})
        if len(self.hand) < MAX_HAND_SIZE:
            self.hand.append(clone)

    def add_shivs_to_hand(self, owner: Creature, count: int) -> None:
        from sts2_env.cards.status import make_shiv

        if owner is not self.player:
            return
        for _ in range(max(0, count)):
            if len(self.hand) >= MAX_HAND_SIZE:
                break
            self.hand.append(make_shiv())

    def _make_named_card(self, card_name: str) -> CardInstance | None:
        from sts2_env.cards.status import make_dazed, make_soot

        factories = {
            "Dazed": make_dazed,
            "Soot": make_soot,
        }
        factory = factories.get(card_name)
        return factory() if factory is not None else None

    def _invoke_card_drawn(self, card: CardInstance, from_hand_draw: bool) -> None:
        from sts2_env.core.enums import CardId
        from sts2_env.core.damage import apply_damage

        if card.card_id == CardId.VOID:
            self.energy = max(0, self.energy - card.effect_vars.get("energy", 1))
            return
        if card.card_id == CardId.BECKON:
            apply_damage(
                self.player,
                card.effect_vars.get("hp_loss", 6),
                ValueProp.UNBLOCKABLE | ValueProp.UNPOWERED | ValueProp.MOVE,
                self,
                None,
            )
            self._check_combat_end()

    def _cleanup_cards_end_of_turn(self) -> None:
        for pile in self.all_piles:
            for card in pile:
                card.end_of_turn_cleanup()

    def _has_turn_end_in_hand_effect(self, card: CardInstance) -> bool:
        from sts2_env.core.enums import CardId

        return card.card_id in {
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

        if card.card_id == CardId.BURN:
            self.deal_damage(
                dealer=self.player,
                target=self.player,
                amount=2,
                props=ValueProp.UNPOWERED | ValueProp.MOVE,
            )
            return

        if card.card_id == CardId.BAD_LUCK:
            apply_damage(
                self.player,
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
            self.deal_damage(
                dealer=self.player,
                target=self.player,
                amount=2,
                props=ValueProp.UNPOWERED | ValueProp.MOVE,
            )
            return

        if card.card_id == CardId.DOUBT:
            already_had = self.player.has_power(PowerId.WEAK)
            self.player.apply_power(PowerId.WEAK, card.effect_vars.get("weak", 1))
            if not already_had and self.player.has_power(PowerId.WEAK):
                self.player.powers[PowerId.WEAK].skip_next_tick = True
            return

        if card.card_id == CardId.REGRET:
            apply_damage(
                self.player,
                cards_in_hand_at_turn_end,
                ValueProp.UNBLOCKABLE | ValueProp.UNPOWERED | ValueProp.MOVE,
                self,
                None,
            )
            self._check_combat_end()
            return

        if card.card_id == CardId.SHAME:
            already_had = self.player.has_power(PowerId.FRAIL)
            self.player.apply_power(PowerId.FRAIL, card.effect_vars.get("frail", 1))
            if not already_had and self.player.has_power(PowerId.FRAIL):
                self.player.powers[PowerId.FRAIL].skip_next_tick = True

    # ---- Combat end ----

    def _check_combat_end(self) -> None:
        if not self.alive_enemies:
            self._end_combat(player_won=True)
        elif self.player.is_dead:
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
            f"player={self.player}, enemies={self.alive_enemies})"
        )
