using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class CurseOfTheBell : CardModel
{
	public override bool CanBeGeneratedByModifiers => false;

	public override int MaxUpgradeLevel => 0;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlyArray<CardKeyword>(new CardKeyword[2]
	{
		CardKeyword.Eternal,
		CardKeyword.Unplayable
	});

	public CurseOfTheBell()
		: base(-1, CardType.Curse, CardRarity.Curse, TargetType.None)
	{
	}
}
