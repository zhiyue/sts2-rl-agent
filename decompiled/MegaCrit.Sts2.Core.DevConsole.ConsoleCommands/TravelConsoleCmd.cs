using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class TravelConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "travel";

	public override string Args => "";

	public override string Description => "Enables you to jump to any room on the map.";

	public override bool IsNetworked => true;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (!RunManager.Instance.IsInProgress)
		{
			return new CmdResult(success: false, "A run is currently not in progress...");
		}
		NMapScreen.Instance.SetDebugTravelEnabled(!NMapScreen.Instance.IsDebugTravelEnabled);
		string text = (NMapScreen.Instance.IsDebugTravelEnabled ? "enabled" : "disabled");
		return new CmdResult(success: true, "Travel mode " + text);
	}
}
