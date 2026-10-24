using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Saves;

/// <summary>
/// Holds progress data on a per enemy creature basis.
/// Used by SerializableProgress. Useful for the Bestiary, Statistics, Achievements, and Epochs.
/// </summary>
public class EnemyStats
{
	/// <summary>
	/// The Id of this enemy!
	/// </summary>
	[JsonPropertyName("enemy_id")]
	public required ModelId Id { get; init; }

	[JsonPropertyName("fight_stats")]
	public List<FightStats> FightStats { get; init; } = new List<FightStats>();

	/// <summary>
	/// How many times the player has won against this enemy.
	/// </summary>
	[JsonIgnore]
	public int TotalWins
	{
		get
		{
			if (FightStats.Count == 0)
			{
				return 0;
			}
			return FightStats.Sum((FightStats f) => f.Wins);
		}
	}

	/// <summary>
	/// How many times the player has died to this enemy.
	/// </summary>
	[JsonIgnore]
	public int TotalLosses
	{
		get
		{
			if (FightStats.Count == 0)
			{
				return 0;
			}
			return FightStats.Sum((FightStats f) => f.Losses);
		}
	}

	/// <summary>
	/// Increments a victory against this enemy.
	/// </summary>
	public void IncrementWin(ModelId characterId)
	{
		FightStats fightStats = FightStats.First((FightStats fight) => fight.Character == characterId);
		fightStats.Wins++;
		Log.Info($"{characterId} has killed a {Id}. That's {fightStats.Wins} kills");
	}

	/// <summary>
	/// Increments a loss against this enemy.
	/// </summary>
	public void IncrementLoss(ModelId characterId)
	{
		FightStats fightStats = FightStats.First((FightStats fight) => fight.Character == characterId);
		fightStats.Losses++;
		Log.Info($"{characterId} has died to a {Id}. That's {fightStats.Losses} losses");
	}
}
