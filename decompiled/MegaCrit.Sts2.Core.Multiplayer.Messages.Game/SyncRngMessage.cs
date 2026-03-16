using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game;

public struct SyncRngMessage : INetMessage, IPacketSerializable
{
	public SerializableRunRngSet rng;

	public SerializableRelicGrabBag sharedRelicGrabBag;

	public bool ShouldBroadcast => false;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.VeryDebug;

	public void Serialize(PacketWriter writer)
	{
		writer.Write(rng);
		writer.Write(sharedRelicGrabBag);
	}

	public void Deserialize(PacketReader reader)
	{
		rng = reader.Read<SerializableRunRngSet>();
		sharedRelicGrabBag = reader.Read<SerializableRelicGrabBag>();
	}
}
