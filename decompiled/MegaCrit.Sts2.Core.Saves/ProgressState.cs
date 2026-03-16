using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using MegaCrit.Sts2.Core.Achievements;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;
using MegaCrit.Sts2.Core.Saves.Validation;
using MegaCrit.Sts2.Core.Timeline;

namespace MegaCrit.Sts2.Core.Saves;

public class ProgressState
{
	private static readonly Dictionary<string, Achievement> AchievementsByName = BuildAchievementLookup();

	private readonly Dictionary<ModelId, CharacterStats> _characterStats = new Dictionary<ModelId, CharacterStats>();

	private readonly Dictionary<ModelId, CardStats> _cardStats = new Dictionary<ModelId, CardStats>();

	private readonly Dictionary<ModelId, EncounterStats> _encounterStats = new Dictionary<ModelId, EncounterStats>();

	private readonly Dictionary<ModelId, EnemyStats> _enemyStats = new Dictionary<ModelId, EnemyStats>();

	private readonly Dictionary<ModelId, AncientStats> _ancientStats = new Dictionary<ModelId, AncientStats>();

	private readonly HashSet<ModelId> _discoveredCards = new HashSet<ModelId>();

	private readonly HashSet<ModelId> _discoveredRelics = new HashSet<ModelId>();

	private readonly HashSet<ModelId> _discoveredPotions = new HashSet<ModelId>();

	private readonly HashSet<ModelId> _discoveredEvents = new HashSet<ModelId>();

	private readonly HashSet<ModelId> _discoveredActs = new HashSet<ModelId>();

	private readonly List<SerializableEpoch> _epochs = new List<SerializableEpoch>();

	private readonly Dictionary<Achievement, long> _unlockedAchievements = new Dictionary<Achievement, long>();

	private readonly HashSet<string> _ftueCompleted = new HashSet<string>();

	public IReadOnlyDictionary<ModelId, CharacterStats> CharacterStats => _characterStats;

	public IReadOnlyDictionary<ModelId, CardStats> CardStats => _cardStats;

	public IReadOnlyDictionary<ModelId, EncounterStats> EncounterStats => _encounterStats;

	public IReadOnlyDictionary<ModelId, EnemyStats> EnemyStats => _enemyStats;

	public IReadOnlyDictionary<ModelId, AncientStats> AncientStats => _ancientStats;

	public IReadOnlySet<ModelId> DiscoveredCards => _discoveredCards;

	public IReadOnlySet<ModelId> DiscoveredRelics => _discoveredRelics;

	public IReadOnlySet<ModelId> DiscoveredPotions => _discoveredPotions;

	public IReadOnlySet<ModelId> DiscoveredEvents => _discoveredEvents;

	public IReadOnlySet<ModelId> DiscoveredActs => _discoveredActs;

	public IReadOnlyList<SerializableEpoch> Epochs => _epochs;

	public IReadOnlyDictionary<Achievement, long> UnlockedAchievements => _unlockedAchievements;

	public IReadOnlySet<string> FtueCompleted => _ftueCompleted;

	public string UniqueId { get; init; } = "";

	public bool EnableFtues { get; set; } = true;

	public long TotalPlaytime { get; set; }

	public int TotalUnlocks { get; set; }

	public int CurrentScore { get; set; }

	public long FloorsClimbed { get; set; }

	public long ArchitectDamage { get; set; }

	public int WongoPoints { get; set; }

	public int PreferredMultiplayerAscension { get; set; }

	public int MaxMultiplayerAscension { get; set; }

	public int TestSubjectKills { get; set; }

	public ModelId PendingCharacterUnlock { get; set; } = ModelId.none;

	public int Wins => _characterStats.Values.Sum((CharacterStats c) => c.TotalWins);

	public int Losses => _characterStats.Values.Sum((CharacterStats c) => c.TotalLosses);

	public long FastestVictory
	{
		get
		{
			if (_characterStats.Count == 0)
			{
				return 999999999L;
			}
			return _characterStats.Values.Min((CharacterStats c) => (c.FastestWinTime != -1) ? c.FastestWinTime : 999999999);
		}
	}

	public long BestWinStreak
	{
		get
		{
			if (_characterStats.Count == 0)
			{
				return 0L;
			}
			return _characterStats.Values.Max((CharacterStats c) => c.BestWinStreak);
		}
	}

	public int NumberOfRuns => Wins + Losses;

	public static ProgressState CreateDefault()
	{
		return FromSerializable(new SerializableProgress(), new DeserializationContext());
	}

