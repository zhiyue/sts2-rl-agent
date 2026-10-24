using MegaCrit.Sts2.Core.Entities.Powers;

namespace MegaCrit.Sts2.Core.Models.Powers;

/// <summary>
/// This power acts as a visual indicator to the player that the Hunt card was successful. The actual behavior
/// lives in <see cref="T:MegaCrit.Sts2.Core.Models.Cards.TheHunt" /> .
/// </summary>
public sealed class TheHuntPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;
}
