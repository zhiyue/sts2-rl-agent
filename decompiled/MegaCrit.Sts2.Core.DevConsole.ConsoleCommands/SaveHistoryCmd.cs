using System;
using System.IO;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Debug;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class SaveHistoryCmd : AbstractConsoleCmd
{
	public override string CmdName => "log-history";

	public override string Args => "";

	public override string Description => "Saves command history and opens a common path to it in the local OS file browser.";

	public override bool IsNetworked => false;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		string userDataDir = OS.GetUserDataDir();
		string text = Path.Combine(userDataDir, "debug_history");
		Directory.CreateDirectory(text);
		Error error = OS.ShellShowInFileManager(text);
		string path = Path.Combine(text, $"history-{DateTime.Now.ToFileTime()}.log");
		string history = NCommandHistory.GetHistory();
		File.WriteAllText(path, history);
		if (error != Error.Ok)
		{
			return new CmdResult(success: false, $"Error {error}: Cannot open OS file manager.");
		}
		return new CmdResult(success: true, "Opened '" + text + "'");
	}
}
