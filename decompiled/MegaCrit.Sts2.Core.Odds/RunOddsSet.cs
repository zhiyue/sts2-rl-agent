using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Odds;

public class RunOddsSet
{
	public UnknownMapPointOdds UnknownMapPoint { get; private init; }

	private RunOddsSet()
	{
	}

	public RunOddsSet(Rng unknownMapPointRng)
	{
		UnknownMapPoint = new UnknownMapPointOdds(unknownMapPointRng);
	}

	public SerializableRunOddsSet ToSerializable()
	{
		return new SerializableRunOddsSet
		{
			UnknownMapPointMonsterOddsValue = UnknownMapPoint.MonsterOdds,
			UnknownMapPointEliteOddsValue = UnknownMapPoint.EliteOdds,
			UnknownMapPointTreasureOddsValue = UnknownMapPoint.TreasureOdds,
			UnknownMapPointShopOddsValue = UnknownMapPoint.ShopOdds
		};
	}

	public static RunOddsSet FromSerializable(SerializableRunOddsSet save, Rng unknownMapPointRng)
	{
		return new RunOddsSet
		{
			UnknownMapPoint = new UnknownMapPointOdds(unknownMapPointRng)
			{
				MonsterOdds = save.UnknownMapPointMonsterOddsValue,
				EliteOdds = save.UnknownMapPointEliteOddsValue,
				TreasureOdds = save.UnknownMapPointTreasureOddsValue,
				ShopOdds = save.UnknownMapPointShopOddsValue
			}
		};
	}
}
