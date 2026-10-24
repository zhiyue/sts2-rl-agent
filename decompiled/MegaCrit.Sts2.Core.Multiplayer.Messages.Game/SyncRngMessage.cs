using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game;

/// <summary>
/// Sent to all clients from host at the start of combat to sync RNG state.
/// TODO: This might not be the right thing to do! If a client has rolled Niche but the host hasn't, and we roll that
///       back, will we end up generating the same RNG again for the same thing?
/// </summary>
public struct SyncRngMessage : INetMessage, IPacketSerializable
{
	public SerializableRunRngSet rng;

	public SerializableRelicGrabBag sharedRelicGrabBag;

	public bool ShouldBroadcast => false;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.VeryDebug;

	public bool ShouldBuffer => true;

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
