using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Models.Badges;

/// <summary>
/// The faster you win a run, the better the badge!
/// </summary>
public class Speedy : Badge
{
	private const int _winTimeGold = 1800;

	private const int _winTimeSilver = 2400;

	private const int _winTimeBronze = 3000;

	public override BadgeRarity Rarity
	{
		get
		{
			long winTime = _run.WinTime;
			if (winTime <= 2400)
			{
				if (winTime <= 1800)
				{
					return BadgeRarity.Gold;
				}
				return BadgeRarity.Silver;
			}
			if (winTime <= 3000)
			{
				return BadgeRarity.Bronze;
			}
			return BadgeRarity.None;
		}
	}

	/// <summary>
	/// The faster you win a run, the better the badge!
	/// </summary>
	public Speedy(SerializableRun run, bool won, ulong playerId)
		: base(run, won, playerId, "SPEEDY", requiresWin: true, multiplayerOnly: false)
	{
	}

	public override bool IsObtained()
	{
		return Rarity != BadgeRarity.None;
	}
}
