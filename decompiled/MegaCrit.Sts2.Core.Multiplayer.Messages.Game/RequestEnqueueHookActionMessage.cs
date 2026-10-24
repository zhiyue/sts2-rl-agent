using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game;

/// <summary>
/// Sent from the clients to host too request that a hook action be enqueued.
/// It is implicitly assumed that the owner of the hook action to be enqueued is the sender.
/// This is slightly different than RequestEnqueueActionMessage. The Action itself is not serialized, because it is
/// difficult to serialize every different action that could be executed by a hook. Since hooks are executed by every
/// player, we instead map the hook to an incrementing integer that is the same across all peers, and use that to
/// identify which hook will be assigned to the GameAction that is created.
/// </summary>
public struct RequestEnqueueHookActionMessage : INetMessage, IPacketSerializable, IRunLocationTargetedMessage
{
	public RunLocation location;

	public uint hookActionId;

	public GameActionType gameActionType;

	public bool ShouldBroadcast => false;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public RunLocation Location => location;

	public LogLevel LogLevel => LogLevel.VeryDebug;

	public bool ShouldBuffer => true;

	public void Serialize(PacketWriter writer)
	{
		writer.Write(location);
		writer.WriteUInt(hookActionId);
		writer.WriteEnum(gameActionType);
	}

	public void Deserialize(PacketReader reader)
	{
		location = reader.Read<RunLocation>();
		hookActionId = reader.ReadUInt();
		gameActionType = reader.ReadEnum<GameActionType>();
	}
}
