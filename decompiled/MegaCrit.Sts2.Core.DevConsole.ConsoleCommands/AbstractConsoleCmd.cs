using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.SourceGeneration;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

[GenerateSubtypes(DynamicallyAccessedMemberTypes = DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
public abstract class AbstractConsoleCmd
{
	public abstract string CmdName { get; }

	public abstract string Args { get; }

	public abstract string Description { get; }

	public abstract bool IsNetworked { get; }

	public virtual bool DebugOnly => true;

	public abstract CmdResult Process(Player? issuingPlayer, string[] args);

	public virtual CompletionResult GetArgumentCompletions(Player? player, string[] args)
	{
		return new CompletionResult
		{
			Type = CompletionType.Argument,
			ArgumentContext = CmdName,
			ArgumentIndex = args.Length - 1,
			CommandPrefix = BuildPrefix(args)
		};
	}

	protected CompletionResult CompleteArgument(IEnumerable<string> candidates, string[] completedArgs, string partialArg, CompletionType type = CompletionType.Argument, Func<string, string, bool>? matchPredicate = null)
	{
		List<string> list = candidates.ToList();
		if (matchPredicate == null)
		{
			matchPredicate = (string candidate, string partial) => candidate.StartsWith(partial, StringComparison.OrdinalIgnoreCase);
		}
		List<string> list2 = ((!string.IsNullOrWhiteSpace(partialArg)) ? list.Where((string c) => matchPredicate(c, partialArg)).ToList() : list);
		string text = BuildPrefix(completedArgs);
		string commonPrefix = CalculateCommonCompletion(list2, text);
		return new CompletionResult
		{
			Candidates = list2,
			CommonPrefix = commonPrefix,
			Type = type,
			ArgumentContext = CmdName,
			ArgumentIndex = completedArgs.Length,
			CommandPrefix = text
		};
	}

	protected string BuildPrefix(string[] completedArgs)
	{
		if (completedArgs.Length == 0)
		{
			return CmdName + " ";
		}
		return CmdName + " " + string.Join(" ", completedArgs) + " ";
	}

	protected static bool TryParseEnum<T>(string input, out T result) where T : struct, Enum
	{
		if (Enum.TryParse<T>(input, ignoreCase: true, out result))
		{
			return Enum.IsDefined(result);
		}
		return false;
	}

	private string CalculateCommonCompletion(List<string> filtered, string prefix)
	{
		if (filtered.Count == 0)
		{
			return "";
		}
		if (filtered.Count == 1)
		{
			return prefix + filtered[0] + " ";
		}
		int num = filtered.Min((string s) => s.Length);
		string text = filtered[0];
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			char c = text[i];
			if (!filtered.All((string s) => char.ToLowerInvariant(s[i]) == char.ToLowerInvariant(c)))
			{
				break;
			}
			num2 = i + 1;
		}
		if (num2 > 0)
		{
			return prefix + text.Substring(0, num2);
		}
		return "";
	}
}
