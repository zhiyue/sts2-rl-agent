using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Godot;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.DevConsole;

public class DevConsole
{
	private readonly Dictionary<string, AbstractConsoleCmd> _commands = new Dictionary<string, AbstractConsoleCmd>();

	public readonly FixedSizedQueue<string> history;

	public int historyIndex;

	private readonly string _historyFilePath;

	public DevConsole(bool shouldAllowDebugCommands)
	{
		_historyFilePath = UserDataPathProvider.GetAccountScopedBasePath("console_history.log");
		history = new FixedSizedQueue<string>(40);
		LoadCommandHistory();
		foreach (Type item in AbstractConsoleCmdSubtypes.All.Concat(ReflectionHelper.GetSubtypesInMods<AbstractConsoleCmd>()))
		{
			AbstractConsoleCmd abstractConsoleCmd = (AbstractConsoleCmd)Activator.CreateInstance(item);
			if (!abstractConsoleCmd.DebugOnly || shouldAllowDebugCommands)
			{
				RegisterCommand(abstractConsoleCmd);
			}
		}
	}

	private void RegisterCommand(AbstractConsoleCmd consoleCmd)
	{
		_commands[consoleCmd.CmdName] = consoleCmd;
	}

	private void LoadCommandHistory()
	{
		using FileAccess fileAccess = FileAccess.Open(_historyFilePath, FileAccess.ModeFlags.Read);
		if (fileAccess != null)
		{
			while (fileAccess.GetPosition() < fileAccess.GetLength())
			{
				history.Add(fileAccess.GetLine());
			}
		}
	}

	private void SaveCommandHistory()
	{
		using FileAccess fileAccess = FileAccess.Open(_historyFilePath, FileAccess.ModeFlags.Write);
		if (fileAccess == null)
		{
			return;
		}
		foreach (string item in history)
		{
			fileAccess.StoreLine(item);
		}
	}

	public CompletionResult GetCompletionResults(string inputBuffer)
	{
		string[] array = (inputBuffer.EndsWith(' ') ? inputBuffer.Trim().Split(' ').Append(string.Empty)
			.ToArray() : inputBuffer.Trim().Split(' '));
		string text = array.First();
		if (_commands.ContainsKey(text) && array.Length == 1)
		{
			CompletionResult completionResult = new CompletionResult();
			int num = 1;
			List<string> list = new List<string>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<string> span = CollectionsMarshal.AsSpan(list);
			int index = 0;
			span[index] = text;
			completionResult.Candidates = list;
			completionResult.CommonPrefix = text + " ";
			completionResult.Type = CompletionType.Command;
			completionResult.CommandPrefix = "";
			completionResult.ArgumentIndex = -1;
			completionResult.ArgumentContext = "";
			return completionResult;
		}
		if (_commands.TryGetValue(text.Trim().ToLowerInvariant(), out AbstractConsoleCmd value))
		{
			string[] array2 = array.Skip(1).ToArray();
			string text2 = text + " ";
			if (array2.Length != 0)
			{
				string[] array3 = array2.Take(array2.Length - 1).ToArray();
				if (array3.Length != 0)
				{
					text2 = text2 + string.Join(" ", array3) + " ";
				}
			}
			CompletionType type = ((array2.Length <= 1) ? CompletionType.Subcommand : CompletionType.Argument);
			int argumentIndex = array2.Length - 1;
			CompletionResult argumentCompletions = value.GetArgumentCompletions(LocalContext.GetMe(RunManager.Instance.DebugOnlyGetState()), array2);
			if (argumentCompletions.Candidates.Count > 0)
			{
				argumentCompletions.Type = type;
				argumentCompletions.CommandPrefix = text2;
				argumentCompletions.ArgumentIndex = argumentIndex;
				argumentCompletions.ArgumentContext = text;
				if (string.IsNullOrEmpty(argumentCompletions.CommonPrefix))
				{
					argumentCompletions.CommonPrefix = inputBuffer;
				}
			}
			else
			{
				argumentCompletions.Type = type;
				argumentCompletions.CommandPrefix = text2;
				argumentCompletions.ArgumentIndex = argumentIndex;
				argumentCompletions.ArgumentContext = text;
			}
			return argumentCompletions;
		}
		if (array.Length > 1)
		{
			return new CompletionResult
			{
				Type = CompletionType.Command,
				CommandPrefix = "",
				ArgumentIndex = -1,
				ArgumentContext = ""
			};
		}
		List<string> possibleTokens = _commands.Values.Select((AbstractConsoleCmd s) => s.CmdName).ToList();
		return GetCompletionResultsFromTokens(text, possibleTokens, inputBuffer);
	}

