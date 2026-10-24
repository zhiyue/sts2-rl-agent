using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Saves;

/// <summary>
/// Holds progress data on a per ancient basis.
/// </summary>
public class AncientStats
{
	/// <summary>
	/// The Id of this ancient!
	/// </summary>
	[JsonPropertyName("ancient_id")]
	public required ModelId Id { get; init; }

	[JsonPropertyName("character_stats")]
	public List<AncientCharacterStats> CharStats { get; init; } = new List<AncientCharacterStats>();

	/// <summary>
	/// Total number of visits to this ancient across all characters.
	/// </summary>
	[JsonIgnore]
	public int TotalVisits
	{
		get
		{
			if (CharStats.Count == 0)
			{
				return 0;
			}
			return CharStats.Sum((AncientCharacterStats c) => c.Visits);
		}
	}

	/// <summary>
	/// How many times the player has won a run after encountering this ancient in it.
	/// </summary>
	[JsonIgnore]
	public int TotalWins
	{
		get
		{
			if (CharStats.Count == 0)
			{
				return 0;
			}
			return CharStats.Sum((AncientCharacterStats fight) => fight.Wins);
		}
	}

	/// <summary>
	/// How many times the player has lost a run after encountering this ancient in it.
	/// </summary>
	[JsonIgnore]
	public int TotalLosses
	{
		get
		{
			if (CharStats.Count == 0)
			{
				return 0;
			}
			return CharStats.Sum((AncientCharacterStats fight) => fight.Losses);
		}
	}

	/// <summary>
	/// Increments a run victory with this ancient.
	/// </summary>
	public void IncrementWin(ModelId characterId)
	{
		AncientCharacterStats ancientCharacterStats = GetStats(characterId);
		if (ancientCharacterStats == null)
		{
			ancientCharacterStats = new AncientCharacterStats
			{
				Character = characterId
			};
			CharStats.Add(ancientCharacterStats);
		}
		ancientCharacterStats.Wins++;
		Log.Info($"{characterId} has won a run with ancient {Id}. That's {ancientCharacterStats.Wins} wins");
	}

	/// <summary>
	/// Increments a run loss with this ancient.
	/// </summary>
	public void IncrementLoss(ModelId characterId)
	{
		AncientCharacterStats ancientCharacterStats = GetStats(characterId);
		if (ancientCharacterStats == null)
		{
			ancientCharacterStats = new AncientCharacterStats
			{
				Character = characterId
			};
			CharStats.Add(ancientCharacterStats);
		}
		ancientCharacterStats.Losses++;
	}

	/// <summary>
	/// Get the number of times the player has visited this ancient as the specified character.
	/// </summary>
	public int GetVisitsAs(ModelId characterId)
	{
		return GetStats(characterId)?.Visits ?? 0;
	}

	/// <summary>
	/// Get the stats for this ancient and the specified character.
	/// </summary>
	private AncientCharacterStats? GetStats(ModelId characterId)
	{
		return CharStats.FirstOrDefault((AncientCharacterStats c) => c.Character == characterId);
	}
}
