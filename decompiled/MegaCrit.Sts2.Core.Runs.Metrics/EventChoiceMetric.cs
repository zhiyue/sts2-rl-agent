using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs.History;

namespace MegaCrit.Sts2.Core.Runs.Metrics;

public struct EventChoiceMetric
{
	public readonly string id;

	public readonly string picked;

	public EventChoiceMetric(MapPointHistoryEntry entry, ulong playerId)
	{
		id = entry.Rooms.First().ModelId.Entry;
		EventOptionHistoryEntry eventOptionHistoryEntry = entry.GetEntry(playerId).EventChoices.Last();
		LocString title = entry.GetEntry(playerId).EventChoices.Last().Title;
		ModelDb.GetById<EventModel>(entry.Rooms.First().ModelId).DynamicVars.AddTo(title);
		if (eventOptionHistoryEntry.Variables != null)
		{
			foreach (KeyValuePair<string, object> variable in eventOptionHistoryEntry.Variables)
			{
				title.AddObj(variable.Key, variable.Value);
			}
		}
		picked = title.GetFormattedText();
	}
}
