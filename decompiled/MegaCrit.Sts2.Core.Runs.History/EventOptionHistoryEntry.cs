using System.Collections.Generic;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Runs.History;

public struct EventOptionHistoryEntry : IPacketSerializable
{
	[JsonPropertyName("title")]
	public LocString Title { get; set; }

	[JsonInclude]
	[JsonPropertyName("variables")]
	[JsonConverter(typeof(LocStringVariablesJsonConverter))]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public Dictionary<string, object>? Variables { get; set; }

	public void Serialize(PacketWriter writer)
	{
		writer.WriteString(Title.LocTable);
		writer.WriteString(Title.LocEntryKey);
		writer.WriteBool(Variables != null);
		if (Variables == null)
		{
			return;
		}
		List<(string, SerializableDynamicVar)> list = new List<(string, SerializableDynamicVar)>();
		foreach (KeyValuePair<string, object> variable in Variables)
		{
			SerializableDynamicVar? serializableDynamicVar = SerializableDynamicVar.FromDynamicVar(variable.Value);
			if (serializableDynamicVar.HasValue)
			{
				list.Add((variable.Key, serializableDynamicVar.Value));
			}
		}
		writer.WriteInt(list.Count);
		foreach (var (str, val) in list)
		{
			writer.WriteString(str);
			writer.Write(val);
		}
	}

	public void Deserialize(PacketReader reader)
	{
		string locTable = reader.ReadString();
		string locEntryKey = reader.ReadString();
		Title = new LocString(locTable, locEntryKey);
		if (reader.ReadBool())
		{
			Variables = new Dictionary<string, object>();
			int num = reader.ReadInt();
			for (int i = 0; i < num; i++)
			{
				string text = reader.ReadString();
				Variables[text] = reader.Read<SerializableDynamicVar>().ToDynamicVar(text);
			}
		}
	}
}
