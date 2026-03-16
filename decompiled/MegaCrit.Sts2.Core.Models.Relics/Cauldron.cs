using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class Cauldron : RelicModel
{
	private const string _potionsKey = "Potions";

	private static readonly PotionModel[] _testPotions = new PotionModel[5]
	{
		ModelDb.Potion<FlexPotion>(),
		ModelDb.Potion<WeakPotion>(),
		ModelDb.Potion<VulnerablePotion>(),
		ModelDb.Potion<StrengthPotion>(),
		ModelDb.Potion<DexterityPotion>()
	};

	public override RelicRarity Rarity => RelicRarity.Shop;

	public override bool HasUponPickupEffect => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("Potions", 5m));

	public override async Task AfterObtained()
	{
		await RewardsCmd.OfferCustom(base.Owner, GenerateRewards());
	}

	private List<Reward> GenerateRewards()
	{
		int intValue = base.DynamicVars["Potions"].IntValue;
		List<Reward> list = new List<Reward>();
		if (TestMode.IsOn)
		{
			for (int i = 0; i < intValue; i++)
			{
				list.Add(new PotionReward(_testPotions[i % intValue].ToMutable(), base.Owner));
			}
		}
		else
		{
			for (int j = 0; j < intValue; j++)
			{
				list.Add(new PotionReward(base.Owner));
			}
		}
		return list;
	}
}
