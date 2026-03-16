using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Lobby;

public struct PlayerLeftMessage : INetMessage, IPacketSerializable
{
	public ulong playerId;

	public bool ShouldBroadcast => false;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.VeryDebug;

	public PlayerLeftMessage(ulong playerId)
	{
		this.playerId = playerId;
	}

	public void Serialize(PacketWriter writer)
	{
		writer.WriteULong(playerId);
	}

	public void Deserialize(PacketReader reader)
	{
		playerId = reader.ReadULong();
	}
}