	private static CompletionResult GetCompletionResultsFromTokens(string partialToken, List<string> possibleTokens, string originalInput)
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		foreach (string possibleToken in possibleTokens)
		{
			if (possibleToken.StartsWith(partialToken, StringComparison.CurrentCultureIgnoreCase))
			{
				list.Add(possibleToken);
			}
			else if (possibleToken.Contains(partialToken, StringComparison.CurrentCultureIgnoreCase))
			{
				list2.Add(possibleToken);
			}
		}
		List<string> list3 = list2;
		int num = list.Count + list3.Count;
		List<string> list4 = new List<string>(num);
		CollectionsMarshal.SetCount(list4, num);
		Span<string> span = CollectionsMarshal.AsSpan(list4);
		int num2 = 0;
		Span<string> span2 = CollectionsMarshal.AsSpan(list);
		span2.CopyTo(span.Slice(num2, span2.Length));
		num2 += span2.Length;
		Span<string> span3 = CollectionsMarshal.AsSpan(list3);
		span3.CopyTo(span.Slice(num2, span3.Length));
		num2 += span3.Length;
		List<string> list5 = list4;
		if (list5.Count == 0)
		{
			return new CompletionResult
			{
				Type = CompletionType.Command,
				CommandPrefix = "",
				ArgumentIndex = -1,
				ArgumentContext = ""
			};
		}
		if (list5.Count == 1)
		{
			string commonPrefix = ((list.Count == 1) ? (ReplaceLastToken(originalInput, list5.First()) + " ") : originalInput);
			return new CompletionResult
			{
				Candidates = list5,
				CommonPrefix = commonPrefix,
				Type = CompletionType.Command,
				CommandPrefix = "",
				ArgumentIndex = -1,
				ArgumentContext = ""
			};
		}
		return new CompletionResult
		{
			Candidates = list5,
			CommonPrefix = ReplaceLastToken(originalInput, LongestCommonSubstring(list5)),
			Type = CompletionType.Command,
			CommandPrefix = "",
			ArgumentIndex = -1,
			ArgumentContext = ""
		};
	}

	private static string ReplaceLastToken(string text, string replacement)
	{
		if (string.IsNullOrWhiteSpace(replacement))
		{
			return text;
		}
		string[] array = text.Trim().Split(' ');
		string[] array2 = ((array.Length != 0) ? array.Take(array.Length - 1).ToArray() : Array.Empty<string>());
		string text2 = ((array2.Length != 0) ? (string.Join(" ", array2) + " ") : string.Empty);
		return text2 + replacement;
	}

	private static string LongestCommonSubstring(List<string> cmdCandidates)
	{
		int num = cmdCandidates.Select((string cmd) => cmd.Length).Min();
		string text = cmdCandidates.First();
		for (int num2 = 0; num2 < int.MaxValue; num2++)
		{
			foreach (string cmdCandidate in cmdCandidates)
			{
				if (num == num2 || char.ToLowerInvariant(cmdCandidate[num2]) != char.ToLowerInvariant(text[num2]))
				{
					return cmdCandidate.Substring(0, num2);
				}
			}
		}
		return string.Empty;
	}

	public CmdResult ProcessCommand(string inputValue)
	{
		inputValue = inputValue.Trim();
		history.Enqueue(inputValue);
		historyIndex = 0;
		SaveCommandHistory();
		string[] array = inputValue.Split(' ');
		Player me = LocalContext.GetMe(RunManager.Instance.DebugOnlyGetState());
		if (!RunManager.Instance.IsSinglePlayerOrFakeMultiplayer && _commands.TryGetValue(array[0].ToLowerInvariant(), out AbstractConsoleCmd value) && value.IsNetworked && me != null)
		{
			RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(new ConsoleCmdGameAction(me, inputValue));
			return new CmdResult(success: true, $"Enqueued {array[0]} command: '{inputValue}'");
		}
		CmdResult result = ProcessCommandInternal(me, array);
		if (result.task != null)
		{
			TaskHelper.RunSafely(result.task);
		}
		return result;
	}

	public CmdResult ProcessNetCommand(Player? player, string inputValue)
	{
		Log.Info($"Executing DevConsole command (player {player?.NetId}): `{inputValue}`");
		string[] tokens = inputValue.Split(' ');
		return ProcessCommandInternal(player, tokens);
	}

	private CmdResult ProcessCommandInternal(Player? player, string[] tokens)
	{
		string cmdName = tokens[0];
		string[] args = tokens.Skip(1).ToArray();
		return ProcessCommand(player, cmdName, args);
	}

	private CmdResult ProcessCommand(Player? player, string cmdName, string[] args)
	{
		args = args.Where((string a) => !string.IsNullOrWhiteSpace(a)).ToArray();
		if (cmdName.Equals("help"))
		{
			return HelpCmd(args);
		}
		if (_commands.TryGetValue(cmdName.ToLowerInvariant(), out AbstractConsoleCmd value))
		{
			return value.Process(player, args);
		}
		return new CmdResult(success: false, "The command '" + cmdName + "' does not exist.\nYou can use the 'help' to get a list of all possible commands.\n");
	}

	private CmdResult HelpCmd(string[] args)
	{
		if (args.Length != 0)
		{
			if (!_commands.TryGetValue(args[0], out AbstractConsoleCmd value))
			{
				return new CmdResult(success: false, "No command named " + args[0] + " found!");
			}
			return new CmdResult(success: true, $"[gold]{value.CmdName}[/gold] {value.Args}\n\t{value.Description}");
		}
		int totalWidth = _commands.Values.Select((AbstractConsoleCmd c) => c.CmdName.Length).Max();
		StringBuilder stringBuilder = new StringBuilder().Append("Usage:\n");
		foreach (KeyValuePair<string, AbstractConsoleCmd> command in _commands)
		{
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(18, 2, stringBuilder2);
			handler.AppendLiteral("\t[gold]");
			handler.AppendFormatted(command.Value.CmdName.PadRight(totalWidth));
			handler.AppendLiteral("[/gold] - ");
			handler.AppendFormatted(command.Value.Description);
			handler.AppendLiteral("\n");
			stringBuilder2.Append(ref handler);
		}
		stringBuilder.Append("Use 'help <cmd>' to obtain help on a specific command.");
		return new CmdResult(success: true, stringBuilder.ToString());
	}
}