	public static ProgressState FromSerializable(SerializableProgress save, DeserializationContext ctx)
	{
		ArgumentNullException.ThrowIfNull(save, "save");
		ArgumentNullException.ThrowIfNull(ctx, "ctx");
		ProgressState progressState = new ProgressState
		{
			UniqueId = save.UniqueId,
			EnableFtues = save.EnableFtues,
			TotalPlaytime = ClampNonNegative(save.TotalPlaytime, "TotalPlaytime", ctx),
			TotalUnlocks = ClampNonNegativeInt(save.TotalUnlocks, "TotalUnlocks", ctx),
			CurrentScore = ClampNonNegativeInt(save.CurrentScore, "CurrentScore", ctx),
			FloorsClimbed = ClampNonNegative(save.FloorsClimbed, "FloorsClimbed", ctx),
			ArchitectDamage = ClampNonNegative(save.ArchitectDamage, "ArchitectDamage", ctx),
			WongoPoints = ClampNonNegativeInt(save.WongoPoints, "WongoPoints", ctx),
			PreferredMultiplayerAscension = ClampAscension(save.PreferredMultiplayerAscension, "PreferredMultiplayerAscension", ctx),
			MaxMultiplayerAscension = ClampAscension(save.MaxMultiplayerAscension, "MaxMultiplayerAscension", ctx),
			TestSubjectKills = ClampNonNegativeInt(save.TestSubjectKills, "TestSubjectKills", ctx),
			PendingCharacterUnlock = ValidateModelId<CharacterModel>(save.PendingCharacterUnlock, "PendingCharacterUnlock", ctx)
		};
		ParseCharacterStats(save.CharStats, progressState._characterStats, ctx);
		ParseCardStats(save.CardStats, progressState._cardStats, ctx);
		ParseEncounterStats(save.EncounterStats, progressState._encounterStats, ctx);
		ParseEnemyStats(save.EnemyStats, progressState._enemyStats, ctx);
		ParseAncientStats(save.AncientStats, progressState._ancientStats, ctx);
		ParseDiscoveredSet<CardModel>(save.DiscoveredCards, progressState._discoveredCards, "DiscoveredCards", ctx);
		ParseDiscoveredSet<RelicModel>(save.DiscoveredRelics, progressState._discoveredRelics, "DiscoveredRelics", ctx);
		ParseDiscoveredSet<PotionModel>(save.DiscoveredPotions, progressState._discoveredPotions, "DiscoveredPotions", ctx);
		ParseDiscoveredSet<EventModel>(save.DiscoveredEvents, progressState._discoveredEvents, "DiscoveredEvents", ctx);
		ParseDiscoveredSet<ActModel>(save.DiscoveredActs, progressState._discoveredActs, "DiscoveredActs", ctx);
		ParseEpochs(save.Epochs, progressState._epochs, ctx);
		FixMissingSlots(progressState._epochs, ctx);
		ParseFtues(save.FtueCompleted, progressState._ftueCompleted, ctx);
		ParseAchievements(save.UnlockedAchievements, progressState._unlockedAchievements, ctx);
		progressState.FilterAndSortEpochs();
		return progressState;
	}

	public SerializableProgress ToSerializable()
	{
		return new SerializableProgress
		{
			UniqueId = UniqueId,
			SchemaVersion = 0,
			EnableFtues = EnableFtues,
			TotalPlaytime = TotalPlaytime,
			TotalUnlocks = TotalUnlocks,
			CurrentScore = CurrentScore,
			FloorsClimbed = FloorsClimbed,
			ArchitectDamage = ArchitectDamage,
			WongoPoints = WongoPoints,
			PreferredMultiplayerAscension = PreferredMultiplayerAscension,
			MaxMultiplayerAscension = MaxMultiplayerAscension,
			TestSubjectKills = TestSubjectKills,
			PendingCharacterUnlock = PendingCharacterUnlock,
			CharStats = _characterStats.Values.ToList(),
			CardStats = _cardStats.Values.ToList(),
			EncounterStats = _encounterStats.Values.ToList(),
			EnemyStats = _enemyStats.Values.ToList(),
			AncientStats = _ancientStats.Values.ToList(),
			DiscoveredCards = _discoveredCards.ToList(),
			DiscoveredRelics = _discoveredRelics.ToList(),
			DiscoveredPotions = _discoveredPotions.ToList(),
			DiscoveredEvents = _discoveredEvents.ToList(),
			DiscoveredActs = _discoveredActs.ToList(),
			Epochs = _epochs.ToList(),
			FtueCompleted = _ftueCompleted.ToList(),
			UnlockedAchievements = _unlockedAchievements.Select((KeyValuePair<Achievement, long> kvp) => new SerializableUnlockedAchievement
			{
				Achievement = JsonNamingPolicy.SnakeCaseLower.ConvertName(kvp.Key.ToString()),
				UnlockTime = kvp.Value
			}).ToList()
		};
	}

