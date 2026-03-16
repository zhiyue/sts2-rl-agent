using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Multiplayer.Replay;

public class CombatReplay : IPacketSerializable
{
	public string version;

	public string gitCommit;

	public uint modelIdHash;

	public List<uint> choiceIds = new List<uint>();

	public uint nextActionId;

	public uint nextChecksumId;

	public uint nextHookId;

	public SerializableRun serializableRun;

	public List<CombatReplayEvent> events = new List<CombatReplayEvent>();

	public List<ReplayChecksumData> checksumData = new List<ReplayChecksumData>();

	public void Serialize(PacketWriter writer)
	{
		writer.WriteString(version);
		writer.WriteString(gitCommit);
		writer.WriteUInt(modelIdHash);
		writer.WriteInt(choiceIds.Count);
		foreach (uint choiceId in choiceIds)
		{
			writer.WriteUInt(choiceId);
		}
		writer.WriteUInt(nextActionId);
		writer.WriteUInt(nextChecksumId);
		writer.WriteUInt(nextHookId);
		writer.Write(serializableRun);
		writer.WriteList(events);
		writer.WriteList(checksumData);
	}

	public void Deserialize(PacketReader reader)
	{
		version = reader.ReadString();
		gitCommit = reader.ReadString();
		modelIdHash = reader.ReadUInt();
		int num = reader.ReadInt();
		for (int i = 0; i < num; i++)
		{
			choiceIds.Add(reader.ReadUInt());
		}
		nextActionId = reader.ReadUInt();
		nextChecksumId = reader.ReadUInt();
		nextHookId = reader.ReadUInt();
		serializableRun = reader.Read<SerializableRun>();
		events = reader.ReadList<CombatReplayEvent>();
		checksumData = reader.ReadList<ReplayChecksumData>();
	}

	public CombatReplay Anonymized()
	{
		return new CombatReplay
		{
			version = version,
			gitCommit = gitCommit,
			modelIdHash = modelIdHash,
			choiceIds = choiceIds,
			nextActionId = nextActionId,
			nextChecksumId = nextChecksumId,
			nextHookId = nextHookId,
			serializableRun = serializableRun.Anonymized(),
			events = events.Select((CombatReplayEvent e) => e.Anonymized()).ToList(),
			checksumData = checksumData.Select((ReplayChecksumData c) => c.Anonymized()).ToList()
		};
	}
}
