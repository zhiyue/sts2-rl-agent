using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class MummifiedHand : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Rare;

	public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (!CombatManager.Instance.IsInProgress)
		{
			return Task.CompletedTask;
		}
		if (cardPlay.Card.Owner != base.Owner)
		{
			return Task.CompletedTask;
		}
		if (cardPlay.Card.Type != CardType.Power)
		{
			return Task.CompletedTask;
		}
		Rng combatCardSelection = base.Owner.RunState.Rng.CombatCardSelection;
		IReadOnlyList<CardModel> cards = PileType.Hand.GetPile(base.Owner).Cards;
		List<CardModel> list = cards.Where((CardModel c) => c.EnergyCost.GetWithModifiers(CostModifiers.None) > 0 || c.BaseStarCost > 0).ToList();
		CardModel cardModel = combatCardSelection.NextItem(list.Where((CardModel c) => c.CostsEnergyOrStars(includeGlobalModifiers: true)));
		if (cardModel == null)
		{
			cardModel = combatCardSelection.NextItem(cards.Where((CardModel c) => c.CostsEnergyOrStars(includeGlobalModifiers: true)));
		}
		if (cardModel == null)
		{
			cardModel = combatCardSelection.NextItem(list);
		}
		if (cardModel == null)
		{
			cardModel = combatCardSelection.NextItem(cards);
		}
		cardModel?.SetToFreeThisTurn();
		return Task.CompletedTask;
	}
}
