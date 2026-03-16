using System.Collections.Generic;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Saves.Runs;

public class SerializableRoomSet : IPacketSerializable
{
	[JsonPropertyName("event_ids")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<ModelId> EventIds { get; set; }

	[JsonPropertyName("events_visited")]
	public int EventsVisited { get; set; }

	[JsonPropertyName("normal_encounter_ids")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<ModelId> NormalEncounterIds { get; set; }

	[JsonPropertyName("normal_encounters_visited")]
	public int NormalEncountersVisited { get; set; }

	[JsonPropertyName("elite_encounter_ids")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<ModelId> EliteEncounterIds { get; set; }

	[JsonPropertyName("elite_encounters_visited")]
	public int EliteEncountersVisited { get; set; }

	[JsonPropertyName("boss_encounters_visited")]
	public int BossEncountersVisited { get; set; }

	[JsonPropertyName("boss_id")]
	public ModelId? BossId { get; set; }

	[JsonPropertyName("second_boss_id")]
	public ModelId? SecondBossId { get; set; }

	[JsonPropertyName("ancient_id")]
	public ModelId? AncientId { get; set; }

	public void Serialize(PacketWriter writer)
	{
		writer.WriteModelEntriesInList(EventIds);
		writer.WriteInt(EventsVisited);
		writer.WriteModelEntriesInList(NormalEncounterIds);
		writer.WriteInt(NormalEncountersVisited);
		writer.WriteModelEntriesInList(EliteEncounterIds);
		writer.WriteInt(EliteEncountersVisited);
		writer.WriteInt(BossEncountersVisited);
		writer.WriteBool(BossId != null);
		if (BossId != null)
		{
			writer.WriteModelEntry(BossId);
		}
		writer.WriteBool(SecondBossId != null);
		if (SecondBossId != null)
		{
			writer.WriteModelEntry(SecondBossId);
		}
		writer.WriteBool(AncientId != null);
		if (AncientId != null)
		{
			writer.WriteModelEntry(AncientId);
		}
	}

	public void Deserialize(PacketReader reader)
	{
		EventIds = reader.ReadModelIdListAssumingType<EventModel>();
		EventsVisited = reader.ReadInt();
		NormalEncounterIds = reader.ReadModelIdListAssumingType<EncounterModel>();
		NormalEncountersVisited = reader.ReadInt();
		EliteEncounterIds = reader.ReadModelIdListAssumingType<EncounterModel>();
		EliteEncountersVisited = reader.ReadInt();
		BossEncountersVisited = reader.ReadInt();
		if (reader.ReadBool())
		{
			BossId = reader.ReadModelIdAssumingType<EncounterModel>();
		}
		if (reader.ReadBool())
		{
			SecondBossId = reader.ReadModelIdAssumingType<EncounterModel>();
		}
		if (reader.ReadBool())
		{
			AncientId = reader.ReadModelIdAssumingType<AncientEventModel>();
		}
	}
}
