using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;

namespace MegaCrit.Sts2.Core.Entities.Cards;

internal static class CardKeywordExtensions
{
	private static readonly LocString _period = new LocString("card_keywords", "PERIOD");

	public static string GetLocKeyPrefix(this CardKeyword keyword)
	{
		return StringHelper.Slugify(keyword.ToString());
	}

	public static LocString GetTitle(this CardKeyword keyword)
	{
		return new LocString("card_keywords", keyword.GetLocKeyPrefix() + ".title");
	}

	public static LocString GetDescription(this CardKeyword keyword)
	{
		return new LocString("card_keywords", keyword.GetLocKeyPrefix() + ".description");
	}

	public static string GetCardText(this CardKeyword keyword)
	{
		return "[gold]" + keyword.GetTitle().GetFormattedText() + "[/gold]" + _period.GetRawText();
	}
}
