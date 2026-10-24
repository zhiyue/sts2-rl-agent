using MegaCrit.Sts2.Core.Entities.Powers;

namespace MegaCrit.Sts2.Core.Models.Powers;

/// <summary>
/// This is just a marker power for <see cref="T:MegaCrit.Sts2.Core.Models.Powers.SurroundedPower" /> to check for.
/// </summary>
public sealed class BackAttackRightPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;
}
