using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Map;

public static class MapPathPruning
{
	public static void PruneDuplicateSegments(MapPoint?[,] grid, HashSet<MapPoint> startMapPoints, MapPoint startingMapPoint, Rng rng)
	{
		int num = 0;
		List<List<MapPoint[]>> matchingSegments = FindMatchingSegments(startingMapPoint);
		while (PrunePaths(grid, startMapPoints, matchingSegments, rng))
		{
			num++;
			if (num > 50)
			{
				throw new InvalidOperationException($"Unable to prune matching segments in {num} iterations");
			}
			matchingSegments = FindMatchingSegments(startingMapPoint);
		}
	}

	public static List<List<MapPoint[]>> FindMatchingSegments(MapPoint startingMapPoint)
	{
		List<List<MapPoint>> list = FindAllPaths(startingMapPoint);
		Dictionary<string, List<MapPoint[]>> segments = new Dictionary<string, List<MapPoint[]>>();
		foreach (List<MapPoint> item in list)
		{
			AddSegmentsToDictionary(item, segments);
		}
		return GetDuplicateSegments(segments);
	}

	public static List<List<MapPoint>> FindAllPaths(MapPoint currentMapPoint)
	{
		List<List<MapPoint>> list = new List<List<MapPoint>>();
		if (currentMapPoint.PointType == MapPointType.Boss)
		{
			int num = 1;
			List<MapPoint> list2 = new List<MapPoint>(num);
			CollectionsMarshal.SetCount(list2, num);
			Span<MapPoint> span = CollectionsMarshal.AsSpan(list2);
			int index = 0;
			span[index] = currentMapPoint;
			list.Add(list2);
			return list;
		}
		foreach (MapPoint child in currentMapPoint.Children)
		{
			List<List<MapPoint>> list3 = FindAllPaths(child);
			foreach (List<MapPoint> item in list3)
			{
				int index = 1;
				List<MapPoint> list4 = new List<MapPoint>(index);
				CollectionsMarshal.SetCount(list4, index);
				Span<MapPoint> span = CollectionsMarshal.AsSpan(list4);
				int num = 0;
				span[num] = currentMapPoint;
				List<MapPoint> list5 = list4;
				list5.AddRange(item);
				list.Add(list5);
			}
		}
		return list;
	}

	private static void AddSegmentsToDictionary(IReadOnlyList<MapPoint> path, IDictionary<string, List<MapPoint[]>> segments)
	{
		for (int i = 0; i < path.Count - 1; i++)
		{
			if (!IsValidSegmentStartMapPoint(path[i]))
			{
				continue;
			}
			for (int j = 2; j < path.Count - i; j++)
			{
				MapPoint endMapPoint = path[i + j];
				if (IsValidSegmentEndMapPoint(endMapPoint))
				{
					MapPoint[] array = path.Skip(i).Take(j + 1).ToArray();
					string key = GenerateSegmentKey(array);
					if (!segments.ContainsKey(key))
					{
						int num = 1;
						List<MapPoint[]> list = new List<MapPoint[]>(num);
						CollectionsMarshal.SetCount(list, num);
						Span<MapPoint[]> span = CollectionsMarshal.AsSpan(list);
						int index = 0;
						span[index] = array;
						segments[key] = list;
					}
					else if (!AnyOverlappingSegments(segments[key], array))
					{
						segments[key].Add(array);
					}
				}
			}
		}
	}

	private static bool IsValidSegmentStartMapPoint(MapPoint startMapPoint)
	{
		if (startMapPoint.Children.Count <= 1)
		{
			return startMapPoint.coord.row == 0;
		}
		return true;
	}

	private static bool IsValidSegmentEndMapPoint(MapPoint endMapPoint)
	{
		return endMapPoint.parents.Count >= 2;
	}

	private static string GenerateSegmentKey(IReadOnlyList<MapPoint> segment)
	{
		StringBuilder stringBuilder = new StringBuilder();
		MapPoint mapPoint = segment[0];
		MapPoint mapPoint2 = segment[segment.Count - 1];
		if (mapPoint.coord.row == 0)
		{
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder3 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(3, 3, stringBuilder2);
			handler.AppendFormatted(mapPoint.coord.row);
			handler.AppendLiteral("-");
			handler.AppendFormatted(mapPoint2.coord.col);
			handler.AppendLiteral(",");
			handler.AppendFormatted(mapPoint2.coord.row);
			handler.AppendLiteral("-");
			stringBuilder3.Append(ref handler);
		}
		else
		{
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder4 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(4, 4, stringBuilder2);
			handler.AppendFormatted(mapPoint.coord.col);
			handler.AppendLiteral(",");
			handler.AppendFormatted(mapPoint.coord.row);
			handler.AppendLiteral("-");
			handler.AppendFormatted(mapPoint2.coord.col);
			handler.AppendLiteral(",");
			handler.AppendFormatted(mapPoint2.coord.row);
			handler.AppendLiteral("-");
			stringBuilder4.Append(ref handler);
		}
		stringBuilder.Append(string.Join(",", segment.Select((MapPoint point) => (int)point.PointType)));
		return stringBuilder.ToString();
	}

