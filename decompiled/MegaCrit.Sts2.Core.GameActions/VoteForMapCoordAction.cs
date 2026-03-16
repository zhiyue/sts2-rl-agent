using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.GameActions;

public class VoteForMapCoordAction : GameAction
{
	private readonly Player _player;

	private readonly RunLocation _source;

	private readonly MapVote? _destination;

	public override ulong OwnerId => _player.NetId;

	public override GameActionType ActionType => GameActionType.NonCombat;

	public VoteForMapCoordAction(Player player, RunLocation source, MapVote? destination)
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
