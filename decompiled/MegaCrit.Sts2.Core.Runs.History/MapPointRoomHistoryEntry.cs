using System.Collections.Generic;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Runs.History;

/// <summary>
/// Represents a room in a map point.
/// Since each map point can contain multiple rooms (e.g. <see cref="T:MegaCrit.Sts2.Core.Models.Events.DenseVegetation" />'s
/// <see cref="T:MegaCrit.Sts2.Core.Models.Encounters.DenseVegetationEventEncounter" />), we need multiple of these per map point.
/// </summary>
public class MapPointRoomHistoryEntry : IPacketSerializable
{
	/// <summary>
	/// The type of room this is. Tells us what type of model ModelId refers to.
	/// </summary>
	[JsonPropertyName("room_type")]
	public RoomType RoomType { get; set; }

	/// <summary>
	/// The model id of the encounter or event at each room in this map point. ie CEREMONIAL_BEAST
	/// </summary>
	[JsonPropertyName("model_id")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotTypeDefault)]
	public ModelId? ModelId { get; set; }

	/// <summary>
	/// The model IDs of the monsters in the encounter, if this was an encounter.
	/// This contains the actual monsters that the encounter resolved to, if the encounter has random monsters.
	/// </summary>
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
