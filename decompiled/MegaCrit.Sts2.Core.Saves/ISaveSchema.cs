using System.Text.Json.Serialization;

namespace MegaCrit.Sts2.Core.Saves;

public interface ISaveSchema
{
	[JsonPropertyName("schema_version")]
	int SchemaVersion { get; set; }
}
