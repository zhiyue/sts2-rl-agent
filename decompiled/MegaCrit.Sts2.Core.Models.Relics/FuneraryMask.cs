using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class FuneraryMask : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Uncommon;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(3));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromCard<Soul>());

	public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
	{
		if (side == CombatSide.Player && combatState.RoundNumber <= 1)
		{
			Flash();
			for (int i = 0; (decimal)i < base.DynamicVars.Cards.BaseValue; i++)
			{
				CardModel card = combatState.CreateCard(ModelDb.Card<Soul>(), base.Owner);
				CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Draw, addedByPlayer: true, CardPilePosition.Random));
			}
		}
	}
}
