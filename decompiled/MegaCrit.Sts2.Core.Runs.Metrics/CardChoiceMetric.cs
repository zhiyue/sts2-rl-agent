using System.Collections.Generic;
using MegaCrit.Sts2.Core.Runs.History;

namespace MegaCrit.Sts2.Core.Runs.Metrics;

public struct CardChoiceMetric
{
	public readonly List<string> picked;

	public readonly List<string> skipped;

	public CardChoiceMetric(List<CardChoiceHistoryEntry> choices)
	{
		picked = new List<string>();
		skipped = new List<string>();
		foreach (CardChoiceHistoryEntry choice in choices)
		{
			if (choice.wasPicked)
			{
				picked.Add(choice.Card.Id.Entry);
			}
			else
			{
				skipped.Add(choice.Card.Id.Entry);
			}
		}
	}
}