	public CharacterStats GetOrCreateCharacterStats(ModelId characterId)
	{
		if (_characterStats.TryGetValue(characterId, out CharacterStats value))
		{
			return value;
		}
		CharacterStats characterStats = new CharacterStats
		{
			Id = characterId
		};
		_characterStats[characterId] = characterStats;
		return characterStats;
	}

	public CardStats GetOrCreateCardStats(ModelId cardId)
	{
		if (_cardStats.TryGetValue(cardId, out CardStats value))
		{
			return value;
		}
		CardStats cardStats = new CardStats
		{
			Id = cardId
		};
		_cardStats[cardId] = cardStats;
		return cardStats;
	}

	public bool MarkCardAsSeen(ModelId cardId)
	{
		return _discoveredCards.Add(cardId);
	}

	public bool MarkRelicAsSeen(ModelId relicId)
	{
		return _discoveredRelics.Add(relicId);
	}

	public bool MarkPotionAsSeen(ModelId potionId)
	{
		return _discoveredPotions.Add(potionId);
	}

	public bool MarkEventAsSeen(ModelId eventId)
	{
		return _discoveredEvents.Add(eventId);
	}

	public bool MarkActAsSeen(ModelId actId)
	{
		return _discoveredActs.Add(actId);
	}

	public bool MarkFtueAsComplete(string ftueId)
	{
		return _ftueCompleted.Add(ftueId);
	}

	public void AddUnlockedAchievement(Achievement achievement, long unlockTime)
	{
		_unlockedAchievements[achievement] = unlockTime;
	}

	public bool RemoveUnlockedAchievement(Achievement achievement)
	{
		return _unlockedAchievements.Remove(achievement);
	}

	public bool IsAchievementUnlocked(Achievement achievement)
	{
		return _unlockedAchievements.ContainsKey(achievement);
	}

	public void ObtainEpoch(string epochId)
	{
		SerializableEpoch serializableEpoch = _epochs.FirstOrDefault((SerializableEpoch e) => e.Id == epochId);
		if (serializableEpoch != null)
		{
			serializableEpoch.SetObtained(EpochState.Obtained);
			return;
		}
		_epochs.Add(new SerializableEpoch(epochId, EpochState.ObtainedNoSlot));
		FilterAndSortEpochs();
	}

	public void ObtainEpochOverride(string epochId, EpochState state)
	{
		SerializableEpoch serializableEpoch = _epochs.FirstOrDefault((SerializableEpoch e) => e.Id == epochId);
		if (serializableEpoch != null)
		{
			if (serializableEpoch.ObtainDate == 0L && state >= EpochState.ObtainedNoSlot)
			{
				serializableEpoch.ObtainDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			}
			serializableEpoch.State = state;
		}
		else
		{
			_epochs.Add(new SerializableEpoch(epochId, state));
			FilterAndSortEpochs();
		}
	}

	public void UnlockSlot(string epochId)
	{
		SerializableEpoch serializableEpoch = _epochs.FirstOrDefault((SerializableEpoch e) => e.Id == epochId);
		if (serializableEpoch == null)
		{
			_epochs.Add(new SerializableEpoch(epochId, EpochState.NotObtained));
			FilterAndSortEpochs();
			return;
		}
		if (serializableEpoch.State == EpochState.ObtainedNoSlot)
		{
			Log.Info("Attempted to get slot " + epochId + " but we already have an Epoch! Set to Obtained");
			serializableEpoch.State = EpochState.Obtained;
			return;
		}
		Log.Error($"Slot unlocked for {epochId} but it's in an invalid state: {serializableEpoch.State}");
	}

	public void RevealEpoch(string epochId)
	{
		SerializableEpoch serializableEpoch = _epochs.FirstOrDefault((SerializableEpoch e) => e.Id == epochId);
		if (serializableEpoch == null)
		{
			throw new InvalidOperationException($"Invalid epoch {epochId} passed to {"RevealEpoch"}!");
		}
		serializableEpoch.State = EpochState.Revealed;
	}

	public void ResetEpochs()
	{
		_epochs.Clear();
	}

	public CharacterStats? GetStatsForCharacter(ModelId characterId)
	{
		_characterStats.TryGetValue(characterId, out CharacterStats value);
		return value;
	}

	public EncounterStats GetOrCreateEncounterStats(ModelId encounterId)
	{
		if (_encounterStats.TryGetValue(encounterId, out EncounterStats value))
		{
			return value;
		}
		EncounterStats encounterStats = new EncounterStats
		{
			Id = encounterId
		};
		_encounterStats[encounterId] = encounterStats;
		return encounterStats;
	}

	public EnemyStats GetOrCreateEnemyStats(ModelId enemyId)
	{
		if (_enemyStats.TryGetValue(enemyId, out EnemyStats value))
		{
			return value;
		}
		EnemyStats enemyStats = new EnemyStats
		{
			Id = enemyId
		};
		_enemyStats[enemyId] = enemyStats;
		return enemyStats;
	}

