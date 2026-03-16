using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class RoyalStamp : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Shop;

	public override bool HasUponPickupEffect => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new CardsVar(1),
		new StringVar("Enchantment", ModelDb.Enchantment<RoyallyApproved>().Title.GetFormattedText())
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromEnchantment<RoyallyApproved>();

	public override async Task AfterObtained()
	{
		EnchantmentModel royalStamp = ModelDb.Enchantment<RoyallyApproved>();
		List<CardModel> list = PileType.Deck.GetPile(base.Owner).Cards.Where((CardModel c) => royalStamp.CanEnchant(c)).ToList();
		CardModel cardModel = (await CardSelectCmd.FromDeckForEnchantment(prefs: new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 1), cards: list.UnstableShuffle(base.Owner.RunState.Rng.Niche).ToList(), enchantment: royalStamp, amount: 1)).FirstOrDefault();
		if (cardModel != null)
		{
			CardCmd.Enchant<RoyallyApproved>(cardModel, 1m);
			NCardEnchantVfx nCardEnchantVfx = NCardEnchantVfx.Create(cardModel);
			if (nCardEnchantVfx != null)
			{
				NRun.Instance?.GlobalUi.CardPreviewContainer.AddChildSafely(nCardEnchantVfx);
			}
		}
	}
}
