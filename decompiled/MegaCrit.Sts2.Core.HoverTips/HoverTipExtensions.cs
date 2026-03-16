using System.Collections.Generic;
using System.Linq;

namespace MegaCrit.Sts2.Core.HoverTips;

public static class HoverTipExtensions
{
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
