using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Combat.History.Entries;

public class PowerReceivedEntry : CombatHistoryEntry
{
	public PowerModel Power { get; }

	public decimal Amount { get; }

	public Creature? Applier { get; }

	public override string Description
	{
		get
		{
			if (Applier == null)
			{
				return $"{base.Actor.ModelId.Entry} received {Amount} {Power.Id.Entry}";
			}
			return $"{Applier.ModelId.Entry} applied {Amount} {Power.Id.Entry} to {base.Actor.ModelId.Entry}";
		}
	}

	public PowerReceivedEntry(PowerModel power, decimal amount, Creature? applier, int roundNumber, CombatSide currentSide, CombatHistory history)
		: base(power.Owner, roundNumber, currentSide, history)
	{
		Power = power;
		Amount = amount;
		Applier = applier;
	}
}