	public AncientStats GetOrCreateAncientStats(ModelId ancientId)
	{
		if (_ancientStats.TryGetValue(ancientId, out AncientStats value))
		{
			return value;
		}
		AncientStats ancientStats = new AncientStats
		{
			Id = ancientId
		};
		_ancientStats[ancientId] = ancientStats;
		return ancientStats;
	}

	public AncientStats? GetStatsForAncient(ModelId ancientId)
	{
		_ancientStats.TryGetValue(ancientId, out AncientStats value);
		return value;
	}

	public void ResetFtues()
	{
		EnableFtues = true;
		_ftueCompleted.Clear();
	}

	public bool HasEpoch(string epochId)
	{
		return _epochs.Any((SerializableEpoch e) => e.Id == epochId);
	}

	public bool IsEpochObtained(string epochId)
	{
		SerializableEpoch serializableEpoch = _epochs.FirstOrDefault((SerializableEpoch e) => e.Id == epochId);
		if (serializableEpoch == null)
		{
			return false;
		}
		return serializableEpoch.State >= EpochState.ObtainedNoSlot;
	}

	public bool IsEpochRevealed(string epochId)
	{
		SerializableEpoch serializableEpoch = _epochs.FirstOrDefault((SerializableEpoch e) => e.Id == epochId);
		if (serializableEpoch == null)
		{
			return false;
		}
		return serializableEpoch.State >= EpochState.Revealed;
	}

	private static void ParseCharacterStats(List<CharacterStats> source, Dictionary<ModelId, CharacterStats> target, DeserializationContext ctx)
	{
		ctx.PushPath("CharStats");
		for (int i = 0; i < source.Count; i++)
		{
			CharacterStats characterStats = source[i];
			ctx.PushPath($"[{i}]");
			ModelId id = characterStats.Id;
			if ((object)id == null || id == ModelId.none)
			{
				ctx.Warn("Null or none character ID, skipping");
				ctx.PopPath();
				continue;
			}
			if (ModelDb.GetByIdOrNull<CharacterModel>(id) == null)
			{
				ctx.Warn($"Unknown character ID: {id}, skipping");
				ctx.PopPath();
				continue;
			}
			ClampCharacterStatsFields(characterStats, ctx);
			if (!target.TryAdd(id, characterStats))
			{
				ctx.Warn($"Duplicate character stats for {id}, merging");
				MergeCharacterStats(target[id], characterStats);
				ctx.PopPath();
			}
			else
			{
				ctx.PopPath();
			}
		}
		ctx.PopPath();
	}

	private static void ParseCardStats(List<CardStats> source, Dictionary<ModelId, CardStats> target, DeserializationContext ctx)
	{
		ctx.PushPath("CardStats");
		for (int i = 0; i < source.Count; i++)
		{
			CardStats cardStats = source[i];
			ctx.PushPath($"[{i}]");
			ModelId id = cardStats.Id;
			if ((object)id == null || id == ModelId.none)
			{
				ctx.Warn("Null or none card ID, skipping");
				ctx.PopPath();
				continue;
			}
			if (ModelDb.GetByIdOrNull<CardModel>(id) == null)
			{
				ctx.Warn($"Unknown card ID: {id}, skipping");
				ctx.PopPath();
				continue;
			}
			ClampCardStatsFields(cardStats, ctx);
			if (!target.TryAdd(id, cardStats))
			{
				ctx.Warn($"Duplicate card stats for {id}, merging");
				MergeCardStats(target[id], cardStats);
				ctx.PopPath();
			}
			else
			{
				ctx.PopPath();
			}
		}
		ctx.PopPath();
	}

	private static void ParseEncounterStats(List<EncounterStats> source, Dictionary<ModelId, EncounterStats> target, DeserializationContext ctx)
	{
		ctx.PushPath("EncounterStats");
		for (int i = 0; i < source.Count; i++)
		{
			EncounterStats encounterStats = source[i];
			ctx.PushPath($"[{i}]");
			if ((object)encounterStats.Id == null || encounterStats.Id == ModelId.none)
			{
				ctx.Warn("Null or none encounter ID, skipping");
				ctx.PopPath();
				continue;
			}
			if (ModelDb.GetByIdOrNull<EncounterModel>(encounterStats.Id) == null)
			{
				ctx.Warn($"Unknown encounter ID: {encounterStats.Id}, skipping");
				ctx.PopPath();
				continue;
			}
			ClampFightStatsFields(encounterStats.FightStats, ctx);
			if (!target.TryAdd(encounterStats.Id, encounterStats))
			{
				ctx.Warn($"Duplicate encounter stats for {encounterStats.Id}, merging");
				MergeFightStatsList(target[encounterStats.Id].FightStats, encounterStats.FightStats);
				ctx.PopPath();
			}
			else
			{
				ctx.PopPath();
			}
		}
		ctx.PopPath();
	}

