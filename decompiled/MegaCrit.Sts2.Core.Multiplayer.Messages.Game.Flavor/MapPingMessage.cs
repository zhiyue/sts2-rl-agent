using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Flavor;

/// <summary>
/// Sent when a player clicks on a map coordinate that they've already voted on.
/// </summary>
public struct MapPingMessage : INetMessage, IPacketSerializable
{
	public required MapCoord coord;

	public bool ShouldBroadcast => true;

	public NetTransferMode Mode => NetTransferMode.Unreliable;

	public LogLevel LogLevel => LogLevel.VeryDebug;

	public bool ShouldBuffer => true;

	public void Serialize(PacketWriter writer)
	{
		writer.Write(coord);
	}

	public void Deserialize(PacketReader reader)
	{
		coord = reader.Read<MapCoord>();
	}
}
