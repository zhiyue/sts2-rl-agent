namespace MegaCrit.Sts2.Core.Multiplayer.Serialization;

public interface IPacketSerializable
{
	void Serialize(PacketWriter writer);

	void Deserialize(PacketReader reader);
}
