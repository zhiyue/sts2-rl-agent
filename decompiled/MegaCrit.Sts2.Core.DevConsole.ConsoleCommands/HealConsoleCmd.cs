using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class HealConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "heal";

	public override string Args => "<amount:int> [index:int]";

	public override string Description => "Heal the player some amount of HP.";

	public override bool IsNetworked => true;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (args.Length < 1)
		{
			return new CmdResult(success: false, "An amount is required");
		}
		if (!int.TryParse(args[0], out var result))
		{
			return new CmdResult(success: false, "First argument (the heal amount) must be an int.");
		}
		if (!RunManager.Instance.IsInProgress)
		{
			return new CmdResult(success: false, "A run does not appear to be in progress");
		}
		if (result < 0)
		{
			return new CmdResult(success: false, "The heal amount cannot be negative.");
		}
		Creature creature;
		if (args.Length > 1)
		{
			if (!int.TryParse(args[1], out var result2))
			{
				return new CmdResult(success: false, "Arg 2 must be the target index (int), got '" + args[1] + "'.");
			}
			IReadOnlyList<Creature> allies = CombatManager.Instance.DebugOnlyGetState().Allies;
			if (result2 < 0 || result2 >= allies.Count)
			{
				return new CmdResult(success: false, $"Invalid target index {result2}. Valid range: 0-{allies.Count - 1}");
			}
			creature = allies[result2];
		}
		else
		{
			creature = issuingPlayer.Creature;
		}
		Task task = CreatureCmd.Heal(creature, result);
		return new CmdResult(task, success: true, $"Healed '{result}' HP to {creature}.");
	}
}
