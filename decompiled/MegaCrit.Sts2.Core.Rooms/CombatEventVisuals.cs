using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Rooms;

public class CombatEventVisuals : ICombatRoomVisuals
{
	public EncounterModel Encounter { get; }

	public IEnumerable<Creature> Allies { get; }

	public IEnumerable<Creature> Enemies => Encounter.MonstersWithSlots.Select<(MonsterModel, string), Creature>(((MonsterModel, string) m) => m.Item1.Creature);

	public ActModel Act { get; }

	public CombatEventVisuals(EncounterModel encounter, IEnumerable<Player> players, ActModel act)
	{
		Encounter = encounter;
		Allies = players.Select((Player p) => p.Creature);
		Act = act;
	}
}
