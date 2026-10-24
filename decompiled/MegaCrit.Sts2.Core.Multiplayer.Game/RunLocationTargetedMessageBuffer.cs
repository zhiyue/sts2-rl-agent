using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Game;

/// <summary>
/// Responsible for holding messages that are not destined for the current map point, and waiting until that map point
/// is entered before processing those messages.
/// We may be slow to transition to a new map point (e.g. because of loading delays), and messages from other peers may
/// come in before we're ready to receive them.
/// Note that it is important that queued messages are sent out in the order that they were received.
/// </summary>
public class RunLocationTargetedMessageBuffer
{
	private delegate void AnonymizedMessageHandlerDelegate(INetMessage message, ulong senderId);

	private struct TypeAndMessageHandlers
	{
		public Type messageType;

		public object netServiceHandler;

		public List<MessageHandler> handlers;
	}

	private struct MessageHandler
	{
		public object originalHandler;

		public AnonymizedMessageHandlerDelegate anonymizedHandler;
	}

	private struct BlockedMessage
	{
		public RunLocation location;

		public INetMessage message;

		public ulong senderId;

		public Type messageType;
	}

	private readonly INetGameService _gameService;

	private readonly List<BlockedMessage> _messagesWaitingOnLocationChange = new List<BlockedMessage>();

	private readonly List<TypeAndMessageHandlers> _messageHandlers = new List<TypeAndMessageHandlers>();

	private readonly HashSet<RunLocation> _visitedLocations = new HashSet<RunLocation>();

	private readonly Logger _logger = new Logger("RunLocationTargetedMessageBuffer", LogType.GameSync);

	public RunLocation CurrentLocation { get; private set; }

	public RunLocationTargetedMessageBuffer(INetGameService gameService)
	{
		_gameService = gameService;
		_visitedLocations.Add(CurrentLocation);
	}

	/// <summary>
	/// This should be called whenever the run location changes. If messages are blocked waiting for a location change,
	/// then they will be sent to registered message handlers in the order that they were received.
	/// </summary>
	public void OnLocationChanged(RunLocation location)
	{
		_logger.Debug($"Run location changed to {location} (previously at: {CurrentLocation}), checking if we have enqueued messages");
		CurrentLocation = location;
		_visitedLocations.Add(CurrentLocation);
		for (int i = 0; i < _messagesWaitingOnLocationChange.Count; i++)
		{
			BlockedMessage blockedMessage = _messagesWaitingOnLocationChange[i];
			if (_visitedLocations.Contains(blockedMessage.location))
			{
				_logger.Debug($"Handling enqueued message {blockedMessage.message} of type {blockedMessage.messageType} from {blockedMessage.senderId}");
				CallHandlersOfType(blockedMessage.messageType, blockedMessage.message, blockedMessage.senderId);
				_messagesWaitingOnLocationChange.RemoveAt(i);
				i--;
			}
		}
		if (_messagesWaitingOnLocationChange.Count > 0)
		{
			_logger.Error($"After transitioning to {location}, there are still {_messagesWaitingOnLocationChange.Count} messages for other locations. This is likely indicates a bug. Messages:\n{string.Join("\n", _messagesWaitingOnLocationChange)}");
		}
	}

	private void CallHandlersOfType(Type type, INetMessage message, ulong senderId)
	{
		foreach (TypeAndMessageHandlers messageHandler in _messageHandlers)
		{
			if (!(messageHandler.messageType == type))
			{
				continue;
			}
			foreach (MessageHandler handler in messageHandler.handlers)
			{
				handler.anonymizedHandler(message, senderId);
			}
		}
	}

	/// <summary>
	/// Registers a message handler for map targeted messages.
	/// <see cref="T:MegaCrit.Sts2.Core.Multiplayer.Messages.Game.IRunLocationTargetedMessage" />s should be registered here instead of directly to the INetGameHandler; otherwise,
	/// they may be received at the wrong time.
	/// </summary>
	/// <param name="handler">
	/// The delegate to call when the message type is received or when we enter a new location and a message for that
	/// location is enqueued.
	/// </param>
	public void RegisterMessageHandler<T>(MessageHandlerDelegate<T> handler) where T : INetMessage, IRunLocationTargetedMessage
	{
		_logger.VeryDebug($"Register message handler {handler} for {typeof(T)}");
		TypeAndMessageHandlers? typeAndMessageHandlers = null;
		foreach (TypeAndMessageHandlers messageHandler in _messageHandlers)
		{
			if (messageHandler.messageType == typeof(T))
			{
				typeAndMessageHandlers = messageHandler;
			}
		}
		if (!typeAndMessageHandlers.HasValue)
		{
			MessageHandlerDelegate<T> messageHandlerDelegate = HandleMessage;
			typeAndMessageHandlers = new TypeAndMessageHandlers
			{
				messageType = typeof(T),
				netServiceHandler = messageHandlerDelegate,
				handlers = new List<MessageHandler>()
			};
			_gameService.RegisterMessageHandler(messageHandlerDelegate);
			_messageHandlers.Add(typeAndMessageHandlers.Value);
		}
		typeAndMessageHandlers.Value.handlers.Add(new MessageHandler
		{
			anonymizedHandler = AnonymousDelegate,
			originalHandler = handler
		});
		void AnonymousDelegate(INetMessage message, ulong senderId)
		{
			handler((T)message, senderId);
		}
	}

	/// <summary>
	/// Unregisters a message handler for map targeted messages.
	/// </summary>
	public void UnregisterMessageHandler<T>(MessageHandlerDelegate<T> handler) where T : INetMessage, IRunLocationTargetedMessage
	{
		for (int i = 0; i < _messageHandlers.Count; i++)
		{
			TypeAndMessageHandlers typeAndMessageHandlers = _messageHandlers[i];
			if (typeAndMessageHandlers.messageType != typeof(T))
			{
				continue;
			}
			for (int j = 0; j < typeAndMessageHandlers.handlers.Count; j++)
			{
				if (typeAndMessageHandlers.handlers[j].originalHandler is MessageHandlerDelegate<T> messageHandlerDelegate && (Delegate?)messageHandlerDelegate == (Delegate?)handler)
				{
					typeAndMessageHandlers.handlers.RemoveAt(j);
					j--;
				}
			}
			if (typeAndMessageHandlers.handlers.Count <= 0)
			{
				_gameService.UnregisterMessageHandler((MessageHandlerDelegate<T>)typeAndMessageHandlers.netServiceHandler);
				_messageHandlers.RemoveAt(i);
				i--;
			}
		}
	}

	/// <summary>
	/// Internal method which either sends the message to the handler, or enqueues it if the message is not destined
	/// for a location we have already visited.
	/// </summary>
	private void HandleMessage<T>(T message, ulong senderId) where T : INetMessage, IRunLocationTargetedMessage
	{
		_logger.VeryDebug($"Handling map-targeted message {message} from {senderId} for location {message.Location}");
		if (_visitedLocations.Contains(message.Location))
		{
			CallHandlersOfType(typeof(T), message, senderId);
			return;
		}
		_logger.Debug($"Message {message} from {senderId} is for location {message.Location}, enqueueing it because we are currently at location {CurrentLocation}");
		BlockedMessage item = new BlockedMessage
		{
			location = message.Location,
			message = message,
			messageType = message.GetType(),
			senderId = senderId
		};
		_messagesWaitingOnLocationChange.Add(item);
	}
}
