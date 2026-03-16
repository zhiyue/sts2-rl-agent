using System;
using System.Text.Json.Serialization;

namespace MegaCrit.Sts2.Core.Debug;

public class ReleaseInfo
{
	[JsonPropertyName("commit")]
	public required string Commit { get; init; }

	[JsonPropertyName("version")]
	public required string Version { get; init; }

	[JsonPropertyName("date")]
	[JsonConverter(typeof(CustomDateTimeConverter))]
	public required DateTime Date { get; init; }

	[JsonPropertyName("branch")]
	public required string Branch { get; init; }
}
