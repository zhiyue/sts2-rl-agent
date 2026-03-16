using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace MegaCrit.Sts2.Core.Localization;

[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true, UseStringEnumConverter = true)]
[JsonSerializable(typeof(Dictionary<string, SerializableDynamicVar>))]
[GeneratedCode("System.Text.Json.SourceGeneration", "9.0.12.31616")]
internal class SerializableDynamicVarDictionarySerializerContext : JsonSerializerContext, IJsonTypeInfoResolver
{
	private JsonTypeInfo<bool>? _Boolean;

	private JsonTypeInfo<decimal>? _Decimal;

	private JsonTypeInfo<DynamicVarType>? _DynamicVarType;

	private JsonTypeInfo<SerializableDynamicVar>? _SerializableDynamicVar;

	private JsonTypeInfo<Dictionary<string, SerializableDynamicVar>>? _DictionaryStringSerializableDynamicVar;

	private JsonTypeInfo<string>? _String;

	private static readonly JsonSerializerOptions s_defaultOptions = new JsonSerializerOptions
	{
		IncludeFields = true,
		WriteIndented = true
	};

	private const BindingFlags InstanceMemberBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private static readonly JsonEncodedText PropName_type = JsonEncodedText.Encode("type");

	private static readonly JsonEncodedText PropName_decimal_value = JsonEncodedText.Encode("decimal_value");

	private static readonly JsonEncodedText PropName_bool_value = JsonEncodedText.Encode("bool_value");

	private static readonly JsonEncodedText PropName_string_value = JsonEncodedText.Encode("string_value");

	public JsonTypeInfo<bool> Boolean => _Boolean ?? (_Boolean = (JsonTypeInfo<bool>)base.Options.GetTypeInfo(typeof(bool)));

	public JsonTypeInfo<decimal> Decimal => _Decimal ?? (_Decimal = (JsonTypeInfo<decimal>)base.Options.GetTypeInfo(typeof(decimal)));

	public JsonTypeInfo<DynamicVarType> DynamicVarType => _DynamicVarType ?? (_DynamicVarType = (JsonTypeInfo<DynamicVarType>)base.Options.GetTypeInfo(typeof(DynamicVarType)));

	public JsonTypeInfo<SerializableDynamicVar> SerializableDynamicVar => _SerializableDynamicVar ?? (_SerializableDynamicVar = (JsonTypeInfo<SerializableDynamicVar>)base.Options.GetTypeInfo(typeof(SerializableDynamicVar)));

	public JsonTypeInfo<Dictionary<string, SerializableDynamicVar>> DictionaryStringSerializableDynamicVar => _DictionaryStringSerializableDynamicVar ?? (_DictionaryStringSerializableDynamicVar = (JsonTypeInfo<Dictionary<string, SerializableDynamicVar>>)base.Options.GetTypeInfo(typeof(Dictionary<string, SerializableDynamicVar>)));

	public JsonTypeInfo<string> String => _String ?? (_String = (JsonTypeInfo<string>)base.Options.GetTypeInfo(typeof(string)));

	public static SerializableDynamicVarDictionarySerializerContext Default { get; } = new SerializableDynamicVarDictionarySerializerContext(new JsonSerializerOptions(s_defaultOptions));

	protected override JsonSerializerOptions? GeneratedSerializerOptions { get; } = s_defaultOptions;