	private static void ParseEnemyStats(List<EnemyStats> source, Dictionary<ModelId, EnemyStats> target, DeserializationContext ctx)
	{
		ctx.PushPath("EnemyStats");
		for (int i = 0; i < source.Count; i++)
		{
			EnemyStats enemyStats = source[i];
			ctx.PushPath($"[{i}]");
			if ((object)enemyStats.Id == null || enemyStats.Id == ModelId.none)
			{
				ctx.Warn("Null or none enemy ID, skipping");
				ctx.PopPath();
				continue;
			}
			if (ModelDb.GetByIdOrNull<MonsterModel>(enemyStats.Id) == null)
			{
				ctx.Warn($"Unknown enemy ID: {enemyStats.Id}, skipping");
				ctx.PopPath();
				continue;
			}
			ClampFightStatsFields(enemyStats.FightStats, ctx);
			if (!target.TryAdd(enemyStats.Id, enemyStats))
			{
				ctx.Warn($"Duplicate enemy stats for {enemyStats.Id}, merging");
				MergeFightStatsList(target[enemyStats.Id].FightStats, enemyStats.FightStats);
				ctx.PopPath();
			}
			else
			{
				ctx.PopPath();
			}
		}
		ctx.PopPath();
	}

	private static void ParseAncientStats(List<AncientStats> source, Dictionary<ModelId, AncientStats> target, DeserializationContext ctx)
	{
		ctx.PushPath("AncientStats");
		for (int i = 0; i < source.Count; i++)
		{
			AncientStats ancientStats = source[i];
			ctx.PushPath($"[{i}]");
			if ((object)ancientStats.Id == null || ancientStats.Id == ModelId.none)
			{
				ctx.Warn("Null or none ancient ID, skipping");
				ctx.PopPath();
				continue;
			}
			if (ModelDb.GetByIdOrNull<EventModel>(ancientStats.Id) == null)
			{
				ctx.Warn($"Unknown ancient event ID: {ancientStats.Id}, skipping");
				ctx.PopPath();
				continue;
			}
			ClampAncientCharacterStatsFields(ancientStats.CharStats, ctx);
			if (!target.TryAdd(ancientStats.Id, ancientStats))
			{
				ctx.Warn($"Duplicate ancient stats for {ancientStats.Id}, merging");
				MergeAncientCharacterStatsList(target[ancientStats.Id].CharStats, ancientStats.CharStats);
				ctx.PopPath();
			}
			else
			{
				ctx.PopPath();
			}
		}
		ctx.PopPath();
	}

	private static void ParseDiscoveredSet<TModel>(List<ModelId> source, HashSet<ModelId> target, string fieldName, DeserializationContext ctx) where TModel : AbstractModel
	{
		ctx.PushPath(fieldName);
		for (int i = 0; i < source.Count; i++)
		{
			ModelId modelId = source[i];
			if ((object)modelId == null || modelId == ModelId.none)
			{
				ctx.Warn($"Null or none ID at index {i}, skipping");
			}
			else if (ModelDb.GetByIdOrNull<TModel>(modelId) == null)
			{
				ctx.Warn($"Unknown ID: {modelId}, skipping");
			}
			else if (!target.Add(modelId))
			{
				ctx.Warn($"Duplicate ID: {modelId}, skipping");
			}
		}
		ctx.PopPath();
	}

	private static void ParseEpochs(List<SerializableEpoch> source, List<SerializableEpoch> target, DeserializationContext ctx)
	{
		ctx.PushPath("Epochs");
		HashSet<string> hashSet = new HashSet<string>();
		for (int i = 0; i < source.Count; i++)
		{
			SerializableEpoch serializableEpoch = source[i];
			ctx.PushPath($"[{i}]");
			if (!EpochModel.IsValid(serializableEpoch.Id))
			{
				ctx.Warn("Unknown epoch ID: " + serializableEpoch.Id + ", skipping");
				ctx.PopPath();
			}
			else if (!Enum.IsDefined(serializableEpoch.State))
			{
				ctx.Warn($"Invalid epoch state {serializableEpoch.State} for {serializableEpoch.Id}, skipping");
				ctx.PopPath();
			}
			else if (serializableEpoch.State < EpochState.NotObtained)
			{
				ctx.Warn($"Epoch {serializableEpoch.Id} has unused state {serializableEpoch.State}, skipping");
				ctx.PopPath();
			}
			else if (!hashSet.Add(serializableEpoch.Id))
			{
				ctx.Warn("Duplicate epoch ID: " + serializableEpoch.Id + ", keeping first");
				ctx.PopPath();
			}
			else
			{
				target.Add(serializableEpoch);
				ctx.PopPath();
			}
		}
		ctx.PopPath();
	}

