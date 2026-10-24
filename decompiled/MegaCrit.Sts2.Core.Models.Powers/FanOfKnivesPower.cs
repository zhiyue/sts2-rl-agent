using MegaCrit.Sts2.Core.Entities.Powers;

namespace MegaCrit.Sts2.Core.Models.Powers;

/// <summary>
/// This power doesn't actually do anything on its own. Instead, the <see cref="T:MegaCrit.Sts2.Core.Models.Cards.Shiv" /> card checks for its existence
/// and modifies its targeting.
/// </summary>
public sealed class FanOfKnivesPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;
}
