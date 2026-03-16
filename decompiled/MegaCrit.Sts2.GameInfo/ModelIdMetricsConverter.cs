using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.GameInfo;

public class ModelIdMetricsConverter : JsonConverter<ModelId>
{
	public override ModelId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}

	public override void Write(Utf8JsonWriter writer, ModelId value, JsonSerializerOptions options)
	{
		if (value == ModelId.none)
		{
			writer.WriteNullValue();
		}
		else
		{
			writer.WriteStringValue(value.Entry);
		}
	}
}
