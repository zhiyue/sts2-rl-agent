using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;

namespace MegaCrit.Sts2.Core.Models.Powers;

/// <summary>
/// This power doesn't actually do anything on its own. Instead, Sovereign blade cards checks for this power and
/// and changes its behavior based off of that
/// </summary>
public sealed class SeekingEdgePower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromForge();
}
