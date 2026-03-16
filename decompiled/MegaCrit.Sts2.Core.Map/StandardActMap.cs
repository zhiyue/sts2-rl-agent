using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Godot;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Map;

public sealed class StandardActMap : ActMap
{
	public const int maxElites = 15;

	private const int _iterations = 7;

	private const int _mapWidth = 7;

	private readonly MapPointTypeCounts _pointTypeCounts;

	private readonly int _mapLength;

	private readonly Rng _rng;

	private static readonly HashSet<MapPointType> _lowerMapPointRestrictions = new HashSet<MapPointType>
	{
		MapPointType.RestSite,
		MapPointType.Elite
	};

	private static readonly HashSet<MapPointType> _upperMapPointRestrictions = new HashSet<MapPointType> { MapPointType.RestSite };

	private static readonly HashSet<MapPointType> _parentMapPointRestrictions = new HashSet<MapPointType>
	{
		MapPointType.Elite,
		MapPointType.RestSite,
		MapPointType.Treasure,
		MapPointType.Shop
	};

	private static readonly HashSet<MapPointType> _childMapPointRestrictions = new HashSet<MapPointType>
	{
		MapPointType.Elite,
		MapPointType.RestSite,
		MapPointType.Treasure,
		MapPointType.Shop
	};

	private static readonly HashSet<MapPointType> _siblingPointTypeRestrictions = new HashSet<MapPointType>
	{
		MapPointType.RestSite,
		MapPointType.Monster,
		MapPointType.Unknown,
		MapPointType.Elite,
		MapPointType.Shop
	};

	public override MapPoint BossMapPoint { get; }

	public override MapPoint StartingMapPoint { get; }

	public override MapPoint? SecondBossMapPoint { get; }

	public bool ShouldReplaceTreasureWithElites { get; }

	protected override MapPoint?[,] Grid { get; }

	public StandardActMap(Rng mapRng, ActModel actModel, bool isMultiplayer, bool shouldReplaceTreasureWithElites, bool hasSecondBoss = false, MapPointTypeCounts? mapPointTypeCountsOverride = null, bool enablePruning = true)
	{
		_mapLength = actModel.GetNumberOfRooms(isMultiplayer) + 1;
		ShouldReplaceTreasureWithElites = shouldReplaceTreasureWithElites;
		Grid = new MapPoint[7, _mapLength];
		_rng = mapRng;
		_pointTypeCounts = mapPointTypeCountsOverride ?? actModel.GetMapPointTypes(mapRng);
		BossMapPoint = new MapPoint(GetColumnCount() / 2, GetRowCount());
		StartingMapPoint = new MapPoint(GetColumnCount() / 2, 0);
		if (hasSecondBoss)
		{
			SecondBossMapPoint = new MapPoint(GetColumnCount() / 2, GetRowCount() + 1);
		}
		GenerateMap();
		AssignPointTypes();
		if (enablePruning)
		{
			MapPathPruning.PruneDuplicateSegments(Grid, startMapPoints, StartingMapPoint, _rng);
		}
		Grid = MapPostProcessing.CenterGrid(Grid);
		Grid = MapPostProcessing.SpreadAdjacentMapPoints(Grid);
		Grid = MapPostProcessing.StraightenPaths(Grid);
	}

	public static StandardActMap CreateFor(RunState runState, bool replaceTreasureWithElites)
	{
		return new StandardActMap(new Rng(runState.Rng.Seed, $"act_{runState.CurrentActIndex + 1}_map"), runState.Act, runState.Players.Count > 1, replaceTreasureWithElites, runState.Act.HasSecondBoss);
	}

	private MapPoint GetOrCreateMapPoint(MapCoord coord)
	{
		return GetOrCreatePoint(coord.col, coord.row);
	}

	private MapPoint GetOrCreatePoint(int col, int row)
	{
		MapPoint point = GetPoint(col, row);
		if (point != null)
		{
			return point;
		}
		point = new MapPoint(col, row);
		Grid[col, row] = point;
		return point;
	}

	private void PathGenerate(MapPoint startingPoint)
	{
		MapPoint mapPoint = startingPoint;
		while (mapPoint.coord.row < _mapLength - 1)
		{
			MapCoord coord = GenerateNextCoord(mapPoint);
			MapPoint orCreateMapPoint = GetOrCreateMapPoint(coord);
			mapPoint.AddChildPoint(orCreateMapPoint);
			mapPoint = orCreateMapPoint;
		}
	}

