using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Saves.Runs;

public class SerializableActModel : IPacketSerializable
{
	[JsonPropertyName("id")]
	public ModelId? Id { get; set; }

	[JsonPropertyName("rooms")]
	public SerializableRoomSet SerializableRooms { get; set; }

	[JsonPropertyName("saved_map")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public SerializableActMap? SavedMap { get; set; }

	public void Serialize(PacketWriter writer)
	{
		writer.WriteModelEntry(Id);
		writer.Write(SerializableRooms);
		writer.WriteBool(SavedMap != null);
		if (SavedMap != null)
		{
			writer.Write(SavedMap);
		}
	}

	public void Deserialize(PacketReader reader)
	{
		Id = reader.ReadModelIdAssumingType<ActModel>();
		SerializableRooms = reader.Read<SerializableRoomSet>();
		if (reader.ReadBool())
		{
			SavedMap = reader.Read<SerializableActMap>();
		}
	}
}
