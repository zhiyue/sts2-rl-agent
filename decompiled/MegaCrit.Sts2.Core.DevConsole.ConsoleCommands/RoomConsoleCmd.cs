using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class RoomConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "room";

	public override string Args => "<id:string>";

	public override string Description => "Jumps a player to a specific room.";

	public override bool IsNetworked => true;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (args.Length == 0)
		{
			return new CmdResult(success: false, "No room name specified.");
		}
		if (!RunManager.Instance.IsInProgress)
		{
			return new CmdResult(success: false, "A run is currently not in progress!");
		}
		string text = args[0].ToUpperInvariant();
		if (!AbstractConsoleCmd.TryParseEnum<RoomType>(text, out var result))
		{
			return new CmdResult(success: false, "Room '" + text + "' not found");
		}
		Task task = RunManager.Instance.EnterRoomDebug(result);
		return new CmdResult(task, success: true, $"Jumped to room: '{result}'");
	}

	public override CompletionResult GetArgumentCompletions(Player? player, string[] args)
	{
		if (args.Length <= 1)
		{
			List<string> candidates = (from n in Enum.GetNames(typeof(RoomType))
				where !n.Equals("Unassigned")
				select n).ToList();
			return CompleteArgument(candidates, Array.Empty<string>(), args.FirstOrDefault() ?? "");
		}
		return new CompletionResult
		{
			Type = CompletionType.Argument,
			ArgumentContext = CmdName
		};
	}
}
