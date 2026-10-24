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

	public override GameActionType ActionType
	{
		get
		{
			if (!InCombat)
			{
				return GameActionType.Any;
			}
			return GameActionType.CombatPlayPhaseOnly;
		}
	}

	public Player Player { get; }

	public string Cmd { get; }

	public bool InCombat { get; }

	public ConsoleCmdGameAction(Player player, string cmd, bool inCombat)
	{
		Player = player;
		Cmd = cmd;
		InCombat = inCombat;
	}

	protected override async Task ExecuteAction()
	{
		await NDevConsole.Instance.ProcessNetCommand(Player, Cmd);
	}

	public override INetAction ToNetAction()
	{
		return new NetConsoleCmdGameAction
		{
			cmd = Cmd,
			inCombat = InCombat
		};
	}

	public override string ToString()
	{
		return $"{"ConsoleCmdGameAction"} player {Player.NetId} cmd {Cmd} InCombat {InCombat}";
	}
}
