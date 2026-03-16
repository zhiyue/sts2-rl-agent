using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class InstantConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "instant";

	public override string Args => "";

	public override string Description => "Turns instant mode on.";

	public override bool IsNetworked => false;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (SaveManager.Instance.PrefsSave.FastMode == FastModeType.Instant)
		{
			SaveManager.Instance.PrefsSave.FastMode = FastModeType.Fast;
			return new CmdResult(success: true, "Instant mode off");
		}
		SaveManager.Instance.PrefsSave.FastMode = FastModeType.Instant;
		return new CmdResult(success: true, "INSTANT MODE ACTIVE");
	}
}
