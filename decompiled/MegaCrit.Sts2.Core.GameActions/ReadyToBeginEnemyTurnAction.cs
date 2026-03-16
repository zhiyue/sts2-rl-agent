using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;

namespace MegaCrit.Sts2.Core.GameActions;

public class ReadyToBeginEnemyTurnAction : GameAction
{
	private readonly Player _player;

	private readonly Func<Task>? _actionDuringEnemyTurn;

	public override ulong OwnerId => _player.NetId;

	public override GameActionType ActionType => GameActionType.Combat;

	public ReadyToBeginEnemyTurnAction(Player player, Func<Task>? actionDuringEnemyTurn = null)
	{
		_player = player;
		_actionDuringEnemyTurn = actionDuringEnemyTurn;
	}

	protected override Task ExecuteAction()
	{
		CombatManager.Instance.SetReadyToBeginEnemyTurn(_player, _actionDuringEnemyTurn);
		return Task.CompletedTask;
	}

	public override INetAction ToNetAction()
	{
		return default(NetReadyToBeginEnemyTurnAction);
	}

	protected override void CancelAction()
	{
		Log.Debug($"Cancel\n{new StackTrace()}");
	}

	public override string ToString()
	{
		return $"{"ReadyToBeginEnemyTurnAction"} {_player.NetId}";
	}
}
