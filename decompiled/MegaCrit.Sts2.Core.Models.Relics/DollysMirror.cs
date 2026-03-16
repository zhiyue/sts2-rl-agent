using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class DollysMirror : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Shop;

	public override bool HasUponPickupEffect => true;

	public override async Task AfterObtained()
	{
		CardModel cardModel = (await CardSelectCmd.FromDeckGeneric(prefs: new CardSelectorPrefs(base.SelectionScreenPrompt, 1), player: base.Owner, filter: Filter)).FirstOrDefault();
		if (cardModel != null)
		{
			CardModel card = base.Owner.RunState.CloneCard(cardModel);
			CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
		}
	}

	private bool Filter(CardModel c)
	{
		return c.Type != CardType.Quest;
	}
}
