using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Sync;

public struct VotedForSharedEventOptionMessage : INetMessage, IPacketSerializable, IRunLocationTargetedMessage
{
	public uint optionIndex;

	public uint pageIndex;

	public RunLocation location;

	public bool ShouldBroadcast => true;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.VeryDebug;

	public RunLocation Location => location;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteUInt(optionIndex, 4);
		writer.WriteUInt(pageIndex, 4);
		writer.Write(location);
	}

	public void Deserialize(PacketReader reader)
	{
		optionIndex = reader.ReadUInt(4);
		pageIndex = reader.ReadUInt(4);
		location = reader.Read<RunLocation>();
	}

	public override string ToString()
	{
		return $"{"VotedForSharedEventOptionMessage"} index {optionIndex} page {pageIndex}";
	}
}
