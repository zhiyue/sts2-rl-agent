"""Integration tests for run flow.

Tests RunState initialization, room transitions, act transitions,
rest site healing, and full run lifecycle.
"""

import math
import pytest

from sts2_env.core.enums import (
    CardId, CardRarity, CardType, MapPointType, RoomType, TargetType,
)
from sts2_env.core.rng import Rng
from sts2_env.cards.base import CardInstance
from sts2_env.map.map_point import MapCoord
from sts2_env.run.run_state import RunState, PlayerState
from sts2_env.run.rooms import create_room, CombatRoom, ShopRoom, RestSiteRoom, EventRoom, TreasureRoom
from sts2_env.run.rest_site import generate_rest_site_options, HealOption, SmithOption
from sts2_env.run.odds import UnknownMapPointOdds, PotionRewardOdds


class TestRunStateInitialization:
    def test_default_ironclad_stats(self):
        rs = RunState(seed=42, character_id="Ironclad")
        assert rs.player.max_hp == 80
        assert rs.player.current_hp == 80
        assert rs.player.gold == 99
        assert rs.player.max_potion_slots == 3

    def test_ascension_3_reduces_gold(self):
        rs = RunState(seed=42, ascension_level=3)
        rs.initialize_run()
        assert rs.player.gold == round(99 * 0.75)  # 74

    def test_ascension_4_reduces_potion_slots(self):
        rs = RunState(seed=42, ascension_level=4)
        rs.initialize_run()
        assert rs.player.max_potion_slots == 2

    def test_ascension_5_adds_curse(self):
        rs = RunState(seed=42, ascension_level=5)
        rs.initialize_run()
        curses = [c for c in rs.player.deck if c.card_id == CardId.ASCENDERS_BANE]
        assert len(curses) == 1
        assert curses[0].card_type == CardType.CURSE

    def test_initialize_generates_map(self):
        rs = RunState(seed=42)
        rs.initialize_run()
        assert rs.map is not None
        assert len(rs.map.room_points()) > 0

    def test_initial_act_is_zero(self):
        rs = RunState(seed=42)
        rs.initialize_run()
        assert rs.current_act_index == 0

    def test_visited_coords_empty_initially(self):
        rs = RunState(seed=42)
        rs.initialize_run()
        assert len(rs.visited_map_coords) == 0


class TestPlayerState:
    def test_heal_caps_at_max(self):
        p = PlayerState(max_hp=80, current_hp=60)
        healed = p.heal(30)
        assert p.current_hp == 80
        assert healed == 20

    def test_heal_exact(self):
        p = PlayerState(max_hp=80, current_hp=70)
        healed = p.heal(5)
        assert p.current_hp == 75
        assert healed == 5

    def test_lose_hp(self):
        p = PlayerState(max_hp=80, current_hp=50)
        lost = p.lose_hp(20)
        assert p.current_hp == 30
        assert lost == 20

    def test_lose_hp_floors_at_zero(self):
        p = PlayerState(max_hp=80, current_hp=10)
        lost = p.lose_hp(30)
        assert p.current_hp == 0
        assert p.is_dead

    def test_gain_gold(self):
        p = PlayerState(gold=50)
        p.gain_gold(30)
        assert p.gold == 80

    def test_lose_gold(self):
        p = PlayerState(gold=50)
        lost = p.lose_gold(20)
        assert p.gold == 30
        assert lost == 20

    def test_lose_gold_caps_at_zero(self):
        p = PlayerState(gold=10)
        lost = p.lose_gold(20)
        assert p.gold == 0
        assert lost == 10

    def test_gain_max_hp(self):
        p = PlayerState(max_hp=80, current_hp=70)
        p.gain_max_hp(10)
        assert p.max_hp == 90
        assert p.current_hp == 80

    def test_lose_max_hp(self):
        p = PlayerState(max_hp=80, current_hp=80)
        p.lose_max_hp(10)
        assert p.max_hp == 70
        assert p.current_hp == 70

    def test_potion_slots(self):
        from sts2_env.potions.base import create_potion
        import sts2_env.potions.all  # noqa: F401
        p = PlayerState(max_potion_slots=3)
        assert p.add_potion(create_potion("FirePotion"))
        assert p.add_potion(create_potion("BlockPotion"))
        assert p.add_potion(create_potion("StrengthPotion"))
        # 4th should fail
        assert not p.add_potion(create_potion("EnergyPotion"))
        assert len(p.held_potions()) == 3

    def test_remove_potion(self):
        from sts2_env.potions.base import create_potion
        import sts2_env.potions.all  # noqa: F401
        p = PlayerState(max_potion_slots=3)
        p.add_potion(create_potion("FirePotion"))
        removed = p.remove_potion(0)
        assert removed is not None
        assert removed.potion_id == "FirePotion"
        assert len(p.held_potions()) == 0


