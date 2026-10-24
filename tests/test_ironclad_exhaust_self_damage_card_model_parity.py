"""Ironclad card-model parity tests for exhaust and self-damage mechanics."""

import random

import sts2_env.powers  # noqa: F401

from sts2_env.cards.ironclad import (
    create_ironclad_starter_deck,
    make_bash,
    make_bloodletting,
    make_dark_embrace,
    make_defend_ironclad,
    make_havoc,
    make_hemokinesis,
    make_inflame,
    make_juggernaut,
    make_mangle,
    make_offering,
    make_rupture,
    make_second_wind,
    make_shrug_it_off,
    make_strike_ironclad,
)
import sts2_env.cards.registry as card_registry
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import PowerId, ValueProp
from sts2_env.core.hooks import fire_after_block_gained
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle
from sts2_env.powers.base import PowerInstance
from sts2_env.relics.registry import create_relic_by_name


class _CannotHitPower(PowerInstance):
    def __init__(self):
        super().__init__(PowerId.COVERED, 1)

    def should_allow_hitting(self, owner, combat):
        return False


def _make_combat(*, extra_enemies: int = 0) -> CombatState:
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=126,
        character_id="Ironclad",
    )
    creature, ai = create_shrinker_beetle(Rng(126))
    combat.add_enemy(creature, ai)
    for i in range(extra_enemies):
        extra_creature, extra_ai = create_shrinker_beetle(Rng(200 + i))
        combat.add_enemy(extra_creature, extra_ai)
    combat.start_combat()
    return combat


class TestIroncladExhaustSelfDamageCardModelParity:
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

    def test_havoc_moves_top_draw_card_to_play_before_autoplaying(self, monkeypatch):
        """Matches AutoPlayFromDrawPile: selected cards move to Play before AutoPlay."""
        combat = _make_combat()
        probe = make_mangle()

        def record_autoplay_pile_state(card, combat_state, target) -> None:
            card.combat_vars["was_in_draw_during_play"] = card in combat_state.draw_pile
            card.combat_vars["was_in_play_during_play"] = card in combat_state.play_pile

        monkeypatch.setitem(card_registry._CARD_EFFECTS, probe.card_id, record_autoplay_pile_state)  # noqa: SLF001
        combat.hand = [make_havoc()]
        combat.draw_pile = [probe]
        combat.energy = 1

        assert combat.play_card(0)
        assert probe.combat_vars["was_in_draw_during_play"] is False
        assert probe.combat_vars["was_in_play_during_play"] is True
        assert probe in combat.exhaust_pile

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

    def test_second_wind_unmovable_counts_same_card_play_once(self):
        combat = _make_combat()
        strike = make_strike_ironclad()
        defend = make_defend_ironclad()
        inflame = make_inflame()
        combat.hand = [make_second_wind(), strike, defend, inflame]
        combat.energy = 1
        combat.player.apply_power(PowerId.UNMOVABLE, 1)

        assert combat.play_card(0)

        assert combat.player.block == 20

    def test_unmovable_stops_doubling_after_prior_card_play_block(self):
        combat = _make_combat()
        defend = make_defend_ironclad()
        inflame = make_inflame()
        combat.hand = [make_second_wind(), defend, inflame]
        combat.energy = 1
        combat.player.apply_power(PowerId.UNMOVABLE, 1)
        combat.player.gain_block(5)
        fire_after_block_gained(combat.player, 5, combat, ValueProp.MOVE, object())

        assert combat.play_card(0)

        assert combat.player.block == 15

    def test_second_wind_stops_after_exhaust_ends_combat(self):
        combat = _make_combat()
        combat.relics.append(create_relic_by_name("CharonsAshes"))
        enemy = combat.enemies[0]
        enemy.current_hp = 3
        defend = make_defend_ironclad()
        inflame = make_inflame()
        combat.hand = [make_second_wind(), defend, inflame]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.is_over
        assert combat.player.block == 0
        assert defend in combat.exhaust_pile
        assert inflame in combat.hand

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
        # Juggernaut.cs: PowerVar<JuggernautPower>(6); OnUpgrade +2.
        assert combat.player.get_power_amount(PowerId.JUGGERNAUT) == 6
        assert combat.play_card(0)
        assert combat.player.block == 8
        assert enemy.current_hp == start_hp - 6

    def test_juggernaut_random_target_uses_only_hittable_enemies(self):
        random.seed(1)
        combat = _make_combat(extra_enemies=1)
        blocked, hittable = combat.enemies
        blocked.current_hp = blocked.max_hp = 100
        hittable.current_hp = hittable.max_hp = 100
        blocked.powers[PowerId.COVERED] = _CannotHitPower()
        combat.hand = [make_juggernaut(), make_shrug_it_off()]
        combat.energy = 3

        assert combat.play_card(0)
        assert combat.play_card(0)

        assert blocked.current_hp == 100
        # Juggernaut base amount 6 per Juggernaut.cs.
        assert hittable.current_hp == 94

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

    def test_hemokinesis_loses_hp_before_damage_but_rupture_pays_out_after_card(self):
        """Matches Hemokinesis.cs + RupturePower.cs ordering."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.max_hp = 100
        enemy.current_hp = 100
        start_hp = combat.player.current_hp
        combat.hand = [make_rupture(), make_hemokinesis()]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.play_card(0, 0)
        assert combat.player.current_hp == start_hp - 2
        # Hemokinesis.cs: DamageVar(15).
        assert enemy.current_hp == 85
        assert combat.player.get_power_amount(PowerId.STRENGTH) == 1
        assert [event[1] for event in combat._damage_events_combat[-2:]] == [combat.player, enemy]  # noqa: SLF001

    def test_hemokinesis_skips_attack_if_self_damage_kills_attacker(self):
        """Matches AttackCommand.cs: a dead attacker cannot execute the attack."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = enemy.max_hp = 100
        combat.player.current_hp = 1
        combat.hand = [make_hemokinesis()]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert combat.player.current_hp == 0
        assert enemy.current_hp == 100
        assert combat.is_over
        assert combat.player_won is False
