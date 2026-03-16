using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Lobby;

public struct LobbyAscensionChangedMessage : INetMessage, IPacketSerializable
{
	public int ascension;

	public bool ShouldBroadcast => true;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.VeryDebug;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteInt(ascension);
	}

	public void Deserialize(PacketReader reader)
	{
		ascension = reader.ReadInt();
	}
}
