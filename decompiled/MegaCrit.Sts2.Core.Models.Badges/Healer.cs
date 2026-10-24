using System.Collections.Generic;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Models.Badges;

/// <summary>
/// For each time you Mend in multiplayer, you get a higher tier badge!
/// </summary>
public class Healer : Badge
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
						if (room.RoomType != RoomType.RestSite)
						{
							continue;
						}
						foreach (PlayerMapPointHistoryEntry playerStat in item2.PlayerStats)
						{
							if (playerStat.PlayerId == _localPlayer.NetId && playerStat.RestSiteChoices.Contains("MEND"))
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
	/// For each time you Mend in multiplayer, you get a higher tier badge!
	/// </summary>
	public Healer(SerializableRun run, bool won, ulong playerId)
		: base(run, won, playerId, "HEALER", requiresWin: false, multiplayerOnly: true)
	{
	}

	public override bool IsObtained()
	{
		return Rarity != BadgeRarity.None;
	}
}
