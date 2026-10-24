using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Models.Badges;

/// <summary>
/// Win a run with 200, 400, 600 gold remaining.
/// </summary>
public class MoneyMoney : Badge
{
	public override BadgeRarity Rarity
	{
		get
		{
			int gold = _localPlayer.Gold;
			if (gold >= 400)
			{
				if (gold >= 600)
				{
					return BadgeRarity.Gold;
				}
				return BadgeRarity.Silver;
			}
			if (gold >= 200)
			{
				return BadgeRarity.Bronze;
			}
			return BadgeRarity.None;
		}
	}

	/// <summary>
	/// Win a run with 200, 400, 600 gold remaining.
	/// </summary>
	public MoneyMoney(SerializableRun run, bool won, ulong playerId)
		: base(run, won, playerId, "MONEY_MONEY", requiresWin: true, multiplayerOnly: false)
	{
	}

	public override bool IsObtained()
	{
		return Rarity != BadgeRarity.None;
	}
}
