using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class LostCoffer : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override bool HasUponPickupEffect => true;

	public override async Task AfterObtained()
	{
		List<Reward> list = new List<Reward>();
		CardCreationOptions options = new CardCreationOptions(new global::_003C_003Ez__ReadOnlySingleElementList<CardPoolModel>(base.Owner.Character.CardPool), CardCreationSource.Other, CardRarityOddsType.RegularEncounter);
		list.Add(new CardReward(options, 3, base.Owner));
		list.Add(new PotionReward(base.Owner));
		await RewardsCmd.OfferCustom(base.Owner, list);
	}
}
