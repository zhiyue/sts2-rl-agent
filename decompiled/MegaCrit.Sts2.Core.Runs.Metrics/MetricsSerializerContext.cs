using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Godot;
using MegaCrit.Sts2.Core.Achievements;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.Core.Timeline;
using MegaCrit.Sts2.GameInfo;

namespace MegaCrit.Sts2.Core.Runs.Metrics;

[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true, UseStringEnumConverter = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, Converters = new Type[] { typeof(ModelIdMetricsConverter) })]
[JsonSerializable(typeof(RunMetrics))]
[JsonSerializable(typeof(AchievementMetric))]
[JsonSerializable(typeof(EpochMetric))]
[JsonSerializable(typeof(SettingsDataMetric))]
[GeneratedCode("System.Text.Json.SourceGeneration", "9.0.12.31616")]
internal class MetricsSerializerContext : JsonSerializerContext, IJsonTypeInfoResolver
{
	private JsonTypeInfo<bool>? _Boolean;

	private JsonTypeInfo<float>? _Single;

	private JsonTypeInfo<Vector2I>? _Vector2I;

	private JsonTypeInfo<AchievementMetric>? _AchievementMetric;

	private JsonTypeInfo<ModelId>? _ModelId;

	private JsonTypeInfo<ActWinMetric>? _ActWinMetric;

	private JsonTypeInfo<AncientMetric>? _AncientMetric;

	private JsonTypeInfo<CardChoiceMetric>? _CardChoiceMetric;

	private JsonTypeInfo<EncounterMetric>? _EncounterMetric;

	private JsonTypeInfo<EventChoiceMetric>? _EventChoiceMetric;

	private JsonTypeInfo<RunMetrics>? _RunMetrics;

	private JsonTypeInfo<SettingsDataMetric>? _SettingsDataMetric;

	private JsonTypeInfo<AspectRatioSetting>? _AspectRatioSetting;

	private JsonTypeInfo<FastModeType>? _FastModeType;

	private JsonTypeInfo<VSyncType>? _VSyncType;

	private JsonTypeInfo<EpochMetric>? _EpochMetric;

	private JsonTypeInfo<IEnumerable<ModelId>>? _IEnumerableModelId;

	private JsonTypeInfo<List<ModelId>>? _ListModelId;

	private JsonTypeInfo<List<ActWinMetric>>? _ListActWinMetric;

	private JsonTypeInfo<List<AncientMetric>>? _ListAncientMetric;

	private JsonTypeInfo<List<CardChoiceMetric>>? _ListCardChoiceMetric;

	private JsonTypeInfo<List<EncounterMetric>>? _ListEncounterMetric;

	private JsonTypeInfo<List<EventChoiceMetric>>? _ListEventChoiceMetric;

	private JsonTypeInfo<List<string>>? _ListString;

	private JsonTypeInfo<int>? _Int32;

	private JsonTypeInfo<long>? _Int64;

	private JsonTypeInfo<string>? _String;

	private static readonly JsonSerializerOptions s_defaultOptions = new JsonSerializerOptions
	{
		Converters = { (JsonConverter)new ModelIdMetricsConverter() },
		IncludeFields = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = true
	};

	private const BindingFlags InstanceMemberBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private static readonly JsonEncodedText PropName_x = JsonEncodedText.Encode("x");

	private static readonly JsonEncodedText PropName_y = JsonEncodedText.Encode("y");

	private static readonly JsonEncodedText PropName_buildId = JsonEncodedText.Encode("buildId");

	private static readonly JsonEncodedText PropName_achievement = JsonEncodedText.Encode("achievement");

	private static readonly JsonEncodedText PropName_totalAchievements = JsonEncodedText.Encode("totalAchievements");

	private static readonly JsonEncodedText PropName_totalPlaytime = JsonEncodedText.Encode("totalPlaytime");

	private static readonly JsonEncodedText PropName_totalRuns = JsonEncodedText.Encode("totalRuns");

	private static readonly JsonEncodedText PropName_category = JsonEncodedText.Encode("category");

	private static readonly JsonEncodedText PropName_entry = JsonEncodedText.Encode("entry");

	private static readonly JsonEncodedText PropName_act = JsonEncodedText.Encode("act");

	private static readonly JsonEncodedText PropName_win = JsonEncodedText.Encode("win");

	private static readonly JsonEncodedText PropName_picked = JsonEncodedText.Encode("picked");

	private static readonly JsonEncodedText PropName_skipped = JsonEncodedText.Encode("skipped");

	private static readonly JsonEncodedText PropName_id = JsonEncodedText.Encode("id");

	private static readonly JsonEncodedText PropName_damage = JsonEncodedText.Encode("damage");

	private static readonly JsonEncodedText PropName_turns = JsonEncodedText.Encode("turns");

	private static readonly JsonEncodedText PropName_playerId = JsonEncodedText.Encode("playerId");

	private static readonly JsonEncodedText PropName_character = JsonEncodedText.Encode("character");

	private static readonly JsonEncodedText PropName_numPlayers = JsonEncodedText.Encode("numPlayers");

	private static readonly JsonEncodedText PropName_team = JsonEncodedText.Encode("team");

	private static readonly JsonEncodedText PropName_buildType = JsonEncodedText.Encode("buildType");

	private static readonly JsonEncodedText PropName_ascension = JsonEncodedText.Encode("ascension");

	private static readonly JsonEncodedText PropName_totalWinRate = JsonEncodedText.Encode("totalWinRate");

	private static readonly JsonEncodedText PropName_runPlaytime = JsonEncodedText.Encode("runPlaytime");

	private static readonly JsonEncodedText PropName_floorReached = JsonEncodedText.Encode("floorReached");

	private static readonly JsonEncodedText PropName_killedByEncounter = JsonEncodedText.Encode("killedByEncounter");

	private static readonly JsonEncodedText PropName_cardChoices = JsonEncodedText.Encode("cardChoices");

	private static readonly JsonEncodedText PropName_campfireUpgrades = JsonEncodedText.Encode("campfireUpgrades");

	private static readonly JsonEncodedText PropName_eventChoices = JsonEncodedText.Encode("eventChoices");

	private static readonly JsonEncodedText PropName_ancientChoices = JsonEncodedText.Encode("ancientChoices");

	private static readonly JsonEncodedText PropName_relicBuys = JsonEncodedText.Encode("relicBuys");

	private static readonly JsonEncodedText PropName_potionBuys = JsonEncodedText.Encode("potionBuys");

	private static readonly JsonEncodedText PropName_colorlessBuys = JsonEncodedText.Encode("colorlessBuys");

	private static readonly JsonEncodedText PropName_potionDiscards = JsonEncodedText.Encode("potionDiscards");

	private static readonly JsonEncodedText PropName_encounters = JsonEncodedText.Encode("encounters");

	private static readonly JsonEncodedText PropName_actWins = JsonEncodedText.Encode("actWins");

	private static readonly JsonEncodedText PropName_deck = JsonEncodedText.Encode("deck");

	private static readonly JsonEncodedText PropName_relics = JsonEncodedText.Encode("relics");

	private static readonly JsonEncodedText PropName_os = JsonEncodedText.Encode("os");

	private static readonly JsonEncodedText PropName_platform = JsonEncodedText.Encode("platform");

	private static readonly JsonEncodedText PropName_systemRam = JsonEncodedText.Encode("systemRam");

	private static readonly JsonEncodedText PropName_language = JsonEncodedText.Encode("language");

	private static readonly JsonEncodedText PropName_combatSpeed = JsonEncodedText.Encode("combatSpeed");

	private static readonly JsonEncodedText PropName_screenshake = JsonEncodedText.Encode("screenshake");

	private static readonly JsonEncodedText PropName_runTimer = JsonEncodedText.Encode("runTimer");

	private static readonly JsonEncodedText PropName_cardIndices = JsonEncodedText.Encode("cardIndices");

	private static readonly JsonEncodedText PropName_displayCount = JsonEncodedText.Encode("displayCount");

	private static readonly JsonEncodedText PropName_displayResolution = JsonEncodedText.Encode("displayResolution");

	private static readonly JsonEncodedText PropName_fullscreen = JsonEncodedText.Encode("fullscreen");

	private static readonly JsonEncodedText PropName_aspectRatio = JsonEncodedText.Encode("aspectRatio");

	private static readonly JsonEncodedText PropName_resizeWindows = JsonEncodedText.Encode("resizeWindows");

	private static readonly JsonEncodedText PropName_vSync = JsonEncodedText.Encode("vSync");

	private static readonly JsonEncodedText PropName_fpsLimit = JsonEncodedText.Encode("fpsLimit");

	private static readonly JsonEncodedText PropName_msaa = JsonEncodedText.Encode("msaa");

	private static readonly JsonEncodedText PropName_epoch = JsonEncodedText.Encode("epoch");

	private static readonly JsonEncodedText PropName_totalEpochs = JsonEncodedText.Encode("totalEpochs");

	public JsonTypeInfo<bool> Boolean => _Boolean ?? (_Boolean = (JsonTypeInfo<bool>)base.Options.GetTypeInfo(typeof(bool)));

	public JsonTypeInfo<float> Single => _Single ?? (_Single = (JsonTypeInfo<float>)base.Options.GetTypeInfo(typeof(float)));

	public JsonTypeInfo<Vector2I> Vector2I => _Vector2I ?? (_Vector2I = (JsonTypeInfo<Vector2I>)base.Options.GetTypeInfo(typeof(Vector2I)));

	public JsonTypeInfo<AchievementMetric> AchievementMetric => _AchievementMetric ?? (_AchievementMetric = (JsonTypeInfo<AchievementMetric>)base.Options.GetTypeInfo(typeof(AchievementMetric)));

	public JsonTypeInfo<ModelId> ModelId => _ModelId ?? (_ModelId = (JsonTypeInfo<ModelId>)base.Options.GetTypeInfo(typeof(ModelId)));

	public JsonTypeInfo<ActWinMetric> ActWinMetric => _ActWinMetric ?? (_ActWinMetric = (JsonTypeInfo<ActWinMetric>)base.Options.GetTypeInfo(typeof(ActWinMetric)));

	public JsonTypeInfo<AncientMetric> AncientMetric => _AncientMetric ?? (_AncientMetric = (JsonTypeInfo<AncientMetric>)base.Options.GetTypeInfo(typeof(AncientMetric)));

	public JsonTypeInfo<CardChoiceMetric> CardChoiceMetric => _CardChoiceMetric ?? (_CardChoiceMetric = (JsonTypeInfo<CardChoiceMetric>)base.Options.GetTypeInfo(typeof(CardChoiceMetric)));

	public JsonTypeInfo<EncounterMetric> EncounterMetric => _EncounterMetric ?? (_EncounterMetric = (JsonTypeInfo<EncounterMetric>)base.Options.GetTypeInfo(typeof(EncounterMetric)));

	public JsonTypeInfo<EventChoiceMetric> EventChoiceMetric => _EventChoiceMetric ?? (_EventChoiceMetric = (JsonTypeInfo<EventChoiceMetric>)base.Options.GetTypeInfo(typeof(EventChoiceMetric)));

	public JsonTypeInfo<RunMetrics> RunMetrics => _RunMetrics ?? (_RunMetrics = (JsonTypeInfo<RunMetrics>)base.Options.GetTypeInfo(typeof(RunMetrics)));

	public JsonTypeInfo<SettingsDataMetric> SettingsDataMetric => _SettingsDataMetric ?? (_SettingsDataMetric = (JsonTypeInfo<SettingsDataMetric>)base.Options.GetTypeInfo(typeof(SettingsDataMetric)));

