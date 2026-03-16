using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class RemoveCardConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "remove_card";

	public override string Args => "<id:string> [pileName:string]";

	public override string Description => "Removes a card from your Hand or Deck. Screaming snake case ('BODY_SLAM', not 'Body Slam').";

	public override bool IsNetworked => true;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (args.Length == 0)
		{
			return new CmdResult(success: false, "No card name specified.");
		}
		if (!RunManager.Instance.IsInProgress)
		{
			return new CmdResult(success: false, "A run is currently not in progress!");
		}
		string cardName = args[0].ToUpperInvariant();
		CardModel cardModel = ModelDb.AllCards.FirstOrDefault((CardModel c) => c.Id.Entry == cardName);
		PileType result = PileType.Hand;
		if (args.Length >= 2 && !AbstractConsoleCmd.TryParseEnum<PileType>(args[1], out result))
		{
			return new CmdResult(success: false, "Unknown pile '" + args[1] + "'. Valid piles: Hand, Deck");
		}
		if (cardModel == null)
		{
			return new CmdResult(success: false, "Card '" + cardName + "' not found");
		}
		ModelId id = cardModel.Id;
		Task task = Task.CompletedTask;
		switch (result)
		{
		case PileType.Hand:
			foreach (CardModel card in PileType.Hand.GetPile(issuingPlayer).Cards)
			{
				if (card.Id == id)
				{
					task = CardPileCmd.RemoveFromCombat(card, isBeingPlayed: false);
					break;
				}
			}
			break;
		case PileType.Deck:
			foreach (CardModel card2 in PileType.Deck.GetPile(issuingPlayer).Cards)
			{
				if (card2.Id == id)
				{
					task = CardPileCmd.RemoveFromDeck(card2);
					break;
				}
			}
			break;
		default:
			return new CmdResult(success: false, "Unsupported pile.");
		}
		return new CmdResult(task, success: true, "Removed card '" + id.Entry + "'");
	}

	public override CompletionResult GetArgumentCompletions(Player? player, string[] args)
	{
		if (args.Length <= 1)
		{
			List<string> list = new List<string>();
			if (player != null && RunManager.Instance.IsInProgress)
			{
				IEnumerable<string> collection = PileType.Hand.GetPile(player).Cards.Select((CardModel c) => c.Id.Entry).Distinct();
				IEnumerable<string> collection2 = PileType.Deck.GetPile(player).Cards.Select((CardModel c) => c.Id.Entry).Distinct();
				list.AddRange(collection);
				list.AddRange(collection2);
				list = list.Distinct().ToList();
			}
			if (list.Count == 0)
			{
				list = ModelDb.AllCards.Select((CardModel card) => card.Id.Entry).ToList();
			}
			return CompleteArgument(list, Array.Empty<string>(), args.FirstOrDefault() ?? "");
		}
		if (args.Length == 2)
		{
			List<string> candidates = new List<string> { "Hand", "Deck" };
			return CompleteArgument(candidates, new string[1] { args[0] }, args[1]);
		}
		return new CompletionResult
		{
			Type = CompletionType.Argument,
			ArgumentContext = CmdName
		};
	}
}
