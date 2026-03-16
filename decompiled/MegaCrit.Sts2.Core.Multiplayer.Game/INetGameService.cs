using System;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Quality;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Platform;

namespace MegaCrit.Sts2.Core.Multiplayer.Game;

public interface INetGameService
{
	ulong NetId { get; }

	bool IsConnected { get; }

	bool IsGameLoading { get; }

	NetGameType Type { get; }

	PlatformType Platform { get; }

	event Action<NetErrorInfo>? Disconnected;

	void SendMessage<T>(T message, ulong playerId) where T : INetMessage;

	void SendMessage<T>(T message) where T : INetMessage;

	void RegisterMessageHandler<T>(MessageHandlerDelegate<T> messageHandlerDelegate) where T : INetMessage;

	void UnregisterMessageHandler<T>(MessageHandlerDelegate<T> messageHandlerDelegate) where T : INetMessage;

	void Update();

	void Disconnect(NetError reason, bool now = false);

	ConnectionStats? GetStatsForPeer(ulong peerId);

	void SetGameLoading(bool isLoading);

	string? GetRawLobbyIdentifier();
}
