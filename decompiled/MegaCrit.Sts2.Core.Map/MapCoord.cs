using System;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Map;

[Serializable]
public struct MapCoord : IEquatable<MapCoord>, IComparable<MapCoord>, IPacketSerializable
{
	[JsonInclude]
	public int col;

	[JsonInclude]
	public int row;

	public MapCoord(int col, int row)
	{
		this.col = col;
		this.row = row;
	}

	public void Serialize(PacketWriter writer)
	{
		writer.WriteByte((byte)col);
		writer.WriteByte((byte)row);
	}

	public void Deserialize(PacketReader reader)
	{
		col = reader.ReadByte();
		row = reader.ReadByte();
	}

	public static bool operator ==(MapCoord first, MapCoord second)
	{
		return first.Equals(second);
	}

	public static bool operator !=(MapCoord first, MapCoord second)
	{
		return !(first == second);
	}

	public bool Equals(MapCoord other)
	{
		if (col == other.col)
		{
			return row == other.row;
		}
		return false;
	}

	public override bool Equals(object? obj)
	{
		if (obj is MapCoord other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (col, row).GetHashCode();
	}

	public int CompareTo(MapCoord other)
	{
		return (col, row).CompareTo((other.col, other.row));
	}

	public override string ToString()
	{
		return $"MapCoord ({col}, {row})";
	}
}
