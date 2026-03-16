using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class RuinedHelmet : RelicModel
{
	private bool _usedThisCombat;

	public override RelicRarity Rarity => RelicRarity.Rare;

	private bool UsedThisCombat
	{
		get
		{
			return _usedThisCombat;
		}
		set
		{
			AssertMutable();
			_usedThisCombat = value;
		}
	}

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<StrengthPower>());

	public override bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount, Creature? applier, out decimal modifiedAmount)
	{
		modifiedAmount = amount;
		if (!(canonicalPower is StrengthPower))
		{
			return false;
		}
		if (target != base.Owner.Creature)
		{
			return false;
		}
		if (amount <= 0m)
		{
			return false;
		}
		if (UsedThisCombat)
		{
			return false;
		}
		modifiedAmount *= 2m;
		return true;
	}

	public override Task AfterModifyingPowerAmountReceived(PowerModel power)
	{
		Flash();
		UsedThisCombat = true;
		return Task.CompletedTask;
	}

	public override Task AfterCombatEnd(CombatRoom _)
	{
		UsedThisCombat = false;
		return Task.CompletedTask;
	}
}
