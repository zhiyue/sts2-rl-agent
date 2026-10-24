"""Port of MegaCrit.Sts2.Core.Map.MapPathPruning (v0.107.1).

Detects and removes duplicate path segments from act maps, then repairs any
room types that fell below their target count. Both StandardActMap and
SpoilsActMap run ``PruneAndRepair`` after point type assignment
(StandardActMap.cs L102-105, SpoilsActMap.cs ctor).

The grid is a plain ``{(col, row): MapPoint}`` dict holding ONLY room points:
the Ancient start and Boss points are never placed in it, exactly like the
C# ``MapPoint?[7, _mapLength]`` Grid array which never contains them (that is
why ``MapPathPruning.IsInMap`` special-cases the Ancient/Boss types).
"""

from __future__ import annotations

from typing import Any, Callable

from sts2_env.core.enums import MapPointType
from sts2_env.map.map_point import MapPoint

MAX_PRUNE_ITERATIONS = 50  # C# MapPathPruning.PruneDuplicateSegments
MAX_PRUNE_AND_REPAIR_ROUNDS = 3  # C# MapPathPruning.PruneAndRepair


def stable_shuffle_points(rng: Any, points: list[MapPoint]) -> list[MapPoint]:
    """C# ListExtensions.StableShuffle: sort by coord (MapPoint.CompareTo ->
    MapCoord.CompareTo sorts by (col, row)), then Fisher-Yates via the rng
    (UnstableShuffle). ``rng.shuffle`` already implements UnstableShuffle.
    """
    ordered = sorted(points, key=lambda p: (p.coord.col, p.coord.row))
    rng.shuffle(ordered)
    return ordered


def prune_and_repair(
    grid: dict[tuple[int, int], MapPoint],
    start_map_points: set,
    starting_map_point: MapPoint,
    rng: Any,
    *,
    is_valid_point_type: Callable[[MapPointType, MapPoint], bool],
    num_shops: int,
    num_elites: int,
    num_rests: int,
    num_unknowns: int,
) -> None:
    """C# MapPathPruning.PruneAndRepair: loop {prune; if !repair: break}."""
    for _ in range(MAX_PRUNE_AND_REPAIR_ROUNDS):
        prune_duplicate_segments(grid, start_map_points, starting_map_point, rng)
        if not repair_pruned_point_types(
            grid,
            rng,
            is_valid_point_type=is_valid_point_type,
            num_shops=num_shops,
            num_elites=num_elites,
            num_rests=num_rests,
            num_unknowns=num_unknowns,
        ):
            break


def repair_pruned_point_types(
    grid: dict[tuple[int, int], MapPoint],
    rng: Any,
    *,
    is_valid_point_type: Callable[[MapPointType, MapPoint], bool],
    num_shops: int,
    num_elites: int,
    num_rests: int,
    num_unknowns: int,
) -> bool:
    """C# MapPathPruning.RepairPrunedPointTypes. Returns True when any repair
    was made (indicating re-pruning may be needed)."""
    repaired = False
    repaired |= repair_point_type(grid, MapPointType.SHOP, num_shops, rng, is_valid_point_type)
    repaired |= repair_point_type(grid, MapPointType.ELITE, num_elites, rng, is_valid_point_type)
    repaired |= repair_point_type(grid, MapPointType.REST_SITE, num_rests, rng, is_valid_point_type)
    repaired |= repair_point_type(grid, MapPointType.UNKNOWN, num_unknowns, rng, is_valid_point_type)
    return repaired


def repair_point_type(
    grid: dict[tuple[int, int], MapPoint],
    point_type: MapPointType,
    target_count: int,
    rng: Any,
    is_valid_point_type: Callable[[MapPointType, MapPoint], bool],
) -> bool:
    """C# MapPathPruning.RepairPointType: replace Monster (modifiable) points
    with the missing type until the target count is reached."""
    current = sum(1 for p in grid.values() if p.point_type == point_type)
    deficit = target_count - current
    if deficit <= 0:
        return False
    result = False
    candidates = [
        p for p in grid.values()
        if p.point_type == MapPointType.MONSTER and p.can_be_modified
    ]
    candidates = stable_shuffle_points(rng, candidates)
    for point in candidates:
        if deficit == 0:
            break
        if is_valid_point_type(point_type, point):
            point.point_type = point_type
            deficit -= 1
            result = True
    return result


def prune_duplicate_segments(
    grid: dict[tuple[int, int], MapPoint],
    start_map_points: set,
    starting_map_point: MapPoint,
    rng: Any,
) -> None:
    """C# MapPathPruning.PruneDuplicateSegments: prune until stable (<=50)."""
    iterations = 0
    matching_segments = find_matching_segments(starting_map_point)
    while prune_paths(grid, start_map_points, matching_segments, rng):
        iterations += 1
        if iterations > MAX_PRUNE_ITERATIONS:
            raise RuntimeError(
                f"Unable to prune matching segments in {iterations} iterations"
            )
        matching_segments = find_matching_segments(starting_map_point)


def find_matching_segments(starting_map_point: MapPoint) -> list[list[tuple]]:
    """C# MapPathPruning.FindMatchingSegments: duplicate segment groups in
    SortedDictionary (ordinal key) order. GetDuplicateSegments keeps only
    buckets holding MORE THAN ONE segment."""
    segments: dict[str, list[tuple]] = {}
    for path in find_all_paths(starting_map_point):
        _add_segments_to_dictionary(path, segments)
    return [
        segments[key]
        for key in sorted(segments.keys())
        if len(segments[key]) > 1
    ]


