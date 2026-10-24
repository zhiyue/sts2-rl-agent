using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.GameActions;

/// <summary>
/// An action enqueued at the rewards screen when the player is ready to move to the next act.
/// Once a player receives actions from all other players indicating that they're ready to move to the next act, then
/// the player should begin transitioning to the next act.
/// </summary>
public class VoteToMoveToNextActAction : GameAction
{
	/// <summary>
	/// The player who is voting.
	/// </summary>
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
