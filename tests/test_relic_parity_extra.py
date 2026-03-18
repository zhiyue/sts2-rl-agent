"""Focused parity tests for high-priority uncovered relics."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.colorless import make_volley
from sts2_env.cards.defect import create_defect_starter_deck
from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import PowerId
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle, create_twig_slime_s


def _make_ironclad_combat(
    relics: list[str] | None = None,
    *,
    enemy_factory=create_shrinker_beetle,
    seed: int = 42,
) -> CombatState:
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=seed,
        character_id="Ironclad",
        relics=relics or [],
    )
    creature, ai = enemy_factory(Rng(seed))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


def _make_defect_combat(
    relics: list[str] | None = None,
    *,
    seed: int = 42,
) -> CombatState:
    combat = CombatState(
        player_hp=75,
        player_max_hp=75,
        deck=create_defect_starter_deck(),
        rng_seed=seed,
        character_id="Defect",
        relics=relics or [],
    )
    creature, ai = create_shrinker_beetle(Rng(seed))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


class TestRelicParityExtra:
    def test_akabeko_applies_vigor_once_on_round_one(self):
        """Matches Akabeko.cs: round-1 start grants 8 Vigor exactly once."""
        combat = _make_ironclad_combat(["Akabeko"])
        assert combat.player.get_power_amount(PowerId.VIGOR) == 8

        combat.end_player_turn()

        assert combat.round_number == 2
        assert combat.player.get_power_amount(PowerId.VIGOR) == 8

    def test_bag_of_marbles_applies_vulnerable_to_all_enemies(self):
        """Matches BagOfMarbles.cs: round-1 start applies Vulnerable to each enemy."""
        combat = CombatState(
            player_hp=80,
            player_max_hp=80,
            deck=create_ironclad_starter_deck(),
            rng_seed=43,
            character_id="Ironclad",
            relics=["BagOfMarbles"],
        )
        enemy_a, ai_a = create_shrinker_beetle(Rng(43))
        enemy_b, ai_b = create_twig_slime_s(Rng(44))
        combat.add_enemy(enemy_a, ai_a)
        combat.add_enemy(enemy_b, ai_b)
        combat.start_combat()

        assert all(enemy.get_power_amount(PowerId.VULNERABLE) == 1 for enemy in combat.enemies)

    def test_bronze_scales_reflects_damage_with_thorns_on_enemy_attack(self):
        """Matches BronzeScales.cs + ThornsPower.cs: thorn damage is returned on hit."""
        combat = _make_ironclad_combat(["BronzeScales"], enemy_factory=create_twig_slime_s, seed=77)
        enemy = combat.enemies[0]
        start_enemy_hp = enemy.current_hp

        assert combat.player.get_power_amount(PowerId.THORNS) == 3
        combat.end_player_turn()

        assert enemy.current_hp == start_enemy_hp - 3

    def test_data_disk_grants_focus_at_combat_start(self):
        """Matches DataDisk.cs: apply Focus(1) before opening turn."""
        combat = _make_defect_combat(["DataDisk"])
        assert combat.player.get_power_amount(PowerId.FOCUS) == 1

    def test_chemical_x_increases_x_value_for_x_cost_cards(self):
        """Matches ChemicalX.cs: X value receives +2 via ModifyXValue hook."""
        combat = _make_ironclad_combat(["ChemicalX"], seed=123)
        enemy = combat.enemies[0]
        enemy.max_hp = 200
        enemy.current_hp = 200
        combat.hand = [make_volley()]
        combat.energy = 3

        assert combat.play_card(0)
        assert combat.energy == 0
        assert enemy.current_hp == 150
