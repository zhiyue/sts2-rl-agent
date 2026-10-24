namespace MegaCrit.Sts2.Core.Entities.Multiplayer;

/// <summary>
/// Used in multiplayer to sync which screen the player is on.
/// </summary>
public enum NetScreenType
{
	None,
	/// <summary>
	/// Whichever room is the current room (e.g. the combat room, or the rest site).
	/// </summary>
	Room,
	/// <summary>
	/// Map screen.
	/// </summary>
	Map,
	/// <summary>
	/// Settings screen.
	/// </summary>
	Settings,
	/// <summary>
	/// Compendium screen.
	/// </summary>
	Compendium,
	/// <summary>
	/// Deck view screen.
	/// </summary>
	DeckView,
	/// <summary>
	/// Any combat card pile screen (exhaust, draw).
	/// </summary>
	CardPile,
	/// <summary>
	/// Simple card view screen, e.g. when viewing cards that were transformed by Rebirth
	/// </summary>
	SimpleCardsView,
	/// <summary>
	/// Any card selection screen (Survivor card selection, smith card selection, event card selection).
	/// </summary>
	CardSelection,
	/// <summary>
	/// Run history screen shown at the end of a run.
	/// </summary>
	GameOver,
	/// <summary>
	/// The Pause menu (ESC during gameplay).
	/// </summary>
	PauseMenu,
	/// <summary>
	/// Any reward offer screen.
	/// </summary>
	Rewards,
	/// <summary>
	/// The send feedback screen.
	/// </summary>
	Feedback,
	/// <summary>
	/// The shared relic picking screen at the treasure room.
	/// </summary>
	SharedRelicPicking,
	/// <summary>
	/// The expanded state displayed when a remote player is clicked.
	/// </summary>
	RemotePlayerExpandedState
}
