using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game;

/// <summary>
/// Sent from the host to clients to indicate that a GenericHookGameAction has been enqueued.
/// This is slightly different than ActionEnqueuedMessage. The Action itself is not serialized, because it is difficult
/// to serialize every different action that could be executed by a hook. Since hooks are executed by every player,
/// we instead map the hook to an incrementing integer that is the same across all peers, and use that to identify
/// which hook will be assigned to the GameAction that is created.
/// </summary>
public struct HookActionEnqueuedMessage : INetMessage, IPacketSerializable, IRunLocationTargetedMessage
{
	public RunLocation location;

	public ulong ownerId;

	public uint hookActionId;

	public GameActionType gameActionType;

	public bool ShouldBroadcast => false;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.Debug;

	public bool ShouldBuffer => true;

	public RunLocation Location => location;

	public void Serialize(PacketWriter writer)
	{
		writer.Write(location);
		writer.WriteULong(ownerId);
		writer.WriteUInt(hookActionId);
		writer.WriteEnum(gameActionType);
	}

	public void Deserialize(PacketReader reader)
	{
		location = reader.Read<RunLocation>();
		ownerId = reader.ReadULong();
		hookActionId = reader.ReadUInt();
		gameActionType = reader.ReadEnum<GameActionType>();
	}

	public override string ToString()
	{
		return $"HookActionEnqueuedMessage id: {hookActionId} type: {gameActionType}";
	}
}
