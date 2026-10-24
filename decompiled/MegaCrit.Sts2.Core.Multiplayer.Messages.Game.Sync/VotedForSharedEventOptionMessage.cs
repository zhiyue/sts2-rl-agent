using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Sync;

/// <summary>
/// Broadcast from peers to all other peers when they vote on an event option during a shared event.
/// This only records the votes; the host is the one that ultimately chooses the event option and sends it via
/// SharedEventOptionChosenMessage.
/// </summary>
public struct VotedForSharedEventOptionMessage : INetMessage, IPacketSerializable, IRunLocationTargetedMessage
{
	public uint optionIndex;

	public uint pageIndex;

	public RunLocation location;

	public bool ShouldBroadcast => true;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.VeryDebug;

	public bool ShouldBuffer => true;

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
