using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.GameActions;

public class UndoEndPlayerTurnAction : GameAction
{
	private readonly Player _player;

	private readonly int _combatRound;

	public override ulong OwnerId => _player.NetId;

	public override GameActionType ActionType => GameActionType.CombatPlayPhaseOnly;

	public UndoEndPlayerTurnAction(Player player, int combatRound)
	{
		_player = player;
		_combatRound = combatRound;
	}

	protected override Task ExecuteAction()
	{
		int roundNumber = _player.Creature.CombatState.RoundNumber;
		if (roundNumber == _combatRound)
		{
			CombatManager.Instance.UndoReadyToEndTurn(_player);
		}
		else
		{
			Log.Info($"Ignoring undo end turn action. Current round number: {roundNumber} action round number: {_combatRound} CombatState: {RunManager.Instance.ActionQueueSynchronizer.CombatState}");
		}
		return Task.CompletedTask;
	}

	public override INetAction ToNetAction()
	{
		return new NetUndoEndPlayerTurnAction
		{
			combatRound = _combatRound
		};
	}

	public override string ToString()
	{
		return $"{"UndoEndPlayerTurnAction"} for player {_player.NetId} round {_combatRound}";
	}
}
