using System;
using System.Text.Json.Serialization;

namespace MegaCrit.Sts2.GameInfo.Objects;

[Serializable]
public class AncientChoiceInfo : IGameInfo
{
	[JsonPropertyName("name")]
	public required string Name { get; init; }

	[JsonPropertyName("bot_keyword")]
	public required string BotKeyword { get; init; }

	/// This is for feedback bot (the discord bot), the text here is what it will display when called.
	[JsonPropertyName("bot_text")]
	public required string BotText { get; init; }

	/// The id for the ancient choice
	[JsonPropertyName("id")]
	public required string Id { get; init; }

	/// This is for the metrics system, the text here is what will be displayed in the metrics website screen.
	[JsonPropertyName("text")]
	public required string Text { get; init; }

	/// Represents the ancient's id
	[JsonPropertyName("ancient")]
	public required string Ancient { get; init; }
}
