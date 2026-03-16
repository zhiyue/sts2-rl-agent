using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Daily;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Leaderboard;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class LeaderboardConsoleCmd : AbstractConsoleCmd
{
	private static readonly List<string> _validOptions;

	public override string CmdName => "leaderboard";

	public override string Args => "[option:string] [name:string] <score:int> [count:int]";

	public override string Description => "Adds scores to the leaderboard. Option can be upload|random. If random, <count> random scores will be uploaded to leaderboard <name>. If upload, one score will be uploaded to leaderboard <name> with <score>.";

	public override bool IsNetworked => false;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (args.Length == 0)
		{
			return new CmdResult(success: false, "Option must be specified (random or upload).");
		}
		string text = args[0];
		string text2 = DailyRunUtility.GetLeaderboardName(DateTimeOffset.Now, 1);
		if (args.Length > 1 && args[1] != "-")
		{
			text2 = args[1];
		}
		if (text == "random")
		{
			int result = 100;
			if (args.Length >= 3 && !int.TryParse(args[2], out result))
			{
				return new CmdResult(success: false, "Count must be a valid integer.");
			}
			TaskHelper.RunSafely(UploadRandomScores(text2, result));
			return new CmdResult(success: true, $"Adding {result} entries to leaderboard {text2}");
		}
		if (text == "upload")
		{
			if (args.Length < 3)
			{
				return new CmdResult(success: false, "Score must be specified when upload is the option.\nUse - as the leaderboard name for the default.");
			}
			if (!int.TryParse(args[2], out var result2))
			{
				return new CmdResult(success: false, "Score must be a valid integer.");
			}
			TaskHelper.RunSafely(UploadScore(text2, result2));
			return new CmdResult(success: true, $"Uploading score {result2} to leaderboard {text2}");
		}
		return new CmdResult(success: false, "Invalid option " + text + ", must be upload or random");
	}

	private async Task UploadRandomScores(string leaderboardName, int count)
	{
		ILeaderboardHandle handle = await LeaderboardManager.GetOrCreateLeaderboard(leaderboardName);
		Rng rng = new Rng(Rng.Chaotic.NextUnsignedInt());
		for (int i = 0; i < count; i++)
		{
			LeaderboardManager.DebugAddEntry(handle, new LeaderboardEntry
			{
				id = rng.NextUnsignedInt(),
				name = rng.NextUnsignedInt().ToString(),
				score = rng.NextInt()
			});
		}
	}

	private async Task UploadScore(string leaderboardName, int score)
	{
		await LeaderboardManager.UploadLocalScore(await LeaderboardManager.GetOrCreateLeaderboard(leaderboardName), score, Array.Empty<ulong>());
	}

	public override CompletionResult GetArgumentCompletions(Player? player, string[] args)
	{
		if (args.Length <= 1)
		{
			string partial = ((args.Length != 0) ? args[0] : "");
			List<string> candidates = ((!string.IsNullOrWhiteSpace(partial)) ? _validOptions.Where((string option) => option.Contains(partial, StringComparison.OrdinalIgnoreCase)).ToList() : _validOptions);
			return new CompletionResult
			{
				Candidates = candidates,
				Type = CompletionType.Argument,
				ArgumentContext = CmdName
			};
		}
		return new CompletionResult
		{
			Type = CompletionType.Argument,
			ArgumentContext = CmdName
		};
	}

	static LeaderboardConsoleCmd()
	{
		int num = 2;
		List<string> list = new List<string>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<string> span = CollectionsMarshal.AsSpan(list);
		int num2 = 0;
		span[num2] = "upload";
		num2++;
		span[num2] = "random";
		_validOptions = list;
	}
}
