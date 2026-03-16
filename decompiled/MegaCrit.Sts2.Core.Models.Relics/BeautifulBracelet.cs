using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class BeautifulBracelet : RelicModel
{
	private const string _swiftKey = "Swift";

	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new CardsVar(3),
		new DynamicVar("Swift", 3m)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromEnchantment<Swift>(base.DynamicVars["Swift"].IntValue);

	public override async Task AfterObtained()
	{
		foreach (CardModel item in await CardSelectCmd.FromDeckForEnchantment(prefs: new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, base.DynamicVars.Cards.IntValue), player: base.Owner, enchantment: ModelDb.Enchantment<Swift>(), amount: base.DynamicVars["Swift"].IntValue))
		{
			CardCmd.Enchant<Swift>(item, 3m);
			NCardEnchantVfx nCardEnchantVfx = NCardEnchantVfx.Create(item);
			if (nCardEnchantVfx != null)
			{
				NRun.Instance?.GlobalUi.CardPreviewContainer.AddChildSafely(nCardEnchantVfx);
			}
		}
	}
}
