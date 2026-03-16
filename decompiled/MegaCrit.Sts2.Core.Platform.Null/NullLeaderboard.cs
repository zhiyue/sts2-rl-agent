using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MegaCrit.Sts2.Core.Platform.Null;

public class NullLeaderboard
{
	[JsonPropertyName("name")]
	public string name = "";

	[JsonPropertyName("entries")]
	public List<NullLeaderboardFileEntry> entries = new List<NullLeaderboardFileEntry>();
}
