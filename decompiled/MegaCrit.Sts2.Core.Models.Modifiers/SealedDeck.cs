using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Modifiers;

public class SealedDeck : ModifierModel
{
	public override bool ClearsPlayerDeck => true;

	public override Func<Task> GenerateNeowOption(EventModel eventModel)
	{
		return () => ChooseCards(eventModel.Owner);
	}

	private static async Task ChooseCards(Player player)
	{
		CardCreationOptions options = new CardCreationOptions(new global::_003C_003Ez__ReadOnlySingleElementList<CardPoolModel>(player.Character.CardPool), CardCreationSource.Other, CardRarityOddsType.RegularEncounter).WithFlags(CardCreationFlags.NoUpgradeRoll | CardCreationFlags.ForceRarityOddsChange);
		IEnumerable<CardCreationResult> source = CardFactory.CreateForReward(player, 30, options).ToList();
		source = from r in source
			orderby r.Card.Rarity, r.Card.Title
			select r;
		CardSelectorPrefs cardSelectorPrefs = new CardSelectorPrefs(new LocString("modifiers", "SEALED_DECK.selectionPrompt"), 10);
		cardSelectorPrefs.Cancelable = false;
		cardSelectorPrefs.RequireManualConfirmation = true;
		CardSelectorPrefs prefs = cardSelectorPrefs;
		List<CardModel> cards = (await CardSelectCmd.FromSimpleGridForRewards(new BlockingPlayerChoiceContext(), source.ToList(), player, prefs)).ToList();
		CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(cards, PileType.Deck), 1.2f, CardPreviewStyle.GridLayout);
		foreach (Player player2 in player.RunState.Players)
		{
			player2.RelicGrabBag.Remove<PandorasBox>();
		}
		player.RunState.SharedRelicGrabBag.Remove<PandorasBox>();
	}
}
