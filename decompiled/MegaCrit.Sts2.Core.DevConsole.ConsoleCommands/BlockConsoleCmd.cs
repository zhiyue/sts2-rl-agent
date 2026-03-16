using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class BlockConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "block";

	public override string Args => "<int:amount> <target-index:int>";

	public override string Description => "Gives block to player, or to target creature if index is given (0 is player).";

	public override bool IsNetworked => true;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (issuingPlayer == null)
		{
			return new CmdResult(success: false, "This command only works during a run.");
		}
		if (!CombatManager.Instance.IsInProgress)
		{
			return new CmdResult(success: false, "This doesn't appear to be a combat!");
		}
		int num = args.Length;
		if ((num < 1 || num > 2) ? true : false)
		{
			return new CmdResult(success: false, "There must be 1 or 2 args.");
		}
		if (!int.TryParse(args[0], out var result))
		{
			return new CmdResult(success: false, "Arg 1 must be the amount of block.");
		}
		if (result < 0)
		{
			return new CmdResult(success: false, "Removing block is not supported.");
		}
		Creature creature = issuingPlayer.Creature;
		Creature creature2;
		if (args.Length == 1)
		{
			creature2 = creature;
		}
		else
		{
			if (!int.TryParse(args[1], out var result2))
			{
				return new CmdResult(success: false, "Arg 2 must be the target index if specified.");
			}
			IReadOnlyList<Creature> creatures = creature.CombatState.Creatures;
			if (result2 < 0 || result2 >= creatures.Count)
			{
				return new CmdResult(success: false, $"Invalid target index {result2}. Valid range: 0-{creatures.Count - 1}");
			}
			creature2 = creatures[result2];
		}
		Task task = GainBlock(creature2, result);
		return new CmdResult(task, success: true, $"Added '{result}' block to {creature2}.");
	}

	private static async Task GainBlock(Creature target, int amount)
	{
		await CreatureCmd.GainBlock(target, new BlockVar(amount, ValueProp.Unpowered), null);
	}
}
