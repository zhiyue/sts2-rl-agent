using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class WingCharm : RelicModel
{
	private const string _swiftAmountKey = "SwiftAmount";

	public override RelicRarity Rarity => RelicRarity.Shop;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("SwiftAmount", 1m));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromEnchantment<Swift>(base.DynamicVars["SwiftAmount"].IntValue);

	public override bool TryModifyCardRewardOptionsLate(Player player, List<CardCreationResult> cardRewards, CardCreationOptions options)
	{
		if (player != base.Owner)
		{
			return false;
		}
		Swift canonicalSwift = ModelDb.Enchantment<Swift>();
		List<CardCreationResult> list = cardRewards.Where((CardCreationResult r) => canonicalSwift.CanEnchant(r.Card)).ToList();
		if (list.Count == 0)
		{
			return false;
		}
		CardCreationResult cardCreationResult = base.Owner.RunState.Rng.Niche.NextItem(list);
		if (cardCreationResult == null)
		{
			return false;
		}
		CardModel card = base.Owner.RunState.CloneCard(cardCreationResult.Card);
		CardCmd.Enchant<Swift>(card, base.DynamicVars["SwiftAmount"].BaseValue);
		cardCreationResult.ModifyCard(card, this);
		return true;
	}
}
