using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Rooms;

public interface ICombatRoomVisuals
{
	/// <summary>
	/// The encounter (for background, scene/slots).
	/// </summary>
	EncounterModel Encounter { get; }

	/// <summary>
	/// Creatures to display on the ally side.
	/// </summary>
	IEnumerable<Creature> Allies { get; }

	/// <summary>
	/// Creatures to display on the enemy side.
	/// </summary>
	IEnumerable<Creature> Enemies { get; }

	/// <summary>
	/// The act this combat room is being shown in.
	/// Used to get a fallback background scene if the encounter doesn't have one.
	/// </summary>
	ActModel Act { get; }
}
