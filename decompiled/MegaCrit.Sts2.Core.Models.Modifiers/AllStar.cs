using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Modifiers;

public class AllStar : ModifierModel
{
	public override Func<Task> GenerateNeowOption(EventModel eventModel)
	{
		return () => ObtainCards(eventModel.Owner, eventModel.Rng);
	}

	private static async Task ObtainCards(Player player, Rng rng)
	{
		List<CardPileAddResult> results = new List<CardPileAddResult>();
		for (int i = 0; i < 5; i++)
		{
			CardCreationOptions options = CardCreationOptions.ForNonCombatWithUniformOdds(new global::_003C_003Ez__ReadOnlySingleElementList<CardPoolModel>(ModelDb.CardPool<ColorlessCardPool>())).WithFlags(CardCreationFlags.NoRarityModification | CardCreationFlags.NoCardPoolModifications);
			CardModel card = CardFactory.CreateForReward(player, 1, options).First().Card;
			results.Add(await CardPileCmd.Add(card, PileType.Deck));
		}
		CardCmd.PreviewCardPileAdd(results);
		await Cmd.CustomScaledWait(0.6f, 1.2f);
	}
}
