using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class ArcaneScroll : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(1));

	public override async Task AfterObtained()
	{
		CardCreationOptions options = new CardCreationOptions(new global::_003C_003Ez__ReadOnlySingleElementList<CardPoolModel>(base.Owner.Character.CardPool), CardCreationSource.Other, CardRarityOddsType.Uniform, (CardModel c) => c.Rarity == CardRarity.Rare).WithFlags(CardCreationFlags.NoUpgradeRoll);
		List<CardModel> list = (from r in CardFactory.CreateForReward(base.Owner, base.DynamicVars.Cards.IntValue, options)
			select r.Card).ToList();
		if (list.Count > 0)
		{
			CardModel card = list[0];
			CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
		}
	}
}
