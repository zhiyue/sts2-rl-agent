using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Sync;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;

namespace MegaCrit.Sts2.Core.Multiplayer.Game;

public class EventSynchronizer : IDisposable
{
	private readonly INetGameService _netService;

	private readonly RunLocationTargetedMessageBuffer _messageBuffer;

	private readonly IPlayerCollection _playerCollection;

	private readonly ulong _localPlayerId;

	private readonly List<EventModel> _events = new List<EventModel>();

	private EventModel? _canonicalEvent;

	private readonly List<uint?> _playerVotes = new List<uint?>();

	private uint _pageIndex;

	private readonly List<Task> _pendingOptionTasks = new List<Task>();

	private readonly Rng _multiplayerOptionSelectionRng;

	private readonly Logger _logger = new Logger("EventSynchronizer", LogType.GameSync);

	public IReadOnlyList<EventModel> Events => _events;

	public bool IsShared => (_canonicalEvent ?? throw new InvalidOperationException("Event is not in progress!")).IsShared;

	private Player LocalPlayer => _playerCollection.GetPlayer(_localPlayerId);

	public event Action<Player>? PlayerVoteChanged;

	public EventSynchronizer(RunLocationTargetedMessageBuffer messageBuffer, INetGameService netService, IPlayerCollection playerCollection, ulong localPlayerId, uint seed)
	{
		_netService = netService;
		_messageBuffer = messageBuffer;
		_playerCollection = playerCollection;
		_localPlayerId = localPlayerId;
		_multiplayerOptionSelectionRng = new Rng(seed, "event_synchronizer");
		_messageBuffer.RegisterMessageHandler<OptionIndexChosenMessage>(HandleEventOptionChosenMessage);
		_messageBuffer.RegisterMessageHandler<VotedForSharedEventOptionMessage>(HandleVotedForSharedEventOptionMessage);
		_messageBuffer.RegisterMessageHandler<SharedEventOptionChosenMessage>(HandleSharedEventOptionChosenMessage);
	}

	public void Dispose()
	{
		_messageBuffer.UnregisterMessageHandler<OptionIndexChosenMessage>(HandleEventOptionChosenMessage);
		_messageBuffer.UnregisterMessageHandler<VotedForSharedEventOptionMessage>(HandleVotedForSharedEventOptionMessage);
		_messageBuffer.UnregisterMessageHandler<SharedEventOptionChosenMessage>(HandleSharedEventOptionChosenMessage);
	}

	/// <summary>
	/// Called when an event room is entered.
	/// </summary>
	/// <param name="canonicalEvent">The canonical version of the event that will be started.</param>
	/// <param name="isPrefinished">If the event is already pre-finished</param>
	/// <param name="debugOnStart">A method to call to inject some debug code right after the model is cloned. The mutable
	/// model instances will be passed to the method.</param>
	public void BeginEvent(EventModel canonicalEvent, bool isPrefinished = false, Action<EventModel>? debugOnStart = null)
	{
		_logger.Debug($"Beginning event {canonicalEvent.Id}, shared: {canonicalEvent.IsShared}");
		for (int i = _playerVotes.Count; i < _playerCollection.Players.Count; i++)
		{
			_playerVotes.Add(null);
		}
		foreach (EventModel @event in _events)
		{
			if (!@event.IsFinished)
			{
				_logger.Warn($"Beginning new event {canonicalEvent}, but event {@event} for player {@event.Owner.NetId} is not yet finished!");
				@event.EnsureCleanup();
			}
		}
		_events.Clear();
		_pendingOptionTasks.Clear();
		ClearPlayerVotes();
		_pageIndex = 0u;
		_canonicalEvent = canonicalEvent;
		foreach (Player player in _playerCollection.Players)
		{
			EventModel eventModel = canonicalEvent.ToMutable();
			debugOnStart?.Invoke(eventModel);
			_events.Add(eventModel);
			TaskHelper.RunSafely(eventModel.BeginEvent(player, isPrefinished));
			_logger.VeryDebug($"Event {eventModel.Id} began for player {player.NetId} with options: {string.Join(",", eventModel.CurrentOptions)}");
		}
	}

	/// <summary>Received during shared events. Records a player's vote for a given event option. </summary>
	private void HandleVotedForSharedEventOptionMessage(VotedForSharedEventOptionMessage message, ulong senderId)
	{
		_logger.Debug($"Received {"VotedForSharedEventOptionMessage"} from player {senderId} for option {message.optionIndex} on page {message.pageIndex}");
		if (!IsShared)
		{
			throw new InvalidOperationException("Received VotedForSharedEventOptionMessage during a non-shared event!");
		}
		Player player = _playerCollection.GetPlayer(senderId);
		if (player == null)
		{
			throw new InvalidOperationException($"Received {"VotedForSharedEventOptionMessage"} for player {senderId} that doesn't exist!");
		}
		PlayerVotedForSharedOptionIndex(player, message.optionIndex, message.pageIndex);
	}

