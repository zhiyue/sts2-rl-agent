using System.Collections.Generic;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Helpers.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class ToxicEgg : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Rare;

	public override bool IsAllowed(IRunState runState)
	{
		return RelicModel.IsBeforeAct3TreasureChest(runState);
	}

	public override bool TryModifyCardRewardOptionsLate(Player player, List<CardCreationResult> cardRewards, CardCreationOptions options)
	{
		if (player != base.Owner)
		{
			return false;
		}
		if (options.Flags.HasFlag(CardCreationFlags.NoHookUpgrades))
		{
			return false;
		}
		EggRelicHelper.UpgradeValidCards(cardRewards, CardType.Skill, this);
		return true;
	}

	public override void ModifyMerchantCardCreationResults(Player player, List<CardCreationResult> cards)
	{
		if (player == base.Owner)
		{
			EggRelicHelper.UpgradeValidCards(cards, CardType.Skill, this);
		}
	}

	public override bool TryModifyCardBeingAddedToDeck(CardModel card, out CardModel? newCard)
	{
		newCard = null;
		if (card.Owner != base.Owner)
		{
			return false;
		}
		if (card.Type != CardType.Skill)
		{
			return false;
		}
		if (!card.IsUpgradable)
		{
			return false;
		}
		newCard = base.Owner.RunState.CloneCard(card);
		CardCmd.Upgrade(newCard, CardPreviewStyle.None);
		return true;
	}
}
