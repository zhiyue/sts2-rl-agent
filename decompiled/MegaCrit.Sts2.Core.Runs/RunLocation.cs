using System;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Runs;

public struct RunLocation : IEquatable<RunLocation>, IComparable<RunLocation>, IPacketSerializable
{
	public int actIndex;

	public MapCoord? coord;

	public RunLocation(MapCoord? coord, int actIndex)
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

	public static bool operator ==(RunLocation first, RunLocation second)
	{
		return first.Equals(second);
	}

	public static bool operator !=(RunLocation first, RunLocation second)
	{
		return !(first == second);
	}

	public bool Equals(RunLocation other)
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
		if (obj is RunLocation other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (actIndex, coord?.col, coord?.row).GetHashCode();
	}

	public int CompareTo(RunLocation other)
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
