using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Exceptions;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class EnchantConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "enchant";

	public override string Args => "<id:string> [amount:int] [hand-index:int]";

	public override string Description => "Enchants a card in the player's hand with the specified enchantment.";

	public override bool IsNetworked => true;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (args.Length == 0)
		{
			return new CmdResult(success: false, "Must specify an enchantment ID!");
		}
		if (!CombatManager.Instance.IsInProgress)
		{
			return new CmdResult(success: false, "Combat is not currently in progress!");
		}
		ModelId modelId = new ModelId(ModelId.SlugifyCategory<EnchantmentModel>(), args[0].ToUpperInvariant());
		EnchantmentModel enchantmentModel;
		try
		{
			enchantmentModel = ModelDb.GetById<EnchantmentModel>(modelId).ToMutable();
		}
		catch (ModelNotFoundException)
		{
			return new CmdResult(success: false, "Enchantment '" + modelId.Entry + "' not found");
		}
		int result = 1;
		int result2 = 0;
		if (args.Length > 1 && !int.TryParse(args[1], out result))
		{
			return new CmdResult(success: false, "Arg 2 must be the enchantment amount (int), got '" + args[1] + "'.");
		}
		if (args.Length > 2 && !int.TryParse(args[2], out result2))
		{
			return new CmdResult(success: false, "Arg 3 must be the hand index (int), got '" + args[2] + "'.");
		}
		CardPile pile = PileType.Hand.GetPile(issuingPlayer);
		IReadOnlyList<CardModel> cards = pile.Cards;
		int count = cards.Count;
		if (result2 < 0 || result2 >= count)
		{
			return new CmdResult(success: false, $"Invalid hand index {result2}. Valid range: 0-{count - 1}.");
		}
		CardModel cardModel = pile.Cards[result2];
		CardCmd.Enchant(enchantmentModel, cardModel, result);
		return new CmdResult(success: true, $"Enchanted card {cardModel.Title} with {result} {enchantmentModel.Title.GetFormattedText()}");
	}

	public override CompletionResult GetArgumentCompletions(Player? player, string[] args)
	{
		if (args.Length <= 1)
		{
			List<string> candidates = ModelDb.DebugEnchantments.Select((EnchantmentModel e) => e.Id.Entry).ToList();
			return CompleteArgument(candidates, Array.Empty<string>(), args.FirstOrDefault() ?? "");
		}
		return new CompletionResult
		{
			Type = CompletionType.Argument,
			ArgumentContext = CmdName
		};
	}
}
