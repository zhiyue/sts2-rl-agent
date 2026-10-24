namespace MegaCrit.Sts2.Core.Entities.Multiplayer;

/// <summary>
/// Sent in the InitialGameInfoMessage. If a client receives this, then they will be disconnected shortly after receiving
/// the message, and they should show a relevant error.
/// This is used specifically during connection. See DisconnectionReason for generic disconnection reasons.
/// </summary>
public enum ConnectionFailureReason
{
	None,
	/// <summary>
	/// The lobby has reached the maximum number of players.
	/// </summary>
	LobbyFull,
	/// <summary>
	/// The host loaded from a save file and the client attempting to join is not one of the players in the save.
	/// </summary>
	NotInSaveGame,
	/// <summary>
	/// The run is already in progress and the client attempting to join is not one of the players in the run.
	/// </summary>
	RunInProgress,
	/// <summary>
	/// The host's game version does not match ours.
	/// </summary>
	VersionMismatch,
	/// <summary>
	/// Either the host has mods that we don't have, or we have mods that the host doesn't have.
	/// </summary>
	ModMismatch
}
