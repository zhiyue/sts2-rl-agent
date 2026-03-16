using System.Collections.Generic;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Runs.History;

public class MapPointRoomHistoryEntry : IPacketSerializable
{
	[JsonPropertyName("room_type")]
	public RoomType RoomType { get; set; }

	[JsonPropertyName("model_id")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotTypeDefault)]
	public ModelId? ModelId { get; set; }

	[JsonPropertyName("monster_ids")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<ModelId> MonsterIds { get; set; } = new List<ModelId>();

	[JsonPropertyName("turns_taken")]
	public int TurnsTaken { get; set; }

	public void Serialize(PacketWriter writer)
	{
		writer.WriteEnum(RoomType);
		writer.WriteBool(ModelId != null);
		if (ModelId != null)
		{
			writer.WriteFullModelId(ModelId);
		}
		writer.WriteModelEntriesInList(MonsterIds);
		writer.WriteInt(TurnsTaken);
	}

	public void Deserialize(PacketReader reader)
	{
		RoomType = reader.ReadEnum<RoomType>();
		if (reader.ReadBool())
		{
			ModelId = reader.ReadFullModelId();
		}
		MonsterIds = reader.ReadModelIdListAssumingType<MonsterModel>();
		TurnsTaken = reader.ReadInt();
	}
}
