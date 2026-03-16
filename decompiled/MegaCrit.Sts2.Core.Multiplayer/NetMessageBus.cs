using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Multiplayer;

public class NetMessageBus
{
	private delegate void AnonymizedMessageHandlerDelegate(INetMessage message, ulong senderId);

	private struct CallbackPair
	{
		public AnonymizedMessageHandlerDelegate handler;

		public object originalHandler;
	}

	private readonly PacketReader _reader = new PacketReader();

	private readonly PacketWriter _writer = new PacketWriter();

	private readonly Logger _logger = new Logger("NetMessageBus", LogType.Network);

	private readonly Dictionary<Type, List<CallbackPair>> _messageHandlers = new Dictionary<Type, List<CallbackPair>>();

	private readonly List<CallbackPair> _cachedPairList = new List<CallbackPair>();

	public byte[] SerializeMessage<T>(ulong senderId, T message, out int length) where T : INetMessage
	{
		_writer.Reset();
		_writer.WriteByte((byte)message.ToId());
		_writer.WriteULong(senderId);
		message.Serialize(_writer);
		length = (int)Math.Ceiling((float)_writer.BitPosition / 8f);
		return _writer.Buffer;
	}

	public bool TryDeserializeMessage(byte[] packetBytes, out INetMessage? message, out ulong? overrideSenderId)
	{
		overrideSenderId = null;
		message = null;
		_reader.Reset(packetBytes);
		byte b = _reader.ReadByte();
		if (!MessageTypes.TryGetMessageType(b, out Type type))
		{
			Log.Error($"Received message with first byte {b} that is not a valid message ID!");
			return false;
		}
		overrideSenderId = _reader.ReadULong();
		message = (INetMessage)Activator.CreateInstance(type);
		message.Deserialize(_reader);
		return true;
	}

	public void SendMessageToAllHandlers(INetMessage message, ulong senderId)
	{
		if (!_messageHandlers.TryGetValue(message.GetType(), out List<CallbackPair> value) || value.Count == 0)
		{
			Log.Error($"Received message of type {message.GetType()}, but no message handlers are registered for that type!");
			return;
		}
		_cachedPairList.Clear();
		_cachedPairList.AddRange(value);
		_logger.LogMessage(message.LogLevel, $"Received message {message}, sending to {_cachedPairList.Count} handlers", 0);
		foreach (CallbackPair cachedPair in _cachedPairList)
		{
			try
			{
				cachedPair.handler(message, senderId);
			}
			catch (Exception value2)
			{
				Log.Error($"Exception encountered while processing message {message}: {value2}");
			}
		}
	}

	public void RegisterMessageHandler<T>(MessageHandlerDelegate<T> handler) where T : INetMessage
	{
		if (typeof(T) == typeof(INetMessage))
		{
			throw new InvalidOperationException("RegisterMessageHandler must be called with a concrete implementation of INetMessage!");
		}
		if (!_messageHandlers.TryGetValue(typeof(T), out List<CallbackPair> value))
		{
			value = new List<CallbackPair>();
			_messageHandlers[typeof(T)] = value;
		}
		CallbackPair item = new CallbackPair
		{
			handler = delegate(INetMessage message, ulong senderId)
			{
				handler((T)message, senderId);
			},
			originalHandler = handler
		};
		value.Add(item);
	}

	public void UnregisterMessageHandler<T>(MessageHandlerDelegate<T> handler) where T : INetMessage
	{
		if (typeof(T) == typeof(INetMessage))
		{
			throw new InvalidOperationException("UnregisterMessageHandler must be called with a concrete implementation of INetMessage!");
		}
		if (_messageHandlers.TryGetValue(typeof(T), out List<CallbackPair> value))
		{
			value.RemoveAll((CallbackPair p) => (Delegate?)(MessageHandlerDelegate<T>)p.originalHandler == (Delegate?)handler);
		}
	}
}
