using System;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Platform.Steam;
using Steamworks;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class CloudConsoleCmd : AbstractConsoleCmd
{
	private static bool _confirmed;

	public override string CmdName => "cloud";

	public override string Args => "delete";

	public override string Description => "Deletes all save files from Steam Cloud, if you are running on Steam";

	public override bool IsNetworked => false;

	public override bool DebugOnly => false;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (args.Length < 1 || args[0] != "delete")
		{
			return new CmdResult(success: false, "No argument specified.\n" + Args);
		}
		if (!_confirmed)
		{
			_confirmed = true;
			return new CmdResult(success: false, "Run this command again to confirm you want to delete all your Steam cloud saves. The game will quit.");
		}
		DeleteCloudSaves();
		return new CmdResult(success: true, "Steam cloud saves deleted.");
	}

	public static void DeleteCloudSaves()
	{
		if (!SteamInitializer.Initialized)
		{
			throw new InvalidOperationException("Steam not initialized");
		}
		int fileCount = SteamRemoteStorage.GetFileCount();
		for (int num = fileCount - 1; num >= 0; num--)
		{
			int pnFileSizeInBytes;
			string fileNameAndSize = SteamRemoteStorage.GetFileNameAndSize(num, out pnFileSizeInBytes);
			Log.Info($"Deleting {fileNameAndSize} from Steam cloud ({pnFileSizeInBytes} bytes)");
			SteamRemoteStorage.FileDelete(fileNameAndSize);
		}
		((SceneTree)Engine.GetMainLoop()).Quit();
	}
}
