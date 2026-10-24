using System;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Runs;

/// <summary>
/// Identifies a coordinate on the map that the player is at.
/// Contains an act index + map coordinate.
/// Make sure you look at <see cref="T:MegaCrit.Sts2.Core.Runs.RunLocation" /> as well, which also includes a room identifier. This class is more
/// suitable for situations where the room doesn't matter, e.g. map voting.
/// </summary>
public struct MapLocation : IEquatable<MapLocation>, IComparable<MapLocation>, IPacketSerializable
{
	/// <summary>
	/// The act that this location is in.
	/// </summary>
	public int actIndex;

	/// <summary>
	/// The coordinate that this location is at.
	/// Will be null when we're in the map room (at the start of an act, before picking the ancient map point).
	/// </summary>
	public MapCoord? coord;

	public MapLocation(MapCoord? coord, int actIndex)
	{
		this.coord = coord;
		this.actIndex = actIndex;
	}

	public void Serialize(PacketWriter writer)
	{
		writer.WriteInt(actIndex, 4);
		writer.WriteBool(coord.HasValue);
		if (coord.HasValue)
		{
			writer.Write(coord.Value);
		}
	}

	public void Deserialize(PacketReader reader)
	{
		actIndex = reader.ReadInt(4);
		if (reader.ReadBool())
		{
			coord = reader.Read<MapCoord>();
		}
	}

	public static bool operator ==(MapLocation first, MapLocation second)
	{
		return first.Equals(second);
	}

	public static bool operator !=(MapLocation first, MapLocation second)
	{
		return !(first == second);
	}

	public bool Equals(MapLocation other)
	{
		if (actIndex == other.actIndex)
		{
			MapCoord? mapCoord = coord;
			MapCoord? mapCoord2 = other.coord;
			if (mapCoord.HasValue != mapCoord2.HasValue)
			{
				return false;
			}
			if (!mapCoord.HasValue)
			{
				return true;
			}
			return mapCoord.GetValueOrDefault() == mapCoord2.GetValueOrDefault();
		}
		return false;
	}

	public override bool Equals(object? obj)
	{
		if (obj is MapLocation other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (actIndex, coord?.col, coord?.row).GetHashCode();
	}

	public int CompareTo(MapLocation other)
	{
		if (actIndex != other.actIndex)
		{
			return actIndex.CompareTo(other.actIndex);
		}
		if (!coord.HasValue && !other.coord.HasValue)
		{
			return 0;
		}
		if (!coord.HasValue && other.coord.HasValue)
		{
			return -1;
		}
		if (coord.HasValue && !other.coord.HasValue)
		{
			return 1;
		}
		return coord.Value.CompareTo(other.coord.Value);
	}

	public override string ToString()
	{
		return $"act {actIndex} coord ({(coord.HasValue ? $"{coord.Value.col}, {coord.Value.row}" : "null")})";
	}
}
