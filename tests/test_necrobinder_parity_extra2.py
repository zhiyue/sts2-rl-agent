"""Additional high-signal Necrobinder parity tests."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.necrobinder import (
    create_necrobinder_starter_deck,
    make_defile,
    make_eradicate,
    make_fear,
    make_no_escape,
    make_putrefy,
    make_reap,
    make_sic_em,
    make_the_scythe,
)
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import CardId, PowerId
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle


def _make_combat() -> CombatState:
    combat = CombatState(
        player_hp=70,
        player_max_hp=70,
        deck=create_necrobinder_starter_deck(),
        rng_seed=42,
        character_id="Necrobinder",
    )
    creature, ai = create_shrinker_beetle(Rng(42))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


class TestNecrobinderParityExtra2:
    def test_defile_upgraded_deals_expected_damage(self):
        """Matches Defile.cs: pure single-target attack, upgraded by +4 damage."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = 100
        enemy.max_hp = 100
        combat.hand = [make_defile(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == 83

    def test_fear_upgraded_deals_damage_and_applies_vulnerable(self):
        """Matches Fear.cs: attack target and apply Vulnerable."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = 100
        enemy.max_hp = 100
        combat.hand = [make_fear(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == 92
        assert enemy.get_power_amount(PowerId.VULNERABLE) == 2

    def test_reap_retain_keyword_keeps_card_in_hand_across_turn(self):
        """Matches Reap.cs keyword behavior: card has Retain."""
        combat = _make_combat()
        combat.hand = [make_reap()]
        combat.energy = 0

        combat.end_player_turn()
        assert any(card.card_id == CardId.REAP for card in combat.hand)

    def test_no_escape_upgraded_scales_doom_with_existing_stacks(self):
        """Matches NoEscape.cs: applied Doom = base + extra * floor(existing / threshold)."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        combat.apply_power_to(enemy, PowerId.DOOM, 29)
        combat.hand = [make_no_escape(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.get_power_amount(PowerId.DOOM) == 54

    def test_putrefy_upgraded_applies_weak_and_vulnerable(self):
        """Matches Putrefy.cs: apply both debuffs for the card's Power amount."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        combat.hand = [make_putrefy(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.get_power_amount(PowerId.WEAK) == 3
        assert enemy.get_power_amount(PowerId.VULNERABLE) == 3

    def test_eradicate_hits_x_times_and_spends_all_energy(self):
        """Matches Eradicate.cs: X-cost attack repeats once per energy spent."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = 100
        enemy.max_hp = 100
        card = make_eradicate()
        combat.hand = [card]
        combat.energy = 3

        assert combat.play_card(0, 0)
        assert card.energy_spent == 3
        assert combat.energy == 0
        assert enemy.current_hp == 67

    def test_sic_em_upgraded_applies_upgraded_sic_em_power_amount(self):
        """Matches SicEm.cs: apply SicEm debuff by its upgraded stack amount."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = 100
        enemy.max_hp = 100
        combat.summon_osty(combat.player, 5)
        combat.hand = [make_sic_em(upgraded=True)]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.current_hp == 94
        assert enemy.get_power_amount(PowerId.SIC_EM) == 3

    def test_the_scythe_damage_increases_after_each_play(self):
        """Matches TheScythe.cs: card permanently gains damage each time it is played."""
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.current_hp = 200
        enemy.max_hp = 200
        card = make_the_scythe()
        combat.hand = [card]
        combat.energy = 2

        assert combat.play_card(0, 0)
        assert enemy.current_hp == 187
        assert card.base_damage == 16
        assert card in combat.exhaust_pile

        combat.exhaust_pile.remove(card)
        combat.hand = [card]
        combat.energy = 2
        assert combat.play_card(0, 0)
        assert enemy.current_hp == 171
        assert card.base_damage == 19
