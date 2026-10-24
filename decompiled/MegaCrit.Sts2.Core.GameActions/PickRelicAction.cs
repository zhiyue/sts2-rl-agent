using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.GameActions;

/// <summary>
/// Enqueued when a player picks a relic at a multiplayer treasure room.
/// </summary>
public class PickRelicAction : GameAction
{
	private readonly Player _player;

	private readonly int? _relicIndex;

	public override ulong OwnerId => _player.NetId;

	public override GameActionType ActionType => GameActionType.NonCombat;

	/// <summary>
	/// Used to inject a relic picking synchronizer in tests.
	/// </summary>
	public TreasureRoomRelicSynchronizer? TestSynchronizer { get; set; }

	public PickRelicAction(Player player, int? relicIndex)
	{
		_player = player;
		_relicIndex = relicIndex;
	}

	protected override Task ExecuteAction()
	{
		TreasureRoomRelicSynchronizer treasureRoomRelicSynchronizer = TestSynchronizer ?? RunManager.Instance.TreasureRoomRelicSynchronizer;
		treasureRoomRelicSynchronizer.OnPicked(_player, _relicIndex);
		return Task.CompletedTask;
	}

	public override INetAction ToNetAction()
	{
		return new NetPickRelicAction
		{
			relicIndex = _relicIndex
		};
	}

	public override string ToString()
	{
		return $"{"NetPickRelicAction"} for player {_player.NetId} index {_relicIndex}";
	}
}
