using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Lobby;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Game;

public struct JoinResult
{
	public GameMode gameMode;

	public RunSessionState? sessionState;

	public ClientRejoinResponseMessage? rejoinResponse;

	public ClientLoadJoinResponseMessage? loadJoinResponse;

	public ClientLobbyJoinResponseMessage? joinResponse;
}
