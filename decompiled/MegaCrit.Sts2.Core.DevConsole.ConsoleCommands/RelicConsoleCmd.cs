using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class RelicConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "relic";

	public override string Args => "[add|remove] <relic-id:string>";

	public override string Description => "Adds/Removes relic from player (add by default)";

	public override bool IsNetworked => true;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (args.Length < 1)
		{
			return new CmdResult(success: false, CmdName + " requires a relic name");
		}
		if (issuingPlayer == null)
		{
			return new CmdResult(success: false, "A run is currently not in progress!");
		}
		string text;
		if (args[0].ToLowerInvariant().Equals("add") || args[0].ToLowerInvariant().Equals("remove"))
		{
			if (args.Length < 2)
			{
				return new CmdResult(success: false, "You need to specify a relic!");
			}
			text = args[1];
		}
		else
		{
			text = args[0];
		}
		RelicModel relicById = GetRelicById(text);
		if (relicById == null)
		{
			return new CmdResult(success: false, "Unable to create relic '" + text + "'.");
		}
		if (args[0].ToLowerInvariant().Equals("remove"))
		{
			RelicModel relicById2 = issuingPlayer.GetRelicById(relicById.Id);
			if (relicById2 != null)
			{
				TaskHelper.RunSafely(RelicCmd.Remove(relicById2));
				return new CmdResult(success: true, "Relic removed!");
			}
			return new CmdResult(success: false, "Unable to find relic in player!");
		}
		TaskHelper.RunSafely(RelicCmd.Obtain(relicById.ToMutable(), issuingPlayer));
		return new CmdResult(success: true, "Added relic '" + text + "'");
	}

	private static RelicModel? GetRelicById(string id)
	{
		id = id.ToUpperInvariant();
		List<RelicModel> source = ModelDb.AllRelics.Where((RelicModel r) => r.Id.Entry.Contains(id)).ToList();
		RelicModel relicModel = source.FirstOrDefault((RelicModel r) => r.Id.Entry == id);
		if (relicModel != null)
		{
			return relicModel;
		}
		RelicModel relicModel2 = source.FirstOrDefault((RelicModel r) => r.Id.Entry.StartsWith(id));
		if (relicModel2 != null)
		{
			return relicModel2;
		}
		return source.FirstOrDefault();
	}

	public override CompletionResult GetArgumentCompletions(Player? player, string[] args)
	{
		if (args.Length == 0 || (args.Length == 1 && string.IsNullOrWhiteSpace(args[0])))
		{
			CompletionResult completionResult = new CompletionResult();
			int num = 2;
			List<string> list = new List<string>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<string> span = CollectionsMarshal.AsSpan(list);
			int num2 = 0;
			span[num2] = "add";
			num2++;
			span[num2] = "remove";
			completionResult.Candidates = list;
			completionResult.Type = CompletionType.Subcommand;
			completionResult.ArgumentContext = CmdName;
			return completionResult;
		}
		if (args.Length == 1)
		{
			List<string> list2 = new List<string>();
			string text = args[0].ToLowerInvariant();
			if ("add".StartsWith(text))
			{
				list2.Add("add");
			}
			if ("remove".StartsWith(text))
			{
				list2.Add("remove");
			}
			if (list2.Count > 0)
			{
				return CompleteArgument(list2, Array.Empty<string>(), text, CompletionType.Subcommand);
			}
			List<string> candidates = ModelDb.AllRelics.Select((RelicModel r) => r.Id.Entry).ToList();
			return CompleteArgument(candidates, Array.Empty<string>(), args[0], CompletionType.Argument, (string candidate, string partialArg) => candidate.Contains(partialArg, StringComparison.OrdinalIgnoreCase));
		}
		if (args.Length == 2)
		{
			List<string> candidates2 = ModelDb.AllRelics.Select((RelicModel r) => r.Id.Entry).ToList();
			return CompleteArgument(candidates2, new string[1] { args[0] }, args[1], CompletionType.Argument, (string candidate, string partial) => candidate.Contains(partial, StringComparison.OrdinalIgnoreCase));
		}
		return new CompletionResult
		{
			Type = CompletionType.Argument,
			ArgumentContext = CmdName
		};
	}
}
