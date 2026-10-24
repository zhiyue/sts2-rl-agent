using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;

namespace MegaCrit.Sts2.Core.Models.Powers;

/// <summary>
/// This power doesn't actually do anything on its own. Instead, the Poison power checks for this power and
/// re-triggers itself if it's there.
/// </summary>
public sealed class AccelerantPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<PoisonPower>());
}
