using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class DrawConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "draw";

	public override string Args => "<count:int>";

	public override string Description => "Draw X many cards.";

	public override bool IsNetworked => true;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		int num;
		if (args.Length == 0)
		{
			num = 1;
		}
		else
		{
			if (!int.TryParse(args[0], out var result))
			{
				return new CmdResult(success: false, "First argument is not an int");
			}
			num = result;
		}
		if (num <= 0)
		{
			return new CmdResult(success: false, "Draw nothing?");
		}
		if (!RunManager.Instance.IsInProgress || issuingPlayer == null)
		{
			return new CmdResult(success: false, "A run hasn't started");
		}
		Task task = DrawTask(issuingPlayer, num);
		return new CmdResult(task, success: true, $"Drawn '{num}' cards.");
	}

	private async Task DrawTask(Player player, int count)
	{
		HookPlayerChoiceContext hookPlayerChoiceContext = new HookPlayerChoiceContext(player, LocalContext.NetId.Value, GameActionType.Combat);
		Task task = CardPileCmd.Draw(hookPlayerChoiceContext, count, player);
		await hookPlayerChoiceContext.AssignTaskAndWaitForPauseOrCompletion(task);
	}
}