	private static void FixMissingSlots(List<SerializableEpoch> epochs, DeserializationContext ctx)
	{
		Dictionary<string, SerializableEpoch> dictionary = new Dictionary<string, SerializableEpoch>();
		foreach (SerializableEpoch epoch in epochs)
		{
			dictionary[epoch.Id] = epoch;
		}
		List<SerializableEpoch> list = epochs.ToList();
		foreach (SerializableEpoch item in list)
		{
			if (item.State < EpochState.Revealed)
			{
				continue;
			}
			EpochModel epochModel = EpochModel.Get(item.Id);
			EpochModel[] timelineExpansion = epochModel.GetTimelineExpansion();
			foreach (EpochModel epochModel2 in timelineExpansion)
			{
				if (dictionary.TryGetValue(epochModel2.Id, out var value))
				{
					if (value.State == EpochState.ObtainedNoSlot)
					{
						ctx.Warn($"Epoch {epochModel2.Id} was ObtainedNoSlot but parent {item.Id} is Revealed, promoting to Obtained");
						value.State = EpochState.Obtained;
					}
				}
				else
				{
					ctx.Warn($"Epoch {epochModel2.Id} slot missing but parent {item.Id} is Revealed, creating as NotObtained");
					SerializableEpoch serializableEpoch = new SerializableEpoch(epochModel2.Id, EpochState.NotObtained);
					epochs.Add(serializableEpoch);
					dictionary[epochModel2.Id] = serializableEpoch;
				}
			}
		}
	}

	private static void ParseFtues(List<string> source, HashSet<string> target, DeserializationContext ctx)
	{
		ctx.PushPath("FtueCompleted");
		for (int i = 0; i < source.Count; i++)
		{
			if (!target.Add(source[i]))
			{
				ctx.Warn("Duplicate FTUE: " + source[i] + ", skipping");
			}
		}
		ctx.PopPath();
	}

	private static void ParseAchievements(List<SerializableUnlockedAchievement>? source, Dictionary<Achievement, long> target, DeserializationContext ctx)
	{
		if (source == null)
		{
			return;
		}
		ctx.PushPath("UnlockedAchievements");
		for (int i = 0; i < source.Count; i++)
		{
			SerializableUnlockedAchievement serializableUnlockedAchievement = source[i];
			if (!AchievementsByName.TryGetValue(serializableUnlockedAchievement.Achievement, out var value))
			{
				ctx.Warn($"Unknown achievement \"{serializableUnlockedAchievement.Achievement}\" at index {i}, skipping");
			}
			else if (!target.TryAdd(value, serializableUnlockedAchievement.UnlockTime))
			{
				ctx.Warn($"Duplicate achievement {value} at index {i}, keeping first");
			}
		}
		ctx.PopPath();
	}

	private static Dictionary<string, Achievement> BuildAchievementLookup()
	{
		Dictionary<string, Achievement> dictionary = new Dictionary<string, Achievement>();
		Achievement[] values = Enum.GetValues<Achievement>();
		for (int i = 0; i < values.Length; i++)
		{
			Achievement value = values[i];
			string key = JsonNamingPolicy.SnakeCaseLower.ConvertName(value.ToString());
			dictionary[key] = value;
		}
		return dictionary;
	}

	private static void ClampCharacterStatsFields(CharacterStats stats, DeserializationContext ctx)
	{
		if (stats.TotalWins < 0)
		{
			ctx.Warn($"Negative TotalWins ({stats.TotalWins}), clamping to 0");
			stats.TotalWins = 0;
		}
		if (stats.TotalLosses < 0)
		{
			ctx.Warn($"Negative TotalLosses ({stats.TotalLosses}), clamping to 0");
			stats.TotalLosses = 0;
		}
		if (stats.Playtime < 0)
		{
			ctx.Warn($"Negative Playtime ({stats.Playtime}), clamping to 0");
			stats.Playtime = 0L;
		}
		if (stats.MaxAscension < 0)
		{
			ctx.Warn($"Negative MaxAscension ({stats.MaxAscension}), clamping to 0");
			stats.MaxAscension = 0;
		}
		if (stats.MaxAscension > 10)
		{
			ctx.Warn($"MaxAscension ({stats.MaxAscension}) exceeds allowed ({10}), clamping");
			stats.MaxAscension = 10;
		}
		if (stats.PreferredAscension < 0)
		{
			ctx.Warn($"Negative PreferredAscension ({stats.PreferredAscension}), clamping to 0");
			stats.PreferredAscension = 0;
		}
		if (stats.PreferredAscension > 10)
		{
			ctx.Warn($"PreferredAscension ({stats.PreferredAscension}) exceeds allowed ({10}), clamping");
			stats.PreferredAscension = 10;
		}
		if (stats.BestWinStreak < 0)
		{
			ctx.Warn($"Negative BestWinStreak ({stats.BestWinStreak}), clamping to 0");
			stats.BestWinStreak = 0L;
		}
		if (stats.CurrentWinStreak < 0)
		{
			ctx.Warn($"Negative CurrentWinStreak ({stats.CurrentWinStreak}), clamping to 0");
			stats.CurrentWinStreak = 0L;
		}
		if (stats.FastestWinTime < -1)
		{
			ctx.Warn($"Invalid FastestWinTime ({stats.FastestWinTime}), resetting to -1");
			stats.FastestWinTime = -1L;
		}
	}

