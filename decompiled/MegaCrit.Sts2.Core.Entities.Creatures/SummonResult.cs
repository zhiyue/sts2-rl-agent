namespace MegaCrit.Sts2.Core.Entities.Creatures;

/// <summary>
/// A class containing meta-info about the result of a Summon.
/// </summary>
public class SummonResult
{
	/// <summary>
	/// The creature that was summoned.
	/// Null if something blocks the summon.
	/// </summary>
	public Creature? Creature { get; init; }

	/// <summary>
	/// The amount that they were summoned for.
	/// 0 if something blocks the summon.
	/// </summary>
	public decimal Amount { get; init; }

	public SummonResult(Creature? creature, decimal amount)
	{
		Creature = creature;
		Amount = amount;
	}
}
