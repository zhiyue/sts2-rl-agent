using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.GameActions.Multiplayer;

public class PlayerChoiceSynchronizer : IDisposable
{
	private struct ReceivedChoice
	{
		public ulong senderId;

		public uint choiceId;

		public TaskCompletionSource<NetPlayerChoiceResult> completionSource;
	}

	private readonly List<uint> _choiceIds = new List<uint>();

	private readonly List<ReceivedChoice> _receivedChoices = new List<ReceivedChoice>();

	private readonly Logger _logger = new Logger("PlayerChoiceSynchronizer", LogType.Actions);

	private readonly INetGameService _netService;

	private readonly IPlayerCollection _players;

	public IReadOnlyList<uint> ChoiceIds => _choiceIds;

	public event Action<Player, uint, NetPlayerChoiceResult>? PlayerChoiceReceived;

	public PlayerChoiceSynchronizer(INetGameService netService, IPlayerCollection players)
	{
		_players = players;
		_netService = netService;
		netService.RegisterMessageHandler<PlayerChoiceMessage>(OnPlayerChoiceMessageReceived);
	}

	public void Dispose()
	{
		_netService.UnregisterMessageHandler<PlayerChoiceMessage>(OnPlayerChoiceMessageReceived);
	}

	public uint ReserveChoiceId(Player player)
	{
		int playerSlotIndex = _players.GetPlayerSlotIndex(player);
		while (_choiceIds.Count <= playerSlotIndex)
		{
			_choiceIds.Add(0u);
		}
		uint num = _choiceIds[playerSlotIndex];
		_choiceIds[playerSlotIndex] = num + 1;
		_logger.VeryDebug($"Reserved choice id {num} for player {player.NetId}, next is {_choiceIds[playerSlotIndex]}");
		return num;
	}

	public void SyncLocalChoice(Player player, uint choiceId, PlayerChoiceResult result)
	{
		if (!ValidateChoiceId(player, choiceId))
		{
			throw new InvalidOperationException($"Tried to sync local choice with ID {choiceId} for player {player.NetId}, but player's choice ID is {GetChoiceId(player)}!");
		}
		PlayerChoiceMessage message = new PlayerChoiceMessage
		{
			result = result.ToNetData(),
			choiceId = choiceId
		};
		_logger.Debug($"Sending player choice id {choiceId} for player {player.NetId}, result {result}");
		this.PlayerChoiceReceived?.Invoke(player, choiceId, message.result);
		_netService.SendMessage(message);
	}

	public async Task<PlayerChoiceResult> WaitForRemoteChoice(Player player, uint choiceId)
	{
		if (_netService.Type == NetGameType.Singleplayer)
		{
			throw new InvalidOperationException("Cannot wait for remote choice in singleplayer!");
		}
		if (!ValidateChoiceId(player, choiceId))
		{
			throw new InvalidOperationException($"Tried to wait for remote choice with ID {choiceId} for player {player.NetId}, but player's choice ID is {GetChoiceId(player)}!");
		}
		int num = _receivedChoices.FindIndex((ReceivedChoice c) => c.choiceId == choiceId && c.senderId == player.NetId);
		ReceivedChoice item;
		if (num >= 0)
		{
			_logger.Debug($"Was going to wait for remote choice {choiceId} for player {player.NetId} but we've already received it");
			item = _receivedChoices[num];
			_receivedChoices.RemoveAt(num);
		}
		else
		{
			item = new ReceivedChoice
			{
				choiceId = choiceId,
				senderId = player.NetId,
				completionSource = new TaskCompletionSource<NetPlayerChoiceResult>()
			};
			_logger.Debug($"Awaiting remote choice {choiceId} for player {player.NetId}");
			_receivedChoices.Add(item);
		}
		NetPlayerChoiceResult netData = await item.completionSource.Task;
		PlayerChoiceResult playerChoiceResult = PlayerChoiceResult.FromNetData(player, _players, netData);
		_logger.Debug($"Finished waiting for remote choice {choiceId} for player {player.NetId}: {playerChoiceResult}");
		return playerChoiceResult;
	}

	private void OnPlayerChoiceMessageReceived(PlayerChoiceMessage message, ulong senderId)
	{
		_logger.Debug($"Received choice from {senderId} for choice ID {message.choiceId}: {message.result}");
		Player player = _players.GetPlayer(senderId);
		OnReceivePlayerChoice(player, message.choiceId, message.result);
	}

	public void FastForwardChoiceIds(List<uint> choiceIds)
	{
		_logger.Debug("Fast-forwarded choice IDs to: " + string.Join(",", choiceIds));
		_choiceIds.Clear();
		_choiceIds.AddRange(choiceIds);
	}

	public void ReceiveReplayChoice(Player player, uint choiceId, NetPlayerChoiceResult result)
	{
		OnReceivePlayerChoice(player, choiceId, result);
	}

	private void OnReceivePlayerChoice(Player player, uint choiceId, NetPlayerChoiceResult result)
	{
		this.PlayerChoiceReceived?.Invoke(player, choiceId, result);
		int num = _receivedChoices.FindIndex((ReceivedChoice c) => c.choiceId == choiceId && c.senderId == player.NetId);
		ReceivedChoice item;
		if (num >= 0)
		{
			_logger.Debug("We are already waiting for the choice, fulfilling the task");
			item = _receivedChoices[num];
			_receivedChoices.RemoveAt(num);
		}
		else
		{
			item = new ReceivedChoice
			{
				choiceId = choiceId,
				senderId = player.NetId,
				completionSource = new TaskCompletionSource<NetPlayerChoiceResult>()
			};
			_logger.Debug("We are not yet waiting for the choice, creating a new received choice");
			_receivedChoices.Add(item);
		}
		item.completionSource.SetResult(result);
	}

	private bool ValidateChoiceId(Player player, uint choiceId)
	{
		return choiceId < GetChoiceId(player);
	}

	private uint GetChoiceId(Player player)
	{
		int playerSlotIndex = _players.GetPlayerSlotIndex(player);
		if (playerSlotIndex >= _choiceIds.Count)
		{
			return 0u;
		}
		return _choiceIds[playerSlotIndex];
	}
}
