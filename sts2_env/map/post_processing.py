"""Port of MegaCrit.Sts2.Core.Map.MapPostProcessing (v0.107.1).

CenterGrid / SpreadAdjacentMapPoints / StraightenPaths, run by
StandardActMap after PruneAndRepair (StandardActMap.cs L106-108). All three
operate on the plain ``{(col, row): MapPoint}`` room grid described in
``path_pruning.py`` (the Ancient start and Boss points are not part of it and
keep their coordinates, exactly like the C# Grid array).
"""

from __future__ import annotations

from typing import Any

from sts2_env.map.map_point import MapPoint

INT_MAX = 2_147_483_647
MAP_WIDTH = 7


def center_grid(grid: dict[tuple[int, int], MapPoint], width: int = MAP_WIDTH) -> None:
    """C# MapPostProcessing.CenterGrid: shift all columns left/right by one
    when the two leftmost (or rightmost) columns are empty and the opposite
    side is not. Out-of-bounds cells are dropped (they are always empty)."""
    left_empty = not any(col in (0, 1) for col, _ in grid)
    right_empty = not any(col in (width - 1, width - 2) for col, _ in grid)
    shift = 0
    if left_empty and not right_empty:
        shift = -1
    elif not left_empty and right_empty:
        shift = 1
    if shift == 0:
        return
    new_grid: dict[tuple[int, int], MapPoint] = {}
    for (col, row), point in grid.items():
        target = col + shift
        if 0 <= target < width:
            new_grid[(target, row)] = point
            point.coord.col = target
    grid.clear()
    grid.update(new_grid)


def _neighbor_allowed_positions(column: int, total_columns: int) -> set[int]:
    """C# MapPostProcessing.GetNeighborAllowedPositions."""
    return {
        position
        for position in (column - 1, column, column + 1)
        if 0 <= position < total_columns
    }


def _allowed_positions(node: MapPoint, total_columns: int) -> list[int]:
    """C# MapPostProcessing.GetAllowedPositions: intersection of the
    neighbor ranges of every parent and child. Returned in ascending order,
    matching the .NET HashSet<int> iteration over small non-negative ints."""
    allowed = set(range(total_columns))
    for parent in node.parents:
        allowed &= _neighbor_allowed_positions(parent.coord.col, total_columns)
    for child in node.children:
        allowed &= _neighbor_allowed_positions(child.coord.col, total_columns)
    return sorted(allowed)


def spread_adjacent_map_points(
    grid: dict[tuple[int, int], MapPoint],
    width: int = MAP_WIDTH,
) -> None:
    """C# MapPostProcessing.SpreadAdjacentMapPoints: greedily move nodes to
    the allowed column with the largest gap to their row neighbours, until a
    full pass makes no move."""
    rows = sorted({row for _, row in grid})
    for row in rows:
        row_points = [
            grid[(col, row)] for col in range(width) if (col, row) in grid
        ]
        changed = True
        while changed:
            changed = False
            for item in row_points:
                col = item.coord.col
                best_col = col
                best_gap = _compute_gap(col, row_points, item)
                for candidate in _allowed_positions(item, width):
                    if candidate == col:
                        continue
                    occupant = grid.get((candidate, row))
                    if occupant is not None and occupant is not item:
                        continue
                    gap = _compute_gap(candidate, row_points, item)
                    if gap > best_gap:
                        best_col = candidate
                        best_gap = gap
                if best_col != col:
                    del grid[(col, row)]
                    grid[(best_col, row)] = item
                    item.coord.col = best_col
                    changed = True


def _compute_gap(
    candidate_col: int,
    row_points: list[MapPoint],
    current_node: MapPoint,
) -> int:
    """C# MapPostProcessing.ComputeGap: minimal column distance to any other
    node in the row (int.MaxValue when alone)."""
    gap: int | None = None
    for row_node in row_points:
        if row_node is current_node:
            continue
        distance = abs(candidate_col - row_node.coord.col)
        gap = distance if gap is None else min(gap, distance)
    return INT_MAX if gap is None else gap


def straighten_paths(
    grid: dict[tuple[int, int], MapPoint],
    width: int = MAP_WIDTH,
) -> None:
    """C# MapPostProcessing.StraightenPaths: single-child single-parent nodes
    that sit to the outside of both their parent and child slide one column
    toward them when the target cell is free."""
    rows = sorted({row for _, row in grid})
    for row in rows:
        for col in range(width):
            point = grid.get((col, row))
            if point is None or len(point.parents) != 1 or len(point.children) != 1:
                continue
            parent = point.parents[0]
            child = point.children[0]
            to_the_left = (
                point.coord.col < child.coord.col
                and point.coord.col < parent.coord.col
            )
            to_the_right = (
                point.coord.col > child.coord.col
                and point.coord.col > parent.coord.col
            )
            if to_the_left and col < width - 1:
                if (col + 1, row) in grid:
                    continue
                point.coord.col = col + 1
                del grid[(col, row)]
                grid[(col + 1, row)] = point
            if to_the_right and col > 0:
                if (col - 1, row) not in grid:
                    point.coord.col = col - 1
                    del grid[(col, row)]
                    grid[(col - 1, row)] = point
