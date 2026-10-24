using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Map;

public class MapPointTypeCounts
{
	public HashSet<MapPointType> PointTypesThatIgnoreRules { get; init; } = new HashSet<MapPointType>();

	public int NumOfElites { get; init; } = (int)Math.Round(5f * (AscensionHelper.HasAscension(AscensionLevel.SwarmingElites) ? 1.6f : 1f));

	public int NumOfShops { get; } = 3;

	public int NumOfUnknowns { get; }

	public int NumOfRests { get; }

	/// <summary>
	/// Whether the map generation MapPointType assignment ignores the rules or follows the rules.
	/// </summary>
	public bool ShouldIgnoreMapPointRulesForMapPointType(MapPointType pointType)
	{
		return PointTypesThatIgnoreRules.Contains(pointType);
	}

	public static int StandardRandomUnknownCount(Rng rng)
	{
		return rng.NextGaussianInt(12, 1, 10, 14);
	}

	public MapPointTypeCounts(int unknownCount, int restCount)
	{
		NumOfUnknowns = unknownCount;
		NumOfRests = restCount;
	}

	public MapPointTypeCounts(ActMap existingMap)
	{
		MapPoint[] source = existingMap.GetAllMapPoints().ToArray();
		NumOfUnknowns = source.Count((MapPoint p) => p.PointType == MapPointType.Unknown);
		NumOfRests = source.Count((MapPoint p) => p.PointType == MapPointType.RestSite);
	}
}
