using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Modifiers;

public class Insanity : ModifierModel
{
	public override bool ClearsPlayerDeck => true;

	public override Func<Task> GenerateNeowOption(EventModel eventModel)
	{
		return () => ObtainCards(eventModel.Owner, eventModel.Rng);
	}

	private static async Task ObtainCards(Player player, Rng rng)
	{
		List<CardPileAddResult> results = new List<CardPileAddResult>();
		for (int i = 0; i < 30; i++)
		{
			CardCreationOptions options = CardCreationOptions.ForNonCombatWithUniformOdds(new global::_003C_003Ez__ReadOnlySingleElementList<CardPoolModel>(player.Character.CardPool)).WithFlags(CardCreationFlags.NoRarityModification);
			CardModel card = CardFactory.CreateForReward(player, 1, options).First().Card;
			results.Add(await CardPileCmd.Add(card, PileType.Deck));
		}
		foreach (CardPileAddResult item in results)
		{
			CardCmd.PreviewCardPileAdd(item, 1.2f, CardPreviewStyle.MessyLayout);
			await Cmd.CustomScaledWait(0.1f, 0.2f);
		}
		await Cmd.CustomScaledWait(0.6f, 1.2f);
		foreach (Player player2 in player.RunState.Players)
		{
			player2.RelicGrabBag.Remove<PandorasBox>();
		}
		player.RunState.SharedRelicGrabBag.Remove<PandorasBox>();
	}
}
