using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Rooms;

public interface ICombatRoomVisuals
{
	EncounterModel Encounter { get; }

	IEnumerable<Creature> Allies { get; }

	IEnumerable<Creature> Enemies { get; }

	ActModel Act { get; }
}
