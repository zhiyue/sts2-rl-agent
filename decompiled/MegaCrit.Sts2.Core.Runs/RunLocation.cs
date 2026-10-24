using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Runs;

/// <summary>
/// Uniquely identifies a location within a run for RunLocationTargetedMessageBuffer.
/// This location takes into account which room the player is in, which is important for multi-room locations, like
/// those that happen in combat events.
/// Make sure you look at <see cref="T:MegaCrit.Sts2.Core.Runs.MapLocation" /> as well, which identifies locations on the map without considering
/// the room.
/// </summary>
public struct RunLocation : IEquatable<RunLocation>, IComparable<RunLocation>, IPacketSerializable
{
	/// <summary>
	/// The act and map coordinate that the location is at.
	/// </summary>
	public MapLocation mapLocation;

	/// <summary>
	/// The room index that this location is in.
	/// Zero is the root room. Any room other than zero is a sub-room, entered using RunManager.EnterRoomWithoutExitingCurrentRoom.
	/// </summary>
	public int? roomId;

	public RunLocation(MapLocation mapLocation, int? roomId)
	{
		this.mapLocation = mapLocation;
		this.roomId = roomId;
	}

	public RunLocation(int actIndex, MapCoord? coord, int? roomId)
	{
		mapLocation = new MapLocation(coord, actIndex);
		this.roomId = roomId;
	}

	public void Serialize(PacketWriter writer)
	{
		writer.WriteBool(roomId.HasValue);
		if (roomId.HasValue)
		{
			writer.WriteInt(roomId.Value, 4);
		}
		writer.Write(mapLocation);
	}

	public void Deserialize(PacketReader reader)
	{
		if (reader.ReadBool())
		{
			roomId = reader.ReadInt(4);
		}
		mapLocation = reader.Read<MapLocation>();
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
		if (roomId == other.roomId)
		{
			return mapLocation == other.mapLocation;
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
		return (roomId, mapLocation.actIndex, mapLocation.coord?.col, mapLocation.coord?.row).GetHashCode();
	}

	public int CompareTo(RunLocation other)
	{
		if (mapLocation != other.mapLocation)
		{
			return mapLocation.CompareTo(other.mapLocation);
		}
		return Comparer<int?>.Default.Compare(roomId, other.roomId);
	}

	public override string ToString()
	{
		return $"{mapLocation} room {roomId}";
	}
}
