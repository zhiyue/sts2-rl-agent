using Godot;
using Godot.Collections;

namespace MegaCrit.Sts2.Core.Helpers;

public static class CommandLineHelper
{
	private static readonly Dictionary<string, string?> _args;

	static CommandLineHelper()
	{
		_args = new Dictionary<string, string>();
		string[] cmdlineArgs = OS.GetCmdlineArgs();
		for (int i = 0; i < cmdlineArgs.Length; i++)
		{
			string text = cmdlineArgs[i].TrimStart('-');
			string key = text;
			string value = null;
			int num = text.IndexOf('=');
			if (num > 0)
			{
				key = text.Substring(0, num);
				value = text.Substring(num + 1);
			}
			else if (i + 1 < cmdlineArgs.Length && !cmdlineArgs[i + 1].StartsWith('-') && !cmdlineArgs[i + 1].StartsWith('+'))
			{
				value = cmdlineArgs[i + 1];
				i++;
			}
			_args[key] = value;
		}
	}

	public static bool HasArg(string key)
	{
		return _args.ContainsKey(key);
	}

	public static bool TryGetValue(string key, out string? value)
	{
		return _args.TryGetValue(key, out value);
	}

	public static string? GetValue(string key)
	{
		if (!TryGetValue(key, out string value))
		{
			return null;
		}
		return value;
	}
}
