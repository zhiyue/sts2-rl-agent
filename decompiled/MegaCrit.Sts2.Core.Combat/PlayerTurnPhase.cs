namespace MegaCrit.Sts2.Core.Combat;

/// <summary>
/// Marks the phase of the player's turn in combat.
/// </summary>
public enum PlayerTurnPhase
{
	/// <summary>
	/// No phase. Used when combat is not in progress and during the enemy's turn.
	/// </summary>
	None,
	/// <summary>
	/// The start of the turn.
	/// The player is in this phase from the moment combat switches over from the enemy's turn until the player is
	/// allowed to play cards.
	///
	/// This encompasses many things, including:
	/// - Block clear
	/// - <see cref="M:MegaCrit.Sts2.Core.Hooks.Hook.AfterSideTurnStart(MegaCrit.Sts2.Core.Combat.ICombatState,MegaCrit.Sts2.Core.Combat.CombatSide,System.Collections.Generic.IReadOnlyList{MegaCrit.Sts2.Core.Entities.Creatures.Creature})" />
	/// - Start-of-turn orb passive triggers
	/// - Energy reset
	/// - Hand draw
	/// </summary>
	Start,
	/// <summary>
	/// The phase of the turn where start-of-turn auto-play effects happen.
	/// This is when <see cref="M:MegaCrit.Sts2.Core.Hooks.Hook.AfterAutoPrePlayPhaseEntered(MegaCrit.Sts2.Core.GameActions.Multiplayer.HookPlayerChoiceContext,MegaCrit.Sts2.Core.Combat.ICombatState,MegaCrit.Sts2.Core.Entities.Players.Player)" /> is called.
	///
	/// Examples:
	/// - <see cref="T:MegaCrit.Sts2.Core.Models.Relics.HistoryCourse" />
	/// - <see cref="T:MegaCrit.Sts2.Core.Models.Enchantments.Imbued" />
	/// - <see cref="T:MegaCrit.Sts2.Core.Models.Relics.WhisperingEarring" />
	/// </summary>
	AutoPrePlay,
	/// <summary>
	/// The phase of the turn where the player is allowed to manually play cards and use potions.
	/// In this phase, hooks only happen in response to player actions.
	/// </summary>
	Play,
	/// <summary>
	/// The phase of the turn where end-of-turn auto-play effects happen.
	/// This is when <see cref="M:MegaCrit.Sts2.Core.Hooks.Hook.AfterAutoPostPlayPhaseEntered(MegaCrit.Sts2.Core.GameActions.Multiplayer.HookPlayerChoiceContext,MegaCrit.Sts2.Core.Combat.ICombatState,MegaCrit.Sts2.Core.Entities.Players.Player)" /> is called.
	///
	/// Examples:
	/// - <see cref="T:MegaCrit.Sts2.Core.Models.Cards.IAmInvincible" />
	/// - <see cref="T:MegaCrit.Sts2.Core.Models.Powers.StampedePower" />
	/// </summary>
	AutoPostPlay,
	/// <summary>
	/// The end of the turn.
	/// The player is in this phase from the moment all players have hit "end turn" and all queued cards have finished
	/// playing until combat switches over to the enemy's turn.
	///
	/// This encompasses many things, including:
	/// - <see cref="M:MegaCrit.Sts2.Core.Hooks.Hook.BeforeTurnEnd(MegaCrit.Sts2.Core.Combat.ICombatState,MegaCrit.Sts2.Core.Combat.CombatSide,System.Collections.Generic.IEnumerable{MegaCrit.Sts2.Core.Entities.Creatures.Creature})" />
	/// - End-of-turn orb passive triggers
	/// - Hand flush
	/// - <see cref="M:MegaCrit.Sts2.Core.Hooks.Hook.AfterTurnEnd(MegaCrit.Sts2.Core.Combat.ICombatState,MegaCrit.Sts2.Core.Combat.CombatSide,System.Collections.Generic.IEnumerable{MegaCrit.Sts2.Core.Entities.Creatures.Creature})" />
	/// </summary>
	End
}
