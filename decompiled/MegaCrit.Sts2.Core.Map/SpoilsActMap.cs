using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Map;

public sealed class SpoilsActMap : ActMap
{
	private const int _mapWidth = 7;

	private const int _pathCount = 7;

	private readonly int _mapLength;

	private readonly Rng _rng;

	private readonly int _treasureRow;

	private readonly MapPointTypeCounts _pointTypeCounts;

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

	protected override MapPoint?[,] Grid { get; }

	public SpoilsActMap(IRunState runState)
	{
		ActModel act = runState.Act;
		bool isMultiplayer = runState.Players.Count > 1;
		_mapLength = act.GetNumberOfRooms(isMultiplayer) + 1;
		_rng = new Rng(runState.Rng.Seed, "spoils_map");
		_pointTypeCounts = act.GetMapPointTypes(_rng);
		Grid = new MapPoint[7, _mapLength];
		_treasureRow = GetRowCount() - 7;
		BossMapPoint = new MapPoint(GetColumnCount() / 2, GetRowCount());
		StartingMapPoint = new MapPoint(GetColumnCount() / 2, 0);
		GenerateHourglassMap();
		AssignPointTypes();
		MapPathPruning.PruneDuplicateSegments(Grid, startMapPoints, StartingMapPoint, _rng);
	}

	private MapPoint GetOrCreatePoint(int col, int row)
	{
		if (col >= 0 && col < GetColumnCount() && row >= 0 && row < GetRowCount())
		{
			MapPoint mapPoint = Grid[col, row];
			if (mapPoint != null)
			{
				return mapPoint;
			}
		}
		MapPoint mapPoint2 = new MapPoint(col, row);
		Grid[col, row] = mapPoint2;
		return mapPoint2;
	}

	private void GenerateHourglassMap()
	{
		int col = GetColumnCount() / 2;
		if (_treasureRow <= 0 || _treasureRow >= GetRowCount())
		{
			throw new InvalidOperationException("Treasure row is out of bounds for SpoilsActMap");
		}
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
		MapPoint orCreatePoint2 = GetOrCreatePoint(col, _treasureRow);
		orCreatePoint2.PointType = MapPointType.Treasure;
		orCreatePoint2.CanBeModified = false;
		foreach (MapPoint item in GetPointsInRow(_treasureRow).ToList())
		{
			if (item != orCreatePoint2)
			{
				RedirectToTreasure(item, orCreatePoint2);
			}
		}
		ConnectRowToBoss();
		ConnectRowToStart();
	}

	private void PathGenerate(MapPoint startingPoint)
	{
		MapPoint mapPoint = startingPoint;
		while (mapPoint.coord.row < _mapLength - 1)
		{
			MapCoord mapCoord = GenerateNextCoord(mapPoint);
			MapPoint orCreatePoint = GetOrCreatePoint(mapCoord.col, mapCoord.row);
			mapPoint.AddChildPoint(orCreatePoint);
			mapPoint = orCreatePoint;
		}
	}

	private MapCoord GenerateNextCoord(MapPoint current)
	{
		int row = current.coord.row + 1;
		(int minCol, int maxCol) allowedColumnsForRow = GetAllowedColumnsForRow(row);
		int item = allowedColumnsForRow.minCol;
		int item2 = allowedColumnsForRow.maxCol;
		int num = GetColumnCount() / 2;
		List<int> list = new List<int> { -1, 0, 1 };
		int num2 = _treasureRow - current.coord.row;
		if (num2 > 3)
		{
			list.StableShuffle(_rng);
		}
		else if (num2 > 0)
		{
			list = BuildCenteredPriorityList(current.coord.col, num);
		}
		else
		{
			list.StableShuffle(_rng);
		}
		foreach (int item3 in list)
		{
			int nextColumn = GetNextColumn(current.coord.col, item3);
			if (nextColumn < item || nextColumn > item2 || HasInvalidCrossover(current, nextColumn))
			{
				continue;
			}
			MapPoint point = GetPoint(nextColumn, row);
			if ((point == null || point.parents.Contains(current) || point.parents.Count < 3) && (current == StartingMapPoint || current.Children.Count < 3 || (point != null && current.Children.Contains(point))))
			{
				if (Math.Abs(nextColumn - current.coord.col) > 1)
				{
					throw new InvalidOperationException($"Invalid step from ({current.coord.col}, {current.coord.row}) to column {nextColumn}");
				}
				return new MapCoord
				{
					col = nextColumn,
					row = row
				};
			}
		}
		int num3 = Math.Clamp(num, item, item2);
		if (Math.Abs(num3 - current.coord.col) > 1)
		{
			num3 = Math.Clamp(current.coord.col + Math.Sign(num3 - current.coord.col), item, item2);
		}
		if (HasInvalidCrossover(current, num3))
		{
			int num4 = Math.Clamp(current.coord.col, item, item2);
			num3 = num4;
		}
		if (Math.Abs(num3 - current.coord.col) > 1)
		{
			throw new InvalidOperationException($"Fallback step from ({current.coord.col}, {current.coord.row}) to column {num3} exceeds adjacency");
		}
		return new MapCoord
		{
			col = num3,
			row = row
		};
	}

