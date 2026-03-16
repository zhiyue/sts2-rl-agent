using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class DamageConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "damage";

	public override string Args => "<amount:int> <target-index:int>";

	public override string Description => "Damage all enemies, or target creature if index is given (0 is player).";

	public override bool IsNetworked => true;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
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
			return new CmdResult(success: false, "Arg 1 must be the amount of damage.");
		}
		if (result < 0)
		{
			return new CmdResult(success: false, "The damage amount cannot be negative.");
		}
		CombatState combatState = CombatManager.Instance.DebugOnlyGetState();
		IEnumerable<Creature> enumerable;
		if (args.Length < 2)
		{
			enumerable = combatState.Enemies;
		}
		else
		{
			if (!int.TryParse(args[1], out var result2))
			{
				return new CmdResult(success: false, "Arg 2 must be the target index if specified.");
			}
			if (result2 < 0 || result2 >= combatState.Creatures.Count)
			{
				return new CmdResult(success: false, $"Invalid target index {result2}. Valid range: 0-{combatState.Creatures.Count - 1}");
			}
			enumerable = new global::_003C_003Ez__ReadOnlySingleElementList<Creature>(combatState.Creatures[result2]);
		}
		IEnumerable<string> values = enumerable.Select((Creature c) => (!c.IsPlayer) ? c.Monster.Id.Entry : "PLAYER");
		Task task = DamageAndCheckWinCondition(enumerable, result);
		return new CmdResult(task, success: true, "Damaged: [" + string.Join(",", values) + "]");
	}

	private async Task DamageAndCheckWinCondition(IEnumerable<Creature> creatures, decimal amount)
	{
		await CreatureCmd.Damage(new BlockingPlayerChoiceContext(), creatures.ToList(), amount, ValueProp.Unpowered, null, null);
		await CombatManager.Instance.CheckWinCondition();
	}
}
