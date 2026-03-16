using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class LuckyFysh : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Uncommon;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new GoldVar(15));

	public override bool IsAllowed(IRunState runState)
	{
		return RelicModel.IsBeforeAct3TreasureChest(runState);
	}

	public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
	{
		CardPile? pile = card.Pile;
		if (pile != null && pile.Type == PileType.Deck && card.Owner == base.Owner)
		{
			Flash();
			await PlayerCmd.GainGold(base.DynamicVars.Gold.BaseValue, base.Owner);
		}
	}
}
