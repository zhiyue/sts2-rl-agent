using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Saves;

public class AncientStats
{
	[JsonPropertyName("ancient_id")]
	public required ModelId Id { get; init; }

	[JsonPropertyName("character_stats")]
	public List<AncientCharacterStats> CharStats { get; init; } = new List<AncientCharacterStats>();

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

	public int GetVisitsAs(ModelId characterId)
	{
		return GetStats(characterId)?.Visits ?? 0;
	}

	private AncientCharacterStats? GetStats(ModelId characterId)
	{
		return CharStats.FirstOrDefault((AncientCharacterStats c) => c.Character == characterId);
	}
}
