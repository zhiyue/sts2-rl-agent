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

	public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner != base.Owner)
		{
			return Task.CompletedTask;
		}
		if (!CombatManager.Instance.IsInProgress)
		{
			return Task.CompletedTask;
		}
		if (cardPlay.Card.Type != CardType.Power)
		{
			return Task.CompletedTask;
		}
		IReadOnlyList<CardModel> cards = PileType.Hand.GetPile(base.Owner).Cards;
		Rng combatCardSelection = base.Owner.RunState.Rng.CombatCardSelection;
		CardModel cardModel = combatCardSelection.NextItem(cards.Where((CardModel c) => c.CostsEnergyOrStars(includeGlobalModifiers: false)));
		if (cardModel == null)
		{
			combatCardSelection.NextItem(cards.Where((CardModel c) => c.CostsEnergyOrStars(includeGlobalModifiers: true)));
		}
		cardModel?.SetToFreeThisTurn();
		return Task.CompletedTask;
	}
}
