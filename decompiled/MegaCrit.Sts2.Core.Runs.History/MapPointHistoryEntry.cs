using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Runs.History;

public class MapPointHistoryEntry : IPacketSerializable
{
	[JsonPropertyName("map_point_type")]
	public MapPointType MapPointType { get; set; }

	[JsonPropertyName("rooms")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<MapPointRoomHistoryEntry> Rooms { get; set; } = new List<MapPointRoomHistoryEntry>();

	[JsonPropertyName("player_stats")]
	public List<PlayerMapPointHistoryEntry> PlayerStats { get; set; } = new List<PlayerMapPointHistoryEntry>();

	public MapPointHistoryEntry()
	{
	}

	public MapPointHistoryEntry(MapPointType mapPointType, IPlayerCollection playerCollection)
	{
		MapPointType = mapPointType;
		foreach (Player player in playerCollection.Players)
		{
			PlayerStats.Add(new PlayerMapPointHistoryEntry
			{
				PlayerId = player.NetId
			});
		}
	}

	public PlayerMapPointHistoryEntry GetEntry(ulong playerId)
	{
		PlayerMapPointHistoryEntry playerMapPointHistoryEntry = PlayerStats.FirstOrDefault((PlayerMapPointHistoryEntry e) => e.PlayerId == playerId);
		if (playerMapPointHistoryEntry == null)
		{
			throw new InvalidOperationException($"Player with ID {playerId} not found in player stats for this run history! We have {string.Join(",", PlayerStats.Select((PlayerMapPointHistoryEntry p) => p.PlayerId))}");
		}
		return playerMapPointHistoryEntry;
	}

	public bool HasRoomOfType(RoomType roomType)
	{
		foreach (MapPointRoomHistoryEntry room in Rooms)
		{
			if (room.RoomType == roomType)
			{
				return true;
			}
		}
		return false;
	}

	public IEnumerable<MapPointRoomHistoryEntry> GetRoomsOfType(RoomType roomType)
	{
		foreach (MapPointRoomHistoryEntry room in Rooms)
		{
			if (room.RoomType == roomType)
			{
				yield return room;
			}
		}
	}

	public MapPointRoomHistoryEntry? FirstRoomOfType(RoomType roomType)
	{
		foreach (MapPointRoomHistoryEntry room in Rooms)
		{
			if (room.RoomType == roomType)
			{
				return room;
			}
		}
		return null;
	}

	public void Serialize(PacketWriter writer)
	{
		writer.WriteEnum(MapPointType);
		writer.WriteList(Rooms);
		writer.WriteList(PlayerStats);
	}

	public void Deserialize(PacketReader reader)
	{
		MapPointType = reader.ReadEnum<MapPointType>();
		Rooms = reader.ReadList<MapPointRoomHistoryEntry>();
		PlayerStats = reader.ReadList<PlayerMapPointHistoryEntry>();
	}

	public MapPointHistoryEntry Anonymized()
	{
		return new MapPointHistoryEntry
		{
			MapPointType = MapPointType,
			Rooms = Rooms,
			PlayerStats = PlayerStats.Select((PlayerMapPointHistoryEntry p) => p.Anonymized()).ToList()
		};
	}
}
