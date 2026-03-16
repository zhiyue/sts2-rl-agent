using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Godot;
using MegaCrit.Sts2.Core.Achievements;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.Rngs;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.FeedbackScreen;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Platform.Null;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.Saves.MapDrawing;
using MegaCrit.Sts2.Core.Saves.Migrations;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Saves;

[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true, ReadCommentHandling = JsonCommentHandling.Skip, Converters = new Type[]
{
	typeof(ModelIdRunSaveConverter),
	typeof(SnakeCaseJsonStringEnumConverter<Achievement>),
	typeof(SnakeCaseJsonStringEnumConverter<AspectRatioSetting>),
	typeof(SnakeCaseJsonStringEnumConverter<VSyncType>),
	typeof(SnakeCaseJsonStringEnumConverter<GameMode>),
	typeof(SnakeCaseJsonStringEnumConverter<RelicRarity>),
	typeof(SnakeCaseJsonStringEnumConverter<RunRngType>),
	typeof(SnakeCaseJsonStringEnumConverter<PlayerRngType>),
	typeof(SnakeCaseJsonStringEnumConverter<MapPointType>),
	typeof(SnakeCaseJsonStringEnumConverter<ModSource>),
	typeof(SnakeCaseJsonStringEnumConverter<PlatformType>),
	typeof(SnakeCaseJsonStringEnumConverter<RewardType>),
	typeof(SnakeCaseJsonStringEnumConverter<RoomType>),
	typeof(SnakeCaseJsonStringEnumConverter<EpochState>),
	typeof(SnakeCaseJsonStringEnumConverter<FastModeType>),
	typeof(SnakeCaseJsonStringEnumConverter<CardCreationSource>),
	typeof(SnakeCaseJsonStringEnumConverter<CardRarityOddsType>),
	typeof(SnakeCaseJsonStringEnumConverter<ControllerMappingType>)
}, UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip, GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(NullLeaderboardFile))]
[JsonSerializable(typeof(SerializableRun))]
[JsonSerializable(typeof(FeedbackData))]
[JsonSerializable(typeof(SettingsSave))]
[JsonSerializable(typeof(PrefsSave))]
[JsonSerializable(typeof(ProfileSave))]
[JsonSerializable(typeof(SerializableProgress))]
[JsonSerializable(typeof(RunHistory))]
[JsonSerializable(typeof(List<MigratingData>))]
[JsonSerializable(typeof(List<List<PlayerMapPointHistoryEntry>>))]
[JsonSerializable(typeof(ModManifest))]
[JsonSerializable(typeof(JsonObject))]
[JsonSerializable(typeof(List<JsonNode>))]
[JsonSerializable(typeof(long))]
[JsonSerializable(typeof(double))]
[JsonSerializable(typeof(List<Dictionary<string, object>>))]
[JsonSerializable(typeof(List<NullMultiplayerName>))]
[GeneratedCode("System.Text.Json.SourceGeneration", "9.0.12.31616")]
internal class MegaCritSerializerContext : JsonSerializerContext, IJsonTypeInfoResolver
{
	private JsonTypeInfo<bool>? _Boolean;

	private JsonTypeInfo<double>? _Double;

	private JsonTypeInfo<float>? _Single;

	private JsonTypeInfo<Vector2>? _Vector2;

	private JsonTypeInfo<Vector2I>? _Vector2I;

	private JsonTypeInfo<ControllerMappingType>? _ControllerMappingType;

	private JsonTypeInfo<RelicRarity>? _RelicRarity;

	private JsonTypeInfo<PlayerRngType>? _PlayerRngType;

	private JsonTypeInfo<RunRngType>? _RunRngType;

	private JsonTypeInfo<LocString>? _LocString;

	private JsonTypeInfo<MapCoord>? _MapCoord;

	private JsonTypeInfo<MapPointType>? _MapPointType;

	private JsonTypeInfo<DisabledMod>? _DisabledMod;

	private JsonTypeInfo<ModManifest>? _ModManifest;

	private JsonTypeInfo<ModSettings>? _ModSettings;

	private JsonTypeInfo<ModSource>? _ModSource;

	private JsonTypeInfo<ModelId>? _ModelId;

	private JsonTypeInfo<FeedbackData>? _FeedbackData;

	private JsonTypeInfo<NullLeaderboard>? _NullLeaderboard;

	private JsonTypeInfo<NullLeaderboardFile>? _NullLeaderboardFile;

	private JsonTypeInfo<NullLeaderboardFileEntry>? _NullLeaderboardFileEntry;

	private JsonTypeInfo<NullMultiplayerName>? _NullMultiplayerName;

	private JsonTypeInfo<PlatformType>? _PlatformType;

	private JsonTypeInfo<RewardType>? _RewardType;

	private JsonTypeInfo<RoomType>? _RoomType;

	private JsonTypeInfo<CardCreationSource>? _CardCreationSource;

	private JsonTypeInfo<CardRarityOddsType>? _CardRarityOddsType;

	private JsonTypeInfo<GameMode>? _GameMode;

	private JsonTypeInfo<AncientChoiceHistoryEntry>? _AncientChoiceHistoryEntry;

	private JsonTypeInfo<CardChoiceHistoryEntry>? _CardChoiceHistoryEntry;

	private JsonTypeInfo<CardEnchantmentHistoryEntry>? _CardEnchantmentHistoryEntry;

	private JsonTypeInfo<CardTransformationHistoryEntry>? _CardTransformationHistoryEntry;

	private JsonTypeInfo<EventOptionHistoryEntry>? _EventOptionHistoryEntry;

	private JsonTypeInfo<MapPointHistoryEntry>? _MapPointHistoryEntry;

	private JsonTypeInfo<MapPointRoomHistoryEntry>? _MapPointRoomHistoryEntry;

	private JsonTypeInfo<ModelChoiceHistoryEntry>? _ModelChoiceHistoryEntry;

	private JsonTypeInfo<PlayerMapPointHistoryEntry>? _PlayerMapPointHistoryEntry;

	private JsonTypeInfo<RunHistory>? _RunHistory;

	private JsonTypeInfo<RunHistoryPlayer>? _RunHistoryPlayer;

	private JsonTypeInfo<AncientCharacterStats>? _AncientCharacterStats;

	private JsonTypeInfo<AncientStats>? _AncientStats;

	private JsonTypeInfo<CardStats>? _CardStats;

	private JsonTypeInfo<CharacterStats>? _CharacterStats;

	private JsonTypeInfo<EncounterStats>? _EncounterStats;

	private JsonTypeInfo<EnemyStats>? _EnemyStats;

	private JsonTypeInfo<EpochState>? _EpochState;

	private JsonTypeInfo<FightStats>? _FightStats;

	private JsonTypeInfo<SerializableMapDrawingLine>? _SerializableMapDrawingLine;

	private JsonTypeInfo<SerializableMapDrawings>? _SerializableMapDrawings;

	private JsonTypeInfo<SerializablePlayerMapDrawings>? _SerializablePlayerMapDrawings;

	private JsonTypeInfo<MigratingData>? _MigratingData;

	private JsonTypeInfo<PrefsSave>? _PrefsSave;

	private JsonTypeInfo<ProfileSave>? _ProfileSave;

	private JsonTypeInfo<SavedProperties>? _SavedProperties;

	private JsonTypeInfo<SavedProperties.SavedProperty<bool>>? _SavedPropertyBoolean;

	private JsonTypeInfo<SavedProperties.SavedProperty<ModelId>>? _SavedPropertyModelId;

	private JsonTypeInfo<SavedProperties.SavedProperty<SerializableCard[]>>? _SavedPropertySerializableCardArray;

	private JsonTypeInfo<SavedProperties.SavedProperty<SerializableCard>>? _SavedPropertySerializableCard;

	private JsonTypeInfo<SavedProperties.SavedProperty<int[]>>? _SavedPropertyInt32Array;

	private JsonTypeInfo<SavedProperties.SavedProperty<int>>? _SavedPropertyInt32;

	private JsonTypeInfo<SavedProperties.SavedProperty<string>>? _SavedPropertyString;

	private JsonTypeInfo<SerializableActMap>? _SerializableActMap;

	private JsonTypeInfo<SerializableActModel>? _SerializableActModel;

	private JsonTypeInfo<SerializableCard>? _SerializableCard;

	private JsonTypeInfo<SerializableCard[]>? _SerializableCardArray;

	private JsonTypeInfo<SerializableEnchantment>? _SerializableEnchantment;

	private JsonTypeInfo<SerializableExtraRunFields>? _SerializableExtraRunFields;

	private JsonTypeInfo<SerializableMapPoint>? _SerializableMapPoint;

	private JsonTypeInfo<SerializableModifier>? _SerializableModifier;

	private JsonTypeInfo<SerializablePlayer>? _SerializablePlayer;

	private JsonTypeInfo<SerializablePlayerOddsSet>? _SerializablePlayerOddsSet;

	private JsonTypeInfo<SerializablePotion>? _SerializablePotion;

	private JsonTypeInfo<SerializableRelic>? _SerializableRelic;

	private JsonTypeInfo<SerializableRelicGrabBag>? _SerializableRelicGrabBag;

	private JsonTypeInfo<SerializableReward>? _SerializableReward;

	private JsonTypeInfo<SerializableRoom>? _SerializableRoom;

	private JsonTypeInfo<SerializableRoomSet>? _SerializableRoomSet;

	private JsonTypeInfo<SerializableRunOddsSet>? _SerializableRunOddsSet;

	private JsonTypeInfo<SerializableRunRngSet>? _SerializableRunRngSet;

	private JsonTypeInfo<SerializableEpoch>? _SerializableEpoch;

	private JsonTypeInfo<SerializableExtraPlayerFields>? _SerializableExtraPlayerFields;

	private JsonTypeInfo<SerializablePlayerRngSet>? _SerializablePlayerRngSet;

	private JsonTypeInfo<SerializableProgress>? _SerializableProgress;

	private JsonTypeInfo<SerializableRun>? _SerializableRun;

	private JsonTypeInfo<SerializableUnlockedAchievement>? _SerializableUnlockedAchievement;

	private JsonTypeInfo<SettingsSave>? _SettingsSave;

	private JsonTypeInfo<AspectRatioSetting>? _AspectRatioSetting;

	private JsonTypeInfo<FastModeType>? _FastModeType;

	private JsonTypeInfo<VSyncType>? _VSyncType;

	private JsonTypeInfo<SerializableUnlockState>? _SerializableUnlockState;

	private JsonTypeInfo<Dictionary<RelicRarity, List<ModelId>>>? _DictionaryRelicRarityListModelId;

	private JsonTypeInfo<Dictionary<PlayerRngType, int>>? _DictionaryPlayerRngTypeInt32;

	private JsonTypeInfo<Dictionary<RunRngType, int>>? _DictionaryRunRngTypeInt32;

	private JsonTypeInfo<Dictionary<string, object>>? _DictionaryStringObject;

	private JsonTypeInfo<Dictionary<string, string>>? _DictionaryStringString;

	private JsonTypeInfo<Dictionary<ulong, List<SerializableReward>>>? _DictionaryUInt64ListSerializableReward;

	private JsonTypeInfo<IEnumerable<SerializableCard>>? _IEnumerableSerializableCard;

	private JsonTypeInfo<IEnumerable<SerializablePotion>>? _IEnumerableSerializablePotion;

	private JsonTypeInfo<IEnumerable<SerializableRelic>>? _IEnumerableSerializableRelic;

	private JsonTypeInfo<List<Vector2>>? _ListVector2;

	private JsonTypeInfo<List<MapCoord>>? _ListMapCoord;

	private JsonTypeInfo<List<DisabledMod>>? _ListDisabledMod;

	private JsonTypeInfo<List<ModelId>>? _ListModelId;

	private JsonTypeInfo<List<NullLeaderboard>>? _ListNullLeaderboard;

	private JsonTypeInfo<List<NullLeaderboardFileEntry>>? _ListNullLeaderboardFileEntry;

	private JsonTypeInfo<List<NullMultiplayerName>>? _ListNullMultiplayerName;

	private JsonTypeInfo<List<AncientChoiceHistoryEntry>>? _ListAncientChoiceHistoryEntry;

	private JsonTypeInfo<List<CardChoiceHistoryEntry>>? _ListCardChoiceHistoryEntry;

	private JsonTypeInfo<List<CardEnchantmentHistoryEntry>>? _ListCardEnchantmentHistoryEntry;

	private JsonTypeInfo<List<CardTransformationHistoryEntry>>? _ListCardTransformationHistoryEntry;

	private JsonTypeInfo<List<EventOptionHistoryEntry>>? _ListEventOptionHistoryEntry;

	private JsonTypeInfo<List<MapPointHistoryEntry>>? _ListMapPointHistoryEntry;

	private JsonTypeInfo<List<MapPointRoomHistoryEntry>>? _ListMapPointRoomHistoryEntry;

	private JsonTypeInfo<List<ModelChoiceHistoryEntry>>? _ListModelChoiceHistoryEntry;

	private JsonTypeInfo<List<PlayerMapPointHistoryEntry>>? _ListPlayerMapPointHistoryEntry;

	private JsonTypeInfo<List<RunHistoryPlayer>>? _ListRunHistoryPlayer;

	private JsonTypeInfo<List<AncientCharacterStats>>? _ListAncientCharacterStats;

	private JsonTypeInfo<List<AncientStats>>? _ListAncientStats;

	private JsonTypeInfo<List<CardStats>>? _ListCardStats;

	private JsonTypeInfo<List<CharacterStats>>? _ListCharacterStats;

	private JsonTypeInfo<List<EncounterStats>>? _ListEncounterStats;

	private JsonTypeInfo<List<EnemyStats>>? _ListEnemyStats;

	private JsonTypeInfo<List<FightStats>>? _ListFightStats;

	private JsonTypeInfo<List<SerializableMapDrawingLine>>? _ListSerializableMapDrawingLine;

	private JsonTypeInfo<List<SerializablePlayerMapDrawings>>? _ListSerializablePlayerMapDrawings;

	private JsonTypeInfo<List<MigratingData>>? _ListMigratingData;

	private JsonTypeInfo<List<SavedProperties.SavedProperty<bool>>>? _ListSavedPropertyBoolean;

	private JsonTypeInfo<List<SavedProperties.SavedProperty<ModelId>>>? _ListSavedPropertyModelId;

	private JsonTypeInfo<List<SavedProperties.SavedProperty<SerializableCard[]>>>? _ListSavedPropertySerializableCardArray;

	private JsonTypeInfo<List<SavedProperties.SavedProperty<SerializableCard>>>? _ListSavedPropertySerializableCard;

	private JsonTypeInfo<List<SavedProperties.SavedProperty<int[]>>>? _ListSavedPropertyInt32Array;

	private JsonTypeInfo<List<SavedProperties.SavedProperty<int>>>? _ListSavedPropertyInt32;

	private JsonTypeInfo<List<SavedProperties.SavedProperty<string>>>? _ListSavedPropertyString;

	private JsonTypeInfo<List<SerializableActModel>>? _ListSerializableActModel;

	private JsonTypeInfo<List<SerializableCard>>? _ListSerializableCard;

	private JsonTypeInfo<List<SerializableMapPoint>>? _ListSerializableMapPoint;

	private JsonTypeInfo<List<SerializableModifier>>? _ListSerializableModifier;

	private JsonTypeInfo<List<SerializablePlayer>>? _ListSerializablePlayer;

	private JsonTypeInfo<List<SerializablePotion>>? _ListSerializablePotion;

	private JsonTypeInfo<List<SerializableRelic>>? _ListSerializableRelic;

	private JsonTypeInfo<List<SerializableReward>>? _ListSerializableReward;

	private JsonTypeInfo<List<SerializableEpoch>>? _ListSerializableEpoch;

	private JsonTypeInfo<List<SerializableUnlockedAchievement>>? _ListSerializableUnlockedAchievement;

	private JsonTypeInfo<List<Dictionary<string, object>>>? _ListDictionaryStringObject;

	private JsonTypeInfo<List<List<MapPointHistoryEntry>>>? _ListListMapPointHistoryEntry;

	private JsonTypeInfo<List<List<PlayerMapPointHistoryEntry>>>? _ListListPlayerMapPointHistoryEntry;

	private JsonTypeInfo<List<JsonNode>>? _ListJsonNode;

	private JsonTypeInfo<List<string>>? _ListString;

	private JsonTypeInfo<List<ulong>>? _ListUInt64;

	private JsonTypeInfo<DateTimeOffset>? _DateTimeOffset;

	private JsonTypeInfo<DateTimeOffset?>? _NullableDateTimeOffset;

	private JsonTypeInfo<JsonNode>? _JsonNode;

	private JsonTypeInfo<JsonObject>? _JsonObject;

	private JsonTypeInfo<int>? _Int32;

	private JsonTypeInfo<int?>? _NullableInt32;

	private JsonTypeInfo<int[]>? _Int32Array;

	private JsonTypeInfo<long>? _Int64;

	private JsonTypeInfo<object>? _Object;

	private JsonTypeInfo<string>? _String;

	private JsonTypeInfo<uint>? _UInt32;

	private JsonTypeInfo<ulong>? _UInt64;

	private static readonly JsonSerializerOptions s_defaultOptions = new JsonSerializerOptions
	{
		Converters = 
		{
			(JsonConverter)new ModelIdRunSaveConverter(),
			(JsonConverter)new SnakeCaseJsonStringEnumConverter<Achievement>(),
			(JsonConverter)new SnakeCaseJsonStringEnumConverter<AspectRatioSetting>(),
			(JsonConverter)new SnakeCaseJsonStringEnumConverter<VSyncType>(),
			(JsonConverter)new SnakeCaseJsonStringEnumConverter<GameMode>(),
			(JsonConverter)new SnakeCaseJsonStringEnumConverter<RelicRarity>(),
			(JsonConverter)new SnakeCaseJsonStringEnumConverter<RunRngType>(),
			(JsonConverter)new SnakeCaseJsonStringEnumConverter<PlayerRngType>(),
			(JsonConverter)new SnakeCaseJsonStringEnumConverter<MapPointType>(),
			(JsonConverter)new SnakeCaseJsonStringEnumConverter<ModSource>(),
			(JsonConverter)new SnakeCaseJsonStringEnumConverter<PlatformType>(),
			(JsonConverter)new SnakeCaseJsonStringEnumConverter<RewardType>(),
			(JsonConverter)new SnakeCaseJsonStringEnumConverter<RoomType>(),
			(JsonConverter)new SnakeCaseJsonStringEnumConverter<EpochState>(),
			(JsonConverter)new SnakeCaseJsonStringEnumConverter<FastModeType>(),
			(JsonConverter)new SnakeCaseJsonStringEnumConverter<CardCreationSource>(),
			(JsonConverter)new SnakeCaseJsonStringEnumConverter<CardRarityOddsType>(),
			(JsonConverter)new SnakeCaseJsonStringEnumConverter<ControllerMappingType>()
		},
		IncludeFields = true,
		ReadCommentHandling = JsonCommentHandling.Skip,
		UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
		WriteIndented = true
	};

	private const BindingFlags InstanceMemberBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	public static JsonSerializerOptions DefaultGeneratedSerializerOptions => Default.GeneratedSerializerOptions;

	public JsonTypeInfo<bool> Boolean => _Boolean ?? (_Boolean = (JsonTypeInfo<bool>)base.Options.GetTypeInfo(typeof(bool)));

	public JsonTypeInfo<double> Double => _Double ?? (_Double = (JsonTypeInfo<double>)base.Options.GetTypeInfo(typeof(double)));

	public JsonTypeInfo<float> Single => _Single ?? (_Single = (JsonTypeInfo<float>)base.Options.GetTypeInfo(typeof(float)));

	public JsonTypeInfo<Vector2> Vector2 => _Vector2 ?? (_Vector2 = (JsonTypeInfo<Vector2>)base.Options.GetTypeInfo(typeof(Vector2)));

	public JsonTypeInfo<Vector2I> Vector2I => _Vector2I ?? (_Vector2I = (JsonTypeInfo<Vector2I>)base.Options.GetTypeInfo(typeof(Vector2I)));

	public JsonTypeInfo<ControllerMappingType> ControllerMappingType => _ControllerMappingType ?? (_ControllerMappingType = (JsonTypeInfo<ControllerMappingType>)base.Options.GetTypeInfo(typeof(ControllerMappingType)));

	public JsonTypeInfo<RelicRarity> RelicRarity => _RelicRarity ?? (_RelicRarity = (JsonTypeInfo<RelicRarity>)base.Options.GetTypeInfo(typeof(RelicRarity)));

	public JsonTypeInfo<PlayerRngType> PlayerRngType => _PlayerRngType ?? (_PlayerRngType = (JsonTypeInfo<PlayerRngType>)base.Options.GetTypeInfo(typeof(PlayerRngType)));

	public JsonTypeInfo<RunRngType> RunRngType => _RunRngType ?? (_RunRngType = (JsonTypeInfo<RunRngType>)base.Options.GetTypeInfo(typeof(RunRngType)));

	public JsonTypeInfo<LocString> LocString => _LocString ?? (_LocString = (JsonTypeInfo<LocString>)base.Options.GetTypeInfo(typeof(LocString)));

	public JsonTypeInfo<MapCoord> MapCoord => _MapCoord ?? (_MapCoord = (JsonTypeInfo<MapCoord>)base.Options.GetTypeInfo(typeof(MapCoord)));

	public JsonTypeInfo<MapPointType> MapPointType => _MapPointType ?? (_MapPointType = (JsonTypeInfo<MapPointType>)base.Options.GetTypeInfo(typeof(MapPointType)));

	public JsonTypeInfo<DisabledMod> DisabledMod => _DisabledMod ?? (_DisabledMod = (JsonTypeInfo<DisabledMod>)base.Options.GetTypeInfo(typeof(DisabledMod)));

	public JsonTypeInfo<ModManifest> ModManifest => _ModManifest ?? (_ModManifest = (JsonTypeInfo<ModManifest>)base.Options.GetTypeInfo(typeof(ModManifest)));

	public JsonTypeInfo<ModSettings> ModSettings => _ModSettings ?? (_ModSettings = (JsonTypeInfo<ModSettings>)base.Options.GetTypeInfo(typeof(ModSettings)));

	public JsonTypeInfo<ModSource> ModSource => _ModSource ?? (_ModSource = (JsonTypeInfo<ModSource>)base.Options.GetTypeInfo(typeof(ModSource)));

	public JsonTypeInfo<ModelId> ModelId => _ModelId ?? (_ModelId = (JsonTypeInfo<ModelId>)base.Options.GetTypeInfo(typeof(ModelId)));

	public JsonTypeInfo<FeedbackData> FeedbackData => _FeedbackData ?? (_FeedbackData = (JsonTypeInfo<FeedbackData>)base.Options.GetTypeInfo(typeof(FeedbackData)));

	public JsonTypeInfo<NullLeaderboard> NullLeaderboard => _NullLeaderboard ?? (_NullLeaderboard = (JsonTypeInfo<NullLeaderboard>)base.Options.GetTypeInfo(typeof(NullLeaderboard)));

	public JsonTypeInfo<NullLeaderboardFile> NullLeaderboardFile => _NullLeaderboardFile ?? (_NullLeaderboardFile = (JsonTypeInfo<NullLeaderboardFile>)base.Options.GetTypeInfo(typeof(NullLeaderboardFile)));

	public JsonTypeInfo<NullLeaderboardFileEntry> NullLeaderboardFileEntry => _NullLeaderboardFileEntry ?? (_NullLeaderboardFileEntry = (JsonTypeInfo<NullLeaderboardFileEntry>)base.Options.GetTypeInfo(typeof(NullLeaderboardFileEntry)));

	public JsonTypeInfo<NullMultiplayerName> NullMultiplayerName => _NullMultiplayerName ?? (_NullMultiplayerName = (JsonTypeInfo<NullMultiplayerName>)base.Options.GetTypeInfo(typeof(NullMultiplayerName)));

	public JsonTypeInfo<PlatformType> PlatformType => _PlatformType ?? (_PlatformType = (JsonTypeInfo<PlatformType>)base.Options.GetTypeInfo(typeof(PlatformType)));

	public JsonTypeInfo<RewardType> RewardType => _RewardType ?? (_RewardType = (JsonTypeInfo<RewardType>)base.Options.GetTypeInfo(typeof(RewardType)));

	public JsonTypeInfo<RoomType> RoomType => _RoomType ?? (_RoomType = (JsonTypeInfo<RoomType>)base.Options.GetTypeInfo(typeof(RoomType)));

	public JsonTypeInfo<CardCreationSource> CardCreationSource => _CardCreationSource ?? (_CardCreationSource = (JsonTypeInfo<CardCreationSource>)base.Options.GetTypeInfo(typeof(CardCreationSource)));

	public JsonTypeInfo<CardRarityOddsType> CardRarityOddsType => _CardRarityOddsType ?? (_CardRarityOddsType = (JsonTypeInfo<CardRarityOddsType>)base.Options.GetTypeInfo(typeof(CardRarityOddsType)));

	public JsonTypeInfo<GameMode> GameMode => _GameMode ?? (_GameMode = (JsonTypeInfo<GameMode>)base.Options.GetTypeInfo(typeof(GameMode)));

	public JsonTypeInfo<AncientChoiceHistoryEntry> AncientChoiceHistoryEntry => _AncientChoiceHistoryEntry ?? (_AncientChoiceHistoryEntry = (JsonTypeInfo<AncientChoiceHistoryEntry>)base.Options.GetTypeInfo(typeof(AncientChoiceHistoryEntry)));

	public JsonTypeInfo<CardChoiceHistoryEntry> CardChoiceHistoryEntry => _CardChoiceHistoryEntry ?? (_CardChoiceHistoryEntry = (JsonTypeInfo<CardChoiceHistoryEntry>)base.Options.GetTypeInfo(typeof(CardChoiceHistoryEntry)));

	public JsonTypeInfo<CardEnchantmentHistoryEntry> CardEnchantmentHistoryEntry => _CardEnchantmentHistoryEntry ?? (_CardEnchantmentHistoryEntry = (JsonTypeInfo<CardEnchantmentHistoryEntry>)base.Options.GetTypeInfo(typeof(CardEnchantmentHistoryEntry)));

	public JsonTypeInfo<CardTransformationHistoryEntry> CardTransformationHistoryEntry => _CardTransformationHistoryEntry ?? (_CardTransformationHistoryEntry = (JsonTypeInfo<CardTransformationHistoryEntry>)base.Options.GetTypeInfo(typeof(CardTransformationHistoryEntry)));

	public JsonTypeInfo<EventOptionHistoryEntry> EventOptionHistoryEntry => _EventOptionHistoryEntry ?? (_EventOptionHistoryEntry = (JsonTypeInfo<EventOptionHistoryEntry>)base.Options.GetTypeInfo(typeof(EventOptionHistoryEntry)));

	public JsonTypeInfo<MapPointHistoryEntry> MapPointHistoryEntry => _MapPointHistoryEntry ?? (_MapPointHistoryEntry = (JsonTypeInfo<MapPointHistoryEntry>)base.Options.GetTypeInfo(typeof(MapPointHistoryEntry)));

	public JsonTypeInfo<MapPointRoomHistoryEntry> MapPointRoomHistoryEntry => _MapPointRoomHistoryEntry ?? (_MapPointRoomHistoryEntry = (JsonTypeInfo<MapPointRoomHistoryEntry>)base.Options.GetTypeInfo(typeof(MapPointRoomHistoryEntry)));

	public JsonTypeInfo<ModelChoiceHistoryEntry> ModelChoiceHistoryEntry => _ModelChoiceHistoryEntry ?? (_ModelChoiceHistoryEntry = (JsonTypeInfo<ModelChoiceHistoryEntry>)base.Options.GetTypeInfo(typeof(ModelChoiceHistoryEntry)));

	public JsonTypeInfo<PlayerMapPointHistoryEntry> PlayerMapPointHistoryEntry => _PlayerMapPointHistoryEntry ?? (_PlayerMapPointHistoryEntry = (JsonTypeInfo<PlayerMapPointHistoryEntry>)base.Options.GetTypeInfo(typeof(PlayerMapPointHistoryEntry)));

	public JsonTypeInfo<RunHistory> RunHistory => _RunHistory ?? (_RunHistory = (JsonTypeInfo<RunHistory>)base.Options.GetTypeInfo(typeof(RunHistory)));

	public JsonTypeInfo<RunHistoryPlayer> RunHistoryPlayer => _RunHistoryPlayer ?? (_RunHistoryPlayer = (JsonTypeInfo<RunHistoryPlayer>)base.Options.GetTypeInfo(typeof(RunHistoryPlayer)));

	public JsonTypeInfo<AncientCharacterStats> AncientCharacterStats => _AncientCharacterStats ?? (_AncientCharacterStats = (JsonTypeInfo<AncientCharacterStats>)base.Options.GetTypeInfo(typeof(AncientCharacterStats)));

	public JsonTypeInfo<AncientStats> AncientStats => _AncientStats ?? (_AncientStats = (JsonTypeInfo<AncientStats>)base.Options.GetTypeInfo(typeof(AncientStats)));

	public JsonTypeInfo<CardStats> CardStats => _CardStats ?? (_CardStats = (JsonTypeInfo<CardStats>)base.Options.GetTypeInfo(typeof(CardStats)));

	public JsonTypeInfo<CharacterStats> CharacterStats => _CharacterStats ?? (_CharacterStats = (JsonTypeInfo<CharacterStats>)base.Options.GetTypeInfo(typeof(CharacterStats)));

	public JsonTypeInfo<EncounterStats> EncounterStats => _EncounterStats ?? (_EncounterStats = (JsonTypeInfo<EncounterStats>)base.Options.GetTypeInfo(typeof(EncounterStats)));

	public JsonTypeInfo<EnemyStats> EnemyStats => _EnemyStats ?? (_EnemyStats = (JsonTypeInfo<EnemyStats>)base.Options.GetTypeInfo(typeof(EnemyStats)));

	public JsonTypeInfo<EpochState> EpochState => _EpochState ?? (_EpochState = (JsonTypeInfo<EpochState>)base.Options.GetTypeInfo(typeof(EpochState)));

	public JsonTypeInfo<FightStats> FightStats => _FightStats ?? (_FightStats = (JsonTypeInfo<FightStats>)base.Options.GetTypeInfo(typeof(FightStats)));

	public JsonTypeInfo<SerializableMapDrawingLine> SerializableMapDrawingLine => _SerializableMapDrawingLine ?? (_SerializableMapDrawingLine = (JsonTypeInfo<SerializableMapDrawingLine>)base.Options.GetTypeInfo(typeof(SerializableMapDrawingLine)));

	public JsonTypeInfo<SerializableMapDrawings> SerializableMapDrawings => _SerializableMapDrawings ?? (_SerializableMapDrawings = (JsonTypeInfo<SerializableMapDrawings>)base.Options.GetTypeInfo(typeof(SerializableMapDrawings)));

	public JsonTypeInfo<SerializablePlayerMapDrawings> SerializablePlayerMapDrawings => _SerializablePlayerMapDrawings ?? (_SerializablePlayerMapDrawings = (JsonTypeInfo<SerializablePlayerMapDrawings>)base.Options.GetTypeInfo(typeof(SerializablePlayerMapDrawings)));

	public JsonTypeInfo<MigratingData> MigratingData => _MigratingData ?? (_MigratingData = (JsonTypeInfo<MigratingData>)base.Options.GetTypeInfo(typeof(MigratingData)));

	public JsonTypeInfo<PrefsSave> PrefsSave => _PrefsSave ?? (_PrefsSave = (JsonTypeInfo<PrefsSave>)base.Options.GetTypeInfo(typeof(PrefsSave)));

	public JsonTypeInfo<ProfileSave> ProfileSave => _ProfileSave ?? (_ProfileSave = (JsonTypeInfo<ProfileSave>)base.Options.GetTypeInfo(typeof(ProfileSave)));

	public JsonTypeInfo<SavedProperties> SavedProperties => _SavedProperties ?? (_SavedProperties = (JsonTypeInfo<SavedProperties>)base.Options.GetTypeInfo(typeof(SavedProperties)));

	public JsonTypeInfo<SavedProperties.SavedProperty<bool>> SavedPropertyBoolean => _SavedPropertyBoolean ?? (_SavedPropertyBoolean = (JsonTypeInfo<SavedProperties.SavedProperty<bool>>)base.Options.GetTypeInfo(typeof(SavedProperties.SavedProperty<bool>)));

	public JsonTypeInfo<SavedProperties.SavedProperty<ModelId>> SavedPropertyModelId => _SavedPropertyModelId ?? (_SavedPropertyModelId = (JsonTypeInfo<SavedProperties.SavedProperty<ModelId>>)base.Options.GetTypeInfo(typeof(SavedProperties.SavedProperty<ModelId>)));

	public JsonTypeInfo<SavedProperties.SavedProperty<SerializableCard[]>> SavedPropertySerializableCardArray => _SavedPropertySerializableCardArray ?? (_SavedPropertySerializableCardArray = (JsonTypeInfo<SavedProperties.SavedProperty<SerializableCard[]>>)base.Options.GetTypeInfo(typeof(SavedProperties.SavedProperty<SerializableCard[]>)));

	public JsonTypeInfo<SavedProperties.SavedProperty<SerializableCard>> SavedPropertySerializableCard => _SavedPropertySerializableCard ?? (_SavedPropertySerializableCard = (JsonTypeInfo<SavedProperties.SavedProperty<SerializableCard>>)base.Options.GetTypeInfo(typeof(SavedProperties.SavedProperty<SerializableCard>)));

	public JsonTypeInfo<SavedProperties.SavedProperty<int[]>> SavedPropertyInt32Array => _SavedPropertyInt32Array ?? (_SavedPropertyInt32Array = (JsonTypeInfo<SavedProperties.SavedProperty<int[]>>)base.Options.GetTypeInfo(typeof(SavedProperties.SavedProperty<int[]>)));

	public JsonTypeInfo<SavedProperties.SavedProperty<int>> SavedPropertyInt32 => _SavedPropertyInt32 ?? (_SavedPropertyInt32 = (JsonTypeInfo<SavedProperties.SavedProperty<int>>)base.Options.GetTypeInfo(typeof(SavedProperties.SavedProperty<int>)));

	public JsonTypeInfo<SavedProperties.SavedProperty<string>> SavedPropertyString => _SavedPropertyString ?? (_SavedPropertyString = (JsonTypeInfo<SavedProperties.SavedProperty<string>>)base.Options.GetTypeInfo(typeof(SavedProperties.SavedProperty<string>)));

	public JsonTypeInfo<SerializableActMap> SerializableActMap => _SerializableActMap ?? (_SerializableActMap = (JsonTypeInfo<SerializableActMap>)base.Options.GetTypeInfo(typeof(SerializableActMap)));

	public JsonTypeInfo<SerializableActModel> SerializableActModel => _SerializableActModel ?? (_SerializableActModel = (JsonTypeInfo<SerializableActModel>)base.Options.GetTypeInfo(typeof(SerializableActModel)));

	public JsonTypeInfo<SerializableCard> SerializableCard => _SerializableCard ?? (_SerializableCard = (JsonTypeInfo<SerializableCard>)base.Options.GetTypeInfo(typeof(SerializableCard)));

	public JsonTypeInfo<SerializableCard[]> SerializableCardArray => _SerializableCardArray ?? (_SerializableCardArray = (JsonTypeInfo<SerializableCard[]>)base.Options.GetTypeInfo(typeof(SerializableCard[])));

	public JsonTypeInfo<SerializableEnchantment> SerializableEnchantment => _SerializableEnchantment ?? (_SerializableEnchantment = (JsonTypeInfo<SerializableEnchantment>)base.Options.GetTypeInfo(typeof(SerializableEnchantment)));

	public JsonTypeInfo<SerializableExtraRunFields> SerializableExtraRunFields => _SerializableExtraRunFields ?? (_SerializableExtraRunFields = (JsonTypeInfo<SerializableExtraRunFields>)base.Options.GetTypeInfo(typeof(SerializableExtraRunFields)));

	public JsonTypeInfo<SerializableMapPoint> SerializableMapPoint => _SerializableMapPoint ?? (_SerializableMapPoint = (JsonTypeInfo<SerializableMapPoint>)base.Options.GetTypeInfo(typeof(SerializableMapPoint)));

	public JsonTypeInfo<SerializableModifier> SerializableModifier => _SerializableModifier ?? (_SerializableModifier = (JsonTypeInfo<SerializableModifier>)base.Options.GetTypeInfo(typeof(SerializableModifier)));

	public JsonTypeInfo<SerializablePlayer> SerializablePlayer => _SerializablePlayer ?? (_SerializablePlayer = (JsonTypeInfo<SerializablePlayer>)base.Options.GetTypeInfo(typeof(SerializablePlayer)));

	public JsonTypeInfo<SerializablePlayerOddsSet> SerializablePlayerOddsSet => _SerializablePlayerOddsSet ?? (_SerializablePlayerOddsSet = (JsonTypeInfo<SerializablePlayerOddsSet>)base.Options.GetTypeInfo(typeof(SerializablePlayerOddsSet)));

	public JsonTypeInfo<SerializablePotion> SerializablePotion => _SerializablePotion ?? (_SerializablePotion = (JsonTypeInfo<SerializablePotion>)base.Options.GetTypeInfo(typeof(SerializablePotion)));

	public JsonTypeInfo<SerializableRelic> SerializableRelic => _SerializableRelic ?? (_SerializableRelic = (JsonTypeInfo<SerializableRelic>)base.Options.GetTypeInfo(typeof(SerializableRelic)));

	public JsonTypeInfo<SerializableRelicGrabBag> SerializableRelicGrabBag => _SerializableRelicGrabBag ?? (_SerializableRelicGrabBag = (JsonTypeInfo<SerializableRelicGrabBag>)base.Options.GetTypeInfo(typeof(SerializableRelicGrabBag)));

	public JsonTypeInfo<SerializableReward> SerializableReward => _SerializableReward ?? (_SerializableReward = (JsonTypeInfo<SerializableReward>)base.Options.GetTypeInfo(typeof(SerializableReward)));

	public JsonTypeInfo<SerializableRoom> SerializableRoom => _SerializableRoom ?? (_SerializableRoom = (JsonTypeInfo<SerializableRoom>)base.Options.GetTypeInfo(typeof(SerializableRoom)));

	public JsonTypeInfo<SerializableRoomSet> SerializableRoomSet => _SerializableRoomSet ?? (_SerializableRoomSet = (JsonTypeInfo<SerializableRoomSet>)base.Options.GetTypeInfo(typeof(SerializableRoomSet)));

	public JsonTypeInfo<SerializableRunOddsSet> SerializableRunOddsSet => _SerializableRunOddsSet ?? (_SerializableRunOddsSet = (JsonTypeInfo<SerializableRunOddsSet>)base.Options.GetTypeInfo(typeof(SerializableRunOddsSet)));

	public JsonTypeInfo<SerializableRunRngSet> SerializableRunRngSet => _SerializableRunRngSet ?? (_SerializableRunRngSet = (JsonTypeInfo<SerializableRunRngSet>)base.Options.GetTypeInfo(typeof(SerializableRunRngSet)));

	public JsonTypeInfo<SerializableEpoch> SerializableEpoch => _SerializableEpoch ?? (_SerializableEpoch = (JsonTypeInfo<SerializableEpoch>)base.Options.GetTypeInfo(typeof(SerializableEpoch)));

	public JsonTypeInfo<SerializableExtraPlayerFields> SerializableExtraPlayerFields => _SerializableExtraPlayerFields ?? (_SerializableExtraPlayerFields = (JsonTypeInfo<SerializableExtraPlayerFields>)base.Options.GetTypeInfo(typeof(SerializableExtraPlayerFields)));

	public JsonTypeInfo<SerializablePlayerRngSet> SerializablePlayerRngSet => _SerializablePlayerRngSet ?? (_SerializablePlayerRngSet = (JsonTypeInfo<SerializablePlayerRngSet>)base.Options.GetTypeInfo(typeof(SerializablePlayerRngSet)));

	public JsonTypeInfo<SerializableProgress> SerializableProgress => _SerializableProgress ?? (_SerializableProgress = (JsonTypeInfo<SerializableProgress>)base.Options.GetTypeInfo(typeof(SerializableProgress)));

	public JsonTypeInfo<SerializableRun> SerializableRun => _SerializableRun ?? (_SerializableRun = (JsonTypeInfo<SerializableRun>)base.Options.GetTypeInfo(typeof(SerializableRun)));

	public JsonTypeInfo<SerializableUnlockedAchievement> SerializableUnlockedAchievement => _SerializableUnlockedAchievement ?? (_SerializableUnlockedAchievement = (JsonTypeInfo<SerializableUnlockedAchievement>)base.Options.GetTypeInfo(typeof(SerializableUnlockedAchievement)));

	public JsonTypeInfo<SettingsSave> SettingsSave => _SettingsSave ?? (_SettingsSave = (JsonTypeInfo<SettingsSave>)base.Options.GetTypeInfo(typeof(SettingsSave)));

	public JsonTypeInfo<AspectRatioSetting> AspectRatioSetting => _AspectRatioSetting ?? (_AspectRatioSetting = (JsonTypeInfo<AspectRatioSetting>)base.Options.GetTypeInfo(typeof(AspectRatioSetting)));

	public JsonTypeInfo<FastModeType> FastModeType => _FastModeType ?? (_FastModeType = (JsonTypeInfo<FastModeType>)base.Options.GetTypeInfo(typeof(FastModeType)));

	public JsonTypeInfo<VSyncType> VSyncType => _VSyncType ?? (_VSyncType = (JsonTypeInfo<VSyncType>)base.Options.GetTypeInfo(typeof(VSyncType)));

	public JsonTypeInfo<SerializableUnlockState> SerializableUnlockState => _SerializableUnlockState ?? (_SerializableUnlockState = (JsonTypeInfo<SerializableUnlockState>)base.Options.GetTypeInfo(typeof(SerializableUnlockState)));

	public JsonTypeInfo<Dictionary<RelicRarity, List<ModelId>>> DictionaryRelicRarityListModelId => _DictionaryRelicRarityListModelId ?? (_DictionaryRelicRarityListModelId = (JsonTypeInfo<Dictionary<RelicRarity, List<ModelId>>>)base.Options.GetTypeInfo(typeof(Dictionary<RelicRarity, List<ModelId>>)));

	public JsonTypeInfo<Dictionary<PlayerRngType, int>> DictionaryPlayerRngTypeInt32 => _DictionaryPlayerRngTypeInt32 ?? (_DictionaryPlayerRngTypeInt32 = (JsonTypeInfo<Dictionary<PlayerRngType, int>>)base.Options.GetTypeInfo(typeof(Dictionary<PlayerRngType, int>)));

	public JsonTypeInfo<Dictionary<RunRngType, int>> DictionaryRunRngTypeInt32 => _DictionaryRunRngTypeInt32 ?? (_DictionaryRunRngTypeInt32 = (JsonTypeInfo<Dictionary<RunRngType, int>>)base.Options.GetTypeInfo(typeof(Dictionary<RunRngType, int>)));

	public JsonTypeInfo<Dictionary<string, object>> DictionaryStringObject => _DictionaryStringObject ?? (_DictionaryStringObject = (JsonTypeInfo<Dictionary<string, object>>)base.Options.GetTypeInfo(typeof(Dictionary<string, object>)));

	public JsonTypeInfo<Dictionary<string, string>> DictionaryStringString => _DictionaryStringString ?? (_DictionaryStringString = (JsonTypeInfo<Dictionary<string, string>>)base.Options.GetTypeInfo(typeof(Dictionary<string, string>)));

	public JsonTypeInfo<Dictionary<ulong, List<SerializableReward>>> DictionaryUInt64ListSerializableReward => _DictionaryUInt64ListSerializableReward ?? (_DictionaryUInt64ListSerializableReward = (JsonTypeInfo<Dictionary<ulong, List<SerializableReward>>>)base.Options.GetTypeInfo(typeof(Dictionary<ulong, List<SerializableReward>>)));

	public JsonTypeInfo<IEnumerable<SerializableCard>> IEnumerableSerializableCard => _IEnumerableSerializableCard ?? (_IEnumerableSerializableCard = (JsonTypeInfo<IEnumerable<SerializableCard>>)base.Options.GetTypeInfo(typeof(IEnumerable<SerializableCard>)));

	public JsonTypeInfo<IEnumerable<SerializablePotion>> IEnumerableSerializablePotion => _IEnumerableSerializablePotion ?? (_IEnumerableSerializablePotion = (JsonTypeInfo<IEnumerable<SerializablePotion>>)base.Options.GetTypeInfo(typeof(IEnumerable<SerializablePotion>)));

	public JsonTypeInfo<IEnumerable<SerializableRelic>> IEnumerableSerializableRelic => _IEnumerableSerializableRelic ?? (_IEnumerableSerializableRelic = (JsonTypeInfo<IEnumerable<SerializableRelic>>)base.Options.GetTypeInfo(typeof(IEnumerable<SerializableRelic>)));

	public JsonTypeInfo<List<Vector2>> ListVector2 => _ListVector2 ?? (_ListVector2 = (JsonTypeInfo<List<Vector2>>)base.Options.GetTypeInfo(typeof(List<Vector2>)));

	public JsonTypeInfo<List<MapCoord>> ListMapCoord => _ListMapCoord ?? (_ListMapCoord = (JsonTypeInfo<List<MapCoord>>)base.Options.GetTypeInfo(typeof(List<MapCoord>)));

	public JsonTypeInfo<List<DisabledMod>> ListDisabledMod => _ListDisabledMod ?? (_ListDisabledMod = (JsonTypeInfo<List<DisabledMod>>)base.Options.GetTypeInfo(typeof(List<DisabledMod>)));

	public JsonTypeInfo<List<ModelId>> ListModelId => _ListModelId ?? (_ListModelId = (JsonTypeInfo<List<ModelId>>)base.Options.GetTypeInfo(typeof(List<ModelId>)));

	public JsonTypeInfo<List<NullLeaderboard>> ListNullLeaderboard => _ListNullLeaderboard ?? (_ListNullLeaderboard = (JsonTypeInfo<List<NullLeaderboard>>)base.Options.GetTypeInfo(typeof(List<NullLeaderboard>)));

	public JsonTypeInfo<List<NullLeaderboardFileEntry>> ListNullLeaderboardFileEntry => _ListNullLeaderboardFileEntry ?? (_ListNullLeaderboardFileEntry = (JsonTypeInfo<List<NullLeaderboardFileEntry>>)base.Options.GetTypeInfo(typeof(List<NullLeaderboardFileEntry>)));

	public JsonTypeInfo<List<NullMultiplayerName>> ListNullMultiplayerName => _ListNullMultiplayerName ?? (_ListNullMultiplayerName = (JsonTypeInfo<List<NullMultiplayerName>>)base.Options.GetTypeInfo(typeof(List<NullMultiplayerName>)));

	public JsonTypeInfo<List<AncientChoiceHistoryEntry>> ListAncientChoiceHistoryEntry => _ListAncientChoiceHistoryEntry ?? (_ListAncientChoiceHistoryEntry = (JsonTypeInfo<List<AncientChoiceHistoryEntry>>)base.Options.GetTypeInfo(typeof(List<AncientChoiceHistoryEntry>)));

	public JsonTypeInfo<List<CardChoiceHistoryEntry>> ListCardChoiceHistoryEntry => _ListCardChoiceHistoryEntry ?? (_ListCardChoiceHistoryEntry = (JsonTypeInfo<List<CardChoiceHistoryEntry>>)base.Options.GetTypeInfo(typeof(List<CardChoiceHistoryEntry>)));

	public JsonTypeInfo<List<CardEnchantmentHistoryEntry>> ListCardEnchantmentHistoryEntry => _ListCardEnchantmentHistoryEntry ?? (_ListCardEnchantmentHistoryEntry = (JsonTypeInfo<List<CardEnchantmentHistoryEntry>>)base.Options.GetTypeInfo(typeof(List<CardEnchantmentHistoryEntry>)));

	public JsonTypeInfo<List<CardTransformationHistoryEntry>> ListCardTransformationHistoryEntry => _ListCardTransformationHistoryEntry ?? (_ListCardTransformationHistoryEntry = (JsonTypeInfo<List<CardTransformationHistoryEntry>>)base.Options.GetTypeInfo(typeof(List<CardTransformationHistoryEntry>)));

	public JsonTypeInfo<List<EventOptionHistoryEntry>> ListEventOptionHistoryEntry => _ListEventOptionHistoryEntry ?? (_ListEventOptionHistoryEntry = (JsonTypeInfo<List<EventOptionHistoryEntry>>)base.Options.GetTypeInfo(typeof(List<EventOptionHistoryEntry>)));

	public JsonTypeInfo<List<MapPointHistoryEntry>> ListMapPointHistoryEntry => _ListMapPointHistoryEntry ?? (_ListMapPointHistoryEntry = (JsonTypeInfo<List<MapPointHistoryEntry>>)base.Options.GetTypeInfo(typeof(List<MapPointHistoryEntry>)));

	public JsonTypeInfo<List<MapPointRoomHistoryEntry>> ListMapPointRoomHistoryEntry => _ListMapPointRoomHistoryEntry ?? (_ListMapPointRoomHistoryEntry = (JsonTypeInfo<List<MapPointRoomHistoryEntry>>)base.Options.GetTypeInfo(typeof(List<MapPointRoomHistoryEntry>)));

	public JsonTypeInfo<List<ModelChoiceHistoryEntry>> ListModelChoiceHistoryEntry => _ListModelChoiceHistoryEntry ?? (_ListModelChoiceHistoryEntry = (JsonTypeInfo<List<ModelChoiceHistoryEntry>>)base.Options.GetTypeInfo(typeof(List<ModelChoiceHistoryEntry>)));

	public JsonTypeInfo<List<PlayerMapPointHistoryEntry>> ListPlayerMapPointHistoryEntry => _ListPlayerMapPointHistoryEntry ?? (_ListPlayerMapPointHistoryEntry = (JsonTypeInfo<List<PlayerMapPointHistoryEntry>>)base.Options.GetTypeInfo(typeof(List<PlayerMapPointHistoryEntry>)));

	public JsonTypeInfo<List<RunHistoryPlayer>> ListRunHistoryPlayer => _ListRunHistoryPlayer ?? (_ListRunHistoryPlayer = (JsonTypeInfo<List<RunHistoryPlayer>>)base.Options.GetTypeInfo(typeof(List<RunHistoryPlayer>)));

	public JsonTypeInfo<List<AncientCharacterStats>> ListAncientCharacterStats => _ListAncientCharacterStats ?? (_ListAncientCharacterStats = (JsonTypeInfo<List<AncientCharacterStats>>)base.Options.GetTypeInfo(typeof(List<AncientCharacterStats>)));

	public JsonTypeInfo<List<AncientStats>> ListAncientStats => _ListAncientStats ?? (_ListAncientStats = (JsonTypeInfo<List<AncientStats>>)base.Options.GetTypeInfo(typeof(List<AncientStats>)));

	public JsonTypeInfo<List<CardStats>> ListCardStats => _ListCardStats ?? (_ListCardStats = (JsonTypeInfo<List<CardStats>>)base.Options.GetTypeInfo(typeof(List<CardStats>)));

	public JsonTypeInfo<List<CharacterStats>> ListCharacterStats => _ListCharacterStats ?? (_ListCharacterStats = (JsonTypeInfo<List<CharacterStats>>)base.Options.GetTypeInfo(typeof(List<CharacterStats>)));

	public JsonTypeInfo<List<EncounterStats>> ListEncounterStats => _ListEncounterStats ?? (_ListEncounterStats = (JsonTypeInfo<List<EncounterStats>>)base.Options.GetTypeInfo(typeof(List<EncounterStats>)));

	public JsonTypeInfo<List<EnemyStats>> ListEnemyStats => _ListEnemyStats ?? (_ListEnemyStats = (JsonTypeInfo<List<EnemyStats>>)base.Options.GetTypeInfo(typeof(List<EnemyStats>)));

	public JsonTypeInfo<List<FightStats>> ListFightStats => _ListFightStats ?? (_ListFightStats = (JsonTypeInfo<List<FightStats>>)base.Options.GetTypeInfo(typeof(List<FightStats>)));

	public JsonTypeInfo<List<SerializableMapDrawingLine>> ListSerializableMapDrawingLine => _ListSerializableMapDrawingLine ?? (_ListSerializableMapDrawingLine = (JsonTypeInfo<List<SerializableMapDrawingLine>>)base.Options.GetTypeInfo(typeof(List<SerializableMapDrawingLine>)));

	public JsonTypeInfo<List<SerializablePlayerMapDrawings>> ListSerializablePlayerMapDrawings => _ListSerializablePlayerMapDrawings ?? (_ListSerializablePlayerMapDrawings = (JsonTypeInfo<List<SerializablePlayerMapDrawings>>)base.Options.GetTypeInfo(typeof(List<SerializablePlayerMapDrawings>)));

	public JsonTypeInfo<List<MigratingData>> ListMigratingData => _ListMigratingData ?? (_ListMigratingData = (JsonTypeInfo<List<MigratingData>>)base.Options.GetTypeInfo(typeof(List<MigratingData>)));

	public JsonTypeInfo<List<SavedProperties.SavedProperty<bool>>> ListSavedPropertyBoolean => _ListSavedPropertyBoolean ?? (_ListSavedPropertyBoolean = (JsonTypeInfo<List<SavedProperties.SavedProperty<bool>>>)base.Options.GetTypeInfo(typeof(List<SavedProperties.SavedProperty<bool>>)));

	public JsonTypeInfo<List<SavedProperties.SavedProperty<ModelId>>> ListSavedPropertyModelId => _ListSavedPropertyModelId ?? (_ListSavedPropertyModelId = (JsonTypeInfo<List<SavedProperties.SavedProperty<ModelId>>>)base.Options.GetTypeInfo(typeof(List<SavedProperties.SavedProperty<ModelId>>)));

	public JsonTypeInfo<List<SavedProperties.SavedProperty<SerializableCard[]>>> ListSavedPropertySerializableCardArray => _ListSavedPropertySerializableCardArray ?? (_ListSavedPropertySerializableCardArray = (JsonTypeInfo<List<SavedProperties.SavedProperty<SerializableCard[]>>>)base.Options.GetTypeInfo(typeof(List<SavedProperties.SavedProperty<SerializableCard[]>>)));

	public JsonTypeInfo<List<SavedProperties.SavedProperty<SerializableCard>>> ListSavedPropertySerializableCard => _ListSavedPropertySerializableCard ?? (_ListSavedPropertySerializableCard = (JsonTypeInfo<List<SavedProperties.SavedProperty<SerializableCard>>>)base.Options.GetTypeInfo(typeof(List<SavedProperties.SavedProperty<SerializableCard>>)));

	public JsonTypeInfo<List<SavedProperties.SavedProperty<int[]>>> ListSavedPropertyInt32Array => _ListSavedPropertyInt32Array ?? (_ListSavedPropertyInt32Array = (JsonTypeInfo<List<SavedProperties.SavedProperty<int[]>>>)base.Options.GetTypeInfo(typeof(List<SavedProperties.SavedProperty<int[]>>)));

	public JsonTypeInfo<List<SavedProperties.SavedProperty<int>>> ListSavedPropertyInt32 => _ListSavedPropertyInt32 ?? (_ListSavedPropertyInt32 = (JsonTypeInfo<List<SavedProperties.SavedProperty<int>>>)base.Options.GetTypeInfo(typeof(List<SavedProperties.SavedProperty<int>>)));

	public JsonTypeInfo<List<SavedProperties.SavedProperty<string>>> ListSavedPropertyString => _ListSavedPropertyString ?? (_ListSavedPropertyString = (JsonTypeInfo<List<SavedProperties.SavedProperty<string>>>)base.Options.GetTypeInfo(typeof(List<SavedProperties.SavedProperty<string>>)));

	public JsonTypeInfo<List<SerializableActModel>> ListSerializableActModel => _ListSerializableActModel ?? (_ListSerializableActModel = (JsonTypeInfo<List<SerializableActModel>>)base.Options.GetTypeInfo(typeof(List<SerializableActModel>)));

	public JsonTypeInfo<List<SerializableCard>> ListSerializableCard => _ListSerializableCard ?? (_ListSerializableCard = (JsonTypeInfo<List<SerializableCard>>)base.Options.GetTypeInfo(typeof(List<SerializableCard>)));

	public JsonTypeInfo<List<SerializableMapPoint>> ListSerializableMapPoint => _ListSerializableMapPoint ?? (_ListSerializableMapPoint = (JsonTypeInfo<List<SerializableMapPoint>>)base.Options.GetTypeInfo(typeof(List<SerializableMapPoint>)));

	public JsonTypeInfo<List<SerializableModifier>> ListSerializableModifier => _ListSerializableModifier ?? (_ListSerializableModifier = (JsonTypeInfo<List<SerializableModifier>>)base.Options.GetTypeInfo(typeof(List<SerializableModifier>)));

	public JsonTypeInfo<List<SerializablePlayer>> ListSerializablePlayer => _ListSerializablePlayer ?? (_ListSerializablePlayer = (JsonTypeInfo<List<SerializablePlayer>>)base.Options.GetTypeInfo(typeof(List<SerializablePlayer>)));

	public JsonTypeInfo<List<SerializablePotion>> ListSerializablePotion => _ListSerializablePotion ?? (_ListSerializablePotion = (JsonTypeInfo<List<SerializablePotion>>)base.Options.GetTypeInfo(typeof(List<SerializablePotion>)));

	public JsonTypeInfo<List<SerializableRelic>> ListSerializableRelic => _ListSerializableRelic ?? (_ListSerializableRelic = (JsonTypeInfo<List<SerializableRelic>>)base.Options.GetTypeInfo(typeof(List<SerializableRelic>)));

	public JsonTypeInfo<List<SerializableReward>> ListSerializableReward => _ListSerializableReward ?? (_ListSerializableReward = (JsonTypeInfo<List<SerializableReward>>)base.Options.GetTypeInfo(typeof(List<SerializableReward>)));

	public JsonTypeInfo<List<SerializableEpoch>> ListSerializableEpoch => _ListSerializableEpoch ?? (_ListSerializableEpoch = (JsonTypeInfo<List<SerializableEpoch>>)base.Options.GetTypeInfo(typeof(List<SerializableEpoch>)));

	public JsonTypeInfo<List<SerializableUnlockedAchievement>> ListSerializableUnlockedAchievement => _ListSerializableUnlockedAchievement ?? (_ListSerializableUnlockedAchievement = (JsonTypeInfo<List<SerializableUnlockedAchievement>>)base.Options.GetTypeInfo(typeof(List<SerializableUnlockedAchievement>)));

	public JsonTypeInfo<List<Dictionary<string, object>>> ListDictionaryStringObject => _ListDictionaryStringObject ?? (_ListDictionaryStringObject = (JsonTypeInfo<List<Dictionary<string, object>>>)base.Options.GetTypeInfo(typeof(List<Dictionary<string, object>>)));

	public JsonTypeInfo<List<List<MapPointHistoryEntry>>> ListListMapPointHistoryEntry => _ListListMapPointHistoryEntry ?? (_ListListMapPointHistoryEntry = (JsonTypeInfo<List<List<MapPointHistoryEntry>>>)base.Options.GetTypeInfo(typeof(List<List<MapPointHistoryEntry>>)));

	public JsonTypeInfo<List<List<PlayerMapPointHistoryEntry>>> ListListPlayerMapPointHistoryEntry => _ListListPlayerMapPointHistoryEntry ?? (_ListListPlayerMapPointHistoryEntry = (JsonTypeInfo<List<List<PlayerMapPointHistoryEntry>>>)base.Options.GetTypeInfo(typeof(List<List<PlayerMapPointHistoryEntry>>)));

	public JsonTypeInfo<List<JsonNode>> ListJsonNode => _ListJsonNode ?? (_ListJsonNode = (JsonTypeInfo<List<JsonNode>>)base.Options.GetTypeInfo(typeof(List<JsonNode>)));

	public JsonTypeInfo<List<string>> ListString => _ListString ?? (_ListString = (JsonTypeInfo<List<string>>)base.Options.GetTypeInfo(typeof(List<string>)));

	public JsonTypeInfo<List<ulong>> ListUInt64 => _ListUInt64 ?? (_ListUInt64 = (JsonTypeInfo<List<ulong>>)base.Options.GetTypeInfo(typeof(List<ulong>)));

	public JsonTypeInfo<DateTimeOffset> DateTimeOffset => _DateTimeOffset ?? (_DateTimeOffset = (JsonTypeInfo<DateTimeOffset>)base.Options.GetTypeInfo(typeof(DateTimeOffset)));

	public JsonTypeInfo<DateTimeOffset?> NullableDateTimeOffset => _NullableDateTimeOffset ?? (_NullableDateTimeOffset = (JsonTypeInfo<DateTimeOffset?>)base.Options.GetTypeInfo(typeof(DateTimeOffset?)));

	public JsonTypeInfo<JsonNode> JsonNode => _JsonNode ?? (_JsonNode = (JsonTypeInfo<JsonNode>)base.Options.GetTypeInfo(typeof(JsonNode)));

	public JsonTypeInfo<JsonObject> JsonObject => _JsonObject ?? (_JsonObject = (JsonTypeInfo<JsonObject>)base.Options.GetTypeInfo(typeof(JsonObject)));

	public JsonTypeInfo<int> Int32 => _Int32 ?? (_Int32 = (JsonTypeInfo<int>)base.Options.GetTypeInfo(typeof(int)));

	public JsonTypeInfo<int?> NullableInt32 => _NullableInt32 ?? (_NullableInt32 = (JsonTypeInfo<int?>)base.Options.GetTypeInfo(typeof(int?)));

	public JsonTypeInfo<int[]> Int32Array => _Int32Array ?? (_Int32Array = (JsonTypeInfo<int[]>)base.Options.GetTypeInfo(typeof(int[])));

	public JsonTypeInfo<long> Int64 => _Int64 ?? (_Int64 = (JsonTypeInfo<long>)base.Options.GetTypeInfo(typeof(long)));

	public JsonTypeInfo<object> Object => _Object ?? (_Object = (JsonTypeInfo<object>)base.Options.GetTypeInfo(typeof(object)));

	public JsonTypeInfo<string> String => _String ?? (_String = (JsonTypeInfo<string>)base.Options.GetTypeInfo(typeof(string)));

	public JsonTypeInfo<uint> UInt32 => _UInt32 ?? (_UInt32 = (JsonTypeInfo<uint>)base.Options.GetTypeInfo(typeof(uint)));

	public JsonTypeInfo<ulong> UInt64 => _UInt64 ?? (_UInt64 = (JsonTypeInfo<ulong>)base.Options.GetTypeInfo(typeof(ulong)));

	public static MegaCritSerializerContext Default { get; } = new MegaCritSerializerContext(new JsonSerializerOptions(s_defaultOptions));

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

	private JsonTypeInfo<double> Create_Double(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<double> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<double>(options, JsonMetadataServices.DoubleConverter);
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

	private JsonTypeInfo<Vector2> Create_Vector2(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<Vector2> jsonTypeInfo))
		{
			JsonObjectInfoValues<Vector2> objectInfo = new JsonObjectInfoValues<Vector2>
			{
				ObjectCreator = () => default(Vector2),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => Vector2PropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(Vector2).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] Vector2PropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<float> jsonPropertyInfoValues = new JsonPropertyInfoValues<float>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(Vector2);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((Vector2)obj).X;
		jsonPropertyInfoValues.Setter = delegate(object obj, float value)
		{
			Unsafe.Unbox<Vector2>(obj).X = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "X";
		jsonPropertyInfoValues.JsonPropertyName = null;
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(Vector2).GetField("X", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<float> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		jsonPropertyInfoValues = new JsonPropertyInfoValues<float>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(Vector2);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((Vector2)obj).Y;
		jsonPropertyInfoValues.Setter = delegate(object obj, float value)
		{
			Unsafe.Unbox<Vector2>(obj).Y = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Y";
		jsonPropertyInfoValues.JsonPropertyName = null;
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(Vector2).GetField("Y", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<float> propertyInfo2 = jsonPropertyInfoValues;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		return array;
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
				SerializeHandler = null
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

	private JsonTypeInfo<ControllerMappingType> Create_ControllerMappingType(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<ControllerMappingType> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<ControllerMappingType>(options, JsonMetadataServices.GetEnumConverter<ControllerMappingType>(options));
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<RelicRarity> Create_RelicRarity(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<RelicRarity> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<RelicRarity>(options, JsonMetadataServices.GetEnumConverter<RelicRarity>(options));
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<PlayerRngType> Create_PlayerRngType(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<PlayerRngType> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<PlayerRngType>(options, JsonMetadataServices.GetEnumConverter<PlayerRngType>(options));
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<RunRngType> Create_RunRngType(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<RunRngType> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<RunRngType>(options, JsonMetadataServices.GetEnumConverter<RunRngType>(options));
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<LocString> Create_LocString(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<LocString> jsonTypeInfo))
		{
			JsonObjectInfoValues<LocString> jsonObjectInfoValues = new JsonObjectInfoValues<LocString>();
			jsonObjectInfoValues.ObjectCreator = null;
			jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = (object[] args) => new LocString((string)args[0], (string)args[1]);
			jsonObjectInfoValues.PropertyMetadataInitializer = (JsonSerializerContext _) => LocStringPropInit(options);
			jsonObjectInfoValues.ConstructorParameterMetadataInitializer = LocStringCtorParamInit;
			jsonObjectInfoValues.ConstructorAttributeProviderFactory = () => typeof(LocString).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[2]
			{
				typeof(string),
				typeof(string)
			}, null);
			jsonObjectInfoValues.SerializeHandler = null;
			JsonObjectInfoValues<LocString> objectInfo = jsonObjectInfoValues;
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] LocStringPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[4];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(LocString);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((LocString)obj).LocTable;
		jsonPropertyInfoValues.Setter = null;
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "LocTable";
		jsonPropertyInfoValues.JsonPropertyName = "table";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(LocString).GetProperty("LocTable", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsGetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(LocString);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((LocString)obj).LocEntryKey;
		jsonPropertyInfoValues.Setter = null;
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "LocEntryKey";
		jsonPropertyInfoValues.JsonPropertyName = "key";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(LocString).GetProperty("LocEntryKey", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo2 = jsonPropertyInfoValues;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsGetNullable = false;
		JsonPropertyInfoValues<bool> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(LocString);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = null;
		jsonPropertyInfoValues2.Setter = null;
		jsonPropertyInfoValues2.IgnoreCondition = JsonIgnoreCondition.Always;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "IsEmpty";
		jsonPropertyInfoValues2.JsonPropertyName = null;
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(LocString).GetProperty("IsEmpty", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<bool> propertyInfo3 = jsonPropertyInfoValues2;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		JsonPropertyInfoValues<IReadOnlyDictionary<string, object>> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<IReadOnlyDictionary<string, object>>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(LocString);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = null;
		jsonPropertyInfoValues3.Setter = null;
		jsonPropertyInfoValues3.IgnoreCondition = JsonIgnoreCondition.Always;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "Variables";
		jsonPropertyInfoValues3.JsonPropertyName = null;
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(LocString).GetProperty("Variables", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(IReadOnlyDictionary<string, object>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<IReadOnlyDictionary<string, object>> propertyInfo4 = jsonPropertyInfoValues3;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		array[3].IsGetNullable = false;
		return array;
	}

	private static JsonParameterInfoValues[] LocStringCtorParamInit()
	{
		return new JsonParameterInfoValues[2]
		{
			new JsonParameterInfoValues
			{
				Name = "locTable",
				ParameterType = typeof(string),
				Position = 0,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			},
			new JsonParameterInfoValues
			{
				Name = "locEntryKey",
				ParameterType = typeof(string),
				Position = 1,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			}
		};
	}

	private JsonTypeInfo<MapCoord> Create_MapCoord(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<MapCoord> jsonTypeInfo))
		{
			JsonObjectInfoValues<MapCoord> objectInfo = new JsonObjectInfoValues<MapCoord>
			{
				ObjectCreator = () => default(MapCoord),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => MapCoordPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(MapCoord).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] MapCoordPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<int> jsonPropertyInfoValues = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(MapCoord);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((MapCoord)obj).col;
		jsonPropertyInfoValues.Setter = delegate(object obj, int value)
		{
			Unsafe.Unbox<MapCoord>(obj).col = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = true;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "col";
		jsonPropertyInfoValues.JsonPropertyName = null;
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(MapCoord).GetField("col", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<int> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		jsonPropertyInfoValues = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(MapCoord);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((MapCoord)obj).row;
		jsonPropertyInfoValues.Setter = delegate(object obj, int value)
		{
			Unsafe.Unbox<MapCoord>(obj).row = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = true;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "row";
		jsonPropertyInfoValues.JsonPropertyName = null;
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(MapCoord).GetField("row", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<int> propertyInfo2 = jsonPropertyInfoValues;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		return array;
	}

	private JsonTypeInfo<MapPointType> Create_MapPointType(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<MapPointType> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<MapPointType>(options, JsonMetadataServices.GetEnumConverter<MapPointType>(options));
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<DisabledMod> Create_DisabledMod(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<DisabledMod> jsonTypeInfo))
		{
			JsonObjectInfoValues<DisabledMod> objectInfo = new JsonObjectInfoValues<DisabledMod>
			{
				ObjectCreator = () => new DisabledMod(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => DisabledModPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(DisabledMod).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] DisabledModPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(DisabledMod);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((DisabledMod)obj).Name;
		jsonPropertyInfoValues.Setter = delegate(object obj, string? value)
		{
			((DisabledMod)obj).Name = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Name";
		jsonPropertyInfoValues.JsonPropertyName = "name";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(DisabledMod).GetProperty("Name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		JsonPropertyInfoValues<ModSource> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<ModSource>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(DisabledMod);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((DisabledMod)obj).Source;
		jsonPropertyInfoValues2.Setter = delegate(object obj, ModSource value)
		{
			((DisabledMod)obj).Source = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "Source";
		jsonPropertyInfoValues2.JsonPropertyName = "source";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(DisabledMod).GetProperty("Source", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModSource), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModSource> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		return array;
	}

	private JsonTypeInfo<ModManifest> Create_ModManifest(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<ModManifest> jsonTypeInfo))
		{
			JsonObjectInfoValues<ModManifest> objectInfo = new JsonObjectInfoValues<ModManifest>
			{
				ObjectCreator = () => new ModManifest(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => ModManifestPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(ModManifest).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] ModManifestPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[5];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(ModManifest);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((ModManifest)obj).pckName;
		jsonPropertyInfoValues.Setter = delegate(object obj, string? value)
		{
			((ModManifest)obj).pckName = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "pckName";
		jsonPropertyInfoValues.JsonPropertyName = "pck_name";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(ModManifest).GetField("pckName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(ModManifest);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((ModManifest)obj).name;
		jsonPropertyInfoValues.Setter = delegate(object obj, string? value)
		{
			((ModManifest)obj).name = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "name";
		jsonPropertyInfoValues.JsonPropertyName = "name";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(ModManifest).GetField("name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<string> propertyInfo2 = jsonPropertyInfoValues;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(ModManifest);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((ModManifest)obj).author;
		jsonPropertyInfoValues.Setter = delegate(object obj, string? value)
		{
			((ModManifest)obj).author = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "author";
		jsonPropertyInfoValues.JsonPropertyName = "author";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(ModManifest).GetField("author", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<string> propertyInfo3 = jsonPropertyInfoValues;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(ModManifest);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((ModManifest)obj).description;
		jsonPropertyInfoValues.Setter = delegate(object obj, string? value)
		{
			((ModManifest)obj).description = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "description";
		jsonPropertyInfoValues.JsonPropertyName = "description";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(ModManifest).GetField("description", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<string> propertyInfo4 = jsonPropertyInfoValues;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(ModManifest);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((ModManifest)obj).version;
		jsonPropertyInfoValues.Setter = delegate(object obj, string? value)
		{
			((ModManifest)obj).version = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "version";
		jsonPropertyInfoValues.JsonPropertyName = "version";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(ModManifest).GetField("version", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<string> propertyInfo5 = jsonPropertyInfoValues;
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		return array;
	}

	private JsonTypeInfo<ModSettings> Create_ModSettings(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<ModSettings> jsonTypeInfo))
		{
			JsonObjectInfoValues<ModSettings> objectInfo = new JsonObjectInfoValues<ModSettings>
			{
				ObjectCreator = () => new ModSettings(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => ModSettingsPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(ModSettings).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] ModSettingsPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<bool> jsonPropertyInfoValues = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(ModSettings);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((ModSettings)obj).PlayerAgreedToModLoading;
		jsonPropertyInfoValues.Setter = delegate(object obj, bool value)
		{
			((ModSettings)obj).PlayerAgreedToModLoading = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "PlayerAgreedToModLoading";
		jsonPropertyInfoValues.JsonPropertyName = "mods_enabled";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(ModSettings).GetProperty("PlayerAgreedToModLoading", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<bool> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<List<DisabledMod>> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<List<DisabledMod>>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(ModSettings);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((ModSettings)obj).DisabledMods;
		jsonPropertyInfoValues2.Setter = delegate(object obj, List<DisabledMod>? value)
		{
			((ModSettings)obj).DisabledMods = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "DisabledMods";
		jsonPropertyInfoValues2.JsonPropertyName = "disabled_mods";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(ModSettings).GetProperty("DisabledMods", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<DisabledMod>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<DisabledMod>> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		return array;
	}

	private JsonTypeInfo<ModSource> Create_ModSource(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<ModSource> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<ModSource>(options, JsonMetadataServices.GetEnumConverter<ModSource>(options));
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
			jsonObjectInfoValues.SerializeHandler = null;
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

	private JsonTypeInfo<FeedbackData> Create_FeedbackData(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<FeedbackData> jsonTypeInfo))
		{
			JsonObjectInfoValues<FeedbackData> objectInfo = new JsonObjectInfoValues<FeedbackData>
			{
				ObjectCreator = () => default(FeedbackData),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => FeedbackDataPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(FeedbackData).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] FeedbackDataPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[6];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(FeedbackData);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((FeedbackData)obj).description;
		jsonPropertyInfoValues.Setter = delegate(object obj, string? value)
		{
			Unsafe.Unbox<FeedbackData>(obj).description = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "description";
		jsonPropertyInfoValues.JsonPropertyName = "description";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(FeedbackData).GetField("description", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(FeedbackData);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((FeedbackData)obj).category;
		jsonPropertyInfoValues.Setter = delegate(object obj, string? value)
		{
			Unsafe.Unbox<FeedbackData>(obj).category = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "category";
		jsonPropertyInfoValues.JsonPropertyName = "category";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(FeedbackData).GetField("category", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<string> propertyInfo2 = jsonPropertyInfoValues;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(FeedbackData);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((FeedbackData)obj).gameVersion;
		jsonPropertyInfoValues.Setter = delegate(object obj, string? value)
		{
			Unsafe.Unbox<FeedbackData>(obj).gameVersion = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "gameVersion";
		jsonPropertyInfoValues.JsonPropertyName = "game_version";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(FeedbackData).GetField("gameVersion", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<string> propertyInfo3 = jsonPropertyInfoValues;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		array[2].IsGetNullable = false;
		array[2].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(FeedbackData);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((FeedbackData)obj).uniqueId;
		jsonPropertyInfoValues.Setter = delegate(object obj, string? value)
		{
			Unsafe.Unbox<FeedbackData>(obj).uniqueId = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "uniqueId";
		jsonPropertyInfoValues.JsonPropertyName = "unique_id";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(FeedbackData).GetField("uniqueId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<string> propertyInfo4 = jsonPropertyInfoValues;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		array[3].IsGetNullable = false;
		array[3].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(FeedbackData);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((FeedbackData)obj).commit;
		jsonPropertyInfoValues.Setter = delegate(object obj, string? value)
		{
			Unsafe.Unbox<FeedbackData>(obj).commit = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "commit";
		jsonPropertyInfoValues.JsonPropertyName = "commit";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(FeedbackData).GetField("commit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<string> propertyInfo5 = jsonPropertyInfoValues;
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		array[4].IsGetNullable = false;
		array[4].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(FeedbackData);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((FeedbackData)obj).platformBranch;
		jsonPropertyInfoValues.Setter = delegate(object obj, string? value)
		{
			Unsafe.Unbox<FeedbackData>(obj).platformBranch = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "platformBranch";
		jsonPropertyInfoValues.JsonPropertyName = "platform_branch";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(FeedbackData).GetField("platformBranch", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<string> propertyInfo6 = jsonPropertyInfoValues;
		array[5] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo6);
		return array;
	}

	private JsonTypeInfo<NullLeaderboard> Create_NullLeaderboard(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<NullLeaderboard> jsonTypeInfo))
		{
			JsonObjectInfoValues<NullLeaderboard> objectInfo = new JsonObjectInfoValues<NullLeaderboard>
			{
				ObjectCreator = () => new NullLeaderboard(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => NullLeaderboardPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(NullLeaderboard).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] NullLeaderboardPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(NullLeaderboard);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((NullLeaderboard)obj).name;
		jsonPropertyInfoValues.Setter = delegate(object obj, string? value)
		{
			((NullLeaderboard)obj).name = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "name";
		jsonPropertyInfoValues.JsonPropertyName = "name";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(NullLeaderboard).GetField("name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		JsonPropertyInfoValues<List<NullLeaderboardFileEntry>> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<List<NullLeaderboardFileEntry>>();
		jsonPropertyInfoValues2.IsProperty = false;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(NullLeaderboard);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((NullLeaderboard)obj).entries;
		jsonPropertyInfoValues2.Setter = delegate(object obj, List<NullLeaderboardFileEntry>? value)
		{
			((NullLeaderboard)obj).entries = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "entries";
		jsonPropertyInfoValues2.JsonPropertyName = "entries";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(NullLeaderboard).GetField("entries", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<List<NullLeaderboardFileEntry>> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		return array;
	}

	private JsonTypeInfo<NullLeaderboardFile> Create_NullLeaderboardFile(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<NullLeaderboardFile> jsonTypeInfo))
		{
			JsonObjectInfoValues<NullLeaderboardFile> objectInfo = new JsonObjectInfoValues<NullLeaderboardFile>
			{
				ObjectCreator = () => new NullLeaderboardFile(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => NullLeaderboardFilePropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(NullLeaderboardFile).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] NullLeaderboardFilePropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<int> jsonPropertyInfoValues = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(NullLeaderboardFile);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((NullLeaderboardFile)obj).SchemaVersion;
		jsonPropertyInfoValues.Setter = delegate(object obj, int value)
		{
			((NullLeaderboardFile)obj).SchemaVersion = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "SchemaVersion";
		jsonPropertyInfoValues.JsonPropertyName = "version";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(NullLeaderboardFile).GetProperty("SchemaVersion", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<List<NullLeaderboard>> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<List<NullLeaderboard>>();
		jsonPropertyInfoValues2.IsProperty = false;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(NullLeaderboardFile);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((NullLeaderboardFile)obj).leaderboards;
		jsonPropertyInfoValues2.Setter = delegate(object obj, List<NullLeaderboard>? value)
		{
			((NullLeaderboardFile)obj).leaderboards = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "leaderboards";
		jsonPropertyInfoValues2.JsonPropertyName = "leaderboards";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(NullLeaderboardFile).GetField("leaderboards", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<List<NullLeaderboard>> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		return array;
	}

	private JsonTypeInfo<NullLeaderboardFileEntry> Create_NullLeaderboardFileEntry(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<NullLeaderboardFileEntry> jsonTypeInfo))
		{
			JsonObjectInfoValues<NullLeaderboardFileEntry> jsonObjectInfoValues = new JsonObjectInfoValues<NullLeaderboardFileEntry>();
			jsonObjectInfoValues.ObjectCreator = null;
			jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = (object[] args) => new NullLeaderboardFileEntry
			{
				name = (string)args[0]
			};
			jsonObjectInfoValues.PropertyMetadataInitializer = (JsonSerializerContext _) => NullLeaderboardFileEntryPropInit(options);
			jsonObjectInfoValues.ConstructorParameterMetadataInitializer = NullLeaderboardFileEntryCtorParamInit;
			jsonObjectInfoValues.ConstructorAttributeProviderFactory = () => typeof(NullLeaderboardFileEntry).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null);
			jsonObjectInfoValues.SerializeHandler = null;
			JsonObjectInfoValues<NullLeaderboardFileEntry> objectInfo = jsonObjectInfoValues;
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] NullLeaderboardFileEntryPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[4];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(NullLeaderboardFileEntry);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((NullLeaderboardFileEntry)obj).name;
		jsonPropertyInfoValues.Setter = delegate(object obj, string? value)
		{
			((NullLeaderboardFileEntry)obj).name = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "name";
		jsonPropertyInfoValues.JsonPropertyName = "name";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(NullLeaderboardFileEntry).GetField("name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsRequired = true;
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		JsonPropertyInfoValues<int> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = false;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(NullLeaderboardFileEntry);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((NullLeaderboardFileEntry)obj).score;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((NullLeaderboardFileEntry)obj).score = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "score";
		jsonPropertyInfoValues2.JsonPropertyName = "score";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(NullLeaderboardFileEntry).GetField("score", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<int> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<ulong> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<ulong>();
		jsonPropertyInfoValues3.IsProperty = false;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(NullLeaderboardFileEntry);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((NullLeaderboardFileEntry)obj).id;
		jsonPropertyInfoValues3.Setter = delegate(object obj, ulong value)
		{
			((NullLeaderboardFileEntry)obj).id = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "id";
		jsonPropertyInfoValues3.JsonPropertyName = "id";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(NullLeaderboardFileEntry).GetField("id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<ulong> propertyInfo3 = jsonPropertyInfoValues3;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		JsonPropertyInfoValues<List<ulong>> jsonPropertyInfoValues4 = new JsonPropertyInfoValues<List<ulong>>();
		jsonPropertyInfoValues4.IsProperty = false;
		jsonPropertyInfoValues4.IsPublic = true;
		jsonPropertyInfoValues4.IsVirtual = false;
		jsonPropertyInfoValues4.DeclaringType = typeof(NullLeaderboardFileEntry);
		jsonPropertyInfoValues4.Converter = null;
		jsonPropertyInfoValues4.Getter = (object obj) => ((NullLeaderboardFileEntry)obj).userIds;
		jsonPropertyInfoValues4.Setter = delegate(object obj, List<ulong>? value)
		{
			((NullLeaderboardFileEntry)obj).userIds = value;
		};
		jsonPropertyInfoValues4.IgnoreCondition = null;
		jsonPropertyInfoValues4.HasJsonInclude = false;
		jsonPropertyInfoValues4.IsExtensionData = false;
		jsonPropertyInfoValues4.NumberHandling = null;
		jsonPropertyInfoValues4.PropertyName = "userIds";
		jsonPropertyInfoValues4.JsonPropertyName = "other_ids";
		jsonPropertyInfoValues4.AttributeProviderFactory = () => typeof(NullLeaderboardFileEntry).GetField("userIds", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<List<ulong>> propertyInfo4 = jsonPropertyInfoValues4;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		array[3].IsGetNullable = false;
		array[3].IsSetNullable = false;
		return array;
	}

	private static JsonParameterInfoValues[] NullLeaderboardFileEntryCtorParamInit()
	{
		return new JsonParameterInfoValues[1]
		{
			new JsonParameterInfoValues
			{
				Name = "name",
				ParameterType = typeof(string),
				Position = 0,
				IsNullable = false,
				IsMemberInitializer = true
			}
		};
	}

	private JsonTypeInfo<NullMultiplayerName> Create_NullMultiplayerName(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<NullMultiplayerName> jsonTypeInfo))
		{
			JsonObjectInfoValues<NullMultiplayerName> objectInfo = new JsonObjectInfoValues<NullMultiplayerName>
			{
				ObjectCreator = () => new NullMultiplayerName(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => NullMultiplayerNamePropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(NullMultiplayerName).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] NullMultiplayerNamePropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<ulong> jsonPropertyInfoValues = new JsonPropertyInfoValues<ulong>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(NullMultiplayerName);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((NullMultiplayerName)obj).netId;
		jsonPropertyInfoValues.Setter = delegate(object obj, ulong value)
		{
			((NullMultiplayerName)obj).netId = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "netId";
		jsonPropertyInfoValues.JsonPropertyName = "net_id";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(NullMultiplayerName).GetField("netId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<ulong> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<string> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues2.IsProperty = false;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(NullMultiplayerName);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((NullMultiplayerName)obj).name;
		jsonPropertyInfoValues2.Setter = delegate(object obj, string? value)
		{
			((NullMultiplayerName)obj).name = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "name";
		jsonPropertyInfoValues2.JsonPropertyName = "name";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(NullMultiplayerName).GetField("name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<string> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		return array;
	}

	private JsonTypeInfo<PlatformType> Create_PlatformType(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<PlatformType> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<PlatformType>(options, JsonMetadataServices.GetEnumConverter<PlatformType>(options));
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<RewardType> Create_RewardType(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<RewardType> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<RewardType>(options, JsonMetadataServices.GetEnumConverter<RewardType>(options));
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<RoomType> Create_RoomType(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<RoomType> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<RoomType>(options, JsonMetadataServices.GetEnumConverter<RoomType>(options));
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<CardCreationSource> Create_CardCreationSource(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<CardCreationSource> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<CardCreationSource>(options, JsonMetadataServices.GetEnumConverter<CardCreationSource>(options));
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<CardRarityOddsType> Create_CardRarityOddsType(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<CardRarityOddsType> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<CardRarityOddsType>(options, JsonMetadataServices.GetEnumConverter<CardRarityOddsType>(options));
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<GameMode> Create_GameMode(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<GameMode> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<GameMode>(options, JsonMetadataServices.GetEnumConverter<GameMode>(options));
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<AncientChoiceHistoryEntry> Create_AncientChoiceHistoryEntry(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<AncientChoiceHistoryEntry> jsonTypeInfo))
		{
			JsonObjectInfoValues<AncientChoiceHistoryEntry> objectInfo = new JsonObjectInfoValues<AncientChoiceHistoryEntry>
			{
				ObjectCreator = () => new AncientChoiceHistoryEntry(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => AncientChoiceHistoryEntryPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(AncientChoiceHistoryEntry).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] AncientChoiceHistoryEntryPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[3];
		JsonPropertyInfoValues<LocString> jsonPropertyInfoValues = new JsonPropertyInfoValues<LocString>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(AncientChoiceHistoryEntry);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((AncientChoiceHistoryEntry)obj).Title;
		jsonPropertyInfoValues.Setter = delegate(object obj, LocString? value)
		{
			((AncientChoiceHistoryEntry)obj).Title = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Title";
		jsonPropertyInfoValues.JsonPropertyName = "title";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(AncientChoiceHistoryEntry).GetProperty("Title", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(LocString), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<LocString> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		JsonPropertyInfoValues<bool> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(AncientChoiceHistoryEntry);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((AncientChoiceHistoryEntry)obj).WasChosen;
		jsonPropertyInfoValues2.Setter = delegate(object obj, bool value)
		{
			((AncientChoiceHistoryEntry)obj).WasChosen = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "WasChosen";
		jsonPropertyInfoValues2.JsonPropertyName = "was_chosen";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(AncientChoiceHistoryEntry).GetProperty("WasChosen", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<bool> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<string> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(AncientChoiceHistoryEntry);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((AncientChoiceHistoryEntry)obj).TextKey;
		jsonPropertyInfoValues3.Setter = null;
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "TextKey";
		jsonPropertyInfoValues3.JsonPropertyName = "TextKey";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(AncientChoiceHistoryEntry).GetProperty("TextKey", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo3 = jsonPropertyInfoValues3;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		array[2].IsGetNullable = false;
		return array;
	}

	private JsonTypeInfo<CardChoiceHistoryEntry> Create_CardChoiceHistoryEntry(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<CardChoiceHistoryEntry> jsonTypeInfo))
		{
			JsonObjectInfoValues<CardChoiceHistoryEntry> objectInfo = new JsonObjectInfoValues<CardChoiceHistoryEntry>
			{
				ObjectCreator = () => default(CardChoiceHistoryEntry),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => CardChoiceHistoryEntryPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(CardChoiceHistoryEntry).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] CardChoiceHistoryEntryPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<SerializableCard> jsonPropertyInfoValues = new JsonPropertyInfoValues<SerializableCard>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(CardChoiceHistoryEntry);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((CardChoiceHistoryEntry)obj).Card;
		jsonPropertyInfoValues.Setter = delegate(object obj, SerializableCard? value)
		{
			Unsafe.Unbox<CardChoiceHistoryEntry>(obj).Card = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Card";
		jsonPropertyInfoValues.JsonPropertyName = "card";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(CardChoiceHistoryEntry).GetProperty("Card", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(SerializableCard), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<SerializableCard> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		JsonPropertyInfoValues<bool> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues2.IsProperty = false;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(CardChoiceHistoryEntry);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((CardChoiceHistoryEntry)obj).wasPicked;
		jsonPropertyInfoValues2.Setter = delegate(object obj, bool value)
		{
			Unsafe.Unbox<CardChoiceHistoryEntry>(obj).wasPicked = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "wasPicked";
		jsonPropertyInfoValues2.JsonPropertyName = "was_picked";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(CardChoiceHistoryEntry).GetField("wasPicked", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<bool> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		return array;
	}

	private JsonTypeInfo<CardEnchantmentHistoryEntry> Create_CardEnchantmentHistoryEntry(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<CardEnchantmentHistoryEntry> jsonTypeInfo))
		{
			JsonObjectInfoValues<CardEnchantmentHistoryEntry> objectInfo = new JsonObjectInfoValues<CardEnchantmentHistoryEntry>
			{
				ObjectCreator = () => default(CardEnchantmentHistoryEntry),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => CardEnchantmentHistoryEntryPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(CardEnchantmentHistoryEntry).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] CardEnchantmentHistoryEntryPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<SerializableCard> jsonPropertyInfoValues = new JsonPropertyInfoValues<SerializableCard>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(CardEnchantmentHistoryEntry);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((CardEnchantmentHistoryEntry)obj).Card;
		jsonPropertyInfoValues.Setter = delegate(object obj, SerializableCard? value)
		{
			Unsafe.Unbox<CardEnchantmentHistoryEntry>(obj).Card = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Card";
		jsonPropertyInfoValues.JsonPropertyName = "card";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(CardEnchantmentHistoryEntry).GetProperty("Card", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(SerializableCard), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<SerializableCard> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		JsonPropertyInfoValues<ModelId> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(CardEnchantmentHistoryEntry);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((CardEnchantmentHistoryEntry)obj).Enchantment;
		jsonPropertyInfoValues2.Setter = delegate(object obj, ModelId? value)
		{
			Unsafe.Unbox<CardEnchantmentHistoryEntry>(obj).Enchantment = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "Enchantment";
		jsonPropertyInfoValues2.JsonPropertyName = "enchantment";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(CardEnchantmentHistoryEntry).GetProperty("Enchantment", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModelId), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModelId> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		return array;
	}

	private JsonTypeInfo<CardTransformationHistoryEntry> Create_CardTransformationHistoryEntry(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<CardTransformationHistoryEntry> jsonTypeInfo))
		{
			JsonObjectInfoValues<CardTransformationHistoryEntry> objectInfo = new JsonObjectInfoValues<CardTransformationHistoryEntry>
			{
				ObjectCreator = () => default(CardTransformationHistoryEntry),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => CardTransformationHistoryEntryPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(CardTransformationHistoryEntry).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] CardTransformationHistoryEntryPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<SerializableCard> jsonPropertyInfoValues = new JsonPropertyInfoValues<SerializableCard>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(CardTransformationHistoryEntry);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((CardTransformationHistoryEntry)obj).OriginalCard;
		jsonPropertyInfoValues.Setter = delegate(object obj, SerializableCard? value)
		{
			Unsafe.Unbox<CardTransformationHistoryEntry>(obj).OriginalCard = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "OriginalCard";
		jsonPropertyInfoValues.JsonPropertyName = "original_card";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(CardTransformationHistoryEntry).GetProperty("OriginalCard", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(SerializableCard), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<SerializableCard> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<SerializableCard>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(CardTransformationHistoryEntry);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((CardTransformationHistoryEntry)obj).FinalCard;
		jsonPropertyInfoValues.Setter = delegate(object obj, SerializableCard? value)
		{
			Unsafe.Unbox<CardTransformationHistoryEntry>(obj).FinalCard = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "FinalCard";
		jsonPropertyInfoValues.JsonPropertyName = "final_card";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(CardTransformationHistoryEntry).GetProperty("FinalCard", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(SerializableCard), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<SerializableCard> propertyInfo2 = jsonPropertyInfoValues;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		return array;
	}

	private JsonTypeInfo<EventOptionHistoryEntry> Create_EventOptionHistoryEntry(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<EventOptionHistoryEntry> jsonTypeInfo))
		{
			JsonObjectInfoValues<EventOptionHistoryEntry> objectInfo = new JsonObjectInfoValues<EventOptionHistoryEntry>
			{
				ObjectCreator = () => default(EventOptionHistoryEntry),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => EventOptionHistoryEntryPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(EventOptionHistoryEntry).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] EventOptionHistoryEntryPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<LocString> jsonPropertyInfoValues = new JsonPropertyInfoValues<LocString>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(EventOptionHistoryEntry);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((EventOptionHistoryEntry)obj).Title;
		jsonPropertyInfoValues.Setter = delegate(object obj, LocString? value)
		{
			Unsafe.Unbox<EventOptionHistoryEntry>(obj).Title = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Title";
		jsonPropertyInfoValues.JsonPropertyName = "title";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(EventOptionHistoryEntry).GetProperty("Title", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(LocString), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<LocString> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		JsonPropertyInfoValues<Dictionary<string, object>> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<Dictionary<string, object>>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(EventOptionHistoryEntry);
		jsonPropertyInfoValues2.Converter = (JsonConverter<Dictionary<string, object>>)ExpandConverter(typeof(Dictionary<string, object>), new LocStringVariablesJsonConverter(), options);
		jsonPropertyInfoValues2.Getter = (object obj) => ((EventOptionHistoryEntry)obj).Variables;
		jsonPropertyInfoValues2.Setter = delegate(object obj, Dictionary<string, object>? value)
		{
			Unsafe.Unbox<EventOptionHistoryEntry>(obj).Variables = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = true;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "Variables";
		jsonPropertyInfoValues2.JsonPropertyName = "variables";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(EventOptionHistoryEntry).GetProperty("Variables", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(Dictionary<string, object>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<Dictionary<string, object>> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		return array;
	}

	private JsonTypeInfo<MapPointHistoryEntry> Create_MapPointHistoryEntry(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<MapPointHistoryEntry> jsonTypeInfo))
		{
			JsonObjectInfoValues<MapPointHistoryEntry> objectInfo = new JsonObjectInfoValues<MapPointHistoryEntry>
			{
				ObjectCreator = () => new MapPointHistoryEntry(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => MapPointHistoryEntryPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(MapPointHistoryEntry).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] MapPointHistoryEntryPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[3];
		JsonPropertyInfoValues<MapPointType> jsonPropertyInfoValues = new JsonPropertyInfoValues<MapPointType>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(MapPointHistoryEntry);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((MapPointHistoryEntry)obj).MapPointType;
		jsonPropertyInfoValues.Setter = delegate(object obj, MapPointType value)
		{
			((MapPointHistoryEntry)obj).MapPointType = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "MapPointType";
		jsonPropertyInfoValues.JsonPropertyName = "map_point_type";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(MapPointHistoryEntry).GetProperty("MapPointType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(MapPointType), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<MapPointType> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<List<MapPointRoomHistoryEntry>> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<List<MapPointRoomHistoryEntry>>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(MapPointHistoryEntry);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((MapPointHistoryEntry)obj).Rooms;
		jsonPropertyInfoValues2.Setter = delegate(object obj, List<MapPointRoomHistoryEntry>? value)
		{
			((MapPointHistoryEntry)obj).Rooms = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "Rooms";
		jsonPropertyInfoValues2.JsonPropertyName = "rooms";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(MapPointHistoryEntry).GetProperty("Rooms", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<MapPointRoomHistoryEntry>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<MapPointRoomHistoryEntry>> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		JsonPropertyInfoValues<List<PlayerMapPointHistoryEntry>> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<List<PlayerMapPointHistoryEntry>>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(MapPointHistoryEntry);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((MapPointHistoryEntry)obj).PlayerStats;
		jsonPropertyInfoValues3.Setter = delegate(object obj, List<PlayerMapPointHistoryEntry>? value)
		{
			((MapPointHistoryEntry)obj).PlayerStats = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "PlayerStats";
		jsonPropertyInfoValues3.JsonPropertyName = "player_stats";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(MapPointHistoryEntry).GetProperty("PlayerStats", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<PlayerMapPointHistoryEntry>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<PlayerMapPointHistoryEntry>> propertyInfo3 = jsonPropertyInfoValues3;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		array[2].IsGetNullable = false;
		array[2].IsSetNullable = false;
		return array;
	}

	private JsonTypeInfo<MapPointRoomHistoryEntry> Create_MapPointRoomHistoryEntry(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<MapPointRoomHistoryEntry> jsonTypeInfo))
		{
			JsonObjectInfoValues<MapPointRoomHistoryEntry> objectInfo = new JsonObjectInfoValues<MapPointRoomHistoryEntry>
			{
				ObjectCreator = () => new MapPointRoomHistoryEntry(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => MapPointRoomHistoryEntryPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(MapPointRoomHistoryEntry).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] MapPointRoomHistoryEntryPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[4];
		JsonPropertyInfoValues<RoomType> jsonPropertyInfoValues = new JsonPropertyInfoValues<RoomType>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(MapPointRoomHistoryEntry);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((MapPointRoomHistoryEntry)obj).RoomType;
		jsonPropertyInfoValues.Setter = delegate(object obj, RoomType value)
		{
			((MapPointRoomHistoryEntry)obj).RoomType = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "RoomType";
		jsonPropertyInfoValues.JsonPropertyName = "room_type";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(MapPointRoomHistoryEntry).GetProperty("RoomType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(RoomType), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<RoomType> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<ModelId> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(MapPointRoomHistoryEntry);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((MapPointRoomHistoryEntry)obj).ModelId;
		jsonPropertyInfoValues2.Setter = delegate(object obj, ModelId? value)
		{
			((MapPointRoomHistoryEntry)obj).ModelId = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "ModelId";
		jsonPropertyInfoValues2.JsonPropertyName = "model_id";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(MapPointRoomHistoryEntry).GetProperty("ModelId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModelId), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModelId> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<List<ModelId>> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<List<ModelId>>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(MapPointRoomHistoryEntry);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((MapPointRoomHistoryEntry)obj).MonsterIds;
		jsonPropertyInfoValues3.Setter = delegate(object obj, List<ModelId>? value)
		{
			((MapPointRoomHistoryEntry)obj).MonsterIds = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "MonsterIds";
		jsonPropertyInfoValues3.JsonPropertyName = "monster_ids";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(MapPointRoomHistoryEntry).GetProperty("MonsterIds", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<ModelId>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<ModelId>> propertyInfo3 = jsonPropertyInfoValues3;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		array[2].IsGetNullable = false;
		array[2].IsSetNullable = false;
		JsonPropertyInfoValues<int> jsonPropertyInfoValues4 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues4.IsProperty = true;
		jsonPropertyInfoValues4.IsPublic = true;
		jsonPropertyInfoValues4.IsVirtual = false;
		jsonPropertyInfoValues4.DeclaringType = typeof(MapPointRoomHistoryEntry);
		jsonPropertyInfoValues4.Converter = null;
		jsonPropertyInfoValues4.Getter = (object obj) => ((MapPointRoomHistoryEntry)obj).TurnsTaken;
		jsonPropertyInfoValues4.Setter = delegate(object obj, int value)
		{
			((MapPointRoomHistoryEntry)obj).TurnsTaken = value;
		};
		jsonPropertyInfoValues4.IgnoreCondition = null;
		jsonPropertyInfoValues4.HasJsonInclude = false;
		jsonPropertyInfoValues4.IsExtensionData = false;
		jsonPropertyInfoValues4.NumberHandling = null;
		jsonPropertyInfoValues4.PropertyName = "TurnsTaken";
		jsonPropertyInfoValues4.JsonPropertyName = "turns_taken";
		jsonPropertyInfoValues4.AttributeProviderFactory = () => typeof(MapPointRoomHistoryEntry).GetProperty("TurnsTaken", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo4 = jsonPropertyInfoValues4;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		return array;
	}

	private JsonTypeInfo<ModelChoiceHistoryEntry> Create_ModelChoiceHistoryEntry(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<ModelChoiceHistoryEntry> jsonTypeInfo))
		{
			JsonObjectInfoValues<ModelChoiceHistoryEntry> objectInfo = new JsonObjectInfoValues<ModelChoiceHistoryEntry>
			{
				ObjectCreator = () => default(ModelChoiceHistoryEntry),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => ModelChoiceHistoryEntryPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(ModelChoiceHistoryEntry).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] ModelChoiceHistoryEntryPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<ModelId> jsonPropertyInfoValues = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(ModelChoiceHistoryEntry);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((ModelChoiceHistoryEntry)obj).choice;
		jsonPropertyInfoValues.Setter = delegate(object obj, ModelId? value)
		{
			Unsafe.Unbox<ModelChoiceHistoryEntry>(obj).choice = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "choice";
		jsonPropertyInfoValues.JsonPropertyName = "choice";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(ModelChoiceHistoryEntry).GetField("choice", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<ModelId> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		JsonPropertyInfoValues<bool> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues2.IsProperty = false;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(ModelChoiceHistoryEntry);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((ModelChoiceHistoryEntry)obj).wasPicked;
		jsonPropertyInfoValues2.Setter = delegate(object obj, bool value)
		{
			Unsafe.Unbox<ModelChoiceHistoryEntry>(obj).wasPicked = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "wasPicked";
		jsonPropertyInfoValues2.JsonPropertyName = "was_picked";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(ModelChoiceHistoryEntry).GetField("wasPicked", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<bool> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		return array;
	}

	private JsonTypeInfo<PlayerMapPointHistoryEntry> Create_PlayerMapPointHistoryEntry(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<PlayerMapPointHistoryEntry> jsonTypeInfo))
		{
			JsonObjectInfoValues<PlayerMapPointHistoryEntry> objectInfo = new JsonObjectInfoValues<PlayerMapPointHistoryEntry>
			{
				ObjectCreator = () => new PlayerMapPointHistoryEntry(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => PlayerMapPointHistoryEntryPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(PlayerMapPointHistoryEntry).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] PlayerMapPointHistoryEntryPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[31];
		JsonPropertyInfoValues<ulong> jsonPropertyInfoValues = new JsonPropertyInfoValues<ulong>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(PlayerMapPointHistoryEntry);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((PlayerMapPointHistoryEntry)obj).PlayerId;
		jsonPropertyInfoValues.Setter = delegate(object obj, ulong value)
		{
			((PlayerMapPointHistoryEntry)obj).PlayerId = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "PlayerId";
		jsonPropertyInfoValues.JsonPropertyName = "player_id";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(PlayerMapPointHistoryEntry).GetProperty("PlayerId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ulong), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ulong> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<int> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(PlayerMapPointHistoryEntry);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((PlayerMapPointHistoryEntry)obj).GoldGained;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((PlayerMapPointHistoryEntry)obj).GoldGained = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "GoldGained";
		jsonPropertyInfoValues2.JsonPropertyName = "gold_gained";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(PlayerMapPointHistoryEntry).GetProperty("GoldGained", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(PlayerMapPointHistoryEntry);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((PlayerMapPointHistoryEntry)obj).GoldSpent;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((PlayerMapPointHistoryEntry)obj).GoldSpent = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "GoldSpent";
		jsonPropertyInfoValues2.JsonPropertyName = "gold_spent";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(PlayerMapPointHistoryEntry).GetProperty("GoldSpent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo3 = jsonPropertyInfoValues2;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(PlayerMapPointHistoryEntry);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((PlayerMapPointHistoryEntry)obj).GoldLost;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((PlayerMapPointHistoryEntry)obj).GoldLost = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "GoldLost";
		jsonPropertyInfoValues2.JsonPropertyName = "gold_lost";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(PlayerMapPointHistoryEntry).GetProperty("GoldLost", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo4 = jsonPropertyInfoValues2;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(PlayerMapPointHistoryEntry);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((PlayerMapPointHistoryEntry)obj).GoldStolen;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((PlayerMapPointHistoryEntry)obj).GoldStolen = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "GoldStolen";
		jsonPropertyInfoValues2.JsonPropertyName = "gold_stolen";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(PlayerMapPointHistoryEntry).GetProperty("GoldStolen", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo5 = jsonPropertyInfoValues2;
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(PlayerMapPointHistoryEntry);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((PlayerMapPointHistoryEntry)obj).CurrentGold;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((PlayerMapPointHistoryEntry)obj).CurrentGold = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "CurrentGold";
		jsonPropertyInfoValues2.JsonPropertyName = "current_gold";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(PlayerMapPointHistoryEntry).GetProperty("CurrentGold", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo6 = jsonPropertyInfoValues2;
		array[5] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo6);
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(PlayerMapPointHistoryEntry);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((PlayerMapPointHistoryEntry)obj).CurrentHp;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((PlayerMapPointHistoryEntry)obj).CurrentHp = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "CurrentHp";
		jsonPropertyInfoValues2.JsonPropertyName = "current_hp";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(PlayerMapPointHistoryEntry).GetProperty("CurrentHp", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo7 = jsonPropertyInfoValues2;
		array[6] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo7);
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(PlayerMapPointHistoryEntry);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((PlayerMapPointHistoryEntry)obj).MaxHp;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((PlayerMapPointHistoryEntry)obj).MaxHp = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "MaxHp";
		jsonPropertyInfoValues2.JsonPropertyName = "max_hp";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(PlayerMapPointHistoryEntry).GetProperty("MaxHp", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo8 = jsonPropertyInfoValues2;
		array[7] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo8);
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(PlayerMapPointHistoryEntry);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((PlayerMapPointHistoryEntry)obj).DamageTaken;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((PlayerMapPointHistoryEntry)obj).DamageTaken = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "DamageTaken";
		jsonPropertyInfoValues2.JsonPropertyName = "damage_taken";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(PlayerMapPointHistoryEntry).GetProperty("DamageTaken", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo9 = jsonPropertyInfoValues2;
		array[8] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo9);
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(PlayerMapPointHistoryEntry);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((PlayerMapPointHistoryEntry)obj).HpHealed;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((PlayerMapPointHistoryEntry)obj).HpHealed = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "HpHealed";
		jsonPropertyInfoValues2.JsonPropertyName = "hp_healed";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(PlayerMapPointHistoryEntry).GetProperty("HpHealed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo10 = jsonPropertyInfoValues2;
		array[9] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo10);
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(PlayerMapPointHistoryEntry);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((PlayerMapPointHistoryEntry)obj).MaxHpLost;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((PlayerMapPointHistoryEntry)obj).MaxHpLost = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "MaxHpLost";
		jsonPropertyInfoValues2.JsonPropertyName = "max_hp_lost";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(PlayerMapPointHistoryEntry).GetProperty("MaxHpLost", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo11 = jsonPropertyInfoValues2;
		array[10] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo11);
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(PlayerMapPointHistoryEntry);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((PlayerMapPointHistoryEntry)obj).MaxHpGained;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((PlayerMapPointHistoryEntry)obj).MaxHpGained = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "MaxHpGained";
		jsonPropertyInfoValues2.JsonPropertyName = "max_hp_gained";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(PlayerMapPointHistoryEntry).GetProperty("MaxHpGained", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo12 = jsonPropertyInfoValues2;
		array[11] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo12);
		JsonPropertyInfoValues<List<AncientChoiceHistoryEntry>> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<List<AncientChoiceHistoryEntry>>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(PlayerMapPointHistoryEntry);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((PlayerMapPointHistoryEntry)obj).AncientChoices;
		jsonPropertyInfoValues3.Setter = delegate(object obj, List<AncientChoiceHistoryEntry>? value)
		{
			((PlayerMapPointHistoryEntry)obj).AncientChoices = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "AncientChoices";
		jsonPropertyInfoValues3.JsonPropertyName = "ancient_choice";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(PlayerMapPointHistoryEntry).GetProperty("AncientChoices", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<AncientChoiceHistoryEntry>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<AncientChoiceHistoryEntry>> propertyInfo13 = jsonPropertyInfoValues3;
		array[12] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo13);
		array[12].IsGetNullable = false;
		array[12].IsSetNullable = false;
		JsonPropertyInfoValues<List<SerializableCard>> jsonPropertyInfoValues4 = new JsonPropertyInfoValues<List<SerializableCard>>();
		jsonPropertyInfoValues4.IsProperty = true;
		jsonPropertyInfoValues4.IsPublic = true;
		jsonPropertyInfoValues4.IsVirtual = false;
		jsonPropertyInfoValues4.DeclaringType = typeof(PlayerMapPointHistoryEntry);
		jsonPropertyInfoValues4.Converter = null;
		jsonPropertyInfoValues4.Getter = (object obj) => ((PlayerMapPointHistoryEntry)obj).CardsGained;
		jsonPropertyInfoValues4.Setter = delegate(object obj, List<SerializableCard>? value)
		{
			((PlayerMapPointHistoryEntry)obj).CardsGained = value;
		};
		jsonPropertyInfoValues4.IgnoreCondition = null;
		jsonPropertyInfoValues4.HasJsonInclude = false;
		jsonPropertyInfoValues4.IsExtensionData = false;
		jsonPropertyInfoValues4.NumberHandling = null;
		jsonPropertyInfoValues4.PropertyName = "CardsGained";
		jsonPropertyInfoValues4.JsonPropertyName = "cards_gained";
		jsonPropertyInfoValues4.AttributeProviderFactory = () => typeof(PlayerMapPointHistoryEntry).GetProperty("CardsGained", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<SerializableCard>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<SerializableCard>> propertyInfo14 = jsonPropertyInfoValues4;
		array[13] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo14);
		array[13].IsGetNullable = false;
		array[13].IsSetNullable = false;
		JsonPropertyInfoValues<List<CardChoiceHistoryEntry>> jsonPropertyInfoValues5 = new JsonPropertyInfoValues<List<CardChoiceHistoryEntry>>();
		jsonPropertyInfoValues5.IsProperty = true;
		jsonPropertyInfoValues5.IsPublic = true;
		jsonPropertyInfoValues5.IsVirtual = false;
		jsonPropertyInfoValues5.DeclaringType = typeof(PlayerMapPointHistoryEntry);
		jsonPropertyInfoValues5.Converter = null;
		jsonPropertyInfoValues5.Getter = (object obj) => ((PlayerMapPointHistoryEntry)obj).CardChoices;
		jsonPropertyInfoValues5.Setter = delegate(object obj, List<CardChoiceHistoryEntry>? value)
		{
			((PlayerMapPointHistoryEntry)obj).CardChoices = value;
		};
		jsonPropertyInfoValues5.IgnoreCondition = null;
		jsonPropertyInfoValues5.HasJsonInclude = false;
		jsonPropertyInfoValues5.IsExtensionData = false;
		jsonPropertyInfoValues5.NumberHandling = null;
		jsonPropertyInfoValues5.PropertyName = "CardChoices";
		jsonPropertyInfoValues5.JsonPropertyName = "card_choices";
		jsonPropertyInfoValues5.AttributeProviderFactory = () => typeof(PlayerMapPointHistoryEntry).GetProperty("CardChoices", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<CardChoiceHistoryEntry>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<CardChoiceHistoryEntry>> propertyInfo15 = jsonPropertyInfoValues5;
		array[14] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo15);
		array[14].IsGetNullable = false;
		array[14].IsSetNullable = false;
		JsonPropertyInfoValues<List<ModelChoiceHistoryEntry>> jsonPropertyInfoValues6 = new JsonPropertyInfoValues<List<ModelChoiceHistoryEntry>>();
		jsonPropertyInfoValues6.IsProperty = true;
		jsonPropertyInfoValues6.IsPublic = true;
		jsonPropertyInfoValues6.IsVirtual = false;
		jsonPropertyInfoValues6.DeclaringType = typeof(PlayerMapPointHistoryEntry);
		jsonPropertyInfoValues6.Converter = null;
		jsonPropertyInfoValues6.Getter = (object obj) => ((PlayerMapPointHistoryEntry)obj).RelicChoices;
		jsonPropertyInfoValues6.Setter = delegate(object obj, List<ModelChoiceHistoryEntry>? value)
		{
			((PlayerMapPointHistoryEntry)obj).RelicChoices = value;
		};
		jsonPropertyInfoValues6.IgnoreCondition = null;
		jsonPropertyInfoValues6.HasJsonInclude = false;
		jsonPropertyInfoValues6.IsExtensionData = false;
		jsonPropertyInfoValues6.NumberHandling = null;
		jsonPropertyInfoValues6.PropertyName = "RelicChoices";
		jsonPropertyInfoValues6.JsonPropertyName = "relic_choices";
		jsonPropertyInfoValues6.AttributeProviderFactory = () => typeof(PlayerMapPointHistoryEntry).GetProperty("RelicChoices", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<ModelChoiceHistoryEntry>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<ModelChoiceHistoryEntry>> propertyInfo16 = jsonPropertyInfoValues6;
		array[15] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo16);
		array[15].IsGetNullable = false;
		array[15].IsSetNullable = false;
		jsonPropertyInfoValues6 = new JsonPropertyInfoValues<List<ModelChoiceHistoryEntry>>();
		jsonPropertyInfoValues6.IsProperty = true;
		jsonPropertyInfoValues6.IsPublic = true;
		jsonPropertyInfoValues6.IsVirtual = false;
		jsonPropertyInfoValues6.DeclaringType = typeof(PlayerMapPointHistoryEntry);
		jsonPropertyInfoValues6.Converter = null;
		jsonPropertyInfoValues6.Getter = (object obj) => ((PlayerMapPointHistoryEntry)obj).PotionChoices;
		jsonPropertyInfoValues6.Setter = delegate(object obj, List<ModelChoiceHistoryEntry>? value)
		{
			((PlayerMapPointHistoryEntry)obj).PotionChoices = value;
		};
		jsonPropertyInfoValues6.IgnoreCondition = null;
		jsonPropertyInfoValues6.HasJsonInclude = false;
		jsonPropertyInfoValues6.IsExtensionData = false;
		jsonPropertyInfoValues6.NumberHandling = null;
		jsonPropertyInfoValues6.PropertyName = "PotionChoices";
		jsonPropertyInfoValues6.JsonPropertyName = "potion_choices";
		jsonPropertyInfoValues6.AttributeProviderFactory = () => typeof(PlayerMapPointHistoryEntry).GetProperty("PotionChoices", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<ModelChoiceHistoryEntry>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<ModelChoiceHistoryEntry>> propertyInfo17 = jsonPropertyInfoValues6;
		array[16] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo17);
		array[16].IsGetNullable = false;
		array[16].IsSetNullable = false;
		JsonPropertyInfoValues<List<ModelId>> jsonPropertyInfoValues7 = new JsonPropertyInfoValues<List<ModelId>>();
		jsonPropertyInfoValues7.IsProperty = true;
		jsonPropertyInfoValues7.IsPublic = true;
		jsonPropertyInfoValues7.IsVirtual = false;
		jsonPropertyInfoValues7.DeclaringType = typeof(PlayerMapPointHistoryEntry);
		jsonPropertyInfoValues7.Converter = null;
		jsonPropertyInfoValues7.Getter = (object obj) => ((PlayerMapPointHistoryEntry)obj).PotionDiscarded;
		jsonPropertyInfoValues7.Setter = delegate(object obj, List<ModelId>? value)
		{
			((PlayerMapPointHistoryEntry)obj).PotionDiscarded = value;
		};
		jsonPropertyInfoValues7.IgnoreCondition = null;
		jsonPropertyInfoValues7.HasJsonInclude = false;
		jsonPropertyInfoValues7.IsExtensionData = false;
		jsonPropertyInfoValues7.NumberHandling = null;
		jsonPropertyInfoValues7.PropertyName = "PotionDiscarded";
		jsonPropertyInfoValues7.JsonPropertyName = "potion_discarded";
		jsonPropertyInfoValues7.AttributeProviderFactory = () => typeof(PlayerMapPointHistoryEntry).GetProperty("PotionDiscarded", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<ModelId>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<ModelId>> propertyInfo18 = jsonPropertyInfoValues7;
		array[17] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo18);
		array[17].IsGetNullable = false;
		array[17].IsSetNullable = false;
		jsonPropertyInfoValues7 = new JsonPropertyInfoValues<List<ModelId>>();
		jsonPropertyInfoValues7.IsProperty = true;
		jsonPropertyInfoValues7.IsPublic = true;
		jsonPropertyInfoValues7.IsVirtual = false;
		jsonPropertyInfoValues7.DeclaringType = typeof(PlayerMapPointHistoryEntry);
		jsonPropertyInfoValues7.Converter = null;
		jsonPropertyInfoValues7.Getter = (object obj) => ((PlayerMapPointHistoryEntry)obj).PotionUsed;
		jsonPropertyInfoValues7.Setter = delegate(object obj, List<ModelId>? value)
		{
			((PlayerMapPointHistoryEntry)obj).PotionUsed = value;
		};
		jsonPropertyInfoValues7.IgnoreCondition = null;
		jsonPropertyInfoValues7.HasJsonInclude = false;
		jsonPropertyInfoValues7.IsExtensionData = false;
		jsonPropertyInfoValues7.NumberHandling = null;
		jsonPropertyInfoValues7.PropertyName = "PotionUsed";
		jsonPropertyInfoValues7.JsonPropertyName = "potion_used";
		jsonPropertyInfoValues7.AttributeProviderFactory = () => typeof(PlayerMapPointHistoryEntry).GetProperty("PotionUsed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<ModelId>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<ModelId>> propertyInfo19 = jsonPropertyInfoValues7;
		array[18] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo19);
		array[18].IsGetNullable = false;
		array[18].IsSetNullable = false;
		jsonPropertyInfoValues4 = new JsonPropertyInfoValues<List<SerializableCard>>();
		jsonPropertyInfoValues4.IsProperty = true;
		jsonPropertyInfoValues4.IsPublic = true;
		jsonPropertyInfoValues4.IsVirtual = false;
		jsonPropertyInfoValues4.DeclaringType = typeof(PlayerMapPointHistoryEntry);
		jsonPropertyInfoValues4.Converter = null;
		jsonPropertyInfoValues4.Getter = (object obj) => ((PlayerMapPointHistoryEntry)obj).CardsRemoved;
		jsonPropertyInfoValues4.Setter = delegate(object obj, List<SerializableCard>? value)
		{
			((PlayerMapPointHistoryEntry)obj).CardsRemoved = value;
		};
		jsonPropertyInfoValues4.IgnoreCondition = null;
		jsonPropertyInfoValues4.HasJsonInclude = false;
		jsonPropertyInfoValues4.IsExtensionData = false;
		jsonPropertyInfoValues4.NumberHandling = null;
		jsonPropertyInfoValues4.PropertyName = "CardsRemoved";
		jsonPropertyInfoValues4.JsonPropertyName = "cards_removed";
		jsonPropertyInfoValues4.AttributeProviderFactory = () => typeof(PlayerMapPointHistoryEntry).GetProperty("CardsRemoved", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<SerializableCard>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<SerializableCard>> propertyInfo20 = jsonPropertyInfoValues4;
		array[19] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo20);
		array[19].IsGetNullable = false;
		array[19].IsSetNullable = false;
		jsonPropertyInfoValues7 = new JsonPropertyInfoValues<List<ModelId>>();
		jsonPropertyInfoValues7.IsProperty = true;
		jsonPropertyInfoValues7.IsPublic = true;
		jsonPropertyInfoValues7.IsVirtual = false;
		jsonPropertyInfoValues7.DeclaringType = typeof(PlayerMapPointHistoryEntry);
		jsonPropertyInfoValues7.Converter = null;
		jsonPropertyInfoValues7.Getter = (object obj) => ((PlayerMapPointHistoryEntry)obj).RelicsRemoved;
		jsonPropertyInfoValues7.Setter = delegate(object obj, List<ModelId>? value)
		{
			((PlayerMapPointHistoryEntry)obj).RelicsRemoved = value;
		};
		jsonPropertyInfoValues7.IgnoreCondition = null;
		jsonPropertyInfoValues7.HasJsonInclude = false;
		jsonPropertyInfoValues7.IsExtensionData = false;
		jsonPropertyInfoValues7.NumberHandling = null;
		jsonPropertyInfoValues7.PropertyName = "RelicsRemoved";
		jsonPropertyInfoValues7.JsonPropertyName = "relics_removed";
		jsonPropertyInfoValues7.AttributeProviderFactory = () => typeof(PlayerMapPointHistoryEntry).GetProperty("RelicsRemoved", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<ModelId>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<ModelId>> propertyInfo21 = jsonPropertyInfoValues7;
		array[20] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo21);
		array[20].IsGetNullable = false;
		array[20].IsSetNullable = false;
		JsonPropertyInfoValues<List<CardEnchantmentHistoryEntry>> jsonPropertyInfoValues8 = new JsonPropertyInfoValues<List<CardEnchantmentHistoryEntry>>();
		jsonPropertyInfoValues8.IsProperty = true;
		jsonPropertyInfoValues8.IsPublic = true;
		jsonPropertyInfoValues8.IsVirtual = false;
		jsonPropertyInfoValues8.DeclaringType = typeof(PlayerMapPointHistoryEntry);
		jsonPropertyInfoValues8.Converter = null;
		jsonPropertyInfoValues8.Getter = (object obj) => ((PlayerMapPointHistoryEntry)obj).CardsEnchanted;
		jsonPropertyInfoValues8.Setter = delegate(object obj, List<CardEnchantmentHistoryEntry>? value)
		{
			((PlayerMapPointHistoryEntry)obj).CardsEnchanted = value;
		};
		jsonPropertyInfoValues8.IgnoreCondition = null;
		jsonPropertyInfoValues8.HasJsonInclude = false;
		jsonPropertyInfoValues8.IsExtensionData = false;
		jsonPropertyInfoValues8.NumberHandling = null;
		jsonPropertyInfoValues8.PropertyName = "CardsEnchanted";
		jsonPropertyInfoValues8.JsonPropertyName = "cards_enchanted";
		jsonPropertyInfoValues8.AttributeProviderFactory = () => typeof(PlayerMapPointHistoryEntry).GetProperty("CardsEnchanted", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<CardEnchantmentHistoryEntry>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<CardEnchantmentHistoryEntry>> propertyInfo22 = jsonPropertyInfoValues8;
		array[21] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo22);
		array[21].IsGetNullable = false;
		array[21].IsSetNullable = false;
		JsonPropertyInfoValues<List<CardTransformationHistoryEntry>> jsonPropertyInfoValues9 = new JsonPropertyInfoValues<List<CardTransformationHistoryEntry>>();
		jsonPropertyInfoValues9.IsProperty = true;
		jsonPropertyInfoValues9.IsPublic = true;
		jsonPropertyInfoValues9.IsVirtual = false;
		jsonPropertyInfoValues9.DeclaringType = typeof(PlayerMapPointHistoryEntry);
		jsonPropertyInfoValues9.Converter = null;
		jsonPropertyInfoValues9.Getter = (object obj) => ((PlayerMapPointHistoryEntry)obj).CardsTransformed;
		jsonPropertyInfoValues9.Setter = delegate(object obj, List<CardTransformationHistoryEntry>? value)
		{
			((PlayerMapPointHistoryEntry)obj).CardsTransformed = value;
		};
		jsonPropertyInfoValues9.IgnoreCondition = null;
		jsonPropertyInfoValues9.HasJsonInclude = false;
		jsonPropertyInfoValues9.IsExtensionData = false;
		jsonPropertyInfoValues9.NumberHandling = null;
		jsonPropertyInfoValues9.PropertyName = "CardsTransformed";
		jsonPropertyInfoValues9.JsonPropertyName = "cards_transformed";
		jsonPropertyInfoValues9.AttributeProviderFactory = () => typeof(PlayerMapPointHistoryEntry).GetProperty("CardsTransformed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<CardTransformationHistoryEntry>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<CardTransformationHistoryEntry>> propertyInfo23 = jsonPropertyInfoValues9;
		array[22] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo23);
		array[22].IsGetNullable = false;
		array[22].IsSetNullable = false;
		jsonPropertyInfoValues7 = new JsonPropertyInfoValues<List<ModelId>>();
		jsonPropertyInfoValues7.IsProperty = true;
		jsonPropertyInfoValues7.IsPublic = true;
		jsonPropertyInfoValues7.IsVirtual = false;
		jsonPropertyInfoValues7.DeclaringType = typeof(PlayerMapPointHistoryEntry);
		jsonPropertyInfoValues7.Converter = null;
		jsonPropertyInfoValues7.Getter = (object obj) => ((PlayerMapPointHistoryEntry)obj).UpgradedCards;
		jsonPropertyInfoValues7.Setter = delegate(object obj, List<ModelId>? value)
		{
			((PlayerMapPointHistoryEntry)obj).UpgradedCards = value;
		};
		jsonPropertyInfoValues7.IgnoreCondition = null;
		jsonPropertyInfoValues7.HasJsonInclude = false;
		jsonPropertyInfoValues7.IsExtensionData = false;
		jsonPropertyInfoValues7.NumberHandling = null;
		jsonPropertyInfoValues7.PropertyName = "UpgradedCards";
		jsonPropertyInfoValues7.JsonPropertyName = "upgraded_cards";
		jsonPropertyInfoValues7.AttributeProviderFactory = () => typeof(PlayerMapPointHistoryEntry).GetProperty("UpgradedCards", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<ModelId>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<ModelId>> propertyInfo24 = jsonPropertyInfoValues7;
		array[23] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo24);
		array[23].IsGetNullable = false;
		array[23].IsSetNullable = false;
		jsonPropertyInfoValues7 = new JsonPropertyInfoValues<List<ModelId>>();
		jsonPropertyInfoValues7.IsProperty = true;
		jsonPropertyInfoValues7.IsPublic = true;
		jsonPropertyInfoValues7.IsVirtual = false;
		jsonPropertyInfoValues7.DeclaringType = typeof(PlayerMapPointHistoryEntry);
		jsonPropertyInfoValues7.Converter = null;
		jsonPropertyInfoValues7.Getter = (object obj) => ((PlayerMapPointHistoryEntry)obj).DowngradedCards;
		jsonPropertyInfoValues7.Setter = delegate(object obj, List<ModelId>? value)
		{
			((PlayerMapPointHistoryEntry)obj).DowngradedCards = value;
		};
		jsonPropertyInfoValues7.IgnoreCondition = null;
		jsonPropertyInfoValues7.HasJsonInclude = false;
		jsonPropertyInfoValues7.IsExtensionData = false;
		jsonPropertyInfoValues7.NumberHandling = null;
		jsonPropertyInfoValues7.PropertyName = "DowngradedCards";
		jsonPropertyInfoValues7.JsonPropertyName = "downgraded_cards";
		jsonPropertyInfoValues7.AttributeProviderFactory = () => typeof(PlayerMapPointHistoryEntry).GetProperty("DowngradedCards", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<ModelId>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<ModelId>> propertyInfo25 = jsonPropertyInfoValues7;
		array[24] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo25);
		array[24].IsGetNullable = false;
		array[24].IsSetNullable = false;
		JsonPropertyInfoValues<List<EventOptionHistoryEntry>> jsonPropertyInfoValues10 = new JsonPropertyInfoValues<List<EventOptionHistoryEntry>>();
		jsonPropertyInfoValues10.IsProperty = true;
		jsonPropertyInfoValues10.IsPublic = true;
		jsonPropertyInfoValues10.IsVirtual = false;
		jsonPropertyInfoValues10.DeclaringType = typeof(PlayerMapPointHistoryEntry);
		jsonPropertyInfoValues10.Converter = null;
		jsonPropertyInfoValues10.Getter = (object obj) => ((PlayerMapPointHistoryEntry)obj).EventChoices;
		jsonPropertyInfoValues10.Setter = delegate(object obj, List<EventOptionHistoryEntry>? value)
		{
			((PlayerMapPointHistoryEntry)obj).EventChoices = value;
		};
		jsonPropertyInfoValues10.IgnoreCondition = null;
		jsonPropertyInfoValues10.HasJsonInclude = false;
		jsonPropertyInfoValues10.IsExtensionData = false;
		jsonPropertyInfoValues10.NumberHandling = null;
		jsonPropertyInfoValues10.PropertyName = "EventChoices";
		jsonPropertyInfoValues10.JsonPropertyName = "event_choices";
		jsonPropertyInfoValues10.AttributeProviderFactory = () => typeof(PlayerMapPointHistoryEntry).GetProperty("EventChoices", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<EventOptionHistoryEntry>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<EventOptionHistoryEntry>> propertyInfo26 = jsonPropertyInfoValues10;
		array[25] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo26);
		array[25].IsGetNullable = false;
		array[25].IsSetNullable = false;
		JsonPropertyInfoValues<List<string>> jsonPropertyInfoValues11 = new JsonPropertyInfoValues<List<string>>();
		jsonPropertyInfoValues11.IsProperty = true;
		jsonPropertyInfoValues11.IsPublic = true;
		jsonPropertyInfoValues11.IsVirtual = false;
		jsonPropertyInfoValues11.DeclaringType = typeof(PlayerMapPointHistoryEntry);
		jsonPropertyInfoValues11.Converter = null;
		jsonPropertyInfoValues11.Getter = (object obj) => ((PlayerMapPointHistoryEntry)obj).RestSiteChoices;
		jsonPropertyInfoValues11.Setter = delegate(object obj, List<string>? value)
		{
			((PlayerMapPointHistoryEntry)obj).RestSiteChoices = value;
		};
		jsonPropertyInfoValues11.IgnoreCondition = null;
		jsonPropertyInfoValues11.HasJsonInclude = false;
		jsonPropertyInfoValues11.IsExtensionData = false;
		jsonPropertyInfoValues11.NumberHandling = null;
		jsonPropertyInfoValues11.PropertyName = "RestSiteChoices";
		jsonPropertyInfoValues11.JsonPropertyName = "rest_site_choices";
		jsonPropertyInfoValues11.AttributeProviderFactory = () => typeof(PlayerMapPointHistoryEntry).GetProperty("RestSiteChoices", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<string>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<string>> propertyInfo27 = jsonPropertyInfoValues11;
		array[26] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo27);
		array[26].IsGetNullable = false;
		array[26].IsSetNullable = false;
		jsonPropertyInfoValues7 = new JsonPropertyInfoValues<List<ModelId>>();
		jsonPropertyInfoValues7.IsProperty = true;
		jsonPropertyInfoValues7.IsPublic = true;
		jsonPropertyInfoValues7.IsVirtual = false;
		jsonPropertyInfoValues7.DeclaringType = typeof(PlayerMapPointHistoryEntry);
		jsonPropertyInfoValues7.Converter = null;
		jsonPropertyInfoValues7.Getter = (object obj) => ((PlayerMapPointHistoryEntry)obj).BoughtRelics;
		jsonPropertyInfoValues7.Setter = delegate(object obj, List<ModelId>? value)
		{
			((PlayerMapPointHistoryEntry)obj).BoughtRelics = value;
		};
		jsonPropertyInfoValues7.IgnoreCondition = null;
		jsonPropertyInfoValues7.HasJsonInclude = false;
		jsonPropertyInfoValues7.IsExtensionData = false;
		jsonPropertyInfoValues7.NumberHandling = null;
		jsonPropertyInfoValues7.PropertyName = "BoughtRelics";
		jsonPropertyInfoValues7.JsonPropertyName = "bought_relics";
		jsonPropertyInfoValues7.AttributeProviderFactory = () => typeof(PlayerMapPointHistoryEntry).GetProperty("BoughtRelics", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<ModelId>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<ModelId>> propertyInfo28 = jsonPropertyInfoValues7;
		array[27] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo28);
		array[27].IsGetNullable = false;
		array[27].IsSetNullable = false;
		jsonPropertyInfoValues7 = new JsonPropertyInfoValues<List<ModelId>>();
		jsonPropertyInfoValues7.IsProperty = true;
		jsonPropertyInfoValues7.IsPublic = true;
		jsonPropertyInfoValues7.IsVirtual = false;
		jsonPropertyInfoValues7.DeclaringType = typeof(PlayerMapPointHistoryEntry);
		jsonPropertyInfoValues7.Converter = null;
		jsonPropertyInfoValues7.Getter = (object obj) => ((PlayerMapPointHistoryEntry)obj).BoughtPotions;
		jsonPropertyInfoValues7.Setter = delegate(object obj, List<ModelId>? value)
		{
			((PlayerMapPointHistoryEntry)obj).BoughtPotions = value;
		};
		jsonPropertyInfoValues7.IgnoreCondition = null;
		jsonPropertyInfoValues7.HasJsonInclude = false;
		jsonPropertyInfoValues7.IsExtensionData = false;
		jsonPropertyInfoValues7.NumberHandling = null;
		jsonPropertyInfoValues7.PropertyName = "BoughtPotions";
		jsonPropertyInfoValues7.JsonPropertyName = "bought_potions";
		jsonPropertyInfoValues7.AttributeProviderFactory = () => typeof(PlayerMapPointHistoryEntry).GetProperty("BoughtPotions", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<ModelId>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<ModelId>> propertyInfo29 = jsonPropertyInfoValues7;
		array[28] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo29);
		array[28].IsGetNullable = false;
		array[28].IsSetNullable = false;
		jsonPropertyInfoValues7 = new JsonPropertyInfoValues<List<ModelId>>();
		jsonPropertyInfoValues7.IsProperty = true;
		jsonPropertyInfoValues7.IsPublic = true;
		jsonPropertyInfoValues7.IsVirtual = false;
		jsonPropertyInfoValues7.DeclaringType = typeof(PlayerMapPointHistoryEntry);
		jsonPropertyInfoValues7.Converter = null;
		jsonPropertyInfoValues7.Getter = (object obj) => ((PlayerMapPointHistoryEntry)obj).BoughtColorless;
		jsonPropertyInfoValues7.Setter = delegate(object obj, List<ModelId>? value)
		{
			((PlayerMapPointHistoryEntry)obj).BoughtColorless = value;
		};
		jsonPropertyInfoValues7.IgnoreCondition = null;
		jsonPropertyInfoValues7.HasJsonInclude = false;
		jsonPropertyInfoValues7.IsExtensionData = false;
		jsonPropertyInfoValues7.NumberHandling = null;
		jsonPropertyInfoValues7.PropertyName = "BoughtColorless";
		jsonPropertyInfoValues7.JsonPropertyName = "bought_colorless";
		jsonPropertyInfoValues7.AttributeProviderFactory = () => typeof(PlayerMapPointHistoryEntry).GetProperty("BoughtColorless", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<ModelId>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<ModelId>> propertyInfo30 = jsonPropertyInfoValues7;
		array[29] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo30);
		array[29].IsGetNullable = false;
		array[29].IsSetNullable = false;
		jsonPropertyInfoValues7 = new JsonPropertyInfoValues<List<ModelId>>();
		jsonPropertyInfoValues7.IsProperty = true;
		jsonPropertyInfoValues7.IsPublic = true;
		jsonPropertyInfoValues7.IsVirtual = false;
		jsonPropertyInfoValues7.DeclaringType = typeof(PlayerMapPointHistoryEntry);
		jsonPropertyInfoValues7.Converter = null;
		jsonPropertyInfoValues7.Getter = (object obj) => ((PlayerMapPointHistoryEntry)obj).CompletedQuests;
		jsonPropertyInfoValues7.Setter = delegate(object obj, List<ModelId>? value)
		{
			((PlayerMapPointHistoryEntry)obj).CompletedQuests = value;
		};
		jsonPropertyInfoValues7.IgnoreCondition = null;
		jsonPropertyInfoValues7.HasJsonInclude = false;
		jsonPropertyInfoValues7.IsExtensionData = false;
		jsonPropertyInfoValues7.NumberHandling = null;
		jsonPropertyInfoValues7.PropertyName = "CompletedQuests";
		jsonPropertyInfoValues7.JsonPropertyName = "completed_quests";
		jsonPropertyInfoValues7.AttributeProviderFactory = () => typeof(PlayerMapPointHistoryEntry).GetProperty("CompletedQuests", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<ModelId>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<ModelId>> propertyInfo31 = jsonPropertyInfoValues7;
		array[30] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo31);
		array[30].IsGetNullable = false;
		array[30].IsSetNullable = false;
		return array;
	}

	private JsonTypeInfo<RunHistory> Create_RunHistory(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<RunHistory> jsonTypeInfo))
		{
			JsonObjectInfoValues<RunHistory> jsonObjectInfoValues = new JsonObjectInfoValues<RunHistory>();
			jsonObjectInfoValues.ObjectCreator = null;
			jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = (object[] args) => new RunHistory
			{
				PlatformType = (PlatformType)args[0],
				GameMode = (GameMode)args[1],
				Win = (bool)args[2],
				Seed = (string)args[3],
				StartTime = (long)args[4],
				RunTime = (float)args[5],
				Ascension = (int)args[6],
				BuildId = (string)args[7],
				WasAbandoned = (bool)args[8],
				KilledByEncounter = (ModelId)args[9],
				KilledByEvent = (ModelId)args[10],
				Players = (List<RunHistoryPlayer>)args[11],
				Acts = (List<ModelId>)args[12],
				Modifiers = (List<SerializableModifier>)args[13],
				MapPointHistory = (List<List<MapPointHistoryEntry>>)args[14]
			};
			jsonObjectInfoValues.PropertyMetadataInitializer = (JsonSerializerContext _) => RunHistoryPropInit(options);
			jsonObjectInfoValues.ConstructorParameterMetadataInitializer = RunHistoryCtorParamInit;
			jsonObjectInfoValues.ConstructorAttributeProviderFactory = () => typeof(RunHistory).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null);
			jsonObjectInfoValues.SerializeHandler = null;
			JsonObjectInfoValues<RunHistory> objectInfo = jsonObjectInfoValues;
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] RunHistoryPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[16];
		JsonPropertyInfoValues<int> jsonPropertyInfoValues = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(RunHistory);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((RunHistory)obj).SchemaVersion;
		jsonPropertyInfoValues.Setter = delegate(object obj, int value)
		{
			((RunHistory)obj).SchemaVersion = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "SchemaVersion";
		jsonPropertyInfoValues.JsonPropertyName = "schema_version";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(RunHistory).GetProperty("SchemaVersion", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<PlatformType> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<PlatformType>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(RunHistory);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((RunHistory)obj).PlatformType;
		jsonPropertyInfoValues2.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "PlatformType";
		jsonPropertyInfoValues2.JsonPropertyName = "platform_type";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(RunHistory).GetProperty("PlatformType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(PlatformType), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<PlatformType> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<GameMode> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<GameMode>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(RunHistory);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((RunHistory)obj).GameMode;
		jsonPropertyInfoValues3.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "GameMode";
		jsonPropertyInfoValues3.JsonPropertyName = "game_mode";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(RunHistory).GetProperty("GameMode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(GameMode), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<GameMode> propertyInfo3 = jsonPropertyInfoValues3;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		JsonPropertyInfoValues<bool> jsonPropertyInfoValues4 = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues4.IsProperty = true;
		jsonPropertyInfoValues4.IsPublic = true;
		jsonPropertyInfoValues4.IsVirtual = false;
		jsonPropertyInfoValues4.DeclaringType = typeof(RunHistory);
		jsonPropertyInfoValues4.Converter = null;
		jsonPropertyInfoValues4.Getter = (object obj) => ((RunHistory)obj).Win;
		jsonPropertyInfoValues4.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues4.IgnoreCondition = null;
		jsonPropertyInfoValues4.HasJsonInclude = false;
		jsonPropertyInfoValues4.IsExtensionData = false;
		jsonPropertyInfoValues4.NumberHandling = null;
		jsonPropertyInfoValues4.PropertyName = "Win";
		jsonPropertyInfoValues4.JsonPropertyName = "win";
		jsonPropertyInfoValues4.AttributeProviderFactory = () => typeof(RunHistory).GetProperty("Win", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<bool> propertyInfo4 = jsonPropertyInfoValues4;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		JsonPropertyInfoValues<string> jsonPropertyInfoValues5 = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues5.IsProperty = true;
		jsonPropertyInfoValues5.IsPublic = true;
		jsonPropertyInfoValues5.IsVirtual = false;
		jsonPropertyInfoValues5.DeclaringType = typeof(RunHistory);
		jsonPropertyInfoValues5.Converter = null;
		jsonPropertyInfoValues5.Getter = (object obj) => ((RunHistory)obj).Seed;
		jsonPropertyInfoValues5.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues5.IgnoreCondition = null;
		jsonPropertyInfoValues5.HasJsonInclude = false;
		jsonPropertyInfoValues5.IsExtensionData = false;
		jsonPropertyInfoValues5.NumberHandling = null;
		jsonPropertyInfoValues5.PropertyName = "Seed";
		jsonPropertyInfoValues5.JsonPropertyName = "seed";
		jsonPropertyInfoValues5.AttributeProviderFactory = () => typeof(RunHistory).GetProperty("Seed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo5 = jsonPropertyInfoValues5;
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		array[4].IsGetNullable = false;
		array[4].IsSetNullable = false;
		JsonPropertyInfoValues<long> jsonPropertyInfoValues6 = new JsonPropertyInfoValues<long>();
		jsonPropertyInfoValues6.IsProperty = true;
		jsonPropertyInfoValues6.IsPublic = true;
		jsonPropertyInfoValues6.IsVirtual = false;
		jsonPropertyInfoValues6.DeclaringType = typeof(RunHistory);
		jsonPropertyInfoValues6.Converter = null;
		jsonPropertyInfoValues6.Getter = (object obj) => ((RunHistory)obj).StartTime;
		jsonPropertyInfoValues6.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues6.IgnoreCondition = null;
		jsonPropertyInfoValues6.HasJsonInclude = false;
		jsonPropertyInfoValues6.IsExtensionData = false;
		jsonPropertyInfoValues6.NumberHandling = null;
		jsonPropertyInfoValues6.PropertyName = "StartTime";
		jsonPropertyInfoValues6.JsonPropertyName = "start_time";
		jsonPropertyInfoValues6.AttributeProviderFactory = () => typeof(RunHistory).GetProperty("StartTime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(long), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<long> propertyInfo6 = jsonPropertyInfoValues6;
		array[5] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo6);
		JsonPropertyInfoValues<float> jsonPropertyInfoValues7 = new JsonPropertyInfoValues<float>();
		jsonPropertyInfoValues7.IsProperty = true;
		jsonPropertyInfoValues7.IsPublic = true;
		jsonPropertyInfoValues7.IsVirtual = false;
		jsonPropertyInfoValues7.DeclaringType = typeof(RunHistory);
		jsonPropertyInfoValues7.Converter = null;
		jsonPropertyInfoValues7.Getter = (object obj) => ((RunHistory)obj).RunTime;
		jsonPropertyInfoValues7.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues7.IgnoreCondition = null;
		jsonPropertyInfoValues7.HasJsonInclude = false;
		jsonPropertyInfoValues7.IsExtensionData = false;
		jsonPropertyInfoValues7.NumberHandling = null;
		jsonPropertyInfoValues7.PropertyName = "RunTime";
		jsonPropertyInfoValues7.JsonPropertyName = "run_time";
		jsonPropertyInfoValues7.AttributeProviderFactory = () => typeof(RunHistory).GetProperty("RunTime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(float), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<float> propertyInfo7 = jsonPropertyInfoValues7;
		array[6] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo7);
		jsonPropertyInfoValues = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(RunHistory);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((RunHistory)obj).Ascension;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Ascension";
		jsonPropertyInfoValues.JsonPropertyName = "ascension";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(RunHistory).GetProperty("Ascension", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo8 = jsonPropertyInfoValues;
		array[7] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo8);
		jsonPropertyInfoValues5 = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues5.IsProperty = true;
		jsonPropertyInfoValues5.IsPublic = true;
		jsonPropertyInfoValues5.IsVirtual = false;
		jsonPropertyInfoValues5.DeclaringType = typeof(RunHistory);
		jsonPropertyInfoValues5.Converter = null;
		jsonPropertyInfoValues5.Getter = (object obj) => ((RunHistory)obj).BuildId;
		jsonPropertyInfoValues5.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues5.IgnoreCondition = null;
		jsonPropertyInfoValues5.HasJsonInclude = false;
		jsonPropertyInfoValues5.IsExtensionData = false;
		jsonPropertyInfoValues5.NumberHandling = null;
		jsonPropertyInfoValues5.PropertyName = "BuildId";
		jsonPropertyInfoValues5.JsonPropertyName = "build_id";
		jsonPropertyInfoValues5.AttributeProviderFactory = () => typeof(RunHistory).GetProperty("BuildId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo9 = jsonPropertyInfoValues5;
		array[8] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo9);
		array[8].IsGetNullable = false;
		array[8].IsSetNullable = false;
		jsonPropertyInfoValues4 = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues4.IsProperty = true;
		jsonPropertyInfoValues4.IsPublic = true;
		jsonPropertyInfoValues4.IsVirtual = false;
		jsonPropertyInfoValues4.DeclaringType = typeof(RunHistory);
		jsonPropertyInfoValues4.Converter = null;
		jsonPropertyInfoValues4.Getter = (object obj) => ((RunHistory)obj).WasAbandoned;
		jsonPropertyInfoValues4.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues4.IgnoreCondition = null;
		jsonPropertyInfoValues4.HasJsonInclude = false;
		jsonPropertyInfoValues4.IsExtensionData = false;
		jsonPropertyInfoValues4.NumberHandling = null;
		jsonPropertyInfoValues4.PropertyName = "WasAbandoned";
		jsonPropertyInfoValues4.JsonPropertyName = "was_abandoned";
		jsonPropertyInfoValues4.AttributeProviderFactory = () => typeof(RunHistory).GetProperty("WasAbandoned", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<bool> propertyInfo10 = jsonPropertyInfoValues4;
		array[9] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo10);
		JsonPropertyInfoValues<ModelId> jsonPropertyInfoValues8 = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues8.IsProperty = true;
		jsonPropertyInfoValues8.IsPublic = true;
		jsonPropertyInfoValues8.IsVirtual = false;
		jsonPropertyInfoValues8.DeclaringType = typeof(RunHistory);
		jsonPropertyInfoValues8.Converter = null;
		jsonPropertyInfoValues8.Getter = (object obj) => ((RunHistory)obj).KilledByEncounter;
		jsonPropertyInfoValues8.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues8.IgnoreCondition = null;
		jsonPropertyInfoValues8.HasJsonInclude = false;
		jsonPropertyInfoValues8.IsExtensionData = false;
		jsonPropertyInfoValues8.NumberHandling = null;
		jsonPropertyInfoValues8.PropertyName = "KilledByEncounter";
		jsonPropertyInfoValues8.JsonPropertyName = "killed_by_encounter";
		jsonPropertyInfoValues8.AttributeProviderFactory = () => typeof(RunHistory).GetProperty("KilledByEncounter", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModelId), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModelId> propertyInfo11 = jsonPropertyInfoValues8;
		array[10] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo11);
		array[10].IsGetNullable = false;
		array[10].IsSetNullable = false;
		jsonPropertyInfoValues8 = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues8.IsProperty = true;
		jsonPropertyInfoValues8.IsPublic = true;
		jsonPropertyInfoValues8.IsVirtual = false;
		jsonPropertyInfoValues8.DeclaringType = typeof(RunHistory);
		jsonPropertyInfoValues8.Converter = null;
		jsonPropertyInfoValues8.Getter = (object obj) => ((RunHistory)obj).KilledByEvent;
		jsonPropertyInfoValues8.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues8.IgnoreCondition = null;
		jsonPropertyInfoValues8.HasJsonInclude = false;
		jsonPropertyInfoValues8.IsExtensionData = false;
		jsonPropertyInfoValues8.NumberHandling = null;
		jsonPropertyInfoValues8.PropertyName = "KilledByEvent";
		jsonPropertyInfoValues8.JsonPropertyName = "killed_by_event";
		jsonPropertyInfoValues8.AttributeProviderFactory = () => typeof(RunHistory).GetProperty("KilledByEvent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModelId), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModelId> propertyInfo12 = jsonPropertyInfoValues8;
		array[11] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo12);
		array[11].IsGetNullable = false;
		array[11].IsSetNullable = false;
		JsonPropertyInfoValues<List<RunHistoryPlayer>> jsonPropertyInfoValues9 = new JsonPropertyInfoValues<List<RunHistoryPlayer>>();
		jsonPropertyInfoValues9.IsProperty = true;
		jsonPropertyInfoValues9.IsPublic = true;
		jsonPropertyInfoValues9.IsVirtual = false;
		jsonPropertyInfoValues9.DeclaringType = typeof(RunHistory);
		jsonPropertyInfoValues9.Converter = null;
		jsonPropertyInfoValues9.Getter = (object obj) => ((RunHistory)obj).Players;
		jsonPropertyInfoValues9.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues9.IgnoreCondition = null;
		jsonPropertyInfoValues9.HasJsonInclude = false;
		jsonPropertyInfoValues9.IsExtensionData = false;
		jsonPropertyInfoValues9.NumberHandling = null;
		jsonPropertyInfoValues9.PropertyName = "Players";
		jsonPropertyInfoValues9.JsonPropertyName = "players";
		jsonPropertyInfoValues9.AttributeProviderFactory = () => typeof(RunHistory).GetProperty("Players", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<RunHistoryPlayer>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<RunHistoryPlayer>> propertyInfo13 = jsonPropertyInfoValues9;
		array[12] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo13);
		array[12].IsGetNullable = false;
		array[12].IsSetNullable = false;
		JsonPropertyInfoValues<List<ModelId>> jsonPropertyInfoValues10 = new JsonPropertyInfoValues<List<ModelId>>();
		jsonPropertyInfoValues10.IsProperty = true;
		jsonPropertyInfoValues10.IsPublic = true;
		jsonPropertyInfoValues10.IsVirtual = false;
		jsonPropertyInfoValues10.DeclaringType = typeof(RunHistory);
		jsonPropertyInfoValues10.Converter = null;
		jsonPropertyInfoValues10.Getter = (object obj) => ((RunHistory)obj).Acts;
		jsonPropertyInfoValues10.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues10.IgnoreCondition = null;
		jsonPropertyInfoValues10.HasJsonInclude = false;
		jsonPropertyInfoValues10.IsExtensionData = false;
		jsonPropertyInfoValues10.NumberHandling = null;
		jsonPropertyInfoValues10.PropertyName = "Acts";
		jsonPropertyInfoValues10.JsonPropertyName = "acts";
		jsonPropertyInfoValues10.AttributeProviderFactory = () => typeof(RunHistory).GetProperty("Acts", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<ModelId>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<ModelId>> propertyInfo14 = jsonPropertyInfoValues10;
		array[13] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo14);
		array[13].IsGetNullable = false;
		array[13].IsSetNullable = false;
		JsonPropertyInfoValues<List<SerializableModifier>> jsonPropertyInfoValues11 = new JsonPropertyInfoValues<List<SerializableModifier>>();
		jsonPropertyInfoValues11.IsProperty = true;
		jsonPropertyInfoValues11.IsPublic = true;
		jsonPropertyInfoValues11.IsVirtual = false;
		jsonPropertyInfoValues11.DeclaringType = typeof(RunHistory);
		jsonPropertyInfoValues11.Converter = null;
		jsonPropertyInfoValues11.Getter = (object obj) => ((RunHistory)obj).Modifiers;
		jsonPropertyInfoValues11.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues11.IgnoreCondition = null;
		jsonPropertyInfoValues11.HasJsonInclude = false;
		jsonPropertyInfoValues11.IsExtensionData = false;
		jsonPropertyInfoValues11.NumberHandling = null;
		jsonPropertyInfoValues11.PropertyName = "Modifiers";
		jsonPropertyInfoValues11.JsonPropertyName = "modifiers";
		jsonPropertyInfoValues11.AttributeProviderFactory = () => typeof(RunHistory).GetProperty("Modifiers", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<SerializableModifier>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<SerializableModifier>> propertyInfo15 = jsonPropertyInfoValues11;
		array[14] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo15);
		array[14].IsGetNullable = false;
		array[14].IsSetNullable = false;
		JsonPropertyInfoValues<List<List<MapPointHistoryEntry>>> jsonPropertyInfoValues12 = new JsonPropertyInfoValues<List<List<MapPointHistoryEntry>>>();
		jsonPropertyInfoValues12.IsProperty = true;
		jsonPropertyInfoValues12.IsPublic = true;
		jsonPropertyInfoValues12.IsVirtual = false;
		jsonPropertyInfoValues12.DeclaringType = typeof(RunHistory);
		jsonPropertyInfoValues12.Converter = null;
		jsonPropertyInfoValues12.Getter = (object obj) => ((RunHistory)obj).MapPointHistory;
		jsonPropertyInfoValues12.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues12.IgnoreCondition = null;
		jsonPropertyInfoValues12.HasJsonInclude = false;
		jsonPropertyInfoValues12.IsExtensionData = false;
		jsonPropertyInfoValues12.NumberHandling = null;
		jsonPropertyInfoValues12.PropertyName = "MapPointHistory";
		jsonPropertyInfoValues12.JsonPropertyName = "map_point_history";
		jsonPropertyInfoValues12.AttributeProviderFactory = () => typeof(RunHistory).GetProperty("MapPointHistory", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<List<MapPointHistoryEntry>>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<List<MapPointHistoryEntry>>> propertyInfo16 = jsonPropertyInfoValues12;
		array[15] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo16);
		array[15].IsGetNullable = false;
		array[15].IsSetNullable = false;
		return array;
	}

	private static JsonParameterInfoValues[] RunHistoryCtorParamInit()
	{
		return new JsonParameterInfoValues[15]
		{
			new JsonParameterInfoValues
			{
				Name = "PlatformType",
				ParameterType = typeof(PlatformType),
				Position = 0,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "GameMode",
				ParameterType = typeof(GameMode),
				Position = 1,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Win",
				ParameterType = typeof(bool),
				Position = 2,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Seed",
				ParameterType = typeof(string),
				Position = 3,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "StartTime",
				ParameterType = typeof(long),
				Position = 4,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "RunTime",
				ParameterType = typeof(float),
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
				Name = "BuildId",
				ParameterType = typeof(string),
				Position = 7,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "WasAbandoned",
				ParameterType = typeof(bool),
				Position = 8,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "KilledByEncounter",
				ParameterType = typeof(ModelId),
				Position = 9,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "KilledByEvent",
				ParameterType = typeof(ModelId),
				Position = 10,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Players",
				ParameterType = typeof(List<RunHistoryPlayer>),
				Position = 11,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Acts",
				ParameterType = typeof(List<ModelId>),
				Position = 12,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Modifiers",
				ParameterType = typeof(List<SerializableModifier>),
				Position = 13,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "MapPointHistory",
				ParameterType = typeof(List<List<MapPointHistoryEntry>>),
				Position = 14,
				IsNullable = false,
				IsMemberInitializer = true
			}
		};
	}

	private JsonTypeInfo<RunHistoryPlayer> Create_RunHistoryPlayer(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<RunHistoryPlayer> jsonTypeInfo))
		{
			JsonObjectInfoValues<RunHistoryPlayer> jsonObjectInfoValues = new JsonObjectInfoValues<RunHistoryPlayer>();
			jsonObjectInfoValues.ObjectCreator = null;
			jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = (object[] args) => new RunHistoryPlayer
			{
				Id = (ulong)args[0],
				Character = (ModelId)args[1],
				Deck = (IEnumerable<SerializableCard>)args[2],
				Relics = (IEnumerable<SerializableRelic>)args[3],
				Potions = (IEnumerable<SerializablePotion>)args[4]
			};
			jsonObjectInfoValues.PropertyMetadataInitializer = (JsonSerializerContext _) => RunHistoryPlayerPropInit(options);
			jsonObjectInfoValues.ConstructorParameterMetadataInitializer = RunHistoryPlayerCtorParamInit;
			jsonObjectInfoValues.ConstructorAttributeProviderFactory = () => typeof(RunHistoryPlayer).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null);
			jsonObjectInfoValues.SerializeHandler = null;
			JsonObjectInfoValues<RunHistoryPlayer> objectInfo = jsonObjectInfoValues;
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] RunHistoryPlayerPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[6];
		JsonPropertyInfoValues<ulong> jsonPropertyInfoValues = new JsonPropertyInfoValues<ulong>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(RunHistoryPlayer);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((RunHistoryPlayer)obj).Id;
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
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(RunHistoryPlayer).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ulong), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ulong> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<ModelId> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(RunHistoryPlayer);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((RunHistoryPlayer)obj).Character;
		jsonPropertyInfoValues2.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "Character";
		jsonPropertyInfoValues2.JsonPropertyName = "character";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(RunHistoryPlayer).GetProperty("Character", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModelId), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModelId> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		JsonPropertyInfoValues<IEnumerable<SerializableCard>> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<IEnumerable<SerializableCard>>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(RunHistoryPlayer);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((RunHistoryPlayer)obj).Deck;
		jsonPropertyInfoValues3.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "Deck";
		jsonPropertyInfoValues3.JsonPropertyName = "deck";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(RunHistoryPlayer).GetProperty("Deck", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(IEnumerable<SerializableCard>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<IEnumerable<SerializableCard>> propertyInfo3 = jsonPropertyInfoValues3;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		array[2].IsGetNullable = false;
		array[2].IsSetNullable = false;
		JsonPropertyInfoValues<IEnumerable<SerializableRelic>> jsonPropertyInfoValues4 = new JsonPropertyInfoValues<IEnumerable<SerializableRelic>>();
		jsonPropertyInfoValues4.IsProperty = true;
		jsonPropertyInfoValues4.IsPublic = true;
		jsonPropertyInfoValues4.IsVirtual = false;
		jsonPropertyInfoValues4.DeclaringType = typeof(RunHistoryPlayer);
		jsonPropertyInfoValues4.Converter = null;
		jsonPropertyInfoValues4.Getter = (object obj) => ((RunHistoryPlayer)obj).Relics;
		jsonPropertyInfoValues4.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues4.IgnoreCondition = null;
		jsonPropertyInfoValues4.HasJsonInclude = false;
		jsonPropertyInfoValues4.IsExtensionData = false;
		jsonPropertyInfoValues4.NumberHandling = null;
		jsonPropertyInfoValues4.PropertyName = "Relics";
		jsonPropertyInfoValues4.JsonPropertyName = "relics";
		jsonPropertyInfoValues4.AttributeProviderFactory = () => typeof(RunHistoryPlayer).GetProperty("Relics", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(IEnumerable<SerializableRelic>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<IEnumerable<SerializableRelic>> propertyInfo4 = jsonPropertyInfoValues4;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		array[3].IsGetNullable = false;
		array[3].IsSetNullable = false;
		JsonPropertyInfoValues<IEnumerable<SerializablePotion>> jsonPropertyInfoValues5 = new JsonPropertyInfoValues<IEnumerable<SerializablePotion>>();
		jsonPropertyInfoValues5.IsProperty = true;
		jsonPropertyInfoValues5.IsPublic = true;
		jsonPropertyInfoValues5.IsVirtual = false;
		jsonPropertyInfoValues5.DeclaringType = typeof(RunHistoryPlayer);
		jsonPropertyInfoValues5.Converter = null;
		jsonPropertyInfoValues5.Getter = (object obj) => ((RunHistoryPlayer)obj).Potions;
		jsonPropertyInfoValues5.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues5.IgnoreCondition = null;
		jsonPropertyInfoValues5.HasJsonInclude = false;
		jsonPropertyInfoValues5.IsExtensionData = false;
		jsonPropertyInfoValues5.NumberHandling = null;
		jsonPropertyInfoValues5.PropertyName = "Potions";
		jsonPropertyInfoValues5.JsonPropertyName = "potions";
		jsonPropertyInfoValues5.AttributeProviderFactory = () => typeof(RunHistoryPlayer).GetProperty("Potions", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(IEnumerable<SerializablePotion>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<IEnumerable<SerializablePotion>> propertyInfo5 = jsonPropertyInfoValues5;
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		array[4].IsGetNullable = false;
		array[4].IsSetNullable = false;
		JsonPropertyInfoValues<int> jsonPropertyInfoValues6 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues6.IsProperty = true;
		jsonPropertyInfoValues6.IsPublic = true;
		jsonPropertyInfoValues6.IsVirtual = false;
		jsonPropertyInfoValues6.DeclaringType = typeof(RunHistoryPlayer);
		jsonPropertyInfoValues6.Converter = null;
		jsonPropertyInfoValues6.Getter = (object obj) => ((RunHistoryPlayer)obj).MaxPotionSlotCount;
		jsonPropertyInfoValues6.Setter = delegate(object obj, int value)
		{
			((RunHistoryPlayer)obj).MaxPotionSlotCount = value;
		};
		jsonPropertyInfoValues6.IgnoreCondition = null;
		jsonPropertyInfoValues6.HasJsonInclude = false;
		jsonPropertyInfoValues6.IsExtensionData = false;
		jsonPropertyInfoValues6.NumberHandling = null;
		jsonPropertyInfoValues6.PropertyName = "MaxPotionSlotCount";
		jsonPropertyInfoValues6.JsonPropertyName = "max_potion_slot_count";
		jsonPropertyInfoValues6.AttributeProviderFactory = () => typeof(RunHistoryPlayer).GetProperty("MaxPotionSlotCount", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo6 = jsonPropertyInfoValues6;
		array[5] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo6);
		return array;
	}

	private static JsonParameterInfoValues[] RunHistoryPlayerCtorParamInit()
	{
		return new JsonParameterInfoValues[5]
		{
			new JsonParameterInfoValues
			{
				Name = "Id",
				ParameterType = typeof(ulong),
				Position = 0,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Character",
				ParameterType = typeof(ModelId),
				Position = 1,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Deck",
				ParameterType = typeof(IEnumerable<SerializableCard>),
				Position = 2,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Relics",
				ParameterType = typeof(IEnumerable<SerializableRelic>),
				Position = 3,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "Potions",
				ParameterType = typeof(IEnumerable<SerializablePotion>),
				Position = 4,
				IsNullable = false,
				IsMemberInitializer = true
			}
		};
	}

	private JsonTypeInfo<AncientCharacterStats> Create_AncientCharacterStats(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<AncientCharacterStats> jsonTypeInfo))
		{
			JsonObjectInfoValues<AncientCharacterStats> jsonObjectInfoValues = new JsonObjectInfoValues<AncientCharacterStats>();
			jsonObjectInfoValues.ObjectCreator = null;
			jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = (object[] args) => new AncientCharacterStats
			{
				Character = (ModelId)args[0]
			};
			jsonObjectInfoValues.PropertyMetadataInitializer = (JsonSerializerContext _) => AncientCharacterStatsPropInit(options);
			jsonObjectInfoValues.ConstructorParameterMetadataInitializer = AncientCharacterStatsCtorParamInit;
			jsonObjectInfoValues.ConstructorAttributeProviderFactory = () => typeof(AncientCharacterStats).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null);
			jsonObjectInfoValues.SerializeHandler = null;
			JsonObjectInfoValues<AncientCharacterStats> objectInfo = jsonObjectInfoValues;
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] AncientCharacterStatsPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[4];
		JsonPropertyInfoValues<ModelId> jsonPropertyInfoValues = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(AncientCharacterStats);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((AncientCharacterStats)obj).Character;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Character";
		jsonPropertyInfoValues.JsonPropertyName = "character";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(AncientCharacterStats).GetProperty("Character", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModelId), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModelId> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsRequired = true;
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		JsonPropertyInfoValues<int> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(AncientCharacterStats);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((AncientCharacterStats)obj).Wins;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((AncientCharacterStats)obj).Wins = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "Wins";
		jsonPropertyInfoValues2.JsonPropertyName = "wins";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(AncientCharacterStats).GetProperty("Wins", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(AncientCharacterStats);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((AncientCharacterStats)obj).Losses;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((AncientCharacterStats)obj).Losses = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "Losses";
		jsonPropertyInfoValues2.JsonPropertyName = "losses";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(AncientCharacterStats).GetProperty("Losses", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo3 = jsonPropertyInfoValues2;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(AncientCharacterStats);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = null;
		jsonPropertyInfoValues2.Setter = null;
		jsonPropertyInfoValues2.IgnoreCondition = JsonIgnoreCondition.Always;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "Visits";
		jsonPropertyInfoValues2.JsonPropertyName = null;
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(AncientCharacterStats).GetProperty("Visits", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo4 = jsonPropertyInfoValues2;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		return array;
	}

	private static JsonParameterInfoValues[] AncientCharacterStatsCtorParamInit()
	{
		return new JsonParameterInfoValues[1]
		{
			new JsonParameterInfoValues
			{
				Name = "Character",
				ParameterType = typeof(ModelId),
				Position = 0,
				IsNullable = false,
				IsMemberInitializer = true
			}
		};
	}

	private JsonTypeInfo<AncientStats> Create_AncientStats(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<AncientStats> jsonTypeInfo))
		{
			JsonObjectInfoValues<AncientStats> jsonObjectInfoValues = new JsonObjectInfoValues<AncientStats>();
			jsonObjectInfoValues.ObjectCreator = null;
			jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = (object[] args) => new AncientStats
			{
				Id = (ModelId)args[0],
				CharStats = (List<AncientCharacterStats>)args[1]
			};
			jsonObjectInfoValues.PropertyMetadataInitializer = (JsonSerializerContext _) => AncientStatsPropInit(options);
			jsonObjectInfoValues.ConstructorParameterMetadataInitializer = AncientStatsCtorParamInit;
			jsonObjectInfoValues.ConstructorAttributeProviderFactory = () => typeof(AncientStats).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null);
			jsonObjectInfoValues.SerializeHandler = null;
			JsonObjectInfoValues<AncientStats> objectInfo = jsonObjectInfoValues;
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] AncientStatsPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[5];
		JsonPropertyInfoValues<ModelId> jsonPropertyInfoValues = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(AncientStats);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((AncientStats)obj).Id;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Id";
		jsonPropertyInfoValues.JsonPropertyName = "ancient_id";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(AncientStats).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModelId), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModelId> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsRequired = true;
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		JsonPropertyInfoValues<List<AncientCharacterStats>> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<List<AncientCharacterStats>>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(AncientStats);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((AncientStats)obj).CharStats;
		jsonPropertyInfoValues2.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "CharStats";
		jsonPropertyInfoValues2.JsonPropertyName = "character_stats";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(AncientStats).GetProperty("CharStats", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<AncientCharacterStats>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<AncientCharacterStats>> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		JsonPropertyInfoValues<int> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(AncientStats);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = null;
		jsonPropertyInfoValues3.Setter = null;
		jsonPropertyInfoValues3.IgnoreCondition = JsonIgnoreCondition.Always;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "TotalVisits";
		jsonPropertyInfoValues3.JsonPropertyName = null;
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(AncientStats).GetProperty("TotalVisits", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo3 = jsonPropertyInfoValues3;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		jsonPropertyInfoValues3 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(AncientStats);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = null;
		jsonPropertyInfoValues3.Setter = null;
		jsonPropertyInfoValues3.IgnoreCondition = JsonIgnoreCondition.Always;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "TotalWins";
		jsonPropertyInfoValues3.JsonPropertyName = null;
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(AncientStats).GetProperty("TotalWins", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo4 = jsonPropertyInfoValues3;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		jsonPropertyInfoValues3 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(AncientStats);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = null;
		jsonPropertyInfoValues3.Setter = null;
		jsonPropertyInfoValues3.IgnoreCondition = JsonIgnoreCondition.Always;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "TotalLosses";
		jsonPropertyInfoValues3.JsonPropertyName = null;
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(AncientStats).GetProperty("TotalLosses", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo5 = jsonPropertyInfoValues3;
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		return array;
	}

	private static JsonParameterInfoValues[] AncientStatsCtorParamInit()
	{
		return new JsonParameterInfoValues[2]
		{
			new JsonParameterInfoValues
			{
				Name = "Id",
				ParameterType = typeof(ModelId),
				Position = 0,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "CharStats",
				ParameterType = typeof(List<AncientCharacterStats>),
				Position = 1,
				IsNullable = false,
				IsMemberInitializer = true
			}
		};
	}

	private JsonTypeInfo<CardStats> Create_CardStats(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<CardStats> jsonTypeInfo))
		{
			JsonObjectInfoValues<CardStats> jsonObjectInfoValues = new JsonObjectInfoValues<CardStats>();
			jsonObjectInfoValues.ObjectCreator = null;
			jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = (object[] args) => new CardStats
			{
				Id = (ModelId)args[0]
			};
			jsonObjectInfoValues.PropertyMetadataInitializer = (JsonSerializerContext _) => CardStatsPropInit(options);
			jsonObjectInfoValues.ConstructorParameterMetadataInitializer = CardStatsCtorParamInit;
			jsonObjectInfoValues.ConstructorAttributeProviderFactory = () => typeof(CardStats).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null);
			jsonObjectInfoValues.SerializeHandler = null;
			JsonObjectInfoValues<CardStats> objectInfo = jsonObjectInfoValues;
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] CardStatsPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[5];
		JsonPropertyInfoValues<ModelId> jsonPropertyInfoValues = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(CardStats);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((CardStats)obj).Id;
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
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(CardStats).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModelId), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModelId> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<long> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<long>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(CardStats);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((CardStats)obj).TimesPicked;
		jsonPropertyInfoValues2.Setter = delegate(object obj, long value)
		{
			((CardStats)obj).TimesPicked = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "TimesPicked";
		jsonPropertyInfoValues2.JsonPropertyName = "times_picked";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(CardStats).GetProperty("TimesPicked", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(long), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<long> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<long>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(CardStats);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((CardStats)obj).TimesSkipped;
		jsonPropertyInfoValues2.Setter = delegate(object obj, long value)
		{
			((CardStats)obj).TimesSkipped = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "TimesSkipped";
		jsonPropertyInfoValues2.JsonPropertyName = "times_skipped";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(CardStats).GetProperty("TimesSkipped", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(long), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<long> propertyInfo3 = jsonPropertyInfoValues2;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<long>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(CardStats);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((CardStats)obj).TimesWon;
		jsonPropertyInfoValues2.Setter = delegate(object obj, long value)
		{
			((CardStats)obj).TimesWon = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "TimesWon";
		jsonPropertyInfoValues2.JsonPropertyName = "times_won";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(CardStats).GetProperty("TimesWon", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(long), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<long> propertyInfo4 = jsonPropertyInfoValues2;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<long>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(CardStats);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((CardStats)obj).TimesLost;
		jsonPropertyInfoValues2.Setter = delegate(object obj, long value)
		{
			((CardStats)obj).TimesLost = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "TimesLost";
		jsonPropertyInfoValues2.JsonPropertyName = "times_lost";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(CardStats).GetProperty("TimesLost", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(long), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<long> propertyInfo5 = jsonPropertyInfoValues2;
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		return array;
	}

	private static JsonParameterInfoValues[] CardStatsCtorParamInit()
	{
		return new JsonParameterInfoValues[1]
		{
			new JsonParameterInfoValues
			{
				Name = "Id",
				ParameterType = typeof(ModelId),
				Position = 0,
				IsNullable = true,
				IsMemberInitializer = true
			}
		};
	}

	private JsonTypeInfo<CharacterStats> Create_CharacterStats(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<CharacterStats> jsonTypeInfo))
		{
			JsonObjectInfoValues<CharacterStats> jsonObjectInfoValues = new JsonObjectInfoValues<CharacterStats>();
			jsonObjectInfoValues.ObjectCreator = null;
			jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = (object[] args) => new CharacterStats
			{
				Id = (ModelId)args[0]
			};
			jsonObjectInfoValues.PropertyMetadataInitializer = (JsonSerializerContext _) => CharacterStatsPropInit(options);
			jsonObjectInfoValues.ConstructorParameterMetadataInitializer = CharacterStatsCtorParamInit;
			jsonObjectInfoValues.ConstructorAttributeProviderFactory = () => typeof(CharacterStats).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null);
			jsonObjectInfoValues.SerializeHandler = null;
			JsonObjectInfoValues<CharacterStats> objectInfo = jsonObjectInfoValues;
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] CharacterStatsPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[9];
		JsonPropertyInfoValues<ModelId> jsonPropertyInfoValues = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(CharacterStats);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((CharacterStats)obj).Id;
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
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(CharacterStats).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModelId), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModelId> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<int> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(CharacterStats);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((CharacterStats)obj).MaxAscension;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((CharacterStats)obj).MaxAscension = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "MaxAscension";
		jsonPropertyInfoValues2.JsonPropertyName = "max_ascension";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(CharacterStats).GetProperty("MaxAscension", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(CharacterStats);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((CharacterStats)obj).PreferredAscension;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((CharacterStats)obj).PreferredAscension = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "PreferredAscension";
		jsonPropertyInfoValues2.JsonPropertyName = "preferred_ascension";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(CharacterStats).GetProperty("PreferredAscension", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo3 = jsonPropertyInfoValues2;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(CharacterStats);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((CharacterStats)obj).TotalWins;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((CharacterStats)obj).TotalWins = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "TotalWins";
		jsonPropertyInfoValues2.JsonPropertyName = "total_wins";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(CharacterStats).GetProperty("TotalWins", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo4 = jsonPropertyInfoValues2;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(CharacterStats);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((CharacterStats)obj).TotalLosses;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((CharacterStats)obj).TotalLosses = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "TotalLosses";
		jsonPropertyInfoValues2.JsonPropertyName = "total_losses";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(CharacterStats).GetProperty("TotalLosses", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo5 = jsonPropertyInfoValues2;
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		JsonPropertyInfoValues<long> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<long>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(CharacterStats);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((CharacterStats)obj).FastestWinTime;
		jsonPropertyInfoValues3.Setter = delegate(object obj, long value)
		{
			((CharacterStats)obj).FastestWinTime = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "FastestWinTime";
		jsonPropertyInfoValues3.JsonPropertyName = "fastest_win_time";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(CharacterStats).GetProperty("FastestWinTime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(long), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<long> propertyInfo6 = jsonPropertyInfoValues3;
		array[5] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo6);
		jsonPropertyInfoValues3 = new JsonPropertyInfoValues<long>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(CharacterStats);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((CharacterStats)obj).BestWinStreak;
		jsonPropertyInfoValues3.Setter = delegate(object obj, long value)
		{
			((CharacterStats)obj).BestWinStreak = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "BestWinStreak";
		jsonPropertyInfoValues3.JsonPropertyName = "best_win_streak";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(CharacterStats).GetProperty("BestWinStreak", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(long), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<long> propertyInfo7 = jsonPropertyInfoValues3;
		array[6] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo7);
		jsonPropertyInfoValues3 = new JsonPropertyInfoValues<long>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(CharacterStats);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((CharacterStats)obj).CurrentWinStreak;
		jsonPropertyInfoValues3.Setter = delegate(object obj, long value)
		{
			((CharacterStats)obj).CurrentWinStreak = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "CurrentWinStreak";
		jsonPropertyInfoValues3.JsonPropertyName = "current_streak";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(CharacterStats).GetProperty("CurrentWinStreak", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(long), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<long> propertyInfo8 = jsonPropertyInfoValues3;
		array[7] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo8);
		jsonPropertyInfoValues3 = new JsonPropertyInfoValues<long>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(CharacterStats);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((CharacterStats)obj).Playtime;
		jsonPropertyInfoValues3.Setter = delegate(object obj, long value)
		{
			((CharacterStats)obj).Playtime = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "Playtime";
		jsonPropertyInfoValues3.JsonPropertyName = "playtime";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(CharacterStats).GetProperty("Playtime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(long), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<long> propertyInfo9 = jsonPropertyInfoValues3;
		array[8] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo9);
		return array;
	}

	private static JsonParameterInfoValues[] CharacterStatsCtorParamInit()
	{
		return new JsonParameterInfoValues[1]
		{
			new JsonParameterInfoValues
			{
				Name = "Id",
				ParameterType = typeof(ModelId),
				Position = 0,
				IsNullable = true,
				IsMemberInitializer = true
			}
		};
	}

	private JsonTypeInfo<EncounterStats> Create_EncounterStats(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<EncounterStats> jsonTypeInfo))
		{
			JsonObjectInfoValues<EncounterStats> jsonObjectInfoValues = new JsonObjectInfoValues<EncounterStats>();
			jsonObjectInfoValues.ObjectCreator = null;
			jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = (object[] args) => new EncounterStats
			{
				Id = (ModelId)args[0],
				FightStats = (List<FightStats>)args[1]
			};
			jsonObjectInfoValues.PropertyMetadataInitializer = (JsonSerializerContext _) => EncounterStatsPropInit(options);
			jsonObjectInfoValues.ConstructorParameterMetadataInitializer = EncounterStatsCtorParamInit;
			jsonObjectInfoValues.ConstructorAttributeProviderFactory = () => typeof(EncounterStats).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null);
			jsonObjectInfoValues.SerializeHandler = null;
			JsonObjectInfoValues<EncounterStats> objectInfo = jsonObjectInfoValues;
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] EncounterStatsPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[4];
		JsonPropertyInfoValues<ModelId> jsonPropertyInfoValues = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(EncounterStats);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((EncounterStats)obj).Id;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Id";
		jsonPropertyInfoValues.JsonPropertyName = "encounter_id";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(EncounterStats).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModelId), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModelId> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsRequired = true;
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		JsonPropertyInfoValues<List<FightStats>> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<List<FightStats>>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(EncounterStats);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((EncounterStats)obj).FightStats;
		jsonPropertyInfoValues2.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "FightStats";
		jsonPropertyInfoValues2.JsonPropertyName = "fight_stats";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(EncounterStats).GetProperty("FightStats", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<FightStats>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<FightStats>> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		JsonPropertyInfoValues<int> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(EncounterStats);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = null;
		jsonPropertyInfoValues3.Setter = null;
		jsonPropertyInfoValues3.IgnoreCondition = JsonIgnoreCondition.Always;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "TotalWins";
		jsonPropertyInfoValues3.JsonPropertyName = null;
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(EncounterStats).GetProperty("TotalWins", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo3 = jsonPropertyInfoValues3;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		jsonPropertyInfoValues3 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(EncounterStats);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = null;
		jsonPropertyInfoValues3.Setter = null;
		jsonPropertyInfoValues3.IgnoreCondition = JsonIgnoreCondition.Always;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "TotalLosses";
		jsonPropertyInfoValues3.JsonPropertyName = null;
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(EncounterStats).GetProperty("TotalLosses", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo4 = jsonPropertyInfoValues3;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		return array;
	}

	private static JsonParameterInfoValues[] EncounterStatsCtorParamInit()
	{
		return new JsonParameterInfoValues[2]
		{
			new JsonParameterInfoValues
			{
				Name = "Id",
				ParameterType = typeof(ModelId),
				Position = 0,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "FightStats",
				ParameterType = typeof(List<FightStats>),
				Position = 1,
				IsNullable = false,
				IsMemberInitializer = true
			}
		};
	}

	private JsonTypeInfo<EnemyStats> Create_EnemyStats(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<EnemyStats> jsonTypeInfo))
		{
			JsonObjectInfoValues<EnemyStats> jsonObjectInfoValues = new JsonObjectInfoValues<EnemyStats>();
			jsonObjectInfoValues.ObjectCreator = null;
			jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = (object[] args) => new EnemyStats
			{
				Id = (ModelId)args[0],
				FightStats = (List<FightStats>)args[1]
			};
			jsonObjectInfoValues.PropertyMetadataInitializer = (JsonSerializerContext _) => EnemyStatsPropInit(options);
			jsonObjectInfoValues.ConstructorParameterMetadataInitializer = EnemyStatsCtorParamInit;
			jsonObjectInfoValues.ConstructorAttributeProviderFactory = () => typeof(EnemyStats).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null);
			jsonObjectInfoValues.SerializeHandler = null;
			JsonObjectInfoValues<EnemyStats> objectInfo = jsonObjectInfoValues;
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] EnemyStatsPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[4];
		JsonPropertyInfoValues<ModelId> jsonPropertyInfoValues = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(EnemyStats);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((EnemyStats)obj).Id;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Id";
		jsonPropertyInfoValues.JsonPropertyName = "enemy_id";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(EnemyStats).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModelId), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModelId> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsRequired = true;
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		JsonPropertyInfoValues<List<FightStats>> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<List<FightStats>>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(EnemyStats);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((EnemyStats)obj).FightStats;
		jsonPropertyInfoValues2.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "FightStats";
		jsonPropertyInfoValues2.JsonPropertyName = "fight_stats";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(EnemyStats).GetProperty("FightStats", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<FightStats>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<FightStats>> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		JsonPropertyInfoValues<int> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(EnemyStats);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = null;
		jsonPropertyInfoValues3.Setter = null;
		jsonPropertyInfoValues3.IgnoreCondition = JsonIgnoreCondition.Always;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "TotalWins";
		jsonPropertyInfoValues3.JsonPropertyName = null;
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(EnemyStats).GetProperty("TotalWins", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo3 = jsonPropertyInfoValues3;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		jsonPropertyInfoValues3 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(EnemyStats);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = null;
		jsonPropertyInfoValues3.Setter = null;
		jsonPropertyInfoValues3.IgnoreCondition = JsonIgnoreCondition.Always;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "TotalLosses";
		jsonPropertyInfoValues3.JsonPropertyName = null;
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(EnemyStats).GetProperty("TotalLosses", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo4 = jsonPropertyInfoValues3;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		return array;
	}

	private static JsonParameterInfoValues[] EnemyStatsCtorParamInit()
	{
		return new JsonParameterInfoValues[2]
		{
			new JsonParameterInfoValues
			{
				Name = "Id",
				ParameterType = typeof(ModelId),
				Position = 0,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "FightStats",
				ParameterType = typeof(List<FightStats>),
				Position = 1,
				IsNullable = false,
				IsMemberInitializer = true
			}
		};
	}

	private JsonTypeInfo<EpochState> Create_EpochState(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<EpochState> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<EpochState>(options, JsonMetadataServices.GetEnumConverter<EpochState>(options));
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<FightStats> Create_FightStats(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<FightStats> jsonTypeInfo))
		{
			JsonObjectInfoValues<FightStats> jsonObjectInfoValues = new JsonObjectInfoValues<FightStats>();
			jsonObjectInfoValues.ObjectCreator = null;
			jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = (object[] args) => new FightStats
			{
				Character = (ModelId)args[0]
			};
			jsonObjectInfoValues.PropertyMetadataInitializer = (JsonSerializerContext _) => FightStatsPropInit(options);
			jsonObjectInfoValues.ConstructorParameterMetadataInitializer = FightStatsCtorParamInit;
			jsonObjectInfoValues.ConstructorAttributeProviderFactory = () => typeof(FightStats).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null);
			jsonObjectInfoValues.SerializeHandler = null;
			JsonObjectInfoValues<FightStats> objectInfo = jsonObjectInfoValues;
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] FightStatsPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[3];
		JsonPropertyInfoValues<ModelId> jsonPropertyInfoValues = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(FightStats);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((FightStats)obj).Character;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Character";
		jsonPropertyInfoValues.JsonPropertyName = "character";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(FightStats).GetProperty("Character", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModelId), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModelId> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsRequired = true;
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		JsonPropertyInfoValues<int> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(FightStats);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((FightStats)obj).Wins;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((FightStats)obj).Wins = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "Wins";
		jsonPropertyInfoValues2.JsonPropertyName = "wins";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(FightStats).GetProperty("Wins", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(FightStats);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((FightStats)obj).Losses;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((FightStats)obj).Losses = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "Losses";
		jsonPropertyInfoValues2.JsonPropertyName = "losses";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(FightStats).GetProperty("Losses", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo3 = jsonPropertyInfoValues2;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		return array;
	}

	private static JsonParameterInfoValues[] FightStatsCtorParamInit()
	{
		return new JsonParameterInfoValues[1]
		{
			new JsonParameterInfoValues
			{
				Name = "Character",
				ParameterType = typeof(ModelId),
				Position = 0,
				IsNullable = false,
				IsMemberInitializer = true
			}
		};
	}

	private JsonTypeInfo<SerializableMapDrawingLine> Create_SerializableMapDrawingLine(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SerializableMapDrawingLine> jsonTypeInfo))
		{
			JsonObjectInfoValues<SerializableMapDrawingLine> objectInfo = new JsonObjectInfoValues<SerializableMapDrawingLine>
			{
				ObjectCreator = () => new SerializableMapDrawingLine(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SerializableMapDrawingLinePropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SerializableMapDrawingLine).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SerializableMapDrawingLinePropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<bool> jsonPropertyInfoValues = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableMapDrawingLine);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableMapDrawingLine)obj).isEraser;
		jsonPropertyInfoValues.Setter = delegate(object obj, bool value)
		{
			((SerializableMapDrawingLine)obj).isEraser = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "isEraser";
		jsonPropertyInfoValues.JsonPropertyName = null;
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableMapDrawingLine).GetField("isEraser", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<bool> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<List<Vector2>> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<List<Vector2>>();
		jsonPropertyInfoValues2.IsProperty = false;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SerializableMapDrawingLine);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SerializableMapDrawingLine)obj).mapPoints;
		jsonPropertyInfoValues2.Setter = delegate(object obj, List<Vector2>? value)
		{
			((SerializableMapDrawingLine)obj).mapPoints = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "mapPoints";
		jsonPropertyInfoValues2.JsonPropertyName = null;
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SerializableMapDrawingLine).GetField("mapPoints", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<List<Vector2>> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		return array;
	}

	private JsonTypeInfo<SerializableMapDrawings> Create_SerializableMapDrawings(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SerializableMapDrawings> jsonTypeInfo))
		{
			JsonObjectInfoValues<SerializableMapDrawings> objectInfo = new JsonObjectInfoValues<SerializableMapDrawings>
			{
				ObjectCreator = () => new SerializableMapDrawings(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SerializableMapDrawingsPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SerializableMapDrawings).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SerializableMapDrawingsPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[1];
		JsonPropertyInfoValues<List<SerializablePlayerMapDrawings>> jsonPropertyInfoValues = new JsonPropertyInfoValues<List<SerializablePlayerMapDrawings>>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableMapDrawings);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableMapDrawings)obj).drawings;
		jsonPropertyInfoValues.Setter = delegate(object obj, List<SerializablePlayerMapDrawings>? value)
		{
			((SerializableMapDrawings)obj).drawings = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "drawings";
		jsonPropertyInfoValues.JsonPropertyName = null;
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableMapDrawings).GetField("drawings", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<List<SerializablePlayerMapDrawings>> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		return array;
	}

	private JsonTypeInfo<SerializablePlayerMapDrawings> Create_SerializablePlayerMapDrawings(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SerializablePlayerMapDrawings> jsonTypeInfo))
		{
			JsonObjectInfoValues<SerializablePlayerMapDrawings> objectInfo = new JsonObjectInfoValues<SerializablePlayerMapDrawings>
			{
				ObjectCreator = () => new SerializablePlayerMapDrawings(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SerializablePlayerMapDrawingsPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SerializablePlayerMapDrawings).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SerializablePlayerMapDrawingsPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<ulong> jsonPropertyInfoValues = new JsonPropertyInfoValues<ulong>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializablePlayerMapDrawings);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializablePlayerMapDrawings)obj).playerId;
		jsonPropertyInfoValues.Setter = delegate(object obj, ulong value)
		{
			((SerializablePlayerMapDrawings)obj).playerId = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "playerId";
		jsonPropertyInfoValues.JsonPropertyName = null;
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializablePlayerMapDrawings).GetField("playerId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<ulong> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<List<SerializableMapDrawingLine>> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<List<SerializableMapDrawingLine>>();
		jsonPropertyInfoValues2.IsProperty = false;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SerializablePlayerMapDrawings);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SerializablePlayerMapDrawings)obj).lines;
		jsonPropertyInfoValues2.Setter = delegate(object obj, List<SerializableMapDrawingLine>? value)
		{
			((SerializablePlayerMapDrawings)obj).lines = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "lines";
		jsonPropertyInfoValues2.JsonPropertyName = null;
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SerializablePlayerMapDrawings).GetField("lines", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<List<SerializableMapDrawingLine>> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		return array;
	}

	private JsonTypeInfo<MigratingData> Create_MigratingData(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<MigratingData> jsonTypeInfo))
		{
			JsonObjectInfoValues<MigratingData> objectInfo = new JsonObjectInfoValues<MigratingData>
			{
				ObjectCreator = null,
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => MigratingDataPropInit(options),
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

	private static JsonPropertyInfo[] MigratingDataPropInit(JsonSerializerOptions options)
	{
		return new JsonPropertyInfo[0];
	}

	private JsonTypeInfo<PrefsSave> Create_PrefsSave(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<PrefsSave> jsonTypeInfo))
		{
			JsonObjectInfoValues<PrefsSave> objectInfo = new JsonObjectInfoValues<PrefsSave>
			{
				ObjectCreator = () => new PrefsSave(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => PrefsSavePropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(PrefsSave).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] PrefsSavePropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[9];
		JsonPropertyInfoValues<int> jsonPropertyInfoValues = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(PrefsSave);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((PrefsSave)obj).SchemaVersion;
		jsonPropertyInfoValues.Setter = delegate(object obj, int value)
		{
			((PrefsSave)obj).SchemaVersion = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "SchemaVersion";
		jsonPropertyInfoValues.JsonPropertyName = "schema_version";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(PrefsSave).GetProperty("SchemaVersion", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<FastModeType> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<FastModeType>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(PrefsSave);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((PrefsSave)obj).FastMode;
		jsonPropertyInfoValues2.Setter = delegate(object obj, FastModeType value)
		{
			((PrefsSave)obj).FastMode = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "FastMode";
		jsonPropertyInfoValues2.JsonPropertyName = "fast_mode";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(PrefsSave).GetProperty("FastMode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(FastModeType), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<FastModeType> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		jsonPropertyInfoValues = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(PrefsSave);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((PrefsSave)obj).ScreenShakeOptionIndex;
		jsonPropertyInfoValues.Setter = delegate(object obj, int value)
		{
			((PrefsSave)obj).ScreenShakeOptionIndex = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "ScreenShakeOptionIndex";
		jsonPropertyInfoValues.JsonPropertyName = "screenshake";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(PrefsSave).GetProperty("ScreenShakeOptionIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo3 = jsonPropertyInfoValues;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		JsonPropertyInfoValues<bool> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(PrefsSave);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((PrefsSave)obj).ShowRunTimer;
		jsonPropertyInfoValues3.Setter = delegate(object obj, bool value)
		{
			((PrefsSave)obj).ShowRunTimer = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "ShowRunTimer";
		jsonPropertyInfoValues3.JsonPropertyName = "show_run_timer";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(PrefsSave).GetProperty("ShowRunTimer", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<bool> propertyInfo4 = jsonPropertyInfoValues3;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		jsonPropertyInfoValues3 = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(PrefsSave);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((PrefsSave)obj).ShowCardIndices;
		jsonPropertyInfoValues3.Setter = delegate(object obj, bool value)
		{
			((PrefsSave)obj).ShowCardIndices = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "ShowCardIndices";
		jsonPropertyInfoValues3.JsonPropertyName = "show_card_indices";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(PrefsSave).GetProperty("ShowCardIndices", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<bool> propertyInfo5 = jsonPropertyInfoValues3;
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		jsonPropertyInfoValues3 = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(PrefsSave);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((PrefsSave)obj).UploadData;
		jsonPropertyInfoValues3.Setter = delegate(object obj, bool value)
		{
			((PrefsSave)obj).UploadData = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "UploadData";
		jsonPropertyInfoValues3.JsonPropertyName = "upload_data";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(PrefsSave).GetProperty("UploadData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<bool> propertyInfo6 = jsonPropertyInfoValues3;
		array[5] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo6);
		jsonPropertyInfoValues3 = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(PrefsSave);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((PrefsSave)obj).MuteInBackground;
		jsonPropertyInfoValues3.Setter = delegate(object obj, bool value)
		{
			((PrefsSave)obj).MuteInBackground = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "MuteInBackground";
		jsonPropertyInfoValues3.JsonPropertyName = "mute_in_background";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(PrefsSave).GetProperty("MuteInBackground", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<bool> propertyInfo7 = jsonPropertyInfoValues3;
		array[6] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo7);
		jsonPropertyInfoValues3 = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(PrefsSave);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((PrefsSave)obj).IsLongPressEnabled;
		jsonPropertyInfoValues3.Setter = delegate(object obj, bool value)
		{
			((PrefsSave)obj).IsLongPressEnabled = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "IsLongPressEnabled";
		jsonPropertyInfoValues3.JsonPropertyName = "long_press";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(PrefsSave).GetProperty("IsLongPressEnabled", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<bool> propertyInfo8 = jsonPropertyInfoValues3;
		array[7] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo8);
		jsonPropertyInfoValues3 = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(PrefsSave);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((PrefsSave)obj).TextEffectsEnabled;
		jsonPropertyInfoValues3.Setter = delegate(object obj, bool value)
		{
			((PrefsSave)obj).TextEffectsEnabled = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "TextEffectsEnabled";
		jsonPropertyInfoValues3.JsonPropertyName = "text_effects_enabled";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(PrefsSave).GetProperty("TextEffectsEnabled", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<bool> propertyInfo9 = jsonPropertyInfoValues3;
		array[8] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo9);
		return array;
	}

	private JsonTypeInfo<ProfileSave> Create_ProfileSave(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<ProfileSave> jsonTypeInfo))
		{
			JsonObjectInfoValues<ProfileSave> objectInfo = new JsonObjectInfoValues<ProfileSave>
			{
				ObjectCreator = () => new ProfileSave(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => ProfileSavePropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(ProfileSave).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] ProfileSavePropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<int> jsonPropertyInfoValues = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(ProfileSave);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((ProfileSave)obj).SchemaVersion;
		jsonPropertyInfoValues.Setter = delegate(object obj, int value)
		{
			((ProfileSave)obj).SchemaVersion = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "SchemaVersion";
		jsonPropertyInfoValues.JsonPropertyName = "schema_version";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(ProfileSave).GetProperty("SchemaVersion", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		jsonPropertyInfoValues = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(ProfileSave);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((ProfileSave)obj).LastProfileId;
		jsonPropertyInfoValues.Setter = delegate(object obj, int value)
		{
			((ProfileSave)obj).LastProfileId = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "LastProfileId";
		jsonPropertyInfoValues.JsonPropertyName = "last_profile_id";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(ProfileSave).GetProperty("LastProfileId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo2 = jsonPropertyInfoValues;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		return array;
	}

	private JsonTypeInfo<SavedProperties> Create_SavedProperties(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SavedProperties> jsonTypeInfo))
		{
			JsonObjectInfoValues<SavedProperties> objectInfo = new JsonObjectInfoValues<SavedProperties>
			{
				ObjectCreator = () => new SavedProperties(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SavedPropertiesPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SavedProperties).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SavedPropertiesPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[7];
		JsonPropertyInfoValues<List<SavedProperties.SavedProperty<int>>> jsonPropertyInfoValues = new JsonPropertyInfoValues<List<SavedProperties.SavedProperty<int>>>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SavedProperties);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SavedProperties)obj).ints;
		jsonPropertyInfoValues.Setter = delegate(object obj, List<SavedProperties.SavedProperty<int>>? value)
		{
			((SavedProperties)obj).ints = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "ints";
		jsonPropertyInfoValues.JsonPropertyName = "ints";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SavedProperties).GetField("ints", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<List<SavedProperties.SavedProperty<int>>> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<List<SavedProperties.SavedProperty<bool>>> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<List<SavedProperties.SavedProperty<bool>>>();
		jsonPropertyInfoValues2.IsProperty = false;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SavedProperties);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SavedProperties)obj).bools;
		jsonPropertyInfoValues2.Setter = delegate(object obj, List<SavedProperties.SavedProperty<bool>>? value)
		{
			((SavedProperties)obj).bools = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "bools";
		jsonPropertyInfoValues2.JsonPropertyName = "bools";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SavedProperties).GetField("bools", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<List<SavedProperties.SavedProperty<bool>>> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<List<SavedProperties.SavedProperty<string>>> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<List<SavedProperties.SavedProperty<string>>>();
		jsonPropertyInfoValues3.IsProperty = false;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(SavedProperties);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((SavedProperties)obj).strings;
		jsonPropertyInfoValues3.Setter = delegate(object obj, List<SavedProperties.SavedProperty<string>>? value)
		{
			((SavedProperties)obj).strings = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "strings";
		jsonPropertyInfoValues3.JsonPropertyName = "strings";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(SavedProperties).GetField("strings", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<List<SavedProperties.SavedProperty<string>>> propertyInfo3 = jsonPropertyInfoValues3;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		JsonPropertyInfoValues<List<SavedProperties.SavedProperty<int[]>>> jsonPropertyInfoValues4 = new JsonPropertyInfoValues<List<SavedProperties.SavedProperty<int[]>>>();
		jsonPropertyInfoValues4.IsProperty = false;
		jsonPropertyInfoValues4.IsPublic = true;
		jsonPropertyInfoValues4.IsVirtual = false;
		jsonPropertyInfoValues4.DeclaringType = typeof(SavedProperties);
		jsonPropertyInfoValues4.Converter = null;
		jsonPropertyInfoValues4.Getter = (object obj) => ((SavedProperties)obj).intArrays;
		jsonPropertyInfoValues4.Setter = delegate(object obj, List<SavedProperties.SavedProperty<int[]>>? value)
		{
			((SavedProperties)obj).intArrays = value;
		};
		jsonPropertyInfoValues4.IgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
		jsonPropertyInfoValues4.HasJsonInclude = false;
		jsonPropertyInfoValues4.IsExtensionData = false;
		jsonPropertyInfoValues4.NumberHandling = null;
		jsonPropertyInfoValues4.PropertyName = "intArrays";
		jsonPropertyInfoValues4.JsonPropertyName = "int_arrays";
		jsonPropertyInfoValues4.AttributeProviderFactory = () => typeof(SavedProperties).GetField("intArrays", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<List<SavedProperties.SavedProperty<int[]>>> propertyInfo4 = jsonPropertyInfoValues4;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		JsonPropertyInfoValues<List<SavedProperties.SavedProperty<ModelId>>> jsonPropertyInfoValues5 = new JsonPropertyInfoValues<List<SavedProperties.SavedProperty<ModelId>>>();
		jsonPropertyInfoValues5.IsProperty = false;
		jsonPropertyInfoValues5.IsPublic = true;
		jsonPropertyInfoValues5.IsVirtual = false;
		jsonPropertyInfoValues5.DeclaringType = typeof(SavedProperties);
		jsonPropertyInfoValues5.Converter = null;
		jsonPropertyInfoValues5.Getter = (object obj) => ((SavedProperties)obj).modelIds;
		jsonPropertyInfoValues5.Setter = delegate(object obj, List<SavedProperties.SavedProperty<ModelId>>? value)
		{
			((SavedProperties)obj).modelIds = value;
		};
		jsonPropertyInfoValues5.IgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
		jsonPropertyInfoValues5.HasJsonInclude = false;
		jsonPropertyInfoValues5.IsExtensionData = false;
		jsonPropertyInfoValues5.NumberHandling = null;
		jsonPropertyInfoValues5.PropertyName = "modelIds";
		jsonPropertyInfoValues5.JsonPropertyName = "model_ids";
		jsonPropertyInfoValues5.AttributeProviderFactory = () => typeof(SavedProperties).GetField("modelIds", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<List<SavedProperties.SavedProperty<ModelId>>> propertyInfo5 = jsonPropertyInfoValues5;
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		JsonPropertyInfoValues<List<SavedProperties.SavedProperty<SerializableCard>>> jsonPropertyInfoValues6 = new JsonPropertyInfoValues<List<SavedProperties.SavedProperty<SerializableCard>>>();
		jsonPropertyInfoValues6.IsProperty = false;
		jsonPropertyInfoValues6.IsPublic = true;
		jsonPropertyInfoValues6.IsVirtual = false;
		jsonPropertyInfoValues6.DeclaringType = typeof(SavedProperties);
		jsonPropertyInfoValues6.Converter = null;
		jsonPropertyInfoValues6.Getter = (object obj) => ((SavedProperties)obj).cards;
		jsonPropertyInfoValues6.Setter = delegate(object obj, List<SavedProperties.SavedProperty<SerializableCard>>? value)
		{
			((SavedProperties)obj).cards = value;
		};
		jsonPropertyInfoValues6.IgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
		jsonPropertyInfoValues6.HasJsonInclude = false;
		jsonPropertyInfoValues6.IsExtensionData = false;
		jsonPropertyInfoValues6.NumberHandling = null;
		jsonPropertyInfoValues6.PropertyName = "cards";
		jsonPropertyInfoValues6.JsonPropertyName = "cards";
		jsonPropertyInfoValues6.AttributeProviderFactory = () => typeof(SavedProperties).GetField("cards", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<List<SavedProperties.SavedProperty<SerializableCard>>> propertyInfo6 = jsonPropertyInfoValues6;
		array[5] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo6);
		JsonPropertyInfoValues<List<SavedProperties.SavedProperty<SerializableCard[]>>> jsonPropertyInfoValues7 = new JsonPropertyInfoValues<List<SavedProperties.SavedProperty<SerializableCard[]>>>();
		jsonPropertyInfoValues7.IsProperty = false;
		jsonPropertyInfoValues7.IsPublic = true;
		jsonPropertyInfoValues7.IsVirtual = false;
		jsonPropertyInfoValues7.DeclaringType = typeof(SavedProperties);
		jsonPropertyInfoValues7.Converter = null;
		jsonPropertyInfoValues7.Getter = (object obj) => ((SavedProperties)obj).cardArrays;
		jsonPropertyInfoValues7.Setter = delegate(object obj, List<SavedProperties.SavedProperty<SerializableCard[]>>? value)
		{
			((SavedProperties)obj).cardArrays = value;
		};
		jsonPropertyInfoValues7.IgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
		jsonPropertyInfoValues7.HasJsonInclude = false;
		jsonPropertyInfoValues7.IsExtensionData = false;
		jsonPropertyInfoValues7.NumberHandling = null;
		jsonPropertyInfoValues7.PropertyName = "cardArrays";
		jsonPropertyInfoValues7.JsonPropertyName = "card_arrays";
		jsonPropertyInfoValues7.AttributeProviderFactory = () => typeof(SavedProperties).GetField("cardArrays", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<List<SavedProperties.SavedProperty<SerializableCard[]>>> propertyInfo7 = jsonPropertyInfoValues7;
		array[6] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo7);
		return array;
	}

	private JsonTypeInfo<SavedProperties.SavedProperty<bool>> Create_SavedPropertyBoolean(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SavedProperties.SavedProperty<bool>> jsonTypeInfo))
		{
			JsonObjectInfoValues<SavedProperties.SavedProperty<bool>> objectInfo = new JsonObjectInfoValues<SavedProperties.SavedProperty<bool>>
			{
				ObjectCreator = () => default(SavedProperties.SavedProperty<bool>),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SavedPropertyBooleanPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SavedProperties.SavedProperty<bool>).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SavedPropertyBooleanPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SavedProperties.SavedProperty<bool>);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SavedProperties.SavedProperty<bool>)obj).name;
		jsonPropertyInfoValues.Setter = delegate(object obj, string? value)
		{
			Unsafe.Unbox<SavedProperties.SavedProperty<bool>>(obj).name = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "name";
		jsonPropertyInfoValues.JsonPropertyName = null;
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SavedProperties.SavedProperty<bool>).GetField("name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		JsonPropertyInfoValues<bool> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues2.IsProperty = false;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SavedProperties.SavedProperty<bool>);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SavedProperties.SavedProperty<bool>)obj).value;
		jsonPropertyInfoValues2.Setter = delegate(object obj, bool value)
		{
			Unsafe.Unbox<SavedProperties.SavedProperty<bool>>(obj).value = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "value";
		jsonPropertyInfoValues2.JsonPropertyName = null;
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SavedProperties.SavedProperty<bool>).GetField("value", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<bool> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		return array;
	}

	private JsonTypeInfo<SavedProperties.SavedProperty<ModelId>> Create_SavedPropertyModelId(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SavedProperties.SavedProperty<ModelId>> jsonTypeInfo))
		{
			JsonObjectInfoValues<SavedProperties.SavedProperty<ModelId>> objectInfo = new JsonObjectInfoValues<SavedProperties.SavedProperty<ModelId>>
			{
				ObjectCreator = () => default(SavedProperties.SavedProperty<ModelId>),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SavedPropertyModelIdPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SavedProperties.SavedProperty<ModelId>).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SavedPropertyModelIdPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SavedProperties.SavedProperty<ModelId>);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SavedProperties.SavedProperty<ModelId>)obj).name;
		jsonPropertyInfoValues.Setter = delegate(object obj, string? value)
		{
			Unsafe.Unbox<SavedProperties.SavedProperty<ModelId>>(obj).name = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "name";
		jsonPropertyInfoValues.JsonPropertyName = null;
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SavedProperties.SavedProperty<ModelId>).GetField("name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		JsonPropertyInfoValues<ModelId> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues2.IsProperty = false;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SavedProperties.SavedProperty<ModelId>);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SavedProperties.SavedProperty<ModelId>)obj).value;
		jsonPropertyInfoValues2.Setter = delegate(object obj, ModelId? value)
		{
			Unsafe.Unbox<SavedProperties.SavedProperty<ModelId>>(obj).value = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "value";
		jsonPropertyInfoValues2.JsonPropertyName = null;
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SavedProperties.SavedProperty<ModelId>).GetField("value", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<ModelId> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		return array;
	}

	private JsonTypeInfo<SavedProperties.SavedProperty<SerializableCard[]>> Create_SavedPropertySerializableCardArray(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SavedProperties.SavedProperty<SerializableCard[]>> jsonTypeInfo))
		{
			JsonObjectInfoValues<SavedProperties.SavedProperty<SerializableCard[]>> objectInfo = new JsonObjectInfoValues<SavedProperties.SavedProperty<SerializableCard[]>>
			{
				ObjectCreator = () => default(SavedProperties.SavedProperty<SerializableCard[]>),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SavedPropertySerializableCardArrayPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SavedProperties.SavedProperty<SerializableCard[]>).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SavedPropertySerializableCardArrayPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SavedProperties.SavedProperty<SerializableCard[]>);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SavedProperties.SavedProperty<SerializableCard[]>)obj).name;
		jsonPropertyInfoValues.Setter = delegate(object obj, string? value)
		{
			Unsafe.Unbox<SavedProperties.SavedProperty<SerializableCard[]>>(obj).name = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "name";
		jsonPropertyInfoValues.JsonPropertyName = null;
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SavedProperties.SavedProperty<SerializableCard[]>).GetField("name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		JsonPropertyInfoValues<SerializableCard[]> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<SerializableCard[]>();
		jsonPropertyInfoValues2.IsProperty = false;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SavedProperties.SavedProperty<SerializableCard[]>);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SavedProperties.SavedProperty<SerializableCard[]>)obj).value;
		jsonPropertyInfoValues2.Setter = delegate(object obj, SerializableCard[]? value)
		{
			Unsafe.Unbox<SavedProperties.SavedProperty<SerializableCard[]>>(obj).value = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "value";
		jsonPropertyInfoValues2.JsonPropertyName = null;
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SavedProperties.SavedProperty<SerializableCard[]>).GetField("value", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<SerializableCard[]> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		return array;
	}

	private JsonTypeInfo<SavedProperties.SavedProperty<SerializableCard>> Create_SavedPropertySerializableCard(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SavedProperties.SavedProperty<SerializableCard>> jsonTypeInfo))
		{
			JsonObjectInfoValues<SavedProperties.SavedProperty<SerializableCard>> objectInfo = new JsonObjectInfoValues<SavedProperties.SavedProperty<SerializableCard>>
			{
				ObjectCreator = () => default(SavedProperties.SavedProperty<SerializableCard>),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SavedPropertySerializableCardPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SavedProperties.SavedProperty<SerializableCard>).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SavedPropertySerializableCardPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SavedProperties.SavedProperty<SerializableCard>);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SavedProperties.SavedProperty<SerializableCard>)obj).name;
		jsonPropertyInfoValues.Setter = delegate(object obj, string? value)
		{
			Unsafe.Unbox<SavedProperties.SavedProperty<SerializableCard>>(obj).name = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "name";
		jsonPropertyInfoValues.JsonPropertyName = null;
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SavedProperties.SavedProperty<SerializableCard>).GetField("name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		JsonPropertyInfoValues<SerializableCard> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<SerializableCard>();
		jsonPropertyInfoValues2.IsProperty = false;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SavedProperties.SavedProperty<SerializableCard>);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SavedProperties.SavedProperty<SerializableCard>)obj).value;
		jsonPropertyInfoValues2.Setter = delegate(object obj, SerializableCard? value)
		{
			Unsafe.Unbox<SavedProperties.SavedProperty<SerializableCard>>(obj).value = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "value";
		jsonPropertyInfoValues2.JsonPropertyName = null;
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SavedProperties.SavedProperty<SerializableCard>).GetField("value", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<SerializableCard> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		return array;
	}

	private JsonTypeInfo<SavedProperties.SavedProperty<int[]>> Create_SavedPropertyInt32Array(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SavedProperties.SavedProperty<int[]>> jsonTypeInfo))
		{
			JsonObjectInfoValues<SavedProperties.SavedProperty<int[]>> objectInfo = new JsonObjectInfoValues<SavedProperties.SavedProperty<int[]>>
			{
				ObjectCreator = () => default(SavedProperties.SavedProperty<int[]>),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SavedPropertyInt32ArrayPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SavedProperties.SavedProperty<int[]>).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SavedPropertyInt32ArrayPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SavedProperties.SavedProperty<int[]>);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SavedProperties.SavedProperty<int[]>)obj).name;
		jsonPropertyInfoValues.Setter = delegate(object obj, string? value)
		{
			Unsafe.Unbox<SavedProperties.SavedProperty<int[]>>(obj).name = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "name";
		jsonPropertyInfoValues.JsonPropertyName = null;
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SavedProperties.SavedProperty<int[]>).GetField("name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		JsonPropertyInfoValues<int[]> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int[]>();
		jsonPropertyInfoValues2.IsProperty = false;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SavedProperties.SavedProperty<int[]>);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SavedProperties.SavedProperty<int[]>)obj).value;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int[]? value)
		{
			Unsafe.Unbox<SavedProperties.SavedProperty<int[]>>(obj).value = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "value";
		jsonPropertyInfoValues2.JsonPropertyName = null;
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SavedProperties.SavedProperty<int[]>).GetField("value", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<int[]> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		return array;
	}

	private JsonTypeInfo<SavedProperties.SavedProperty<int>> Create_SavedPropertyInt32(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SavedProperties.SavedProperty<int>> jsonTypeInfo))
		{
			JsonObjectInfoValues<SavedProperties.SavedProperty<int>> objectInfo = new JsonObjectInfoValues<SavedProperties.SavedProperty<int>>
			{
				ObjectCreator = () => default(SavedProperties.SavedProperty<int>),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SavedPropertyInt32PropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SavedProperties.SavedProperty<int>).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SavedPropertyInt32PropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SavedProperties.SavedProperty<int>);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SavedProperties.SavedProperty<int>)obj).name;
		jsonPropertyInfoValues.Setter = delegate(object obj, string? value)
		{
			Unsafe.Unbox<SavedProperties.SavedProperty<int>>(obj).name = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "name";
		jsonPropertyInfoValues.JsonPropertyName = null;
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SavedProperties.SavedProperty<int>).GetField("name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		JsonPropertyInfoValues<int> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = false;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SavedProperties.SavedProperty<int>);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SavedProperties.SavedProperty<int>)obj).value;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			Unsafe.Unbox<SavedProperties.SavedProperty<int>>(obj).value = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "value";
		jsonPropertyInfoValues2.JsonPropertyName = null;
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SavedProperties.SavedProperty<int>).GetField("value", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<int> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		return array;
	}

	private JsonTypeInfo<SavedProperties.SavedProperty<string>> Create_SavedPropertyString(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SavedProperties.SavedProperty<string>> jsonTypeInfo))
		{
			JsonObjectInfoValues<SavedProperties.SavedProperty<string>> objectInfo = new JsonObjectInfoValues<SavedProperties.SavedProperty<string>>
			{
				ObjectCreator = () => default(SavedProperties.SavedProperty<string>),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SavedPropertyStringPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SavedProperties.SavedProperty<string>).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SavedPropertyStringPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SavedProperties.SavedProperty<string>);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SavedProperties.SavedProperty<string>)obj).name;
		jsonPropertyInfoValues.Setter = delegate(object obj, string? value)
		{
			Unsafe.Unbox<SavedProperties.SavedProperty<string>>(obj).name = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "name";
		jsonPropertyInfoValues.JsonPropertyName = null;
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SavedProperties.SavedProperty<string>).GetField("name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = false;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SavedProperties.SavedProperty<string>);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SavedProperties.SavedProperty<string>)obj).value;
		jsonPropertyInfoValues.Setter = delegate(object obj, string? value)
		{
			Unsafe.Unbox<SavedProperties.SavedProperty<string>>(obj).value = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "value";
		jsonPropertyInfoValues.JsonPropertyName = null;
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SavedProperties.SavedProperty<string>).GetField("value", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		JsonPropertyInfoValues<string> propertyInfo2 = jsonPropertyInfoValues;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		return array;
	}

	private JsonTypeInfo<SerializableActMap> Create_SerializableActMap(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SerializableActMap> jsonTypeInfo))
		{
			JsonObjectInfoValues<SerializableActMap> objectInfo = new JsonObjectInfoValues<SerializableActMap>
			{
				ObjectCreator = () => new SerializableActMap(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SerializableActMapPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SerializableActMap).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SerializableActMapPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[7];
		JsonPropertyInfoValues<List<SerializableMapPoint>> jsonPropertyInfoValues = new JsonPropertyInfoValues<List<SerializableMapPoint>>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableActMap);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableActMap)obj).Points;
		jsonPropertyInfoValues.Setter = delegate(object obj, List<SerializableMapPoint>? value)
		{
			((SerializableActMap)obj).Points = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Points";
		jsonPropertyInfoValues.JsonPropertyName = "points";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableActMap).GetProperty("Points", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<SerializableMapPoint>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<SerializableMapPoint>> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		JsonPropertyInfoValues<SerializableMapPoint> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<SerializableMapPoint>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SerializableActMap);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SerializableActMap)obj).BossPoint;
		jsonPropertyInfoValues2.Setter = delegate(object obj, SerializableMapPoint? value)
		{
			((SerializableActMap)obj).BossPoint = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "BossPoint";
		jsonPropertyInfoValues2.JsonPropertyName = "boss";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SerializableActMap).GetProperty("BossPoint", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(SerializableMapPoint), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<SerializableMapPoint> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<SerializableMapPoint>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SerializableActMap);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SerializableActMap)obj).SecondBossPoint;
		jsonPropertyInfoValues2.Setter = delegate(object obj, SerializableMapPoint? value)
		{
			((SerializableActMap)obj).SecondBossPoint = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "SecondBossPoint";
		jsonPropertyInfoValues2.JsonPropertyName = "second_boss";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SerializableActMap).GetProperty("SecondBossPoint", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(SerializableMapPoint), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<SerializableMapPoint> propertyInfo3 = jsonPropertyInfoValues2;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<SerializableMapPoint>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SerializableActMap);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SerializableActMap)obj).StartingPoint;
		jsonPropertyInfoValues2.Setter = delegate(object obj, SerializableMapPoint? value)
		{
			((SerializableActMap)obj).StartingPoint = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "StartingPoint";
		jsonPropertyInfoValues2.JsonPropertyName = "start";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SerializableActMap).GetProperty("StartingPoint", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(SerializableMapPoint), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<SerializableMapPoint> propertyInfo4 = jsonPropertyInfoValues2;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		array[3].IsGetNullable = false;
		array[3].IsSetNullable = false;
		JsonPropertyInfoValues<List<MapCoord>> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<List<MapCoord>>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(SerializableActMap);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((SerializableActMap)obj).StartMapPointCoords;
		jsonPropertyInfoValues3.Setter = delegate(object obj, List<MapCoord>? value)
		{
			((SerializableActMap)obj).StartMapPointCoords = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "StartMapPointCoords";
		jsonPropertyInfoValues3.JsonPropertyName = "start_coords";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(SerializableActMap).GetProperty("StartMapPointCoords", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<MapCoord>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<MapCoord>> propertyInfo5 = jsonPropertyInfoValues3;
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		JsonPropertyInfoValues<int> jsonPropertyInfoValues4 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues4.IsProperty = true;
		jsonPropertyInfoValues4.IsPublic = true;
		jsonPropertyInfoValues4.IsVirtual = false;
		jsonPropertyInfoValues4.DeclaringType = typeof(SerializableActMap);
		jsonPropertyInfoValues4.Converter = null;
		jsonPropertyInfoValues4.Getter = (object obj) => ((SerializableActMap)obj).GridWidth;
		jsonPropertyInfoValues4.Setter = delegate(object obj, int value)
		{
			((SerializableActMap)obj).GridWidth = value;
		};
		jsonPropertyInfoValues4.IgnoreCondition = null;
		jsonPropertyInfoValues4.HasJsonInclude = false;
		jsonPropertyInfoValues4.IsExtensionData = false;
		jsonPropertyInfoValues4.NumberHandling = null;
		jsonPropertyInfoValues4.PropertyName = "GridWidth";
		jsonPropertyInfoValues4.JsonPropertyName = "width";
		jsonPropertyInfoValues4.AttributeProviderFactory = () => typeof(SerializableActMap).GetProperty("GridWidth", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo6 = jsonPropertyInfoValues4;
		array[5] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo6);
		jsonPropertyInfoValues4 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues4.IsProperty = true;
		jsonPropertyInfoValues4.IsPublic = true;
		jsonPropertyInfoValues4.IsVirtual = false;
		jsonPropertyInfoValues4.DeclaringType = typeof(SerializableActMap);
		jsonPropertyInfoValues4.Converter = null;
		jsonPropertyInfoValues4.Getter = (object obj) => ((SerializableActMap)obj).GridHeight;
		jsonPropertyInfoValues4.Setter = delegate(object obj, int value)
		{
			((SerializableActMap)obj).GridHeight = value;
		};
		jsonPropertyInfoValues4.IgnoreCondition = null;
		jsonPropertyInfoValues4.HasJsonInclude = false;
		jsonPropertyInfoValues4.IsExtensionData = false;
		jsonPropertyInfoValues4.NumberHandling = null;
		jsonPropertyInfoValues4.PropertyName = "GridHeight";
		jsonPropertyInfoValues4.JsonPropertyName = "height";
		jsonPropertyInfoValues4.AttributeProviderFactory = () => typeof(SerializableActMap).GetProperty("GridHeight", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo7 = jsonPropertyInfoValues4;
		array[6] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo7);
		return array;
	}

	private JsonTypeInfo<SerializableActModel> Create_SerializableActModel(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SerializableActModel> jsonTypeInfo))
		{
			JsonObjectInfoValues<SerializableActModel> objectInfo = new JsonObjectInfoValues<SerializableActModel>
			{
				ObjectCreator = () => new SerializableActModel(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SerializableActModelPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SerializableActModel).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SerializableActModelPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[3];
		JsonPropertyInfoValues<ModelId> jsonPropertyInfoValues = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableActModel);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableActModel)obj).Id;
		jsonPropertyInfoValues.Setter = delegate(object obj, ModelId? value)
		{
			((SerializableActModel)obj).Id = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Id";
		jsonPropertyInfoValues.JsonPropertyName = "id";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableActModel).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModelId), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModelId> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<SerializableRoomSet> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<SerializableRoomSet>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SerializableActModel);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SerializableActModel)obj).SerializableRooms;
		jsonPropertyInfoValues2.Setter = delegate(object obj, SerializableRoomSet? value)
		{
			((SerializableActModel)obj).SerializableRooms = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "SerializableRooms";
		jsonPropertyInfoValues2.JsonPropertyName = "rooms";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SerializableActModel).GetProperty("SerializableRooms", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(SerializableRoomSet), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<SerializableRoomSet> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		JsonPropertyInfoValues<SerializableActMap> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<SerializableActMap>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(SerializableActModel);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((SerializableActModel)obj).SavedMap;
		jsonPropertyInfoValues3.Setter = delegate(object obj, SerializableActMap? value)
		{
			((SerializableActModel)obj).SavedMap = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "SavedMap";
		jsonPropertyInfoValues3.JsonPropertyName = "saved_map";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(SerializableActModel).GetProperty("SavedMap", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(SerializableActMap), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<SerializableActMap> propertyInfo3 = jsonPropertyInfoValues3;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		return array;
	}

	private JsonTypeInfo<SerializableCard> Create_SerializableCard(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SerializableCard> jsonTypeInfo))
		{
			JsonObjectInfoValues<SerializableCard> objectInfo = new JsonObjectInfoValues<SerializableCard>
			{
				ObjectCreator = () => new SerializableCard(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SerializableCardPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SerializableCard).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SerializableCardPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[5];
		JsonPropertyInfoValues<ModelId> jsonPropertyInfoValues = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableCard);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableCard)obj).Id;
		jsonPropertyInfoValues.Setter = delegate(object obj, ModelId? value)
		{
			((SerializableCard)obj).Id = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Id";
		jsonPropertyInfoValues.JsonPropertyName = "id";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableCard).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModelId), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModelId> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<int> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SerializableCard);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SerializableCard)obj).CurrentUpgradeLevel;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((SerializableCard)obj).CurrentUpgradeLevel = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "CurrentUpgradeLevel";
		jsonPropertyInfoValues2.JsonPropertyName = "current_upgrade_level";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SerializableCard).GetProperty("CurrentUpgradeLevel", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<SerializableEnchantment> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<SerializableEnchantment>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(SerializableCard);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((SerializableCard)obj).Enchantment;
		jsonPropertyInfoValues3.Setter = delegate(object obj, SerializableEnchantment? value)
		{
			((SerializableCard)obj).Enchantment = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "Enchantment";
		jsonPropertyInfoValues3.JsonPropertyName = "enchantment";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(SerializableCard).GetProperty("Enchantment", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(SerializableEnchantment), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<SerializableEnchantment> propertyInfo3 = jsonPropertyInfoValues3;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		JsonPropertyInfoValues<SavedProperties> jsonPropertyInfoValues4 = new JsonPropertyInfoValues<SavedProperties>();
		jsonPropertyInfoValues4.IsProperty = true;
		jsonPropertyInfoValues4.IsPublic = true;
		jsonPropertyInfoValues4.IsVirtual = false;
		jsonPropertyInfoValues4.DeclaringType = typeof(SerializableCard);
		jsonPropertyInfoValues4.Converter = null;
		jsonPropertyInfoValues4.Getter = (object obj) => ((SerializableCard)obj).Props;
		jsonPropertyInfoValues4.Setter = delegate(object obj, SavedProperties? value)
		{
			((SerializableCard)obj).Props = value;
		};
		jsonPropertyInfoValues4.IgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
		jsonPropertyInfoValues4.HasJsonInclude = false;
		jsonPropertyInfoValues4.IsExtensionData = false;
		jsonPropertyInfoValues4.NumberHandling = null;
		jsonPropertyInfoValues4.PropertyName = "Props";
		jsonPropertyInfoValues4.JsonPropertyName = "props";
		jsonPropertyInfoValues4.AttributeProviderFactory = () => typeof(SerializableCard).GetProperty("Props", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(SavedProperties), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<SavedProperties> propertyInfo4 = jsonPropertyInfoValues4;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		JsonPropertyInfoValues<int?> jsonPropertyInfoValues5 = new JsonPropertyInfoValues<int?>();
		jsonPropertyInfoValues5.IsProperty = true;
		jsonPropertyInfoValues5.IsPublic = true;
		jsonPropertyInfoValues5.IsVirtual = false;
		jsonPropertyInfoValues5.DeclaringType = typeof(SerializableCard);
		jsonPropertyInfoValues5.Converter = null;
		jsonPropertyInfoValues5.Getter = (object obj) => ((SerializableCard)obj).FloorAddedToDeck;
		jsonPropertyInfoValues5.Setter = delegate(object obj, int? value)
		{
			((SerializableCard)obj).FloorAddedToDeck = value;
		};
		jsonPropertyInfoValues5.IgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
		jsonPropertyInfoValues5.HasJsonInclude = false;
		jsonPropertyInfoValues5.IsExtensionData = false;
		jsonPropertyInfoValues5.NumberHandling = null;
		jsonPropertyInfoValues5.PropertyName = "FloorAddedToDeck";
		jsonPropertyInfoValues5.JsonPropertyName = "floor_added_to_deck";
		jsonPropertyInfoValues5.AttributeProviderFactory = () => typeof(SerializableCard).GetProperty("FloorAddedToDeck", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int?), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int?> propertyInfo5 = jsonPropertyInfoValues5;
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		return array;
	}

	private JsonTypeInfo<SerializableCard[]> Create_SerializableCardArray(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SerializableCard[]> jsonTypeInfo))
		{
			JsonCollectionInfoValues<SerializableCard[]> collectionInfo = new JsonCollectionInfoValues<SerializableCard[]>
			{
				ObjectCreator = null,
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateArrayInfo(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<SerializableEnchantment> Create_SerializableEnchantment(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SerializableEnchantment> jsonTypeInfo))
		{
			JsonObjectInfoValues<SerializableEnchantment> objectInfo = new JsonObjectInfoValues<SerializableEnchantment>
			{
				ObjectCreator = () => new SerializableEnchantment(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SerializableEnchantmentPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SerializableEnchantment).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SerializableEnchantmentPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[3];
		JsonPropertyInfoValues<ModelId> jsonPropertyInfoValues = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableEnchantment);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableEnchantment)obj).Id;
		jsonPropertyInfoValues.Setter = delegate(object obj, ModelId? value)
		{
			((SerializableEnchantment)obj).Id = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Id";
		jsonPropertyInfoValues.JsonPropertyName = "id";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableEnchantment).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModelId), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModelId> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<int> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SerializableEnchantment);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SerializableEnchantment)obj).Amount;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((SerializableEnchantment)obj).Amount = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "Amount";
		jsonPropertyInfoValues2.JsonPropertyName = "amount";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SerializableEnchantment).GetProperty("Amount", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<SavedProperties> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<SavedProperties>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(SerializableEnchantment);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((SerializableEnchantment)obj).Props;
		jsonPropertyInfoValues3.Setter = delegate(object obj, SavedProperties? value)
		{
			((SerializableEnchantment)obj).Props = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "Props";
		jsonPropertyInfoValues3.JsonPropertyName = "props";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(SerializableEnchantment).GetProperty("Props", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(SavedProperties), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<SavedProperties> propertyInfo3 = jsonPropertyInfoValues3;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		return array;
	}

	private JsonTypeInfo<SerializableExtraRunFields> Create_SerializableExtraRunFields(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SerializableExtraRunFields> jsonTypeInfo))
		{
			JsonObjectInfoValues<SerializableExtraRunFields> objectInfo = new JsonObjectInfoValues<SerializableExtraRunFields>
			{
				ObjectCreator = () => new SerializableExtraRunFields(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SerializableExtraRunFieldsPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SerializableExtraRunFields).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SerializableExtraRunFieldsPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[3];
		JsonPropertyInfoValues<bool> jsonPropertyInfoValues = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableExtraRunFields);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableExtraRunFields)obj).StartedWithNeow;
		jsonPropertyInfoValues.Setter = delegate(object obj, bool value)
		{
			((SerializableExtraRunFields)obj).StartedWithNeow = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "StartedWithNeow";
		jsonPropertyInfoValues.JsonPropertyName = "started_with_neow";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableExtraRunFields).GetProperty("StartedWithNeow", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<bool> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<int> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SerializableExtraRunFields);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SerializableExtraRunFields)obj).TestSubjectKills;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((SerializableExtraRunFields)obj).TestSubjectKills = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "TestSubjectKills";
		jsonPropertyInfoValues2.JsonPropertyName = "test_subject_kills";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SerializableExtraRunFields).GetProperty("TestSubjectKills", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		jsonPropertyInfoValues = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableExtraRunFields);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableExtraRunFields)obj).FreedRepy;
		jsonPropertyInfoValues.Setter = delegate(object obj, bool value)
		{
			((SerializableExtraRunFields)obj).FreedRepy = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "FreedRepy";
		jsonPropertyInfoValues.JsonPropertyName = "freed_repy";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableExtraRunFields).GetProperty("FreedRepy", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<bool> propertyInfo3 = jsonPropertyInfoValues;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		return array;
	}

	private JsonTypeInfo<SerializableMapPoint> Create_SerializableMapPoint(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SerializableMapPoint> jsonTypeInfo))
		{
			JsonObjectInfoValues<SerializableMapPoint> objectInfo = new JsonObjectInfoValues<SerializableMapPoint>
			{
				ObjectCreator = () => new SerializableMapPoint(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SerializableMapPointPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SerializableMapPoint).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SerializableMapPointPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[4];
		JsonPropertyInfoValues<MapCoord> jsonPropertyInfoValues = new JsonPropertyInfoValues<MapCoord>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableMapPoint);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableMapPoint)obj).Coord;
		jsonPropertyInfoValues.Setter = delegate(object obj, MapCoord value)
		{
			((SerializableMapPoint)obj).Coord = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Coord";
		jsonPropertyInfoValues.JsonPropertyName = "coord";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableMapPoint).GetProperty("Coord", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(MapCoord), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<MapCoord> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<MapPointType> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<MapPointType>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SerializableMapPoint);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SerializableMapPoint)obj).PointType;
		jsonPropertyInfoValues2.Setter = delegate(object obj, MapPointType value)
		{
			((SerializableMapPoint)obj).PointType = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "PointType";
		jsonPropertyInfoValues2.JsonPropertyName = "type";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SerializableMapPoint).GetProperty("PointType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(MapPointType), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<MapPointType> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<bool> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(SerializableMapPoint);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((SerializableMapPoint)obj).CanBeModified;
		jsonPropertyInfoValues3.Setter = delegate(object obj, bool value)
		{
			((SerializableMapPoint)obj).CanBeModified = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "CanBeModified";
		jsonPropertyInfoValues3.JsonPropertyName = "can_modify";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(SerializableMapPoint).GetProperty("CanBeModified", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<bool> propertyInfo3 = jsonPropertyInfoValues3;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		JsonPropertyInfoValues<List<MapCoord>> jsonPropertyInfoValues4 = new JsonPropertyInfoValues<List<MapCoord>>();
		jsonPropertyInfoValues4.IsProperty = true;
		jsonPropertyInfoValues4.IsPublic = true;
		jsonPropertyInfoValues4.IsVirtual = false;
		jsonPropertyInfoValues4.DeclaringType = typeof(SerializableMapPoint);
		jsonPropertyInfoValues4.Converter = null;
		jsonPropertyInfoValues4.Getter = (object obj) => ((SerializableMapPoint)obj).ChildCoords;
		jsonPropertyInfoValues4.Setter = delegate(object obj, List<MapCoord>? value)
		{
			((SerializableMapPoint)obj).ChildCoords = value;
		};
		jsonPropertyInfoValues4.IgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
		jsonPropertyInfoValues4.HasJsonInclude = false;
		jsonPropertyInfoValues4.IsExtensionData = false;
		jsonPropertyInfoValues4.NumberHandling = null;
		jsonPropertyInfoValues4.PropertyName = "ChildCoords";
		jsonPropertyInfoValues4.JsonPropertyName = "children";
		jsonPropertyInfoValues4.AttributeProviderFactory = () => typeof(SerializableMapPoint).GetProperty("ChildCoords", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<MapCoord>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<MapCoord>> propertyInfo4 = jsonPropertyInfoValues4;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		return array;
	}

	private JsonTypeInfo<SerializableModifier> Create_SerializableModifier(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SerializableModifier> jsonTypeInfo))
		{
			JsonObjectInfoValues<SerializableModifier> objectInfo = new JsonObjectInfoValues<SerializableModifier>
			{
				ObjectCreator = () => new SerializableModifier(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SerializableModifierPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SerializableModifier).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SerializableModifierPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<ModelId> jsonPropertyInfoValues = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableModifier);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableModifier)obj).Id;
		jsonPropertyInfoValues.Setter = delegate(object obj, ModelId? value)
		{
			((SerializableModifier)obj).Id = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Id";
		jsonPropertyInfoValues.JsonPropertyName = "id";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableModifier).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModelId), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModelId> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<SavedProperties> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<SavedProperties>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SerializableModifier);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SerializableModifier)obj).Props;
		jsonPropertyInfoValues2.Setter = delegate(object obj, SavedProperties? value)
		{
			((SerializableModifier)obj).Props = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "Props";
		jsonPropertyInfoValues2.JsonPropertyName = "props";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SerializableModifier).GetProperty("Props", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(SavedProperties), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<SavedProperties> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		return array;
	}

	private JsonTypeInfo<SerializablePlayer> Create_SerializablePlayer(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SerializablePlayer> jsonTypeInfo))
		{
			JsonObjectInfoValues<SerializablePlayer> objectInfo = new JsonObjectInfoValues<SerializablePlayer>
			{
				ObjectCreator = () => new SerializablePlayer(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SerializablePlayerPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SerializablePlayer).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SerializablePlayerPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[21];
		JsonPropertyInfoValues<ModelId> jsonPropertyInfoValues = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializablePlayer);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializablePlayer)obj).CharacterId;
		jsonPropertyInfoValues.Setter = delegate(object obj, ModelId? value)
		{
			((SerializablePlayer)obj).CharacterId = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "CharacterId";
		jsonPropertyInfoValues.JsonPropertyName = "character_id";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializablePlayer).GetProperty("CharacterId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModelId), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModelId> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<int> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SerializablePlayer);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SerializablePlayer)obj).CurrentHp;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((SerializablePlayer)obj).CurrentHp = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "CurrentHp";
		jsonPropertyInfoValues2.JsonPropertyName = "current_hp";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SerializablePlayer).GetProperty("CurrentHp", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SerializablePlayer);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SerializablePlayer)obj).MaxHp;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((SerializablePlayer)obj).MaxHp = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "MaxHp";
		jsonPropertyInfoValues2.JsonPropertyName = "max_hp";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SerializablePlayer).GetProperty("MaxHp", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo3 = jsonPropertyInfoValues2;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SerializablePlayer);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SerializablePlayer)obj).MaxEnergy;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((SerializablePlayer)obj).MaxEnergy = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "MaxEnergy";
		jsonPropertyInfoValues2.JsonPropertyName = "max_energy";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SerializablePlayer).GetProperty("MaxEnergy", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo4 = jsonPropertyInfoValues2;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SerializablePlayer);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SerializablePlayer)obj).MaxPotionSlotCount;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((SerializablePlayer)obj).MaxPotionSlotCount = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "MaxPotionSlotCount";
		jsonPropertyInfoValues2.JsonPropertyName = "max_potion_slot_count";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SerializablePlayer).GetProperty("MaxPotionSlotCount", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo5 = jsonPropertyInfoValues2;
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SerializablePlayer);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SerializablePlayer)obj).Gold;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((SerializablePlayer)obj).Gold = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "Gold";
		jsonPropertyInfoValues2.JsonPropertyName = "gold";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SerializablePlayer).GetProperty("Gold", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo6 = jsonPropertyInfoValues2;
		array[5] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo6);
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SerializablePlayer);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SerializablePlayer)obj).BaseOrbSlotCount;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((SerializablePlayer)obj).BaseOrbSlotCount = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "BaseOrbSlotCount";
		jsonPropertyInfoValues2.JsonPropertyName = "base_orb_slot_count";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SerializablePlayer).GetProperty("BaseOrbSlotCount", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo7 = jsonPropertyInfoValues2;
		array[6] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo7);
		JsonPropertyInfoValues<ulong> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<ulong>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(SerializablePlayer);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((SerializablePlayer)obj).NetId;
		jsonPropertyInfoValues3.Setter = delegate(object obj, ulong value)
		{
			((SerializablePlayer)obj).NetId = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "NetId";
		jsonPropertyInfoValues3.JsonPropertyName = "net_id";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(SerializablePlayer).GetProperty("NetId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ulong), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ulong> propertyInfo8 = jsonPropertyInfoValues3;
		array[7] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo8);
		JsonPropertyInfoValues<List<SerializableCard>> jsonPropertyInfoValues4 = new JsonPropertyInfoValues<List<SerializableCard>>();
		jsonPropertyInfoValues4.IsProperty = true;
		jsonPropertyInfoValues4.IsPublic = true;
		jsonPropertyInfoValues4.IsVirtual = false;
		jsonPropertyInfoValues4.DeclaringType = typeof(SerializablePlayer);
		jsonPropertyInfoValues4.Converter = null;
		jsonPropertyInfoValues4.Getter = (object obj) => ((SerializablePlayer)obj).Deck;
		jsonPropertyInfoValues4.Setter = delegate(object obj, List<SerializableCard>? value)
		{
			((SerializablePlayer)obj).Deck = value;
		};
		jsonPropertyInfoValues4.IgnoreCondition = null;
		jsonPropertyInfoValues4.HasJsonInclude = false;
		jsonPropertyInfoValues4.IsExtensionData = false;
		jsonPropertyInfoValues4.NumberHandling = null;
		jsonPropertyInfoValues4.PropertyName = "Deck";
		jsonPropertyInfoValues4.JsonPropertyName = "deck";
		jsonPropertyInfoValues4.AttributeProviderFactory = () => typeof(SerializablePlayer).GetProperty("Deck", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<SerializableCard>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<SerializableCard>> propertyInfo9 = jsonPropertyInfoValues4;
		array[8] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo9);
		array[8].IsGetNullable = false;
		array[8].IsSetNullable = false;
		JsonPropertyInfoValues<List<SerializableRelic>> jsonPropertyInfoValues5 = new JsonPropertyInfoValues<List<SerializableRelic>>();
		jsonPropertyInfoValues5.IsProperty = true;
		jsonPropertyInfoValues5.IsPublic = true;
		jsonPropertyInfoValues5.IsVirtual = false;
		jsonPropertyInfoValues5.DeclaringType = typeof(SerializablePlayer);
		jsonPropertyInfoValues5.Converter = null;
		jsonPropertyInfoValues5.Getter = (object obj) => ((SerializablePlayer)obj).Relics;
		jsonPropertyInfoValues5.Setter = delegate(object obj, List<SerializableRelic>? value)
		{
			((SerializablePlayer)obj).Relics = value;
		};
		jsonPropertyInfoValues5.IgnoreCondition = null;
		jsonPropertyInfoValues5.HasJsonInclude = false;
		jsonPropertyInfoValues5.IsExtensionData = false;
		jsonPropertyInfoValues5.NumberHandling = null;
		jsonPropertyInfoValues5.PropertyName = "Relics";
		jsonPropertyInfoValues5.JsonPropertyName = "relics";
		jsonPropertyInfoValues5.AttributeProviderFactory = () => typeof(SerializablePlayer).GetProperty("Relics", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<SerializableRelic>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<SerializableRelic>> propertyInfo10 = jsonPropertyInfoValues5;
		array[9] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo10);
		array[9].IsGetNullable = false;
		array[9].IsSetNullable = false;
		JsonPropertyInfoValues<List<SerializablePotion>> jsonPropertyInfoValues6 = new JsonPropertyInfoValues<List<SerializablePotion>>();
		jsonPropertyInfoValues6.IsProperty = true;
		jsonPropertyInfoValues6.IsPublic = true;
		jsonPropertyInfoValues6.IsVirtual = false;
		jsonPropertyInfoValues6.DeclaringType = typeof(SerializablePlayer);
		jsonPropertyInfoValues6.Converter = null;
		jsonPropertyInfoValues6.Getter = (object obj) => ((SerializablePlayer)obj).Potions;
		jsonPropertyInfoValues6.Setter = delegate(object obj, List<SerializablePotion>? value)
		{
			((SerializablePlayer)obj).Potions = value;
		};
		jsonPropertyInfoValues6.IgnoreCondition = null;
		jsonPropertyInfoValues6.HasJsonInclude = false;
		jsonPropertyInfoValues6.IsExtensionData = false;
		jsonPropertyInfoValues6.NumberHandling = null;
		jsonPropertyInfoValues6.PropertyName = "Potions";
		jsonPropertyInfoValues6.JsonPropertyName = "potions";
		jsonPropertyInfoValues6.AttributeProviderFactory = () => typeof(SerializablePlayer).GetProperty("Potions", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<SerializablePotion>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<SerializablePotion>> propertyInfo11 = jsonPropertyInfoValues6;
		array[10] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo11);
		array[10].IsGetNullable = false;
		array[10].IsSetNullable = false;
		JsonPropertyInfoValues<SerializablePlayerRngSet> jsonPropertyInfoValues7 = new JsonPropertyInfoValues<SerializablePlayerRngSet>();
		jsonPropertyInfoValues7.IsProperty = true;
		jsonPropertyInfoValues7.IsPublic = true;
		jsonPropertyInfoValues7.IsVirtual = false;
		jsonPropertyInfoValues7.DeclaringType = typeof(SerializablePlayer);
		jsonPropertyInfoValues7.Converter = null;
		jsonPropertyInfoValues7.Getter = (object obj) => ((SerializablePlayer)obj).Rng;
		jsonPropertyInfoValues7.Setter = delegate(object obj, SerializablePlayerRngSet? value)
		{
			((SerializablePlayer)obj).Rng = value;
		};
		jsonPropertyInfoValues7.IgnoreCondition = null;
		jsonPropertyInfoValues7.HasJsonInclude = false;
		jsonPropertyInfoValues7.IsExtensionData = false;
		jsonPropertyInfoValues7.NumberHandling = null;
		jsonPropertyInfoValues7.PropertyName = "Rng";
		jsonPropertyInfoValues7.JsonPropertyName = "rng";
		jsonPropertyInfoValues7.AttributeProviderFactory = () => typeof(SerializablePlayer).GetProperty("Rng", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(SerializablePlayerRngSet), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<SerializablePlayerRngSet> propertyInfo12 = jsonPropertyInfoValues7;
		array[11] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo12);
		array[11].IsGetNullable = false;
		array[11].IsSetNullable = false;
		JsonPropertyInfoValues<SerializablePlayerOddsSet> jsonPropertyInfoValues8 = new JsonPropertyInfoValues<SerializablePlayerOddsSet>();
		jsonPropertyInfoValues8.IsProperty = true;
		jsonPropertyInfoValues8.IsPublic = true;
		jsonPropertyInfoValues8.IsVirtual = false;
		jsonPropertyInfoValues8.DeclaringType = typeof(SerializablePlayer);
		jsonPropertyInfoValues8.Converter = null;
		jsonPropertyInfoValues8.Getter = (object obj) => ((SerializablePlayer)obj).Odds;
		jsonPropertyInfoValues8.Setter = delegate(object obj, SerializablePlayerOddsSet? value)
		{
			((SerializablePlayer)obj).Odds = value;
		};
		jsonPropertyInfoValues8.IgnoreCondition = null;
		jsonPropertyInfoValues8.HasJsonInclude = false;
		jsonPropertyInfoValues8.IsExtensionData = false;
		jsonPropertyInfoValues8.NumberHandling = null;
		jsonPropertyInfoValues8.PropertyName = "Odds";
		jsonPropertyInfoValues8.JsonPropertyName = "odds";
		jsonPropertyInfoValues8.AttributeProviderFactory = () => typeof(SerializablePlayer).GetProperty("Odds", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(SerializablePlayerOddsSet), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<SerializablePlayerOddsSet> propertyInfo13 = jsonPropertyInfoValues8;
		array[12] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo13);
		array[12].IsGetNullable = false;
		array[12].IsSetNullable = false;
		JsonPropertyInfoValues<SerializableRelicGrabBag> jsonPropertyInfoValues9 = new JsonPropertyInfoValues<SerializableRelicGrabBag>();
		jsonPropertyInfoValues9.IsProperty = true;
		jsonPropertyInfoValues9.IsPublic = true;
		jsonPropertyInfoValues9.IsVirtual = false;
		jsonPropertyInfoValues9.DeclaringType = typeof(SerializablePlayer);
		jsonPropertyInfoValues9.Converter = null;
		jsonPropertyInfoValues9.Getter = (object obj) => ((SerializablePlayer)obj).RelicGrabBag;
		jsonPropertyInfoValues9.Setter = delegate(object obj, SerializableRelicGrabBag? value)
		{
			((SerializablePlayer)obj).RelicGrabBag = value;
		};
		jsonPropertyInfoValues9.IgnoreCondition = null;
		jsonPropertyInfoValues9.HasJsonInclude = false;
		jsonPropertyInfoValues9.IsExtensionData = false;
		jsonPropertyInfoValues9.NumberHandling = null;
		jsonPropertyInfoValues9.PropertyName = "RelicGrabBag";
		jsonPropertyInfoValues9.JsonPropertyName = "relic_grab_bag";
		jsonPropertyInfoValues9.AttributeProviderFactory = () => typeof(SerializablePlayer).GetProperty("RelicGrabBag", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(SerializableRelicGrabBag), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<SerializableRelicGrabBag> propertyInfo14 = jsonPropertyInfoValues9;
		array[13] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo14);
		array[13].IsGetNullable = false;
		array[13].IsSetNullable = false;
		JsonPropertyInfoValues<SerializableExtraPlayerFields> jsonPropertyInfoValues10 = new JsonPropertyInfoValues<SerializableExtraPlayerFields>();
		jsonPropertyInfoValues10.IsProperty = true;
		jsonPropertyInfoValues10.IsPublic = true;
		jsonPropertyInfoValues10.IsVirtual = false;
		jsonPropertyInfoValues10.DeclaringType = typeof(SerializablePlayer);
		jsonPropertyInfoValues10.Converter = null;
		jsonPropertyInfoValues10.Getter = (object obj) => ((SerializablePlayer)obj).ExtraFields;
		jsonPropertyInfoValues10.Setter = delegate(object obj, SerializableExtraPlayerFields? value)
		{
			((SerializablePlayer)obj).ExtraFields = value;
		};
		jsonPropertyInfoValues10.IgnoreCondition = null;
		jsonPropertyInfoValues10.HasJsonInclude = false;
		jsonPropertyInfoValues10.IsExtensionData = false;
		jsonPropertyInfoValues10.NumberHandling = null;
		jsonPropertyInfoValues10.PropertyName = "ExtraFields";
		jsonPropertyInfoValues10.JsonPropertyName = "extra_fields";
		jsonPropertyInfoValues10.AttributeProviderFactory = () => typeof(SerializablePlayer).GetProperty("ExtraFields", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(SerializableExtraPlayerFields), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<SerializableExtraPlayerFields> propertyInfo15 = jsonPropertyInfoValues10;
		array[14] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo15);
		array[14].IsGetNullable = false;
		array[14].IsSetNullable = false;
		JsonPropertyInfoValues<SerializableUnlockState> jsonPropertyInfoValues11 = new JsonPropertyInfoValues<SerializableUnlockState>();
		jsonPropertyInfoValues11.IsProperty = true;
		jsonPropertyInfoValues11.IsPublic = true;
		jsonPropertyInfoValues11.IsVirtual = false;
		jsonPropertyInfoValues11.DeclaringType = typeof(SerializablePlayer);
		jsonPropertyInfoValues11.Converter = null;
		jsonPropertyInfoValues11.Getter = (object obj) => ((SerializablePlayer)obj).UnlockState;
		jsonPropertyInfoValues11.Setter = delegate(object obj, SerializableUnlockState? value)
		{
			((SerializablePlayer)obj).UnlockState = value;
		};
		jsonPropertyInfoValues11.IgnoreCondition = null;
		jsonPropertyInfoValues11.HasJsonInclude = false;
		jsonPropertyInfoValues11.IsExtensionData = false;
		jsonPropertyInfoValues11.NumberHandling = null;
		jsonPropertyInfoValues11.PropertyName = "UnlockState";
		jsonPropertyInfoValues11.JsonPropertyName = "unlock_state";
		jsonPropertyInfoValues11.AttributeProviderFactory = () => typeof(SerializablePlayer).GetProperty("UnlockState", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(SerializableUnlockState), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<SerializableUnlockState> propertyInfo16 = jsonPropertyInfoValues11;
		array[15] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo16);
		array[15].IsGetNullable = false;
		array[15].IsSetNullable = false;
		JsonPropertyInfoValues<List<ModelId>> jsonPropertyInfoValues12 = new JsonPropertyInfoValues<List<ModelId>>();
		jsonPropertyInfoValues12.IsProperty = true;
		jsonPropertyInfoValues12.IsPublic = true;
		jsonPropertyInfoValues12.IsVirtual = false;
		jsonPropertyInfoValues12.DeclaringType = typeof(SerializablePlayer);
		jsonPropertyInfoValues12.Converter = null;
		jsonPropertyInfoValues12.Getter = (object obj) => ((SerializablePlayer)obj).DiscoveredCards;
		jsonPropertyInfoValues12.Setter = delegate(object obj, List<ModelId>? value)
		{
			((SerializablePlayer)obj).DiscoveredCards = value;
		};
		jsonPropertyInfoValues12.IgnoreCondition = null;
		jsonPropertyInfoValues12.HasJsonInclude = false;
		jsonPropertyInfoValues12.IsExtensionData = false;
		jsonPropertyInfoValues12.NumberHandling = null;
		jsonPropertyInfoValues12.PropertyName = "DiscoveredCards";
		jsonPropertyInfoValues12.JsonPropertyName = "discovered_cards";
		jsonPropertyInfoValues12.AttributeProviderFactory = () => typeof(SerializablePlayer).GetProperty("DiscoveredCards", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<ModelId>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<ModelId>> propertyInfo17 = jsonPropertyInfoValues12;
		array[16] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo17);
		array[16].IsGetNullable = false;
		array[16].IsSetNullable = false;
		jsonPropertyInfoValues12 = new JsonPropertyInfoValues<List<ModelId>>();
		jsonPropertyInfoValues12.IsProperty = true;
		jsonPropertyInfoValues12.IsPublic = true;
		jsonPropertyInfoValues12.IsVirtual = false;
		jsonPropertyInfoValues12.DeclaringType = typeof(SerializablePlayer);
		jsonPropertyInfoValues12.Converter = null;
		jsonPropertyInfoValues12.Getter = (object obj) => ((SerializablePlayer)obj).DiscoveredEnemies;
		jsonPropertyInfoValues12.Setter = delegate(object obj, List<ModelId>? value)
		{
			((SerializablePlayer)obj).DiscoveredEnemies = value;
		};
		jsonPropertyInfoValues12.IgnoreCondition = null;
		jsonPropertyInfoValues12.HasJsonInclude = false;
		jsonPropertyInfoValues12.IsExtensionData = false;
		jsonPropertyInfoValues12.NumberHandling = null;
		jsonPropertyInfoValues12.PropertyName = "DiscoveredEnemies";
		jsonPropertyInfoValues12.JsonPropertyName = "discovered_enemies";
		jsonPropertyInfoValues12.AttributeProviderFactory = () => typeof(SerializablePlayer).GetProperty("DiscoveredEnemies", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<ModelId>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<ModelId>> propertyInfo18 = jsonPropertyInfoValues12;
		array[17] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo18);
		array[17].IsGetNullable = false;
		array[17].IsSetNullable = false;
		JsonPropertyInfoValues<List<string>> jsonPropertyInfoValues13 = new JsonPropertyInfoValues<List<string>>();
		jsonPropertyInfoValues13.IsProperty = true;
		jsonPropertyInfoValues13.IsPublic = true;
		jsonPropertyInfoValues13.IsVirtual = false;
		jsonPropertyInfoValues13.DeclaringType = typeof(SerializablePlayer);
		jsonPropertyInfoValues13.Converter = (JsonConverter<List<string>>)ExpandConverter(typeof(List<string>), new EpochIdListConverter(), options);
		jsonPropertyInfoValues13.Getter = (object obj) => ((SerializablePlayer)obj).DiscoveredEpochs;
		jsonPropertyInfoValues13.Setter = delegate(object obj, List<string>? value)
		{
			((SerializablePlayer)obj).DiscoveredEpochs = value;
		};
		jsonPropertyInfoValues13.IgnoreCondition = null;
		jsonPropertyInfoValues13.HasJsonInclude = false;
		jsonPropertyInfoValues13.IsExtensionData = false;
		jsonPropertyInfoValues13.NumberHandling = null;
		jsonPropertyInfoValues13.PropertyName = "DiscoveredEpochs";
		jsonPropertyInfoValues13.JsonPropertyName = "discovered_epochs";
		jsonPropertyInfoValues13.AttributeProviderFactory = () => typeof(SerializablePlayer).GetProperty("DiscoveredEpochs", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<string>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<string>> propertyInfo19 = jsonPropertyInfoValues13;
		array[18] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo19);
		array[18].IsGetNullable = false;
		array[18].IsSetNullable = false;
		jsonPropertyInfoValues12 = new JsonPropertyInfoValues<List<ModelId>>();
		jsonPropertyInfoValues12.IsProperty = true;
		jsonPropertyInfoValues12.IsPublic = true;
		jsonPropertyInfoValues12.IsVirtual = false;
		jsonPropertyInfoValues12.DeclaringType = typeof(SerializablePlayer);
		jsonPropertyInfoValues12.Converter = null;
		jsonPropertyInfoValues12.Getter = (object obj) => ((SerializablePlayer)obj).DiscoveredPotions;
		jsonPropertyInfoValues12.Setter = delegate(object obj, List<ModelId>? value)
		{
			((SerializablePlayer)obj).DiscoveredPotions = value;
		};
		jsonPropertyInfoValues12.IgnoreCondition = null;
		jsonPropertyInfoValues12.HasJsonInclude = false;
		jsonPropertyInfoValues12.IsExtensionData = false;
		jsonPropertyInfoValues12.NumberHandling = null;
		jsonPropertyInfoValues12.PropertyName = "DiscoveredPotions";
		jsonPropertyInfoValues12.JsonPropertyName = "discovered_potions";
		jsonPropertyInfoValues12.AttributeProviderFactory = () => typeof(SerializablePlayer).GetProperty("DiscoveredPotions", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<ModelId>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<ModelId>> propertyInfo20 = jsonPropertyInfoValues12;
		array[19] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo20);
		array[19].IsGetNullable = false;
		array[19].IsSetNullable = false;
		jsonPropertyInfoValues12 = new JsonPropertyInfoValues<List<ModelId>>();
		jsonPropertyInfoValues12.IsProperty = true;
		jsonPropertyInfoValues12.IsPublic = true;
		jsonPropertyInfoValues12.IsVirtual = false;
		jsonPropertyInfoValues12.DeclaringType = typeof(SerializablePlayer);
		jsonPropertyInfoValues12.Converter = null;
		jsonPropertyInfoValues12.Getter = (object obj) => ((SerializablePlayer)obj).DiscoveredRelics;
		jsonPropertyInfoValues12.Setter = delegate(object obj, List<ModelId>? value)
		{
			((SerializablePlayer)obj).DiscoveredRelics = value;
		};
		jsonPropertyInfoValues12.IgnoreCondition = null;
		jsonPropertyInfoValues12.HasJsonInclude = false;
		jsonPropertyInfoValues12.IsExtensionData = false;
		jsonPropertyInfoValues12.NumberHandling = null;
		jsonPropertyInfoValues12.PropertyName = "DiscoveredRelics";
		jsonPropertyInfoValues12.JsonPropertyName = "discovered_relics";
		jsonPropertyInfoValues12.AttributeProviderFactory = () => typeof(SerializablePlayer).GetProperty("DiscoveredRelics", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<ModelId>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<ModelId>> propertyInfo21 = jsonPropertyInfoValues12;
		array[20] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo21);
		array[20].IsGetNullable = false;
		array[20].IsSetNullable = false;
		return array;
	}

	private JsonTypeInfo<SerializablePlayerOddsSet> Create_SerializablePlayerOddsSet(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SerializablePlayerOddsSet> jsonTypeInfo))
		{
			JsonObjectInfoValues<SerializablePlayerOddsSet> objectInfo = new JsonObjectInfoValues<SerializablePlayerOddsSet>
			{
				ObjectCreator = () => new SerializablePlayerOddsSet(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SerializablePlayerOddsSetPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SerializablePlayerOddsSet).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SerializablePlayerOddsSetPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<float> jsonPropertyInfoValues = new JsonPropertyInfoValues<float>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializablePlayerOddsSet);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializablePlayerOddsSet)obj).CardRarityOddsValue;
		jsonPropertyInfoValues.Setter = delegate(object obj, float value)
		{
			((SerializablePlayerOddsSet)obj).CardRarityOddsValue = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "CardRarityOddsValue";
		jsonPropertyInfoValues.JsonPropertyName = "card_rarity_odds_value";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializablePlayerOddsSet).GetProperty("CardRarityOddsValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(float), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<float> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		jsonPropertyInfoValues = new JsonPropertyInfoValues<float>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializablePlayerOddsSet);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializablePlayerOddsSet)obj).PotionRewardOddsValue;
		jsonPropertyInfoValues.Setter = delegate(object obj, float value)
		{
			((SerializablePlayerOddsSet)obj).PotionRewardOddsValue = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "PotionRewardOddsValue";
		jsonPropertyInfoValues.JsonPropertyName = "potion_reward_odds_value";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializablePlayerOddsSet).GetProperty("PotionRewardOddsValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(float), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<float> propertyInfo2 = jsonPropertyInfoValues;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		return array;
	}

	private JsonTypeInfo<SerializablePotion> Create_SerializablePotion(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SerializablePotion> jsonTypeInfo))
		{
			JsonObjectInfoValues<SerializablePotion> objectInfo = new JsonObjectInfoValues<SerializablePotion>
			{
				ObjectCreator = () => new SerializablePotion(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SerializablePotionPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SerializablePotion).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SerializablePotionPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<ModelId> jsonPropertyInfoValues = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializablePotion);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializablePotion)obj).Id;
		jsonPropertyInfoValues.Setter = delegate(object obj, ModelId? value)
		{
			((SerializablePotion)obj).Id = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Id";
		jsonPropertyInfoValues.JsonPropertyName = "id";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializablePotion).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModelId), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModelId> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<int> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SerializablePotion);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SerializablePotion)obj).SlotIndex;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((SerializablePotion)obj).SlotIndex = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "SlotIndex";
		jsonPropertyInfoValues2.JsonPropertyName = "slot_index";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SerializablePotion).GetProperty("SlotIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		return array;
	}

	private JsonTypeInfo<SerializableRelic> Create_SerializableRelic(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SerializableRelic> jsonTypeInfo))
		{
			JsonObjectInfoValues<SerializableRelic> objectInfo = new JsonObjectInfoValues<SerializableRelic>
			{
				ObjectCreator = () => new SerializableRelic(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SerializableRelicPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SerializableRelic).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SerializableRelicPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[3];
		JsonPropertyInfoValues<ModelId> jsonPropertyInfoValues = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableRelic);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableRelic)obj).Id;
		jsonPropertyInfoValues.Setter = delegate(object obj, ModelId? value)
		{
			((SerializableRelic)obj).Id = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Id";
		jsonPropertyInfoValues.JsonPropertyName = "id";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableRelic).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModelId), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModelId> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<SavedProperties> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<SavedProperties>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SerializableRelic);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SerializableRelic)obj).Props;
		jsonPropertyInfoValues2.Setter = delegate(object obj, SavedProperties? value)
		{
			((SerializableRelic)obj).Props = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "Props";
		jsonPropertyInfoValues2.JsonPropertyName = "props";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SerializableRelic).GetProperty("Props", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(SavedProperties), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<SavedProperties> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<int?> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<int?>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(SerializableRelic);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((SerializableRelic)obj).FloorAddedToDeck;
		jsonPropertyInfoValues3.Setter = delegate(object obj, int? value)
		{
			((SerializableRelic)obj).FloorAddedToDeck = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "FloorAddedToDeck";
		jsonPropertyInfoValues3.JsonPropertyName = "floor_added_to_deck";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(SerializableRelic).GetProperty("FloorAddedToDeck", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int?), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int?> propertyInfo3 = jsonPropertyInfoValues3;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		return array;
	}

	private JsonTypeInfo<SerializableRelicGrabBag> Create_SerializableRelicGrabBag(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SerializableRelicGrabBag> jsonTypeInfo))
		{
			JsonObjectInfoValues<SerializableRelicGrabBag> objectInfo = new JsonObjectInfoValues<SerializableRelicGrabBag>
			{
				ObjectCreator = () => new SerializableRelicGrabBag(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SerializableRelicGrabBagPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SerializableRelicGrabBag).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SerializableRelicGrabBagPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[1];
		JsonPropertyInfoValues<Dictionary<RelicRarity, List<ModelId>>> jsonPropertyInfoValues = new JsonPropertyInfoValues<Dictionary<RelicRarity, List<ModelId>>>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableRelicGrabBag);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableRelicGrabBag)obj).RelicIdLists;
		jsonPropertyInfoValues.Setter = delegate(object obj, Dictionary<RelicRarity, List<ModelId>>? value)
		{
			((SerializableRelicGrabBag)obj).RelicIdLists = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "RelicIdLists";
		jsonPropertyInfoValues.JsonPropertyName = "relic_id_lists";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableRelicGrabBag).GetProperty("RelicIdLists", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(Dictionary<RelicRarity, List<ModelId>>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<Dictionary<RelicRarity, List<ModelId>>> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		return array;
	}

	private JsonTypeInfo<SerializableReward> Create_SerializableReward(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SerializableReward> jsonTypeInfo))
		{
			JsonObjectInfoValues<SerializableReward> objectInfo = new JsonObjectInfoValues<SerializableReward>
			{
				ObjectCreator = () => new SerializableReward(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SerializableRewardPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SerializableReward).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SerializableRewardPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[9];
		JsonPropertyInfoValues<RewardType> jsonPropertyInfoValues = new JsonPropertyInfoValues<RewardType>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableReward);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableReward)obj).RewardType;
		jsonPropertyInfoValues.Setter = delegate(object obj, RewardType value)
		{
			((SerializableReward)obj).RewardType = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "RewardType";
		jsonPropertyInfoValues.JsonPropertyName = "reward_type";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableReward).GetProperty("RewardType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(RewardType), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<RewardType> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<SerializableCard> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<SerializableCard>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SerializableReward);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SerializableReward)obj).SpecialCard;
		jsonPropertyInfoValues2.Setter = delegate(object obj, SerializableCard? value)
		{
			((SerializableReward)obj).SpecialCard = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "SpecialCard";
		jsonPropertyInfoValues2.JsonPropertyName = "special_card";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SerializableReward).GetProperty("SpecialCard", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(SerializableCard), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<SerializableCard> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		JsonPropertyInfoValues<int> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(SerializableReward);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((SerializableReward)obj).GoldAmount;
		jsonPropertyInfoValues3.Setter = delegate(object obj, int value)
		{
			((SerializableReward)obj).GoldAmount = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "GoldAmount";
		jsonPropertyInfoValues3.JsonPropertyName = "gold_amount";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(SerializableReward).GetProperty("GoldAmount", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo3 = jsonPropertyInfoValues3;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		JsonPropertyInfoValues<bool> jsonPropertyInfoValues4 = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues4.IsProperty = true;
		jsonPropertyInfoValues4.IsPublic = true;
		jsonPropertyInfoValues4.IsVirtual = false;
		jsonPropertyInfoValues4.DeclaringType = typeof(SerializableReward);
		jsonPropertyInfoValues4.Converter = null;
		jsonPropertyInfoValues4.Getter = (object obj) => ((SerializableReward)obj).WasGoldStolenBack;
		jsonPropertyInfoValues4.Setter = delegate(object obj, bool value)
		{
			((SerializableReward)obj).WasGoldStolenBack = value;
		};
		jsonPropertyInfoValues4.IgnoreCondition = null;
		jsonPropertyInfoValues4.HasJsonInclude = false;
		jsonPropertyInfoValues4.IsExtensionData = false;
		jsonPropertyInfoValues4.NumberHandling = null;
		jsonPropertyInfoValues4.PropertyName = "WasGoldStolenBack";
		jsonPropertyInfoValues4.JsonPropertyName = "was_gold_stolen_back";
		jsonPropertyInfoValues4.AttributeProviderFactory = () => typeof(SerializableReward).GetProperty("WasGoldStolenBack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<bool> propertyInfo4 = jsonPropertyInfoValues4;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		JsonPropertyInfoValues<CardCreationSource> jsonPropertyInfoValues5 = new JsonPropertyInfoValues<CardCreationSource>();
		jsonPropertyInfoValues5.IsProperty = true;
		jsonPropertyInfoValues5.IsPublic = true;
		jsonPropertyInfoValues5.IsVirtual = false;
		jsonPropertyInfoValues5.DeclaringType = typeof(SerializableReward);
		jsonPropertyInfoValues5.Converter = null;
		jsonPropertyInfoValues5.Getter = (object obj) => ((SerializableReward)obj).Source;
		jsonPropertyInfoValues5.Setter = delegate(object obj, CardCreationSource value)
		{
			((SerializableReward)obj).Source = value;
		};
		jsonPropertyInfoValues5.IgnoreCondition = null;
		jsonPropertyInfoValues5.HasJsonInclude = false;
		jsonPropertyInfoValues5.IsExtensionData = false;
		jsonPropertyInfoValues5.NumberHandling = null;
		jsonPropertyInfoValues5.PropertyName = "Source";
		jsonPropertyInfoValues5.JsonPropertyName = "source";
		jsonPropertyInfoValues5.AttributeProviderFactory = () => typeof(SerializableReward).GetProperty("Source", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(CardCreationSource), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<CardCreationSource> propertyInfo5 = jsonPropertyInfoValues5;
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		JsonPropertyInfoValues<CardRarityOddsType> jsonPropertyInfoValues6 = new JsonPropertyInfoValues<CardRarityOddsType>();
		jsonPropertyInfoValues6.IsProperty = true;
		jsonPropertyInfoValues6.IsPublic = true;
		jsonPropertyInfoValues6.IsVirtual = false;
		jsonPropertyInfoValues6.DeclaringType = typeof(SerializableReward);
		jsonPropertyInfoValues6.Converter = null;
		jsonPropertyInfoValues6.Getter = (object obj) => ((SerializableReward)obj).RarityOdds;
		jsonPropertyInfoValues6.Setter = delegate(object obj, CardRarityOddsType value)
		{
			((SerializableReward)obj).RarityOdds = value;
		};
		jsonPropertyInfoValues6.IgnoreCondition = null;
		jsonPropertyInfoValues6.HasJsonInclude = false;
		jsonPropertyInfoValues6.IsExtensionData = false;
		jsonPropertyInfoValues6.NumberHandling = null;
		jsonPropertyInfoValues6.PropertyName = "RarityOdds";
		jsonPropertyInfoValues6.JsonPropertyName = "rarity_odds";
		jsonPropertyInfoValues6.AttributeProviderFactory = () => typeof(SerializableReward).GetProperty("RarityOdds", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(CardRarityOddsType), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<CardRarityOddsType> propertyInfo6 = jsonPropertyInfoValues6;
		array[5] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo6);
		JsonPropertyInfoValues<List<ModelId>> jsonPropertyInfoValues7 = new JsonPropertyInfoValues<List<ModelId>>();
		jsonPropertyInfoValues7.IsProperty = true;
		jsonPropertyInfoValues7.IsPublic = true;
		jsonPropertyInfoValues7.IsVirtual = false;
		jsonPropertyInfoValues7.DeclaringType = typeof(SerializableReward);
		jsonPropertyInfoValues7.Converter = null;
		jsonPropertyInfoValues7.Getter = (object obj) => ((SerializableReward)obj).CardPoolIds;
		jsonPropertyInfoValues7.Setter = delegate(object obj, List<ModelId>? value)
		{
			((SerializableReward)obj).CardPoolIds = value;
		};
		jsonPropertyInfoValues7.IgnoreCondition = null;
		jsonPropertyInfoValues7.HasJsonInclude = false;
		jsonPropertyInfoValues7.IsExtensionData = false;
		jsonPropertyInfoValues7.NumberHandling = null;
		jsonPropertyInfoValues7.PropertyName = "CardPoolIds";
		jsonPropertyInfoValues7.JsonPropertyName = "card_pools";
		jsonPropertyInfoValues7.AttributeProviderFactory = () => typeof(SerializableReward).GetProperty("CardPoolIds", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<ModelId>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<ModelId>> propertyInfo7 = jsonPropertyInfoValues7;
		array[6] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo7);
		array[6].IsGetNullable = false;
		array[6].IsSetNullable = false;
		jsonPropertyInfoValues3 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(SerializableReward);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((SerializableReward)obj).OptionCount;
		jsonPropertyInfoValues3.Setter = delegate(object obj, int value)
		{
			((SerializableReward)obj).OptionCount = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "OptionCount";
		jsonPropertyInfoValues3.JsonPropertyName = "option_count";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(SerializableReward).GetProperty("OptionCount", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo8 = jsonPropertyInfoValues3;
		array[7] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo8);
		JsonPropertyInfoValues<ModelId> jsonPropertyInfoValues8 = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues8.IsProperty = true;
		jsonPropertyInfoValues8.IsPublic = true;
		jsonPropertyInfoValues8.IsVirtual = false;
		jsonPropertyInfoValues8.DeclaringType = typeof(SerializableReward);
		jsonPropertyInfoValues8.Converter = null;
		jsonPropertyInfoValues8.Getter = (object obj) => ((SerializableReward)obj).CustomDescriptionEncounterSourceId;
		jsonPropertyInfoValues8.Setter = delegate(object obj, ModelId? value)
		{
			((SerializableReward)obj).CustomDescriptionEncounterSourceId = value;
		};
		jsonPropertyInfoValues8.IgnoreCondition = null;
		jsonPropertyInfoValues8.HasJsonInclude = false;
		jsonPropertyInfoValues8.IsExtensionData = false;
		jsonPropertyInfoValues8.NumberHandling = null;
		jsonPropertyInfoValues8.PropertyName = "CustomDescriptionEncounterSourceId";
		jsonPropertyInfoValues8.JsonPropertyName = "custom_description_encounter_source_id";
		jsonPropertyInfoValues8.AttributeProviderFactory = () => typeof(SerializableReward).GetProperty("CustomDescriptionEncounterSourceId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModelId), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModelId> propertyInfo9 = jsonPropertyInfoValues8;
		array[8] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo9);
		array[8].IsGetNullable = false;
		array[8].IsSetNullable = false;
		return array;
	}

	private JsonTypeInfo<SerializableRoom> Create_SerializableRoom(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SerializableRoom> jsonTypeInfo))
		{
			JsonObjectInfoValues<SerializableRoom> objectInfo = new JsonObjectInfoValues<SerializableRoom>
			{
				ObjectCreator = () => new SerializableRoom(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SerializableRoomPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SerializableRoom).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SerializableRoomPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[9];
		JsonPropertyInfoValues<RoomType> jsonPropertyInfoValues = new JsonPropertyInfoValues<RoomType>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableRoom);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableRoom)obj).RoomType;
		jsonPropertyInfoValues.Setter = delegate(object obj, RoomType value)
		{
			((SerializableRoom)obj).RoomType = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "RoomType";
		jsonPropertyInfoValues.JsonPropertyName = "room_type";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableRoom).GetProperty("RoomType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(RoomType), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<RoomType> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<ModelId> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SerializableRoom);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SerializableRoom)obj).EncounterId;
		jsonPropertyInfoValues2.Setter = delegate(object obj, ModelId? value)
		{
			((SerializableRoom)obj).EncounterId = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "EncounterId";
		jsonPropertyInfoValues2.JsonPropertyName = "encounter_id";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SerializableRoom).GetProperty("EncounterId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModelId), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModelId> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SerializableRoom);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SerializableRoom)obj).EventId;
		jsonPropertyInfoValues2.Setter = delegate(object obj, ModelId? value)
		{
			((SerializableRoom)obj).EventId = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "EventId";
		jsonPropertyInfoValues2.JsonPropertyName = "event_id";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SerializableRoom).GetProperty("EventId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModelId), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModelId> propertyInfo3 = jsonPropertyInfoValues2;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		JsonPropertyInfoValues<bool> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(SerializableRoom);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((SerializableRoom)obj).IsPreFinished;
		jsonPropertyInfoValues3.Setter = delegate(object obj, bool value)
		{
			((SerializableRoom)obj).IsPreFinished = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "IsPreFinished";
		jsonPropertyInfoValues3.JsonPropertyName = "is_pre_finished";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(SerializableRoom).GetProperty("IsPreFinished", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<bool> propertyInfo4 = jsonPropertyInfoValues3;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		JsonPropertyInfoValues<float> jsonPropertyInfoValues4 = new JsonPropertyInfoValues<float>();
		jsonPropertyInfoValues4.IsProperty = true;
		jsonPropertyInfoValues4.IsPublic = true;
		jsonPropertyInfoValues4.IsVirtual = false;
		jsonPropertyInfoValues4.DeclaringType = typeof(SerializableRoom);
		jsonPropertyInfoValues4.Converter = null;
		jsonPropertyInfoValues4.Getter = (object obj) => ((SerializableRoom)obj).GoldProportion;
		jsonPropertyInfoValues4.Setter = delegate(object obj, float value)
		{
			((SerializableRoom)obj).GoldProportion = value;
		};
		jsonPropertyInfoValues4.IgnoreCondition = null;
		jsonPropertyInfoValues4.HasJsonInclude = false;
		jsonPropertyInfoValues4.IsExtensionData = false;
		jsonPropertyInfoValues4.NumberHandling = null;
		jsonPropertyInfoValues4.PropertyName = "GoldProportion";
		jsonPropertyInfoValues4.JsonPropertyName = "reward_proportion";
		jsonPropertyInfoValues4.AttributeProviderFactory = () => typeof(SerializableRoom).GetProperty("GoldProportion", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(float), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<float> propertyInfo5 = jsonPropertyInfoValues4;
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		JsonPropertyInfoValues<Dictionary<ulong, List<SerializableReward>>> jsonPropertyInfoValues5 = new JsonPropertyInfoValues<Dictionary<ulong, List<SerializableReward>>>();
		jsonPropertyInfoValues5.IsProperty = true;
		jsonPropertyInfoValues5.IsPublic = true;
		jsonPropertyInfoValues5.IsVirtual = false;
		jsonPropertyInfoValues5.DeclaringType = typeof(SerializableRoom);
		jsonPropertyInfoValues5.Converter = null;
		jsonPropertyInfoValues5.Getter = (object obj) => ((SerializableRoom)obj).ExtraRewards;
		jsonPropertyInfoValues5.Setter = delegate(object obj, Dictionary<ulong, List<SerializableReward>>? value)
		{
			((SerializableRoom)obj).ExtraRewards = value;
		};
		jsonPropertyInfoValues5.IgnoreCondition = null;
		jsonPropertyInfoValues5.HasJsonInclude = false;
		jsonPropertyInfoValues5.IsExtensionData = false;
		jsonPropertyInfoValues5.NumberHandling = null;
		jsonPropertyInfoValues5.PropertyName = "ExtraRewards";
		jsonPropertyInfoValues5.JsonPropertyName = "extra_rewards";
		jsonPropertyInfoValues5.AttributeProviderFactory = () => typeof(SerializableRoom).GetProperty("ExtraRewards", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(Dictionary<ulong, List<SerializableReward>>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<Dictionary<ulong, List<SerializableReward>>> propertyInfo6 = jsonPropertyInfoValues5;
		array[5] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo6);
		array[5].IsGetNullable = false;
		array[5].IsSetNullable = false;
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SerializableRoom);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SerializableRoom)obj).ParentEventId;
		jsonPropertyInfoValues2.Setter = delegate(object obj, ModelId? value)
		{
			((SerializableRoom)obj).ParentEventId = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "ParentEventId";
		jsonPropertyInfoValues2.JsonPropertyName = "parent_event_id";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SerializableRoom).GetProperty("ParentEventId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModelId), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModelId> propertyInfo7 = jsonPropertyInfoValues2;
		array[6] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo7);
		jsonPropertyInfoValues3 = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(SerializableRoom);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((SerializableRoom)obj).ShouldResumeParentEvent;
		jsonPropertyInfoValues3.Setter = delegate(object obj, bool value)
		{
			((SerializableRoom)obj).ShouldResumeParentEvent = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "ShouldResumeParentEvent";
		jsonPropertyInfoValues3.JsonPropertyName = "should_resume_parent_event";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(SerializableRoom).GetProperty("ShouldResumeParentEvent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<bool> propertyInfo8 = jsonPropertyInfoValues3;
		array[7] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo8);
		JsonPropertyInfoValues<Dictionary<string, string>> jsonPropertyInfoValues6 = new JsonPropertyInfoValues<Dictionary<string, string>>();
		jsonPropertyInfoValues6.IsProperty = true;
		jsonPropertyInfoValues6.IsPublic = true;
		jsonPropertyInfoValues6.IsVirtual = false;
		jsonPropertyInfoValues6.DeclaringType = typeof(SerializableRoom);
		jsonPropertyInfoValues6.Converter = null;
		jsonPropertyInfoValues6.Getter = (object obj) => ((SerializableRoom)obj).EncounterState;
		jsonPropertyInfoValues6.Setter = delegate(object obj, Dictionary<string, string>? value)
		{
			((SerializableRoom)obj).EncounterState = value;
		};
		jsonPropertyInfoValues6.IgnoreCondition = null;
		jsonPropertyInfoValues6.HasJsonInclude = false;
		jsonPropertyInfoValues6.IsExtensionData = false;
		jsonPropertyInfoValues6.NumberHandling = null;
		jsonPropertyInfoValues6.PropertyName = "EncounterState";
		jsonPropertyInfoValues6.JsonPropertyName = "encounter_state";
		jsonPropertyInfoValues6.AttributeProviderFactory = () => typeof(SerializableRoom).GetProperty("EncounterState", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(Dictionary<string, string>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<Dictionary<string, string>> propertyInfo9 = jsonPropertyInfoValues6;
		array[8] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo9);
		array[8].IsGetNullable = false;
		array[8].IsSetNullable = false;
		return array;
	}

	private JsonTypeInfo<SerializableRoomSet> Create_SerializableRoomSet(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SerializableRoomSet> jsonTypeInfo))
		{
			JsonObjectInfoValues<SerializableRoomSet> objectInfo = new JsonObjectInfoValues<SerializableRoomSet>
			{
				ObjectCreator = () => new SerializableRoomSet(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SerializableRoomSetPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SerializableRoomSet).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SerializableRoomSetPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[10];
		JsonPropertyInfoValues<List<ModelId>> jsonPropertyInfoValues = new JsonPropertyInfoValues<List<ModelId>>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableRoomSet);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableRoomSet)obj).EventIds;
		jsonPropertyInfoValues.Setter = delegate(object obj, List<ModelId>? value)
		{
			((SerializableRoomSet)obj).EventIds = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "EventIds";
		jsonPropertyInfoValues.JsonPropertyName = "event_ids";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableRoomSet).GetProperty("EventIds", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<ModelId>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<ModelId>> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		JsonPropertyInfoValues<int> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SerializableRoomSet);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SerializableRoomSet)obj).EventsVisited;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((SerializableRoomSet)obj).EventsVisited = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "EventsVisited";
		jsonPropertyInfoValues2.JsonPropertyName = "events_visited";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SerializableRoomSet).GetProperty("EventsVisited", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		jsonPropertyInfoValues = new JsonPropertyInfoValues<List<ModelId>>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableRoomSet);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableRoomSet)obj).NormalEncounterIds;
		jsonPropertyInfoValues.Setter = delegate(object obj, List<ModelId>? value)
		{
			((SerializableRoomSet)obj).NormalEncounterIds = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "NormalEncounterIds";
		jsonPropertyInfoValues.JsonPropertyName = "normal_encounter_ids";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableRoomSet).GetProperty("NormalEncounterIds", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<ModelId>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<ModelId>> propertyInfo3 = jsonPropertyInfoValues;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		array[2].IsGetNullable = false;
		array[2].IsSetNullable = false;
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SerializableRoomSet);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SerializableRoomSet)obj).NormalEncountersVisited;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((SerializableRoomSet)obj).NormalEncountersVisited = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "NormalEncountersVisited";
		jsonPropertyInfoValues2.JsonPropertyName = "normal_encounters_visited";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SerializableRoomSet).GetProperty("NormalEncountersVisited", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo4 = jsonPropertyInfoValues2;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		jsonPropertyInfoValues = new JsonPropertyInfoValues<List<ModelId>>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableRoomSet);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableRoomSet)obj).EliteEncounterIds;
		jsonPropertyInfoValues.Setter = delegate(object obj, List<ModelId>? value)
		{
			((SerializableRoomSet)obj).EliteEncounterIds = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "EliteEncounterIds";
		jsonPropertyInfoValues.JsonPropertyName = "elite_encounter_ids";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableRoomSet).GetProperty("EliteEncounterIds", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<ModelId>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<ModelId>> propertyInfo5 = jsonPropertyInfoValues;
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		array[4].IsGetNullable = false;
		array[4].IsSetNullable = false;
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SerializableRoomSet);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SerializableRoomSet)obj).EliteEncountersVisited;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((SerializableRoomSet)obj).EliteEncountersVisited = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "EliteEncountersVisited";
		jsonPropertyInfoValues2.JsonPropertyName = "elite_encounters_visited";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SerializableRoomSet).GetProperty("EliteEncountersVisited", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo6 = jsonPropertyInfoValues2;
		array[5] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo6);
		jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SerializableRoomSet);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SerializableRoomSet)obj).BossEncountersVisited;
		jsonPropertyInfoValues2.Setter = delegate(object obj, int value)
		{
			((SerializableRoomSet)obj).BossEncountersVisited = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "BossEncountersVisited";
		jsonPropertyInfoValues2.JsonPropertyName = "boss_encounters_visited";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SerializableRoomSet).GetProperty("BossEncountersVisited", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo7 = jsonPropertyInfoValues2;
		array[6] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo7);
		JsonPropertyInfoValues<ModelId> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(SerializableRoomSet);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((SerializableRoomSet)obj).BossId;
		jsonPropertyInfoValues3.Setter = delegate(object obj, ModelId? value)
		{
			((SerializableRoomSet)obj).BossId = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "BossId";
		jsonPropertyInfoValues3.JsonPropertyName = "boss_id";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(SerializableRoomSet).GetProperty("BossId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModelId), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModelId> propertyInfo8 = jsonPropertyInfoValues3;
		array[7] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo8);
		jsonPropertyInfoValues3 = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(SerializableRoomSet);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((SerializableRoomSet)obj).SecondBossId;
		jsonPropertyInfoValues3.Setter = delegate(object obj, ModelId? value)
		{
			((SerializableRoomSet)obj).SecondBossId = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "SecondBossId";
		jsonPropertyInfoValues3.JsonPropertyName = "second_boss_id";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(SerializableRoomSet).GetProperty("SecondBossId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModelId), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModelId> propertyInfo9 = jsonPropertyInfoValues3;
		array[8] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo9);
		jsonPropertyInfoValues3 = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(SerializableRoomSet);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((SerializableRoomSet)obj).AncientId;
		jsonPropertyInfoValues3.Setter = delegate(object obj, ModelId? value)
		{
			((SerializableRoomSet)obj).AncientId = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "AncientId";
		jsonPropertyInfoValues3.JsonPropertyName = "ancient_id";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(SerializableRoomSet).GetProperty("AncientId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModelId), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModelId> propertyInfo10 = jsonPropertyInfoValues3;
		array[9] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo10);
		return array;
	}

	private JsonTypeInfo<SerializableRunOddsSet> Create_SerializableRunOddsSet(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SerializableRunOddsSet> jsonTypeInfo))
		{
			JsonObjectInfoValues<SerializableRunOddsSet> objectInfo = new JsonObjectInfoValues<SerializableRunOddsSet>
			{
				ObjectCreator = () => new SerializableRunOddsSet(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SerializableRunOddsSetPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SerializableRunOddsSet).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SerializableRunOddsSetPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[4];
		JsonPropertyInfoValues<float> jsonPropertyInfoValues = new JsonPropertyInfoValues<float>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableRunOddsSet);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableRunOddsSet)obj).UnknownMapPointMonsterOddsValue;
		jsonPropertyInfoValues.Setter = delegate(object obj, float value)
		{
			((SerializableRunOddsSet)obj).UnknownMapPointMonsterOddsValue = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "UnknownMapPointMonsterOddsValue";
		jsonPropertyInfoValues.JsonPropertyName = "unknown_map_point_monster_odds_value";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableRunOddsSet).GetProperty("UnknownMapPointMonsterOddsValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(float), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<float> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		jsonPropertyInfoValues = new JsonPropertyInfoValues<float>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableRunOddsSet);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableRunOddsSet)obj).UnknownMapPointEliteOddsValue;
		jsonPropertyInfoValues.Setter = delegate(object obj, float value)
		{
			((SerializableRunOddsSet)obj).UnknownMapPointEliteOddsValue = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "UnknownMapPointEliteOddsValue";
		jsonPropertyInfoValues.JsonPropertyName = "unknown_map_point_elite_odds_value";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableRunOddsSet).GetProperty("UnknownMapPointEliteOddsValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(float), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<float> propertyInfo2 = jsonPropertyInfoValues;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		jsonPropertyInfoValues = new JsonPropertyInfoValues<float>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableRunOddsSet);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableRunOddsSet)obj).UnknownMapPointTreasureOddsValue;
		jsonPropertyInfoValues.Setter = delegate(object obj, float value)
		{
			((SerializableRunOddsSet)obj).UnknownMapPointTreasureOddsValue = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "UnknownMapPointTreasureOddsValue";
		jsonPropertyInfoValues.JsonPropertyName = "unknown_map_point_treasure_odds_value";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableRunOddsSet).GetProperty("UnknownMapPointTreasureOddsValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(float), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<float> propertyInfo3 = jsonPropertyInfoValues;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		jsonPropertyInfoValues = new JsonPropertyInfoValues<float>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableRunOddsSet);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableRunOddsSet)obj).UnknownMapPointShopOddsValue;
		jsonPropertyInfoValues.Setter = delegate(object obj, float value)
		{
			((SerializableRunOddsSet)obj).UnknownMapPointShopOddsValue = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "UnknownMapPointShopOddsValue";
		jsonPropertyInfoValues.JsonPropertyName = "unknown_map_point_shop_odds_value";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableRunOddsSet).GetProperty("UnknownMapPointShopOddsValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(float), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<float> propertyInfo4 = jsonPropertyInfoValues;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		return array;
	}

	private JsonTypeInfo<SerializableRunRngSet> Create_SerializableRunRngSet(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SerializableRunRngSet> jsonTypeInfo))
		{
			JsonObjectInfoValues<SerializableRunRngSet> objectInfo = new JsonObjectInfoValues<SerializableRunRngSet>
			{
				ObjectCreator = () => new SerializableRunRngSet(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SerializableRunRngSetPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SerializableRunRngSet).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SerializableRunRngSetPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableRunRngSet);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableRunRngSet)obj).Seed;
		jsonPropertyInfoValues.Setter = delegate(object obj, string? value)
		{
			((SerializableRunRngSet)obj).Seed = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Seed";
		jsonPropertyInfoValues.JsonPropertyName = "seed";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableRunRngSet).GetProperty("Seed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<Dictionary<RunRngType, int>> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<Dictionary<RunRngType, int>>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SerializableRunRngSet);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SerializableRunRngSet)obj).Counters;
		jsonPropertyInfoValues2.Setter = delegate(object obj, Dictionary<RunRngType, int>? value)
		{
			((SerializableRunRngSet)obj).Counters = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "Counters";
		jsonPropertyInfoValues2.JsonPropertyName = "counters";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SerializableRunRngSet).GetProperty("Counters", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(Dictionary<RunRngType, int>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<Dictionary<RunRngType, int>> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		return array;
	}

	private JsonTypeInfo<SerializableEpoch> Create_SerializableEpoch(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SerializableEpoch> jsonTypeInfo))
		{
			JsonObjectInfoValues<SerializableEpoch> jsonObjectInfoValues = new JsonObjectInfoValues<SerializableEpoch>();
			jsonObjectInfoValues.ObjectCreator = null;
			jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = (object[] args) => new SerializableEpoch((string)args[0], (EpochState)args[1]);
			jsonObjectInfoValues.PropertyMetadataInitializer = (JsonSerializerContext _) => SerializableEpochPropInit(options);
			jsonObjectInfoValues.ConstructorParameterMetadataInitializer = SerializableEpochCtorParamInit;
			jsonObjectInfoValues.ConstructorAttributeProviderFactory = () => typeof(SerializableEpoch).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[2]
			{
				typeof(string),
				typeof(EpochState)
			}, null);
			jsonObjectInfoValues.SerializeHandler = null;
			JsonObjectInfoValues<SerializableEpoch> objectInfo = jsonObjectInfoValues;
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SerializableEpochPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[3];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableEpoch);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableEpoch)obj).Id;
		jsonPropertyInfoValues.Setter = null;
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Id";
		jsonPropertyInfoValues.JsonPropertyName = "id";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableEpoch).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsGetNullable = false;
		JsonPropertyInfoValues<EpochState> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<EpochState>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SerializableEpoch);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SerializableEpoch)obj).State;
		jsonPropertyInfoValues2.Setter = delegate(object obj, EpochState value)
		{
			((SerializableEpoch)obj).State = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "State";
		jsonPropertyInfoValues2.JsonPropertyName = "state";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SerializableEpoch).GetProperty("State", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(EpochState), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<EpochState> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<long> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<long>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(SerializableEpoch);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((SerializableEpoch)obj).ObtainDate;
		jsonPropertyInfoValues3.Setter = delegate(object obj, long value)
		{
			((SerializableEpoch)obj).ObtainDate = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "ObtainDate";
		jsonPropertyInfoValues3.JsonPropertyName = "obtain_date";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(SerializableEpoch).GetProperty("ObtainDate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(long), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<long> propertyInfo3 = jsonPropertyInfoValues3;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		return array;
	}

	private static JsonParameterInfoValues[] SerializableEpochCtorParamInit()
	{
		return new JsonParameterInfoValues[2]
		{
			new JsonParameterInfoValues
			{
				Name = "id",
				ParameterType = typeof(string),
				Position = 0,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			},
			new JsonParameterInfoValues
			{
				Name = "state",
				ParameterType = typeof(EpochState),
				Position = 1,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			}
		};
	}

	private JsonTypeInfo<SerializableExtraPlayerFields> Create_SerializableExtraPlayerFields(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SerializableExtraPlayerFields> jsonTypeInfo))
		{
			JsonObjectInfoValues<SerializableExtraPlayerFields> objectInfo = new JsonObjectInfoValues<SerializableExtraPlayerFields>
			{
				ObjectCreator = () => new SerializableExtraPlayerFields(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SerializableExtraPlayerFieldsPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SerializableExtraPlayerFields).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SerializableExtraPlayerFieldsPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<int> jsonPropertyInfoValues = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableExtraPlayerFields);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableExtraPlayerFields)obj).CardShopRemovalsUsed;
		jsonPropertyInfoValues.Setter = delegate(object obj, int value)
		{
			((SerializableExtraPlayerFields)obj).CardShopRemovalsUsed = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "CardShopRemovalsUsed";
		jsonPropertyInfoValues.JsonPropertyName = "card_shop_removals_used";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableExtraPlayerFields).GetProperty("CardShopRemovalsUsed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		jsonPropertyInfoValues = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableExtraPlayerFields);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableExtraPlayerFields)obj).WongoPoints;
		jsonPropertyInfoValues.Setter = delegate(object obj, int value)
		{
			((SerializableExtraPlayerFields)obj).WongoPoints = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "WongoPoints";
		jsonPropertyInfoValues.JsonPropertyName = "wongo_points";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableExtraPlayerFields).GetProperty("WongoPoints", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo2 = jsonPropertyInfoValues;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		return array;
	}

	private JsonTypeInfo<SerializablePlayerRngSet> Create_SerializablePlayerRngSet(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SerializablePlayerRngSet> jsonTypeInfo))
		{
			JsonObjectInfoValues<SerializablePlayerRngSet> objectInfo = new JsonObjectInfoValues<SerializablePlayerRngSet>
			{
				ObjectCreator = () => new SerializablePlayerRngSet(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SerializablePlayerRngSetPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SerializablePlayerRngSet).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SerializablePlayerRngSetPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<uint> jsonPropertyInfoValues = new JsonPropertyInfoValues<uint>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializablePlayerRngSet);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializablePlayerRngSet)obj).Seed;
		jsonPropertyInfoValues.Setter = delegate(object obj, uint value)
		{
			((SerializablePlayerRngSet)obj).Seed = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Seed";
		jsonPropertyInfoValues.JsonPropertyName = "seed";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializablePlayerRngSet).GetProperty("Seed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(uint), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<uint> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<Dictionary<PlayerRngType, int>> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<Dictionary<PlayerRngType, int>>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SerializablePlayerRngSet);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SerializablePlayerRngSet)obj).Counters;
		jsonPropertyInfoValues2.Setter = delegate(object obj, Dictionary<PlayerRngType, int>? value)
		{
			((SerializablePlayerRngSet)obj).Counters = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "Counters";
		jsonPropertyInfoValues2.JsonPropertyName = "counters";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SerializablePlayerRngSet).GetProperty("Counters", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(Dictionary<PlayerRngType, int>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<Dictionary<PlayerRngType, int>> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		return array;
	}

	private JsonTypeInfo<SerializableProgress> Create_SerializableProgress(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SerializableProgress> jsonTypeInfo))
		{
			JsonObjectInfoValues<SerializableProgress> jsonObjectInfoValues = new JsonObjectInfoValues<SerializableProgress>();
			jsonObjectInfoValues.ObjectCreator = null;
			jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = (object[] args) => new SerializableProgress
			{
				UniqueId = (string)args[0]
			};
			jsonObjectInfoValues.PropertyMetadataInitializer = (JsonSerializerContext _) => SerializableProgressPropInit(options);
			jsonObjectInfoValues.ConstructorParameterMetadataInitializer = SerializableProgressCtorParamInit;
			jsonObjectInfoValues.ConstructorAttributeProviderFactory = () => typeof(SerializableProgress).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null);
			jsonObjectInfoValues.SerializeHandler = null;
			JsonObjectInfoValues<SerializableProgress> objectInfo = jsonObjectInfoValues;
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SerializableProgressPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[31];
		JsonPropertyInfoValues<int> jsonPropertyInfoValues = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableProgress);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableProgress)obj).SchemaVersion;
		jsonPropertyInfoValues.Setter = delegate(object obj, int value)
		{
			((SerializableProgress)obj).SchemaVersion = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "SchemaVersion";
		jsonPropertyInfoValues.JsonPropertyName = "schema_version";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableProgress).GetProperty("SchemaVersion", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<string> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SerializableProgress);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SerializableProgress)obj).UniqueId;
		jsonPropertyInfoValues2.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "UniqueId";
		jsonPropertyInfoValues2.JsonPropertyName = "unique_id";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SerializableProgress).GetProperty("UniqueId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		JsonPropertyInfoValues<List<CharacterStats>> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<List<CharacterStats>>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(SerializableProgress);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((SerializableProgress)obj).CharStats;
		jsonPropertyInfoValues3.Setter = delegate(object obj, List<CharacterStats>? value)
		{
			((SerializableProgress)obj).CharStats = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "CharStats";
		jsonPropertyInfoValues3.JsonPropertyName = "character_stats";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(SerializableProgress).GetProperty("CharStats", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<CharacterStats>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<CharacterStats>> propertyInfo3 = jsonPropertyInfoValues3;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		array[2].IsGetNullable = false;
		array[2].IsSetNullable = false;
		JsonPropertyInfoValues<List<CardStats>> jsonPropertyInfoValues4 = new JsonPropertyInfoValues<List<CardStats>>();
		jsonPropertyInfoValues4.IsProperty = true;
		jsonPropertyInfoValues4.IsPublic = true;
		jsonPropertyInfoValues4.IsVirtual = false;
		jsonPropertyInfoValues4.DeclaringType = typeof(SerializableProgress);
		jsonPropertyInfoValues4.Converter = null;
		jsonPropertyInfoValues4.Getter = (object obj) => ((SerializableProgress)obj).CardStats;
		jsonPropertyInfoValues4.Setter = delegate(object obj, List<CardStats>? value)
		{
			((SerializableProgress)obj).CardStats = value;
		};
		jsonPropertyInfoValues4.IgnoreCondition = null;
		jsonPropertyInfoValues4.HasJsonInclude = false;
		jsonPropertyInfoValues4.IsExtensionData = false;
		jsonPropertyInfoValues4.NumberHandling = null;
		jsonPropertyInfoValues4.PropertyName = "CardStats";
		jsonPropertyInfoValues4.JsonPropertyName = "card_stats";
		jsonPropertyInfoValues4.AttributeProviderFactory = () => typeof(SerializableProgress).GetProperty("CardStats", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<CardStats>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<CardStats>> propertyInfo4 = jsonPropertyInfoValues4;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		array[3].IsGetNullable = false;
		array[3].IsSetNullable = false;
		JsonPropertyInfoValues<List<EncounterStats>> jsonPropertyInfoValues5 = new JsonPropertyInfoValues<List<EncounterStats>>();
		jsonPropertyInfoValues5.IsProperty = true;
		jsonPropertyInfoValues5.IsPublic = true;
		jsonPropertyInfoValues5.IsVirtual = false;
		jsonPropertyInfoValues5.DeclaringType = typeof(SerializableProgress);
		jsonPropertyInfoValues5.Converter = null;
		jsonPropertyInfoValues5.Getter = (object obj) => ((SerializableProgress)obj).EncounterStats;
		jsonPropertyInfoValues5.Setter = delegate(object obj, List<EncounterStats>? value)
		{
			((SerializableProgress)obj).EncounterStats = value;
		};
		jsonPropertyInfoValues5.IgnoreCondition = null;
		jsonPropertyInfoValues5.HasJsonInclude = false;
		jsonPropertyInfoValues5.IsExtensionData = false;
		jsonPropertyInfoValues5.NumberHandling = null;
		jsonPropertyInfoValues5.PropertyName = "EncounterStats";
		jsonPropertyInfoValues5.JsonPropertyName = "encounter_stats";
		jsonPropertyInfoValues5.AttributeProviderFactory = () => typeof(SerializableProgress).GetProperty("EncounterStats", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<EncounterStats>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<EncounterStats>> propertyInfo5 = jsonPropertyInfoValues5;
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		array[4].IsGetNullable = false;
		array[4].IsSetNullable = false;
		JsonPropertyInfoValues<List<EnemyStats>> jsonPropertyInfoValues6 = new JsonPropertyInfoValues<List<EnemyStats>>();
		jsonPropertyInfoValues6.IsProperty = true;
		jsonPropertyInfoValues6.IsPublic = true;
		jsonPropertyInfoValues6.IsVirtual = false;
		jsonPropertyInfoValues6.DeclaringType = typeof(SerializableProgress);
		jsonPropertyInfoValues6.Converter = null;
		jsonPropertyInfoValues6.Getter = (object obj) => ((SerializableProgress)obj).EnemyStats;
		jsonPropertyInfoValues6.Setter = delegate(object obj, List<EnemyStats>? value)
		{
			((SerializableProgress)obj).EnemyStats = value;
		};
		jsonPropertyInfoValues6.IgnoreCondition = null;
		jsonPropertyInfoValues6.HasJsonInclude = false;
		jsonPropertyInfoValues6.IsExtensionData = false;
		jsonPropertyInfoValues6.NumberHandling = null;
		jsonPropertyInfoValues6.PropertyName = "EnemyStats";
		jsonPropertyInfoValues6.JsonPropertyName = "enemy_stats";
		jsonPropertyInfoValues6.AttributeProviderFactory = () => typeof(SerializableProgress).GetProperty("EnemyStats", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<EnemyStats>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<EnemyStats>> propertyInfo6 = jsonPropertyInfoValues6;
		array[5] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo6);
		array[5].IsGetNullable = false;
		array[5].IsSetNullable = false;
		JsonPropertyInfoValues<List<AncientStats>> jsonPropertyInfoValues7 = new JsonPropertyInfoValues<List<AncientStats>>();
		jsonPropertyInfoValues7.IsProperty = true;
		jsonPropertyInfoValues7.IsPublic = true;
		jsonPropertyInfoValues7.IsVirtual = false;
		jsonPropertyInfoValues7.DeclaringType = typeof(SerializableProgress);
		jsonPropertyInfoValues7.Converter = null;
		jsonPropertyInfoValues7.Getter = (object obj) => ((SerializableProgress)obj).AncientStats;
		jsonPropertyInfoValues7.Setter = delegate(object obj, List<AncientStats>? value)
		{
			((SerializableProgress)obj).AncientStats = value;
		};
		jsonPropertyInfoValues7.IgnoreCondition = null;
		jsonPropertyInfoValues7.HasJsonInclude = false;
		jsonPropertyInfoValues7.IsExtensionData = false;
		jsonPropertyInfoValues7.NumberHandling = null;
		jsonPropertyInfoValues7.PropertyName = "AncientStats";
		jsonPropertyInfoValues7.JsonPropertyName = "ancient_stats";
		jsonPropertyInfoValues7.AttributeProviderFactory = () => typeof(SerializableProgress).GetProperty("AncientStats", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<AncientStats>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<AncientStats>> propertyInfo7 = jsonPropertyInfoValues7;
		array[6] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo7);
		array[6].IsGetNullable = false;
		array[6].IsSetNullable = false;
		JsonPropertyInfoValues<bool> jsonPropertyInfoValues8 = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues8.IsProperty = true;
		jsonPropertyInfoValues8.IsPublic = true;
		jsonPropertyInfoValues8.IsVirtual = false;
		jsonPropertyInfoValues8.DeclaringType = typeof(SerializableProgress);
		jsonPropertyInfoValues8.Converter = null;
		jsonPropertyInfoValues8.Getter = (object obj) => ((SerializableProgress)obj).EnableFtues;
		jsonPropertyInfoValues8.Setter = delegate(object obj, bool value)
		{
			((SerializableProgress)obj).EnableFtues = value;
		};
		jsonPropertyInfoValues8.IgnoreCondition = null;
		jsonPropertyInfoValues8.HasJsonInclude = false;
		jsonPropertyInfoValues8.IsExtensionData = false;
		jsonPropertyInfoValues8.NumberHandling = null;
		jsonPropertyInfoValues8.PropertyName = "EnableFtues";
		jsonPropertyInfoValues8.JsonPropertyName = "enable_ftues";
		jsonPropertyInfoValues8.AttributeProviderFactory = () => typeof(SerializableProgress).GetProperty("EnableFtues", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<bool> propertyInfo8 = jsonPropertyInfoValues8;
		array[7] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo8);
		JsonPropertyInfoValues<List<SerializableEpoch>> jsonPropertyInfoValues9 = new JsonPropertyInfoValues<List<SerializableEpoch>>();
		jsonPropertyInfoValues9.IsProperty = true;
		jsonPropertyInfoValues9.IsPublic = true;
		jsonPropertyInfoValues9.IsVirtual = false;
		jsonPropertyInfoValues9.DeclaringType = typeof(SerializableProgress);
		jsonPropertyInfoValues9.Converter = null;
		jsonPropertyInfoValues9.Getter = (object obj) => ((SerializableProgress)obj).Epochs;
		jsonPropertyInfoValues9.Setter = delegate(object obj, List<SerializableEpoch>? value)
		{
			((SerializableProgress)obj).Epochs = value;
		};
		jsonPropertyInfoValues9.IgnoreCondition = null;
		jsonPropertyInfoValues9.HasJsonInclude = false;
		jsonPropertyInfoValues9.IsExtensionData = false;
		jsonPropertyInfoValues9.NumberHandling = null;
		jsonPropertyInfoValues9.PropertyName = "Epochs";
		jsonPropertyInfoValues9.JsonPropertyName = "epochs";
		jsonPropertyInfoValues9.AttributeProviderFactory = () => typeof(SerializableProgress).GetProperty("Epochs", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<SerializableEpoch>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<SerializableEpoch>> propertyInfo9 = jsonPropertyInfoValues9;
		array[8] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo9);
		array[8].IsGetNullable = false;
		array[8].IsSetNullable = false;
		JsonPropertyInfoValues<List<string>> jsonPropertyInfoValues10 = new JsonPropertyInfoValues<List<string>>();
		jsonPropertyInfoValues10.IsProperty = true;
		jsonPropertyInfoValues10.IsPublic = true;
		jsonPropertyInfoValues10.IsVirtual = false;
		jsonPropertyInfoValues10.DeclaringType = typeof(SerializableProgress);
		jsonPropertyInfoValues10.Converter = null;
		jsonPropertyInfoValues10.Getter = (object obj) => ((SerializableProgress)obj).FtueCompleted;
		jsonPropertyInfoValues10.Setter = delegate(object obj, List<string>? value)
		{
			((SerializableProgress)obj).FtueCompleted = value;
		};
		jsonPropertyInfoValues10.IgnoreCondition = null;
		jsonPropertyInfoValues10.HasJsonInclude = false;
		jsonPropertyInfoValues10.IsExtensionData = false;
		jsonPropertyInfoValues10.NumberHandling = null;
		jsonPropertyInfoValues10.PropertyName = "FtueCompleted";
		jsonPropertyInfoValues10.JsonPropertyName = "ftue_completed";
		jsonPropertyInfoValues10.AttributeProviderFactory = () => typeof(SerializableProgress).GetProperty("FtueCompleted", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<string>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<string>> propertyInfo10 = jsonPropertyInfoValues10;
		array[9] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo10);
		array[9].IsGetNullable = false;
		array[9].IsSetNullable = false;
		JsonPropertyInfoValues<List<SerializableUnlockedAchievement>> jsonPropertyInfoValues11 = new JsonPropertyInfoValues<List<SerializableUnlockedAchievement>>();
		jsonPropertyInfoValues11.IsProperty = true;
		jsonPropertyInfoValues11.IsPublic = true;
		jsonPropertyInfoValues11.IsVirtual = false;
		jsonPropertyInfoValues11.DeclaringType = typeof(SerializableProgress);
		jsonPropertyInfoValues11.Converter = null;
		jsonPropertyInfoValues11.Getter = (object obj) => ((SerializableProgress)obj).UnlockedAchievements;
		jsonPropertyInfoValues11.Setter = delegate(object obj, List<SerializableUnlockedAchievement>? value)
		{
			((SerializableProgress)obj).UnlockedAchievements = value;
		};
		jsonPropertyInfoValues11.IgnoreCondition = null;
		jsonPropertyInfoValues11.HasJsonInclude = false;
		jsonPropertyInfoValues11.IsExtensionData = false;
		jsonPropertyInfoValues11.NumberHandling = null;
		jsonPropertyInfoValues11.PropertyName = "UnlockedAchievements";
		jsonPropertyInfoValues11.JsonPropertyName = "unlocked_achievements";
		jsonPropertyInfoValues11.AttributeProviderFactory = () => typeof(SerializableProgress).GetProperty("UnlockedAchievements", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<SerializableUnlockedAchievement>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<SerializableUnlockedAchievement>> propertyInfo11 = jsonPropertyInfoValues11;
		array[10] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo11);
		array[10].IsGetNullable = false;
		array[10].IsSetNullable = false;
		JsonPropertyInfoValues<List<ModelId>> jsonPropertyInfoValues12 = new JsonPropertyInfoValues<List<ModelId>>();
		jsonPropertyInfoValues12.IsProperty = true;
		jsonPropertyInfoValues12.IsPublic = true;
		jsonPropertyInfoValues12.IsVirtual = false;
		jsonPropertyInfoValues12.DeclaringType = typeof(SerializableProgress);
		jsonPropertyInfoValues12.Converter = null;
		jsonPropertyInfoValues12.Getter = (object obj) => ((SerializableProgress)obj).DiscoveredCards;
		jsonPropertyInfoValues12.Setter = delegate(object obj, List<ModelId>? value)
		{
			((SerializableProgress)obj).DiscoveredCards = value;
		};
		jsonPropertyInfoValues12.IgnoreCondition = null;
		jsonPropertyInfoValues12.HasJsonInclude = false;
		jsonPropertyInfoValues12.IsExtensionData = false;
		jsonPropertyInfoValues12.NumberHandling = null;
		jsonPropertyInfoValues12.PropertyName = "DiscoveredCards";
		jsonPropertyInfoValues12.JsonPropertyName = "discovered_cards";
		jsonPropertyInfoValues12.AttributeProviderFactory = () => typeof(SerializableProgress).GetProperty("DiscoveredCards", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<ModelId>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<ModelId>> propertyInfo12 = jsonPropertyInfoValues12;
		array[11] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo12);
		array[11].IsGetNullable = false;
		array[11].IsSetNullable = false;
		jsonPropertyInfoValues12 = new JsonPropertyInfoValues<List<ModelId>>();
		jsonPropertyInfoValues12.IsProperty = true;
		jsonPropertyInfoValues12.IsPublic = true;
		jsonPropertyInfoValues12.IsVirtual = false;
		jsonPropertyInfoValues12.DeclaringType = typeof(SerializableProgress);
		jsonPropertyInfoValues12.Converter = null;
		jsonPropertyInfoValues12.Getter = (object obj) => ((SerializableProgress)obj).DiscoveredRelics;
		jsonPropertyInfoValues12.Setter = delegate(object obj, List<ModelId>? value)
		{
			((SerializableProgress)obj).DiscoveredRelics = value;
		};
		jsonPropertyInfoValues12.IgnoreCondition = null;
		jsonPropertyInfoValues12.HasJsonInclude = false;
		jsonPropertyInfoValues12.IsExtensionData = false;
		jsonPropertyInfoValues12.NumberHandling = null;
		jsonPropertyInfoValues12.PropertyName = "DiscoveredRelics";
		jsonPropertyInfoValues12.JsonPropertyName = "discovered_relics";
		jsonPropertyInfoValues12.AttributeProviderFactory = () => typeof(SerializableProgress).GetProperty("DiscoveredRelics", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<ModelId>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<ModelId>> propertyInfo13 = jsonPropertyInfoValues12;
		array[12] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo13);
		array[12].IsGetNullable = false;
		array[12].IsSetNullable = false;
		jsonPropertyInfoValues12 = new JsonPropertyInfoValues<List<ModelId>>();
		jsonPropertyInfoValues12.IsProperty = true;
		jsonPropertyInfoValues12.IsPublic = true;
		jsonPropertyInfoValues12.IsVirtual = false;
		jsonPropertyInfoValues12.DeclaringType = typeof(SerializableProgress);
		jsonPropertyInfoValues12.Converter = null;
		jsonPropertyInfoValues12.Getter = (object obj) => ((SerializableProgress)obj).DiscoveredEvents;
		jsonPropertyInfoValues12.Setter = delegate(object obj, List<ModelId>? value)
		{
			((SerializableProgress)obj).DiscoveredEvents = value;
		};
		jsonPropertyInfoValues12.IgnoreCondition = null;
		jsonPropertyInfoValues12.HasJsonInclude = false;
		jsonPropertyInfoValues12.IsExtensionData = false;
		jsonPropertyInfoValues12.NumberHandling = null;
		jsonPropertyInfoValues12.PropertyName = "DiscoveredEvents";
		jsonPropertyInfoValues12.JsonPropertyName = "discovered_events";
		jsonPropertyInfoValues12.AttributeProviderFactory = () => typeof(SerializableProgress).GetProperty("DiscoveredEvents", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<ModelId>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<ModelId>> propertyInfo14 = jsonPropertyInfoValues12;
		array[13] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo14);
		array[13].IsGetNullable = false;
		array[13].IsSetNullable = false;
		jsonPropertyInfoValues12 = new JsonPropertyInfoValues<List<ModelId>>();
		jsonPropertyInfoValues12.IsProperty = true;
		jsonPropertyInfoValues12.IsPublic = true;
		jsonPropertyInfoValues12.IsVirtual = false;
		jsonPropertyInfoValues12.DeclaringType = typeof(SerializableProgress);
		jsonPropertyInfoValues12.Converter = null;
		jsonPropertyInfoValues12.Getter = (object obj) => ((SerializableProgress)obj).DiscoveredPotions;
		jsonPropertyInfoValues12.Setter = delegate(object obj, List<ModelId>? value)
		{
			((SerializableProgress)obj).DiscoveredPotions = value;
		};
		jsonPropertyInfoValues12.IgnoreCondition = null;
		jsonPropertyInfoValues12.HasJsonInclude = false;
		jsonPropertyInfoValues12.IsExtensionData = false;
		jsonPropertyInfoValues12.NumberHandling = null;
		jsonPropertyInfoValues12.PropertyName = "DiscoveredPotions";
		jsonPropertyInfoValues12.JsonPropertyName = "discovered_potions";
		jsonPropertyInfoValues12.AttributeProviderFactory = () => typeof(SerializableProgress).GetProperty("DiscoveredPotions", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<ModelId>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<ModelId>> propertyInfo15 = jsonPropertyInfoValues12;
		array[14] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo15);
		array[14].IsGetNullable = false;
		array[14].IsSetNullable = false;
		jsonPropertyInfoValues12 = new JsonPropertyInfoValues<List<ModelId>>();
		jsonPropertyInfoValues12.IsProperty = true;
		jsonPropertyInfoValues12.IsPublic = true;
		jsonPropertyInfoValues12.IsVirtual = false;
		jsonPropertyInfoValues12.DeclaringType = typeof(SerializableProgress);
		jsonPropertyInfoValues12.Converter = null;
		jsonPropertyInfoValues12.Getter = (object obj) => ((SerializableProgress)obj).DiscoveredActs;
		jsonPropertyInfoValues12.Setter = delegate(object obj, List<ModelId>? value)
		{
			((SerializableProgress)obj).DiscoveredActs = value;
		};
		jsonPropertyInfoValues12.IgnoreCondition = null;
		jsonPropertyInfoValues12.HasJsonInclude = false;
		jsonPropertyInfoValues12.IsExtensionData = false;
		jsonPropertyInfoValues12.NumberHandling = null;
		jsonPropertyInfoValues12.PropertyName = "DiscoveredActs";
		jsonPropertyInfoValues12.JsonPropertyName = "discovered_acts";
		jsonPropertyInfoValues12.AttributeProviderFactory = () => typeof(SerializableProgress).GetProperty("DiscoveredActs", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<ModelId>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<ModelId>> propertyInfo16 = jsonPropertyInfoValues12;
		array[15] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo16);
		array[15].IsGetNullable = false;
		array[15].IsSetNullable = false;
		JsonPropertyInfoValues<long> jsonPropertyInfoValues13 = new JsonPropertyInfoValues<long>();
		jsonPropertyInfoValues13.IsProperty = true;
		jsonPropertyInfoValues13.IsPublic = true;
		jsonPropertyInfoValues13.IsVirtual = false;
		jsonPropertyInfoValues13.DeclaringType = typeof(SerializableProgress);
		jsonPropertyInfoValues13.Converter = null;
		jsonPropertyInfoValues13.Getter = (object obj) => ((SerializableProgress)obj).TotalPlaytime;
		jsonPropertyInfoValues13.Setter = delegate(object obj, long value)
		{
			((SerializableProgress)obj).TotalPlaytime = value;
		};
		jsonPropertyInfoValues13.IgnoreCondition = null;
		jsonPropertyInfoValues13.HasJsonInclude = false;
		jsonPropertyInfoValues13.IsExtensionData = false;
		jsonPropertyInfoValues13.NumberHandling = null;
		jsonPropertyInfoValues13.PropertyName = "TotalPlaytime";
		jsonPropertyInfoValues13.JsonPropertyName = "total_playtime";
		jsonPropertyInfoValues13.AttributeProviderFactory = () => typeof(SerializableProgress).GetProperty("TotalPlaytime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(long), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<long> propertyInfo17 = jsonPropertyInfoValues13;
		array[16] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo17);
		jsonPropertyInfoValues = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableProgress);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableProgress)obj).TotalUnlocks;
		jsonPropertyInfoValues.Setter = delegate(object obj, int value)
		{
			((SerializableProgress)obj).TotalUnlocks = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "TotalUnlocks";
		jsonPropertyInfoValues.JsonPropertyName = "total_unlocks";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableProgress).GetProperty("TotalUnlocks", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo18 = jsonPropertyInfoValues;
		array[17] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo18);
		jsonPropertyInfoValues = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableProgress);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableProgress)obj).CurrentScore;
		jsonPropertyInfoValues.Setter = delegate(object obj, int value)
		{
			((SerializableProgress)obj).CurrentScore = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "CurrentScore";
		jsonPropertyInfoValues.JsonPropertyName = "current_score";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableProgress).GetProperty("CurrentScore", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo19 = jsonPropertyInfoValues;
		array[18] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo19);
		jsonPropertyInfoValues13 = new JsonPropertyInfoValues<long>();
		jsonPropertyInfoValues13.IsProperty = true;
		jsonPropertyInfoValues13.IsPublic = true;
		jsonPropertyInfoValues13.IsVirtual = false;
		jsonPropertyInfoValues13.DeclaringType = typeof(SerializableProgress);
		jsonPropertyInfoValues13.Converter = null;
		jsonPropertyInfoValues13.Getter = (object obj) => ((SerializableProgress)obj).FloorsClimbed;
		jsonPropertyInfoValues13.Setter = delegate(object obj, long value)
		{
			((SerializableProgress)obj).FloorsClimbed = value;
		};
		jsonPropertyInfoValues13.IgnoreCondition = null;
		jsonPropertyInfoValues13.HasJsonInclude = false;
		jsonPropertyInfoValues13.IsExtensionData = false;
		jsonPropertyInfoValues13.NumberHandling = null;
		jsonPropertyInfoValues13.PropertyName = "FloorsClimbed";
		jsonPropertyInfoValues13.JsonPropertyName = "floors_climbed";
		jsonPropertyInfoValues13.AttributeProviderFactory = () => typeof(SerializableProgress).GetProperty("FloorsClimbed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(long), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<long> propertyInfo20 = jsonPropertyInfoValues13;
		array[19] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo20);
		jsonPropertyInfoValues13 = new JsonPropertyInfoValues<long>();
		jsonPropertyInfoValues13.IsProperty = true;
		jsonPropertyInfoValues13.IsPublic = true;
		jsonPropertyInfoValues13.IsVirtual = false;
		jsonPropertyInfoValues13.DeclaringType = typeof(SerializableProgress);
		jsonPropertyInfoValues13.Converter = null;
		jsonPropertyInfoValues13.Getter = (object obj) => ((SerializableProgress)obj).ArchitectDamage;
		jsonPropertyInfoValues13.Setter = delegate(object obj, long value)
		{
			((SerializableProgress)obj).ArchitectDamage = value;
		};
		jsonPropertyInfoValues13.IgnoreCondition = null;
		jsonPropertyInfoValues13.HasJsonInclude = false;
		jsonPropertyInfoValues13.IsExtensionData = false;
		jsonPropertyInfoValues13.NumberHandling = null;
		jsonPropertyInfoValues13.PropertyName = "ArchitectDamage";
		jsonPropertyInfoValues13.JsonPropertyName = "architect_damage";
		jsonPropertyInfoValues13.AttributeProviderFactory = () => typeof(SerializableProgress).GetProperty("ArchitectDamage", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(long), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<long> propertyInfo21 = jsonPropertyInfoValues13;
		array[20] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo21);
		jsonPropertyInfoValues = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableProgress);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableProgress)obj).WongoPoints;
		jsonPropertyInfoValues.Setter = delegate(object obj, int value)
		{
			((SerializableProgress)obj).WongoPoints = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "WongoPoints";
		jsonPropertyInfoValues.JsonPropertyName = "wongo_points";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableProgress).GetProperty("WongoPoints", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo22 = jsonPropertyInfoValues;
		array[21] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo22);
		jsonPropertyInfoValues = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableProgress);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableProgress)obj).PreferredMultiplayerAscension;
		jsonPropertyInfoValues.Setter = delegate(object obj, int value)
		{
			((SerializableProgress)obj).PreferredMultiplayerAscension = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "PreferredMultiplayerAscension";
		jsonPropertyInfoValues.JsonPropertyName = "preferred_multiplayer_ascension";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableProgress).GetProperty("PreferredMultiplayerAscension", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo23 = jsonPropertyInfoValues;
		array[22] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo23);
		jsonPropertyInfoValues = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableProgress);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableProgress)obj).MaxMultiplayerAscension;
		jsonPropertyInfoValues.Setter = delegate(object obj, int value)
		{
			((SerializableProgress)obj).MaxMultiplayerAscension = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "MaxMultiplayerAscension";
		jsonPropertyInfoValues.JsonPropertyName = "max_multiplayer_ascension";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableProgress).GetProperty("MaxMultiplayerAscension", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo24 = jsonPropertyInfoValues;
		array[23] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo24);
		jsonPropertyInfoValues = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableProgress);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableProgress)obj).TestSubjectKills;
		jsonPropertyInfoValues.Setter = delegate(object obj, int value)
		{
			((SerializableProgress)obj).TestSubjectKills = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "TestSubjectKills";
		jsonPropertyInfoValues.JsonPropertyName = "test_subject_kills";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableProgress).GetProperty("TestSubjectKills", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo25 = jsonPropertyInfoValues;
		array[24] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo25);
		JsonPropertyInfoValues<ModelId> jsonPropertyInfoValues14 = new JsonPropertyInfoValues<ModelId>();
		jsonPropertyInfoValues14.IsProperty = true;
		jsonPropertyInfoValues14.IsPublic = true;
		jsonPropertyInfoValues14.IsVirtual = false;
		jsonPropertyInfoValues14.DeclaringType = typeof(SerializableProgress);
		jsonPropertyInfoValues14.Converter = null;
		jsonPropertyInfoValues14.Getter = (object obj) => ((SerializableProgress)obj).PendingCharacterUnlock;
		jsonPropertyInfoValues14.Setter = delegate(object obj, ModelId? value)
		{
			((SerializableProgress)obj).PendingCharacterUnlock = value;
		};
		jsonPropertyInfoValues14.IgnoreCondition = null;
		jsonPropertyInfoValues14.HasJsonInclude = false;
		jsonPropertyInfoValues14.IsExtensionData = false;
		jsonPropertyInfoValues14.NumberHandling = null;
		jsonPropertyInfoValues14.PropertyName = "PendingCharacterUnlock";
		jsonPropertyInfoValues14.JsonPropertyName = "pending_character_unlock";
		jsonPropertyInfoValues14.AttributeProviderFactory = () => typeof(SerializableProgress).GetProperty("PendingCharacterUnlock", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModelId), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModelId> propertyInfo26 = jsonPropertyInfoValues14;
		array[25] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo26);
		array[25].IsGetNullable = false;
		array[25].IsSetNullable = false;
		jsonPropertyInfoValues = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableProgress);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = null;
		jsonPropertyInfoValues.Setter = null;
		jsonPropertyInfoValues.IgnoreCondition = JsonIgnoreCondition.Always;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Wins";
		jsonPropertyInfoValues.JsonPropertyName = null;
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableProgress).GetProperty("Wins", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo27 = jsonPropertyInfoValues;
		array[26] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo27);
		jsonPropertyInfoValues = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableProgress);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = null;
		jsonPropertyInfoValues.Setter = null;
		jsonPropertyInfoValues.IgnoreCondition = JsonIgnoreCondition.Always;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Losses";
		jsonPropertyInfoValues.JsonPropertyName = null;
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableProgress).GetProperty("Losses", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo28 = jsonPropertyInfoValues;
		array[27] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo28);
		jsonPropertyInfoValues13 = new JsonPropertyInfoValues<long>();
		jsonPropertyInfoValues13.IsProperty = true;
		jsonPropertyInfoValues13.IsPublic = true;
		jsonPropertyInfoValues13.IsVirtual = false;
		jsonPropertyInfoValues13.DeclaringType = typeof(SerializableProgress);
		jsonPropertyInfoValues13.Converter = null;
		jsonPropertyInfoValues13.Getter = null;
		jsonPropertyInfoValues13.Setter = null;
		jsonPropertyInfoValues13.IgnoreCondition = JsonIgnoreCondition.Always;
		jsonPropertyInfoValues13.HasJsonInclude = false;
		jsonPropertyInfoValues13.IsExtensionData = false;
		jsonPropertyInfoValues13.NumberHandling = null;
		jsonPropertyInfoValues13.PropertyName = "FastestVictory";
		jsonPropertyInfoValues13.JsonPropertyName = null;
		jsonPropertyInfoValues13.AttributeProviderFactory = () => typeof(SerializableProgress).GetProperty("FastestVictory", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(long), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<long> propertyInfo29 = jsonPropertyInfoValues13;
		array[28] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo29);
		jsonPropertyInfoValues13 = new JsonPropertyInfoValues<long>();
		jsonPropertyInfoValues13.IsProperty = true;
		jsonPropertyInfoValues13.IsPublic = true;
		jsonPropertyInfoValues13.IsVirtual = false;
		jsonPropertyInfoValues13.DeclaringType = typeof(SerializableProgress);
		jsonPropertyInfoValues13.Converter = null;
		jsonPropertyInfoValues13.Getter = null;
		jsonPropertyInfoValues13.Setter = null;
		jsonPropertyInfoValues13.IgnoreCondition = JsonIgnoreCondition.Always;
		jsonPropertyInfoValues13.HasJsonInclude = false;
		jsonPropertyInfoValues13.IsExtensionData = false;
		jsonPropertyInfoValues13.NumberHandling = null;
		jsonPropertyInfoValues13.PropertyName = "BestWinStreak";
		jsonPropertyInfoValues13.JsonPropertyName = null;
		jsonPropertyInfoValues13.AttributeProviderFactory = () => typeof(SerializableProgress).GetProperty("BestWinStreak", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(long), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<long> propertyInfo30 = jsonPropertyInfoValues13;
		array[29] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo30);
		jsonPropertyInfoValues = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableProgress);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = null;
		jsonPropertyInfoValues.Setter = null;
		jsonPropertyInfoValues.IgnoreCondition = JsonIgnoreCondition.Always;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "NumberOfRuns";
		jsonPropertyInfoValues.JsonPropertyName = null;
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableProgress).GetProperty("NumberOfRuns", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo31 = jsonPropertyInfoValues;
		array[30] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo31);
		return array;
	}

	private static JsonParameterInfoValues[] SerializableProgressCtorParamInit()
	{
		return new JsonParameterInfoValues[1]
		{
			new JsonParameterInfoValues
			{
				Name = "UniqueId",
				ParameterType = typeof(string),
				Position = 0,
				IsNullable = false,
				IsMemberInitializer = true
			}
		};
	}

	private JsonTypeInfo<SerializableRun> Create_SerializableRun(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SerializableRun> jsonTypeInfo))
		{
			JsonObjectInfoValues<SerializableRun> objectInfo = new JsonObjectInfoValues<SerializableRun>
			{
				ObjectCreator = () => new SerializableRun(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SerializableRunPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SerializableRun).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SerializableRunPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[21];
		JsonPropertyInfoValues<int> jsonPropertyInfoValues = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableRun);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableRun)obj).SchemaVersion;
		jsonPropertyInfoValues.Setter = delegate(object obj, int value)
		{
			((SerializableRun)obj).SchemaVersion = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "SchemaVersion";
		jsonPropertyInfoValues.JsonPropertyName = "schema_version";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableRun).GetProperty("SchemaVersion", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<List<SerializableActModel>> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<List<SerializableActModel>>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SerializableRun);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SerializableRun)obj).Acts;
		jsonPropertyInfoValues2.Setter = delegate(object obj, List<SerializableActModel>? value)
		{
			((SerializableRun)obj).Acts = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "Acts";
		jsonPropertyInfoValues2.JsonPropertyName = "acts";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SerializableRun).GetProperty("Acts", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<SerializableActModel>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<SerializableActModel>> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		JsonPropertyInfoValues<List<SerializableModifier>> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<List<SerializableModifier>>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(SerializableRun);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((SerializableRun)obj).Modifiers;
		jsonPropertyInfoValues3.Setter = delegate(object obj, List<SerializableModifier>? value)
		{
			((SerializableRun)obj).Modifiers = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "Modifiers";
		jsonPropertyInfoValues3.JsonPropertyName = "modifiers";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(SerializableRun).GetProperty("Modifiers", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<SerializableModifier>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<SerializableModifier>> propertyInfo3 = jsonPropertyInfoValues3;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		array[2].IsGetNullable = false;
		array[2].IsSetNullable = false;
		JsonPropertyInfoValues<DateTimeOffset?> jsonPropertyInfoValues4 = new JsonPropertyInfoValues<DateTimeOffset?>();
		jsonPropertyInfoValues4.IsProperty = true;
		jsonPropertyInfoValues4.IsPublic = true;
		jsonPropertyInfoValues4.IsVirtual = false;
		jsonPropertyInfoValues4.DeclaringType = typeof(SerializableRun);
		jsonPropertyInfoValues4.Converter = null;
		jsonPropertyInfoValues4.Getter = (object obj) => ((SerializableRun)obj).DailyTime;
		jsonPropertyInfoValues4.Setter = delegate(object obj, DateTimeOffset? value)
		{
			((SerializableRun)obj).DailyTime = value;
		};
		jsonPropertyInfoValues4.IgnoreCondition = null;
		jsonPropertyInfoValues4.HasJsonInclude = false;
		jsonPropertyInfoValues4.IsExtensionData = false;
		jsonPropertyInfoValues4.NumberHandling = null;
		jsonPropertyInfoValues4.PropertyName = "DailyTime";
		jsonPropertyInfoValues4.JsonPropertyName = "dailyTime";
		jsonPropertyInfoValues4.AttributeProviderFactory = () => typeof(SerializableRun).GetProperty("DailyTime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(DateTimeOffset?), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<DateTimeOffset?> propertyInfo4 = jsonPropertyInfoValues4;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		jsonPropertyInfoValues = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableRun);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableRun)obj).CurrentActIndex;
		jsonPropertyInfoValues.Setter = delegate(object obj, int value)
		{
			((SerializableRun)obj).CurrentActIndex = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "CurrentActIndex";
		jsonPropertyInfoValues.JsonPropertyName = "current_act_index";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableRun).GetProperty("CurrentActIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo5 = jsonPropertyInfoValues;
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		JsonPropertyInfoValues<List<ModelId>> jsonPropertyInfoValues5 = new JsonPropertyInfoValues<List<ModelId>>();
		jsonPropertyInfoValues5.IsProperty = true;
		jsonPropertyInfoValues5.IsPublic = true;
		jsonPropertyInfoValues5.IsVirtual = false;
		jsonPropertyInfoValues5.DeclaringType = typeof(SerializableRun);
		jsonPropertyInfoValues5.Converter = null;
		jsonPropertyInfoValues5.Getter = (object obj) => ((SerializableRun)obj).EventsSeen;
		jsonPropertyInfoValues5.Setter = delegate(object obj, List<ModelId>? value)
		{
			((SerializableRun)obj).EventsSeen = value;
		};
		jsonPropertyInfoValues5.IgnoreCondition = null;
		jsonPropertyInfoValues5.HasJsonInclude = false;
		jsonPropertyInfoValues5.IsExtensionData = false;
		jsonPropertyInfoValues5.NumberHandling = null;
		jsonPropertyInfoValues5.PropertyName = "EventsSeen";
		jsonPropertyInfoValues5.JsonPropertyName = "events_seen";
		jsonPropertyInfoValues5.AttributeProviderFactory = () => typeof(SerializableRun).GetProperty("EventsSeen", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<ModelId>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<ModelId>> propertyInfo6 = jsonPropertyInfoValues5;
		array[5] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo6);
		array[5].IsGetNullable = false;
		array[5].IsSetNullable = false;
		JsonPropertyInfoValues<SerializableRoom> jsonPropertyInfoValues6 = new JsonPropertyInfoValues<SerializableRoom>();
		jsonPropertyInfoValues6.IsProperty = true;
		jsonPropertyInfoValues6.IsPublic = true;
		jsonPropertyInfoValues6.IsVirtual = false;
		jsonPropertyInfoValues6.DeclaringType = typeof(SerializableRun);
		jsonPropertyInfoValues6.Converter = null;
		jsonPropertyInfoValues6.Getter = (object obj) => ((SerializableRun)obj).PreFinishedRoom;
		jsonPropertyInfoValues6.Setter = delegate(object obj, SerializableRoom? value)
		{
			((SerializableRun)obj).PreFinishedRoom = value;
		};
		jsonPropertyInfoValues6.IgnoreCondition = null;
		jsonPropertyInfoValues6.HasJsonInclude = false;
		jsonPropertyInfoValues6.IsExtensionData = false;
		jsonPropertyInfoValues6.NumberHandling = null;
		jsonPropertyInfoValues6.PropertyName = "PreFinishedRoom";
		jsonPropertyInfoValues6.JsonPropertyName = "pre_finished_room";
		jsonPropertyInfoValues6.AttributeProviderFactory = () => typeof(SerializableRun).GetProperty("PreFinishedRoom", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(SerializableRoom), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<SerializableRoom> propertyInfo7 = jsonPropertyInfoValues6;
		array[6] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo7);
		JsonPropertyInfoValues<SerializableRunOddsSet> jsonPropertyInfoValues7 = new JsonPropertyInfoValues<SerializableRunOddsSet>();
		jsonPropertyInfoValues7.IsProperty = true;
		jsonPropertyInfoValues7.IsPublic = true;
		jsonPropertyInfoValues7.IsVirtual = false;
		jsonPropertyInfoValues7.DeclaringType = typeof(SerializableRun);
		jsonPropertyInfoValues7.Converter = null;
		jsonPropertyInfoValues7.Getter = (object obj) => ((SerializableRun)obj).SerializableOdds;
		jsonPropertyInfoValues7.Setter = delegate(object obj, SerializableRunOddsSet? value)
		{
			((SerializableRun)obj).SerializableOdds = value;
		};
		jsonPropertyInfoValues7.IgnoreCondition = null;
		jsonPropertyInfoValues7.HasJsonInclude = false;
		jsonPropertyInfoValues7.IsExtensionData = false;
		jsonPropertyInfoValues7.NumberHandling = null;
		jsonPropertyInfoValues7.PropertyName = "SerializableOdds";
		jsonPropertyInfoValues7.JsonPropertyName = "odds";
		jsonPropertyInfoValues7.AttributeProviderFactory = () => typeof(SerializableRun).GetProperty("SerializableOdds", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(SerializableRunOddsSet), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<SerializableRunOddsSet> propertyInfo8 = jsonPropertyInfoValues7;
		array[7] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo8);
		array[7].IsGetNullable = false;
		array[7].IsSetNullable = false;
		JsonPropertyInfoValues<SerializableRelicGrabBag> jsonPropertyInfoValues8 = new JsonPropertyInfoValues<SerializableRelicGrabBag>();
		jsonPropertyInfoValues8.IsProperty = true;
		jsonPropertyInfoValues8.IsPublic = true;
		jsonPropertyInfoValues8.IsVirtual = false;
		jsonPropertyInfoValues8.DeclaringType = typeof(SerializableRun);
		jsonPropertyInfoValues8.Converter = null;
		jsonPropertyInfoValues8.Getter = (object obj) => ((SerializableRun)obj).SerializableSharedRelicGrabBag;
		jsonPropertyInfoValues8.Setter = delegate(object obj, SerializableRelicGrabBag? value)
		{
			((SerializableRun)obj).SerializableSharedRelicGrabBag = value;
		};
		jsonPropertyInfoValues8.IgnoreCondition = null;
		jsonPropertyInfoValues8.HasJsonInclude = false;
		jsonPropertyInfoValues8.IsExtensionData = false;
		jsonPropertyInfoValues8.NumberHandling = null;
		jsonPropertyInfoValues8.PropertyName = "SerializableSharedRelicGrabBag";
		jsonPropertyInfoValues8.JsonPropertyName = "shared_relic_grab_bag";
		jsonPropertyInfoValues8.AttributeProviderFactory = () => typeof(SerializableRun).GetProperty("SerializableSharedRelicGrabBag", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(SerializableRelicGrabBag), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<SerializableRelicGrabBag> propertyInfo9 = jsonPropertyInfoValues8;
		array[8] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo9);
		array[8].IsGetNullable = false;
		array[8].IsSetNullable = false;
		JsonPropertyInfoValues<List<SerializablePlayer>> jsonPropertyInfoValues9 = new JsonPropertyInfoValues<List<SerializablePlayer>>();
		jsonPropertyInfoValues9.IsProperty = true;
		jsonPropertyInfoValues9.IsPublic = true;
		jsonPropertyInfoValues9.IsVirtual = false;
		jsonPropertyInfoValues9.DeclaringType = typeof(SerializableRun);
		jsonPropertyInfoValues9.Converter = null;
		jsonPropertyInfoValues9.Getter = (object obj) => ((SerializableRun)obj).Players;
		jsonPropertyInfoValues9.Setter = delegate(object obj, List<SerializablePlayer>? value)
		{
			((SerializableRun)obj).Players = value;
		};
		jsonPropertyInfoValues9.IgnoreCondition = null;
		jsonPropertyInfoValues9.HasJsonInclude = false;
		jsonPropertyInfoValues9.IsExtensionData = false;
		jsonPropertyInfoValues9.NumberHandling = null;
		jsonPropertyInfoValues9.PropertyName = "Players";
		jsonPropertyInfoValues9.JsonPropertyName = "players";
		jsonPropertyInfoValues9.AttributeProviderFactory = () => typeof(SerializableRun).GetProperty("Players", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<SerializablePlayer>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<SerializablePlayer>> propertyInfo10 = jsonPropertyInfoValues9;
		array[9] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo10);
		array[9].IsGetNullable = false;
		array[9].IsSetNullable = false;
		JsonPropertyInfoValues<SerializableRunRngSet> jsonPropertyInfoValues10 = new JsonPropertyInfoValues<SerializableRunRngSet>();
		jsonPropertyInfoValues10.IsProperty = true;
		jsonPropertyInfoValues10.IsPublic = true;
		jsonPropertyInfoValues10.IsVirtual = false;
		jsonPropertyInfoValues10.DeclaringType = typeof(SerializableRun);
		jsonPropertyInfoValues10.Converter = null;
		jsonPropertyInfoValues10.Getter = (object obj) => ((SerializableRun)obj).SerializableRng;
		jsonPropertyInfoValues10.Setter = delegate(object obj, SerializableRunRngSet? value)
		{
			((SerializableRun)obj).SerializableRng = value;
		};
		jsonPropertyInfoValues10.IgnoreCondition = null;
		jsonPropertyInfoValues10.HasJsonInclude = false;
		jsonPropertyInfoValues10.IsExtensionData = false;
		jsonPropertyInfoValues10.NumberHandling = null;
		jsonPropertyInfoValues10.PropertyName = "SerializableRng";
		jsonPropertyInfoValues10.JsonPropertyName = "rng";
		jsonPropertyInfoValues10.AttributeProviderFactory = () => typeof(SerializableRun).GetProperty("SerializableRng", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(SerializableRunRngSet), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<SerializableRunRngSet> propertyInfo11 = jsonPropertyInfoValues10;
		array[10] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo11);
		array[10].IsGetNullable = false;
		array[10].IsSetNullable = false;
		JsonPropertyInfoValues<List<MapCoord>> jsonPropertyInfoValues11 = new JsonPropertyInfoValues<List<MapCoord>>();
		jsonPropertyInfoValues11.IsProperty = true;
		jsonPropertyInfoValues11.IsPublic = true;
		jsonPropertyInfoValues11.IsVirtual = false;
		jsonPropertyInfoValues11.DeclaringType = typeof(SerializableRun);
		jsonPropertyInfoValues11.Converter = null;
		jsonPropertyInfoValues11.Getter = (object obj) => ((SerializableRun)obj).VisitedMapCoords;
		jsonPropertyInfoValues11.Setter = delegate(object obj, List<MapCoord>? value)
		{
			((SerializableRun)obj).VisitedMapCoords = value;
		};
		jsonPropertyInfoValues11.IgnoreCondition = null;
		jsonPropertyInfoValues11.HasJsonInclude = false;
		jsonPropertyInfoValues11.IsExtensionData = false;
		jsonPropertyInfoValues11.NumberHandling = null;
		jsonPropertyInfoValues11.PropertyName = "VisitedMapCoords";
		jsonPropertyInfoValues11.JsonPropertyName = "visited_map_coords";
		jsonPropertyInfoValues11.AttributeProviderFactory = () => typeof(SerializableRun).GetProperty("VisitedMapCoords", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<MapCoord>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<MapCoord>> propertyInfo12 = jsonPropertyInfoValues11;
		array[11] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo12);
		array[11].IsGetNullable = false;
		array[11].IsSetNullable = false;
		JsonPropertyInfoValues<List<List<MapPointHistoryEntry>>> jsonPropertyInfoValues12 = new JsonPropertyInfoValues<List<List<MapPointHistoryEntry>>>();
		jsonPropertyInfoValues12.IsProperty = true;
		jsonPropertyInfoValues12.IsPublic = true;
		jsonPropertyInfoValues12.IsVirtual = false;
		jsonPropertyInfoValues12.DeclaringType = typeof(SerializableRun);
		jsonPropertyInfoValues12.Converter = null;
		jsonPropertyInfoValues12.Getter = (object obj) => ((SerializableRun)obj).MapPointHistory;
		jsonPropertyInfoValues12.Setter = delegate(object obj, List<List<MapPointHistoryEntry>>? value)
		{
			((SerializableRun)obj).MapPointHistory = value;
		};
		jsonPropertyInfoValues12.IgnoreCondition = null;
		jsonPropertyInfoValues12.HasJsonInclude = false;
		jsonPropertyInfoValues12.IsExtensionData = false;
		jsonPropertyInfoValues12.NumberHandling = null;
		jsonPropertyInfoValues12.PropertyName = "MapPointHistory";
		jsonPropertyInfoValues12.JsonPropertyName = "map_point_history";
		jsonPropertyInfoValues12.AttributeProviderFactory = () => typeof(SerializableRun).GetProperty("MapPointHistory", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<List<MapPointHistoryEntry>>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<List<MapPointHistoryEntry>>> propertyInfo13 = jsonPropertyInfoValues12;
		array[12] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo13);
		array[12].IsGetNullable = false;
		array[12].IsSetNullable = false;
		JsonPropertyInfoValues<long> jsonPropertyInfoValues13 = new JsonPropertyInfoValues<long>();
		jsonPropertyInfoValues13.IsProperty = true;
		jsonPropertyInfoValues13.IsPublic = true;
		jsonPropertyInfoValues13.IsVirtual = false;
		jsonPropertyInfoValues13.DeclaringType = typeof(SerializableRun);
		jsonPropertyInfoValues13.Converter = null;
		jsonPropertyInfoValues13.Getter = (object obj) => ((SerializableRun)obj).SaveTime;
		jsonPropertyInfoValues13.Setter = delegate(object obj, long value)
		{
			((SerializableRun)obj).SaveTime = value;
		};
		jsonPropertyInfoValues13.IgnoreCondition = null;
		jsonPropertyInfoValues13.HasJsonInclude = false;
		jsonPropertyInfoValues13.IsExtensionData = false;
		jsonPropertyInfoValues13.NumberHandling = null;
		jsonPropertyInfoValues13.PropertyName = "SaveTime";
		jsonPropertyInfoValues13.JsonPropertyName = "save_time";
		jsonPropertyInfoValues13.AttributeProviderFactory = () => typeof(SerializableRun).GetProperty("SaveTime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(long), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<long> propertyInfo14 = jsonPropertyInfoValues13;
		array[13] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo14);
		jsonPropertyInfoValues13 = new JsonPropertyInfoValues<long>();
		jsonPropertyInfoValues13.IsProperty = true;
		jsonPropertyInfoValues13.IsPublic = true;
		jsonPropertyInfoValues13.IsVirtual = false;
		jsonPropertyInfoValues13.DeclaringType = typeof(SerializableRun);
		jsonPropertyInfoValues13.Converter = null;
		jsonPropertyInfoValues13.Getter = (object obj) => ((SerializableRun)obj).StartTime;
		jsonPropertyInfoValues13.Setter = delegate(object obj, long value)
		{
			((SerializableRun)obj).StartTime = value;
		};
		jsonPropertyInfoValues13.IgnoreCondition = null;
		jsonPropertyInfoValues13.HasJsonInclude = false;
		jsonPropertyInfoValues13.IsExtensionData = false;
		jsonPropertyInfoValues13.NumberHandling = null;
		jsonPropertyInfoValues13.PropertyName = "StartTime";
		jsonPropertyInfoValues13.JsonPropertyName = "start_time";
		jsonPropertyInfoValues13.AttributeProviderFactory = () => typeof(SerializableRun).GetProperty("StartTime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(long), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<long> propertyInfo15 = jsonPropertyInfoValues13;
		array[14] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo15);
		jsonPropertyInfoValues13 = new JsonPropertyInfoValues<long>();
		jsonPropertyInfoValues13.IsProperty = true;
		jsonPropertyInfoValues13.IsPublic = true;
		jsonPropertyInfoValues13.IsVirtual = false;
		jsonPropertyInfoValues13.DeclaringType = typeof(SerializableRun);
		jsonPropertyInfoValues13.Converter = null;
		jsonPropertyInfoValues13.Getter = (object obj) => ((SerializableRun)obj).RunTime;
		jsonPropertyInfoValues13.Setter = delegate(object obj, long value)
		{
			((SerializableRun)obj).RunTime = value;
		};
		jsonPropertyInfoValues13.IgnoreCondition = null;
		jsonPropertyInfoValues13.HasJsonInclude = false;
		jsonPropertyInfoValues13.IsExtensionData = false;
		jsonPropertyInfoValues13.NumberHandling = null;
		jsonPropertyInfoValues13.PropertyName = "RunTime";
		jsonPropertyInfoValues13.JsonPropertyName = "run_time";
		jsonPropertyInfoValues13.AttributeProviderFactory = () => typeof(SerializableRun).GetProperty("RunTime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(long), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<long> propertyInfo16 = jsonPropertyInfoValues13;
		array[15] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo16);
		jsonPropertyInfoValues13 = new JsonPropertyInfoValues<long>();
		jsonPropertyInfoValues13.IsProperty = true;
		jsonPropertyInfoValues13.IsPublic = true;
		jsonPropertyInfoValues13.IsVirtual = false;
		jsonPropertyInfoValues13.DeclaringType = typeof(SerializableRun);
		jsonPropertyInfoValues13.Converter = null;
		jsonPropertyInfoValues13.Getter = (object obj) => ((SerializableRun)obj).WinTime;
		jsonPropertyInfoValues13.Setter = delegate(object obj, long value)
		{
			((SerializableRun)obj).WinTime = value;
		};
		jsonPropertyInfoValues13.IgnoreCondition = null;
		jsonPropertyInfoValues13.HasJsonInclude = false;
		jsonPropertyInfoValues13.IsExtensionData = false;
		jsonPropertyInfoValues13.NumberHandling = null;
		jsonPropertyInfoValues13.PropertyName = "WinTime";
		jsonPropertyInfoValues13.JsonPropertyName = "win_time";
		jsonPropertyInfoValues13.AttributeProviderFactory = () => typeof(SerializableRun).GetProperty("WinTime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(long), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<long> propertyInfo17 = jsonPropertyInfoValues13;
		array[16] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo17);
		jsonPropertyInfoValues = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableRun);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableRun)obj).Ascension;
		jsonPropertyInfoValues.Setter = delegate(object obj, int value)
		{
			((SerializableRun)obj).Ascension = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Ascension";
		jsonPropertyInfoValues.JsonPropertyName = "ascension";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableRun).GetProperty("Ascension", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo18 = jsonPropertyInfoValues;
		array[17] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo18);
		JsonPropertyInfoValues<PlatformType> jsonPropertyInfoValues14 = new JsonPropertyInfoValues<PlatformType>();
		jsonPropertyInfoValues14.IsProperty = true;
		jsonPropertyInfoValues14.IsPublic = true;
		jsonPropertyInfoValues14.IsVirtual = false;
		jsonPropertyInfoValues14.DeclaringType = typeof(SerializableRun);
		jsonPropertyInfoValues14.Converter = null;
		jsonPropertyInfoValues14.Getter = (object obj) => ((SerializableRun)obj).PlatformType;
		jsonPropertyInfoValues14.Setter = delegate(object obj, PlatformType value)
		{
			((SerializableRun)obj).PlatformType = value;
		};
		jsonPropertyInfoValues14.IgnoreCondition = null;
		jsonPropertyInfoValues14.HasJsonInclude = false;
		jsonPropertyInfoValues14.IsExtensionData = false;
		jsonPropertyInfoValues14.NumberHandling = null;
		jsonPropertyInfoValues14.PropertyName = "PlatformType";
		jsonPropertyInfoValues14.JsonPropertyName = "platform_type";
		jsonPropertyInfoValues14.AttributeProviderFactory = () => typeof(SerializableRun).GetProperty("PlatformType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(PlatformType), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<PlatformType> propertyInfo19 = jsonPropertyInfoValues14;
		array[18] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo19);
		JsonPropertyInfoValues<SerializableMapDrawings> jsonPropertyInfoValues15 = new JsonPropertyInfoValues<SerializableMapDrawings>();
		jsonPropertyInfoValues15.IsProperty = true;
		jsonPropertyInfoValues15.IsPublic = true;
		jsonPropertyInfoValues15.IsVirtual = false;
		jsonPropertyInfoValues15.DeclaringType = typeof(SerializableRun);
		jsonPropertyInfoValues15.Converter = (JsonConverter<SerializableMapDrawings>)ExpandConverter(typeof(SerializableMapDrawings), new SerializableMapDrawingsJsonConverter(), options);
		jsonPropertyInfoValues15.Getter = (object obj) => ((SerializableRun)obj).MapDrawings;
		jsonPropertyInfoValues15.Setter = delegate(object obj, SerializableMapDrawings? value)
		{
			((SerializableRun)obj).MapDrawings = value;
		};
		jsonPropertyInfoValues15.IgnoreCondition = null;
		jsonPropertyInfoValues15.HasJsonInclude = false;
		jsonPropertyInfoValues15.IsExtensionData = false;
		jsonPropertyInfoValues15.NumberHandling = null;
		jsonPropertyInfoValues15.PropertyName = "MapDrawings";
		jsonPropertyInfoValues15.JsonPropertyName = "map_drawings";
		jsonPropertyInfoValues15.AttributeProviderFactory = () => typeof(SerializableRun).GetProperty("MapDrawings", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(SerializableMapDrawings), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<SerializableMapDrawings> propertyInfo20 = jsonPropertyInfoValues15;
		array[19] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo20);
		JsonPropertyInfoValues<SerializableExtraRunFields> jsonPropertyInfoValues16 = new JsonPropertyInfoValues<SerializableExtraRunFields>();
		jsonPropertyInfoValues16.IsProperty = true;
		jsonPropertyInfoValues16.IsPublic = true;
		jsonPropertyInfoValues16.IsVirtual = false;
		jsonPropertyInfoValues16.DeclaringType = typeof(SerializableRun);
		jsonPropertyInfoValues16.Converter = null;
		jsonPropertyInfoValues16.Getter = (object obj) => ((SerializableRun)obj).ExtraFields;
		jsonPropertyInfoValues16.Setter = delegate(object obj, SerializableExtraRunFields? value)
		{
			((SerializableRun)obj).ExtraFields = value;
		};
		jsonPropertyInfoValues16.IgnoreCondition = null;
		jsonPropertyInfoValues16.HasJsonInclude = false;
		jsonPropertyInfoValues16.IsExtensionData = false;
		jsonPropertyInfoValues16.NumberHandling = null;
		jsonPropertyInfoValues16.PropertyName = "ExtraFields";
		jsonPropertyInfoValues16.JsonPropertyName = "extra_fields";
		jsonPropertyInfoValues16.AttributeProviderFactory = () => typeof(SerializableRun).GetProperty("ExtraFields", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(SerializableExtraRunFields), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<SerializableExtraRunFields> propertyInfo21 = jsonPropertyInfoValues16;
		array[20] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo21);
		array[20].IsGetNullable = false;
		array[20].IsSetNullable = false;
		return array;
	}

	private JsonTypeInfo<SerializableUnlockedAchievement> Create_SerializableUnlockedAchievement(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SerializableUnlockedAchievement> jsonTypeInfo))
		{
			JsonObjectInfoValues<SerializableUnlockedAchievement> jsonObjectInfoValues = new JsonObjectInfoValues<SerializableUnlockedAchievement>();
			jsonObjectInfoValues.ObjectCreator = null;
			jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = (object[] args) => new SerializableUnlockedAchievement
			{
				Achievement = (string)args[0],
				UnlockTime = (long)args[1]
			};
			jsonObjectInfoValues.PropertyMetadataInitializer = (JsonSerializerContext _) => SerializableUnlockedAchievementPropInit(options);
			jsonObjectInfoValues.ConstructorParameterMetadataInitializer = SerializableUnlockedAchievementCtorParamInit;
			jsonObjectInfoValues.ConstructorAttributeProviderFactory = () => typeof(SerializableUnlockedAchievement).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null);
			jsonObjectInfoValues.SerializeHandler = null;
			JsonObjectInfoValues<SerializableUnlockedAchievement> objectInfo = jsonObjectInfoValues;
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SerializableUnlockedAchievementPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableUnlockedAchievement);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableUnlockedAchievement)obj).Achievement;
		jsonPropertyInfoValues.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Achievement";
		jsonPropertyInfoValues.JsonPropertyName = "achievement";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableUnlockedAchievement).GetProperty("Achievement", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		JsonPropertyInfoValues<long> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<long>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SerializableUnlockedAchievement);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SerializableUnlockedAchievement)obj).UnlockTime;
		jsonPropertyInfoValues2.Setter = delegate
		{
			throw new InvalidOperationException("Setting init-only properties is not supported in source generation mode.");
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "UnlockTime";
		jsonPropertyInfoValues2.JsonPropertyName = "unlock_time";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SerializableUnlockedAchievement).GetProperty("UnlockTime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(long), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<long> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		return array;
	}

	private static JsonParameterInfoValues[] SerializableUnlockedAchievementCtorParamInit()
	{
		return new JsonParameterInfoValues[2]
		{
			new JsonParameterInfoValues
			{
				Name = "Achievement",
				ParameterType = typeof(string),
				Position = 0,
				IsNullable = false,
				IsMemberInitializer = true
			},
			new JsonParameterInfoValues
			{
				Name = "UnlockTime",
				ParameterType = typeof(long),
				Position = 1,
				IsNullable = false,
				IsMemberInitializer = true
			}
		};
	}

	private JsonTypeInfo<SettingsSave> Create_SettingsSave(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SettingsSave> jsonTypeInfo))
		{
			JsonObjectInfoValues<SettingsSave> objectInfo = new JsonObjectInfoValues<SettingsSave>
			{
				ObjectCreator = () => new SettingsSave(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SettingsSavePropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SettingsSave).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SettingsSavePropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[23];
		JsonPropertyInfoValues<int> jsonPropertyInfoValues = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SettingsSave);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SettingsSave)obj).SchemaVersion;
		jsonPropertyInfoValues.Setter = delegate(object obj, int value)
		{
			((SettingsSave)obj).SchemaVersion = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "SchemaVersion";
		jsonPropertyInfoValues.JsonPropertyName = "schema_version";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SettingsSave).GetProperty("SchemaVersion", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		jsonPropertyInfoValues = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SettingsSave);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SettingsSave)obj).FpsLimit;
		jsonPropertyInfoValues.Setter = delegate(object obj, int value)
		{
			((SettingsSave)obj).FpsLimit = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "FpsLimit";
		jsonPropertyInfoValues.JsonPropertyName = "fps_limit";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SettingsSave).GetProperty("FpsLimit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo2 = jsonPropertyInfoValues;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<string> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<string>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SettingsSave);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SettingsSave)obj).Language;
		jsonPropertyInfoValues2.Setter = delegate(object obj, string? value)
		{
			((SettingsSave)obj).Language = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "Language";
		jsonPropertyInfoValues2.JsonPropertyName = "language";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SettingsSave).GetProperty("Language", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<string> propertyInfo3 = jsonPropertyInfoValues2;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		JsonPropertyInfoValues<Vector2I> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<Vector2I>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(SettingsSave);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((SettingsSave)obj).WindowPosition;
		jsonPropertyInfoValues3.Setter = delegate(object obj, Vector2I value)
		{
			((SettingsSave)obj).WindowPosition = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "WindowPosition";
		jsonPropertyInfoValues3.JsonPropertyName = "window_position";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(SettingsSave).GetProperty("WindowPosition", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(Vector2I), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<Vector2I> propertyInfo4 = jsonPropertyInfoValues3;
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		jsonPropertyInfoValues3 = new JsonPropertyInfoValues<Vector2I>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(SettingsSave);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((SettingsSave)obj).WindowSize;
		jsonPropertyInfoValues3.Setter = delegate(object obj, Vector2I value)
		{
			((SettingsSave)obj).WindowSize = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "WindowSize";
		jsonPropertyInfoValues3.JsonPropertyName = "window_size";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(SettingsSave).GetProperty("WindowSize", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(Vector2I), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<Vector2I> propertyInfo5 = jsonPropertyInfoValues3;
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		JsonPropertyInfoValues<bool> jsonPropertyInfoValues4 = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues4.IsProperty = true;
		jsonPropertyInfoValues4.IsPublic = true;
		jsonPropertyInfoValues4.IsVirtual = false;
		jsonPropertyInfoValues4.DeclaringType = typeof(SettingsSave);
		jsonPropertyInfoValues4.Converter = null;
		jsonPropertyInfoValues4.Getter = (object obj) => ((SettingsSave)obj).Fullscreen;
		jsonPropertyInfoValues4.Setter = delegate(object obj, bool value)
		{
			((SettingsSave)obj).Fullscreen = value;
		};
		jsonPropertyInfoValues4.IgnoreCondition = null;
		jsonPropertyInfoValues4.HasJsonInclude = false;
		jsonPropertyInfoValues4.IsExtensionData = false;
		jsonPropertyInfoValues4.NumberHandling = null;
		jsonPropertyInfoValues4.PropertyName = "Fullscreen";
		jsonPropertyInfoValues4.JsonPropertyName = "fullscreen";
		jsonPropertyInfoValues4.AttributeProviderFactory = () => typeof(SettingsSave).GetProperty("Fullscreen", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<bool> propertyInfo6 = jsonPropertyInfoValues4;
		array[5] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo6);
		JsonPropertyInfoValues<AspectRatioSetting> jsonPropertyInfoValues5 = new JsonPropertyInfoValues<AspectRatioSetting>();
		jsonPropertyInfoValues5.IsProperty = true;
		jsonPropertyInfoValues5.IsPublic = true;
		jsonPropertyInfoValues5.IsVirtual = false;
		jsonPropertyInfoValues5.DeclaringType = typeof(SettingsSave);
		jsonPropertyInfoValues5.Converter = null;
		jsonPropertyInfoValues5.Getter = (object obj) => ((SettingsSave)obj).AspectRatioSetting;
		jsonPropertyInfoValues5.Setter = delegate(object obj, AspectRatioSetting value)
		{
			((SettingsSave)obj).AspectRatioSetting = value;
		};
		jsonPropertyInfoValues5.IgnoreCondition = null;
		jsonPropertyInfoValues5.HasJsonInclude = false;
		jsonPropertyInfoValues5.IsExtensionData = false;
		jsonPropertyInfoValues5.NumberHandling = null;
		jsonPropertyInfoValues5.PropertyName = "AspectRatioSetting";
		jsonPropertyInfoValues5.JsonPropertyName = "aspect_ratio";
		jsonPropertyInfoValues5.AttributeProviderFactory = () => typeof(SettingsSave).GetProperty("AspectRatioSetting", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(AspectRatioSetting), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<AspectRatioSetting> propertyInfo7 = jsonPropertyInfoValues5;
		array[6] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo7);
		jsonPropertyInfoValues = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SettingsSave);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SettingsSave)obj).TargetDisplay;
		jsonPropertyInfoValues.Setter = delegate(object obj, int value)
		{
			((SettingsSave)obj).TargetDisplay = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "TargetDisplay";
		jsonPropertyInfoValues.JsonPropertyName = "target_display";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SettingsSave).GetProperty("TargetDisplay", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo8 = jsonPropertyInfoValues;
		array[7] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo8);
		jsonPropertyInfoValues4 = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues4.IsProperty = true;
		jsonPropertyInfoValues4.IsPublic = true;
		jsonPropertyInfoValues4.IsVirtual = false;
		jsonPropertyInfoValues4.DeclaringType = typeof(SettingsSave);
		jsonPropertyInfoValues4.Converter = null;
		jsonPropertyInfoValues4.Getter = (object obj) => ((SettingsSave)obj).ResizeWindows;
		jsonPropertyInfoValues4.Setter = delegate(object obj, bool value)
		{
			((SettingsSave)obj).ResizeWindows = value;
		};
		jsonPropertyInfoValues4.IgnoreCondition = null;
		jsonPropertyInfoValues4.HasJsonInclude = false;
		jsonPropertyInfoValues4.IsExtensionData = false;
		jsonPropertyInfoValues4.NumberHandling = null;
		jsonPropertyInfoValues4.PropertyName = "ResizeWindows";
		jsonPropertyInfoValues4.JsonPropertyName = "resize_windows";
		jsonPropertyInfoValues4.AttributeProviderFactory = () => typeof(SettingsSave).GetProperty("ResizeWindows", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<bool> propertyInfo9 = jsonPropertyInfoValues4;
		array[8] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo9);
		JsonPropertyInfoValues<VSyncType> jsonPropertyInfoValues6 = new JsonPropertyInfoValues<VSyncType>();
		jsonPropertyInfoValues6.IsProperty = true;
		jsonPropertyInfoValues6.IsPublic = true;
		jsonPropertyInfoValues6.IsVirtual = false;
		jsonPropertyInfoValues6.DeclaringType = typeof(SettingsSave);
		jsonPropertyInfoValues6.Converter = null;
		jsonPropertyInfoValues6.Getter = (object obj) => ((SettingsSave)obj).VSync;
		jsonPropertyInfoValues6.Setter = delegate(object obj, VSyncType value)
		{
			((SettingsSave)obj).VSync = value;
		};
		jsonPropertyInfoValues6.IgnoreCondition = null;
		jsonPropertyInfoValues6.HasJsonInclude = false;
		jsonPropertyInfoValues6.IsExtensionData = false;
		jsonPropertyInfoValues6.NumberHandling = null;
		jsonPropertyInfoValues6.PropertyName = "VSync";
		jsonPropertyInfoValues6.JsonPropertyName = "vsync";
		jsonPropertyInfoValues6.AttributeProviderFactory = () => typeof(SettingsSave).GetProperty("VSync", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(VSyncType), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<VSyncType> propertyInfo10 = jsonPropertyInfoValues6;
		array[9] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo10);
		jsonPropertyInfoValues = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SettingsSave);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SettingsSave)obj).Msaa;
		jsonPropertyInfoValues.Setter = delegate(object obj, int value)
		{
			((SettingsSave)obj).Msaa = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "Msaa";
		jsonPropertyInfoValues.JsonPropertyName = "msaa";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SettingsSave).GetProperty("Msaa", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo11 = jsonPropertyInfoValues;
		array[10] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo11);
		JsonPropertyInfoValues<float> jsonPropertyInfoValues7 = new JsonPropertyInfoValues<float>();
		jsonPropertyInfoValues7.IsProperty = true;
		jsonPropertyInfoValues7.IsPublic = true;
		jsonPropertyInfoValues7.IsVirtual = false;
		jsonPropertyInfoValues7.DeclaringType = typeof(SettingsSave);
		jsonPropertyInfoValues7.Converter = null;
		jsonPropertyInfoValues7.Getter = (object obj) => ((SettingsSave)obj).VolumeBgm;
		jsonPropertyInfoValues7.Setter = delegate(object obj, float value)
		{
			((SettingsSave)obj).VolumeBgm = value;
		};
		jsonPropertyInfoValues7.IgnoreCondition = null;
		jsonPropertyInfoValues7.HasJsonInclude = false;
		jsonPropertyInfoValues7.IsExtensionData = false;
		jsonPropertyInfoValues7.NumberHandling = null;
		jsonPropertyInfoValues7.PropertyName = "VolumeBgm";
		jsonPropertyInfoValues7.JsonPropertyName = "volume_bgm";
		jsonPropertyInfoValues7.AttributeProviderFactory = () => typeof(SettingsSave).GetProperty("VolumeBgm", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(float), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<float> propertyInfo12 = jsonPropertyInfoValues7;
		array[11] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo12);
		jsonPropertyInfoValues7 = new JsonPropertyInfoValues<float>();
		jsonPropertyInfoValues7.IsProperty = true;
		jsonPropertyInfoValues7.IsPublic = true;
		jsonPropertyInfoValues7.IsVirtual = false;
		jsonPropertyInfoValues7.DeclaringType = typeof(SettingsSave);
		jsonPropertyInfoValues7.Converter = null;
		jsonPropertyInfoValues7.Getter = (object obj) => ((SettingsSave)obj).VolumeMaster;
		jsonPropertyInfoValues7.Setter = delegate(object obj, float value)
		{
			((SettingsSave)obj).VolumeMaster = value;
		};
		jsonPropertyInfoValues7.IgnoreCondition = null;
		jsonPropertyInfoValues7.HasJsonInclude = false;
		jsonPropertyInfoValues7.IsExtensionData = false;
		jsonPropertyInfoValues7.NumberHandling = null;
		jsonPropertyInfoValues7.PropertyName = "VolumeMaster";
		jsonPropertyInfoValues7.JsonPropertyName = "volume_master";
		jsonPropertyInfoValues7.AttributeProviderFactory = () => typeof(SettingsSave).GetProperty("VolumeMaster", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(float), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<float> propertyInfo13 = jsonPropertyInfoValues7;
		array[12] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo13);
		jsonPropertyInfoValues7 = new JsonPropertyInfoValues<float>();
		jsonPropertyInfoValues7.IsProperty = true;
		jsonPropertyInfoValues7.IsPublic = true;
		jsonPropertyInfoValues7.IsVirtual = false;
		jsonPropertyInfoValues7.DeclaringType = typeof(SettingsSave);
		jsonPropertyInfoValues7.Converter = null;
		jsonPropertyInfoValues7.Getter = (object obj) => ((SettingsSave)obj).VolumeSfx;
		jsonPropertyInfoValues7.Setter = delegate(object obj, float value)
		{
			((SettingsSave)obj).VolumeSfx = value;
		};
		jsonPropertyInfoValues7.IgnoreCondition = null;
		jsonPropertyInfoValues7.HasJsonInclude = false;
		jsonPropertyInfoValues7.IsExtensionData = false;
		jsonPropertyInfoValues7.NumberHandling = null;
		jsonPropertyInfoValues7.PropertyName = "VolumeSfx";
		jsonPropertyInfoValues7.JsonPropertyName = "volume_sfx";
		jsonPropertyInfoValues7.AttributeProviderFactory = () => typeof(SettingsSave).GetProperty("VolumeSfx", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(float), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<float> propertyInfo14 = jsonPropertyInfoValues7;
		array[13] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo14);
		jsonPropertyInfoValues7 = new JsonPropertyInfoValues<float>();
		jsonPropertyInfoValues7.IsProperty = true;
		jsonPropertyInfoValues7.IsPublic = true;
		jsonPropertyInfoValues7.IsVirtual = false;
		jsonPropertyInfoValues7.DeclaringType = typeof(SettingsSave);
		jsonPropertyInfoValues7.Converter = null;
		jsonPropertyInfoValues7.Getter = (object obj) => ((SettingsSave)obj).VolumeAmbience;
		jsonPropertyInfoValues7.Setter = delegate(object obj, float value)
		{
			((SettingsSave)obj).VolumeAmbience = value;
		};
		jsonPropertyInfoValues7.IgnoreCondition = null;
		jsonPropertyInfoValues7.HasJsonInclude = false;
		jsonPropertyInfoValues7.IsExtensionData = false;
		jsonPropertyInfoValues7.NumberHandling = null;
		jsonPropertyInfoValues7.PropertyName = "VolumeAmbience";
		jsonPropertyInfoValues7.JsonPropertyName = "volume_ambience";
		jsonPropertyInfoValues7.AttributeProviderFactory = () => typeof(SettingsSave).GetProperty("VolumeAmbience", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(float), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<float> propertyInfo15 = jsonPropertyInfoValues7;
		array[14] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo15);
		JsonPropertyInfoValues<ModSettings> jsonPropertyInfoValues8 = new JsonPropertyInfoValues<ModSettings>();
		jsonPropertyInfoValues8.IsProperty = true;
		jsonPropertyInfoValues8.IsPublic = true;
		jsonPropertyInfoValues8.IsVirtual = false;
		jsonPropertyInfoValues8.DeclaringType = typeof(SettingsSave);
		jsonPropertyInfoValues8.Converter = null;
		jsonPropertyInfoValues8.Getter = (object obj) => ((SettingsSave)obj).ModSettings;
		jsonPropertyInfoValues8.Setter = delegate(object obj, ModSettings? value)
		{
			((SettingsSave)obj).ModSettings = value;
		};
		jsonPropertyInfoValues8.IgnoreCondition = null;
		jsonPropertyInfoValues8.HasJsonInclude = false;
		jsonPropertyInfoValues8.IsExtensionData = false;
		jsonPropertyInfoValues8.NumberHandling = null;
		jsonPropertyInfoValues8.PropertyName = "ModSettings";
		jsonPropertyInfoValues8.JsonPropertyName = "mod_settings";
		jsonPropertyInfoValues8.AttributeProviderFactory = () => typeof(SettingsSave).GetProperty("ModSettings", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ModSettings), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ModSettings> propertyInfo16 = jsonPropertyInfoValues8;
		array[15] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo16);
		jsonPropertyInfoValues4 = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues4.IsProperty = true;
		jsonPropertyInfoValues4.IsPublic = true;
		jsonPropertyInfoValues4.IsVirtual = false;
		jsonPropertyInfoValues4.DeclaringType = typeof(SettingsSave);
		jsonPropertyInfoValues4.Converter = null;
		jsonPropertyInfoValues4.Getter = (object obj) => ((SettingsSave)obj).SkipIntroLogo;
		jsonPropertyInfoValues4.Setter = delegate(object obj, bool value)
		{
			((SettingsSave)obj).SkipIntroLogo = value;
		};
		jsonPropertyInfoValues4.IgnoreCondition = null;
		jsonPropertyInfoValues4.HasJsonInclude = false;
		jsonPropertyInfoValues4.IsExtensionData = false;
		jsonPropertyInfoValues4.NumberHandling = null;
		jsonPropertyInfoValues4.PropertyName = "SkipIntroLogo";
		jsonPropertyInfoValues4.JsonPropertyName = "skip_intro_logo";
		jsonPropertyInfoValues4.AttributeProviderFactory = () => typeof(SettingsSave).GetProperty("SkipIntroLogo", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<bool> propertyInfo17 = jsonPropertyInfoValues4;
		array[16] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo17);
		JsonPropertyInfoValues<Dictionary<string, string>> jsonPropertyInfoValues9 = new JsonPropertyInfoValues<Dictionary<string, string>>();
		jsonPropertyInfoValues9.IsProperty = true;
		jsonPropertyInfoValues9.IsPublic = true;
		jsonPropertyInfoValues9.IsVirtual = false;
		jsonPropertyInfoValues9.DeclaringType = typeof(SettingsSave);
		jsonPropertyInfoValues9.Converter = null;
		jsonPropertyInfoValues9.Getter = (object obj) => ((SettingsSave)obj).KeyboardMapping;
		jsonPropertyInfoValues9.Setter = delegate(object obj, Dictionary<string, string>? value)
		{
			((SettingsSave)obj).KeyboardMapping = value;
		};
		jsonPropertyInfoValues9.IgnoreCondition = null;
		jsonPropertyInfoValues9.HasJsonInclude = false;
		jsonPropertyInfoValues9.IsExtensionData = false;
		jsonPropertyInfoValues9.NumberHandling = null;
		jsonPropertyInfoValues9.PropertyName = "KeyboardMapping";
		jsonPropertyInfoValues9.JsonPropertyName = "keyboard_mapping";
		jsonPropertyInfoValues9.AttributeProviderFactory = () => typeof(SettingsSave).GetProperty("KeyboardMapping", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(Dictionary<string, string>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<Dictionary<string, string>> propertyInfo18 = jsonPropertyInfoValues9;
		array[17] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo18);
		array[17].IsGetNullable = false;
		array[17].IsSetNullable = false;
		JsonPropertyInfoValues<ControllerMappingType> jsonPropertyInfoValues10 = new JsonPropertyInfoValues<ControllerMappingType>();
		jsonPropertyInfoValues10.IsProperty = true;
		jsonPropertyInfoValues10.IsPublic = true;
		jsonPropertyInfoValues10.IsVirtual = false;
		jsonPropertyInfoValues10.DeclaringType = typeof(SettingsSave);
		jsonPropertyInfoValues10.Converter = null;
		jsonPropertyInfoValues10.Getter = (object obj) => ((SettingsSave)obj).ControllerMappingType;
		jsonPropertyInfoValues10.Setter = delegate(object obj, ControllerMappingType value)
		{
			((SettingsSave)obj).ControllerMappingType = value;
		};
		jsonPropertyInfoValues10.IgnoreCondition = null;
		jsonPropertyInfoValues10.HasJsonInclude = false;
		jsonPropertyInfoValues10.IsExtensionData = false;
		jsonPropertyInfoValues10.NumberHandling = null;
		jsonPropertyInfoValues10.PropertyName = "ControllerMappingType";
		jsonPropertyInfoValues10.JsonPropertyName = "controller_mapping_type";
		jsonPropertyInfoValues10.AttributeProviderFactory = () => typeof(SettingsSave).GetProperty("ControllerMappingType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ControllerMappingType), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<ControllerMappingType> propertyInfo19 = jsonPropertyInfoValues10;
		array[18] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo19);
		jsonPropertyInfoValues9 = new JsonPropertyInfoValues<Dictionary<string, string>>();
		jsonPropertyInfoValues9.IsProperty = true;
		jsonPropertyInfoValues9.IsPublic = true;
		jsonPropertyInfoValues9.IsVirtual = false;
		jsonPropertyInfoValues9.DeclaringType = typeof(SettingsSave);
		jsonPropertyInfoValues9.Converter = null;
		jsonPropertyInfoValues9.Getter = (object obj) => ((SettingsSave)obj).ControllerMapping;
		jsonPropertyInfoValues9.Setter = delegate(object obj, Dictionary<string, string>? value)
		{
			((SettingsSave)obj).ControllerMapping = value;
		};
		jsonPropertyInfoValues9.IgnoreCondition = null;
		jsonPropertyInfoValues9.HasJsonInclude = false;
		jsonPropertyInfoValues9.IsExtensionData = false;
		jsonPropertyInfoValues9.NumberHandling = null;
		jsonPropertyInfoValues9.PropertyName = "ControllerMapping";
		jsonPropertyInfoValues9.JsonPropertyName = "controller_mapping";
		jsonPropertyInfoValues9.AttributeProviderFactory = () => typeof(SettingsSave).GetProperty("ControllerMapping", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(Dictionary<string, string>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<Dictionary<string, string>> propertyInfo20 = jsonPropertyInfoValues9;
		array[19] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo20);
		array[19].IsGetNullable = false;
		array[19].IsSetNullable = false;
		jsonPropertyInfoValues4 = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues4.IsProperty = true;
		jsonPropertyInfoValues4.IsPublic = true;
		jsonPropertyInfoValues4.IsVirtual = false;
		jsonPropertyInfoValues4.DeclaringType = typeof(SettingsSave);
		jsonPropertyInfoValues4.Converter = null;
		jsonPropertyInfoValues4.Getter = (object obj) => ((SettingsSave)obj).LimitFpsInBackground;
		jsonPropertyInfoValues4.Setter = delegate(object obj, bool value)
		{
			((SettingsSave)obj).LimitFpsInBackground = value;
		};
		jsonPropertyInfoValues4.IgnoreCondition = null;
		jsonPropertyInfoValues4.HasJsonInclude = false;
		jsonPropertyInfoValues4.IsExtensionData = false;
		jsonPropertyInfoValues4.NumberHandling = null;
		jsonPropertyInfoValues4.PropertyName = "LimitFpsInBackground";
		jsonPropertyInfoValues4.JsonPropertyName = "limit_fps_in_background";
		jsonPropertyInfoValues4.AttributeProviderFactory = () => typeof(SettingsSave).GetProperty("LimitFpsInBackground", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<bool> propertyInfo21 = jsonPropertyInfoValues4;
		array[20] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo21);
		jsonPropertyInfoValues4 = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues4.IsProperty = true;
		jsonPropertyInfoValues4.IsPublic = true;
		jsonPropertyInfoValues4.IsVirtual = false;
		jsonPropertyInfoValues4.DeclaringType = typeof(SettingsSave);
		jsonPropertyInfoValues4.Converter = null;
		jsonPropertyInfoValues4.Getter = (object obj) => ((SettingsSave)obj).FullConsole;
		jsonPropertyInfoValues4.Setter = delegate(object obj, bool value)
		{
			((SettingsSave)obj).FullConsole = value;
		};
		jsonPropertyInfoValues4.IgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
		jsonPropertyInfoValues4.HasJsonInclude = false;
		jsonPropertyInfoValues4.IsExtensionData = false;
		jsonPropertyInfoValues4.NumberHandling = null;
		jsonPropertyInfoValues4.PropertyName = "FullConsole";
		jsonPropertyInfoValues4.JsonPropertyName = "full_console";
		jsonPropertyInfoValues4.AttributeProviderFactory = () => typeof(SettingsSave).GetProperty("FullConsole", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<bool> propertyInfo22 = jsonPropertyInfoValues4;
		array[21] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo22);
		jsonPropertyInfoValues4 = new JsonPropertyInfoValues<bool>();
		jsonPropertyInfoValues4.IsProperty = true;
		jsonPropertyInfoValues4.IsPublic = true;
		jsonPropertyInfoValues4.IsVirtual = false;
		jsonPropertyInfoValues4.DeclaringType = typeof(SettingsSave);
		jsonPropertyInfoValues4.Converter = null;
		jsonPropertyInfoValues4.Getter = (object obj) => ((SettingsSave)obj).SeenEaDisclaimer;
		jsonPropertyInfoValues4.Setter = delegate(object obj, bool value)
		{
			((SettingsSave)obj).SeenEaDisclaimer = value;
		};
		jsonPropertyInfoValues4.IgnoreCondition = null;
		jsonPropertyInfoValues4.HasJsonInclude = false;
		jsonPropertyInfoValues4.IsExtensionData = false;
		jsonPropertyInfoValues4.NumberHandling = null;
		jsonPropertyInfoValues4.PropertyName = "SeenEaDisclaimer";
		jsonPropertyInfoValues4.JsonPropertyName = "seen_ea_disclaimer";
		jsonPropertyInfoValues4.AttributeProviderFactory = () => typeof(SettingsSave).GetProperty("SeenEaDisclaimer", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<bool> propertyInfo23 = jsonPropertyInfoValues4;
		array[22] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo23);
		return array;
	}

	private JsonTypeInfo<AspectRatioSetting> Create_AspectRatioSetting(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<AspectRatioSetting> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<AspectRatioSetting>(options, JsonMetadataServices.GetEnumConverter<AspectRatioSetting>(options));
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<FastModeType> Create_FastModeType(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<FastModeType> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<FastModeType>(options, JsonMetadataServices.GetEnumConverter<FastModeType>(options));
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<VSyncType> Create_VSyncType(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<VSyncType> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<VSyncType>(options, JsonMetadataServices.GetEnumConverter<VSyncType>(options));
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<SerializableUnlockState> Create_SerializableUnlockState(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SerializableUnlockState> jsonTypeInfo))
		{
			JsonObjectInfoValues<SerializableUnlockState> objectInfo = new JsonObjectInfoValues<SerializableUnlockState>
			{
				ObjectCreator = () => new SerializableUnlockState(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SerializableUnlockStatePropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SerializableUnlockState).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SerializableUnlockStatePropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[3];
		JsonPropertyInfoValues<List<string>> jsonPropertyInfoValues = new JsonPropertyInfoValues<List<string>>();
		jsonPropertyInfoValues.IsProperty = true;
		jsonPropertyInfoValues.IsPublic = true;
		jsonPropertyInfoValues.IsVirtual = false;
		jsonPropertyInfoValues.DeclaringType = typeof(SerializableUnlockState);
		jsonPropertyInfoValues.Converter = null;
		jsonPropertyInfoValues.Getter = (object obj) => ((SerializableUnlockState)obj).UnlockedEpochs;
		jsonPropertyInfoValues.Setter = delegate(object obj, List<string>? value)
		{
			((SerializableUnlockState)obj).UnlockedEpochs = value;
		};
		jsonPropertyInfoValues.IgnoreCondition = null;
		jsonPropertyInfoValues.HasJsonInclude = false;
		jsonPropertyInfoValues.IsExtensionData = false;
		jsonPropertyInfoValues.NumberHandling = null;
		jsonPropertyInfoValues.PropertyName = "UnlockedEpochs";
		jsonPropertyInfoValues.JsonPropertyName = "unlocked_epochs";
		jsonPropertyInfoValues.AttributeProviderFactory = () => typeof(SerializableUnlockState).GetProperty("UnlockedEpochs", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<string>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<string>> propertyInfo = jsonPropertyInfoValues;
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		JsonPropertyInfoValues<List<ModelId>> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<List<ModelId>>();
		jsonPropertyInfoValues2.IsProperty = true;
		jsonPropertyInfoValues2.IsPublic = true;
		jsonPropertyInfoValues2.IsVirtual = false;
		jsonPropertyInfoValues2.DeclaringType = typeof(SerializableUnlockState);
		jsonPropertyInfoValues2.Converter = null;
		jsonPropertyInfoValues2.Getter = (object obj) => ((SerializableUnlockState)obj).EncountersSeen;
		jsonPropertyInfoValues2.Setter = delegate(object obj, List<ModelId>? value)
		{
			((SerializableUnlockState)obj).EncountersSeen = value;
		};
		jsonPropertyInfoValues2.IgnoreCondition = null;
		jsonPropertyInfoValues2.HasJsonInclude = false;
		jsonPropertyInfoValues2.IsExtensionData = false;
		jsonPropertyInfoValues2.NumberHandling = null;
		jsonPropertyInfoValues2.PropertyName = "EncountersSeen";
		jsonPropertyInfoValues2.JsonPropertyName = "encounters_seen";
		jsonPropertyInfoValues2.AttributeProviderFactory = () => typeof(SerializableUnlockState).GetProperty("EncountersSeen", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<ModelId>), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<List<ModelId>> propertyInfo2 = jsonPropertyInfoValues2;
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		JsonPropertyInfoValues<int> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<int>();
		jsonPropertyInfoValues3.IsProperty = true;
		jsonPropertyInfoValues3.IsPublic = true;
		jsonPropertyInfoValues3.IsVirtual = false;
		jsonPropertyInfoValues3.DeclaringType = typeof(SerializableUnlockState);
		jsonPropertyInfoValues3.Converter = null;
		jsonPropertyInfoValues3.Getter = (object obj) => ((SerializableUnlockState)obj).NumberOfRuns;
		jsonPropertyInfoValues3.Setter = delegate(object obj, int value)
		{
			((SerializableUnlockState)obj).NumberOfRuns = value;
		};
		jsonPropertyInfoValues3.IgnoreCondition = null;
		jsonPropertyInfoValues3.HasJsonInclude = false;
		jsonPropertyInfoValues3.IsExtensionData = false;
		jsonPropertyInfoValues3.NumberHandling = null;
		jsonPropertyInfoValues3.PropertyName = "NumberOfRuns";
		jsonPropertyInfoValues3.JsonPropertyName = "number_of_runs";
		jsonPropertyInfoValues3.AttributeProviderFactory = () => typeof(SerializableUnlockState).GetProperty("NumberOfRuns", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null);
		JsonPropertyInfoValues<int> propertyInfo3 = jsonPropertyInfoValues3;
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		return array;
	}

	private JsonTypeInfo<Dictionary<RelicRarity, List<ModelId>>> Create_DictionaryRelicRarityListModelId(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<Dictionary<RelicRarity, List<ModelId>>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<Dictionary<RelicRarity, List<ModelId>>> collectionInfo = new JsonCollectionInfoValues<Dictionary<RelicRarity, List<ModelId>>>
			{
				ObjectCreator = () => new Dictionary<RelicRarity, List<ModelId>>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateDictionaryInfo<Dictionary<RelicRarity, List<ModelId>>, RelicRarity, List<ModelId>>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<Dictionary<PlayerRngType, int>> Create_DictionaryPlayerRngTypeInt32(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<Dictionary<PlayerRngType, int>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<Dictionary<PlayerRngType, int>> collectionInfo = new JsonCollectionInfoValues<Dictionary<PlayerRngType, int>>
			{
				ObjectCreator = () => new Dictionary<PlayerRngType, int>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateDictionaryInfo<Dictionary<PlayerRngType, int>, PlayerRngType, int>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<Dictionary<RunRngType, int>> Create_DictionaryRunRngTypeInt32(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<Dictionary<RunRngType, int>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<Dictionary<RunRngType, int>> collectionInfo = new JsonCollectionInfoValues<Dictionary<RunRngType, int>>
			{
				ObjectCreator = () => new Dictionary<RunRngType, int>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateDictionaryInfo<Dictionary<RunRngType, int>, RunRngType, int>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<Dictionary<string, object>> Create_DictionaryStringObject(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<Dictionary<string, object>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<Dictionary<string, object>> collectionInfo = new JsonCollectionInfoValues<Dictionary<string, object>>
			{
				ObjectCreator = () => new Dictionary<string, object>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateDictionaryInfo<Dictionary<string, object>, string, object>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<Dictionary<string, string>> Create_DictionaryStringString(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<Dictionary<string, string>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<Dictionary<string, string>> collectionInfo = new JsonCollectionInfoValues<Dictionary<string, string>>
			{
				ObjectCreator = () => new Dictionary<string, string>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateDictionaryInfo<Dictionary<string, string>, string, string>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<Dictionary<ulong, List<SerializableReward>>> Create_DictionaryUInt64ListSerializableReward(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<Dictionary<ulong, List<SerializableReward>>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<Dictionary<ulong, List<SerializableReward>>> collectionInfo = new JsonCollectionInfoValues<Dictionary<ulong, List<SerializableReward>>>
			{
				ObjectCreator = () => new Dictionary<ulong, List<SerializableReward>>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateDictionaryInfo<Dictionary<ulong, List<SerializableReward>>, ulong, List<SerializableReward>>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<IEnumerable<SerializableCard>> Create_IEnumerableSerializableCard(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<IEnumerable<SerializableCard>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<IEnumerable<SerializableCard>> collectionInfo = new JsonCollectionInfoValues<IEnumerable<SerializableCard>>
			{
				ObjectCreator = null,
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateIEnumerableInfo<IEnumerable<SerializableCard>, SerializableCard>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<IEnumerable<SerializablePotion>> Create_IEnumerableSerializablePotion(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<IEnumerable<SerializablePotion>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<IEnumerable<SerializablePotion>> collectionInfo = new JsonCollectionInfoValues<IEnumerable<SerializablePotion>>
			{
				ObjectCreator = null,
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateIEnumerableInfo<IEnumerable<SerializablePotion>, SerializablePotion>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<IEnumerable<SerializableRelic>> Create_IEnumerableSerializableRelic(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<IEnumerable<SerializableRelic>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<IEnumerable<SerializableRelic>> collectionInfo = new JsonCollectionInfoValues<IEnumerable<SerializableRelic>>
			{
				ObjectCreator = null,
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateIEnumerableInfo<IEnumerable<SerializableRelic>, SerializableRelic>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<Vector2>> Create_ListVector2(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<Vector2>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<Vector2>> collectionInfo = new JsonCollectionInfoValues<List<Vector2>>
			{
				ObjectCreator = () => new List<Vector2>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<Vector2>, Vector2>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<MapCoord>> Create_ListMapCoord(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<MapCoord>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<MapCoord>> collectionInfo = new JsonCollectionInfoValues<List<MapCoord>>
			{
				ObjectCreator = () => new List<MapCoord>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<MapCoord>, MapCoord>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<DisabledMod>> Create_ListDisabledMod(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<DisabledMod>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<DisabledMod>> collectionInfo = new JsonCollectionInfoValues<List<DisabledMod>>
			{
				ObjectCreator = () => new List<DisabledMod>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<DisabledMod>, DisabledMod>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<ModelId>> Create_ListModelId(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<ModelId>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<ModelId>> collectionInfo = new JsonCollectionInfoValues<List<ModelId>>
			{
				ObjectCreator = () => new List<ModelId>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<ModelId>, ModelId>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<NullLeaderboard>> Create_ListNullLeaderboard(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<NullLeaderboard>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<NullLeaderboard>> collectionInfo = new JsonCollectionInfoValues<List<NullLeaderboard>>
			{
				ObjectCreator = () => new List<NullLeaderboard>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<NullLeaderboard>, NullLeaderboard>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<NullLeaderboardFileEntry>> Create_ListNullLeaderboardFileEntry(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<NullLeaderboardFileEntry>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<NullLeaderboardFileEntry>> collectionInfo = new JsonCollectionInfoValues<List<NullLeaderboardFileEntry>>
			{
				ObjectCreator = () => new List<NullLeaderboardFileEntry>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<NullLeaderboardFileEntry>, NullLeaderboardFileEntry>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<NullMultiplayerName>> Create_ListNullMultiplayerName(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<NullMultiplayerName>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<NullMultiplayerName>> collectionInfo = new JsonCollectionInfoValues<List<NullMultiplayerName>>
			{
				ObjectCreator = () => new List<NullMultiplayerName>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<NullMultiplayerName>, NullMultiplayerName>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<AncientChoiceHistoryEntry>> Create_ListAncientChoiceHistoryEntry(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<AncientChoiceHistoryEntry>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<AncientChoiceHistoryEntry>> collectionInfo = new JsonCollectionInfoValues<List<AncientChoiceHistoryEntry>>
			{
				ObjectCreator = () => new List<AncientChoiceHistoryEntry>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<AncientChoiceHistoryEntry>, AncientChoiceHistoryEntry>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<CardChoiceHistoryEntry>> Create_ListCardChoiceHistoryEntry(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<CardChoiceHistoryEntry>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<CardChoiceHistoryEntry>> collectionInfo = new JsonCollectionInfoValues<List<CardChoiceHistoryEntry>>
			{
				ObjectCreator = () => new List<CardChoiceHistoryEntry>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<CardChoiceHistoryEntry>, CardChoiceHistoryEntry>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<CardEnchantmentHistoryEntry>> Create_ListCardEnchantmentHistoryEntry(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<CardEnchantmentHistoryEntry>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<CardEnchantmentHistoryEntry>> collectionInfo = new JsonCollectionInfoValues<List<CardEnchantmentHistoryEntry>>
			{
				ObjectCreator = () => new List<CardEnchantmentHistoryEntry>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<CardEnchantmentHistoryEntry>, CardEnchantmentHistoryEntry>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<CardTransformationHistoryEntry>> Create_ListCardTransformationHistoryEntry(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<CardTransformationHistoryEntry>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<CardTransformationHistoryEntry>> collectionInfo = new JsonCollectionInfoValues<List<CardTransformationHistoryEntry>>
			{
				ObjectCreator = () => new List<CardTransformationHistoryEntry>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<CardTransformationHistoryEntry>, CardTransformationHistoryEntry>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<EventOptionHistoryEntry>> Create_ListEventOptionHistoryEntry(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<EventOptionHistoryEntry>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<EventOptionHistoryEntry>> collectionInfo = new JsonCollectionInfoValues<List<EventOptionHistoryEntry>>
			{
				ObjectCreator = () => new List<EventOptionHistoryEntry>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<EventOptionHistoryEntry>, EventOptionHistoryEntry>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<MapPointHistoryEntry>> Create_ListMapPointHistoryEntry(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<MapPointHistoryEntry>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<MapPointHistoryEntry>> collectionInfo = new JsonCollectionInfoValues<List<MapPointHistoryEntry>>
			{
				ObjectCreator = () => new List<MapPointHistoryEntry>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<MapPointHistoryEntry>, MapPointHistoryEntry>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<MapPointRoomHistoryEntry>> Create_ListMapPointRoomHistoryEntry(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<MapPointRoomHistoryEntry>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<MapPointRoomHistoryEntry>> collectionInfo = new JsonCollectionInfoValues<List<MapPointRoomHistoryEntry>>
			{
				ObjectCreator = () => new List<MapPointRoomHistoryEntry>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<MapPointRoomHistoryEntry>, MapPointRoomHistoryEntry>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<ModelChoiceHistoryEntry>> Create_ListModelChoiceHistoryEntry(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<ModelChoiceHistoryEntry>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<ModelChoiceHistoryEntry>> collectionInfo = new JsonCollectionInfoValues<List<ModelChoiceHistoryEntry>>
			{
				ObjectCreator = () => new List<ModelChoiceHistoryEntry>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<ModelChoiceHistoryEntry>, ModelChoiceHistoryEntry>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<PlayerMapPointHistoryEntry>> Create_ListPlayerMapPointHistoryEntry(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<PlayerMapPointHistoryEntry>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<PlayerMapPointHistoryEntry>> collectionInfo = new JsonCollectionInfoValues<List<PlayerMapPointHistoryEntry>>
			{
				ObjectCreator = () => new List<PlayerMapPointHistoryEntry>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<PlayerMapPointHistoryEntry>, PlayerMapPointHistoryEntry>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<RunHistoryPlayer>> Create_ListRunHistoryPlayer(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<RunHistoryPlayer>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<RunHistoryPlayer>> collectionInfo = new JsonCollectionInfoValues<List<RunHistoryPlayer>>
			{
				ObjectCreator = () => new List<RunHistoryPlayer>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<RunHistoryPlayer>, RunHistoryPlayer>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<AncientCharacterStats>> Create_ListAncientCharacterStats(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<AncientCharacterStats>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<AncientCharacterStats>> collectionInfo = new JsonCollectionInfoValues<List<AncientCharacterStats>>
			{
				ObjectCreator = () => new List<AncientCharacterStats>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<AncientCharacterStats>, AncientCharacterStats>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<AncientStats>> Create_ListAncientStats(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<AncientStats>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<AncientStats>> collectionInfo = new JsonCollectionInfoValues<List<AncientStats>>
			{
				ObjectCreator = () => new List<AncientStats>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<AncientStats>, AncientStats>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<CardStats>> Create_ListCardStats(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<CardStats>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<CardStats>> collectionInfo = new JsonCollectionInfoValues<List<CardStats>>
			{
				ObjectCreator = () => new List<CardStats>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<CardStats>, CardStats>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<CharacterStats>> Create_ListCharacterStats(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<CharacterStats>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<CharacterStats>> collectionInfo = new JsonCollectionInfoValues<List<CharacterStats>>
			{
				ObjectCreator = () => new List<CharacterStats>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<CharacterStats>, CharacterStats>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<EncounterStats>> Create_ListEncounterStats(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<EncounterStats>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<EncounterStats>> collectionInfo = new JsonCollectionInfoValues<List<EncounterStats>>
			{
				ObjectCreator = () => new List<EncounterStats>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<EncounterStats>, EncounterStats>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<EnemyStats>> Create_ListEnemyStats(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<EnemyStats>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<EnemyStats>> collectionInfo = new JsonCollectionInfoValues<List<EnemyStats>>
			{
				ObjectCreator = () => new List<EnemyStats>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<EnemyStats>, EnemyStats>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<FightStats>> Create_ListFightStats(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<FightStats>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<FightStats>> collectionInfo = new JsonCollectionInfoValues<List<FightStats>>
			{
				ObjectCreator = () => new List<FightStats>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<FightStats>, FightStats>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<SerializableMapDrawingLine>> Create_ListSerializableMapDrawingLine(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<SerializableMapDrawingLine>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<SerializableMapDrawingLine>> collectionInfo = new JsonCollectionInfoValues<List<SerializableMapDrawingLine>>
			{
				ObjectCreator = () => new List<SerializableMapDrawingLine>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<SerializableMapDrawingLine>, SerializableMapDrawingLine>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<SerializablePlayerMapDrawings>> Create_ListSerializablePlayerMapDrawings(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<SerializablePlayerMapDrawings>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<SerializablePlayerMapDrawings>> collectionInfo = new JsonCollectionInfoValues<List<SerializablePlayerMapDrawings>>
			{
				ObjectCreator = () => new List<SerializablePlayerMapDrawings>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<SerializablePlayerMapDrawings>, SerializablePlayerMapDrawings>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<MigratingData>> Create_ListMigratingData(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<MigratingData>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<MigratingData>> collectionInfo = new JsonCollectionInfoValues<List<MigratingData>>
			{
				ObjectCreator = () => new List<MigratingData>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<MigratingData>, MigratingData>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<SavedProperties.SavedProperty<bool>>> Create_ListSavedPropertyBoolean(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<SavedProperties.SavedProperty<bool>>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<SavedProperties.SavedProperty<bool>>> collectionInfo = new JsonCollectionInfoValues<List<SavedProperties.SavedProperty<bool>>>
			{
				ObjectCreator = () => new List<SavedProperties.SavedProperty<bool>>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<SavedProperties.SavedProperty<bool>>, SavedProperties.SavedProperty<bool>>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<SavedProperties.SavedProperty<ModelId>>> Create_ListSavedPropertyModelId(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<SavedProperties.SavedProperty<ModelId>>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<SavedProperties.SavedProperty<ModelId>>> collectionInfo = new JsonCollectionInfoValues<List<SavedProperties.SavedProperty<ModelId>>>
			{
				ObjectCreator = () => new List<SavedProperties.SavedProperty<ModelId>>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<SavedProperties.SavedProperty<ModelId>>, SavedProperties.SavedProperty<ModelId>>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<SavedProperties.SavedProperty<SerializableCard[]>>> Create_ListSavedPropertySerializableCardArray(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<SavedProperties.SavedProperty<SerializableCard[]>>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<SavedProperties.SavedProperty<SerializableCard[]>>> collectionInfo = new JsonCollectionInfoValues<List<SavedProperties.SavedProperty<SerializableCard[]>>>
			{
				ObjectCreator = () => new List<SavedProperties.SavedProperty<SerializableCard[]>>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<SavedProperties.SavedProperty<SerializableCard[]>>, SavedProperties.SavedProperty<SerializableCard[]>>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<SavedProperties.SavedProperty<SerializableCard>>> Create_ListSavedPropertySerializableCard(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<SavedProperties.SavedProperty<SerializableCard>>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<SavedProperties.SavedProperty<SerializableCard>>> collectionInfo = new JsonCollectionInfoValues<List<SavedProperties.SavedProperty<SerializableCard>>>
			{
				ObjectCreator = () => new List<SavedProperties.SavedProperty<SerializableCard>>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<SavedProperties.SavedProperty<SerializableCard>>, SavedProperties.SavedProperty<SerializableCard>>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<SavedProperties.SavedProperty<int[]>>> Create_ListSavedPropertyInt32Array(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<SavedProperties.SavedProperty<int[]>>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<SavedProperties.SavedProperty<int[]>>> collectionInfo = new JsonCollectionInfoValues<List<SavedProperties.SavedProperty<int[]>>>
			{
				ObjectCreator = () => new List<SavedProperties.SavedProperty<int[]>>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<SavedProperties.SavedProperty<int[]>>, SavedProperties.SavedProperty<int[]>>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<SavedProperties.SavedProperty<int>>> Create_ListSavedPropertyInt32(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<SavedProperties.SavedProperty<int>>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<SavedProperties.SavedProperty<int>>> collectionInfo = new JsonCollectionInfoValues<List<SavedProperties.SavedProperty<int>>>
			{
				ObjectCreator = () => new List<SavedProperties.SavedProperty<int>>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<SavedProperties.SavedProperty<int>>, SavedProperties.SavedProperty<int>>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<SavedProperties.SavedProperty<string>>> Create_ListSavedPropertyString(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<SavedProperties.SavedProperty<string>>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<SavedProperties.SavedProperty<string>>> collectionInfo = new JsonCollectionInfoValues<List<SavedProperties.SavedProperty<string>>>
			{
				ObjectCreator = () => new List<SavedProperties.SavedProperty<string>>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<SavedProperties.SavedProperty<string>>, SavedProperties.SavedProperty<string>>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<SerializableActModel>> Create_ListSerializableActModel(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<SerializableActModel>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<SerializableActModel>> collectionInfo = new JsonCollectionInfoValues<List<SerializableActModel>>
			{
				ObjectCreator = () => new List<SerializableActModel>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<SerializableActModel>, SerializableActModel>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<SerializableCard>> Create_ListSerializableCard(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<SerializableCard>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<SerializableCard>> collectionInfo = new JsonCollectionInfoValues<List<SerializableCard>>
			{
				ObjectCreator = () => new List<SerializableCard>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<SerializableCard>, SerializableCard>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<SerializableMapPoint>> Create_ListSerializableMapPoint(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<SerializableMapPoint>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<SerializableMapPoint>> collectionInfo = new JsonCollectionInfoValues<List<SerializableMapPoint>>
			{
				ObjectCreator = () => new List<SerializableMapPoint>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<SerializableMapPoint>, SerializableMapPoint>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<SerializableModifier>> Create_ListSerializableModifier(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<SerializableModifier>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<SerializableModifier>> collectionInfo = new JsonCollectionInfoValues<List<SerializableModifier>>
			{
				ObjectCreator = () => new List<SerializableModifier>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<SerializableModifier>, SerializableModifier>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<SerializablePlayer>> Create_ListSerializablePlayer(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<SerializablePlayer>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<SerializablePlayer>> collectionInfo = new JsonCollectionInfoValues<List<SerializablePlayer>>
			{
				ObjectCreator = () => new List<SerializablePlayer>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<SerializablePlayer>, SerializablePlayer>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<SerializablePotion>> Create_ListSerializablePotion(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<SerializablePotion>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<SerializablePotion>> collectionInfo = new JsonCollectionInfoValues<List<SerializablePotion>>
			{
				ObjectCreator = () => new List<SerializablePotion>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<SerializablePotion>, SerializablePotion>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<SerializableRelic>> Create_ListSerializableRelic(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<SerializableRelic>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<SerializableRelic>> collectionInfo = new JsonCollectionInfoValues<List<SerializableRelic>>
			{
				ObjectCreator = () => new List<SerializableRelic>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<SerializableRelic>, SerializableRelic>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<SerializableReward>> Create_ListSerializableReward(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<SerializableReward>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<SerializableReward>> collectionInfo = new JsonCollectionInfoValues<List<SerializableReward>>
			{
				ObjectCreator = () => new List<SerializableReward>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<SerializableReward>, SerializableReward>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<SerializableEpoch>> Create_ListSerializableEpoch(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<SerializableEpoch>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<SerializableEpoch>> collectionInfo = new JsonCollectionInfoValues<List<SerializableEpoch>>
			{
				ObjectCreator = () => new List<SerializableEpoch>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<SerializableEpoch>, SerializableEpoch>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<SerializableUnlockedAchievement>> Create_ListSerializableUnlockedAchievement(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<SerializableUnlockedAchievement>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<SerializableUnlockedAchievement>> collectionInfo = new JsonCollectionInfoValues<List<SerializableUnlockedAchievement>>
			{
				ObjectCreator = () => new List<SerializableUnlockedAchievement>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<SerializableUnlockedAchievement>, SerializableUnlockedAchievement>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<Dictionary<string, object>>> Create_ListDictionaryStringObject(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<Dictionary<string, object>>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<Dictionary<string, object>>> collectionInfo = new JsonCollectionInfoValues<List<Dictionary<string, object>>>
			{
				ObjectCreator = () => new List<Dictionary<string, object>>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<Dictionary<string, object>>, Dictionary<string, object>>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<List<MapPointHistoryEntry>>> Create_ListListMapPointHistoryEntry(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<List<MapPointHistoryEntry>>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<List<MapPointHistoryEntry>>> collectionInfo = new JsonCollectionInfoValues<List<List<MapPointHistoryEntry>>>
			{
				ObjectCreator = () => new List<List<MapPointHistoryEntry>>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<List<MapPointHistoryEntry>>, List<MapPointHistoryEntry>>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<List<PlayerMapPointHistoryEntry>>> Create_ListListPlayerMapPointHistoryEntry(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<List<PlayerMapPointHistoryEntry>>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<List<PlayerMapPointHistoryEntry>>> collectionInfo = new JsonCollectionInfoValues<List<List<PlayerMapPointHistoryEntry>>>
			{
				ObjectCreator = () => new List<List<PlayerMapPointHistoryEntry>>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<List<PlayerMapPointHistoryEntry>>, List<PlayerMapPointHistoryEntry>>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<JsonNode>> Create_ListJsonNode(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<JsonNode>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<JsonNode>> collectionInfo = new JsonCollectionInfoValues<List<JsonNode>>
			{
				ObjectCreator = () => new List<JsonNode>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<JsonNode>, JsonNode>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<string>> Create_ListString(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<string>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<string>> collectionInfo = new JsonCollectionInfoValues<List<string>>
			{
				ObjectCreator = () => new List<string>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<string>, string>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<ulong>> Create_ListUInt64(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<ulong>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<ulong>> collectionInfo = new JsonCollectionInfoValues<List<ulong>>
			{
				ObjectCreator = () => new List<ulong>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<ulong>, ulong>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<DateTimeOffset> Create_DateTimeOffset(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<DateTimeOffset> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<DateTimeOffset>(options, JsonMetadataServices.DateTimeOffsetConverter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<DateTimeOffset?> Create_NullableDateTimeOffset(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<DateTimeOffset?> jsonTypeInfo))
		{
			JsonConverter nullableConverter = JsonMetadataServices.GetNullableConverter<DateTimeOffset>(options);
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<DateTimeOffset?>(options, nullableConverter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<JsonNode> Create_JsonNode(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<JsonNode> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<JsonNode>(options, JsonMetadataServices.JsonNodeConverter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<JsonObject> Create_JsonObject(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<JsonObject> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<JsonObject>(options, JsonMetadataServices.JsonObjectConverter);
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

	private JsonTypeInfo<int?> Create_NullableInt32(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<int?> jsonTypeInfo))
		{
			JsonConverter nullableConverter = JsonMetadataServices.GetNullableConverter<int>(options);
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<int?>(options, nullableConverter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<int[]> Create_Int32Array(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<int[]> jsonTypeInfo))
		{
			JsonCollectionInfoValues<int[]> collectionInfo = new JsonCollectionInfoValues<int[]>
			{
				ObjectCreator = null,
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateArrayInfo(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
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

	private JsonTypeInfo<object> Create_Object(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<object> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<object>(options, JsonMetadataServices.ObjectConverter);
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

	private JsonTypeInfo<uint> Create_UInt32(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<uint> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<uint>(options, JsonMetadataServices.UInt32Converter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<ulong> Create_UInt64(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<ulong> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<ulong>(options, JsonMetadataServices.UInt64Converter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	public MegaCritSerializerContext()
		: base(null)
	{
	}

	public MegaCritSerializerContext(JsonSerializerOptions options)
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
		if (type == typeof(double))
		{
			return Create_Double(options);
		}
		if (type == typeof(float))
		{
			return Create_Single(options);
		}
		if (type == typeof(Vector2))
		{
			return Create_Vector2(options);
		}
		if (type == typeof(Vector2I))
		{
			return Create_Vector2I(options);
		}
		if (type == typeof(ControllerMappingType))
		{
			return Create_ControllerMappingType(options);
		}
		if (type == typeof(RelicRarity))
		{
			return Create_RelicRarity(options);
		}
		if (type == typeof(PlayerRngType))
		{
			return Create_PlayerRngType(options);
		}
		if (type == typeof(RunRngType))
		{
			return Create_RunRngType(options);
		}
		if (type == typeof(LocString))
		{
			return Create_LocString(options);
		}
		if (type == typeof(MapCoord))
		{
			return Create_MapCoord(options);
		}
		if (type == typeof(MapPointType))
		{
			return Create_MapPointType(options);
		}
		if (type == typeof(DisabledMod))
		{
			return Create_DisabledMod(options);
		}
		if (type == typeof(ModManifest))
		{
			return Create_ModManifest(options);
		}
		if (type == typeof(ModSettings))
		{
			return Create_ModSettings(options);
		}
		if (type == typeof(ModSource))
		{
			return Create_ModSource(options);
		}
		if (type == typeof(ModelId))
		{
			return Create_ModelId(options);
		}
		if (type == typeof(FeedbackData))
		{
			return Create_FeedbackData(options);
		}
		if (type == typeof(NullLeaderboard))
		{
			return Create_NullLeaderboard(options);
		}
		if (type == typeof(NullLeaderboardFile))
		{
			return Create_NullLeaderboardFile(options);
		}
		if (type == typeof(NullLeaderboardFileEntry))
		{
			return Create_NullLeaderboardFileEntry(options);
		}
		if (type == typeof(NullMultiplayerName))
		{
			return Create_NullMultiplayerName(options);
		}
		if (type == typeof(PlatformType))
		{
			return Create_PlatformType(options);
		}
		if (type == typeof(RewardType))
		{
			return Create_RewardType(options);
		}
		if (type == typeof(RoomType))
		{
			return Create_RoomType(options);
		}
		if (type == typeof(CardCreationSource))
		{
			return Create_CardCreationSource(options);
		}
		if (type == typeof(CardRarityOddsType))
		{
			return Create_CardRarityOddsType(options);
		}
		if (type == typeof(GameMode))
		{
			return Create_GameMode(options);
		}
		if (type == typeof(AncientChoiceHistoryEntry))
		{
			return Create_AncientChoiceHistoryEntry(options);
		}
		if (type == typeof(CardChoiceHistoryEntry))
		{
			return Create_CardChoiceHistoryEntry(options);
		}
		if (type == typeof(CardEnchantmentHistoryEntry))
		{
			return Create_CardEnchantmentHistoryEntry(options);
		}
		if (type == typeof(CardTransformationHistoryEntry))
		{
			return Create_CardTransformationHistoryEntry(options);
		}
		if (type == typeof(EventOptionHistoryEntry))
		{
			return Create_EventOptionHistoryEntry(options);
		}
		if (type == typeof(MapPointHistoryEntry))
		{
			return Create_MapPointHistoryEntry(options);
		}
		if (type == typeof(MapPointRoomHistoryEntry))
		{
			return Create_MapPointRoomHistoryEntry(options);
		}
		if (type == typeof(ModelChoiceHistoryEntry))
		{
			return Create_ModelChoiceHistoryEntry(options);
		}
		if (type == typeof(PlayerMapPointHistoryEntry))
		{
			return Create_PlayerMapPointHistoryEntry(options);
		}
		if (type == typeof(RunHistory))
		{
			return Create_RunHistory(options);
		}
		if (type == typeof(RunHistoryPlayer))
		{
			return Create_RunHistoryPlayer(options);
		}
		if (type == typeof(AncientCharacterStats))
		{
			return Create_AncientCharacterStats(options);
		}
		if (type == typeof(AncientStats))
		{
			return Create_AncientStats(options);
		}
		if (type == typeof(CardStats))
		{
			return Create_CardStats(options);
		}
		if (type == typeof(CharacterStats))
		{
			return Create_CharacterStats(options);
		}
		if (type == typeof(EncounterStats))
		{
			return Create_EncounterStats(options);
		}
		if (type == typeof(EnemyStats))
		{
			return Create_EnemyStats(options);
		}
		if (type == typeof(EpochState))
		{
			return Create_EpochState(options);
		}
		if (type == typeof(FightStats))
		{
			return Create_FightStats(options);
		}
		if (type == typeof(SerializableMapDrawingLine))
		{
			return Create_SerializableMapDrawingLine(options);
		}
		if (type == typeof(SerializableMapDrawings))
		{
			return Create_SerializableMapDrawings(options);
		}
		if (type == typeof(SerializablePlayerMapDrawings))
		{
			return Create_SerializablePlayerMapDrawings(options);
		}
		if (type == typeof(MigratingData))
		{
			return Create_MigratingData(options);
		}
		if (type == typeof(PrefsSave))
		{
			return Create_PrefsSave(options);
		}
		if (type == typeof(ProfileSave))
		{
			return Create_ProfileSave(options);
		}
		if (type == typeof(SavedProperties))
		{
			return Create_SavedProperties(options);
		}
		if (type == typeof(SavedProperties.SavedProperty<bool>))
		{
			return Create_SavedPropertyBoolean(options);
		}
		if (type == typeof(SavedProperties.SavedProperty<ModelId>))
		{
			return Create_SavedPropertyModelId(options);
		}
		if (type == typeof(SavedProperties.SavedProperty<SerializableCard[]>))
		{
			return Create_SavedPropertySerializableCardArray(options);
		}
		if (type == typeof(SavedProperties.SavedProperty<SerializableCard>))
		{
			return Create_SavedPropertySerializableCard(options);
		}
		if (type == typeof(SavedProperties.SavedProperty<int[]>))
		{
			return Create_SavedPropertyInt32Array(options);
		}
		if (type == typeof(SavedProperties.SavedProperty<int>))
		{
			return Create_SavedPropertyInt32(options);
		}
		if (type == typeof(SavedProperties.SavedProperty<string>))
		{
			return Create_SavedPropertyString(options);
		}
		if (type == typeof(SerializableActMap))
		{
			return Create_SerializableActMap(options);
		}
		if (type == typeof(SerializableActModel))
		{
			return Create_SerializableActModel(options);
		}
		if (type == typeof(SerializableCard))
		{
			return Create_SerializableCard(options);
		}
		if (type == typeof(SerializableCard[]))
		{
			return Create_SerializableCardArray(options);
		}
		if (type == typeof(SerializableEnchantment))
		{
			return Create_SerializableEnchantment(options);
		}
		if (type == typeof(SerializableExtraRunFields))
		{
			return Create_SerializableExtraRunFields(options);
		}
		if (type == typeof(SerializableMapPoint))
		{
			return Create_SerializableMapPoint(options);
		}
		if (type == typeof(SerializableModifier))
		{
			return Create_SerializableModifier(options);
		}
		if (type == typeof(SerializablePlayer))
		{
			return Create_SerializablePlayer(options);
		}
		if (type == typeof(SerializablePlayerOddsSet))
		{
			return Create_SerializablePlayerOddsSet(options);
		}
		if (type == typeof(SerializablePotion))
		{
			return Create_SerializablePotion(options);
		}
		if (type == typeof(SerializableRelic))
		{
			return Create_SerializableRelic(options);
		}
		if (type == typeof(SerializableRelicGrabBag))
		{
			return Create_SerializableRelicGrabBag(options);
		}
		if (type == typeof(SerializableReward))
		{
			return Create_SerializableReward(options);
		}
		if (type == typeof(SerializableRoom))
		{
			return Create_SerializableRoom(options);
		}
		if (type == typeof(SerializableRoomSet))
		{
			return Create_SerializableRoomSet(options);
		}
		if (type == typeof(SerializableRunOddsSet))
		{
			return Create_SerializableRunOddsSet(options);
		}
		if (type == typeof(SerializableRunRngSet))
		{
			return Create_SerializableRunRngSet(options);
		}
		if (type == typeof(SerializableEpoch))
		{
			return Create_SerializableEpoch(options);
		}
		if (type == typeof(SerializableExtraPlayerFields))
		{
			return Create_SerializableExtraPlayerFields(options);
		}
		if (type == typeof(SerializablePlayerRngSet))
		{
			return Create_SerializablePlayerRngSet(options);
		}
		if (type == typeof(SerializableProgress))
		{
			return Create_SerializableProgress(options);
		}
		if (type == typeof(SerializableRun))
		{
			return Create_SerializableRun(options);
		}
		if (type == typeof(SerializableUnlockedAchievement))
		{
			return Create_SerializableUnlockedAchievement(options);
		}
		if (type == typeof(SettingsSave))
		{
			return Create_SettingsSave(options);
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
		if (type == typeof(SerializableUnlockState))
		{
			return Create_SerializableUnlockState(options);
		}
		if (type == typeof(Dictionary<RelicRarity, List<ModelId>>))
		{
			return Create_DictionaryRelicRarityListModelId(options);
		}
		if (type == typeof(Dictionary<PlayerRngType, int>))
		{
			return Create_DictionaryPlayerRngTypeInt32(options);
		}
		if (type == typeof(Dictionary<RunRngType, int>))
		{
			return Create_DictionaryRunRngTypeInt32(options);
		}
		if (type == typeof(Dictionary<string, object>))
		{
			return Create_DictionaryStringObject(options);
		}
		if (type == typeof(Dictionary<string, string>))
		{
			return Create_DictionaryStringString(options);
		}
		if (type == typeof(Dictionary<ulong, List<SerializableReward>>))
		{
			return Create_DictionaryUInt64ListSerializableReward(options);
		}
		if (type == typeof(IEnumerable<SerializableCard>))
		{
			return Create_IEnumerableSerializableCard(options);
		}
		if (type == typeof(IEnumerable<SerializablePotion>))
		{
			return Create_IEnumerableSerializablePotion(options);
		}
		if (type == typeof(IEnumerable<SerializableRelic>))
		{
			return Create_IEnumerableSerializableRelic(options);
		}
		if (type == typeof(List<Vector2>))
		{
			return Create_ListVector2(options);
		}
		if (type == typeof(List<MapCoord>))
		{
			return Create_ListMapCoord(options);
		}
		if (type == typeof(List<DisabledMod>))
		{
			return Create_ListDisabledMod(options);
		}
		if (type == typeof(List<ModelId>))
		{
			return Create_ListModelId(options);
		}
		if (type == typeof(List<NullLeaderboard>))
		{
			return Create_ListNullLeaderboard(options);
		}
		if (type == typeof(List<NullLeaderboardFileEntry>))
		{
			return Create_ListNullLeaderboardFileEntry(options);
		}
		if (type == typeof(List<NullMultiplayerName>))
		{
			return Create_ListNullMultiplayerName(options);
		}
		if (type == typeof(List<AncientChoiceHistoryEntry>))
		{
			return Create_ListAncientChoiceHistoryEntry(options);
		}
		if (type == typeof(List<CardChoiceHistoryEntry>))
		{
			return Create_ListCardChoiceHistoryEntry(options);
		}
		if (type == typeof(List<CardEnchantmentHistoryEntry>))
		{
			return Create_ListCardEnchantmentHistoryEntry(options);
		}
		if (type == typeof(List<CardTransformationHistoryEntry>))
		{
			return Create_ListCardTransformationHistoryEntry(options);
		}
		if (type == typeof(List<EventOptionHistoryEntry>))
		{
			return Create_ListEventOptionHistoryEntry(options);
		}
		if (type == typeof(List<MapPointHistoryEntry>))
		{
			return Create_ListMapPointHistoryEntry(options);
		}
		if (type == typeof(List<MapPointRoomHistoryEntry>))
		{
			return Create_ListMapPointRoomHistoryEntry(options);
		}
		if (type == typeof(List<ModelChoiceHistoryEntry>))
		{
			return Create_ListModelChoiceHistoryEntry(options);
		}
		if (type == typeof(List<PlayerMapPointHistoryEntry>))
		{
			return Create_ListPlayerMapPointHistoryEntry(options);
		}
		if (type == typeof(List<RunHistoryPlayer>))
		{
			return Create_ListRunHistoryPlayer(options);
		}
		if (type == typeof(List<AncientCharacterStats>))
		{
			return Create_ListAncientCharacterStats(options);
		}
		if (type == typeof(List<AncientStats>))
		{
			return Create_ListAncientStats(options);
		}
		if (type == typeof(List<CardStats>))
		{
			return Create_ListCardStats(options);
		}
		if (type == typeof(List<CharacterStats>))
		{
			return Create_ListCharacterStats(options);
		}
		if (type == typeof(List<EncounterStats>))
		{
			return Create_ListEncounterStats(options);
		}
		if (type == typeof(List<EnemyStats>))
		{
			return Create_ListEnemyStats(options);
		}
		if (type == typeof(List<FightStats>))
		{
			return Create_ListFightStats(options);
		}
		if (type == typeof(List<SerializableMapDrawingLine>))
		{
			return Create_ListSerializableMapDrawingLine(options);
		}
		if (type == typeof(List<SerializablePlayerMapDrawings>))
		{
			return Create_ListSerializablePlayerMapDrawings(options);
		}
		if (type == typeof(List<MigratingData>))
		{
			return Create_ListMigratingData(options);
		}
		if (type == typeof(List<SavedProperties.SavedProperty<bool>>))
		{
			return Create_ListSavedPropertyBoolean(options);
		}
		if (type == typeof(List<SavedProperties.SavedProperty<ModelId>>))
		{
			return Create_ListSavedPropertyModelId(options);
		}
		if (type == typeof(List<SavedProperties.SavedProperty<SerializableCard[]>>))
		{
			return Create_ListSavedPropertySerializableCardArray(options);
		}
		if (type == typeof(List<SavedProperties.SavedProperty<SerializableCard>>))
		{
			return Create_ListSavedPropertySerializableCard(options);
		}
		if (type == typeof(List<SavedProperties.SavedProperty<int[]>>))
		{
			return Create_ListSavedPropertyInt32Array(options);
		}
		if (type == typeof(List<SavedProperties.SavedProperty<int>>))
		{
			return Create_ListSavedPropertyInt32(options);
		}
		if (type == typeof(List<SavedProperties.SavedProperty<string>>))
		{
			return Create_ListSavedPropertyString(options);
		}
		if (type == typeof(List<SerializableActModel>))
		{
			return Create_ListSerializableActModel(options);
		}
		if (type == typeof(List<SerializableCard>))
		{
			return Create_ListSerializableCard(options);
		}
		if (type == typeof(List<SerializableMapPoint>))
		{
			return Create_ListSerializableMapPoint(options);
		}
		if (type == typeof(List<SerializableModifier>))
		{
			return Create_ListSerializableModifier(options);
		}
		if (type == typeof(List<SerializablePlayer>))
		{
			return Create_ListSerializablePlayer(options);
		}
		if (type == typeof(List<SerializablePotion>))
		{
			return Create_ListSerializablePotion(options);
		}
		if (type == typeof(List<SerializableRelic>))
		{
			return Create_ListSerializableRelic(options);
		}
		if (type == typeof(List<SerializableReward>))
		{
			return Create_ListSerializableReward(options);
		}
		if (type == typeof(List<SerializableEpoch>))
		{
			return Create_ListSerializableEpoch(options);
		}
		if (type == typeof(List<SerializableUnlockedAchievement>))
		{
			return Create_ListSerializableUnlockedAchievement(options);
		}
		if (type == typeof(List<Dictionary<string, object>>))
		{
			return Create_ListDictionaryStringObject(options);
		}
		if (type == typeof(List<List<MapPointHistoryEntry>>))
		{
			return Create_ListListMapPointHistoryEntry(options);
		}
		if (type == typeof(List<List<PlayerMapPointHistoryEntry>>))
		{
			return Create_ListListPlayerMapPointHistoryEntry(options);
		}
		if (type == typeof(List<JsonNode>))
		{
			return Create_ListJsonNode(options);
		}
		if (type == typeof(List<string>))
		{
			return Create_ListString(options);
		}
		if (type == typeof(List<ulong>))
		{
			return Create_ListUInt64(options);
		}
		if (type == typeof(DateTimeOffset))
		{
			return Create_DateTimeOffset(options);
		}
		if (type == typeof(DateTimeOffset?))
		{
			return Create_NullableDateTimeOffset(options);
		}
		if (type == typeof(JsonNode))
		{
			return Create_JsonNode(options);
		}
		if (type == typeof(JsonObject))
		{
			return Create_JsonObject(options);
		}
		if (type == typeof(int))
		{
			return Create_Int32(options);
		}
		if (type == typeof(int?))
		{
			return Create_NullableInt32(options);
		}
		if (type == typeof(int[]))
		{
			return Create_Int32Array(options);
		}
		if (type == typeof(long))
		{
			return Create_Int64(options);
		}
		if (type == typeof(object))
		{
			return Create_Object(options);
		}
		if (type == typeof(string))
		{
			return Create_String(options);
		}
		if (type == typeof(uint))
		{
			return Create_UInt32(options);
		}
		if (type == typeof(ulong))
		{
			return Create_UInt64(options);
		}
		return null;
	}
}
