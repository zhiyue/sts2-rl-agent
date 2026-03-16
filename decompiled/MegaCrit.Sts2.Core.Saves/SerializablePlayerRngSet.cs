using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Entities.Rngs;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Saves;

public class SerializablePlayerRngSet : IPacketSerializable
{
	[JsonPropertyName("seed")]
	public uint Seed { get; set; }

	[JsonPropertyName("counters")]
	public Dictionary<PlayerRngType, int> Counters { get; set; } = new Dictionary<PlayerRngType, int>();

	public void Serialize(PacketWriter writer)
	{
		writer.WriteUInt(Seed);
		writer.WriteInt(Counters.Count, 8);
		PlayerRngType[] values = Enum.GetValues<PlayerRngType>();
		foreach (PlayerRngType playerRngType in values)
		{
			if (Counters.TryGetValue(playerRngType, out var value))
			{
				writer.WriteEnum(playerRngType);
				writer.WriteInt(value);
			}
		}
	}

	public void Deserialize(PacketReader reader)
	{
		Seed = reader.ReadUInt();
		int num = reader.ReadInt(8);
		for (int i = 0; i < num; i++)
		{
			PlayerRngType key = reader.ReadEnum<PlayerRngType>();
			int value = reader.ReadInt();
			Counters[key] = value;
		}
	}
}
