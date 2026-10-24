using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace MegaCrit.Sts2.Core.Models.Powers.Mocks;

public class MockScaleInMultiplayerPower : PowerModel
{
	public delegate decimal GetScaledAmountForMultiplayerDelegate(ICombatState combatState, Creature? applier, decimal amount, Creature target, CardModel? cardSource);

	public bool shouldScaleInMultiplayer = true;

	public static GetScaledAmountForMultiplayerDelegate? getScaledAmountForMultiplayer;

	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override bool AllowNegative => true;

	public override bool ShouldScaleInMultiplayer => shouldScaleInMultiplayer;

	public override decimal GetScaledAmountForMultiplayer(ICombatState combatState, Creature? applier, decimal amount, Creature target, CardModel? cardSource)
	{
		if (getScaledAmountForMultiplayer != null)
		{
			return getScaledAmountForMultiplayer(combatState, applier, amount, target, cardSource);
		}
		return base.GetScaledAmountForMultiplayer(combatState, applier, amount, target, cardSource);
	}
}