	private MapCoord GenerateNextCoord(MapPoint current)
	{
		int col = current.coord.col;
		int num = Mathf.Max(0, col - 1);
		int num2 = Mathf.Min(col + 1, 6);
		int num3 = 3;
		List<int> list = new List<int>(num3);
		CollectionsMarshal.SetCount(list, num3);
		Span<int> span = CollectionsMarshal.AsSpan(list);
		int num4 = 0;
		span[num4] = -1;
		num4++;
		span[num4] = 0;
		num4++;
		span[num4] = 1;
		List<int> list2 = list;
		list2.StableShuffle(_rng);
		foreach (int item in list2)
		{
			int row = current.coord.row + 1;
			int num5 = item switch
			{
				-1 => num, 
				0 => col, 
				1 => num2, 
				_ => throw new InvalidOperationException("This isn't possible"), 
			};
			if (!HasInvalidCrossover(current, num5))
			{
				return new MapCoord
				{
					col = num5,
					row = row
				};
			}
		}
		throw new InvalidOperationException($"Cannot find next node: seed={_rng.Seed}");
	}

	private bool HasInvalidCrossover(MapPoint current, int targetX)
	{
		int num = targetX - current.coord.col;
		if ((num == 0 || num == 7) ? true : false)
		{
			return false;
		}
		MapPoint mapPoint = Grid[targetX, current.coord.row];
		if (mapPoint == null)
		{
			return false;
		}
		foreach (MapPoint child in mapPoint.Children)
		{
			int num2 = child.coord.col - mapPoint.coord.col;
			if (num2 == -num)
			{
				return true;
			}
		}
		return false;
	}

	private void GenerateMap()
	{
		for (int i = 0; i < 7; i++)
		{
			MapPoint orCreatePoint = GetOrCreatePoint(_rng.NextInt(0, 7), 1);
			if (i == 1)
			{
				while (startMapPoints.Contains(orCreatePoint))
				{
					orCreatePoint = GetOrCreatePoint(_rng.NextInt(0, 7), 1);
				}
			}
			startMapPoints.Add(orCreatePoint);
			PathGenerate(orCreatePoint);
		}
		ForEachInRow(Grid, GetRowCount() - 1, delegate(MapPoint x)
		{
			x.AddChildPoint(BossMapPoint);
		});
		if (SecondBossMapPoint != null)
		{
			BossMapPoint.AddChildPoint(SecondBossMapPoint);
		}
		ForEachInRow(Grid, 1, delegate(MapPoint x)
		{
			StartingMapPoint.AddChildPoint(x);
		});
	}

	private static void ForEachInRow(MapPoint?[,] grid, int rowIndex, Action<MapPoint> processor, bool canBeModified = false)
	{
		for (int i = 0; i < grid.GetLength(0); i++)
		{
			MapPoint mapPoint = grid[i, rowIndex];
			if (mapPoint != null)
			{
				processor(mapPoint);
			}
		}
	}

	private void AssignPointTypes()
	{
		ForEachInRow(Grid, GetRowCount() - 1, delegate(MapPoint p)
		{
			p.PointType = MapPointType.RestSite;
			p.CanBeModified = false;
		});
		if (ShouldReplaceTreasureWithElites)
		{
			ForEachInRow(Grid, GetRowCount() - 7, delegate(MapPoint p)
			{
				p.PointType = MapPointType.Elite;
				p.CanBeModified = false;
			});
		}
		else
		{
			ForEachInRow(Grid, GetRowCount() - 7, delegate(MapPoint p)
			{
				p.PointType = MapPointType.Treasure;
				p.CanBeModified = false;
			});
		}
		ForEachInRow(Grid, 1, delegate(MapPoint p)
		{
			p.PointType = MapPointType.Monster;
		});
		List<MapPointType> list = new List<MapPointType>();
		for (int num = 0; num < _pointTypeCounts.NumOfRests; num++)
		{
			list.Add(MapPointType.RestSite);
		}
		for (int num2 = 0; num2 < _pointTypeCounts.NumOfShops; num2++)
		{
			list.Add(MapPointType.Shop);
		}
		for (int num3 = 0; num3 < _pointTypeCounts.NumOfElites; num3++)
		{
			list.Add(MapPointType.Elite);
		}
		for (int num4 = 0; num4 < _pointTypeCounts.NumOfUnknowns; num4++)
		{
			list.Add(MapPointType.Unknown);
		}
		Queue<MapPointType> pointTypesToBeAssigned = new Queue<MapPointType>(list);
		AssignRemainingTypesToRandomPoints(pointTypesToBeAssigned);
		foreach (MapPoint item in from x in GetAllMapPoints()
			where x.PointType == MapPointType.Unassigned
			select x)
		{
			item.PointType = MapPointType.Monster;
		}
		BossMapPoint.PointType = MapPointType.Boss;
		StartingMapPoint.PointType = MapPointType.Ancient;
		if (SecondBossMapPoint != null)
		{
			SecondBossMapPoint.PointType = MapPointType.Boss;
		}
	}

	private void EnsureRowsContainsPointType(MapPointType pointType, List<List<MapPoint>> rows)
	{
		if (!RowsContainPointType(pointType, rows))
		{
			Queue<MapPointType> queue = new Queue<MapPointType>();
			queue.Enqueue(pointType);
			AssignPointTypesToRandomRows(queue, rows);
		}
	}

