using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class SneckoEye : RelicModel
{
	private int _testEnergyCostOverride = -1;

	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(2));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<ConfusedPower>());

	public override async Task AfterObtained()
	{
		if (CombatManager.Instance.IsInProgress)
		{
			await ApplyPower();
		}
	}

	public override async Task BeforeCombatStart()
	{
		await ApplyPower();
	}

	public override decimal ModifyHandDraw(Player player, decimal count)
	{
		if (player != base.Owner)
		{
			return count;
		}
		return count + base.DynamicVars.Cards.BaseValue;
	}

	private async Task ApplyPower()
	{
		await PowerCmd.Apply<ConfusedPower>(base.Owner.Creature, 1m, base.Owner.Creature, null);
		ApplyTestEnergyCostOverrideToPower();
	}

	public void SetTestEnergyCostOverride(int value)
	{
		TestMode.AssertOn();
		AssertMutable();
		_testEnergyCostOverride = value;
		ApplyTestEnergyCostOverrideToPower();
	}

	private void ApplyTestEnergyCostOverrideToPower()
	{
		if (_testEnergyCostOverride >= 0)
		{
			ConfusedPower power = base.Owner.Creature.GetPower<ConfusedPower>();
			if (power != null)
			{
				power.TestEnergyCostOverride = _testEnergyCostOverride;
			}
		}
	}
}