	private static void ClampCardStatsFields(CardStats stats, DeserializationContext ctx)
	{
		if (stats.TimesPicked < 0)
		{
			ctx.Warn($"Negative TimesPicked ({stats.TimesPicked}), clamping to 0");
			stats.TimesPicked = 0L;
		}
		if (stats.TimesSkipped < 0)
		{
			ctx.Warn($"Negative TimesSkipped ({stats.TimesSkipped}), clamping to 0");
			stats.TimesSkipped = 0L;
		}
		if (stats.TimesWon < 0)
		{
			ctx.Warn($"Negative TimesWon ({stats.TimesWon}), clamping to 0");
			stats.TimesWon = 0L;
		}
		if (stats.TimesLost < 0)
		{
			ctx.Warn($"Negative TimesLost ({stats.TimesLost}), clamping to 0");
			stats.TimesLost = 0L;
		}
	}

	private static void ClampFightStatsFields(List<FightStats>? fightStats, DeserializationContext ctx)
	{
		if (fightStats == null)
		{
			return;
		}
		for (int num = fightStats.Count - 1; num >= 0; num--)
		{
			FightStats fight = fightStats[num];
			ctx.PushPath($"FightStats[{num}]");
			if (fight.Character == ModelId.none || ModelDb.GetByIdOrNull<CharacterModel>(fight.Character) == null)
			{
				ctx.Warn($"Unknown character ID: {fight.Character}, removing");
				fightStats.RemoveAt(num);
				ctx.PopPath();
			}
			else
			{
				FightStats fightStats2 = fightStats.Take(num).FirstOrDefault((FightStats f) => f.Character == fight.Character);
				if (fightStats2 != null)
				{
					ctx.Warn($"Duplicate character {fight.Character}, merging into earlier entry");
					fightStats2.Wins = Math.Max(fightStats2.Wins, fight.Wins);
					fightStats2.Losses = Math.Max(fightStats2.Losses, fight.Losses);
					fightStats.RemoveAt(num);
					ctx.PopPath();
				}
				else
				{
					if (fight.Wins < 0)
					{
						ctx.Warn($"Negative Wins ({fight.Wins}), clamping to 0");
						fight.Wins = 0;
					}
					if (fight.Losses < 0)
					{
						ctx.Warn($"Negative Losses ({fight.Losses}), clamping to 0");
						fight.Losses = 0;
					}
					ctx.PopPath();
				}
			}
		}
	}

	private static void ClampAncientCharacterStatsFields(List<AncientCharacterStats>? charStats, DeserializationContext ctx)
	{
		if (charStats == null)
		{
			return;
		}
		for (int num = charStats.Count - 1; num >= 0; num--)
		{
			AncientCharacterStats stats = charStats[num];
			ctx.PushPath($"CharStats[{num}]");
			if (stats.Character == ModelId.none || ModelDb.GetByIdOrNull<CharacterModel>(stats.Character) == null)
			{
				ctx.Warn($"Unknown character ID: {stats.Character}, removing");
				charStats.RemoveAt(num);
				ctx.PopPath();
			}
			else
			{
				AncientCharacterStats ancientCharacterStats = charStats.Take(num).FirstOrDefault((AncientCharacterStats c) => c.Character == stats.Character);
				if (ancientCharacterStats != null)
				{
					ctx.Warn($"Duplicate character {stats.Character}, merging into earlier entry");
					ancientCharacterStats.Wins = Math.Max(ancientCharacterStats.Wins, stats.Wins);
					ancientCharacterStats.Losses = Math.Max(ancientCharacterStats.Losses, stats.Losses);
					charStats.RemoveAt(num);
					ctx.PopPath();
				}
				else
				{
					if (stats.Wins < 0)
					{
						ctx.Warn($"Negative Wins ({stats.Wins}), clamping to 0");
						stats.Wins = 0;
					}
					if (stats.Losses < 0)
					{
						ctx.Warn($"Negative Losses ({stats.Losses}), clamping to 0");
						stats.Losses = 0;
					}
					ctx.PopPath();
				}
			}
		}
	}

