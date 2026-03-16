using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace MegaCrit.Sts2.Core.Models.Cards.Mocks;

public sealed class MockCurseCard : MockCardModel
{
	protected override int CanonicalEnergyCost => -1;

	public override CardType Type => CardType.Curse;

	public override CardRarity Rarity => CardRarity.Curse;

	public override TargetType TargetType => TargetType.None;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlyArray<CardKeyword>(new CardKeyword[2]
	{
		CardKeyword.Unplayable,
		CardKeyword.Ethereal
	});

	public override MockCardModel MockBlock(int block)
	{
		throw new NotImplementedException();
	}

	protected override int GetBaseBlock()
	{
		throw new NotImplementedException();
	}
}
