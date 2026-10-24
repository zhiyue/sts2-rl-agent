"""A10 parity tests: map path pruning, repair, and post-processing.

C# sources (v0.107.1 decompiled):
- MegaCrit.Sts2.Core.Map/MapPathPruning.cs -- PruneAndRepair: after point
  type assignment, prune duplicate path segments then repair any type that
  fell below its target count (loops up to 3 rounds).
- MegaCrit.Sts2.Core.Map/StandardActMap.cs L100-108 -- pipeline order:
  GenerateMap -> AssignPointTypes -> PruneAndRepair -> CenterGrid ->
  SpreadAdjacentMapPoints -> StraightenPaths; and the ctor assigns
  _pointTypeCounts (GetMapPointTypes) BEFORE GenerateMap.
- MegaCrit.Sts2.Core.Map/SpoilsActMap.cs -- hourglass map also runs
  PruneAndRepair (and nothing else).

Guarantee semantics: RepairPointType tops a type back up to its target by
converting modifiable Monster points; a deficit can remain only when no
candidate point passes IsValidPointType (that exhaustion condition is
asserted here exactly, so the test is faithful rather than lenient).
"""

from collections import Counter

from sts2_env.core.enums import MapPointType
from sts2_env.core.rng import Rng
from sts2_env.map.generator import (
    generate_act_map,
    generate_spoils_act_map,
    _is_type_valid_for_point,
    _map_point_type_counts,
)
from sts2_env.map.path_pruning import find_matching_segments

ACT_SHAPE = ((0, 15), (1, 14), (2, 13))
SEEDS = range(40)

POOL_TYPES = (
    MapPointType.SHOP,
    MapPointType.ELITE,
    MapPointType.REST_SITE,
    MapPointType.UNKNOWN,
)


def _targets(seed: int, act_index: int, ascension: int):
    """Per-act MapPointTypeCounts rolled from an identical fresh stream
    (C# StandardActMap ctor: GetMapPointTypes runs before GenerateMap)."""
    counts_rng = Rng(seed, f"act_{act_index + 1}_map")
    elites, shops, unknowns, rests = _map_point_type_counts(
        counts_rng, ascension_level=ascension, act_index=act_index
    )
    return {
        MapPointType.SHOP: shops,
        MapPointType.ELITE: elites,
        MapPointType.UNKNOWN: unknowns,
        MapPointType.REST_SITE: rests,
    }


def _deficit_is_unplaceable(act_map, counts, targets) -> bool:
    """C# MapPathPruning.RepairPointType leaves a deficit only when every
    candidate (Monster + CanBeModified) fails IsValidPointType. The fixed
    rows are CanBeModified=false (C# AssignPointTypes), so they are never
    candidates."""
    for point_type, target in targets.items():
        if counts[point_type] >= target:
            continue
        for point in act_map.room_points():
            if point.point_type != MapPointType.MONSTER or not point.can_be_modified:
                continue
            if _is_type_valid_for_point(point, point_type, act_map.map_length):
                return False
    return True


