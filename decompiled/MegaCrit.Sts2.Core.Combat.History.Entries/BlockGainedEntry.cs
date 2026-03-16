using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Combat.History.Entries;

public class BlockGainedEntry : CombatHistoryEntry
{
	public int Amount { get; }

	public Creature Receiver => base.Actor;

	public ValueProp Props { get; }

	public CardPlay? CardPlay { get; }

	public override string Description
	{
		get
		{
			string id = GetId(Receiver);
			return $"{id} gained {Amount} block";
		}
	}

	public BlockGainedEntry(int amount, ValueProp props, CardPlay? cardPlay, Creature receiver, int roundNumber, CombatSide currentSide, CombatHistory history)
		: base(receiver, roundNumber, currentSide, history)
	{
		Amount = amount;
		Props = props;
		CardPlay = cardPlay;
	}

	private static string GetId(Creature creature)
	{
		if (!creature.IsPlayer)
		{
			return creature.Monster.Id.Entry;
		}
		return creature.Player.Character.Id.Entry;
	}
}
