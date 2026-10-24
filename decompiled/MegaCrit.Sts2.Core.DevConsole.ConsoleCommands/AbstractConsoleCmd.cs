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

	/// <summary>
	/// Provides argument completion candidates for console commands.
	/// Commands should override this method to provide context-aware completion suggestions.
	/// Returns an empty result by default for commands that don't require argument completion.
	/// </summary>
	/// <param name="player">The player invoking the command</param>
	/// <param name="args">Current command arguments being typed</param>
	/// <returns>Completion result containing candidates and context information</returns>
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

	/// <summary>
	/// Creates a completion result for command arguments with explicit separation of completed vs partial arguments.
	/// This makes the completion logic clear and prevents bugs from implicit argument slicing.
	/// </summary>
	/// <param name="candidates">All possible completion candidates</param>
	/// <param name="completedArgs">Arguments that are already fully typed (e.g., ["OFFERING"] when completing second arg)</param>
	/// <param name="partialArg">The argument currently being typed (e.g., "de" when typing "deck")</param>
	/// <param name="type">The type of completion being performed</param>
	/// <param name="matchPredicate">Optional custom matching predicate (defaults to StartsWith)</param>
	/// <returns>CompletionResult with filtered candidates and completion prefix</returns>
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

	/// <summary>
	/// Builds the command prefix from completed arguments (not including the partial arg being typed).
	/// Example: BuildPrefix(["OFFERING"]) returns "card OFFERING "
	/// </summary>
	protected string BuildPrefix(string[] completedArgs)
	{
		if (completedArgs.Length == 0)
		{
			return CmdName + " ";
		}
		return CmdName + " " + string.Join(" ", completedArgs) + " ";
	}

	/// <summary>
	/// Parses a string into an enum value, rejecting numeric strings and undefined values.
	/// Use this instead of Enum.TryParse to avoid accepting raw integers as valid enum values.
	/// </summary>
	protected static bool TryParseEnum<T>(string input, out T result) where T : struct, Enum
	{
		if (Enum.TryParse<T>(input, ignoreCase: true, out result))
		{
			return Enum.IsDefined(result);
		}
		return false;
	}

	/// <summary>
	/// Calculates the common completion string from filtered candidates.
	/// Single match gets trailing space, multiple matches get longest common prefix.
	/// </summary>
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