	/// <summary> Records a player's vote. Called in response to a remote player's message, or immediately when the
	/// local player votes for a shared event option. </summary>
	private void PlayerVotedForSharedOptionIndex(Player player, uint optionIndex, uint pageIndex)
	{
		if (pageIndex < _pageIndex)
		{
			_logger.Warn($"Received message from player {player.NetId} voting for option {optionIndex} on page {pageIndex}, but we are on greater page {_pageIndex}. Ignoring vote");
		}
		else if (pageIndex > _pageIndex)
		{
			_logger.Error($"Received message from player {player.NetId} voting for option {optionIndex} on page {pageIndex}, but we are on lesser page {_pageIndex}. This is a bug!");
		}
		else
		{
			int playerSlotIndex = _playerCollection.GetPlayerSlotIndex(player);
			_playerVotes[playerSlotIndex] = optionIndex;
			this.PlayerVoteChanged?.Invoke(player);
			if (_playerVotes.All((uint? p) => p.HasValue) && _netService.Type != NetGameType.Client)
			{
				_logger.Debug("All votes received and we are host. Choosing shared event option");
				ChooseSharedEventOption();
			}
		}
	}

	/// <summary> Called on the host when all votes for a shared event page are received. Decides which event option
	/// should be chosen and notifies all clients of the option chosen. </summary>
	private void ChooseSharedEventOption()
	{
		if (_netService.Type == NetGameType.Client)
		{
			throw new InvalidOperationException("Only host should be choosing shared event option!");
		}
		_logger.Debug($"All votes received on host for shared event {_canonicalEvent}, choosing option");
		uint value = _multiplayerOptionSelectionRng.NextItem(_playerVotes).Value;
		_logger.Debug($"Shared event option {value} was chosen");
		SharedEventOptionChosenMessage message = new SharedEventOptionChosenMessage
		{
			optionIndex = value,
			pageIndex = _pageIndex,
			location = _messageBuffer.CurrentLocation
		};
		_netService.SendMessage(message);
		ChooseOptionForSharedEvent(value);
	}

	/// <summary> Called on clients when the host receives all player votes and decides on an event option to choose. </summary>
	private void HandleSharedEventOptionChosenMessage(SharedEventOptionChosenMessage message, ulong senderId)
	{
		_logger.Debug($"Received {"SharedEventOptionChosenMessage"} for option {message.optionIndex} on page {message.pageIndex}");
		if (_netService.Type != NetGameType.Client)
		{
			throw new InvalidOperationException($"Received {"SharedEventOptionChosenMessage"} on non-client! {_netService.Type}");
		}
		if (_pageIndex != message.pageIndex)
		{
			throw new InvalidOperationException($"Received {"SharedEventOptionChosenMessage"} for page {message.pageIndex} while we were on page {_pageIndex}!");
		}
		ChooseOptionForSharedEvent(message.optionIndex);
	}

	/// <summary> Called on all peers during non-shared events when one peer selects an event option. </summary>
	private void HandleEventOptionChosenMessage(OptionIndexChosenMessage message, ulong senderId)
	{
		if (message.type == OptionIndexType.Event)
		{
			_logger.Debug($"Received {"OptionIndexChosenMessage"} from player {senderId} for event option index {message.optionIndex}");
			if (IsShared)
			{
				throw new InvalidOperationException("Received OptionIndexChosenMessage during a shared event!");
			}
			Player player = _playerCollection.GetPlayer(senderId);
			if (player == null)
			{
				throw new InvalidOperationException($"Received EventOptionChosenMessage for player {senderId} that doesn't exist!");
			}
			ChooseOptionForEvent(player, (int)message.optionIndex);
		}
	}

