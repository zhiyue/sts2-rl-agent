using System.Collections.Generic;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Platform.Null;

public class NullLeaderboardFile : ISaveSchema
{
	[JsonPropertyName("leaderboards")]
	public List<NullLeaderboard> leaderboards = new List<NullLeaderboard>();

	[JsonPropertyName("version")]
	public int SchemaVersion { get; set; }
}
