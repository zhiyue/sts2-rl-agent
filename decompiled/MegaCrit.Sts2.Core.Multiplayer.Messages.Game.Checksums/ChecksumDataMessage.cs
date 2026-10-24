using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Checksums;

/// <summary>
/// Sent from clients to host when a checksum is generated. The host should validate the checksum. No action is needed
/// if the checksum matches the host's, but if it does not, a StateDivergenceMessage should be generated and sent to
/// the client.
/// </summary>
public struct ChecksumDataMessage : INetMessage, IPacketSerializable
{
	public NetChecksumData checksumData;

	public bool ShouldBroadcast => false;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.VeryDebug;

	public bool ShouldBuffer => true;

	public void Serialize(PacketWriter writer)
	{
		writer.Write(checksumData);
	}

	public void Deserialize(PacketReader reader)
	{
		checksumData = reader.Read<NetChecksumData>();
	}
}
