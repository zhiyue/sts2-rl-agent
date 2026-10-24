namespace MegaCrit.Sts2.Core.Entities.Multiplayer;

public enum GameActionType
{
	None,
	/// <summary>
	/// Canceled if in the queue when we reach the end of combat, or if it is enqueued while we are not in combat.
	/// </summary>
	Combat,
	/// <summary>
	/// Canceled if in the queue when we reach the end of combat, or if it is enqueued while we are not in combat.
	/// If it is requested to be enqueued outside of the player's play phase, the request is deferred until it becomes
	/// the play phase for the local player.
	/// This is used for things like card plays and potion uses so that they occur only after the player turn is completely
	/// set up.
	/// </summary>
	CombatPlayPhaseOnly,
	/// <summary>
	/// Not canceled at the end of combat.
	/// If the action reaches the front of queue during combat, it will not be executed until the combat ends.
	/// </summary>
	NonCombat,
	/// <summary>
	/// Not canceled at the end of combat, and can be executed at any time (in or out of combat).
	/// </summary>
	Any
}
