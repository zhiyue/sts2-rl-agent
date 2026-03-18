"""Additional parity coverage for high-signal Colorless cards."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.colorless import (
    make_believe_in_you,
    make_fasten,
    make_finesse,
    make_panic_button,
    make_shockwave,
    make_stratagem,
    make_the_gambit,
    make_thrumming_hatchet,
)
from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.cards.ironclad_basic import make_defend_ironclad, make_strike_ironclad
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import PowerId, ValueProp
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle
from sts2_env.run.run_state import PlayerState


def _make_combat(*, extra_enemies: int = 0) -> CombatState:
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=31337,
        character_id="Ironclad",
    )
    creature, ai = create_shrinker_beetle(Rng(31337))
    combat.add_enemy(creature, ai)
    for i in range(extra_enemies):
        extra_creature, extra_ai = create_shrinker_beetle(Rng(31338 + i))
        combat.add_enemy(extra_creature, extra_ai)
    combat.start_combat()
    return combat


class TestColorlessParityExtra3:
    def test_believe_in_you_upgraded_grants_four_energy_to_target_ally(self):
        combat = _make_combat()
        ally_state = PlayerState(player_id=2, character_id="Ironclad", max_hp=60, current_hp=60)
        ally = combat.add_ally_player(ally_state)
        ally_combat_state = combat.combat_player_state_for(ally)
        assert ally_combat_state is not None

        card = make_believe_in_you(upgraded=True)
        card.owner = ally
        ally_combat_state.hand = [card]
        ally_combat_state.zone_map["hand"] = ally_combat_state.hand
        ally_combat_state.energy = 0
        starting_primary_energy = combat.energy

        assert combat.play_card_from_creature(ally, 0, 0)
        assert combat.energy == starting_primary_energy + 4
        assert ally_combat_state.energy == 0

    def test_fasten_base_and_upgraded_stack_expected_amounts(self):
        combat = _make_combat()
        combat.hand = [make_fasten(), make_fasten(upgraded=True)]
        combat.energy = 2

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.FASTEN) == 5

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.FASTEN) == 12

    def test_finesse_upgraded_grants_seven_block_and_draws_one(self):
        combat = _make_combat()
        drawn = make_strike_ironclad()
        combat.hand = [make_finesse(upgraded=True)]
        combat.draw_pile = [drawn]

        assert combat.play_card(0)
        assert combat.player.block == 7
        assert combat.hand == [drawn]
        assert len(combat.draw_pile) == 0

    def test_panic_button_applies_no_block_and_prevents_later_block_gains(self):
        combat = _make_combat()
        combat.hand = [make_panic_button(), make_defend_ironclad()]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.block == 30
        assert combat.player.get_power_amount(PowerId.NO_BLOCK) == 2

        assert combat.play_card(0)
        assert combat.player.block == 30

        combat.end_player_turn()
        assert combat.player.get_power_amount(PowerId.NO_BLOCK) == 2

        combat.end_player_turn()
        assert combat.player.get_power_amount(PowerId.NO_BLOCK) == 1

        combat.end_player_turn()
        assert combat.player.get_power_amount(PowerId.NO_BLOCK) == 0

    def test_shockwave_upgraded_hits_all_enemies_and_artifact_blocks_first_debuff_only(self):
        combat = _make_combat(extra_enemies=1)
        first_enemy = combat.enemies[0]
        second_enemy = combat.enemies[1]
        first_enemy.apply_power(PowerId.ARTIFACT, 1)
        combat.hand = [make_shockwave(upgraded=True)]
        combat.energy = 2

        assert combat.play_card(0)

        assert first_enemy.get_power_amount(PowerId.ARTIFACT) == 0
        assert first_enemy.get_power_amount(PowerId.WEAK) == 0
        assert first_enemy.get_power_amount(PowerId.VULNERABLE) == 5

        assert second_enemy.get_power_amount(PowerId.WEAK) == 5
        assert second_enemy.get_power_amount(PowerId.VULNERABLE) == 5

    def test_stratagem_base_and_upgrade_costs_and_power_stack(self):
        combat = _make_combat()
        base = make_stratagem()
        upgraded = make_stratagem(upgraded=True)
        assert base.cost == 1
        assert upgraded.cost == 0
        combat.hand = [base, upgraded]
        combat.energy = 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.STRATAGEM) == 1

        assert combat.play_card(0)
        assert combat.player.get_power_amount(PowerId.STRATAGEM) == 2

    def test_the_gambit_kills_owner_on_first_unblocked_powered_attack_hit(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        combat.hand = [make_the_gambit()]
        combat.energy = 0

        assert combat.play_card(0)
        assert combat.player.block == 50
        assert combat.player.get_power_amount(PowerId.THE_GAMBIT) == 1

        combat.player.block = 0
        combat.deal_damage(dealer=enemy, target=combat.player, amount=1, props=ValueProp.MOVE)

        assert combat.player.is_dead
        assert combat.player.get_power_amount(PowerId.THE_GAMBIT) == 0

    def test_thrumming_hatchet_base_and_upgrade_damage_values_match_reference(self):
        combat = _make_combat()
        enemy = combat.enemies[0]
        enemy.max_hp = 200
        enemy.current_hp = 200
        combat.hand = [make_thrumming_hatchet(), make_thrumming_hatchet(upgraded=True)]
        combat.energy = 2

        assert combat.play_card(0, 0)
        assert enemy.current_hp == 189

        assert combat.play_card(0, 0)
        assert enemy.current_hp == 175
