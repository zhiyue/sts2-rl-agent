using System;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.GameInfo.Objects;

[Serializable]
public class RelicInfo : IGameInfo
{
	[JsonPropertyName("name")]
	public required string Name { get; init; }

	[JsonPropertyName("bot_keyword")]
	public required string BotKeyword { get; init; }

	/// This is for feedback bot (the discord bot), the text here is what it will display when called.
	[JsonPropertyName("bot_text")]
	public required string BotText { get; init; }

	[JsonPropertyName("id")]
	public required ModelId Id { get; init; }

	[JsonPropertyName("rarity")]
	public required string Rarity { get; init; }

	/// This is for the metrics system, the text here is what will be displayed in the metrics website screen.
	[JsonPropertyName("text")]
	public required string Text { get; init; }

	[JsonPropertyName("color")]
	public required string Color { get; init; }
}
