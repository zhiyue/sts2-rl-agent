using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Lobby;

public struct LobbySeedChangedMessage : INetMessage, IPacketSerializable
{
	public string? seed;

	public bool ShouldBroadcast => true;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.VeryDebug;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteBool(seed != null);
		if (seed != null)
		{
			writer.WriteString(seed);
		}
	}

	public void Deserialize(PacketReader reader)
	{
		if (reader.ReadBool())
		{
			seed = reader.ReadString();
		}
	}
}
