using System.Text.Json.Serialization;

namespace MegaCrit.Sts2.Core.Modding;

public class ModManifest
{
	[JsonPropertyName("pck_name")]
	public string? pckName;

	[JsonPropertyName("name")]
	public string? name;

	[JsonPropertyName("author")]
	public string? author;

	[JsonPropertyName("description")]
	public string? description;

	[JsonPropertyName("version")]
	public string? version;
}
