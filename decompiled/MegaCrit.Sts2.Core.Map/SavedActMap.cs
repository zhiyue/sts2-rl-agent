using System.Collections.Generic;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Map;

public sealed class SavedActMap : ActMap
{
	public override MapPoint BossMapPoint { get; }

	public override MapPoint StartingMapPoint { get; }

	public override MapPoint? SecondBossMapPoint { get; }

	protected override MapPoint?[,] Grid { get; }

	public SavedActMap(SerializableActMap saved)
	{
		Grid = new MapPoint[saved.GridWidth, saved.GridHeight];
		Dictionary<MapCoord, MapPoint> dictionary = new Dictionary<MapCoord, MapPoint>();
		foreach (SerializableMapPoint point in saved.Points)
		{
			MapPoint mapPoint = CreatePoint(point);
			Grid[point.Coord.col, point.Coord.row] = mapPoint;
			dictionary[point.Coord] = mapPoint;
		}
		BossMapPoint = CreatePoint(saved.BossPoint);
		dictionary[saved.BossPoint.Coord] = BossMapPoint;
		StartingMapPoint = CreatePoint(saved.StartingPoint);
		dictionary[saved.StartingPoint.Coord] = StartingMapPoint;
		if (saved.SecondBossPoint != null)
		{
			SecondBossMapPoint = CreatePoint(saved.SecondBossPoint);
			dictionary[saved.SecondBossPoint.Coord] = SecondBossMapPoint;
		}
		WireChildren(saved.Points, dictionary);
		WireChildren(saved.BossPoint, dictionary);
		WireChildren(saved.StartingPoint, dictionary);
		if (saved.SecondBossPoint != null)
		{
			WireChildren(saved.SecondBossPoint, dictionary);
		}
		if (saved.StartMapPointCoords == null)
		{
			return;
		}
		foreach (MapCoord startMapPointCoord in saved.StartMapPointCoords)
		{
			if (dictionary.TryGetValue(startMapPointCoord, out var value))
			{
				startMapPoints.Add(value);
			}
		}
	}

	private static MapPoint CreatePoint(SerializableMapPoint saved)
	{
		return new MapPoint(saved.Coord.col, saved.Coord.row)
		{
			PointType = saved.PointType,
			CanBeModified = saved.CanBeModified
		};
	}

	private static void WireChildren(IEnumerable<SerializableMapPoint> points, Dictionary<MapCoord, MapPoint> lookup)
	{
		foreach (SerializableMapPoint point in points)
		{
			WireChildren(point, lookup);
		}
	}

	private static void WireChildren(SerializableMapPoint savedPoint, Dictionary<MapCoord, MapPoint> lookup)
	{
		if (savedPoint.ChildCoords == null)
		{
			return;
		}
		MapPoint mapPoint = lookup[savedPoint.Coord];
		foreach (MapCoord childCoord in savedPoint.ChildCoords)
		{
			if (lookup.TryGetValue(childCoord, out MapPoint value))
			{
				mapPoint.AddChildPoint(value);
			}
		}
	}
}
