using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class SneckoSkull : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Common;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<PoisonPower>());

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new PowerVar<PoisonPower>(1m));

	public override decimal ModifyPowerAmountGiven(PowerModel power, Creature giver, decimal amount, Creature? target, CardModel? cardSource)
	{
		if (!(power is PoisonPower))
		{
			return amount;
		}
		if (giver != base.Owner.Creature)
		{
			return amount;
		}
		return amount + (decimal)base.DynamicVars.Poison.IntValue;
	}

	public override Task AfterModifyingPowerAmountGiven(PowerModel power)
	{
		Flash();
		return Task.CompletedTask;
	}
}
