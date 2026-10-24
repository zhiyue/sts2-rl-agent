namespace MegaCrit.Sts2.Core.Entities.Multiplayer;

public enum ActionSynchronizerCombatState
{
	/// <summary>
	/// We're not in combat.
	/// </summary>
	NotInCombat,
	/// <summary>
	/// We're in combat, and the player is fully in control.
	/// </summary>
	PlayPhase,
	/// <summary>
	/// We're in combat, and we're in phase one of ending the turn.
	/// Queues are unpaused during this time to allow generated hook actions (like Well-Laid Plans) to execute, but all
	/// other types of actions will be automatically canceled to prevent interleaving of actions with hooks.
	/// </summary>
	EndTurnPhaseOne,
	/// <summary>
	/// We're in combat, and the player is not fully in control. This encapsulates the enemy turn and the draw phase of
	/// the player's turn.
	/// </summary>
	NotPlayPhase
}
