using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Checksums;

public struct ChecksumDataMessage : INetMessage, IPacketSerializable
{
	public NetChecksumData checksumData;

	public bool ShouldBroadcast => false;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.VeryDebug;

	public void Serialize(PacketWriter writer)
	{
		writer.Write(checksumData);
	}

	public void Deserialize(PacketReader reader)
	{
		checksumData = reader.Read<NetChecksumData>();
	}
}
