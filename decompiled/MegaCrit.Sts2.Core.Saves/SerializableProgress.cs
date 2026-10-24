using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Saves;

/// <summary>
/// The player's gameplay progress for Slay the Spire 2.
/// This would be considered the most important save file and is synced across devices/platforms.
/// There would be a progress.save in each respective profile's folder and contains data such as:
/// - Total wins, losses, playtime, and progress per character
/// - Which cards/relics/potions/etc have been seen or unlocked
/// - Which enemies/elites/bosses have been encountered and defeated
/// - Character data
/// - Timeline and Unlock progress (meta progression)
/// - Unlocked achievements
/// - Which FTUEs this player has seen
/// </summary>
public class SerializableProgress : ISaveSchema
{
	/// <summary>
	/// The schema version of this save.
	/// </summary>
	[JsonPropertyName("schema_version")]
	public int SchemaVersion { get; set; }

	[JsonPropertyName("unique_id")]
	public string UniqueId { get; init; }

	[JsonPropertyName("character_stats")]
	public List<CharacterStats> CharStats { get; set; } = new List<CharacterStats>();

	[JsonPropertyName("card_stats")]
	public List<CardStats> CardStats { get; set; } = new List<CardStats>();

	/// <summary>
	/// Tracks how often each character has encountered, won, and lost against each Encounter.
	/// </summary>
	[JsonPropertyName("encounter_stats")]
	public List<EncounterStats> EncounterStats { get; set; } = new List<EncounterStats>();

	/// <summary>
	/// Tracks how often each character has encountered, won, and lost against each Enemy Creature.
	/// </summary>
	[JsonPropertyName("enemy_stats")]
	public List<EnemyStats> EnemyStats { get; set; } = new List<EnemyStats>();

	/// <summary>
	/// Tracks how often each character has encountered, won, and lost after encountering each ancient.
	/// </summary>
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

	/// <summary>
	/// The amount of agnostic unlocks via score system this player has unlocked.
	/// </summary>
	[JsonPropertyName("total_unlocks")]
	public int TotalUnlocks { get; set; }

	/// <summary>
	/// How much score this player has (is not cumulative).
	/// </summary>
	[JsonPropertyName("current_score")]
	public int CurrentScore { get; set; }

	[JsonPropertyName("floors_climbed")]
	public long FloorsClimbed { get; set; }

	[JsonPropertyName("architect_damage")]
	public long ArchitectDamage { get; set; }

	[JsonPropertyName("wongo_points")]
	public int WongoPoints { get; set; }

	/// <summary> Which multiplayer ascension we had selected the last time we started a multiplayer run. </summary>
	[JsonPropertyName("preferred_multiplayer_ascension")]
	public int PreferredMultiplayerAscension { get; set; }

	/// <summary>
	/// The maximum unlocked ascension in multiplayer.
	/// In singleplayer runs, we use character stats. In multiplayer runs, all characters share an ascension.
	/// </summary>
	[JsonPropertyName("max_multiplayer_ascension")]
	public int MaxMultiplayerAscension { get; set; }

	[JsonPropertyName("test_subject_kills")]
	public int TestSubjectKills { get; set; }

	/// <summary>
	/// Characters that have been unlocked between the last run and the next time the player opens the character
	/// select screen.
	/// Used to show an animation on the character select screen.
	/// </summary>
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

	/// <summary>
	/// Get the stats for the specified character.
	/// </summary>
	public CharacterStats? GetStatsForCharacter(ModelId characterId)
	{
		return CharStats.FirstOrDefault((CharacterStats c) => c.Id == characterId);
	}

	/// <summary>
	/// Get the stats for the specified ancient.
	/// </summary>
	public AncientStats? GetStatsForAncient(ModelId ancientId)
	{
		return AncientStats.FirstOrDefault((AncientStats a) => a.Id == ancientId);
	}
}
