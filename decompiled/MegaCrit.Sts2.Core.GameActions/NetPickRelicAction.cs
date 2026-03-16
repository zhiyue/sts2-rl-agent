using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.GameActions;

public struct NetPickRelicAction : INetAction, IPacketSerializable
{
	public int relicIndex;

	public GameAction ToGameAction(Player player)
	{
		return new PickRelicAction(player, relicIndex);
	}

	public void Serialize(PacketWriter writer)
	{
		writer.WriteInt(relicIndex, 8);
	}

	public void Deserialize(PacketReader reader)
	{
		relicIndex = reader.ReadInt(8);
	}

	public override string ToString()
	{
		return $"{"NetPickRelicAction"} index: {relicIndex}";
	}
}
