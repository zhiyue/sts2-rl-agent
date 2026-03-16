using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.GameActions;

public struct NetDiscardPotionGameAction : INetAction, IPacketSerializable
{
	public uint potionSlotIndex;

	public GameAction ToGameAction(Player player)
	{
		return new DiscardPotionGameAction(player, potionSlotIndex);
	}

	public void Serialize(PacketWriter writer)
	{
		writer.WriteUInt(potionSlotIndex, 4);
	}

	public void Deserialize(PacketReader reader)
	{
		potionSlotIndex = reader.ReadUInt(4);
	}

	public override string ToString()
	{
		return $"{"NetDiscardPotionGameAction"} slot {potionSlotIndex}";
	}
}
