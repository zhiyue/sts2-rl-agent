using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game;

/// <summary>
/// Requests that the action currently paused on the sender's action queue be resumed.
/// The action ID sent must be an action that is in the WaitingForPlayerChoice state.
/// The GameAction should not be resumed until the host sends back a ResumeActionAfterPlayerChoiceMessage.
/// </summary>
public struct RequestResumeActionAfterPlayerChoiceMessage : INetMessage, IPacketSerializable, IRunLocationTargetedMessage
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
