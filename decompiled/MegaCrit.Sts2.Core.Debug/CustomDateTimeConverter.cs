using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MegaCrit.Sts2.Core.Debug;

public class CustomDateTimeConverter : JsonConverter<DateTime>
{
	public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		string s = reader.GetString() ?? string.Empty;
		return DateTime.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
	}

	public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss zzz", CultureInfo.InvariantCulture));
	}
}