	private static bool RowsContainPointType(MapPointType pointType, IEnumerable<List<MapPoint>> rows)
	{
		return rows.SelectMany((List<MapPoint> row) => row.Where((MapPoint p) => p.PointType == pointType)).Any();
	}

	private List<List<MapPoint>> GetRows(int firstRow, int lastRow)
	{
		List<List<MapPoint>> list = new List<List<MapPoint>>();
		for (int i = firstRow; i <= lastRow; i++)
		{
			List<MapPoint> list2 = new List<MapPoint>();
			for (int j = 0; j < 7; j++)
			{
				MapPoint mapPoint = Grid[j, i];
				if (mapPoint != null)
				{
					list2.Add(mapPoint);
				}
			}
			list.Add(list2);
		}
		return list;
	}

	private void AssignPointTypesToRandomRows(Queue<MapPointType> pointTypesToBeAssigned, List<List<MapPoint>> rows)
	{
		rows.UnstableShuffle(_rng);
		foreach (List<MapPoint> row in rows)
		{
			row.StableShuffle(_rng);
			using IEnumerator<MapPoint> enumerator2 = row.Where((MapPoint r) => r.PointType == MapPointType.Unassigned).GetEnumerator();
			if (enumerator2.MoveNext())
			{
				MapPoint current2 = enumerator2.Current;
				MapPointType nextValidPointType = GetNextValidPointType(pointTypesToBeAssigned, current2);
				current2.PointType = nextValidPointType;
			}
		}
	}

	private void AssignRemainingTypesToRandomPoints(Queue<MapPointType> pointTypesToBeAssigned)
	{
		List<MapPoint> list = GetAllMapPoints().ToList();
		list.StableShuffle(_rng);
		foreach (MapPoint item in list.Where((MapPoint p) => p.PointType == MapPointType.Unassigned))
		{
			item.PointType = GetNextValidPointType(pointTypesToBeAssigned, item);
		}
	}

	private MapPointType GetNextValidPointType(Queue<MapPointType> pointTypesQueue, MapPoint mapPoint)
	{
		for (int i = 0; i < pointTypesQueue.Count; i++)
		{
			MapPointType mapPointType = pointTypesQueue.Dequeue();
			if (_pointTypeCounts.ShouldIgnoreMapPointRulesForMapPointType(mapPointType))
			{
				return mapPointType;
			}
			if (IsValidPointType(mapPointType, mapPoint))
			{
				return mapPointType;
			}
			pointTypesQueue.Enqueue(mapPointType);
		}
		return MapPointType.Unassigned;
	}

	public bool IsValidPointType(MapPointType pointType, MapPoint mapPoint)
	{
		if (!IsValidForUpper(pointType, mapPoint))
		{
			return false;
		}
		if (!IsValidForLower(pointType, mapPoint))
		{
			return false;
		}
		if (!IsValidWithParents(pointType, mapPoint))
		{
			return false;
		}
		if (!IsValidWithChildren(pointType, mapPoint))
		{
			return false;
		}
		if (!IsValidWithSiblings(pointType, mapPoint))
		{
			return false;
		}
		return true;
	}

	private static bool IsValidForLower(MapPointType pointType, MapPoint mapPoint)
	{
		if (mapPoint.coord.row < 5)
		{
			return !_lowerMapPointRestrictions.Contains(pointType);
		}
		return true;
	}

	private bool IsValidForUpper(MapPointType pointType, MapPoint mapPoint)
	{
		if (mapPoint.coord.row >= _mapLength - 3)
		{
			return !_upperMapPointRestrictions.Contains(pointType);
		}
		return true;
	}

	private static bool IsValidWithParents(MapPointType pointType, MapPoint mapPoint)
	{
		if (_parentMapPointRestrictions.Contains(pointType))
		{
			return !mapPoint.parents.Concat(mapPoint.Children).Any((MapPoint p) => pointType == p.PointType);
		}
		return true;
	}

	private static bool IsValidWithChildren(MapPointType pointType, MapPoint mapPoint)
	{
		if (_childMapPointRestrictions.Contains(pointType))
		{
			return !mapPoint.Children.Any((MapPoint p) => pointType == p.PointType);
		}
		return true;
	}

	private static bool IsValidWithSiblings(MapPointType pointType, MapPoint mapPoint)
	{
		if (_siblingPointTypeRestrictions.Contains(pointType))
		{
			return !GetSiblings(mapPoint).Any((MapPoint p) => pointType == p.PointType);
		}
		return true;
	}

	private static IEnumerable<MapPoint> GetSiblings(MapPoint mapPoint)
	{
		return from x in mapPoint.parents.SelectMany((MapPoint x) => x.Children)
			where !object.Equals(x, mapPoint)
			select x;
	}
}
