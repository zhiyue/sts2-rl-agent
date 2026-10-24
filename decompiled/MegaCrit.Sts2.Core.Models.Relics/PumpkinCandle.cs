using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class PumpkinCandle : RelicModel
{
	private const string _defaultCombatCountKey = "CombatCount";

	public const int kindleAmount = 5;

	private int _kindleCount;

	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override bool ShowCounter => true;

	public override int DisplayAmount => KindleCount;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DynamicVar("CombatCount", 5m),
		new EnergyVar(1)
	});

	[SavedProperty]
	public int KindleCount
	{
		get
		{
			return _kindleCount;
		}
		set
		{
			AssertMutable();
			_kindleCount = value;
			base.Status = ((KindleCount <= 0) ? RelicStatus.Disabled : RelicStatus.Normal);
			InvokeDisplayAmountChanged();
		}
	}

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.ForEnergy(this));

	public override Task AfterObtained()
	{
		Rekindle();
		return Task.CompletedTask;
	}

	public override decimal ModifyMaxEnergy(Player player, decimal amount)
	{
		if (player != base.Owner)
		{
			return amount;
		}
		if (KindleCount <= 0)
		{
			return amount;
		}
		return amount + (decimal)base.DynamicVars.Energy.IntValue;
	}

	public override Task AfterCombatEnd(CombatRoom room)
	{
		KindleCount = Math.Max(KindleCount - 1, 0);
		return Task.CompletedTask;
	}

	public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
	{
		if (player != base.Owner)
		{
			return false;
		}
		options.Add(new KindleRestSiteOption(player));
		return true;
	}

	public void Rekindle()
	{
		KindleCount += 5;
		Flash();
	}
}
