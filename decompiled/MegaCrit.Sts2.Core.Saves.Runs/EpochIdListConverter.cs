using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MegaCrit.Sts2.Core.Saves.Runs;

public class EpochIdListConverter : JsonConverter<List<string>>
{
	public override List<string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		List<string> list = new List<string>();
		if (reader.TokenType != JsonTokenType.StartArray)
		{
			throw new JsonException($"Expected start of array for epoch ID list, but got {reader.TokenType}");
		}
		while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
		{
			string text = reader.GetString();
			if (text != null)
			{
				int num = text.IndexOf('.');
				if (num >= 0)
				{
					string text2 = text;
					int num2 = num + 1;
					text = text2.Substring(num2, text2.Length - num2);
				}
				list.Add(text);
			}
		}
		return list;
	}

	public override void Write(Utf8JsonWriter writer, List<string> value, JsonSerializerOptions options)
	{
		writer.WriteStartArray();
		foreach (string item in value)
		{
			writer.WriteStringValue(item);
		}
		writer.WriteEndArray();
	}
}
