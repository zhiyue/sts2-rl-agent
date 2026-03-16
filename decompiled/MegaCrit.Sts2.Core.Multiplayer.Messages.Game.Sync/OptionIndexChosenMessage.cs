using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Sync;

public struct OptionIndexChosenMessage : INetMessage, IPacketSerializable, IRunLocationTargetedMessage
{
	public OptionIndexType type;

	public uint optionIndex;

	public RunLocation location;

	public bool ShouldBroadcast => true;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.VeryDebug;

	public RunLocation Location => location;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteEnum(type);
		writer.WriteUInt(optionIndex, 4);
		writer.Write(location);
	}

	public void Deserialize(PacketReader reader)
	{
		type = reader.ReadEnum<OptionIndexType>();
		optionIndex = reader.ReadUInt(4);
		location = reader.Read<RunLocation>();
	}

	public override string ToString()
	{
		return $"{"OptionIndexChosenMessage"} type {type} index {optionIndex}";
	}
}
