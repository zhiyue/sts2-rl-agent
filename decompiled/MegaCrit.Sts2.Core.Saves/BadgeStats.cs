using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Models.Badges;

namespace MegaCrit.Sts2.Core.Saves;

/// <summary>
/// Holds progress data on a single Badge.
/// Represents which badges a character has acquired.
/// Used by CharacterStats
/// </summary>
public class BadgeStats
{
	/// <summary>
	/// The badge this BadgeStat entry is for.
	/// </summary>
	[JsonPropertyName("id")]
	public required string Id { get; init; }

	/// <summary>
	/// How many times this character has acquired this badge.
	/// </summary>
	[JsonPropertyName("count")]
	public required int Count { get; set; }

	/// <summary>
	/// The rarity of this badge.
	/// </summary>
	[JsonPropertyName("rarity")]
	public required BadgeRarity Rarity { get; set; }
}
