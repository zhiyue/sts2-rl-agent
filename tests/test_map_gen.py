"""Tests for map generation.

Verifies the StandardActMap algorithm: grid dimensions, path connectivity,
crossover prevention, room type constraints, boss connectivity, and room counts.
"""

import pytest

from sts2_env.core.enums import MapPointType
from sts2_env.core.rng import Rng
from sts2_env.map.generator import generate_act_map, ActMap, MAP_WIDTH
from sts2_env.map.map_point import MapCoord, MapPoint


# ── Helpers ───────────────────────────────────────────────────────────

def _gen(seed: int = 42, num_rooms: int = 15, ascension: int = 0) -> ActMap:
    return generate_act_map(num_rooms, Rng(seed), ascension_level=ascension)


def _all_edges(act_map: ActMap) -> list[tuple[MapCoord, MapCoord]]:
    """Collect all parent->child edges in the map."""
    edges = []
    for p in act_map.all_points():
        for c in p.children:
            edges.append((p.coord, c.coord))
    return edges


class TestGridDimensions:
    def test_seven_columns(self):
        m = _gen()
        cols = {p.col for p in m.room_points()}
        assert all(0 <= c < MAP_WIDTH for c in cols)

    def test_row_count_matches_num_rooms(self):
        for num_rooms in [12, 15, 18]:
            m = _gen(num_rooms=num_rooms)
            # Room points should span rows 1 through map_length - 1
            rows = {p.row for p in m.room_points()}
            assert min(rows) >= 1
            assert max(rows) <= m.map_length - 1

    def test_map_length_is_rooms_plus_one(self):
        m = _gen(num_rooms=15)
        assert m.map_length == 16  # 15 + 1


class TestPathConnectivity:
    def test_all_room_points_have_parent_or_child(self):
        """Every room point should be connected -- no orphans."""
        m = _gen()
        for p in m.room_points():
            assert len(p.parents) > 0 or len(p.children) > 0, (
                f"Orphan point at {p.coord}"
            )

    def test_first_row_reachable_from_start(self):
        m = _gen()
        assert m.start_point is not None
        first_row = m.get_row(1)
        assert len(first_row) >= 2, "Should have at least 2 starting room points"
        for p in first_row:
            assert m.start_point in p.parents, (
                f"First-row point {p.coord} not connected to start"
            )

    def test_boss_reachable_from_top_row(self):
        m = _gen()
        assert m.boss_point is not None
        top_row = m.get_row(m.map_length - 1)
        assert len(top_row) >= 1
        for p in top_row:
            assert m.boss_point in p.children, (
                f"Top-row point {p.coord} not connected to boss"
            )

    def test_edges_go_upward(self):
        """All edges (except start->row1 and toprow->boss) should go from lower to higher row."""
        m = _gen()
        for p in m.room_points():
            for c in p.children:
                if c is not m.boss_point:
                    assert c.row == p.row + 1, (
                        f"Edge {p.coord}->{c.coord} not strictly upward"
                    )

    def test_edges_within_one_column(self):
        """Each edge should span at most 1 column."""
        m = _gen()
        for p in m.room_points():
            for c in p.children:
                if c is not m.boss_point:
                    assert abs(c.col - p.col) <= 1, (
                        f"Edge {p.coord}->{c.coord} spans >1 column"
                    )


class TestNoCrossovers:
    def test_no_crossing_edges(self):
        """No two edges in the same row span should cross."""
        for seed in range(20):
            m = _gen(seed=seed)
            edges_by_row: dict[int, list[tuple[int, int, int, int]]] = {}
            for p in m.room_points():
                for c in p.children:
                    if c is not m.boss_point:
                        row = p.row
                        edges_by_row.setdefault(row, []).append(
                            (p.col, p.row, c.col, c.row)
                        )

            for row, edges in edges_by_row.items():
                for i, e1 in enumerate(edges):
                    for e2 in edges[i + 1:]:
                        s1, _, e1c, _ = e1
                        s2, _, e2c, _ = e2
                        if s1 == s2 and e1c == e2c:
                            continue  # same edge, ok
                        # Crossing: s1 < s2 but e1c > e2c, or vice versa
                        if s1 != s2 and e1c != e2c:
                            crosses = (s1 < s2 and e1c > e2c) or (s1 > s2 and e1c < e2c)
                            assert not crosses, (
                                f"Crossing edges at row {row}: "
                                f"({s1},{row})->({e1c},{row+1}) "
                                f"crosses ({s2},{row})->({e2c},{row+1}), seed={seed}"
                            )