	/// <summary>
	/// Called when the local player selects an event option button.
	/// During shared events, this votes for the event option.
	/// During non-shared events, this immediately executes the option and sends the event option to all other peers.
	/// </summary>
	/// <param name="index">The index of the event option that was chosen.</param>
	public void ChooseLocalOption(int index)
	{
		if (IsShared)
		{
			_logger.Debug($"Local player voted for shared event option index {index}");
			PlayerVotedForSharedOptionIndex(LocalPlayer, (uint)index, _pageIndex);
			VotedForSharedEventOptionMessage message = new VotedForSharedEventOptionMessage
			{
				optionIndex = (uint)index,
				pageIndex = _pageIndex,
				location = _messageBuffer.CurrentLocation
			};
			_netService.SendMessage(message);
		}
		else
		{
			_logger.Debug($"Local player chose event option index {index}");
			ChooseOptionForEvent(LocalPlayer, index);
			OptionIndexChosenMessage message2 = new OptionIndexChosenMessage
			{
				type = OptionIndexType.Event,
				optionIndex = (uint)index,
				location = _messageBuffer.CurrentLocation
			};
			_netService.SendMessage(message2);
		}
	}

	/// <summary> Executes the shared event option on all players' event model instances and clears player votes. </summary>
	private void ChooseOptionForSharedEvent(uint optionIndex)
	{
		if (!IsShared)
		{
			throw new InvalidOperationException("ChooseOptionForSharedEvent called during non-shared event!");
		}
		_logger.Debug($"Choosing option index {optionIndex} for shared event on page {_pageIndex}");
		ClearPlayerVotes();
		_pageIndex++;
		foreach (Player player in _playerCollection.Players)
		{
			ChooseOptionForEvent(player, (int)optionIndex);
		}
	}

	/// <summary>
	/// Executes the given event option for a given player.
	/// </summary>
	private void ChooseOptionForEvent(Player player, int optionIndex)
	{
		EventModel eventForPlayer = GetEventForPlayer(player);
		if (eventForPlayer.IsFinished)
		{
			throw new InvalidOperationException($"Option chosen for player {player} on {eventForPlayer}, but it is already finished!");
		}
		if (optionIndex >= eventForPlayer.CurrentOptions.Count)
		{
			throw new InvalidOperationException($"Player {player.NetId} attempted to choose option index {optionIndex} in event {eventForPlayer.Id}, but there were only {eventForPlayer.CurrentOptions.Count} options available!");
		}
		_logger.Debug($"Option index {optionIndex} chosen for player {player.NetId} in event {eventForPlayer.Id}. Choice key: {eventForPlayer.CurrentOptions[optionIndex].TextKey}");
		EventOption eventOption = eventForPlayer.CurrentOptions[optionIndex];
		_pendingOptionTasks.Add(TaskHelper.RunSafely(eventOption.Chosen()));
		SaveEventOptionToHistory(player, eventOption);
	}

	private void SaveEventOptionToHistory(Player player, EventOption option)
	{
		if (!option.ShouldSaveChoiceToHistory)
		{
			return;
		}
		EventOptionHistoryEntry item = new EventOptionHistoryEntry
		{
			Title = option.HistoryName,
			Variables = new Dictionary<string, object>()
		};
		if (option.ShouldSaveVariablesToHistory)
		{
			foreach (KeyValuePair<string, object> variable in option.HistoryName.Variables)
			{
				item.Variables[variable.Key] = variable.Value;
			}
		}
		player.RunState.CurrentMapPointHistoryEntry?.GetEntry(player.NetId).EventChoices.Add(item);
	}

	private void ClearPlayerVotes()
	{
		for (int i = 0; i < _playerVotes.Count; i++)
		{
			_playerVotes[i] = null;
		}
	}

	public uint? GetPlayerVote(Player player)
	{
		return _playerVotes[_playerCollection.GetPlayerSlotIndex(player)];
	}

	public EventModel GetLocalEvent()
	{
		return GetEventForPlayer(LocalPlayer);
	}

	public EventModel GetEventForPlayer(Player player)
	{
		int playerSlotIndex = _playerCollection.GetPlayerSlotIndex(player);
		return _events[playerSlotIndex];
	}

	public void ResumeEvents(AbstractRoom exitedRoom)
	{
		foreach (EventModel @event in _events)
		{
			TaskHelper.RunSafely(@event.Resume(exitedRoom));
		}
	}

	/// <summary>
	/// Awaits all in-flight event option tasks. Called before the room exit checksum to ensure that all
	/// players' event handlers have finished mutating state. Without this, a peer whose local event
	/// finished quickly (e.g. Touch a Mirror) can exit the room while a remote player's slow handler
	/// (e.g. Shatter's per-card deck duplication loop) is still running, causing the checksum to
	/// capture incomplete state.
	/// </summary>
	public async Task AwaitPendingOptionTasks()
	{
		try
		{
			await Task.WhenAll(_pendingOptionTasks);
		}
		catch (Exception)
		{
		}
		_pendingOptionTasks.Clear();
	}
}
