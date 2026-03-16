using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class Claws : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(6));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromCardWithCardHoverTips<Maul>();

	public override async Task AfterObtained()
	{
		CardSelectorPrefs cardSelectorPrefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 0, base.DynamicVars.Cards.IntValue);
		cardSelectorPrefs.Cancelable = false;
		cardSelectorPrefs.RequireManualConfirmation = true;
		CardSelectorPrefs prefs = cardSelectorPrefs;
		List<CardTransformation> transformations = (await CardSelectCmd.FromDeckForTransformation(base.Owner, prefs, (CardModel c) => new CardTransformation(c, CreateMaulFromOriginal(c, forPreview: true)))).Select((CardModel original) => new CardTransformation(original, CreateMaulFromOriginal(original, forPreview: false))).ToList();
		await CardCmd.Transform(transformations, base.Owner.PlayerRng.Transformations);
	}

	private CardModel CreateMaulFromOriginal(CardModel original, bool forPreview)
	{
		CardModel cardModel = (forPreview ? ModelDb.Card<Maul>().ToMutable() : base.Owner.RunState.CreateCard<Maul>(base.Owner));
		if (original.IsUpgraded && cardModel.IsUpgradable)
		{
			if (forPreview)
			{
				cardModel.UpgradeInternal();
			}
			else
			{
				CardCmd.Upgrade(cardModel);
			}
		}
		if (original.Enchantment != null)
		{
			EnchantmentModel enchantmentModel = (EnchantmentModel)original.Enchantment.MutableClone();
			if (enchantmentModel.CanEnchant(cardModel))
			{
				if (forPreview)
				{
					cardModel.EnchantInternal(enchantmentModel, enchantmentModel.Amount);
					enchantmentModel.ModifyCard();
				}
				else
				{
					CardCmd.Enchant(enchantmentModel, cardModel, enchantmentModel.Amount);
				}
			}
		}
		return cardModel;
	}
}
