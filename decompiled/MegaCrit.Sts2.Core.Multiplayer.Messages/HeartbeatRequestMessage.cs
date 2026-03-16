using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages;

public record struct HeartbeatRequestMessage : INetMessage, IPacketSerializable
{
	public bool ShouldBroadcast => false;

	public NetTransferMode Mode => NetTransferMode.Unreliable;

	public LogLevel LogLevel => LogLevel.VeryDebug;

	public int counter;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteInt(counter);
	}

	public void Deserialize(PacketReader reader)
	{
		counter = reader.ReadInt();
	}
}
