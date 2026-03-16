using System;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Runs.History;

[Serializable]
public struct CardChoiceHistoryEntry
{
	[JsonPropertyName("was_picked")]
	public bool wasPicked;

	[JsonPropertyName("card")]
	public SerializableCard Card { get; set; }

	public CardChoiceHistoryEntry(CardModel card, bool wasPicked)
	{
		Card = card.ToSerializable();
		this.wasPicked = wasPicked;
	}

	public void Serialize<T>(PacketWriter writer) where T : AbstractModel
	{
		writer.Write(Card);
		writer.WriteBool(wasPicked);
	}

	public void Deserialize<T>(PacketReader reader) where T : AbstractModel
	{
		Card = reader.Read<SerializableCard>();
		wasPicked = reader.ReadBool();
	}
}
