using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Saves;

public class SerializableProgress : ISaveSchema
{
	[JsonPropertyName("schema_version")]
	public int SchemaVersion { get; set; }

	[JsonPropertyName("unique_id")]
	public string UniqueId { get; init; }

	[JsonPropertyName("character_stats")]
	public List<CharacterStats> CharStats { get; set; } = new List<CharacterStats>();

	[JsonPropertyName("card_stats")]
	public List<CardStats> CardStats { get; set; } = new List<CardStats>();

	[JsonPropertyName("encounter_stats")]
	public List<EncounterStats> EncounterStats { get; set; } = new List<EncounterStats>();

	[JsonPropertyName("enemy_stats")]
	public List<EnemyStats> EnemyStats { get; set; } = new List<EnemyStats>();

	[JsonPropertyName("ancient_stats")]
	public List<AncientStats> AncientStats { get; set; } = new List<AncientStats>();

	[JsonPropertyName("enable_ftues")]
	public bool EnableFtues { get; set; } = true;

	[JsonPropertyName("epochs")]
	public List<SerializableEpoch> Epochs { get; set; } = new List<SerializableEpoch>();

	[JsonPropertyName("ftue_completed")]
	public List<string> FtueCompleted { get; set; } = new List<string>();

	[JsonPropertyName("unlocked_achievements")]
	public List<SerializableUnlockedAchievement> UnlockedAchievements { get; set; } = new List<SerializableUnlockedAchievement>();

	[JsonPropertyName("discovered_cards")]
	public List<ModelId> DiscoveredCards { get; set; } = new List<ModelId>();

	[JsonPropertyName("discovered_relics")]
	public List<ModelId> DiscoveredRelics { get; set; } = new List<ModelId>();

	[JsonPropertyName("discovered_events")]
	public List<ModelId> DiscoveredEvents { get; set; } = new List<ModelId>();

	[JsonPropertyName("discovered_potions")]
	public List<ModelId> DiscoveredPotions { get; set; } = new List<ModelId>();

	[JsonPropertyName("discovered_acts")]
	public List<ModelId> DiscoveredActs { get; set; } = new List<ModelId>();

	[JsonPropertyName("total_playtime")]
	public long TotalPlaytime { get; set; }

	[JsonPropertyName("total_unlocks")]
	public int TotalUnlocks { get; set; }

	[JsonPropertyName("current_score")]
	public int CurrentScore { get; set; }

	[JsonPropertyName("floors_climbed")]
	public long FloorsClimbed { get; set; }

	[JsonPropertyName("architect_damage")]
	public long ArchitectDamage { get; set; }

	[JsonPropertyName("wongo_points")]
	public int WongoPoints { get; set; }

	[JsonPropertyName("preferred_multiplayer_ascension")]
	public int PreferredMultiplayerAscension { get; set; }

	[JsonPropertyName("max_multiplayer_ascension")]
	public int MaxMultiplayerAscension { get; set; }

	[JsonPropertyName("test_subject_kills")]
	public int TestSubjectKills { get; set; }

	[JsonPropertyName("pending_character_unlock")]
	public ModelId PendingCharacterUnlock { get; set; } = ModelId.none;

	[JsonIgnore]
	public int Wins => CharStats.Sum((CharacterStats character) => character.TotalWins);

	[JsonIgnore]
	public int Losses => CharStats.Sum((CharacterStats character) => character.TotalLosses);

	[JsonIgnore]
	public long FastestVictory
	{
		get
		{
			if (CharStats.Count == 0)
			{
				return 999999999L;
			}
			return CharStats.Min((CharacterStats c) => (c.FastestWinTime != -1) ? c.FastestWinTime : 999999999);
		}
	}

	[JsonIgnore]
	public long BestWinStreak
	{
		get
		{
			if (CharStats.Count == 0)
			{
				return 0L;
			}
			return CharStats.Max((CharacterStats c) => c.BestWinStreak);
		}
	}

	[JsonIgnore]
	public int NumberOfRuns => Wins + Losses;

	public SerializableProgress()
	{
		UniqueId = GenerateUniqueId();
		static string GenerateUniqueId(int length = 7)
		{
			return new string((from s in Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", length)
				select s[Rng.Chaotic.NextInt(s.Length)]).ToArray());
		}
	}

	public CharacterStats? GetStatsForCharacter(ModelId characterId)
	{
		return CharStats.FirstOrDefault((CharacterStats c) => c.Id == characterId);
	}

	public AncientStats? GetStatsForAncient(ModelId ancientId)
	{
		return AncientStats.FirstOrDefault((AncientStats a) => a.Id == ancientId);
	}
}
