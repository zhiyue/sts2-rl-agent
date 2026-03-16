using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Enchantments;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class GnarledHammer : RelicModel
{
	private const string _sharpAmountKey = "SharpAmount";

	public override RelicRarity Rarity => RelicRarity.Shop;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new CardsVar(3),
		new DynamicVar("SharpAmount", 3m)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromEnchantment<Sharp>(base.DynamicVars["SharpAmount"].IntValue);

	public override async Task AfterObtained()
	{
		CardSelectorPrefs cardSelectorPrefs = new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 0, base.DynamicVars.Cards.IntValue);
		cardSelectorPrefs.Cancelable = false;
		cardSelectorPrefs.RequireManualConfirmation = true;
		CardSelectorPrefs prefs = cardSelectorPrefs;
		Sharp canonicalEnchantment = ModelDb.Enchantment<Sharp>();
		foreach (CardModel item in await CardSelectCmd.FromDeckForEnchantment(base.Owner, canonicalEnchantment, base.DynamicVars["SharpAmount"].IntValue, prefs))
		{
			CardCmd.Enchant(canonicalEnchantment.ToMutable(), item, base.DynamicVars["SharpAmount"].IntValue);
			CardCmd.Preview(item);
		}
	}
}
