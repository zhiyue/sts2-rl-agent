using System.Text.Json.Serialization;

namespace MegaCrit.Sts2.Core.Nodes.Screens.FeedbackScreen;

public struct FeedbackData
{
	[JsonPropertyName("description")]
	public string description;

	[JsonPropertyName("category")]
	public string category;

	[JsonPropertyName("game_version")]
	public string gameVersion;

	[JsonPropertyName("unique_id")]
	public string uniqueId;

	[JsonPropertyName("commit")]
	public string commit;

	[JsonPropertyName("platform_branch")]
	public string? platformBranch;
}
