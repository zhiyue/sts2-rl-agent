using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game;

public class PaelsWingSacrificeMessage : INetMessage, IPacketSerializable, IRunLocationTargetedMessage
{
	public uint relicIndex;

	public bool ShouldBroadcast => true;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.Debug;

	public RunLocation Location { get; set; }

	public void Serialize(PacketWriter writer)
	{
		writer.WriteUInt(relicIndex, 8);
		writer.Write(Location);
	}

	public void Deserialize(PacketReader reader)
	{
		relicIndex = reader.ReadUInt(8);
		Location = reader.Read<RunLocation>();
	}
}
