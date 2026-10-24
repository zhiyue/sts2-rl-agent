using System.Text.Json.Serialization;

namespace MegaCrit.Sts2.Core.Saves;

/// <summary>
/// This class serves as a schema for the Profile save file.
/// It's used for storing the preferred profile that the player plays as.
/// This file is stored at the account level, not at the profile level, and is synced across devices/platforms.
/// </summary>
public class ProfileSave : ISaveSchema
{
	/// <summary>
	/// The schema version of this save.
	/// </summary>
	[JsonPropertyName("schema_version")]
	public int SchemaVersion { get; set; }

	[JsonPropertyName("last_profile_id")]
	public int LastProfileId { get; set; } = 1;
}
