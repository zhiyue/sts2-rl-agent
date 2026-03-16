using System.Collections.Generic;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Unlocks;

public class SerializableUnlockState : IPacketSerializable
{
	[JsonPropertyName("unlocked_epochs")]
	public List<string> UnlockedEpochs { get; set; } = new List<string>();

	[JsonPropertyName("encounters_seen")]
	public List<ModelId> EncountersSeen { get; set; } = new List<ModelId>();

	[JsonPropertyName("number_of_runs")]
	public int NumberOfRuns { get; set; }

	public void Serialize(PacketWriter writer)
	{
		writer.WriteInt(UnlockedEpochs.Count);
		foreach (string unlockedEpoch in UnlockedEpochs)
		{
			writer.WriteEpochId(unlockedEpoch);
		}
		writer.WriteModelEntriesInList(EncountersSeen);
		writer.WriteInt(NumberOfRuns, 16);
	}

	public void Deserialize(PacketReader reader)
	{
		int num = reader.ReadInt();
		for (int i = 0; i < num; i++)
		{
			UnlockedEpochs.Add(reader.ReadEpochId());
		}
		EncountersSeen = reader.ReadModelIdListAssumingType<EncounterModel>();
		NumberOfRuns = reader.ReadInt(16);
	}
}
