using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Flavor;

/// <summary>
/// Sent when a player presses the pencil button at the bottom-left of the map screen.
/// </summary>
public struct MapDrawingModeChangedMessage : INetMessage, IPacketSerializable
{
	public DrawingMode drawingMode;

	public bool ShouldBroadcast => true;

	public NetTransferMode Mode => NetTransferMode.Unreliable;

	public LogLevel LogLevel => LogLevel.VeryDebug;

	public bool ShouldBuffer => true;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteEnum(drawingMode);
	}

	public void Deserialize(PacketReader reader)
	{
		drawingMode = reader.ReadEnum<DrawingMode>();
	}
}
