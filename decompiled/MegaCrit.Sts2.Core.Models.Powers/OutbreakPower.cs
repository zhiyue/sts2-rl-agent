using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class OutbreakPower : PowerModel
{
	private class Data
	{
		public int timesPoisoned;
	}

	public const int poisonThreshold = 3;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override int DisplayAmount => GetInternalData<Data>().timesPoisoned;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new RepeatVar(3));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<PoisonPower>());

	protected override object InitInternalData()
	{
		return new Data();
	}

	public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		if (applier == base.Owner && !(amount <= 0m) && power is PoisonPower)
		{
			Data data = GetInternalData<Data>();
			data.timesPoisoned++;
			if (data.timesPoisoned >= 3)
			{
				InvokeDisplayAmountChanged();
				Flash();
				await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), base.Owner.CombatState.HittableEnemies, base.Amount, ValueProp.Unpowered, base.Owner, null);
				data.timesPoisoned %= 3;
			}
			InvokeDisplayAmountChanged();
		}
	}
}
