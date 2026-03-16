using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Map;

public class MapPointTypeCounts
{
	public HashSet<MapPointType> PointTypesThatIgnoreRules { get; init; } = new HashSet<MapPointType>();

	public int NumOfElites { get; init; }

	public int NumOfShops { get; init; }

	public int NumOfUnknowns { get; init; }

	public int NumOfRests { get; init; }

	public bool ShouldIgnoreMapPointRulesForMapPointType(MapPointType pointType)
	{
		return PointTypesThatIgnoreRules.Contains(pointType);
	}

	public MapPointTypeCounts(Rng rng)
	{
		NumOfElites = (int)Math.Round(5f * (AscensionHelper.HasAscension(AscensionLevel.SwarmingElites) ? 1.6f : 1f));
		NumOfShops = 3;
		NumOfUnknowns = rng.NextGaussianInt(12, 1, 10, 14);
		NumOfRests = rng.NextGaussianInt(5, 1, 3, 6);
	}
}
