using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class PossessSpeedPower : PowerModel
{
	private class Data
	{
		public Dictionary<Creature, decimal> stolenDexterity = new Dictionary<Creature, decimal>();
	}

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<DexterityPower>());

	private Dictionary<Creature, decimal> StolenDexterity => GetInternalData<Data>().stolenDexterity;

	protected override object InitInternalData()
	{
		return new Data();
	}

	public override Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		if (applier != base.Owner)
		{
			return Task.CompletedTask;
		}
		if (!power.Owner.IsPlayer)
		{
			return Task.CompletedTask;
		}
		if (!(power is DexterityPower))
		{
			return Task.CompletedTask;
		}
		if (amount >= 0m)
		{
			return Task.CompletedTask;
		}
		if (!StolenDexterity.ContainsKey(power.Owner))
		{
			StolenDexterity.Add(power.Owner, 0m);
		}
		StolenDexterity[power.Owner] += amount;
		return Task.CompletedTask;
	}

	public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		if (wasRemovalPrevented || creature != base.Owner)
		{
			return;
		}
		foreach (KeyValuePair<Creature, decimal> item in StolenDexterity)
		{
			await PowerCmd.Apply<DexterityPower>(item.Key, -item.Value, null, null);
		}
	}
}
