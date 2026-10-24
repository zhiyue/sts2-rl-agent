using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class LizardTail : RelicModel
{
	private bool _wasUsed;

	public override RelicRarity Rarity => RelicRarity.Rare;

	public override bool IsUsedUp => _wasUsed;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new HealVar(50m));

	[SavedProperty]
	public bool WasUsed
	{
		get
		{
			return _wasUsed;
		}
		set
		{
			AssertMutable();
			_wasUsed = value;
			if (IsUsedUp)
			{
				base.Status = RelicStatus.Disabled;
			}
		}
	}

	public override bool ShouldDieLate(Creature creature)
	{
		if (creature != base.Owner.Creature)
		{
			return true;
		}
		if (WasUsed)
		{
			return true;
		}
		return false;
	}

	public override async Task AfterPreventingDeath(Creature creature)
	{
		Flash();
		WasUsed = true;
		decimal amount = Math.Max(1m, (decimal)creature.MaxHp * (base.DynamicVars.Heal.BaseValue / 100m));
		await CreatureCmd.Heal(creature, amount);
	}
}
