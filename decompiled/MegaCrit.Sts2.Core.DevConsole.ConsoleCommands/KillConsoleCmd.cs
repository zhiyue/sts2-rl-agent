using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class KillConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "kill";

	public override string Args => "<target-index:int>|'all'";

	public override string Description => "Will kill one target if the index is given, or all if 'all', or the first if no arguments.";

	public override bool IsNetworked => true;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (!CombatManager.Instance.IsInProgress)
		{
			return new CmdResult(success: false, "This doesn't appear to be a combat!");
		}
		List<Creature> list = new List<Creature>();
		IReadOnlyList<Creature> readOnlyList = CombatManager.Instance.DebugOnlyGetState().Enemies.ToList();
		if (args.Length == 0)
		{
			list.Add(readOnlyList[0]);
		}
		else if (args[0].Equals("all"))
		{
			foreach (Creature item in readOnlyList)
			{
				list.Add(item);
			}
		}
		else
		{
			if (!int.TryParse(args[0], out var result))
			{
				return new CmdResult(success: false, "Invalid argument '" + args[0] + "'. Use a target index or 'all'.");
			}
			if (result < 0 || result >= readOnlyList.Count)
			{
				return new CmdResult(success: false, $"Invalid target index {result}. Valid range: 0-{readOnlyList.Count - 1}");
			}
			list.Add(readOnlyList[result]);
		}
		IEnumerable<string> values = from c in list
			select c.Monster into m
			where m != null
			select m.Id.Entry.ToString();
		TaskHelper.RunSafely(DoKill(list));
		return new CmdResult(success: true, "Killed: [" + string.Join(",", values) + "]");
	}

	private async Task DoKill(List<Creature> toKill)
	{
		foreach (Creature item in toKill)
		{
			await CreatureCmd.Kill(item);
		}
		await CombatManager.Instance.CheckWinCondition();
	}

	public override CompletionResult GetArgumentCompletions(Player? player, string[] args)
	{
		if (args.Length <= 1)
		{
			int num = 1;
			List<string> list = new List<string>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<string> span = CollectionsMarshal.AsSpan(list);
			int index = 0;
			span[index] = "all";
			List<string> list2 = list;
			if (CombatManager.Instance.IsInProgress)
			{
				IReadOnlyList<Creature> readOnlyList = CombatManager.Instance.DebugOnlyGetState()?.Enemies;
				if (readOnlyList != null)
				{
					for (int i = 0; i < readOnlyList.Count; i++)
					{
						list2.Add(i.ToString());
					}
				}
			}
			return CompleteArgument(list2, Array.Empty<string>(), args.FirstOrDefault() ?? "");
		}
		return new CompletionResult
		{
			Type = CompletionType.Argument,
			ArgumentContext = CmdName
		};
	}
}
