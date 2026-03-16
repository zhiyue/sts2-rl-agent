using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Saves.Runs;

public class SerializableRelicGrabBag : IPacketSerializable
{
	[JsonPropertyName("relic_id_lists")]
	public Dictionary<RelicRarity, List<ModelId>> RelicIdLists { get; set; } = new Dictionary<RelicRarity, List<ModelId>>();

	public void Serialize(PacketWriter writer)
	{
		writer.WriteInt(RelicIdLists.Count);
		RelicRarity[] values = Enum.GetValues<RelicRarity>();
		foreach (RelicRarity relicRarity in values)
		{
			if (RelicIdLists.TryGetValue(relicRarity, out List<ModelId> value))
			{
				writer.WriteEnum(relicRarity);
				writer.WriteModelEntriesInList(value);
			}
		}
	}

	public void Deserialize(PacketReader reader)
	{
		int num = reader.ReadInt();
		for (int i = 0; i < num; i++)
		{
			RelicRarity key = reader.ReadEnum<RelicRarity>();
			List<ModelId> value = reader.ReadModelIdListAssumingType<RelicModel>();
			RelicIdLists[key] = value;
		}
	}
}
