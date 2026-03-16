using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class BowlerHat : RelicModel
{
	private const decimal _bonusMultiplier = 0.2m;

	private decimal _pendingBonusGold;

	private bool _isApplyingBonus;

	public override RelicRarity Rarity => RelicRarity.Uncommon;

	public override bool IsAllowed(IRunState runState)
	{
		return RelicModel.IsBeforeAct3TreasureChest(runState);
	}

	public override bool ShouldGainGold(decimal amount, Player player)
	{
		if (_isApplyingBonus)
		{
			return true;
		}
		if (player != base.Owner)
		{
			return true;
		}
		_pendingBonusGold = Math.Floor(amount * 0.2m);
		return true;
	}

	public override async Task AfterGoldGained(Player player)
	{
		if (player == base.Owner && !_isApplyingBonus && !(_pendingBonusGold <= 0m))
		{
			decimal pendingBonusGold = _pendingBonusGold;
			_pendingBonusGold = default(decimal);
			_isApplyingBonus = true;
			Flash();
			await PlayerCmd.GainGold(pendingBonusGold, base.Owner);
			_isApplyingBonus = false;
		}
	}
}
