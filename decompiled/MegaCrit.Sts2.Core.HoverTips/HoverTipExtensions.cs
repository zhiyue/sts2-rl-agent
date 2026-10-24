using System.Collections.Generic;
using System.Linq;

namespace MegaCrit.Sts2.Core.HoverTips;

public static class HoverTipExtensions
{
	/// <summary>
	/// Tries to add the tip to the list if the list doesn't already contain a tip of the same type that is equal or
	/// smarter than it.
	/// </summary>
	/// <param name="tips">List we are trying to add to.</param>
	/// <param name="tip">Tip we are trying to add.</param>
	public static void MegaTryAddingTip(this ICollection<IHoverTip> tips, IHoverTip tip)
	{
		IHoverTip hoverTip = tips.FirstOrDefault((IHoverTip t) => t.Id == tip.Id);
		if (hoverTip != null && !hoverTip.IsInstanced)
		{
			if (!hoverTip.IsSmart && tip.IsSmart)
			{
				tips.Remove(hoverTip);
				tips.Add(tip);
			}
		}
		else
		{
			tips.Add(tip);
		}
	}
}
