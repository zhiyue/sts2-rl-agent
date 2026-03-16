using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Enchantments;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class PunchDagger : RelicModel
{
	private const string _momentumKey = "Momentum";

	public override RelicRarity Rarity => RelicRarity.Shop;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("Momentum", 5m));

	public override bool HasUponPickupEffect => true;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromEnchantment<Momentum>(base.DynamicVars["Momentum"].IntValue);

	public override async Task AfterObtained()
	{
		CardSelectorPrefs prefs = new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 1);
		Momentum canonicalMomentum = ModelDb.Enchantment<Momentum>();
		foreach (CardModel item in await CardSelectCmd.FromDeckForEnchantment(base.Owner, canonicalMomentum, base.DynamicVars["Momentum"].IntValue, prefs))
		{
			CardCmd.Enchant(canonicalMomentum.ToMutable(), item, base.DynamicVars["Momentum"].IntValue);
			CardCmd.Preview(item);
		}
	}
}
