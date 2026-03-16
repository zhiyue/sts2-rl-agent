using System;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game;

public struct RequestEnqueueActionMessage : INetMessage, IPacketSerializable, IRunLocationTargetedMessage
{
	public RunLocation location;

	public INetAction action;

	public bool ShouldBroadcast => false;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public RunLocation Location => location;

	public LogLevel LogLevel => LogLevel.VeryDebug;

	public void Serialize(PacketWriter writer)
	{
		writer.Write(location);
		writer.WriteByte((byte)action.ToId());
		writer.Write(action);
	}

	public void Deserialize(PacketReader reader)
	{
		location = reader.Read<RunLocation>();
		int num = reader.ReadByte();
		if (!ActionTypes.TryGetActionType(num, out Type type))
		{
			throw new InvalidOperationException($"Received net action of type {num} that does not map to any type!");
		}
		action = (INetAction)Activator.CreateInstance(type);
		action.Deserialize(reader);
	}

	public override string ToString()
	{
		return $"{"RequestEnqueueActionMessage"} location: {location} action: {action}";
	}
}