class TestMapNavigation:
    def test_available_coords_at_start(self):
        rs = RunState(seed=42)
        rs.initialize_run()
        coords = rs.get_available_next_coords()
        assert len(coords) >= 2  # at least 2 starting paths

    def test_available_coords_all_row_1(self):
        rs = RunState(seed=42)
        rs.initialize_run()
        coords = rs.get_available_next_coords()
        for c in coords:
            assert c.row == 1

    def test_visit_coord_updates_state(self):
        rs = RunState(seed=42)
        rs.initialize_run()
        coords = rs.get_available_next_coords()
        rs.add_visited_coord(coords[0])
        assert len(rs.visited_map_coords) == 1
        assert rs.act_floor == coords[0].row + 1
        assert rs.total_floor == 1

    def test_second_move_goes_to_row_2(self):
        rs = RunState(seed=42)
        rs.initialize_run()
        coords = rs.get_available_next_coords()
        rs.add_visited_coord(coords[0])
        next_coords = rs.get_available_next_coords()
        for c in next_coords:
            assert c.row == 2

    def test_full_path_to_boss(self):
        """Walk a full path from start to boss."""
        rs = RunState(seed=42)
        rs.initialize_run()

        steps = 0
        while True:
            coords = rs.get_available_next_coords()
            if not coords:
                break
            rs.add_visited_coord(coords[0])
            steps += 1
            point = rs.map.get_point(coords[0])
            if point and point.point_type == MapPointType.BOSS:
                break

        assert steps > 0
        assert rs.total_floor == steps


class TestRoomTypeResolution:
    def test_monster_resolves_to_monster(self):
        rs = RunState(seed=42)
        rs.initialize_run()
        assert rs.resolve_room_type(MapPointType.MONSTER) == RoomType.MONSTER

    def test_elite_resolves_to_elite(self):
        rs = RunState(seed=42)
        rs.initialize_run()
        assert rs.resolve_room_type(MapPointType.ELITE) == RoomType.ELITE

    def test_boss_resolves_to_boss(self):
        rs = RunState(seed=42)
        rs.initialize_run()
        assert rs.resolve_room_type(MapPointType.BOSS) == RoomType.BOSS

    def test_shop_resolves_to_shop(self):
        rs = RunState(seed=42)
        rs.initialize_run()
        assert rs.resolve_room_type(MapPointType.SHOP) == RoomType.SHOP

    def test_rest_site_resolves_to_rest_site(self):
        rs = RunState(seed=42)
        rs.initialize_run()
        assert rs.resolve_room_type(MapPointType.REST_SITE) == RoomType.REST_SITE

    def test_treasure_resolves_to_treasure(self):
        rs = RunState(seed=42)
        rs.initialize_run()
        assert rs.resolve_room_type(MapPointType.TREASURE) == RoomType.TREASURE

    def test_unknown_resolves_to_valid_type(self):
        rs = RunState(seed=42)
        rs.initialize_run()
        result = rs.resolve_room_type(MapPointType.UNKNOWN)
        valid = {RoomType.MONSTER, RoomType.ELITE, RoomType.SHOP,
                 RoomType.TREASURE, RoomType.EVENT}
        assert result in valid

    def test_ancient_resolves_to_event(self):
        rs = RunState(seed=42)
        rs.initialize_run()
        assert rs.resolve_room_type(MapPointType.ANCIENT) == RoomType.EVENT


