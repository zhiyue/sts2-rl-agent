using System;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace MegaCrit.Sts2.Core.Debug;

[JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true, Converters = new Type[] { typeof(CustomDateTimeConverter) })]
[JsonSerializable(typeof(ReleaseInfo))]
[GeneratedCode("System.Text.Json.SourceGeneration", "9.0.12.31616")]
internal class ReleaseInfoJsonSerializerContext : JsonSerializerContext, IJsonTypeInfoResolver
{
	private JsonTypeInfo<ReleaseInfo>? _ReleaseInfo;

	private JsonTypeInfo<DateTime>? _DateTime;

	private JsonTypeInfo<int>? _Int32;

	private JsonTypeInfo<string>? _String;

	private static readonly JsonSerializerOptions s_defaultOptions = new JsonSerializerOptions
	{
		Converters = { (JsonConverter)new CustomDateTimeConverter() },
		WriteIndented = true
	};

	private const BindingFlags InstanceMemberBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	/// <summary>
	/// Defines the source generated JSON serialization contract metadata for a given type.
	/// </summary>
	public JsonTypeInfo<ReleaseInfo> ReleaseInfo => _ReleaseInfo ?? (_ReleaseInfo = (JsonTypeInfo<ReleaseInfo>)base.Options.GetTypeInfo(typeof(ReleaseInfo)));

	/// <summary>
	/// Defines the source generated JSON serialization contract metadata for a given type.
	/// </summary>
	public JsonTypeInfo<DateTime> DateTime => _DateTime ?? (_DateTime = (JsonTypeInfo<DateTime>)base.Options.GetTypeInfo(typeof(DateTime)));

	/// <summary>
	/// Defines the source generated JSON serialization contract metadata for a given type.
	/// </summary>
	public JsonTypeInfo<int> Int32 => _Int32 ?? (_Int32 = (JsonTypeInfo<int>)base.Options.GetTypeInfo(typeof(int)));

	/// <summary>
	/// Defines the source generated JSON serialization contract metadata for a given type.
	/// </summary>
	public JsonTypeInfo<string> String => _String ?? (_String = (JsonTypeInfo<string>)base.Options.GetTypeInfo(typeof(string)));

	/// <summary>
	/// The default <see cref="T:System.Text.Json.Serialization.JsonSerializerContext" /> associated with a default <see cref="T:System.Text.Json.JsonSerializerOptions" /> instance.
	/// </summary>
	public static ReleaseInfoJsonSerializerContext Default { get; } = new ReleaseInfoJsonSerializerContext(new JsonSerializerOptions(s_defaultOptions));

	/// <summary>
	/// The source-generated options associated with this context.
	/// </summary>
	protected override JsonSerializerOptions? GeneratedSerializerOptions { get; } = s_defaultOptions;

	private JsonTypeInfo<ReleaseInfo> Create_ReleaseInfo(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<ReleaseInfo> jsonTypeInfo))
		{
			JsonObjectInfoValues<ReleaseInfo> jsonObjectInfoValues = new JsonObjectInfoValues<ReleaseInfo>();
			jsonObjectInfoValues.ObjectCreator = null;
			jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = (object[] args) => new ReleaseInfo
			{
				Commit = (string)args[0],
				Version = (string)args[1],
				Date = (DateTime)args[2],
				Branch = (string)args[3],
				MainAssemblyHash = (int)args[4]
			};
			jsonObjectInfoValues.PropertyMetadataInitializer = (JsonSerializerContext _) => ReleaseInfoPropInit(options);
			jsonObjectInfoValues.ConstructorParameterMetadataInitializer = ReleaseInfoCtorParamInit;
			jsonObjectInfoValues.ConstructorAttributeProviderFactory = () => typeof(ReleaseInfo).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null);
			jsonObjectInfoValues.SerializeHandler = null;
			JsonObjectInfoValues<ReleaseInfo> objectInfo = jsonObjectInfoValues;
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] ReleaseInfoPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[5];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(ReleaseInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((ReleaseInfo)obj).Commit;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Commit";
		jsonPropertyInfoValues.JsonPropertyName = "commit";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(ReleaseInfo).GetProperty("Commit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsRequired = true;
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(ReleaseInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((ReleaseInfo)obj).Version;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Version";
		jsonPropertyInfoValues.JsonPropertyName = "version";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(ReleaseInfo).GetProperty("Version", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo2 = jsonPropertyInfoValues;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsRequired = true;
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		JsonPropertyInfoValues<DateTime> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<DateTime>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(ReleaseInfo);
		jsonPropertyInfoValues2.Converter = (JsonConverter<DateTime>)ExpandConverter(typeof(DateTime), new CustomDateTimeConverter(), options);
		jsonPropertyInfoValues2.Getter = (object obj) => ((ReleaseInfo)obj).Date;
		jsonPropertyInfoValues2.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "Date";
		jsonPropertyInfoValues2.JsonPropertyName = "date";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(ReleaseInfo).GetProperty("Date", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(DateTime), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<DateTime> propertyInfo3 = jsonPropertyInfoValues2;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		array[2].IsRequired = true;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(ReleaseInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((ReleaseInfo)obj).Branch;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Branch";
		jsonPropertyInfoValues.JsonPropertyName = "branch";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(ReleaseInfo).GetProperty("Branch", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo4 = jsonPropertyInfoValues;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		array[3].IsRequired = true;
		array[3].IsGetNullable = false;
		array[3].IsSetNullable = false;
		JsonPropertyInfoValues<int> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(ReleaseInfo);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((ReleaseInfo)obj).MainAssemblyHash;
		jsonPropertyInfoValues3.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "MainAssemblyHash";
		jsonPropertyInfoValues3.JsonPropertyName = "main_assembly_hash";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(ReleaseInfo).GetProperty("MainAssemblyHash", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo5 = jsonPropertyInfoValues3;
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		array[4].IsRequired = true;
		return array;
	}

	private static JsonParameterInfoValues[] ReleaseInfoCtorParamInit()
	{
		return new JsonParameterInfoValues[5]
		{
			new JsonParameterInfoValues
			{
				Name = "Commit",
				ParameterType = typeof(string),
				Position = 0,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Version",
				ParameterType = typeof(string),
				Position = 1,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Date",
				ParameterType = typeof(DateTime),
				Position = 2,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Branch",
				ParameterType = typeof(string),
				Position = 3,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "MainAssemblyHash",
				ParameterType = typeof(int),
				Position = 4,
				IsNullable = false,
				IsMemberInitializer = true
			}
		};
	}

	private JsonTypeInfo<DateTime> Create_DateTime(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<DateTime> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<DateTime>(options, JsonMetadataServices.DateTimeConverter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
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

	/// <inheritdoc />
	public ReleaseInfoJsonSerializerContext()
		: base(null)
	{
	}

	/// <inheritdoc />
	public ReleaseInfoJsonSerializerContext(JsonSerializerOptions options)
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

	/// <inheritdoc />
	public override JsonTypeInfo? GetTypeInfo(Type type)
	{
		base.Options.TryGetTypeInfo(type, out JsonTypeInfo typeInfo);
		return typeInfo;
	}

	JsonTypeInfo? IJsonTypeInfoResolver.GetTypeInfo(Type type, JsonSerializerOptions options)
	{
		if (type == typeof(ReleaseInfo))
		{
			return Create_ReleaseInfo(options);
		}
		if (type == typeof(DateTime))
		{
			return Create_DateTime(options);
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
