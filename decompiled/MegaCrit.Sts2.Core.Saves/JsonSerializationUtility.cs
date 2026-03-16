using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Saves;

public static class JsonSerializationUtility
{
	public static IJsonTypeInfoResolver DefaultResolver { get; } = MegaCritSerializerContext.Default;

	public static JsonSerializerOptions Options { get; } = new JsonSerializerOptions(MegaCritSerializerContext.DefaultGeneratedSerializerOptions)
	{
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
		TypeInfoResolver = MegaCritSerializerContext.Default.WithAddedModifier(AlphabetizeProperties).WithAddedModifier(JsonSerializeConditionAttribute.CheckJsonSerializeConditionsModifier)
	};

	public static void AddTypeInfoResolver(IJsonTypeInfoResolver resolver)
	{
		Options.TypeInfoResolverChain.Add(resolver);
	}

	public static async Task<string> SerializeAsync<T>(T data) where T : ISaveSchema
	{
		using MemoryStream stream = new MemoryStream();
		await JsonSerializer.SerializeAsync((Stream)stream, data, GetTypeInfo<T>(), default(CancellationToken));
		stream.Position = 0L;
		using StreamReader reader = new StreamReader(stream);
		return await reader.ReadToEndAsync();
	}

	public static JsonTypeInfo<T> GetTypeInfo<T>(T value)
	{
		return (JsonTypeInfo<T>)Options.GetTypeInfo(typeof(T));
	}

	public static JsonTypeInfo<T> GetTypeInfo<T>()
	{
		return (JsonTypeInfo<T>)Options.GetTypeInfo(typeof(T));
	}

	public static void AlphabetizeProperties(JsonTypeInfo info)
	{
		if (info.Kind == JsonTypeInfoKind.Object)
		{
			List<JsonPropertyInfo> list = new List<JsonPropertyInfo>();
			list.AddRange(info.Properties);
			list.Sort((JsonPropertyInfo p1, JsonPropertyInfo p2) => string.CompareOrdinal(p1.Name, p2.Name));
			info.Properties.Clear();
			for (int num = 0; num < list.Count; num++)
			{
				list[num].Order = num;
				info.Properties.Add(list[num]);
			}
		}
	}

	public static string ToJson<T>(T obj) where T : ISaveSchema
	{
		return JsonSerializer.Serialize(obj, GetTypeInfo<T>());
	}

	public static ReadSaveResult<T> FromJson<T>(string json) where T : ISaveSchema, new()
	{
		if (string.IsNullOrWhiteSpace(json))
		{
			Log.Error($"The json for type={typeof(T)} was empty!");
			return new ReadSaveResult<T>(ReadSaveStatus.FileEmpty);
		}
		try
		{
			T val = JsonSerializer.Deserialize(json, GetTypeInfo<T>());
			if (val == null)
			{
				Log.Error($"Json parsed as null! type={typeof(T)}");
				return new ReadSaveResult<T>(ReadSaveStatus.JsonParseError);
			}
			return new ReadSaveResult<T>(val);
		}
		catch (JsonException ex)
		{
			string value = ex.Path ?? "unknown";
			Log.Error($"Failed to deserialize type={typeof(T)} at path={value}, line={ex.LineNumber}, position={ex.BytePositionInLine}: {ex.Message}");
			return new ReadSaveResult<T>(ReadSaveStatus.JsonParseError, $"JSON error at {value} (line {ex.LineNumber}): {ex.Message}");
		}
	}
}
