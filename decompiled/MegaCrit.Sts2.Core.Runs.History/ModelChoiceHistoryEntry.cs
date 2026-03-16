using System;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Runs.History;

[Serializable]
public struct ModelChoiceHistoryEntry
{
	[JsonPropertyName("choice")]
	public ModelId choice;

	[JsonPropertyName("was_picked")]
	public bool wasPicked;

	public ModelChoiceHistoryEntry(ModelId choice, bool wasPicked)
	{
		this.choice = choice;
		this.wasPicked = wasPicked;
	}

	public void Serialize<T>(PacketWriter writer) where T : AbstractModel
	{
		writer.WriteModelEntry(choice);
		writer.WriteBool(wasPicked);
	}

	public void Deserialize<T>(PacketReader reader) where T : AbstractModel
	{
		choice = reader.ReadModelIdAssumingType<T>();
		wasPicked = reader.ReadBool();
	}
}
