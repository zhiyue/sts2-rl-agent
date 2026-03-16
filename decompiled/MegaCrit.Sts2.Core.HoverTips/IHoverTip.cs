using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.HoverTips;

public interface IHoverTip
{
	private static readonly string _summonStaticId = HoverTipFactory.Static(StaticHoverTip.SummonStatic).Id;

	private static readonly string _summonDynamicId = HoverTipFactory.Static(StaticHoverTip.SummonDynamic, new SummonVar(0m)).Id;

	string Id { get; }

	bool IsSmart { get; }

	bool IsDebuff { get; }

	bool IsInstanced { get; }

	AbstractModel? CanonicalModel { get; }

	static IEnumerable<IHoverTip> RemoveDupes(IEnumerable<IHoverTip> tips)
	{
		List<IHoverTip> list = new List<IHoverTip>();
		foreach (IHoverTip hoverTip in tips)
		{
			if (string.IsNullOrEmpty(hoverTip.Id))
			{
				list.Add(hoverTip);
				continue;
			}
			IHoverTip hoverTip2 = list.FirstOrDefault((IHoverTip tip) => tip.Id == hoverTip.Id && !tip.IsInstanced);
			if (hoverTip2 == null)
			{
				list.Add(hoverTip);
			}
			else if (!hoverTip2.IsSmart || hoverTip.IsSmart)
			{
				list.Remove(hoverTip2);
				list.Add(hoverTip);
			}
		}
		if (list.Any((IHoverTip tip) => tip.Id == _summonStaticId))
		{
			list.RemoveAll((IHoverTip tip) => tip.Id == _summonDynamicId);
		}
		return list;
	}
}
