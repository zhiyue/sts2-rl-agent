using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game;

/// <summary>
/// Sent when a player begins removing a card at the merchant.
/// </summary>
public class MerchantCardRemovalMessage : INetMessage, IPacketSerializable, IRunLocationTargetedMessage
{
	public int goldCost;

	public bool ShouldBroadcast => true;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.VeryDebug;

	public bool ShouldBuffer => true;

	public RunLocation Location { get; set; }

	public void Serialize(PacketWriter writer)
	{
		writer.WriteInt(goldCost);
		writer.Write(Location);
	}

	public void Deserialize(PacketReader reader)
	{
		goldCost = reader.ReadInt();
		Location = reader.Read<RunLocation>();
	}
}