	private static bool AnyOverlappingSegments(IEnumerable<MapPoint[]> existingSegments, IReadOnlyList<MapPoint> segment)
	{
		return existingSegments.Any((MapPoint[] existingSegment) => OverlappingSegment(existingSegment, segment));
	}

	private static bool OverlappingSegment(IReadOnlyList<MapPoint> a, IReadOnlyList<MapPoint> b)
	{
		if (a.Count < 3 || b.Count < 3)
		{
			return false;
		}
		for (int i = 1; i <= a.Count - 2; i++)
		{
			if (object.Equals(a[i], b[i]))
			{
				return true;
			}
		}
		return false;
	}

	private static List<List<MapPoint[]>> GetDuplicateSegments(Dictionary<string, List<MapPoint[]>> segments)
	{
		return segments.Values.Where((List<MapPoint[]> segmentList) => segmentList.Count > 1).ToList();
	}

	private static bool PrunePaths(MapPoint?[,] grid, HashSet<MapPoint> startMapPoints, IEnumerable<List<MapPoint[]>> matchingSegments, Rng rng)
	{
		foreach (List<MapPoint[]> matchingSegment in matchingSegments)
		{
			matchingSegment.UnstableShuffle(rng);
			if (PruneAllButLast(grid, startMapPoints, matchingSegment) != 0)
			{
				return true;
			}
			if (BreakAParentChildRelationshipInAnySegment(matchingSegment))
			{
				return true;
			}
		}
		return false;
	}

	private static int PruneAllButLast(MapPoint?[,] grid, HashSet<MapPoint> startMapPoints, IReadOnlyList<MapPoint[]> matches)
	{
		int num = 0;
		foreach (MapPoint[] match in matches)
		{
			if (num == matches.Count - 1)
			{
				return num;
			}
			if (PruneSegment(grid, startMapPoints, match))
			{
				num++;
			}
		}
		return num;
	}

	private static bool PruneSegment(MapPoint?[,] grid, HashSet<MapPoint> startMapPoints, MapPoint[] segment)
	{
		bool result = false;
		for (int i = 0; i < segment.Length - 1; i++)
		{
			MapPoint mapPoint = segment[i];
			if (!IsInMap(grid, mapPoint))
			{
				return true;
			}
			if (mapPoint.Children.Count > 1 || mapPoint.parents.Count > 1 || mapPoint.parents.Any((MapPoint n) => n.Children.Count == 1 && !IsRemoved(grid, n)))
			{
				continue;
			}
			MapPoint[] source = segment.Skip(i).ToArray();
			if (!source.Any((MapPoint n) => n.Children.Count > 1 && n.parents.Count == 1))
			{
				if (segment[^1].parents.Count == 1)
				{
					return false;
				}
				if (!mapPoint.Children.Where((MapPoint c) => !segment.Contains(c)).Any((MapPoint c) => c.parents.Count == 1))
				{
					RemovePoint(grid, startMapPoints, mapPoint);
					result = true;
				}
			}
		}
		return result;
	}

	private static void RemovePoint(MapPoint?[,] grid, HashSet<MapPoint> startMapPoints, MapPoint mapPoint)
	{
		grid[mapPoint.coord.col, mapPoint.coord.row] = null;
		startMapPoints.Remove(mapPoint);
		foreach (MapPoint item in mapPoint.Children.ToList())
		{
			mapPoint.RemoveChildPoint(item);
		}
		foreach (MapPoint item2 in mapPoint.parents.ToList())
		{
			item2.RemoveChildPoint(mapPoint);
		}
	}

	private static bool IsInMap(MapPoint?[,] grid, MapPoint mapPoint)
	{
		if (grid[mapPoint.coord.col, mapPoint.coord.row] == null && mapPoint.PointType != MapPointType.Ancient)
		{
			return mapPoint.PointType == MapPointType.Boss;
		}
		return true;
	}

	private static bool IsRemoved(MapPoint?[,] grid, MapPoint mapPoint)
	{
		return grid[mapPoint.coord.col, mapPoint.coord.row] == null;
	}

	private static bool BreakAParentChildRelationshipInAnySegment(List<MapPoint[]> matches)
	{
		foreach (MapPoint[] match in matches)
		{
			if (BreakAParentChildRelationshipInSegment(match))
			{
				return true;
			}
		}
		return false;
	}

	private static bool BreakAParentChildRelationshipInSegment(MapPoint[] segment)
	{
		bool result = false;
		for (int i = 0; i < segment.Length - 1; i++)
		{
			MapPoint mapPoint = segment[i];
			if (mapPoint.Children.Count >= 2)
			{
				MapPoint mapPoint2 = segment[i + 1];
				if (mapPoint2.parents.Count != 1)
				{
					mapPoint.RemoveChildPoint(mapPoint2);
					result = true;
				}
			}
		}
		return result;
	}
}
