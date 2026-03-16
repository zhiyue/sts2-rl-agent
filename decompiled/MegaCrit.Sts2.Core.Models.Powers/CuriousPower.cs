using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class CuriousPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
	{
		modifiedCost = originalCost;
		if (card.Owner.Creature != base.Owner)
		{
			return false;
		}
		if (card.Type != CardType.Power)
		{
			return false;
		}
		if (originalCost <= 0m)
		{
			return false;
		}
		modifiedCost = originalCost - (decimal)base.Amount;
		if (modifiedCost < 0m)
		{
			modifiedCost = default(decimal);
		}
		return true;
	}
}
