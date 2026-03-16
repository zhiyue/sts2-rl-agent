using System.Text.Json.Serialization;

namespace MegaCrit.Sts2.Core.Platform.Null;

public record NullMultiplayerName
{
	[JsonPropertyName("net_id")]
	public ulong netId;

	[JsonPropertyName("name")]
	public string name = "";
}
