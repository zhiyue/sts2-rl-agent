using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Clumsy : CardModel
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlyArray<CardKeyword>(new CardKeyword[2]
	{
		CardKeyword.Unplayable,
		CardKeyword.Ethereal
	});

	public override int MaxUpgradeLevel => 0;

	public Clumsy()
		: base(-1, CardType.Curse, CardRarity.Curse, TargetType.None)
	{
	}
}
