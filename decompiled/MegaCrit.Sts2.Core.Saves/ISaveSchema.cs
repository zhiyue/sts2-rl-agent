using System.Text.Json.Serialization;

namespace MegaCrit.Sts2.Core.Saves;

/// <summary>
/// Interface for all save schemas that can be serialized and migrated.
/// </summary>
public interface ISaveSchema
{
	/// <summary>
	/// The version of the schema, used for migration purposes.
	/// </summary>
	[JsonPropertyName("schema_version")]
	int SchemaVersion { get; set; }
}
