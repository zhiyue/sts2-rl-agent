using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class ActConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "act";

	public override string Args => "<int|string: act>";

	public override string Description => "Jumps to an act. If integer, will jump to that act. Otherwise, replaces the current act with the act passed.";

	public override bool IsNetworked => true;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (args.Length != 1)
		{
			return new CmdResult(success: false, "There must be one argument.");
		}
		if (issuingPlayer?.RunState == null)
		{
			return new CmdResult(success: false, "This command only works during a run.");
		}
		if (int.TryParse(args[0], out var result))
		{
			int count = issuingPlayer.RunState.Acts.Count;
			if (result > count || result < 1)
			{
				return new CmdResult(success: false, $"The act you are trying to navigate to does not exist. Select act indexes between: 1-{count}");
			}
			int actIndex = result - 1;
			Task task = NextAct(actIndex);
			return new CmdResult(task, success: true, $"Navigated to act '{result}'.");
		}
		string actName = args[0].ToUpperInvariant();
		ActModel actModel = ModelDb.Acts.FirstOrDefault((ActModel c) => c.Id.Entry == actName);
		if (actModel == null)
		{
			return new CmdResult(success: false, "Act named " + actName + " not found.");
		}
		RunState runState = (RunState)issuingPlayer.RunState;
		actModel = actModel.ToMutable();
		runState.SetActDebug(actModel);
		actModel.GenerateRooms(runState.Rng.UpFront, runState.UnlockState, runState.Players.Count > 1);
		Task task2 = RunManager.Instance.EnterAct(runState.CurrentActIndex);
		return new CmdResult(task2, success: true, "Replaced current act with act " + actName + ".");
	}

	private static async Task NextAct(int actIndex)
	{
		NMapScreen.Instance.SetTravelEnabled(enabled: true);
		await RunManager.Instance.EnterAct(actIndex);
	}
}
