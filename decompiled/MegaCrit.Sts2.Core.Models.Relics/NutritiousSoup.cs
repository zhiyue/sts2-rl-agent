using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class NutritiousSoup : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromEnchantment<TezcatarasEmber>();

	public override Task AfterObtained()
	{
		IEnumerable<CardModel> enumerable = PileType.Deck.GetPile(base.Owner).Cards.ToList();
		foreach (CardModel item in enumerable)
		{
			if (item.Rarity == CardRarity.Basic && item.Tags.Contains(CardTag.Strike) && ModelDb.Enchantment<TezcatarasEmber>().CanEnchant(item))
			{
				CardCmd.Enchant<TezcatarasEmber>(item, 1m);
				NCardEnchantVfx nCardEnchantVfx = NCardEnchantVfx.Create(item);
				if (nCardEnchantVfx != null)
				{
					NRun.Instance?.GlobalUi.CardPreviewContainer.AddChildSafely(nCardEnchantVfx);
				}
			}
		}
		return Task.CompletedTask;
	}
}
