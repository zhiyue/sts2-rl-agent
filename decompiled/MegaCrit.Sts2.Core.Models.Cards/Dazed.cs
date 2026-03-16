using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Dazed : CardModel
{
	public override int MaxUpgradeLevel => 0;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlyArray<CardKeyword>(new CardKeyword[2]
	{
		CardKeyword.Ethereal,
		CardKeyword.Unplayable
	});

	public Dazed()
		: base(-1, CardType.Status, CardRarity.Status, TargetType.None)
	{
	}
}
