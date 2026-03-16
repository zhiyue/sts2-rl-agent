using System;

namespace MegaCrit.Sts2.Core.Entities.Cards;

public class LocalCostModifier
{
	public int Amount { get; set; }

	public LocalCostType Type { get; }

	public LocalCostModifierExpiration Expiration { get; }

	public bool IsReduceOnly { get; }

	public LocalCostModifier(int amount, LocalCostType type, LocalCostModifierExpiration expiration, bool reduceOnly)
	{
		Amount = amount;
		Type = type;
		Expiration = expiration;
		IsReduceOnly = reduceOnly;
	}

	public int Modify(int currentCost)
	{
		return Type switch
		{
			LocalCostType.Absolute => IsReduceOnly ? Math.Min(currentCost, Amount) : Amount, 
			LocalCostType.Relative => IsReduceOnly ? Math.Min(currentCost, currentCost + Amount) : (currentCost + Amount), 
			_ => throw new ArgumentOutOfRangeException("Type", Type, null), 
		};
	}
}
