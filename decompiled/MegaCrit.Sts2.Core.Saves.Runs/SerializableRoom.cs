using System.Collections.Generic;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Saves.Runs;

public class SerializableRoom : IPacketSerializable
{
	[JsonPropertyName("room_type")]
	public RoomType RoomType { get; set; }

	[JsonPropertyName("encounter_id")]
	public ModelId? EncounterId { get; set; }

	[JsonPropertyName("event_id")]
	public ModelId? EventId { get; set; }

	[JsonPropertyName("is_pre_finished")]
	public bool IsPreFinished { get; set; }

	[JsonPropertyName("reward_proportion")]
	public float GoldProportion { get; set; }

	[JsonPropertyName("extra_rewards")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public Dictionary<ulong, List<SerializableReward>> ExtraRewards { get; set; } = new Dictionary<ulong, List<SerializableReward>>();

	[JsonPropertyName("parent_event_id")]
	public ModelId? ParentEventId { get; set; }

	[JsonPropertyName("should_resume_parent_event")]
	public bool ShouldResumeParentEvent { get; set; } = true;

	[JsonPropertyName("encounter_state")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public Dictionary<string, string> EncounterState { get; set; } = new Dictionary<string, string>();

	public void Serialize(PacketWriter writer)
	{
		writer.WriteInt((int)RoomType);
		writer.WriteBool(EncounterId != null);
		if (EncounterId != null)
		{
			writer.WriteModelEntry(EncounterId);
		}
		writer.WriteBool(EventId != null);
		if (EventId != null)
		{
			writer.WriteModelEntry(EventId);
		}
		writer.WriteFloat(GoldProportion);
		writer.WriteBool(IsPreFinished);
		writer.WriteInt(ExtraRewards.Count);
		foreach (var (val, list2) in ExtraRewards)
		{
			writer.WriteULong(val);
			writer.WriteList(list2);
		}
		writer.WriteBool(ParentEventId != null);
		if (ParentEventId != null)
		{
			writer.WriteModelEntry(ParentEventId);
		}
		writer.WriteBool(ShouldResumeParentEvent);
		writer.WriteInt(EncounterState.Count);
		foreach (var (str, str2) in EncounterState)
		{
			writer.WriteString(str);
			writer.WriteString(str2);
		}
	}

	public void Deserialize(PacketReader reader)
	{
		RoomType = (RoomType)reader.ReadInt();
		if (reader.ReadBool())
		{
			EncounterId = reader.ReadModelIdAssumingType<EncounterModel>();
		}
		if (reader.ReadBool())
		{
			EventId = reader.ReadModelIdAssumingType<EventModel>();
		}
		GoldProportion = reader.ReadFloat();
		IsPreFinished = reader.ReadBool();
		int num = reader.ReadInt();
		for (int i = 0; i < num; i++)
		{
			ulong key = reader.ReadULong();
			ExtraRewards[key] = reader.ReadList<SerializableReward>();
		}
		if (reader.ReadBool())
		{
			ParentEventId = reader.ReadModelIdAssumingType<EventModel>();
		}
		ShouldResumeParentEvent = reader.ReadBool();
		int num2 = reader.ReadInt();
		EncounterState = new Dictionary<string, string>(num2);
		for (int j = 0; j < num2; j++)
		{
			string key2 = reader.ReadString();
			string value = reader.ReadString();
			EncounterState[key2] = value;
		}
	}
}
