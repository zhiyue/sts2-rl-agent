using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.TestSupport;

public static class TestRngInjector
{
	private static RelicModel? _relicOverride;

	private static RelicRarity? _relicRarityOverride;

	public static void SetRelicOverride<T>() where T : RelicModel
	{
		_relicOverride = ModelDb.Relic<T>();
	}

	public static RelicModel? ConsumeRelicOverride()
	{
		RelicModel relicOverride = _relicOverride;
		_relicOverride = null;
		return relicOverride;
	}

	public static void SetRelicRarityOverride(RelicRarity relicRarity)
	{
		_relicRarityOverride = relicRarity;
	}

	public static RelicRarity? GetRelicRarityOverride()
	{
		return _relicRarityOverride;
	}

	public static void Cleanup()
	{
		_relicOverride = null;
	}
}
