using System.Runtime.InteropServices;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game;

/// <summary>
/// Sent by the host to all clients when the host abandons the run.
/// The clients should also abandon the run.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct RunAbandonedMessage : INetMessage, IPacketSerializable
{
	public bool ShouldBroadcast => false;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.Debug;

	public bool ShouldBuffer => true;

	public void Serialize(PacketWriter writer)
	{
	}

	public void Deserialize(PacketReader reader)
	{
	}
}
