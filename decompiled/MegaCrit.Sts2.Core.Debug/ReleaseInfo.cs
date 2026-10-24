using System;
using System.Text.Json.Serialization;

namespace MegaCrit.Sts2.Core.Debug;

/// <summary>
/// Represents release information for the game.
/// </summary>
public class ReleaseInfo
{
	/// <summary>
	/// The commit hash.
	/// </summary>
	[JsonPropertyName("commit")]
	public required string Commit { get; init; }

	/// <summary>
	/// The release version number.
	/// </summary>
	[JsonPropertyName("version")]
	public required string Version { get; init; }

	/// <summary>
	/// The release date in git's iso8601 format
	/// </summary>
	[JsonPropertyName("date")]
	[JsonConverter(typeof(CustomDateTimeConverter))]
	public required DateTime Date { get; init; }

	/// <summary>
	/// The branch name.
	/// </summary>
	[JsonPropertyName("branch")]
	public required string Branch { get; init; }

	/// <summary>
	/// A hash of the contents of the main STS2 assembly, at build-time.
	/// </summary>
	[JsonPropertyName("main_assembly_hash")]
	public required int MainAssemblyHash { get; init; }
}
