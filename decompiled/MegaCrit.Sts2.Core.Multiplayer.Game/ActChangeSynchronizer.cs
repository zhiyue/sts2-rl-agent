using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Game;

public class ActChangeSynchronizer
{
	private readonly RunState _runState;

	private readonly List<bool> _readyPlayers = new List<bool>();

	private readonly Logger _logger = new Logger("ActChangeSynchronizer", LogType.GameSync);

	public ActChangeSynchronizer(RunState runState)
	{
		_runState = runState;
		for (int i = 0; i < runState.Players.Count; i++)
		{
			_readyPlayers.Add(item: false);
		}
	}

	public void SetLocalPlayerReady()
	{
		_logger.Info("Local player ready to move to next act");
		RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(new VoteToMoveToNextActAction(LocalContext.GetMe(_runState)));
	}

	public bool IsWaitingForOtherPlayers()
	{
		int playerSlotIndex = _runState.GetPlayerSlotIndex(LocalContext.NetId.Value);
		for (int i = 0; i < _readyPlayers.Count; i++)
		{
			if (!_readyPlayers[i] && i != playerSlotIndex)
			{
				return true;
			}
		}
		return false;
	}

	public void OnPlayerReady(Player player)
	{
		_logger.Debug($"Player {player.NetId} ready to move to next act");
		int playerSlotIndex = _runState.GetPlayerSlotIndex(player);
		_readyPlayers[playerSlotIndex] = true;
		if (_readyPlayers.All((bool x) => x))
		{
			MoveToNextAct();
		}
	}

	private void MoveToNextAct()
	{
		for (int i = 0; i < _readyPlayers.Count; i++)
		{
			_readyPlayers[i] = false;
		}
		_logger.Info("All players ready to move to next act, beginning transition");
		_runState.ActFloor++;
		TaskHelper.RunSafely(RunManager.Instance.EnterNextAct());
		if (NOverlayStack.Instance?.Peek() is NRewardsScreen nRewardsScreen)
		{
			nRewardsScreen.HideWaitingForPlayersScreen();
		}
	}
}
