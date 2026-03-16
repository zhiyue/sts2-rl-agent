using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class PowerCell : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Rare;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(2));

	public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
	{
		if (side == CombatSide.Player && combatState.RoundNumber <= 1)
		{
			Flash();
			IEnumerable<CardModel> cards = PileType.Draw.GetPile(base.Owner).Cards.Where((CardModel c) => !c.EnergyCost.CostsX && c.EnergyCost.GetWithModifiers(CostModifiers.Local) == 0).ToList().StableShuffle(base.Owner.RunState.Rng.CombatCardSelection)
				.Take(base.DynamicVars.Cards.IntValue);
			await CardPileCmd.Add(cards, PileType.Hand);
		}
	}
}
