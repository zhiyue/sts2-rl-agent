using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.GameActions;

/// <summary>
/// An action enqueued at the map screen when the player picks/votes for a map coord to move to.
/// Once all players have selected a map coord, the host decides which map coord to move to based on the votes. It then
/// enqueues MoveToMapCoordAction which triggers entering the room.
/// This action may be enqueued multiple times with the same origin location, and should overwrite which coord the owning
/// player is voting for. If it is enqueued after MoveToMapCoordAction has already been executed, then it should be
/// ignored.
/// </summary>
public class VoteForMapCoordAction : GameAction
{
	/// <summary>
	/// The player who is voting.
	/// </summary>
	private readonly Player _player;

	/// <summary>
	/// The location that the player is currently at while casting the vote.
	/// </summary>
	private readonly MapLocation _source;

	/// <summary>
	/// The player's vote. If null, any existing votes for the player should be cancelled.
	/// </summary>
	private readonly MapVote? _destination;

	public override ulong OwnerId => _player.NetId;

	public override GameActionType ActionType => GameActionType.NonCombat;

	public VoteForMapCoordAction(Player player, MapLocation source, MapVote? destination)
	{
		_player = player;
		_source = source;
		_destination = destination;
	}

	protected override Task ExecuteAction()
	{
		RunManager.Instance.MapSelectionSynchronizer.PlayerVotedForMapCoord(_player, _source, _destination);
		return Task.CompletedTask;
	}

	public override INetAction ToNetAction()
	{
		return new NetVoteForMapCoordAction
		{
			source = _source,
			destination = _destination
		};
	}

	public override string ToString()
	{
		return $"{"VoteForMapCoordAction"} {_player.NetId} {_source}->{_destination}";
	}
}
