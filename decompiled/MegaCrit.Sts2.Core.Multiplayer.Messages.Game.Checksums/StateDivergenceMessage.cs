using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Checksums;

/// <summary>
/// Sent when a state divergence is detected due to the host and client checksums mismatching.
/// First this is sent from host to client, then it is sent from client to host.
/// </summary>
public struct StateDivergenceMessage : INetMessage, IPacketSerializable
{
	public NetChecksumData senderChecksum;

	public NetFullCombatState senderCombatState;

	public bool ShouldBroadcast => false;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.Info;

	public bool ShouldBuffer => true;

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
