using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game;

/// <summary>
/// Sent at a treasure room when a remote player opens the treasure chest.
/// </summary>
public class TreasureChestOpenedMessage : INetMessage, IPacketSerializable, IRunLocationTargetedMessage
{
	public bool ShouldBroadcast => true;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.Debug;

	public bool ShouldBuffer => true;

	public RunLocation Location { get; set; }

	public void Serialize(PacketWriter writer)
	{
		writer.Write(Location);
	}

	public void Deserialize(PacketReader reader)
	{
		Location = reader.Read<RunLocation>();
	}
}
