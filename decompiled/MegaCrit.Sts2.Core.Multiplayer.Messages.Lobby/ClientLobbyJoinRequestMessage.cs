using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Lobby;

public struct ClientLobbyJoinRequestMessage : INetMessage, IPacketSerializable
{
	public int maxAscensionUnlocked;

	public SerializableUnlockState unlockState;

	public bool ShouldBroadcast => false;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.Info;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteInt(maxAscensionUnlocked);
		writer.Write(unlockState);
	}

	public void Deserialize(PacketReader reader)
	{
		maxAscensionUnlocked = reader.ReadInt();
		unlockState = reader.Read<SerializableUnlockState>();
	}
}
