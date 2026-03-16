using Godot;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace MegaCrit.Sts2.Core.Multiplayer.Game.PeerInput;

public struct NetMapDrawingEvent : IPacketSerializable
{
	private static readonly QuantizeParams _quantizeParamsX = new QuantizeParams(-3f, 3f, 16);

	private static readonly QuantizeParams _quantizeParamsY = new QuantizeParams(-2f, 2f, 24);

	public MapDrawingEventType type;

	public DrawingMode? overrideDrawingMode;

	public Vector2 position;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteEnum(type);
		writer.WriteBool(overrideDrawingMode.HasValue);
		if (overrideDrawingMode.HasValue)
		{
			writer.WriteEnum(overrideDrawingMode.Value);
		}
		if (type != MapDrawingEventType.EndLine)
		{
			writer.WriteVector2(position, _quantizeParamsX, _quantizeParamsY);
		}
	}

	public void Deserialize(PacketReader reader)
	{
		type = reader.ReadEnum<MapDrawingEventType>();
		if (reader.ReadBool())
		{
			overrideDrawingMode = reader.ReadEnum<DrawingMode>();
		}
		if (type != MapDrawingEventType.EndLine)
		{
			position = reader.ReadVector2(_quantizeParamsX, _quantizeParamsY);
		}
	}
}
