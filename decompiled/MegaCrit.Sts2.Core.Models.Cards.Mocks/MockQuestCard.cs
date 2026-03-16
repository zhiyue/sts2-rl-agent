using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace MegaCrit.Sts2.Core.Models.Cards.Mocks;

public sealed class MockQuestCard : MockCardModel
{
	protected override int CanonicalEnergyCost => -1;

	public override CardType Type => CardType.Quest;

	public override CardRarity Rarity => CardRarity.Quest;

	public override TargetType TargetType => TargetType.None;

	public override int MaxUpgradeLevel => 0;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Unplayable);

	public override MockCardModel MockBlock(int block)
	{
		throw new NotImplementedException();
	}

	protected override int GetBaseBlock()
	{
		throw new NotImplementedException();
	}
}
