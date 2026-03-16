using MegaCrit.Sts2.Core.Entities.Creatures;

namespace MegaCrit.Sts2.Core.Combat.History;

public abstract class CombatHistoryEntry
{
	public Creature Actor { get; }

	public int RoundNumber { get; }

	public CombatSide CurrentSide { get; }

	public CombatHistory History { get; }

	public string HumanReadableString => $"Rd {RoundNumber} ({CurrentSide} turn): {Description}.";

	public abstract string Description { get; }

	protected CombatHistoryEntry(Creature actor, int roundNumber, CombatSide currentSide, CombatHistory history)
	{
		Actor = actor;
		RoundNumber = roundNumber;
		CurrentSide = currentSide;
		History = history;
	}

	public bool HappenedThisTurn(CombatState? state)
	{
		if (state == null)
		{
			return false;
		}
		if (RoundNumber == state.RoundNumber)
		{
			return CurrentSide == state.CurrentSide;
		}
		return false;
	}
}
