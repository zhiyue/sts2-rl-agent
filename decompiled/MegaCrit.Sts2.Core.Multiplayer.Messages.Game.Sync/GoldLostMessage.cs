using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Sync;

public struct GoldLostMessage : INetMessage, IPacketSerializable, IRunLocationTargetedMessage
{
	public required int goldLost;

	public required RunLocation location;

	public bool ShouldBroadcast => true;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.VeryDebug;

	public RunLocation Location => location;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteInt(goldLost);
		writer.Write(location);
	}

	public void Deserialize(PacketReader reader)
	{
		goldLost = reader.ReadInt();
		location = reader.Read<RunLocation>();
	}
}
