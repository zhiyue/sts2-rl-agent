using System;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class LogConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "log";

	public override string Args => "[type:string] <level:string>";

	public override string Description => "Set log level for specific log types. Type can be: " + string.Join(",", Enum.GetNames<LogType>()) + ". Levels can be: " + string.Join(",", Enum.GetNames<LogLevel>());

	public override bool IsNetworked => false;

	public override bool DebugOnly => false;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (args.Length == 0)
		{
			return new CmdResult(success: false, "At least one arg must be supplied!");
		}
		LogType? enumVal2;
		if (TryParseEnumCaseInsensitive(args[0], out LogLevel? enumVal))
		{
			enumVal2 = LogType.Generic;
		}
		else
		{
			if (!TryParseEnumCaseInsensitive(args[0], out enumVal2))
			{
				return new CmdResult(success: false, "First argument '" + args[0] + "' could not be parsed as either a log level or type!");
			}
			if (args.Length <= 1)
			{
				return new CmdResult(success: false, "Must supply a log level as the second argument!");
			}
			if (!TryParseEnumCaseInsensitive(args[1], out enumVal))
			{
				return new CmdResult(success: false, "Second argument '" + args[1] + "' could not be parsed as a log level!");
			}
		}
		Logger.logLevelTypeMap[enumVal2.Value] = enumVal.Value;
		return new CmdResult(success: true, $"Logging level for {enumVal2} set to {enumVal}");
	}

	public static bool TryParseEnumCaseInsensitive<T>(string str, out T? enumVal) where T : struct, Enum
	{
		T[] values = Enum.GetValues<T>();
		for (int i = 0; i < values.Length; i++)
		{
			T value = values[i];
			if (str.Equals(value.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				enumVal = value;
				return true;
			}
		}
		enumVal = null;
		return false;
	}
}
