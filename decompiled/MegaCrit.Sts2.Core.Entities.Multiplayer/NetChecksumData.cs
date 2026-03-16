using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Entities.Multiplayer;

public struct NetChecksumData : IPacketSerializable
{
	public uint id;

	public uint checksum;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteUInt(id);
		writer.WriteUInt(checksum);
	}

	public void Deserialize(PacketReader reader)
	{
		id = reader.ReadUInt();
		checksum = reader.ReadUInt();
	}
}
