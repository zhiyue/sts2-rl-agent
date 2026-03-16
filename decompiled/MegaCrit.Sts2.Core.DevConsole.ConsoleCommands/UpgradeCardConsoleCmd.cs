using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class UpgradeCardConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "upgrade";

	public override string Args => "<hand-index:int>";

	public override string Description => "Upgrade the target card based on its hand position (0 is the left most).";

	public override bool IsNetworked => true;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (!RunManager.Instance.IsInProgress)
		{
			return new CmdResult(success: false, "A run is currently not in progress!");
		}
		int result = 0;
		if (args.Length != 0 && !int.TryParse(args[0], out result))
		{
			return new CmdResult(success: false, "Arg 1 must be the hand index (int), got '" + args[0] + "'.");
		}
		CardPile pile = PileType.Hand.GetPile(issuingPlayer);
		IReadOnlyList<CardModel> cards = pile.Cards;
		int count = cards.Count;
		if (result < 0 || result >= count)
		{
			return new CmdResult(success: false, $"Invalid hand index {result}. Valid range: 0-{count - 1}.");
		}
		CardModel cardModel = pile.Cards[result];
		if (cardModel.CurrentUpgradeLevel == cardModel.MaxUpgradeLevel)
		{
			return new CmdResult(success: false, $"The card at index={result} is already upgraded to max_level={cardModel.MaxUpgradeLevel}!");
		}
		CardCmd.Upgrade(cardModel);
		return new CmdResult(success: true, $"Upgraded '{cardModel.Title}' at index '{result}' in hand.");
	}

	public override CompletionResult GetArgumentCompletions(Player? player, string[] args)
	{
		if (args.Length <= 1 && RunManager.Instance.IsInProgress && player != null)
		{
			CardPile pile = PileType.Hand.GetPile(player);
			int count = pile.Cards.Count;
			if (count > 0)
			{
				List<string> candidates = (from i in Enumerable.Range(0, count)
					select i.ToString()).ToList();
				return CompleteArgument(candidates, Array.Empty<string>(), args.FirstOrDefault() ?? "");
			}
		}
		return new CompletionResult
		{
			Type = CompletionType.Argument,
			ArgumentContext = CmdName
		};
	}
}
