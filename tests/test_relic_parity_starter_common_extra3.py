"""Third batch of focused parity tests for starter/common relic hooks."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.defect import create_defect_starter_deck
from sts2_env.cards.ironclad import create_ironclad_starter_deck, make_inflame
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import OrbType, PowerId, RoomType, ValueProp
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle, create_twig_slime_s
from sts2_env.run.rooms import RoomVisitContext
from sts2_env.run.run_state import RunState


def _make_ironclad_combat(
    relics: list[str] | None = None,
    *,
    seed: int = 300,
    enemies: int = 1,
) -> CombatState:
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=seed,
        character_id="Ironclad",
        relics=relics or [],
    )
    for i in range(enemies):
        if i == 0:
            creature, ai = create_shrinker_beetle(Rng(seed + i))
        else:
            creature, ai = create_twig_slime_s(Rng(seed + i))
        combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


def _make_defect_combat(
    relics: list[str] | None = None,
    *,
    seed: int = 400,
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


class TestRelicParityStarterCommonExtra3:
    def test_ring_of_the_drake_draws_plus_two_for_first_three_turns_only(self):
        """Matches RingOfTheDrake.cs: +2 draw on rounds 1-3, then stop."""
        combat = _make_ironclad_combat(["RingOfTheDrake"], seed=301)

        assert len(combat.hand) == 7

        combat.end_player_turn()
        assert combat.round_number == 2
        assert len(combat.hand) == 7

        combat.end_player_turn()
        assert combat.round_number == 3
        assert len(combat.hand) == 7

        combat.end_player_turn()
        assert combat.round_number == 4
        assert len(combat.hand) == 5

    def test_infused_core_channels_three_lightning_on_first_turn_only(self):
        """Matches InfusedCore.cs: channel 3 Lightning at first player-turn start."""
        combat = _make_defect_combat(["InfusedCore"], seed=302)

        assert combat.orb_queue is not None
        assert len(combat.orb_queue.orbs) == 3
        assert all(orb.orb_type == OrbType.LIGHTNING for orb in combat.orb_queue.orbs)

        combat.end_player_turn()
        assert combat.round_number == 2
        assert len(combat.orb_queue.orbs) == 3

    def test_permafrost_triggers_block_once_for_first_power_each_combat(self):
        """Matches Permafrost.cs: first Power card in combat grants 6 block once."""
        combat = _make_ironclad_combat(["Permafrost"], seed=303)
        combat.hand = [make_inflame(), make_inflame()]
        combat.energy = 2

        assert combat.player.block == 0
        assert combat.play_card(0)
        assert combat.player.block == 6

        assert combat.play_card(0)
        assert combat.player.block == 6

    def test_red_skull_applies_and_removes_strength_when_crossing_hp_threshold(self):
        """Matches RedSkull.cs: <=50% HP grants +3 Strength; above removes it."""
        combat = CombatState(
            player_hp=41,
            player_max_hp=80,
            deck=create_ironclad_starter_deck(),
            rng_seed=304,
            character_id="Ironclad",
            relics=["RedSkull"],
        )
        enemy, ai = create_twig_slime_s(Rng(304))
        combat.add_enemy(enemy, ai)
        combat.start_combat()

        assert combat.player.get_power_amount(PowerId.STRENGTH) == 0

        combat.deal_damage(enemy, combat.player, 1, ValueProp.UNPOWERED)
        assert combat.player.current_hp == 40
        assert combat.player.get_power_amount(PowerId.STRENGTH) == 3

        combat.player.heal(2)
        assert combat.player.current_hp == 42
        assert combat.player.get_power_amount(PowerId.STRENGTH) == 3

        combat.deal_damage(enemy, combat.player, 1, ValueProp.UNPOWERED)
        assert combat.player.current_hp == 41
        assert combat.player.get_power_amount(PowerId.STRENGTH) == 0

    def test_festive_popper_deals_round_one_aoe_damage_only_once(self):
        """Matches FestivePopper.cs: deal 9 unpowered to all enemies on round 1 only."""
        combat = _make_ironclad_combat(["FestivePopper"], seed=305, enemies=2)
        enemy_a, enemy_b = combat.enemies
        round_one_a = enemy_a.current_hp
        round_one_b = enemy_b.current_hp

        assert enemy_a.max_hp - round_one_a == 9
        assert enemy_b.max_hp - round_one_b == 9

        combat.end_player_turn()

        assert enemy_a.current_hp == round_one_a
        assert enemy_b.current_hp == round_one_b

    def test_venerable_tea_set_grants_next_combat_energy_once_after_rest_site(self):
        """Matches VenerableTeaSet.cs: rest site queues +2 energy for next combat only."""
        run_state = RunState(seed=306, character_id="Ironclad")
        assert run_state.player.obtain_relic("VENERABLE_TEA_SET")
        tea_set = next(relic for relic in run_state.player.get_relic_objects() if relic.relic_id.name == "VENERABLE_TEA_SET")
        tea_set.after_room_entered(run_state.player, RoomVisitContext(RoomType.REST_SITE))

        combat = CombatState(
            player_hp=run_state.player.current_hp,
            player_max_hp=run_state.player.max_hp,
            deck=list(run_state.player.deck),
            rng_seed=306,
            character_id="Ironclad",
            player_state=run_state.player,
        )
        enemy, ai = create_shrinker_beetle(Rng(306))
        combat.add_enemy(enemy, ai)
        combat.start_combat()

        assert combat.energy == 5

        combat2 = CombatState(
            player_hp=run_state.player.current_hp,
            player_max_hp=run_state.player.max_hp,
            deck=list(run_state.player.deck),
            rng_seed=307,
            character_id="Ironclad",
            player_state=run_state.player,
        )
        enemy2, ai2 = create_shrinker_beetle(Rng(307))
        combat2.add_enemy(enemy2, ai2)
        combat2.start_combat()

        assert combat2.energy == 3
