using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;

namespace MegaCrit.Sts2.Core.Combat.History.Entries;

public class CreatureAttackedEntry : CombatHistoryEntry
{
	public IReadOnlyList<DamageResult> DamageResults { get; }

	public override string Description => base.Actor.Name + " attacked";

	public CreatureAttackedEntry(Creature attacker, IReadOnlyList<DamageResult> damageResults, int roundNumber, CombatSide currentSide, CombatHistory history, IEnumerable<Player> players)
		: base(attacker, roundNumber, currentSide, history, players)
	{
		DamageResults = damageResults;
	}
}
