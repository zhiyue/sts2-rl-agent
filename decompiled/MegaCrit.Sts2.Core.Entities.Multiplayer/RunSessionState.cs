namespace MegaCrit.Sts2.Core.Entities.Multiplayer;

/// <summary>
/// The state of the multiplayer game that the client is joining.
/// </summary>
public enum RunSessionState
{
	None,
	InLobby,
	InLoadedLobby,
	Running
}