class TestRoomTypeTargetsReachCSharpCounts:
    def test_all_acts_multiple_seeds_meet_or_exhaust_targets(self):
        """Multi-seed/act check that each pool type reaches its C# target
        count; any remaining deficit must be provably unplaceable under the
        C# validity rules (RepairPointType exhaustion)."""
        for ascension in (0, 1):
            for seed in SEEDS:
                for act_index, num_rooms in ACT_SHAPE:
                    targets = _targets(seed, act_index, ascension)
                    act_map = generate_act_map(
                        num_rooms,
                        Rng(seed, f"act_{act_index + 1}_map"),
                        ascension_level=ascension,
                        act_index=act_index,
                    )
                    counts = Counter(p.point_type for p in act_map.room_points())
                    deficits = {
                        t: (counts[t], want)
                        for t, want in targets.items()
                        if counts[t] < want
                    }
                    assert not deficits or _deficit_is_unplaceable(
                        act_map, counts, targets
                    ), (ascension, seed, act_index, deficits)

    def test_fixed_rows_exact(self):
        """C# AssignPointTypes: last room row is always RestSite, treasure
        row (rowCount-7) is Treasure, first room row is Monster."""
        act_map = generate_act_map(15, Rng(7, "act_1_map"), act_index=0)
        last_row = act_map.get_row(act_map.map_length - 1)
        assert last_row and all(
            p.point_type == MapPointType.REST_SITE for p in last_row
        )
        first_row = act_map.get_row(1)
        assert first_row and all(
            p.point_type == MapPointType.MONSTER for p in first_row
        )
        treasure_row = act_map.get_row(act_map.map_length - 7)
        assert treasure_row and all(
            p.point_type == MapPointType.TREASURE for p in treasure_row
        )

    def test_elite_rest_lower_bound_row6(self):
        """C# StandardActMap.IsValidForLower: Elite/RestSite cannot be placed
        at row < 6 (A4); IsValidForUpper: no RestSite at row >= length-3
        except the fixed last row."""
        for seed in SEEDS:
            for act_index, num_rooms in ACT_SHAPE:
                act_map = generate_act_map(
                    num_rooms,
                    Rng(seed, f"act_{act_index + 1}_map"),
                    act_index=act_index,
                )
                for point in act_map.room_points():
                    if point.row < 6:
                        assert point.point_type not in (
                            MapPointType.ELITE,
                            MapPointType.REST_SITE,
                        ), (seed, act_index, point)
                    if (
                        point.row >= act_map.map_length - 3
                        and point.point_type == MapPointType.REST_SITE
                    ):
                        assert point.row == act_map.map_length - 1


class TestPruneAndRepairTopology:
    def test_no_remaining_duplicate_segments(self):
        """C# MapPathPruning.PruneDuplicateSegments loops until PrunePaths
        reports nothing left to prune, so the final map has no duplicate
        segment groups (same junction pair + same type sequence on parallel
        node sets). Re-running FindMatchingSegments must come back empty."""
        for seed in SEEDS:
            for act_index, num_rooms in ACT_SHAPE:
                act_map = generate_act_map(
                    num_rooms,
                    Rng(seed, f"act_{act_index + 1}_map"),
                    act_index=act_index,
                )
                remaining = find_matching_segments(act_map.start_point)
                assert remaining == [], (seed, act_index, len(remaining))

    def test_pruning_grid_coords_consistent(self):
        """Post-processing moves points inside the grid dict; every point's
        coord must equal its grid key (C# mutates coord when moving)."""
        for seed in SEEDS:
            for act_index, num_rooms in ACT_SHAPE:
                act_map = generate_act_map(
                    num_rooms,
                    Rng(seed, f"act_{act_index + 1}_map"),
                    act_index=act_index,
                )
                for coord, point in act_map._grid.items():
                    assert (point.coord.col, point.coord.row) == coord


class TestSpoilsMapPruneAndRepair:
    def test_spoils_meets_or_exhausts_targets(self):
        """C# SpoilsActMap ctor also runs MapPathPruning.PruneAndRepair."""
        for seed in range(20):
            counts_rng = Rng(seed, "spoils_map")
            elites, shops, unknowns, rests = _map_point_type_counts(
                counts_rng, ascension_level=0, act_index=1
            )
            targets = {
                MapPointType.SHOP: shops,
                MapPointType.ELITE: elites,
                MapPointType.UNKNOWN: unknowns,
                MapPointType.REST_SITE: rests,
            }
            act_map = generate_spoils_act_map(14, Rng(seed, "spoils_map"), act_index=1)
            counts = Counter(p.point_type for p in act_map.room_points())
            deficits = {
                t: (counts[t], want) for t, want in targets.items() if counts[t] < want
            }
            assert not deficits or _deficit_is_unplaceable(act_map, counts, targets), (
                seed,
                deficits,
            )
            # Hourglass: every path converges through the single center treasure.
            treasures = [
                p for p in act_map.room_points()
                if p.point_type == MapPointType.TREASURE
            ]
            assert len(treasures) == 1
            assert treasures[0].col == 3
