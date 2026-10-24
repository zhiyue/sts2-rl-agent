namespace MegaCrit.Sts2.Core.Map;

/// <summary>
/// An empty map used as a placeholder before a real map is generated.
/// </summary>
public class NullActMap : ActMap
{
	public static NullActMap Instance { get; } = new NullActMap();

	public override MapPoint BossMapPoint { get; } = new MapPoint(0, 0);

	public override MapPoint StartingMapPoint { get; } = new MapPoint(0, 0);

	protected override MapPoint?[,] Grid { get; } = new MapPoint[0, 0];
}
