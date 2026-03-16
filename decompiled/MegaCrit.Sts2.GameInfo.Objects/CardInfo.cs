using System;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.GameInfo.Objects;

[Serializable]
public class CardInfo : IGameInfo
{
	[JsonPropertyName("name")]
	public required string Name { get; init; }

	[JsonPropertyName("bot_keyword")]
	public required string BotKeyword { get; init; }

	[JsonPropertyName("bot_text")]
	public required string BotText { get; init; }

	[JsonPropertyName("id")]
	public required ModelId Id { get; init; }

	[JsonPropertyName("upgraded")]
	public required bool Upgraded { get; init; }

	[JsonPropertyName("color")]
	public required string Color { get; init; }

	[JsonPropertyName("rarity")]
	public required string Rarity { get; init; }

	[JsonPropertyName("type")]
	public required string Type { get; init; }

	[JsonPropertyName("base_damage")]
	public required int BaseDamage { get; init; }

	[JsonPropertyName("energy")]
	public required int Energy { get; init; }

	[JsonPropertyName("star_cost")]
	public required int StarCost { get; init; }

	[JsonPropertyName("text")]
	public required string Text { get; init; }

	[JsonPropertyName("has_art")]
	public required bool HasArt { get; init; }

	[JsonPropertyName("has_joke_art")]
	public required bool HasJokeArt { get; init; }
}