	private JsonTypeInfo<bool> Create_Boolean(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<bool> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<bool>(options, JsonMetadataServices.BooleanConverter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<decimal> Create_Decimal(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<decimal> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<decimal>(options, JsonMetadataServices.DecimalConverter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<DynamicVarType> Create_DynamicVarType(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<DynamicVarType> jsonTypeInfo))
		{
			JsonConverter converter = ExpandConverter(typeof(DynamicVarType), new JsonStringEnumConverter<DynamicVarType>(), options);
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<DynamicVarType>(options, converter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<SerializableDynamicVar> Create_SerializableDynamicVar(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SerializableDynamicVar> jsonTypeInfo))
		{
			JsonObjectInfoValues<SerializableDynamicVar> objectInfo = new JsonObjectInfoValues<SerializableDynamicVar>
			{
				ObjectCreator = () => default(SerializableDynamicVar),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SerializableDynamicVarPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SerializableDynamicVar).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = SerializableDynamicVarSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SerializableDynamicVarPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[4];
		JsonPropertyInfoValues<DynamicVarType> jsonPropertyInfoValues = new JsonPropertyInfoValues<DynamicVarType>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableDynamicVar);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableDynamicVar)obj).type;
		jsonPropertyInfoValues.Setter = delegate(object obj, DynamicVarType value)
		{
			Unsafe.Unbox<SerializableDynamicVar>(obj).type = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "type";
		jsonPropertyInfoValues.JsonPropertyName = "type";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableDynamicVar).GetField("type", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<DynamicVarType> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<decimal> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<decimal>();
		jsonPropertyInfoValues2.IsProperty = false;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SerializableDynamicVar);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SerializableDynamicVar)obj).decimalValue;
		jsonPropertyInfoValues2.Setter = delegate(object obj, decimal value)
		{
			Unsafe.Unbox<SerializableDynamicVar>(obj).decimalValue = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "decimalValue";
		jsonPropertyInfoValues2.JsonPropertyName = "decimal_value";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SerializableDynamicVar).GetField("decimalValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<decimal> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<bool> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues3.IsProperty = false;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(SerializableDynamicVar);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((SerializableDynamicVar)obj).boolValue;
		jsonPropertyInfoValues3.Setter = delegate(object obj, bool value)
		{
			Unsafe.Unbox<SerializableDynamicVar>(obj).boolValue = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "boolValue";
		jsonPropertyInfoValues3.JsonPropertyName = "bool_value";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(SerializableDynamicVar).GetField("boolValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<bool> propertyInfo3 = jsonPropertyInfoValues3;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		JsonPropertyInfoValues<string> jsonPropertyInfoValues4 = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues4.IsProperty = false;
		jsonPropertyInfoValues4.IsPublic = true;
		jsonPropertyInfoValues4.IsVirtual = false;
		jsonPropertyInfoValues4.DeclaringType = typeof(SerializableDynamicVar);
		jsonPropertyInfoValues4.Converter = null;
		jsonPropertyInfoValues4.Getter = (object obj) => ((SerializableDynamicVar)obj).stringValue;
		jsonPropertyInfoValues4.Setter = delegate(object obj, string? value)
		{
			Unsafe.Unbox<SerializableDynamicVar>(obj).stringValue = value;
		};
		jsonPropertyInfoValues4.IgnoreCondition = null;
		jsonPropertyInfoValues4.HasJsonInclude = false;
		jsonPropertyInfoValues4.IsExtensionData = false;
		jsonPropertyInfoValues4.NumberHandling = null;
		jsonPropertyInfoValues4.PropertyName = "stringValue";
		jsonPropertyInfoValues4.JsonPropertyName = "string_value";
		jsonPropertyInfoValues4.AttributeProviderFactory = () => typeof(SerializableDynamicVar).GetField("stringValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<string> propertyInfo4 = jsonPropertyInfoValues4;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		return array;
	}

	private void SerializableDynamicVarSerializeHandler(Utf8JsonWriter writer, SerializableDynamicVar value)
	{
		writer.WriteStartObject();
		writer.WritePropertyName(PropName_type);
		JsonSerializer.Serialize(writer, value.type, DynamicVarType);
		writer.WriteNumber(PropName_decimal_value, value.decimalValue);
		writer.WriteBoolean(PropName_bool_value, value.boolValue);
		writer.WriteString(PropName_string_value, value.stringValue);
		writer.WriteEndObject();
	}

	private JsonTypeInfo<Dictionary<string, SerializableDynamicVar>> Create_DictionaryStringSerializableDynamicVar(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<Dictionary<string, SerializableDynamicVar>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<Dictionary<string, SerializableDynamicVar>> collectionInfo = new JsonCollectionInfoValues<Dictionary<string, SerializableDynamicVar>>
			{
				ObjectCreator = () => new Dictionary<string, SerializableDynamicVar>(),
				SerializeHandler = DictionaryStringSerializableDynamicVarSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateDictionaryInfo<Dictionary<string, SerializableDynamicVar>, string, SerializableDynamicVar>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private void DictionaryStringSerializableDynamicVarSerializeHandler(Utf8JsonWriter writer, Dictionary<string, SerializableDynamicVar>? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		foreach (KeyValuePair<string, SerializableDynamicVar> item in value)
		{
			writer.WritePropertyName(item.Key);
			SerializableDynamicVarSerializeHandler(writer, item.Value);
		}
		writer.WriteEndObject();
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

	public SerializableDynamicVarDictionarySerializerContext()
		: base(null)
	{
	}

	public SerializableDynamicVarDictionarySerializerContext(JsonSerializerOptions options)
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
		if (type == typeof(bool))
		{
			return Create_Boolean(options);
		}
		if (type == typeof(decimal))
		{
			return Create_Decimal(options);
		}
		if (type == typeof(DynamicVarType))
		{
			return Create_DynamicVarType(options);
		}
		if (type == typeof(SerializableDynamicVar))
		{
			return Create_SerializableDynamicVar(options);
		}
		if (type == typeof(Dictionary<string, SerializableDynamicVar>))
		{
			return Create_DictionaryStringSerializableDynamicVar(options);
		}
		if (type == typeof(string))
		{
			return Create_String(options);
		}
		return null;
	}
}
