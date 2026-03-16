using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class StoneHumidifier : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new MaxHpVar(5m));

	public override async Task AfterRestSiteHeal(Player player, bool isMimicked)
	{
		if (player == base.Owner)
		{
			Flash();
			await CreatureCmd.GainMaxHp(base.Owner.Creature, base.DynamicVars.MaxHp.BaseValue);
		}
	}

	public override IReadOnlyList<LocString> ModifyExtraRestSiteHealText(Player player, IReadOnlyList<LocString> currentExtraText)
	{
		if (!LocalContext.IsMe(base.Owner))
		{
			return currentExtraText;
		}
		int num = 0;
		LocString[] array = new LocString[1 + currentExtraText.Count];
		foreach (LocString item in currentExtraText)
		{
			array[num] = item;
			num++;
		}
		array[num] = base.AdditionalRestSiteHealText;
		return new global::_003C_003Ez__ReadOnlyArray<LocString>(array);
	}
}
