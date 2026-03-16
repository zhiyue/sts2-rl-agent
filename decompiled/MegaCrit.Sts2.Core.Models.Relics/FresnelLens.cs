using System.Collections.Generic;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class FresnelLens : RelicModel
{
	private const string _nimbleAmountKey = "NimbleAmount";

	public override RelicRarity Rarity => RelicRarity.Event;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("NimbleAmount", 2m));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromEnchantment<Nimble>(base.DynamicVars["NimbleAmount"].IntValue);

	public override bool TryModifyCardRewardOptionsLate(Player player, List<CardCreationResult> cardRewards, CardCreationOptions options)
	{
		if (player != base.Owner)
		{
			return false;
		}
		EnchantValidCards(cardRewards);
		return true;
	}

	public override void ModifyMerchantCardCreationResults(Player player, List<CardCreationResult> cards)
	{
		if (player == base.Owner)
		{
			EnchantValidCards(cards);
		}
	}

	public override bool TryModifyCardBeingAddedToDeck(CardModel card, out CardModel? newCard)
	{
		newCard = null;
		if (card.Owner != base.Owner)
		{
			return false;
		}
		if (!ModelDb.Enchantment<Nimble>().CanEnchant(card))
		{
			return false;
		}
		newCard = EnchantCard(card);
		return true;
	}

	private void EnchantValidCards(List<CardCreationResult> options)
	{
		Nimble nimble = ModelDb.Enchantment<Nimble>();
		foreach (CardCreationResult option in options)
		{
			CardModel card = option.Card;
			if (nimble.CanEnchant(card))
			{
				option.ModifyCard(EnchantCard(card), this);
			}
		}
	}

	private CardModel EnchantCard(CardModel card)
	{
		CardModel cardModel = base.Owner.RunState.CloneCard(card);
		CardCmd.Enchant<Nimble>(cardModel, base.DynamicVars["NimbleAmount"].BaseValue);
		return cardModel;
	}
}
