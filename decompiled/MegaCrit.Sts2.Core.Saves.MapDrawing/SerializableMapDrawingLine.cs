using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Saves.MapDrawing;

public class SerializableMapDrawingLine : IPacketSerializable
{
	private static readonly QuantizeParams _quantizeParamsX = new QuantizeParams(-3f, 3f, 13);

	private static readonly QuantizeParams _quantizeParamsY = new QuantizeParams(-2f, 2f, 16);

	public bool isEraser;

	public List<Vector2> mapPoints = new List<Vector2>();

	public void Serialize(PacketWriter writer)
	{
		writer.WriteBool(isEraser);
		writer.WriteInt(mapPoints.Count, 16);
		foreach (Vector2 mapPoint in mapPoints)
		{
			writer.WriteVector2(mapPoint, _quantizeParamsX, _quantizeParamsY);
		}
	}

	public void Deserialize(PacketReader reader)
	{
		isEraser = reader.ReadBool();
		int num = reader.ReadInt(16);
		for (int i = 0; i < num; i++)
		{
			mapPoints.Add(reader.ReadVector2(_quantizeParamsX, _quantizeParamsY));
		}
	}
}
