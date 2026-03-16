using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Debug;

namespace MegaCrit.Sts2.Core.DevConsole;

public class ConsoleCmdGameAction : GameAction
{
	public override ulong OwnerId => Player.NetId;

	public override GameActionType ActionType => GameActionType.NonCombat;

	public Player Player { get; private set; }

	public string Cmd { get; private set; }

	public ConsoleCmdGameAction(Player player, string cmd)
	{
		Player = player;
		Cmd = cmd;
	}

	protected override async Task ExecuteAction()
	{
		await NDevConsole.Instance.ProcessNetCommand(Player, Cmd);
	}

	public override INetAction ToNetAction()
	{
		return new NetConsoleCmdGameAction
		{
			cmd = Cmd
		};
	}
}
