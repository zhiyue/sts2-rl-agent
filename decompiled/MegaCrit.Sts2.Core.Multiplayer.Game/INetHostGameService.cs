using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace MegaCrit.Sts2.Core.Multiplayer.Game;

public interface INetHostGameService : INetGameService
{
	IReadOnlyList<NetClientData> ConnectedPeers { get; }

	NetHost? NetHost { get; }

	event Action<ulong>? ClientConnected;

	event Action<ulong, NetErrorInfo>? ClientDisconnected;

	void DisconnectClient(ulong peerId, NetError reason, bool now = false);

	void SetPeerReadyForBroadcasting(ulong peerId);
}
