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

	[JsonPropertyName("bot_text")]
	public required string BotText { get; init; }

	[JsonPropertyName("id")]
	public required ModelId Id { get; init; }

	[JsonPropertyName("rarity")]
	public required string Rarity { get; init; }

	[JsonPropertyName("text")]
	public required string Text { get; init; }

	[JsonPropertyName("color")]
	public required string Color { get; init; }
}
