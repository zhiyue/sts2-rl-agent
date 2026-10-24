using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Sync;

/// <summary>
/// Used both in events and rest sites to synchronize the option chosen by a player.
/// In events, optionIndex is the index of the currently displayed EventOption to choose.
/// In rest sites, optionIndex is the index of the currently displayed RestSiteOption to choose.
/// </summary>
public struct OptionIndexChosenMessage : INetMessage, IPacketSerializable, IRunLocationTargetedMessage
{
	public OptionIndexType type;

	public uint optionIndex;

	public RunLocation location;

	public bool ShouldBroadcast => true;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.VeryDebug;

	public bool ShouldBuffer => true;

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
