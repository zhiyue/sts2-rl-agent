"""Fourth batch of focused parity tests for starter/common relic hooks."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import PowerId, RoomType, ValueProp
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle
from sts2_env.run.run_manager import RunManager
from sts2_env.run.run_state import RunState


def _make_ironclad_combat(
    relics: list[str] | None = None,
    *,
    seed: int = 900,
    player_hp: int = 80,
    player_max_hp: int = 80,
    player_state=None,
) -> CombatState:
    deck = list(player_state.deck) if player_state is not None else create_ironclad_starter_deck()
    combat = CombatState(
        player_hp=player_hp,
        player_max_hp=player_max_hp,
        deck=deck,
        rng_seed=seed,
        character_id="Ironclad",
        relics=relics or [],
        player_state=player_state,
    )
    creature, ai = create_shrinker_beetle(Rng(seed))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


class TestRelicParityStarterCommonExtra4:
    def test_blood_vial_heals_only_on_round_one_player_turn_start(self):
        """Matches BloodVial.cs: heal 2 only at first player turn start."""
        combat = _make_ironclad_combat(["BloodVial"], seed=901, player_hp=70)

        assert combat.player.current_hp == 72

        combat.player.current_hp = 60
        combat.end_player_turn()
        assert combat.round_number == 2
        assert combat.player.current_hp == 60

    def test_centennial_puzzle_draws_three_only_after_first_unblocked_damage(self):
        """Matches CentennialPuzzle.cs: first positive HP loss in combat draws 3 cards."""
        combat = _make_ironclad_combat(["CentennialPuzzle"], seed=902)
        enemy = combat.enemies[0]
        hand_before = len(combat.hand)

        combat.player.gain_block(99, unpowered=True)
        combat.deal_damage(enemy, combat.player, 3, ValueProp.UNPOWERED)
        assert len(combat.hand) == hand_before

        combat.player.block = 0
        combat.deal_damage(enemy, combat.player, 1, ValueProp.UNPOWERED)
        assert len(combat.hand) == hand_before + 3

        combat.deal_damage(enemy, combat.player, 1, ValueProp.UNPOWERED)
        assert len(combat.hand) == hand_before + 3

    def test_happy_flower_grants_energy_every_third_turn_and_persists_between_combats(self):
        """Matches HappyFlower.cs: +1 energy on every 3rd player turn, counter persists."""
        run_state = RunState(seed=903, character_id="Ironclad")
        assert run_state.player.obtain_relic("HAPPY_FLOWER")

        combat1 = _make_ironclad_combat(
            seed=903,
            player_hp=run_state.player.current_hp,
            player_max_hp=run_state.player.max_hp,
            player_state=run_state.player,
        )
        assert combat1.energy == 3

        combat1.end_player_turn()
        assert combat1.round_number == 2
        assert combat1.energy == 3

        combat1.end_player_turn()
        assert combat1.round_number == 3
        assert combat1.energy == 4

        combat2 = _make_ironclad_combat(
            seed=904,
            player_hp=run_state.player.current_hp,
            player_max_hp=run_state.player.max_hp,
            player_state=run_state.player,
        )
        assert combat2.energy == 3

        combat2.end_player_turn()
        assert combat2.round_number == 2
        assert combat2.energy == 3

        combat2.end_player_turn()
        assert combat2.round_number == 3
        assert combat2.energy == 4

    def test_meal_ticket_heals_only_when_entering_shop_rooms(self):
        """Matches MealTicket.cs: heal 15 when entering Merchant rooms only."""
        mgr = RunManager(seed=905, character_id="Ironclad")
        assert mgr.run_state.player.obtain_relic("MEAL_TICKET")
        mgr.run_state.player.current_hp = 40
        mgr.run_state.player.max_hp = 80

        mgr._enter_room(RoomType.MONSTER)
        assert mgr.run_state.player.current_hp == 40

        mgr._enter_room(RoomType.SHOP)
        assert mgr.run_state.player.current_hp == 55

        mgr._enter_room(RoomType.SHOP)
        assert mgr.run_state.player.current_hp == 70

    def test_potion_belt_increases_max_potion_slots_by_two_on_obtain(self):
        """Matches PotionBelt.cs: after obtaining, max potion slots +2."""
        run_state = RunState(seed=906, character_id="Ironclad")
        starting_slots = run_state.player.max_potion_slots

        assert run_state.player.obtain_relic("POTION_BELT")
        assert run_state.player.max_potion_slots == starting_slots + 2

    def test_strawberry_increases_max_hp_and_current_hp_by_seven_on_obtain(self):
        """Matches Strawberry.cs: gain 7 max HP and heal for 7 immediately."""
        run_state = RunState(seed=907, character_id="Ironclad")
        run_state.player.max_hp = 80
        run_state.player.current_hp = 30

        assert run_state.player.obtain_relic("STRAWBERRY")
        assert run_state.player.max_hp == 87
        assert run_state.player.current_hp == 37

    def test_vajra_applies_strength_at_combat_start(self):
        """Matches Vajra.cs: gain 1 Strength at combat start."""
        combat = _make_ironclad_combat(["Vajra"], seed=908)
        assert combat.player.get_power_amount(PowerId.STRENGTH) == 1

    def test_gorget_applies_plating_at_combat_start(self):
        """Matches Gorget.cs: gain 4 Plating at combat start."""
        combat = _make_ironclad_combat(["Gorget"], seed=909)
        assert combat.player.get_power_amount(PowerId.PLATING) == 4
