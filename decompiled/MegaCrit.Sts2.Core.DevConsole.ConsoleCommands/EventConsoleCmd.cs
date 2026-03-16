using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class EventConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "event";

	public override string Args => "<id:string>";

	public override string Description => "Jumps a player to a specific event.";

	public override bool IsNetworked => true;

	private static IEnumerable<EventModel> Events => ModelDb.AllEvents.Concat(ModelDb.AllAncients);

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (args.Length == 0)
		{
			return new CmdResult(success: false, "No event name specified.");
		}
		if (!RunManager.Instance.IsInProgress)
		{
			return new CmdResult(success: false, "A run is currently not in progress!");
		}
		string eventName = args[0].ToUpperInvariant();
		EventModel eventModel = Events.FirstOrDefault((EventModel c) => c.Id.Entry == eventName);
		if (eventModel == null)
		{
			return new CmdResult(success: false, "Event '" + eventName + "' not found");
		}
		MapPointType mapPointType = ((!(eventModel is AncientEventModel)) ? MapPointType.Unknown : MapPointType.Ancient);
		issuingPlayer.RunState.AppendToMapPointHistory(mapPointType, RoomType.Event, eventModel.Id);
		Task task = RunManager.Instance.EnterRoom(new EventRoom(eventModel));
		return new CmdResult(task, success: true, "Jumped to event: '" + eventModel.Id.Entry + "'");
	}

	public override CompletionResult GetArgumentCompletions(Player? player, string[] args)
	{
		if (args.Length <= 1)
		{
			List<string> candidates = Events.Select((EventModel e) => e.Id.Entry).ToList();
			return CompleteArgument(candidates, Array.Empty<string>(), args.FirstOrDefault() ?? "");
		}
		return new CompletionResult
		{
			Type = CompletionType.Argument,
			ArgumentContext = CmdName
		};
	}
}
