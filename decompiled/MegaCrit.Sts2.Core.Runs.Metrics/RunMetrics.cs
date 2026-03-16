using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Runs.Metrics;

public class RunMetrics
{
	public required string BuildId { get; init; }

	public required string PlayerId { get; init; }

	public required ModelId Character { get; init; }

	public required bool Win { get; init; }

	public required int NumPlayers { get; init; }

	public required List<ModelId> Team { get; init; }

	public string BuildType { get; } = "Beta";

	public int Ascension { get; init; }

	public required float TotalPlaytime { get; init; }

	public required float TotalWinRate { get; init; }

	public required float RunPlaytime { get; init; }

	public required int FloorReached { get; init; }

	public required ModelId KilledByEncounter { get; init; }

	public required List<CardChoiceMetric> CardChoices { get; init; }

	public required List<string> CampfireUpgrades { get; init; }

	public required List<EventChoiceMetric> EventChoices { get; init; }

	public required List<AncientMetric> AncientChoices { get; init; }

	public required List<string> RelicBuys { get; init; }

	public required List<string> PotionBuys { get; init; }

	public required List<string> ColorlessBuys { get; init; }

	public required List<string> PotionDiscards { get; init; }

	public required List<EncounterMetric> Encounters { get; init; }

	public required List<ActWinMetric> ActWins { get; init; }

	public required IEnumerable<ModelId> Deck { get; init; }

	public required IEnumerable<ModelId> Relics { get; init; }
}
