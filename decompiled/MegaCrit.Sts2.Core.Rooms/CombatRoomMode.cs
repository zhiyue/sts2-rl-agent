namespace MegaCrit.Sts2.Core.Rooms;

/// <summary>
/// The mode that the combat room is in.
/// This allows for combat rooms to be used in many different scenarios. See the specific options for details.
/// </summary>
public enum CombatRoomMode
{
	/// <summary>
	/// Full combat - shows all creatures, combat UI, enables interactivity, runs hooks.
	/// </summary>
	ActiveCombat,
	/// <summary>
	/// Finished combat - shows players only, no combat UI, no interactivity, no hooks.
	/// Used when loading a save after combat ended but before leaving the room.
	/// </summary>
	FinishedCombat,
	/// <summary>
	/// Visual only - shows all creatures (allies and enemies) but no combat UI, no interactivity, no hooks.
	/// Used for combat-style events where enemies are visible but not yet fightable.
	/// </summary>
	VisualOnly
}