	public JsonTypeInfo<AspectRatioSetting> AspectRatioSetting => _AspectRatioSetting ?? (_AspectRatioSetting = (JsonTypeInfo<AspectRatioSetting>)base.Options.GetTypeInfo(typeof(AspectRatioSetting)));

	public JsonTypeInfo<FastModeType> FastModeType => _FastModeType ?? (_FastModeType = (JsonTypeInfo<FastModeType>)base.Options.GetTypeInfo(typeof(FastModeType)));

	public JsonTypeInfo<VSyncType> VSyncType => _VSyncType ?? (_VSyncType = (JsonTypeInfo<VSyncType>)base.Options.GetTypeInfo(typeof(VSyncType)));

	public JsonTypeInfo<EpochMetric> EpochMetric => _EpochMetric ?? (_EpochMetric = (JsonTypeInfo<EpochMetric>)base.Options.GetTypeInfo(typeof(EpochMetric)));

	public JsonTypeInfo<IEnumerable<ModelId>> IEnumerableModelId => _IEnumerableModelId ?? (_IEnumerableModelId = (JsonTypeInfo<IEnumerable<ModelId>>)base.Options.GetTypeInfo(typeof(IEnumerable<ModelId>)));

	public JsonTypeInfo<List<ModelId>> ListModelId => _ListModelId ?? (_ListModelId = (JsonTypeInfo<List<ModelId>>)base.Options.GetTypeInfo(typeof(List<ModelId>)));

	public JsonTypeInfo<List<ActWinMetric>> ListActWinMetric => _ListActWinMetric ?? (_ListActWinMetric = (JsonTypeInfo<List<ActWinMetric>>)base.Options.GetTypeInfo(typeof(List<ActWinMetric>)));

	public JsonTypeInfo<List<AncientMetric>> ListAncientMetric => _ListAncientMetric ?? (_ListAncientMetric = (JsonTypeInfo<List<AncientMetric>>)base.Options.GetTypeInfo(typeof(List<AncientMetric>)));

	public JsonTypeInfo<List<CardChoiceMetric>> ListCardChoiceMetric => _ListCardChoiceMetric ?? (_ListCardChoiceMetric = (JsonTypeInfo<List<CardChoiceMetric>>)base.Options.GetTypeInfo(typeof(List<CardChoiceMetric>)));

	public JsonTypeInfo<List<EncounterMetric>> ListEncounterMetric => _ListEncounterMetric ?? (_ListEncounterMetric = (JsonTypeInfo<List<EncounterMetric>>)base.Options.GetTypeInfo(typeof(List<EncounterMetric>)));

	public JsonTypeInfo<List<EventChoiceMetric>> ListEventChoiceMetric => _ListEventChoiceMetric ?? (_ListEventChoiceMetric = (JsonTypeInfo<List<EventChoiceMetric>>)base.Options.GetTypeInfo(typeof(List<EventChoiceMetric>)));

	public JsonTypeInfo<List<string>> ListString => _ListString ?? (_ListString = (JsonTypeInfo<List<string>>)base.Options.GetTypeInfo(typeof(List<string>)));

	public JsonTypeInfo<int> Int32 => _Int32 ?? (_Int32 = (JsonTypeInfo<int>)base.Options.GetTypeInfo(typeof(int)));

	public JsonTypeInfo<long> Int64 => _Int64 ?? (_Int64 = (JsonTypeInfo<long>)base.Options.GetTypeInfo(typeof(long)));

	public JsonTypeInfo<string> String => _String ?? (_String = (JsonTypeInfo<string>)base.Options.GetTypeInfo(typeof(string)));

	public static MetricsSerializerContext Default { get; } = new MetricsSerializerContext(new JsonSerializerOptions(s_defaultOptions));

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

