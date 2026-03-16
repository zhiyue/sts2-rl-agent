using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.GameActions;

public class MoveToMapCoordAction : GameAction
{
	private readonly Player _player;

	private readonly MapCoord _destination;

	public override ulong OwnerId => _player.NetId;

	public override GameActionType ActionType => GameActionType.NonCombat;

	public MoveToMapCoordAction(Player player, MapCoord destination)
	{
		_player = player;
		_destination = destination;
	}

	protected override Task ExecuteAction()
	{
		TaskHelper.RunSafely(GoToMapCoord());
		return Task.CompletedTask;
	}

	private async Task GoToMapCoord()
	{
		if (TestMode.IsOn)
		{
			await RunManager.Instance.EnterMapCoord(_destination);
		}
		else
		{
			await NMapScreen.Instance.TravelToMapCoord(_destination);
		}
	}

	public override INetAction ToNetAction()
	{
		return new NetMoveToMapCoordAction
		{
			destination = _destination
		};
	}

	public override string ToString()
	{
		return $"{"MoveToMapCoordAction"} {_player.NetId} {_destination}";
	}
}
