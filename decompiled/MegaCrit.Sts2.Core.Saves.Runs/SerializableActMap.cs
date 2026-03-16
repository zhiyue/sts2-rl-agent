using System.Collections.Generic;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Saves.Runs;

public class SerializableActMap : IPacketSerializable
{
	[JsonPropertyName("points")]
	public List<SerializableMapPoint> Points { get; set; } = new List<SerializableMapPoint>();

	[JsonPropertyName("boss")]
	public SerializableMapPoint BossPoint { get; set; }

	[JsonPropertyName("second_boss")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public SerializableMapPoint? SecondBossPoint { get; set; }

	[JsonPropertyName("start")]
	public SerializableMapPoint StartingPoint { get; set; }

	[JsonPropertyName("start_coords")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public List<MapCoord>? StartMapPointCoords { get; set; }

	[JsonPropertyName("width")]
	public int GridWidth { get; set; }

	[JsonPropertyName("height")]
	public int GridHeight { get; set; }

	public void Serialize(PacketWriter writer)
	{
		writer.WriteInt(GridWidth, 8);
		writer.WriteInt(GridHeight, 8);
		writer.Write(BossPoint);
		writer.Write(StartingPoint);
		writer.WriteBool(SecondBossPoint != null);
		if (SecondBossPoint != null)
		{
			writer.Write(SecondBossPoint);
		}
		writer.WriteInt(Points.Count, 16);
		foreach (SerializableMapPoint point in Points)
		{
			writer.Write(point);
		}
		int val = StartMapPointCoords?.Count ?? 0;
		writer.WriteInt(val, 8);
		if (StartMapPointCoords == null)
		{
			return;
		}
		foreach (MapCoord startMapPointCoord in StartMapPointCoords)
		{
			writer.Write(startMapPointCoord);
		}
	}

	public void Deserialize(PacketReader reader)
	{
		GridWidth = reader.ReadInt(8);
		GridHeight = reader.ReadInt(8);
		BossPoint = reader.Read<SerializableMapPoint>();
		StartingPoint = reader.Read<SerializableMapPoint>();
		if (reader.ReadBool())
		{
			SecondBossPoint = reader.Read<SerializableMapPoint>();
		}
		int num = reader.ReadInt(16);
		Points = new List<SerializableMapPoint>(num);
		for (int i = 0; i < num; i++)
		{
			Points.Add(reader.Read<SerializableMapPoint>());
		}
		int num2 = reader.ReadInt(8);
		if (num2 > 0)
		{
			StartMapPointCoords = new List<MapCoord>(num2);
			for (int j = 0; j < num2; j++)
			{
				StartMapPointCoords.Add(reader.Read<MapCoord>());
			}
		}
	}

	public static SerializableActMap FromActMap(ActMap map)
	{
		List<SerializableMapPoint> list = new List<SerializableMapPoint>();
		foreach (MapPoint allMapPoint in map.GetAllMapPoints())
		{
			list.Add(SerializableMapPoint.FromMapPoint(allMapPoint));
		}
		List<MapCoord> list2 = null;
		if (map.startMapPoints.Count > 0)
		{
			list2 = new List<MapCoord>();
			foreach (MapPoint startMapPoint in map.startMapPoints)
			{
				list2.Add(startMapPoint.coord);
			}
		}
		return new SerializableActMap
		{
			Points = list,
			BossPoint = SerializableMapPoint.FromMapPoint(map.BossMapPoint),
			SecondBossPoint = ((map.SecondBossMapPoint != null) ? SerializableMapPoint.FromMapPoint(map.SecondBossMapPoint) : null),
			StartingPoint = SerializableMapPoint.FromMapPoint(map.StartingMapPoint),
			StartMapPointCoords = list2,
			GridWidth = map.GetColumnCount(),
			GridHeight = map.GetRowCount()
		};
	}
}
