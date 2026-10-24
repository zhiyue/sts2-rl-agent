using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Lobby;

/// <summary>
/// Sent by a player when that player is ready to begin the run.
/// </summary>
public struct LobbyPlayerSetReadyMessage : INetMessage, IPacketSerializable
{
	public bool ready;

	public bool ShouldBroadcast => true;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.VeryDebug;

	public bool ShouldBuffer => true;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteBool(ready);
	}

	public void Deserialize(PacketReader reader)
	{
		ready = reader.ReadBool();
	}
}