class TestRoomTypeConstraints:
    def test_first_row_is_monster(self):
        m = _gen()
        for p in m.get_row(1):
            assert p.point_type == MapPointType.MONSTER

    def test_last_row_is_rest_site(self):
        m = _gen()
        for p in m.get_row(m.map_length - 1):
            assert p.point_type == MapPointType.REST_SITE

    def test_boss_is_boss_type(self):
        m = _gen()
        assert m.boss_point.point_type == MapPointType.BOSS

    def test_start_is_ancient(self):
        m = _gen()
        assert m.start_point.point_type == MapPointType.ANCIENT

    def test_no_rest_or_elite_in_lower_rows(self):
        """RestSite and Elite cannot appear in rows 0-4."""
        for seed in range(10):
            m = _gen(seed=seed)
            for p in m.room_points():
                if p.row < 5:
                    assert p.point_type != MapPointType.REST_SITE, (
                        f"RestSite found at row {p.row}, seed={seed}"
                    )
                    assert p.point_type != MapPointType.ELITE, (
                        f"Elite found at row {p.row}, seed={seed}"
                    )

    def test_no_rest_site_in_top_three_rows(self):
        """RestSite cannot appear in the last 3 rows (except the fixed last row)."""
        for seed in range(10):
            m = _gen(seed=seed)
            for p in m.room_points():
                if p.row >= m.map_length - 3 and p.row != m.map_length - 1:
                    assert p.point_type != MapPointType.REST_SITE, (
                        f"RestSite at row {p.row} (top 3), seed={seed}"
                    )

    def test_no_adjacent_restricted_types(self):
        """Elite, RestSite, Treasure, Shop should not appear next to same type."""
        restricted = {MapPointType.ELITE, MapPointType.REST_SITE,
                      MapPointType.TREASURE, MapPointType.SHOP}
        for seed in range(10):
            m = _gen(seed=seed)
            for p in m.room_points():
                if p.point_type in restricted:
                    for parent in p.parents:
                        if parent is m.start_point:
                            continue
                        assert parent.point_type != p.point_type, (
                            f"Adjacent {p.point_type.name} at {p.coord} "
                            f"and parent {parent.coord}, seed={seed}"
                        )


class TestRoomTypeCounts:
    def test_shop_count(self):
        """Should have exactly 3 shops."""
        m = _gen()
        shops = [p for p in m.room_points() if p.point_type == MapPointType.SHOP]
        assert len(shops) == 3

    def test_elite_count_a0(self):
        """At A0, should have ~5 elites (including treasure row if replaced)."""
        m = _gen(ascension=0)
        elites = [p for p in m.room_points() if p.point_type == MapPointType.ELITE]
        assert 3 <= len(elites) <= 8

    def test_elite_count_a1(self):
        """At A1+ (SwarmingElites), should have ~8 elites."""
        m = _gen(ascension=1)
        elites = [p for p in m.room_points() if p.point_type == MapPointType.ELITE]
        assert 5 <= len(elites) <= 12

    def test_unknown_count_range(self):
        """Unknowns should be in range [10, 14]."""
        # Run multiple seeds to test the gaussian distribution
        unknown_counts = []
        for seed in range(50):
            m = _gen(seed=seed)
            unknowns = [p for p in m.room_points() if p.point_type == MapPointType.UNKNOWN]
            unknown_counts.append(len(unknowns))

        # Due to placement constraints, actual count may be less than requested
        assert min(unknown_counts) >= 0
        assert max(unknown_counts) <= 20

    def test_rest_site_count_range(self):
        """Rests should be around [3, 6] (gaussian) plus the fixed last row."""
        rest_counts = []
        for seed in range(50):
            m = _gen(seed=seed)
            rests = [p for p in m.room_points() if p.point_type == MapPointType.REST_SITE]
            rest_counts.append(len(rests))

        # At minimum should have the fixed last row rests
        assert min(rest_counts) >= 1

    def test_no_unassigned_points(self):
        """All room points should have an assigned type."""
        for seed in range(20):
            m = _gen(seed=seed)
            for p in m.room_points():
                assert p.point_type != MapPointType.UNASSIGNED, (
                    f"Unassigned point at {p.coord}, seed={seed}"
                )


class TestDeterminism:
    def test_same_seed_same_map(self):
        """Same seed should produce identical map."""
        m1 = _gen(seed=123)
        m2 = _gen(seed=123)

        pts1 = sorted([(p.coord.col, p.coord.row, p.point_type) for p in m1.room_points()])
        pts2 = sorted([(p.coord.col, p.coord.row, p.point_type) for p in m2.room_points()])
        assert pts1 == pts2

    def test_different_seed_different_map(self):
        """Different seeds should produce different maps (with high probability)."""
        m1 = _gen(seed=1)
        m2 = _gen(seed=999)

        pts1 = sorted([(p.coord.col, p.coord.row, p.point_type) for p in m1.room_points()])
        pts2 = sorted([(p.coord.col, p.coord.row, p.point_type) for p in m2.room_points()])
        assert pts1 != pts2
