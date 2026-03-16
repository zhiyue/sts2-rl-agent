using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class StarsConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "stars";

	public override string Args => "<amount:int>";

	public override string Description => "Adds stars to player";

	public override bool IsNetworked => true;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (args.Length == 0)
		{
			return new CmdResult(success: false, "The first argument must be an int.");
		}
		if (int.TryParse(args[0], out var result))
		{
			if (issuingPlayer == null)
			{
				return new CmdResult(success: false, "This command only works during a run.");
			}
			if (issuingPlayer.PlayerCombatState == null)
			{
				return new CmdResult(success: false, "This command only works in combat.");
			}
			if (result < 0)
			{
				return new CmdResult(success: false, "The stars amount cannot be negative.");
			}
			Task task = PlayerCmd.GainStars(result, issuingPlayer);
			return new CmdResult(task, success: true, $"Added '{result}' stars.");
		}
		return new CmdResult(success: false, "The first argument must be an int.");
	}
}
