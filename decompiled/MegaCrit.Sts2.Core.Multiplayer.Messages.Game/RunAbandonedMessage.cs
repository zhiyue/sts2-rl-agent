using System.Runtime.InteropServices;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct RunAbandonedMessage : INetMessage, IPacketSerializable
{
	public bool ShouldBroadcast => false;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.Debug;

	public void Serialize(PacketWriter writer)
	{
	}

	public void Deserialize(PacketReader reader)
	{
	}
}
