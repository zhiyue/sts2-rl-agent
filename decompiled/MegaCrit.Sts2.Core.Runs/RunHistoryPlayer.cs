using System.Collections.Generic;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Runs;

public class RunHistoryPlayer
{
	[JsonPropertyName("id")]
	public ulong Id { get; init; }

	[JsonPropertyName("character")]
	public ModelId Character { get; init; } = ModelId.none;

	[JsonPropertyName("deck")]
	public IEnumerable<SerializableCard> Deck { get; set; } = new List<SerializableCard>();

	[JsonPropertyName("relics")]
	public IEnumerable<SerializableRelic> Relics { get; set; } = new List<SerializableRelic>();

	[JsonPropertyName("potions")]
	public IEnumerable<SerializablePotion> Potions { get; set; } = new List<SerializablePotion>();

	[JsonPropertyName("badges")]
	public IEnumerable<SerializableBadge> Badges { get; set; } = new List<SerializableBadge>();

	[JsonPropertyName("max_potion_slot_count")]
	public int MaxPotionSlotCount { get; set; } = 3;
}
