using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Entities.Cards;

internal static class UnplayableReasonExtensions
{
	public static bool HasResourceCostReason(this UnplayableReason reason)
	{
		if (!reason.HasFlag(UnplayableReason.EnergyCostTooHigh))
		{
			return reason.HasFlag(UnplayableReason.StarCostTooHigh);
		}
		return true;
	}

	public static LocString? GetPlayerDialogueLine(this UnplayableReason reason, AbstractModel? preventer = null)
	{
		if (reason.HasFlag(UnplayableReason.NoLivingAllies))
		{
			return new LocString("combat_messages", "NO_LIVING_ALLIES");
		}
		if (reason.HasFlag(UnplayableReason.EnergyCostTooHigh))
		{
			return new LocString("combat_messages", "NOT_ENOUGH_ENERGY");
		}
		if (reason.HasFlag(UnplayableReason.StarCostTooHigh))
		{
			return new LocString("combat_messages", "NOT_ENOUGH_STARS");
		}
		if (reason.HasFlag(UnplayableReason.BlockedByHook))
		{
			if (preventer == null)
			{
				return new LocString("combat_messages", "UNPLAYABLE");
			}
			LocString locString = new LocString("combat_messages", "BLOCKED_BY_HOOK");
			string text = ((preventer is CardModel cardModel) ? cardModel.Title : ((preventer is RelicModel relicModel) ? relicModel.Title.GetFormattedText() : ((preventer is PowerModel powerModel) ? powerModel.Title.GetFormattedText() : ((preventer is EnchantmentModel enchantmentModel) ? enchantmentModel.Title.GetFormattedText() : ((!(preventer is AfflictionModel afflictionModel)) ? null : afflictionModel.Title.GetFormattedText())))));
			string text2 = text;
			if (text2 == null)
			{
				Log.Error($"Missing case for model {preventer} in UnplayableReason.GetPlayerDialogueLine switch!");
				text2 = "<UNKNOWN>";
			}
			locString.Add("BlockingHook", text2);
			return locString;
		}
		if (reason.HasFlag(UnplayableReason.HasUnplayableKeyword) || reason.HasFlag(UnplayableReason.BlockedByCardLogic))
		{
			return new LocString("combat_messages", "UNPLAYABLE");
		}
		return null;
	}
}
