using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class ToyBox : RelicModel
{
	private const string _relicsKey = "Relics";

	private const string _combatsKey = "Combats";

	private bool _isActivating;

	private int _combatsSeen;

	public static LocString WaxRelicPrefix => new LocString("relics", "TOY_BOX.waxRelicPrefix");

	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override bool HasUponPickupEffect => true;

	public override bool IsUsedUp => CombatsSeen >= base.DynamicVars["Combats"].IntValue * base.DynamicVars["Relics"].IntValue;

	public override bool ShowCounter => !IsUsedUp;

	public override int DisplayAmount
	{
		get
		{
			if (!IsActivating)
			{
				return CombatsSeen % base.DynamicVars["Combats"].IntValue;
			}
			return base.DynamicVars["Combats"].IntValue;
		}
	}

	private bool IsActivating
	{
		get
		{
			return _isActivating;
		}
		set
		{
			AssertMutable();
			_isActivating = value;
			InvokeDisplayAmountChanged();
		}
	}

	[SavedProperty]
	public int CombatsSeen
	{
		get
		{
			return _combatsSeen;
		}
		set
		{
			AssertMutable();
			_combatsSeen = value;
			InvokeDisplayAmountChanged();
			if (IsUsedUp)
			{
				base.Status = RelicStatus.Disabled;
			}
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DynamicVar("Relics", 4m),
		new DynamicVar("Combats", 3m)
	});

	public override async Task AfterObtained()
	{
		List<Reward> list = new List<Reward>();
		for (int i = 0; i < base.DynamicVars["Relics"].IntValue; i++)
		{
			RelicModel relicModel = RelicFactory.PullNextRelicFromFront(base.Owner).ToMutable();
			relicModel.IsWax = true;
			list.Add(new RelicReward(relicModel, base.Owner));
		}
		await RewardsCmd.OfferCustom(base.Owner, list);
	}

	public override async Task AfterCombatEnd(CombatRoom __)
	{
		CombatsSeen++;
		if (CombatsSeen % base.DynamicVars["Combats"].IntValue == 0)
		{
			TaskHelper.RunSafely(DoActivateVisuals());
			RelicModel relicModel = base.Owner.Relics.FirstOrDefault((RelicModel r) => r != null && r.IsWax && !r.IsMelted);
			if (relicModel != null)
			{
				await RelicCmd.Melt(relicModel);
				await Cmd.CustomScaledWait(0.5f, 0.75f);
			}
		}
	}

	private async Task DoActivateVisuals()
	{
		IsActivating = true;
		Flash();
		await Cmd.Wait(1f);
		IsActivating = false;
	}
}
