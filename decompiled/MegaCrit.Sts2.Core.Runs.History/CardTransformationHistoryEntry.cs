using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Runs.History;

public struct CardTransformationHistoryEntry : IPacketSerializable
{
	[JsonPropertyName("original_card")]
	public SerializableCard OriginalCard { get; set; }

	[JsonPropertyName("final_card")]
	public SerializableCard FinalCard { get; set; }

	public CardTransformationHistoryEntry(CardModel originalCard, CardModel finalCard)
	{
		OriginalCard = originalCard.ToSerializable();
		FinalCard = finalCard.ToSerializable();
	}

	public void Serialize(PacketWriter writer)
	{
		writer.Write(OriginalCard);
		writer.Write(FinalCard);
	}

	public void Deserialize(PacketReader reader)
	{
		OriginalCard = reader.Read<SerializableCard>();
		FinalCard = reader.Read<SerializableCard>();
	}
}
