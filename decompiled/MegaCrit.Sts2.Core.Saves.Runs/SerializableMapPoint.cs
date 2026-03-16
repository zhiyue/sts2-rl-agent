using System.Collections.Generic;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Saves.Runs;

public class SerializableMapPoint : IPacketSerializable
{
	[JsonPropertyName("coord")]
	public MapCoord Coord { get; set; }

	[JsonPropertyName("type")]
	public MapPointType PointType { get; set; }

	[JsonPropertyName("can_modify")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public bool CanBeModified { get; set; } = true;

	[JsonPropertyName("children")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public List<MapCoord>? ChildCoords { get; set; }

	public void Serialize(PacketWriter writer)
	{
		writer.Write(Coord);
		writer.WriteInt((int)PointType, 8);
		writer.WriteBool(CanBeModified);
		int val = ChildCoords?.Count ?? 0;
		writer.WriteInt(val, 8);
		if (ChildCoords == null)
		{
			return;
		}
		foreach (MapCoord childCoord in ChildCoords)
		{
			writer.Write(childCoord);
		}
	}

	public void Deserialize(PacketReader reader)
	{
		Coord = reader.Read<MapCoord>();
		PointType = (MapPointType)reader.ReadInt(8);
		CanBeModified = reader.ReadBool();
		int num = reader.ReadInt(8);
		if (num > 0)
		{
			ChildCoords = new List<MapCoord>(num);
			for (int i = 0; i < num; i++)
			{
				ChildCoords.Add(reader.Read<MapCoord>());
			}
		}
	}

	public static SerializableMapPoint FromMapPoint(MapPoint point)
	{
		List<MapCoord> list = null;
		if (point.Children.Count > 0)
		{
			list = new List<MapCoord>(point.Children.Count);
			foreach (MapPoint child in point.Children)
			{
				list.Add(child.coord);
			}
		}
		return new SerializableMapPoint
		{
			Coord = point.coord,
			PointType = point.PointType,
			CanBeModified = point.CanBeModified,
			ChildCoords = list
		};
	}
}
