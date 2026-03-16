using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class DieConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "die";

	public override string Args => "";

	public override string Description => "You die";

	public override bool IsNetworked => true;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (issuingPlayer == null || !RunManager.Instance.IsInProgress)
		{
			return new CmdResult(success: false, "A run does not appear to be in progress");
		}
		Task task = CreatureCmd.Kill(issuingPlayer.Creature);
		return new CmdResult(task, success: true, "You died.");
	}
}
