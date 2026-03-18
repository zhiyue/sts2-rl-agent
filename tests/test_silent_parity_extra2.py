"""Additional high-signal parity tests for remaining Silent cards."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.silent import (
    make_backflip,
    make_calculated_gamble,
    make_defend_silent,
    make_footwork,
    make_leg_sweep,
    make_piercing_wail,
    make_reflex,
    make_strike_silent,
    make_tools_of_the_trade,
    make_well_laid_plans,
)
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import CombatSide, PowerId
from sts2_env.core.hooks import fire_after_turn_end, fire_before_turn_end
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle


def _make_combat(deck: list | None = None, *, extra_enemies: int = 0) -> CombatState:
    combat = CombatState(
        player_hp=70,
        player_max_hp=70,
        deck=deck or [make_strike_silent() for _ in range(15)],
        rng_seed=42,
        character_id="Silent",
    )
    creature, ai = create_shrinker_beetle(Rng(42))
    combat.add_enemy(creature, ai)
    for i in range(extra_enemies):
        extra_creature, extra_ai = create_shrinker_beetle(Rng(100 + i))
        combat.add_enemy(extra_creature, extra_ai)
    combat.start_combat()
    return combat


class TestSilentParityExtra2:
    def test_backflip_gains_block_then_draws_two_cards(self):
        combat = _make_combat()
        first_draw = make_strike_silent()
        second_draw = make_defend_silent()
        combat.hand = [make_backflip()]
        combat.draw_pile = [first_draw, second_draw]
        combat.discard_pile = []
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.block == 5
        assert combat.hand == [first_draw, second_draw]

    def test_footwork_applies_dexterity_and_scales_next_block_card(self):
        combat = _make_combat()
        combat.hand = [make_footwork(), make_defend_silent()]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.DEXTERITY) == 2
        assert combat.play_card(0)
        assert combat.player.block == 7

    def test_leg_sweep_gains_block_and_applies_weak_to_target(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        combat.hand = [make_leg_sweep()]
        combat.energy = 2

        assert combat.play_card(0, 0)
        assert combat.player.block == 11
        assert enemy.get_power_amount(PowerId.WEAK) == 2

    def test_piercing_wail_applies_temporary_strength_loss_to_all_enemies(self):
        combat = _make_combat(extra_enemies=1)
        for enemy in combat.enemies:
            enemy.apply_power(PowerId.STRENGTH, 2)
        combat.hand = [make_piercing_wail()]
        combat.energy = 1

        assert combat.play_card(0)
        for enemy in combat.enemies:
            assert enemy.get_power_amount(PowerId.STRENGTH) == -4
            assert enemy.get_power_amount(PowerId.PIERCING_WAIL) == 6

        fire_after_turn_end(CombatSide.ENEMY, combat)
        for enemy in combat.enemies:
            assert enemy.get_power_amount(PowerId.STRENGTH) == 2
            assert enemy.get_power_amount(PowerId.PIERCING_WAIL) == 0

    def test_tools_of_the_trade_draws_extra_then_discards_one_at_turn_start(self):
        combat = _make_combat()
        scripted_draw = [make_strike_silent() for _ in range(6)]
        scripted_ids = {id(card) for card in scripted_draw}

        combat.hand = [make_tools_of_the_trade()]
        combat.draw_pile = list(scripted_draw)
        combat.discard_pile = []
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.TOOLS_OF_THE_TRADE) == 1

        combat.end_player_turn()

        assert combat.round_number == 2
        assert len(combat.hand) == 5
        assert len(combat.discard_pile) == 1
        assert id(combat.discard_pile[0]) in scripted_ids
        seen_scripted = sum(1 for card in combat.hand + combat.discard_pile if id(card) in scripted_ids)
        assert seen_scripted == 6

    def test_well_laid_plans_retains_one_card_during_end_of_turn_flush(self):
        combat = _make_combat()
        keep = make_defend_silent()
        toss = make_strike_silent()
        combat.hand = [make_well_laid_plans(), keep, toss]
        combat.draw_pile = []
        combat.discard_pile = []
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.WELL_LAID_PLANS) == 1

        fire_before_turn_end(CombatSide.PLAYER, combat)
        combat._resolve_end_of_turn_hand()  # noqa: SLF001

        assert combat.hand == [keep]
        assert combat.discard_pile == [toss]

    def test_reflex_draws_two_cards_when_played(self):
        combat = _make_combat()
        first_draw = make_strike_silent()
        second_draw = make_defend_silent()
        combat.hand = [make_reflex()]
        combat.draw_pile = [first_draw, second_draw]
        combat.discard_pile = []
        combat.energy = 3

        assert combat.play_card(0)
        assert combat.hand == [first_draw, second_draw]

    def test_calculated_gamble_discards_hand_draws_same_count_and_triggers_reflex(self):
        combat = _make_combat()
        reflex = make_reflex()
        discard_target = make_strike_silent()
        draw_a = make_defend_silent()
        draw_b = make_strike_silent()
        draw_c = make_defend_silent()
        draw_d = make_strike_silent()
        combat.hand = [make_calculated_gamble(), reflex, discard_target]
        combat.draw_pile = [draw_a, draw_b, draw_c, draw_d]
        combat.discard_pile = []
        combat.energy = 0

        assert combat.play_card(0)
        assert reflex in combat.discard_pile
        assert discard_target in combat.discard_pile
        assert combat.hand == [draw_a, draw_b, draw_c, draw_d]
