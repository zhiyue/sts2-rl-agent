using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Saves.Runs;

public class SerializableExtraRunFields : IPacketSerializable
{
	[JsonPropertyName("started_with_neow")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public bool StartedWithNeow { get; set; }

	[JsonPropertyName("test_subject_kills")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public int TestSubjectKills { get; set; }

	[JsonPropertyName("freed_repy")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public bool FreedRepy { get; set; }

	public void Serialize(PacketWriter writer)
	{
		writer.WriteBool(StartedWithNeow);
		writer.WriteInt(TestSubjectKills);
		writer.WriteBool(FreedRepy);
	}

	public void Deserialize(PacketReader reader)
	{
		StartedWithNeow = reader.ReadBool();
		TestSubjectKills = reader.ReadInt();
		FreedRepy = reader.ReadBool();
	}
}
