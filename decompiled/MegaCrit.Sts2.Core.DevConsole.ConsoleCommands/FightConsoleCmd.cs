using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Exceptions;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class FightConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "fight";

	public override string Args => "<id:string>";

	public override string Description => "Jumps a player to a specific encounter.";

	public override bool IsNetworked => true;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (args.Length == 0)
		{
			return new CmdResult(success: false, "No encounter name specified.");
		}
		if (!RunManager.Instance.IsInProgress)
		{
			return new CmdResult(success: false, "A run is currently not in progress!");
		}
		ModelId modelId = new ModelId(ModelId.SlugifyCategory<EncounterModel>(), args[0].ToUpperInvariant());
		EncounterModel encounterModel;
		try
		{
			encounterModel = ModelDb.GetById<EncounterModel>(modelId).ToMutable();
		}
		catch (ModelNotFoundException)
		{
			return new CmdResult(success: false, "Encounter '" + modelId.Entry + "' not found");
		}
		encounterModel.DebugRandomizeRng();
		Task task = RunManager.Instance.EnterRoomDebug(RoomType.Monster, MapPointType.Unassigned, encounterModel);
		return new CmdResult(task, success: true, "Jumped to encounter: '" + encounterModel.Id.Entry + "'");
	}

	public override CompletionResult GetArgumentCompletions(Player? player, string[] args)
	{
		if (args.Length <= 1)
		{
			List<string> candidates = ModelDb.AllEncounters.Select((EncounterModel e) => e.Id.Entry).ToList();
			return CompleteArgument(candidates, Array.Empty<string>(), args.FirstOrDefault() ?? "");
		}
		return new CompletionResult
		{
			Type = CompletionType.Argument,
			ArgumentContext = CmdName
		};
	}
}
