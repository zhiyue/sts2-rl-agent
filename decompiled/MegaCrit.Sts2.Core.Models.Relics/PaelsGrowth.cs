using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Enchantments;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class PaelsGrowth : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new StringVar("EnchantmentName", ModelDb.Enchantment<Clone>().Title.GetFormattedText()));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromEnchantment<Clone>();

	public override async Task AfterObtained()
	{
		foreach (CardModel item in await CardSelectCmd.FromDeckForEnchantment(prefs: new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 1), player: base.Owner, enchantment: ModelDb.Enchantment<Clone>(), amount: 1))
		{
			CardCmd.Enchant<Clone>(item, 4m);
			CardCmd.Preview(item);
		}
	}

	public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
	{
		if (player != base.Owner)
		{
			return false;
		}
		options.Add(new CloneRestSiteOption(player));
		return true;
	}
}
