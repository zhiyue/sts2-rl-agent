namespace MegaCrit.Sts2.Core.Entities.UI;

/// <summary>
/// Some UI elements are shared between screens in multiplayer UI. This enumeration describes what mode they should be
/// placed in depending on what screen is currently open.
/// </summary>
public enum MultiplayerUiMode
{
	/// <summary>
	/// Invalid!
	/// </summary>
	None,
	/// <summary>
	/// We are starting a new singleplayer game.
	/// Max and preferred ascension is per-character.
	/// Custom modifiers are modifiable.
	/// </summary>
	Singleplayer,
	/// <summary>
	/// We are hosting a new multiplayer game.
	/// In multiplayer, preferred ascension is per-player. Max ascension is the minimum of the ascensions of all the
	/// players in the lobby.
	/// The player is able to change the ascension and any custom modifiers.
	/// </summary>
	Host,
	/// <summary>
	/// We have joined a new multiplayer game.
	/// In client mode, the player is not able to change the ascension or custom modifiers. When the host changes either
	/// the clients simply reflect what the host has set.
	/// </summary>
	Client,
	/// <summary>
	/// We have either hosted or joined a multiplayer game loaded from save file.
	/// When loading a multiplayer game, no players are able to change any run data.
	/// Note that this does not include singleplayer loads - those simply go straight to the run with no intermediate screen.
	/// </summary>
	Load
}
