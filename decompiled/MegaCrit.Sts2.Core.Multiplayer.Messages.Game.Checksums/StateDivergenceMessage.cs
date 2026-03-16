using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Checksums;

public struct StateDivergenceMessage : INetMessage, IPacketSerializable
{
	public NetChecksumData senderChecksum;

	public NetFullCombatState senderCombatState;

	public bool ShouldBroadcast => false;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.Info;

	public void Serialize(PacketWriter writer)
	{
		writer.Write(senderChecksum);
		writer.Write(senderCombatState);
	}

	public void Deserialize(PacketReader reader)
	{
		senderChecksum = reader.Read<NetChecksumData>();
		senderCombatState = reader.Read<NetFullCombatState>();
	}
}
