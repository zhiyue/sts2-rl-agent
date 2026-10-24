using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Saves;

/// <summary>
/// Holds progress data of a specific character's statistics with an ancient.
/// </summary>
public class AncientCharacterStats
{
	/// <summary>
	/// The character this stats entry is for.
	/// </summary>
	[JsonPropertyName("character")]
	public required ModelId Character { get; init; }

	/// <summary>
	/// How many times this character has won a run after encountering this ancient.
	/// </summary>
	[JsonPropertyName("wins")]
	public int Wins { get; set; }

	/// <summary>
	/// How many times this character has lost a run after encountering this ancient.
	/// </summary>
	[JsonPropertyName("losses")]
	public int Losses { get; set; }

	/// <summary>
	/// How many times this character has visited this ancient.
	/// </summary>
	[JsonIgnore]
	public int Visits => Wins + Losses;
}
