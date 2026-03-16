namespace MegaCrit.Sts2.Core.Entities.Creatures;

public class SummonResult
{
	public Creature? Creature { get; init; }

	public decimal Amount { get; init; }

	public SummonResult(Creature? creature, decimal amount)
	{
		Creature = creature;
		Amount = amount;
	}
}
