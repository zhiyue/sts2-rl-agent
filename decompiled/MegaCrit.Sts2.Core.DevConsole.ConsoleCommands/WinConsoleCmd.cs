using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class WinConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "win";

	public override string Args => "";

	public override string Description => "You win the combat";

	public override bool IsNetworked => true;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (!CombatManager.Instance.IsInProgress)
		{
			return new CmdResult(success: false, "This doesn't appear to be a combat!");
		}
		List<Creature> list = CombatManager.Instance.DebugOnlyGetState().Enemies.ToList();
		Task task = KillEnemies(list);
		IEnumerable<string> values = from c in list
			select c.Monster into m
			where m != null
			select m.Id.Entry.ToString();
		return new CmdResult(task, success: true, "Killed: [" + string.Join(",", values) + "]");
	}

	private async Task KillEnemies(List<Creature> creatures)
	{
		foreach (Creature creature in creatures)
		{
			creature.RemoveAllPowersInternalExcept();
			await CreatureCmd.Kill(creature);
		}
		await CombatManager.Instance.CheckWinCondition();
	}
}
