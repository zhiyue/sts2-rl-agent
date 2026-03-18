"""Additional Ironclad parity tests for remaining high-signal cards."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.ironclad import (
    create_ironclad_starter_deck,
    make_bash,
    make_bloodletting,
    make_dark_embrace,
    make_defend_ironclad,
    make_havoc,
    make_inflame,
    make_juggernaut,
    make_offering,
    make_rupture,
    make_second_wind,
    make_shrug_it_off,
    make_strike_ironclad,
)
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import PowerId
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle


def _make_combat() -> CombatState:
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=126,
        character_id="Ironclad",
    )
    creature, ai = create_shrinker_beetle(Rng(126))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


class TestIroncladParityExtra3:
    def test_bash_plus_deals_upgraded_damage_and_applies_upgraded_vulnerable(self):
        """Matches Bash.cs: deal 10 then apply 3 Vulnerable when upgraded."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        start_hp = enemy.current_hp
        combat.hand = [make_bash(upgraded=True)]
        combat.energy = 2

        assert combat.play_card(0, 0)
        assert enemy.current_hp == start_hp - 10
        assert enemy.get_power_amount(PowerId.VULNERABLE) == 3

    def test_havoc_auto_plays_top_draw_card_and_force_exhausts_it(self):
        """Matches Havoc.cs: auto-play top draw card with force-exhaust semantics."""
        combat = _make_combat()
        defend = make_defend_ironclad()
        combat.hand = [make_havoc()]
        combat.draw_pile = [defend]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.block == 5
        assert defend in combat.exhaust_pile
        assert defend not in combat.discard_pile

    def test_shrug_it_off_gains_block_then_draws_one_card(self):
        """Matches ShrugItOff.cs: gain 8 block and draw 1."""
        combat = _make_combat()
        drawn = make_inflame()
        combat.hand = [make_shrug_it_off()]
        combat.draw_pile = [drawn]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.block == 8
        assert combat.hand == [drawn]

    def test_offering_is_unblockable_self_hp_loss_plus_energy_and_draw(self):
        """Matches Offering.cs: lose 6 HP unblockable, gain 2 energy, draw 3, exhaust."""
        combat = _make_combat()
        start_hp = combat.player.current_hp
        draw_a = make_strike_ironclad()
        draw_b = make_defend_ironclad()
        draw_c = make_inflame()
        offering = make_offering()
        combat.player.gain_block(30)
        combat.hand = [offering]
        combat.draw_pile = [draw_a, draw_b, draw_c]
        combat.energy = 0

        assert combat.play_card(0)
        assert combat.player.current_hp == start_hp - 6
        assert combat.player.block == 30
        assert combat.energy == 2
        assert len(combat.hand) == 3
        assert draw_a in combat.hand
        assert draw_b in combat.hand
        assert draw_c in combat.hand
        assert offering in combat.exhaust_pile

    def test_second_wind_exhausts_non_attacks_and_grants_block_per_card(self):
        """Matches SecondWind.cs: exhaust all non-attacks in hand; gain block per exhaust."""
        combat = _make_combat()
        strike = make_strike_ironclad()
        defend = make_defend_ironclad()
        inflame = make_inflame()
        combat.hand = [make_second_wind(), strike, defend, inflame]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.block == 10
        assert strike in combat.hand
        assert defend in combat.exhaust_pile
        assert inflame in combat.exhaust_pile

    def test_dark_embrace_draws_for_each_second_wind_exhaust(self):
        """Matches DarkEmbracePower + SecondWind: each owner exhaust draws 1 card."""
        combat = _make_combat()
        draw_a = make_bash()
        draw_b = make_shrug_it_off()
        strike = make_strike_ironclad()
        defend = make_defend_ironclad()
        inflame = make_inflame()
        combat.hand = [make_dark_embrace(), make_second_wind(), strike, defend, inflame]
        combat.draw_pile = [draw_a, draw_b]
        combat.energy = 3

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.DARK_EMBRACE) == 1
        assert combat.play_card(0)
        assert draw_a in combat.hand
        assert draw_b in combat.hand
        assert defend in combat.exhaust_pile
        assert inflame in combat.exhaust_pile

    def test_juggernaut_deals_damage_when_block_is_gained(self):
        """Matches JuggernautPower.cs: after owner gains block, deal power amount damage."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        start_hp = enemy.current_hp
        combat.hand = [make_juggernaut(), make_shrug_it_off()]
        combat.energy = 3

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.JUGGERNAUT) == 5
        assert combat.play_card(0)
        assert combat.player.block == 8
        assert enemy.current_hp == start_hp - 5

    def test_rupture_gains_strength_when_owner_loses_hp(self):
        """Matches RupturePower.cs: owner HP loss on own turn grants Strength."""
        combat = _make_combat()
        start_hp = combat.player.current_hp
        combat.player.gain_block(12)
        combat.hand = [make_rupture(), make_bloodletting()]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.RUPTURE) == 1
        assert combat.play_card(0)
        assert combat.player.current_hp == start_hp - 3
        assert combat.player.block == 12
        assert combat.player.get_power_amount(PowerId.STRENGTH) == 1
        assert combat.energy == 2
