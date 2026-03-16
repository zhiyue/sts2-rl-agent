using System.Text.Json.Serialization;

namespace MegaCrit.Sts2.Core.Achievements;

public class AchievementMetric
{
	[JsonPropertyName("buildId")]
	public required string BuildId { get; set; }

	[JsonPropertyName("achievement")]
	public required string Achievement { get; set; }

	[JsonPropertyName("totalAchievements")]
	public int TotalAchievements { get; set; }

	[JsonPropertyName("totalPlaytime")]
	public long TotalPlaytime { get; set; }

	[JsonPropertyName("totalRuns")]
	public int TotalRuns { get; set; }
}
