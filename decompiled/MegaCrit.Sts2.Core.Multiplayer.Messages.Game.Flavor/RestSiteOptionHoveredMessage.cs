using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Flavor;

public struct RestSiteOptionHoveredMessage : INetMessage, IPacketSerializable, IRunLocationTargetedMessage
{
	public required uint? optionIndex;

	public bool ShouldBroadcast => true;

	public NetTransferMode Mode => NetTransferMode.Unreliable;

	public LogLevel LogLevel => LogLevel.VeryDebug;

	public RunLocation Location { get; set; }

	public void Serialize(PacketWriter writer)
	{
		writer.WriteBool(optionIndex.HasValue);
		if (optionIndex.HasValue)
		{
			writer.WriteUInt(optionIndex.Value, 5);
		}
		writer.Write(Location);
	}

	public void Deserialize(PacketReader reader)
	{
		if (reader.ReadBool())
		{
			optionIndex = reader.ReadUInt(5);
		}
		Location = reader.Read<RunLocation>();
	}
}
