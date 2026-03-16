using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class DumpConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "dump";

	public override string Args => "";

	public override string Description => "Dumps Model ID database to console & logs.";

	public override bool IsNetworked => false;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		Log.Info(ModelIdSerializationCache.Dump());
		return new CmdResult(success: true, "Model ID database dumped to console & logs");
	}
}
