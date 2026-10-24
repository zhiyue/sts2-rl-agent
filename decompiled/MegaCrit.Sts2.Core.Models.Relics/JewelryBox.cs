using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class JewelryBox : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromCardWithCardHoverTips<Apotheosis>();

	public override bool HasUponPickupEffect => true;

	public override async Task AfterObtained()
	{
		CardModel card = base.Owner.RunState.CreateCard<Apotheosis>(base.Owner);
		CardCmd.PreviewCardPileAdd(new global::_003C_003Ez__ReadOnlySingleElementList<CardPileAddResult>(await CardPileCmd.Add(card, PileType.Deck)), 2f);
	}
}
