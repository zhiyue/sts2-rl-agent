using MegaCrit.Sts2.Core.Entities.Players;

namespace MegaCrit.Sts2.Core.Combat.History.Entries;

public class SummonedEntry : CombatHistoryEntry
{
	public int Amount { get; }

	public override string Description => $"{base.Actor.Player.Character.Id.Entry} summoned {Amount}";

	public SummonedEntry(int amount, Player player, int roundNumber, CombatSide currentSide, CombatHistory history)
	{
		Amount = amount;
		base._002Ector(player.Creature, roundNumber, currentSide, history);
	}
}
