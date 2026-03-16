namespace MegaCrit.Sts2.Core.Entities.Cards;

public static class CardUpgradePreviewTypeExtensions
{
	public static bool IsPreview(this CardUpgradePreviewType previewType)
	{
		return previewType != CardUpgradePreviewType.None;
	}
}
