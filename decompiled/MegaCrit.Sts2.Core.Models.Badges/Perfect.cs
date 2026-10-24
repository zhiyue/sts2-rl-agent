using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Models.Badges;

/// <summary>
/// Defeat a boss without taking damage.
/// </summary>
public class Perfect : Badge
{
	public override BadgeRarity Rarity
	{
		get
		{
			int num = 0;
			foreach (List<MapPointHistoryEntry> item in _run.MapPointHistory)
			{
				foreach (MapPointHistoryEntry item2 in item)
				{
					foreach (MapPointRoomHistoryEntry room in item2.Rooms)
					{
						if (room.RoomType != RoomType.Boss || (!_won && room == _run.MapPointHistory.Last().Last().Rooms.Last()))
						{
							continue;
						}
						foreach (PlayerMapPointHistoryEntry playerStat in item2.PlayerStats)
						{
							if (playerStat.PlayerId == _localPlayer.NetId && playerStat.DamageTaken <= 0)
							{
								num++;
							}
						}
					}
				}
			}
			if (num < 3)
			{
				return num switch
				{
					2 => BadgeRarity.Silver, 
					1 => BadgeRarity.Bronze, 
					_ => BadgeRarity.None, 
				};
			}
			return BadgeRarity.Gold;
		}
	}

	/// <summary>
	/// Defeat a boss without taking damage.
	/// </summary>
	public Perfect(SerializableRun run, bool won, ulong playerId)
		: base(run, won, playerId, "PERFECT", requiresWin: false, multiplayerOnly: false)
	{
	}

	public override bool IsObtained()
	{
		return Rarity != BadgeRarity.None;
	}
}
