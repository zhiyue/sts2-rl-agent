using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class Orrery : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Shop;

	public override bool HasUponPickupEffect => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(5));

	public override async Task AfterObtained()
	{
		List<Reward> list = new List<Reward>();
		CardCreationOptions options = new CardCreationOptions(new global::_003C_003Ez__ReadOnlySingleElementList<CardPoolModel>(base.Owner.Character.CardPool), CardCreationSource.Other, CardRarityOddsType.RegularEncounter);
		for (int i = 0; i < base.DynamicVars.Cards.IntValue; i++)
		{
			list.Add(new CardReward(options, 3, base.Owner));
		}
		await RewardsCmd.OfferCustom(base.Owner, list);
	}
}
