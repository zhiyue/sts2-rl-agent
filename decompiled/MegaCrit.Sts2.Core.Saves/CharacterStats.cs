using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Saves;

public class CharacterStats
{
	[JsonPropertyName("id")]
	public ModelId? Id { get; init; }

	[JsonPropertyName("max_ascension")]
	public int MaxAscension { get; set; }

	[JsonPropertyName("preferred_ascension")]
	public int PreferredAscension { get; set; }

	[JsonPropertyName("total_wins")]
	public int TotalWins { get; set; }

	[JsonPropertyName("total_losses")]
	public int TotalLosses { get; set; }

	[JsonPropertyName("fastest_win_time")]
	public long FastestWinTime { get; set; } = -1L;

	[JsonPropertyName("best_win_streak")]
	public long BestWinStreak { get; set; }

	[JsonPropertyName("current_streak")]
	public long CurrentWinStreak { get; set; }

	[JsonPropertyName("playtime")]
	public long Playtime { get; set; }
}