	private List<int> BuildCenteredPriorityList(int currentCol, int centerCol)
	{
		List<int> list = new List<int>();
		int num = Math.Sign(centerCol - currentCol);
		if (num != 0)
		{
			list.Add(num);
		}
		list.Add(0);
		int item = -num;
		if (num != 0)
		{
			list.Add(item);
		}
		if (!list.Contains(-1))
		{
			list.Add(-1);
		}
		if (!list.Contains(1))
		{
			list.Add(1);
		}
		return list;
	}

	private int GetNextColumn(int currentCol, int direction)
	{
		return direction switch
		{
			-1 => Math.Max(0, currentCol - 1), 
			0 => currentCol, 
			1 => Math.Min(6, currentCol + 1), 
			_ => currentCol, 
		};
	}

	private (int minCol, int maxCol) GetAllowedColumnsForRow(int row)
	{
		int num = GetColumnCount() / 2;
		int val = Math.Abs(row - _treasureRow);
		int val2 = _mapLength - 1 - row;
		int val3 = Math.Min(num, Math.Max(0, val2) + 1);
		int num2 = Math.Min(num, Math.Min(val, val3));
		int item = Math.Max(0, num - num2);
		int item2 = Math.Min(6, num + num2);
		return (minCol: item, maxCol: item2);
	}

	private bool HasInvalidCrossover(MapPoint current, int targetCol)
	{
		int num = targetCol - current.coord.col;
		if (num == 0)
		{
			return false;
		}
		MapPoint point = GetPoint(targetCol, current.coord.row);
		if (point == null)
		{
			return false;
		}
		foreach (MapPoint child in point.Children)
		{
			int num2 = child.coord.col - point.coord.col;
			if (num2 == -num)
			{
				return true;
			}
		}
		return false;
	}

	private void RedirectToTreasure(MapPoint strayNode, MapPoint treasure)
	{
		foreach (MapPoint item in strayNode.parents.ToList())
		{
			item.RemoveChildPoint(strayNode);
			item.AddChildPoint(treasure);
		}
		foreach (MapPoint item2 in strayNode.Children.ToList())
		{
			strayNode.RemoveChildPoint(item2);
			treasure.AddChildPoint(item2);
		}
		Grid[strayNode.coord.col, strayNode.coord.row] = null;
	}

	private void ConnectRowToBoss()
	{
		int num = GetRowCount() - 1;
		for (int i = 0; i < GetColumnCount(); i++)
		{
			MapPoint mapPoint = Grid[i, num];
			if (mapPoint != null && !mapPoint.Children.Contains(BossMapPoint))
			{
				mapPoint.AddChildPoint(BossMapPoint);
			}
		}
	}

	private void ConnectRowToStart()
	{
		for (int i = 0; i < GetColumnCount(); i++)
		{
			MapPoint mapPoint = Grid[i, 1];
			if (mapPoint != null && !StartingMapPoint.Children.Contains(mapPoint))
			{
				StartingMapPoint.AddChildPoint(mapPoint);
			}
		}
	}

	private void AssignPointTypes()
	{
		ForEachInRow(GetRowCount() - 1, delegate(MapPoint p)
		{
			p.PointType = MapPointType.RestSite;
			p.CanBeModified = false;
		});
		ForEachInRow(1, delegate(MapPoint p)
		{
			if (p.PointType == MapPointType.Unassigned)
			{
				p.PointType = MapPointType.Monster;
			}
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
		foreach (MapPoint item in from p in GetAllMapPoints()
			where p.PointType == MapPointType.Unassigned
			select p)
		{
			item.PointType = MapPointType.Monster;
		}
		BossMapPoint.PointType = MapPointType.Boss;
		StartingMapPoint.PointType = MapPointType.Ancient;
	}

	private void ForEachInRow(int rowIndex, Action<MapPoint> processor)
	{
		for (int i = 0; i < GetColumnCount(); i++)
		{
			MapPoint mapPoint = Grid[i, rowIndex];
			if (mapPoint != null)
			{
				processor(mapPoint);
			}
		}
	}

	private void AssignRemainingTypesToRandomPoints(Queue<MapPointType> pointTypesToBeAssigned)
	{
		if (pointTypesToBeAssigned.Count == 0)
		{
			return;
		}
		List<MapPoint> list = (from p in GetAllMapPoints()
			where p != BossMapPoint && p != StartingMapPoint
			select p).ToList();
		list.StableShuffle(_rng);
		foreach (MapPoint item in list)
		{
			if (item.PointType == MapPointType.Unassigned)
			{
				MapPointType nextValidPointType = GetNextValidPointType(pointTypesToBeAssigned, item);
				if (nextValidPointType != MapPointType.Unassigned)
				{
					item.PointType = nextValidPointType;
				}
			}
		}
	}

	private MapPointType GetNextValidPointType(Queue<MapPointType> pointTypesQueue, MapPoint mapPoint)
	{
		if (pointTypesQueue.Count == 0)
		{
			return MapPointType.Unassigned;
		}
		int count = pointTypesQueue.Count;
		for (int i = 0; i < count; i++)
		{
			MapPointType mapPointType = pointTypesQueue.Dequeue();
			if (_pointTypeCounts.ShouldIgnoreMapPointRulesForMapPointType(mapPointType) || IsValidPointType(mapPointType, mapPoint))
			{
				return mapPointType;
			}
			pointTypesQueue.Enqueue(mapPointType);
		}
		return MapPointType.Unassigned;
	}

	private bool IsValidPointType(MapPointType pointType, MapPoint mapPoint)
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
