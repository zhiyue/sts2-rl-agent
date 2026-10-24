using System.Linq;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Runs.Metrics;

public struct EventChoiceMetric
{
	public readonly string id;

	public readonly string act;

	public readonly string picked;

	public EventChoiceMetric(MapPointHistoryEntry entry, ulong playerId, SerializableActModel actModel)
	{
		id = entry.Rooms.First().ModelId.Entry;
		act = actModel.Id.Entry;
		LocString title = entry.GetEntry(playerId).EventChoices.Last().Title;
		picked = title.LocEntryKey.Split(".")[^2];
	}
}
