using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Saves.Runs;

public class SerializableModifier : IPacketSerializable
{
	[JsonPropertyName("id")]
	public ModelId? Id { get; set; }

	[JsonPropertyName("props")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public SavedProperties? Props { get; set; }

	public void Serialize(PacketWriter writer)
	{
		writer.WriteModelEntry(Id);
		writer.WriteBool(Props != null);
		if (Props != null)
		{
			writer.Write(Props);
		}
	}

	public void Deserialize(PacketReader reader)
	{
		Id = reader.ReadModelIdAssumingType<ModifierModel>();
		if (reader.ReadBool())
		{
			Props = reader.Read<SavedProperties>();
		}
	}
}