	private JsonTypeInfo<float> Create_Single(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<float> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<float>(options, JsonMetadataServices.SingleConverter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<Vector2I> Create_Vector2I(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<Vector2I> jsonTypeInfo))
		{
			JsonObjectInfoValues<Vector2I> objectInfo = new JsonObjectInfoValues<Vector2I>
			{
				ObjectCreator = () => default(Vector2I),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => Vector2IPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(Vector2I).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = Vector2ISerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] Vector2IPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<int> jsonPropertyInfoValues = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(Vector2I);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((Vector2I)obj).X;
		jsonPropertyInfoValues.Setter = delegate(object obj, int value)
		{
			Unsafe.Unbox<Vector2I>(obj).X = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "X";
		jsonPropertyInfoValues.JsonPropertyName = null;
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(Vector2I).GetField("X", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<int> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		jsonPropertyInfoValues = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(Vector2I);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((Vector2I)obj).Y;
		jsonPropertyInfoValues.Setter = delegate(object obj, int value)
		{
			Unsafe.Unbox<Vector2I>(obj).Y = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Y";
		jsonPropertyInfoValues.JsonPropertyName = null;
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(Vector2I).GetField("Y", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<int> propertyInfo2 = jsonPropertyInfoValues;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		return array;
	}

	private void Vector2ISerializeHandler(Utf8JsonWriter writer, Vector2I value)
	{
		writer.WriteStartObject();
		writer.WriteNumber(PropName_x, value.X);
		writer.WriteNumber(PropName_y, value.Y);
		writer.WriteEndObject();
	}

	private JsonTypeInfo<AchievementMetric> Create_AchievementMetric(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<AchievementMetric> jsonTypeInfo))
		{
			JsonObjectInfoValues<AchievementMetric> jsonObjectInfoValues = new JsonObjectInfoValues<AchievementMetric>();
			jsonObjectInfoValues.ObjectCreator = null;
			jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = (object[] args) => new AchievementMetric
			{
				BuildId = (string)args[0],
				Achievement = (string)args[1]
			};
			jsonObjectInfoValues.PropertyMetadataInitializer = (JsonSerializerContext _) => AchievementMetricPropInit(options);
			jsonObjectInfoValues.ConstructorParameterMetadataInitializer = AchievementMetricCtorParamInit;
			jsonObjectInfoValues.ConstructorAttributeProviderFactory = () => typeof(AchievementMetric).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null);
			jsonObjectInfoValues.SerializeHandler = AchievementMetricSerializeHandler;
			JsonObjectInfoValues<AchievementMetric> objectInfo = jsonObjectInfoValues;
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] AchievementMetricPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[5];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(AchievementMetric);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((AchievementMetric)obj).BuildId;
		jsonPropertyInfoValues.Setter = delegate(object obj, string? value)
		{
			((AchievementMetric)obj).BuildId = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "BuildId";
		jsonPropertyInfoValues.JsonPropertyName = "buildId";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(AchievementMetric).GetProperty("BuildId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsRequired = true;
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(AchievementMetric);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((AchievementMetric)obj).Achievement;
		jsonPropertyInfoValues.Setter = delegate(object obj, string? value)
		{
			((AchievementMetric)obj).Achievement = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Achievement";
		jsonPropertyInfoValues.JsonPropertyName = "achievement";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(AchievementMetric).GetProperty("Achievement", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo2 = jsonPropertyInfoValues;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsRequired = true;
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		JsonPropertyInfoValues<int> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(AchievementMetric);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((AchievementMetric)obj).TotalAchievements;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((AchievementMetric)obj).TotalAchievements = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "TotalAchievements";
		jsonPropertyInfoValues2.JsonPropertyName = "totalAchievements";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(AchievementMetric).GetProperty("TotalAchievements", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo3 = jsonPropertyInfoValues2;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		JsonPropertyInfoValues<long> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<long>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(AchievementMetric);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((AchievementMetric)obj).TotalPlaytime;
		jsonPropertyInfoValues3.Setter = delegate(object obj, long value)
		{
			((AchievementMetric)obj).TotalPlaytime = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "TotalPlaytime";
		jsonPropertyInfoValues3.JsonPropertyName = "totalPlaytime";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(AchievementMetric).GetProperty("TotalPlaytime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(long), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<long> propertyInfo4 = jsonPropertyInfoValues3;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(AchievementMetric);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((AchievementMetric)obj).TotalRuns;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((AchievementMetric)obj).TotalRuns = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "TotalRuns";
		jsonPropertyInfoValues2.JsonPropertyName = "totalRuns";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(AchievementMetric).GetProperty("TotalRuns", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo5 = jsonPropertyInfoValues2;
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		return array;
	}

	private void AchievementMetricSerializeHandler(Utf8JsonWriter writer, AchievementMetric? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteString(PropName_buildId, value.BuildId);
		writer.WriteString(PropName_achievement, value.Achievement);
		writer.WriteNumber(PropName_totalAchievements, value.TotalAchievements);
		writer.WriteNumber(PropName_totalPlaytime, value.TotalPlaytime);
		writer.WriteNumber(PropName_totalRuns, value.TotalRuns);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] AchievementMetricCtorParamInit()
	{
		return new JsonParameterInfoValues[2]
		{
			new JsonParameterInfoValues
			{
				Name = "BuildId",
				ParameterType = typeof(string),
				Position = 0,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Achievement",
				ParameterType = typeof(string),
				Position = 1,
				IsNullable = false,
				IsMemberInitializer = true
			}
		};
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

	private JsonTypeInfo<ActWinMetric> Create_ActWinMetric(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<ActWinMetric> jsonTypeInfo))
		{
			JsonObjectInfoValues<ActWinMetric> objectInfo = new JsonObjectInfoValues<ActWinMetric>
			{
				ObjectCreator = () => default(ActWinMetric),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => ActWinMetricPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(ActWinMetric).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = ActWinMetricSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] ActWinMetricPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(ActWinMetric);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((ActWinMetric)obj).act;
		jsonPropertyInfoValues.Setter = null;
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "act";
		jsonPropertyInfoValues.JsonPropertyName = null;
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(ActWinMetric).GetField("act", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		JsonPropertyInfoValues<bool> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues2.IsProperty = false;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(ActWinMetric);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((ActWinMetric)obj).win;
		jsonPropertyInfoValues2.Setter = null;
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "win";
		jsonPropertyInfoValues2.JsonPropertyName = null;
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(ActWinMetric).GetField("win", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<bool> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		return array;
	}

	private void ActWinMetricSerializeHandler(Utf8JsonWriter writer, ActWinMetric value)
	{
		writer.WriteStartObject();
		writer.WriteString(PropName_act, value.act);
		writer.WriteBoolean(PropName_win, value.win);
		writer.WriteEndObject();
	}

	private JsonTypeInfo<AncientMetric> Create_AncientMetric(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<AncientMetric> jsonTypeInfo))
		{
			JsonObjectInfoValues<AncientMetric> objectInfo = new JsonObjectInfoValues<AncientMetric>
			{
				ObjectCreator = () => default(AncientMetric),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => AncientMetricPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(AncientMetric).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = AncientMetricSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] AncientMetricPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(AncientMetric);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((AncientMetric)obj).picked;
		jsonPropertyInfoValues.Setter = null;
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "picked";
		jsonPropertyInfoValues.JsonPropertyName = null;
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(AncientMetric).GetField("picked", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		JsonPropertyInfoValues<List<string>> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<List<string>>();
		jsonPropertyInfoValues2.IsProperty = false;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(AncientMetric);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((AncientMetric)obj).skipped;
		jsonPropertyInfoValues2.Setter = null;
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "skipped";
		jsonPropertyInfoValues2.JsonPropertyName = null;
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(AncientMetric).GetField("skipped", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<List<string>> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		return array;
	}

	private void AncientMetricSerializeHandler(Utf8JsonWriter writer, AncientMetric value)
	{
		writer.WriteStartObject();
		writer.WriteString(PropName_picked, value.picked);
		writer.WritePropertyName(PropName_skipped);
		ListStringSerializeHandler(writer, value.skipped);
		writer.WriteEndObject();
	}

	private JsonTypeInfo<CardChoiceMetric> Create_CardChoiceMetric(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<CardChoiceMetric> jsonTypeInfo))
		{
			JsonObjectInfoValues<CardChoiceMetric> objectInfo = new JsonObjectInfoValues<CardChoiceMetric>
			{
				ObjectCreator = () => default(CardChoiceMetric),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => CardChoiceMetricPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(CardChoiceMetric).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = CardChoiceMetricSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] CardChoiceMetricPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<List<string>> jsonPropertyInfoValues = new JsonPropertyInfoValues<List<string>>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(CardChoiceMetric);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((CardChoiceMetric)obj).picked;
		jsonPropertyInfoValues.Setter = null;
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "picked";
		jsonPropertyInfoValues.JsonPropertyName = null;
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(CardChoiceMetric).GetField("picked", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<List<string>> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<List<string>>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(CardChoiceMetric);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((CardChoiceMetric)obj).skipped;
		jsonPropertyInfoValues.Setter = null;
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "skipped";
		jsonPropertyInfoValues.JsonPropertyName = null;
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(CardChoiceMetric).GetField("skipped", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<List<string>> propertyInfo2 = jsonPropertyInfoValues;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		return array;
	}

	private void CardChoiceMetricSerializeHandler(Utf8JsonWriter writer, CardChoiceMetric value)
	{
		writer.WriteStartObject();
		writer.WritePropertyName(PropName_picked);
		ListStringSerializeHandler(writer, value.picked);
		writer.WritePropertyName(PropName_skipped);
		ListStringSerializeHandler(writer, value.skipped);
		writer.WriteEndObject();
	}

	private JsonTypeInfo<EncounterMetric> Create_EncounterMetric(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<EncounterMetric> jsonTypeInfo))
		{
			JsonObjectInfoValues<EncounterMetric> objectInfo = new JsonObjectInfoValues<EncounterMetric>
			{
				ObjectCreator = () => default(EncounterMetric),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => EncounterMetricPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(EncounterMetric).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = EncounterMetricSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] EncounterMetricPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[3];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(EncounterMetric);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((EncounterMetric)obj).id;
		jsonPropertyInfoValues.Setter = null;
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "id";
		jsonPropertyInfoValues.JsonPropertyName = null;
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(EncounterMetric).GetField("id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		JsonPropertyInfoValues<int> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = false;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(EncounterMetric);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((EncounterMetric)obj).damage;
		jsonPropertyInfoValues2.Setter = null;
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "damage";
		jsonPropertyInfoValues2.JsonPropertyName = null;
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(EncounterMetric).GetField("damage", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<int> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = false;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(EncounterMetric);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((EncounterMetric)obj).turns;
		jsonPropertyInfoValues2.Setter = null;
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "turns";
		jsonPropertyInfoValues2.JsonPropertyName = null;
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(EncounterMetric).GetField("turns", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<int> propertyInfo3 = jsonPropertyInfoValues2;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		return array;
	}

	private void EncounterMetricSerializeHandler(Utf8JsonWriter writer, EncounterMetric value)
	{
		writer.WriteStartObject();
		writer.WriteString(PropName_id, value.id);
		writer.WriteNumber(PropName_damage, value.damage);
		writer.WriteNumber(PropName_turns, value.turns);
		writer.WriteEndObject();
	}

	private JsonTypeInfo<EventChoiceMetric> Create_EventChoiceMetric(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<EventChoiceMetric> jsonTypeInfo))
		{
			JsonObjectInfoValues<EventChoiceMetric> objectInfo = new JsonObjectInfoValues<EventChoiceMetric>
			{
				ObjectCreator = () => default(EventChoiceMetric),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => EventChoiceMetricPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(EventChoiceMetric).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = EventChoiceMetricSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] EventChoiceMetricPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(EventChoiceMetric);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((EventChoiceMetric)obj).id;
		jsonPropertyInfoValues.Setter = null;
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "id";
		jsonPropertyInfoValues.JsonPropertyName = null;
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(EventChoiceMetric).GetField("id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(EventChoiceMetric);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((EventChoiceMetric)obj).picked;
		jsonPropertyInfoValues.Setter = null;
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "picked";
		jsonPropertyInfoValues.JsonPropertyName = null;
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(EventChoiceMetric).GetField("picked", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<string> propertyInfo2 = jsonPropertyInfoValues;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		return array;
	}

	private void EventChoiceMetricSerializeHandler(Utf8JsonWriter writer, EventChoiceMetric value)
	{
		writer.WriteStartObject();
		writer.WriteString(PropName_id, value.id);
		writer.WriteString(PropName_picked, value.picked);
		writer.WriteEndObject();
	}

	private JsonTypeInfo<RunMetrics> Create_RunMetrics(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<RunMetrics> jsonTypeInfo))
		{
			JsonObjectInfoValues<RunMetrics> jsonObjectInfoValues = new JsonObjectInfoValues<RunMetrics>();
			jsonObjectInfoValues.ObjectCreator = null;
			jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = (object[] args) => new RunMetrics
			{
				BuildId = (string)args[0],
				PlayerId = (string)args[1],
				Character = (ModelId)args[2],
				Win = (bool)args[3],
				NumPlayers = (int)args[4],
				Team = (List<ModelId>)args[5],
				Ascension = (int)args[6],
				TotalPlaytime = (float)args[7],
				TotalWinRate = (float)args[8],
				RunPlaytime = (float)args[9],
				FloorReached = (int)args[10],
				KilledByEncounter = (ModelId)args[11],
				CardChoices = (List<CardChoiceMetric>)args[12],
				CampfireUpgrades = (List<string>)args[13],
				EventChoices = (List<EventChoiceMetric>)args[14],
				AncientChoices = (List<AncientMetric>)args[15],
				RelicBuys = (List<string>)args[16],
				PotionBuys = (List<string>)args[17],
				ColorlessBuys = (List<string>)args[18],
				PotionDiscards = (List<string>)args[19],
				Encounters = (List<EncounterMetric>)args[20],
				ActWins = (List<ActWinMetric>)args[21],
				Deck = (IEnumerable<ModelId>)args[22],
				Relics = (IEnumerable<ModelId>)args[23]
			};
			jsonObjectInfoValues.PropertyMetadataInitializer = (JsonSerializerContext _) => RunMetricsPropInit(options);
			jsonObjectInfoValues.ConstructorParameterMetadataInitializer = RunMetricsCtorParamInit;
			jsonObjectInfoValues.ConstructorAttributeProviderFactory = () => typeof(RunMetrics).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null);
			jsonObjectInfoValues.SerializeHandler = RunMetricsSerializeHandler;
			JsonObjectInfoValues<RunMetrics> objectInfo = jsonObjectInfoValues;
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] RunMetricsPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[25];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(RunMetrics);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((RunMetrics)obj).BuildId;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "BuildId";
		jsonPropertyInfoValues.JsonPropertyName = null;
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(RunMetrics).GetProperty("BuildId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsRequired = true;
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(RunMetrics);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((RunMetrics)obj).PlayerId;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "PlayerId";
		jsonPropertyInfoValues.JsonPropertyName = null;
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(RunMetrics).GetProperty("PlayerId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo2 = jsonPropertyInfoValues;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsRequired = true;
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		JsonPropertyInfoValues<ModelId> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(RunMetrics);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((RunMetrics)obj).Character;
		jsonPropertyInfoValues2.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "Character";
		jsonPropertyInfoValues2.JsonPropertyName = null;
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(RunMetrics).GetProperty("Character", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModelId), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModelId> propertyInfo3 = jsonPropertyInfoValues2;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		array[2].IsRequired = true;
		array[2].IsGetNullable = false;
		array[2].IsSetNullable = false;
		JsonPropertyInfoValues<bool> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(RunMetrics);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((RunMetrics)obj).Win;
		jsonPropertyInfoValues3.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "Win";
		jsonPropertyInfoValues3.JsonPropertyName = null;
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(RunMetrics).GetProperty("Win", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<bool> propertyInfo4 = jsonPropertyInfoValues3;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		array[3].IsRequired = true;
		JsonPropertyInfoValues<int> jsonPropertyInfoValues4 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues4.IsProperty = true;
		jsonPropertyInfoValues4.IsPublic = true;
		jsonPropertyInfoValues4.IsVirtual = false;
		jsonPropertyInfoValues4.DeclaringType = typeof(RunMetrics);
		jsonPropertyInfoValues4.Converter = null;
		jsonPropertyInfoValues4.Getter = (object obj) => ((RunMetrics)obj).NumPlayers;
		jsonPropertyInfoValues4.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues4.IgnoreCondition = null;
		jsonPropertyInfoValues4.HasJsonInclude = false;
		jsonPropertyInfoValues4.IsExtensionData = false;
		jsonPropertyInfoValues4.NumberHandling = null;
		jsonPropertyInfoValues4.PropertyName = "NumPlayers";
		jsonPropertyInfoValues4.JsonPropertyName = null;
		jsonPropertyInfoValues4.AttributeProviderFactory = () => typeof(RunMetrics).GetProperty("NumPlayers", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo5 = jsonPropertyInfoValues4;
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		array[4].IsRequired = true;
		JsonPropertyInfoValues<List<ModelId>> jsonPropertyInfoValues5 = new JsonPropertyInfoValues<List<ModelId>>();
		jsonPropertyInfoValues5.IsProperty = true;
		jsonPropertyInfoValues5.IsPublic = true;
		jsonPropertyInfoValues5.IsVirtual = false;
		jsonPropertyInfoValues5.DeclaringType = typeof(RunMetrics);
		jsonPropertyInfoValues5.Converter = null;
		jsonPropertyInfoValues5.Getter = (object obj) => ((RunMetrics)obj).Team;
		jsonPropertyInfoValues5.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues5.IgnoreCondition = null;
		jsonPropertyInfoValues5.HasJsonInclude = false;
		jsonPropertyInfoValues5.IsExtensionData = false;
		jsonPropertyInfoValues5.NumberHandling = null;
		jsonPropertyInfoValues5.PropertyName = "Team";
		jsonPropertyInfoValues5.JsonPropertyName = null;
		jsonPropertyInfoValues5.AttributeProviderFactory = () => typeof(RunMetrics).GetProperty("Team", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<ModelId>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<ModelId>> propertyInfo6 = jsonPropertyInfoValues5;
		array[5] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo6);
		array[5].IsRequired = true;
		array[5].IsGetNullable = false;
		array[5].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(RunMetrics);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((RunMetrics)obj).BuildType;
		jsonPropertyInfoValues.Setter = null;
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "BuildType";
		jsonPropertyInfoValues.JsonPropertyName = null;
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(RunMetrics).GetProperty("BuildType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo7 = jsonPropertyInfoValues;
		array[6] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo7);
		array[6].IsGetNullable = false;
		jsonPropertyInfoValues4 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues4.IsProperty = true;
		jsonPropertyInfoValues4.IsPublic = true;
		jsonPropertyInfoValues4.IsVirtual = false;
		jsonPropertyInfoValues4.DeclaringType = typeof(RunMetrics);
		jsonPropertyInfoValues4.Converter = null;
		jsonPropertyInfoValues4.Getter = (object obj) => ((RunMetrics)obj).Ascension;
		jsonPropertyInfoValues4.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues4.IgnoreCondition = null;
		jsonPropertyInfoValues4.HasJsonInclude = false;
		jsonPropertyInfoValues4.IsExtensionData = false;
		jsonPropertyInfoValues4.NumberHandling = null;
		jsonPropertyInfoValues4.PropertyName = "Ascension";
		jsonPropertyInfoValues4.JsonPropertyName = null;
		jsonPropertyInfoValues4.AttributeProviderFactory = () => typeof(RunMetrics).GetProperty("Ascension", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo8 = jsonPropertyInfoValues4;
		array[7] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo8);
		JsonPropertyInfoValues<float> jsonPropertyInfoValues6 = new JsonPropertyInfoValues<float>();
		jsonPropertyInfoValues6.IsProperty = true;
		jsonPropertyInfoValues6.IsPublic = true;
		jsonPropertyInfoValues6.IsVirtual = false;
		jsonPropertyInfoValues6.DeclaringType = typeof(RunMetrics);
		jsonPropertyInfoValues6.Converter = null;
		jsonPropertyInfoValues6.Getter = (object obj) => ((RunMetrics)obj).TotalPlaytime;
		jsonPropertyInfoValues6.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues6.IgnoreCondition = null;
		jsonPropertyInfoValues6.HasJsonInclude = false;
		jsonPropertyInfoValues6.IsExtensionData = false;
		jsonPropertyInfoValues6.NumberHandling = null;
		jsonPropertyInfoValues6.PropertyName = "TotalPlaytime";
		jsonPropertyInfoValues6.JsonPropertyName = null;
		jsonPropertyInfoValues6.AttributeProviderFactory = () => typeof(RunMetrics).GetProperty("TotalPlaytime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(float), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<float> propertyInfo9 = jsonPropertyInfoValues6;
		array[8] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo9);
		array[8].IsRequired = true;
		jsonPropertyInfoValues6 = new JsonPropertyInfoValues<float>();
		jsonPropertyInfoValues6.IsProperty = true;
		jsonPropertyInfoValues6.IsPublic = true;
		jsonPropertyInfoValues6.IsVirtual = false;
		jsonPropertyInfoValues6.DeclaringType = typeof(RunMetrics);
		jsonPropertyInfoValues6.Converter = null;
		jsonPropertyInfoValues6.Getter = (object obj) => ((RunMetrics)obj).TotalWinRate;
		jsonPropertyInfoValues6.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues6.IgnoreCondition = null;
		jsonPropertyInfoValues6.HasJsonInclude = false;
		jsonPropertyInfoValues6.IsExtensionData = false;
		jsonPropertyInfoValues6.NumberHandling = null;
		jsonPropertyInfoValues6.PropertyName = "TotalWinRate";
		jsonPropertyInfoValues6.JsonPropertyName = null;
		jsonPropertyInfoValues6.AttributeProviderFactory = () => typeof(RunMetrics).GetProperty("TotalWinRate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(float), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<float> propertyInfo10 = jsonPropertyInfoValues6;
		array[9] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo10);
		array[9].IsRequired = true;
		jsonPropertyInfoValues6 = new JsonPropertyInfoValues<float>();
		jsonPropertyInfoValues6.IsProperty = true;
		jsonPropertyInfoValues6.IsPublic = true;
		jsonPropertyInfoValues6.IsVirtual = false;
		jsonPropertyInfoValues6.DeclaringType = typeof(RunMetrics);
		jsonPropertyInfoValues6.Converter = null;
		jsonPropertyInfoValues6.Getter = (object obj) => ((RunMetrics)obj).RunPlaytime;
		jsonPropertyInfoValues6.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues6.IgnoreCondition = null;
		jsonPropertyInfoValues6.HasJsonInclude = false;
		jsonPropertyInfoValues6.IsExtensionData = false;
		jsonPropertyInfoValues6.NumberHandling = null;
		jsonPropertyInfoValues6.PropertyName = "RunPlaytime";
		jsonPropertyInfoValues6.JsonPropertyName = null;
		jsonPropertyInfoValues6.AttributeProviderFactory = () => typeof(RunMetrics).GetProperty("RunPlaytime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(float), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<float> propertyInfo11 = jsonPropertyInfoValues6;
		array[10] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo11);
		array[10].IsRequired = true;
		jsonPropertyInfoValues4 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues4.IsProperty = true;
		jsonPropertyInfoValues4.IsPublic = true;
		jsonPropertyInfoValues4.IsVirtual = false;
		jsonPropertyInfoValues4.DeclaringType = typeof(RunMetrics);
		jsonPropertyInfoValues4.Converter = null;
		jsonPropertyInfoValues4.Getter = (object obj) => ((RunMetrics)obj).FloorReached;
		jsonPropertyInfoValues4.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues4.IgnoreCondition = null;
		jsonPropertyInfoValues4.HasJsonInclude = false;
		jsonPropertyInfoValues4.IsExtensionData = false;
		jsonPropertyInfoValues4.NumberHandling = null;
		jsonPropertyInfoValues4.PropertyName = "FloorReached";
		jsonPropertyInfoValues4.JsonPropertyName = null;
		jsonPropertyInfoValues4.AttributeProviderFactory = () => typeof(RunMetrics).GetProperty("FloorReached", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo12 = jsonPropertyInfoValues4;
		array[11] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo12);
		array[11].IsRequired = true;
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(RunMetrics);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((RunMetrics)obj).KilledByEncounter;
		jsonPropertyInfoValues2.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "KilledByEncounter";
		jsonPropertyInfoValues2.JsonPropertyName = null;
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(RunMetrics).GetProperty("KilledByEncounter", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModelId), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModelId> propertyInfo13 = jsonPropertyInfoValues2;
		array[12] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo13);
		array[12].IsRequired = true;
		array[12].IsGetNullable = false;
		array[12].IsSetNullable = false;
		JsonPropertyInfoValues<List<CardChoiceMetric>> jsonPropertyInfoValues7 = new JsonPropertyInfoValues<List<CardChoiceMetric>>();
		jsonPropertyInfoValues7.IsProperty = true;
		jsonPropertyInfoValues7.IsPublic = true;
		jsonPropertyInfoValues7.IsVirtual = false;
		jsonPropertyInfoValues7.DeclaringType = typeof(RunMetrics);
		jsonPropertyInfoValues7.Converter = null;
		jsonPropertyInfoValues7.Getter = (object obj) => ((RunMetrics)obj).CardChoices;
		jsonPropertyInfoValues7.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues7.IgnoreCondition = null;
		jsonPropertyInfoValues7.HasJsonInclude = false;
		jsonPropertyInfoValues7.IsExtensionData = false;
		jsonPropertyInfoValues7.NumberHandling = null;
		jsonPropertyInfoValues7.PropertyName = "CardChoices";
		jsonPropertyInfoValues7.JsonPropertyName = null;
		jsonPropertyInfoValues7.AttributeProviderFactory = () => typeof(RunMetrics).GetProperty("CardChoices", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<CardChoiceMetric>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<CardChoiceMetric>> propertyInfo14 = jsonPropertyInfoValues7;
		array[13] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo14);
		array[13].IsRequired = true;
		array[13].IsGetNullable = false;
		array[13].IsSetNullable = false;
		JsonPropertyInfoValues<List<string>> jsonPropertyInfoValues8 = new JsonPropertyInfoValues<List<string>>();
		jsonPropertyInfoValues8.IsProperty = true;
		jsonPropertyInfoValues8.IsPublic = true;
		jsonPropertyInfoValues8.IsVirtual = false;
		jsonPropertyInfoValues8.DeclaringType = typeof(RunMetrics);
		jsonPropertyInfoValues8.Converter = null;
		jsonPropertyInfoValues8.Getter = (object obj) => ((RunMetrics)obj).CampfireUpgrades;
		jsonPropertyInfoValues8.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues8.IgnoreCondition = null;
		jsonPropertyInfoValues8.HasJsonInclude = false;
		jsonPropertyInfoValues8.IsExtensionData = false;
		jsonPropertyInfoValues8.NumberHandling = null;
		jsonPropertyInfoValues8.PropertyName = "CampfireUpgrades";
		jsonPropertyInfoValues8.JsonPropertyName = null;
		jsonPropertyInfoValues8.AttributeProviderFactory = () => typeof(RunMetrics).GetProperty("CampfireUpgrades", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<string>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<string>> propertyInfo15 = jsonPropertyInfoValues8;
		array[14] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo15);
		array[14].IsRequired = true;
		array[14].IsGetNullable = false;
		array[14].IsSetNullable = false;
		JsonPropertyInfoValues<List<EventChoiceMetric>> jsonPropertyInfoValues9 = new JsonPropertyInfoValues<List<EventChoiceMetric>>();
		jsonPropertyInfoValues9.IsProperty = true;
		jsonPropertyInfoValues9.IsPublic = true;
		jsonPropertyInfoValues9.IsVirtual = false;
		jsonPropertyInfoValues9.DeclaringType = typeof(RunMetrics);
		jsonPropertyInfoValues9.Converter = null;
		jsonPropertyInfoValues9.Getter = (object obj) => ((RunMetrics)obj).EventChoices;
		jsonPropertyInfoValues9.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues9.IgnoreCondition = null;
		jsonPropertyInfoValues9.HasJsonInclude = false;
		jsonPropertyInfoValues9.IsExtensionData = false;
		jsonPropertyInfoValues9.NumberHandling = null;
		jsonPropertyInfoValues9.PropertyName = "EventChoices";
		jsonPropertyInfoValues9.JsonPropertyName = null;
		jsonPropertyInfoValues9.AttributeProviderFactory = () => typeof(RunMetrics).GetProperty("EventChoices", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<EventChoiceMetric>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<EventChoiceMetric>> propertyInfo16 = jsonPropertyInfoValues9;
		array[15] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo16);
		array[15].IsRequired = true;
		array[15].IsGetNullable = false;
		array[15].IsSetNullable = false;
		JsonPropertyInfoValues<List<AncientMetric>> jsonPropertyInfoValues10 = new JsonPropertyInfoValues<List<AncientMetric>>();
		jsonPropertyInfoValues10.IsProperty = true;
		jsonPropertyInfoValues10.IsPublic = true;
		jsonPropertyInfoValues10.IsVirtual = false;
		jsonPropertyInfoValues10.DeclaringType = typeof(RunMetrics);
		jsonPropertyInfoValues10.Converter = null;
		jsonPropertyInfoValues10.Getter = (object obj) => ((RunMetrics)obj).AncientChoices;
		jsonPropertyInfoValues10.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues10.IgnoreCondition = null;
		jsonPropertyInfoValues10.HasJsonInclude = false;
		jsonPropertyInfoValues10.IsExtensionData = false;
		jsonPropertyInfoValues10.NumberHandling = null;
		jsonPropertyInfoValues10.PropertyName = "AncientChoices";
		jsonPropertyInfoValues10.JsonPropertyName = null;
		jsonPropertyInfoValues10.AttributeProviderFactory = () => typeof(RunMetrics).GetProperty("AncientChoices", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<AncientMetric>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<AncientMetric>> propertyInfo17 = jsonPropertyInfoValues10;
		array[16] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo17);
		array[16].IsRequired = true;
		array[16].IsGetNullable = false;
		array[16].IsSetNullable = false;
		jsonPropertyInfoValues8 = new JsonPropertyInfoValues<List<string>>();
		jsonPropertyInfoValues8.IsProperty = true;
		jsonPropertyInfoValues8.IsPublic = true;
		jsonPropertyInfoValues8.IsVirtual = false;
		jsonPropertyInfoValues8.DeclaringType = typeof(RunMetrics);
		jsonPropertyInfoValues8.Converter = null;
		jsonPropertyInfoValues8.Getter = (object obj) => ((RunMetrics)obj).RelicBuys;
		jsonPropertyInfoValues8.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues8.IgnoreCondition = null;
		jsonPropertyInfoValues8.HasJsonInclude = false;
		jsonPropertyInfoValues8.IsExtensionData = false;
		jsonPropertyInfoValues8.NumberHandling = null;
		jsonPropertyInfoValues8.PropertyName = "RelicBuys";
		jsonPropertyInfoValues8.JsonPropertyName = null;
		jsonPropertyInfoValues8.AttributeProviderFactory = () => typeof(RunMetrics).GetProperty("RelicBuys", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<string>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<string>> propertyInfo18 = jsonPropertyInfoValues8;
		array[17] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo18);
		array[17].IsRequired = true;
		array[17].IsGetNullable = false;
		array[17].IsSetNullable = false;
		jsonPropertyInfoValues8 = new JsonPropertyInfoValues<List<string>>();
		jsonPropertyInfoValues8.IsProperty = true;
		jsonPropertyInfoValues8.IsPublic = true;
		jsonPropertyInfoValues8.IsVirtual = false;
		jsonPropertyInfoValues8.DeclaringType = typeof(RunMetrics);
		jsonPropertyInfoValues8.Converter = null;
		jsonPropertyInfoValues8.Getter = (object obj) => ((RunMetrics)obj).PotionBuys;
		jsonPropertyInfoValues8.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues8.IgnoreCondition = null;
		jsonPropertyInfoValues8.HasJsonInclude = false;
		jsonPropertyInfoValues8.IsExtensionData = false;
		jsonPropertyInfoValues8.NumberHandling = null;
		jsonPropertyInfoValues8.PropertyName = "PotionBuys";
		jsonPropertyInfoValues8.JsonPropertyName = null;
		jsonPropertyInfoValues8.AttributeProviderFactory = () => typeof(RunMetrics).GetProperty("PotionBuys", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<string>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<string>> propertyInfo19 = jsonPropertyInfoValues8;
		array[18] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo19);
		array[18].IsRequired = true;
		array[18].IsGetNullable = false;
		array[18].IsSetNullable = false;
		jsonPropertyInfoValues8 = new JsonPropertyInfoValues<List<string>>();
		jsonPropertyInfoValues8.IsProperty = true;
		jsonPropertyInfoValues8.IsPublic = true;
		jsonPropertyInfoValues8.IsVirtual = false;
		jsonPropertyInfoValues8.DeclaringType = typeof(RunMetrics);
		jsonPropertyInfoValues8.Converter = null;
		jsonPropertyInfoValues8.Getter = (object obj) => ((RunMetrics)obj).ColorlessBuys;
		jsonPropertyInfoValues8.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues8.IgnoreCondition = null;
		jsonPropertyInfoValues8.HasJsonInclude = false;
		jsonPropertyInfoValues8.IsExtensionData = false;
		jsonPropertyInfoValues8.NumberHandling = null;
		jsonPropertyInfoValues8.PropertyName = "ColorlessBuys";
		jsonPropertyInfoValues8.JsonPropertyName = null;
		jsonPropertyInfoValues8.AttributeProviderFactory = () => typeof(RunMetrics).GetProperty("ColorlessBuys", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<string>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<string>> propertyInfo20 = jsonPropertyInfoValues8;
		array[19] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo20);
		array[19].IsRequired = true;
		array[19].IsGetNullable = false;
		array[19].IsSetNullable = false;
		jsonPropertyInfoValues8 = new JsonPropertyInfoValues<List<string>>();
		jsonPropertyInfoValues8.IsProperty = true;
		jsonPropertyInfoValues8.IsPublic = true;
		jsonPropertyInfoValues8.IsVirtual = false;
		jsonPropertyInfoValues8.DeclaringType = typeof(RunMetrics);
		jsonPropertyInfoValues8.Converter = null;
		jsonPropertyInfoValues8.Getter = (object obj) => ((RunMetrics)obj).PotionDiscards;
		jsonPropertyInfoValues8.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues8.IgnoreCondition = null;
		jsonPropertyInfoValues8.HasJsonInclude = false;
		jsonPropertyInfoValues8.IsExtensionData = false;
		jsonPropertyInfoValues8.NumberHandling = null;
		jsonPropertyInfoValues8.PropertyName = "PotionDiscards";
		jsonPropertyInfoValues8.JsonPropertyName = null;
		jsonPropertyInfoValues8.AttributeProviderFactory = () => typeof(RunMetrics).GetProperty("PotionDiscards", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<string>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<string>> propertyInfo21 = jsonPropertyInfoValues8;
		array[20] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo21);
		array[20].IsRequired = true;
		array[20].IsGetNullable = false;
		array[20].IsSetNullable = false;
		JsonPropertyInfoValues<List<EncounterMetric>> jsonPropertyInfoValues11 = new JsonPropertyInfoValues<List<EncounterMetric>>();
		jsonPropertyInfoValues11.IsProperty = true;
		jsonPropertyInfoValues11.IsPublic = true;
		jsonPropertyInfoValues11.IsVirtual = false;
		jsonPropertyInfoValues11.DeclaringType = typeof(RunMetrics);
		jsonPropertyInfoValues11.Converter = null;
		jsonPropertyInfoValues11.Getter = (object obj) => ((RunMetrics)obj).Encounters;
		jsonPropertyInfoValues11.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues11.IgnoreCondition = null;
		jsonPropertyInfoValues11.HasJsonInclude = false;
		jsonPropertyInfoValues11.IsExtensionData = false;
		jsonPropertyInfoValues11.NumberHandling = null;
		jsonPropertyInfoValues11.PropertyName = "Encounters";
		jsonPropertyInfoValues11.JsonPropertyName = null;
		jsonPropertyInfoValues11.AttributeProviderFactory = () => typeof(RunMetrics).GetProperty("Encounters", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<EncounterMetric>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<EncounterMetric>> propertyInfo22 = jsonPropertyInfoValues11;
		array[21] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo22);
		array[21].IsRequired = true;
		array[21].IsGetNullable = false;
		array[21].IsSetNullable = false;
		JsonPropertyInfoValues<List<ActWinMetric>> jsonPropertyInfoValues12 = new JsonPropertyInfoValues<List<ActWinMetric>>();
		jsonPropertyInfoValues12.IsProperty = true;
		jsonPropertyInfoValues12.IsPublic = true;
		jsonPropertyInfoValues12.IsVirtual = false;
		jsonPropertyInfoValues12.DeclaringType = typeof(RunMetrics);
		jsonPropertyInfoValues12.Converter = null;
		jsonPropertyInfoValues12.Getter = (object obj) => ((RunMetrics)obj).ActWins;
		jsonPropertyInfoValues12.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues12.IgnoreCondition = null;
		jsonPropertyInfoValues12.HasJsonInclude = false;
		jsonPropertyInfoValues12.IsExtensionData = false;
		jsonPropertyInfoValues12.NumberHandling = null;
		jsonPropertyInfoValues12.PropertyName = "ActWins";
		jsonPropertyInfoValues12.JsonPropertyName = null;
		jsonPropertyInfoValues12.AttributeProviderFactory = () => typeof(RunMetrics).GetProperty("ActWins", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<ActWinMetric>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<ActWinMetric>> propertyInfo23 = jsonPropertyInfoValues12;
		array[22] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo23);
		array[22].IsRequired = true;
		array[22].IsGetNullable = false;
		array[22].IsSetNullable = false;
		JsonPropertyInfoValues<IEnumerable<ModelId>> jsonPropertyInfoValues13 = new JsonPropertyInfoValues<IEnumerable<ModelId>>();
		jsonPropertyInfoValues13.IsProperty = true;
		jsonPropertyInfoValues13.IsPublic = true;
		jsonPropertyInfoValues13.IsVirtual = false;
		jsonPropertyInfoValues13.DeclaringType = typeof(RunMetrics);
		jsonPropertyInfoValues13.Converter = null;
		jsonPropertyInfoValues13.Getter = (object obj) => ((RunMetrics)obj).Deck;
		jsonPropertyInfoValues13.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues13.IgnoreCondition = null;
		jsonPropertyInfoValues13.HasJsonInclude = false;
		jsonPropertyInfoValues13.IsExtensionData = false;
		jsonPropertyInfoValues13.NumberHandling = null;
		jsonPropertyInfoValues13.PropertyName = "Deck";
		jsonPropertyInfoValues13.JsonPropertyName = null;
		jsonPropertyInfoValues13.AttributeProviderFactory = () => typeof(RunMetrics).GetProperty("Deck", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(IEnumerable<ModelId>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<IEnumerable<ModelId>> propertyInfo24 = jsonPropertyInfoValues13;
		array[23] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo24);
		array[23].IsRequired = true;
		array[23].IsGetNullable = false;
		array[23].IsSetNullable = false;
		jsonPropertyInfoValues13 = new JsonPropertyInfoValues<IEnumerable<ModelId>>();
		jsonPropertyInfoValues13.IsProperty = true;
		jsonPropertyInfoValues13.IsPublic = true;
		jsonPropertyInfoValues13.IsVirtual = false;
		jsonPropertyInfoValues13.DeclaringType = typeof(RunMetrics);
		jsonPropertyInfoValues13.Converter = null;
		jsonPropertyInfoValues13.Getter = (object obj) => ((RunMetrics)obj).Relics;
		jsonPropertyInfoValues13.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues13.IgnoreCondition = null;
		jsonPropertyInfoValues13.HasJsonInclude = false;
		jsonPropertyInfoValues13.IsExtensionData = false;
		jsonPropertyInfoValues13.NumberHandling = null;
		jsonPropertyInfoValues13.PropertyName = "Relics";
		jsonPropertyInfoValues13.JsonPropertyName = null;
		jsonPropertyInfoValues13.AttributeProviderFactory = () => typeof(RunMetrics).GetProperty("Relics", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(IEnumerable<ModelId>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<IEnumerable<ModelId>> propertyInfo25 = jsonPropertyInfoValues13;
		array[24] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo25);
		array[24].IsRequired = true;
		array[24].IsGetNullable = false;
		array[24].IsSetNullable = false;
		return array;
	}

	private void RunMetricsSerializeHandler(Utf8JsonWriter writer, RunMetrics? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteString(PropName_buildId, value.BuildId);
		writer.WriteString(PropName_playerId, value.PlayerId);
		writer.WritePropertyName(PropName_character);
		ModelIdSerializeHandler(writer, value.Character);
		writer.WriteBoolean(PropName_win, value.Win);
		writer.WriteNumber(PropName_numPlayers, value.NumPlayers);
		writer.WritePropertyName(PropName_team);
		ListModelIdSerializeHandler(writer, value.Team);
		writer.WriteString(PropName_buildType, value.BuildType);
		writer.WriteNumber(PropName_ascension, value.Ascension);
		writer.WriteNumber(PropName_totalPlaytime, value.TotalPlaytime);
		writer.WriteNumber(PropName_totalWinRate, value.TotalWinRate);
		writer.WriteNumber(PropName_runPlaytime, value.RunPlaytime);
		writer.WriteNumber(PropName_floorReached, value.FloorReached);
		writer.WritePropertyName(PropName_killedByEncounter);
		ModelIdSerializeHandler(writer, value.KilledByEncounter);
		writer.WritePropertyName(PropName_cardChoices);
		ListCardChoiceMetricSerializeHandler(writer, value.CardChoices);
		writer.WritePropertyName(PropName_campfireUpgrades);
		ListStringSerializeHandler(writer, value.CampfireUpgrades);
		writer.WritePropertyName(PropName_eventChoices);
		ListEventChoiceMetricSerializeHandler(writer, value.EventChoices);
		writer.WritePropertyName(PropName_ancientChoices);
		ListAncientMetricSerializeHandler(writer, value.AncientChoices);
		writer.WritePropertyName(PropName_relicBuys);
		ListStringSerializeHandler(writer, value.RelicBuys);
		writer.WritePropertyName(PropName_potionBuys);
		ListStringSerializeHandler(writer, value.PotionBuys);
		writer.WritePropertyName(PropName_colorlessBuys);
		ListStringSerializeHandler(writer, value.ColorlessBuys);
		writer.WritePropertyName(PropName_potionDiscards);
		ListStringSerializeHandler(writer, value.PotionDiscards);
		writer.WritePropertyName(PropName_encounters);
		ListEncounterMetricSerializeHandler(writer, value.Encounters);
		writer.WritePropertyName(PropName_actWins);
		ListActWinMetricSerializeHandler(writer, value.ActWins);
		writer.WritePropertyName(PropName_deck);
		IEnumerableModelIdSerializeHandler(writer, value.Deck);
		writer.WritePropertyName(PropName_relics);
		IEnumerableModelIdSerializeHandler(writer, value.Relics);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] RunMetricsCtorParamInit()
	{
		return new JsonParameterInfoValues[24]
		{
			new JsonParameterInfoValues
			{
				Name = "BuildId",
				ParameterType = typeof(string),
				Position = 0,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "PlayerId",
				ParameterType = typeof(string),
				Position = 1,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Character",
				ParameterType = typeof(ModelId),
				Position = 2,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Win",
				ParameterType = typeof(bool),
				Position = 3,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "NumPlayers",
				ParameterType = typeof(int),
				Position = 4,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Team",
				ParameterType = typeof(List<ModelId>),
				Position = 5,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Ascension",
				ParameterType = typeof(int),
				Position = 6,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "TotalPlaytime",
				ParameterType = typeof(float),
				Position = 7,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "TotalWinRate",
				ParameterType = typeof(float),
				Position = 8,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "RunPlaytime",
				ParameterType = typeof(float),
				Position = 9,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "FloorReached",
				ParameterType = typeof(int),
				Position = 10,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "KilledByEncounter",
				ParameterType = typeof(ModelId),
				Position = 11,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "CardChoices",
				ParameterType = typeof(List<CardChoiceMetric>),
				Position = 12,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "CampfireUpgrades",
				ParameterType = typeof(List<string>),
				Position = 13,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "EventChoices",
				ParameterType = typeof(List<EventChoiceMetric>),
				Position = 14,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "AncientChoices",
				ParameterType = typeof(List<AncientMetric>),
				Position = 15,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "RelicBuys",
				ParameterType = typeof(List<string>),
				Position = 16,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "PotionBuys",
				ParameterType = typeof(List<string>),
				Position = 17,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "ColorlessBuys",
				ParameterType = typeof(List<string>),
				Position = 18,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "PotionDiscards",
				ParameterType = typeof(List<string>),
				Position = 19,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Encounters",
				ParameterType = typeof(List<EncounterMetric>),
				Position = 20,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "ActWins",
				ParameterType = typeof(List<ActWinMetric>),
				Position = 21,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Deck",
				ParameterType = typeof(IEnumerable<ModelId>),
				Position = 22,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Relics",
				ParameterType = typeof(IEnumerable<ModelId>),
				Position = 23,
				IsNullable = false,
				IsMemberInitializer = true
			}
		};
	}

	private JsonTypeInfo<SettingsDataMetric> Create_SettingsDataMetric(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SettingsDataMetric> jsonTypeInfo))
		{
			JsonObjectInfoValues<SettingsDataMetric> jsonObjectInfoValues = new JsonObjectInfoValues<SettingsDataMetric>();
			jsonObjectInfoValues.ObjectCreator = null;
			jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = (object[] args) => new SettingsDataMetric
			{
				BuildId = (string)args[0],
				FastModeType = (FastModeType)args[1]
			};
			jsonObjectInfoValues.PropertyMetadataInitializer = (JsonSerializerContext _) => SettingsDataMetricPropInit(options);
			jsonObjectInfoValues.ConstructorParameterMetadataInitializer = SettingsDataMetricCtorParamInit;
			jsonObjectInfoValues.ConstructorAttributeProviderFactory = () => typeof(SettingsDataMetric).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null);
			jsonObjectInfoValues.SerializeHandler = SettingsDataMetricSerializeHandler;
			JsonObjectInfoValues<SettingsDataMetric> objectInfo = jsonObjectInfoValues;
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SettingsDataMetricPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[17];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SettingsDataMetric);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SettingsDataMetric)obj).BuildId;
		jsonPropertyInfoValues.Setter = delegate(object obj, string? value)
		{
			Unsafe.Unbox<SettingsDataMetric>(obj).BuildId = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "BuildId";
		jsonPropertyInfoValues.JsonPropertyName = "buildId";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SettingsDataMetric).GetProperty("BuildId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsRequired = true;
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SettingsDataMetric);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SettingsDataMetric)obj).Os;
		jsonPropertyInfoValues.Setter = delegate(object obj, string? value)
		{
			Unsafe.Unbox<SettingsDataMetric>(obj).Os = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Os";
		jsonPropertyInfoValues.JsonPropertyName = "os";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SettingsDataMetric).GetProperty("Os", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo2 = jsonPropertyInfoValues;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SettingsDataMetric);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SettingsDataMetric)obj).Platform;
		jsonPropertyInfoValues.Setter = delegate(object obj, string? value)
		{
			Unsafe.Unbox<SettingsDataMetric>(obj).Platform = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Platform";
		jsonPropertyInfoValues.JsonPropertyName = "platform";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SettingsDataMetric).GetProperty("Platform", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo3 = jsonPropertyInfoValues;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		array[2].IsGetNullable = false;
		array[2].IsSetNullable = false;
		JsonPropertyInfoValues<int> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SettingsDataMetric);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SettingsDataMetric)obj).SystemRam;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			Unsafe.Unbox<SettingsDataMetric>(obj).SystemRam = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "SystemRam";
		jsonPropertyInfoValues2.JsonPropertyName = "systemRam";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SettingsDataMetric).GetProperty("SystemRam", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo4 = jsonPropertyInfoValues2;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SettingsDataMetric);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SettingsDataMetric)obj).LanguageCode;
		jsonPropertyInfoValues.Setter = delegate(object obj, string? value)
		{
			Unsafe.Unbox<SettingsDataMetric>(obj).LanguageCode = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "LanguageCode";
		jsonPropertyInfoValues.JsonPropertyName = "language";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SettingsDataMetric).GetProperty("LanguageCode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo5 = jsonPropertyInfoValues;
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		array[4].IsGetNullable = false;
		array[4].IsSetNullable = false;
		JsonPropertyInfoValues<FastModeType> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<FastModeType>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(SettingsDataMetric);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((SettingsDataMetric)obj).FastModeType;
		jsonPropertyInfoValues3.Setter = delegate(object obj, FastModeType value)
		{
			Unsafe.Unbox<SettingsDataMetric>(obj).FastModeType = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "FastModeType";
		jsonPropertyInfoValues3.JsonPropertyName = "combatSpeed";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(SettingsDataMetric).GetProperty("FastModeType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(FastModeType), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<FastModeType> propertyInfo6 = jsonPropertyInfoValues3;
		array[5] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo6);
		array[5].IsRequired = true;
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SettingsDataMetric);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SettingsDataMetric)obj).Screenshake;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			Unsafe.Unbox<SettingsDataMetric>(obj).Screenshake = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "Screenshake";
		jsonPropertyInfoValues2.JsonPropertyName = "screenshake";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SettingsDataMetric).GetProperty("Screenshake", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo7 = jsonPropertyInfoValues2;
		array[6] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo7);
		JsonPropertyInfoValues<bool> jsonPropertyInfoValues4 = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues4.IsProperty = true;
		jsonPropertyInfoValues4.IsPublic = true;
		jsonPropertyInfoValues4.IsVirtual = false;
		jsonPropertyInfoValues4.DeclaringType = typeof(SettingsDataMetric);
		jsonPropertyInfoValues4.Converter = null;
		jsonPropertyInfoValues4.Getter = (object obj) => ((SettingsDataMetric)obj).ShowRunTimer;
		jsonPropertyInfoValues4.Setter = delegate(object obj, bool value)
		{
			Unsafe.Unbox<SettingsDataMetric>(obj).ShowRunTimer = value;
		};
		jsonPropertyInfoValues4.IgnoreCondition = null;
		jsonPropertyInfoValues4.HasJsonInclude = false;
		jsonPropertyInfoValues4.IsExtensionData = false;
		jsonPropertyInfoValues4.NumberHandling = null;
		jsonPropertyInfoValues4.PropertyName = "ShowRunTimer";
		jsonPropertyInfoValues4.JsonPropertyName = "runTimer";
		jsonPropertyInfoValues4.AttributeProviderFactory = () => typeof(SettingsDataMetric).GetProperty("ShowRunTimer", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<bool> propertyInfo8 = jsonPropertyInfoValues4;
		array[7] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo8);
		jsonPropertyInfoValues4 = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues4.IsProperty = true;
		jsonPropertyInfoValues4.IsPublic = true;
		jsonPropertyInfoValues4.IsVirtual = false;
		jsonPropertyInfoValues4.DeclaringType = typeof(SettingsDataMetric);
		jsonPropertyInfoValues4.Converter = null;
		jsonPropertyInfoValues4.Getter = (object obj) => ((SettingsDataMetric)obj).ShowCardIndices;
		jsonPropertyInfoValues4.Setter = delegate(object obj, bool value)
		{
			Unsafe.Unbox<SettingsDataMetric>(obj).ShowCardIndices = value;
		};
		jsonPropertyInfoValues4.IgnoreCondition = null;
		jsonPropertyInfoValues4.HasJsonInclude = false;
		jsonPropertyInfoValues4.IsExtensionData = false;
		jsonPropertyInfoValues4.NumberHandling = null;
		jsonPropertyInfoValues4.PropertyName = "ShowCardIndices";
		jsonPropertyInfoValues4.JsonPropertyName = "cardIndices";
		jsonPropertyInfoValues4.AttributeProviderFactory = () => typeof(SettingsDataMetric).GetProperty("ShowCardIndices", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<bool> propertyInfo9 = jsonPropertyInfoValues4;
		array[8] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo9);
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SettingsDataMetric);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SettingsDataMetric)obj).DisplayCount;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			Unsafe.Unbox<SettingsDataMetric>(obj).DisplayCount = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "DisplayCount";
		jsonPropertyInfoValues2.JsonPropertyName = "displayCount";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SettingsDataMetric).GetProperty("DisplayCount", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo10 = jsonPropertyInfoValues2;
		array[9] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo10);
		JsonPropertyInfoValues<Vector2I> jsonPropertyInfoValues5 = new JsonPropertyInfoValues<Vector2I>();
		jsonPropertyInfoValues5.IsProperty = true;
		jsonPropertyInfoValues5.IsPublic = true;
		jsonPropertyInfoValues5.IsVirtual = false;
		jsonPropertyInfoValues5.DeclaringType = typeof(SettingsDataMetric);
		jsonPropertyInfoValues5.Converter = null;
		jsonPropertyInfoValues5.Getter = (object obj) => ((SettingsDataMetric)obj).DisplayResolution;
		jsonPropertyInfoValues5.Setter = delegate(object obj, Vector2I value)
		{
			Unsafe.Unbox<SettingsDataMetric>(obj).DisplayResolution = value;
		};
		jsonPropertyInfoValues5.IgnoreCondition = null;
		jsonPropertyInfoValues5.HasJsonInclude = false;
		jsonPropertyInfoValues5.IsExtensionData = false;
		jsonPropertyInfoValues5.NumberHandling = null;
		jsonPropertyInfoValues5.PropertyName = "DisplayResolution";
		jsonPropertyInfoValues5.JsonPropertyName = "displayResolution";
		jsonPropertyInfoValues5.AttributeProviderFactory = () => typeof(SettingsDataMetric).GetProperty("DisplayResolution", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(Vector2I), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<Vector2I> propertyInfo11 = jsonPropertyInfoValues5;
		array[10] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo11);
		jsonPropertyInfoValues4 = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues4.IsProperty = true;
		jsonPropertyInfoValues4.IsPublic = true;
		jsonPropertyInfoValues4.IsVirtual = false;
		jsonPropertyInfoValues4.DeclaringType = typeof(SettingsDataMetric);
		jsonPropertyInfoValues4.Converter = null;
		jsonPropertyInfoValues4.Getter = (object obj) => ((SettingsDataMetric)obj).Fullscreen;
		jsonPropertyInfoValues4.Setter = delegate(object obj, bool value)
		{
			Unsafe.Unbox<SettingsDataMetric>(obj).Fullscreen = value;
		};
		jsonPropertyInfoValues4.IgnoreCondition = null;
		jsonPropertyInfoValues4.HasJsonInclude = false;
		jsonPropertyInfoValues4.IsExtensionData = false;
		jsonPropertyInfoValues4.NumberHandling = null;
		jsonPropertyInfoValues4.PropertyName = "Fullscreen";
		jsonPropertyInfoValues4.JsonPropertyName = "fullscreen";
		jsonPropertyInfoValues4.AttributeProviderFactory = () => typeof(SettingsDataMetric).GetProperty("Fullscreen", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<bool> propertyInfo12 = jsonPropertyInfoValues4;
		array[11] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo12);
		JsonPropertyInfoValues<AspectRatioSetting> jsonPropertyInfoValues6 = new JsonPropertyInfoValues<AspectRatioSetting>();
		jsonPropertyInfoValues6.IsProperty = true;
		jsonPropertyInfoValues6.IsPublic = true;
		jsonPropertyInfoValues6.IsVirtual = false;
		jsonPropertyInfoValues6.DeclaringType = typeof(SettingsDataMetric);
		jsonPropertyInfoValues6.Converter = null;
		jsonPropertyInfoValues6.Getter = (object obj) => ((SettingsDataMetric)obj).AspectRatio;
		jsonPropertyInfoValues6.Setter = delegate(object obj, AspectRatioSetting value)
		{
			Unsafe.Unbox<SettingsDataMetric>(obj).AspectRatio = value;
		};
		jsonPropertyInfoValues6.IgnoreCondition = null;
		jsonPropertyInfoValues6.HasJsonInclude = false;
		jsonPropertyInfoValues6.IsExtensionData = false;
		jsonPropertyInfoValues6.NumberHandling = null;
		jsonPropertyInfoValues6.PropertyName = "AspectRatio";
		jsonPropertyInfoValues6.JsonPropertyName = "aspectRatio";
		jsonPropertyInfoValues6.AttributeProviderFactory = () => typeof(SettingsDataMetric).GetProperty("AspectRatio", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(AspectRatioSetting), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<AspectRatioSetting> propertyInfo13 = jsonPropertyInfoValues6;
		array[12] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo13);
		jsonPropertyInfoValues4 = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues4.IsProperty = true;
		jsonPropertyInfoValues4.IsPublic = true;
		jsonPropertyInfoValues4.IsVirtual = false;
		jsonPropertyInfoValues4.DeclaringType = typeof(SettingsDataMetric);
		jsonPropertyInfoValues4.Converter = null;
		jsonPropertyInfoValues4.Getter = (object obj) => ((SettingsDataMetric)obj).ResizeWindows;
		jsonPropertyInfoValues4.Setter = delegate(object obj, bool value)
		{
			Unsafe.Unbox<SettingsDataMetric>(obj).ResizeWindows = value;
		};
		jsonPropertyInfoValues4.IgnoreCondition = null;
		jsonPropertyInfoValues4.HasJsonInclude = false;
		jsonPropertyInfoValues4.IsExtensionData = false;
		jsonPropertyInfoValues4.NumberHandling = null;
		jsonPropertyInfoValues4.PropertyName = "ResizeWindows";
		jsonPropertyInfoValues4.JsonPropertyName = "resizeWindows";
		jsonPropertyInfoValues4.AttributeProviderFactory = () => typeof(SettingsDataMetric).GetProperty("ResizeWindows", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<bool> propertyInfo14 = jsonPropertyInfoValues4;
		array[13] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo14);
		JsonPropertyInfoValues<VSyncType> jsonPropertyInfoValues7 = new JsonPropertyInfoValues<VSyncType>();
		jsonPropertyInfoValues7.IsProperty = true;
		jsonPropertyInfoValues7.IsPublic = true;
		jsonPropertyInfoValues7.IsVirtual = false;
		jsonPropertyInfoValues7.DeclaringType = typeof(SettingsDataMetric);
		jsonPropertyInfoValues7.Converter = null;
		jsonPropertyInfoValues7.Getter = (object obj) => ((SettingsDataMetric)obj).VSync;
		jsonPropertyInfoValues7.Setter = delegate(object obj, VSyncType value)
		{
			Unsafe.Unbox<SettingsDataMetric>(obj).VSync = value;
		};
		jsonPropertyInfoValues7.IgnoreCondition = null;
		jsonPropertyInfoValues7.HasJsonInclude = false;
		jsonPropertyInfoValues7.IsExtensionData = false;
		jsonPropertyInfoValues7.NumberHandling = null;
		jsonPropertyInfoValues7.PropertyName = "VSync";
		jsonPropertyInfoValues7.JsonPropertyName = "vSync";
		jsonPropertyInfoValues7.AttributeProviderFactory = () => typeof(SettingsDataMetric).GetProperty("VSync", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(VSyncType), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<VSyncType> propertyInfo15 = jsonPropertyInfoValues7;
		array[14] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo15);
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SettingsDataMetric);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SettingsDataMetric)obj).FpsLimit;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			Unsafe.Unbox<SettingsDataMetric>(obj).FpsLimit = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "FpsLimit";
		jsonPropertyInfoValues2.JsonPropertyName = "fpsLimit";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SettingsDataMetric).GetProperty("FpsLimit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo16 = jsonPropertyInfoValues2;
		array[15] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo16);
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SettingsDataMetric);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SettingsDataMetric)obj).Msaa;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			Unsafe.Unbox<SettingsDataMetric>(obj).Msaa = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "Msaa";
		jsonPropertyInfoValues2.JsonPropertyName = "msaa";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SettingsDataMetric).GetProperty("Msaa", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo17 = jsonPropertyInfoValues2;
		array[16] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo17);
		return array;
	}

	private void SettingsDataMetricSerializeHandler(Utf8JsonWriter writer, SettingsDataMetric value)
	{
		writer.WriteStartObject();
		JsonEncodedText propName_buildId = PropName_buildId;
		SettingsDataMetric settingsDataMetric = value;
		writer.WriteString(propName_buildId, settingsDataMetric.BuildId);
		JsonEncodedText propName_os = PropName_os;
		settingsDataMetric = value;
		writer.WriteString(propName_os, settingsDataMetric.Os);
		JsonEncodedText propName_platform = PropName_platform;
		settingsDataMetric = value;
		writer.WriteString(propName_platform, settingsDataMetric.Platform);
		JsonEncodedText propName_systemRam = PropName_systemRam;
		settingsDataMetric = value;
		writer.WriteNumber(propName_systemRam, settingsDataMetric.SystemRam);
		JsonEncodedText propName_language = PropName_language;
		settingsDataMetric = value;
		writer.WriteString(propName_language, settingsDataMetric.LanguageCode);
		writer.WritePropertyName(PropName_combatSpeed);
		settingsDataMetric = value;
		JsonSerializer.Serialize(writer, settingsDataMetric.FastModeType, FastModeType);
		JsonEncodedText propName_screenshake = PropName_screenshake;
		settingsDataMetric = value;
		writer.WriteNumber(propName_screenshake, settingsDataMetric.Screenshake);
		JsonEncodedText propName_runTimer = PropName_runTimer;
		settingsDataMetric = value;
		writer.WriteBoolean(propName_runTimer, settingsDataMetric.ShowRunTimer);
		JsonEncodedText propName_cardIndices = PropName_cardIndices;
		settingsDataMetric = value;
		writer.WriteBoolean(propName_cardIndices, settingsDataMetric.ShowCardIndices);
		JsonEncodedText propName_displayCount = PropName_displayCount;
		settingsDataMetric = value;
		writer.WriteNumber(propName_displayCount, settingsDataMetric.DisplayCount);
		writer.WritePropertyName(PropName_displayResolution);
		settingsDataMetric = value;
		Vector2ISerializeHandler(writer, settingsDataMetric.DisplayResolution);
		JsonEncodedText propName_fullscreen = PropName_fullscreen;
		settingsDataMetric = value;
		writer.WriteBoolean(propName_fullscreen, settingsDataMetric.Fullscreen);
		writer.WritePropertyName(PropName_aspectRatio);
		settingsDataMetric = value;
		JsonSerializer.Serialize(writer, settingsDataMetric.AspectRatio, AspectRatioSetting);
		JsonEncodedText propName_resizeWindows = PropName_resizeWindows;
		settingsDataMetric = value;
		writer.WriteBoolean(propName_resizeWindows, settingsDataMetric.ResizeWindows);
		writer.WritePropertyName(PropName_vSync);
		settingsDataMetric = value;
		JsonSerializer.Serialize(writer, settingsDataMetric.VSync, VSyncType);
		JsonEncodedText propName_fpsLimit = PropName_fpsLimit;
		settingsDataMetric = value;
		writer.WriteNumber(propName_fpsLimit, settingsDataMetric.FpsLimit);
		JsonEncodedText propName_msaa = PropName_msaa;
		settingsDataMetric = value;
		writer.WriteNumber(propName_msaa, settingsDataMetric.Msaa);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] SettingsDataMetricCtorParamInit()
	{
		return new JsonParameterInfoValues[2]
		{
			new JsonParameterInfoValues
			{
				Name = "BuildId",
				ParameterType = typeof(string),
				Position = 0,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "FastModeType",
				ParameterType = typeof(FastModeType),
				Position = 1,
				IsNullable = false,
				IsMemberInitializer = true
			}
		};
	}

	private JsonTypeInfo<AspectRatioSetting> Create_AspectRatioSetting(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<AspectRatioSetting> jsonTypeInfo))
		{
			JsonConverter converter = ExpandConverter(typeof(AspectRatioSetting), new JsonStringEnumConverter<AspectRatioSetting>(), options);
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<AspectRatioSetting>(options, converter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<FastModeType> Create_FastModeType(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<FastModeType> jsonTypeInfo))
		{
			JsonConverter converter = ExpandConverter(typeof(FastModeType), new JsonStringEnumConverter<FastModeType>(), options);
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<FastModeType>(options, converter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<VSyncType> Create_VSyncType(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<VSyncType> jsonTypeInfo))
		{
			JsonConverter converter = ExpandConverter(typeof(VSyncType), new JsonStringEnumConverter<VSyncType>(), options);
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<VSyncType>(options, converter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<EpochMetric> Create_EpochMetric(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<EpochMetric> jsonTypeInfo))
		{
			JsonObjectInfoValues<EpochMetric> jsonObjectInfoValues = new JsonObjectInfoValues<EpochMetric>();
			jsonObjectInfoValues.ObjectCreator = null;
			jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = (object[] args) => new EpochMetric
			{
				BuildId = (string)args[0],
				Epoch = (string)args[1]
			};
			jsonObjectInfoValues.PropertyMetadataInitializer = (JsonSerializerContext _) => EpochMetricPropInit(options);
			jsonObjectInfoValues.ConstructorParameterMetadataInitializer = EpochMetricCtorParamInit;
			jsonObjectInfoValues.ConstructorAttributeProviderFactory = () => typeof(EpochMetric).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null);
			jsonObjectInfoValues.SerializeHandler = EpochMetricSerializeHandler;
			JsonObjectInfoValues<EpochMetric> objectInfo = jsonObjectInfoValues;
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] EpochMetricPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[5];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(EpochMetric);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((EpochMetric)obj).BuildId;
		jsonPropertyInfoValues.Setter = delegate(object obj, string? value)
		{
			((EpochMetric)obj).BuildId = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "BuildId";
		jsonPropertyInfoValues.JsonPropertyName = "buildId";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(EpochMetric).GetProperty("BuildId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsRequired = true;
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(EpochMetric);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((EpochMetric)obj).Epoch;
		jsonPropertyInfoValues.Setter = delegate(object obj, string? value)
		{
			((EpochMetric)obj).Epoch = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Epoch";
		jsonPropertyInfoValues.JsonPropertyName = "epoch";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(EpochMetric).GetProperty("Epoch", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo2 = jsonPropertyInfoValues;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsRequired = true;
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		JsonPropertyInfoValues<int> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(EpochMetric);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((EpochMetric)obj).TotalEpochs;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((EpochMetric)obj).TotalEpochs = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "TotalEpochs";
		jsonPropertyInfoValues2.JsonPropertyName = "totalEpochs";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(EpochMetric).GetProperty("TotalEpochs", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo3 = jsonPropertyInfoValues2;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		JsonPropertyInfoValues<long> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<long>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(EpochMetric);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((EpochMetric)obj).TotalPlaytime;
		jsonPropertyInfoValues3.Setter = delegate(object obj, long value)
		{
			((EpochMetric)obj).TotalPlaytime = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "TotalPlaytime";
		jsonPropertyInfoValues3.JsonPropertyName = "totalPlaytime";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(EpochMetric).GetProperty("TotalPlaytime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(long), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<long> propertyInfo4 = jsonPropertyInfoValues3;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(EpochMetric);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((EpochMetric)obj).TotalRuns;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((EpochMetric)obj).TotalRuns = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "TotalRuns";
		jsonPropertyInfoValues2.JsonPropertyName = "totalRuns";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(EpochMetric).GetProperty("TotalRuns", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo5 = jsonPropertyInfoValues2;
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		return array;
	}

	private void EpochMetricSerializeHandler(Utf8JsonWriter writer, EpochMetric? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteString(PropName_buildId, value.BuildId);
		writer.WriteString(PropName_epoch, value.Epoch);
		writer.WriteNumber(PropName_totalEpochs, value.TotalEpochs);
		writer.WriteNumber(PropName_totalPlaytime, value.TotalPlaytime);
		writer.WriteNumber(PropName_totalRuns, value.TotalRuns);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] EpochMetricCtorParamInit()
	{
		return new JsonParameterInfoValues[2]
		{
			new JsonParameterInfoValues
			{
				Name = "BuildId",
				ParameterType = typeof(string),
				Position = 0,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Epoch",
				ParameterType = typeof(string),
				Position = 1,
				IsNullable = false,
				IsMemberInitializer = true
			}
		};
	}

	private JsonTypeInfo<IEnumerable<ModelId>> Create_IEnumerableModelId(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<IEnumerable<ModelId>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<IEnumerable<ModelId>> collectionInfo = new JsonCollectionInfoValues<IEnumerable<ModelId>>
			{
				ObjectCreator = null,
				SerializeHandler = IEnumerableModelIdSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateIEnumerableInfo<IEnumerable<ModelId>, ModelId>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private void IEnumerableModelIdSerializeHandler(Utf8JsonWriter writer, IEnumerable<ModelId>? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartArray();
		foreach (ModelId item in value)
		{
			ModelIdSerializeHandler(writer, item);
		}
		writer.WriteEndArray();
	}

	private JsonTypeInfo<List<ModelId>> Create_ListModelId(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<ModelId>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<ModelId>> collectionInfo = new JsonCollectionInfoValues<List<ModelId>>
			{
				ObjectCreator = () => new List<ModelId>(),
				SerializeHandler = ListModelIdSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<ModelId>, ModelId>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private void ListModelIdSerializeHandler(Utf8JsonWriter writer, List<ModelId>? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartArray();
		for (int i = 0; i < value.Count; i++)
		{
			ModelIdSerializeHandler(writer, value[i]);
		}
		writer.WriteEndArray();
	}

	private JsonTypeInfo<List<ActWinMetric>> Create_ListActWinMetric(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<ActWinMetric>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<ActWinMetric>> collectionInfo = new JsonCollectionInfoValues<List<ActWinMetric>>
			{
				ObjectCreator = () => new List<ActWinMetric>(),
				SerializeHandler = ListActWinMetricSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<ActWinMetric>, ActWinMetric>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private void ListActWinMetricSerializeHandler(Utf8JsonWriter writer, List<ActWinMetric>? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartArray();
		for (int i = 0; i < value.Count; i++)
		{
			ActWinMetricSerializeHandler(writer, value[i]);
		}
		writer.WriteEndArray();
	}

	private JsonTypeInfo<List<AncientMetric>> Create_ListAncientMetric(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<AncientMetric>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<AncientMetric>> collectionInfo = new JsonCollectionInfoValues<List<AncientMetric>>
			{
				ObjectCreator = () => new List<AncientMetric>(),
				SerializeHandler = ListAncientMetricSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<AncientMetric>, AncientMetric>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private void ListAncientMetricSerializeHandler(Utf8JsonWriter writer, List<AncientMetric>? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartArray();
		for (int i = 0; i < value.Count; i++)
		{
			AncientMetricSerializeHandler(writer, value[i]);
		}
		writer.WriteEndArray();
	}

	private JsonTypeInfo<List<CardChoiceMetric>> Create_ListCardChoiceMetric(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<CardChoiceMetric>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<CardChoiceMetric>> collectionInfo = new JsonCollectionInfoValues<List<CardChoiceMetric>>
			{
				ObjectCreator = () => new List<CardChoiceMetric>(),
				SerializeHandler = ListCardChoiceMetricSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<CardChoiceMetric>, CardChoiceMetric>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private void ListCardChoiceMetricSerializeHandler(Utf8JsonWriter writer, List<CardChoiceMetric>? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartArray();
		for (int i = 0; i < value.Count; i++)
		{
			CardChoiceMetricSerializeHandler(writer, value[i]);
		}
		writer.WriteEndArray();
	}

	private JsonTypeInfo<List<EncounterMetric>> Create_ListEncounterMetric(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<EncounterMetric>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<EncounterMetric>> collectionInfo = new JsonCollectionInfoValues<List<EncounterMetric>>
			{
				ObjectCreator = () => new List<EncounterMetric>(),
				SerializeHandler = ListEncounterMetricSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<EncounterMetric>, EncounterMetric>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private void ListEncounterMetricSerializeHandler(Utf8JsonWriter writer, List<EncounterMetric>? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartArray();
		for (int i = 0; i < value.Count; i++)
		{
			EncounterMetricSerializeHandler(writer, value[i]);
		}
		writer.WriteEndArray();
	}

	private JsonTypeInfo<List<EventChoiceMetric>> Create_ListEventChoiceMetric(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<EventChoiceMetric>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<EventChoiceMetric>> collectionInfo = new JsonCollectionInfoValues<List<EventChoiceMetric>>
			{
				ObjectCreator = () => new List<EventChoiceMetric>(),
				SerializeHandler = ListEventChoiceMetricSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<EventChoiceMetric>, EventChoiceMetric>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private void ListEventChoiceMetricSerializeHandler(Utf8JsonWriter writer, List<EventChoiceMetric>? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartArray();
		for (int i = 0; i < value.Count; i++)
		{
			EventChoiceMetricSerializeHandler(writer, value[i]);
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

	private JsonTypeInfo<long> Create_Int64(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<long> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<long>(options, JsonMetadataServices.Int64Converter);
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

	public MetricsSerializerContext()
		: base(null)
	{
	}

	public MetricsSerializerContext(JsonSerializerOptions options)
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
		if (type == typeof(float))
		{
			return Create_Single(options);
		}
		if (type == typeof(Vector2I))
		{
			return Create_Vector2I(options);
		}
		if (type == typeof(AchievementMetric))
		{
			return Create_AchievementMetric(options);
		}
		if (type == typeof(ModelId))
		{
			return Create_ModelId(options);
		}
		if (type == typeof(ActWinMetric))
		{
			return Create_ActWinMetric(options);
		}
		if (type == typeof(AncientMetric))
		{
			return Create_AncientMetric(options);
		}
		if (type == typeof(CardChoiceMetric))
		{
			return Create_CardChoiceMetric(options);
		}
		if (type == typeof(EncounterMetric))
		{
			return Create_EncounterMetric(options);
		}
		if (type == typeof(EventChoiceMetric))
		{
			return Create_EventChoiceMetric(options);
		}
		if (type == typeof(RunMetrics))
		{
			return Create_RunMetrics(options);
		}
		if (type == typeof(SettingsDataMetric))
		{
			return Create_SettingsDataMetric(options);
		}
		if (type == typeof(AspectRatioSetting))
		{
			return Create_AspectRatioSetting(options);
		}
		if (type == typeof(FastModeType))
		{
			return Create_FastModeType(options);
		}
		if (type == typeof(VSyncType))
		{
			return Create_VSyncType(options);
		}
		if (type == typeof(EpochMetric))
		{
			return Create_EpochMetric(options);
		}
		if (type == typeof(IEnumerable<ModelId>))
		{
			return Create_IEnumerableModelId(options);
		}
		if (type == typeof(List<ModelId>))
		{
			return Create_ListModelId(options);
		}
		if (type == typeof(List<ActWinMetric>))
		{
			return Create_ListActWinMetric(options);
		}
		if (type == typeof(List<AncientMetric>))
		{
			return Create_ListAncientMetric(options);
		}
		if (type == typeof(List<CardChoiceMetric>))
		{
			return Create_ListCardChoiceMetric(options);
		}
		if (type == typeof(List<EncounterMetric>))
		{
			return Create_ListEncounterMetric(options);
		}
		if (type == typeof(List<EventChoiceMetric>))
		{
			return Create_ListEventChoiceMetric(options);
		}
		if (type == typeof(List<string>))
		{
			return Create_ListString(options);
		}
		if (type == typeof(int))
		{
			return Create_Int32(options);
		}
		if (type == typeof(long))
		{
			return Create_Int64(options);
		}
		if (type == typeof(string))
		{
			return Create_String(options);
		}
		return null;
	}
}
