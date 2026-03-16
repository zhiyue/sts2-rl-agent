using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace MegaCrit.Sts2.Core.Localization;

[JsonSourceGenerationOptions(ReadCommentHandling = JsonCommentHandling.Skip)]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(Dictionary<string, int>))]
[GeneratedCode("System.Text.Json.SourceGeneration", "9.0.12.31616")]
internal class LocManagerSerializerContext : JsonSerializerContext, IJsonTypeInfoResolver
{
	private JsonTypeInfo<Dictionary<string, int>>? _DictionaryStringInt32;

	private JsonTypeInfo<Dictionary<string, string>>? _DictionaryStringString;

	private JsonTypeInfo<int>? _Int32;

	private JsonTypeInfo<string>? _String;

	private static readonly JsonSerializerOptions s_defaultOptions = new JsonSerializerOptions
	{
		ReadCommentHandling = JsonCommentHandling.Skip
	};

	private const BindingFlags InstanceMemberBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	public JsonTypeInfo<Dictionary<string, int>> DictionaryStringInt32 => _DictionaryStringInt32 ?? (_DictionaryStringInt32 = (JsonTypeInfo<Dictionary<string, int>>)base.Options.GetTypeInfo(typeof(Dictionary<string, int>)));

	public JsonTypeInfo<Dictionary<string, string>> DictionaryStringString => _DictionaryStringString ?? (_DictionaryStringString = (JsonTypeInfo<Dictionary<string, string>>)base.Options.GetTypeInfo(typeof(Dictionary<string, string>)));

	public JsonTypeInfo<int> Int32 => _Int32 ?? (_Int32 = (JsonTypeInfo<int>)base.Options.GetTypeInfo(typeof(int)));

	public JsonTypeInfo<string> String => _String ?? (_String = (JsonTypeInfo<string>)base.Options.GetTypeInfo(typeof(string)));

	public static LocManagerSerializerContext Default { get; } = new LocManagerSerializerContext(new JsonSerializerOptions(s_defaultOptions));

	protected override JsonSerializerOptions? GeneratedSerializerOptions { get; } = s_defaultOptions;

	private JsonTypeInfo<Dictionary<string, int>> Create_DictionaryStringInt32(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<Dictionary<string, int>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<Dictionary<string, int>> collectionInfo = new JsonCollectionInfoValues<Dictionary<string, int>>
			{
				ObjectCreator = () => new Dictionary<string, int>(),
				SerializeHandler = DictionaryStringInt32SerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateDictionaryInfo<Dictionary<string, int>, string, int>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private void DictionaryStringInt32SerializeHandler(Utf8JsonWriter writer, Dictionary<string, int>? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		foreach (KeyValuePair<string, int> item in value)
		{
			writer.WriteNumber(item.Key, item.Value);
		}
		writer.WriteEndObject();
	}

	private JsonTypeInfo<Dictionary<string, string>> Create_DictionaryStringString(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<Dictionary<string, string>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<Dictionary<string, string>> collectionInfo = new JsonCollectionInfoValues<Dictionary<string, string>>
			{
				ObjectCreator = () => new Dictionary<string, string>(),
				SerializeHandler = DictionaryStringStringSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateDictionaryInfo<Dictionary<string, string>, string, string>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private void DictionaryStringStringSerializeHandler(Utf8JsonWriter writer, Dictionary<string, string>? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		foreach (KeyValuePair<string, string> item in value)
		{
			writer.WriteString(item.Key, item.Value);
		}
		writer.WriteEndObject();
	}

	private JsonTypeInfo<int> Create_Int32(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<int> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<int>(options, JsonMetadataServices.Int32Converter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<string> Create_String(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<string> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<string>(options, JsonMetadataServices.StringConverter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	public LocManagerSerializerContext()
		: base(null)
	{
	}

	public LocManagerSerializerContext(JsonSerializerOptions options)
		: base(options)
	{
	}

	private static bool TryGetTypeInfoForRuntimeCustomConverter<TJsonMetadataType>(JsonSerializerOptions options, out JsonTypeInfo<TJsonMetadataType> jsonTypeInfo)
	{
		JsonConverter runtimeConverterForType = GetRuntimeConverterForType(typeof(TJsonMetadataType), options);
		if (runtimeConverterForType != null)
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<TJsonMetadataType>(options, runtimeConverterForType);
			return true;
		}
		jsonTypeInfo = null;
		return false;
	}

	private static JsonConverter? GetRuntimeConverterForType(Type type, JsonSerializerOptions options)
	{
		for (int i = 0; i < options.Converters.Count; i++)
		{
			JsonConverter jsonConverter = options.Converters[i];
			if (jsonConverter != null && jsonConverter.CanConvert(type))
			{
				return ExpandConverter(type, jsonConverter, options, validateCanConvert: false);
			}
		}
		return null;
	}

	private static JsonConverter ExpandConverter(Type type, JsonConverter converter, JsonSerializerOptions options, bool validateCanConvert = true)
	{
		if (validateCanConvert && !converter.CanConvert(type))
		{
			throw new InvalidOperationException($"The converter '{converter.GetType()}' is not compatible with the type '{type}'.");
		}
		if (converter is JsonConverterFactory jsonConverterFactory)
		{
			converter = jsonConverterFactory.CreateConverter(type, options);
			if (converter == null || converter is JsonConverterFactory)
			{
				throw new InvalidOperationException($"The converter '{jsonConverterFactory.GetType()}' cannot return null or a JsonConverterFactory instance.");
			}
		}
		return converter;
	}

	public override JsonTypeInfo? GetTypeInfo(Type type)
	{
		base.Options.TryGetTypeInfo(type, out JsonTypeInfo typeInfo);
		return typeInfo;
	}

	JsonTypeInfo? IJsonTypeInfoResolver.GetTypeInfo(Type type, JsonSerializerOptions options)
	{
		if (type == typeof(Dictionary<string, int>))
		{
			return Create_DictionaryStringInt32(options);
		}
		if (type == typeof(Dictionary<string, string>))
		{
			return Create_DictionaryStringString(options);
		}
		if (type == typeof(int))
		{
			return Create_Int32(options);
		}
		if (type == typeof(string))
		{
			return Create_String(options);
		}
		return null;
	}
}