class TestRoomCreation:
    def test_create_monster_room(self):
        room = create_room(RoomType.MONSTER)
        assert isinstance(room, CombatRoom)
        assert room.room_type == RoomType.MONSTER

    def test_create_elite_room(self):
        room = create_room(RoomType.ELITE)
        assert isinstance(room, CombatRoom)
        assert room.is_elite

    def test_create_boss_room(self):
        room = create_room(RoomType.BOSS)
        assert isinstance(room, CombatRoom)
        assert room.is_boss

    def test_create_shop_room(self):
        room = create_room(RoomType.SHOP)
        assert isinstance(room, ShopRoom)

    def test_create_rest_site_room(self):
        room = create_room(RoomType.REST_SITE)
        assert isinstance(room, RestSiteRoom)

    def test_create_treasure_room(self):
        room = create_room(RoomType.TREASURE)
        assert isinstance(room, TreasureRoom)

    def test_create_event_room(self):
        room = create_room(RoomType.EVENT)
        assert isinstance(room, EventRoom)


class TestActTransition:
    def test_enter_next_act(self):
        rs = RunState(seed=42)
        rs.initialize_run()
        assert rs.current_act_index == 0
        result = rs.enter_next_act()
        assert result is True
        assert rs.current_act_index == 1
        assert rs.map is not None
        assert len(rs.visited_map_coords) == 0

    def test_final_act_wins_run(self):
        rs = RunState(seed=42)
        rs.initialize_run()
        rs.enter_act(len(rs.acts) - 1)
        result = rs.enter_next_act()
        assert result is False
        assert rs.is_over
        assert rs.player_won

    def test_act_transition_resets_unknown_odds(self):
        rs = RunState(seed=42)
        rs.initialize_run()
        # Mutate unknown odds
        rs.unknown_odds._current[RoomType.MONSTER] = 0.99
        rs.enter_next_act()
        # Should be reset to base
        assert rs.unknown_odds._current[RoomType.MONSTER] == pytest.approx(
            UnknownMapPointOdds.BASE_MONSTER
        )


class TestRestSiteHealing:
    def test_heal_30_percent(self):
        p = PlayerState(max_hp=80, current_hp=50)
        options = generate_rest_site_options(p)
        heal_opt = next(o for o in options if o.option_id == "HEAL")
        result = heal_opt.execute(p)
        expected_heal = math.floor(80 * 0.3)  # 24
        assert p.current_hp == 50 + expected_heal
        assert "24" in result

    def test_heal_does_not_exceed_max(self):
        p = PlayerState(max_hp=80, current_hp=78)
        options = generate_rest_site_options(p)
        heal_opt = next(o for o in options if o.option_id == "HEAL")
        heal_opt.execute(p)
        assert p.current_hp == 80  # capped

    def test_smith_available(self):
        p = PlayerState()
        p.deck.append(CardInstance(
            card_id=CardId.STRIKE_IRONCLAD, cost=1, card_type=CardType.ATTACK,
            target_type=TargetType.ANY_ENEMY,
        ))
        options = generate_rest_site_options(p)
        smith = next(o for o in options if o.option_id == "SMITH")
        assert smith.enabled

    def test_smith_disabled_when_all_upgraded(self):
        p = PlayerState()
        card = CardInstance(
            card_id=CardId.STRIKE_IRONCLAD, cost=1, card_type=CardType.ATTACK,
            target_type=TargetType.ANY_ENEMY, upgraded=True,
        )
        p.deck.append(card)
        options = generate_rest_site_options(p)
        smith = next(o for o in options if o.option_id == "SMITH")
        assert not smith.enabled

    def test_smith_upgrades_card(self):
        p = PlayerState()
        card = CardInstance(
            card_id=CardId.STRIKE_IRONCLAD, cost=1, card_type=CardType.ATTACK,
            target_type=TargetType.ANY_ENEMY,
        )
        p.deck.append(card)
        options = generate_rest_site_options(p)
        smith = next(o for o in options if o.option_id == "SMITH")
        smith.execute(p, card_index=0)
        assert p.deck[0].upgraded

    def test_dig_available_with_shovel(self):
        p = PlayerState()
        options = generate_rest_site_options(p, relic_ids=["Shovel"])
        dig = [o for o in options if o.option_id == "DIG"]
        assert len(dig) == 1

    def test_lift_available_with_girya(self):
        p = PlayerState()
        options = generate_rest_site_options(p, relic_ids=["Girya"])
        lift = [o for o in options if o.option_id == "LIFT"]
        assert len(lift) == 1


