using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Entities.RestSite;

public sealed class CookRestSiteOption : RestSiteOption
{
	private const int _cardsToRemove = 2;

	private const int _maxHpGain = 9;

	public override string OptionId => "COOK";

	public override LocString Description
	{
		get
		{
			if (base.IsEnabled)
			{
				LocString locString = new LocString("rest_site_ui", "OPTION_" + OptionId + ".description");
				locString.Add("Cards", 2m);
				locString.Add("MaxHp", 9m);
				return locString;
			}
			return new LocString("rest_site_ui", "OPTION_" + OptionId + ".descriptionDisabled");
		}
	}

	public CookRestSiteOption(Player owner)
		: base(owner)
	{
		base.IsEnabled = GetRemovableCardCount(owner) >= 2;
	}

	public override async Task<bool> OnSelect()
	{
		CardSelectorPrefs cardSelectorPrefs = new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 2);
		cardSelectorPrefs.Cancelable = true;
		cardSelectorPrefs.RequireManualConfirmation = true;
		CardSelectorPrefs prefs = cardSelectorPrefs;
		IEnumerable<CardModel> enumerable = await CardSelectCmd.FromDeckForRemoval(base.Owner, prefs);
		if (!enumerable.Any())
		{
			return false;
		}
		foreach (CardModel item in enumerable)
		{
			await CardPileCmd.RemoveFromDeck(item);
		}
		await CreatureCmd.GainMaxHp(base.Owner.Creature, 9m);
		return true;
	}

	private static int GetRemovableCardCount(Player player)
	{
		return PileType.Deck.GetPile(player).Cards.Count((CardModel c) => c.IsRemovable);
	}
}
