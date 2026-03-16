using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Multiplayer.Replay;

public struct ReplayChecksumData : IPacketSerializable
{
	public NetChecksumData checksumData;

	public string context;

	public NetFullCombatState fullState;

	public void Serialize(PacketWriter writer)
	{
		writer.Write(checksumData);
		writer.WriteString(context);
		writer.Write(fullState);
	}

	public void Deserialize(PacketReader reader)
	{
		checksumData = reader.Read<NetChecksumData>();
		context = reader.ReadString();
		fullState = reader.Read<NetFullCombatState>();
	}

	public ReplayChecksumData Anonymized()
	{
		ReplayChecksumData result = this;
		result.fullState = fullState.Anonymized();
		return result;
	}
}
