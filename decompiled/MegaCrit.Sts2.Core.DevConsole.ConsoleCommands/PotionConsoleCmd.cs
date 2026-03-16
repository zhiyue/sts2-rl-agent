using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class PotionConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "potion";

	public override string Args => "<id:string>";

	public override string Description => "Adds potion to belt. Screaming snake case ('ENTROPIC_BREW', not 'Entropic Brew').";

	public override bool IsNetworked => true;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (args.Length < 1)
		{
			return new CmdResult(success: false, CmdName + " requires a potion name");
		}
		if (!RunManager.Instance.IsInProgress)
		{
			return new CmdResult(success: false, "A run is not in progress.");
		}
		string potionId = args[0].ToUpperInvariant();
		PotionModel potionModel = ModelDb.AllPotions.FirstOrDefault((PotionModel p) => p.Id.Entry == potionId);
		if (potionModel == null)
		{
			return new CmdResult(success: false, "Potion '" + potionId + "' not found");
		}
		PotionModel potion = potionModel.ToMutable();
		Task task = PotionCmd.TryToProcure(potion, issuingPlayer);
		return new CmdResult(task, success: true, "Added potion " + potionModel.Id.Entry);
	}

	public override CompletionResult GetArgumentCompletions(Player? player, string[] args)
	{
		if (args.Length <= 1)
		{
			List<string> candidates = ModelDb.AllPotions.Select((PotionModel p) => p.Id.Entry).ToList();
			return CompleteArgument(candidates, Array.Empty<string>(), args.FirstOrDefault() ?? "");
		}
		return new CompletionResult
		{
			Type = CompletionType.Argument,
			ArgumentContext = CmdName
		};
	}
}
