using System.Collections.Generic;

namespace MegaCrit.Sts2.Core.Map;

public abstract class ActMap
{
	public readonly HashSet<MapPoint> startMapPoints = new HashSet<MapPoint>();

	public abstract MapPoint BossMapPoint { get; }

	public abstract MapPoint StartingMapPoint { get; }

	public virtual MapPoint? SecondBossMapPoint => null;

	protected abstract MapPoint?[,] Grid { get; }

	public int GetColumnCount()
	{
		return Grid.GetLength(0);
	}

	public int GetRowCount()
	{
		return Grid.GetLength(1);
	}

	public IEnumerable<MapPoint> GetAllMapPoints()
	{
		for (int c = 0; c < GetColumnCount(); c++)
		{
			for (int r = 0; r < Grid.GetLength(1); r++)
			{
				MapPoint mapPoint = Grid[c, r];
				if (mapPoint != null)
				{
					yield return mapPoint;
				}
			}
		}
	}

	public IEnumerable<MapPoint> GetPointsInRow(int row)
	{
		if (row < 0 || row >= Grid.GetLength(1))
		{
			yield break;
		}
		for (int c = 0; c < GetColumnCount(); c++)
		{
			MapPoint mapPoint = Grid[c, row];
			if (mapPoint != null)
			{
				yield return mapPoint;
			}
		}
	}

	public virtual MapPoint? GetPoint(MapCoord coord)
	{
		return GetPoint(coord.col, coord.row);
	}

	protected MapPoint? GetPoint(int col, int row)
	{
		if (col == BossMapPoint.coord.col && row == BossMapPoint.coord.row)
		{
			return BossMapPoint;
		}
		if (SecondBossMapPoint != null && col == SecondBossMapPoint.coord.col && row == SecondBossMapPoint.coord.row)
		{
			return SecondBossMapPoint;
		}
		if (col == StartingMapPoint.coord.col && row == StartingMapPoint.coord.row)
		{
			return StartingMapPoint;
		}
		if (col >= 0 && col < Grid.GetLength(0) && row >= 0 && row < Grid.GetLength(1))
		{
			return Grid[col, row];
		}
		return null;
	}

	public bool IsInMap(MapPoint mapPoint)
	{
		if (mapPoint.PointType == MapPointType.Ancient || mapPoint.PointType == MapPointType.Boss)
		{
			return true;
		}
		int col = mapPoint.coord.col;
		int row = mapPoint.coord.row;
		if (col < 0 || col >= Grid.GetLength(0) || row < 0 || row >= Grid.GetLength(1))
		{
			return false;
		}
		return Grid[col, row] != null;
	}

	public bool HasPoint(MapCoord coord)
	{
		if (coord.col == BossMapPoint.coord.col && coord.row == BossMapPoint.coord.row)
		{
			return true;
		}
		if (SecondBossMapPoint != null && coord.col == SecondBossMapPoint.coord.col && coord.row == SecondBossMapPoint.coord.row)
		{
			return true;
		}
		if (coord.col == StartingMapPoint.coord.col && coord.row == StartingMapPoint.coord.row)
		{
			return true;
		}
		if (coord.col < 0 || coord.col >= Grid.GetLength(0))
		{
			return false;
		}
		if (coord.row < 0 || coord.row >= Grid.GetLength(1))
		{
			return false;
		}
		return Grid[coord.col, coord.row] != null;
	}
}
