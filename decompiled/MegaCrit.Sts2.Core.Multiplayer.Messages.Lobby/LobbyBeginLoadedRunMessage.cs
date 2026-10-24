using System.Runtime.InteropServices;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Lobby;

/// <summary>
/// Sent when the <see cref="T:MegaCrit.Sts2.Core.Multiplayer.Game.Lobby.LoadRunLobby" /> closes and the run begins.
/// Used only in <see cref="T:MegaCrit.Sts2.Core.Multiplayer.Game.Lobby.LoadRunLobby" />. Lobby uses <see cref="T:MegaCrit.Sts2.Core.Multiplayer.Messages.Lobby.LobbyBeginRunMessage" />.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct LobbyBeginLoadedRunMessage : INetMessage, IPacketSerializable
{
	public bool ShouldBroadcast => true;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.VeryDebug;

	public bool ShouldBuffer => true;

	public void Serialize(PacketWriter writer)
	{
	}

	public void Deserialize(PacketReader reader)
	{
	}
}
