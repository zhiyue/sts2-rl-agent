using System;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Runs.History;

[Serializable]
public class AncientChoiceHistoryEntry : IPacketSerializable
{
	[JsonPropertyName("title")]
	public LocString Title { get; set; }

	[JsonPropertyName("was_chosen")]
	public bool WasChosen { get; set; }

	[JsonPropertyName("TextKey")]
	public string TextKey => Title.LocEntryKey.Split(".")[^2];

	public AncientChoiceHistoryEntry()
	{
		Title = new LocString(string.Empty, string.Empty);
	}

	public AncientChoiceHistoryEntry(LocString title, bool wasChosen)
	{
		Title = title;
		WasChosen = wasChosen;
	}

	public void Serialize(PacketWriter writer)
	{
		writer.WriteString(Title.LocTable);
		writer.WriteString(Title.LocEntryKey);
		writer.WriteBool(WasChosen);
	}

	public void Deserialize(PacketReader reader)
	{
		string locTable = reader.ReadString();
		string locEntryKey = reader.ReadString();
		Title = new LocString(locTable, locEntryKey);
		WasChosen = reader.ReadBool();
	}
}
