using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class DistinguishedCape : RelicModel
{
	public const int maxHpLoss = 9;

	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override bool HasUponPickupEffect => true;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromCardWithCardHoverTips<Apparition>();

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new HpLossVar(9m),
		new CardsVar(3)
	});

	public override async Task AfterObtained()
	{
		await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), base.Owner.Creature, base.DynamicVars.HpLoss.BaseValue, isFromCard: false);
		List<CardPileAddResult> results = new List<CardPileAddResult>();
		for (int i = 0; i < base.DynamicVars.Cards.IntValue; i++)
		{
			CardModel card = base.Owner.RunState.CreateCard<Apparition>(base.Owner);
			List<CardPileAddResult> list = results;
			list.Add(await CardPileCmd.Add(card, PileType.Deck));
		}
		CardCmd.PreviewCardPileAdd(results, 2f);
	}
}
