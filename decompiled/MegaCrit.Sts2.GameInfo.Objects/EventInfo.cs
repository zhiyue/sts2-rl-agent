using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.GameInfo.Objects;

[Serializable]
public class EventInfo : IGameInfo
{
	[JsonPropertyName("name")]
	public required string Name { get; init; }

	[JsonPropertyName("bot_keyword")]
	public required string BotKeyword { get; init; }

	[JsonPropertyName("bot_text")]
	public required string BotText { get; init; }

	[JsonPropertyName("id")]
	public required ModelId Id { get; init; }

	[JsonPropertyName("act")]
	public required string Act { get; init; }

	[JsonPropertyName("options")]
	public required List<string> Options { get; init; }
}
