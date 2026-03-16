using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Enthralled : CardModel
{
	public override bool CanBeGeneratedByModifiers => false;

	protected override bool ShouldGlowRedInternal => true;

	public override int MaxUpgradeLevel => 0;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Eternal);

	public Enthralled()
		: base(2, CardType.Curse, CardRarity.Curse, TargetType.None)
	{
	}

	public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
	{
		if (card.Owner != base.Owner)
		{
			return true;
		}
		CardPile? pile = base.Pile;
		if (pile == null || pile.Type != PileType.Hand)
		{
			return true;
		}
		if (card is Enthralled)
		{
			return true;
		}
		if (autoPlayType != AutoPlayType.None)
		{
			return true;
		}
		return false;
	}
}
