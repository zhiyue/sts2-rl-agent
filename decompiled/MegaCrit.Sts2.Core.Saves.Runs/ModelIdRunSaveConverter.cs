using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Saves.Runs;

public class ModelIdRunSaveConverter : JsonConverter<ModelId>
{
	public override ModelId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		string text = reader.GetString();
		if (text != null)
		{
			return ModelId.Deserialize(text);
		}
		return ModelId.none;
	}

	public override void Write(Utf8JsonWriter writer, ModelId value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}
