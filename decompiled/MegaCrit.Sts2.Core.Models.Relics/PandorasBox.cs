using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class PandorasBox : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override bool HasUponPickupEffect => true;

	public override async Task AfterObtained()
	{
		List<CardModel> source = PileType.Deck.GetPile(base.Owner).Cards.Where((CardModel c) => c != null && c.IsBasicStrikeOrDefend && c.IsRemovable).ToList();
		IEnumerable<CardTransformation> transformations = source.Select((CardModel c) => new CardTransformation(c, CardFactory.CreateRandomCardForTransform(c, isInCombat: false, base.Owner.RunState.Rng.Niche)));
		List<CardPileAddResult> list = (await CardCmd.Transform(transformations, null, CardPreviewStyle.None)).ToList();
		if (list.Count > 0 && LocalContext.IsMe(base.Owner))
		{
			NSimpleCardsViewScreen.ShowScreen(list, new LocString("relics", "PANDORAS_BOX.infoText"));
		}
	}
}
