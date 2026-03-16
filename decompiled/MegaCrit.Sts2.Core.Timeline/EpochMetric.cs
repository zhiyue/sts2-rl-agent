using System.Text.Json.Serialization;

namespace MegaCrit.Sts2.Core.Timeline;

public class EpochMetric
{
	[JsonPropertyName("buildId")]
	public required string BuildId { get; set; }

	[JsonPropertyName("epoch")]
	public required string Epoch { get; set; }

	[JsonPropertyName("totalEpochs")]
	public int TotalEpochs { get; set; }

	[JsonPropertyName("totalPlaytime")]
	public long TotalPlaytime { get; set; }

	[JsonPropertyName("totalRuns")]
	public int TotalRuns { get; set; }
}
