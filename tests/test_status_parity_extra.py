"""Focused parity tests for status/curse cards backed by decompiled models."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.cards.ironclad_basic import make_strike_ironclad
from sts2_env.cards.status import make_burn, make_debt, make_frantic_escape, make_regret, make_void
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import PowerId
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle
from sts2_env.powers.remaining_c import SandpitPower


def _make_combat(gold: int = 0) -> CombatState:
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=42,
        character_id="Ironclad",
        gold=gold,
    )
    creature, ai = create_shrinker_beetle(Rng(42))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


class TestStatusParityExtra:
    def test_burn_deals_turn_end_in_hand_damage_then_discards(self):
        """Matches Burn.cs: in-hand turn-end burn deals 2 and follows turn cleanup."""
        combat = _make_combat()
        burn = make_burn()
        combat.hand = [burn]
        starting_hp = combat.player.current_hp

        combat.end_player_turn()

        assert combat.player.current_hp == starting_hp - 2
        assert burn in combat.discard_pile

    def test_void_loses_energy_on_draw(self):
        """Matches Void.cs: drawing Void immediately loses configured energy."""
        combat = _make_combat()
        combat.hand.clear()
        void = make_void()
        combat.draw_pile = [void]
        combat.energy = 3

        combat.draw_cards(combat.player, 1)

        assert combat.energy == 2
        assert void in combat.hand

    def test_regret_uses_pre_flush_hand_size_for_hp_loss(self):
        """Matches Regret.cs: hp loss equals hand size at turn end before flush."""
        combat = _make_combat()
        regret = make_regret()
        filler_a = make_strike_ironclad()
        filler_b = make_strike_ironclad()
        combat.hand = [regret, filler_a, filler_b]
        starting_hp = combat.player.current_hp

        combat.end_player_turn()

        assert combat.player.current_hp == starting_hp - 3

    def test_debt_loses_up_to_ten_gold_without_going_negative(self):
        """Matches Debt.cs: turn-end gold loss is clamped by current gold."""
        combat = _make_combat(gold=7)
        combat.hand = [make_debt()]

        combat.end_player_turn()

        assert combat.gold == 0

    def test_frantic_escape_increases_matching_sandpit_and_its_own_cost_per_play(self):
        """Matches FranticEscape.cs: increment matching Sandpit then increase own cost."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        sandpit = SandpitPower(4)
        sandpit.target = combat.player
        enemy.powers[PowerId.SANDPIT] = sandpit

        frantic = make_frantic_escape()
        combat.hand = [frantic]
        combat.energy = 10

        assert combat.play_card(0)
        assert sandpit.amount == 5
        assert frantic.cost == 2

        # Replay the same instance to validate accumulating combat-time cost increase.
        combat.hand = [frantic]
        assert combat.play_card(0)
        assert sandpit.amount == 6
        assert frantic.cost == 3

