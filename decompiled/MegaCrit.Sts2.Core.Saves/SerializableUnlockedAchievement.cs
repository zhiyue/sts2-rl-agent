using System.Text.Json.Serialization;

namespace MegaCrit.Sts2.Core.Saves;

/// <summary>
/// Records an achievement unlock in the SerializableProgress file.
/// Stores the achievement identifier as a raw string at the wire format boundary.
/// Enum resolution happens in <see cref="M:MegaCrit.Sts2.Core.Saves.ProgressState.ParseAchievements(System.Collections.Generic.List{MegaCrit.Sts2.Core.Saves.SerializableUnlockedAchievement},System.Collections.Generic.Dictionary{MegaCrit.Sts2.Core.Achievements.Achievement,System.Int64},System.Collections.Generic.List{MegaCrit.Sts2.Core.Saves.SerializableUnlockedAchievement},MegaCrit.Sts2.Core.Saves.Validation.DeserializationContext)" />.
/// </summary>
public record SerializableUnlockedAchievement
{
	[JsonPropertyName("achievement")]
	public string Achievement { get; init; } = "";

	[JsonPropertyName("unlock_time")]
	public long UnlockTime { get; init; }
}
