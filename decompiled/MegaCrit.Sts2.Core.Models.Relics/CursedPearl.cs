using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class CursedPearl : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromCardWithCardHoverTips<Greed>();

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new GoldVar(333));

	public override async Task AfterObtained()
	{
		await CardPileCmd.AddCurseToDeck<Greed>(base.Owner);
		await PlayerCmd.GainGold(base.DynamicVars.Gold.BaseValue, base.Owner);
	}
}
