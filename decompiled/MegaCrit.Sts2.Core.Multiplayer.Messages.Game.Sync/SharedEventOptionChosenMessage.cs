using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Sync;

/// <summary>
/// Sent by the host to all clients when votes are received by all players during a shared event. This message specifies
/// the option that all players should execute.
/// </summary>
public struct SharedEventOptionChosenMessage : INetMessage, IPacketSerializable, IRunLocationTargetedMessage
{
	public uint optionIndex;

	public uint pageIndex;

	public RunLocation location;

	public bool ShouldBroadcast => false;

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
		return $"{"SharedEventOptionChosenMessage"} index {optionIndex} page {pageIndex}";
	}
}
