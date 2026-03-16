using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.GameActions;

public class VoteToMoveToNextActAction : GameAction
{
	private readonly Player _player;

	public override ulong OwnerId => _player.NetId;

	public override GameActionType ActionType => GameActionType.NonCombat;

	public VoteToMoveToNextActAction(Player player)
	{
		_player = player;
	}

	protected override Task ExecuteAction()
	{
		RunManager.Instance.ActChangeSynchronizer.OnPlayerReady(_player);
		return Task.CompletedTask;
	}

	public override INetAction ToNetAction()
	{
		return default(NetVoteToMoveToNextActAction);
	}

	public override string ToString()
	{
		return $"{"VoteForMapCoordAction"} {_player.NetId}";
	}
}