	private static void MergeCharacterStats(CharacterStats existing, CharacterStats incoming)
	{
		existing.TotalWins = Math.Max(existing.TotalWins, incoming.TotalWins);
		existing.TotalLosses = Math.Max(existing.TotalLosses, incoming.TotalLosses);
		existing.Playtime = Math.Max(existing.Playtime, incoming.Playtime);
		existing.MaxAscension = Math.Max(existing.MaxAscension, incoming.MaxAscension);
		existing.BestWinStreak = Math.Max(existing.BestWinStreak, incoming.BestWinStreak);
		existing.CurrentWinStreak = Math.Max(existing.CurrentWinStreak, incoming.CurrentWinStreak);
		existing.PreferredAscension = Math.Max(existing.PreferredAscension, incoming.PreferredAscension);
		existing.FastestWinTime = MergeFastestWinTime(existing.FastestWinTime, incoming.FastestWinTime);
	}

	private static long MergeFastestWinTime(long a, long b)
	{
		if (a == -1)
		{
			return b;
		}
		if (b == -1)
		{
			return a;
		}
		return Math.Min(a, b);
	}

	private static void MergeCardStats(CardStats existing, CardStats incoming)
	{
		existing.TimesPicked = Math.Max(existing.TimesPicked, incoming.TimesPicked);
		existing.TimesSkipped = Math.Max(existing.TimesSkipped, incoming.TimesSkipped);
		existing.TimesWon = Math.Max(existing.TimesWon, incoming.TimesWon);
		existing.TimesLost = Math.Max(existing.TimesLost, incoming.TimesLost);
	}

	private static void MergeFightStatsList(List<FightStats>? existing, List<FightStats>? incoming)
	{
		if (existing == null || incoming == null)
		{
			return;
		}
		foreach (FightStats incomingFight in incoming)
		{
			FightStats fightStats = existing.FirstOrDefault((FightStats f) => f.Character == incomingFight.Character);
			if (fightStats != null)
			{
				fightStats.Wins = Math.Max(fightStats.Wins, incomingFight.Wins);
				fightStats.Losses = Math.Max(fightStats.Losses, incomingFight.Losses);
			}
			else
			{
				existing.Add(incomingFight);
			}
		}
	}

	private static void MergeAncientCharacterStatsList(List<AncientCharacterStats>? existing, List<AncientCharacterStats>? incoming)
	{
		if (existing == null || incoming == null)
		{
			return;
		}
		foreach (AncientCharacterStats incomingStats in incoming)
		{
			AncientCharacterStats ancientCharacterStats = existing.FirstOrDefault((AncientCharacterStats c) => c.Character == incomingStats.Character);
			if (ancientCharacterStats != null)
			{
				ancientCharacterStats.Wins = Math.Max(ancientCharacterStats.Wins, incomingStats.Wins);
				ancientCharacterStats.Losses = Math.Max(ancientCharacterStats.Losses, incomingStats.Losses);
			}
			else
			{
				existing.Add(incomingStats);
			}
		}
	}

	private static ModelId ValidateModelId<TModel>(ModelId id, string fieldName, DeserializationContext ctx) where TModel : AbstractModel
	{
		if (id == ModelId.none)
		{
			return ModelId.none;
		}
		if (ModelDb.GetByIdOrNull<TModel>(id) == null)
		{
			ctx.PushPath(fieldName);
			ctx.Warn($"Unknown ID: {id}, resetting to none");
			ctx.PopPath();
			return ModelId.none;
		}
		return id;
	}

	private static long ClampNonNegative(long value, string fieldName, DeserializationContext ctx)
	{
		if (value >= 0)
		{
			return value;
		}
		ctx.PushPath(fieldName);
		ctx.Warn($"Negative value ({value}), clamping to 0");
		ctx.PopPath();
		return 0L;
	}

	private static int ClampNonNegativeInt(int value, string fieldName, DeserializationContext ctx)
	{
		if (value >= 0)
		{
			return value;
		}
		ctx.PushPath(fieldName);
		ctx.Warn($"Negative value ({value}), clamping to 0");
		ctx.PopPath();
		return 0;
	}

	private static int ClampAscension(int value, string fieldName, DeserializationContext ctx)
	{
		value = ClampNonNegativeInt(value, fieldName, ctx);
		if (value > 10)
		{
			ctx.PushPath(fieldName);
			ctx.Warn($"Value ({value}) exceeds allowed ({10}), clamping");
			ctx.PopPath();
			return 10;
		}
		return value;
	}

	private void FilterAndSortEpochs()
	{
		int num = _epochs.RemoveAll((SerializableEpoch e) => !EpochModel.IsValid(e.Id));
		if (num > 0)
		{
			Log.Warn($"Removed {num} invalid epoch(s) from progress state");
		}
		_epochs.Sort(new EpochComparer());
	}
}
