using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Saves;

public class CardStats
{
	[JsonPropertyName("id")]
	public ModelId? Id { get; init; }

	[JsonPropertyName("times_picked")]
	public long TimesPicked { get; set; }

	[JsonPropertyName("times_skipped")]
	public long TimesSkipped { get; set; }

	[JsonPropertyName("times_won")]
	public long TimesWon { get; set; }

	[JsonPropertyName("times_lost")]
	public long TimesLost { get; set; }
}
