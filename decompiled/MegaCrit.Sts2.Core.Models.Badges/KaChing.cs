using System.Collections.Generic;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Models.Badges;

/// <summary>
/// Spent more than 1,000 Gold at the Merchant (aggregate).
/// </summary>
public class KaChing : Badge
{
	private const int _goldRequirement = 1000;

	public override BadgeRarity Rarity => BadgeRarity.Bronze;

	/// <summary>
	/// Spent more than 1,000 Gold at the Merchant (aggregate).
	/// </summary>
	public KaChing(SerializableRun run, bool won, ulong playerId)
		: base(run, won, playerId, "KACHING", requiresWin: false, multiplayerOnly: false)
	{
	}

	public override bool IsObtained()
	{
		int num = 0;
		foreach (List<MapPointHistoryEntry> item in _run.MapPointHistory)
		{
			foreach (MapPointHistoryEntry item2 in item)
			{
				foreach (MapPointRoomHistoryEntry room in item2.Rooms)
				{
					if (room.RoomType != RoomType.Shop)
					{
						continue;
					}
					foreach (PlayerMapPointHistoryEntry playerStat in item2.PlayerStats)
					{
						if (playerStat.PlayerId == _localPlayer.NetId)
						{
							num += playerStat.GoldSpent;
						}
					}
				}
			}
		}
		return num >= 1000;
	}
}
