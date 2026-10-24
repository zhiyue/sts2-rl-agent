using System.Collections.Generic;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Models.Badges;

/// <summary>
/// Traveled to 15 or more ? rooms.
/// </summary>
public class MysteryMachine : Badge
{
	private const int _eventCount = 15;

	public override BadgeRarity Rarity => BadgeRarity.Bronze;

	/// <summary>
	/// Traveled to 15 or more ? rooms.
	/// </summary>
	public MysteryMachine(SerializableRun run, bool won, ulong playerId)
		: base(run, won, playerId, "MYSTERY_MACHINE", requiresWin: false, multiplayerOnly: false)
	{
	}

	public override bool IsObtained()
	{
		int num = 0;
		foreach (List<MapPointHistoryEntry> item in _run.MapPointHistory)
		{
			foreach (MapPointHistoryEntry item2 in item)
			{
				if (item2.MapPointType == MapPointType.Unknown)
				{
					num++;
				}
			}
		}
		return num >= 15;
	}
}
