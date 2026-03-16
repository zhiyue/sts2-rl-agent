using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Flavor;

public struct MapDrawingModeChangedMessage : INetMessage, IPacketSerializable
{
	public DrawingMode drawingMode;

	public bool ShouldBroadcast => true;

	public NetTransferMode Mode => NetTransferMode.Unreliable;

	public LogLevel LogLevel => LogLevel.VeryDebug;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteEnum(drawingMode);
	}

	public void Deserialize(PacketReader reader)
	{
		drawingMode = reader.ReadEnum<DrawingMode>();
	}
}
