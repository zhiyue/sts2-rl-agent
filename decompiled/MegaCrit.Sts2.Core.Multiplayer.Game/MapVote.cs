using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Multiplayer.Game;

public struct MapVote : IPacketSerializable
{
	public int mapGenerationCount;

	public MapCoord coord;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteInt(mapGenerationCount, 4);
		writer.Write(coord);
	}

	public void Deserialize(PacketReader reader)
	{
		mapGenerationCount = reader.ReadInt(4);
		coord = reader.Read<MapCoord>();
	}

	public override string ToString()
	{
		return $"{"MapVote"} (gen: {mapGenerationCount} coord: ({coord.col}, {coord.row}))";
	}
}
