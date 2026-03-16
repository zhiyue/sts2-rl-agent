using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Entities.Rngs;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Saves.Runs;

public class SerializableRunRngSet : IPacketSerializable
{
	[JsonPropertyName("seed")]
	public string? Seed { get; set; }

	[JsonPropertyName("counters")]
	public Dictionary<RunRngType, int> Counters { get; set; } = new Dictionary<RunRngType, int>();

	public void Serialize(PacketWriter writer)
	{
		writer.WriteString(Seed);
		writer.WriteInt(Counters.Count, 8);
		RunRngType[] values = Enum.GetValues<RunRngType>();
		foreach (RunRngType runRngType in values)
		{
			if (Counters.TryGetValue(runRngType, out var value))
			{
				writer.WriteEnum(runRngType);
				writer.WriteInt(value);
			}
		}
	}

	public void Deserialize(PacketReader reader)
	{
		Seed = reader.ReadString();
		int num = reader.ReadInt(8);
		for (int i = 0; i < num; i++)
		{
			RunRngType key = reader.ReadEnum<RunRngType>();
			int value = reader.ReadInt();
			Counters[key] = value;
		}
	}
}
