using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game;

public struct SyncPlayerDataMessage : INetMessage, IPacketSerializable
{
	public SerializablePlayer player;

	public bool ShouldBroadcast => true;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.VeryDebug;

	public void Serialize(PacketWriter writer)
	{
		writer.Write(player);
	}

	public void Deserialize(PacketReader reader)
	{
		player = reader.Read<SerializablePlayer>();
	}
}
