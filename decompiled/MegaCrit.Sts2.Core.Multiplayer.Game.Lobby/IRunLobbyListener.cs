using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Lobby;

namespace MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;

public interface IRunLobbyListener
{
	ClientRejoinResponseMessage GetRejoinMessage();

	void LocalPlayerDisconnected(NetErrorInfo info);

	void RunAbandoned();
}
