using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class PaelsHorn : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromCardWithCardHoverTips<Relax>();

	public override async Task AfterObtained()
	{
		List<CardPileAddResult> results = new List<CardPileAddResult>();
		for (int i = 0; i < 2; i++)
		{
			CardModel card = base.Owner.RunState.CreateCard<Relax>(base.Owner);
			results.Add(await CardPileCmd.Add(card, PileType.Deck));
		}
		CardCmd.PreviewCardPileAdd(results, 2f);
	}
}
