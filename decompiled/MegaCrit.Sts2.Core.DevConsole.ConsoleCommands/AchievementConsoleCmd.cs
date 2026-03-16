using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using MegaCrit.Sts2.Core.Achievements;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Platform;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class AchievementConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "achievement";

	public override string Args => "<operation:string> [id:string]";

	public override string Description => "Unlocks or revokes an achievement. If no achievement is provided, all achievements are unlocked or revoked.";

	public override bool IsNetworked => false;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		int num = 3;
		List<string> list = new List<string>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<string> span = CollectionsMarshal.AsSpan(list);
		int num2 = 0;
		span[num2] = "unlock";
		num2++;
		span[num2] = "revoke";
		num2++;
		span[num2] = "check";
		List<string> list2 = list;
		if (args.Length < 1 || !list2.Contains(args[0]))
		{
			return new CmdResult(success: false, "First argument must be one of: " + string.Join(",", list2) + ".\n" + Args);
		}
		Achievement? achievement = null;
		if (args.Length >= 2)
		{
			string text = args[1].ToLowerInvariant();
			Achievement[] values = Enum.GetValues<Achievement>();
			for (int i = 0; i < values.Length; i++)
			{
				Achievement value = values[i];
				if (StringHelper.SnakeCase(value.ToString()) == text)
				{
					achievement = value;
					break;
				}
			}
			if (!achievement.HasValue)
			{
				return new CmdResult(success: false, "Achievement '" + args[1] + "' unrecognized.\n" + Args);
			}
		}
		if (args[0] == "unlock")
		{
			if (achievement.HasValue)
			{
				AchievementsUtil.Unlock(achievement.Value, issuingPlayer);
				return new CmdResult(success: true, $"Unlocked {achievement.Value}");
			}
			Achievement[] values2 = Enum.GetValues<Achievement>();
			foreach (Achievement achievement2 in values2)
			{
				AchievementsUtil.Unlock(achievement2, issuingPlayer);
			}
			return new CmdResult(success: true, "Unlocked all achievements");
		}
		if (args[0] == "revoke")
		{
			if (achievement.HasValue)
			{
				AchievementsUtil.Revoke(achievement.Value);
				return new CmdResult(success: true, $"Revoked {achievement.Value}");
			}
			Achievement[] values3 = Enum.GetValues<Achievement>();
			foreach (Achievement achievement3 in values3)
			{
				AchievementsUtil.Revoke(achievement3);
			}
			return new CmdResult(success: true, "Revoked all achievements");
		}
		if (args[0] == "check")
		{
			if (!achievement.HasValue)
			{
				return new CmdResult(success: false, "Achievement name must be provided");
			}
			if (AchievementsUtil.IsUnlocked(achievement.Value))
			{
				return new CmdResult(success: true, $"{achievement.Value} is unlocked");
			}
			return new CmdResult(success: true, $"{achievement.Value} is not unlocked");
		}
		throw new NotImplementedException();
	}

	public override CompletionResult GetArgumentCompletions(Player? player, string[] args)
	{
		if (args.Length <= 1)
		{
			List<string> candidates = new List<string> { "unlock", "revoke", "check" };
			return CompleteArgument(candidates, Array.Empty<string>(), args.FirstOrDefault() ?? "", CompletionType.Subcommand);
		}
		if (args.Length == 2)
		{
			List<string> list = (from a in Enum.GetValues<Achievement>()
				select StringHelper.SnakeCase(a.ToString())).ToList();
			list.Sort();
			return CompleteArgument(list, new string[1] { args[0] }, args[1]);
		}
		return new CompletionResult
		{
			Type = CompletionType.Argument,
			ArgumentContext = CmdName
		};
	}
}
