using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class ApplyPowerConsoleCmd : AbstractConsoleCmd
{
	private static List<PowerModel>? _allPowers;

	public override string CmdName => "power";

	public override string Args => "<id:string> <amount:int> <target-index:int>";

	public override string Description => "Grant power to given target at index.";

	public override bool IsNetworked => true;

	private static IEnumerable<PowerModel> AllPowers
	{
		get
		{
			if (_allPowers == null)
			{
				_allPowers = ModelDb.AllAbstractModelSubtypes.Where((Type t) => t.IsSubclassOf(typeof(PowerModel))).Select(ModelDb.DebugPower).ToList();
			}
			return _allPowers;
		}
	}

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (!CombatManager.Instance.IsInProgress)
		{
			return new CmdResult(success: false, "This doesn't appear to be a combat!");
		}
		if (args.Length < 3)
		{
			return new CmdResult(success: false, "There must be 3 args.");
		}
		if (!int.TryParse(args[1], out var result))
		{
			return new CmdResult(success: false, "Arg 1 must be the amount of power to be applied.");
		}
		string powerId = args[0].ToUpperInvariant();
		PowerModel power = AllPowers.FirstOrDefault((PowerModel c) => c.Id.Entry == powerId);
		if (power == null)
		{
			return new CmdResult(success: false, "The power id " + powerId + " does not exist.");
		}
		if (!int.TryParse(args[2], out var result2))
		{
			return new CmdResult(success: false, "Arg 2 must be the target index if specified.");
		}
		IReadOnlyList<Creature> creatures = CombatManager.Instance.DebugOnlyGetState().Creatures;
		if (result2 < 0 || result2 >= creatures.Count)
		{
			return new CmdResult(success: false, $"Invalid target index {result2}. Valid range: 0-{creatures.Count - 1}");
		}
		Creature creature = creatures[result2];
		PowerModel powerModel = creature.Powers.FirstOrDefault((PowerModel p) => p.GetType() == power.GetType());
		Task task = ((!power.IsInstanced && powerModel != null) ? PowerCmd.ModifyAmount(powerModel, result, null, null) : PowerCmd.Apply(power.ToMutable(), creature, result, null, null));
		string reference = (creature.IsPlayer ? "PLAYER" : creature.Monster.Id.Entry);
		return new CmdResult(task, success: true, "AppliedPower: [" + string.Join(",", new ReadOnlySpan<string>(in reference)) + "]");
	}

	public override CompletionResult GetArgumentCompletions(Player? player, string[] args)
	{
		if (args.Length <= 1)
		{
			List<string> candidates = AllPowers.Select((PowerModel p) => p.Id.Entry).ToList();
			return CompleteArgument(candidates, Array.Empty<string>(), args.FirstOrDefault() ?? "");
		}
		return new CompletionResult
		{
			Type = CompletionType.Argument,
			ArgumentContext = CmdName
		};
	}
}
