"""Combat flow parity tests against decompiled turn/relic semantics."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.ironclad_basic import create_ironclad_starter_deck
from sts2_env.cards.ironclad_basic import make_strike_ironclad
from sts2_env.cards.silent import make_bullet_time, make_well_laid_plans
from sts2_env.cards.status import make_burn, make_debt, make_regret, make_void
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import CombatSide, PowerId
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle


def _make_combat(relics: list[str] | None = None, gold: int = 0) -> CombatState:
    rng = Rng(42)
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=42,
        relics=relics or [],
        gold=gold,
    )
    creature, ai = create_shrinker_beetle(rng)
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


class TestRelicTurnHooks:
    def test_anchor_grants_block_before_first_turn(self):
        combat = _make_combat(["Anchor"])
        assert combat.player.block == 10

    def test_ring_of_the_snake_modifies_round_1_hand_draw(self):
        combat = _make_combat(["RingOfTheSnake"])
        assert len(combat.hand) == 7

    def test_lantern_adds_energy_after_round_1_reset(self):
        combat = _make_combat(["Lantern"])
        assert combat.energy == 4

    def test_ice_cream_uses_add_max_energy_instead_of_reset(self):
        combat = _make_combat(["IceCream"])
        combat.energy = 1
        combat.end_player_turn()
        assert combat.round_number == 2
        assert combat.energy == 4

    def test_runic_pyramid_prevents_hand_flush(self):
        combat = _make_combat(["RunicPyramid"])
        retained = combat.hand[0]
        combat.end_player_turn()
        assert retained in combat.hand

    def test_bookmark_reduces_cost_of_single_turn_retained_card(self):
        combat = _make_combat(["Bookmark"])
        retained = make_strike_ironclad()
        retained.keywords = frozenset({"retain"})
        combat.hand = [retained]

        combat.end_player_turn()

        assert retained in combat.hand
        assert retained.cost == 0


class TestPowerTurnHooks:
    def test_retain_hand_power_retains_current_hand(self):
        combat = _make_combat()
        retained = combat.hand[0]
        combat.apply_power_to(combat.player, PowerId.RETAIN_HAND, 1)
        combat.end_player_turn()
        assert retained in combat.hand

    def test_automation_triggers_from_draw_pipeline(self):
        combat = _make_combat()
        combat.player.apply_power(PowerId.AUTOMATION, 2)
        combat.discard_pile.extend(combat.hand)
        combat.hand.clear()

        initial_energy = combat.energy
        combat.draw_cards(combat.player, 10)

        assert combat.energy == initial_energy + 2

    def test_no_draw_blocks_non_hand_draw(self):
        combat = _make_combat()
        combat.discard_pile.extend(combat.hand)
        combat.hand.clear()
        combat.player.apply_power(PowerId.NO_DRAW, 1)

        combat.draw_cards(combat.player, 1)

        assert len(combat.hand) == 0

    def test_no_draw_does_not_block_opening_hand_draw(self):
        rng = Rng(42)
        combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=create_ironclad_starter_deck(),
            rng_seed=42,
        )
        combat.player.apply_power(PowerId.NO_DRAW, 1)
        creature, ai = create_shrinker_beetle(rng)
        combat.add_enemy(creature, ai)

        combat.start_combat()

        assert len(combat.hand) == 5

    def test_duplication_replays_full_card_play_hooks(self):
        combat = _make_combat()
        strike = make_strike_ironclad()
        combat.hand = [strike]
        combat.energy = 1
        combat.player.apply_power(PowerId.RAGE, 3)
        combat.player.apply_power(PowerId.DUPLICATION, 1)

        combat.play_card(0, 0)

        assert combat.player.block == 6


class TestCardCleanup:
    def test_bullet_time_temporary_cost_resets_at_end_of_turn(self):
        combat = _make_combat()
        bullet_time = make_bullet_time()
        strike = make_strike_ironclad()
        combat.hand = [bullet_time, strike]
        combat.energy = 3

        assert strike.cost == strike.original_cost == 1
        combat.play_card(0)
        assert strike.cost == 0

        combat.end_player_turn()
        assert strike.cost == 1

    def test_well_laid_plans_applies_correct_power(self):
        combat = _make_combat()
        well_laid_plans = make_well_laid_plans()
        combat.hand = [well_laid_plans]
        combat.energy = 1

        combat.play_card(0)

        assert combat.player.has_power(PowerId.WELL_LAID_PLANS)

    def test_burn_triggers_turn_end_in_hand_damage(self):
        combat = _make_combat()
        burn = make_burn()
        combat.hand = [burn]
        starting_hp = combat.player.current_hp

        combat.end_player_turn()

        assert combat.player.current_hp == starting_hp - 2
        assert burn in combat.discard_pile

    def test_regret_uses_pre_flush_hand_size_for_damage(self):
        combat = _make_combat()
        regret = make_regret()
        filler = make_strike_ironclad()
        combat.hand = [regret, filler]
        starting_hp = combat.player.current_hp

        combat.end_player_turn()

        assert combat.player.current_hp == starting_hp - 2

    def test_debt_triggers_turn_end_gold_loss(self):
        combat = _make_combat(gold=20)
        debt = make_debt()
        combat.hand = [debt]

        combat.end_player_turn()

        assert combat.gold == 10

    def test_void_loses_energy_when_drawn(self):
        combat = _make_combat()
        combat.hand.clear()
        combat.draw_pile = [make_void()]
        combat.energy = 3

        combat.draw_cards(combat.player, 1)

        assert combat.energy == 2


class TestExtraTurn:
    def test_paels_eye_skips_enemy_turn_and_grants_extra_turn(self):
        combat = _make_combat(["PaelsEye"])
        enemy_ai = combat.enemy_ais[0]

        combat.end_player_turn()

        assert combat.current_side == CombatSide.PLAYER
        assert combat.round_number == 2
        assert enemy_ai.current_move.state_id == "SHRINK"

    def test_paels_eye_exhausts_cards_through_exhaust_hooks(self):
        combat = _make_combat(["PaelsEye", "CharonsAshes"])
        enemy = combat.enemies[0]
        starting_hp = enemy.current_hp

        combat.end_player_turn()

        assert enemy.current_hp == starting_hp - 15


class TestEnemyTurnStart:
    def test_enemy_block_clears_on_first_enemy_turn(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.gain_block(7)

        combat.end_player_turn()

        assert enemy.block == 0
