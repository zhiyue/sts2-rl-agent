"""Parity tests for selected Colorless card flows backed by decompiled cards."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.colorless import make_alchemize, make_hand_of_greed
from sts2_env.cards.factory import create_card
from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.cards.ironclad_basic import make_strike_ironclad
from sts2_env.cards.status import make_dazed
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import CardId
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle
from sts2_env.relics.shop_event import DragonFruit
from sts2_env.relics.uncommon import BowlerHat


def _make_combat(relics: list[str] | None = None) -> CombatState:
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=42,
        character_id="Ironclad",
        relics=relics or [],
    )
    creature, ai = create_shrinker_beetle(Rng(42))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


class TestColorlessParity:
    def test_alchemize_procures_a_combat_generatable_potion(self):
        """Matches Alchemize.cs: procure a random in-combat potion into the belt."""
        combat = _make_combat()
        combat.hand = [make_alchemize()]
        combat.potions = [None, None, None]
        combat.max_potion_slots = 3
        combat.energy = 1

        assert combat.play_card(0)
        held = combat.held_potions()
        assert len(held) == 1
        assert held[0].slot_index == 0
        assert held[0].model.can_be_generated_in_combat is True

    def test_beat_down_autoplays_up_to_three_attack_cards_from_discard(self):
        """Matches BeatDown.cs: stable-shuffle attack cards in discard, then auto-play up to N."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp
        combat.hand = [create_card(CardId.BEAT_DOWN)]
        combat.discard_pile = [
            make_strike_ironclad(),
            make_strike_ironclad(),
            make_strike_ironclad(),
            make_dazed(),
        ]
        combat.energy = 3

        assert combat.play_card(0)
        assert enemy.current_hp == starting_hp - 18
        assert any(card.card_id == CardId.DAZED for card in combat.discard_pile)

    def test_hand_of_greed_uses_gold_gain_pipeline_on_fatal(self):
        """Matches HandOfGreed.cs: fatal kill grants gold through the standard gain-gold path."""
        combat = _make_combat(["DragonFruit"])
        enemy = combat.enemies[0]
        enemy.current_hp = 10
        enemy.max_hp = 10
        combat.hand = [make_hand_of_greed()]
        combat.energy = 2

        assert combat.play_card(0, 0)
        assert enemy.is_dead
        assert combat.gold == 20
        assert combat.player.max_hp == 81

    def test_hand_of_greed_bowler_hat_bonus_uses_combat_gold_pipeline(self):
        """Matches BowlerHat.cs:18 GoldIncrease 1.25m + :29-34 ModifyGoldGained:
        gold gained is multiplied by 1.25; the bonus is a nested standard gold
        gain (DragonFruit.cs)."""
        combat = _make_combat(["BowlerHat", "DragonFruit"])
        enemy = combat.enemies[0]
        hand_of_greed = make_hand_of_greed()
        enemy.current_hp = max(1, hand_of_greed.base_damage // 2)
        enemy.max_hp = enemy.current_hp
        combat.hand = [hand_of_greed]
        combat.energy = hand_of_greed.cost
        starting_max_hp = combat.player.max_hp
        hand_of_greed_gold = hand_of_greed.effect_vars["gold"]
        bowler_hat_bonus_gold = int(hand_of_greed_gold * BowlerHat.MULTIPLIER) - hand_of_greed_gold

        assert combat.play_card(0, 0)
        assert enemy.is_dead
        assert combat.gold == hand_of_greed_gold + bowler_hat_bonus_gold
        assert combat.player.max_hp == starting_max_hp + DragonFruit.MAX_HP * 2
        assert combat.current_player_state.player_state.max_hp == starting_max_hp + DragonFruit.MAX_HP * 2

    def test_hand_of_greed_respects_gold_gain_prevention_relics(self):
        """Matches HandOfGreed.cs fatal semantics plus relic-based gold prevention."""
        combat = _make_combat(["Ectoplasm"])
        enemy = combat.enemies[0]
        enemy.current_hp = 10
        enemy.max_hp = 10
        combat.hand = [make_hand_of_greed()]
        combat.energy = 2

        assert combat.play_card(0, 0)
        assert enemy.is_dead
        assert combat.gold == 0
