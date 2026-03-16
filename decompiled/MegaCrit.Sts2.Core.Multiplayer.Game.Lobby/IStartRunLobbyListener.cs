using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;

public interface IStartRunLobbyListener
{
	void PlayerConnected(LobbyPlayer player);

	void PlayerChanged(LobbyPlayer player);

	void AscensionChanged();

	void SeedChanged();

	void ModifiersChanged();

	void MaxAscensionChanged();

	void RemotePlayerDisconnected(LobbyPlayer player);

	void BeginRun(string seed, List<ActModel> acts, IReadOnlyList<ModifierModel> modifiers);

	void LocalPlayerDisconnected(NetErrorInfo info);
}
