using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Runs.History;

namespace MegaCrit.Sts2.Core.Runs.Metrics;

public struct AncientMetric
{
	public readonly string picked;

	public readonly List<string> skipped;

	public AncientMetric(MapPointHistoryEntry entry, PlayerMapPointHistoryEntry playerEntry)
	{
		AncientChoiceHistoryEntry ancientChoiceHistoryEntry = playerEntry.AncientChoices.FirstOrDefault((AncientChoiceHistoryEntry o) => o.WasChosen);
		if (ancientChoiceHistoryEntry == null)
		{
			throw new InvalidOperationException($"Failed to find chosen ancient choice! {entry.Rooms.First().ModelId} {playerEntry.PlayerId}");
		}
		picked = ancientChoiceHistoryEntry.TextKey;
		skipped = (from o in playerEntry.AncientChoices
			where !o.WasChosen
			select o.TextKey).ToList();
	}
}
