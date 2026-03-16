using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Saves.Runs;

public class SerializableRelic : IPacketSerializable
{
	[JsonPropertyName("id")]
	public ModelId? Id { get; set; }

	[JsonPropertyName("props")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public SavedProperties? Props { get; set; }

	[JsonPropertyName("floor_added_to_deck")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? FloorAddedToDeck { get; set; }

	public void Serialize(PacketWriter writer)
	{
		writer.WriteModelEntry(Id);
		writer.WriteBool(Props != null);
		if (Props != null)
		{
			writer.Write(Props);
		}
		writer.WriteBool(FloorAddedToDeck.HasValue);
		if (FloorAddedToDeck.HasValue)
		{
			writer.WriteInt(FloorAddedToDeck.Value, 8);
		}
	}

	public void Deserialize(PacketReader reader)
	{
		Id = reader.ReadModelIdAssumingType<RelicModel>();
		if (reader.ReadBool())
		{
			Props = reader.Read<SavedProperties>();
		}
		if (reader.ReadBool())
		{
			FloorAddedToDeck = reader.ReadInt(8);
		}
	}
}
