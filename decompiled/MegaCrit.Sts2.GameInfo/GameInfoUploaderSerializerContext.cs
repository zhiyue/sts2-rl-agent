using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.GameInfo.Objects;

namespace MegaCrit.Sts2.GameInfo;

[JsonSourceGenerationOptions(WriteIndented = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, IncludeFields = true, Converters = new Type[] { typeof(ModelIdMetricsConverter) })]
[JsonSerializable(typeof(List<IGameInfo>))]
[GeneratedCode("System.Text.Json.SourceGeneration", "9.0.12.31616")]
internal class GameInfoUploaderSerializerContext : JsonSerializerContext, IJsonTypeInfoResolver
{
	private JsonTypeInfo<bool>? _Boolean;

	private JsonTypeInfo<ModelId>? _ModelId;

	private JsonTypeInfo<AncientChoiceInfo>? _AncientChoiceInfo;

	private JsonTypeInfo<CardInfo>? _CardInfo;

	private JsonTypeInfo<DailyMods>? _DailyMods;

	private JsonTypeInfo<EnchantmentInfo>? _EnchantmentInfo;

	private JsonTypeInfo<EncounterInfo>? _EncounterInfo;

	private JsonTypeInfo<MegaCrit.Sts2.GameInfo.Objects.EventInfo>? _EventInfo;

	private JsonTypeInfo<IGameInfo>? _IGameInfo;

	private JsonTypeInfo<Keywords>? _Keywords;

	private JsonTypeInfo<NeowBonusInfo>? _NeowBonusInfo;

	private JsonTypeInfo<PotionInfo>? _PotionInfo;

	private JsonTypeInfo<RelicInfo>? _RelicInfo;

	private JsonTypeInfo<List<IGameInfo>>? _ListIGameInfo;

	private JsonTypeInfo<List<string>>? _ListString;

	private JsonTypeInfo<int>? _Int32;

	private JsonTypeInfo<string>? _String;

	private static readonly JsonSerializerOptions s_defaultOptions = new JsonSerializerOptions
	{
		Converters = { (JsonConverter)new ModelIdMetricsConverter() },
		IncludeFields = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = true
	};

	private const BindingFlags InstanceMemberBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private static readonly JsonEncodedText PropName_category = JsonEncodedText.Encode("category");

	private static readonly JsonEncodedText PropName_entry = JsonEncodedText.Encode("entry");

	private static readonly JsonEncodedText PropName_name = JsonEncodedText.Encode("name");

	private static readonly JsonEncodedText PropName_bot_keyword = JsonEncodedText.Encode("bot_keyword");

	private static readonly JsonEncodedText PropName_bot_text = JsonEncodedText.Encode("bot_text");

	private static readonly JsonEncodedText PropName_id = JsonEncodedText.Encode("id");

	private static readonly JsonEncodedText PropName_text = JsonEncodedText.Encode("text");

	private static readonly JsonEncodedText PropName_ancient = JsonEncodedText.Encode("ancient");

	private static readonly JsonEncodedText PropName_upgraded = JsonEncodedText.Encode("upgraded");

	private static readonly JsonEncodedText PropName_color = JsonEncodedText.Encode("color");

	private static readonly JsonEncodedText PropName_rarity = JsonEncodedText.Encode("rarity");

	private static readonly JsonEncodedText PropName_type = JsonEncodedText.Encode("type");

	private static readonly JsonEncodedText PropName_base_damage = JsonEncodedText.Encode("base_damage");

	private static readonly JsonEncodedText PropName_energy = JsonEncodedText.Encode("energy");

	private static readonly JsonEncodedText PropName_star_cost = JsonEncodedText.Encode("star_cost");

	private static readonly JsonEncodedText PropName_has_art = JsonEncodedText.Encode("has_art");

	private static readonly JsonEncodedText PropName_has_joke_art = JsonEncodedText.Encode("has_joke_art");

	private static readonly JsonEncodedText PropName_act = JsonEncodedText.Encode("act");

	private static readonly JsonEncodedText PropName_tier = JsonEncodedText.Encode("tier");

	private static readonly JsonEncodedText PropName_options = JsonEncodedText.Encode("options");

	public JsonTypeInfo<bool> Boolean => _Boolean ?? (_Boolean = (JsonTypeInfo<bool>)base.Options.GetTypeInfo(typeof(bool)));

	public JsonTypeInfo<ModelId> ModelId => _ModelId ?? (_ModelId = (JsonTypeInfo<ModelId>)base.Options.GetTypeInfo(typeof(ModelId)));

	public JsonTypeInfo<AncientChoiceInfo> AncientChoiceInfo => _AncientChoiceInfo ?? (_AncientChoiceInfo = (JsonTypeInfo<AncientChoiceInfo>)base.Options.GetTypeInfo(typeof(AncientChoiceInfo)));

	public JsonTypeInfo<CardInfo> CardInfo => _CardInfo ?? (_CardInfo = (JsonTypeInfo<CardInfo>)base.Options.GetTypeInfo(typeof(CardInfo)));

	public JsonTypeInfo<DailyMods> DailyMods => _DailyMods ?? (_DailyMods = (JsonTypeInfo<DailyMods>)base.Options.GetTypeInfo(typeof(DailyMods)));

	public JsonTypeInfo<EnchantmentInfo> EnchantmentInfo => _EnchantmentInfo ?? (_EnchantmentInfo = (JsonTypeInfo<EnchantmentInfo>)base.Options.GetTypeInfo(typeof(EnchantmentInfo)));

	public JsonTypeInfo<EncounterInfo> EncounterInfo => _EncounterInfo ?? (_EncounterInfo = (JsonTypeInfo<EncounterInfo>)base.Options.GetTypeInfo(typeof(EncounterInfo)));

	public JsonTypeInfo<MegaCrit.Sts2.GameInfo.Objects.EventInfo> EventInfo => _EventInfo ?? (_EventInfo = (JsonTypeInfo<MegaCrit.Sts2.GameInfo.Objects.EventInfo>)base.Options.GetTypeInfo(typeof(MegaCrit.Sts2.GameInfo.Objects.EventInfo)));

	public JsonTypeInfo<IGameInfo> IGameInfo => _IGameInfo ?? (_IGameInfo = (JsonTypeInfo<IGameInfo>)base.Options.GetTypeInfo(typeof(IGameInfo)));

	public JsonTypeInfo<Keywords> Keywords => _Keywords ?? (_Keywords = (JsonTypeInfo<Keywords>)base.Options.GetTypeInfo(typeof(Keywords)));

	public JsonTypeInfo<NeowBonusInfo> NeowBonusInfo => _NeowBonusInfo ?? (_NeowBonusInfo = (JsonTypeInfo<NeowBonusInfo>)base.Options.GetTypeInfo(typeof(NeowBonusInfo)));

	public JsonTypeInfo<PotionInfo> PotionInfo => _PotionInfo ?? (_PotionInfo = (JsonTypeInfo<PotionInfo>)base.Options.GetTypeInfo(typeof(PotionInfo)));

	public JsonTypeInfo<RelicInfo> RelicInfo => _RelicInfo ?? (_RelicInfo = (JsonTypeInfo<RelicInfo>)base.Options.GetTypeInfo(typeof(RelicInfo)));

	public JsonTypeInfo<List<IGameInfo>> ListIGameInfo => _ListIGameInfo ?? (_ListIGameInfo = (JsonTypeInfo<List<IGameInfo>>)base.Options.GetTypeInfo(typeof(List<IGameInfo>)));

	public JsonTypeInfo<List<string>> ListString => _ListString ?? (_ListString = (JsonTypeInfo<List<string>>)base.Options.GetTypeInfo(typeof(List<string>)));

	public JsonTypeInfo<int> Int32 => _Int32 ?? (_Int32 = (JsonTypeInfo<int>)base.Options.GetTypeInfo(typeof(int)));

