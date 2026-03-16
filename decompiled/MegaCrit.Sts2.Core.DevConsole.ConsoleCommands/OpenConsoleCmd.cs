using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class OpenConsoleCmd : AbstractConsoleCmd
{
	private static readonly IReadOnlyCollection<string> _options = new global::_003C_003Ez__ReadOnlyArray<string>(new string[5] { "logs", "saves", "root", "build-logs", "loc-override" });

	public override string CmdName => "open";

	public override string Args => "logs|saves|root|build-logs|loc-override";

	public override string Description => "Opens a common path in the local OS file browser.";

	public override bool IsNetworked => false;

	public override bool DebugOnly => false;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (args.Length < 1)
		{
			return new CmdResult(success: false, "No argument specified.\n" + Args);
		}
		if (!_options.Contains(args[0]))
		{
			return new CmdResult(success: false, "Argument '" + args[0] + "' unrecognized.\n" + Args);
		}
		string userDataDir = OS.GetUserDataDir();
		if (userDataDir == null)
		{
			return new CmdResult(success: false, "Unable to open the user data directory.");
		}
		string dataDir = OS.GetDataDir();
		string text = args[0] switch
		{
			"logs" => Path.Combine(userDataDir, "logs"), 
			"saves" => ProjectSettings.GlobalizePath(SaveManager.Instance.GetProfileScopedPath("saves")), 
			"root" => userDataDir, 
			"build-logs" => Path.Combine(dataDir, "Godot", "mono", "build_logs"), 
			"loc-override" => ProjectSettings.GlobalizePath("user://localization_override"), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			text = text.Replace('/', '\\');
		}
		Error error = OS.ShellShowInFileManager(text);
		if (error != Error.Ok)
		{
			return new CmdResult(success: false, $"Error {error}: Cannot open OS file manager.");
		}
		return new CmdResult(success: true, "Opened '" + text + "'");
	}

	public override CompletionResult GetArgumentCompletions(Player? player, string[] args)
	{
		if (args.Length <= 1)
		{
			return CompleteArgument(_options, Array.Empty<string>(), args.FirstOrDefault() ?? "");
		}
		return new CompletionResult
		{
			Type = CompletionType.Argument,
			ArgumentContext = CmdName
		};
	}
}
