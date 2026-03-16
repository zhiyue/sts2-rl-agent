using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Multiplayer;

namespace MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;

public interface ILoadRunLobbyListener
{
	void PlayerConnected(ulong playerId);

	void RemotePlayerDisconnected(ulong playerId);

	Task<bool> ShouldAllowRunToBegin();

	void BeginRun();

	void PlayerReadyChanged(ulong playerId);

	void LocalPlayerDisconnected(NetErrorInfo info);
}
