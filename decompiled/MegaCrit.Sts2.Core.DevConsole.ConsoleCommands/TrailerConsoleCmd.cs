using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class TrailerConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "trailer";

	public override string Args => "";

	public override string Description => "Toggles the ability to show and hide UI elements via 0 - 9 and +- keys.";

	public override bool IsNetworked => false;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		NGame.ToggleTrailerMode();
		string text = (NGame.IsTrailerMode ? "enabled" : "disabled");
		return new CmdResult(success: true, "Trailer mode " + text);
	}
}
