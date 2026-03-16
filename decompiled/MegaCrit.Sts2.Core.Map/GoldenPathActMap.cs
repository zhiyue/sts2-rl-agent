using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Map;

public sealed class GoldenPathActMap : ActMap
{
	private readonly MapPointType[] _defaultPointTypes = new MapPointType[16]
	{
		MapPointType.Monster,
		MapPointType.Unknown,
		MapPointType.Monster,
		MapPointType.RestSite,
		MapPointType.Monster,
		MapPointType.RestSite,
		MapPointType.Unknown,
		MapPointType.Treasure,
		MapPointType.Unknown,
		MapPointType.Treasure,
		MapPointType.Unknown,
		MapPointType.Shop,
		MapPointType.Elite,
		MapPointType.RestSite,
		MapPointType.Elite,
		MapPointType.RestSite
	};

	private const int _width = 7;

	private const int _middle = 3;

	public override MapPoint BossMapPoint { get; }

	public override MapPoint StartingMapPoint { get; }

	protected override MapPoint?[,] Grid { get; }

	public GoldenPathActMap(IRunState runState)
	{
		List<MapPointType> list = _defaultPointTypes.ToList();
		if (runState.Players.Count > 1)
		{
			list.RemoveAt(2);
		}
		Grid = new MapPoint[7, list.Count + 1];
		BossMapPoint = new MapPoint(GetColumnCount() / 2, GetRowCount())
		{
			PointType = MapPointType.Boss
		};
		StartingMapPoint = new MapPoint(GetColumnCount() / 2, 0)
		{
			PointType = MapPointType.Ancient
		};
		for (int i = 0; i < list.Count; i++)
		{
			MapPoint mapPoint = new MapPoint(3, i + 1);
			Grid[3, i + 1] = mapPoint;
			mapPoint.PointType = list[i];
			if (i > 0)
			{
				Grid[3, i].AddChildPoint(mapPoint);
			}
		}
		startMapPoints.Add(Grid[3, 1]);
		Grid[3, GetRowCount() - 1].AddChildPoint(BossMapPoint);
		StartingMapPoint.AddChildPoint(Grid[3, 1]);
	}
}
