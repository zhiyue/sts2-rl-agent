using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class GoldConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "gold";

	public override string Args => "<amount:int>";

	public override string Description => "Manipulate player gold. Cha-ching!";

	public override bool IsNetworked => true;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (args.Length < 1)
		{
			return new CmdResult(success: false, "An amount is required");
		}
		if (!int.TryParse(args[0], out var result))
		{
			return new CmdResult(success: false, "First argument (the gold amount) must be an int.");
		}
		if (issuingPlayer == null || !RunManager.Instance.IsInProgress)
		{
			return new CmdResult(success: false, "A run does not appear to be in progress");
		}
		Task task = PlayerCmd.GainGold(result, issuingPlayer);
		return new CmdResult(task, success: true, $"'{result}' gold added.");
	}
}
