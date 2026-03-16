using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class DarkstonePeriapt : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Event;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new MaxHpVar(6m));

	public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
	{
		CardPile? pile = card.Pile;
		if (pile != null && pile.Type == PileType.Deck && card.Owner == base.Owner && card.Type == CardType.Curse)
		{
			Flash();
			await CreatureCmd.GainMaxHp(base.Owner.Creature, base.DynamicVars.MaxHp.BaseValue);
		}
	}
}
