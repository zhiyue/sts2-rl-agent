namespace MegaCrit.Sts2.Core.Map;

public class MockActMap : ActMap
{
	private readonly MapPoint _currentMapPoint = new MapPoint(0, 0);

	public override MapPoint BossMapPoint { get; } = new MapPoint(0, 0)
	{
		PointType = MapPointType.Boss
	};

	public override MapPoint StartingMapPoint { get; } = new MapPoint(0, 0)
	{
		PointType = MapPointType.Ancient
	};

	protected override MapPoint?[,] Grid { get; } = new MapPoint[0, 0];

	public override MapPoint GetPoint(MapCoord coord)
	{
		return _currentMapPoint;
	}

	public void MockCurrentMapPointType(MapPointType pointType)
	{
		_currentMapPoint.PointType = pointType;
	}
}
