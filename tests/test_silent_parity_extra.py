"""Focused parity tests for additional Silent card flows."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.silent import (
    make_backstab,
    make_blade_dance,
    make_burst,
    make_defend_silent,
    make_escape_plan,
    make_strike_silent,
    make_wraith_form,
)
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import CardId, PowerId
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle


def _make_combat(deck: list | None = None) -> CombatState:
    combat = CombatState(
        player_hp=70,
        player_max_hp=70,
        deck=deck or [make_strike_silent() for _ in range(12)],
        rng_seed=42,
        character_id="Silent",
    )
    creature, ai = create_shrinker_beetle(Rng(42))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


class TestSilentParityExtra:
    def test_backstab_is_innate_and_exhausts_after_play(self):
        """Matches Backstab.cs: Innate+Exhaust attack that should appear in opening hand."""
        deck = [make_backstab()] + [make_strike_silent() for _ in range(7)]
        combat = _make_combat(deck=deck)
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp

        backstab_index = next(i for i, card in enumerate(combat.hand) if card.card_id == CardId.BACKSTAB)
        assert combat.play_card(backstab_index, 0)

        assert enemy.current_hp == starting_hp - 11
        assert all(card.card_id != CardId.BACKSTAB for card in combat.hand)
        assert any(card.card_id == CardId.BACKSTAB for card in combat.exhaust_pile)

    def test_blade_dance_creates_three_shivs(self):
        """Matches BladeDance.cs: create configured number of Shiv cards in hand."""
        combat = _make_combat()
        combat.hand = [make_blade_dance()]
        combat.energy = 1

        assert combat.play_card(0)

        shivs = [card for card in combat.hand if card.card_id == CardId.SHIV]
        assert len(shivs) == 3
        assert all(card.cost == 0 for card in shivs)

    def test_burst_replays_next_skill_once_then_is_consumed(self):
        """Matches Burst.cs + BurstPower: next skill plays an additional time, then decrements."""
        combat = _make_combat()
        combat.hand = [make_burst(), make_blade_dance()]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.BURST) == 1

        assert combat.play_card(0)
        shivs = [card for card in combat.hand if card.card_id == CardId.SHIV]
        assert len(shivs) == 6
        assert combat.player.get_power_amount(PowerId.BURST) == 0

    def test_escape_plan_only_gains_block_when_the_drawn_card_is_a_skill(self):
        """Matches EscapePlan.cs: draw first, gain block only if the drawn card is a Skill."""
        combat = _make_combat()
        attack = make_strike_silent()
        combat.hand = [make_escape_plan()]
        combat.draw_pile = [attack]
        combat.energy = 0

        assert combat.play_card(0)
        assert combat.player.block == 0
        assert attack in combat.hand

        combat = _make_combat()
        skill = make_defend_silent()
        combat.hand = [make_escape_plan()]
        combat.draw_pile = [skill]
        combat.energy = 0

        assert combat.play_card(0)
        assert combat.player.block == 3
        assert skill in combat.hand

    def test_wraith_form_applies_intangible_and_wraith_form_power_then_drains_dex_each_turn(self):
        """Matches WraithForm.cs + WraithFormPower: apply both powers, then lose Dexterity each turn."""
        combat = _make_combat()
        combat.hand = [make_wraith_form()]
        combat.energy = 3

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.INTANGIBLE) == 2
        assert combat.player.get_power_amount(PowerId.WRAITH_FORM) == 1

        combat.end_player_turn()

        assert combat.current_side.name == "PLAYER"
        assert combat.player.get_power_amount(PowerId.DEXTERITY) == -1
