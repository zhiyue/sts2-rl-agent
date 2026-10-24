"""Per-act encounter room set (C# RoomSet) draw-sequence parity tests.

C# refs:
- MegaCrit.Sts2.Core.Rooms/RoomSet.cs: NextNormalEncounter /
  NextEliteEncounter read ``list[visited % Count]``; MarkVisited increments
  the counter per room entry; NextBossEncounter returns SecondBoss after the
  first boss visit (Double Boss ascension).
- MegaCrit.Sts2.Core.Models/ActModel.cs GenerateRooms: the first
  NumberOfWeakEncounters normal entries come from the weak pool, the rest
  from the regular pool; elites fill 15 slots; boss is one uniform pick.
"""

from sts2_env.core.enums import RoomType
from sts2_env.map.acts import ALL_ACTS
from sts2_env.run.run_state import RunState


def _encounter_names(room_set) -> list[str]:
    return [fn.__name__ for fn in room_set.normal_encounters]


class TestActRoomSetGeneration:
    def test_normal_queue_leads_with_weak_encounters(self):
        """C# GenerateRooms: first NumberOfWeakEncounters normal fights are
        from the weak pool."""
        weak_names = {
            "setup_shrinker_beetle_weak", "setup_fuzzy_wurm_crawler_weak",
            "setup_nibbits_weak", "setup_slimes_weak",
        }
        for seed in range(10):
            run_state = RunState(seed=seed)
            run_state.initialize_run()
            room_set = run_state.get_act_room_set(0)
            names = _encounter_names(room_set)
            assert len(names) == 15  # C# Overgrowth BaseNumberOfRooms
            assert all(name in weak_names for name in names[:3]), names

    def test_normal_queue_length_matches_act_rooms(self):
        run_state = RunState(seed=1)
        run_state.initialize_run()
        for act_index, act in enumerate(run_state.acts):
            room_set = run_state.get_act_room_set(act_index)
            assert len(room_set.normal_encounters) == act.num_rooms
            assert len(room_set.elite_encounters) == 15  # C# elite pulls
            assert room_set.boss is not None

    def test_normal_pulls_follow_visited_counter(self):
        """C# RoomSet.NextNormalEncounter: list[visited % Count], then
        MarkVisited increments."""
        run_state = RunState(seed=2)
        run_state.initialize_run()
        room_set = run_state.get_act_room_set(0)

        first = room_set.next_normal_encounter
        room_set.mark_visited(RoomType.MONSTER)
        second = room_set.next_normal_encounter
        assert first is not second or len(room_set.normal_encounters) == 1
        assert room_set.normal_encounters_visited == 1

        # Cycle back to the start after exhausting the list.
        for _ in range(len(room_set.normal_encounters) - 1):
            room_set.mark_visited(RoomType.MONSTER)
        assert room_set.next_normal_encounter is first

    def test_elite_pulls_follow_visited_counter(self):
        run_state = RunState(seed=3)
        run_state.initialize_run()
        room_set = run_state.get_act_room_set(0)
        first = room_set.next_elite_encounter
        room_set.mark_visited(RoomType.ELITE)
        assert room_set.next_elite_encounter is room_set.elite_encounters[1]
        assert room_set.elite_encounters_visited == 1
        assert first is room_set.elite_encounters[0]

    def test_boss_fixed_for_whole_act(self):
        """C#: the boss is rolled once per act; every boss room of the act
        uses the same encounter."""
        run_state = RunState(seed=4)
        run_state.initialize_run()
        room_set = run_state.get_act_room_set(0)
        first = room_set.next_boss_encounter
        room_set.mark_visited(RoomType.BOSS)
        assert room_set.next_boss_encounter is first

    def test_double_boss_second_visit_uses_second_boss(self):
        """C# RoomSet.NextBossEncounter: after the first boss visit
        (bossEncountersVisited != 0), SecondBoss is served; the second boss
        is distinct from the first (RunManager.GenerateRooms)."""
        run_state = RunState(seed=5, ascension_level=10)
        run_state.initialize_run()
        room_set = run_state.get_act_room_set(len(run_state.acts) - 1)
        assert room_set.second_boss is not None
        assert room_set.second_boss is not room_set.boss

        first = room_set.next_boss_encounter
        room_set.mark_visited(RoomType.BOSS)
        assert first is room_set.boss
        assert room_set.next_boss_encounter is room_set.second_boss

    def test_normal_queue_contains_only_act_pool_members(self):
        """Every queued normal encounter comes from the act's weak or normal
        pool (C# AllWeakEncounters / AllRegularEncounters)."""
        from sts2_env.encounters import get_act_encounter_pools

        run_state = RunState(seed=6)
        run_state.initialize_run()
        for act_index in range(len(run_state.acts)):
            pools = get_act_encounter_pools(act_index)
            allowed = set(pools["weak"]) | set(pools["normal"])
            room_set = run_state.get_act_room_set(act_index)
            for fn in room_set.normal_encounters:
                assert fn in allowed, f"{fn.__name__} not in act {act_index} pools"