	public JsonTypeInfo<string> String => _String ?? (_String = (JsonTypeInfo<string>)base.Options.GetTypeInfo(typeof(string)));

	public static GameInfoUploaderSerializerContext Default { get; } = new GameInfoUploaderSerializerContext(new JsonSerializerOptions(s_defaultOptions));

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

	private JsonTypeInfo<ModelId> Create_ModelId(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<ModelId> jsonTypeInfo))
		{
			JsonObjectInfoValues<ModelId> jsonObjectInfoValues = new JsonObjectInfoValues<ModelId>();
			jsonObjectInfoValues.ObjectCreator = null;
			jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = (object[] args) => new ModelId((string)args[0], (string)args[1]);
			jsonObjectInfoValues.PropertyMetadataInitializer = (JsonSerializerContext _) => ModelIdPropInit(options);
			jsonObjectInfoValues.ConstructorParameterMetadataInitializer = ModelIdCtorParamInit;
			jsonObjectInfoValues.ConstructorAttributeProviderFactory = () => typeof(ModelId).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[2]
			{
				typeof(string),
				typeof(string)
			}, null);
			jsonObjectInfoValues.SerializeHandler = ModelIdSerializeHandler;
			JsonObjectInfoValues<ModelId> objectInfo = jsonObjectInfoValues;
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] ModelIdPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(ModelId);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((ModelId)obj).Category;
		jsonPropertyInfoValues.Setter = null;
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Category";
		jsonPropertyInfoValues.JsonPropertyName = null;
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(ModelId).GetProperty("Category", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsGetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(ModelId);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((ModelId)obj).Entry;
		jsonPropertyInfoValues.Setter = null;
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Entry";
		jsonPropertyInfoValues.JsonPropertyName = null;
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(ModelId).GetProperty("Entry", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo2 = jsonPropertyInfoValues;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsGetNullable = false;
		return array;
	}

	private void ModelIdSerializeHandler(Utf8JsonWriter writer, ModelId? value)
	{
		if ((object)value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteString(PropName_category, value.Category);
		writer.WriteString(PropName_entry, value.Entry);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] ModelIdCtorParamInit()
	{
		return new JsonParameterInfoValues[2]
		{
			new JsonParameterInfoValues
			{
				Name = "category",
				ParameterType = typeof(string),
				Position = 0,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			},
			new JsonParameterInfoValues
			{
				Name = "entry",
				ParameterType = typeof(string),
				Position = 1,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			}
		};
	}

	private JsonTypeInfo<AncientChoiceInfo> Create_AncientChoiceInfo(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<AncientChoiceInfo> jsonTypeInfo))
		{
			JsonObjectInfoValues<AncientChoiceInfo> jsonObjectInfoValues = new JsonObjectInfoValues<AncientChoiceInfo>();
			jsonObjectInfoValues.ObjectCreator = null;
			jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = (object[] args) => new AncientChoiceInfo
			{
				Name = (string)args[0],
				BotKeyword = (string)args[1],
				BotText = (string)args[2],
				Id = (string)args[3],
				Text = (string)args[4],
				Ancient = (string)args[5]
			};
			jsonObjectInfoValues.PropertyMetadataInitializer = (JsonSerializerContext _) => AncientChoiceInfoPropInit(options);
			jsonObjectInfoValues.ConstructorParameterMetadataInitializer = AncientChoiceInfoCtorParamInit;
			jsonObjectInfoValues.ConstructorAttributeProviderFactory = () => typeof(AncientChoiceInfo).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null);
			jsonObjectInfoValues.SerializeHandler = AncientChoiceInfoSerializeHandler;
			JsonObjectInfoValues<AncientChoiceInfo> objectInfo = jsonObjectInfoValues;
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] AncientChoiceInfoPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[6];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(AncientChoiceInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((AncientChoiceInfo)obj).Name;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Name";
		jsonPropertyInfoValues.JsonPropertyName = "name";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(AncientChoiceInfo).GetProperty("Name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsRequired = true;
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(AncientChoiceInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((AncientChoiceInfo)obj).BotKeyword;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "BotKeyword";
		jsonPropertyInfoValues.JsonPropertyName = "bot_keyword";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(AncientChoiceInfo).GetProperty("BotKeyword", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo2 = jsonPropertyInfoValues;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsRequired = true;
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(AncientChoiceInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((AncientChoiceInfo)obj).BotText;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "BotText";
		jsonPropertyInfoValues.JsonPropertyName = "bot_text";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(AncientChoiceInfo).GetProperty("BotText", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo3 = jsonPropertyInfoValues;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		array[2].IsRequired = true;
		array[2].IsGetNullable = false;
		array[2].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(AncientChoiceInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((AncientChoiceInfo)obj).Id;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Id";
		jsonPropertyInfoValues.JsonPropertyName = "id";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(AncientChoiceInfo).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo4 = jsonPropertyInfoValues;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		array[3].IsRequired = true;
		array[3].IsGetNullable = false;
		array[3].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(AncientChoiceInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((AncientChoiceInfo)obj).Text;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Text";
		jsonPropertyInfoValues.JsonPropertyName = "text";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(AncientChoiceInfo).GetProperty("Text", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo5 = jsonPropertyInfoValues;
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		array[4].IsRequired = true;
		array[4].IsGetNullable = false;
		array[4].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(AncientChoiceInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((AncientChoiceInfo)obj).Ancient;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Ancient";
		jsonPropertyInfoValues.JsonPropertyName = "ancient";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(AncientChoiceInfo).GetProperty("Ancient", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo6 = jsonPropertyInfoValues;
		array[5] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo6);
		array[5].IsRequired = true;
		array[5].IsGetNullable = false;
		array[5].IsSetNullable = false;
		return array;
	}

	private void AncientChoiceInfoSerializeHandler(Utf8JsonWriter writer, AncientChoiceInfo? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteString(PropName_name, value.Name);
		writer.WriteString(PropName_bot_keyword, value.BotKeyword);
		writer.WriteString(PropName_bot_text, value.BotText);
		writer.WriteString(PropName_id, value.Id);
		writer.WriteString(PropName_text, value.Text);
		writer.WriteString(PropName_ancient, value.Ancient);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] AncientChoiceInfoCtorParamInit()
	{
		return new JsonParameterInfoValues[6]
		{
			new JsonParameterInfoValues
			{
				Name = "Name",
				ParameterType = typeof(string),
				Position = 0,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "BotKeyword",
				ParameterType = typeof(string),
				Position = 1,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "BotText",
				ParameterType = typeof(string),
				Position = 2,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Id",
				ParameterType = typeof(string),
				Position = 3,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Text",
				ParameterType = typeof(string),
				Position = 4,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Ancient",
				ParameterType = typeof(string),
				Position = 5,
				IsNullable = false,
				IsMemberInitializer = true
			}
		};
	}

	private JsonTypeInfo<CardInfo> Create_CardInfo(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<CardInfo> jsonTypeInfo))
		{
			JsonObjectInfoValues<CardInfo> jsonObjectInfoValues = new JsonObjectInfoValues<CardInfo>();
			jsonObjectInfoValues.ObjectCreator = null;
			jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = (object[] args) => new CardInfo
			{
				Name = (string)args[0],
				BotKeyword = (string)args[1],
				BotText = (string)args[2],
				Id = (ModelId)args[3],
				Upgraded = (bool)args[4],
				Color = (string)args[5],
				Rarity = (string)args[6],
				Type = (string)args[7],
				BaseDamage = (int)args[8],
				Energy = (int)args[9],
				StarCost = (int)args[10],
				Text = (string)args[11],
				HasArt = (bool)args[12],
				HasJokeArt = (bool)args[13]
			};
			jsonObjectInfoValues.PropertyMetadataInitializer = (JsonSerializerContext _) => CardInfoPropInit(options);
			jsonObjectInfoValues.ConstructorParameterMetadataInitializer = CardInfoCtorParamInit;
			jsonObjectInfoValues.ConstructorAttributeProviderFactory = () => typeof(CardInfo).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null);
			jsonObjectInfoValues.SerializeHandler = CardInfoSerializeHandler;
			JsonObjectInfoValues<CardInfo> objectInfo = jsonObjectInfoValues;
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] CardInfoPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[14];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(CardInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((CardInfo)obj).Name;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Name";
		jsonPropertyInfoValues.JsonPropertyName = "name";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(CardInfo).GetProperty("Name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsRequired = true;
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(CardInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((CardInfo)obj).BotKeyword;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "BotKeyword";
		jsonPropertyInfoValues.JsonPropertyName = "bot_keyword";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(CardInfo).GetProperty("BotKeyword", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo2 = jsonPropertyInfoValues;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsRequired = true;
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(CardInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((CardInfo)obj).BotText;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "BotText";
		jsonPropertyInfoValues.JsonPropertyName = "bot_text";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(CardInfo).GetProperty("BotText", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo3 = jsonPropertyInfoValues;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		array[2].IsRequired = true;
		array[2].IsGetNullable = false;
		array[2].IsSetNullable = false;
		JsonPropertyInfoValues<ModelId> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(CardInfo);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((CardInfo)obj).Id;
		jsonPropertyInfoValues2.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "Id";
		jsonPropertyInfoValues2.JsonPropertyName = "id";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(CardInfo).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModelId), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModelId> propertyInfo4 = jsonPropertyInfoValues2;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		array[3].IsRequired = true;
		array[3].IsGetNullable = false;
		array[3].IsSetNullable = false;
		JsonPropertyInfoValues<bool> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(CardInfo);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((CardInfo)obj).Upgraded;
		jsonPropertyInfoValues3.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "Upgraded";
		jsonPropertyInfoValues3.JsonPropertyName = "upgraded";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(CardInfo).GetProperty("Upgraded", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<bool> propertyInfo5 = jsonPropertyInfoValues3;
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		array[4].IsRequired = true;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(CardInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((CardInfo)obj).Color;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Color";
		jsonPropertyInfoValues.JsonPropertyName = "color";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(CardInfo).GetProperty("Color", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo6 = jsonPropertyInfoValues;
		array[5] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo6);
		array[5].IsRequired = true;
		array[5].IsGetNullable = false;
		array[5].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(CardInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((CardInfo)obj).Rarity;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Rarity";
		jsonPropertyInfoValues.JsonPropertyName = "rarity";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(CardInfo).GetProperty("Rarity", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo7 = jsonPropertyInfoValues;
		array[6] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo7);
		array[6].IsRequired = true;
		array[6].IsGetNullable = false;
		array[6].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(CardInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((CardInfo)obj).Type;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Type";
		jsonPropertyInfoValues.JsonPropertyName = "type";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(CardInfo).GetProperty("Type", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo8 = jsonPropertyInfoValues;
		array[7] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo8);
		array[7].IsRequired = true;
		array[7].IsGetNullable = false;
		array[7].IsSetNullable = false;
		JsonPropertyInfoValues<int> jsonPropertyInfoValues4 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues4.IsProperty = true;
		jsonPropertyInfoValues4.IsPublic = true;
		jsonPropertyInfoValues4.IsVirtual = false;
		jsonPropertyInfoValues4.DeclaringType = typeof(CardInfo);
		jsonPropertyInfoValues4.Converter = null;
		jsonPropertyInfoValues4.Getter = (object obj) => ((CardInfo)obj).BaseDamage;
		jsonPropertyInfoValues4.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues4.IgnoreCondition = null;
		jsonPropertyInfoValues4.HasJsonInclude = false;
		jsonPropertyInfoValues4.IsExtensionData = false;
		jsonPropertyInfoValues4.NumberHandling = null;
		jsonPropertyInfoValues4.PropertyName = "BaseDamage";
		jsonPropertyInfoValues4.JsonPropertyName = "base_damage";
		jsonPropertyInfoValues4.AttributeProviderFactory = () => typeof(CardInfo).GetProperty("BaseDamage", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo9 = jsonPropertyInfoValues4;
		array[8] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo9);
		array[8].IsRequired = true;
		jsonPropertyInfoValues4 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues4.IsProperty = true;
		jsonPropertyInfoValues4.IsPublic = true;
		jsonPropertyInfoValues4.IsVirtual = false;
		jsonPropertyInfoValues4.DeclaringType = typeof(CardInfo);
		jsonPropertyInfoValues4.Converter = null;
		jsonPropertyInfoValues4.Getter = (object obj) => ((CardInfo)obj).Energy;
		jsonPropertyInfoValues4.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues4.IgnoreCondition = null;
		jsonPropertyInfoValues4.HasJsonInclude = false;
		jsonPropertyInfoValues4.IsExtensionData = false;
		jsonPropertyInfoValues4.NumberHandling = null;
		jsonPropertyInfoValues4.PropertyName = "Energy";
		jsonPropertyInfoValues4.JsonPropertyName = "energy";
		jsonPropertyInfoValues4.AttributeProviderFactory = () => typeof(CardInfo).GetProperty("Energy", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo10 = jsonPropertyInfoValues4;
		array[9] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo10);
		array[9].IsRequired = true;
		jsonPropertyInfoValues4 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues4.IsProperty = true;
		jsonPropertyInfoValues4.IsPublic = true;
		jsonPropertyInfoValues4.IsVirtual = false;
		jsonPropertyInfoValues4.DeclaringType = typeof(CardInfo);
		jsonPropertyInfoValues4.Converter = null;
		jsonPropertyInfoValues4.Getter = (object obj) => ((CardInfo)obj).StarCost;
		jsonPropertyInfoValues4.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues4.IgnoreCondition = null;
		jsonPropertyInfoValues4.HasJsonInclude = false;
		jsonPropertyInfoValues4.IsExtensionData = false;
		jsonPropertyInfoValues4.NumberHandling = null;
		jsonPropertyInfoValues4.PropertyName = "StarCost";
		jsonPropertyInfoValues4.JsonPropertyName = "star_cost";
		jsonPropertyInfoValues4.AttributeProviderFactory = () => typeof(CardInfo).GetProperty("StarCost", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo11 = jsonPropertyInfoValues4;
		array[10] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo11);
		array[10].IsRequired = true;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(CardInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((CardInfo)obj).Text;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Text";
		jsonPropertyInfoValues.JsonPropertyName = "text";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(CardInfo).GetProperty("Text", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo12 = jsonPropertyInfoValues;
		array[11] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo12);
		array[11].IsRequired = true;
		array[11].IsGetNullable = false;
		array[11].IsSetNullable = false;
		jsonPropertyInfoValues3 = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(CardInfo);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((CardInfo)obj).HasArt;
		jsonPropertyInfoValues3.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "HasArt";
		jsonPropertyInfoValues3.JsonPropertyName = "has_art";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(CardInfo).GetProperty("HasArt", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<bool> propertyInfo13 = jsonPropertyInfoValues3;
		array[12] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo13);
		array[12].IsRequired = true;
		jsonPropertyInfoValues3 = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(CardInfo);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((CardInfo)obj).HasJokeArt;
		jsonPropertyInfoValues3.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "HasJokeArt";
		jsonPropertyInfoValues3.JsonPropertyName = "has_joke_art";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(CardInfo).GetProperty("HasJokeArt", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<bool> propertyInfo14 = jsonPropertyInfoValues3;
		array[13] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo14);
		array[13].IsRequired = true;
		return array;
	}

	private void CardInfoSerializeHandler(Utf8JsonWriter writer, CardInfo? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteString(PropName_name, value.Name);
		writer.WriteString(PropName_bot_keyword, value.BotKeyword);
		writer.WriteString(PropName_bot_text, value.BotText);
		writer.WritePropertyName(PropName_id);
		ModelIdSerializeHandler(writer, value.Id);
		writer.WriteBoolean(PropName_upgraded, value.Upgraded);
		writer.WriteString(PropName_color, value.Color);
		writer.WriteString(PropName_rarity, value.Rarity);
		writer.WriteString(PropName_type, value.Type);
		writer.WriteNumber(PropName_base_damage, value.BaseDamage);
		writer.WriteNumber(PropName_energy, value.Energy);
		writer.WriteNumber(PropName_star_cost, value.StarCost);
		writer.WriteString(PropName_text, value.Text);
		writer.WriteBoolean(PropName_has_art, value.HasArt);
		writer.WriteBoolean(PropName_has_joke_art, value.HasJokeArt);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] CardInfoCtorParamInit()
	{
		return new JsonParameterInfoValues[14]
		{
			new JsonParameterInfoValues
			{
				Name = "Name",
				ParameterType = typeof(string),
				Position = 0,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "BotKeyword",
				ParameterType = typeof(string),
				Position = 1,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "BotText",
				ParameterType = typeof(string),
				Position = 2,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Id",
				ParameterType = typeof(ModelId),
				Position = 3,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Upgraded",
				ParameterType = typeof(bool),
				Position = 4,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Color",
				ParameterType = typeof(string),
				Position = 5,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Rarity",
				ParameterType = typeof(string),
				Position = 6,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Type",
				ParameterType = typeof(string),
				Position = 7,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "BaseDamage",
				ParameterType = typeof(int),
				Position = 8,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Energy",
				ParameterType = typeof(int),
				Position = 9,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "StarCost",
				ParameterType = typeof(int),
				Position = 10,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Text",
				ParameterType = typeof(string),
				Position = 11,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "HasArt",
				ParameterType = typeof(bool),
				Position = 12,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "HasJokeArt",
				ParameterType = typeof(bool),
				Position = 13,
				IsNullable = false,
				IsMemberInitializer = true
			}
		};
	}

	private JsonTypeInfo<DailyMods> Create_DailyMods(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<DailyMods> jsonTypeInfo))
		{
			JsonObjectInfoValues<DailyMods> jsonObjectInfoValues = new JsonObjectInfoValues<DailyMods>();
			jsonObjectInfoValues.ObjectCreator = null;
			jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = (object[] args) => new DailyMods
			{
				Name = (string)args[0],
				BotKeyword = (string)args[1],
				BotText = (string)args[2],
				Text = (string)args[3]
			};
			jsonObjectInfoValues.PropertyMetadataInitializer = (JsonSerializerContext _) => DailyModsPropInit(options);
			jsonObjectInfoValues.ConstructorParameterMetadataInitializer = DailyModsCtorParamInit;
			jsonObjectInfoValues.ConstructorAttributeProviderFactory = () => typeof(DailyMods).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null);
			jsonObjectInfoValues.SerializeHandler = DailyModsSerializeHandler;
			JsonObjectInfoValues<DailyMods> objectInfo = jsonObjectInfoValues;
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] DailyModsPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[4];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(DailyMods);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((DailyMods)obj).Name;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Name";
		jsonPropertyInfoValues.JsonPropertyName = "name";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(DailyMods).GetProperty("Name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsRequired = true;
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(DailyMods);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((DailyMods)obj).BotKeyword;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "BotKeyword";
		jsonPropertyInfoValues.JsonPropertyName = "bot_keyword";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(DailyMods).GetProperty("BotKeyword", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo2 = jsonPropertyInfoValues;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsRequired = true;
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(DailyMods);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((DailyMods)obj).BotText;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "BotText";
		jsonPropertyInfoValues.JsonPropertyName = "bot_text";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(DailyMods).GetProperty("BotText", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo3 = jsonPropertyInfoValues;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		array[2].IsRequired = true;
		array[2].IsGetNullable = false;
		array[2].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(DailyMods);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((DailyMods)obj).Text;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Text";
		jsonPropertyInfoValues.JsonPropertyName = "text";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(DailyMods).GetProperty("Text", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo4 = jsonPropertyInfoValues;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		array[3].IsRequired = true;
		array[3].IsGetNullable = false;
		array[3].IsSetNullable = false;
		return array;
	}

	private void DailyModsSerializeHandler(Utf8JsonWriter writer, DailyMods? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteString(PropName_name, value.Name);
		writer.WriteString(PropName_bot_keyword, value.BotKeyword);
		writer.WriteString(PropName_bot_text, value.BotText);
		writer.WriteString(PropName_text, value.Text);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] DailyModsCtorParamInit()
	{
		return new JsonParameterInfoValues[4]
		{
			new JsonParameterInfoValues
			{
				Name = "Name",
				ParameterType = typeof(string),
				Position = 0,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "BotKeyword",
				ParameterType = typeof(string),
				Position = 1,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "BotText",
				ParameterType = typeof(string),
				Position = 2,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Text",
				ParameterType = typeof(string),
				Position = 3,
				IsNullable = false,
				IsMemberInitializer = true
			}
		};
	}

	private JsonTypeInfo<EnchantmentInfo> Create_EnchantmentInfo(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<EnchantmentInfo> jsonTypeInfo))
		{
			JsonObjectInfoValues<EnchantmentInfo> jsonObjectInfoValues = new JsonObjectInfoValues<EnchantmentInfo>();
			jsonObjectInfoValues.ObjectCreator = null;
			jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = (object[] args) => new EnchantmentInfo
			{
				Name = (string)args[0],
				BotKeyword = (string)args[1],
				BotText = (string)args[2],
				Id = (ModelId)args[3],
				Text = (string)args[4]
			};
			jsonObjectInfoValues.PropertyMetadataInitializer = (JsonSerializerContext _) => EnchantmentInfoPropInit(options);
			jsonObjectInfoValues.ConstructorParameterMetadataInitializer = EnchantmentInfoCtorParamInit;
			jsonObjectInfoValues.ConstructorAttributeProviderFactory = () => typeof(EnchantmentInfo).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null);
			jsonObjectInfoValues.SerializeHandler = EnchantmentInfoSerializeHandler;
			JsonObjectInfoValues<EnchantmentInfo> objectInfo = jsonObjectInfoValues;
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] EnchantmentInfoPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[5];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(EnchantmentInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((EnchantmentInfo)obj).Name;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Name";
		jsonPropertyInfoValues.JsonPropertyName = "name";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(EnchantmentInfo).GetProperty("Name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsRequired = true;
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(EnchantmentInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((EnchantmentInfo)obj).BotKeyword;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "BotKeyword";
		jsonPropertyInfoValues.JsonPropertyName = "bot_keyword";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(EnchantmentInfo).GetProperty("BotKeyword", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo2 = jsonPropertyInfoValues;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsRequired = true;
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(EnchantmentInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((EnchantmentInfo)obj).BotText;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "BotText";
		jsonPropertyInfoValues.JsonPropertyName = "bot_text";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(EnchantmentInfo).GetProperty("BotText", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo3 = jsonPropertyInfoValues;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		array[2].IsRequired = true;
		array[2].IsGetNullable = false;
		array[2].IsSetNullable = false;
		JsonPropertyInfoValues<ModelId> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(EnchantmentInfo);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((EnchantmentInfo)obj).Id;
		jsonPropertyInfoValues2.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "Id";
		jsonPropertyInfoValues2.JsonPropertyName = "id";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(EnchantmentInfo).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModelId), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModelId> propertyInfo4 = jsonPropertyInfoValues2;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		array[3].IsRequired = true;
		array[3].IsGetNullable = false;
		array[3].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(EnchantmentInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((EnchantmentInfo)obj).Text;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Text";
		jsonPropertyInfoValues.JsonPropertyName = "text";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(EnchantmentInfo).GetProperty("Text", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo5 = jsonPropertyInfoValues;
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		array[4].IsRequired = true;
		array[4].IsGetNullable = false;
		array[4].IsSetNullable = false;
		return array;
	}

	private void EnchantmentInfoSerializeHandler(Utf8JsonWriter writer, EnchantmentInfo? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteString(PropName_name, value.Name);
		writer.WriteString(PropName_bot_keyword, value.BotKeyword);
		writer.WriteString(PropName_bot_text, value.BotText);
		writer.WritePropertyName(PropName_id);
		ModelIdSerializeHandler(writer, value.Id);
		writer.WriteString(PropName_text, value.Text);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] EnchantmentInfoCtorParamInit()
	{
		return new JsonParameterInfoValues[5]
		{
			new JsonParameterInfoValues
			{
				Name = "Name",
				ParameterType = typeof(string),
				Position = 0,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "BotKeyword",
				ParameterType = typeof(string),
				Position = 1,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "BotText",
				ParameterType = typeof(string),
				Position = 2,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Id",
				ParameterType = typeof(ModelId),
				Position = 3,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Text",
				ParameterType = typeof(string),
				Position = 4,
				IsNullable = false,
				IsMemberInitializer = true
			}
		};
	}

	private JsonTypeInfo<EncounterInfo> Create_EncounterInfo(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<EncounterInfo> jsonTypeInfo))
		{
			JsonObjectInfoValues<EncounterInfo> jsonObjectInfoValues = new JsonObjectInfoValues<EncounterInfo>();
			jsonObjectInfoValues.ObjectCreator = null;
			jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = (object[] args) => new EncounterInfo
			{
				Name = (string)args[0],
				BotKeyword = (string)args[1],
				BotText = (string)args[2],
				Id = (ModelId)args[3],
				Act = (string)args[4],
				Tier = (string)args[5]
			};
			jsonObjectInfoValues.PropertyMetadataInitializer = (JsonSerializerContext _) => EncounterInfoPropInit(options);
			jsonObjectInfoValues.ConstructorParameterMetadataInitializer = EncounterInfoCtorParamInit;
			jsonObjectInfoValues.ConstructorAttributeProviderFactory = () => typeof(EncounterInfo).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null);
			jsonObjectInfoValues.SerializeHandler = EncounterInfoSerializeHandler;
			JsonObjectInfoValues<EncounterInfo> objectInfo = jsonObjectInfoValues;
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] EncounterInfoPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[6];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(EncounterInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((EncounterInfo)obj).Name;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Name";
		jsonPropertyInfoValues.JsonPropertyName = "name";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(EncounterInfo).GetProperty("Name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsRequired = true;
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(EncounterInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((EncounterInfo)obj).BotKeyword;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "BotKeyword";
		jsonPropertyInfoValues.JsonPropertyName = "bot_keyword";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(EncounterInfo).GetProperty("BotKeyword", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo2 = jsonPropertyInfoValues;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsRequired = true;
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(EncounterInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((EncounterInfo)obj).BotText;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "BotText";
		jsonPropertyInfoValues.JsonPropertyName = "bot_text";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(EncounterInfo).GetProperty("BotText", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo3 = jsonPropertyInfoValues;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		array[2].IsRequired = true;
		array[2].IsGetNullable = false;
		array[2].IsSetNullable = false;
		JsonPropertyInfoValues<ModelId> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(EncounterInfo);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((EncounterInfo)obj).Id;
		jsonPropertyInfoValues2.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "Id";
		jsonPropertyInfoValues2.JsonPropertyName = "id";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(EncounterInfo).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModelId), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModelId> propertyInfo4 = jsonPropertyInfoValues2;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		array[3].IsRequired = true;
		array[3].IsGetNullable = false;
		array[3].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(EncounterInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((EncounterInfo)obj).Act;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Act";
		jsonPropertyInfoValues.JsonPropertyName = "act";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(EncounterInfo).GetProperty("Act", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo5 = jsonPropertyInfoValues;
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		array[4].IsRequired = true;
		array[4].IsGetNullable = false;
		array[4].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(EncounterInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((EncounterInfo)obj).Tier;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Tier";
		jsonPropertyInfoValues.JsonPropertyName = "tier";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(EncounterInfo).GetProperty("Tier", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo6 = jsonPropertyInfoValues;
		array[5] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo6);
		array[5].IsRequired = true;
		array[5].IsGetNullable = false;
		array[5].IsSetNullable = false;
		return array;
	}

	private void EncounterInfoSerializeHandler(Utf8JsonWriter writer, EncounterInfo? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteString(PropName_name, value.Name);
		writer.WriteString(PropName_bot_keyword, value.BotKeyword);
		writer.WriteString(PropName_bot_text, value.BotText);
		writer.WritePropertyName(PropName_id);
		ModelIdSerializeHandler(writer, value.Id);
		writer.WriteString(PropName_act, value.Act);
		writer.WriteString(PropName_tier, value.Tier);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] EncounterInfoCtorParamInit()
	{
		return new JsonParameterInfoValues[6]
		{
			new JsonParameterInfoValues
			{
				Name = "Name",
				ParameterType = typeof(string),
				Position = 0,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "BotKeyword",
				ParameterType = typeof(string),
				Position = 1,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "BotText",
				ParameterType = typeof(string),
				Position = 2,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Id",
				ParameterType = typeof(ModelId),
				Position = 3,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Act",
				ParameterType = typeof(string),
				Position = 4,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Tier",
				ParameterType = typeof(string),
				Position = 5,
				IsNullable = false,
				IsMemberInitializer = true
			}
		};
	}

	private JsonTypeInfo<MegaCrit.Sts2.GameInfo.Objects.EventInfo> Create_EventInfo(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<MegaCrit.Sts2.GameInfo.Objects.EventInfo> jsonTypeInfo))
		{
			JsonObjectInfoValues<MegaCrit.Sts2.GameInfo.Objects.EventInfo> jsonObjectInfoValues = new JsonObjectInfoValues<MegaCrit.Sts2.GameInfo.Objects.EventInfo>();
			jsonObjectInfoValues.ObjectCreator = null;
			jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = (object[] args) => new MegaCrit.Sts2.GameInfo.Objects.EventInfo
			{
				Name = (string)args[0],
				BotKeyword = (string)args[1],
				BotText = (string)args[2],
				Id = (ModelId)args[3],
				Act = (string)args[4],
				Options = (List<string>)args[5]
			};
			jsonObjectInfoValues.PropertyMetadataInitializer = (JsonSerializerContext _) => EventInfoPropInit(options);
			jsonObjectInfoValues.ConstructorParameterMetadataInitializer = EventInfoCtorParamInit;
			jsonObjectInfoValues.ConstructorAttributeProviderFactory = () => typeof(MegaCrit.Sts2.GameInfo.Objects.EventInfo).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null);
			jsonObjectInfoValues.SerializeHandler = EventInfoSerializeHandler;
			JsonObjectInfoValues<MegaCrit.Sts2.GameInfo.Objects.EventInfo> objectInfo = jsonObjectInfoValues;
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] EventInfoPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[6];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(MegaCrit.Sts2.GameInfo.Objects.EventInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((MegaCrit.Sts2.GameInfo.Objects.EventInfo)obj).Name;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Name";
		jsonPropertyInfoValues.JsonPropertyName = "name";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(MegaCrit.Sts2.GameInfo.Objects.EventInfo).GetProperty("Name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsRequired = true;
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(MegaCrit.Sts2.GameInfo.Objects.EventInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((MegaCrit.Sts2.GameInfo.Objects.EventInfo)obj).BotKeyword;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "BotKeyword";
		jsonPropertyInfoValues.JsonPropertyName = "bot_keyword";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(MegaCrit.Sts2.GameInfo.Objects.EventInfo).GetProperty("BotKeyword", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo2 = jsonPropertyInfoValues;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsRequired = true;
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(MegaCrit.Sts2.GameInfo.Objects.EventInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((MegaCrit.Sts2.GameInfo.Objects.EventInfo)obj).BotText;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "BotText";
		jsonPropertyInfoValues.JsonPropertyName = "bot_text";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(MegaCrit.Sts2.GameInfo.Objects.EventInfo).GetProperty("BotText", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo3 = jsonPropertyInfoValues;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		array[2].IsRequired = true;
		array[2].IsGetNullable = false;
		array[2].IsSetNullable = false;
		JsonPropertyInfoValues<ModelId> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(MegaCrit.Sts2.GameInfo.Objects.EventInfo);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((MegaCrit.Sts2.GameInfo.Objects.EventInfo)obj).Id;
		jsonPropertyInfoValues2.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "Id";
		jsonPropertyInfoValues2.JsonPropertyName = "id";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(MegaCrit.Sts2.GameInfo.Objects.EventInfo).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModelId), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModelId> propertyInfo4 = jsonPropertyInfoValues2;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		array[3].IsRequired = true;
		array[3].IsGetNullable = false;
		array[3].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(MegaCrit.Sts2.GameInfo.Objects.EventInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((MegaCrit.Sts2.GameInfo.Objects.EventInfo)obj).Act;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Act";
		jsonPropertyInfoValues.JsonPropertyName = "act";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(MegaCrit.Sts2.GameInfo.Objects.EventInfo).GetProperty("Act", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo5 = jsonPropertyInfoValues;
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		array[4].IsRequired = true;
		array[4].IsGetNullable = false;
		array[4].IsSetNullable = false;
		JsonPropertyInfoValues<List<string>> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<List<string>>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(MegaCrit.Sts2.GameInfo.Objects.EventInfo);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((MegaCrit.Sts2.GameInfo.Objects.EventInfo)obj).Options;
		jsonPropertyInfoValues3.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "Options";
		jsonPropertyInfoValues3.JsonPropertyName = "options";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(MegaCrit.Sts2.GameInfo.Objects.EventInfo).GetProperty("Options", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<string>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<string>> propertyInfo6 = jsonPropertyInfoValues3;
		array[5] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo6);
		array[5].IsRequired = true;
		array[5].IsGetNullable = false;
		array[5].IsSetNullable = false;
		return array;
	}

	private void EventInfoSerializeHandler(Utf8JsonWriter writer, MegaCrit.Sts2.GameInfo.Objects.EventInfo? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteString(PropName_name, value.Name);
		writer.WriteString(PropName_bot_keyword, value.BotKeyword);
		writer.WriteString(PropName_bot_text, value.BotText);
		writer.WritePropertyName(PropName_id);
		ModelIdSerializeHandler(writer, value.Id);
		writer.WriteString(PropName_act, value.Act);
		writer.WritePropertyName(PropName_options);
		ListStringSerializeHandler(writer, value.Options);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] EventInfoCtorParamInit()
	{
		return new JsonParameterInfoValues[6]
		{
			new JsonParameterInfoValues
			{
				Name = "Name",
				ParameterType = typeof(string),
				Position = 0,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "BotKeyword",
				ParameterType = typeof(string),
				Position = 1,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "BotText",
				ParameterType = typeof(string),
				Position = 2,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Id",
				ParameterType = typeof(ModelId),
				Position = 3,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Act",
				ParameterType = typeof(string),
				Position = 4,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Options",
				ParameterType = typeof(List<string>),
				Position = 5,
				IsNullable = false,
				IsMemberInitializer = true
			}
		};
	}

	private JsonTypeInfo<IGameInfo> Create_IGameInfo(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<IGameInfo> jsonTypeInfo))
		{
			JsonObjectInfoValues<IGameInfo> objectInfo = new JsonObjectInfoValues<IGameInfo>
			{
				ObjectCreator = null,
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => IGameInfoPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = null,
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] IGameInfoPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[3];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = true;
		jsonPropertyInfoValues.DeclaringType = typeof(IGameInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((IGameInfo)obj).Name;
		jsonPropertyInfoValues.Setter = null;
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Name";
		jsonPropertyInfoValues.JsonPropertyName = "name";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(IGameInfo).GetProperty("Name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsGetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = true;
		jsonPropertyInfoValues.DeclaringType = typeof(IGameInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((IGameInfo)obj).BotKeyword;
		jsonPropertyInfoValues.Setter = null;
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "BotKeyword";
		jsonPropertyInfoValues.JsonPropertyName = "bot_keyword";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(IGameInfo).GetProperty("BotKeyword", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo2 = jsonPropertyInfoValues;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsGetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = true;
		jsonPropertyInfoValues.DeclaringType = typeof(IGameInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((IGameInfo)obj).BotText;
		jsonPropertyInfoValues.Setter = null;
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "BotText";
		jsonPropertyInfoValues.JsonPropertyName = "bot_text";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(IGameInfo).GetProperty("BotText", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo3 = jsonPropertyInfoValues;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		array[2].IsGetNullable = false;
		return array;
	}

	private JsonTypeInfo<Keywords> Create_Keywords(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<Keywords> jsonTypeInfo))
		{
			JsonObjectInfoValues<Keywords> jsonObjectInfoValues = new JsonObjectInfoValues<Keywords>();
			jsonObjectInfoValues.ObjectCreator = null;
			jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = (object[] args) => new Keywords
			{
				Name = (string)args[0],
				BotKeyword = (string)args[1],
				BotText = (string)args[2]
			};
			jsonObjectInfoValues.PropertyMetadataInitializer = (JsonSerializerContext _) => KeywordsPropInit(options);
			jsonObjectInfoValues.ConstructorParameterMetadataInitializer = KeywordsCtorParamInit;
			jsonObjectInfoValues.ConstructorAttributeProviderFactory = () => typeof(Keywords).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null);
			jsonObjectInfoValues.SerializeHandler = KeywordsSerializeHandler;
			JsonObjectInfoValues<Keywords> objectInfo = jsonObjectInfoValues;
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] KeywordsPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[3];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(Keywords);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((Keywords)obj).Name;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Name";
		jsonPropertyInfoValues.JsonPropertyName = "name";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(Keywords).GetProperty("Name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsRequired = true;
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(Keywords);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((Keywords)obj).BotKeyword;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "BotKeyword";
		jsonPropertyInfoValues.JsonPropertyName = "bot_keyword";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(Keywords).GetProperty("BotKeyword", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo2 = jsonPropertyInfoValues;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsRequired = true;
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(Keywords);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((Keywords)obj).BotText;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "BotText";
		jsonPropertyInfoValues.JsonPropertyName = "bot_text";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(Keywords).GetProperty("BotText", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo3 = jsonPropertyInfoValues;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		array[2].IsRequired = true;
		array[2].IsGetNullable = false;
		array[2].IsSetNullable = false;
		return array;
	}

	private void KeywordsSerializeHandler(Utf8JsonWriter writer, Keywords? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteString(PropName_name, value.Name);
		writer.WriteString(PropName_bot_keyword, value.BotKeyword);
		writer.WriteString(PropName_bot_text, value.BotText);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] KeywordsCtorParamInit()
	{
		return new JsonParameterInfoValues[3]
		{
			new JsonParameterInfoValues
			{
				Name = "Name",
				ParameterType = typeof(string),
				Position = 0,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "BotKeyword",
				ParameterType = typeof(string),
				Position = 1,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "BotText",
				ParameterType = typeof(string),
				Position = 2,
				IsNullable = false,
				IsMemberInitializer = true
			}
		};
	}

	private JsonTypeInfo<NeowBonusInfo> Create_NeowBonusInfo(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<NeowBonusInfo> jsonTypeInfo))
		{
			JsonObjectInfoValues<NeowBonusInfo> jsonObjectInfoValues = new JsonObjectInfoValues<NeowBonusInfo>();
			jsonObjectInfoValues.ObjectCreator = null;
			jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = (object[] args) => new NeowBonusInfo
			{
				Name = (string)args[0],
				BotKeyword = (string)args[1],
				BotText = (string)args[2],
				Id = (string)args[3],
				Text = (string)args[4]
			};
			jsonObjectInfoValues.PropertyMetadataInitializer = (JsonSerializerContext _) => NeowBonusInfoPropInit(options);
			jsonObjectInfoValues.ConstructorParameterMetadataInitializer = NeowBonusInfoCtorParamInit;
			jsonObjectInfoValues.ConstructorAttributeProviderFactory = () => typeof(NeowBonusInfo).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null);
			jsonObjectInfoValues.SerializeHandler = NeowBonusInfoSerializeHandler;
			JsonObjectInfoValues<NeowBonusInfo> objectInfo = jsonObjectInfoValues;
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] NeowBonusInfoPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[5];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(NeowBonusInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((NeowBonusInfo)obj).Name;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Name";
		jsonPropertyInfoValues.JsonPropertyName = "name";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(NeowBonusInfo).GetProperty("Name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsRequired = true;
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(NeowBonusInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((NeowBonusInfo)obj).BotKeyword;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "BotKeyword";
		jsonPropertyInfoValues.JsonPropertyName = "bot_keyword";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(NeowBonusInfo).GetProperty("BotKeyword", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo2 = jsonPropertyInfoValues;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsRequired = true;
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(NeowBonusInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((NeowBonusInfo)obj).BotText;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "BotText";
		jsonPropertyInfoValues.JsonPropertyName = "bot_text";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(NeowBonusInfo).GetProperty("BotText", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo3 = jsonPropertyInfoValues;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		array[2].IsRequired = true;
		array[2].IsGetNullable = false;
		array[2].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(NeowBonusInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((NeowBonusInfo)obj).Id;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Id";
		jsonPropertyInfoValues.JsonPropertyName = "id";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(NeowBonusInfo).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo4 = jsonPropertyInfoValues;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		array[3].IsRequired = true;
		array[3].IsGetNullable = false;
		array[3].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(NeowBonusInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((NeowBonusInfo)obj).Text;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Text";
		jsonPropertyInfoValues.JsonPropertyName = "text";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(NeowBonusInfo).GetProperty("Text", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo5 = jsonPropertyInfoValues;
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		array[4].IsRequired = true;
		array[4].IsGetNullable = false;
		array[4].IsSetNullable = false;
		return array;
	}

	private void NeowBonusInfoSerializeHandler(Utf8JsonWriter writer, NeowBonusInfo? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteString(PropName_name, value.Name);
		writer.WriteString(PropName_bot_keyword, value.BotKeyword);
		writer.WriteString(PropName_bot_text, value.BotText);
		writer.WriteString(PropName_id, value.Id);
		writer.WriteString(PropName_text, value.Text);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] NeowBonusInfoCtorParamInit()
	{
		return new JsonParameterInfoValues[5]
		{
			new JsonParameterInfoValues
			{
				Name = "Name",
				ParameterType = typeof(string),
				Position = 0,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "BotKeyword",
				ParameterType = typeof(string),
				Position = 1,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "BotText",
				ParameterType = typeof(string),
				Position = 2,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Id",
				ParameterType = typeof(string),
				Position = 3,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Text",
				ParameterType = typeof(string),
				Position = 4,
				IsNullable = false,
				IsMemberInitializer = true
			}
		};
	}

	private JsonTypeInfo<PotionInfo> Create_PotionInfo(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<PotionInfo> jsonTypeInfo))
		{
			JsonObjectInfoValues<PotionInfo> jsonObjectInfoValues = new JsonObjectInfoValues<PotionInfo>();
			jsonObjectInfoValues.ObjectCreator = null;
			jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = (object[] args) => new PotionInfo
			{
				Name = (string)args[0],
				BotKeyword = (string)args[1],
				BotText = (string)args[2],
				Id = (ModelId)args[3],
				Rarity = (string)args[4],
				Text = (string)args[5],
				Color = (string)args[6]
			};
			jsonObjectInfoValues.PropertyMetadataInitializer = (JsonSerializerContext _) => PotionInfoPropInit(options);
			jsonObjectInfoValues.ConstructorParameterMetadataInitializer = PotionInfoCtorParamInit;
			jsonObjectInfoValues.ConstructorAttributeProviderFactory = () => typeof(PotionInfo).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null);
			jsonObjectInfoValues.SerializeHandler = PotionInfoSerializeHandler;
			JsonObjectInfoValues<PotionInfo> objectInfo = jsonObjectInfoValues;
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] PotionInfoPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[7];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(PotionInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((PotionInfo)obj).Name;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Name";
		jsonPropertyInfoValues.JsonPropertyName = "name";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(PotionInfo).GetProperty("Name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsRequired = true;
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(PotionInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((PotionInfo)obj).BotKeyword;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "BotKeyword";
		jsonPropertyInfoValues.JsonPropertyName = "bot_keyword";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(PotionInfo).GetProperty("BotKeyword", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo2 = jsonPropertyInfoValues;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsRequired = true;
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(PotionInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((PotionInfo)obj).BotText;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "BotText";
		jsonPropertyInfoValues.JsonPropertyName = "bot_text";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(PotionInfo).GetProperty("BotText", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo3 = jsonPropertyInfoValues;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		array[2].IsRequired = true;
		array[2].IsGetNullable = false;
		array[2].IsSetNullable = false;
		JsonPropertyInfoValues<ModelId> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(PotionInfo);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((PotionInfo)obj).Id;
		jsonPropertyInfoValues2.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "Id";
		jsonPropertyInfoValues2.JsonPropertyName = "id";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(PotionInfo).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModelId), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModelId> propertyInfo4 = jsonPropertyInfoValues2;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		array[3].IsRequired = true;
		array[3].IsGetNullable = false;
		array[3].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(PotionInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((PotionInfo)obj).Rarity;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Rarity";
		jsonPropertyInfoValues.JsonPropertyName = "rarity";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(PotionInfo).GetProperty("Rarity", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo5 = jsonPropertyInfoValues;
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		array[4].IsRequired = true;
		array[4].IsGetNullable = false;
		array[4].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(PotionInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((PotionInfo)obj).Text;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Text";
		jsonPropertyInfoValues.JsonPropertyName = "text";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(PotionInfo).GetProperty("Text", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo6 = jsonPropertyInfoValues;
		array[5] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo6);
		array[5].IsRequired = true;
		array[5].IsGetNullable = false;
		array[5].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(PotionInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((PotionInfo)obj).Color;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Color";
		jsonPropertyInfoValues.JsonPropertyName = "color";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(PotionInfo).GetProperty("Color", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo7 = jsonPropertyInfoValues;
		array[6] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo7);
		array[6].IsRequired = true;
		array[6].IsGetNullable = false;
		array[6].IsSetNullable = false;
		return array;
	}

	private void PotionInfoSerializeHandler(Utf8JsonWriter writer, PotionInfo? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteString(PropName_name, value.Name);
		writer.WriteString(PropName_bot_keyword, value.BotKeyword);
		writer.WriteString(PropName_bot_text, value.BotText);
		writer.WritePropertyName(PropName_id);
		ModelIdSerializeHandler(writer, value.Id);
		writer.WriteString(PropName_rarity, value.Rarity);
		writer.WriteString(PropName_text, value.Text);
		writer.WriteString(PropName_color, value.Color);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] PotionInfoCtorParamInit()
	{
		return new JsonParameterInfoValues[7]
		{
			new JsonParameterInfoValues
			{
				Name = "Name",
				ParameterType = typeof(string),
				Position = 0,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "BotKeyword",
				ParameterType = typeof(string),
				Position = 1,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "BotText",
				ParameterType = typeof(string),
				Position = 2,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Id",
				ParameterType = typeof(ModelId),
				Position = 3,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Rarity",
				ParameterType = typeof(string),
				Position = 4,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Text",
				ParameterType = typeof(string),
				Position = 5,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Color",
				ParameterType = typeof(string),
				Position = 6,
				IsNullable = false,
				IsMemberInitializer = true
			}
		};
	}

	private JsonTypeInfo<RelicInfo> Create_RelicInfo(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<RelicInfo> jsonTypeInfo))
		{
			JsonObjectInfoValues<RelicInfo> jsonObjectInfoValues = new JsonObjectInfoValues<RelicInfo>();
			jsonObjectInfoValues.ObjectCreator = null;
			jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = (object[] args) => new RelicInfo
			{
				Name = (string)args[0],
				BotKeyword = (string)args[1],
				BotText = (string)args[2],
				Id = (ModelId)args[3],
				Rarity = (string)args[4],
				Text = (string)args[5],
				Color = (string)args[6]
			};
			jsonObjectInfoValues.PropertyMetadataInitializer = (JsonSerializerContext _) => RelicInfoPropInit(options);
			jsonObjectInfoValues.ConstructorParameterMetadataInitializer = RelicInfoCtorParamInit;
			jsonObjectInfoValues.ConstructorAttributeProviderFactory = () => typeof(RelicInfo).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null);
			jsonObjectInfoValues.SerializeHandler = RelicInfoSerializeHandler;
			JsonObjectInfoValues<RelicInfo> objectInfo = jsonObjectInfoValues;
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] RelicInfoPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[7];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(RelicInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((RelicInfo)obj).Name;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Name";
		jsonPropertyInfoValues.JsonPropertyName = "name";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(RelicInfo).GetProperty("Name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsRequired = true;
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(RelicInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((RelicInfo)obj).BotKeyword;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "BotKeyword";
		jsonPropertyInfoValues.JsonPropertyName = "bot_keyword";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(RelicInfo).GetProperty("BotKeyword", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo2 = jsonPropertyInfoValues;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsRequired = true;
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(RelicInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((RelicInfo)obj).BotText;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "BotText";
		jsonPropertyInfoValues.JsonPropertyName = "bot_text";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(RelicInfo).GetProperty("BotText", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo3 = jsonPropertyInfoValues;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		array[2].IsRequired = true;
		array[2].IsGetNullable = false;
		array[2].IsSetNullable = false;
		JsonPropertyInfoValues<ModelId> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(RelicInfo);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((RelicInfo)obj).Id;
		jsonPropertyInfoValues2.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "Id";
		jsonPropertyInfoValues2.JsonPropertyName = "id";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(RelicInfo).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModelId), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModelId> propertyInfo4 = jsonPropertyInfoValues2;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		array[3].IsRequired = true;
		array[3].IsGetNullable = false;
		array[3].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(RelicInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((RelicInfo)obj).Rarity;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Rarity";
		jsonPropertyInfoValues.JsonPropertyName = "rarity";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(RelicInfo).GetProperty("Rarity", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo5 = jsonPropertyInfoValues;
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		array[4].IsRequired = true;
		array[4].IsGetNullable = false;
		array[4].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(RelicInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((RelicInfo)obj).Text;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Text";
		jsonPropertyInfoValues.JsonPropertyName = "text";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(RelicInfo).GetProperty("Text", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo6 = jsonPropertyInfoValues;
		array[5] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo6);
		array[5].IsRequired = true;
		array[5].IsGetNullable = false;
		array[5].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(RelicInfo);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((RelicInfo)obj).Color;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Color";
		jsonPropertyInfoValues.JsonPropertyName = "color";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(RelicInfo).GetProperty("Color", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo7 = jsonPropertyInfoValues;
		array[6] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo7);
		array[6].IsRequired = true;
		array[6].IsGetNullable = false;
		array[6].IsSetNullable = false;
		return array;
	}

	private void RelicInfoSerializeHandler(Utf8JsonWriter writer, RelicInfo? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteString(PropName_name, value.Name);
		writer.WriteString(PropName_bot_keyword, value.BotKeyword);
		writer.WriteString(PropName_bot_text, value.BotText);
		writer.WritePropertyName(PropName_id);
		ModelIdSerializeHandler(writer, value.Id);
		writer.WriteString(PropName_rarity, value.Rarity);
		writer.WriteString(PropName_text, value.Text);
		writer.WriteString(PropName_color, value.Color);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] RelicInfoCtorParamInit()
	{
		return new JsonParameterInfoValues[7]
		{
			new JsonParameterInfoValues
			{
				Name = "Name",
				ParameterType = typeof(string),
				Position = 0,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "BotKeyword",
				ParameterType = typeof(string),
				Position = 1,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "BotText",
				ParameterType = typeof(string),
				Position = 2,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Id",
				ParameterType = typeof(ModelId),
				Position = 3,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Rarity",
				ParameterType = typeof(string),
				Position = 4,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Text",
				ParameterType = typeof(string),
				Position = 5,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Color",
				ParameterType = typeof(string),
				Position = 6,
				IsNullable = false,
				IsMemberInitializer = true
			}
		};
	}

	private JsonTypeInfo<List<IGameInfo>> Create_ListIGameInfo(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<IGameInfo>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<IGameInfo>> collectionInfo = new JsonCollectionInfoValues<List<IGameInfo>>
			{
				ObjectCreator = () => new List<IGameInfo>(),
				SerializeHandler = ListIGameInfoSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<IGameInfo>, IGameInfo>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private void ListIGameInfoSerializeHandler(Utf8JsonWriter writer, List<IGameInfo>? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartArray();
		for (int i = 0; i < value.Count; i++)
		{
			JsonSerializer.Serialize(writer, value[i], IGameInfo);
		}
		writer.WriteEndArray();
	}

	private JsonTypeInfo<List<string>> Create_ListString(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<string>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<string>> collectionInfo = new JsonCollectionInfoValues<List<string>>
			{
				ObjectCreator = () => new List<string>(),
				SerializeHandler = ListStringSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<string>, string>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private void ListStringSerializeHandler(Utf8JsonWriter writer, List<string>? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartArray();
		for (int i = 0; i < value.Count; i++)
		{
			writer.WriteStringValue(value[i]);
		}
		writer.WriteEndArray();
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

	public GameInfoUploaderSerializerContext()
		: base(null)
	{
	}

	public GameInfoUploaderSerializerContext(JsonSerializerOptions options)
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
		if (type == typeof(ModelId))
		{
			return Create_ModelId(options);
		}
		if (type == typeof(AncientChoiceInfo))
		{
			return Create_AncientChoiceInfo(options);
		}
		if (type == typeof(CardInfo))
		{
			return Create_CardInfo(options);
		}
		if (type == typeof(DailyMods))
		{
			return Create_DailyMods(options);
		}
		if (type == typeof(EnchantmentInfo))
		{
			return Create_EnchantmentInfo(options);
		}
		if (type == typeof(EncounterInfo))
		{
			return Create_EncounterInfo(options);
		}
		if (type == typeof(MegaCrit.Sts2.GameInfo.Objects.EventInfo))
		{
			return Create_EventInfo(options);
		}
		if (type == typeof(IGameInfo))
		{
			return Create_IGameInfo(options);
		}
		if (type == typeof(Keywords))
		{
			return Create_Keywords(options);
		}
		if (type == typeof(NeowBonusInfo))
		{
			return Create_NeowBonusInfo(options);
		}
		if (type == typeof(PotionInfo))
		{
			return Create_PotionInfo(options);
		}
		if (type == typeof(RelicInfo))
		{
			return Create_RelicInfo(options);
		}
		if (type == typeof(List<IGameInfo>))
		{
			return Create_ListIGameInfo(options);
		}
		if (type == typeof(List<string>))
		{
			return Create_ListString(options);
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
