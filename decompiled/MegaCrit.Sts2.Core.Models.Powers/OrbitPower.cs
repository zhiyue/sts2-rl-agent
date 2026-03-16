using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class OrbitPower : PowerModel
{
	private class Data
	{
		public int energySpent;

		public int triggerCount;
	}

	private const int _energyIncrement = 4;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override int DisplayAmount => 4 - GetInternalData<Data>().energySpent % 4;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.ForEnergy(this));

	public override bool IsInstanced => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new EnergyVar(4));

	protected override object InitInternalData()
	{
		return new Data();
	}

	public override async Task AfterEnergySpent(CardModel card, int amount)
	{
		if (card.Owner.Creature == base.Owner && amount > 0)
		{
			Data data = GetInternalData<Data>();
			data.energySpent += amount;
			int triggers = data.energySpent / 4 - data.triggerCount;
			if (triggers > 0)
			{
				Flash();
				await PlayerCmd.GainEnergy(base.Amount * triggers, base.Owner.Player);
				data.triggerCount += triggers;
			}
			InvokeDisplayAmountChanged();
		}
	}
}
