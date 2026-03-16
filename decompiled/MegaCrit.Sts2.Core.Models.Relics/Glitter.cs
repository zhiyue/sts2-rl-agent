using System.Collections.Generic;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class Glitter : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromEnchantment<Glam>();

	public override bool TryModifyCardRewardOptionsLate(Player player, List<CardCreationResult> cardRewards, CardCreationOptions options)
	{
		if (player != base.Owner)
		{
			return false;
		}
		Glam glam = ModelDb.Enchantment<Glam>();
		foreach (CardCreationResult cardReward in cardRewards)
		{
			CardModel card = cardReward.Card;
			if (glam.CanEnchant(card))
			{
				CardModel card2 = base.Owner.RunState.CloneCard(card);
				CardCmd.Enchant<Glam>(card2, 1m);
				cardReward.ModifyCard(card2, this);
			}
		}
		return true;
	}
}