def find_all_paths(current_map_point: MapPoint) -> list[list[MapPoint]]:
    """C# MapPathPruning.FindAllPaths: every start->boss path."""
    paths: list[list[MapPoint]] = []
    if current_map_point.point_type == MapPointType.BOSS:
        return [[current_map_point]]
    for child in current_map_point.children:
        for tail in find_all_paths(child):
            paths.append([current_map_point, *tail])
    return paths


def _add_segments_to_dictionary(
    path: list[MapPoint],
    segments: dict[str, list[tuple]],
) -> None:
    """C# MapPathPruning.AddSegmentsToDictionary."""
    for i in range(len(path) - 1):
        if not _is_valid_segment_start(path[i]):
            continue
        for j in range(2, len(path) - i):
            end = path[i + j]
            if _is_valid_segment_end(end):
                segment = tuple(path[i:i + j + 1])
                key = _segment_key(segment)
                bucket = segments.get(key)
                if bucket is None:
                    segments[key] = [segment]
                elif not _any_overlapping_segments(bucket, segment):
                    bucket.append(segment)


def _is_valid_segment_start(start: MapPoint) -> bool:
    """C#: Children.Count > 1 || coord.row == 0."""
    return len(start.children) > 1 or start.coord.row == 0


def _is_valid_segment_end(end: MapPoint) -> bool:
    """C#: parents.Count >= 2."""
    return len(end.parents) >= 2


def _segment_key(segment: tuple[MapPoint, ...]) -> str:
    """C# MapPathPruning.GenerateSegmentKey (exact string format)."""
    first, last = segment[0], segment[-1]
    if first.coord.row == 0:
        prefix = f"{first.coord.row}-{last.coord.col},{last.coord.row}-"
    else:
        prefix = f"{first.coord.col},{first.coord.row}-{last.coord.col},{last.coord.row}-"
    return prefix + ",".join(str(point.point_type.value) for point in segment)


def _any_overlapping_segments(
    existing: list[tuple],
    segment: tuple[MapPoint, ...],
) -> bool:
    return any(_overlapping_segment(seg, segment) for seg in existing)


def _overlapping_segment(a: tuple, b: tuple) -> bool:
    """C# MapPathPruning.OverlappingSegment (reference equality on interior
    nodes; one object per coord so identity == coord equality here)."""
    if len(a) < 3 or len(b) < 3:
        return False
    for i in range(1, len(a) - 1):
        if a[i] is b[i]:
            return True
    return False


def prune_paths(
    grid: dict[tuple[int, int], MapPoint],
    start_map_points: set,
    matching_segments: list[list[tuple]],
    rng: Any,
) -> None:
    """C# MapPathPruning.PrunePaths."""
    for matching_segment in matching_segments:
        rng.shuffle(matching_segment)  # C# UnstableShuffle(matchingSegment)
        if prune_all_but_last(grid, start_map_points, matching_segment) != 0:
            return True
        if _break_parent_child_relationship_in_any_segment(matching_segment):
            return True
    return False


def prune_all_but_last(
    grid: dict[tuple[int, int], MapPoint],
    start_map_points: set,
    matches: list[tuple],
) -> int:
    """C# MapPathPruning.PruneAllButLast."""
    pruned = 0
    for match in matches:
        if pruned == len(matches) - 1:
            return pruned
        if prune_segment(grid, start_map_points, match):
            pruned += 1
    return pruned


def prune_segment(
    grid: dict[tuple[int, int], MapPoint],
    start_map_points: set,
    segment: tuple,
) -> bool:
    """C# MapPathPruning.PruneSegment."""
    result = False
    for i in range(len(segment) - 1):
        point = segment[i]
        if not is_in_map(grid, point):
            return True
        if (
            len(point.children) > 1
            or len(point.parents) > 1
            or any(
                len(parent.children) == 1 and not is_removed(grid, parent)
                for parent in point.parents
            )
        ):
            continue
        source = segment[i:]
        if not any(
            len(n.children) > 1 and len(n.parents) == 1 for n in source
        ):
            if len(segment[-1].parents) == 1:
                return False
            if not any(
                len(child.parents) == 1
                for child in point.children
                if child not in segment
            ):
                remove_point(grid, start_map_points, point)
                result = True
    return result


def remove_point(
    grid: dict[tuple[int, int], MapPoint],
    start_map_points: set,
    point: MapPoint,
) -> None:
    """C# MapPathPruning.RemovePoint."""
    grid.pop((point.coord.col, point.coord.row), None)
    start_map_points.discard(point)
    for child in list(point.children):
        point.remove_child(child)
    for parent in list(point.parents):
        parent.remove_child(point)


def is_in_map(grid: dict[tuple[int, int], MapPoint], point: MapPoint) -> bool:
    """C# MapPathPruning.IsInMap."""
    if (point.coord.col, point.coord.row) not in grid and point.point_type != MapPointType.ANCIENT:
        return point.point_type == MapPointType.BOSS
    return True


def is_removed(grid: dict[tuple[int, int], MapPoint], point: MapPoint) -> bool:
    """C# MapPathPruning.IsRemoved."""
    return (point.coord.col, point.coord.row) not in grid


def _break_parent_child_relationship_in_any_segment(matches: list[tuple]) -> bool:
    return any(_break_parent_child_relationship_in_segment(match) for match in matches)


def _break_parent_child_relationship_in_segment(segment: tuple) -> bool:
    """C# MapPathPruning.BreakAParentChildRelationshipInSegment."""
    result = False
    for i in range(len(segment) - 1):
        point = segment[i]
        if len(point.children) >= 2:
            nxt = segment[i + 1]
            if len(nxt.parents) != 1:
                point.remove_child(nxt)
                result = True
    return result
