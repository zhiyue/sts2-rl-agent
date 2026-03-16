using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Game;

public class MapSelectionSynchronizer
{
	private readonly INetGameService _netService;

	private readonly ActionQueueSynchronizer _actionQueueSynchronizer;

	private readonly RunState _runState;

	private RunLocation _acceptingVotesFromSource;

	private readonly List<MapVote?> _votes = new List<MapVote?>();

	private readonly Logger _logger = new Logger("MapSelectionSynchronizer", LogType.GameSync);

	private readonly Rng _multiplayerMapPointSelection;

	public int MapGenerationCount { get; private set; }

	public event Action<Player, MapVote?, MapVote?>? PlayerVoteChanged;

	public event Action<Player>? PlayerVoteCancelled;

	public event Action? PlayerVotesCleared;

	public MapSelectionSynchronizer(INetGameService netService, ActionQueueSynchronizer actionQueueSynchronizer, RunState runState)
	{
		_netService = netService;
		_actionQueueSynchronizer = actionQueueSynchronizer;
		_runState = runState;
		_multiplayerMapPointSelection = new Rng(_runState.Rng.Seed);
		OnRunLocationChanged(_runState.CurrentLocation);
	}

	public void PlayerVotedForMapCoord(Player player, RunLocation source, MapVote? destination)
	{
		if (_acceptingVotesFromSource != source)
		{
			_logger.Warn($"Received map vote from player {player.NetId} for source {source}, but we're currently accepting votes for {_acceptingVotesFromSource}");
			return;
		}
		if (destination?.mapGenerationCount < MapGenerationCount)
		{
			_logger.Warn($"Received map vote from player {player.NetId} for destination {destination}, but the map generation count is lower than our current: {MapGenerationCount}");
		}
		int playerSlotIndex = _runState.GetPlayerSlotIndex(player);
		MapVote? arg = _votes[playerSlotIndex];
		_votes[playerSlotIndex] = destination;
		this.PlayerVoteChanged?.Invoke(player, arg, destination);
		if (destination.HasValue)
		{
			_logger.Debug($"Received vote to move to {destination} from player {player.NetId} (slot {playerSlotIndex})");
		}
		else
		{
			_logger.Debug($"Received cancellation of vote from player {player.NetId} (slot {playerSlotIndex})");
		}
		if (_votes.All((MapVote? p) => p.HasValue && p.Value.mapGenerationCount == MapGenerationCount) && _netService.Type != NetGameType.Client)
		{
			_logger.Debug("All votes received and we are host, choosing coordinate");
			MoveToMapCoord();
		}
	}

	public MapVote? GetVote(Player player)
	{
		return _votes[_runState.GetPlayerSlotIndex(player)];
	}

	private void MoveToMapCoord()
	{
		if (_netService.Type == NetGameType.Client)
		{
			throw new InvalidOperationException("Only host should be moving to new map point!");
		}
		MapCoord coord = _multiplayerMapPointSelection.NextItem(_votes).Value.coord;
		_acceptingVotesFromSource.coord = coord;
		_logger.Debug($"Moving to coordinate {coord}");
		MoveToMapCoordAction action = new MoveToMapCoordAction(LocalContext.GetMe(_runState), coord);
		_actionQueueSynchronizer.RequestEnqueue(action);
	}

	public void OnRunLocationChanged(RunLocation location)
	{
		_acceptingVotesFromSource = location;
		_votes.Clear();
		for (int i = 0; i < _runState.Players.Count; i++)
		{
			_votes.Add(null);
		}
		this.PlayerVotesCleared?.Invoke();
	}

	public void BeforeMapGenerated()
	{
		MapGenerationCount++;
		for (int i = 0; i < _votes.Count; i++)
		{
			MapVote? mapVote = _votes[i];
			if (mapVote.HasValue && mapVote.Value.mapGenerationCount < MapGenerationCount)
			{
				Player player = _runState.Players[i];
				_logger.Debug($"Cancelling map vote for player {player.NetId} because the map has re-generated and their vote is old");
				_votes[i] = null;
				this.PlayerVoteCancelled?.Invoke(player);
			}
		}
	}
}
