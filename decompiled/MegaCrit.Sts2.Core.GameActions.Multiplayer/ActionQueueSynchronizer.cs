using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.GameActions.Multiplayer;

public class ActionQueueSynchronizer
{
	private readonly ActionQueueSet _actionQueueSet;

	private readonly INetGameService _netService;

	private readonly Logger _logger;

	private readonly IPlayerCollection _playerCollection;

	private readonly RunLocationTargetedMessageBuffer _messageBuffer;

	private readonly List<GenericHookGameAction> _hookActions = new List<GenericHookGameAction>();

	private uint _nextHookId;

	private readonly List<GameAction> _requestedActionsWaitingForPlayerTurn = new List<GameAction>();

	public uint NextHookId => _nextHookId;

	public ActionSynchronizerCombatState CombatState { get; private set; }

	public ActionQueueSynchronizer(IPlayerCollection players, ActionQueueSet actionQueueSet, RunLocationTargetedMessageBuffer messageBuffer, INetGameService netService)
	{
		_playerCollection = players;
		_actionQueueSet = actionQueueSet;
		_netService = netService;
		_messageBuffer = messageBuffer;
		_messageBuffer.RegisterMessageHandler<RequestEnqueueActionMessage>(HandleRequestEnqueueActionMessage);
		_messageBuffer.RegisterMessageHandler<ActionEnqueuedMessage>(HandleActionEnqueuedMessage);
		_messageBuffer.RegisterMessageHandler<RequestEnqueueHookActionMessage>(HandleRequestEnqueueHookActionMessage);
		_messageBuffer.RegisterMessageHandler<HookActionEnqueuedMessage>(HandleHookActionEnqueuedMessage);
		_messageBuffer.RegisterMessageHandler<RequestResumeActionAfterPlayerChoiceMessage>(HandleRequestResumeActionAfterPlayerChoiceMessage);
		_messageBuffer.RegisterMessageHandler<ResumeActionAfterPlayerChoiceMessage>(HandleResumeActionAfterPlayerChoiceMessage);
		_logger = new Logger("ActionQueueSynchronizer", LogType.Actions);
	}

	public void Dispose()
	{
		_messageBuffer.UnregisterMessageHandler<RequestEnqueueActionMessage>(HandleRequestEnqueueActionMessage);
		_messageBuffer.UnregisterMessageHandler<ActionEnqueuedMessage>(HandleActionEnqueuedMessage);
		_messageBuffer.UnregisterMessageHandler<RequestEnqueueHookActionMessage>(HandleRequestEnqueueHookActionMessage);
		_messageBuffer.UnregisterMessageHandler<HookActionEnqueuedMessage>(HandleHookActionEnqueuedMessage);
		_messageBuffer.UnregisterMessageHandler<RequestResumeActionAfterPlayerChoiceMessage>(HandleRequestResumeActionAfterPlayerChoiceMessage);
		_messageBuffer.UnregisterMessageHandler<ResumeActionAfterPlayerChoiceMessage>(HandleResumeActionAfterPlayerChoiceMessage);
	}

	public void SetCombatState(ActionSynchronizerCombatState combatState)
	{
		if (CombatState == combatState)
		{
			return;
		}
		ActionSynchronizerCombatState combatState2 = CombatState;
		CombatState = combatState;
		if (combatState2 == ActionSynchronizerCombatState.NotInCombat)
		{
			_logger.Debug($"Combat state was previously {combatState2}. Signaling queues that combat has begun");
			_actionQueueSet.CombatStarted();
		}
		switch (combatState)
		{
		case ActionSynchronizerCombatState.NotInCombat:
			_logger.Debug($"Combat state becomes {combatState}. Cancelling deferred actions and cancelling all remaining combat actions");
			_actionQueueSet.CombatEnded();
			_actionQueueSet.UnpauseAllPlayerQueues();
			foreach (GameAction item in _requestedActionsWaitingForPlayerTurn)
			{
				item.Cancel();
			}
			_requestedActionsWaitingForPlayerTurn.Clear();
			break;
		case ActionSynchronizerCombatState.PlayPhase:
			_logger.Debug($"Combat state becomes {combatState}. Requesting {_requestedActionsWaitingForPlayerTurn.Count} actions and unpausing action queues");
			foreach (GameAction item2 in _requestedActionsWaitingForPlayerTurn)
			{
				RequestEnqueue(item2);
			}
			_requestedActionsWaitingForPlayerTurn.Clear();
			_actionQueueSet.UnpauseAllPlayerQueues();
			break;
		case ActionSynchronizerCombatState.EndTurnPhaseOne:
			_logger.Debug($"Combat state becomes {combatState} (from {combatState2}). Starting to cancel all player-driven actions");
			_actionQueueSet.StartCancellingAllPlayerDrivenCombatActions();
			break;
		case ActionSynchronizerCombatState.NotPlayPhase:
			_logger.Debug($"Combat state becomes {combatState}. Pausing all player queues");
			_actionQueueSet.PauseAllPlayerQueues();
			break;
		}
	}

