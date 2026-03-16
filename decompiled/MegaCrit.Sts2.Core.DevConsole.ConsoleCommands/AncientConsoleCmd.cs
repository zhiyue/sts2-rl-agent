using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class AncientConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "ancient";

	public override string Args => "<id:string> <choice:string>";

	public override string Description => "Opens an ancient event with the selected choice";

	public override bool IsNetworked => true;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (args.Length == 0)
		{
			return new CmdResult(success: false, "No ancient ID specified.");
		}
		ModelId id = new ModelId(ModelDb.GetCategory(typeof(EventModel)), args[0].ToUpperInvariant());
		EventModel byIdOrNull = ModelDb.GetByIdOrNull<EventModel>(id);
		if (!(byIdOrNull is AncientEventModel ancientEventModel))
		{
			return new CmdResult(success: false, "Invalid ancient ID.");
		}
		string choice = null;
		if (args.Length > 1)
		{
			choice = args[1].ToUpperInvariant();
		}
		if (choice != null && !ancientEventModel.AllPossibleOptions.Any((EventOption option) => option.TextKey.Contains(choice)))
		{
			return new CmdResult(success: false, "invalid ancient choice.");
		}
		EventRoom room = new EventRoom(byIdOrNull)
		{
			OnStart = SetDebugOption
		};
		issuingPlayer.RunState.AppendToMapPointHistory(MapPointType.Ancient, RoomType.Event, byIdOrNull.Id);
		Task task = RunManager.Instance.EnterRoom(room);
		return new CmdResult(task, success: true, "Opened Ancient Event. Forced " + (choice ?? "no") + " option");
		void SetDebugOption(EventModel e)
		{
			((AncientEventModel)e).DebugOption = choice;
		}
	}

	public override CompletionResult GetArgumentCompletions(Player? player, string[] args)
	{
		if (args.Length <= 1)
		{
			List<string> candidates = ModelDb.AllAncients.Select((AncientEventModel ancient) => ancient.Id.Entry).ToList();
			return CompleteArgument(candidates, Array.Empty<string>(), args.FirstOrDefault() ?? "");
		}
		if (args.Length == 2)
		{
			ModelId id = new ModelId(ModelDb.GetCategory(typeof(EventModel)), args[0].ToUpperInvariant());
			EventModel byIdOrNull = ModelDb.GetByIdOrNull<EventModel>(id);
			if (byIdOrNull is AncientEventModel ancientEventModel)
			{
				List<string> candidates2 = ancientEventModel.AllPossibleOptions.Select((EventOption option) => option.TextKey.Split('.').Last()).ToList();
				return CompleteArgument(candidates2, new string[1] { args[0] }, args[1]);
			}
		}
		return new CompletionResult
		{
			Type = CompletionType.Argument,
			ArgumentContext = CmdName
		};
	}
}
