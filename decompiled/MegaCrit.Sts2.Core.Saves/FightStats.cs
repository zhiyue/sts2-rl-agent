using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Saves;

/// <summary>
/// Holds progress data of a specific Character's matchup against a specific Encounter or Enemy.
/// </summary>
public class FightStats
{
	/// <summary>
	/// The character this FightStat entry is for.
	/// </summary>
	[JsonPropertyName("character")]
	public required ModelId Character { get; init; }

	/// <summary>
	/// How many times this character has won against this encounter or enemy.
	/// </summary>
	[JsonPropertyName("wins")]
	public int Wins { get; set; }

	/// <summary>
	/// How many times this character has lost to this encounter or enemy.
	/// </summary>
	[JsonPropertyName("losses")]
	public int Losses { get; set; }
}