class TestUnknownMapPointOdds:
    def test_initial_odds(self):
        odds = UnknownMapPointOdds()
        assert odds._current[RoomType.MONSTER] == pytest.approx(0.10)
        assert odds._current[RoomType.ELITE] == pytest.approx(-1.00)
        assert odds._current[RoomType.TREASURE] == pytest.approx(0.02)
        assert odds._current[RoomType.SHOP] == pytest.approx(0.03)

    def test_reset_to_base(self):
        odds = UnknownMapPointOdds()
        odds._current[RoomType.MONSTER] = 0.99
        odds.reset_to_base()
        assert odds._current[RoomType.MONSTER] == pytest.approx(0.10)

    def test_odds_shift_over_time(self):
        """Non-rolled types should increase, rolled type should reset."""
        odds = UnknownMapPointOdds()
        rs = RunState(42)
        rs.initialize_run()
        rng = Rng(42)

        initial_monster = odds._current[RoomType.MONSTER]
        result = odds.roll(rng, rs)

        if result == RoomType.MONSTER:
            assert odds._current[RoomType.MONSTER] == pytest.approx(initial_monster)
        else:
            assert odds._current[RoomType.MONSTER] > initial_monster


class TestPotionRewardOdds:
    def test_initial_value(self):
        odds = PotionRewardOdds()
        assert odds.current_value == pytest.approx(0.40)

    def test_oscillation(self):
        """Odds should swing by 0.10 each roll."""
        odds = PotionRewardOdds()
        rng = Rng(42)
        initial = odds.current_value
        got = odds.roll(rng)
        if got:
            assert odds.current_value == pytest.approx(initial - 0.10)
        else:
            assert odds.current_value == pytest.approx(initial + 0.10)

    def test_elite_bonus(self):
        """Elite rolls should have higher drop rate."""
        rng_reg = Rng(42)
        rng_elite = Rng(42)
        n = 5000
        odds_reg = PotionRewardOdds()
        odds_elite = PotionRewardOdds()

        reg_drops = sum(1 for _ in range(n) if odds_reg.roll(rng_reg, is_elite=False))
        elite_drops = sum(1 for _ in range(n) if odds_elite.roll(rng_elite, is_elite=True))

        assert elite_drops > reg_drops


class TestRunDeterminism:
    def test_same_seed_same_state(self):
        rs1 = RunState(seed=42)
        rs1.initialize_run()
        rs2 = RunState(seed=42)
        rs2.initialize_run()

        assert rs1.player.gold == rs2.player.gold
        assert rs1.player.max_hp == rs2.player.max_hp

        coords1 = rs1.get_available_next_coords()
        coords2 = rs2.get_available_next_coords()
        assert coords1 == coords2


class TestWinLose:
    def test_win_run(self):
        rs = RunState(seed=42)
        rs.win_run()
        assert rs.is_over
        assert rs.player_won

    def test_lose_run(self):
        rs = RunState(seed=42)
        rs.lose_run()
        assert rs.is_over
        assert not rs.player_won
