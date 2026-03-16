using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game;

public struct RequestEnqueueHookActionMessage : INetMessage, IPacketSerializable, IRunLocationTargetedMessage
{
	public RunLocation location;

	public uint hookActionId;

	public GameActionType gameActionType;

	public bool ShouldBroadcast => false;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public RunLocation Location => location;

	public LogLevel LogLevel => LogLevel.VeryDebug;

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
