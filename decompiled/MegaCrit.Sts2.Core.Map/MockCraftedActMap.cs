namespace MegaCrit.Sts2.Core.Map;

/// <summary>
/// A map specifically for tests that allows you to specify the size and contents of the map.
/// </summary>
public sealed class MockCraftedActMap : ActMap
{
	public override MapPoint BossMapPoint { get; }

	public override MapPoint StartingMapPoint { get; }

	protected override MapPoint?[,] Grid { get; }

	public MockCraftedActMap(int width, int height, MapPoint startingPoint, MapPoint bossPoint)
	{
		Grid = new MapPoint[width, height];
		StartingMapPoint = startingPoint;
		BossMapPoint = bossPoint;
	}

	/// <summary>
	/// Creates a new point with the specified type at the specified coordinates.
	/// </summary>
	public void Put(int col, int row, MapPointType type = MapPointType.Monster)
	{
		MapPoint mapPoint = new MapPoint(col, row)
		{
			PointType = type
		};
		Grid[col, row] = mapPoint;
	}
}
