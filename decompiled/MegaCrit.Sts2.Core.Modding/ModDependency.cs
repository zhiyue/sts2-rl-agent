using System.Text.Json.Serialization;

namespace MegaCrit.Sts2.Core.Modding;

public class ModDependency
{
	[JsonPropertyName("id")]
	public string id;

	[JsonPropertyName("min_version")]
	public string? minVersion;

	public ModDependency(string id, string? minVersion = null)
	{
		this.id = id;
		this.minVersion = minVersion;
	}
}
