using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace MegaCrit.Sts2.Core.Models.Cards;

public class AscendersBane : CardModel
{
	public override bool CanBeGeneratedByModifiers => false;

	public override int MaxUpgradeLevel => 0;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlyArray<CardKeyword>(new CardKeyword[3]
	{
		CardKeyword.Eternal,
		CardKeyword.Unplayable,
		CardKeyword.Ethereal
	});

	public AscendersBane()
		: base(-1, CardType.Curse, CardRarity.Curse, TargetType.None)
	{
	}
}
