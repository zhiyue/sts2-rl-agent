using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class LoomingFruit : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override string IconBaseName
	{
		get
		{
			if (!HasCornucopia())
			{
				return base.IconBaseName + "_2";
			}
			return base.IconBaseName;
		}
	}

	public override bool HasUponPickupEffect => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new MaxHpVar(31m));

	private static bool HasCornucopia()
	{
		string uniqueId = SaveManager.Instance.Progress.UniqueId;
		if (string.IsNullOrEmpty(uniqueId))
		{
			return false;
		}
		return uniqueId[uniqueId.Length - 1] % 2 == 0;
	}

	public override async Task AfterObtained()
	{
		await CreatureCmd.GainMaxHp(base.Owner.Creature, base.DynamicVars.MaxHp.BaseValue);
	}
}
