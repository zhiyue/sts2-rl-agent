using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class RegalPillow : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Common;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new HealVar(15m));

	public override decimal ModifyRestSiteHealAmount(Creature creature, decimal amount)
	{
		if (creature.Player != base.Owner && creature.PetOwner != base.Owner)
		{
			return amount;
		}
		return amount + base.DynamicVars.Heal.BaseValue;
	}

	public override Task AfterRestSiteHeal(Player player, bool isMimicked)
	{
		if (player != base.Owner)
		{
			return Task.CompletedTask;
		}
		Flash();
		base.Status = RelicStatus.Normal;
		return Task.CompletedTask;
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

	public override Task AfterRoomEntered(AbstractRoom room)
	{
		base.Status = ((room is RestSiteRoom) ? RelicStatus.Active : RelicStatus.Normal);
		return Task.CompletedTask;
	}
}
