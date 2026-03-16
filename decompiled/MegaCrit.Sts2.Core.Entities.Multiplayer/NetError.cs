namespace MegaCrit.Sts2.Core.Entities.Multiplayer;

public enum NetError
{
	None,
	Quit,
	QuitGameOver,
	HostAbandoned,
	Kicked,
	InvalidJoin,
	CancelledJoin,
	LobbyFull,
	RunInProgress,
	NotInSaveGame,
	VersionMismatch,
	JoinBlockedByUser,
	StateDivergence,
	HandshakeTimeout,
	ModMismatch,
	NoInternet,
	Timeout,
	InternalError,
	UnknownNetworkError,
	TryAgainLater,
	FailedToHost
}
