using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game;

public struct ResumeActionAfterPlayerChoiceMessage : INetMessage, IPacketSerializable, IRunLocationTargetedMessage
{
	public uint actionId;

	public RunLocation location;

	public bool ShouldBroadcast => false;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public RunLocation Location => location;

	public LogLevel LogLevel => LogLevel.VeryDebug;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteUInt(actionId);
		writer.Write(location);
	}

	public void Deserialize(PacketReader reader)
	{
		actionId = reader.ReadUInt();
		location = reader.Read<RunLocation>();
	}
}
