using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class RedSkull : RelicModel
{
	private const string _hpThresholdKey = "HpThreshold";

	private bool _strengthApplied;

	public override RelicRarity Rarity => RelicRarity.Common;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DynamicVar("HpThreshold", 50m),
		new PowerVar<StrengthPower>(3m)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<StrengthPower>());

	private bool StrengthApplied
	{
		get
		{
			return _strengthApplied;
		}
		set
		{
			AssertMutable();
			_strengthApplied = value;
		}
	}

	public override async Task AfterRoomEntered(AbstractRoom room)
	{
		if (room is CombatRoom)
		{
			await ModifyStrengthIfNecessary();
		}
	}

	public override Task AfterCombatEnd(CombatRoom _)
	{
		StrengthApplied = false;
		base.Status = RelicStatus.Normal;
		return Task.CompletedTask;
	}

	public override async Task AfterCurrentHpChanged(Creature creature, decimal _)
	{
		if (CombatManager.Instance.IsInProgress)
		{
			await ModifyStrengthIfNecessary();
		}
	}

	private async Task ModifyStrengthIfNecessary()
	{
		Creature creature = base.Owner.Creature;
		bool flag = (decimal)creature.CurrentHp > (decimal)creature.MaxHp * (base.DynamicVars["HpThreshold"].BaseValue / 100m);
		base.Status = ((!flag) ? RelicStatus.Active : RelicStatus.Normal);
		decimal baseValue = base.DynamicVars.Strength.BaseValue;
		if (flag && StrengthApplied)
		{
			Flash();
			await PowerCmd.Apply<StrengthPower>(creature, -baseValue, creature, null);
			StrengthApplied = false;
		}
		else if (!flag && !StrengthApplied)
		{
			Flash();
			await PowerCmd.Apply<StrengthPower>(creature, baseValue, creature, null);
			StrengthApplied = true;
		}
	}
}
