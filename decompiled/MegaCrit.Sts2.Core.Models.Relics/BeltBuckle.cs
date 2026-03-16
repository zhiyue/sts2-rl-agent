using System.Collections.Generic;
using System.Linq;
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

public sealed class BeltBuckle : RelicModel
{
	private bool _dexterityApplied;

	public override RelicRarity Rarity => RelicRarity.Shop;

	private bool DexterityApplied
	{
		get
		{
			return _dexterityApplied;
		}
		set
		{
			AssertMutable();
			_dexterityApplied = value;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new PowerVar<DexterityPower>(2m));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<DexterityPower>());

	public override async Task AfterObtained()
	{
		if (CombatManager.Instance.IsInProgress && !base.Owner.Potions.Any())
		{
			await ApplyDexterity();
		}
	}

	public override async Task BeforeCombatStart()
	{
		DexterityApplied = false;
		RefreshStatus();
		if (!base.Owner.Potions.Any())
		{
			await ApplyDexterity();
		}
	}

	public override Task AfterCombatEnd(CombatRoom room)
	{
		RefreshStatus();
		return Task.CompletedTask;
	}

	public override async Task AfterPotionProcured(PotionModel potion)
	{
		RefreshStatus();
		if (CombatManager.Instance.IsInProgress)
		{
			await RemoveDexterity();
		}
	}

	public override async Task AfterPotionDiscarded(PotionModel potion)
	{
		RefreshStatus();
		if (CombatManager.Instance.IsInProgress && !base.Owner.Potions.Any())
		{
			await ApplyDexterity();
		}
	}

	public override async Task AfterPotionUsed(PotionModel potion, Creature? target)
	{
		RefreshStatus();
		if (CombatManager.Instance.IsInProgress && !base.Owner.Potions.Any())
		{
			await ApplyDexterity();
		}
	}

	public override Task AfterCombatVictory(CombatRoom room)
	{
		DexterityApplied = false;
		RefreshStatus();
		return Task.CompletedTask;
	}

	private async Task ApplyDexterity()
	{
		if (!DexterityApplied)
		{
			DexterityApplied = true;
			Flash();
			await PowerCmd.Apply<DexterityPower>(base.Owner.Creature, base.DynamicVars.Dexterity.BaseValue, null, null);
		}
	}

	private async Task RemoveDexterity()
	{
		if (DexterityApplied)
		{
			DexterityApplied = false;
			Flash();
			await PowerCmd.Apply<DexterityPower>(base.Owner.Creature, -base.DynamicVars.Dexterity.BaseValue, null, null);
		}
	}

	private void RefreshStatus()
	{
		if (CombatManager.Instance.IsInProgress && !base.Owner.Potions.Any())
		{
			base.Status = RelicStatus.Active;
		}
		else
		{
			base.Status = RelicStatus.Normal;
		}
	}
}
