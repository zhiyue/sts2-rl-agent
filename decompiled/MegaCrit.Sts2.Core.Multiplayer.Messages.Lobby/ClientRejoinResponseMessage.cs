using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Lobby;

public struct ClientRejoinResponseMessage : INetMessage, IPacketSerializable
{
	public SerializableRun serializableRun;

	public NetFullCombatState? combatState;

	public bool ShouldBroadcast => false;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.Info;

	public void Serialize(PacketWriter writer)
	{
		writer.Write(serializableRun);
		writer.WriteBool(combatState != null);
		if (combatState != null)
		{
			writer.Write(combatState);
		}
	}

	public void Deserialize(PacketReader reader)
	{
		serializableRun = reader.Read<SerializableRun>();
		if (reader.ReadBool())
		{
			combatState = reader.Read<NetFullCombatState>();
		}
	}
}
