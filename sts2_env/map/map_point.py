"""Map point and edge data structures for the act map."""

from __future__ import annotations

from dataclasses import dataclass, field

from sts2_env.core.enums import MapPointType


@dataclass
class MapCoord:
    """A (column, row) coordinate on the map grid."""
    col: int
    row: int

    def __hash__(self) -> int:
        return hash((self.col, self.row))

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, MapCoord):
            return NotImplemented
        return self.col == other.col and self.row == other.row

    def __repr__(self) -> str:
        return f"({self.col},{self.row})"


@dataclass
class MapEdge:
    """A directed edge between two map points."""
    src: MapCoord
    dst: MapCoord

    def __repr__(self) -> str:
        return f"{self.src}->{self.dst}"


@dataclass
class MapPoint:
    """A node on the act map grid."""

    coord: MapCoord
    point_type: MapPointType = MapPointType.UNASSIGNED
    children: list[MapPoint] = field(default_factory=list)
    parents: list[MapPoint] = field(default_factory=list)
    x_offset: float = 0.0  # visual offset for spreading

    @property
    def col(self) -> int:
        return self.coord.col

    @property
    def row(self) -> int:
        return self.coord.row

    def add_child(self, child: MapPoint) -> None:
        if child not in self.children:
            self.children.append(child)
        if self not in child.parents:
            child.parents.append(self)

    def has_child_at(self, col: int, row: int) -> bool:
        return any(c.col == col and c.row == row for c in self.children)

    def siblings(self) -> list[MapPoint]:
        """All children of any parent of this point, excluding self."""
        sibs: list[MapPoint] = []
        for parent in self.parents:
            for child in parent.children:
                if child is not self and child not in sibs:
                    sibs.append(child)
        return sibs

    def __hash__(self) -> int:
        return hash(self.coord)

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, MapPoint):
            return NotImplemented
        return self.coord == other.coord

    def __repr__(self) -> str:
        return f"MapPoint({self.coord}, {self.point_type.name})"
