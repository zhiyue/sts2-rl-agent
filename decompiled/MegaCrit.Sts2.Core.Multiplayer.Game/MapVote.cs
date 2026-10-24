using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Multiplayer.Game;

/// <summary>
/// Represents a player's vote for the map coordinate to move to next.
/// Used in multiplayer to tally player votes and move on to the one that the majority have voted for.
/// </summary>
public struct MapVote : IPacketSerializable
{
	/// <summary>
	/// How many map generations have occurred before this map vote was received.
	/// Used to invalidate map votes for old maps, in the case that a map was generated multiple times for the same act.
	/// See Golden Compass.
	/// </summary>
	public int mapGenerationCount;

	/// <summary>
	/// The coordinate within the map the player has voted for.
	/// </summary>
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
