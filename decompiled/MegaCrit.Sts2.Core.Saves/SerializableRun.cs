using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.Saves.MapDrawing;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Saves;

public class SerializableRun : ISaveSchema, IPacketSerializable
{
	/// <summary>
	/// The schema version of this save.
	/// </summary>
	[JsonPropertyName("schema_version")]
	public int SchemaVersion { get; set; }

	[JsonPropertyName("acts")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<SerializableActModel> Acts { get; set; } = new List<SerializableActModel>();

	[JsonPropertyName("modifiers")]
	public List<SerializableModifier> Modifiers { get; set; } = new List<SerializableModifier>();

	/// <summary>
	/// This is null if the run is not a daily.
	/// Otherwise, it contains the date from the time server of the daily.
	/// </summary>
	[JsonPropertyName("dailyTime")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotTypeDefault)]
	public DateTimeOffset? DailyTime { get; set; }

	[JsonPropertyName("current_act_index")]
	public int CurrentActIndex { get; set; }

	[JsonPropertyName("events_seen")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<ModelId> EventsSeen { get; set; } = new List<ModelId>();

	[JsonPropertyName("pre_finished_room")]
	public SerializableRoom? PreFinishedRoom { get; set; }

	[JsonPropertyName("odds")]
	public SerializableRunOddsSet SerializableOdds { get; set; }

	[JsonPropertyName("shared_relic_grab_bag")]
	public SerializableRelicGrabBag SerializableSharedRelicGrabBag { get; set; }

	[JsonPropertyName("players")]
	public List<SerializablePlayer> Players { get; set; }

	[JsonPropertyName("rng")]
	public SerializableRunRngSet SerializableRng { get; set; }

	/// <summary>
	/// The map coordinates you've visited in the current Act.
	/// </summary>
	[JsonPropertyName("visited_map_coords")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<MapCoord> VisitedMapCoords { get; set; } = new List<MapCoord>();

	[JsonPropertyName("map_point_history")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<List<MapPointHistoryEntry>> MapPointHistory { get; set; } = new List<List<MapPointHistoryEntry>>();

	/// <summary>
	/// When this save was created or last updated.
	/// </summary>
	[JsonPropertyName("save_time")]
	public long SaveTime { get; set; }

	[JsonPropertyName("start_time")]
	public long StartTime { get; set; }

	/// <summary>
	/// The amount of seconds that has elapsed for this run.
	/// </summary>
	[JsonPropertyName("run_time")]
	public long RunTime { get; set; }

	/// <summary>
	/// The exact moment when a Win was clocked on the RunTime. (Currently when you beat the Act 3 boss)
	/// </summary>
	[JsonPropertyName("win_time")]
	public long WinTime { get; set; }

	[JsonPropertyName("ascension")]
	public int Ascension { get; set; }

	[JsonPropertyName("num_reloads")]
	public int NumReloads { get; set; }

	[JsonPropertyName("platform_type")]
	public PlatformType PlatformType { get; set; }

	[JsonConverter(typeof(SerializableMapDrawingsJsonConverter))]
	[JsonPropertyName("map_drawings")]
	public SerializableMapDrawings? MapDrawings { get; set; }

	[JsonPropertyName("extra_fields")]
	public SerializableExtraRunFields ExtraFields { get; set; } = new SerializableExtraRunFields();

	[JsonPropertyName("game_mode")]
	public GameMode GameMode { get; set; }

	/// <summary>
	/// The furthest floor count reached during this run.
	/// Used when uploading the "floor" value when uploading to the daily leaderboards.
	/// </summary>
	[JsonIgnore]
	public int FloorReached => MapPointHistory.Sum((List<MapPointHistoryEntry> c) => c.Count);

	public void Serialize(PacketWriter writer)
	{
		writer.WriteInt(SchemaVersion);
		writer.WriteList(Acts);
		writer.WriteList(Modifiers);
		writer.WriteBool(DailyTime.HasValue);
		if (DailyTime.HasValue)
		{
			writer.WriteLong(DailyTime.Value.ToUnixTimeSeconds());
		}
		writer.WriteEnum(GameMode);
		writer.WriteInt(CurrentActIndex, 4);
		writer.WriteModelEntriesInList(EventsSeen);
		writer.WriteBool(PreFinishedRoom != null);
		if (PreFinishedRoom != null)
		{
			writer.Write(PreFinishedRoom);
		}
		writer.Write(SerializableOdds);
		writer.WriteList(Players);
		writer.Write(SerializableRng);
		writer.Write(SerializableSharedRelicGrabBag);
		writer.WriteList(VisitedMapCoords);
		writer.WriteInt(MapPointHistory.Count);
		foreach (List<MapPointHistoryEntry> item in MapPointHistory)
		{
			writer.WriteList(item);
		}
		writer.WriteLong(SaveTime);
		writer.WriteLong(StartTime);
		writer.WriteLong(RunTime);
		writer.WriteLong(WinTime);
		writer.WriteInt(Ascension, 8);
		writer.WriteBool(MapDrawings != null);
		if (MapDrawings != null)
		{
			writer.Write(MapDrawings);
		}
		writer.Write(ExtraFields);
		writer.WriteInt(NumReloads);
	}

	public void Deserialize(PacketReader reader)
	{
		SchemaVersion = reader.ReadInt();
		Acts = reader.ReadList<SerializableActModel>();
		Modifiers = reader.ReadList<SerializableModifier>();
		if (reader.ReadBool())
		{
			DailyTime = DateTimeOffset.FromUnixTimeSeconds(reader.ReadLong());
		}
		GameMode = reader.ReadEnum<GameMode>();
		CurrentActIndex = reader.ReadInt(4);
		EventsSeen = reader.ReadModelIdListAssumingType<EventModel>();
		if (reader.ReadBool())
		{
			PreFinishedRoom = reader.Read<SerializableRoom>();
		}
		SerializableOdds = reader.Read<SerializableRunOddsSet>();
		Players = reader.ReadList<SerializablePlayer>();
		SerializableRng = reader.Read<SerializableRunRngSet>();
		SerializableSharedRelicGrabBag = reader.Read<SerializableRelicGrabBag>();
		VisitedMapCoords = reader.ReadList<MapCoord>();
		int num = reader.ReadInt();
		for (int i = 0; i < num; i++)
		{
			List<MapPointHistoryEntry> item = reader.ReadList<MapPointHistoryEntry>();
			MapPointHistory.Add(item);
		}
		SaveTime = reader.ReadLong();
		StartTime = reader.ReadLong();
		RunTime = reader.ReadLong();
		WinTime = reader.ReadLong();
		Ascension = reader.ReadInt(8);
		if (reader.ReadBool())
		{
			MapDrawings = reader.Read<SerializableMapDrawings>();
		}
		ExtraFields = reader.Read<SerializableExtraRunFields>();
		NumReloads = reader.ReadInt();
	}

	public SerializableRun Anonymized()
	{
		return new SerializableRun
		{
			SchemaVersion = SchemaVersion,
			Acts = Acts,
			Modifiers = Modifiers,
			DailyTime = DailyTime,
			CurrentActIndex = CurrentActIndex,
			EventsSeen = EventsSeen,
			GameMode = GameMode,
			PreFinishedRoom = PreFinishedRoom,
			SerializableOdds = SerializableOdds,
			SerializableSharedRelicGrabBag = SerializableSharedRelicGrabBag,
			Players = Players.Select((SerializablePlayer p) => p.Anonymized()).ToList(),
			SerializableRng = SerializableRng,
			VisitedMapCoords = VisitedMapCoords,
			MapPointHistory = MapPointHistory.Select((List<MapPointHistoryEntry> l) => l.Select((MapPointHistoryEntry h) => h.Anonymized()).ToList()).ToList(),
			SaveTime = SaveTime,
			StartTime = StartTime,
			RunTime = RunTime,
			WinTime = WinTime,
			Ascension = Ascension,
			PlatformType = PlatformType,
			MapDrawings = MapDrawings?.Anonymized(),
			ExtraFields = ExtraFields
		};
	}
}
