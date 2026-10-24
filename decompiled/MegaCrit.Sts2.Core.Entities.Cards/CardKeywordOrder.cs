namespace MegaCrit.Sts2.Core.Entities.Cards;

public static class CardKeywordOrder
{
	/// <summary>
	/// All the keywords that should be added before the card's description.
	/// Order matters here. If a card has multiple keywords, they will appear in this order.
	/// </summary>
	public static readonly CardKeyword[] beforeDescription = new CardKeyword[5]
	{
		CardKeyword.Ethereal,
		CardKeyword.Sly,
		CardKeyword.Retain,
		CardKeyword.Innate,
		CardKeyword.Unplayable
	};

	/// <summary>
	/// All the keywords that should be added after the card's description.
	/// Order matters here. If a card has multiple keywords, they will appear in this order.
	/// </summary>
	public static readonly CardKeyword[] afterDescription = new CardKeyword[2]
	{
		CardKeyword.Exhaust,
		CardKeyword.Eternal
	};
}
