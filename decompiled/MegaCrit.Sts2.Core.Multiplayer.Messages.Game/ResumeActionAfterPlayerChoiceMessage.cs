using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game;

/// <summary>
/// Sent from the host to the client. Informs clients that an action has been resumed after it was placed in the
/// WaitingForPlayerChoice state.
/// Note that this message and ActionEnqueuedMessage must be received in the same order that they are sent.
/// </summary>
public struct ResumeActionAfterPlayerChoiceMessage : INetMessage, IPacketSerializable, IRunLocationTargetedMessage
{
	public uint actionId;

	public RunLocation location;

	public bool ShouldBroadcast => false;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public RunLocation Location => location;

	public LogLevel LogLevel => LogLevel.VeryDebug;

	public bool ShouldBuffer => true;

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
