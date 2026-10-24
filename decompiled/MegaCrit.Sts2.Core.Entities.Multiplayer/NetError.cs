namespace MegaCrit.Sts2.Core.Entities.Multiplayer;

/// <summary>
/// A list of reasons for which we may be disconnected from a remote host. Each of these is associated with a particular
/// error message that we can show to the user.
/// It's likely that this list will expand over time.
/// </summary>
public enum NetError
{
	/// <summary>
	/// No reason was passed.
	/// </summary>
	None,
	/// <summary>
	/// Normal quit (Host save and quit or quit the application).
	/// </summary>
	Quit,
	/// <summary>
	/// Normal quit at the end of the run. Signals to clients they should not also quit the application.
	/// </summary>
	QuitGameOver,
	/// <summary>
	/// Host abandoned the game without saving it.
	/// </summary>
	HostAbandoned,
	/// <summary>
	/// We were forcibly removed from the game.
	/// </summary>
	Kicked,
	/// <summary>
	/// Tried to join a user that is not currently in a multiplayer game.
	/// </summary>
	InvalidJoin,
	/// <summary>
	/// The user cancelled the join flow before it was completed.
	/// </summary>
	CancelledJoin,
	/// <summary>The lobby we tried to connect to is full.</summary>
	LobbyFull,
	/// <summary>
	/// The run is already in progress, and rejoining is not implemented.
	/// </summary>
	RunInProgress,
	/// <summary>
	/// The run was loaded from a save file, and the player attempting to connect is not in the save.
	/// </summary>
	NotInSaveGame,
	/// <summary>
	/// The host's version does not match the client's.
	/// </summary>
	VersionMismatch,
	/// <summary>
	/// You are banned from the lobby, you have blocked someone in the lobby, or someone in the lobby blocked you.
	/// </summary>
	JoinBlockedByUser,
	/// <summary>
	/// Our state, as a client, diverged from the host's during combat.
	/// </summary>
	StateDivergence,
	/// <summary>
	/// The client did not send the lobby join handshake response in time.
	/// Different from an internet timeout, as that is below the application layer.
	/// </summary>
	HandshakeTimeout,
	/// <summary>
	/// Either the host had mods that we didn't have, or we had mods that the host didn't have.
	/// </summary>
	ModMismatch,
	/// <summary>
	/// Couldn't connect to the session, likely because of internet issues.
	/// </summary>
	NoInternet,
	/// <summary>
	/// Connection timed out.
	/// </summary>
	Timeout,
	/// <summary>
	/// Internal error, like an exception or a similar local bug.
	/// </summary>
	InternalError,
	/// <summary>
	/// Network issue that we are not sure how to diagnose.
	/// </summary>
	UnknownNetworkError,
	/// <summary>
	/// Too many attempts to do the same thing. Player should try again in a bit.
	/// </summary>
	RateLimited,
	/// <summary>
	/// Generic transient issue. Player should try again later.
	/// </summary>
	TryAgainLater,
	/// <summary>
	/// Hosting the game failed.
	/// </summary>
	FailedToHost,
	/// <summary>
	/// Couldn't make secure connection (Steam BadCert and BadCrypt). Most common cause is out-of-sync clocks.
	/// </summary>
	SecureConnectionFailed
}
