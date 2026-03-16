using System.Text.Json.Serialization;

namespace MegaCrit.Sts2.Core.Saves;

public record SerializableUnlockedAchievement
{
	[JsonPropertyName("achievement")]
	public string Achievement { get; init; } = "";

	[JsonPropertyName("unlock_time")]
	public long UnlockTime { get; init; }
}
