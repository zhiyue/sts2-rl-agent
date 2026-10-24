using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class LeafyPoultice : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new MaxHpVar(12m));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.Static(StaticHoverTip.Transform));

	public override async Task AfterObtained()
	{
		await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), base.Owner.Creature, base.DynamicVars.MaxHp.BaseValue, isFromCard: false);
		List<CardModel> source = PileType.Deck.GetPile(base.Owner).Cards.Where((CardModel c) => c.Rarity == CardRarity.Basic).ToList();
		CardModel cardModel = source.FirstOrDefault((CardModel c) => c.Tags.Contains(CardTag.Strike));
		CardModel cardModel2 = source.FirstOrDefault((CardModel c) => c.Tags.Contains(CardTag.Defend));
		List<CardTransformation> list = new List<CardTransformation>();
		if (cardModel != null)
		{
			list.Add(new CardTransformation(cardModel));
		}
		if (cardModel2 != null)
		{
			list.Add(new CardTransformation(cardModel2));
		}
		await CardCmd.Transform(list, base.Owner.PlayerRng.Transformations);
	}
}
