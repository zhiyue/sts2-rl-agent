using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Saves;

public class AncientCharacterStats
{
	[JsonPropertyName("character")]
	public required ModelId Character { get; init; }

	[JsonPropertyName("wins")]
	public int Wins { get; set; }

	[JsonPropertyName("losses")]
	public int Losses { get; set; }

	[JsonIgnore]
	public int Visits => Wins + Losses;
}
