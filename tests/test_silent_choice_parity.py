"""Parity tests for Silent discard-choice flows backed by decompiled cards."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.ironclad_basic import make_bash, make_defend_ironclad, make_strike_ironclad
from sts2_env.cards.silent import (
    create_silent_starter_deck,
    make_acrobatics,
    make_dagger_throw,
    make_defend_silent,
    make_hidden_daggers,
    make_prepared,
    make_the_hunt,
)
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import PowerId, RoomType
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle
from sts2_env.run.reward_objects import CardReward
from sts2_env.run.rooms import create_room


def _make_combat() -> CombatState:
    combat = CombatState(
        player_hp=70,
        player_max_hp=70,
        deck=create_silent_starter_deck(),
        rng_seed=42,
        character_id="Silent",
    )
    creature, ai = create_shrinker_beetle(Rng(42))
    combat.add_enemy(creature, ai)
    return combat


class TestSilentDiscardChoiceParity:
    def test_acrobatics_draws_before_requesting_a_single_discard_choice(self):
        """Matches Acrobatics.cs: draw first, then choose exactly one card to discard."""
        combat = _make_combat()
        kept = make_defend_silent()
        first_draw = make_strike_ironclad()
        second_draw = make_defend_ironclad()
        third_draw = make_bash()
        combat.hand = [make_acrobatics(), kept]
        combat.draw_pile = [first_draw, second_draw, third_draw]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert [option.card for option in combat.pending_choice.options] == [kept, first_draw, second_draw, third_draw]

        assert combat.resolve_pending_choice(2)
        assert combat.pending_choice is None
        assert second_draw in combat.discard_pile
        assert kept in combat.hand
        assert first_draw in combat.hand
        assert third_draw in combat.hand

    def test_dagger_throw_draws_then_discards_the_selected_hand_card(self):
        """Matches DaggerThrow.cs: attack, draw, then choose one discard."""
        combat = _make_combat()
        kept = make_defend_silent()
        drawn = make_strike_ironclad()
        combat.hand = [make_dagger_throw(), kept]
        combat.draw_pile = [drawn]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert combat.pending_choice is not None
        assert [option.card for option in combat.pending_choice.options] == [kept, drawn]

        assert combat.resolve_pending_choice(1)
        assert combat.pending_choice is None
        assert drawn in combat.discard_pile
        assert kept in combat.hand

    def test_prepared_upgraded_style_flow_requires_exact_discard_count(self):
        """Matches Prepared.cs: discard count tracks the draw count, not a fixed pop()."""
        combat = _make_combat()
        prepared = make_prepared()
        prepared.effect_vars["cards"] = 2
        kept = make_defend_silent()
        first_draw = make_strike_ironclad()
        second_draw = make_defend_ironclad()
        combat.hand = [prepared, kept]
        combat.draw_pile = [first_draw, second_draw]

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert combat.pending_choice.is_multi is True
        assert combat.pending_choice.min_choices == 2
        assert combat.pending_choice.max_choices == 2
        assert [option.card for option in combat.pending_choice.options] == [kept, first_draw, second_draw]

        assert combat.resolve_pending_choice(0)
        assert combat.pending_choice is not None
        assert combat.resolve_pending_choice(2)
        assert combat.pending_choice is not None
        assert combat.resolve_pending_choice(None)
        assert combat.pending_choice is None
        assert kept in combat.discard_pile
        assert second_draw in combat.discard_pile
        assert first_draw in combat.hand

    def test_hidden_daggers_discards_exact_selected_cards_before_creating_shivs(self):
        """Matches HiddenDaggers.cs: discard selected cards first, then add Shiv payload."""
        combat = _make_combat()
        strike = make_strike_ironclad()
        defend = make_defend_ironclad()
        bash = make_bash()
        combat.hand = [make_hidden_daggers(), strike, defend, bash]

        assert combat.play_card(0)
        assert combat.pending_choice is not None
        assert combat.pending_choice.is_multi is True
        assert combat.pending_choice.min_choices == 2
        assert combat.pending_choice.max_choices == 2
        assert [option.card for option in combat.pending_choice.options] == [strike, defend, bash]

        assert combat.resolve_pending_choice(0)
        assert combat.resolve_pending_choice(2)
        assert combat.resolve_pending_choice(None)
        assert combat.pending_choice is None
        assert strike in combat.discard_pile
        assert bash in combat.discard_pile
        assert defend in combat.hand

        shivs = [card for card in combat.hand if card.card_id.name == "SHIV"]
        assert len(shivs) == 2

    def test_the_hunt_adds_extra_card_reward_and_power_on_fatal_kill(self):
        """Matches TheHunt.cs: fatal kill adds extra card reward and applies TheHuntPower."""
        combat = _make_combat()
        combat.start_combat()
        combat.room = create_room(RoomType.ELITE)
        enemy = combat.enemies[0]
        enemy.current_hp = 8
        enemy.max_hp = 8
        combat.hand = [make_the_hunt()]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.is_dead
        assert combat.player.get_power_amount(PowerId.THE_HUNT) == 1

        rewards = combat.room.extra_rewards[combat.player_id]
        assert len(rewards) == 1
        assert isinstance(rewards[0], CardReward)
        assert rewards[0].context == "elite"

    def test_the_hunt_uses_fallback_extra_reward_counter_without_room(self):
        """Matches TheHunt.cs fatal semantics even when only the combat fallback counter is available."""
        combat = _make_combat()
        combat.start_combat()
        combat.room = None
        enemy = combat.enemies[0]
        enemy.current_hp = 8
        enemy.max_hp = 8
        combat.hand = [make_the_hunt()]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.is_dead
        assert combat.extra_card_rewards == 1
        assert combat.player.get_power_amount(PowerId.THE_HUNT) == 1
