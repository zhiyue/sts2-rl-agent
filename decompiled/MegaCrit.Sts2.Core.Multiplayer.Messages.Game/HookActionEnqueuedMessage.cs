using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game;

public struct HookActionEnqueuedMessage : INetMessage, IPacketSerializable, IRunLocationTargetedMessage
{
	public RunLocation location;

	public ulong ownerId;

	public uint hookActionId;

	public GameActionType gameActionType;

	public bool ShouldBroadcast => false;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.Debug;

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