	public void RequestEnqueue(GameAction action)
	{
		if (action.ActionType == GameActionType.CombatPlayPhaseOnly && CombatState == ActionSynchronizerCombatState.NotPlayPhase)
		{
			_logger.Debug($"Attempted to request enqueue of action {action} during the enemy turn. Deferring the action until the player turn");
			_requestedActionsWaitingForPlayerTurn.Add(action);
			return;
		}
		switch (_netService.Type)
		{
		case NetGameType.Client:
		{
			_logger.Debug($"Sending message to host requesting to enqueue action {action} at {Log.Timestamp}");
			RequestEnqueueActionMessage message = new RequestEnqueueActionMessage
			{
				action = action.ToNetAction(),
				location = _messageBuffer.CurrentLocation
			};
			_netService.SendMessage(message);
			break;
		}
		case NetGameType.Singleplayer:
		case NetGameType.Host:
			EnqueueAction(action, _netService.NetId);
			break;
		}
	}

	public GenericHookGameAction GenerateHookAction(ulong ownerId, GameActionType gameActionType)
	{
		GenericHookGameAction hookActionForId = GetHookActionForId(_nextHookId, ownerId, gameActionType);
		_nextHookId++;
		return hookActionForId;
	}

	public void RequestEnqueueHookAction(GenericHookGameAction action)
	{
		switch (_netService.Type)
		{
		case NetGameType.Client:
		{
			_logger.Debug($"Sending message to host requesting to enqueue HOOK action with id {action.HookId} at {Log.Timestamp}");
			RequestEnqueueHookActionMessage message = new RequestEnqueueHookActionMessage
			{
				hookActionId = action.HookId,
				location = _messageBuffer.CurrentLocation,
				gameActionType = action.ActionType
			};
			_netService.SendMessage(message);
			break;
		}
		case NetGameType.Singleplayer:
		case NetGameType.Host:
			EnqueueHookAction(action);
			break;
		}
	}

	public void RequestResumeActionAfterPlayerChoice(GameAction action)
	{
		switch (_netService.Type)
		{
		case NetGameType.Client:
		{
			_logger.Debug($"Sending message to host requesting resumption of action {action}, id {action.Id}, at {Log.Timestamp}");
			RequestResumeActionAfterPlayerChoiceMessage message = new RequestResumeActionAfterPlayerChoiceMessage
			{
				actionId = action.Id.Value,
				location = _messageBuffer.CurrentLocation
			};
			_netService.SendMessage(message);
			break;
		}
		case NetGameType.Singleplayer:
		case NetGameType.Host:
			ResumeActionAfterPlayerChoice(action.Id.Value);
			break;
		}
	}

	private void EnqueueAction(GameAction action, ulong actionOwnerId)
	{
		if (_netService.Type == NetGameType.Host)
		{
			_logger.Debug($"Sending message to client to enqueue action {action} at {Log.Timestamp}");
			ActionEnqueuedMessage message = new ActionEnqueuedMessage
			{
				playerId = actionOwnerId,
				location = _messageBuffer.CurrentLocation,
				action = action.ToNetAction()
			};
			_netService.SendMessage(message);
		}
		_logger.Debug($"Enqueueing action {action} from owner {actionOwnerId}");
		_actionQueueSet.EnqueueWithoutSynchronizing(action);
	}

	private void EnqueueHookAction(GenericHookGameAction gameAction)
	{
		if (_netService.Type == NetGameType.Host)
		{
			_logger.Debug($"Sending message to client to enqueue hook action {gameAction} at {Log.Timestamp}");
			HookActionEnqueuedMessage message = new HookActionEnqueuedMessage
			{
				hookActionId = gameAction.HookId,
				ownerId = gameAction.OwnerId,
				location = _messageBuffer.CurrentLocation,
				gameActionType = gameAction.ActionType
			};
			_netService.SendMessage(message);
		}
		_logger.Debug($"Enqueueing HOOK action with id {gameAction.HookId}");
		_actionQueueSet.EnqueueWithoutSynchronizing(gameAction);
	}

	private void ResumeActionAfterPlayerChoice(uint id)
	{
		if (_netService.Type == NetGameType.Host)
		{
			_logger.Debug($"Sending message to client to resume action id {id} at {Log.Timestamp}");
			ResumeActionAfterPlayerChoiceMessage message = new ResumeActionAfterPlayerChoiceMessage
			{
				actionId = id,
				location = _messageBuffer.CurrentLocation
			};
			_netService.SendMessage(message);
		}
		_logger.Debug($"Resuming action with ID {id}");
		_actionQueueSet.ResumeActionWithoutSynchronizing(id);
	}

