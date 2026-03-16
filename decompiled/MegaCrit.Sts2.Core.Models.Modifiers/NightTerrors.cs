using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;

namespace MegaCrit.Sts2.Core.Models.Modifiers;

public class NightTerrors : ModifierModel
{
	private const int _maxHpLoss = 5;

	public override decimal ModifyRestSiteHealAmount(Creature creature, decimal amount)
	{
		return creature.MaxHp;
	}

	public override async Task AfterRestSiteHeal(Player player, bool isMimicked)
	{
		if (!isMimicked)
		{
			await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), player.Creature, 5m, isFromCard: false);
		}
	}

	public override IReadOnlyList<LocString> ModifyExtraRestSiteHealText(Player player, IReadOnlyList<LocString> currentExtraText)
	{
		LocString additionalRestSiteHealText = base.AdditionalRestSiteHealText;
		additionalRestSiteHealText.Add("Heal", player.Creature.MaxHp);
		additionalRestSiteHealText.Add("MaxHpLoss", 5m);
		int num = 0;
		LocString[] array = new LocString[1 + currentExtraText.Count];
		foreach (LocString item in currentExtraText)
		{
			array[num] = item;
			num++;
		}
		array[num] = additionalRestSiteHealText;
		return new global::_003C_003Ez__ReadOnlyArray<LocString>(array);
	}
}
