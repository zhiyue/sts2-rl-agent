namespace MegaCrit.Sts2.Core.Entities.Cards;

public static class CardKeywordOrder
{
	public static readonly CardKeyword[] beforeDescription = new CardKeyword[5]
	{
		CardKeyword.Ethereal,
		CardKeyword.Sly,
		CardKeyword.Retain,
		CardKeyword.Innate,
		CardKeyword.Unplayable
	};

	public static readonly CardKeyword[] afterDescription = new CardKeyword[2]
	{
		CardKeyword.Exhaust,
		CardKeyword.Eternal
	};
}
