using System.Collections.Generic;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Map;

public static class MapTravel
{
	/// <summary>
	/// Get all the points on the map that are reachable from the specified point.
	/// </summary>
	/// <param name="runState">The state of the current run.</param>
	/// <param name="currentPoint">The current point that the players are at.</param>
	public static IEnumerable<MapPoint> GetTravelablePointsFrom(IRunState runState, MapPoint currentPoint)
	{
		if (Hook.ShouldAllowFreeTravel(runState))
		{
			return runState.Map.GetPointsInRow(currentPoint.coord.row + 1);
		}
		return currentPoint.Children;
	}
}
