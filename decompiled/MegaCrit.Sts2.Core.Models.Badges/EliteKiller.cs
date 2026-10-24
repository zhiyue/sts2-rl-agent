using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Models.Badges;

/// <summary>
/// Defeat X Elites. The more you kill, the higher the rarity.
/// </summary>
public class EliteKiller : Badge
{
	public override BadgeRarity Rarity
	{
		get
		{
			int elitesKilledCount = ScoreUtility.GetElitesKilledCount(_run.MapPointHistory);
			if (elitesKilledCount >= 6)
			{
				if (elitesKilledCount >= 9)
				{
					return BadgeRarity.Gold;
				}
				return BadgeRarity.Silver;
			}
			if (elitesKilledCount >= 3)
			{
				return BadgeRarity.Bronze;
			}
			return BadgeRarity.None;
		}
	}

	/// <summary>
	/// Defeat X Elites. The more you kill, the higher the rarity.
	/// </summary>
	public EliteKiller(SerializableRun run, bool won, ulong playerId)
		: base(run, won, playerId, "ELITE", requiresWin: false, multiplayerOnly: false)
	{
	}

	public override bool IsObtained()
	{
		return Rarity != BadgeRarity.None;
	}
}