	private void HandleRequestEnqueueActionMessage(RequestEnqueueActionMessage message, ulong senderId)
	{
		if (_netService.Type != NetGameType.Host)
		{
			throw new InvalidOperationException("Received action enqueue request while not host!");
		}
		_logger.Debug($"Received request enqueue action for action {message.action} ({senderId}) at {Log.Timestamp}");
		GameAction action = NetActionToGameAction(message.action, senderId);
		EnqueueAction(action, senderId);
	}

	private void HandleActionEnqueuedMessage(ActionEnqueuedMessage message, ulong _)
	{
		if (_netService.Type != NetGameType.Client)
		{
			throw new InvalidOperationException("Received action enqueued message while not client!");
		}
		_logger.Debug($"Received handle action enqueued message {message} at {Log.Timestamp}");
		GameAction action = NetActionToGameAction(message.action, message.playerId);
		EnqueueAction(action, message.playerId);
	}

	private void HandleRequestEnqueueHookActionMessage(RequestEnqueueHookActionMessage message, ulong senderId)
	{
		if (_netService.Type != NetGameType.Host)
		{
			throw new InvalidOperationException("Received hook action enqueue request while not host!");
		}
		_logger.Debug($"Received HOOK request enqueue action from {senderId}, hook id: {message.hookActionId}, at {Log.Timestamp}");
		GenericHookGameAction hookActionForId = GetHookActionForId(message.hookActionId, senderId, message.gameActionType);
		EnqueueHookAction(hookActionForId);
	}

	private void HandleHookActionEnqueuedMessage(HookActionEnqueuedMessage message, ulong _)
	{
		if (_netService.Type != NetGameType.Client)
		{
			throw new InvalidOperationException("Received hook action enqueued message while not host!");
		}
		_logger.Debug($"Received HOOK request enqueue action, hook id: {message.hookActionId}, at {Log.Timestamp}");
		GenericHookGameAction hookActionForId = GetHookActionForId(message.hookActionId, message.ownerId, message.gameActionType);
		EnqueueHookAction(hookActionForId);
	}

	private void HandleRequestResumeActionAfterPlayerChoiceMessage(RequestResumeActionAfterPlayerChoiceMessage afterPlayerChoiceMessage, ulong senderId)
	{
		if (_netService.Type != NetGameType.Host)
		{
			throw new InvalidOperationException("Received action ready request message while not host!");
		}
		_logger.Debug($"Received action ready request message for action {afterPlayerChoiceMessage.actionId}, sender {senderId}, at {Log.Timestamp}");
		ResumeActionAfterPlayerChoice(afterPlayerChoiceMessage.actionId);
	}

	private void HandleResumeActionAfterPlayerChoiceMessage(ResumeActionAfterPlayerChoiceMessage afterPlayerChoiceMessage, ulong _)
	{
		if (_netService.Type != NetGameType.Client)
		{
			throw new InvalidOperationException("Received action ready message while not client!");
		}
		_logger.Debug($"Received action ready message for action {afterPlayerChoiceMessage.actionId} at {Log.Timestamp}");
		ResumeActionAfterPlayerChoice(afterPlayerChoiceMessage.actionId);
	}

	private GameAction NetActionToGameAction(INetAction action, ulong actionOwnerId)
	{
		Player player = _playerCollection.GetPlayer(actionOwnerId);
		if (player == null)
		{
			throw new InvalidOperationException($"Action owner ID {actionOwnerId} for action {action} could not be mapped to a Player!");
		}
		return action.ToGameAction(player);
	}

	public GenericHookGameAction GetHookActionForId(uint id, ulong ownerId, GameActionType gameActionType)
	{
		GenericHookGameAction action = _hookActions.Find((GenericHookGameAction a) => a.HookId == id);
		if (action == null)
		{
			action = new GenericHookGameAction(id, ownerId, gameActionType);
			action.ExecutionStartedTask.ContinueWith(delegate
			{
				HookActionStarted(action);
			});
			_hookActions.Add(action);
		}
		else
		{
			if (action.OwnerId != ownerId)
			{
				throw new InvalidOperationException($"Attempted to get hook for owner {ownerId} with hook ID {id}, but the hook already existed and had owner {action.OwnerId}!");
			}
			if (action.ActionType != gameActionType)
			{
				throw new InvalidOperationException($"Generating GenericHookGameAction with type {gameActionType}. Found one already enqueued, but with a mismatching game action type {action.ActionType}!");
			}
		}
		return action;
	}

	private void HookActionStarted(GenericHookGameAction action)
	{
		_hookActions.Remove(action);
	}

	public void FastForwardHookId(uint hookId)
	{
		_nextHookId = hookId;
	}
}
