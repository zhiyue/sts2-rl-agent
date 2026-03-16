using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class CardConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "card";

	public override string Args => "<card-id:string> [pileName:string]";

	public override string Description => "Spawns a card into a pile (hand by default). Screaming snake case ('BODY_SLAM', not 'Body Slam').";

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
		PileType result = PileType.Hand;
		if (args.Length >= 2 && !AbstractConsoleCmd.TryParseEnum<PileType>(args[1], out result))
		{
			return new CmdResult(success: false, "Unknown pile '" + args[1] + "'. Valid piles: " + string.Join(", ", Enum.GetNames<PileType>()));
		}
		if (result == PileType.Hand)
		{
			CardPile pile = PileType.Hand.GetPile(issuingPlayer);
			int count = pile.Cards.Count;
			if (count >= 10)
			{
				return new CmdResult(success: false, $"The hand is full ({count}/{10}).");
			}
		}
		string cardName = args[0].ToUpperInvariant();
		CardModel cardModel = ModelDb.AllCards.FirstOrDefault((CardModel c) => c.Id.Entry == cardName);
		if (cardModel == null)
		{
			return new CmdResult(success: false, "Card '" + cardName + "' not found");
		}
		ICardScope cardScope2;
		if (!result.IsCombatPile())
		{
			ICardScope cardScope = RunManager.Instance.DebugOnlyGetState();
			cardScope2 = cardScope;
		}
		else
		{
			ICardScope cardScope = CombatManager.Instance.DebugOnlyGetState();
			cardScope2 = cardScope;
		}
		ICardScope cardScope3 = cardScope2;
		CardModel card = cardScope3.CreateCard(cardModel, issuingPlayer);
		Task task = CardPileCmd.Add(card, result);
		return new CmdResult(task, success: true, $"Added card '{cardModel.Id.Entry}' to '{result}'");
	}

	public override CompletionResult GetArgumentCompletions(Player? player, string[] args)
	{
		if (args.Length <= 1)
		{
			List<string> candidates = ModelDb.AllCards.Select((CardModel card) => card.Id.Entry).ToList();
			return CompleteArgument(candidates, Array.Empty<string>(), args.FirstOrDefault() ?? "");
		}
		if (args.Length == 2)
		{
			List<string> candidates2 = Enum.GetNames<PileType>().ToList();
			return CompleteArgument(candidates2, new string[1] { args[0] }, args[1]);
		}
		return new CompletionResult
		{
			Type = CompletionType.Argument,
			ArgumentContext = CmdName
		};
	}
}
