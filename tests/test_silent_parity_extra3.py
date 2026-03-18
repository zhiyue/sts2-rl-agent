"""Additional high-signal parity tests for remaining Silent cards."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.silent import (
    make_blade_dance,
    make_deadly_poison,
    make_defend_silent,
    make_dodge_and_roll,
    make_expertise,
    make_malaise,
    make_noxious_fumes,
    make_phantom_blades,
    make_strike_silent,
    make_tactician,
    make_tracking,
)
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import CardId, CombatSide, PowerId
from sts2_env.core.hooks import fire_after_side_turn_start
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle


def _make_combat(deck: list | None = None, *, extra_enemies: int = 0) -> CombatState:
    combat = CombatState(
        player_hp=70,
        player_max_hp=70,
        deck=deck or [make_strike_silent() for _ in range(12)],
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


class TestSilentParityExtra3:
    def test_dodge_and_roll_applies_block_now_and_next_turn_with_current_block_scaling(self):
        combat = _make_combat()
        combat.player.apply_power(PowerId.DEXTERITY, 2)
        combat.hand = [make_dodge_and_roll()]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.block == 6

        combat.player.block = 0
        fire_after_side_turn_start(CombatSide.PLAYER, combat)

        assert combat.player.block == 6
        assert combat.player.get_power_amount(PowerId.BLOCK_NEXT_TURN) == 0

    def test_deadly_poison_applies_poison_and_ticks_on_enemy_turn_start(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        combat.hand = [make_deadly_poison()]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.get_power_amount(PowerId.POISON) == 5

        hp_before_tick = enemy.current_hp
        fire_after_side_turn_start(CombatSide.ENEMY, combat)

        assert enemy.current_hp == hp_before_tick - 5
        assert enemy.get_power_amount(PowerId.POISON) == 4

    def test_expertise_draws_up_to_six_and_does_not_overdraw_when_hand_is_full_enough(self):
        combat = _make_combat()
        kept_attack = make_strike_silent()
        kept_skill = make_defend_silent()
        draw_1 = make_strike_silent()
        draw_2 = make_defend_silent()
        draw_3 = make_strike_silent()
        draw_4 = make_defend_silent()
        draw_5 = make_strike_silent()
        combat.hand = [make_expertise(), kept_attack, kept_skill]
        combat.draw_pile = [draw_1, draw_2, draw_3, draw_4, draw_5]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.hand == [kept_attack, kept_skill, draw_1, draw_2, draw_3, draw_4]
        assert combat.draw_pile == [draw_5]

        combat = _make_combat()
        combat.hand = [make_expertise()] + [make_strike_silent() for _ in range(6)]
        combat.draw_pile = [make_defend_silent()]
        combat.energy = 1

        assert combat.play_card(0)
        assert len(combat.hand) == 6
        assert len(combat.draw_pile) == 1

    def test_noxious_fumes_applies_poison_to_all_enemies_at_player_turn_start(self):
        combat = _make_combat(extra_enemies=1)
        combat.hand = [make_noxious_fumes()]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.NOXIOUS_FUMES) == 2

        fire_after_side_turn_start(CombatSide.PLAYER, combat)

        for enemy in combat.enemies:
            assert enemy.get_power_amount(PowerId.POISON) == 2

    def test_phantom_blades_applies_power_and_buffs_only_first_shiv_each_turn(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        combat.hand = [make_phantom_blades(), make_blade_dance()]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.PHANTOM_BLADES) == 9
        assert combat.play_card(0)

        shiv_indices = [i for i, card in enumerate(combat.hand) if card.card_id == CardId.SHIV]
        assert len(shiv_indices) == 3

        hp_before_first = enemy.current_hp
        assert combat.play_card(shiv_indices[0], 0)
        assert enemy.current_hp == hp_before_first - 13

        next_shiv_index = next(i for i, card in enumerate(combat.hand) if card.card_id == CardId.SHIV)
        hp_before_second = enemy.current_hp
        assert combat.play_card(next_shiv_index, 0)
        assert enemy.current_hp == hp_before_second - 4

    def test_tactician_is_energy_positive_by_one_after_play_cost(self):
        combat = _make_combat()
        combat.hand = [make_tactician()]
        combat.energy = 3

        assert combat.play_card(0)
        assert combat.energy == 1

    def test_tracking_applies_two_stacks_and_doubles_damage_against_weak_targets(self):
        tracked_combat = _make_combat()
        tracked_enemy = tracked_combat.enemies[0]
        tracked_enemy.apply_power(PowerId.WEAK, 1)
        tracked_combat.hand = [make_tracking(), make_strike_silent()]
        tracked_combat.energy = 3

        assert tracked_combat.play_card(0)
        assert tracked_combat.player.get_power_amount(PowerId.TRACKING) == 2
        tracked_start_hp = tracked_enemy.current_hp
        assert tracked_combat.play_card(0, 0)
        tracked_damage = tracked_start_hp - tracked_enemy.current_hp

        baseline_combat = _make_combat()
        baseline_enemy = baseline_combat.enemies[0]
        baseline_enemy.apply_power(PowerId.WEAK, 1)
        baseline_combat.hand = [make_strike_silent()]
        baseline_combat.energy = 1

        baseline_start_hp = baseline_enemy.current_hp
        assert baseline_combat.play_card(0, 0)
        baseline_damage = baseline_start_hp - baseline_enemy.current_hp

        assert tracked_damage == baseline_damage * 2

    def test_malaise_uses_x_energy_for_strength_and_weak_and_requires_energy_to_play(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        combat.hand = [make_malaise()]
        combat.energy = 3

        assert combat.play_card(0, 0)
        assert combat.energy == 0
        assert enemy.get_power_amount(PowerId.STRENGTH) == -3
        assert enemy.get_power_amount(PowerId.WEAK) == 3

        no_energy_combat = _make_combat()
        no_energy_combat.hand = [make_malaise()]
        no_energy_combat.energy = 0

        assert not no_energy_combat.play_card(0, 0)
