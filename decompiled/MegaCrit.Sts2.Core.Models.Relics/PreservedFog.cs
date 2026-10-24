using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class PreservedFog : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromCardWithCardHoverTips<Folly>();

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(3));

	public override async Task AfterObtained()
	{
		foreach (CardModel item in await CardSelectCmd.FromDeckForRemoval(prefs: new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, base.DynamicVars.Cards.IntValue), player: base.Owner))
		{
			await CardPileCmd.RemoveFromDeck(item);
		}
		await CardPileCmd.AddCurseToDeck<Folly>(base.Owner);
	}
}
