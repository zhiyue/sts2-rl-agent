using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class GlassEye : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override async Task AfterObtained()
	{
		List<Reward> list = new List<Reward>();
		CardRarity[] array = new CardRarity[5]
		{
			CardRarity.Common,
			CardRarity.Common,
			CardRarity.Uncommon,
			CardRarity.Uncommon,
			CardRarity.Rare
		};
		CardRarity[] array2 = array;
		foreach (CardRarity rarity in array2)
		{
			CardCreationOptions options = CardCreationOptions.ForNonCombatWithUniformOdds(new global::_003C_003Ez__ReadOnlySingleElementList<CardPoolModel>(base.Owner.Character.CardPool), (CardModel c) => c.Rarity == rarity).WithFlags(CardCreationFlags.NoRarityModification);
			list.Add(new CardReward(options, 3, base.Owner));
		}
		await RewardsCmd.OfferCustom(base.Owner, list);
	}
}
