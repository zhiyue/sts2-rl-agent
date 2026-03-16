using System;
using System.Text.Json.Serialization;

namespace MegaCrit.Sts2.GameInfo.Objects;

[Serializable]
public class DailyMods : IGameInfo
{
	[JsonPropertyName("name")]
	public required string Name { get; init; }

	[JsonPropertyName("bot_keyword")]
	public required string BotKeyword { get; init; }

	[JsonPropertyName("bot_text")]
	public required string BotText { get; init; }

	[JsonPropertyName("text")]
	public required string Text { get; init; }
}
