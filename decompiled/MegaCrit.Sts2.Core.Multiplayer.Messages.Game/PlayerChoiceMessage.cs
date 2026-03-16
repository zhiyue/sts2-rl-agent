using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game;

public struct PlayerChoiceMessage : INetMessage, IPacketSerializable
{
	public uint choiceId;

	public NetPlayerChoiceResult result;

	public bool ShouldBroadcast => true;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.VeryDebug;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteUInt(choiceId);
		writer.Write(result);
	}

	public void Deserialize(PacketReader reader)
	{
		choiceId = reader.ReadUInt();
		result = reader.Read<NetPlayerChoiceResult>();
	}
}
